using System;
using Pakuri.Combat;

namespace Pakuri.Data
{
    public enum StatusEffectClassification
    {
        Buff,
        Debuff
    }

    [Serializable]
    public sealed class StatusEffectDefinitionData
    {
        public string StatusEffectId;
        public string StatusEffectLabel;
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
        public float CriticalResistanceBonusPerStack;
        public float ElementResistReductionPerStack;
        public float ElementDamageTakenBonusPerStack;
    }
}
