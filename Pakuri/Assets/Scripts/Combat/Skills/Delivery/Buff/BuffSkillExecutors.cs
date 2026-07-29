using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 강화 계열 스킬의 세부 실행기를 정의한다.
 * 일반 버프와 보호막·회복 처리를 각 전용 실행기로 전달한다.
 */
namespace Pakuri.InGame
{
    internal static class BuffSkillExecutor
    {
        // 일반 Buff의 대상 선정, 상태 적용, 추가 효과 실행을 구현.
        /*
         * 현재 스킬의 노드 효과 중 요청한 실행 시점에 맞는 효과를 적용한다.
         */

        /*
         * 추가 효과의 지연시간이 지난 뒤 같은 Executor에서 효과를 적용한다.
         */

        /*
         * 추가 효과 종류에 맞는 실제 적용 기능을 호출한다.
         */

        /*
         * 요청받은 버프 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            BuffSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var statusSpec = BuffStatusSpec(skill, snapshot);
            if (statusSpec == null)
            {
                return false;
            }

            var targets = skill.UseConfiguredTargeting
                ? ConfiguredTargets(context, skill.Targeting)
                : BuffTargets(context.CasterEntry, context.Roster, skill.Target);
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
                if (!StatusCombatRules.ApplyStatus(
                    context.CombatManager,
                    target.Model,
                    statusSpec,
                    context.Caster))
                {
                    continue;
                }

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

                    visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, visualName, visualTarget.position, Quaternion.identity, visualTarget, statusSpec.DurationSeconds, null, false, true, false));
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            return routed || castCommitted;
        }

        /*
         * 버프 상태 설정을 결정한다.
         */
        private static ProjectileStatusHitSpec BuffStatusSpec(BuffSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
        {
            if (skill == null)
            {
                return null;
            }

            return SkillStatus.StatusSpec(skill.AttachedStatus, snapshot);
        }

        /*
         * 버프 대상을 결정한다.
         */
        internal static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> BuffTargets(
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

            return SkillTargeting.TargetList(
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
        internal static IReadOnlyList<CombatUnitEntry> ConfiguredTargets(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */)
        {
            var targets = SkillTargeting.OrderedTargets(context, targeting);
            var caster = context != null ? context.CasterEntry : null;
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
        // 보호막 수치 계산, 대상 적용, 보호막 시각 효과 생성을 구현.
        /*
         * 요청받은 보호막 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            BuffShieldSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var shieldStat = context.Caster.Stats.SpellPower;
            shieldStat *= StatusCombatRules.SpellPowerMultiplier(context.Caster);
            if (skill.ShieldStatSource == StatSource.Attack)
            {
                shieldStat = context.Caster.Stats.AttackPower;
                shieldStat *= StatusCombatRules.AttackPowerMultiplier(context.Caster);
            }

            var shield = Mathf.Max(0f, skill.ShieldBase + shieldStat * skill.ShieldCoefficient);
            if (snapshot != null)
            {
                if (context.ApplyDamageMultiplierToShield)
                {
                    shield *= Mathf.Max(0f, snapshot.DamageMultiplier);
                }
                shield *= Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
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

            var statusData = SkillStatus.StatusData(skill.ShieldStatus, StatusEffectKind.Shield, snapshot);
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
                targets = BuffSkillExecutor.ConfiguredTargets(context, skill.Targeting);
            }
            else
            {
                targets = BuffSkillExecutor.BuffTargets(context.CasterEntry, context.Roster, skill.Target);
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

                    visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, visualName, visualTarget.position, Quaternion.identity, visualTarget, duration, null, false, true, false));
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            return routed;
        }
    }

    /*
     * 회복 스킬을 실행한다.
     */
    internal static class BuffHealSkillExecutor
    {
        // 회복 수치 계산과 대상 체력 회복을 구현.
        /*
         * 요청받은 회복 스킬을 실행한다.
         */
        internal static bool Execute(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            BuffHealSkillDefinition skill /* 실행하거나 검사할 스킬 */)
        {
            var targets = SkillTargeting.OrderedTargets(context, skill.Targeting);
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
            var attack = context.Caster.Stats.AttackPower * StatusCombatRules.AttackPowerMultiplier(context.Caster);
            var spell = context.Caster.Stats.SpellPower * StatusCombatRules.SpellPowerMultiplier(context.Caster);
            var amount = healing.BaseDamage
                + attack * healing.AttackPowerCoefficient
                + spell * healing.SpellPowerCoefficient;
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

                effects.CreateEffect(new EffectCreateRequest(skill.RuntimeVisual, null, visualName, target.Transform.position, Quaternion.identity, target.Transform, 0.8f, null, false, true, false));
            }
            return true;
        }
    }

}
