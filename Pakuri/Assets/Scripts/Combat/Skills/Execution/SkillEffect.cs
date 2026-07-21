using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 효과 목록과 적중 후 추가 효과를 대상에게 적용한다.
 */
namespace Pakuri.InGame
{

static class SkillEffect
{
	public static bool Execute(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter)
	{
		return ExecuteFiltered(context, snapshot, effects, fallbackCenter, false, SkillMultiEffectTiming.OnCast, false);
	}

	public static bool ExecuteDirect(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, Vector2 fallbackCenter, bool scaleStatusDurationWithSnapshot = false)
	{
		if (effect == null)
		{
			return false;
		}
		return SkillNodeAction.ExecuteEffect(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot);
	}

	public static bool ExecuteWithStatusDurationScaling(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter)
	{
		return ExecuteFiltered(context, snapshot, effects, fallbackCenter, false, SkillMultiEffectTiming.OnCast, true);
	}

	public static bool ExecuteOnExpire(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter)
	{
		return ExecuteFiltered(context, snapshot, effects, fallbackCenter, true, SkillMultiEffectTiming.OnExpire, false);
	}

	public static bool ExecuteOnDeploymentCast(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter)
	{
		return ExecuteFiltered(context, snapshot, effects, fallbackCenter, true, SkillMultiEffectTiming.OnDeploymentCast, false);
	}

	public static bool ExecuteOnHit(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter, UnitCombatState eventTarget)
	{
		if (context == null)
		{
			return false;
		}
		return ExecuteFiltered(new SkillExecutionContext(context.CombatManager, context.Roster, context.CasterEntry, context.Runtime, eventTarget, context.HasManualAimDirection, context.ManualAimDirection, context.HasManualTargetPoint, context.ManualTargetPoint, context.RecastGeneration), snapshot, effects, fallbackCenter, true, SkillMultiEffectTiming.OnHit, false);
	}

	public static bool ExecuteOnHitCount(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter, int hitCount)
	{
		return ExecuteFiltered(context, snapshot, effects, fallbackCenter, true, SkillMultiEffectTiming.OnHitCount, false, hitCount);
	}

	private static bool ExecuteFiltered(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects, Vector2 fallbackCenter, bool hasRequiredTiming, SkillMultiEffectTiming requiredTiming, bool scaleStatusDurationWithSnapshot, int eventHitCount = 0)
	{
		if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
		{
			return false;
		}
		bool flag = false;
		foreach (SkillEffectDefinition skillEffectDefinition in effects)
		{
			if (!ShouldRun(context, skillEffectDefinition, snapshot))
			{
				continue;
			}
			if (hasRequiredTiming)
			{
				if (skillEffectDefinition.EffectTiming != requiredTiming)
				{
					continue;
				}
			}
			else if (skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.OnHit || skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.OnDeploymentCast || skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.OnExpire || skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.OnHitCount)
			{
				continue;
			}
			if (MatchesHitCountCondition(skillEffectDefinition, eventHitCount))
			{
				if (skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.Delayed || skillEffectDefinition.DelaySeconds > 0f)
				{
					context.CombatManager.StartCoroutine(ExecuteDelayed(context, snapshot, skillEffectDefinition, fallbackCenter, scaleStatusDurationWithSnapshot));
					flag = true;
				}
				else
				{
					flag = SkillNodeAction.ExecuteEffect(context, snapshot, skillEffectDefinition, fallbackCenter, scaleStatusDurationWithSnapshot) || flag;
				}
			}
		}
		return flag;
	}

	private static IEnumerator ExecuteDelayed(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, Vector2 fallbackCenter, bool scaleStatusDurationWithSnapshot)
	{
		float num = Mathf.Max(0f, effect.DelaySeconds);
		if (num > 0f)
		{
			yield return new WaitForSeconds(num);
		}
		else
		{
			yield return null;
		}
		SkillNodeAction.ExecuteEffect(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot);
	}

	public static bool ShouldRun(SkillExecutionContext context, SkillEffectDefinition effect, SkillSnapshot snapshot)
	{
		if (effect == null)
		{
			return false;
		}
		if (!effect.EnabledByDefault && string.IsNullOrWhiteSpace(effect.RequiresActiveChoiceId))
		{
			return false;
		}
		if (!SkillRequirement.HasAllActiveChoices(snapshot, effect.RequiresActiveChoiceId))
		{
			return false;
		}
		if (SkillRequirement.HasAnyActiveChoice(snapshot, effect.ExcludesActiveChoiceId))
		{
			return false;
		}
		if (!SkillRequirement.HasAllLearnedPassives(context.Caster, effect.RequiresPassiveSkillId))
		{
			return false;
		}
		if (!SkillRequirement.HasAnyLearnedPassive(context.Caster, effect.ExcludesPassiveSkillId))
		{
			return SkillRequirement.HasSourceStatus(context.Caster, effect.RequiredSourceStatusKind, effect.RequiredSourceStatusMinStacks);
		}
		return false;
	}

	public static bool ExecuteDamageEffectAction(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, Vector2 fallbackCenter)
	{
		SkillTargetingSpec skillTargetingSpec = BuildTargeting(effect);
		Vector2 vector = ResolveEffectCenter(context, effect, skillTargetingSpec, fallbackCenter);
		var statCoefficient = effect.AttackPowerCoefficient;
		var statSource = StatSource.Attack;
		if (Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient))
		{
			statCoefficient = effect.SpellPowerCoefficient;
			statSource = StatSource.Intelligence;
		}
		SkillDamageSpec skillDamageSpec = new SkillDamageSpec
		{
			SkillId = effect.SkillId,
			Element = effect.Attribute,
			BaseDamage = effect.BaseDamage,
			StatCoefficient = statCoefficient,
			StatSource = statSource,
			CriticalAllowed = true
		};
		float num = DamageCalculator.ResolveDamage(context.Caster, skillDamageSpec, snapshot) * Mathf.Max(0f, effect.DamageMultiplier);
		ProjectileStatusHitSpec projectileStatusHitSpec = ResolveStatusSpec(effect, snapshot);
		if (HasPersistentZone(effect))
		{
			return SpawnPersistentDamageZone(context, snapshot, effect, skillTargetingSpec, vector, num, projectileStatusHitSpec);
		}
		if (TryExecuteRuntimeHitboxDamageEffect(context, snapshot, effect, skillTargetingSpec, vector, num, projectileStatusHitSpec, skillDamageSpec.CriticalAllowed, out var routed))
		{
			return routed;
		}
		UnitCombatState unitState = ResolveExplicitEventTarget(context, effect);
		var criticalChanceBonus = 0f;
		var criticalDamageBonus = 0f;
		if (snapshot != null)
		{
			criticalChanceBonus = snapshot.CritChanceBonus;
			criticalDamageBonus = snapshot.CritDamageBonus;
		}

		var effectId = effect.SkillId;
		if (!string.IsNullOrWhiteSpace(effect.EffectId))
		{
			effectId = effect.EffectId;
		}

		if (unitState != null)
		{
			float baseDamage = DamageCalculator.ResolveDamageAgainstTarget(num, snapshot, unitState);
			InGameResourceChangeResult damageResult = context.CombatManager.ApplyDamage(unitState, baseDamage, effect.Attribute, context.Caster, skillDamageSpec.CriticalAllowed, criticalChanceBonus, criticalDamageBonus, effectId);
			if (!damageResult.IsDead)
			{
				StatusCombatRules.ApplyStatus(context.CombatManager, unitState, projectileStatusHitSpec, context.Caster);
			}
			EffectManager effects = context.CombatManager.Effects;
			if (effects != null)
			{
				effects.SpawnEffectVisual(effect, vector, 1f);
			}
			return true;
		}
		bool num2 = ZoneSkillActor.ApplyAreaTick(context.CombatManager, context.CasterEntry, context.Roster, skillTargetingSpec, vector, ResolveRadius(effect, snapshot), effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield, num, effect.Attribute, projectileStatusHitSpec, context.Caster, effectId, null, skillDamageSpec.CriticalAllowed, criticalChanceBonus, criticalDamageBonus);
		if (num2)
		{
			EffectManager effects2 = context.CombatManager.Effects;
			if (effects2 != null)
			{
				effects2.SpawnEffectVisual(effect, vector, 1f);
			}
		}
		return num2;
	}

	private static bool TryExecuteRuntimeHitboxDamageEffect(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, SkillTargetingSpec targeting, Vector2 center, float damage, ProjectileStatusHitSpec statusSpec, bool criticalAllowed, out bool routed)
	{
		routed = false;
		RuntimeSkillVisualSpec runtimeSkillVisualSpec = effect.RuntimeVisual;
		RuntimeSkillHitboxSpec runtimeSkillHitboxSpec = null;
		if (runtimeSkillVisualSpec != null)
		{
			runtimeSkillHitboxSpec = runtimeSkillVisualSpec.Hitbox;
		}
		if (runtimeSkillHitboxSpec == null || !runtimeSkillHitboxSpec.HasHitbox())
		{
			return false;
		}
		string text = effect.SkillId;
		if (!string.IsNullOrWhiteSpace(effect.EffectId))
		{
			text = effect.EffectId;
		}
		if (context == null || context.CombatManager == null || context.CombatManager.Effects == null)
		{
			Debug.LogError("Runtime hitbox effect '" + text + "' could not create its RuntimeEffectVisual.");
			return true;
		}
		var objectName = "SkillEffectVisual";
		if (!string.IsNullOrWhiteSpace(effect.EffectId))
		{
			objectName = "SkillEffectVisual_" + effect.EffectId;
		}
		GameObject gameObject = context.CombatManager.Effects.SpawnAreaEffect(runtimeSkillVisualSpec, null, objectName, center, effect.Radius, snapshot, 1f, requireHitbox: true);
		if (gameObject == null)
		{
			Debug.LogError("Runtime hitbox effect '" + text + "' failed to create its RuntimeEffectVisual.");
			return true;
		}
		Collider2D[] componentsInChildren = gameObject.GetComponentsInChildren<Collider2D>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			Debug.LogError("Runtime hitbox effect '" + text + "' created no Collider2D components.");
			return true;
		}
		var criticalChanceBonus = 0f;
		var criticalDamageBonus = 0f;
		if (snapshot != null)
		{
			criticalChanceBonus = snapshot.CritChanceBonus;
			criticalDamageBonus = snapshot.CritDamageBonus;
		}
		routed = ZoneSkillActor.ApplyColliderAreaTick(context.CombatManager, context.CasterEntry, context.Roster, targeting, componentsInChildren, int.MaxValue, damage, effect.Attribute, statusSpec, context.Caster, text, null, criticalAllowed, criticalChanceBonus, criticalDamageBonus, null);
		return true;
	}

	public static bool ExecuteExtendStatusDurationEffectAction(SkillExecutionContext context, SkillEffectDefinition effect)
	{
		if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || effect == null)
		{
			return false;
		}
		var kind = effect.StatusKind;
		float num = Mathf.Max(0f, effect.StatusDurationSeconds);
		if (num <= 0f)
		{
			return false;
		}
		SkillTargetingSpec targeting = BuildTargeting(effect);
		IReadOnlyList<CombatUnitEntry> readOnlyList = SkillTargeting.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
		bool flag = false;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatUnitEntry unitEntry = readOnlyList[i];
			if (unitEntry != null && unitEntry.IsAlive && unitEntry.Model != null)
			{
				flag = context.CombatManager.ExtendStatusDuration(unitEntry.Model, kind, num) || flag;
			}
		}
		return flag;
	}

	public static bool ExecuteStatusEffectAction(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, Vector2 fallbackCenter, bool scaleStatusDurationWithSnapshot)
	{
		ProjectileStatusHitSpec projectileStatusHitSpec = ResolveStatusSpec(effect, snapshot, scaleStatusDurationWithSnapshot);
		if (projectileStatusHitSpec == null || !projectileStatusHitSpec.Enabled)
		{
			return false;
		}
		SkillTargetingSpec targeting = BuildTargeting(effect);
		IReadOnlyList<CombatUnitEntry> readOnlyList = ResolveStatusTargets(context, effect, targeting);
		List<CombatUnitEntry> list = null;
		if (effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets)
		{
			list = new List<CombatUnitEntry>();
		}
		bool flag = false;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatUnitEntry unitEntry = readOnlyList[i];
			if (unitEntry != null && unitEntry.IsAlive && unitEntry.Model != null && TargetMatchesCondition(unitEntry.Model, effect))
			{
				if (projectileStatusHitSpec.StatusData != null && projectileStatusHitSpec.StatusData.Kind == StatusEffectKind.Shield)
				{
					context.CombatManager.ApplyShieldStatus(unitEntry.Model, projectileStatusHitSpec.StatusData, ResolveStatusEffectShieldAmount(context.Caster, effect, snapshot), projectileStatusHitSpec.DurationSeconds, projectileStatusHitSpec.Stacks, projectileStatusHitSpec.MaxStacks, projectileStatusHitSpec.Permanent, projectileStatusHitSpec.RefreshDuration, context.Caster);
				}
				else if (!StatusCombatRules.ApplyStatus(context.CombatManager, unitEntry.Model, projectileStatusHitSpec, context.Caster))
				{
					continue;
				}
				if (list != null)
				{
					list.Add(unitEntry);
				}
				flag = true;
			}
		}
		if (flag)
		{
			if (list != null)
			{
				EffectManager effects = context.CombatManager.Effects;
				if (effects != null)
				{
					effects.SpawnEffectVisualOnTargets(effect, list, projectileStatusHitSpec.DurationSeconds);
				}
			}
			else
			{
				EffectManager effects2 = context.CombatManager.Effects;
				if (effects2 != null)
				{
					effects2.SpawnEffectVisual(effect, ResolveEffectCenter(context, effect, targeting, fallbackCenter), 1f);
				}
			}
		}
		return flag;
	}

	public static List<CombatUnitEntry> ResolvePassiveStatusTargets(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect)
	{
		List<CombatUnitEntry> list = new List<CombatUnitEntry>();
		if (context == null || effect == null || effect.EffectKind != SkillMultiEffectKind.Status || !ShouldRun(context, effect, snapshot))
		{
			return list;
		}
		SkillTargetingSpec targeting = BuildTargeting(effect);
		IReadOnlyList<CombatUnitEntry> readOnlyList = ResolveStatusTargets(context, effect, targeting);
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatUnitEntry unitEntry = readOnlyList[i];
			if (unitEntry != null && unitEntry.IsAlive && unitEntry.Model != null && TargetMatchesCondition(unitEntry.Model, effect))
			{
				list.Add(unitEntry);
			}
		}
		return list;
	}

	public static bool ApplyPersistentPassiveStatus(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, CombatUnitEntry target, Vector2 fallbackCenter)
	{
		if (context == null || context.CombatManager == null || effect == null || target == null || !target.IsAlive || target.Model == null || !TargetMatchesCondition(target.Model, effect))
		{
			return false;
		}
		ProjectileStatusHitSpec projectileStatusHitSpec = ResolveStatusSpec(effect, snapshot);
		if (projectileStatusHitSpec == null || !projectileStatusHitSpec.Enabled || projectileStatusHitSpec.StatusData == null)
		{
			return false;
		}
		float durationSeconds = projectileStatusHitSpec.DurationSeconds;
		projectileStatusHitSpec.Permanent = true;
		projectileStatusHitSpec.DurationSeconds = 0f;
		projectileStatusHitSpec.RefreshDuration = false;
		var applied = false;
		if (projectileStatusHitSpec.StatusData.Kind == StatusEffectKind.Shield)
		{
			var appliedShield = context.CombatManager.ApplyShieldStatus(target.Model, projectileStatusHitSpec.StatusData, ResolveStatusEffectShieldAmount(context.Caster, effect, snapshot), projectileStatusHitSpec.DurationSeconds, projectileStatusHitSpec.Stacks, projectileStatusHitSpec.MaxStacks, projectileStatusHitSpec.Permanent, projectileStatusHitSpec.RefreshDuration, context.Caster);
			applied = appliedShield != null;
		}
		else
		{
			applied = StatusCombatRules.ApplyStatus(context.CombatManager, target.Model, projectileStatusHitSpec, context.Caster);
		}
		if (!applied)
		{
			return false;
		}
		if (effect.VisualAnchorMode == SkillMultiEffectVisualAnchorMode.AppliedTargets)
		{
			EffectManager effects = context.CombatManager.Effects;
			if (effects != null)
			{
				effects.SpawnEffectVisualOnTargets(effect, new CombatUnitEntry[1] { target }, durationSeconds);
			}
		}
		else
		{
			SkillTargetingSpec targeting = BuildTargeting(effect);
			EffectManager effects2 = context.CombatManager.Effects;
			if (effects2 != null)
			{
				effects2.SpawnEffectVisual(effect, ResolveEffectCenter(context, effect, targeting, fallbackCenter), 1f);
			}
		}
		return true;
	}

	private static IReadOnlyList<CombatUnitEntry> ResolveStatusTargets(SkillExecutionContext context, SkillEffectDefinition effect, SkillTargetingSpec targeting)
	{
		UnitCombatState unitState = ResolveExplicitEventTarget(context, effect);
		CombatUnitEntry unitEntry = null;
		if (unitState != null && context != null && context.Roster != null)
		{
			unitEntry = context.Roster.Find(unitState);
		}
		if (unitEntry == null)
		{
			return SkillTargeting.ResolveTargetList(context.CasterEntry, context.Roster, targeting);
		}
		return new List<CombatUnitEntry> { unitEntry };
	}

	public static bool TargetMatchesCondition(UnitCombatState target, SkillEffectDefinition effect)
	{
		if (effect == null)
		{
			return true;
		}
		bool flag = true;
		if (!string.IsNullOrWhiteSpace(effect.ConditionStatusId))
		{
			flag = StatusConditionRules.MatchesConditionStatus(target, effect.ConditionStatuses, effect.ConditionStatusSourceSkillIds);
		}
		bool flag2 = true;
		if (!string.IsNullOrWhiteSpace(effect.ConditionSkillAttribute))
		{
			flag2 = HasActiveSkillAttribute(target, effect.ConditionSkillAttribute);
		}
		bool flag3 = true;
		if (effect.ConditionHealthRatioMax > 0f)
		{
			flag3 = IsWithinHealthRatio(target, effect.ConditionHealthRatioMax);
		}
		return flag && flag2 && flag3;
	}

	private static bool MatchesHitCountCondition(SkillEffectDefinition effect, int hitCount)
	{
		if (effect != null && effect.ConditionHitCountMin > 0)
		{
			return hitCount >= effect.ConditionHitCountMin;
		}
		return true;
	}

	private static bool IsWithinHealthRatio(UnitCombatState target, float maxRatio)
	{
		UnitCombatResources unitResourceRuntime = null;
		UnitCombatStats unitStatsRuntime = null;
		if (target != null)
		{
			unitResourceRuntime = target.Resources;
			unitStatsRuntime = target.Stats;
		}
		if (unitResourceRuntime != null && unitStatsRuntime != null && unitStatsRuntime.MaxHealth > 0f)
		{
			return unitResourceRuntime.CurrentHealth / unitStatsRuntime.MaxHealth <= Mathf.Clamp01(maxRatio);
		}
		return false;
	}

	private static float ResolveStatusDurationBonus(SkillSnapshot snapshot, StatusRuntimeData statusData, StatusEffectKind kind)
	{
		if (snapshot == null)
		{
			return 0f;
		}

		return snapshot.ResolveStatusDurationBonus(statusData.StatusTag);
	}

	public static ProjectileStatusHitSpec ResolveStatusSpec(SkillEffectDefinition effect, SkillSnapshot snapshot = null, bool scaleDurationWithSnapshot = false)
	{
		StatusRuntimeData runtimeStatusData = effect.CompiledStatusData;
		runtimeStatusData = SkillStatus.ResolveStatusData(runtimeStatusData, runtimeStatusData.Kind, snapshot);
		float num = runtimeStatusData.Duration;
		float num2 = ResolveStatusDurationBonus(snapshot, runtimeStatusData, runtimeStatusData.Kind);
		if (!Mathf.Approximately(num2, 0f))
		{
			num = Mathf.Max(0f, num + num2);
		}
		if (scaleDurationWithSnapshot && snapshot != null)
		{
			num = num * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
		}
		return new ProjectileStatusHitSpec
		{
			Enabled = true,
			Kind = runtimeStatusData.Kind,
			StatusData = runtimeStatusData,
			Chance = Mathf.Clamp01(effect.StatusChance),
			Stacks = effect.StatusStackAmount,
			DurationSeconds = num,
			MaxStacks = runtimeStatusData.MaxStacks,
			Permanent = runtimeStatusData.Permanent,
			RefreshDuration = true
		};
	}

	private static bool HasActiveSkillAttribute(UnitCombatState target, string rawAttribute)
	{
		if (target == null || target.SkillRuntime == null || string.IsNullOrWhiteSpace(rawAttribute) || !Enum.TryParse<DamageAttribute>(rawAttribute.Trim(), ignoreCase: true, out var result))
		{
			return false;
		}
		IReadOnlyList<SkillRuntimeInstance> activeSkills = target.SkillRuntime.ActiveSkills;
		for (int i = 0; i < activeSkills.Count; i++)
		{
			SkillRuntimeInstance skillRuntimeInstance = activeSkills[i];
			if (skillRuntimeInstance != null && skillRuntimeInstance.Data != null && skillRuntimeInstance.Data.Element == result)
			{
				return true;
			}
		}
		return false;
	}

	private static float ResolveStatusEffectShieldAmount(UnitCombatState caster, SkillEffectDefinition effect, SkillSnapshot snapshot)
	{
		if (effect == null)
		{
			return 0f;
		}
		bool flag = Mathf.Abs(effect.SpellPowerCoefficient) >= Mathf.Abs(effect.AttackPowerCoefficient);
		UnitCombatStats unitStatsRuntime = null;
		if (caster != null)
		{
			unitStatsRuntime = caster.Stats;
		}
		float num = 0f;
		if (unitStatsRuntime != null)
		{
			if (flag)
			{
				num = unitStatsRuntime.SpellPower * StatusCombatRules.ResolveSpellPowerMultiplier(caster);
			}
			else
			{
				num = unitStatsRuntime.AttackPower * StatusCombatRules.ResolveAttackPowerMultiplier(caster);
			}
		}
		float num2 = effect.AttackPowerCoefficient;
		if (flag)
		{
			num2 = effect.SpellPowerCoefficient;
		}
		float num3 = (effect.BaseDamage + num * num2) * Mathf.Max(0f, effect.DamageMultiplier);
		if (snapshot != null)
		{
			num3 = (num3 + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
		}
		return Mathf.Max(0f, num3);
	}

	private static SkillTargetingSpec BuildTargeting(SkillEffectDefinition effect)
	{
		return new SkillTargetingSpec
		{
			TargetSide = MapTargetSide(effect.TargetSide),
			Selection = MapTargetSelection(effect.TargetSelection),
			Shape = MapTargetShape(effect.TargetShape),
			Radius = effect.Radius,
			CoverAll = (effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield)
		};
	}

	private static SkillTargetSide MapTargetSide(SkillMultiEffectTargetSide side)
	{
		return side switch
		{
			SkillMultiEffectTargetSide.Self => SkillTargetSide.Self, 
			SkillMultiEffectTargetSide.AllAllies => SkillTargetSide.AllAllies, 
			_ => SkillTargetSide.Enemy, 
		};
	}

	private static SkillTargetSelection MapTargetSelection(SkillMultiEffectTargetSelection selection)
	{
		return selection switch
		{
			SkillMultiEffectTargetSelection.Owner => SkillTargetSelection.Owner, 
			SkillMultiEffectTargetSelection.EventTarget => SkillTargetSelection.Nearest, 
			_ => SkillTargetSelection.Nearest, 
		};
	}

	private static SkillTargetShape MapTargetShape(SkillMultiEffectTargetShape shape)
	{
		return shape switch
		{
			SkillMultiEffectTargetShape.Battlefield => SkillTargetShape.Battlefield, 
			SkillMultiEffectTargetShape.Single => SkillTargetShape.Single, 
			_ => SkillTargetShape.Circle, 
		};
	}

	private static Vector2 ResolveEffectCenter(SkillExecutionContext context, SkillEffectDefinition effect, SkillTargetingSpec targeting, Vector2 fallbackCenter)
	{
		if (effect != null)
		{
			switch (effect.CenterMode)
			{
			case SkillMultiEffectCenterMode.EffectTarget:
				if (context != null && context.EventTarget != null)
				{
					CombatUnitEntry unitEntry2 = null;
					if (context.Roster != null)
					{
						unitEntry2 = context.Roster.Find(context.EventTarget);
					}
					if (unitEntry2 != null && unitEntry2.Transform != null)
					{
						return unitEntry2.Transform.position;
					}
				}
				return fallbackCenter;
			case SkillMultiEffectCenterMode.PrimarySkillCenter:
				return fallbackCenter;
			case SkillMultiEffectCenterMode.Caster:
				if (context == null || context.CasterEntry == null || !(context.CasterEntry.Transform != null))
				{
					return fallbackCenter;
				}
				return context.CasterEntry.Transform.position;
			case SkillMultiEffectCenterMode.NearestEnemy:
			{
				SkillTargetingSpec targeting2 = new SkillTargetingSpec
				{
					TargetSide = SkillTargetSide.Enemy,
					Selection = SkillTargetSelection.Nearest,
					Shape = SkillTargetShape.Circle,
					Radius = effect.Radius,
					CoverAll = false
				};
				CombatUnitEntry unitEntry = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, targeting2);
				if (unitEntry == null || !(unitEntry.Transform != null))
				{
					return fallbackCenter;
				}
				return unitEntry.Transform.position;
			}
			}
		}
		CombatUnitEntry unitEntry3 = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, targeting);
		if (unitEntry3 != null && unitEntry3.Transform != null)
		{
			return unitEntry3.Transform.position;
		}
		return fallbackCenter;
	}

	private static bool HasPersistentZone(SkillEffectDefinition effect)
	{
		if (effect != null && effect.ActiveDurationSeconds > 0f)
		{
			return effect.TickIntervalSeconds > 0f;
		}
		return false;
	}

	private static UnitCombatState ResolveExplicitEventTarget(SkillExecutionContext context, SkillEffectDefinition effect)
	{
		if (effect == null || effect.TargetSelection != SkillMultiEffectTargetSelection.EventTarget)
		{
			return null;
		}
		if (context == null)
		{
			return null;
		}

		return context.EventTarget;
	}

	private static bool SpawnPersistentDamageZone(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition effect, SkillTargetingSpec targeting, Vector2 center, float damage, ProjectileStatusHitSpec statusSpec)
	{
		if (context == null || context.CombatManager == null || context.CombatManager.Effects == null || context.CasterEntry == null || context.Roster == null)
		{
			return false;
		}
		float num = effect.ActiveDurationSeconds;
		if (snapshot != null)
		{
			num = num * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
		}
		float num2 = effect.TickIntervalSeconds;
		if (snapshot != null)
		{
			num2 *= Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier);
		}
		num = Mathf.Max(0.05f, num);
		num2 = Mathf.Max(0.05f, num2);
		bool areaCoversAll = effect.CoverAll || effect.TargetShape == SkillMultiEffectTargetShape.Battlefield;
		float areaRadius = ResolveRadius(effect, snapshot);
		var objectName = "SkillEffectZone";
		if (!string.IsNullOrWhiteSpace(effect.EffectId))
		{
			objectName = "SkillEffectZone_" + effect.EffectId;
		}
		GameObject gameObject = context.CombatManager.Effects.CreateAreaEffectObject(effect.RuntimeVisual, effect.SkillEffectPrefab, objectName, center, effect.Radius, snapshot, createEmptyObject: true);
		ZoneSkillActor zoneSkillActor = gameObject.GetComponent<ZoneSkillActor>();
		if (zoneSkillActor == null)
		{
			zoneSkillActor = gameObject.AddComponent<ZoneSkillActor>();
		}
		var criticalChanceBonus = 0f;
		var criticalDamageBonus = 0f;
		if (snapshot != null)
		{
			criticalChanceBonus = snapshot.CritChanceBonus;
			criticalDamageBonus = snapshot.CritDamageBonus;
		}
		zoneSkillActor.Initialize(context.CombatManager, context.CasterEntry, context.Roster, targeting, center, areaRadius, areaCoversAll, num, num2, int.MaxValue, damage, effect.Attribute, statusSpec, context.Runtime, snapshot, Array.Empty<SkillEffectDefinition>(), context.Caster, allowCritical: true, criticalChanceBonus, criticalDamageBonus);
		return true;
	}

	private static float ResolveRadius(SkillEffectDefinition effect, SkillSnapshot snapshot)
	{
		var radius = 0f;
		if (effect != null)
		{
			radius = effect.Radius;
		}

		return SkillTargeting.ResolveRadius(radius, snapshot);
	}
}

static class SkillOnHitEffect
{
	private const string HitTarget = "HitTarget";

	private static bool applyingAdditionalDamage;

	public static void TryApply(InGameCombatManager manager, CombatUnitRegistry roster, SkillRuntimeInstance runtime, SkillSnapshot snapshot, CombatUnitEntry sourceEntry, UnitCombatState source, string sourceSkillId, CombatUnitEntry hitTarget, Vector2 hitPosition, float primaryBaseDamage)
	{
		if (manager == null || roster == null || snapshot == null || (!snapshot.HasOnHitAdditionalDamageBehavior && !HasReloadReductionBehavior(snapshot)) || source == null || hitTarget == null || hitTarget.Model == null || primaryBaseDamage <= 0f || applyingAdditionalDamage)
		{
			return;
		}
		int hitIndex = 0;
		if (runtime != null)
		{
			hitIndex = runtime.AdvanceSkillHitCount();
		}
		applyingAdditionalDamage = true;
		try
		{
			ApplyReloadReduction(runtime, snapshot);
			ApplyHitTargetDamage(manager, snapshot, source, sourceSkillId, hitTarget, primaryBaseDamage);
			ApplyChainDamage(manager, roster, snapshot, sourceEntry, source, sourceSkillId, hitTarget, hitPosition, primaryBaseDamage, hitIndex);
		}
		finally
		{
			applyingAdditionalDamage = false;
		}
	}

	private static void ApplyHitTargetDamage(InGameCombatManager manager, SkillSnapshot snapshot, UnitCombatState source, string sourceSkillId, CombatUnitEntry hitTarget, float primaryBaseDamage)
	{
		if (snapshot.HasOnHitAdditionalDamage && !(snapshot.OnHitAdditionalDamageMultiplier <= 0f) && TargetsHitTarget(snapshot.OnHitAdditionalDamageTarget) && hitTarget != null && hitTarget.IsAlive && hitTarget.Model != null && !(UnityEngine.Random.value > Mathf.Clamp01(snapshot.OnHitAdditionalDamageChance)))
		{
			manager.ApplyDamage(hitTarget.Model, primaryBaseDamage * snapshot.OnHitAdditionalDamageMultiplier, snapshot.OnHitAdditionalDamageAttribute, source, criticalAllowed: false, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: true);
		}
	}

	private static void ApplyChainDamage(InGameCombatManager manager, CombatUnitRegistry roster, SkillSnapshot snapshot, CombatUnitEntry sourceEntry, UnitCombatState source, string sourceSkillId, CombatUnitEntry hitTarget, Vector2 hitPosition, float primaryBaseDamage, int hitIndex)
	{
		if (!snapshot.HasOnHitChainDamageBehavior || hitIndex <= 0 || hitIndex % snapshot.OnHitChainHitPeriod != 0)
		{
			return;
		}
		List<CombatUnitEntry> list = ResolveChainTargets(roster, sourceEntry, source, hitTarget, hitPosition, snapshot.OnHitChainSearchRadius);
		int num = Mathf.Min(snapshot.OnHitChainTargetCount, list.Count);
		for (int i = 0; i < num; i++)
		{
			CombatUnitEntry unitEntry = list[i];
			if (unitEntry != null && unitEntry.IsAlive && unitEntry.Model != null)
			{
				manager.ApplyDamage(unitEntry.Model, primaryBaseDamage * snapshot.OnHitChainDamageMultiplier, snapshot.OnHitChainDamageAttribute, source, criticalAllowed: false, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: true);
			}
		}
	}

	private static List<CombatUnitEntry> ResolveChainTargets(CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source, CombatUnitEntry hitTarget, Vector2 hitPosition, float searchRadius)
	{
		List<CombatUnitEntry> list = new List<CombatUnitEntry>();
		if (roster == null || source == null || searchRadius <= 0f)
		{
			return list;
		}
		UnitCombatState hitTargetModel = null;
		if (hitTarget != null)
		{
			hitTargetModel = hitTarget.Model;
		}
		string text = ResolveUnitId(hitTargetModel);
		IReadOnlyList<CombatUnitEntry> readOnlyList = ResolveOpposingEntries(roster, sourceEntry, source);
		float num = searchRadius * searchRadius;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatUnitEntry unitEntry = readOnlyList[i];
			if (unitEntry == null || !unitEntry.IsAlive || unitEntry.Model == null || unitEntry.Transform == null)
			{
				continue;
			}
			UnitIdentity identity = unitEntry.Model.Identity;
			if (identity == null || identity.Role != UnitRole.Nexus)
			{
				string text2 = ResolveUnitId(unitEntry.Model);
				if ((string.IsNullOrWhiteSpace(text) || !(text2 == text)) && unitEntry.Model != hitTargetModel && ((Vector2)unitEntry.Transform.position - hitPosition).sqrMagnitude <= num)
				{
					list.Add(unitEntry);
				}
			}
		}
		list.Sort(delegate(CombatUnitEntry left, CombatUnitEntry right)
		{
			float sqrMagnitude = ((Vector2)left.Transform.position - hitPosition).sqrMagnitude;
			float sqrMagnitude2 = ((Vector2)right.Transform.position - hitPosition).sqrMagnitude;
			return sqrMagnitude.CompareTo(sqrMagnitude2);
		});
		return list;
	}

	private static IReadOnlyList<CombatUnitEntry> ResolveOpposingEntries(CombatUnitRegistry roster, CombatUnitEntry sourceEntry, UnitCombatState source)
	{
		var sourceSide = UnitSide.Player;
		if (source.Identity != null)
		{
			sourceSide = source.Identity.Side;
		}
		else if (sourceEntry != null && sourceEntry.Model != null && sourceEntry.Model.Identity != null)
		{
			sourceSide = sourceEntry.Model.Identity.Side;
		}
		if (sourceSide != UnitSide.Enemy)
		{
			return roster.Enemies;
		}
		return roster.Players;
	}

	private static string ResolveUnitId(UnitCombatState model)
	{
		if (model == null || model.Identity == null)
		{
			return string.Empty;
		}
		return model.Identity.UnitId;
	}

	private static bool TargetsHitTarget(string target)
	{
		if (!string.IsNullOrWhiteSpace(target))
		{
			return string.Equals(target, "HitTarget", StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private static bool HasReloadReductionBehavior(SkillSnapshot snapshot)
	{
		if (snapshot != null && !string.IsNullOrWhiteSpace(snapshot.ReloadReduceTargetSkillId))
		{
			return snapshot.ReloadReduceSecondsPerHit > 0f;
		}
		return false;
	}

	private static void ApplyReloadReduction(SkillRuntimeInstance runtime, SkillSnapshot snapshot)
	{
		if (runtime != null && runtime.Owner != null && runtime.Owner.SkillRuntime != null && HasReloadReductionBehavior(snapshot))
		{
			SkillRuntimeInstance skillRuntimeInstance = runtime.Owner.SkillRuntime.FindBySkillId(snapshot.ReloadReduceTargetSkillId);
			if (skillRuntimeInstance != null && skillRuntimeInstance.IsReloading)
			{
				skillRuntimeInstance.ReduceReloadRemaining(snapshot.ReloadReduceSecondsPerHit);
			}
		}
	}
}

/*
 * 스킬 상태 설정에 선택지 보정값을 반영한다.
 */
static class SkillStatus
{
    public static ProjectileStatusHitSpec ResolveStatusSpec(
        StatusApplicationSpec baseStatus,
        SkillSnapshot snapshot)
    {
        StatusRuntimeData statusData = null;
        if (baseStatus != null)
        {
            statusData = baseStatus.Status;
        }

        if (statusData == null)
        {
            return null;
        }

        var kind = statusData.Kind;
        var stacks = 1;
        var chance = 1f;
        var refreshDuration = true;
        if (baseStatus != null)
        {
            stacks = Math.Max(0, baseStatus.Stacks);
            chance = Mathf.Clamp01(baseStatus.Chance);
            refreshDuration = baseStatus.RefreshDuration;
        }

        if (snapshot != null)
        {
            chance = Mathf.Clamp01(chance + snapshot.StatusChanceBonus);
            if (snapshot.HasStatusStacksSet)
            {
                stacks = Math.Max(0, snapshot.StatusStacksSet);
            }
            else
            {
                stacks = Math.Max(0, stacks + snapshot.StatusStacksBonus);
            }
        }

        if (stacks <= 0 || chance <= 0f)
        {
            return null;
        }

        if (statusData == null || statusData.Kind != kind)
        {
            statusData = StatusDataCompiler.Create(kind, null);
        }

        var resolvedStatusData = ResolveStatusData(statusData, kind, snapshot);
        var duration = resolvedStatusData.Duration;
        var maxStacks = resolvedStatusData.MaxStacks;
        var maxStacksBonus = ResolveStatusMaxStacksBonus(snapshot, resolvedStatusData);
        if (maxStacksBonus != 0)
        {
            maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
        }

        var permanent = resolvedStatusData.Permanent;
        if (snapshot != null
            && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
        {
            duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            if (duration > 0f)
            {
                permanent = false;
            }
        }

        var durationBonus = ResolveStatusDurationBonus(snapshot, resolvedStatusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
            if (duration > 0f)
            {
                permanent = false;
            }
        }

        var thresholdStatusKind = StatusEffectKind.None;
        var thresholdStatusMinStacks = 0;
        if (snapshot != null)
        {
            thresholdStatusKind = snapshot.ThresholdStatusKind;
            thresholdStatusMinStacks = snapshot.ThresholdStatusMinStacks;
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = resolvedStatusData,
            Chance = chance,
            Stacks = stacks,
            DurationSeconds = duration,
            MaxStacks = maxStacks,
            Permanent = permanent,
            RefreshDuration = refreshDuration,
            ThresholdSourceStatusKind = thresholdStatusKind,
            ThresholdSourceMinStacks = thresholdStatusMinStacks,
            ThresholdStatusSpec = ResolveThresholdStatusSpec(snapshot)
        };
    }

    public static ProjectileStatusHitSpec CreateDirectStatusSpec(
        StatusEffectKind kind,
        int stacks,
        SkillSnapshot snapshot)
    {
        if (kind == StatusEffectKind.None || stacks <= 0)
        {
            return null;
        }

        var statusData = StatusDataCompiler.Create(kind, null);
        statusData = ResolveStatusData(statusData, kind, snapshot);
        var duration = statusData.Duration;
        var durationBonus = ResolveStatusDurationBonus(snapshot, statusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
        }

        var maxStacks = statusData.MaxStacks;
        var maxStacksBonus = ResolveStatusMaxStacksBonus(snapshot, statusData);
        if (maxStacksBonus != 0)
        {
            maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = statusData,
            Chance = 1f,
            Stacks = stacks,
            DurationSeconds = duration,
            MaxStacks = maxStacks,
            Permanent = statusData.Permanent && duration <= 0f,
            RefreshDuration = true
        };
    }

    public static StatusRuntimeData ResolveStatusData(
        StatusRuntimeData statusData,
        StatusEffectKind kind,
        SkillSnapshot snapshot)
    {
        if (snapshot == null)
        {
            return statusData;
        }

        var actionSpeedBonus = snapshot.ResolveStatusActionSpeedBonus(statusData.StatusTag);
        var hasActionSpeedBonus = !Mathf.Approximately(actionSpeedBonus, 0f);
        var hasOverride = snapshot.HasStatusElementDamageTakenBonus
            || snapshot.HasStatusCriticalDamageTakenBonus
            || snapshot.HasStatusAilmentResistanceBonus
            || snapshot.HasStatusDamageBonusRate
            || snapshot.HasStatusShieldReceivedBonus
            || snapshot.HasStatusCriticalChanceBonus
            || snapshot.HasStatusDamageTakenBonus
            || snapshot.HasStatusFlatElementResistReduction
            || snapshot.HasStatusConditionalDamageTakenBonus
            || snapshot.HasStatusAttackPowerBonus
            || hasActionSpeedBonus;
        if (!hasOverride)
        {
            return statusData;
        }

        var resolvedStatus = statusData.Clone();
        if (snapshot.HasStatusElementDamageTakenBonus)
        {
            resolvedStatus.ElementDamageTakenBonus += snapshot.StatusElementDamageTakenBonus;
        }

        if (snapshot.HasStatusCriticalDamageTakenBonus)
        {
            resolvedStatus.CriticalDamageTakenBonus += snapshot.StatusCriticalDamageTakenBonus;
        }

        if (snapshot.HasStatusAilmentResistanceBonus)
        {
            resolvedStatus.AilmentResistanceBonus += snapshot.StatusAilmentResistanceBonus;
        }

        if (snapshot.HasStatusDamageBonusRate)
        {
            resolvedStatus.Modifiers.DamageBonusRate += snapshot.StatusDamageBonusRate;
        }

        if (snapshot.HasStatusShieldReceivedBonus)
        {
            resolvedStatus.Modifiers.ShieldReceivedBonus += snapshot.StatusShieldReceivedBonus;
        }

        if (snapshot.HasStatusCriticalChanceBonus)
        {
            resolvedStatus.Modifiers.CritChanceBonusRate += snapshot.StatusCriticalChanceBonus;
        }

        if (snapshot.HasStatusDamageTakenBonus)
        {
            resolvedStatus.DamageTakenBonus += snapshot.StatusDamageTakenBonus;
        }

        if (snapshot.HasStatusFlatElementResistReduction)
        {
            resolvedStatus.FlatElementResistReduction += snapshot.StatusFlatElementResistReduction;
        }

        if (snapshot.HasStatusConditionalDamageTakenBonus)
        {
            resolvedStatus.ConditionalSourceStatusKind = snapshot.StatusConditionalSourceStatusKind;
            resolvedStatus.ConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus;
        }

        if (hasActionSpeedBonus)
        {
            resolvedStatus.Modifiers.ActionSpeedBonus += actionSpeedBonus;
        }

        if (snapshot.HasStatusAttackPowerBonus)
        {
            resolvedStatus.Modifiers.AttackPowerBonus += snapshot.StatusAttackPowerBonus;
        }

        return resolvedStatus;
    }

    private static float ResolveStatusDurationBonus(SkillSnapshot snapshot, StatusRuntimeData statusData)
    {
        if (snapshot == null)
        {
            return 0f;
        }

        return snapshot.ResolveStatusDurationBonus(statusData.StatusTag);
    }

    private static int ResolveStatusMaxStacksBonus(SkillSnapshot snapshot, StatusRuntimeData statusData)
    {
        if (snapshot == null)
        {
            return 0;
        }

        return snapshot.ResolveStatusMaxStacksBonus(statusData.StatusTag);
    }

    private static ProjectileStatusHitSpec ResolveThresholdStatusSpec(SkillSnapshot snapshot)
    {
        if (snapshot == null || snapshot.ThresholdApplyStatusKind == StatusEffectKind.None)
        {
            return null;
        }

        var kind = snapshot.ThresholdApplyStatusKind;
        var statusData = StatusDataCompiler.Create(kind, null);
        var duration = statusData.Duration;
        var durationBonus = ResolveStatusDurationBonus(snapshot, statusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = statusData,
            Chance = 1f,
            Stacks = statusData.BaseStackAmount,
            DurationSeconds = duration,
            MaxStacks = statusData.MaxStacks,
            Permanent = statusData.Permanent && duration <= 0f,
            RefreshDuration = true
        };
    }
}

}
