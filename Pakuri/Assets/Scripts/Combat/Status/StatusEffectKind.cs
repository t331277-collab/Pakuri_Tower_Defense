using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 상태 효과 종류에서 사용하는 선택 값을 정의한다.
 */
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
        PassiveBuff,
        SeinAHitMark,
        SeinDHeatStack,
        SeinDSuperheatedPresence
    }

    /*
     * 상태 효과 계산과 변환 기능을 제공한다.
     */
    public static class StatusEffectUtility
    {
        /*
         * 문자열을 상태 종류로 해석한다.
         */
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
                case "sein-a-hit-mark":
                    kind = StatusEffectKind.SeinAHitMark;
                    return true;
                case "sein-d-heat-stack":
                    kind = StatusEffectKind.SeinDHeatStack;
                    return true;
                case "sein-d-superheated-presence":
                    kind = StatusEffectKind.SeinDSuperheatedPresence;
                    return true;
                default:
                    return false;
            }
        }

        /*
         * 정의를 반환한다.
         */
        public static StatusEffectDefinition GetDefinition(StatusEffectKind kind)
        {
            var id = ToId(kind);
            var catalog = CsvDataLoader.CurrentCatalog;
            if (catalog != null
                && !string.IsNullOrWhiteSpace(id)
                && catalog.TryGetData(id, out StatusEffectDefinition definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Status definition '{id}' is not registered.");
        }

        /*
         * 상태 종류를 CSV와 런타임에서 사용하는 ID로 변환한다.
         */
        public static string ToId(StatusEffectKind kind)
        {
            switch (kind)
            {
                case StatusEffectKind.Shock: return "shock";
                case StatusEffectKind.Chill: return "chill";
                case StatusEffectKind.Freeze: return "freeze";
                case StatusEffectKind.Slow: return "slow";
                case StatusEffectKind.Vulnerable: return "vulnerable";
                case StatusEffectKind.FireResistDown: return "fire-resist-down";
                case StatusEffectKind.FireExposure: return "fire-exposure";
                case StatusEffectKind.Shield: return "shield";
                case StatusEffectKind.Blessing: return "blessing";
                case StatusEffectKind.HolyExposure: return "holy-exposure";
                case StatusEffectKind.HolyResistDown: return "holy-resist-down";
                case StatusEffectKind.NameMark: return "name-mark";
                case StatusEffectKind.Silence: return "silence";
                case StatusEffectKind.SlaughterPermit: return "slaughter-permit";
                case StatusEffectKind.ActionSpeedUp: return "action-speed-up";
                case StatusEffectKind.PassiveBuff: return "passive-buff";
                case StatusEffectKind.SeinAHitMark: return "sein-a-hit-mark";
                case StatusEffectKind.SeinDHeatStack: return "sein-d-heat-stack";
                case StatusEffectKind.SeinDSuperheatedPresence: return "sein-d-superheated-presence";
                default: return string.Empty;
            }
        }

        /*
         * 상태 종류를 화면에 표시할 이름으로 변환한다.
         */
        public static string ToDisplayName(StatusEffectKind kind)
        {
            return GetDefinition(kind).DisplayName;
        }

        /*
         * 상태 중첩과 남은 시간을 표시할 문자열을 구성한다.
         */
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

}
