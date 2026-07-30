/*
 * 역할: 공통 유닛 충돌 판정.
 * 책임: 스킬 Hitbox의 Collider 접촉을 등록 유닛으로 대응시키고 허용된 대상만 반환한다.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{

    /// UnitCollisionResolver 처리에 필요한 런타임 규칙 또는 대상을 결정한다.
    internal static class UnitCollisionResolver
    {
        private static readonly List<Collider2D> overlapResults = new List<Collider2D>(32);
        private static readonly List<RaycastHit2D> castResults = new List<RaycastHit2D>(32);
        private static readonly List<Collider2D> sourceColliders = new List<Collider2D>(4);
        private static readonly HashSet<CombatUnitEntry> collidedUnits = new HashSet<CombatUnitEntry>();

        /// 전달된 런타임 입력값을 사용해 Targets를 결과 컬렉션에 수집한다.
        public static void CollectTargets(
            UnitSpawnManager units,
            IReadOnlyList<CombatUnitEntry> candidates,
            CombatUnitEntry collisionSource,
            Vector2 movement,
            List<CombatUnitEntry> targets)
        {
            sourceColliders.Clear();
            if (collisionSource != null && collisionSource.HitboxRoot != null)
            {
                collisionSource.HitboxRoot.GetComponentsInChildren(false, sourceColliders);
            }

            CollectTargets(units, candidates, sourceColliders, movement, targets);
        }

        /// 전달된 런타임 입력값을 사용해 Targets를 결과 컬렉션에 수집한다.
        public static void CollectTargets(
            UnitSpawnManager units,
            IReadOnlyList<CombatUnitEntry> candidates,
            IReadOnlyList<Collider2D> hitboxColliders,
            Vector2 movement,
            List<CombatUnitEntry> targets)
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

        /// 전달된 런타임 입력값을 사용해 MappedUnit를 소유한 런타임 상태에 추가한다.
        private static void AddMappedUnit(UnitSpawnManager units, Collider2D collider)
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
