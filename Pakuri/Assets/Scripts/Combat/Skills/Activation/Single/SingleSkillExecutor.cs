/*
 * 역할: 단일 대상 스킬 전달 조정.
 * 책임: 확정된 Single 배치 계획을 순회하고 실제 판정을 Actor에 전달한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

/// 확정된 Single 중심 계획을 실행하고 실제 판정은 SingleSkillActor에 맡긴다.
internal static class SingleSkillExecutor
{
	internal static bool Execute(
		SkillActionContext context,
		SkillExecutionData snapshot)
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
