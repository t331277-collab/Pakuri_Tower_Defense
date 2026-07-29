/*
 * 역할: 적 대상 및 스킬 판단.
 * 책임: 플레이어 또는 아군 대상을 선택하고 유효한 공격·지원·전투 시작 스킬을 결정한다.
 */

using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>EnemyCombatDecision</c>가 담당하는 런타임 판단을 결정한다.</summary>
    internal static class EnemyCombatDecision
    {

        /// <summary>전달된 런타임 입력값을 사용해 <c>NearestPlayerTarget</c>를 찾는다.</summary>
        public static CombatUnitEntry FindNearestPlayerTarget(CombatUnitEntry enemyEntry, UnitSpawnManager registry)
        {
            var best = FindNearestPlayerTarget(enemyEntry, registry, includeNexus: false);
            if (best != null)
            {
                return best;
            }

            return FindNearestPlayerTarget(enemyEntry, registry, includeNexus: true);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>NearestPlayerTarget</c>를 찾는다.</summary>
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

        /// <summary>전달된 <c>registry</c> 값을 사용해 <c>LowestHealthEnemyAlly</c>를 찾는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>OffensiveSkill</c>를 결정한다.</summary>
        public static SkillUseState ResolveOffensiveSkill(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            SkillUseState specialRuntime,
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>SelectableSkill</c>를 결정한다.</summary>
        public static SkillUseState ResolveSelectableSkill(EnemyCombatState enemyModel, SkillSlot slot)
        {
            var runtime = enemyModel.SkillState.FindBySlot(slot);
            if (HasCombatStartTrigger(runtime))
            {
                return null;
            }

            return runtime;
        }

        /// <summary>전달된 <c>runtime</c> 값을 사용해 <c>SupportSkill</c> 조건 충족 여부를 반환한다.</summary>
        public static bool IsSupportSkill(SkillUseState runtime)
        {
            return runtime != null && runtime.Data.Targeting.TargetSide != SkillTargetSide.Enemy;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteSupportSkill</c> 실행 가능 여부를 반환한다.</summary>
        public static bool CanExecuteSupportSkill(SkillUseState runtime, UnitSpawnManager registry)
        {
            if (runtime.Data is BuffHealSkillDefinition)
            {
                return FindLowestHealthEnemyAlly(registry) != null;
            }

            return true;
        }

        /// <summary>전달된 <c>runtime</c> 값을 사용해 소유한 런타임 상태에 <c>CombatStartTrigger</c>가 있는지 반환한다.</summary>
        private static bool HasCombatStartTrigger(SkillUseState runtime)
        {
            if (runtime == null)
            {
                return false;
            }

            var triggers = runtime.Data.SkillTriggers;
            for (var i = 0; i < triggers.Length; i++)
            {
                if (triggers[i].TriggerEvent == SkillTriggerEvent.CombatStart)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
