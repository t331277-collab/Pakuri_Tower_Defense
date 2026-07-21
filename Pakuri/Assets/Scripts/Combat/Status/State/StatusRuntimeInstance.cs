using System;
using Pakuri.Combat;

/*
 * 적용된 상태 효과 하나의 출처, 중첩, 지속시간, 보호막과 피해 기록을 보관한다.
 */
namespace Pakuri.InGame
{
    public sealed class StatusRuntimeInstance
    {
        public StatusRuntimeInstance(StatusEffectKind kind)
        {
            Kind = kind;
        }

        public StatusEffectKind Kind { get; }
        public string Tag => SourceData != null && !string.IsNullOrWhiteSpace(SourceData.StatusTag)
            ? SourceData.StatusTag
            : StatusEffectLookup.ToId(Kind);
        public string DisplayName => SourceData != null && !string.IsNullOrWhiteSpace(SourceData.StatusName)
            ? SourceData.StatusName
            : StatusEffectLookup.ToDisplayName(Kind);
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
            if (sourceData != null)
            {
                SourceData = sourceData;
            }
        }

        public void SetSourceMetadata(StatusRuntimeData sourceData)
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

        public void SetSourceUnit(UnitCombatState source)
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

        public int ConsumeStacks(int stacks)
        {
            var consumed = System.Math.Min(System.Math.Max(0, stacks), System.Math.Max(0, Stacks));
            Stacks = System.Math.Max(0, Stacks - consumed);
            return consumed;
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
