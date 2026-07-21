using Pakuri.Combat;
using UnityEngine;

/*
 * 적을 넥서스로 이동시키고 접촉하면 넥서스 피해와 적 제거를 요청한다.
 */
namespace Pakuri.InGame
{
    internal static class EnemyNexusAttack
    {
        public static void Tick(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            CombatUnitEntry nexusTarget,
            float deltaTime,
            InGameCombatManager combatManager)
        {
            if (!IsTouchingNexus(enemyEntry, nexusTarget))
            {
                if (StatusCombatRules.CanMove(enemyModel))
                {
                    EnemyMovement.MoveToward(enemyEntry, nexusTarget, enemyModel, deltaTime);
                }

                return;
            }

            var damage = Mathf.Max(1f, enemyModel.NexusDamage);
            combatManager.ApplyDamage(
                nexusTarget.Model,
                damage,
                DamageAttribute.Physical,
                enemyModel,
                false);
            combatManager.DespawnUnit(enemyModel);
        }

        private static bool IsTouchingNexus(CombatUnitEntry enemyEntry, CombatUnitEntry nexusTarget)
        {
            var enemyPoint = enemyEntry.ResolveTargetPoint();
            var targetColliders = nexusTarget.GetHitboxColliders();
            for (var i = 0; i < targetColliders.Length; i++)
            {
                var collider = targetColliders[i];
                if (collider.enabled && collider.OverlapPoint(enemyPoint))
                {
                    return true;
                }
            }

            if (UnitHitboxOverlap.IsTargetInsideHitbox(enemyEntry.GetHitboxColliders(), nexusTarget))
            {
                return true;
            }

            return Vector2.Distance(enemyEntry.Transform.position, nexusTarget.Transform.position) <= 0.25f;
        }
    }
}
