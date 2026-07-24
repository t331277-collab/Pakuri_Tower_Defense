using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public sealed class SkillTargeting
    {
        private readonly Func<int, int> randomIndex;

        public SkillTargeting(Func<int, int> randomIndex)
        {
            this.randomIndex = randomIndex ?? throw new ArgumentNullException(nameof(randomIndex));
        }

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
            if (string.Equals(selection, "Self", StringComparison.Ordinal))
            {
                return new[] { source };
            }

            if (manualTargetPoint.HasValue)
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
                || string.Equals(ReadString(skill, "target_scope"), "FriendlyInRadius", StringComparison.Ordinal);
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

        public IReadOnlyList<UnitBaseModel> ResolveOrderedAll(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> registeredUnits,
            CombatVector2? manualTargetPoint = null)
        {
            List<UnitBaseModel> candidates = BuildCandidates(source, skill, registeredUnits);
            string selection = ReadString(skill, "target_selection");
            FilterBySelectionStatus(candidates, skill, selection);
            if (manualTargetPoint.HasValue)
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

        private static List<UnitBaseModel> BuildCandidates(
            UnitBaseModel source,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> units)
        {
            string scope = ReadString(skill, "target_scope");
            bool friendly = string.Equals(scope, "Friendly", StringComparison.Ordinal)
                || string.Equals(scope, "FriendlyInRadius", StringComparison.Ordinal)
                || string.Equals(scope, "Self", StringComparison.Ordinal)
                || string.Equals(ReadString(skill, "target_selection"), "AllFriendlies", StringComparison.Ordinal)
                || string.Equals(ReadString(skill, "target_selection"), "LowestHealthFriendly", StringComparison.Ordinal);
            if (string.Equals(scope, "Self", StringComparison.Ordinal))
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
                        left.CurrentHealth.CompareTo(right.CurrentHealth),
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

        private static int CompareDistance(
            CombatVector2 center,
            UnitBaseModel left,
            UnitBaseModel right)
        {
            return (left.Position - center).SqrMagnitude.CompareTo(
                (right.Position - center).SqrMagnitude);
        }

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

        private int ResolveRandomIndex(int count)
        {
            int index = randomIndex(count);
            if (index < 0 || index >= count)
            {
                throw new InvalidOperationException("The random index source returned an invalid index.");
            }

            return index;
        }

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

        internal static float ReadFloat(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) && value is float number
                ? number
                : 0f;
        }

        internal static int ReadInt(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) && value is int number
                ? number
                : 0;
        }

        internal static string ReadString(SkillDefinition skill, string column)
        {
            return skill.Columns.TryGetValue(column, out object value) ? value as string : null;
        }
    }
}
