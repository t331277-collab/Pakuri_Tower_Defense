using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

/*
 * 시전자의 능력치와 스킬 데이터를 이용해 피해, 회복, 보호막의 원본 수치를 만든다.
 * 대상 방어력, 치명타, 받는 피해 배율을 반영하는 DamageCalculator와 달리
 * 스킬이 대상에게 전달하기 전의 수치와 스킬 조건부 피해 배율만 계산한다.
 */
namespace Pakuri.Combat
{

    internal static class SkillValueCalculator
    {
        /*
         * 공격력 계수와 스킬 강화, 공격자의 주는 피해 보정을 적용한다.
         */
        public static float ResolveDamage(
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
        public static float ResolvePowerValue(UnitCombatState caster, SkillDamageSpec spec)
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
        public static float ResolveShield(UnitCombatState caster, BuffShieldSkillRuntimeData skill, SkillSnapshot snapshot = null)
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
         * 대상 조건에 따른 스킬 피해 배율만 적용한다.
         * 대상 방어력과 받는 피해 보정은 이후 DamageCalculator가 처리한다.
         */
        public static float ResolveDamageAgainstTarget(
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
         * 스킬 계수에 사용할 공격력 또는 주문력을 반환한다.
         */
        private static float ResolveStat(UnitCombatState caster, StatSource source)
        {
            var stats = caster != null ? caster.Stats : null;
            if (stats == null)
            {
                return 0f;
            }

            if (source == StatSource.Attack)
            {
                return stats.AttackPower * StatusCombatRules.ResolveAttackPowerMultiplier(caster);
            }

            return stats.SpellPower * StatusCombatRules.ResolveSpellPowerMultiplier(caster);
        }

    }
}


