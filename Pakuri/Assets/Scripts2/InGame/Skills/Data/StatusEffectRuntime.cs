using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public static class StatusEffectRuntime
    {
        private const float MinimumActionMultiplier = 0.05f;

        public static StatusEffectData CreateStatusData(StatusEffectKind kind, string label, SkillDefinition source = null)
        {
            if (kind == StatusEffectKind.None)
            {
                return null;
            }

            var fallback = StatusEffectUtility.GetDefinition(kind);
            var catalogDefinition = ResolveCatalogDefinition(kind);
            var status = ScriptableObject.CreateInstance<StatusEffectData>();
            status.name = fallback.Id;
            status.hideFlags = HideFlags.DontSave;
            status.Kind = kind;
            status.StatusTag = fallback.Id;
            status.StatusName = !string.IsNullOrWhiteSpace(label)
                ? label
                : catalogDefinition != null && !string.IsNullOrWhiteSpace(catalogDefinition.StatusEffectLabel)
                    ? catalogDefinition.StatusEffectLabel
                    : fallback.DisplayName;

            var defaultDuration = catalogDefinition != null
                ? catalogDefinition.DefaultDurationSeconds
                : fallback.DefaultDurationSeconds;
            var sourceDuration = source != null ? source.StatusDurationSeconds : 0f;
            status.Duration = sourceDuration > 0f ? sourceDuration : defaultDuration;

            var sourceMaxStacks = source != null ? source.StatusMaxStacks : 0;
            status.MaxStacks = sourceMaxStacks > 0
                ? sourceMaxStacks
                : catalogDefinition != null ? catalogDefinition.MaxStacks : fallback.DefaultMaxStacks;
            status.IsStackable = status.MaxStacks != 1;

            var sourceStacks = source != null ? source.StatusStackAmount : 0;
            status.BaseStackAmount = sourceStacks > 0
                ? sourceStacks
                : catalogDefinition != null && catalogDefinition.BaseStackAmount > 0 ? catalogDefinition.BaseStackAmount : 1;

            var catalogPermanent = catalogDefinition != null ? catalogDefinition.IsPermanent : fallback.Permanent;
            status.Permanent = catalogPermanent && status.Duration <= 0f;
            status.CanMove = catalogDefinition == null || catalogDefinition.CanMove;
            status.CanAct = catalogDefinition == null || catalogDefinition.CanAct;
            status.CanUseSpecialSkill = catalogDefinition == null || catalogDefinition.CanUseSpecialSkill;

            var moveSpeedBonus = ResolveOverride(source != null ? source.StatusMoveSpeedBonus : 0f, catalogDefinition != null ? catalogDefinition.MoveSpeedBonusPerStack : 0f);
            status.MoveSpeedBonus = moveSpeedBonus;
            status.MovementSlowRate = moveSpeedBonus < 0f ? -moveSpeedBonus : 0f;
            status.DamageTakenBonus = ResolveOverride(source != null ? source.StatusDamageTakenBonus : 0f, catalogDefinition != null ? catalogDefinition.DamageTakenBonusPerStack : 0f);
            status.CriticalDamageTakenBonus = ResolveOverride(source != null ? source.StatusCriticalDamageTakenBonus : 0f, catalogDefinition != null ? catalogDefinition.CriticalDamageTakenBonusPerStack : 0f);
            status.AilmentResistanceBonus = ResolveOverride(source != null ? source.StatusAilmentResistanceBonus : 0f, 0f);
            status.CriticalResistanceBonus = ResolveOverride(source != null ? source.StatusCriticalResistanceBonus : 0f, catalogDefinition != null ? catalogDefinition.CriticalResistanceBonusPerStack : 0f);
            status.ElementResistReduction = ResolveOverride(source != null ? source.StatusElementResistReduction : 0f, catalogDefinition != null ? catalogDefinition.ElementResistReductionPerStack : 0f);
            status.FlatElementResistReduction = ResolveOverride(source != null ? source.StatusFlatElementResistReduction : 0f, 0f);
            status.ElementDamageTakenBonus = ResolveOverride(source != null ? source.StatusElementDamageTakenBonus : 0f, catalogDefinition != null ? catalogDefinition.ElementDamageTakenBonusPerStack : 0f);

            var actionSpeedBonus = ResolveOverride(source != null ? source.StatusActionSpeedBonus : 0f, catalogDefinition != null ? catalogDefinition.ActionSpeedBonusPerStack : 0f);
            var attackPowerBonus = ResolveOverride(source != null ? source.StatusAttackPowerBonus : 0f, catalogDefinition != null ? catalogDefinition.AttackPowerBonusPerStack : 0f);
            status.Modifiers.ActionSpeedBonus = actionSpeedBonus;
            status.Modifiers.AttackPowerBonus = attackPowerBonus;
            status.Modifiers.SpellPowerBonus = 0f;
            status.Modifiers.DamageBonusRate = 0f;
            status.Modifiers.ShieldReceivedBonus = 0f;
            status.Modifiers.CritChanceBonusRate = 0f;

            if (catalogDefinition != null && catalogDefinition.HasAttribute)
            {
                status.HasElementModifierTarget = true;
                status.ElementModifierTarget = MapElement(catalogDefinition.Attribute);
                status.Modifiers.ResistReductionElement = status.ElementModifierTarget;
            }

            ApplySourceAwareMetadata(status, kind, source);
            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.IsControlEffect = !status.CanMove || !status.CanAct || !status.CanUseSpecialSkill;
            status.StatusEffectPrefab = source != null && source.StatusEffectPrefab != null
                ? source.StatusEffectPrefab
                : catalogDefinition != null ? catalogDefinition.StatusEffectPrefab : null;
            return status;
        }

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

        public static float ComputeModifierMagnitude(StatusEffectData data)
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
                + Mathf.Abs(data.MoveSpeedBonus)
                + Mathf.Abs(data.DamageTakenBonus)
                + Mathf.Abs(data.CriticalDamageTakenBonus)
                + Mathf.Abs(data.AilmentResistanceBonus)
                + Mathf.Abs(data.CriticalResistanceBonus)
                + Mathf.Abs(data.ElementResistReduction)
                + Mathf.Abs(data.FlatElementResistReduction)
                + Mathf.Abs(data.ElementDamageTakenBonus)
                + Mathf.Abs(data.ConditionalDamageTakenBonus);
        }

        public static bool CanMove(BaseUnitRuntimeModel model)
        {
            return !HasAnyStatus(model, data => !data.CanMove);
        }

        public static bool CanAct(BaseUnitRuntimeModel model)
        {
            return !HasAnyStatus(model, data => !data.CanAct);
        }

        public static bool CanUseSpecialSkill(BaseUnitRuntimeModel model)
        {
            return !HasAnyStatus(model, data => !data.CanUseSpecialSkill);
        }

        public static float ResolveActionSpeedMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(MinimumActionMultiplier, 1f + SumStacked(model, data => data.Modifiers.ActionSpeedBonus));
        }

        public static float ResolveMoveSpeedMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.MoveSpeedBonus));
        }

        public static float ResolveAttackPowerMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.AttackPowerBonus));
        }

        public static float ResolveSpellPowerMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.SpellPowerBonus));
        }

        public static float ResolveShieldReceivedMultiplier(BaseUnitRuntimeModel model)
        {
            return Mathf.Max(0f, 1f + SumStacked(model, data => data.Modifiers.ShieldReceivedBonus));
        }

        public static float ResolveCriticalChanceBonus(BaseUnitRuntimeModel model)
        {
            return SumStacked(model, data => data.Modifiers.CritChanceBonusRate);
        }

        public static float ResolveOutgoingDamageMultiplier(BaseUnitRuntimeModel source, DamageAttribute attribute)
        {
            return Mathf.Max(0f, 1f + SumStacked(source, data =>
                MatchesAttribute(data, attribute) ? data.Modifiers.DamageBonusRate : 0f));
        }

        public static float ResolveIncomingDamageMultiplier(BaseUnitRuntimeModel target, BaseUnitRuntimeModel source, DamageAttribute attribute)
        {
            return Mathf.Max(0f, 1f + SumStacked(target, data =>
            {
                var bonus = data.DamageTakenBonus;
                if (MatchesAttribute(data, attribute))
                {
                    bonus += data.ElementDamageTakenBonus;
                }

                if (MatchesConditionalSourceStatus(source, data))
                {
                    bonus += data.ConditionalDamageTakenBonus;
                }

                return bonus;
            }));
        }

        public static float ResolveElementResistReduction(BaseUnitRuntimeModel target, DamageAttribute attribute)
        {
            return Mathf.Clamp01(SumStacked(target, data => MatchesAttribute(data, attribute) ? data.ElementResistReduction : 0f));
        }

        public static float ResolveFlatElementResistReduction(BaseUnitRuntimeModel target, DamageAttribute attribute)
        {
            return Mathf.Max(0f, SumStacked(target, data => MatchesAttribute(data, attribute) ? data.FlatElementResistReduction : 0f));
        }

        public static float ResolveCriticalDamageTakenBonus(BaseUnitRuntimeModel target)
        {
            return SumStacked(target, data => data.CriticalDamageTakenBonus);
        }

        public static float ResolveAilmentResistanceBonus(BaseUnitRuntimeModel target)
        {
            return Mathf.Clamp01(SumStacked(target, data => data.AilmentResistanceBonus));
        }

        public static float ResolveCriticalResistanceBonus(BaseUnitRuntimeModel target)
        {
            return SumStacked(target, data => data.CriticalResistanceBonus);
        }

        private static bool HasAnyStatus(BaseUnitRuntimeModel model, System.Func<StatusEffectData, bool> predicate)
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

        private static float SumStacked(BaseUnitRuntimeModel model, System.Func<StatusEffectData, float> selector)
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

        private static StatusEffectData ResolveRuntimeData(UnitStatusRuntime runtime)
        {
            if (runtime == null || runtime.Kind == StatusEffectKind.None)
            {
                return null;
            }

            return runtime.SourceData != null
                ? runtime.SourceData
                : CreateStatusData(runtime.Kind, runtime.DisplayName);
        }

        private static StatusEffectDefinitionData ResolveCatalogDefinition(StatusEffectKind kind)
        {
            var id = StatusEffectUtility.ToId(kind);
            return !string.IsNullOrWhiteSpace(id)
                && PakuriDataManager.Instance.TryGetData<StatusEffectDefinitionData>(id, out var definition)
                    ? definition
                    : null;
        }

        private static float ResolveOverride(float sourceValue, float defaultValue)
        {
            return !Mathf.Approximately(sourceValue, 0f) ? sourceValue : defaultValue;
        }

        private static bool MatchesAttribute(StatusEffectData data, DamageAttribute attribute)
        {
            return data != null && data.HasElementModifierTarget && (DamageAttribute)(int)data.ElementModifierTarget == attribute;
        }

        private static bool MatchesConditionalSourceStatus(BaseUnitRuntimeModel source, StatusEffectData data)
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

        private static void ApplySourceAwareMetadata(StatusEffectData status, StatusEffectKind kind, SkillDefinition source)
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

        private static StatusMergePolicy ResolveMergePolicy(SkillDefinition source)
        {
            if (source != null && TryParseStatusMergePolicy(source.StatusMergePolicy, out var parsed))
            {
                return parsed;
            }

            return StatusMergePolicy.SameSourceRefresh;
        }

        private static ShieldRefreshRule ResolveShieldRefreshPolicy(SkillDefinition source)
        {
            if (source != null && TryParseShieldRefreshPolicy(source.ShieldAmountRefreshPolicy, out var parsed))
            {
                return parsed;
            }

            return ShieldRefreshRule.TakeHighest;
        }

        private static ElementType MapElement(DamageAttribute attribute)
        {
            return (ElementType)(int)attribute;
        }
    }
}
