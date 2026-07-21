using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * Buff 계열 스킬의 세부 실행기를 정의한다.
 * 일반 버프와 보호막·회복 처리를 각 전용 실행기로 전달한다.
 */
namespace Pakuri.InGame
{
    internal static class BuffSkillExecutor
    {
        /*
         * 요청받은 버프 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context,
            SkillSnapshot snapshot,
            BuffSkillRuntimeData skill)
        {
            var statusSpec = ResolveBuffStatusSpec(skill, snapshot);
            if (statusSpec == null)
            {
                return false;
            }

            var targets = skill.UseConfiguredTargeting
                ? ResolveConfiguredTargets(context.CasterEntry, context.Roster, skill.Targeting)
                : ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }
            var routed = false;
            var castCommitted = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                castCommitted = true;
                if (UnityEngine.Random.value > Mathf.Clamp01(statusSpec.Chance))
                {
                    continue;
                }

                context.CombatManager.ApplyStatus(
                    target.Model,
                    statusSpec.StatusData,
                    statusSpec.Stacks,
                    statusSpec.DurationSeconds,
                    statusSpec.MaxStacks,
                    statusSpec.Permanent,
                    statusSpec.RefreshDuration,
                    context.Caster);

                var visualTarget = skill.AttachVisualToCaster ? context.CasterEntry.Transform : target.Transform;
                var canSpawnVisual = !skill.AttachVisualToCaster || !casterVisualSpawned;
                GameObject visualInstance = null;
                if (canSpawnVisual && visualTarget != null && effects != null)
                {
                    visualInstance = effects.SpawnAttachedEffect(
                        runtimeVisual,
                        prefab,
                        string.IsNullOrWhiteSpace(skill.SkillId)
                            ? "RuntimeBuffVisual"
                            : $"RuntimeBuffVisual_{skill.SkillId}",
                        visualTarget,
                        statusSpec.DurationSeconds,
                        Vector3.zero);
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            var multiEffectRouted = false;
            var planEffects = SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillEffect.ExecuteWithStatusDurationScaling(context, snapshot, planEffects, center);
            }

            return routed || castCommitted || multiEffectRouted;
        }

        /*
         * 버프 상태 설정을 결정한다.
         */
        private static ProjectileStatusHitSpec ResolveBuffStatusSpec(BuffSkillRuntimeData skill, SkillSnapshot snapshot)
        {
            if (skill == null)
            {
                return null;
            }

            return SkillStatus.ResolveStatusSpec(skill.AttachedStatus, snapshot);
        }

        /*
         * 버프 대상을 결정한다.
         */
        internal static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> ResolveBuffTargets(
            CombatUnitEntry caster,
            CombatUnitRegistry roster,
            BuffTarget targetMode)
        {
            if (targetMode == BuffTarget.Self)
            {
                return caster != null
                    ? new[] { caster }
                    : System.Array.Empty<CombatUnitEntry>();
            }

            return SkillTargeting.ResolveTargetList(
                caster,
                roster,
                new SkillTargetingSpec
                {
                    TargetSide = SkillTargetSide.AllAllies,
                    Selection = SkillTargetSelection.Owner,
                    Shape = SkillTargetShape.Battlefield,
                    CoverAll = true
                });
        }

        /*
         * 설정된 대상을 결정한다.
         */
        internal static IReadOnlyList<CombatUnitEntry> ResolveConfiguredTargets(
            CombatUnitEntry caster,
            CombatUnitRegistry roster,
            SkillTargetingSpec targeting)
        {
            var targets = SkillTargeting.ResolveOrderedTargets(caster, roster, targeting);
            if (caster == null || caster.Transform == null || targeting == null || targeting.Radius <= 0f)
            {
                return targets;
            }

            var radiusSq = targeting.Radius * targeting.Radius;
            targets.RemoveAll(target =>
                target == null
                || target.Transform == null
                || ((Vector2)target.Transform.position - (Vector2)caster.Transform.position).sqrMagnitude > radiusSq);
            return targets;
        }
    }

    /*
     * 보호막 스킬을 실행한다.
     */
    internal static class BuffShieldSkillExecutor
    {
        /*
         * 요청받은 보호막 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context,
            SkillSnapshot snapshot,
            BuffShieldSkillRuntimeData skill)
        {
            var shield = DamageCalculator.ResolveShield(context.Caster, skill, snapshot);
            var duration = skill.ShieldDuration > 0f
                ? skill.ShieldDuration
                : skill.ShieldStatus != null ? skill.ShieldStatus.Duration : 0f;
            if (snapshot != null
                && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                    || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            var statusData = SkillStatus.ResolveStatusData(skill.ShieldStatus, StatusEffectKind.Shield, snapshot);
            if (statusData == null || duration <= 0f)
            {
                return false;
            }

            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }

            var targets = skill.UseConfiguredTargeting
                ? BuffSkillExecutor.ResolveConfiguredTargets(context.CasterEntry, context.Roster, skill.Targeting)
                : BuffSkillExecutor.ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            var routed = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    statusData,
                    shield,
                    duration,
                    1,
                    0,
                    false,
                    true,
                    context.Caster);
                var visualTarget = skill.AttachVisualToCaster ? context.CasterEntry.Transform : target.Transform;
                var canSpawnVisual = !skill.AttachVisualToCaster || !casterVisualSpawned;
                GameObject visualInstance = null;
                if (canSpawnVisual && visualTarget != null && effects != null)
                {
                    visualInstance = effects.SpawnAttachedEffect(
                        runtimeVisual,
                        prefab,
                        string.IsNullOrWhiteSpace(skill.SkillId)
                            ? "RuntimeShieldVisual"
                            : $"RuntimeShieldVisual_{skill.SkillId}",
                        visualTarget,
                        duration,
                        Vector3.zero);
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            var multiEffectRouted = false;
            var planEffects = SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillEffect.ExecuteWithStatusDurationScaling(context, snapshot, planEffects, center);
            }

            return routed || multiEffectRouted;
        }
    }

    /*
     * 회복 스킬을 실행한다.
     */
    internal static class BuffHealSkillExecutor
    {
        /*
         * 요청받은 회복 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context,
            SkillSnapshot snapshot,
            BuffHealSkillRuntimeData skill)
        {
            var targets = SkillTargeting.ResolveOrderedTargets(context.CasterEntry, context.Roster, skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            if (target == null || target.Model == null)
            {
                return false;
            }

            var amount = DamageCalculator.ResolvePowerValue(context.Caster, skill.Healing);
            if (context.Caster is EnemyCombatState enemy)
            {
                amount *= Mathf.Max(0f, enemy.PassiveHealingMultiplier);
            }

            context.CombatManager.Heal(target.Model, amount);
            var effects = context.CombatManager.Effects;
            if (effects != null)
            {
                effects.SpawnAttachedSkillEffect(skill, target.Transform, 0.8f);
            }
            return true;
        }
    }

}
