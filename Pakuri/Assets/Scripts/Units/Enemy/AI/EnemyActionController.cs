using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 등록소에 있는 적들의 매 프레임 행동을 조율하는 일반 C# 컨트롤러.
 * 상태 효과에 따른 행동 가능 여부를 확인하고 결정된 이동과 스킬 실행 순서를 조율한다.
 */
namespace Pakuri.InGame
{
    public class EnemyActionController
    {
        private readonly CombatUnitRegistry registry;
        private readonly SkillExecution skillExecution;
        private readonly InGameCombatManager combatManager;

        /*
         * 적 행동에 필요한 전투 시스템을 연결한다.
         */
        public EnemyActionController(
            CombatUnitRegistry registry,
            SkillExecution skillExecution,
            InGameCombatManager combatManager)
        {
            this.registry = registry;
            this.skillExecution = skillExecution;
            this.combatManager = combatManager;
        }

        /*
         * 살아 있는 모든 적의 행동을 한 프레임 갱신한다.
         */
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var enemies = registry.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                TickEnemy(enemies[i], deltaTime);
            }
        }

        /*
         * 한 적의 충전, 대상 선택, 지원 스킬, 이동, 공격 순서를 처리한다.
         */
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

            // 돌진 스킬이 행동을 점유한 프레임에는 일반 이동과 스킬 선택을 실행하지 않는다.
            if (SingleChargeActor.Tick(enemyEntry, registry, combatManager, deltaTime))
            {
                return;
            }

            var target = EnemyCombatDecision.FindNearestPlayerTarget(enemyEntry, registry);
            // 일반 플레이어가 모두 사라진 뒤 선택된 넥서스는 별도 접촉 공격으로 처리한다.
            if (target != null && target.Model.IsNexus)
            {
                EnemyNexusAttack.Tick(enemyEntry, enemyModel, target, deltaTime, combatManager);
                return;
            }

            var canAct = StatusCombatRules.CanAct(enemyModel);
            var canUseSpecialSkill = canAct && StatusCombatRules.CanUseSpecialSkill(enemyModel);
            var specialRuntime = EnemyCombatDecision.ResolveSelectableSkill(enemyModel, SkillSlot.B);
            // 사용 가능한 특수 지원 스킬은 공격 대상과 무관하게 적 아군 대상으로 먼저 시도한다.
            var usedSupportSkill = canUseSpecialSkill
                && EnemyCombatDecision.IsSupportSkill(specialRuntime)
                && EnemyCombatDecision.CanExecuteSupportSkill(specialRuntime, registry)
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
                registry);
            if (offensiveRuntime == null)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = EnemyCombatDecision.ResolveAttackRange(enemyModel, offensiveRuntime);
            // 사거리 밖에서는 공격하지 않고 이동만 시도한다.
            if (distance > attackRange)
            {
                if (StatusCombatRules.CanMove(enemyModel))
                {
                    EnemyMovement.MoveToward(enemyEntry, target, enemyModel, deltaTime);
                }

                return;
            }

            // 행동할 수 없거나 이미 지원 스킬을 사용했으면 공격을 생략한다.
            if (!canAct || usedSupportSkill)
            {
                return;
            }

            TryUseSkill(
                enemyEntry,
                offensiveRuntime);
        }

        /*
         * 선택된 스킬을 실행한다.
         */
        private bool TryUseSkill(
            CombatUnitEntry enemyEntry,
            SkillRuntimeInstance runtime)
        {
            return skillExecution.TryExecuteSelected(
                enemyEntry,
                runtime,
                registry,
                combatManager);
        }

    }
}
