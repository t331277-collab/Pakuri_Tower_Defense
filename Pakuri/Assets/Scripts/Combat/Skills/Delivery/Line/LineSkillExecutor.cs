using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 직선 공격을 준비하고 생성한 오브젝트의 처리를 LineSkillActor에 맡긴다.
 */
namespace Pakuri.InGame
{

    internal static class LineSkillExecutor
    {
        // 직선 공격의 방향, 길이, 폭, 지속시간을 조립하고 Actor 생성을 구현.
        /*
         * 현재 스킬의 노드 효과 중 요청한 실행 시점에 맞는 효과를 적용한다.
         */

        /*
         * 추가 효과의 지연시간이 지난 뒤 같은 Executor에서 효과를 적용한다.
         */

        /*
         * 추가 효과 종류에 맞는 실제 적용 기능을 호출한다.
         */

        private static bool applyingHitEnhancement;

        /*
         * 적중 후 추가 피해, 연쇄 피해, 재장전 감소 강화 효과를 적용한다.
         */
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitSpawnManager roster /* 전투 유닛 목록 */,
            SkillUseState runtime /* 실행 중인 스킬 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            CombatUnitEntry sourceEntry /* 시전자 등록 정보 */,
            UnitCombatState source /* 시전자 */,
            string sourceSkillId /* 원본 스킬 식별자 */,
            CombatUnitEntry hitTarget /* 최초 적중 대상 */,
            Vector2 hitPosition /* 최초 적중 위치 */,
            float primaryBaseDamage /* 최초 적중 기본 피해 */)
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

        /*
         * 같은 시전에서 예약된 추가 직선 공격을 순서대로 실행한다.
         */
        private static IEnumerator ExecuteRepeatedLineCasts(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            LineSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            Vector2 origin /* 직선 시작 위치 */,
            IReadOnlyList<Vector2> directions /* 반복별 직선 진행 방향 */,
            float repeatIntervalSeconds /* 반복 간격 */)
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

        /*
         * 자동 시전은 반복 수만큼 가까운 서로 다른 대상 방향을 만들고,
         * 수동 조준은 입력한 방향을 모든 반복에 유지한다.
         */
        private static List<Vector2> CastDirections(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            LineSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            Vector2 origin /* 직선 시작 위치 */,
            int repeatCount /* 한 번의 시전에서 실행할 총 횟수 */)
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

        /*
         * 한 번의 직선 공격을 실행한다.
         */
        private static bool ExecuteOnce(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            LineSkillDefinition skill /* 실행하거나 검사할 스킬 */,
            Vector2 origin /* 직선 시작 위치 */,
            Vector2 direction /* 직선 진행 방향 */)
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

        /*
         * Line 길이를 결정한다.
         */
        private static float LineLength(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            return Mathf.Max(0.1f, skill != null ? skill.LineLength : 0f);
        }

        /*
         * 한 번의 시전에서 실행할 직선 공격 횟수를 결정한다.
         */
        private static int CastRepeatCount(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            var baseCount = skill != null ? skill.CastRepeatCount : 1;
            var bonus = snapshot != null ? snapshot.LineCastRepeatCountBonus : 0;
            return Mathf.Max(1, baseCount + bonus);
        }

        /*
         * 직선 공격 반복 간격을 결정한다.
         */
        private static float CastRepeatInterval(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            return Mathf.Max(0f, skill != null ? skill.CastRepeatIntervalSeconds : 0f);
        }

        /*
         * 지속시간을 결정한다.
         */
        private static float Duration(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * Line 너비를 결정한다.
         */
        private static float LineWidth(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            var width = skill != null ? skill.LineWidth : 0f;
            if (snapshot != null)
            {
                width *= LineVisualWidthScale(snapshot);
            }

            return Mathf.Max(0.1f, width);
        }

        /*
         * 밀쳐내기 거리를 결정한다.
         */
        private static float KnockbackDistance(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
        private static float LineVisualWidthScale(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            return snapshot != null
                ? Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus)
                : 1f;
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float TickInterval(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            var interval = TickInterval(skill);
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float TickInterval(LineSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var timing = skill != null ? skill.Timing : null;
            return timing != null && timing.TickInterval > 0f
                ? timing.TickInterval
                : 0.1f;
        }

        /*
         * 적중 상태 효과를 결정한다.
         */

        /*
         * 시전 효과를 결정한다.
         */
    }
}
