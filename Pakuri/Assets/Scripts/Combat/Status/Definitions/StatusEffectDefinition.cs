/*
 * 역할: 상태 효과 데이터 계약.
 * 책임: 상태 종류·중첩·지속 시간·주기 효과·보호막·배율·Trigger 정보를 정의한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{

    public enum StatusTargetScope
    {
        Unspecified,
        AllAllies,
        Self
    }

    public enum StatusMergePolicy
    {
        Unspecified,
        SameSourceTakeHighest,
        SameSourceRefresh,
        AlwaysStack
    }

    public enum ShieldRefreshRule
    {
        Replace,
        TakeHighest,
        Stack
    }

    [Serializable]
    public class BuffModifierSpec
    {
        public float ActionSpeedBonus;
        public float AttackPowerBonus;
        public float SpellPowerBonus;
        public float DamageBonusRate;
        public float ShieldReceivedBonus;
        public float CritChanceBonusRate;
        public float CritDamageBonusRate;
        public float ResistReduction;
        public DamageAttribute ResistReductionElement;
    }

    public enum StatusEffectClassification
    {
        Buff,
        Debuff
    }

    /// StatusEffectDefinition의 저작 데이터와 런타임 설정을 정의한다.
    [Serializable]

    public class StatusEffectDefinition
    {

        public string StatusEffectName;
        public string StatusEffectLabel;
        public StatusEffectKind Kind;
        public StatusEffectClassification Classification;
        public bool HasAttribute;
        public DamageAttribute Attribute;
        public float DefaultDurationSeconds;
        public bool IsPermanent;
        public int MaxStacks;
        public int BaseStackAmount = 1;

        public bool CanMove = true;
        public bool CanAct = true;
        public bool CanUseSpecialSkill = true;

        public float ActionSpeedBonusPerStack;
        public float MoveSpeedBonusPerStack;
        public float AttackPowerBonusPerStack;
        public float DamageTakenBonusPerStack;
        public float CriticalDamageTakenBonusPerStack;
        public float ElementResistReductionPerStack;
        public float ElementDamageTakenBonusPerStack;

        public GameObject StatusEffectPrefab;

        public StatusRuntimeData RuntimeData;

        public string Name => StatusEffectName;
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

    public class StatusRuntimeData
    {
        public StatusEffectDefinition Definition;

        [Header("Identity")]
        public StatusEffectKind Kind = StatusEffectKind.None;
        public string StatusTag;
        public string StatusName;
        public string SourceSkillName;
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
        public SkillRuntimeKindCondition[] ConditionalIncomingSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();
        public StatusConditionGroup[] ConditionalTargetStatusGroups = Array.Empty<StatusConditionGroup>();
        public string[] ConditionalTargetStatusSourceSkillNames = Array.Empty<string>();
        public SkillTargetSide ConditionalTargetSide = SkillTargetSide.Self;
        public float ConditionalTargetHealthRatioMax;

        [Header("Conditional Status Application")]
        public StatusEffectKind[] ConditionalTargetStatusKinds = Array.Empty<StatusEffectKind>();
        public float ConditionalStatusChanceBonus;
        public SkillRuntimeKindCondition[] ConditionalOutgoingSkillRuntimeKindValues = Array.Empty<SkillRuntimeKindCondition>();

        [Header("Applied Status Duration Bonus")]
        public string AppliedStatusDurationBonusStatusName;
        public float AppliedStatusDurationBonus;

        [Header("Outgoing Additional Damage")]
        public float OutgoingAdditionalDamageMultiplier;
        public DamageAttribute OutgoingAdditionalDamageTriggerAttribute;
        public DamageAttribute OutgoingAdditionalDamageAttribute;

        /// 상태 정의를 복사해 런타임 계산에 사용할 독립 인스턴스를 만든다.
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
