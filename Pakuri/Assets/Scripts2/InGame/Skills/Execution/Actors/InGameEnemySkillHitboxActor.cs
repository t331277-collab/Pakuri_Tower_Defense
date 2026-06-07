using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameEnemySkillHitboxActor : MonoBehaviour
    {
        private readonly HashSet<string> hitUnitIds = new HashSet<string>();

        private InGameCombatManager combatManager;
        private BaseUnitRuntimeModel owner;
        private DamageAttribute damageAttribute = DamageAttribute.Physical;
        private float damage;
        private float lifetimeSeconds;
        private int remainingHits = 1;

        public void Initialize(
            InGameCombatManager manager,
            BaseUnitRuntimeModel source,
            float baseDamage,
            DamageAttribute attribute,
            float radius,
            float lifetime,
            int maxHits = 1)
        {
            combatManager = manager;
            owner = source;
            damage = Mathf.Max(0f, baseDamage);
            damageAttribute = attribute;
            lifetimeSeconds = Mathf.Max(0.05f, lifetime);
            remainingHits = Mathf.Max(1, maxHits);
            EnsurePhysicsRelay();
        }

        private void Awake()
        {
            EnsurePhysicsRelay();
        }

        private void Update()
        {
            lifetimeSeconds -= Time.deltaTime;
            TryHitRosterTargets();

            if (lifetimeSeconds <= 0f)
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

            TryHitTarget(combatManager.FindUnitByCollider(other));
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

            combatManager.ApplyDamage(target.Model, damage, damageAttribute, owner, true);
            remainingHits--;
            if (remainingHits <= 0)
            {
                Destroy(gameObject);
            }

            return true;
        }

        private bool IsSameSide(BaseUnitRuntimeModel target)
        {
            var ownerIdentity = owner != null ? owner.Identity : null;
            var targetIdentity = target != null ? target.Identity : null;
            return ownerIdentity != null
                && targetIdentity != null
                && ownerIdentity.Side == targetIdentity.Side;
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
        }
    }
}
