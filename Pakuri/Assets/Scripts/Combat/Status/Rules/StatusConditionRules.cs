using System;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * 상태 중첩 조건과 출처 스킬·스킬 실행 종류 조건을 해석한다.
 */
namespace Pakuri.InGame
{
    public static class StatusConditionRules
    {
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
        internal static bool MatchesConditionStatus(UnitCombatState target, string rawValue)
        {
            return MatchesConditionStatus(target, rawValue, null);
        }

        /*
         * 상태 목록이 조건식을 만족하는지 확인한다.
         */
        internal static bool MatchesConditionStatus(UnitCombatState target, string rawValue, string requiredSourceSkillId)
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
        private static bool MatchesRequiredSourceSkill(UnitCombatState target, StatusEffectKind kind, string requiredSourceSkillId)
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
        internal static bool MatchesConditionStatus(StatusRuntimeInstance status, string rawValue)
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

                    if (!StatusEffectLookup.TryParse(statusId, out var kind))
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
    }
}
