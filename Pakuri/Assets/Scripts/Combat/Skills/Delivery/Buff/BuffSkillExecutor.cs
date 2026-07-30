/*
 * 역할: 버프 계열 스킬 전달.
 * 책임: 상태·회복·보호막·돌진 효과를 하나의 버프 실행 경로로 전달한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// BuffSkillDefinition의 효과 종류에 맞는 런타임 동작을 실행한다.
    internal static class BuffSkillExecutor
    {

        /// 설정된 버프 효과를 실행한다.
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill)
        {
            switch (skill.EffectKind)
            {
                case BuffEffectKind.Heal:
                    return ExecuteHeal(context, snapshot, skill);
                case BuffEffectKind.Shield:
                    return ExecuteShield(context, snapshot, skill);
                case BuffEffectKind.Charge:
                    return ExecuteCharge(context);
                default:
                    return ExecuteStatus(context, snapshot, skill);
            }
        }

        private static bool ExecuteStatus(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill)
        {
            var statusSpec = SkillStatus.StatusSpec(skill.AttachedStatus, snapshot);
            if (statusSpec == null)
            {
                return false;
            }

            var targets = Targets(context, skill);
            var routed = false;
            var castCommitted = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!IsValid(target))
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

                SpawnVisual(
                    context,
                    snapshot,
                    skill,
                    target,
                    "RuntimeBuffVisual",
                    statusSpec.DurationSeconds,
                    ref casterVisualSpawned);
                routed = true;
            }

            return routed || castCommitted;
        }

        private static bool ExecuteHeal(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill)
        {
            var targets = SkillTargeting.OrderedTargets(context, skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            if (!IsValid(target))
            {
                return false;
            }

            var healing = skill.Healing;
            var attack = context.Caster.Stats.AttackPower
                * StatusCombatRules.AttackPowerMultiplier(context.Caster);
            var spell = context.Caster.Stats.SpellPower
                * StatusCombatRules.SpellPowerMultiplier(context.Caster);
            var amount = healing.BaseDamage
                + attack * healing.AttackPowerCoefficient
                + spell * healing.SpellPowerCoefficient;
            amount = Mathf.Max(0f, amount)
                * context.Caster.SkillState.PassiveHealingMultiplier();

            context.CombatManager.Heal(target.Model, amount);
            var casterVisualSpawned = false;
            SpawnVisual(
                context,
                snapshot,
                skill,
                target,
                "RuntimeSupportVisual",
                0.8f,
                ref casterVisualSpawned);
            return true;
        }

        private static bool ExecuteShield(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill)
        {
            var shieldStat = context.Caster.Stats.SpellPower
                * StatusCombatRules.SpellPowerMultiplier(context.Caster);
            if (skill.ShieldStatSource == StatSource.Attack)
            {
                shieldStat = context.Caster.Stats.AttackPower
                    * StatusCombatRules.AttackPowerMultiplier(context.Caster);
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

            var duration = skill.ShieldDuration;
            if (duration <= 0f && skill.ShieldStatus != null)
            {
                duration = skill.ShieldStatus.Duration;
            }
            if (snapshot != null
                && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                    || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier)
                    + snapshot.DurationBonus;
            }

            var statusData = SkillStatus.StatusData(
                skill.ShieldStatus,
                StatusEffectKind.Shield,
                snapshot);
            if (statusData == null || duration <= 0f)
            {
                return false;
            }

            var targets = Targets(context, skill);
            var routed = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!IsValid(target))
                {
                    continue;
                }

                context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    statusData,
                    Mathf.Max(0f, shield),
                    duration,
                    1,
                    0,
                    false,
                    true,
                    context.Caster);
                SpawnVisual(
                    context,
                    snapshot,
                    skill,
                    target,
                    "RuntimeShieldVisual",
                    duration,
                    ref casterVisualSpawned);
                routed = true;
            }

            return routed;
        }

        private static bool ExecuteCharge(SkillExecutionContext context)
        {
            return context.Caster != null && context.Runtime != null;
        }

        /// 활성 Charge 버프가 접촉한 대상에게 정의된 피해와 상태를 적용한다.
        internal static bool ApplyChargeContact(
            InGameCombatManager combatManager,
            UnitCombatState caster,
            CombatUnitEntry target,
            SkillUseState runtime)
        {
            var skill = runtime != null ? runtime.Data as BuffSkillDefinition : null;
            if (combatManager == null
                || caster == null
                || !IsValid(target)
                || skill == null
                || skill.EffectKind != BuffEffectKind.Charge)
            {
                return false;
            }

            var maxHealth = target.Model.Stats != null
                ? Mathf.Max(0f, target.Model.Stats.MaxHealth)
                : 0f;
            var damageResult = combatManager.ApplyDamage(
                target.Model,
                maxHealth * Mathf.Max(0f, skill.ChargeTargetMaxHealthRatio),
                skill.Element,
                caster,
                true,
                sourceSkillId: skill.SkillId);

            var statusSpec = SkillStatus.StatusSpec(skill.AttachedStatus, null);
            if (!damageResult.IsDead && statusSpec != null)
            {
                StatusCombatRules.ApplyStatus(combatManager, target.Model, statusSpec, caster);
            }

            runtime.StopActive();
            return true;
        }

        private static IReadOnlyList<CombatUnitEntry> Targets(
            SkillExecutionContext context,
            BuffSkillDefinition skill)
        {
            return skill.UseConfiguredTargeting
                ? ConfiguredTargets(context, skill.Targeting)
                : BuffTargets(context.CasterEntry, context.Roster, skill.Target);
        }

        private static bool IsValid(CombatUnitEntry target)
        {
            return target != null && target.IsAlive && target.Model != null;
        }

        private static void SpawnVisual(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill,
            CombatUnitEntry target,
            string namePrefix,
            float duration,
            ref bool casterVisualSpawned)
        {
            if (skill.AttachVisualToCaster && casterVisualSpawned)
            {
                return;
            }

            var visualTarget = skill.AttachVisualToCaster
                ? context.CasterEntry.Transform
                : target.Transform;
            var effects = context.CombatManager.Effects;
            if (visualTarget == null || effects == null)
            {
                return;
            }

            var prefab = snapshot != null && snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : skill.SkillEffectPrefab;
            var visualName = string.IsNullOrWhiteSpace(skill.SkillId)
                ? namePrefix
                : namePrefix + "_" + skill.SkillId;
            var instance = effects.CreateEffect(new EffectCreateRequest(
                skill.RuntimeVisual,
                prefab,
                visualName,
                visualTarget.position,
                Quaternion.identity,
                visualTarget,
                null,
                false,
                true,
                false));
            if (instance != null)
            {
                BuffSkillActor.Attach(instance).InitializeTimed(effects, duration);
                casterVisualSpawned = skill.AttachVisualToCaster;
            }
        }

        internal static IReadOnlyList<CombatUnitEntry> BuffTargets(
            CombatUnitEntry caster,
            UnitSpawnManager roster,
            SkillTargetSide targetMode)
        {
            if (targetMode == SkillTargetSide.Self)
            {
                return caster != null
                    ? new[] { caster }
                    : Array.Empty<CombatUnitEntry>();
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

        internal static IReadOnlyList<CombatUnitEntry> ConfiguredTargets(
            SkillExecutionContext context,
            SkillTargetingSpec targeting)
        {
            var targets = SkillTargeting.OrderedTargets(context, targeting);
            var caster = context != null ? context.CasterEntry : null;
            if (caster == null
                || caster.Transform == null
                || targeting == null
                || targeting.Radius <= 0f)
            {
                return targets;
            }

            var radiusSq = targeting.Radius * targeting.Radius;
            targets.RemoveAll(target =>
                target == null
                || target.Transform == null
                || ((Vector2)target.Transform.position
                    - (Vector2)caster.Transform.position).sqrMagnitude > radiusSq);
            return targets;
        }
    }
}
