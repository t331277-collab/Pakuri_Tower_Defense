using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class EnemyCombatSystem
    {
        private readonly Dictionary<string, EnemyCombatState> enemyStates = new Dictionary<string, EnemyCombatState>();

        public int LastAttackAttemptCount { get; private set; }

        public void Clear()
        {
            enemyStates.Clear();
            LastAttackAttemptCount = 0;
        }

        public void Tick(UnitRosterService roster, float deltaTime, bool logAttackAttempts)
        {
            Tick(roster, null, deltaTime, logAttackAttempts);
        }

        public void Tick(
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logAttackAttempts)
        {
            LastAttackAttemptCount = 0;

            if (roster == null || deltaTime <= 0f)
            {
                return;
            }

            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                TickEnemy(enemies[i], roster, combatManager, deltaTime, logAttackAttempts);
            }
        }

        private void TickEnemy(
            UnitRosterEntry enemyEntry,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logAttackAttempts)
        {
            if (!EnemyTargeting.IsActive(enemyEntry))
            {
                return;
            }

            var enemyModel = enemyEntry.Model as EnemyUnitRuntimeModel;
            if (enemyModel == null || !enemyModel.AutoAttackEnabled)
            {
                return;
            }

            EnemyCombatRules.TickTemporaryEnemyModifiers(enemyModel, deltaTime);

            var state = GetState(enemyModel);
            var actionDeltaTime = deltaTime * StatusEffectRuntime.ResolveActionSpeedMultiplier(enemyModel);
            EnemyCombatRules.TickEnemyCooldowns(state, actionDeltaTime);
            EnemySkillPlanRuntime.TickPendingActions(enemyEntry, enemyModel, roster, combatManager, state, actionDeltaTime);
            if (EnemySkillPlanRuntime.TickActiveCharge(enemyEntry, enemyModel, roster, combatManager, state, deltaTime))
            {
                return;
            }

            var target = EnemyTargeting.FindNearestPlayerTarget(enemyEntry, roster);
            if (target != null)
            {
                state.TargetUnitId = target.Model != null && target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : null;
            }

            if (EnemyTargeting.IsNexus(target))
            {
                TickNexusAssault(enemyEntry, enemyModel, target, combatManager, deltaTime);
                return;
            }

            var specialSkill = EnemyCombatRules.ResolveSpecialSkill(enemyModel);
            var canAct = StatusEffectRuntime.CanAct(enemyModel);
            var canUseSpecialSkill = canAct && StatusEffectRuntime.CanUseSpecialSkill(enemyModel);
            var executedStartSkill = canUseSpecialSkill && TryExecuteCombatStartSpecialSkill(
                enemyEntry,
                enemyModel,
                roster,
                combatManager,
                specialSkill,
                state,
                logAttackAttempts);
            var executedSupportSkill = !executedStartSkill && canUseSpecialSkill && TryExecuteCooldownDrivenSpecialSkill(
                enemyEntry,
                enemyModel,
                roster,
                combatManager,
                specialSkill,
                state,
                logAttackAttempts,
                target);

            if (target == null)
            {
                return;
            }

            var offensiveSkill = EnemyCombatRules.ResolvePreferredOffensiveSkill(enemyModel, state, specialSkill);
            if (offensiveSkill.SlotType == EnemySkillSlotType.Special && !canUseSpecialSkill)
            {
                offensiveSkill = EnemyCombatRules.ResolveBasicSkill(enemyModel);
            }

            if (!offensiveSkill.IsAssigned)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = EnemyCombatRules.ResolveAttackAttemptRange(enemyModel, offensiveSkill);
            if (distance > attackRange)
            {
                if (StatusEffectRuntime.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, target, enemyModel, deltaTime);
                }

                return;
            }

            if (!canAct || executedStartSkill || executedSupportSkill || !EnemyCombatRules.IsSkillReady(state, offensiveSkill.SlotType))
            {
                return;
            }

            EnemyCombatRules.SetSkillCooldown(state, offensiveSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            ExecuteSkillWithPlanFallback(enemyEntry, enemyModel, target, roster, combatManager, offensiveSkill, state);

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, offensiveSkill.SkillKind));
            }
        }

        private bool TryExecuteCooldownDrivenSpecialSkill(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData specialSkill,
            EnemyCombatState state,
            bool logAttackAttempts,
            UnitRosterEntry target)
        {
            if (!specialSkill.IsAssigned
                || !EnemyCombatRules.IsCooldownDrivenSelfOrAllySkill(specialSkill.SkillKind)
                || !EnemyCombatRules.IsSkillReady(state, EnemySkillSlotType.Special)
                || !EnemyCombatRules.CanExecuteCooldownDrivenSelfOrAllySkill(specialSkill.SkillKind, roster))
            {
                return false;
            }

            EnemyCombatRules.SetSkillCooldown(state, specialSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            ExecuteSkillWithPlanFallback(enemyEntry, enemyModel, target, roster, combatManager, specialSkill, state);

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, specialSkill.SkillKind));
            }

            return true;
        }

        private bool TryExecuteCombatStartSpecialSkill(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData specialSkill,
            EnemyCombatState state,
            bool logAttackAttempts)
        {
            if (state.CombatStartSpecialExecuted
                || !specialSkill.IsAssigned
                || specialSkill.Plan == null
                || !EnemySkillPlanRuntime.HasCombatStartTrigger(specialSkill.Plan)
                || !EnemyCombatRules.IsSkillReady(state, EnemySkillSlotType.Special))
            {
                return false;
            }

            state.CombatStartSpecialExecuted = true;
            EnemyCombatRules.SetSkillCooldown(state, specialSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            EnemySkillPlanRuntime.Execute(enemyEntry, enemyModel, null, roster, combatManager, specialSkill, state, "CombatStart");

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, null, specialSkill.SkillKind));
            }

            return true;
        }

        private static void ExecuteSkillWithPlanFallback(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemyCombatState state)
        {
            if (!EnemySkillPlanRuntime.Execute(enemyEntry, enemyModel, target, roster, combatManager, skillData, state, string.Empty))
            {
                EnemySkillExecutor.Execute(enemyEntry, enemyModel, target, roster, combatManager, skillData);
            }
        }

        internal static void MoveToward(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            float deltaTime)
        {
            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
            moveSpeed *= EnemyCombatRules.ResolveMoveSpeedMultiplier(enemyModel);
            moveSpeed *= StatusEffectRuntime.ResolveMoveSpeedMultiplier(enemyModel);
            if (moveSpeed <= 0f)
            {
                return;
            }

            var current = enemyEntry.Transform.position;
            var targetPosition = target.Transform.position;
            targetPosition.z = current.z;
            enemyEntry.Transform.position = Vector3.MoveTowards(current, targetPosition, moveSpeed * deltaTime);
        }

        private static void TickNexusAssault(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry nexusTarget,
            InGameCombatManager combatManager,
            float deltaTime)
        {
            if (enemyEntry == null || enemyModel == null || nexusTarget == null || combatManager == null)
            {
                return;
            }

            if (!IsTouchingNexus(enemyEntry, nexusTarget))
            {
                if (StatusEffectRuntime.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, nexusTarget, enemyModel, deltaTime);
                }

                return;
            }

            var damage = Mathf.Max(1f, enemyModel.NexusDamage);
            combatManager.ApplyDamage(nexusTarget.Model, damage, DamageAttribute.Physical, enemyModel, false);
            combatManager.DespawnUnit(enemyModel);
        }

        private static bool IsTouchingNexus(UnitRosterEntry enemyEntry, UnitRosterEntry nexusTarget)
        {
            if (enemyEntry == null || nexusTarget == null || enemyEntry.Transform == null || nexusTarget.Transform == null)
            {
                return false;
            }

            var enemyPoint = enemyEntry.ResolveTargetPoint();
            var targetColliders = nexusTarget.GetHitboxColliders();
            for (var i = 0; i < targetColliders.Length; i++)
            {
                var collider = targetColliders[i];
                if (collider != null && collider.enabled && collider.OverlapPoint(enemyPoint))
                {
                    return true;
                }
            }

            if (UnitHitboxUtility.IsTargetInsideHitbox(enemyEntry.GetHitboxColliders(), nexusTarget))
            {
                return true;
            }

            return Vector2.Distance(enemyEntry.Transform.position, nexusTarget.Transform.position) <= 0.25f;
        }

        private EnemyCombatState GetState(EnemyUnitRuntimeModel enemyModel)
        {
            var unitId = enemyModel.Identity != null ? enemyModel.Identity.UnitId : null;
            if (string.IsNullOrWhiteSpace(unitId))
            {
                unitId = "enemy-unknown";
            }

            if (!enemyStates.TryGetValue(unitId, out var state))
            {
                state = new EnemyCombatState();
                enemyStates.Add(unitId, state);
            }

            return state;
        }

        private static string BuildAttackAttemptLog(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            StageOneEnemySkillKind skillKind)
        {
            var enemyName = enemyModel.Identity != null && !string.IsNullOrWhiteSpace(enemyModel.Identity.DisplayName)
                ? enemyModel.Identity.DisplayName
                : enemyModel.Identity != null ? enemyModel.Identity.DefinitionId : "enemy";
            var targetName = target != null
                && target.Model != null
                && target.Model.Identity != null
                && !string.IsNullOrWhiteSpace(target.Model.Identity.DisplayName)
                    ? target.Model.Identity.DisplayName
                    : target != null && target.Model != null && target.Model.Identity != null ? target.Model.Identity.DefinitionId : "target";

            return $"Enemy skill attempt: {enemyName} -> {targetName} ({skillKind})";
        }
    }

    public sealed class EnemyCombatState
    {
        public string TargetUnitId;
        public float BasicSkillCooldownRemaining;
        public float SpecialSkillCooldownRemaining;
        public int AttackAttemptCount;
        public bool CombatStartSpecialExecuted;
        internal PendingEnemySkillAction PendingAction;
        internal ActiveEnemyChargeAction ActiveCharge;
    }

    internal sealed class PendingEnemySkillAction
    {
        public float RemainingSeconds;
        public string ActionOp;
        public string ExcludedTargetUnitId;
        public float DamageMultiplier = 1f;
        public DamageAttribute Attribute = DamageAttribute.Physical;
        public float SearchRadius;
        public EnemyResolvedSkillData SkillData;
    }

    internal sealed class ActiveEnemyChargeAction
    {
        public string TargetUnitId;
        public float ElapsedSeconds;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public float DamageTargetMaxHealthRatio = 1f;
        public float FreezeDurationSeconds = 5f;
        public DamageAttribute Attribute = DamageAttribute.Physical;
    }

    internal enum EnemySkillSlotType
    {
        None,
        Basic,
        Special
    }

    internal struct EnemyResolvedSkillData
    {
        public EnemySkillSlotType SlotType;
        public StageOneEnemySkillKind SkillKind;
        public float Coefficient;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float CooldownSeconds;
        public float Duration;
        public float Radius;
        public float FlatValue;
        public float ProjectileSpeed;
        public float ProjectileLifetime;
        public float MoveSpeedMultiplier;
        public float OutgoingDamageMultiplier;
        public EnemySkillPlanDefinition Plan;
        public bool IsAssigned;
    }

    internal static class EnemyCombatRules
    {
        public static EnemyResolvedSkillData ResolveSpecialSkill(EnemyUnitRuntimeModel enemyModel)
        {
            if (enemyModel == null)
            {
                return default;
            }

            return new EnemyResolvedSkillData
            {
                SlotType = EnemySkillSlotType.Special,
                SkillKind = enemyModel.StageOneSkill,
                Coefficient = enemyModel.ActiveSkillCoefficient,
                AttackPowerCoefficient = enemyModel.ActiveSkillAttackPowerCoefficient,
                SpellPowerCoefficient = enemyModel.ActiveSkillSpellPowerCoefficient,
                CooldownSeconds = enemyModel.ActiveSkillCooldownSeconds,
                Duration = enemyModel.ActiveSkillDuration,
                Radius = enemyModel.ActiveSkillRadius,
                FlatValue = enemyModel.ActiveSkillFlatValue,
                ProjectileSpeed = enemyModel.ActiveSkillProjectileSpeed,
                ProjectileLifetime = enemyModel.ActiveSkillProjectileLifetime,
                MoveSpeedMultiplier = enemyModel.ActiveSkillMoveSpeedMultiplier,
                OutgoingDamageMultiplier = enemyModel.ActiveSkillOutgoingDamageMultiplier,
                Plan = enemyModel.ActiveSkillPlan,
                IsAssigned = true
            };
        }

        public static EnemyResolvedSkillData ResolveBasicSkill(EnemyUnitRuntimeModel enemyModel)
        {
            if (enemyModel == null || !enemyModel.HasBasicSkill || enemyModel.BasicSkill == enemyModel.StageOneSkill)
            {
                return default;
            }

            return new EnemyResolvedSkillData
            {
                SlotType = EnemySkillSlotType.Basic,
                SkillKind = enemyModel.BasicSkill,
                Coefficient = enemyModel.BasicSkillCoefficient,
                AttackPowerCoefficient = enemyModel.BasicSkillAttackPowerCoefficient,
                SpellPowerCoefficient = enemyModel.BasicSkillSpellPowerCoefficient,
                CooldownSeconds = enemyModel.BasicSkillCooldownSeconds,
                Duration = enemyModel.BasicSkillDuration,
                Radius = enemyModel.BasicSkillRadius,
                FlatValue = enemyModel.BasicSkillFlatValue,
                ProjectileSpeed = enemyModel.BasicSkillProjectileSpeed,
                ProjectileLifetime = enemyModel.BasicSkillProjectileLifetime,
                MoveSpeedMultiplier = enemyModel.BasicSkillMoveSpeedMultiplier,
                OutgoingDamageMultiplier = enemyModel.BasicSkillOutgoingDamageMultiplier,
                Plan = enemyModel.BasicSkillPlan,
                IsAssigned = true
            };
        }

        public static EnemyResolvedSkillData ResolvePreferredOffensiveSkill(
            EnemyUnitRuntimeModel enemyModel,
            EnemyCombatState state,
            EnemyResolvedSkillData specialSkill)
        {
            var basicSkill = ResolveBasicSkill(enemyModel);
            var specialIsOffensive = IsOffensiveSkill(specialSkill);
            var basicIsOffensive = IsOffensiveSkill(basicSkill);

            if (specialIsOffensive && IsSkillReady(state, EnemySkillSlotType.Special))
            {
                return specialSkill;
            }

            if (basicIsOffensive && IsSkillReady(state, EnemySkillSlotType.Basic))
            {
                return basicSkill;
            }

            if (basicIsOffensive)
            {
                return basicSkill;
            }

            if (specialIsOffensive)
            {
                return specialSkill;
            }

            return default;
        }

        public static float ResolveAttackAttemptRange(EnemyUnitRuntimeModel enemyModel, EnemyResolvedSkillData skillData)
        {
            if (skillData.Radius > 0f)
            {
                return Mathf.Max(0.1f, skillData.Radius);
            }

            switch (enemyModel.AttackType)
            {
                case EnemyAttackType.Ranged:
                    return 5f;
                case EnemyAttackType.MeleeAndRanged:
                    return 4f;
                case EnemyAttackType.Buffer:
                    return 5f;
                default:
                    return 1.4f;
            }
        }

        public static void TickEnemyCooldowns(EnemyCombatState state, float deltaTime)
        {
            state.BasicSkillCooldownRemaining = Mathf.Max(0f, state.BasicSkillCooldownRemaining - deltaTime);
            state.SpecialSkillCooldownRemaining = Mathf.Max(0f, state.SpecialSkillCooldownRemaining - deltaTime);
        }

        public static bool IsSkillReady(EnemyCombatState state, EnemySkillSlotType slotType)
        {
            switch (slotType)
            {
                case EnemySkillSlotType.Basic:
                    return state.BasicSkillCooldownRemaining <= 0f;
                case EnemySkillSlotType.Special:
                    return state.SpecialSkillCooldownRemaining <= 0f;
                default:
                    return false;
            }
        }

        public static void SetSkillCooldown(EnemyCombatState state, EnemyResolvedSkillData skillData)
        {
            var cooldown = Mathf.Max(0.1f, skillData.CooldownSeconds);
            if (skillData.SlotType == EnemySkillSlotType.Basic)
            {
                state.BasicSkillCooldownRemaining = cooldown;
            }
            else if (skillData.SlotType == EnemySkillSlotType.Special)
            {
                state.SpecialSkillCooldownRemaining = cooldown;
            }
        }

        public static bool IsCooldownDrivenSelfOrAllySkill(StageOneEnemySkillKind skillKind)
        {
            switch (skillKind)
            {
                case StageOneEnemySkillKind.Heal:
                case StageOneEnemySkillKind.HolyDragonHeal:
                case StageOneEnemySkillKind.ShieldUp:
                case StageOneEnemySkillKind.GuardianFlag:
                case StageOneEnemySkillKind.ChargeCommand:
                    return true;
                default:
                    return false;
            }
        }

        public static bool CanExecuteCooldownDrivenSelfOrAllySkill(
            StageOneEnemySkillKind skillKind,
            UnitRosterService roster)
        {
            return (skillKind != StageOneEnemySkillKind.Heal && skillKind != StageOneEnemySkillKind.HolyDragonHeal)
                || EnemyTargeting.FindLowestHealthEnemyAlly(roster) != null;
        }

        public static void TickTemporaryEnemyModifiers(EnemyUnitRuntimeModel enemyModel, float deltaTime)
        {
            TickTemporaryMultiplier(ref enemyModel.IncomingDamageMultiplierRemainingSeconds, ref enemyModel.IncomingDamageMultiplier, deltaTime);
            TickTemporaryMultiplier(ref enemyModel.OutgoingDamageMultiplierRemainingSeconds, ref enemyModel.OutgoingDamageMultiplier, deltaTime);
            TickTemporaryMultiplier(ref enemyModel.MoveSpeedMultiplierRemainingSeconds, ref enemyModel.MoveSpeedMultiplier, deltaTime);
        }

        public static float ResolveMoveSpeedMultiplier(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.MoveSpeedMultiplierRemainingSeconds > 0f
                ? Mathf.Max(0f, enemyModel.MoveSpeedMultiplier)
                : 1f;
        }

        private static bool IsOffensiveSkill(EnemyResolvedSkillData skillData)
        {
            return skillData.IsAssigned && !IsCooldownDrivenSelfOrAllySkill(skillData.SkillKind);
        }

        private static void TickTemporaryMultiplier(ref float remainingSeconds, ref float multiplier, float deltaTime)
        {
            if (remainingSeconds <= 0f)
            {
                multiplier = 1f;
                return;
            }

            remainingSeconds = Mathf.Max(0f, remainingSeconds - deltaTime);
            if (remainingSeconds <= 0f)
            {
                multiplier = 1f;
            }
        }
    }

    internal static class EnemySkillPlanRuntime
    {
        private const string CombatStartTrigger = "CombatStart";
        private const int HitAllColliderTargets = int.MaxValue;
        private const bool StagePersistentStatus = true;

        public static bool HasCombatStartTrigger(EnemySkillPlanDefinition plan)
        {
            var nodes = plan != null ? plan.Nodes : null;
            if (nodes == null)
            {
                return false;
            }

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node != null
                    && node.Enabled
                    && string.Equals(node.Trigger, CombatStartTrigger, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static void TickPendingActions(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyCombatState state,
            float deltaTime)
        {
            var pending = state != null ? state.PendingAction : null;
            if (pending == null)
            {
                return;
            }

            pending.RemainingSeconds = Mathf.Max(0f, pending.RemainingSeconds - deltaTime);
            if (pending.RemainingSeconds > 0f)
            {
                return;
            }

            state.PendingAction = null;
            if (string.Equals(pending.ActionOp, "ChainDamage", System.StringComparison.OrdinalIgnoreCase))
            {
                ExecutePendingChainDamage(enemyEntry, enemyModel, roster, combatManager, pending);
            }
        }

        public static bool TickActiveCharge(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyCombatState state,
            float deltaTime)
        {
            var charge = state != null ? state.ActiveCharge : null;
            if (charge == null)
            {
                return false;
            }

            if (enemyEntry == null || enemyEntry.Transform == null || enemyModel == null || roster == null || combatManager == null)
            {
                ClearActiveCharge(enemyModel, state);
                return true;
            }

            var hitTarget = FindChargeHitTarget(enemyEntry, roster);
            if (hitTarget != null)
            {
                ResolveChargeHit(enemyModel, hitTarget, combatManager, state, charge);
                return true;
            }

            charge.ElapsedSeconds += Mathf.Max(0f, deltaTime);
            var rampProgress = charge.RampSeconds > 0f
                ? Mathf.Clamp01(charge.ElapsedSeconds / charge.RampSeconds)
                : 1f;
            enemyModel.MoveSpeedMultiplier = Mathf.Lerp(1f, Mathf.Max(1f, charge.MaxMoveSpeedMultiplier), rampProgress);
            enemyModel.MoveSpeedMultiplierRemainingSeconds = Mathf.Max(0.1f, deltaTime + 0.05f);

            var chargeTarget = FindPlayerTargetByUnitId(roster, charge.TargetUnitId) ?? EnemyTargeting.FindRandomPlayerTarget(roster);
            if (chargeTarget == null || chargeTarget.Transform == null)
            {
                ClearActiveCharge(enemyModel, state);
                return true;
            }

            if (StatusEffectRuntime.CanMove(enemyModel))
            {
                EnemyCombatSystem.MoveToward(enemyEntry, chargeTarget, enemyModel, deltaTime);
            }

            hitTarget = FindChargeHitTarget(enemyEntry, roster);
            if (hitTarget != null)
            {
                ResolveChargeHit(enemyModel, hitTarget, combatManager, state, charge);
            }

            return true;
        }

        public static bool Execute(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry currentTarget,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemyCombatState state,
            string triggerFilter)
        {
            var plan = skillData.Plan;
            var nodes = plan != null ? plan.Nodes : null;
            if (nodes == null || nodes.Length == 0 || enemyModel == null || combatManager == null || roster == null)
            {
                return false;
            }

            var executed = false;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null || !node.Enabled || !ShouldExecuteForTrigger(node, triggerFilter))
                {
                    continue;
                }

                executed |= ExecuteNode(enemyEntry, enemyModel, currentTarget, roster, combatManager, skillData, state, node);
            }

            return executed;
        }

        private static bool ShouldExecuteForTrigger(EnemySkillPlanNodeDefinition node, string triggerFilter)
        {
            var wantsCombatStart = string.Equals(node.Trigger, CombatStartTrigger, System.StringComparison.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(triggerFilter))
            {
                return !wantsCombatStart;
            }

            return string.Equals(node.Trigger, triggerFilter, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool ExecuteNode(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry currentTarget,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemyCombatState state,
            EnemySkillPlanNodeDefinition node)
        {
            switch ((node.ActionOp ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "damagearea":
                    ExecuteColliderDamageAreaOrFallback(enemyEntry, enemyModel, ResolveSingleTarget(node, enemyEntry, currentTarget, roster), combatManager, skillData, node);
                    return true;
                case "spawnprojectile":
                    EnemySkillExecutor.ExecuteEnemyProjectile(enemyEntry, enemyModel, ResolveSingleTarget(node, enemyEntry, currentTarget, roster), combatManager, skillData, GetFloat(node, "fallback_speed", 10f), GetFloat(node, "fallback_lifetime", 2.5f));
                    return true;
                case "heal":
                    EnemySkillExecutor.ExecuteHeal(enemyModel, roster, combatManager, skillData);
                    return true;
                case "applyselfincomingdamagemultiplier":
                    EnemySkillExecutor.ExecuteShieldUp(enemyEntry, enemyModel, combatManager, skillData);
                    return true;
                case "grantshieldtoenemyallies":
                    EnemySkillExecutor.ExecuteGuardianFlag(enemyEntry, enemyModel, roster, combatManager, skillData);
                    return true;
                case "applyallymoveanddamagemultiplier":
                    EnemySkillExecutor.ExecuteChargeCommand(enemyEntry, enemyModel, roster, combatManager, skillData);
                    return true;
                case "damage":
                    ExecuteColliderDamageOrFallback(enemyEntry, enemyModel, ResolveSingleTarget(node, enemyEntry, currentTarget, roster), combatManager, skillData, node);
                    return true;
                case "damageandactionspeeddebuff":
                    ExecuteDamageAndActionSpeedDebuff(enemyEntry, enemyModel, ResolveSingleTarget(node, enemyEntry, currentTarget, roster), combatManager, skillData, node);
                    return true;
                case "damagethendelayedchain":
                    ExecuteDamageThenDelayedChain(enemyEntry, enemyModel, currentTarget, roster, combatManager, skillData, state, node);
                    return true;
                case "chargedamagestatus":
                    ExecuteChargeDamageStatus(enemyEntry, enemyModel, roster, combatManager, skillData, state, node);
                    return true;
                case "applyoutgoingdamagemultiplierstatus":
                    ExecuteOutgoingDamageMultiplierStatus(enemyModel, roster, combatManager, skillData, node);
                    return true;
                default:
                    return false;
            }
        }

        private static void ExecuteColliderDamageOrFallback(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemySkillPlanNodeDefinition node)
        {
            if (!TrySpawnColliderDamageSkill(enemyEntry, enemyModel, target, combatManager, skillData, node, null))
            {
                ApplyDamageToSingleTarget(enemyModel, target, combatManager, skillData, ResolveAttribute(node, enemyModel.Attribute), 1f);
            }
        }

        private static void ExecuteColliderDamageAreaOrFallback(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemySkillPlanNodeDefinition node)
        {
            var maxHits = skillData.SkillKind == StageOneEnemySkillKind.FireDragonSlash ? HitAllColliderTargets : 1;
            if (!TrySpawnColliderDamageSkill(enemyEntry, enemyModel, target, combatManager, skillData, node, null, maxHits))
            {
                EnemySkillExecutor.ExecuteSlash(enemyEntry, enemyModel, target, combatManager, skillData);
            }
        }

        private static void ExecuteDamageAndActionSpeedDebuff(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemySkillPlanNodeDefinition node)
        {
            if (target == null || target.Model == null)
            {
                return;
            }

            var status = StatusEffectRuntime.CreateStatusData(StatusEffectKind.PassiveBuff, "Action Speed Down");
            if (status == null)
            {
                ApplyDamageToSingleTarget(enemyModel, target, combatManager, skillData, ResolveAttribute(node, DamageAttribute.Ice), 1f);
                return;
            }

            status.StatusTag = "enemy-action-speed-down";
            status.StatusName = "Action Speed Down";
            status.Modifiers.ActionSpeedBonus = GetFloat(node, "action_speed_bonus", -0.2f);
            if (!TrySpawnColliderDamageSkill(enemyEntry, enemyModel, target, combatManager, skillData, node, status))
            {
                ApplyDamageToSingleTarget(enemyModel, target, combatManager, skillData, ResolveAttribute(node, DamageAttribute.Ice), 1f);
                combatManager.ApplyStatus(target.Model, status, 1, GetFloat(node, "duration", 3f), 1, false, true, enemyModel);
            }
        }

        private static void ExecuteDamageThenDelayedChain(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry currentTarget,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemyCombatState state,
            EnemySkillPlanNodeDefinition node)
        {
            var target = ResolveSingleTarget(node, enemyEntry, currentTarget, roster);
            if (target == null || target.Model == null)
            {
                return;
            }

            ApplyDamageToSingleTarget(enemyModel, target, combatManager, skillData, ResolveAttribute(node, DamageAttribute.Lightning), 1f);
            EnemySkillExecutor.SpawnAttachedEnemySkillEffect(target, enemyModel, combatManager, skillData, GetFloat(node, "visual_duration", 0.8f));
            if (state == null)
            {
                return;
            }

            state.PendingAction = new PendingEnemySkillAction
            {
                RemainingSeconds = GetFloat(node, "delay", 0.5f),
                ActionOp = "ChainDamage",
                ExcludedTargetUnitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null,
                DamageMultiplier = GetFloat(node, "chain_multiplier", 0.5f),
                Attribute = ResolveAttribute(node, DamageAttribute.Lightning),
                SearchRadius = GetFloat(node, "chain_radius", skillData.Radius),
                SkillData = skillData
            };
        }

        private static void ExecutePendingChainDamage(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            PendingEnemySkillAction pending)
        {
            var target = FindNearestDifferentPlayerTarget(enemyEntry, roster, pending.ExcludedTargetUnitId, pending.SearchRadius);
            if (target == null)
            {
                return;
            }

            ApplyDamageToSingleTarget(enemyModel, target, combatManager, pending.SkillData, pending.Attribute, pending.DamageMultiplier);
            EnemySkillExecutor.SpawnAttachedEnemySkillEffect(target, enemyModel, combatManager, pending.SkillData, 0.8f);
        }

        private static void ExecuteChargeDamageStatus(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemyCombatState state,
            EnemySkillPlanNodeDefinition node)
        {
            var target = EnemyTargeting.FindRandomPlayerTarget(roster);
            if (target == null || target.Model == null)
            {
                return;
            }

            if (state == null)
            {
                return;
            }

            state.ActiveCharge = new ActiveEnemyChargeAction
            {
                TargetUnitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null,
                RampSeconds = GetFloat(node, "ramp_seconds", 3f),
                MaxMoveSpeedMultiplier = GetFloat(node, "move_speed_multiplier", 2.5f),
                DamageTargetMaxHealthRatio = GetFloat(node, "target_max_health_ratio", 1f),
                FreezeDurationSeconds = GetFloat(node, "status_duration", 5f),
                Attribute = ResolveAttribute(node, DamageAttribute.Physical)
            };
        }

        private static UnitRosterEntry FindChargeHitTarget(UnitRosterEntry enemyEntry, UnitRosterService roster)
        {
            var players = roster != null ? roster.Players : null;
            if (enemyEntry == null || players == null)
            {
                return null;
            }

            var enemyColliders = enemyEntry.GetHitboxColliders();
            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                if (!EnemyTargeting.IsActive(candidate) || EnemyTargeting.IsNexus(candidate))
                {
                    continue;
                }

                if (UnitHitboxUtility.IsTargetInsideHitbox(enemyColliders, candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static UnitRosterEntry FindPlayerTargetByUnitId(UnitRosterService roster, string unitId)
        {
            var players = roster != null ? roster.Players : null;
            if (players == null || string.IsNullOrWhiteSpace(unitId))
            {
                return null;
            }

            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                var identity = candidate != null && candidate.Model != null ? candidate.Model.Identity : null;
                if (EnemyTargeting.IsActive(candidate)
                    && !EnemyTargeting.IsNexus(candidate)
                    && identity != null
                    && string.Equals(identity.UnitId, unitId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void ResolveChargeHit(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyCombatState state,
            ActiveEnemyChargeAction charge)
        {
            if (target == null || target.Model == null || combatManager == null)
            {
                ClearActiveCharge(enemyModel, state);
                return;
            }

            var maxHealth = target.Model.Stats != null ? Mathf.Max(0f, target.Model.Stats.MaxHealth) : 0f;
            var damage = maxHealth * Mathf.Max(0f, charge.DamageTargetMaxHealthRatio);
            combatManager.ApplyDamage(target.Model, damage, charge.Attribute, enemyModel, true);
            combatManager.ApplyStatus(target.Model, StatusEffectKind.Freeze, 1, Mathf.Max(0f, charge.FreezeDurationSeconds), 1, false, true);
            ClearActiveCharge(enemyModel, state);
        }

        private static void ClearActiveCharge(EnemyUnitRuntimeModel enemyModel, EnemyCombatState state)
        {
            if (state != null)
            {
                state.ActiveCharge = null;
            }

            if (enemyModel == null)
            {
                return;
            }

            enemyModel.MoveSpeedMultiplier = 1f;
            enemyModel.MoveSpeedMultiplierRemainingSeconds = 0f;
        }

        private static void ExecuteOutgoingDamageMultiplierStatus(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemySkillPlanNodeDefinition node)
        {
            var targets = EnemyTargeting.FindAllPlayerTargets(roster);
            var multiplier = GetFloat(node, "multiplier", 0.7f);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || target.Model == null)
                {
                    continue;
                }

                var status = StatusEffectRuntime.CreateStatusData(StatusEffectKind.PassiveBuff, "Intimidated");
                if (status == null)
                {
                    continue;
                }

                status.StatusTag = "enemy-intimidation";
                status.StatusName = "Intimidated";
                status.Modifiers.DamageBonusRate = multiplier - 1f;
                combatManager.ApplyStatus(target.Model, status, 1, 0f, 1, StagePersistentStatus, true);
                EnemySkillExecutor.SpawnAttachedEnemySkillEffect(target, enemyModel, combatManager, skillData, GetFloat(node, "visual_duration", 0.8f));
            }
        }

        private static bool TrySpawnColliderDamageSkill(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            EnemySkillPlanNodeDefinition node,
            StatusEffectData statusOnHit,
            int maxHits = 1)
        {
            if (enemyEntry == null || enemyEntry.Transform == null || target == null || target.Transform == null || combatManager == null)
            {
                return false;
            }

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                return false;
            }

            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.left;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, target.Transform.position, EnemySkillExecutor.ResolveRotation(direction));
            if (instance == null)
            {
                return false;
            }

            var colliders = instance.GetComponentsInChildren<Collider2D>();
            var hasCollider = false;
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null && colliders[i].enabled)
                {
                    hasCollider = true;
                    break;
                }
            }

            if (!hasCollider)
            {
                UnityEngine.Object.Destroy(instance);
                return false;
            }

            var actor = instance.GetComponent<InGameEnemySkillHitboxActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameEnemySkillHitboxActor>();
            }

            actor.Initialize(
                combatManager,
                enemyModel,
                EnemySkillExecutor.ResolveAttackDamage(enemyModel, skillData),
                ResolveAttribute(node, enemyModel.Attribute),
                skillData.Radius,
                SkillVisualSpawnUtility.ResolveVisualLifetime(instance, GetFloat(node, "hitbox_lifetime", 0.35f)),
                maxHits);
            if (statusOnHit != null)
            {
                actor.ConfigureStatusOnHit(statusOnHit, 1, GetFloat(node, "duration", 3f), 1, false, true);
            }

            return true;
        }

        private static UnitRosterEntry ResolveSingleTarget(
            EnemySkillPlanNodeDefinition node,
            UnitRosterEntry enemyEntry,
            UnitRosterEntry currentTarget,
            UnitRosterService roster)
        {
            switch ((node.TargetSelector ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "farthesttower":
                    return EnemyTargeting.FindFarthestPlayerTarget(enemyEntry, roster);
                case "randomtower":
                    return EnemyTargeting.FindRandomPlayerTarget(roster);
                case "lowesthealthenemyally":
                    return EnemyTargeting.FindLowestHealthEnemyAlly(roster);
                case "nearesttower":
                case "currenttarget":
                default:
                    return currentTarget ?? EnemyTargeting.FindNearestPlayerTarget(enemyEntry, roster);
            }
        }

        private static UnitRosterEntry FindNearestDifferentPlayerTarget(
            UnitRosterEntry enemyEntry,
            UnitRosterService roster,
            string excludedTargetUnitId,
            float searchRadius)
        {
            var players = roster != null ? roster.Players : null;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = enemyEntry != null && enemyEntry.Transform != null ? enemyEntry.Transform.position : Vector3.zero;
            var radiusSq = searchRadius > 0f ? searchRadius * searchRadius : float.MaxValue;
            if (players == null)
            {
                return null;
            }

            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                var identity = candidate != null && candidate.Model != null ? candidate.Model.Identity : null;
                if (!EnemyTargeting.IsActive(candidate)
                    || EnemyTargeting.IsNexus(candidate)
                    || identity == null
                    || string.Equals(identity.UnitId, excludedTargetUnitId, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq > radiusSq || distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        private static void ApplyDamageToSingleTarget(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            DamageAttribute attribute,
            float multiplier)
        {
            if (target == null || target.Model == null)
            {
                return;
            }

            var damage = EnemySkillExecutor.ResolveAttackDamage(enemyModel, skillData) * Mathf.Max(0f, multiplier);
            combatManager.ApplyDamage(target.Model, damage, attribute, enemyModel, true);
        }

        private static DamageAttribute ResolveAttribute(EnemySkillPlanNodeDefinition node, DamageAttribute fallback)
        {
            var value = GetString(node, "attribute", string.Empty);
            return System.Enum.TryParse(value, true, out DamageAttribute parsed) ? parsed : fallback;
        }

        private static string GetString(EnemySkillPlanNodeDefinition node, string key, string defaultValue)
        {
            var parameters = node != null ? node.Params : null;
            if (parameters == null)
            {
                return defaultValue;
            }

            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter != null && string.Equals(parameter.ParamKey, key, System.StringComparison.OrdinalIgnoreCase))
                {
                    return parameter.ParamValue;
                }
            }

            return defaultValue;
        }

        private static float GetFloat(EnemySkillPlanNodeDefinition node, string key, float defaultValue)
        {
            var value = GetString(node, key, string.Empty);
            return float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : defaultValue;
        }
    }

    internal static class EnemySkillExecutor
    {
        public static void Execute(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            if (combatManager == null || enemyEntry == null || enemyModel == null || roster == null || !skillData.IsAssigned)
            {
                return;
            }

            switch (skillData.SkillKind)
            {
                case StageOneEnemySkillKind.Slash:
                    ExecuteSlash(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.ShurikenThrow:
                    ExecuteShuriken(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.Heal:
                    ExecuteHeal(enemyModel, roster, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.ShieldUp:
                    ExecuteShieldUp(enemyEntry, enemyModel, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.AimedShot:
                    ExecuteAimedShot(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.GuardianFlag:
                    ExecuteGuardianFlag(enemyEntry, enemyModel, roster, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.ChargeCommand:
                    ExecuteChargeCommand(enemyEntry, enemyModel, roster, combatManager, skillData);
                    break;
                case StageOneEnemySkillKind.SacredSwordWave:
                    ExecuteSacredSwordWave(enemyEntry, enemyModel, target, combatManager, skillData);
                    break;
            }
        }

        internal static void ExecuteSlash(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel, skillData);
            var radius = Mathf.Max(0.1f, skillData.Radius);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.left;
            }

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var origin = enemyEntry.Transform.position + direction.normalized * Mathf.Min(radius * 0.5f, 0.75f);
            var instance = effects.InstantiateSkillPrefab(prefab, origin, ResolveRotation(direction));
            if (instance == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var actor = instance.GetComponent<InGameEnemySkillHitboxActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameEnemySkillHitboxActor>();
            }

            actor.Initialize(combatManager, enemyModel, damage, enemyModel.Attribute, radius, SkillVisualSpawnUtility.ResolveVisualLifetime(instance, 0.35f));
        }

        private static void ExecuteShuriken(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, skillData, 9f, 2.5f);
        }

        private static void ExecuteAimedShot(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, skillData, 10f, 2.5f);
        }

        internal static void ExecuteEnemyProjectile(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            float projectileSpeed,
            float lifetimeSeconds)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel, skillData);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, enemyEntry.Transform.position, ResolveRotation(direction));
            if (instance == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute, enemyModel, true);
                return;
            }

            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            var resolvedSpeed = ResolveEnemyProjectileSpeed(enemyModel, skillData, projectileSpeed);
            var resolvedLifetime = ResolveEnemyProjectileLifetime(skillData, lifetimeSeconds);

            actor.Initialize(
                combatManager,
                enemyModel,
                direction,
                resolvedSpeed,
                damage,
                enemyModel.Attribute,
                0,
                ResolveEnemyProjectileBoundaryX(enemyEntry.Transform.position, direction, resolvedSpeed, resolvedLifetime),
                resolvedLifetime,
                null,
                null,
                null,
                null,
                null,
                true,
                false,
                0f,
                null,
                null,
                false,
                0f,
                0f,
                null,
                null,
                null,
                null,
                false,
                true);
        }

        internal static void ExecuteHeal(
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            var target = EnemyTargeting.FindLowestHealthEnemyAlly(roster);
            if (target == null)
            {
                return;
            }

            var healAmount = Mathf.Max(
                0f,
                (skillData.FlatValue + ResolveSpellPower(enemyModel) * Mathf.Max(0f, skillData.Coefficient))
                * Mathf.Max(0f, enemyModel.PassiveHealingMultiplier));
            combatManager.Heal(target.Model, healAmount);

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab != null && target.Transform != null && effects != null)
            {
                var instance = effects.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
                if (instance != null)
                {
                    var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                    if (actor == null)
                    {
                        actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                    }

                    actor.Initialize(target.Transform, 0.8f, Vector3.zero);
                }
            }
        }

        internal static void ExecuteShieldUp(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            enemyModel.IncomingDamageMultiplier = Mathf.Clamp01(1f - Mathf.Max(0f, skillData.FlatValue));
            enemyModel.IncomingDamageMultiplierRemainingSeconds = Mathf.Max(0.1f, skillData.Duration);
            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, skillData.Duration);
        }

        internal static void ExecuteGuardianFlag(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            var allies = EnemyTargeting.FindEnemyAlliesInRadius(enemyEntry, roster, Mathf.Max(0f, skillData.Radius));
            var shield = Mathf.Max(0f, skillData.FlatValue);
            for (var i = 0; i < allies.Count; i++)
            {
                combatManager.GrantShield(allies[i].Model, shield);
            }

            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, skillData.Duration);
        }

        internal static void ExecuteChargeCommand(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            var allies = EnemyTargeting.FindEnemyAlliesInRadius(enemyEntry, roster, Mathf.Max(0f, skillData.Radius));
            var duration = Mathf.Max(0.1f, skillData.Duration);
            for (var i = 0; i < allies.Count; i++)
            {
                var ally = allies[i].Model as EnemyUnitRuntimeModel;
                if (ally == null)
                {
                    continue;
                }

                ally.MoveSpeedMultiplier = Mathf.Max(0f, skillData.MoveSpeedMultiplier);
                ally.MoveSpeedMultiplierRemainingSeconds = duration;
                ally.OutgoingDamageMultiplier = Mathf.Max(0f, skillData.OutgoingDamageMultiplier);
                ally.OutgoingDamageMultiplierRemainingSeconds = duration;
            }

            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, duration);
        }

        private static void ExecuteSacredSwordWave(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, skillData, 12f, 4f);
        }

        internal static float ResolveAttackDamage(EnemyUnitRuntimeModel enemyModel, EnemyResolvedSkillData skillData)
        {
            var attribute = enemyModel != null ? enemyModel.Attribute : DamageAttribute.Physical;
            var attackCoefficient = Mathf.Max(0f, skillData.AttackPowerCoefficient);
            var spellCoefficient = Mathf.Max(0f, skillData.SpellPowerCoefficient);
            if (attackCoefficient <= 0f && spellCoefficient <= 0f && skillData.Coefficient > 0f)
            {
                attackCoefficient = skillData.Coefficient;
            }

            var damage = ResolveAttackPower(enemyModel) * Mathf.Max(0f, attackCoefficient);
            damage += ResolveSpellPower(enemyModel) * spellCoefficient;
            return Mathf.Max(0f, damage * ResolveOutgoingDamageMultiplier(enemyModel, attribute));
        }

        internal static float ResolveAttackPower(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.Stats != null ? enemyModel.Stats.AttackPower : 0f;
        }

        internal static float ResolveSpellPower(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.Stats != null ? enemyModel.Stats.SpellPower : 0f;
        }

        internal static float ResolveOutgoingDamageMultiplier(EnemyUnitRuntimeModel enemyModel, DamageAttribute attribute)
        {
            if (enemyModel == null)
            {
                return 1f;
            }

            var multiplier = Mathf.Max(0f, enemyModel.PassiveOutgoingDamageMultiplier);
            multiplier *= ResolvePassiveAttributeDamageMultiplier(enemyModel, attribute);

            if (enemyModel.OutgoingDamageMultiplierRemainingSeconds > 0f)
            {
                multiplier *= Mathf.Max(0f, enemyModel.OutgoingDamageMultiplier);
            }

            return multiplier;
        }

        private static float ResolvePassiveAttributeDamageMultiplier(EnemyUnitRuntimeModel enemyModel, DamageAttribute attribute)
        {
            if (enemyModel == null)
            {
                return 1f;
            }

            switch (attribute)
            {
                case DamageAttribute.Fire:
                    return Mathf.Max(0f, enemyModel.PassiveFireDamageMultiplier);
                case DamageAttribute.Lightning:
                    return Mathf.Max(0f, enemyModel.PassiveLightningDamageMultiplier);
                case DamageAttribute.Ice:
                    return Mathf.Max(0f, enemyModel.PassiveIceDamageMultiplier);
                case DamageAttribute.Darkness:
                    return Mathf.Max(0f, enemyModel.PassiveDarknessDamageMultiplier);
                case DamageAttribute.Holy:
                    return Mathf.Max(0f, enemyModel.PassiveHolyDamageMultiplier);
                default:
                    return Mathf.Max(0f, enemyModel.PassivePhysicalDamageMultiplier);
            }
        }

        internal static float ResolveEnemyProjectileSpeed(EnemyResolvedSkillData skillData, float fallbackSpeed)
        {
            return skillData.ProjectileSpeed > 0f
                ? skillData.ProjectileSpeed
                : fallbackSpeed;
        }

        internal static float ResolveEnemyProjectileSpeed(EnemyUnitRuntimeModel enemyModel, EnemyResolvedSkillData skillData, float fallbackSpeed)
        {
            if (skillData.SkillKind == StageOneEnemySkillKind.HolySpearThrow)
            {
                return 12f;
            }

            return ResolveEnemyProjectileSpeed(skillData, fallbackSpeed);
        }

        internal static float ResolveCurrentMoveSpeed(EnemyUnitRuntimeModel enemyModel)
        {
            if (enemyModel == null)
            {
                return 0f;
            }

            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
            moveSpeed *= EnemyCombatRules.ResolveMoveSpeedMultiplier(enemyModel);
            moveSpeed *= StatusEffectRuntime.ResolveMoveSpeedMultiplier(enemyModel);
            return moveSpeed;
        }

        internal static float ResolveEnemyProjectileLifetime(EnemyResolvedSkillData skillData, float fallbackLifetime)
        {
            if (IsEnemyProjectileSkillWithContactLifetime(skillData.SkillKind))
            {
                return 10f;
            }

            return skillData.ProjectileLifetime > 0f
                ? skillData.ProjectileLifetime
                : fallbackLifetime;
        }

        private static bool IsEnemyProjectileSkillWithContactLifetime(StageOneEnemySkillKind skillKind)
        {
            switch (skillKind)
            {
                case StageOneEnemySkillKind.AimedShot:
                case StageOneEnemySkillKind.ShurikenThrow:
                case StageOneEnemySkillKind.SacredSwordWave:
                case StageOneEnemySkillKind.HolySpearThrow:
                    return true;
                default:
                    return false;
            }
        }

        internal static void SpawnAttachedEnemySkillEffect(
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData,
            float duration)
        {
            if (target == null || target.Transform == null || combatManager == null)
            {
                return;
            }

            var effects = combatManager.Effects;
            var prefab = effects != null ? effects.ResolveEnemySkillEffectPrefab(enemyModel, skillData.SkillKind) : null;
            if (prefab == null)
            {
                return;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
            }

            actor.Initialize(target.Transform, Mathf.Max(0.1f, duration), Vector3.zero);
        }

        internal static Quaternion ResolveRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        private static float ResolveEnemyProjectileBoundaryX(Vector3 origin, Vector3 direction, float speed, float lifetimeSeconds)
        {
            var normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.left;
            var maxTravelDistance = Mathf.Max(40f, Mathf.Max(0f, speed) * Mathf.Max(0.1f, lifetimeSeconds) + 1f);
            return origin.x + normalized.x * maxTravelDistance;
        }
    }
}
