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

    /// UnitStatusCollection가 소유하는 데이터와 동작을 캡슐화한다.
    public class UnitStatusCollection
    {
        private readonly List<StatusRuntimeInstance> statuses = new List<StatusRuntimeInstance>();

        public IReadOnlyList<StatusRuntimeInstance> ActiveStatuses => statuses;
        public int Count => statuses.Count;

        /// 전달된 런타임 입력값을 사용해 요청값를 적용한다.
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

        /// 전달된 deltaTime 값을 사용해 요청값를 경과 시간 기준으로 갱신한다.
        public bool Tick(float deltaTime)
        {
            return Tick(deltaTime, null);
        }

        /// 전달된 런타임 입력값을 사용해 요청값를 경과 시간 기준으로 갱신한다.
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

        /// 전달된 kind 값을 사용해 소유한 런타임 상태에 요청값가 있는지 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태에 요청값가 있는지 반환한다.
        public bool Has(StatusEffectKind kind, string sourceSkillId)
        {
            var status = Find(kind, sourceSkillId);
            return status != null && status.Stacks > 0;
        }

        /// 전달된 kind 값을 사용해 Stacks를 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 Stacks를 현재 런타임 상태에서 소비한다.
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

        /// 전달된 kind 값을 사용해 요청값를 소유한 런타임 상태에서 제거한다.
        public bool Remove(StatusEffectKind kind)
        {
            return Remove(kind, null, null);
        }

        /// 전달된 런타임 입력값을 사용해 요청값를 소유한 런타임 상태에서 제거한다.
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

        /// 소유한 모든 런타임 값를 소유한 런타임 상태에서 비운다.
        public void Clear()
        {
            statuses.Clear();
        }

        /// TotalShieldAmount를 반환한다.
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

        /// 전달된 amount 값을 사용해 Shield를 현재 런타임 상태에서 소비한다.
        public float ConsumeShield(float amount)
        {
            return ConsumeShield(amount, null);
        }

        /// 전달된 런타임 입력값을 사용해 Shield를 현재 런타임 상태에서 소비한다.
        public float ConsumeShield(float amount, ICollection<StatusRuntimeInstance> depletedStatuses)
        {
            return ConsumeShield(amount, depletedStatuses, null);
        }

        /// 전달된 런타임 입력값을 사용해 Shield를 현재 런타임 상태에서 소비한다.
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

        /// 전달된 런타임 입력값을 사용해 Durations를 연장한다.
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

        /// 전달된 런타임 입력값을 사용해 RecordIncomingDamage 작업을 수행한다.
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

        /// 전달된 런타임 입력값을 사용해 요청값를 찾는다.
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

        /// 전달된 statusData 값을 사용해 소유한 런타임 상태에 SourceAwareIdentity가 있는지 반환한다.
        private static bool HasSourceAwareIdentity(StatusRuntimeData statusData)
        {
            return statusData != null
                && !string.IsNullOrWhiteSpace(statusData.SourceSkillId)
                && statusData.MergePolicy != StatusMergePolicy.Unspecified;
        }

        /// 전달된 런타임 입력값을 사용해 ReplaceSourceData 실행 필요 여부를 반환한다.
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

    /// StatusRuntimeInstance가 소유하는 데이터와 동작을 캡슐화한다.
    public class StatusRuntimeInstance
    {

        /// StatusRuntimeInstance 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

        /// 전달된 sourceData 값을 사용해 SourceData를 갱신한다.
        public void SetSourceData(StatusRuntimeData sourceData)
        {
            SourceData = sourceData;
        }

        /// 전달된 sourceData 값을 사용해 SourceMetadata를 갱신한다.
        public void SetSourceMetadata(StatusRuntimeData sourceData)
        {
            SourceSkillId = sourceData.SourceSkillId;
            TargetScope = sourceData.TargetScope;
            MergePolicy = sourceData.MergePolicy;
            ShieldAmountRefreshPolicy = sourceData.ShieldAmountRefreshPolicy;
        }

        /// 전달된 source 값을 사용해 SourceUnit를 갱신한다.
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

        /// 전달된 런타임 입력값을 사용해 Stacks를 소유한 런타임 상태에 추가한다.
        public void AddStacks(int stacks, int maxStacks)
        {
            var nextStacks = Stacks + Math.Max(0, stacks);
            Stacks = nextStacks;
            if (maxStacks > 0)
            {
                Stacks = Math.Min(maxStacks, nextStacks);
            }
        }

        /// 전달된 런타임 입력값을 사용해 Stacks를 현재 런타임 모델을 기준으로 갱신한다.
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

        /// 전달된 stacks 값을 사용해 Stacks를 현재 런타임 상태에서 소비한다.
        public int ConsumeStacks(int stacks)
        {
            var consumed = Math.Min(Math.Max(0, stacks), Math.Max(0, Stacks));
            Stacks = Math.Max(0, Stacks - consumed);
            return consumed;
        }

        /// 전달된 durationSeconds 값을 사용해 Duration를 갱신한다.
        public void SetDuration(float durationSeconds)
        {
            DurationRemaining = Math.Max(0f, durationSeconds);
        }

        /// 전달된 durationDelta 값을 사용해 Duration를 연장한다.
        public bool ExtendDuration(float durationDelta)
        {
            if (Permanent || durationDelta <= 0f)
            {
                return false;
            }

            DurationRemaining = Math.Max(0f, DurationRemaining + durationDelta);
            return true;
        }

        /// 전달된 permanent 값을 사용해 Permanent를 갱신한다.
        public void SetPermanent(bool permanent)
        {
            Permanent = permanent;
            if (Permanent)
            {
                DurationRemaining = 0f;
            }
        }

        /// 전달된 런타임 입력값을 사용해 Shield를 적용한다.
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

        /// 전달된 amount 값을 사용해 Shield를 현재 런타임 상태에서 소비한다.
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

        /// 전달된 런타임 입력값을 사용해 RecordIncomingDamage 작업을 수행한다.
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

        /// 전달된 attribute 값을 사용해 TrackedIncomingDamage를 반환한다.
        public float GetTrackedIncomingDamage(DamageAttribute attribute)
        {
            var index = (int)attribute;
            if (index < 0 || index >= trackedIncomingDamageTotals.Length)
            {
                return 0f;
            }

            return trackedIncomingDamageTotals[index];
        }

        /// 전달된 deltaTime 값을 사용해 요청값를 경과 시간 기준으로 갱신한다.
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

        /// ShieldAbsorptionRecord 인스턴스를 전달된 런타임 입력값으로 초기화한다.
        public ShieldAbsorptionRecord(StatusRuntimeInstance status, float absorbedAmount)
        {
            Status = status;
            AbsorbedAmount = absorbedAmount;
        }

        public StatusRuntimeInstance Status { get; }
        public float AbsorbedAmount { get; }
    }
}
