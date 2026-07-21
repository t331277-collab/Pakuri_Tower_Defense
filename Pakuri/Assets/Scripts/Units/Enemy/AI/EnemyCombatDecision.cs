using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 적이 공격할 대상과 사용할 스킬을 결정한다.
 * 실제 이동과 스킬 실행은 하지 않고 선택 결과만 반환한다.
 */
namespace Pakuri.InGame
{
    internal static class EnemyCombatDecision
    {
        /*
         * 가장 가까운 일반 플레이어를 찾고 없으면 넥서스를 반환한다.
         */
        public static CombatUnitEntry FindNearestPlayerTarget(CombatUnitEntry enemyEntry, CombatUnitRegistry registry)
        {
            var best = FindNearestPlayerTarget(enemyEntry, registry, includeNexus: false);
            if (best != null)
            {
                return best;
            }

            // 일반 플레이어가 없을 때만 넥서스를 대상으로 선택한다.
            return FindNearestPlayerTarget(enemyEntry, registry, includeNexus: true);
        }

        /*
         * 넥서스 포함 여부가 일치하는 살아 있는 플레이어 중 가장 가까운 대상을 찾는다.
         */
        private static CombatUnitEntry FindNearestPlayerTarget(
            CombatUnitEntry enemyEntry,
            CombatUnitRegistry registry,
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
                // 1차 검색은 일반 플레이어만, 2차 검색은 넥서스만 통과시킨다.
                if (isNexus != includeNexus)
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                // 전투 거리는 XY 평면만 사용한다.
                offset.z = 0f;
                // 제곱 거리는 제곱근 계산 없이 같은 근접 순서를 비교할 수 있다.
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

        /*
         * 체력이 감소한 살아 있는 적 유닛 중 체력 비율이 가장 낮은 아군을 찾는다.
         */
        public static CombatUnitEntry FindLowestHealthEnemyAlly(CombatUnitRegistry registry)
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
                // 최대 체력이 없는 유닛은 비율 비교에서 제외한다.
                if (stats.MaxHealth <= 0f)
                {
                    continue;
                }

                var healthRatio = Mathf.Clamp01(resources.CurrentHealth / stats.MaxHealth);
                // 체력이 가득 찼거나 현재 후보보다 건강한 유닛은 제외한다.
                // 같은 체력 비율이면 등록소에서 먼저 발견한 유닛을 유지한다.
                if (healthRatio >= 1f || healthRatio >= bestHealthRatio)
                {
                    continue;
                }

                best = candidate;
                bestHealthRatio = healthRatio;
            }

            return best;
        }

        /*
         * 실행 가능한 특수 공격을 우선하고, 없으면 기본 공격을 선택한다.
         */
        public static SkillRuntimeInstance ResolveOffensiveSkill(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            SkillRuntimeInstance specialRuntime,
            bool canUseSpecialSkill,
            SkillExecution skillExecution,
            CombatUnitRegistry registry)
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

        /*
         * 지정 슬롯의 스킬을 찾고 전투 시작 전용 스킬은 일반 행동에서 제외한다.
         */
        public static SkillRuntimeInstance ResolveSelectableSkill(EnemyCombatState enemyModel, SkillSlot slot)
        {
            var runtime = enemyModel.SkillRuntime.FindBySlot(slot);
            if (HasCombatStartTrigger(runtime))
            {
                return null;
            }

            return runtime;
        }

        public static bool IsSupportSkill(SkillRuntimeInstance runtime)
        {
            return runtime != null && runtime.Data.Targeting.TargetSide != SkillTargetSide.Enemy;
        }

        /*
         * 회복 스킬은 체력이 감소한 적 아군이 있을 때만 사용한다.
         */
        public static bool CanExecuteSupportSkill(SkillRuntimeInstance runtime, CombatUnitRegistry registry)
        {
            if (runtime.Data is BuffHealSkillRuntimeData)
            {
                return FindLowestHealthEnemyAlly(registry) != null;
            }

            return true;
        }

        private static bool HasCombatStartTrigger(SkillRuntimeInstance runtime)
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
