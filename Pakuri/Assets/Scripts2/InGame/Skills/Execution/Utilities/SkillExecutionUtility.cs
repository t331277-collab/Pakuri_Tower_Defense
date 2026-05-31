using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    internal static class SkillExecutionUtility
    {
        public static UnitRosterEntry FindNearestTarget(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return SkillTargetingUtility.FindNearestTarget(caster, roster, targeting);
        }

        public static Vector2 DirectionToTarget(Vector3 origin, UnitRosterEntry target)
        {
            return SkillTargetingUtility.DirectionToTarget(origin, target);
        }

        public static Quaternion ResolveRotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        public static void ApplyPrefabScale(Transform target, float baseRadius, SkillExecutionSnapshot snapshot)
        {
            if (target == null || snapshot == null)
            {
                return;
            }

            var scaleFactor = SkillAreaUtility.ResolvePrefabScaleFactor(baseRadius, snapshot);
            if (Mathf.Approximately(scaleFactor, 1f))
            {
                return;
            }

            target.localScale *= scaleFactor;
        }

        public static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec)
        {
            return ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec, null, 0);
        }

        public static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec,
            string requiredStatusId,
            int requiredStatusMinStacks)
        {
            var candidates = SkillTargetingUtility.ResolveTargetList(
                sourceEntry,
                unitRoster,
                targetingSpec,
                requiredStatusId,
                requiredStatusMinStacks);
            var targets = new List<UnitRosterEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && target.IsAlive && target.Model != null && target.Transform != null)
                {
                    targets.Add(target);
                }
            }

            var selection = targetingSpec != null ? targetingSpec.Selection : SkillTargetSelection.Nearest;
            targets.Sort((left, right) => CompareTargets(sourceEntry, targetingSpec, selection, left, right));
            return targets;
        }

        public static Vector2 ResolveTargetGroupCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            Vector2 fallbackCenter)
        {
            if (context == null || context.CasterEntry == null || context.Roster == null)
            {
                return fallbackCenter;
            }

            var targets = ResolveOrderedTargets(context.CasterEntry, context.Roster, targeting);
            if (targets.Count <= 0)
            {
                return fallbackCenter;
            }

            var sum = Vector2.zero;
            var count = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || target.Transform == null)
                {
                    continue;
                }

                sum += (Vector2)target.Transform.position;
                count++;
            }

            return count > 0 ? sum / count : fallbackCenter;
        }

        public static float ResolveDamage(
            BaseUnitRuntimeModel caster,
            SkillDamageSpec damage,
            SkillExecutionSnapshot snapshot)
        {
            if (damage == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, damage.StatSource);
            var baseDamage = Mathf.Max(0f, damage.BaseDamage + stat * damage.StatCoefficient);
            if (snapshot != null)
            {
                baseDamage = (baseDamage + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.DamageMultiplier);
            }

            baseDamage *= StatusEffectRuntime.ResolveOutgoingDamageMultiplier(caster, MapAttribute(damage.Element), damage.SkillId);
            return Mathf.Max(0f, baseDamage);
        }

        public static float ResolveShield(BaseUnitRuntimeModel caster, ShieldSkillData skill, SkillExecutionSnapshot snapshot = null)
        {
            if (skill == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, skill.ShieldStatSource);
            var shield = Mathf.Max(0f, skill.ShieldBase + stat * skill.ShieldCoefficient);
            if (snapshot != null)
            {
                shield = (shield + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.DamageMultiplier);
            }

            return Mathf.Max(0f, shield);
        }

        public static float ResolveProjectileLifetime(ProjectileSkillData skill)
        {
            var projectile = skill != null ? skill.Projectile : null;
            var speed = projectile != null ? Mathf.Max(0.1f, projectile.ProjectileSpeed) : 1f;
            const float battlefieldTravelDistance = 31f;
            return Mathf.Max(0.25f, battlefieldTravelDistance / speed + 0.5f);
        }

        public static float ResolveDamageAgainstTarget(
            float baseDamage,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel target)
        {
            if (snapshot == null || target == null)
            {
                return Mathf.Max(0f, baseDamage);
            }

            return Mathf.Max(0f, baseDamage * snapshot.ResolveConditionalDamageMultiplier(target));
        }

        public static DamageAttribute MapAttribute(ElementType element)
        {
            return (DamageAttribute)(int)element;
        }

        private static float ResolveStat(BaseUnitRuntimeModel caster, StatSource source)
        {
            var stats = caster != null ? caster.Stats : null;
            if (stats == null)
            {
                return 0f;
            }

            if (source == StatSource.Attack)
            {
                return stats.AttackPower * StatusEffectRuntime.ResolveAttackPowerMultiplier(caster);
            }

            return stats.SpellPower * StatusEffectRuntime.ResolveSpellPowerMultiplier(caster);
        }

        public static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return SkillTargetingUtility.ResolveTargetList(caster, roster, targeting);
        }

        private static int CompareTargets(
            UnitRosterEntry sourceEntry,
            SkillTargetingSpec targetingSpec,
            SkillTargetSelection selection,
            UnitRosterEntry left,
            UnitRosterEntry right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var leftHealth = left.Model != null && left.Model.Resources != null ? left.Model.Resources.CurrentHealth : 0f;
            var rightHealth = right.Model != null && right.Model.Resources != null ? right.Model.Resources.CurrentHealth : 0f;
            if (selection == SkillTargetSelection.HighestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return rightHealth.CompareTo(leftHealth);
            }

            if (selection == SkillTargetSelection.LowestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return leftHealth.CompareTo(rightHealth);
            }

            if (selection == SkillTargetSelection.HighestStacks)
            {
                var statusId = targetingSpec != null ? targetingSpec.SelectionStatusId : string.Empty;
                var leftStacks = ResolveStatusStacks(left != null ? left.Model : null, statusId);
                var rightStacks = ResolveStatusStacks(right != null ? right.Model : null, statusId);
                if (leftStacks != rightStacks)
                {
                    return rightStacks.CompareTo(leftStacks);
                }
            }

            var leftDistance = ResolveDistanceSquared(sourceEntry, left);
            var rightDistance = ResolveDistanceSquared(sourceEntry, right);
            return leftDistance.CompareTo(rightDistance);
        }

        private static float ResolveDistanceSquared(UnitRosterEntry sourceEntry, UnitRosterEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
        }

        private static int ResolveStatusStacks(BaseUnitRuntimeModel model, string statusId)
        {
            if (model == null || string.IsNullOrWhiteSpace(statusId))
            {
                return 0;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return 0;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f ? 1 : 0;
            }

            return model.Statuses != null ? model.Statuses.GetStacks(kind) : 0;
        }
    }
}

