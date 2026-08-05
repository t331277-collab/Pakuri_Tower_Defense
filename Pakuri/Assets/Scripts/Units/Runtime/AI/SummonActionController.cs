/*
 * 역할: 소환수 런타임 행동 반복.
 * 책임: 소환수 이동과 적 접촉에 따른 이동 정지를 갱신한다.
 */

using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 소환수의 이동 행동을 전투 프레임에 맞춰 진행한다.
    public class SummonActionController
    {
        private readonly UnitSpawnManager units;
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>(1);

        public SummonActionController(UnitSpawnManager units)
        {
            this.units = units;
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f || units.EnemyCount <= 0 || units.EnemySpawnPoint == null)
            {
                return;
            }

            var players = units.Players;
            for (var i = 0; i < players.Count; i++)
            {
                TickSummon(players[i], deltaTime);
            }
        }

        private void TickSummon(CombatUnitEntry entry, float deltaTime)
        {
            var model = entry != null ? entry.Model : null;
            if (model == null
                || model.Identity == null
                || model.Identity.Role != UnitRole.Summon
                || !entry.IsAlive
                || entry.Transform == null
                || model.Stats == null)
            {
                return;
            }

            var current = entry.Transform.position;
            var moveDistance = Mathf.Max(0f, model.Stats.MoveSpeed) * deltaTime;
            var next = Vector3.MoveTowards(
                current,
                units.EnemySpawnPoint.position,
                moveDistance);
            var movement = (Vector2)(next - current);

            UnitCollisionResolver.CollectTargets(
                units,
                units.Enemies,
                entry,
                movement,
                collisionTargets);
            if (collisionTargets.Count > 0)
            {
                return;
            }

            entry.Transform.position = next;
        }
    }
}
