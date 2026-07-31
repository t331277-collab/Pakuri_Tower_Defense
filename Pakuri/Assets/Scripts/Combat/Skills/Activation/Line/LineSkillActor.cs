/*
 * 역할: 런타임 Line Hit Actor 동작.
 * 책임: Line Hitbox 판정·피해·상태·넉백·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// LineSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
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
        private SkillExecutionData runtime;
        private SkillExecutionData executionData;
        private UnitCombatState sourceModel;
        private string sourceSkillId;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private bool visualOnly;
        private readonly HashSet<string> appliedBaseStatusTargets = new HashSet<string>();
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>();
        private readonly Collider2D[] lineHitboxes = new Collider2D[1];
        private BoxCollider2D lineHitbox;

        /// 직선 영역의 범위, 지속과 적중 규칙을 시작한다.
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
            SkillExecutionData sourceRuntime,
            SkillExecutionData snapshot,
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
            appliedBaseStatusTargets.Clear();

            EffectVisualBuilder.ConfigureLineEffect(gameObject, origin, direction, length, width);
            lineHitbox = EffectVisualBuilder.ConfigureLineHitbox(gameObject, length, width);
            lineHitboxes[0] = lineHitbox;
            ApplyLineTick();
        }

        /// 직선 시각 효과의 수명을 정한다.
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            return remainingDuration;
        }

        /// 현재 틱의 직선 충돌과 피해를 실행한다.
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
                var resolvedDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier = SkillExecutionRuleResolver.ResolveHitDamageMultiplier(executionData, target.Model);
                var damageResult = combatManager.ApplyDamage(target.Model, resolvedDamage, attribute, sourceModel, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, finalDamageMultiplier: finalDamageMultiplier);
                TryApplyKnockback(target, direction, knockbackDistance);
                if (!damageResult.IsDead)
                {
                    var targetKey = TargetKey(target.Model);
                    TryApplyStatus(combatManager, target.Model, statusSpec, sourceModel, targetKey, appliedBaseStatusTargets);
                }
                SkillExecution.ApplyHitEnhancements(
                    combatManager,
                    roster,
                    runtime,
                    executionData,
                    casterEntry,
                    sourceModel,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
            }

            return routed;
        }

        /// 프레임 경과에 따라 직선 영역을 갱신한다.
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

        /// 적중 대상의 밀어내기를 적용한다.
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

        /// 직선 적중 상태를 공통 경로에 적용한다.
        private static void TryApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            StatusApplicationSpec status,
            UnitCombatState source,
            string targetKey,
            HashSet<string> appliedTargets)
        {
            if (status == null || !status.Enabled)
            {
                return;
            }

            if (appliedTargets != null && !string.IsNullOrWhiteSpace(targetKey) && appliedTargets.Contains(targetKey))
            {
                return;
            }

            if (StatusCombatRules.ApplyStatus(manager, target, status, source)
                && appliedTargets != null
                && !string.IsNullOrWhiteSpace(targetKey))
            {
                appliedTargets.Add(targetKey);
            }
        }

        /// 대상 중복 처리를 위한 식별자를 고른다.
        private static string TargetKey(UnitCombatState target)
        {
            var unitId = target != null && target.Identity != null ? target.Identity.UnitId : null;
            if (!string.IsNullOrWhiteSpace(unitId))
            {
                return unitId;
            }

            return target != null ? target.GetHashCode().ToString() : string.Empty;
        }

    }
}
