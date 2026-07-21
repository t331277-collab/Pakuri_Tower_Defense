using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Data;

/*
 * 작성 데이터의 SkillNodeDefinition을 전투 실행용 SkillNode로 변환한다.
 * 실행 순서를 만드는 SkillNodeCompiler와 달리 노드 종류와 파라미터 해석만 담당한다.
 */
namespace Pakuri.InGame
{
    public static class SkillNodeMapper
    {
	public static SkillNode[] MapSkillNodeDefinitions(SkillNodeDefinition[] source)
	{
		if (source == null || source.Length == 0)
		{
			return Array.Empty<SkillNode>();
		}
		List<SkillNode> list = new List<SkillNode>(source.Length);
		for (int i = 0; i < source.Length; i++)
		{
			SkillNode skillExecutionPlanNode = MapSkillNodeDefinition(source[i]);
			if (skillExecutionPlanNode != null)
			{
				list.Add(skillExecutionPlanNode);
			}
		}
		if (list.Count != 0)
		{
			return list.ToArray();
		}
		return Array.Empty<SkillNode>();
	}

	public static SkillNodeDefinition[] FilterSkillNodeDefinitionsForTarget(SkillNodeDefinition[] source, string targetSkillId)
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

	public static bool HasSkillNodeForTarget(SkillNodeDefinition[] source, string targetSkillId)
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

	private static SkillNode MapSkillNodeDefinition(SkillNodeDefinition node)
	{
		if (node == null || !node.EnabledByDefault)
		{
			return null;
		}
		string text = node.HandlerId ?? string.Empty;
		if (string.Equals(text, "TargetHealthRatioCondition", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromCastCondition(new CastConditionOp(CastConditionOpKind.TargetHealthRatioBonus, GetFloatParam(node, "threshold", 0f)));
		}
		if (string.Equals(text, "TargetHealthRatioThresholdBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromCastCondition(new CastConditionOp(CastConditionOpKind.TargetHealthRatioBonus, GetFloatParam(node, "threshold_bonus", 0f)));
		}
		if (string.Equals(text, "ExecuteDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromDamageModifier(new DamageModifierOp(DamageModifierOpKind.ExecuteMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "TargetPredicateDamageMultiplier", StringComparison.OrdinalIgnoreCase) && string.Equals(GetParam(node, "predicate"), "is_boss", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromDamageModifier(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "BossDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromDamageModifier(new DamageModifierOp(DamageModifierOpKind.BossMultiplier, GetFloatParam(node, "multiplier", 1f)));
		}
		if (string.Equals(text, "ExecuteCritChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromCritModifier(new CritModifierOp(CritModifierOpKind.ExecuteChanceBonus, GetFloatParam(node, "crit_chance_bonus", 0f)));
		}
		if (string.Equals(text, "CooldownReset", StringComparison.OrdinalIgnoreCase) || string.Equals(text, "CooldownResetOnKill", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromKillAction(new KillActionOp(KillActionOpKind.CooldownReset, 0f, GetBoolParam(node, "requires_execute", fallback: false)));
		}
		if (string.Equals(text, "CooldownRefund", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromKillAction(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio", 0f), requiresExecute: false));
		}
		if (string.Equals(text, "CooldownRefundBonus", StringComparison.OrdinalIgnoreCase))
		{
			return SkillNode.FromKillAction(new KillActionOp(KillActionOpKind.CooldownRefundBonus, GetFloatParam(node, "ratio_bonus", 0f), requiresExecute: false));
		}
		SkillActionOp? skillActionOp = MapSkillActionOp(node, text);
		if (skillActionOp.HasValue)
		{
			return SkillNode.FromAction(skillActionOp.Value);
		}
		return null;
	}

	private static SkillActionOp? MapSkillActionOp(SkillNodeDefinition node, string handlerId)
	{
		if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.DamageMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShieldAmountMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "CountStatusDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CountStatusDamageMultiplier, GetFloatParam(node, "amount_per_count", 0f), GetIntParam(node, "max_count", 0), GetParam(node, "status_id"), null, GetEnumParam(node, "target_side", SkillMultiEffectTargetSide.AllAllies));
		}
		if (string.Equals(handlerId, "CooldownMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.CooldownMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "MagazineBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.MagazineBonus, 0f, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ReloadTimeMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ReloadTimeMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "PierceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.PierceBonus, 0f, GetIntParam(node, "bonus", 0));
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
			return new SkillActionOp(SkillActionOpKind.AdditionalProjectileBonus, 0f, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ShotIntervalMultiplier, GetFloatParam(node, "multiplier", 1f));
		}
		if (string.Equals(handlerId, "ConsecutiveHitDamageBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ConsecutiveHitDamageBonus, GetFloatParam(node, "bonus_rate", 0f), 0, null, null, SkillMultiEffectTargetSide.Enemy, GetFloatParam(node, "max_bonus", 0f));
		}
		if (string.Equals(handlerId, "BranchDamage", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.BranchDamage, GetFloatParam(node, "chance_bonus", 0f), GetIntParam(node, "count", 0), null, null, SkillMultiEffectTargetSide.Enemy, GetFloatParam(node, "damage_multiplier", 0f), GetFloatParam(node, "search_radius", 0f));
		}
		if (string.Equals(handlerId, "StatusStackAmountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountBonus, 0f, GetIntParam(node, "bonus", 0), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "StatusStackAmountSet", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusStackAmountSet, 0f, GetIntParam(node, "value", 0), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "StatusMaxStacksBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusMaxStacksBonus, 0f, GetIntParam(node, "bonus", 0), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "ConditionalDamageMultiplier", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.ConditionalDamageMultiplier, GetFloatParam(node, "multiplier", 1f), GetIntParam(node, "min_stacks", 1), GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "TargetStatusStackDamageRateBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TargetStatusStackDamageRateBonus, GetFloatParam(node, "bonus_rate_per_stack", 0f), 0, GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "TriggerProcChanceBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.TriggerProcChanceBonus, GetFloatParam(node, "bonus", 0f), 0, GetParam(node, "trigger_id"));
		}
		if (string.Equals(handlerId, "HitTargetCountBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.HitTargetCountBonus, 0f, GetIntParam(node, "bonus", 0));
		}
		if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusActionSpeedBonus, GetFloatParam(node, "bonus", 0f), 0, GetParam(node, "status_id"));
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
			return new SkillActionOp(SkillActionOpKind.StatusDurationBonus, GetFloatParam(node, "bonus_seconds", 0f), 0, GetParam(node, "status_id"));
		}
		if (string.Equals(handlerId, "StatusConditionalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusConditionalDamageTakenBonus, GetFloatParam(node, "bonus", 0f), 0, GetParam(node, "source_status_id"));
		}
		if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusElementDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		if (string.Equals(handlerId, "StatusCriticalDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
		{
			return new SkillActionOp(SkillActionOpKind.StatusCriticalDamageTakenBonus, GetFloatParam(node, "bonus", 0f));
		}
		return null;
	}

	internal static string GetParam(SkillNodeDefinition node, string key)
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
				return skillNodeParamDefinition.Value ?? string.Empty;
			}
		}
		return string.Empty;
	}

	internal static float GetFloatParam(SkillNodeDefinition node, string key, float fallback)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !float.TryParse(param, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
		{
			return fallback;
		}
		return result;
	}

	internal static int GetIntParam(SkillNodeDefinition node, string key, int fallback)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !int.TryParse(param, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
		{
			return fallback;
		}
		return result;
	}

	internal static bool GetBoolParam(SkillNodeDefinition node, string key, bool fallback)
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param))
		{
			return fallback;
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

	internal static T GetEnumParam<T>(SkillNodeDefinition node, string key, T fallback) where T : struct
	{
		string param = GetParam(node, key);
		if (string.IsNullOrWhiteSpace(param) || !Enum.TryParse<T>(param, ignoreCase: true, out var result))
		{
			return fallback;
		}
		return result;
	}
    }
}
