using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class EnemyTargeting
    {
        public static UnitRosterEntry FindNearestPlayerTarget(UnitRosterEntry enemyEntry, UnitRosterService roster)
        {
            var best = FindNearestPlayerTarget(enemyEntry, roster, includeNexus: false);
            return best ?? FindNearestPlayerTarget(enemyEntry, roster, includeNexus: true);
        }

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
                if (isNexus != includeNexus)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
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

        public static bool IsNexus(UnitRosterEntry entry)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return identity != null && identity.Role == UnitRole.Nexus;
        }

        public static UnitRosterEntry FindLowestHealthEnemyAlly(UnitRosterService roster)
        {
            var enemies = roster.Enemies;
            UnitRosterEntry best = null;
            var bestHealthRatio = float.MaxValue;

            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!IsActive(candidate) || candidate.Model == null || candidate.Transform == null)
                {
                    continue;
                }

                var resources = candidate.Model.Resources;
                var stats = candidate.Model.Stats;
                if (resources == null || stats == null || stats.MaxHealth <= 0f)
                {
                    continue;
                }

                var healthRatio = Mathf.Clamp01(resources.CurrentHealth / stats.MaxHealth);
                if (healthRatio >= 1f || healthRatio >= bestHealthRatio)
                {
                    continue;
                }

                best = candidate;
                bestHealthRatio = healthRatio;
            }

            return best;
        }

        public static List<UnitRosterEntry> FindEnemyAlliesInRadius(
            UnitRosterEntry source,
            UnitRosterService roster,
            float radius)
        {
            var result = new List<UnitRosterEntry>();
            if (source == null || source.Transform == null || roster == null)
            {
                return result;
            }

            var radiusSq = radius > 0f ? radius * radius : 0f;
            var origin = source.Transform.position;
            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!IsActive(candidate) || candidate.Model == null || candidate.Transform == null)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                if (radius <= 0f && candidate != source)
                {
                    continue;
                }

                if (radius > 0f && offset.sqrMagnitude > radiusSq)
                {
                    continue;
                }

                result.Add(candidate);
            }

            return result;
        }

        public static bool IsActive(UnitRosterEntry entry)
        {
            return entry != null && entry.IsAlive && entry.Transform != null;
        }
    }
}
