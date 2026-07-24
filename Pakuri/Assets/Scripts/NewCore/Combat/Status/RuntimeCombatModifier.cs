using System;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Status
{
    public sealed class RuntimeCombatModifier
    {
        internal RuntimeCombatModifier(
            string kind,
            float value,
            string filter,
            string secondaryFilter,
            UnitBaseModel source,
            float durationSeconds)
        {
            Kind = string.IsNullOrEmpty(kind)
                ? throw new ArgumentException("Modifier kind is required.", nameof(kind))
                : kind;
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value));
            }
            if (durationSeconds < 0f
                || float.IsNaN(durationSeconds)
                || float.IsInfinity(durationSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }
            Value = value;
            Filter = filter;
            SecondaryFilter = secondaryFilter;
            Source = source ?? throw new ArgumentNullException(nameof(source));
            RemainingDuration = durationSeconds;
        }

        public string Kind { get; }

        public float Value { get; }

        public string Filter { get; }

        public string SecondaryFilter { get; }

        public UnitBaseModel Source { get; }

        public float RemainingDuration { get; private set; }

        public bool IsExpired => RemainingDuration <= 0f;

        internal void Tick(float deltaTime)
        {
            RemainingDuration = Math.Max(0f, RemainingDuration - deltaTime);
        }
    }
}
