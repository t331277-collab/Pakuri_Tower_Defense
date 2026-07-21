using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;

/*
 * 정규화된 스킬 노드와 그래프 CSV를 공통 실행 데이터로 만든다.
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

		internal sealed class SkillNodeRow
		{
			public string Id;

			public string MonsterId;

			public SkillNodeOwnerKind OwnerKind;

			public string OwnerId;

			public string TargetSkillId;

			public SkillNodeKind NodeKind;

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

		internal sealed class SkillNodeParamRow
		{
			public string NodeId;

			public string MonsterId;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public string Value;
		}

		internal sealed class SkillNodeTypeRow
		{
			public string Id;

			public string HandlerId;

			public SkillNodeKind NodeKind;

			public string RuntimeSupportState;

			public string RuntimeSupportNotes;
		}

		internal sealed class SkillNodeTypeParamRow
		{
			public string NodeTypeId;

			public int ParamOrder;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public bool Required;

			public string AllowedValues;
		}

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

		internal sealed class SkillNodeHandlerSchema
		{
			public string HandlerId { get; }

			public SkillNodeKind NodeKind { get; }

			public HashSet<string> RequiredParams { get; }

			public HashSet<string> AllowedParams { get; }

			public Dictionary<string, HashSet<string>> EnumParamAllowedValues { get; }

			public SkillNodeHandlerSchema(string handlerId, SkillNodeKind nodeKind, string[] requiredParams, string[] optionalParams = null, Dictionary<string, string[]> enumParamAllowedValues = null)
			{
				HandlerId = handlerId;
				NodeKind = nodeKind;
				RequiredParams = new HashSet<string>(requiredParams ?? new string[0], StringComparer.OrdinalIgnoreCase);
				AllowedParams = new HashSet<string>(RequiredParams, StringComparer.OrdinalIgnoreCase);
				EnumParamAllowedValues = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
				if (optionalParams != null)
				{
					for (int i = 0; i < optionalParams.Length; i++)
					{
						AllowedParams.Add(optionalParams[i]);
					}
				}
				if (enumParamAllowedValues == null)
				{
					return;
				}
				foreach (KeyValuePair<string, string[]> enumParamAllowedValue in enumParamAllowedValues)
				{
					EnumParamAllowedValues[enumParamAllowedValue.Key] = new HashSet<string>(enumParamAllowedValue.Value ?? new string[0], StringComparer.OrdinalIgnoreCase);
				}
			}
		}

		internal static readonly Dictionary<string, SkillNodeHandlerSchema> SkillNodeHandlerSchemas = BuildSkillNodeHandlerSchemas();

		internal static SkillNodeTypeRow ParseSkillNodeTypeRow(CsvParser.CsvRecord record)
		{
			return new SkillNodeTypeRow
			{
				Id = record.ReadRequiredString("node_type_id"),
				HandlerId = record.ReadRequiredString("handler_id"),
				NodeKind = record.ReadEnum<SkillNodeKind>("node_kind"),
				RuntimeSupportState = CsvRowParser.ReadOptionalStringIfColumnExists(record, "runtime_support_state"),
				RuntimeSupportNotes = CsvRowParser.ReadOptionalStringIfColumnExists(record, "runtime_support_notes")
			};
		}

		internal static SkillNodeTypeParamRow ParseSkillNodeTypeParamRow(CsvParser.CsvRecord record)
		{
			return new SkillNodeTypeParamRow
			{
				NodeTypeId = record.ReadRequiredString("node_type_id"),
				ParamOrder = record.ReadInt("param_order"),
				ParamKey = record.ReadRequiredString("param_key"),
				ValueType = ParseSkillNodeValueType(record.ReadRequiredString("value_type"), record),
				Required = record.ReadBool("required"),
				AllowedValues = CsvRowParser.ReadOptionalStringIfColumnExists(record, "allowed_values")
			};
		}

		internal static SkillGraphNodeRow ParseSkillGraphNodeRow(CsvParser.CsvRecord record)
		{
			SkillGraphNodeRow skillGraphNodeRow = new SkillGraphNodeRow
			{
				MonsterId = record.ReadRequiredString("monster_id"),
				OwnerKind = record.ReadEnum<SkillNodeOwnerKind>("owner_kind"),
				OwnerId = record.ReadRequiredString("owner_id"),
				GraphKind = record.ReadEnum<SkillGraphKind>("graph_kind"),
				GraphIndex = record.ReadInt("graph_index"),
				TargetSkillId = CsvRowParser.ReadOptionalStringIfColumnExists(record, "target_skill_id"),
				NodeOrder = record.ReadInt("node_order"),
				NodeTypeId = record.ReadRequiredString("node_type_id"),
				ExcludesActiveChoiceId = CsvRowParser.ReadOptionalStringIfColumnExists(record, "excludes_active_choice_id")
			};
			for (int i = 0; i < skillGraphNodeRow.Args.Length; i++)
			{
				skillGraphNodeRow.Args[i] = CsvRowParser.ReadOptionalStringIfColumnExists(record, $"arg_{i + 1}");
			}
			return skillGraphNodeRow;
		}

		internal static SkillNodeValueType ParseSkillNodeValueType(string rawValue, CsvParser.CsvRecord record)
		{
			return rawValue.Trim().Replace("-", "_").ToLowerInvariant() switch
			{
				"string" => SkillNodeValueType.String,
				"int" => SkillNodeValueType.Int,
				"float" => SkillNodeValueType.Float,
				"bool" => SkillNodeValueType.Bool,
				"enum" => SkillNodeValueType.Enum,
				"asset_path" => SkillNodeValueType.AssetPath,
				"skill_id" => SkillNodeValueType.SkillId,
				"status_id" => SkillNodeValueType.StatusId,
				"choice_id" => SkillNodeValueType.ChoiceId,
				_ => throw new CsvParser.CsvFatalException($"CSV row {record.RowNumber} in '{record.TableName}' has unsupported value_type '{rawValue}'."),
			};
		}

		internal static Dictionary<string, SkillNodeHandlerSchema> BuildSkillNodeHandlerSchemas()
		{
			Dictionary<string, SkillNodeHandlerSchema> dictionary = new Dictionary<string, SkillNodeHandlerSchema>(StringComparer.OrdinalIgnoreCase);
			AddSkillNodeHandlerSchema(dictionary, "TargetHealthRatioCondition", SkillNodeKind.CastCondition, new string[1] { "threshold" }, new string[1] { "reject_if_missing_target" });
			AddSkillNodeHandlerSchema(dictionary, "ExecuteDamageMultiplier", SkillNodeKind.DamageModifier, new string[1] { "multiplier" }, new string[1] { "threshold_source" });
			AddSkillNodeHandlerSchema(dictionary, "TargetPredicateDamageMultiplier", SkillNodeKind.DamageModifier, new string[2] { "predicate", "multiplier" }, null, EnumParamValues("predicate", "is_boss"));
			AddSkillNodeHandlerSchema(dictionary, "CooldownRefund", SkillNodeKind.OnKillAction, new string[1] { "ratio" });
			AddSkillNodeHandlerSchema(dictionary, "DamageMultiplier", SkillNodeKind.DamageModifier, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "ShieldAmountMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "CountStatusDamageMultiplier", SkillNodeKind.DamageModifier, new string[3] { "status_id", "target_side", "amount_per_count" }, new string[1] { "max_count" }, EnumParamValues("target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
			AddSkillNodeHandlerSchema(dictionary, "CooldownMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "CritChanceBonus", SkillNodeKind.CritModifier, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "CritDamageBonus", SkillNodeKind.CritModifier, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "MagazineBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "ReloadTimeMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "PierceBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "HitTargetCountBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "RadiusMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "RadiusBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "BeamWidthBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "KnockbackDistanceMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "ReloadReducePerHit", SkillNodeKind.OnHitAction, new string[2] { "target_skill_id", "seconds_per_hit" });
			AddSkillNodeHandlerSchema(dictionary, "CoreDamageMultiplier", SkillNodeKind.DamageModifier, new string[2] { "hitbox_name", "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "CoreAdditionalDamage", SkillNodeKind.OnHitAction, new string[4] { "hitbox_name", "chance", "multiplier", "attribute" }, null, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "HitCountCooldownRefund", SkillNodeKind.OnHitAction, new string[3] { "target_skill_id", "min_targets", "ratio" });
			AddSkillNodeHandlerSchema(dictionary, "DurationBonus", SkillNodeKind.Action, new string[1] { "bonus_seconds" });
			AddSkillNodeHandlerSchema(dictionary, "DurationMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "DamageDelayMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "AdditionalProjectileBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "ShotIntervalMultiplier", SkillNodeKind.Action, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "ConsecutiveHitDamageBonus", SkillNodeKind.Action, new string[2] { "bonus_rate", "max_bonus" });
			AddSkillNodeHandlerSchema(dictionary, "BurstDamageRule", SkillNodeKind.DamageModifier, new string[2] { "projectile_index", "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "FollowUpProjectile", SkillNodeKind.Action, new string[3] { "count", "delay_seconds", "damage_multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "ThresholdApplyStatus", SkillNodeKind.Action, new string[3] { "source_status_id", "min_stacks", "apply_status_id" });
			AddSkillNodeHandlerSchema(dictionary, "TargetStatusStackDamageMultiplier", SkillNodeKind.DamageModifier, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "ConsumeTargetStatusRatioOverride", SkillNodeKind.Action, new string[1] { "ratio" });
			AddSkillNodeHandlerSchema(dictionary, "BurstStatusStacksBonus", SkillNodeKind.Action, new string[2] { "projectile_index", "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusStackAmountBonus", SkillNodeKind.Action, new string[2] { "status_id", "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusStackAmountSet", SkillNodeKind.Action, new string[2] { "status_id", "value" });
			AddSkillNodeHandlerSchema(dictionary, "StatusMaxStacksBonus", SkillNodeKind.Action, new string[2] { "status_id", "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "ConditionalDamageMultiplier", SkillNodeKind.DamageModifier, new string[3] { "status_id", "min_stacks", "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "TargetStatusStackDamageRateBonus", SkillNodeKind.DamageModifier, new string[2] { "status_id", "bonus_rate_per_stack" });
			AddSkillNodeHandlerSchema(dictionary, "TriggerProcChanceBonus", SkillNodeKind.Action, new string[2] { "trigger_id", "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusActionSpeedBonus", SkillNodeKind.Action, new string[1] { "bonus" }, new string[1] { "status_id" });
			AddSkillNodeHandlerSchema(dictionary, "StatusAttackPowerBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusMoveSpeedBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusAilmentResistanceBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusDamageBonusRate", SkillNodeKind.Action, new string[1] { "bonus" }, new string[1] { "attribute" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "StatusShieldReceivedBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusCriticalChanceBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusDamageTakenBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusFlatElementResistReduction", SkillNodeKind.Action, new string[1] { "bonus" }, new string[1] { "attribute" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "StatusDurationBonus", SkillNodeKind.Action, new string[2] { "status_id", "bonus_seconds" });
			AddSkillNodeHandlerSchema(dictionary, "StatusConditionalDamageTakenBonus", SkillNodeKind.Action, new string[2] { "source_status_id", "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusElementDamageTakenBonus", SkillNodeKind.Action, new string[1] { "bonus" }, new string[1] { "attribute" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "StatusConditionalStatusChanceBonus", SkillNodeKind.Action, new string[2] { "status_ids", "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusCriticalDamageTakenBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusCriticalDamageBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "StatusElementResistReduction", SkillNodeKind.Action, new string[1] { "bonus" }, new string[1] { "attribute" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "StatusOutgoingAdditionalDamage", SkillNodeKind.Action, new string[3] { "multiplier", "trigger_attribute", "damage_attribute" }, null, EnumParamValues("trigger_attribute", Enum.GetNames(typeof(DamageAttribute)), "damage_attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "StatusSpellPowerBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "ApplyStatus", SkillNodeKind.Action, new string[1] { "status_id" }, new string[8] { "status_chance", "status_label", "status_effect_prefab_path", "status_max_stacks", "status_stack_amount", "status_target_scope", "status_merge_policy", "shield_amount_refresh_policy" });
			AddSkillNodeHandlerSchema(dictionary, "ApplyShield", SkillNodeKind.Action, Array.Empty<string>(), new string[12]
			{
				"base_damage", "attack_power_coefficient", "spell_power_coefficient", "damage_multiplier", "status_chance", "status_label", "status_effect_prefab_path", "status_max_stacks", "status_stack_amount", "status_target_scope",
				"status_merge_policy", "shield_amount_refresh_policy"
			});
			AddSkillNodeHandlerSchema(dictionary, "StatusModifier", SkillNodeKind.Action, Array.Empty<string>(), new string[7] { "status_chance", "status_label", "status_effect_prefab_path", "status_max_stacks", "status_stack_amount", "status_target_scope", "status_merge_policy" });
			AddSkillNodeHandlerSchema(dictionary, "EffectStatus", SkillNodeKind.Action, new string[1] { "status_id" }, new string[8] { "status_chance", "status_label", "status_effect_prefab_path", "status_max_stacks", "status_stack_amount", "status_target_scope", "status_merge_policy", "shield_amount_refresh_policy" });
			AddSkillNodeHandlerSchema(dictionary, "EffectDamage", SkillNodeKind.Action, new string[1] { "attribute" }, new string[6] { "base_damage", "attack_power_coefficient", "spell_power_coefficient", "damage_multiplier", "radius", "tick_interval_seconds" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "EffectExtendStatusDuration", SkillNodeKind.Action, new string[1] { "status_id" });
			AddSkillNodeHandlerSchema(dictionary, "RecastZone", SkillNodeKind.OnExpireAction, new string[6] { "source_skill_id", "delay_seconds", "duration_seconds", "radius_multiplier", "inherit_snapshot", "max_generation" });
			AddSkillNodeHandlerSchema(dictionary, "EffectTarget", SkillNodeKind.Action, Array.Empty<string>(), new string[9] { "target_side", "target_selection", "target_shape", "center_mode", "visual_anchor_mode", "effect_timing", "delay_seconds", "apply_once", "cover_all" }, EffectBaseEnumParamValues());
			AddSkillNodeHandlerSchema(dictionary, "EffectVisual", SkillNodeKind.Action, new string[1] { "skill_effect_prefab_path" });
			AddSkillNodeHandlerSchema(dictionary, "AttachStatusPayload", SkillNodeKind.Action, new string[1] { "status_id" }, new string[6] { "status_chance", "status_label", "status_duration_seconds", "status_max_stacks", "status_stack_amount", "status_merge_policy" });
			AddSkillNodeHandlerSchema(dictionary, "RequiredSourceStatus", SkillNodeKind.Action, new string[1] { "status_id" }, new string[1] { "min_stacks" });
			AddSkillNodeHandlerSchema(dictionary, "StatusRuntimeKindFilter", SkillNodeKind.Action, Array.Empty<string>(), new string[2] { "incoming_skill_runtime_kinds", "outgoing_skill_runtime_kinds" });
			AddSkillNodeHandlerSchema(dictionary, "StatusCriticalResistanceBonus", SkillNodeKind.Action, new string[1] { "bonus" });
			AddSkillNodeHandlerSchema(dictionary, "RuntimeEffectVisual", SkillNodeKind.Action, new string[1] { "runtime_visual_sprite_path" }, new string[5] { "runtime_visual_animator_controller_path", "runtime_visual_scale", "runtime_visual_sorting_order", "runtime_hitbox_size_x", "runtime_hitbox_size_y" });
			AddSkillNodeHandlerSchema(dictionary, "ConditionStatus", SkillNodeKind.Action, new string[1] { "status_id" }, new string[3] { "target_side", "source_skill_id", "min_stacks" }, EnumParamValues("target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
			AddSkillNodeHandlerSchema(dictionary, "ConditionAnyStatus", SkillNodeKind.Action, new string[1] { "status_ids" }, new string[3] { "target_side", "source_skill_id", "min_stacks" }, EnumParamValues("target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
			AddSkillNodeHandlerSchema(dictionary, "ConditionSkillAttribute", SkillNodeKind.Action, new string[1] { "attribute" }, null, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute))));
			AddSkillNodeHandlerSchema(dictionary, "ConditionHealthRatioMax", SkillNodeKind.Action, new string[1] { "ratio" });
			AddSkillNodeHandlerSchema(dictionary, "ConditionHitCountMin", SkillNodeKind.Action, new string[1] { "min_targets" });
			AddSkillNodeHandlerSchema(dictionary, "EffectLifetime", SkillNodeKind.Action, new string[1] { "duration_seconds" });
			AddSkillNodeHandlerSchema(dictionary, "DelayedDamage", SkillNodeKind.Action, new string[1] { "delay_seconds" });
			AddSkillNodeHandlerSchema(dictionary, "RequiredTargetStatus", SkillNodeKind.CastCondition, new string[1] { "status_id" }, new string[1] { "min_stacks" });
			AddSkillNodeHandlerSchema(dictionary, "TargetStatusStackDamage", SkillNodeKind.DamageModifier, new string[1] { "status_id" }, new string[5] { "max_stacks", "base_damage", "attack_power_coefficient", "spell_power_coefficient", "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "StatusFilteredDeployment", SkillNodeKind.Action, new string[2] { "status_id", "min_stacks" });
			AddSkillNodeHandlerSchema(dictionary, "ConsumeTargetStatus", SkillNodeKind.OnHitAction, new string[1] { "status_id" }, new string[2] { "ratio", "stacks" });
			AddSkillNodeHandlerSchema(dictionary, "CooldownReset", SkillNodeKind.OnKillAction, new string[0], new string[1] { "requires_execute" });
			AddSkillNodeHandlerSchema(dictionary, "AdditionalDamage", SkillNodeKind.OnHitAction, new string[1] { "multiplier" }, new string[5] { "base_damage", "chance", "attribute", "target", "target_side" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute)), "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
			AddSkillNodeHandlerSchema(dictionary, "EveryNthHitChainDamage", SkillNodeKind.OnHitAction, new string[2] { "hit_count", "multiplier" }, new string[4] { "radius", "max_targets", "attribute", "target_side" }, EnumParamValues("attribute", Enum.GetNames(typeof(DamageAttribute)), "target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide))));
			AddSkillNodeHandlerSchema(dictionary, "RepeatPerTarget", SkillNodeKind.Action, new string[3] { "repeat_count", "repeat_interval_seconds", "repeat_damage_multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "TargetStatusCritBonus", SkillNodeKind.CritModifier, new string[1] { "status_id" }, new string[3] { "crit_chance_bonus", "crit_damage_bonus", "min_stacks" });
			AddSkillNodeHandlerSchema(dictionary, "RedistributeConsumedStatus", SkillNodeKind.OnKillAction, new string[2] { "status_id", "ratio" }, new string[3] { "radius", "stacks", "target_count" });
			AddSkillNodeHandlerSchema(dictionary, "TargetHealthRatioThresholdBonus", SkillNodeKind.CastCondition, new string[1] { "threshold_bonus" });
			AddSkillNodeHandlerSchema(dictionary, "ExecuteCritChanceBonus", SkillNodeKind.CritModifier, new string[1] { "crit_chance_bonus" });
			AddSkillNodeHandlerSchema(dictionary, "CooldownRefundBonus", SkillNodeKind.OnKillAction, new string[1] { "ratio_bonus" });
			AddSkillNodeHandlerSchema(dictionary, "BranchDamage", SkillNodeKind.Action, new string[0], new string[4] { "chance_bonus", "count", "damage_multiplier", "search_radius" });
			AddSkillNodeHandlerSchema(dictionary, "SpawnProjectile", SkillNodeKind.Action, new string[0], new string[4] { "projectile_prefab_path", "projectile_sprite_path", "count", "speed" });
			AddSkillNodeHandlerSchema(dictionary, "BossDamageMultiplier", SkillNodeKind.DamageModifier, new string[1] { "multiplier" });
			AddSkillNodeHandlerSchema(dictionary, "CooldownResetOnKill", SkillNodeKind.OnKillAction, new string[0], new string[1] { "requires_execute" });
			return dictionary;
		}

		internal static Dictionary<string, string[]> EffectBaseEnumParamValues()
		{
			return EnumParamValues("target_side", Enum.GetNames(typeof(SkillMultiEffectTargetSide)), "target_selection", Enum.GetNames(typeof(SkillMultiEffectTargetSelection)), "target_shape", Enum.GetNames(typeof(SkillMultiEffectTargetShape)), "center_mode", Enum.GetNames(typeof(SkillMultiEffectCenterMode)), "visual_anchor_mode", Enum.GetNames(typeof(SkillMultiEffectVisualAnchorMode)), "effect_timing", Enum.GetNames(typeof(SkillMultiEffectTiming)), "attribute", Enum.GetNames(typeof(DamageAttribute)));
		}

		internal static void AddSkillNodeHandlerSchema(Dictionary<string, SkillNodeHandlerSchema> schemas, string handlerId, SkillNodeKind nodeKind, string[] requiredParams, string[] optionalParams = null, Dictionary<string, string[]> enumParamAllowedValues = null)
		{
			schemas.Add(handlerId, new SkillNodeHandlerSchema(handlerId, nodeKind, requiredParams, optionalParams, enumParamAllowedValues));
		}

		internal static Dictionary<string, string[]> EnumParamValues(params object[] values)
		{
			Dictionary<string, string[]> dictionary = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i + 1 < values.Length; i += 2)
			{
				dictionary.Add((string)values[i], (string[])values[i + 1]);
			}
			return dictionary;
		}

		internal static Dictionary<string, string[]> EnumParamValues(string paramKey, params string[] allowedValues)
		{
			return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase) { { paramKey, allowedValues } };
		}

		internal static void ValidateNormalizedSkillAuthoringRows(CsvSourceModel.SourceModel model, CsvRuntimeCatalog assetCatalog, List<string> errors)
		{
			if (model == null)
			{
				return;
			}
			Dictionary<string, List<SkillNodeParamRow>> dictionary = new Dictionary<string, List<SkillNodeParamRow>>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, HashSet<string>> dictionary2 = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
			foreach (SkillNodeParamRow skillNodeParam in model.SkillNodeParams)
			{
				if (!model.SkillNodes.ContainsKey(skillNodeParam.NodeId))
				{
					errors.Add("Skill node param '" + skillNodeParam.ParamKey + "' references unknown node_id '" + skillNodeParam.NodeId + "'.");
					continue;
				}
				if (!dictionary.TryGetValue(skillNodeParam.NodeId, out var value))
				{
					value = new List<SkillNodeParamRow>();
					dictionary.Add(skillNodeParam.NodeId, value);
					dictionary2.Add(skillNodeParam.NodeId, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
				}
				if (!dictionary2[skillNodeParam.NodeId].Add(skillNodeParam.ParamKey))
				{
					errors.Add("Skill node '" + skillNodeParam.NodeId + "' has duplicate param '" + skillNodeParam.ParamKey + "'.");
				}
				value.Add(skillNodeParam);
				ValidateSkillNodeParamValue(skillNodeParam, model, assetCatalog, errors);
			}
			foreach (SkillNodeRow value4 in model.SkillNodes.Values)
			{
				ValidateSkillNodeOwner(value4, model, errors);
				ValidateSkillNodeGateReferences(value4, model, errors);
				if (string.IsNullOrWhiteSpace(value4.HandlerId))
				{
					errors.Add("Skill node '" + value4.Id + "' requires handler_id.");
					continue;
				}
				if (!SkillNodeHandlerSchemas.TryGetValue(value4.HandlerId, out var value2))
				{
					errors.Add("Skill node '" + value4.Id + "' uses unregistered handler_id '" + value4.HandlerId + "'.");
					continue;
				}
				if (value4.NodeKind != value2.NodeKind)
				{
					errors.Add($"Skill node '{value4.Id}' handler '{value4.HandlerId}' requires node_kind '{value2.NodeKind}' but row uses '{value4.NodeKind}'.");
				}
				if (!dictionary.TryGetValue(value4.Id, out var value3))
				{
					value3 = new List<SkillNodeParamRow>();
				}
				ValidateSkillNodeParams(value4, value2, value3, errors);
				ValidateSkillNodeLegacyOverlap(value4, model, errors);
			}
		}

		internal static void ValidateSkillNodeOwner(SkillNodeRow node, CsvSourceModel.SourceModel model, List<string> errors)
		{
			switch (node.OwnerKind)
			{
			case SkillNodeOwnerKind.Skill:
				if (!model.Skills.ContainsKey(node.OwnerId))
				{
					errors.Add("Skill node '" + node.Id + "' references unknown owner skill '" + node.OwnerId + "'.");
				}
				break;
			case SkillNodeOwnerKind.Passive:
			{
				if (!model.Skills.TryGetValue(node.OwnerId, out var value) || value.SkillKind != PakuriCsvSkillKind.Passive)
				{
					errors.Add("Skill node '" + node.Id + "' references unknown passive owner '" + node.OwnerId + "'.");
				}
				break;
			}
			case SkillNodeOwnerKind.Choice:
				if (!model.SkillChoices.ContainsKey(node.OwnerId))
				{
					errors.Add("Skill node '" + node.Id + "' references unknown choice owner '" + node.OwnerId + "'.");
				}
				break;
			case SkillNodeOwnerKind.Effect:
				if (string.IsNullOrWhiteSpace(node.OwnerId))
				{
					errors.Add("Skill node '" + node.Id + "' requires owner_id for effect-owned nodes.");
				}
				if (string.IsNullOrWhiteSpace(node.TargetSkillId) || !model.Skills.ContainsKey(node.TargetSkillId))
				{
					errors.Add("Skill node '" + node.Id + "' effect owner '" + node.OwnerId + "' requires a known target_skill_id.");
				}
				break;
			case SkillNodeOwnerKind.Trigger:
				errors.Add("Skill node '" + node.Id + "' uses owner_kind 'Trigger', but trigger-owned normalized nodes are not wired into runtime plans yet.");
				if (!model.SkillTriggers.ContainsKey(node.OwnerId))
				{
					errors.Add("Skill node '" + node.Id + "' references unknown trigger owner '" + node.OwnerId + "'.");
				}
				break;
			default:
				errors.Add($"Skill node '{node.Id}' uses unsupported owner_kind '{node.OwnerKind}'.");
				break;
			}
			if (!string.IsNullOrWhiteSpace(node.TargetSkillId) && !model.Skills.ContainsKey(node.TargetSkillId))
			{
				errors.Add("Skill node '" + node.Id + "' references unknown target_skill_id '" + node.TargetSkillId + "'.");
			}
		}

		internal static void ValidateSkillNodeGateReferences(SkillNodeRow node, CsvSourceModel.SourceModel model, List<string> errors)
		{
			ValidateChoiceGate(node.Id, "requires_active_choice_id", node.RequiresActiveChoiceId, model, errors);
			ValidateChoiceGate(node.Id, "excludes_active_choice_id", node.ExcludesActiveChoiceId, model, errors);
			ValidatePassiveGate(node.Id, "requires_passive_skill_id", node.RequiresPassiveSkillId, model, errors);
			ValidatePassiveGate(node.Id, "excludes_passive_skill_id", node.ExcludesPassiveSkillId, model, errors);
		}

		internal static void ValidateChoiceGate(string nodeId, string columnName, string choiceId, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(choiceId) && !model.SkillChoices.ContainsKey(choiceId))
			{
				errors.Add("Skill node '" + nodeId + "' " + columnName + " references unknown choice '" + choiceId + "'.");
			}
		}

		internal static void ValidatePassiveGate(string nodeId, string columnName, string passiveId, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(passiveId) && (!model.Skills.TryGetValue(passiveId, out var value) || value.SkillKind != PakuriCsvSkillKind.Passive))
			{
				errors.Add("Skill node '" + nodeId + "' " + columnName + " references unknown passive '" + passiveId + "'.");
			}
		}

		internal static void ValidateSkillNodeParams(SkillNodeRow node, SkillNodeHandlerSchema schema, List<SkillNodeParamRow> nodeParams, List<string> errors)
		{
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < nodeParams.Count; i++)
			{
				SkillNodeParamRow skillNodeParamRow = nodeParams[i];
				hashSet.Add(skillNodeParamRow.ParamKey);
				if (!schema.AllowedParams.Contains(skillNodeParamRow.ParamKey))
				{
					errors.Add("Skill node '" + node.Id + "' handler '" + node.HandlerId + "' has unknown param '" + skillNodeParamRow.ParamKey + "'.");
				}
				else if (schema.EnumParamAllowedValues.ContainsKey(skillNodeParamRow.ParamKey))
				{
					if (skillNodeParamRow.ValueType != SkillNodeValueType.Enum)
					{
						errors.Add($"Skill node '{node.Id}' handler '{node.HandlerId}' param '{skillNodeParamRow.ParamKey}' must use value_type 'enum' but row uses '{skillNodeParamRow.ValueType}'.");
					}
					ValidateSkillNodeSchemaEnumParam(node, schema, skillNodeParamRow, errors);
				}
				else if (skillNodeParamRow.ValueType == SkillNodeValueType.Enum)
				{
					ValidateSkillNodeSchemaEnumParam(node, schema, skillNodeParamRow, errors);
				}
			}
			foreach (string requiredParam in schema.RequiredParams)
			{
				if (!hashSet.Contains(requiredParam))
				{
					errors.Add("Skill node '" + node.Id + "' handler '" + node.HandlerId + "' is missing required param '" + requiredParam + "'.");
				}
			}
		}

		internal static void ValidateSkillNodeLegacyOverlap(SkillNodeRow node, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (node == null || !node.EnabledByDefault || string.IsNullOrWhiteSpace(node.HandlerId))
			{
				return;
			}
			switch (node.OwnerKind)
			{
			case SkillNodeOwnerKind.Skill:
			{
				if (model.Skills.TryGetValue(node.OwnerId, out var value2))
				{
					ValidateSkillNodeLegacySkillOverlap(node, value2, errors);
				}
				break;
			}
			case SkillNodeOwnerKind.Choice:
			{
				if (model.SkillChoices.TryGetValue(node.OwnerId, out var value))
				{
					ValidateSkillNodeLegacyChoiceOverlap(node, value, errors);
				}
				break;
			}
			}
		}

		internal static void ValidateSkillNodeLegacySkillOverlap(SkillNodeRow node, CsvRowParser.SkillRow skill, List<string> errors)
		{
			if (string.Equals(node.HandlerId, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase) && skill.RequireExecuteThresholdToCast && !NearlyZero(skill.ExecuteHealthRatioThreshold))
			{
				AddLegacyOverlapError(node, "execute threshold wide columns", errors);
			}
			if (string.Equals(node.HandlerId, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase) && !NearlyEqual(skill.ExecuteDamageMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "execute_damage_multiplier", errors);
			}
			if ((string.Equals(node.HandlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase) || string.Equals(node.HandlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase)) && !NearlyEqual(skill.BossDamageMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "boss_damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "CooldownRefund", StringComparison.OrdinalIgnoreCase) && !NearlyZero(skill.KillCooldownRefundRatio))
			{
				AddLegacyOverlapError(node, "kill_cooldown_refund_ratio", errors);
			}
		}

		internal static void ValidateSkillNodeLegacyChoiceOverlap(SkillNodeRow node, CsvRowParser.SkillChoiceRow choice, List<string> errors)
		{
			if (string.Equals(node.HandlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasDamageMultiplier && !NearlyEqual(choice.DamageMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasDamageMultiplier && !NearlyEqual(choice.DamageMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.CountStatusId) || !NearlyZero(choice.DamageMultiplierPerCount) || choice.CountMax > 0))
			{
				AddLegacyOverlapError(node, "count_status_id/damage_multiplier_per_count/count_max", errors);
			}
			if (string.Equals(node.HandlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasCooldownMultiplier && !NearlyEqual(choice.CooldownMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "cooldown_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "CritChanceBonus", StringComparison.OrdinalIgnoreCase) && !NearlyZero(choice.CritChanceBonus))
			{
				AddLegacyOverlapError(node, "crit_chance_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "CritDamageBonus", StringComparison.OrdinalIgnoreCase) && !NearlyZero(choice.CritDamageBonus))
			{
				AddLegacyOverlapError(node, "crit_damage_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase) && choice.HasMagazineBonus && choice.MagazineBonus != 0)
			{
				AddLegacyOverlapError(node, "magazine_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasReloadTimeMultiplier && !NearlyEqual(choice.ReloadTimeMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "reload_time_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase) && choice.PierceBonus != 0)
			{
				AddLegacyOverlapError(node, "pierce_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase) && choice.HitTargetCountBonus != 0)
			{
				AddLegacyOverlapError(node, "hit_target_count_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasRadiusMultiplier && !NearlyEqual(choice.RadiusMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "radius_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase) && !NearlyZero(choice.RadiusBonus))
			{
				AddLegacyOverlapError(node, "radius_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase) && !NearlyZero(choice.BeamWidthBonus))
			{
				AddLegacyOverlapError(node, "beam_width_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasKnockbackDistanceMultiplier && !NearlyEqual(choice.KnockbackDistanceMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "knockback_distance_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.ReloadReduceTargetSkillId) || !NearlyZero(choice.ReloadReduceSecondsPerHit)))
			{
				AddLegacyOverlapError(node, "reload_reduce_*", errors);
			}
			if (string.Equals(node.HandlerId, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.CoreHitboxName) || choice.HasCoreDamageMultiplier))
			{
				AddLegacyOverlapError(node, "core_hitbox_name/core_damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.CoreHitboxName) || choice.HasCoreOnHitAdditionalDamage))
			{
				AddLegacyOverlapError(node, "core_on_hit_additional_damage_*", errors);
			}
			if (string.Equals(node.HandlerId, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.HitCountCooldownRefundTargetSkillId) || choice.HitCountCooldownRefundMinTargets > 0 || !NearlyZero(choice.HitCountCooldownRefundRatio)))
			{
				AddLegacyOverlapError(node, "hit_count_cooldown_refund_*", errors);
			}
			if (string.Equals(node.HandlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase) && !NearlyZero(choice.DurationBonus))
			{
				AddLegacyOverlapError(node, "duration_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasDamageDelayMultiplier && !NearlyEqual(choice.DamageDelayMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "damage_delay_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase) && (!NearlyZero(choice.ConsecutiveHitBonusRate) || !NearlyZero(choice.ConsecutiveHitMax)))
			{
				AddLegacyOverlapError(node, "consecutive_hit_bonus_rate/consecutive_hit_max", errors);
			}
			if (string.Equals(node.HandlerId, "BurstDamageRule", StringComparison.OrdinalIgnoreCase) && (choice.HasBurstDamageProjectileIndex || choice.HasBurstDamageMultiplier))
			{
				AddLegacyOverlapError(node, "burst_damage_projectile_index/burst_damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase) && (choice.FollowUpProjectileCount > 0 || !NearlyZero(choice.FollowUpProjectileDelaySeconds) || !NearlyEqual(choice.FollowUpProjectileDamageMultiplier, 1f)))
			{
				AddLegacyOverlapError(node, "follow_up_projectile_*", errors);
			}
			if (string.Equals(node.HandlerId, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.ThresholdStatusId) || choice.ThresholdStatusMinStacks > 0 || !string.IsNullOrWhiteSpace(choice.ThresholdApplyStatusId)))
			{
				AddLegacyOverlapError(node, "threshold_status_*/threshold_apply_status_id", errors);
			}
			if (string.Equals(node.HandlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase) && choice.HasTargetStatusStackDamageMultiplier)
			{
				AddLegacyOverlapError(node, "target_status_stack_damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase) && choice.HasConsumeTargetStatusRatioOverride)
			{
				AddLegacyOverlapError(node, "consume_target_status_ratio_override", errors);
			}
			if (string.Equals(node.HandlerId, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase) && (choice.HasBurstStatusProjectileIndex || choice.BurstStatusStacksBonus != 0))
			{
				AddLegacyOverlapError(node, "burst_status_projectile_index/burst_status_stacks_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase) && choice.HasStatusActionSpeedBonus && !NearlyZero(choice.StatusActionSpeedBonus))
			{
				AddLegacyOverlapError(node, "status_action_speed_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase) && choice.HasStatusAttackPowerBonus && !NearlyZero(choice.StatusAttackPowerBonus))
			{
				AddLegacyOverlapError(node, "status_attack_power_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase) && choice.HasStatusAilmentResistanceBonus && !NearlyZero(choice.StatusAilmentResistanceBonus))
			{
				AddLegacyOverlapError(node, "status_ailment_resistance_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase) && (!string.IsNullOrWhiteSpace(choice.StatusDurationBonusStatusId) || !NearlyZero(choice.StatusDurationBonus)))
			{
				AddLegacyOverlapError(node, "status_duration_bonus_*", errors);
			}
			if (string.Equals(node.HandlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase) && choice.HasStatusConditionalDamageTakenBonus)
			{
				AddLegacyOverlapError(node, "status_conditional_*", errors);
			}
			if (string.Equals(node.HandlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase) && choice.HasStatusElementDamageTakenBonus && !NearlyZero(choice.StatusElementDamageTakenBonus))
			{
				AddLegacyOverlapError(node, "status_element_damage_taken_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase) && choice.HasStatusCriticalDamageTakenBonus && !NearlyZero(choice.StatusCriticalDamageTakenBonus))
			{
				AddLegacyOverlapError(node, "status_critical_damage_taken_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase) && choice.HasExecuteHealthRatioBonus && !NearlyZero(choice.ExecuteHealthRatioBonus))
			{
				AddLegacyOverlapError(node, "execute_health_ratio_bonus", errors);
			}
			if (string.Equals(node.HandlerId, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase) && !NearlyZero(choice.ExecuteCritChanceBonus))
			{
				AddLegacyOverlapError(node, "execute_crit_chance_bonus", errors);
			}
			if ((string.Equals(node.HandlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase) || string.Equals(node.HandlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase)) && choice.HasBossDamageMultiplier && !NearlyEqual(choice.BossDamageMultiplier, 1f))
			{
				AddLegacyOverlapError(node, "boss_damage_multiplier", errors);
			}
			if (string.Equals(node.HandlerId, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase) && choice.HasKillCooldownRefundRatioBonus && !NearlyZero(choice.KillCooldownRefundRatioBonus))
			{
				AddLegacyOverlapError(node, "kill_cooldown_refund_ratio_bonus", errors);
			}
			if ((string.Equals(node.HandlerId, "CooldownReset", StringComparison.OrdinalIgnoreCase) || string.Equals(node.HandlerId, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase)) && choice.KillResetsCooldown)
			{
				AddLegacyOverlapError(node, "kill_resets_cooldown", errors);
			}
			if (string.Equals(node.HandlerId, "AdditionalDamage", StringComparison.OrdinalIgnoreCase) && choice.HasOnHitAdditionalDamage)
			{
				AddLegacyOverlapError(node, "on_hit_additional_damage_*", errors);
			}
			if (string.Equals(node.HandlerId, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase) && (choice.OnHitChainHitPeriod > 0 || choice.OnHitChainTargetCount > 0 || !NearlyZero(choice.OnHitChainSearchRadius) || (choice.OnHitChainDamageMultiplier > 0f && !NearlyEqual(choice.OnHitChainDamageMultiplier, 1f))))
			{
				AddLegacyOverlapError(node, "on_hit_chain_*", errors);
			}
			if (string.Equals(node.HandlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase) && (choice.RepeatCountPerTarget > 0 || !NearlyZero(choice.RepeatIntervalSeconds) || (choice.RepeatDamageMultiplier > 0f && !NearlyEqual(choice.RepeatDamageMultiplier, 1f))))
			{
				AddLegacyOverlapError(node, "repeat_*", errors);
			}
			if (string.Equals(node.HandlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase) && (!NearlyZero(choice.ConditionalCritChanceBonus) || !string.IsNullOrWhiteSpace(choice.ConditionalCritTargetStatusId) || choice.ConditionalCritTargetStatusMinStacks > 0))
			{
				AddLegacyOverlapError(node, "conditional_crit_*", errors);
			}
			if (string.Equals(node.HandlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase) && (!NearlyZero(choice.RedistributeConsumedStatusRatioOnKill) || !string.IsNullOrWhiteSpace(choice.RedistributeConsumedStatusId) || !NearlyZero(choice.RedistributeConsumedStatusSearchRadius) || choice.RedistributeConsumedStatusTargetCount > 0))
			{
				AddLegacyOverlapError(node, "redistribute_consumed_status_*", errors);
			}
		}

		internal static void AddLegacyOverlapError(SkillNodeRow node, string legacyColumn, List<string> errors)
		{
			errors.Add("Skill node '" + node.Id + "' handler '" + node.HandlerId + "' overlaps active legacy wide field '" + legacyColumn + "' on owner '" + node.OwnerId + "'. Disable one side before validation.");
		}

		internal static bool NearlyZero(float value)
		{
			return Math.Abs(value) <= 0.0001f;
		}

		internal static bool NearlyEqual(float left, float right)
		{
			return Math.Abs(left - right) <= 0.0001f;
		}

		internal static void ValidateSkillNodeParamValue(SkillNodeParamRow param, CsvSourceModel.SourceModel model, CsvRuntimeCatalog assetCatalog, List<string> errors)
		{
			string text = ((param.Value != null) ? param.Value.Trim() : string.Empty);
			switch (param.ValueType)
			{
			case SkillNodeValueType.String:
				break;
			case SkillNodeValueType.Int:
			{
				if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' value '" + param.Value + "' is not a valid int.");
				}
				break;
			}
			case SkillNodeValueType.Float:
			{
				if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' value '" + param.Value + "' is not a valid float.");
				}
				break;
			}
			case SkillNodeValueType.Bool:
			{
				if (!bool.TryParse(text, out var _))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' value '" + param.Value + "' is not a valid bool.");
				}
				break;
			}
			case SkillNodeValueType.Enum:
				ValidateSkillNodeEnumParam(param, text, errors);
				break;
			case SkillNodeValueType.AssetPath:
				if (string.IsNullOrWhiteSpace(text) || assetCatalog == null || (!assetCatalog.HasSprite(text) && !assetCatalog.HasPrefab(text) && !assetCatalog.HasAnimatorController(text)))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' references unknown asset path '" + param.Value + "'.");
				}
				break;
			case SkillNodeValueType.SkillId:
				if (string.IsNullOrWhiteSpace(text) || (!model.Skills.ContainsKey(text) && (!IsEffectSourceSkillNodeParam(param) || !CsvDataValidator.HasSkillEffectSource(model, text))))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' references unknown skill '" + param.Value + "'.");
				}
				break;
			case SkillNodeValueType.StatusId:
			{
				if (string.IsNullOrWhiteSpace(text) || (!model.StatusEffects.ContainsKey(text) && !StatusEffectLookup.TryParse(text, out var _)))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' references unknown status '" + param.Value + "'.");
				}
				break;
			}
			case SkillNodeValueType.ChoiceId:
				if (string.IsNullOrWhiteSpace(text) || !model.SkillChoices.ContainsKey(text))
				{
					errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' references unknown choice '" + param.Value + "'.");
				}
				break;
			default:
				errors.Add($"Skill node param '{param.NodeId}.{param.ParamKey}' has unsupported value_type '{param.ValueType}'.");
				break;
			}
		}

		internal static bool IsEffectSourceSkillNodeParam(SkillNodeParamRow param)
		{
			if (param != null)
			{
				return string.Equals(param.ParamKey, "source_skill_id", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		internal static void ValidateSkillNodeEnumParam(SkillNodeParamRow param, string value, List<string> errors)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' requires a non-empty enum value.");
			}
		}

		internal static void ValidateSkillNodeSchemaEnumParam(SkillNodeRow node, SkillNodeHandlerSchema schema, SkillNodeParamRow param, List<string> errors)
		{
			string text = ((param.Value != null) ? param.Value.Trim() : string.Empty);
			if (!string.IsNullOrWhiteSpace(text))
			{
				if (!schema.EnumParamAllowedValues.TryGetValue(param.ParamKey, out var value))
				{
					errors.Add("Skill node '" + node.Id + "' handler '" + node.HandlerId + "' param '" + param.ParamKey + "' is marked enum but has no registered enum value schema.");
				}
				else if (!value.Contains(text))
				{
					errors.Add("Skill node '" + node.Id + "' handler '" + node.HandlerId + "' param '" + param.ParamKey + "' has invalid enum value '" + param.Value + "'. Allowed values: " + string.Join(", ", value) + ".");
				}
			}
		}

		internal static void MaterializeSkillGraphRows(CsvSourceModel.SourceModel model)
		{
			if (model == null || model.SkillGraphNodes.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < model.SkillGraphNodes.Count; i++)
			{
				hashSet.Add(model.SkillGraphNodes[i].MonsterId);
			}
			foreach (SkillNodeRow value8 in model.SkillNodes.Values)
			{
				if (value8 != null && !string.IsNullOrWhiteSpace(value8.MonsterId) && hashSet.Contains(value8.MonsterId))
				{
					list.Add("Monster '" + value8.MonsterId + "' has both skill_graph_nodes rows and legacy node '" + value8.Id + "'. Remove one authoring path.");
				}
			}
			Dictionary<string, List<SkillNodeTypeParamRow>> dictionary = BuildSkillNodeTypeParamLookup(model, list);
			ValidateSkillNodeTypeDefinitions(model, dictionary, list);
			List<SkillNodeRow> list2 = new List<SkillNodeRow>(model.SkillGraphNodes.Count);
			List<SkillNodeParamRow> list3 = new List<SkillNodeParamRow>();
			HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet3 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Dictionary<string, int> dictionary2 = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
			for (int j = 0; j < model.SkillGraphNodes.Count; j++)
			{
				SkillGraphNodeRow skillGraphNodeRow = model.SkillGraphNodes[j];
				string text = BuildSkillGraphKey(skillGraphNodeRow);
				string text2 = $"{text}:{skillGraphNodeRow.NodeOrder}";
				if (!hashSet3.Add(text2))
				{
					list.Add($"Skill graph '{text}' has duplicate node_order '{skillGraphNodeRow.NodeOrder}'.");
					continue;
				}
				if (skillGraphNodeRow.GraphIndex < 0)
				{
					list.Add("Skill graph '" + text + "' requires graph_index >= 0.");
				}
				if (!model.Monsters.ContainsKey(skillGraphNodeRow.MonsterId))
				{
					list.Add("Skill graph '" + text + "' references unknown monster '" + skillGraphNodeRow.MonsterId + "'.");
				}
				if (!model.SkillNodeTypes.TryGetValue(skillGraphNodeRow.NodeTypeId, out var value))
				{
					list.Add("Skill graph node '" + text2 + "' references unknown node_type_id '" + skillGraphNodeRow.NodeTypeId + "'.");
					continue;
				}
				if (!SkillNodeHandlerSchemas.TryGetValue(value.HandlerId, out var _))
				{
					list.Add("Skill graph node '" + text2 + "' uses unregistered handler_id '" + value.HandlerId + "'.");
					continue;
				}
				string text3 = ResolveSkillGraphTargetSkillId(model, skillGraphNodeRow, list);
				if (!string.IsNullOrWhiteSpace(text3) && model.Skills.TryGetValue(text3, out var value3) && !string.Equals(value3.MonsterId, skillGraphNodeRow.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					list.Add("Skill graph '" + text + "' target skill '" + text3 + "' belongs to '" + value3.MonsterId + "', not '" + skillGraphNodeRow.MonsterId + "'.");
				}
				if (skillGraphNodeRow.GraphKind == SkillGraphKind.Plan && IsEffectGraphOnlyHandler(value.HandlerId))
				{
					list.Add($"Skill graph '{text}' is Plan but node '{skillGraphNodeRow.NodeOrder}' uses Effect-only handler '{value.HandlerId}'.");
				}
				if (skillGraphNodeRow.GraphKind == SkillGraphKind.Effect && GameDataBuilder.IsEffectOperationHandler(value.HandlerId))
				{
					dictionary2.TryGetValue(text, out var value4);
					dictionary2[text] = value4 + 1;
				}
				string text4 = BuildGeneratedSkillGraphNodeId(skillGraphNodeRow);
				if (!hashSet2.Add(text4) || model.SkillNodes.ContainsKey(text4))
				{
					list.Add("Skill graph generated duplicate node id '" + text4 + "'.");
					continue;
				}
				SkillNodeOwnerKind ownerKind = ((skillGraphNodeRow.GraphKind == SkillGraphKind.Effect) ? SkillNodeOwnerKind.Effect : skillGraphNodeRow.OwnerKind);
				string ownerId = ((skillGraphNodeRow.GraphKind == SkillGraphKind.Effect) ? BuildGeneratedSkillGraphEffectId(skillGraphNodeRow.OwnerKind, skillGraphNodeRow.OwnerId, skillGraphNodeRow.GraphIndex) : skillGraphNodeRow.OwnerId);
				string requiresActiveChoiceId = ((skillGraphNodeRow.GraphKind == SkillGraphKind.Effect && skillGraphNodeRow.OwnerKind == SkillNodeOwnerKind.Choice) ? skillGraphNodeRow.OwnerId : string.Empty);
				string requiresPassiveSkillId = ((skillGraphNodeRow.GraphKind == SkillGraphKind.Effect) ? ResolveGeneratedEffectPassiveSkillId(model, skillGraphNodeRow) : string.Empty);
				list2.Add(new SkillNodeRow
				{
					Id = text4,
					MonsterId = skillGraphNodeRow.MonsterId,
					OwnerKind = ownerKind,
					OwnerId = ownerId,
					TargetSkillId = text3,
					NodeKind = value.NodeKind,
					HandlerId = value.HandlerId,
					SortOrder = skillGraphNodeRow.NodeOrder,
					EnabledByDefault = true,
					RequiresActiveChoiceId = requiresActiveChoiceId,
					ExcludesActiveChoiceId = skillGraphNodeRow.ExcludesActiveChoiceId,
					RequiresPassiveSkillId = requiresPassiveSkillId,
					ExcludesPassiveSkillId = string.Empty,
					RuntimeSupportState = value.RuntimeSupportState,
					RuntimeSupportNotes = value.RuntimeSupportNotes
				});
				dictionary.TryGetValue(skillGraphNodeRow.NodeTypeId, out var value5);
				value5 = value5 ?? new List<SkillNodeTypeParamRow>();
				HashSet<int> hashSet4 = new HashSet<int>();
				for (int k = 0; k < value5.Count; k++)
				{
					SkillNodeTypeParamRow skillNodeTypeParamRow = value5[k];
					hashSet4.Add(skillNodeTypeParamRow.ParamOrder);
					string value6 = skillGraphNodeRow.Args[skillNodeTypeParamRow.ParamOrder - 1];
					if (string.IsNullOrWhiteSpace(value6))
					{
						if (skillNodeTypeParamRow.Required)
						{
							list.Add($"Skill graph node '{text2}' requires arg_{skillNodeTypeParamRow.ParamOrder} for param '{skillNodeTypeParamRow.ParamKey}'.");
						}
						continue;
					}
					ValidateSkillGraphAllowedValue(text2, skillNodeTypeParamRow, value6, list);
					list3.Add(new SkillNodeParamRow
					{
						NodeId = text4,
						MonsterId = skillGraphNodeRow.MonsterId,
						ParamKey = skillNodeTypeParamRow.ParamKey,
						ValueType = skillNodeTypeParamRow.ValueType,
						Value = value6
					});
				}
				for (int l = 0; l < skillGraphNodeRow.Args.Length; l++)
				{
					if (!string.IsNullOrWhiteSpace(skillGraphNodeRow.Args[l]) && !hashSet4.Contains(l + 1))
					{
						list.Add($"Skill graph node '{text2}' sets arg_{l + 1}, but node type '{skillGraphNodeRow.NodeTypeId}' has no matching param definition.");
					}
				}
			}
			HashSet<string> hashSet5 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int m = 0; m < model.SkillGraphNodes.Count; m++)
			{
				SkillGraphNodeRow skillGraphNodeRow2 = model.SkillGraphNodes[m];
				if (skillGraphNodeRow2.GraphKind == SkillGraphKind.Effect)
				{
					hashSet5.Add(BuildSkillGraphKey(skillGraphNodeRow2));
				}
			}
			foreach (string item in hashSet5)
			{
				dictionary2.TryGetValue(item, out var value7);
				if (value7 != 1)
				{
					list.Add($"Effect graph '{item}' requires exactly one operation handler but has {value7}.");
				}
			}
			if (list.Count > 0)
			{
				throw new CsvParser.CsvFatalException("Skill graph authoring materialization failed.", list);
			}
			for (int n = 0; n < list2.Count; n++)
			{
				model.SkillNodes.Add(list2[n].Id, list2[n]);
			}
			model.SkillNodeParams.AddRange(list3);
		}

		internal static Dictionary<string, List<SkillNodeTypeParamRow>> BuildSkillNodeTypeParamLookup(CsvSourceModel.SourceModel model, List<string> errors)
		{
			Dictionary<string, List<SkillNodeTypeParamRow>> dictionary = new Dictionary<string, List<SkillNodeTypeParamRow>>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (int i = 0; i < model.SkillNodeTypeParams.Count; i++)
			{
				SkillNodeTypeParamRow skillNodeTypeParamRow = model.SkillNodeTypeParams[i];
				if (!model.SkillNodeTypes.ContainsKey(skillNodeTypeParamRow.NodeTypeId))
				{
					errors.Add("Skill node type param '" + skillNodeTypeParamRow.NodeTypeId + "." + skillNodeTypeParamRow.ParamKey + "' references unknown node_type_id.");
					continue;
				}
				if (skillNodeTypeParamRow.ParamOrder < 1 || skillNodeTypeParamRow.ParamOrder > 12)
				{
					errors.Add("Skill node type param '" + skillNodeTypeParamRow.NodeTypeId + "." + skillNodeTypeParamRow.ParamKey + "' requires param_order between 1 and 12.");
					continue;
				}
				string item = $"{skillNodeTypeParamRow.NodeTypeId}:{skillNodeTypeParamRow.ParamOrder}";
				string item2 = skillNodeTypeParamRow.NodeTypeId + ":" + skillNodeTypeParamRow.ParamKey;
				if (!hashSet.Add(item) || !hashSet.Add(item2))
				{
					errors.Add("Skill node type '" + skillNodeTypeParamRow.NodeTypeId + "' has duplicate param order or key for '" + skillNodeTypeParamRow.ParamKey + "'.");
				}
				else
				{
					if (!dictionary.TryGetValue(skillNodeTypeParamRow.NodeTypeId, out var value))
					{
						value = new List<SkillNodeTypeParamRow>();
						dictionary.Add(skillNodeTypeParamRow.NodeTypeId, value);
					}
					value.Add(skillNodeTypeParamRow);
				}
			}
			foreach (KeyValuePair<string, List<SkillNodeTypeParamRow>> item3 in dictionary)
			{
				item3.Value.Sort((SkillNodeTypeParamRow left, SkillNodeTypeParamRow right) => left.ParamOrder.CompareTo(right.ParamOrder));
			}
			return dictionary;
		}

		internal static void ValidateSkillNodeTypeDefinitions(CsvSourceModel.SourceModel model, Dictionary<string, List<SkillNodeTypeParamRow>> paramsByType, List<string> errors)
		{
			foreach (SkillNodeTypeRow value3 in model.SkillNodeTypes.Values)
			{
				if (!SkillNodeHandlerSchemas.TryGetValue(value3.HandlerId, out var value))
				{
					errors.Add("Skill node type '" + value3.Id + "' uses unregistered handler_id '" + value3.HandlerId + "'.");
					continue;
				}
				if (value3.NodeKind != value.NodeKind)
				{
					errors.Add($"Skill node type '{value3.Id}' handler '{value3.HandlerId}' requires node_kind '{value.NodeKind}', not '{value3.NodeKind}'.");
				}
				paramsByType.TryGetValue(value3.Id, out var value2);
				value2 = value2 ?? new List<SkillNodeTypeParamRow>();
				HashSet<string> hashSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < value2.Count; i++)
				{
					SkillNodeTypeParamRow skillNodeTypeParamRow = value2[i];
					hashSet.Add(skillNodeTypeParamRow.ParamKey);
					if (!value.AllowedParams.Contains(skillNodeTypeParamRow.ParamKey))
					{
						errors.Add("Skill node type '" + value3.Id + "' defines unsupported param '" + skillNodeTypeParamRow.ParamKey + "' for handler '" + value3.HandlerId + "'.");
					}
					bool flag = value.RequiredParams.Contains(skillNodeTypeParamRow.ParamKey);
					if (skillNodeTypeParamRow.Required != flag)
					{
						errors.Add($"Skill node type '{value3.Id}' param '{skillNodeTypeParamRow.ParamKey}' required={skillNodeTypeParamRow.Required} but handler schema requires {flag}.");
					}
				}
				foreach (string requiredParam in value.RequiredParams)
				{
					if (!hashSet.Contains(requiredParam))
					{
						errors.Add("Skill node type '" + value3.Id + "' is missing required handler param definition '" + requiredParam + "'.");
					}
				}
			}
		}

		internal static string ResolveSkillGraphTargetSkillId(CsvSourceModel.SourceModel model, SkillGraphNodeRow graph, List<string> errors)
		{
			string text;
			switch (graph.OwnerKind)
			{
			case SkillNodeOwnerKind.Choice:
			{
				if (!model.SkillChoices.TryGetValue(graph.OwnerId, out var value2))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown choice owner '" + graph.OwnerId + "'.");
					return graph.TargetSkillId;
				}
				if (!string.Equals(value2.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph choice owner '" + graph.OwnerId + "' belongs to '" + value2.MonsterId + "', not '" + graph.MonsterId + "'.");
				}
				text = (string.IsNullOrWhiteSpace(value2.TargetSkillId) ? value2.SkillId : value2.TargetSkillId);
				break;
			}
			case SkillNodeOwnerKind.Skill:
			{
				if (!model.Skills.TryGetValue(graph.OwnerId, out var value3))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown skill owner '" + graph.OwnerId + "'.");
					return graph.TargetSkillId;
				}
				if (!string.Equals(value3.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph skill owner '" + graph.OwnerId + "' belongs to '" + value3.MonsterId + "', not '" + graph.MonsterId + "'.");
				}
				text = graph.OwnerId;
				break;
			}
			case SkillNodeOwnerKind.Trigger:
			{
				if (!model.SkillTriggers.TryGetValue(graph.OwnerId, out var value))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown trigger owner '" + graph.OwnerId + "'.");
					return graph.TargetSkillId;
				}
				if (!string.Equals(value.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph trigger owner '" + graph.OwnerId + "' belongs to '" + value.MonsterId + "', not '" + graph.MonsterId + "'.");
				}
				text = value.SourceSkillId;
				break;
			}
			default:
				errors.Add($"Skill graph '{BuildSkillGraphKey(graph)}' uses unsupported owner_kind '{graph.OwnerKind}'.");
				return graph.TargetSkillId;
			}
			if (!string.IsNullOrWhiteSpace(graph.TargetSkillId))
			{
				text = graph.TargetSkillId;
			}
			if (string.IsNullOrWhiteSpace(text) || !model.Skills.ContainsKey(text))
			{
				errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' resolves unknown target_skill_id '" + text + "'.");
			}
			return text;
		}

		internal static string ResolveGeneratedEffectPassiveSkillId(CsvSourceModel.SourceModel model, SkillGraphNodeRow graph)
		{
			if (model == null || graph == null || graph.GraphKind != SkillGraphKind.Effect)
			{
				return string.Empty;
			}
			if (graph.OwnerKind == SkillNodeOwnerKind.Skill && model.Skills.TryGetValue(graph.OwnerId, out var value) && value.SkillKind == PakuriCsvSkillKind.Passive)
			{
				return value.Id;
			}
			if (graph.OwnerKind == SkillNodeOwnerKind.Choice && model.SkillChoices.TryGetValue(graph.OwnerId, out var value2) && model.Skills.TryGetValue(value2.SkillId, out var value3) && value3.SkillKind == PakuriCsvSkillKind.Passive)
			{
				return value3.Id;
			}
			return string.Empty;
		}

		internal static void ValidateSkillGraphAllowedValue(string graphNodeKey, SkillNodeTypeParamRow param, string value, List<string> errors)
		{
			if (string.IsNullOrWhiteSpace(param.AllowedValues))
			{
				return;
			}
			string[] array = param.AllowedValues.Split('|');
			for (int i = 0; i < array.Length; i++)
			{
				if (string.Equals(array[i].Trim(), value.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
			}
			errors.Add("Skill graph node '" + graphNodeKey + "' param '" + param.ParamKey + "' has invalid value '" + value + "'. Allowed: " + param.AllowedValues + ".");
		}

		internal static bool IsEffectGraphOnlyHandler(string handlerId)
		{
			if (!GameDataBuilder.IsEffectOperationHandler(handlerId) && !string.Equals(handlerId, "EffectTarget", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "StatusCriticalResistanceBonus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConditionStatus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(handlerId, "EffectLifetime", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		internal static string BuildSkillGraphKey(SkillGraphNodeRow graph)
		{
			return $"{graph.MonsterId}:{graph.OwnerKind}:{graph.OwnerId}:{graph.GraphKind}:{graph.GraphIndex}";
		}

		internal static string BuildGeneratedSkillGraphNodeId(SkillGraphNodeRow graph)
		{
			return $"{graph.OwnerKind}:{graph.OwnerId}:{graph.GraphKind}:{graph.GraphIndex}:{graph.NodeOrder}";
		}

		internal static string BuildGeneratedSkillGraphEffectId(SkillNodeOwnerKind ownerKind, string ownerId, int graphIndex)
		{
			if (ownerKind == SkillNodeOwnerKind.Choice || ownerKind == SkillNodeOwnerKind.Trigger)
			{
				if (graphIndex != 0)
				{
					return $"{ownerId}@effect{graphIndex + 1}";
				}
				return ownerId;
			}
			return $"{ownerId}@effect{graphIndex + 1}";
		}

		internal static bool HasSkillGraphReference(CsvRowParser.SkillTriggerRow trigger)
		{
			if (trigger != null)
			{
				return !string.IsNullOrWhiteSpace(trigger.TriggeredGraphOwnerId);
			}
			return false;
		}

		internal static string ResolveTriggeredEffectId(CsvRowParser.SkillTriggerRow trigger)
		{
			if (!HasSkillGraphReference(trigger))
			{
				if (trigger == null)
				{
					return string.Empty;
				}
				return trigger.TriggeredEffectId;
			}
			return BuildGeneratedSkillGraphEffectId(trigger.TriggeredGraphOwnerKind, trigger.TriggeredGraphOwnerId, trigger.TriggeredGraphIndex);
		}
	}
}
