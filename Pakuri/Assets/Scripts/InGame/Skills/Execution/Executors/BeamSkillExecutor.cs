using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public sealed class BeamSkillExecutor : TypedSkillExecutor<BeamSkillData>
    {
        private const float DefaultBeamLength = 31f;

        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillData as BeamSkillData : null;
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
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            direction.Normalize();
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            var statusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var length = ResolveBeamLength(skill);
            var width = ResolveBeamWidth(skill, snapshot);
            var knockbackDistance = ResolveKnockbackDistance(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            var onHitStatusEffects = ResolveOnHitStatusEffects(context, snapshot, planEffects);
            var castEffects = ResolveCastEffects(context, snapshot, planEffects);
            var center = (Vector2)origin + direction * (length * 0.5f);
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = EffectVisualUtility.HasVisual(runtimeVisual);
            var prefab = !hasRuntimeVisual && snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : !hasRuntimeVisual && context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;

            if ((!hasRuntimeVisual && prefab == null) || context.CombatManager.Effects == null)
            {
                InGameLineAttackActor.ApplyLineTick(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    skill.Targeting,
                    origin,
                    direction,
                    length,
                    width,
                    knockbackDistance,
                    damage,
                    attribute,
                    statusSpec,
                    onHitStatusEffects,
                    context.Runtime,
                    snapshot,
                    context.Caster,
                    skill.SkillId,
                    skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
                if (castEffects.Length > 0)
                {
                    SkillMultiEffectExecutor.Execute(context, snapshot, castEffects, center);
                }
                return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
            }

            var rotation = SkillExecutionUtility.ResolveRotation(direction);
            var instance = hasRuntimeVisual
                ? context.CombatManager.Effects.CreateRuntimeVisual(
                    runtimeVisual,
                    string.IsNullOrWhiteSpace(skill.SkillId) ? "InGameLineAttack" : $"InGameLineAttack_{skill.SkillId}",
                    center,
                    rotation)
                : context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, rotation);
            if (instance == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var actor = instance.GetComponent<InGameLineAttackActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameLineAttackActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill.Targeting,
                origin,
                direction,
                length,
                width,
                knockbackDistance,
                duration,
                tickInterval,
                damage,
                attribute,
                statusSpec,
                onHitStatusEffects,
                context.Runtime,
                snapshot,
                context.Caster,
                skill.SkillId,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            if (castEffects.Length > 0)
            {
                SkillMultiEffectExecutor.Execute(context, snapshot, castEffects, center);
            }
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        private static float ResolveBeamLength(BeamSkillData skill)
        {
            if (skill != null && skill.BeamLength > 0f)
            {
                return skill.BeamLength;
            }

            // Code Builder: 빔 길이 기본값은 빔 실행기가 직접 소유한다.
            return DefaultBeamLength;
        }

        private static float ResolveDuration(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var timing = skill != null ? skill.Timing : null;
            var duration = timing != null && timing.ActiveDuration > 0f
                ? timing.ActiveDuration
                : ResolveTickInterval(skill, snapshot);
            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        private static float ResolveBeamWidth(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var width = skill != null ? skill.BeamWidth : 0f;
            if (snapshot != null)
            {
                width *= ResolveBeamVisualWidthScale(snapshot);
            }

            return Mathf.Max(0.1f, width);
        }

        private static float ResolveKnockbackDistance(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var distance = skill != null ? Mathf.Max(0f, skill.KnockbackDistance) : 0f;
            if (snapshot != null)
            {
                distance *= Mathf.Max(0f, snapshot.KnockbackDistanceMultiplier);
            }

            return Mathf.Max(0f, distance);
        }

        private static float ResolveBeamVisualWidthScale(SkillExecutionSnapshot snapshot)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        private static float ResolveTickInterval(BeamSkillData skill, SkillExecutionSnapshot snapshot)
        {
            var interval = ResolveTickInterval(skill);
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        private static float ResolveTickInterval(BeamSkillData skill)
        {
            var timing = skill != null ? skill.Timing : null;
            return timing != null && timing.TickInterval > 0f
                ? timing.TickInterval
                : 0.1f;
        }

        private static SkillEffectDefinition[] ResolveOnHitStatusEffects(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects)
        {
            if (effects == null || effects.Length == 0)
            {
                return Array.Empty<SkillEffectDefinition>();
            }

            var resolved = new List<SkillEffectDefinition>();
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null
                    || effect.EffectTiming != SkillMultiEffectTiming.OnHit
                    || effect.EffectKind != SkillMultiEffectKind.Status
                    || effect.TargetSide != SkillMultiEffectTargetSide.Enemy
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }

        private static SkillEffectDefinition[] ResolveCastEffects(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SkillEffectDefinition[] effects)
        {
            if (effects == null || effects.Length == 0)
            {
                return Array.Empty<SkillEffectDefinition>();
            }

            var resolved = new List<SkillEffectDefinition>();
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null
                    || effect.EffectTiming == SkillMultiEffectTiming.OnHit
                    || !SkillMultiEffectExecutor.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }
    }
}


