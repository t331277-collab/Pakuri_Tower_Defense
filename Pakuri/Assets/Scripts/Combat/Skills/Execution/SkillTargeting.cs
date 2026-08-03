/*
 * 역할: 진영, 형태, 상태, 거리, 선택 우선순위에 따라 후보를 걸러 순서를 정한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 여러 배치를 새 대상에 펼칠지 기존 대상을 다시 사용할지 구분한다.
    internal enum SkillDeploymentRepeatMode
    {
        RepeatNearest,
        RandomExisting
    }

    /// 동일한 대상 규칙이 모든 스킬 계열에서 같은 결과를 내게 한다.
    internal static class SkillTargeting
    {

        /// 기본 대상 규칙으로 후보를 우선순위에 맞춰 정렬한다.
        public static List<CombatUnitEntry> OrderedTargets(
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec)
        {

            return OrderedTargets(sourceEntry, unitRoster, targetingSpec, StatusEffectKind.None, 0);
        }

        /// 사건 대상 고정 여부까지 반영해 실행 순서를 만든다.
        public static List<CombatUnitEntry> OrderedTargets(
            SkillExecutionContext context,
            SkillTargetingSpec targetingSpec)
        {
            if (context == null)
            {
                return new List<CombatUnitEntry>();
            }

            if (!context.LockToEventTarget)
            {
                return OrderedTargets(
                    context.CasterEntry,
                    context.Roster,
                    targetingSpec);
            }

            return OrderedTargets(
                context.CasterEntry,
                context.Roster,
                targetingSpec,
                context.EventTarget,
                true);
        }

        /// 연쇄 선택과 사건 대상 고정을 포함한 실행 순서를 만든다.
        public static List<CombatUnitEntry> OrderedTargets(
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            UnitCombatState eventTarget,
            bool lockToEventTarget)
        {
            if (targetingSpec != null
                && targetingSpec.Selection == SkillTargetSelection.NearestOtherFromEventTarget)
            {
                var hitTarget = unitRoster != null ? unitRoster.Find(eventTarget) : null;
                var hitPosition = hitTarget != null && hitTarget.Transform != null
                    ? (Vector2)hitTarget.Transform.position
                    : Vector2.zero;
                return ChainTargets(
                    unitRoster,
                    sourceEntry,
                    sourceEntry != null ? sourceEntry.Model : null,
                    hitTarget,
                    hitPosition,
                    targetingSpec.Radius);
            }

            if (!lockToEventTarget)
            {
                return OrderedTargets(sourceEntry, unitRoster, targetingSpec);
            }

            var target = unitRoster != null ? unitRoster.Find(eventTarget) : null;
            if (target == null || !target.IsAlive)
            {
                return new List<CombatUnitEntry>();
            }

            var candidates = TargetList(
                sourceEntry,
                unitRoster,
                targetingSpec);
            for (var i = 0; i < candidates.Count; i++)
            {
                if (candidates[i] == target)
                {
                    return new List<CombatUnitEntry> { target };
                }
            }
            return new List<CombatUnitEntry>();
        }

        /// 필수 상태를 충족한 대상만 실행 순서로 정렬한다.
        public static List<CombatUnitEntry> OrderedTargets(
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            StatusEffectKind requiredStatusKind,
            int requiredStatusMinStacks)
        {
            var candidates = TargetList(
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

        /// 선택 방식에 맞는 최우선 대상을 거리 기준과 함께 고른다.
        public static CombatUnitEntry FindNearestTarget(
            CombatUnitEntry caster,
            UnitSpawnManager roster,
            SkillTargetingSpec targeting)
        {
            if (caster == null || caster.Transform == null || roster == null)
            {
                return null;
            }

            var candidates = TargetList(caster, roster, targeting);
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
                    var health = candidate.Model.Resources != null && candidate.Model.Stats.MaxHealth > 0f
                        ? candidate.Model.Resources.CurrentHealth / candidate.Model.Stats.MaxHealth
                        : 1f;
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
                    var stacks = StatusStacks(candidate.Model, selectionStatusKind);
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

        /// 시전자에서 대상까지의 방향을 계산한다.
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

        /// 타기팅 조건을 만족하는 후보를 모은다.
        public static IReadOnlyList<CombatUnitEntry> TargetList(
            CombatUnitEntry caster,
            UnitSpawnManager roster,
            SkillTargetingSpec targeting)
        {
            return TargetList(caster, roster, targeting, StatusEffectKind.None, 0);
        }

        /// 타기팅 조건을 만족하는 후보를 모은다.
        public static IReadOnlyList<CombatUnitEntry> TargetList(
            CombatUnitEntry caster,
            UnitSpawnManager roster,
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
            var useSkillAttributeFilter =
                targeting != null && targeting.HasSelectionSkillAttribute;
            var mustFilterNexus = ContainsNexusTarget(targets);
            if (requiredStatusKind == StatusEffectKind.None
                && !useSelectionStatusFilter
                && !useSkillAttributeFilter
                && !mustFilterNexus)
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
                if (useSkillAttributeFilter
                    && !HasActiveSkillAttribute(model, targeting.SelectionSkillAttribute))
                {
                    continue;
                }

                filtered.Add(target);
            }

            return filtered;
        }

        /// 보유 스킬 중 지정 속성이 있는지 확인한다.
        private static bool HasActiveSkillAttribute(
            UnitCombatState target,
            DamageAttribute attribute)
        {
            if (target == null || target.Skills == null)
            {
                return false;
            }

            var activeSkills = target.SkillState.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                if (activeSkills[i] != null
                    && activeSkills[i].Data != null
                    && activeSkills[i].Data.Element == attribute)
                {
                    return true;
                }
            }
            return false;
        }

        public static Vector2 AreaCenter(
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
                var radius = BaseRadius(targeting, area);
                return (Vector2)origin + context.ManualAimDirection.normalized * Mathf.Max(1f, radius);
            }

            var target = FindNearestTarget(context.CasterEntry, context.Roster, targeting);
            if (target != null && target.Transform != null)
            {
                return target.Transform.position;
            }

            return origin;
        }

        /// 영역 전용 값이 있으면 공통 대상 범위보다 우선한다.
        public static float BaseRadius(SkillTargetingSpec targeting, AreaBlueprintSpec area)
        {
            if (area != null && area.Radius > 0f)
            {
                return area.Radius;
            }

            return targeting != null ? targeting.Radius : 0f;
        }

        /// 보정된 영역 범위를 계산한다.
        public static float Radius(
            float baseRadius,
            float radiusMultiplier,
            float radiusBonus)
        {
            var radius = baseRadius * Mathf.Max(0f, radiusMultiplier) + radiusBonus;
            return Mathf.Max(0f, radius);
        }

        /// 판정 반경의 변화가 표현 크기에도 같은 비율로 보이게 한다.
        public static float PrefabScaleFactor(
            float baseRadius,
            float radiusMultiplier,
            float radiusBonus)
        {
            if (baseRadius <= 0.0001f)
            {
                return Mathf.Max(0.01f, radiusMultiplier);
            }

            return Mathf.Max(0.01f, Radius(baseRadius, radiusMultiplier, radiusBonus) / baseRadius);
        }

        /// 대상 기준 영역 중심을 순서대로 만든다.
        public static List<Vector2> TargetAnchoredCenters(
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

            var orderedTargets = SkillTargeting.OrderedTargets(
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

        /// 선택 결과에 핵심 오브젝트가 포함됐는지 확인한다.
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

        /// 두 대상의 실행 우선순위를 비교한다.
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

            var leftHealthRatio = left.Model != null && left.Model.Resources != null && left.Model.Stats.MaxHealth > 0f
                ? leftHealth / left.Model.Stats.MaxHealth
                : 1f;
            var rightHealthRatio = right.Model != null && right.Model.Resources != null && right.Model.Stats.MaxHealth > 0f
                ? rightHealth / right.Model.Stats.MaxHealth
                : 1f;
            if (selection == SkillTargetSelection.LowestHealth && !Mathf.Approximately(leftHealthRatio, rightHealthRatio))
            {
                return leftHealthRatio.CompareTo(rightHealthRatio);
            }

            if (selection == SkillTargetSelection.HighestStacks)
            {
                var statusKind = StatusEffectKind.None;
                if (targetingSpec != null)
                {
                    statusKind = targetingSpec.SelectionStatusKind;
                }
                var leftStacks = StatusStacks(left.Model, statusKind);
                var rightStacks = StatusStacks(right.Model, statusKind);
                if (leftStacks != rightStacks)
                {
                    return rightStacks.CompareTo(leftStacks);
                }
            }

            var leftDistance = DistanceSquared(sourceEntry, left);
            var rightDistance = DistanceSquared(sourceEntry, right);
            if (selection == SkillTargetSelection.Farthest)
            {
                return rightDistance.CompareTo(leftDistance);
            }

            return leftDistance.CompareTo(rightDistance);
        }

        /// 두 항목 사이의 거리 제곱을 계산한다.
        private static float DistanceSquared(CombatUnitEntry sourceEntry, CombatUnitEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
        }

        /// 항목이 현재 스킬의 유효 대상인지 확인한다.
        private static bool IsSkillTargetable(CombatUnitEntry entry)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return identity == null || identity.Role != UnitRole.Nexus;
        }

        /// 대상이 요구 상태와 중첩을 충족하는지 확인한다.
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

        /// 대상의 상태 중첩 수를 읽는다.
        private static int StatusStacks(UnitCombatState model, StatusEffectKind kind)
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

        /// 연쇄 피해가 이어질 대상을 거리 순으로 고른다.
        public static List<CombatUnitEntry> ChainTargets(
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float searchRadius)
        {
            var resolved = new List<CombatUnitEntry>();
            if (roster == null || source == null || searchRadius <= 0f)
            {
                return resolved;
            }

            UnitCombatState hitModel = null;
            if (hitTarget != null)
            {
                hitModel = hitTarget.Model;
            }

            var hitId = string.Empty;
            if (hitModel != null && hitModel.Identity != null)
            {
                hitId = hitModel.Identity.UnitId;
            }

            var sourceSide = UnitSide.Player;
            if (source.Identity != null)
            {
                sourceSide = source.Identity.Side;
            }
            else if (sourceEntry != null && sourceEntry.Model != null && sourceEntry.Model.Identity != null)
            {
                sourceSide = sourceEntry.Model.Identity.Side;
            }

            IReadOnlyList<CombatUnitEntry> candidates = roster.Enemies;
            if (sourceSide == UnitSide.Enemy)
            {
                candidates = roster.Players;
            }

            var searchRadiusSquared = searchRadius * searchRadius;
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || candidate.Model == null || candidate.Transform == null)
                {
                    continue;
                }
                if (candidate.Model.Identity != null && candidate.Model.Identity.Role == UnitRole.Nexus)
                {
                    continue;
                }

                var candidateId = string.Empty;
                if (candidate.Model.Identity != null)
                {
                    candidateId = candidate.Model.Identity.UnitId;
                }
                if (!string.IsNullOrWhiteSpace(hitId) && candidateId == hitId)
                {
                    continue;
                }
                if (candidate.Model == hitModel)
                {
                    continue;
                }
                if (((Vector2)candidate.Transform.position - hitPosition).sqrMagnitude <= searchRadiusSquared)
                {
                    resolved.Add(candidate);
                }
            }

            resolved.Sort(delegate(CombatUnitEntry left, CombatUnitEntry right)
            {
                var leftDistance = ((Vector2)left.Transform.position - hitPosition).sqrMagnitude;
                var rightDistance = ((Vector2)right.Transform.position - hitPosition).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });
            return resolved;
        }

        /// 지원 효과가 닿을 대상을 모은다.
        internal static IReadOnlyList<CombatUnitEntry> BuffTargets(
            SkillExecutionContext context,
            SkillTargetSide targetMode,
            bool useConfiguredTargeting,
            SkillTargetingSpec targeting)
        {
            if (context == null)
            {
                return Array.Empty<CombatUnitEntry>();
            }
            if (!useConfiguredTargeting)
            {
                if (targetMode == SkillTargetSide.Self)
                {
                    return context.CasterEntry != null
                        ? new[] { context.CasterEntry }
                        : Array.Empty<CombatUnitEntry>();
                }
                return TargetList(
                    context.CasterEntry,
                    context.Roster,
                    new SkillTargetingSpec
                    {
                        TargetSide = SkillTargetSide.AllAllies,
                        Selection = SkillTargetSelection.Owner,
                        Shape = SkillTargetShape.Battlefield,
                        CoverAll = true
                    });
            }

            var targets = OrderedTargets(context, targeting);
            var caster = context.CasterEntry;
            if (caster == null
                || caster.Transform == null
                || targeting == null
                || targeting.Radius <= 0f)
            {
                return targets;
            }

            var radiusSquared = targeting.Radius * targeting.Radius;
            targets.RemoveAll(target =>
                target == null
                || target.Transform == null
                || ((Vector2)target.Transform.position
                    - (Vector2)caster.Transform.position).sqrMagnitude > radiusSquared);
            return targets;
        }
    }
}
