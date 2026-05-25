using System;
using Pakuri.Combat;

namespace Pakuri.InGame
{
    public readonly struct ShieldAbsorbRecord
    {
        public ShieldAbsorbRecord(UnitStatusRuntime status, float absorbedAmount)
        {
            Status = status;
            AbsorbedAmount = absorbedAmount;
        }

        public UnitStatusRuntime Status { get; }
        public float AbsorbedAmount { get; }
    }

    public enum UnitSide
    {
        Player,
        Enemy
    }

    public enum UnitRole
    {
        Monster,
        Enemy,
        Summon
    }

    [Serializable]
    public sealed class UnitIdentity
    {
        public string UnitId;
        public string DefinitionId;
        public string DisplayName;
        public UnitSide Side;
        public UnitRole Role;
        public int SlotIndex;
    }

    [Serializable]
    public sealed class UnitStatsRuntime
    {
        public float MaxHealth;
        public float AttackPower;
        public float SpellPower;
        public float MoveSpeed;
        public float CriticalChance;
        public float CriticalDamage;
        public float CriticalResistance;
    }

    [Serializable]
    public sealed class UnitResourceRuntime
    {
        public float CurrentHealth;
        public float CurrentShield;
        public float DirectShield;
    }

    public class BaseUnitRuntimeModel
    {
        public UnitIdentity Identity = new UnitIdentity();
        public UnitStatsRuntime Stats = new UnitStatsRuntime();
        public UnitDefenseRuntime Defenses = new UnitDefenseRuntime();
        public UnitResourceRuntime Resources = new UnitResourceRuntime();
        public UnitSkillRuntimeSet SkillRuntime = new UnitSkillRuntimeSet();
        public UnitStatusRuntimeSet Statuses = new UnitStatusRuntimeSet();
        public bool IsBoss;
        public bool AutoAttackEnabled = true;
        public bool AutoSkillEnabled = true;
    }

    public class UnitRuntimeModel : BaseUnitRuntimeModel
    {
    }

    public sealed class UnitStatusRuntimeSet
    {
        private readonly System.Collections.Generic.List<UnitStatusRuntime> statuses =
            new System.Collections.Generic.List<UnitStatusRuntime>();

        public System.Collections.Generic.IReadOnlyList<UnitStatusRuntime> ActiveStatuses => statuses;
        public int Count => statuses.Count;

        public UnitStatusRuntime Apply(
            string tag,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            float shieldAmount = 0f)
        {
            return StatusEffectUtility.TryParse(tag, out var kind)
                ? Apply(kind, stacks, durationSeconds, maxStacks, permanent, refreshDuration, shieldAmount)
                : null;
        }

        public UnitStatusRuntime Apply(
            StatusEffectKind kind,
            int stacks,
            float durationSeconds,
            int maxStacks = 0,
            bool permanent = false,
            bool refreshDuration = true,
            float shieldAmount = 0f)
        {
            var statusData = StatusEffectRuntime.CreateStatusData(kind, null);
            return Apply(statusData, stacks, durationSeconds, maxStacks, permanent, refreshDuration, shieldAmount);
        }

        public UnitStatusRuntime Apply(
            StatusEffectData statusData,
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
                status = new UnitStatusRuntime(kind);
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

        public bool Tick(float deltaTime, System.Collections.Generic.ICollection<UnitStatusRuntime> removedStatuses)
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
            return StatusEffectUtility.TryParse(tag, out var kind) && Has(kind);
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

        public int GetStacks(string tag)
        {
            return StatusEffectUtility.TryParse(tag, out var kind) ? GetStacks(kind) : 0;
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
            return StatusEffectUtility.TryParse(tag, out var kind) && Remove(kind);
        }

        public bool Remove(StatusEffectKind kind)
        {
            var removed = false;
            for (var i = 0; i < statuses.Count; i++)
            {
                if (statuses[i] != null && statuses[i].Kind == kind)
                {
                    statuses.RemoveAt(i);
                    removed = true;
                    i--;
                }
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

        public float ConsumeShield(float amount, System.Collections.Generic.ICollection<UnitStatusRuntime> depletedStatuses)
        {
            return ConsumeShield(amount, depletedStatuses, null);
        }

        public float ConsumeShield(
            float amount,
            System.Collections.Generic.ICollection<UnitStatusRuntime> depletedStatuses,
            System.Collections.Generic.ICollection<ShieldAbsorbRecord> absorbRecords)
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
                    absorbRecords.Add(new ShieldAbsorbRecord(status, absorbed));
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

        public bool ExtendDurations(StatusEffectKind kind, float durationDelta, Func<UnitStatusRuntime, bool> predicate = null)
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

        private UnitStatusRuntime Find(StatusEffectKind kind, string sourceSkillId = null)
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

        private static bool HasSourceAwareIdentity(StatusEffectData statusData)
        {
            return statusData != null
                && !string.IsNullOrWhiteSpace(statusData.SourceSkillId)
                && statusData.MergePolicy != StatusMergePolicy.Unspecified;
        }

        private static bool ShouldReplaceSourceData(StatusEffectData current, StatusEffectData incoming)
        {
            if (incoming == null)
            {
                return false;
            }

            if (current == null)
            {
                return true;
            }

            return StatusEffectRuntime.ComputeModifierMagnitude(incoming) >= StatusEffectRuntime.ComputeModifierMagnitude(current);
        }
    }

    public sealed class UnitStatusRuntime
    {
        public UnitStatusRuntime(StatusEffectKind kind)
        {
            Kind = kind;
        }

        public StatusEffectKind Kind { get; }
        public string Tag => SourceData != null && !string.IsNullOrWhiteSpace(SourceData.StatusTag)
            ? SourceData.StatusTag
            : StatusEffectUtility.ToId(Kind);
        public string DisplayName => SourceData != null && !string.IsNullOrWhiteSpace(SourceData.StatusName)
            ? SourceData.StatusName
            : StatusEffectUtility.ToDisplayName(Kind);
        public StatusEffectData SourceData { get; private set; }
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

        public void SetSourceData(StatusEffectData sourceData)
        {
            if (sourceData != null)
            {
                SourceData = sourceData;
            }
        }

        public void SetSourceMetadata(StatusEffectData sourceData)
        {
            if (sourceData == null)
            {
                return;
            }

            SourceSkillId = sourceData.SourceSkillId;
            TargetScope = sourceData.TargetScope;
            MergePolicy = sourceData.MergePolicy;
            ShieldAmountRefreshPolicy = sourceData.ShieldAmountRefreshPolicy;
        }

        public void SetSourceUnit(BaseUnitRuntimeModel source)
        {
            var identity = source != null ? source.Identity : null;
            SourceUnitId = identity != null ? identity.UnitId : string.Empty;
            SourceDefinitionId = identity != null ? identity.DefinitionId : string.Empty;
        }

        public void AddStacks(int stacks, int maxStacks)
        {
            var nextStacks = Stacks + System.Math.Max(0, stacks);
            Stacks = maxStacks > 0 ? System.Math.Min(maxStacks, nextStacks) : nextStacks;
        }

        public void RefreshStacks(int stacks, int maxStacks)
        {
            var incomingStacks = System.Math.Max(0, stacks);
            if (incomingStacks <= 0)
            {
                return;
            }

            var nextStacks = System.Math.Max(Stacks, incomingStacks);
            Stacks = maxStacks > 0 ? System.Math.Min(maxStacks, nextStacks) : nextStacks;
        }

        public void SetDuration(float durationSeconds)
        {
            DurationRemaining = System.Math.Max(0f, durationSeconds);
        }

        public bool ExtendDuration(float durationDelta)
        {
            if (Permanent || durationDelta <= 0f)
            {
                return false;
            }

            DurationRemaining = System.Math.Max(0f, DurationRemaining + durationDelta);
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
            var resolvedAmount = System.Math.Max(0f, amount);
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
                default:
                    var chosenAmount = System.Math.Max(RemainingShieldAmount, resolvedAmount);
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

            var consumed = System.Math.Min(RemainingShieldAmount, amount);
            RemainingShieldAmount = System.Math.Max(0f, RemainingShieldAmount - consumed);
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

            DurationRemaining = System.Math.Max(0f, DurationRemaining - deltaTime);
            return DurationRemaining <= 0f;
        }
    }
}
