// 'System' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System;
// 'System.Collections' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections;
// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'System.Globalization' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Globalization;
// 'Pakuri.Run' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Run;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;
// 'UnityEngine.SceneManagement' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine.SceneManagement;
// 'UnityEngine.UI' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine.UI;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // CSV 기반 스테이지 일차 흐름을 실행하고 적 웨이브, 보상, 넥서스 승패, 날짜 전환을 관리한다.
    // [낯선 문법] DisallowMultipleComponent attribute: 같은 GameObject에 이 컴포넌트가 중복 부착되는 것을 막는다.
    [DisallowMultipleComponent]
    // [방어 로직][낯선 문법] RequireComponent attribute: 'SceneEntryManager' 의존 컴포넌트가 같은 GameObject에 있도록 Unity에 요구한다.
    [RequireComponent(typeof(SceneEntryManager))]
    // [방어 로직][낯선 문법] RequireComponent attribute: 'InGameCombatManager' 의존 컴포넌트가 같은 GameObject에 있도록 Unity에 요구한다.
    [RequireComponent(typeof(InGameCombatManager))]
    // 'StageManager' 클래스 정의를 시작한다.
    public class StageManager : MonoBehaviour
    {
        // 'DefaultClearCheckInterval' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const float DefaultClearCheckInterval = 0.25f;

        // [낯선 문법] readonly 필드 'activeEncounterRows'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly List<StageEncounterRow> activeEncounterRows = new List<StageEncounterRow>();
        // [낯선 문법] readonly 필드 'pendingPrisonerEnemyIds'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly List<string> pendingPrisonerEnemyIds = new List<string>();
        // [낯선 문법] readonly 필드 'prisonerCandidatePool'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly List<string> prisonerCandidatePool = new List<string>();

        // [낯선 문법] SerializeField attribute: private 상태 'entryManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private SceneEntryManager entryManager;
        // [낯선 문법] SerializeField attribute: private 상태 'combatManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private InGameCombatManager combatManager;
        // [낯선 문법] SerializeField attribute: private 상태 'stageDayCsv'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private TextAsset stageDayCsv;
        // [낯선 문법] SerializeField attribute: private 상태 'stageEncounterCsv'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private TextAsset stageEncounterCsv;
        // [낯선 문법] SerializeField attribute: private 상태 'stageRewardCsv'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private TextAsset stageRewardCsv;
        // [낯선 문법] SerializeField attribute: private 상태 'startFlowOnStart'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool startFlowOnStart = true;
        // [낯선 문법] SerializeField attribute: private 상태 'clearCheckInterval'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private float clearCheckInterval = DefaultClearCheckInterval;
        // [낯선 문법] SerializeField attribute: private 상태 'restorePlayerHealthOnDayAdvance'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool restorePlayerHealthOnDayAdvance = true;
        // [낯선 문법] SerializeField attribute: private 상태 'nexusActor'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private NexusUnitActor nexusActor;
        // [낯선 문법] SerializeField attribute: private 상태 'winPanel'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject winPanel;
        // [낯선 문법] SerializeField attribute: private 상태 'defeatPanel'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private GameObject defeatPanel;
        // [낯선 문법] SerializeField attribute: private 상태 'winButton'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Button winButton;
        // [낯선 문법] SerializeField attribute: private 상태 'defeatButton'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Button defeatButton;
        // [낯선 문법] SerializeField attribute: private 상태 'mainMenuScenePath'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private string mainMenuScenePath = "Assets/Scenes/NewScene/NewMainMenu.unity";
        // [낯선 문법] SerializeField attribute: private 상태 'winStageIndex'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private int winStageIndex = 2;
        // [낯선 문법] SerializeField attribute: private 상태 'winDayIndex'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private int winDayIndex = 11;

        // 'table' 필드를 선언하고 새 객체 또는 호출 결과로 초기화한다.
        private StageFlowTable table = new StageFlowTable();
        // 'flowCoroutine' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private Coroutine flowCoroutine;
        // 'currentDay' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private StageDayRow currentDay;
        // 'currentReward' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private StageRewardRow currentReward;
        // 'activeSession' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private RunSession activeSession;
        // 'endButtonsBound' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private bool endButtonsBound;
        // 'hasPreservedNexusHealth' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private bool hasPreservedNexusHealth;
        // 'preservedNexusHealth' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private float preservedNexusHealth;

        // 'State' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public StageState State { get; private set; } = StageState.NotStarted;
        // [낯선 문법] 식 본문 property: 'CurrentStage' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int CurrentStage => activeSession != null ? activeSession.StageIndex : 1;
        // [낯선 문법] 식 본문 property: 'CurrentDay' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int CurrentDay => activeSession != null ? activeSession.DayIndex : 1;
        // [낯선 문법] 식 본문 property: 'PendingPrisonerEnemyIds' 값을 오른쪽 식 하나로 계산해 반환한다.
        public IReadOnlyList<string> PendingPrisonerEnemyIds => pendingPrisonerEnemyIds;
        // 'PendingGoldReward' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public int PendingGoldReward { get; private set; }
        // 'PendingDarkTraceReward' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public int PendingDarkTraceReward { get; private set; }
        // 'PendingPrisonerCount' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public int PendingPrisonerCount { get; private set; }
        // [낯선 문법] 식 본문 property: 'PendingManifestSuccessChance' 값을 오른쪽 식 하나로 계산해 반환한다.
        public float PendingManifestSuccessChance => currentReward != null ? currentReward.ManifestSuccessChance : 0.7f;
        // [낯선 문법] 식 본문 property: 'ActiveSession' 값을 오른쪽 식 하나로 계산해 반환한다.
        public RunSession ActiveSession => activeSession;
        // 스테이지 진행에 필요한 컴포넌트와 승패 UI를 확보하고 버튼 이벤트를 연결한다.
        // 'Awake' 메소드의 입력과 반환 계약을 선언한다.
        private void Awake()
        {
            // 'ResolveEndFlowReferences' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ResolveEndFlowReferences();
            // 'HideEndPanels' 메소드를 호출해 현재 단계의 처리를 실행한다.
            HideEndPanels();
            // 'BindEndButtons' 메소드를 호출해 현재 단계의 처리를 실행한다.
            BindEndButtons();
        }

        // 스테이지 CSV를 로드하고 설정에 따라 현재 날짜 전투 흐름을 자동 시작한다.
        // 'Start' 메소드의 입력과 반환 계약을 선언한다.
        private void Start()
        {
            // 'ResolveEndFlowReferences' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ResolveEndFlowReferences();
            // 'HideEndPanels' 메소드를 호출해 현재 단계의 처리를 실행한다.
            HideEndPanels();
            // 'BindEndButtons' 메소드를 호출해 현재 단계의 처리를 실행한다.
            BindEndButtons();
            // 'LoadTables' 메소드를 호출해 현재 단계의 처리를 실행한다.
            LoadTables();

            // 'startFlowOnStart' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (startFlowOnStart)
            {
                // 'StartCurrentDay' 메소드를 호출해 현재 단계의 처리를 실행한다.
                StartCurrentDay();
            }
        }

        // StageManager 제거 시 넥서스 패배 이벤트 구독을 해제한다.
        // 'OnDestroy' 메소드의 입력과 반환 계약을 선언한다.
        private void OnDestroy()
        {
            // [방어 로직] 'nexusActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (nexusActor != null)
            {
                // 'nexusActor.Defeated -= OnNexusDefeated;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                nexusActor.Defeated -= OnNexusDefeated;
            }
        }

        // 실행 중인 흐름을 교체하고 피해량 측정기를 초기화한 뒤 현재 날짜 코루틴을 시작한다.
        // 'StartCurrentDay' 메소드의 입력과 반환 계약을 선언한다.
        public void StartCurrentDay()
        {
            // [방어 로직] 'flowCoroutine != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (flowCoroutine != null)
            {
                // [방어 로직] 이미 실행 중인 Unity coroutine을 중단해 중복 흐름을 막는다.
                StopCoroutine(flowCoroutine);
            }

            // [방어 로직] 'DamageMeterRuntimeTracker.Active != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (DamageMeterRuntimeTracker.Active != null)
            {
                // 'DamageMeterRuntimeTracker.Active.ResetMeter' 메소드를 호출해 해당 객체의 처리를 실행한다.
                DamageMeterRuntimeTracker.Active.ResetMeter();
            }

            // 'flowCoroutine'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            flowCoroutine = StartCoroutine(RunCurrentDayFlow());
        }

        // 보상 상태를 정리하고 넥서스·파티 상태를 보존한 뒤 세션 날짜를 증가시킨다.
        // 'ContinueToNextDay' 메소드의 입력과 반환 계약을 선언한다.
        public void ContinueToNextDay()
        {
            // 'State != StageState.RewardReady' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (State != StageState.RewardReady)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "StageManager cannot continue because reward state is not ready.".
                Debug.LogWarning("StageManager cannot continue because reward state is not ready.");
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'activeSession == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeSession == null)
            {
                // [방어 로직] 계속 실행할 수 있지만 잘못된 설정을 경고 로그로 남긴다: "StageManager cannot continue because no active run session exists.".
                Debug.LogWarning("StageManager cannot continue because no active run session exists.");
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            pendingPrisonerEnemyIds.Clear();
            // 'PendingGoldReward'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PendingGoldReward = 0;
            // 'PendingDarkTraceReward'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PendingDarkTraceReward = 0;
            // 'PendingPrisonerCount'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PendingPrisonerCount = 0;
            // 'PreserveCurrentNexusHealth' 메소드를 호출해 현재 단계의 처리를 실행한다.
            PreserveCurrentNexusHealth();
            // [방어 로직] 'combatManager != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager != null)
            {
                // 날짜 전환 의미는 StageManager가 소유하고 전투 상태 초기화만 요청한다.
                combatManager.ResetCombatState();
            }

            // 'activeSession.AdvanceDay' 메소드를 호출해 해당 객체의 처리를 실행한다.
            activeSession.AdvanceDay();
            // 'RestorePlayerHealthForNextDay' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RestorePlayerHealthForNextDay();
            // 'StartCurrentDay' 메소드를 호출해 현재 단계의 처리를 실행한다.
            StartCurrentDay();
        }

        // 날짜 전환 후 파티를 복원하고 모든 플레이어 몬스터 체력을 최대치로 회복한다.
        // 'RestorePlayerHealthForNextDay' 메소드의 입력과 반환 계약을 선언한다.
        private void RestorePlayerHealthForNextDay()
        {
            // '!restorePlayerHealthOnDayAdvance' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!restorePlayerHealthOnDayAdvance)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'entryManager != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entryManager != null)
            {
                // 'entryManager.RestorePlayerPartyFromSession' 메소드를 호출해 해당 객체의 처리를 실행한다.
                entryManager.RestorePlayerPartyFromSession();
            }

            // [방어 로직] 'combatManager == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var players = combatManager.Roster != null ? combatManager.Roster.Players : null;
            // [방어 로직] 'players == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (players == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var entry = players[i];
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var model = entry != null ? entry.Model : null;
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var identity = model != null ? model.Identity : null;
                // [방어 로직] 'identity == null || identity.Role != UnitRole.Monster' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (identity == null || identity.Role != UnitRole.Monster)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var resources = model != null ? model.Resources : null;
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var stats = model != null ? model.Stats : null;
                // [방어 로직] 'resources == null || stats == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (resources == null || stats == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                resources.CurrentHealth = Mathf.Max(0f, stats.MaxHealth);
                // 로스터에 등록된 Actor 표시를 갱신한다.
                combatManager.Roster.RefreshActor(model);
            }
        }

        // 현재 세션의 일차·인카운터·보상 행을 찾아 생성, 전투 대기, 승리 또는 보상 준비를 순서대로 실행한다.
        // 'RunCurrentDayFlow' 메소드의 입력과 반환 계약을 선언한다.
        private IEnumerator RunCurrentDayFlow()
        {
            // [방어 로직] 'entryManager != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entryManager != null)
            {
                // 'entryManager.SpawnSelectedPlayerUnit' 메소드를 호출해 해당 객체의 처리를 실행한다.
                entryManager.SpawnSelectedPlayerUnit();
            }

            // 'EnsureNexusRegistered' 메소드를 호출해 현재 단계의 처리를 실행한다.
            EnsureNexusRegistered();

            // 'activeSession'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            activeSession = entryManager != null ? entryManager.ActiveSession : null;
            // [방어 로직] 'activeSession == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeSession == null)
            {
                // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                State = StageState.Error;
                // 실행을 막는 오류 상태를 로그로 남긴다: "StageManager could not start because SceneEntryManager has no active session.".
                Debug.LogError("StageManager could not start because SceneEntryManager has no active session.");
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 'currentDay'에 오른쪽 계산 또는 조회 결과를 저장한다.
            currentDay = table.FindDay(activeSession.StageIndex, activeSession.DayIndex);
            // [방어 로직] 'currentDay == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (currentDay == null)
            {
                // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                State = StageState.Error;
                // 실행을 막는 오류 상태를 로그로 남긴다: $"StageManager has no StageDay row for stage {activeSession.StageIndex}, day {activeSession.DayIndex}.".
                Debug.LogError($"StageManager has no StageDay row for stage {activeSession.StageIndex}, day {activeSession.DayIndex}.");
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 'currentReward'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            currentReward = table.FindReward(currentDay.RewardRuleId);
            // [방어 로직] 'currentReward == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (currentReward == null)
            {
                // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                State = StageState.Error;
                // 실행을 막는 오류 상태를 로그로 남긴다: $"StageManager has no StageReward row for '{currentDay.RewardRuleId}'.".
                Debug.LogError($"StageManager has no StageReward row for '{currentDay.RewardRuleId}'.");
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 'table.FindEncounterRows' 메소드를 호출해 해당 객체의 처리를 실행한다.
            table.FindEncounterRows(currentDay.EncounterId, activeEncounterRows);
            // [방어 로직] 'activeEncounterRows.Count == 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeEncounterRows.Count == 0)
            {
                // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                State = StageState.Error;
                // 실행을 막는 오류 상태를 로그로 남긴다: $"StageManager has no StageEncounter rows for '{currentDay.EncounterId}'.".
                Debug.LogError($"StageManager has no StageEncounter rows for '{currentDay.EncounterId}'.");
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 'SelectBossRows' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SelectBossRows();
            // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            State = StageState.Spawning;
            // [낯선 문법] coroutine을 유지한 채 'SpawnEncounterRows(activeEncounterRows)' 작업이 끝날 때까지 실행을 양보한다.
            yield return SpawnEncounterRows(activeEncounterRows);

            // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            State = StageState.Combat;
            // [낯선 문법] coroutine을 유지한 채 'WaitForEnemyClear()' 작업이 끝날 때까지 실행을 양보한다.
            yield return WaitForEnemyClear();

            // 'IsConfiguredWinDay()' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (IsConfiguredWinDay())
            {
                // 'ShowWinPanel' 메소드를 호출해 현재 단계의 처리를 실행한다.
                ShowWinPanel();
                // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                State = StageState.Victory;
                // 'flowCoroutine'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                flowCoroutine = null;
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 'PrepareReward' 메소드를 호출해 현재 단계의 처리를 실행한다.
            PrepareReward();
            // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            State = StageState.RewardReady;
            // 'flowCoroutine'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            flowCoroutine = null;
        }

        // 인카운터 행의 수량과 간격에 따라 일반 적 또는 보스를 순서대로 생성한다.
        // 'SpawnEncounterRows' 메소드의 입력과 반환 계약을 선언한다.
        private IEnumerator SpawnEncounterRows(IReadOnlyList<StageEncounterRow> rows)
        {
            // 지역 변수 'spawnIndex'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var spawnIndex = 0;
            // 'var i = 0; i < rows.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < rows.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = rows[i];
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var count = Mathf.Max(0, row.Count);
                // 'var j = 0; j < count; j++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
                for (var j = 0; j < count; j++)
                {
                    // [방어 로직] 'entryManager != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                    if (entryManager != null)
                    {
                        // 지역 변수 'isBoss'에 오른쪽 계산 또는 조회 결과를 저장한다.
                        var isBoss = IsBossEncounter(row);
                        // 지역 변수 'healthMultiplier'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                        var healthMultiplier = isBoss
                            // [낯선 문법] 삼항 연산자의 조건 참 결과로 'UnityEngine.Random.Range(row.BossHealthMultiplierMin, row.BossHealthMultiplierMax)' 값을 선택한다.
                            ? UnityEngine.Random.Range(row.BossHealthMultiplierMin, row.BossHealthMultiplierMax)
                            // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 '1f;' 값을 선택한다.
                            : 1f;
                        // 'entryManager.SpawnEnemyById' 메소드를 호출해 해당 객체의 처리를 실행한다.
                        entryManager.SpawnEnemyById(
                            // 'row.EnemyId' 값을 현재 메소드 호출의 인수로 전달한다.
                            row.EnemyId,
                            // 'spawnIndex' 열거값을 선택 가능한 상수 항목으로 정의한다.
                            spawnIndex,
                            // 'row.SpawnX' 값을 현재 메소드 호출의 인수로 전달한다.
                            row.SpawnX,
                            // 'row.SpawnYMin' 값을 현재 메소드 호출의 인수로 전달한다.
                            row.SpawnYMin,
                            // 'row.SpawnYMax' 값을 현재 메소드 호출의 인수로 전달한다.
                            row.SpawnYMax,
                            // 'healthMultiplier' 열거값을 선택 가능한 상수 항목으로 정의한다.
                            healthMultiplier,
                            // 'isBoss' 열거값을 선택 가능한 상수 항목으로 정의한다.
                            isBoss,
                            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                            out _);
                    }

                    // 'spawnIndex' 값에 '1' 결과를 누적한다.
                    spawnIndex += 1;
                    // [낯선 문법] coroutine을 유지한 채 'new WaitForSeconds(Mathf.Max(0f, row.IntervalSeconds))' 작업이 끝날 때까지 실행을 양보한다.
                    yield return new WaitForSeconds(Mathf.Max(0f, row.IntervalSeconds));
                }
            }
        }

        // 패배하지 않은 동안 활성 적 수가 0이 될 때까지 설정 간격으로 대기한다.
        // 'WaitForEnemyClear' 메소드의 입력과 반환 계약을 선언한다.
        private IEnumerator WaitForEnemyClear()
        {
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var wait = new WaitForSeconds(Mathf.Max(0.05f, clearCheckInterval));
            // 'combatManager != null && combatManager.ActiveEnemyCount > 0 && State != StageState.Defeat' 조건이 참인 동안 반복 실행한다.
            while (combatManager != null && combatManager.ActiveEnemyCount > 0 && State != StageState.Defeat)
            {
                // [낯선 문법] coroutine을 유지한 채 'wait' 작업이 끝날 때까지 실행을 양보한다.
                yield return wait;
            }
        }

        // 현재 보상 규칙으로 재화와 포로 수를 계산하고 보장·무작위 포로 목록을 구성한다.
        // 'PrepareReward' 메소드의 입력과 반환 계약을 선언한다.
        private void PrepareReward()
        {
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            pendingPrisonerEnemyIds.Clear();
            // 'PendingGoldReward'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PendingGoldReward = currentReward != null ? currentReward.Gold : 0;
            // 'PendingDarkTraceReward'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PendingDarkTraceReward = currentReward != null ? currentReward.DarkTrace : 0;
            // 'PendingPrisonerCount'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PendingPrisonerCount = currentReward != null ? currentReward.RollPrisonerCount() : 0;

            // 'AddGuaranteedPrisoners' 메소드를 호출해 현재 단계의 처리를 실행한다.
            AddGuaranteedPrisoners();
            // 'BuildPrisonerCandidatePool' 메소드를 호출해 현재 단계의 처리를 실행한다.
            BuildPrisonerCandidatePool();
            // 'AddCandidatePrisonersUntilFull' 메소드를 호출해 현재 단계의 처리를 실행한다.
            AddCandidatePrisonersUntilFull();

            // 지역 변수 'prisonerSummary'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var prisonerSummary = string.Join("|", pendingPrisonerEnemyIds);
            // 'Debug.Log' 메소드를 호출해 해당 객체의 처리를 실행한다.
            Debug.Log(
                // [낯선 문법] 문자열 보간($"...")으로 런타임 값을 문장 안에 넣어 메시지를 구성한다.
                $"Stage reward ready: stage={activeSession.StageIndex}, day={activeSession.DayIndex}, " +
                // [낯선 문법] 문자열 보간($"...")으로 런타임 값을 문장 안에 넣어 메시지를 구성한다.
                $"gold={PendingGoldReward}, darkTrace={PendingDarkTraceReward}, prisoners={prisonerSummary}");
        }

        // 보장 포로 또는 선택된 보스 인카운터의 적 ID를 포로 목록에 추가한다.
        // 'AddGuaranteedPrisoners' 메소드의 입력과 반환 계약을 선언한다.
        private void AddGuaranteedPrisoners()
        {
            // 'var i = 0; i < activeEncounterRows.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = activeEncounterRows[i];
                // 'row.GuaranteedPrisoner || row.SelectedAsBoss' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (row.GuaranteedPrisoner || row.SelectedAsBoss)
                {
                    // 'AddPrisoner' 메소드를 호출해 현재 단계의 처리를 실행한다.
                    AddPrisoner(row.EnemyId);
                }
            }
        }

        // 실제 생성 수량을 반영한 포로 후보 풀을 만들고 이미 보장된 포로 한 개씩을 제외한다.
        // 'BuildPrisonerCandidatePool' 메소드의 입력과 반환 계약을 선언한다.
        private void BuildPrisonerCandidatePool()
        {
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            prisonerCandidatePool.Clear();
            // 'var i = 0; i < activeEncounterRows.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = activeEncounterRows[i];
                // 'var count = 0; count < row.Count; count++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
                for (var count = 0; count < row.Count; count++)
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    prisonerCandidatePool.Add(row.EnemyId);
                }
            }

            // 'var i = 0; i < pendingPrisonerEnemyIds.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < pendingPrisonerEnemyIds.Count; i++)
            {
                // 'RemoveOnePrisonerCandidate' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RemoveOnePrisonerCandidate(pendingPrisonerEnemyIds[i]);
            }
        }

        // 보상 포로 수에 도달할 때까지 후보 풀에서 무작위 적을 뽑는다.
        // 'AddCandidatePrisonersUntilFull' 메소드의 입력과 반환 계약을 선언한다.
        private void AddCandidatePrisonersUntilFull()
        {
            // [방어 로직] 'PendingPrisonerCount <= 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (PendingPrisonerCount <= 0)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'pendingPrisonerEnemyIds.Count < PendingPrisonerCount && prisonerCandidatePool.Count > 0' 조건이 참인 동안 반복 실행한다.
            while (pendingPrisonerEnemyIds.Count < PendingPrisonerCount && prisonerCandidatePool.Count > 0)
            {
                // 지역 변수 'poolIndex'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var poolIndex = UnityEngine.Random.Range(0, prisonerCandidatePool.Count);
                // 'AddPrisoner' 메소드를 호출해 현재 단계의 처리를 실행한다.
                AddPrisoner(prisonerCandidatePool[poolIndex]);
                // 'prisonerCandidatePool.RemoveAt' 메소드를 호출해 해당 객체의 처리를 실행한다.
                prisonerCandidatePool.RemoveAt(poolIndex);
            }
        }

        // 유효한 적 ID를 지급 대기 포로 목록에 추가한다.
        // 'AddPrisoner' 메소드의 입력과 반환 계약을 선언한다.
        private void AddPrisoner(string enemyId)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(enemyId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
            pendingPrisonerEnemyIds.Add(enemyId);
        }

        // 후보 풀에서 지정 적 ID와 일치하는 항목 하나를 제거한다.
        // 'RemoveOnePrisonerCandidate' 메소드의 입력과 반환 계약을 선언한다.
        private void RemoveOnePrisonerCandidate(string enemyId)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(enemyId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(enemyId))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'var i = 0; i < prisonerCandidatePool.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < prisonerCandidatePool.Count; i++)
            {
                // 'string.Equals(prisonerCandidatePool[i], enemyId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (string.Equals(prisonerCandidatePool[i], enemyId, StringComparison.OrdinalIgnoreCase))
                {
                    // 'prisonerCandidatePool.RemoveAt' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    prisonerCandidatePool.RemoveAt(i);
                    // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                    return;
                }
            }
        }

        // 보장 보스를 표시하고 일반 보스 후보가 있으면 그중 하나를 무작위 선택한다.
        // 'SelectBossRows' 메소드의 입력과 반환 계약을 선언한다.
        private void SelectBossRows()
        {
            // 지역 변수 'normalBossCandidates'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var normalBossCandidates = new List<StageEncounterRow>();
            // 'var i = 0; i < activeEncounterRows.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < activeEncounterRows.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = activeEncounterRows[i];
                // 'row.SelectedAsBoss'에 오른쪽 계산 또는 조회 결과를 저장한다.
                row.SelectedAsBoss = false;
                // 'row.IsGuaranteedBoss' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (row.IsGuaranteedBoss)
                {
                    // 'row.SelectedAsBoss'에 오른쪽 계산 또는 조회 결과를 저장한다.
                    row.SelectedAsBoss = true;
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'row.IsBossCandidate' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (row.IsBossCandidate)
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    normalBossCandidates.Add(row);
                }
            }

            // [방어 로직] 'normalBossCandidates.Count == 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (normalBossCandidates.Count == 0)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'normalBossCandidates[UnityEngine.Random.Range(0, normalBossCandidates.Count)].SelectedAsBoss = true;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            normalBossCandidates[UnityEngine.Random.Range(0, normalBossCandidates.Count)].SelectedAsBoss = true;
        }

        // 선택·보장 상태와 현재 런 전투 유형을 기준으로 인카운터가 보스인지 판별한다.
        // 'IsBossEncounter' 메소드의 입력과 반환 계약을 선언한다.
        private bool IsBossEncounter(StageEncounterRow row)
        {
            // [방어 로직] 'row == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (row == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 'row.SelectedAsBoss' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (row.SelectedAsBoss)
            {
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // [방어 로직] 'activeSession == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeSession == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'isMidbossCombat'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var isMidbossCombat = activeSession.CurrentCombatType == RunCombatType.Day5Midboss
                // [방어 로직] 앞 조건과 OR로 'activeSession.CurrentCombatType == RunCombatType.Day10Midboss;' 조건을 추가한다.
                || activeSession.CurrentCombatType == RunCombatType.Day10Midboss;
            // 계산 또는 조회 결과 'isMidbossCombat && (row.IsGuaranteedBoss || row.IsBossCandidate)'을 호출자에게 반환한다.
            return isMidbossCombat && (row.IsGuaranteedBoss || row.IsBossCandidate);
        }

        // 직렬화되지 않은 승리·패배 버튼을 각 패널 자식에서 확보한다.
        // 'ResolveEndFlowReferences' 메소드의 입력과 반환 계약을 선언한다.
        private void ResolveEndFlowReferences()
        {
            // [방어 로직] 'winButton == null && winPanel != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (winButton == null && winPanel != null)
            {
                // 'winButton'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                winButton = winPanel.GetComponentInChildren<Button>(true);
            }

            // [방어 로직] 'defeatButton == null && defeatPanel != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (defeatButton == null && defeatPanel != null)
            {
                // 'defeatButton'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                defeatButton = defeatPanel.GetComponentInChildren<Button>(true);
            }
        }

        // 넥서스 Actor를 초기화하고 패배 이벤트와 전투 로스터에 연결하며 보존 체력을 복원한다.
        // 'EnsureNexusRegistered' 메소드의 입력과 반환 계약을 선언한다.
        private void EnsureNexusRegistered()
        {
            // 'ResolveEndFlowReferences' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ResolveEndFlowReferences();
            // 'nexusActor.Defeated -= OnNexusDefeated;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            nexusActor.Defeated -= OnNexusDefeated;
            // 'nexusActor.Defeated += OnNexusDefeated;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            nexusActor.Defeated += OnNexusDefeated;
            // 'nexusActor.Initialize' 메소드를 호출해 해당 객체의 처리를 실행한다.
            nexusActor.Initialize();
            // 'RestorePreservedNexusHealth' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RestorePreservedNexusHealth();

            // [방어 로직] 'combatManager != null && nexusActor.Model != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (combatManager != null && nexusActor.Model != null)
            {
                // 'combatManager.RegisterNexus' 메소드를 호출해 해당 객체의 처리를 실행한다.
                combatManager.RegisterNexus(nexusActor.Model, nexusActor, nexusActor.transform);
            }
        }

        // 날짜 전환 전에 넥서스의 현재 체력을 임시 저장한다.
        // 'PreserveCurrentNexusHealth' 메소드의 입력과 반환 계약을 선언한다.
        private void PreserveCurrentNexusHealth()
        {
            // 'ResolveEndFlowReferences' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ResolveEndFlowReferences();
            // [방어 로직] 'nexusActor == null || !nexusActor.TryGetCurrentHealth(out var currentHealth)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (nexusActor == null || !nexusActor.TryGetCurrentHealth(out var currentHealth))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'preservedNexusHealth'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            preservedNexusHealth = currentHealth;
            // 'hasPreservedNexusHealth'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            hasPreservedNexusHealth = true;
        }

        // 새 날짜 넥서스 초기화 후 이전 날짜에서 저장한 체력을 적용한다.
        // 'RestorePreservedNexusHealth' 메소드의 입력과 반환 계약을 선언한다.
        private void RestorePreservedNexusHealth()
        {
            // [방어 로직] '!hasPreservedNexusHealth || nexusActor == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!hasPreservedNexusHealth || nexusActor == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'nexusActor.SetCurrentHealth' 메소드를 호출해 해당 객체의 처리를 실행한다.
            nexusActor.SetCurrentHealth(preservedNexusHealth);
        }

        // 넥서스 패배 시 진행 코루틴을 중단하고 스테이지를 패배 상태로 전환한다.
        // 'OnNexusDefeated' 메소드의 입력과 반환 계약을 선언한다.
        private void OnNexusDefeated(NexusUnitActor defeatedNexus)
        {
            // 'State == StageState.Victory' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (State == StageState.Victory)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'flowCoroutine != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (flowCoroutine != null)
            {
                // [방어 로직] 이미 실행 중인 Unity coroutine을 중단해 중복 흐름을 막는다.
                StopCoroutine(flowCoroutine);
                // 'flowCoroutine'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                flowCoroutine = null;
            }

            // 'State'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            State = StageState.Defeat;
            // 'ShowDefeatPanel' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ShowDefeatPanel();
        }

        // 승리와 패배 패널을 모두 숨긴다.
        // 'HideEndPanels' 메소드의 입력과 반환 계약을 선언한다.
        private void HideEndPanels()
        {
            // 'SetActive' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetActive(winPanel, false);
            // 'SetActive' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetActive(defeatPanel, false);
        }

        // 패배 패널을 숨기고 승리 패널을 표시한다.
        // 'ShowWinPanel' 메소드의 입력과 반환 계약을 선언한다.
        private void ShowWinPanel()
        {
            // 'SetActive' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetActive(defeatPanel, false);
            // 'SetActive' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetActive(winPanel, true);
        }

        // 승리 패널을 숨기고 패배 패널을 표시한다.
        // 'ShowDefeatPanel' 메소드의 입력과 반환 계약을 선언한다.
        private void ShowDefeatPanel()
        {
            // 'SetActive' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetActive(winPanel, false);
            // 'SetActive' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetActive(defeatPanel, true);
        }

        // 승리·패배 버튼을 메인 메뉴 복귀 함수에 한 번만 연결한다.
        // 'BindEndButtons' 메소드의 입력과 반환 계약을 선언한다.
        private void BindEndButtons()
        {
            // 'endButtonsBound' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (endButtonsBound)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] 'winButton != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (winButton != null)
            {
                // 'winButton.onClick.AddListener' 메소드를 호출해 해당 객체의 처리를 실행한다.
                winButton.onClick.AddListener(ReturnToMainMenu);
            }

            // [방어 로직] 'defeatButton != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (defeatButton != null)
            {
                // 'defeatButton.onClick.AddListener' 메소드를 호출해 해당 객체의 처리를 실행한다.
                defeatButton.onClick.AddListener(ReturnToMainMenu);
            }

            // 'endButtonsBound'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            endButtonsBound = true;
        }

        // 설정된 메인 메뉴 씬 경로를 로드한다.
        // 'ReturnToMainMenu' 메소드의 입력과 반환 계약을 선언한다.
        private void ReturnToMainMenu()
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(mainMenuScenePath)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(mainMenuScenePath))
            {
                // 실행을 막는 오류 상태를 로그로 남긴다: "StageManager cannot return to main menu because mainMenuScenePath is empty.".
                Debug.LogError("StageManager cannot return to main menu because mainMenuScenePath is empty.");
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SceneManager.LoadScene' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SceneManager.LoadScene(mainMenuScenePath);
        }

        // 현재 세션이 설정된 최종 스테이지와 날짜에 도달했는지 판별한다.
        // 'IsConfiguredWinDay' 메소드의 입력과 반환 계약을 선언한다.
        private bool IsConfiguredWinDay()
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return activeSession != null
                // 앞 조건과 AND로 'activeSession.StageIndex == winStageIndex' 조건을 추가한다.
                && activeSession.StageIndex == winStageIndex
                // 앞 조건과 AND로 'activeSession.DayIndex == winDayIndex;' 조건을 추가한다.
                && activeSession.DayIndex == winDayIndex;
        }

        // GameObject가 존재할 때만 활성 상태를 변경한다.
        // 'SetActive' 메소드의 입력과 반환 계약을 선언한다.
        private static void SetActive(GameObject target, bool active)
        {
            // [방어 로직] 'target != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target != null)
            {
                // 'target.SetActive' 메소드를 호출해 해당 객체의 처리를 실행한다.
                target.SetActive(active);
            }
        }

        // 직렬화된 세 CSV TextAsset을 스테이지 흐름 테이블로 파싱한다.
        // 'LoadTables' 메소드의 입력과 반환 계약을 선언한다.
        private void LoadTables()
        {
            // 'table'에 오른쪽 계산 또는 조회 결과를 저장한다.
            table = StageFlowTable.Load(stageDayCsv, stageEncounterCsv, stageRewardCsv);
        }
    }

    // 스테이지 일차 흐름의 시작 전부터 오류까지 가능한 진행 상태를 정의한다.
    // 'StageState' 열거형 정의를 시작한다.
    public enum StageState
    {
        // 'NotStarted' 열거값을 선택 가능한 상수 항목으로 정의한다.
        NotStarted,
        // 'Spawning' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Spawning,
        // 'Combat' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Combat,
        // 'RewardReady' 열거값을 선택 가능한 상수 항목으로 정의한다.
        RewardReady,
        // 'Victory' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Victory,
        // 'Defeat' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Defeat,
        // 'Error' 열거값을 선택 가능한 상수 항목으로 정의한다.
        Error
    }

    // 한 스테이지 날짜가 사용할 전투 유형, 인카운터, 보상 규칙을 저장한다.
    // 'StageDayRow' 클래스 정의를 시작한다.
    internal class StageDayRow
    {
        // 'Stage' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int Stage;
        // 'Day' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int Day;
        // 'EncounterId' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public string EncounterId;
        // 'RewardRuleId' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public string RewardRuleId;
    }

    // 한 적 생성 묶음의 수량, 간격, 위치, 보스·포로 규칙을 저장한다.
    // 'StageEncounterRow' 클래스 정의를 시작한다.
    internal class StageEncounterRow
    {
        // 'EncounterId' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public string EncounterId;
        // 'SpawnOrder' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int SpawnOrder;
        // 'EnemyId' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public string EnemyId;
        // 'Count' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int Count;
        // 'IntervalSeconds' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float IntervalSeconds;
        // 'SpawnX' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float SpawnX;
        // 'SpawnYMin' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float SpawnYMin;
        // 'SpawnYMax' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float SpawnYMax;
        // 'IsBossCandidate' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public bool IsBossCandidate;
        // 'IsGuaranteedBoss' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public bool IsGuaranteedBoss;
        // 'BossHealthMultiplierMin' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float BossHealthMultiplierMin;
        // 'BossHealthMultiplierMax' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float BossHealthMultiplierMax;
        // 'GuaranteedPrisoner' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public bool GuaranteedPrisoner;
        // 'SelectedAsBoss' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public bool SelectedAsBoss;
    }

    // 재화, 포로 수 확률, 현현 확률 등 한 보상 규칙을 저장한다.
    // 'StageRewardRow' 클래스 정의를 시작한다.
    internal class StageRewardRow
    {
        // 'RewardRuleId' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public string RewardRuleId;
        // 'Gold' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int Gold;
        // 'DarkTrace' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int DarkTrace;
        // 'PrisonerCount1Chance' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float PrisonerCount1Chance;
        // 'PrisonerCount2Chance' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float PrisonerCount2Chance;
        // 'ManifestSuccessChance' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public float ManifestSuccessChance;
        // 'EliteBonusPrisoners' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int EliteBonusPrisoners;

        // 포로 수 확률과 엘리트 추가 수량을 사용해 지급 포로 수를 추첨한다.
        // 'RollPrisonerCount' 메소드의 입력과 반환 계약을 선언한다.
        public int RollPrisonerCount()
        {
            // 지역 변수 'roll'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var roll = UnityEngine.Random.value;
            // 'roll < PrisonerCount1Chance' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (roll < PrisonerCount1Chance)
            {
                // 계산 또는 조회 결과 '1 + EliteBonusPrisoners'을 호출자에게 반환한다.
                return 1 + EliteBonusPrisoners;
            }

            // 'roll < PrisonerCount1Chance + PrisonerCount2Chance' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (roll < PrisonerCount1Chance + PrisonerCount2Chance)
            {
                // 계산 또는 조회 결과 '2 + EliteBonusPrisoners'을 호출자에게 반환한다.
                return 2 + EliteBonusPrisoners;
            }

            // 계산 또는 조회 결과 '3 + EliteBonusPrisoners'을 호출자에게 반환한다.
            return 3 + EliteBonusPrisoners;
        }
    }

    // 스테이지 날짜·인카운터·보상 CSV 행을 메모리에 보관하고 조회한다.
    // 'StageFlowTable' 클래스 정의를 시작한다.
    internal class StageFlowTable
    {
        // [낯선 문법] readonly 필드 'days'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly List<StageDayRow> days = new List<StageDayRow>();
        // [낯선 문법] readonly 필드 'encounters'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly List<StageEncounterRow> encounters = new List<StageEncounterRow>();
        // [낯선 문법] readonly 필드 'rewards'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly List<StageRewardRow> rewards = new List<StageRewardRow>();

        // 세 CSV TextAsset을 읽어 완성된 스테이지 흐름 테이블을 만든다.
        // 'Load' 메소드의 입력과 반환 계약을 선언한다.
        public static StageFlowTable Load(TextAsset dayCsv, TextAsset encounterCsv, TextAsset rewardCsv)
        {
            // 지역 변수 'table'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var table = new StageFlowTable();
            // 'table.LoadDays' 메소드를 호출해 해당 객체의 처리를 실행한다.
            table.LoadDays(dayCsv);
            // 'table.LoadEncounters' 메소드를 호출해 해당 객체의 처리를 실행한다.
            table.LoadEncounters(encounterCsv);
            // 'table.LoadRewards' 메소드를 호출해 해당 객체의 처리를 실행한다.
            table.LoadRewards(rewardCsv);
            // 계산 또는 조회 결과 'table'을 호출자에게 반환한다.
            return table;
        }

        // 스테이지와 날짜가 모두 일치하는 날짜 행을 찾는다.
        // 'FindDay' 메소드의 입력과 반환 계약을 선언한다.
        public StageDayRow FindDay(int stage, int day)
        {
            // 'var i = 0; i < days.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < days.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = days[i];
                // 'row.Stage == stage && row.Day == day' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (row.Stage == stage && row.Day == day)
                {
                    // 계산 또는 조회 결과 'row'을 호출자에게 반환한다.
                    return row;
                }
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 대소문자를 구분하지 않고 보상 규칙 ID에 해당하는 행을 찾는다.
        // 'FindReward' 메소드의 입력과 반환 계약을 선언한다.
        public StageRewardRow FindReward(string rewardRuleId)
        {
            // 'var i = 0; i < rewards.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < rewards.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = rewards[i];
                // 'string.Equals(row.RewardRuleId, rewardRuleId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (string.Equals(row.RewardRuleId, rewardRuleId, StringComparison.OrdinalIgnoreCase))
                {
                    // 계산 또는 조회 결과 'row'을 호출자에게 반환한다.
                    return row;
                }
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 인카운터 ID에 속한 모든 생성 행을 찾아 생성 순서로 정렬한다.
        // 'FindEncounterRows' 메소드의 입력과 반환 계약을 선언한다.
        public void FindEncounterRows(string encounterId, List<StageEncounterRow> results)
        {
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            results.Clear();
            // 'var i = 0; i < encounters.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < encounters.Count; i++)
            {
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = encounters[i];
                // 'string.Equals(row.EncounterId, encounterId, StringComparison.OrdinalIgnoreCase)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (string.Equals(row.EncounterId, encounterId, StringComparison.OrdinalIgnoreCase))
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    results.Add(row);
                }
            }

            // [낯선 문법] 람다 또는 식 본문 연산자(=>)로 짧은 실행 규칙을 정의한다.
            results.Sort((left, right) => left.SpawnOrder.CompareTo(right.SpawnOrder));
        }

        // 날짜 CSV의 각 행을 StageDayRow로 변환해 저장한다.
        // 'LoadDays' 메소드의 입력과 반환 계약을 선언한다.
        private void LoadDays(TextAsset csv)
        {
            // 'var row in ReadRows(csv)' 컬렉션의 각 항목을 순서대로 처리한다.
            foreach (var row in ReadRows(csv))
            {
                // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                days.Add(new StageDayRow
                {
                    // 'Stage'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Stage = ParseInt(row, "stage"),
                    // 'Day'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Day = ParseInt(row, "day"),
                    // 'EncounterId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    EncounterId = Read(row, "encounter_id"),
                    // 'RewardRuleId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    RewardRuleId = Read(row, "reward_rule_id")
                });
            }
        }

        // 인카운터 CSV의 각 행을 StageEncounterRow로 변환해 저장한다.
        // 'LoadEncounters' 메소드의 입력과 반환 계약을 선언한다.
        private void LoadEncounters(TextAsset csv)
        {
            // 'var row in ReadRows(csv)' 컬렉션의 각 항목을 순서대로 처리한다.
            foreach (var row in ReadRows(csv))
            {
                // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                encounters.Add(new StageEncounterRow
                {
                    // 'EncounterId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    EncounterId = Read(row, "encounter_id"),
                    // 'SpawnOrder'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    SpawnOrder = ParseInt(row, "spawn_order"),
                    // 'EnemyId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    EnemyId = Read(row, "enemy_id"),
                    // 'Count'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Count = ParseInt(row, "count"),
                    // 'IntervalSeconds'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    IntervalSeconds = ParseFloat(row, "interval_sec"),
                    // 'SpawnX'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    SpawnX = ParseFloat(row, "spawn_x"),
                    // 'SpawnYMin'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    SpawnYMin = ParseFloat(row, "spawn_y_min"),
                    // 'SpawnYMax'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    SpawnYMax = ParseFloat(row, "spawn_y_max"),
                    // 'IsBossCandidate'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    IsBossCandidate = ParseBool(row, "is_boss_candidate"),
                    // 'IsGuaranteedBoss'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    IsGuaranteedBoss = ParseBool(row, "is_guaranteed_boss"),
                    // 'BossHealthMultiplierMin'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    BossHealthMultiplierMin = ParseFloat(row, "boss_health_multiplier_min"),
                    // 'BossHealthMultiplierMax'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    BossHealthMultiplierMax = ParseFloat(row, "boss_health_multiplier_max"),
                    // 'GuaranteedPrisoner'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    GuaranteedPrisoner = ParseBool(row, "guaranteed_prisoner")
                });
            }
        }

        // 보상 CSV의 각 행을 StageRewardRow로 변환해 저장한다.
        // 'LoadRewards' 메소드의 입력과 반환 계약을 선언한다.
        private void LoadRewards(TextAsset csv)
        {
            // 'var row in ReadRows(csv)' 컬렉션의 각 항목을 순서대로 처리한다.
            foreach (var row in ReadRows(csv))
            {
                // 세 번째 포로 확률은 나머지 구간이지만 CSV 숫자 형식 검증은 유지한다.
                _ = ParseFloat(row, "prisoner_count_3_chance");
                // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                rewards.Add(new StageRewardRow
                {
                    // 'RewardRuleId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    RewardRuleId = Read(row, "reward_rule_id"),
                    // 'Gold'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    Gold = ParseInt(row, "gold"),
                    // 'DarkTrace'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    DarkTrace = ParseInt(row, "dark_trace"),
                    // 'PrisonerCount1Chance'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    PrisonerCount1Chance = ParseFloat(row, "prisoner_count_1_chance"),
                    // 'PrisonerCount2Chance'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    PrisonerCount2Chance = ParseFloat(row, "prisoner_count_2_chance"),
                    // 'ManifestSuccessChance'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    ManifestSuccessChance = ParseFloat(row, "manifest_success_chance"),
                    // 'EliteBonusPrisoners'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                    EliteBonusPrisoners = ParseInt(row, "elite_bonus_prisoners")
                });
            }
        }

        // CSV 헤더와 데이터 줄을 열 이름 기반 사전 행으로 변환해 순회 제공한다.
        // 'ReadRows' 메소드의 입력과 반환 계약을 선언한다.
        private static IEnumerable<Dictionary<string, string>> ReadRows(TextAsset csv)
        {
            // [방어 로직] 'csv == null || string.IsNullOrWhiteSpace(csv.text)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (csv == null || string.IsNullOrWhiteSpace(csv.text))
            {
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 지역 변수 'lines'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var lines = csv.text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            // [방어 로직] 'lines.Length == 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (lines.Length == 0)
            {
                // [낯선 문법] coroutine을 즉시 종료하고 더 이상 다음 단계로 진행하지 않는다.
                yield break;
            }

            // 지역 변수 'headers'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var headers = SplitCsvLine(lines[0]);
            // 'var i = 1; i < lines.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 1; i < lines.Length; i++)
            {
                // [방어 로직] 'string.IsNullOrWhiteSpace(lines[i])' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (string.IsNullOrWhiteSpace(lines[i]))
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'values'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var values = SplitCsvLine(lines[i]);
                // 지역 변수 'row'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                // 'var j = 0; j < headers.Count; j++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
                for (var j = 0; j < headers.Count; j++)
                {
                    // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                    row[headers[j]] = j < values.Count ? values[j] : string.Empty;
                }

                // [낯선 문법] coroutine을 유지한 채 'row' 작업이 끝날 때까지 실행을 양보한다.
                yield return row;
            }
        }

        // 따옴표와 이중 따옴표 이스케이프를 처리하며 CSV 한 줄을 필드 목록으로 분리한다.
        // 'SplitCsvLine' 메소드의 입력과 반환 계약을 선언한다.
        private static List<string> SplitCsvLine(string line)
        {
            // 지역 변수 'values'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var values = new List<string>();
            // 지역 변수 'current'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var current = string.Empty;
            // 지역 변수 'inQuotes'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var inQuotes = false;

            // 'var i = 0; i < line.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < line.Length; i++)
            {
                // 지역 변수 'c'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var c = line[i];
                // 'c == '"'' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (c == '"')
                {
                    // 'inQuotes && i + 1 < line.Length && line[i + 1] == '"'' 조건이 참인지 검사해 실행 분기를 결정한다.
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // 'current' 값에 ''"'' 결과를 누적한다.
                        current += '"';
                        // 'i' 값에 '1' 결과를 누적한다.
                        i += 1;
                    }
                    // 'else' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    else
                    {
                        // 'inQuotes'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                        inQuotes = !inQuotes;
                    }
                }
                // 앞 조건이 거짓이고 'c == ',' && !inQuotes' 조건이 참일 때 실행할 분기를 선택한다.
                else if (c == ',' && !inQuotes)
                {
                    // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                    values.Add(current);
                    // 'current'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                    current = string.Empty;
                }
                // 'else' 열거값을 선택 가능한 상수 항목으로 정의한다.
                else
                {
                    // 'current' 값에 'c' 결과를 누적한다.
                    current += c;
                }
            }

            // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
            values.Add(current);
            // 계산 또는 조회 결과 'values'을 호출자에게 반환한다.
            return values;
        }

        // CSV 행에서 지정 열 문자열을 읽고 앞뒤 공백을 제거한다.
        // 'Read' 메소드의 입력과 반환 계약을 선언한다.
        private static string Read(Dictionary<string, string> row, string key)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건 결과에 맞는 값 하나를 반환한다.
            return row.TryGetValue(key, out var value) ? value.Trim() : string.Empty;
        }

        // CSV 열을 고정 문화권 정수로 변환하며 잘못된 값은 예외로 알린다.
        // 'ParseInt' 메소드의 입력과 반환 계약을 선언한다.
        private static int ParseInt(Dictionary<string, string> row, string key)
        {
            // 고정 문화권으로 파싱한 정수를 반환하고 실패하면 FormatException을 발생시킨다.
            return int.Parse(Read(row, key), NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        // CSV 열을 고정 문화권 실수로 변환하며 잘못된 값은 예외로 알린다.
        // 'ParseFloat' 메소드의 입력과 반환 계약을 선언한다.
        private static float ParseFloat(Dictionary<string, string> row, string key)
        {
            // 고정 문화권으로 파싱한 실수를 반환하고 실패하면 FormatException을 발생시킨다.
            return float.Parse(Read(row, key), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        // CSV 열이 true로 파싱되는지 확인해 불리언 값을 반환한다.
        // 'ParseBool' 메소드의 입력과 반환 계약을 선언한다.
        private static bool ParseBool(Dictionary<string, string> row, string key)
        {
            // 계산 또는 조회 결과 'bool.TryParse(Read(row, key), out var value) && value'을 호출자에게 반환한다.
            return bool.TryParse(Read(row, key), out var value) && value;
        }
    }
}
