using UnityEngine;

/*
 * 두 유닛의 실제 Collider가 겹치는지 검사한다.
 */
namespace Pakuri.InGame
{
    internal static class UnitHitboxOverlap
    {
        public static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, CombatUnitEntry target)
        {
            if (hitboxColliders == null || target == null || target.Model == null || !target.IsAlive)
            {
                return false;
            }

            var targetColliders = target.GetHitboxColliders();
            if (targetColliders == null || targetColliders.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < hitboxColliders.Length; i++)
            {
                var hitbox = hitboxColliders[i];
                if (hitbox == null || !hitbox.enabled)
                {
                    continue;
                }

                for (var j = 0; j < targetColliders.Length; j++)
                {
                    var targetCollider = targetColliders[j];
                    if (targetCollider == null || !targetCollider.enabled)
                    {
                        continue;
                    }

                    if (hitbox.Distance(targetCollider).isOverlapped)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
