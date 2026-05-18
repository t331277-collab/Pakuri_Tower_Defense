using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
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

        private static void ExecuteSlash(
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
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

            var origin = enemyEntry.Transform.position + direction.normalized * Mathf.Min(radius * 0.5f, 0.75f);
            var instance = effects.InstantiateSkillPrefab(prefab, origin, ResolveRotation(direction));
            if (instance == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

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

        private static void ExecuteEnemyProjectile(
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
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

            var instance = effects.InstantiateSkillPrefab(prefab, enemyEntry.Transform.position, ResolveRotation(direction));
            if (instance == null)
            {
                combatManager.ApplyDamage(target.Model, damage, enemyModel.Attribute);
                return;
            }

            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            actor.Initialize(
                combatManager,
                enemyModel,
                direction,
                ResolveEnemyProjectileSpeed(skillData, projectileSpeed),
                damage,
                enemyModel.Attribute,
                0,
                ResolveEnemyProjectileBoundaryX(enemyEntry.Transform.position, direction),
                ResolveEnemyProjectileLifetime(skillData, lifetimeSeconds));
        }

        private static void ExecuteHeal(
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

        private static void ExecuteShieldUp(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            InGameCombatManager combatManager,
            EnemyResolvedSkillData skillData)
        {
            enemyModel.IncomingDamageMultiplier = Mathf.Clamp01(1f - Mathf.Max(0f, skillData.FlatValue));
            enemyModel.IncomingDamageMultiplierRemainingSeconds = Mathf.Max(0.1f, skillData.Duration);
            SpawnAttachedEnemySkillEffect(enemyEntry, enemyModel, combatManager, skillData, skillData.Duration);
        }

        private static void ExecuteGuardianFlag(
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

        private static void ExecuteChargeCommand(
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

        private static float ResolveAttackDamage(EnemyUnitRuntimeModel enemyModel, EnemyResolvedSkillData skillData)
        {
            return Mathf.Max(0f, ResolveAttackPower(enemyModel) * Mathf.Max(0f, skillData.Coefficient) * ResolveOutgoingDamageMultiplier(enemyModel));
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

        private static float ResolveEnemyProjectileSpeed(EnemyResolvedSkillData skillData, float fallbackSpeed)
        {
            return skillData.ProjectileSpeed > 0f
                ? skillData.ProjectileSpeed
                : fallbackSpeed;
        }

        private static float ResolveEnemyProjectileLifetime(EnemyResolvedSkillData skillData, float fallbackLifetime)
        {
            return skillData.ProjectileLifetime > 0f
                ? skillData.ProjectileLifetime
                : fallbackLifetime;
        }

        private static void SpawnAttachedEnemySkillEffect(
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
    }
}
