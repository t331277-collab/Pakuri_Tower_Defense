/*
 * 역할: 직선형 공격의 실제 적중을 진행한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    public class LineSkillActor : MonoBehaviour
    {

        private InGameCombatManager combatManager;
        private EffectManager effectManager;
        private CombatUnitEntry casterEntry;
        private UnitSpawnManager roster;
        private SkillTargetingSpec targeting;
        private Vector2 origin;
        private Vector2 direction = Vector2.right;
        private float length;
        private float width;
        private float knockbackDistance;
        private float remainingDuration;
        private float tickInterval;
        private float tickRemaining;
        private float damage;
        private DamageAttribute attribute;
        private StatusApplicationSpec statusSpec;
        private SkillExecutionState runtime;
        private SkillExecutionState executionData;
        private UnitCombatState sourceModel;
        private string sourceSkillId;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private bool visualOnly;
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>();
        private readonly Collider2D[] lineHitboxes = new Collider2D[1];
        private BoxCollider2D lineHitbox;

        /// 확정된 직선 영역과 적중 기준으로 첫 판정을 시작한다.
        public void Initialize(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 lineOrigin,
            Vector2 lineDirection,
            float lineLength,
            float lineWidth,
            float lineKnockbackDistance,
            float durationSeconds,
            float tickIntervalSeconds,
            float damagePerTick,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            SkillExecutionState sourceRuntime,
            SkillExecutionState snapshot,
            UnitCombatState source,
            string skillId,
            bool allowCritical,
            float criticalChanceBonus,
            float criticalDamageBonus)
        {
            combatManager = manager;
            effectManager = manager.Effects;
            visualOnly = false;
            casterEntry = sourceEntry;
            roster = unitRoster;
            targeting = targetingSpec;
            origin = lineOrigin;
            direction = lineDirection.sqrMagnitude > 0.0001f ? lineDirection.normalized : Vector2.right;
            length = Mathf.Max(0.1f, lineLength);
            width = Mathf.Max(0.1f, lineWidth);
            knockbackDistance = Mathf.Max(0f, lineKnockbackDistance);
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            tickInterval = Mathf.Max(0.01f, tickIntervalSeconds);
            tickRemaining = tickInterval;
            damage = Mathf.Max(0f, damagePerTick);
            attribute = damageAttribute;
            statusSpec = onHitStatus;
            runtime = sourceRuntime;
            executionData = snapshot;
            sourceModel = source;
            sourceSkillId = skillId;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            EffectVisualBuilder.ConfigureLineEffect(gameObject, origin, direction, length, width);
            lineHitbox = EffectVisualBuilder.ConfigureLineHitbox(gameObject, length, width);
            lineHitboxes[0] = lineHitbox;
            ApplyLineTick();
        }

        /// 현재 직선과 겹친 대상이 받을 데미지를 InGameCombatManager 에 넘기고 후속 적중 효과를 적용한다.
        private bool ApplyLineTick()
        {
            if (combatManager == null || casterEntry == null || roster == null || lineHitbox == null)
            {
                return false;
            }

            var candidates = SkillTargeting.TargetList(casterEntry, roster, targeting);
            UnitCollisionResolver.CollectTargets(
                roster,
                candidates,
                lineHitboxes,
                Vector2.zero,
                collisionTargets);
            var routed = false;
            for (var i = 0; i < collisionTargets.Count; i++)
            {
                var target = collisionTargets[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                var hitPosition = (Vector2)target.Transform.position;
                var finalDamageMultiplier = SkillExecutionRules.ResolveHitDamageMultiplier(executionData, target.Model);
                var damageResult = combatManager.ApplyDamageWithTriggerState(target.Model, damage, attribute, sourceModel, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, false, false, null, finalDamageMultiplier, executionData != null ? executionData.TriggerExecutionState : null);
                TryApplyKnockback(target, direction, knockbackDistance);
                if (!damageResult.IsDead)
                {
                    StatusCombatRules.ApplyStatus(combatManager, target.Model, statusSpec, sourceModel);
                }
                ZoneSkillActor.PublishHitOutcome(
                    combatManager,
                    roster,
                    runtime,
                    executionData,
                    casterEntry,
                    sourceModel,
                    sourceSkillId,
                    target,
                    hitPosition,
                    damage);
                routed = true;
            }

            return routed;
        }

        /// 영역의 다음 판정 시점과 종료 시점을 진행한다.
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            remainingDuration -= deltaTime;
            if (!visualOnly)
            {
                tickRemaining -= deltaTime;
                if (remainingDuration > 0f && tickRemaining <= 0f)
                {
                    tickRemaining += tickInterval;
                    ApplyLineTick();
                }
            }

            if (remainingDuration <= 0f)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }

        /// 유효한 방향과 거리만 대상 위치 변화로 반영한다.
        private static void TryApplyKnockback(CombatUnitEntry target, Vector2 normalizedDirection, float distance)
        {
            if (target == null
                || target.Transform == null
                || distance <= 0f
                || normalizedDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            target.Transform.position += (Vector3)(normalizedDirection.normalized * distance);
        }

    }
}
