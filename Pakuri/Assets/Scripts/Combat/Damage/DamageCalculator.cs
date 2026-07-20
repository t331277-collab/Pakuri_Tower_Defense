using System;
using Pakuri.InGame;
using UnityEngine;

/*
 * 전투에서 사용하는 피해 속성과 최종 피해 계산 규칙을 정의한다.
 * 기본 피해에 대상 방어력, 방어력 감소, 치명타, 상태 효과와 적 패시브 배율을 반영하고
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
            BaseUnitRuntimeModel target,
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
                    + StatusEffectRules.ResolveCriticalChanceBonus(options.Source);
                criticalDamage = sourceStats.CriticalDamage;
                criticalDamage += StatusEffectRules.ResolveCriticalDamageBonus(options.Source);
            }

            var criticalResistance = 0f;
            var criticalDamageTaken = 0f;
            if (criticalAllowed)
            {
                criticalResistance = target.Stats.CriticalResistance
                    + StatusEffectRules.ResolveCriticalResistanceBonus(target);
                criticalDamageTaken = StatusEffectRules.ResolveCriticalDamageTakenBonus(target);
            }

            // 공격자 치명타 보정과 대상의 방어·받는 피해 보정을 최종 계산기로 전달한다.
            var damage = Resolve(
                Mathf.Max(0f, baseDamage),
                attribute,
                CopyDefenses(target.Defenses),
                criticalAllowed,
                flatDefenseReduction: StatusEffectRules.ResolveFlatElementResistReduction(target, attribute),
                percentDefenseReductions: new[] { StatusEffectRules.ResolveElementResistReduction(target, attribute) },
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
        private static AttributeDefenseSet CopyDefenses(UnitDefenseRuntime defenses)
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
            BaseUnitRuntimeModel target,
            BaseUnitRuntimeModel source,
            DamageAttribute attribute,
            string sourceSkillId)
        {
            var statusMultiplier = StatusEffectRules.ResolveIncomingDamageMultiplier(
                target,
                source,
                attribute,
                sourceSkillId);
            var enemy = target as EnemyUnitRuntimeModel;
            if (enemy == null)
            {
                return statusMultiplier;
            }

            return Mathf.Max(0f, enemy.PassiveIncomingDamageMultiplier) * statusMultiplier;
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
