/*
 * 역할: 단일 대상 스킬 전달 조정.
 * 책임: 실행용 Single Actor를 생성하고 해석된 실행 데이터를 전달한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

/// SingleSkillActor를 생성하고 실행을 시작한다.
internal static class SingleSkillExecutor
{
	internal static bool Execute(
		SkillExecutionContext context,
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

		return SingleSkillActor.Attach(instance).InitializeExecution(context, snapshot);
	}
}
}
