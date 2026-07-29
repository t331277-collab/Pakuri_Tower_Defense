using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

/*
 * 시전자의 능력치와 스킬 데이터로 원본 피해량을 계산
 * 대상의 방어력, 상태 효과, 치명타와 학습한 패시브를 반영해 최종 피해량을 반환
 */
namespace Pakuri.Combat
{
    /*
     * 공격자의 능력치로 원본 피해량(CalculateRawDamage)을 만들고 대상 조건으로 최종 피해량(CalculateFinalDamage)을 계산한다.
     */
    public static class DamageCalculator
    {
        /*
         * 공격력과 주문력 계수만으로 원본 피해량을 계산한다.
         */
        internal static float CalculateRawDamage(
            UnitCombatState caster, /* 스킬을 사용하는 유닛 */
            SkillDamageSpec damage /* 피해량 */)
        {
            var attack = caster.Stats.AttackPower * StatusCombatRules.AttackPowerMultiplier(caster);
            var spell = caster.Stats.SpellPower * StatusCombatRules.SpellPowerMultiplier(caster);
            var rawDamage = damage.BaseDamage
                + attack * damage.AttackPowerCoefficient
                + spell * damage.SpellPowerCoefficient;
            return Mathf.Max(0f, rawDamage);
        }

        /*
         * 속성 방어력, 합산 최종 피해 보너스, 치명타 순서로 최종 피해량을 계산한다.
         */
        public static float CalculateFinalDamage(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            float rawDamage /* 원본 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            AttackRule attackRule /* 처리에 사용할 공격 규칙 */)
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

            // 치명타 허용 공격만 모든 최종 배율 뒤에 치명타를 판정한다.
            if (attackRule.CriticalAllowed && attackRule.Source != null)
            {
                var criticalChance = attackRule.Source.Stats.CriticalChance;
                criticalChance += StatusCombatRules.CriticalChanceBonus(attackRule.Source);
                criticalChance += attackRule.Source.SkillState.PassiveCriticalChanceBonus();
                criticalChance += attackRule.CritChanceBonus;
                criticalChance -= target.Stats.CriticalResistance;
                criticalChance -= StatusCombatRules.CriticalResistanceBonus(target);

                if (UnityEngine.Random.value < Mathf.Clamp01(criticalChance)) //치명타 성공
                {
                    var criticalDamage = attackRule.Source.Stats.CriticalDamage;
                    criticalDamage += StatusCombatRules.CriticalDamageBonus(attackRule.Source);
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
