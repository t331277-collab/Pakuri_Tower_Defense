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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
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
        private static ProjectileStatusHitSpec ResolveBuffStatusSpec(BuffSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */)
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
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
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
            var planEffects = SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = Vector2.zero;
                if (context.CasterEntry.Transform != null)
                {
                    center = context.CasterEntry.Transform.position;
                }
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
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
