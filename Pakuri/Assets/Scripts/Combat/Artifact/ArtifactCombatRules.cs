/*
 * 역할: 활성 유물 Effect Node를 대상 전투 수치로 해석한다.
 */

using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public static class ArtifactCombatRules
    {
        public readonly struct Modifiers
        {
            internal Modifiers(
                float defenseBonusRate,
                float flatDefenseBonus,
                float finalDamageTakenMultiplier,
                float cooldownChargeSpeedBonus)
            {
                DefenseBonusRate = defenseBonusRate;
                FlatDefenseBonus = flatDefenseBonus;
                FinalDamageTakenMultiplier = finalDamageTakenMultiplier;
                CooldownChargeMultiplier = Mathf.Max(0f, 1f + cooldownChargeSpeedBonus);
            }

            public float DefenseBonusRate { get; }
            public float FlatDefenseBonus { get; }
            public float FinalDamageTakenMultiplier { get; }
            public float CooldownChargeMultiplier { get; }
        }

        public static float DefenseBonusRate(UnitCombatState target)
        {
            return Resolve(target).DefenseBonusRate;
        }

        public static float FlatDefenseBonus(UnitCombatState target)
        {
            return Resolve(target).FlatDefenseBonus;
        }

        public static float FinalDamageTakenMultiplier(UnitCombatState target)
        {
            return Resolve(target).FinalDamageTakenMultiplier;
        }

        public static float CooldownChargeMultiplier(UnitCombatState target)
        {
            return Resolve(target).CooldownChargeMultiplier;
        }

        public static Modifiers Resolve(UnitCombatState target)
        {
            var defenseBonusRate = 0f;
            var flatDefenseBonus = 0f;
            var finalDamageTakenMultiplier = 1f;
            var cooldownChargeSpeedBonus = 0f;
            var effectNames = target?.Artifacts?.ActiveArtifactEffectNames;
            if (effectNames == null)
            {
                return new Modifiers(0f, 0f, 1f, 0f);
            }

            for (var effectIndex = 0; effectIndex < effectNames.Count; effectIndex++)
            {
                if (!TryGetNodes(effectNames[effectIndex], out var nodes)
                    || !ConditionsMatch(nodes, target, null))
                {
                    continue;
                }

                for (var nodeIndex = 0; nodeIndex < nodes.Count; nodeIndex++)
                {
                    var node = nodes[nodeIndex];
                    var defense = node?.GetOperation<DefenseModifierOp>();
                    if (defense.HasValue)
                    {
                        defenseBonusRate += defense.Value.BonusRate;
                        flatDefenseBonus += defense.Value.FlatBonus;
                    }

                    var finalDamage = node?.GetOperation<FinalDamageTakenMultiplierOp>();
                    if (finalDamage.HasValue)
                    {
                        finalDamageTakenMultiplier *= Mathf.Max(0f, finalDamage.Value.Multiplier);
                    }

                    var cooldown = node?.GetOperation<CooldownChargeSpeedBonusOp>();
                    if (cooldown.HasValue)
                    {
                        cooldownChargeSpeedBonus += cooldown.Value.Bonus;
                    }
                }
            }

            return new Modifiers(
                defenseBonusRate,
                flatDefenseBonus,
                finalDamageTakenMultiplier,
                cooldownChargeSpeedBonus);
        }

        internal static bool ConditionsMatch(
            IReadOnlyList<SkillNode> nodes,
            UnitCombatState owner,
            SkillDefinition skill)
        {
            for (var i = 0; nodes != null && i < nodes.Count; i++)
            {
                var attribute = nodes[i]?.GetOperation<SkillAttributeConditionOp>();
                if (attribute.HasValue
                    && (skill == null || skill.Element != attribute.Value.Attribute))
                {
                    return false;
                }

                var status = nodes[i]?.GetOperation<SourceStatusConditionOp>();
                if (status.HasValue
                    && (owner == null
                        || (status.Value.StatusKind == StatusEffectKind.Shield
                            ? owner.GetTotalShield() <= 0f
                            : owner.Statuses.GetStacks(status.Value.StatusKind)
                                < status.Value.MinimumStacks)))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetNodes(
            string effectName,
            out IReadOnlyList<SkillNode> nodes)
        {
            nodes = null;
            var catalog = GameDataLoader.CurrentCatalog;
            if (catalog == null)
            {
                return false;
            }

            if (catalog.TryGetData(effectName, out ArtifactEffectDefinition effect)
                && effect != null)
            {
                nodes = effect.Nodes;
                return nodes != null;
            }

            if (catalog.TryGetData(
                    effectName,
                    out ArtifactSynergyEffectDefinition synergyEffect)
                && synergyEffect != null)
            {
                nodes = synergyEffect.Nodes;
                return nodes != null;
            }

            return false;
        }
    }
}
