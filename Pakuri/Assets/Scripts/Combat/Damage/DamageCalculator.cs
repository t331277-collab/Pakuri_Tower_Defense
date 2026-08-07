/*
 * 역할: 전투 피해량 계산.
 * 시전자 보너스·피해 속성·대상 방어력을 반영해 기본 피해와 최종 피해를 계산한다.
 */

using Pakuri.Data;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Combat
{
    public static class DamageCalculator
    {
        /// 기본 데미지를 계산 후 반환

        public static float CalculateRawDamage(
            UnitCombatState caster,
            SkillDamageSpec damage)
        {
            var attack = caster.GetAttackPower() * StatusCombatRules.AttackPowerMultiplier(caster);
            var spell = caster.GetSpellPower() * StatusCombatRules.SpellPowerMultiplier(caster);
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
            return CalculateFinalDamage(target, rawDamage, attribute, attackRule, out _);
        }

        public static float CalculateFinalDamage(
            UnitCombatState target,
            float rawDamage,
            DamageAttribute attribute,
            AttackRule attackRule,
            out bool isCritical)
        {
            isCritical = false;
            var damage = rawDamage;
            var artifactModifiers = ArtifactCombatRules.Resolve(target);
            var defense = target.Defenses.Get(attribute);    // 피해 속성에 대응하는 대상의 기본 방어력
            defense *= target.SkillState.PassiveDefenseMultiplier(attribute);    // 학습한 DefenseUp 패시브의 방어력 증가 배율 적용
            defense *= Mathf.Max(0f, 1f + artifactModifiers.DefenseBonusRate);
            defense += artifactModifiers.FlatDefenseBonus;
            defense *= StatusCombatRules.ElementResistMultiplier(target, attribute);  // 활성 상태 효과의 속성 저항 감소율을 순차 곱셈
            defense -= StatusCombatRules.FlatElementResistReduction(target, attribute); // 퍼센트 증감 후 고정 속성 방어력 감소량 차감
            damage *= 100f / Mathf.Max(0.01f, 100f + defense);

            var outgoingDamageMultiplier = attackRule.DamageMultiplier; // 스킬에 붙는 피해 배율
            outgoingDamageMultiplier *= StatusCombatRules.OutgoingDamageMultiplier( // 공격자가 가진 상태 효과의 주는 피해 배율
                attackRule.Source,
                target,
                attribute,
                attackRule.SourceSkillName);
            if (attackRule.Source != null)
            {
                outgoingDamageMultiplier *= attackRule.Source.SkillState.PassiveOutgoingDamageMultiplier(attribute);
            }
            damage *= Mathf.Max(0f, outgoingDamageMultiplier);

            var incomingDamageMultiplier = StatusCombatRules.IncomingDamageMultiplier( // 대상이 가진 상태 효과의 받는 피해 배율
                target,
                attackRule.Source,
                attribute,
                attackRule.SourceSkillName);
            incomingDamageMultiplier *= target.SkillState.PassiveIncomingDamageMultiplier(); // 대상이 가진 스킬 학습에 따른 받는 피해 배율
            damage *= Mathf.Max(0f, incomingDamageMultiplier);

            if (attackRule.CriticalAllowed && attackRule.Source != null)
            {
                var criticalChance = ResolveCriticalChance(target, attackRule);
                if (UnityEngine.Random.value < criticalChance)
                {
                    damage *= ResolveCriticalDamageMultiplier(target, attackRule);
                    isCritical = true;
                }
            }

            damage *= Mathf.Max(0f, attackRule.FinalDamageModifier);
            if (isCritical)
            {
                damage *= Mathf.Max(0f, attackRule.CriticalFinalDamageModifier);
            }
            damage *= artifactModifiers.FinalDamageTakenMultiplier;

            return Mathf.Round(Mathf.Max(0f, damage));
        }

        public static float ResolveCriticalChance(UnitCombatState target, AttackRule attackRule)
        {
            if (attackRule.Source == null)
            {
                return 0f;
            }

            var criticalChance = attackRule.Source.Stats.CriticalChance;
            criticalChance += StatusCombatRules.CriticalChanceBonus(attackRule.Source, target);
            criticalChance += attackRule.Source.SkillState.PassiveCriticalChanceBonus();
            criticalChance += attackRule.CritChanceBonus;
            return Mathf.Clamp01(criticalChance);
        }

        public static float ResolveCriticalDamageMultiplier(UnitCombatState target, AttackRule attackRule)
        {
            if (attackRule.Source == null)
            {
                return 1f;
            }

            var multiplier = Mathf.Max(0f, attackRule.Source.Stats.CriticalDamage);
            multiplier *= StatusCombatRules.CriticalDamageMultiplier(attackRule.Source, target);
            multiplier *= attackRule.Source.SkillState.PassiveCriticalDamageMultiplier();
            multiplier *= Mathf.Max(0f, 1f + attackRule.CritDamageBonus);
            multiplier *= StatusCombatRules.CriticalDamageTakenMultiplier(target);
            return Mathf.Max(0f, multiplier);
        }

    }
}
