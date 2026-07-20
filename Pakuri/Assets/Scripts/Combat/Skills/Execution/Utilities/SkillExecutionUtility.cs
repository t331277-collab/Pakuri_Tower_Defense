using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 실행 계산과 변환 기능을 제공한다.
 */
namespace Pakuri.InGame
{

    internal static class SkillExecutionUtility
    {
        /*
         * 가장 가까운 대상을 찾는다.
         */
        public static UnitRosterEntry FindNearestTarget(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return SkillTargetingUtility.FindNearestTarget(caster, roster, targeting);
        }

        /*
         * 대상을 방향을 계산한다.
         */
        public static Vector2 DirectionToTarget(Vector3 origin, UnitRosterEntry target)
        {
            return SkillTargetingUtility.DirectionToTarget(origin, target);
        }

        /*
         * 회전을 결정한다.
         */
        public static Quaternion ResolveRotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        /*
         * 프리팹 크기를 적용한다.
         */
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

        /*
         * 정렬된 대상을 결정한다.
         */
        public static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SkillTargetingSpec targetingSpec)
        {
            return ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec, null, 0);
        }

        /*
         * 정렬된 대상을 결정한다.
         */
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

        /*
         * 피해를 결정한다.
         */
        public static float ResolveDamage(
            BaseUnitRuntimeModel caster,
            SkillDamageSpec damage,
            SkillExecutionSnapshot snapshot)
        {
            if (damage == null)
            {
                return 0f;
            }

            var baseDamage = ResolvePowerValue(caster, damage);
            if (snapshot != null)
            {
                baseDamage = (baseDamage + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.DamageMultiplier);
            }

            baseDamage *= StatusEffectRules.ResolveOutgoingDamageMultiplier(caster, MapAttribute(damage.Element), damage.SkillId);
            if (caster is EnemyUnitRuntimeModel enemy)
            {
                baseDamage *= EnemyPassiveRuntime.ResolveOutgoingDamageMultiplier(
                    enemy,
                    MapAttribute(damage.Element));
            }

            return Mathf.Max(0f, baseDamage);
        }

        /*
         * 능력치 값을 결정한다.
         */
        public static float ResolvePowerValue(BaseUnitRuntimeModel caster, SkillDamageSpec spec)
        {
            if (spec == null)
            {
                return 0f;
            }

            if (spec.UseCombinedStatCoefficients)
            {
                var attack = ResolveStat(caster, StatSource.Attack);
                var spell = ResolveStat(caster, StatSource.Intelligence);
                return Mathf.Max(
                    0f,
                    spec.BaseDamage
                    + attack * spec.AttackPowerCoefficient
                    + spell * spec.SpellPowerCoefficient);
            }

            var stat = ResolveStat(caster, spec.StatSource);
            return Mathf.Max(0f, spec.BaseDamage + stat * spec.StatCoefficient);
        }

        /*
         * 보호막을 결정한다.
         */
        public static float ResolveShield(BaseUnitRuntimeModel caster, BuffShieldSkillRuntimeData skill, SkillExecutionSnapshot snapshot = null)
        {
            if (skill == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, skill.ShieldStatSource);
            var shield = Mathf.Max(0f, skill.ShieldBase + stat * skill.ShieldCoefficient);
            if (snapshot != null)
            {
                shield = (shield + snapshot.BaseDamageBonus)
                    * Mathf.Max(0f, snapshot.DamageMultiplier)
                    * Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
            }

            return Mathf.Max(0f, shield);
        }

        /*
         * 투사체 수명을 결정한다.
         */
        public static float ResolveProjectileLifetime(ProjectileSkillRuntimeData skill)
        {
            var projectile = skill != null ? skill.Projectile : null;
            if (projectile != null && projectile.LifetimeSeconds > 0f)
            {
                return projectile.LifetimeSeconds;
            }

            var speed = projectile != null ? Mathf.Max(0.1f, projectile.ProjectileSpeed) : 1f;
            const float battlefieldTravelDistance = 31f;
            return Mathf.Max(0.25f, battlefieldTravelDistance / speed + 0.5f);
        }

        /*
         * 대상의 방어와 상태를 반영한 최종 피해를 계산한다.
         */
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

        /*
         * 속성을 런타임 값으로 변환한다.
         */
        public static DamageAttribute MapAttribute(DamageAttribute element)
        {
            return (DamageAttribute)(int)element;
        }

        /*
         * 능력치를 결정한다.
         */
        private static float ResolveStat(BaseUnitRuntimeModel caster, StatSource source)
        {
            var stats = caster != null ? caster.Stats : null;
            if (stats == null)
            {
                return 0f;
            }

            if (source == StatSource.Attack)
            {
                return stats.AttackPower * StatusEffectRules.ResolveAttackPowerMultiplier(caster);
            }

            return stats.SpellPower * StatusEffectRules.ResolveSpellPowerMultiplier(caster);
        }

        /*
         * 대상 목록을 결정한다.
         */
        public static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return SkillTargetingUtility.ResolveTargetList(caster, roster, targeting);
        }

        /*
         * 대상을 비교한다.
         */
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
            if (selection == SkillTargetSelection.Farthest)
            {
                return rightDistance.CompareTo(leftDistance);
            }

            return leftDistance.CompareTo(rightDistance);
        }

        /*
         * 거리 제곱을 결정한다.
         */
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

        /*
         * 상태 중첩을 결정한다.
         */
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

