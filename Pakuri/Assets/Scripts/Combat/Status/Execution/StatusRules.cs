/*
 * 역할: 상태 효과 변경 규칙.
 * 책임: 병합·갱신·중첩·보호막·지속 시간·소비·제거 정책을 적용한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// StatusCombatRules에 공통으로 적용되는 런타임 규칙을 구현한다.
    public static class StatusCombatRules
    {
        private const float MinimumActionMultiplier = 0.05f;

        public static bool ApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            StatusApplicationSpec status,
            UnitCombatState source = null)
        {
            if (manager == null || target == null || status == null || !status.Enabled)
            {
                return false;
            }

            if (target.Resources.CurrentHealth <= 0f)
            {
                return false;
            }

            var chance = ApplicationChance(target, status, source);
            if (chance <= 0f || UnityEngine.Random.value > chance)
            {
                return false;
            }

            var duration = DurationSeconds(status, source);
            var appliedStatus = manager.ApplyStatus(
                target,
                status.Status,
                status.Stacks,
                duration,
                status.RuntimeResolved ? status.RuntimeMaxStacks : status.Status.MaxStacks,
                status.RuntimeResolved ? status.RuntimePermanent : status.Status.Permanent,
                status.RefreshDuration,
                source);
            if (appliedStatus == null)
            {
                return false;
            }

            ApplyThresholdStatus(manager, target, status, source);
            return true;
        }

        public static float ApplicationChance(
            UnitCombatState target,
            StatusApplicationSpec status,
            UnitCombatState source = null)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chanceBonus = ConditionalStatusChanceBonus(source, target);
            var chance = Mathf.Clamp01(status.Chance + chanceBonus);
            if (chance <= 0f || target == null || !IsDebuff(status.Status))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - AilmentResistanceBonus(target));
        }

        private static float DurationSeconds(StatusApplicationSpec status, UnitCombatState source)
        {
            var duration = status.RuntimeResolved
                ? status.RuntimeDurationSeconds
                : status.Status.Duration;
            duration = Mathf.Max(0f, duration);
            var statusName = status.Status.StatusTag;
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                duration += AppliedStatusDurationBonus(source, statusName);
                duration = Mathf.Max(0f, duration);
            }

            return duration;
        }

        private static bool IsDebuff(StatusRuntimeData statusData)
        {
            return statusData.Definition.Classification == StatusEffectClassification.Debuff;
        }

        private static void ApplyThresholdStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            StatusApplicationSpec status,
            UnitCombatState source)
        {
            if (target.Statuses == null
                || status.ThresholdStatus == null
                || !status.ThresholdStatus.Enabled
                || status.ThresholdSourceStatusKind == StatusEffectKind.None
                || status.ThresholdSourceMinStacks <= 0)
            {
                return;
            }

            if (target.Statuses.GetStacks(status.ThresholdSourceStatusKind) < status.ThresholdSourceMinStacks)
            {
                return;
            }

            var thresholdStatus = status.ThresholdStatus;
            manager.ApplyStatus(
                target,
                thresholdStatus.Status,
                thresholdStatus.Stacks,
                thresholdStatus.RuntimeResolved
                    ? thresholdStatus.RuntimeDurationSeconds
                    : thresholdStatus.Status.Duration,
                thresholdStatus.RuntimeResolved
                    ? thresholdStatus.RuntimeMaxStacks
                    : thresholdStatus.Status.MaxStacks,
                thresholdStatus.RuntimeResolved
                    ? thresholdStatus.RuntimePermanent
                    : thresholdStatus.Status.Permanent,
                thresholdStatus.RefreshDuration,
                source);
        }

        /// OutgoingAdditionalDamageSpec 처리에 함께 전달되는 값들을 묶는다.
        internal readonly struct OutgoingAdditionalDamageSpec
        {

            public OutgoingAdditionalDamageSpec(float multiplier, DamageAttribute triggerAttribute, DamageAttribute damageAttribute)
            {
                Multiplier = multiplier;
                TriggerAttribute = triggerAttribute;
                DamageAttribute = damageAttribute;
            }

            public float Multiplier { get; }
            public DamageAttribute TriggerAttribute { get; }
            public DamageAttribute DamageAttribute { get; }
        }

        public static float ComputeModifierMagnitude(StatusRuntimeData data)
        {
            if (data == null)
            {
                return 0f;
            }

            return Mathf.Abs(data.Modifiers.ActionSpeedBonus)
                + Mathf.Abs(data.Modifiers.AttackPowerBonus)
                + Mathf.Abs(data.Modifiers.SpellPowerBonus)
                + Mathf.Abs(data.Modifiers.DamageBonusRate)
                + Mathf.Abs(data.Modifiers.ShieldReceivedBonus)
                + Mathf.Abs(data.Modifiers.CritChanceBonusRate)
                + Mathf.Abs(data.Modifiers.CritDamageBonusRate)
                + Mathf.Abs(data.MoveSpeedBonus)
                + Mathf.Abs(data.DamageTakenBonus)
                + Mathf.Abs(data.CriticalDamageTakenBonus)
                + Mathf.Abs(data.AilmentResistanceBonus)
                + Mathf.Abs(data.ElementResistReduction)
                + Mathf.Abs(data.FlatElementResistReduction)
                + Mathf.Abs(data.ElementDamageTakenBonus)
                + Mathf.Abs(data.ConditionalDamageTakenBonus)
                + Mathf.Abs(data.OutgoingAdditionalDamageMultiplier);
        }

        public static bool CanMove(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanMove);
        }

        public static bool CanAct(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanAct);
        }

        public static bool CanUseSpecialSkill(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanUseSpecialSkill);
        }

        public static float ActionSpeedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(
                MinimumActionMultiplier,
                1f + SumStacked(
                    model,
                    data => MeetsConditionalEffectTarget(model, model, data)
                        ? data.Modifiers.ActionSpeedBonus
                        : 0f));
        }

        public static float MoveSpeedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(
                0f,
                1f + SumStacked(
                    model,
                    data => MeetsConditionalEffectTarget(model, model, data)
                        ? data.MoveSpeedBonus
                        : 0f));
        }

        public static float AttackPowerMultiplier(UnitCombatState model)
        {
            return MultiplyStacked(
                model,
                data => MeetsConditionalEffectTarget(model, model, data)
                    ? data.Modifiers.AttackPowerBonus
                    : 0f);
        }

        public static float SpellPowerMultiplier(UnitCombatState model)
        {
            return MultiplyStacked(
                model,
                data => MeetsConditionalEffectTarget(model, model, data)
                    ? data.Modifiers.SpellPowerBonus
                    : 0f);
        }

        public static float ShieldReceivedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(
                0f,
                1f + SumStacked(
                    model,
                    data => MeetsConditionalEffectTarget(model, model, data)
                        ? data.Modifiers.ShieldReceivedBonus
                        : 0f));
        }

        public static float CriticalChanceBonus(
            UnitCombatState model,
            UnitCombatState target = null)
        {
            return SumStacked(
                model,
                data => MeetsConditionalEffectTarget(model, target, data)
                    ? data.Modifiers.CritChanceBonusRate
                    : 0f);
        }

        public static float CriticalDamageMultiplier(
            UnitCombatState model,
            UnitCombatState target = null)
        {
            return MultiplyStacked(
                model,
                data => MeetsConditionalEffectTarget(model, target, data)
                    ? data.Modifiers.CritDamageBonusRate
                    : 0f);
        }

        public static float OutgoingDamageMultiplier(UnitCombatState source, DamageAttribute attribute, string sourceSkillName = null)
        {
            return OutgoingDamageMultiplier(source, null, attribute, sourceSkillName);
        }

        public static float OutgoingDamageMultiplier(
            UnitCombatState source,
            UnitCombatState target,
            DamageAttribute attribute,
            string sourceSkillName = null)
        {
            return MultiplyStacked(source, data =>
            {
                if (MatchesAttribute(data, attribute)
                    && MeetsConditionalEffectTarget(source, target, data)
                    && StatusConditionRules.MatchesSkillRuntimeKinds(
                        data.ConditionalOutgoingSkillRuntimeKindValues,
                        sourceSkillName))
                {
                    return data.Modifiers.DamageBonusRate;
                }

                return 0f;
            });
        }

        internal static List<OutgoingAdditionalDamageSpec> OutgoingAdditionalDamageSpecs(UnitCombatState source, DamageAttribute triggerAttribute)
        {
            var results = new List<OutgoingAdditionalDamageSpec>();
            IReadOnlyList<StatusRuntimeInstance> statuses = null;
            if (source != null && source.Statuses != null)
            {
                statuses = source.Statuses.ActiveStatuses;
            }
            if (statuses == null)
            {
                return results;
            }

            for (var i = 0; i < statuses.Count; i++)
            {
                var runtime = statuses[i];
                if (runtime == null || runtime.Stacks <= 0)
                {
                    continue;
                }

                var data = RuntimeData(runtime);
                if (data == null
                    || !MeetsConditionalSourceStatus(runtime.SourceUnit, data)
                    || data.OutgoingAdditionalDamageMultiplier <= 0f
                    || data.OutgoingAdditionalDamageTriggerAttribute != triggerAttribute)
                {
                    continue;
                }

                results.Add(new OutgoingAdditionalDamageSpec(
                    data.OutgoingAdditionalDamageMultiplier * runtime.Stacks,
                    data.OutgoingAdditionalDamageTriggerAttribute,
                    data.OutgoingAdditionalDamageAttribute));
            }

            return results;
        }

        public static float IncomingDamageMultiplier(UnitCombatState target, UnitCombatState source, DamageAttribute attribute, string sourceSkillName = null)
        {
            return MultiplyStacked(target, data =>
            {
                var runtimeKindMatches = StatusConditionRules.MatchesSkillRuntimeKinds(data.ConditionalIncomingSkillRuntimeKindValues, sourceSkillName);
                var bonus = 0f;
                if (runtimeKindMatches)
                {
                    bonus = data.DamageTakenBonus;
                }
                if (runtimeKindMatches && MatchesAttribute(data, attribute))
                {
                    bonus += data.ElementDamageTakenBonus;
                }

                if (runtimeKindMatches && MatchesConditionalSourceStatus(source, data))
                {
                    bonus += data.ConditionalDamageTakenBonus;
                }
                if (!MeetsConditionalEffectTarget(target, target, data))
                {
                    bonus = 0f;
                }

                return bonus;
            });
        }

        public static float ElementResistMultiplier(UnitCombatState target, DamageAttribute attribute)
        {
            if (target == null || target.Statuses == null)
            {
                return 1f;
            }

            var multiplier = 1f;
            var statuses = target.Statuses.ActiveStatuses;
            for (var statusIndex = 0; statusIndex < statuses.Count; statusIndex++)
            {
                var status = statuses[statusIndex];
                if (status == null || status.Stacks <= 0)
                {
                    continue;
                }

                var data = RuntimeData(status);
                if (data == null
                    || !MeetsConditionalSourceStatus(status.SourceUnit, data)
                    || !MatchesAttribute(data, attribute))
                {
                    continue;
                }
                if (!MeetsConditionalEffectTarget(target, target, data))
                {
                    continue;
                }

                var reductionMultiplier = 1f - Mathf.Clamp01(data.ElementResistReduction);
                for (var stackIndex = 0; stackIndex < status.Stacks; stackIndex++)
                {
                    multiplier *= reductionMultiplier;
                }
            }

            return multiplier;
        }

        public static float FlatElementResistReduction(UnitCombatState target, DamageAttribute attribute)
        {
            return Mathf.Max(0f, SumStacked(target, data =>
            {
                if (MatchesAttribute(data, attribute))
                {
                    return MeetsConditionalEffectTarget(target, target, data)
                        ? data.FlatElementResistReduction
                        : 0f;
                }

                return 0f;
            }));
        }

        public static float CriticalDamageTakenMultiplier(UnitCombatState target)
        {
            return MultiplyStacked(
                target,
                data => MeetsConditionalEffectTarget(target, target, data)
                    ? data.CriticalDamageTakenBonus
                    : 0f);
        }

        public static float AilmentResistanceBonus(UnitCombatState target)
        {
            return Mathf.Clamp01(SumStacked(
                target,
                data => MeetsConditionalEffectTarget(target, target, data)
                    ? data.AilmentResistanceBonus
                    : 0f));
        }

        public static float ConditionalStatusChanceBonus(UnitCombatState source, UnitCombatState target)
        {
            return SumStacked(source, data =>
            {
                if (MatchesConditionalTargetStatus(target, data))
                {
                    return data.ConditionalStatusChanceBonus;
                }

                return 0f;
            });
        }

        public static float AppliedStatusDurationBonus(UnitCombatState source, string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                return 0f;
            }

            return SumStacked(source, data =>
            {
                if (string.Equals(
                    data.AppliedStatusDurationBonusStatusName,
                    statusName,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return data.AppliedStatusDurationBonus;
                }

                return 0f;
            });
        }

        private static bool HasAnyStatus(UnitCombatState model, System.Func<StatusRuntimeData, bool> predicate)
        {
            IReadOnlyList<StatusRuntimeInstance> statuses = null;
            if (model != null && model.Statuses != null)
            {
                statuses = model.Statuses.ActiveStatuses;
            }
            if (statuses == null)
            {
                return false;
            }

            for (var i = 0; i < statuses.Count; i++)
            {
                var runtime = statuses[i];
                if (runtime == null || runtime.Stacks <= 0)
                {
                    continue;
                }

                var data = RuntimeData(runtime);
                if (data != null && predicate(data))
                {
                    return true;
                }
            }

            return false;
        }

        private static float SumStacked(UnitCombatState model, System.Func<StatusRuntimeData, float> selector)
        {
            IReadOnlyList<StatusRuntimeInstance> statuses = null;
            if (model != null && model.Statuses != null)
            {
                statuses = model.Statuses.ActiveStatuses;
            }
            if (statuses == null)
            {
                return 0f;
            }

            var total = 0f;
            for (var i = 0; i < statuses.Count; i++)
            {
                var runtime = statuses[i];
                if (runtime == null || runtime.Stacks <= 0)
                {
                    continue;
                }

                var data = RuntimeData(runtime);
                if (data == null
                    || !MeetsConditionalSourceStatus(runtime.SourceUnit, data))
                {
                    continue;
                }

                total += selector(data) * runtime.Stacks;
            }

            return total;
        }

        private static float MultiplyStacked(UnitCombatState model, System.Func<StatusRuntimeData, float> selector)
        {
            IReadOnlyList<StatusRuntimeInstance> statuses = null;
            if (model != null && model.Statuses != null)
            {
                statuses = model.Statuses.ActiveStatuses;
            }
            if (statuses == null)
            {
                return 1f;
            }

            var multiplier = 1f;
            for (var i = 0; i < statuses.Count; i++)
            {
                var runtime = statuses[i];
                if (runtime == null || runtime.Stacks <= 0)
                {
                    continue;
                }

                var data = RuntimeData(runtime);
                if (data == null
                    || !MeetsConditionalSourceStatus(runtime.SourceUnit, data))
                {
                    continue;
                }

                multiplier *= Mathf.Max(0f, 1f + selector(data) * runtime.Stacks);
            }

            return multiplier;
        }

        private static StatusRuntimeData RuntimeData(StatusRuntimeInstance runtime)
        {
            if (runtime == null)
            {
                return null;
            }

            return runtime.SourceData;
        }

        private static bool MatchesAttribute(StatusRuntimeData data, DamageAttribute attribute)
        {
            return data != null && data.HasElementModifierTarget && (DamageAttribute)(int)data.ElementModifierTarget == attribute;
        }

        private static bool MatchesConditionalSourceStatus(UnitCombatState source, StatusRuntimeData data)
        {
            if (data == null || data.ConditionalSourceStatusKind == StatusEffectKind.None)
            {
                return false;
            }

            if (source == null)
            {
                return false;
            }

            if (data.ConditionalSourceStatusKind == StatusEffectKind.Shield)
            {
                return source.Resources != null && source.Resources.CurrentShield > 0f;
            }

            return source.Statuses != null && source.Statuses.Has(data.ConditionalSourceStatusKind);
        }

        private static bool MeetsConditionalSourceStatus(
            UnitCombatState source,
            StatusRuntimeData data)
        {
            return data == null
                || data.ConditionalSourceStatusKind == StatusEffectKind.None
                || MatchesConditionalSourceStatus(source, data);
        }

        private static bool MeetsConditionalEffectTarget(
            UnitCombatState carrier,
            UnitCombatState effectTarget,
            StatusRuntimeData data)
        {
            if (data == null)
            {
                return false;
            }
            var target = data.ConditionalTargetSide == SkillTargetSide.Enemy
                ? effectTarget
                : carrier;
            if (data.ConditionalTargetHealthRatioMax > 0f)
            {
                if (target?.Resources == null
                    || target.Stats == null
                    || target.Stats.MaxHealth <= 0f
                    || target.Resources.CurrentHealth / target.Stats.MaxHealth
                        > data.ConditionalTargetHealthRatioMax)
                {
                    return false;
                }
            }
            if (data.ConditionalTargetStatusGroups != null
                && data.ConditionalTargetStatusGroups.Length > 0
                && !StatusConditionRules.MatchesConditionStatus(
                    target,
                    data.ConditionalTargetStatusGroups,
                    data.ConditionalTargetStatusSourceSkillNames))
            {
                return false;
            }
            if (data.ConditionalTargetStatusKinds != null
                && data.ConditionalTargetStatusKinds.Length > 0
                && !MatchesConditionalTargetStatus(target, data))
            {
                return false;
            }
            return true;
        }

        private static bool MatchesConditionalTargetStatus(UnitCombatState target, StatusRuntimeData data)
        {
            if (data == null
                || data.ConditionalTargetStatusKinds == null
                || data.ConditionalTargetStatusKinds.Length == 0)
            {
                return false;
            }

            if (target == null)
            {
                return false;
            }

            for (var i = 0; i < data.ConditionalTargetStatusKinds.Length; i++)
            {
                var kind = data.ConditionalTargetStatusKinds[i];
                if (kind == StatusEffectKind.Shield)
                {
                    if (target.Resources != null && target.Resources.CurrentShield > 0f)
                    {
                        return true;
                    }

                    continue;
                }

                if (target.Statuses != null && target.Statuses.Has(kind))
                {
                    return true;
                }
            }

            return false;
        }
    }

    /// StatusConditionRules에 공통으로 적용되는 런타임 규칙을 구현한다.
    static class StatusConditionRules
    {

        public static bool MatchesConditionStatus(
            UnitCombatState target,
            StatusConditionGroup[] groups,
            string[] requiredSourceSkillNames)
        {
            if (groups == null || groups.Length == 0)
            {
                return true;
            }

            if (target == null)
            {
                return false;
            }

            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var requirements = groups[groupIndex].Requirements;
                var matchesGroup = true;
                for (var requirementIndex = 0; requirementIndex < requirements.Length; requirementIndex++)
                {
                    var requirement = requirements[requirementIndex];
                    var stacks = 0;
                    if (target.Statuses != null)
                    {
                        stacks = target.Statuses.GetStacks(requirement.Kind);
                    }

                    if (requirement.Kind == StatusEffectKind.Shield
                        && target.Resources != null
                        && target.Resources.CurrentShield > 0f)
                    {
                        stacks = Math.Max(1, stacks);
                    }

                    if (stacks < requirement.MinStacks
                        || !MatchesRequiredSourceSkill(target, requirement.Kind, requiredSourceSkillNames))
                    {
                        matchesGroup = false;
                        break;
                    }
                }

                if (matchesGroup)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MatchesConditionStatus(
            UnitCombatState target,
            StatusConditionGroup[] groups)
        {
            return MatchesConditionStatus(target, groups, Array.Empty<string>());
        }

        public static bool MatchesConditionStatus(
            StatusRuntimeInstance status,
            StatusConditionGroup[] groups)
        {
            if (groups == null || groups.Length == 0)
            {
                return true;
            }

            if (status == null)
            {
                return false;
            }

            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var requirements = groups[groupIndex].Requirements;
                var matchesGroup = true;
                for (var requirementIndex = 0; requirementIndex < requirements.Length; requirementIndex++)
                {
                    var requirement = requirements[requirementIndex];
                    if (status.Kind != requirement.Kind || status.Stacks < requirement.MinStacks)
                    {
                        matchesGroup = false;
                        break;
                    }
                }

                if (matchesGroup)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MatchesSkillRuntimeKinds(
            SkillRuntimeKindCondition[] conditions,
            string sourceSkillName)
        {
            if (conditions == null || conditions.Length == 0)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(sourceSkillName))
            {
                return false;
            }

            var catalog = GameDataLoader.CurrentCatalog;
            if (catalog == null
                || !catalog.TryGetData(sourceSkillName, out SkillDefinition skill)
                || skill == null)
            {
                return false;
            }

            for (var i = 0; i < conditions.Length; i++)
            {
                var condition = conditions[i];
                if (condition.AreaLike && IsAreaLikeSkill(skill))
                {
                    return true;
                }

                if (!condition.AreaLike && skill.RuntimeKind == condition.Kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesRequiredSourceSkill(
            UnitCombatState target,
            StatusEffectKind kind,
            string[] requiredSourceSkillNames)
        {
            if (requiredSourceSkillNames == null || requiredSourceSkillNames.Length == 0)
            {
                return true;
            }

            if (target.Statuses == null)
            {
                return false;
            }

            var statuses = target.Statuses.ActiveStatuses;
            for (var statusIndex = 0; statusIndex < statuses.Count; statusIndex++)
            {
                var status = statuses[statusIndex];
                if (status == null || status.Kind != kind || status.Stacks <= 0)
                {
                    continue;
                }

                for (var idIndex = 0; idIndex < requiredSourceSkillNames.Length; idIndex++)
                {
                    if (string.Equals(
                        requiredSourceSkillNames[idIndex],
                        status.SourceSkillName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsAreaLikeSkill(SkillDefinition skill)
        {
            if (skill.RuntimeKind == SkillRuntimeKind.AreaAttack)
            {
                return true;
            }

            if (skill.RuntimeKind != SkillRuntimeKind.SingleAttack
                || !(skill is SingleSkillDefinition single))
            {
                return false;
            }

            return single.HitAllTargets || single.Area.CoverAll || single.Area.Radius > 0f;
        }
    }
}
