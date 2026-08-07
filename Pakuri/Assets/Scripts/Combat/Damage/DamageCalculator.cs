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
            var attack = CalculateFinalAttackPower(caster);
            var spell = CalculateFinalSpellPower(caster);
            var rawDamage = damage.BaseDamage
                + attack * damage.AttackPowerCoefficient
                + spell * damage.SpellPowerCoefficient;
            return Mathf.Max(0f, rawDamage);
        }

        public static float CalculateFinalAttackPower(UnitCombatState source)
        {
            return source == null
                ? 0f
                : source.GetAttackPower() * StatusCombatRules.AttackPowerMultiplier(source);
        }

        public static float CalculateFinalSpellPower(UnitCombatState source)
        {
            return source == null
                ? 0f
                : source.GetSpellPower() * StatusCombatRules.SpellPowerMultiplier(source);
        }

        public static float CalculateFinalDefense(UnitCombatState target, DamageAttribute attribute)
        {
            if (target == null || target.Defenses == null)
            {
                return 0f;
            }

            var artifactModifiers = ArtifactCombatRules.Resolve(target);
            var defense = target.Defenses.Get(attribute);
            defense *= target.SkillState.PassiveDefenseMultiplier(attribute);
            defense *= Mathf.Max(0f, 1f + artifactModifiers.DefenseBonusRate);
            defense += artifactModifiers.FlatDefenseBonus;
            defense *= StatusCombatRules.ElementResistMultiplier(target, attribute);
            defense -= StatusCombatRules.FlatElementResistReduction(target, attribute);
            return defense;
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
            var defense = CalculateFinalDefense(target, attribute);
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

            return ResolveCriticalChance(
                attackRule.Source,
                target,
                attackRule.CritChanceBonus);
        }

        public static float CalculateFinalCriticalChance(UnitCombatState source)
        {
            return source == null
                ? 0f
                : ResolveCriticalChance(source, null, 0f);
        }

        public static float ResolveCriticalDamageMultiplier(UnitCombatState target, AttackRule attackRule)
        {
            if (attackRule.Source == null)
            {
                return 1f;
            }

            return ResolveCriticalDamageMultiplier(
                attackRule.Source,
                target,
                attackRule.CritDamageBonus);
        }

        public static float CalculateFinalCriticalDamageMultiplier(UnitCombatState source)
        {
            return source == null
                ? 1f
                : ResolveCriticalDamageMultiplier(source, null, 0f);
        }

        private static float ResolveCriticalChance(
            UnitCombatState source,
            UnitCombatState target,
            float critChanceBonus)
        {
            var criticalChance = source.Stats.CriticalChance;
            criticalChance += StatusCombatRules.CriticalChanceBonus(source, target);
            criticalChance += source.SkillState.PassiveCriticalChanceBonus();
            criticalChance += critChanceBonus;
            return Mathf.Clamp01(criticalChance);
        }

        private static float ResolveCriticalDamageMultiplier(
            UnitCombatState source,
            UnitCombatState target,
            float critDamageBonus)
        {
            var multiplier = Mathf.Max(0f, source.Stats.CriticalDamage);
            multiplier *= StatusCombatRules.CriticalDamageMultiplier(source, target);
            multiplier *= source.SkillState.PassiveCriticalDamageMultiplier();
            multiplier *= Mathf.Max(0f, 1f + critDamageBonus);
            multiplier *= StatusCombatRules.CriticalDamageTakenMultiplier(target);
            return Mathf.Max(0f, multiplier);
        }

    }
}
