/*
 * 역할: 적 런타임 행동 반복.
 * 책임: 적 이동·스킬 사용·대상 추적·Nexus 접촉·Nexus 공격을 갱신한다.
 */

using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>EnemyActionController</c>가 담당하는 입력 또는 표시 흐름을 조정하고 관련 런타임 상태를 갱신한다.</summary>
    public class EnemyActionController
    {
        private readonly UnitSpawnManager units;
        private readonly SkillExecution skillExecution;
        private readonly InGameCombatManager combatManager;
        private readonly List<CombatUnitEntry> nexusCandidate = new List<CombatUnitEntry>(1);
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>(1);

        /// <summary><c>EnemyActionController</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
        public EnemyActionController(
            UnitSpawnManager units,
            SkillExecution skillExecution,
            InGameCombatManager combatManager)
        {
            this.units = units;
            this.skillExecution = skillExecution;
            this.combatManager = combatManager;
        }

        /// <summary>전달된 <c>deltaTime</c> 값을 사용해 <c>요청값</c>를 경과 시간 기준으로 갱신한다.</summary>
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var enemies = units.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                TickEnemy(enemies[i], deltaTime);
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Enemy</c>를 경과 시간 기준으로 갱신한다.</summary>
        private void TickEnemy(
            CombatUnitEntry enemyEntry,
            float deltaTime)
        {
            if (!enemyEntry.IsAlive)
            {
                return;
            }

            var enemyModel = (EnemyCombatState)enemyEntry.Model;
            if (!enemyModel.AutoAttackEnabled)
            {
                return;
            }

            if (SingleChargeActor.Tick(enemyEntry, units, combatManager, deltaTime))
            {
                return;
            }

            var target = EnemyCombatDecision.FindNearestPlayerTarget(enemyEntry, units);

            if (target != null && target.Model.IsNexus)
            {
                TickNexusAttack(enemyEntry, enemyModel, target, deltaTime);
                return;
            }

            var canAct = StatusCombatRules.CanAct(enemyModel);
            var canUseSpecialSkill = canAct && StatusCombatRules.CanUseSpecialSkill(enemyModel);
            var specialRuntime = EnemyCombatDecision.ResolveSelectableSkill(enemyModel, SkillSlot.B);

            var usedSupportSkill = canUseSpecialSkill
                && EnemyCombatDecision.IsSupportSkill(specialRuntime)
                && EnemyCombatDecision.CanExecuteSupportSkill(specialRuntime, units)
                && TryUseSkill(
                    enemyEntry,
                    specialRuntime);

            if (target == null)
            {
                return;
            }

            var offensiveRuntime = EnemyCombatDecision.ResolveOffensiveSkill(
                enemyEntry,
                enemyModel,
                specialRuntime,
                canUseSpecialSkill,
                skillExecution,
                units);
            if (offensiveRuntime == null)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = offensiveRuntime.Data.Targeting.Range;

            if (distance > attackRange)
            {
                if (StatusCombatRules.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, target, enemyModel, deltaTime);
                }

                return;
            }

            if (!canAct || usedSupportSkill)
            {
                return;
            }

            TryUseSkill(
                enemyEntry,
                offensiveRuntime);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>UseSkill</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private bool TryUseSkill(
            CombatUnitEntry enemyEntry,
            SkillUseState runtime)
        {
            return skillExecution.TryExecuteSelected(
                enemyEntry,
                runtime,
                units,
                combatManager);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Toward</c>를 이동시킨다.</summary>
        private static void MoveToward(
            CombatUnitEntry enemyEntry,
            CombatUnitEntry target,
            EnemyCombatState enemyModel,
            float deltaTime)
        {
            var moveSpeed = enemyModel.Stats.MoveSpeed;
            moveSpeed *= StatusCombatRules.MoveSpeedMultiplier(enemyModel);
            if (moveSpeed <= 0f)
            {
                return;
            }

            var current = enemyEntry.Transform.position;
            var targetPosition = target.Transform.position;
            targetPosition.z = current.z;
            enemyEntry.Transform.position = Vector3.MoveTowards(
                current,
                targetPosition,
                moveSpeed * deltaTime);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>NexusAttack</c>를 경과 시간 기준으로 갱신한다.</summary>
        private void TickNexusAttack(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            CombatUnitEntry nexusTarget,
            float deltaTime)
        {
            if (!IsTouchingNexus(enemyEntry, nexusTarget))
            {
                if (StatusCombatRules.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, nexusTarget, enemyModel, deltaTime);
                }

                if (!IsTouchingNexus(enemyEntry, nexusTarget))
                {
                    return;
                }
            }

            combatManager.ApplyDamage(
                nexusTarget.Model,
                enemyModel.NexusDamage,
                DamageAttribute.Physical,
                enemyModel,
                false);
            units.DespawnUnit(enemyModel);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>TouchingNexus</c> 조건 충족 여부를 반환한다.</summary>
        private bool IsTouchingNexus(CombatUnitEntry enemyEntry, CombatUnitEntry nexusTarget)
        {
            nexusCandidate.Clear();
            nexusCandidate.Add(nexusTarget);
            UnitCollisionResolver.CollectTargets(
                units,
                nexusCandidate,
                enemyEntry,
                Vector2.zero,
                collisionTargets);
            return collisionTargets.Count > 0;
        }

    }
}
