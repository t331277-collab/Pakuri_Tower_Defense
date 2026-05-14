using System.Collections.Generic;
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
            LastAttackAttemptCount = 0;

            if (roster == null || deltaTime <= 0f)
            {
                return;
            }

            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                TickEnemy(enemies[i], roster, deltaTime, logAttackAttempts);
            }
        }

        private void TickEnemy(UnitRosterEntry enemyEntry, UnitRosterService roster, float deltaTime, bool logAttackAttempts)
        {
            if (!IsActive(enemyEntry))
            {
                return;
            }

            var enemyModel = enemyEntry.Model as EnemyUnitRuntimeModel;
            if (enemyModel == null || !enemyModel.AutoAttackEnabled)
            {
                return;
            }

            var target = FindNearestPlayerTarget(enemyEntry, roster);
            if (target == null)
            {
                return;
            }

            var state = GetState(enemyModel);
            state.TargetUnitId = target.Model != null && target.Model.Identity != null
                ? target.Model.Identity.UnitId
                : null;

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = Mathf.Max(0.1f, enemyModel.AttackAttemptRange);
            if (distance > attackRange)
            {
                MoveToward(enemyEntry, target, enemyModel, deltaTime);
                state.AttackCooldownRemaining = Mathf.Max(0f, state.AttackCooldownRemaining - deltaTime);
                return;
            }

            state.AttackCooldownRemaining = Mathf.Max(0f, state.AttackCooldownRemaining - deltaTime);
            if (state.AttackCooldownRemaining > 0f)
            {
                return;
            }

            state.AttackCooldownRemaining = Mathf.Max(0.1f, enemyModel.AttackAttemptCooldownSeconds);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target));
            }
        }

        private static UnitRosterEntry FindNearestPlayerTarget(UnitRosterEntry enemyEntry, UnitRosterService roster)
        {
            var players = roster.Players;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = enemyEntry.Transform.position;

            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                if (!IsActive(candidate))
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        private static void MoveToward(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            float deltaTime)
        {
            var moveSpeed = enemyModel.Stats != null ? Mathf.Max(0f, enemyModel.Stats.MoveSpeed) : 0f;
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

        private static bool IsActive(UnitRosterEntry entry)
        {
            return entry != null && entry.IsAlive && entry.Transform != null;
        }

        private static string BuildAttackAttemptLog(EnemyUnitRuntimeModel enemyModel, UnitRosterEntry target)
        {
            var enemyName = enemyModel.Identity != null && !string.IsNullOrWhiteSpace(enemyModel.Identity.DisplayName)
                ? enemyModel.Identity.DisplayName
                : enemyModel.Identity != null ? enemyModel.Identity.DefinitionId : "enemy";
            var targetName = target.Model != null
                && target.Model.Identity != null
                && !string.IsNullOrWhiteSpace(target.Model.Identity.DisplayName)
                    ? target.Model.Identity.DisplayName
                    : target.Model != null && target.Model.Identity != null ? target.Model.Identity.DefinitionId : "target";

            return $"Enemy basic attack attempt: {enemyName} -> {targetName}";
        }
    }

    public sealed class EnemyCombatState
    {
        public string TargetUnitId;
        public float AttackCooldownRemaining;
        public int AttackAttemptCount;
    }
}
