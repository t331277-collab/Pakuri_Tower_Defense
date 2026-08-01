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
        /// 기본 데미지를 계산 후 반환

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

        /// Actor 로 부터 호출되어서 최종 데미지를 계산한다.
        public static float CalculateFinalDamage(
            UnitCombatState target,
            float rawDamage,
            DamageAttribute attribute,
            AttackRule attackRule)
        {
            var damage = rawDamage;
            var defense = target.Defenses.Get(attribute);    // 피해 속성에 대응하는 대상의 기본 방어력
            defense -= StatusCombatRules.FlatElementResistReduction(target, attribute); // 활성 상태 효과의 고정 속성 방어력 감소량 차감
            defense *= target.SkillState.PassiveDefenseMultiplier(attribute);    // 학습한 DefenseUp 패시브의 합산된 방어력 배율 적용
            defense *= StatusCombatRules.ElementResistMultiplier(target, attribute);  // 활성 상태 효과의 속성 저항 감소율을 순차 곱셈
            damage *= 100f / Mathf.Max(0.01f, 100f + defense);

            var finalDamageBonus = attackRule.FinalDamageBonus;
            finalDamageBonus += StatusCombatRules.OutgoingDamageBonus( // 공격자가 주는 피해 보정
                attackRule.Source,
                target,
                attribute,
                attackRule.SourceSkillId);
            if (attackRule.Source != null)
            {
                finalDamageBonus += attackRule.Source.SkillState.PassiveOutgoingDamageBonus(attribute); // 공격자가 학습한 DamageUp 패시브의 피해 보정
            }

            finalDamageBonus += StatusCombatRules.IncomingDamageBonus( // 대상이 받는 피해 보정
                target,
                attackRule.Source,
                attribute,
                attackRule.SourceSkillId);
            finalDamageBonus += target.SkillState.PassiveIncomingDamageBonus(); // 대상이 학습한 IncomingDamageDown 패시브의 받는 피해 감소

            damage *= Mathf.Max(0f, 1f + finalDamageBonus);

            if (attackRule.CriticalAllowed && attackRule.Source != null)
            {
                /// 크리티컬 확률 보너스
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
                    /// 크리티컬 적용
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
