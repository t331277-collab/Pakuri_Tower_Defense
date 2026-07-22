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
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */,
            UnitCombatState source = null /* 효과를 발생시킨 유닛 */)
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
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */,
            UnitCombatState source = null /* 효과를 발생시킨 유닛 */)
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
        private static float ResolveDurationSeconds(ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */, UnitCombatState source /* 효과를 발생시킨 유닛 */)
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
        private static bool IsDebuff(StatusRuntimeData statusData /* 상태 효과 실행 데이터 */)
        {
            return statusData.Definition.Classification == StatusEffectClassification.Debuff;
        }

        /*
         * ApplyThresholdStatus 처리를 대상에 적용한다.
         */
        private static void ApplyThresholdStatus(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */)
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
            public OutgoingAdditionalDamageSpec(float multiplier /* 값에 곱할 배율 */, DamageAttribute triggerAttribute /* 트리거 속성 */, DamageAttribute damageAttribute /* 적용할 피해 속성 */)
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
        public static float ComputeModifierMagnitude(StatusRuntimeData data /* 처리할 실행 데이터 */)
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
        public static bool CanMove(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return !HasAnyStatus(model, data => !data.CanMove);
        }

        /*
         * 현재 상태에서 행동할 수 있는지 확인한다.
         */
        public static bool CanAct(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return !HasAnyStatus(model, data => !data.CanAct);
        }

        /*
         * 현재 상태에서 특수 스킬을 사용할 수 있는지 확인한다.
         */
        public static bool CanUseSpecialSkill(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return !HasAnyStatus(model, data => !data.CanUseSpecialSkill);
        }

        /*
         * 행동 속도 배율을 결정한다.
         */
        public static float ResolveActionSpeedMultiplier(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return Mathf.Max(MinimumActionMultiplier, 1f + SumStacked(model, data => data.Modifiers.ActionSpeedBonus));
        }

        /*
         * 이동 속도 배율을 결정한다.
         */
        public static float ResolveMoveSpeedMultiplier(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.MoveSpeedBonus));
        }

        /*
         * 공격 능력치 배율을 결정한다.
         */
        public static float ResolveAttackPowerMultiplier(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.AttackPowerBonus));
        }

        /*
         * 주문 능력치 배율을 결정한다.
         */
        public static float ResolveSpellPowerMultiplier(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.SpellPowerBonus));
        }

        /*
         * 보호막 받는 배율을 결정한다.
         */
        public static float ResolveShieldReceivedMultiplier(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.ShieldReceivedBonus));
        }

        /*
         * 치명타 확률 보너스를 결정한다.
         */
        public static float ResolveCriticalChanceBonus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return SumStacked(model, data => data.Modifiers.CritChanceBonusRate);
        }

        /*
         * 치명타 피해 보너스를 결정한다.
         */
        public static float ResolveCriticalDamageBonus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return SumStacked(model, data => data.Modifiers.CritDamageBonusRate);
        }

        /*
         * 주는 피해 배율을 결정한다.
         */
        public static float ResolveOutgoingDamageMultiplier(UnitCombatState source /* 효과를 발생시킨 유닛 */, DamageAttribute attribute /* 피해 속성 */, string sourceSkillId = null /* 효과를 발생시킨 스킬 식별자 */)
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
        internal static List<OutgoingAdditionalDamageSpec> ResolveOutgoingAdditionalDamageSpecs(UnitCombatState source /* 효과를 발생시킨 유닛 */, DamageAttribute triggerAttribute /* 트리거 속성 */)
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
        public static float ResolveIncomingDamageMultiplier(UnitCombatState target /* 효과를 받을 대상 유닛 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, DamageAttribute attribute /* 피해 속성 */, string sourceSkillId = null /* 효과를 발생시킨 스킬 식별자 */)
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
        public static float ResolveElementResistReduction(UnitCombatState target /* 효과를 받을 대상 유닛 */, DamageAttribute attribute /* 피해 속성 */)
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
        public static float ResolveFlatElementResistReduction(UnitCombatState target /* 효과를 받을 대상 유닛 */, DamageAttribute attribute /* 피해 속성 */)
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
        public static float ResolveCriticalDamageTakenBonus(UnitCombatState target /* 효과를 받을 대상 유닛 */)
        {
            return SumStacked(target, data => data.CriticalDamageTakenBonus);
        }

        /*
         * 상태 이상 저항 보너스를 결정한다.
         */
        public static float ResolveAilmentResistanceBonus(UnitCombatState target /* 효과를 받을 대상 유닛 */)
        {
            return Mathf.Clamp01(SumStacked(target, data => data.AilmentResistanceBonus));
        }

        /*
         * 치명타 저항 보너스를 결정한다.
         */
        public static float ResolveCriticalResistanceBonus(UnitCombatState target /* 효과를 받을 대상 유닛 */)
        {
            return SumStacked(target, data => data.CriticalResistanceBonus);
        }

        /*
         * 조건부 상태 확률 보너스를 결정한다.
         */
        public static float ResolveConditionalStatusChanceBonus(UnitCombatState source /* 효과를 발생시킨 유닛 */, UnitCombatState target /* 효과를 받을 대상 유닛 */)
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
        public static float ResolveAppliedStatusDurationBonus(UnitCombatState source /* 효과를 발생시킨 유닛 */, string statusId /* 상태 효과 식별자 */)
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
        private static bool HasAnyStatus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, System.Func<StatusRuntimeData, bool> predicate /* 판정 조건 */)
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
        private static float SumStacked(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, System.Func<StatusRuntimeData, float> selector /* 선택 함수 */)
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
        private static StatusRuntimeData ResolveRuntimeData(StatusRuntimeInstance runtime /* 실행 중인 스킬 정보 */)
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
        private static bool MatchesAttribute(StatusRuntimeData data /* 처리할 실행 데이터 */, DamageAttribute attribute /* 피해 속성 */)
        {
            return data != null && data.HasElementModifierTarget && (DamageAttribute)(int)data.ElementModifierTarget == attribute;
        }

        /*
         * 출처 유닛의 상태가 조건을 만족하는지 확인한다.
         */
        private static bool MatchesConditionalSourceStatus(UnitCombatState source /* 효과를 발생시킨 유닛 */, StatusRuntimeData data /* 처리할 실행 데이터 */)
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
        private static bool MatchesConditionalTargetStatus(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusRuntimeData data /* 처리할 실행 데이터 */)
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
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            StatusConditionGroup[] groups /* 그룹 목록 */,
            string[] requiredSourceSkillIds /* 필수 발생 원본 스킬 식별자 목록 여부 */)
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
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            StatusConditionGroup[] groups /* 그룹 목록 */)
        {
            return MatchesConditionStatus(target, groups, Array.Empty<string>());
        }

        /*
         * MatchesConditionStatus 조건을 만족하는지 확인한다.
         */
        public static bool MatchesConditionStatus(
            StatusRuntimeInstance status /* 실행 중인 상태 효과 */,
            StatusConditionGroup[] groups /* 그룹 목록 */)
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
            SkillRuntimeKindCondition[] conditions /* 조건 목록 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */)
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
                || !catalog.TryGetData(sourceSkillId, out SkillSourceDefinition skill)
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
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            StatusEffectKind kind /* 처리할 종류 */,
            string[] requiredSourceSkillIds /* 필수 발생 원본 스킬 식별자 목록 여부 */)
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
        private static bool IsAreaLikeSkill(SkillSourceDefinition skill /* 처리할 스킬 정의 */)
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

/*
 * 기본 상태 설정에 선택지의 확률, 중첩, 지속시간과 능력치 보정을 반영한다.
 */
static class SkillStatus
{
    /*
     * 추가 효과 정의의 상태 데이터와 현재 강화 정보를 적중 설정으로 만든다.
     */
    public static ProjectileStatusHitSpec ResolveEffectStatusSpec(
        SkillEffectDefinition effect /* 적용할 추가 효과 */,
        SkillExecutionData skillData = null /* 현재 스킬 강화 정보 */,
        bool scaleDurationWithSkillData = false /* 스킬 지속시간 보정 적용 여부 */)
    {
        var statusData = ResolveStatusData(effect.CompiledStatusData, effect.CompiledStatusData.Kind, skillData);
        var duration = statusData.Duration;
        if (skillData != null)
        {
            var durationBonus = skillData.ResolveStatusDurationBonus(statusData.StatusTag);
            if (!Mathf.Approximately(durationBonus, 0f))
            {
                duration = Mathf.Max(0f, duration + durationBonus);
            }

            if (scaleDurationWithSkillData)
            {
                duration = duration * Mathf.Max(0f, skillData.DurationMultiplier) + skillData.DurationBonus;
            }
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = statusData.Kind,
            StatusData = statusData,
            Chance = Mathf.Clamp01(effect.StatusChance),
            Stacks = effect.StatusStackAmount,
            DurationSeconds = duration,
            MaxStacks = statusData.MaxStacks,
            Permanent = statusData.Permanent,
            RefreshDuration = true
        };
    }

    /*
     * 추가 효과에 지정된 상태를 대상에게 적용한다.
     */
    public static bool ApplyEffect(
        SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
        SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
        SkillEffectDefinition effect /* 적용할 추가 효과 */,
        Vector2 defaultCenter /* 기본 효과 중심 */,
        bool scaleDurationWithSkillData /* 스킬 지속시간 보정 적용 여부 */)
    {
        var statusSpec = ResolveEffectStatusSpec(effect, skillData, scaleDurationWithSkillData);
        if (context == null || context.CombatManager == null || statusSpec == null || !statusSpec.Enabled)
        {
            return false;
        }

        var targeting = SkillTargeting.BuildEffectTargeting(effect);
        var targets = SkillTargeting.ResolveEffectTargets(context, effect, targeting);
        List<CombatUnitEntry> visualTargets = null;
        if (effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets)
        {
            visualTargets = new List<CombatUnitEntry>();
        }

        var applied = false;
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target == null || !target.IsAlive || target.Model == null || !SkillTargeting.MatchesEffectTarget(target.Model, effect))
            {
                continue;
            }

            if (statusSpec.StatusData != null && statusSpec.StatusData.Kind == StatusEffectKind.Shield)
            {
                context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    statusSpec.StatusData,
                    ResolveEffectShieldAmount(context.Caster, effect, skillData),
                    statusSpec.DurationSeconds,
                    statusSpec.Stacks,
                    statusSpec.MaxStacks,
                    statusSpec.Permanent,
                    statusSpec.RefreshDuration,
                    context.Caster);
            }
            else if (!StatusCombatRules.ApplyStatus(context.CombatManager, target.Model, statusSpec, context.Caster))
            {
                continue;
            }

            if (visualTargets != null)
            {
                visualTargets.Add(target);
            }
            applied = true;
        }

        if (!applied || context.CombatManager.Effects == null)
        {
            return applied;
        }

        if (visualTargets != null)
        {
            context.CombatManager.Effects.ShowFollowingSkillEffects(effect, visualTargets, statusSpec.DurationSeconds);
        }
        else
        {
            var center = SkillTargeting.ResolveEffectCenter(context, effect, targeting, defaultCenter);
            context.CombatManager.Effects.ShowTimedSkillEffect(effect, center, 1f);
        }
        return true;
    }

    /*
     * 추가 효과 대상의 지정 상태 지속시간을 늘린다.
     */
    public static bool ExtendEffectDuration(
        SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
        SkillEffectDefinition effect /* 적용할 추가 효과 */)
    {
        if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || effect == null)
        {
            return false;
        }

        var duration = Mathf.Max(0f, effect.StatusDurationSeconds);
        if (duration <= 0f)
        {
            return false;
        }

        var targeting = SkillTargeting.BuildEffectTargeting(effect);
        var targets = SkillTargeting.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
        var extended = false;
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target != null && target.IsAlive && target.Model != null)
            {
                extended = context.CombatManager.ExtendStatusDuration(target.Model, effect.StatusKind, duration) || extended;
            }
        }
        return extended;
    }

    /*
     * 지속 패시브 상태를 적용할 수 있는 대상 목록을 반환한다.
     */
    public static List<CombatUnitEntry> ResolvePassiveEffectTargets(
        SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
        SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
        SkillEffectDefinition effect /* 적용할 추가 효과 */)
    {
        var resolved = new List<CombatUnitEntry>();
        if (context == null
            || effect == null
            || effect.EffectKind != SkillMultiEffectKind.Status
            || !SkillRequirement.CanRunEffect(context, effect))
        {
            return resolved;
        }

        var targeting = SkillTargeting.BuildEffectTargeting(effect);
        var targets = SkillTargeting.ResolveEffectTargets(context, effect, targeting);
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target != null && target.IsAlive && target.Model != null && SkillTargeting.MatchesEffectTarget(target.Model, effect))
            {
                resolved.Add(target);
            }
        }
        return resolved;
    }

    /*
     * 패시브가 유지되는 동안 대상에게 영구 상태를 적용한다.
     */
    public static bool ApplyPersistentEffect(
        SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
        SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
        SkillEffectDefinition effect /* 적용할 추가 효과 */,
        CombatUnitEntry target /* 효과를 받을 대상 */,
        Vector2 defaultCenter /* 기본 효과 중심 */)
    {
        if (context == null
            || context.CombatManager == null
            || effect == null
            || target == null
            || !target.IsAlive
            || target.Model == null
            || !SkillTargeting.MatchesEffectTarget(target.Model, effect))
        {
            return false;
        }

        var statusSpec = ResolveEffectStatusSpec(effect, skillData);
        if (statusSpec == null || !statusSpec.Enabled || statusSpec.StatusData == null)
        {
            return false;
        }

        var visualDuration = statusSpec.DurationSeconds;
        statusSpec.Permanent = true;
        statusSpec.DurationSeconds = 0f;
        statusSpec.RefreshDuration = false;
        var applied = false;
        if (statusSpec.StatusData.Kind == StatusEffectKind.Shield)
        {
            var shield = context.CombatManager.ApplyShieldStatus(
                target.Model,
                statusSpec.StatusData,
                ResolveEffectShieldAmount(context.Caster, effect, skillData),
                statusSpec.DurationSeconds,
                statusSpec.Stacks,
                statusSpec.MaxStacks,
                statusSpec.Permanent,
                statusSpec.RefreshDuration,
                context.Caster);
            applied = shield != null;
        }
        else
        {
            applied = StatusCombatRules.ApplyStatus(context.CombatManager, target.Model, statusSpec, context.Caster);
        }

        if (!applied || context.CombatManager.Effects == null)
        {
            return applied;
        }

        if (effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets)
        {
            context.CombatManager.Effects.ShowFollowingSkillEffects(effect, new CombatUnitEntry[] { target }, visualDuration);
        }
        else
        {
            var targeting = SkillTargeting.BuildEffectTargeting(effect);
            var center = SkillTargeting.ResolveEffectCenter(context, effect, targeting, defaultCenter);
            context.CombatManager.Effects.ShowTimedSkillEffect(effect, center, 1f);
        }
        return true;
    }

    /*
     * 추가 보호막 효과의 기본값과 시전자 능력치 계수를 계산한다.
     */
    private static float ResolveEffectShieldAmount(
        UnitCombatState caster /* 스킬을 사용하는 유닛 */,
        SkillEffectDefinition effect /* 적용할 추가 효과 */,
        SkillExecutionData skillData /* 현재 스킬 강화 정보 */)
    {
        var useSpellPower = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient);
        var power = 0f;
        if (caster != null && caster.Stats != null)
        {
            if (useSpellPower)
            {
                power = caster.Stats.SpellPower * StatusCombatRules.ResolveSpellPowerMultiplier(caster);
            }
            else
            {
                power = caster.Stats.AttackPower * StatusCombatRules.ResolveAttackPowerMultiplier(caster);
            }
        }

        var coefficient = effect.AttackPowerCoefficient;
        if (useSpellPower)
        {
            coefficient = effect.SpellPowerCoefficient;
        }

        var amount = (effect.BaseDamage + power * coefficient) * Mathf.Max(0f, effect.DamageMultiplier);
        if (skillData != null)
        {
            amount = (amount + skillData.BaseDamageBonus) * Mathf.Max(0f, skillData.ShieldAmountMultiplier);
        }
        return Mathf.Max(0f, amount);
    }

    /*
     * 스킬의 기본 상태 설정과 실행 데이터 보정을 합쳐 투사체 적중 설정을 만든다.
     */
    public static ProjectileStatusHitSpec ResolveStatusSpec(
        StatusApplicationSpec baseStatus /* 기본 상태 효과 */,
        SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
    {
        StatusRuntimeData statusData = null;
        if (baseStatus != null)
        {
            statusData = baseStatus.Status;
        }

        if (statusData == null)
        {
            return null;
        }

        var kind = statusData.Kind;
        var stacks = 1;
        var chance = 1f;
        var refreshDuration = true;
        if (baseStatus != null)
        {
            stacks = Math.Max(0, baseStatus.Stacks);
            chance = Mathf.Clamp01(baseStatus.Chance);
            refreshDuration = baseStatus.RefreshDuration;
        }

        if (snapshot != null)
        {
            chance = Mathf.Clamp01(chance + snapshot.StatusChanceBonus);
            if (snapshot.HasStatusStacksSet)
            {
                stacks = Math.Max(0, snapshot.StatusStacksSet);
            }
            else
            {
                stacks = Math.Max(0, stacks + snapshot.StatusStacksBonus);
            }
        }

        if (stacks <= 0 || chance <= 0f)
        {
            return null;
        }

        if (statusData == null || statusData.Kind != kind)
        {
            statusData = StatusRuntimeCompiler.Create(kind, null);
        }

        var resolvedStatusData = ResolveStatusData(statusData, kind, snapshot);
        var duration = resolvedStatusData.Duration;
        var maxStacks = resolvedStatusData.MaxStacks;
        var maxStacksBonus = ResolveStatusMaxStacksBonus(snapshot, resolvedStatusData);
        if (maxStacksBonus != 0)
        {
            maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
        }

        var permanent = resolvedStatusData.Permanent;
        if (snapshot != null
            && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
        {
            duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            if (duration > 0f)
            {
                permanent = false;
            }
        }

        var durationBonus = ResolveStatusDurationBonus(snapshot, resolvedStatusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
            if (duration > 0f)
            {
                permanent = false;
            }
        }

        var thresholdStatusKind = StatusEffectKind.None;
        var thresholdStatusMinStacks = 0;
        if (snapshot != null)
        {
            thresholdStatusKind = snapshot.ThresholdStatusKind;
            thresholdStatusMinStacks = snapshot.ThresholdStatusMinStacks;
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = resolvedStatusData,
            Chance = chance,
            Stacks = stacks,
            DurationSeconds = duration,
            MaxStacks = maxStacks,
            Permanent = permanent,
            RefreshDuration = refreshDuration,
            ThresholdSourceStatusKind = thresholdStatusKind,
            ThresholdSourceMinStacks = thresholdStatusMinStacks,
            ThresholdStatusSpec = ResolveThresholdStatusSpec(snapshot)
        };
    }

    /*
     * 상태 종류와 중첩 수만으로 즉시 적용할 상태 적중 설정을 만든다.
     */
    public static ProjectileStatusHitSpec CreateDirectStatusSpec(
        StatusEffectKind kind /* 처리할 종류 */,
        int stacks /* 중첩 수 */,
        SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
    {
        if (kind == StatusEffectKind.None || stacks <= 0)
        {
            return null;
        }

        var statusData = StatusRuntimeCompiler.Create(kind, null);
        statusData = ResolveStatusData(statusData, kind, snapshot);
        var duration = statusData.Duration;
        var durationBonus = ResolveStatusDurationBonus(snapshot, statusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
        }

        var maxStacks = statusData.MaxStacks;
        var maxStacksBonus = ResolveStatusMaxStacksBonus(snapshot, statusData);
        if (maxStacksBonus != 0)
        {
            maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = statusData,
            Chance = 1f,
            Stacks = stacks,
            DurationSeconds = duration,
            MaxStacks = maxStacks,
            Permanent = statusData.Permanent && duration <= 0f,
            RefreshDuration = true
        };
    }

    /*
     * 실행 데이터의 상태 능력치 보너스를 복사한 상태 데이터에 적용한다.
     */
    public static StatusRuntimeData ResolveStatusData(
        StatusRuntimeData statusData /* 상태 효과 실행 데이터 */,
        StatusEffectKind kind /* 처리할 종류 */,
        SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
    {
        if (snapshot == null)
        {
            return statusData;
        }

        var actionSpeedBonus = snapshot.ResolveStatusActionSpeedBonus(statusData.StatusTag);
        var hasActionSpeedBonus = !Mathf.Approximately(actionSpeedBonus, 0f);
        var hasOverride = snapshot.HasStatusElementDamageTakenBonus
            || snapshot.HasStatusCriticalDamageTakenBonus
            || snapshot.HasStatusAilmentResistanceBonus
            || snapshot.HasStatusDamageBonusRate
            || snapshot.HasStatusShieldReceivedBonus
            || snapshot.HasStatusCriticalChanceBonus
            || snapshot.HasStatusDamageTakenBonus
            || snapshot.HasStatusFlatElementResistReduction
            || snapshot.HasStatusConditionalDamageTakenBonus
            || snapshot.HasStatusAttackPowerBonus
            || hasActionSpeedBonus;
        if (!hasOverride)
        {
            return statusData;
        }

        var resolvedStatus = statusData.Clone();
        if (snapshot.HasStatusElementDamageTakenBonus)
        {
            resolvedStatus.ElementDamageTakenBonus += snapshot.StatusElementDamageTakenBonus;
        }

        if (snapshot.HasStatusCriticalDamageTakenBonus)
        {
            resolvedStatus.CriticalDamageTakenBonus += snapshot.StatusCriticalDamageTakenBonus;
        }

        if (snapshot.HasStatusAilmentResistanceBonus)
        {
            resolvedStatus.AilmentResistanceBonus += snapshot.StatusAilmentResistanceBonus;
        }

        if (snapshot.HasStatusDamageBonusRate)
        {
            resolvedStatus.Modifiers.DamageBonusRate += snapshot.StatusDamageBonusRate;
        }

        if (snapshot.HasStatusShieldReceivedBonus)
        {
            resolvedStatus.Modifiers.ShieldReceivedBonus += snapshot.StatusShieldReceivedBonus;
        }

        if (snapshot.HasStatusCriticalChanceBonus)
        {
            resolvedStatus.Modifiers.CritChanceBonusRate += snapshot.StatusCriticalChanceBonus;
        }

        if (snapshot.HasStatusDamageTakenBonus)
        {
            resolvedStatus.DamageTakenBonus += snapshot.StatusDamageTakenBonus;
        }

        if (snapshot.HasStatusFlatElementResistReduction)
        {
            resolvedStatus.FlatElementResistReduction += snapshot.StatusFlatElementResistReduction;
        }

        if (snapshot.HasStatusConditionalDamageTakenBonus)
        {
            resolvedStatus.ConditionalSourceStatusKind = snapshot.StatusConditionalSourceStatusKind;
            resolvedStatus.ConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus;
        }

        if (hasActionSpeedBonus)
        {
            resolvedStatus.Modifiers.ActionSpeedBonus += actionSpeedBonus;
        }

        if (snapshot.HasStatusAttackPowerBonus)
        {
            resolvedStatus.Modifiers.AttackPowerBonus += snapshot.StatusAttackPowerBonus;
        }

        return resolvedStatus;
    }

    /*
     * 상태 태그에 연결된 실행 데이터 지속시간 보너스를 반환한다.
     */
    private static float ResolveStatusDurationBonus(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, StatusRuntimeData statusData /* 상태 효과 실행 데이터 */)
    {
        if (snapshot == null)
        {
            return 0f;
        }

        return snapshot.ResolveStatusDurationBonus(statusData.StatusTag);
    }

    /*
     * 상태 태그에 연결된 실행 데이터 최대 중첩 보너스를 반환한다.
     */
    private static int ResolveStatusMaxStacksBonus(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, StatusRuntimeData statusData /* 상태 효과 실행 데이터 */)
    {
        if (snapshot == null)
        {
            return 0;
        }

        return snapshot.ResolveStatusMaxStacksBonus(statusData.StatusTag);
    }

    /*
     * 임계 중첩에 도달했을 때 추가로 적용할 상태 설정을 만든다.
     */
    private static ProjectileStatusHitSpec ResolveThresholdStatusSpec(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
    {
        if (snapshot == null || snapshot.ThresholdApplyStatusKind == StatusEffectKind.None)
        {
            return null;
        }

        var kind = snapshot.ThresholdApplyStatusKind;
        var statusData = StatusRuntimeCompiler.Create(kind, null);
        var duration = statusData.Duration;
        var durationBonus = ResolveStatusDurationBonus(snapshot, statusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = statusData,
            Chance = 1f,
            Stacks = statusData.BaseStackAmount,
            DurationSeconds = duration,
            MaxStacks = statusData.MaxStacks,
            Permanent = statusData.Permanent && duration <= 0f,
            RefreshDuration = true
        };
    }
}
}
