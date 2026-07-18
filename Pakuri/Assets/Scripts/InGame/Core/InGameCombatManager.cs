// 'System' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System;
// 'System.Collections.Generic' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using System.Collections.Generic;
// 'Pakuri.Combat' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using Pakuri.Combat;
// 'AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;
// 'UnityEngine' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine;
// 'UnityEngine.EventSystems' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine.EventSystems;
// 'UnityEngine.InputSystem' 네임스페이스의 타입과 API를 이 파일에서 사용한다.
using UnityEngine.InputSystem;

// 'Pakuri.InGame' 네임스페이스 범위를 선언해 관련 타입 이름의 충돌을 막는다.
namespace Pakuri.InGame
{
    // 피해 적용 시 공격자, 치명타, 스킬 출처, Trigger 억제, 피해량 표시 정보를 전달한다.
    // 'DamageApplicationOptions' 값 형식 구조체 정의를 시작한다.
    public readonly struct DamageApplicationOptions
    {
        // 한 번의 피해 적용에 필요한 선택 옵션을 불변 값으로 구성한다.
        // 'DamageApplicationOptions' 메소드의 입력과 반환 계약을 선언한다.
        public DamageApplicationOptions(
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
            // [Fallback][낯선 문법] 선택 인수 'damageMeterSourceId'가 생략되면 기본값 'null'을 사용한다.
            string damageMeterSourceId = null,
            // [Fallback][낯선 문법] 선택 인수 'damageMeterDisplayName'가 생략되면 기본값 'null'을 사용한다.
            string damageMeterDisplayName = null)
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
            // 'DamageMeterDisplayName'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            DamageMeterDisplayName = damageMeterDisplayName;
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
        // 'DamageMeterDisplayName' 읽기 전용 property로 계산 결과 또는 상태를 외부에 공개한다.
        public string DamageMeterDisplayName { get; }
    }

    // 전투 로스터, 스킬 실행, 적 AI, 피해·회복·상태, Trigger, 수동 입력을 통합 관리한다.
    // [낯선 문법] DisallowMultipleComponent attribute: 같은 GameObject에 이 컴포넌트가 중복 부착되는 것을 막는다.
    [DisallowMultipleComponent]
    // 'InGameCombatManager' 클래스 정의를 시작한다.
    public sealed class InGameCombatManager : MonoBehaviour
    {
        // 'PassiveEffectRefreshInterval' 상수에 실행 중 바뀌지 않는 기준값을 선언한다.
        private const float PassiveEffectRefreshInterval = 0.25f;

        // [낯선 문법] readonly 필드 'roster'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly UnitRosterService roster = new UnitRosterService();
        // [낯선 문법] readonly 필드 'enemyCombatSystem'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly EnemyCombatSystem enemyCombatSystem = new EnemyCombatSystem();
        // [낯선 문법] readonly 필드 'resourceMutations'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly UnitResourceMutationService resourceMutations = new UnitResourceMutationService();
        // [낯선 문법] readonly 필드 'skillExecution'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();
        // [낯선 문법] readonly 필드 'statusEffectVisuals'를 초기화하며, 생성 뒤에는 이 참조를 다시 대입할 수 없다.
        private readonly Dictionary<string, GameObject> statusEffectVisuals = new Dictionary<string, GameObject>();
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
        // 'hasLatchedManualProjectileInput' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private bool hasLatchedManualProjectileInput;
        // 'latchedManualProjectileAimDirection' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private Vector2 latchedManualProjectileAimDirection;
        // 'latchedManualProjectileTargetPoint' 필드 또는 property를 선언해 객체 상태를 보관하거나 공개한다.
        private Vector2 latchedManualProjectileTargetPoint;

        // [낯선 문법] SerializeField attribute: private 상태 'enemyCombatSimulationEnabled'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        // [낯선 문법] SerializeField attribute: private 상태 'skillExecutionEnabled'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool skillExecutionEnabled = true;
        // [낯선 문법] SerializeField attribute: private 상태 'logEnemyAttackAttempts'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool logEnemyAttackAttempts;
        // [낯선 문법] SerializeField attribute: private 상태 'logSkillExecutionContracts'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool logSkillExecutionContracts;
        // [낯선 문법] SerializeField attribute: private 상태 'playerAutoSkillEnabled'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private bool playerAutoSkillEnabled;
        // [낯선 문법] SerializeField attribute: private 상태 'inputCamera'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Camera inputCamera;
        // [낯선 문법] SerializeField attribute: private 상태 'projectileDestroyBoundary'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private Transform projectileDestroyBoundary;
        // [낯선 문법] SerializeField attribute: private 상태 'effectManager'을 Unity 직렬화와 Inspector 편집 대상으로 만든다.
        [SerializeField] private EffectManager effectManager;

        // [낯선 문법] 식 본문 property: 'Roster' 값을 오른쪽 식 하나로 계산해 반환한다.
        public UnitRosterService Roster => roster;
        // [낯선 문법] 식 본문 property: 'Effects' 값을 오른쪽 식 하나로 계산해 반환한다.
        public EffectManager Effects => effectManager;

        // [낯선 문법] 식 본문 property: 'ActiveUnitCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int ActiveUnitCount => roster.Count;
        // [낯선 문법] 식 본문 property: 'ActivePlayerCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int ActivePlayerCount => roster.PlayerCount;
        // [낯선 문법] 식 본문 property: 'ActiveEnemyCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int ActiveEnemyCount => roster.EnemyCount;
        // [낯선 문법] 식 본문 property: 'LastEnemyAttackAttemptCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int LastEnemyAttackAttemptCount => enemyCombatSystem.LastAttackAttemptCount;
        // [낯선 문법] 식 본문 property: 'LastSkillExecutionRoutedCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int LastSkillExecutionRoutedCount => skillExecution.LastRoutedCount;
        // [낯선 문법] 식 본문 property: 'LastSkillExecutionRejectedCount' 값을 오른쪽 식 하나로 계산해 반환한다.
        public int LastSkillExecutionRejectedCount => skillExecution.LastRejectedCount;
        // [낯선 문법] 식 본문 property: 'PlayerAutoSkillEnabled' 값을 오른쪽 식 하나로 계산해 반환한다.
        public bool PlayerAutoSkillEnabled => playerAutoSkillEnabled;

        // 전투 시작 전 로스터, 적 상태, 전투 시작 Trigger 기록, 패시브 상태를 초기화한다.
        // 'Awake' 메소드의 입력과 반환 계약을 선언한다.
        private void Awake()
        {
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            roster.Clear();
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            enemyCombatSystem.Clear();
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
                // 'HandleSelectedPlayerManualSkillInput' 메소드를 호출해 현재 단계의 처리를 실행한다.
                HandleSelectedPlayerManualSkillInput();
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
        public UnitRosterEntry RegisterPlayerMonster(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot = null)
        {
            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Register(model, actor, hitboxRoot);
            // 'IsSelectedPlayerModel(model)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (IsSelectedPlayerModel(model))
            {
                // 'SetSelectedPlayerAutoSkillMode' 메소드를 호출해 현재 단계의 처리를 실행한다.
                SetSelectedPlayerAutoSkillMode(playerAutoSkillEnabled);
            }

            // 'DispatchCombatStartOnce' 메소드를 호출해 현재 단계의 처리를 실행한다.
            DispatchCombatStartOnce(model);
            // 계산 또는 조회 결과 'entry'을 호출자에게 반환한다.
            return entry;
        }

        // 적을 로스터에 등록하고 해당 유닛의 전투 시작 Trigger를 한 번 실행한다.
        // 'RegisterEnemy' 메소드의 입력과 반환 계약을 선언한다.
        public UnitRosterEntry RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot = null)
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
        public UnitRosterEntry RegisterNexus(NexusUnitRuntimeModel model, NexusUnitActor actor, Transform hitboxRoot = null)
        {
            // 계산 또는 조회 결과 'roster.Register(model, actor, hitboxRoot)'을 호출자에게 반환한다.
            return roster.Register(model, actor, hitboxRoot);
        }

        // 모델을 로스터에서 해제하고 연결된 Actor GameObject를 지정 지연 후 제거한다.
        // 'DespawnUnit' 메소드의 입력과 반환 계약을 선언한다.
        public bool DespawnUnit(BaseUnitRuntimeModel model, float destroyDelay = 0f)
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
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                Destroy(actor.gameObject, Mathf.Max(0f, destroyDelay));
            }

            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // GameObject 제거 없이 지정 모델만 전투 로스터에서 해제한다.
        // 'UnregisterUnit' 메소드의 입력과 반환 계약을 선언한다.
        public bool UnregisterUnit(BaseUnitRuntimeModel model)
        {
            // 계산 또는 조회 결과 'roster.Unregister(model)'을 호출자에게 반환한다.
            return roster.Unregister(model);
        }

        // 공격자 정보 없이 기본 옵션으로 대상에게 피해를 적용한다.
        // 'ApplyDamage' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult ApplyDamage(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDamage,
            // [Fallback][낯선 문법] 선택 인수 'attribute'가 생략되면 기본값 'DamageAttribute.Physical'을 사용한다.
            DamageAttribute attribute = DamageAttribute.Physical)
        {
            // 계산 또는 조회 결과 'ApplyDamage(target, baseDamage, attribute, null)'을 호출자에게 반환한다.
            return ApplyDamage(target, baseDamage, attribute, null);
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
            // [Fallback][낯선 문법] 선택 인수 'damageMeterSourceId'가 생략되면 기본값 'null'을 사용한다.
            string damageMeterSourceId = null,
            // [Fallback][낯선 문법] 선택 인수 'damageMeterDisplayName'가 생략되면 기본값 'null'을 사용한다.
            string damageMeterDisplayName = null)
        {
            // 지역 변수 'depletedShields'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var depletedShields = new List<UnitStatusRuntime>();
            // 지역 변수 'absorbedShields'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var absorbedShields = new List<ShieldAbsorbRecord>();
            // 지역 변수 'options'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var options = new DamageApplicationOptions(source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers, sourceHitWasExecute, damageMeterSourceId, damageMeterDisplayName);
            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = resourceMutations.ApplyDamage(target, baseDamage, attribute, options, depletedShields, absorbedShields);
            // 'DamageMeterRuntimeTracker.RecordDamage' 메소드를 호출해 해당 객체의 처리를 실행한다.
            DamageMeterRuntimeTracker.RecordDamage(options, result);
            // 'RefreshActorIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshActorIfChanged(result);
            // 'ShowDamageIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ShowDamageIfChanged(result);
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

        // 넥서스를 제외한 대상에게 기존 값에 더해 직접 보호막을 부여하고 Actor를 갱신한다.
        // 'GrantShield' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult GrantShield(BaseUnitRuntimeModel target, float amount)
        {
            // 'IsNexusModel(target)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (IsNexusModel(target))
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = resourceMutations.GrantShield(target, amount);
            // 'RefreshActorIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshActorIfChanged(result);
            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 넥서스를 제외한 대상의 직접 보호막을 지정 값으로 설정하고 Actor를 갱신한다.
        // 'SetShield' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult SetShield(BaseUnitRuntimeModel target, float amount)
        {
            // 'IsNexusModel(target)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (IsNexusModel(target))
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = resourceMutations.SetShield(target, amount);
            // 'RefreshActorIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshActorIfChanged(result);
            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 넥서스를 제외한 대상의 체력을 최대 체력 범위에서 회복하고 Actor를 갱신한다.
        // 'Heal' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult Heal(BaseUnitRuntimeModel target, float amount)
        {
            // 'IsNexusModel(target)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (IsNexusModel(target))
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'result'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var result = resourceMutations.Heal(target, amount);
            // 'RefreshActorIfChanged' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshActorIfChanged(result);
            // 계산 또는 조회 결과 'result'을 호출자에게 반환한다.
            return result;
        }

        // 문자열 상태 태그를 상태 종류로 변환해 대상에게 적용한다.
        // 'ApplyStatus' 메소드의 입력과 반환 계약을 선언한다.
        public UnitStatusRuntime ApplyStatus(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'statusTag' 매개변수 또는 지역값의 타입을 'string'로 지정한다.
            string statusTag,
            // 'stacks' 매개변수 또는 지역값의 타입을 'int'로 지정한다.
            int stacks,
            // 'durationSeconds' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float durationSeconds,
            // [Fallback][낯선 문법] 선택 인수 'maxStacks'가 생략되면 기본값 '0'을 사용한다.
            int maxStacks = 0,
            // [Fallback][낯선 문법] 선택 인수 'permanent'가 생략되면 기본값 'false'을 사용한다.
            bool permanent = false,
            // [Fallback][낯선 문법] 선택 인수 'refreshDuration'가 생략되면 기본값 'true'을 사용한다.
            bool refreshDuration = true)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return StatusEffectUtility.TryParse(statusTag, out var kind)
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'ApplyStatus(target, kind, stacks, durationSeconds, maxStacks, permanent, refreshDuration)' 값을 선택한다.
                ? ApplyStatus(target, kind, stacks, durationSeconds, maxStacks, permanent, refreshDuration)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'null;' 값을 선택한다.
                : null;
        }

        // 상태 종류와 중첩·지속시간 규칙을 적용하고 보호막·Actor·시각 효과를 갱신한다.
        // 'ApplyStatus' 메소드의 입력과 반환 계약을 선언한다.
        public UnitStatusRuntime ApplyStatus(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'kind' 매개변수 또는 지역값의 타입을 'StatusEffectKind'로 지정한다.
            StatusEffectKind kind,
            // 'stacks' 매개변수 또는 지역값의 타입을 'int'로 지정한다.
            int stacks,
            // 'durationSeconds' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float durationSeconds,
            // [Fallback][낯선 문법] 선택 인수 'maxStacks'가 생략되면 기본값 '0'을 사용한다.
            int maxStacks = 0,
            // [Fallback][낯선 문법] 선택 인수 'permanent'가 생략되면 기본값 'false'을 사용한다.
            bool permanent = false,
            // [Fallback][낯선 문법] 선택 인수 'refreshDuration'가 생략되면 기본값 'true'을 사용한다.
            bool refreshDuration = true)
        {
            // [방어 로직] 'target == null || target.Statuses == null || kind == StatusEffectKind.None || IsNexusModel(target)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null || kind == StatusEffectKind.None || IsNexusModel(target))
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 지역 변수 'status'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var status = target.Statuses.Apply(
                // 'kind' 열거값을 선택 가능한 상수 항목으로 정의한다.
                kind,
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
            // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
            resourceMutations.SynchronizeShieldView(target);
            // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshUnitActor(target);
            // 'SpawnOrRefreshStatusEffectVisual' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SpawnOrRefreshStatusEffectVisual(target, StatusEffectRuntime.CreateStatusData(kind, null), status);
            // 계산 또는 조회 결과 'status'을 호출자에게 반환한다.
            return status;
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
            // [Fallback][낯선 문법] 선택 인수 'maxStacks'가 생략되면 기본값 '0'을 사용한다.
            int maxStacks = 0,
            // [Fallback][낯선 문법] 선택 인수 'permanent'가 생략되면 기본값 'false'을 사용한다.
            bool permanent = false,
            // [Fallback][낯선 문법] 선택 인수 'refreshDuration'가 생략되면 기본값 'true'을 사용한다.
            bool refreshDuration = true,
            // [Fallback][낯선 문법] 선택 인수 'source'가 생략되면 기본값 'null'을 사용한다.
            BaseUnitRuntimeModel source = null)
        {
            // [방어 로직] 'target == null || target.Statuses == null || statusData == null || statusData.Kind == StatusEffectKind.None || IsNexusModel(target)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind == StatusEffectKind.None || IsNexusModel(target))
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
            // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
            resourceMutations.SynchronizeShieldView(target);
            // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshUnitActor(target);
            // 'SpawnOrRefreshStatusEffectVisual' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SpawnOrRefreshStatusEffectVisual(target, statusData, status);
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
            // [Fallback][낯선 문법] 선택 인수 'stacks'가 생략되면 기본값 '1'을 사용한다.
            int stacks = 1,
            // [Fallback][낯선 문법] 선택 인수 'maxStacks'가 생략되면 기본값 '0'을 사용한다.
            int maxStacks = 0,
            // [Fallback][낯선 문법] 선택 인수 'permanent'가 생략되면 기본값 'false'을 사용한다.
            bool permanent = false,
            // [Fallback][낯선 문법] 선택 인수 'refreshDuration'가 생략되면 기본값 'true'을 사용한다.
            bool refreshDuration = true,
            // [Fallback][낯선 문법] 선택 인수 'source'가 생략되면 기본값 'null'을 사용한다.
            BaseUnitRuntimeModel source = null)
        {
            // [방어 로직] 'target == null || target.Statuses == null || statusData == null || statusData.Kind != StatusEffectKind.Shield || IsNexusModel(target)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null || statusData == null || statusData.Kind != StatusEffectKind.Shield || IsNexusModel(target))
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

            // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
            resourceMutations.SynchronizeShieldView(target);
            // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshUnitActor(target);
            // 'SpawnOrRefreshStatusEffectVisual' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SpawnOrRefreshStatusEffectVisual(target, statusData, status);
            // 계산 또는 조회 결과 'status'을 호출자에게 반환한다.
            return status;
        }

        // 문자열 상태 태그를 변환해 해당 상태들의 지속시간 연장을 요청한다.
        // 'ExtendStatusDuration' 메소드의 입력과 반환 계약을 선언한다.
        public bool ExtendStatusDuration(BaseUnitRuntimeModel target, string statusTag, float durationDelta)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return StatusEffectUtility.TryParse(statusTag, out var kind)
                // 앞 조건과 AND로 'ExtendStatusDuration(target, kind, durationDelta);' 조건을 추가한다.
                && ExtendStatusDuration(target, kind, durationDelta);
        }

        // 영구 상태와 소진 보호막을 제외한 지정 종류 상태의 지속시간과 시각 효과를 연장한다.
        // 'ExtendStatusDuration' 메소드의 입력과 반환 계약을 선언한다.
        public bool ExtendStatusDuration(BaseUnitRuntimeModel target, StatusEffectKind kind, float durationDelta)
        {
            // [방어 로직] 'target == null || target.Statuses == null || kind == StatusEffectKind.None || durationDelta <= 0f || IsNexusModel(target)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null || kind == StatusEffectKind.None || durationDelta <= 0f || IsNexusModel(target))
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

            // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
            resourceMutations.SynchronizeShieldView(target);
            // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
            RefreshUnitActor(target);

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

                // 'SpawnOrRefreshStatusEffectVisual' 메소드를 호출해 현재 단계의 처리를 실행한다.
                SpawnOrRefreshStatusEffectVisual(target, status.SourceData, status);
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
        // 'ResetTransientCombatStateForNextDay' 메소드의 입력과 반환 계약을 선언한다.
        public void ResetTransientCombatStateForNextDay()
        {
            // 'StopAllCoroutines' 메소드를 호출해 현재 단계의 처리를 실행한다.
            StopAllCoroutines();
            // 'ClearLatchedManualProjectileInput' 메소드를 호출해 현재 단계의 처리를 실행한다.
            ClearLatchedManualProjectileInput();
            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            enemyCombatSystem.Clear();

            // 직렬화된 효과 관리자의 런타임 스킬 오브젝트를 모두 정리한다.
            effectManager.ClearRuntimeSkillObjects();

            // 컬렉션에 남은 항목을 모두 제거해 상태를 초기화한다.
            statusEffectVisuals.Clear();
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

                // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                resourceMutations.SynchronizeShieldView(model);
                // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RefreshUnitActor(entry);
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
            // [Fallback][낯선 문법] 선택 인수 'triggeredDamageMultiplier'가 생략되면 기본값 '1f'을 사용한다.
            float triggeredDamageMultiplier = 1f,
            // [Fallback][낯선 문법] 선택 인수 'triggerSourceSkillId'가 생략되면 기본값 'null'을 사용한다.
            string triggerSourceSkillId = null)
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
            // [Fallback][낯선 문법] 선택 인수 'triggerSourceSkillId'가 생략되면 기본값 'null'을 사용한다.
            string triggerSourceSkillId = null)
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

        // 대상이 문자열 태그에 해당하는 상태를 보유했는지 확인한다.
        // 'HasStatus' 메소드의 입력과 반환 계약을 선언한다.
        public bool HasStatus(BaseUnitRuntimeModel target, string statusTag)
        {
            // 계산 또는 조회 결과 'target != null && target.Statuses != null && target.Statuses.Has(statusTag)'을 호출자에게 반환한다.
            return target != null && target.Statuses != null && target.Statuses.Has(statusTag);
        }

        // 대상이 지정 종류의 상태를 보유했는지 확인한다.
        // 'HasStatus' 메소드의 입력과 반환 계약을 선언한다.
        public bool HasStatus(BaseUnitRuntimeModel target, StatusEffectKind kind)
        {
            // 계산 또는 조회 결과 'target != null && target.Statuses != null && target.Statuses.Has(kind)'을 호출자에게 반환한다.
            return target != null && target.Statuses != null && target.Statuses.Has(kind);
        }

        // 문자열 태그 상태의 현재 중첩 수를 반환한다.
        // 'GetStatusStacks' 메소드의 입력과 반환 계약을 선언한다.
        public int GetStatusStacks(BaseUnitRuntimeModel target, string statusTag)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건 결과에 맞는 값 하나를 반환한다.
            return target != null && target.Statuses != null ? target.Statuses.GetStacks(statusTag) : 0;
        }

        // 지정 종류 상태의 현재 중첩 수를 반환한다.
        // 'GetStatusStacks' 메소드의 입력과 반환 계약을 선언한다.
        public int GetStatusStacks(BaseUnitRuntimeModel target, StatusEffectKind kind)
        {
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건 결과에 맞는 값 하나를 반환한다.
            return target != null && target.Statuses != null ? target.Statuses.GetStacks(kind) : 0;
        }

        // 문자열 태그 상태를 제거하고 보호막 표시와 Actor를 갱신한다.
        // 'RemoveStatus' 메소드의 입력과 반환 계약을 선언한다.
        public bool RemoveStatus(BaseUnitRuntimeModel target, string statusTag)
        {
            // [방어 로직] 'target == null || target.Statuses == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'removed'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var removed = target.Statuses.Remove(statusTag);
            // 'removed' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (removed)
            {
                // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                resourceMutations.SynchronizeShieldView(target);
                // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RefreshUnitActor(target);
            }

            // 계산 또는 조회 결과 'removed'을 호출자에게 반환한다.
            return removed;
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
                // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                resourceMutations.SynchronizeShieldView(target);
                // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RefreshUnitActor(target);
            }

            // 계산 또는 조회 결과 'consumed'을 호출자에게 반환한다.
            return consumed;
        }

        // 지정 종류 상태의 중첩을 지정 수만큼 소비하고 표시를 갱신한다.
        // 'ConsumeStatusStacks' 메소드의 입력과 반환 계약을 선언한다.
        public int ConsumeStatusStacks(BaseUnitRuntimeModel target, StatusEffectKind kind, int stacks)
        {
            // [방어 로직] 'target == null || target.Statuses == null || stacks <= 0' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null || stacks <= 0)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 '0'을 호출자에게 반환한다.
                return 0;
            }

            // 지역 변수 'consumed'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var consumed = target.Statuses.ConsumeStacks(kind, stacks);
            // 'consumed > 0' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (consumed > 0)
            {
                // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                resourceMutations.SynchronizeShieldView(target);
                // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RefreshUnitActor(target);
            }

            // 계산 또는 조회 결과 'consumed'을 호출자에게 반환한다.
            return consumed;
        }

        // 지정 종류 상태를 제거하고 보호막 표시와 Actor를 갱신한다.
        // 'RemoveStatus' 메소드의 입력과 반환 계약을 선언한다.
        public bool RemoveStatus(BaseUnitRuntimeModel target, StatusEffectKind kind)
        {
            // [방어 로직] 'target == null || target.Statuses == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Statuses == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'removed'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var removed = target.Statuses.Remove(kind);
            // 'removed' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (removed)
            {
                // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                resourceMutations.SynchronizeShieldView(target);
                // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RefreshUnitActor(target);
            }

            // 계산 또는 조회 결과 'removed'을 호출자에게 반환한다.
            return removed;
        }

        // 선택 플레이어 몬스터의 자동 스킬 모드를 활성화한다.
        // 'EnablePlayerAutoSkillMode' 메소드의 입력과 반환 계약을 선언한다.
        public void EnablePlayerAutoSkillMode()
        {
            // 'SetSelectedPlayerAutoSkillMode' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetSelectedPlayerAutoSkillMode(true);
        }

        // 선택 플레이어 몬스터의 자동 스킬 모드를 현재 값의 반대로 전환한다.
        // 'ToggleSelectedPlayerAutoSkillMode' 메소드의 입력과 반환 계약을 선언한다.
        public void ToggleSelectedPlayerAutoSkillMode()
        {
            // 'SetSelectedPlayerAutoSkillMode' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SetSelectedPlayerAutoSkillMode(!playerAutoSkillEnabled);
        }

        // 관리자 설정과 선택 플레이어 모델의 자동 스킬 허용 값을 함께 변경한다.
        // 'SetSelectedPlayerAutoSkillMode' 메소드의 입력과 반환 계약을 선언한다.
        public void SetSelectedPlayerAutoSkillMode(bool enabled)
        {
            // 'playerAutoSkillEnabled'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            playerAutoSkillEnabled = enabled;
            // 지역 변수 'player'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var player = GetSelectedPlayerEntry();
            // [방어 로직] 'player != null && player.Model != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (player != null && player.Model != null)
            {
                // 'player.Model.AutoSkillEnabled'에 오른쪽 계산 또는 조회 결과를 저장한다.
                player.Model.AutoSkillEnabled = enabled;
            }
        }

        // 직렬화된 투사체 제거 경계 Transform의 X 좌표를 반환한다.
        // 'ResolveProjectileDestroyBoundaryX' 메소드의 입력과 반환 계약을 선언한다.
        public float ResolveProjectileDestroyBoundaryX()
        {
            // 직렬화된 제거 경계의 X 좌표를 호출자에게 반환한다.
            return projectileDestroyBoundary.position.x;
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

        // 모델에 대응하는 로스터 항목을 찾아 Actor 표시를 갱신한다.
        // 'RefreshUnitActor' 메소드의 입력과 반환 계약을 선언한다.
        public bool RefreshUnitActor(BaseUnitRuntimeModel model)
        {
            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Find(model);
            // 계산 또는 조회 결과 'RefreshUnitActor(entry)'을 호출자에게 반환한다.
            return RefreshUnitActor(entry);
        }

        // 자동 모드가 꺼진 1P의 마우스 입력을 스킬별 수동 조준·연속 발사 실행으로 전달한다.
        // 'HandleSelectedPlayerManualSkillInput' 메소드의 입력과 반환 계약을 선언한다.
        private void HandleSelectedPlayerManualSkillInput()
        {
            // 'playerAutoSkillEnabled' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (playerAutoSkillEnabled)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'player'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var player = GetSelectedPlayerEntry();
            // [방어 로직] 'player == null || player.Model == null || player.Model.SkillRuntime == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (player == null || player.Model == null || player.Model.SkillRuntime == null)
            {
                // 'ClearLatchedManualProjectileInput' 메소드를 호출해 현재 단계의 처리를 실행한다.
                ClearLatchedManualProjectileInput();
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'mousePressedThisFrame'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var mousePressedThisFrame = IsPrimaryMousePressedThisFrame();
            // 지역 변수 'mouseHeld'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var mouseHeld = IsPrimaryMouseHeld();
            // 지역 변수 'pointerOverUi'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var pointerOverUi = IsPointerOverUi();
            // 지역 변수 'hasCurrentManualInput'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var hasCurrentManualInput = TryResolveCurrentManualInput(
                // 'player' 열거값을 선택 가능한 상수 항목으로 정의한다.
                player,
                // 'mousePressedThisFrame || mouseHeld' 식의 값을 현재 생성자 또는 메소드 호출에 전달한다.
                mousePressedThisFrame || mouseHeld,
                // 'pointerOverUi' 열거값을 선택 가능한 상수 항목으로 정의한다.
                pointerOverUi,
                // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                out var currentAimDirection,
                // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                out var currentTargetPoint);
            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (!hasCurrentManualInput
                // 앞 조건과 AND로 '!HasProjectileBursting(player.Model.SkillRuntime.ActiveSkills))' 조건을 추가한다.
                && !HasProjectileBursting(player.Model.SkillRuntime.ActiveSkills))
            {
                // 'ClearLatchedManualProjectileInput' 메소드를 호출해 현재 단계의 처리를 실행한다.
                ClearLatchedManualProjectileInput();
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'activeSkills'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var activeSkills = player.Model.SkillRuntime.ActiveSkills;
            // 'var i = 0; i < activeSkills.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < activeSkills.Count; i++)
            {
                // 지역 변수 'runtime'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var runtime = activeSkills[i];
                // [방어 로직] 'runtime == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (runtime == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // [낯선 문법] is 패턴으로 런타임 타입을 검사하고 필요하면 해당 타입 변수로 받는다.
                var isProjectile = runtime.Data is ProjectileSkillData;
                //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
                if (!TryResolveManualSkillInputForRuntime(
                        // 'runtime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        runtime,
                        // 'isProjectile' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        isProjectile,
                        // 'mousePressedThisFrame' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        mousePressedThisFrame,
                        // 'mouseHeld' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        mouseHeld,
                        // 'hasCurrentManualInput' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        hasCurrentManualInput,
                        // 'currentAimDirection' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        currentAimDirection,
                        // 'currentTargetPoint' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        currentTargetPoint,
                        // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                        out var aimDirection,
                        // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
                        out var targetPoint))
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 'skillExecution.TryExecuteManual' 메소드를 호출해 해당 객체의 처리를 실행한다.
                skillExecution.TryExecuteManual(
                    // 'player' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    player,
                    // 'runtime' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    runtime,
                    // 'roster' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    roster,
                    // 'this' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    this,
                    // 'Time.deltaTime' 값을 현재 메소드 호출의 인수로 전달한다.
                    Time.deltaTime,
                    // 'aimDirection' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    aimDirection,
                    // 'targetPoint' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    targetPoint,
                    // 'logSkillExecutionContracts' 값을 현재 메소드 호출의 인수로 전달한다.
                    logSkillExecutionContracts);
            }

            // '!mouseHeld && !HasProjectileBursting(activeSkills)' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!mouseHeld && !HasProjectileBursting(activeSkills))
            {
                // 'ClearLatchedManualProjectileInput' 메소드를 호출해 현재 단계의 처리를 실행한다.
                ClearLatchedManualProjectileInput();
            }
        }

        // 활성 상태의 프리팹 또는 런타임 시각 정의를 생성하고 대상 Transform에 부착해 수명을 갱신한다.
        // 'SpawnOrRefreshStatusEffectVisual' 메소드의 입력과 반환 계약을 선언한다.
        private void SpawnOrRefreshStatusEffectVisual(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'statusData' 매개변수 또는 지역값의 타입을 'StatusEffectData'로 지정한다.
            StatusEffectData statusData,
            // 'status' 매개변수 또는 지역값의 타입을 'UnitStatusRuntime'로 지정한다.
            UnitStatusRuntime status)
        {
            //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
            if (target == null
                // [방어 로직] 앞 조건과 OR로 'statusData == null' 조건을 추가한다.
                || statusData == null
                // [방어 로직] 앞 조건과 OR로 '(!RuntimeSkillVisualFactory.HasVisual(statusData.RuntimeVisual) && statusData.StatusEffectPrefab == null)' 조건을 추가한다.
                || (!RuntimeSkillVisualFactory.HasVisual(statusData.RuntimeVisual) && statusData.StatusEffectPrefab == null)
                // [방어 로직] 앞 조건과 OR로 'status == null' 조건을 추가한다.
                || status == null
                // [방어 로직] 앞 조건과 OR로 'Effects == null)' 조건을 추가한다.
                || Effects == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Find(target);
            // [방어 로직] 'entry == null || entry.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry == null || entry.Transform == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var unitId = target.Identity != null ? target.Identity.UnitId : string.Empty;
            // 지역 변수 'sourceId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var sourceId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'status.SourceSkillId' 값을 선택한다.
                ? status.SourceSkillId
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'statusData.SourceSkillId;' 값을 선택한다.
                : statusData.SourceSkillId;
            // 지역 변수 'hasRuntimeVisual'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var hasRuntimeVisual = RuntimeSkillVisualFactory.HasVisual(statusData.RuntimeVisual);
            // 지역 변수 'visualId'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var visualId = hasRuntimeVisual
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'statusData.RuntimeVisual.GetHashCode()' 값을 선택한다.
                ? statusData.RuntimeVisual.GetHashCode()
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'statusData.StatusEffectPrefab.GetInstanceID();' 값을 선택한다.
                : statusData.StatusEffectPrefab.GetInstanceID();
            // 지역 변수 'key'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var key = $"{unitId}:{status.Kind}:{sourceId}:{visualId}";
            // [방어 로직] 'statusEffectVisuals.TryGetValue(key, out var existing) && existing == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (statusEffectVisuals.TryGetValue(key, out var existing) && existing == null)
            {
                // 지정 항목을 컬렉션에서 제거하고 이후 처리 대상에서 제외한다.
                statusEffectVisuals.Remove(key);
                // 'existing'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                existing = null;
            }

            // 지역 변수 'lifetime'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var lifetime = status.Permanent
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 '3600f' 값을 선택한다.
                ? 3600f
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                : Mathf.Max(0.1f, status.DurationRemaining);
            // [방어 로직] 'existing == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (existing == null)
            {
                // 'existing'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                existing = hasRuntimeVisual
                    // [낯선 문법] 삼항 연산자의 조건 참 결과로 'RuntimeSkillVisualFactory.Create(' 값을 선택한다.
                    ? RuntimeSkillVisualFactory.Create(
                        // 'Effects' 열거값을 선택 가능한 상수 항목으로 정의한다.
                        Effects,
                        // 'statusData.RuntimeVisual' 값을 현재 메소드 호출의 인수로 전달한다.
                        statusData.RuntimeVisual,
                        // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                        string.IsNullOrWhiteSpace(sourceId) ? "RuntimeStatusVisual" : $"RuntimeStatusVisual_{sourceId}",
                        // 'entry.Transform.position' 값을 현재 메소드 호출의 인수로 전달한다.
                        entry.Transform.position,
                        // 'Quaternion.identity' 값을 현재 메소드 호출의 인수로 전달한다.
                        Quaternion.identity,
                        // [낯선 문법] named argument 'includeHitbox'에 'false)' 값을 전달한다.
                        includeHitbox: false)
                    // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'Effects.InstantiateSkillPrefab(statusData.StatusEffectPrefab, entry.Transform.position, Quaternion.identity);' 값을 선택한다.
                    : Effects.InstantiateSkillPrefab(statusData.StatusEffectPrefab, entry.Transform.position, Quaternion.identity);
                // [방어 로직] 'existing == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (existing == null)
                {
                    // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                    return;
                }

                // 'statusEffectVisuals[key]'에 오른쪽 계산 또는 조회 결과를 저장한다.
                statusEffectVisuals[key] = existing;
            }

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = existing.GetComponent<InGameAttachedSkillEffectActor>();
            // [방어 로직] 'actor == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (actor == null)
            {
                // 'actor'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                actor = existing.AddComponent<InGameAttachedSkillEffectActor>();
            }

            // 'actor.Initialize' 메소드를 호출해 해당 객체의 처리를 실행한다.
            actor.Initialize(entry.Transform, lifetime, Vector3.zero);
        }

        // 화면에 살아 있는 적이 있고 유닛 자동 설정이 허용될 때 플레이어 스킬 자동 실행을 허용한다.
        // 'ShouldAutoRouteSkill' 메소드의 입력과 반환 계약을 선언한다.
        private bool ShouldAutoRouteSkill(UnitRosterEntry entry, SkillRuntimeInstance runtime)
        {
            // [방어 로직] 'entry != null && entry.Model is EnemyUnitRuntimeModel' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry != null && entry.Model is EnemyUnitRuntimeModel)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // '!HasVisibleLivingEnemyInMainCamera(' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!HasVisibleLivingEnemyInMainCamera()
                // [방어 로직] 앞 조건과 OR로 'entry == null' 조건을 추가한다.
                || entry == null
                // [방어 로직] 앞 조건과 OR로 'entry.Model == null' 조건을 추가한다.
                || entry.Model == null
                // [방어 로직] 앞 조건과 OR로 '!entry.Model.AutoSkillEnabled)' 조건을 추가한다.
                || !entry.Model.AutoSkillEnabled)
            {
                // 조건 판단의 부정 결과를 false로 반환한다.
                return false;
            }

            // 계산 또는 조회 결과 '!IsSelectedPlayerEntry(entry) || playerAutoSkillEnabled'을 호출자에게 반환한다.
            return !IsSelectedPlayerEntry(entry) || playerAutoSkillEnabled;
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
                    // 'resourceMutations.SynchronizeShieldView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    resourceMutations.SynchronizeShieldView(model);
                    // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                    RefreshUnitActor(model);
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

        // 플레이어 로스터에서 슬롯 0의 선택 몬스터 항목을 찾는다.
        // 'GetSelectedPlayerEntry' 메소드의 입력과 반환 계약을 선언한다.
        private UnitRosterEntry GetSelectedPlayerEntry()
        {
            // 지역 변수 'players'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var players = roster.Players;
            // 'var i = 0; i < players.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < players.Count; i++)
            {
                // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var entry = players[i];
                // [방어 로직] 'entry != null && IsSelectedPlayerModel(entry.Model)' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (entry != null && IsSelectedPlayerModel(entry.Model))
                {
                    // 계산 또는 조회 결과 'entry'을 호출자에게 반환한다.
                    return entry;
                }
            }

            // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
            return null;
        }

        // 지정 항목이 현재 선택된 1P 몬스터 항목인지 확인한다.
        // 'IsSelectedPlayerEntry' 메소드의 입력과 반환 계약을 선언한다.
        private bool IsSelectedPlayerEntry(UnitRosterEntry entry)
        {
            // 계산 또는 조회 결과 'entry != null && entry == GetSelectedPlayerEntry()'을 호출자에게 반환한다.
            return entry != null && entry == GetSelectedPlayerEntry();
        }

        // 모델 식별자가 플레이어 진영 몬스터 슬롯 0인지 판별한다.
        // 'IsSelectedPlayerModel' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsSelectedPlayerModel(BaseUnitRuntimeModel model)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return model != null
                // 앞 조건과 AND로 'model.Identity != null' 조건을 추가한다.
                && model.Identity != null
                // 앞 조건과 AND로 'model.Identity.Side == UnitSide.Player' 조건을 추가한다.
                && model.Identity.Side == UnitSide.Player
                // 앞 조건과 AND로 'model.Identity.Role == UnitRole.Monster' 조건을 추가한다.
                && model.Identity.Role == UnitRole.Monster
                // 앞 조건과 AND로 'model.Identity.SlotIndex == 0;' 조건을 추가한다.
                && model.Identity.SlotIndex == 0;
        }

        // 모델 식별자의 역할이 넥서스인지 판별한다.
        // 'IsNexusModel' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsNexusModel(BaseUnitRuntimeModel model)
        {
            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return model != null
                // 앞 조건과 AND로 'model.Identity != null' 조건을 추가한다.
                && model.Identity != null
                // 앞 조건과 AND로 'model.Identity.Role == UnitRole.Nexus;' 조건을 추가한다.
                && model.Identity.Role == UnitRole.Nexus;
        }

        // 플레이어 위치에서 목표 월드 좌표로 향하는 조준 벡터를 계산한다.
        // 'ResolveAimDirection' 메소드의 입력과 반환 계약을 선언한다.
        private Vector2 ResolveAimDirection(UnitRosterEntry player, Vector2 targetPoint)
        {
            // [방어 로직] 'player == null || player.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (player == null || player.Transform == null)
            {
                // 계산 또는 조회 결과 'Vector2.zero'을 호출자에게 반환한다.
                return Vector2.zero;
            }

            // 계산 또는 조회 결과 'targetPoint - (Vector2)player.Transform.position'을 호출자에게 반환한다.
            return targetPoint - (Vector2)player.Transform.position;
        }

        // 입력 카메라를 사용해 현재 마우스 화면 좌표를 월드 좌표로 변환한다.
        // 'ResolveMouseWorldPoint' 메소드의 입력과 반환 계약을 선언한다.
        private Vector2 ResolveMouseWorldPoint()
        {
            // 지역 변수 'mouse'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var mouse = Mouse.current.position.ReadValue();
            // 직렬화된 입력 카메라로 화면 좌표를 월드 좌표로 변환한다.
            var world = inputCamera.ScreenToWorldPoint(new Vector3(mouse.x, mouse.y, -inputCamera.transform.position.z));
            // 계산 또는 조회 결과 'world'을 호출자에게 반환한다.
            return world;
        }

        // 현재 프레임에 마우스 왼쪽 버튼이 눌리기 시작했는지 확인한다.
        // 'IsPrimaryMousePressedThisFrame' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsPrimaryMousePressedThisFrame()
        {
            // 지역 변수 'mouse'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var mouse = Mouse.current;
            // 계산 또는 조회 결과 'mouse != null && mouse.leftButton.wasPressedThisFrame'을 호출자에게 반환한다.
            return mouse != null && mouse.leftButton.wasPressedThisFrame;
        }

        // 마우스 왼쪽 버튼이 현재 눌린 상태인지 확인한다.
        // 'IsPrimaryMouseHeld' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsPrimaryMouseHeld()
        {
            // 지역 변수 'mouse'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var mouse = Mouse.current;
            // 계산 또는 조회 결과 'mouse != null && mouse.leftButton.isPressed'을 호출자에게 반환한다.
            return mouse != null && mouse.leftButton.isPressed;
        }

        // UI 위가 아닌 유효 마우스 입력을 월드 목표점과 조준 방향으로 변환하고 투사체 입력을 저장한다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryResolveCurrentManualInput' 메소드의 입력과 반환 계약을 선언한다.
        private bool TryResolveCurrentManualInput(
            // 'player' 매개변수 또는 지역값의 타입을 'UnitRosterEntry'로 지정한다.
            UnitRosterEntry player,
            // 'wantsManualInput' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool wantsManualInput,
            // 'pointerOverUi' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool pointerOverUi,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out Vector2 aimDirection,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out Vector2 targetPoint)
        {
            // 'aimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            aimDirection = Vector2.zero;
            // 'targetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            targetPoint = Vector2.zero;
            // '!wantsManualInput || pointerOverUi' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!wantsManualInput || pointerOverUi)
            {
                // [방어 로직] Try 패턴 메소드 'TryResolveCurrentManualInput'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'targetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            targetPoint = ResolveMouseWorldPoint();
            // 'aimDirection'에 오른쪽 계산 또는 조회 결과를 저장한다.
            aimDirection = ResolveAimDirection(player, targetPoint);
            // [방어 로직] 'aimDirection.sqrMagnitude <= 0.0001f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (aimDirection.sqrMagnitude <= 0.0001f)
            {
                // [방어 로직] Try 패턴 메소드 'TryResolveCurrentManualInput'가 결과를 만들지 못했음을 false로 알린다.
                return false;
            }

            // 'latchedManualProjectileAimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            latchedManualProjectileAimDirection = aimDirection;
            // 'latchedManualProjectileTargetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            latchedManualProjectileTargetPoint = targetPoint;
            // 'hasLatchedManualProjectileInput'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            hasLatchedManualProjectileInput = true;
            // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
            return true;
        }

        // 일반 스킬의 클릭 입력과 투사체 연속 발사의 현재·저장 입력을 구분해 실행 조준값을 만든다.
        // [방어 로직] 성공 여부를 bool로 돌려주는 Try 패턴. 'TryResolveManualSkillInputForRuntime' 메소드의 입력과 반환 계약을 선언한다.
        private bool TryResolveManualSkillInputForRuntime(
            // 'runtime' 매개변수 또는 지역값의 타입을 'SkillRuntimeInstance'로 지정한다.
            SkillRuntimeInstance runtime,
            // 'isProjectile' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool isProjectile,
            // 'mousePressedThisFrame' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool mousePressedThisFrame,
            // 'mouseHeld' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool mouseHeld,
            // 'hasCurrentManualInput' 매개변수 또는 지역값의 타입을 'bool'로 지정한다.
            bool hasCurrentManualInput,
            // 'currentAimDirection' 매개변수 또는 지역값의 타입을 'Vector2'로 지정한다.
            Vector2 currentAimDirection,
            // 'currentTargetPoint' 매개변수 또는 지역값의 타입을 'Vector2'로 지정한다.
            Vector2 currentTargetPoint,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out Vector2 aimDirection,
            // [낯선 문법] out 인수로 메소드 성공 여부와 함께 추가 결과값을 받아온다.
            out Vector2 targetPoint)
        {
            // 'aimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            aimDirection = Vector2.zero;
            // 'targetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            targetPoint = Vector2.zero;
            // '!isProjectile' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (!isProjectile)
            {
                // '!mousePressedThisFrame || !hasCurrentManualInput' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (!mousePressedThisFrame || !hasCurrentManualInput)
                {
                    // [방어 로직] Try 패턴 메소드 'TryResolveManualSkillInputForRuntime'가 결과를 만들지 못했음을 false로 알린다.
                    return false;
                }

                // 'aimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                aimDirection = currentAimDirection;
                // 'targetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                targetPoint = currentTargetPoint;
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // 'hasCurrentManualInput && mouseHeld' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (hasCurrentManualInput && mouseHeld)
            {
                // 'aimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                aimDirection = currentAimDirection;
                // 'targetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                targetPoint = currentTargetPoint;
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // 'runtime.IsBursting && hasLatchedManualProjectileInput' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (runtime.IsBursting && hasLatchedManualProjectileInput)
            {
                // 'aimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                aimDirection = latchedManualProjectileAimDirection;
                // 'targetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
                targetPoint = latchedManualProjectileTargetPoint;
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // [방어 로직] Try 패턴 메소드 'TryResolveManualSkillInputForRuntime'가 결과를 만들지 못했음을 false로 알린다.
            return false;
        }

        // 활성 스킬 중 연속 발사 중인 투사체 스킬이 있는지 확인한다.
        // 'HasProjectileBursting' 메소드의 입력과 반환 계약을 선언한다.
        private static bool HasProjectileBursting(IReadOnlyList<SkillRuntimeInstance> activeSkills)
        {
            // [방어 로직] 'activeSkills == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (activeSkills == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 'var i = 0; i < activeSkills.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < activeSkills.Count; i++)
            {
                // 지역 변수 'runtime'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var runtime = activeSkills[i];
                //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
                if (runtime != null
                    // [낯선 문법] is 패턴으로 런타임 타입을 검사하고 필요하면 해당 타입 변수로 받는다.
                    && runtime.Data is ProjectileSkillData
                    // 앞 조건과 AND로 'runtime.IsBursting)' 조건을 추가한다.
                    && runtime.IsBursting)
                {
                    // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                    return true;
                }
            }

            // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
            return false;
        }

        // 연속 투사체 발사에 보존한 수동 조준 방향과 목표점을 초기화한다.
        // 'ClearLatchedManualProjectileInput' 메소드의 입력과 반환 계약을 선언한다.
        private void ClearLatchedManualProjectileInput()
        {
            // 'hasLatchedManualProjectileInput'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            hasLatchedManualProjectileInput = false;
            // 'latchedManualProjectileAimDirection'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            latchedManualProjectileAimDirection = Vector2.zero;
            // 'latchedManualProjectileTargetPoint'에 오른쪽 계산, 조회, 또는 상수 결과를 저장한다.
            latchedManualProjectileTargetPoint = Vector2.zero;
        }

        // 현재 포인터가 EventSystem UI 위에 있는지 확인한다.
        // 'IsPointerOverUi' 메소드의 입력과 반환 계약을 선언한다.
        private static bool IsPointerOverUi()
        {
            // 계산 또는 조회 결과 'EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()'을 호출자에게 반환한다.
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        // 자원 변경 결과가 실제로 달라졌을 때 대상 Actor 표시를 갱신한다.
        // 'RefreshActorIfChanged' 메소드의 입력과 반환 계약을 선언한다.
        private void RefreshActorIfChanged(InGameResourceChangeResult result)
        {
            // 'result.Changed' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (result.Changed)
            {
                // 'RefreshUnitActor' 메소드를 호출해 현재 단계의 처리를 실행한다.
                RefreshUnitActor(result.Target);
            }
        }

        // 적용 피해가 있으면 대상 Actor의 피해 숫자와 몬스터 피격 애니메이션을 실행한다.
        // 'ShowDamageIfChanged' 메소드의 입력과 반환 계약을 선언한다.
        private void ShowDamageIfChanged(InGameResourceChangeResult result)
        {
            // [방어 로직] '!result.Changed || result.AppliedDamage <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (!result.Changed || result.AppliedDamage <= 0f)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'entry'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var entry = roster.Find(result.Target);
            // [방어 로직] 'entry == null || entry.Actor == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry == null || entry.Actor == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'monsterActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var monsterActor = entry.Actor as MonsterUnitActor;
            // [방어 로직] 'monsterActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (monsterActor != null)
            {
                // 'monsterActor.ShowDamage' 메소드를 호출해 해당 객체의 처리를 실행한다.
                monsterActor.ShowDamage(result.AppliedDamage);
                // '!result.IsDead' 조건이 참인지 검사해 실행 분기를 결정한다.
                if (!result.IsDead)
                {
                    // 'monsterActor.TryPlayHitAnimation' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    monsterActor.TryPlayHitAnimation();
                }

                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // 지역 변수 'enemyActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemyActor = entry.Actor as EnemyUnitActor;
            // [방어 로직] 'enemyActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemyActor != null)
            {
                // 'enemyActor.ShowDamage' 메소드를 호출해 해당 객체의 처리를 실행한다.
                enemyActor.ShowDamage(result.AppliedDamage);
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

            // 지역 변수 'actor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var actor = entry.Actor;
            // 'roster.Unregister' 메소드를 호출해 해당 객체의 처리를 실행한다.
            roster.Unregister(result.Target);
            // [방어 로직] 'actor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (actor != null)
            {
                // 지역 변수 'nexusActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var nexusActor = actor as NexusUnitActor;
                // [방어 로직] 'nexusActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (nexusActor != null)
                {
                    // 'nexusActor.NotifyDefeated' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    nexusActor.NotifyDefeated();
                    // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                    return;
                }

                // 지역 변수 'monsterActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var monsterActor = actor as MonsterUnitActor;
                // [방어 로직] 'monsterActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (monsterActor != null)
                {
                    // 'monsterActor.MarkDefeated' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    monsterActor.MarkDefeated();
                    // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                    return;
                }

                // 지정 Unity Object를 수명 종료 시점에 제거한다.
                Destroy(actor.gameObject, 0.95f);
            }
        }

        // 로스터 Actor 유형에 맞는 디버그·체력 표시 갱신 함수를 호출한다.
        // 'RefreshUnitActor' 메소드의 입력과 반환 계약을 선언한다.
        private static bool RefreshUnitActor(UnitRosterEntry entry)
        {
            // [방어 로직] 'entry == null || entry.Actor == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (entry == null || entry.Actor == null)
            {
                // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
                return false;
            }

            // 지역 변수 'monsterActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var monsterActor = entry.Actor as MonsterUnitActor;
            // [방어 로직] 'monsterActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (monsterActor != null)
            {
                // 'monsterActor.RefreshDebugView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                monsterActor.RefreshDebugView();
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // 지역 변수 'enemyActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemyActor = entry.Actor as EnemyUnitActor;
            // [방어 로직] 'enemyActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemyActor != null)
            {
                // 'enemyActor.RefreshDebugView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                enemyActor.RefreshDebugView();
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // 지역 변수 'nexusActor'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var nexusActor = entry.Actor as NexusUnitActor;
            // [방어 로직] 'nexusActor != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (nexusActor != null)
            {
                // 'nexusActor.RefreshDebugView' 메소드를 호출해 해당 객체의 처리를 실행한다.
                nexusActor.RefreshDebugView();
                // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                return true;
            }

            // [방어 로직] 필수 대상 또는 유효 조건이 없으므로 실패 결과 false를 반환한다.
            return false;
        }

        // 살아 있는 적 중 하나라도 직렬화된 입력 카메라 화면 안에 있는지 확인한다.
        // 'HasVisibleLivingEnemyInMainCamera' 메소드의 입력과 반환 계약을 선언한다.
        private bool HasVisibleLivingEnemyInMainCamera()
        {
            // 지역 변수 'enemies'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemies = roster.Enemies;
            // 'var i = 0; i < enemies.Count; i++' 규칙으로 인덱스를 갱신하며 코드를 반복한다.
            for (var i = 0; i < enemies.Count; i++)
            {
                // 지역 변수 'enemy'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var enemy = enemies[i];
                // [방어 로직] 'enemy == null || !enemy.IsAlive || enemy.Transform == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (enemy == null || !enemy.IsAlive || enemy.Transform == null)
                {
                    // 'continue' 값을 현재 메소드 호출의 인수로 전달한다.
                    continue;
                }

                // 직렬화된 입력 카메라에서 적의 viewport 좌표를 계산한다.
                var viewport = inputCamera.WorldToViewportPoint(enemy.Transform.position);
                //  줄로 이어지는 조건식을 시작하고 최종 결과로 실행 분기를 결정한다.
                if (viewport.z >= 0f
                    // 앞 조건과 AND로 'viewport.x >= 0f' 조건을 추가한다.
                    && viewport.x >= 0f
                    // 앞 조건과 AND로 'viewport.x <= 1f' 조건을 추가한다.
                    && viewport.x <= 1f
                    // 앞 조건과 AND로 'viewport.y >= 0f' 조건을 추가한다.
                    && viewport.y >= 0f
                    // 앞 조건과 AND로 'viewport.y <= 1f)' 조건을 추가한다.
                    && viewport.y <= 1f)
                {
                    // 요청한 검사 또는 처리가 성공했음을 true로 반환한다.
                    return true;
                }
            }

            // 조건 판단의 부정 결과를 false로 반환한다.
            return false;
        }
    }

        // 체력과 직접·상태 보호막을 실제로 변경하고 최종 피해 계산과 자원 반올림을 담당한다.
        // 'UnitResourceMutationService' 클래스 정의를 시작한다.
        internal sealed class UnitResourceMutationService
        {
            // 기본 옵션으로 대상에게 속성 피해를 적용한다.
            // 'ApplyDamage' 메소드의 입력과 반환 계약을 선언한다.
            public InGameResourceChangeResult ApplyDamage(
                // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
                BaseUnitRuntimeModel target,
                // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float baseDamage,
                // [Fallback][낯선 문법] 선택 인수 'attribute'가 생략되면 기본값 'DamageAttribute.Physical'을 사용한다.
                DamageAttribute attribute = DamageAttribute.Physical)
            {
                // 계산 또는 조회 결과 'ApplyDamage(target, baseDamage, attribute, default, null, null)'을 호출자에게 반환한다.
                return ApplyDamage(target, baseDamage, attribute, default, null, null);
            }

            // 소진된 상태 보호막 목록을 수집하며 대상에게 피해를 적용한다.
            // 'ApplyDamage' 메소드의 입력과 반환 계약을 선언한다.
            public InGameResourceChangeResult ApplyDamage(
                // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
                BaseUnitRuntimeModel target,
                // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float baseDamage,
                // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
                DamageAttribute attribute,
                // 'depletedShieldStatuses' 매개변수 또는 지역값의 타입을 'ICollection<UnitStatusRuntime>'로 지정한다.
                ICollection<UnitStatusRuntime> depletedShieldStatuses)
            {
                // 계산 또는 조회 결과 'ApplyDamage(target, baseDamage, attribute, default, depletedShieldStatuses, null)'을 호출자에게 반환한다.
                return ApplyDamage(target, baseDamage, attribute, default, depletedShieldStatuses, null);
            }

            // 방어·치명타·상태 보호막·직접 보호막·체력 순서로 피해를 처리해 변경 결과를 만든다.
            // 'ApplyDamage' 메소드의 입력과 반환 계약을 선언한다.
            public InGameResourceChangeResult ApplyDamage(
                // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
                BaseUnitRuntimeModel target,
                // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
                float baseDamage,
                // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
                DamageAttribute attribute,
                // 'options' 매개변수 또는 지역값의 타입을 'DamageApplicationOptions'로 지정한다.
                DamageApplicationOptions options,
                // 'depletedShieldStatuses' 매개변수 또는 지역값의 타입을 'ICollection<UnitStatusRuntime>'로 지정한다.
                ICollection<UnitStatusRuntime> depletedShieldStatuses,
                // 'absorbedShieldStatuses' 매개변수 또는 지역값의 타입을 'ICollection<ShieldAbsorbRecord>'로 지정한다.
                ICollection<ShieldAbsorbRecord> absorbedShieldStatuses)
            {
                // [방어 로직] 'target == null || target.Resources == null || baseDamage <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (target == null || target.Resources == null || baseDamage <= 0f)
                {
                    // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                    return InGameResourceChangeResult.Unchanged(target);
                }

                // 지역 변수 'resources'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var resources = target.Resources;
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
                // 지역 변수 'beforeShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var beforeShield = ComputeTotalShield(target);
                // 지역 변수 'finalDamage'에 오른쪽 계산 또는 조회 결과를 저장한다.
                var finalDamage = ResolveFinalDamage(target, baseDamage, attribute, options);
                // [방어 로직] 'target.Statuses != null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
                if (target.Statuses != null)
                {
                    // 'target.Statuses.RecordIncomingDamage' 메소드를 호출해 해당 객체의 처리를 실행한다.
                    target.Statuses.RecordIncomingDamage(attribute, finalDamage);
                }

                // 지역 변수 'statusShieldDamage'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                var statusShieldDamage = target.Statuses != null
                    // [낯선 문법] 삼항 연산자의 조건 참 결과로 'target.Statuses.ConsumeShield(finalDamage, depletedShieldStatuses, absorbedShieldStatuses)' 값을 선택한다.
                    ? target.Statuses.ConsumeShield(finalDamage, depletedShieldStatuses, absorbedShieldStatuses)
                    // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 '0f;' 값을 선택한다.
                    : 0f;
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var damageAfterStatusShield = Mathf.Max(0f, finalDamage - statusShieldDamage);
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var directShieldBefore = Mathf.Max(0f, resources.DirectShield);
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var directShieldDamage = Mathf.Min(directShieldBefore, damageAfterStatusShield);
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                var remainingDamage = Mathf.Max(0f, damageAfterStatusShield - directShieldDamage);

                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                resources.DirectShield = RoundResource(Mathf.Max(0f, directShieldBefore - directShieldDamage));
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                resources.CurrentHealth = RoundResource(Mathf.Max(0f, beforeHealth - remainingDamage));
                // 'SynchronizeShieldView' 메소드를 호출해 현재 단계의 처리를 실행한다.
                SynchronizeShieldView(target);

                // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
                return new InGameResourceChangeResult(
                    // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    target,
                    // 'beforeHealth' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    beforeHealth,
                    // 'resources.CurrentHealth' 값을 현재 메소드 호출의 인수로 전달한다.
                    resources.CurrentHealth,
                    // 'beforeShield' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    beforeShield,
                    // 'resources.CurrentShield' 값을 현재 메소드 호출의 인수로 전달한다.
                    resources.CurrentShield,
                    // 'finalDamage' 열거값을 선택 가능한 상수 항목으로 정의한다.
                    finalDamage,
                    // 'resources.CurrentHealth <= 0f);' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                    resources.CurrentHealth <= 0f);
            }

        // 대상의 기존 직접 보호막에 지정 값을 더하고 총 보호막 표시를 동기화한다.
        // 'GrantShield' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult GrantShield(BaseUnitRuntimeModel target, float amount)
        {
            // [방어 로직] 'target == null || target.Resources == null || amount <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Resources == null || amount <= 0f)
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'resources'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var resources = target.Resources;
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            // 지역 변수 'beforeShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var beforeShield = ComputeTotalShield(target);
            // 'resources.CurrentHealth'에 오른쪽 계산 또는 조회 결과를 저장한다.
            resources.CurrentHealth = RoundResource(beforeHealth);
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            resources.DirectShield = RoundResource(Mathf.Max(0f, resources.DirectShield) + amount);
            // 'SynchronizeShieldView' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SynchronizeShieldView(target);

            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return new InGameResourceChangeResult(
                // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                target,
                // 'beforeHealth' 열거값을 선택 가능한 상수 항목으로 정의한다.
                beforeHealth,
                // 'resources.CurrentHealth' 값을 현재 메소드 호출의 인수로 전달한다.
                resources.CurrentHealth,
                // 'beforeShield' 열거값을 선택 가능한 상수 항목으로 정의한다.
                beforeShield,
                // 'resources.CurrentShield' 값을 현재 메소드 호출의 인수로 전달한다.
                resources.CurrentShield,
                // '0f' 식의 값을 현재 생성자 또는 메소드 호출에 전달한다.
                0f,
                // 'resources.CurrentHealth <= 0f);' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                resources.CurrentHealth <= 0f);
        }

        // 대상의 직접 보호막을 지정 값으로 교체하고 총 보호막 표시를 동기화한다.
        // 'SetShield' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult SetShield(BaseUnitRuntimeModel target, float amount)
        {
            // [방어 로직] 'target == null || target.Resources == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Resources == null)
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'resources'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var resources = target.Resources;
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            // 지역 변수 'beforeShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var beforeShield = ComputeTotalShield(target);
            // 'resources.CurrentHealth'에 오른쪽 계산 또는 조회 결과를 저장한다.
            resources.CurrentHealth = RoundResource(beforeHealth);
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            resources.DirectShield = RoundResource(Mathf.Max(0f, amount));
            // 'SynchronizeShieldView' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SynchronizeShieldView(target);

            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return new InGameResourceChangeResult(
                // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                target,
                // 'beforeHealth' 열거값을 선택 가능한 상수 항목으로 정의한다.
                beforeHealth,
                // 'resources.CurrentHealth' 값을 현재 메소드 호출의 인수로 전달한다.
                resources.CurrentHealth,
                // 'beforeShield' 열거값을 선택 가능한 상수 항목으로 정의한다.
                beforeShield,
                // 'resources.CurrentShield' 값을 현재 메소드 호출의 인수로 전달한다.
                resources.CurrentShield,
                // '0f' 식의 값을 현재 생성자 또는 메소드 호출에 전달한다.
                0f,
                // 'resources.CurrentHealth <= 0f);' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                resources.CurrentHealth <= 0f);
        }

        // 대상 체력을 최대 체력 이하로 회복하고 자원 변경 결과를 만든다.
        // 'Heal' 메소드의 입력과 반환 계약을 선언한다.
        public InGameResourceChangeResult Heal(BaseUnitRuntimeModel target, float amount)
        {
            // [방어 로직] 'target == null || target.Resources == null || target.Stats == null || amount <= 0f' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Resources == null || target.Stats == null || amount <= 0f)
            {
                // 계산 또는 조회 결과 'InGameResourceChangeResult.Unchanged(target)'을 호출자에게 반환한다.
                return InGameResourceChangeResult.Unchanged(target);
            }

            // 지역 변수 'resources'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var resources = target.Resources;
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            // 지역 변수 'beforeShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var beforeShield = ComputeTotalShield(target);
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var maxHealth = Mathf.Max(0f, target.Stats.MaxHealth);
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            resources.CurrentHealth = RoundResource(Mathf.Min(maxHealth, beforeHealth + amount));
            // 'SynchronizeShieldView' 메소드를 호출해 현재 단계의 처리를 실행한다.
            SynchronizeShieldView(target);

            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return new InGameResourceChangeResult(
                // 'target' 열거값을 선택 가능한 상수 항목으로 정의한다.
                target,
                // 'beforeHealth' 열거값을 선택 가능한 상수 항목으로 정의한다.
                beforeHealth,
                // 'resources.CurrentHealth' 값을 현재 메소드 호출의 인수로 전달한다.
                resources.CurrentHealth,
                // 'beforeShield' 열거값을 선택 가능한 상수 항목으로 정의한다.
                beforeShield,
                // 'resources.CurrentShield' 값을 현재 메소드 호출의 인수로 전달한다.
                resources.CurrentShield,
                // '0f' 식의 값을 현재 생성자 또는 메소드 호출에 전달한다.
                0f,
                // 'resources.CurrentHealth <= 0f);' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                resources.CurrentHealth <= 0f);
        }

        // 공격자·대상 능력치와 상태 보정을 DamageCalculator에 전달해 반올림된 최종 피해를 계산한다.
        // 'ResolveFinalDamage' 메소드의 입력과 반환 계약을 선언한다.
        private static float ResolveFinalDamage(
            // 'target' 매개변수 또는 지역값의 타입을 'BaseUnitRuntimeModel'로 지정한다.
            BaseUnitRuntimeModel target,
            // 'baseDamage' 매개변수 또는 지역값의 타입을 'float'로 지정한다.
            float baseDamage,
            // 'attribute' 매개변수 또는 지역값의 타입을 'DamageAttribute'로 지정한다.
            DamageAttribute attribute,
            // 'options' 매개변수 또는 지역값의 타입을 'DamageApplicationOptions'로 지정한다.
            DamageApplicationOptions options)
        {
            // 지역 변수 'criticalAllowed'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var criticalAllowed = options.CriticalAllowed && options.Source != null;
            // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
            var sourceStats = criticalAllowed ? options.Source.Stats : null;
            // 지역 변수 'sourceCriticalChance'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var sourceCriticalChance = criticalAllowed
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                ? (sourceStats != null ? sourceStats.CriticalChance : DamageCalculator.BaseCriticalChance)
                    // '+ StatusEffectRuntime.ResolveCriticalChanceBonus(options.Source)' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                    + StatusEffectRuntime.ResolveCriticalChanceBonus(options.Source)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'DamageCalculator.BaseCriticalChance;' 값을 선택한다.
                : DamageCalculator.BaseCriticalChance;
            // 지역 변수 'sourceCriticalDamage'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var sourceCriticalDamage = criticalAllowed
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                ? (sourceStats != null ? sourceStats.CriticalDamage : DamageCalculator.BaseCriticalMultiplier)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 'DamageCalculator.BaseCriticalMultiplier;' 값을 선택한다.
                : DamageCalculator.BaseCriticalMultiplier;
            // 'criticalAllowed' 조건이 참인지 검사해 실행 분기를 결정한다.
            if (criticalAllowed)
            {
                // 'sourceCriticalDamage' 값에 'StatusEffectRuntime.ResolveCriticalDamageBonus(options.Source)' 결과를 누적한다.
                sourceCriticalDamage += StatusEffectRuntime.ResolveCriticalDamageBonus(options.Source);
            }

            // 지역 변수 'targetCriticalResistance'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var targetCriticalResistance = criticalAllowed
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                ? (target != null && target.Stats != null ? target.Stats.CriticalResistance : 0f)
                    // '+ StatusEffectRuntime.ResolveCriticalResistanceBonus(target)' 식을 평가해 현재 계산 또는 상태 변경의 한 단계를 수행한다.
                    + StatusEffectRuntime.ResolveCriticalResistanceBonus(target)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 '0f;' 값을 선택한다.
                : 0f;
            // 지역 변수 'criticalDamageTakenBonus'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var criticalDamageTakenBonus = criticalAllowed
                // [낯선 문법] 삼항 연산자의 조건 참 결과로 'StatusEffectRuntime.ResolveCriticalDamageTakenBonus(target)' 값을 선택한다.
                ? StatusEffectRuntime.ResolveCriticalDamageTakenBonus(target)
                // [Fallback][낯선 문법] 삼항 연산자의 조건 거짓 대체값으로 '0f;' 값을 선택한다.
                : 0f;
            // 지역 변수 'damage'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
            var damage = DamageCalculator.Resolve(
                // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
                Mathf.Max(0f, baseDamage),
                // 'attribute' 열거값을 선택 가능한 상수 항목으로 정의한다.
                attribute,
                // [Fallback][낯선 문법] 삼항 연산자(?:)로 조건에 따라 정상값 또는 대체값을 선택한다.
                target != null ? ToAttributeDefenseSet(target.Defenses) : null,
                // 'criticalAllowed' 열거값을 선택 가능한 상수 항목으로 정의한다.
                criticalAllowed,
                // [낯선 문법] named argument 'flatDefenseReduction'에 'StatusEffectRuntime.ResolveFlatElementResistReduction(target, attribute),' 값을 전달한다.
                flatDefenseReduction: StatusEffectRuntime.ResolveFlatElementResistReduction(target, attribute),
                // [낯선 문법] named argument 'percentDefenseReductions'에 'new[] { StatusEffectRuntime.ResolveElementResistReduction(target, attribute) },' 값을 전달한다.
                percentDefenseReductions: new[] { StatusEffectRuntime.ResolveElementResistReduction(target, attribute) },
                // [낯선 문법] named argument 'criticalChanceBonus'에 'sourceCriticalChance + options.CritChanceBonus - DamageCalculator.BaseCriticalChance,' 값을 전달한다.
                criticalChanceBonus: sourceCriticalChance + options.CritChanceBonus - DamageCalculator.BaseCriticalChance,
                // [낯선 문법] named argument 'criticalMultiplierBonus'에 'sourceCriticalDamage + options.CritDamageBonus - DamageCalculator.BaseCriticalMultiplier,' 값을 전달한다.
                criticalMultiplierBonus: sourceCriticalDamage + options.CritDamageBonus - DamageCalculator.BaseCriticalMultiplier,
                // [낯선 문법] named argument 'targetCriticalResistance'에 'targetCriticalResistance,' 값을 전달한다.
                targetCriticalResistance: targetCriticalResistance,
                // [낯선 문법] named argument 'criticalDamageTakenBonus'에 'criticalDamageTakenBonus,' 값을 전달한다.
                criticalDamageTakenBonus: criticalDamageTakenBonus,
                // 상태 효과와 적 패시브를 반영한 최종 피해 배율을 전달한다.
                finalDamageMultiplier: ResolveIncomingDamageMultiplier(target, options.Source, attribute, options.SourceSkillId));
            // 계산 또는 조회 결과 'Mathf.Round(Mathf.Max(0f, damage))'을 호출자에게 반환한다.
            return Mathf.Round(Mathf.Max(0f, damage));
        }

        // 유닛 방어력 모델을 DamageCalculator가 사용하는 속성 방어력 집합으로 복사한다.
        // 'ToAttributeDefenseSet' 메소드의 입력과 반환 계약을 선언한다.
        private static AttributeDefenseSet ToAttributeDefenseSet(UnitDefenseRuntime defenses)
        {
            // [방어 로직] 'defenses == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (defenses == null)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 'null'을 호출자에게 반환한다.
                return null;
            }

            // 여러 줄로 이어지는 계산 또는 조건 결과를 반환하기 시작한다.
            return new AttributeDefenseSet
            {
                // 'Physical'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                Physical = defenses.Physical,
                // 'Fire'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                Fire = defenses.Fire,
                // 'Lightning'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                Lightning = defenses.Lightning,
                // 'Ice'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                Ice = defenses.Ice,
                // 'Darkness'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                Darkness = defenses.Darkness,
                // 'Holy'에 저장할 여러 줄 계산 또는 선택식을 시작한다.
                Holy = defenses.Holy
            };
        }

        // 상태 기반 받는 피해 배율과 적 고유 패시브 피해 배율을 결합한다.
        // 'ResolveIncomingDamageMultiplier' 메소드의 입력과 반환 계약을 선언한다.
        private static float ResolveIncomingDamageMultiplier(BaseUnitRuntimeModel target, BaseUnitRuntimeModel source, DamageAttribute attribute, string sourceSkillId)
        {
            // 지역 변수 'statusMultiplier'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var statusMultiplier = StatusEffectRuntime.ResolveIncomingDamageMultiplier(target, source, attribute, sourceSkillId);
            // 지역 변수 'enemy'에 오른쪽 계산 또는 조회 결과를 저장한다.
            var enemy = target as EnemyUnitRuntimeModel;
            // [방어 로직] 'enemy == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (enemy == null)
            {
                // 계산 또는 조회 결과 'statusMultiplier'을 호출자에게 반환한다.
                return statusMultiplier;
            }

            // 계산 또는 조회 결과 'Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier) * statusMultiplier'을 호출자에게 반환한다.
            return Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier) * statusMultiplier;
        }

        // 직접 보호막과 시간제 상태 보호막의 합을 CurrentShield에 동기화한다.
        // 'SynchronizeShieldView' 메소드의 입력과 반환 계약을 선언한다.
        public void SynchronizeShieldView(BaseUnitRuntimeModel target)
        {
            // [방어 로직] 'target == null || target.Resources == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Resources == null)
            {
                // [방어 로직] 현재 메소드의 남은 처리를 건너뛰고 즉시 호출 지점으로 돌아간다.
                return;
            }

            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            target.Resources.DirectShield = RoundResource(Mathf.Max(0f, target.Resources.DirectShield));
            // 'target.Resources.CurrentShield'에 오른쪽 계산 또는 조회 결과를 저장한다.
            target.Resources.CurrentShield = ComputeTotalShield(target);
        }

        // 대상의 직접 보호막과 활성 상태 보호막 총량을 합산한다.
        // 'ComputeTotalShield' 메소드의 입력과 반환 계약을 선언한다.
        private static float ComputeTotalShield(BaseUnitRuntimeModel target)
        {
            // [방어 로직] 'target == null || target.Resources == null' 상태가 안전한 실행 조건을 만족하지 않는지 검사한다.
            if (target == null || target.Resources == null)
            {
                // [Fallback] 정상 결과를 만들 수 없을 때 기본 결과 '0f'을 호출자에게 반환한다.
                return 0f;
            }

            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var directShield = Mathf.Max(0f, target.Resources.DirectShield);
            // [방어 로직] Mathf 범위 함수로 계산값이 허용 범위를 벗어나지 않게 보정한다.
            var timedShield = target.Statuses != null ? Mathf.Max(0f, target.Statuses.GetTotalShieldAmount()) : 0f;
            // 계산 또는 조회 결과 'RoundResource(directShield + timedShield)'을 호출자에게 반환한다.
            return RoundResource(directShield + timedShield);
        }

        // 자원 값을 0 이상으로 제한하고 가장 가까운 정수로 반올림한다.
        // 'RoundResource' 메소드의 입력과 반환 계약을 선언한다.
        private static float RoundResource(float value)
        {
            // 계산 또는 조회 결과 'Mathf.Round(Mathf.Max(0f, value))'을 호출자에게 반환한다.
            return Mathf.Round(Mathf.Max(0f, value));
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
