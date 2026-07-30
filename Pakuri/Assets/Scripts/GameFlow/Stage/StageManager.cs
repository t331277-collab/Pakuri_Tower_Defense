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

    /// StageManager가 담당하는 작업을 조정하고 공유 런타임 상태를 소유한다.
    public class StageManager : MonoBehaviour
    {
        private const float DefaultClearCheckInterval = 0.25f;

        private readonly List<StageEncounterRow> activeEncounterRows = new List<StageEncounterRow>();
        private readonly List<string> pendingPrisonerEnemyIds = new List<string>();
        private readonly List<string> prisonerCandidatePool = new List<string>();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private TextAsset stageDayCsv;
        [SerializeField] private TextAsset stageEncounterCsv;
        [SerializeField] private TextAsset stageRewardCsv;
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

        /// CurrentDay를 시작한다.
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

        /// ContinueToNextDay 작업을 수행한다.
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

        /// AdvanceDay 작업을 수행한다.
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

        /// RestorePlayerHealthForNextDay 작업을 수행한다.
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

        /// RunCurrentDayFlow 결과값을 생성해 반환한다.
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

        /// BeginRunSession 작업을 수행한다.
        private void BeginRunSession()
        {
            var monster = GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(StartContext.SelectedMonsterId);
            activeSession = RunSession.Begin(monster);
            StartContext.Clear();
        }

        /// 전달된 rows 값을 사용해 EncounterRows를 런타임 씬 오브젝트로 생성하고 등록한다.
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

        /// WaitForEnemyClear 결과값을 생성해 반환한다.
        private IEnumerator WaitForEnemyClear()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.05f, clearCheckInterval));

            while (combatManager.ActiveEnemyCount > 0 && State != StageState.Defeat)
            {
                yield return wait;
            }
        }

        /// PrepareReward 작업을 수행한다.
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

        /// GuaranteedPrisoners를 소유한 런타임 상태에 추가한다.
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

        /// PrisonerCandidatePool를 구성한다.
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

        /// CandidatePrisonersUntilFull를 소유한 런타임 상태에 추가한다.
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

        /// 전달된 enemyId 값을 사용해 Prisoner를 소유한 런타임 상태에 추가한다.
        private void AddPrisoner(string enemyId)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            pendingPrisonerEnemyIds.Add(enemyId);
        }

        /// 전달된 enemyId 값을 사용해 OnePrisonerCandidate를 소유한 런타임 상태에서 제거한다.
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

        /// 전달된 row 값을 사용해 BossEncounter 조건 충족 여부를 반환한다.
        private bool IsBossEncounter(StageEncounterRow row)
        {
            if (row.SelectedAsBoss)
            {
                return true;
            }

            var isMidbossCombat = activeSession.DayIndex == 5 || activeSession.DayIndex == 10;
            return isMidbossCombat && (row.IsGuaranteedBoss || row.IsBossCandidate);
        }

        /// EndFlowReferences를 결정한다.
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

        /// EnsureNexusRegistered 작업을 수행한다.
        private void EnsureNexusRegistered()
        {
            ResolveEndFlowReferences();
            unitSpawnManager.RegisterNexus(nexusActor);
            RestorePreservedNexusHealth();
        }

        /// PreserveCurrentNexusHealth 작업을 수행한다.
        private void PreserveCurrentNexusHealth()
        {
            ResolveEndFlowReferences();
            preservedNexusHealth = Mathf.Max(0f, nexusActor.Model.Resources.CurrentHealth);
            hasPreservedNexusHealth = true;
        }

        /// RestorePreservedNexusHealth 작업을 수행한다.
        private void RestorePreservedNexusHealth()
        {
            if (!hasPreservedNexusHealth)
            {
                return;
            }

            nexusActor.SetCurrentHealth(preservedNexusHealth);
        }

        /// 전달된 defeatedUnit 값을 사용해 OnUnitDefeated 작업을 수행한다.
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

        /// EndPanels를 숨긴다.
        private void HideEndPanels()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, false);
        }

        /// WinPanel를 표시한다.
        private void ShowWinPanel()
        {
            SetActive(defeatPanel, false);
            SetActive(winPanel, true);
        }

        /// DefeatPanel를 표시한다.
        private void ShowDefeatPanel()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, true);
        }

        /// EndButtons를 런타임 사건 또는 씬 대상에 연결한다.
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

        /// ReturnToMainMenu 작업을 수행한다.
        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuScenePath);
        }

        /// ConfiguredWinDay 조건 충족 여부를 반환한다.
        private bool IsConfiguredWinDay()
        {
            return activeSession.StageIndex == winStageIndex
                && activeSession.DayIndex == winDayIndex;
        }

        /// 전달된 런타임 입력값을 사용해 Active를 갱신한다.
        private static void SetActive(GameObject target, bool active)
        {
            target.SetActive(active);
        }

        /// Tables를 불러온다.
        private void LoadTables()
        {
            table = StageFlowTable.Load(stageDayCsv, stageEncounterCsv, stageRewardCsv);
        }
    }

    /// StartContext 처리에 필요한 불변 실행 문맥을 전달한다.
    public static class StartContext
    {
        public static string SelectedMonsterId { get; private set; }

        /// 전달된 selectedMonsterId 값을 사용해 Prepare 작업을 수행한다.
        public static void Prepare(string selectedMonsterId)
        {
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        /// 소유한 모든 런타임 값를 소유한 런타임 상태에서 비운다.
        public static void Clear()
        {
            SelectedMonsterId = string.Empty;
        }
    }

    /// StageState에서 지원하는 값의 종류를 정의한다.
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

        /// RollPrisonerCount 결과값을 생성해 반환한다.
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

    /// StageFlowTable가 소유하는 데이터와 동작을 캡슐화한다.
    internal class StageFlowTable
    {
        private readonly List<StageDayRow> days = new List<StageDayRow>();
        private readonly List<StageEncounterRow> encounters = new List<StageEncounterRow>();
        private readonly List<StageRewardRow> rewards = new List<StageRewardRow>();

        /// 전달된 런타임 입력값을 사용해 요청값를 불러온다.
        public static StageFlowTable Load(TextAsset dayCsv, TextAsset encounterCsv, TextAsset rewardCsv)
        {
            var table = new StageFlowTable();
            table.LoadDays(dayCsv);
            table.LoadEncounters(encounterCsv);
            table.LoadRewards(rewardCsv);
            return table;
        }

        /// 전달된 런타임 입력값을 사용해 Day를 찾는다.
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

        /// 전달된 rewardRuleId 값을 사용해 Reward를 찾는다.
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

        /// 전달된 런타임 입력값을 사용해 EncounterRows를 찾는다.
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

        /// 전달된 csv 값을 사용해 Days를 불러온다.
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

        /// 전달된 csv 값을 사용해 Encounters를 불러온다.
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

        /// 전달된 csv 값을 사용해 Rewards를 불러온다.
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

        /// 전달된 csv 값을 사용해 Rows를 읽는다.
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

        /// 전달된 line 값을 사용해 SplitCsvLine 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 요청값를 읽는다.
        private static string Read(Dictionary<string, string> row, string key)
        {
            return row.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
        }

        /// 전달된 런타임 입력값을 사용해 Int 값을 런타임 표현으로 파싱한다.
        private static int ParseInt(Dictionary<string, string> row, string key)
        {
            return int.Parse(Read(row, key), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /// 전달된 런타임 입력값을 사용해 Float 값을 런타임 표현으로 파싱한다.
        private static float ParseFloat(Dictionary<string, string> row, string key)
        {
            return float.Parse(Read(row, key), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// 전달된 런타임 입력값을 사용해 Bool 값을 런타임 표현으로 파싱한다.
        private static bool ParseBool(Dictionary<string, string> row, string key)
        {
            return bool.TryParse(Read(row, key), out var value) && value;
        }
    }
}
