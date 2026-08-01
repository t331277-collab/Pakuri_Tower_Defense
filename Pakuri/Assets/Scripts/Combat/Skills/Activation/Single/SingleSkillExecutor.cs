/*
 * 역할: 단발성 공격의 배치 계획을 실행한다.
 * 책임: 확정된 중심마다 공격을 시작하고 반복 작업의 완료를 추적한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

internal static class SingleSkillExecutor
{
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

		var actor = SingleSkillActor.Attach(instance);
		actor.BeginPreparedExecution(effects);
		var runtimeVisual = snapshot.PreparedRuntimeVisual;
		var hasRuntimeVisual = runtimeVisual != null && runtimeVisual.HasVisual();
		var prefab = hasRuntimeVisual ? null : snapshot.PreparedSkillEffectPrefab;
		var routed = false;
		var castCommitted = false;
		for (var i = 0; i < snapshot.PreparedCenters.Count; i++)
		{
			var center = snapshot.PreparedCenters[i];
			var outcome = actor.ExecuteAtCenter(
				context,
				snapshot,
				center,
				runtimeVisual,
				prefab,
				allowConditionalFollowUp: true);
			routed |= outcome.Routed;
			castCommitted |= outcome.CastCommitted;
			if (snapshot.PreparedUsesResolvedDeployments)
			{
				SingleSkillActor.PublishDeploymentLifecycle(context, snapshot, center);
				actor.ScheduleRepeatedDeployments(context, snapshot, center, runtimeVisual, prefab);
			}
		}
		actor.FinishPreparedExecution();
		return routed || castCommitted;
	}
}
}
