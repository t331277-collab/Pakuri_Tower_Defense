using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class UnitRosterService
    {
        private readonly List<UnitRosterEntry> entries = new List<UnitRosterEntry>();
        private readonly List<UnitRosterEntry> players = new List<UnitRosterEntry>();
        private readonly List<UnitRosterEntry> enemies = new List<UnitRosterEntry>();

        public IReadOnlyList<UnitRosterEntry> Entries => entries;
        public IReadOnlyList<UnitRosterEntry> Players => players;
        public IReadOnlyList<UnitRosterEntry> Enemies => enemies;

        public int Count => entries.Count;
        public int PlayerCount => players.Count;
        public int EnemyCount => enemies.Count;

        public UnitRosterEntry Register(BaseUnitRuntimeModel model, Component actor, Transform hitboxRoot = null)
        {
            if (model == null)
            {
                return null;
            }

            var existing = Find(model, actor);
            if (existing != null)
            {
                existing.SetActor(actor);
                existing.SetHitboxRoot(hitboxRoot);
                return existing;
            }

            var entry = new UnitRosterEntry(model, actor, hitboxRoot);
            entries.Add(entry);

            if (model.Identity != null && model.Identity.Side == UnitSide.Enemy)
            {
                enemies.Add(entry);
            }
            else
            {
                players.Add(entry);
            }

            return entry;
        }

        public bool Unregister(BaseUnitRuntimeModel model)
        {
            var entry = Find(model, null);
            if (entry == null)
            {
                return false;
            }

            entries.Remove(entry);
            players.Remove(entry);
            enemies.Remove(entry);
            return true;
        }

        public void Clear()
        {
            entries.Clear();
            players.Clear();
            enemies.Clear();
        }

        public UnitRosterEntry Find(BaseUnitRuntimeModel model)
        {
            return Find(model, null);
        }

        private UnitRosterEntry Find(BaseUnitRuntimeModel model, Component actor)
        {
            var unitId = model != null && model.Identity != null ? model.Identity.UnitId : null;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(unitId)
                    && entry.Model != null
                    && entry.Model.Identity != null
                    && entry.Model.Identity.UnitId == unitId)
                {
                    return entry;
                }

                if (actor != null && entry.Actor == actor)
                {
                    return entry;
                }
            }

            return null;
        }
    }

    public sealed class UnitRosterEntry
    {
        private Collider2D[] cachedHitboxColliders;

        public UnitRosterEntry(BaseUnitRuntimeModel model, Component actor, Transform hitboxRoot = null)
        {
            Model = model;
            SetActor(actor);
            SetHitboxRoot(hitboxRoot);
        }

        public BaseUnitRuntimeModel Model { get; }
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
    }

    internal static class UnitHitboxUtility
    {
        public static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, UnitRosterEntry target)
        {
            if (hitboxColliders == null || target == null || target.Model == null || !target.IsAlive)
            {
                return false;
            }

            var targetPoint = target.ResolveTargetPoint();
            var targetColliders = target.GetHitboxColliders();
            for (var i = 0; i < hitboxColliders.Length; i++)
            {
                var hitbox = hitboxColliders[i];
                if (hitbox == null || !hitbox.enabled)
                {
                    continue;
                }

                if (hitbox.OverlapPoint(targetPoint))
                {
                    return true;
                }

                for (var j = 0; j < targetColliders.Length; j++)
                {
                    var targetCollider = targetColliders[j];
                    if (targetCollider == null || !targetCollider.enabled)
                    {
                        continue;
                    }

                    if (hitbox.Distance(targetCollider).isOverlapped)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
