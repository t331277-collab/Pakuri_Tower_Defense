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

    /// 적의 대상 선택·이동·스킬 사용과 Nexus 공격을 전투 상태에 맞춰 진행한다.
    public class EnemyActionController
    {
        private readonly UnitSpawnManager units;
        private readonly SkillExecution skillExecution;
        private readonly InGameCombatManager combatManager;
        private readonly List<CombatUnitEntry> nexusCandidate = new List<CombatUnitEntry>(1);
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>(1);

        public EnemyActionController(
            UnitSpawnManager units,
            SkillExecution skillExecution,
            InGameCombatManager combatManager)
        {
            this.units = units;
            this.skillExecution = skillExecution;
            this.combatManager = combatManager;
        }

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

            var target = EnemyCombatDecision.FindNearestPlayerTarget(enemyEntry, units);
            if (target != null && target.Model.IsNexus)
            {
                TickNexusAttack(enemyEntry, enemyModel, target, deltaTime);
                return;
            }

            var activeCharge = EnemyCombatDecision.ResolveActiveCharge(enemyModel);
            if (activeCharge != null)
            {
                TickCharge(enemyEntry, enemyModel, target, activeCharge, deltaTime);
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

        private bool TryUseSkill(
            CombatUnitEntry enemyEntry,
            SkillExecutionState runtime)
        {
            return skillExecution.TryExecuteSelected(
                enemyEntry,
                runtime,
                units,
                combatManager);
        }

        private static Vector2 MoveToward(
            CombatUnitEntry enemyEntry,
            CombatUnitEntry target,
            EnemyCombatState enemyModel,
            float deltaTime,
            float additionalSpeedMultiplier = 1f)
        {
            var moveSpeed = enemyModel.Stats.MoveSpeed;
            moveSpeed *= StatusCombatRules.MoveSpeedMultiplier(enemyModel);
            moveSpeed *= Mathf.Max(0f, additionalSpeedMultiplier);
            if (moveSpeed <= 0f)
            {
                return Vector2.zero;
            }

            var current = enemyEntry.Transform.position;
            var targetPosition = target.Transform.position;
            targetPosition.z = current.z;
            enemyEntry.Transform.position = Vector3.MoveTowards(
                current,
                targetPosition,
                moveSpeed * deltaTime);
            return (Vector2)(enemyEntry.Transform.position - current);
        }

        /// 일반 AI 대상과 이동 경로를 사용해 활성 Charge 버프를 갱신한다.
        private void TickCharge(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            CombatUnitEntry target,
            SkillExecutionState runtime,
            float deltaTime)
        {
            if (TryApplyChargeContact(enemyEntry, enemyModel, runtime, Vector2.zero)
                || target == null
                || !StatusCombatRules.CanMove(enemyModel))
            {
                return;
            }

            var movement = MoveToward(
                enemyEntry,
                target,
                enemyModel,
                deltaTime,
                ChargeSpeedMultiplier(runtime));
            TryApplyChargeContact(enemyEntry, enemyModel, runtime, -movement);
        }

        /// Charge 이동 배율을 활성 경과 시간에 맞춰 계산한다.
        private static float ChargeSpeedMultiplier(SkillExecutionState runtime)
        {
            var snapshot = runtime.ActiveExecutionData;
            if (snapshot == null)
            {
                return 1f;
            }

            var activeDuration = runtime.Data != null && runtime.Data.Timing != null
                ? runtime.Data.Timing.ActiveDuration
                : 0f;
            var elapsed = Mathf.Max(0f, activeDuration - runtime.ActiveDurationRemaining);
            var ramp = snapshot.PreparedChargeRampSeconds > 0f
                ? Mathf.Clamp01(elapsed / snapshot.PreparedChargeRampSeconds)
                : 1f;
            return Mathf.Lerp(
                1f,
                snapshot.PreparedChargeMaxMoveSpeedMultiplier,
                ramp);
        }

        /// Charge 이동 중 플레이어 접촉을 공통 Collider 경로로 판정한다.
        private bool TryApplyChargeContact(
            CombatUnitEntry enemyEntry,
            EnemyCombatState enemyModel,
            SkillExecutionState runtime,
            Vector2 movement)
        {
            var candidates = SkillTargeting.TargetList(
                enemyEntry,
                units,
                runtime?.Data?.Targeting);
            UnitCollisionResolver.CollectTargets(
                units,
                candidates,
                enemyEntry,
                movement,
                collisionTargets);
            if (collisionTargets.Count == 0)
            {
                return false;
            }

            var snapshot = runtime != null ? runtime.ActiveExecutionData : null;
            if (snapshot == null)
            {
                return false;
            }

            var target = collisionTargets[0];
            var maxHealth = target.Model.Stats != null
                ? Mathf.Max(0f, target.Model.Stats.MaxHealth)
                : 0f;
            var damageResult = combatManager.ApplyDamage(
                target.Model,
                maxHealth * snapshot.PreparedChargeTargetMaxHealthRatio,
                snapshot.PreparedDamageAttribute,
                enemyModel,
                true,
                sourceSkillName: !string.IsNullOrWhiteSpace(snapshot.PreparedSkillName)
                    ? snapshot.PreparedSkillName
                    : runtime.SkillName);
            if (!damageResult.IsDead && snapshot.PreparedStatus != null)
            {
                StatusCombatRules.ApplyStatus(
                    combatManager,
                    target.Model,
                    snapshot.PreparedStatus,
                    enemyModel);
            }

            SkillExecution.StopActive(runtime);
            return true;
        }

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
