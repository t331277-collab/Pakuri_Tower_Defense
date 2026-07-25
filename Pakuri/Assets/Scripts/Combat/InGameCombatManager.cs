using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 인게임 전투 흐름을 조율하는 중앙 컴포넌트와 피해 적용 데이터를 정의한다.
 * 유닛 로스터를 기준으로 스킬, 입력, 적 행동, 패시브, 상태 갱신 순서를 연결하고
 * 피해·회복·상태 변화 결과를 액터 표시, 효과, 트리거, 사망 처리에 전달한다.
 * 실제 피해 계산, 스킬 실행, 적 행동, 시각 효과는 각 전용 시스템에 맡긴다.
 */
namespace Pakuri.InGame
{
    public readonly struct DamageApplicationOptions
    {
        /*
         * 피해 적용 옵션을 구성한다.
         */
        public DamageApplicationOptions(
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            bool criticalAllowed /* 치명타 허용 여부 */,
            float critChanceBonus /* 추가 치명타 확률 */,
            float critDamageBonus /* 추가 치명타 피해 배율 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */,
            bool suppressOutgoingDamageTriggers /* 생략 주는 피해 트리거 목록 여부 */,
            bool sourceHitWasExecute /* 발생 원본 적중 발생 처형 여부 */,
            string damageMeterSourceId /* 피해량 기록에 사용할 발생 원본 식별자 */)
        {
            Source = source;
            CriticalAllowed = criticalAllowed;
            CritChanceBonus = critChanceBonus;
            CritDamageBonus = critDamageBonus;
            SourceSkillId = sourceSkillId;
            SuppressOutgoingDamageTriggers = suppressOutgoingDamageTriggers;
            SourceHitWasExecute = sourceHitWasExecute;
            DamageMeterSourceId = damageMeterSourceId;
        }

        public UnitCombatState Source { get; }
        public bool CriticalAllowed { get; }
        public float CritChanceBonus { get; }
        public float CritDamageBonus { get; }
        public string SourceSkillId { get; }
        public bool SuppressOutgoingDamageTriggers { get; }
        public bool SourceHitWasExecute { get; }
        public string DamageMeterSourceId { get; }
    }

    /*
     * 전투 로스터의 피해, 상태, 스킬, 적 행동 처리 순서를 조율한다.
     */
    public class InGameCombatManager : MonoBehaviour
    {
        private readonly CombatUnitRegistry unitRegistry = new CombatUnitRegistry();
        private EnemyActionController enemyActionController;
        private readonly SkillExecution skillExecution = new SkillExecution();
        private readonly PassiveSkill passiveEffects = new PassiveSkill();
        [SerializeField] private PlayerCombatInputController playerCombatControl;
        private readonly HashSet<UnitCombatState> combatStartDispatchedUnits = new HashSet<UnitCombatState>();
        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private EffectManager effectManager;

        public CombatUnitRegistry UnitRegistry => unitRegistry;
        public EffectManager Effects => effectManager;
        internal PassiveSkill PassiveEffects => passiveEffects;
        internal SkillExecution SkillExecution => skillExecution;

        public int ActiveEnemyCount => unitRegistry.EnemyCount;
        public event Action<DamageApplicationOptions, InGameResourceChangeResult> DamageApplied;
        public event Action<UnitCombatState> UnitDefeated;

        /*
         * 전투 시작 전에 로스터와 전투 기록을 초기화한다.
         */
        private void Awake()
        {
            enemyActionController = new EnemyActionController(unitRegistry, skillExecution, this);
            unitRegistry.Clear();
            combatStartDispatchedUnits.Clear();
            passiveEffects.Reset();
        }

        /*
         * 패시브, 스킬, 입력, 적 행동, 상태 지속시간을 순서대로 갱신한다.
         */
        private void Update()
        {
            FlushPassiveEffectChanges();

            if (skillExecutionEnabled)
            {
                TickSkillStates(Time.deltaTime);
                skillExecution.TryExecuteAutomaticSkills(
                    unitRegistry,
                    this,
                    (entry, runtime) => playerCombatControl.CanUseAutoSkill(entry, unitRegistry));
                playerCombatControl.HandleManualInput(
                    unitRegistry,
                    skillExecution,
                    this);
            }

            if (enemyCombatSimulationEnabled)
            {
                enemyActionController.Tick(Time.deltaTime);
            }

            TickUnitStatuses(Time.deltaTime);
            FlushPassiveEffectChanges();
        }

        /*
         * 전투에 등록된 유닛이 보유한 스킬의 쿨타임과 탄창 시간을 갱신한다.
         */
        private void TickSkillStates(float deltaTime /* 이전 갱신 이후 지난 시간 */)
        {
            var entries = unitRegistry.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.Model != null)
                {
                    entry.Model.SkillState.Tick(deltaTime);
                }
            }
        }

        /*
         * 플레이어 몬스터를 등록하고 전투 시작 처리를 실행한다.
         */
        public CombatUnitEntry RegisterPlayerMonster(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, MonsterActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */, Transform hitboxRoot /* 피격 판정의 기준 위치 */)
        {
            var entry = unitRegistry.Register(model, actor, hitboxRoot);
            passiveEffects.NotifyRosterChanged();
            if (PlayerCombatInputController.IsSelectedPlayerModel(model))
            {
                playerCombatControl.ApplyAutoSkillModeToSelectedPlayer(unitRegistry);
            }

            DispatchCombatStartOnce(model);
            return entry;
        }

        /*
         * 적을 등록하고 전투 시작 처리를 실행한다.
         */
        public CombatUnitEntry RegisterEnemy(EnemyCombatState model /* 처리할 상태 모델 */, EnemyActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */, Transform hitboxRoot /* 피격 판정의 기준 위치 */)
        {
            var entry = unitRegistry.Register(model, actor, hitboxRoot);
            passiveEffects.NotifyRosterChanged();
            DispatchCombatStartOnce(model);
            return entry;
        }

        /*
         * 넥서스를 전투 로스터에 등록한다.
         */
        public CombatUnitEntry RegisterNexus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, NexusActor actor /* 화면에서 유닛을 표현하는 컴포넌트 */, Transform hitboxRoot /* 피격 판정의 기준 위치 */)
        {
            var entry = unitRegistry.Register(model, actor, hitboxRoot);
            passiveEffects.NotifyRosterChanged();
            return entry;
        }

        /*
         * 유닛을 로스터에서 해제하고 연결된 Actor를 제거한다.
         */
        public bool DespawnUnit(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            var entry = unitRegistry.Find(model);
            var actor = entry.Actor;
            unitRegistry.Unregister(model);
            passiveEffects.NotifyRosterChanged();
            Destroy(actor.gameObject);

            return true;
        }

        /*
         * 피해를 적용하고 표시, Trigger, 사망 처리를 실행한다.
         */
        public InGameResourceChangeResult ApplyDamage(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            float baseDamage /* 원본 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            bool criticalAllowed = false /* 치명타 허용 여부 */,
            float critChanceBonus = 0f /* 추가 치명타 확률 */,
            float critDamageBonus = 0f /* 추가 치명타 피해 배율 */,
            string sourceSkillId = null /* 효과를 발생시킨 스킬 식별자 */,
            bool suppressOutgoingDamageTriggers = false /* 생략 주는 피해 트리거 목록 여부 */,
            bool sourceHitWasExecute = false /* 발생 원본 적중 발생 처형 여부 */,
            string damageMeterSourceId = null /* 피해량 기록에 사용할 발생 원본 식별자 */)
        {
            var depletedShields = new List<StatusRuntimeInstance>();
            var absorbedShields = new List<ShieldAbsorptionRecord>();
            var options = new DamageApplicationOptions(source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers, sourceHitWasExecute, damageMeterSourceId);
            var result = ApplyDamageToResources(target, baseDamage, attribute, options, depletedShields, absorbedShields);
            // 자원 변화가 없으면 표시와 Trigger도 실행하지 않는다.
            if (!result.Changed)
            {
                return result;
            }

            passiveEffects.NotifyResourceChanged(result);
            // 보호막 소진은 상태 조건 변경도 함께 알린다.
            if (depletedShields.Count > 0)
            {
                passiveEffects.NotifyStatusChanged(target);
                for (var i = 0; i < depletedShields.Count; i++)
                {
                    effectManager.RemoveEffect(null, depletedShields[i]);
                }
            }
            // 통계와 UI에는 실제 자원 변화 결과만 전달한다.
            DamageApplied?.Invoke(options, result);
            var damagedEntry = unitRegistry.Find(result.Target);
            damagedEntry.RefreshDisplay();
            damagedEntry.ShowDamage(result.AppliedDamage, result.IsDead);
            SkillTrigger.ExecuteShieldAbsorbs(this, unitRegistry, target, source, absorbedShields);
            SkillTrigger.ExecuteExpiredStatuses(this, unitRegistry, target, depletedShields);
            DispatchOutgoingDamageTriggers(target, attribute, options, result, baseDamage);
            if (result.IsDead && options.Source != null)
            {
                SkillTrigger.ExecuteKill(
                    this,
                    unitRegistry,
                    options.Source,
                    options.SourceSkillId,
                    target,
                    attribute,
                    result.AppliedDamage,
                    options.SourceHitWasExecute);
            }

            RemoveUnitIfDead(result);
            return result;
        }

        /*
         * 넥서스를 제외한 대상의 체력을 회복한다.
         */
        public InGameResourceChangeResult Heal(UnitCombatState target /* 효과를 받을 대상 유닛 */, float amount /* 적용할 수치 */)
        {
            // 넥서스는 회복량을 0으로 처리한다.
            var result = HealResources(target, target.IsNexus ? 0f : amount);
            if (!result.Changed)
            {
                return result;
            }

            passiveEffects.NotifyResourceChanged(result);
            unitRegistry.Find(result.Target).RefreshDisplay();
            return result;
        }

        /*
         * 피해를 보호막과 체력에 적용하고 변경 결과를 만든다.
         */
        private static InGameResourceChangeResult ApplyDamageToResources(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            float baseDamage /* 방어 계산 전 기본 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            DamageApplicationOptions options /* 처리에 사용할 추가 설정 */,
            ICollection<StatusRuntimeInstance> depletedShields /* 소진된 보호막 목록 */,
            ICollection<ShieldAbsorptionRecord> absorbedShields /* 흡수된 보호막 목록 */)
        {
            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = target.GetTotalShield();
            var currentHealth = beforeHealth;
            var currentShield = beforeShield;
            var finalDamage = 0f;

            if (baseDamage > 0f)
            {
                finalDamage = DamageCalculator.CalculateFinalDamage(target, baseDamage, attribute, options);

                target.Statuses.RecordIncomingDamage(attribute, finalDamage);

                // 상태 보호막, 직접 보호막, 체력 순서로 피해를 소모한다.
                var statusShieldDamage = target.Statuses.ConsumeShield(finalDamage, depletedShields, absorbedShields);
                var damageAfterStatusShield = Mathf.Max(0f, finalDamage - statusShieldDamage);
                var directShieldBefore = Mathf.Max(0f, resources.DirectShield);
                var directShieldDamage = Mathf.Min(directShieldBefore, damageAfterStatusShield);
                var remainingDamage = Mathf.Max(0f, damageAfterStatusShield - directShieldDamage);

                resources.DirectShield = Round(Mathf.Max(0f, directShieldBefore - directShieldDamage));
                resources.CurrentHealth = Round(Mathf.Max(0f, beforeHealth - remainingDamage));
                target.SyncShield();
                currentHealth = resources.CurrentHealth;
                currentShield = resources.CurrentShield;
            }

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                currentHealth,
                beforeShield,
                currentShield,
                finalDamage,
                currentHealth <= 0f);
        }

        /*
         * 체력을 최대 체력 범위에서 회복하고 변경 결과를 만든다.
         */
        private static InGameResourceChangeResult HealResources(UnitCombatState target /* 효과를 받을 대상 유닛 */, float amount /* 적용할 수치 */)
        {
            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = target.GetTotalShield();
            var currentHealth = beforeHealth;
            var currentShield = beforeShield;

            if (amount > 0f)
            {
                var maxHealth = Mathf.Max(0f, target.Stats.MaxHealth);
                resources.CurrentHealth = Round(Mathf.Min(maxHealth, beforeHealth + amount));
                target.SyncShield();
                currentHealth = resources.CurrentHealth;
                currentShield = resources.CurrentShield;
            }

            return new InGameResourceChangeResult(
                target,
                beforeHealth,
                currentHealth,
                beforeShield,
                currentShield,
                0f,
                currentHealth <= 0f);
        }

        /*
         * 자원 값을 0 이상 정수로 정리한다.
         */
        private static float Round(float value /* 처리할 값 */)
        {
            return Mathf.Round(Mathf.Max(0f, value));
        }

        /*
         * 일반 상태를 적용하고 상태 표시와 패시브 조건을 갱신한다.
         */
        public StatusRuntimeInstance ApplyStatus(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            StatusRuntimeData statusData /* 상태 효과 실행 데이터 */,
            int stacks /* 중첩 수 */,
            float durationSeconds /* 지속 시간(초) */,
            int maxStacks /* 최대 중첩 수 */,
            bool permanent /* 영구 여부 */,
            bool refreshDuration /* 갱신 지속 시간 여부 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */)
        {
            // 넥서스에는 일반 상태를 적용하지 않는다.
            if (statusData.Kind == StatusEffectKind.None || target.IsNexus)
            {
                return null;
            }

            var status = target.Statuses.Apply(
                statusData,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration);
            status.SetSourceUnit(source);
            passiveEffects.NotifyStatusChanged(target);
            target.SyncShield();
            unitRegistry.RefreshDisplay(target);
            effectManager.ShowOrRefreshStatusEffect(unitRegistry.Find(target).Transform, status);
            return status;
        }

        /*
         * 보호막 상태를 적용하고 상태 표시와 패시브 조건을 갱신한다.
         */
        public StatusRuntimeInstance ApplyShieldStatus(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            StatusRuntimeData statusData /* 상태 효과 실행 데이터 */,
            float shieldAmount /* 보호막 수치 */,
            float durationSeconds /* 지속 시간(초) */,
            int stacks /* 중첩 수 */,
            int maxStacks /* 최대 중첩 수 */,
            bool permanent /* 영구 여부 */,
            bool refreshDuration /* 갱신 지속 시간 여부 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */)
        {
            // 넥서스에는 보호막 상태를 적용하지 않는다.
            if (statusData.Kind != StatusEffectKind.Shield || target.IsNexus)
            {
                return null;
            }

            // 대상의 보호막 수신 배율을 먼저 반영한다.
            var adjustedShieldAmount = Mathf.Max(0f, shieldAmount) * StatusCombatRules.ResolveShieldReceivedMultiplier(target);
            var status = target.Statuses.Apply(
                statusData,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration,
                adjustedShieldAmount);
            status.SetSourceUnit(source);
            passiveEffects.NotifyStatusChanged(target);

            target.SyncShield();
            unitRegistry.RefreshDisplay(target);
            effectManager.ShowOrRefreshStatusEffect(unitRegistry.Find(target).Transform, status);
            return status;
        }

        /*
         * 지정한 상태의 지속시간을 연장하고 표시를 갱신한다.
         */
        public bool ExtendStatusDuration(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusEffectKind kind /* 처리할 종류 */, float durationDelta /* 지속 시간 경과 */)
        {
            if (kind == StatusEffectKind.None || durationDelta <= 0f || target.IsNexus)
            {
                return false;
            }

            // 영구 상태와 이미 소진된 보호막은 연장하지 않는다.
            var changed = target.Statuses.ExtendDurations(
                kind,
                durationDelta,
                status => !status.Permanent && (!status.IsShieldStatus || status.RemainingShieldAmount > 0f));
            if (!changed)
            {
                return false;
            }

            target.SyncShield();
            unitRegistry.RefreshDisplay(target);

            var activeStatuses = target.Statuses.ActiveStatuses;
            for (var i = 0; i < activeStatuses.Count; i++)
            {
                var status = activeStatuses[i];
                if (status.Kind != kind)
                {
                    continue;
                }

                effectManager.ShowOrRefreshStatusEffect(unitRegistry.Find(target).Transform, status);
            }

            return true;
        }

        /*
         * 코루틴, 입력, 효과, 상태, 보호막을 전투 시작 상태로 되돌린다.
         */
        public void ResetCombatState()
        {
            StopAllCoroutines();
            playerCombatControl.ClearManualInput();
            effectManager.ClearEffects();

            combatStartDispatchedUnits.Clear();
            passiveEffects.Reset();

            var entries = unitRegistry.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var model = entry.Model;

                // 몬스터는 전용 서비스로, 나머지는 공통 상태만 초기화한다.
                if (model.Identity.Role == UnitRole.Monster)
                {
                    MonsterDayRecovery.ResetTransient(model);
                }
                else
                {
                    model.Statuses.Clear();
                    model.Resources.DirectShield = 0f;
                    model.Resources.CurrentShield = 0f;
                }

                model.SyncShield();
                entry.RefreshDisplay();
            }
        }

        /*
         * 유닛별 전투 시작 Trigger를 한 번만 실행한다.
         */
        private void DispatchCombatStartOnce(UnitCombatState source /* 효과를 발생시킨 유닛 */)
        {
            // 같은 유닛의 전투 시작 Trigger는 다시 보내지 않는다.
            if (!combatStartDispatchedUnits.Add(source))
            {
                return;
            }

            SkillTrigger.ExecuteCombatStart(this, unitRegistry, source);
        }

        /*
         * 모인 패시브 조건 변경을 활성 효과에 반영한다.
         */
        private void FlushPassiveEffectChanges()
        {
            passiveEffects.FlushPendingChanges(this, unitRegistry);
        }

        /*
         * 지정한 패시브 출처가 만든 상태를 제거한다.
         */
        internal bool RemovePassiveStatus(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusEffectKind kind /* 처리할 종류 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */)
        {
            var removedStatuses = new List<StatusRuntimeInstance>();
            // 같은 종류라도 해당 패시브 출처가 만든 상태만 제거한다.
            if (!target.Statuses.Remove(kind, sourceSkillId, removedStatuses))
            {
                return false;
            }

            target.SyncShield();
            unitRegistry.RefreshDisplay(target);
            for (var i = 0; i < removedStatuses.Count; i++)
            {
                effectManager.RemoveEffect(null, removedStatuses[i]);
            }

            SkillTrigger.ExecuteExpiredStatuses(this, unitRegistry, target, removedStatuses);
            passiveEffects.NotifyStatusChanged(target);
            return true;
        }

        /*
         * 공격자의 피해 Trigger와 추가 피해 상태를 실행한다.
         */
        private void DispatchOutgoingDamageTriggers(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            DamageAttribute attribute /* 피해 속성 */,
            DamageApplicationOptions options /* 처리에 사용할 추가 설정 */,
            InGameResourceChangeResult result /* 처리 결과 */,
            float sourceBaseDamage /* 발생 원본 기본 피해 */)
        {
            // 출처나 실제 피해가 없거나 연쇄 Trigger를 막은 피해는 전달하지 않는다.
            if (options.Source == null || options.SuppressOutgoingDamageTriggers || result.AppliedDamage <= 0f)
            {
                return;
            }

            SkillTrigger.ExecuteOutgoingDamage(
                this,
                unitRegistry,
                options.Source,
                options.SourceSkillId,
                target,
                attribute,
                result.AppliedDamage,
                options.SourceHitWasExecute);

            ApplyOutgoingAdditionalDamageStatuses(target, attribute, options, sourceBaseDamage);
        }

        /*
         * 공격자 상태가 제공하는 추가 속성 피해를 적용한다.
         */
        private void ApplyOutgoingAdditionalDamageStatuses(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            DamageAttribute triggerAttribute /* 트리거 속성 */,
            DamageApplicationOptions options /* 처리에 사용할 추가 설정 */,
            float sourceBaseDamage /* 발생 원본 기본 피해 */)
        {
            if (target.Resources.CurrentHealth <= 0f)
            {
                return;
            }

            var specs = StatusCombatRules.ResolveOutgoingAdditionalDamageSpecs(options.Source, triggerAttribute);
            for (var i = 0; i < specs.Count; i++)
            {
                if (target.Resources.CurrentHealth <= 0f)
                {
                    break;
                }

                var spec = specs[i];
                if (spec.Multiplier <= 0f)
                {
                    continue;
                }

                // 추가 피해가 다시 추가 피해 Trigger를 부르지 않도록 막는다.
                ApplyDamage(
                    target,
                    sourceBaseDamage * spec.Multiplier,
                    spec.DamageAttribute,
                    options.Source,
                    true,
                    0f,
                    0f,
                    options.SourceSkillId,
                    true);
            }
        }

        /*
         * 지정한 종류의 상태를 필요한 수만큼 소비한다.
         */
        public int ConsumeStatusStacks(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusEffectKind kind /* 처리할 종류 */, int stacks /* 중첩 수 */)
        {
            if (stacks <= 0)
            {
                return 0;
            }

            var removedStatuses = new List<StatusRuntimeInstance>();
            var consumed = target.Statuses.ConsumeStacks(kind, stacks, removedStatuses);
            if (consumed > 0)
            {
                for (var i = 0; i < removedStatuses.Count; i++)
                {
                    effectManager.RemoveEffect(null, removedStatuses[i]);
                }

                target.SyncShield();
                unitRegistry.RefreshDisplay(target);
                passiveEffects.NotifyStatusChanged(target);
            }

            return consumed;
        }

        /*
         * 모든 유닛의 상태 지속시간과 만료 처리를 갱신한다.
         */
        private void TickUnitStatuses(float deltaTime /* 이전 갱신 이후 지난 시간 */)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var entries = unitRegistry.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i].Model;

                var removedStatuses = new List<StatusRuntimeInstance>();
                if (model.Statuses.Tick(deltaTime, removedStatuses))
                {
                    model.SyncShield();
                    unitRegistry.RefreshDisplay(model);
                    for (var j = 0; j < removedStatuses.Count; j++)
                    {
                        effectManager.RemoveEffect(null, removedStatuses[j]);
                    }

                    // 만료 상태는 일반 만료와 보호막 만료 Trigger에 각각 전달한다.
                    SkillTrigger.ExecuteExpiredStatuses(this, unitRegistry, model, removedStatuses);
                    passiveEffects.NotifyStatusChanged(model);
                }
            }
        }

        /*
         * 사망한 유닛을 로스터에서 해제
         */
        private void RemoveUnitIfDead(InGameResourceChangeResult result /* 처리 결과 */)
        {
            if (!result.IsDead)
            {
                return;
            }

            var entry = unitRegistry.Find(result.Target);
            unitRegistry.Unregister(result.Target);
            passiveEffects.NotifyRosterChanged();
            entry.HandleDefeat();
            UnitDefeated?.Invoke(result.Target);
        }

    }

    /*
     * 피해와 회복 전후의 자원 값과 사망 여부를 전달한다.
     */
    public readonly struct InGameResourceChangeResult
    {
        /*
         * 자원 변경 결과를 구성한다.
         */
        public InGameResourceChangeResult(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            float previousHealth /* 이전 체력 */,
            float currentHealth /* 현재 체력 */,
            float previousShield /* 이전 보호막 */,
            float currentShield /* 현재 보호막 */,
            float appliedDamage /* 적용된 피해 */,
            bool isDead /* 여부 사망 여부 */)
        {
            Target = target;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            PreviousShield = previousShield;
            CurrentShield = currentShield;
            AppliedDamage = appliedDamage;
            IsDead = isDead;
        }

        public UnitCombatState Target { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float PreviousShield { get; }
        public float CurrentShield { get; }
        public float AppliedDamage { get; }
        public bool IsDead { get; }
        public bool Changed =>
            !Mathf.Approximately(PreviousHealth, CurrentHealth)
            || !Mathf.Approximately(PreviousShield, CurrentShield);

    }
}
