using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private enum SkillNodeOwnerKind
        {
            Skill,
            Choice,
            Passive,
            Effect,
            Trigger
        }

        private enum SkillNodeValueType
        {
            String,
            Int,
            Float,
            Bool,
            Enum,
            AssetPath,
            SkillId,
            StatusId,
            ChoiceId
        }

        private sealed class SkillNodeRow
        {
            public string Id;
            public SkillNodeOwnerKind OwnerKind;
            public string OwnerId;
            public string TargetSkillId;
            public SkillExecutionPlanNodeKind NodeKind;
            public string HandlerId;
            public int SortOrder;
            public bool EnabledByDefault;
            public string RequiresActiveChoiceId;
            public string ExcludesActiveChoiceId;
            public string RequiresPassiveSkillId;
            public string ExcludesPassiveSkillId;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        private sealed class SkillNodeParamRow
        {
            public string NodeId;
            public string ParamKey;
            public SkillNodeValueType ValueType;
            public string Value;
        }

        private sealed class SkillNodeHandlerSchema
        {
            public SkillNodeHandlerSchema(
                string handlerId,
                SkillExecutionPlanNodeKind nodeKind,
                string[] requiredParams,
                string[] optionalParams = null,
                Dictionary<string, string[]> enumParamAllowedValues = null)
            {
                HandlerId = handlerId;
                NodeKind = nodeKind;
                RequiredParams = new HashSet<string>(requiredParams ?? new string[0], StringComparer.OrdinalIgnoreCase);
                AllowedParams = new HashSet<string>(RequiredParams, StringComparer.OrdinalIgnoreCase);
                EnumParamAllowedValues = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                if (optionalParams != null)
                {
                    for (var i = 0; i < optionalParams.Length; i++)
                    {
                        AllowedParams.Add(optionalParams[i]);
                    }
                }

                if (enumParamAllowedValues == null)
                {
                    return;
                }

                foreach (var entry in enumParamAllowedValues)
                {
                    EnumParamAllowedValues[entry.Key] =
                        new HashSet<string>(entry.Value ?? new string[0], StringComparer.OrdinalIgnoreCase);
                }
            }

            public string HandlerId { get; }
            public SkillExecutionPlanNodeKind NodeKind { get; }
            public HashSet<string> RequiredParams { get; }
            public HashSet<string> AllowedParams { get; }
            public Dictionary<string, HashSet<string>> EnumParamAllowedValues { get; }
        }

        private static readonly Dictionary<string, SkillNodeHandlerSchema> SkillNodeHandlerSchemas =
            BuildSkillNodeHandlerSchemas();

        private static SkillNodeRow ParseSkillNodeRow(CsvRecord record)
        {
            return new SkillNodeRow
            {
                Id = record.ReadRequiredString("node_id"),
                OwnerKind = record.ReadEnum<SkillNodeOwnerKind>("owner_kind"),
                OwnerId = record.ReadRequiredString("owner_id"),
                TargetSkillId = record.ReadString("target_skill_id"),
                NodeKind = record.ReadEnum<SkillExecutionPlanNodeKind>("node_kind"),
                HandlerId = record.ReadRequiredString("handler_id"),
                SortOrder = record.ReadInt("sort_order"),
                EnabledByDefault = ReadOptionalBoolWithDefaultIfColumnExists(record, "enabled_by_default", true),
                RequiresActiveChoiceId = ReadOptionalStringIfColumnExists(record, "requires_active_choice_id"),
                ExcludesActiveChoiceId = ReadOptionalStringIfColumnExists(record, "excludes_active_choice_id"),
                RequiresPassiveSkillId = ReadOptionalStringIfColumnExists(record, "requires_passive_skill_id"),
                ExcludesPassiveSkillId = ReadOptionalStringIfColumnExists(record, "excludes_passive_skill_id"),
                RuntimeSupportState = ReadOptionalStringIfColumnExists(record, "runtime_support_state"),
                RuntimeSupportNotes = ReadOptionalStringIfColumnExists(record, "runtime_support_notes")
            };
        }

        private static SkillNodeParamRow ParseSkillNodeParamRow(CsvRecord record)
        {
            return new SkillNodeParamRow
            {
                NodeId = record.ReadRequiredString("node_id"),
                ParamKey = record.ReadRequiredString("param_key"),
                ValueType = ParseSkillNodeValueType(record.ReadRequiredString("value_type"), record),
                Value = record.ReadString("value")
            };
        }

        private static SkillNodeValueType ParseSkillNodeValueType(string rawValue, CsvRecord record)
        {
            var normalized = rawValue.Trim().Replace("-", "_");
            switch (normalized.ToLowerInvariant())
            {
                case "string":
                    return SkillNodeValueType.String;
                case "int":
                    return SkillNodeValueType.Int;
                case "float":
                    return SkillNodeValueType.Float;
                case "bool":
                    return SkillNodeValueType.Bool;
                case "enum":
                    return SkillNodeValueType.Enum;
                case "asset_path":
                    return SkillNodeValueType.AssetPath;
                case "skill_id":
                    return SkillNodeValueType.SkillId;
                case "status_id":
                    return SkillNodeValueType.StatusId;
                case "choice_id":
                    return SkillNodeValueType.ChoiceId;
                default:
                    throw new CsvFatalException(
                        $"CSV row {record.RowNumber} in '{record.TableName}' has unsupported value_type '{rawValue}'.");
            }
        }

        private static Dictionary<string, SkillNodeHandlerSchema> BuildSkillNodeHandlerSchemas()
        {
            var schemas = new Dictionary<string, SkillNodeHandlerSchema>(StringComparer.OrdinalIgnoreCase);
            AddSkillNodeHandlerSchema(schemas, "TargetHealthRatioCondition", SkillExecutionPlanNodeKind.CastCondition,
                new[] { "threshold" }, new[] { "reject_if_missing_target" });
            AddSkillNodeHandlerSchema(schemas, "ExecuteDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "multiplier" }, new[] { "threshold_source" });
            AddSkillNodeHandlerSchema(schemas, "TargetPredicateDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "predicate", "multiplier" }, enumParamAllowedValues: EnumParamValues(
                    "predicate", "is_boss"));
            AddSkillNodeHandlerSchema(schemas, "CooldownRefund", SkillExecutionPlanNodeKind.OnKillAction,
                new[] { "ratio" });
            AddSkillNodeHandlerSchema(schemas, "DamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "ShieldAmountMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "CountStatusDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "status_id", "target_side", "amount_per_count" }, new[] { "max_count" }, EnumParamValues(
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "CooldownMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "MagazineBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "ReloadTimeMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "PierceBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "HitTargetCountBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "RadiusMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "RadiusBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "DurationBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus_seconds" });
            AddSkillNodeHandlerSchema(schemas, "StatusActionSpeedBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" }, new[] { "status_id" });
            AddSkillNodeHandlerSchema(schemas, "StatusAttackPowerBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusAilmentResistanceBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusDamageBonusRate", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" }, new[] { "attribute" }, EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "StatusShieldReceivedBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusCriticalChanceBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusDamageTakenBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusFlatElementResistReduction", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" }, new[] { "attribute" }, EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "StatusDurationBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id", "bonus_seconds" });
            AddSkillNodeHandlerSchema(schemas, "StatusConditionalDamageTakenBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "source_status_id", "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusElementDamageTakenBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusCriticalDamageTakenBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusSpellPowerBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "ApplyStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" },
                new[]
                {
                    "status_chance",
                    "status_label",
                    "status_effect_prefab_path",
                    "status_max_stacks",
                    "status_stack_amount",
                    "status_target_scope",
                    "status_merge_policy",
                    "shield_amount_refresh_policy"
                });
            AddSkillNodeHandlerSchema(schemas, "ApplyShield", SkillExecutionPlanNodeKind.Action,
                Array.Empty<string>(),
                new[]
                {
                    "base_damage",
                    "attack_power_coefficient",
                    "spell_power_coefficient",
                    "damage_multiplier",
                    "status_chance",
                    "status_label",
                    "status_effect_prefab_path",
                    "status_max_stacks",
                    "status_stack_amount",
                    "status_target_scope",
                    "status_merge_policy",
                    "shield_amount_refresh_policy"
                });
            AddSkillNodeHandlerSchema(schemas, "StatusModifier", SkillExecutionPlanNodeKind.Action,
                Array.Empty<string>(),
                new[]
                {
                    "status_chance",
                    "status_label",
                    "status_effect_prefab_path",
                    "status_max_stacks",
                    "status_stack_amount",
                    "status_target_scope",
                    "status_merge_policy"
                });
            AddSkillNodeHandlerSchema(schemas, "EffectStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" },
                new[]
                {
                    "status_chance",
                    "status_label",
                    "status_effect_prefab_path",
                    "status_max_stacks",
                    "status_stack_amount",
                    "status_target_scope",
                    "status_merge_policy",
                    "shield_amount_refresh_policy"
                });
            AddSkillNodeHandlerSchema(schemas, "EffectDamage", SkillExecutionPlanNodeKind.Action,
                new[] { "attribute" },
                new[]
                {
                    "base_damage",
                    "attack_power_coefficient",
                    "spell_power_coefficient",
                    "damage_multiplier",
                    "radius",
                    "tick_interval_seconds"
                },
                enumParamAllowedValues: EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "EffectExtendStatusDuration", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" });
            AddSkillNodeHandlerSchema(schemas, "EffectTarget", SkillExecutionPlanNodeKind.Action,
                Array.Empty<string>(),
                new[]
                {
                    "target_side",
                    "target_selection",
                    "target_shape",
                    "center_mode",
                    "visual_anchor_mode",
                    "effect_timing",
                    "delay_seconds",
                    "apply_once",
                    "cover_all"
                },
                EffectBaseEnumParamValues());
            AddSkillNodeHandlerSchema(schemas, "EffectVisual", SkillExecutionPlanNodeKind.Action,
                new[] { "skill_effect_prefab_path" });
            AddSkillNodeHandlerSchema(schemas, "ConditionStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" }, new[] { "target_side", "source_skill_id", "min_stacks" }, EnumParamValues(
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "ConditionSkillAttribute", SkillExecutionPlanNodeKind.Action,
                new[] { "attribute" }, enumParamAllowedValues: EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "EffectLifetime", SkillExecutionPlanNodeKind.Action,
                new[] { "duration_seconds" });
            AddSkillNodeHandlerSchema(schemas, "DelayedDamage", SkillExecutionPlanNodeKind.Action,
                new[] { "delay_seconds" });
            AddSkillNodeHandlerSchema(schemas, "RequiredTargetStatus", SkillExecutionPlanNodeKind.CastCondition,
                new[] { "status_id" }, new[] { "min_stacks" });
            AddSkillNodeHandlerSchema(schemas, "TargetStatusStackDamage", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "status_id" }, new[] { "max_stacks", "base_damage", "attack_power_coefficient", "spell_power_coefficient", "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "ConsumeTargetStatus", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "status_id" }, new[] { "ratio", "stacks" });
            AddSkillNodeHandlerSchema(schemas, "CooldownReset", SkillExecutionPlanNodeKind.OnKillAction,
                new string[0], new[] { "requires_execute" });
            AddSkillNodeHandlerSchema(schemas, "AdditionalDamage", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "multiplier" }, new[] { "base_damage", "chance", "attribute", "target", "target_side" }, EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute)),
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "EveryNthHitChainDamage", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "hit_count", "multiplier" }, new[] { "radius", "max_targets", "attribute", "target_side" }, EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute)),
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "HitCountCooldownRefund", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "hit_count", "ratio" });
            AddSkillNodeHandlerSchema(schemas, "RepeatPerTarget", SkillExecutionPlanNodeKind.Action,
                new[] { "repeat_count", "repeat_interval_seconds", "repeat_damage_multiplier" });
            AddSkillNodeHandlerSchema(schemas, "TargetStatusCritBonus", SkillExecutionPlanNodeKind.CritModifier,
                new[] { "status_id" }, new[] { "crit_chance_bonus", "crit_damage_bonus", "min_stacks" });
            AddSkillNodeHandlerSchema(schemas, "RedistributeConsumedStatus", SkillExecutionPlanNodeKind.OnKillAction,
                new[] { "status_id", "ratio" }, new[] { "radius", "stacks", "target_count" });
            AddSkillNodeHandlerSchema(schemas, "TargetHealthRatioThresholdBonus", SkillExecutionPlanNodeKind.CastCondition,
                new[] { "threshold_bonus" });
            AddSkillNodeHandlerSchema(schemas, "ExecuteCritChanceBonus", SkillExecutionPlanNodeKind.CritModifier,
                new[] { "crit_chance_bonus" });
            AddSkillNodeHandlerSchema(schemas, "CooldownRefundBonus", SkillExecutionPlanNodeKind.OnKillAction,
                new[] { "ratio_bonus" });
            AddSkillNodeHandlerSchema(schemas, "BranchProjectile", SkillExecutionPlanNodeKind.Action,
                new string[0], new[] { "projectile_index", "count", "angle", "damage_multiplier" });
            AddSkillNodeHandlerSchema(schemas, "SpawnProjectile", SkillExecutionPlanNodeKind.Action,
                new string[0], new[] { "projectile_prefab_path", "projectile_sprite_path", "count", "speed" });
            AddSkillNodeHandlerSchema(schemas, "ApplyStatus", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "status_id" }, new[] { "stacks", "duration_seconds", "chance", "target_side" }, EnumParamValues(
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "BossDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "CooldownResetOnKill", SkillExecutionPlanNodeKind.OnKillAction,
                new string[0], new[] { "requires_execute" });
            return schemas;
        }

        private static string[] EffectBaseRequiredParams(params string[] extraParams)
        {
            var required = new List<string>();
            if (extraParams != null)
            {
                required.AddRange(extraParams);
            }

            return required.ToArray();
        }

        private static string[] EffectBaseOptionalParams(params string[] extraParams)
        {
            var optional = new List<string>
            {
                "target_side",
                "target_selection",
                "target_shape",
                "center_mode",
                "visual_anchor_mode",
                "effect_timing",
                "delay_seconds",
                "apply_once",
                "cover_all",
                "attribute",
                "skill_effect_prefab_path",
                "required_source_status_id",
                "required_source_status_min_stacks"
            };
            if (extraParams != null)
            {
                optional.AddRange(extraParams);
            }

            return optional.ToArray();
        }

        private static Dictionary<string, string[]> EffectBaseEnumParamValues()
        {
            return EnumParamValues(
                "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide)),
                "target_selection", Enum.GetNames(typeof(SkillMultiEffectTargetSelection)),
                "target_shape", Enum.GetNames(typeof(SkillMultiEffectTargetShape)),
                "center_mode", Enum.GetNames(typeof(SkillMultiEffectCenterMode)),
                "visual_anchor_mode", Enum.GetNames(typeof(SkillMultiEffectVisualAnchorMode)),
                "effect_timing", Enum.GetNames(typeof(SkillMultiEffectTiming)),
                "attribute", Enum.GetNames(typeof(DamageAttribute)));
        }

        private static void AddSkillNodeHandlerSchema(
            Dictionary<string, SkillNodeHandlerSchema> schemas,
            string handlerId,
            SkillExecutionPlanNodeKind nodeKind,
            string[] requiredParams,
            string[] optionalParams = null,
            Dictionary<string, string[]> enumParamAllowedValues = null)
        {
            schemas.Add(handlerId, new SkillNodeHandlerSchema(
                handlerId,
                nodeKind,
                requiredParams,
                optionalParams,
                enumParamAllowedValues));
        }

        private static Dictionary<string, string[]> EnumParamValues(params object[] values)
        {
            var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i + 1 < values.Length; i += 2)
            {
                result.Add((string)values[i], (string[])values[i + 1]);
            }

            return result;
        }

        private static Dictionary<string, string[]> EnumParamValues(string paramKey, params string[] allowedValues)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { paramKey, allowedValues }
            };
        }

        private static void ValidateNormalizedSkillAuthoringRows(
            SourceModel model,
            PakuriCsvRuntimeAssetCatalog assetCatalog,
            List<string> errors)
        {
            if (model == null)
            {
                return;
            }

            var paramsByNode = new Dictionary<string, List<SkillNodeParamRow>>(StringComparer.OrdinalIgnoreCase);
            var paramKeyLookupByNode = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var param in model.SkillNodeParams)
            {
                if (!model.SkillNodes.ContainsKey(param.NodeId))
                {
                    errors.Add($"Skill node param '{param.ParamKey}' references unknown node_id '{param.NodeId}'.");
                    continue;
                }

                if (!paramsByNode.TryGetValue(param.NodeId, out var list))
                {
                    list = new List<SkillNodeParamRow>();
                    paramsByNode.Add(param.NodeId, list);
                    paramKeyLookupByNode.Add(param.NodeId, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
                }

                if (!paramKeyLookupByNode[param.NodeId].Add(param.ParamKey))
                {
                    errors.Add($"Skill node '{param.NodeId}' has duplicate param '{param.ParamKey}'.");
                }

                list.Add(param);
                ValidateSkillNodeParamValue(param, model, assetCatalog, errors);
            }

            foreach (var node in model.SkillNodes.Values)
            {
                ValidateSkillNodeOwner(node, model, errors);
                ValidateSkillNodeGateReferences(node, model, errors);

                if (string.IsNullOrWhiteSpace(node.HandlerId))
                {
                    errors.Add($"Skill node '{node.Id}' requires handler_id.");
                    continue;
                }

                if (!SkillNodeHandlerSchemas.TryGetValue(node.HandlerId, out var schema))
                {
                    errors.Add($"Skill node '{node.Id}' uses unregistered handler_id '{node.HandlerId}'.");
                    continue;
                }

                if (node.NodeKind != schema.NodeKind)
                {
                    errors.Add(
                        $"Skill node '{node.Id}' handler '{node.HandlerId}' requires node_kind '{schema.NodeKind}' but row uses '{node.NodeKind}'.");
                }

                if (!paramsByNode.TryGetValue(node.Id, out var nodeParams))
                {
                    nodeParams = new List<SkillNodeParamRow>();
                }

                ValidateSkillNodeParams(node, schema, nodeParams, errors);
                ValidateSkillNodeLegacyOverlap(node, model, errors);
            }
        }

        private static void ValidateSkillNodeOwner(SkillNodeRow node, SourceModel model, List<string> errors)
        {
            switch (node.OwnerKind)
            {
                case SkillNodeOwnerKind.Skill:
                    if (!model.Skills.ContainsKey(node.OwnerId))
                    {
                        errors.Add($"Skill node '{node.Id}' references unknown owner skill '{node.OwnerId}'.");
                    }
                    break;
                case SkillNodeOwnerKind.Passive:
                    if (!model.Skills.TryGetValue(node.OwnerId, out var passive) || passive.SkillKind != PakuriCsvSkillKind.Passive)
                    {
                        errors.Add($"Skill node '{node.Id}' references unknown passive owner '{node.OwnerId}'.");
                    }
                    break;
                case SkillNodeOwnerKind.Choice:
                    if (!model.SkillChoices.ContainsKey(node.OwnerId))
                    {
                        errors.Add($"Skill node '{node.Id}' references unknown choice owner '{node.OwnerId}'.");
                    }
                    break;
                case SkillNodeOwnerKind.Effect:
                    if (string.IsNullOrWhiteSpace(node.OwnerId))
                    {
                        errors.Add($"Skill node '{node.Id}' requires owner_id for effect-owned nodes.");
                    }
                    if (string.IsNullOrWhiteSpace(node.TargetSkillId) || !model.Skills.ContainsKey(node.TargetSkillId))
                    {
                        errors.Add($"Skill node '{node.Id}' effect owner '{node.OwnerId}' requires a known target_skill_id.");
                    }
                    break;
                case SkillNodeOwnerKind.Trigger:
                    errors.Add($"Skill node '{node.Id}' uses owner_kind 'Trigger', but trigger-owned normalized nodes are not wired into runtime plans yet.");
                    if (!model.SkillTriggers.ContainsKey(node.OwnerId))
                    {
                        errors.Add($"Skill node '{node.Id}' references unknown trigger owner '{node.OwnerId}'.");
                    }
                    break;
                default:
                    errors.Add($"Skill node '{node.Id}' uses unsupported owner_kind '{node.OwnerKind}'.");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(node.TargetSkillId) && !model.Skills.ContainsKey(node.TargetSkillId))
            {
                errors.Add($"Skill node '{node.Id}' references unknown target_skill_id '{node.TargetSkillId}'.");
            }
        }

        private static void ValidateSkillNodeGateReferences(SkillNodeRow node, SourceModel model, List<string> errors)
        {
            ValidateChoiceGate(node.Id, "requires_active_choice_id", node.RequiresActiveChoiceId, model, errors);
            ValidateChoiceGate(node.Id, "excludes_active_choice_id", node.ExcludesActiveChoiceId, model, errors);
            ValidatePassiveGate(node.Id, "requires_passive_skill_id", node.RequiresPassiveSkillId, model, errors);
            ValidatePassiveGate(node.Id, "excludes_passive_skill_id", node.ExcludesPassiveSkillId, model, errors);
        }

        private static void ValidateChoiceGate(
            string nodeId,
            string columnName,
            string choiceId,
            SourceModel model,
            List<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(choiceId) && !model.SkillChoices.ContainsKey(choiceId))
            {
                errors.Add($"Skill node '{nodeId}' {columnName} references unknown choice '{choiceId}'.");
            }
        }

        private static void ValidatePassiveGate(
            string nodeId,
            string columnName,
            string passiveId,
            SourceModel model,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(passiveId))
            {
                return;
            }

            if (!model.Skills.TryGetValue(passiveId, out var passive) || passive.SkillKind != PakuriCsvSkillKind.Passive)
            {
                errors.Add($"Skill node '{nodeId}' {columnName} references unknown passive '{passiveId}'.");
            }
        }

        private static void ValidateSkillNodeParams(
            SkillNodeRow node,
            SkillNodeHandlerSchema schema,
            List<SkillNodeParamRow> nodeParams,
            List<string> errors)
        {
            var actualParams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < nodeParams.Count; i++)
            {
                var param = nodeParams[i];
                actualParams.Add(param.ParamKey);
                if (!schema.AllowedParams.Contains(param.ParamKey))
                {
                    errors.Add($"Skill node '{node.Id}' handler '{node.HandlerId}' has unknown param '{param.ParamKey}'.");
                    continue;
                }

                if (schema.EnumParamAllowedValues.ContainsKey(param.ParamKey))
                {
                    if (param.ValueType != SkillNodeValueType.Enum)
                    {
                        errors.Add(
                            $"Skill node '{node.Id}' handler '{node.HandlerId}' param '{param.ParamKey}' must use value_type 'enum' but row uses '{param.ValueType}'.");
                    }

                    ValidateSkillNodeSchemaEnumParam(node, schema, param, errors);
                }
                else if (param.ValueType == SkillNodeValueType.Enum)
                {
                    ValidateSkillNodeSchemaEnumParam(node, schema, param, errors);
                }
            }

            foreach (var requiredParam in schema.RequiredParams)
            {
                if (!actualParams.Contains(requiredParam))
                {
                    errors.Add($"Skill node '{node.Id}' handler '{node.HandlerId}' is missing required param '{requiredParam}'.");
                }
            }
        }

        private static void ValidateSkillNodeLegacyOverlap(SkillNodeRow node, SourceModel model, List<string> errors)
        {
            if (node == null || !node.EnabledByDefault || string.IsNullOrWhiteSpace(node.HandlerId))
            {
                return;
            }

            switch (node.OwnerKind)
            {
                case SkillNodeOwnerKind.Skill:
                    if (model.Skills.TryGetValue(node.OwnerId, out var skill))
                    {
                        ValidateSkillNodeLegacySkillOverlap(node, skill, errors);
                    }
                    break;
                case SkillNodeOwnerKind.Choice:
                    if (model.SkillChoices.TryGetValue(node.OwnerId, out var choice))
                    {
                        ValidateSkillNodeLegacyChoiceOverlap(node, choice, errors);
                    }
                    break;
            }
        }

        private static void ValidateSkillNodeLegacySkillOverlap(
            SkillNodeRow node,
            SkillRow skill,
            List<string> errors)
        {
            if (string.Equals(node.HandlerId, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase)
                && skill.RequireExecuteThresholdToCast
                && !NearlyZero(skill.ExecuteHealthRatioThreshold))
            {
                AddLegacyOverlapError(node, "execute threshold wide columns", errors);
            }

            if (string.Equals(node.HandlerId, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                && !NearlyEqual(skill.ExecuteDamageMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "execute_damage_multiplier", errors);
            }

            if ((string.Equals(node.HandlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(node.HandlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
                && !NearlyEqual(skill.BossDamageMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "boss_damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "CooldownRefund", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(skill.KillCooldownRefundRatio))
            {
                AddLegacyOverlapError(node, "kill_cooldown_refund_ratio", errors);
            }
        }

        private static void ValidateSkillNodeLegacyChoiceOverlap(
            SkillNodeRow node,
            SkillChoiceRow choice,
            List<string> errors)
        {
            if (string.Equals(node.HandlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasDamageMultiplier
                && !NearlyEqual(choice.DamageMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasDamageMultiplier
                && !NearlyEqual(choice.DamageMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.CountStatusId)
                    || !NearlyZero(choice.DamageMultiplierPerCount)
                    || choice.CountMax > 0))
            {
                AddLegacyOverlapError(node, "count_status_id/damage_multiplier_per_count/count_max", errors);
            }

            if (string.Equals(node.HandlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasCooldownMultiplier
                && !NearlyEqual(choice.CooldownMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "cooldown_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasMagazineBonus
                && choice.MagazineBonus != 0)
            {
                AddLegacyOverlapError(node, "magazine_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasReloadTimeMultiplier
                && !NearlyEqual(choice.ReloadTimeMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "reload_time_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase)
                && choice.PierceBonus != 0)
            {
                AddLegacyOverlapError(node, "pierce_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HitTargetCountBonus != 0)
            {
                AddLegacyOverlapError(node, "hit_target_count_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasRadiusMultiplier
                && !NearlyEqual(choice.RadiusMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "radius_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.RadiusBonus))
            {
                AddLegacyOverlapError(node, "radius_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.DurationBonus))
            {
                AddLegacyOverlapError(node, "duration_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasStatusActionSpeedBonus
                && !NearlyZero(choice.StatusActionSpeedBonus))
            {
                AddLegacyOverlapError(node, "status_action_speed_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasStatusAttackPowerBonus
                && !NearlyZero(choice.StatusAttackPowerBonus))
            {
                AddLegacyOverlapError(node, "status_attack_power_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasStatusAilmentResistanceBonus
                && !NearlyZero(choice.StatusAilmentResistanceBonus))
            {
                AddLegacyOverlapError(node, "status_ailment_resistance_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.StatusDurationBonusStatusId)
                    || !NearlyZero(choice.StatusDurationBonus)))
            {
                AddLegacyOverlapError(node, "status_duration_bonus_*", errors);
            }

            if (string.Equals(node.HandlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasStatusConditionalDamageTakenBonus)
            {
                AddLegacyOverlapError(node, "status_conditional_*", errors);
            }

            if (string.Equals(node.HandlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasStatusElementDamageTakenBonus
                && !NearlyZero(choice.StatusElementDamageTakenBonus))
            {
                AddLegacyOverlapError(node, "status_element_damage_taken_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasStatusCriticalDamageTakenBonus
                && !NearlyZero(choice.StatusCriticalDamageTakenBonus))
            {
                AddLegacyOverlapError(node, "status_critical_damage_taken_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasExecuteHealthRatioBonus
                && !NearlyZero(choice.ExecuteHealthRatioBonus))
            {
                AddLegacyOverlapError(node, "execute_health_ratio_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.ExecuteCritChanceBonus))
            {
                AddLegacyOverlapError(node, "execute_crit_chance_bonus", errors);
            }

            if ((string.Equals(node.HandlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(node.HandlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
                && choice.HasBossDamageMultiplier
                && !NearlyEqual(choice.BossDamageMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "boss_damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase)
                && choice.HasKillCooldownRefundRatioBonus
                && !NearlyZero(choice.KillCooldownRefundRatioBonus))
            {
                AddLegacyOverlapError(node, "kill_cooldown_refund_ratio_bonus", errors);
            }

            if ((string.Equals(node.HandlerId, "CooldownReset", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(node.HandlerId, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase))
                && choice.KillResetsCooldown)
            {
                AddLegacyOverlapError(node, "kill_resets_cooldown", errors);
            }

            if (string.Equals(node.HandlerId, "AdditionalDamage", StringComparison.OrdinalIgnoreCase)
                && choice.HasOnHitAdditionalDamage)
            {
                AddLegacyOverlapError(node, "on_hit_additional_damage_*", errors);
            }

            if (string.Equals(node.HandlerId, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase)
                && (choice.OnHitChainHitPeriod > 0
                    || choice.OnHitChainTargetCount > 0
                    || !NearlyZero(choice.OnHitChainSearchRadius)
                    || (choice.OnHitChainDamageMultiplier > 0f && !NearlyEqual(choice.OnHitChainDamageMultiplier, 1f))))
            {
                AddLegacyOverlapError(node, "on_hit_chain_*", errors);
            }

            if (string.Equals(node.HandlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase)
                && (choice.RepeatCountPerTarget > 0
                    || !NearlyZero(choice.RepeatIntervalSeconds)
                    || (choice.RepeatDamageMultiplier > 0f && !NearlyEqual(choice.RepeatDamageMultiplier, 1f))))
            {
                AddLegacyOverlapError(node, "repeat_*", errors);
            }

            if (string.Equals(node.HandlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase)
                && (!NearlyZero(choice.ConditionalCritChanceBonus)
                    || !string.IsNullOrWhiteSpace(choice.ConditionalCritTargetStatusId)
                    || choice.ConditionalCritTargetStatusMinStacks > 0))
            {
                AddLegacyOverlapError(node, "conditional_crit_*", errors);
            }

            if (string.Equals(node.HandlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase)
                && (!NearlyZero(choice.RedistributeConsumedStatusRatioOnKill)
                    || !string.IsNullOrWhiteSpace(choice.RedistributeConsumedStatusId)
                    || !NearlyZero(choice.RedistributeConsumedStatusSearchRadius)
                    || choice.RedistributeConsumedStatusTargetCount > 0))
            {
                AddLegacyOverlapError(node, "redistribute_consumed_status_*", errors);
            }
        }

        private static void AddLegacyOverlapError(SkillNodeRow node, string legacyColumn, List<string> errors)
        {
            errors.Add(
                $"Skill node '{node.Id}' handler '{node.HandlerId}' overlaps active legacy wide field '{legacyColumn}' on owner '{node.OwnerId}'. Disable one side before validation.");
        }

        private static bool NearlyZero(float value)
        {
            return Math.Abs(value) <= 0.0001f;
        }

        private static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= 0.0001f;
        }

        private static void ValidateSkillNodeParamValue(
            SkillNodeParamRow param,
            SourceModel model,
            PakuriCsvRuntimeAssetCatalog assetCatalog,
            List<string> errors)
        {
            var value = param.Value != null ? param.Value.Trim() : string.Empty;
            switch (param.ValueType)
            {
                case SkillNodeValueType.String:
                    return;
                case SkillNodeValueType.Int:
                    if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' value '{param.Value}' is not a valid int.");
                    }
                    return;
                case SkillNodeValueType.Float:
                    if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' value '{param.Value}' is not a valid float.");
                    }
                    return;
                case SkillNodeValueType.Bool:
                    if (!bool.TryParse(value, out _))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' value '{param.Value}' is not a valid bool.");
                    }
                    return;
                case SkillNodeValueType.Enum:
                    ValidateSkillNodeEnumParam(param, value, errors);
                    return;
                case SkillNodeValueType.AssetPath:
                    if (string.IsNullOrWhiteSpace(value)
                        || assetCatalog == null
                        || (!assetCatalog.HasSprite(value) && !assetCatalog.HasPrefab(value)))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' references unknown asset path '{param.Value}'.");
                    }
                    return;
                case SkillNodeValueType.SkillId:
                    if (string.IsNullOrWhiteSpace(value) || !model.Skills.ContainsKey(value))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' references unknown skill '{param.Value}'.");
                    }
                    return;
                case SkillNodeValueType.StatusId:
                    if (string.IsNullOrWhiteSpace(value) || (!model.StatusEffects.ContainsKey(value) && !StatusEffectUtility.TryParse(value, out _)))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' references unknown status '{param.Value}'.");
                    }
                    return;
                case SkillNodeValueType.ChoiceId:
                    if (string.IsNullOrWhiteSpace(value) || !model.SkillChoices.ContainsKey(value))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' references unknown choice '{param.Value}'.");
                    }
                    return;
                default:
                    errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' has unsupported value_type '{param.ValueType}'.");
                    return;
            }
        }

        private static void ValidateSkillNodeEnumParam(SkillNodeParamRow param, string value, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' requires a non-empty enum value.");
            }
        }

        private static void ValidateSkillNodeSchemaEnumParam(
            SkillNodeRow node,
            SkillNodeHandlerSchema schema,
            SkillNodeParamRow param,
            List<string> errors)
        {
            var value = param.Value != null ? param.Value.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!schema.EnumParamAllowedValues.TryGetValue(param.ParamKey, out var allowedValues))
            {
                errors.Add(
                    $"Skill node '{node.Id}' handler '{node.HandlerId}' param '{param.ParamKey}' is marked enum but has no registered enum value schema.");
                return;
            }

            if (!allowedValues.Contains(value))
            {
                errors.Add(
                    $"Skill node '{node.Id}' handler '{node.HandlerId}' param '{param.ParamKey}' has invalid enum value '{param.Value}'. Allowed values: {string.Join(", ", allowedValues)}.");
            }
        }
    }
}
