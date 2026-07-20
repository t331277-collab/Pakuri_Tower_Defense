using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 상태 효과 런타임의 전투 중 상태와 실행을 관리한다.
     */
    public static class StatusEffectRules
    {
        private const float MinimumActionMultiplier = 0.05f;

        /*
         * 상태 조건 조건에 필요한 값을 보관한다.
         */
        internal readonly struct StatusConditionRequirement
        {
            /*
             * 상태 조건 조건에 필요한 값을 초기화한다.
             */
            public StatusConditionRequirement(StatusEffectKind kind, int minStacks)
            {
                Kind = kind;
                MinStacks = Mathf.Max(1, minStacks);
            }

            public StatusEffectKind Kind { get; }
            public int MinStacks { get; }
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
         * 상태 데이터를 생성한다.
         */
        public static RuntimeStatusData CreateStatusData(StatusEffectKind kind, string label, SkillDefinition source = null)
        {
            if (kind == StatusEffectKind.None)
            {
                return null;
            }

            var catalogDefinition = StatusEffectUtility.GetDefinition(kind);
            var status = new RuntimeStatusData();
            status.Definition = catalogDefinition;
            status.Kind = kind;
            status.StatusTag = catalogDefinition.Id;
            status.StatusName = !string.IsNullOrWhiteSpace(label)
                ? label
                : !string.IsNullOrWhiteSpace(catalogDefinition.StatusEffectLabel)
                    ? catalogDefinition.StatusEffectLabel
                    : catalogDefinition.Id;

            var defaultDuration = catalogDefinition.DefaultDurationSeconds;
            var sourceDuration = source != null ? source.StatusDurationSeconds : 0f;
            status.Duration = sourceDuration > 0f ? sourceDuration : defaultDuration;

            var sourceMaxStacks = source != null ? source.StatusMaxStacks : 0;
            status.MaxStacks = sourceMaxStacks > 0
                ? sourceMaxStacks
                : catalogDefinition.MaxStacks;
            status.IsStackable = status.MaxStacks != 1;

            var sourceStacks = source != null ? source.StatusStackAmount : 0;
            status.BaseStackAmount = sourceStacks > 0
                ? sourceStacks
                : catalogDefinition.BaseStackAmount > 0 ? catalogDefinition.BaseStackAmount : 1;

            var catalogPermanent = catalogDefinition.IsPermanent;
            status.Permanent = (catalogPermanent || (source != null && source.StatusPermanent))
                && status.Duration <= 0f;
            status.CanMove = catalogDefinition.CanMove;
            status.CanAct = catalogDefinition.CanAct;
            status.CanUseSpecialSkill = catalogDefinition.CanUseSpecialSkill;

            var moveSpeedBonus = ResolveOverride(source != null ? source.StatusMoveSpeedBonus : 0f, catalogDefinition.MoveSpeedBonusPerStack);
            status.MoveSpeedBonus = moveSpeedBonus;
            status.MovementSlowRate = moveSpeedBonus < 0f ? -moveSpeedBonus : 0f;
            status.DamageTakenBonus = ResolveOverride(source != null ? source.StatusDamageTakenBonus : 0f, catalogDefinition.DamageTakenBonusPerStack);
            status.CriticalDamageTakenBonus = ResolveOverride(source != null ? source.StatusCriticalDamageTakenBonus : 0f, catalogDefinition.CriticalDamageTakenBonusPerStack);
            status.AilmentResistanceBonus = ResolveOverride(source != null ? source.StatusAilmentResistanceBonus : 0f, 0f);
            status.CriticalResistanceBonus = ResolveOverride(source != null ? source.StatusCriticalResistanceBonus : 0f, catalogDefinition.CriticalResistanceBonusPerStack);
            status.ElementResistReduction = ResolveOverride(source != null ? source.StatusElementResistReduction : 0f, catalogDefinition.ElementResistReductionPerStack);
            status.FlatElementResistReduction = ResolveOverride(source != null ? source.StatusFlatElementResistReduction : 0f, 0f);
            status.ElementDamageTakenBonus = ResolveOverride(source != null ? source.StatusElementDamageTakenBonus : 0f, catalogDefinition.ElementDamageTakenBonusPerStack);

            var actionSpeedBonus = ResolveOverride(source != null ? source.StatusActionSpeedBonus : 0f, catalogDefinition.ActionSpeedBonusPerStack);
            var attackPowerBonus = ResolveOverride(source != null ? source.StatusAttackPowerBonus : 0f, catalogDefinition.AttackPowerBonusPerStack);
            status.Modifiers.ActionSpeedBonus = actionSpeedBonus;
            status.Modifiers.AttackPowerBonus = attackPowerBonus;
            status.Modifiers.SpellPowerBonus = source != null ? source.StatusSpellPowerBonus : 0f;
            status.Modifiers.DamageBonusRate = source != null ? source.StatusDamageBonusRate : 0f;
            status.Modifiers.ShieldReceivedBonus = 0f;
            status.Modifiers.CritChanceBonusRate = 0f;
            status.Modifiers.CritDamageBonusRate = 0f;
            status.OutgoingAdditionalDamageMultiplier = 0f;
            status.OutgoingAdditionalDamageTriggerAttribute = DamageAttribute.Physical;
            status.OutgoingAdditionalDamageAttribute = DamageAttribute.Physical;

            if (catalogDefinition.HasAttribute)
            {
                status.HasElementModifierTarget = true;
                status.ElementModifierTarget = catalogDefinition.Attribute;
                status.Modifiers.ResistReductionElement = status.ElementModifierTarget;
            }

            ApplySourceAwareMetadata(status, kind, source);
            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.IsControlEffect = !status.CanMove || !status.CanAct || !status.CanUseSpecialSkill;
            status.StatusEffectPrefab = source != null && source.StatusEffectPrefab != null
                ? source.StatusEffectPrefab
                : catalogDefinition.StatusEffectPrefab;
            status.RuntimeVisual = source != null
                && source.RuntimeVisual != null
                && source.RuntimeVisual.Anchor == RuntimeSkillVisualAnchor.StatusTarget
                && EffectVisualUtility.HasVisual(source.RuntimeVisual)
                ? source.RuntimeVisual
                : new Pakuri.Data.RuntimeSkillVisualSpec();
            return status;
        }

        /*
         * 상태 대상 범위를 해석하고 성공 여부를 반환한다.
         */
        public static bool TryParseStatusTargetScope(string rawValue, out StatusTargetScope scope)
        {
            scope = StatusTargetScope.Unspecified;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            switch (rawValue.Trim().ToLowerInvariant())
            {
                case "self":
                    scope = StatusTargetScope.Self;
                    return true;
                case "all_allies":
                    scope = StatusTargetScope.AllAllies;
                    return true;
                default:
                    return false;
            }
        }

        /*
         * 상태 병합 규칙을 해석하고 성공 여부를 반환한다.
         */
        public static bool TryParseStatusMergePolicy(string rawValue, out StatusMergePolicy policy)
        {
            policy = StatusMergePolicy.Unspecified;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            switch (rawValue.Trim().ToLowerInvariant())
            {
                case "same_source_take_highest":
                    policy = StatusMergePolicy.SameSourceTakeHighest;
                    return true;
                case "same_source_refresh":
                    policy = StatusMergePolicy.SameSourceRefresh;
                    return true;
                case "always_stack":
                    policy = StatusMergePolicy.AlwaysStack;
                    return true;
                default:
                    return false;
            }
        }

        /*
         * 보호막 갱신 규칙을 해석하고 성공 여부를 반환한다.
         */
        public static bool TryParseShieldRefreshPolicy(string rawValue, out ShieldRefreshRule rule)
        {
            rule = ShieldRefreshRule.TakeHighest;
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return false;
            }

            switch (rawValue.Trim().ToLowerInvariant())
            {
                case "take_highest":
                    rule = ShieldRefreshRule.TakeHighest;
                    return true;
                case "replace":
                    rule = ShieldRefreshRule.Replace;
                    return true;
                case "stack":
                    rule = ShieldRefreshRule.Stack;
                    return true;
                default:
                    return false;
            }
        }

        /*
         * 기본 보정값과 상태 중첩을 합쳐 최종 보정량을 계산한다.
         */
        public static float ComputeModifierMagnitude(RuntimeStatusData data)
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
        public static bool CanMove(BaseUnitRuntimeModel model)
        {
            return !HasAnyStatus(model, data => !data.CanMove);
        }

        /*
         * 현재 상태에서 행동할 수 있는지 확인한다.
         */
        public static bool CanAct(BaseUnitRuntimeModel model)
        {
            return !HasAnyStatus(model, data => !data.CanAct);
        }

        /*
         * 현재 상태에서 특수 스킬을 사용할 수 있는지 확인한다.
         */
        public static bool CanUseSpecialSkill(BaseUnitRuntimeModel model)
        {
            return !HasAnyStatus(model, data => !data.CanUseSpecialSkill);
        }

        /*
         * 행동 속도 배율을 결정한다.
         */
        public static float ResolveActionSpeedMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(MinimumActionMultiplier, 1f + SumStacked(model, data => data.Modifiers.ActionSpeedBonus));
        }

        /*
         * 이동 속도 배율을 결정한다.
         */
        public static float ResolveMoveSpeedMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.MoveSpeedBonus));
        }

        /*
         * 공격 능력치 배율을 결정한다.
         */
        public static float ResolveAttackPowerMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.AttackPowerBonus));
        }

        /*
         * 주문 능력치 배율을 결정한다.
         */
        public static float ResolveSpellPowerMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.SpellPowerBonus));
        }

        /*
         * 보호막 받는 배율을 결정한다.
         */
        public static float ResolveShieldReceivedMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.ShieldReceivedBonus));
        }

        /*
         * 치명타 확률 보너스를 결정한다.
         */
        public static float ResolveCriticalChanceBonus(BaseUnitRuntimeModel model)
        {
            return SumStacked(model, data => data.Modifiers.CritChanceBonusRate);
        }

        /*
         * 치명타 피해 보너스를 결정한다.
         */
        public static float ResolveCriticalDamageBonus(BaseUnitRuntimeModel model)
        {
            return SumStacked(model, data => data.Modifiers.CritDamageBonusRate);
        }

        /*
         * 주는 피해 배율을 결정한다.
         */
        public static float ResolveOutgoingDamageMultiplier(BaseUnitRuntimeModel source, DamageAttribute attribute, string sourceSkillId = null)
        {
            return Mathf.Max(0f, 1f + SumStacked(source, data =>
                MatchesAttribute(data, attribute) && MatchesSkillRuntimeKinds(data.ConditionalOutgoingSkillRuntimeKinds, sourceSkillId)
                    ? data.Modifiers.DamageBonusRate
                    : 0f));
        }

        /*
         * 주는 추가 피해 설정을 결정한다.
         */
        internal static List<OutgoingAdditionalDamageSpec> ResolveOutgoingAdditionalDamageSpecs(BaseUnitRuntimeModel source, DamageAttribute triggerAttribute)
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
        public static float ResolveIncomingDamageMultiplier(BaseUnitRuntimeModel target, BaseUnitRuntimeModel source, DamageAttribute attribute, string sourceSkillId = null)
        {
            return Mathf.Max(0f, 1f + SumStacked(target, data =>
            {
                var runtimeKindMatches = MatchesSkillRuntimeKinds(data.ConditionalIncomingSkillRuntimeKinds, sourceSkillId);
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
        public static float ResolveElementResistReduction(BaseUnitRuntimeModel target, DamageAttribute attribute)
        {
            return Mathf.Clamp01(SumStacked(target, data => MatchesAttribute(data, attribute) ? data.ElementResistReduction : 0f));
        }

        /*
         * 고정 원소 저항 감소를 결정한다.
         */
        public static float ResolveFlatElementResistReduction(BaseUnitRuntimeModel target, DamageAttribute attribute)
        {
            return Mathf.Max(0f, SumStacked(target, data => MatchesAttribute(data, attribute) ? data.FlatElementResistReduction : 0f));
        }

        /*
         * 치명타 피해 받는 보너스를 결정한다.
         */
        public static float ResolveCriticalDamageTakenBonus(BaseUnitRuntimeModel target)
        {
            return SumStacked(target, data => data.CriticalDamageTakenBonus);
        }

        /*
         * 상태 이상 저항 보너스를 결정한다.
         */
        public static float ResolveAilmentResistanceBonus(BaseUnitRuntimeModel target)
        {
            return Mathf.Clamp01(SumStacked(target, data => data.AilmentResistanceBonus));
        }

        /*
         * 치명타 저항 보너스를 결정한다.
         */
        public static float ResolveCriticalResistanceBonus(BaseUnitRuntimeModel target)
        {
            return SumStacked(target, data => data.CriticalResistanceBonus);
        }

        /*
         * 조건부 상태 확률 보너스를 결정한다.
         */
        public static float ResolveConditionalStatusChanceBonus(BaseUnitRuntimeModel source, BaseUnitRuntimeModel target)
        {
            return SumStacked(source, data => MatchesConditionalTargetStatus(target, data) ? data.ConditionalStatusChanceBonus : 0f);
        }

        /*
         * 적용된 상태 지속시간 보너스를 결정한다.
         */
        public static float ResolveAppliedStatusDurationBonus(BaseUnitRuntimeModel source, string statusId)
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
         * 조건 상태 표현식을 해석하고 성공 여부를 반환한다.
         */
        internal static bool TryParseConditionStatusExpression(string rawValue, out StatusConditionRequirement[] requirements)
        {
            if (!TryParseConditionStatusExpressionGroups(rawValue, out var groups))
            {
                requirements = Array.Empty<StatusConditionRequirement>();
                return false;
            }

            if (groups.Length == 0)
            {
                requirements = Array.Empty<StatusConditionRequirement>();
                return true;
            }

            var flattened = new List<StatusConditionRequirement>();
            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                for (var j = 0; j < group.Length; j++)
                {
                    flattened.Add(group[j]);
                }
            }

            requirements = flattened.ToArray();
            return true;
        }

        /*
         * 대상 상태가 조건식을 만족하는지 확인한다.
         */
        internal static bool MatchesConditionStatus(BaseUnitRuntimeModel target, string rawValue)
        {
            return MatchesConditionStatus(target, rawValue, null);
        }

        /*
         * 상태 목록이 조건식을 만족하는지 확인한다.
         */
        internal static bool MatchesConditionStatus(BaseUnitRuntimeModel target, string rawValue, string requiredSourceSkillId)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            if (target == null || !TryParseConditionStatusExpressionGroups(rawValue, out var groups))
            {
                return false;
            }

            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var matchesGroup = true;
                for (var j = 0; j < group.Length; j++)
                {
                    var requirement = group[j];
                    var stacks = requirement.Kind == StatusEffectKind.Shield && target.Resources != null && target.Resources.CurrentShield > 0f
                        ? Math.Max(1, target.Statuses != null ? target.Statuses.GetStacks(requirement.Kind) : 0)
                        : target.Statuses != null ? target.Statuses.GetStacks(requirement.Kind) : 0;
                    if (stacks < requirement.MinStacks)
                    {
                        matchesGroup = false;
                        break;
                    }

                    if (!MatchesRequiredSourceSkill(target, requirement.Kind, requiredSourceSkillId))
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
         * 상태를 부여한 스킬이 필수 출처 스킬과 일치하는지 확인한다.
         */
        private static bool MatchesRequiredSourceSkill(BaseUnitRuntimeModel target, StatusEffectKind kind, string requiredSourceSkillId)
        {
            if (string.IsNullOrWhiteSpace(requiredSourceSkillId))
            {
                return true;
            }

            var statuses = target != null && target.Statuses != null ? target.Statuses.ActiveStatuses : null;
            var tokens = requiredSourceSkillId.Split(';', ',');
            for (var i = 0; statuses != null && i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status == null || status.Kind != kind || status.Stacks <= 0)
                {
                    continue;
                }

                var sourceSkillId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                    ? status.SourceSkillId
                    : status.SourceData != null ? status.SourceData.SourceSkillId : string.Empty;
                if (string.IsNullOrWhiteSpace(sourceSkillId))
                {
                    continue;
                }

                for (var j = 0; j < tokens.Length; j++)
                {
                    var token = tokens[j] != null ? tokens[j].Trim() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(token)
                        && string.Equals(token, sourceSkillId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /*
         * 상태 종류와 중첩이 조건을 만족하는지 확인한다.
         */
        internal static bool MatchesConditionStatus(UnitStatusRuntime status, string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            if (status == null || !TryParseConditionStatusExpressionGroups(rawValue, out var groups))
            {
                return false;
            }

            for (var i = 0; i < groups.Length; i++)
            {
                var group = groups[i];
                var matchesGroup = true;
                for (var j = 0; j < group.Length; j++)
                {
                    var requirement = group[j];
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
         * 조건 상태 표현식 그룹을 해석하고 성공 여부를 반환한다.
         */
        private static bool TryParseConditionStatusExpressionGroups(string rawValue, out StatusConditionRequirement[][] groups)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                groups = Array.Empty<StatusConditionRequirement[]>();
                return true;
            }

            // 쉼표와 세미콜론은 조건 묶음을 나누고, 묶음 안의 &는 동시에 필요한 상태를 뜻한다.
            var tokens = rawValue.Split(';', ',');
            var parsedGroups = new List<StatusConditionRequirement[]>();
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] != null ? tokens[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                var andTokens = token.Split('&');
                var group = new List<StatusConditionRequirement>(andTokens.Length);
                for (var j = 0; j < andTokens.Length; j++)
                {
                    var part = andTokens[j] != null ? andTokens[j].Trim() : string.Empty;
                    if (string.IsNullOrWhiteSpace(part))
                    {
                        groups = Array.Empty<StatusConditionRequirement[]>();
                        return false;
                    }

                    var statusId = part;
                    var minStacks = 1;
                    // "freeze>=2"와 "freeze:2" 형식을 모두 상태 ID와 최소 중첩으로 해석한다.
                    var separatorIndex = part.IndexOf(">=", StringComparison.OrdinalIgnoreCase);
                    var separatorLength = 2;
                    if (separatorIndex < 0)
                    {
                        separatorIndex = part.IndexOf(':');
                        separatorLength = 1;
                    }

                    if (separatorIndex >= 0)
                    {
                        statusId = part.Substring(0, separatorIndex).Trim();
                        var minStackText = part.Substring(separatorIndex + separatorLength).Trim();
                        if (!int.TryParse(minStackText, out minStacks) || minStacks <= 0)
                        {
                            groups = Array.Empty<StatusConditionRequirement[]>();
                            return false;
                        }
                    }

                    if (!StatusEffectUtility.TryParse(statusId, out var kind))
                    {
                        groups = Array.Empty<StatusConditionRequirement[]>();
                        return false;
                    }

                    group.Add(new StatusConditionRequirement(kind, minStacks));
                }

                if (group.Count > 0)
                {
                    parsedGroups.Add(group.ToArray());
                }
            }

            groups = parsedGroups.ToArray();
            return groups.Length > 0;
        }

        /*
         * 출처 스킬의 실행 종류가 허용 목록과 일치하는지 확인한다.
         */
        internal static bool MatchesSkillRuntimeKinds(string rawValue, string sourceSkillId)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(sourceSkillId))
            {
                return false;
            }

            var manager = CsvDataLoader.CurrentCatalog;
            if (manager == null || !manager.TryGetData(sourceSkillId, out SkillDefinition skill) || skill == null)
            {
                return false;
            }

            var tokens = rawValue.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] != null ? tokens[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (string.Equals(token, "Area", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token, "AoE", StringComparison.OrdinalIgnoreCase))
                {
                    if (IsAreaLikeSkill(skill))
                    {
                        return true;
                    }

                    continue;
                }

                if (Enum.TryParse(token, true, out SkillRuntimeKind runtimeKind)
                    && skill.RuntimeKind == runtimeKind)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 하나 이상의 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasAnyStatus(BaseUnitRuntimeModel model, System.Func<RuntimeStatusData, bool> predicate)
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
         * 스킬이 범위 공격으로 취급되는 종류인지 확인한다.
         */
        private static bool IsAreaLikeSkill(SkillDefinition skill)
        {
            if (skill == null)
            {
                return false;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.AreaAttack || skill.RuntimeKind == SkillRuntimeKind.Field)
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

        /*
         * 조건에 맞는 상태 보정값을 중첩 수만큼 합산한다.
         */
        private static float SumStacked(BaseUnitRuntimeModel model, System.Func<RuntimeStatusData, float> selector)
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
        private static RuntimeStatusData ResolveRuntimeData(UnitStatusRuntime runtime)
        {
            if (runtime == null || runtime.Kind == StatusEffectKind.None)
            {
                return null;
            }

            return runtime.SourceData != null
                ? runtime.SourceData
                : CreateStatusData(runtime.Kind, runtime.DisplayName);
        }

        /*
         * 재정의를 결정한다.
         */
        private static float ResolveOverride(float sourceValue, float defaultValue)
        {
            return !Mathf.Approximately(sourceValue, 0f) ? sourceValue : defaultValue;
        }

        /*
         * 피해 속성이 상태 효과 조건과 일치하는지 확인한다.
         */
        private static bool MatchesAttribute(RuntimeStatusData data, DamageAttribute attribute)
        {
            return data != null && data.HasElementModifierTarget && (DamageAttribute)(int)data.ElementModifierTarget == attribute;
        }

        /*
         * 출처 유닛의 상태가 조건을 만족하는지 확인한다.
         */
        private static bool MatchesConditionalSourceStatus(BaseUnitRuntimeModel source, RuntimeStatusData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ConditionalSourceStatusTag))
            {
                return false;
            }

            if (source == null || !StatusEffectUtility.TryParse(data.ConditionalSourceStatusTag, out var kind))
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
        private static bool MatchesConditionalTargetStatus(BaseUnitRuntimeModel target, RuntimeStatusData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.ConditionalTargetStatusTag))
            {
                return false;
            }

            if (target == null || !StatusEffectUtility.TryParse(data.ConditionalTargetStatusTag, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return target.Resources != null && target.Resources.CurrentShield > 0f;
            }

            return target.Statuses != null && target.Statuses.Has(kind);
        }

        /*
         * 출처 반영 부가 정보를 적용한다.
         */
        private static void ApplySourceAwareMetadata(RuntimeStatusData status, StatusEffectKind kind, SkillDefinition source)
        {
            if (status == null || source == null)
            {
                return;
            }

            var isSourceAwareSkill = source.RuntimeKind == SkillRuntimeKind.Buff || source.RuntimeKind == SkillRuntimeKind.Shield;
            if (!isSourceAwareSkill)
            {
                return;
            }

            status.SourceSkillId = source.SkillId != null ? source.SkillId.Trim() : string.Empty;
            status.TargetScope = ResolveTargetScope(source, kind);
            status.MergePolicy = ResolveMergePolicy(source);
            status.ShieldAmountRefreshPolicy = ResolveShieldRefreshPolicy(source);
        }

        /*
         * 대상 범위를 결정한다.
         */
        private static StatusTargetScope ResolveTargetScope(SkillDefinition source, StatusEffectKind kind)
        {
            if (source != null && TryParseStatusTargetScope(source.StatusTargetScope, out var parsed))
            {
                return parsed;
            }

            if (source != null && source.RuntimeKind == SkillRuntimeKind.Buff)
            {
                var statusKey = !string.IsNullOrWhiteSpace(source.StatusEffectId)
                    ? source.StatusEffectId
                    : source.StatusEffectLabel;
                if (StatusEffectUtility.TryParse(statusKey, out var parsedKind)
                    && parsedKind == StatusEffectKind.SlaughterPermit)
                {
                    return StatusTargetScope.Self;
                }
            }

            return kind == StatusEffectKind.Shield
                ? StatusTargetScope.AllAllies
                : StatusTargetScope.Unspecified;
        }

        /*
         * 병합 규칙을 결정한다.
         */
        private static StatusMergePolicy ResolveMergePolicy(SkillDefinition source)
        {
            if (source != null && TryParseStatusMergePolicy(source.StatusMergePolicy, out var parsed))
            {
                return parsed;
            }

            return StatusMergePolicy.SameSourceRefresh;
        }

        /*
         * 보호막 갱신 규칙을 결정한다.
         */
        private static ShieldRefreshRule ResolveShieldRefreshPolicy(SkillDefinition source)
        {
            if (source != null && TryParseShieldRefreshPolicy(source.ShieldAmountRefreshPolicy, out var parsed))
            {
                return parsed;
            }

            return ShieldRefreshRule.TakeHighest;
        }

    }
}
