using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 전투 로스터에 등록된 적들의 매 프레임 행동을 조율하는 일반 C# 컨트롤러.
 * 상태 효과에 따른 행동 가능 여부를 확인하고 대상 선택, 이동, 공격 스킬 우선순위,
 * 지원 스킬 사용과 넥서스 접촉 공격을 SkillExecutionSystem과 전투 Manager에 전달한다.
 */
namespace Pakuri.InGame
{
    public class EnemyController
    {
        private readonly UnitRosterService roster;
        private readonly SkillExecutionSystem skillExecution;
        private readonly InGameCombatManager combatManager;

        /*
         * 적 행동에 필요한 전투 시스템을 연결한다.
         */
        public EnemyController(
            UnitRosterService roster,
            SkillExecutionSystem skillExecution,
            InGameCombatManager combatManager)
        {
            this.roster = roster;
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

            var enemies = roster.Enemies;
            for (var i = 0; i < enemies.Count; i++)
            {
                TickEnemy(enemies[i], deltaTime);
            }
        }

        /*
         * 한 적의 충전, 대상 선택, 지원 스킬, 이동, 공격 순서를 처리한다.
         */
        private void TickEnemy(
            UnitRosterEntry enemyEntry,
            float deltaTime)
        {
            if (!EnemyTargeting.IsActive(enemyEntry))
            {
                return;
            }

            var enemyModel = (EnemyUnitRuntimeModel)enemyEntry.Model;
            if (!enemyModel.AutoAttackEnabled)
            {
                return;
            }

            // 돌진 스킬이 행동을 점유한 프레임에는 일반 이동과 스킬 선택을 실행하지 않는다.
            if (SharedChargeSkillRuntime.Tick(enemyEntry, roster, combatManager, deltaTime))
            {
                return;
            }

            var target = EnemyTargeting.FindNearestPlayerTarget(enemyEntry, roster);
            // 일반 플레이어가 모두 사라진 뒤 선택된 넥서스는 별도 접촉 공격으로 처리한다.
            if (target != null && EnemyTargeting.IsNexus(target))
            {
                TickNexusAssault(enemyEntry, enemyModel, target, deltaTime);
                return;
            }

            var canAct = StatusEffectRules.CanAct(enemyModel);
            var canUseSpecialSkill = canAct && StatusEffectRules.CanUseSpecialSkill(enemyModel);
            var specialRuntime = ResolveSelectableRuntime(enemyModel, SkillSlot.B);
            // 사용 가능한 특수 지원 스킬은 공격 대상과 무관하게 적 아군 대상으로 먼저 시도한다.
            var usedSupportSkill = canUseSpecialSkill
                && IsSupportSkill(specialRuntime)
                && CanExecuteSupportSkill(specialRuntime)
                && TryUseSkill(
                    enemyEntry,
                    specialRuntime,
                    deltaTime);

            if (target == null)
            {
                return;
            }

            var offensiveRuntime = ResolvePreferredOffensiveRuntime(
                enemyEntry,
                enemyModel,
                specialRuntime,
                canUseSpecialSkill);
            if (offensiveRuntime == null)
            {
                return;
            }

            var distance = Vector2.Distance(enemyEntry.Transform.position, target.Transform.position);
            var attackRange = ResolveAttackAttemptRange(enemyModel, offensiveRuntime);
            // 사거리 밖에서는 공격하지 않고 이동만 시도한다.
            if (distance > attackRange)
            {
                if (StatusEffectRules.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, target, enemyModel, deltaTime);
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
                offensiveRuntime,
                deltaTime);
        }

        /*
         * 실행 가능한 특수 공격을 우선하고 없으면 기본 공격을 선택한다.
         */
        private SkillRuntimeInstance ResolvePreferredOffensiveRuntime(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            SkillRuntimeInstance specialRuntime,
            bool canUseSpecialSkill)
        {
            // 사용 가능한 B 슬롯 공격 스킬을 우선 선택한다.
            if (specialRuntime != null
                && canUseSpecialSkill
                && !IsSupportSkill(specialRuntime)
                && skillExecution.CanExecuteSelected(enemyEntry, specialRuntime, roster))
            {
                return specialRuntime;
            }

            var basicRuntime = ResolveSelectableRuntime(enemyModel, SkillSlot.A);
            // B 슬롯을 쓸 수 없으면 실행 가능한 A 슬롯 공격 스킬을 선택한다.
            if (basicRuntime != null
                && !IsSupportSkill(basicRuntime)
                && skillExecution.CanExecuteSelected(enemyEntry, basicRuntime, roster))
            {
                return basicRuntime;
            }

            return null;
        }

        /*
         * 선택된 스킬을 실행한다.
         */
        private bool TryUseSkill(
            UnitRosterEntry enemyEntry,
            SkillRuntimeInstance runtime,
            float deltaTime)
        {
            return skillExecution.TryExecuteSelected(
                enemyEntry,
                runtime,
                roster,
                combatManager,
                deltaTime,
                false);
        }

        /*
         * 지정 슬롯의 스킬을 찾고 전투 시작 전용 스킬은 일반 행동에서 제외한다.
         */
        private static SkillRuntimeInstance ResolveSelectableRuntime(
            EnemyUnitRuntimeModel enemyModel,
            SkillSlot slot)
        {
            var runtime = enemyModel.SkillRuntime.FindBySlot(slot);
            return HasCombatStartTrigger(runtime) ? null : runtime;
        }

        /*
         * 스킬에 전투 시작 Trigger가 있는지 확인한다.
         */
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

        /*
         * 적 진영을 대상으로 하지 않는 스킬인지 확인한다.
         */
        private static bool IsSupportSkill(SkillRuntimeInstance runtime)
        {
            return runtime != null && runtime.Data.Targeting.TargetSide != SkillTargetSide.Enemy;
        }

        /*
         * 회복 스킬은 부상당한 적 아군이 있을 때만 사용하도록 제한한다.
         */
        private bool CanExecuteSupportSkill(SkillRuntimeInstance runtime)
        {
            // 회복 스킬만 부상당한 아군 존재 여부를 추가로 확인한다.
            return !(runtime.Data is HealSkillRuntimeData)
                || EnemyTargeting.FindLowestHealthEnemyAlly(roster) != null;
        }

        /*
         * 스킬 사거리를 우선하고 없으면 적 공격 유형의 기본 사거리를 반환한다.
         */
        private static float ResolveAttackAttemptRange(
            EnemyUnitRuntimeModel enemyModel,
            SkillRuntimeInstance runtime)
        {
            var targeting = runtime.Data.Targeting;
            if (targeting.Range > 0f)
            {
                return Mathf.Max(0.1f, targeting.Range);
            }

            // 스킬 설정에 사거리가 없을 때만 기존 공격 유형 기본값을 사용한다.
            switch (enemyModel.AttackType)
            {
                case EnemyAttackType.Ranged:
                case EnemyAttackType.Buffer:
                    return 5f;
                case EnemyAttackType.MeleeAndRanged:
                    return 4f;
                default:
                    return 1.4f;
            }
        }

        /*
         * 이동 속도와 상태 효과 배율을 반영해 적을 대상 쪽으로 이동시킨다.
         */
        private static void MoveToward(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry target,
            EnemyUnitRuntimeModel enemyModel,
            float deltaTime)
        {
            var moveSpeed = Mathf.Max(0f, enemyModel.Stats.MoveSpeed);
            moveSpeed *= StatusEffectRules.ResolveMoveSpeedMultiplier(enemyModel);
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

        /*
         * 적을 넥서스로 이동시키고 접촉하면 피해 적용 후 적을 제거한다.
         */
        private void TickNexusAssault(
            UnitRosterEntry enemyEntry,
            EnemyUnitRuntimeModel enemyModel,
            UnitRosterEntry nexusTarget,
            float deltaTime)
        {
            if (!IsTouchingNexus(enemyEntry, nexusTarget))
            {
                if (StatusEffectRules.CanMove(enemyModel))
                {
                    MoveToward(enemyEntry, nexusTarget, enemyModel, deltaTime);
                }

                return;
            }

            var damage = Mathf.Max(1f, enemyModel.NexusDamage);
            combatManager.ApplyDamage(
                nexusTarget.Model,
                damage,
                DamageAttribute.Physical,
                enemyModel,
                false);
            combatManager.DespawnUnit(enemyModel);
        }

        /*
         * 히트박스 겹침과 근접 거리로 넥서스 접촉 여부를 확인한다.
         */
        private static bool IsTouchingNexus(
            UnitRosterEntry enemyEntry,
            UnitRosterEntry nexusTarget)
        {
            var enemyPoint = enemyEntry.ResolveTargetPoint();
            var targetColliders = nexusTarget.GetHitboxColliders();
            // 적 중심점이 넥서스 히트박스 안에 들어왔는지 먼저 확인한다.
            for (var i = 0; i < targetColliders.Length; i++)
            {
                var collider = targetColliders[i];
                if (collider.enabled && collider.OverlapPoint(enemyPoint))
                {
                    return true;
                }
            }

            if (UnitHitboxUtility.IsTargetInsideHitbox(
                    enemyEntry.GetHitboxColliders(),
                    nexusTarget))
            {
                return true;
            }

            // 콜라이더 겹침을 잡지 못한 경우 짧은 중심 거리로 접촉을 보완한다.
            return Vector2.Distance(
                enemyEntry.Transform.position,
                nexusTarget.Transform.position) <= 0.25f;
        }

    }
}
