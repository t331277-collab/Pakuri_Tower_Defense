using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.CsvDataValidator;
using static Pakuri.Data.GameDataBuilder;


/*
 * 정규화된 스킬 노드와 그래프 CSV를 공통 실행 구조로 만드는 빌더.
 * 노드 형식·파라미터·소유자·선택 조건을 읽고 스키마와 참조를 검사하며
 * 기존 열과의 중복을 확인한 뒤 실행 가능한 그래프 노드와 효과 연결을 생성한다.
 */
namespace Pakuri.Data
{
    internal static class SkillGraphBuilder
    {
        internal enum SkillNodeOwnerKind
        {
            Skill,
            Choice,
            Passive,
            Effect,
            Trigger
        }

        internal enum SkillNodeValueType
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

        internal enum SkillGraphKind
        {
            Plan,
            Effect
        }

        /*
         * 스킬 또는 선택지가 소유한 실행 노드 한 행을 보관한다.
         */
        internal sealed class SkillNodeRow
        {
            public string Id;
            public string MonsterId;
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

        /*
         * 실행 노드에 전달할 매개변수 한 행을 보관한다.
         */
        internal sealed class SkillNodeParamRow
        {
            public string NodeId;
            public string MonsterId;
            public string ParamKey;
            public SkillNodeValueType ValueType;
            public string Value;
        }

        /*
         * 사용할 수 있는 노드 종류와 연결 처리기를 보관한다.
         */
        internal sealed class SkillNodeTypeRow
        {
            public string Id;
            public string HandlerId;
            public SkillExecutionPlanNodeKind NodeKind;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        /*
         * 노드 종류가 요구하는 매개변수 규칙 한 행을 보관한다.
         */
        internal sealed class SkillNodeTypeParamRow
        {
            public string NodeTypeId;
            public int ParamOrder;
            public string ParamKey;
            public SkillNodeValueType ValueType;
            public bool Required;
            public string AllowedValues;
        }

        /*
         * 소유자 그래프에 배치된 노드와 인자 값을 보관한다.
         */
        internal sealed class SkillGraphNodeRow
        {
            public string MonsterId;
            public SkillNodeOwnerKind OwnerKind;
            public string OwnerId;
            public SkillGraphKind GraphKind;
            public int GraphIndex;
            public string TargetSkillId;
            public int NodeOrder;
            public string NodeTypeId;
            public readonly string[] Args = new string[12];
            public string ExcludesActiveChoiceId;
        }

        /*
         * 노드 처리기가 허용하는 노드 종류와 매개변수 규칙을 제공한다.
         */
        internal sealed class SkillNodeHandlerSchema
        {
            /*
             * 스킬 노드 처리 규칙을 구성한다.
             */
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

        internal static readonly Dictionary<string, SkillNodeHandlerSchema> SkillNodeHandlerSchemas =
            BuildSkillNodeHandlerSchemas();

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillNodeTypeRow ParseSkillNodeTypeRow(CsvRecord record)
        {
            return new SkillNodeTypeRow
            {
                Id = record.ReadRequiredString("node_type_id"),
                HandlerId = record.ReadRequiredString("handler_id"),
                NodeKind = record.ReadEnum<SkillExecutionPlanNodeKind>("node_kind"),
                RuntimeSupportState = ReadOptionalStringIfColumnExists(record, "runtime_support_state"),
                RuntimeSupportNotes = ReadOptionalStringIfColumnExists(record, "runtime_support_notes")
            };
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillNodeTypeParamRow ParseSkillNodeTypeParamRow(CsvRecord record)
        {
            return new SkillNodeTypeParamRow
            {
                NodeTypeId = record.ReadRequiredString("node_type_id"),
                ParamOrder = record.ReadInt("param_order"),
                ParamKey = record.ReadRequiredString("param_key"),
                ValueType = ParseSkillNodeValueType(record.ReadRequiredString("value_type"), record),
                Required = record.ReadBool("required"),
                AllowedValues = ReadOptionalStringIfColumnExists(record, "allowed_values")
            };
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillGraphNodeRow ParseSkillGraphNodeRow(CsvRecord record)
        {
            var row = new SkillGraphNodeRow
            {
                MonsterId = record.ReadRequiredString("monster_id"),
                OwnerKind = record.ReadEnum<SkillNodeOwnerKind>("owner_kind"),
                OwnerId = record.ReadRequiredString("owner_id"),
                GraphKind = record.ReadEnum<SkillGraphKind>("graph_kind"),
                GraphIndex = record.ReadInt("graph_index"),
                TargetSkillId = ReadOptionalStringIfColumnExists(record, "target_skill_id"),
                NodeOrder = record.ReadInt("node_order"),
                NodeTypeId = record.ReadRequiredString("node_type_id"),
                ExcludesActiveChoiceId = ReadOptionalStringIfColumnExists(record, "excludes_active_choice_id")
            };

            for (var i = 0; i < row.Args.Length; i++)
            {
                row.Args[i] = ReadOptionalStringIfColumnExists(record, $"arg_{i + 1}");
            }

            return row;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillNodeValueType ParseSkillNodeValueType(string rawValue, CsvRecord record)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static Dictionary<string, SkillNodeHandlerSchema> BuildSkillNodeHandlerSchemas()
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
            AddSkillNodeHandlerSchema(schemas, "CritChanceBonus", SkillExecutionPlanNodeKind.CritModifier,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "CritDamageBonus", SkillExecutionPlanNodeKind.CritModifier,
                new[] { "bonus" });
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
            AddSkillNodeHandlerSchema(schemas, "BeamWidthBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "KnockbackDistanceMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "ReloadReducePerHit", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "target_skill_id", "seconds_per_hit" });
            AddSkillNodeHandlerSchema(schemas, "CoreDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "hitbox_name", "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "CoreAdditionalDamage", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "hitbox_name", "chance", "multiplier", "attribute" }, enumParamAllowedValues: EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "HitCountCooldownRefund", SkillExecutionPlanNodeKind.OnHitAction,
                new[] { "target_skill_id", "min_targets", "ratio" });
            AddSkillNodeHandlerSchema(schemas, "DurationBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus_seconds" });
            AddSkillNodeHandlerSchema(schemas, "DurationMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "DamageDelayMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "AdditionalProjectileBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "ShotIntervalMultiplier", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "ConsecutiveHitDamageBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus_rate", "max_bonus" });
            AddSkillNodeHandlerSchema(schemas, "BurstDamageRule", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "projectile_index", "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "FollowUpProjectile", SkillExecutionPlanNodeKind.Action,
                new[] { "count", "delay_seconds", "damage_multiplier" });
            AddSkillNodeHandlerSchema(schemas, "ThresholdApplyStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "source_status_id", "min_stacks", "apply_status_id" });
            AddSkillNodeHandlerSchema(schemas, "TargetStatusStackDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "ConsumeTargetStatusRatioOverride", SkillExecutionPlanNodeKind.Action,
                new[] { "ratio" });
            AddSkillNodeHandlerSchema(schemas, "BurstStatusStacksBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "projectile_index", "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusStackAmountBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id", "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusStackAmountSet", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id", "value" });
            AddSkillNodeHandlerSchema(schemas, "StatusMaxStacksBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id", "bonus" });
            AddSkillNodeHandlerSchema(schemas, "ConditionalDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "status_id", "min_stacks", "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "TargetStatusStackDamageRateBonus", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "status_id", "bonus_rate_per_stack" });
            AddSkillNodeHandlerSchema(schemas, "TriggerProcChanceBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "trigger_id", "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusActionSpeedBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" }, new[] { "status_id" });
            AddSkillNodeHandlerSchema(schemas, "StatusAttackPowerBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusMoveSpeedBonus", SkillExecutionPlanNodeKind.Action,
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
                new[] { "bonus" }, new[] { "attribute" }, EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "StatusConditionalStatusChanceBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_ids", "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusCriticalDamageTakenBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusCriticalDamageBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "StatusElementResistReduction", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" }, new[] { "attribute" }, EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "StatusOutgoingAdditionalDamage", SkillExecutionPlanNodeKind.Action,
                new[] { "multiplier", "trigger_attribute", "damage_attribute" }, enumParamAllowedValues: EnumParamValues(
                    "trigger_attribute", Enum.GetNames(typeof(DamageAttribute)),
                    "damage_attribute", Enum.GetNames(typeof(DamageAttribute))));
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
            AddSkillNodeHandlerSchema(schemas, "RecastZone", SkillExecutionPlanNodeKind.OnExpireAction,
                new[] { "source_skill_id", "delay_seconds", "duration_seconds", "radius_multiplier", "inherit_snapshot", "max_generation" });
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
            AddSkillNodeHandlerSchema(schemas, "AttachStatusPayload", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" },
                new[]
                {
                    "status_chance",
                    "status_label",
                    "status_duration_seconds",
                    "status_max_stacks",
                    "status_stack_amount",
                    "status_merge_policy"
                });
            AddSkillNodeHandlerSchema(schemas, "RequiredSourceStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" }, new[] { "min_stacks" });
            AddSkillNodeHandlerSchema(schemas, "StatusRuntimeKindFilter", SkillExecutionPlanNodeKind.Action,
                Array.Empty<string>(), new[] { "incoming_skill_runtime_kinds", "outgoing_skill_runtime_kinds" });
            AddSkillNodeHandlerSchema(schemas, "StatusCriticalResistanceBonus", SkillExecutionPlanNodeKind.Action,
                new[] { "bonus" });
            AddSkillNodeHandlerSchema(schemas, "RuntimeEffectVisual", SkillExecutionPlanNodeKind.Action,
                new[] { "runtime_visual_sprite_path" },
                new[]
                {
                    "runtime_visual_animator_controller_path",
                    "runtime_visual_scale",
                    "runtime_visual_sorting_order",
                    "runtime_hitbox_size_x",
                    "runtime_hitbox_size_y"
                });
            AddSkillNodeHandlerSchema(schemas, "ConditionStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id" }, new[] { "target_side", "source_skill_id", "min_stacks" }, EnumParamValues(
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "ConditionAnyStatus", SkillExecutionPlanNodeKind.Action,
                new[] { "status_ids" }, new[] { "target_side", "source_skill_id", "min_stacks" }, EnumParamValues(
                    "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
            AddSkillNodeHandlerSchema(schemas, "ConditionSkillAttribute", SkillExecutionPlanNodeKind.Action,
                new[] { "attribute" }, enumParamAllowedValues: EnumParamValues(
                    "attribute", Enum.GetNames(typeof(DamageAttribute))));
            AddSkillNodeHandlerSchema(schemas, "ConditionHealthRatioMax", SkillExecutionPlanNodeKind.Action,
                new[] { "ratio" });
            AddSkillNodeHandlerSchema(schemas, "ConditionHitCountMin", SkillExecutionPlanNodeKind.Action,
                new[] { "min_targets" });
            AddSkillNodeHandlerSchema(schemas, "EffectLifetime", SkillExecutionPlanNodeKind.Action,
                new[] { "duration_seconds" });
            AddSkillNodeHandlerSchema(schemas, "DelayedDamage", SkillExecutionPlanNodeKind.Action,
                new[] { "delay_seconds" });
            AddSkillNodeHandlerSchema(schemas, "RequiredTargetStatus", SkillExecutionPlanNodeKind.CastCondition,
                new[] { "status_id" }, new[] { "min_stacks" });
            AddSkillNodeHandlerSchema(schemas, "TargetStatusStackDamage", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "status_id" }, new[] { "max_stacks", "base_damage", "attack_power_coefficient", "spell_power_coefficient", "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "StatusFilteredDeployment", SkillExecutionPlanNodeKind.Action,
                new[] { "status_id", "min_stacks" });
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
            AddSkillNodeHandlerSchema(schemas, "BranchDamage", SkillExecutionPlanNodeKind.Action,
                new string[0], new[] { "chance_bonus", "count", "damage_multiplier", "search_radius" });
            AddSkillNodeHandlerSchema(schemas, "SpawnProjectile", SkillExecutionPlanNodeKind.Action,
                new string[0], new[] { "projectile_prefab_path", "projectile_sprite_path", "count", "speed" });
            AddSkillNodeHandlerSchema(schemas, "BossDamageMultiplier", SkillExecutionPlanNodeKind.DamageModifier,
                new[] { "multiplier" });
            AddSkillNodeHandlerSchema(schemas, "CooldownResetOnKill", SkillExecutionPlanNodeKind.OnKillAction,
                new string[0], new[] { "requires_execute" });
            return schemas;
        }

        /*
         * 효과 노드가 허용하는 열거형 값을 만든다.
         */
        internal static Dictionary<string, string[]> EffectBaseEnumParamValues()
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

        /*
         * 항목을 대상 목록에 추가한다.
         */
        internal static void AddSkillNodeHandlerSchema(
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

        /*
         * 열거형 매개변수의 허용값 목록을 만든다.
         */
        internal static Dictionary<string, string[]> EnumParamValues(params object[] values)
        {
            var result = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i + 1 < values.Length; i += 2)
            {
                result.Add((string)values[i], (string[])values[i + 1]);
            }

            return result;
        }

        /*
         * 열거형 매개변수의 허용값 목록을 만든다.
         */
        internal static Dictionary<string, string[]> EnumParamValues(string paramKey, params string[] allowedValues)
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { paramKey, allowedValues }
            };
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateNormalizedSkillAuthoringRows(
            SourceModel model,
            CsvRuntimeCatalog assetCatalog,
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeOwner(SkillNodeRow node, SourceModel model, List<string> errors)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeGateReferences(SkillNodeRow node, SourceModel model, List<string> errors)
        {
            ValidateChoiceGate(node.Id, "requires_active_choice_id", node.RequiresActiveChoiceId, model, errors);
            ValidateChoiceGate(node.Id, "excludes_active_choice_id", node.ExcludesActiveChoiceId, model, errors);
            ValidatePassiveGate(node.Id, "requires_passive_skill_id", node.RequiresPassiveSkillId, model, errors);
            ValidatePassiveGate(node.Id, "excludes_passive_skill_id", node.ExcludesPassiveSkillId, model, errors);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateChoiceGate(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidatePassiveGate(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeParams(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeLegacyOverlap(SkillNodeRow node, SourceModel model, List<string> errors)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeLegacySkillOverlap(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeLegacyChoiceOverlap(
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

            if (string.Equals(node.HandlerId, "CritChanceBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.CritChanceBonus))
            {
                AddLegacyOverlapError(node, "crit_chance_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "CritDamageBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.CritDamageBonus))
            {
                AddLegacyOverlapError(node, "crit_damage_bonus", errors);
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

            if (string.Equals(node.HandlerId, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.BeamWidthBonus))
            {
                AddLegacyOverlapError(node, "beam_width_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasKnockbackDistanceMultiplier
                && !NearlyEqual(choice.KnockbackDistanceMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "knockback_distance_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.ReloadReduceTargetSkillId)
                    || !NearlyZero(choice.ReloadReduceSecondsPerHit)))
            {
                AddLegacyOverlapError(node, "reload_reduce_*", errors);
            }

            if (string.Equals(node.HandlerId, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.CoreHitboxName)
                    || choice.HasCoreDamageMultiplier))
            {
                AddLegacyOverlapError(node, "core_hitbox_name/core_damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.CoreHitboxName)
                    || choice.HasCoreOnHitAdditionalDamage))
            {
                AddLegacyOverlapError(node, "core_on_hit_additional_damage_*", errors);
            }

            if (string.Equals(node.HandlerId, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.HitCountCooldownRefundTargetSkillId)
                    || choice.HitCountCooldownRefundMinTargets > 0
                    || !NearlyZero(choice.HitCountCooldownRefundRatio)))
            {
                AddLegacyOverlapError(node, "hit_count_cooldown_refund_*", errors);
            }

            if (string.Equals(node.HandlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase)
                && !NearlyZero(choice.DurationBonus))
            {
                AddLegacyOverlapError(node, "duration_bonus", errors);
            }

            if (string.Equals(node.HandlerId, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasDamageDelayMultiplier
                && !NearlyEqual(choice.DamageDelayMultiplier, 1f))
            {
                AddLegacyOverlapError(node, "damage_delay_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase)
                && (!NearlyZero(choice.ConsecutiveHitBonusRate) || !NearlyZero(choice.ConsecutiveHitMax)))
            {
                AddLegacyOverlapError(node, "consecutive_hit_bonus_rate/consecutive_hit_max", errors);
            }

            if (string.Equals(node.HandlerId, "BurstDamageRule", StringComparison.OrdinalIgnoreCase)
                && (choice.HasBurstDamageProjectileIndex || choice.HasBurstDamageMultiplier))
            {
                AddLegacyOverlapError(node, "burst_damage_projectile_index/burst_damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase)
                && (choice.FollowUpProjectileCount > 0
                    || !NearlyZero(choice.FollowUpProjectileDelaySeconds)
                    || !NearlyEqual(choice.FollowUpProjectileDamageMultiplier, 1f)))
            {
                AddLegacyOverlapError(node, "follow_up_projectile_*", errors);
            }

            if (string.Equals(node.HandlerId, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(choice.ThresholdStatusId)
                    || choice.ThresholdStatusMinStacks > 0
                    || !string.IsNullOrWhiteSpace(choice.ThresholdApplyStatusId)))
            {
                AddLegacyOverlapError(node, "threshold_status_*/threshold_apply_status_id", errors);
            }

            if (string.Equals(node.HandlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase)
                && choice.HasTargetStatusStackDamageMultiplier)
            {
                AddLegacyOverlapError(node, "target_status_stack_damage_multiplier", errors);
            }

            if (string.Equals(node.HandlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase)
                && choice.HasConsumeTargetStatusRatioOverride)
            {
                AddLegacyOverlapError(node, "consume_target_status_ratio_override", errors);
            }

            if (string.Equals(node.HandlerId, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase)
                && (choice.HasBurstStatusProjectileIndex || choice.BurstStatusStacksBonus != 0))
            {
                AddLegacyOverlapError(node, "burst_status_projectile_index/burst_status_stacks_bonus", errors);
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

        /*
         * 항목을 대상 목록에 추가한다.
         */
        internal static void AddLegacyOverlapError(SkillNodeRow node, string legacyColumn, List<string> errors)
        {
            errors.Add(
                $"Skill node '{node.Id}' handler '{node.HandlerId}' overlaps active legacy wide field '{legacyColumn}' on owner '{node.OwnerId}'. Disable one side before validation.");
        }

        /*
         * 두 값이 허용 오차 안에 있는지 확인한다.
         */
        internal static bool NearlyZero(float value)
        {
            return Math.Abs(value) <= 0.0001f;
        }

        /*
         * 두 값이 허용 오차 안에 있는지 확인한다.
         */
        internal static bool NearlyEqual(float left, float right)
        {
            return Math.Abs(left - right) <= 0.0001f;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeParamValue(
            SkillNodeParamRow param,
            SourceModel model,
            CsvRuntimeCatalog assetCatalog,
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
                        || (!assetCatalog.HasSprite(value)
                            && !assetCatalog.HasPrefab(value)
                            && !assetCatalog.HasAnimatorController(value)))
                    {
                        errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' references unknown asset path '{param.Value}'.");
                    }
                    return;
                case SkillNodeValueType.SkillId:
                    if (string.IsNullOrWhiteSpace(value)
                        || (!model.Skills.ContainsKey(value)
                            && !(IsEffectSourceSkillNodeParam(param) && HasSkillEffectSource(model, value))))
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool IsEffectSourceSkillNodeParam(SkillNodeParamRow param)
        {
            return param != null
                && string.Equals(param.ParamKey, "source_skill_id", StringComparison.OrdinalIgnoreCase);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeEnumParam(SkillNodeParamRow param, string value, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' requires a non-empty enum value.");
            }
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeSchemaEnumParam(
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

        /*
         * 정규화된 그래프 행을 실행용 스킬 노드와 매개변수로 변환한다.
         */
        internal static void MaterializeSkillGraphRows(SourceModel model)
        {
            if (model == null || model.SkillGraphNodes.Count == 0)
            {
                return;
            }

            var errors = new List<string>();
            var migratedMonsterIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                migratedMonsterIds.Add(model.SkillGraphNodes[i].MonsterId);
            }

            foreach (var legacyNode in model.SkillNodes.Values)
            {
                if (legacyNode != null
                    && !string.IsNullOrWhiteSpace(legacyNode.MonsterId)
                    && migratedMonsterIds.Contains(legacyNode.MonsterId))
                {
                    errors.Add(
                        $"Monster '{legacyNode.MonsterId}' has both skill_graph_nodes rows and legacy node '{legacyNode.Id}'. Remove one authoring path.");
                }
            }

            var paramsByType = BuildSkillNodeTypeParamLookup(model, errors);
            ValidateSkillNodeTypeDefinitions(model, paramsByType, errors);

            var generatedNodes = new List<SkillNodeRow>(model.SkillGraphNodes.Count);
            var generatedParams = new List<SkillNodeParamRow>();
            var generatedNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var graphNodeKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var effectOperationCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                var graphKey = BuildSkillGraphKey(graph);
                var graphNodeKey = $"{graphKey}:{graph.NodeOrder}";
                if (!graphNodeKeys.Add(graphNodeKey))
                {
                    errors.Add($"Skill graph '{graphKey}' has duplicate node_order '{graph.NodeOrder}'.");
                    continue;
                }

                if (graph.GraphIndex < 0)
                {
                    errors.Add($"Skill graph '{graphKey}' requires graph_index >= 0.");
                }

                if (!model.Monsters.ContainsKey(graph.MonsterId))
                {
                    errors.Add($"Skill graph '{graphKey}' references unknown monster '{graph.MonsterId}'.");
                }

                if (!model.SkillNodeTypes.TryGetValue(graph.NodeTypeId, out var nodeType))
                {
                    errors.Add($"Skill graph node '{graphNodeKey}' references unknown node_type_id '{graph.NodeTypeId}'.");
                    continue;
                }

                if (!SkillNodeHandlerSchemas.TryGetValue(nodeType.HandlerId, out var handlerSchema))
                {
                    errors.Add($"Skill graph node '{graphNodeKey}' uses unregistered handler_id '{nodeType.HandlerId}'.");
                    continue;
                }

                var targetSkillId = ResolveSkillGraphTargetSkillId(model, graph, errors);
                if (!string.IsNullOrWhiteSpace(targetSkillId)
                    && model.Skills.TryGetValue(targetSkillId, out var targetSkill)
                    && !string.Equals(targetSkill.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Skill graph '{graphKey}' target skill '{targetSkillId}' belongs to '{targetSkill.MonsterId}', not '{graph.MonsterId}'.");
                }

                if (graph.GraphKind == SkillGraphKind.Plan && IsEffectGraphOnlyHandler(nodeType.HandlerId))
                {
                    errors.Add(
                        $"Skill graph '{graphKey}' is Plan but node '{graph.NodeOrder}' uses Effect-only handler '{nodeType.HandlerId}'.");
                }

                if (graph.GraphKind == SkillGraphKind.Effect && IsEffectOperationHandler(nodeType.HandlerId))
                {
                    effectOperationCounts.TryGetValue(graphKey, out var operationCount);
                    effectOperationCounts[graphKey] = operationCount + 1;
                }

                var nodeId = BuildGeneratedSkillGraphNodeId(graph);
                if (!generatedNodeIds.Add(nodeId) || model.SkillNodes.ContainsKey(nodeId))
                {
                    errors.Add($"Skill graph generated duplicate node id '{nodeId}'.");
                    continue;
                }

                var ownerKind = graph.GraphKind == SkillGraphKind.Effect
                    ? SkillNodeOwnerKind.Effect
                    : graph.OwnerKind;
                var ownerId = graph.GraphKind == SkillGraphKind.Effect
                    ? BuildGeneratedSkillGraphEffectId(graph.OwnerKind, graph.OwnerId, graph.GraphIndex)
                    : graph.OwnerId;
                var requiresChoiceId = graph.GraphKind == SkillGraphKind.Effect
                    && graph.OwnerKind == SkillNodeOwnerKind.Choice
                        ? graph.OwnerId
                        : string.Empty;
                var requiresPassiveSkillId = graph.GraphKind == SkillGraphKind.Effect
                    ? ResolveGeneratedEffectPassiveSkillId(model, graph)
                    : string.Empty;

                generatedNodes.Add(new SkillNodeRow
                {
                    Id = nodeId,
                    MonsterId = graph.MonsterId,
                    OwnerKind = ownerKind,
                    OwnerId = ownerId,
                    TargetSkillId = targetSkillId,
                    NodeKind = nodeType.NodeKind,
                    HandlerId = nodeType.HandlerId,
                    SortOrder = graph.NodeOrder,
                    EnabledByDefault = true,
                    RequiresActiveChoiceId = requiresChoiceId,
                    ExcludesActiveChoiceId = graph.ExcludesActiveChoiceId,
                    RequiresPassiveSkillId = requiresPassiveSkillId,
                    ExcludesPassiveSkillId = string.Empty,
                    RuntimeSupportState = nodeType.RuntimeSupportState,
                    RuntimeSupportNotes = nodeType.RuntimeSupportNotes
                });

                paramsByType.TryGetValue(graph.NodeTypeId, out var typeParams);
                typeParams = typeParams ?? new List<SkillNodeTypeParamRow>();
                var definedOrders = new HashSet<int>();
                for (var paramIndex = 0; paramIndex < typeParams.Count; paramIndex++)
                {
                    var typeParam = typeParams[paramIndex];
                    definedOrders.Add(typeParam.ParamOrder);
                    var value = graph.Args[typeParam.ParamOrder - 1];
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        if (typeParam.Required)
                        {
                            errors.Add(
                                $"Skill graph node '{graphNodeKey}' requires arg_{typeParam.ParamOrder} for param '{typeParam.ParamKey}'.");
                        }
                        continue;
                    }

                    ValidateSkillGraphAllowedValue(graphNodeKey, typeParam, value, errors);
                    generatedParams.Add(new SkillNodeParamRow
                    {
                        NodeId = nodeId,
                        MonsterId = graph.MonsterId,
                        ParamKey = typeParam.ParamKey,
                        ValueType = typeParam.ValueType,
                        Value = value
                    });
                }

                for (var argIndex = 0; argIndex < graph.Args.Length; argIndex++)
                {
                    if (!string.IsNullOrWhiteSpace(graph.Args[argIndex]) && !definedOrders.Contains(argIndex + 1))
                    {
                        errors.Add(
                            $"Skill graph node '{graphNodeKey}' sets arg_{argIndex + 1}, but node type '{graph.NodeTypeId}' has no matching param definition.");
                    }
                }
            }

            var effectGraphKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                if (graph.GraphKind == SkillGraphKind.Effect)
                {
                    effectGraphKeys.Add(BuildSkillGraphKey(graph));
                }
            }

            foreach (var graphKey in effectGraphKeys)
            {
                effectOperationCounts.TryGetValue(graphKey, out var operationCount);
                if (operationCount != 1)
                {
                    errors.Add($"Effect graph '{graphKey}' requires exactly one operation handler but has {operationCount}.");
                }
            }

            if (errors.Count > 0)
            {
                throw new CsvFatalException("Skill graph authoring materialization failed.", errors);
            }

            for (var i = 0; i < generatedNodes.Count; i++)
            {
                model.SkillNodes.Add(generatedNodes[i].Id, generatedNodes[i]);
            }
            model.SkillNodeParams.AddRange(generatedParams);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static Dictionary<string, List<SkillNodeTypeParamRow>> BuildSkillNodeTypeParamLookup(
            SourceModel model,
            List<string> errors)
        {
            var result = new Dictionary<string, List<SkillNodeTypeParamRow>>(StringComparer.OrdinalIgnoreCase);
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.SkillNodeTypeParams.Count; i++)
            {
                var param = model.SkillNodeTypeParams[i];
                if (!model.SkillNodeTypes.ContainsKey(param.NodeTypeId))
                {
                    errors.Add(
                        $"Skill node type param '{param.NodeTypeId}.{param.ParamKey}' references unknown node_type_id.");
                    continue;
                }

                if (param.ParamOrder < 1 || param.ParamOrder > 12)
                {
                    errors.Add(
                        $"Skill node type param '{param.NodeTypeId}.{param.ParamKey}' requires param_order between 1 and 12.");
                    continue;
                }

                var orderKey = $"{param.NodeTypeId}:{param.ParamOrder}";
                var nameKey = $"{param.NodeTypeId}:{param.ParamKey}";
                if (!keys.Add(orderKey) || !keys.Add(nameKey))
                {
                    errors.Add(
                        $"Skill node type '{param.NodeTypeId}' has duplicate param order or key for '{param.ParamKey}'.");
                    continue;
                }

                if (!result.TryGetValue(param.NodeTypeId, out var list))
                {
                    list = new List<SkillNodeTypeParamRow>();
                    result.Add(param.NodeTypeId, list);
                }
                list.Add(param);
            }

            foreach (var entry in result)
            {
                entry.Value.Sort((left, right) => left.ParamOrder.CompareTo(right.ParamOrder));
            }
            return result;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillNodeTypeDefinitions(
            SourceModel model,
            Dictionary<string, List<SkillNodeTypeParamRow>> paramsByType,
            List<string> errors)
        {
            foreach (var nodeType in model.SkillNodeTypes.Values)
            {
                if (!SkillNodeHandlerSchemas.TryGetValue(nodeType.HandlerId, out var schema))
                {
                    errors.Add($"Skill node type '{nodeType.Id}' uses unregistered handler_id '{nodeType.HandlerId}'.");
                    continue;
                }

                if (nodeType.NodeKind != schema.NodeKind)
                {
                    errors.Add(
                        $"Skill node type '{nodeType.Id}' handler '{nodeType.HandlerId}' requires node_kind '{schema.NodeKind}', not '{nodeType.NodeKind}'.");
                }

                paramsByType.TryGetValue(nodeType.Id, out var typeParams);
                typeParams = typeParams ?? new List<SkillNodeTypeParamRow>();
                var definedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < typeParams.Count; i++)
                {
                    var param = typeParams[i];
                    definedKeys.Add(param.ParamKey);
                    if (!schema.AllowedParams.Contains(param.ParamKey))
                    {
                        errors.Add(
                            $"Skill node type '{nodeType.Id}' defines unsupported param '{param.ParamKey}' for handler '{nodeType.HandlerId}'.");
                    }

                    var expectedRequired = schema.RequiredParams.Contains(param.ParamKey);
                    if (param.Required != expectedRequired)
                    {
                        errors.Add(
                            $"Skill node type '{nodeType.Id}' param '{param.ParamKey}' required={param.Required} but handler schema requires {expectedRequired}.");
                    }
                }

                foreach (var requiredParam in schema.RequiredParams)
                {
                    if (!definedKeys.Contains(requiredParam))
                    {
                        errors.Add(
                            $"Skill node type '{nodeType.Id}' is missing required handler param definition '{requiredParam}'.");
                    }
                }
            }
        }

        /*
         * 현재 조건에 맞는 값을 결정한다.
         */
        internal static string ResolveSkillGraphTargetSkillId(
            SourceModel model,
            SkillGraphNodeRow graph,
            List<string> errors)
        {
            string targetSkillId;
            switch (graph.OwnerKind)
            {
                case SkillNodeOwnerKind.Choice:
                    if (!model.SkillChoices.TryGetValue(graph.OwnerId, out var choice))
                    {
                        errors.Add($"Skill graph '{BuildSkillGraphKey(graph)}' references unknown choice owner '{graph.OwnerId}'.");
                        return graph.TargetSkillId;
                    }
                    if (!string.Equals(choice.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Skill graph choice owner '{graph.OwnerId}' belongs to '{choice.MonsterId}', not '{graph.MonsterId}'.");
                    }
                    targetSkillId = string.IsNullOrWhiteSpace(choice.TargetSkillId) ? choice.SkillId : choice.TargetSkillId;
                    break;
                case SkillNodeOwnerKind.Skill:
                    if (!model.Skills.TryGetValue(graph.OwnerId, out var ownerSkill))
                    {
                        errors.Add($"Skill graph '{BuildSkillGraphKey(graph)}' references unknown skill owner '{graph.OwnerId}'.");
                        return graph.TargetSkillId;
                    }
                    if (!string.Equals(ownerSkill.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Skill graph skill owner '{graph.OwnerId}' belongs to '{ownerSkill.MonsterId}', not '{graph.MonsterId}'.");
                    }
                    targetSkillId = graph.OwnerId;
                    break;
                case SkillNodeOwnerKind.Trigger:
                    if (!model.SkillTriggers.TryGetValue(graph.OwnerId, out var trigger))
                    {
                        errors.Add($"Skill graph '{BuildSkillGraphKey(graph)}' references unknown trigger owner '{graph.OwnerId}'.");
                        return graph.TargetSkillId;
                    }
                    if (!string.Equals(trigger.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Skill graph trigger owner '{graph.OwnerId}' belongs to '{trigger.MonsterId}', not '{graph.MonsterId}'.");
                    }
                    targetSkillId = trigger.SourceSkillId;
                    break;
                default:
                    errors.Add(
                        $"Skill graph '{BuildSkillGraphKey(graph)}' uses unsupported owner_kind '{graph.OwnerKind}'.");
                    return graph.TargetSkillId;
            }

            if (!string.IsNullOrWhiteSpace(graph.TargetSkillId))
            {
                targetSkillId = graph.TargetSkillId;
            }

            if (string.IsNullOrWhiteSpace(targetSkillId) || !model.Skills.ContainsKey(targetSkillId))
            {
                errors.Add(
                    $"Skill graph '{BuildSkillGraphKey(graph)}' resolves unknown target_skill_id '{targetSkillId}'.");
            }
            return targetSkillId;
        }

        /*
         * 현재 조건에 맞는 값을 결정한다.
         */
        internal static string ResolveGeneratedEffectPassiveSkillId(SourceModel model, SkillGraphNodeRow graph)
        {
            if (model == null || graph == null || graph.GraphKind != SkillGraphKind.Effect)
            {
                return string.Empty;
            }

            if (graph.OwnerKind == SkillNodeOwnerKind.Skill
                && model.Skills.TryGetValue(graph.OwnerId, out var skill)
                && skill.SkillKind == PakuriCsvSkillKind.Passive)
            {
                return skill.Id;
            }

            if (graph.OwnerKind == SkillNodeOwnerKind.Choice
                && model.SkillChoices.TryGetValue(graph.OwnerId, out var choice)
                && model.Skills.TryGetValue(choice.SkillId, out var choiceSkill)
                && choiceSkill.SkillKind == PakuriCsvSkillKind.Passive)
            {
                return choiceSkill.Id;
            }

            return string.Empty;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillGraphAllowedValue(
            string graphNodeKey,
            SkillNodeTypeParamRow param,
            string value,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(param.AllowedValues))
            {
                return;
            }

            var allowed = param.AllowedValues.Split('|');
            for (var i = 0; i < allowed.Length; i++)
            {
                if (string.Equals(allowed[i].Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            errors.Add(
                $"Skill graph node '{graphNodeKey}' param '{param.ParamKey}' has invalid value '{value}'. Allowed: {param.AllowedValues}.");
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool IsEffectGraphOnlyHandler(string handlerId)
        {
            return IsEffectOperationHandler(handlerId)
                || string.Equals(handlerId, "EffectTarget", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "StatusCriticalResistanceBonus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "ConditionStatus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "EffectLifetime", StringComparison.OrdinalIgnoreCase);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static string BuildSkillGraphKey(SkillGraphNodeRow graph)
        {
            return $"{graph.MonsterId}:{graph.OwnerKind}:{graph.OwnerId}:{graph.GraphKind}:{graph.GraphIndex}";
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static string BuildGeneratedSkillGraphNodeId(SkillGraphNodeRow graph)
        {
            return $"{graph.OwnerKind}:{graph.OwnerId}:{graph.GraphKind}:{graph.GraphIndex}:{graph.NodeOrder}";
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static string BuildGeneratedSkillGraphEffectId(
            SkillNodeOwnerKind ownerKind,
            string ownerId,
            int graphIndex)
        {
            if (ownerKind == SkillNodeOwnerKind.Choice || ownerKind == SkillNodeOwnerKind.Trigger)
            {
                return graphIndex == 0 ? ownerId : $"{ownerId}@effect{graphIndex + 1}";
            }

            return $"{ownerId}@effect{graphIndex + 1}";
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool HasSkillGraphReference(SkillTriggerRow trigger)
        {
            return trigger != null
                && !string.IsNullOrWhiteSpace(trigger.TriggeredGraphOwnerId);
        }

        /*
         * 현재 조건에 맞는 값을 결정한다.
         */
        internal static string ResolveTriggeredEffectId(SkillTriggerRow trigger)
        {
            if (!HasSkillGraphReference(trigger))
            {
                return trigger != null ? trigger.TriggeredEffectId : string.Empty;
            }

            return BuildGeneratedSkillGraphEffectId(
                trigger.TriggeredGraphOwnerKind,
                trigger.TriggeredGraphOwnerId,
                trigger.TriggeredGraphIndex);
        }
    }
}
