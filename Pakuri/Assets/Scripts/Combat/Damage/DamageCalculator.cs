using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

/*
 * 시전자의 능력치와 스킬 데이터로 원본 피해량을 계산한다.
 * 대상의 방어력, 상태 효과, 치명타와 적 패시브를 반영해 최종 피해량을 반환한다.
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
     * 공격자의 능력치로 원본 피해량을 만들고 대상 조건으로 최종 피해량을 계산한다.
     */
    public static class DamageCalculator
    {
        /*
         * 공격력 계수, 스킬 강화와 공격자의 주는 피해 보정으로 원본 피해량을 계산한다.
         */
        internal static float CalculateRawDamage(
            UnitCombatState caster, /* 스킬을 사용하는 유닛 */
            SkillDamageSpec damage /* 피해량 */,
            float damageMultiplier /* 강화로 적용할 피해 배율 */)
        {
            var rawDamage = damage.BaseDamage;
            if (damage.UseCombinedStatCoefficients)
            {
                var attack = caster.Stats.AttackPower;
                attack *= StatusCombatRules.ResolveAttackPowerMultiplier(caster);
                var spell = caster.Stats.SpellPower;
                spell *= StatusCombatRules.ResolveSpellPowerMultiplier(caster);
                rawDamage += attack * damage.AttackPowerCoefficient;
                rawDamage += spell * damage.SpellPowerCoefficient;
            }
            else if (damage.StatSource == StatSource.Attack)
            {
                var attack = caster.Stats.AttackPower;
                attack *= StatusCombatRules.ResolveAttackPowerMultiplier(caster);
                rawDamage += attack * damage.StatCoefficient;
            }
            else
            {
                var spell = caster.Stats.SpellPower;
                spell *= StatusCombatRules.ResolveSpellPowerMultiplier(caster);
                rawDamage += spell * damage.StatCoefficient;
            }
            rawDamage = Mathf.Max(0f, rawDamage);

            rawDamage *= damageMultiplier;

            rawDamage *= StatusCombatRules.ResolveOutgoingDamageMultiplier(caster, damage.Element, damage.SkillId);
            if (caster is EnemyCombatState enemy)
            {
                rawDamage *= EnemyPassiveModifiers.ResolveOutgoingDamageMultiplier(
                    enemy,
                    damage.Element);
            }

            return rawDamage;
        }

        /*
         * 대상의 방어력, 상태 효과, 치명타와 적 패시브를 반영해 최종 피해량을 계산한다.
         */
        public static float CalculateFinalDamage(
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            float rawDamage /* 원본 피해량 */,
            DamageAttribute attribute /* 피해 속성 */,
            DamageApplicationOptions options /* 처리에 사용할 추가 설정 */)
        {
            var damage = Mathf.Max(0f, rawDamage);
            var defense = target.Defenses.Get(attribute);
            defense -= StatusCombatRules.ResolveFlatElementResistReduction(target, attribute);
            var defenseReduction = StatusCombatRules.ResolveElementResistReduction(target, attribute);
            defense *= 1f - defenseReduction;
            defense = Mathf.Max(0f, defense);
            damage *= 100f / (100f + defense);

            if (options.CriticalAllowed)
            {
                var criticalChance = options.Source.Stats.CriticalChance;
                criticalChance += StatusCombatRules.ResolveCriticalChanceBonus(options.Source);
                criticalChance += options.CritChanceBonus;
                criticalChance -= target.Stats.CriticalResistance;
                criticalChance -= StatusCombatRules.ResolveCriticalResistanceBonus(target);

                if (UnityEngine.Random.value < Mathf.Clamp01(criticalChance))
                {
                    var criticalDamage = options.Source.Stats.CriticalDamage;
                    criticalDamage += StatusCombatRules.ResolveCriticalDamageBonus(options.Source);
                    criticalDamage += options.CritDamageBonus;
                    criticalDamage += StatusCombatRules.ResolveCriticalDamageTakenBonus(target);
                    damage *= criticalDamage;
                }
            }

            var incomingDamageMultiplier = StatusCombatRules.ResolveIncomingDamageMultiplier(
                target,
                options.Source,
                attribute,
                options.SourceSkillId);
            if (target is EnemyCombatState enemy)
            {
                incomingDamageMultiplier *= Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier);
            }

            damage *= incomingDamageMultiplier;
            return Mathf.Round(Mathf.Max(0f, damage));
        }

    }
}
