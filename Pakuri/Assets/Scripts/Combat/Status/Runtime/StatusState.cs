using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 유닛의 상태 목록, 개별 상태 실행값, 보호막 흡수 결과를 한곳에서 관리한다.
 */
namespace Pakuri.InGame
{
    public class UnitStatusCollection
    {
        private readonly List<StatusRuntimeInstance> statuses = new List<StatusRuntimeInstance>();

        public IReadOnlyList<StatusRuntimeInstance> ActiveStatuses => statuses;
        public int Count => statuses.Count;

        /*
         * Apply 처리를 대상에 적용한다.
         */
        public StatusRuntimeInstance Apply(
            StatusRuntimeData statusData /* 상태 효과 실행 데이터 */,
            int stacks /* 중첩 수 */,
            float durationSeconds /* 지속 시간(초) */,
            int maxStacks = 0 /* 최대 중첩 수 */,
            bool permanent = false /* 영구 여부 */,
            bool refreshDuration = true /* 갱신 지속 시간 여부 */,
            float shieldAmount = 0f /* 보호막 수치 */)
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

        /*
         * Tick 작업 결과를 반환한다.
         */
        public bool Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */)
        {
            return Tick(deltaTime, null);
        }

        /*
         * Tick 작업 결과를 반환한다.
         */
        public bool Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */, ICollection<StatusRuntimeInstance> removedStatuses /* 제거된 상태 효과 목록 */)
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

        /*
         * Has 조건을 만족하는지 확인한다.
         */
        public bool Has(StatusEffectKind kind /* 처리할 종류 */)
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
        public bool Has(StatusEffectKind kind /* 처리할 종류 */, string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */)
        {
            var status = Find(kind, sourceSkillId);
            return status != null && status.Stacks > 0;
        }

        /*
         * GetStacks에 해당하는 값을 찾아 반환한다.
         */
        public int GetStacks(StatusEffectKind kind /* 처리할 종류 */)
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

        /*
         * ConsumeStacks 작업 결과를 반환한다.
         */
        public int ConsumeStacks(
            StatusEffectKind kind /* 처리할 종류 */,
            int stacks /* 중첩 수 */,
            ICollection<StatusRuntimeInstance> removedStatuses /* 제거된 상태 효과 목록 */)
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

        /*
         * Remove 작업 결과를 반환한다.
         */
        public bool Remove(StatusEffectKind kind /* 처리할 종류 */)
        {
            return Remove(kind, null, null);
        }

        /*
         * 상태 종류와 출처 스킬이 모두 일치하는 상태만 제거한다.
         */
        public bool Remove(
            StatusEffectKind kind /* 처리할 종류 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */,
            ICollection<StatusRuntimeInstance> removedStatuses /* 제거된 상태 효과 목록 */)
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

        /*
         * Clear 작업을 수행한다.
         */
        public void Clear()
        {
            statuses.Clear();
        }

        /*
         * GetTotalShieldAmount에 해당하는 값을 찾아 반환한다.
         */
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

        /*
         * ConsumeShield 작업 결과를 반환한다.
         */
        public float ConsumeShield(float amount /* 적용할 수치 */)
        {
            return ConsumeShield(amount, null);
        }

        /*
         * ConsumeShield 작업 결과를 반환한다.
         */
        public float ConsumeShield(float amount /* 적용할 수치 */, ICollection<StatusRuntimeInstance> depletedStatuses /* 소진된 상태 효과 목록 */)
        {
            return ConsumeShield(amount, depletedStatuses, null);
        }

        /*
         * ConsumeShield 작업 결과를 반환한다.
         */
        public float ConsumeShield(
            float amount /* 적용할 수치 */,
            ICollection<StatusRuntimeInstance> depletedStatuses /* 소진된 상태 효과 목록 */,
            ICollection<ShieldAbsorptionRecord> absorbRecords /* 흡수 기록 목록 */)
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

        /*
         * ExtendDurations 작업 결과를 반환한다.
         */
        public bool ExtendDurations(StatusEffectKind kind /* 처리할 종류 */, float durationDelta /* 지속 시간 경과 */, Func<StatusRuntimeInstance, bool> predicate = null /* 판정 조건 */)
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

        /*
         * RecordIncomingDamage 작업을 수행한다.
         */
        public void RecordIncomingDamage(DamageAttribute attribute /* 피해 속성 */, float amount /* 적용할 수치 */)
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

        /*
         * Find에 해당하는 값을 찾아 반환한다.
         */
        private StatusRuntimeInstance Find(StatusEffectKind kind /* 처리할 종류 */, string sourceSkillId = null /* 효과를 발생시킨 스킬 식별자 */)
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

        /*
         * HasSourceAwareIdentity 조건을 만족하는지 확인한다.
         */
        private static bool HasSourceAwareIdentity(StatusRuntimeData statusData /* 상태 효과 실행 데이터 */)
        {
            return statusData != null
                && !string.IsNullOrWhiteSpace(statusData.SourceSkillId)
                && statusData.MergePolicy != StatusMergePolicy.Unspecified;
        }

        /*
         * ShouldReplaceSourceData 조건을 만족하는지 확인한다.
         */
        private static bool ShouldReplaceSourceData(StatusRuntimeData current /* 현재 */, StatusRuntimeData incoming /* 받는 */)
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

    /*
     * 적용된 상태 효과 하나의 출처, 중첩, 지속시간, 보호막과 피해 기록을 보관한다.
     */
    public class StatusRuntimeInstance
    {
        /*
         * StatusRuntimeInstance에 필요한 값을 초기화한다.
         */
        public StatusRuntimeInstance(StatusEffectKind kind /* 처리할 종류 */)
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

        /*
         * SetSourceData에 필요한 값을 설정한다.
         */
        public void SetSourceData(StatusRuntimeData sourceData /* 발생 원본 데이터 */)
        {
            SourceData = sourceData;
        }

        /*
         * SetSourceMetadata에 필요한 값을 설정한다.
         */
        public void SetSourceMetadata(StatusRuntimeData sourceData /* 발생 원본 데이터 */)
        {
            SourceSkillId = sourceData.SourceSkillId;
            TargetScope = sourceData.TargetScope;
            MergePolicy = sourceData.MergePolicy;
            ShieldAmountRefreshPolicy = sourceData.ShieldAmountRefreshPolicy;
        }

        /*
         * SetSourceUnit에 필요한 값을 설정한다.
         */
        public void SetSourceUnit(UnitCombatState source /* 효과를 발생시킨 유닛 */)
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

        /*
         * AddStacks 작업을 수행한다.
         */
        public void AddStacks(int stacks /* 중첩 수 */, int maxStacks /* 최대 중첩 수 */)
        {
            var nextStacks = Stacks + Math.Max(0, stacks);
            Stacks = nextStacks;
            if (maxStacks > 0)
            {
                Stacks = Math.Min(maxStacks, nextStacks);
            }
        }

        /*
         * RefreshStacks 대상의 현재 상태를 갱신한다.
         */
        public void RefreshStacks(int stacks /* 중첩 수 */, int maxStacks /* 최대 중첩 수 */)
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

        /*
         * ConsumeStacks 작업 결과를 반환한다.
         */
        public int ConsumeStacks(int stacks /* 중첩 수 */)
        {
            var consumed = Math.Min(Math.Max(0, stacks), Math.Max(0, Stacks));
            Stacks = Math.Max(0, Stacks - consumed);
            return consumed;
        }

        /*
         * SetDuration에 필요한 값을 설정한다.
         */
        public void SetDuration(float durationSeconds /* 지속 시간(초) */)
        {
            DurationRemaining = Math.Max(0f, durationSeconds);
        }

        /*
         * ExtendDuration 작업 결과를 반환한다.
         */
        public bool ExtendDuration(float durationDelta /* 지속 시간 경과 */)
        {
            if (Permanent || durationDelta <= 0f)
            {
                return false;
            }

            DurationRemaining = Math.Max(0f, DurationRemaining + durationDelta);
            return true;
        }

        /*
         * SetPermanent에 필요한 값을 설정한다.
         */
        public void SetPermanent(bool permanent /* 영구 여부 */)
        {
            Permanent = permanent;
            if (Permanent)
            {
                DurationRemaining = 0f;
            }
        }

        /*
         * ApplyShield 처리를 대상에 적용한다.
         */
        public void ApplyShield(float amount /* 적용할 수치 */, ShieldRefreshRule refreshRule /* 갱신 규칙 */, bool mergedExisting /* 기존 값과 병합했는지 여부 */)
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

        /*
         * ConsumeShield 작업 결과를 반환한다.
         */
        public float ConsumeShield(float amount /* 적용할 수치 */)
        {
            if (!IsShieldStatus || RemainingShieldAmount <= 0f || amount <= 0f)
            {
                return 0f;
            }

            var consumed = Math.Min(RemainingShieldAmount, amount);
            RemainingShieldAmount = Math.Max(0f, RemainingShieldAmount - consumed);
            return consumed;
        }

        /*
         * RecordIncomingDamage 작업을 수행한다.
         */
        public void RecordIncomingDamage(DamageAttribute attribute /* 피해 속성 */, float amount /* 적용할 수치 */)
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

        /*
         * GetTrackedIncomingDamage에 해당하는 값을 찾아 반환한다.
         */
        public float GetTrackedIncomingDamage(DamageAttribute attribute /* 피해 속성 */)
        {
            var index = (int)attribute;
            if (index < 0 || index >= trackedIncomingDamageTotals.Length)
            {
                return 0f;
            }

            return trackedIncomingDamageTotals[index];
        }

        /*
         * Tick 작업 결과를 반환한다.
         */
        public bool Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */)
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

    /*
     * 상태 보호막이 흡수한 피해량과 해당 상태를 함께 전달한다.
     */
    public struct ShieldAbsorptionRecord
    {
        /*
         * ShieldAbsorptionRecord에 필요한 값을 초기화한다.
         */
        public ShieldAbsorptionRecord(StatusRuntimeInstance status /* 실행 중인 상태 효과 */, float absorbedAmount /* 흡수된 수치 */)
        {
            Status = status;
            AbsorbedAmount = absorbedAmount;
        }

        public StatusRuntimeInstance Status { get; }
        public float AbsorbedAmount { get; }
    }
}
