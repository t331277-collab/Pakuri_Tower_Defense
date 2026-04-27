using System;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    public enum SkillSlot
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }

    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

    public enum SkillRuntimeKind
    {
        MagazineProjectile,
        CooldownProjectile,
        LineAttack,
        AreaAttack,
        Field,
        Buff,
        Shield,
        Heal,
        Mark,
        Execute,
        Passive
    }

    [Serializable]
    public class SkillDefinition
    {
        public string SkillId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public float Range = 8f;
        public float Radius;
        public float CooldownSeconds;
        public int MagazineCapacity;
        public float ReloadSeconds;
        public float ShotIntervalSeconds;
        public bool CriticalAllowed = true;
        public string StatusEffectId;
        [TextArea(2, 4)] public string Summary;
    }

    [Serializable]
    public class PassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillSlot RequiredActiveSlot;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        [TextArea(2, 4)] public string Summary;
    }
}
