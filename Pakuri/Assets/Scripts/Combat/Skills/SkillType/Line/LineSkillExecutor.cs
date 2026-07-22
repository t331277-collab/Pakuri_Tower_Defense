using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * Line 스킬을 실행한다.
 */
namespace Pakuri.InGame
{

    internal static class LineSkillExecutor
    {
        private const float DefaultLineLength = 31f;

        /*
         * 요청받은 Line 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            LineSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillTargeting.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            direction.Normalize();
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var length = ResolveLineLength(skill);
            var width = ResolveLineWidth(skill, snapshot);
            var knockbackDistance = ResolveKnockbackDistance(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var planEffects = SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects);
            var onHitStatusEffects = ResolveOnHitStatusEffects(context, snapshot, planEffects);
            var castEffects = ResolveCastEffects(context, snapshot, planEffects);
            var center = (Vector2)origin + direction * (length * 0.5f);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }
            var hasEffectVisual = effects != null
                && ((runtimeVisual != null && runtimeVisual.HasVisual()) || prefab != null);

            if (!hasEffectVisual)
            {
                LineSkillActor.ApplyLineTick(
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
                    SkillEffect.Execute(context, snapshot, castEffects, center);
                }
                return true;
            }

            var rotation = EffectVisualBuilder.ResolveRotation(direction);
            var objectName = "LineSkill";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "LineSkill_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(
                runtimeVisual,
                prefab,
                objectName,
                center,
                rotation);
            if (instance == null)
            {
                return false;
            }

            var actor = instance.GetComponent<LineSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<LineSkillActor>();
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
                SkillEffect.Execute(context, snapshot, castEffects, center);
            }
            return true;
        }

        /*
         * Line 길이를 결정한다.
         */
        private static float ResolveLineLength(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            if (skill != null && skill.LineLength > 0f)
            {
                return skill.LineLength;
            }

            // Line 길이 기본값은 Line 실행기가 직접 소유한다.
            return DefaultLineLength;
        }

        /*
         * 지속시간을 결정한다.
         */
        private static float ResolveDuration(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * Line 너비를 결정한다.
         */
        private static float ResolveLineWidth(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            var width = skill != null ? skill.LineWidth : 0f;
            if (snapshot != null)
            {
                width *= ResolveLineVisualWidthScale(snapshot);
            }

            return Mathf.Max(0.1f, width);
        }

        /*
         * 밀쳐내기 거리를 결정한다.
         */
        private static float ResolveKnockbackDistance(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            var distance = skill != null ? Mathf.Max(0f, skill.KnockbackDistance) : 0f;
            if (snapshot != null)
            {
                distance *= Mathf.Max(0f, snapshot.KnockbackDistanceMultiplier);
            }

            return Mathf.Max(0f, distance);
        }

        /*
         * Line 비주얼 너비 크기를 결정한다.
         */
        private static float ResolveLineVisualWidthScale(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            var interval = ResolveTickInterval(skill);
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var timing = skill != null ? skill.Timing : null;
            return timing != null && timing.TickInterval > 0f
                ? timing.TickInterval
                : 0.1f;
        }

        /*
         * 적중 상태 효과를 결정한다.
         */
        private static SkillEffectDefinition[] ResolveOnHitStatusEffects(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillEffectDefinition[] effects /* 실행할 효과 목록 */)
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
                    || !SkillEffect.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }

        /*
         * 시전 효과를 결정한다.
         */
        private static SkillEffectDefinition[] ResolveCastEffects(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillEffectDefinition[] effects /* 실행할 효과 목록 */)
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
                    || !SkillEffect.ShouldRun(context, effect, snapshot))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }
    }
}
