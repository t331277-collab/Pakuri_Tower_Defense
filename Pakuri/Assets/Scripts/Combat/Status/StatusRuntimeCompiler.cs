using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

/*
 * 검증된 상태 ID와 스킬 설정을 전투에서 바로 사용하는 상태 데이터로 변환한다.
 */
namespace Pakuri.Data
{
    public static class StatusRuntimeCompiler
    {
        /*
         * CompileTriggers 작업을 수행한다.
         */
        public static void CompileTriggers(SkillTriggerDefinition[] triggers /* 트리거 목록 */)
        {
            if (triggers == null)
            {
                return;
            }

            for (var i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (trigger == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(trigger.RequiredSourceStatusId))
                {
                    trigger.RequiredSourceStatusKind = ParseStatusKind(trigger.RequiredSourceStatusId);
                }

                trigger.ConditionStatuses = ParseConditionStatusExpression(trigger.ConditionStatusId);
                trigger.ConditionStatusSourceSkillIds = ParseIdList(trigger.ConditionStatusSourceSkillId);
                trigger.RequiredActiveChoiceIds = ParseIdList(trigger.RequiresActiveChoiceId);
                trigger.ExcludedActiveChoiceIds = ParseIdList(trigger.ExcludesActiveChoiceId);
                trigger.TriggerAttributes = ParseDamageAttributes(trigger.TriggerAttribute);
                trigger.EventSkillIds = ParseIdList(trigger.EventSkillId);
                trigger.EventSkillRuntimeKindValues = ParseSkillRuntimeKindConditions(
                    trigger.EventSkillRuntimeKinds);
                trigger.EventSourceScopeValue = ParseEventSourceScope(trigger.EventSourceScope);
                trigger.Nodes = SkillNodeMapper.MapSkillNodeDefinitions(trigger.NormalizedNodes);
            }
        }

        /*
         * ParseStatusKind에 필요한 데이터를 읽어 변환한다.
         */
        public static StatusEffectKind ParseStatusKind(string value /* 처리할 값 */)
        {
            if (StatusEffectLookup.TryParse(value, out var kind))
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
         * Create에 필요한 결과를 만들어 반환한다.
         */
        public static StatusRuntimeData Create(StatusEffectKind kind /* 처리할 종류 */, string label /* 표시 문구 */)
        {
            if (kind == StatusEffectKind.None)
            {
                throw new InvalidOperationException("StatusEffectKind.None cannot create runtime status data.");
            }

            var definition = StatusEffectLookup.GetDefinition(kind);
            var status = new StatusRuntimeData();
            status.Definition = definition;
            status.Kind = kind;
            status.StatusTag = definition.Id;
            status.StatusName = definition.StatusEffectLabel;
            if (!string.IsNullOrWhiteSpace(label))
            {
                status.StatusName = label;
            }

            status.Duration = definition.DefaultDurationSeconds;
            status.MaxStacks = definition.MaxStacks;
            status.IsStackable = definition.MaxStacks != 1;
            status.BaseStackAmount = definition.BaseStackAmount;
            status.Permanent = definition.IsPermanent && status.Duration <= 0f;
            status.CanMove = definition.CanMove;
            status.CanAct = definition.CanAct;
            status.CanUseSpecialSkill = definition.CanUseSpecialSkill;
            status.MoveSpeedBonus = definition.MoveSpeedBonusPerStack;
            if (status.MoveSpeedBonus < 0f)
            {
                status.MovementSlowRate = -status.MoveSpeedBonus;
            }

            status.DamageTakenBonus = definition.DamageTakenBonusPerStack;
            status.CriticalDamageTakenBonus = definition.CriticalDamageTakenBonusPerStack;
            status.CriticalResistanceBonus = definition.CriticalResistanceBonusPerStack;
            status.ElementResistReduction = definition.ElementResistReductionPerStack;
            status.ElementDamageTakenBonus = definition.ElementDamageTakenBonusPerStack;
            status.Modifiers.ActionSpeedBonus = definition.ActionSpeedBonusPerStack;
            status.Modifiers.AttackPowerBonus = definition.AttackPowerBonusPerStack;
            status.StatusEffectPrefab = definition.StatusEffectPrefab;

            if (definition.HasAttribute)
            {
                status.HasElementModifierTarget = true;
                status.ElementModifierTarget = definition.Attribute;
                status.Modifiers.ResistReductionElement = definition.Attribute;
            }

            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.IsControlEffect = !status.CanMove || !status.CanAct || !status.CanUseSpecialSkill;
            return status;
        }

        /*
         * Create에 필요한 결과를 만들어 반환한다.
         */
        public static StatusRuntimeData Create(StatusEffectKind kind /* 처리할 종류 */, string label /* 표시 문구 */, SkillSourceDefinition source /* 변환할 스킬 정의 */)
        {
            var status = Create(kind, label);
            if (source == null)
            {
                return status;
            }

            if (source.StatusDurationSeconds > 0f)
            {
                status.Duration = source.StatusDurationSeconds;
                status.Permanent = false;
            }

            if (source.StatusMaxStacks > 0)
            {
                status.MaxStacks = source.StatusMaxStacks;
                status.IsStackable = status.MaxStacks != 1;
            }

            if (source.StatusStackAmount > 0)
            {
                status.BaseStackAmount = source.StatusStackAmount;
            }

            if (source.StatusPermanent && status.Duration <= 0f)
            {
                status.Permanent = true;
            }

            if (!Mathf.Approximately(source.StatusMoveSpeedBonus, 0f))
            {
                status.MoveSpeedBonus = source.StatusMoveSpeedBonus;
            }

            if (status.MoveSpeedBonus < 0f)
            {
                status.MovementSlowRate = -status.MoveSpeedBonus;
            }
            else
            {
                status.MovementSlowRate = 0f;
            }

            if (!Mathf.Approximately(source.StatusDamageTakenBonus, 0f))
            {
                status.DamageTakenBonus = source.StatusDamageTakenBonus;
            }

            if (!Mathf.Approximately(source.StatusCriticalDamageTakenBonus, 0f))
            {
                status.CriticalDamageTakenBonus = source.StatusCriticalDamageTakenBonus;
            }

            status.AilmentResistanceBonus = source.StatusAilmentResistanceBonus;
            if (!Mathf.Approximately(source.StatusCriticalResistanceBonus, 0f))
            {
                status.CriticalResistanceBonus = source.StatusCriticalResistanceBonus;
            }

            if (!Mathf.Approximately(source.StatusElementResistReduction, 0f))
            {
                status.ElementResistReduction = source.StatusElementResistReduction;
            }

            status.FlatElementResistReduction = source.StatusFlatElementResistReduction;
            if (!Mathf.Approximately(source.StatusElementDamageTakenBonus, 0f))
            {
                status.ElementDamageTakenBonus = source.StatusElementDamageTakenBonus;
            }

            if (!Mathf.Approximately(source.StatusActionSpeedBonus, 0f))
            {
                status.Modifiers.ActionSpeedBonus = source.StatusActionSpeedBonus;
            }

            if (!Mathf.Approximately(source.StatusAttackPowerBonus, 0f))
            {
                status.Modifiers.AttackPowerBonus = source.StatusAttackPowerBonus;
            }

            status.Modifiers.SpellPowerBonus = source.StatusSpellPowerBonus;
            status.Modifiers.DamageBonusRate = source.StatusDamageBonusRate;
            status.SourceSkillId = source.SkillId;

            if (!string.IsNullOrWhiteSpace(source.StatusTargetScope))
            {
                status.TargetScope = ParseTargetScope(source.StatusTargetScope);
            }

            if (!string.IsNullOrWhiteSpace(source.StatusMergePolicy))
            {
                status.MergePolicy = ParseMergePolicy(source.StatusMergePolicy);
            }

            if (!string.IsNullOrWhiteSpace(source.ShieldAmountRefreshPolicy))
            {
                status.ShieldAmountRefreshPolicy = ParseShieldRefreshRule(source.ShieldAmountRefreshPolicy);
            }

            if (source.StatusEffectPrefab != null)
            {
                status.StatusEffectPrefab = source.StatusEffectPrefab;
            }

            if (source.RuntimeVisual != null
                && source.RuntimeVisual.Anchor == RuntimeSkillVisualAnchor.StatusTarget)
            {
                status.RuntimeVisual = source.RuntimeVisual;
            }

            status.Modifiers.ResistReduction = status.ElementResistReduction;
            return status;
        }

        /*
         * Create에 필요한 결과를 만들어 반환한다.
         */

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

                    if (!StatusEffectLookup.TryParse(statusId, out var kind))
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

            return (SkillTriggerEventSourceScope)Enum.Parse(
                typeof(SkillTriggerEventSourceScope),
                rawValue,
                true);
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
