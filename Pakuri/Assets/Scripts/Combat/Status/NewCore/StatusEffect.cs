using System;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Status
{
    public sealed class StatusEffect
    {
        internal StatusEffect(
            StatusDefinition definition,
            UnitBaseModel applyingUnit,
            UnitBaseModel affectedUnit,
            float? durationSeconds,
            int? stackAmount,
            string sourceSkillId)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ApplyingUnit =
                applyingUnit ?? throw new ArgumentNullException(nameof(applyingUnit));
            AffectedUnit =
                affectedUnit ?? throw new ArgumentNullException(nameof(affectedUnit));
            SourceSkillId = sourceSkillId;
            CurrentStacks = ResolveStackAmount(stackAmount);
            RemainingDuration = ResolveDuration(durationSeconds);
        }

        public StatusDefinition Definition { get; }

        public UnitBaseModel ApplyingUnit { get; }

        public UnitBaseModel AffectedUnit { get; }

        public float? RemainingDuration { get; private set; }

        public int CurrentStacks { get; private set; }

        public string SourceSkillId { get; }

        public float TrackedIncomingDamage { get; private set; }

        public string LastTrackedAttribute { get; private set; }

        public bool IsPermanent => Definition.is_permanent == true;

        public bool IsExpired =>
            !IsPermanent && RemainingDuration.HasValue && RemainingDuration.Value <= 0f;

        internal void Refresh(float? durationSeconds, int? stackAmount)
        {
            int refreshedStacks =
                AddStacks(CurrentStacks, ResolveStackAmount(stackAmount));
            float? refreshedDuration = ResolveDuration(durationSeconds);

            CurrentStacks = refreshedStacks;
            RemainingDuration = refreshedDuration;
        }

        internal void Tick(float deltaTime)
        {
            if (IsPermanent || !RemainingDuration.HasValue)
            {
                return;
            }

            RemainingDuration = Math.Max(0f, RemainingDuration.Value - deltaTime);
        }

        public void Extend(float durationSeconds)
        {
            if (durationSeconds < 0f
                || float.IsNaN(durationSeconds)
                || float.IsInfinity(durationSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }
            if (!IsPermanent && RemainingDuration.HasValue)
            {
                RemainingDuration += durationSeconds;
            }
        }

        internal int RemoveStacks(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }
            int removed = Math.Min(CurrentStacks, amount);
            CurrentStacks -= removed;
            return removed;
        }

        internal void TrackIncomingDamage(string attribute, float amount)
        {
            if (amount <= 0f) return;
            TrackedIncomingDamage += amount;
            LastTrackedAttribute = attribute;
        }

        private int ResolveStackAmount(int? stackAmount)
        {
            int resolved = stackAmount
                ?? Definition.base_stack_amount
                ?? throw new ArgumentException(
                    "Status Definition has no base_stack_amount.",
                    nameof(Definition));
            if (resolved < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(stackAmount));
            }

            return AddStacks(0, resolved);
        }

        private int AddStacks(int current, int amount)
        {
            int updated = checked(current + amount);
            int maximum = (Definition.max_stacks ?? 0)
                + (int)AffectedUnit.ResolveRuntimeModifier(
                    "StatusMaxStacksBonus",
                    Definition.status_effect_id);
            return maximum > 0 ? Math.Min(updated, maximum) : updated;
        }

        private float? ResolveDuration(float? durationSeconds)
        {
            if (IsPermanent)
            {
                return null;
            }

            float duration = durationSeconds
                ?? Definition.default_duration_seconds
                ?? throw new ArgumentException(
                    "Status Definition has no default_duration_seconds.",
                    nameof(Definition));
            if (duration < 0f || float.IsNaN(duration) || float.IsInfinity(duration))
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            return duration;
        }
    }
}
