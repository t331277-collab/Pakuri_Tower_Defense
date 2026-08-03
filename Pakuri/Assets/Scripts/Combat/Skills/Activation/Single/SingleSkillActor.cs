/*
 * 역할: 단일 공격 배치의 대상 판정과 전투 결과를 적용한다.
 * 책임: 충돌, 대상 제한, 피해, 상태, 후속 적중 효과와 표현 수명을 처리한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 단발성 효과가 맡은 작업과 표현이 모두 끝날 때까지 수명을 유지한다.
    public partial class SingleSkillActor : MonoBehaviour
    {

        private EffectManager effectManager;
        private float remainingLifetime;

        /// 단일 애니메이션 클립의 길이를 효과 수명으로 사용한다.
        public float InitializeAnimation(EffectManager manager)
        {
            var animator = GetComponent<Animator>();
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                throw new InvalidOperationException(
                    $"SingleAttack visual '{name}' must have an Animator with one animation clip.");
            }

            var clips = animator.runtimeAnimatorController.animationClips;
            if (clips == null || clips.Length != 1 || clips[0] == null)
            {
                throw new InvalidOperationException(
                    $"SingleAttack visual '{name}' must have exactly one animation clip.");
            }

            var lifetime = Mathf.Max(0.01f, clips[0].length);
            effectManager = manager;
            remainingLifetime = lifetime;
            return lifetime;
        }

        /// 효과 객체의 수명을 끝낸다.
        private void Complete()
        {
            if (effectManager == null)
            {
                return;
            }

            var manager = effectManager;
            effectManager = null;
            manager.RemoveEffect(gameObject);
        }

        /// 효과 객체에 실행 컴포넌트를 연결한다.
        public static SingleSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<SingleSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<SingleSkillActor>();
            }

            return actor;
        }

        /// 프레임 경과에 따라 효과 수명을 갱신한다.
        private void Update()
        {
            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Complete();
            }
        }
    }

/// 배치된 공격의 대상 판정과 전투 결과를 적용한다.
public partial class SingleSkillActor
{

	/// 대상 상태를 반영한 피해 입력과 처형 결과를 고정한다.
	private readonly struct TargetDamageResolution
	{
		public float Damage { get; }

		public float FinalDamageMultiplier { get; }

		public float CritChanceBonus { get; }

		public bool IsExecute { get; }

		public int PendingConsumedStacks { get; }

		/// 피해 적용과 상태 소비가 같은 판정 결과를 사용하게 한다.
		public TargetDamageResolution(float damage, float finalDamageMultiplier, float critChanceBonus, bool isExecute, int pendingConsumedStacks)
		{
			Damage = damage;
			FinalDamageMultiplier = finalDamageMultiplier;
			CritChanceBonus = critChanceBonus;
			IsExecute = isExecute;
			PendingConsumedStacks = pendingConsumedStacks;
		}
	}

	/// 준비된 단일 공격을 중심 위치에서 실행한다.
	internal static bool ExecuteAtCenter(SkillExecutionContext context, SkillExecutionState snapshot, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, bool useRuntimeState)
	{
		float radius = snapshot.PreparedRadius;
		bool coverAll = snapshot.PreparedCoverAll;
		float damage = snapshot.PreparedDamage;
		DamageAttribute attribute = snapshot.PreparedDamageAttribute;
		StatusApplicationSpec statusSpec = snapshot.PreparedStatus;
		float critChanceBonus = snapshot?.CritChanceBonus ?? 0f;
		float critDamageBonus = snapshot?.CritDamageBonus ?? 0f;
		int num = snapshot.PreparedHitTargetCount;
		SkillExecutionState skillRuntimeInstance = null;
		if (useRuntimeState && context.PublishSkillLifecycleEvents)
		{
			skillRuntimeInstance = context.Runtime;
		}
		bool flag = false;
		bool flag2 = false;
		bool castCommitted = false;
		EffectManager effects = context.CombatManager.Effects;
		bool flag3 = effects != null && runtimeVisual != null && runtimeVisual.HasVisual();
		if (snapshot.PreparedUsePrefabHitbox && (flag3 || prefab != null) && effects != null)
		{
			if (snapshot.PreparedPrefabHitboxAtOrigin)
			{
				center = snapshot.PreparedOrigin;
			}
			GameObject gameObject = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleHitbox", center, Quaternion.identity, null, null, false, true, false));
			if (gameObject != null)
			{
				flag = true;
				castCommitted = true;
				if (!flag3)
				{
					EffectVisualBuilder.ConfigureAreaEffect(
						gameObject,
						snapshot.PreparedBaseRadius,
						snapshot.RadiusMultiplier,
						snapshot.RadiusBonus);
				}
				flag2 = ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, gameObject, num, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, skillRuntimeInstance, snapshot.PreparedCriticalAllowed, critChanceBonus, critDamageBonus, snapshot, context.EventTarget, context.LockToEventTarget);
				SingleSkillActor.Attach(gameObject).InitializeAnimation(effects);
			}
		}
		if (!flag)
		{
			castCommitted = true;
			flag2 = ApplyNonPrefabTargets(context, snapshot, center, radius, coverAll, num, damage, attribute, statusSpec, skillRuntimeInstance, snapshot.PreparedCriticalAllowed, critChanceBonus, critDamageBonus);
			if (flag2 && effects != null)
			{
				var visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, null, null, false, true, false));
				if (visualInstance != null)
				{
					SingleSkillActor.Attach(visualInstance).InitializeAnimation(effects);
				}
			}
		}
		return castCommitted;
	}

	/// 일반 단일 공격의 대상 경로를 선택한다.
	private static bool ApplyNonPrefabTargets(SkillExecutionContext context, SkillExecutionState snapshot, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, SkillExecutionState onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus)
	{
		if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
		{
			return false;
		}
		if (snapshot.PreparedUsesHitTargetCount && snapshot.PreparedHitTargetCount != int.MaxValue)
		{
			return ApplyLimitedTargets(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, effectiveHitTargetCount, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, center, context.EventTarget, context.LockToEventTarget);
		}
		return ApplyAreaTargets(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, center, radius, coverAll, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, context.EventTarget, context.LockToEventTarget);
	}

	/// 충돌 영역 결과를 공통 피해 경로에 연결한다.
	private static bool ApplyPrefabHitbox(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SkillTargetingSpec targetingSpec, GameObject hitboxObject, int maxTargets, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillExecutionState sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionState snapshot, UnitCombatState eventTarget, bool lockToEventTarget)
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
				Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : Vector2.zero);
				bool isCoreHit = coreCollisionTargets.Contains(unitEntry);
				TargetDamageResolution damageResolution = TargetDamage(snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit);
                InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, false, damageResolution.IsExecute, null, damageResolution.FinalDamageMultiplier, isTrigger: snapshot.IsTrigger);
				int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, snapshot, damageResolution);
				SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result2, damageResolution.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
				if (!result2.IsDead)
				{
					StatusCombatRules.ApplyStatus(manager, unitEntry.Model, statusSpec, source);
				}
				TryApplyCoreOnHitAdditionalDamage(manager, snapshot, source, sourceSkillId, unitEntry, damageResolution.Damage, isCoreHit);
				ZoneSkillActor.PublishHitOutcome(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
				result = true;
				num++;
				if (num >= maxTargets)
				{
					break;
				}
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, snapshot, num, hitboxObject.transform.position);
		return result;
	}

	/// 제한된 대상 목록에 피해를 적용한다.
	private static bool ApplyLimitedTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SkillTargetingSpec targetingSpec, int maxTargets, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillExecutionState sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionState snapshot, Vector2 center, UnitCombatState eventTarget, bool lockToEventTarget)
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
			Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : center);
			TargetDamageResolution damageResolution = TargetDamage(snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
            InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, false, damageResolution.IsExecute, null, damageResolution.FinalDamageMultiplier, isTrigger: snapshot.IsTrigger);
			int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, snapshot, damageResolution);
			SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result2, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
			if (!result2.IsDead)
			{
				StatusCombatRules.ApplyStatus(manager, unitEntry.Model, statusSpec, source);
			}
				ZoneSkillActor.PublishHitOutcome(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
			result = true;
			num++;
			if (num >= maxTargets)
			{
				break;
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, snapshot, num, center);
		return result;
	}

	/// 범위 안 대상에 피해를 적용한다.
	private static bool ApplyAreaTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SkillTargetingSpec targetingSpec, Vector2 center, float radius, bool coverAll, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillExecutionState sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionState snapshot, UnitCombatState eventTarget, bool lockToEventTarget)
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
			Vector2 hitPosition = ((unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : center);
			TargetDamageResolution damageResolution = TargetDamage(snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
            InGameResourceChangeResult result = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, false, damageResolution.IsExecute, null, damageResolution.FinalDamageMultiplier, isTrigger: snapshot.IsTrigger);
			int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, snapshot, damageResolution);
			SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result, consumedStacks);
			if (!result.IsDead)
			{
				StatusCombatRules.ApplyStatus(manager, unitEntry.Model, statusSpec, source);
			}
				ZoneSkillActor.PublishHitOutcome(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
			TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, 1);
			TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, snapshot, 1, center);
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
				Vector2 hitPosition2 = ((unitEntry2.Transform != null) ? ((Vector2)unitEntry2.Transform.position) : center);
				TargetDamageResolution damageResolution2 = TargetDamage(snapshot, damage, unitEntry2.Model, critChanceBonus, isCoreHit: false);
                InGameResourceChangeResult result3 = manager.ApplyDamage(unitEntry2.Model, damageResolution2.Damage, attribute, source, criticalAllowed, damageResolution2.CritChanceBonus, critDamageBonus, sourceSkillId, false, damageResolution2.IsExecute, null, damageResolution2.FinalDamageMultiplier, isTrigger: snapshot.IsTrigger);
				int consumedStacks2 = ConsumePendingTargetStatusStacks(manager, unitEntry2.Model, snapshot, damageResolution2);
				SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result3, damageResolution2.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry2, result3, consumedStacks2);
				if (!result3.IsDead)
				{
					StatusCombatRules.ApplyStatus(manager, unitEntry2.Model, statusSpec, source);
				}
				ZoneSkillActor.PublishHitOutcome(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry2, hitPosition2, damageResolution2.Damage);
				result2 = true;
				num++;
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, snapshot, num, center);
		return result2;
	}

	/// 핵심 충돌 영역을 찾는다.
	private static Collider2D[] CoreHitboxColliders(GameObject hitboxObject, SkillExecutionState snapshot)
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

	/// 핵심 적중의 추가 피해를 적용한다.
	private static void TryApplyCoreOnHitAdditionalDamage(InGameCombatManager manager, SkillExecutionState snapshot, UnitCombatState source, string sourceSkillId, CombatUnitEntry target, float primaryDamage, bool isCoreHit)
	{
		if (manager == null || source == null || target == null || !target.IsAlive || target.Model == null || primaryDamage <= 0f
			|| !SkillExecutionRules.ResolveCoreAdditionalDamage(
				snapshot,
				isCoreHit,
				out var chance,
				out var multiplier,
				out var attribute)
			|| UnityEngine.Random.value > chance)
		{
			return;
		}

		manager.ApplyDamage(
			target.Model,
			primaryDamage * multiplier,
			attribute,
			source,
			criticalAllowed: false,
			0f,
			0f,
			sourceSkillId,
			true,
			false,
			null,
			1f,
			isTrigger: snapshot.IsTrigger);
	}

	/// 적중 수 보정을 실행에 반영한다.
	private static void TryApplyHitCountCooldownRefund(SkillExecutionState sourceRuntime, SkillExecutionState snapshot, int hitCount)
	{
		SkillExecution.ApplyHitCountCooldownRefund(sourceRuntime, snapshot, hitCount);
	}

	/// 적중 수 사건을 후속 효과에 전달한다.
	private static void TryExecuteOnHitCountEffects(InGameCombatManager manager, UnitSpawnManager roster, CombatUnitEntry sourceEntry, SkillExecutionState sourceRuntime, SkillExecutionState snapshot, int hitCount, Vector2 center)
	{
		if (!(manager == null) && roster != null && sourceEntry != null && hitCount > 0)
		{
			var executionContext = new SkillExecutionContext(
				manager,
				roster,
				sourceEntry,
				sourceRuntime,
				publishSkillLifecycleEvents: sourceRuntime != null);
			SkillTrigger.PublishLifecycleEvent(
				SkillTriggerEvent.OnHitCount,
				new SkillExecutionContext(
					sourceEntry.Model,
					sourceRuntime != null && !string.IsNullOrWhiteSpace(sourceRuntime.SkillId)
						? sourceRuntime.SkillId
						: !string.IsNullOrWhiteSpace(snapshot.PreparedSkillId)
							? snapshot.PreparedSkillId
							: snapshot.SkillId,
					null,
					center,
					0f,
					hitCount,
					snapshot,
					executionContext));
		}
	}

	/// 대상별 최종 피해 입력을 계산한다.
	private static TargetDamageResolution TargetDamage(SkillExecutionState snapshot, float baseDamage, UnitCombatState target, float baseCritChanceBonus, bool isCoreHit)
	{
		float num = Mathf.Max(0f, baseDamage + SkillExecutionRules.ResolveTargetStatusStackDamage(snapshot, target, baseDamage));
		float num2 = SkillExecutionRules.ResolveHitDamageMultiplier(snapshot, target);
		float critChanceBonus = baseCritChanceBonus;
		if (snapshot != null)
		{
			critChanceBonus += SkillExecutionRules.ResolveHitCritChanceBonus(snapshot, target);
		}
		bool flag = false;
		int pendingConsumedStacks = SkillExecutionRules.ResolveConsumedStatusStacks(snapshot, target);
		if (isCoreHit && snapshot != null && snapshot.HasCoreDamageMultiplier)
		{
			num2 *= snapshot.CoreDamageMultiplier;
		}
		SkillExecutionRules.ApplySingleDamageModifiers(
			snapshot,
			target,
			ref num2,
			ref critChanceBonus,
			out flag);
		return new TargetDamageResolution(Mathf.Max(0f, num), Mathf.Max(0f, num2), critChanceBonus, flag, pendingConsumedStacks);
	}

	/// 적중 직후 대상 상태를 소비한다.
	private static int ConsumePendingTargetStatusStacks(InGameCombatManager manager, UnitCombatState target, SkillExecutionState snapshot, TargetDamageResolution damageResolution)
	{
		if (manager == null || target == null || snapshot == null || damageResolution.PendingConsumedStacks <= 0 || snapshot.PreparedConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		return manager.ConsumeStatusStacks(target, snapshot.PreparedConsumeTargetStatusKind, damageResolution.PendingConsumedStacks);
	}

	/// 처치 뒤 소비된 상태를 주변 대상에 분배한다.
	private static void TryRedistributeConsumedStatusOnKill(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager roster, UnitCombatState source, SkillExecutionState snapshot, CombatUnitEntry defeatedTarget, InGameResourceChangeResult result, int consumedStacks)
	{
		if (manager == null || sourceEntry == null || roster == null || source == null || defeatedTarget == null || defeatedTarget.Transform == null || !result.IsDead
			|| !SkillExecutionRules.ResolveStatusRedistribution(
				snapshot,
				consumedStacks,
				out var redistributedStacks,
				out var statusKind,
				out var searchRadius,
				out var maxTargetCount))
		{
			return;
		}
		List<CombatUnitEntry> list = RedistributionTargets(
			sourceEntry,
			roster,
			defeatedTarget.Transform.position,
			searchRadius,
			defeatedTarget.Model,
			maxTargetCount);
		if (list.Count <= 0)
		{
			return;
		}
		int num2 = redistributedStacks / list.Count;
		int num3 = redistributedStacks % list.Count;
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
				StatusApplicationSpec projectileStatusHitSpec = SkillExecutionRules.CreateDirectStatusSpec(statusKind, num4, snapshot);
				if (projectileStatusHitSpec != null)
				{
					StatusCombatRules.ApplyStatus(manager, unitEntry.Model, projectileStatusHitSpec, source);
				}
			}
		}
	}

	/// 상태 분배 대상 후보를 거리 순으로 고른다.
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

}
}
