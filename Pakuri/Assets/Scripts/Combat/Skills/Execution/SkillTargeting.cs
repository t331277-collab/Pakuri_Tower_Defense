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
         * OrderedTargets 결과를 계산해 반환한다.
         */
        public static List<CombatUnitEntry> OrderedTargets(
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */)
        {
            // 시전자와 Targeting 설정을 기준으로 유효 대상을 정렬하는 부분을 구현.
            return OrderedTargets(sourceEntry, unitRoster, targetingSpec, StatusEffectKind.None, 0);
        }

        /*
         * OrderedTargets 결과를 계산해 반환한다.
         */
        public static List<CombatUnitEntry> OrderedTargets(
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            CombatUnitRegistry unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            StatusEffectKind requiredStatusKind /* 필수 상태 효과 종류 여부 */,
            int requiredStatusMinStacks /* 필수 상태 효과 최소 중첩 수 여부 */)
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
        /*
         * 가장 가까운 대상을 찾는다.
         */
        public static CombatUnitEntry FindNearestTarget(
            CombatUnitEntry caster /* 스킬을 사용하는 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */)
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

        /*
         * 대상을 방향을 계산한다.
         */
        public static Vector2 DirectionToTarget(Vector3 origin /* 시작 위치 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */)
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
        public static IReadOnlyList<CombatUnitEntry> TargetList(
            CombatUnitEntry caster /* 스킬을 사용하는 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */)
        {
            return TargetList(caster, roster, targeting, StatusEffectKind.None, 0);
        }

        /*
         * 대상 목록을 결정한다.
         */
        public static IReadOnlyList<CombatUnitEntry> TargetList(
            CombatUnitEntry caster /* 스킬을 사용하는 유닛 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            StatusEffectKind requiredStatusKind /* 필수 상태 효과 종류 여부 */,
            int requiredStatusMinStacks /* 필수 상태 효과 최소 중첩 수 여부 */)
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
        public static Vector2 AreaCenter(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            AreaBlueprintSpec area /* 범위 */)
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

        /*
         * 대상 설정에 기록된 기본 반경을 결정한다.
         */
        public static float BaseRadius(SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */, AreaBlueprintSpec area /* 범위 */)
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
        public static float Radius(
            float baseRadius /* 기본 반지름 */,
            float radiusMultiplier /* 적용할 반지름 배율 */,
            float radiusBonus /* 추가할 반지름 */)
        {
            var radius = baseRadius * Mathf.Max(0f, radiusMultiplier) + radiusBonus;
            return Mathf.Max(0f, radius);
        }

        /*
         * 반경 변화에 맞는 프리팹 크기 배율을 결정한다.
         */
        public static float PrefabScaleFactor(
            float baseRadius /* 기본 반지름 */,
            float radiusMultiplier /* 적용할 반지름 배율 */,
            float radiusBonus /* 추가할 반지름 */)
        {
            if (baseRadius <= 0.0001f)
            {
                return Mathf.Max(0.01f, radiusMultiplier);
            }

            return Mathf.Max(0.01f, Radius(baseRadius, radiusMultiplier, radiusBonus) / baseRadius);
        }

        /*
         * 반복 배치 횟수만큼 서로 다른 대상 위치를 우선 선택한다.
         */
        public static List<Vector2> TargetAnchoredCenters(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillTargetingSpec targeting /* 스킬 대상 선택 규칙 */,
            Vector2 primaryCenter /* 주 대상 중심 위치 */,
            int deploymentCount /* 배치 개수 */,
            bool coverAll /* 범위 안의 모든 대상 포함 여부 */,
            SkillDeploymentRepeatMode repeatMode /* 반복 방식 */)
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

        /*
         * 넥서스 대상을 포함하는지 확인한다.
         */
        private static bool ContainsNexusTarget(IReadOnlyList<CombatUnitEntry> targets /* 처리할 대상 목록 */)
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
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            SkillTargetSelection selection /* 선택 방식 */,
            CombatUnitEntry left /* 왼쪽 */,
            CombatUnitEntry right /* 오른쪽 */)
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

        /*
         * DistanceSquared 결과를 계산해 반환한다.
         */
        private static float DistanceSquared(CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */, CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */)
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
        private static bool IsSkillTargetable(CombatUnitEntry entry /* 처리할 등록 정보 */)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return identity == null || identity.Role != UnitRole.Nexus;
        }

        /*
         * 필수 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasRequiredStatus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, StatusEffectKind kind /* 처리할 종류 */, int minimumStacks /* 최소 중첩 수 */)
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
        private static int StatusStacks(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, StatusEffectKind kind /* 처리할 종류 */)
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

        /*
         * 추가 효과 정의를 실행용 대상 설정으로 변환한다.
         */

        /*
         * 추가 효과가 적용될 중심 위치를 결정한다.
         */

        /*
         * 사건 대상이 지정되면 그 대상만 반환하고 아니면 일반 대상 목록을 반환한다.
         */

        /*
         * 대상이 추가 효과의 상태, 속성, 체력 조건을 만족하는지 확인한다.
         */

        /*
         * 대상이 지정한 속성의 액티브 스킬을 가지고 있는지 확인한다.
         */
        private static bool HasActiveSkillAttribute(UnitCombatState target /* 검사할 대상 */, string rawAttribute /* 피해 속성 이름 */)
        {
            if (target == null || target.Skills == null || string.IsNullOrWhiteSpace(rawAttribute))
            {
                return false;
            }

            DamageAttribute attribute;
            if (!Enum.TryParse(rawAttribute.Trim(), true, out attribute))
            {
                return false;
            }

            var activeSkills = target.SkillState.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var activeSkill = activeSkills[i];
                if (activeSkill != null && activeSkill.Data != null && activeSkill.Data.Element == attribute)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 최초 적중 대상과 Nexus를 제외한 반대 진영 유닛을 거리순으로 찾는다.
         */
        public static List<CombatUnitEntry> ChainTargets(
            CombatUnitRegistry roster /* 전투 유닛 목록 */,
            CombatUnitEntry sourceEntry /* 시전자 등록 정보 */,
            UnitCombatState source /* 시전자 */,
            CombatUnitEntry hitTarget /* 최초 적중 대상 */,
            Vector2 hitPosition /* 최초 적중 위치 */,
            float searchRadius /* 검색 반지름 */)
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
    }
}

/*
 * 스킬 실행 전에 필요한 Choice, 패시브, 시전자 상태 조건을 판정한다.
 * SkillExecution과 각 Executor가 같은 사전 조건 판정을 공유하도록 한다.
 */
namespace Pakuri.InGame
{
    static class SkillRequirement
    {
        /*
         * 추가 효과에 설정된 Choice, 패시브, 시전자 상태 조건을 확인한다.
         */

        /*
         * 현재 적중 횟수가 추가 효과의 최소 횟수를 만족하는지 확인한다.
         */

        /*
         * 목록에 적힌 모든 Choice가 현재 실행 데이터에 적용되었는지 확인한다.
         */
        public static bool HasAllActiveChoices(UnitCombatState owner /* 스킬을 사용하는 유닛 */, string choiceList /* 선택지 목록 */)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (owner == null || owner.Skills == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choiceId = choices[i].Trim();
                if (choiceId.Length > 0 && !owner.Skills.HasChoice(choiceId))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * 목록에 적힌 Choice 중 하나라도 현재 실행 데이터에 적용되었는지 확인한다.
         */
        public static bool HasAnyActiveChoice(UnitCombatState owner /* 스킬을 사용하는 유닛 */, string choiceList /* 선택지 목록 */)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || owner == null || owner.Skills == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choiceId = choices[i].Trim();
                if (choiceId.Length > 0 && owner.Skills.HasChoice(choiceId))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 목록에 적힌 모든 패시브를 유닛이 학습했는지 확인한다.
         */
        public static bool HasAllLearnedPassives(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveList /* 패시브 목록 */)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i].Trim();
                if (passiveId.Length > 0 && !HasLearnedPassive(owner, passiveId))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * 목록에 적힌 패시브 중 하나라도 유닛이 학습했는지 확인한다.
         */
        public static bool HasAnyLearnedPassive(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveList /* 패시브 목록 */)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i].Trim();
                if (passiveId.Length > 0 && HasLearnedPassive(owner, passiveId))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 시전자가 지정한 상태 또는 보호막 조건을 만족하는지 확인한다.
         */
        public static bool HasSourceStatus(UnitCombatState owner /* 정보를 소유한 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minimumStacks /* 최소 중첩 수 */)
        {
            if (statusKind == StatusEffectKind.None)
            {
                return true;
            }

            if (statusKind == StatusEffectKind.Shield)
            {
                return owner != null && owner.Resources != null && owner.Resources.CurrentShield > 0f;
            }

            return owner != null
                && owner.Statuses != null
                && owner.Statuses.GetStacks(statusKind) >= Mathf.Max(1, minimumStacks);
        }

        /*
         * 유닛의 학습한 패시브 목록에 지정한 ID가 있는지 확인한다.
         */
        private static bool HasLearnedPassive(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveId /* 패시브 식별자 */)
        {
            return owner != null
                && owner.Skills != null
                && owner.Skills.HasPassiveSkill(passiveId);
        }
    }
}
