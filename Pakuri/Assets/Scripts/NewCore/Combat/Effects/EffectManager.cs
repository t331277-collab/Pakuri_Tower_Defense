using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Effects
{
    public readonly struct EffectVisualSpec
    {
        public EffectVisualSpec(
            string prefabPath,
            string spritePath,
            string animatorControllerPath,
            float scale,
            float scaleX,
            float scaleY,
            float scaleZ,
            int sortingOrder)
        {
            PrefabPath = prefabPath ?? string.Empty;
            SpritePath = spritePath ?? string.Empty;
            AnimatorControllerPath =
                animatorControllerPath ?? string.Empty;
            Scale = scale > 0f ? scale : 1f;
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
            SortingOrder = sortingOrder;
        }

        public string PrefabPath { get; }

        public string SpritePath { get; }

        public string AnimatorControllerPath { get; }

        public float Scale { get; }

        public float ScaleX { get; }

        public float ScaleY { get; }

        public float ScaleZ { get; }

        public int SortingOrder { get; }

        public bool UsesLocalScale =>
            ScaleX != 0f || ScaleY != 0f || ScaleZ != 0f;

        public bool HasResource =>
            !string.IsNullOrWhiteSpace(PrefabPath)
            || !string.IsNullOrWhiteSpace(SpritePath)
            || !string.IsNullOrWhiteSpace(AnimatorControllerPath);
    }

    public sealed class EffectHandle
    {
        internal EffectHandle(
            int id,
            EffectVisualSpec visual,
            CombatVector2 position,
            CombatVector2 direction)
        {
            Id = id;
            Visual = visual;
            Position = position;
            Direction = direction;
            IsActive = true;
        }

        public int Id { get; }

        public EffectVisualSpec Visual { get; }

        public string ResourcePath =>
            !string.IsNullOrWhiteSpace(Visual.PrefabPath)
                ? Visual.PrefabPath
                : Visual.SpritePath;

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
            return Create(
                new EffectVisualSpec(
                    string.Empty,
                    resourcePath,
                    string.Empty,
                    1f,
                    0f,
                    0f,
                    0f,
                    0),
                position,
                direction);
        }

        public EffectHandle Create(
            EffectVisualSpec visual,
            CombatVector2 position,
            CombatVector2 direction)
        {
            EffectHandle handle =
                new EffectHandle(nextId++, visual, position, direction);
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
