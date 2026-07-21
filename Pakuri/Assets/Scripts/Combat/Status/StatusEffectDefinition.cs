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
        public StatusEffectKind Kind;
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

            var enumName = value.Trim().Replace("-", string.Empty);
            if (!Enum.TryParse(enumName, true, out kind))
            {
                return false;
            }

            return kind != StatusEffectKind.None;
        }

        public static StatusEffectDefinition GetDefinition(StatusEffectKind kind)
        {
            var catalog = GameDataLoader.CurrentCatalog;
            if (catalog != null)
            {
                var definitions = catalog.StatusEffects;
                for (var i = 0; i < definitions.Length; i++)
                {
                    var definition = definitions[i];
                    if (definition != null && definition.Kind == kind)
                    {
                        return definition;
                    }
                }
            }

            throw new KeyNotFoundException($"Status definition '{kind}' is not registered.");
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
        public StatusEffectKind[] ConditionalTargetStatusKinds = Array.Empty<StatusEffectKind>();
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
