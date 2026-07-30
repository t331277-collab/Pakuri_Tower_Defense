/*
 * 역할: 스킬 그래프 원본 파싱.
 * 책임: CSV 행에서 Node 종류·Node·Trigger·조건·작업·그래프 관계를 파싱한다.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pakuri.Combat;
using Pakuri.InGame;

namespace Pakuri.Data
{

	/// SkillGraphParser 원본 값을 런타임 모델로 파싱한다.
	internal static class SkillGraphParser
	{

		/// SkillNodeOwnerKind에서 지원하는 값의 종류를 정의한다.
		internal enum SkillNodeOwnerKind
		{
			Skill,
			Choice,
			Passive,
			Effect,
			Trigger
		}

		/// SkillNodeValueType에서 지원하는 값의 종류를 정의한다.
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

		/// SkillNodeRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeRow
		{
			public string Id;

			public string MonsterId;

			public SkillNodeOwnerKind OwnerKind;

			public string OwnerId;

			public string TargetSkillId;

			public string HandlerId;

			public int SortOrder;

			public bool EnabledByDefault;

			public string RequiresActiveChoiceId;

			public string ExcludesActiveChoiceId;

			public string RequiresPassiveSkillId;

			public string ExcludesPassiveSkillId;

		}

		/// SkillNodeParamRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeParamRow
		{
			public string NodeId;

			public string MonsterId;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public string Value;
		}

		/// SkillNodeTypeRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeTypeRow
		{
			public string Id;

			public string HandlerId;

		}

		/// SkillNodeTypeParamRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeTypeParamRow
		{
			public string NodeTypeId;

			public int ParamOrder;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public bool Required;

			public string AllowedValues;
		}

		/// SkillGraphNodeRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillGraphNodeRow
		{
			public string MonsterId;

			public SkillNodeOwnerKind OwnerKind;

			public string OwnerId;

			public string TargetSkillId;

			public int NodeOrder;

			public string NodeTypeId;

			public readonly string[] Args = new string[12];

			public string ExcludesActiveChoiceId;
		}

		/// 전달된 record 값을 사용해 SkillNodeTypeRow 값을 런타임 표현으로 파싱한다.
		internal static SkillNodeTypeRow ParseSkillNodeTypeRow(CsvParser.CsvRecord record)
		{
			return new SkillNodeTypeRow
			{
				Id = record.ReadRequiredString("node_type_id"),
				HandlerId = record.ReadRequiredString("handler_id")
			};
		}

		/// 전달된 record 값을 사용해 SkillNodeTypeParamRow 값을 런타임 표현으로 파싱한다.
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

		/// 전달된 record 값을 사용해 SkillGraphNodeRow 값을 런타임 표현으로 파싱한다.
		internal static SkillGraphNodeRow ParseSkillGraphNodeRow(CsvParser.CsvRecord record)
		{
			SkillGraphNodeRow skillGraphNodeRow = new SkillGraphNodeRow
			{
				MonsterId = record.ReadRequiredString("monster_id"),
				OwnerKind = record.ReadEnum<SkillNodeOwnerKind>("owner_kind"),
				OwnerId = record.ReadRequiredString("owner_id"),
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

		/// 전달된 런타임 입력값을 사용해 SkillNodeValueType 값을 런타임 표현으로 파싱한다.
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

		/// 전달된 런타임 입력값을 사용해 NormalizedSkillAuthoringRows를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
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
			}
		}

		/// 전달된 런타임 입력값을 사용해 SkillNodeOwner를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
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

		/// 전달된 런타임 입력값을 사용해 SkillNodeGateReferences를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
		internal static void ValidateSkillNodeGateReferences(SkillNodeRow node, CsvSourceModel.SourceModel model, List<string> errors)
		{
			ValidateChoiceGate(node.Id, "requires_active_choice_id", node.RequiresActiveChoiceId, model, errors);
			ValidateChoiceGate(node.Id, "excludes_active_choice_id", node.ExcludesActiveChoiceId, model, errors);
			ValidatePassiveGate(node.Id, "requires_passive_skill_id", node.RequiresPassiveSkillId, model, errors);
			ValidatePassiveGate(node.Id, "excludes_passive_skill_id", node.ExcludesPassiveSkillId, model, errors);
		}

		/// 전달된 런타임 입력값을 사용해 ChoiceGate를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
		internal static void ValidateChoiceGate(string nodeId, string columnName, string choiceId, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(choiceId) && !model.SkillChoices.ContainsKey(choiceId))
			{
				errors.Add("Skill node '" + nodeId + "' " + columnName + " references unknown choice '" + choiceId + "'.");
			}
		}

		/// 전달된 런타임 입력값을 사용해 PassiveGate를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
		internal static void ValidatePassiveGate(string nodeId, string columnName, string passiveId, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(passiveId) && (!model.Skills.TryGetValue(passiveId, out var value) || value.SkillKind != PakuriCsvSkillKind.Passive))
			{
				errors.Add("Skill node '" + nodeId + "' " + columnName + " references unknown passive '" + passiveId + "'.");
			}
		}

		/// 전달된 런타임 입력값을 사용해 SkillNodeParamValue를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
		internal static void ValidateSkillNodeParamValue(SkillNodeParamRow param, CsvSourceModel.SourceModel model, CsvRuntimeCatalog assetCatalog, List<string> errors)
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
				if (string.IsNullOrWhiteSpace(text) || !model.Skills.ContainsKey(text))
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

		/// 전달된 런타임 입력값을 사용해 SkillNodeEnumParam를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
		internal static void ValidateSkillNodeEnumParam(SkillNodeParamRow param, string value, List<string> errors)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				errors.Add("Skill node param '" + param.NodeId + "." + param.ParamKey + "' requires a non-empty enum value.");
			}
		}

		/// 전달된 model 값을 사용해 MaterializeSkillGraphRows 작업을 수행한다.
		internal static void MaterializeSkillGraphRows(CsvSourceModel.SourceModel model)
		{
			if (model == null || model.SkillGraphNodes.Count == 0)
			{
				return;
			}
			List<string> list = new List<string>();
			Dictionary<string, List<SkillNodeTypeParamRow>> dictionary = BuildSkillNodeTypeParamLookup(model, list);
			ValidateContiguousNodeOrder(model.SkillGraphNodes, list);
			List<SkillNodeRow> list2 = new List<SkillNodeRow>(model.SkillGraphNodes.Count);
			List<SkillNodeParamRow> list3 = new List<SkillNodeParamRow>();
			HashSet<string> hashSet2 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			HashSet<string> hashSet3 = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
				if (!model.Monsters.ContainsKey(skillGraphNodeRow.MonsterId))
				{
					list.Add("Skill graph '" + text + "' references unknown monster '" + skillGraphNodeRow.MonsterId + "'.");
				}
				if (!model.SkillNodeTypes.TryGetValue(skillGraphNodeRow.NodeTypeId, out var value))
				{
					list.Add("Skill graph node '" + text2 + "' references unknown node_type_id '" + skillGraphNodeRow.NodeTypeId + "'.");
					continue;
				}
				string text3 = ResolveSkillGraphTargetSkillId(model, skillGraphNodeRow, list);
				if (!string.IsNullOrWhiteSpace(text3) && model.Skills.TryGetValue(text3, out var value3) && !string.Equals(value3.MonsterId, skillGraphNodeRow.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					list.Add("Skill graph '" + text + "' target skill '" + text3 + "' belongs to '" + value3.MonsterId + "', not '" + skillGraphNodeRow.MonsterId + "'.");
				}
				string text4 = BuildGeneratedSkillGraphNodeId(skillGraphNodeRow);
				if (!hashSet2.Add(text4) || model.SkillNodes.ContainsKey(text4))
				{
					list.Add("Skill graph generated duplicate node id '" + text4 + "'.");
					continue;
				}
				list2.Add(new SkillNodeRow
				{
					Id = text4,
					MonsterId = skillGraphNodeRow.MonsterId,
					OwnerKind = skillGraphNodeRow.OwnerKind,
					OwnerId = skillGraphNodeRow.OwnerId,
					TargetSkillId = text3,
					HandlerId = value.HandlerId,
					SortOrder = skillGraphNodeRow.NodeOrder,
					EnabledByDefault = true,
					RequiresActiveChoiceId = string.Empty,
					ExcludesActiveChoiceId = skillGraphNodeRow.ExcludesActiveChoiceId,
					RequiresPassiveSkillId = string.Empty,
					ExcludesPassiveSkillId = string.Empty
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

		/// 전달된 런타임 입력값을 사용해 SkillNodeTypeParamLookup를 구성한다.
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

		/// 전달된 런타임 입력값을 사용해 SkillGraphTargetSkillId를 결정한다.
		internal static string ResolveSkillGraphTargetSkillId(CsvSourceModel.SourceModel model, SkillGraphNodeRow graph, List<string> errors)
		{
			string text2 = graph?.OwnerId ?? string.Empty;
			string text;
			switch (graph.OwnerKind)
			{
			case SkillNodeOwnerKind.Choice:
			{
				if (!model.SkillChoices.TryGetValue(text2, out var value2))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown choice owner '" + text2 + "'.");
					return graph.TargetSkillId;
				}
				if (!string.Equals(value2.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph choice owner '" + text2 + "' belongs to '" + value2.MonsterId + "', not '" + graph.MonsterId + "'.");
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
				if (!model.Skills.TryGetValue(text2, out var value3))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown skill owner '" + text2 + "'.");
					return graph.TargetSkillId;
				}
				if (!string.Equals(value3.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph skill owner '" + text2 + "' belongs to '" + value3.MonsterId + "', not '" + graph.MonsterId + "'.");
				}
				text = text2;
				break;
			}
			case SkillNodeOwnerKind.Trigger:
			{
				if (!model.SkillTriggers.TryGetValue(text2, out var value))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown trigger owner '" + text2 + "'.");
					return graph.TargetSkillId;
				}
				if (!string.Equals(value.MonsterId, graph.MonsterId, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph trigger owner '" + text2 + "' belongs to '" + value.MonsterId + "', not '" + graph.MonsterId + "'.");
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

		/// 전달된 런타임 입력값을 사용해 SkillGraphAllowedValue를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
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

		/// 전달된 런타임 입력값을 사용해 ContiguousNodeOrder를 검증한다. 발견한 문제는 전달된 오류 컬렉션에 추가한다.
		internal static void ValidateContiguousNodeOrder(
			IReadOnlyList<SkillGraphNodeRow> rows,
			List<string> errors)
		{
			if (rows == null || errors == null)
			{
				return;
			}

			var groups = rows
				.Where(row => row != null)
				.GroupBy(BuildSkillGraphKey, StringComparer.OrdinalIgnoreCase);
			foreach (var group in groups)
			{
				var ordered = group.OrderBy(row => row.NodeOrder).ToArray();
				for (var i = 0; i < ordered.Length; i++)
				{
					var expected = i + 1;
					if (ordered[i].NodeOrder != expected)
					{
						errors.Add(
							$"Skill graph '{group.Key}' requires contiguous node_order values starting at 1; expected '{expected}' but found '{ordered[i].NodeOrder}'.");
						break;
					}
				}
			}
		}

		/// 전달된 graph 값을 사용해 SkillGraphKey를 구성한다.
		internal static string BuildSkillGraphKey(SkillGraphNodeRow graph)
		{
			return graph == null
				? string.Empty
				: $"{graph.MonsterId}:{graph.OwnerKind}:{graph.OwnerId}:{graph.TargetSkillId}";
		}

		/// 전달된 graph 값을 사용해 GeneratedSkillGraphNodeId를 구성한다.
		internal static string BuildGeneratedSkillGraphNodeId(SkillGraphNodeRow graph)
		{
			return graph == null
				? string.Empty
				: $"{graph.OwnerKind}:{graph.OwnerId}:{graph.TargetSkillId}:{graph.NodeOrder}";
		}

	}
}
