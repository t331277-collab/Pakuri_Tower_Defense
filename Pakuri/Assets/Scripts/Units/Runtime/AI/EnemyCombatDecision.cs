/*
 * 역할: 적 대상 및 스킬 판단.
 * 책임: 플레이어 또는 아군 대상을 선택하고 유효한 공격·지원·전투 시작 스킬을 결정한다.
 */

using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    internal static class EnemyCombatDecision
    {

        public static CombatUnitEntry FindNearestPlayerTarget(CombatUnitEntry enemyEntry, UnitSpawnManager registry)
        {
            var best = FindNearestPlayerTarget(enemyEntry, registry, includeNexus: false);
            if (best != null)
            {
                return best;
            }

            return FindNearestPlayerTarget(enemyEntry, registry, includeNexus: true);
        }

        private static CombatUnitEntry FindNearestPlayerTarget(
            CombatUnitEntry enemyEntry,
            UnitSpawnManager registry,
            bool includeNexus)
        {
            var players = registry.Players;
            CombatUnitEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = enemyEntry.Transform.position;
            for (var i = 0; i < players.Count; i++)
            {
                var candidate = players[i];
                if (!candidate.IsAlive)
                {
                    continue;
                }

                var isNexus = candidate.Model.IsNexus;

                if (isNexus != includeNexus)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;

                offset.z = 0f;

                var distanceSq = offset.sqrMagnitude;
                if (distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        public static CombatUnitEntry FindLowestHealthEnemyAlly(UnitSpawnManager registry)
        {
            var enemies = registry.Enemies;
            CombatUnitEntry best = null;
            var bestHealthRatio = float.MaxValue;

            for (var i = 0; i < enemies.Count; i++)
            {
                var candidate = enemies[i];
                if (!candidate.IsAlive)
                {
                    continue;
                }

                var resources = candidate.Model.Resources;
                var stats = candidate.Model.Stats;

                if (stats.MaxHealth <= 0f)
                {
                    continue;
                }

                var healthRatio = Mathf.Clamp01(resources.CurrentHealth / stats.MaxHealth);

                if (healthRatio >= 1f || healthRatio >= bestHealthRatio)
                {
                    continue;
                }

                best = candidate;
                bestHealthRatio = healthRatio;
            }

            return best;
        }

        public static SkillExecutionState ResolveOffensiveSkill(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            SkillExecutionState specialRuntime,
            bool canUseSpecialSkill,
            SkillExecution skillExecution,
            UnitSpawnManager registry)
        {
            if (specialRuntime != null
                && canUseSpecialSkill
                && !IsSupportSkill(specialRuntime)
                && skillExecution.CanExecuteSelected(enemyEntry, specialRuntime, registry))
            {
                return specialRuntime;
            }

            var basicRuntime = ResolveSelectableSkill(enemyModel, SkillSlot.A);
            if (basicRuntime != null
                && !IsSupportSkill(basicRuntime)
                && skillExecution.CanExecuteSelected(enemyEntry, basicRuntime, registry))
            {
                return basicRuntime;
            }

            return null;
        }

        public static SkillExecutionState ResolveSelectableSkill(EnemyCombatState enemyModel, SkillSlot slot)
        {
            var runtime = enemyModel.SkillState.FindBySlot(slot);
            if (HasCombatStartTrigger(runtime))
            {
                return null;
            }

            return runtime;
        }

        /// 활성화된 Charge 버프 런타임을 반환한다.
        public static SkillExecutionState ResolveActiveCharge(EnemyCombatState enemyModel)
        {
            var activeSkills = enemyModel.SkillState.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime != null
                    && runtime.IsActive
                    && runtime.Data is BuffSkillDefinition buff
                    && buff.EffectKind == BuffEffectKind.Charge)
                {
                    return runtime;
                }
            }

            return null;
        }

        public static bool IsSupportSkill(SkillExecutionState runtime)
        {
            return runtime != null && runtime.Data.Targeting.TargetSide != SkillTargetSide.Enemy;
        }

        public static bool CanExecuteSupportSkill(SkillExecutionState runtime, UnitSpawnManager registry)
        {
            if (runtime.Data is BuffSkillDefinition buff
                && buff.EffectKind == BuffEffectKind.Heal)
            {
                return FindLowestHealthEnemyAlly(registry) != null;
            }

            return true;
        }

        private static bool HasCombatStartTrigger(SkillExecutionState runtime)
        {
            if (runtime == null)
            {
                return false;
            }

            var reactions = SkillExecutionRules.CreateDefinitionSnapshot(runtime.Data).Reactions;
            for (var i = 0; i < reactions.Count; i++)
            {
                if (reactions[i].Event == SkillTriggerEvent.CombatStart)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
