using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 단일 대상, 연쇄, 돌진형 스킬의 실행 순서와 피해 적용을 처리한다.
 */
namespace Pakuri.InGame
{

internal static class SingleSkillExecutor
{
	private readonly struct SingleExecutionOutcome
	{
		public bool Routed { get; }

		public bool CastCommitted { get; }

		/*
		 * SingleExecutionOutcome에 필요한 값을 초기화한다.
		 */
		public SingleExecutionOutcome(bool routed, bool castCommitted)
		{
			Routed = routed;
			CastCommitted = castCommitted;
		}
	}

	private readonly struct SingleFollowUpSpec
	{
		public StatusEffectKind RequiredStatusKind { get; }

		public int RepeatCount { get; }

		public float IntervalSeconds { get; }

		public float DamageMultiplier { get; }

		public GameObject Prefab { get; }

		/*
		 * SingleFollowUpSpec에 필요한 값을 초기화한다.
		 */
		public SingleFollowUpSpec(StatusEffectKind requiredStatusKind, int repeatCount, float intervalSeconds, float damageMultiplier, GameObject prefab)
		{
			RequiredStatusKind = requiredStatusKind;
			RepeatCount = repeatCount;
			IntervalSeconds = intervalSeconds;
			DamageMultiplier = damageMultiplier;
			Prefab = prefab;
		}
	}

	private readonly struct SingleFollowUpTarget
	{
		public UnitCombatState Model { get; }

		public Vector2 Center { get; }

		/*
		 * SingleFollowUpTarget에 필요한 값을 초기화한다.
		 */
		public SingleFollowUpTarget(UnitCombatState model, Vector2 center)
		{
			Model = model;
			Center = center;
		}
	}

	private readonly struct TargetDamageResolution
	{
		public float Damage { get; }

		public float CritChanceBonus { get; }

		public bool IsExecute { get; }

		public int PlannedConsumedStacks { get; }

		/*
		 * TargetDamageResolution에 필요한 값을 초기화한다.
		 */
		public TargetDamageResolution(float damage, float critChanceBonus, bool isExecute, int plannedConsumedStacks)
		{
			Damage = damage;
			CritChanceBonus = critChanceBonus;
			IsExecute = isExecute;
			PlannedConsumedStacks = plannedConsumedStacks;
		}
	}

	private const float DefaultVisualLifetimeSeconds = 1f;

	private const float PostDamageLifetimePaddingSeconds = 0.05f;

	/*
	 * Execute 실행 결과를 반환한다.
	 */
	internal static bool Execute(SkillExecutionContext context, SkillSnapshot snapshot, SingleChainSkillRuntimeData skill)
	{
		CombatUnitEntry unitEntry = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
		if (unitEntry == null || unitEntry.Model == null)
		{
			return false;
		}
		ApplyChainHit(context, snapshot, skill, unitEntry, 1f);
		if (skill.ChainDelaySeconds > 0f)
		{
			context.CombatManager.StartCoroutine(ExecuteChainAfterDelay(context, snapshot, skill, unitEntry.Model));
		}
		else
		{
			ExecuteChain(context, snapshot, skill, unitEntry.Model);
		}
		return true;
	}

	/*
	 * Execute 실행 결과를 반환한다.
	 */
	internal static bool Execute(SkillExecutionContext context, SkillSnapshot snapshot, SingleChargeSkillRuntimeData skill)
	{
		CombatUnitEntry unitEntry = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
		if (unitEntry == null || unitEntry.Model == null)
		{
			return false;
		}
		context.Caster.ActiveCharge = new SingleChargeState
		{
			SkillId = skill.SkillId,
			TargetUnitId = ((unitEntry.Model.Identity != null) ? unitEntry.Model.Identity.UnitId : null),
			RampSeconds = skill.RampSeconds,
			MaxMoveSpeedMultiplier = skill.MaxMoveSpeedMultiplier,
			DamageTargetMaxHealthRatio = skill.TargetMaxHealthRatio,
			OnHitStatus = skill.OnHitStatus,
			Attribute = skill.Element
		};
		return true;
	}

	/*
	 * ExecuteChainAfterDelay 실행 결과를 반환한다.
	 */
	private static IEnumerator ExecuteChainAfterDelay(SkillExecutionContext context, SkillSnapshot snapshot, SingleChainSkillRuntimeData skill, UnitCombatState primary)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, skill.ChainDelaySeconds));
		ExecuteChain(context, snapshot, skill, primary);
	}

	/*
	 * ExecuteChain 실행을 처리한다.
	 */
	private static void ExecuteChain(SkillExecutionContext context, SkillSnapshot snapshot, SingleChainSkillRuntimeData skill, UnitCombatState primary)
	{
		List<CombatUnitEntry> list = SkillTargeting.ResolveOrderedTargets(context.CasterEntry, context.Roster, skill.Targeting);
		CombatUnitEntry unitEntry = context.Roster.Find(primary);
		Vector2 vector = ((context.CasterEntry.Transform != null) ? ((Vector2)context.CasterEntry.Transform.position) : Vector2.zero);
		if (unitEntry != null && unitEntry.Transform != null)
		{
			vector = unitEntry.Transform.position;
		}
		CombatUnitEntry unitEntry2 = null;
		float num = float.MaxValue;
		float num2 = ((skill.ChainRadius > 0f) ? (skill.ChainRadius * skill.ChainRadius) : float.MaxValue);
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry3 = list[i];
			if (unitEntry3 != null && unitEntry3.Model != null && !(unitEntry3.Transform == null) && (!skill.ExcludePrimaryTarget || unitEntry3.Model != primary))
			{
				float sqrMagnitude = ((Vector2)unitEntry3.Transform.position - vector).sqrMagnitude;
				if (!(sqrMagnitude > num2) && !(sqrMagnitude >= num))
				{
					unitEntry2 = unitEntry3;
					num = sqrMagnitude;
				}
			}
		}
		if (unitEntry2 != null)
		{
			ApplyChainHit(context, snapshot, skill, unitEntry2, skill.ChainDamageMultiplier);
		}
	}

	/*
	 * ApplyChainHit 처리를 대상에 적용한다.
	 */
	private static void ApplyChainHit(SkillExecutionContext context, SkillSnapshot snapshot, SingleChainSkillRuntimeData skill, CombatUnitEntry target, float multiplier)
	{
		float baseDamage = DamageCalculator.ResolveDamage(context.Caster, skill.Damage, snapshot) * Mathf.Max(0f, multiplier);
		context.CombatManager.ApplyDamage(target.Model, baseDamage, skill.Damage.Element, context.Caster, skill.Damage.CriticalAllowed, 0f, 0f, skill.SkillId);
		EffectManager effects = context.CombatManager.Effects;
		if (effects != null)
		{
			effects.SpawnAttachedSkillEffect(skill, target.Transform, 0.8f);
		}
	}

	/*
	 * Execute 실행 결과를 반환한다.
	 */
	internal static bool Execute(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill)
	{
		if (SingleSkillRules.ShouldRejectCastForExecuteThreshold(context, snapshot, skill))
		{
			return false;
		}
		Vector2 vector = ResolveAreaCenter(context, skill.Targeting, skill.Area);
		EffectManager effects = context.CombatManager.Effects;
		RuntimeSkillVisualSpec runtimeVisual = skill.RuntimeVisual;
		bool num = effects != null && EffectManager.HasVisual(runtimeVisual);
		GameObject prefab = skill.SkillEffectPrefab;
		if (snapshot != null && snapshot.SkillEffectPrefab != null)
		{
			prefab = snapshot.SkillEffectPrefab;
		}
		if (num || effects == null)
		{
			prefab = null;
		}
		SingleExecutionOutcome singleExecutionOutcome = (UsesResolvedDeployments(skill) ? ExecuteResolvedDeployments(context, snapshot, skill, vector, runtimeVisual, prefab) : ExecuteAtCenter(context, snapshot, skill, vector, runtimeVisual, prefab, allowConditionalFollowUp: true));
		bool flag = SkillEffect.Execute(context, snapshot, SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects), vector);
		if (!(singleExecutionOutcome.Routed || flag))
		{
			return singleExecutionOutcome.CastCommitted;
		}
		return true;
	}

	/*
	 * ResolveAreaCenter 결과를 계산해 반환한다.
	 */
	private static Vector2 ResolveAreaCenter(SkillExecutionContext context, SkillTargetingSpec targeting, AreaBlueprintSpec area)
	{
		return SkillTargeting.ResolveAreaCenter(context, targeting, area);
	}

	/*
	 * ResolveRadius 결과를 계산해 반환한다.
	 */
	private static float ResolveRadius(SingleSkillRuntimeData skill, SkillSnapshot snapshot)
	{
		AreaBlueprintSpec area = skill?.Area;
		return SkillTargeting.ResolveRadius(SkillTargeting.ResolveBaseRadius(skill?.Targeting, area), snapshot);
	}

	/*
	 * ResolvePrefabHitboxCenter 결과를 계산해 반환한다.
	 */
	private static Vector2 ResolvePrefabHitboxCenter(SkillExecutionContext context, Vector2 fallbackCenter, SingleSkillRuntimeData skill)
	{
		if (skill != null && skill.HitAllTargets && !UsesStatusFilteredDeployments(skill))
		{
			if (context == null || context.CasterEntry == null || !(context.CasterEntry.Transform != null))
			{
				return fallbackCenter;
			}
			return context.CasterEntry.Transform.position;
		}
		return fallbackCenter;
	}

	/*
	 * ResolveDeploymentCount 결과를 계산해 반환한다.
	 */
	private static int ResolveDeploymentCount(SingleSkillRuntimeData skill, SkillSnapshot snapshot)
	{
		if (skill == null || !skill.UseMultiDeployment)
		{
			return 1;
		}
		int num = snapshot?.HitTargetCountBonus ?? 0;
		return Mathf.Max(1, skill.DeploymentCount + num);
	}

	/*
	 * UsesStatusFilteredDeployments 조건을 만족하는지 확인한다.
	 */
	private static bool UsesStatusFilteredDeployments(SingleSkillRuntimeData skill)
	{
		if (skill != null)
		{
			return !string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId);
		}
		return false;
	}

	/*
	 * UsesSingleLineVisual 조건을 만족하는지 확인한다.
	 */
	private static bool UsesSingleLineVisual(SingleSkillRuntimeData skill)
	{
		if (skill == null || !skill.UseMultiDeployment)
		{
			return false;
		}

		return string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId);
	}

	/*
	 * ResolveEffectiveHitTargetCount 결과를 계산해 반환한다.
	 */
	private static int ResolveEffectiveHitTargetCount(SingleSkillRuntimeData skill, SkillSnapshot snapshot)
	{
		if (skill == null)
		{
			return 1;
		}
		if (UsesSingleLineVisual(skill) || skill.HitAllTargets || skill.HitTargetCount == int.MaxValue)
		{
			return int.MaxValue;
		}
		int num = snapshot?.HitTargetCountBonus ?? 0;
		return Mathf.Max(1, skill.HitTargetCount + num);
	}

	/*
	 * UsesResolvedDeployments 조건을 만족하는지 확인한다.
	 */
	private static bool UsesResolvedDeployments(SingleSkillRuntimeData skill)
	{
		if (skill != null)
		{
			if (!skill.UseMultiDeployment)
			{
				return UsesStatusFilteredDeployments(skill);
			}
			return true;
		}
		return false;
	}

	/*
	 * ResolveDeploymentCenters 결과를 계산해 반환한다.
	 */
	private static List<Vector2> ResolveDeploymentCenters(SkillExecutionContext context, SingleSkillRuntimeData skill, Vector2 primaryCenter, int deploymentCount)
	{
		if (UsesStatusFilteredDeployments(skill))
		{
			int requiredStatusMinStacks = Mathf.Max(1, skill.DeploymentRequiredTargetStatusMinStacks);
			CombatUnitEntry casterEntry = null;
			CombatUnitRegistry roster = null;
			if (context != null)
			{
				casterEntry = context.CasterEntry;
				roster = context.Roster;
			}
			List<CombatUnitEntry> list = SkillTargeting.ResolveOrderedTargets(casterEntry, roster, skill.Targeting, skill.DeploymentRequiredTargetStatusKind, requiredStatusMinStacks);
			List<Vector2> list2 = new List<Vector2>(list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				CombatUnitEntry unitEntry = list[i];
				if (unitEntry != null && unitEntry.Transform != null)
				{
					list2.Add(unitEntry.Transform.position);
				}
			}
			return list2;
		}
		bool coverAll = (skill != null && skill.Area != null && skill.Area.CoverAll) || (skill != null && skill.Targeting != null && skill.Targeting.CoverAll);
		SkillTargetingSpec targeting = null;
		if (skill != null)
		{
			targeting = skill.Targeting;
		}
		return SkillTargeting.ResolveTargetAnchoredCenters(context, targeting, primaryCenter, deploymentCount, coverAll, SkillDeploymentRepeatMode.RepeatNearest);
	}

	/*
	 * ExecuteResolvedDeployments 실행 결과를 반환한다.
	 */
	private static SingleExecutionOutcome ExecuteResolvedDeployments(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, Vector2 primaryCenter, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab)
	{
		int deploymentCount = ResolveDeploymentCount(skill, snapshot);
		List<Vector2> list = ResolveDeploymentCenters(context, skill, primaryCenter, deploymentCount);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < list.Count; i++)
		{
			Vector2 vector = list[i];
			SingleExecutionOutcome singleExecutionOutcome = ExecuteAtCenter(context, snapshot, skill, vector, runtimeVisual, prefab, allowConditionalFollowUp: true);
			flag = flag || singleExecutionOutcome.Routed;
			flag2 = flag2 || singleExecutionOutcome.CastCommitted;
			flag = SkillEffect.ExecuteOnDeploymentCast(context, snapshot, SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects), vector) || flag;
			ScheduleRepeatedDeployments(context, snapshot, skill, vector, runtimeVisual, prefab);
		}
		return new SingleExecutionOutcome(flag, flag2);
	}

	/*
	 * ScheduleRepeatedDeployments 작업을 수행한다.
	 */
	private static void ScheduleRepeatedDeployments(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab)
	{
		if (context == null || context.CombatManager == null || skill == null || snapshot == null || snapshot.RepeatCountPerTarget <= 0)
		{
			return;
		}
		SkillSnapshot snapshot2 = ((!Mathf.Approximately(snapshot.RepeatDamageMultiplier, 1f)) ? snapshot.CopyWithDamageMultiplier(snapshot.RepeatDamageMultiplier) : snapshot);
		for (int i = 1; i <= snapshot.RepeatCountPerTarget; i++)
		{
			float num = Mathf.Max(0f, snapshot.RepeatIntervalSeconds * (float)i);
			if (num <= 0f)
			{
				ExecuteAtCenter(context, snapshot2, skill, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
				SkillEffect.ExecuteOnDeploymentCast(context, snapshot2, SkillNodeAction.ResolveEffects(snapshot2, skill.MultiEffects), center);
			}
			else
			{
				context.CombatManager.StartCoroutine(ExecuteRepeatedDeploymentAfterDelay(context, snapshot2, skill, center, runtimeVisual, prefab, num));
			}
		}
	}

	/*
	 * ExecuteRepeatedDeploymentAfterDelay 실행 결과를 반환한다.
	 */
	private static IEnumerator ExecuteRepeatedDeploymentAfterDelay(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null && skill != null)
		{
			ExecuteAtCenter(context, snapshot, skill, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
			SkillEffect.ExecuteOnDeploymentCast(context, snapshot, SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects), center);
		}
	}

	/*
	 * ExecuteAtCenter 실행 결과를 반환한다.
	 */
	private static SingleExecutionOutcome ExecuteAtCenter(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, bool allowConditionalFollowUp)
	{
		float radius = ResolveRadius(skill, snapshot);
		bool coverAll = (skill.Area != null && skill.Area.CoverAll) || (skill.Targeting != null && skill.Targeting.CoverAll);
		float damage = DamageCalculator.ResolveDamage(context.Caster, skill.Damage, snapshot);
		DamageAttribute attribute = (skill.Damage != null) ? skill.Damage.Element : skill.Element;
		ProjectileStatusHitSpec statusSpec = SkillStatus.ResolveStatusSpec(skill.OnHitStatus, snapshot);
		SkillEffectDefinition[] onHitStatusEffects = ResolveOnHitStatusEffects(context, snapshot, SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects));
		float critChanceBonus = snapshot?.CritChanceBonus ?? 0f;
		float critDamageBonus = snapshot?.CritDamageBonus ?? 0f;
		int num = ResolveEffectiveHitTargetCount(skill, snapshot);
		float num2 = Mathf.Max(0f, skill.DamageDelaySeconds);
		SingleFollowUpSpec? followUpSpec = (allowConditionalFollowUp ? ResolveFollowUpSpec(snapshot, statusSpec, prefab) : ((SingleFollowUpSpec?)null));
		List<SingleFollowUpTarget> followUpTargets = (followUpSpec.HasValue ? new List<SingleFollowUpTarget>() : null);
		SkillRuntimeInstance skillRuntimeInstance = (allowConditionalFollowUp ? context.Runtime : null);
		bool flag = false;
		bool flag2 = false;
		bool castCommitted = false;
		EffectManager effects = context.CombatManager.Effects;
		bool flag3 = effects != null && EffectManager.HasVisual(runtimeVisual);
		if (skill.UsePrefabHitbox && (flag3 || prefab != null) && effects != null)
		{
			center = ResolvePrefabHitboxCenter(context, center, skill);
			GameObject gameObject = effects.CreateEffectObject(runtimeVisual, prefab, "RuntimeSingleHitbox", center, Quaternion.identity);
			if (gameObject != null)
			{
				flag = true;
				castCommitted = true;
				if (UsesSingleLineVisual(skill))
				{
					effects.ConfigureSingleLineEffect(gameObject, context, skill, snapshot, center);
				}
				else if (!flag3)
				{
					effects.ConfigureAreaEffect(gameObject, SkillTargeting.ResolveBaseRadius(skill.Targeting, skill.Area), snapshot);
				}
				if (num2 > 0f)
				{
					context.CombatManager.StartCoroutine(ApplyPrefabHitboxAfterDelay(context, snapshot, skill, gameObject, num, damage, attribute, statusSpec, onHitStatusEffects, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
				}
				else
				{
					Physics2D.SyncTransforms();
					flag2 = ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, gameObject, num, damage, attribute, statusSpec, onHitStatusEffects, context.Caster, skill.SkillId, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets);
				}
				float visualLifetime = Mathf.Max(num2 + 0.05f, 1f);
				effects.DestroyAfterAnimation(gameObject, visualLifetime);
			}
		}
		if (!flag)
		{
			castCommitted = true;
			if (num2 > 0f)
			{
				if (effects != null)
				{
					float visualLifetime = Mathf.Max(num2 + 0.05f, 1f);
					effects.SpawnAnimatedEffect(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, visualLifetime);
				}
				context.CombatManager.StartCoroutine(ApplyNonPrefabTargetsAfterDelay(context, snapshot, skill, center, radius, coverAll, num, damage, attribute, statusSpec, onHitStatusEffects, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
			}
			else
			{
				flag2 = ApplyNonPrefabTargets(context, snapshot, skill, center, radius, coverAll, num, damage, attribute, statusSpec, onHitStatusEffects, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
				if (flag2 && effects != null)
				{
					effects.SpawnAnimatedEffect(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, 1f);
				}
			}
		}
		if (allowConditionalFollowUp && num2 <= 0f)
		{
			ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
		}
		return new SingleExecutionOutcome(flag2, castCommitted);
	}

	/*
	 * ApplyNonPrefabTargets 처리를 대상에 적용한다.
	 */
	private static bool ApplyNonPrefabTargets(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillEffectDefinition[] onHitStatusEffects, SkillRuntimeInstance onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || skill == null)
		{
			return false;
		}
		if (skill.UsesHitTargetCount && !skill.HitAllTargets)
		{
			return ApplyLimitedTargets(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, effectiveHitTargetCount, damage, attribute, statusSpec, onHitStatusEffects, context.Caster, skill.SkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, center, followUpSpec, followUpTargets);
		}
		return ApplyAreaTargets(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, center, radius, coverAll, damage, attribute, statusSpec, onHitStatusEffects, context.Caster, skill.SkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets);
	}

	/*
	 * ApplyNonPrefabTargetsAfterDelay 처리를 대상에 적용한다.
	 */
	private static IEnumerator ApplyNonPrefabTargetsAfterDelay(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillEffectDefinition[] onHitStatusEffects, SkillRuntimeInstance onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, float delaySeconds, bool allowConditionalFollowUp)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		ApplyNonPrefabTargets(context, snapshot, skill, center, radius, coverAll, effectiveHitTargetCount, damage, attribute, statusSpec, onHitStatusEffects, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
		if (allowConditionalFollowUp)
		{
			ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
		}
	}

	/*
	 * ApplyPrefabHitboxAfterDelay 처리를 대상에 적용한다.
	 */
	private static IEnumerator ApplyPrefabHitboxAfterDelay(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, GameObject instance, int effectiveHitTargetCount, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillEffectDefinition[] onHitStatusEffects, SkillRuntimeInstance onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, float delaySeconds, bool allowConditionalFollowUp)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.CasterEntry != null && context.Roster != null && skill != null && !(instance == null))
		{
			Physics2D.SyncTransforms();
			ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, instance, effectiveHitTargetCount, damage, attribute, statusSpec, onHitStatusEffects, context.Caster, skill.SkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets);
			if (allowConditionalFollowUp)
			{
				ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
			}
		}
	}

	/*
	 * ApplyPrefabHitbox 처리를 대상에 적용한다.
	 */
	private static bool ApplyPrefabHitbox(InGameCombatManager manager, CombatUnitEntry sourceEntry, CombatUnitRegistry unitRoster, SingleSkillRuntimeData skill, SkillTargetingSpec targetingSpec, GameObject hitboxObject, int maxTargets, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillEffectDefinition[] onHitStatusEffects, UnitCombatState source, string sourceSkillId, SkillRuntimeInstance sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillSnapshot snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (manager == null || sourceEntry == null || unitRoster == null || hitboxObject == null || maxTargets <= 0)
		{
			return false;
		}
		Collider2D[] componentsInChildren = hitboxObject.GetComponentsInChildren<Collider2D>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return false;
		}
		Collider2D[] array = ResolveCoreHitboxColliders(hitboxObject, snapshot);
		List<CombatUnitEntry> list = SkillTargeting.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
		bool result = false;
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry = list[i];
			if (IsTargetInsideHitbox(componentsInChildren, unitEntry))
			{
				RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry, (unitEntry != null && unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : Vector2.zero);
				Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : Vector2.zero);
				bool isCoreHit = array.Length != 0 && IsTargetInsideHitbox(array, unitEntry);
				TargetDamageResolution damageResolution = ResolveTargetDamage(source, skill, snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit);
				InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute);
				int consumedStacks = ConsumePlannedTargetStatusStacks(manager, unitEntry.Model, skill, damageResolution);
				SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result2, damageResolution.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
				if (!result2.IsDead)
				{
					TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
					TryApplyOnHitStatusEffects(manager, unitEntry.Model, onHitStatusEffects, source);
				}
				TryApplyCoreOnHitAdditionalDamage(manager, snapshot, source, sourceSkillId, unitEntry, damageResolution.Damage, isCoreHit);
				SkillOnHitEffect.TryApply(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
				result = true;
				num++;
				if (num >= maxTargets)
				{
					break;
				}
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, num, hitboxObject.transform.position);
		return result;
	}

	/*
	 * ApplyLimitedTargets 처리를 대상에 적용한다.
	 */
	private static bool ApplyLimitedTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, CombatUnitRegistry unitRoster, SingleSkillRuntimeData skill, SkillTargetingSpec targetingSpec, int maxTargets, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillEffectDefinition[] onHitStatusEffects, UnitCombatState source, string sourceSkillId, SkillRuntimeInstance sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillSnapshot snapshot, Vector2 center, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (manager == null || sourceEntry == null || unitRoster == null || maxTargets <= 0)
		{
			return false;
		}
		List<CombatUnitEntry> list = SkillTargeting.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
		bool result = false;
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry = list[i];
			RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry, center);
			Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : center);
			TargetDamageResolution damageResolution = ResolveTargetDamage(source, skill, snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
			InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute);
			int consumedStacks = ConsumePlannedTargetStatusStacks(manager, unitEntry.Model, skill, damageResolution);
			SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result2, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
			if (!result2.IsDead)
			{
				TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
				TryApplyOnHitStatusEffects(manager, unitEntry.Model, onHitStatusEffects, source);
			}
			SkillOnHitEffect.TryApply(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
			result = true;
			num++;
			if (num >= maxTargets)
			{
				break;
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, num, center);
		return result;
	}

	/*
	 * ApplyAreaTargets 처리를 대상에 적용한다.
	 */
	private static bool ApplyAreaTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, CombatUnitRegistry unitRoster, SingleSkillRuntimeData skill, SkillTargetingSpec targetingSpec, Vector2 center, float radius, bool coverAll, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillEffectDefinition[] onHitStatusEffects, UnitCombatState source, string sourceSkillId, SkillRuntimeInstance sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillSnapshot snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (manager == null || sourceEntry == null || unitRoster == null)
		{
			return false;
		}
		List<CombatUnitEntry> list = SkillTargeting.ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec);
		if (!coverAll && radius <= 0f)
		{
			CombatUnitEntry unitEntry = ((list.Count > 0) ? list[0] : null);
			if (unitEntry == null || !unitEntry.IsAlive || unitEntry.Model == null)
			{
				return false;
			}
			RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry, center);
			Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : center);
			TargetDamageResolution damageResolution = ResolveTargetDamage(source, skill, snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
			InGameResourceChangeResult result = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute);
			int consumedStacks = ConsumePlannedTargetStatusStacks(manager, unitEntry.Model, skill, damageResolution);
			SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result, consumedStacks);
			if (!result.IsDead)
			{
				TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
				TryApplyOnHitStatusEffects(manager, unitEntry.Model, onHitStatusEffects, source);
			}
			SkillOnHitEffect.TryApply(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
			TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, 1);
			TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, 1, center);
			return true;
		}
		bool result2 = false;
		int num = 0;
		float num2 = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry2 = list[i];
			if (unitEntry2 != null && unitEntry2.IsAlive && unitEntry2.Model != null && !(unitEntry2.Transform == null) && (coverAll || !(((Vector2)unitEntry2.Transform.position - center).sqrMagnitude > num2)))
			{
				RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry2, center);
				Vector2 hitPosition2 = ((unitEntry2.Transform != null) ? ((Vector2)unitEntry2.Transform.position) : center);
				TargetDamageResolution damageResolution2 = ResolveTargetDamage(source, skill, snapshot, damage, unitEntry2.Model, critChanceBonus, isCoreHit: false);
				InGameResourceChangeResult result3 = manager.ApplyDamage(unitEntry2.Model, damageResolution2.Damage, attribute, source, criticalAllowed, damageResolution2.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution2.IsExecute);
				int consumedStacks2 = ConsumePlannedTargetStatusStacks(manager, unitEntry2.Model, skill, damageResolution2);
				SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result3, damageResolution2.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry2, result3, consumedStacks2);
				if (!result3.IsDead)
				{
					TryApplyStatus(manager, unitEntry2.Model, statusSpec, source);
					TryApplyOnHitStatusEffects(manager, unitEntry2.Model, onHitStatusEffects, source);
				}
				SkillOnHitEffect.TryApply(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry2, hitPosition2, damageResolution2.Damage);
				result2 = true;
				num++;
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, num, center);
		return result2;
	}

	/*
	 * IsTargetInsideHitbox 조건을 만족하는지 확인한다.
	 */
	private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, CombatUnitEntry target)
	{
		return UnitHitboxOverlap.IsTargetInsideHitbox(hitboxColliders, target);
	}

	/*
	 * ResolveCoreHitboxColliders 결과를 계산해 반환한다.
	 */
	private static Collider2D[] ResolveCoreHitboxColliders(GameObject hitboxObject, SkillSnapshot snapshot)
	{
		if (hitboxObject == null || snapshot == null || string.IsNullOrWhiteSpace(snapshot.CoreHitboxName))
		{
			return Array.Empty<Collider2D>();
		}
		List<Collider2D> list = new List<Collider2D>();
		Transform[] componentsInChildren = hitboxObject.GetComponentsInChildren<Transform>(includeInactive: true);
		foreach (Transform transform in componentsInChildren)
		{
			if (!(transform == null) && string.Equals(transform.name, snapshot.CoreHitboxName, StringComparison.OrdinalIgnoreCase))
			{
				Collider2D[] componentsInChildren2 = transform.GetComponentsInChildren<Collider2D>(includeInactive: true);
				if (componentsInChildren2 != null && componentsInChildren2.Length != 0)
				{
					list.AddRange(componentsInChildren2);
				}
			}
		}
		if (list.Count <= 0)
		{
			return Array.Empty<Collider2D>();
		}
		return list.ToArray();
	}

	/*
	 * ResolveOnHitStatusEffects 결과를 계산해 반환한다.
	 */
	private static SkillEffectDefinition[] ResolveOnHitStatusEffects(SkillExecutionContext context, SkillSnapshot snapshot, SkillEffectDefinition[] effects)
	{
		if (effects == null || effects.Length == 0)
		{
			return Array.Empty<SkillEffectDefinition>();
		}
		List<SkillEffectDefinition> list = new List<SkillEffectDefinition>();
		foreach (SkillEffectDefinition skillEffectDefinition in effects)
		{
			if (skillEffectDefinition != null && skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.OnHit && skillEffectDefinition.EffectKind == SkillMultiEffectKind.Status && skillEffectDefinition.TargetSide == SkillMultiEffectTargetSide.Enemy && SkillEffect.ShouldRun(context, skillEffectDefinition, snapshot))
			{
				list.Add(skillEffectDefinition);
			}
		}
		if (list.Count <= 0)
		{
			return Array.Empty<SkillEffectDefinition>();
		}
		return list.ToArray();
	}

	/*
	 * TryApplyOnHitStatusEffects 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryApplyOnHitStatusEffects(InGameCombatManager manager, UnitCombatState target, SkillEffectDefinition[] effects, UnitCombatState source)
	{
		if (manager == null || target == null || effects == null || effects.Length == 0)
		{
			return;
		}
		foreach (SkillEffectDefinition skillEffectDefinition in effects)
		{
			if (skillEffectDefinition != null && SkillEffect.TargetMatchesCondition(target, skillEffectDefinition))
			{
				ProjectileStatusHitSpec projectileStatusHitSpec = SkillEffect.ResolveStatusSpec(skillEffectDefinition);
				if (projectileStatusHitSpec != null && projectileStatusHitSpec.Enabled)
				{
					StatusCombatRules.ApplyStatus(manager, target, projectileStatusHitSpec, source);
				}
			}
		}
	}

	/*
	 * TryApplyCoreOnHitAdditionalDamage 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryApplyCoreOnHitAdditionalDamage(InGameCombatManager manager, SkillSnapshot snapshot, UnitCombatState source, string sourceSkillId, CombatUnitEntry target, float primaryDamage, bool isCoreHit)
	{
		if (isCoreHit && !(manager == null) && snapshot != null && snapshot.HasCoreOnHitAdditionalDamage && !(snapshot.CoreOnHitAdditionalDamageMultiplier <= 0f) && source != null && target != null && target.IsAlive && target.Model != null && !(primaryDamage <= 0f) && !(UnityEngine.Random.value > Mathf.Clamp01(snapshot.CoreOnHitAdditionalDamageChance)))
		{
			manager.ApplyDamage(target.Model, primaryDamage * snapshot.CoreOnHitAdditionalDamageMultiplier, snapshot.CoreOnHitAdditionalDamageAttribute, source, criticalAllowed: false, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: true);
		}
	}

	/*
	 * TryApplyHitCountCooldownRefund 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryApplyHitCountCooldownRefund(SkillRuntimeInstance sourceRuntime, SkillSnapshot snapshot, int hitCount)
	{
		if (sourceRuntime != null && sourceRuntime.Owner != null && sourceRuntime.Owner.SkillRuntime != null && snapshot != null && hitCount >= snapshot.HitCountCooldownRefundMinTargets && !string.IsNullOrWhiteSpace(snapshot.HitCountCooldownRefundTargetSkillId) && !(snapshot.HitCountCooldownRefundRatio <= 0f))
		{
			SkillRuntimeInstance skillRuntimeInstance = sourceRuntime.Owner.SkillRuntime.FindBySkillId(snapshot.HitCountCooldownRefundTargetSkillId);
			skillRuntimeInstance?.ReduceCooldownRemaining(skillRuntimeInstance.EffectiveCooldownDuration * Mathf.Clamp01(snapshot.HitCountCooldownRefundRatio));
		}
	}

	/*
	 * TryExecuteOnHitCountEffects 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryExecuteOnHitCountEffects(InGameCombatManager manager, CombatUnitRegistry roster, CombatUnitEntry sourceEntry, SkillRuntimeInstance sourceRuntime, SingleSkillRuntimeData skill, SkillSnapshot snapshot, int hitCount, Vector2 center)
	{
		if (!(manager == null) && roster != null && sourceEntry != null && skill != null && hitCount > 0)
		{
			SkillEffect.ExecuteOnHitCount(new SkillExecutionContext(manager, roster, sourceEntry, sourceRuntime), snapshot, SkillNodeAction.ResolveEffects(snapshot, skill.MultiEffects), center, hitCount);
		}
	}

	/*
	 * ResolveFollowUpSpec 결과를 계산해 반환한다.
	 */
	private static SingleFollowUpSpec? ResolveFollowUpSpec(SkillSnapshot snapshot, ProjectileStatusHitSpec statusSpec, GameObject prefab)
	{
		if (snapshot == null || !snapshot.HasBranchCount || snapshot.BranchCount <= 0 || !snapshot.HasBranchDamageMultiplier || snapshot.BranchDamageMultiplier <= 0f || !snapshot.HasBranchSearchRadius || snapshot.BranchSearchRadius <= 0f)
		{
			return null;
		}
		if (statusSpec == null || statusSpec.Kind == StatusEffectKind.None)
		{
			return null;
		}
		return new SingleFollowUpSpec(statusSpec.Kind, snapshot.BranchCount, snapshot.BranchSearchRadius, snapshot.BranchDamageMultiplier, prefab);
	}

	/*
	 * RegisterFollowUpTarget 작업을 수행한다.
	 */
	private static void RegisterFollowUpTarget(List<SingleFollowUpTarget> followUpTargets, SingleFollowUpSpec? followUpSpec, CombatUnitEntry target, Vector2 center)
	{
		if (followUpTargets == null || !followUpSpec.HasValue || target == null || target.Model == null || !HasStatus(target.Model, followUpSpec.Value.RequiredStatusKind))
		{
			return;
		}
		for (int i = 0; i < followUpTargets.Count; i++)
		{
			if (followUpTargets[i].Model == target.Model)
			{
				return;
			}
		}
		followUpTargets.Add(new SingleFollowUpTarget(target.Model, center));
	}

	/*
	 * ScheduleConditionalFollowUps 작업을 수행한다.
	 */
	private static void ScheduleConditionalFollowUps(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (context == null || context.CombatManager == null || context.Roster == null || context.CasterEntry == null || context.Caster == null || skill == null || !followUpSpec.HasValue || followUpTargets == null || followUpTargets.Count == 0)
		{
			return;
		}
		SingleFollowUpSpec value = followUpSpec.Value;
		for (int i = 0; i < followUpTargets.Count; i++)
		{
			SingleFollowUpTarget followUpTarget = followUpTargets[i];
			for (int j = 1; j <= value.RepeatCount; j++)
			{
				context.CombatManager.StartCoroutine(ExecuteConditionalFollowUpAfterDelay(context, snapshot, skill, followUpTarget, value, value.IntervalSeconds * (float)j));
			}
		}
	}

	/*
	 * ExecuteConditionalFollowUpAfterDelay 실행 결과를 반환한다.
	 */
	private static IEnumerator ExecuteConditionalFollowUpAfterDelay(SkillExecutionContext context, SkillSnapshot snapshot, SingleSkillRuntimeData skill, SingleFollowUpTarget followUpTarget, SingleFollowUpSpec followUpSpec, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null && skill != null)
		{
			CombatUnitEntry unitEntry = ((followUpTarget.Model != null) ? context.Roster.Find(followUpTarget.Model) : null);
			Vector2 center = ((unitEntry != null && unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : followUpTarget.Center);
			SkillSnapshot snapshot2 = ((snapshot != null) ? snapshot.CopyWithDamageMultiplier(followUpSpec.DamageMultiplier) : null);
			ExecuteAtCenter(context, snapshot2, skill, center, null, followUpSpec.Prefab, allowConditionalFollowUp: false);
		}
	}


	/*
	 * ResolveTargetDamage 결과를 계산해 반환한다.
	 */
	private static TargetDamageResolution ResolveTargetDamage(UnitCombatState caster, SingleSkillRuntimeData skill, SkillSnapshot snapshot, float baseDamage, UnitCombatState target, float baseCritChanceBonus, bool isCoreHit)
	{
		float num = Mathf.Max(0f, baseDamage + ResolveTargetStatusStackAdditionalDamage(caster, skill, snapshot, target, baseDamage));
		float num2 = snapshot?.ResolveConditionalDamageMultiplier(target) ?? 1f;
		float critChanceBonus = baseCritChanceBonus + (snapshot?.ResolveConditionalCritChanceBonus(target) ?? 0f);
		bool flag = false;
		int plannedConsumedStacks = ResolvePlannedConsumedStacks(skill, snapshot, target);
		if (isCoreHit && snapshot != null && snapshot.HasCoreDamageMultiplier)
		{
			num2 *= snapshot.CoreDamageMultiplier;
		}
		SingleDamageModifierState singleDamageModifierState = SingleSkillRules.ApplyDamageModifiers(skill, snapshot, target, num2, critChanceBonus);
		num2 = singleDamageModifierState.DamageMultiplier;
		critChanceBonus = singleDamageModifierState.CritChanceBonus;
		flag = singleDamageModifierState.IsExecute;
		return new TargetDamageResolution(Mathf.Max(0f, num * Mathf.Max(0f, num2)), critChanceBonus, flag, plannedConsumedStacks);
	}

	/*
	 * ResolveTargetStatusStackAdditionalDamage 결과를 계산해 반환한다.
	 */
	private static float ResolveTargetStatusStackAdditionalDamage(UnitCombatState caster, SingleSkillRuntimeData skill, SkillSnapshot snapshot, UnitCombatState target, float baseDamage)
	{
		if (caster == null || skill == null || target == null || skill.TargetStatusStackDamage == null || skill.TargetStatusStackStatusKind == StatusEffectKind.None)
		{
			return 0f;
		}
		int num = ResolveStatusStacks(target, skill.TargetStatusStackStatusKind);
		if (num <= 0)
		{
			return 0f;
		}
		if (skill.TargetStatusStackMaxStacks > 0)
		{
			num = Mathf.Min(num, skill.TargetStatusStackMaxStacks);
		}
		float num2 = DamageCalculator.ResolveDamage(caster, skill.TargetStatusStackDamage, snapshot);
		float b = 1f;
		float num3 = 0f;
		if (snapshot != null)
		{
			b = snapshot.TargetStatusStackDamageMultiplier;
			num3 = snapshot.ResolveTargetStatusStackDamageRateBonus(skill.TargetStatusStackStatusId);
		}
		float num4 = num2 * Mathf.Max(0f, b) + Mathf.Max(0f, baseDamage) * num3;
		return Mathf.Max(0f, (float)num * num4);
	}

	/*
	 * ResolvePlannedConsumedStacks 결과를 계산해 반환한다.
	 */
	private static int ResolvePlannedConsumedStacks(SingleSkillRuntimeData skill, SkillSnapshot snapshot, UnitCombatState target)
	{
		if (skill == null || target == null || skill.ConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		int num = ResolveStatusStacks(target, skill.ConsumeTargetStatusKind);
		if (num <= 0)
		{
			return 0;
		}
		if (snapshot != null && snapshot.HasConsumeTargetStatusStacksOverride)
		{
			return Mathf.Clamp(snapshot.ConsumeTargetStatusStacksOverride, 0, num);
		}
		if (skill.ConsumeTargetStatusStacks > 0)
		{
			return Mathf.Clamp(skill.ConsumeTargetStatusStacks, 0, num);
		}
		float num2 = skill.ConsumeTargetStatusRatio;
		if (snapshot != null && snapshot.HasConsumeTargetStatusRatioOverride)
		{
			num2 = snapshot.ConsumeTargetStatusRatioOverride;
		}
		if (num2 <= 0f)
		{
			return 0;
		}
		return Mathf.Clamp(Mathf.FloorToInt((float)num * Mathf.Clamp01(num2)), 0, num);
	}

	/*
	 * ConsumePlannedTargetStatusStacks 작업 결과를 반환한다.
	 */
	private static int ConsumePlannedTargetStatusStacks(InGameCombatManager manager, UnitCombatState target, SingleSkillRuntimeData skill, TargetDamageResolution damageResolution)
	{
		if (manager == null || target == null || skill == null || damageResolution.PlannedConsumedStacks <= 0 || skill.ConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		return manager.ConsumeStatusStacks(target, skill.ConsumeTargetStatusKind, damageResolution.PlannedConsumedStacks);
	}

	/*
	 * TryRedistributeConsumedStatusOnKill 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryRedistributeConsumedStatusOnKill(InGameCombatManager manager, CombatUnitEntry sourceEntry, CombatUnitRegistry roster, UnitCombatState source, SkillSnapshot snapshot, CombatUnitEntry defeatedTarget, InGameResourceChangeResult result, int consumedStacks)
	{
		if (manager == null || sourceEntry == null || roster == null || source == null || snapshot == null || defeatedTarget == null || defeatedTarget.Transform == null || !result.IsDead || consumedStacks <= 0 || snapshot.RedistributeConsumedStatusRatioOnKill <= 0f || snapshot.RedistributeConsumedStatusKind == StatusEffectKind.None || snapshot.RedistributeConsumedStatusSearchRadius <= 0f)
		{
			return;
		}
		int num = Mathf.FloorToInt((float)consumedStacks * Mathf.Clamp01(snapshot.RedistributeConsumedStatusRatioOnKill));
		if (num <= 0)
		{
			return;
		}
		List<CombatUnitEntry> list = ResolveRedistributionTargets(sourceEntry, roster, defeatedTarget.Transform.position, snapshot.RedistributeConsumedStatusSearchRadius, defeatedTarget.Model, snapshot.RedistributeConsumedStatusTargetCount);
		if (list.Count <= 0)
		{
			return;
		}
		int num2 = num / list.Count;
		int num3 = num % list.Count;
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry = list[i];
			int num4 = num2;
			if (i < num3)
			{
				num4++;
			}
			if (unitEntry != null && unitEntry.Model != null && num4 > 0)
			{
				ProjectileStatusHitSpec projectileStatusHitSpec = SkillStatus.CreateDirectStatusSpec(snapshot.RedistributeConsumedStatusKind, num4, snapshot);
				if (projectileStatusHitSpec != null)
				{
					StatusCombatRules.ApplyStatus(manager, unitEntry.Model, projectileStatusHitSpec, source);
				}
			}
		}
	}

	/*
	 * ResolveRedistributionTargets 결과를 계산해 반환한다.
	 */
	private static List<CombatUnitEntry> ResolveRedistributionTargets(CombatUnitEntry sourceEntry, CombatUnitRegistry roster, Vector2 center, float radius, UnitCombatState excludedModel, int maxTargetCount)
	{
		List<CombatUnitEntry> list = new List<CombatUnitEntry>();
		if (sourceEntry == null || roster == null || radius <= 0f)
		{
			return list;
		}
		IReadOnlyList<CombatUnitEntry> readOnlyList = SkillTargeting.ResolveTargetList(sourceEntry, roster, new SkillTargetingSpec
		{
			TargetSide = SkillTargetSide.Enemy,
			Selection = SkillTargetSelection.Nearest,
			Shape = SkillTargetShape.Circle,
			Radius = radius
		});
		float num = radius * radius;
		for (int i = 0; i < readOnlyList.Count; i++)
		{
			CombatUnitEntry unitEntry = readOnlyList[i];
			if (unitEntry != null && unitEntry.IsAlive && unitEntry.Model != null && !(unitEntry.Transform == null) && unitEntry.Model != excludedModel && !(((Vector2)unitEntry.Transform.position - center).sqrMagnitude > num))
			{
				list.Add(unitEntry);
			}
		}
		list.Sort(delegate(CombatUnitEntry left, CombatUnitEntry right)
		{
			float num2 = ((left != null && left.Transform != null) ? ((Vector2)left.Transform.position - center).sqrMagnitude : float.MaxValue);
			float value = ((right != null && right.Transform != null) ? ((Vector2)right.Transform.position - center).sqrMagnitude : float.MaxValue);
			return num2.CompareTo(value);
		});
		if (maxTargetCount > 0 && list.Count > maxTargetCount)
		{
			list.RemoveRange(maxTargetCount, list.Count - maxTargetCount);
		}
		return list;
	}

	/*
	 * ResolveStatusStacks 결과를 계산해 반환한다.
	 */
	private static int ResolveStatusStacks(UnitCombatState target, StatusEffectKind kind)
	{
		if (target == null || kind == StatusEffectKind.None)
		{
			return 0;
		}
		if (kind == StatusEffectKind.Shield)
		{
			if (target.Resources == null || !(target.Resources.CurrentShield > 0f))
			{
				return 0;
			}
			return 1;
		}
		if (target.Statuses == null)
		{
			return 0;
		}
		return target.Statuses.GetStacks(kind);
	}

	/*
	 * HasStatus 조건을 만족하는지 확인한다.
	 */
	private static bool HasStatus(UnitCombatState target, StatusEffectKind kind)
	{
		if (target != null && target.Statuses != null && kind != StatusEffectKind.None)
		{
			return target.Statuses.Has(kind);
		}
		return false;
	}

	/*
	 * TryApplyStatus 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryApplyStatus(InGameCombatManager manager, UnitCombatState target, ProjectileStatusHitSpec statusSpec, UnitCombatState source)
	{
		StatusCombatRules.ApplyStatus(manager, target, statusSpec, source);
	}
}

}
