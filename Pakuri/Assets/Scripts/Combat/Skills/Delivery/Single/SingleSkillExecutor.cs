/*
 * 역할: 단일 대상 스킬 전달 조정.
 * 책임: 즉시·연쇄·차지·돌진 등 단일 대상 전달 방식을 실행한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

/// <summary><c>SingleSkillExecutor</c>에 해당하는 런타임 동작을 실행한다.</summary>
internal static class SingleSkillExecutor
{

	private static bool applyingHitEnhancement;

	/// <summary>전달된 런타임 입력값을 사용해 <c>HitEnhancements</c>를 적용한다.</summary>
	internal static void ApplyHitEnhancements(
	    InGameCombatManager manager,
	    UnitSpawnManager roster,
	    SkillUseState runtime,
	    SkillExecutionData skillData,
	    CombatUnitEntry sourceEntry,
	    UnitCombatState source,
	    string sourceSkillId,
	    CombatUnitEntry hitTarget,
	    Vector2 hitPosition,
	    float primaryBaseDamage)
	{
	    if (manager != null && roster != null && source != null && hitTarget != null && hitTarget.Model != null)
	    {
	        var actionExecutionContext = new SkillExecutionContext(
	            manager,
	            roster,
	            sourceEntry,
	            runtime,
	            hitTarget.Model,
	            publishSkillLifecycleEvents: runtime != null,
	            sourceSkillId: sourceSkillId);
	        SkillTrigger.PublishLifecycleEvent(
	            SkillTriggerEvent.OnHit,
	            new SkillActionContext(
	                source,
	                sourceSkillId,
	                hitTarget.Model,
	                hitPosition,
	                primaryBaseDamage,
	                1,
	                skillData,
	                actionExecutionContext));
	    }

	    if (manager == null
	        || roster == null
	        || skillData == null
	        || source == null
	        || hitTarget == null
	        || hitTarget.Model == null
	        || primaryBaseDamage <= 0f
	        || applyingHitEnhancement)
	    {
	        return;
	    }

	    var hasReloadReduction = !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillId)
	        && skillData.ReloadReduceSecondsPerHit > 0f;
	    if (!skillData.HasOnHitAdditionalDamageBehavior && !hasReloadReduction)
	    {
	        return;
	    }

	    var hitIndex = 0;
	    if (runtime != null)
	    {
	        hitIndex = runtime.AdvanceSkillHitCount();
	    }

	    applyingHitEnhancement = true;
	    try
	    {
	        if (hasReloadReduction && runtime != null && runtime.Owner != null && runtime.Owner.Skills != null)
	        {
	            var reloadSkill = runtime.Owner.SkillState.FindBySkillId(skillData.ReloadReduceTargetSkillId);
	            if (reloadSkill != null && reloadSkill.IsReloading)
	            {
	                reloadSkill.ReduceReloadRemaining(skillData.ReloadReduceSecondsPerHit);
	            }
	        }

	        var targetsHitUnit = string.IsNullOrWhiteSpace(skillData.OnHitAdditionalDamageTarget)
	            || string.Equals(skillData.OnHitAdditionalDamageTarget, "HitTarget", StringComparison.OrdinalIgnoreCase);
	        if (skillData.HasOnHitAdditionalDamage
	            && skillData.OnHitAdditionalDamageMultiplier > 0f
	            && targetsHitUnit
	            && hitTarget.IsAlive
	            && UnityEngine.Random.value <= Mathf.Clamp01(skillData.OnHitAdditionalDamageChance))
	        {
	            manager.ApplyDamage(
	                hitTarget.Model,
                primaryBaseDamage,
	                skillData.OnHitAdditionalDamageAttribute,
	                source,
	                criticalAllowed: false,
	                0f,
	                0f,
	                sourceSkillId,
                suppressOutgoingDamageTriggers: true,
                finalDamageMultiplier: skillData.OnHitAdditionalDamageMultiplier);
	        }

	        if (skillData.HasOnHitChainDamageBehavior
	            && hitIndex > 0
	            && hitIndex % skillData.OnHitChainHitPeriod == 0)
	        {
	            var chainTargets = SkillTargeting.ChainTargets(
	                roster,
	                sourceEntry,
	                source,
	                hitTarget,
	                hitPosition,
	                skillData.OnHitChainSearchRadius);
	            var targetCount = Mathf.Min(skillData.OnHitChainTargetCount, chainTargets.Count);
	            for (var i = 0; i < targetCount; i++)
	            {
	                var chainTarget = chainTargets[i];
	                if (chainTarget != null && chainTarget.IsAlive && chainTarget.Model != null)
	                {
	                    manager.ApplyDamage(
	                        chainTarget.Model,
                        primaryBaseDamage,
	                        skillData.OnHitChainDamageAttribute,
	                        source,
	                        criticalAllowed: false,
	                        0f,
	                        0f,
	                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true,
                        finalDamageMultiplier: skillData.OnHitChainDamageMultiplier);
	                }
	            }
	        }
	    }
	    finally
	    {
	        applyingHitEnhancement = false;
	    }
	}

	/// <summary><c>SingleExecutionOutcome</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
	private readonly struct SingleExecutionOutcome
	{
		public bool Routed { get; }

		public bool CastCommitted { get; }

		/// <summary><c>SingleExecutionOutcome</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
		public SingleExecutionOutcome(bool routed, bool castCommitted)
		{
			Routed = routed;
			CastCommitted = castCommitted;
		}
	}

	/// <summary><c>SingleFollowUpSpec</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
	private readonly struct SingleFollowUpSpec
	{
		public StatusEffectKind RequiredStatusKind { get; }

		public int RepeatCount { get; }

		public float IntervalSeconds { get; }

		public float DamageMultiplier { get; }

		public GameObject Prefab { get; }

		/// <summary><c>SingleFollowUpSpec</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
		public SingleFollowUpSpec(StatusEffectKind requiredStatusKind, int repeatCount, float intervalSeconds, float damageMultiplier, GameObject prefab)
		{
			RequiredStatusKind = requiredStatusKind;
			RepeatCount = repeatCount;
			IntervalSeconds = intervalSeconds;
			DamageMultiplier = damageMultiplier;
			Prefab = prefab;
		}
	}

	/// <summary><c>SingleFollowUpTarget</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
	private readonly struct SingleFollowUpTarget
	{
		public UnitCombatState Model { get; }

		public Vector2 Center { get; }

		/// <summary><c>SingleFollowUpTarget</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
		public SingleFollowUpTarget(UnitCombatState model, Vector2 center)
		{
			Model = model;
			Center = center;
		}
	}

	/// <summary><c>TargetDamageResolution</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
	private readonly struct TargetDamageResolution
	{
		public float Damage { get; }

		public float FinalDamageMultiplier { get; }

		public float CritChanceBonus { get; }

		public bool IsExecute { get; }

		public int PendingConsumedStacks { get; }

		/// <summary><c>TargetDamageResolution</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
		public TargetDamageResolution(float damage, float finalDamageMultiplier, float critChanceBonus, bool isExecute, int pendingConsumedStacks)
		{
			Damage = damage;
			FinalDamageMultiplier = finalDamageMultiplier;
			CritChanceBonus = critChanceBonus;
			IsExecute = isExecute;
			PendingConsumedStacks = pendingConsumedStacks;
		}
	}

	private const float DefaultVisualLifetimeSeconds = 1f;

	private const float PostDamageLifetimePaddingSeconds = 0.05f;

	/// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
	internal static bool Execute(SkillExecutionContext context, SkillExecutionData snapshot, SingleChainSkillDefinition skill)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
	internal static bool Execute(SkillExecutionContext context, SkillExecutionData snapshot, SingleChargeSkillDefinition skill)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>ChainAfterDelay</c>를 실행한다.</summary>
	private static IEnumerator ExecuteChainAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, SingleChainSkillDefinition skill, UnitCombatState primary)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, skill.ChainDelaySeconds));
		ExecuteChain(context, snapshot, skill, primary);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>Chain</c>를 실행한다.</summary>
	private static void ExecuteChain(SkillExecutionContext context, SkillExecutionData snapshot, SingleChainSkillDefinition skill, UnitCombatState primary)
	{
		List<CombatUnitEntry> list = SkillTargeting.OrderedTargets(context, skill.Targeting);
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>ChainHit</c>를 적용한다.</summary>
	private static void ApplyChainHit(SkillExecutionContext context, SkillExecutionData snapshot, SingleChainSkillDefinition skill, CombatUnitEntry target, float multiplier)
	{
		float baseDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage);
		var finalDamageMultiplier = Mathf.Max(0f, snapshot.DamageMultiplier) * Mathf.Max(0f, multiplier);
		context.CombatManager.ApplyDamage(target.Model, baseDamage, skill.Damage.Element, context.Caster, skill.Damage.CriticalAllowed, 0f, 0f, skill.SkillId, finalDamageMultiplier: finalDamageMultiplier);
		EffectManager effects = context.CombatManager.Effects;
		if (effects != null)
		{
			var visualName = "RuntimeSupportVisual";
			if (!string.IsNullOrWhiteSpace(skill.SkillId))
			{
				visualName = "RuntimeSupportVisual_" + skill.SkillId;
			}

			var visualInstance = effects.CreateEffect(new EffectCreateRequest(
				skill.RuntimeVisual,
				null,
				visualName,
				target.Transform.position,
				Quaternion.identity,
				null,
				0f,
				null,
				false,
				true,
				false));
			if (visualInstance != null)
			{
				SingleSkillActor.Attach(visualInstance).InitializeFollowing(
					effects,
					target.Transform,
					0.8f,
					Vector3.zero);
			}
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>설정된 런타임 작업</c>를 실행한다.</summary>
	internal static bool Execute(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill)
	{
		if (SingleSkillRules.ShouldRejectCastForExecuteThreshold(context, snapshot, skill))
		{
			return false;
		}
		Vector2 vector = AreaCenter(context, skill.Targeting, skill.Area);
		EffectManager effects = context.CombatManager.Effects;
		RuntimeSkillVisualSpec runtimeVisual = skill.RuntimeVisual;
		bool num = effects != null && runtimeVisual != null && runtimeVisual.HasVisual();
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
		if (!singleExecutionOutcome.Routed)
		{
			return singleExecutionOutcome.CastCommitted;
		}
		return true;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>AreaCenter</c> 결과값을 생성해 반환한다.</summary>
	private static Vector2 AreaCenter(SkillExecutionContext context, SkillTargetingSpec targeting, AreaBlueprintSpec area)
	{
		return SkillTargeting.AreaCenter(context, targeting, area);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>Radius</c> 결과값을 생성해 반환한다.</summary>
	private static float Radius(SingleSkillDefinition skill, SkillExecutionData snapshot)
	{
		AreaBlueprintSpec area = null;
		SkillTargetingSpec targeting = null;
		if (skill != null)
		{
			area = skill.Area;
			targeting = skill.Targeting;
		}
		return SkillTargeting.Radius(
			SkillTargeting.BaseRadius(targeting, area),
			snapshot.RadiusMultiplier,
			snapshot.RadiusBonus);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>PrefabHitboxCenter</c> 결과값을 생성해 반환한다.</summary>
	private static Vector2 PrefabHitboxCenter(SkillExecutionContext context, Vector2 fallbackCenter, SingleSkillDefinition skill)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>DeploymentCount</c> 결과값을 생성해 반환한다.</summary>
	private static int DeploymentCount(SingleSkillDefinition skill, SkillExecutionData snapshot)
	{
		if (skill == null || !skill.UseMultiDeployment)
		{
			return 1;
		}
		int num = snapshot?.HitTargetCountBonus ?? 0;
		return Mathf.Max(1, skill.DeploymentCount + num);
	}

	/// <summary>전달된 <c>skill</c> 값을 사용해 <c>UsesStatusFilteredDeployments</c> 조건을 평가하고 결과를 반환한다.</summary>
	private static bool UsesStatusFilteredDeployments(SingleSkillDefinition skill)
	{
		if (skill != null)
		{
			return !string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId);
		}
		return false;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>EffectiveHitTargetCount</c> 결과값을 생성해 반환한다.</summary>
	private static int EffectiveHitTargetCount(SingleSkillDefinition skill, SkillExecutionData snapshot)
	{
		if (skill == null)
		{
			return 1;
		}
		if (skill.HitAllTargets || skill.HitTargetCount == int.MaxValue)
		{
			return int.MaxValue;
		}
		int num = snapshot?.HitTargetCountBonus ?? 0;
		return Mathf.Max(1, skill.HitTargetCount + num);
	}

	/// <summary>전달된 <c>skill</c> 값을 사용해 <c>UsesResolvedDeployments</c> 조건을 평가하고 결과를 반환한다.</summary>
	private static bool UsesResolvedDeployments(SingleSkillDefinition skill)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>DeploymentCenters</c> 결과값을 생성해 반환한다.</summary>
	private static List<Vector2> DeploymentCenters(SkillExecutionContext context, SingleSkillDefinition skill, Vector2 primaryCenter, int deploymentCount)
	{
		if (UsesStatusFilteredDeployments(skill))
		{
			int requiredStatusMinStacks = Mathf.Max(1, skill.DeploymentRequiredTargetStatusMinStacks);
			CombatUnitEntry casterEntry = null;
			UnitSpawnManager roster = null;
			if (context != null)
			{
				casterEntry = context.CasterEntry;
				roster = context.Roster;
			}
			List<CombatUnitEntry> list = SkillTargeting.OrderedTargets(casterEntry, roster, skill.Targeting, skill.DeploymentRequiredTargetStatusKind, requiredStatusMinStacks);
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
		return SkillTargeting.TargetAnchoredCenters(context, targeting, primaryCenter, deploymentCount, coverAll, SkillDeploymentRepeatMode.RepeatNearest);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ResolvedDeployments</c>를 실행한다.</summary>
	private static SingleExecutionOutcome ExecuteResolvedDeployments(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, Vector2 primaryCenter, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab)
	{
		int deploymentCount = DeploymentCount(skill, snapshot);
		List<Vector2> list = DeploymentCenters(context, skill, primaryCenter, deploymentCount);
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < list.Count; i++)
		{
			Vector2 vector = list[i];
			SingleExecutionOutcome singleExecutionOutcome = ExecuteAtCenter(context, snapshot, skill, vector, runtimeVisual, prefab, allowConditionalFollowUp: true);
			flag = flag || singleExecutionOutcome.Routed;
			flag2 = flag2 || singleExecutionOutcome.CastCommitted;
			PublishDeploymentLifecycle(context, snapshot, skill, vector);
			ScheduleRepeatedDeployments(context, snapshot, skill, vector, runtimeVisual, prefab);
		}
		return new SingleExecutionOutcome(flag, flag2);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ScheduleRepeatedDeployments</c> 작업을 수행한다.</summary>
	private static void ScheduleRepeatedDeployments(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab)
	{
		if (context == null || context.CombatManager == null || skill == null || snapshot == null || snapshot.RepeatCountPerTarget <= 0)
		{
			return;
		}
		SkillExecutionData snapshot2 = snapshot;
		if (!Mathf.Approximately(snapshot.RepeatDamageMultiplier, 1f))
		{
			snapshot2 = snapshot.CopyWithDamageMultiplier(snapshot.RepeatDamageMultiplier);
		}
		for (int i = 1; i <= snapshot.RepeatCountPerTarget; i++)
		{
			float num = Mathf.Max(0f, snapshot.RepeatIntervalSeconds * (float)i);
			if (num <= 0f)
			{
				ExecuteAtCenter(context, snapshot2, skill, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
				PublishDeploymentLifecycle(context, snapshot2, skill, center);
			}
			else
			{
				context.CombatManager.StartCoroutine(ExecuteRepeatedDeploymentAfterDelay(context, snapshot2, skill, center, runtimeVisual, prefab, num));
			}
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>RepeatedDeploymentAfterDelay</c>를 실행한다.</summary>
	private static IEnumerator ExecuteRepeatedDeploymentAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null && skill != null)
		{
			ExecuteAtCenter(context, snapshot, skill, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
			PublishDeploymentLifecycle(context, snapshot, skill, center);
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>PublishDeploymentLifecycle</c> 작업을 수행한다.</summary>
	private static void PublishDeploymentLifecycle(
		SkillExecutionContext context,
		SkillExecutionData snapshot,
		SingleSkillDefinition skill,
		Vector2 center)
	{
		if (context == null || skill == null)
		{
			return;
		}

		SkillTrigger.PublishLifecycleEvent(
			SkillTriggerEvent.OnDeploymentCast,
			new SkillActionContext(context.Caster, context.SourceSkillId, null, center, 0f, 0, snapshot, context));
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>AtCenter</c>를 실행한다.</summary>
	private static SingleExecutionOutcome ExecuteAtCenter(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, bool allowConditionalFollowUp)
	{
		float radius = Radius(skill, snapshot);
		bool coverAll = (skill.Area != null && skill.Area.CoverAll) || (skill.Targeting != null && skill.Targeting.CoverAll);
		float damage = snapshot != null && snapshot.HasRawDamageOverride
			? snapshot.RawDamageOverride
			: DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage);
		DamageAttribute attribute = (skill.Damage != null) ? skill.Damage.Element : skill.Element;
		ProjectileStatusHitSpec statusSpec = SkillStatus.StatusSpec(skill.OnHitStatus, snapshot);
		float critChanceBonus = snapshot?.CritChanceBonus ?? 0f;
		float critDamageBonus = snapshot?.CritDamageBonus ?? 0f;
		int num = EffectiveHitTargetCount(skill, snapshot);
		float num2 = Mathf.Max(0f, skill.DamageDelaySeconds);
		SingleFollowUpSpec? followUpSpec = (allowConditionalFollowUp ? FollowUpSpec(snapshot, statusSpec, prefab) : ((SingleFollowUpSpec?)null));
		List<SingleFollowUpTarget> followUpTargets = (followUpSpec.HasValue ? new List<SingleFollowUpTarget>() : null);
		SkillUseState skillRuntimeInstance = null;
		if (allowConditionalFollowUp)
		{
			skillRuntimeInstance = context.Runtime;
		}
		bool flag = false;
		bool flag2 = false;
		bool castCommitted = false;
		EffectManager effects = context.CombatManager.Effects;
		bool flag3 = effects != null && runtimeVisual != null && runtimeVisual.HasVisual();
		if (skill.UsePrefabHitbox && (flag3 || prefab != null) && effects != null)
		{
			center = PrefabHitboxCenter(context, center, skill);
			GameObject gameObject = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleHitbox", center, Quaternion.identity, null, 0f, null, false, true, false));
			if (gameObject != null)
			{
				flag = true;
				castCommitted = true;
				if (!flag3)
				{
					EffectVisualBuilder.ConfigureAreaEffect(
						gameObject,
						SkillTargeting.BaseRadius(skill.Targeting, skill.Area),
						snapshot.RadiusMultiplier,
						snapshot.RadiusBonus);
				}
				if (num2 > 0f)
				{
					context.CombatManager.StartCoroutine(ApplyPrefabHitboxAfterDelay(context, snapshot, skill, gameObject, num, damage, attribute, statusSpec, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
				}
				else
				{
					flag2 = ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, gameObject, num, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
				}
				float visualLifetime = Mathf.Max(num2 + 0.05f, 1f);
				SingleSkillActor.Attach(gameObject).InitializeAnimation(effects, visualLifetime);
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
					var visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, null, 0f, null, false, true, false));
					if (visualInstance != null)
					{
						SingleSkillActor.Attach(visualInstance).InitializeAnimation(effects, visualLifetime);
					}
				}
				context.CombatManager.StartCoroutine(ApplyNonPrefabTargetsAfterDelay(context, snapshot, skill, center, radius, coverAll, num, damage, attribute, statusSpec, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
			}
			else
			{
				flag2 = ApplyNonPrefabTargets(context, snapshot, skill, center, radius, coverAll, num, damage, attribute, statusSpec, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
				if (flag2 && effects != null)
				{
					var visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, null, 0f, null, false, true, false));
					if (visualInstance != null)
					{
						SingleSkillActor.Attach(visualInstance).InitializeAnimation(effects, 1f);
					}
				}
			}
		}
		if (allowConditionalFollowUp && num2 <= 0f)
		{
			ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
		}
		return new SingleExecutionOutcome(flag2, castCommitted);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>NonPrefabTargets</c>를 적용한다.</summary>
	private static bool ApplyNonPrefabTargets(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillUseState onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null || skill == null)
		{
			return false;
		}
		if (skill.UsesHitTargetCount && !skill.HitAllTargets)
		{
			return ApplyLimitedTargets(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, effectiveHitTargetCount, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, center, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
		}
		return ApplyAreaTargets(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, center, radius, coverAll, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>NonPrefabTargetsAfterDelay</c>를 적용한다.</summary>
	private static IEnumerator ApplyNonPrefabTargetsAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillUseState onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, float delaySeconds, bool allowConditionalFollowUp)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		ApplyNonPrefabTargets(context, snapshot, skill, center, radius, coverAll, effectiveHitTargetCount, damage, attribute, statusSpec, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
		if (allowConditionalFollowUp)
		{
			ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>PrefabHitboxAfterDelay</c>를 적용한다.</summary>
	private static IEnumerator ApplyPrefabHitboxAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, GameObject instance, int effectiveHitTargetCount, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, SkillUseState onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, float delaySeconds, bool allowConditionalFollowUp)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.CasterEntry != null && context.Roster != null && skill != null && !(instance == null))
		{
			ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, skill, skill.Targeting, instance, effectiveHitTargetCount, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
			if (allowConditionalFollowUp)
			{
				ScheduleConditionalFollowUps(context, snapshot, skill, followUpSpec, followUpTargets);
			}
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>PrefabHitbox</c>를 적용한다.</summary>
	private static bool ApplyPrefabHitbox(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SingleSkillDefinition skill, SkillTargetingSpec targetingSpec, GameObject hitboxObject, int maxTargets, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillUseState sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionData snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, UnitCombatState eventTarget, bool lockToEventTarget)
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
		Collider2D[] array = CoreHitboxColliders(hitboxObject, snapshot);
		List<CombatUnitEntry> list = SkillTargeting.OrderedTargets(sourceEntry, unitRoster, targetingSpec, eventTarget, lockToEventTarget);
		List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>();
		List<CombatUnitEntry> coreCollisionTargets = new List<CombatUnitEntry>();
		UnitCollisionResolver.CollectTargets(unitRoster, list, componentsInChildren, Vector2.zero, collisionTargets);
		if (array.Length != 0)
		{
			UnitCollisionResolver.CollectTargets(unitRoster, list, array, Vector2.zero, coreCollisionTargets);
		}
		bool result = false;
		int num = 0;
		for (int i = 0; i < collisionTargets.Count; i++)
		{
			CombatUnitEntry unitEntry = collisionTargets[i];
			if (unitEntry != null && unitEntry.Model != null && unitEntry.Transform != null)
			{
				RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry, (unitEntry != null && unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : Vector2.zero);
				Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : Vector2.zero);
				bool isCoreHit = coreCollisionTargets.Contains(unitEntry);
				TargetDamageResolution damageResolution = TargetDamage(source, skill, snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit);
				InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute, finalDamageMultiplier: damageResolution.FinalDamageMultiplier);
				int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, skill, damageResolution);
				SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result2, damageResolution.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
				if (!result2.IsDead)
				{
					TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
				}
				TryApplyCoreOnHitAdditionalDamage(manager, snapshot, source, sourceSkillId, unitEntry, damageResolution.Damage, isCoreHit);
				SingleSkillExecutor.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>LimitedTargets</c>를 적용한다.</summary>
	private static bool ApplyLimitedTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SingleSkillDefinition skill, SkillTargetingSpec targetingSpec, int maxTargets, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillUseState sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionData snapshot, Vector2 center, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, UnitCombatState eventTarget, bool lockToEventTarget)
	{
		if (manager == null || sourceEntry == null || unitRoster == null || maxTargets <= 0)
		{
			return false;
		}
		List<CombatUnitEntry> list = SkillTargeting.OrderedTargets(sourceEntry, unitRoster, targetingSpec, eventTarget, lockToEventTarget);
		bool result = false;
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			CombatUnitEntry unitEntry = list[i];
			RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry, center);
			Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : center);
			TargetDamageResolution damageResolution = TargetDamage(source, skill, snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
			InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute, finalDamageMultiplier: damageResolution.FinalDamageMultiplier);
			int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, skill, damageResolution);
			SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result2, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
			if (!result2.IsDead)
			{
				TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
			}
			SingleSkillExecutor.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>AreaTargets</c>를 적용한다.</summary>
	private static bool ApplyAreaTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SingleSkillDefinition skill, SkillTargetingSpec targetingSpec, Vector2 center, float radius, bool coverAll, float damage, DamageAttribute attribute, ProjectileStatusHitSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillUseState sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionData snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, UnitCombatState eventTarget, bool lockToEventTarget)
	{
		if (manager == null || sourceEntry == null || unitRoster == null)
		{
			return false;
		}
		List<CombatUnitEntry> list = SkillTargeting.OrderedTargets(sourceEntry, unitRoster, targetingSpec, eventTarget, lockToEventTarget);
		if (!coverAll && radius <= 0f)
		{
			CombatUnitEntry unitEntry = ((list.Count > 0) ? list[0] : null);
			if (unitEntry == null || !unitEntry.IsAlive || unitEntry.Model == null)
			{
				return false;
			}
			RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry, center);
			Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : center);
			TargetDamageResolution damageResolution = TargetDamage(source, skill, snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
			InGameResourceChangeResult result = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute, finalDamageMultiplier: damageResolution.FinalDamageMultiplier);
			int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, skill, damageResolution);
			SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result, consumedStacks);
			if (!result.IsDead)
			{
				TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
			}
			SingleSkillExecutor.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
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
				TargetDamageResolution damageResolution2 = TargetDamage(source, skill, snapshot, damage, unitEntry2.Model, critChanceBonus, isCoreHit: false);
				InGameResourceChangeResult result3 = manager.ApplyDamage(unitEntry2.Model, damageResolution2.Damage, attribute, source, criticalAllowed, damageResolution2.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution2.IsExecute, finalDamageMultiplier: damageResolution2.FinalDamageMultiplier);
				int consumedStacks2 = ConsumePendingTargetStatusStacks(manager, unitEntry2.Model, skill, damageResolution2);
				SingleSkillRules.HandleKillRecovery(sourceRuntime, skill, snapshot, result3, damageResolution2.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry2, result3, consumedStacks2);
				if (!result3.IsDead)
				{
					TryApplyStatus(manager, unitEntry2.Model, statusSpec, source);
				}
				SingleSkillExecutor.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry2, hitPosition2, damageResolution2.Damage);
				result2 = true;
				num++;
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, skill, snapshot, num, center);
		return result2;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>CoreHitboxColliders</c> 결과값을 생성해 반환한다.</summary>
	private static Collider2D[] CoreHitboxColliders(GameObject hitboxObject, SkillExecutionData snapshot)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>ApplyCoreOnHitAdditionalDamage</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
	private static void TryApplyCoreOnHitAdditionalDamage(InGameCombatManager manager, SkillExecutionData snapshot, UnitCombatState source, string sourceSkillId, CombatUnitEntry target, float primaryDamage, bool isCoreHit)
	{
		if (isCoreHit && !(manager == null) && snapshot != null && snapshot.HasCoreOnHitAdditionalDamage && !(snapshot.CoreOnHitAdditionalDamageMultiplier <= 0f) && source != null && target != null && target.IsAlive && target.Model != null && !(primaryDamage <= 0f) && !(UnityEngine.Random.value > Mathf.Clamp01(snapshot.CoreOnHitAdditionalDamageChance)))
		{
			manager.ApplyDamage(target.Model, primaryDamage * snapshot.CoreOnHitAdditionalDamageMultiplier, snapshot.CoreOnHitAdditionalDamageAttribute, source, criticalAllowed: false, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: true);
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ApplyHitCountCooldownRefund</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
	private static void TryApplyHitCountCooldownRefund(SkillUseState sourceRuntime, SkillExecutionData snapshot, int hitCount)
	{
		if (sourceRuntime != null && sourceRuntime.Owner != null && sourceRuntime.Owner.Skills != null && snapshot != null && hitCount >= snapshot.HitCountCooldownRefundMinTargets && !string.IsNullOrWhiteSpace(snapshot.HitCountCooldownRefundTargetSkillId) && !(snapshot.HitCountCooldownRefundRatio <= 0f))
		{
			SkillUseState skillRuntimeInstance = sourceRuntime.Owner.SkillState.FindBySkillId(snapshot.HitCountCooldownRefundTargetSkillId);
			skillRuntimeInstance?.ReduceCooldownRemaining(skillRuntimeInstance.EffectiveCooldownDuration * Mathf.Clamp01(snapshot.HitCountCooldownRefundRatio));
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteOnHitCountEffects</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
	private static void TryExecuteOnHitCountEffects(InGameCombatManager manager, UnitSpawnManager roster, CombatUnitEntry sourceEntry, SkillUseState sourceRuntime, SingleSkillDefinition skill, SkillExecutionData snapshot, int hitCount, Vector2 center)
	{
		if (!(manager == null) && roster != null && sourceEntry != null && skill != null && hitCount > 0)
		{
			var executionContext = new SkillExecutionContext(
				manager,
				roster,
				sourceEntry,
				sourceRuntime,
				publishSkillLifecycleEvents: sourceRuntime != null);
			SkillTrigger.PublishLifecycleEvent(
				SkillTriggerEvent.OnHitCount,
				new SkillActionContext(sourceEntry.Model, skill.SkillId, null, center, 0f, hitCount, snapshot, executionContext));
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>FollowUpSpec</c> 결과값을 생성해 반환한다.</summary>
	private static SingleFollowUpSpec? FollowUpSpec(SkillExecutionData snapshot, ProjectileStatusHitSpec statusSpec, GameObject prefab)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>FollowUpTarget</c>를 소유 런타임 Registry에 등록한다.</summary>
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>ScheduleConditionalFollowUps</c> 작업을 수행한다.</summary>
	private static void ScheduleConditionalFollowUps(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>ConditionalFollowUpAfterDelay</c>를 실행한다.</summary>
	private static IEnumerator ExecuteConditionalFollowUpAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, SingleSkillDefinition skill, SingleFollowUpTarget followUpTarget, SingleFollowUpSpec followUpSpec, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null && skill != null)
		{
			CombatUnitEntry unitEntry = ((followUpTarget.Model != null) ? context.Roster.Find(followUpTarget.Model) : null);
			Vector2 center = ((unitEntry != null && unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : followUpTarget.Center);
			SkillExecutionData snapshot2 = null;
			if (snapshot != null)
			{
				snapshot2 = snapshot.CopyWithDamageMultiplier(followUpSpec.DamageMultiplier);
			}
			ExecuteAtCenter(context, snapshot2, skill, center, null, followUpSpec.Prefab, allowConditionalFollowUp: false);
		}
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>TargetDamage</c> 결과값을 생성해 반환한다.</summary>
	private static TargetDamageResolution TargetDamage(UnitCombatState caster, SingleSkillDefinition skill, SkillExecutionData snapshot, float baseDamage, UnitCombatState target, float baseCritChanceBonus, bool isCoreHit)
	{
		float num = Mathf.Max(0f, baseDamage + TargetStatusStackAdditionalDamage(caster, skill, snapshot, target, baseDamage));
		float num2 = 1f;
		float critChanceBonus = baseCritChanceBonus;
		if (snapshot != null)
		{
				num2 = Mathf.Max(0f, snapshot.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(snapshot, target);
			critChanceBonus += SkillExecutionRuleResolver.ConditionalCritChanceBonus(snapshot, target);
		}
		bool flag = false;
		int pendingConsumedStacks = PendingConsumedStacks(skill, snapshot, target);
		if (isCoreHit && snapshot != null && snapshot.HasCoreDamageMultiplier)
		{
			num2 *= snapshot.CoreDamageMultiplier;
		}
		SingleDamageModifierState singleDamageModifierState = SingleSkillRules.ApplyDamageModifiers(skill, snapshot, target, num2, critChanceBonus);
		num2 = singleDamageModifierState.DamageMultiplier;
		critChanceBonus = singleDamageModifierState.CritChanceBonus;
		flag = singleDamageModifierState.IsExecute;
		return new TargetDamageResolution(Mathf.Max(0f, num), Mathf.Max(0f, num2), critChanceBonus, flag, pendingConsumedStacks);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>TargetStatusStackAdditionalDamage</c> 결과값을 생성해 반환한다.</summary>
	private static float TargetStatusStackAdditionalDamage(UnitCombatState caster, SingleSkillDefinition skill, SkillExecutionData snapshot, UnitCombatState target, float baseDamage)
	{
		if (caster == null || skill == null || target == null || skill.TargetStatusStackDamage == null || skill.TargetStatusStackStatusKind == StatusEffectKind.None)
		{
			return 0f;
		}
		int num = StatusStacks(target, skill.TargetStatusStackStatusKind);
		if (num <= 0)
		{
			return 0f;
		}
		if (skill.TargetStatusStackMaxStacks > 0)
		{
			num = Mathf.Min(num, skill.TargetStatusStackMaxStacks);
		}
		float num2 = DamageCalculator.CalculateRawDamage(caster, skill.TargetStatusStackDamage);
		float b = 1f;
		float num3 = 0f;
		if (snapshot != null)
		{
			b = snapshot.TargetStatusStackDamageMultiplier;
			num3 = snapshot.TargetStatusStackDamageRateBonus(skill.TargetStatusStackStatusId);
		}
		float num4 = num2 * Mathf.Max(0f, b) + Mathf.Max(0f, baseDamage) * num3;
		return Mathf.Max(0f, (float)num * num4);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>PendingConsumedStacks</c> 결과값을 생성해 반환한다.</summary>
	private static int PendingConsumedStacks(SingleSkillDefinition skill, SkillExecutionData snapshot, UnitCombatState target)
	{
		if (skill == null || target == null || skill.ConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		int num = StatusStacks(target, skill.ConsumeTargetStatusKind);
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>PendingTargetStatusStacks</c>를 현재 런타임 상태에서 소비한다.</summary>
	private static int ConsumePendingTargetStatusStacks(InGameCombatManager manager, UnitCombatState target, SingleSkillDefinition skill, TargetDamageResolution damageResolution)
	{
		if (manager == null || target == null || skill == null || damageResolution.PendingConsumedStacks <= 0 || skill.ConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		return manager.ConsumeStatusStacks(target, skill.ConsumeTargetStatusKind, damageResolution.PendingConsumedStacks);
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>RedistributeConsumedStatusOnKill</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
	private static void TryRedistributeConsumedStatusOnKill(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager roster, UnitCombatState source, SkillExecutionData snapshot, CombatUnitEntry defeatedTarget, InGameResourceChangeResult result, int consumedStacks)
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
		List<CombatUnitEntry> list = RedistributionTargets(sourceEntry, roster, defeatedTarget.Transform.position, snapshot.RedistributeConsumedStatusSearchRadius, defeatedTarget.Model, snapshot.RedistributeConsumedStatusTargetCount);
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>RedistributionTargets</c> 결과값을 생성해 반환한다.</summary>
	private static List<CombatUnitEntry> RedistributionTargets(CombatUnitEntry sourceEntry, UnitSpawnManager roster, Vector2 center, float radius, UnitCombatState excludedModel, int maxTargetCount)
	{
		List<CombatUnitEntry> list = new List<CombatUnitEntry>();
		if (sourceEntry == null || roster == null || radius <= 0f)
		{
			return list;
		}
		IReadOnlyList<CombatUnitEntry> readOnlyList = SkillTargeting.TargetList(sourceEntry, roster, new SkillTargetingSpec
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

	/// <summary>전달된 런타임 입력값을 사용해 <c>StatusStacks</c> 결과값을 생성해 반환한다.</summary>
	private static int StatusStacks(UnitCombatState target, StatusEffectKind kind)
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

	/// <summary>전달된 런타임 입력값을 사용해 소유한 런타임 상태에 <c>Status</c>가 있는지 반환한다.</summary>
	private static bool HasStatus(UnitCombatState target, StatusEffectKind kind)
	{
		if (target != null && target.Statuses != null && kind != StatusEffectKind.None)
		{
			return target.Statuses.Has(kind);
		}
		return false;
	}

	/// <summary>전달된 런타임 입력값을 사용해 <c>ApplyStatus</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
	private static void TryApplyStatus(InGameCombatManager manager, UnitCombatState target, ProjectileStatusHitSpec statusSpec, UnitCombatState source)
	{
		StatusCombatRules.ApplyStatus(manager, target, statusSpec, source);
	}
}

}
