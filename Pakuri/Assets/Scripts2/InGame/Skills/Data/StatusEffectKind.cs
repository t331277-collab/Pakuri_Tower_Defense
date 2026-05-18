using System.Collections.Generic;

namespace Pakuri.InGame
{
    public enum StatusEffectKind
    {
        None,
        Shock,
        Chill,
        Freeze,
        Slow,
        Vulnerable,
        Shield
    }

    public readonly struct StatusEffectDefinition
    {
        public StatusEffectDefinition(
            StatusEffectKind kind,
            string id,
            string displayName,
            float defaultDurationSeconds,
            int defaultMaxStacks,
            bool permanent)
        {
            Kind = kind;
            Id = id;
            DisplayName = displayName;
            DefaultDurationSeconds = defaultDurationSeconds;
            DefaultMaxStacks = defaultMaxStacks;
            Permanent = permanent;
        }

        public StatusEffectKind Kind { get; }
        public string Id { get; }
        public string DisplayName { get; }
        public float DefaultDurationSeconds { get; }
        public int DefaultMaxStacks { get; }
        public bool Permanent { get; }
    }

    public static class StatusEffectUtility
    {
        private static readonly StatusEffectDefinition NoneDefinition =
            new StatusEffectDefinition(StatusEffectKind.None, string.Empty, string.Empty, 0f, 0, false);

        public static bool TryParse(string value, out StatusEffectKind kind)
        {
            kind = StatusEffectKind.None;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            switch (value.Trim().ToLowerInvariant())
            {
                case "shock":
                case "감전":
                    kind = StatusEffectKind.Shock;
                    return true;
                case "chill":
                case "추위":
                case "냉기":
                    kind = StatusEffectKind.Chill;
                    return true;
                case "freeze":
                case "빙결":
                    kind = StatusEffectKind.Freeze;
                    return true;
                case "slow":
                case "둔화":
                    kind = StatusEffectKind.Slow;
                    return true;
                case "vulnerable":
                case "취약":
                    kind = StatusEffectKind.Vulnerable;
                    return true;
                case "shield":
                case "방어막":
                    kind = StatusEffectKind.Shield;
                    return true;
                default:
                    return false;
            }
        }

        public static StatusEffectDefinition GetDefinition(StatusEffectKind kind)
        {
            switch (kind)
            {
                case StatusEffectKind.Shock:
                    return new StatusEffectDefinition(kind, "shock", "감전", 1.25f, 10, false);
                case StatusEffectKind.Chill:
                    return new StatusEffectDefinition(kind, "chill", "추위", 2.5f, 10, false);
                case StatusEffectKind.Freeze:
                    return new StatusEffectDefinition(kind, "freeze", "빙결", 0f, 0, false);
                case StatusEffectKind.Slow:
                    return new StatusEffectDefinition(kind, "slow", "둔화", 0f, 0, false);
                case StatusEffectKind.Vulnerable:
                    return new StatusEffectDefinition(kind, "vulnerable", "취약", 0f, 10, true);
                case StatusEffectKind.Shield:
                    return new StatusEffectDefinition(kind, "shield", "방어막", 0f, 0, false);
                default:
                    return NoneDefinition;
            }
        }

        public static string ToId(StatusEffectKind kind)
        {
            return GetDefinition(kind).Id;
        }

        public static string ToDisplayName(StatusEffectKind kind)
        {
            return GetDefinition(kind).DisplayName;
        }

        public static string BuildDisplaySuffix(IReadOnlyList<UnitStatusRuntime> statuses)
        {
            if (statuses == null || statuses.Count == 0)
            {
                return string.Empty;
            }

            var names = new List<string>();
            var addedKinds = new HashSet<StatusEffectKind>();
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status == null || status.Kind == StatusEffectKind.None || status.Stacks <= 0)
                {
                    continue;
                }

                var displayName = ToDisplayName(status.Kind);
                if (!string.IsNullOrWhiteSpace(displayName) && addedKinds.Add(status.Kind))
                {
                    names.Add($"{displayName} +{status.Stacks}");
                }
            }

            return names.Count > 0 ? $"[{string.Join("/", names)}]" : string.Empty;
        }
    }
}
