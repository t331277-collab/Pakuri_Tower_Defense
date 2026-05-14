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

        public static DamageResult Resolve(
            float baseDamage,
            float defense,
            float criticalChanceBonus = 0f,
            float criticalMultiplierBonus = 0f)
        {
            var safeDefense = Mathf.Max(-95f, defense);
            var damageAfterDefense = baseDamage * (100f / (100f + safeDefense));
            var criticalChance = Mathf.Clamp01(BaseCriticalChance + criticalChanceBonus);
            var isCritical = Random.value < criticalChance;
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
            var isCritical = Random.value < criticalChance;
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
