using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

/*
 * CSV 상태 정의와 전투에서 사용하는 상태 종류 및 실행 데이터를 정의한다.
 */
namespace Pakuri.Data
{
    public enum StatusEffectClassification
    {
        Buff,
        Debuff
    }

    [Serializable]
    /*
     * 상태 효과의 지속시간, 중첩, 행동 제한, 능력치 변경값을 보관한다.
     */
    public class StatusEffectDefinition
    {
        // 상태 식별과 기본 적용 규칙
        public string StatusEffectId;
        public string StatusEffectLabel;
        public StatusEffectClassification Classification;
        public bool HasAttribute;
        public DamageAttribute Attribute;
        public float DefaultDurationSeconds;
        public bool IsPermanent;
        public int MaxStacks;
        public int BaseStackAmount = 1;
        // 상태가 제한하는 유닛 행동
        public bool CanMove = true;
        public bool CanAct = true;
        public bool CanUseSpecialSkill = true;
        // 중첩 하나당 적용할 능력치 변화
        public float ActionSpeedBonusPerStack;
        public float MoveSpeedBonusPerStack;
        public float AttackPowerBonusPerStack;
        public float DamageTakenBonusPerStack;
        public float CriticalDamageTakenBonusPerStack;
        public float CriticalResistanceBonusPerStack;
        public float ElementResistReductionPerStack;
        public float ElementDamageTakenBonusPerStack;
        // 상태와 함께 표시할 선택적 프리팹
        public GameObject StatusEffectPrefab;

        public string Id => StatusEffectId;
        public string DisplayName => StatusEffectLabel;
        public int DefaultMaxStacks => MaxStacks;
        public bool Permanent => IsPermanent;
    }

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

    [Serializable]
    public class StatusConditionRequirement
    {
        public StatusEffectKind Kind;
        public int MinStacks;
    }

    [Serializable]
    public class StatusConditionGroup
    {
        public StatusConditionRequirement[] Requirements = Array.Empty<StatusConditionRequirement>();
    }

    [Serializable]
    public class SkillRuntimeKindCondition
    {
        public bool AreaLike;
        public SkillRuntimeKind Kind;
    }

    public static class StatusEffectLookup
    {
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
            }

            return false;
        }

        public static StatusEffectDefinition GetDefinition(StatusEffectKind kind)
        {
            var id = ToId(kind);
            var catalog = CsvDataLoader.CurrentCatalog;
            if (catalog != null && catalog.TryGetData(id, out StatusEffectDefinition definition))
            {
                return definition;
            }

            throw new KeyNotFoundException($"Status definition '{id}' is not registered.");
        }

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
            }

            return string.Empty;
        }

        public static string ToDisplayName(StatusEffectKind kind)
        {
            return GetDefinition(kind).DisplayName;
        }
    }

    public class StatusRuntimeData
    {
        public StatusEffectDefinition Definition;

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
        public float CriticalDamageBonus;
        public float AilmentResistanceBonus;
        public float CriticalResistanceBonus;
        public float DamageTakenBonus;
        public float ElementResistReduction;
        public float FlatElementResistReduction;
        public float ElementDamageTakenBonus;
        public DamageAttribute ElementModifierTarget;
        public bool HasElementModifierTarget;
        public bool IsControlEffect;
        public GameObject StatusEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Conditional Conversion")]
        public string TriggerConditionTag;
        public int TriggerConditionStacks;

        [Header("Conditional Incoming Damage")]
        public StatusEffectKind ConditionalSourceStatusKind;
        public float ConditionalDamageTakenBonus;
        public string ConditionalIncomingSkillRuntimeKinds;
        public SkillRuntimeKindCondition[] ConditionalIncomingSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();

        [Header("Conditional Status Application")]
        public string ConditionalTargetStatusTag;
        public StatusEffectKind ConditionalTargetStatusKind;
        public float ConditionalStatusChanceBonus;
        public string ConditionalOutgoingSkillRuntimeKinds;
        public SkillRuntimeKindCondition[] ConditionalOutgoingSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();

        [Header("Applied Status Duration Bonus")]
        public string AppliedStatusDurationBonusStatusId;
        public float AppliedStatusDurationBonus;

        [Header("Outgoing Additional Damage")]
        public float OutgoingAdditionalDamageMultiplier;
        public DamageAttribute OutgoingAdditionalDamageTriggerAttribute;
        public DamageAttribute OutgoingAdditionalDamageAttribute;

        public StatusRuntimeData Clone()
        {
            var clone = (StatusRuntimeData)MemberwiseClone();
            clone.Modifiers = new BuffModifierSpec
            {
                ActionSpeedBonus = Modifiers.ActionSpeedBonus,
                AttackPowerBonus = Modifiers.AttackPowerBonus,
                SpellPowerBonus = Modifiers.SpellPowerBonus,
                DamageBonusRate = Modifiers.DamageBonusRate,
                ShieldReceivedBonus = Modifiers.ShieldReceivedBonus,
                CritChanceBonusRate = Modifiers.CritChanceBonusRate,
                CritDamageBonusRate = Modifiers.CritDamageBonusRate,
                ResistReduction = Modifiers.ResistReduction,
                ResistReductionElement = Modifiers.ResistReductionElement
            };
            return clone;
        }
    }
}
