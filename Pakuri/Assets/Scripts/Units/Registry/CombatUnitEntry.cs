using System;
using UnityEngine;

/*
 * 전투 유닛 하나의 모델, Actor, Transform과 피격 Collider를 연결한다.
 */
namespace Pakuri.InGame
{
    public sealed class CombatUnitEntry
    {
        private Collider2D[] cachedHitboxColliders;

        public CombatUnitEntry(UnitCombatState model, Component actor, Transform hitboxRoot = null)
        {
            Model = model;
            SetActor(actor);
            SetHitboxRoot(hitboxRoot);
        }

        public UnitCombatState Model { get; }
        public Component Actor { get; internal set; }
        public Transform Transform { get; internal set; }
        public Transform HitboxRoot { get; internal set; }

        public bool IsAlive
        {
            get
            {
                var resources = Model != null ? Model.Resources : null;
                return resources != null && resources.CurrentHealth > 0f;
            }
        }

        internal void SetActor(Component actor)
        {
            Actor = actor;
            Transform = actor != null ? actor.transform : null;
            if (HitboxRoot == null)
            {
                HitboxRoot = Transform;
            }

            cachedHitboxColliders = null;
        }

        internal void SetHitboxRoot(Transform hitboxRoot)
        {
            HitboxRoot = hitboxRoot != null ? hitboxRoot : Transform;
            cachedHitboxColliders = null;
        }

        public Vector3 ResolveTargetPoint()
        {
            var root = HitboxRoot != null ? HitboxRoot : Transform;
            return root != null ? root.position : Vector3.zero;
        }

        public Collider2D[] GetHitboxColliders()
        {
            if (cachedHitboxColliders != null)
            {
                return cachedHitboxColliders;
            }

            var root = HitboxRoot != null ? HitboxRoot : Transform;
            cachedHitboxColliders = root != null
                ? root.GetComponentsInChildren<Collider2D>()
                : Array.Empty<Collider2D>();
            return cachedHitboxColliders;
        }

        public bool ContainsTransform(Transform candidate)
        {
            if (candidate == null)
            {
                return false;
            }

            if (Transform != null && (candidate == Transform || candidate.IsChildOf(Transform)))
            {
                return true;
            }

            return HitboxRoot != null && (candidate == HitboxRoot || candidate.IsChildOf(HitboxRoot));
        }

        internal void ShowDamage(float damageAmount, bool isDead)
        {
            if (Actor == null || damageAmount <= 0f)
            {
                return;
            }

            if (Actor is MonsterActor monster)
            {
                monster.ShowDamage(damageAmount);
                if (!isDead)
                {
                    monster.TryPlayHitAnimation();
                }

                return;
            }

            if (Actor is EnemyActor enemy)
            {
                enemy.ShowDamage(damageAmount);
            }
        }

        internal bool RefreshDisplay()
        {
            if (Actor is MonsterActor monster)
            {
                monster.RefreshDisplay();
                return true;
            }

            if (Actor is EnemyActor enemy)
            {
                enemy.RefreshDisplay();
                return true;
            }

            if (Actor is NexusActor nexus)
            {
                nexus.RefreshDisplay();
                return true;
            }

            return false;
        }

        internal void HandleDefeat()
        {
            if (Actor == null)
            {
                return;
            }

            if (Actor is NexusActor nexus)
            {
                nexus.RefreshDisplay();
                return;
            }

            if (Actor is MonsterActor monster)
            {
                monster.Defeat();
                return;
            }

            UnityEngine.Object.Destroy(Actor.gameObject, 0.95f);
        }
    }
}
