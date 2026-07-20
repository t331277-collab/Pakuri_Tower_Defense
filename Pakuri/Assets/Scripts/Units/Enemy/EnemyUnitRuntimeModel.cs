using System;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 적의 전투 수치와 패시브 배율을 보관하고 적용하는 런타임 데이터.
 */
namespace Pakuri.InGame
{
    public sealed class EnemyUnitRuntimeModel : BaseUnitRuntimeModel
    {
        public EnemyEncounterRole EncounterRole;
        public EnemyAttackType AttackType;
        public DamageAttribute Attribute;
        public float NexusDamage = 1f;
        public float PassivePhysicalDamageMultiplier = 1f;
        public float PassiveFireDamageMultiplier = 1f;
        public float PassiveLightningDamageMultiplier = 1f;
        public float PassiveIceDamageMultiplier = 1f;
        public float PassiveDarknessDamageMultiplier = 1f;
        public float PassiveHolyDamageMultiplier = 1f;
        public float PassiveIncomingDamageMultiplier = 1f;
        public float PassiveHealingMultiplier = 1f;
    }

    internal static class EnemyPassiveRuntime
    {
        public static void Apply(
            EnemyUnitRuntimeModel enemy,
            EnemyPassiveDefinition passive)
        {
            if (enemy == null
                || passive == null
                || passive.ApplyTarget != EnemyPassiveTarget.Self
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
                case EnemyPassiveModifierKind.PhysicalDamageUp:
                    enemy.PassivePhysicalDamageMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.FireDamageUp:
                    enemy.PassiveFireDamageMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.LightningDamageUp:
                    enemy.PassiveLightningDamageMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.IceDamageUp:
                    enemy.PassiveIceDamageMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.DarknessDamageUp:
                    enemy.PassiveDarknessDamageMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.HolyDamageUp:
                    enemy.PassiveHolyDamageMultiplier *= 1f + value;
                    break;
                case EnemyPassiveModifierKind.DefenseUp:
                    MultiplyDefenses(enemy.Defenses, 1f + value);
                    break;
                case EnemyPassiveModifierKind.PhysicalDefenseUp:
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Physical, 1f + value);
                    break;
                case EnemyPassiveModifierKind.FireDefenseUp:
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Fire, 1f + value);
                    break;
                case EnemyPassiveModifierKind.LightningDefenseUp:
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Lightning, 1f + value);
                    break;
                case EnemyPassiveModifierKind.IceDefenseUp:
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Ice, 1f + value);
                    break;
                case EnemyPassiveModifierKind.DarknessDefenseUp:
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Darkness, 1f + value);
                    break;
                case EnemyPassiveModifierKind.HolyDefenseUp:
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Holy, 1f + value);
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
            EnemyUnitRuntimeModel enemy,
            DamageAttribute attribute)
        {
            if (enemy == null)
            {
                return 1f;
            }

            switch (attribute)
            {
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
                default:
                    return Math.Max(0f, enemy.PassivePhysicalDamageMultiplier);
            }
        }

        private static void MultiplyDefenses(UnitDefenseRuntime defenses, float multiplier)
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
            UnitDefenseRuntime defenses,
            DamageAttribute attribute,
            float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            switch (attribute)
            {
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
                default:
                    defenses.Physical *= multiplier;
                    break;
            }
        }
    }
}
