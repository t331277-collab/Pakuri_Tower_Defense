using System;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 적 데이터에 정의된 패시브 수치를 적 전투 상태에 적용한다.
 */
namespace Pakuri.InGame
{
    internal static class EnemyPassiveModifiers
    {
        public static void Apply(
            EnemyCombatState enemy,
            EnemyPassiveDefinition passive)
        {
            if (enemy == null
                || passive == null
                || passive.ModifierKind == EnemyPassiveModifierKind.None)
            {
                return;
            }

            var value = Math.Max(0f, passive.ModifierValue);
            if (value <= 0f)
            {
                return;
            }

            switch (passive.ModifierKind)
            {
                case EnemyPassiveModifierKind.DamageUp:
                    MultiplyOutgoingDamage(enemy, passive.Attribute, 1f + value);
                    break;
                case EnemyPassiveModifierKind.DefenseUp:
                    if (passive.HasAttribute)
                    {
                        MultiplyDefense(enemy.Defenses, passive.Attribute, 1f + value);
                    }
                    else
                    {
                        MultiplyDefenses(enemy.Defenses, 1f + value);
                    }
                    break;
                case EnemyPassiveModifierKind.CritChanceUp:
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalChance += value;
                    }

                    break;
                case EnemyPassiveModifierKind.CritDamageUp:
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalDamage += value;
                    }

                    break;
                case EnemyPassiveModifierKind.HealingUp:
                    enemy.PassiveHealingMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.IncomingDamageDown:
                    enemy.PassiveIncomingDamageMultiplier *= Math.Max(0f, 1f - value);
                    break;
            }
        }

        public static float ResolveOutgoingDamageMultiplier(
            EnemyCombatState enemy,
            DamageAttribute attribute)
        {
            if (enemy == null)
            {
                return 1f;
            }

            switch (attribute)
            {
                case DamageAttribute.Physical:
                    return Math.Max(0f, enemy.PassivePhysicalDamageMultiplier);
                case DamageAttribute.Fire:
                    return Math.Max(0f, enemy.PassiveFireDamageMultiplier);
                case DamageAttribute.Lightning:
                    return Math.Max(0f, enemy.PassiveLightningDamageMultiplier);
                case DamageAttribute.Ice:
                    return Math.Max(0f, enemy.PassiveIceDamageMultiplier);
                case DamageAttribute.Darkness:
                    return Math.Max(0f, enemy.PassiveDarknessDamageMultiplier);
                case DamageAttribute.Holy:
                    return Math.Max(0f, enemy.PassiveHolyDamageMultiplier);
            }

            throw new InvalidOperationException("Unsupported damage attribute: " + attribute);
        }

        private static void MultiplyOutgoingDamage(
            EnemyCombatState enemy,
            DamageAttribute attribute,
            float multiplier)
        {
            switch (attribute)
            {
                case DamageAttribute.Physical:
                    enemy.PassivePhysicalDamageMultiplier *= multiplier;
                    break;
                case DamageAttribute.Fire:
                    enemy.PassiveFireDamageMultiplier *= multiplier;
                    break;
                case DamageAttribute.Lightning:
                    enemy.PassiveLightningDamageMultiplier *= multiplier;
                    break;
                case DamageAttribute.Ice:
                    enemy.PassiveIceDamageMultiplier *= multiplier;
                    break;
                case DamageAttribute.Darkness:
                    enemy.PassiveDarknessDamageMultiplier *= multiplier;
                    break;
                case DamageAttribute.Holy:
                    enemy.PassiveHolyDamageMultiplier *= multiplier;
                    break;
            }
        }

        private static void MultiplyDefenses(UnitDefenseStats defenses, float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            defenses.Physical *= multiplier;
            defenses.Fire *= multiplier;
            defenses.Lightning *= multiplier;
            defenses.Ice *= multiplier;
            defenses.Darkness *= multiplier;
            defenses.Holy *= multiplier;
        }

        private static void MultiplyDefense(
            UnitDefenseStats defenses,
            DamageAttribute attribute,
            float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            switch (attribute)
            {
                case DamageAttribute.Physical:
                    defenses.Physical *= multiplier;
                    break;
                case DamageAttribute.Fire:
                    defenses.Fire *= multiplier;
                    break;
                case DamageAttribute.Lightning:
                    defenses.Lightning *= multiplier;
                    break;
                case DamageAttribute.Ice:
                    defenses.Ice *= multiplier;
                    break;
                case DamageAttribute.Darkness:
                    defenses.Darkness *= multiplier;
                    break;
                case DamageAttribute.Holy:
                    defenses.Holy *= multiplier;
                    break;
            }
        }
    }
}
