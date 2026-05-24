using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillTargetingUtility
    {
        public static UnitRosterEntry FindNearestTarget(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            if (caster == null || caster.Transform == null || roster == null)
            {
                return null;
            }

            var candidates = ResolveTargetList(caster, roster, targeting);
            var selection = targeting != null ? targeting.Selection : SkillTargetSelection.Nearest;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var bestHealth = float.MinValue;
            var bestLowestHealth = float.MaxValue;
            var origin = caster.Transform.position;

            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || candidate.Transform == null || candidate.Model == null)
                {
                    continue;
                }

                if (selection == SkillTargetSelection.HighestHealth)
                {
                    var health = candidate.Model.Resources != null ? candidate.Model.Resources.CurrentHealth : 0f;
                    if (health < bestHealth)
                    {
                        continue;
                    }

                    if (Mathf.Approximately(health, bestHealth))
                    {
                        var tieOffset = candidate.Transform.position - origin;
                        tieOffset.z = 0f;
                        var tieDistanceSq = tieOffset.sqrMagnitude;
                        if (tieDistanceSq >= bestDistanceSq)
                        {
                            continue;
                        }

                        bestDistanceSq = tieDistanceSq;
                    }
                    else
                    {
                        var offsetForTie = candidate.Transform.position - origin;
                        offsetForTie.z = 0f;
                        bestDistanceSq = offsetForTie.sqrMagnitude;
                    }

                    best = candidate;
                    bestHealth = health;
                    continue;
                }

                if (selection == SkillTargetSelection.LowestHealth)
                {
                    var health = candidate.Model.Resources != null ? candidate.Model.Resources.CurrentHealth : 0f;
                    if (health > bestLowestHealth)
                    {
                        continue;
                    }

                    if (Mathf.Approximately(health, bestLowestHealth))
                    {
                        var tieOffset = candidate.Transform.position - origin;
                        tieOffset.z = 0f;
                        var tieDistanceSq = tieOffset.sqrMagnitude;
                        if (tieDistanceSq >= bestDistanceSq)
                        {
                            continue;
                        }

                        bestDistanceSq = tieDistanceSq;
                    }
                    else
                    {
                        var offsetForTie = candidate.Transform.position - origin;
                        offsetForTie.z = 0f;
                        bestDistanceSq = offsetForTie.sqrMagnitude;
                    }

                    best = candidate;
                    bestLowestHealth = health;
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

        public static Vector2 DirectionToTarget(Vector3 origin, UnitRosterEntry target)
        {
            if (target == null || target.Transform == null)
            {
                return Vector2.zero;
            }

            var direction = target.Transform.position - origin;
            direction.z = 0f;
            return direction;
        }

        public static IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            if (caster == null || roster == null)
            {
                return Array.Empty<UnitRosterEntry>();
            }

            var side = targeting != null ? targeting.TargetSide : SkillTargetSide.Enemy;
            if (side == SkillTargetSide.Self)
            {
                return new[] { caster };
            }

            if (side == SkillTargetSide.Ally || side == SkillTargetSide.AllAllies)
            {
                return caster.Model != null
                    && caster.Model.Identity != null
                    && caster.Model.Identity.Side == UnitSide.Enemy
                        ? roster.Enemies
                        : roster.Players;
            }

            return caster.Model != null
                && caster.Model.Identity != null
                && caster.Model.Identity.Side == UnitSide.Enemy
                    ? roster.Players
                    : roster.Enemies;
        }
    }
}
