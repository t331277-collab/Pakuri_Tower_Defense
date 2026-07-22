using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 대상 지정 계산과 변환 기능을 제공한다.
 */
namespace Pakuri.InGame
{
    internal enum SkillDeploymentRepeatMode
    {
        RepeatNearest,
        RandomExisting
    }

    internal static class SkillTargeting
    {
        /*
         * ResolveOrderedTargets 결과를 계산해 반환한다.
         */
        public static List<CombatUnitEntry> ResolveOrderedTargets(
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
            SkillTargetingSpec targetingSpec)
        {
            return ResolveOrderedTargets(sourceEntry, unitRoster, targetingSpec, StatusEffectKind.None, 0);
        }

        /*
         * ResolveOrderedTargets 결과를 계산해 반환한다.
         */
        public static List<CombatUnitEntry> ResolveOrderedTargets(
            CombatUnitEntry sourceEntry,
            CombatUnitRegistry unitRoster,
            SkillTargetingSpec targetingSpec,
            StatusEffectKind requiredStatusKind,
            int requiredStatusMinStacks)
        {
            var candidates = ResolveTargetList(
                sourceEntry,
                unitRoster,
                targetingSpec,
                requiredStatusKind,
                requiredStatusMinStacks);
            var targets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && target.IsAlive && target.Model != null && target.Transform != null)
                {
                    targets.Add(target);
                }
            }

            var selection = targetingSpec != null ? targetingSpec.Selection : SkillTargetSelection.Nearest;
            targets.Sort((left, right) => CompareTargets(sourceEntry, targetingSpec, selection, left, right));
            return targets;
        }
        /*
         * 가장 가까운 대상을 찾는다.
         */
        public static CombatUnitEntry FindNearestTarget(
            CombatUnitEntry caster,
            CombatUnitRegistry roster,
            SkillTargetingSpec targeting)
        {
            if (caster == null || caster.Transform == null || roster == null)
            {
                return null;
            }

            var candidates = ResolveTargetList(caster, roster, targeting);
            var selection = targeting != null ? targeting.Selection : SkillTargetSelection.Nearest;
            CombatUnitEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var bestHealth = float.MinValue;
            var bestLowestHealth = float.MaxValue;
            var bestStacks = int.MinValue;
            var origin = caster.Transform.position;
            var selectionStatusKind = StatusEffectKind.None;
            var selectionStatusMinStacks = 0;
            if (targeting != null)
            {
                selectionStatusKind = targeting.SelectionStatusKind;
                selectionStatusMinStacks = targeting.SelectionStatusMinStacks;
            }

            if (selection == SkillTargetSelection.Random)
            {
                var valid = new List<CombatUnitEntry>();
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
                    var stacks = ResolveStatusStacks(candidate.Model, selectionStatusKind);
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
        public static Vector2 DirectionToTarget(Vector3 origin, CombatUnitEntry target)
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
        public static IReadOnlyList<CombatUnitEntry> ResolveTargetList(
            CombatUnitEntry caster,
            CombatUnitRegistry roster,
            SkillTargetingSpec targeting)
        {
            return ResolveTargetList(caster, roster, targeting, StatusEffectKind.None, 0);
        }

        /*
         * 대상 목록을 결정한다.
         */
        public static IReadOnlyList<CombatUnitEntry> ResolveTargetList(
            CombatUnitEntry caster,
            CombatUnitRegistry roster,
            SkillTargetingSpec targeting,
            StatusEffectKind requiredStatusKind,
            int requiredStatusMinStacks)
        {
            if (caster == null || roster == null)
            {
                return Array.Empty<CombatUnitEntry>();
            }

            var side = targeting != null ? targeting.TargetSide : SkillTargetSide.Enemy;
            if (side == SkillTargetSide.Self)
            {
                return IsSkillTargetable(caster) ? new[] { caster } : Array.Empty<CombatUnitEntry>();
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

            var selectionStatusKind = StatusEffectKind.None;
            var selectionStatusMinStacks = 0;
            if (targeting != null)
            {
                selectionStatusKind = targeting.SelectionStatusKind;
                selectionStatusMinStacks = Mathf.Max(0, targeting.SelectionStatusMinStacks);
            }
            var useSelectionStatusFilter = selectionStatusKind != StatusEffectKind.None && selectionStatusMinStacks > 0;
            var mustFilterNexus = ContainsNexusTarget(targets);
            if (requiredStatusKind == StatusEffectKind.None && !useSelectionStatusFilter && !mustFilterNexus)
            {
                return targets;
            }

            var filtered = new List<CombatUnitEntry>();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                var model = target != null ? target.Model : null;
                if (!IsSkillTargetable(target))
                {
                    continue;
                }

                if (requiredStatusKind != StatusEffectKind.None
                    && !HasRequiredStatus(model, requiredStatusKind, requiredStatusMinStacks))
                {
                    continue;
                }

                if (useSelectionStatusFilter
                    && !HasRequiredStatus(model, selectionStatusKind, selectionStatusMinStacks))
                {
                    continue;
                }

                filtered.Add(target);
            }

            return filtered;
        }

        /*
         * 조준 정보와 대상 설정으로 범위 중심점을 결정한다.
         */
        public static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            if (context.HasManualTargetPoint)
            {
                return context.ManualTargetPoint;
            }

            if (context.HasManualAimDirection && context.ManualAimDirection.sqrMagnitude > 0.0001f)
            {
                var radius = ResolveBaseRadius(targeting, area);
                return (Vector2)origin + context.ManualAimDirection.normalized * Mathf.Max(1f, radius);
            }

            var target = FindNearestTarget(context.CasterEntry, context.Roster, targeting);
            if (target != null && target.Transform != null)
            {
                return target.Transform.position;
            }

            return origin;
        }

        /*
         * 대상 설정에 기록된 기본 반경을 결정한다.
         */
        public static float ResolveBaseRadius(SkillTargetingSpec targeting, AreaBlueprintSpec area)
        {
            if (area != null && area.Radius > 0f)
            {
                return area.Radius;
            }

            return targeting != null ? targeting.Radius : 0f;
        }

        /*
         * 선택지의 반경 배율과 추가값을 적용한다.
         */
        public static float ResolveRadius(float baseRadius, SkillSnapshot snapshot)
        {
            var radius = baseRadius;
            if (snapshot != null)
            {
                radius = radius * Mathf.Max(0f, snapshot.RadiusMultiplier) + snapshot.RadiusBonus;
            }

            return Mathf.Max(0f, radius);
        }

        /*
         * 반경 변화에 맞는 프리팹 크기 배율을 결정한다.
         */
        public static float ResolvePrefabScaleFactor(float baseRadius, SkillSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 1f;
            }

            if (baseRadius <= 0.0001f)
            {
                return Mathf.Max(0.01f, snapshot.RadiusMultiplier);
            }

            return Mathf.Max(0.01f, ResolveRadius(baseRadius, snapshot) / baseRadius);
        }

        /*
         * 반복 배치 횟수만큼 서로 다른 대상 위치를 우선 선택한다.
         */
        public static List<Vector2> ResolveTargetAnchoredCenters(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            Vector2 primaryCenter,
            int deploymentCount,
            bool coverAll,
            SkillDeploymentRepeatMode repeatMode)
        {
            var centers = new List<Vector2> { primaryCenter };
            if (deploymentCount <= 1 || coverAll)
            {
                while (centers.Count < deploymentCount)
                {
                    centers.Add(primaryCenter);
                }

                return centers;
            }

            var orderedTargets = SkillTargeting.ResolveOrderedTargets(
                context.CasterEntry,
                context.Roster,
                targeting);
            if (orderedTargets.Count == 0)
            {
                while (centers.Count < deploymentCount)
                {
                    centers.Add(primaryCenter);
                }

                return centers;
            }

            centers.Clear();
            var claimedUnitIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < orderedTargets.Count && centers.Count < deploymentCount; i++)
            {
                var target = orderedTargets[i];
                var identity = target != null && target.Model != null ? target.Model.Identity : null;
                var unitId = identity != null ? identity.UnitId : string.Empty;
                if (!string.IsNullOrWhiteSpace(unitId) && !claimedUnitIds.Add(unitId))
                {
                    continue;
                }

                if (target != null && target.Transform != null)
                {
                    centers.Add(target.Transform.position);
                }
            }

            var repeatIndex = 0;
            while (centers.Count < deploymentCount)
            {
                CombatUnitEntry fallbackTarget;
                if (repeatMode == SkillDeploymentRepeatMode.RandomExisting)
                {
                    fallbackTarget = orderedTargets[UnityEngine.Random.Range(0, orderedTargets.Count)];
                }
                else
                {
                    fallbackTarget = orderedTargets[repeatIndex % orderedTargets.Count];
                    repeatIndex++;
                }

                if (fallbackTarget != null && fallbackTarget.Transform != null)
                {
                    centers.Add(fallbackTarget.Transform.position);
                }
                else
                {
                    centers.Add(primaryCenter);
                }
            }

            return centers;
        }

        /*
         * 넥서스 대상을 포함하는지 확인한다.
         */
        private static bool ContainsNexusTarget(IReadOnlyList<CombatUnitEntry> targets)
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
         * CompareTargets 작업 결과를 반환한다.
         */
        private static int CompareTargets(
            CombatUnitEntry sourceEntry,
            SkillTargetingSpec targetingSpec,
            SkillTargetSelection selection,
            CombatUnitEntry left,
            CombatUnitEntry right)
        {
            if (left == right)
            {
                return 0;
            }

            if (left == null)
            {
                return 1;
            }

            if (right == null)
            {
                return -1;
            }

            var leftHealth = left.Model != null && left.Model.Resources != null ? left.Model.Resources.CurrentHealth : 0f;
            var rightHealth = right.Model != null && right.Model.Resources != null ? right.Model.Resources.CurrentHealth : 0f;
            if (selection == SkillTargetSelection.HighestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return rightHealth.CompareTo(leftHealth);
            }

            if (selection == SkillTargetSelection.LowestHealth && !Mathf.Approximately(leftHealth, rightHealth))
            {
                return leftHealth.CompareTo(rightHealth);
            }

            if (selection == SkillTargetSelection.HighestStacks)
            {
                var statusKind = StatusEffectKind.None;
                if (targetingSpec != null)
                {
                    statusKind = targetingSpec.SelectionStatusKind;
                }
                var leftStacks = ResolveStatusStacks(left.Model, statusKind);
                var rightStacks = ResolveStatusStacks(right.Model, statusKind);
                if (leftStacks != rightStacks)
                {
                    return rightStacks.CompareTo(leftStacks);
                }
            }

            var leftDistance = ResolveDistanceSquared(sourceEntry, left);
            var rightDistance = ResolveDistanceSquared(sourceEntry, right);
            if (selection == SkillTargetSelection.Farthest)
            {
                return rightDistance.CompareTo(leftDistance);
            }

            return leftDistance.CompareTo(rightDistance);
        }

        /*
         * ResolveDistanceSquared 결과를 계산해 반환한다.
         */
        private static float ResolveDistanceSquared(CombatUnitEntry sourceEntry, CombatUnitEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
        }

        /*
         * 유닛이 현재 스킬의 대상으로 지정될 수 있는지 확인한다.
         */
        private static bool IsSkillTargetable(CombatUnitEntry entry)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return identity == null || identity.Role != UnitRole.Nexus;
        }

        /*
         * 필수 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasRequiredStatus(UnitCombatState model, StatusEffectKind kind, int minimumStacks)
        {
            if (model == null || kind == StatusEffectKind.None)
            {
                return false;
            }

            var minStacks = Mathf.Max(1, minimumStacks);
            if (kind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.GetStacks(kind) >= minStacks;
        }

        /*
         * 상태 중첩을 결정한다.
         */
        private static int ResolveStatusStacks(UnitCombatState model, StatusEffectKind kind)
        {
            if (model == null || kind == StatusEffectKind.None)
            {
                return 0;
            }

            if (kind == StatusEffectKind.Shield)
            {
                if (model.Resources != null && model.Resources.CurrentShield > 0f)
                {
                    return 1;
                }

                return 0;
            }

            if (model.Statuses != null)
            {
                return model.Statuses.GetStacks(kind);
            }

            return 0;
        }
    }
}
