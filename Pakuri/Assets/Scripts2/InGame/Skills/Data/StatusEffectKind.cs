using System.Collections.Generic;
using UnityEngine;

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
        FireResistDown,
        FireExposure,
        Shield,
        Blessing,
        HolyExposure,
        HolyResistDown,
        NameMark,
        Silence,
        SlaughterPermit,
        ActionSpeedUp,
        PassiveBuff
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
                case "fire-resist-down":
                case "화염 저항 감소":
                    kind = StatusEffectKind.FireResistDown;
                    return true;
                case "fire-exposure":
                case "화염 노출":
                    kind = StatusEffectKind.FireExposure;
                    return true;
                case "shield":
                case "holy-shield":
                case "신성 방어막":
                case "방어막":
                    kind = StatusEffectKind.Shield;
                    return true;
                case "blessing":
                case "축복":
                    kind = StatusEffectKind.Blessing;
                    return true;
                case "holy-exposure":
                case "신성 노출":
                    kind = StatusEffectKind.HolyExposure;
                    return true;
                case "holy-resist-down":
                case "신성 저항 감소":
                    kind = StatusEffectKind.HolyResistDown;
                    return true;
                case "name-mark":
                case "이름표식":
                case "이름표식 연계":
                    kind = StatusEffectKind.NameMark;
                    return true;
                case "silence":
                case "침묵":
                    kind = StatusEffectKind.Silence;
                    return true;
                case "slaughter-permit":
                case "몰살 허가":
                    kind = StatusEffectKind.SlaughterPermit;
                    return true;
                case "action-speed-up":
                case "행동속도 증가":
                case "행동속도":
                    kind = StatusEffectKind.ActionSpeedUp;
                    return true;
                case "passive-buff":
                case "passive":
                    kind = StatusEffectKind.PassiveBuff;
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
                    return new StatusEffectDefinition(kind, "shock", "감전", 4f, 5, false);
                case StatusEffectKind.Chill:
                    return new StatusEffectDefinition(kind, "chill", "추위", 3f, 5, false);
                case StatusEffectKind.Freeze:
                    return new StatusEffectDefinition(kind, "freeze", "빙결", 2f, 1, false);
                case StatusEffectKind.Slow:
                    return new StatusEffectDefinition(kind, "slow", "둔화", 3f, 5, false);
                case StatusEffectKind.Vulnerable:
                    return new StatusEffectDefinition(kind, "vulnerable", "취약", 0f, 10, true);
                case StatusEffectKind.FireResistDown:
                    return new StatusEffectDefinition(kind, "fire-resist-down", "화염 저항 감소", 0f, 0, false);
                case StatusEffectKind.FireExposure:
                    return new StatusEffectDefinition(kind, "fire-exposure", "화염 노출", 0f, 0, false);
                case StatusEffectKind.Shield:
                    return new StatusEffectDefinition(kind, "shield", "신성 방어막", 0f, 0, false);
                case StatusEffectKind.Blessing:
                    return new StatusEffectDefinition(kind, "blessing", "축복", 0f, 1, false);
                case StatusEffectKind.HolyExposure:
                    return new StatusEffectDefinition(kind, "holy-exposure", "신성 노출", 0f, 0, false);
                case StatusEffectKind.HolyResistDown:
                    return new StatusEffectDefinition(kind, "holy-resist-down", "신성 저항 감소", 0f, 0, false);
                case StatusEffectKind.NameMark:
                    return new StatusEffectDefinition(kind, "name-mark", "이름표식", 0f, 0, true);
                case StatusEffectKind.Silence:
                    return new StatusEffectDefinition(kind, "silence", "침묵", 0f, 1, false);
                case StatusEffectKind.SlaughterPermit:
                    return new StatusEffectDefinition(kind, "slaughter-permit", "몰살 허가", 6f, 1, false);
                case StatusEffectKind.ActionSpeedUp:
                    return new StatusEffectDefinition(kind, "action-speed-up", "행동속도 증가", 6f, 1, false);
                case StatusEffectKind.PassiveBuff:
                    return new StatusEffectDefinition(kind, "passive-buff", "Passive Buff", 0f, 1, false);
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
            var totals = new Dictionary<StatusEffectKind, int>();
            var labels = new Dictionary<StatusEffectKind, string>();
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status == null || status.Kind == StatusEffectKind.None || status.Stacks <= 0)
                {
                    continue;
                }

                var displayName = status.DisplayName;
                if (string.IsNullOrWhiteSpace(displayName))
                {
                    displayName = ToDisplayName(status.Kind);
                }

                if (string.IsNullOrWhiteSpace(displayName))
                {
                    continue;
                }

                totals[status.Kind] = totals.TryGetValue(status.Kind, out var currentStacks)
                    ? currentStacks + status.Stacks
                    : status.Stacks;
                labels[status.Kind] = displayName;
            }

            foreach (var pair in totals)
            {
                if (labels.TryGetValue(pair.Key, out var displayName))
                {
                    names.Add($"{displayName} +{pair.Value}");
                }
            }

            return names.Count > 0 ? $"[{string.Join("/", names)}]" : string.Empty;
        }
    }

    [CreateAssetMenu(menuName = "Pakuri/InGame/Status Effect Data", fileName = "StatusEffectData")]
    public sealed class StatusEffectData : ScriptableObject
    {
        [Header("Identity")]
        public StatusEffectKind Kind = StatusEffectKind.None;
        public string StatusTag;
        public string StatusName;
        public string SourceSkillId;
        public StatusTargetScope TargetScope = StatusTargetScope.Unspecified;
        public StatusMergePolicy MergePolicy = StatusMergePolicy.Unspecified;
        public ShieldRefreshRule ShieldAmountRefreshPolicy = ShieldRefreshRule.TakeHighest;

        [Header("Stacking")]
        public bool IsStackable;
        public int MaxStacks;
        public float Duration;
        public bool Permanent;
        public int BaseStackAmount = 1;

        [Header("Action Rules")]
        public bool CanMove = true;
        public bool CanAct = true;
        public bool CanUseSpecialSkill = true;

        [Header("Effect")]
        public float TickDamageBase;
        public float MovementSlowRate;
        public float MoveSpeedBonus;
        public float CriticalDamageTakenBonus;
        public float AilmentResistanceBonus;
        public float CriticalResistanceBonus;
        public float DamageTakenBonus;
        public float ElementResistReduction;
        public float FlatElementResistReduction;
        public float ElementDamageTakenBonus;
        public ElementType ElementModifierTarget;
        public bool HasElementModifierTarget;
        public bool IsControlEffect;
        public GameObject StatusEffectPrefab;
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Conditional Conversion")]
        public string TriggerConditionTag;
        public int TriggerConditionStacks;

        [Header("Conditional Incoming Damage")]
        public string ConditionalSourceStatusTag;
        public float ConditionalDamageTakenBonus;
    }
}
