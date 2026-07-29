/*
 * 역할: 스킬 기반 상태 효과 적용.
 * 책임: 상태 대상을 확정하고 전투 관리자를 통해 상태를 적용·연장·소비·제거한다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

/// <summary>스킬에 정의된 상태 효과 작업을 확정된 전투 대상에게 적용한다.</summary>
static class SkillStatus
{

    /// <summary>전달된 런타임 입력값을 사용해 <c>StatusSpec</c> 결과값을 생성해 반환한다.</summary>
    public static ProjectileStatusHitSpec StatusSpec(
        StatusApplicationSpec baseStatus,
        SkillExecutionData snapshot)
    {
        StatusRuntimeData statusData = null;
        if (baseStatus != null)
        {
            statusData = baseStatus.Status;
        }

        if (statusData == null)
        {
            return null;
        }

        var kind = statusData.Kind;
        var stacks = 1;
        var chance = 1f;
        var refreshDuration = true;
        if (baseStatus != null)
        {
            stacks = Math.Max(0, baseStatus.Stacks);
            chance = Mathf.Clamp01(baseStatus.Chance);
            refreshDuration = baseStatus.RefreshDuration;
        }

        if (snapshot != null)
        {
            chance = Mathf.Clamp01(chance + snapshot.StatusChanceBonus);
            if (snapshot.HasStatusStacksSet)
            {
                stacks = Math.Max(0, snapshot.StatusStacksSet);
            }
            else
            {
                stacks = Math.Max(0, stacks + snapshot.StatusStacksBonus);
            }
        }

        if (stacks <= 0 || chance <= 0f)
        {
            return null;
        }

        if (statusData == null || statusData.Kind != kind)
        {
            statusData = CatalogStatusData(kind);
        }

        var resolvedStatusData = StatusData(statusData, kind, snapshot);
        var duration = resolvedStatusData.Duration;
        var maxStacks = resolvedStatusData.MaxStacks;
        var maxStacksBonus = StatusMaxStacksBonus(snapshot, resolvedStatusData);
        if (maxStacksBonus != 0)
        {
            maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
        }

        var permanent = resolvedStatusData.Permanent;
        if (snapshot != null
            && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
        {
            duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            if (duration > 0f)
            {
                permanent = false;
            }
        }

        var durationBonus = StatusDurationBonus(snapshot, resolvedStatusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
            if (duration > 0f)
            {
                permanent = false;
            }
        }

        var thresholdStatusKind = StatusEffectKind.None;
        var thresholdStatusMinStacks = 0;
        if (snapshot != null)
        {
            thresholdStatusKind = snapshot.ThresholdStatusKind;
            thresholdStatusMinStacks = snapshot.ThresholdStatusMinStacks;
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = resolvedStatusData,
            Chance = chance,
            Stacks = stacks,
            DurationSeconds = duration,
            MaxStacks = maxStacks,
            Permanent = permanent,
            RefreshDuration = refreshDuration,
            ThresholdSourceStatusKind = thresholdStatusKind,
            ThresholdSourceMinStacks = thresholdStatusMinStacks,
            ThresholdStatusSpec = ThresholdStatusSpec(snapshot)
        };
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>DirectStatusSpec</c>를 생성한다.</summary>
    public static ProjectileStatusHitSpec CreateDirectStatusSpec(
        StatusEffectKind kind,
        int stacks,
        SkillExecutionData snapshot)
    {
        if (kind == StatusEffectKind.None || stacks <= 0)
        {
            return null;
        }

        var statusData = CatalogStatusData(kind);
        statusData = StatusData(statusData, kind, snapshot);
        var duration = statusData.Duration;
        var durationBonus = StatusDurationBonus(snapshot, statusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
        }

        var maxStacks = statusData.MaxStacks;
        var maxStacksBonus = StatusMaxStacksBonus(snapshot, statusData);
        if (maxStacksBonus != 0)
        {
            maxStacks = Mathf.Max(0, maxStacks + maxStacksBonus);
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = statusData,
            Chance = 1f,
            Stacks = stacks,
            DurationSeconds = duration,
            MaxStacks = maxStacks,
            Permanent = statusData.Permanent && duration <= 0f,
            RefreshDuration = true
        };
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>StatusData</c> 결과값을 생성해 반환한다.</summary>
    public static StatusRuntimeData StatusData(
        StatusRuntimeData statusData,
        StatusEffectKind kind,
        SkillExecutionData snapshot)
    {
        if (snapshot == null)
        {
            return statusData;
        }

        var actionSpeedBonus = snapshot.GetStatusActionSpeedBonus(statusData.StatusTag);
        var hasActionSpeedBonus = !Mathf.Approximately(actionSpeedBonus, 0f);
        var hasOverride = snapshot.HasStatusElementDamageTakenBonus
            || snapshot.HasStatusCriticalDamageTakenBonus
            || snapshot.HasStatusAilmentResistanceBonus
            || snapshot.HasStatusDamageBonusRate
            || snapshot.HasStatusShieldReceivedBonus
            || snapshot.HasStatusCriticalChanceBonus
            || snapshot.HasStatusDamageTakenBonus
            || snapshot.HasStatusFlatElementResistReduction
            || snapshot.HasStatusConditionalDamageTakenBonus
            || snapshot.HasStatusAttackPowerBonus
            || hasActionSpeedBonus;
        if (!hasOverride)
        {
            return statusData;
        }

        var resolvedStatus = statusData.Clone();
        if (snapshot.HasStatusElementDamageTakenBonus)
        {
            resolvedStatus.ElementDamageTakenBonus += snapshot.StatusElementDamageTakenBonus;
        }

        if (snapshot.HasStatusCriticalDamageTakenBonus)
        {
            resolvedStatus.CriticalDamageTakenBonus += snapshot.StatusCriticalDamageTakenBonus;
        }

        if (snapshot.HasStatusAilmentResistanceBonus)
        {
            resolvedStatus.AilmentResistanceBonus += snapshot.StatusAilmentResistanceBonus;
        }

        if (snapshot.HasStatusDamageBonusRate)
        {
            resolvedStatus.Modifiers.DamageBonusRate += snapshot.StatusDamageBonusRate;
        }

        if (snapshot.HasStatusShieldReceivedBonus)
        {
            resolvedStatus.Modifiers.ShieldReceivedBonus += snapshot.StatusShieldReceivedBonus;
        }

        if (snapshot.HasStatusCriticalChanceBonus)
        {
            resolvedStatus.Modifiers.CritChanceBonusRate += snapshot.StatusCriticalChanceBonus;
        }

        if (snapshot.HasStatusDamageTakenBonus)
        {
            resolvedStatus.DamageTakenBonus += snapshot.StatusDamageTakenBonus;
        }

        if (snapshot.HasStatusFlatElementResistReduction)
        {
            resolvedStatus.FlatElementResistReduction += snapshot.StatusFlatElementResistReduction;
        }

        if (snapshot.HasStatusConditionalDamageTakenBonus)
        {
            resolvedStatus.ConditionalSourceStatusKind = snapshot.StatusConditionalSourceStatusKind;
            resolvedStatus.ConditionalDamageTakenBonus = snapshot.StatusConditionalDamageTakenBonus;
        }

        if (hasActionSpeedBonus)
        {
            resolvedStatus.Modifiers.ActionSpeedBonus += actionSpeedBonus;
        }

        if (snapshot.HasStatusAttackPowerBonus)
        {
            resolvedStatus.Modifiers.AttackPowerBonus += snapshot.StatusAttackPowerBonus;
        }

        return resolvedStatus;
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>StatusDurationBonus</c> 결과값을 생성해 반환한다.</summary>
    private static float StatusDurationBonus(SkillExecutionData snapshot, StatusRuntimeData statusData)
    {
        if (snapshot == null)
        {
            return 0f;
        }

        return snapshot.StatusDurationBonus(statusData.StatusTag);
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>StatusMaxStacksBonus</c> 결과값을 생성해 반환한다.</summary>
    private static int StatusMaxStacksBonus(SkillExecutionData snapshot, StatusRuntimeData statusData)
    {
        if (snapshot == null)
        {
            return 0;
        }

        return snapshot.StatusMaxStacksBonus(statusData.StatusTag);
    }

    /// <summary>전달된 <c>snapshot</c> 값을 사용해 <c>ThresholdStatusSpec</c> 결과값을 생성해 반환한다.</summary>
    private static ProjectileStatusHitSpec ThresholdStatusSpec(SkillExecutionData snapshot)
    {
        if (snapshot == null || snapshot.ThresholdApplyStatusKind == StatusEffectKind.None)
        {
            return null;
        }

        var kind = snapshot.ThresholdApplyStatusKind;
        var statusData = CatalogStatusData(kind);
        var duration = statusData.Duration;
        var durationBonus = StatusDurationBonus(snapshot, statusData);
        if (!Mathf.Approximately(durationBonus, 0f))
        {
            duration = Mathf.Max(0f, duration + durationBonus);
        }

        return new ProjectileStatusHitSpec
        {
            Enabled = true,
            Kind = kind,
            StatusData = statusData,
            Chance = 1f,
            Stacks = statusData.BaseStackAmount,
            DurationSeconds = duration,
            MaxStacks = statusData.MaxStacks,
            Permanent = statusData.Permanent && duration <= 0f,
            RefreshDuration = true
        };
    }

    /// <summary>전달된 <c>kind</c> 값을 사용해 <c>CatalogStatusData</c> 결과값을 생성해 반환한다.</summary>
    private static StatusRuntimeData CatalogStatusData(StatusEffectKind kind)
    {
        return GameDataLoader.CurrentCatalog?.GetStatusRuntimeData(kind)
            ?? throw new InvalidOperationException($"Status runtime data '{kind}' is not registered.");
    }
}
}
