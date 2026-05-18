using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class EnemyCombatSimulationSystem
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

            EnemySkillCooldown.TickTemporaryEnemyModifiers(enemyModel, deltaTime);

            var state = GetState(enemyModel);
            EnemySkillCooldown.TickEnemyCooldowns(state, deltaTime);

            var target = EnemyTargeting.FindNearestPlayerTarget(enemyEntry, roster);
            if (target != null)
            {
                state.TargetUnitId = target.Model != null && target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : null;
            }

            var specialSkill = EnemySkillCooldown.ResolveSpecialSkill(enemyModel);
            var executedSupportSkill = TryExecuteCooldownDrivenSpecialSkill(
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

            var offensiveSkill = EnemySkillCooldown.ResolvePreferredOffensiveSkill(enemyModel, state, specialSkill);
            if (!offensiveSkill.IsAssigned)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = EnemySkillCooldown.ResolveAttackAttemptRange(enemyModel, offensiveSkill);
            if (distance > attackRange)
            {
                MoveToward(enemyEntry, target, enemyModel, deltaTime);
                return;
            }

            if (executedSupportSkill || !EnemySkillCooldown.IsSkillReady(state, offensiveSkill.SlotType))
            {
                return;
            }

            EnemySkillCooldown.SetSkillCooldown(state, offensiveSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            EnemySkillExecutor.Execute(enemyEntry, enemyModel, target, roster, combatManager, offensiveSkill);

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
                || !EnemySkillCooldown.IsCooldownDrivenSelfOrAllySkill(specialSkill.SkillKind)
                || !EnemySkillCooldown.IsSkillReady(state, EnemySkillSlotType.Special)
                || !EnemySkillCooldown.CanExecuteCooldownDrivenSelfOrAllySkill(specialSkill.SkillKind, roster))
            {
                return false;
            }

            EnemySkillCooldown.SetSkillCooldown(state, specialSkill);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            EnemySkillExecutor.Execute(enemyEntry, enemyModel, target, roster, combatManager, specialSkill);

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target, specialSkill.SkillKind));
            }

            return true;
        }

        private static void MoveToward(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            float deltaTime)
        {
            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
            moveSpeed *= EnemySkillCooldown.ResolveMoveSpeedMultiplier(enemyModel);
            if (moveSpeed <= 0f)
            {
                return;
            }

            var current = enemyEntry.Transform.position;
            var targetPosition = target.Transform.position;
            targetPosition.z = current.z;
            enemyEntry.Transform.position = Vector3.MoveTowards(current, targetPosition, moveSpeed * deltaTime);
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
    }
}
