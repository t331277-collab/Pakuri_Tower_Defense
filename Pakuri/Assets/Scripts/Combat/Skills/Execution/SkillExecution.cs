/*
 * 역할: 사용 가능 여부를 검증하고 확정값을 준비해 계열별 실행기로 분배하며 진행 상태를 갱신한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 시전 가능 여부를 확인하고 확정된 효과를 실행 방식에 맞게 분배한다.
    public class SkillExecution
    {
        /// 자동 시전 후보를 선별한다.
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry, SkillExecutionState runtime);

        /// 쿨타임, 탄창 초기화
        public static void ResetRuntimeState(SkillExecutionState runtime)
        {
            if (runtime == null)
            {
                return;
            }

            runtime.CastRemaining = 0f;
            runtime.ActiveDurationRemaining = 0f;
            runtime.TickRemaining = 0f;
            runtime.ReloadRemaining = 0f;
            runtime.MagazineRemaining = runtime.MaxMagazineSize;
            runtime.reloadCyclePending = false;
            runtime.reloadCompleteEventPending = false;
            runtime.pendingReloadDamageMultiplier = 1f;
            runtime.armedReloadDamageMultiplier = 1f;
            ResetCooldown(runtime);
            runtime.queuedBurstShotsRemaining = 0;
            runtime.ProjectileLaunchCount = 0;
            runtime.SkillHitCount = 0;
            runtime.ActiveExecutionData = null;
            runtime.consecutiveHitTargetUnitName = string.Empty;
            runtime.consecutiveHitRepeatCount = 0;
        }

        /// 발사 순서에 의존하는 규칙이 사용할 다음 순번을 확정
        public static int AdvanceProjectileLaunchCount(SkillExecutionState runtime)
        {
            if (runtime == null)
            {
                return 0;
            }

            if (runtime.ProjectileLaunchCount == int.MaxValue)
            {
                runtime.ProjectileLaunchCount = 0;
            }

            runtime.ProjectileLaunchCount++;
            return runtime.ProjectileLaunchCount;
        }

        /// 누적 적중 규칙이 사용할 다음 순번을 확정
        public static int AdvanceSkillHitCount(SkillExecutionState runtime)
        {
            if (runtime == null)
            {
                return 0;
            }

            if (runtime.SkillHitCount == int.MaxValue)
            {
                runtime.SkillHitCount = 0;
            }

            runtime.SkillHitCount++;
            return runtime.SkillHitCount;
        }

        /// 연속 적중 상태를 다음 적중 기준으로 갱신한다.
        public static int AdvanceConsecutiveHitCount(
            SkillExecutionState runtime,
            UnitCombatState target)
        {
            if (runtime == null || target == null)
            {
                return -1;
            }

            var unitName = target.Identity != null ? target.Identity.UnitName : string.Empty;
            if (string.IsNullOrWhiteSpace(unitName))
            {
                runtime.consecutiveHitTargetUnitName = string.Empty;
                runtime.consecutiveHitRepeatCount = 0;
                return 0;
            }

            if (string.Equals(runtime.consecutiveHitTargetUnitName, unitName, StringComparison.Ordinal))
            {
                runtime.consecutiveHitRepeatCount = Math.Min(
                    runtime.consecutiveHitRepeatCount + 1,
                    int.MaxValue - 1);
            }
            else
            {
                runtime.consecutiveHitTargetUnitName = unitName;
                runtime.consecutiveHitRepeatCount = 0;
            }

            return runtime.consecutiveHitRepeatCount;
        }

        /// 행동 속도와 실제 시간 기준에 맞춰 모든 쿨다운 상태를 대기.
        public static void Tick(SkillExecutionState runtime, float deltaTime)
        {
            if (runtime == null || deltaTime <= 0f)
            {
                return;
            }

            var actionDeltaTime = deltaTime
                * StatusCombatRules.ActionSpeedMultiplier(runtime.Owner);
            var cooldownDeltaTime = actionDeltaTime
                * ArtifactCombatRules.CooldownChargeMultiplier(runtime.Owner);
            runtime.CooldownRemaining = TickDown(runtime.CooldownRemaining, cooldownDeltaTime);
            runtime.CastRemaining = TickDown(runtime.CastRemaining, actionDeltaTime);
            runtime.ActiveDurationRemaining = TickDown(
                runtime.ActiveDurationRemaining,
                deltaTime);
            runtime.TickRemaining = TickDown(runtime.TickRemaining, actionDeltaTime);
            runtime.ReloadRemaining = TickDown(runtime.ReloadRemaining, deltaTime);

            TryCompleteReload(runtime);
        }

        /// 쿨다운과 탄창, 시전 간격(투사체)이 현재 스킬 사용 발동을 허용하는지 판정
        public static bool CanCastWithData(
            SkillExecutionState runtime,
            SkillExecutionState snapshot)
        {
            if (runtime == null)
            {
                return false;
            }

            if (runtime.Data == null
                || !runtime.Data.IsActive
                || runtime.IsCasting
                || !IsCastIntervalReady(runtime))
            {
                return false;
            }

            if (runtime.IsBursting)
            {
                return !runtime.IsReloading;
            }

            return runtime.CooldownRemaining <= 0f
                && !runtime.IsReloading
                && runtime.HasMagazine;
        }

        /// 성공한 시전의 탄창과 연사상태를 갱신한다.
        public static bool TryBeginCast(
            SkillExecutionState runtime,
            SkillExecutionState snapshot)
        {
            if (runtime == null)
            {
                return false;
            }

            if (runtime.IsBursting)
            {
                runtime.queuedBurstShotsRemaining = Math.Max(
                    0,
                    runtime.queuedBurstShotsRemaining - 1);
                runtime.TickRemaining = runtime.IsBursting
                    ? runtime.effectiveBurstInterval
                    : runtime.effectiveTickInterval;
                if (!runtime.IsBursting)
                {
                    BeginRecoveryIfNeeded(runtime);
                }

                runtime.ActiveExecutionData = snapshot;
                return true;
            }

            if (!CanCastWithData(runtime, snapshot))
            {
                return false;
            }

            if (runtime.UsesMagazine)
            {
                var wasLastProjectile = runtime.MagazineRemaining == 1;
                runtime.MagazineRemaining = Math.Max(
                    0,
                    runtime.MagazineRemaining - 1);
                if (wasLastProjectile)
                {
                    runtime.pendingReloadDamageMultiplier = Mathf.Max(
                        1f,
                        snapshot?.ReloadCompleteDamageMultiplier ?? 1f);
                }
            }

            var timing = runtime.Data.Timing;
            runtime.ActiveDurationRemaining = timing != null
                ? Mathf.Max(0f, timing.ActiveDuration)
                : 0f;
            runtime.queuedBurstShotsRemaining = Math.Max(
                0,
                runtime.effectiveBurstProjectileCount - 1);
            runtime.TickRemaining = runtime.IsBursting
                ? runtime.effectiveBurstInterval
                : runtime.effectiveTickInterval;
            if (!runtime.IsBursting)
            {
                BeginRecoveryIfNeeded(runtime);
            }

            runtime.ActiveExecutionData = snapshot;
            runtime.armedReloadDamageMultiplier = 1f;
            return true;
        }

        /// 진행 중인 지속 실행을 끝낸다.
        public static void StopActive(SkillExecutionState runtime)
        {
            if (runtime == null)
            {
                return;
            }

            runtime.ActiveDurationRemaining = 0f;
            runtime.ActiveExecutionData = null;
        }

        /// 현재 연사 묶음의 투사체 순번을 계산한다.
        public static int CurrentBurstProjectileIndex(SkillExecutionState runtime)
        {
            if (runtime == null
                || runtime.effectiveBurstProjectileCount <= 1
                || !runtime.IsBursting)
            {
                return 1;
            }

            return Mathf.Clamp(
                runtime.effectiveBurstProjectileCount
                    - runtime.queuedBurstShotsRemaining
                    + 1,
                1,
                runtime.effectiveBurstProjectileCount);
        }

        /// 재장전 대기를 적용
        public static bool ReduceReloadRemaining(
            SkillExecutionState runtime,
            float seconds)
        {
            if (runtime == null
                || seconds <= 0f
                || runtime.ReloadRemaining <= 0f)
            {
                return false;
            }

            runtime.ReloadRemaining = Mathf.Max(
                0f,
                runtime.ReloadRemaining - seconds);
            TryCompleteReload(runtime);

            return true;
        }

        /// 재사용 대기를 적용
        public static bool ReduceCooldownRemaining(
            SkillExecutionState runtime,
            float seconds)
        {
            if (runtime == null
                || seconds <= 0f
                || runtime.CooldownRemaining <= 0f)
            {
                return false;
            }

            runtime.CooldownRemaining = Mathf.Max(
                0f,
                runtime.CooldownRemaining - seconds);
            TryCompleteReload(runtime);

            return true;
        }

        /// 재사용 대기를 즉시 끝낸다.
        public static void ResetCooldown(SkillExecutionState runtime)
        {
            if (runtime == null)
            {
                return;
            }

            runtime.CooldownRemaining = 0f;
            TryCompleteReload(runtime);
        }

        /// 남은 시간을 0 아래로 내려가지 않게 줄인다.
        private static float TickDown(float value, float deltaTime)
        {
            return value > 0f
                ? Mathf.Max(0f, value - deltaTime)
                : 0f;
        }

        /// 다음 발사 간격이 지났는지 확인한다.
        private static bool IsCastIntervalReady(SkillExecutionState runtime)
        {
            return runtime.effectiveTickInterval <= 0f
                || runtime.TickRemaining <= 0f;
        }

        /// 탄창 소모 결과에 맞는 회복을 시작한다.
        private static void BeginRecoveryIfNeeded(SkillExecutionState runtime)
        {
            if (!runtime.UsesMagazine)
            {
                runtime.CooldownRemaining = runtime.effectiveCooldownDuration;
                return;
            }

            if (runtime.MagazineRemaining > 0)
            {
                return;
            }

            runtime.CooldownRemaining = runtime.effectiveCooldownDuration;
            runtime.reloadCyclePending = true;
            if (runtime.ReloadDuration > 0f)
            {
                runtime.ReloadRemaining = runtime.ReloadDuration;
                return;
            }

            TryCompleteReload(runtime);
        }

        /// 실제 탄창 복구와 reload lifecycle event 예약을 한 지점에서 수행한다.
        private static bool TryCompleteReload(SkillExecutionState runtime)
        {
            if (runtime == null
                || !runtime.reloadCyclePending
                || !runtime.UsesMagazine
                || runtime.MagazineRemaining > 0
                || runtime.ReloadRemaining > 0f
                || runtime.CooldownRemaining > 0f
                || runtime.IsBursting)
            {
                return false;
            }

            runtime.MagazineRemaining = runtime.MaxMagazineSize;
            runtime.reloadCyclePending = false;
            runtime.reloadCompleteEventPending = true;
            runtime.armedReloadDamageMultiplier = Mathf.Max(
                1f,
                runtime.pendingReloadDamageMultiplier);
            runtime.pendingReloadDamageMultiplier = 1f;
            return true;
        }

        /// reload 완료 사건을 정확히 한 번 소비한다.
        internal static bool ConsumeReloadCompleteEvent(SkillExecutionState runtime)
        {
            if (runtime == null || !runtime.reloadCompleteEventPending)
            {
                return false;
            }

            runtime.reloadCompleteEventPending = false;
            return true;
        }
        /// 자동 시전 가능한 스킬만 실행 흐름에 올린다.
        public void TryExecuteAutomaticSkills(
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            SkillAutoRoutePredicate canAutoRoute = null)
        {
            if (roster == null)
            {
                return;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || entry.Model == null)
                {
                    continue;
                }

                var model = entry.Model;
                if (!model.AutoSkillEnabled || !entry.IsAlive || !StatusCombatRules.CanAct(model))
                {
                    continue;
                }

                var activeSkills = model.SkillState.ActiveSkills;
                for (var skillIndex = 0; skillIndex < activeSkills.Count; skillIndex++)
                {
                    var runtime = activeSkills[skillIndex];
                    if (canAutoRoute != null && !canAutoRoute(entry, runtime))
                    {
                        continue;
                    }

                    var targeting = runtime?.Data?.Targeting;
                    if (targeting != null
                        && targeting.TargetSide == SkillTargetSide.Enemy
                        && SkillTargeting.FindNearestTarget(entry, roster, targeting) == null)
                    {
                        continue;
                    }

                    TryExecuteSelected(entry, runtime, roster, combatManager);
                }
            }
        }

        /// 수동 조준을 실행 입력으로 바꾼다.
        public bool TryExecuteManual(
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            Vector2 aimDirection,
            Vector2 targetPoint)
        {
            return TryExecuteSkill(
                entry,
                runtime,
                roster,
                combatManager,
                true,
                aimDirection,
                true,
                targetPoint,
                true,
                1f,
                null);
        }

        /// 현재 상태에서 선택한 스킬의 시작 가능 여부를 판정한다.
        public bool CanExecuteSelected(
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            UnitSpawnManager roster)
        {
            if (entry == null
                || runtime == null
                || !StatusCombatRules.CanAct(entry.Model))
            {
                return false;
            }

            var snapshot = entry.Model.SkillState.CreateExecutionData(entry.Model, runtime, roster);
            return CanCastWithData(runtime, snapshot);
        }

        /// 자동 조준으로 선택한 스킬을 실행 흐름에 올린다.
        public bool TryExecuteSelected(
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            UnitSpawnManager roster,
            InGameCombatManager combatManager)
        {
            return TryExecuteSkill(
                entry,
                runtime,
                roster,
                combatManager,
                false,
                default,
                false,
                default,
                true,
                1f,
                null);
        }

        /// 사건에서 파생된 실행을 기존 스킬 경로에 연결한다.
        public bool TryExecuteReaction(
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            SkillExecutionState snapshotRuntime,
            SkillDefinition definition,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            UnitCombatState eventTarget,
            Vector2 targetPoint,
            bool hasTargetPoint,
            bool hasRawDamageOverride,
            float rawDamageOverride,
            int recastGeneration,
            float damageMultiplier,
            string sourceSkillName,
            bool lockToEventTarget,
            bool publishSkillLifecycleEvents,
            bool beginCast,
            StatusApplicationSpec onHitStatusOverride = null,
            bool executeCastEffects = true)
        {
            if (entry == null
                || runtime == null
                || snapshotRuntime == null
                || definition == null)
            {
                return false;
            }

            var snapshot = entry.Model.SkillState.CreateExecutionData(
                entry.Model,
                snapshotRuntime,
                roster);
            if (!Mathf.Approximately(damageMultiplier, 1f))
            {
                snapshot.ScaleDamageMultiplier(damageMultiplier);
            }
            if (hasRawDamageOverride)
            {
                snapshot.SetRawDamageOverride(rawDamageOverride);
            }
            snapshot.OnHitStatusOverride = onHitStatusOverride;

            var aimDirection = entry.Transform != null && hasTargetPoint
                ? targetPoint - (Vector2)entry.Transform.position
                : default;
            return ExecutePrepared(
                entry,
                runtime,
                definition,
                snapshot,
                roster,
                combatManager,
                aimDirection.sqrMagnitude > 0.0001f,
                aimDirection,
                hasTargetPoint,
                targetPoint,
                beginCast,
                sourceSkillName,
                eventTarget,
                lockToEventTarget,
                publishSkillLifecycleEvents,
                recastGeneration,
                executeCastEffects);
        }

        /// 일반 입력을 학습이 반영된 실행값으로 바꿔 공통 시전 흐름에 넣는다.
        private bool TryExecuteSkill(
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            bool beginCast,
            float damageMultiplier,
            string triggerSourceSkillName)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            if (beginCast && !StatusCombatRules.CanAct(entry.Model))
            {
                return false;
            }

            var snapshot = entry.Model.SkillState.CreateExecutionData(entry.Model, runtime, roster);
            if (!Mathf.Approximately(damageMultiplier, 1f))
            {
                snapshot.ApplyDynamicDamageMultiplier(damageMultiplier);
            }

            return ExecutePrepared(
                entry,
                runtime,
                runtime.Data,
                snapshot,
                roster,
                combatManager,
                hasManualAimDirection,
                manualAimDirection,
                hasManualTargetPoint,
                manualTargetPoint,
                beginCast,
                triggerSourceSkillName);
        }

        /// 대상과 계열 입력을 확정한 뒤 성공한 시전만 상태와 사건에 반영한다.
        private bool ExecutePrepared(
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            SkillDefinition definition,
            SkillExecutionState snapshot,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            bool beginCast,
            string triggerSourceSkillName,
            UnitCombatState eventTarget = null,
            bool lockToEventTarget = false,
            bool publishSkillLifecycleEvents = true,
            int recastGeneration = 0,
            bool executeCastEffects = true)
        {
            if (beginCast && !CanCastWithData(runtime, snapshot))
            {
                return false;
            }

            var context = new SkillExecutionContext(
                combatManager,
                roster,
                entry,
                runtime,
                hasManualAimDirection: hasManualAimDirection,
                manualAimDirection: manualAimDirection,
                hasManualTargetPoint: hasManualTargetPoint,
                manualTargetPoint: manualTargetPoint,
                recastGeneration: recastGeneration,
                lockToEventTarget: lockToEventTarget,
                publishSkillLifecycleEvents: publishSkillLifecycleEvents,
                applyDamageMultiplierToShield: publishSkillLifecycleEvents,
                sourceSkillName: publishSkillLifecycleEvents
                    ? null
                    : triggerSourceSkillName,
                eventTarget: eventTarget);
            context.IsTrigger = snapshot.IsTrigger;
            if (lockToEventTarget
                && SkillTargeting.OrderedTargets(context, definition.Targeting).Count == 0)
            {
                return false;
            }
            if (definition is SingleSkillDefinition single
                && SkillExecutionRules.ShouldRejectCastForExecuteThreshold(context, snapshot, single))
            {
                return false;
            }
            if (!PrepareExecutionData(context, snapshot, definition))
            {
                return false;
            }
            if (!publishSkillLifecycleEvents
                && !string.IsNullOrWhiteSpace(triggerSourceSkillName))
            {
                snapshot.PreparedSkillName = triggerSourceSkillName;
            }
            var lifecycleCenter = hasManualTargetPoint
                ? manualTargetPoint
                : entry.Transform != null
                    ? (Vector2)entry.Transform.position
                    : Vector2.zero;
            if (publishSkillLifecycleEvents)
            {
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.BuildExecutionData,
                    new SkillExecutionContext(
                        entry.Model,
                        definition.SkillName,
                        eventTarget,
                        lifecycleCenter,
                        0f,
                        0,
                        snapshot,
                        context));
            }
            var routed = ExecuteSkill(context, snapshot, definition);
            if (routed)
            {
                if (beginCast && !TryBeginCast(runtime, snapshot))
                {
                    return false;
                }

                var monsterActor = entry.Actor as MonsterActor;
                if (beginCast && monsterActor != null)
                {
                    monsterActor.TryPlayActiveSkillAnimation();
                }

                if (executeCastEffects)
                {
                    ExecuteCastEffects(context, snapshot);
                }

                if (publishSkillLifecycleEvents)
                {
                    SkillTrigger.PublishLifecycleEvent(
                        SkillTriggerEvent.OnCast,
                        new SkillExecutionContext(
                            entry.Model,
                            definition.SkillName,
                            eventTarget,
                            lifecycleCenter,
                            0f,
                            0,
                            snapshot,
                            context));
                    NotifySkillCastTriggers(
                        combatManager,
                        roster,
                        entry,
                        runtime,
                        context,
                        snapshot,
                        triggerSourceSkillName);
                }
            }

            return routed;
        }

        /// 지속 효과를 전투 시작 흐름에 반영한다.
        public void ExecutePassiveEffects(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            UnitCombatState owner,
            bool enemyTargetsOnly = false)
        {
            var ownerEntry = roster != null ? roster.Find(owner) : null;
            if (combatManager == null
                || roster == null
                || ownerEntry == null
                || owner?.SkillState == null)
            {
                return;
            }

            var passives = owner.SkillState.PassiveSkills;
            for (var i = 0; i < passives.Count; i++)
            {
                var runtime = passives[i];
                if (runtime?.Data == null)
                {
                    continue;
                }

                var snapshot = owner.SkillState.CreateExecutionData(owner, runtime, roster);
                var context = new SkillExecutionContext(
                    combatManager,
                    roster,
                    ownerEntry,
                    runtime,
                    publishSkillLifecycleEvents: false,
                    sourceSkillName: runtime.SkillName);
                ExecuteCastEffects(context, snapshot, enemyTargetsOnly);
            }
        }

        /// 후속 효과도 일반 시전과 같은 준비와 계열 실행 경로로 되돌린다.
        internal bool TryExecuteResolvedEffect(
            CombatUnitEntry entry,
            SkillExecutionState sourceRuntime,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            SkillCastEffect effect,
            UnitCombatState eventTarget,
            Vector2 targetPoint,
            bool hasTargetPoint,
            int recastGeneration,
            string sourceSkillName,
            bool lockToEventTarget,
            float damageMultiplier,
            bool hasRawDamageOverride,
            float rawDamageOverride,
            SkillExecutionState sourceSnapshot = null,
            bool publishSkillLifecycleEvents = false,
            bool isTrigger = false)
        {
            if (entry?.Model == null
                || sourceRuntime == null
                || effect == null
                || effect.ResolvedDefinition == null)
            {
                return false;
            }

            sourceSnapshot ??= entry.Model.SkillState.CreateExecutionData(
                entry.Model,
                sourceRuntime,
                roster);
            sourceSnapshot.IsTrigger |= isTrigger;
            if (effect.IsRecast)
            {
                var zone = effect.ResolvedDefinition as ZoneSkillDefinition;
                if (zone == null
                    || recastGeneration >= Math.Max(1, effect.MaxGeneration))
                {
                    return false;
                }

                var recastContext = new SkillExecutionContext(
                    combatManager,
                    roster,
                    entry,
                    sourceRuntime,
                    eventTarget,
                    hasManualTargetPoint: hasTargetPoint,
                    manualTargetPoint: targetPoint,
                    recastGeneration: recastGeneration,
                    lockToEventTarget: lockToEventTarget,
                    publishSkillLifecycleEvents: publishSkillLifecycleEvents,
                    sourceSkillName: sourceSkillName);
                var recastSnapshot = effect.InheritSnapshot
                    ? sourceSnapshot
                    : SkillExecutionRules.CreateDefinitionSnapshot(zone);
                recastSnapshot.IsTrigger |= isTrigger;
                recastContext.IsTrigger = recastSnapshot.IsTrigger;
                return TryExecuteRecast(
                    recastContext,
                    recastSnapshot,
                    zone,
                    effect,
                    targetPoint);
            }

            var runtime = entry.Model.SkillState.FindByDefinition(
                effect.ResolvedDefinition);
            if (runtime == null)
            {
                runtime = new SkillExecutionState(entry.Model, effect.ResolvedDefinition);
            }

            var snapshot = runtime == sourceRuntime
                ? sourceSnapshot
                : SkillExecutionRules.BuildExecutionData(
                    entry.Model,
                    runtime,
                    roster);
            if (!Mathf.Approximately(damageMultiplier, 1f))
            {
                snapshot.ScaleDamageMultiplier(damageMultiplier);
            }
            if (hasRawDamageOverride)
            {
                snapshot.SetRawDamageOverride(rawDamageOverride);
            }
            snapshot.IsTrigger |= isTrigger;
            snapshot.OnHitStatusOverride = effect.OnHitStatusOverride;
            var aimDirection = entry.Transform != null && hasTargetPoint
                ? targetPoint - (Vector2)entry.Transform.position
                : default;
            return ExecutePrepared(
                entry,
                runtime,
                effect.ResolvedDefinition,
                snapshot,
                roster,
                combatManager,
                aimDirection.sqrMagnitude > 0.0001f,
                aimDirection,
                hasTargetPoint,
                targetPoint,
                false,
                sourceSkillName,
                eventTarget,
                lockToEventTarget,
                publishSkillLifecycleEvents,
                recastGeneration,
                false);
        }

        /// 시전 효과를 즉시 실행하거나 예약한다.
        private static void ExecuteCastEffects(
            SkillExecutionContext context,
            SkillExecutionState sourceSnapshot,
            bool enemyTargetsOnly = false)
        {
            if (context?.CombatManager == null || sourceSnapshot == null)
            {
                return;
            }

            var effects = SkillExecutionRules.ResolveCastEffects(
                sourceSnapshot,
                enemyTargetsOnly);
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null)
                {
                    continue;
                }

                if (effect.DelaySeconds > 0f)
                {
                    context.CombatManager.StartCoroutine(
                        ExecuteResolvedCastEffectDelayed(context, sourceSnapshot, effect));
                }
                else
                {
                    ExecuteResolvedCastEffect(context, sourceSnapshot, effect);
                }
            }
        }

        /// 예약된 시전 효과를 생존 조건 아래 실행한다.
    private static IEnumerator ExecuteResolvedCastEffectDelayed(
            SkillExecutionContext context,
            SkillExecutionState sourceSnapshot,
            SkillCastEffect effect)
        {
            yield return new WaitForSeconds(effect.DelaySeconds);
            if (context?.CasterEntry != null && context.CasterEntry.IsAlive)
            {
                ExecuteResolvedCastEffect(context, sourceSnapshot, effect);
            }
        }

        private static bool ExecuteResolvedCastEffect(
            SkillExecutionContext context,
            SkillExecutionState sourceSnapshot,
            SkillCastEffect effect,
            bool hasRawDamageOverride = false,
            float rawDamageOverride = 0f)
        {
            if (context == null || sourceSnapshot == null || effect == null)
            {
                return false;
            }

            if (effect.Command != null && effect.ResolvedDefinition == null)
            {
                var commandContext = new SkillTrigger.TriggerExecutionContext(
                    context.EventTarget,
                    context.Caster,
                    context.HasManualTargetPoint
                        ? context.ManualTargetPoint
                        : context.CasterEntry.Transform != null
                            ? (Vector2)context.CasterEntry.Transform.position
                            : Vector2.zero,
                    null,
                    0f,
                    0f,
                    DamageAttribute.Physical,
                    context.SourceSkillName,
                    context.Caster,
                    recastGeneration: context.RecastGeneration);
                return ApplyReactionCommand(
                    context.CombatManager,
                    context.Roster,
                    context.CasterEntry,
                    context.Runtime,
                    effect.Command,
                    commandContext);
            }

            var hasTargetPoint = context.HasManualTargetPoint;
            var targetPoint = context.ManualTargetPoint;
            if (effect.UseSourcePreparedAim
                && sourceSnapshot.PreparedDirections != null
                && sourceSnapshot.PreparedDirections.Count > 0
                && sourceSnapshot.PreparedDirections[0].sqrMagnitude > 0.0001f)
            {
                var origin = context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : Vector2.zero;
                targetPoint = origin + sourceSnapshot.PreparedDirections[0];
                hasTargetPoint = true;
            }
            if (effect.UseSourcePreparedCenter
                && sourceSnapshot.PreparedCenters != null
                && sourceSnapshot.PreparedCenters.Count > 0)
            {
                targetPoint = sourceSnapshot.PreparedCenters[0];
                hasTargetPoint = true;
            }

            return context.CombatManager.SkillExecution.TryExecuteResolvedEffect(
                context.CasterEntry,
                context.Runtime,
                context.Roster,
                context.CombatManager,
                effect,
                context.EventTarget,
                targetPoint,
                hasTargetPoint,
                context.RecastGeneration,
                effect.EffectName,
                false,
                effect.DamageMultiplier,
                hasRawDamageOverride,
                rawDamageOverride,
                sourceSnapshot: sourceSnapshot,
                isTrigger: sourceSnapshot.IsTrigger);
        }

        /// 시전 완료를 후속 반응에 알린다.
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry entry,
            SkillExecutionState runtime,
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            string triggerSourceSkillName = null)
        {
            if (context == null || context.IsTrigger)
            {
                return;
            }

            var center = Vector2.zero;
            if (entry.Transform != null)
            {
                center = entry.Transform.position;
            }
            if (context.HasManualTargetPoint)
            {
                center = context.ManualTargetPoint;
            }
            SkillTrigger.ExecuteSkillCast(
                combatManager,
                roster,
                entry.Model,
                runtime.Data.SkillName,
                center,
                triggerSourceSkillName,
                snapshot != null && snapshot.PreparedMagazineLastProjectile);
        }

        /// 확정된 실행값을 물리적 형태에 맞는 실행기로 보낸다.
        private static bool ExecuteSkill(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            SkillDefinition skillData)
        {
            switch (skillData.RuntimeKind)
            {
                case SkillRuntimeKind.MagazineProjectile:
                case SkillRuntimeKind.CooldownProjectile:
                    RequireDefinition<ProjectileSkillDefinition>(skillData);
                    return ProjectileSkillExecutor.Execute(context, snapshot);
                case SkillRuntimeKind.LineAttack:
                    RequireDefinition<LineSkillDefinition>(skillData);
                    return LineSkillExecutor.Execute(context, snapshot);
                case SkillRuntimeKind.SingleAttack:
                case SkillRuntimeKind.Mark:
                case SkillRuntimeKind.Execute:
                    RequireDefinition<SingleSkillDefinition>(skillData);
                    return SingleSkillExecutor.Execute(context, snapshot);
                case SkillRuntimeKind.AreaAttack:
                    if (skillData is SingleSkillDefinition)
                    {
                        return SingleSkillExecutor.Execute(context, snapshot);
                    }
                    RequireDefinition<ZoneSkillDefinition>(skillData);
                    return ZoneSkillExecutor.Execute(context, snapshot);
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                case SkillRuntimeKind.Heal:
                    RequireDefinition<BuffSkillDefinition>(skillData);
                    return BuffSkillExecutor.Execute(context, snapshot);
                default:
                    throw new InvalidOperationException(
                        "Unsupported skill runtime kind: " + skillData.RuntimeKind);
            }
        }

        /// 공통 실행값을 각 물리적 형태가 바로 사용할 입력으로 완성한다.
        private static bool PrepareExecutionData(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            SkillDefinition definition)
        {
            if (context == null || snapshot == null || definition == null)
            {
                return false;
            }

            snapshot.PreparedSkillName = definition.SkillName;
            switch (definition.RuntimeKind)
            {
                case SkillRuntimeKind.MagazineProjectile:
                case SkillRuntimeKind.CooldownProjectile:
                    return PrepareProjectileExecutionData(
                        context,
                        snapshot,
                        RequireDefinition<ProjectileSkillDefinition>(definition));
                case SkillRuntimeKind.LineAttack:
                    return PrepareLineExecutionData(
                        context,
                        snapshot,
                        RequireDefinition<LineSkillDefinition>(definition));
                case SkillRuntimeKind.SingleAttack:
                case SkillRuntimeKind.Mark:
                case SkillRuntimeKind.Execute:
                    return PrepareSingleExecutionData(
                        context,
                        snapshot,
                        RequireDefinition<SingleSkillDefinition>(definition));
                case SkillRuntimeKind.AreaAttack:
                    if (definition is SingleSkillDefinition areaSingle)
                    {
                        return PrepareSingleExecutionData(context, snapshot, areaSingle);
                    }
                    return PrepareZoneExecutionData(
                        context,
                        snapshot,
                        RequireDefinition<ZoneSkillDefinition>(definition),
                        1f,
                        0f,
                        null);
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Shield:
                case SkillRuntimeKind.Heal:
                    return PrepareBuffExecutionData(
                        context,
                        snapshot,
                        RequireDefinition<BuffSkillDefinition>(definition));
                default:
                    return false;
            }
        }

        /// 정의가 기대한 스킬 계열인지 확인한다.
        private static T RequireDefinition<T>(SkillDefinition definition)
            where T : SkillDefinition
        {
            if (definition is T typed)
            {
                return typed;
            }

            throw new InvalidOperationException(
                definition.RuntimeKind + " requires " + typeof(T).Name
                + ", got " + definition.GetType().Name);
        }

        /// 남은 재시전을 같은 실행 흐름으로 이어간다.
        internal bool TryExecuteRecast(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            ZoneSkillDefinition skill,
            SkillCastEffect effect,
            Vector2 center)
        {
            return PrepareZoneExecutionData(
                    context,
                    snapshot,
                    skill,
                    effect.RadiusMultiplier,
                    effect.DurationSeconds,
                    center)
                && ZoneSkillExecutor.Execute(context, snapshot);
        }

        /// 직선형 공격의 위치와 피해 입력을 준비한다.
        private static bool PrepareLineExecutionData(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            LineSkillDefinition skill)
        {
            var origin = context.CasterEntry.Transform != null
                ? (Vector2)context.CasterEntry.Transform.position
                : Vector2.zero;
            var repeatCount = Mathf.Max(1, skill.CastRepeatCount + snapshot.LineCastRepeatCountBonus);
            var directions = new List<Vector2>(repeatCount);
            if (context.HasManualAimDirection)
            {
                var direction = context.ManualAimDirection;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    return false;
                }

                direction.Normalize();
                for (var i = 0; i < repeatCount; i++)
                {
                    directions.Add(direction);
                }
            }
            else
            {
                var target = SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
                var primaryDirection = SkillTargeting.DirectionToTarget(origin, target);
                if (primaryDirection.sqrMagnitude <= 0.0001f || target == null || target.Transform == null)
                {
                    return false;
                }

                var centers = SkillTargeting.TargetAnchoredCenters(
                    context,
                    skill.Targeting,
                    target.Transform.position,
                    repeatCount,
                    false,
                    SkillDeploymentRepeatMode.RepeatNearest);
                for (var i = 0; i < centers.Count; i++)
                {
                    var direction = centers[i] - origin;
                    if (direction.sqrMagnitude > 0.0001f)
                    {
                        directions.Add(direction.normalized);
                    }
                }
            }

            if (directions.Count == 0)
            {
                return false;
            }

            var baseTickInterval = skill.Timing != null && skill.Timing.TickInterval > 0f
                ? skill.Timing.TickInterval
                : 0.1f;
            var tickInterval = Mathf.Max(
                0.05f,
                baseTickInterval * Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier));
            var baseDuration = skill.Timing != null && skill.Timing.ActiveDuration > 0f
                ? skill.Timing.ActiveDuration
                : tickInterval;

            snapshot.PreparedTargeting = skill.Targeting;
            snapshot.PreparedRuntimeVisual = skill.RuntimeVisual;
            snapshot.PreparedSkillEffectPrefab = snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : skill.SkillEffectPrefab;
            snapshot.PreparedOrigin = origin;
            snapshot.PreparedDirections = directions;
            snapshot.PreparedDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            snapshot.PreparedDamageAttribute = SkillExecutionRules.ResolveSkillAttribute(
                context.Caster,
                skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            snapshot.PreparedStatus = SkillExecutionRules.StatusSpec(
                snapshot.OnHitStatusOverride ?? skill.OnHitStatus,
                snapshot);
            snapshot.PreparedLength = Mathf.Max(0.1f, skill.LineLength);
            snapshot.PreparedWidth = Mathf.Max(
                0.1f,
                skill.LineWidth * Mathf.Max(0.01f, 1f + snapshot.BeamWidthBonus));
            snapshot.PreparedKnockbackDistance = Mathf.Max(
                0f,
                skill.KnockbackDistance * Mathf.Max(0f, snapshot.KnockbackDistanceMultiplier));
            snapshot.PreparedDuration = Mathf.Max(
                0.05f,
                baseDuration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus);
            snapshot.PreparedTickInterval = tickInterval;
            snapshot.PreparedRepeatInterval = Mathf.Max(0f, skill.CastRepeatIntervalSeconds);
            snapshot.PreparedCriticalAllowed =
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed;
            return true;
        }

        /// 영역형 공격의 중심과 지속 입력을 준비한다.
        private static bool PrepareZoneExecutionData(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            ZoneSkillDefinition skill,
            float recastRadiusMultiplier,
            float recastDuration,
            Vector2? recastCenter)
        {
            if (context == null || snapshot == null || skill == null)
            {
                return false;
            }

            var isRecast = recastCenter.HasValue;
            var baseRadius = SkillTargeting.BaseRadius(skill.Targeting, skill.Area);
            var radiusMultiplier = isRecast ? Mathf.Max(0f, recastRadiusMultiplier) : 1f;
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            IReadOnlyList<Vector2> centers;
            if (isRecast)
            {
                centers = new[] { recastCenter.Value };
            }
            else
            {
                var primaryCenter = SkillTargeting.AreaCenter(context, skill.Targeting, skill.Area);
                var deploymentCount = 1
                    + (snapshot.HasBranchCount ? Math.Max(0, snapshot.BranchCount) : 0);
                centers = SkillTargeting.TargetAnchoredCenters(
                    context,
                    skill.Targeting,
                    primaryCenter,
                    deploymentCount,
                    coverAll,
                    SkillDeploymentRepeatMode.RandomExisting);
            }

            var interval = skill.Area != null && skill.Area.TickInterval > 0f
                ? skill.Area.TickInterval
                : skill.Timing != null && skill.Timing.TickInterval > 0f
                    ? skill.Timing.TickInterval
                    : 1f;
            interval = Mathf.Max(0.05f, interval * Mathf.Max(0.05f, snapshot.ShotIntervalMultiplier));
            var duration = isRecast
                ? Mathf.Max(0.05f, recastDuration)
                : skill.Area != null && skill.Area.Duration > 0f
                    ? skill.Area.Duration
                    : skill.Timing != null
                        ? skill.Timing.ActiveDuration
                        : 0f;
            if (!isRecast)
            {
                if (duration <= 0f)
                {
                    duration = interval;
                }
                duration = duration * Mathf.Max(0f, snapshot.DurationMultiplier) + snapshot.DurationBonus;
            }

            snapshot.PreparedTargeting = skill.Targeting;
            snapshot.PreparedRuntimeVisual = skill.RuntimeVisual;
            snapshot.PreparedSkillEffectPrefab = snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : skill.SkillEffectPrefab;
            snapshot.PreparedCenters = centers;
            snapshot.PreparedDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            snapshot.PreparedDamageAttribute = SkillExecutionRules.ResolveSkillAttribute(
                context.Caster,
                skill.DamagePerTick != null ? skill.DamagePerTick.Element : skill.Element);
            snapshot.PreparedStatus = SkillExecutionRules.StatusSpec(skill.OnTickStatus, snapshot);
            snapshot.PreparedBaseRadius = baseRadius;
            snapshot.PreparedVisualRadiusMultiplier = radiusMultiplier;
            snapshot.PreparedDuration = Mathf.Max(0.05f, duration);
            snapshot.PreparedTickInterval = interval;
            snapshot.PreparedCriticalAllowed =
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed;
            snapshot.PreparedIsRecast = isRecast;
            snapshot.PreparedRecastGeneration = isRecast ? context.RecastGeneration + 1 : 0;
            return centers.Count > 0;
        }

        /// 투사체 공격의 방향과 충돌 입력을 준비한다.
        private static bool PrepareProjectileExecutionData(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            ProjectileSkillDefinition skill)
        {
            var origin = context.CasterEntry.Transform != null
                ? (Vector2)context.CasterEntry.Transform.position
                : Vector2.zero;
            var target = !context.HasManualAimDirection && !context.HasManualTargetPoint
                ? SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting)
                : null;
            var hasTargetPoint = context.HasManualTargetPoint;
            var targetPoint = context.ManualTargetPoint;
            if (!hasTargetPoint && target != null && target.Transform != null)
            {
                hasTargetPoint = true;
                targetPoint = target.Transform.position;
            }

            var direction = context.HasManualTargetPoint
                ? context.ManualTargetPoint - origin
                : context.HasManualAimDirection
                    ? context.ManualAimDirection
                    : SkillTargeting.DirectionToTarget(origin, target);
            if (direction.sqrMagnitude <= 0.0001f)
            {
                if (!context.HasManualAimDirection)
                {
                    return false;
                }
                direction = Vector2.right;
            }
            direction.Normalize();

            var projectile = skill.Projectile;
            var burstCount = projectile != null ? Math.Max(1, projectile.BurstProjectileCount) : 1;
            var burstIndex = context.Runtime != null
                ? CurrentBurstProjectileIndex(context.Runtime)
                : 1;
            var projectileCount = projectile != null ? Math.Max(1, projectile.ProjectilesPerShot) : 1;
            var pierce = projectile != null ? projectile.PierceCount : 0;
            if (burstCount <= 1)
            {
                projectileCount += snapshot.AdditionalProjectileBonus;
            }
            pierce = Math.Max(0, pierce + snapshot.PierceBonus);
            projectileCount = Math.Max(1, projectileCount);
            var speed = projectile != null ? projectile.ProjectileSpeed : 0f;
            var lifetime = projectile != null && projectile.LifetimeSeconds > 0f
                ? projectile.LifetimeSeconds
                : Mathf.Max(0.25f, 31f / Mathf.Max(0.1f, speed) + 0.5f);
            var directions = new List<Vector2>(projectileCount);
            var boundaries = new List<float>(projectileCount);
            for (var i = 0; i < projectileCount; i++)
            {
                var spreadDirection = ProjectileSpreadDirection(direction, i, projectileCount);
                directions.Add(spreadDirection);
                boundaries.Add(SkillExecutionRules.ProjectileDestroyBoundaryX(
                    origin,
                    spreadDirection,
                    speed,
                    lifetime));
            }

            var burstDamageMultiplier = 1f;
            if (projectile != null
                && projectile.BurstDamageMultiplier > 0f
                && MatchesProjectileIndex(
                    projectile.BurstDamageProjectileIndex,
                    burstIndex,
                    burstCount))
            {
                burstDamageMultiplier *= projectile.BurstDamageMultiplier;
            }
            burstDamageMultiplier *= SkillExecutionRules.BurstDamageMultiplier(
                snapshot,
                burstIndex,
                burstCount);

            var status = SkillExecutionRules.StatusSpec(skill.OnHitStatus, snapshot);
            var stacksBonus = SkillExecutionRules.BurstStatusStacksBonus(
                snapshot,
                burstIndex,
                burstCount);
            if (status != null && stacksBonus != 0)
            {
                status = CloneStatusWithStacks(status, Mathf.Max(1, status.Stacks + stacksBonus));
            }

            snapshot.PreparedTargeting = skill.Targeting;
            snapshot.PreparedRuntimeVisual = skill.RuntimeVisual;
            snapshot.PreparedOrigin = origin;
            snapshot.PreparedDirection = direction;
            snapshot.PreparedDirections = directions;
            snapshot.PreparedBoundaries = boundaries;
            snapshot.PreparedDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage);
            snapshot.PreparedDamageAttribute = SkillExecutionRules.ResolveSkillAttribute(
                context.Caster,
                skill.Damage != null ? skill.Damage.Element : skill.Element);
            snapshot.PreparedStatus = status;
            snapshot.PreparedCriticalAllowed = skill.Damage != null && skill.Damage.CriticalAllowed;
            snapshot.PreparedProjectileSpeed = speed;
            snapshot.PreparedPierceCount = pierce;
            snapshot.PreparedProjectileLifetime = lifetime;
            snapshot.PreparedHasProjectileTargetPoint = hasTargetPoint;
            snapshot.PreparedProjectileTargetPoint = targetPoint;
            snapshot.PreparedBurstProjectileCount = burstCount;
            snapshot.PreparedBurstProjectileIndex = burstIndex;
            snapshot.PreparedBurstDamageMultiplier = Mathf.Max(0f, burstDamageMultiplier);
            snapshot.PreparedMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            snapshot.PreparedMagazineFirstProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == context.Runtime.MaxMagazineSize;
            var followUpCount = snapshot.HasFollowUpProjectile
                && skill.RuntimeVisual != null
                && skill.RuntimeVisual.HasVisual()
                && burstIndex >= burstCount
                && (!snapshot.FollowUpProjectileFirstMagazineOnly
                    || snapshot.PreparedMagazineFirstProjectile)
                    ? Math.Max(1, snapshot.FollowUpProjectileCount)
                    : 0;
            var plannedLaunchCount = projectileCount + followUpCount;
            var branchChances = new List<float>(plannedLaunchCount);
            var branchCounts = new List<int>(plannedLaunchCount);
            var branchDamageMultipliers = new List<float>(plannedLaunchCount);
            var branchSearchRadii = new List<float>(plannedLaunchCount);
            for (var i = 0; i < plannedLaunchCount; i++)
            {
                var launchIndex = context.Runtime != null
                    ? (int)(((long)context.Runtime.ProjectileLaunchCount + i) % int.MaxValue) + 1
                    : 0;
                SkillExecutionRules.ResolveProjectileBranch(
                    snapshot,
                    launchIndex,
                    out var branchChance,
                    out var branchCount,
                    out var branchDamageMultiplier,
                    out var branchSearchRadius);
                branchChances.Add(branchChance);
                branchCounts.Add(branchCount);
                branchDamageMultipliers.Add(branchDamageMultiplier);
                branchSearchRadii.Add(branchSearchRadius);
            }
            snapshot.PreparedBranchChances = branchChances;
            snapshot.PreparedBranchCounts = branchCounts;
            snapshot.PreparedBranchDamageMultipliers = branchDamageMultipliers;
            snapshot.PreparedBranchSearchRadii = branchSearchRadii;
            snapshot.PreparedContactDamageEnabled = skill.ContactDamageEnabled;
            snapshot.PreparedArrivalDelay = Mathf.Max(
                0f,
                skill.ArrivalDelaySeconds * Mathf.Max(0f, snapshot.DamageDelayMultiplier));
            snapshot.PreparedArrivalSkill = skill.ArrivalSkill;
            return true;
        }

        /// 단일 공격의 대상과 적중 입력을 준비한다.
        private static bool PrepareSingleExecutionData(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            SingleSkillDefinition skill)
        {
            var primaryCenter = SkillTargeting.AreaCenter(context, skill.Targeting, skill.Area);
            var usesStatusFilteredDeployments =
                !string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusName);
            var usesResolvedDeployments = skill.UseMultiDeployment || usesStatusFilteredDeployments;
            var coverAll = (skill.Area != null && skill.Area.CoverAll)
                || (skill.Targeting != null && skill.Targeting.CoverAll);
            IReadOnlyList<Vector2> centers;
            if (usesStatusFilteredDeployments)
            {
                var targets = SkillTargeting.OrderedTargets(
                    context.CasterEntry,
                    context.Roster,
                    skill.Targeting,
                    skill.DeploymentRequiredTargetStatusKind,
                    Mathf.Max(1, skill.DeploymentRequiredTargetStatusMinStacks));
                var resolvedCenters = new List<Vector2>(targets.Count);
                for (var i = 0; i < targets.Count; i++)
                {
                    if (targets[i] != null && targets[i].Transform != null)
                    {
                        resolvedCenters.Add(targets[i].Transform.position);
                    }
                }
                centers = resolvedCenters;
            }
            else if (usesResolvedDeployments)
            {
                centers = SkillTargeting.TargetAnchoredCenters(
                    context,
                    skill.Targeting,
                    primaryCenter,
                    Mathf.Max(1, skill.DeploymentCount + snapshot.HitTargetCountBonus),
                    coverAll,
                    SkillDeploymentRepeatMode.RepeatNearest);
            }
            else
            {
                centers = new[] { primaryCenter };
            }

            var baseRadius = SkillTargeting.BaseRadius(skill.Targeting, skill.Area);
            var executeThresholdBonus = SkillExecutionRules.ResolveCastConditionHealthBonus(snapshot);

            snapshot.PreparedTargeting = skill.Targeting;
            snapshot.PreparedRuntimeVisual = skill.RuntimeVisual;
            snapshot.PreparedOrigin = context.CasterEntry.Transform != null
                ? (Vector2)context.CasterEntry.Transform.position
                : Vector2.zero;
            snapshot.PreparedCenters = centers;
            snapshot.PreparedBaseRadius = baseRadius;
            snapshot.PreparedRadius = SkillTargeting.Radius(
                baseRadius,
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus);
            snapshot.PreparedCoverAll = coverAll;
            snapshot.PreparedDamage = snapshot.HasRawDamageOverride
                ? snapshot.RawDamageOverride
                : DamageCalculator.CalculateRawDamage(context.Caster, skill.Damage);
            snapshot.PreparedDamageAttribute = SkillExecutionRules.ResolveSkillAttribute(
                context.Caster,
                skill.Damage != null ? skill.Damage.Element : skill.Element);
            snapshot.PreparedStatus = SkillExecutionRules.StatusSpec(skill.OnHitStatus, snapshot);
            snapshot.PreparedCriticalAllowed = skill.Damage != null && skill.Damage.CriticalAllowed;
            snapshot.PreparedHitTargetCount = skill.HitAllTargets || skill.HitTargetCount == int.MaxValue
                ? int.MaxValue
                : Mathf.Max(1, skill.HitTargetCount + snapshot.HitTargetCountBonus);
            snapshot.PreparedSkillEffectPrefab = snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : skill.SkillEffectPrefab;
            snapshot.PreparedUsePrefabHitbox = skill.UsePrefabHitbox;
            snapshot.PreparedUsesHitTargetCount = skill.UsesHitTargetCount;
            snapshot.PreparedUsesResolvedDeployments = usesResolvedDeployments;
            snapshot.PreparedPrefabHitboxAtOrigin = skill.HitAllTargets
                && !usesStatusFilteredDeployments;
            snapshot.PreparedTargetStatusStackStatusKind = skill.TargetStatusStackStatusKind;
            snapshot.PreparedTargetStatusStackMaxStacks = skill.TargetStatusStackMaxStacks;
            snapshot.PreparedTargetStatusStackDamage =
                DamageCalculator.CalculateRawDamage(context.Caster, skill.TargetStatusStackDamage);
            snapshot.PreparedTargetStatusStackDamageRateBonus =
                snapshot.TargetStatusStackDamageRateBonus(skill.TargetStatusStackStatusName);
            snapshot.PreparedConsumeTargetStatusKind = skill.ConsumeTargetStatusKind;
            snapshot.PreparedConsumeTargetStatusRatio = snapshot.HasConsumeTargetStatusRatioOverride
                ? snapshot.ConsumeTargetStatusRatioOverride
                : skill.ConsumeTargetStatusRatio;
            snapshot.PreparedConsumeTargetStatusStacks = snapshot.HasConsumeTargetStatusStacksOverride
                ? snapshot.ConsumeTargetStatusStacksOverride
                : skill.ConsumeTargetStatusStacks;
            snapshot.PreparedExecuteHealthRatioThreshold = Mathf.Clamp01(
                Mathf.Max(0f, skill.ExecuteHealthRatioThreshold) + executeThresholdBonus);
            snapshot.PreparedExecuteDamageMultiplier = skill.ExecuteDamageMultiplier;
            snapshot.PreparedKillCooldownRefundRatio = skill.KillCooldownRefundRatio;
            snapshot.PreparedBossDamageMultiplier = skill.BossDamageMultiplier;
            return centers.Count > 0 || !usesResolvedDeployments;
        }

        /// 지원 효과의 대상과 수치를 준비한다.
        private static bool PrepareBuffExecutionData(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            BuffSkillDefinition skill)
        {
            snapshot.PreparedBuffEffectKind = skill.EffectKind;
            snapshot.PreparedTargeting = skill.Targeting;
            snapshot.PreparedRuntimeVisual = skill.RuntimeVisual;
            snapshot.PreparedDamageAttribute = SkillExecutionRules.ResolveSkillAttribute(
                context.Caster,
                skill.Element);
            snapshot.PreparedTargets = skill.EffectKind == BuffEffectKind.Heal
                ? SkillTargeting.OrderedTargets(context, skill.Targeting)
                : SkillTargeting.BuffTargets(
                    context,
                    skill.Target,
                    skill.UseConfiguredTargeting,
                    skill.Targeting);
            snapshot.PreparedStatus = SkillExecutionRules.StatusSpec(skill.AttachedStatus, snapshot);
            snapshot.PreparedSkillEffectPrefab = snapshot.SkillEffectPrefab != null
                ? snapshot.SkillEffectPrefab
                : skill.SkillEffectPrefab;
            snapshot.PreparedAttachVisualToCaster = skill.AttachVisualToCaster;

            var healing = skill.Healing;
            if (skill.EffectKind == BuffEffectKind.Heal && healing != null)
            {
                var attack = context.Caster.Stats.AttackPower
                    * StatusCombatRules.AttackPowerMultiplier(context.Caster);
                var spell = context.Caster.Stats.SpellPower
                    * StatusCombatRules.SpellPowerMultiplier(context.Caster);
                snapshot.PreparedHealAmount = Mathf.Max(
                    0f,
                    healing.BaseDamage
                    + attack * healing.AttackPowerCoefficient
                    + spell * healing.SpellPowerCoefficient)
                    * context.Caster.SkillState.PassiveHealingMultiplier();
            }

            if (skill.EffectKind == BuffEffectKind.Shield)
            {
                var shieldStat = context.Caster.Stats.SpellPower
                    * StatusCombatRules.SpellPowerMultiplier(context.Caster);
                if (skill.ShieldStatSource == StatSource.Attack)
                {
                    shieldStat = context.Caster.Stats.AttackPower
                        * StatusCombatRules.AttackPowerMultiplier(context.Caster);
                }
                var shield = Mathf.Max(0f, skill.ShieldBase + shieldStat * skill.ShieldCoefficient);
                if (context.ApplyDamageMultiplierToShield)
                {
                    shield *= Mathf.Max(0f, snapshot.DamageMultiplier);
                }
                snapshot.PreparedShieldAmount =
                    shield * Mathf.Max(0f, snapshot.ShieldAmountMultiplier);
                snapshot.PreparedShieldTargetMaxHealthRatio =
                    Mathf.Max(0f, skill.ShieldTargetMaxHealthRatio);
                var shieldDuration = skill.ShieldDuration > 0f
                    ? skill.ShieldDuration
                    : skill.ShieldStatus != null
                        ? skill.ShieldStatus.Duration
                        : 0f;
                snapshot.PreparedShieldStatusData = SkillExecutionRules.StatusData(
                    skill.ShieldStatus,
                    StatusEffectKind.Shield,
                    snapshot);
                snapshot.PreparedDuration =
                    shieldDuration * Mathf.Max(0f, snapshot.DurationMultiplier)
                    + snapshot.DurationBonus
                    + snapshot.StatusDurationBonus(
                        snapshot.PreparedShieldStatusData.StatusTag);
            }
            snapshot.PreparedChargeTargetMaxHealthRatio =
                Mathf.Max(0f, skill.ChargeTargetMaxHealthRatio);
            snapshot.PreparedChargeRampSeconds = Mathf.Max(0f, skill.ChargeRampSeconds);
            snapshot.PreparedChargeMaxMoveSpeedMultiplier =
                Mathf.Max(1f, skill.ChargeMaxMoveSpeedMultiplier);
            return skill.EffectKind == BuffEffectKind.Charge
                || snapshot.PreparedTargets.Count > 0;
        }

        /// 분산 투사체의 방향을 정한다.
        private static Vector2 ProjectileSpreadDirection(Vector2 direction, int index, int count)
        {
            if (count <= 1)
            {
                return direction;
            }

            const float angleStep = 10f;
            var offset = (index - (count - 1) * 0.5f) * angleStep;
            var radians = offset * Mathf.Deg2Rad;
            var cos = Mathf.Cos(radians);
            var sin = Mathf.Sin(radians);
            return new Vector2(
                direction.x * cos - direction.y * sin,
                direction.x * sin + direction.y * cos).normalized;
        }

        /// 투사체 순번이 효과 조건에 맞는지 확인한다.
        private static bool MatchesProjectileIndex(
            int configuredIndex,
            int projectileIndex,
            int burstProjectileCount)
        {
            return configuredIndex == 0
                ? burstProjectileCount > 0 && projectileIndex == burstProjectileCount
                : configuredIndex > 0 && configuredIndex == projectileIndex;
        }

        /// 상태 적용값을 중첩 수치만 바꿔 복제한다.
        private static StatusApplicationSpec CloneStatusWithStacks(
            StatusApplicationSpec source,
            int stacks)
        {
            return new StatusApplicationSpec
            {
                Enabled = source.Enabled,
                RuntimeResolved = source.RuntimeResolved,
                Status = source.Status,
                Chance = source.Chance,
                Stacks = stacks,
                RuntimeDurationSeconds = source.RuntimeDurationSeconds,
                RuntimeMaxStacks = source.RuntimeMaxStacks,
                RuntimePermanent = source.RuntimePermanent,
                RefreshDuration = source.RefreshDuration,
                ThresholdSourceStatusKind = source.ThresholdSourceStatusKind,
                ThresholdSourceMinStacks = source.ThresholdSourceMinStacks,
                ThresholdStatus = source.ThresholdStatus
            };
        }

        /// 처치 결과가 허용한 대기 회복을 반영한다.
        internal static void HandleSingleKillRecovery(
            SkillExecutionState sourceRuntime,
            SkillExecutionState snapshot,
            InGameResourceChangeResult result,
            bool wasExecute)
        {
            if (sourceRuntime == null || !result.IsDead)
            {
                return;
            }

            SkillExecutionRules.ResolveKillRecovery(
                snapshot,
                wasExecute,
                out var resetCooldown,
                out var refundRatio);
            if (resetCooldown)
            {
                ResetCooldown(sourceRuntime);
                return;
            }
            if (refundRatio > 0f)
            {
                ReduceCooldownRemaining(
                    sourceRuntime,
                    sourceRuntime.EffectiveCooldownDuration * refundRatio);
            }
        }

        /// 적중 수가 만든 대기 환급을 반영한다.
        internal static void ApplyHitCountCooldownRefund(
            SkillExecutionState sourceRuntime,
            SkillExecutionState snapshot,
            int hitCount)
        {
            if (sourceRuntime?.Owner?.Skills == null
                || !SkillExecutionRules.ResolveHitCountCooldownRefund(
                    snapshot,
                    hitCount,
                    out var targetSkillName,
                    out var secondsRatio))
            {
                return;
            }

            var targetRuntime = sourceRuntime.Owner.SkillState.FindBySkillName(targetSkillName);
            if (targetRuntime != null)
            {
                ReduceCooldownRemaining(
                    targetRuntime,
                    targetRuntime.EffectiveCooldownDuration * secondsRatio);
            }
        }

        /// 통과한 반응을 지연과 반복 실행으로 연결한다.
        internal static void ScheduleReaction(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext,
            float resolvedRawDamage)
        {
            if (combatManager == null || roster == null || sourceEntry == null || source == null || trigger == null)
            {
                return;
            }

            var repeatCount = Mathf.Max(1, trigger.RepeatCount);
            for (var i = 0; i < repeatCount; i++)
            {
                var delaySeconds = Mathf.Max(0f, trigger.DelaySeconds)
                    + (i > 0 ? Mathf.Max(0f, trigger.RepeatIntervalSeconds) * i : 0f);
                if (delaySeconds <= 0f)
                {
                    RunScheduledReaction(
                        combatManager,
                        roster,
                        sourceEntry,
                        source,
                        trigger,
                        triggerContext,
                        resolvedRawDamage);
                }
                else
                {
                    combatManager.StartCoroutine(
                        RunScheduledReactionDelayed(
                            combatManager,
                            roster,
                            sourceEntry,
                            source,
                            trigger,
                            triggerContext,
                            delaySeconds,
                            resolvedRawDamage));
                }
            }
        }

        /// 예약된 반응을 지정 시점에 실행한다.
        private static IEnumerator RunScheduledReactionDelayed(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext,
            float delaySeconds,
            float resolvedRawDamage)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
            RunScheduledReaction(
                combatManager,
                roster,
                sourceEntry,
                source,
                trigger,
                triggerContext,
                resolvedRawDamage);
        }

        /// 반응 한 회분의 실행을 시작한다.
        private static void RunScheduledReaction(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext,
            float resolvedRawDamage)
        {
            ExecuteReactionOutcome(
                combatManager,
                roster,
                sourceEntry,
                source,
                trigger,
                triggerContext,
                resolvedRawDamage);
        }

        /// 반응 결과에 맞는 실행 경로를 선택한다.
        private static bool ExecuteReactionOutcome(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext,
            float resolvedRawDamage)
        {
            if (combatManager == null || roster == null || sourceEntry == null || source == null || trigger == null)
            {
                return false;
            }

            var sourceRuntime = source.SkillState.FindBySkillName(trigger.SourceSkillName);
            var targetPoint = triggerContext.EventCenter;
            if (trigger.CenterMode == SkillTriggerCenterMode.Caster && sourceEntry.Transform != null)
            {
                targetPoint = sourceEntry.Transform.position;
            }
            else if (trigger.CenterMode == SkillTriggerCenterMode.EventTarget && triggerContext.EventTarget != null)
            {
                var targetEntry = roster.Find(triggerContext.EventTarget);
                if (targetEntry != null && targetEntry.Transform != null)
                {
                    targetPoint = targetEntry.Transform.position;
                }
            }

            if (trigger.Effect != null)
            {
                var effect = trigger.Effect;
                var executionEntry = sourceEntry;
                if (effect.UseEventSourceSkill)
                {
                    var eventSource = triggerContext.EventSource ?? source;
                    sourceRuntime = eventSource?.SkillState?.FindBySkillName(triggerContext.EventSourceSkillName);
                    executionEntry = eventSource != null ? roster.Find(eventSource) ?? sourceEntry : sourceEntry;
                    if (sourceRuntime == null)
                    {
                        return false;
                    }

                    effect = new SkillCastEffect
                    {
                        EffectName = effect.EffectName,
                        UseEventSourceSkill = true,
                        DamageMultiplier = effect.DamageMultiplier,
                        ResolvedDefinition = sourceRuntime.Data,
                        UseSourcePreparedAim = effect.UseSourcePreparedAim,
                        UseSourcePreparedCenter = effect.UseSourcePreparedCenter,
                        OnHitStatusOverride = effect.OnHitStatusOverride,
                        Command = effect.Command,
                        IsRecast = effect.IsRecast,
                        RadiusMultiplier = effect.RadiusMultiplier,
                        DurationSeconds = effect.DurationSeconds,
                        InheritSnapshot = effect.InheritSnapshot,
                        MaxGeneration = effect.MaxGeneration
                    };
                }
                else if (sourceRuntime == null && effect.ResolvedDefinition != null)
                {
                    sourceRuntime = new SkillExecutionState(source, effect.ResolvedDefinition);
                }

                var executionSkillName = effect.UseEventSourceSkill
                    ? triggerContext.EventSourceSkillName
                    : trigger.SourceSkillName;

                if (effect.UseEventSourceSkill && sourceRuntime.UsesMagazine)
                {
                    var magazineShotCount = Mathf.Max(1, sourceRuntime.MaxMagazineSize);
                    combatManager.StartCoroutine(
                        RunAutomaticEncoreRecast(
                            combatManager,
                            roster,
                            executionEntry,
                            sourceRuntime,
                            effect,
                            triggerContext.EventTarget,
                            triggerContext.RecastGeneration,
                            executionSkillName,
                            trigger.LockToEventTarget,
                            trigger.DamageMultiplier,
                            trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed,
                            resolvedRawDamage,
                            magazineShotCount));
                    return true;
                }

                return sourceRuntime != null
                    && combatManager.SkillExecution.TryExecuteResolvedEffect(
                        executionEntry,
                        sourceRuntime,
                        roster,
                        combatManager,
                        effect,
                        triggerContext.EventTarget,
                        targetPoint,
                        !effect.UseEventSourceSkill,
                        triggerContext.RecastGeneration,
                        executionSkillName,
                        trigger.LockToEventTarget,
                        trigger.DamageMultiplier,
                        trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed,
                        resolvedRawDamage,
                        publishSkillLifecycleEvents: trigger.PublishSkillLifecycleEvents,
                        isTrigger: trigger.IsTrigger);
            }

            if (sourceRuntime == null && trigger.Command != null)
            {
                sourceRuntime = new SkillExecutionState(source, null);
            }

            return trigger.Command != null
                && ApplyReactionCommand(
                    combatManager,
                    roster,
                    sourceEntry,
                    sourceRuntime,
                    trigger.Command,
                    triggerContext);
        }

        private static IEnumerator RunAutomaticEncoreRecast(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry entry,
            SkillExecutionState sourceRuntime,
            SkillCastEffect effect,
            UnitCombatState eventTarget,
            int recastGeneration,
            string sourceSkillName,
            bool lockToEventTarget,
            float damageMultiplier,
            bool hasRawDamageOverride,
            float rawDamageOverride,
            int shotCount)
        {
            for (var shotIndex = 0; shotIndex < shotCount; shotIndex++)
            {
                if (combatManager == null
                    || roster == null
                    || entry == null
                    || !entry.IsAlive
                    || sourceRuntime == null)
                {
                    yield break;
                }

                Debug.Log(
                    $"[ChosenOne][Encore] recast skill={sourceSkillName} "
                    + $"shot={shotIndex + 1}/{shotCount} multiplier={damageMultiplier:0.00} "
                    + "autoTarget=true");

                var executed = combatManager.SkillExecution.TryExecuteResolvedEffect(
                    entry,
                    sourceRuntime,
                    roster,
                    combatManager,
                    effect,
                    eventTarget,
                    Vector2.zero,
                    false,
                    recastGeneration,
                    sourceSkillName,
                    lockToEventTarget,
                    damageMultiplier,
                    hasRawDamageOverride,
                    rawDamageOverride,
                    publishSkillLifecycleEvents: false,
                    isTrigger: true);
                if (!executed)
                {
                    Debug.Log(
                        $"[ChosenOne][Encore] recast stopped: skill={sourceSkillName} "
                        + $"shot={shotIndex + 1}/{shotCount} execution failed.");
                    yield break;
                }

                if (shotIndex + 1 < shotCount)
                {
                    var shotInterval = Mathf.Max(0f, sourceRuntime.effectiveTickInterval);
                    if (shotInterval > 0f)
                    {
                        yield return new WaitForSeconds(shotInterval);
                    }
                    else
                    {
                        yield return null;
                    }
                }
            }
        }

        /// 물리 효과가 아닌 반응 결과를 기존 상태 변경 경로로 전달한다.
        private static bool ApplyReactionCommand(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry source,
            SkillExecutionState sourceRuntime,
            SkillReactionCommand command,
            SkillTrigger.TriggerExecutionContext triggerContext)
        {
            if (command == null || sourceRuntime == null)
            {
                return false;
            }

            var context = new SkillExecutionContext(
                combatManager,
                roster,
                source,
                sourceRuntime,
                triggerContext.EventTarget,
                recastGeneration: triggerContext.RecastGeneration,
                lockToEventTarget: command.LockToEventTarget,
                publishSkillLifecycleEvents: false);
            var targets = SkillTargeting.OrderedTargets(context, command.Targeting);
            var limit = command.Targeting != null && command.Targeting.Shape == SkillTargetShape.Single
                ? 1
                : command.MaxTargets > 0 ? command.MaxTargets : targets.Count;
            var changed = false;
            for (var i = 0; i < targets.Count && i < limit; i++)
            {
                var target = targets[i] != null ? targets[i].Model : null;
                if (target == null)
                {
                    continue;
                }

                if (command.Kind == SkillReactionCommandKind.ExtendStatusDuration)
                {
                    changed |= combatManager.ExtendStatusDuration(
                        target,
                        command.StatusKind,
                        command.DurationSeconds);
                    continue;
                }
                if (target.Skills == null)
                {
                    continue;
                }

                var runtimes = string.IsNullOrWhiteSpace(command.TargetName)
                    ? target.SkillState.ActiveSkills
                    : target.SkillState.FindBySkillName(command.TargetName) is SkillExecutionState matchedRuntime
                        ? new[] { matchedRuntime }
                        : Array.Empty<SkillExecutionState>();
                for (var runtimeIndex = 0; runtimeIndex < runtimes.Count; runtimeIndex++)
                {
                    var targetRuntime = runtimes[runtimeIndex];
                    if (command.Kind == SkillReactionCommandKind.RefundCooldown)
                    {
                        changed |= ReduceCooldownRemaining(
                            targetRuntime,
                            targetRuntime.EffectiveCooldownDuration * command.Ratio);
                    }
                    else if (command.Kind == SkillReactionCommandKind.ReduceReload)
                    {
                        changed |= ReduceReloadRemaining(
                            targetRuntime,
                            targetRuntime.ReloadDuration * command.Ratio);
                    }
                }
            }
            return changed;
        }

    }

}
