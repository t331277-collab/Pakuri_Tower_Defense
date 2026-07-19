// 'System' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System;
// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'Pakuri.Combat' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Combat;
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 피해 적용 시 공격자, 치명타, 스킬 출처, Trigger 억제, 피해 통계 출처를 전달한다.
    // 'DamageApplicationOptions' 값 형식 구조체 정의를 시작한다.
    public readonly struct DamageApplicationOptions
    {
        // 한 번의 피해 적용에 필요한 선택 옵션을 불변 값으로 구성한다.
        // 'DamageApplicationOptions' 메소드의 입력과 반환 계약을 선언한다.
        public DamageApplicationOptions(
            // 'source' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel source,
            // 이 공격이 치명타 계산을 허용하는지 지정한다.
            bool criticalAllowed,
            // 기본 치명타 확률에 더할 값을 지정한다.
            float critChanceBonus,
            // 기본 치명타 피해 배율에 더할 값을 지정한다.
            float critDamageBonus,
            // 피해를 발생시킨 스킬 ID를 지정한다.
            string sourceSkillId,
            // 추가 피해 Trigger 재호출을 막을지 지정한다.
            bool suppressOutgoingDamageTriggers,
            // 현재 타격이 처형 판정이었는지 지정한다.
            bool sourceHitWasExecute,
            // 피해 통계에서 사용할 출처 ID를 지정한다.
            string damageMeterSourceId)
        {
            // 'Source'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            Source = source;
            // 'CriticalAllowed'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            CriticalAllowed = criticalAllowed;
            // 'CritChanceBonus'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            CritChanceBonus = critChanceBonus;
            // 'CritDamageBonus'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            CritDamageBonus = critDamageBonus;
            // 'SourceSkillId'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SourceSkillId = sourceSkillId;
            // 'SuppressOutgoingDamageTriggers'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SuppressOutgoingDamageTriggers = suppressOutgoingDamageTriggers;
            // 'SourceHitWasExecute'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            SourceHitWasExecute = sourceHitWasExecute;
            // 'DamageMeterSourceId'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            DamageMeterSourceId = damageMeterSourceId;
        }

        // 'Source' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public BaseUnitRuntimeModel Source { get; }
        // 'CriticalAllowed' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public bool CriticalAllowed { get; }
        // 'CritChanceBonus' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float CritChanceBonus { get; }
        // 'CritDamageBonus' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float CritDamageBonus { get; }
        // 'SourceSkillId' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public string SourceSkillId { get; }
        // 'SuppressOutgoingDamageTriggers' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public bool SuppressOutgoingDamageTriggers { get; }
        // 'SourceHitWasExecute' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public bool SourceHitWasExecute { get; }
        // 'DamageMeterSourceId' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public string DamageMeterSourceId { get; }
    }

    // 전투 로스터, 피해·상태 처리, 스킬 실행, 적 AI를 순서대로 조율한다.
    // Code Builder: 입력·표시·통계·날짜·경계 소유권은 각 담당 스크립트로 분리했다.

    // 'InGameCombatManager' 클래스 정의를 시작한다.
    public sealed class InGameCombatManager : MonoBehaviour
    {
        // 'PassiveEffectRefreshInterval' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const float PassiveEffectRefreshInterval = 0.25f;

        // [낯선 문법] readonly 필드 'roster'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly UnitRosterService roster = new UnitRosterService();
        // [낯선 문법] readonly 필드 'enemyCombatSystem'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly EnemyCombatSystem enemyCombatSystem = new EnemyCombatSystem();
        // [낯선 문법] readonly 필드 'skillExecution'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();
        // Code Builder: 입력 상태는 PlayerCombatControl이 소유하고 이 관리자는 실행 순서만 조율한다.
        [SerializeField] private PlayerCombatControl playerCombatControl;
        // [낯선 문법] readonly 필드 'appliedOneShotPassiveEffects'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly HashSet<string> appliedOneShotPassiveEffects = new HashSet<string>();
        // [낯선 문법] readonly 필드 'combatStartDispatchedUnits'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly HashSet<BaseUnitRuntimeModel> combatStartDispatchedUnits = new HashSet<BaseUnitRuntimeModel>();
        // [낯선 문법] readonly 필드 'passiveTriggerCooldowns'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly Dictionary<string, float> passiveTriggerCooldowns = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase);
        // [낯선 문법] readonly 필드 'passiveTriggerCounts'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly Dictionary<string, int> passiveTriggerCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // 'passiveEffectRefreshRemaining' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private float passiveEffectRefreshRemaining;

        // [낯선 문법] SerializeField attribute: private 상태 'enemyCombatSimulationEnabled'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        // [낯선 문법] SerializeField attribute: private 상태 'skillExecutionEnabled'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool skillExecutionEnabled = true;
        // [낯선 문법] SerializeField attribute: private 상태 'logEnemyAttackAttempts'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool logEnemyAttackAttempts;
        // [낯선 문법] SerializeField attribute: private 상태 'logSkillExecutionContracts'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool logSkillExecutionContracts;
        // [낯선 문법] SerializeField attribute: private 상태 'effectManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private EffectManager effectManager;

        // [낯선 문법] 식 본문 property: 'Roster' 값을 오른쪽 식 하나로 계산해 반환한다.
        public UnitRosterService Roster => roster;
        // [낯선 문법] 식 본문 property: 'Effects' 값을 오른쪽 식 하나로 계산해 반환한다.
        public EffectManager Effects => effectManager;

        // [낯선 문법] 식 본문 property: 'ActiveEnemyCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int ActiveEnemyCount => roster.EnemyCount;
        // Code Builder: 피해 통계는 전투 결과 이벤트를 구독하는 쪽에서 기록한다.
        public event Action<DamageApplicationOptions, InGameResourceChangeResult> DamageApplied;

        // 전투 시작 전 로스터, 적 상태, 전투 시작 Trigger 기록, 패시브 상태를 초기화한다.
        // 'Awake' 메소드의 입력과 반환 계약을 선언한다.
        private void Awake()
        {
            // 같은 GameObject에 연결된 플레이어 입력 처리기를 찾는다.
            if (playerCombatControl == null)
            {
                playerCombatControl = GetComponent<PlayerCombatControl>();
            }

            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            roster.Clear();
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            combatStartDispatchedUnits.Clear();
            // 'ResetPassiveEffectState' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ResetPassiveEffectState();
        }

        // 매 프레임 패시브, 스킬, 수동 입력, 적 AI, 상태 지속시간을 순서대로 갱신한다.
        // 'Update' 메소드의 입력과 반환 계약을 선언한다.
        private void Update()
        {
            // 'TickLearnedPassiveEffects' 메소드를 호출해 현재 단계의 처리를 실행한다.
            TickLearnedPassiveEffects(Time.deltaTime);

            // 'skillExecutionEnabled' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (skillExecutionEnabled)
            {
                // 'skillExecution.Tick' 메소드를 호출해 해당 객체의 처리를 실행한다.
                skillExecution.Tick(
                    // 'roster' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    roster,
                    // 'this' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    this,
                    // 'Time.deltaTime' 값을 현재 메소드 호출의 인수로 전달한다.
                    Time.deltaTime,
                    // 'logSkillExecutionContracts' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    logSkillExecutionContracts,
                    // 'ShouldAutoRouteSkill' 값을 현재 메소드 호출의 인수로 전달한다.
                    ShouldAutoRouteSkill);
                // 선택 플레이어의 수동 입력을 별도 입력 처리기에 전달한다.
                if (playerCombatControl != null)
                {
                    playerCombatControl.HandleManualInput(
                        roster,
                        skillExecution,
                        this,
                        Time.deltaTime,
                        logSkillExecutionContracts);
                }
            }

            // 'enemyCombatSimulationEnabled' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (enemyCombatSimulationEnabled)
            {
                // 'enemyCombatSystem.Tick' 메소드를 호출해 해당 객체의 처리를 실행한다.
                enemyCombatSystem.Tick(roster, this, Time.deltaTime, logEnemyAttackAttempts);
            }

            // 'TickUnitStatuses' 메소드를 호출해 현재 단계의 처리를 실행한다.
            TickUnitStatuses(Time.deltaTime);
        }

        // 플레이어 몬스터를 로스터에 등록하고 자동 스킬 설정과 전투 시작 Trigger를 적용한다.
        // 'RegisterPlayerMonster' 메소드의 입력과 반환 계약을 선언한다.
        public UnitRosterEntry RegisterPlayerMonster(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot)
        {
            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Register(model, actor, hitboxRoot);
            // 'IsSelectedPlayerModel(model)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (playerCombatControl != null && PlayerCombatControl.IsSelectedPlayerModel(model))
            {
                // 현재 자동 스킬 설정을 새로 등록된 선택 플레이어에게 적용한다.
                playerCombatControl.ApplyAutoSkillModeToSelectedPlayer(roster);
            }

            // 'DispatchCombatStartOnce' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchCombatStartOnce(model);
            // 계산 또는 조회 결과 'entry'을 호출자에게 반환한다.
            return entry;
        }

        // 적을 로스터에 등록하고 해당 유닛의 전투 시작 Trigger를 한 번 실행한다.
        // 'RegisterEnemy' 메소드의 입력과 반환 계약을 선언한다.
        public UnitRosterEntry RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot)
        {
            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Register(model, actor, hitboxRoot);
            // 'DispatchCombatStartOnce' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchCombatStartOnce(model);
            // 계산 또는 조회 결과 'entry'을 호출자에게 반환한다.
            return entry;
        }

        // 넥서스 모델과 Actor를 전투 로스터에 등록한다.
        // 'RegisterNexus' 메소드의 입력과 반환 계약을 선언한다.
        public UnitRosterEntry RegisterNexus(NexusUnitRuntimeModel model, NexusUnitActor actor, Transform hitboxRoot)
        {
            // 계산 또는 조회 결과 'roster.Register(model, actor, hitboxRoot)'을 호출자에게 반환한다.
            return roster.Register(model, actor, hitboxRoot);
        }

        // 모델을 로스터에서 해제하고 연결된 Actor GameObject를 제거한다.
        // 'DespawnUnit' 메소드의 입력과 반환 계약을 선언한다.
        public bool DespawnUnit(BaseUnitRuntimeModel model)
        {
            // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (model == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Find(model);
            // [방어 로직] 'entry == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = entry.Actor;
            // 'roster.Unregister' 메소드를 호출해 해당 객체의 처리를 실행한다.
            roster.Unregister(model);
            // [방어 로직] 'actor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (actor != null)
            {
                // 지연 없이 로스터에서 해제된 Actor를 제거한다.
                Destroy(actor.gameObject);
            }

            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 피해 옵션을 구성해 자원 변경, 피해 표시, 보호막·공격·처치 Trigger, 사망 처리를 실행한다.
        // 'ApplyDamage' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult ApplyDamage(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDamage,
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'source' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel source,
            // [Fallback][낯선 문법] 선택 인수 'criticalAllowed'가 생략되면 기본값 'false'을 사용한다.
            bool criticalAllowed = false,
            // [Fallback][낯선 문법] 선택 인수 'critChanceBonus'가 생략되면 기본값 '0f'을 사용한다.
            float critChanceBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'critDamageBonus'가 생략되면 기본값 '0f'을 사용한다.
            float critDamageBonus = 0f,
            // [Fallback][낯선 문법] 선택 인수 'sourceSkillId'가 생략되면 기본값 'null'을 사용한다.
            string sourceSkillId = null,
            // [Fallback][낯선 문법] 선택 인수 'suppressOutgoingDamageTriggers'가 생략되면 기본값 'false'을 사용한다.
            bool suppressOutgoingDamageTriggers = false,
            // [Fallback][낯선 문법] 선택 인수 'sourceHitWasExecute'가 생략되면 기본값 'false'을 사용한다.
            bool sourceHitWasExecute = false,
            string damageMeterSourceId = null)
        {
            // 지역 변수 'depletedShields'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var depletedShields = new List<UnitStatusRuntime>();
            // 지역 변수 'absorbedShields'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var absorbedShields = new List<ShieldAbsorbRecord>();
            // 지역 변수 'options'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var options = new DamageApplicationOptions(source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers, sourceHitWasExecute, damageMeterSourceId);
            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = ApplyDamageToResources(target, baseDamage, attribute, options, depletedShields, absorbedShields);
            // 통계와 UI가 피해 결과를 직접 구독할 수 있도록 알린다.
            DamageApplied?.Invoke(options, result);
            // 'RefreshActorIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshActorIfChanged(result);
            // 피해 숫자와 피격 연출을 등록된 Actor에 전달한다.
            var damagedEntry = roster.Find(result.Target);
            // 실제 자원 변화가 있는 등록 유닛만 피해 표시를 갱신한다.
            if (result.Changed && damagedEntry != null)
            {
                damagedEntry.ShowDamage(result.AppliedDamage, result.IsDead);
            }
            // 'DispatchShieldAbsorbTriggers' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchShieldAbsorbTriggers(target, source, absorbedShields);
            // 'DispatchShieldExpireTriggers' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchShieldExpireTriggers(target, depletedShields);
            // 'DispatchOutgoingDamageTriggers' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchOutgoingDamageTriggers(target, attribute, options, result, baseDamage);
            // 'DispatchKillTriggers' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchKillTriggers(target, attribute, options, result);
            // 'RemoveUnitIfDead' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RemoveUnitIfDead(result);
            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 넥서스를 제외한 대상의 체력을 최대 체력 범위에서 회복하고 Actor를 갱신한다.
        // 'Heal' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult Heal(BaseUnitRuntimeModel target, float amount)
        {
            // Nexus는 회복 대상에서 제외한다.
            if (target != null && target.IsNexus)
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = HealResources(target, amount);
            // 'RefreshActorIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshActorIfChanged(result);
            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 피해를 상태 보호막, 직접 보호막, 체력 순서로 적용한다.
        private static InGameResourceChangeResult ApplyDamageToResources(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute,
            DamageApplicationOptions options,
            ICollection<UnitStatusRuntime> depletedShields,
            ICollection<ShieldAbsorbRecord> absorbedShields)
        {
            // 피해를 적용할 수 없는 요청은 변경 없음으로 끝낸다.
            if (target == null || target.Resources == null || baseDamage <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 적용 전 자원과 방어 계산이 끝난 최종 피해를 기록한다.
            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = GetTotalShield(target);
            var finalDamage = CalculateDamage(target, baseDamage, attribute, options);

            // 상태 Trigger가 참조할 수 있도록 이번 속성 피해를 상태 런타임에 기록한다.
            if (target.Statuses != null)
            {
                target.Statuses.RecordIncomingDamage(attribute, finalDamage);
            }

            // 상태 보호막, 직접 보호막, 체력 순서로 남은 피해를 전달한다.
            var statusShieldDamage = target.Statuses != null
                ? target.Statuses.ConsumeShield(finalDamage, depletedShields, absorbedShields)
                : 0f;
            var damageAfterStatusShield = Mathf.Max(0f, finalDamage - statusShieldDamage);
            var directShieldBefore = Mathf.Max(0f, resources.DirectShield);
            var directShieldDamage = Mathf.Min(directShieldBefore, damageAfterStatusShield);
            var remainingDamage = Mathf.Max(0f, damageAfterStatusShield - directShieldDamage);

            // 계산 결과를 반올림해 자원 모델과 총 보호막 표시를 동기화한다.
            resources.DirectShield = Round(Mathf.Max(0f, directShieldBefore - directShieldDamage));
            resources.CurrentHealth = Round(Mathf.Max(0f, beforeHealth - remainingDamage));
            SyncShield(target);

            // 적용 전후 값과 사망 여부를 하나의 피해 결과로 반환한다.
            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                finalDamage,
                resources.CurrentHealth <= 0f);
        }

        // 대상 체력을 최대 체력 범위에서 회복한다.
        private static InGameResourceChangeResult HealResources(BaseUnitRuntimeModel target, float amount)
        {
            // 체력과 최대 체력을 확인할 수 없는 요청은 변경하지 않는다.
            if (target == null || target.Resources == null || target.Stats == null || amount <= 0f)
            {
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 회복 전 값을 보존하고 최대 체력을 넘지 않도록 회복량을 적용한다.
            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = GetTotalShield(target);
            var maxHealth = Mathf.Max(0f, target.Stats.MaxHealth);
            resources.CurrentHealth = Round(Mathf.Min(maxHealth, beforeHealth + amount));
            SyncShield(target);

            // 회복 전후 자원 상태를 변경 결과로 반환한다.
            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                resources.CurrentHealth,
                beforeShield,
                resources.CurrentShield,
                0f,
                resources.CurrentHealth <= 0f);
        }

        // 직접 보호막과 상태 보호막의 합계를 표시 자원에 맞춘다.
        private static void SyncShield(BaseUnitRuntimeModel target)
        {
            // 자원 모델이 없는 대상은 동기화할 수 없다.
            if (target == null || target.Resources == null)
            {
                return;
            }

            // 직접 보호막을 정리한 뒤 상태 보호막을 포함한 총량을 갱신한다.
            target.Resources.DirectShield = Round(Mathf.Max(0f, target.Resources.DirectShield));
            target.Resources.CurrentShield = GetTotalShield(target);
        }

        // 공격자 치명타, 대상 저항, 상태 보정, 적 패시브를 반영한 최종 피해를 계산한다.
        private static float CalculateDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute,
            DamageApplicationOptions options)
        {
            // 공격자가 있고 치명타가 허용된 공격만 공격자 치명타 능력치를 사용한다.
            var criticalAllowed = options.CriticalAllowed && options.Source != null;
            var sourceStats = criticalAllowed ? options.Source.Stats : null;
            var criticalChance = criticalAllowed
                ? (sourceStats != null ? sourceStats.CriticalChance : DamageCalculator.BaseCriticalChance)
                    + StatusEffectRuntime.ResolveCriticalChanceBonus(options.Source)
                : DamageCalculator.BaseCriticalChance;
            var criticalDamage = criticalAllowed
                ? (sourceStats != null ? sourceStats.CriticalDamage : DamageCalculator.BaseCriticalMultiplier)
                : DamageCalculator.BaseCriticalMultiplier;

            // 공격자 상태가 제공하는 치명타 피해 보너스를 합산한다.
            if (criticalAllowed)
            {
                criticalDamage += StatusEffectRuntime.ResolveCriticalDamageBonus(options.Source);
            }

            // 대상의 치명타 저항과 치명타 피격 보정을 계산한다.
            var criticalResistance = criticalAllowed
                ? (target != null && target.Stats != null ? target.Stats.CriticalResistance : 0f)
                    + StatusEffectRuntime.ResolveCriticalResistanceBonus(target)
                : 0f;
            var criticalDamageTaken = criticalAllowed
                ? StatusEffectRuntime.ResolveCriticalDamageTakenBonus(target)
                : 0f;
            // 공통 DamageCalculator에 방어력과 상태 기반 보정값을 전달한다.
            var damage = DamageCalculator.Resolve(
                Mathf.Max(0f, baseDamage),
                attribute,
                target != null ? CopyDefenses(target.Defenses) : null,
                criticalAllowed,
                flatDefenseReduction: StatusEffectRuntime.ResolveFlatElementResistReduction(target, attribute),
                percentDefenseReductions: new[] { StatusEffectRuntime.ResolveElementResistReduction(target, attribute) },
                criticalChanceBonus: criticalChance + options.CritChanceBonus - DamageCalculator.BaseCriticalChance,
                criticalMultiplierBonus: criticalDamage + options.CritDamageBonus - DamageCalculator.BaseCriticalMultiplier,
                targetCriticalResistance: criticalResistance,
                criticalDamageTakenBonus: criticalDamageTaken,
                finalDamageMultiplier: GetIncomingDamageMultiplier(target, options.Source, attribute, options.SourceSkillId));

            // 자원 계산에 사용할 수 있도록 음수를 막고 정수 단위로 반올림한다.
            return Mathf.Round(Mathf.Max(0f, damage));
        }

        // 유닛 방어력 값을 DamageCalculator 입력 형식으로 복사한다.
        private static AttributeDefenseSet CopyDefenses(UnitDefenseRuntime defenses)
        {
            // 방어력 모델이 없으면 계산기에 빈 방어값을 전달한다.
            if (defenses == null)
            {
                return null;
            }

            // 여섯 속성 방어력을 같은 속성 항목에 대응시킨다.
            return new AttributeDefenseSet
            {
                Physical = defenses.Physical,
                Fire = defenses.Fire,
                Lightning = defenses.Lightning,
                Ice = defenses.Ice,
                Darkness = defenses.Darkness,
                Holy = defenses.Holy
            };
        }

        // 상태 기반 받는 피해 배율과 적 전용 패시브 배율을 결합한다.
        private static float GetIncomingDamageMultiplier(
            BaseUnitRuntimeModel target,
            BaseUnitRuntimeModel source,
            DamageAttribute attribute,
            string sourceSkillId)
        {
            // 모든 유닛에 공통으로 적용되는 상태 피해 배율을 먼저 계산한다.
            var statusMultiplier = StatusEffectRuntime.ResolveIncomingDamageMultiplier(
                target,
                source,
                attribute,
                sourceSkillId);
            // 적 유닛이면 고유 패시브 배율을 추가하고, 다른 유닛은 상태 배율만 사용한다.
            var enemy = target as EnemyUnitRuntimeModel;
            return enemy == null
                ? statusMultiplier
                : Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier) * statusMultiplier;
        }

        // 직접 보호막과 활성 상태 보호막의 총량을 반환한다.
        private static float GetTotalShield(BaseUnitRuntimeModel target)
        {
            // 보호막 자원을 확인할 수 없으면 0을 반환한다.
            if (target == null || target.Resources == null)
            {
                return 0f;
            }

            // 직접 보호막과 상태 보호막을 각각 0 이상으로 제한해 합산한다.
            var directShield = Mathf.Max(0f, target.Resources.DirectShield);
            var statusShield = target.Statuses != null
                ? Mathf.Max(0f, target.Statuses.GetTotalShieldAmount())
                : 0f;
            return Round(directShield + statusShield);
        }

        // 자원 값을 0 이상 정수 단위로 정리한다.
        private static float Round(float value)
        {
            return Mathf.Round(Mathf.Max(0f, value));
        }

        // StatusEffectData와 출처 유닛을 포함한 상태를 적용하고 런타임 표시를 갱신한다.
        // 'ApplyStatus' 메소드의 입력과 반환 계약을 선언한다.
        public UnitStatusRuntime ApplyStatus(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'statusData' 매개변수 또는 지역값의 타입을 'StatusEffectData'로 지정한다.
            StatusEffectData statusData,
            // 'stacks' 매개변수 또는 지역값의 타입을 'int'로 지정한다.
            int stacks,
            // 'durationSeconds' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float durationSeconds,
            // 허용할 최대 중첩 수를 지정한다.
            int maxStacks,
            // 지속시간 없이 유지할지 지정한다.
            bool permanent,
            // 재적용 시 지속시간을 갱신할지 지정한다.
            bool refreshDuration,
            // 상태를 발생시킨 유닛을 지정한다.
            BaseUnitRuntimeModel source)
        {
            // 상태 적용에 필요한 대상과 상태 정의를 확인하고 Nexus를 제외한다.
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind == StatusEffectKind.None || target.IsNexus)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 지역 변수 'status'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var status = target.Statuses.Apply(
                // 'statusData' 열거값을 선택 가능한 상수 항목으로 정의한다.
                statusData,
                // 'stacks' 열거값을 선택 가능한 상수 항목으로 정의한다.
                stacks,
                // 'durationSeconds' 열거값을 선택 가능한 상수 항목으로 정의한다.
                durationSeconds,
                // 'maxStacks' 열거값을 선택 가능한 상수 항목으로 정의한다.
                maxStacks,
                // 'permanent' 열거값을 선택 가능한 상수 항목으로 정의한다.
                permanent,
                // 'refreshDuration' 값을 현재 메소드 호출의 인수로 전달한다.
                refreshDuration);
            // [방어 로직] 'status != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (status != null)
            {
                // 'status.SetSourceUnit' 메소드를 호출해 해당 객체의 처리를 실행한다.
                status.SetSourceUnit(source);
            }
            // 직접 보호막과 상태 보호막 표시값을 맞춘다.
            SyncShield(target);
            // 로스터에 등록된 Actor 표시를 갱신한다.
            roster.RefreshActor(target);
            // 상태 비주얼 갱신은 EffectManager에 맡긴다.
            effectManager?.SpawnOrRefreshStatusVisual(target, roster, statusData, status);
            // 계산 또는 조회 결과 'status'을 호출자에게 반환한다.
            return status;
        }

        // 보호막 수신 배율을 적용한 시간제 Shield 상태를 만들고 출처와 시각 효과를 연결한다.
        // 'ApplyShieldStatus' 메소드의 입력과 반환 계약을 선언한다.
        public UnitStatusRuntime ApplyShieldStatus(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'statusData' 매개변수 또는 지역값의 타입을 'StatusEffectData'로 지정한다.
            StatusEffectData statusData,
            // 'shieldAmount' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float shieldAmount,
            // 'durationSeconds' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float durationSeconds,
            // 적용할 보호막 상태 중첩 수를 지정한다.
            int stacks,
            // 허용할 최대 중첩 수를 지정한다.
            int maxStacks,
            // 지속시간 없이 유지할지 지정한다.
            bool permanent,
            // 재적용 시 지속시간을 갱신할지 지정한다.
            bool refreshDuration,
            // 보호막 상태를 발생시킨 유닛을 지정한다.
            BaseUnitRuntimeModel source)
        {
            // 보호막 적용에 필요한 대상과 상태 정의를 확인하고 Nexus를 제외한다.
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind != StatusEffectKind.Shield || target.IsNexus)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var adjustedShieldAmount = Mathf.Max(0f, shieldAmount) * StatusEffectRuntime.ResolveShieldReceivedMultiplier(target);
            // 지역 변수 'status'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var status = target.Statuses.Apply(
                // 'statusData' 열거값을 선택 가능한 상수 항목으로 정의한다.
                statusData,
                // 'stacks' 열거값을 선택 가능한 상수 항목으로 정의한다.
                stacks,
                // 'durationSeconds' 열거값을 선택 가능한 상수 항목으로 정의한다.
                durationSeconds,
                // 'maxStacks' 열거값을 선택 가능한 상수 항목으로 정의한다.
                maxStacks,
                // 'permanent' 열거값을 선택 가능한 상수 항목으로 정의한다.
                permanent,
                // 'refreshDuration' 열거값을 선택 가능한 상수 항목으로 정의한다.
                refreshDuration,
                // 'adjustedShieldAmount' 값을 현재 메소드 호출의 인수로 전달한다.
                adjustedShieldAmount);
            // [방어 로직] 'status != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (status != null)
            {
                // 'status.SetSourceUnit' 메소드를 호출해 해당 객체의 처리를 실행한다.
                status.SetSourceUnit(source);
            }

            // 직접 보호막과 상태 보호막 표시값을 맞춘다.
            SyncShield(target);
            // 로스터에 등록된 Actor 표시를 갱신한다.
            roster.RefreshActor(target);
            // 상태 비주얼 갱신은 EffectManager에 맡긴다.
            effectManager?.SpawnOrRefreshStatusVisual(target, roster, statusData, status);
            // 계산 또는 조회 결과 'status'을 호출자에게 반환한다.
            return status;
        }

        // 영구 상태와 소진 보호막을 제외한 지정 종류 상태의 지속시간과 시각 효과를 연장한다.
        // 'ExtendStatusDuration' 메소드의 입력과 반환 계약을 선언한다.
        public bool ExtendStatusDuration(BaseUnitRuntimeModel target, StatusEffectKind kind, float durationDelta)
        {
            // 연장할 상태와 시간을 확인하고 Nexus를 제외한다.
            if (target == null || target.Statuses == null || kind == StatusEffectKind.None || durationDelta <= 0f || target.IsNexus)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'changed'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var changed = target.Statuses.ExtendDurations(
                // 'kind' 열거값을 선택 가능한 상수 항목으로 정의한다.
                kind,
                // 'durationDelta' 열거값을 선택 가능한 상수 항목으로 정의한다.
                durationDelta,
                // 'status'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                status => status != null && !status.Permanent && (!status.IsShieldStatus || status.RemainingShieldAmount > 0f));
            // '!changed' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!changed)
            {
                // 조건 판단의 부정 결과를 false로 반환한다.
                return false;
            }

            // 직접 보호막과 상태 보호막 표시값을 맞춘다.
            SyncShield(target);
            // 로스터에 등록된 Actor 표시를 갱신한다.
            roster.RefreshActor(target);

            // 지역 변수 'activeStatuses'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var activeStatuses = target.Statuses.ActiveStatuses;
            // 'var i = 0; i < activeStatuses.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < activeStatuses.Count; i++)
            {
                // 지역 변수 'status'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var status = activeStatuses[i];
                // [방어 로직] 'status == null || status.Kind != kind || status.SourceData == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (status == null || status.Kind != kind || status.SourceData == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 상태 비주얼 갱신은 EffectManager에 맡긴다.
                effectManager?.SpawnOrRefreshStatusVisual(target, roster, status.SourceData, status);
            }

            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 일회성 패시브, Trigger 재사용 대기, 주기 카운트, 갱신 타이머를 초기화한다.
        // 'ResetPassiveEffectState' 메소드의 입력과 반환 계약을 선언한다.
        public void ResetPassiveEffectState()
        {
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            appliedOneShotPassiveEffects.Clear();
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            passiveTriggerCooldowns.Clear();
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            passiveTriggerCounts.Clear();
            // 'passiveEffectRefreshRemaining'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            passiveEffectRefreshRemaining = 0f;
        }

        // 날짜 전환 시 코루틴, 입력, 적 AI, 효과 오브젝트, 상태, 보호막 등 일시 전투 상태를 정리한다.
        // Code Builder: 날짜 의미는 StageManager에 두고 전투 상태만 초기화한다.
        public void ResetCombatState()
        {
            // 'StopAllCoroutines' 메소드를 호출해 현재 단계의 처리를 실행한다.
            StopAllCoroutines();
            // 저장된 수동 투사체 입력을 비운다.
            playerCombatControl?.ClearManualInput();
            // 직렬화된 효과 관리자의 런타임 스킬 오브젝트를 모두 정리한다.
            effectManager.ClearRuntimeSkillObjects();

            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            combatStartDispatchedUnits.Clear();
            // 'ResetPassiveEffectState' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ResetPassiveEffectState();

            // 지역 변수 'entries'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entries = roster.Entries;
            // 'var i = 0; i < entries.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < entries.Count; i++)
            {
                // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var entry = entries[i];
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var model = entry != null ? entry.Model : null;
                // [방어 로직] 'model == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (model == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'model is MonsterUnitRuntimeModel monsterModel' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (model is MonsterUnitRuntimeModel monsterModel)
                {
                    // 'MonsterUnitRuntimeStateService.ResetTransientCombatState' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    MonsterUnitRuntimeStateService.ResetTransientCombatState(monsterModel);
                }
                // 'else' 열거값을 선택 가능한 상수 항목으로 정의한다.
                else
                {
                    // [방어 로직][낯선 문법] null 조건 연산자(?.): 대상이 있을 때만 뒤의 멤버를 호출한다.
                    model.Statuses?.Clear();
                    // [방어 로직] 'model.Resources != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                    if (model.Resources != null)
                    {
                        // 'model.Resources.DirectShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
                        model.Resources.DirectShield = 0f;
                        // 'model.Resources.CurrentShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
                        model.Resources.CurrentShield = 0f;
                    }
                }

                // 직접 보호막과 상태 보호막 표시값을 맞춘다.
                SyncShield(model);
                // Actor 표시를 현재 자원 상태로 갱신한다.
                entry.RefreshActor();
            }
        }

        // 패시브 Trigger 키의 재사용 가능 시간을 확인하고 성공 시 다음 준비 시간을 기록한다.
        // 'ConsumePassiveTriggerCooldown' 메소드의 입력과 반환 계약을 선언한다.
        public bool ConsumePassiveTriggerCooldown(string key, float cooldownSeconds)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(key)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(key))
            {
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // 지역 변수 'now'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var now = Time.time;
            // [방어 로직] 'passiveTriggerCooldowns.TryGetValue(key, out var readyAt) && readyAt > now' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (passiveTriggerCooldowns.TryGetValue(key, out var readyAt) && readyAt > now)
            {
                // 조건 판단의 부정 결과를 false로 반환한다.
                return false;
            }

            // 'cooldownSeconds > 0f' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (cooldownSeconds > 0f)
            {
                // 'passiveTriggerCooldowns[key]'에 오른쪽 계산 또는 조회 결과를 저장한다.
                passiveTriggerCooldowns[key] = now + cooldownSeconds;
            }
            // 'else' 열거값을 선택 가능한 상수 항목으로 정의한다.
            else
            {
                // 지정 항목을 컬렉션에서 제거하고 이후 처리 대상에서 제외한다.
                passiveTriggerCooldowns.Remove(key);
            }

            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 패시브 Trigger 발생 횟수를 누적하고 지정 주기마다 한 번 실행을 허용한다.
        // 'ConsumePassiveTriggerCount' 메소드의 입력과 반환 계약을 선언한다.
        public bool ConsumePassiveTriggerCount(string key, int triggerEveryCount)
        {
            // [방어 로직] 'string.IsNullOrWhiteSpace(key) || triggerEveryCount <= 1' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (string.IsNullOrWhiteSpace(key) || triggerEveryCount <= 1)
            {
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            passiveTriggerCounts.TryGetValue(key, out var currentCount);
            // 'currentCount++;' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            currentCount++;
            // 'currentCount < triggerEveryCount' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (currentCount < triggerEveryCount)
            {
                // 'passiveTriggerCounts[key]'에 오른쪽 계산 또는 조회 결과를 저장한다.
                passiveTriggerCounts[key] = currentCount;
                // 조건 판단의 부정 결과를 false로 반환한다.
                return false;
            }

            // 'passiveTriggerCounts[key]'에 오른쪽 계산 또는 조회 결과를 저장한다.
            passiveTriggerCounts[key] = 0;
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // Trigger가 지정한 목표점과 피해 배율로 스킬 실행 시스템에 실행을 요청한다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryExecuteTriggeredSkill' 메소드의 입력과 반환 계약을 선언한다.
        public bool TryExecuteTriggeredSkill(
            // 'casterEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry casterEntry,
            // 'runtime' 매개변수 또는 지역값의 타입을 'SkillRuntimeInstance'로 지정한다.
            SkillRuntimeInstance runtime,
            // 'targetPoint' 매개변수 또는 지역값의 타입을 'Vector2'로 지정한다.
            Vector2 targetPoint,
            // 'hasTargetPoint' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool hasTargetPoint,
            // Trigger가 스킬 피해에 적용할 배율을 지정한다.
            float triggeredDamageMultiplier,
            // 재귀 Trigger 판정에 사용할 원본 스킬 ID를 지정한다.
            string triggerSourceSkillId)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return skillExecution.TryExecuteTriggered(
                // 'casterEntry' 열거값을 선택 가능한 상수 항목으로 정의한다.
                casterEntry,
                // 'runtime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                runtime,
                // 'roster' 열거값을 선택 가능한 상수 항목으로 정의한다.
                roster,
                // 'this' 열거값을 선택 가능한 상수 항목으로 정의한다.
                this,
                // 'logSkillExecutionContracts' 열거값을 선택 가능한 상수 항목으로 정의한다.
                logSkillExecutionContracts,
                // 'targetPoint' 열거값을 선택 가능한 상수 항목으로 정의한다.
                targetPoint,
                // 'hasTargetPoint' 열거값을 선택 가능한 상수 항목으로 정의한다.
                hasTargetPoint,
                // 'triggeredDamageMultiplier' 열거값을 선택 가능한 상수 항목으로 정의한다.
                triggeredDamageMultiplier,
                // 'triggerSourceSkillId' 값을 현재 메소드 호출의 인수로 전달한다.
                triggerSourceSkillId);
        }

        // 지정 시전자와 스킬이 현재 로스터 상태에서 실행 가능한지 확인한다.
        // 'CanExecuteSelectedSkill' 메소드의 입력과 반환 계약을 선언한다.
        public bool CanExecuteSelectedSkill(UnitRosterEntry casterEntry, SkillRuntimeInstance runtime)
        {
            // 계산 또는 조회 결과 'skillExecution.CanExecuteSelected(casterEntry, runtime, roster)'을 호출자에게 반환한다.
            return skillExecution.CanExecuteSelected(casterEntry, runtime, roster);
        }

        // 선택된 스킬을 현재 시간 변화량으로 실행 시스템에 전달한다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryExecuteSelectedSkill' 메소드의 입력과 반환 계약을 선언한다.
        public bool TryExecuteSelectedSkill(
            // 'casterEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry casterEntry,
            // 'runtime' 매개변수 또는 지역값의 타입을 'SkillRuntimeInstance'로 지정한다.
            SkillRuntimeInstance runtime,
            // 'deltaTime' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float deltaTime)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return skillExecution.TryExecuteSelected(
                // 'casterEntry' 열거값을 선택 가능한 상수 항목으로 정의한다.
                casterEntry,
                // 'runtime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                runtime,
                // 'roster' 열거값을 선택 가능한 상수 항목으로 정의한다.
                roster,
                // 'this' 열거값을 선택 가능한 상수 항목으로 정의한다.
                this,
                // 'deltaTime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                deltaTime,
                // 'logSkillExecutionContracts' 값을 현재 메소드 호출의 인수로 전달한다.
                logSkillExecutionContracts);
        }

        // 스킬 시전 이벤트를 SkillTriggerRuntime에 전달해 연결 Trigger를 실행한다.
        // 'DispatchSkillCastTriggers' 메소드의 입력과 반환 계약을 선언한다.
        public void DispatchSkillCastTriggers(
            // 'sourceEntry' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry sourceEntry,
            // 'sourceSkillId' 매개변수 또는 지역값의 타입을 'string'로 지정한다.
            string sourceSkillId,
            // 'eventCenter' 매개변수 또는 지역값의 타입을 'Vector2'로 지정한다.
            Vector2 eventCenter,
            // 재귀 Trigger 판정에 사용할 원본 스킬 ID를 지정한다.
            string triggerSourceSkillId)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var source = sourceEntry != null ? sourceEntry.Model : null;
            // [방어 로직] 'source == null || string.IsNullOrWhiteSpace(sourceSkillId)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (source == null || string.IsNullOrWhiteSpace(sourceSkillId))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SkillTriggerRuntime.ExecuteSkillCast' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillTriggerRuntime.ExecuteSkillCast(this, roster, source, sourceSkillId, eventCenter, triggerSourceSkillId);
        }

        // 유닛별로 최초 등록 시 한 번만 전투 시작 Trigger를 실행한다.
        // 'DispatchCombatStartOnce' 메소드의 입력과 반환 계약을 선언한다.
        private void DispatchCombatStartOnce(BaseUnitRuntimeModel source)
        {
            // [방어 로직] 'source == null || !combatStartDispatchedUnits.Add(source)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (source == null || !combatStartDispatchedUnits.Add(source))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SkillTriggerRuntime.ExecuteCombatStart' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillTriggerRuntime.ExecuteCombatStart(this, roster, source);
        }

        // 고정 간격마다 로스터 유닛의 학습 패시브 효과를 평가하고 적용한다.
        // 'TickLearnedPassiveEffects' 메소드의 입력과 반환 계약을 선언한다.
        private void TickLearnedPassiveEffects(float deltaTime)
        {
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            passiveEffectRefreshRemaining -= Mathf.Max(0f, deltaTime);
            // 'passiveEffectRefreshRemaining > 0f' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (passiveEffectRefreshRemaining > 0f)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'passiveEffectRefreshRemaining'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            passiveEffectRefreshRemaining = PassiveEffectRefreshInterval;
            // 'InGamePassiveEffectRuntime.ApplyLearnedPassiveEffects' 메소드를 호출해 해당 객체의 처리를 실행한다.
            InGamePassiveEffectRuntime.ApplyLearnedPassiveEffects(this, roster, appliedOneShotPassiveEffects);
        }

        // 실제 피해를 준 공격자의 outgoing-damage Trigger와 추가 피해 상태를 실행한다.
        // 'DispatchOutgoingDamageTriggers' 메소드의 입력과 반환 계약을 선언한다.
        private void DispatchOutgoingDamageTriggers(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'options' 매개변수 또는 지역값의 타입을 'DamageApplicationOptions'로 지정한다.
            DamageApplicationOptions options,
            // 'result' 매개변수 또는 지역값의 타입을 'InGameResourceChangeResult'로 지정한다.
            InGameResourceChangeResult result,
            // 'sourceBaseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float sourceBaseDamage)
        {
            // [방어 로직] 'options.Source == null || options.SuppressOutgoingDamageTriggers || result.AppliedDamage <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (options.Source == null || options.SuppressOutgoingDamageTriggers || result.AppliedDamage <= 0f)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SkillTriggerRuntime.ExecuteOutgoingDamage' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillTriggerRuntime.ExecuteOutgoingDamage(
                // 'this' 열거값을 선택 가능한 상수 항목으로 정의한다.
                this,
                // 'roster' 열거값을 선택 가능한 상수 항목으로 정의한다.
                roster,
                // 'options.Source' 값을 현재 메소드 호출의 인수로 전달한다.
                options.Source,
                // 'options.SourceSkillId' 값을 현재 메소드 호출의 인수로 전달한다.
                options.SourceSkillId,
                // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                target,
                // 'attribute' 열거값을 선택 가능한 상수 항목으로 정의한다.
                attribute,
                // 'result.AppliedDamage' 값을 현재 메소드 호출의 인수로 전달한다.
                result.AppliedDamage,
                // 'options.SourceHitWasExecute' 값을 현재 메소드 호출의 인수로 전달한다.
                options.SourceHitWasExecute);

            // 'ApplyOutgoingAdditionalDamageStatuses' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ApplyOutgoingAdditionalDamageStatuses(target, attribute, options, sourceBaseDamage);
        }

        // 대상이 사망했을 때 공격자의 처치 Trigger를 실행한다.
        // 'DispatchKillTriggers' 메소드의 입력과 반환 계약을 선언한다.
        private void DispatchKillTriggers(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'options' 매개변수 또는 지역값의 타입을 'DamageApplicationOptions'로 지정한다.
            DamageApplicationOptions options,
            // 'result' 매개변수 또는 지역값의 타입을 'InGameResourceChangeResult'로 지정한다.
            InGameResourceChangeResult result)
        {
            // [방어 로직] '!result.IsDead || options.Source == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!result.IsDead || options.Source == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'SkillTriggerRuntime.ExecuteKill' 메소드를 호출해 해당 객체의 처리를 실행한다.
            SkillTriggerRuntime.ExecuteKill(
                // 'this' 열거값을 선택 가능한 상수 항목으로 정의한다.
                this,
                // 'roster' 열거값을 선택 가능한 상수 항목으로 정의한다.
                roster,
                // 'options.Source' 값을 현재 메소드 호출의 인수로 전달한다.
                options.Source,
                // 'options.SourceSkillId' 값을 현재 메소드 호출의 인수로 전달한다.
                options.SourceSkillId,
                // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                target,
                // 'attribute' 열거값을 선택 가능한 상수 항목으로 정의한다.
                attribute,
                // 'result.AppliedDamage' 값을 현재 메소드 호출의 인수로 전달한다.
                result.AppliedDamage,
                // 'options.SourceHitWasExecute' 값을 현재 메소드 호출의 인수로 전달한다.
                options.SourceHitWasExecute);
        }

        // 공격자 상태가 제공하는 추가 속성 피해 명세를 원래 피해 기준으로 연쇄 적용한다.
        // 'ApplyOutgoingAdditionalDamageStatuses' 메소드의 입력과 반환 계약을 선언한다.
        private void ApplyOutgoingAdditionalDamageStatuses(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'triggerAttribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute triggerAttribute,
            // 'options' 매개변수 또는 지역값의 타입을 'DamageApplicationOptions'로 지정한다.
            DamageApplicationOptions options,
            // 'sourceBaseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float sourceBaseDamage)
        {
            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (target == null
                // [방어 로직] 앞 조건과 OR로 'options.Source == null' 조건을 추가한다.
                || options.Source == null
                // [방어 로직] 앞 조건과 OR로 'sourceBaseDamage <= 0f' 조건을 추가한다.
                || sourceBaseDamage <= 0f
                // [방어 로직] 앞 조건과 OR로 '(target.Resources != null && target.Resources.CurrentHealth <= 0f))' 조건을 추가한다.
                || (target.Resources != null && target.Resources.CurrentHealth <= 0f))
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'specs'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var specs = StatusEffectRuntime.ResolveOutgoingAdditionalDamageSpecs(options.Source, triggerAttribute);
            // 'var i = 0; i < specs.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < specs.Count; i++)
            {
                // [방어 로직] 'target.Resources != null && target.Resources.CurrentHealth <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (target.Resources != null && target.Resources.CurrentHealth <= 0f)
                {
                    // 'break' 값을 현재 메소드 호출의 인수로 전달한다.
                    break;
                }

                // 지역 변수 'spec'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var spec = specs[i];
                // [방어 로직] 'spec.Multiplier <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (spec.Multiplier <= 0f)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'ApplyDamage' 메소드를 호출해 현재 단계의 처리를 실행한다.
                ApplyDamage(
                    // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    target,
                    // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                    Mathf.Max(0f, sourceBaseDamage) * spec.Multiplier,
                    // 'spec.DamageAttribute' 값을 현재 메소드 호출의 인수로 전달한다.
                    spec.DamageAttribute,
                    // 'options.Source' 값을 현재 메소드 호출의 인수로 전달한다.
                    options.Source,
                    // 'true' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    true,
                    // '0f' 식의 값을 현재 생성자 또는 메소드 호출에 전달한다.
                    0f,
                    // '0f' 식의 값을 현재 생성자 또는 메소드 호출에 전달한다.
                    0f,
                    // 'options.SourceSkillId' 값을 현재 메소드 호출의 인수로 전달한다.
                    options.SourceSkillId,
                    // 'true' 값을 현재 메소드 호출의 인수로 전달한다.
                    true);
            }
        }

        // 문자열 태그 상태의 중첩을 지정 수만큼 소비하고 표시를 갱신한다.
        // 'ConsumeStatusStacks' 메소드의 입력과 반환 계약을 선언한다.
        public int ConsumeStatusStacks(BaseUnitRuntimeModel target, string statusTag, int stacks)
        {
            // [방어 로직] 'target == null || target.Statuses == null || stacks <= 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null || stacks <= 0)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 '0'을 호출자에게 반환한다.
                return 0;
            }

            // 지역 변수 'consumed'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var consumed = target.Statuses.ConsumeStacks(statusTag, stacks);
            // 'consumed > 0' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (consumed > 0)
            {
                // 직접 보호막과 상태 보호막 표시값을 맞춘다.
                SyncShield(target);
                // 로스터에 등록된 Actor 표시를 갱신한다.
                roster.RefreshActor(target);
            }

            // 계산 또는 조회 결과 'consumed'을 호출자에게 반환한다.
            return consumed;
        }

        // Collider Transform을 포함하는 전투 로스터 항목을 찾는다.
        // 'FindUnitByCollider' 메소드의 입력과 반환 계약을 선언한다.
        public UnitRosterEntry FindUnitByCollider(Collider2D collider)
        {
            // [방어 로직] 'collider == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (collider == null)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 지역 변수 'entries'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entries = roster.Entries;
            // 'var i = 0; i < entries.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < entries.Count; i++)
            {
                // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var entry = entries[i];
                // [방어 로직] 'entry == null || entry.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (entry == null || entry.Transform == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'entry.ContainsTransform(collider.transform)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (entry.ContainsTransform(collider.transform))
                {
                    // 계산 또는 조회 결과 'entry'을 호출자에게 반환한다.
                    return entry;
                }
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 화면에 살아 있는 적이 있고 유닛 자동 설정이 허용될 때 플레이어 스킬 자동 실행을 허용한다.
        // 'ShouldAutoRouteSkill' 메소드의 입력과 반환 계약을 선언한다.
        private bool ShouldAutoRouteSkill(UnitRosterEntry entry, SkillRuntimeInstance runtime)
        {
            return playerCombatControl != null
                && playerCombatControl.CanUseAutoSkill(entry, roster);
        }

        // 모든 로스터 유닛의 상태 지속시간을 갱신하고 만료 상태·보호막 Trigger를 실행한다.
        // 'TickUnitStatuses' 메소드의 입력과 반환 계약을 선언한다.
        private void TickUnitStatuses(float deltaTime)
        {
            // [방어 로직] 'deltaTime <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (deltaTime <= 0f)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'entries'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entries = roster.Entries;
            // 'var i = 0; i < entries.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < entries.Count; i++)
            {
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                var model = entries[i] != null ? entries[i].Model : null;
                // [방어 로직] 'model == null || model.Statuses == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (model == null || model.Statuses == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 지역 변수 'removedStatuses'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var removedStatuses = new List<UnitStatusRuntime>();
                // 'model.Statuses.Tick(deltaTime, removedStatuses)' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (model.Statuses.Tick(deltaTime, removedStatuses))
                {
                    // 직접 보호막과 상태 보호막 표시값을 맞춘다.
                    SyncShield(model);
                    // 로스터에 등록된 Actor 표시를 갱신한다.
                    roster.RefreshActor(model);
                    // 'DispatchStatusExpireTriggers' 메소드를 호출해 현재 단계의 처리를 실행한다.
                    DispatchStatusExpireTriggers(model, removedStatuses);
                    // 'DispatchShieldExpireTriggers' 메소드를 호출해 현재 단계의 처리를 실행한다.
                    DispatchShieldExpireTriggers(model, removedStatuses);
                }
            }
        }

        // 피해를 흡수한 각 상태 보호막에 대해 흡수 Trigger를 실행한다.
        // 'DispatchShieldAbsorbTriggers' 메소드의 입력과 반환 계약을 선언한다.
        private void DispatchShieldAbsorbTriggers(
            // 'shieldTarget' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel shieldTarget,
            // 'attacker' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel attacker,
            // 'absorbedShields' 매개변수 또는 지역값의 타입을 'IReadOnlyList<ShieldAbsorbRecord>'로 지정한다.
            IReadOnlyList<ShieldAbsorbRecord> absorbedShields)
        {
            // [방어 로직] 'shieldTarget == null || absorbedShields == null || absorbedShields.Count == 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (shieldTarget == null || absorbedShields == null || absorbedShields.Count == 0)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'var i = 0; i < absorbedShields.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < absorbedShields.Count; i++)
            {
                // 지역 변수 'record'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var record = absorbedShields[i];
                // [방어 로직] 'record.Status == null || record.AbsorbedAmount <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (record.Status == null || record.AbsorbedAmount <= 0f)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'SkillTriggerRuntime.ExecuteShieldAbsorb' 메소드를 호출해 해당 객체의 처리를 실행한다.
                SkillTriggerRuntime.ExecuteShieldAbsorb(this, roster, shieldTarget, attacker, record.Status, record.AbsorbedAmount);
            }
        }

        // 제거된 상태 중 보호막 상태에 대해 만료 Trigger를 실행한다.
        // 'DispatchShieldExpireTriggers' 메소드의 입력과 반환 계약을 선언한다.
        private void DispatchShieldExpireTriggers(BaseUnitRuntimeModel shieldTarget, IReadOnlyList<UnitStatusRuntime> removedStatuses)
        {
            // [방어 로직] 'shieldTarget == null || removedStatuses == null || removedStatuses.Count == 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (shieldTarget == null || removedStatuses == null || removedStatuses.Count == 0)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'var i = 0; i < removedStatuses.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < removedStatuses.Count; i++)
            {
                // 지역 변수 'status'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var status = removedStatuses[i];
                // [방어 로직] 'status == null || !status.IsShieldStatus' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (status == null || !status.IsShieldStatus)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'SkillTriggerRuntime.ExecuteShieldExpire' 메소드를 호출해 해당 객체의 처리를 실행한다.
                SkillTriggerRuntime.ExecuteShieldExpire(this, roster, shieldTarget, status);
            }
        }

        // 제거된 모든 상태에 대해 일반 상태 만료 Trigger를 실행한다.
        // 'DispatchStatusExpireTriggers' 메소드의 입력과 반환 계약을 선언한다.
        private void DispatchStatusExpireTriggers(BaseUnitRuntimeModel statusOwner, IReadOnlyList<UnitStatusRuntime> removedStatuses)
        {
            // [방어 로직] 'statusOwner == null || removedStatuses == null || removedStatuses.Count == 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (statusOwner == null || removedStatuses == null || removedStatuses.Count == 0)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'var i = 0; i < removedStatuses.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < removedStatuses.Count; i++)
            {
                // 지역 변수 'status'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var status = removedStatuses[i];
                // [방어 로직] 'status == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (status == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'SkillTriggerRuntime.ExecuteStatusExpire' 메소드를 호출해 해당 객체의 처리를 실행한다.
                SkillTriggerRuntime.ExecuteStatusExpire(this, roster, statusOwner, status);
            }
        }

        // 자원 변경 결과가 실제로 달라졌을 때 대상 Actor 표시를 갱신한다.
        // 'RefreshActorIfChanged' 메소드의 입력과 반환 계약을 선언한다.
        private void RefreshActorIfChanged(InGameResourceChangeResult result)
        {
            // 'result.Changed' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (result.Changed)
            {
                // 로스터에 등록된 Actor 표시를 갱신한다.
                roster.RefreshActor(result.Target);
            }
        }

        // 사망 대상을 로스터에서 해제하고 넥서스·몬스터·적 Actor 유형에 맞게 패배 또는 제거 처리한다.
        // 'RemoveUnitIfDead' 메소드의 입력과 반환 계약을 선언한다.
        private void RemoveUnitIfDead(InGameResourceChangeResult result)
        {
            // [방어 로직] '!result.Changed || !result.IsDead || result.Target == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!result.Changed || !result.IsDead || result.Target == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Find(result.Target);
            // [방어 로직] 'entry == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 'roster.Unregister' 메소드를 호출해 해당 객체의 처리를 실행한다.
            roster.Unregister(result.Target);
            // 등록된 Actor가 자신의 패배 연출 또는 Nexus 패배 통지를 처리한다.
            entry.ShowDefeated();
        }

    }

    // 피해·보호막·회복 전후 자원 값과 사망 여부를 전달하는 불변 결과다.
    // 'InGameResourceChangeResult' 값 형식 구조체 정의를 시작한다.
    public readonly struct InGameResourceChangeResult
    {
        // 자원 변경 전후 값과 적용 피해, 사망 여부를 결과로 구성한다.
        // 'InGameResourceChangeResult' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'previousHealth' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float previousHealth,
            // 'currentHealth' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float currentHealth,
            // 'previousShield' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float previousShield,
            // 'currentShield' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float currentShield,
            // 'appliedDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float appliedDamage,
            // 'isDead' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool isDead)
        {
            // 'Target'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            Target = target;
            // 'PreviousHealth'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PreviousHealth = previousHealth;
            // 'CurrentHealth'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            CurrentHealth = currentHealth;
            // 'PreviousShield'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            PreviousShield = previousShield;
            // 'CurrentShield'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            CurrentShield = currentShield;
            // 'AppliedDamage'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            AppliedDamage = appliedDamage;
            // 'IsDead'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            IsDead = isDead;
        }

        // 'Target' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public BaseUnitRuntimeModel Target { get; }
        // 'PreviousHealth' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float PreviousHealth { get; }
        // 'CurrentHealth' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float CurrentHealth { get; }
        // 'PreviousShield' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float PreviousShield { get; }
        // 'CurrentShield' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float CurrentShield { get; }
        // 'AppliedDamage' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public float AppliedDamage { get; }
        // 'IsDead' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public bool IsDead { get; }
        // [낯선 문법] 식 본문 property: 'Changed' 값을 오른쪽 식 하나로 계산해 반환한다.
        public bool Changed =>
            // '!Mathf.Approximately(PreviousHealth, CurrentHealth)' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
            !Mathf.Approximately(PreviousHealth, CurrentHealth)
            // [방어 로직] 앞 조건과 OR로 '!Mathf.Approximately(PreviousShield, CurrentShield);' 조건을 추가한다.
            || !Mathf.Approximately(PreviousShield, CurrentShield);

        // 대상의 현재 자원 값을 전후 동일하게 사용한 변경 없음 결과를 만든다.
        // 'Unchanged' 메소드의 입력과 반환 계약을 선언한다.
        public static InGameResourceChangeResult Unchanged(BaseUnitRuntimeModel target)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var resources = target != null ? target.Resources : null;
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var health = resources != null ? Mathf.Max(0f, resources.CurrentHealth) : 0f;
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var shield = resources != null ? Mathf.Max(0f, resources.CurrentShield) : 0f;
            // 계산 또는 조회 결과 'new InGameResourceChangeResult(target, health, health, shield, shield, 0f, health <= 0f)'을 호출자에게 반환한다.
            return new InGameResourceChangeResult(target, health, health, shield, shield, 0f, health <= 0f);
        }
    }
}
