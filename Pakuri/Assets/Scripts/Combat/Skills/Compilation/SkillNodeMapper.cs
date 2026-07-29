using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 작성 데이터의 SkillNodeDefinition을 전투 실행용 SkillNode로 변환한다.
 * 노드 종류와 값을 해석해 SkillNode 전투 실행 값으로 옮긴다.
 */
namespace Pakuri.InGame
{
    public static class SkillNodeMapper
    {
	/*
	 * MapSkillNodeDefinitions에 필요한 형식으로 변환해 반환한다.
	 */
	public static SkillNode[] MapSkillNodeDefinitions(SkillNodeDefinition[] source /* 변환할 스킬 노드 정의 목록 */)
	{
		if (source == null || source.Length == 0)
		{
			return Array.Empty<SkillNode>();
		}
		List<SkillNode> list = new List<SkillNode>(source.Length);
		for (int i = 0; i < source.Length; i++)
		{
			SkillNode skillExecutionNode = MapSkillNodeDefinition(source[i]);
			if (skillExecutionNode != null)
			{
				skillExecutionNode.TargetSkillId = source[i].TargetSkillId ?? string.Empty;
				list.Add(skillExecutionNode);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<SkillNode>();
	}

	/*
	 * 선택지의 정리된 노드를 대상 스킬별 실행 값으로 한 번만 변환해 반환한다.
	 * 모든 처리기는 SkillNode로 변환하므로 별도 임시 SkillChoice 객체를 만들지 않는다.
	 */
	internal static SkillNode[] GetChoiceRuntimeNodes(SkillChoice choice /* 적용할 선택지 */, string targetSkillId /* 적용 대상 스킬 식별자 */)
	{
		if (choice == null || choice.Source == null)
		{
			return Array.Empty<SkillNode>();
		}

		if (choice.TryGetRuntimeNodes(targetSkillId, out var cached))
		{
			return cached;
		}

		SkillNodeDefinition[] filtered = FilterSkillNodeDefinitionsForTarget(
			choice.Source.NormalizedNodes,
			targetSkillId);
		SkillNode[] nodes = MapSkillNodeDefinitions(filtered);
		choice.CacheRuntimeNodes(targetSkillId, nodes);
		return nodes;
	}

	/*
	 * FilterSkillNodeDefinitionsForTarget에 해당하는 값을 찾아 반환한다.
	 */
	public static SkillNodeDefinition[] FilterSkillNodeDefinitionsForTarget(SkillNodeDefinition[] source /* 변환할 스킬 노드 정의 목록 */, string targetSkillId /* 대상 스킬 식별자 */)
	{
		if (source == null || source.Length == 0)
		{
			return Array.Empty<SkillNodeDefinition>();
		}
		if (string.IsNullOrWhiteSpace(targetSkillId))
		{
			return source;
		}
		List<SkillNodeDefinition> list = new List<SkillNodeDefinition>(source.Length);
		foreach (SkillNodeDefinition skillNodeDefinition in source)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault && string.Equals(skillNodeDefinition.TargetSkillId, targetSkillId, StringComparison.OrdinalIgnoreCase))
			{
				list.Add(skillNodeDefinition);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<SkillNodeDefinition>();
	}

	/*
	 * HasSkillNodeForTarget 조건을 만족하는지 확인한다.
	 */
	public static bool HasSkillNodeForTarget(SkillNodeDefinition[] source /* 변환할 스킬 노드 정의 목록 */, string targetSkillId /* 대상 스킬 식별자 */)
	{
		if (source == null || source.Length == 0 || string.IsNullOrWhiteSpace(targetSkillId))
		{
			return false;
		}
		foreach (SkillNodeDefinition skillNodeDefinition in source)
		{
			if (skillNodeDefinition != null && skillNodeDefinition.EnabledByDefault && string.Equals(skillNodeDefinition.TargetSkillId, targetSkillId, StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}

	/*
	 * CanProcessNode 조건을 만족하는지 확인한다.
	 */
	internal static bool CanProcessNode(string ownerKind /* 소유자 종류 */, string handlerId /* 처리기 식별자 */)
	{
		if (string.Equals(ownerKind, "Skill", StringComparison.OrdinalIgnoreCase)
			&& IsSingleBaseFieldHandler(handlerId))
		{
			return true;
		}
		return IsRuntimeNodeHandler(handlerId);
	}

	/*
	 * MapSkillNodeDefinition에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillNode MapSkillNodeDefinition(SkillNodeDefinition node /* 노드 */)
	{
		if (node == null || !node.EnabledByDefault)
		{
			return null;
		}
		string text = node.HandlerId;
		if (text == null)
		{
			text = string.Empty;
		}
		if (string.Equals(text, "EffectDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "ApplyDamage", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ApplyDamageNodeOp(
				GetEnumParam(node, "attribute", DamageAttribute.Physical),
				GetFloatParam(node, "base_damage", 0f),
				GetFloatParam(node, "attack_power_coefficient", 0f),
				GetFloatParam(node, "spell_power_coefficient", 0f),
				GetFloatParam(node, "damage_multiplier", 1f),
				GetFloatParam(node, "radius", 0f),
				GetFloatParam(node, "tick_interval_seconds", 0f),
				GetEnumParam(node, "value_source", NodeDamageValueSource.Fixed),
				GetFloatParam(node, "value_source_multiplier", 1f),
				GetEnumParam(node, "tracked_attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "ApplyShield", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ApplyShieldNodeOp(
				GetFloatParam(node, "base_damage", 0f),
				GetFloatParam(node, "spell_power_coefficient", 0f)));
		}
		if (string.Equals(text, "StatusModifier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ApplyStatusNodeOp(
				StatusRuntimeCompiler.ParseStatusKind("passive-buff"),
				ParseTargetScope(GetParam(node, "status_target_scope")),
				ParseMergePolicy(GetParam(node, "status_merge_policy"))));
		}
		if (string.Equals(text, "ApplyStatus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ApplyStatusNodeOp(
				StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"))));
		}
		if (string.Equals(text, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase))
		{
			var statusId = GetParam(node, "status_id");
			var statusKind = string.IsNullOrWhiteSpace(statusId)
				? StatusEffectKind.None
				: StatusRuntimeCompiler.ParseStatusKind(statusId);
			return SkillNode.FromOperation(new StatusPayloadNodeOp(
				statusKind,
				GetFloatParam(node, "status_chance", 1f),
				GetIntParam(node, "status_stack_amount", 1),
				GetFloatParam(node, "status_duration_seconds", 0f),
				GetIntParam(node, "status_max_stacks", 1),
				!string.Equals(GetParam(node, "status_merge_policy"), "StackDuration", StringComparison.OrdinalIgnoreCase)));
		}
		if (string.Equals(text, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "ExtendStatusDuration", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ExtendStatusDurationNodeOp(
				StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"))));
		}
		if (string.Equals(text, "EffectTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "SelectTargets", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new SelectTargetsNodeOp(
				GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.Enemy),
				GetEnumParam(node, "target_selection", SkillMultiEffectTargetSelection.Nearest),
				GetEnumParam(node, "target_shape", SkillMultiEffectTargetShape.Single),
				GetEnumParam(node, "center_mode", SkillMultiEffectCenterMode.PrimarySkillCenter),
				GetEnumParam(node, "visual_anchor_mode", SkillMultiEffectVisualAnchorMode.Center),
				GetBoolParam(node, "apply_once", false),
				GetBoolParam(node, "cover_all", false),
				GetIntParam(node, "max_targets", 0)));
		}
		if (string.Equals(text, "EffectLifetime", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "SetDuration", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new SetDurationNodeOp(
				GetFloatParam(node, "duration_seconds", 0f)));
		}
		if (string.Equals(text, "ConditionStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "ConditionStatusExpression", StringComparison.OrdinalIgnoreCase))
		{
			var expression = GetParam(node, "status_id");
			var minimumStacks = GetIntParam(node, "min_stacks", 1);
			if (string.Equals(text, "ConditionStatus", StringComparison.OrdinalIgnoreCase)
				&& minimumStacks > 1
				&& !string.IsNullOrWhiteSpace(expression))
			{
				expression = string.Concat(
					expression,
					":",
					minimumStacks.ToString(CultureInfo.InvariantCulture));
			}
			return SkillNode.FromOperation(new StatusConditionNodeOp(
				StatusRuntimeCompiler.ParseConditionStatusExpression(expression),
				GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.Enemy),
				StatusRuntimeCompiler.ParseIdList(GetParam(node, "source_skill_id"))));
		}
		if (string.Equals(text, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new StatusConditionNodeOp(
				StatusRuntimeCompiler.ParseConditionStatusExpression(GetParam(node, "status_ids")),
				GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.Enemy),
				StatusRuntimeCompiler.ParseIdList(GetParam(node, "source_skill_id"))));
		}
		if (string.Equals(text, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new SkillAttributeConditionNodeOp(
				GetEnumParam(node, "attribute", DamageAttribute.Physical)));
		}
		if (string.Equals(text, "ConditionHealthRatioMax", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new HealthRatioConditionNodeOp(
				GetFloatParam(node, "ratio", 0f)));
		}
		if (string.Equals(text, "ConditionHitCountMin", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new HitCountConditionNodeOp(
				GetIntParam(node, "min_targets", 0)));
		}
		if (string.Equals(text, "EffectVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(text, "ShowVisual", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ShowVisualNodeOp(
				node.ResolvedPrefab,
				node.ResolvedRuntimeVisual));
		}
		if (string.Equals(text, "RecastZone", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new RecastZoneNodeOp(
				GetParam(node, "source_skill_id"),
				GetFloatParam(node, "delay_seconds", 0f),
				GetFloatParam(node, "duration_seconds", 0f),
				GetFloatParam(node, "radius_multiplier", 1f),
				GetBoolParam(node, "inherit_snapshot", true),
				GetIntParam(node, "max_generation", 1)));
		}
		if (string.Equals(text, "ExecuteSkill", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ExecuteSkillNodeOp(
				GetParam(node, "skill_id"),
				GetFloatParam(node, "damage_multiplier", 1f)));
		}
		if (string.Equals(text, "RefundCooldown", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new RefundCooldownNodeOp(
				GetParam(node, "skill_id"),
				GetFloatParam(node, "ratio", 0f)));
		}
		if (string.Equals(text, "ReduceReload", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ReduceReloadNodeOp(
				GetParam(node, "skill_id"),
				GetFloatParam(node, "ratio", 0f)));
		}
		if (string.Equals(node.OwnerKind, "Trigger", StringComparison.OrdinalIgnoreCase)
			&& TryMapStatusMutation(node, text, out var statusMutation))
		{
			return SkillNode.FromOperation(statusMutation);
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
			string statusId = GetParam(node, "status_id");
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(statusId);
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
			string statusId = GetParam(node, "status_id");
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(statusId);
			return SkillNode.FromOperation(new ConditionalDamageActionOp(
				GetFloatParam(node, "multiplier", 1f),
				statusKind,
				GetIntParam(node, "min_stacks", 1)));
		}
		if (string.Equals(text, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			string sourceStatusId = GetParam(node, "source_status_id");
			StatusEffectKind sourceStatusKind = StatusRuntimeCompiler.ParseStatusKind(sourceStatusId);
			return SkillNode.FromOperation(new StatusConditionalDamageTakenActionOp(
				GetFloatParam(node, "bonus", 0f),
				sourceStatusKind));
		}
		if (string.Equals(text, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"));
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
			StatusEffectKind sourceStatus = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "source_status_id"));
			StatusEffectKind appliedStatus = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "apply_status_id"));
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
		if (string.Equals(text, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"));
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
				GetParam(node, "target_skill_id"),
				GetIntParam(node, "min_targets", 0),
				GetFloatParam(node, "ratio", 0f)));
		}
		if (string.Equals(text, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromOperation(new ReloadReducePerHitActionOp(
				GetParam(node, "target_skill_id"),
				GetFloatParam(node, "seconds_per_hit", 0f)));
		}
		if (string.Equals(text, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			StatusEffectKind statusKind = StatusRuntimeCompiler.ParseStatusKind(GetParam(node, "status_id"));
			return SkillNode.FromOperation(new SourceStatusRequirementOp(
				statusKind,
				GetIntParam(node, "min_stacks", 1)));
		}
		var skillActionOp = MapSkillActionOp(node, text);
		return SkillNode.FromOperation(skillActionOp);
	}

	/*
	 * IsSingleBaseFieldHandler 조건을 만족하는지 확인한다.
	 */
	private static bool IsSingleBaseFieldHandler(string handlerId /* 처리기 식별자 */)
	{
		if (string.Equals(handlerId, "StatusFilteredDeployment", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return string.Equals(handlerId, "TargetStatusStackDamage", StringComparison.OrdinalIgnoreCase);
	}

	/*
	 * IsRuntimeNodeHandler 조건을 만족하는지 확인한다.
	 */
	private static bool IsRuntimeNodeHandler(string handlerId /* 처리기 식별자 */)
	{
		if (string.Equals(handlerId, "EffectDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ApplyDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ApplyShield", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusModifier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ExtendStatusDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "EffectTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "SelectTargets", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "EffectLifetime", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "SetDuration", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionStatusExpression", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionHealthRatioMax", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionHitCountMin", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "EffectVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ShowVisual", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RecastZone", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ExecuteSkill", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RefundCooldown", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ReduceReload", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "StatusMoveSpeedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusSpellPowerBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusCriticalDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusCriticalResistanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusElementResistReduction", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusConditionalStatusChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusOutgoingAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownReset", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownRefund", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "DurationMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "AdditionalProjectileBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BranchDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "LineCastRepeatCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		if (string.Equals(handlerId, "BurstDamageRule", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "FollowUpProjectile", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ThresholdApplyStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BurstStatusStacksBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RepeatPerTarget", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "TargetStatusCritBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RedistributeConsumedStatus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "AdditionalDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CoreAdditionalDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CoreDamageMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CritChanceBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "CritDamageBonus", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "EveryNthHitChainDamage", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "HitCountCooldownRefund", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "ReloadReducePerHit", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(handlerId, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
		{
			return true;
		}
		return false;
	}

	/*
	 * MapSkillActionOp에 필요한 형식으로 변환해 반환한다.
	 */
	private static SkillActionOp MapSkillActionOp(SkillNodeDefinition node /* 노드 */, string handlerId /* 처리기 식별자 */)
	{
		if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShieldAmountMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CooldownMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.MagazineBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ReloadTimeMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.PierceBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "RadiusMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.RadiusMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "RadiusBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.RadiusBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "DurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DurationBonus, GetFloatParam(node, "bonus_seconds", 0f));
		}
		if (string.Equals(handlerId, "DurationMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DurationMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "DamageDelayMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageDelayMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "AdditionalProjectileBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.AdditionalProjectileBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShotIntervalMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountSet, GetIntParam(node, "value", 0));
		}
		if (string.Equals(handlerId, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusMaxStacksBonus, GetParam(node, "status_id"), GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageRateBonus, GetParam(node, "status_id"), GetFloatParam(node, "bonus_rate_per_stack", 0f));
		}
		if (string.Equals(handlerId, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TriggerProcChanceBonus, GetParam(node, "trigger_id"), GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.HitTargetCountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "LineCastRepeatCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.LineCastRepeatCountBonus, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusActionSpeedBonus, GetParam(node, "status_id"), GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusAttackPowerBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusAilmentResistanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusAilmentResistanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusDamageBonusRate, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusShieldReceivedBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalChanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusFlatElementResistReduction, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusDurationBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusDurationBonus, GetParam(node, "status_id"), GetFloatParam(node, "bonus_seconds", 0f));
		}
		if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusElementDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "CritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CritChanceBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "CritDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CritDamageBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "BeamWidthBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.BeamWidthBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "KnockbackDistanceMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.KnockbackDistanceMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "TargetStatusStackDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ConsumeTargetStatusRatioOverride", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ConsumeTargetStatusRatioOverride, GetFloatParam(node, "ratio", 0f));
		}
		throw new InvalidOperationException("Unsupported skill node handler: " + handlerId);
	}

	private static bool TryMapStatusMutation(
		SkillNodeDefinition node,
		string handlerId,
		out StatusMutationNodeOp operation)
	{
		var amount = GetFloatParam(node, "bonus", 0f);
		var attribute = GetEnumParam(node, "attribute", DamageAttribute.Physical);
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(
				StatusMutationKind.ActionSpeedBonus,
				amount,
				attribute,
				GetParam(node, "status_id"));
			return true;
		}
		if (string.Equals(handlerId, "StatusMoveSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.MoveSpeedBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.AttackPowerBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusSpellPowerBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.SpellPowerBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.DamageBonusRate, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.ShieldReceivedBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.CriticalChanceBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusCriticalDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.CriticalDamageBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusCriticalResistanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.CriticalResistanceBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.DamageTakenBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.ElementResistReduction, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.FlatElementResistReduction, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(StatusMutationKind.ElementDamageTakenBonus, amount, attribute);
			return true;
		}
		if (string.Equals(handlerId, "StatusConditionalStatusChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(
				StatusMutationKind.ConditionalStatusChanceBonus,
				amount,
				attribute,
				conditionalStatusKinds: StatusRuntimeCompiler.ParseStatusKinds(GetParam(node, "status_ids")));
			return true;
		}
		if (string.Equals(handlerId, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(
				StatusMutationKind.RuntimeKindFilter,
				0f,
				attribute,
				incomingRuntimeKinds: StatusRuntimeCompiler.ParseSkillRuntimeKindConditions(
					GetParam(node, "incoming_skill_runtime_kinds")),
				outgoingRuntimeKinds: StatusRuntimeCompiler.ParseSkillRuntimeKindConditions(
					GetParam(node, "outgoing_skill_runtime_kinds")));
			return true;
		}
		if (string.Equals(handlerId, "StatusOutgoingAdditionalDamage", StringComparison.OrdinalIgnoreCase))
		{
			operation = new StatusMutationNodeOp(
				StatusMutationKind.OutgoingAdditionalDamage,
				GetFloatParam(node, "multiplier", 0f),
				GetEnumParam(node, "trigger_attribute", DamageAttribute.Physical),
				string.Empty,
				GetEnumParam(node, "damage_attribute", DamageAttribute.Physical));
			return true;
		}

		operation = default;
		return false;
	}

	private static StatusTargetScope ParseTargetScope(string value)
	{
		return string.IsNullOrWhiteSpace(value)
			? StatusTargetScope.Unspecified
			: StatusRuntimeCompiler.ParseTargetScope(value);
	}

	private static StatusMergePolicy ParseMergePolicy(string value)
	{
		return string.IsNullOrWhiteSpace(value)
			? StatusMergePolicy.Unspecified
			: StatusRuntimeCompiler.ParseMergePolicy(value);
	}

	/*
	 * GetParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static string GetParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */)
	{
		if (node == null || node.Params == null || string.IsNullOrWhiteSpace(key))
		{
			return string.Empty;
		}
		for (int i = 0; i < node.Params.Length; i++)
		{
			SkillNodeParamDefinition skillNodeParamDefinition = node.Params[i];
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

	/*
	 * GetFloatParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static float GetFloatParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, float defaultValue /* 값이 없을 때 사용할 기본값 */)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !float.TryParse(param, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	/*
	 * GetIntParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static int GetIntParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, int defaultValue /* 값이 없을 때 사용할 기본값 */)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !int.TryParse(param, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return defaultValue;
		}
		return result;
	}

	/*
	 * GetBoolParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static bool GetBoolParam(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, bool defaultValue /* 값이 없을 때 사용할 기본값 */)
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

	/*
	 * GetEnumParam에 해당하는 값을 찾아 반환한다.
	 */
	internal static T GetEnumParam<T>(SkillNodeDefinition node /* 노드 */, string key /* 조회 키 */, T defaultValue /* 값이 없을 때 사용할 기본값 */) where T : struct
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
