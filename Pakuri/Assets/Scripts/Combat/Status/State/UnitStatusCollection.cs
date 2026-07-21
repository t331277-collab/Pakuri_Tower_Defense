using System;
using System.Collections.Generic;
using Pakuri.Combat;

/*
 * 한 유닛에게 적용된 모든 상태 효과의 추가, 중첩, 제거, 보호막 흡수를 관리한다.
 */
namespace Pakuri.InGame
{
    public sealed class UnitStatusCollection
    {
        private readonly List<StatusRuntimeInstance> statuses = new List<StatusRuntimeInstance>();

        public IReadOnlyList<StatusRuntimeInstance> ActiveStatuses => statuses;
        public int Count => statuses.Count;

        public StatusRuntimeInstance Apply(
            string tag,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            float shieldAmount = 0f)
        {
            return StatusEffectLookup.TryParse(tag, out var kind)
                ? Apply(kind, stacks, durationSeconds, maxStacks, permanent, refreshDuration, shieldAmount)
                : null;
        }

        public StatusRuntimeInstance Apply(
            StatusEffectKind kind,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            float shieldAmount = 0f)
        {
            var statusData = StatusRuntimeDataFactory.Create(kind, null);
            return Apply(statusData, stacks, durationSeconds, maxStacks, permanent, refreshDuration, shieldAmount);
        }

        public StatusRuntimeInstance Apply(
            StatusRuntimeData statusData,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            float shieldAmount = 0f)
        {
            if (statusData == null || statusData.Kind == StatusEffectKind.None)
            {
                return null;
            }

            var resolvedDuration = durationSeconds > 0f ? durationSeconds : statusData.Duration;
            var resolvedMaxStacks = maxStacks > 0 ? maxStacks : statusData.MaxStacks;
            var resolvedPermanent = permanent || statusData.Permanent;
            var kind = statusData.Kind;
            var sourceAware = HasSourceAwareIdentity(statusData);
            var mergedExisting = sourceAware && statusData.MergePolicy != StatusMergePolicy.AlwaysStack;
            var status = mergedExisting
                ? Find(kind, statusData.SourceSkillId)
                : Find(kind);
            if (status == null || (sourceAware && statusData.MergePolicy == StatusMergePolicy.AlwaysStack))
            {
                status = new StatusRuntimeInstance(kind);
                statuses.Add(status);
                mergedExisting = false;
            }

            if (ShouldReplaceSourceData(status.SourceData, statusData))
            {
                status.SetSourceData(statusData);
            }

            status.SetSourceMetadata(statusData);
            if (mergedExisting)
            {
                status.RefreshStacks(stacks, resolvedMaxStacks);
            }
            else
            {
                status.AddStacks(stacks, resolvedMaxStacks);
            }

            status.SetPermanent(resolvedPermanent);
            if (resolvedPermanent || refreshDuration || status.DurationRemaining <= 0f)
            {
                status.SetDuration(resolvedDuration);
            }

            if (kind == StatusEffectKind.Shield)
            {
                status.ApplyShield(shieldAmount, statusData.ShieldAmountRefreshPolicy, mergedExisting);
            }

            return status;
        }

        public bool Tick(float deltaTime)
        {
            return Tick(deltaTime, null);
        }

        public bool Tick(float deltaTime, ICollection<StatusRuntimeInstance> removedStatuses)
        {
            if (deltaTime <= 0f)
            {
                return false;
            }

            var changed = false;
            for (var i = statuses.Count - 1; i >= 0; i--)
            {
                var status = statuses[i];
                if (status == null || status.Tick(deltaTime))
                {
                    if (status != null && removedStatuses != null)
                    {
                        removedStatuses.Add(status);
                    }

                    statuses.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        public bool Has(string tag)
        {
            return StatusEffectLookup.TryParse(tag, out var kind) && Has(kind);
        }

        public bool Has(StatusEffectKind kind)
        {
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status != null && status.Kind == kind && status.Stacks > 0)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 같은 상태 종류 중 지정 스킬이 만든 상태만 확인한다.
         */
        public bool Has(StatusEffectKind kind, string sourceSkillId)
        {
            var status = Find(kind, sourceSkillId);
            return status != null && status.Stacks > 0;
        }

        public int GetStacks(string tag)
        {
            return StatusEffectLookup.TryParse(tag, out var kind) ? GetStacks(kind) : 0;
        }

        public int GetStacks(StatusEffectKind kind)
        {
            var total = 0;
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status != null && status.Kind == kind)
                {
                    total += status.Stacks;
                }
            }

            return total;
        }

        public bool Remove(string tag)
        {
            return StatusEffectLookup.TryParse(tag, out var kind) && Remove(kind);
        }

        public int ConsumeStacks(string tag, int stacks)
        {
            return StatusEffectLookup.TryParse(tag, out var kind) ? ConsumeStacks(kind, stacks) : 0;
        }

        public int ConsumeStacks(StatusEffectKind kind, int stacks)
        {
            var remaining = System.Math.Max(0, stacks);
            if (remaining <= 0)
            {
                return 0;
            }

            var consumed = 0;
            for (var i = statuses.Count - 1; i >= 0 && remaining > 0; i--)
            {
                var status = statuses[i];
                if (status == null || status.Kind != kind || status.Stacks <= 0)
                {
                    continue;
                }

                var consumedFromStatus = status.ConsumeStacks(remaining);
                if (consumedFromStatus <= 0)
                {
                    continue;
                }

                consumed += consumedFromStatus;
                remaining -= consumedFromStatus;
                if (status.Stacks <= 0)
                {
                    statuses.RemoveAt(i);
                }
            }

            return consumed;
        }

        public bool Remove(StatusEffectKind kind)
        {
            return Remove(kind, null, null);
        }

        /*
         * 상태 종류와 출처 스킬이 모두 일치하는 상태만 제거한다.
         */
        public bool Remove(
            StatusEffectKind kind,
            string sourceSkillId,
            ICollection<StatusRuntimeInstance> removedStatuses)
        {
            var removed = false;
            var hasSourceSkillId = !string.IsNullOrWhiteSpace(sourceSkillId);
            for (var i = statuses.Count - 1; i >= 0; i--)
            {
                var status = statuses[i];
                if (status == null
                    || status.Kind != kind
                    || (hasSourceSkillId && !string.Equals(status.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                removedStatuses?.Add(status);
                statuses.RemoveAt(i);
                removed = true;
            }

            return removed;
        }

        public void Clear()
        {
            statuses.Clear();
        }

        public float GetTotalShieldAmount()
        {
            var total = 0f;
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status != null && status.IsShieldStatus)
                {
                    total += status.RemainingShieldAmount;
                }
            }

            return total;
        }

        public float ConsumeShield(float amount)
        {
            return ConsumeShield(amount, null);
        }

        public float ConsumeShield(float amount, ICollection<StatusRuntimeInstance> depletedStatuses)
        {
            return ConsumeShield(amount, depletedStatuses, null);
        }

        public float ConsumeShield(
            float amount,
            ICollection<StatusRuntimeInstance> depletedStatuses,
            ICollection<ShieldAbsorptionRecord> absorbRecords)
        {
            var remaining = Math.Max(0f, amount);
            if (remaining <= 0f)
            {
                return 0f;
            }

            for (var i = 0; i < statuses.Count && remaining > 0f; i++)
            {
                var status = statuses[i];
                if (status == null || !status.IsShieldStatus || status.RemainingShieldAmount <= 0f)
                {
                    continue;
                }

                var absorbed = status.ConsumeShield(remaining);
                remaining -= absorbed;
                if (absorbRecords != null && absorbed > 0f)
                {
                    absorbRecords.Add(new ShieldAbsorptionRecord(status, absorbed));
                }

                if (status.RemainingShieldAmount <= 0f)
                {
                    if (depletedStatuses != null)
                    {
                        depletedStatuses.Add(status);
                    }

                    statuses.RemoveAt(i);
                    i--;
                }
            }

            return Math.Max(0f, amount - remaining);
        }

        public bool ExtendDurations(StatusEffectKind kind, float durationDelta, Func<StatusRuntimeInstance, bool> predicate = null)
        {
            if (durationDelta <= 0f)
            {
                return false;
            }

            var changed = false;
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status == null || status.Kind != kind)
                {
                    continue;
                }

                if (predicate != null && !predicate(status))
                {
                    continue;
                }

                changed |= status.ExtendDuration(durationDelta);
            }

            return changed;
        }

        public void RecordIncomingDamage(DamageAttribute attribute, float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status != null)
                {
                    status.RecordIncomingDamage(attribute, amount);
                }
            }
        }

        private StatusRuntimeInstance Find(StatusEffectKind kind, string sourceSkillId = null)
        {
            var hasSourceSkillId = !string.IsNullOrWhiteSpace(sourceSkillId);
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status == null || status.Kind != kind)
                {
                    continue;
                }

                if (!hasSourceSkillId || string.Equals(status.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase))
                {
                    return status;
                }
            }

            return null;
        }

        private static bool HasSourceAwareIdentity(StatusRuntimeData statusData)
        {
            return statusData != null
                && !string.IsNullOrWhiteSpace(statusData.SourceSkillId)
                && statusData.MergePolicy != StatusMergePolicy.Unspecified;
        }

        private static bool ShouldReplaceSourceData(StatusRuntimeData current, StatusRuntimeData incoming)
        {
            if (incoming == null)
            {
                return false;
            }

            if (current == null)
            {
                return true;
            }

            return StatusCombatRules.ComputeModifierMagnitude(incoming) >= StatusCombatRules.ComputeModifierMagnitude(current);
        }
    }
}
