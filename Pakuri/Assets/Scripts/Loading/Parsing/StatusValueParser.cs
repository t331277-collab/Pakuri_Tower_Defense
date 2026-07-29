/*
 * 역할: 상태 효과 표현식 파싱.
 * 책임: CSV 저작에 쓰이는 상태 ID·목록·중첩·지속 시간·조건식을 파싱한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.InGame;

namespace Pakuri.Data
{

    /// <summary><c>StatusValueParser</c> 원본 값을 런타임 모델로 파싱한다.</summary>
    internal static class StatusValueParser
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>StatusKind</c> 파싱을 시도하고 성공 여부를 반환한다.</summary>
        public static bool TryParseStatusKind(string value, out StatusEffectKind kind)
        {
            kind = StatusEffectKind.None;
            return !string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value.Trim().Replace("-", string.Empty), true, out kind)
                && kind != StatusEffectKind.None;
        }

        /// <summary>전달된 <c>value</c> 값을 사용해 <c>StatusKind</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static StatusEffectKind ParseStatusKind(string value)
        {
            if (TryParseStatusKind(value, out var kind))
            {
                return kind;
            }

            throw new InvalidOperationException($"Unsupported status id '{value}'.");
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>StatusKinds</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static StatusEffectKind[] ParseStatusKinds(string rawValue)
        {
            var statusIds = ParseIdList(rawValue);
            var kinds = new StatusEffectKind[statusIds.Length];
            for (var i = 0; i < statusIds.Length; i++)
            {
                kinds[i] = ParseStatusKind(statusIds[i]);
            }

            return kinds;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConditionStatusExpression</c> 파싱을 시도하고 성공 여부를 반환한다.</summary>
        public static bool TryParseConditionStatusExpression(
            string rawValue,
            out StatusConditionGroup[] groups)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                groups = Array.Empty<StatusConditionGroup>();
                return true;
            }

            var groupTokens = rawValue.Split(';', ',');
            var parsedGroups = new List<StatusConditionGroup>();
            for (var groupIndex = 0; groupIndex < groupTokens.Length; groupIndex++)
            {
                var groupText = groupTokens[groupIndex].Trim();
                if (string.IsNullOrWhiteSpace(groupText))
                {
                    continue;
                }

                var requirementTokens = groupText.Split('&');
                var requirements = new List<StatusConditionRequirement>();
                for (var requirementIndex = 0; requirementIndex < requirementTokens.Length; requirementIndex++)
                {
                    var requirementText = requirementTokens[requirementIndex].Trim();
                    if (string.IsNullOrWhiteSpace(requirementText))
                    {
                        groups = Array.Empty<StatusConditionGroup>();
                        return false;
                    }

                    var statusId = requirementText;
                    var minStacks = 1;
                    var separatorIndex = requirementText.IndexOf(">=", StringComparison.OrdinalIgnoreCase);
                    var separatorLength = 2;
                    if (separatorIndex < 0)
                    {
                        separatorIndex = requirementText.IndexOf(':');
                        separatorLength = 1;
                    }

                    if (separatorIndex >= 0)
                    {
                        statusId = requirementText.Substring(0, separatorIndex).Trim();
                        var minStackText = requirementText.Substring(separatorIndex + separatorLength).Trim();
                        if (!int.TryParse(minStackText, out minStacks) || minStacks <= 0)
                        {
                            groups = Array.Empty<StatusConditionGroup>();
                            return false;
                        }
                    }

                    if (!TryParseStatusKind(statusId, out var kind))
                    {
                        groups = Array.Empty<StatusConditionGroup>();
                        return false;
                    }

                    requirements.Add(new StatusConditionRequirement
                    {
                        Kind = kind,
                        MinStacks = minStacks
                    });
                }

                parsedGroups.Add(new StatusConditionGroup
                {
                    Requirements = requirements.ToArray()
                });
            }

            groups = parsedGroups.ToArray();
            return groups.Length > 0;
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>ConditionStatusExpression</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static StatusConditionGroup[] ParseConditionStatusExpression(string rawValue)
        {
            if (TryParseConditionStatusExpression(rawValue, out var groups))
            {
                return groups;
            }

            throw new InvalidOperationException($"Unsupported status condition '{rawValue}'.");
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>SkillRuntimeKindConditions</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static SkillRuntimeKindCondition[] ParseSkillRuntimeKindConditions(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return Array.Empty<SkillRuntimeKindCondition>();
            }

            var tokens = rawValue.Split(';', ',');
            var conditions = new List<SkillRuntimeKindCondition>();
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i].Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (string.Equals(token, "Area", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token, "AoE", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add(new SkillRuntimeKindCondition { AreaLike = true });
                    continue;
                }

                var kind = (SkillRuntimeKind)Enum.Parse(typeof(SkillRuntimeKind), token, true);
                conditions.Add(new SkillRuntimeKindCondition { Kind = kind });
            }

            return conditions.ToArray();
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>IdList</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static string[] ParseIdList(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return Array.Empty<string>();
            }

            var tokens = rawValue.Split(';', ',');
            var ids = new List<string>();
            for (var i = 0; i < tokens.Length; i++)
            {
                var id = tokens[i].Trim();
                if (!string.IsNullOrWhiteSpace(id))
                {
                    ids.Add(id);
                }
            }

            return ids.ToArray();
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>DamageAttributes</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static DamageAttribute[] ParseDamageAttributes(string rawValue)
        {
            var values = ParseIdList(rawValue);
            var attributes = new DamageAttribute[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                attributes[i] = (DamageAttribute)Enum.Parse(typeof(DamageAttribute), values[i], true);
            }

            return attributes;
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>EventSourceScope</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static SkillTriggerEventSourceScope ParseEventSourceScope(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return SkillTriggerEventSourceScope.Any;
            }

            switch (rawValue.Trim().ToLowerInvariant())
            {
                case "owner":
                    return SkillTriggerEventSourceScope.Owner;
                case "all_allies":
                    return SkillTriggerEventSourceScope.AllAllies;
                case "any":
                    return SkillTriggerEventSourceScope.Any;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported trigger event source scope '{rawValue}'.");
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>TargetScope</c> 파싱을 시도하고 성공 여부를 반환한다.</summary>
        public static bool TryParseTargetScope(string rawValue, out StatusTargetScope scope)
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
            }

            return false;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MergePolicy</c> 파싱을 시도하고 성공 여부를 반환한다.</summary>
        public static bool TryParseMergePolicy(string rawValue, out StatusMergePolicy policy)
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
            }

            return false;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ShieldRefreshRule</c> 파싱을 시도하고 성공 여부를 반환한다.</summary>
        public static bool TryParseShieldRefreshRule(string rawValue, out ShieldRefreshRule rule)
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
            }

            return false;
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>TargetScope</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static StatusTargetScope ParseTargetScope(string rawValue)
        {
            if (TryParseTargetScope(rawValue, out var scope))
            {
                return scope;
            }

            throw new InvalidOperationException($"Unsupported status target scope '{rawValue}'.");
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>MergePolicy</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static StatusMergePolicy ParseMergePolicy(string rawValue)
        {
            if (TryParseMergePolicy(rawValue, out var policy))
            {
                return policy;
            }

            throw new InvalidOperationException($"Unsupported status merge policy '{rawValue}'.");
        }

        /// <summary>전달된 <c>rawValue</c> 값을 사용해 <c>ShieldRefreshRule</c> 값을 런타임 표현으로 파싱한다.</summary>
        public static ShieldRefreshRule ParseShieldRefreshRule(string rawValue)
        {
            if (TryParseShieldRefreshRule(rawValue, out var rule))
            {
                return rule;
            }

            throw new InvalidOperationException($"Unsupported shield refresh rule '{rawValue}'.");
        }
    }
}
