/*
 * 역할: 투사체 스킬 전달 조정.
 * 책임: 실행용 Projectile Actor를 생성하고 해석된 실행 데이터를 전달한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// ProjectileSkillActor를 생성하고 실행을 시작한다.
    internal static class ProjectileSkillExecutor
    {
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill)
        {
            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return false;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                null,
                null,
                "RuntimeProjectileExecution",
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

            var actor = instance.GetComponent<ProjectileSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ProjectileSkillActor>();
            }

            return actor.InitializeExecution(context, snapshot, skill);
        }
    }
}
