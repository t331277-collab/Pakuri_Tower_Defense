/*
 * 역할: 런타임 Line Hit Actor 동작.
 * 책임: Line Hitbox를 이동·조절하고 Collider 접촉을 판정해 유효 적중을 스킬 실행에 전달한다.
 */

using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>LineSkillActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
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
        private ProjectileStatusHitSpec statusSpec;
        private SkillUseState runtime;
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
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
            ProjectileStatusHitSpec onHitStatus,
            SkillUseState sourceRuntime,
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

            ConfigureVisual();
            ConfigureHitbox();
            ApplyLineTick();
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>VisualLifetime</c>를 초기화한다.</summary>
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            return remainingDuration;
        }

        /// <summary><c>LineTick</c>를 적용한다.</summary>
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
                var finalDamageMultiplier = executionData != null
                    ? Mathf.Max(0f, executionData.DamageMultiplier) * SkillExecutionRuleResolver.ConditionalDamageMultiplier(executionData, target.Model)
                    : 1f;
                var damageResult = combatManager.ApplyDamage(target.Model, resolvedDamage, attribute, sourceModel, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, finalDamageMultiplier: finalDamageMultiplier);
                TryApplyKnockback(target, direction, knockbackDistance);
                if (!damageResult.IsDead)
                {
                    var targetKey = TargetKey(target.Model);
                    TryApplyStatus(combatManager, target.Model, statusSpec, sourceModel, targetKey, appliedBaseStatusTargets);
                }
                LineSkillExecutor.ApplyHitEnhancements(
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

        /// <summary>현재 Unity 프레임에서 <c>Update</c> 갱신 동작을 진행한다.</summary>
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

        /// <summary><c>ConfigureVisual</c> 작업을 수행한다.</summary>
        private void ConfigureVisual()
        {
            transform.position = origin + direction * (length * 0.5f);
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var size = spriteRenderer.sprite.bounds.size;
                var scale = transform.localScale;
                if (size.x > 0.0001f)
                {
                    scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (length / size.x);
                }

                if (size.y > 0.0001f)
                {
                    scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (width / size.y);
                }

                transform.localScale = scale;
            }
        }

        /// <summary><c>ConfigureHitbox</c> 작업을 수행한다.</summary>
        private void ConfigureHitbox()
        {
            lineHitbox = GetComponent<BoxCollider2D>();
            if (lineHitbox == null)
            {
                lineHitbox = gameObject.AddComponent<BoxCollider2D>();
            }

            var scale = transform.lossyScale;
            lineHitbox.size = new Vector2(
                length / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
                width / Mathf.Max(0.0001f, Mathf.Abs(scale.y)));
            lineHitbox.offset = Vector2.zero;
            lineHitbox.isTrigger = true;
            lineHitboxes[0] = lineHitbox;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ApplyKnockback</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ApplyStatus</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static void TryApplyStatus(
            InGameCombatManager manager,
            UnitCombatState target,
            ProjectileStatusHitSpec status,
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

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>TargetKey</c> 결과값을 생성해 반환한다.</summary>
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
