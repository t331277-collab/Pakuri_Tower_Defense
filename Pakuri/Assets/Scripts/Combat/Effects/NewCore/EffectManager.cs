using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Effects
{
    public sealed class EffectHandle
    {
        internal EffectHandle(int id, string resourcePath, CombatVector2 position, CombatVector2 direction)
        {
            Id = id;
            ResourcePath = resourcePath ?? string.Empty;
            Position = position;
            Direction = direction;
            IsActive = true;
        }

        public int Id { get; }

        public string ResourcePath { get; }

        public CombatVector2 Position { get; internal set; }

        public CombatVector2 Direction { get; internal set; }

        public bool IsActive { get; internal set; }
    }

    public sealed class EffectManager
    {
        private readonly List<EffectHandle> handles = new List<EffectHandle>();
        private readonly IReadOnlyList<EffectHandle> readOnlyHandles;
        private int nextId = 1;

        public EffectManager()
        {
            readOnlyHandles = new ReadOnlyCollection<EffectHandle>(handles);
        }

        public IReadOnlyList<EffectHandle> ActiveEffects => readOnlyHandles;

        public EffectHandle Create(
            string resourcePath,
            CombatVector2 position,
            CombatVector2 direction)
        {
            EffectHandle handle =
                new EffectHandle(nextId++, resourcePath, position, direction);
            handles.Add(handle);
            return handle;
        }

        public bool TryUpdate(
            EffectHandle handle,
            CombatVector2 position,
            CombatVector2 direction)
        {
            if (handle == null || !handle.IsActive || !handles.Contains(handle))
            {
                return false;
            }

            handle.Position = position;
            handle.Direction = direction;
            return true;
        }

        public bool Remove(EffectHandle handle)
        {
            if (handle == null || !handle.IsActive || !handles.Remove(handle))
            {
                return false;
            }

            handle.IsActive = false;
            return true;
        }

        public void Clear()
        {
            for (int index = 0; index < handles.Count; index++)
            {
                handles[index].IsActive = false;
            }

            handles.Clear();
        }
    }
}
