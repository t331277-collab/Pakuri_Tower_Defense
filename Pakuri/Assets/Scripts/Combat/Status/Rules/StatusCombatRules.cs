using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

/*
 * 유닛에 적용된 상태를 이동·행동·공격·피해·저항 수치에 반영한다.
 */
namespace Pakuri.InGame
{
    public static class StatusCombatRules
    {
        private const float MinimumActionMultiplier = 0.05f;

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
                MatchesAttribute(data, attribute) && StatusConditionRules.MatchesSkillRuntimeKinds(data.ConditionalOutgoingSkillRuntimeKinds, sourceSkillId)
                    ? data.Modifiers.DamageBonusRate
                    : 0f));
        }

        /*
         * 주는 추가 피해 설정을 결정한다.
         */
        internal static List<OutgoingAdditionalDamageSpec> ResolveOutgoingAdditionalDamageSpecs(UnitCombatState source, DamageAttribute triggerAttribute)
        {
            var results = new List<OutgoingAdditionalDamageSpec>();
            var statuses = source != null && source.Statuses != null ? source.Statuses.ActiveStatuses : null;
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
                var runtimeKindMatches = StatusConditionRules.MatchesSkillRuntimeKinds(data.ConditionalIncomingSkillRuntimeKinds, sourceSkillId);
                var bonus = runtimeKindMatches ? data.DamageTakenBonus : 0f;
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
            return Mathf.Clamp01(SumStacked(target, data => MatchesAttribute(data, attribute) ? data.ElementResistReduction : 0f));
        }

        /*
         * 고정 원소 저항 감소를 결정한다.
         */
        public static float ResolveFlatElementResistReduction(UnitCombatState target, DamageAttribute attribute)
        {
            return Mathf.Max(0f, SumStacked(target, data => MatchesAttribute(data, attribute) ? data.FlatElementResistReduction : 0f));
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
            return SumStacked(source, data => MatchesConditionalTargetStatus(target, data) ? data.ConditionalStatusChanceBonus : 0f);
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
                string.Equals(data.AppliedStatusDurationBonusStatusId, statusId, StringComparison.OrdinalIgnoreCase)
                    ? data.AppliedStatusDurationBonus
                    : 0f);
        }

        /*
         * 하나 이상의 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasAnyStatus(UnitCombatState model, System.Func<StatusRuntimeData, bool> predicate)
        {
            var statuses = model != null && model.Statuses != null ? model.Statuses.ActiveStatuses : null;
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
            var statuses = model != null && model.Statuses != null ? model.Statuses.ActiveStatuses : null;
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
            if (runtime == null || runtime.Kind == StatusEffectKind.None)
            {
                return null;
            }

            return runtime.SourceData != null
                ? runtime.SourceData
                : StatusRuntimeDataFactory.Create(runtime.Kind, runtime.DisplayName);
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
            if (data == null || string.IsNullOrWhiteSpace(data.ConditionalSourceStatusTag))
            {
                return false;
            }

            if (source == null || !StatusEffectLookup.TryParse(data.ConditionalSourceStatusTag, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return source.Resources != null && source.Resources.CurrentShield > 0f;
            }

            return source.Statuses != null && source.Statuses.Has(kind);
        }

        /*
         * 대상 유닛의 상태가 조건을 만족하는지 확인한다.
         */
        private static bool MatchesConditionalTargetStatus(UnitCombatState target, StatusRuntimeData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ConditionalTargetStatusTag))
            {
                return false;
            }

            if (target == null || !StatusEffectLookup.TryParse(data.ConditionalTargetStatusTag, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }

            return target.Statuses != null && target.Statuses.Has(kind);
        }
    }
}
