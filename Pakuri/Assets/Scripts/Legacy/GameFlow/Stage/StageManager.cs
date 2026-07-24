using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * 현재 런 세션의 날짜별 전투 진행을 관리하는 컴포넌트.
 * 스테이지 표에서 인카운터와 보상을 읽어 적 생성, 전투 종료 대기, 보상 준비,
 * 다음 날짜 진행을 순서대로 실행하고 넥서스 체력 보존과 최종 승패 화면도 처리한다.
 */
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

        /*
         * 종료 화면 참조와 버튼 이벤트를 준비한다.
         */
        private void Awake()
        {
            ResolveEndFlowReferences();
            HideEndPanels();
            BindEndButtons();
            combatManager.UnitDefeated += OnUnitDefeated;
        }

        /*
         * 스테이지 표를 읽고 설정에 따라 현재 날짜를 시작한다.
         */
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

        /*
         * 전투 패배 통지 연결을 해제한다.
         */
        private void OnDestroy()
        {
            combatManager.UnitDefeated -= OnUnitDefeated;
        }

        /*
         * 진행 중인 날짜 흐름을 정리하고 현재 날짜를 시작한다.
         */
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

        /*
         * 보상 상태를 정리하고 세션을 다음 날짜로 진행한다.
         */
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

        /*
         * 다음 일차로 이동하고 11일차 다음에는 다음 스테이지의 1일차로 이동한다.
         */
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

        /*
         * 다음 날짜에 플레이어 파티를 복원하고 몬스터 체력을 채운다.
         */
        private void RestorePlayerHealthForNextDay()
        {
            if (!restorePlayerHealthOnDayAdvance)
            {
                return;
            }

            unitSpawnManager.RestorePlayerPartyFromSession(activeSession);
            var players = combatManager.UnitRegistry.Players;

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
                combatManager.UnitRegistry.RefreshDisplay(model);
            }
        }

        /*
         * 현재 날짜의 세션, 전투표, 적 생성, 승리, 보상 흐름을 순서대로 실행한다.
         */
        private IEnumerator RunCurrentDayFlow()
        {
            if (activeSession == null)
            {
                BeginRunSession();
            }

            unitSpawnManager.SpawnSelectedPlayerUnit(activeSession);

            EnsureNexusRegistered();

            // 세션의 날짜를 기준으로 날짜 행, 보상 행, 인카운터 행을 차례로 연결한다.
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

        /*
         * 씬 전환으로 전달된 선택 몬스터를 기준으로 새 런 세션을 시작한다.
         */
        private void BeginRunSession()
        {
            var monster = GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(StartContext.SelectedMonsterId);
            activeSession = RunSession.Begin(monster);
            StartContext.Clear();
        }

        /*
         * 전투표 순서와 간격에 맞춰 적을 생성한다.
         */
        private IEnumerator SpawnEncounterRows(IReadOnlyList<StageEncounterRow> rows /* 행 목록 */)
        {
            var spawnIndex = 0;

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var count = Mathf.Max(0, row.Count);

                for (var j = 0; j < count; j++)
                {
                    // 같은 행의 보스 여부는 공유하지만 체력 배율은 생성 개체마다 다시 뽑는다.
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

        /*
         * 모든 적이 제거되거나 패배 상태가 될 때까지 기다린다.
         */
        private IEnumerator WaitForEnemyClear()
        {
            var wait = new WaitForSeconds(Mathf.Max(0.05f, clearCheckInterval));

            // 패배 상태가 되면 적이 남아 있어도 현재 날짜의 승리 대기를 끝낸다.
            while (combatManager.ActiveEnemyCount > 0 && State != StageState.Defeat)
            {
                yield return wait;
            }
        }

        /*
         * 현재 보상표와 전투 결과로 지급 보상과 포로 목록을 준비한다.
         */
        private void PrepareReward()
        {
            pendingPrisonerEnemyIds.Clear();
            PendingGoldReward = currentReward.Gold;
            PendingDarkTraceReward = currentReward.DarkTrace;
            PendingPrisonerCount = currentReward.RollPrisonerCount();

            // 확정 포로를 먼저 넣고 남은 수만 무작위 후보에서 채운다.
            AddGuaranteedPrisoners();
            BuildPrisonerCandidatePool();
            AddCandidatePrisonersUntilFull();
        }

        /*
         * 확정 포로와 선택된 보스를 포로 목록에 추가한다.
         */
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

        /*
         * 생성된 적 수를 기준으로 무작위 포로 후보 목록을 만든다.
         */
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

            // 이미 확정된 포로는 무작위 후보에서 한 명씩 제외한다.
            for (var i = 0; i < pendingPrisonerEnemyIds.Count; i++)
            {
                RemoveOnePrisonerCandidate(pendingPrisonerEnemyIds[i]);
            }
        }

        /*
         * 목표 포로 수가 찰 때까지 후보를 무작위로 추가한다.
         */
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

        /*
         * 유효한 적 ID를 포로 목록에 추가한다.
         */
        private void AddPrisoner(string enemyId /* 적 식별자 */)
        {
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                return;
            }

            pendingPrisonerEnemyIds.Add(enemyId);
        }

        /*
         * 지정 적 ID와 일치하는 포로 후보 하나를 제거한다.
         */
        private void RemoveOnePrisonerCandidate(string enemyId /* 적 식별자 */)
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

        /*
         * 확정 보스를 표시하고 일반 보스 후보 중 하나를 선택한다.
         */
        private void SelectBossRows()
        {
            var normalBossCandidates = new List<StageEncounterRow>();

            // 보장 보스는 모두 유지하고 일반 후보는 별도 목록에 모은다.
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

            // 일반 후보가 여러 행이어도 그중 한 행만 이번 전투의 보스로 선택한다.
            normalBossCandidates[UnityEngine.Random.Range(0, normalBossCandidates.Count)].SelectedAsBoss = true;
        }

        /*
         * 전투 유형과 행 설정을 기준으로 보스 생성 여부를 반환한다.
         */
        private bool IsBossEncounter(StageEncounterRow row /* 스테이지 전투 조우 행 */)
        {
            if (row.SelectedAsBoss)
            {
                return true;
            }

            // 5일과 10일 중간 보스 전투에서는 보스 설정 행을 모두 보스로 취급한다.
            var isMidbossCombat = activeSession.DayIndex == 5 || activeSession.DayIndex == 10;
            return isMidbossCombat && (row.IsGuaranteedBoss || row.IsBossCandidate);
        }

        /*
         * 승리와 패배 패널에서 사용할 버튼 참조를 찾는다.
         */
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

        /*
         * 넥서스를 초기화하고 전투 등록소에 등록한다.
         */
        private void EnsureNexusRegistered()
        {
            ResolveEndFlowReferences();
            unitSpawnManager.RegisterNexus(nexusActor);
            RestorePreservedNexusHealth();
        }

        /*
         * 날짜 전환 전에 현재 넥서스 체력을 보관한다.
         */
        private void PreserveCurrentNexusHealth()
        {
            ResolveEndFlowReferences();
            preservedNexusHealth = Mathf.Max(0f, nexusActor.Model.Resources.CurrentHealth);
            hasPreservedNexusHealth = true;
        }

        /*
         * 보관된 넥서스 체력을 새 날짜에 복원한다.
         */
        private void RestorePreservedNexusHealth()
        {
            if (!hasPreservedNexusHealth)
            {
                return;
            }

            nexusActor.SetCurrentHealth(preservedNexusHealth);
        }

        /*
         * 넥서스 패배 시 날짜 흐름을 중단하고 패배 화면을 표시한다.
         */
        private void OnUnitDefeated(UnitCombatState defeatedUnit /* 쓰러진 유닛 */)
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

        /*
         * 승리와 패배 패널을 모두 숨긴다.
         */
        private void HideEndPanels()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, false);
        }

        /*
         * 패배 패널을 숨기고 승리 패널을 표시한다.
         */
        private void ShowWinPanel()
        {
            SetActive(defeatPanel, false);
            SetActive(winPanel, true);
        }

        /*
         * 승리 패널을 숨기고 패배 패널을 표시한다.
         */
        private void ShowDefeatPanel()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, true);
        }

        /*
         * 승리와 패배 버튼에 메인 메뉴 이동 이벤트를 한 번 연결한다.
         */
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

        /*
         * 설정된 메인 메뉴 씬을 불러온다.
         */
        private void ReturnToMainMenu()
        {
            SceneManager.LoadScene(mainMenuScenePath);
        }

        /*
         * 현재 세션이 설정된 최종 승리 날짜인지 반환한다.
         */
        private bool IsConfiguredWinDay()
        {
            return activeSession.StageIndex == winStageIndex
                && activeSession.DayIndex == winDayIndex;
        }

        /*
         * 지정 오브젝트의 활성 상태를 변경한다.
         */
        private static void SetActive(GameObject target /* 활성화하거나 변경할 게임 오브젝트 */, bool active /* 대상 활성화 여부 */)
        {
            target.SetActive(active);
        }

        /*
         * 날짜, 전투, 보상 CSV를 스테이지 표로 읽는다.
         */
        private void LoadTables()
        {
            table = StageFlowTable.Load(stageDayCsv, stageEncounterCsv, stageRewardCsv);
        }
    }

    /*
     * 씬 전환 사이에 선택 몬스터 ID를 임시 보관한다.
     */
    public static class StartContext
    {
        public static string SelectedMonsterId { get; private set; }

        /*
         * 다음 인게임 씬에서 사용할 선택 몬스터 ID를 저장한다.
         */
        public static void Prepare(string selectedMonsterId /* 선택된 몬스터 식별자 */)
        {
            SelectedMonsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? string.Empty : selectedMonsterId;
        }

        /*
         * 사용이 끝난 선택 몬스터 ID를 비운다.
         */
        public static void Clear()
        {
            SelectedMonsterId = string.Empty;
        }
    }

    /*
     * 현재 스테이지 진행 상태를 나타낸다.
     */
    public enum StageState
    {
        NotStarted,
        Spawning,
        Combat,
        RewardReady,
        Victory,
        Defeat
    }

    /*
     * 날짜별 전투와 보상 규칙 연결을 보관한다.
     */
    internal class StageDayRow
    {
        public int Stage;
        public int Day;
        public string EncounterId;
        public string RewardRuleId;
    }

    /*
     * 한 전투의 적 생성과 보스 설정을 보관한다.
     */
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

    /*
     * 전투 종료 후 지급할 재화와 포로 확률을 보관한다.
     */
    internal class StageRewardRow
    {
        public string RewardRuleId;
        public int Gold;
        public int DarkTrace;
        public float PrisonerCount1Chance;
        public float PrisonerCount2Chance;
        public float ManifestSuccessChance;
        public int EliteBonusPrisoners;

        /*
         * 설정된 확률에 따라 포로 수를 결정한다.
         */
        public int RollPrisonerCount()
        {
            var roll = UnityEngine.Random.value;

            // 두 확률을 누적 구간으로 비교하고 남은 구간은 3명을 지급한다.
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

    /*
     * 스테이지 CSV를 읽고 날짜, 전투, 보상 행을 조회한다.
     */
    internal class StageFlowTable
    {
        private readonly List<StageDayRow> days = new List<StageDayRow>();
        private readonly List<StageEncounterRow> encounters = new List<StageEncounterRow>();
        private readonly List<StageRewardRow> rewards = new List<StageRewardRow>();

        /*
         * 세 CSV에서 스테이지 표를 만든다.
         */
        public static StageFlowTable Load(TextAsset dayCsv /* 일차 CSV */, TextAsset encounterCsv /* 전투 조우 CSV */, TextAsset rewardCsv /* 보상 CSV */)
        {
            var table = new StageFlowTable();
            table.LoadDays(dayCsv);
            table.LoadEncounters(encounterCsv);
            table.LoadRewards(rewardCsv);
            return table;
        }

        /*
         * 스테이지와 날짜가 일치하는 날짜 행을 찾는다.
         */
        public StageDayRow FindDay(int stage /* 스테이지 */, int day /* 일차 */)
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

        /*
         * ID가 일치하는 보상 행을 찾는다.
         */
        public StageRewardRow FindReward(string rewardRuleId /* 보상 규칙 식별자 */)
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

        /*
         * ID가 일치하는 전투 행을 생성 순서로 반환한다.
         */
        public void FindEncounterRows(string encounterId /* 전투 조우 식별자 */, List<StageEncounterRow> results /* 처리 결과 목록 */)
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

            // CSV의 생성 순서가 실제 적 생성 순서가 되도록 마지막에 정렬한다.
            results.Sort((left, right) => left.SpawnOrder.CompareTo(right.SpawnOrder));
        }

        /*
         * 날짜 CSV 행을 런타임 목록에 추가한다.
         */
        private void LoadDays(TextAsset csv /* CSV */)
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

        /*
         * 전투 CSV 행을 런타임 목록에 추가한다.
         */
        private void LoadEncounters(TextAsset csv /* CSV */)
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

        /*
         * 보상 CSV 행을 런타임 목록에 추가한다.
         */
        private void LoadRewards(TextAsset csv /* CSV */)
        {
            foreach (var row in ReadRows(csv))
            {
                // 3명 확률은 나머지 누적 구간으로 결정되므로 형식만 검증한다.
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

        /*
         * CSV 본문을 헤더 기준의 문자열 사전으로 변환한다.
         */
        private static IEnumerable<Dictionary<string, string>> ReadRows(TextAsset csv /* CSV */)
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
                    // 행에 빠진 뒤쪽 열은 빈 문자열로 채워 헤더 수를 유지한다.
                    row[headers[j]] = j < values.Count ? values[j] : string.Empty;
                }

                yield return row;
            }
        }

        /*
         * 따옴표와 이스케이프 따옴표를 처리해 CSV 한 줄을 분리한다.
         */
        private static List<string> SplitCsvLine(string line /* 직선 */)
        {
            var values = new List<string>();
            var current = string.Empty;
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    // 따옴표 안의 연속 따옴표 두 개는 실제 따옴표 한 글자로 읽는다.
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

        /*
         * CSV 행에서 문자열 값을 읽고 앞뒤 공백을 제거한다.
         */
        private static string Read(Dictionary<string, string> row /* 열 이름과 값으로 구성된 CSV 행 */, string key /* 조회 키 */)
        {
            return row.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
        }

        /*
         * CSV 행의 값을 정수로 변환한다.
         */
        private static int ParseInt(Dictionary<string, string> row /* 열 이름과 값으로 구성된 CSV 행 */, string key /* 조회 키 */)
        {
            return int.Parse(Read(row, key), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /*
         * CSV 행의 값을 실수로 변환한다.
         */
        private static float ParseFloat(Dictionary<string, string> row /* 열 이름과 값으로 구성된 CSV 행 */, string key /* 조회 키 */)
        {
            return float.Parse(Read(row, key), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /*
         * CSV 행의 값을 논리값으로 변환한다.
         */
        private static bool ParseBool(Dictionary<string, string> row /* 열 이름과 값으로 구성된 CSV 행 */, string key /* 조회 키 */)
        {
            return bool.TryParse(Read(row, key), out var value) && value;
        }
    }
}
