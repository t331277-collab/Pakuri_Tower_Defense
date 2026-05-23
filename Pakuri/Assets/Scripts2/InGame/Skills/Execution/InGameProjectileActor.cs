using System.Collections.Generic;
using Pakuri.Combat;
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
        private float hitRadius = 0.5f;
        private bool destroyWhenGreaterThanBoundary = true;
        private ProjectileStatusHitSpec statusOnHit;
        private ProjectileBranchHitSpec branchOnHit;
        private string sourceSkillId;
        private bool isMagazineLastProjectile;
        private bool magazineLastProjectileTriggerFired;
        private bool criticalAllowed;
        private float critChanceBonus;
        private float critDamageBonus;

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
            sourceSkillId = null;
            isMagazineLastProjectile = false;
            magazineLastProjectileTriggerFired = false;
            criticalAllowed = false;
            critChanceBonus = 0f;
            critDamageBonus = 0f;
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
            transform.position += (Vector3)(direction * speed * deltaTime);
            maxLifetime -= deltaTime;
            TryHitRosterTargets();

            if (HasPassedDestroyBoundary() || maxLifetime <= 0f)
            {
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

            var radiusSq = hitRadius * hitRadius;
            var current = transform.position;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (hasColliderHitbox)
                {
                    if (UnitHitboxUtility.IsTargetInsideHitbox(selfColliders, entry) && TryHitTarget(entry))
                    {
                        return;
                    }
                }
                else
                {
                    var offset = entry.ResolveTargetPoint() - current;
                    offset.z = 0f;
                    if (offset.sqrMagnitude <= radiusSq && TryHitTarget(entry))
                    {
                        return;
                    }
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

            combatManager.ApplyDamage(target.Model, damage, damageAttribute, owner, criticalAllowed, critChanceBonus, critDamageBonus, sourceSkillId);
            TryApplyStatus(target.Model);
            TryRunProjectileHitTriggers();
            TrySpawnBranches(target);
            remainingHits--;
            if (remainingHits <= 0)
            {
                Destroy(gameObject);
            }

            return true;
        }

        private void TryApplyStatus(BaseUnitRuntimeModel target)
        {
            SkillStatusApplyUtility.TryApplyStatus(combatManager, target, statusOnHit, owner);
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
            if (combatManager == null || hitTarget == null || hitTarget.Transform == null || branchOnHit == null || !branchOnHit.Enabled)
            {
                return;
            }

            if (branchOnHit.ProjectilePrefab == null || Random.value > Mathf.Clamp01(branchOnHit.Chance))
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

            var instance = effects.InstantiateSkillPrefab(
                branchOnHit.ProjectilePrefab,
                origin,
                ResolveRotation(branchDirection));
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
            return destroyWhenGreaterThanBoundary
                ? transform.position.x > destroyBeyondX
                : transform.position.x < destroyBeyondX;
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
        public GameObject ProjectilePrefab;
        public float Chance;
        public int Count;
        public float DamageMultiplier = 1f;
        public float SearchRadius;

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
                ProjectilePrefab = ProjectilePrefab,
                Chance = Chance,
                Count = Count,
                DamageMultiplier = DamageMultiplier,
                SearchRadius = SearchRadius
            };
        }
    }
}
