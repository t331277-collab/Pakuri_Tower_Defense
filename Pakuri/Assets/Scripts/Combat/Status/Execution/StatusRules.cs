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

    /// <summary><c>StatusCombatRules</c>에 공통으로 적용되는 런타임 규칙을 구현한다.</summary>
    public static class StatusCombatRules
    {
        private const float MinimumActionMultiplier = 0.05f;

        /// <summary>전달된 런타임 입력값을 사용해 <c>Status</c>를 적용한다.</summary>
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

            var chance = ApplicationChance(target, status, source);
            if (chance <= 0f || UnityEngine.Random.value > chance)
            {
                return false;
            }

            var duration = DurationSeconds(status, source);
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ApplicationChance</c> 결과값을 생성해 반환한다.</summary>
        public static float ApplicationChance(
            UnitCombatState target,
            ProjectileStatusHitSpec status,
            UnitCombatState source = null)
        {
            if (status == null || !status.Enabled)
            {
                return 0f;
            }

            var chanceBonus = ConditionalStatusChanceBonus(source, target);
            var chance = Mathf.Clamp01(status.Chance + chanceBonus);
            if (chance <= 0f || target == null || !IsDebuff(status.StatusData))
            {
                return chance;
            }

            return Mathf.Clamp01(chance - AilmentResistanceBonus(target));
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>DurationSeconds</c> 결과값을 생성해 반환한다.</summary>
        private static float DurationSeconds(ProjectileStatusHitSpec status, UnitCombatState source)
        {
            var duration = Mathf.Max(0f, status.DurationSeconds);
            var statusId = status.StatusData.StatusTag;
            if (!string.IsNullOrWhiteSpace(statusId))
            {
                duration += AppliedStatusDurationBonus(source, statusId);
                duration = Mathf.Max(0f, duration);
            }

            return duration;
        }

        /// <summary>전달된 <c>statusData</c> 값을 사용해 <c>Debuff</c> 조건 충족 여부를 반환한다.</summary>
        private static bool IsDebuff(StatusRuntimeData statusData)
        {
            return statusData.Definition.Classification == StatusEffectClassification.Debuff;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ThresholdStatus</c>를 적용한다.</summary>
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

        /// <summary><c>OutgoingAdditionalDamageSpec</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
        internal readonly struct OutgoingAdditionalDamageSpec
        {

            /// <summary><c>OutgoingAdditionalDamageSpec</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
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

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>ModifierMagnitude</c>를 계산한다.</summary>
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

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>Move</c> 실행 가능 여부를 반환한다.</summary>
        public static bool CanMove(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanMove);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>Act</c> 실행 가능 여부를 반환한다.</summary>
        public static bool CanAct(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanAct);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>UseSpecialSkill</c> 실행 가능 여부를 반환한다.</summary>
        public static bool CanUseSpecialSkill(UnitCombatState model)
        {
            return !HasAnyStatus(model, data => !data.CanUseSpecialSkill);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>ActionSpeedMultiplier</c> 결과값을 생성해 반환한다.</summary>
        public static float ActionSpeedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(MinimumActionMultiplier, 1f + SumStacked(model, data => data.Modifiers.ActionSpeedBonus));
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>SpeedMultiplier</c>를 이동시킨다.</summary>
        public static float MoveSpeedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.MoveSpeedBonus));
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>AttackPowerMultiplier</c> 결과값을 생성해 반환한다.</summary>
        public static float AttackPowerMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.AttackPowerBonus));
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>SpellPowerMultiplier</c> 결과값을 생성해 반환한다.</summary>
        public static float SpellPowerMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.SpellPowerBonus));
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>ShieldReceivedMultiplier</c> 결과값을 생성해 반환한다.</summary>
        public static float ShieldReceivedMultiplier(UnitCombatState model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.ShieldReceivedBonus));
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>CriticalChanceBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float CriticalChanceBonus(UnitCombatState model)
        {
            return SumStacked(model, data => data.Modifiers.CritChanceBonusRate);
        }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>CriticalDamageBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float CriticalDamageBonus(UnitCombatState model)
        {
            return SumStacked(model, data => data.Modifiers.CritDamageBonusRate);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>OutgoingDamageBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float OutgoingDamageBonus(UnitCombatState source, DamageAttribute attribute, string sourceSkillId = null)
        {
            return SumStacked(source, data =>
            {
                if (MatchesAttribute(data, attribute)
                    && StatusConditionRules.MatchesSkillRuntimeKinds(
                        data.ConditionalOutgoingSkillRuntimeKindValues,
                        sourceSkillId))
                {
                    return data.Modifiers.DamageBonusRate;
                }

                return 0f;
            });
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>OutgoingAdditionalDamageSpecs</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>IncomingDamageBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float IncomingDamageBonus(UnitCombatState target, UnitCombatState source, DamageAttribute attribute, string sourceSkillId = null)
        {
            return SumStacked(target, data =>
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
            });
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ElementResistMultiplier</c> 결과값을 생성해 반환한다.</summary>
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
                if (data == null || !MatchesAttribute(data, attribute))
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>FlatElementResistReduction</c> 결과값을 생성해 반환한다.</summary>
        public static float FlatElementResistReduction(UnitCombatState target, DamageAttribute attribute)
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

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>CriticalDamageTakenBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float CriticalDamageTakenBonus(UnitCombatState target)
        {
            return SumStacked(target, data => data.CriticalDamageTakenBonus);
        }

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>AilmentResistanceBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float AilmentResistanceBonus(UnitCombatState target)
        {
            return Mathf.Clamp01(SumStacked(target, data => data.AilmentResistanceBonus));
        }

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>CriticalResistanceBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float CriticalResistanceBonus(UnitCombatState target)
        {
            return SumStacked(target, data => data.CriticalResistanceBonus);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConditionalStatusChanceBonus</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>AppliedStatusDurationBonus</c> 결과값을 생성해 반환한다.</summary>
        public static float AppliedStatusDurationBonus(UnitCombatState source, string statusId)
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

        /// <summary>전달된 런타임 입력값을 사용해 소유한 런타임 상태에 <c>AnyStatus</c>가 있는지 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>SumStacked</c> 결과값을 생성해 반환한다.</summary>
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
                if (data == null)
                {
                    continue;
                }

                total += selector(data) * runtime.Stacks;
            }

            return total;
        }

        /// <summary>전달된 <c>runtime</c> 값을 사용해 <c>RuntimeData</c> 결과값을 생성해 반환한다.</summary>
        private static StatusRuntimeData RuntimeData(StatusRuntimeInstance runtime)
        {
            if (runtime == null)
            {
                return null;
            }

            return runtime.SourceData;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesAttribute</c> 조건을 평가하고 결과를 반환한다.</summary>
        private static bool MatchesAttribute(StatusRuntimeData data, DamageAttribute attribute)
        {
            return data != null && data.HasElementModifierTarget && (DamageAttribute)(int)data.ElementModifierTarget == attribute;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesConditionalSourceStatus</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesConditionalTargetStatus</c> 조건을 평가하고 결과를 반환한다.</summary>
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

    /// <summary><c>StatusConditionRules</c>에 공통으로 적용되는 런타임 규칙을 구현한다.</summary>
    static class StatusConditionRules
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesConditionStatus</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesConditionStatus</c> 조건을 평가하고 결과를 반환한다.</summary>
        public static bool MatchesConditionStatus(
            UnitCombatState target,
            StatusConditionGroup[] groups)
        {
            return MatchesConditionStatus(target, groups, Array.Empty<string>());
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesConditionStatus</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesSkillRuntimeKinds</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>MatchesRequiredSourceSkill</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 <c>skill</c> 값을 사용해 <c>AreaLikeSkill</c> 조건 충족 여부를 반환한다.</summary>
        private static bool IsAreaLikeSkill(SkillDefinition skill)
        {
            if (skill.RuntimeKind == SkillRuntimeKind.AreaAttack
                || skill.RuntimeKind == SkillRuntimeKind.Field)
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
