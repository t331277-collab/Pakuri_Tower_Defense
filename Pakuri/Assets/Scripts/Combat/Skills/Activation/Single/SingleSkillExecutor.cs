/*
 * 역할: 단발성 공격의 배치 계획을 실행한다.
 * 확정된 중심마다 공격을 시작하고 반복 작업의 완료를 추적한다.
 */

using System.Collections;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

internal sealed class SingleSkillExecutor : MonoBehaviour
{
	private EffectManager effects;
	private int pendingSchedules;

	/// 여러 중심과 후속 작업을 함께 추적할 실행 오브젝트를 만든다.
	internal static bool Execute(
		SkillExecutionContext context,
		SkillExecutionState snapshot)
	{
		var effects = context.CombatManager.Effects;
		if (effects == null)
		{
			return false;
		}

		var instance = effects.CreateEffect(new EffectCreateRequest(
			null,
			null,
			"RuntimeSingleExecution",
			context.CasterEntry.Transform != null
				? context.CasterEntry.Transform.position
				: Vector3.zero,
			Quaternion.identity,
			null,
			null,
			false,
			false,
			true));
		if (instance == null)
		{
			return false;
		}

		var executor = instance.AddComponent<SingleSkillExecutor>();
		return executor.Initialize(context, snapshot);
	}

	/// 준비된 중심에 반복 배치를 실행한다.
	private bool Initialize(
		SkillExecutionContext context,
		SkillExecutionState snapshot)
	{
		effects = context.CombatManager.Effects;
		var runtimeVisual = snapshot.PreparedRuntimeVisual;
		var hasRuntimeVisual = runtimeVisual != null && runtimeVisual.HasVisual();
		var prefab = hasRuntimeVisual ? null : snapshot.PreparedSkillEffectPrefab;
		var castCommitted = false;
		for (var i = 0; i < snapshot.PreparedCenters.Count; i++)
		{
			var center = snapshot.PreparedCenters[i];
			castCommitted |= SingleSkillActor.ExecuteAtCenter(
				context,
				snapshot,
				center,
				runtimeVisual,
				prefab,
				useRuntimeState: true);
			if (snapshot.PreparedUsesResolvedDeployments)
			{
				PublishDeploymentLifecycle(context, snapshot, center);
			}
			ScheduleRepeatedDeployments(context, snapshot, center, runtimeVisual, prefab);
		}
		CompleteIfIdle();
		return castCommitted;
	}

	/// 반복 배치 계획을 시간 순서로 예약한다.
	private void ScheduleRepeatedDeployments(
		SkillExecutionContext context,
		SkillExecutionState snapshot,
		Vector2 center,
		RuntimeSkillVisualSpec runtimeVisual,
		GameObject prefab)
	{
		if (context == null || context.CombatManager == null
			|| !SkillExecutionRules.ResolveRepeat(
				snapshot,
				out var repeatCount,
				out var repeatInterval,
				out var repeatDamageMultiplier))
		{
			return;
		}

		var repeatedSnapshot = snapshot;
		if (!Mathf.Approximately(repeatDamageMultiplier, 1f))
		{
			repeatedSnapshot = snapshot.CopyWithDamageMultiplier(repeatDamageMultiplier);
		}

		if (repeatInterval <= 0f)
		{
			for (var i = 0; i < repeatCount; i++)
			{
				var repeatCenter = ResolveRepeatCenter(context, repeatedSnapshot, center, i);
				SingleSkillActor.ExecuteAtCenter(
					context,
					repeatedSnapshot,
					repeatCenter,
					runtimeVisual,
					prefab,
					useRuntimeState: false);
				if (repeatedSnapshot.PreparedUsesResolvedDeployments)
				{
					PublishDeploymentLifecycle(context, repeatedSnapshot, repeatCenter);
				}
			}
			return;
		}

		pendingSchedules++;
		StartCoroutine(ExecuteRepeatedDeployments(
			context,
			repeatedSnapshot,
			center,
			runtimeVisual,
			prefab,
			repeatCount,
			repeatInterval));
	}

	/// 예약된 반복 배치를 실행한다.
	private IEnumerator ExecuteRepeatedDeployments(
		SkillExecutionContext context,
		SkillExecutionState snapshot,
		Vector2 center,
		RuntimeSkillVisualSpec runtimeVisual,
		GameObject prefab,
		int repeatCount,
		float repeatInterval)
	{
		for (var i = 0; i < repeatCount; i++)
		{
			yield return new WaitForSeconds(Mathf.Max(0f, repeatInterval));
			if (context == null
				|| context.CombatManager == null
				|| context.Roster == null
				|| context.CasterEntry == null
					|| context.Caster == null)
			{
				break;
			}

			var repeatCenter = ResolveRepeatCenter(context, snapshot, center, i);
			SingleSkillActor.ExecuteAtCenter(
				context,
				snapshot,
				repeatCenter,
				runtimeVisual,
				prefab,
				useRuntimeState: false);
			if (snapshot.PreparedUsesResolvedDeployments)
			{
				PublishDeploymentLifecycle(context, snapshot, repeatCenter);
			}
		}

		pendingSchedules = Mathf.Max(0, pendingSchedules - 1);
		if (pendingSchedules == 0)
		{
			Complete();
		}
	}

	/// 밀집 대상 스킬은 반복 시점의 현재 위치를 다시 계산한다.
	private static Vector2 ResolveRepeatCenter(
		SkillExecutionContext context,
		SkillExecutionState snapshot,
		Vector2 fallback,
		int repeatIndex)
	{
		if (snapshot == null
			|| snapshot.PreparedTargeting == null
			|| snapshot.PreparedTargeting.Selection != SkillTargetSelection.Densest)
		{
			return fallback;
		}

		var centers = SkillTargeting.TargetAnchoredCenters(
			context,
			snapshot.PreparedTargeting,
			fallback,
			repeatIndex + 2,
			false,
			SkillDeploymentRepeatMode.RepeatNearest);
		var targetIndex = repeatIndex + 1;
		return centers != null && targetIndex < centers.Count
			? centers[targetIndex]
			: fallback;
	}

	/// 배치 시작 사건을 전달한다.
	private static void PublishDeploymentLifecycle(
		SkillExecutionContext context,
		SkillExecutionState snapshot,
		Vector2 center)
	{
		if (context == null)
		{
			return;
		}

		SkillTrigger.PublishLifecycleEvent(
			SkillTriggerEvent.OnDeploymentCast,
			new SkillExecutionContext(context.Caster, context.SourceSkillId, null, center, 0f, 0, snapshot, context));
	}

	/// 예약된 반복 배치가 없으면 실행 오브젝트를 즉시 정리한다.
	private void CompleteIfIdle()
	{
		if (pendingSchedules == 0)
		{
			Complete();
		}
	}

	/// 실행 오브젝트를 정리한다.
	private void Complete()
	{
		if (effects == null)
		{
			return;
		}

		var manager = effects;
		effects = null;
		manager.RemoveEffect(gameObject);
	}
}
}
