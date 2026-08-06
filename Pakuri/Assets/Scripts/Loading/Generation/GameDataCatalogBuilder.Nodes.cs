/*
 * 역할: 스킬 Node 런타임 변환.
 * 책임: 파싱된 스킬 그래프 Node 행을 실행 가능한 조건·배율·행동 작업으로 변환한다.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;
using static Pakuri.Data.CsvRowParser;

namespace Pakuri.Data
{

    internal sealed class SkillNodeParamBuildData
    {
        public string ParamKey;
        public string Value;
    }

    internal sealed class SkillNodeBuildData
    {
        public string OwnerKind;
        public string TargetSkillName;
        public string HandlerName;
        public bool EnabledByDefault;
        public SkillNodeParamBuildData[] Params = Array.Empty<SkillNodeParamBuildData>();
        public GameObject ResolvedPrefab;
        public RuntimeSkillVisualSpec ResolvedRuntimeVisual;
    }

    /// GameDataCatalogBuilder 런타임 데이터를 파싱된 저작 데이터에서 생성한다.
    internal sealed partial class GameDataCatalogBuilder
    {

	public static SkillNode[] MapSkillNodes(SkillNodeBuildData[] source)
	{
		if (source == null || source.Length == 0)
		{
			return Array.Empty<SkillNode>();
		}
		List<SkillNode> list = new List<SkillNode>(source.Length);
		for (int i = 0; i < source.Length; i++)
		{
			SkillNode skillExecutionNode = MapSkillNode(source[i]);
			if (skillExecutionNode != null)
			{
				skillExecutionNode.TargetSkillName = source[i].TargetSkillName ?? string.Empty;
				list.Add(skillExecutionNode);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<SkillNode>();
	}

	private static void BuildReactionOutcome(
		SkillReaction reaction,
		SkillNodeBuildData[] nodes,
		StatusEffectDefinition[] statusDefinitions,
		SkillDefinition[] activeSkills,
		PassiveSkillDefinition[] passiveSkills = null)
	{
		if (reaction == null || nodes == null)
		{
			return;
		}

		var state = BuildReactionOutcomeState(nodes, out var outcomeCount);
		if (outcomeCount > 1)
		{
			throw new InvalidOperationException(
				"Reaction has more than one runtime outcome: " + reaction.ReactionName);
		}

		var targeting = BuildTriggerTargeting(state);
		reaction.LockToEventTarget =
			state.TargetSelection == SkillMultiEffectTargetSelection.EventTarget;
		reaction.CenterMode = state.CenterMode == SkillMultiEffectCenterMode.Caster
			? SkillTriggerCenterMode.Caster
			: state.CenterMode == SkillMultiEffectCenterMode.EffectTarget
				? SkillTriggerCenterMode.EventTarget
				: SkillTriggerCenterMode.EventCenter;

		for (var i = 0; i < nodes.Length; i++)
		{
			var node = nodes[i];
			if (node == null || !node.EnabledByDefault)
			{
				continue;
			}

			var handler = node.HandlerName ?? string.Empty;
			if (string.Equals(handler, "EffectDamage", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "ApplyDamage", StringComparison.OrdinalIgnoreCase))
			{
				BuildReactionDamage(reaction, node, state, targeting);
				return;
			}
			if (string.Equals(handler, "ApplyStatus", StringComparison.OrdinalIgnoreCase))
			{
				BuildReactionStatus(
					reaction,
					node,
					state,
					targeting,
					statusDefinitions);
				return;
			}
			if (string.Equals(handler, "ApplyShield", StringComparison.OrdinalIgnoreCase))
			{
				BuildReactionShield(
					reaction,
					node,
					state,
					targeting,
					statusDefinitions);
				return;
			}
			if (string.Equals(handler, "ExecuteSkill", StringComparison.OrdinalIgnoreCase))
			{
				var targetSkillName = GetParam(node, "skill_name");
				reaction.DamageMultiplier = Mathf.Max(
					0f,
					GetFloatParam(node, "damage_multiplier", 1f));
				reaction.PublishSkillLifecycleEvents = true;
				reaction.Effect = new SkillCastEffect
				{
					EffectName = reaction.ReactionName,
					DamageMultiplier = reaction.DamageMultiplier
				};
				reaction.Effect.ResolvedDefinition = FindSkillDefinition(
					activeSkills,
					passiveSkills,
					targetSkillName);
				if (reaction.Effect.ResolvedDefinition == null)
				{
					throw new InvalidOperationException(
						"Triggered skill is not registered: " + targetSkillName);
				}
				return;
			}
			if (string.Equals(handler, "RecastZone", StringComparison.OrdinalIgnoreCase))
			{
				reaction.DelaySeconds += Mathf.Max(
					0f,
					GetFloatParam(node, "delay_seconds", 0f));
				var targetSkillName = GetParam(node, "source_skill_name");
				reaction.Effect = new SkillCastEffect
				{
					EffectName = reaction.ReactionName,
					IsRecast = true,
					DurationSeconds = Mathf.Max(0f, GetFloatParam(node, "duration_seconds", 0f)),
					RadiusMultiplier = Mathf.Max(0f, GetFloatParam(node, "radius_multiplier", 1f)),
					InheritSnapshot = GetBoolParam(node, "inherit_snapshot", true),
					MaxGeneration = Mathf.Max(1, GetIntParam(node, "max_generation", 1)),
				};
				reaction.Effect.ResolvedDefinition = FindSkillDefinition(
					activeSkills,
					passiveSkills,
					targetSkillName);
				if (!(reaction.Effect.ResolvedDefinition is ZoneSkillDefinition))
				{
					throw new InvalidOperationException(
						"Recast zone is not registered: " + targetSkillName);
				}
				return;
			}
			if (string.Equals(handler, "RefundCooldown", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "ReduceReload", StringComparison.OrdinalIgnoreCase))
			{
				reaction.Command = new SkillReactionCommand
				{
					Kind = string.Equals(handler, "RefundCooldown", StringComparison.OrdinalIgnoreCase)
						? SkillReactionCommandKind.RefundCooldown
						: SkillReactionCommandKind.ReduceReload,
					TargetName = GetParam(node, "skill_name"),
					Ratio = Mathf.Clamp01(GetFloatParam(node, "ratio", 0f)),
					Targeting = targeting,
					LockToEventTarget = reaction.LockToEventTarget,
					MaxTargets = state.MaxTargets
				};
				return;
			}
			if (string.Equals(handler, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "ExtendStatusDuration", StringComparison.OrdinalIgnoreCase))
			{
				reaction.Command = new SkillReactionCommand
				{
					Kind = SkillReactionCommandKind.ExtendStatusDuration,
					StatusKind = StatusValueParser.ParseStatusKind(
						GetParam(node, "status_name")),
					DurationSeconds = state.DurationSeconds,
					Targeting = targeting,
					LockToEventTarget = reaction.LockToEventTarget,
					MaxTargets = state.MaxTargets
				};
				return;
			}
		}
	}

	private static ReactionOutcomeBuildState BuildReactionOutcomeState(
		SkillNodeBuildData[] nodes,
		out int outcomeCount)
	{
		var state = new ReactionOutcomeBuildState();
		outcomeCount = 0;
		for (var i = 0; i < nodes.Length; i++)
		{
			var node = nodes[i];
			if (node == null || !node.EnabledByDefault)
			{
				continue;
			}

			var handler = node.HandlerName ?? string.Empty;
			if (IsTriggerOutcomeHandler(handler))
			{
				outcomeCount++;
			}
			if (string.Equals(handler, "EffectTarget", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "SelectTargets", StringComparison.OrdinalIgnoreCase))
			{
				state.TargetSide = GetEnumParam(
					node,
					"target_side",
					SkillMultiEffectTargetSide.Enemy);
				state.TargetSelection = GetEnumParam(
					node,
					"target_selection",
					SkillMultiEffectTargetSelection.Nearest);
				state.TargetShape = GetEnumParam(
					node,
					"target_shape",
					SkillMultiEffectTargetShape.Single);
				state.CenterMode = GetEnumParam(
					node,
					"center_mode",
					SkillMultiEffectCenterMode.PrimarySkillCenter);
				state.CoverAll = GetBoolParam(node, "cover_all", false);
				state.MaxTargets = GetIntParam(node, "max_targets", 0);
				continue;
			}
			if (string.Equals(handler, "EffectLifetime", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "SetDuration", StringComparison.OrdinalIgnoreCase))
			{
				state.DurationSeconds = Mathf.Max(
					0f,
					GetFloatParam(node, "duration_seconds", 0f));
				continue;
			}
			if (string.Equals(handler, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase))
			{
				state.StatusKind = StatusValueParser.ParseStatusKind(
					GetParam(node, "status_name"));
				state.StatusChance = Mathf.Clamp01(
					GetFloatParam(node, "status_chance", 1f));
				state.StatusStacks = Mathf.Max(
					1,
					GetIntParam(node, "status_stack_amount", 1));
				state.StatusDurationSeconds = Mathf.Max(
					0f,
					GetFloatParam(node, "status_duration_seconds", 0f));
				state.StatusMaxStacks = Mathf.Max(
					1,
					GetIntParam(node, "status_max_stacks", 1));
				state.RefreshDuration = !string.Equals(
					GetParam(node, "status_merge_policy"),
					"StackDuration",
					StringComparison.OrdinalIgnoreCase);
				state.HasStatusPayload = true;
				continue;
			}
			if (string.Equals(handler, "ConditionStatus", StringComparison.OrdinalIgnoreCase))
			{
				if (StatusValueParser.TryParseStatusKind(
					GetParam(node, "status_name"),
					out var selectionStatusKind))
				{
					state.SelectionStatusKind = selectionStatusKind;
					state.SelectionStatusMinStacks = Mathf.Max(
						1,
						GetIntParam(node, "min_stacks", 1));
				}
				continue;
			}
			if (string.Equals(handler, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase))
			{
				state.HasSelectionSkillAttribute = true;
				state.SelectionSkillAttribute = GetEnumParam(
					node,
					"attribute",
					DamageAttribute.Physical);
				continue;
			}
			if (string.Equals(handler, "EffectVisual", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "ShowVisual", StringComparison.OrdinalIgnoreCase))
			{
				state.Prefab = node.ResolvedPrefab;
				state.RuntimeVisual = node.ResolvedRuntimeVisual;
				continue;
			}
			if (TryMapStatusMutation(node, handler, out var mutation))
			{
				state.StatusMutations.Add(mutation);
			}
		}
		return state;
	}

	/// Trigger authoring에 남아 있는 일반 시전 효과를 기존 SkillNode 효과로 변환한다.
	private static SkillNode BuildNormalCastEffectNode(
		SkillTriggerRow row,
		SkillNodeBuildData[] nodes,
		StatusEffectDefinition[] statusDefinitions,
		SkillDefinition[] activeSkills,
		PassiveSkillDefinition[] passiveSkills)
	{
		var state = BuildReactionOutcomeState(nodes, out _);
		var reaction = new SkillReaction
		{
			ReactionName = row.Name,
			SourceSkillName = row.SourceSkillName
		};
		BuildReactionOutcome(
			reaction,
			nodes,
			statusDefinitions,
			activeSkills,
			passiveSkills);

		var effect = reaction.Effect;
		if (effect == null
			&& reaction.Command?.Kind
				== SkillReactionCommandKind.ExtendStatusDuration)
		{
			effect = new SkillCastEffect
			{
				EffectName = row.Name,
				Command = reaction.Command
			};
		}
		if (effect == null && HasHandler(nodes, "StatusModifier"))
		{
			effect = BuildNormalStatusModifierEffect(
				row,
				nodes,
				statusDefinitions);
		}
		if (effect != null)
		{
			effect.DelaySeconds = Mathf.Max(0f, row.TriggerDelaySeconds);
			effect.UseSourcePreparedAim = effect.ResolvedDefinition != null
				&& string.Equals(
					effect.ResolvedDefinition.SkillName,
					row.SourceSkillName,
					StringComparison.OrdinalIgnoreCase);
			effect.UseSourcePreparedCenter = effect.ResolvedDefinition is SingleSkillDefinition
				&& state.CenterMode
					== SkillMultiEffectCenterMode.PrimarySkillCenter;
		}

		return effect != null
			? SkillNode.FromOperation(
				new SkillCastEffectOp(effect),
				row.SourceSkillName)
			: null;
	}

	private static SkillCastEffect BuildNormalStatusModifierEffect(
		SkillTriggerRow row,
		SkillNodeBuildData[] nodes,
		StatusEffectDefinition[] statusDefinitions)
	{
		var state = BuildReactionOutcomeState(nodes, out _);
		var status = GetStatusRuntimeData(
			StatusEffectKind.PassiveBuff,
			statusDefinitions);
		status.SourceSkillName = row.Name;
		status.Duration = state.DurationSeconds > 0f
			? state.DurationSeconds
			: status.Duration;
		status.Permanent = status.Duration <= 0f || status.Duration >= 9999f;
		ApplyTriggeredStatusMutations(status, state.StatusMutations);

		for (var i = 0; i < nodes.Length; i++)
		{
			var node = nodes[i];
			if (node == null || !node.EnabledByDefault)
			{
				continue;
			}

			var handler = node.HandlerName ?? string.Empty;
			if (string.Equals(handler, "StatusModifier", StringComparison.OrdinalIgnoreCase))
			{
				var scope = GetParam(node, "status_target_scope");
				if (!string.IsNullOrWhiteSpace(scope))
				{
					status.TargetScope = StatusValueParser.ParseTargetScope(scope);
				}
				var merge = GetParam(node, "status_merge_policy");
				status.MergePolicy = string.IsNullOrWhiteSpace(merge)
					? StatusMergePolicy.SameSourceRefresh
					: StatusValueParser.ParseMergePolicy(merge);
			}
			else if (string.Equals(handler, "ConditionStatus", StringComparison.OrdinalIgnoreCase)
				|| string.Equals(handler, "ConditionStatusExpression", StringComparison.OrdinalIgnoreCase))
			{
				status.ConditionalTargetStatusGroups =
					StatusValueParser.ParseConditionStatusExpression(
						GetParam(node, "status_name"));
				var minimumStacks = Mathf.Max(1, GetIntParam(node, "min_stacks", 1));
				for (var groupIndex = 0;
					groupIndex < status.ConditionalTargetStatusGroups.Length;
					groupIndex++)
				{
					var requirements =
						status.ConditionalTargetStatusGroups[groupIndex].Requirements;
					for (var requirementIndex = 0;
						requirementIndex < requirements.Length;
						requirementIndex++)
					{
						requirements[requirementIndex].MinStacks = Mathf.Max(
							requirements[requirementIndex].MinStacks,
							minimumStacks);
					}
				}
				status.ConditionalTargetStatusSourceSkillNames =
					StatusValueParser.ParseIdList(GetParam(node, "source_skill_name"));
				status.ConditionalTargetSide = GetEnumParam(
					node,
					"target_side",
					SkillTargetSide.Self);
			}
			else if (string.Equals(handler, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase))
			{
				var kinds = StatusValueParser.ParseStatusKinds(
					GetParam(node, "status_ids"));
				var groups = new StatusConditionGroup[kinds.Length];
				for (var kindIndex = 0; kindIndex < kinds.Length; kindIndex++)
				{
					groups[kindIndex] = new StatusConditionGroup
					{
						Requirements = new[]
						{
							new StatusConditionRequirement
							{
								Kind = kinds[kindIndex],
								MinStacks = Mathf.Max(
									1,
									GetIntParam(node, "min_stacks", 1))
							}
						}
					};
				}
				status.ConditionalTargetStatusGroups = groups;
				status.ConditionalTargetStatusSourceSkillNames =
					StatusValueParser.ParseIdList(GetParam(node, "source_skill_name"));
				status.ConditionalTargetSide = GetEnumParam(
					node,
					"target_side",
					SkillTargetSide.Self);
			}
			else if (string.Equals(handler, "ConditionHealthRatioMax", StringComparison.OrdinalIgnoreCase))
			{
				status.ConditionalTargetHealthRatioMax = Mathf.Clamp01(
					GetFloatParam(node, "ratio", 0f));
			}
			else if (string.Equals(handler, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
			{
				status.ConditionalSourceStatusKind =
					StatusValueParser.ParseStatusKind(GetParam(node, "status_name"));
			}
		}

		var targeting = BuildTriggerTargeting(state);
		if (status.ConditionalTargetStatusGroups.Length > 0
			|| status.ConditionalTargetStatusKinds.Length > 0
			|| status.ConditionalTargetHealthRatioMax > 0f)
		{
			targeting.SelectionStatusKind = StatusEffectKind.None;
			targeting.SelectionStatusMinStacks = 0;
		}

		return new SkillCastEffect
		{
			EffectName = row.Name,
			DelaySeconds = Mathf.Max(0f, row.TriggerDelaySeconds),
			ResolvedDefinition = new BuffSkillDefinition
			{
				SkillName = row.Name,
				DisplayName = row.Name,
				RuntimeKind = SkillRuntimeKind.Buff,
				ImplementationState = SkillImplementationState.RuntimeImplemented,
				Targeting = targeting,
				Target = targeting.TargetSide,
				UseConfiguredTargeting = true,
				EffectKind = BuffEffectKind.Status,
				AttachedStatus = new StatusApplicationSpec
				{
					Status = status,
					Chance = 1f,
					Stacks = 1,
					RefreshDuration = true
				},
				SkillEffectPrefab = status.StatusEffectPrefab,
				RuntimeVisual = status.RuntimeVisual
			}
		};
	}

	private static bool HasHandler(
		SkillNodeBuildData[] nodes,
		string handlerName)
	{
		for (var i = 0; nodes != null && i < nodes.Length; i++)
		{
			if (nodes[i] != null
				&& nodes[i].EnabledByDefault
				&& string.Equals(
					nodes[i].HandlerName,
					handlerName,
					StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	internal static bool IsTriggerOutcomeHandler(string handler)
	{
		return string.Equals(handler, "EffectDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "ApplyDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "ApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "ApplyShield", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "ExecuteSkill", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "RecastZone", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "RefundCooldown", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "ReduceReload", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handler, "ExtendStatusDuration", StringComparison.OrdinalIgnoreCase);
	}

	private static void BuildReactionDamage(
		SkillReaction reaction,
		SkillNodeBuildData node,
		ReactionOutcomeBuildState state,
		SkillTargetingSpec targeting)
	{
		var multiplier = Mathf.Max(
			0f,
			GetFloatParam(node, "damage_multiplier", 1f));
		var shape = targeting.Shape;
		var maxTargets = shape == SkillTargetShape.Single
			? 1
			: state.MaxTargets;
		var area = new AreaBlueprintSpec
		{
			Radius = Mathf.Max(0f, GetFloatParam(node, "radius", 0f)),
			Duration = state.DurationSeconds,
			CoverAll = shape != SkillTargetShape.Single || state.CoverAll
		};
		targeting.CoverAll = area.CoverAll;
		var damage = new SkillDamageSpec
			{
				SkillName = reaction.SourceSkillName,
				Element = GetEnumParam(
					node,
					"attribute",
					DamageAttribute.Physical),
				BaseDamage = GetFloatParam(node, "base_damage", 0f) * multiplier,
				AttackPowerCoefficient =
					GetFloatParam(node, "attack_power_coefficient", 0f) * multiplier,
				SpellPowerCoefficient =
					GetFloatParam(node, "spell_power_coefficient", 0f) * multiplier,
				CriticalAllowed = false
			};
		reaction.Effect = new SkillCastEffect
		{
			EffectName = reaction.ReactionName,
			ResolvedDefinition = new SingleSkillDefinition
			{
				SkillName = reaction.ReactionName,
				DisplayName = reaction.ReactionName,
				RuntimeKind = SkillRuntimeKind.SingleAttack,
				ImplementationState = SkillImplementationState.RuntimeImplemented,
				Element = damage.Element,
				SkillEffectPrefab = state.Prefab,
				RuntimeVisual = state.RuntimeVisual ?? new RuntimeSkillVisualSpec(),
				Targeting = targeting,
				Area = area,
				UsesHitTargetCount = true,
				HitAllTargets = targeting.CoverAll && maxTargets <= 0,
				HitTargetCount = maxTargets > 0
					? maxTargets
					: targeting.CoverAll ? int.MaxValue : 1,
				Damage = damage
			}
		};
		reaction.DamageValueSource = GetEnumParam(
			node,
			"value_source",
			SkillTriggerDamageValueSource.Fixed);
		reaction.DamageValueMultiplier =
			Mathf.Max(0f, GetFloatParam(node, "value_source_multiplier", 1f))
			* multiplier;
		reaction.TrackedDamageAttribute = GetEnumParam(
			node,
			"tracked_attribute",
			DamageAttribute.Physical);
		reaction.PublishSkillLifecycleEvents = false;
	}

	private static void BuildReactionStatus(
		SkillReaction reaction,
		SkillNodeBuildData node,
		ReactionOutcomeBuildState state,
		SkillTargetingSpec targeting,
		StatusEffectDefinition[] statusDefinitions)
	{
		var kind = state.HasStatusPayload
			? state.StatusKind
			: StatusValueParser.ParseStatusKind(GetParam(node, "status_name"));
		var status = CreateReactionStatus(kind, reaction, state, statusDefinitions);
		reaction.Effect = new SkillCastEffect
		{
			EffectName = reaction.ReactionName,
			ResolvedDefinition = new BuffSkillDefinition
			{
				SkillName = reaction.ReactionName,
				DisplayName = reaction.ReactionName,
				RuntimeKind = SkillRuntimeKind.Buff,
				ImplementationState = SkillImplementationState.RuntimeImplemented,
				Element = DamageAttribute.Physical,
				SkillEffectPrefab = state.Prefab,
				RuntimeVisual = state.RuntimeVisual ?? new RuntimeSkillVisualSpec(),
				Targeting = targeting,
				Target = targeting.TargetSide,
				UseConfiguredTargeting = true,
				EffectKind = BuffEffectKind.Status,
				AttachedStatus = new StatusApplicationSpec
				{
					Status = status,
					Chance = state.HasStatusPayload ? state.StatusChance : 1f,
					Stacks = state.HasStatusPayload ? state.StatusStacks : 1,
					RefreshDuration = !state.HasStatusPayload || state.RefreshDuration
				}
			}
		};
		reaction.PublishSkillLifecycleEvents = false;
	}

	private static void BuildReactionShield(
		SkillReaction reaction,
		SkillNodeBuildData node,
		ReactionOutcomeBuildState state,
		SkillTargetingSpec targeting,
		StatusEffectDefinition[] statusDefinitions)
	{
		var status = CreateReactionStatus(
			StatusEffectKind.Shield,
			reaction,
			state,
			statusDefinitions);
		reaction.Effect = new SkillCastEffect
		{
			EffectName = reaction.ReactionName,
			ResolvedDefinition = new BuffSkillDefinition
			{
				SkillName = reaction.ReactionName,
				DisplayName = reaction.ReactionName,
				RuntimeKind = SkillRuntimeKind.Buff,
				ImplementationState = SkillImplementationState.RuntimeImplemented,
				Element = DamageAttribute.Physical,
				SkillEffectPrefab = state.Prefab,
				RuntimeVisual = state.RuntimeVisual ?? new RuntimeSkillVisualSpec(),
				Targeting = targeting,
				Target = targeting.TargetSide,
				UseConfiguredTargeting = true,
				EffectKind = BuffEffectKind.Shield,
				ShieldBase = GetFloatParam(node, "base_damage", 0f),
				ShieldCoefficient = GetFloatParam(node, "spell_power_coefficient", 0f),
				ShieldStatSource = StatSource.Intelligence,
				ShieldDuration = status.Duration,
				ShieldStatus = status
			}
		};
		reaction.PublishSkillLifecycleEvents = false;
	}

	private static StatusRuntimeData CreateReactionStatus(
		StatusEffectKind kind,
		SkillReaction reaction,
		ReactionOutcomeBuildState state,
		StatusEffectDefinition[] statusDefinitions)
	{
		var status = GetStatusRuntimeData(kind, statusDefinitions);
		status.SourceSkillName = reaction.ReactionName;
		if (state.HasStatusPayload)
		{
			status.BaseStackAmount = state.StatusStacks;
			status.MaxStacks = state.StatusMaxStacks;
			status.IsStackable = status.MaxStacks != 1;
			if (state.StatusDurationSeconds > 0f)
			{
				status.Duration = state.StatusDurationSeconds;
				status.Permanent = false;
			}
		}
		if (state.DurationSeconds > 0f)
		{
			status.Duration = state.DurationSeconds;
			status.Permanent = false;
		}
		if (state.Prefab != null)
		{
			status.StatusEffectPrefab = state.Prefab;
		}
		if (state.RuntimeVisual != null)
		{
			status.RuntimeVisual = state.RuntimeVisual;
		}
		ApplyTriggeredStatusMutations(status, state.StatusMutations);
		return status;
	}

	private static SkillTargetingSpec BuildTriggerTargeting(
		ReactionOutcomeBuildState state)
	{
		var targeting = new SkillTargetingSpec
		{
			TargetSide = state.TargetSide == SkillMultiEffectTargetSide.Self
				? SkillTargetSide.Self
				: state.TargetSide == SkillMultiEffectTargetSide.AllAllies
					? SkillTargetSide.AllAllies
					: SkillTargetSide.Enemy,
			Selection = state.TargetSelection == SkillMultiEffectTargetSelection.Owner
				? SkillTargetSelection.Owner
				: SkillTargetSelection.Nearest,
			Shape = state.TargetShape == SkillMultiEffectTargetShape.Single
				? SkillTargetShape.Single
				: state.TargetShape == SkillMultiEffectTargetShape.Battlefield
					? SkillTargetShape.Battlefield
					: SkillTargetShape.Circle,
			CoverAll = state.CoverAll
				|| state.TargetShape == SkillMultiEffectTargetShape.Battlefield,
			SelectionStatusKind = state.SelectionStatusKind,
			SelectionStatusMinStacks = state.SelectionStatusMinStacks,
			HasSelectionSkillAttribute = state.HasSelectionSkillAttribute,
			SelectionSkillAttribute = state.SelectionSkillAttribute
		};
		return targeting;
	}

	private static SkillDefinition FindSkill(
		SkillDefinition[] skills,
		string skillName)
	{
		if (skills != null)
		{
			for (var i = 0; i < skills.Length; i++)
			{
				if (skills[i] != null
					&& string.Equals(
						skills[i].SkillName,
						skillName,
						StringComparison.OrdinalIgnoreCase))
				{
					return skills[i];
				}
			}
		}
		throw new InvalidOperationException(
			"Triggered skill is not registered: " + skillName);
	}

	private static void ApplyTriggeredStatusMutations(
		StatusRuntimeData status,
		IReadOnlyList<TriggerStatusMutation> mutations)
	{
		for (var i = 0; status != null && mutations != null && i < mutations.Count; i++)
		{
			var mutation = mutations[i];
			switch (mutation.Kind)
			{
				case TriggerStatusMutationKind.ActionSpeedBonus:
					status.Modifiers.ActionSpeedBonus += mutation.Amount;
					break;
				case TriggerStatusMutationKind.MoveSpeedBonus:
					status.MoveSpeedBonus += mutation.Amount;
					status.MovementSlowRate = status.MoveSpeedBonus < 0f
						? -status.MoveSpeedBonus
						: 0f;
					break;
				case TriggerStatusMutationKind.AttackPowerBonus:
					status.Modifiers.AttackPowerBonus += mutation.Amount;
					break;
				case TriggerStatusMutationKind.SpellPowerBonus:
					status.Modifiers.SpellPowerBonus += mutation.Amount;
					break;
				case TriggerStatusMutationKind.DamageBonusRate:
					status.Modifiers.DamageBonusRate += mutation.Amount;
					SetTriggeredElementModifier(status, mutation);
					break;
				case TriggerStatusMutationKind.ShieldReceivedBonus:
					status.Modifiers.ShieldReceivedBonus += mutation.Amount;
					break;
				case TriggerStatusMutationKind.CriticalChanceBonus:
					status.Modifiers.CritChanceBonusRate += mutation.Amount;
					break;
				case TriggerStatusMutationKind.CriticalDamageBonus:
					status.Modifiers.CritDamageBonusRate += mutation.Amount;
					break;
				case TriggerStatusMutationKind.DamageTakenBonus:
					status.DamageTakenBonus += mutation.Amount;
					break;
				case TriggerStatusMutationKind.ElementResistReduction:
					status.ElementResistReduction += mutation.Amount;
					status.Modifiers.ResistReduction = status.ElementResistReduction;
					status.Modifiers.ResistReductionElement = mutation.Attribute;
					SetTriggeredElementModifier(status, mutation);
					break;
				case TriggerStatusMutationKind.FlatElementResistReduction:
					status.FlatElementResistReduction += mutation.Amount;
					SetTriggeredElementModifier(status, mutation);
					break;
				case TriggerStatusMutationKind.ElementDamageTakenBonus:
					status.ElementDamageTakenBonus += mutation.Amount;
					SetTriggeredElementModifier(status, mutation);
					break;
				case TriggerStatusMutationKind.ConditionalStatusChanceBonus:
					status.ConditionalTargetStatusKinds = mutation.ConditionalStatusKinds;
					status.ConditionalStatusChanceBonus += mutation.Amount;
					break;
				case TriggerStatusMutationKind.RuntimeKindFilter:
					status.ConditionalIncomingSkillRuntimeKindValues = mutation.IncomingRuntimeKinds;
					status.ConditionalOutgoingSkillRuntimeKindValues = mutation.OutgoingRuntimeKinds;
					break;
				case TriggerStatusMutationKind.OutgoingAdditionalDamage:
					status.OutgoingAdditionalDamageMultiplier += mutation.Amount;
					status.OutgoingAdditionalDamageTriggerAttribute = mutation.Attribute;
					status.OutgoingAdditionalDamageAttribute = mutation.SecondaryAttribute;
					break;
			}
		}
	}

	private static void SetTriggeredElementModifier(
		StatusRuntimeData status,
		TriggerStatusMutation mutation)
	{
		status.HasElementModifierTarget = true;
		status.ElementModifierTarget = mutation.Attribute;
	}

	private enum TriggerStatusMutationKind
	{
		ActionSpeedBonus,
		MoveSpeedBonus,
		AttackPowerBonus,
		SpellPowerBonus,
		DamageBonusRate,
		ShieldReceivedBonus,
		CriticalChanceBonus,
		CriticalDamageBonus,
		DamageTakenBonus,
		ElementResistReduction,
		FlatElementResistReduction,
		ElementDamageTakenBonus,
		ConditionalStatusChanceBonus,
		RuntimeKindFilter,
		OutgoingAdditionalDamage
	}

	/// TriggerStatusMutation 처리에 함께 전달되는 값들을 묶는다.
	private readonly struct TriggerStatusMutation
	{

		internal TriggerStatusMutation(
			TriggerStatusMutationKind kind,
			float amount,
			DamageAttribute attribute,
			string referenceName = "",
			DamageAttribute secondaryAttribute = DamageAttribute.Physical,
			StatusEffectKind[] conditionalStatusKinds = null,
			SkillRuntimeKindCondition[] incomingRuntimeKinds = null,
			SkillRuntimeKindCondition[] outgoingRuntimeKinds = null)
		{
			Kind = kind;
			Amount = amount;
			Attribute = attribute;
			ReferenceName = referenceName ?? string.Empty;
			SecondaryAttribute = secondaryAttribute;
			ConditionalStatusKinds = conditionalStatusKinds ?? Array.Empty<StatusEffectKind>();
			IncomingRuntimeKinds = incomingRuntimeKinds ?? Array.Empty<SkillRuntimeKindCondition>();
			OutgoingRuntimeKinds = outgoingRuntimeKinds ?? Array.Empty<SkillRuntimeKindCondition>();
		}

		internal TriggerStatusMutationKind Kind { get; }
		internal float Amount { get; }
		internal DamageAttribute Attribute { get; }
		internal string ReferenceName { get; }
		internal DamageAttribute SecondaryAttribute { get; }
		internal StatusEffectKind[] ConditionalStatusKinds { get; }
		internal SkillRuntimeKindCondition[] IncomingRuntimeKinds { get; }
		internal SkillRuntimeKindCondition[] OutgoingRuntimeKinds { get; }
	}

	/// ReactionOutcomeBuildState의 변경 가능한 런타임 상태를 보관한다.
	private sealed class ReactionOutcomeBuildState
	{
		internal SkillMultiEffectTargetSide TargetSide =
			SkillMultiEffectTargetSide.Enemy;
		internal SkillMultiEffectTargetSelection TargetSelection =
			SkillMultiEffectTargetSelection.Nearest;
		internal SkillMultiEffectTargetShape TargetShape =
			SkillMultiEffectTargetShape.Single;
		internal SkillMultiEffectCenterMode CenterMode =
			SkillMultiEffectCenterMode.PrimarySkillCenter;
		internal bool CoverAll;
		internal int MaxTargets;
		internal float DurationSeconds;
		internal bool HasStatusPayload;
		internal StatusEffectKind StatusKind;
		internal float StatusChance = 1f;
		internal int StatusStacks = 1;
		internal float StatusDurationSeconds;
		internal int StatusMaxStacks = 1;
		internal bool RefreshDuration = true;
		internal StatusEffectKind SelectionStatusKind;
		internal int SelectionStatusMinStacks;
		internal bool HasSelectionSkillAttribute;
		internal DamageAttribute SelectionSkillAttribute;
		internal GameObject Prefab;
		internal RuntimeSkillVisualSpec RuntimeVisual;
		internal readonly List<TriggerStatusMutation> StatusMutations =
			new List<TriggerStatusMutation>();
	}

	internal static bool CanProcessNode(string ownerKind, string handlerName)
	{
		if (string.Equals(ownerKind, "Skill", StringComparison.OrdinalIgnoreCase)
			&& IsSingleBaseFieldHandler(handlerName))
		{
			return true;
		}
		return IsRuntimeNodeHandler(handlerName);
	}

	private static SkillNode MapSkillNode(SkillNodeBuildData node)
	{
		if (node == null || !node.EnabledByDefault)
		{
			return null;
		}
		string text = node.HandlerName;
		if (text == null)
		{
			text = string.Empty;
		}
		if (string.Equals(node.OwnerKind, "Skill", StringComparison.OrdinalIgnoreCase)
			&& IsSingleBaseFieldHandler(text))
		{
			return null;
		}
		if (string.Equals(text, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CastConditionOp(GetFloatParam(node, "threshold", 0f)));
		}
		if (string.Equals(text, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new SkillAttributeConditionOp(
				GetEnumParam(node, "attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new TargetStatusConditionOp(
				StatusValueParser.ParseConditionStatusExpression(
					GetParam(node, "status_ids"))));
		}
		if (string.Equals(text, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new SourceStatusConditionOp(
				StatusValueParser.ParseStatusKind(GetParam(node, "status_name")),
				Mathf.Max(1, GetIntParam(node, "min_stacks", 1))));
		}
		if (string.Equals(text, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CastConditionOp(GetFloatParam(node, "threshold_bonus", 0f)));
		}
		if (string.Equals(text, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.ExecuteMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase) && string.Equals(GetParam(node, "predicate"), "is_boss", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CritModifierOp(GetFloatParam(node, "crit_chance_bonus", 0f)));
		}
		if (string.Equals(text, "CooldownReset", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new KillActionOp(KillActionOpKind.CooldownReset, 0f, GetBoolParam(node, "requires_execute", defaultValue: false)));
		}
		if (string.Equals(text, "CooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio", 0f), requiresExecute: false));
		}
		if (string.Equals(text, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio_bonus", 0f), requiresExecute: false));
		}
		if (string.Equals(text, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			string statusName = GetParam(node, "status_name");
			StatusEffectKind statusKind = StatusValueParser.ParseStatusKind(statusName);
			return SkillNode.FromOperation(new CountStatusDamageActionOp(
				GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies),
				statusKind,
				GetFloatParam(node, "amount_per_count", 0f),
				GetIntParam(node, "max_count", 0)));
		}
		if (string.Equals(text, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ConsecutiveHitActionOp(
				GetFloatParam(node, "bonus_rate", 0f),
				GetFloatParam(node, "max_bonus", 0f)));
		}
		if (string.Equals(text, "BranchDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new BranchDamageActionOp(
				GetFloatParam(node, "chance_bonus", 0f),
				GetIntParam(node, "count", 0),
				GetFloatParam(node, "damage_multiplier", 0f),
				GetFloatParam(node, "search_radius", 0f)));
		}
		if (string.Equals(text, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			string statusName = GetParam(node, "status_name");
			StatusEffectKind statusKind = StatusValueParser.ParseStatusKind(statusName);
			return SkillNode.FromOperation(new ConditionalDamageActionOp(
				GetFloatParam(node, "multiplier", 1f),
				statusKind,
				GetIntParam(node, "min_stacks", 1)));
		}
		if (string.Equals(text, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			string sourceStatusName = GetParam(node, "source_status_name");
			StatusEffectKind sourceStatusKind = StatusValueParser.ParseStatusKind(sourceStatusName);
			return SkillNode.FromOperation(new StatusConditionalDamageTakenActionOp(
				GetFloatParam(node, "bonus", 0f),
				sourceStatusKind));
		}
		if (string.Equals(text, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusValueParser.ParseStatusKind(GetParam(node, "status_name"));
			return SkillNode.FromOperation(new ConditionalCritChanceActionOp(
				GetFloatParam(node, "crit_chance_bonus", 0f),
				statusKind,
				GetIntParam(node, "min_stacks", 0)));
		}
		if (string.Equals(text, "BurstDamageRule", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new BurstDamageActionOp(
				GetIntParam(node, "projectile_index", 0),
				GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new BurstStatusActionOp(
				GetIntParam(node, "projectile_index", 0),
				GetIntParam(node, "bonus", 0)));
		}
		if (string.Equals(text, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new FollowUpProjectileActionOp(
				GetIntParam(node, "count", 0),
				GetFloatParam(node, "delay_seconds", 0f),
				GetFloatParam(node, "damage_multiplier", 1f)));
		}
		if (string.Equals(text, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind sourceStatus = StatusValueParser.ParseStatusKind(GetParam(node, "source_status_name"));
			StatusEffectKind appliedStatus = StatusValueParser.ParseStatusKind(GetParam(node, "apply_status_name"));
			return SkillNode.FromOperation(new ThresholdStatusActionOp(
				sourceStatus,
				GetIntParam(node, "min_stacks", 0),
				appliedStatus));
		}
		if (string.Equals(text, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new RepeatPerTargetActionOp(
				GetIntParam(node, "repeat_count", 0),
				GetFloatParam(node, "repeat_interval_seconds", 0f),
				GetFloatParam(node, "repeat_damage_multiplier", 1f)));
		}
		if (string.Equals(text, "PullToCenter", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new PullToCenterActionOp(
				GetFloatParam(node, "distance_per_tick", 0f)));
		}
		if (string.Equals(text, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusValueParser.ParseStatusKind(GetParam(node, "status_name"));
			return SkillNode.FromOperation(new RedistributeConsumedStatusActionOp(
				GetFloatParam(node, "ratio", 0f),
				statusKind,
				GetFloatParam(node, "radius", 0f),
				GetIntParam(node, "target_count", 0)));
		}
		if (string.Equals(text, "AdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			string target = GetParam(node, "target");
			if (string.IsNullOrWhiteSpace(target))
			{
				target = GetParam(node, "target_side");
			}
			return SkillNode.FromOperation(new AdditionalDamageActionOp(
				GetFloatParam(node, "chance", 1f),
				GetFloatParam(node, "multiplier", 1f),
				GetEnumParam(node, "attribute", DamageAttribute.Physical),
				target));
		}
		if (string.Equals(text, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CoreDamageActionOp(
				GetParam(node, "hitbox_name"),
				GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new CoreAdditionalDamageActionOp(
				GetParam(node, "hitbox_name"),
				GetFloatParam(node, "chance", 1f),
				GetFloatParam(node, "multiplier", 1f),
				GetEnumParam(node, "attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new HitChainDamageActionOp(
				GetIntParam(node, "hit_count", 0),
				GetIntParam(node, "max_targets", 0),
				GetFloatParam(node, "radius", 0f),
				GetFloatParam(node, "multiplier", 1f),
				GetEnumParam(node, "attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new HitCountCooldownRefundActionOp(
				GetParam(node, "target_skill_name"),
				GetIntParam(node, "min_targets", 0),
				GetFloatParam(node, "ratio", 0f)));
		}
		if (string.Equals(text, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ReloadReducePerHitActionOp(
				GetParam(node, "target_skill_name"),
				GetFloatParam(node, "seconds_per_hit", 0f)));
		}
		var skillActionOp = MapSkillActionOp(node, text);
		return SkillNode.FromOperation(skillActionOp);
	}

	private static bool IsSingleBaseFieldHandler(string handlerName)
	{
		if (string.Equals(handlerName, "StatusFilteredDeployment", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return string.Equals(handlerName, "TargetStatusStackDamage", StringComparison.OrdinalIgnoreCase);
	}

	private static bool IsRuntimeNodeHandler(string handlerName)
	{
		if (string.Equals(handlerName, "EffectDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ApplyDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ApplyShield", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusModifier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "EffectTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "SelectTargets", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "EffectLifetime", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "SetDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionStatusExpression", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionHealthRatioMax", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionHitCountMin", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "EffectVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ShowVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RecastZone", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ExecuteSkill", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RefundCooldown", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ReduceReload", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerName, "StatusMoveSpeedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusSpellPowerBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusCriticalDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusElementResistReduction", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusConditionalStatusChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusOutgoingAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerName, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CooldownReset", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CooldownRefund", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerName, "DamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "MagazineBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "PierceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RadiusBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "DurationBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "DurationMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "AdditionalProjectileBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "BranchDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "LineCastRepeatCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerName, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerName, "BurstDamageRule", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "PullToCenter", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "AdditionalDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CritChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "CritDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerName, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	private static SkillActionOp MapSkillActionOp(SkillNodeBuildData node, string handlerName)
	{
		if (string.Equals(handlerName, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShieldAmountMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CooldownMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.MagazineBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ReloadTimeMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.PierceBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.RadiusMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "RadiusBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.RadiusBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "DurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DurationBonus, GetFloatParam(node, "bonus_seconds", 0f));
		}
		if (string.Equals(handlerName, "DurationMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DurationMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageDelayMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "AdditionalProjectileBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.AdditionalProjectileBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShotIntervalMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountSet, GetIntParam(node, "value", 0));
		}
		if (string.Equals(handlerName, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusMaxStacksBonus, GetParam(node, "status_name"), GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageRateBonus, GetParam(node, "status_name"), GetFloatParam(node, "bonus_rate_per_stack", 0f));
		}
		if (string.Equals(handlerName, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TriggerProcChanceBonus, GetParam(node, "trigger_name"), GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.HitTargetCountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "LineCastRepeatCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.LineCastRepeatCountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerName, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusActionSpeedBonus, GetParam(node, "status_name"), GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusAttackPowerBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusAilmentResistanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusDamageBonusRate, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusShieldReceivedBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalChanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusFlatElementResistReduction, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusDurationBonus, GetParam(node, "status_name"), GetFloatParam(node, "bonus_seconds", 0f));
		}
		if (string.Equals(handlerName, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusElementDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "CritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CritChanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "CritDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CritDamageBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.BeamWidthBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerName, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.KnockbackDistanceMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerName, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ConsumeTargetStatusRatioOverride, GetFloatParam(node, "ratio", 0f));
		}
		throw new InvalidOperationException("Unsupported skill node handler: " + handlerName);
	}

	private static bool TryMapStatusMutation(
		SkillNodeBuildData node,
		string handlerName,
		out TriggerStatusMutation operation)
	{
		var amount = GetFloatParam(node, "bonus", 0f);
		var attribute = GetEnumParam(node, "attribute", DamageAttribute.Physical);
		if (string.Equals(handlerName, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(
				TriggerStatusMutationKind.ActionSpeedBonus,
				amount,
				attribute,
				GetParam(node, "status_name"));
			return true;
		}
		if (string.Equals(handlerName, "StatusMoveSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.MoveSpeedBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.AttackPowerBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusSpellPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.SpellPowerBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.DamageBonusRate, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.ShieldReceivedBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.CriticalChanceBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusCriticalDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.CriticalDamageBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.DamageTakenBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.ElementResistReduction, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.FlatElementResistReduction, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(TriggerStatusMutationKind.ElementDamageTakenBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerName, "StatusConditionalStatusChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(
				TriggerStatusMutationKind.ConditionalStatusChanceBonus,
				amount,
				attribute,
				conditionalStatusKinds: StatusValueParser.ParseStatusKinds(GetParam(node, "status_ids")));
			return true;
		}
		if (string.Equals(handlerName, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(
				TriggerStatusMutationKind.RuntimeKindFilter,
				0f,
				attribute,
				incomingRuntimeKinds: StatusValueParser.ParseSkillRuntimeKindConditions(
					GetParam(node, "incoming_skill_runtime_kinds")),
				outgoingRuntimeKinds: StatusValueParser.ParseSkillRuntimeKindConditions(
					GetParam(node, "outgoing_skill_runtime_kinds")));
			return true;
		}
		if (string.Equals(handlerName, "StatusOutgoingAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			operation = new TriggerStatusMutation(
				TriggerStatusMutationKind.OutgoingAdditionalDamage,
				GetFloatParam(node, "multiplier", 0f),
				GetEnumParam(node, "trigger_attribute", DamageAttribute.Physical),
				string.Empty,
				GetEnumParam(node, "damage_attribute", DamageAttribute.Physical));
			return true;
		}

		operation = default;
		return false;
	}

	internal static string GetParam(SkillNodeBuildData node, string key)
	{
		if (node == null || node.Params == null || string.IsNullOrWhiteSpace(key))
		{
			return string.Empty;
		}
		for (int i = 0; i < node.Params.Length; i++)
		{
			SkillNodeParamBuildData skillNodeParamDefinition = node.Params[i];
			if (skillNodeParamDefinition != null && string.Equals(skillNodeParamDefinition.ParamKey, key, StringComparison.OrdinalIgnoreCase))
			{
				if (skillNodeParamDefinition.Value == null)
				{
					return string.Empty;
				}
				return skillNodeParamDefinition.Value;
			}
		}
		return string.Empty;
	}

	internal static float GetFloatParam(SkillNodeBuildData node, string key, float defaultValue)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !float.TryParse(param, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	internal static int GetIntParam(SkillNodeBuildData node, string key, int defaultValue)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !int.TryParse(param, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	internal static bool GetBoolParam(SkillNodeBuildData node, string key, bool defaultValue)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param))
		{
			return defaultValue;
		}
		if (bool.TryParse(param, out var result))
		{
			return result;
		}
		if (!string.Equals(param, "1", StringComparison.OrdinalIgnoreCase) && !string.Equals(param, "yes", StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(param, "y", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	internal static T GetEnumParam<T>(SkillNodeBuildData node, string key, T defaultValue) where T : struct
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !Enum.TryParse<T>(param, ignoreCase: true, out var result))
		{
			return defaultValue;
		}
		return result;
	}
    }
}
