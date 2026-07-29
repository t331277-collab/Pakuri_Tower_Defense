/*
 * 역할: Line 스킬 전달 조정.
 * 책임: Line 시전을 확정하고 Actor와 비주얼을 생성해 공통 충돌 규칙으로 적중을 전달한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>LineSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
    internal static class LineSkillExecutor
    {

        private static bool applyingHitEnhancement;

        /// <summary>전달된 런타임 입력값을 사용해 <c>HitEnhancements</c>를 적용한다.</summary>
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager,
            UnitSpawnManager roster,
            SkillUseState runtime,
            SkillExecutionData skillData,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            string sourceSkillId,
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage)
        {
            if (manager != null && roster != null && source != null && hitTarget != null && hitTarget.Model != null)
            {
                var actionExecutionContext = new SkillExecutionContext(
                    manager,
                    roster,
                    sourceEntry,
                    runtime,
                    hitTarget.Model);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnHit,
                    new SkillActionContext(
                        source,
                        sourceSkillId,
                        hitTarget.Model,
                        hitPosition,
                        primaryBaseDamage,
                        1,
                        skillData,
                        actionExecutionContext));
            }

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
                        primaryBaseDamage,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true,
                        finalDamageMultiplier: skillData.OnHitAdditionalDamageMultiplier);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ChainTargets(
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
                                primaryBaseDamage,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true,
                                finalDamageMultiplier: skillData.OnHitChainDamageMultiplier);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            LineSkillDefinition skill)
        {
            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var repeatCount = CastRepeatCount(skill, snapshot);
            var directions = CastDirections(context, skill, origin, repeatCount);
            if (directions.Count == 0)
            {
                return false;
            }

            if (!ExecuteOnce(context, snapshot, skill, origin, directions[0]))
            {
                return false;
            }

            if (repeatCount > 1)
            {
                context.CombatManager.StartCoroutine(ExecuteRepeatedLineCasts(
                    context,
                    snapshot,
                    skill,
                    origin,
                    directions,
                    CastRepeatInterval(skill)));
            }

            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>RepeatedLineCasts</c>를 실행한다.</summary>
        private static IEnumerator ExecuteRepeatedLineCasts(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            LineSkillDefinition skill,
            Vector2 origin,
            IReadOnlyList<Vector2> directions,
            float repeatIntervalSeconds)
        {
            for (var i = 1; i < directions.Count; i++)
            {
                yield return new WaitForSeconds(repeatIntervalSeconds);
                if (context == null
                    || context.CombatManager == null
                    || context.CasterEntry == null
                    || context.Caster == null
                    || skill == null)
                {
                    yield break;
                }

                ExecuteOnce(context, snapshot, skill, origin, directions[i]);
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>CastDirections</c> 결과값을 생성해 반환한다.</summary>
        private static List<Vector2> CastDirections(
            SkillExecutionContext context,
            LineSkillDefinition skill,
            Vector2 origin,
            int repeatCount)
        {
            var directions = new List<Vector2>(repeatCount);
            if (context.HasManualAimDirection)
            {
                var direction = context.ManualAimDirection;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    return directions;
                }

                direction.Normalize();
                for (var i = 0; i < repeatCount; i++)
                {
                    directions.Add(direction);
                }

                return directions;
            }

            var target = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var primaryDirection = SkillTargeting.DirectionToTarget(origin, target);
            if (primaryDirection.sqrMagnitude <= 0.0001f || target.Transform == null)
            {
                return directions;
            }

            var centers = SkillTargeting.TargetAnchoredCenters(
                context,
                skill.Targeting,
                target.Transform.position,
                repeatCount,
                false,
                SkillDeploymentRepeatMode.RepeatNearest);
            for (var i = 0; i < centers.Count; i++)
            {
                var direction = centers[i] - origin;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    continue;
                }

                directions.Add(direction.normalized);
            }

            return directions;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Once</c>를 실행한다.</summary>
        private static bool ExecuteOnce(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            LineSkillDefinition skill,
            Vector2 origin,
            Vector2 direction)
        {
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.StatusSpec(skill.OnHitStatus, snapshot);
            var length = LineLength(skill);
            var width = LineWidth(skill, snapshot);
            var knockbackDistance = KnockbackDistance(skill, snapshot);
            var duration = Duration(skill, snapshot);
            var tickInterval = TickInterval(skill, snapshot);
            var center = (Vector2)origin + direction * (length * 0.5f);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }
            if (effects == null)
            {
                return false;
            }

            var rotation = EffectVisualBuilder.Rotation(direction);
            var objectName = "LineSkill";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "LineSkill_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                runtimeVisual,
                prefab,
                objectName,
                center,
                rotation,
                null,
                0f,
                null,
                false,
                false,
                true));
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
                context.Runtime,
                snapshot,
                context.Caster,
                skill.SkillId,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
            SkillTrigger.PublishLifecycleEvent(
                SkillTriggerEvent.OnDeploymentCast,
                new SkillActionContext(context.Caster, skill.SkillId, null, center, 0f, 0, snapshot, context));
            return true;
        }

        /// <summary>전달된 <c>skill</c> 값을 사용해 <c>LineLength</c> 결과값을 생성해 반환한다.</summary>
        private static float LineLength(LineSkillDefinition skill)
        {
            return Mathf.Max(0.1f, skill != null ? skill.LineLength : 0f);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>CastRepeatCount</c> 결과값을 생성해 반환한다.</summary>
        private static int CastRepeatCount(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var baseCount = skill != null ? skill.CastRepeatCount : 1;
            var bonus = snapshot != null ? snapshot.LineCastRepeatCountBonus : 0;
            return Mathf.Max(1, baseCount + bonus);
        }

        /// <summary>전달된 <c>skill</c> 값을 사용해 <c>CastRepeatInterval</c> 결과값을 생성해 반환한다.</summary>
        private static float CastRepeatInterval(LineSkillDefinition skill)
        {
            return Mathf.Max(0f, skill != null ? skill.CastRepeatIntervalSeconds : 0f);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Duration</c> 결과값을 생성해 반환한다.</summary>
        private static float Duration(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var timing = skill != null ? skill.Timing : null;
            var duration = timing != null && timing.ActiveDuration > 0f
                ? timing.ActiveDuration
                : TickInterval(skill, snapshot);
            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>LineWidth</c> 결과값을 생성해 반환한다.</summary>
        private static float LineWidth(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var width = skill != null ? skill.LineWidth : 0f;
            if (snapshot != null)
            {
                width *= LineVisualWidthScale(snapshot);
            }

            return Mathf.Max(0.1f, width);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>KnockbackDistance</c> 결과값을 생성해 반환한다.</summary>
        private static float KnockbackDistance(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var distance = skill != null ? Mathf.Max(0f, skill.KnockbackDistance) : 0f;
            if (snapshot != null)
            {
                distance *= Mathf.Max(0f, snapshot.KnockbackDistanceMultiplier);
            }

            return Mathf.Max(0f, distance);
        }

        /// <summary>전달된 <c>snapshot</c> 값을 사용해 <c>LineVisualWidthScale</c> 결과값을 생성해 반환한다.</summary>
        private static float LineVisualWidthScale(SkillExecutionData snapshot)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Interval</c>를 경과 시간 기준으로 갱신한다.</summary>
        private static float TickInterval(LineSkillDefinition skill, SkillExecutionData snapshot)
        {
            var interval = TickInterval(skill);
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        /// <summary>전달된 <c>skill</c> 값을 사용해 <c>Interval</c>를 경과 시간 기준으로 갱신한다.</summary>
        private static float TickInterval(LineSkillDefinition skill)
        {
            var timing = skill != null ? skill.Timing : null;
            return timing != null && timing.TickInterval > 0f
                ? timing.TickInterval
                : 0.1f;
        }

    }
}
