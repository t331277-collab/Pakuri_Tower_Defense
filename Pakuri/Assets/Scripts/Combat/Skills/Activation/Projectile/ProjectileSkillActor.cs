/*
 * 역할: 런타임 투사체 이동과 충돌.
 * 책임: Projectile 이동·충돌·피해·상태·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// ProjectileSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
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
        private SkillExecutionData runtime;
        private SkillExecutionData executionData;
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

        /// 투사체의 이동, 충돌과 후속 효과를 시작한다.
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

        /// 시각 효과의 수명을 정한다.
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            maxLifetime = Mathf.Max(0.1f, durationSeconds);
            return maxLifetime;
        }

        /// 투사체의 충돌과 후속 효과를 완성한다.
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
            SkillExecutionData sourceRuntime,
            SkillExecutionData snapshot,
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

        /// 충돌 영역을 준비한다.
        private void Awake()
        {
            CacheHitboxColliders();
        }

        /// 프레임 경과에 따라 이동과 수명을 갱신한다.
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

        /// 이동 결과를 전투 충돌 판정에 연결한다.
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

        /// 충돌한 대상의 피해와 후속 처리를 시작한다.
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
                resolvedDamage = HitDamage();
                var damageResult = combatManager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, owner, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId, finalDamageMultiplier: HitDamageMultiplier(target.Model));
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(target.Model);
                }
                SkillExecution.ApplyHitEnhancements(
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

        /// 투사체의 기본 피해를 확정한다.
        private float HitDamage()
        {
            return Mathf.Max(0f, damage);
        }

        /// 적중 대상에 맞는 피해 배율을 정한다.
        private float HitDamageMultiplier(UnitCombatState target)
        {
            var multiplier = SkillExecutionRuleResolver.ResolveHitDamageMultiplier(executionData, target);

            if (runtime != null && executionData != null)
            {
                multiplier *= runtime.ConsecutiveHitDamageMultiplier(target, executionData);
            }

            return Mathf.Max(0f, multiplier);
        }

        /// 적중 상태를 공통 경로에 적용한다.
        private void TryApplyStatus(UnitCombatState target)
        {
            StatusCombatRules.ApplyStatus(combatManager, target, statusOnHit, owner);
        }

        /// 투사체 적중 사건을 반응 흐름에 전달한다.
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
                transform.position);
        }

        /// 분기 피해 조건을 적중 결과에 반영한다.
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
                combatManager.ApplyDamage(
                    target.Model,
                    branchDamage,
                    damageAttribute,
                    owner,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    sourceSkillId,
                    suppressOutgoingDamageTriggers: true,
                    finalDamageMultiplier: HitDamageMultiplier(target.Model) * branchDamageMultiplier);
                SpawnBranchDamageLine(hitPosition, targetPosition);
            }
        }

        /// 분기 피해가 이어질 대상을 고른다.
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

        /// 분기 피해 영역을 생성한다.
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

        /// 대상이 발사자와 같은 진영인지 확인한다.
        private bool IsSameSide(UnitCombatState target)
        {
            var ownerIdentity = owner.Identity;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
        }

        /// 투사체가 종료 경계를 넘었는지 확인한다.
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

        /// 지연 충돌 후속 효과를 준비한다.
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

        /// 준비된 충돌 영역의 피해를 실행한다.
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
                SkillExecution.ApplyAreaHits(
                    combatManager,
                    combatManager.Units != null ? combatManager.Units.Find(owner) : null,
                    combatManager.Units,
                    executionData.PreparedImpactTargeting,
                    impactCenter,
                    impactRadius,
                    false,
                    impactDamage,
                    damageAttribute,
                    impactStatusOnHit,
                    owner,
                    sourceSkillId,
                    runtime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    int.MaxValue,
                    executionData);
            }

            TryExecuteOnExpireEffects();
            effects.RemoveEffect(gameObject);
        }

        /// 투사체 종료 사건을 후속 효과에 전달한다.
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
                var lifecycleContext = new SkillActionContext(
                    combatManager,
                    combatManager.Units,
                    lifecycleSourceEntry,
                    runtime,
                    impactTarget);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnExpire,
                    new SkillActionContext(
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

        /// 투사체 충돌 영역을 저장한다.
        private void CacheHitboxColliders()
        {
            hitboxColliders = GetComponentsInChildren<Collider2D>();
        }

    }
}
