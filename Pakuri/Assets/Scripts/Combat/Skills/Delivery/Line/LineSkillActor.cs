using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 인게임 직선 공격의 위치, 충돌, 수명 주기를 처리한다.
 */
namespace Pakuri.InGame
{
    public class LineSkillActor : MonoBehaviour
    {
        // 생성된 직선 공격의 충돌 판정, 주기 피해, 상태, 수명을 구현.
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

        /*
         * 인게임 직선 공격 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
        public void Initialize(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            UnitSpawnManager unitRoster /* 전투에 등록된 유닛 목록 */,
            SkillTargetingSpec targetingSpec /* 스킬 대상 선택 설정 */,
            Vector2 lineOrigin /* 직선 시작 위치 */,
            Vector2 lineDirection /* 직선 방향 */,
            float lineLength /* 직선 길이 */,
            float lineWidth /* 직선 너비 */,
            float lineKnockbackDistance /* 직선 밀쳐내기 거리 */,
            float durationSeconds /* 지속 시간(초) */,
            float tickIntervalSeconds /* 반복 적용 간격(초) */,
            float damagePerTick /* 피해 개별 반복 적용 */,
            DamageAttribute damageAttribute /* 적용할 피해 속성 */,
            ProjectileStatusHitSpec onHitStatus /* 발생 시 적중 상태 효과 */,
            SkillUseState sourceRuntime /* 효과를 발생시킨 스킬 실행 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string skillId /* 스킬 식별자 */,
            bool allowCritical /* 허용 치명타 여부 */,
            float criticalChanceBonus /* 치명타 확률 추가값 */,
            float criticalDamageBonus /* 치명타 피해 추가값 */)
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

        /*
         * 피해 처리가 없는 직선 비주얼의 지정 수명을 설정하고 반환한다.
         */
        public float InitializeVisualLifetime(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            float durationSeconds /* 지속 시간(초) */)
        {
            effectManager = manager;
            visualOnly = true;
            remainingDuration = Mathf.Max(0.05f, durationSeconds);
            return remainingDuration;
        }

        /*
         * 직선 주기를 적용한다.
         */
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

        /*
         * 인게임 직선 공격의 이동, 수명, 주기 처리를 매 프레임 갱신한다.
         */
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

        /*
         * 직선 공격 비주얼과 히트박스 크기를 설정한다.
         */
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

        /*
         * 직선 공격의 실제 BoxCollider 크기를 월드 길이와 너비에 맞춘다.
         */
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

        /*
         * 밀쳐내기를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyKnockback(CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */, Vector2 normalizedDirection /* 정규화된 방향 */, float distance /* 거리 */)
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

        /*
         * 상태를 적용하고 성공 여부를 반환한다.
         */
        private static void TryApplyStatus(
            InGameCombatManager manager /* 전투 진행 관리자 */,
            UnitCombatState target /* 효과를 받을 대상 유닛 */,
            ProjectileStatusHitSpec status /* 적용하거나 검사할 상태 효과 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            string targetKey /* 대상 조회 키 */,
            HashSet<string> appliedTargets /* 적용된 대상 목록 */)
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

        /*
         * 적중 효과를 적용하고 성공 여부를 반환한다.
         */

        /*
         * 대상 키를 결정한다.
         */
        private static string TargetKey(UnitCombatState target /* 효과를 받을 대상 유닛 */)
        {
            var unitId = target != null && target.Identity != null ? target.Identity.UnitId : null;
            if (!string.IsNullOrWhiteSpace(unitId))
            {
                return unitId;
            }

            return target != null ? target.GetHashCode().ToString() : string.Empty;
        }

        /*
         * 효과 대상 키를 구성한다.
         */
    }
}
