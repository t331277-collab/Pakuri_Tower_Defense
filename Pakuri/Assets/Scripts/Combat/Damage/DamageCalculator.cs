using System;
using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

/*
 * 시전자의 능력치와 스킬 데이터로 원본 수치를 만들고 대상의 최종 피해로 확정한다.
 * 공격자와 대상의 상태 효과, 방어력, 치명타, 적 패시브 배율을 순서대로 반영하고
 * InGameCombatManager가 자원에 적용할 정수 피해량을 반환한다.
 */
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

    /*
     * 피해 속성에 맞는 방어력과 치명타 및 최종 피해 배율을 반영해
     * 최종 피해량을 계산한다.
     */
    public static class DamageCalculator
    {
        public const float BaseCriticalChance = 0.05f;
        public const float BaseCriticalMultiplier = 1.5f;

        /*
         * 공격력 계수와 스킬 강화, 공격자의 주는 피해 보정을 적용한다.
         */
        internal static float ResolveDamage(
            UnitCombatState caster,
            SkillDamageSpec damage,
            SkillSnapshot snapshot)
        {
            if (damage == null)
            {
                return 0f;
            }

            var baseDamage = ResolvePowerValue(caster, damage);
            if (snapshot != null)
            {
                baseDamage = (baseDamage + snapshot.BaseDamageBonus) * Mathf.Max(0f, snapshot.DamageMultiplier);
            }

            baseDamage *= StatusCombatRules.ResolveOutgoingDamageMultiplier(caster, damage.Element, damage.SkillId);
            if (caster is EnemyCombatState enemy)
            {
                baseDamage *= EnemyPassiveModifiers.ResolveOutgoingDamageMultiplier(
                    enemy,
                    damage.Element);
            }

            return Mathf.Max(0f, baseDamage);
        }

        /*
         * 스킬 기본값과 공격력 또는 주문력 계수로 원본 수치를 만든다.
         */
        internal static float ResolvePowerValue(UnitCombatState caster, SkillDamageSpec spec)
        {
            if (spec == null)
            {
                return 0f;
            }

            if (spec.UseCombinedStatCoefficients)
            {
                var attack = ResolveStat(caster, StatSource.Attack);
                var spell = ResolveStat(caster, StatSource.Intelligence);
                return Mathf.Max(
                    0f,
                    spec.BaseDamage
                    + attack * spec.AttackPowerCoefficient
                    + spell * spec.SpellPowerCoefficient);
            }

            var stat = ResolveStat(caster, spec.StatSource);
            return Mathf.Max(0f, spec.BaseDamage + stat * spec.StatCoefficient);
        }

        /*
         * 스킬 능력치 계수와 강화 배율로 보호막 수치를 만든다.
         */
        internal static float ResolveShield(UnitCombatState caster, BuffShieldSkillRuntimeData skill, SkillSnapshot snapshot = null)
        {
            if (skill == null)
            {
                return 0f;
            }

            var stat = ResolveStat(caster, skill.ShieldStatSource);
            var shield = Mathf.Max(0f, skill.ShieldBase + stat * skill.ShieldCoefficient);
            if (snapshot != null)
            {
                shield = (shield + snapshot.BaseDamageBonus)
                    * Mathf.Max(0f, snapshot.DamageMultiplier)
                    * Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
            }

            return Mathf.Max(0f, shield);
        }

        /*
         * 대상 조건에 따른 스킬 피해 배율을 적용한다.
         */
        internal static float ResolveDamageAgainstTarget(
            float baseDamage,
            SkillSnapshot snapshot,
            UnitCombatState target)
        {
            if (snapshot == null || target == null)
            {
                return Mathf.Max(0f, baseDamage);
            }

            return Mathf.Max(0f, baseDamage * snapshot.ResolveConditionalDamageMultiplier(target));
        }

        /*
         * 물리와 각 원소 속성의 방어력을 한 묶음으로 보관한다.
         */
        [Serializable]
        public class AttributeDefenseSet
        {
            public float Physical;
            public float Fire;
            public float Lightning;
            public float Ice;
            public float Darkness;
            public float Holy;

            /*
             * 전달받은 피해 속성에 대응하는 방어력 값을 반환한다.
             */
            public float Get(DamageAttribute attribute)
            {
                switch (attribute)
                {
                    case DamageAttribute.Physical:
                        return Physical;
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
                        throw new ArgumentOutOfRangeException(nameof(attribute), attribute, null);
                }
            }
        }

        /*
         * 기본 피해량에 대상 방어력, 방어력 감소, 치명타와 최종 피해 배율을
         * 순서대로 적용해 최종 피해량을 반환한다.
         */
        public static float Resolve(
            float baseDamage,
            DamageAttribute attribute,
            AttributeDefenseSet defenses,
            bool criticalAllowed,
            float flatDefenseReduction = 0f,
            float[] percentDefenseReductions = null,
            float criticalChanceBonus = 0f,
            float criticalMultiplierBonus = 0f,
            float targetCriticalResistance = 0f,
            float criticalDamageTakenBonus = 0f,
            float finalDamageMultiplier = 1f)
        {
            var baseDefense = 0f;
            if (defenses != null)
            {
                baseDefense = defenses.Get(attribute);
            }

            var finalDefense = ResolveDefense(
                baseDefense,
                flatDefenseReduction,
                percentDefenseReductions);

            var damageAfterDefense = baseDamage * (100f / (100f + finalDefense));
            var criticalChance = Mathf.Clamp01(BaseCriticalChance + criticalChanceBonus - targetCriticalResistance);
            var isCritical = criticalAllowed && UnityEngine.Random.value < criticalChance;
            var criticalMultiplier = BaseCriticalMultiplier + criticalMultiplierBonus + criticalDamageTakenBonus;
            var afterCritical = damageAfterDefense;
            if (isCritical)
            {
                afterCritical *= criticalMultiplier;
            }

            var safeFinalMultiplier = Mathf.Max(0f, finalDamageMultiplier);
            return afterCritical * safeFinalMultiplier;
        }

        /*
         * 유닛 능력치와 상태 효과를 반영해 실제 적용할 최종 피해를 계산한다.
         */
        public static float CalculateDamage(
            UnitCombatState target,
            float baseDamage,
            DamageAttribute attribute,
            DamageApplicationOptions options)
        {
            var criticalAllowed = options.CriticalAllowed;
            var sourceStats = options.Source.Stats;
            var criticalChance = BaseCriticalChance;
            var criticalDamage = BaseCriticalMultiplier;

            if (criticalAllowed)
            {
                criticalChance = sourceStats.CriticalChance
                    + StatusCombatRules.ResolveCriticalChanceBonus(options.Source);
                criticalDamage = sourceStats.CriticalDamage;
                criticalDamage += StatusCombatRules.ResolveCriticalDamageBonus(options.Source);
            }

            var criticalResistance = 0f;
            var criticalDamageTaken = 0f;
            if (criticalAllowed)
            {
                criticalResistance = target.Stats.CriticalResistance
                    + StatusCombatRules.ResolveCriticalResistanceBonus(target);
                criticalDamageTaken = StatusCombatRules.ResolveCriticalDamageTakenBonus(target);
            }

            // 공격자 치명타 보정과 대상의 방어·받는 피해 보정을 최종 계산기로 전달한다.
            var damage = Resolve(
                Mathf.Max(0f, baseDamage),
                attribute,
                CopyDefenses(target.Defenses),
                criticalAllowed,
                flatDefenseReduction: StatusCombatRules.ResolveFlatElementResistReduction(target, attribute),
                percentDefenseReductions: new[] { StatusCombatRules.ResolveElementResistReduction(target, attribute) },
                criticalChanceBonus: criticalChance + options.CritChanceBonus - BaseCriticalChance,
                criticalMultiplierBonus: criticalDamage + options.CritDamageBonus - BaseCriticalMultiplier,
                targetCriticalResistance: criticalResistance,
                criticalDamageTakenBonus: criticalDamageTaken,
                finalDamageMultiplier: GetIncomingDamageMultiplier(target, options.Source, attribute, options.SourceSkillId));

            return Mathf.Round(Mathf.Max(0f, damage));
        }

        /*
         * 유닛 방어력 값을 피해 계산 형식으로 복사한다.
         */
        private static AttributeDefenseSet CopyDefenses(UnitDefenseStats defenses)
        {
            return new AttributeDefenseSet
            {
                Physical = defenses.Physical,
                Fire = defenses.Fire,
                Lightning = defenses.Lightning,
                Ice = defenses.Ice,
                Darkness = defenses.Darkness,
                Holy = defenses.Holy
            };
        }

        /*
         * 상태 효과와 적 패시브의 받는 피해 배율을 합친다.
         */
        private static float GetIncomingDamageMultiplier(
            UnitCombatState target,
            UnitCombatState source,
            DamageAttribute attribute,
            string sourceSkillId)
        {
            var statusMultiplier = StatusCombatRules.ResolveIncomingDamageMultiplier(
                target,
                source,
                attribute,
                sourceSkillId);
            var enemy = target as EnemyCombatState;
            if (enemy == null)
            {
                return statusMultiplier;
            }

            return Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier) * statusMultiplier;
        }

        /*
         * 스킬 계수에 사용할 공격력 또는 주문력을 반환한다.
         */
        private static float ResolveStat(UnitCombatState caster, StatSource source)
        {
            if (caster == null || caster.Stats == null)
            {
                return 0f;
            }

            if (source == StatSource.Attack)
            {
                return caster.Stats.AttackPower * StatusCombatRules.ResolveAttackPowerMultiplier(caster);
            }

            return caster.Stats.SpellPower * StatusCombatRules.ResolveSpellPowerMultiplier(caster);
        }

        /*
         * 기본 방어력에서 고정 감소와 비율 감소를 순서대로 적용하고
         * 최종 방어력이 0 아래로 내려가지 않도록 제한한다.
         */
        public static float ResolveDefense(
            float baseDefense,
            float flatDefenseReduction,
            float[] percentDefenseReductions)
        {
            var finalDefense = baseDefense - flatDefenseReduction;
            var safeReductions = percentDefenseReductions;
            if (safeReductions == null)
            {
                safeReductions = Array.Empty<float>();
            }

            // 여러 비율 감소는 합산하지 않고 남은 방어력에 차례대로 곱한다.
            for (var i = 0; i < safeReductions.Length; i++)
            {
                finalDefense *= 1f - Mathf.Clamp01(safeReductions[i]);
            }

            return Mathf.Max(0f, finalDefense);
        }
    }
}
