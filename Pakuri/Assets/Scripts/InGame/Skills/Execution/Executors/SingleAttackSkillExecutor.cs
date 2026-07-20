using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /*
     * 단일 공격 스킬을 실행한다.
     */
    public sealed class SingleAttackSkillExecutor : TypedSkillExecutor<SingleAttackSkillRuntimeData>
    {
        private const float DefaultVisualLifetimeSeconds = 1f;
        private const float DefaultMultiDeploymentLineLength = 31f;
        private const float PostDamageLifetimePaddingSeconds = 0.05f;

        /*
         * 단일 공격 실행 결과에 필요한 값을 보관한다.
         */
        private readonly struct SingleAttackExecutionOutcome
        {
            /*
             * 단일 공격 실행 결과에 필요한 값을 초기화한다.
             */
            public SingleAttackExecutionOutcome(bool routed, bool castCommitted)
            {
                Routed = routed;
                CastCommitted = castCommitted;
            }

            public bool Routed { get; }
            public bool CastCommitted { get; }
        }

        /*
         * 단일 공격 후속 설정에 필요한 값을 보관한다.
         */
        private readonly struct SingleAttackFollowUpSpec
        {
            /*
             * 단일 공격 후속 설정에 필요한 값을 초기화한다.
             */
            public SingleAttackFollowUpSpec(string requiredStatusId, int repeatCount, float intervalSeconds, float damageMultiplier, GameObject prefab)
            {
                RequiredStatusId = requiredStatusId;
                RepeatCount = repeatCount;
                IntervalSeconds = intervalSeconds;
                DamageMultiplier = damageMultiplier;
                Prefab = prefab;
            }

            public string RequiredStatusId { get; }
            public int RepeatCount { get; }
            public float IntervalSeconds { get; }
            public float DamageMultiplier { get; }
            public GameObject Prefab { get; }
        }

        /*
         * 단일 공격 후속 대상에 필요한 값을 보관한다.
         */
        private readonly struct SingleAttackFollowUpTarget
        {
            /*
             * 단일 공격 후속 대상에 필요한 값을 초기화한다.
             */
            public SingleAttackFollowUpTarget(BaseUnitRuntimeModel model, Vector2 center)
            {
                Model = model;
                Center = center;
            }

            public BaseUnitRuntimeModel Model { get; }
            public Vector2 Center { get; }
        }

        /*
         * 대상 피해 결과에 필요한 값을 보관한다.
         */
        private readonly struct TargetDamageResolution
        {
            /*
             * 대상 피해 결과에 필요한 값을 초기화한다.
             */
            public TargetDamageResolution(float damage, float critChanceBonus, bool isExecute, int plannedConsumedStacks)
            {
                Damage = damage;
                CritChanceBonus = critChanceBonus;
                IsExecute = isExecute;
                PlannedConsumedStacks = plannedConsumedStacks;
            }

            public float Damage { get; }
            public float CritChanceBonus { get; }
            public bool IsExecute { get; }
            public int PlannedConsumedStacks { get; }
        }

        /*
         * 요청받은 단일 공격 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as SingleAttackSkillRuntimeData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            if (SingleAttackSkillRuleHandlers.ShouldRejectCastForExecuteThreshold(context, snapshot, skill))
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var center = ResolveAreaCenter(context, skill.Targeting, skill.Area);
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = EffectVisualUtility.HasVisual(runtimeVisual);
            var prefab = hasRuntimeVisual ? null : ResolvePrefab(context, snapshot, skill);
            var outcome = UsesResolvedDeployments(skill)
                ? ExecuteResolvedDeployments(context, snapshot, skill, center, runtimeVisual, prefab)
                : ExecuteAtCenter(context, snapshot, skill, center, runtimeVisual, prefab, true);
            var multiEffectRouted = SkillMultiEffectExecutor.Execute(
                context,
                snapshot,
                SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects),
                center);
            var routed = outcome.Routed || multiEffectRouted;
            return new SkillExecutionResult(
                routed || outcome.CastCommitted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected,
                skill.SkillId,
                GetType().Name);
        }

        /*
         * 범위 중심점을 결정한다.
         */
        private static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            return SkillAreaUtility.ResolveAreaCenter(context, targeting, area);
        }

        /*
         * 반경을 결정한다.
         */
        private static float ResolveRadius(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillAreaUtility.ResolveRadius(SkillAreaUtility.ResolveBaseRadius(targeting, area), snapshot);
        }

        /*
         * 프리팹을 결정한다.
         */
        private static GameObject ResolvePrefab(SkillExecutionContext context, SkillExecutionSnapshot snapshot, SingleAttackSkillRuntimeData skill)
        {
            return snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : context.CombatManager.Effects != null
                    ? context.CombatManager.Effects.ResolveMonsterSkillEffectPrefab(context.Caster, skill.SkillId)
                    : null;
        }

        /*
         * 비주얼을 생성한다.
         */
        private static void SpawnVisual(
            SkillExecutionContext context,
            RuntimeSkillVisualSpec runtimeVisual,
            GameObject prefab,
            Vector2 center,
            float minimumLifetimeSeconds)
        {
            if (context.CombatManager.Effects == null)
            {
                return;
            }

            var instance = EffectVisualUtility.HasVisual(runtimeVisual)
                ? context.CombatManager.Effects.CreateRuntimeVisual(
                    runtimeVisual,
                    "RuntimeSingleAttackVisual",
                    center,
                    Quaternion.identity)
                : prefab != null
                    ? context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity)
                    : null;
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance, ResolveVisualLifetime(instance, minimumLifetimeSeconds));
            }
        }

        /*
         * 프리팹 히트박스 중심점을 결정한다.
         */
        private static Vector2 ResolvePrefabHitboxCenter(SkillExecutionContext context, Vector2 fallbackCenter, SingleAttackSkillRuntimeData skill)
        {
            if (skill != null
                && skill.HitAllTargets
                && !UsesStatusFilteredDeployments(skill))
            {
                return context != null && context.CasterEntry != null && context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : fallbackCenter;
            }

            return fallbackCenter;
        }

        /*
         * 배치 횟수를 결정한다.
         */
        private static int ResolveDeploymentCount(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot)
        {
            if (skill == null || !skill.UseMultiDeployment)
            {
                return 1;
            }

            var bonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            return Mathf.Max(1, skill.DeploymentCount + bonus);
        }

        /*
         * 상태 조건 선별 배치를 사용하는 구성인지 확인한다.
         */
        private static bool UsesStatusFilteredDeployments(SingleAttackSkillRuntimeData skill)
        {
            return skill != null && !string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId);
        }

        /*
         * 직선 형태 다중 배치 비주얼을 사용하는 구성인지 확인한다.
         */
        private static bool UsesLineStyleMultiDeploymentVisual(SingleAttackSkillRuntimeData skill)
        {
            return skill != null
                && skill.UseMultiDeployment
                && !UsesStatusFilteredDeployments(skill);
        }

        /*
         * 실제 적중 대상 횟수를 결정한다.
         */
        private static int ResolveEffectiveHitTargetCount(SingleAttackSkillRuntimeData skill, SkillExecutionSnapshot snapshot)
        {
            if (skill == null)
            {
                return 1;
            }

            if (UsesLineStyleMultiDeploymentVisual(skill)
                || skill.HitAllTargets
                || skill.HitTargetCount == int.MaxValue)
            {
                return int.MaxValue;
            }

            var hitTargetCountBonus = snapshot != null ? snapshot.HitTargetCountBonus : 0;
            return Mathf.Max(1, skill.HitTargetCount + hitTargetCountBonus);
        }

        /*
         * 결정된 배치를 사용하는 구성인지 확인한다.
         */
        private static bool UsesResolvedDeployments(SingleAttackSkillRuntimeData skill)
        {
            return skill != null
                && (skill.UseMultiDeployment || UsesStatusFilteredDeployments(skill));
        }

        /*
         * 배치 중심점을 결정한다.
         */
        private static List<Vector2> ResolveDeploymentCenters(
            SkillExecutionContext context,
            SingleAttackSkillRuntimeData skill,
            Vector2 primaryCenter,
            int deploymentCount)
        {
            if (UsesStatusFilteredDeployments(skill))
            {
                var requiredStacks = Mathf.Max(1, skill.DeploymentRequiredTargetStatusMinStacks);
                var filteredTargets = SkillExecutionUtility.ResolveOrderedTargets(
                    context != null ? context.CasterEntry : null,
                    context != null ? context.Roster : null,
                    skill.Targeting,
                    skill.DeploymentRequiredTargetStatusId,
                    requiredStacks);
                var centers = new List<Vector2>(filteredTargets.Count);
                for (var i = 0; i < filteredTargets.Count; i++)
                {
                    var target = filteredTargets[i];
                    if (target != null && target.Transform != null)
                    {
                        centers.Add((Vector2)target.Transform.position);
                    }
                }

                return centers;
            }

            var coverAll = (skill != null && skill.Area != null && skill.Area.CoverAll)
                || (skill != null && skill.Targeting != null && skill.Targeting.CoverAll);
            return SkillDeploymentCenterUtility.ResolveTargetAnchoredCenters(
                context,
                skill != null ? skill.Targeting : null,
                primaryCenter,
                deploymentCount,
                coverAll,
                SkillDeploymentRepeatMode.RepeatNearest);
        }

        /*
         * 결정된 배치를 실행한다.
         */
        private static SingleAttackExecutionOutcome ExecuteResolvedDeployments(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            Vector2 primaryCenter,
            RuntimeSkillVisualSpec runtimeVisual,
            GameObject prefab)
        {
            var deploymentCount = ResolveDeploymentCount(skill, snapshot);
            var centers = ResolveDeploymentCenters(context, skill, primaryCenter, deploymentCount);
            var routed = false;
            var castCommitted = false;
            for (var i = 0; i < centers.Count; i++)
            {
                var center = centers[i];
                var outcome = ExecuteAtCenter(context, snapshot, skill, center, runtimeVisual, prefab, true);
                routed = routed || outcome.Routed;
                castCommitted = castCommitted || outcome.CastCommitted;
                routed = SkillMultiEffectExecutor.ExecuteOnDeploymentCast(
                    context,
                    snapshot,
                    SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects),
                    center) || routed;
                ScheduleRepeatedDeployments(context, snapshot, skill, center, runtimeVisual, prefab);
            }

            return new SingleAttackExecutionOutcome(routed, castCommitted);
        }

        /*
         * 반복 배치를 예약한다.
         */
        private static void ScheduleRepeatedDeployments(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            Vector2 center,
            RuntimeSkillVisualSpec runtimeVisual,
            GameObject prefab)
        {
            if (context == null
                || context.CombatManager == null
                || skill == null
                || snapshot == null
                || snapshot.RepeatCountPerTarget <= 0)
            {
                return;
            }

            var repeatSnapshot = !Mathf.Approximately(snapshot.RepeatDamageMultiplier, 1f)
                ? CloneSnapshotWithDamageMultiplier(snapshot, snapshot.RepeatDamageMultiplier)
                : snapshot;
            for (var repeatIndex = 1; repeatIndex <= snapshot.RepeatCountPerTarget; repeatIndex++)
            {
                var delaySeconds = Mathf.Max(0f, snapshot.RepeatIntervalSeconds * repeatIndex);
                if (delaySeconds <= 0f)
                {
                    ExecuteAtCenter(context, repeatSnapshot, skill, center, runtimeVisual, prefab, false);
                    SkillMultiEffectExecutor.ExecuteOnDeploymentCast(
                        context,
                        repeatSnapshot,
                        SkillPlanActionDispatcher.ResolveEffects(repeatSnapshot, skill.MultiEffects),
                        center);
                    continue;
                }

                context.CombatManager.StartCoroutine(ExecuteRepeatedDeploymentAfterDelay(
                    context,
                    repeatSnapshot,
                    skill,
                    center,
                    runtimeVisual,
                    prefab,
                    delaySeconds));
            }
        }

        /*
         * 지정 간격 후 다음 반복 배치를 실행한다.
         */
        private static IEnumerator ExecuteRepeatedDeploymentAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            Vector2 center,
            RuntimeSkillVisualSpec runtimeVisual,
            GameObject prefab,
            float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));

            if (context == null
                || context.CombatManager == null
                || context.Roster == null
                || context.CasterEntry == null
                || context.Caster == null
                || skill == null)
            {
                yield break;
            }

            ExecuteAtCenter(context, snapshot, skill, center, runtimeVisual, prefab, false);
            SkillMultiEffectExecutor.ExecuteOnDeploymentCast(
                context,
                snapshot,
                SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects),
                center);
        }

        /*
         * 다중 배치 프리팹의 방향과 길이를 설정한다.
         */
        private static void ConfigureMultiDeploymentPrefabVisual(
            Transform transform,
            SkillExecutionContext context,
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            Vector2 center)
        {
            if (transform == null || skill == null)
            {
                return;
            }

            var origin = context != null && context.CasterEntry != null && context.CasterEntry.Transform != null
                ? (Vector2)context.CasterEntry.Transform.position
                : center;
            var direction = center - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            transform.position = center;
            transform.rotation = SkillExecutionUtility.ResolveRotation(direction.normalized);

            var width = ResolveRadius(skill, snapshot);
            var spriteRenderer = transform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var size = spriteRenderer.sprite.bounds.size;
                var scale = transform.localScale;
                if (size.x > 0.0001f)
                {
                    scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (DefaultMultiDeploymentLineLength / size.x);
                }

                if (size.y > 0.0001f)
                {
                    scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (width / size.y);
                }

                transform.localScale = scale;
                return;
            }

            SkillExecutionUtility.ApplyPrefabScale(transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
        }

        /*
         * 지정한 중심점에서 단일 공격 판정을 실행한다.
         */
        private static SingleAttackExecutionOutcome ExecuteAtCenter(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            Vector2 center,
            RuntimeSkillVisualSpec runtimeVisual,
            GameObject prefab,
            bool allowConditionalFollowUp)
        {
            var radius = ResolveRadius(skill, snapshot);
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot);
            var attribute = SkillExecutionUtility.MapAttribute(skill.Damage != null ? skill.Damage.Element : skill.Element);
            var statusSpec = SkillStatusSpecUtility.ResolveStatusSpec(skill.OnHitStatus, snapshot);
            var onHitStatusEffects = ResolveOnHitStatusEffects(
                context,
                snapshot,
                SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects));
            var critChanceBonus = snapshot != null ? snapshot.CritChanceBonus : 0f;
            var critDamageBonus = snapshot != null ? snapshot.CritDamageBonus : 0f;
            var effectiveHitTargetCount = ResolveEffectiveHitTargetCount(skill, snapshot);
            var damageDelaySeconds = Mathf.Max(0f, skill.DamageDelaySeconds);
            var followUpSpec = allowConditionalFollowUp ? ResolveFollowUpSpec(snapshot, statusSpec, prefab) : null;
            var followUpTargets = followUpSpec.HasValue ? new List<SingleAttackFollowUpTarget>() : null;
            var onHitRuntime = allowConditionalFollowUp ? context.Runtime : null;
            var spawnedHitbox = false;
            var routed = false;
            var castCommitted = false;

            var hasRuntimeVisual = EffectVisualUtility.HasVisual(runtimeVisual);
            // 히트박스가 있는 비주얼은 실제 콜라이더를 기준으로 피해 대상을 판정한다.
            if (skill.UsePrefabHitbox && (hasRuntimeVisual || prefab != null) && context.CombatManager.Effects != null)
            {
                center = ResolvePrefabHitboxCenter(context, center, skill);
                var instance = hasRuntimeVisual
                    ? context.CombatManager.Effects.CreateRuntimeVisual(
                        runtimeVisual,
                        "RuntimeSingleAttackHitbox",
                        center,
                        Quaternion.identity)
                    : context.CombatManager.Effects.InstantiateSkillPrefab(prefab, center, Quaternion.identity);
                if (instance != null)
                {
                    spawnedHitbox = true;
                    castCommitted = true;
                    if (UsesLineStyleMultiDeploymentVisual(skill))
                    {
                        ConfigureMultiDeploymentPrefabVisual(instance.transform, context, skill, snapshot, center);
                    }
                    else if (!hasRuntimeVisual)
                    {
                        SkillExecutionUtility.ApplyPrefabScale(instance.transform, SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
                    }
                    if (damageDelaySeconds > 0f)
                    {
                        context.CombatManager.StartCoroutine(ApplyPrefabHitboxAfterDelay(
                            context,
                            snapshot,
                            skill,
                            instance,
                            effectiveHitTargetCount,
                            damage,
                            attribute,
                            statusSpec,
                            onHitStatusEffects,
                            onHitRuntime,
                            skill.Damage != null && skill.Damage.CriticalAllowed,
                            critChanceBonus,
                            critDamageBonus,
                            followUpSpec,
                            followUpTargets,
                            damageDelaySeconds,
                            allowConditionalFollowUp));
                    }
                    else
                    {
                        Physics2D.SyncTransforms();
                        routed = ApplyPrefabHitbox(
                            context.CombatManager,
                            context.CasterEntry,
                            context.Roster,
                            skill,
                            skill.Targeting,
                            instance,
                            effectiveHitTargetCount,
                            damage,
                            attribute,
                            statusSpec,
                            onHitStatusEffects,
                            context.Caster,
                            skill.SkillId,
                            onHitRuntime,
                            skill.Damage != null && skill.Damage.CriticalAllowed,
                            critChanceBonus,
                            critDamageBonus,
                            snapshot,
                            followUpSpec,
                            followUpTargets);
                    }

                    UnityEngine.Object.Destroy(instance, ResolveVisualLifetime(instance, damageDelaySeconds + PostDamageLifetimePaddingSeconds));
                }
            }

            if (!spawnedHitbox)
            {
                // 히트박스를 만들지 못한 경우에는 설정된 중심점과 반경으로 대상을 직접 찾는다.
                castCommitted = true;
                if (damageDelaySeconds > 0f)
                {
                    SpawnVisual(context, runtimeVisual, prefab, center, damageDelaySeconds + PostDamageLifetimePaddingSeconds);
                    context.CombatManager.StartCoroutine(ApplyNonPrefabTargetsAfterDelay(
                        context,
                        snapshot,
                        skill,
                        center,
                        radius,
                        coverAll,
                        effectiveHitTargetCount,
                        damage,
                        attribute,
                        statusSpec,
                        onHitStatusEffects,
                        onHitRuntime,
                        skill.Damage != null && skill.Damage.CriticalAllowed,
                        critChanceBonus,
                        critDamageBonus,
                        followUpSpec,
                        followUpTargets,
                        damageDelaySeconds,
                        allowConditionalFollowUp));
                }
                else
                {
                    routed = ApplyNonPrefabTargets(
                        context,
                        snapshot,
                        skill,
                        center,
                        radius,
                        coverAll,
                        effectiveHitTargetCount,
                        damage,
                        attribute,
                        statusSpec,
                        onHitStatusEffects,
                        onHitRuntime,
                        skill.Damage != null && skill.Damage.CriticalAllowed,
                        critChanceBonus,
                        critDamageBonus,
                        followUpSpec,
                        followUpTargets);

                    if (routed)
                    {
                        SpawnVisual(context, runtimeVisual, prefab, center, PostDamageLifetimePaddingSeconds);
                    }
                }
            }

            if (allowConditionalFollowUp && damageDelaySeconds <= 0f)
            {
                ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
            }

            return new SingleAttackExecutionOutcome(routed, castCommitted);
        }

        /*
         * 비 프리팹 대상을 적용한다.
         */
        private static bool ApplyNonPrefabTargets(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            Vector2 center,
            float radius,
            bool coverAll,
            int effectiveHitTargetCount,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitStatusEffects,
            SkillRuntimeInstance onHitRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || skill == null)
            {
                return false;
            }

            if (skill.UsesHitTargetCount && !skill.HitAllTargets)
            {
                return ApplyLimitedTargets(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    skill,
                    skill.Targeting,
                    effectiveHitTargetCount,
                    damage,
                    attribute,
                    statusSpec,
                    onHitStatusEffects,
                    context.Caster,
                    skill.SkillId,
                    onHitRuntime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    snapshot,
                    center,
                    followUpSpec,
                    followUpTargets);
            }

            return ApplyAreaTargets(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill,
                skill.Targeting,
                center,
                radius,
                coverAll,
                damage,
                attribute,
                statusSpec,
                onHitStatusEffects,
                context.Caster,
                skill.SkillId,
                onHitRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                snapshot,
                followUpSpec,
                followUpTargets);
        }

        /*
         * 지연시간 후 좌표 기반 대상에게 피해를 적용한다.
         */
        private static IEnumerator ApplyNonPrefabTargetsAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            Vector2 center,
            float radius,
            bool coverAll,
            int effectiveHitTargetCount,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitStatusEffects,
            SkillRuntimeInstance onHitRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets,
            float delaySeconds,
            bool allowConditionalFollowUp)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));

            ApplyNonPrefabTargets(
                context,
                snapshot,
                skill,
                center,
                radius,
                coverAll,
                effectiveHitTargetCount,
                damage,
                attribute,
                statusSpec,
                onHitStatusEffects,
                onHitRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                followUpSpec,
                followUpTargets);

            if (allowConditionalFollowUp)
            {
                ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
            }
        }

        /*
         * 지연시간 후 프리팹 히트박스의 대상에게 피해를 적용한다.
         */
        private static IEnumerator ApplyPrefabHitboxAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            GameObject instance,
            int effectiveHitTargetCount,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitStatusEffects,
            SkillRuntimeInstance onHitRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets,
            float delaySeconds,
            bool allowConditionalFollowUp)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));

            if (context == null
                || context.CombatManager == null
                || context.CasterEntry == null
                || context.Roster == null
                || skill == null
                || instance == null)
            {
                yield break;
            }

            Physics2D.SyncTransforms();
            ApplyPrefabHitbox(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                skill,
                skill.Targeting,
                instance,
                effectiveHitTargetCount,
                damage,
                attribute,
                statusSpec,
                onHitStatusEffects,
                context.Caster,
                skill.SkillId,
                onHitRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                snapshot,
                followUpSpec,
                followUpTargets);

            if (allowConditionalFollowUp)
            {
                ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
            }
        }

        /*
         * 프리팹 히트박스를 적용한다.
         */
        private static bool ApplyPrefabHitbox(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SingleAttackSkillRuntimeData skill,
            SkillTargetingSpec targetingSpec,
            GameObject hitboxObject,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitStatusEffects,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot snapshot,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || hitboxObject == null || maxTargets <= 0)
            {
                return false;
            }

            var hitboxColliders = hitboxObject.GetComponentsInChildren<Collider2D>();
            if (hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var coreHitboxColliders = ResolveCoreHitboxColliders(hitboxObject, snapshot);
            var targets = SkillExecutionUtility.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!IsTargetInsideHitbox(hitboxColliders, target))
                {
                    continue;
                }

                RegisterFollowUpTarget(
                    followUpTargets,
                    followUpSpec,
                    target,
                    target != null && target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
                var isCoreHit = coreHitboxColliders.Length > 0 && IsTargetInsideHitbox(coreHitboxColliders, target);
                var targetDamage = ResolveTargetDamage(source, skill, snapshot, damage, target.Model, critChanceBonus, isCoreHit);
                var result = manager.ApplyDamage(target.Model, targetDamage.Damage, attribute, source, criticalAllowed, targetDamage.CritChanceBonus, critDamageBonus, sourceSkillId, false, targetDamage.IsExecute);
                var consumedStacks = ConsumePlannedTargetStatusStacks(manager, target.Model, skill, targetDamage);
                SingleAttackSkillRuleHandlers.HandleKillRecovery(sourceRuntime, skill, snapshot, result, targetDamage.IsExecute);
                TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, target, result, consumedStacks);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                TryApplyOnHitStatusEffects(manager, target.Model, onHitStatusEffects, source);
                TryApplyCoreOnHitAdditionalDamage(manager, snapshot, source, sourceSkillId, target, targetDamage.Damage, isCoreHit);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    targetDamage.Damage);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, hitCount);
            TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, hitCount, (Vector2)hitboxObject.transform.position);
            return routed;
        }

        /*
         * 제한된 대상을 적용한다.
         */
        private static bool ApplyLimitedTargets(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SingleAttackSkillRuntimeData skill,
            SkillTargetingSpec targetingSpec,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitStatusEffects,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot snapshot,
            Vector2 center,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (manager == null || sourceEntry == null || unitRoster == null || maxTargets <= 0)
            {
                return false;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : center;
                var targetDamage = ResolveTargetDamage(source, skill, snapshot, damage, target.Model, critChanceBonus, false);
                var result = manager.ApplyDamage(target.Model, targetDamage.Damage, attribute, source, criticalAllowed, targetDamage.CritChanceBonus, critDamageBonus, sourceSkillId, false, targetDamage.IsExecute);
                var consumedStacks = ConsumePlannedTargetStatusStacks(manager, target.Model, skill, targetDamage);
                SingleAttackSkillRuleHandlers.HandleKillRecovery(sourceRuntime, skill, snapshot, result, targetDamage.IsExecute);
                TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, target, result, consumedStacks);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                TryApplyOnHitStatusEffects(manager, target.Model, onHitStatusEffects, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    targetDamage.Damage);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, hitCount);
            TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, hitCount, center);
            return routed;
        }

        /*
         * 범위 대상을 적용한다.
         */
        private static bool ApplyAreaTargets(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService unitRoster,
            SingleAttackSkillRuntimeData skill,
            SkillTargetingSpec targetingSpec,
            Vector2 center,
            float radius,
            bool coverAll,
            float damage,
            DamageAttribute attribute,
            ProjectileStatusHitSpec statusSpec,
            SkillEffectDefinition[] onHitStatusEffects,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillRuntimeInstance sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionSnapshot snapshot,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
            if (!coverAll && radius <= 0f)
            {
                var target = targets.Count > 0 ? targets[0] : null;
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : center;
                var targetDamage = ResolveTargetDamage(source, skill, snapshot, damage, target.Model, critChanceBonus, false);
                var result = manager.ApplyDamage(target.Model, targetDamage.Damage, attribute, source, criticalAllowed, targetDamage.CritChanceBonus, critDamageBonus, sourceSkillId, false, targetDamage.IsExecute);
                var consumedStacks = ConsumePlannedTargetStatusStacks(manager, target.Model, skill, targetDamage);
                SingleAttackSkillRuleHandlers.HandleKillRecovery(sourceRuntime, skill, snapshot, result, targetDamage.IsExecute);
                TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, target, result, consumedStacks);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                TryApplyOnHitStatusEffects(manager, target.Model, onHitStatusEffects, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    targetDamage.Damage);
                TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, 1);
                TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, 1, center);
                return true;
            }

            var routed = false;
            var hitCount = 0;
            var radiusSq = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                if (!coverAll)
                {
                    var offset = (Vector2)target.Transform.position - center;
                    if (offset.sqrMagnitude > radiusSq)
                    {
                        continue;
                    }
                }

                RegisterFollowUpTarget(followUpTargets, followUpSpec, target, center);
                var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : center;
                var targetDamage = ResolveTargetDamage(source, skill, snapshot, damage, target.Model, critChanceBonus, false);
                var result = manager.ApplyDamage(target.Model, targetDamage.Damage, attribute, source, criticalAllowed, targetDamage.CritChanceBonus, critDamageBonus, sourceSkillId, false, targetDamage.IsExecute);
                var consumedStacks = ConsumePlannedTargetStatusStacks(manager, target.Model, skill, targetDamage);
                SingleAttackSkillRuleHandlers.HandleKillRecovery(sourceRuntime, skill, snapshot, result, targetDamage.IsExecute);
                TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, target, result, consumedStacks);
                TryApplyStatus(manager, target.Model, statusSpec, source);
                TryApplyOnHitStatusEffects(manager, target.Model, onHitStatusEffects, source);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    manager,
                    unitRoster,
                    sourceRuntime,
                    snapshot,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    targetDamage.Damage);
                routed = true;
                hitCount++;
            }

            TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, hitCount);
            TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, hitCount, center);
            return routed;
        }

        /*
         * 대상이 현재 히트박스 안에 있는지 확인한다.
         */
        private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, UnitRosterEntry target)
        {
            return UnitHitboxUtility.IsTargetInsideHitbox(hitboxColliders, target);
        }

        /*
         * 핵심 히트박스 콜라이더를 결정한다.
         */
        private static Collider2D[] ResolveCoreHitboxColliders(GameObject hitboxObject, SkillExecutionSnapshot snapshot)
        {
            if (hitboxObject == null || snapshot == null || string.IsNullOrWhiteSpace(snapshot.CoreHitboxName))
            {
                return Array.Empty<Collider2D>();
            }

            var result = new List<Collider2D>();
            var transforms = hitboxObject.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var current = transforms[i];
                if (current == null || !string.Equals(current.name, snapshot.CoreHitboxName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var colliders = current.GetComponentsInChildren<Collider2D>(true);
                if (colliders != null && colliders.Length > 0)
                {
                    result.AddRange(colliders);
                }
            }

            return result.Count > 0 ? result.ToArray() : Array.Empty<Collider2D>();
        }

        /*
         * 적중 상태 효과를 결정한다.
         */
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

        /*
         * 적중 상태 효과를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyOnHitStatusEffects(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            SkillEffectDefinition[] effects,
            BaseUnitRuntimeModel source)
        {
            if (manager == null || target == null || effects == null || effects.Length == 0)
            {
                return;
            }

            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect == null || !SkillMultiEffectExecutor.TargetMatchesCondition(target, effect))
                {
                    continue;
                }

                var status = SkillMultiEffectExecutor.ResolveStatusSpec(effect);
                if (status == null || !status.Enabled)
                {
                    continue;
                }

                SkillStatusApplyUtility.TryApplyStatus(manager, target, status, source);
            }
        }

        /*
         * 핵심 적중 추가 피해를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyCoreOnHitAdditionalDamage(
            InGameCombatManager manager,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            UnitRosterEntry target,
            float primaryDamage,
            bool isCoreHit)
        {
            if (!isCoreHit
                || manager == null
                || snapshot == null
                || !snapshot.HasCoreOnHitAdditionalDamage
                || snapshot.CoreOnHitAdditionalDamageMultiplier <= 0f
                || source == null
                || target == null
                || !target.IsAlive
                || target.Model == null
                || primaryDamage <= 0f
                || UnityEngine.Random.value > Mathf.Clamp01(snapshot.CoreOnHitAdditionalDamageChance))
            {
                return;
            }

            manager.ApplyDamage(
                target.Model,
                primaryDamage * snapshot.CoreOnHitAdditionalDamageMultiplier,
                snapshot.CoreOnHitAdditionalDamageAttribute,
                source,
                false,
                0f,
                0f,
                sourceSkillId,
                true);
        }

        /*
         * 적중 횟수 재사용 대기시간 반환을 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyHitCountCooldownRefund(
            SkillRuntimeInstance sourceRuntime,
            SkillExecutionSnapshot snapshot,
            int hitCount)
        {
            if (sourceRuntime == null
                || sourceRuntime.Owner == null
                || sourceRuntime.Owner.SkillRuntime == null
                || snapshot == null
                || hitCount < snapshot.HitCountCooldownRefundMinTargets
                || string.IsNullOrWhiteSpace(snapshot.HitCountCooldownRefundTargetSkillId)
                || snapshot.HitCountCooldownRefundRatio <= 0f)
            {
                return;
            }

            var targetRuntime = sourceRuntime.Owner.SkillRuntime.FindBySkillId(snapshot.HitCountCooldownRefundTargetSkillId);
            if (targetRuntime == null)
            {
                return;
            }

            targetRuntime.ReduceCooldownRemaining(targetRuntime.EffectiveCooldownDuration * Mathf.Clamp01(snapshot.HitCountCooldownRefundRatio));
        }

        /*
         * 적중 횟수 효과를 실행하고 성공 여부를 반환한다.
         */
        private static void TryExecuteOnHitCountEffects(
            InGameCombatManager manager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            SkillRuntimeInstance sourceRuntime,
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            int hitCount,
            Vector2 center)
        {
            if (manager == null
                || roster == null
                || sourceEntry == null
                || skill == null
                || hitCount <= 0)
            {
                return;
            }

            var context = new SkillExecutionContext(manager, roster, sourceEntry, sourceRuntime, 0f);
            SkillMultiEffectExecutor.ExecuteOnHitCount(
                context,
                snapshot,
                SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects),
                center,
                hitCount);
        }

        /*
         * 비주얼 수명을 결정한다.
         */
        private static float ResolveVisualLifetime(GameObject instance, float minimumLifetimeSeconds)
        {
            var minimum = Mathf.Max(0.01f, minimumLifetimeSeconds);
            var animationLength = ResolveAnimationLength(instance);
            return Mathf.Max(minimum, animationLength > 0f ? animationLength : DefaultVisualLifetimeSeconds);
        }

        /*
         * 애니메이션 길이를 결정한다.
         */
        private static float ResolveAnimationLength(GameObject instance)
        {
            if (instance == null)
            {
                return 0f;
            }

            var maxLength = 0f;
            var animators = instance.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var controller = animators[i] != null ? animators[i].runtimeAnimatorController : null;
                var clips = controller != null ? controller.animationClips : null;
                if (clips == null)
                {
                    continue;
                }

                for (var j = 0; j < clips.Length; j++)
                {
                    var clip = clips[j];
                    if (clip != null)
                    {
                        maxLength = Mathf.Max(maxLength, clip.length);
                    }
                }
            }

            var legacyAnimations = instance.GetComponentsInChildren<UnityEngine.Animation>(true);
            for (var i = 0; i < legacyAnimations.Length; i++)
            {
                var legacyAnimation = legacyAnimations[i];
                if (legacyAnimation == null)
                {
                    continue;
                }

                foreach (AnimationState state in legacyAnimation)
                {
                    if (state != null)
                    {
                        maxLength = Mathf.Max(maxLength, state.length);
                    }
                }
            }

            return maxLength;
        }

        /*
         * 후속 설정을 결정한다.
         */
        private static SingleAttackFollowUpSpec? ResolveFollowUpSpec(
            SkillExecutionSnapshot snapshot,
            ProjectileStatusHitSpec statusSpec,
            GameObject prefab)
        {
            if (snapshot == null
                || !snapshot.HasBranchCount
                || snapshot.BranchCount <= 0
                || !snapshot.HasBranchDamageMultiplier
                || snapshot.BranchDamageMultiplier <= 0f
                || !snapshot.HasBranchSearchRadius
                || snapshot.BranchSearchRadius <= 0f)
            {
                return null;
            }

            var requiredStatusId = !string.IsNullOrWhiteSpace(snapshot.StatusTag)
                ? snapshot.StatusTag
                : statusSpec != null && statusSpec.StatusData != null
                    ? statusSpec.StatusData.StatusTag
                    : statusSpec != null
                        ? StatusEffectUtility.ToId(statusSpec.Kind)
                        : string.Empty;
            if (string.IsNullOrWhiteSpace(requiredStatusId))
            {
                return null;
            }

            return new SingleAttackFollowUpSpec(
                requiredStatusId,
                snapshot.BranchCount,
                snapshot.BranchSearchRadius,
                snapshot.BranchDamageMultiplier,
                prefab);
        }

        /*
         * 후속 대상을 등록한다.
         */
        private static void RegisterFollowUpTarget(
            List<SingleAttackFollowUpTarget> followUpTargets,
            SingleAttackFollowUpSpec? followUpSpec,
            UnitRosterEntry target,
            Vector2 center)
        {
            if (followUpTargets == null
                || !followUpSpec.HasValue
                || target == null
                || target.Model == null
                || !HasStatus(target.Model, followUpSpec.Value.RequiredStatusId))
            {
                return;
            }

            for (var i = 0; i < followUpTargets.Count; i++)
            {
                if (ReferenceEquals(followUpTargets[i].Model, target.Model))
                {
                    return;
                }
            }

            followUpTargets.Add(new SingleAttackFollowUpTarget(target.Model, center));
        }

        /*
         * 조건부 후속을 예약한다.
         */
        private static void ScheduleConditionalFollowUps(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            SingleAttackFollowUpSpec? followUpSpec,
            List<SingleAttackFollowUpTarget> followUpTargets)
        {
            if (context == null
                || context.CombatManager == null
                || context.Roster == null
                || context.CasterEntry == null
                || context.Caster == null
                || skill == null
                || !followUpSpec.HasValue
                || followUpTargets == null
                || followUpTargets.Count == 0)
            {
                return;
            }

            var spec = followUpSpec.Value;
            for (var i = 0; i < followUpTargets.Count; i++)
            {
                var followUpTarget = followUpTargets[i];
                for (var repeatIndex = 1; repeatIndex <= spec.RepeatCount; repeatIndex++)
                {
                    context.CombatManager.StartCoroutine(ExecuteConditionalFollowUpAfterDelay(
                        context,
                        snapshot,
                        skill,
                        followUpTarget,
                        spec,
                        spec.IntervalSeconds * repeatIndex));
                }
            }
        }

        /*
         * 지연시간 후 조건부 후속 공격을 실행한다.
         */
        private static IEnumerator ExecuteConditionalFollowUpAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleAttackSkillRuntimeData skill,
            SingleAttackFollowUpTarget followUpTarget,
            SingleAttackFollowUpSpec followUpSpec,
            float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));

            if (context == null
                || context.CombatManager == null
                || context.Roster == null
                || context.CasterEntry == null
                || context.Caster == null
                || skill == null)
            {
                yield break;
            }

            var liveTarget = followUpTarget.Model != null
                ? context.Roster.Find(followUpTarget.Model)
                : null;
            var center = liveTarget != null && liveTarget.Transform != null
                ? (Vector2)liveTarget.Transform.position
                : followUpTarget.Center;
            var followUpSnapshot = snapshot != null ? CloneSnapshotWithDamageMultiplier(snapshot, followUpSpec.DamageMultiplier) : null;
            ExecuteAtCenter(context, followUpSnapshot, skill, center, null, followUpSpec.Prefab, false);
        }

        /*
         * 실행 정보 포함 피해 배율을 복사본을 생성한다.
         */
        private static SkillExecutionSnapshot CloneSnapshotWithDamageMultiplier(
            SkillExecutionSnapshot snapshot,
            float damageMultiplier)
        {
            if (snapshot == null)
            {
                return null;
            }

            var clone = new SkillExecutionSnapshot(snapshot.Source);
            clone.ApplyChoiceSpec(new SkillChoiceRuntimeData
            {
                HasDamageMultiplier = true,
                DamageMultiplier = snapshot.DamageMultiplier * Mathf.Max(0f, damageMultiplier),
                BaseDamageBonus = snapshot.BaseDamageBonus,
                HasCooldownMultiplier = true,
                CooldownMultiplier = snapshot.CooldownMultiplier,
                HasRadiusMultiplier = true,
                RadiusMultiplier = snapshot.RadiusMultiplier,
                RadiusBonus = snapshot.RadiusBonus,
                HasDurationMultiplier = true,
                DurationMultiplier = snapshot.DurationMultiplier,
                DurationBonus = snapshot.DurationBonus,
                HasReloadTimeMultiplier = true,
                ReloadTimeMultiplier = snapshot.ReloadTimeMultiplier,
                HasShotIntervalMultiplier = true,
                ShotIntervalMultiplier = snapshot.ShotIntervalMultiplier,
                BranchChanceBonus = snapshot.BranchChanceBonus,
                HasBranchChanceSet = snapshot.HasBranchChanceSet,
                BranchChanceSet = snapshot.BranchChanceSet,
                HasBranchCount = snapshot.HasBranchCount,
                BranchCount = snapshot.BranchCount,
                HasBranchDamageMultiplier = snapshot.HasBranchDamageMultiplier,
                BranchDamageMultiplier = snapshot.BranchDamageMultiplier,
                HasBranchSearchRadius = snapshot.HasBranchSearchRadius,
                BranchSearchRadius = snapshot.BranchSearchRadius,
                HitTargetCountBonus = snapshot.HitTargetCountBonus,
                CritChanceBonus = snapshot.CritChanceBonus,
                CritDamageBonus = snapshot.CritDamageBonus,
                ExecuteCritChanceBonus = snapshot.ExecuteCritChanceBonus,
                HasBossDamageMultiplier = !Mathf.Approximately(snapshot.BossDamageMultiplier, 1f),
                BossDamageMultiplier = snapshot.BossDamageMultiplier,
                HasKillCooldownRefundRatioBonus = !Mathf.Approximately(snapshot.KillCooldownRefundRatioBonus, 0f),
                KillCooldownRefundRatioBonus = snapshot.KillCooldownRefundRatioBonus,
                KillResetsCooldown = snapshot.KillResetsCooldown,
                KillResetsCooldownRequiresExecute = snapshot.KillResetsCooldownRequiresExecute,
                StatusTag = snapshot.StatusTag,
                HasStatusChanceBonus = !Mathf.Approximately(snapshot.StatusChanceBonus, 0f),
                StatusChanceBonus = snapshot.StatusChanceBonus,
                HasStatusActionSpeedBonus = snapshot.HasStatusActionSpeedBonus,
                StatusActionSpeedBonus = snapshot.StatusActionSpeedBonus,
                HasStatusAttackPowerBonus = snapshot.HasStatusAttackPowerBonus,
                StatusAttackPowerBonus = snapshot.StatusAttackPowerBonus,
                StatusStacksBonus = snapshot.StatusStacksBonus,
                HasStatusStacksSet = snapshot.HasStatusStacksSet,
                StatusStacksSet = snapshot.StatusStacksSet,
                HasStatusElementDamageTakenBonus = snapshot.HasStatusElementDamageTakenBonus,
                StatusElementDamageTakenBonus = snapshot.StatusElementDamageTakenBonus,
                HasStatusCriticalDamageTakenBonus = snapshot.HasStatusCriticalDamageTakenBonus,
                StatusCriticalDamageTakenBonus = snapshot.StatusCriticalDamageTakenBonus,
                HasStatusAilmentResistanceBonus = snapshot.HasStatusAilmentResistanceBonus,
                StatusAilmentResistanceBonus = snapshot.StatusAilmentResistanceBonus,
                HasStatusDamageBonusRate = snapshot.HasStatusDamageBonusRate,
                StatusDamageBonusRate = snapshot.StatusDamageBonusRate,
                HasStatusShieldReceivedBonus = snapshot.HasStatusShieldReceivedBonus,
                StatusShieldReceivedBonus = snapshot.StatusShieldReceivedBonus,
                HasStatusCriticalChanceBonus = snapshot.HasStatusCriticalChanceBonus,
                StatusCriticalChanceBonus = snapshot.StatusCriticalChanceBonus,
                HasStatusDamageTakenBonus = snapshot.HasStatusDamageTakenBonus,
                StatusDamageTakenBonus = snapshot.StatusDamageTakenBonus,
                HasStatusFlatElementResistReduction = snapshot.HasStatusFlatElementResistReduction,
                StatusFlatElementResistReduction = snapshot.StatusFlatElementResistReduction,
                ThresholdStatusId = snapshot.ThresholdStatusId,
                ThresholdStatusMinStacks = snapshot.ThresholdStatusMinStacks,
                ThresholdApplyStatusId = snapshot.ThresholdApplyStatusId,
                HasTargetStatusStackDamageMultiplier = !Mathf.Approximately(snapshot.TargetStatusStackDamageMultiplier, 1f),
                TargetStatusStackDamageMultiplier = snapshot.TargetStatusStackDamageMultiplier,
                HasConsumeTargetStatusRatioOverride = snapshot.HasConsumeTargetStatusRatioOverride,
                ConsumeTargetStatusRatioOverride = snapshot.ConsumeTargetStatusRatioOverride,
                HasConsumeTargetStatusStacksOverride = snapshot.HasConsumeTargetStatusStacksOverride,
                ConsumeTargetStatusStacksOverride = snapshot.ConsumeTargetStatusStacksOverride,
                HasExecuteHealthRatioBonus = !Mathf.Approximately(snapshot.ExecuteHealthRatioBonus, 0f),
                ExecuteHealthRatioBonus = snapshot.ExecuteHealthRatioBonus,
                SkillEffectPrefab = snapshot.SkillEffectPrefab,
                HasStatusConditionalDamageTakenBonus = snapshot.HasStatusConditionalDamageTakenBonus,
                StatusConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus,
                StatusConditionalSourceStatusId = snapshot.StatusConditionalSourceStatusId,
                RedistributeConsumedStatusRatioOnKill = snapshot.RedistributeConsumedStatusRatioOnKill,
                RedistributeConsumedStatusId = snapshot.RedistributeConsumedStatusId,
                RedistributeConsumedStatusSearchRadius = snapshot.RedistributeConsumedStatusSearchRadius,
                RedistributeConsumedStatusTargetCount = snapshot.RedistributeConsumedStatusTargetCount,
                RepeatCountPerTarget = snapshot.RepeatCountPerTarget,
                RepeatIntervalSeconds = snapshot.RepeatIntervalSeconds,
                RepeatDamageMultiplier = snapshot.RepeatDamageMultiplier
            });
            return clone;
        }

        /*
         * 대상 피해를 결정한다.
         */
        private static TargetDamageResolution ResolveTargetDamage(
            BaseUnitRuntimeModel caster,
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            float baseDamage,
            BaseUnitRuntimeModel target,
            float baseCritChanceBonus,
            bool isCoreHit)
        {
            var totalDamage = Mathf.Max(0f, baseDamage + ResolveTargetStatusStackAdditionalDamage(caster, skill, snapshot, target, baseDamage));
            var damageMultiplier = snapshot != null ? snapshot.ResolveConditionalDamageMultiplier(target) : 1f;
            var critChanceBonus = baseCritChanceBonus + (snapshot != null ? snapshot.ResolveConditionalCritChanceBonus(target) : 0f);
            var isExecute = false;
            var plannedConsumedStacks = ResolvePlannedConsumedStacks(skill, snapshot, target);

            if (isCoreHit && snapshot != null && snapshot.HasCoreDamageMultiplier)
            {
                damageMultiplier *= snapshot.CoreDamageMultiplier;
            }

            var modifierState = SingleAttackSkillRuleHandlers.ApplyDamageModifiers(skill, snapshot, target, damageMultiplier, critChanceBonus);
            damageMultiplier = modifierState.DamageMultiplier;
            critChanceBonus = modifierState.CritChanceBonus;
            isExecute = modifierState.IsExecute;

            return new TargetDamageResolution(
                Mathf.Max(0f, totalDamage * Mathf.Max(0f, damageMultiplier)),
                critChanceBonus,
                isExecute,
                plannedConsumedStacks);
        }

        /*
         * 대상 상태 중첩 추가 피해를 결정한다.
         */
        private static float ResolveTargetStatusStackAdditionalDamage(
            BaseUnitRuntimeModel caster,
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel target,
            float baseDamage)
        {
            if (caster == null
                || skill == null
                || target == null
                || skill.TargetStatusStackDamage == null
                || string.IsNullOrWhiteSpace(skill.TargetStatusStackStatusId))
            {
                return 0f;
            }

            var stacks = ResolveStatusStacks(target, skill.TargetStatusStackStatusId);
            if (stacks <= 0)
            {
                return 0f;
            }

            if (skill.TargetStatusStackMaxStacks > 0)
            {
                stacks = Mathf.Min(stacks, skill.TargetStatusStackMaxStacks);
            }

            var perStackDamage = SkillExecutionUtility.ResolveDamage(caster, skill.TargetStatusStackDamage, snapshot);
            var stackMultiplier = snapshot != null ? snapshot.TargetStatusStackDamageMultiplier : 1f;
            var rateBonus = snapshot != null
                ? snapshot.ResolveTargetStatusStackDamageRateBonus(skill.TargetStatusStackStatusId)
                : 0f;
            var perStackTotal = perStackDamage * Mathf.Max(0f, stackMultiplier)
                + Mathf.Max(0f, baseDamage) * rateBonus;
            return Mathf.Max(0f, stacks * perStackTotal);
        }

        /*
         * 계획된 소모할 중첩을 결정한다.
         */
        private static int ResolvePlannedConsumedStacks(
            SingleAttackSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel target)
        {
            if (skill == null
                || target == null
                || string.IsNullOrWhiteSpace(skill.ConsumeTargetStatusId))
            {
                return 0;
            }

            var currentStacks = ResolveStatusStacks(target, skill.ConsumeTargetStatusId);
            if (currentStacks <= 0)
            {
                return 0;
            }

            if (snapshot != null && snapshot.HasConsumeTargetStatusStacksOverride)
            {
                return Mathf.Clamp(snapshot.ConsumeTargetStatusStacksOverride, 0, currentStacks);
            }

            if (skill.ConsumeTargetStatusStacks > 0)
            {
                return Mathf.Clamp(skill.ConsumeTargetStatusStacks, 0, currentStacks);
            }

            var ratio = snapshot != null && snapshot.HasConsumeTargetStatusRatioOverride
                ? snapshot.ConsumeTargetStatusRatioOverride
                : skill.ConsumeTargetStatusRatio;
            if (ratio <= 0f)
            {
                return 0;
            }

            return Mathf.Clamp(Mathf.FloorToInt(currentStacks * Mathf.Clamp01(ratio)), 0, currentStacks);
        }

        /*
         * 계획된 대상 상태 중첩을 소모하고 적용 결과를 반환한다.
         */
        private static int ConsumePlannedTargetStatusStacks(
            InGameCombatManager manager,
            BaseUnitRuntimeModel target,
            SingleAttackSkillRuntimeData skill,
            TargetDamageResolution damageResolution)
        {
            if (manager == null
                || target == null
                || skill == null
                || damageResolution.PlannedConsumedStacks <= 0
                || string.IsNullOrWhiteSpace(skill.ConsumeTargetStatusId))
            {
                return 0;
            }

            return manager.ConsumeStatusStacks(target, skill.ConsumeTargetStatusId, damageResolution.PlannedConsumedStacks);
        }

        /*
         * 소모할 상태 처치를 재분배하고 성공 여부를 반환한다.
         */
        private static void TryRedistributeConsumedStatusOnKill(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            SkillExecutionSnapshot snapshot,
            UnitRosterEntry defeatedTarget,
            InGameResourceChangeResult result,
            int consumedStacks)
        {
            if (manager == null
                || sourceEntry == null
                || roster == null
                || source == null
                || snapshot == null
                || defeatedTarget == null
                || defeatedTarget.Transform == null
                || !result.IsDead
                || consumedStacks <= 0
                || snapshot.RedistributeConsumedStatusRatioOnKill <= 0f
                || string.IsNullOrWhiteSpace(snapshot.RedistributeConsumedStatusId)
                || snapshot.RedistributeConsumedStatusSearchRadius <= 0f)
            {
                return;
            }

            var totalRedistributedStacks = Mathf.FloorToInt(consumedStacks * Mathf.Clamp01(snapshot.RedistributeConsumedStatusRatioOnKill));
            if (totalRedistributedStacks <= 0)
            {
                return;
            }

            var targets = ResolveRedistributionTargets(
                sourceEntry,
                roster,
                defeatedTarget.Transform.position,
                snapshot.RedistributeConsumedStatusSearchRadius,
                defeatedTarget.Model,
                snapshot.RedistributeConsumedStatusTargetCount);
            if (targets.Count <= 0)
            {
                return;
            }

            var baseShare = totalRedistributedStacks / targets.Count;
            var remainder = totalRedistributedStacks % targets.Count;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var stacks = baseShare + (i < remainder ? 1 : 0);
                if (target == null || target.Model == null || stacks <= 0)
                {
                    continue;
                }

                var statusSpec = SkillStatusSpecUtility.CreateDirectStatusSpec(snapshot.RedistributeConsumedStatusId, stacks, snapshot);
                if (statusSpec != null)
                {
                    SkillStatusApplyUtility.TryApplyStatus(manager, target.Model, statusSpec, source);
                }
            }
        }

        /*
         * 재분배 대상을 결정한다.
         */
        private static List<UnitRosterEntry> ResolveRedistributionTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            Vector2 center,
            float radius,
            BaseUnitRuntimeModel excludedModel,
            int maxTargetCount)
        {
            var result = new List<UnitRosterEntry>();
            if (sourceEntry == null || roster == null || radius <= 0f)
            {
                return result;
            }

            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, roster, new SkillTargetingSpec
            {
                TargetSide = SkillTargetSide.Enemy,
                Selection = SkillTargetSelection.Nearest,
                Shape = SkillTargetShape.Circle,
                Radius = radius
            });
            var radiusSq = radius * radius;
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null
                    || !candidate.IsAlive
                    || candidate.Model == null
                    || candidate.Transform == null
                    || ReferenceEquals(candidate.Model, excludedModel))
                {
                    continue;
                }

                var offset = (Vector2)candidate.Transform.position - center;
                if (offset.sqrMagnitude > radiusSq)
                {
                    continue;
                }

                result.Add(candidate);
            }

            result.Sort((left, right) =>
            {
                var leftDistance = left != null && left.Transform != null ? ((Vector2)left.Transform.position - center).sqrMagnitude : float.MaxValue;
                var rightDistance = right != null && right.Transform != null ? ((Vector2)right.Transform.position - center).sqrMagnitude : float.MaxValue;
                return leftDistance.CompareTo(rightDistance);
            });

            if (maxTargetCount > 0 && result.Count > maxTargetCount)
            {
                result.RemoveRange(maxTargetCount, result.Count - maxTargetCount);
            }

            return result;
        }

        /*
         * 상태 중첩을 결정한다.
         */
        private static int ResolveStatusStacks(BaseUnitRuntimeModel target, string statusId)
        {
            if (target == null || string.IsNullOrWhiteSpace(statusId))
            {
                return 0;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return 0;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f ? 1 : 0;
            }

            return target.Statuses != null ? target.Statuses.GetStacks(kind) : 0;
        }

        /*
         * 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasStatus(BaseUnitRuntimeModel target, string statusId)
        {
            return target != null
                && target.Statuses != null
                && !string.IsNullOrWhiteSpace(statusId)
                && target.Statuses.Has(statusId);
        }

        /*
         * 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyStatus(InGameCombatManager manager, BaseUnitRuntimeModel target, ProjectileStatusHitSpec statusSpec, BaseUnitRuntimeModel source)
        {
            SkillStatusApplyUtility.TryApplyStatus(manager, target, statusSpec, source);
        }
    }
}


