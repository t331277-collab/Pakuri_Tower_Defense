using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 스킬의 진영·거리·체력·상태·투사체 교차 규칙으로 대상을 선정한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public readonly struct CombatFootprint
    {
        /* 중심 기준 반너비·반높이로 축 정렬 전투 범위를 구성한다. */
        public CombatFootprint(float halfWidth, float halfHeight)
            : this(default, halfWidth, halfHeight)
        {
        }

        /* 중심 offset과 반너비·반높이로 축 정렬 전투 범위를 구성한다. */
        public CombatFootprint(
            CombatVector2 centerOffset,
            float halfWidth,
            float halfHeight)
        {
            if (halfWidth < 0f
                || halfHeight < 0f
                || float.IsNaN(halfWidth)
                || float.IsNaN(halfHeight)
                || float.IsInfinity(halfWidth)
                || float.IsInfinity(halfHeight))
            {
                throw new ArgumentOutOfRangeException(nameof(halfWidth));
            }

            CenterOffset = centerOffset;
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
        }

        public CombatVector2 CenterOffset { get; }

        public float HalfWidth { get; }

        public float HalfHeight { get; }
    }

    public readonly struct ProjectileIntersection
    {
        /* 투사체 선분이 유닛 범위에 진입한 비율과 교차 위치를 묶는다. */
        public ProjectileIntersection(
            UnitBaseModel target,
            float segmentFraction,
            CombatVector2 position)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            SegmentFraction = segmentFraction;
            Position = position;
        }

        public UnitBaseModel Target { get; }

        public float SegmentFraction { get; }

        public CombatVector2 Position { get; }
    }

    public sealed class SkillTargeting
    {
        private readonly Func<int, int> randomIndex;
        private readonly Func<UnitBaseModel, CombatFootprint> footprintResolver;

        /* 무작위 index 공급원과 선택적 유닛 footprint 해석기를 저장한다. */
        public SkillTargeting(
            Func<int, int> randomIndex,
            Func<UnitBaseModel, CombatFootprint> footprintResolver = null)
        {
            this.randomIndex = randomIndex ?? throw new ArgumentNullException(nameof(randomIndex));
            this.footprintResolver =
                footprintResolver ?? (_ => default);
        }

        /* 스킬 기본 최대 대상 수로 등록 유닛에서 대상을 선정한다. */
        public IReadOnlyList<UnitBaseModel> Resolve(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2? manualTargetPoint = null)
        {
            return Resolve(
                source,
                skill,
                registeredUnits,
                manualTargetPoint,
                0);
        }

        /* 추가 최대 대상 수를 반영해 등록 유닛에서 대상을 선정한다. */
        public IReadOnlyList<UnitBaseModel> Resolve(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2? manualTargetPoint,
            int hitTargetCountBonus)
        {
            if (source == null || skill == null || registeredUnits == null)
            {
                throw new ArgumentNullException(
                    source == null
                        ? nameof(source)
                        : skill == null ? nameof(skill) : nameof(registeredUnits));
            }

            string selection = ReadString(skill, "target_selection");
            List<UnitBaseModel> candidates = BuildCandidates(source, skill, registeredUnits);
            FilterBySelectionStatus(candidates, skill, selection);
            FilterUnavailableSupportTargets(candidates, skill, selection);
            if (string.Equals(selection, "Self", StringComparison.Ordinal))
            {
                return new[] { source };
            }

            if (manualTargetPoint.HasValue
                && string.IsNullOrEmpty(selection))
            {
                StableSort(candidates, (left, right) =>
                    CompareDistance(manualTargetPoint.Value, left, right));
            }
            else
            {
                Sort(source, skill, candidates, selection);
            }

            if (string.Equals(selection, "RandomHostile", StringComparison.Ordinal)
                || string.Equals(selection, "Random", StringComparison.Ordinal))
            {
                return candidates.Count == 0
                    ? Array.Empty<UnitBaseModel>()
                    : new[] { candidates[ResolveRandomIndex(candidates.Count)] };
            }

            bool all = string.Equals(selection, "AllHostiles", StringComparison.Ordinal)
                || string.Equals(selection, "AllFriendlies", StringComparison.Ordinal)
                || string.Equals(ReadString(skill, "target_scope"), "HostileAll", StringComparison.Ordinal)
                || string.Equals(ReadString(skill, "target_scope"), "FriendlyInRadius", StringComparison.Ordinal)
                || string.Equals(
                    ReadString(skill, "status_target_scope"),
                    "all_allies",
                    StringComparison.OrdinalIgnoreCase);
            int maximum = all
                ? candidates.Count
                : Math.Max(
                    0,
                    ResolveHitTargetCount(skill) + hitTargetCountBonus);
            if (maximum <= 0 || candidates.Count == 0)
            {
                return Array.Empty<UnitBaseModel>();
            }

            if (maximum >= candidates.Count)
            {
                return candidates.AsReadOnly();
            }

            return candidates.GetRange(0, maximum).AsReadOnly();
        }

        /* 스킬 대상 선정 조건에 맞는 대상을 탐색해 반환한다. */
        public UnitBaseModel FindNearestLiving(
            UnitBaseModel source,
            IReadOnlyList<UnitBaseModel> candidates,
            bool includeNexus)
        {
            if (source == null || candidates == null)
            {
                throw new ArgumentNullException(source == null ? nameof(source) : nameof(candidates));
            }

            UnitBaseModel nearest = null;
            float nearestDistance = float.MaxValue;
            for (int index = 0; index < candidates.Count; index++)
            {
                UnitBaseModel candidate = candidates[index];
                if (candidate == null
                    || !candidate.IsAlive
                    || ReferenceEquals(candidate, source)
                    || (!includeNexus && candidate is NexusModel))
                {
                    continue;
                }

                float distance = (candidate.Position - source.Position).SqrMagnitude;
                if (distance < nearestDistance)
                {
                    nearest = candidate;
                    nearestDistance = distance;
                }
            }

            return nearest;
        }

        /* 조건에 맞는 모든 생존 후보를 선택 규칙에 따른 안정 순서로 반환한다. */
        public IReadOnlyList<UnitBaseModel> ResolveOrderedAll(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2? manualTargetPoint = null)
        {
            List<UnitBaseModel> candidates = BuildCandidates(source, skill, registeredUnits);
            string selection = ReadString(skill, "target_selection");
            FilterBySelectionStatus(candidates, skill, selection);
            FilterUnavailableSupportTargets(candidates, skill, selection);
            if (manualTargetPoint.HasValue
                && string.IsNullOrEmpty(selection))
            {
                StableSort(candidates, (left, right) =>
                    CompareDistance(manualTargetPoint.Value, left, right));
            }
            else
            {
                Sort(source, skill, candidates, selection);
            }
            return candidates.AsReadOnly();
        }

        /* 중심점 반경 안의 생존 유닛을 입력 순서대로 반환한다. */
        public IReadOnlyList<UnitBaseModel> InRadius(
            IReadOnlyList<UnitBaseModel> candidates,
            CombatVector2 center,
            float radius)
        {
            if (candidates == null)
            {
                throw new ArgumentNullException(nameof(candidates));
            }

            if (radius < 0f || float.IsNaN(radius) || float.IsInfinity(radius))
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            float radiusSquared = radius * radius;
            List<UnitBaseModel> result = new List<UnitBaseModel>();
            for (int index = 0; index < candidates.Count; index++)
            {
                UnitBaseModel candidate = candidates[index];
                if (candidate != null
                    && candidate.IsAlive
                    && !(candidate is NexusModel)
                    && (candidate.Position - center).SqrMagnitude <= radiusSquared)
                {
                    result.Add(candidate);
                }
            }

            return result.AsReadOnly();
        }

        /* 투사체 선분과 후보 유닛 범위의 교차점을 거리 순으로 반환한다. */
        public IReadOnlyList<ProjectileIntersection> ResolveProjectileIntersections(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2 segmentStart,
            CombatVector2 segmentEnd,
            ISet<UnitBaseModel> alreadyHit)
        {
            if (source == null || skill == null || registeredUnits == null)
            {
                throw new ArgumentNullException(
                    source == null
                        ? nameof(source)
                        : skill == null ? nameof(skill) : nameof(registeredUnits));
            }

            float projectileHalfWidth = Math.Max(
                0f,
                ReadFloat(skill, "runtime_hitbox_size_x") * 0.5f);
            float projectileHalfHeight = Math.Max(
                0f,
                ReadFloat(skill, "runtime_hitbox_size_y") * 0.5f);
            List<UnitBaseModel> candidates =
                BuildCandidates(source, skill, registeredUnits);
            List<ProjectileIntersection> intersections =
                new List<ProjectileIntersection>();
            for (int index = 0; index < candidates.Count; index++)
            {
                UnitBaseModel candidate = candidates[index];
                if (alreadyHit != null && alreadyHit.Contains(candidate))
                {
                    continue;
                }

                CombatFootprint footprint = footprintResolver(candidate);
                if (!TryIntersectSegmentBounds(
                        segmentStart,
                        segmentEnd,
                        candidate.Position + footprint.CenterOffset,
                        footprint.HalfWidth + projectileHalfWidth,
                        footprint.HalfHeight + projectileHalfHeight,
                        out float fraction))
                {
                    continue;
                }

                intersections.Add(new ProjectileIntersection(
                    candidate,
                    fraction,
                    segmentStart + ((segmentEnd - segmentStart) * fraction)));
            }

            StableSort(intersections, (left, right) =>
                left.SegmentFraction.CompareTo(right.SegmentFraction));
            return intersections.AsReadOnly();
        }

        /* target scope와 진영 규칙에 맞는 생존 유닛 후보 목록을 구성한다. */
        private static List<UnitBaseModel> BuildCandidates(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> units)
        {
            string scope = ReadString(skill, "target_scope");
            string statusScope =
                ReadString(skill, "status_target_scope");
            bool friendly = string.Equals(scope, "Friendly", StringComparison.Ordinal)
                || string.Equals(scope, "FriendlyInRadius", StringComparison.Ordinal)
                || string.Equals(scope, "Self", StringComparison.Ordinal)
                || string.Equals(
                    statusScope,
                    "all_allies",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    statusScope,
                    "self",
                    StringComparison.OrdinalIgnoreCase)
                || string.Equals(ReadString(skill, "target_selection"), "AllFriendlies", StringComparison.Ordinal)
                || string.Equals(ReadString(skill, "target_selection"), "LowestHealthFriendly", StringComparison.Ordinal);
            if (string.Equals(scope, "Self", StringComparison.Ordinal)
                || string.Equals(
                    statusScope,
                    "self",
                    StringComparison.OrdinalIgnoreCase))
            {
                return new List<UnitBaseModel> { source };
            }

            bool sourceIsEnemy = source is EnemyModel;
            List<UnitBaseModel> result = new List<UnitBaseModel>();
            for (int index = 0; index < units.Count; index++)
            {
                UnitBaseModel candidate = units[index];
                if (candidate == null || !candidate.IsAlive || candidate is NexusModel)
                {
                    continue;
                }

                bool sameSide = (candidate is EnemyModel) == sourceIsEnemy;
                if (friendly == sameSide)
                {
                    result.Add(candidate);
                }
            }

            return result;
        }

        /* 대상 선택 규칙에 따라 후보 목록을 안정적으로 정렬한다. */
        private static void Sort(
            UnitBaseModel source,
            SkillDefinition skill,
            List<UnitBaseModel> candidates,
            string selection)
        {
            if (string.Equals(selection, "HighestHealth", StringComparison.Ordinal))
            {
                StableSort(candidates, (left, right) =>
                    CompareWithDistance(
                        source,
                        right.CurrentHealth.CompareTo(left.CurrentHealth),
                        left,
                        right));
                return;
            }

            if (string.Equals(selection, "LowestHealth", StringComparison.Ordinal)
                || string.Equals(selection, "LowestHealthFriendly", StringComparison.Ordinal))
            {
                StableSort(candidates, (left, right) =>
                    CompareWithDistance(
                        source,
                        CompareHealthRatio(left, right),
                        left,
                        right));
                return;
            }

            if (string.Equals(selection, "HighestStacks", StringComparison.Ordinal))
            {
                StableSort(candidates, (left, right) =>
                    CompareWithDistance(
                        source,
                        CountStacks(
                            right,
                            ReadString(skill, "target_selection_status_id"))
                            .CompareTo(
                                CountStacks(
                                    left,
                                    ReadString(
                                        skill,
                                        "target_selection_status_id"))),
                        left,
                        right));
                return;
            }

            bool farthest = string.Equals(selection, "Farthest", StringComparison.Ordinal)
                || string.Equals(selection, "FarthestHostile", StringComparison.Ordinal);
            StableSort(candidates, (left, right) =>
            {
                float leftDistance = (left.Position - source.Position).SqrMagnitude;
                float rightDistance = (right.Position - source.Position).SqrMagnitude;
                return farthest
                    ? rightDistance.CompareTo(leftDistance)
                    : leftDistance.CompareTo(rightDistance);
            });
        }

        /* 동률의 원래 순서를 보존하며 비교 함수 기준으로 목록을 정렬한다. */
        private static void StableSort<T>(
            List<T> values,
            Comparison<T> comparison)
        {
            for (int index = 1; index < values.Count; index++)
            {
                T value = values[index];
                int insertion = index;
                while (insertion > 0
                    && comparison(values[insertion - 1], value) > 0)
                {
                    values[insertion] = values[insertion - 1];
                    insertion--;
                }
                values[insertion] = value;
            }
        }

        /* 선택 규칙 비교가 동률이면 시전자와의 거리로 우선순위를 정한다. */
        private static int CompareWithDistance(
            UnitBaseModel source,
            int primary,
            UnitBaseModel left,
            UnitBaseModel right)
        {
            return primary != 0
                ? primary
                : (left.Position - source.Position).SqrMagnitude.CompareTo(
                    (right.Position - source.Position).SqrMagnitude);
        }

        /* 두 후보의 시전자 기준 제곱거리를 비교한다. */
        private static int CompareDistance(
            CombatVector2 center,
            UnitBaseModel left,
            UnitBaseModel right)
        {
            return (left.Position - center).SqrMagnitude.CompareTo(
                (right.Position - center).SqrMagnitude);
        }

        /* 스킬의 선택 상태 조건과 일치하지 않는 후보를 제거한다. */
        private static void FilterBySelectionStatus(
            List<UnitBaseModel> candidates,
            SkillDefinition skill,
            string selection)
        {
            if (!string.Equals(selection, "HighestStacks", StringComparison.Ordinal))
            {
                return;
            }

            string statusId = ReadString(skill, "target_selection_status_id");
            int minimum = Math.Max(
                0,
                ReadInt(skill, "target_selection_status_min_stacks"));
            if (string.IsNullOrEmpty(statusId) || minimum <= 0)
            {
                return;
            }

            for (int index = candidates.Count - 1; index >= 0; index--)
            {
                if (CountStacks(candidates[index], statusId) < minimum)
                {
                    candidates.RemoveAt(index);
                }
            }
        }

        /* 체력이 가득 찬 유닛을 최저 체력 회복 대상 후보에서 제거한다. */
        private static void FilterUnavailableSupportTargets(
            List<UnitBaseModel> candidates,
            SkillDefinition skill,
            string selection)
        {
            if (!(skill is HealDefinition)
                || !string.Equals(
                    selection,
                    "LowestHealthFriendly",
                    StringComparison.Ordinal))
            {
                return;
            }

            for (int index = candidates.Count - 1; index >= 0; index--)
            {
                if (candidates[index].CurrentHealth
                    >= candidates[index].MaximumHealth)
                {
                    candidates.RemoveAt(index);
                }
            }
        }

        /* 두 후보의 현재 체력 비율을 비교한다. */
        private static int CompareHealthRatio(
            UnitBaseModel left,
            UnitBaseModel right)
        {
            float leftRatio = left.MaximumHealth <= 0f
                ? 1f
                : left.CurrentHealth / left.MaximumHealth;
            float rightRatio = right.MaximumHealth <= 0f
                ? 1f
                : right.CurrentHealth / right.MaximumHealth;
            return leftRatio.CompareTo(rightRatio);
        }

        /* 선분과 축 정렬 범위의 교차 여부와 최초 진입 비율을 계산한다. */
        private static bool TryIntersectSegmentBounds(
            CombatVector2 start,
            CombatVector2 end,
            CombatVector2 center,
            float halfWidth,
            float halfHeight,
            out float fraction)
        {
            CombatVector2 delta = end - start;
            float minimum = 0f;
            float maximum = 1f;
            if (!ClipAxis(
                    start.X,
                    delta.X,
                    center.X - halfWidth,
                    center.X + halfWidth,
                    ref minimum,
                    ref maximum)
                || !ClipAxis(
                    start.Y,
                    delta.Y,
                    center.Y - halfHeight,
                    center.Y + halfHeight,
                    ref minimum,
                    ref maximum))
            {
                fraction = 0f;
                return false;
            }

            fraction = minimum;
            return true;
        }

        /* 선분과 축 정렬 범위의 한 축 교차 구간을 clipping한다. */
        private static bool ClipAxis(
            float origin,
            float delta,
            float minimumBound,
            float maximumBound,
            ref float minimum,
            ref float maximum)
        {
            if (Math.Abs(delta) <= 0.00001f)
            {
                return origin >= minimumBound && origin <= maximumBound;
            }

            float first = (minimumBound - origin) / delta;
            float second = (maximumBound - origin) / delta;
            if (first > second)
            {
                float swap = first;
                first = second;
                second = swap;
            }

            minimum = Math.Max(minimum, first);
            maximum = Math.Min(maximum, second);
            return minimum <= maximum;
        }

        /* 유닛에서 지정 status id 또는 전체 상태의 스택 합계를 계산한다. */
        private static int CountStacks(UnitBaseModel unit, string statusId)
        {
            int stacks = 0;
            for (int index = 0; index < unit.StatusEffects.Count; index++)
            {
                if (string.IsNullOrEmpty(statusId)
                    || string.Equals(
                        unit.StatusEffects[index].Definition.status_effect_id,
                        statusId,
                        StringComparison.Ordinal))
                {
                    stacks += unit.StatusEffects[index].CurrentStacks;
                }
            }

            return stacks;
        }

        /* 난수 공급원이 반환한 index가 후보 범위 안인지 검증한다. */
        private int ResolveRandomIndex(int count)
        {
            int index = randomIndex(count);
            if (index < 0 || index >= count)
            {
                throw new InvalidOperationException("The random index source returned an invalid index.");
            }

            return index;
        }

        /* 스킬 적중 수 열을 숫자·All/global 규칙으로 해석하고 미지정 시 1을 반환한다. */
        private static int ResolveHitTargetCount(SkillDefinition skill)
        {
            if (!skill.Columns.TryGetValue("hit_target_count", out object value)
                || !(value is string text)
                || string.IsNullOrEmpty(text))
            {
                return 1;
            }

            if (string.Equals(text, "All", StringComparison.OrdinalIgnoreCase)
                || string.Equals(
                    text,
                    "global",
                    StringComparison.OrdinalIgnoreCase))
            {
                return int.MaxValue;
            }
            return int.TryParse(
                text,
                System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture,
                out int parsed)
                    ? parsed
                    : 1;
        }

        /* 스킬 정의의 지정 열이 float면 반환하고 아니면 0을 반환한다. */
        internal static float ReadFloat(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) && value is float number
                ? number
                : 0f;
        }

        /* 스킬 정의의 지정 열이 int면 반환하고 아니면 0을 반환한다. */
        internal static int ReadInt(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) && value is int number
                ? number
                : 0;
        }

        /* 스킬 정의의 지정 열이 문자열이면 반환하고 아니면 null을 반환한다. */
        internal static string ReadString(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) ? value as string : null;
        }
    }
}
