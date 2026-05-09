using Pakuri.Data;

namespace Pakuri.Combat
{
    public readonly struct EnemyAttackResolution
    {
        public EnemyAttackResolution(float finalDamage, DamageResult damageResult)
        {
            FinalDamage = finalDamage;
            DamageResult = damageResult;
        }

        public float FinalDamage { get; }
        public DamageResult DamageResult { get; }
    }

    public static class EnemyAttackResolver
    {
        public static EnemyAttackResolution ResolveAgainstMonster(
            EnemyDefinition definition,
            float attackPower,
            float damageMultiplier,
            float attackBuffMultiplier,
            float criticalChanceBonus,
            float criticalMultiplierBonus,
            AttributeDefenseSet monsterDefenses)
        {
            var baseDamage = ResolveBaseDamage(definition, attackPower);
            var finalMultiplier = damageMultiplier * attackBuffMultiplier;
            var result = DamageCalculator.Resolve(
                baseDamage,
                definition != null ? definition.Attribute : DamageAttribute.Physical,
                monsterDefenses,
                criticalChanceBonus: criticalChanceBonus,
                criticalMultiplierBonus: criticalMultiplierBonus,
                finalDamageMultiplier: finalMultiplier);

            return new EnemyAttackResolution(result.FinalDamage, result);
        }

        public static EnemyAttackResolution ResolveAgainstNexus(
            EnemyDefinition definition,
            float attackPower,
            float damageMultiplier,
            float attackBuffMultiplier,
            float criticalChanceBonus,
            float criticalMultiplierBonus)
        {
            var baseDamage = ResolveBaseDamage(definition, attackPower);
            var finalMultiplier = damageMultiplier * attackBuffMultiplier;
            var result = DamageCalculator.Resolve(
                baseDamage,
                definition != null ? definition.Attribute : DamageAttribute.Physical,
                null,
                criticalChanceBonus: criticalChanceBonus,
                criticalMultiplierBonus: criticalMultiplierBonus,
                finalDamageMultiplier: finalMultiplier);

            return new EnemyAttackResolution(result.FinalDamage, result);
        }

        private static float ResolveBaseDamage(EnemyDefinition definition, float attackPower)
        {
            var coefficient = definition != null ? definition.ActiveSkillCoefficient : 1f;
            return UnityEngine.Mathf.Max(0f, attackPower * UnityEngine.Mathf.Max(0f, coefficient));
        }
    }
}
