using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 스킬 대상 지정 계산과 변환 기능을 제공한다.
     */
    internal static class SkillTargetingUtility
    {
        /*
         * 가장 가까운 대상을 찾는다.
         */
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
            var bestStacks = int.MinValue;
            var origin = caster.Transform.position;
            var selectionStatusId = targeting != null ? targeting.SelectionStatusId : string.Empty;
            var selectionStatusMinStacks = targeting != null ? targeting.SelectionStatusMinStacks : 0;

            if (selection == SkillTargetSelection.Random)
            {
                var valid = new List<UnitRosterEntry>();
                for (var i = 0; i < candidates.Count; i++)
                {
                    var candidate = candidates[i];
                    if (candidate != null && candidate.IsAlive && candidate.Transform != null && candidate.Model != null)
                    {
                        valid.Add(candidate);
                    }
                }

                return valid.Count > 0 ? valid[UnityEngine.Random.Range(0, valid.Count)] : null;
            }

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

                if (selection == SkillTargetSelection.HighestStacks)
                {
                    var stacks = ResolveStatusStacks(candidate.Model, selectionStatusId);
                    if (stacks < Mathf.Max(0, selectionStatusMinStacks))
                    {
                        continue;
                    }

                    if (stacks < bestStacks)
                    {
                        continue;
                    }

                    var tieOffset = candidate.Transform.position - origin;
                    tieOffset.z = 0f;
                    var tieDistanceSq = tieOffset.sqrMagnitude;
                    if (stacks == bestStacks && tieDistanceSq >= bestDistanceSq)
                    {
                        continue;
                    }

                    best = candidate;
                    bestStacks = stacks;
                    bestDistanceSq = tieDistanceSq;
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
                var distanceSq = offset.sqrMagnitude;
                if (selection == SkillTargetSelection.Farthest)
                {
                    if (best != null && distanceSq <= bestDistanceSq)
                    {
                        continue;
                    }

                    best = candidate;
                    bestDistanceSq = distanceSq;
                    continue;
                }

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
         * 대상을 방향을 계산한다.
         */
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

        /*
         * 대상 목록을 결정한다.
         */
        public static IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            return ResolveTargetList(caster, roster, targeting, null, 0);
        }

        /*
         * 대상 목록을 결정한다.
         */
        public static IReadOnlyList<UnitRosterEntry> ResolveTargetList(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            string requiredStatusId,
            int requiredStatusMinStacks)
        {
            if (caster == null || roster == null)
            {
                return Array.Empty<UnitRosterEntry>();
            }

            var side = targeting != null ? targeting.TargetSide : SkillTargetSide.Enemy;
            if (side == SkillTargetSide.Self)
            {
                return IsSkillTargetable(caster) ? new[] { caster } : Array.Empty<UnitRosterEntry>();
            }

            var targets = side == SkillTargetSide.Ally || side == SkillTargetSide.AllAllies
                ? caster.Model != null
                    && caster.Model.Identity != null
                    && caster.Model.Identity.Side == UnitSide.Enemy
                        ? roster.Enemies
                        : roster.Players
                : caster.Model != null
                    && caster.Model.Identity != null
                    && caster.Model.Identity.Side == UnitSide.Enemy
                        ? roster.Players
                        : roster.Enemies;

            var selectionStatusId = targeting != null ? targeting.SelectionStatusId : string.Empty;
            var selectionStatusMinStacks = targeting != null ? Mathf.Max(0, targeting.SelectionStatusMinStacks) : 0;
            var useSelectionStatusFilter = !string.IsNullOrWhiteSpace(selectionStatusId) && selectionStatusMinStacks > 0;
            var mustFilterNexus = ContainsNexusTarget(targets);
            if (string.IsNullOrWhiteSpace(requiredStatusId) && !useSelectionStatusFilter && !mustFilterNexus)
            {
                return targets;
            }

            var filtered = new List<UnitRosterEntry>();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var model = target != null ? target.Model : null;
                if (!IsSkillTargetable(target))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(requiredStatusId)
                    && !HasRequiredStatus(model, requiredStatusId, requiredStatusMinStacks))
                {
                    continue;
                }

                if (useSelectionStatusFilter
                    && !HasRequiredStatus(model, selectionStatusId, selectionStatusMinStacks))
                {
                    continue;
                }

                filtered.Add(target);
            }

            return filtered;
        }

        /*
         * 넥서스 대상을 포함하는지 확인한다.
         */
        private static bool ContainsNexusTarget(IReadOnlyList<UnitRosterEntry> targets)
        {
            for (var i = 0; targets != null && i < targets.Count; i++)
            {
                if (!IsSkillTargetable(targets[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 유닛이 현재 스킬의 대상으로 지정될 수 있는지 확인한다.
         */
        private static bool IsSkillTargetable(UnitRosterEntry entry)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return identity == null || identity.Role != UnitRole.Nexus;
        }

        /*
         * 필수 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasRequiredStatus(BaseUnitRuntimeModel model, string statusId, int minimumStacks)
        {
            if (model == null || string.IsNullOrWhiteSpace(statusId))
            {
                return false;
            }

            var minStacks = Mathf.Max(1, minimumStacks);
            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.GetStacks(kind) >= minStacks;
        }

        /*
         * 상태 중첩을 결정한다.
         */
        private static int ResolveStatusStacks(BaseUnitRuntimeModel model, string statusId)
        {
            if (model == null || string.IsNullOrWhiteSpace(statusId))
            {
                return 0;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return 0;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f ? 1 : 0;
            }

            return model.Statuses != null ? model.Statuses.GetStacks(kind) : 0;
        }
    }
}
