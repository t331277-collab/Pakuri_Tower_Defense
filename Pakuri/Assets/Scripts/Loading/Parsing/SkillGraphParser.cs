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

		internal enum SkillNodeOwnerKind
		{
			Skill,
			Choice,
			Passive,
			Base,
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
			SkillName,
			StatusName,
			ChoiceName
		}

		/// SkillNodeRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeRow
		{
			public string Name;

			public string MonsterName;

			public SkillNodeOwnerKind OwnerKind;

			public string OwnerName;

			public string TargetSkillName;

			public string HandlerName;

			public int SortOrder;

			public bool EnabledByDefault;

			public string RequiresActiveChoiceName;

			public string ExcludesActiveChoiceName;

			public string RequiresPassiveSkillName;

			public string ExcludesPassiveSkillName;

		}

		/// SkillNodeParamRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeParamRow
		{
			public string NodeName;

			public string MonsterName;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public string Value;
		}

		/// SkillNodeTypeRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeTypeRow
		{
			public string Name;

			public string HandlerName;

		}

		/// SkillNodeTypeParamRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillNodeTypeParamRow
		{
			public string NodeTypeName;

			public int ParamOrder;

			public string ParamKey;

			public SkillNodeValueType ValueType;

			public bool Required;

			public string AllowedValues;
		}

		/// SkillGraphNodeRow에 해당하는 CSV 한 행을 표현한다.
		internal class SkillGraphNodeRow
		{
			public string MonsterName;

			public SkillNodeOwnerKind OwnerKind;

			public string OwnerName;

			public string TargetSkillName;

			public int NodeOrder;

			public string NodeTypeName;

			public readonly string[] Args = new string[12];

			public string ExcludesActiveChoiceName;
		}

		internal static SkillNodeTypeRow ParseSkillNodeTypeRow(CsvParser.CsvRecord record)
		{
			return new SkillNodeTypeRow
			{
				Name = record.ReadRequiredString("node_type_name"),
				HandlerName = record.ReadRequiredString("handler_name")
			};
		}

		internal static SkillNodeTypeParamRow ParseSkillNodeTypeParamRow(CsvParser.CsvRecord record)
		{
			return new SkillNodeTypeParamRow
			{
				NodeTypeName = record.ReadRequiredString("node_type_name"),
				ParamOrder = record.ReadInt("param_order"),
				ParamKey = record.ReadRequiredString("param_key"),
				ValueType = ParseSkillNodeValueType(record.ReadRequiredString("value_type"), record),
				Required = record.ReadBool("required"),
				AllowedValues = CsvRowParser.ReadOptionalStringIfColumnExists(record, "allowed_values")
			};
		}

		internal static SkillGraphNodeRow ParseSkillGraphNodeRow(CsvParser.CsvRecord record)
		{
			var monsterName = record.ReadRequiredString("monster_name");
			var ownerName = record.ReadRequiredString("owner_name");
			SkillGraphNodeRow skillGraphNodeRow = new SkillGraphNodeRow
			{
				MonsterName = monsterName,
				OwnerKind = record.ReadEnum<SkillNodeOwnerKind>("owner_kind"),
				OwnerName = NormalizeOwnerName(monsterName, ownerName),
				TargetSkillName = CsvRowParser.ReadOptionalStringIfColumnExists(record, "target_skill_name"),
				NodeOrder = record.ReadInt("node_order"),
				NodeTypeName = record.ReadRequiredString("node_type_name"),
				ExcludesActiveChoiceName = CsvRowParser.ReadOptionalStringIfColumnExists(record, "excludes_active_choice_name")
			};
			for (int i = 0; i < skillGraphNodeRow.Args.Length; i++)
			{
				skillGraphNodeRow.Args[i] = CsvRowParser.ReadOptionalStringIfColumnExists(record, $"arg_{i + 1}");
			}
			return skillGraphNodeRow;
		}

		/// 그래프 CSV의 짧은 owner_name를 저장소의 전역 Name로 복원한다.
		internal static string NormalizeOwnerName(string monsterName, string ownerName)
		{
			if (string.IsNullOrWhiteSpace(monsterName) || string.IsNullOrWhiteSpace(ownerName))
			{
				return ownerName;
			}

			var normalizedMonsterName = monsterName.Trim();
			var normalizedOwnerName = ownerName.Trim();
			var prefix = normalizedMonsterName + "-";
			return normalizedOwnerName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
				? normalizedOwnerName
				: prefix + normalizedOwnerName;
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
				"skill_name" => SkillNodeValueType.SkillName,
				"status_name" => SkillNodeValueType.StatusName,
				"choice_name" => SkillNodeValueType.ChoiceName,
				_ => throw new CsvParser.CsvFatalException($"CSV row {record.RowNumber} in '{record.TableName}' has unsupported value_type '{rawValue}'."),
			};
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
				if (!model.SkillNodes.ContainsKey(skillNodeParam.NodeName))
				{
					errors.Add("Skill node param '" + skillNodeParam.ParamKey + "' references unknown node_name '" + skillNodeParam.NodeName + "'.");
					continue;
				}
				if (!dictionary.TryGetValue(skillNodeParam.NodeName, out var value))
				{
					value = new List<SkillNodeParamRow>();
					dictionary.Add(skillNodeParam.NodeName, value);
					dictionary2.Add(skillNodeParam.NodeName, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
				}
				if (!dictionary2[skillNodeParam.NodeName].Add(skillNodeParam.ParamKey))
				{
					errors.Add("Skill node '" + skillNodeParam.NodeName + "' has duplicate param '" + skillNodeParam.ParamKey + "'.");
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

		internal static void ValidateSkillNodeOwner(SkillNodeRow node, CsvSourceModel.SourceModel model, List<string> errors)
		{
			switch (node.OwnerKind)
			{
			case SkillNodeOwnerKind.Skill:
				if (!model.Skills.ContainsKey(node.OwnerName)
					&& !model.SummonSkills.ContainsKey(node.OwnerName))
				{
					errors.Add("Skill node '" + node.Name + "' references unknown owner skill '" + node.OwnerName + "'.");
				}
				break;
			case SkillNodeOwnerKind.Passive:
			{
				if (!model.Skills.TryGetValue(node.OwnerName, out var value) || value.SkillKind != PakuriCsvSkillKind.Passive)
				{
					errors.Add("Skill node '" + node.Name + "' references unknown passive owner '" + node.OwnerName + "'.");
				}
				break;
			}
			case SkillNodeOwnerKind.Base:
				if (!model.SkillTriggers.TryGetValue(node.OwnerName, out var baseTrigger)
					|| !string.IsNullOrWhiteSpace(baseTrigger.RequiresActiveChoiceName)
					|| !model.Skills.TryGetValue(baseTrigger.SourceSkillName, out var baseSkill)
					|| baseSkill.SkillKind != PakuriCsvSkillKind.Passive)
				{
					errors.Add("Skill node '" + node.Name + "' references unknown passive Base owner '" + node.OwnerName + "'.");
				}
				break;
			case SkillNodeOwnerKind.Choice:
				if (!model.SkillChoices.ContainsKey(node.OwnerName))
				{
					errors.Add("Skill node '" + node.Name + "' references unknown choice owner '" + node.OwnerName + "'.");
				}
				break;
			case SkillNodeOwnerKind.Effect:
				if (!IsArtifactEffectOwner(model, node.OwnerName, node.MonsterName))
				{
					errors.Add("Skill node '" + node.Name + "' references unknown artifact effect owner '" + node.OwnerName + "'.");
				}
				break;
			case SkillNodeOwnerKind.Trigger:
				if (!model.SkillTriggers.ContainsKey(node.OwnerName))
				{
					errors.Add("Skill node '" + node.Name + "' references unknown trigger owner '" + node.OwnerName + "'.");
				}
				break;
			default:
				errors.Add($"Skill node '{node.Name}' uses unsupported owner_kind '{node.OwnerKind}'.");
				break;
			}
			if (!string.IsNullOrWhiteSpace(node.TargetSkillName)
				&& !model.Skills.ContainsKey(node.TargetSkillName)
				&& !model.SummonSkills.ContainsKey(node.TargetSkillName))
			{
				errors.Add("Skill node '" + node.Name + "' references unknown target_skill_name '" + node.TargetSkillName + "'.");
			}
		}

		internal static void ValidateSkillNodeGateReferences(SkillNodeRow node, CsvSourceModel.SourceModel model, List<string> errors)
		{
			ValidateChoiceGate(node.Name, "requires_active_choice_name", node.RequiresActiveChoiceName, model, errors);
			ValidateChoiceGate(node.Name, "excludes_active_choice_name", node.ExcludesActiveChoiceName, model, errors);
			ValidatePassiveGate(node.Name, "requires_passive_skill_name", node.RequiresPassiveSkillName, model, errors);
			ValidatePassiveGate(node.Name, "excludes_passive_skill_name", node.ExcludesPassiveSkillName, model, errors);
		}

		internal static void ValidateChoiceGate(string nodeName, string columnName, string choiceName, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(choiceName) && !model.SkillChoices.ContainsKey(choiceName))
			{
				errors.Add("Skill node '" + nodeName + "' " + columnName + " references unknown choice '" + choiceName + "'.");
			}
		}

		internal static void ValidatePassiveGate(string nodeName, string columnName, string passiveName, CsvSourceModel.SourceModel model, List<string> errors)
		{
			if (!string.IsNullOrWhiteSpace(passiveName) && (!model.Skills.TryGetValue(passiveName, out var value) || value.SkillKind != PakuriCsvSkillKind.Passive))
			{
				errors.Add("Skill node '" + nodeName + "' " + columnName + " references unknown passive '" + passiveName + "'.");
			}
		}

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
					CsvDataValidator.ValidateStatusIdList(param.NodeName, text, model, errors);
				}
				break;
			case SkillNodeValueType.Int:
			{
				if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var _))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' value '" + param.Value + "' is not a valid int.");
				}
				break;
			}
			case SkillNodeValueType.Float:
			{
				if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var _))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' value '" + param.Value + "' is not a valid float.");
				}
				break;
			}
			case SkillNodeValueType.Bool:
			{
				if (!bool.TryParse(text, out var _))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' value '" + param.Value + "' is not a valid bool.");
				}
				break;
			}
			case SkillNodeValueType.Enum:
				ValidateSkillNodeEnumParam(param, text, errors);
				break;
			case SkillNodeValueType.AssetPath:
				if (string.IsNullOrWhiteSpace(text) || assetCatalog == null || (!assetCatalog.HasSprite(text) && !assetCatalog.HasPrefab(text) && !assetCatalog.HasAnimatorController(text)))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' references unknown asset path '" + param.Value + "'.");
				}
				break;
			case SkillNodeValueType.SkillName:
				if (string.IsNullOrWhiteSpace(text)
					|| (!model.Skills.ContainsKey(text) && !model.SummonSkills.ContainsKey(text)))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' references unknown skill '" + param.Value + "'.");
				}
				break;
			case SkillNodeValueType.StatusName:
			{
				if (string.IsNullOrWhiteSpace(text) || !model.StatusEffects.ContainsKey(text))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' references unknown status '" + param.Value + "'.");
				}
				break;
			}
			case SkillNodeValueType.ChoiceName:
				if (string.IsNullOrWhiteSpace(text) || !model.SkillChoices.ContainsKey(text))
				{
					errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' references unknown choice '" + param.Value + "'.");
				}
				break;
			default:
				errors.Add($"Skill node param '{param.NodeName}.{param.ParamKey}' has unsupported value_type '{param.ValueType}'.");
				break;
			}
		}

		internal static void ValidateSkillNodeEnumParam(SkillNodeParamRow param, string value, List<string> errors)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				errors.Add("Skill node param '" + param.NodeName + "." + param.ParamKey + "' requires a non-empty enum value.");
			}
		}

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
				if (!model.Monsters.ContainsKey(skillGraphNodeRow.MonsterName)
					&& !model.Summons.ContainsKey(skillGraphNodeRow.MonsterName)
					&& !IsArtifactGraphOwner(model, skillGraphNodeRow))
				{
					list.Add("Skill graph '" + text + "' references unknown monster '" + skillGraphNodeRow.MonsterName + "'.");
				}
				if (!model.SkillNodeTypes.TryGetValue(skillGraphNodeRow.NodeTypeName, out var value))
				{
					list.Add("Skill graph node '" + text2 + "' references unknown node_type_name '" + skillGraphNodeRow.NodeTypeName + "'.");
					continue;
				}
				string text3 = ResolveSkillGraphTargetSkillName(model, skillGraphNodeRow, list);
				if (!IsArtifactGraphOwner(model, skillGraphNodeRow)
					&& !string.IsNullOrWhiteSpace(text3)
					&& TryGetSkill(model, text3, out var value3)
					&& !string.Equals(value3.MonsterName, skillGraphNodeRow.MonsterName, StringComparison.OrdinalIgnoreCase))
				{
					list.Add("Skill graph '" + text + "' target skill '" + text3 + "' belongs to '" + value3.MonsterName + "', not '" + skillGraphNodeRow.MonsterName + "'.");
				}
				string text4 = BuildGeneratedSkillGraphNodeName(skillGraphNodeRow);
				if (!hashSet2.Add(text4) || model.SkillNodes.ContainsKey(text4))
				{
					list.Add("Skill graph generated duplicate node Name '" + text4 + "'.");
					continue;
				}
				list2.Add(new SkillNodeRow
				{
					Name = text4,
					MonsterName = skillGraphNodeRow.MonsterName,
					OwnerKind = skillGraphNodeRow.OwnerKind,
					OwnerName = skillGraphNodeRow.OwnerName,
					TargetSkillName = text3,
					HandlerName = value.HandlerName,
					SortOrder = skillGraphNodeRow.NodeOrder,
					EnabledByDefault = true,
					RequiresActiveChoiceName = string.Empty,
					ExcludesActiveChoiceName = skillGraphNodeRow.ExcludesActiveChoiceName,
					RequiresPassiveSkillName = string.Empty,
					ExcludesPassiveSkillName = string.Empty
				});
				dictionary.TryGetValue(skillGraphNodeRow.NodeTypeName, out var value5);
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
						NodeName = text4,
						MonsterName = skillGraphNodeRow.MonsterName,
						ParamKey = skillNodeTypeParamRow.ParamKey,
						ValueType = skillNodeTypeParamRow.ValueType,
						Value = value6
					});
				}
				for (int l = 0; l < skillGraphNodeRow.Args.Length; l++)
				{
					if (!string.IsNullOrWhiteSpace(skillGraphNodeRow.Args[l]) && !hashSet4.Contains(l + 1))
					{
						list.Add($"Skill graph node '{text2}' sets arg_{l + 1}, but node type '{skillGraphNodeRow.NodeTypeName}' has no matching param definition.");
					}
				}
			}
			if (list.Count > 0)
			{
				throw new CsvParser.CsvFatalException("Skill graph authoring materialization failed.", list);
			}
			for (int n = 0; n < list2.Count; n++)
			{
				model.SkillNodes.Add(list2[n].Name, list2[n]);
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
				if (!model.SkillNodeTypes.ContainsKey(skillNodeTypeParamRow.NodeTypeName))
				{
					errors.Add("Skill node type param '" + skillNodeTypeParamRow.NodeTypeName + "." + skillNodeTypeParamRow.ParamKey + "' references unknown node_type_name.");
					continue;
				}
				if (skillNodeTypeParamRow.ParamOrder < 1 || skillNodeTypeParamRow.ParamOrder > 12)
				{
					errors.Add("Skill node type param '" + skillNodeTypeParamRow.NodeTypeName + "." + skillNodeTypeParamRow.ParamKey + "' requires param_order between 1 and 12.");
					continue;
				}
				string item = $"{skillNodeTypeParamRow.NodeTypeName}:{skillNodeTypeParamRow.ParamOrder}";
				string item2 = skillNodeTypeParamRow.NodeTypeName + ":" + skillNodeTypeParamRow.ParamKey;
				if (!hashSet.Add(item) || !hashSet.Add(item2))
				{
					errors.Add("Skill node type '" + skillNodeTypeParamRow.NodeTypeName + "' has duplicate param order or key for '" + skillNodeTypeParamRow.ParamKey + "'.");
				}
				else
				{
					if (!dictionary.TryGetValue(skillNodeTypeParamRow.NodeTypeName, out var value))
					{
						value = new List<SkillNodeTypeParamRow>();
						dictionary.Add(skillNodeTypeParamRow.NodeTypeName, value);
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

		internal static string ResolveSkillGraphTargetSkillName(CsvSourceModel.SourceModel model, SkillGraphNodeRow graph, List<string> errors)
		{
			string text2 = graph?.OwnerName ?? string.Empty;
			string text;
			switch (graph.OwnerKind)
			{
			case SkillNodeOwnerKind.Choice:
			{
				if (!model.SkillChoices.TryGetValue(text2, out var value2))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown choice owner '" + text2 + "'.");
					return graph.TargetSkillName;
				}
				if (!string.Equals(value2.MonsterName, graph.MonsterName, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph choice owner '" + text2 + "' belongs to '" + value2.MonsterName + "', not '" + graph.MonsterName + "'.");
				}
				text = value2.TargetSkillName;
				if (string.IsNullOrWhiteSpace(text))
				{
					text = value2.SkillName;
				}
				break;
			}
			case SkillNodeOwnerKind.Skill:
			{
				if (!TryGetSkill(model, text2, out var value3))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown skill owner '" + text2 + "'.");
					return graph.TargetSkillName;
				}
				if (!string.Equals(value3.MonsterName, graph.MonsterName, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph skill owner '" + text2 + "' belongs to '" + value3.MonsterName + "', not '" + graph.MonsterName + "'.");
				}
				text = text2;
				break;
			}
			case SkillNodeOwnerKind.Trigger:
			{
				if (!model.SkillTriggers.TryGetValue(text2, out var value))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown trigger owner '" + text2 + "'.");
					return graph.TargetSkillName;
				}
				if (!string.Equals(value.MonsterName, graph.MonsterName, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph trigger owner '" + text2 + "' belongs to '" + value.MonsterName + "', not '" + graph.MonsterName + "'.");
				}
				text = value.SourceSkillName;
				if (IsArtifactEffectOwner(model, text, graph.MonsterName))
				{
					text = graph.TargetSkillName;
					break;
				}
				break;
			}
			case SkillNodeOwnerKind.Base:
			{
				if (!model.SkillTriggers.TryGetValue(text2, out var baseTrigger))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown Base owner '" + text2 + "'.");
					return graph.TargetSkillName;
				}
				if (!string.Equals(baseTrigger.MonsterName, graph.MonsterName, StringComparison.OrdinalIgnoreCase))
				{
					errors.Add("Skill graph Base owner '" + text2 + "' belongs to '" + baseTrigger.MonsterName + "', not '" + graph.MonsterName + "'.");
				}
				text = baseTrigger.SourceSkillName;
				break;
			}
			case SkillNodeOwnerKind.Effect:
				if (!IsArtifactEffectOwner(model, text2, graph.MonsterName))
				{
					errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' references unknown artifact effect owner '" + text2 + "'.");
				}
				text = graph.TargetSkillName;
				break;
			default:
				errors.Add($"Skill graph '{BuildSkillGraphKey(graph)}' uses unsupported owner_kind '{graph.OwnerKind}'.");
				return graph.TargetSkillName;
			}
			if (!string.IsNullOrWhiteSpace(graph.TargetSkillName))
			{
				text = graph.TargetSkillName;
			}
			if (!string.IsNullOrWhiteSpace(text) && !TryGetSkill(model, text, out _))
			{
				errors.Add("Skill graph '" + BuildSkillGraphKey(graph) + "' resolves unknown target_skill_name '" + text + "'.");
			}
			return text;
		}

		private static bool IsArtifactGraphOwner(
			CsvSourceModel.SourceModel model,
			SkillGraphNodeRow graph)
		{
			if (model == null || graph == null)
			{
				return false;
			}

			if (graph.OwnerKind == SkillNodeOwnerKind.Effect)
			{
				return IsArtifactEffectOwner(model, graph.OwnerName, graph.MonsterName);
			}

			return graph.OwnerKind == SkillNodeOwnerKind.Trigger
				&& model.SkillTriggers.TryGetValue(graph.OwnerName, out var trigger)
				&& IsArtifactEffectOwner(model, trigger.SourceSkillName, graph.MonsterName);
		}

		private static bool TryGetSkill(
			CsvSourceModel.SourceModel model,
			string skillName,
			out CsvRowParser.SkillRow skill)
		{
			if (model.Skills.TryGetValue(skillName ?? string.Empty, out skill))
			{
				return true;
			}

			return model.SummonSkills.TryGetValue(skillName ?? string.Empty, out skill);
		}

		internal static bool IsArtifactEffectOwner(
			CsvSourceModel.SourceModel model,
			string effectName,
			string artifactName)
		{
			return model != null
				&& model.ArtifactEffects.TryGetValue(effectName ?? string.Empty, out var effect)
				&& string.Equals(effect.ArtifactName, artifactName, StringComparison.OrdinalIgnoreCase);
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

		internal static string BuildSkillGraphKey(SkillGraphNodeRow graph)
		{
			return graph == null
				? string.Empty
				: $"{graph.MonsterName}:{graph.OwnerKind}:{graph.OwnerName}:{graph.TargetSkillName}";
		}

		internal static string BuildGeneratedSkillGraphNodeName(SkillGraphNodeRow graph)
		{
			return graph == null
				? string.Empty
				: $"{graph.OwnerKind}:{graph.OwnerName}:{graph.TargetSkillName}:{graph.NodeOrder}";
		}

	}
}
