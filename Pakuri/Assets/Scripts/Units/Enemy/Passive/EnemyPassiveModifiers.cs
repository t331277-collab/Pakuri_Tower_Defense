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
        /*
         * Apply 처리를 대상에 적용한다.
         */
        public static void Apply(
            EnemyCombatState enemy /* 적 */,
            EnemyPassiveDefinition passive /* 패시브 */)
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

        /*
         * 주는 피해 보너스를 계산해 반환한다.
         */
        public static float OutgoingDamageBonus(
            EnemyCombatState enemy /* 적 */,
            DamageAttribute attribute /* 피해 속성 */)
        {
            if (enemy == null)
            {
                return 0f;
            }

            switch (attribute)
            {
                case DamageAttribute.Physical:
                    return Math.Max(0f, enemy.PassivePhysicalDamageMultiplier) - 1f;
                case DamageAttribute.Fire:
                    return Math.Max(0f, enemy.PassiveFireDamageMultiplier) - 1f;
                case DamageAttribute.Lightning:
                    return Math.Max(0f, enemy.PassiveLightningDamageMultiplier) - 1f;
                case DamageAttribute.Ice:
                    return Math.Max(0f, enemy.PassiveIceDamageMultiplier) - 1f;
                case DamageAttribute.Darkness:
                    return Math.Max(0f, enemy.PassiveDarknessDamageMultiplier) - 1f;
                case DamageAttribute.Holy:
                    return Math.Max(0f, enemy.PassiveHolyDamageMultiplier) - 1f;
            }

            throw new InvalidOperationException("Unsupported damage attribute: " + attribute);
        }

        /*
         * 받는 피해 보너스를 계산해 반환한다.
         */
        public static float IncomingDamageBonus(EnemyCombatState enemy /* 적 */)
        {
            if (enemy == null)
            {
                return 0f;
            }

            return Math.Max(0f, enemy.PassiveIncomingDamageMultiplier) - 1f;
        }

        /*
         * MultiplyOutgoingDamage 작업을 수행한다.
         */
        private static void MultiplyOutgoingDamage(
            EnemyCombatState enemy /* 적 */,
            DamageAttribute attribute /* 피해 속성 */,
            float multiplier /* 값에 곱할 배율 */)
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

        /*
         * MultiplyDefenses 작업을 수행한다.
         */
        private static void MultiplyDefenses(UnitDefenseStats defenses /* 방어력 묶음 */, float multiplier /* 값에 곱할 배율 */)
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

        /*
         * MultiplyDefense 작업을 수행한다.
         */
        private static void MultiplyDefense(
            UnitDefenseStats defenses /* 방어력 묶음 */,
            DamageAttribute attribute /* 피해 속성 */,
            float multiplier /* 값에 곱할 배율 */)
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
