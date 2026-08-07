/*
 * 역할: 전투 자원과 상태 변경의 중앙 처리.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
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
            string sourceSkillName,
            bool suppressOutgoingDamageTriggers,
            bool sourceHitWasExecute,
            string damageMeterSourceName,
            float damageMultiplier,
            float finalDamageModifier = 1f,
            float criticalFinalDamageModifier = 1f,
            bool isTrigger = false)
        {
            Source = source;
            CriticalAllowed = criticalAllowed;
            CritChanceBonus = critChanceBonus;
            CritDamageBonus = critDamageBonus;
            SourceSkillName = sourceSkillName;
            SuppressOutgoingDamageTriggers = suppressOutgoingDamageTriggers;
            SourceHitWasExecute = sourceHitWasExecute;
            DamageMeterSourceName = damageMeterSourceName;
            DamageMultiplier = damageMultiplier;
            FinalDamageModifier = finalDamageModifier;
            CriticalFinalDamageModifier = criticalFinalDamageModifier;
            IsTrigger = isTrigger;
        }

        public UnitCombatState Source { get; }
        public bool CriticalAllowed { get; }
        public float CritChanceBonus { get; }
        public float CritDamageBonus { get; }
        public string SourceSkillName { get; }
        public bool SuppressOutgoingDamageTriggers { get; }
        public bool SourceHitWasExecute { get; }
        public string DamageMeterSourceName { get; }
        public float DamageMultiplier { get; }
        public float FinalDamageModifier { get; }
        public float CriticalFinalDamageModifier { get; }
        public float FinalDamageMultiplier => DamageMultiplier;
        internal bool IsTrigger { get; }
    }

    public class InGameCombatManager : MonoBehaviour
    {
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        private EnemyActionController enemyActionController;
        private SummonActionController summonActionController;
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
            summonActionController = new SummonActionController(Units);
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

            summonActionController.Tick(Time.deltaTime);
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
                    var skills = entry.Model.SkillState.ActiveSkills;
                    for (var skillIndex = 0; skillIndex < skills.Count; skillIndex++)
                    {
                        var runtime = skills[skillIndex];
                        if (SkillExecution.ConsumeReloadCompleteEvent(runtime))
                        {
                            SkillTrigger.ExecuteReloadComplete(
                                this,
                                Units,
                                entry.Model,
                                runtime.SkillName);
                        }
                    }
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
        }

        /// 등록된 플레이어 전체의 Stage 시작 패시브와 사건을 한 번 실행한다.
        internal void BeginPlayerCombat(bool isBossEncounter)
        {
            var players = Units.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i]?.Model;
                if (model == null
                    || model.IsNexus
                    || combatStartDispatchedUnits.Contains(model))
                {
                    continue;
                }

                model.SkillState.RefreshLearnedRuntimeValues(model);
                RefreshPassiveEffects(model);
                DispatchCombatStartOnce(model);
                if (isBossEncounter)
                {
                    SkillTrigger.ExecuteBossCombatStart(this, Units, model);
                }
            }

            LogPlayerCriticalStats();
        }

        // 삭제 대상: 치명타 수치 검증이 끝나면 임시 Stage 시작 로그와 함께 제거한다.
        private void LogPlayerCriticalStats()
        {
            var players = Units.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i]?.Model;
                if (model == null || model.IsNexus)
                {
                    continue;
                }

                var unitName = model.Identity?.DefinitionName ?? "unknown";
                var summaries = new List<string>();
                var activeSkills = model.SkillState?.ActiveSkills;
                if (activeSkills != null)
                {
                    for (var skillIndex = 0; skillIndex < activeSkills.Count; skillIndex++)
                    {
                        var runtime = activeSkills[skillIndex];
                        if (runtime?.Data == null)
                        {
                            continue;
                        }

                        var snapshot = SkillExecutionRules.BuildExecutionData(model, runtime, Units);
                        var attackRule = new AttackRule(
                            source: model,
                            criticalAllowed: true,
                            critChanceBonus: snapshot.CritChanceBonus,
                            critDamageBonus: snapshot.CritDamageBonus,
                            sourceSkillName: runtime.Data.SkillName,
                            suppressOutgoingDamageTriggers: true,
                            sourceHitWasExecute: false,
                            damageMeterSourceName: null,
                            damageMultiplier: 1f,
                            finalDamageModifier: snapshot.FinalDamageModifier,
                            criticalFinalDamageModifier: snapshot.CriticalFinalDamageModifier);
                        var finalChance = DamageCalculator.ResolveCriticalChance(null, attackRule);
                        var finalDamage = DamageCalculator.ResolveCriticalDamageMultiplier(null, attackRule)
                            * Mathf.Max(0f, attackRule.FinalDamageModifier)
                            * Mathf.Max(0f, attackRule.CriticalFinalDamageModifier);
                        summaries.Add(
                            $"{runtime.Data.SkillName}: chance={(finalChance * 100f).ToString("F2", CultureInfo.InvariantCulture)}%, damage=x{finalDamage.ToString("F3", CultureInfo.InvariantCulture)}");
                    }
                }

                if (summaries.Count == 0)
                {
                    var attackRule = new AttackRule(
                        source: model,
                        criticalAllowed: true,
                        critChanceBonus: 0f,
                        critDamageBonus: 0f,
                        sourceSkillName: null,
                        suppressOutgoingDamageTriggers: true,
                        sourceHitWasExecute: false,
                        damageMeterSourceName: null,
                        damageMultiplier: 1f);
                    var finalChance = DamageCalculator.ResolveCriticalChance(null, attackRule);
                    var finalDamage = DamageCalculator.ResolveCriticalDamageMultiplier(null, attackRule)
                        * Mathf.Max(0f, attackRule.FinalDamageModifier)
                        * Mathf.Max(0f, attackRule.CriticalFinalDamageModifier);
                    summaries.Add(
                        $"base: chance={(finalChance * 100f).ToString("F2", CultureInfo.InvariantCulture)}%, damage=x{finalDamage.ToString("F3", CultureInfo.InvariantCulture)}");
                }

                Debug.Log(
                    $"[삭제대상][FinalCritStats] StageStart TargetIndependent Unit={unitName} "
                    + string.Join("; ", summaries));
            }
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

        /// 피해 계산과 사망 정리를 처리한다.
        public InGameResourceChangeResult ApplyDamage(
            UnitCombatState target,
            float baseDamage,
            DamageAttribute attribute,
            UnitCombatState source,
            bool criticalAllowed = false,
            float critChanceBonus = 0f,
            float critDamageBonus = 0f,
            string sourceSkillName = null,
            bool suppressOutgoingDamageTriggers = false,
            bool sourceHitWasExecute = false,
            string damageMeterSourceName = null,
            float damageMultiplier = 1f,
            float finalDamageModifier = 1f,
            float criticalFinalDamageModifier = 1f,
            bool isTrigger = false)
        {
            // 보호막·HP를 차감하고 피해 결과를 만든다.
            var depletedShields = new List<StatusRuntimeInstance>();
            var absorbedShields = new List<ShieldAbsorptionRecord>();
            var fateCoinBonus = source != null
                && !isTrigger
                && criticalAllowed
                && source.Artifacts != null
                && source.Artifacts.HasActiveEffect("coin-of-fate-effect")
                ? source.Artifacts.FateCoinCritChanceBonus
                : 0f;
            var attackRule = new AttackRule(source, criticalAllowed, critChanceBonus + fateCoinBonus, critDamageBonus, sourceSkillName, suppressOutgoingDamageTriggers, sourceHitWasExecute, damageMeterSourceName, Mathf.Max(0f, damageMultiplier), Mathf.Max(0f, finalDamageModifier), Mathf.Max(0f, criticalFinalDamageModifier), isTrigger);
            var result = ApplyDamageToResources(target, baseDamage, attribute, attackRule, depletedShields, absorbedShields);

            if (!result.Changed)
            {
                return result;
            }

            if (source != null
                && !isTrigger
                && criticalAllowed
                && baseDamage > 0f
                && source.Artifacts != null
                && source.Artifacts.HasActiveEffect("coin-of-fate-effect"))
            {
                source.Artifacts.AdvanceFateCoin(result.IsCritical);
            }

            // 보호막을 모두 없애면 연결된 시각 효과를 삭제한다.
            if (depletedShields.Count > 0)
            {
                for (var i = 0; i < depletedShields.Count; i++)
                {
                    effectManager.RemoveEffect(status: depletedShields[i]);
                }
            }

            // 피해 이벤트와 유닛 표시를 갱신한다.
            DamageApplied?.Invoke(attackRule, result);
            var damagedEntry = Units.Find(result.Target);
            damagedEntry.RefreshDisplay();
            damagedEntry.ShowDamage(result.AppliedDamage, result.IsDead, result.IsCritical);
            // 일반 피해만 보호막·상태·피해·처치 후속반응을 발행한다.
            if (!attackRule.IsTrigger)
            {
                SkillTrigger.ExecuteShieldAbsorbs(this, Units, target, source, absorbedShields);
                SkillTrigger.ExecuteShieldBreaks(this, Units, target, depletedShields);
                SkillTrigger.ExecuteExpiredStatuses(this, Units, target, depletedShields);
                if (!attackRule.SuppressOutgoingDamageTriggers)
                {
                    SkillTrigger.ExecuteOutgoingDamage(
                        this,
                        Units,
                        attackRule.Source,
                        attackRule.SourceSkillName,
                        target,
                        attribute,
                        result.AppliedDamage,
                        attackRule.SourceHitWasExecute,
                        baseDamage,
                        result.IsCritical);
                }
                if (result.IsDead && attackRule.Source != null)
                {
                    SkillTrigger.ExecuteKill(
                        this,
                        Units,
                        attackRule.Source,
                        attackRule.SourceSkillName,
                        target,
                        attribute,
                        result.AppliedDamage,
                        attackRule.SourceHitWasExecute);
                }
            }

            // 유닛이 사망했다면 전투 Registry에서 정리한다.
            RemoveUnitIfDead(result);
            return result;
        }

        public InGameResourceChangeResult Heal(
            UnitCombatState target,
            float amount,
            UnitCombatState source = null,
            string sourceSkillName = null)
        {

            var result = HealResources(target, target.IsNexus ? 0f : amount);
            if (!result.Changed)
            {
                return result;
            }

            Units.Find(result.Target).RefreshDisplay();
            SkillTrigger.ExecuteHealOrShieldReceived(
                this,
                Units,
                target,
                source,
                sourceSkillName,
                null);
            return result;
        }

        /// 실제 수치 계산(DamageCalculator.cs 사용)

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
            var isCritical = false;

            if (baseDamage > 0f)
            {
                finalDamage = DamageCalculator.CalculateFinalDamage(target, baseDamage, attribute, attackRule, out isCritical);

                target.Statuses.RecordIncomingDamage(attribute, finalDamage);

                var statusShieldDamage = target.Statuses.ConsumeShield(finalDamage, depletedShields, absorbedShields);
                var damageAfterStatusShield = Mathf.Max(0f, finalDamage - statusShieldDamage);
                var directShieldBefore = Mathf.Max(0f, resources.DirectShield);
                var directShieldDamage = Mathf.Min(directShieldBefore, damageAfterStatusShield);
                var remainingDamage = Mathf.Max(0f, damageAfterStatusShield - directShieldDamage);

                resources.DirectShield = Mathf.Round(Mathf.Max(0f, directShieldBefore - directShieldDamage));
                resources.CurrentHealth = Mathf.Round(Mathf.Max(0f, beforeHealth - remainingDamage));
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
                currentHealth <= 0f,
                isCritical);
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
                resources.CurrentHealth = Mathf.Round(Mathf.Min(maxHealth, beforeHealth + amount));
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

        /// 상태 적용 규칙을 통과한 상태 데이터를 대상의 런타임 상태에 반영한다.
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
            BuffSkillExecutor.ShowStatusEffectVisual(this, target, status);
            return status;
        }

        /// 보호막 상태와 보호막 수치를 대상의 런타임 상태에 반영한다.
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

            var beforeShield = target.GetTotalShield();
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
            BuffSkillExecutor.ShowStatusEffectVisual(this, target, status);
            if (target.GetTotalShield() > beforeShield)
            {
                SkillTrigger.ExecuteHealOrShieldReceived(
                    this,
                    Units,
                    target,
                    source,
                    status.SourceSkillName,
                    status);
            }
            return status;
        }

        /// 상태 저장소에 지속시간 연장을 요청하고 전투 표현을 갱신한다.
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

                BuffSkillExecutor.ShowStatusEffectVisual(this, target, status);
            }

            return true;
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


        /// 상태이상 차감 후 상태 갱신

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
                    effectManager.RemoveEffect(status: removedStatuses[i]);
                }

                target.SyncShield();
                Units.RefreshDisplay(target);
            }

            return consumed;
        }


        /// 상태이상 만료 관리

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
                        effectManager.RemoveEffect(status: removedStatuses[j]);
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
            bool isDead,
            bool isCritical = false)
        {
            Target = target;
            PreviousHealth = previousHealth;
            CurrentHealth = currentHealth;
            PreviousShield = previousShield;
            CurrentShield = currentShield;
            AppliedDamage = appliedDamage;
            IsDead = isDead;
            IsCritical = isCritical;
        }

        public UnitCombatState Target { get; }
        public float PreviousHealth { get; }
        public float CurrentHealth { get; }
        public float PreviousShield { get; }
        public float CurrentShield { get; }
        public float AppliedDamage { get; }
        public bool IsDead { get; }
        public bool IsCritical { get; }
        public bool Changed =>
            !Mathf.Approximately(PreviousHealth, CurrentHealth)
            || !Mathf.Approximately(PreviousShield, CurrentShield);

    }
}
