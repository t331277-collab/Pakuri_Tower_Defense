using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.InGame;

/*
 * CSV에 작성된 상태 관련 문자열을 강타입 값으로 변환한다.
 */
namespace Pakuri.Data
{
    internal static class StatusValueParser
    {
        public static bool TryParseStatusKind(string value, out StatusEffectKind kind)
        {
            kind = StatusEffectKind.None;
            return !string.IsNullOrWhiteSpace(value)
                && Enum.TryParse(value.Trim().Replace("-", string.Empty), true, out kind)
                && kind != StatusEffectKind.None;
        }

        /*
         * ParseStatusKind에 필요한 데이터를 읽어 변환한다.
         */
        public static StatusEffectKind ParseStatusKind(string value /* 처리할 값 */)
        {
            if (TryParseStatusKind(value, out var kind))
            {
                return kind;
            }

            throw new InvalidOperationException($"Unsupported status id '{value}'.");
        }

        /*
         * ParseStatusKinds에 필요한 데이터를 읽어 변환한다.
         */
        public static StatusEffectKind[] ParseStatusKinds(string rawValue /* 변환 전 원본 문자열 */)
        {
            var statusIds = ParseIdList(rawValue);
            var kinds = new StatusEffectKind[statusIds.Length];
            for (var i = 0; i < statusIds.Length; i++)
            {
                kinds[i] = ParseStatusKind(statusIds[i]);
            }

            return kinds;
        }

        /*
         * TryParseConditionStatusExpression 작업을 시도하고 성공 여부를 반환한다.
         */
        public static bool TryParseConditionStatusExpression(
            string rawValue /* 변환 전 원본 문자열 */,
            out StatusConditionGroup[] groups /* 그룹 목록 */)
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

        /*
         * ParseConditionStatusExpression에 필요한 데이터를 읽어 변환한다.
         */
        public static StatusConditionGroup[] ParseConditionStatusExpression(string rawValue /* 변환 전 원본 문자열 */)
        {
            if (TryParseConditionStatusExpression(rawValue, out var groups))
            {
                return groups;
            }

            throw new InvalidOperationException($"Unsupported status condition '{rawValue}'.");
        }

        /*
         * ParseSkillRuntimeKindConditions에 필요한 데이터를 읽어 변환한다.
         */
        public static SkillRuntimeKindCondition[] ParseSkillRuntimeKindConditions(string rawValue /* 변환 전 원본 문자열 */)
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

        /*
         * ParseIdList에 필요한 데이터를 읽어 변환한다.
         */
        public static string[] ParseIdList(string rawValue /* 변환 전 원본 문자열 */)
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

        /*
         * TryParseTargetScope 작업을 시도하고 성공 여부를 반환한다.
         */
        public static bool TryParseTargetScope(string rawValue /* 변환 전 원본 문자열 */, out StatusTargetScope scope /* 적용 범위 */)
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

        /*
         * TryParseMergePolicy 작업을 시도하고 성공 여부를 반환한다.
         */
        public static bool TryParseMergePolicy(string rawValue /* 변환 전 원본 문자열 */, out StatusMergePolicy policy /* 정책 */)
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

        /*
         * TryParseShieldRefreshRule 작업을 시도하고 성공 여부를 반환한다.
         */
        public static bool TryParseShieldRefreshRule(string rawValue /* 변환 전 원본 문자열 */, out ShieldRefreshRule rule /* 규칙 */)
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

        /*
         * ParseTargetScope에 필요한 데이터를 읽어 변환한다.
         */
        public static StatusTargetScope ParseTargetScope(string rawValue /* 변환 전 원본 문자열 */)
        {
            if (TryParseTargetScope(rawValue, out var scope))
            {
                return scope;
            }

            throw new InvalidOperationException($"Unsupported status target scope '{rawValue}'.");
        }

        /*
         * ParseMergePolicy에 필요한 데이터를 읽어 변환한다.
         */
        public static StatusMergePolicy ParseMergePolicy(string rawValue /* 변환 전 원본 문자열 */)
        {
            if (TryParseMergePolicy(rawValue, out var policy))
            {
                return policy;
            }

            throw new InvalidOperationException($"Unsupported status merge policy '{rawValue}'.");
        }

        /*
         * ParseShieldRefreshRule에 필요한 데이터를 읽어 변환한다.
         */
        public static ShieldRefreshRule ParseShieldRefreshRule(string rawValue /* 변환 전 원본 문자열 */)
        {
            if (TryParseShieldRefreshRule(rawValue, out var rule))
            {
                return rule;
            }

            throw new InvalidOperationException($"Unsupported shield refresh rule '{rawValue}'.");
        }
    }
}
