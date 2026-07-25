using System;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Units.Models;

/* 상태 효과와 시간 제한 전투 수정치의 생명주기 상태를 소유한다. */
namespace Pakuri.NewCore.Combat.Status
{
    public sealed class StatusEffect
    {
        /* 상태 정의와 적용·피적용 유닛을 결합해 초기 스택과 지속시간을 계산한다. */
        internal StatusEffect(
            StatusDefinition definition,
            UnitBaseModel applyingUnit,
            UnitBaseModel affectedUnit,
            float? durationSeconds,
            int? stackAmount,
            string sourceSkillId,
            int? maximumStacks)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            ApplyingUnit =
                applyingUnit ?? throw new ArgumentNullException(nameof(applyingUnit));
            AffectedUnit =
                affectedUnit ?? throw new ArgumentNullException(nameof(affectedUnit));
            SourceSkillId = sourceSkillId;
            MaximumStacks = NormalizeMaximum(maximumStacks);
            CurrentStacks = ResolveStackAmount(stackAmount);
            RemainingDuration = ResolveDuration(durationSeconds);
        }

        public StatusDefinition Definition { get; }

        public UnitBaseModel ApplyingUnit { get; }

        public UnitBaseModel AffectedUnit { get; }

        public float? RemainingDuration { get; private set; }

        public int CurrentStacks { get; private set; }

        public string SourceSkillId { get; }

        public int? MaximumStacks { get; private set; }

        public float TrackedIncomingDamage { get; private set; }

        public string LastTrackedAttribute { get; private set; }

        public bool IsPermanent => Definition.is_permanent == true;

        public bool IsExpired =>
            !IsPermanent && RemainingDuration.HasValue && RemainingDuration.Value <= 0f;

        /* 재적용된 상태의 최대 스택, 현재 스택, 지속시간을 갱신한다. */
        internal void Refresh(
            float? durationSeconds,
            int? stackAmount,
            int? maximumStacks)
        {
            int? refreshedMaximum = maximumStacks.HasValue
                ? NormalizeMaximum(maximumStacks)
                : MaximumStacks;
            int refreshedStacks =
                AddStacks(
                    CurrentStacks,
                    ResolveStackAmount(stackAmount, refreshedMaximum),
                    refreshedMaximum);
            float? refreshedDuration = ResolveDuration(durationSeconds);

            MaximumStacks = refreshedMaximum;
            CurrentStacks = refreshedStacks;
            RemainingDuration = refreshedDuration;
        }

        /* 영구 상태가 아닌 경우 경과 시간만큼 남은 지속시간을 줄인다. */
        internal void Tick(float deltaTime)
        {
            if (IsPermanent || !RemainingDuration.HasValue)
            {
                return;
            }

            RemainingDuration = Math.Max(0f, RemainingDuration.Value - deltaTime);
        }

        /* public 연장 입력을 검증하고 만료 가능한 상태의 지속시간을 늘린다. */
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

        /* 지정 수만큼 현재 상태 스택을 제거하고 실제 제거량을 반환한다. */
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

        /* 양수 피해량과 마지막 피해 속성을 만료 트리거용으로 누적한다. */
        internal void TrackIncomingDamage(string attribute, float amount)
        {
            if (amount <= 0f) return;
            TrackedIncomingDamage += amount;
            LastTrackedAttribute = attribute;
        }

        /* 요청 스택 또는 정의 기본값을 선택하고 최대 스택 규칙을 적용한다. */
        private int ResolveStackAmount(
            int? stackAmount,
            int? maximumStacks = null)
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

            return AddStacks(0, resolved, maximumStacks ?? MaximumStacks);
        }

        /* 현재 스택에 증가량과 런타임 최대 스택 보너스를 적용한다. */
        private int AddStacks(int current, int amount, int? maximumStacks)
        {
            int updated = checked(current + amount);
            int maximum = (maximumStacks ?? Definition.max_stacks ?? 0)
                + (int)AffectedUnit.ResolveRuntimeModifier(
                    "StatusMaxStacksBonus",
                    Definition.status_effect_id);
            return maximum > 0 ? Math.Min(updated, maximum) : updated;
        }

        /* 최대 스택 0을 무제한으로 정규화하고 음수 입력을 거부한다. */
        private static int? NormalizeMaximum(int? maximumStacks)
        {
            if (!maximumStacks.HasValue || maximumStacks.Value == 0)
            {
                return null;
            }
            if (maximumStacks.Value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumStacks));
            }
            return maximumStacks;
        }

        /* 영구 상태 또는 요청·정의 지속시간에서 실제 남은 시간을 계산한다. */
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

    public sealed class RuntimeCombatModifier
    {
        /* public 소유 경계에서 검증된 수정치 값을 시간 제한 런타임 상태로 저장한다. */
        internal RuntimeCombatModifier(
            string kind,
            float value,
            string filter,
            string secondaryFilter,
            UnitBaseModel source,
            float durationSeconds)
        {
            Kind = kind;
            Value = value;
            Filter = filter;
            SecondaryFilter = secondaryFilter;
            Source = source;
            RemainingDuration = durationSeconds;
        }

        public string Kind { get; }

        public float Value { get; }

        public string Filter { get; }

        public string SecondaryFilter { get; }

        public UnitBaseModel Source { get; }

        public float RemainingDuration { get; private set; }

        public bool IsExpired => RemainingDuration <= 0f;

        /* 경과 시간만큼 남은 수정치 지속시간을 0까지 줄인다. */
        internal void Tick(float deltaTime)
        {
            RemainingDuration = Math.Max(0f, RemainingDuration - deltaTime);
        }
    }
}
