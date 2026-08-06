/*
 * 역할: 투사체의 이동과 실제 적중을 진행한다.
 * 이동, 충돌, 관통, 피해, 상태, 목표 지점 도착 후 효과와 수명을 처리한다.
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

        private readonly HashSet<string> hitUnitNames = new HashSet<string>();
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
        private string sourceSkillName;
        private bool isMagazineLastProjectile;
        private bool magazineLastProjectileTriggerFired;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private bool contactDamageEnabled = true;
        private float arrivalDelaySeconds;
        private SingleSkillDefinition arrivalSkill;
        private bool hasArrivalPoint;
        private Vector2 arrivalPoint;
        private bool arrivalArmed;
        private float arrivalDelayRemaining;
        private Vector2 arrivalCenter;
        private UnitCombatState arrivalEventTarget;
        private bool arrivalResolved;
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
            hitUnitNames.Clear();
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
            sourceSkillName = null;
            isMagazineLastProjectile = false;
            magazineLastProjectileTriggerFired = false;
            criticalAllowed = false;
            critChanceBonus = 0f;
            critDamageBonus = 0f;
            contactDamageEnabled = true;
            arrivalDelaySeconds = 0f;
            arrivalSkill = null;
            hasArrivalPoint = false;
            arrivalPoint = Vector2.zero;
            arrivalArmed = false;
            arrivalDelayRemaining = 0f;
            arrivalCenter = Vector2.zero;
            arrivalEventTarget = null;
            arrivalResolved = false;
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

        /// 상태와 분기, 목표 지점 도착 후 효과를 기본 이동 규칙에 결합한다.
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
            bool enableContactDamage,
            float arrivalDelay,
            SingleSkillDefinition preparedArrivalSkill,
            bool hasPreparedArrivalPoint,
            Vector2 preparedArrivalPoint,
            SkillExecutionState sourceRuntime,
            SkillExecutionState snapshot,
            string ignoredUnitName = null,
            string skillName = null,
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
            contactDamageEnabled = enableContactDamage;
            arrivalDelaySeconds = Mathf.Max(0f, arrivalDelay);
            arrivalSkill = preparedArrivalSkill;
            hasArrivalPoint = hasPreparedArrivalPoint && preparedArrivalSkill != null;
            arrivalPoint = preparedArrivalPoint;
            arrivalCenter = preparedArrivalPoint;
            runtime = sourceRuntime;
            executionData = snapshot;
            sourceSkillName = skillName;
            isMagazineLastProjectile = magazineLastProjectile;
            magazineLastProjectileTriggerFired = false;
            criticalAllowed = allowCritical;
            critChanceBonus = criticalChanceBonus;
            critDamageBonus = criticalDamageBonus;
            if (!string.IsNullOrWhiteSpace(ignoredUnitName))
            {
                hitUnitNames.Add(ignoredUnitName);
            }
        }

        /// 생성 직후 충돌 판정을 확보한다.
        private void Awake()
        {
            CacheHitboxColliders();
        }

        /// 이동, 목표 지점 도착 지연, 만료 시점을 매 프레임 진행한다.
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

            if (!arrivalArmed)
            {
                var start = (Vector2)transform.position;
                var movement = direction * speed * deltaTime;
                var reachesTargetPoint = hasArrivalPoint
                    && ReachesTargetPoint(start, movement);
                if (reachesTargetPoint)
                {
                    movement = arrivalPoint - start;
                }

                TryHitRosterTargets(movement);
                if (reachesTargetPoint && !arrivalArmed)
                {
                    transform.position = arrivalPoint;
                    BeginArrivalDelay();
                }
            }
            else if (!arrivalResolved)
            {
                arrivalDelayRemaining -= deltaTime;
                if (arrivalDelayRemaining <= 0f)
                {
                    ExecuteArrivalSkill();
                }
            }

            if (!arrivalArmed)
            {
                maxLifetime -= deltaTime;
                if (HasPassedDestroyBoundary() || maxLifetime <= 0f)
                {
                    if (hasArrivalPoint && !arrivalResolved)
                    {
                        transform.position = arrivalPoint;
                        BeginArrivalDelay();
                    }
                    else
                    {
                        TryExecuteOnExpireEffects();
                        combatManager.Effects.RemoveEffect(gameObject);
                    }
                }
            }
        }

        /// 이번 이동으로 고정된 목표 지점을 통과하는지 확인한다.
        private bool ReachesTargetPoint(Vector2 start, Vector2 movement)
        {
            var toTarget = arrivalPoint - start;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return true;
            }

            var distance = movement.magnitude;
            return distance > 0.0001f
                && Vector2.Dot(toTarget, direction) <= distance;
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

            var unitName = target.Model.Identity != null ? target.Model.Identity.UnitName : null;
            if (!string.IsNullOrWhiteSpace(unitName) && !hitUnitNames.Add(unitName))
            {
                return false;
            }

            var hitPosition = target.Transform != null ? (Vector2)target.Transform.position : Vector2.zero;
            var resolvedDamage = 0f;
            if (contactDamageEnabled)
            {
                resolvedDamage = damage;
                var resolvedCritChance = critChanceBonus;
                var resolvedCritDamage = critDamageBonus;
                SkillExecutionRules.ResolveHitCritModifiers(executionData, target.Model, combatManager.Units, ref resolvedCritChance, ref resolvedCritDamage);
                if (isMagazineLastProjectile && executionData != null)
                {
                    resolvedCritDamage = SkillExecutionRules.CombineCritDamageBonus(resolvedCritDamage, executionData.MagazineLastProjectileCritDamageBonus);
                }
                var damageResult = combatManager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, owner, criticalAllowed, resolvedCritChance, resolvedCritDamage, sourceSkillName, false, false, null, HitDamageMultiplier(target.Model), SkillExecutionRules.ResolveHitFinalDamageModifier(executionData, target.Model, combatManager.Units), executionData != null ? executionData.CriticalFinalDamageModifier : 1f, isTrigger: executionData != null && executionData.IsTrigger);
                if (!damageResult.IsDead)
                {
                    StatusCombatRules.ApplyStatus(combatManager, target.Model, statusOnHit, owner);
                }
            }

            ZoneSkillActor.PublishHitOutcome(
                combatManager,
                combatManager != null ? combatManager.Units : null,
                runtime,
                executionData,
                combatManager != null && combatManager.Units != null ? combatManager.Units.Find(owner) : null,
                owner,
                sourceSkillName,
                target,
                hitPosition,
                resolvedDamage);

            TryRunProjectileHitTriggers();
            TryApplyBranchDamage(target, hitPosition, resolvedDamage);
            if (hasArrivalPoint)
            {
                if (arrivalEventTarget == null)
                {
                    arrivalEventTarget = target.Model;
                }

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
            if (!isMagazineLastProjectile
                || magazineLastProjectileTriggerFired
                || (executionData != null && executionData.IsTrigger))
            {
                return;
            }

            magazineLastProjectileTriggerFired = true;
            SkillTrigger.ExecuteProjectileHit(
                combatManager,
                combatManager != null ? combatManager.Units : null,
                owner,
                sourceSkillName,
                true,
                transform.position);
        }

        /// 확률 조건을 통과하면 주변의 다른 대상에게 피해를 잇는다.
        private void TryApplyBranchDamage(
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryDamage)
        {
            if (arrivalArmed
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
                    sourceSkillName,
                    true,
                    false,
                    null,
                    HitDamageMultiplier(target.Model) * branchDamageMultiplier,
                    finalDamageModifier: SkillExecutionRules.ResolveHitFinalDamageModifier(executionData, target.Model, combatManager.Units),
                    criticalFinalDamageModifier: executionData != null ? executionData.CriticalFinalDamageModifier : 1f,
                    isTrigger: executionData != null && executionData.IsTrigger);
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
            if (arrivalArmed)
            {
                return false;
            }

            return destroyWhenGreaterThanBoundary
                ? transform.position.x > destroyBeyondX
                : transform.position.x < destroyBeyondX;
        }

        /// 목표 지점에 멈추고 도착 후 SingleSkill의 지연 시간을 시작한다.
        private void BeginArrivalDelay()
        {
            if (arrivalArmed || arrivalResolved)
            {
                return;
            }

            arrivalArmed = true;
            arrivalDelayRemaining = arrivalDelaySeconds;
            arrivalCenter = arrivalPoint;
            speed = 0f;
            var colliders = GetComponentsInChildren<Collider2D>(true);
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = false;
                }
            }
        }

        /// 목표 지점에서 기존 SingleSkill 실행 경로를 호출한다.
        private void ExecuteArrivalSkill()
        {
            if (arrivalResolved || combatManager == null)
            {
                return;
            }

            arrivalResolved = true;
            var sourceEntry = combatManager.Units != null
                ? combatManager.Units.Find(owner)
                : null;
            if (arrivalSkill != null && sourceEntry != null && runtime != null)
            {
                combatManager.SkillExecution.TryExecuteReaction(
                    sourceEntry,
                    runtime,
                    runtime,
                    arrivalSkill,
                    combatManager.Units,
                    combatManager,
                    arrivalEventTarget,
                    arrivalCenter,
                    hasTargetPoint: true,
                    hasRawDamageOverride: false,
                    rawDamageOverride: 0f,
                    recastGeneration: 0,
                    damageMultiplier: 1f,
                    sourceSkillName: sourceSkillName,
                    lockToEventTarget: false,
                    publishSkillLifecycleEvents: false,
                    beginCast: false,
                    onHitStatusOverride: executionData != null
                        ? executionData.PreparedStatus
                        : statusOnHit);
            }

            TryExecuteOnExpireEffects();
            combatManager.Effects.RemoveEffect(gameObject);
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
                    arrivalEventTarget);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnExpire,
                    new SkillExecutionContext(
                        owner,
                        sourceSkillName,
                        arrivalEventTarget,
                        arrivalCenter,
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
