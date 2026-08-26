/*
 * 역할: Stage 및 Wave 진행.
 * 책임: Stage 시작·적 Wave 예약·전투 종료 감지·보상 지급·Run 진행을 처리한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakuri.InGame
{

    public class StageManager : MonoBehaviour
    {
        private const float DefaultClearCheckInterval = 0.25f;

        private readonly List<StageEncounterDefinition> activeEncounterRows = new List<StageEncounterDefinition>();
        private readonly List<string> pendingPrisonerEnemyNames = new List<string>();
        private readonly List<string> prisonerCandidatePool = new List<string>();
        private readonly ArtifactSynergyManager artifactSynergyManager = new ArtifactSynergyManager();

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private bool startFlowOnStart = true;
        [SerializeField] private float clearCheckInterval = DefaultClearCheckInterval;
        [SerializeField] private bool restorePlayerHealthOnDayAdvance = true;
        [SerializeField] private NexusActor nexusActor;
        [SerializeField] private StageEndPanelUI winPanelUI;
        [SerializeField] private StageEndPanelUI defeatPanelUI;
        [SerializeField] private string mainMenuScenePath = "Assets/Scenes/NewScene/MainMenuScene.unity";
        [SerializeField] private int winStageIndex = 2;
        [SerializeField] private int winDayIndex = 11;

        private StageDefinition stageDefinition;
        private Coroutine flowCoroutine;
        private StageDayDefinition currentDay;
        private StageRewardDefinition currentReward;
        private RunSession activeSession;
        private bool endButtonsBound;
        private StageState state = StageState.NotStarted;

        public StageState State
        {
            get => state;
            private set
            {
                if (state == value)
                {
                    return;
                }

                state = value;
                StateChanged?.Invoke(state);
            }
        }
        public event Action<StageState> StateChanged;
        public event Func<bool> ContinueRequested;
        public int CurrentStage => activeSession != null ? activeSession.StageIndex : 1;
        public int CurrentDay => activeSession != null ? activeSession.DayIndex : 1;
        public IReadOnlyList<string> PendingPrisonerEnemyNames => pendingPrisonerEnemyNames;
        public int PendingGoldReward { get; private set; }
        public int PendingDarkTraceReward { get; private set; }
        public int PendingPrisonerCount { get; private set; }
        public int PendingArtifactChoiceCount { get; private set; }
        public float PendingManifestSuccessChance => currentReward != null ? currentReward.ManifestSuccessChance : 0.7f;
        public RunSession ActiveSession => activeSession;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            HideEndPanels();
            BindEndPanelButtons();
            combatManager.UnitDefeated += OnUnitDefeated;
        }

        /// 컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.
        private void Start()
        {
            HideEndPanels();
            BindEndPanelButtons();
            stageDefinition = StartContext.Mode == RunMode.Tutorial
                ? GameDataLoader.CurrentCatalog.TutorialStage
                : GameDataLoader.CurrentCatalog.Stage;

            if (StartContext.Mode == RunMode.Tutorial)
            {
                BeginRunSession();
                var tutorialRoot = GameObject.Find("TutorialUI");
                if (tutorialRoot == null)
                {
                    Debug.LogError("StageManager requires scene root 'TutorialUI' for a tutorial run.", this);
                    return;
                }

                var flowManager = tutorialRoot.GetComponent<TutorialFlowManager>()
                    ?? tutorialRoot.AddComponent<TutorialFlowManager>();
                flowManager.Initialize(this, combatManager, FindFirstObjectByType<InGameUIManager>());
                return;
            }

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

        /// 전투 중 선택받은자 Highlight 쿨타임 초기화를 진행한다.
        private void Update()
        {
            artifactSynergyManager.TickStage(Time.deltaTime, State, unitSpawnManager);
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

        /// 보상을 정리하고 RunSession을 다음 Day로 넘긴 뒤 파티를 복구
        public void ContinueToNextDay()
        {
            pendingPrisonerEnemyNames.Clear();
            PendingGoldReward = 0;
            PendingDarkTraceReward = 0;
            PendingPrisonerCount = 0;
            PendingArtifactChoiceCount = 0;
            combatManager.ResetCombatState();

            if (IsConfiguredWinDay())
            {
                ShowWinPanel();
                State = StageState.Victory;
                return;
            }

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
            artifactSynergyManager.PrepareStage(
                activeSession,
                spawnManager: unitSpawnManager,
                effectManager: combatManager.Effects);

            currentDay = stageDefinition.FindDay(activeSession.StageIndex, activeSession.DayIndex);
            currentReward = stageDefinition.FindReward(currentDay.RewardRuleName);
            stageDefinition.FindEncounterRows(currentDay.EncounterName, activeEncounterRows);

            SelectBossRows();
            combatManager.BeginPlayerCombat(
                activeEncounterRows.Exists(row => row.Count > 0 && IsBossEncounter(row)));
            State = StageState.Spawning;
            yield return SpawnEncounterRows(activeEncounterRows);

            State = StageState.Combat;
            yield return WaitForEnemyClear();

            PrepareReward();
            State = StageState.RewardReady;
            flowCoroutine = null;
        }

        private void BeginRunSession()
        {
            var monster = GameDataLoader.CurrentCatalog.GetData<MonsterDefinition>(StartContext.SelectedMonsterName);
            activeSession = RunSession.Begin(monster, StartContext.Mode);
            StartContext.Clear();
        }

        private IEnumerator SpawnEncounterRows(IReadOnlyList<StageEncounterDefinition> rows)
        {
            var spawnIndex = 0;

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var count = Mathf.Max(0, row.Count);

                for (var j = 0; j < count; j++)
                {

                    var isOriginalBoss = IsOriginalBoss(row);
                    var isRunAssignedBoss = !isOriginalBoss && IsRunAssignedBoss(row);
                    var isBoss = isOriginalBoss || isRunAssignedBoss;
                    var healthMultiplier = isRunAssignedBoss
                        ? UnityEngine.Random.Range(row.BossHealthMultiplierMin, row.BossHealthMultiplierMax)
                        : 1f;
                    unitSpawnManager.SpawnEnemyByName(
                        row.EnemyName,
                        spawnIndex,
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
            pendingPrisonerEnemyNames.Clear();
            PendingGoldReward = currentReward.Gold;
            PendingDarkTraceReward = currentReward.DarkTrace;
            PendingPrisonerCount = currentReward.RollPrisonerCount();
            PendingArtifactChoiceCount = Math.Max(0, currentReward.ArtifactChoiceCount);

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
                    AddPrisoner(row.EnemyName);
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
                    prisonerCandidatePool.Add(row.EnemyName);
                }
            }

            for (var i = 0; i < pendingPrisonerEnemyNames.Count; i++)
            {
                RemoveOnePrisonerCandidate(pendingPrisonerEnemyNames[i]);
            }
        }

        private void AddCandidatePrisonersUntilFull()
        {
            if (PendingPrisonerCount <= 0)
            {
                return;
            }

            while (pendingPrisonerEnemyNames.Count < PendingPrisonerCount && prisonerCandidatePool.Count > 0)
            {
                var poolIndex = UnityEngine.Random.Range(0, prisonerCandidatePool.Count);
                AddPrisoner(prisonerCandidatePool[poolIndex]);
                prisonerCandidatePool.RemoveAt(poolIndex);
            }
        }

        private void AddPrisoner(string enemyName)
        {
            if (string.IsNullOrWhiteSpace(enemyName))
            {
                return;
            }

            pendingPrisonerEnemyNames.Add(enemyName);
        }

        private void RemoveOnePrisonerCandidate(string enemyName)
        {
            if (string.IsNullOrWhiteSpace(enemyName))
            {
                return;
            }

            for (var i = 0; i < prisonerCandidatePool.Count; i++)
            {
                if (string.Equals(prisonerCandidatePool[i], enemyName, StringComparison.OrdinalIgnoreCase))
                {
                    prisonerCandidatePool.RemoveAt(i);
                    return;
                }
            }
        }

        /// BossRows를 선택한다.
        private void SelectBossRows()
        {
            var normalBossCandidates = new List<StageEncounterDefinition>();
            var allowRandomBossSelection = currentDay != null
                && string.Equals(currentDay.CombatType, "Normal", StringComparison.OrdinalIgnoreCase);

            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                var row = activeEncounterRows[i];
                row.SelectedAsBoss = false;

                if (!row.IsBossCandidate)
                {
                    row.SelectedAsBoss = true;
                    continue;
                }

                if (allowRandomBossSelection)
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

        private bool IsBossEncounter(StageEncounterDefinition row)
        {
            return IsOriginalBoss(row) || IsRunAssignedBoss(row);
        }

        public bool CanContinueToNextDay()
        {
            if (ContinueRequested == null)
            {
                return true;
            }

            foreach (Func<bool> handler in ContinueRequested.GetInvocationList())
            {
                if (!handler())
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsRunAssignedBoss(StageEncounterDefinition row)
        {
            return row.SelectedAsBoss;
        }

        private bool IsOriginalBoss(StageEncounterDefinition row)
        {
            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(row.EnemyName)
                ?? throw new InvalidOperationException($"Enemy data '{row.EnemyName}' is required.");
            return enemy.EncounterRole != EnemyEncounterRole.Normal;
        }

        private void EnsureNexusRegistered()
        {
            unitSpawnManager.RegisterNexus(nexusActor);
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
            winPanelUI.SetVisible(false);
            defeatPanelUI.SetVisible(false);
        }

        /// 전투 승리 패널을 열고 패배 패널은 닫는다.
        private void ShowWinPanel()
        {
            defeatPanelUI.SetVisible(false);
            winPanelUI.SetVisible(true);
        }

        /// 전투 패배 패널을 열고 승리 패널은 닫는다.
        private void ShowDefeatPanel()
        {
            winPanelUI.SetVisible(false);
            defeatPanelUI.SetVisible(true);
        }

        private void BindEndPanelButtons()
        {
            if (endButtonsBound)
            {
                return;
            }

            winPanelUI.BindReturn(ReturnToMainMenu);
            defeatPanelUI.BindReturn(ReturnToMainMenu);

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

    }

    /// StartContext 처리에 필요한 불변 실행 문맥을 전달한다.
    public static class StartContext
    {
        public static string SelectedMonsterName { get; private set; }
        public static RunMode Mode { get; private set; } = RunMode.Normal;

        public static void Prepare(string selectedMonsterName, RunMode mode = RunMode.Normal)
        {
            SelectedMonsterName = string.IsNullOrWhiteSpace(selectedMonsterName) ? string.Empty : selectedMonsterName;
            Mode = mode;
        }

        public static void Clear()
        {
            SelectedMonsterName = string.Empty;
            Mode = RunMode.Normal;
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

}
