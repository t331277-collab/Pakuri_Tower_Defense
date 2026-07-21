using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 카탈로그 상태 정의와 스킬 설정을 합쳐 실행용 상태 데이터를 만든다.
 * 대상 범위, 병합 방식, 보호막 갱신 규칙도 생성 단계에서 해석한다.
 */
namespace Pakuri.InGame
{
    public static class StatusRuntimeDataFactory
    {
        /*
         * 상태 데이터를 생성한다.
         */
        public static StatusRuntimeData Create(StatusEffectKind kind, string label, SkillDefinition source = null)
        {
            if (kind == StatusEffectKind.None)
            {
                return null;
            }

            var catalogDefinition = StatusEffectLookup.GetDefinition(kind);
            var status = new StatusRuntimeData();
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
                default:
                    return false;
            }
        }

        /*
         * 상태 병합 규칙을 해석하고 성공 여부를 반환한다.
         */
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
                default:
                    return false;
            }
        }

        /*
         * 보호막 갱신 규칙을 해석하고 성공 여부를 반환한다.
         */
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
                default:
                    return false;
            }
        }

        /*
         * 재정의를 결정한다.
         */
        private static float ResolveOverride(float sourceValue, float defaultValue)
        {
            return !Mathf.Approximately(sourceValue, 0f) ? sourceValue : defaultValue;
        }

        /*
         * 출처 반영 부가 정보를 적용한다.
         */
        private static void ApplySourceAwareMetadata(StatusRuntimeData status, StatusEffectKind kind, SkillDefinition source)
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
            if (source != null && TryParseTargetScope(source.StatusTargetScope, out var parsed))
            {
                return parsed;
            }

            if (source != null && source.RuntimeKind == SkillRuntimeKind.Buff)
            {
                var statusKey = !string.IsNullOrWhiteSpace(source.StatusEffectId)
                    ? source.StatusEffectId
                    : source.StatusEffectLabel;
                if (StatusEffectLookup.TryParse(statusKey, out var parsedKind)
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
            if (source != null && TryParseMergePolicy(source.StatusMergePolicy, out var parsed))
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
            if (source != null && TryParseShieldRefreshRule(source.ShieldAmountRefreshPolicy, out var parsed))
            {
                return parsed;
            }

            return ShieldRefreshRule.TakeHighest;
        }
    }
}
