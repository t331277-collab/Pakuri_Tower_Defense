using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;

/*
 * 스킬 노드와 그래프 CSV를 파싱하고 참조를 검증한 뒤 원본 모델에 정규화한다.
 */
namespace Pakuri.Data
{
	internal static class SkillGraphParser
	{
		/*
		 * CSV 실행 그래프 노드의 역할을 구분한다.
		 */
		internal enum SkillNodeKind
		{
			CastCondition,
			Action,
			DamageModifier,
			CritModifier,
			OnHitAction,
			OnKillAction,
			OnExpireAction
		}

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

		internal class SkillNodeRow
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

		internal class SkillNodeParamRow
		{
			public string NodeId;

			public string MonsterId;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public string Value;
		}

		internal class SkillNodeTypeRow
		{
			public string Id;

			public string HandlerId;

			public SkillNodeKind NodeKind;

			public string RuntimeSupportState;

			public string RuntimeSupportNotes;
		}

		internal class SkillNodeTypeParamRow
		{
			public string NodeTypeId;

			public int ParamOrder;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public bool Required;

			public string AllowedValues;
		}

		internal class SkillGraphNodeRow
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

		internal class SkillNodeHandlerSchema
		{
			public string HandlerId { get; }
			public SkillNodeKind NodeKind { get; }

			/*
			 * SkillNodeHandlerSchema에 필요한 값을 초기화한다.
			 */
			public SkillNodeHandlerSchema(string handlerId /* 처리기 식별자 */, SkillNodeKind nodeKind /* 노드 종류 */)
			{
				HandlerId = handlerId;
				NodeKind = nodeKind;
			}
		}

		internal static readonly Dictionary<string, SkillNodeHandlerSchema> SkillNodeHandlerSchemas = BuildSkillNodeHandlerSchemas();

		/*
		 * ParseSkillNodeTypeRow에 필요한 데이터를 읽어 변환한다.
		 */
		internal static SkillNodeTypeRow ParseSkillNodeTypeRow(CsvParser.CsvRecord record /* 읽을 CSV 행 */)
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

		/*
		 * ParseSkillNodeTypeParamRow에 필요한 데이터를 읽어 변환한다.
		 */
		internal static SkillNodeTypeParamRow ParseSkillNodeTypeParamRow(CsvParser.CsvRecord record /* 읽을 CSV 행 */)
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

		/*
		 * ParseSkillGraphNodeRow에 필요한 데이터를 읽어 변환한다.
		 */
		internal static SkillGraphNodeRow ParseSkillGraphNodeRow(CsvParser.CsvRecord record /* 읽을 CSV 행 */)
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

		/*
		 * ParseSkillNodeValueType에 필요한 데이터를 읽어 변환한다.
		 */
		internal static SkillNodeValueType ParseSkillNodeValueType(string rawValue /* 변환 전 원본 문자열 */, CsvParser.CsvRecord record /* 읽을 CSV 행 */)
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

		/*
		 * BuildSkillNodeHandlerSchemas에 필요한 결과를 만들어 반환한다.
		 */
		internal static Dictionary<string, SkillNodeHandlerSchema> BuildSkillNodeHandlerSchemas()
		{
			Dictionary<string, SkillNodeHandlerSchema> dictionary = new Dictionary<string, SkillNodeHandlerSchema>(StringComparer.OrdinalIgnoreCase);
			AddSkillNodeHandler(dictionary, "TargetHealthRatioCondition", SkillNodeKind.CastCondition);
			AddSkillNodeHandler(dictionary, "ExecuteDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "TargetPredicateDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "CooldownRefund", SkillNodeKind.OnKillAction);
			AddSkillNodeHandler(dictionary, "DamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "ShieldAmountMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "CountStatusDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "CooldownMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "CritChanceBonus", SkillNodeKind.CritModifier);
			AddSkillNodeHandler(dictionary, "CritDamageBonus", SkillNodeKind.CritModifier);
			AddSkillNodeHandler(dictionary, "MagazineBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ReloadTimeMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "PierceBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "HitTargetCountBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "RadiusMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "RadiusBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "BeamWidthBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "KnockbackDistanceMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ReloadReducePerHit", SkillNodeKind.OnHitAction);
			AddSkillNodeHandler(dictionary, "CoreDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "CoreAdditionalDamage", SkillNodeKind.OnHitAction);
			AddSkillNodeHandler(dictionary, "HitCountCooldownRefund", SkillNodeKind.OnHitAction);
			AddSkillNodeHandler(dictionary, "DurationBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "DurationMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "DamageDelayMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "AdditionalProjectileBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ShotIntervalMultiplier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConsecutiveHitDamageBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "BurstDamageRule", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "FollowUpProjectile", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ThresholdApplyStatus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "TargetStatusStackDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "ConsumeTargetStatusRatioOverride", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "BurstStatusStacksBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusStackAmountBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusStackAmountSet", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusMaxStacksBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConditionalDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "TargetStatusStackDamageRateBonus", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "TriggerProcChanceBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusActionSpeedBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusAttackPowerBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusMoveSpeedBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusAilmentResistanceBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusDamageBonusRate", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusShieldReceivedBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusCriticalChanceBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusDamageTakenBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusFlatElementResistReduction", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusDurationBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusConditionalDamageTakenBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusElementDamageTakenBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusConditionalStatusChanceBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusCriticalDamageTakenBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusCriticalDamageBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusElementResistReduction", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusOutgoingAdditionalDamage", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusSpellPowerBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ApplyStatus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ApplyShield", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusModifier", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "EffectStatus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "EffectDamage", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "EffectExtendStatusDuration", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "RecastZone", SkillNodeKind.OnExpireAction);
			AddSkillNodeHandler(dictionary, "EffectTarget", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "EffectVisual", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "AttachStatusPayload", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "RequiredSourceStatus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusRuntimeKindFilter", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "StatusCriticalResistanceBonus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "RuntimeEffectVisual", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConditionStatus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConditionAnyStatus", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConditionSkillAttribute", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConditionHealthRatioMax", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConditionHitCountMin", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "EffectLifetime", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "DelayedDamage", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "RequiredTargetStatus", SkillNodeKind.CastCondition);
			AddSkillNodeHandler(dictionary, "TargetStatusStackDamage", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "StatusFilteredDeployment", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "ConsumeTargetStatus", SkillNodeKind.OnHitAction);
			AddSkillNodeHandler(dictionary, "CooldownReset", SkillNodeKind.OnKillAction);
			AddSkillNodeHandler(dictionary, "AdditionalDamage", SkillNodeKind.OnHitAction);
			AddSkillNodeHandler(dictionary, "EveryNthHitChainDamage", SkillNodeKind.OnHitAction);
			AddSkillNodeHandler(dictionary, "RepeatPerTarget", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "TargetStatusCritBonus", SkillNodeKind.CritModifier);
			AddSkillNodeHandler(dictionary, "RedistributeConsumedStatus", SkillNodeKind.OnKillAction);
			AddSkillNodeHandler(dictionary, "TargetHealthRatioThresholdBonus", SkillNodeKind.CastCondition);
			AddSkillNodeHandler(dictionary, "ExecuteCritChanceBonus", SkillNodeKind.CritModifier);
			AddSkillNodeHandler(dictionary, "CooldownRefundBonus", SkillNodeKind.OnKillAction);
			AddSkillNodeHandler(dictionary, "BranchDamage", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "SpawnProjectile", SkillNodeKind.Action);
			AddSkillNodeHandler(dictionary, "BossDamageMultiplier", SkillNodeKind.DamageModifier);
			AddSkillNodeHandler(dictionary, "CooldownResetOnKill", SkillNodeKind.OnKillAction);
			return dictionary;
		}

		/*
		 * AddSkillNodeHandler 작업을 수행한다.
		 */
		internal static void AddSkillNodeHandler(
			Dictionary<string, SkillNodeHandlerSchema> schemas /* 형식 목록 */,
			string handlerId /* 처리기 식별자 */,
			SkillNodeKind nodeKind /* 노드 종류 */)
		{
			schemas.Add(handlerId, new SkillNodeHandlerSchema(handlerId, nodeKind));
		}

		/*
		 * ValidateNormalizedSkillAuthoringRows 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateNormalizedSkillAuthoringRows(CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */, List<string> errors /* 검증 오류를 모을 목록 */)
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
			}
		}

		/*
		 * ValidateSkillNodeOwner 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateSkillNodeOwner(SkillNodeRow node /* 노드 */, CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

		/*
		 * ValidateSkillNodeGateReferences 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateSkillNodeGateReferences(SkillNodeRow node /* 노드 */, CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, List<string> errors /* 검증 오류를 모을 목록 */)
		{
			ValidateChoiceGate(node.Id, "requires_active_choice_id", node.RequiresActiveChoiceId, model, errors);
			ValidateChoiceGate(node.Id, "excludes_active_choice_id", node.ExcludesActiveChoiceId, model, errors);
			ValidatePassiveGate(node.Id, "requires_passive_skill_id", node.RequiresPassiveSkillId, model, errors);
			ValidatePassiveGate(node.Id, "excludes_passive_skill_id", node.ExcludesPassiveSkillId, model, errors);
		}

		/*
		 * ValidateChoiceGate 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateChoiceGate(string nodeId /* 노드 식별자 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, string choiceId /* 스킬 선택지 식별자 */, CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, List<string> errors /* 검증 오류를 모을 목록 */)
		{
			if (!string.IsNullOrWhiteSpace(choiceId) && !model.SkillChoices.ContainsKey(choiceId))
			{
				errors.Add("Skill node '" + nodeId + "' " + columnName + " references unknown choice '" + choiceId + "'.");
			}
		}

		/*
		 * ValidatePassiveGate 데이터가 올바른지 검사한다.
		 */
		internal static void ValidatePassiveGate(string nodeId /* 노드 식별자 */, string columnName /* 읽거나 검사할 CSV 열 이름 */, string passiveId /* 패시브 식별자 */, CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, List<string> errors /* 검증 오류를 모을 목록 */)
		{
			if (!string.IsNullOrWhiteSpace(passiveId) && (!model.Skills.TryGetValue(passiveId, out var value) || value.SkillKind != PakuriCsvSkillKind.Passive))
			{
				errors.Add("Skill node '" + nodeId + "' " + columnName + " references unknown passive '" + passiveId + "'.");
			}
		}

		/*
		 * ValidateSkillNodeParamValue 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateSkillNodeParamValue(SkillNodeParamRow param /* 매개변수 */, CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */, List<string> errors /* 검증 오류를 모을 목록 */)
		{
			string text = string.Empty;
			if (param.Value != null)
			{
				text = param.Value.Trim();
			}
			switch (param.ValueType)
			{
			case SkillNodeValueType.String:
				if (string.Equals(param.ParamKey, "status_ids", StringComparison.OrdinalIgnoreCase))
				{
					CsvDataValidator.ValidateStatusIdList(param.NodeId, text, model, errors);
				}
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
				if (string.IsNullOrWhiteSpace(text) || !model.StatusEffects.ContainsKey(text))
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

		/*
		 * IsEffectSourceSkillNodeParam 조건을 만족하는지 확인한다.
		 */
		internal static bool IsEffectSourceSkillNodeParam(SkillNodeParamRow param /* 매개변수 */)
		{
			if (param != null)
			{
				return string.Equals(param.ParamKey, "source_skill_id", StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		/*
		 * ValidateSkillNodeEnumParam 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateSkillNodeEnumParam(SkillNodeParamRow param /* 매개변수 */, string value /* 처리할 값 */, List<string> errors /* 검증 오류를 모을 목록 */)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' requires a non-empty enum value.");
			}
		}

		/*
		 * MaterializeSkillGraphRows 작업을 수행한다.
		 */
		internal static void MaterializeSkillGraphRows(CsvSourceModel.SourceModel model /* 처리할 상태 모델 */)
		{
			if (model == null || model.SkillGraphNodes.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			Dictionary<string, List<SkillNodeTypeParamRow>> dictionary = BuildSkillNodeTypeParamLookup(model, list);
			ValidateSkillNodeTypeDefinitions(model, list);
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
				if (skillGraphNodeRow.GraphKind == SkillGraphKind.Effect && IsEffectOperationHandler(value.HandlerId))
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
				SkillNodeOwnerKind ownerKind = skillGraphNodeRow.OwnerKind;
				string ownerId = skillGraphNodeRow.OwnerId;
				string requiresActiveChoiceId = string.Empty;
				string requiresPassiveSkillId = string.Empty;
				if (skillGraphNodeRow.GraphKind == SkillGraphKind.Effect)
				{
					ownerKind = SkillNodeOwnerKind.Effect;
					ownerId = BuildGeneratedSkillGraphEffectId(
						skillGraphNodeRow.OwnerKind,
						skillGraphNodeRow.OwnerId,
						skillGraphNodeRow.GraphIndex);
					requiresPassiveSkillId = ResolveGeneratedEffectPassiveSkillId(model, skillGraphNodeRow);
					if (skillGraphNodeRow.OwnerKind == SkillNodeOwnerKind.Choice)
					{
						requiresActiveChoiceId = skillGraphNodeRow.OwnerId;
					}
				}
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
				if (value5 == null)
				{
					value5 = new List<SkillNodeTypeParamRow>();
				}
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

		/*
		 * BuildSkillNodeTypeParamLookup에 필요한 결과를 만들어 반환한다.
		 */
		internal static Dictionary<string, List<SkillNodeTypeParamRow>> BuildSkillNodeTypeParamLookup(CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

		/*
		 * ValidateSkillNodeTypeDefinitions 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateSkillNodeTypeDefinitions(
			CsvSourceModel.SourceModel model /* 처리할 상태 모델 */,
			List<string> errors /* 검증 오류를 모을 목록 */)
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
			}
		}

		/*
		 * ResolveSkillGraphTargetSkillId 결과를 계산해 반환한다.
		 */
		internal static string ResolveSkillGraphTargetSkillId(CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, SkillGraphNodeRow graph /* 그래프 */, List<string> errors /* 검증 오류를 모을 목록 */)
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
				text = value2.TargetSkillId;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = value2.SkillId;
				}
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

		/*
		 * ResolveGeneratedEffectPassiveSkillId 결과를 계산해 반환한다.
		 */
		internal static string ResolveGeneratedEffectPassiveSkillId(CsvSourceModel.SourceModel model /* 처리할 상태 모델 */, SkillGraphNodeRow graph /* 그래프 */)
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

		/*
		 * ValidateSkillGraphAllowedValue 데이터가 올바른지 검사한다.
		 */
		internal static void ValidateSkillGraphAllowedValue(string graphNodeKey /* 그래프 노드 조회 키 */, SkillNodeTypeParamRow param /* 매개변수 */, string value /* 처리할 값 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

		/*
		 * IsEffectOperationHandler 조건을 만족하는지 확인한다.
		 */
		internal static bool IsEffectOperationHandler(string handlerId /* 처리기 식별자 */)
		{
			return string.Equals(handlerId, "ApplyStatus", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handlerId, "ApplyShield", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handlerId, "StatusModifier", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handlerId, "EffectStatus", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handlerId, "EffectDamage", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handlerId, "RecastZone", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handlerId, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase);
		}

		/*
		 * 효과 그래프에서만 허용하는 핸들러인지 확인한다.
		 */
		internal static bool IsEffectGraphOnlyHandler(string handlerId /* 처리기 식별자 */)
		{
			if (!IsEffectOperationHandler(handlerId) && !string.Equals(handlerId, "EffectTarget", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "StatusCriticalResistanceBonus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConditionStatus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase) && !string.Equals(handlerId, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase))
			{
				return string.Equals(handlerId, "EffectLifetime", StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}

		/*
		 * BuildSkillGraphKey에 필요한 결과를 만들어 반환한다.
		 */
		internal static string BuildSkillGraphKey(SkillGraphNodeRow graph /* 그래프 */)
		{
			return $"{graph.MonsterId}:{graph.OwnerKind}:{graph.OwnerId}:{graph.GraphKind}:{graph.GraphIndex}";
		}

		/*
		 * BuildGeneratedSkillGraphNodeId에 필요한 결과를 만들어 반환한다.
		 */
		internal static string BuildGeneratedSkillGraphNodeId(SkillGraphNodeRow graph /* 그래프 */)
		{
			return $"{graph.OwnerKind}:{graph.OwnerId}:{graph.GraphKind}:{graph.GraphIndex}:{graph.NodeOrder}";
		}

		/*
		 * BuildGeneratedSkillGraphEffectId에 필요한 결과를 만들어 반환한다.
		 */
		internal static string BuildGeneratedSkillGraphEffectId(SkillNodeOwnerKind ownerKind /* 소유자 종류 */, string ownerId /* 소유자 식별자 */, int graphIndex /* 그래프 순서 번호 */)
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

		/*
		 * HasSkillGraphReference 조건을 만족하는지 확인한다.
		 */
		internal static bool HasSkillGraphReference(CsvRowParser.SkillTriggerRow trigger /* 실행하거나 검사할 트리거 */)
		{
			if (trigger != null)
			{
				return !string.IsNullOrWhiteSpace(trigger.TriggeredGraphOwnerId);
			}
			return false;
		}

		/*
		 * ResolveTriggeredEffectId 결과를 계산해 반환한다.
		 */
		internal static string ResolveTriggeredEffectId(CsvRowParser.SkillTriggerRow trigger /* 실행하거나 검사할 트리거 */)
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
