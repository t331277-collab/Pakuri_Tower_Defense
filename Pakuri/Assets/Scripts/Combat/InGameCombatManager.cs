/*
 * 역할: 전투 자원과 상태 변경의 중앙 처리.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// AttackRule 처리에 함께 전달되는 값들을 묶는다.
    public readonly struct AttackRule
    {

        public AttackRule(
            UnitCombatState source,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            string sourceSkillId,
            bool suppressOutgoingDamageTriggers,
            bool sourceHitWasExecute,
            string damageMeterSourceId,
            float finalDamageBonus,
            bool isTrigger = false)
        {
            Source = source;
            CriticalAllowed = criticalAllowed;
            CritChanceBonus = critChanceBonus;
            CritDamageBonus = critDamageBonus;
            SourceSkillId = sourceSkillId;
            SuppressOutgoingDamageTriggers = suppressOutgoingDamageTriggers;
            SourceHitWasExecute = sourceHitWasExecute;
            DamageMeterSourceId = damageMeterSourceId;
            FinalDamageBonus = finalDamageBonus;
            IsTrigger = isTrigger;
        }

        public UnitCombatState Source { get; }
        public bool CriticalAllowed { get; }
        public float CritChanceBonus { get; }
        public float CritDamageBonus { get; }
        public string SourceSkillId { get; }
        public bool SuppressOutgoingDamageTriggers { get; }
        public bool SourceHitWasExecute { get; }
        public string DamageMeterSourceId { get; }
        public float FinalDamageBonus { get; }
        internal bool IsTrigger { get; }
    }

    public class InGameCombatManager : MonoBehaviour
    {
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        private EnemyActionController enemyActionController;
        private readonly SkillExecution skillExecution = new SkillExecution();
        [SerializeField] private PlayerCombatInputController playerCombatControl;
        private readonly HashSet<UnitCombatState> combatStartDispatchedUnits = new HashSet<UnitCombatState>();
        [SerializeField] private bool enemyCombatSimulationEnabled = true;
        [SerializeField] private bool skillExecutionEnabled = true;
        [SerializeField] private EffectManager effectManager;

        public UnitSpawnManager Units
        {
            get
            {
                if (unitSpawnManager == null)
                {
                    unitSpawnManager = FindFirstObjectByType<UnitSpawnManager>();
                }

                return unitSpawnManager != null
                    ? unitSpawnManager
                    : throw new InvalidOperationException("UnitSpawnManager is required.");
            }
        }
        public EffectManager Effects => effectManager;
        internal SkillExecution SkillExecution => skillExecution;

        public int ActiveEnemyCount => Units.EnemyCount;
        public event Action<AttackRule, InGameResourceChangeResult> DamageApplied;
        public event Action<UnitCombatState> UnitDefeated;

        /// 초기화
        private void Awake()
        {
            enemyActionController = new EnemyActionController(Units, skillExecution, this);
            combatStartDispatchedUnits.Clear();
            SkillTrigger.Reset(this);
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            if (skillExecutionEnabled)
            {
                TickSkillStates(Time.deltaTime);
                skillExecution.TryExecuteAutomaticSkills(
                    Units,
                    this,
                    (entry, runtime) => playerCombatControl.CanUseAutoSkill(entry, Units));
                playerCombatControl.HandleManualInput(
                    Units,
                    skillExecution,
                    this);
            }

            if (enemyCombatSimulationEnabled)
            {
                enemyActionController.Tick(Time.deltaTime);
            }

            TickUnitStatuses(Time.deltaTime);
        }

        /// 게임 시간 관리

        private void TickSkillStates(float deltaTime)
        {
            var entries = Units.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry != null && entry.Model != null)
                {
                    entry.Model.SkillState.Tick(deltaTime);
                }
            }
        }

        /// 몬스터 전투 등록

        internal void NotifyPlayerUnitRegistered(UnitCombatState model)
        {
            if (PlayerCombatInputController.IsSelectedPlayerModel(model))
            {
                playerCombatControl.ApplyAutoSkillModeToSelectedPlayer(Units);
            }

            RefreshPassiveEffects(model);
            DispatchCombatStartOnce(model);
        }

        /// 현재 학습한 passive 목록을 갱신한다.
        internal void RefreshPassiveEffects(UnitCombatState model)
        {
            skillExecution.ExecutePassiveEffects(this, Units, model);
        }

        internal void NotifyEnemyUnitRegistered(EnemyCombatState model)
        {
            var entries = Units.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var owner = entries[i]?.Model;
                if (owner != null && !(owner is EnemyCombatState))
                {
                    skillExecution.ExecutePassiveEffects(
                        this,
                        Units,
                        owner,
                        enemyTargetsOnly: true);
                }
            }
            DispatchCombatStartOnce(model);
        }

        /// 피해 계산과 자원 변경 후 화면 갱신, 후속 Trigger, 사망 정리를 처리한다.
        public InGameResourceChangeResult ApplyDamage(
            UnitCombatState target,
            float baseDamage,
            DamageAttribute attribute,
            UnitCombatState source,
            bool criticalAllowed = false,
            float critChanceBonus = 0f,
            float critDamageBonus = 0f,
            string sourceSkillId = null,
            bool suppressOutgoingDamageTriggers = false,
            bool sourceHitWasExecute = false,
            string damageMeterSourceId = null,
            float finalDamageMultiplier = 1f,
            bool isTrigger = false)
        {
            // 보호막·HP를 차감하고 피해 결과를 만든다.
            var depletedShields = new List<StatusRuntimeInstance>();
            var absorbedShields = new List<ShieldAbsorptionRecord>();
            var finalDamageBonus = Mathf.Max(0f, finalDamageMultiplier) - 1f;
            var attackRule = new AttackRule(source, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers, sourceHitWasExecute, damageMeterSourceId, finalDamageBonus, isTrigger);
            var result = ApplyDamageToResources(target, baseDamage, attribute, attackRule, depletedShields, absorbedShields);

            if (!result.Changed)
            {
                return result;
            }

            // 보호막을 모두 없애면 이펙트 삭제.
            if (depletedShields.Count > 0)
            {
                for (var i = 0; i < depletedShields.Count; i++)
                {
                    effectManager.SignalStatusEffectEnded(depletedShields[i]);
                }
            }

            // 피해 이벤트와 유닛 표시를 갱신한다.
            DamageApplied?.Invoke(attackRule, result);
            var damagedEntry = Units.Find(result.Target);
            damagedEntry.RefreshDisplay();
            damagedEntry.ShowDamage(result.AppliedDamage, result.IsDead);
            // 일반 피해만 보호막·상태·피해·처치 후속반응을 발행한다.
            if (!attackRule.IsTrigger)
            {
                SkillTrigger.ExecuteShieldAbsorbs(this, Units, target, source, absorbedShields);
                SkillTrigger.ExecuteExpiredStatuses(this, Units, target, depletedShields);
                DispatchOutgoingDamageTriggers(target, attribute, attackRule, result, baseDamage);
                if (result.IsDead && attackRule.Source != null)
                {
                    SkillTrigger.ExecuteKill(
                        this,
                        Units,
                        attackRule.Source,
                        attackRule.SourceSkillId,
                        target,
                        attribute,
                        result.AppliedDamage,
                        attackRule.SourceHitWasExecute);
                }
            }

            // 사망한 유닛을 전투 Registry에서 정리한다.
            RemoveUnitIfDead(result);
            return result;
        }

        public InGameResourceChangeResult Heal(UnitCombatState target, float amount)
        {

            var result = HealResources(target, target.IsNexus ? 0f : amount);
            if (!result.Changed)
            {
                return result;
            }

            Units.Find(result.Target).RefreshDisplay();
            return result;
        }

        private static InGameResourceChangeResult ApplyDamageToResources(
            UnitCombatState target,
            float baseDamage,
            DamageAttribute attribute,
            AttackRule attackRule,
            ICollection<StatusRuntimeInstance> depletedShields,
            ICollection<ShieldAbsorptionRecord> absorbedShields)
        {
            var resources = target.Resources;
            var beforeHealth = Mathf.Max(0f, resources.CurrentHealth);
            var beforeShield = target.GetTotalShield();
            var currentHealth = beforeHealth;
            var currentShield = beforeShield;
            var finalDamage = 0f;

            if (baseDamage > 0f)
            {
                finalDamage = DamageCalculator.CalculateFinalDamage(target, baseDamage, attribute, attackRule);

                target.Statuses.RecordIncomingDamage(attribute, finalDamage);

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

        private static InGameResourceChangeResult HealResources(UnitCombatState target, float amount)
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

        private static float Round(float value)
        {
            return Mathf.Round(Mathf.Max(0f, value));
        }

        public StatusRuntimeInstance ApplyStatus(
            UnitCombatState target,
            StatusRuntimeData statusData,
            int stacks,
            float durationSeconds,
            int maxStacks,
            bool permanent,
            bool refreshDuration,
            UnitCombatState source)
        {

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
            target.SyncShield();
            Units.RefreshDisplay(target);
            ShowStatusEffectVisual(target, status);
            return status;
        }

        public StatusRuntimeInstance ApplyShieldStatus(
            UnitCombatState target,
            StatusRuntimeData statusData,
            float shieldAmount,
            float durationSeconds,
            int stacks,
            int maxStacks,
            bool permanent,
            bool refreshDuration,
            UnitCombatState source)
        {

            if (statusData.Kind != StatusEffectKind.Shield || target.IsNexus)
            {
                return null;
            }

            var adjustedShieldAmount = Mathf.Max(0f, shieldAmount) * StatusCombatRules.ShieldReceivedMultiplier(target);
            var status = target.Statuses.Apply(
                statusData,
                stacks,
                durationSeconds,
                maxStacks,
                permanent,
                refreshDuration,
                adjustedShieldAmount);
            status.SetSourceUnit(source);
            target.SyncShield();
            Units.RefreshDisplay(target);
            ShowStatusEffectVisual(target, status);
            return status;
        }

        public bool ExtendStatusDuration(UnitCombatState target, StatusEffectKind kind, float durationDelta)
        {
            if (kind == StatusEffectKind.None || durationDelta <= 0f || target.IsNexus)
            {
                return false;
            }

            var changed = target.Statuses.ExtendDurations(
                kind,
                durationDelta,
                status => !status.Permanent && (!status.IsShieldStatus || status.RemainingShieldAmount > 0f));
            if (!changed)
            {
                return false;
            }

            target.SyncShield();
            Units.RefreshDisplay(target);

            var activeStatuses = target.Statuses.ActiveStatuses;
            for (var i = 0; i < activeStatuses.Count; i++)
            {
                var status = activeStatuses[i];
                if (status.Kind != kind)
                {
                    continue;
                }

                ShowStatusEffectVisual(target, status);
            }

            return true;
        }

        private void ShowStatusEffectVisual(
            UnitCombatState target,
            StatusRuntimeInstance status)
        {
            var instance = effectManager.CreateEffect(new EffectCreateRequest(
                status.SourceData.RuntimeVisual,
                status.SourceData.StatusEffectPrefab,
                "RuntimeStatusVisual_" + status.SourceSkillId,
                Units.Find(target).Transform.position,
                Quaternion.identity,
                Units.Find(target).Transform,
                status,
                false,
                false,
                false));
            if (instance != null)
            {
                BuffSkillActor.Attach(instance).InitializePersistent(effectManager, status);
            }
        }

        /// CombatState를 초기 런타임 상태로 되돌린다.
        public void ResetCombatState()
        {
            StopAllCoroutines();
            playerCombatControl.ClearManualInput();
            effectManager.ClearEffects();

            combatStartDispatchedUnits.Clear();
            SkillTrigger.Reset(this);

            var entries = Units.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var model = entry.Model;

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

        private void DispatchCombatStartOnce(UnitCombatState source)
        {

            if (!combatStartDispatchedUnits.Add(source))
            {
                return;
            }

            SkillTrigger.ExecuteCombatStart(this, Units, source);
        }

        internal bool RemovePassiveStatus(UnitCombatState target, StatusEffectKind kind, string sourceSkillId)
        {
            var removedStatuses = new List<StatusRuntimeInstance>();

            if (!target.Statuses.Remove(kind, sourceSkillId, removedStatuses))
            {
                return false;
            }

            target.SyncShield();
            Units.RefreshDisplay(target);
            for (var i = 0; i < removedStatuses.Count; i++)
            {
                effectManager.SignalStatusEffectEnded(removedStatuses[i]);
            }

            SkillTrigger.ExecuteExpiredStatuses(this, Units, target, removedStatuses);
            return true;
        }

        private void DispatchOutgoingDamageTriggers(
            UnitCombatState target,
            DamageAttribute attribute,
            AttackRule attackRule,
            InGameResourceChangeResult result,
            float sourceBaseDamage)
        {

            if (attackRule.Source == null || attackRule.SuppressOutgoingDamageTriggers || result.AppliedDamage <= 0f)
            {
                return;
            }

            SkillTrigger.ExecuteOutgoingDamage(
                this,
                Units,
                attackRule.Source,
                attackRule.SourceSkillId,
                target,
                attribute,
                result.AppliedDamage,
                attackRule.SourceHitWasExecute);

            ApplyOutgoingAdditionalDamageStatuses(target, attribute, attackRule, sourceBaseDamage);
        }

        private void ApplyOutgoingAdditionalDamageStatuses(
            UnitCombatState target,
            DamageAttribute triggerAttribute,
            AttackRule attackRule,
            float sourceBaseDamage)
        {
            if (target.Resources.CurrentHealth <= 0f)
            {
                return;
            }

            var specs = StatusCombatRules.OutgoingAdditionalDamageSpecs(attackRule.Source, triggerAttribute);
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

                ApplyDamage(
                    target,
                    sourceBaseDamage * spec.Multiplier,
                    spec.DamageAttribute,
                    attackRule.Source,
                    true,
                    0f,
                    0f,
                    attackRule.SourceSkillId,
                    true);
            }
        }

        public int ConsumeStatusStacks(UnitCombatState target, StatusEffectKind kind, int stacks)
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
                    effectManager.SignalStatusEffectEnded(removedStatuses[i]);
                }

                target.SyncShield();
                Units.RefreshDisplay(target);
            }

            return consumed;
        }

        private void TickUnitStatuses(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var entries = Units.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i].Model;

                var removedStatuses = new List<StatusRuntimeInstance>();
                if (model.Statuses.Tick(deltaTime, removedStatuses))
                {
                    model.SyncShield();
                    Units.RefreshDisplay(model);
                    for (var j = 0; j < removedStatuses.Count; j++)
                    {
                        effectManager.SignalStatusEffectEnded(removedStatuses[j]);
                    }

                    SkillTrigger.ExecuteExpiredStatuses(this, Units, model, removedStatuses);
                }
            }
        }

        private void RemoveUnitIfDead(InGameResourceChangeResult result)
        {
            if (!result.IsDead)
            {
                return;
            }

            Units.DefeatUnit(result.Target);
            UnitDefeated?.Invoke(result.Target);
        }

    }

    /// InGameResourceChangeResult 처리에 함께 전달되는 값들을 묶는다.
    public readonly struct InGameResourceChangeResult
    {

        public InGameResourceChangeResult(
            UnitCombatState target,
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
