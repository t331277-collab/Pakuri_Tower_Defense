// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'Pakuri.Combat' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Combat;
// 'Pakuri.Data' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Data;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 로스터의 적 유닛을 매 프레임 갱신해 이동, 대상 선택, 스킬 사용, 넥서스 공격을 처리한다.
    // 'EnemyCombatSystem' 클래스 정의를 시작한다.
    public class EnemyCombatSystem
    {
        // [낯선 문법] readonly 필드 'enemyStates'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly Dictionary<string, EnemyCombatState> enemyStates = new Dictionary<string, EnemyCombatState>();

        // 'LastAttackAttemptCount' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public int LastAttackAttemptCount { get; private set; }

        // 적별 전투 상태와 마지막 공격 시도 횟수를 초기화한다.
        // 'Clear' 메소드의 입력과 반환 계약을 선언한다.
        public void Clear()
        {
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            enemyStates.Clear();
            // 'LastAttackAttemptCount'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            LastAttackAttemptCount = 0;
        }

        // 전투 관리자 없이 적 전투 갱신을 실행하는 간편 호출이다.
        // 'Tick' 메소드의 입력과 반환 계약을 선언한다.
        public void Tick(UnitRosterService roster, float deltaTime, bool logAttackAttempts)
        {
            // 'Tick' 메소드를 호출해 현재 단계의 처리를 실행한다.
            Tick(roster, null, deltaTime, logAttackAttempts);
        }

        // 모든 적을 순회하며 전투 관리자와 시간 변화량을 사용해 행동을 갱신한다.
        // 'Tick' 메소드의 입력과 반환 계약을 선언한다.
        public void Tick(
            // 'roster' 매개변수 또는 지역값의 타입을 'UnitRosterService'로 지정한다.
            UnitRosterService roster,
            // 'combatManager' 매개변수 또는 지역값의 타입을 'InGameCombatManager'로 지정한다.
            InGameCombatManager combatManager,
            // 'deltaTime' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float deltaTime,
            // 'logAttackAttempts' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool logAttackAttempts)
        {
            // 'LastAttackAttemptCount'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            LastAttackAttemptCount = 0;

            // [방어 로직] 'roster == null || deltaTime <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (roster == null || deltaTime <= 0f)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'enemies'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemies = roster.Enemies;
            // 'var i = 0; i < enemies.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < enemies.Count; i++)
            {
                // 'TickEnemy' 메소드를 호출해 현재 단계의 처리를 실행한다.
                TickEnemy(enemies[i], roster, combatManager, deltaTime, logAttackAttempts);
            }
        }

        // 한 적의 상태 효과, 대상, 지원 스킬, 이동, 공격 실행 순서를 처리한다.
        // 'TickEnemy' 메소드의 입력과 반환 계약을 선언한다.
        private void TickEnemy(
            // 'enemyEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry enemyEntry,
            // 'roster' 매개변수 또는 지역값의 타입을 'UnitRosterService'로 지정한다.
            UnitRosterService roster,
            // 'combatManager' 매개변수 또는 지역값의 타입을 'InGameCombatManager'로 지정한다.
            InGameCombatManager combatManager,
            // 'deltaTime' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float deltaTime,
            // 'logAttackAttempts' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool logAttackAttempts)
        {
            // '!EnemyTargeting.IsActive(enemyEntry)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!EnemyTargeting.IsActive(enemyEntry))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'enemyModel'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemyModel = enemyEntry.Model as EnemyUnitRuntimeModel;
            // [방어 로직] 'enemyModel == null || !enemyModel.AutoAttackEnabled' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemyModel == null || !enemyModel.AutoAttackEnabled)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'state'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var state = GetState(enemyModel);
            // 'SharedChargeSkillRuntime.Tick(enemyEntry, roster, combatManager, deltaTime)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (SharedChargeSkillRuntime.Tick(enemyEntry, roster, combatManager, deltaTime))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'target'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var target = EnemyTargeting.FindNearestPlayerTarget(enemyEntry, roster);
            // [방어 로직] 'target != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target != null)
            {
                // 'state.TargetUnitId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                state.TargetUnitId = target.Model != null && target.Model.Identity != null
                    // [낯선 문법] 삼항 연산자의 조건 참 결과로 'target.Model.Identity.UnitId' 값을 선택한다.
                    ? target.Model.Identity.UnitId
                    // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'null;' 값을 선택한다.
                    : null;
            }

            // 'EnemyTargeting.IsNexus(target)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (EnemyTargeting.IsNexus(target))
            {
                // 'TickNexusAssault' 메소드를 호출해 현재 단계의 처리를 실행한다.
                TickNexusAssault(enemyEntry, enemyModel, target, combatManager, deltaTime);
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'canAct'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var canAct = StatusEffectRuntime.CanAct(enemyModel);
            // 지역 변수 'canUseSpecialSkill'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var canUseSpecialSkill = canAct && StatusEffectRuntime.CanUseSpecialSkill(enemyModel);
            // 지역 변수 'specialRuntime'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var specialRuntime = ResolveSelectableRuntime(enemyModel, InGameSkillSlot.B);
            // 지역 변수 'executedSupportSkill'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var executedSupportSkill = canUseSpecialSkill
                // 앞 조건과 AND로 'IsSupportSkill(specialRuntime)' 조건을 추가한다.
                && IsSupportSkill(specialRuntime)
                // 앞 조건과 AND로 'CanExecuteSupportSkill(specialRuntime, roster)' 조건을 추가한다.
                && CanExecuteSupportSkill(specialRuntime, roster)
                // 앞 조건과 AND로 'TryExecuteSharedSkill(' 조건을 추가한다.
                && TryExecuteSharedSkill(
                    // 'enemyEntry' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    enemyEntry,
                    // 'enemyModel' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    enemyModel,
                    // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    target,
                    // 'combatManager' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    combatManager,
                    // 'specialRuntime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    specialRuntime,
                    // 'state' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    state,
                    // 'deltaTime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    deltaTime,
                    // 'logAttackAttempts' 값을 현재 메소드 호출의 인수로 전달한다.
                    logAttackAttempts);

            // [방어 로직] 'target == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'offensiveRuntime'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var offensiveRuntime = ResolvePreferredOffensiveRuntime(
                // 'enemyEntry' 열거값을 선택 가능한 상수 항목으로 정의한다.
                enemyEntry,
                // 'enemyModel' 열거값을 선택 가능한 상수 항목으로 정의한다.
                enemyModel,
                // 'combatManager' 열거값을 선택 가능한 상수 항목으로 정의한다.
                combatManager,
                // 'canUseSpecialSkill' 값을 현재 메소드 호출의 인수로 전달한다.
                canUseSpecialSkill);
            // [방어 로직] 'offensiveRuntime == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (offensiveRuntime == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'distance'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            // 지역 변수 'attackRange'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var attackRange = ResolveAttackAttemptRange(enemyModel, offensiveRuntime);
            // 'distance > attackRange' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (distance > attackRange)
            {
                // 'StatusEffectRuntime.CanMove(enemyModel)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (StatusEffectRuntime.CanMove(enemyModel))
                {
                    // 'MoveToward' 메소드를 호출해 현재 단계의 처리를 실행한다.
                    MoveToward(enemyEntry, target, enemyModel, deltaTime);
                }

                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // '!canAct || executedSupportSkill' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!canAct || executedSupportSkill)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'TryExecuteSharedSkill' 메소드를 호출해 현재 단계의 처리를 실행한다.
            TryExecuteSharedSkill(
                // 'enemyEntry' 열거값을 선택 가능한 상수 항목으로 정의한다.
                enemyEntry,
                // 'enemyModel' 열거값을 선택 가능한 상수 항목으로 정의한다.
                enemyModel,
                // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                target,
                // 'combatManager' 열거값을 선택 가능한 상수 항목으로 정의한다.
                combatManager,
                // 'offensiveRuntime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                offensiveRuntime,
                // 'state' 열거값을 선택 가능한 상수 항목으로 정의한다.
                state,
                // 'deltaTime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                deltaTime,
                // 'logAttackAttempts' 값을 현재 메소드 호출의 인수로 전달한다.
                logAttackAttempts);
        }

        // 실행 가능한 특수 공격을 우선하고, 없으면 기본 공격 스킬을 선택한다.
        // 'ResolvePreferredOffensiveRuntime' 메소드의 입력과 반환 계약을 선언한다.
        private SkillRuntimeInstance ResolvePreferredOffensiveRuntime(
            // 'enemyEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry enemyEntry,
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'combatManager' 매개변수 또는 지역값의 타입을 'InGameCombatManager'로 지정한다.
            InGameCombatManager combatManager,
            // 'canUseSpecialSkill' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool canUseSpecialSkill)
        {
            // 지역 변수 'special'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var special = ResolveSelectableRuntime(enemyModel, InGameSkillSlot.B);
            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (canUseSpecialSkill
                // 앞 조건과 AND로 '!IsSupportSkill(special)' 조건을 추가한다.
                && !IsSupportSkill(special)
                // 앞 조건과 AND로 'combatManager != null' 조건을 추가한다.
                && combatManager != null
                // 앞 조건과 AND로 'combatManager.CanExecuteSelectedSkill(enemyEntry, special))' 조건을 추가한다.
                && combatManager.CanExecuteSelectedSkill(enemyEntry, special))
            {
                // 계산 또는 조회 결과 'special'을 호출자에게 반환한다.
                return special;
            }

            // 지역 변수 'basic'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var basic = ResolveSelectableRuntime(enemyModel, InGameSkillSlot.A);
            // '!IsSupportSkill(basic' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!IsSupportSkill(basic)
                // 앞 조건과 AND로 'combatManager != null' 조건을 추가한다.
                && combatManager != null
                // 앞 조건과 AND로 'combatManager.CanExecuteSelectedSkill(enemyEntry, basic))' 조건을 추가한다.
                && combatManager.CanExecuteSelectedSkill(enemyEntry, basic))
            {
                // 계산 또는 조회 결과 'basic'을 호출자에게 반환한다.
                return basic;
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 선택된 스킬의 실행 가능성을 확인하고 실행한 뒤 공격 시도 상태와 로그를 갱신한다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryExecuteSharedSkill' 메소드의 입력과 반환 계약을 선언한다.
        private bool TryExecuteSharedSkill(
            // 'enemyEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry enemyEntry,
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'target' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry target,
            // 'combatManager' 매개변수 또는 지역값의 타입을 'InGameCombatManager'로 지정한다.
            InGameCombatManager combatManager,
            // 'runtime' 매개변수 또는 지역값의 타입을 'SkillRuntimeInstance'로 지정한다.
            SkillRuntimeInstance runtime,
            // 'state' 매개변수 또는 지역값의 타입을 'EnemyCombatState'로 지정한다.
            EnemyCombatState state,
            // 'deltaTime' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float deltaTime,
            // 'logAttackAttempts' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool logAttackAttempts)
        {
            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (runtime == null
                // [방어 로직] 앞 조건과 OR로 'combatManager == null' 조건을 추가한다.
                || combatManager == null
                // [방어 로직] 앞 조건과 OR로 '!combatManager.CanExecuteSelectedSkill(enemyEntry, runtime)' 조건을 추가한다.
                || !combatManager.CanExecuteSelectedSkill(enemyEntry, runtime)
                // [방어 로직] 앞 조건과 OR로 '!combatManager.TryExecuteSelectedSkill(enemyEntry, runtime, deltaTime))' 조건을 추가한다.
                || !combatManager.TryExecuteSelectedSkill(enemyEntry, runtime, deltaTime))
            {
                // [방어 로직] Try 패턴 메소드 'TryExecuteSharedSkill'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'state.AttackAttemptCount++;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            state.AttackAttemptCount++;
            // 'LastAttackAttemptCount++;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            LastAttackAttemptCount++;

            // 'logAttackAttempts' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (logAttackAttempts)
            {
                // 현재 처리 결과를 진단 로그로 남긴다: BuildAttackAttemptLog(enemyModel, target, runtime.SkillId).
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, runtime.SkillId));
            }

            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 지정 슬롯의 런타임 스킬을 찾되 전투 시작 전용 Trigger 스킬은 일반 선택에서 제외한다.
        // 'ResolveSelectableRuntime' 메소드의 입력과 반환 계약을 선언한다.
        private static SkillRuntimeInstance ResolveSelectableRuntime(
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'slot' 매개변수 또는 지역값의 타입을 'InGameSkillSlot'로 지정한다.
            InGameSkillSlot slot)
        {
            // 지역 변수 'runtime'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var runtime = enemyModel != null && enemyModel.SkillRuntime != null
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'enemyModel.SkillRuntime.FindBySlot(slot)' 값을 선택한다.
                ? enemyModel.SkillRuntime.FindBySlot(slot)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'null;' 값을 선택한다.
                : null;
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건 결과에 맞는 값 하나를 반환한다.
            return HasCombatStartTrigger(runtime) ? null : runtime;
        }

        // 스킬에 CombatStart 이벤트 Trigger가 포함됐는지 검사한다.
        // 'HasCombatStartTrigger' 메소드의 입력과 반환 계약을 선언한다.
        private static bool HasCombatStartTrigger(SkillRuntimeInstance runtime)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var triggers = runtime != null && runtime.Data != null ? runtime.Data.SkillTriggers : null;
            // 'var i = 0; triggers != null && i < triggers.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; triggers != null && i < triggers.Length; i++)
            {
                // [방어 로직] 'triggers[i] != null && triggers[i].TriggerEvent == SkillTriggerEvent.CombatStart' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (triggers[i] != null && triggers[i].TriggerEvent == SkillTriggerEvent.CombatStart)
                {
                    // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                    return true;
                }
            }

            // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
            return false;
        }

        // 스킬 대상 진영이 적이 아닌지 확인해 지원 스킬 여부를 판별한다.
        // 'IsSupportSkill' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsSupportSkill(SkillRuntimeInstance runtime)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var targeting = runtime != null && runtime.Data != null ? runtime.Data.Targeting : null;
            // 계산 또는 조회 결과 'targeting != null && targeting.TargetSide != SkillTargetSide.Enemy'을 호출자에게 반환한다.
            return targeting != null && targeting.TargetSide != SkillTargetSide.Enemy;
        }

        // 회복 스킬은 부상당한 적 아군이 있을 때만 허용하고 다른 지원 스킬은 허용한다.
        // 'CanExecuteSupportSkill' 메소드의 입력과 반환 계약을 선언한다.
        private static bool CanExecuteSupportSkill(SkillRuntimeInstance runtime, UnitRosterService roster)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return !(runtime != null && runtime.Data is HealSkillData)
                // [방어 로직] 앞 조건과 OR로 'EnemyTargeting.FindLowestHealthEnemyAlly(roster) != null;' 조건을 추가한다.
                || EnemyTargeting.FindLowestHealthEnemyAlly(roster) != null;
        }

        // 스킬 사거리 값을 우선하고 없으면 적 공격 유형별 기본 행동 사거리를 반환한다.
        // 'ResolveAttackAttemptRange' 메소드의 입력과 반환 계약을 선언한다.
        private static float ResolveAttackAttemptRange(
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'runtime' 매개변수 또는 지역값의 타입을 'SkillRuntimeInstance'로 지정한다.
            SkillRuntimeInstance runtime)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var targeting = runtime != null && runtime.Data != null ? runtime.Data.Targeting : null;
            // [방어 로직] 'targeting != null && targeting.Range > 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (targeting != null && targeting.Range > 0f)
            {
                // 계산 또는 조회 결과 'Mathf.Max(0.1f, targeting.Range)'을 호출자에게 반환한다.
                return Mathf.Max(0.1f, targeting.Range);
            }

            // 'enemyModel != null ? enemyModel.AttackType : EnemyAttackType.Melee' 값에 따라 여러 처리 경로 중 하나를 선택한다.
            switch (enemyModel != null ? enemyModel.AttackType : EnemyAttackType.Melee)
            {
                // switch 값이 'EnemyAttackType.Ranged'일 때 이 분기를 실행한다.
                case EnemyAttackType.Ranged:
                // switch 값이 'EnemyAttackType.Buffer'일 때 이 분기를 실행한다.
                case EnemyAttackType.Buffer:
                    // 계산 또는 조회 결과 '5f'을 호출자에게 반환한다.
                    return 5f;
                // switch 값이 'EnemyAttackType.MeleeAndRanged'일 때 이 분기를 실행한다.
                case EnemyAttackType.MeleeAndRanged:
                    // 계산 또는 조회 결과 '4f'을 호출자에게 반환한다.
                    return 4f;
                // [Fallback] 어떤 case에도 맞지 않을 때 기본 처리 분기를 실행한다.
                default:
                    // 계산 또는 조회 결과 '1.4f'을 호출자에게 반환한다.
                    return 1.4f;
            }
        }

        // 이동 속도와 상태 효과 배율을 적용해 적을 목표 위치 쪽으로 이동시킨다.
        // 'MoveToward' 메소드의 입력과 반환 계약을 선언한다.
        internal static void MoveToward(
            // 'enemyEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry enemyEntry,
            // 'target' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry target,
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'deltaTime' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float deltaTime)
        {
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
            // 'moveSpeed *= StatusEffectRuntime.ResolveMoveSpeedMultiplier(enemyModel);' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            moveSpeed *= StatusEffectRuntime.ResolveMoveSpeedMultiplier(enemyModel);
            // [방어 로직] 'moveSpeed <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (moveSpeed <= 0f)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'current'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var current = enemyEntry.Transform.position;
            // 지역 변수 'targetPosition'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var targetPosition = target.Transform.position;
            // 'targetPosition.z'에 오른쪽 계산 또는 조회 결과를 저장한다.
            targetPosition.z = current.z;
            // 'enemyEntry.Transform.position'에 오른쪽 계산 또는 조회 결과를 저장한다.
            enemyEntry.Transform.position = Vector3.MoveTowards(current, targetPosition, moveSpeed * deltaTime);
        }

        // 적이 넥서스에 닿을 때까지 이동시키고 접촉하면 넥서스 피해 후 적을 제거한다.
        // 'TickNexusAssault' 메소드의 입력과 반환 계약을 선언한다.
        private static void TickNexusAssault(
            // 'enemyEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry enemyEntry,
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'nexusTarget' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry nexusTarget,
            // 'combatManager' 매개변수 또는 지역값의 타입을 'InGameCombatManager'로 지정한다.
            InGameCombatManager combatManager,
            // 'deltaTime' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float deltaTime)
        {
            // [방어 로직] 'enemyEntry == null || enemyModel == null || nexusTarget == null || combatManager == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemyEntry == null || enemyModel == null || nexusTarget == null || combatManager == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // '!IsTouchingNexus(enemyEntry, nexusTarget)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!IsTouchingNexus(enemyEntry, nexusTarget))
            {
                // 'StatusEffectRuntime.CanMove(enemyModel)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (StatusEffectRuntime.CanMove(enemyModel))
                {
                    // 'MoveToward' 메소드를 호출해 현재 단계의 처리를 실행한다.
                    MoveToward(enemyEntry, nexusTarget, enemyModel, deltaTime);
                }

                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var damage = Mathf.Max(1f, enemyModel.NexusDamage);
            // 'combatManager.ApplyDamage' 메소드를 호출해 해당 객체의 처리를 실행한다.
            combatManager.ApplyDamage(nexusTarget.Model, damage, DamageAttribute.Physical, enemyModel, false);
            // 'combatManager.DespawnUnit' 메소드를 호출해 해당 객체의 처리를 실행한다.
            combatManager.DespawnUnit(enemyModel);
        }

        // 히트박스 겹침과 근접 거리로 적이 넥서스에 접촉했는지 판별한다.
        // 'IsTouchingNexus' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsTouchingNexus(UnitRosterEntry enemyEntry, UnitRosterEntry nexusTarget)
        {
            // [방어 로직] 'enemyEntry == null || nexusTarget == null || enemyEntry.Transform == null || nexusTarget.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemyEntry == null || nexusTarget == null || enemyEntry.Transform == null || nexusTarget.Transform == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'enemyPoint'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemyPoint = enemyEntry.ResolveTargetPoint();
            // 지역 변수 'targetColliders'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var targetColliders = nexusTarget.GetHitboxColliders();
            // 'var i = 0; i < targetColliders.Length; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < targetColliders.Length; i++)
            {
                // 지역 변수 'collider'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var collider = targetColliders[i];
                // [방어 로직] 'collider != null && collider.enabled && collider.OverlapPoint(enemyPoint)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (collider != null && collider.enabled && collider.OverlapPoint(enemyPoint))
                {
                    // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                    return true;
                }
            }

            // 'UnitHitboxUtility.IsTargetInsideHitbox(enemyEntry.GetHitboxColliders(), nexusTarget)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (UnitHitboxUtility.IsTargetInsideHitbox(enemyEntry.GetHitboxColliders(), nexusTarget))
            {
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // 계산 또는 조회 결과 'Vector2.Distance(enemyEntry.Transform.position, nexusTarget.Transform.position) <= 0.25f'을 호출자에게 반환한다.
            return Vector2.Distance(enemyEntry.Transform.position, nexusTarget.Transform.position) <= 0.25f;
        }

        // 적 유닛 ID에 대응하는 지속 전투 상태를 찾거나 새로 만든다.
        // 'GetState' 메소드의 입력과 반환 계약을 선언한다.
        private EnemyCombatState GetState(EnemyUnitRuntimeModel enemyModel)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var unitId = enemyModel.Identity != null ? enemyModel.Identity.UnitId : null;
            // [방어 로직] 'string.IsNullOrWhiteSpace(unitId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(unitId))
            {
                // 'unitId'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                unitId = "enemy-unknown";
            }

            // [방어 로직] '!enemyStates.TryGetValue(unitId, out var state)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!enemyStates.TryGetValue(unitId, out var state))
            {
                // 'state'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                state = new EnemyCombatState();
                // Add 호출 결과 또는 지정 항목을 컬렉션에 추가한다.
                enemyStates.Add(unitId, state);
            }

            // 계산 또는 조회 결과 'state'을 호출자에게 반환한다.
            return state;
        }

        // 적 이름, 대상 이름, 스킬 ID를 포함한 공격 시도 로그 문자열을 만든다.
        // 'BuildAttackAttemptLog' 메소드의 입력과 반환 계약을 선언한다.
        private static string BuildAttackAttemptLog(
            // 'enemyModel' 매개변수 또는 지역값의 타입을 'EnemyUnitRuntimeModel'로 지정한다.
            EnemyUnitRuntimeModel enemyModel,
            // 'target' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry target,
            // 'skillId' 매개변수 또는 지역값의 타입을 'string'로 지정한다.
            string skillId)
        {
            // 지역 변수 'enemyName'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var enemyName = enemyModel.Identity != null && !string.IsNullOrWhiteSpace(enemyModel.Identity.DisplayName)
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'enemyModel.Identity.DisplayName' 값을 선택한다.
                ? enemyModel.Identity.DisplayName
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                : enemyModel.Identity != null ? enemyModel.Identity.DefinitionId : "enemy";
            // 지역 변수 'targetName'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var targetName = target != null
                // 앞 조건과 AND로 'target.Model != null' 조건을 추가한다.
                && target.Model != null
                // 앞 조건과 AND로 'target.Model.Identity != null' 조건을 추가한다.
                && target.Model.Identity != null
                // 앞 조건과 AND로 '!string.IsNullOrWhiteSpace(target.Model.Identity.DisplayName)' 조건을 추가한다.
                && !string.IsNullOrWhiteSpace(target.Model.Identity.DisplayName)
                    // [낯선 문법] 삼항 연산자의 조건 참 결과로 'target.Model.Identity.DisplayName' 값을 선택한다.
                    ? target.Model.Identity.DisplayName
                    // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                    : target != null && target.Model != null && target.Model.Identity != null ? target.Model.Identity.DefinitionId : "target";

            // 계산 또는 조회 결과 '$"Enemy skill attempt: {enemyName} -> {targetName} ({skillId})"'을 호출자에게 반환한다.
            return $"Enemy skill attempt: {enemyName} -> {targetName} ({skillId})";
        }
    }

    // 한 적이 현재 추적하는 대상과 누적 공격 시도 횟수를 저장한다.
    // 'EnemyCombatState' 클래스 정의를 시작한다.
    internal class EnemyCombatState
    {
        // 'TargetUnitId' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public string TargetUnitId;
        // 'AttackAttemptCount' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        public int AttackAttemptCount;
    }
}
