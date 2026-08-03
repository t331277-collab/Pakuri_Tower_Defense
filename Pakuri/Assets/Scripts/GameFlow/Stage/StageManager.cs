/*
 * 역할: Stage 및 Wave 진행.
 * 책임: Stage 시작·적 Wave 예약·전투 종료 감지·보상 지급·Run 진행을 처리한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    public class StageManager : MonoBehaviour
    {
        private const float DefaultClearCheckInterval = 0.25f;

        private readonly List<StageEncounterRow> activeEncounterRows = new List<StageEncounterRow>();
        private readonly List<string> pendingPrisonerEnemyIds = new List<string>();
        private readonly List<string> prisonerCandidatePool = new List<string>();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private TextAsset stageDayCsv;
        [SerializeField] private TextAsset stage1EncounterCsv;
        [SerializeField] private TextAsset stage1RewardCsv;
        [SerializeField] private TextAsset stage2EncounterCsv;
        [SerializeField] private TextAsset stage2RewardCsv;
        [SerializeField] private bool startFlowOnStart = true;
        [SerializeField] private float clearCheckInterval = DefaultClearCheckInterval;
        [SerializeField] private bool restorePlayerHealthOnDayAdvance = true;
        [SerializeField] private NexusActor nexusActor;
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

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            ResolveEndFlowReferences();
            HideEndPanels();
            BindEndButtons();
            combatManager.UnitDefeated += OnUnitDefeated;
        }

        /// 컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.
        private void Start()
        {
            ResolveEndFlowReferences();
            HideEndPanels();
            BindEndButtons();
            LoadTables();

            if (startFlowOnStart)
            {
                StartCurrentDay();
            }
        }

        /// Unity가 컴포넌트를 제거할 때 구독과 런타임 오브젝트를 해제한다.
        private void OnDestroy()
        {
            combatManager.UnitDefeated -= OnUnitDefeated;
        }

        /// 현재 Stage와 Day의 전투 흐름을 시작한다.
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

        /// 보상을 정리하고 RunSession을 다음 Day로 넘긴 뒤 파티를 복구한다.
        public void ContinueToNextDay()
        {
            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = 0;
            PendingDarkTraceReward = 0;
            PendingPrisonerCount = 0;
            PreserveCurrentNexusHealth();
            combatManager.ResetCombatState();

            AdvanceDay();
            RestorePlayerHealthForNextDay();
            StartCurrentDay();
        }

        private void AdvanceDay()
        {
            activeSession.DayIndex += 1;
            if (activeSession.DayIndex <= 11)
            {
                return;
            }

            activeSession.DayIndex = 1;
            activeSession.StageIndex = Math.Min(activeSession.StageIndex + 1, 4);
        }

        /// 다음 Day 시작 전에 복구된 플레이어 몬스터의 체력을 최대치로 채운다.
        private void RestorePlayerHealthForNextDay()
        {
            if (!restorePlayerHealthOnDayAdvance)
            {
                return;
            }

            unitSpawnManager.RestorePlayerPartyFromSession(activeSession);
            var players = unitSpawnManager.Players;

            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                var model = entry.Model;
                var identity = model.Identity;
                if (identity.Role != UnitRole.Monster)
                {
                    continue;
                }

                var resources = model.Resources;
                var stats = model.Stats;

                resources.CurrentHealth = Mathf.Max(0f, stats.MaxHealth);
                unitSpawnManager.RefreshDisplay(model);
            }
        }

        /// 현재 Day의 세션을 준비하고 적 생성부터 보상 대기까지 순서대로 진행한다.
        private IEnumerator RunCurrentDayFlow()
        {
            if (activeSession == null)
            {
                BeginRunSession();
            }

            unitSpawnManager.SpawnSelectedPlayerUnit(activeSession);

            EnsureNexusRegistered();

            currentDay = table.FindDay(activeSession.StageIndex, activeSession.DayIndex);
            currentReward = table.FindReward(currentDay.RewardRuleId);
            table.FindEncounterRows(currentDay.EncounterId, activeEncounterRows);

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

        private void BeginRunSession()
        {
            var monster = GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(StartContext.SelectedMonsterId);
            activeSession = RunSession.Begin(monster);
            StartContext.Clear();
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

                    var isBoss = IsBossEncounter(row);
                    var healthMultiplier = isBoss
                        ? UnityEngine.Random.Range(row.BossHealthMultiplierMin, row.BossHealthMultiplierMax)
                        : 1f;
                    unitSpawnManager.SpawnEnemyById(
                        row.EnemyId,
                        spawnIndex,
                        row.SpawnX,
                        row.SpawnYMin,
                        row.SpawnYMax,
                        healthMultiplier,
                        isBoss);

                    spawnIndex += 1;
                    yield return new WaitForSeconds(Mathf.Max(0f, row.IntervalSeconds));
                }
            }
        }

        /// 모든 적이 사라지거나 전투가 패배로 끝날 때까지 기다린다.
        private IEnumerator WaitForEnemyClear()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.05f, clearCheckInterval));

            while (combatManager.ActiveEnemyCount > 0 && State != StageState.Defeat)
            {
                yield return wait;
            }
        }

        /// 전투 결과를 바탕으로 골드·Dark Trace·포로 보상을 준비한다.
        private void PrepareReward()
        {
            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = currentReward.Gold;
            PendingDarkTraceReward = currentReward.DarkTrace;
            PendingPrisonerCount = currentReward.RollPrisonerCount();

            AddGuaranteedPrisoners();
            BuildPrisonerCandidatePool();
            AddCandidatePrisonersUntilFull();
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

        /// 확정 포로를 제외한 후보에서 이번 보상에 사용할 포로 목록을 준비한다.
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

        /// BossRows를 선택한다.
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
            if (row.SelectedAsBoss)
            {
                return true;
            }

            var isMidbossCombat = activeSession.DayIndex == 5 || activeSession.DayIndex == 10;
            return isMidbossCombat && (row.IsGuaranteedBoss || row.IsBossCandidate);
        }

        private void ResolveEndFlowReferences()
        {
            if (winButton == null)
            {
                winButton = winPanel.GetComponentInChildren<Button>(true);
            }

            if (defeatButton == null)
            {
                defeatButton = defeatPanel.GetComponentInChildren<Button>(true);
            }
        }

        private void EnsureNexusRegistered()
        {
            ResolveEndFlowReferences();
            unitSpawnManager.RegisterNexus(nexusActor);
            RestorePreservedNexusHealth();
        }

        private void PreserveCurrentNexusHealth()
        {
            ResolveEndFlowReferences();
            preservedNexusHealth = Mathf.Max(0f, nexusActor.Model.Resources.CurrentHealth);
            hasPreservedNexusHealth = true;
        }

        private void RestorePreservedNexusHealth()
        {
            if (!hasPreservedNexusHealth)
            {
                return;
            }

            nexusActor.SetCurrentHealth(preservedNexusHealth);
        }

        private void OnUnitDefeated(UnitCombatState defeatedUnit)
        {
            if (!defeatedUnit.IsNexus)
            {
                return;
            }

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

        /// 승리·패배 패널과 보상 관련 화면을 닫는다.
        private void HideEndPanels()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, false);
        }

        /// 전투 승리 패널을 열고 패배 패널은 닫는다.
        private void ShowWinPanel()
        {
            SetActive(defeatPanel, false);
            SetActive(winPanel, true);
        }

        /// 전투 패배 패널을 열고 승리 패널은 닫는다.
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

            winButton.onClick.AddListener(ReturnToMainMenu);
            defeatButton.onClick.AddListener(ReturnToMainMenu);

            endButtonsBound = true;
        }

        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuScenePath);
        }

        /// 현재 Run 위치가 설정된 승리 시점인지 확인한다.
        private bool IsConfiguredWinDay()
        {
            return activeSession.StageIndex == winStageIndex
                && activeSession.DayIndex == winDayIndex;
        }

        private static void SetActive(GameObject target, bool active)
        {
            target.SetActive(active);
        }

        /// Tables를 불러온다.
        private void LoadTables()
        {
            table = StageFlowTable.Load(
                stageDayCsv,
                stage1EncounterCsv,
                stage1RewardCsv,
                stage2EncounterCsv,
                stage2RewardCsv);
        }
    }

    /// StartContext 처리에 필요한 불변 실행 문맥을 전달한다.
    public static class StartContext
    {
        public static string SelectedMonsterId { get; private set; }

        public static void Prepare(string selectedMonsterId)
        {
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        public static void Clear()
        {
            SelectedMonsterId = string.Empty;
        }
    }

    public enum StageState
    {
        NotStarted,
        Spawning,
        Combat,
        RewardReady,
        Victory,
        Defeat
    }

    /// StageDayRow에 해당하는 CSV 한 행을 표현한다.
    internal class StageDayRow
    {
        public int Stage;
        public int Day;
        public string EncounterId;
        public string RewardRuleId;
    }

    /// StageEncounterRow에 해당하는 CSV 한 행을 표현한다.
    internal class StageEncounterRow
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

    /// StageRewardRow에 해당하는 CSV 한 행을 표현한다.
    internal class StageRewardRow
    {
        public string RewardRuleId;
        public int Gold;
        public int DarkTrace;
        public float PrisonerCount1Chance;
        public float PrisonerCount2Chance;
        public float ManifestSuccessChance;
        public int EliteBonusPrisoners;

        /// 보상 규칙의 확률에 따라 이번 전투에서 얻을 포로 수를 결정한다.
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

    internal class StageFlowTable
    {
        private readonly List<StageDayRow> days = new List<StageDayRow>();
        private readonly List<StageEncounterRow> encounters = new List<StageEncounterRow>();
        private readonly List<StageRewardRow> rewards = new List<StageRewardRow>();

        public static StageFlowTable Load(
            TextAsset dayCsv,
            TextAsset stage1EncounterCsv,
            TextAsset stage1RewardCsv,
            TextAsset stage2EncounterCsv,
            TextAsset stage2RewardCsv)
        {
            var table = new StageFlowTable();
            table.LoadDays(dayCsv);
            table.LoadEncounters(stage1EncounterCsv);
            table.LoadRewards(stage1RewardCsv);
            table.LoadEncounters(stage2EncounterCsv);
            table.LoadRewards(stage2RewardCsv);
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
                    BossHealthMultiplierMin = ParseFloat(row, "boss_health_multiplier_min"),
                    BossHealthMultiplierMax = ParseFloat(row, "boss_health_multiplier_max"),
                    GuaranteedPrisoner = ParseBool(row, "guaranteed_prisoner")
                });
            }
        }

        private void LoadRewards(TextAsset csv)
        {
            foreach (var row in ReadRows(csv))
            {

                _ = ParseFloat(row, "prisoner_count_3_chance");

                rewards.Add(new StageRewardRow
                {
                    RewardRuleId = Read(row, "reward_rule_id"),
                    Gold = ParseInt(row, "gold"),
                    DarkTrace = ParseInt(row, "dark_trace"),
                    PrisonerCount1Chance = ParseFloat(row, "prisoner_count_1_chance"),
                    PrisonerCount2Chance = ParseFloat(row, "prisoner_count_2_chance"),
                    ManifestSuccessChance = ParseFloat(row, "manifest_success_chance"),
                    EliteBonusPrisoners = ParseInt(row, "elite_bonus_prisoners")
                });
            }
        }

        private static IEnumerable<Dictionary<string, string>> ReadRows(TextAsset csv)
        {
            if (string.IsNullOrWhiteSpace(csv.text))
            {
                yield break;
            }

            var lines = csv.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
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
            return int.Parse(Read(row, key), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private static float ParseFloat(Dictionary<string, string> row, string key)
        {
            return float.Parse(Read(row, key), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private static bool ParseBool(Dictionary<string, string> row, string key)
        {
            return bool.TryParse(Read(row, key), out var value) && value;
        }
    }
}
