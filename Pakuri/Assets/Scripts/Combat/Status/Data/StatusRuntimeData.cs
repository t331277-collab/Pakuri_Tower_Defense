using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 카탈로그 상태 정의에 스킬 출처별 실행 설정을 더해 보관한다.
 */
namespace Pakuri.InGame
{
    public sealed class StatusRuntimeData
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
        public string ConditionalSourceStatusTag;
        public float ConditionalDamageTakenBonus;
        public string ConditionalIncomingSkillRuntimeKinds;

        [Header("Conditional Status Application")]
        public string ConditionalTargetStatusTag;
        public float ConditionalStatusChanceBonus;
        public string ConditionalOutgoingSkillRuntimeKinds;

        [Header("Applied Status Duration Bonus")]
        public string AppliedStatusDurationBonusStatusId;
        public float AppliedStatusDurationBonus;

        [Header("Outgoing Additional Damage")]
        public float OutgoingAdditionalDamageMultiplier;
        public DamageAttribute OutgoingAdditionalDamageTriggerAttribute;
        public DamageAttribute OutgoingAdditionalDamageAttribute;

        /*
         * 선택지 보정용 복사본을 만들고 변경되는 보정값 묶음을 분리한다.
         */
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
