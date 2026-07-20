using UnityEngine;

/*
 * 적이 공격 대상과 회복 대상을 선택할 때 사용하는 검색 규칙을 제공한다.
 */
namespace Pakuri.InGame
{
    internal static class EnemyTargetSelector
    {
        /*
         * 가장 가까운 일반 플레이어를 찾고 없으면 넥서스를 반환한다.
         */
        public static UnitRosterEntry FindNearestPlayerTarget(UnitRosterEntry enemyEntry, UnitRosterService roster)
        {
            var best = FindNearestPlayerTarget(enemyEntry, roster, includeNexus: false);
            if (best != null)
            {
                return best;
            }

            // 일반 플레이어가 없을 때만 넥서스를 대상으로 선택한다.
            return FindNearestPlayerTarget(enemyEntry, roster, includeNexus: true);
        }

        /*
         * 넥서스 포함 여부가 일치하는 살아 있는 플레이어 중 가장 가까운 대상을 찾는다.
         */
        private static UnitRosterEntry FindNearestPlayerTarget(
            UnitRosterEntry enemyEntry,
            UnitRosterService roster,
            bool includeNexus)
        {
            var players = roster.Players;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = enemyEntry.Transform.position;
            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                if (!IsActive(candidate))
                {
                    continue;
                }

                var isNexus = IsNexus(candidate);
                // 1차 검색은 일반 플레이어만, 2차 검색은 넥서스만 통과시킨다.
                if (isNexus != includeNexus)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                // 전투 거리는 XY 평면만 사용한다.
                offset.z = 0f;
                // 제곱 거리는 제곱근 계산 없이 같은 근접 순서를 비교할 수 있다.
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        /*
         * 로스터 항목이 넥서스인지 반환한다.
         */
        public static bool IsNexus(UnitRosterEntry entry)
        {
            return entry.Model.IsNexus;
        }

        /*
         * 체력이 감소한 살아 있는 적 유닛 중 체력 비율이 가장 낮은 아군을 찾는다.
         */
        public static UnitRosterEntry FindLowestHealthEnemyAlly(UnitRosterService roster)
        {
            var enemies = roster.Enemies;
            UnitRosterEntry best = null;
            var bestHealthRatio = float.MaxValue;

            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!IsActive(candidate))
                {
                    continue;
                }

                var resources = candidate.Model.Resources;
                var stats = candidate.Model.Stats;
                // 최대 체력이 없는 유닛은 비율 비교에서 제외한다.
                if (stats.MaxHealth <= 0f)
                {
                    continue;
                }

                var healthRatio = Mathf.Clamp01(resources.CurrentHealth / stats.MaxHealth);
                // 체력이 가득 찼거나 현재 후보보다 건강한 유닛은 제외한다.
                // 같은 체력 비율이면 로스터에서 먼저 발견한 유닛을 유지한다.
                if (healthRatio >= 1f || healthRatio >= bestHealthRatio)
                {
                    continue;
                }

                best = candidate;
                bestHealthRatio = healthRatio;
            }

            return best;
        }

        /*
         * 로스터 항목이 살아 있는지 반환한다.
         */
        public static bool IsActive(UnitRosterEntry entry)
        {
            return entry.IsAlive;
        }
    }
}
