/*
 * 역할: 런타임 투사체 이동과 충돌.
 * 책임: Projectile 타기팅·발사·이동·충돌·후속 실행·비주얼 수명과 완료를 소유한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// ProjectileSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public partial class ProjectileSkillActor : MonoBehaviour
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
        private bool executionActor;
        private bool executionLaunchFinished;
        private int pendingOperations;
        private Collider2D[] hitboxColliders;

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태를 초기화한다.
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
            executionActor = false;
            executionLaunchFinished = false;
            pendingOperations = 0;
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

        /// 전달된 런타임 입력값을 사용해 VisualLifetime를 초기화한다.
        public float InitializeVisualLifetime(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            visualOnly = true;
            executionActor = false;
            executionLaunchFinished = false;
            pendingOperations = 0;
            maxLifetime = Mathf.Max(0.1f, durationSeconds);
            return maxLifetime;
        }

        /// Projectile 실행 Actor의 작업 추적을 시작한다.
        private void BeginExecution(EffectManager manager)
        {
            effectManager = manager;
            executionActor = true;
            executionLaunchFinished = false;
            pendingOperations = 0;
            visualOnly = false;
        }

        /// Projectile 실행 초기화를 끝낸다.
        private void FinishExecution()
        {
            executionLaunchFinished = true;
            TryCompleteExecution();
        }

        /// Projectile 지연 작업을 이 Actor의 수명에 연결한다.
        private void StartTrackedCoroutine(IEnumerator operation)
        {
            pendingOperations++;
            StartCoroutine(TrackOperation(operation));
        }

        /// Projectile 지연 작업 완료를 추적한다.
        private IEnumerator TrackOperation(IEnumerator operation)
        {
            yield return operation;
            pendingOperations = Mathf.Max(0, pendingOperations - 1);
            TryCompleteExecution();
        }

        /// 모든 Projectile 실행 작업이 끝났으면 삭제를 요청한다.
        private void TryCompleteExecution()
        {
            if (executionActor && executionLaunchFinished && pendingOperations == 0)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }

        /// 전달된 런타임 입력값을 사용해 DestroyBoundaryX 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태를 초기화한다.
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

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            CacheHitboxColliders();
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            if (executionActor)
            {
                return;
            }

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

        /// 전달된 movement 값을 사용해 HitRosterTargets 작업을 시도하고 성공 여부를 반환한다.
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

        /// 전달된 target 값을 사용해 HitTarget 작업을 시도하고 성공 여부를 반환한다.
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
                SkillExecutionRuleResolver.ApplyHitEnhancements(
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

        /// HitDamage 결과값을 생성해 반환한다.
        private float HitDamage()
        {
            return Mathf.Max(0f, damage);
        }

        /// 전달된 target 값을 사용해 HitDamageMultiplier 결과값을 생성해 반환한다.
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

        /// 전달된 target 값을 사용해 ApplyStatus 작업을 시도하고 성공 여부를 반환한다.
        private void TryApplyStatus(UnitCombatState target)
        {
            StatusCombatRules.ApplyStatus(combatManager, target, statusOnHit, owner);
        }

        /// RunProjectileHitTriggers 작업을 시도하고 성공 여부를 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 ApplyBranchDamage 작업을 시도하고 성공 여부를 반환한다.
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

            if (UnityEngine.Random.value > Mathf.Clamp01(branchDamageOnHit.Chance))
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

        /// 전달된 런타임 입력값을 사용해 NearestBranchTarget를 찾는다.
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

        /// 전달된 런타임 입력값을 사용해 BranchDamageLine를 런타임 씬 오브젝트로 생성하고 등록한다.
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
            var lineActor = lineObject.GetComponent<ProjectileSkillActor>();
            if (lineActor == null)
            {
                lineActor = lineObject.AddComponent<ProjectileSkillActor>();
            }

            lineActor.InitializeVisualLifetime(combatManager.Effects, durationSeconds);
        }

        /// 전달된 target 값을 사용해 SameSide 조건 충족 여부를 반환한다.
        private bool IsSameSide(UnitCombatState target)
        {
            var ownerIdentity = owner.Identity;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
        }

        /// 소유한 런타임 상태에 PassedDestroyBoundary가 있는지 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 ArmImpact 작업을 수행한다.
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

        /// Impact 작업을 수행한다.
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

        /// ExecuteOnExpireEffects 작업을 시도하고 성공 여부를 반환한다.
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

        /// ImpactTargeting를 구성한다.
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

        /// CacheHitboxColliders 작업을 수행한다.
        private void CacheHitboxColliders()
        {
            hitboxColliders = GetComponentsInChildren<Collider2D>();
        }

        /// 전달된 direction 값을 사용해 Rotation 결과값을 생성해 반환한다.
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

    /// ProjectileStatusHitSpec을 설명하는 설정값을 묶는다.
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

    /// ProjectileBranchDamageSpec을 설명하는 설정값을 묶는다.
    public class ProjectileBranchDamageSpec
    {
        public bool Enabled;
        public float Chance;
        public int Count;
        public float DamageMultiplier = 1f;
        public float SearchRadius;
    }

    /// Projectile 계열 판정과 적용을 소유한다.
    public partial class ProjectileSkillActor
    {

        /// 전달된 런타임 입력값을 사용해 설정된 런타임 작업를 실행한다.
        internal bool InitializeExecution(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill)
        {
            BeginExecution(context.CombatManager.Effects);
            var origin = context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
                ? context.ManualAimDirection
                : SkillTargeting.DirectionToTarget(origin, target);

            if (direction.sqrMagnitude <= 0.0001f)
            {
                if (!context.HasManualAimDirection)
                {
                    FinishExecution();
                    return false;
                }

                direction = Vector2.right;
            }

            var damage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage);
            var attribute = skill.Damage != null ? skill.Damage.Element : skill.Element;
            var currentBurstProjectileIndex = context.Runtime != null
                ? context.Runtime.CurrentBurstProjectileIndex()
                : 1;
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var hasRuntimeVisual = effects != null && runtimeVisual != null && runtimeVisual.HasVisual();

            var baseStatusSpec = SkillStatus.StatusSpec(skill.OnHitStatus, snapshot);
            var projectile = skill.Projectile;
            var burstProjectileCount = projectile != null ? Math.Max(1, projectile.BurstProjectileCount) : 1;
            var speed = projectile != null ? projectile.ProjectileSpeed : 0f;
            var pierce = projectile != null ? projectile.PierceCount : 0;
            var projectileCount = projectile != null ? Math.Max(1, projectile.ProjectilesPerShot) : 1;
            if (snapshot != null)
            {
                pierce += snapshot.PierceBonus;
                if (burstProjectileCount <= 1)
                {
                    projectileCount += snapshot.AdditionalProjectileBonus;
                }
            }

            projectileCount = Math.Max(1, projectileCount);
            pierce = Math.Max(0, pierce);
            var burstDamageMultiplier = BurstDamageMultiplier(
                skill,
                snapshot,
                currentBurstProjectileIndex,
                burstProjectileCount);
            var launchSnapshot = snapshot.CopyWithDamageMultiplier(burstDamageMultiplier);
            var isMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            var lifetime = ProjectileLifetime(skill);
            for (var i = 0; i < projectileCount; i++)
            {
                var spreadDirection = ProjectileSpreadDirection(direction, i, projectileCount);
                var boundary = ProjectileSkillActor.DestroyBoundaryX(
                    origin,
                    spreadDirection,
                    speed,
                    lifetime);
                if (effects == null)
                {
                    continue;
                }

                var projectileLaunchIndex = context.Runtime != null
                    ? context.Runtime.AdvanceProjectileLaunchCount()
                    : 0;
                var branchSpec = BranchDamageSpec(snapshot, projectileLaunchIndex);
                var rotation = EffectVisualBuilder.Rotation(spreadDirection);
                var objectName = "Projectile";
                if (!string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    objectName = "Projectile_" + skill.SkillId;
                }

                var instance = effects.CreateEffect(new EffectCreateRequest(
                    runtimeVisual,
                    null,
                    objectName,
                    origin,
                    rotation,
                    null,
                    null,
                    true,
                    true,
                    true));

                if (instance == null)
                {
                    continue;
                }

                var actor = instance.GetComponent<ProjectileSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ProjectileSkillActor>();
                }

                var statusSpec = BurstStatusSpec(baseStatusSpec, snapshot, currentBurstProjectileIndex, burstProjectileCount);
                var impactRadius = 0f;
                if (skill.ImpactArea != null)
                {
                    impactRadius = skill.ImpactArea.Radius;
                }
                actor.Initialize(
                    context.CombatManager,
                    context.Caster,
                    spreadDirection,
                    speed,
                    damage,
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    statusSpec,
                    branchSpec,
                    SkillStatus.StatusSpec(skill.ImpactStatus, snapshot),
                    skill.ContactDamageEnabled,
                    skill.StopOnFirstHit,
                    ImpactDelay(skill, snapshot),
                    skill.ImpactRuntimeVisual,
                    skill.HasImpactArea,
                    SkillTargeting.Radius(
                        impactRadius,
                        snapshot.RadiusMultiplier,
                        snapshot.RadiusBonus),
                    damage,
                    context.Runtime,
                    launchSnapshot,
                    null,
                    skill.SkillId,
                    isMagazineLastProjectile,
                    skill.Damage != null && skill.Damage.CriticalAllowed,
                    snapshot != null ? snapshot.CritChanceBonus : 0f,
                    snapshot != null ? snapshot.CritDamageBonus : 0f);
            }

            TryScheduleFollowUpProjectile(
                context,
                snapshot,
                skill,
                runtimeVisual,
                baseStatusSpec,
                origin,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                ProjectileSkillActor.DestroyBoundaryX(
                    origin,
                    direction,
                    speed,
                    lifetime),
                lifetime,
                burstProjectileCount,
                currentBurstProjectileIndex);

            FinishExecution();
            return true;
        }

        /// 전달된 런타임 입력값을 사용해 ProjectileSpreadDirection 결과값을 생성해 반환한다.
        private static Vector2 ProjectileSpreadDirection(Vector2 direction, int index, int count)
        {
            if (count <= 1)
            {
                return direction;
            }

            const float angleStep = 10f;
            var offset = (index - (count - 1) * 0.5f) * angleStep;
            return RotateDirection(direction, offset);
        }

        /// 전달된 런타임 입력값을 사용해 RotateDirection 결과값을 생성해 반환한다.
        private static Vector2 RotateDirection(Vector2 direction, float degrees)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Vector2.right;
            }

            var radians = degrees * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }

        /// 전달된 런타임 입력값을 사용해 BranchDamageSpec 결과값을 생성해 반환한다.
        private static ProjectileBranchDamageSpec BranchDamageSpec(
            SkillExecutionData snapshot,
            int projectileLaunchIndex)
        {
            if (snapshot == null || !snapshot.HasBranchBehavior)
            {
                return null;
            }

            var chance = BranchChance(snapshot, projectileLaunchIndex);
            var count = snapshot.HasBranchCount ? snapshot.BranchCount : chance > 0f ? 1 : 0;
            var radius = snapshot.HasBranchSearchRadius ? snapshot.BranchSearchRadius : 4.5f;
            if (chance <= 0f || count <= 0 || radius <= 0f)
            {
                return null;
            }

            return new ProjectileBranchDamageSpec
            {
                Enabled = true,
                Chance = Mathf.Clamp01(chance),
                Count = Math.Max(1, count),
                DamageMultiplier = snapshot.HasBranchDamageMultiplier ? Mathf.Max(0f, snapshot.BranchDamageMultiplier) : 1f,
                SearchRadius = Mathf.Max(0f, radius)
            };
        }

        /// 전달된 런타임 입력값을 사용해 BranchChance 결과값을 생성해 반환한다.
        private static float BranchChance(SkillExecutionData snapshot, int projectileLaunchIndex)
        {
            var chance = snapshot.HasBranchChanceSet ? snapshot.BranchChanceSet : snapshot.BranchChanceBonus;
            if (snapshot.HasBranchLaunchTrigger
                && projectileLaunchIndex > 0
                && projectileLaunchIndex % snapshot.BranchLaunchPeriod == 0)
            {
                chance = snapshot.BranchLaunchChanceSet;
            }

            return chance;
        }

        /// 전달된 런타임 입력값을 사용해 BurstDamageMultiplier 결과값을 생성해 반환한다.
        private static float BurstDamageMultiplier(
            ProjectileSkillDefinition skill,
            SkillExecutionData snapshot,
            int projectileIndex,
            int burstProjectileCount)
        {
            var multiplier = 1f;
            var projectile = skill != null ? skill.Projectile : null;
            if (projectile != null
                && projectile.BurstDamageMultiplier > 0f
                && MatchesBurstProjectileIndex(projectile.BurstDamageProjectileIndex, projectileIndex, burstProjectileCount))
            {
                multiplier *= projectile.BurstDamageMultiplier;
            }

            if (snapshot != null)
            {
                multiplier *= SkillExecutionRuleResolver.BurstDamageMultiplier(snapshot, projectileIndex, burstProjectileCount);
            }

            return Mathf.Max(0f, multiplier);
        }

        /// 전달된 런타임 입력값을 사용해 MatchesBurstProjectileIndex 조건을 평가하고 결과를 반환한다.
        private static bool MatchesBurstProjectileIndex(int configuredIndex, int projectileIndex, int burstProjectileCount)
        {
            if (configuredIndex == 0)
            {
                return burstProjectileCount > 0 && projectileIndex == burstProjectileCount;
            }

            return configuredIndex > 0 && configuredIndex == projectileIndex;
        }

        /// 전달된 런타임 입력값을 사용해 ScheduleFollowUpProjectile 작업을 시도하고 성공 여부를 반환한다.
        private void TryScheduleFollowUpProjectile(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill,
            RuntimeSkillVisualSpec runtimeVisual,
            ProjectileStatusHitSpec statusSpec,
            Vector2 origin,
            Vector2 direction,
            float speed,
            float baseDamage,
            DamageAttribute attribute,
            int pierce,
            float boundary,
            float lifetime,
            int burstProjectileCount,
            int currentBurstProjectileIndex)
        {
            if (context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || skill == null
                || snapshot == null
                || !snapshot.HasFollowUpProjectile
                || runtimeVisual == null
                || !runtimeVisual.HasVisual()
                || currentBurstProjectileIndex < burstProjectileCount)
            {
                return;
            }

            StartTrackedCoroutine(ExecuteFollowUpProjectilesAfterDelay(
                context,
                snapshot,
                skill,
                runtimeVisual,
                statusSpec,
                origin,
                direction,
                speed,
                baseDamage,
                attribute,
                pierce,
                boundary,
                lifetime));
        }

        /// 전달된 런타임 입력값을 사용해 FollowUpProjectilesAfterDelay를 실행한다.
        private IEnumerator ExecuteFollowUpProjectilesAfterDelay(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill,
            RuntimeSkillVisualSpec runtimeVisual,
            ProjectileStatusHitSpec statusSpec,
            Vector2 origin,
            Vector2 direction,
            float speed,
            float baseDamage,
            DamageAttribute attribute,
            int pierce,
            float boundary,
            float lifetime)
        {
            if (snapshot.FollowUpProjectileDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(snapshot.FollowUpProjectileDelaySeconds);
            }
            else
            {
                yield return null;
            }

            if (context == null
                || context.CombatManager == null
                || context.CombatManager.Effects == null
                || skill == null
                || runtimeVisual == null
                || !runtimeVisual.HasVisual())
            {
                yield break;
            }

            var count = Math.Max(1, snapshot.FollowUpProjectileCount);
            for (var i = 0; i < count; i++)
            {
                SpawnProjectileActor(
                    context,
                    snapshot,
                    skill,
                    runtimeVisual,
                    statusSpec,
                    origin,
                    direction,
                    speed,
                    baseDamage * Mathf.Max(0f, snapshot.FollowUpProjectileDamageMultiplier),
                    attribute,
                    pierce,
                    boundary,
                    lifetime,
                    false);
            }
        }

        /// 전달된 런타임 입력값을 사용해 ProjectileActor를 런타임 씬 오브젝트로 생성하고 등록한다.
        private static void SpawnProjectileActor(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill,
            RuntimeSkillVisualSpec runtimeVisual,
            ProjectileStatusHitSpec statusSpec,
            Vector2 origin,
            Vector2 direction,
            float speed,
            float damage,
            DamageAttribute attribute,
            int pierce,
            float boundary,
            float lifetime,
            bool isMagazineLastProjectile)
        {
            if (context == null
                || context.CombatManager == null
                || skill == null
                || context.CombatManager.Effects == null
                || runtimeVisual == null
                || !runtimeVisual.HasVisual())
            {
                return;
            }

            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return;
            }

            var projectileLaunchIndex = context.Runtime != null
                ? context.Runtime.AdvanceProjectileLaunchCount()
                : 0;
            var branchSpec = BranchDamageSpec(snapshot, projectileLaunchIndex);
            var rotation = EffectVisualBuilder.Rotation(direction);
            var objectName = "Projectile";
            if (!string.IsNullOrWhiteSpace(skill.SkillId))
            {
                objectName = "Projectile_" + skill.SkillId;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                runtimeVisual,
                null,
                objectName,
                origin,
                rotation,
                null,
                null,
                true,
                true,
                false));
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<ProjectileSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ProjectileSkillActor>();
            }

            var impactRadius = 0f;
            if (skill.ImpactArea != null)
            {
                impactRadius = skill.ImpactArea.Radius;
            }
            actor.Initialize(
                context.CombatManager,
                context.Caster,
                direction,
                speed,
                damage,
                attribute,
                pierce,
                boundary,
                lifetime,
                statusSpec,
                branchSpec,
                SkillStatus.StatusSpec(skill.ImpactStatus, snapshot),
                skill.ContactDamageEnabled,
                skill.StopOnFirstHit,
                ImpactDelay(skill, snapshot),
                skill.ImpactRuntimeVisual,
                skill.HasImpactArea,
                SkillTargeting.Radius(
                    impactRadius,
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus),
                damage,
                context.Runtime,
                snapshot,
                null,
                skill.SkillId,
                isMagazineLastProjectile,
                skill.Damage != null && skill.Damage.CriticalAllowed,
                snapshot != null ? snapshot.CritChanceBonus : 0f,
                snapshot != null ? snapshot.CritDamageBonus : 0f);
        }

        /// 전달된 런타임 입력값을 사용해 ImpactDelay 결과값을 생성해 반환한다.
        private static float ImpactDelay(ProjectileSkillDefinition skill, SkillExecutionData snapshot)
        {
            var delay = skill != null ? skill.ImpactDelaySeconds : 0f;
            if (snapshot != null)
            {
                delay *= Mathf.Max(0f, snapshot.DamageDelayMultiplier);
            }

            return Mathf.Max(0f, delay);
        }

        /// 전달된 런타임 입력값을 사용해 BurstStatusSpec 결과값을 생성해 반환한다.
        private static ProjectileStatusHitSpec BurstStatusSpec(
            ProjectileStatusHitSpec baseStatusSpec,
            SkillExecutionData snapshot,
            int projectileIndex,
            int burstProjectileCount)
        {
            if (baseStatusSpec == null || snapshot == null)
            {
                return baseStatusSpec;
            }

            var stacksBonus = SkillExecutionRuleResolver.BurstStatusStacksBonus(snapshot, projectileIndex, burstProjectileCount);
            if (stacksBonus == 0)
            {
                return baseStatusSpec;
            }

            return CloneStatusSpecWithStacks(baseStatusSpec, Mathf.Max(1, baseStatusSpec.Stacks + stacksBonus));
        }

        /// 전달된 런타임 입력값을 사용해 CloneStatusSpecWithStacks 결과값을 생성해 반환한다.
        private static ProjectileStatusHitSpec CloneStatusSpecWithStacks(ProjectileStatusHitSpec source, int stacks)
        {
            if (source == null)
            {
                return null;
            }

            return new ProjectileStatusHitSpec
            {
                Enabled = source.Enabled,
                Kind = source.Kind,
                StatusData = source.StatusData,
                Chance = source.Chance,
                Stacks = stacks,
                DurationSeconds = source.DurationSeconds,
                MaxStacks = source.MaxStacks,
                Permanent = source.Permanent,
                RefreshDuration = source.RefreshDuration,
                ThresholdSourceStatusKind = source.ThresholdSourceStatusKind,
                ThresholdSourceMinStacks = source.ThresholdSourceMinStacks,
                ThresholdStatusSpec = source.ThresholdStatusSpec
            };
        }

        /// 전달된 skill 값을 사용해 ProjectileLifetime 결과값을 생성해 반환한다.
        private static float ProjectileLifetime(ProjectileSkillDefinition skill)
        {
            var projectile = skill.Projectile;
            if (projectile.LifetimeSeconds > 0f)
            {
                return projectile.LifetimeSeconds;
            }

            var speed = Mathf.Max(0.1f, projectile.ProjectileSpeed);
            const float battlefieldTravelDistance = 31f;
            return Mathf.Max(0.25f, battlefieldTravelDistance / speed + 0.5f);
        }
    }
}
