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
            if (!IsActive(enemyEntry))
            {
                return;
            }

            var enemyModel = enemyEntry.Model as EnemyUnitRuntimeModel;
            if (enemyModel == null || !enemyModel.AutoAttackEnabled)
            {
                return;
            }

            TickTemporaryEnemyModifiers(enemyModel, deltaTime);

            var state = GetState(enemyModel);
            var target = FindNearestPlayerTarget(enemyEntry, roster);
            var cooldownAlreadyTicked = false;
            if (target != null)
            {
                state.TargetUnitId = target.Model != null && target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : null;
            }

            if (IsCooldownDrivenSelfOrAllySkill(enemyModel.StageOneSkill))
            {
                state.AttackCooldownRemaining = Mathf.Max(0f, state.AttackCooldownRemaining - deltaTime);
                cooldownAlreadyTicked = true;
                if (state.AttackCooldownRemaining <= 0f
                    && CanExecuteCooldownDrivenSelfOrAllySkill(enemyModel.StageOneSkill, roster))
                {
                    state.AttackCooldownRemaining = Mathf.Max(0.1f, enemyModel.AttackAttemptCooldownSeconds);
                    state.AttackAttemptCount++;
                    LastAttackAttemptCount++;
                    ExecuteEnemySkill(enemyEntry, enemyModel, target, roster, combatManager);
                }

                if (logAttackAttempts && target != null && state.AttackCooldownRemaining > 0f)
                {
                    Debug.Log(BuildAttackAttemptLog(enemyModel, target));
                }
            }

            if (target == null)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = Mathf.Max(0.1f, enemyModel.AttackAttemptRange);
            if (distance > attackRange)
            {
                MoveToward(enemyEntry, target, enemyModel, deltaTime);
                if (!cooldownAlreadyTicked)
                {
                    state.AttackCooldownRemaining = Mathf.Max(0f, state.AttackCooldownRemaining - deltaTime);
                }

                return;
            }

            if (!cooldownAlreadyTicked)
            {
                state.AttackCooldownRemaining = Mathf.Max(0f, state.AttackCooldownRemaining - deltaTime);
            }

            if (state.AttackCooldownRemaining > 0f)
            {
                return;
            }

            state.AttackCooldownRemaining = Mathf.Max(0.1f, enemyModel.AttackAttemptCooldownSeconds);
            state.AttackAttemptCount++;
            LastAttackAttemptCount++;
            ExecuteEnemySkill(enemyEntry, enemyModel, target, roster, combatManager);

            if (logAttackAttempts)
            {
                Debug.Log(BuildAttackAttemptLog(enemyModel, target));
            }
        }

        private static void ExecuteEnemySkill(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            UnitRosterService roster,
            InGameCombatManager combatManager)
        {
            if (combatManager == null || enemyEntry == null || enemyModel == null || roster == null)
            {
                return;
            }

            switch (enemyModel.StageOneSkill)
            {
                case StageOneEnemySkillKind.Slash:
                    ExecuteSlash(enemyEntry, enemyModel, target, combatManager);
                    break;
                case StageOneEnemySkillKind.ShurikenThrow:
                    ExecuteShuriken(enemyEntry, enemyModel, target, combatManager);
                    break;
                case StageOneEnemySkillKind.Heal:
                    ExecuteHeal(enemyEntry, enemyModel, roster, combatManager);
                    break;
                case StageOneEnemySkillKind.ShieldUp:
                    ExecuteShieldUp(enemyEntry, enemyModel, combatManager);
                    break;
                case StageOneEnemySkillKind.AimedShot:
                    ExecuteAimedShot(enemyEntry, enemyModel, target, combatManager);
                    break;
                case StageOneEnemySkillKind.GuardianFlag:
                    ExecuteGuardianFlag(enemyEntry, enemyModel, roster, combatManager);
                    break;
                case StageOneEnemySkillKind.ChargeCommand:
                    ExecuteChargeCommand(enemyEntry, enemyModel, roster, combatManager);
                    break;
                case StageOneEnemySkillKind.SacredSwordWave:
                    ExecuteSacredSwordWave(enemyEntry, enemyModel, target, combatManager);
                    break;
            }
        }

        private static void ExecuteSlash(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel);
            var radius = Mathf.Max(0.1f, enemyModel.ActiveSkillRadius);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.left;
            }

            var prefab = combatManager.ResolveEnemySkillPrefab(enemyModel);
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

            var origin = enemyEntry.Transform.position + direction.normalized * Mathf.Min(radius * 0.5f, 0.75f);
            var instance = combatManager.InstantiateSkillPrefab(prefab, origin, ResolveRotation(direction));
            var actor = instance.GetComponent<InGameEnemySkillHitboxActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameEnemySkillHitboxActor>();
            }

            actor.Initialize(combatManager, enemyModel, damage, enemyModel.Attribute, radius, 0.35f);
        }

        private static void ExecuteShuriken(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            var prefab = combatManager.ResolveEnemySkillPrefab(enemyModel);
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

            var instance = combatManager.InstantiateSkillPrefab(prefab, enemyEntry.Transform.position, ResolveRotation(direction));
            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            actor.Initialize(
                combatManager,
                enemyModel,
                direction,
                9f,
                damage,
                enemyModel.Attribute,
                0,
                ResolveEnemyProjectileBoundaryX(enemyEntry.Transform.position, direction),
                2.5f);
        }

        private static void ExecuteAimedShot(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, 10f, 2.5f);
        }

        private static void ExecuteEnemyProjectile(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            float projectileSpeed,
            float lifetimeSeconds)
        {
            if (target == null || target.Transform == null || enemyEntry.Transform == null)
            {
                return;
            }

            var damage = ResolveAttackDamage(enemyModel);
            var direction = target.Transform.position - enemyEntry.Transform.position;
            direction.z = 0f;
            var prefab = combatManager.ResolveEnemySkillPrefab(enemyModel);
            if (prefab == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

            var instance = combatManager.InstantiateSkillPrefab(prefab, enemyEntry.Transform.position, ResolveRotation(direction));
            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            actor.Initialize(
                combatManager,
                enemyModel,
                direction,
                projectileSpeed,
                damage,
                enemyModel.Attribute,
                0,
                ResolveEnemyProjectileBoundaryX(enemyEntry.Transform.position, direction),
                lifetimeSeconds);
        }

        private static void ExecuteHeal(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager)
        {
            var target = FindLowestHealthEnemyAlly(roster);
            if (target == null)
            {
                return;
            }

            var healAmount = Mathf.Max(
                0f,
                (enemyModel.ActiveSkillFlatValue + ResolveSpellPower(enemyModel) * Mathf.Max(0f, enemyModel.ActiveSkillCoefficient))
                * Mathf.Max(0f, enemyModel.PassiveHealingMultiplier));
            combatManager.Heal(target.Model, healAmount);

            var prefab = combatManager.ResolveEnemySkillPrefab(enemyModel);
            if (prefab != null && target.Transform != null)
            {
                var instance = combatManager.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
                var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                }

                actor.Initialize(target.Transform, 0.8f, Vector3.zero);
            }
        }

        private static void ExecuteShieldUp(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager)
        {
            enemyModel.IncomingDamageMultiplier = Mathf.Clamp01(1f - Mathf.Max(0f, enemyModel.ActiveSkillFlatValue));
            enemyModel.IncomingDamageMultiplierRemainingSeconds = Mathf.Max(0.1f, enemyModel.ActiveSkillDuration);
            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, enemyModel.ActiveSkillDuration);
        }

        private static void ExecuteGuardianFlag(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager)
        {
            var allies = FindEnemyAlliesInRadius(enemyEntry, roster, Mathf.Max(0f, enemyModel.ActiveSkillRadius));
            var shield = Mathf.Max(0f, enemyModel.ActiveSkillFlatValue);
            for (var i = 0; i < allies.Count; i++)
            {
                combatManager.GrantShield(allies[i].Model, shield);
            }

            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, enemyModel.ActiveSkillDuration);
        }

        private static void ExecuteChargeCommand(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterService roster,
            InGameCombatManager combatManager)
        {
            var allies = FindEnemyAlliesInRadius(enemyEntry, roster, Mathf.Max(0f, enemyModel.ActiveSkillRadius));
            var duration = Mathf.Max(0.1f, enemyModel.ActiveSkillDuration);
            for (var i = 0; i < allies.Count; i++)
            {
                var ally = allies[i].Model as EnemyUnitRuntimeModel;
                if (ally == null)
                {
                    continue;
                }

                ally.MoveSpeedMultiplier = 1.2f;
                ally.MoveSpeedMultiplierRemainingSeconds = duration;
                ally.OutgoingDamageMultiplier = 1.15f;
                ally.OutgoingDamageMultiplierRemainingSeconds = duration;
            }

            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, duration);
        }

        private static void ExecuteSacredSwordWave(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry target,
            InGameCombatManager combatManager)
        {
            ExecuteEnemyProjectile(enemyEntry, enemyModel, target, combatManager, 12f, 4f);
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

        private static UnitRosterEntry FindLowestHealthEnemyAlly(UnitRosterService roster)
        {
            var enemies = roster.Enemies;
            UnitRosterEntry best = null;
            var bestHealthRatio = float.MaxValue;

            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!IsActive(candidate) || candidate.Model == null || candidate.Transform == null)
                {
                    continue;
                }

                var resources = candidate.Model.Resources;
                var stats = candidate.Model.Stats;
                if (resources == null || stats == null || stats.MaxHealth <= 0f)
                {
                    continue;
                }

                var healthRatio = Mathf.Clamp01(resources.CurrentHealth / stats.MaxHealth);
                if (healthRatio >= 1f || healthRatio >= bestHealthRatio)
                {
                    continue;
                }

                best = candidate;
                bestHealthRatio = healthRatio;
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
            moveSpeed *= ResolveMoveSpeedMultiplier(enemyModel);
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

        private static float ResolveAttackDamage(EnemyUnitRuntimeModel enemyModel)
        {
            return Mathf.Max(0f, ResolveAttackPower(enemyModel) * Mathf.Max(0f, enemyModel.ActiveSkillCoefficient) * ResolveOutgoingDamageMultiplier(enemyModel));
        }

        private static float ResolveAttackPower(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.Stats != null ? enemyModel.Stats.AttackPower : 0f;
        }

        private static float ResolveSpellPower(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.Stats != null ? enemyModel.Stats.SpellPower : 0f;
        }

        private static float ResolveOutgoingDamageMultiplier(EnemyUnitRuntimeModel enemyModel)
        {
            if (enemyModel == null)
            {
                return 1f;
            }

            var multiplier = Mathf.Max(0f, enemyModel.PassiveOutgoingDamageMultiplier);
            if (enemyModel.OutgoingDamageMultiplierRemainingSeconds > 0f)
            {
                multiplier *= Mathf.Max(0f, enemyModel.OutgoingDamageMultiplier);
            }

            return multiplier;
        }

        private static float ResolveMoveSpeedMultiplier(EnemyUnitRuntimeModel enemyModel)
        {
            return enemyModel != null && enemyModel.MoveSpeedMultiplierRemainingSeconds > 0f
                ? Mathf.Max(0f, enemyModel.MoveSpeedMultiplier)
                : 1f;
        }

        private static void TickTemporaryEnemyModifiers(EnemyUnitRuntimeModel enemyModel, float deltaTime)
        {
            TickTemporaryMultiplier(ref enemyModel.IncomingDamageMultiplierRemainingSeconds, ref enemyModel.IncomingDamageMultiplier, deltaTime);
            TickTemporaryMultiplier(ref enemyModel.OutgoingDamageMultiplierRemainingSeconds, ref enemyModel.OutgoingDamageMultiplier, deltaTime);
            TickTemporaryMultiplier(ref enemyModel.MoveSpeedMultiplierRemainingSeconds, ref enemyModel.MoveSpeedMultiplier, deltaTime);
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

        private static bool IsCooldownDrivenSelfOrAllySkill(StageOneEnemySkillKind skillKind)
        {
            switch (skillKind)
            {
                case StageOneEnemySkillKind.Heal:
                case StageOneEnemySkillKind.ShieldUp:
                case StageOneEnemySkillKind.GuardianFlag:
                case StageOneEnemySkillKind.ChargeCommand:
                    return true;
                default:
                    return false;
            }
        }

        private static bool CanExecuteCooldownDrivenSelfOrAllySkill(
            StageOneEnemySkillKind skillKind,
            UnitRosterService roster)
        {
            return skillKind != StageOneEnemySkillKind.Heal || FindLowestHealthEnemyAlly(roster) != null;
        }

        private static List<UnitRosterEntry> FindEnemyAlliesInRadius(
            UnitRosterEntry source,
            UnitRosterService roster,
            float radius)
        {
            var result = new List<UnitRosterEntry>();
            if (source == null || source.Transform == null || roster == null)
            {
                return result;
            }

            var radiusSq = radius > 0f ? radius * radius : 0f;
            var origin = source.Transform.position;
            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!IsActive(candidate) || candidate.Model == null || candidate.Transform == null)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                if (radius <= 0f && candidate != source)
                {
                    continue;
                }

                if (radius > 0f && offset.sqrMagnitude > radiusSq)
                {
                    continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        private static void SpawnAttachedEnemySkillEffect(
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            float duration)
        {
            if (target == null || target.Transform == null || combatManager == null)
            {
                return;
            }

            var prefab = combatManager.ResolveEnemySkillPrefab(enemyModel);
            if (prefab == null)
            {
                return;
            }

            var instance = combatManager.InstantiateSkillPrefab(prefab, target.Transform.position, Quaternion.identity);
            var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
            }

            actor.Initialize(target.Transform, Mathf.Max(0.1f, duration), Vector3.zero);
        }

        private static Quaternion ResolveRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        private static float ResolveEnemyProjectileBoundaryX(Vector3 origin, Vector3 direction)
        {
            var normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.left;
            return origin.x + normalized.x * 40f;
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
