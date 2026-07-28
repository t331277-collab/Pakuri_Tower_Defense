using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 지속 범위 공격을 준비하고 생성한 오브젝트의 처리를 ZoneSkillActor에 맡긴다.
 */
namespace Pakuri.InGame
{

    internal static class ZoneSkillExecutor
    {
        // 범위 중심, 반지름, 배치 수, 지속시간을 조립하고 Actor 생성을 구현.
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

        /*
         * 노드 피해 효과를 대상, 범위 또는 지속 영역에 적용한다.
         */
        internal static bool ApplyAdditionalDamageEffect(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            Vector2 defaultCenter /* 기본 효과 중심 */)
        {
            var targeting = SkillTargeting.BuildEffectTargeting(effect);
            var center = SkillTargeting.EffectCenter(context, effect, targeting, defaultCenter);
            var damageSpec = new SkillDamageSpec
            {
                SkillId = effect.SkillId,
                Element = effect.Attribute,
                BaseDamage = effect.BaseDamage,
                AttackPowerCoefficient = effect.AttackPowerCoefficient,
                SpellPowerCoefficient = effect.SpellPowerCoefficient,
                CriticalAllowed = true
            };
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, damageSpec);
            var effectSkillData = skillData.CopyWithDamageMultiplier(effect.DamageMultiplier);
            var statusSpec = SkillStatus.EffectStatusSpec(effect, skillData);

            if (effect.ActiveDurationSeconds > 0f && effect.TickIntervalSeconds > 0f)
            {
                return CreateAdditionalDamageZone(context, effectSkillData, effect, targeting, center, damage, statusSpec);
            }

            bool routed;
            if (TryApplyRuntimeHitboxEffect(context, skillData, effect, targeting, center, damage, statusSpec, out routed))
            {
                return routed;
            }

            UnitCombatState eventTarget = null;
            if (effect.TargetSelection == SkillMultiEffectTargetSelection.EventTarget)
            {
                eventTarget = context.EventTarget;
            }

            var criticalChanceBonus = 0f;
            var criticalDamageBonus = 0f;
            if (skillData != null)
            {
                criticalChanceBonus = skillData.CritChanceBonus;
                criticalDamageBonus = skillData.CritDamageBonus;
            }

            var effectId = effect.SkillId;
            if (!string.IsNullOrWhiteSpace(effect.EffectId))
            {
                effectId = effect.EffectId;
            }

            if (eventTarget != null)
            {
                var targetDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier = Mathf.Max(0f, effectSkillData.DamageMultiplier)
                    * SkillExecutionRuleResolver.ConditionalDamageMultiplier(effectSkillData, eventTarget);
                var result = context.CombatManager.ApplyDamage(
                    eventTarget,
                    targetDamage,
                    effect.Attribute,
                    context.Caster,
                    damageSpec.CriticalAllowed,
                    criticalChanceBonus,
                    criticalDamageBonus,
                    effectId,
                    finalDamageMultiplier: finalDamageMultiplier);
                if (!result.IsDead)
                {
                    StatusCombatRules.ApplyStatus(context.CombatManager, eventTarget, statusSpec, context.Caster);
                }
                if (context.CombatManager.Effects != null)
                {
                    ShowTimedEffectVisual(context, effect, center);
                }
                return true;
            }

            var hit = ZoneSkillActor.ApplyAreaTick(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                center,
                SkillTargeting.Radius(effect.Radius, skillData.RadiusMultiplier, skillData.RadiusBonus),
                effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                damage,
                effect.Attribute,
                statusSpec,
                context.Caster,
                effectId,
                null,
                damageSpec.CriticalAllowed,
                criticalChanceBonus,
                criticalDamageBonus);
            if (hit && context.CombatManager.Effects != null)
            {
                ShowTimedEffectVisual(context, effect, center);
            }
            return hit;
        }

        private static void ShowTimedEffectVisual(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillEffectDefinition effect /* 표시할 추가 효과 */,
            Vector2 center /* 표시 위치 */)
        {
            context.CombatManager.Effects.CreateEffect(new EffectCreateRequest(
                effect.RuntimeVisual,
                effect.SkillEffectPrefab,
                effect.RuntimeObjectName("SkillEffectVisual"),
                center,
                Quaternion.identity,
                null,
                1f,
                null,
                false,
                true,
                false));
        }

        /*
         * 추가 피해의 런타임 비주얼 충돌체를 사용해 범위 피해를 적용한다.
         */
        private static bool TryApplyRuntimeHitboxEffect(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            SkillTargetingSpec targeting /* 대상 선택 설정 */,
            Vector2 center /* 효과 중심 */,
            float damage /* 적용할 피해 */,
            ProjectileStatusHitSpec statusSpec /* 적중 상태 설정 */,
            out bool routed /* 실제 피해 적용 여부 */)
        {
            routed = false;
            var visual = effect.RuntimeVisual;
            RuntimeSkillHitboxSpec hitbox = null;
            if (visual != null)
            {
                hitbox = visual.Hitbox;
            }
            if (hitbox == null || !hitbox.HasHitbox())
            {
                return false;
            }
            if (context == null || context.CombatManager == null || context.CombatManager.Effects == null)
            {
                return true;
            }

            var objectName = effect.RuntimeObjectName("SkillEffectVisual");
            var instance = context.CombatManager.Effects.CreateEffect(new EffectCreateRequest(visual, null, objectName, center, Quaternion.identity, null, 0f, null, false, true, false));
            if (instance == null)
            {
                return true;
            }

            EffectVisualBuilder.ConfigureAreaEffect(instance, effect.Radius, skillData.RadiusMultiplier, skillData.RadiusBonus);
            SingleSkillActor.Attach(instance).InitializeTimed(context.CombatManager.Effects, 1f);
            var colliders = instance.GetComponentsInChildren<Collider2D>();
            if (colliders == null || colliders.Length == 0)
            {
                return true;
            }

            var criticalChanceBonus = 0f;
            var criticalDamageBonus = 0f;
            if (skillData != null)
            {
                criticalChanceBonus = skillData.CritChanceBonus;
                criticalDamageBonus = skillData.CritDamageBonus;
            }
            var effectId = effect.SkillId;
            if (!string.IsNullOrWhiteSpace(effect.EffectId))
            {
                effectId = effect.EffectId;
            }

            routed = ZoneSkillActor.ApplyColliderAreaTick(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                colliders,
                int.MaxValue,
                damage,
                effect.Attribute,
                statusSpec,
                context.Caster,
                effectId,
                null,
                true,
                criticalChanceBonus,
                criticalDamageBonus,
                null);
            return true;
        }

        /*
         * 지속 피해 추가 효과용 Zone Actor를 생성하고 실행 정보를 전달한다.
         */
        private static bool CreateAdditionalDamageZone(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 적용할 추가 효과 */,
            SkillTargetingSpec targeting /* 대상 선택 설정 */,
            Vector2 center /* 효과 중심 */,
            float damage /* Tick 피해 */,
            ProjectileStatusHitSpec statusSpec /* 적중 상태 설정 */)
        {
            if (context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || context.CasterEntry == null
                || context.Roster == null)
            {
                return false;
            }

            var duration = effect.ActiveDurationSeconds;
            var interval = effect.TickIntervalSeconds;
            if (skillData != null)
            {
                duration = duration * Mathf.Max(0f, skillData.DurationMultiplier) + skillData.DurationBonus;
                interval *= Mathf.Max(0.05f, skillData.ShotIntervalMultiplier);
            }
            duration = Mathf.Max(0.05f, duration);
            interval = Mathf.Max(0.05f, interval);

            var objectName = effect.RuntimeObjectName("SkillEffectZone");
            var instance = context.CombatManager.Effects.CreateEffect(new EffectCreateRequest(
                effect.RuntimeVisual,
                effect.SkillEffectPrefab,
                objectName,
                center,
                Quaternion.identity,
                null,
                0f,
                null,
                false,
                true,
                true));
            if (instance != null)
            {
                EffectVisualBuilder.ConfigureAreaEffect(instance, effect.Radius, skillData.RadiusMultiplier, skillData.RadiusBonus);
            }
            var actor = instance.GetComponent<ZoneSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ZoneSkillActor>();
            }
            var criticalChanceBonus = 0f;
            var criticalDamageBonus = 0f;
            if (skillData != null)
            {
                criticalChanceBonus = skillData.CritChanceBonus;
                criticalDamageBonus = skillData.CritDamageBonus;
            }
            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                targeting,
                center,
                SkillTargeting.Radius(effect.Radius, skillData.RadiusMultiplier, skillData.RadiusBonus),
                effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                duration,
                interval,
                int.MaxValue,
                damage,
                effect.Attribute,
                statusSpec,
                context.Runtime,
                skillData,
                Array.Empty<SkillEffectDefinition>(),
                context.Caster,
                true,
                criticalChanceBonus,
                criticalDamageBonus);
            return true;
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
                        actionExecutionContext),
                    legacyEffectActive: true);
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
         * 재시전을 실행한다.
         */
        internal static bool ExecuteRecast(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData inheritedData /* 앞 실행에서 이어받은 스킬 강화 정보 */,
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

            var snapshot = effect.RecastInheritSkillData
                ? inheritedData
                : new SkillExecutionData(skill);
            var radius = Radius(skill, snapshot) * Mathf.Max(0f, effect.RecastRadiusMultiplier);
            var duration = Mathf.Max(0.05f, effect.RecastDurationSeconds);
            var tickInterval = TickInterval(skill, snapshot);
            var hitTargetCount = HitTargetCount(skill, snapshot);
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.StatusSpec(skill.OnTickStatus, snapshot);
            var planEffects = skill.MultiEffects;
            var expireEffects = OnExpireEffects(context, snapshot, planEffects);
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

            var instance = effects.CreateEffect(new EffectCreateRequest(
                runtimeVisual,
                prefab,
                objectName,
                center,
                Quaternion.identity,
                null,
                0f,
                null,
                false,
                true,
                true));

            EffectVisualBuilder.ConfigureAreaEffect(
                instance,
                SkillTargeting.BaseRadius(skill.Targeting, skill.Area),
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus,
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
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var deploymentCount = DeploymentCount(snapshot);
            var centers = AreaCenters(context, skill.Targeting, skill.Area, deploymentCount);
            var radius = Radius(skill, snapshot);
            var duration = Duration(skill, snapshot);
            var tickInterval = TickInterval(skill, snapshot);
            var hitTargetCount = HitTargetCount(skill, snapshot);
            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            var attribute = skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element;
            var statusSpec = SkillStatus.StatusSpec(skill.OnTickStatus, snapshot);
            var planEffects = skill.MultiEffects;
            var expireEffects = OnExpireEffects(context, snapshot, planEffects);
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

                var instance = effects.CreateEffect(new EffectCreateRequest(
                    runtimeVisual,
                    prefab,
                    objectName,
                    center,
                    Quaternion.identity,
                    null,
                    0f,
                    null,
                    false,
                    true,
                    true));

                EffectVisualBuilder.ConfigureAreaEffect(
                    instance,
                    SkillTargeting.BaseRadius(skill.Targeting, skill.Area),
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus);

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
            routed = ExecuteAdditionalEffects(context, snapshot, planEffects, center, false, SkillMultiEffectTiming.OnCast, false) || routed;
            }

            return routed;
        }

        /*
         * 배치 횟수를 결정한다.
         */
        private static int DeploymentCount(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            return 1 + (snapshot != null && snapshot.HasBranchCount ? Math.Max(0, snapshot.BranchCount) : 0);
        }

        /*
         * 적중 대상 횟수를 결정한다.
         */
        private static int HitTargetCount(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
        private static List<Vector2> AreaCenters(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */,
            int deploymentCount /* 배치 개수 */)
        {
            var primaryCenter = AreaCenter(context, targeting, area);
            var coverAll = (area != null && area.CoverAll)
                || (targeting != null && targeting.CoverAll);
            return SkillTargeting.TargetAnchoredCenters(
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
        private static Vector2 AreaCenter(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */)
        {
            return SkillTargeting.AreaCenter(context, targeting, area);
        }

        /*
         * 반경을 결정한다.
         */
        private static float Radius(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            var area = skill != null ? skill.Area : null;
            var targeting = skill != null ? skill.Targeting : null;
            return SkillTargeting.Radius(
                SkillTargeting.BaseRadius(targeting, area),
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus);
        }

        /*
         * 지속시간을 결정한다.
         */
        private static float Duration(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            var area = skill != null ? skill.Area : null;
            var timing = skill != null ? skill.Timing : null;
            var duration = area != null && area.Duration > 0f
                ? area.Duration
                : timing != null ? timing.ActiveDuration : 0f;
            if (duration <= 0f)
            {
                duration = TickInterval(skill, snapshot);
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
        private static float TickInterval(ZoneSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
        private static SkillEffectDefinition[] OnExpireEffects(
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
                    || effect.EffectTiming != SkillMultiEffectTiming.OnExpire
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
