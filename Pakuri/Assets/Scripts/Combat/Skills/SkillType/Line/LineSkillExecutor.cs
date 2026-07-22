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
        /*
         * 현재 스킬의 노드 효과 중 요청한 실행 시점에 맞는 효과를 적용한다.
         */
        internal static bool ExecuteAdditionalEffects(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition[] effects /* 적용할 추가 효과 목록 */,
            Vector2 defaultCenter /* 기본 효과 중심 */,
            bool requireTiming /* 특정 실행 시점만 처리할지 여부 */,
            SkillMultiEffectTiming timing /* 처리할 실행 시점 */,
            bool scaleStatusDuration /* 상태 지속시간 보정 여부 */,
            int hitCount = 0 /* 현재 적중 횟수 */,
            UnitCombatState eventTarget = null /* 현재 적중 대상 */,
            bool useEventTarget = false /* 적중 대상을 문맥에 넣을지 여부 */)
        {
            if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
            {
                return false;
            }

            var effectContext = context;
            if (useEventTarget)
            {
                effectContext = new SkillExecutionContext(
                    context.CombatManager,
                    context.Roster,
                    context.CasterEntry,
                    context.Runtime,
                    eventTarget,
                    context.HasManualAimDirection,
                    context.ManualAimDirection,
                    context.HasManualTargetPoint,
                    context.ManualTargetPoint,
                    context.RecastGeneration);
            }

            var applied = false;
            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (!SkillRequirement.CanRunEffect(effectContext, effect))
                {
                    continue;
                }
                if (requireTiming)
                {
                    if (effect.EffectTiming != timing)
                    {
                        continue;
                    }
                }
                else if (effect.EffectTiming == SkillMultiEffectTiming.OnHit
                    || effect.EffectTiming == SkillMultiEffectTiming.OnDeploymentCast
                    || effect.EffectTiming == SkillMultiEffectTiming.OnExpire
                    || effect.EffectTiming == SkillMultiEffectTiming.OnHitCount)
                {
                    continue;
                }
                if (!SkillRequirement.MatchesEffectHitCount(effect, hitCount))
                {
                    continue;
                }

                if (effect.EffectTiming == SkillMultiEffectTiming.Delayed || effect.DelaySeconds > 0f)
                {
                    effectContext.CombatManager.StartCoroutine(ApplyAdditionalEffectAfterDelay(
                        effectContext,
                        skillData,
                        effect,
                        defaultCenter,
                        scaleStatusDuration));
                    applied = true;
                }
                else
                {
                    applied = ApplyAdditionalEffect(
                        effectContext,
                        skillData,
                        effect,
                        defaultCenter,
                        scaleStatusDuration) || applied;
                }
            }
            return applied;
        }

        /*
         * 추가 효과의 지연시간이 지난 뒤 같은 Executor에서 효과를 적용한다.
         */
        private static IEnumerator ApplyAdditionalEffectAfterDelay(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            Vector2 defaultCenter /* 기본 효과 중심 */,
            bool scaleStatusDuration /* 상태 지속시간 보정 여부 */)
        {
            var delay = Mathf.Max(0f, effect.DelaySeconds);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }
            else
            {
                yield return null;
            }
            ApplyAdditionalEffect(context, skillData, effect, defaultCenter, scaleStatusDuration);
        }

        /*
         * 추가 효과 종류에 맞는 실제 적용 기능을 호출한다.
         */
        private static bool ApplyAdditionalEffect(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            Vector2 defaultCenter /* 기본 효과 중심 */,
            bool scaleStatusDuration /* 상태 지속시간 보정 여부 */)
        {
            if (effect == null || context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return false;
            }

            if (effect.EffectKind == SkillMultiEffectKind.Damage)
            {
                return ZoneSkillExecutor.ApplyAdditionalDamageEffect(context, skillData, effect, defaultCenter);
            }
            if (effect.EffectKind == SkillMultiEffectKind.Status)
            {
                return SkillStatus.ApplyEffect(context, skillData, effect, defaultCenter, scaleStatusDuration);
            }
            if (effect.EffectKind == SkillMultiEffectKind.ExtendStatusDuration)
            {
                return SkillStatus.ExtendEffectDuration(context, effect);
            }
            if (effect.EffectKind == SkillMultiEffectKind.RecastZone)
            {
                return ZoneSkillExecutor.ExecuteRecast(context, skillData, effect, defaultCenter);
            }
            return false;
        }

        private static bool applyingHitEnhancement;

        /*
         * 적중 후 추가 피해, 연쇄 피해, 재장전 감소 강화 효과를 적용한다.
         */
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitRegistry roster /* 전투 유닛 목록 */,
            SkillUseState runtime /* 실행 중인 스킬 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            CombatUnitEntry sourceEntry /* 시전자 등록 정보 */,
            UnitCombatState source /* 시전자 */,
            string sourceSkillId /* 원본 스킬 식별자 */,
            CombatUnitEntry hitTarget /* 최초 적중 대상 */,
            Vector2 hitPosition /* 최초 적중 위치 */,
            float primaryBaseDamage /* 최초 적중 기본 피해 */)
        {
            if (manager == null
                || roster == null
                || skillData == null
                || source == null
                || hitTarget == null
                || hitTarget.Model == null
                || primaryBaseDamage <= 0f
                || applyingHitEnhancement)
            {
                return;
            }

            var hasReloadReduction = !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillId)
                && skillData.ReloadReduceSecondsPerHit > 0f;
            if (!skillData.HasOnHitAdditionalDamageBehavior && !hasReloadReduction)
            {
                return;
            }

            var hitIndex = 0;
            if (runtime != null)
            {
                hitIndex = runtime.AdvanceSkillHitCount();
            }

            applyingHitEnhancement = true;
            try
            {
                if (hasReloadReduction && runtime != null && runtime.Owner != null && runtime.Owner.Skills != null)
                {
                    var reloadSkill = runtime.Owner.SkillState.FindBySkillId(skillData.ReloadReduceTargetSkillId);
                    if (reloadSkill != null && reloadSkill.IsReloading)
                    {
                        reloadSkill.ReduceReloadRemaining(skillData.ReloadReduceSecondsPerHit);
                    }
                }

                var targetsHitUnit = string.IsNullOrWhiteSpace(skillData.OnHitAdditionalDamageTarget)
                    || string.Equals(skillData.OnHitAdditionalDamageTarget, "HitTarget", StringComparison.OrdinalIgnoreCase);
                if (skillData.HasOnHitAdditionalDamage
                    && skillData.OnHitAdditionalDamageMultiplier > 0f
                    && targetsHitUnit
                    && hitTarget.IsAlive
                    && UnityEngine.Random.value <= Mathf.Clamp01(skillData.OnHitAdditionalDamageChance))
                {
                    manager.ApplyDamage(
                        hitTarget.Model,
                        primaryBaseDamage * skillData.OnHitAdditionalDamageMultiplier,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ResolveChainTargets(
                        roster,
                        sourceEntry,
                        source,
                        hitTarget,
                        hitPosition,
                        skillData.OnHitChainSearchRadius);
                    var targetCount = Mathf.Min(skillData.OnHitChainTargetCount, chainTargets.Count);
                    for (var i = 0; i < targetCount; i++)
                    {
                        var chainTarget = chainTargets[i];
                        if (chainTarget != null && chainTarget.IsAlive && chainTarget.Model != null)
                        {
                            manager.ApplyDamage(
                                chainTarget.Model,
                                primaryBaseDamage * skillData.OnHitChainDamageMultiplier,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        private const float DefaultLineLength = 31f;

        /*
         * 요청받은 Line 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
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
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick, snapshot.BaseDamageBonus, snapshot.DamageMultiplier);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var length = ResolveLineLength(skill);
            var width = ResolveLineWidth(skill, snapshot);
            var knockbackDistance = ResolveKnockbackDistance(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var planEffects = snapshot.CollectEffects(skill.MultiEffects);
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
                    ExecuteAdditionalEffects(context, snapshot, castEffects, center, false, SkillMultiEffectTiming.OnCast, false);
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
                ExecuteAdditionalEffects(context, snapshot, castEffects, center, false, SkillMultiEffectTiming.OnCast, false);
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
        private static float ResolveDuration(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
        private static float ResolveLineWidth(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
        private static float ResolveKnockbackDistance(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
        private static float ResolveLineVisualWidthScale(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
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
                    || !SkillRequirement.CanRunEffect(context, effect))
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
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
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
                    || !SkillRequirement.CanRunEffect(context, effect))
                {
                    continue;
                }

                resolved.Add(effect);
            }

            return resolved.Count > 0 ? resolved.ToArray() : Array.Empty<SkillEffectDefinition>();
        }
    }
}
