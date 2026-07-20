using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * Buff와 Single 계열의 세부 실행기를 정의한다.
 * Buff의 보호막·회복과 Single의 연쇄·돌진 처리를 각 전용 실행기로 전달한다.
 */
namespace Pakuri.InGame
{
    public sealed class BuffSkillExecutor : TypedSkillExecutor<BuffSkillRuntimeData>
    {
        /*
         * 요청받은 버프 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as BuffSkillRuntimeData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var statusSpec = ResolveBuffStatusSpec(skill, snapshot);
            if (statusSpec == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var targets = skill.UseConfiguredTargeting
                ? ResolveConfiguredTargets(context.CasterEntry, context.Roster, skill.Targeting)
                : ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var preferredPrefab = snapshot != null ? snapshot.SkillEffectPrefab : null;
            var prefab = effects != null
                ? effects.ResolveSkillEffectPrefab(context.Caster, skill.SkillId, preferredPrefab)
                : null;
            var routed = false;
            var castCommitted = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                castCommitted = true;
                if (UnityEngine.Random.value > Mathf.Clamp01(statusSpec.Chance))
                {
                    continue;
                }

                context.CombatManager.ApplyStatus(
                    target.Model,
                    statusSpec.StatusData,
                    statusSpec.Stacks,
                    statusSpec.DurationSeconds,
                    statusSpec.MaxStacks,
                    statusSpec.Permanent,
                    statusSpec.RefreshDuration,
                    context.Caster);

                var visualTarget = skill.AttachVisualToCaster ? context.CasterEntry.Transform : target.Transform;
                var canSpawnVisual = !skill.AttachVisualToCaster || !casterVisualSpawned;
                GameObject visualInstance = null;
                if (canSpawnVisual && visualTarget != null && effects != null)
                {
                    visualInstance = effects.SpawnAttachedEffect(
                        runtimeVisual,
                        prefab,
                        string.IsNullOrWhiteSpace(skill.SkillId)
                            ? "RuntimeBuffVisual"
                            : $"RuntimeBuffVisual_{skill.SkillId}",
                        visualTarget,
                        statusSpec.DurationSeconds,
                        Vector3.zero);
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            var multiEffectRouted = false;
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillMultiEffectExecutor.ExecuteWithStatusDurationScaling(context, snapshot, planEffects, center);
            }

            return new SkillExecutionResult(routed || castCommitted || multiEffectRouted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }

        /*
         * 버프 상태 설정을 결정한다.
         */
        private static ProjectileStatusHitSpec ResolveBuffStatusSpec(BuffSkillRuntimeData skill, SkillExecutionSnapshot snapshot)
        {
            if (skill == null)
            {
                return null;
            }

            var spec = SkillStatusSpecUtility.ResolveStatusSpec(skill.AttachedStatus, snapshot);
            if (spec != null)
            {
                return spec;
            }

            if (!string.IsNullOrWhiteSpace(skill.ApplyStatusTag)
                && StatusEffectUtility.TryParse(skill.ApplyStatusTag, out var kind))
            {
                var statusData = StatusEffectFactory.Create(kind, skill.ApplyStatusTag);
                if (statusData != null)
                {
                    statusData.SourceSkillId = skill.SkillId;
                    statusData.TargetScope = skill.Target == BuffTarget.Self
                        ? StatusTargetScope.Self
                        : StatusTargetScope.AllAllies;
                    statusData.MergePolicy = StatusMergePolicy.SameSourceRefresh;
                }

                return new ProjectileStatusHitSpec
                {
                    Enabled = true,
                    Kind = kind,
                    StatusData = statusData,
                    Chance = 1f,
                    Stacks = statusData != null ? Math.Max(1, statusData.BaseStackAmount) : 1,
                    DurationSeconds = statusData != null ? statusData.Duration : 0f,
                    MaxStacks = statusData != null ? statusData.MaxStacks : 0,
                    Permanent = statusData != null && statusData.Permanent,
                    RefreshDuration = true
                };
            }

            return null;
        }

        /*
         * 버프 대상을 결정한다.
         */
        internal static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveBuffTargets(
            UnitRosterEntry caster,
            UnitRosterService roster,
            BuffTarget targetMode)
        {
            if (targetMode == BuffTarget.Self)
            {
                return caster != null
                    ? new[] { caster }
                    : System.Array.Empty<UnitRosterEntry>();
            }

            return SkillExecutionUtility.ResolveTargetList(
                caster,
                roster,
                new SkillTargetingSpec
                {
                    TargetSide = SkillTargetSide.AllAllies,
                    Selection = SkillTargetSelection.Owner,
                    Shape = SkillTargetShape.Battlefield,
                    CoverAll = true
                });
        }

        /*
         * 설정된 대상을 결정한다.
         */
        internal static IReadOnlyList<UnitRosterEntry> ResolveConfiguredTargets(
            UnitRosterEntry caster,
            UnitRosterService roster,
            SkillTargetingSpec targeting)
        {
            var targets = SkillExecutionUtility.ResolveOrderedTargets(caster, roster, targeting);
            if (caster == null || caster.Transform == null || targeting == null || targeting.Radius <= 0f)
            {
                return targets;
            }

            var radiusSq = targeting.Radius * targeting.Radius;
            targets.RemoveAll(target =>
                target == null
                || target.Transform == null
                || ((Vector2)target.Transform.position - (Vector2)caster.Transform.position).sqrMagnitude > radiusSq);
            return targets;
        }
    }

    /*
     * 보호막 스킬을 실행한다.
     */
    public sealed class BuffShieldSkillExecutor : TypedSkillExecutor<BuffShieldSkillRuntimeData>
    {
        /*
         * 요청받은 보호막 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as BuffShieldSkillRuntimeData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var shield = SkillExecutionUtility.ResolveShield(context.Caster, skill, snapshot);
            var duration = skill.ShieldDuration > 0f
                ? skill.ShieldDuration
                : skill.ShieldStatus != null ? skill.ShieldStatus.Duration : 0f;
            if (snapshot != null
                && (!Mathf.Approximately(snapshot.DurationMultiplier, 1f)
                    || !Mathf.Approximately(snapshot.DurationBonus, 0f)))
            {
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            var statusData = SkillStatusSpecUtility.ResolveStatusData(skill.ShieldStatus, StatusEffectKind.Shield, snapshot);
            if (statusData == null || duration <= 0f)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var effects = context.CombatManager.Effects;
            var runtimeVisual = skill.RuntimeVisual;
            var preferredPrefab = snapshot != null ? snapshot.SkillEffectPrefab : null;
            var prefab = effects != null
                ? effects.ResolveSkillEffectPrefab(context.Caster, skill.SkillId, preferredPrefab)
                : null;

            var targets = skill.UseConfiguredTargeting
                ? BuffSkillExecutor.ResolveConfiguredTargets(context.CasterEntry, context.Roster, skill.Targeting)
                : BuffSkillExecutor.ResolveBuffTargets(context.CasterEntry, context.Roster, skill.Target);
            var routed = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    statusData,
                    shield,
                    duration,
                    1,
                    0,
                    false,
                    true,
                    context.Caster);
                var visualTarget = skill.AttachVisualToCaster ? context.CasterEntry.Transform : target.Transform;
                var canSpawnVisual = !skill.AttachVisualToCaster || !casterVisualSpawned;
                GameObject visualInstance = null;
                if (canSpawnVisual && visualTarget != null && effects != null)
                {
                    visualInstance = effects.SpawnAttachedEffect(
                        runtimeVisual,
                        prefab,
                        string.IsNullOrWhiteSpace(skill.SkillId)
                            ? "RuntimeShieldVisual"
                            : $"RuntimeShieldVisual_{skill.SkillId}",
                        visualTarget,
                        duration,
                        Vector3.zero);
                }

                if (visualInstance != null)
                {
                    casterVisualSpawned = skill.AttachVisualToCaster;
                }

                routed = true;
            }

            var multiEffectRouted = false;
            var planEffects = SkillPlanActionDispatcher.ResolveEffects(snapshot, skill.MultiEffects);
            if (routed && planEffects.Length > 0)
            {
                var center = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                multiEffectRouted = SkillMultiEffectExecutor.ExecuteWithStatusDurationScaling(context, snapshot, planEffects, center);
            }

            return new SkillExecutionResult(routed || multiEffectRouted ? SkillExecutionStatus.Routed : SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
        }
    }

    /*
     * 회복 스킬을 실행한다.
     */
    public sealed class BuffHealSkillExecutor : TypedSkillExecutor<BuffHealSkillRuntimeData>
    {
        /*
         * 요청받은 회복 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as BuffHealSkillRuntimeData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(context.CasterEntry, context.Roster, skill.Targeting);
            var target = targets.Count > 0 ? targets[0] : null;
            if (target == null || target.Model == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            var amount = SkillExecutionUtility.ResolvePowerValue(context.Caster, skill.Healing);
            if (context.Caster is EnemyUnitRuntimeModel enemy)
            {
                amount *= Mathf.Max(0f, enemy.PassiveHealingMultiplier);
            }

            context.CombatManager.Heal(target.Model, amount);
            var effects = context.CombatManager.Effects;
            if (effects != null)
            {
                effects.SpawnAttachedSkillEffect(skill, target.Transform, 0.8f);
            }
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }
    }

    /*
     * 연쇄 공격 스킬을 실행한다.
     */
    public sealed class SingleChainSkillExecutor : TypedSkillExecutor<SingleChainSkillRuntimeData>
    {
        /*
         * 요청받은 연쇄 공격 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as SingleChainSkillRuntimeData : null;
            if (skill == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var primary = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            if (primary == null || primary.Model == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            ApplyDamage(context, snapshot, skill, primary, 1f);
            var effects = context.CombatManager.Effects;
            if (effects != null)
            {
                effects.SpawnAttachedSkillEffect(skill, primary.Transform, 0.8f);
            }
            if (skill.ChainDelaySeconds > 0f)
            {
                context.CombatManager.StartCoroutine(ExecuteChainAfterDelay(context, snapshot, skill, primary.Model));
            }
            else
            {
                ExecuteChain(context, snapshot, skill, primary.Model);
            }

            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }

        /*
         * 지정 간격 후 다음 연쇄 공격을 실행한다.
         */
        private static IEnumerator ExecuteChainAfterDelay(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleChainSkillRuntimeData skill,
            BaseUnitRuntimeModel primary)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, skill.ChainDelaySeconds));
            ExecuteChain(context, snapshot, skill, primary);
        }

        /*
         * 연쇄를 실행한다.
         */
        private static void ExecuteChain(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleChainSkillRuntimeData skill,
            BaseUnitRuntimeModel primary)
        {
            if (context == null || context.Roster == null || context.CasterEntry == null)
            {
                return;
            }

            var targets = SkillExecutionUtility.ResolveOrderedTargets(context.CasterEntry, context.Roster, skill.Targeting);
            var primaryEntry = context.Roster.Find(primary);
            var origin = primaryEntry != null && primaryEntry.Transform != null
                ? (Vector2)primaryEntry.Transform.position
                : context.CasterEntry.Transform != null ? (Vector2)context.CasterEntry.Transform.position : Vector2.zero;
            UnitRosterEntry best = null;
            var bestDistanceSq = float.MaxValue;
            var radiusSq = skill.ChainRadius > 0f ? skill.ChainRadius * skill.ChainRadius : float.MaxValue;
            for (var i = 0; i < targets.Count; i++)
            {
                var candidate = targets[i];
                if (candidate == null
                    || candidate.Model == null
                    || candidate.Transform == null
                    || (skill.ExcludePrimaryTarget && ReferenceEquals(candidate.Model, primary)))
                {
                    continue;
                }

                var distanceSq = ((Vector2)candidate.Transform.position - origin).sqrMagnitude;
                if (distanceSq > radiusSq || distanceSq >= bestDistanceSq)
                {
                    continue;
                }

                best = candidate;
                bestDistanceSq = distanceSq;
            }

            if (best == null)
            {
                return;
            }

            ApplyDamage(context, snapshot, skill, best, skill.ChainDamageMultiplier);
            var effects = context.CombatManager.Effects;
            if (effects != null)
            {
                effects.SpawnAttachedSkillEffect(skill, best.Transform, 0.8f);
            }
        }

        /*
         * 피해를 적용한다.
         */
        private static void ApplyDamage(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
            SingleChainSkillRuntimeData skill,
            UnitRosterEntry target,
            float multiplier)
        {
            var damage = SkillExecutionUtility.ResolveDamage(context.Caster, skill.Damage, snapshot)
                * Mathf.Max(0f, multiplier);
            context.CombatManager.ApplyDamage(
                target.Model,
                damage,
                SkillExecutionUtility.MapAttribute(skill.Damage.Element),
                context.Caster,
                skill.Damage.CriticalAllowed,
                sourceSkillId: skill.SkillId);
        }
    }

    /*
     * 돌진 스킬을 실행한다.
     */
    public sealed class SingleChargeSkillExecutor : TypedSkillExecutor<SingleChargeSkillRuntimeData>
    {
        /*
         * 요청받은 돌진 스킬을 실행한다.
         */
        public override SkillExecutionResult Execute(SkillExecutionContext context, SkillExecutionSnapshot snapshot)
        {
            var skill = context != null ? context.SkillRuntimeData as SingleChargeSkillRuntimeData : null;
            if (skill == null || context.Caster == null || context.CasterEntry == null || context.Roster == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, snapshot != null ? snapshot.SkillId : string.Empty, GetType().Name);
            }

            var target = SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            if (target == null || target.Model == null)
            {
                return new SkillExecutionResult(SkillExecutionStatus.Rejected, skill.SkillId, GetType().Name);
            }

            context.Caster.ActiveCharge = new UnitChargeRuntime
            {
                SkillId = skill.SkillId,
                TargetUnitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null,
                RampSeconds = skill.RampSeconds,
                MaxMoveSpeedMultiplier = skill.MaxMoveSpeedMultiplier,
                DamageTargetMaxHealthRatio = skill.TargetMaxHealthRatio,
                OnHitStatus = skill.OnHitStatus,
                Attribute = SkillExecutionUtility.MapAttribute(skill.Element)
            };
            return new SkillExecutionResult(SkillExecutionStatus.Routed, skill.SkillId, GetType().Name);
        }
    }

    /*
     * 돌진 중인 유닛과 목표, 접촉 판정을 추적한다.
     */
    public static class SingleChargeRuntime
    {
        /*
         * 돌진 이동과 대상 접촉을 갱신하고 종료 여부를 반환한다.
         */
        public static bool Tick(
            UnitRosterEntry casterEntry,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime)
        {
            var caster = casterEntry != null ? casterEntry.Model : null;
            var charge = caster != null ? caster.ActiveCharge : null;
            if (charge == null)
            {
                return false;
            }

            if (casterEntry.Transform == null || roster == null || combatManager == null)
            {
                caster.ActiveCharge = null;
                return true;
            }

            var hitTarget = FindHitTarget(casterEntry, roster);
            if (hitTarget != null)
            {
                ResolveHit(caster, hitTarget, combatManager, charge);
                return true;
            }

            var target = FindTargetByUnitId(casterEntry, roster, charge.TargetUnitId)
                ?? SkillExecutionUtility.FindNearestTarget(
                    casterEntry,
                    roster,
                    new SkillTargetingSpec
                    {
                        TargetSide = SkillTargetSide.Enemy,
                        Selection = SkillTargetSelection.Random
                    });
            if (target == null || target.Transform == null)
            {
                caster.ActiveCharge = null;
                return true;
            }

            charge.ElapsedSeconds += Mathf.Max(0f, deltaTime);
            var ramp = charge.RampSeconds > 0f ? Mathf.Clamp01(charge.ElapsedSeconds / charge.RampSeconds) : 1f;
            var speedMultiplier = Mathf.Lerp(1f, Mathf.Max(1f, charge.MaxMoveSpeedMultiplier), ramp);
            var baseSpeed = caster.Stats != null ? Mathf.Max(0f, caster.Stats.MoveSpeed) : 0f;
            var speed = baseSpeed * speedMultiplier * StatusEffectRules.ResolveMoveSpeedMultiplier(caster);
            if (speed > 0f && StatusEffectRules.CanMove(caster))
            {
                var current = casterEntry.Transform.position;
                var next = Vector3.MoveTowards(current, target.Transform.position, speed * Mathf.Max(0f, deltaTime));
                casterEntry.Transform.position = next;
            }

            hitTarget = FindHitTarget(casterEntry, roster);
            if (hitTarget != null)
            {
                ResolveHit(caster, hitTarget, combatManager, charge);
            }

            return true;
        }

        /*
         * 대상 기준 유닛 ID를 찾는다.
         */
        private static UnitRosterEntry FindTargetByUnitId(
            UnitRosterEntry casterEntry,
            UnitRosterService roster,
            string unitId)
        {
            var targets = SkillExecutionUtility.ResolveTargetList(
                casterEntry,
                roster,
                new SkillTargetingSpec { TargetSide = SkillTargetSide.Enemy });
            for (var i = 0; i < targets.Count; i++)
            {
                var identity = targets[i] != null && targets[i].Model != null ? targets[i].Model.Identity : null;
                if (identity != null && string.Equals(identity.UnitId, unitId, StringComparison.OrdinalIgnoreCase))
                {
                    return targets[i];
                }
            }

            return null;
        }

        /*
         * 적중 대상을 찾는다.
         */
        private static UnitRosterEntry FindHitTarget(UnitRosterEntry casterEntry, UnitRosterService roster)
        {
            var targets = SkillExecutionUtility.ResolveTargetList(
                casterEntry,
                roster,
                new SkillTargetingSpec { TargetSide = SkillTargetSide.Enemy });
            var casterColliders = casterEntry.GetHitboxColliders();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target != null && target.IsAlive && HasChargeContact(casterEntry, casterColliders, target))
                {
                    return target;
                }
            }

            return null;
        }

        /*
         * 돌진 접촉을 보유하고 있는지 확인한다.
         */
        private static bool HasChargeContact(
            UnitRosterEntry casterEntry,
            Collider2D[] casterColliders,
            UnitRosterEntry target)
        {
            if (UnitHitboxUtility.IsTargetInsideHitbox(casterColliders, target))
            {
                return true;
            }

            if (casterEntry == null
                || casterEntry.Transform == null
                || target == null
                || target.Transform == null)
            {
                return false;
            }

            return ((Vector2)casterEntry.Transform.position - (Vector2)target.Transform.position).sqrMagnitude <= 0.0025f;
        }

        /*
         * 적중을 결정한다.
         */
        private static void ResolveHit(
            BaseUnitRuntimeModel caster,
            UnitRosterEntry target,
            InGameCombatManager combatManager,
            UnitChargeRuntime charge)
        {
            var maxHealth = target.Model != null && target.Model.Stats != null
                ? Mathf.Max(0f, target.Model.Stats.MaxHealth)
                : 0f;
            combatManager.ApplyDamage(
                target.Model,
                maxHealth * Mathf.Max(0f, charge.DamageTargetMaxHealthRatio),
                charge.Attribute,
                caster,
                true,
                sourceSkillId: charge.SkillId);
            var statusSpec = SkillStatusSpecUtility.ResolveStatusSpec(charge.OnHitStatus, null);
            if (statusSpec != null)
            {
                SkillStatusApplyUtility.TryApplyStatus(combatManager, target.Model, statusSpec, caster);
            }

            caster.ActiveCharge = null;
        }
    }
}
