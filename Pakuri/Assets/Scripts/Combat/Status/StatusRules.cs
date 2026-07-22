using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 상태 적용 규칙, 전투 수치 반영, 타입으로 변환된 상태 조건 판정을 담당한다.
 */
namespace Pakuri.InGame
{
    public static class StatusCombatRules
    {
        private const float MinimumActionMultiplier = 0.05f;

        /*
         * 상태 적용 확률과 지속시간을 계산해 대상에게 적용한다.
         */
        public static bool ApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
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

            var chance = ResolveApplicationChance(target, status, source);
            if (chance <= 0f || UnityEngine.Random.value > chance)
            {
                return false;
            }

            var duration = ResolveDurationSeconds(status, source);
            var appliedStatus = manager.ApplyStatus(
                target,
                status.StatusData,
                status.Stacks,
                duration,
                status.MaxStacks,
                status.Permanent,
                status.RefreshDuration,
                source);
            if (appliedStatus == null)
            {
                return false;
            }

            ApplyThresholdStatus(manager, target, status, source);
            return true;
        }

        /*
         * ResolveApplicationChance 결과를 계산해 반환한다.
         */
        public static float ResolveApplicationChance(
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source = null)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chanceBonus = ResolveConditionalStatusChanceBonus(source, target);
            var chance = Mathf.Clamp01(status.Chance + chanceBonus);
            if (chance <= 0f || target == null || !IsDebuff(status.StatusData))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - ResolveAilmentResistanceBonus(target));
        }

        /*
         * ResolveDurationSeconds 결과를 계산해 반환한다.
         */
        private static float ResolveDurationSeconds(ProjectileStatusHitSpec status, UnitCombatState source)
        {
            var duration = Mathf.Max(0f, status.DurationSeconds);
            var statusId = status.StatusData.StatusTag;
            if (!string.IsNullOrWhiteSpace(statusId))
            {
                duration += ResolveAppliedStatusDurationBonus(source, statusId);
                duration = Mathf.Max(0f, duration);
            }

            return duration;
        }

        /*
         * IsDebuff 조건을 만족하는지 확인한다.
         */
        private static bool IsDebuff(StatusRuntimeData statusData)
        {
            return statusData.Definition.Classification == StatusEffectClassification.Debuff;
        }

        /*
         * ApplyThresholdStatus 처리를 대상에 적용한다.
         */
        private static void ApplyThresholdStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source)
        {
            if (target.Statuses == null
                || status.ThresholdStatusSpec == null
                || !status.ThresholdStatusSpec.Enabled
                || status.ThresholdSourceStatusKind == StatusEffectKind.None
                || status.ThresholdSourceMinStacks <= 0)
            {
                return;
            }

            if (target.Statuses.GetStacks(status.ThresholdSourceStatusKind) < status.ThresholdSourceMinStacks)
            {
                return;
            }

            var thresholdStatus = status.ThresholdStatusSpec;
            manager.ApplyStatus(
                target,
                thresholdStatus.StatusData,
                thresholdStatus.Stacks,
                thresholdStatus.DurationSeconds,
                thresholdStatus.MaxStacks,
                thresholdStatus.Permanent,
                thresholdStatus.RefreshDuration,
                source);
        }

        /*
         * 주는 추가 피해 설정에 필요한 값을 보관한다.
         */
        internal readonly struct OutgoingAdditionalDamageSpec
        {
            /*
             * 주는 추가 피해 설정에 필요한 값을 초기화한다.
             */
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

        /*
         * 기본 보정값과 상태 중첩을 합쳐 최종 보정량을 계산한다.
         */
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
                + Mathf.Abs(data.CriticalResistanceBonus)
                + Mathf.Abs(data.ElementResistReduction)
                + Mathf.Abs(data.FlatElementResistReduction)
                + Mathf.Abs(data.ElementDamageTakenBonus)
                + Mathf.Abs(data.ConditionalDamageTakenBonus)
                + Mathf.Abs(data.OutgoingAdditionalDamageMultiplier);
        }

        /*
         * 이동을 가능한 상태인지 확인한다.
         */
        public static bool CanMove(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanMove);
        }

        /*
         * 현재 상태에서 행동할 수 있는지 확인한다.
         */
        public static bool CanAct(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanAct);
        }

        /*
         * 현재 상태에서 특수 스킬을 사용할 수 있는지 확인한다.
         */
        public static bool CanUseSpecialSkill(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanUseSpecialSkill);
        }

        /*
         * 행동 속도 배율을 결정한다.
         */
        public static float ResolveActionSpeedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(MinimumActionMultiplier, 1f + SumStacked(model, data => data.Modifiers.ActionSpeedBonus));
        }

        /*
         * 이동 속도 배율을 결정한다.
         */
        public static float ResolveMoveSpeedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.MoveSpeedBonus));
        }

        /*
         * 공격 능력치 배율을 결정한다.
         */
        public static float ResolveAttackPowerMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.AttackPowerBonus));
        }

        /*
         * 주문 능력치 배율을 결정한다.
         */
        public static float ResolveSpellPowerMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.SpellPowerBonus));
        }

        /*
         * 보호막 받는 배율을 결정한다.
         */
        public static float ResolveShieldReceivedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.ShieldReceivedBonus));
        }

        /*
         * 치명타 확률 보너스를 결정한다.
         */
        public static float ResolveCriticalChanceBonus(UnitCombatState model)
        {
            return SumStacked(model, data => data.Modifiers.CritChanceBonusRate);
        }

        /*
         * 치명타 피해 보너스를 결정한다.
         */
        public static float ResolveCriticalDamageBonus(UnitCombatState model)
        {
            return SumStacked(model, data => data.Modifiers.CritDamageBonusRate);
        }

        /*
         * 주는 피해 배율을 결정한다.
         */
        public static float ResolveOutgoingDamageMultiplier(UnitCombatState source, DamageAttribute attribute, string sourceSkillId = null)
        {
            return Mathf.Max(0f, 1f + SumStacked(source, data =>
            {
                if (MatchesAttribute(data, attribute)
                    && StatusConditionRules.MatchesSkillRuntimeKinds(
                        data.ConditionalOutgoingSkillRuntimeKindValues,
                        sourceSkillId))
                {
                    return data.Modifiers.DamageBonusRate;
                }

                return 0f;
            }));
        }

        /*
         * 주는 추가 피해 설정을 결정한다.
         */
        internal static List<OutgoingAdditionalDamageSpec> ResolveOutgoingAdditionalDamageSpecs(UnitCombatState source, DamageAttribute triggerAttribute)
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

                var data = ResolveRuntimeData(runtime);
                if (data == null
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

        /*
         * 받는 피해 배율을 결정한다.
         */
        public static float ResolveIncomingDamageMultiplier(UnitCombatState target, UnitCombatState source, DamageAttribute attribute, string sourceSkillId = null)
        {
            return Mathf.Max(0f, 1f + SumStacked(target, data =>
            {
                var runtimeKindMatches = StatusConditionRules.MatchesSkillRuntimeKinds(data.ConditionalIncomingSkillRuntimeKindValues, sourceSkillId);
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

                return bonus;
            }));
        }

        /*
         * 원소 저항 감소를 결정한다.
         */
        public static float ResolveElementResistReduction(UnitCombatState target, DamageAttribute attribute)
        {
            return Mathf.Clamp01(SumStacked(target, data =>
            {
                if (MatchesAttribute(data, attribute))
                {
                    return data.ElementResistReduction;
                }

                return 0f;
            }));
        }

        /*
         * 고정 원소 저항 감소를 결정한다.
         */
        public static float ResolveFlatElementResistReduction(UnitCombatState target, DamageAttribute attribute)
        {
            return Mathf.Max(0f, SumStacked(target, data =>
            {
                if (MatchesAttribute(data, attribute))
                {
                    return data.FlatElementResistReduction;
                }

                return 0f;
            }));
        }

        /*
         * 치명타 피해 받는 보너스를 결정한다.
         */
        public static float ResolveCriticalDamageTakenBonus(UnitCombatState target)
        {
            return SumStacked(target, data => data.CriticalDamageTakenBonus);
        }

        /*
         * 상태 이상 저항 보너스를 결정한다.
         */
        public static float ResolveAilmentResistanceBonus(UnitCombatState target)
        {
            return Mathf.Clamp01(SumStacked(target, data => data.AilmentResistanceBonus));
        }

        /*
         * 치명타 저항 보너스를 결정한다.
         */
        public static float ResolveCriticalResistanceBonus(UnitCombatState target)
        {
            return SumStacked(target, data => data.CriticalResistanceBonus);
        }

        /*
         * 조건부 상태 확률 보너스를 결정한다.
         */
        public static float ResolveConditionalStatusChanceBonus(UnitCombatState source, UnitCombatState target)
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

        /*
         * 적용된 상태 지속시간 보너스를 결정한다.
         */
        public static float ResolveAppliedStatusDurationBonus(UnitCombatState source, string statusId)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return 0f;
            }

            return SumStacked(source, data =>
            {
                if (string.Equals(
                    data.AppliedStatusDurationBonusStatusId,
                    statusId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return data.AppliedStatusDurationBonus;
                }

                return 0f;
            });
        }

        /*
         * 하나 이상의 상태를 보유하고 있는지 확인한다.
         */
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

                var data = ResolveRuntimeData(runtime);
                if (data != null && predicate(data))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 조건에 맞는 상태 보정값을 중첩 수만큼 합산한다.
         */
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

                var data = ResolveRuntimeData(runtime);
                if (data == null)
                {
                    continue;
                }

                total += selector(data) * runtime.Stacks;
            }

            return total;
        }

        /*
         * 런타임 데이터를 결정한다.
         */
        private static StatusRuntimeData ResolveRuntimeData(StatusRuntimeInstance runtime)
        {
            if (runtime == null)
            {
                return null;
            }

            return runtime.SourceData;
        }

        /*
         * 피해 속성이 상태 효과 조건과 일치하는지 확인한다.
         */
        private static bool MatchesAttribute(StatusRuntimeData data, DamageAttribute attribute)
        {
            return data != null && data.HasElementModifierTarget && (DamageAttribute)(int)data.ElementModifierTarget == attribute;
        }

        /*
         * 출처 유닛의 상태가 조건을 만족하는지 확인한다.
         */
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

        /*
         * 대상 유닛의 상태가 조건을 만족하는지 확인한다.
         */
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

    /*
     * 변환된 상태 조건과 스킬 실행 종류 조건이 현재 전투 상태와 맞는지 확인한다.
     */
    static class StatusConditionRules
    {
        /*
         * MatchesConditionStatus 조건을 만족하는지 확인한다.
         */
        public static bool MatchesConditionStatus(
            UnitCombatState target,
            StatusConditionGroup[] groups,
            string[] requiredSourceSkillIds)
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
                        || !MatchesRequiredSourceSkill(target, requirement.Kind, requiredSourceSkillIds))
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

        /*
         * MatchesConditionStatus 조건을 만족하는지 확인한다.
         */
        public static bool MatchesConditionStatus(
            UnitCombatState target,
            StatusConditionGroup[] groups)
        {
            return MatchesConditionStatus(target, groups, Array.Empty<string>());
        }

        /*
         * MatchesConditionStatus 조건을 만족하는지 확인한다.
         */
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

        /*
         * MatchesSkillRuntimeKinds 조건을 만족하는지 확인한다.
         */
        public static bool MatchesSkillRuntimeKinds(
            SkillRuntimeKindCondition[] conditions,
            string sourceSkillId)
        {
            if (conditions == null || conditions.Length == 0)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(sourceSkillId))
            {
                return false;
            }

            var catalog = GameDataLoader.CurrentCatalog;
            if (catalog == null
                || !catalog.TryGetData(sourceSkillId, out SkillDefinition skill)
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

        /*
         * MatchesRequiredSourceSkill 조건을 만족하는지 확인한다.
         */
        private static bool MatchesRequiredSourceSkill(
            UnitCombatState target,
            StatusEffectKind kind,
            string[] requiredSourceSkillIds)
        {
            if (requiredSourceSkillIds == null || requiredSourceSkillIds.Length == 0)
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

                for (var idIndex = 0; idIndex < requiredSourceSkillIds.Length; idIndex++)
                {
                    if (string.Equals(
                        requiredSourceSkillIds[idIndex],
                        status.SourceSkillId,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /*
         * IsAreaLikeSkill 조건을 만족하는지 확인한다.
         */
        private static bool IsAreaLikeSkill(SkillDefinition skill)
        {
            if (skill.RuntimeKind == SkillRuntimeKind.AreaAttack
                || skill.RuntimeKind == SkillRuntimeKind.Field)
            {
                return true;
            }

            if (skill.RuntimeKind != SkillRuntimeKind.SingleAttack)
            {
                return false;
            }

            return string.Equals(skill.HitTargetCount, "global", StringComparison.OrdinalIgnoreCase)
                || skill.Radius > 0f;
        }
    }
}
