/*
 * 역할: 전투 피해량 계산.
 * 책임: 시전자 보너스·피해 속성·대상 방어력을 반영해 원시 피해와 최종 피해를 계산한다.
 */

using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Combat
{

    /// DamageCalculator가 담당하는 런타임 값을 계산한다.
    public static class DamageCalculator
    {

        /// 전달된 런타임 입력값을 사용해 RawDamage를 계산한다.
        internal static float CalculateRawDamage(
            UnitCombatState caster,
            SkillDamageSpec damage)
        {
            var attack = caster.Stats.AttackPower * StatusCombatRules.AttackPowerMultiplier(caster);
            var spell = caster.Stats.SpellPower * StatusCombatRules.SpellPowerMultiplier(caster);
            var rawDamage = damage.BaseDamage
                + attack * damage.AttackPowerCoefficient
                + spell * damage.SpellPowerCoefficient;
            return Mathf.Max(0f, rawDamage);
        }

        /// 전달된 런타임 입력값을 사용해 FinalDamage를 계산한다.
        public static float CalculateFinalDamage(
            UnitCombatState target,
            float rawDamage,
            DamageAttribute attribute,
            AttackRule attackRule)
        {
            var damage = rawDamage;
            var defense = target.Defenses.Get(attribute);
            defense *= target.SkillState.PassiveDefenseMultiplier(attribute);
            defense -= StatusCombatRules.FlatElementResistReduction(target, attribute);
            defense *= StatusCombatRules.ElementResistMultiplier(target, attribute);
            damage *= 100f / Mathf.Max(0.01f, 100f + defense);

            var finalDamageBonus = attackRule.FinalDamageBonus;
            finalDamageBonus += StatusCombatRules.OutgoingDamageBonus(
                attackRule.Source,
                target,
                attribute,
                attackRule.SourceSkillId);
            if (attackRule.Source != null)
            {
                finalDamageBonus += attackRule.Source.SkillState.PassiveOutgoingDamageBonus(attribute);
            }

            finalDamageBonus += StatusCombatRules.IncomingDamageBonus(
                target,
                attackRule.Source,
                attribute,
                attackRule.SourceSkillId);
            finalDamageBonus += target.SkillState.PassiveIncomingDamageBonus();

            damage *= Mathf.Max(0f, 1f + finalDamageBonus);

            if (attackRule.CriticalAllowed && attackRule.Source != null)
            {
                var criticalChance = attackRule.Source.Stats.CriticalChance;
                criticalChance += StatusCombatRules.CriticalChanceBonus(
                    attackRule.Source,
                    target);
                criticalChance += attackRule.Source.SkillState.PassiveCriticalChanceBonus();
                criticalChance += attackRule.CritChanceBonus;
                criticalChance -= target.Stats.CriticalResistance;
                criticalChance -= StatusCombatRules.CriticalResistanceBonus(target);

                if (UnityEngine.Random.value < Mathf.Clamp01(criticalChance))
                {
                    var criticalDamage = attackRule.Source.Stats.CriticalDamage;
                    criticalDamage += StatusCombatRules.CriticalDamageBonus(
                        attackRule.Source,
                        target);
                    criticalDamage += attackRule.Source.SkillState.PassiveCriticalDamageBonus();
                    criticalDamage += attackRule.CritDamageBonus;
                    criticalDamage += StatusCombatRules.CriticalDamageTakenBonus(target);
                    damage *= criticalDamage;
                }
            }

            return Mathf.Round(Mathf.Max(0f, damage));
        }

    }
}
