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

        public UnitRosterEntry Register(BaseUnitRuntimeModel model, Component actor)
        {
            if (model == null)
            {
                return null;
            }

            var existing = Find(model, actor);
            if (existing != null)
            {
                existing.Actor = actor;
                existing.Transform = actor != null ? actor.transform : existing.Transform;
                return existing;
            }

            var entry = new UnitRosterEntry(model, actor);
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
        public UnitRosterEntry(BaseUnitRuntimeModel model, Component actor)
        {
            Model = model;
            Actor = actor;
            Transform = actor != null ? actor.transform : null;
        }

        public BaseUnitRuntimeModel Model { get; }
        public Component Actor { get; internal set; }
        public Transform Transform { get; internal set; }

        public bool IsAlive
        {
            get
            {
                var resources = Model != null ? Model.Resources : null;
                return resources != null && resources.CurrentHealth > 0f;
            }
        }
    }
}
