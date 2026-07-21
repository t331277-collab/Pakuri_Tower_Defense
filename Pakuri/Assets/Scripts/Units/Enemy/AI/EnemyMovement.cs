using UnityEngine;

/*
 * 적의 이동 속도와 상태 효과 배율을 적용해 선택된 대상 쪽으로 이동시킨다.
 */
namespace Pakuri.InGame
{
    internal static class EnemyMovement
    {
        public static void MoveToward(
            CombatUnitEntry enemyEntry,
            CombatUnitEntry target,
            EnemyCombatState enemyModel,
            float deltaTime)
        {
            var moveSpeed = Mathf.Max(0f, enemyModel.Stats.MoveSpeed);
            moveSpeed *= StatusCombatRules.ResolveMoveSpeedMultiplier(enemyModel);
            if (moveSpeed <= 0f)
            {
                return;
            }

            var current = enemyEntry.Transform.position;
            var targetPosition = target.Transform.position;
            targetPosition.z = current.z;
            enemyEntry.Transform.position = Vector3.MoveTowards(
                current,
                targetPosition,
                moveSpeed * deltaTime);
        }
    }
}
