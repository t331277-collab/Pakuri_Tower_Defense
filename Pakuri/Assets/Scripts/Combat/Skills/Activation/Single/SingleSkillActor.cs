/*
 * 역할: 단일 스킬 런타임 Actor 동작.
 * 책임: Single 타기팅·지연·판정·피해·후속 실행·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// SingleSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public partial class SingleSkillActor : MonoBehaviour
    {

        private EffectManager effectManager;
        private Transform target;
        private Vector3 offset;
        private float remainingLifetime;
        private bool followsTarget;
        private bool executionActor;
        private bool executionLaunchFinished;
        private int pendingOperations;

        /// 전달된 런타임 입력값을 사용해 Timed를 초기화한다.
        public void InitializeTimed(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            executionActor = false;
            executionLaunchFinished = false;
            pendingOperations = 0;
            target = null;
            offset = Vector3.zero;
            followsTarget = false;
            remainingLifetime = Mathf.Max(0.01f, durationSeconds);
        }

        /// 전달된 런타임 입력값을 사용해 Animation를 초기화한다.
        public float InitializeAnimation(
            EffectManager manager,
            float durationSeconds)
        {
            var lifetime = Mathf.Max(0.01f, durationSeconds);
            InitializeTimed(manager, lifetime);
            return lifetime;
        }

        /// 전달된 런타임 입력값을 사용해 Following를 초기화한다.
        public void InitializeFollowing(
            EffectManager manager,
            Transform followTarget,
            float durationSeconds,
            Vector3 localOffset)
        {
            effectManager = manager;
            target = followTarget;
            offset = localOffset;
            followsTarget = true;
            remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            transform.position = followTarget.position + offset;
        }

        /// Single 실행 Actor의 작업 추적을 시작한다.
	internal void BeginPreparedExecution(EffectManager manager)
        {
            effectManager = manager;
            executionActor = true;
            executionLaunchFinished = false;
            pendingOperations = 0;
            target = null;
            followsTarget = false;
            remainingLifetime = 0f;
        }

        /// Single 실행 초기화를 끝내고 남은 작업이 없으면 Actor를 종료한다.
	internal void FinishPreparedExecution()
        {
            executionLaunchFinished = true;
            TryCompleteExecution();
        }

        /// Single 지연 작업을 이 Actor의 수명에 연결한다.
        private void StartTrackedCoroutine(IEnumerator operation)
        {
            pendingOperations++;
            StartCoroutine(TrackOperation(operation));
        }

        /// Single 지연 작업 완료를 추적한다.
        private IEnumerator TrackOperation(IEnumerator operation)
        {
            yield return operation;
            pendingOperations = Mathf.Max(0, pendingOperations - 1);
            TryCompleteExecution();
        }

        /// 모든 Single 실행 작업이 끝났으면 EffectManager에 삭제를 요청한다.
        private void TryCompleteExecution()
        {
            if (executionActor && executionLaunchFinished && pendingOperations == 0)
            {
                Complete();
            }
        }

        /// 이 Actor가 소유한 효과의 삭제를 EffectManager에 요청한다.
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

        /// 전달된 instance 값을 사용해 요청값를 연결한다.
        public static SingleSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<SingleSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<SingleSkillActor>();
            }

            return actor;
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            if (executionActor)
            {
                return;
            }

            if (followsTarget)
            {
                if (target == null)
                {
                    Complete();
                    return;
                }

                transform.position = target.position + offset;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Complete();
            }
        }
    }

/// Single 계열 판정과 적용을 소유한다.
public partial class SingleSkillActor
{

	/// SingleExecutionOutcome 처리에 함께 전달되는 값들을 묶는다.
	internal readonly struct SingleExecutionOutcome
	{
		public bool Routed { get; }

		public bool CastCommitted { get; }

		/// SingleExecutionOutcome 인스턴스를 전달된 런타임 입력값으로 초기화한다.
		public SingleExecutionOutcome(bool routed, bool castCommitted)
		{
			Routed = routed;
			CastCommitted = castCommitted;
		}
	}

	/// SingleFollowUpSpec 처리에 함께 전달되는 값들을 묶는다.
	private readonly struct SingleFollowUpSpec
	{
		public StatusEffectKind RequiredStatusKind { get; }

		public int RepeatCount { get; }

		public float IntervalSeconds { get; }

		public float DamageMultiplier { get; }

		public GameObject Prefab { get; }

		/// SingleFollowUpSpec 인스턴스를 전달된 런타임 입력값으로 초기화한다.
		public SingleFollowUpSpec(StatusEffectKind requiredStatusKind, int repeatCount, float intervalSeconds, float damageMultiplier, GameObject prefab)
		{
			RequiredStatusKind = requiredStatusKind;
			RepeatCount = repeatCount;
			IntervalSeconds = intervalSeconds;
			DamageMultiplier = damageMultiplier;
			Prefab = prefab;
		}
	}

	/// SingleFollowUpTarget 처리에 함께 전달되는 값들을 묶는다.
	private readonly struct SingleFollowUpTarget
	{
		public UnitCombatState Model { get; }

		public Vector2 Center { get; }

		/// SingleFollowUpTarget 인스턴스를 전달된 런타임 입력값으로 초기화한다.
		public SingleFollowUpTarget(UnitCombatState model, Vector2 center)
		{
			Model = model;
			Center = center;
		}
	}

	/// TargetDamageResolution 처리에 함께 전달되는 값들을 묶는다.
	private readonly struct TargetDamageResolution
	{
		public float Damage { get; }

		public float FinalDamageMultiplier { get; }

		public float CritChanceBonus { get; }

		public bool IsExecute { get; }

		public int PendingConsumedStacks { get; }

		/// TargetDamageResolution 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

	/// 전달된 런타임 입력값으로 Single 실행을 초기화한다.
	/// 전달된 런타임 입력값을 사용해 ScheduleRepeatedDeployments 작업을 수행한다.
	internal void ScheduleRepeatedDeployments(SkillExecutionContext context, SkillExecutionData snapshot, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab)
	{
		if (context == null || context.CombatManager == null || snapshot == null || snapshot.RepeatCountPerTarget <= 0)
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
				ExecuteAtCenter(context, snapshot2, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
				PublishDeploymentLifecycle(context, snapshot2, center);
			}
			else
			{
				StartTrackedCoroutine(ExecuteRepeatedDeploymentAfterDelay(context, snapshot2, center, runtimeVisual, prefab, num));
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 RepeatedDeploymentAfterDelay를 실행한다.
	private IEnumerator ExecuteRepeatedDeploymentAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null)
		{
			ExecuteAtCenter(context, snapshot, center, runtimeVisual, prefab, allowConditionalFollowUp: false);
			PublishDeploymentLifecycle(context, snapshot, center);
		}
	}

	/// 전달된 런타임 입력값을 사용해 PublishDeploymentLifecycle 작업을 수행한다.
	internal static void PublishDeploymentLifecycle(
		SkillExecutionContext context,
		SkillExecutionData snapshot,
		Vector2 center)
	{
		if (context == null)
		{
			return;
		}

		SkillTrigger.PublishLifecycleEvent(
			SkillTriggerEvent.OnDeploymentCast,
			new SkillActionContext(context.Caster, context.SourceSkillId, null, center, 0f, 0, snapshot, context));
	}

	/// 전달된 런타임 입력값을 사용해 AtCenter를 실행한다.
	internal SingleExecutionOutcome ExecuteAtCenter(SkillExecutionContext context, SkillExecutionData snapshot, Vector2 center, RuntimeSkillVisualSpec runtimeVisual, GameObject prefab, bool allowConditionalFollowUp)
	{
		float radius = snapshot.PreparedRadius;
		bool coverAll = snapshot.PreparedCoverAll;
		float damage = snapshot.PreparedDamage;
		DamageAttribute attribute = snapshot.PreparedDamageAttribute;
		StatusApplicationSpec statusSpec = snapshot.PreparedStatus;
		float critChanceBonus = snapshot?.CritChanceBonus ?? 0f;
		float critDamageBonus = snapshot?.CritDamageBonus ?? 0f;
		int num = snapshot.PreparedHitTargetCount;
		float num2 = snapshot.PreparedDamageDelay;
		SingleFollowUpSpec? followUpSpec = (allowConditionalFollowUp ? FollowUpSpec(snapshot, statusSpec, prefab) : ((SingleFollowUpSpec?)null));
		List<SingleFollowUpTarget> followUpTargets = (followUpSpec.HasValue ? new List<SingleFollowUpTarget>() : null);
		SkillExecutionData skillRuntimeInstance = null;
		if (allowConditionalFollowUp && context.PublishSkillLifecycleEvents)
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
				if (num2 > 0f)
				{
					StartTrackedCoroutine(ApplyPrefabHitboxAfterDelay(context, snapshot, gameObject, num, damage, attribute, statusSpec, skillRuntimeInstance, snapshot.PreparedCriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
				}
				else
				{
					flag2 = ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, gameObject, num, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, skillRuntimeInstance, snapshot.PreparedCriticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
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
					var visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, null, null, false, true, false));
					if (visualInstance != null)
					{
						SingleSkillActor.Attach(visualInstance).InitializeAnimation(effects, visualLifetime);
					}
				}
				StartTrackedCoroutine(ApplyNonPrefabTargetsAfterDelay(context, snapshot, center, radius, coverAll, num, damage, attribute, statusSpec, skillRuntimeInstance, snapshot.PreparedCriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets, num2, allowConditionalFollowUp));
			}
			else
			{
				flag2 = ApplyNonPrefabTargets(context, snapshot, center, radius, coverAll, num, damage, attribute, statusSpec, skillRuntimeInstance, snapshot.PreparedCriticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
				if (flag2 && effects != null)
				{
					var visualInstance = effects.CreateEffect(new EffectCreateRequest(runtimeVisual, prefab, "RuntimeSingleVisual", center, Quaternion.identity, null, null, false, true, false));
					if (visualInstance != null)
					{
						SingleSkillActor.Attach(visualInstance).InitializeAnimation(effects, 1f);
					}
				}
			}
		}
		if (allowConditionalFollowUp && num2 <= 0f)
		{
			ScheduleConditionalFollowUps(context, snapshot, followUpSpec, followUpTargets);
		}
		return new SingleExecutionOutcome(flag2, castCommitted);
	}

	/// 전달된 런타임 입력값을 사용해 NonPrefabTargets를 적용한다.
	private static bool ApplyNonPrefabTargets(SkillExecutionContext context, SkillExecutionData snapshot, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, SkillExecutionData onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
		{
			return false;
		}
		if (snapshot.PreparedUsesHitTargetCount && snapshot.PreparedHitTargetCount != int.MaxValue)
		{
			return ApplyLimitedTargets(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, effectiveHitTargetCount, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, center, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
		}
		return ApplyAreaTargets(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, center, radius, coverAll, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
	}

	/// 전달된 런타임 입력값을 사용해 NonPrefabTargetsAfterDelay를 적용한다.
	private IEnumerator ApplyNonPrefabTargetsAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, Vector2 center, float radius, bool coverAll, int effectiveHitTargetCount, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, SkillExecutionData onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, float delaySeconds, bool allowConditionalFollowUp)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		ApplyNonPrefabTargets(context, snapshot, center, radius, coverAll, effectiveHitTargetCount, damage, attribute, statusSpec, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, followUpSpec, followUpTargets);
		if (allowConditionalFollowUp)
		{
			ScheduleConditionalFollowUps(context, snapshot, followUpSpec, followUpTargets);
		}
	}

	/// 전달된 런타임 입력값을 사용해 PrefabHitboxAfterDelay를 적용한다.
	private IEnumerator ApplyPrefabHitboxAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, GameObject instance, int effectiveHitTargetCount, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, SkillExecutionData onHitRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, float delaySeconds, bool allowConditionalFollowUp)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.CasterEntry != null && context.Roster != null && !(instance == null))
		{
			ApplyPrefabHitbox(context.CombatManager, context.CasterEntry, context.Roster, snapshot.PreparedTargeting, instance, effectiveHitTargetCount, damage, attribute, statusSpec, context.Caster, context.SourceSkillId, onHitRuntime, criticalAllowed, critChanceBonus, critDamageBonus, snapshot, followUpSpec, followUpTargets, context.EventTarget, context.LockToEventTarget);
			if (allowConditionalFollowUp)
			{
				ScheduleConditionalFollowUps(context, snapshot, followUpSpec, followUpTargets);
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 PrefabHitbox를 적용한다.
	private static bool ApplyPrefabHitbox(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SkillTargetingSpec targetingSpec, GameObject hitboxObject, int maxTargets, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillExecutionData sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionData snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, UnitCombatState eventTarget, bool lockToEventTarget)
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
				TargetDamageResolution damageResolution = TargetDamage(snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit);
				InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute, finalDamageMultiplier: damageResolution.FinalDamageMultiplier);
				int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, snapshot, damageResolution);
				SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result2, damageResolution.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
				if (!result2.IsDead)
				{
					TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
				}
				TryApplyCoreOnHitAdditionalDamage(manager, snapshot, source, sourceSkillId, unitEntry, damageResolution.Damage, isCoreHit);
				SkillExecutionRuleResolver.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
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

	/// 전달된 런타임 입력값을 사용해 LimitedTargets를 적용한다.
	private static bool ApplyLimitedTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SkillTargetingSpec targetingSpec, int maxTargets, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillExecutionData sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionData snapshot, Vector2 center, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, UnitCombatState eventTarget, bool lockToEventTarget)
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
			TargetDamageResolution damageResolution = TargetDamage(snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
			InGameResourceChangeResult result2 = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute, finalDamageMultiplier: damageResolution.FinalDamageMultiplier);
			int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, snapshot, damageResolution);
			SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result2, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result2, consumedStacks);
			if (!result2.IsDead)
			{
				TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
			}
			SkillExecutionRuleResolver.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
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

	/// 전달된 런타임 입력값을 사용해 AreaTargets를 적용한다.
	private static bool ApplyAreaTargets(InGameCombatManager manager, CombatUnitEntry sourceEntry, UnitSpawnManager unitRoster, SkillTargetingSpec targetingSpec, Vector2 center, float radius, bool coverAll, float damage, DamageAttribute attribute, StatusApplicationSpec statusSpec, UnitCombatState source, string sourceSkillId, SkillExecutionData sourceRuntime, bool criticalAllowed, float critChanceBonus, float critDamageBonus, SkillExecutionData snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets, UnitCombatState eventTarget, bool lockToEventTarget)
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
			TargetDamageResolution damageResolution = TargetDamage(snapshot, damage, unitEntry.Model, critChanceBonus, isCoreHit: false);
			InGameResourceChangeResult result = manager.ApplyDamage(unitEntry.Model, damageResolution.Damage, attribute, source, criticalAllowed, damageResolution.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution.IsExecute, finalDamageMultiplier: damageResolution.FinalDamageMultiplier);
			int consumedStacks = ConsumePendingTargetStatusStacks(manager, unitEntry.Model, snapshot, damageResolution);
			SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result, damageResolution.IsExecute);
			TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry, result, consumedStacks);
			if (!result.IsDead)
			{
				TryApplyStatus(manager, unitEntry.Model, statusSpec, source);
			}
			SkillExecutionRuleResolver.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry, hitPosition, damageResolution.Damage);
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
				RegisterFollowUpTarget(followUpTargets, followUpSpec, unitEntry2, center);
				Vector2 hitPosition2 = ((unitEntry2.Transform != null) ? ((Vector2)unitEntry2.Transform.position) : center);
				TargetDamageResolution damageResolution2 = TargetDamage(snapshot, damage, unitEntry2.Model, critChanceBonus, isCoreHit: false);
				InGameResourceChangeResult result3 = manager.ApplyDamage(unitEntry2.Model, damageResolution2.Damage, attribute, source, criticalAllowed, damageResolution2.CritChanceBonus, critDamageBonus, sourceSkillId, suppressOutgoingDamageTriggers: false, damageResolution2.IsExecute, finalDamageMultiplier: damageResolution2.FinalDamageMultiplier);
				int consumedStacks2 = ConsumePendingTargetStatusStacks(manager, unitEntry2.Model, snapshot, damageResolution2);
				SkillExecution.HandleSingleKillRecovery(sourceRuntime, snapshot, result3, damageResolution2.IsExecute);
				TryRedistributeConsumedStatusOnKill(manager, sourceEntry, unitRoster, source, snapshot, unitEntry2, result3, consumedStacks2);
				if (!result3.IsDead)
				{
					TryApplyStatus(manager, unitEntry2.Model, statusSpec, source);
				}
				SkillExecutionRuleResolver.ApplyHitEnhancements(manager, unitRoster, sourceRuntime, snapshot, sourceEntry, source, sourceSkillId, unitEntry2, hitPosition2, damageResolution2.Damage);
				result2 = true;
				num++;
			}
		}
		TryApplyHitCountCooldownRefund(sourceRuntime, snapshot, num);
		TryExecuteOnHitCountEffects(manager, unitRoster, sourceEntry, sourceRuntime, snapshot, num, center);
		return result2;
	}

	/// 전달된 런타임 입력값을 사용해 CoreHitboxColliders 결과값을 생성해 반환한다.
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

	/// 전달된 런타임 입력값을 사용해 ApplyCoreOnHitAdditionalDamage 작업을 시도하고 성공 여부를 반환한다.
	private static void TryApplyCoreOnHitAdditionalDamage(InGameCombatManager manager, SkillExecutionData snapshot, UnitCombatState source, string sourceSkillId, CombatUnitEntry target, float primaryDamage, bool isCoreHit)
	{
		if (isCoreHit && !(manager == null) && snapshot != null && snapshot.HasCoreOnHitAdditionalDamage && !(snapshot.CoreOnHitAdditionalDamageMultiplier <= 0f) && source != null && target != null && target.IsAlive && target.Model != null && !(primaryDamage <= 0f) && !(UnityEngine.Random.value > Mathf.Clamp01(snapshot.CoreOnHitAdditionalDamageChance)))
		{
			manager.ApplyDamage(target.Model, primaryDamage * snapshot.CoreOnHitAdditionalDamageMultiplier, snapshot.CoreOnHitAdditionalDamageAttribute, source, criticalAllowed: false, 0f, 0f, sourceSkillId, suppressOutgoingDamageTriggers: true);
		}
	}

	/// 전달된 런타임 입력값을 사용해 ApplyHitCountCooldownRefund 작업을 시도하고 성공 여부를 반환한다.
	private static void TryApplyHitCountCooldownRefund(SkillExecutionData sourceRuntime, SkillExecutionData snapshot, int hitCount)
	{
		if (sourceRuntime != null && sourceRuntime.Owner != null && sourceRuntime.Owner.Skills != null && snapshot != null && hitCount >= snapshot.HitCountCooldownRefundMinTargets && !string.IsNullOrWhiteSpace(snapshot.HitCountCooldownRefundTargetSkillId) && !(snapshot.HitCountCooldownRefundRatio <= 0f))
		{
			SkillExecutionData skillRuntimeInstance = sourceRuntime.Owner.SkillState.FindBySkillId(snapshot.HitCountCooldownRefundTargetSkillId);
			skillRuntimeInstance?.ReduceCooldownRemaining(skillRuntimeInstance.EffectiveCooldownDuration * Mathf.Clamp01(snapshot.HitCountCooldownRefundRatio));
		}
	}

	/// 전달된 런타임 입력값을 사용해 ExecuteOnHitCountEffects 작업을 시도하고 성공 여부를 반환한다.
	private static void TryExecuteOnHitCountEffects(InGameCombatManager manager, UnitSpawnManager roster, CombatUnitEntry sourceEntry, SkillExecutionData sourceRuntime, SkillExecutionData snapshot, int hitCount, Vector2 center)
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
				new SkillActionContext(
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

	/// 전달된 런타임 입력값을 사용해 FollowUpSpec 결과값을 생성해 반환한다.
	private static SingleFollowUpSpec? FollowUpSpec(SkillExecutionData snapshot, StatusApplicationSpec statusSpec, GameObject prefab)
	{
		if (snapshot == null || !snapshot.HasBranchCount || snapshot.BranchCount <= 0 || !snapshot.HasBranchDamageMultiplier || snapshot.BranchDamageMultiplier <= 0f || !snapshot.HasBranchSearchRadius || snapshot.BranchSearchRadius <= 0f)
		{
			return null;
		}
		if (statusSpec == null || statusSpec.Status == null || statusSpec.Status.Kind == StatusEffectKind.None)
		{
			return null;
		}
		return new SingleFollowUpSpec(statusSpec.Status.Kind, snapshot.BranchCount, snapshot.BranchSearchRadius, snapshot.BranchDamageMultiplier, prefab);
	}

	/// 전달된 런타임 입력값을 사용해 FollowUpTarget를 소유 런타임 Registry에 등록한다.
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

	/// 전달된 런타임 입력값을 사용해 ScheduleConditionalFollowUps 작업을 수행한다.
	private void ScheduleConditionalFollowUps(SkillExecutionContext context, SkillExecutionData snapshot, SingleFollowUpSpec? followUpSpec, List<SingleFollowUpTarget> followUpTargets)
	{
		if (context == null || context.CombatManager == null || context.Roster == null || context.CasterEntry == null || context.Caster == null || !followUpSpec.HasValue || followUpTargets == null || followUpTargets.Count == 0)
		{
			return;
		}
		SingleFollowUpSpec value = followUpSpec.Value;
		for (int i = 0; i < followUpTargets.Count; i++)
		{
			SingleFollowUpTarget followUpTarget = followUpTargets[i];
			for (int j = 1; j <= value.RepeatCount; j++)
			{
				StartTrackedCoroutine(ExecuteConditionalFollowUpAfterDelay(context, snapshot, followUpTarget, value, value.IntervalSeconds * (float)j));
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 ConditionalFollowUpAfterDelay를 실행한다.
	private IEnumerator ExecuteConditionalFollowUpAfterDelay(SkillExecutionContext context, SkillExecutionData snapshot, SingleFollowUpTarget followUpTarget, SingleFollowUpSpec followUpSpec, float delaySeconds)
	{
		yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
		if (context != null && !(context.CombatManager == null) && context.Roster != null && context.CasterEntry != null && context.Caster != null)
		{
			CombatUnitEntry unitEntry = ((followUpTarget.Model != null) ? context.Roster.Find(followUpTarget.Model) : null);
			Vector2 center = ((unitEntry != null && unitEntry.Transform != null) ? ((Vector2)unitEntry.Transform.position) : followUpTarget.Center);
			SkillExecutionData snapshot2 = null;
			if (snapshot != null)
			{
				snapshot2 = snapshot.CopyWithDamageMultiplier(followUpSpec.DamageMultiplier);
			}
			ExecuteAtCenter(context, snapshot2, center, null, followUpSpec.Prefab, allowConditionalFollowUp: false);
		}
	}

	/// 전달된 런타임 입력값을 사용해 TargetDamage 결과값을 생성해 반환한다.
	private static TargetDamageResolution TargetDamage(SkillExecutionData snapshot, float baseDamage, UnitCombatState target, float baseCritChanceBonus, bool isCoreHit)
	{
		float num = Mathf.Max(0f, baseDamage + TargetStatusStackAdditionalDamage(snapshot, target, baseDamage));
		float num2 = 1f;
		float critChanceBonus = baseCritChanceBonus;
		if (snapshot != null)
		{
				num2 = Mathf.Max(0f, snapshot.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(snapshot, target);
			critChanceBonus += SkillExecutionRuleResolver.ConditionalCritChanceBonus(snapshot, target);
		}
		bool flag = false;
		int pendingConsumedStacks = PendingConsumedStacks(snapshot, target);
		if (isCoreHit && snapshot != null && snapshot.HasCoreDamageMultiplier)
		{
			num2 *= snapshot.CoreDamageMultiplier;
		}
		SkillExecutionRuleResolver.ApplySingleDamageModifiers(
			snapshot,
			target,
			ref num2,
			ref critChanceBonus,
			out flag);
		return new TargetDamageResolution(Mathf.Max(0f, num), Mathf.Max(0f, num2), critChanceBonus, flag, pendingConsumedStacks);
	}

	/// 전달된 런타임 입력값을 사용해 TargetStatusStackAdditionalDamage 결과값을 생성해 반환한다.
	private static float TargetStatusStackAdditionalDamage(SkillExecutionData snapshot, UnitCombatState target, float baseDamage)
	{
		if (snapshot == null || target == null || snapshot.PreparedTargetStatusStackStatusKind == StatusEffectKind.None)
		{
			return 0f;
		}
		int num = StatusStacks(target, snapshot.PreparedTargetStatusStackStatusKind);
		if (num <= 0)
		{
			return 0f;
		}
		if (snapshot.PreparedTargetStatusStackMaxStacks > 0)
		{
			num = Mathf.Min(num, snapshot.PreparedTargetStatusStackMaxStacks);
		}
		float num2 = snapshot.PreparedTargetStatusStackDamage;
		float b = snapshot.TargetStatusStackDamageMultiplier;
		float num3 = snapshot.PreparedTargetStatusStackDamageRateBonus;
		float num4 = num2 * Mathf.Max(0f, b) + Mathf.Max(0f, baseDamage) * num3;
		return Mathf.Max(0f, (float)num * num4);
	}

	/// 전달된 런타임 입력값을 사용해 PendingConsumedStacks 결과값을 생성해 반환한다.
	private static int PendingConsumedStacks(SkillExecutionData snapshot, UnitCombatState target)
	{
		if (snapshot == null || target == null || snapshot.PreparedConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		int num = StatusStacks(target, snapshot.PreparedConsumeTargetStatusKind);
		if (num <= 0)
		{
			return 0;
		}
		if (snapshot.PreparedConsumeTargetStatusStacks > 0)
		{
			return Mathf.Clamp(snapshot.PreparedConsumeTargetStatusStacks, 0, num);
		}
		float num2 = snapshot.PreparedConsumeTargetStatusRatio;
		if (num2 <= 0f)
		{
			return 0;
		}
		return Mathf.Clamp(Mathf.FloorToInt((float)num * Mathf.Clamp01(num2)), 0, num);
	}

	/// 전달된 런타임 입력값을 사용해 PendingTargetStatusStacks를 현재 런타임 상태에서 소비한다.
	private static int ConsumePendingTargetStatusStacks(InGameCombatManager manager, UnitCombatState target, SkillExecutionData snapshot, TargetDamageResolution damageResolution)
	{
		if (manager == null || target == null || snapshot == null || damageResolution.PendingConsumedStacks <= 0 || snapshot.PreparedConsumeTargetStatusKind == StatusEffectKind.None)
		{
			return 0;
		}
		return manager.ConsumeStatusStacks(target, snapshot.PreparedConsumeTargetStatusKind, damageResolution.PendingConsumedStacks);
	}

	/// 전달된 런타임 입력값을 사용해 RedistributeConsumedStatusOnKill 작업을 시도하고 성공 여부를 반환한다.
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
				StatusApplicationSpec projectileStatusHitSpec = SkillExecutionRuleResolver.CreateDirectStatusSpec(snapshot.RedistributeConsumedStatusKind, num4, snapshot);
				if (projectileStatusHitSpec != null)
				{
					StatusCombatRules.ApplyStatus(manager, unitEntry.Model, projectileStatusHitSpec, source);
				}
			}
		}
	}

	/// 전달된 런타임 입력값을 사용해 RedistributionTargets 결과값을 생성해 반환한다.
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

	/// 전달된 런타임 입력값을 사용해 StatusStacks 결과값을 생성해 반환한다.
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

	/// 전달된 런타임 입력값을 사용해 소유한 런타임 상태에 Status가 있는지 반환한다.
	private static bool HasStatus(UnitCombatState target, StatusEffectKind kind)
	{
		if (target != null && target.Statuses != null && kind != StatusEffectKind.None)
		{
			return target.Statuses.Has(kind);
		}
		return false;
	}

	/// 전달된 런타임 입력값을 사용해 ApplyStatus 작업을 시도하고 성공 여부를 반환한다.
	private static void TryApplyStatus(InGameCombatManager manager, UnitCombatState target, StatusApplicationSpec statusSpec, UnitCombatState source)
	{
		StatusCombatRules.ApplyStatus(manager, target, statusSpec, source);
	}
}
}
