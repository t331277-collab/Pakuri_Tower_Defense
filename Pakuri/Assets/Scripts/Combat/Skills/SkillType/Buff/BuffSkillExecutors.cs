using System;
using System.Collections;
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
         * 요청받은 버프 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            BuffSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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

                var visualTarget = target.Transform;
                if (skill.AttachVisualToCaster)
                {
                    visualTarget = context.CasterEntry.Transform;
                }

                var canSpawnVisual = !skill.AttachVisualToCaster || !casterVisualSpawned;
                GameObject visualInstance = null;
                if (canSpawnVisual && visualTarget != null && effects != null)
                {
                    var visualName = "RuntimeBuffVisual";
                    if (!string.IsNullOrWhiteSpace(skill.SkillId))
                    {
                        visualName = "RuntimeBuffVisual_" + skill.SkillId;
                    }

                    visualInstance = effects.CreateEffect(
                        runtimeVisual,
                        prefab,
                        visualName,
                        visualTarget.position,
                        Quaternion.identity);
                    if (visualInstance != null)
                    {
                        BuffSkillActor.Attach(visualInstance).Initialize(
                            effects,
                            visualTarget,
                            statusSpec.DurationSeconds,
                            Vector3.zero);
                    }
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            var multiEffectRouted = false;
            var planEffects = skill.MultiEffects;
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = ExecuteAdditionalEffects(context, snapshot, planEffects, center, false, SkillMultiEffectTiming.OnCast, true);
            }

            return routed || castCommitted || multiEffectRouted;
        }

        /*
         * 버프 상태 설정을 결정한다.
         */
        private static ProjectileStatusHitSpec ResolveBuffStatusSpec(BuffSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
            CombatUnitEntry caster /* 스킬을 사용하는 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillTargetSide targetMode /* 대상 방식 */)
        {
            if (targetMode == SkillTargetSide.Self)
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
            CombatUnitEntry caster /* 스킬을 사용하는 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */)
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            BuffShieldSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var shieldStat = context.Caster.Stats.SpellPower;
            shieldStat *= StatusCombatRules.ResolveSpellPowerMultiplier(context.Caster);
            if (skill.ShieldStatSource == StatSource.Attack)
            {
                shieldStat = context.Caster.Stats.AttackPower;
                shieldStat *= StatusCombatRules.ResolveAttackPowerMultiplier(context.Caster);
            }

            var shield = Mathf.Max(0f, skill.ShieldBase + shieldStat * skill.ShieldCoefficient);
            if (snapshot != null)
            {
                shield = (shield + snapshot.BaseDamageBonus)
                    * Mathf.Max(0f, snapshot.DamageMultiplier)
                    * Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
            }
            shield = Mathf.Max(0f, shield);

            var duration = skill.ShieldDuration;
            if (duration <= 0f && skill.ShieldStatus != null)
            {
                duration = skill.ShieldStatus.Duration;
            }
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

            IReadOnlyList<CombatUnitEntry> targets;
            if (skill.UseConfiguredTargeting)
            {
                targets = BuffSkillExecutor.ResolveConfiguredTargets(context.CasterEntry, context.Roster, skill.Targeting);
            }
            else
            {
                targets = BuffSkillExecutor.ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            }
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
                var visualTarget = target.Transform;
                if (skill.AttachVisualToCaster)
                {
                    visualTarget = context.CasterEntry.Transform;
                }

                var canSpawnVisual = !skill.AttachVisualToCaster || !casterVisualSpawned;
                GameObject visualInstance = null;
                if (canSpawnVisual && visualTarget != null && effects != null)
                {
                    var visualName = "RuntimeShieldVisual";
                    if (!string.IsNullOrWhiteSpace(skill.SkillId))
                    {
                        visualName = $"RuntimeShieldVisual_{skill.SkillId}";
                    }

                    visualInstance = effects.CreateEffect(
                        runtimeVisual,
                        prefab,
                        visualName,
                        visualTarget.position,
                        Quaternion.identity);
                    if (visualInstance != null)
                    {
                        BuffSkillActor.Attach(visualInstance).Initialize(
                            effects,
                            visualTarget,
                            duration,
                            Vector3.zero);
                    }
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            var multiEffectRouted = false;
            var planEffects = skill.MultiEffects;
            if (routed && planEffects.Length > 0)
            {
                var center = Vector2.zero;
                if (context.CasterEntry.Transform != null)
                {
                    center = context.CasterEntry.Transform.position;
                }
                multiEffectRouted = BuffSkillExecutor.ExecuteAdditionalEffects(context, snapshot, planEffects, center, false, SkillMultiEffectTiming.OnCast, true);
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            BuffHealSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var targets = SkillTargeting.ResolveOrderedTargets(context.CasterEntry, context.Roster, skill.Targeting);
            CombatUnitEntry target = null;
            if (targets.Count > 0)
            {
                target = targets[0];
            }
            if (target == null || target.Model == null)
            {
                return false;
            }

            var healing = skill.Healing;
            var amount = healing.BaseDamage;
            if (healing.UseCombinedStatCoefficients)
            {
                var attack = context.Caster.Stats.AttackPower;
                attack *= StatusCombatRules.ResolveAttackPowerMultiplier(context.Caster);
                var spell = context.Caster.Stats.SpellPower;
                spell *= StatusCombatRules.ResolveSpellPowerMultiplier(context.Caster);
                amount += attack * healing.AttackPowerCoefficient;
                amount += spell * healing.SpellPowerCoefficient;
            }
            else if (healing.StatSource == StatSource.Attack)
            {
                var attack = context.Caster.Stats.AttackPower;
                attack *= StatusCombatRules.ResolveAttackPowerMultiplier(context.Caster);
                amount += attack * healing.StatCoefficient;
            }
            else
            {
                var spell = context.Caster.Stats.SpellPower;
                spell *= StatusCombatRules.ResolveSpellPowerMultiplier(context.Caster);
                amount += spell * healing.StatCoefficient;
            }
            amount = Mathf.Max(0f, amount);
            if (context.Caster is EnemyCombatState enemy)
            {
                amount *= Mathf.Max(0f, enemy.PassiveHealingMultiplier);
            }

            context.CombatManager.Heal(target.Model, amount);
            var effects = context.CombatManager.Effects;
            if (effects != null)
            {
                var visualName = "RuntimeSupportVisual";
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    visualName = "RuntimeSupportVisual_" + skill.SkillId;
                }

                var visualInstance = effects.CreateEffect(
                    skill.RuntimeVisual,
                    null,
                    visualName,
                    target.Transform.position,
                    Quaternion.identity);
                if (visualInstance != null)
                {
                    BuffSkillActor.Attach(visualInstance).Initialize(
                        effects,
                        target.Transform,
                        0.8f,
                        Vector3.zero);
                }
            }
            return true;
        }
    }

}
