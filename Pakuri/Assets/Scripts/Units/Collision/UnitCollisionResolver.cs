using System.Collections.Generic;
using UnityEngine;

/*
 * 스킬 Collider가 실제로 겹치거나 이동 중 통과한 등록 유닛을 한 경로로 반환한다.
 */
namespace Pakuri.InGame
{
    internal static class UnitCollisionResolver
    {
        private static readonly List<Collider2D> overlapResults = new List<Collider2D>(32);
        private static readonly List<RaycastHit2D> castResults = new List<RaycastHit2D>(32);
        private static readonly List<Collider2D> sourceColliders = new List<Collider2D>(4);
        private static readonly HashSet<CombatUnitEntry> collidedUnits = new HashSet<CombatUnitEntry>();

        /*
         * 등록 유닛의 Collider를 공격 Collider로 사용해 충돌 대상을 반환한다.
         */
        public static void CollectTargets(
            UnitSpawnManager units /* 필드 전투 유닛 관리자 */,
            IReadOnlyList<CombatUnitEntry> candidates /* 충돌 후 허용할 대상 후보 */,
            CombatUnitEntry collisionSource /* 충돌 판정을 발생시키는 등록 유닛 */,
            Vector2 movement /* 이번 판정에서 이동할 거리와 방향 */,
            List<CombatUnitEntry> targets /* 실제 충돌한 대상 결과 */)
        {
            sourceColliders.Clear();
            if (collisionSource != null && collisionSource.HitboxRoot != null)
            {
                collisionSource.HitboxRoot.GetComponentsInChildren(false, sourceColliders);
            }

            CollectTargets(units, candidates, sourceColliders, movement, targets);
        }

        /*
         * 공격 Collider와 겹치거나 이동 중 통과한 등록 유닛을 후보 순서로 반환한다.
         */
        public static void CollectTargets(
            UnitSpawnManager units /* 필드 전투 유닛 관리자 */,
            IReadOnlyList<CombatUnitEntry> candidates /* 충돌 후 허용할 대상 후보 */,
            IReadOnlyList<Collider2D> hitboxColliders /* 공격 판정 Collider 목록 */,
            Vector2 movement /* 이번 판정에서 이동할 거리와 방향 */,
            List<CombatUnitEntry> targets /* 실제 충돌한 대상 결과 */)
        {
            targets.Clear();
            collidedUnits.Clear();
            if (units == null || candidates == null || hitboxColliders == null)
            {
                return;
            }

            Physics2D.SyncTransforms();
            for (var i = 0; i < hitboxColliders.Count; i++)
            {
                var hitbox = hitboxColliders[i];
                if (hitbox == null || !hitbox.enabled)
                {
                    continue;
                }

                overlapResults.Clear();
                hitbox.Overlap(overlapResults);
                for (var j = 0; j < overlapResults.Count; j++)
                {
                    AddMappedUnit(units, overlapResults[j]);
                }

                if (movement.sqrMagnitude <= 0.000001f)
                {
                    continue;
                }

                castResults.Clear();
                hitbox.Cast(
                    movement.normalized,
                    ContactFilter2D.noFilter,
                    castResults,
                    movement.magnitude,
                    false);
                for (var j = 0; j < castResults.Count; j++)
                {
                    AddMappedUnit(units, castResults[j].collider);
                }
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate != null
                    && candidate.Model != null
                    && candidate.IsAlive
                    && collidedUnits.Contains(candidate))
                {
                    targets.Add(candidate);
                }
            }
        }

        /*
         * 물리 결과 Collider를 UnitSpawnManager의 등록 유닛으로 변환한다.
         */
        private static void AddMappedUnit(UnitSpawnManager units /* 필드 전투 유닛 관리자 */, Collider2D collider /* 물리 판정에서 얻은 Collider */)
        {
            if (collider == null)
            {
                return;
            }

            var unit = units.FindByCollider(collider);
            if (unit != null)
            {
                collidedUnits.Add(unit);
            }
        }
    }
}
