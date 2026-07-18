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

            var state = GetState(enemyModel);
            if (SharedChargeSkillRuntime.Tick(enemyEntry, roster, combatManager, deltaTime))
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

            var canAct = StatusEffectRuntime.CanAct(enemyModel);
            var canUseSpecialSkill = canAct && StatusEffectRuntime.CanUseSpecialSkill(enemyModel);
            var specialRuntime = ResolveSelectableRuntime(enemyModel, InGameSkillSlot.B);
            var executedSupportSkill = canUseSpecialSkill
                && IsSupportSkill(specialRuntime)
                && CanExecuteSupportSkill(specialRuntime, roster)
                && TryExecuteSharedSkill(
                    enemyEntry,
                    enemyModel,
                    target,
                    combatManager,
                    specialRuntime,
                    state,
                    deltaTime,
                    logAttackAttempts);

            if (target == null)
            {
                return;
            }

            var offensiveRuntime = ResolvePreferredOffensiveRuntime(
                enemyEntry,
                enemyModel,
                combatManager,
                canUseSpecialSkill);
            if (offensiveRuntime == null)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = ResolveAttackAttemptRange(enemyModel, offensiveRuntime);
            if (distance > attackRange)
            {
                if (StatusEffectRuntime.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, target, enemyModel, deltaTime);
                }

                return;
            }

            if (!canAct || executedSupportSkill)
            {
                return;
            }

            TryExecuteSharedSkill(
                enemyEntry,
                enemyModel,
                target,
                combatManager,
                offensiveRuntime,
                state,
                deltaTime,
                logAttackAttempts);
        }

        private SkillRuntimeInstance ResolvePreferredOffensiveRuntime(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            bool canUseSpecialSkill)
        {
            var special = ResolveSelectableRuntime(enemyModel, InGameSkillSlot.B);
            if (canUseSpecialSkill
                && !IsSupportSkill(special)
                && combatManager != null
                && combatManager.CanExecuteSelectedSkill(enemyEntry, special))
            {
                return special;
            }

            var basic = ResolveSelectableRuntime(enemyModel, InGameSkillSlot.A);
            if (!IsSupportSkill(basic)
                && combatManager != null
                && combatManager.CanExecuteSelectedSkill(enemyEntry, basic))
            {
                return basic;
            }

            return null;
        }

        private bool TryExecuteSharedSkill(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            SkillRuntimeInstance runtime,
            EnemyCombatState state,
            float deltaTime,
            bool logAttackAttempts)
        {
            if (runtime == null
                || combatManager == null
                || !combatManager.CanExecuteSelectedSkill(enemyEntry, runtime)
                || !combatManager.TryExecuteSelectedSkill(enemyEntry, runtime, deltaTime))
            {
                return false;
            }

            state.AttackAttemptCount++;
            LastAttackAttemptCount++;

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, runtime.SkillId));
            }

            return true;
        }

        private static SkillRuntimeInstance ResolveSelectableRuntime(
            EnemyUnitRuntimeModel enemyModel,
            InGameSkillSlot slot)
        {
            var runtime = enemyModel != null && enemyModel.SkillRuntime != null
                ? enemyModel.SkillRuntime.FindBySlot(slot)
                : null;
            return HasCombatStartTrigger(runtime) ? null : runtime;
        }

        private static bool HasCombatStartTrigger(SkillRuntimeInstance runtime)
        {
            var triggers = runtime != null && runtime.Data != null ? runtime.Data.SkillTriggers : null;
            for (var i = 0; triggers != null && i < triggers.Length; i++)
            {
                if (triggers[i] != null && triggers[i].TriggerEvent == SkillTriggerEvent.CombatStart)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSupportSkill(SkillRuntimeInstance runtime)
        {
            var targeting = runtime != null && runtime.Data != null ? runtime.Data.Targeting : null;
            return targeting != null && targeting.TargetSide != SkillTargetSide.Enemy;
        }

        private static bool CanExecuteSupportSkill(SkillRuntimeInstance runtime, UnitRosterService roster)
        {
            return !(runtime != null && runtime.Data is HealSkillData)
                || EnemyTargeting.FindLowestHealthEnemyAlly(roster) != null;
        }

        private static float ResolveAttackAttemptRange(
            EnemyUnitRuntimeModel enemyModel,
            SkillRuntimeInstance runtime)
        {
            var targeting = runtime != null && runtime.Data != null ? runtime.Data.Targeting : null;
            if (targeting != null && targeting.Range > 0f)
            {
                return Mathf.Max(0.1f, targeting.Range);
            }

            switch (enemyModel != null ? enemyModel.AttackType : EnemyAttackType.Melee)
            {
                case EnemyAttackType.Ranged:
                case EnemyAttackType.Buffer:
                    return 5f;
                case EnemyAttackType.MeleeAndRanged:
                    return 4f;
                default:
                    return 1.4f;
            }
        }

        internal static void MoveToward(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            float deltaTime)
        {
            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
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
            string skillId)
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

            return $"Enemy skill attempt: {enemyName} -> {targetName} ({skillId})";
        }
    }

    internal sealed class EnemyCombatState
    {
        public string TargetUnitId;
        public int AttackAttemptCount;
    }
}
