/*
 * 역할: 투사체의 이동과 실제 적중을 진행한다.
 * 책임: 이동, 충돌, 관통, 피해, 상태, 충돌 후 효과와 수명을 처리한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 발사된 투사체가 사라질 때까지 이동과 충돌 결과를 전투에 반영한다.
    public class ProjectileSkillActor : MonoBehaviour
    {

        private readonly HashSet<string> hitUnitIds = new HashSet<string>();
        private readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>();

        private InGameCombatManager combatManager;
        private EffectManager effectManager;
        private UnitCombatState owner;
        private Vector2 direction = Vector2.right;
        private DamageAttribute damageAttribute = DamageAttribute.Physical;
        private float damage;
        private float speed;
        private float destroyBeyondX;
        private float maxLifetime;
        private int remainingHits = 1;
        private bool destroyWhenGreaterThanBoundary = true;
        private StatusApplicationSpec statusOnHit;
        private float branchDamageChance;
        private int branchDamageCount;
        private float branchDamageMultiplier = 1f;
        private float branchDamageSearchRadius;
        private SkillExecutionState runtime;
        private SkillExecutionState executionData;
        private string sourceSkillId;
        private bool isMagazineLastProjectile;
        private bool magazineLastProjectileTriggerFired;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private StatusApplicationSpec impactStatusOnHit;
        private bool contactDamageEnabled = true;
        private bool stopOnFirstHit;
        private float impactDelaySeconds;
        private RuntimeSkillVisualSpec impactRuntimeVisual;
        private bool hasImpactArea;
        private float impactRadius;
        private float impactDamage;
        private bool impactArmed;
        private float impactDelayRemaining;
        private Vector2 impactCenter;
        private UnitCombatState impactTarget;
        private bool impactResolved;
        private bool expirePublished;
        private bool visualOnly;
        private Collider2D[] hitboxColliders;

        /// 기본 이동과 충돌 횟수, 종료 기준을 초기화한다.
        public void Initialize(
            InGameCombatManager manager,
            UnitCombatState source,
            Vector2 fireDirection,
            float projectileSpeed,
            float baseDamage,
            DamageAttribute attribute,
            int pierceCount,
            float boundaryX,
            float lifetimeSeconds)
        {
            hitUnitIds.Clear();
            combatManager = manager;
            effectManager = manager.Effects;
            visualOnly = false;
            owner = source;
            direction = fireDirection.sqrMagnitude > 0.0001f ? fireDirection.normalized : Vector2.right;
            speed = Mathf.Max(0f, projectileSpeed);
            damage = Mathf.Max(0f, baseDamage);
            damageAttribute = attribute;
            remainingHits = Mathf.Max(1, pierceCount + 1);
            destroyBeyondX = boundaryX;
            maxLifetime = Mathf.Max(0.1f, lifetimeSeconds);
            destroyWhenGreaterThanBoundary = direction.x >= 0f;
            statusOnHit = null;
            branchDamageChance = 0f;
            branchDamageCount = 0;
            branchDamageMultiplier = 1f;
            branchDamageSearchRadius = 0f;
            runtime = null;
            executionData = null;
            sourceSkillId = null;
            isMagazineLastProjectile = false;
            magazineLastProjectileTriggerFired = false;
            criticalAllowed = false;
            critChanceBonus = 0f;
            critDamageBonus = 0f;
            impactStatusOnHit = null;
            contactDamageEnabled = true;
            stopOnFirstHit = false;
            impactDelaySeconds = 0f;
            impactRuntimeVisual = null;
            hasImpactArea = false;
            impactRadius = 0f;
            impactDamage = 0f;
            impactArmed = false;
            impactDelayRemaining = 0f;
            impactCenter = Vector2.zero;
            impactTarget = null;
            impactResolved = false;
            expirePublished = false;
            CacheHitboxColliders();
        }

        /// 판정 없이 표현만 남을 때 사라질 시점을 정한다.
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            maxLifetime = Mathf.Max(0.1f, durationSeconds);
            return maxLifetime;
        }

        /// 상태와 분기, 충돌 후 효과를 기본 이동 규칙에 결합한다.
        public void Initialize(
            InGameCombatManager manager,
            UnitCombatState source,
            Vector2 fireDirection,
            float projectileSpeed,
            float baseDamage,
            DamageAttribute attribute,
            int pierceCount,
            float boundaryX,
            float lifetimeSeconds,
            StatusApplicationSpec statusSpec,
            float branchChance,
            int branchCount,
            float branchMultiplier,
            float branchSearchRadius,
            StatusApplicationSpec impactStatusSpec,
            bool enableContactDamage,
            bool stopAfterFirstHit,
            float impactDelay,
            RuntimeSkillVisualSpec runtimeImpactVisual,
            bool enableImpactArea,
            float impactAreaRadius,
            float delayedImpactDamage,
            SkillExecutionState sourceRuntime,
            SkillExecutionState snapshot,
            string ignoredUnitId = null,
            string skillId = null,
            bool magazineLastProjectile = false,
            bool allowCritical = false,
            float criticalChanceBonus = 0f,
            float criticalDamageBonus = 0f)
        {
            Initialize(
                manager,
                source,
                fireDirection,
                projectileSpeed,
                baseDamage,
                attribute,
                pierceCount,
                boundaryX,
                lifetimeSeconds);

            statusOnHit = statusSpec;
            branchDamageChance = Mathf.Clamp01(branchChance);
            branchDamageCount = Mathf.Max(0, branchCount);
            branchDamageMultiplier = Mathf.Max(0f, branchMultiplier);
            branchDamageSearchRadius = Mathf.Max(0f, branchSearchRadius);
            impactStatusOnHit = impactStatusSpec;
            contactDamageEnabled = enableContactDamage;
            stopOnFirstHit = stopAfterFirstHit;
            impactDelaySeconds = Mathf.Max(0f, impactDelay);
            impactRuntimeVisual = runtimeImpactVisual;
            hasImpactArea = enableImpactArea;
            impactRadius = Mathf.Max(0f, impactAreaRadius);
            impactDamage = Mathf.Max(0f, delayedImpactDamage);
            runtime = sourceRuntime;
            executionData = snapshot;
            sourceSkillId = skillId;
            isMagazineLastProjectile = magazineLastProjectile;
            magazineLastProjectileTriggerFired = false;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            if (!string.IsNullOrWhiteSpace(ignoredUnitId))
            {
                hitUnitIds.Add(ignoredUnitId);
            }
        }

        /// 생성 직후 충돌 판정에 사용할 영역을 확보한다.
        private void Awake()
        {
            CacheHitboxColliders();
        }

        /// 이동, 지연 충돌 효과, 만료 시점을 매 프레임 진행한다.
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            if (visualOnly)
            {
                maxLifetime -= deltaTime;
                if (maxLifetime <= 0f)
                {
                    effectManager.RemoveEffect(gameObject);
                }

                return;
            }

            if (!impactArmed)
            {
                var movement = direction * speed * deltaTime;
                TryHitRosterTargets(movement);
            }
            else if (!impactResolved)
            {
                impactDelayRemaining -= deltaTime;
                if (impactDelayRemaining <= 0f)
                {
                    Impact();
                }
            }

            maxLifetime -= deltaTime;
            if (HasPassedDestroyBoundary() || maxLifetime <= 0f)
            {
                TryExecuteOnExpireEffects();
                combatManager.Effects.RemoveEffect(gameObject);
            }
        }

        /// 이번 이동 경로와 겹친 대상을 순서대로 판정한다.
        private void TryHitRosterTargets(Vector2 movement)
        {
            if (combatManager == null || combatManager.Units == null || owner == null)
            {
                transform.position += (Vector3)movement;
                return;
            }

            var entries = combatManager.Units.Entries;
            UnitCollisionResolver.CollectTargets(
                combatManager.Units,
                entries,
                hitboxColliders,
                movement,
                collisionTargets);
            transform.position += (Vector3)movement;
            for (var i = 0; i < collisionTargets.Count; i++)
            {
                if (TryHitTarget(collisionTargets[i]))
                {
                    return;
                }
            }
        }

        /// 처음 만난 유효 대상에 직격 결과와 후속 규칙을 적용한다.
        private bool TryHitTarget(CombatUnitEntry target)
        {
            if (target == null || target.Model == null || !target.IsAlive || IsSameSide(target.Model))
            {
                return false;
            }

            var unitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null;
            if (!string.IsNullOrWhiteSpace(unitId) && !hitUnitIds.Add(unitId))
            {
                return false;
            }

            var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
            var resolvedDamage = 0f;
            if (contactDamageEnabled)
            {
                resolvedDamage = damage;
                var damageResult = combatManager.ApplyDamageWithTriggerState(target.Model, resolvedDamage, damageAttribute, owner, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, false, false, null, HitDamageMultiplier(target.Model), executionData != null ? executionData.TriggerExecutionState : null);
                if (!damageResult.IsDead)
                {
                    StatusCombatRules.ApplyStatus(combatManager, target.Model, statusOnHit, owner);
                }
                ZoneSkillActor.PublishHitOutcome(
                    combatManager,
                    combatManager != null ? combatManager.Units : null,
                    runtime,
                    executionData,
                    combatManager != null && combatManager.Units != null ? combatManager.Units.Find(owner) : null,
                    owner,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
            }

            TryRunProjectileHitTriggers();
            TryApplyBranchDamage(target, hitPosition, resolvedDamage);
            if (stopOnFirstHit)
            {
                ArmImpact(target, hitPosition);
                return true;
            }

            remainingHits--;
            if (remainingHits <= 0)
            {
                TryExecuteOnExpireEffects();
                combatManager.Effects.RemoveEffect(gameObject);
            }

            return true;
        }

        /// 대상 조건과 연속 적중 횟수를 현재 피해 배율에 합친다.
        private float HitDamageMultiplier(UnitCombatState target)
        {
            var multiplier = SkillExecutionRules.ResolveHitDamageMultiplier(executionData, target);

            if (runtime != null && executionData != null)
            {
                var repeatCount = SkillExecution.AdvanceConsecutiveHitCount(
                    runtime,
                    target);
                multiplier *= SkillExecutionRules.ResolveConsecutiveHitDamageMultiplier(
                    runtime,
                    executionData,
                    repeatCount);
            }

            return Mathf.Max(0f, multiplier);
        }

        /// 탄창의 마지막 발사가 맞은 순간을 반응 판정에 알린다.
        private void TryRunProjectileHitTriggers()
        {
            if (!isMagazineLastProjectile || magazineLastProjectileTriggerFired)
            {
                return;
            }

            magazineLastProjectileTriggerFired = true;
            SkillTrigger.ExecuteProjectileHit(
                combatManager,
                combatManager != null ? combatManager.Units : null,
                owner,
                sourceSkillId,
                true,
                transform.position,
                executionData != null ? executionData.TriggerExecutionState : null);
        }

        /// 확률 조건을 통과하면 주변의 다른 대상에게 피해를 잇는다.
        private void TryApplyBranchDamage(
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryDamage)
        {
            if (impactArmed
                || combatManager == null
                || combatManager.Units == null
                || hitTarget == null
                || branchDamageChance <= 0f
                || branchDamageCount <= 0
                || branchDamageSearchRadius <= 0f
                || primaryDamage <= 0f)
            {
                return;
            }

            if (UnityEngine.Random.value > branchDamageChance)
            {
                return;
            }

            var candidates = combatManager.Units.Entries;
            var radiusSq = branchDamageSearchRadius * branchDamageSearchRadius;
            var selectedTargets = new HashSet<UnitCombatState>();
            var branchDamage = primaryDamage;
            if (branchDamage <= 0f)
            {
                return;
            }

            for (var i = 0; i < branchDamageCount; i++)
            {
                var target = FindNearestBranchTarget(candidates, hitTarget, hitPosition, radiusSq, selectedTargets);
                if (target == null || target.Model == null || target.Transform == null)
                {
                    break;
                }

                selectedTargets.Add(target.Model);
                var targetPosition = (Vector2)target.Transform.position;
                combatManager.ApplyDamageWithTriggerState(
                    target.Model,
                    branchDamage,
                    damageAttribute,
                    owner,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    sourceSkillId,
                    true,
                    false,
                    null,
                    HitDamageMultiplier(target.Model) * branchDamageMultiplier,
                    executionData != null ? executionData.TriggerExecutionState : null);
                SpawnBranchDamageLine(hitPosition, targetPosition);
            }
        }

        /// 이미 맞힌 대상을 제외하고 가장 가까운 다음 대상을 고른다.
        private CombatUnitEntry FindNearestBranchTarget(
            IReadOnlyList<CombatUnitEntry> candidates,
            CombatUnitEntry hitTarget,
            Vector2 origin,
            float radiusSq,
            HashSet<UnitCombatState> selectedTargets)
        {
            CombatUnitEntry best = null;
            var bestDistanceSq = float.MaxValue;
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || candidate == hitTarget || candidate.Model == null || candidate.Transform == null || !candidate.IsAlive)
                {
                    continue;
                }

                if (IsSameSide(candidate.Model))
                {
                    continue;
                }

                if (selectedTargets.Contains(candidate.Model))
                {
                    continue;
                }

                var offset = (Vector2)candidate.Transform.position - origin;
                var distanceSq = offset.sqrMagnitude;
                if (distanceSq > radiusSq || distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            return best;
        }

        /// 분기된 두 지점을 짧은 연결 표현으로 보여준다.
        private void SpawnBranchDamageLine(Vector2 origin, Vector2 target)
        {
            const float durationSeconds = 0.12f;
            var lineObject = EffectVisualBuilder.CreateBranchDamageLine(
                combatManager.Effects,
                origin,
                target,
                out var material);
            if (lineObject == null)
            {
                return;
            }
            Destroy(material, durationSeconds);
            var lineActor = lineObject.GetComponent<ProjectileSkillActor>();
            if (lineActor == null)
            {
                lineActor = lineObject.AddComponent<ProjectileSkillActor>();
            }

            lineActor.InitializeVisualLifetime(combatManager.Effects, durationSeconds);
        }

        /// 아군 충돌을 피해 판정에서 제외한다.
        private bool IsSameSide(UnitCombatState target)
        {
            var ownerIdentity = owner.Identity;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
        }

        /// 진행 방향을 기준으로 유효 이동 범위를 벗어났는지 확인한다.
        private bool HasPassedDestroyBoundary()
        {
            if (impactArmed)
            {
                return false;
            }

            return destroyWhenGreaterThanBoundary
                ? transform.position.x > destroyBeyondX
                : transform.position.x < destroyBeyondX;
        }

        /// 첫 충돌 위치에 멈추고 후속 범위 효과의 시간을 시작한다.
        private void ArmImpact(CombatUnitEntry target, Vector2 hitPosition)
        {
            impactArmed = true;
            impactDelayRemaining = impactDelaySeconds;
            impactCenter = hitPosition;
            impactTarget = target != null ? target.Model : null;
            speed = 0f;
            remainingHits = 0;
            var colliders = GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        /// 멈춘 위치에서 충돌 후 표현과 범위 결과를 확정한다.
        private void Impact()
        {
            if (impactResolved || combatManager == null)
            {
                return;
            }

            impactResolved = true;
            var effects = combatManager.Effects;
            GameObject instance = null;
            if (effects != null && impactRuntimeVisual != null && impactRuntimeVisual.HasVisual())
            {
                var objectName = "ProjectileImpact";
                if (!string.IsNullOrWhiteSpace(sourceSkillId))
                {
                    objectName = "ProjectileImpact_" + sourceSkillId;
                }

                instance = effects.CreateEffect(new EffectCreateRequest(
                    impactRuntimeVisual,
                    null,
                    objectName,
                    impactCenter,
                    Quaternion.identity,
                    null,
                    null,
                    false,
                    false,
                    false));
            }

            if (instance != null)
            {
                var impactActor = instance.GetComponent<ProjectileSkillActor>();
                if (impactActor == null)
                {
                    impactActor = instance.AddComponent<ProjectileSkillActor>();
                }

                impactActor.InitializeVisualLifetime(effects, 0.1f);
            }

            if (hasImpactArea)
            {
                ApplyImpactAreaTargets(
                    combatManager,
                    combatManager.Units != null ? combatManager.Units.Find(owner) : null,
                    combatManager.Units,
                    executionData.PreparedImpactTargeting,
                    impactCenter,
                    impactRadius,
                    impactDamage,
                    damageAttribute,
                    impactStatusOnHit,
                    owner,
                    sourceSkillId,
                    runtime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    executionData);
            }

            TryExecuteOnExpireEffects();
            effects.RemoveEffect(gameObject);
        }

        /// 충돌 지점의 폭발 반경 안에 있는 모든 대상을 적중 처리한다.
        private static bool ApplyImpactAreaTargets(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager unitRoster,
            SkillTargetingSpec targetingSpec,
            Vector2 center,
            float radius,
            float damage,
            DamageAttribute damageAttribute,
            StatusApplicationSpec onHitStatus,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionState sourceRuntime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionState executionData)
        {
            if (manager == null || sourceEntry == null || unitRoster == null)
            {
                return false;
            }

            var candidates = SkillTargeting.TargetList(
                sourceEntry,
                unitRoster,
                targetingSpec);
            var radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target == null
                    || !target.IsAlive
                    || target.Model == null
                    || target.Transform == null)
                {
                    continue;
                }

                var unitId = target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : null;
                if (!string.IsNullOrWhiteSpace(unitId)
                    && !hitUnitIds.Add(unitId))
                {
                    continue;
                }
                if (((Vector2)target.Transform.position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                eligibleTargets.Add(target);
            }

            return ZoneSkillActor.ApplyResolvedTargets(
                manager,
                sourceEntry,
                unitRoster,
                eligibleTargets,
                damage,
                damageAttribute,
                onHitStatus,
                source,
                sourceSkillId,
                sourceRuntime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
        }

        /// 만료가 한 번만 후속 반응으로 이어지게 전달한다.
        private void TryExecuteOnExpireEffects()
        {
            if (expirePublished)
            {
                return;
            }

            expirePublished = true;
            if (combatManager != null && combatManager.Units != null && owner != null)
            {
                var lifecycleSourceEntry = combatManager.Units.Find(owner);
                var lifecycleContext = new SkillExecutionContext(
                    combatManager,
                    combatManager.Units,
                    lifecycleSourceEntry,
                    runtime,
                    impactTarget);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnExpire,
                    new SkillExecutionContext(
                        owner,
                        sourceSkillId,
                        impactTarget,
                        impactCenter,
                        0f,
                        0,
                        executionData,
                        lifecycleContext));
            }
        }

        /// 이동 경로 판정에 사용할 충돌 영역을 모은다.
        private void CacheHitboxColliders()
        {
            hitboxColliders = GetComponentsInChildren<Collider2D>();
        }

    }
}
