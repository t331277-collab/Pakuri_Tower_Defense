using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 인게임 투사체의 위치, 충돌, 수명 주기를 처리한다.
 */
namespace Pakuri.InGame
{
    public sealed class ProjectileSkillActor : MonoBehaviour
    {
        private readonly HashSet<string> hitUnitIds = new HashSet<string>();

        private InGameCombatManager combatManager;
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
        private SkillRuntimeInstance runtime;
        private SkillSnapshot executionSnapshot;
        private string sourceSkillId;
        private bool isMagazineLastProjectile;
        private bool magazineLastProjectileTriggerFired;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;
        private ProjectileStatusHitSpec impactStatusOnHit;
        private SkillEffectDefinition[] onHitEffects;
        private SkillEffectDefinition[] onExpireEffects;
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
        private bool awaitingExpireEffects;

        /*
         * 인게임 투사체 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
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
            executionSnapshot = null;
            sourceSkillId = null;
            isMagazineLastProjectile = false;
            magazineLastProjectileTriggerFired = false;
            criticalAllowed = false;
            critChanceBonus = 0f;
            critDamageBonus = 0f;
            impactStatusOnHit = null;
            onHitEffects = System.Array.Empty<SkillEffectDefinition>();
            onExpireEffects = System.Array.Empty<SkillEffectDefinition>();
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
            awaitingExpireEffects = false;
            EnsurePhysicsRelay();
        }

        /*
         * 투사체가 사라질 좌우 경계 위치를 결정한다.
         */
        internal static float ResolveDestroyBoundaryX(
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

        /*
         * 인게임 투사체 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
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
            SkillEffectDefinition[] onHitEffectSpecs,
            SkillEffectDefinition[] onExpireEffectSpecs,
            bool enableContactDamage,
            bool stopAfterFirstHit,
            float impactDelay,
            RuntimeSkillVisualSpec runtimeImpactVisual,
            bool enableImpactArea,
            float impactAreaRadius,
            float delayedImpactDamage,
            SkillRuntimeInstance sourceRuntime,
            SkillSnapshot snapshot,
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
            onHitEffects = onHitEffectSpecs ?? System.Array.Empty<SkillEffectDefinition>();
            onExpireEffects = onExpireEffectSpecs ?? System.Array.Empty<SkillEffectDefinition>();
            contactDamageEnabled = enableContactDamage;
            stopOnFirstHit = stopAfterFirstHit;
            impactDelaySeconds = Mathf.Max(0f, impactDelay);
            impactRuntimeVisual = runtimeImpactVisual;
            hasImpactArea = enableImpactArea;
            impactRadius = Mathf.Max(0f, impactAreaRadius);
            impactDamage = Mathf.Max(0f, delayedImpactDamage);
            runtime = sourceRuntime;
            executionSnapshot = snapshot;
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

        /*
         * 투사체 충돌 처리에 필요한 물리 릴레이를 준비한다.
         */
        private void Awake()
        {
            EnsurePhysicsRelay();
        }

        /*
         * 인게임 투사체의 이동, 수명, 주기 처리를 매 프레임 갱신한다.
         */
        private void Update()
        {
            var deltaTime = Time.deltaTime;
            if (!impactArmed)
            {
                transform.position += (Vector3)(direction * speed * deltaTime);
                TryHitRosterTargets();
            }
            else if (!impactResolved)
            {
                impactDelayRemaining -= deltaTime;
                if (impactDelayRemaining <= 0f)
                {
                    ResolveImpact();
                }
            }

            maxLifetime -= deltaTime;
            if (HasPassedDestroyBoundary() || maxLifetime <= 0f)
            {
                TryExecuteOnExpireEffects();
                Destroy(gameObject);
            }
        }

        /*
         * 투사체가 충돌한 콜라이더의 유닛에게 적중 처리를 시도한다.
         */
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (combatManager == null || other == null || owner == null)
            {
                return;
            }

            var target = combatManager.UnitRegistry.FindByCollider(other);
            TryHitTarget(target);
        }

        /*
         * 적중 로스터 대상을 처리 조건을 확인하고 성공 여부를 반환한다.
         */
        private void TryHitRosterTargets()
        {
            if (combatManager == null || owner == null)
            {
                return;
            }

            var entries = combatManager.UnitRegistry.Entries;
            var hasColliderHitbox = false;
            var selfColliders = GetComponentsInChildren<Collider2D>();
            if (selfColliders != null)
            {
                for (var i = 0; i < selfColliders.Length; i++)
                {
                    if (selfColliders[i] != null && selfColliders[i].enabled)
                    {
                        hasColliderHitbox = true;
                        break;
                    }
                }
            }

            if (!hasColliderHitbox)
            {
                return;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (UnitHitboxOverlap.IsTargetInsideHitbox(selfColliders, entry) && TryHitTarget(entry))
                {
                    return;
                }
            }
        }

        /*
         * 적중 대상을 처리 조건을 확인하고 성공 여부를 반환한다.
         */
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
                resolvedDamage = ResolveHitDamage(target.Model);
                var damageResult = combatManager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, owner, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                if (!damageResult.IsDead)
                {
                    TryApplyStatus(target.Model);
                }
                SkillOnHitEffect.TryApply(
                    combatManager,
                    combatManager != null ? combatManager.UnitRegistry : null,
                    runtime,
                    executionSnapshot,
                    combatManager != null && combatManager.UnitRegistry != null ? combatManager.UnitRegistry.Find(owner) : null,
                    owner,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
            }

            TryRunProjectileHitTriggers();
            TryApplyOnHitEffects(target, hitPosition);
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
                Destroy(gameObject);
            }

            return true;
        }

        /*
         * 적중 피해를 결정한다.
         */
        private float ResolveHitDamage(UnitCombatState target)
        {
            var hitDamage = damage;
            if (runtime != null && executionSnapshot != null)
            {
                hitDamage *= runtime.ResolveConsecutiveHitDamageMultiplier(target, executionSnapshot);
            }

            return Mathf.Max(0f, hitDamage);
        }

        /*
         * 상태를 적용하고 성공 여부를 반환한다.
         */
        private void TryApplyStatus(UnitCombatState target)
        {
            StatusCombatRules.ApplyStatus(combatManager, target, statusOnHit, owner);
        }

        /*
         * 적중 효과를 적용하고 성공 여부를 반환한다.
         */
        private void TryApplyOnHitEffects(CombatUnitEntry target, Vector2 hitPosition)
        {
            if (target == null || onHitEffects == null || onHitEffects.Length == 0)
            {
                return;
            }

            var context = new SkillExecutionContext(
                combatManager,
                combatManager != null ? combatManager.UnitRegistry : null,
                combatManager != null && combatManager.UnitRegistry != null ? combatManager.UnitRegistry.Find(owner) : null,
                runtime,
                target.Model);
            SkillEffect.ExecuteOnHit(context, executionSnapshot, onHitEffects, hitPosition, target.Model);
        }

        /*
         * 투사체 적중 트리거를 실행 조건을 확인하고 성공 여부를 반환한다.
         */
        private void TryRunProjectileHitTriggers()
        {
            if (!isMagazineLastProjectile || magazineLastProjectileTriggerFired)
            {
                return;
            }

            magazineLastProjectileTriggerFired = true;
            SkillTrigger.ExecuteProjectileHit(
                combatManager,
                combatManager != null ? combatManager.UnitRegistry : null,
                owner,
                sourceSkillId,
                true,
                transform.position);
        }

        /*
         * 분기 피해를 적용하고 성공 여부를 반환한다.
         */
        private void TryApplyBranchDamage(
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryDamage)
        {
            if (impactArmed
                || combatManager == null
                || combatManager.UnitRegistry == null
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

            var candidates = combatManager.UnitRegistry.Entries;
            var radiusSq = branchDamageOnHit.SearchRadius * branchDamageOnHit.SearchRadius;
            var selectedTargets = new HashSet<UnitCombatState>();
            var branchDamage = primaryDamage * Mathf.Max(0f, branchDamageOnHit.DamageMultiplier);
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
                    true);
                SpawnBranchDamageLine(hitPosition, targetPosition);
            }
        }

        /*
         * 가장 가까운 분기 대상을 찾는다.
         */
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

        /*
         * 분기 피해 직선을 생성한다.
         */
        private void SpawnBranchDamageLine(Vector2 origin, Vector2 target)
        {
            var shader = Shader.Find("Sprites/Default");
            if (shader == null || combatManager.Effects == null)
            {
                return;
            }

            const float durationSeconds = 0.12f;
            var lineObject = combatManager.Effects.CreateRuntimeSkillObject(
                "InGameBranchDamageLine",
                Vector3.zero,
                Quaternion.identity);
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
            combatManager.Effects.DestroyAfter(lineObject, durationSeconds);
        }

        /*
         * 두 유닛이 같은 진영인지 확인한다.
         */
        private bool IsSameSide(UnitCombatState target)
        {
            var ownerIdentity = owner.Identity;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
        }

        /*
         * 투사체가 제거 경계를 통과했는지 확인한다.
         */
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

        /*
         * 충돌을 후속 처리를 준비한다.
         */
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

        /*
         * 충돌을 결정한다.
         */
        private void ResolveImpact()
        {
            if (impactResolved || combatManager == null)
            {
                return;
            }

            impactResolved = true;
            var impactVisualLifetime = 0.05f;
            var effects = combatManager.Effects;
            GameObject instance = null;
            if (effects != null && EffectManager.HasVisual(impactRuntimeVisual))
            {
                instance = effects.CreateRuntimeVisual(
                    impactRuntimeVisual,
                    string.IsNullOrWhiteSpace(sourceSkillId) ? "ProjectileImpact" : $"ProjectileImpact_{sourceSkillId}",
                    impactCenter,
                    Quaternion.identity,
                    includeHitbox: false);
            }

            if (instance != null)
            {
                impactVisualLifetime = effects.DestroyAfterAnimation(instance, 0.1f);
            }

            if (hasImpactArea)
            {
                ZoneSkillActor.ApplyAreaTick(
                    combatManager,
                    combatManager.UnitRegistry != null ? combatManager.UnitRegistry.Find(owner) : null,
                    combatManager.UnitRegistry,
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
                    executionSnapshot);
            }

            if (onExpireEffects != null && onExpireEffects.Length > 0 && combatManager != null)
            {
                awaitingExpireEffects = true;
                combatManager.StartCoroutine(ExecuteOnExpireAfterDelay(impactVisualLifetime));
                return;
            }

            Destroy(gameObject);
        }

        /*
         * 투사체 수명 종료 후 지연 효과를 실행한다.
         */
        private System.Collections.IEnumerator ExecuteOnExpireAfterDelay(float delaySeconds)
        {
            var delay = Mathf.Max(0.01f, delaySeconds);
            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            TryExecuteOnExpireEffects();
            Destroy(gameObject);
        }

        /*
         * 종료 효과를 실행하고 성공 여부를 반환한다.
         */
        private void TryExecuteOnExpireEffects()
        {
            if (!awaitingExpireEffects && !impactResolved)
            {
                return;
            }

            if (onExpireEffects == null || onExpireEffects.Length == 0 || combatManager == null || combatManager.UnitRegistry == null)
            {
                onExpireEffects = System.Array.Empty<SkillEffectDefinition>();
                return;
            }

            var sourceEntry = combatManager.UnitRegistry.Find(owner);
            var context = new SkillExecutionContext(
                combatManager,
                combatManager.UnitRegistry,
                sourceEntry,
                runtime,
                impactTarget);
            SkillEffect.ExecuteOnExpire(context, executionSnapshot, onExpireEffects, impactCenter);
            onExpireEffects = System.Array.Empty<SkillEffectDefinition>();
            awaitingExpireEffects = false;
        }

        /*
         * 충돌 대상 지정을 구성한다.
         */
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

        /*
         * 투사체 충돌을 전달할 물리 릴레이를 준비한다.
         */
        private void EnsurePhysicsRelay()
        {
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            var body = GetComponent<Rigidbody2D>();
            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody2D>();
            }

            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.simulated = true;

            // Canvas/AutoBtn switches the selected 1P A skill from mouse-held manual fire
            // to automatic combat fire. This projectile only relays movement and hits.
        }

        /*
         * 회전을 결정한다.
         */
        private static Quaternion ResolveRotation(Vector3 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }
    }

    /*
     * 투사체 상태 적중 설정에 필요한 값을 보관한다.
     */
    public sealed class ProjectileStatusHitSpec
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

    /*
     * 투사체 분기 피해 설정에 필요한 값을 보관한다.
     */
    public sealed class ProjectileBranchDamageSpec
    {
        public bool Enabled;
        public float Chance;
        public int Count;
        public float DamageMultiplier = 1f;
        public float SearchRadius;
    }
}
