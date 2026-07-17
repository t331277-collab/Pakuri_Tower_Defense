using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Run;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SceneEntryManager))]
    [RequireComponent(typeof(InGameCombatManager))]
    public sealed class StageManager : MonoBehaviour
    {
        private const float DefaultClearCheckInterval = 0.25f;

        private readonly List<StageEncounterRow> activeEncounterRows = new List<StageEncounterRow>();
        private readonly List<string> pendingPrisonerEnemyIds = new List<string>();
        private readonly List<string> prisonerCandidatePool = new List<string>();

        [SerializeField] private SceneEntryManager entryManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private TextAsset stageDayCsv;
        [SerializeField] private TextAsset stageEncounterCsv;
        [SerializeField] private TextAsset stageRewardCsv;
        [SerializeField] private bool startFlowOnStart = true;
        [SerializeField] private float clearCheckInterval = DefaultClearCheckInterval;
        [SerializeField] private bool restorePlayerHealthOnDayAdvance = true;
        [SerializeField] private NexusUnitActor nexusActor;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private Button winButton;
        [SerializeField] private Button defeatButton;
        [SerializeField] private string mainMenuScenePath = "Assets/Scenes/NewScene/NewMainMenu.unity";
        [SerializeField] private int winStageIndex = 2;
        [SerializeField] private int winDayIndex = 11;

        private StageFlowTable table = new StageFlowTable();
        private Coroutine flowCoroutine;
        private StageDayRow currentDay;
        private StageRewardRow currentReward;
        private RunSession activeSession;
        private bool endButtonsBound;
        private bool hasPreservedNexusHealth;
        private float preservedNexusHealth;

        public StageState State { get; private set; } = StageState.NotStarted;
        public int CurrentStage => activeSession != null ? activeSession.StageIndex : 1;
        public int CurrentDay => activeSession != null ? activeSession.DayIndex : 1;
        public IReadOnlyList<string> PendingPrisonerEnemyIds => pendingPrisonerEnemyIds;
        public int PendingGoldReward { get; private set; }
        public int PendingDarkTraceReward { get; private set; }
        public int PendingPrisonerCount { get; private set; }
        public float PendingManifestSuccessChance => currentReward != null ? currentReward.ManifestSuccessChance : 0.7f;
        public RunSession ActiveSession => activeSession;
        public string CurrentEncounterId => currentDay != null ? currentDay.EncounterId : string.Empty;
        public string CurrentRewardRuleId => currentReward != null ? currentReward.RewardRuleId : string.Empty;

        private void Awake()
        {
            ResolveReferences();
            ResolveEndFlowReferences();
            HideEndPanels();
            BindEndButtons();
        }

        private void Start()
        {
            ResolveReferences();
            ResolveEndFlowReferences();
            HideEndPanels();
            BindEndButtons();
            LoadTables();

            if (startFlowOnStart)
            {
                StartCurrentDay();
            }
        }

        private void OnDestroy()
        {
            if (nexusActor != null)
            {
                nexusActor.Defeated -= OnNexusDefeated;
            }
        }

        public void StartCurrentDay()
        {
            if (flowCoroutine != null)
            {
                StopCoroutine(flowCoroutine);
            }

            if (DamageMeterRuntimeTracker.Active != null)
            {
                DamageMeterRuntimeTracker.Active.ResetMeter();
            }

            flowCoroutine = StartCoroutine(RunCurrentDayFlow());
        }

        public void ContinueToNextDay()
        {
            if (State != StageState.RewardReady)
            {
                Debug.LogWarning("StageManager cannot continue because reward state is not ready.");
                return;
            }

            if (activeSession == null)
            {
                Debug.LogWarning("StageManager cannot continue because no active run session exists.");
                return;
            }

            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = 0;
            PendingDarkTraceReward = 0;
            PendingPrisonerCount = 0;
            PreserveCurrentNexusHealth();
            if (combatManager != null)
            {
                combatManager.ResetTransientCombatStateForNextDay();
            }

            activeSession.AdvanceDay();
            RestorePlayerHealthForNextDay();
            StartCurrentDay();
        }

        private void RestorePlayerHealthForNextDay()
        {
            if (!restorePlayerHealthOnDayAdvance)
            {
                return;
            }

            if (entryManager != null)
            {
                entryManager.RestorePlayerPartyFromSession();
            }

            if (combatManager == null)
            {
                return;
            }

            var players = combatManager.Roster != null ? combatManager.Roster.Players : null;
            if (players == null)
            {
                return;
            }

            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                var model = entry != null ? entry.Model : null;
                var identity = model != null ? model.Identity : null;
                if (identity == null || identity.Role != UnitRole.Monster)
                {
                    continue;
                }

                var resources = model != null ? model.Resources : null;
                var stats = model != null ? model.Stats : null;
                if (resources == null || stats == null)
                {
                    continue;
                }

                resources.CurrentHealth = Mathf.Max(0f, stats.MaxHealth);
                combatManager.RefreshUnitActor(model);
            }
        }

        private IEnumerator RunCurrentDayFlow()
        {
            ResolveReferences();
            if (entryManager != null)
            {
                entryManager.SpawnSelectedPlayerUnit();
            }

            EnsureNexusRegistered();

            activeSession = entryManager != null ? entryManager.ActiveSession : null;
            if (activeSession == null)
            {
                State = StageState.Error;
                Debug.LogError("StageManager could not start because SceneEntryManager has no active session.");
                yield break;
            }

            currentDay = table.FindDay(activeSession.StageIndex, activeSession.DayIndex);
            if (currentDay == null)
            {
                State = StageState.Error;
                Debug.LogError($"StageManager has no StageDay row for stage {activeSession.StageIndex}, day {activeSession.DayIndex}.");
                yield break;
            }

            currentReward = table.FindReward(currentDay.RewardRuleId);
            if (currentReward == null)
            {
                State = StageState.Error;
                Debug.LogError($"StageManager has no StageReward row for '{currentDay.RewardRuleId}'.");
                yield break;
            }

            table.FindEncounterRows(currentDay.EncounterId, activeEncounterRows);
            if (activeEncounterRows.Count == 0)
            {
                State = StageState.Error;
                Debug.LogError($"StageManager has no StageEncounter rows for '{currentDay.EncounterId}'.");
                yield break;
            }

            SelectBossRows();
            State = StageState.Spawning;
            yield return SpawnEncounterRows(activeEncounterRows);

            State = StageState.Combat;
            yield return WaitForEnemyClear();

            if (IsConfiguredWinDay())
            {
                ShowWinPanel();
                State = StageState.Victory;
                flowCoroutine = null;
                yield break;
            }

            PrepareReward();
            State = StageState.RewardReady;
            flowCoroutine = null;
        }

        private IEnumerator SpawnEncounterRows(IReadOnlyList<StageEncounterRow> rows)
        {
            var spawnIndex = 0;
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var count = Mathf.Max(0, row.Count);
                for (var j = 0; j < count; j++)
                {
                    if (entryManager != null)
                    {
                        var isBoss = IsBossEncounter(row);
                        var healthMultiplier = isBoss
                            ? UnityEngine.Random.Range(row.BossHealthMultiplierMin, row.BossHealthMultiplierMax)
                            : 1f;
                        entryManager.SpawnEnemyById(
                            row.EnemyId,
                            spawnIndex,
                            row.SpawnX,
                            row.SpawnYMin,
                            row.SpawnYMax,
                            healthMultiplier,
                            isBoss,
                            out _);
                    }

                    spawnIndex += 1;
                    yield return new WaitForSeconds(Mathf.Max(0f, row.IntervalSeconds));
                }
            }
        }

        private IEnumerator WaitForEnemyClear()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.05f, clearCheckInterval));
            while (combatManager != null && combatManager.ActiveEnemyCount > 0 && State != StageState.Defeat)
            {
                yield return wait;
            }
        }

        private void PrepareReward()
        {
            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = currentReward != null ? currentReward.Gold : 0;
            PendingDarkTraceReward = currentReward != null ? currentReward.DarkTrace : 0;
            PendingPrisonerCount = currentReward != null ? currentReward.RollPrisonerCount() : 0;

            AddGuaranteedPrisoners();
            BuildPrisonerCandidatePool();
            AddCandidatePrisonersUntilFull();

            var prisonerSummary = string.Join("|", pendingPrisonerEnemyIds);
            Debug.Log(
                $"Stage reward ready: stage={activeSession.StageIndex}, day={activeSession.DayIndex}, " +
                $"gold={PendingGoldReward}, darkTrace={PendingDarkTraceReward}, prisoners={prisonerSummary}");
        }

        private void AddGuaranteedPrisoners()
        {
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                if (row.GuaranteedPrisoner || row.SelectedAsBoss)
                {
                    AddPrisoner(row.EnemyId);
                }
            }
        }

        private void BuildPrisonerCandidatePool()
        {
            prisonerCandidatePool.Clear();
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                for (var count = 0; count < row.Count; count++)
                {
                    prisonerCandidatePool.Add(row.EnemyId);
                }
            }

            for (var i = 0; i < pendingPrisonerEnemyIds.Count; i++)
            {
                RemoveOnePrisonerCandidate(pendingPrisonerEnemyIds[i]);
            }
        }

        private void AddCandidatePrisonersUntilFull()
        {
            if (PendingPrisonerCount <= 0)
            {
                return;
            }

            while (pendingPrisonerEnemyIds.Count < PendingPrisonerCount && prisonerCandidatePool.Count > 0)
            {
                var poolIndex = UnityEngine.Random.Range(0, prisonerCandidatePool.Count);
                AddPrisoner(prisonerCandidatePool[poolIndex]);
                prisonerCandidatePool.RemoveAt(poolIndex);
            }
        }

        private void AddPrisoner(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            pendingPrisonerEnemyIds.Add(enemyId);
        }

        private void RemoveOnePrisonerCandidate(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            for (var i = 0; i < prisonerCandidatePool.Count; i++)
            {
                if (string.Equals(prisonerCandidatePool[i], enemyId, StringComparison.OrdinalIgnoreCase))
                {
                    prisonerCandidatePool.RemoveAt(i);
                    return;
                }
            }
        }

        private void SelectBossRows()
        {
            var normalBossCandidates = new List<StageEncounterRow>();
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                row.SelectedAsBoss = false;
                if (row.IsGuaranteedBoss)
                {
                    row.SelectedAsBoss = true;
                    continue;
                }

                if (row.IsBossCandidate)
                {
                    normalBossCandidates.Add(row);
                }
            }

            if (normalBossCandidates.Count == 0)
            {
                return;
            }

            normalBossCandidates[UnityEngine.Random.Range(0, normalBossCandidates.Count)].SelectedAsBoss = true;
        }

        private bool IsBossEncounter(StageEncounterRow row)
        {
            if (row == null)
            {
                return false;
            }

            if (row.SelectedAsBoss)
            {
                return true;
            }

            if (activeSession == null)
            {
                return false;
            }

            var isMidbossCombat = activeSession.CurrentCombatType == RunCombatType.Day5Midboss
                || activeSession.CurrentCombatType == RunCombatType.Day10Midboss;
            return isMidbossCombat && (row.IsGuaranteedBoss || row.IsBossCandidate);
        }

        private void ResolveReferences()
        {
            if (entryManager == null)
            {
                entryManager = GetComponent<SceneEntryManager>();
            }

            if (combatManager == null)
            {
                combatManager = GetComponent<InGameCombatManager>();
            }
        }

        private void ResolveEndFlowReferences()
        {
            if (nexusActor == null)
            {
                var nexusObject = FindSceneGameObjectByPath("Nexus");
                nexusActor = nexusObject != null ? nexusObject.GetComponent<NexusUnitActor>() : null;
            }

            if (winPanel == null)
            {
                winPanel = FindSceneGameObjectByPath("Canvas/WinPanel");
            }

            if (defeatPanel == null)
            {
                defeatPanel = FindSceneGameObjectByPath("Canvas/DefeatPanel");
            }

            if (winButton == null && winPanel != null)
            {
                winButton = winPanel.GetComponentInChildren<Button>(true);
            }

            if (defeatButton == null && defeatPanel != null)
            {
                defeatButton = defeatPanel.GetComponentInChildren<Button>(true);
            }
        }

        private void EnsureNexusRegistered()
        {
            ResolveEndFlowReferences();
            if (nexusActor == null)
            {
                var nexusObject = FindSceneGameObjectByPath("Nexus");
                if (nexusObject == null)
                {
                    Debug.LogWarning("StageManager could not find Nexus in NewRunScene.");
                    return;
                }

                nexusActor = nexusObject.AddComponent<NexusUnitActor>();
            }

            nexusActor.Defeated -= OnNexusDefeated;
            nexusActor.Defeated += OnNexusDefeated;
            nexusActor.Initialize();
            RestorePreservedNexusHealth();

            if (combatManager != null && nexusActor.Model != null)
            {
                combatManager.RegisterNexus(nexusActor.Model, nexusActor, nexusActor.transform);
            }
        }

        private void PreserveCurrentNexusHealth()
        {
            ResolveEndFlowReferences();
            if (nexusActor == null || !nexusActor.TryGetCurrentHealth(out var currentHealth))
            {
                return;
            }

            preservedNexusHealth = currentHealth;
            hasPreservedNexusHealth = true;
        }

        private void RestorePreservedNexusHealth()
        {
            if (!hasPreservedNexusHealth || nexusActor == null)
            {
                return;
            }

            nexusActor.SetCurrentHealth(preservedNexusHealth);
        }

        private void OnNexusDefeated(NexusUnitActor defeatedNexus)
        {
            if (State == StageState.Victory)
            {
                return;
            }

            if (flowCoroutine != null)
            {
                StopCoroutine(flowCoroutine);
                flowCoroutine = null;
            }

            State = StageState.Defeat;
            ShowDefeatPanel();
        }

        private void HideEndPanels()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, false);
        }

        private void ShowWinPanel()
        {
            SetActive(defeatPanel, false);
            SetActive(winPanel, true);
        }

        private void ShowDefeatPanel()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, true);
        }

        private void BindEndButtons()
        {
            if (endButtonsBound)
            {
                return;
            }

            if (winButton != null)
            {
                winButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (defeatButton != null)
            {
                defeatButton.onClick.AddListener(ReturnToMainMenu);
            }

            endButtonsBound = true;
        }

        private void ReturnToMainMenu()
        {
            if (string.IsNullOrWhiteSpace(mainMenuScenePath))
            {
                Debug.LogError("StageManager cannot return to main menu because mainMenuScenePath is empty.");
                return;
            }

            SceneManager.LoadScene(mainMenuScenePath);
        }

        private bool IsConfiguredWinDay()
        {
            return activeSession != null
                && activeSession.StageIndex == winStageIndex
                && activeSession.DayIndex == winDayIndex;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static GameObject FindSceneGameObjectByPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var objects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (var i = 0; i < objects.Length; i++)
            {
                var candidate = objects[i];
                if (candidate == null || !candidate.scene.IsValid())
                {
                    continue;
                }

                if (string.Equals(BuildPath(candidate.transform), path, StringComparison.Ordinal))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static string BuildPath(Transform transform)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            var path = transform.name;
            var parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }

            return path;
        }

        private void LoadTables()
        {
            table = StageFlowTable.Load(stageDayCsv, stageEncounterCsv, stageRewardCsv);
        }
    }

    public enum StageState
    {
        NotStarted,
        Spawning,
        Combat,
        RewardReady,
        Victory,
        Defeat,
        Error
    }

    internal sealed class StageDayRow
    {
        public int Stage;
        public int Day;
        public string CombatType;
        public string EncounterId;
        public string RewardRuleId;
    }

    internal sealed class StageEncounterRow
    {
        public string EncounterId;
        public int SpawnOrder;
        public string EnemyId;
        public int Count;
        public float IntervalSeconds;
        public float SpawnX;
        public float SpawnYMin;
        public float SpawnYMax;
        public bool IsBossCandidate;
        public bool IsGuaranteedBoss;
        public float BossHealthMultiplierMin;
        public float BossHealthMultiplierMax;
        public bool GuaranteedPrisoner;
        public bool SelectedAsBoss;
    }

    internal sealed class StageRewardRow
    {
        public string RewardRuleId;
        public int Gold;
        public int DarkTrace;
        public float PrisonerCount1Chance;
        public float PrisonerCount2Chance;
        public float PrisonerCount3Chance;
        public float ManifestSuccessChance;
        public int EliteBonusPrisoners;

        public int RollPrisonerCount()
        {
            var roll = UnityEngine.Random.value;
            if (roll < PrisonerCount1Chance)
            {
                return 1 + EliteBonusPrisoners;
            }

            if (roll < PrisonerCount1Chance + PrisonerCount2Chance)
            {
                return 2 + EliteBonusPrisoners;
            }

            return 3 + EliteBonusPrisoners;
        }
    }

    internal sealed class StageFlowTable
    {
        private readonly List<StageDayRow> days = new List<StageDayRow>();
        private readonly List<StageEncounterRow> encounters = new List<StageEncounterRow>();
        private readonly List<StageRewardRow> rewards = new List<StageRewardRow>();

        public static StageFlowTable Load(TextAsset dayCsv, TextAsset encounterCsv, TextAsset rewardCsv)
        {
            var table = new StageFlowTable();
            table.LoadDays(dayCsv);
            table.LoadEncounters(encounterCsv);
            table.LoadRewards(rewardCsv);
            return table;
        }

        public StageDayRow FindDay(int stage, int day)
        {
            for (var i = 0; i < days.Count; i++)
            {
                var row = days[i];
                if (row.Stage == stage && row.Day == day)
                {
                    return row;
                }
            }

            return null;
        }

        public StageRewardRow FindReward(string rewardRuleId)
        {
            for (var i = 0; i < rewards.Count; i++)
            {
                var row = rewards[i];
                if (string.Equals(row.RewardRuleId, rewardRuleId, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        public void FindEncounterRows(string encounterId, List<StageEncounterRow> results)
        {
            results.Clear();
            for (var i = 0; i < encounters.Count; i++)
            {
                var row = encounters[i];
                if (string.Equals(row.EncounterId, encounterId, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(row);
                }
            }

            results.Sort((left, right) => left.SpawnOrder.CompareTo(right.SpawnOrder));
        }

        private void LoadDays(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {
                days.Add(new StageDayRow
                {
                    Stage = ParseInt(row, "stage"),
                    Day = ParseInt(row, "day"),
                    CombatType = Read(row, "combat_type"),
                    EncounterId = Read(row, "encounter_id"),
                    RewardRuleId = Read(row, "reward_rule_id")
                });
            }
        }

        private void LoadEncounters(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {
                encounters.Add(new StageEncounterRow
                {
                    EncounterId = Read(row, "encounter_id"),
                    SpawnOrder = ParseInt(row, "spawn_order"),
                    EnemyId = Read(row, "enemy_id"),
                    Count = ParseInt(row, "count"),
                    IntervalSeconds = ParseFloat(row, "interval_sec"),
                    SpawnX = ParseFloat(row, "spawn_x"),
                    SpawnYMin = ParseFloat(row, "spawn_y_min"),
                    SpawnYMax = ParseFloat(row, "spawn_y_max"),
                    IsBossCandidate = ParseBool(row, "is_boss_candidate"),
                    IsGuaranteedBoss = ParseBool(row, "is_guaranteed_boss"),
                    BossHealthMultiplierMin = ParseFloat(row, "boss_health_multiplier_min", 1f),
                    BossHealthMultiplierMax = ParseFloat(row, "boss_health_multiplier_max", 1f),
                    GuaranteedPrisoner = ParseBool(row, "guaranteed_prisoner")
                });
            }
        }

        private void LoadRewards(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {
                rewards.Add(new StageRewardRow
                {
                    RewardRuleId = Read(row, "reward_rule_id"),
                    Gold = ParseInt(row, "gold"),
                    DarkTrace = ParseInt(row, "dark_trace"),
                    PrisonerCount1Chance = ParseFloat(row, "prisoner_count_1_chance"),
                    PrisonerCount2Chance = ParseFloat(row, "prisoner_count_2_chance"),
                    PrisonerCount3Chance = ParseFloat(row, "prisoner_count_3_chance"),
                    ManifestSuccessChance = ParseFloat(row, "manifest_success_chance", 0.7f),
                    EliteBonusPrisoners = ParseInt(row, "elite_bonus_prisoners")
                });
            }
        }

        private static IEnumerable<Dictionary<string, string>> ReadRows(TextAsset csv)
        {
            if (csv == null || string.IsNullOrWhiteSpace(csv.text))
            {
                yield break;
            }

            var lines = csv.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            if (lines.Length == 0)
            {
                yield break;
            }

            var headers = SplitCsvLine(lines[0]);
            for (var i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    continue;
                }

                var values = SplitCsvLine(lines[i]);
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                for (var j = 0; j < headers.Count; j++)
                {
                    row[headers[j]] = j < values.Count ? values[j] : string.Empty;
                }

                yield return row;
            }
        }

        private static List<string> SplitCsvLine(string line)
        {
            var values = new List<string>();
            var current = string.Empty;
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current += '"';
                        i += 1;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current);
                    current = string.Empty;
                }
                else
                {
                    current += c;
                }
            }

            values.Add(current);
            return values;
        }

        private static string Read(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
        }

        private static int ParseInt(Dictionary<string, string> row, string key)
        {
            return int.TryParse(Read(row, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : 0;
        }

        private static float ParseFloat(Dictionary<string, string> row, string key, float fallback = 0f)
        {
            return float.TryParse(Read(row, key), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        private static bool ParseBool(Dictionary<string, string> row, string key)
        {
            return bool.TryParse(Read(row, key), out var value) && value;
        }
    }
}
