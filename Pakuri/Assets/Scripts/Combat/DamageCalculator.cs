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
        public DamageResult(float baseDamage, float defense, float finalDamage, bool isCritical)
        {
            BaseDamage = baseDamage;
            Defense = defense;
            FinalDamage = finalDamage;
            IsCritical = isCritical;
        }

        public float BaseDamage { get; }
        public float Defense { get; }
        public float FinalDamage { get; }
        public bool IsCritical { get; }
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
            return new DamageResult(baseDamage, safeDefense, finalDamage, isCritical);
        }
    }
}
