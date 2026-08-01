/*
 * 역할: 변경 가능한 상태 효과 런타임 상태.
 * 책임: 활성 상태 인스턴스를 보관하고 색인·변경·갱신·집계 조회를 제공한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.InGame
{

    public class UnitStatusCollection
    {
        private readonly List<StatusRuntimeInstance> statuses = new List<StatusRuntimeInstance>();

        public IReadOnlyList<StatusRuntimeInstance> ActiveStatuses => statuses;
        public int Count => statuses.Count;

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

            var resolvedDuration = statusData.Duration;
            if (durationSeconds > 0f)
            {
                resolvedDuration = durationSeconds;
            }

            var resolvedMaxStacks = statusData.MaxStacks;
            if (maxStacks > 0)
            {
                resolvedMaxStacks = maxStacks;
            }

            var resolvedPermanent = permanent || statusData.Permanent;
            var kind = statusData.Kind;
            var sourceAware = HasSourceAwareIdentity(statusData);
            var mergedExisting = sourceAware && statusData.MergePolicy != StatusMergePolicy.AlwaysStack;
            StatusRuntimeInstance status;
            if (mergedExisting)
            {
                status = Find(kind, statusData.SourceSkillId);
            }
            else
            {
                status = Find(kind);
            }
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

        public bool Has(StatusEffectKind kind, string sourceSkillId)
        {
            var status = Find(kind, sourceSkillId);
            return status != null && status.Stacks > 0;
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

        public int ConsumeStacks(
            StatusEffectKind kind,
            int stacks,
            ICollection<StatusRuntimeInstance> removedStatuses)
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
                    removedStatuses.Add(status);
                    statuses.RemoveAt(i);
                }
            }

            return consumed;
        }

        public bool Remove(StatusEffectKind kind)
        {
            return Remove(kind, null, null);
        }

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

                if (removedStatuses != null)
                {
                    removedStatuses.Add(status);
                }
                statuses.RemoveAt(i);
                removed = true;
            }

            return removed;
        }

        public void Clear()
        {
            statuses.Clear();
        }

        /// 현재 유닛이 보유한 보호막의 합계를 계산한다.
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

    public class StatusRuntimeInstance
    {

        public StatusRuntimeInstance(StatusEffectKind kind)
        {
            Kind = kind;
        }

        public StatusEffectKind Kind { get; }
        public string Tag
        {
            get
            {
                if (SourceData != null && !string.IsNullOrWhiteSpace(SourceData.StatusTag))
                {
                    return SourceData.StatusTag;
                }

                return string.Empty;
            }
        }

        public string DisplayName
        {
            get
            {
                if (SourceData != null && !string.IsNullOrWhiteSpace(SourceData.StatusName))
                {
                    return SourceData.StatusName;
                }

                return GameDataLoader.CurrentCatalog?.GetStatusRuntimeData(Kind)?.StatusName
                    ?? throw new KeyNotFoundException($"Status definition '{Kind}' is not registered.");
            }
        }

        public StatusRuntimeData SourceData { get; private set; }
        public string SourceSkillId { get; private set; }
        public string SourceUnitId { get; private set; }
        public string SourceDefinitionId { get; private set; }
        public StatusTargetScope TargetScope { get; private set; } = StatusTargetScope.Unspecified;
        public StatusMergePolicy MergePolicy { get; private set; } = StatusMergePolicy.Unspecified;
        public ShieldRefreshRule ShieldAmountRefreshPolicy { get; private set; } = ShieldRefreshRule.TakeHighest;
        public int Stacks { get; private set; }
        public float DurationRemaining { get; private set; }
        public bool Permanent { get; private set; }
        public float AppliedShieldAmount { get; private set; }
        public float RemainingShieldAmount { get; private set; }
        private readonly float[] trackedIncomingDamageTotals = new float[Enum.GetValues(typeof(DamageAttribute)).Length];

        public bool IsTimed => !Permanent && DurationRemaining > 0f;
        public bool IsShieldStatus => Kind == StatusEffectKind.Shield;

        public void SetSourceData(StatusRuntimeData sourceData)
        {
            SourceData = sourceData;
        }

        public void SetSourceMetadata(StatusRuntimeData sourceData)
        {
            SourceSkillId = sourceData.SourceSkillId;
            TargetScope = sourceData.TargetScope;
            MergePolicy = sourceData.MergePolicy;
            ShieldAmountRefreshPolicy = sourceData.ShieldAmountRefreshPolicy;
        }

        public void SetSourceUnit(UnitCombatState source)
        {
            SourceUnitId = string.Empty;
            SourceDefinitionId = string.Empty;
            if (source == null || source.Identity == null)
            {
                return;
            }

            SourceUnitId = source.Identity.UnitId;
            SourceDefinitionId = source.Identity.DefinitionId;
        }

        public void AddStacks(int stacks, int maxStacks)
        {
            var nextStacks = Stacks + Math.Max(0, stacks);
            Stacks = nextStacks;
            if (maxStacks > 0)
            {
                Stacks = Math.Min(maxStacks, nextStacks);
            }
        }

        public void RefreshStacks(int stacks, int maxStacks)
        {
            var incomingStacks = Math.Max(0, stacks);
            if (incomingStacks <= 0)
            {
                return;
            }

            var nextStacks = Math.Max(Stacks, incomingStacks);
            Stacks = nextStacks;
            if (maxStacks > 0)
            {
                Stacks = Math.Min(maxStacks, nextStacks);
            }
        }

        public int ConsumeStacks(int stacks)
        {
            var consumed = Math.Min(Math.Max(0, stacks), Math.Max(0, Stacks));
            Stacks = Math.Max(0, Stacks - consumed);
            return consumed;
        }

        public void SetDuration(float durationSeconds)
        {
            DurationRemaining = Math.Max(0f, durationSeconds);
        }

        public bool ExtendDuration(float durationDelta)
        {
            if (Permanent || durationDelta <= 0f)
            {
                return false;
            }

            DurationRemaining = Math.Max(0f, DurationRemaining + durationDelta);
            return true;
        }

        public void SetPermanent(bool permanent)
        {
            Permanent = permanent;
            if (Permanent)
            {
                DurationRemaining = 0f;
            }
        }

        public void ApplyShield(float amount, ShieldRefreshRule refreshRule, bool mergedExisting)
        {
            var resolvedAmount = Math.Max(0f, amount);
            ShieldAmountRefreshPolicy = refreshRule;
            if (!mergedExisting)
            {
                AppliedShieldAmount = resolvedAmount;
                RemainingShieldAmount = resolvedAmount;
                return;
            }

            switch (refreshRule)
            {
                case ShieldRefreshRule.Replace:
                    AppliedShieldAmount = resolvedAmount;
                    RemainingShieldAmount = resolvedAmount;
                    break;
                case ShieldRefreshRule.Stack:
                    AppliedShieldAmount += resolvedAmount;
                    RemainingShieldAmount += resolvedAmount;
                    break;
                case ShieldRefreshRule.TakeHighest:
                    var chosenAmount = Math.Max(RemainingShieldAmount, resolvedAmount);
                    AppliedShieldAmount = chosenAmount;
                    RemainingShieldAmount = chosenAmount;
                    break;
            }
        }

        public float ConsumeShield(float amount)
        {
            if (!IsShieldStatus || RemainingShieldAmount <= 0f || amount <= 0f)
            {
                return 0f;
            }

            var consumed = Math.Min(RemainingShieldAmount, amount);
            RemainingShieldAmount = Math.Max(0f, RemainingShieldAmount - consumed);
            return consumed;
        }

        public void RecordIncomingDamage(DamageAttribute attribute, float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            var index = (int)attribute;
            if (index < 0 || index >= trackedIncomingDamageTotals.Length)
            {
                return;
            }

            trackedIncomingDamageTotals[index] += amount;
        }

        public float GetTrackedIncomingDamage(DamageAttribute attribute)
        {
            var index = (int)attribute;
            if (index < 0 || index >= trackedIncomingDamageTotals.Length)
            {
                return 0f;
            }

            return trackedIncomingDamageTotals[index];
        }

        public bool Tick(float deltaTime)
        {
            if (IsShieldStatus && RemainingShieldAmount <= 0f)
            {
                return true;
            }

            if (Permanent)
            {
                return false;
            }

            if (DurationRemaining <= 0f)
            {
                return IsShieldStatus;
            }

            DurationRemaining = Math.Max(0f, DurationRemaining - deltaTime);
            return DurationRemaining <= 0f;
        }
    }

    /// ShieldAbsorptionRecord 처리에 함께 전달되는 값들을 묶는다.
    public struct ShieldAbsorptionRecord
    {

        public ShieldAbsorptionRecord(StatusRuntimeInstance status, float absorbedAmount)
        {
            Status = status;
            AbsorbedAmount = absorbedAmount;
        }

        public StatusRuntimeInstance Status { get; }
        public float AbsorbedAmount { get; }
    }
}
