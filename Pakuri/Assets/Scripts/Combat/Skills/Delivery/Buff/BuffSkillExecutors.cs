/*
 * 역할: 버프 계열 스킬 전달.
 * 책임: 전투 및 상태 시스템을 통해 버프·회복·보호막·패시브 효과를 실행한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>BuffSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
    internal static class BuffSkillExecutor
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill)
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>BuffStatusSpec</c> 결과값을 생성해 반환한다.</summary>
        private static ProjectileStatusHitSpec BuffStatusSpec(BuffSkillDefinition skill, SkillExecutionData snapshot)
        {
            if (skill == null)
            {
                return null;
            }

            return SkillStatus.StatusSpec(skill.AttachedStatus, snapshot);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>BuffTargets</c> 결과값을 생성해 반환한다.</summary>
        internal static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> BuffTargets(
            CombatUnitEntry caster,
            UnitSpawnManager roster,
            SkillTargetSide targetMode)
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConfiguredTargets</c> 결과값을 생성해 반환한다.</summary>
        internal static IReadOnlyList<CombatUnitEntry> ConfiguredTargets(
            SkillExecutionContext context,
            SkillTargetingSpec targeting)
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

    /// <summary><c>BuffShieldSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
    internal static class BuffShieldSkillExecutor
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffShieldSkillDefinition skill)
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

    /// <summary><c>BuffHealSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
    internal static class BuffHealSkillExecutor
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffHealSkillDefinition skill)
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
            amount *= context.Caster.SkillState.PassiveHealingMultiplier();

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
