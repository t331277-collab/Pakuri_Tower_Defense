using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    public abstract class TypedSkillExecutor<TSkillData> : IInGameSkillExecutor
        where TSkillData : SkillData
    {
        public bool CanExecute(SkillData skillData)
        {
            return skillData is TSkillData;
        }

        public virtual SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skillId = snapshot != null ? snapshot.SkillId : string.Empty;
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skillId, GetType().Name);
        }
    }

    public sealed class ProjectileSkillExecutor : TypedSkillExecutor<ProjectileSkillData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as ProjectileSkillData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillExecutionUtility.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.Damage != null ? skill.Damage.Element : skill.Element);
            var prefab = skill.Projectile != null ? skill.Projectile.ProjectilePrefab : null;
            if (prefab == null)
            {
                prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                    ? snapshot.SkillEffectPrefab
                    : context.CombatManager.ResolveSkillEffectPrefab(skill.SkillId);
            }

            if (prefab == null)
            {
                if (target != null)
                {
                    context.CombatManager.ApplyDamage(target.Model, damage, attribute);
                    return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
                }

                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var instance = Object.Instantiate(prefab, origin, Quaternion.identity);
            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            var projectile = skill.Projectile;
            var speed = projectile != null ? projectile.ProjectileSpeed : 0f;
            var pierce = projectile != null ? projectile.PierceCount : 0;
            if (snapshot != null)
            {
                pierce += snapshot.PierceBonus;
            }

            actor.Initialize(
                context.CombatManager,
                context.Caster,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                context.CombatManager.ResolveProjectileDestroyBoundaryX(),
                SkillExecutionUtility.ResolveProjectileLifetime(skill));

            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }
    }

    public sealed class BeamSkillExecutor : TypedSkillExecutor<BeamSkillData>
    {
    }

    public sealed class ZoneSkillExecutor : TypedSkillExecutor<ZoneSkillData>
    {
    }

    public sealed class BuffSkillExecutor : TypedSkillExecutor<BuffSkillData>
    {
    }

    public sealed class ShieldSkillExecutor : TypedSkillExecutor<ShieldSkillData>
    {
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as ShieldSkillData : null;
            if (skill == null || context.CombatManager == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var shield = SkillExecutionUtility.ResolveShield(context.Caster, skill);
            var duration = Mathf.Max(skill.ShieldDuration, skill.Timing != null ? skill.Timing.ActiveDuration : 0f);
            if (duration <= 0f)
            {
                duration = 5f;
            }
            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.ResolveSkillEffectPrefab(skill.SkillId);

            var targets = context.Roster.Players;
            var routed = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                context.CombatManager.GrantShield(target.Model, shield);
                if (prefab != null && target.Transform != null)
                {
                    var instance = Object.Instantiate(prefab, target.Transform.position, Quaternion.identity);
                    var actor = instance.GetComponent<InGameAttachedSkillEffectActor>();
                    if (actor == null)
                    {
                        actor = instance.AddComponent<InGameAttachedSkillEffectActor>();
                    }

                    actor.Initialize(target.Transform, duration, Vector3.zero);
                }

                routed = true;
            }

            return new SkillExecutionResult(routed ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }
    }

    public sealed class PassiveSkillExecutor : TypedSkillExecutor<PassiveSkillData>
    {
    }

    internal static class SkillExecutionUtility
    {
        public static UnitRosterEntry FindNearestTarget(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            if (caster == null || caster.Transform == null || roster == null)
            {
                return null;
            }

            var candidates = ResolveTargetList(caster, roster, targeting);
            var range = targeting != null && targeting.Range > 0f ? targeting.Range : float.MaxValue;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = caster.Transform.position;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || candidate.Transform == null)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq > range * range || distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        public static Vector2 DirectionToTarget(Vector3 origin, UnitRosterEntry target)
        {
            if (target == null || target.Transform == null)
            {
                return Vector2.zero;
            }

            var direction = target.Transform.position - origin;
            direction.z = 0f;
            return direction;
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

            return Mathf.Max(0f, baseDamage);
        }

        public static float ResolveShield(BaseUnitRuntimeModel caster, ShieldSkillData skill)
        {
            if (skill == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, skill.ShieldStatSource);
            return Mathf.Max(0f, skill.ShieldBase + stat * skill.ShieldCoefficient);
        }

        public static float ResolveProjectileLifetime(ProjectileSkillData skill)
        {
            var projectile = skill != null ? skill.Projectile : null;
            var speed = projectile != null ? Mathf.Max(0.1f, projectile.ProjectileSpeed) : 1f;
            var range = skill != null && skill.Targeting != null && skill.Targeting.Range > 0f
                ? skill.Targeting.Range
                : 31f;
            return Mathf.Max(0.25f, range / speed + 0.5f);
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

            return source == StatSource.Attack ? stats.AttackPower : stats.SpellPower;
        }

        private static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            var side = targeting != null ? targeting.TargetSide : SkillTargetSide.Enemy;
            if (side == SkillTargetSide.Ally || side == SkillTargetSide.AllAllies || side == SkillTargetSide.Self)
            {
                return caster.Model != null
                    && caster.Model.Identity != null
                    && caster.Model.Identity.Side == UnitSide.Enemy
                        ? roster.Enemies
                        : roster.Players;
            }

            return caster.Model != null
                && caster.Model.Identity != null
                && caster.Model.Identity.Side == UnitSide.Enemy
                    ? roster.Players
                    : roster.Enemies;
        }
    }
}
