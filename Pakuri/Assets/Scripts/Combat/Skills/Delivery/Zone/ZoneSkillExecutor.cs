/*
 * 역할: Zone 스킬 전달 조정.
 * 책임: 실행용 Zone Actor를 생성하고 해석된 실행 데이터를 전달한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// ZoneSkillActor를 생성하고 실행을 시작한다.
    internal static class ZoneSkillExecutor
    {
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ZoneSkillDefinition skill,
            SkillTriggerCommand command = null,
            Vector2? recastCenter = null)
        {
            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return false;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                null,
                null,
                "RuntimeZoneExecution",
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

            var actor = instance.GetComponent<ZoneSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ZoneSkillActor>();
            }

            return actor.InitializeExecution(context, snapshot, skill, command, recastCenter);
        }
    }
}
