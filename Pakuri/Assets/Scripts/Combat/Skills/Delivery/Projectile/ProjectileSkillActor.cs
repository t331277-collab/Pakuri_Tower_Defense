/*
 * 역할: 런타임 투사체 이동과 충돌.
 * 책임: 투사체를 이동시키고 Collider 접촉·적중 제한·충돌·수명 종료를 처리한다.
 */

using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>ProjectileSkillActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
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
        private ProjectileStatusHitSpec statusOnHit;
        private ProjectileBranchDamageSpec branchDamageOnHit;
        private SkillUseState runtime;
        private SkillExecutionData executionData;
        private string sourceSkillId;
        private bool isMagazineLastProjectile;
        private bool magazineLastProjectileTriggerFired;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private ProjectileStatusHitSpec impactStatusOnHit;
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
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
            branchDamageOnHit = null;
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>VisualLifetime</c>를 초기화한다.</summary>
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            maxLifetime = Mathf.Max(0.1f, durationSeconds);
            return maxLifetime;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>DestroyBoundaryX</c> 결과값을 생성해 반환한다.</summary>
        internal static float DestroyBoundaryX(
            Vector2 origin,
            Vector2 fireDirection,
            float projectileSpeed,
            float lifetimeSeconds)
        {
            var normalizedDirection = fireDirection.sqrMagnitude > 0.0001f
                ? fireDirection.normalized
                : Vector2.right;
            var maxTravelDistance = Mathf.Max(
                40f,
                Mathf.Max(0f, projectileSpeed) * Mathf.Max(0.1f, lifetimeSeconds) + 1f);
            return origin.x + normalizedDirection.x * maxTravelDistance;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
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
            ProjectileStatusHitSpec statusSpec,
            ProjectileBranchDamageSpec branchSpec,
            ProjectileStatusHitSpec impactStatusSpec,
            bool enableContactDamage,
            bool stopAfterFirstHit,
            float impactDelay,
            RuntimeSkillVisualSpec runtimeImpactVisual,
            bool enableImpactArea,
            float impactAreaRadius,
            float delayedImpactDamage,
            SkillUseState sourceRuntime,
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
            branchDamageOnHit = branchSpec;
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

        /// <summary>Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.</summary>
        private void Awake()
        {
            CacheHitboxColliders();
        }

        /// <summary>현재 Unity 프레임에서 <c>Update</c> 갱신 동작을 진행한다.</summary>
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

        /// <summary>전달된 <c>movement</c> 값을 사용해 <c>HitRosterTargets</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>HitTarget</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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
                ProjectileSkillExecutor.ApplyHitEnhancements(
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

        /// <summary><c>HitDamage</c> 결과값을 생성해 반환한다.</summary>
        private float HitDamage()
        {
            return Mathf.Max(0f, damage);
        }

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>HitDamageMultiplier</c> 결과값을 생성해 반환한다.</summary>
        private float HitDamageMultiplier(UnitCombatState target)
        {
            var multiplier = executionData != null
                ? Mathf.Max(0f, executionData.DamageMultiplier)
                : 1f;
            if (executionData != null)
            {
                multiplier *= SkillExecutionRuleResolver.ConditionalDamageMultiplier(executionData, target);
            }

            if (runtime != null && executionData != null)
            {
                multiplier *= runtime.ConsecutiveHitDamageMultiplier(target, executionData);
            }

            return Mathf.Max(0f, multiplier);
        }

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>ApplyStatus</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private void TryApplyStatus(UnitCombatState target)
        {
            StatusCombatRules.ApplyStatus(combatManager, target, statusOnHit, owner);
        }

        /// <summary><c>RunProjectileHitTriggers</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ApplyBranchDamage</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private void TryApplyBranchDamage(
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryDamage)
        {
            if (impactArmed
                || combatManager == null
                || combatManager.Units == null
                || hitTarget == null
                || branchDamageOnHit == null
                || !branchDamageOnHit.Enabled
                || primaryDamage <= 0f)
            {
                return;
            }

            if (Random.value > Mathf.Clamp01(branchDamageOnHit.Chance))
            {
                return;
            }

            var candidates = combatManager.Units.Entries;
            var radiusSq = branchDamageOnHit.SearchRadius * branchDamageOnHit.SearchRadius;
            var selectedTargets = new HashSet<UnitCombatState>();
            var branchDamage = primaryDamage;
            if (branchDamage <= 0f)
            {
                return;
            }

            for (var i = 0; i < branchDamageOnHit.Count; i++)
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
                    finalDamageMultiplier: HitDamageMultiplier(target.Model) * Mathf.Max(0f, branchDamageOnHit.DamageMultiplier));
                SpawnBranchDamageLine(hitPosition, targetPosition);
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>NearestBranchTarget</c>를 찾는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>BranchDamageLine</c>를 런타임 씬 오브젝트로 생성하고 등록한다.</summary>
        private void SpawnBranchDamageLine(Vector2 origin, Vector2 target)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null || combatManager.Effects == null)
            {
                return;
            }

            const float durationSeconds = 0.12f;
            var lineObject = combatManager.Effects.CreateEffect(new EffectCreateRequest(
                null,
                null,
                "InGameBranchDamageLine",
                Vector3.zero,
                Quaternion.identity,
                null,
                0f,
                null,
                false,
                false,
                true));
            var material = new Material(shader)
            {
                name = "RuntimeBranchDamageLineMaterial"
            };
            var line = lineObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = 0.08f;
            line.endWidth = 0.04f;
            line.startColor = new Color(0.1f, 0.65f, 1f, 1f);
            line.endColor = new Color(0.1f, 0.35f, 1f, 0.75f);
            line.numCapVertices = 2;
            line.sortingOrder = 100;
            line.SetPosition(0, new Vector3(origin.x, origin.y, 0f));
            line.SetPosition(1, new Vector3(target.x, target.y, 0f));
            Destroy(material, durationSeconds);
            var lineActor = lineObject.GetComponent<LineSkillActor>();
            if (lineActor == null)
            {
                lineActor = lineObject.AddComponent<LineSkillActor>();
            }

            lineActor.InitializeVisualLifetime(combatManager.Effects, durationSeconds);
        }

        /// <summary>전달된 <c>target</c> 값을 사용해 <c>SameSide</c> 조건 충족 여부를 반환한다.</summary>
        private bool IsSameSide(UnitCombatState target)
        {
            var ownerIdentity = owner.Identity;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
        }

        /// <summary>소유한 런타임 상태에 <c>PassedDestroyBoundary</c>가 있는지 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ArmImpact</c> 작업을 수행한다.</summary>
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

        /// <summary><c>Impact</c> 작업을 수행한다.</summary>
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
                    0f,
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
                ZoneSkillActor.ApplyAreaTick(
                    combatManager,
                    combatManager.Units != null ? combatManager.Units.Find(owner) : null,
                    combatManager.Units,
                    BuildImpactTargeting(),
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

        /// <summary><c>ExecuteOnExpireEffects</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary><c>ImpactTargeting</c>를 구성한다.</summary>
        private SkillTargetingSpec BuildImpactTargeting()
        {
            return new SkillTargetingSpec
            {
                TargetSide = SkillTargetSide.Enemy,
                Selection = SkillTargetSelection.Nearest,
                Shape = SkillTargetShape.Circle,
                Radius = impactRadius,
                CoverAll = false
            };
        }

        /// <summary><c>CacheHitboxColliders</c> 작업을 수행한다.</summary>
        private void CacheHitboxColliders()
        {
            hitboxColliders = GetComponentsInChildren<Collider2D>();
        }

        /// <summary>전달된 <c>direction</c> 값을 사용해 <c>Rotation</c> 결과값을 생성해 반환한다.</summary>
        private static Quaternion Rotation(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }
    }

    /// <summary><c>ProjectileStatusHitSpec</c>을 설명하는 설정값을 묶는다.</summary>
    public class ProjectileStatusHitSpec
    {
        public bool Enabled;
        public StatusEffectKind Kind;
        public StatusRuntimeData StatusData;
        public float Chance;
        public int Stacks;
        public float DurationSeconds;
        public int MaxStacks;
        public bool Permanent;
        public bool RefreshDuration = true;
        public StatusEffectKind ThresholdSourceStatusKind;
        public int ThresholdSourceMinStacks;
        public ProjectileStatusHitSpec ThresholdStatusSpec;
    }

    /// <summary><c>ProjectileBranchDamageSpec</c>을 설명하는 설정값을 묶는다.</summary>
    public class ProjectileBranchDamageSpec
    {
        public bool Enabled;
        public float Chance;
        public int Count;
        public float DamageMultiplier = 1f;
        public float SearchRadius;
    }
}
