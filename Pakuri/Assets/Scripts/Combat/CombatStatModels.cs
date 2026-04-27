using System;
using UnityEngine;

namespace Pakuri.Combat
{
    [Serializable]
    public class AttributeDefenseSet
    {
        public float Physical;
        public float Fire;
        public float Lightning;
        public float Ice;
        public float Darkness;
        public float Holy;

        public float Get(DamageAttribute attribute)
        {
            switch (attribute)
            {
                case DamageAttribute.Fire:
                    return Fire;
                case DamageAttribute.Lightning:
                    return Lightning;
                case DamageAttribute.Ice:
                    return Ice;
                case DamageAttribute.Darkness:
                    return Darkness;
                case DamageAttribute.Holy:
                    return Holy;
                default:
                    return Physical;
            }
        }

        public void Set(DamageAttribute attribute, float value)
        {
            switch (attribute)
            {
                case DamageAttribute.Fire:
                    Fire = value;
                    break;
                case DamageAttribute.Lightning:
                    Lightning = value;
                    break;
                case DamageAttribute.Ice:
                    Ice = value;
                    break;
                case DamageAttribute.Darkness:
                    Darkness = value;
                    break;
                case DamageAttribute.Holy:
                    Holy = value;
                    break;
                default:
                    Physical = value;
                    break;
            }
        }

        public AttributeDefenseSet Clone()
        {
            return new AttributeDefenseSet
            {
                Physical = Physical,
                Fire = Fire,
                Lightning = Lightning,
                Ice = Ice,
                Darkness = Darkness,
                Holy = Holy
            };
        }
    }

    [Serializable]
    public class CombatStatBlock
    {
        public float MaxHealth = 100f;
        public float AttackPower = 30f;
        public float SpellPower = 30f;
        public float MoveSpeed = 1f;
        [Range(0f, 1f)] public float CriticalChance = DamageCalculator.BaseCriticalChance;
        public float CriticalDamage = DamageCalculator.BaseCriticalMultiplier;
        [Range(0f, 1f)] public float CriticalResistance;
    }

    public readonly struct DefenseBreakdown
    {
        public DefenseBreakdown(
            DamageAttribute attribute,
            float baseDefense,
            float flatBonus,
            float flatReduction,
            float percentBonus,
            float[] percentReductions,
            float finalDefense)
        {
            Attribute = attribute;
            BaseDefense = baseDefense;
            FlatBonus = flatBonus;
            FlatReduction = flatReduction;
            PercentBonus = percentBonus;
            PercentReductions = percentReductions ?? Array.Empty<float>();
            FinalDefense = finalDefense;
        }

        public DamageAttribute Attribute { get; }
        public float BaseDefense { get; }
        public float FlatBonus { get; }
        public float FlatReduction { get; }
        public float PercentBonus { get; }
        public float[] PercentReductions { get; }
        public float FinalDefense { get; }
    }
}
