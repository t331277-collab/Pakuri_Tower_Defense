using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 지속 범위 스킬을 실행한다.
 */
namespace Pakuri.InGame
{

    internal static class ZoneSkillExecutor
    {
        /*
         * 재시전을 실행한다.
         */
        internal static bool ExecuteRecast(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot inheritedSnapshot /* 앞 실행에서 이어받은 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 실행하거나 변환할 효과 */,
            Vector2 center /* 효과가 적용될 중심 위치 */)
        {
            var skill = context != null && context.Runtime != null
                ? context.Runtime.Data as ZoneSkillDefinition
                : null;
            if (skill == null
                || effect == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || context.CasterEntry == null
                || context.Roster == null
                || (!string.IsNullOrWhiteSpace(effect.RecastSourceSkillId)
                    && !string.Equals(effect.RecastSourceSkillId, skill.SkillId, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            var maxGeneration = Math.Max(1, effect.RecastMaxGeneration);
            if (context.RecastGeneration >= maxGeneration)
            {
                return false;
            }

            var snapshot = effect.RecastInheritSnapshot
                ? inheritedSnapshot
                : new SkillSnapshot(skill);
            var radius = ResolveRadius(skill, snapshot) * Mathf.Max(0f, effect.RecastRadiusMultiplier);
            var duration = Mathf.Max(0.05f, effect.RecastDurationSeconds);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var hitTargetCount = ResolveHitTargetCount(skill, snapshot);
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.ResolveStatusSpec(skill.OnTickStatus, snapshot);
            var planEffects = SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects);
            var expireEffects = ResolveOnExpireEffects(context, snapshot, planEffects);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }
            var objectName = "InGameRecastZone";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "InGameRecastZone_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(
                runtimeVisual,
                prefab,
                objectName,
                center,
                Quaternion.identity);
            if (instance == null)
            {
                instance = effects.CreateSkillActorObject(objectName, center, Quaternion.identity);
            }

            EffectVisualBuilder.ConfigureAreaEffect(
                instance,
                SkillTargeting.ResolveBaseRadius(skill.Targeting, skill.Area),
                snapshot,
                effect.RecastRadiusMultiplier);

            var actor = instance.GetComponent<ZoneSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ZoneSkillActor>();
            }

            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill.Targeting,
                center,
                radius,
                coverAll,
                duration,
                tickInterval,
                hitTargetCount,
                damage,
                attribute,
                statusSpec,
                context.Runtime,
                snapshot,
                expireEffects,
                context.Caster,
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f,
                context.RecastGeneration + 1);
            return true;
        }

        /*
         * 요청받은 지속 범위 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var deploymentCount = ResolveDeploymentCount(snapshot);
            var centers = ResolveAreaCenters(context, skill.Targeting, skill.Area, deploymentCount);
            var radius = ResolveRadius(skill, snapshot);
            var duration = ResolveDuration(skill, snapshot);
            var tickInterval = ResolveTickInterval(skill, snapshot);
            var hitTargetCount = ResolveHitTargetCount(skill, snapshot);
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick, snapshot);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.ResolveStatusSpec(skill.OnTickStatus, snapshot);
            var planEffects = SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects);
            var expireEffects = ResolveOnExpireEffects(context, snapshot, planEffects);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var prefab = skill.SkillEffectPrefab;
            if (snapshot != null && snapshot.SkillEffectPrefab != null)
            {
                prefab = snapshot.SkillEffectPrefab;
            }

            var routed = false;
            for (var i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                var objectName = "ZoneSkill";
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    objectName = "ZoneSkill_" + skill.SkillId;
                }

                var instance = effects.CreateEffect(
                    runtimeVisual,
                    prefab,
                    objectName,
                    center,
                    Quaternion.identity);
                if (instance == null)
                {
                    instance = effects.CreateSkillActorObject(objectName, center, Quaternion.identity);
                }

                EffectVisualBuilder.ConfigureAreaEffect(
                    instance,
                    SkillTargeting.ResolveBaseRadius(skill.Targeting, skill.Area),
                    snapshot);

                var actor = instance.GetComponent<ZoneSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ZoneSkillActor>();
                }

                actor.Initialize(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    skill.Targeting,
                    center,
                    radius,
                    coverAll,
                    duration,
                    tickInterval,
                    hitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    context.Runtime,
                    snapshot,
                    expireEffects,
                    context.Caster,
                    skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
                routed = true;
            routed = SkillEffect.Execute(context, snapshot, planEffects, center) || routed;
            }

            return routed;
        }

        /*
         * 배치 횟수를 결정한다.
         */
        private static int ResolveDeploymentCount(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            return 1 + (snapshot != null && snapshot.HasBranchCount ? Math.Max(0, snapshot.BranchCount) : 0);
        }

        /*
         * 적중 대상 횟수를 결정한다.
         */
        private static int ResolveHitTargetCount(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            if (skill == null || skill.HitAllTargets || !skill.UsesHitTargetCount)
            {
                return int.MaxValue;
            }

            var baseCount = Math.Max(1, skill.HitTargetCount);
            var bonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            return Math.Max(1, baseCount + bonus);
        }

        /*
         * 범위 중심점을 결정한다.
         */
        private static List<Vector2> ResolveAreaCenters(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */,
            int deploymentCount /* 배치 개수 */)
        {
            var primaryCenter = ResolveAreaCenter(context, targeting, area);
            var coverAll = (area != null && area.CoverAll)
                || (targeting != null && targeting.CoverAll);
            return SkillTargeting.ResolveTargetAnchoredCenters(
                context,
                targeting,
                primaryCenter,
                deploymentCount,
                coverAll,
                SkillDeploymentRepeatMode.RandomExisting);
        }

        /*
         * 범위 중심점을 결정한다.
         */
        private static Vector2 ResolveAreaCenter(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */)
        {
            return SkillTargeting.ResolveAreaCenter(context, targeting, area);
        }

        /*
         * 반경을 결정한다.
         */
        private static float ResolveRadius(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillTargeting.ResolveRadius(SkillTargeting.ResolveBaseRadius(targeting, area), snapshot);
        }

        /*
         * 지속시간을 결정한다.
         */
        private static float ResolveDuration(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var duration = area != null && area.Duration > 0f
                ? area.Duration
                : timing != null ? timing.ActiveDuration : 0f;
            if (duration <= 0f)
            {
                duration = ResolveTickInterval(skill, snapshot);
            }

            if (snapshot != null)
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            return Mathf.Max(0.05f, duration);
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float ResolveTickInterval(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var interval = area != null && area.TickInterval > 0f
                ? area.TickInterval
                : timing != null && timing.TickInterval > 0f ? timing.TickInterval : 1f;
            if (snapshot != null)
            {
                interval *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
            }

            return Mathf.Max(0.05f, interval);
        }

        /*
         * 종료 효과를 결정한다.
         */
        private static SkillEffectDefinition[] ResolveOnExpireEffects(
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
                    || effect.EffectTiming != SkillMultiEffectTiming.OnExpire
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
