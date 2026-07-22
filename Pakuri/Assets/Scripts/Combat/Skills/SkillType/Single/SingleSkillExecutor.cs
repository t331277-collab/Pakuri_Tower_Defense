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
	/*
	 * 현재 스킬의 노드 효과 중 요청한 실행 시점에 맞는 효과를 적용한다.
	 */
	internal static bool ExecuteAdditionalEffects(
	    SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
	    SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
	    SkillEffectDefinition[] effects /* 적용할 추가 효과 목록 */,
	    Vector2 defaultCenter /* 기본 효과 중심 */,
	    bool requireTiming /* 특정 실행 시점만 처리할지 여부 */,
	    SkillMultiEffectTiming timing /* 처리할 실행 시점 */,
	    bool scaleStatusDuration /* 상태 지속시간 보정 여부 */,
	    int hitCount = 0 /* 현재 적중 횟수 */,
	    UnitCombatState eventTarget = null /* 현재 적중 대상 */,
	    bool useEventTarget = false /* 적중 대상을 문맥에 넣을지 여부 */)
	{
	    if (context == null || context.CombatManager == null || effects == null || effects.Length == 0)
	    {
	        return false;
	    }

	    var effectContext = context;
	    if (useEventTarget)
	    {
	        effectContext = new SkillExecutionContext(
	            context.CombatManager,
	            context.Roster,
	            context.CasterEntry,
	            context.Runtime,
	            eventTarget,
	            context.HasManualAimDirection,
	            context.ManualAimDirection,
	            context.HasManualTargetPoint,
	            context.ManualTargetPoint,
	            context.RecastGeneration);
	    }

	    var applied = false;
	    for (var i = 0; i < effects.Length; i++)
	    {
	        var effect = effects[i];
	        if (!SkillRequirement.CanRunEffect(effectContext, effect))
	        {
	            continue;
	        }
	        if (requireTiming)
	        {
	            if (effect.EffectTiming != timing)
	            {
	                continue;
	            }
	        }
	        else if (effect.EffectTiming == SkillMultiEffectTiming.OnHit
	            || effect.EffectTiming == SkillMultiEffectTiming.OnDeploymentCast
	            || effect.EffectTiming == SkillMultiEffectTiming.OnExpire
	            || effect.EffectTiming == SkillMultiEffectTiming.OnHitCount)
	        {
	            continue;
	        }
	        if (!SkillRequirement.MatchesEffectHitCount(effect, hitCount))
	        {
	            continue;
	        }

	        if (effect.EffectTiming == SkillMultiEffectTiming.Delayed || effect.DelaySeconds > 0f)
	        {
	            effectContext.CombatManager.StartCoroutine(ApplyAdditionalEffectAfterDelay(
	                effectContext,
	                skillData,
	                effect,
	                defaultCenter,
	                scaleStatusDuration));
	            applied = true;
	        }
	        else
	        {
	            applied = ApplyAdditionalEffect(
	                effectContext,
	                skillData,
	                effect,
	                defaultCenter,
	                scaleStatusDuration) || applied;
	        }
	    }
	    return applied;
	}

	/*
	 * 추가 효과의 지연시간이 지난 뒤 같은 Executor에서 효과를 적용한다.
	 */
	private static IEnumerator ApplyAdditionalEffectAfterDelay(
	    SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
	    SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
	    SkillEffectDefinition effect /* 적용할 추가 효과 */,
	    Vector2 defaultCenter /* 기본 효과 중심 */,
	    bool scaleStatusDuration /* 상태 지속시간 보정 여부 */)
	{
	    var delay = Mathf.Max(0f, effect.DelaySeconds);
	    if (delay > 0f)
	    {
	        yield return new WaitForSeconds(delay);
	    }
	    else
	    {
	        yield return null;
	    }
	    ApplyAdditionalEffect(context, skillData, effect, defaultCenter, scaleStatusDuration);
	}

	/*
	 * 추가 효과 종류에 맞는 실제 적용 기능을 호출한다.
	 */
	private static bool ApplyAdditionalEffect(
	    SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
	    SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
	    SkillEffectDefinition effect /* 적용할 추가 효과 */,
	    Vector2 defaultCenter /* 기본 효과 중심 */,
	    bool scaleStatusDuration /* 상태 지속시간 보정 여부 */)
	{
	    if (effect == null || context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
	    {
	        return false;
	    }

	    if (effect.EffectKind == SkillMultiEffectKind.Damage)
	    {
	        return ZoneSkillExecutor.ApplyAdditionalDamageEffect(context, skillData, effect, defaultCenter);
	    }
	    if (effect.EffectKind == SkillMultiEffectKind.Status)
	    {
	        return SkillStatus.ApplyEffect(context, skillData, effect, defaultCenter, scaleStatusDuration);
	    }
	    if (effect.EffectKind == SkillMultiEffectKind.ExtendStatusDuration)
	    {
	        return SkillStatus.ExtendEffectDuration(context, effect);
	    }
	    if (effect.EffectKind == SkillMultiEffectKind.RecastZone)
	    {
	        return ZoneSkillExecutor.ExecuteRecast(context, skillData, effect, defaultCenter);
	    }
	    return false;
	}

	private static bool applyingHitEnhancement;

	/*
	 * 적중 후 추가 피해, 연쇄 피해, 재장전 감소 강화 효과를 적용한다.
	 */
	internal static void ApplyHitEnhancements(
	    InGameCombatManager manager /* 전투 진행 관리자 */,
	    CombatUnitRegistry roster /* 전투 유닛 목록 */,
	    SkillUseState runtime /* 실행 중인 스킬 */,
	    SkillExecutionData skillData /* 현재 스킬 강화 정보 */,
	    CombatUnitEntry sourceEntry /* 시전자 등록 정보 */,
	    UnitCombatState source /* 시전자 */,
	    string sourceSkillId /* 원본 스킬 식별자 */,
	    CombatUnitEntry hitTarget /* 최초 적중 대상 */,
	    Vector2 hitPosition /* 최초 적중 위치 */,
	    float primaryBaseDamage /* 최초 적중 기본 피해 */)
	{
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
	                primaryBaseDamage * skillData.OnHitAdditionalDamageMultiplier,
	                skillData.OnHitAdditionalDamageAttribute,
	                source,
	                criticalAllowed: false,
	                0f,
	                0f,
	                sourceSkillId,
	                suppressOutgoingDamageTriggers: true);
	        }

	        if (skillData.HasOnHitChainDamageBehavior
	            && hitIndex > 0
	            && hitIndex % skillData.OnHitChainHitPeriod == 0)
	        {
	            var chainTargets = SkillTargeting.ResolveChainTargets(
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
	                        primaryBaseDamage * skillData.OnHitChainDamageMultiplier,
	                        skillData.OnHitChainDamageAttribute,
	                        source,
	                        criticalAllowed: false,
	                        0f,
	                        0f,
	                        sourceSkillId,
	                        suppressOutgoingDamageTriggers: true);
	                }
	            }
	        }
	    }
	    finally
	    {
	        applyingHitEnhancement = false;
	    }
	}

	private readonly struct SingleExecutionOutcome
	{
		public bool Routed { get; }

		public bool CastCommitted { get; }

		/*
		 * SingleExecutionOutcome에 필요한 값을 초기화한다.
		 */
		public SingleExecutionOutcome(bool routed /* 경로 결정 여부 */, bool castCommitted /* 스킬 사용 확정 여부 */)
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
		public SingleFollowUpSpec(StatusEffectKind requiredStatusKind /* 필수 상태 효과 종류 여부 */, int repeatCount /* 반복 개수 */, float intervalSeconds /* 간격 초 */, float damageMultiplier /* 피해량에 곱할 배율 */, GameObject prefab /* 생성할 프리팹 */)
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
		public SingleFollowUpTarget(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, Vector2 center /* 효과가 적용될 중심 위치 */)
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
		public TargetDamageResolution(float damage /* 적용하거나 전달할 피해량 */, float critChanceBonus /* 추가 치명타 확률 */, bool isExecute /* 여부 처형 여부 */, int plannedConsumedStacks /* 예정된 소모된 중첩 수 */)
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
	internal static bool Execute(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleChainSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
	internal static bool Execute(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleChargeSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
	private static IEnumerator ExecuteChainAfterDelay(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleChainSkillDefinition skill /* 실행하거나 검사할 스킬 */, UnitCombatState primary /* 주 대상 */)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, skill.ChainDelaySeconds));
		ExecuteChain(context, snapshot, skill, primary);
	}

	/*
	 * ExecuteChain 실행을 처리한다.
	 */
	private static void ExecuteChain(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleChainSkillDefinition skill /* 실행하거나 검사할 스킬 */, UnitCombatState primary /* 주 대상 */)
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
	private static void ApplyChainHit(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleChainSkillDefinition skill /* 실행하거나 검사할 스킬 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */, float multiplier /* 값에 곱할 배율 */)
	{
		float baseDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage, snapshot.BaseDamageBonus, snapshot.DamageMultiplier) * Mathf.Max(0f, multiplier);
		context.CombatManager.ApplyDamage(target.Model, baseDamage, skill.Damage.Element, context.Caster, skill.Damage.CriticalAllowed, 0f, 0f, skill.SkillId);
		EffectManager effects = context.CombatManager.Effects;
		if (effects != null)
		{
			var visualName = "RuntimeSupportVisual";
			if (!string.IsNullOrWhiteSpace(skill.SkillId))
			{
				visualName = "RuntimeSupportVisual_" + skill.SkillId;
			}

			var visualInstance = effects.CreateEffect(
				skill.RuntimeVisual,
				null,
				visualName,
				target.Transform.position,
				Quaternion.identity);
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

	/*
	 * Execute 실행 결과를 반환한다.
	 */
	internal static bool Execute(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */)
	{
		if (SingleSkillRules.ShouldRejectCastForExecuteThreshold(context, snapshot, skill))
		{
			return false;
		}
		Vector2 vector = ResolveAreaCenter(context, skill.Targeting, skill.Area);
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
		bool flag = ExecuteAdditionalEffects(context, snapshot, skill.MultiEffects, vector, false, SkillMultiEffectTiming.OnCast, false);
		if (!(singleExecutionOutcome.Routed || flag))
		{
			return singleExecutionOutcome.CastCommitted;
		}
		return true;
	}

	/*
	 * ResolveAreaCenter 결과를 계산해 반환한다.
	 */
	private static Vector2 ResolveAreaCenter(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */, AreaBlueprintSpec area /* 범위 */)
	{
		return SkillTargeting.ResolveAreaCenter(context, targeting, area);
	}

	/*
	 * ResolveRadius 결과를 계산해 반환한다.
	 */
	private static float ResolveRadius(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
	{
		AreaBlueprintSpec area = null;
		SkillTargetingSpec targeting = null;
		if (skill != null)
		{
			area = skill.Area;
			targeting = skill.Targeting;
		}
		return SkillTargeting.ResolveRadius(
			SkillTargeting.ResolveBaseRadius(targeting, area),
			snapshot.RadiusMultiplier,
			snapshot.RadiusBonus);
	}

	/*
	 * ResolvePrefabHitboxCenter 결과를 계산해 반환한다.
	 */
	private static Vector2 ResolvePrefabHitboxCenter(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, Vector2 fallbackCenter /* 중심을 정하지 못했을 때 사용할 위치 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
	private static int ResolveDeploymentCount(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
	private static bool UsesStatusFilteredDeployments(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
	private static bool UsesSingleLineVisual(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
	private static int ResolveEffectiveHitTargetCount(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
	private static bool UsesResolvedDeployments(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */)
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
	private static List<Vector2> ResolveDeploymentCenters(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 primaryCenter /* 주 대상 중심 위치 */, int deploymentCount /* 배치 개수 */)
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
	private static SingleExecutionOutcome ExecuteResolvedDeployments(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 primaryCenter /* 주 대상 중심 위치 */, RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */, GameObject prefab /* 생성할 프리팹 */)
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
			flag = ExecuteAdditionalEffects(context, snapshot, skill.MultiEffects, vector, true, SkillMultiEffectTiming.OnDeploymentCast, false) || flag;
			ScheduleRepeatedDeployments(context, snapshot, skill, vector, runtimeVisual, prefab);
		}
		return new SingleExecutionOutcome(flag, flag2);
	}

	/*
	 * ScheduleRepeatedDeployments 작업을 수행한다.
	 */
	private static void ScheduleRepeatedDeployments(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 center /* 효과가 적용될 중심 위치 */, RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */, GameObject prefab /* 생성할 프리팹 */)
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
				ExecuteAdditionalEffects(context, snapshot2, skill.MultiEffects, center, true, SkillMultiEffectTiming.OnDeploymentCast, false);
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
	private static IEnumerator ExecuteRepeatedDeploymentAfterDelay(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 center /* 효과가 적용될 중심 위치 */, RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */, GameObject prefab /* 생성할 프리팹 */, float delaySeconds /* 실행 전 대기 시간(초) */)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null && skill != null)
		{
			ExecuteAtCenter(context, snapshot, skill, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
			ExecuteAdditionalEffects(context, snapshot, skill.MultiEffects, center, true, SkillMultiEffectTiming.OnDeploymentCast, false);
		}
	}

	/*
	 * ExecuteAtCenter 실행 결과를 반환한다.
	 */
	private static SingleExecutionOutcome ExecuteAtCenter(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 center /* 효과가 적용될 중심 위치 */, RuntimeSkillVisualSpec runtimeVisual /* 런타임 시각 효과 설정 */, GameObject prefab /* 생성할 프리팹 */, bool allowConditionalFollowUp /* 조건부 후속 공격 허용 여부 */)
	{
		float radius = ResolveRadius(skill, snapshot);
		bool coverAll = (skill.Area != null && skill.Area.CoverAll) || (skill.Targeting != null && skill.Targeting.CoverAll);
		float damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage, snapshot.BaseDamageBonus, snapshot.DamageMultiplier);
		DamageAttribute attribute = (skill.Damage != null) ? skill.Damage.Element : skill.Element;
		ProjectileStatusHitSpec statusSpec = SkillStatus.ResolveStatusSpec(skill.OnHitStatus, snapshot);
		SkillEffectDefinition[] onHitStatusEffects = ResolveOnHitStatusEffects(context, snapshot, skill.MultiEffects);
		float critChanceBonus = snapshot?.CritChanceBonus ?? 0f;
		float critDamageBonus = snapshot?.CritDamageBonus ?? 0f;
		int num = ResolveEffectiveHitTargetCount(skill, snapshot);
		float num2 = Mathf.Max(0f, skill.DamageDelaySeconds);
		SingleFollowUpSpec? followUpSpec = (allowConditionalFollowUp ? ResolveFollowUpSpec(snapshot, statusSpec, prefab) : ((SingleFollowUpSpec?)null));
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
			center = ResolvePrefabHitboxCenter(context, center, skill);
			GameObject gameObject = effects.CreateEffect(runtimeVisual, prefab, "RuntimeSingleHitbox", center, Quaternion.identity);
			if (gameObject != null)
			{
				flag = true;
				castCommitted = true;
				if (UsesSingleLineVisual(skill))
				{
					EffectVisualBuilder.ConfigureSingleAttackLine(
						gameObject.transform,
						context,
						skill,
						snapshot.RadiusMultiplier,
						snapshot.RadiusBonus,
						center);
				}
				else if (!flag3)
				{
					EffectVisualBuilder.ConfigureAreaEffect(
						gameObject,
						SkillTargeting.ResolveBaseRadius(skill.Targeting, skill.Area),
						snapshot.RadiusMultiplier,
						snapshot.RadiusBonus);
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
					var visualInstance = effects.CreateEffect(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity);
					if (visualInstance != null)
					{
						SingleSkillActor.Attach(visualInstance).InitializeAnimation(effects, visualLifetime);
					}
				}
				context.CombatManager.StartCoroutine(ApplyNonPrefabTargetsAfterDelay(context, snapshot, skill, center, radius, coverAll, num, damage, attribute, statusSpec, onHitStatusEffects, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
			}
			else
			{
				flag2 = ApplyNonPrefabTargets(context, snapshot, skill, center, radius, coverAll, num, damage, attribute, statusSpec, onHitStatusEffects, skillRuntimeInstance, skill.Damage != null && skill.Damage.CriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
				if (flag2 && effects != null)
				{
					var visualInstance = effects.CreateEffect(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity);
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

	/*
	 * ApplyNonPrefabTargets 처리를 대상에 적용한다.
	 */
	private static bool ApplyNonPrefabTargets(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 center /* 효과가 적용될 중심 위치 */, float radius /* 효과가 적용될 반지름 */, bool coverAll /* 범위 안의 모든 대상 포함 여부 */, int effectiveHitTargetCount /* 실제 적용 적중 대상 개수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, SkillEffectDefinition[] onHitStatusEffects /* 적중 시 적용할 상태 효과 목록 */, SkillUseState onHitRuntime /* 적중 효과를 실행할 스킬 정보 */, bool criticalAllowed /* 치명타 허용 여부 */, float critChanceBonus /* 추가 치명타 확률 */, float critDamageBonus /* 추가 치명타 피해 배율 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */)
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
	private static IEnumerator ApplyNonPrefabTargetsAfterDelay(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, Vector2 center /* 효과가 적용될 중심 위치 */, float radius /* 효과가 적용될 반지름 */, bool coverAll /* 범위 안의 모든 대상 포함 여부 */, int effectiveHitTargetCount /* 실제 적용 적중 대상 개수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, SkillEffectDefinition[] onHitStatusEffects /* 적중 시 적용할 상태 효과 목록 */, SkillUseState onHitRuntime /* 적중 효과를 실행할 스킬 정보 */, bool criticalAllowed /* 치명타 허용 여부 */, float critChanceBonus /* 추가 치명타 확률 */, float critDamageBonus /* 추가 치명타 피해 배율 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */, float delaySeconds /* 실행 전 대기 시간(초) */, bool allowConditionalFollowUp /* 조건부 후속 공격 허용 여부 */)
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
	private static IEnumerator ApplyPrefabHitboxAfterDelay(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, GameObject instance /* 생성된 게임 오브젝트 */, int effectiveHitTargetCount /* 실제 적용 적중 대상 개수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, SkillEffectDefinition[] onHitStatusEffects /* 적중 시 적용할 상태 효과 목록 */, SkillUseState onHitRuntime /* 적중 효과를 실행할 스킬 정보 */, bool criticalAllowed /* 치명타 허용 여부 */, float critChanceBonus /* 추가 치명타 확률 */, float critDamageBonus /* 추가 치명타 피해 배율 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */, float delaySeconds /* 실행 전 대기 시간(초) */, bool allowConditionalFollowUp /* 조건부 후속 공격 허용 여부 */)
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
	private static bool ApplyPrefabHitbox(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */, GameObject hitboxObject /* 피격 판정 게임 오브젝트 */, int maxTargets /* 처리할 수 있는 최대 대상 수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, SkillEffectDefinition[] onHitStatusEffects /* 적중 시 적용할 상태 효과 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */, bool criticalAllowed /* 치명타 허용 여부 */, float critChanceBonus /* 추가 치명타 확률 */, float critDamageBonus /* 추가 치명타 피해 배율 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */)
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

	/*
	 * ApplyLimitedTargets 처리를 대상에 적용한다.
	 */
	private static bool ApplyLimitedTargets(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */, int maxTargets /* 처리할 수 있는 최대 대상 수 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, SkillEffectDefinition[] onHitStatusEffects /* 적중 시 적용할 상태 효과 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */, bool criticalAllowed /* 치명타 허용 여부 */, float critChanceBonus /* 추가 치명타 확률 */, float critDamageBonus /* 추가 치명타 피해 배율 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, Vector2 center /* 효과가 적용될 중심 위치 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */)
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

	/*
	 * ApplyAreaTargets 처리를 대상에 적용한다.
	 */
	private static bool ApplyAreaTargets(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */, Vector2 center /* 효과가 적용될 중심 위치 */, float radius /* 효과가 적용될 반지름 */, bool coverAll /* 범위 안의 모든 대상 포함 여부 */, float damage /* 적용하거나 전달할 피해량 */, DamageAttribute attribute /* 피해 속성 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, SkillEffectDefinition[] onHitStatusEffects /* 적중 시 적용할 상태 효과 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */, bool criticalAllowed /* 치명타 허용 여부 */, float critChanceBonus /* 추가 치명타 확률 */, float critDamageBonus /* 추가 치명타 피해 배율 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */)
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
				SingleSkillExecutor.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry2, hitPosition2, damageResolution2.Damage);
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
	private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders /* 피격 판정 콜라이더 목록 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */)
	{
		return UnitHitboxOverlap.IsTargetInsideHitbox(hitboxColliders, target);
	}

	/*
	 * ResolveCoreHitboxColliders 결과를 계산해 반환한다.
	 */
	private static Collider2D[] ResolveCoreHitboxColliders(GameObject hitboxObject /* 피격 판정 게임 오브젝트 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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
	private static SkillEffectDefinition[] ResolveOnHitStatusEffects(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SkillEffectDefinition[] effects /* 실행할 효과 목록 */)
	{
		if (effects == null || effects.Length == 0)
		{
			return Array.Empty<SkillEffectDefinition>();
		}
		List<SkillEffectDefinition> list = new List<SkillEffectDefinition>();
		foreach (SkillEffectDefinition skillEffectDefinition in effects)
		{
			if (skillEffectDefinition != null && skillEffectDefinition.EffectTiming == SkillMultiEffectTiming.OnHit && skillEffectDefinition.EffectKind == SkillMultiEffectKind.Status && skillEffectDefinition.TargetSide == SkillMultiEffectTargetSide.Enemy && SkillRequirement.CanRunEffect(context, skillEffectDefinition))
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
	private static void TryApplyOnHitStatusEffects(InGameCombatManager manager /* 전투 진행 관리자 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, SkillEffectDefinition[] effects /* 실행할 효과 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */)
	{
		if (manager == null || target == null || effects == null || effects.Length == 0)
		{
			return;
		}
		foreach (SkillEffectDefinition skillEffectDefinition in effects)
		{
			if (skillEffectDefinition != null && SkillTargeting.MatchesEffectTarget(target, skillEffectDefinition))
			{
				ProjectileStatusHitSpec projectileStatusHitSpec = SkillStatus.ResolveEffectStatusSpec(skillEffectDefinition);
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
	private static void TryApplyCoreOnHitAdditionalDamage(InGameCombatManager manager /* 전투 진행 관리자 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */, float primaryDamage /* 주 대상 피해 */, bool isCoreHit /* 여부 핵심 적중 여부 */)
	{
		if (isCoreHit && !(manager == null) && snapshot != null && snapshot.HasCoreOnHitAdditionalDamage && !(snapshot.CoreOnHitAdditionalDamageMultiplier <= 0f) && source != null && target != null && target.IsAlive && target.Model != null && !(primaryDamage <= 0f) && !(UnityEngine.Random.value > Mathf.Clamp01(snapshot.CoreOnHitAdditionalDamageChance)))
		{
			manager.ApplyDamage(target.Model, primaryDamage * snapshot.CoreOnHitAdditionalDamageMultiplier, snapshot.CoreOnHitAdditionalDamageAttribute, source, criticalAllowed: false, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: true);
		}
	}

	/*
	 * TryApplyHitCountCooldownRefund 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryApplyHitCountCooldownRefund(SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, int hitCount /* 적중한 횟수 */)
	{
		if (sourceRuntime != null && sourceRuntime.Owner != null && sourceRuntime.Owner.Skills != null && snapshot != null && hitCount >= snapshot.HitCountCooldownRefundMinTargets && !string.IsNullOrWhiteSpace(snapshot.HitCountCooldownRefundTargetSkillId) && !(snapshot.HitCountCooldownRefundRatio <= 0f))
		{
			SkillUseState skillRuntimeInstance = sourceRuntime.Owner.SkillState.FindBySkillId(snapshot.HitCountCooldownRefundTargetSkillId);
			skillRuntimeInstance?.ReduceCooldownRemaining(skillRuntimeInstance.EffectiveCooldownDuration * Mathf.Clamp01(snapshot.HitCountCooldownRefundRatio));
		}
	}

	/*
	 * TryExecuteOnHitCountEffects 작업을 시도하고 성공 여부를 반환한다.
	 */
	private static void TryExecuteOnHitCountEffects(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, int hitCount /* 적중한 횟수 */, Vector2 center /* 효과가 적용될 중심 위치 */)
	{
		if (!(manager == null) && roster != null && sourceEntry != null && skill != null && hitCount > 0)
		{
			ExecuteAdditionalEffects(
				new SkillExecutionContext(manager, roster, sourceEntry, sourceRuntime),
				snapshot,
				skill.MultiEffects,
				center,
				true,
				SkillMultiEffectTiming.OnHitCount,
				false,
				hitCount);
		}
	}

	/*
	 * ResolveFollowUpSpec 결과를 계산해 반환한다.
	 */
	private static SingleFollowUpSpec? ResolveFollowUpSpec(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, GameObject prefab /* 생성할 프리팹 */)
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
	private static void RegisterFollowUpTarget(List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */, Vector2 center /* 효과가 적용될 중심 위치 */)
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
	private static void ScheduleConditionalFollowUps(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SingleFollowUpSpec? followUpSpec /* 후속 공격 설정 */, List<SingleFollowUpTarget> followUpTargets /* 후속 공격 대상 목록 */)
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
	private static IEnumerator ExecuteConditionalFollowUpAfterDelay(SkillExecutionContext context /* 스킬 실행에 필요한 정보 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SingleFollowUpTarget followUpTarget /* 후속 공격 대상 */, SingleFollowUpSpec followUpSpec /* 후속 공격 설정 */, float delaySeconds /* 실행 전 대기 시간(초) */)
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


	/*
	 * ResolveTargetDamage 결과를 계산해 반환한다.
	 */
	private static TargetDamageResolution ResolveTargetDamage(UnitCombatState caster /* 스킬을 사용하는 유닛 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, float baseDamage /* 방어 계산 전 기본 피해량 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, float baseCritChanceBonus /* 기본 치명타 확률 추가값 */, bool isCoreHit /* 여부 핵심 적중 여부 */)
	{
		float num = Mathf.Max(0f, baseDamage + ResolveTargetStatusStackAdditionalDamage(caster, skill, snapshot, target, baseDamage));
		float num2 = 1f;
		float critChanceBonus = baseCritChanceBonus;
		if (snapshot != null)
		{
			num2 = SkillExecutionRuleResolver.ResolveConditionalDamageMultiplier(snapshot, target);
			critChanceBonus += SkillExecutionRuleResolver.ResolveConditionalCritChanceBonus(snapshot, target);
		}
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
	private static float ResolveTargetStatusStackAdditionalDamage(UnitCombatState caster /* 스킬을 사용하는 유닛 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, float baseDamage /* 방어 계산 전 기본 피해량 */)
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
		float num2 = DamageCalculator.CalculateRawDamage(caster, skill.TargetStatusStackDamage, snapshot.BaseDamageBonus, snapshot.DamageMultiplier);
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
	private static int ResolvePlannedConsumedStacks(SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, UnitCombatState target /* 효과를 받을 대상 유닛 */)
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
	private static int ConsumePlannedTargetStatusStacks(InGameCombatManager manager /* 전투 진행 관리자 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, SingleSkillDefinition skill /* 실행하거나 검사할 스킬 */, TargetDamageResolution damageResolution /* 피해 계산 결과 */)
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
	private static void TryRedistributeConsumedStatusOnKill(InGameCombatManager manager /* 전투 진행 관리자 */, CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, UnitCombatState source /* 효과를 발생시킨 유닛 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */, CombatUnitEntry defeatedTarget /* 쓰러진 대상 */, InGameResourceChangeResult result /* 처리 결과 */, int consumedStacks /* 소모된 중첩 수 */)
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
	private static List<CombatUnitEntry> ResolveRedistributionTargets(CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */, Vector2 center /* 효과가 적용될 중심 위치 */, float radius /* 효과가 적용될 반지름 */, UnitCombatState excludedModel /* 제외할 상태 모델 */, int maxTargetCount /* 최대 대상 개수 */)
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
	private static int ResolveStatusStacks(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusEffectKind kind /* 처리할 종류 */)
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
	private static bool HasStatus(UnitCombatState target /* 효과를 받을 대상 유닛 */, StatusEffectKind kind /* 처리할 종류 */)
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
	private static void TryApplyStatus(InGameCombatManager manager /* 전투 진행 관리자 */, UnitCombatState target /* 효과를 받을 대상 유닛 */, ProjectileStatusHitSpec statusSpec /* 상태 효과 적용 설정 */, UnitCombatState source /* 효과를 발생시킨 유닛 */)
	{
		StatusCombatRules.ApplyStatus(manager, target, statusSpec, source);
	}
}

}
