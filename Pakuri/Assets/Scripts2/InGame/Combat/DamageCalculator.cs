using System;
using UnityEngine;

namespace Pakuri.Combat
{
    public enum DamageAttribute
    {
        Physical,
        Fire,
        Lightning,
        Ice,
        Darkness,
        Holy
    }

    public readonly struct DamageResult
    {
        public DamageResult(
            float baseDamage,
            float defense,
            float finalDamage,
            bool isCritical,
            DamageAttribute attribute = DamageAttribute.Physical,
            string formulaLog = "")
        {
            BaseDamage = baseDamage;
            Defense = defense;
            FinalDamage = finalDamage;
            IsCritical = isCritical;
            Attribute = attribute;
            FormulaLog = formulaLog;
        }

        public float BaseDamage { get; }
        public float Defense { get; }
        public float FinalDamage { get; }
        public bool IsCritical { get; }
        public DamageAttribute Attribute { get; }
        public string FormulaLog { get; }
    }

    public static class DamageCalculator
    {
        public const float BaseCriticalChance = 0.05f;
        public const float BaseCriticalMultiplier = 1.5f;

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
            [Range(0f, 1f)] public float CriticalChance = BaseCriticalChance;
            public float CriticalDamage = BaseCriticalMultiplier;
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

        public static DamageResult Resolve(
            float baseDamage,
            float defense,
            float criticalChanceBonus = 0f,
            float criticalMultiplierBonus = 0f)
        {
            var safeDefense = Mathf.Max(-95f, defense);
            var damageAfterDefense = baseDamage * (100f / (100f + safeDefense));
            var criticalChance = Mathf.Clamp01(BaseCriticalChance + criticalChanceBonus);
            var isCritical = UnityEngine.Random.value < criticalChance;
            var criticalMultiplier = BaseCriticalMultiplier + criticalMultiplierBonus;
            var finalDamage = isCritical ? damageAfterDefense * criticalMultiplier : damageAfterDefense;
            var formula =
                $"BaseDamage {baseDamage:0.##} * (100 / (100 + Def {safeDefense:0.##})) = {damageAfterDefense:0.##}" +
                (isCritical ? $" * Crit {criticalMultiplier:0.##}" : string.Empty) +
                $" => {finalDamage:0.##}";
            return new DamageResult(baseDamage, safeDefense, finalDamage, isCritical, DamageAttribute.Physical, formula);
        }

        public static DamageResult Resolve(
            float baseDamage,
            DamageAttribute attribute,
            AttributeDefenseSet defenses,
            float flatDefenseBonus = 0f,
            float flatDefenseReduction = 0f,
            float percentDefenseBonus = 0f,
            float[] percentDefenseReductions = null,
            float criticalChanceBonus = 0f,
            float criticalMultiplierBonus = 0f,
            float targetCriticalResistance = 0f,
            float criticalDamageTakenBonus = 0f,
            float finalDamageMultiplier = 1f)
        {
            var baseDefense = defenses != null ? defenses.Get(attribute) : 0f;
            var breakdown = ResolveDefense(
                attribute,
                baseDefense,
                flatDefenseBonus,
                flatDefenseReduction,
                percentDefenseBonus,
                percentDefenseReductions);

            var safeDefense = Mathf.Max(-95f, breakdown.FinalDefense);
            var damageAfterDefense = baseDamage * (100f / (100f + safeDefense));
            var criticalChance = Mathf.Clamp01(BaseCriticalChance + criticalChanceBonus - targetCriticalResistance);
            var isCritical = UnityEngine.Random.value < criticalChance;
            var criticalMultiplier = BaseCriticalMultiplier + criticalMultiplierBonus + criticalDamageTakenBonus;
            var afterCritical = isCritical ? damageAfterDefense * criticalMultiplier : damageAfterDefense;
            var safeFinalMultiplier = Mathf.Max(0f, finalDamageMultiplier);
            var finalDamage = afterCritical * safeFinalMultiplier;
            var reductionText = FormatPercentReductions(breakdown.PercentReductions);
            var formula =
                $"[{attribute}] Def: (({baseDefense:0.##} + {flatDefenseBonus:0.##} - {flatDefenseReduction:0.##}) * (1 + {percentDefenseBonus:0.##}){reductionText}) = {safeDefense:0.##}; " +
                $"Damage: {baseDamage:0.##} * (100 / (100 + {safeDefense:0.##})) = {damageAfterDefense:0.##}" +
                (isCritical ? $" * Crit {criticalMultiplier:0.##}" : string.Empty) +
                (Mathf.Approximately(safeFinalMultiplier, 1f) ? string.Empty : $" * FinalMul {safeFinalMultiplier:0.##}") +
                $" => {finalDamage:0.##}";

            return new DamageResult(baseDamage, safeDefense, finalDamage, isCritical, attribute, formula);
        }

        public static DefenseBreakdown ResolveDefense(
            DamageAttribute attribute,
            float baseDefense,
            float flatDefenseBonus,
            float flatDefenseReduction,
            float percentDefenseBonus,
            float[] percentDefenseReductions)
        {
            var finalDefense = (baseDefense + flatDefenseBonus - flatDefenseReduction) * (1f + percentDefenseBonus);
            var safeReductions = percentDefenseReductions ?? System.Array.Empty<float>();
            for (var i = 0; i < safeReductions.Length; i++)
            {
                finalDefense *= 1f - Mathf.Clamp01(safeReductions[i]);
            }

            return new DefenseBreakdown(
                attribute,
                baseDefense,
                flatDefenseBonus,
                flatDefenseReduction,
                percentDefenseBonus,
                safeReductions,
                finalDefense);
        }

        private static string FormatPercentReductions(float[] reductions)
        {
            if (reductions == null || reductions.Length == 0)
            {
                return string.Empty;
            }

            var text = string.Empty;
            for (var i = 0; i < reductions.Length; i++)
            {
                text += $" * (1 - {Mathf.Clamp01(reductions[i]):0.##})";
            }

            return text;
        }
    }
}
