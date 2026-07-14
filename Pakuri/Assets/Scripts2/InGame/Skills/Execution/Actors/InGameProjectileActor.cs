using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameProjectileActor : MonoBehaviour
    {
        private readonly HashSet<string> hitUnitIds = new HashSet<string>();

        private InGameCombatManager combatManager;
        private BaseUnitRuntimeModel owner;
        private Vector2 direction = Vector2.right;
        private DamageAttribute damageAttribute = DamageAttribute.Physical;
        private float damage;
        private float speed;
        private float destroyBeyondX;
        private float maxLifetime;
        private int remainingHits = 1;
        private bool destroyWhenGreaterThanBoundary = true;
        private ProjectileStatusHitSpec statusOnHit;
        private ProjectileBranchHitSpec branchOnHit;
        private SkillRuntimeInstance runtime;
        private SkillExecutionSnapshot executionSnapshot;
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
        private GameObject impactEffectPrefab;
        private RuntimeSkillVisualSpec impactRuntimeVisual;
        private bool hasImpactArea;
        private float impactRadius;
        private float impactDamage;
        private bool impactArmed;
        private float impactDelayRemaining;
        private Vector2 impactCenter;
        private BaseUnitRuntimeModel impactTarget;
        private bool impactResolved;
        private bool awaitingExpireEffects;

        public void Initialize(
            InGameCombatManager manager,
            BaseUnitRuntimeModel source,
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
            branchOnHit = null;
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
            impactEffectPrefab = null;
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

        public void Initialize(
            InGameCombatManager manager,
            BaseUnitRuntimeModel source,
            Vector2 fireDirection,
            float projectileSpeed,
            float baseDamage,
            DamageAttribute attribute,
            int pierceCount,
            float boundaryX,
            float lifetimeSeconds,
            ProjectileStatusHitSpec statusSpec,
            ProjectileBranchHitSpec branchSpec,
            ProjectileStatusHitSpec impactStatusSpec,
            SkillEffectDefinition[] onHitEffectSpecs,
            SkillEffectDefinition[] onExpireEffectSpecs,
            bool enableContactDamage,
            bool stopAfterFirstHit,
            float impactDelay,
            GameObject impactEffect,
            RuntimeSkillVisualSpec runtimeImpactVisual,
            bool enableImpactArea,
            float impactAreaRadius,
            float delayedImpactDamage,
            SkillRuntimeInstance sourceRuntime,
            SkillExecutionSnapshot snapshot,
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
            branchOnHit = branchSpec;
            impactStatusOnHit = impactStatusSpec;
            onHitEffects = onHitEffectSpecs ?? System.Array.Empty<SkillEffectDefinition>();
            onExpireEffects = onExpireEffectSpecs ?? System.Array.Empty<SkillEffectDefinition>();
            contactDamageEnabled = enableContactDamage;
            stopOnFirstHit = stopAfterFirstHit;
            impactDelaySeconds = Mathf.Max(0f, impactDelay);
            impactEffectPrefab = impactEffect;
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

        private void Awake()
        {
            EnsurePhysicsRelay();
        }

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

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (combatManager == null || other == null || owner == null)
            {
                return;
            }

            var target = combatManager.FindUnitByCollider(other);
            TryHitTarget(target);
        }

        private void TryHitRosterTargets()
        {
            if (combatManager == null || owner == null)
            {
                return;
            }

            var entries = combatManager.Roster.Entries;
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

                if (UnitHitboxUtility.IsTargetInsideHitbox(selfColliders, entry) && TryHitTarget(entry))
                {
                    return;
                }
            }
        }

        private bool TryHitTarget(UnitRosterEntry target)
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
                combatManager.ApplyDamage(target.Model, resolvedDamage, damageAttribute, owner, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
                TryApplyStatus(target.Model);
                SkillOnHitAdditionalDamageUtility.TryApply(
                    combatManager,
                    combatManager != null ? combatManager.Roster : null,
                    runtime,
                    executionSnapshot,
                    combatManager != null && combatManager.Roster != null ? combatManager.Roster.Find(owner) : null,
                    owner,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
            }

            TryRunProjectileHitTriggers();
            TryApplyOnHitEffects(target, hitPosition);
            TrySpawnBranches(target);
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

        private float ResolveHitDamage(BaseUnitRuntimeModel target)
        {
            var hitDamage = damage;
            if (runtime != null && executionSnapshot != null)
            {
                hitDamage *= runtime.ResolveConsecutiveHitDamageMultiplier(target, executionSnapshot);
            }

            return Mathf.Max(0f, hitDamage);
        }

        private void TryApplyStatus(BaseUnitRuntimeModel target)
        {
            SkillStatusApplyUtility.TryApplyStatus(combatManager, target, statusOnHit, owner);
        }

        private void TryApplyOnHitEffects(UnitRosterEntry target, Vector2 hitPosition)
        {
            if (target == null || onHitEffects == null || onHitEffects.Length == 0)
            {
                return;
            }

            var context = new SkillExecutionContext(
                combatManager,
                combatManager != null ? combatManager.Roster : null,
                combatManager != null && combatManager.Roster != null ? combatManager.Roster.Find(owner) : null,
                runtime,
                0f,
                target.Model);
            SkillMultiEffectExecutor.ExecuteOnHit(context, executionSnapshot, onHitEffects, hitPosition, target.Model);
        }

        private void TryRunProjectileHitTriggers()
        {
            if (!isMagazineLastProjectile || magazineLastProjectileTriggerFired)
            {
                return;
            }

            magazineLastProjectileTriggerFired = true;
            SkillTriggerRuntime.ExecuteProjectileHit(
                combatManager,
                combatManager != null ? combatManager.Roster : null,
                owner,
                sourceSkillId,
                true,
                transform.position);
        }

        private void TrySpawnBranches(UnitRosterEntry hitTarget)
        {
            if (impactArmed || combatManager == null || hitTarget == null || hitTarget.Transform == null || branchOnHit == null || !branchOnHit.Enabled)
            {
                return;
            }

            if (!branchOnHit.HasProjectileVisual || Random.value > Mathf.Clamp01(branchOnHit.Chance))
            {
                return;
            }

            var candidates = combatManager.Roster.Entries;
            var radiusSq = branchOnHit.SearchRadius * branchOnHit.SearchRadius;
            var spawned = 0;
            for (var i = 0; i < branchOnHit.Count; i++)
            {
                var target = FindNearestBranchTarget(candidates, hitTarget, radiusSq);
                if (target != null && target.Transform != null)
                {
                    SpawnBranchProjectile(hitTarget, target);
                    spawned++;
                    var targetId = target.Model != null && target.Model.Identity != null ? target.Model.Identity.UnitId : null;
                    if (!string.IsNullOrWhiteSpace(targetId))
                    {
                        branchOnHit.MarkBranchedTarget(targetId);
                    }

                    continue;
                }

                SpawnFallbackBranchProjectile(hitTarget, i);
                spawned++;
            }

            if (spawned > 0)
            {
                branchOnHit.ClearBranchedTargets();
            }
        }

        private UnitRosterEntry FindNearestBranchTarget(
            IReadOnlyList<UnitRosterEntry> candidates,
            UnitRosterEntry hitTarget,
            float radiusSq)
        {
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var origin = hitTarget.Transform.position;
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

                var unitId = candidate.Model.Identity != null ? candidate.Model.Identity.UnitId : null;
                if (!string.IsNullOrWhiteSpace(unitId) && branchOnHit.HasBranchedTarget(unitId))
                {
                    continue;
                }

                var offset = candidate.Transform.position - origin;
                offset.z = 0f;
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

        private void SpawnBranchProjectile(UnitRosterEntry hitTarget, UnitRosterEntry branchTarget)
        {
            var origin = hitTarget.Transform.position;
            var directionToTarget = branchTarget.Transform.position - origin;
            directionToTarget.z = 0f;
            if (directionToTarget.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            SpawnBranchProjectile(hitTarget, directionToTarget);
        }

        private void SpawnFallbackBranchProjectile(UnitRosterEntry hitTarget, int branchIndex)
        {
            if (hitTarget == null || hitTarget.Transform == null)
            {
                return;
            }

            var baseAngle = branchOnHit.Count > 1
                ? Mathf.Lerp(-18f, 18f, branchIndex / (float)(branchOnHit.Count - 1))
                : 0f;
            var jitter = Random.Range(-12f, 12f);
            var directionToTarget = Quaternion.Euler(0f, 0f, baseAngle + jitter) * Vector2.right;
            SpawnBranchProjectile(hitTarget, directionToTarget);
        }

        private void SpawnBranchProjectile(UnitRosterEntry hitTarget, Vector2 branchDirection)
        {
            if (hitTarget == null || hitTarget.Transform == null || branchDirection.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            var origin = hitTarget.Transform.position;
            var effects = combatManager.Effects;
            if (effects == null)
            {
                return;
            }

            var rotation = ResolveRotation(branchDirection);
            var instance = RuntimeSkillVisualFactory.HasVisual(branchOnHit.RuntimeVisual)
                ? RuntimeSkillVisualFactory.Create(
                    effects,
                    branchOnHit.RuntimeVisual,
                    "InGameProjectileBranch",
                    origin,
                    rotation,
                    hitboxIsTrigger: true)
                : effects.InstantiateSkillPrefab(branchOnHit.ProjectilePrefab, origin, rotation);
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<InGameProjectileActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<InGameProjectileActor>();
            }

            var ignoredUnitId = hitTarget.Model != null && hitTarget.Model.Identity != null
                ? hitTarget.Model.Identity.UnitId
                : null;
            actor.Initialize(
                combatManager,
                owner,
                branchDirection,
                speed,
                damage * Mathf.Max(0f, branchOnHit.DamageMultiplier),
                damageAttribute,
                0,
                destroyBeyondX,
                Mathf.Max(0.1f, maxLifetime),
                statusOnHit,
                branchOnHit.CloneForChild(),
                null,
                null,
                null,
                true,
                false,
                0f,
                null,
                null,
                false,
                0f,
                0f,
                null,
                null,
                ignoredUnitId,
                null,
                false,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus);
        }

        private bool IsSameSide(BaseUnitRuntimeModel target)
        {
            var ownerIdentity = owner.Identity;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
        }

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

        private void ArmImpact(UnitRosterEntry target, Vector2 hitPosition)
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

        private void ResolveImpact()
        {
            if (impactResolved || combatManager == null)
            {
                return;
            }

            impactResolved = true;
            var impactVisualLifetime = 0.05f;
            if (RuntimeSkillVisualFactory.HasVisual(impactRuntimeVisual) && combatManager.Effects != null)
            {
                var instance = RuntimeSkillVisualFactory.Create(
                    combatManager.Effects,
                    impactRuntimeVisual,
                    string.IsNullOrWhiteSpace(sourceSkillId) ? "InGameProjectileImpact" : $"InGameProjectileImpact_{sourceSkillId}",
                    impactCenter,
                    Quaternion.identity,
                    includeHitbox: false);
                if (instance != null)
                {
                    impactVisualLifetime = SkillVisualSpawnUtility.ResolveVisualLifetime(instance, 0.1f);
                    Destroy(instance, impactVisualLifetime);
                }
            }
            else if (impactEffectPrefab != null && combatManager.Effects != null)
            {
                var instance = combatManager.Effects.InstantiateSkillPrefab(impactEffectPrefab, impactCenter, Quaternion.identity);
                if (instance != null)
                {
                    impactVisualLifetime = SkillVisualSpawnUtility.ResolveVisualLifetime(instance, 0.1f);
                    Destroy(instance, impactVisualLifetime);
                }
            }

            if (hasImpactArea)
            {
                InGameZoneSkillActor.ApplyAreaTick(
                    combatManager,
                    combatManager.Roster != null ? combatManager.Roster.Find(owner) : null,
                    combatManager.Roster,
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

        private void TryExecuteOnExpireEffects()
        {
            if (!awaitingExpireEffects && !impactResolved)
            {
                return;
            }

            if (onExpireEffects == null || onExpireEffects.Length == 0 || combatManager == null || combatManager.Roster == null)
            {
                onExpireEffects = System.Array.Empty<SkillEffectDefinition>();
                return;
            }

            var sourceEntry = combatManager.Roster.Find(owner);
            var context = new SkillExecutionContext(
                combatManager,
                combatManager.Roster,
                sourceEntry,
                runtime,
                0f,
                impactTarget);
            SkillMultiEffectExecutor.ExecuteOnExpire(context, executionSnapshot, onExpireEffects, impactCenter);
            onExpireEffects = System.Array.Empty<SkillEffectDefinition>();
            awaitingExpireEffects = false;
        }

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

    public sealed class ProjectileStatusHitSpec
    {
        public bool Enabled;
        public StatusEffectKind Kind;
        public StatusEffectData StatusData;
        public float Chance;
        public int Stacks;
        public float DurationSeconds;
        public int MaxStacks;
        public bool Permanent;
        public bool RefreshDuration = true;
        public string ThresholdSourceStatusId;
        public int ThresholdSourceMinStacks;
        public ProjectileStatusHitSpec ThresholdStatusSpec;
    }

    public sealed class ProjectileBranchHitSpec
    {
        private readonly HashSet<string> branchedTargets = new HashSet<string>();

        public bool Enabled;
        public RuntimeSkillVisualSpec RuntimeVisual;
        public GameObject ProjectilePrefab;
        public float Chance;
        public int Count;
        public float DamageMultiplier = 1f;
        public float SearchRadius;

        public bool HasProjectileVisual => RuntimeSkillVisualFactory.HasVisual(RuntimeVisual) || ProjectilePrefab != null;

        public bool HasBranchedTarget(string unitId)
        {
            return !string.IsNullOrWhiteSpace(unitId) && branchedTargets.Contains(unitId);
        }

        public void MarkBranchedTarget(string unitId)
        {
            if (!string.IsNullOrWhiteSpace(unitId))
            {
                branchedTargets.Add(unitId);
            }
        }

        public void ClearBranchedTargets()
        {
            branchedTargets.Clear();
        }

        public ProjectileBranchHitSpec CloneForChild()
        {
            return new ProjectileBranchHitSpec
            {
                Enabled = Enabled,
                RuntimeVisual = RuntimeVisual,
                ProjectilePrefab = ProjectilePrefab,
                Chance = Chance,
                Count = Count,
                DamageMultiplier = DamageMultiplier,
                SearchRadius = SearchRadius
            };
        }
    }
}
