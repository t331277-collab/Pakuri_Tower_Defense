using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

/*
 * 인게임 전투 흐름을 조율하는 중앙 컴포넌트와 피해 적용 데이터를 정의한다.
 * 유닛 로스터를 기준으로 스킬, 입력, 적 행동, 패시브, 상태 갱신 순서를 연결하고
 * 피해·회복·상태 변화 결과를 액터 표시, 효과, Trigger, 사망 처리에 전달한다.
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
            BaseUnitRuntimeModel source,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            string sourceSkillId,
            bool suppressOutgoingDamageTriggers,
            bool sourceHitWasExecute,
            string damageMeterSourceId)
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

        public BaseUnitRuntimeModel Source { get; }
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
        private readonly UnitRosterService roster = new UnitRosterService();
        private EnemyCombatController enemyController;
        private readonly SkillExecutionSystem skillExecution = new SkillExecutionSystem();
        private readonly PassiveEffectRuntime passiveEffects = new PassiveEffectRuntime();
        [SerializeField] private PlayerCombatInputController playerCombatControl;
        private readonly HashSet<BaseUnitRuntimeModel> combatStartDispatchedUnits = new HashSet<BaseUnitRuntimeModel>();
        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private bool logSkillExecutionContracts;
        [SerializeField] private EffectManager effectManager;

        public UnitRosterService Roster => roster;
        public EffectManager Effects => effectManager;
        internal PassiveEffectRuntime PassiveEffects => passiveEffects;
        internal SkillExecutionSystem SkillExecution => skillExecution;
        internal bool LogSkillExecutionContracts => logSkillExecutionContracts;

        public int ActiveEnemyCount => roster.EnemyCount;
        public event Action<DamageApplicationOptions, InGameResourceChangeResult> DamageApplied;

        /*
         * 전투 시작 전에 로스터와 전투 기록을 초기화한다.
         */
        private void Awake()
        {
            enemyController = new EnemyCombatController(roster, skillExecution, this);
            roster.Clear();
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
                skillExecution.Tick(
                    roster,
                    this,
                    Time.deltaTime,
                    logSkillExecutionContracts,
                    (entry, runtime) => playerCombatControl.CanUseAutoSkill(entry, roster));
                playerCombatControl.HandleManualInput(
                    roster,
                    skillExecution,
                    this,
                    Time.deltaTime,
                    logSkillExecutionContracts);
            }

            if (enemyCombatSimulationEnabled)
            {
                enemyController.Tick(Time.deltaTime);
            }

            TickUnitStatuses(Time.deltaTime);
            FlushPassiveEffectChanges();
        }

        /*
         * 플레이어 몬스터를 등록하고 전투 시작 처리를 실행한다.
         */
        public UnitRosterEntry RegisterPlayerMonster(MonsterUnitRuntimeModel model, MonsterUnitActor actor, Transform hitboxRoot)
        {
            var entry = roster.Register(model, actor, hitboxRoot);
            passiveEffects.NotifyRosterChanged();
            if (PlayerCombatInputController.IsSelectedPlayerModel(model))
            {
                playerCombatControl.ApplyAutoSkillModeToSelectedPlayer(roster);
            }

            DispatchCombatStartOnce(model);
            return entry;
        }

        /*
         * 적을 등록하고 전투 시작 처리를 실행한다.
         */
        public UnitRosterEntry RegisterEnemy(EnemyUnitRuntimeModel model, EnemyUnitActor actor, Transform hitboxRoot)
        {
            var entry = roster.Register(model, actor, hitboxRoot);
            passiveEffects.NotifyRosterChanged();
            DispatchCombatStartOnce(model);
            return entry;
        }

        /*
         * 넥서스를 전투 로스터에 등록한다.
         */
        public UnitRosterEntry RegisterNexus(NexusUnitRuntimeModel model, NexusUnitActor actor, Transform hitboxRoot)
        {
            var entry = roster.Register(model, actor, hitboxRoot);
            passiveEffects.NotifyRosterChanged();
            return entry;
        }

        /*
         * 유닛을 로스터에서 해제하고 연결된 Actor를 제거한다.
         */
        public bool DespawnUnit(BaseUnitRuntimeModel model)
        {
            var entry = roster.Find(model);
            var actor = entry.Actor;
            roster.Unregister(model);
            passiveEffects.NotifyRosterChanged();
            Destroy(actor.gameObject);

            return true;
        }

        /*
         * 피해를 적용하고 표시, Trigger, 사망 처리를 실행한다.
         */
        public InGameResourceChangeResult ApplyDamage(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute,
            BaseUnitRuntimeModel source,
            bool criticalAllowed = false,
            float critChanceBonus = 0f,
            float critDamageBonus = 0f,
            string sourceSkillId = null,
            bool suppressOutgoingDamageTriggers = false,
            bool sourceHitWasExecute = false,
            string damageMeterSourceId = null)
        {
            var depletedShields = new List<UnitStatusRuntime>();
            var absorbedShields = new List<ShieldAbsorbRecord>();
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
            }
            // 통계와 UI에는 실제 자원 변화 결과만 전달한다.
            DamageApplied?.Invoke(options, result);
            var damagedEntry = roster.Find(result.Target);
            damagedEntry.RefreshActor();
            damagedEntry.ShowDamage(result.AppliedDamage, result.IsDead);
            SkillTriggerRuntime.ExecuteShieldAbsorbs(this, roster, target, source, absorbedShields);
            SkillTriggerRuntime.ExecuteExpiredStatuses(this, roster, target, depletedShields);
            DispatchOutgoingDamageTriggers(target, attribute, options, result, baseDamage);
            if (result.IsDead && options.Source != null)
            {
                SkillTriggerRuntime.ExecuteKill(
                    this,
                    roster,
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
        public InGameResourceChangeResult Heal(BaseUnitRuntimeModel target, float amount)
        {
            // 넥서스는 회복량을 0으로 처리한다.
            var result = HealResources(target, target.IsNexus ? 0f : amount);
            if (!result.Changed)
            {
                return result;
            }

            passiveEffects.NotifyResourceChanged(result);
            roster.Find(result.Target).RefreshActor();
            return result;
        }

        /*
         * 피해를 보호막과 체력에 적용하고 변경 결과를 만든다.
         */
        private static InGameResourceChangeResult ApplyDamageToResources(
            BaseUnitRuntimeModel target,
            float baseDamage,
            DamageAttribute attribute,
            DamageApplicationOptions options,
            ICollection<UnitStatusRuntime> depletedShields,
            ICollection<ShieldAbsorbRecord> absorbedShields)
        {
            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = target.GetTotalShield();
            var currentHealth = beforeHealth;
            var currentShield = beforeShield;
            var finalDamage = 0f;

            if (baseDamage > 0f)
            {
                finalDamage = DamageCalculator.CalculateDamage(target, baseDamage, attribute, options);

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
        private static InGameResourceChangeResult HealResources(BaseUnitRuntimeModel target, float amount)
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
        private static float Round(float value)
        {
            return Mathf.Round(Mathf.Max(0f, value));
        }

        /*
         * 일반 상태를 적용하고 상태 표시와 패시브 조건을 갱신한다.
         */
        public UnitStatusRuntime ApplyStatus(
            BaseUnitRuntimeModel target,
            RuntimeStatusData statusData,
            int stacks,
            float durationSeconds,
            int maxStacks,
            bool permanent,
            bool refreshDuration,
            BaseUnitRuntimeModel source)
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
            roster.RefreshActor(target);
            effectManager.SpawnOrRefreshStatusVisual(target, roster.Find(target).Transform, statusData, status);
            return status;
        }

        /*
         * 보호막 상태를 적용하고 상태 표시와 패시브 조건을 갱신한다.
         */
        public UnitStatusRuntime ApplyShieldStatus(
            BaseUnitRuntimeModel target,
            RuntimeStatusData statusData,
            float shieldAmount,
            float durationSeconds,
            int stacks,
            int maxStacks,
            bool permanent,
            bool refreshDuration,
            BaseUnitRuntimeModel source)
        {
            // 넥서스에는 보호막 상태를 적용하지 않는다.
            if (statusData.Kind != StatusEffectKind.Shield || target.IsNexus)
            {
                return null;
            }

            // 대상의 보호막 수신 배율을 먼저 반영한다.
            var adjustedShieldAmount = Mathf.Max(0f, shieldAmount) * StatusEffectRules.ResolveShieldReceivedMultiplier(target);
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
            roster.RefreshActor(target);
            effectManager.SpawnOrRefreshStatusVisual(target, roster.Find(target).Transform, statusData, status);
            return status;
        }

        /*
         * 지정한 상태의 지속시간을 연장하고 표시를 갱신한다.
         */
        public bool ExtendStatusDuration(BaseUnitRuntimeModel target, StatusEffectKind kind, float durationDelta)
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
            roster.RefreshActor(target);

            var activeStatuses = target.Statuses.ActiveStatuses;
            for (var i = 0; i < activeStatuses.Count; i++)
            {
                var status = activeStatuses[i];
                if (status.Kind != kind)
                {
                    continue;
                }

                effectManager.SpawnOrRefreshStatusVisual(target, roster.Find(target).Transform, status.SourceData, status);
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
            effectManager.ClearRuntimeSkillObjects();

            combatStartDispatchedUnits.Clear();
            passiveEffects.Reset();

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var model = entry.Model;

                // 몬스터는 전용 서비스로, 나머지는 공통 상태만 초기화한다.
                if (model is MonsterUnitRuntimeModel monsterModel)
                {
                    MonsterUnitRuntimeStateService.ResetTransientCombatState(monsterModel);
                }
                else
                {
                    model.Statuses.Clear();
                    model.Resources.DirectShield = 0f;
                    model.Resources.CurrentShield = 0f;
                }

                model.SyncShield();
                entry.RefreshActor();
            }
        }

        /*
         * 유닛별 전투 시작 Trigger를 한 번만 실행한다.
         */
        private void DispatchCombatStartOnce(BaseUnitRuntimeModel source)
        {
            // 같은 유닛의 전투 시작 Trigger는 다시 보내지 않는다.
            if (!combatStartDispatchedUnits.Add(source))
            {
                return;
            }

            SkillTriggerRuntime.ExecuteCombatStart(this, roster, source);
        }

        /*
         * 모인 패시브 조건 변경을 활성 효과에 반영한다.
         */
        private void FlushPassiveEffectChanges()
        {
            passiveEffects.FlushPendingChanges(this, roster);
        }

        /*
         * 지정한 패시브 출처가 만든 상태를 제거한다.
         */
        internal bool RemovePassiveStatus(BaseUnitRuntimeModel target, StatusEffectKind kind, string sourceSkillId)
        {
            var removedStatuses = new List<UnitStatusRuntime>();
            // 같은 종류라도 해당 패시브 출처가 만든 상태만 제거한다.
            if (!target.Statuses.Remove(kind, sourceSkillId, removedStatuses))
            {
                return false;
            }

            target.SyncShield();
            roster.RefreshActor(target);
            for (var i = 0; i < removedStatuses.Count; i++)
            {
                effectManager.RemoveStatusVisual(target, removedStatuses[i]);
            }

            SkillTriggerRuntime.ExecuteExpiredStatuses(this, roster, target, removedStatuses);
            passiveEffects.NotifyStatusChanged(target);
            return true;
        }

        /*
         * 공격자의 피해 Trigger와 추가 피해 상태를 실행한다.
         */
        private void DispatchOutgoingDamageTriggers(
            BaseUnitRuntimeModel target,
            DamageAttribute attribute,
            DamageApplicationOptions options,
            InGameResourceChangeResult result,
            float sourceBaseDamage)
        {
            // 출처나 실제 피해가 없거나 연쇄 Trigger를 막은 피해는 전달하지 않는다.
            if (options.Source == null || options.SuppressOutgoingDamageTriggers || result.AppliedDamage <= 0f)
            {
                return;
            }

            SkillTriggerRuntime.ExecuteOutgoingDamage(
                this,
                roster,
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
            BaseUnitRuntimeModel target,
            DamageAttribute triggerAttribute,
            DamageApplicationOptions options,
            float sourceBaseDamage)
        {
            if (target.Resources.CurrentHealth <= 0f)
            {
                return;
            }

            var specs = StatusEffectRules.ResolveOutgoingAdditionalDamageSpecs(options.Source, triggerAttribute);
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
         * 문자열 태그 상태를 지정 수만큼 소비한다.
         */
        public int ConsumeStatusStacks(BaseUnitRuntimeModel target, string statusTag, int stacks)
        {
            if (stacks <= 0)
            {
                return 0;
            }

            var consumed = target.Statuses.ConsumeStacks(statusTag, stacks);
            if (consumed > 0)
            {
                target.SyncShield();
                roster.RefreshActor(target);
                passiveEffects.NotifyStatusChanged(target);
            }

            return consumed;
        }

        /*
         * 모든 유닛의 상태 지속시간과 만료 처리를 갱신한다.
         */
        private void TickUnitStatuses(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i].Model;

                var removedStatuses = new List<UnitStatusRuntime>();
                if (model.Statuses.Tick(deltaTime, removedStatuses))
                {
                    model.SyncShield();
                    roster.RefreshActor(model);
                    // 만료 상태는 일반 만료와 보호막 만료 Trigger에 각각 전달한다.
                    SkillTriggerRuntime.ExecuteExpiredStatuses(this, roster, model, removedStatuses);
                    passiveEffects.NotifyStatusChanged(model);
                }
            }
        }

        /*
         * 사망한 유닛을 로스터에서 해제
         */
        private void RemoveUnitIfDead(InGameResourceChangeResult result)
        {
            if (!result.IsDead)
            {
                return;
            }

            var entry = roster.Find(result.Target);
            roster.Unregister(result.Target);
            passiveEffects.NotifyRosterChanged();
            // Actor가 유닛 유형에 맞는 패배 연출과 통지를 처리한다.
            entry.ShowDefeated();
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
            BaseUnitRuntimeModel target,
            float previousHealth,
            float currentHealth,
            float previousShield,
            float currentShield,
            float appliedDamage,
            bool isDead)
        {
            Target = target;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            PreviousShield = previousShield;
            CurrentShield = currentShield;
            AppliedDamage = appliedDamage;
            IsDead = isDead;
        }

        public BaseUnitRuntimeModel Target { get; }
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
