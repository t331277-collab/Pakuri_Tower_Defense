/*
 * 스킬 사용 조건을 검증하고 확정된 효과를 알맞은 실행 경로로 전달한다.
 * 개별 스킬의 대기시간과 학습·선택 보정 상태도 함께 관리한다.
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
        private const int MaxTriggeredExecutionDepth = 8;
        private static int triggeredExecutionDepth;
        private static bool applyingHitEnhancement;

        private static readonly SkillSlot[] ActiveSlots =
        {
            SkillSlot.A,
            SkillSlot.B,
            SkillSlot.C,
            SkillSlot.D,
            SkillSlot.E
        };

        /// 자동 시전 후보를 외부 정책으로 선별한다.
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry, SkillExecutionData runtime);

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

                    TryExecuteSelected(entry, runtime, roster, combatManager);
                }
            }
        }

        /// 수동 조준을 실행 입력으로 바꾼다.
        public bool TryExecuteManual(
            CombatUnitEntry entry,
            SkillExecutionData runtime,
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
            SkillExecutionData runtime,
            UnitSpawnManager roster)
        {
            if (entry == null
                || runtime == null
                || !StatusCombatRules.CanAct(entry.Model))
            {
                return false;
            }

            var snapshot = entry.Model.SkillState.CreateExecutionData(entry.Model, runtime, roster);
            return runtime.CanCastWithData(snapshot);
        }

        /// 자동 조준으로 선택한 스킬을 실행 흐름에 올린다.
        public bool TryExecuteSelected(
            CombatUnitEntry entry,
            SkillExecutionData runtime,
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
            SkillExecutionData runtime,
            SkillExecutionData snapshotRuntime,
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
            string sourceSkillId,
            bool lockToEventTarget,
            bool publishSkillLifecycleEvents,
            bool beginCast,
            StatusApplicationSpec onHitStatusOverride = null,
            bool executeCastEffects = true)
        {
            if (entry == null
                || runtime == null
                || snapshotRuntime == null
                || definition == null
                || triggeredExecutionDepth >= MaxTriggeredExecutionDepth)
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
            try
            {
                triggeredExecutionDepth++;
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
                    sourceSkillId,
                    eventTarget,
                    lockToEventTarget,
                    publishSkillLifecycleEvents,
                    recastGeneration,
                    executeCastEffects);
            }
            finally
            {
                triggeredExecutionDepth--;
            }
        }

        /// 입력을 실행값으로 만들고 공통 실행으로 넘긴다.
        private bool TryExecuteSkill(
            CombatUnitEntry entry,
            SkillExecutionData runtime,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            bool beginCast,
            float damageMultiplier,
            string triggerSourceSkillId)
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
                triggerSourceSkillId);
        }

        /// 확정된 스킬을 검증하고 실행 단계로 통과시킨다.
        private bool ExecutePrepared(
            CombatUnitEntry entry,
            SkillExecutionData runtime,
            SkillDefinition definition,
            SkillExecutionData snapshot,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            bool beginCast,
            string triggerSourceSkillId,
            UnitCombatState eventTarget = null,
            bool lockToEventTarget = false,
            bool publishSkillLifecycleEvents = true,
            int recastGeneration = 0,
            bool executeCastEffects = true)
        {
            if (beginCast && !runtime.CanCastWithData(snapshot))
            {
                return false;
            }

            var context = new SkillActionContext(
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
                sourceSkillId: publishSkillLifecycleEvents
                    ? null
                    : triggerSourceSkillId,
                eventTarget: eventTarget);
            if (lockToEventTarget
                && SkillTargeting.OrderedTargets(context, definition.Targeting).Count == 0)
            {
                return false;
            }
            if (definition is SingleSkillDefinition single
                && SkillExecutionRuleResolver.ShouldRejectCastForExecuteThreshold(context, snapshot, single))
            {
                return false;
            }
            if (!PrepareExecutionData(context, snapshot, definition))
            {
                return false;
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
                    new SkillActionContext(
                        entry.Model,
                        definition.SkillId,
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
                if (beginCast && !runtime.TryBeginCast(snapshot))
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
                        new SkillActionContext(
                            entry.Model,
                            definition.SkillId,
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
                        triggerSourceSkillId);
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
                var context = new SkillActionContext(
                    combatManager,
                    roster,
                    ownerEntry,
                    runtime,
                    publishSkillLifecycleEvents: false,
                    sourceSkillId: runtime.SkillId);
                ExecuteCastEffects(context, snapshot, enemyTargetsOnly);
            }
        }

        /// 사건에서 파생된 단일 효과를 실행한다.
        internal bool TryExecuteReactionEffect(
            CombatUnitEntry entry,
            SkillExecutionData sourceRuntime,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            SkillCastEffect effect,
            UnitCombatState eventTarget,
            Vector2 targetPoint,
            int recastGeneration,
            string sourceSkillId,
            bool lockToEventTarget,
            float damageMultiplier,
            bool hasRawDamageOverride,
            float rawDamageOverride)
        {
            if (entry?.Model == null
                || sourceRuntime == null
                || effect == null
                || triggeredExecutionDepth >= MaxTriggeredExecutionDepth)
            {
                return false;
            }

            var snapshot = entry.Model.SkillState.CreateExecutionData(
                entry.Model,
                sourceRuntime,
                roster);
            if (!Mathf.Approximately(damageMultiplier, 1f))
            {
                snapshot.ScaleDamageMultiplier(damageMultiplier);
            }
            var context = new SkillActionContext(
                combatManager,
                roster,
                entry,
                sourceRuntime,
                eventTarget,
                hasManualTargetPoint: true,
                manualTargetPoint: targetPoint,
                recastGeneration: recastGeneration,
                lockToEventTarget: lockToEventTarget,
                publishSkillLifecycleEvents: false,
                sourceSkillId: sourceSkillId);
            try
            {
                triggeredExecutionDepth++;
                return ExecuteCastEffect(
                    context,
                    snapshot,
                    effect,
                    hasRawDamageOverride,
                    rawDamageOverride);
            }
            finally
            {
                triggeredExecutionDepth--;
            }
        }

        /// 시전 효과를 즉시 실행하거나 예약한다.
        private static void ExecuteCastEffects(
            SkillActionContext context,
            SkillExecutionData sourceSnapshot,
            bool enemyTargetsOnly = false)
        {
            if (context?.CombatManager == null || sourceSnapshot == null)
            {
                return;
            }

            var effects = SkillExecutionRuleResolver.ResolveCastEffects(
                sourceSnapshot,
                enemyTargetsOnly);
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                if (effect == null
                    || (enemyTargetsOnly
                        && effect.Targeting?.TargetSide != SkillTargetSide.Enemy))
                {
                    continue;
                }

                if (effect.DelaySeconds > 0f)
                {
                    context.CombatManager.StartCoroutine(
                        ExecuteCastEffectDelayed(context, sourceSnapshot, effect));
                }
                else
                {
                    ExecuteCastEffect(context, sourceSnapshot, effect);
                }
            }
        }

        /// 예약된 시전 효과를 생존 조건 아래 실행한다.
        private static IEnumerator ExecuteCastEffectDelayed(
            SkillActionContext context,
            SkillExecutionData sourceSnapshot,
            SkillCastEffect effect)
        {
            yield return new WaitForSeconds(effect.DelaySeconds);
            if (context?.CasterEntry != null && context.CasterEntry.IsAlive)
            {
                ExecuteCastEffect(context, sourceSnapshot, effect);
            }
        }

        /// 시전 효과의 대상과 실행 방식을 결정한다.
        private static bool ExecuteCastEffect(
            SkillActionContext context,
            SkillExecutionData sourceSnapshot,
            SkillCastEffect effect,
            bool hasRawDamageOverride = false,
            float rawDamageOverride = 0f)
        {
            if (context == null || sourceSnapshot == null || effect == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(effect.TargetSkillId))
            {
                var runtime = context.Caster?.SkillState.FindBySkillId(
                    effect.TargetSkillId);
                if (runtime?.Data == null)
                {
                    return false;
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

                return context.CombatManager.SkillExecution.TryExecuteReaction(
                    context.CasterEntry,
                    runtime,
                    runtime,
                    runtime.Data,
                    context.Roster,
                    context.CombatManager,
                    context.EventTarget,
                    targetPoint,
                    hasTargetPoint,
                    false,
                    0f,
                    context.RecastGeneration,
                    effect.DamageMultiplier,
                    effect.EffectId,
                    false,
                    false,
                    false,
                    effect.OnHitStatusOverride,
                    false);
            }

            var snapshot = sourceSnapshot.CopyWithDamageMultiplier(1f);
            snapshot.PreparedSkillId = effect.EffectId;
            snapshot.PreparedTargeting = effect.Targeting;
            snapshot.PreparedRuntimeVisual = effect.RuntimeVisual;
            snapshot.PreparedSkillEffectPrefab = effect.SkillEffectPrefab;

            if (effect.HasDamage)
            {
                var primaryCenter = effect.UseSourcePreparedCenter
                    && sourceSnapshot.PreparedCenters != null
                    && sourceSnapshot.PreparedCenters.Count > 0
                        ? sourceSnapshot.PreparedCenters[0]
                        : SkillTargeting.AreaCenter(
                            context,
                            effect.Targeting,
                            effect.Area);
                var baseRadius = SkillTargeting.BaseRadius(effect.Targeting, effect.Area);
                snapshot.PreparedCenters = new[] { primaryCenter };
                snapshot.PreparedBaseRadius = baseRadius;
                snapshot.PreparedRadius = SkillTargeting.Radius(
                    baseRadius,
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus);
                snapshot.PreparedCoverAll = effect.Targeting != null
                    && effect.Targeting.CoverAll;
                snapshot.PreparedDamage = hasRawDamageOverride
                    ? Mathf.Max(0f, rawDamageOverride)
                    : DamageCalculator.CalculateRawDamage(
                        context.Caster,
                        effect.Damage);
                snapshot.PreparedDamageAttribute = effect.Damage.Element;
                snapshot.PreparedStatus = SkillExecutionRuleResolver.StatusSpec(effect.Status, snapshot);
                snapshot.PreparedCriticalAllowed = effect.Damage.CriticalAllowed;
                snapshot.PreparedHitTargetCount = snapshot.PreparedCoverAll
                    ? int.MaxValue
                    : 1;
                snapshot.PreparedUsesHitTargetCount = true;
                return SingleSkillExecutor.Execute(context, snapshot);
            }

            var targets = SkillTargeting.BuffTargets(
                context,
                effect.Targeting != null
                    ? effect.Targeting.TargetSide
                    : SkillTargetSide.Self,
                true,
                effect.Targeting);
            snapshot.PreparedTargets = targets;
            if (effect.ExtendsStatus)
            {
                var changed = false;
                for (var i = 0; i < targets.Count; i++)
                {
                    var target = targets[i];
                    if (target?.Model != null)
                    {
                        changed |= context.CombatManager.ExtendStatusDuration(
                            target.Model,
                            effect.ExtendStatusKind,
                            effect.DurationSeconds);
                    }
                }
                return changed;
            }

            if (effect.HasShield)
            {
                var stat = effect.ShieldStatSource == StatSource.Attack
                    ? context.Caster.Stats.AttackPower
                        * StatusCombatRules.AttackPowerMultiplier(context.Caster)
                    : context.Caster.Stats.SpellPower
                        * StatusCombatRules.SpellPowerMultiplier(context.Caster);
                snapshot.PreparedBuffEffectKind = BuffEffectKind.Shield;
                snapshot.PreparedShieldAmount = Mathf.Max(
                    0f,
                    effect.ShieldBase + stat * effect.ShieldCoefficient);
                snapshot.PreparedDuration = Mathf.Max(0f, effect.DurationSeconds);
                snapshot.PreparedShieldStatusData = effect.ShieldStatus;
                return BuffSkillExecutor.Execute(context, snapshot);
            }

            if (effect.HasStatus)
            {
                snapshot.PreparedBuffEffectKind = BuffEffectKind.Status;
                snapshot.PreparedStatus = SkillExecutionRuleResolver.StatusSpec(effect.Status, snapshot);
                return BuffSkillExecutor.Execute(context, snapshot);
            }

            return false;
        }

        /// 시전 완료를 후속 반응에 알린다.
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry entry,
            SkillExecutionData runtime,
            SkillActionContext context,
            string triggerSourceSkillId = null)
        {
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
                runtime.Data.SkillId,
                center,
                triggerSourceSkillId);
        }

        /// 스킬 계열을 알맞은 실행기로 보낸다.
        private static bool ExecuteSkill(
            SkillActionContext context,
            SkillExecutionData snapshot,
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
                case SkillRuntimeKind.Field:
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

        /// 스킬 계열에 맞는 실행 입력을 완성한다.
        private static bool PrepareExecutionData(
            SkillActionContext context,
            SkillExecutionData snapshot,
            SkillDefinition definition)
        {
            if (context == null || snapshot == null || definition == null)
            {
                return false;
            }

            snapshot.PreparedSkillId = definition.SkillId;
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
                        null,
                        null);
                case SkillRuntimeKind.Field:
                    return PrepareZoneExecutionData(
                        context,
                        snapshot,
                        RequireDefinition<ZoneSkillDefinition>(definition),
                        null,
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
            SkillActionContext context,
            SkillExecutionData snapshot,
            ZoneSkillDefinition skill,
            SkillReactionCommand command,
            Vector2 center)
        {
            return PrepareZoneExecutionData(context, snapshot, skill, command, center)
                && ZoneSkillExecutor.Execute(context, snapshot);
        }

        /// 직선형 공격의 위치와 피해 입력을 준비한다.
        private static bool PrepareLineExecutionData(
            SkillActionContext context,
            SkillExecutionData snapshot,
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
            snapshot.PreparedDamageAttribute = skill.DamagePerTick != null
                ? skill.DamagePerTick.Element
                : skill.Element;
            snapshot.PreparedStatus = SkillExecutionRuleResolver.StatusSpec(
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
            SkillActionContext context,
            SkillExecutionData snapshot,
            ZoneSkillDefinition skill,
            SkillReactionCommand command,
            Vector2? recastCenter)
        {
            if (context == null || snapshot == null || skill == null)
            {
                return false;
            }

            var isRecast = command != null && recastCenter.HasValue;
            var baseRadius = SkillTargeting.BaseRadius(skill.Targeting, skill.Area);
            var radiusMultiplier = isRecast ? Mathf.Max(0f, command.RadiusMultiplier) : 1f;
            var radius = SkillTargeting.Radius(
                baseRadius,
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus) * radiusMultiplier;
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
                ? Mathf.Max(0.05f, command.DurationSeconds)
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
            snapshot.PreparedDamageAttribute = skill.DamagePerTick != null
                ? skill.DamagePerTick.Element
                : skill.Element;
            snapshot.PreparedStatus = SkillExecutionRuleResolver.StatusSpec(skill.OnTickStatus, snapshot);
            snapshot.PreparedBaseRadius = baseRadius;
            snapshot.PreparedVisualRadiusMultiplier = radiusMultiplier;
            snapshot.PreparedRadius = Mathf.Max(0f, radius);
            snapshot.PreparedCoverAll = coverAll;
            snapshot.PreparedDuration = Mathf.Max(0.05f, duration);
            snapshot.PreparedTickInterval = interval;
            snapshot.PreparedHitTargetCount = skill.HitAllTargets || !skill.UsesHitTargetCount
                ? int.MaxValue
                : Math.Max(1, Math.Max(1, skill.HitTargetCount) + snapshot.HitTargetCountBonus);
            snapshot.PreparedCriticalAllowed =
                skill.DamagePerTick != null && skill.DamagePerTick.CriticalAllowed;
            snapshot.PreparedIsRecast = isRecast;
            snapshot.PreparedRecastGeneration = isRecast ? context.RecastGeneration + 1 : 0;
            return centers.Count > 0;
        }

        /// 투사체 공격의 방향과 충돌 입력을 준비한다.
        private static bool PrepareProjectileExecutionData(
            SkillActionContext context,
            SkillExecutionData snapshot,
            ProjectileSkillDefinition skill)
        {
            var origin = context.CasterEntry.Transform != null
                ? (Vector2)context.CasterEntry.Transform.position
                : Vector2.zero;
            var target = context.HasManualAimDirection
                ? null
                : SkillTargeting.FindNearestTarget(context.CasterEntry, context.Roster, skill.Targeting);
            var direction = context.HasManualAimDirection
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
                ? context.Runtime.CurrentBurstProjectileIndex()
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
                boundaries.Add(SkillExecutionRuleResolver.ProjectileDestroyBoundaryX(
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
            burstDamageMultiplier *= SkillExecutionRuleResolver.BurstDamageMultiplier(
                snapshot,
                burstIndex,
                burstCount);

            var status = SkillExecutionRuleResolver.StatusSpec(skill.OnHitStatus, snapshot);
            var stacksBonus = SkillExecutionRuleResolver.BurstStatusStacksBonus(
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
            snapshot.PreparedDamageAttribute = skill.Damage != null
                ? skill.Damage.Element
                : skill.Element;
            snapshot.PreparedStatus = status;
            snapshot.PreparedCriticalAllowed = skill.Damage != null && skill.Damage.CriticalAllowed;
            snapshot.PreparedProjectileSpeed = speed;
            snapshot.PreparedPierceCount = pierce;
            snapshot.PreparedProjectileLifetime = lifetime;
            snapshot.PreparedBurstProjectileCount = burstCount;
            snapshot.PreparedBurstProjectileIndex = burstIndex;
            snapshot.PreparedBurstDamageMultiplier = Mathf.Max(0f, burstDamageMultiplier);
            snapshot.PreparedMagazineLastProjectile = context.Runtime != null
                && context.Runtime.UsesMagazine
                && context.Runtime.MagazineRemaining == 1;
            var followUpCount = snapshot.HasFollowUpProjectile
                && skill.RuntimeVisual != null
                && skill.RuntimeVisual.HasVisual()
                && burstIndex >= burstCount
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
                SkillExecutionRuleResolver.ResolveProjectileBranch(
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
            snapshot.PreparedImpactStatus = SkillExecutionRuleResolver.StatusSpec(skill.ImpactStatus, snapshot);
            snapshot.PreparedImpactRuntimeVisual = skill.ImpactRuntimeVisual;
            snapshot.PreparedImpactTargeting = new SkillTargetingSpec
            {
                TargetSide = SkillTargetSide.Enemy,
                Selection = SkillTargetSelection.Nearest,
                Shape = SkillTargetShape.Circle,
                Radius = snapshot.PreparedImpactRadius,
                CoverAll = false
            };
            snapshot.PreparedContactDamageEnabled = skill.ContactDamageEnabled;
            snapshot.PreparedStopOnFirstHit = skill.StopOnFirstHit;
            snapshot.PreparedImpactDelay = Mathf.Max(
                0f,
                skill.ImpactDelaySeconds * Mathf.Max(0f, snapshot.DamageDelayMultiplier));
            snapshot.PreparedHasImpactArea = skill.HasImpactArea;
            snapshot.PreparedImpactRadius = SkillTargeting.Radius(
                skill.ImpactArea != null ? skill.ImpactArea.Radius : 0f,
                snapshot.RadiusMultiplier,
                snapshot.RadiusBonus);
            snapshot.PreparedImpactTargeting.Radius = snapshot.PreparedImpactRadius;
            snapshot.PreparedImpactDamage = snapshot.PreparedDamage;
            return true;
        }

        /// 단일 공격의 대상과 적중 입력을 준비한다.
        private static bool PrepareSingleExecutionData(
            SkillActionContext context,
            SkillExecutionData snapshot,
            SingleSkillDefinition skill)
        {
            var primaryCenter = SkillTargeting.AreaCenter(context, skill.Targeting, skill.Area);
            var usesStatusFilteredDeployments =
                !string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId);
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
            var executeThresholdBonus = SkillExecutionRuleResolver.ResolveCastConditionHealthBonus(snapshot);

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
            snapshot.PreparedDamageAttribute = skill.Damage != null
                ? skill.Damage.Element
                : skill.Element;
            snapshot.PreparedStatus = SkillExecutionRuleResolver.StatusSpec(skill.OnHitStatus, snapshot);
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
            snapshot.PreparedDamageDelay = Mathf.Max(0f, skill.DamageDelaySeconds);
            snapshot.PreparedTargetStatusStackStatusKind = skill.TargetStatusStackStatusKind;
            snapshot.PreparedTargetStatusStackMaxStacks = skill.TargetStatusStackMaxStacks;
            snapshot.PreparedTargetStatusStackDamage =
                DamageCalculator.CalculateRawDamage(context.Caster, skill.TargetStatusStackDamage);
            snapshot.PreparedTargetStatusStackDamageRateBonus =
                snapshot.TargetStatusStackDamageRateBonus(skill.TargetStatusStackStatusId);
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
            SkillActionContext context,
            SkillExecutionData snapshot,
            BuffSkillDefinition skill)
        {
            snapshot.PreparedBuffEffectKind = skill.EffectKind;
            snapshot.PreparedTargeting = skill.Targeting;
            snapshot.PreparedRuntimeVisual = skill.RuntimeVisual;
            snapshot.PreparedDamageAttribute = skill.Element;
            snapshot.PreparedTargets = skill.EffectKind == BuffEffectKind.Heal
                ? SkillTargeting.OrderedTargets(context, skill.Targeting)
                : SkillTargeting.BuffTargets(
                    context,
                    skill.Target,
                    skill.UseConfiguredTargeting,
                    skill.Targeting);
            snapshot.PreparedStatus = SkillExecutionRuleResolver.StatusSpec(skill.AttachedStatus, snapshot);
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
                var shieldDuration = skill.ShieldDuration > 0f
                    ? skill.ShieldDuration
                    : skill.ShieldStatus != null
                        ? skill.ShieldStatus.Duration
                        : 0f;
                snapshot.PreparedDuration =
                    shieldDuration * Mathf.Max(0f, snapshot.DurationMultiplier)
                    + snapshot.DurationBonus;
                snapshot.PreparedShieldStatusData = SkillExecutionRuleResolver.StatusData(
                    skill.ShieldStatus,
                    StatusEffectKind.Shield,
                    snapshot);
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
            SkillExecutionData sourceRuntime,
            SkillExecutionData snapshot,
            InGameResourceChangeResult result,
            bool wasExecute)
        {
            if (sourceRuntime == null || !result.IsDead)
            {
                return;
            }

            SkillExecutionRuleResolver.ResolveKillRecovery(
                snapshot,
                wasExecute,
                out var resetCooldown,
                out var refundRatio);
            if (resetCooldown)
            {
                sourceRuntime.ResetCooldown();
                return;
            }
            if (refundRatio > 0f)
            {
                sourceRuntime.ReduceCooldownRemaining(sourceRuntime.EffectiveCooldownDuration * refundRatio);
            }
        }

        /// 적중 수가 만든 대기 환급을 반영한다.
        internal static void ApplyHitCountCooldownRefund(
            SkillExecutionData sourceRuntime,
            SkillExecutionData snapshot,
            int hitCount)
        {
            if (sourceRuntime?.Owner?.Skills == null
                || !SkillExecutionRuleResolver.ResolveHitCountCooldownRefund(
                    snapshot,
                    hitCount,
                    out var targetSkillId,
                    out var secondsRatio))
            {
                return;
            }

            var targetRuntime = sourceRuntime.Owner.SkillState.FindBySkillId(targetSkillId);
            targetRuntime?.ReduceCooldownRemaining(targetRuntime.EffectiveCooldownDuration * secondsRatio);
        }

        /// 범위 판정을 공통 적중 경로에 연결한다.
        internal static bool ApplyAreaHits(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager roster,
            SkillTargetingSpec targeting,
            Vector2 center,
            float radius,
            bool coverAll,
            float damage,
            DamageAttribute attribute,
            StatusApplicationSpec status,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData runtime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            int maxTargets,
            SkillExecutionData executionData)
        {
            if (manager == null || sourceEntry == null || roster == null)
            {
                return false;
            }

            if (!coverAll && radius <= 0f)
            {
                var target = SkillTargeting.FindNearestTarget(sourceEntry, roster, targeting);
                return ApplyResolvedHits(
                    manager,
                    sourceEntry,
                    roster,
                    target != null ? new[] { target } : Array.Empty<CombatUnitEntry>(),
                    1,
                    damage,
                    attribute,
                    status,
                    source,
                    sourceSkillId,
                    runtime,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    executionData);
            }

            var candidates = SkillTargeting.TargetList(sourceEntry, roster, targeting);
            var radiusSquared = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            var hitUnitIds = new HashSet<string>();
            var eligibleTargets = new List<CombatUnitEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                var unitId = target.Model.Identity != null ? target.Model.Identity.UnitId : null;
                if (!string.IsNullOrWhiteSpace(unitId) && !hitUnitIds.Add(unitId))
                {
                    continue;
                }
                if (!coverAll
                    && ((Vector2)target.Transform.position - center).sqrMagnitude > radiusSquared)
                {
                    continue;
                }

                eligibleTargets.Add(target);
            }

            return ApplyResolvedHits(
                manager,
                sourceEntry,
                roster,
                eligibleTargets,
                maxTargets,
                damage,
                attribute,
                status,
                source,
                sourceSkillId,
                runtime,
                criticalAllowed,
                critChanceBonus,
                critDamageBonus,
                executionData);
        }

        /// 선택된 대상에 공통 피해와 후속 처리를 적용한다.
        internal static bool ApplyResolvedHits(
            InGameCombatManager manager,
            CombatUnitEntry sourceEntry,
            UnitSpawnManager roster,
            IReadOnlyList<CombatUnitEntry> eligibleTargets,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            StatusApplicationSpec status,
            UnitCombatState source,
            string sourceSkillId,
            SkillExecutionData runtime,
            bool criticalAllowed,
            float critChanceBonus,
            float critDamageBonus,
            SkillExecutionData executionData)
        {
            if (manager == null || eligibleTargets == null || eligibleTargets.Count == 0)
            {
                return false;
            }

            var selectedTargets = new List<CombatUnitEntry>(eligibleTargets);
            if (maxTargets > 0 && maxTargets < selectedTargets.Count)
            {
                for (var i = 0; i < maxTargets; i++)
                {
                    var randomIndex = UnityEngine.Random.Range(i, selectedTargets.Count);
                    (selectedTargets[i], selectedTargets[randomIndex]) =
                        (selectedTargets[randomIndex], selectedTargets[i]);
                }
                selectedTargets.RemoveRange(maxTargets, selectedTargets.Count - maxTargets);
            }

            var routed = false;
            for (var i = 0; i < selectedTargets.Count; i++)
            {
                var target = selectedTargets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                var hitPosition = target.Transform != null
                    ? (Vector2)target.Transform.position
                    : Vector2.zero;
                var resolvedDamage = Mathf.Max(0f, damage);
                var finalDamageMultiplier = SkillExecutionRuleResolver.ResolveHitDamageMultiplier(
                    executionData,
                    target.Model);
                var result = manager.ApplyDamage(
                    target.Model,
                    resolvedDamage,
                    attribute,
                    source,
                    criticalAllowed,
                    critChanceBonus,
                    critDamageBonus,
                    sourceSkillId,
                    finalDamageMultiplier: finalDamageMultiplier);
                if (!result.IsDead)
                {
                    StatusCombatRules.ApplyStatus(manager, target.Model, status, source);
                }
                ApplyHitEnhancements(
                    manager,
                    runtime != null ? roster : null,
                    runtime,
                    executionData,
                    sourceEntry,
                    source,
                    sourceSkillId,
                    target,
                    hitPosition,
                    resolvedDamage);
                routed = true;
            }

            return routed;
        }

        /// 적중 사건과 후속 효과를 한 경로로 처리한다.
        internal static void ApplyHitEnhancements(
            InGameCombatManager manager,
            UnitSpawnManager roster,
            SkillExecutionData runtime,
            SkillExecutionData skillData,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            string sourceSkillId,
            CombatUnitEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage)
        {
            if (manager != null && roster != null && source != null && hitTarget != null && hitTarget.Model != null)
            {
                var actionExecutionContext = new SkillActionContext(
                    manager,
                    roster,
                    sourceEntry,
                    runtime,
                    hitTarget.Model,
                    publishSkillLifecycleEvents: runtime != null,
                    sourceSkillId: sourceSkillId);
                SkillTrigger.PublishLifecycleEvent(
                    SkillTriggerEvent.OnHit,
                    new SkillActionContext(
                        source,
                        sourceSkillId,
                        hitTarget.Model,
                        hitPosition,
                        primaryBaseDamage,
                        1,
                        skillData,
                        actionExecutionContext));
            }

            if (manager == null
                || roster == null
                || skillData == null
                || source == null
                || hitTarget == null
                || hitTarget.Model == null
                || primaryBaseDamage <= 0f
                || applyingHitEnhancement)
            {
                return;
            }

            var hasReloadReduction = !string.IsNullOrWhiteSpace(skillData.ReloadReduceTargetSkillId)
                && skillData.ReloadReduceSecondsPerHit > 0f;
            if (!skillData.HasOnHitAdditionalDamageBehavior && !hasReloadReduction)
            {
                return;
            }

            var hitIndex = runtime != null
                ? runtime.AdvanceSkillHitCount()
                : 0;

            applyingHitEnhancement = true;
            try
            {
                if (hasReloadReduction && runtime != null && runtime.Owner != null && runtime.Owner.Skills != null)
                {
                    var reloadSkill = runtime.Owner.SkillState.FindBySkillId(skillData.ReloadReduceTargetSkillId);
                    if (reloadSkill != null && reloadSkill.IsReloading)
                    {
                        reloadSkill.ReduceReloadRemaining(skillData.ReloadReduceSecondsPerHit);
                    }
                }

                var targetsHitUnit = string.IsNullOrWhiteSpace(skillData.OnHitAdditionalDamageTarget)
                    || string.Equals(skillData.OnHitAdditionalDamageTarget, "HitTarget", StringComparison.OrdinalIgnoreCase);
                if (skillData.HasOnHitAdditionalDamage
                    && skillData.OnHitAdditionalDamageMultiplier > 0f
                    && targetsHitUnit
                    && hitTarget.IsAlive
                    && UnityEngine.Random.value <= Mathf.Clamp01(skillData.OnHitAdditionalDamageChance))
                {
                    manager.ApplyDamage(
                        hitTarget.Model,
                        primaryBaseDamage,
                        skillData.OnHitAdditionalDamageAttribute,
                        source,
                        criticalAllowed: false,
                        0f,
                        0f,
                        sourceSkillId,
                        suppressOutgoingDamageTriggers: true,
                        finalDamageMultiplier: skillData.OnHitAdditionalDamageMultiplier);
                }

                if (skillData.HasOnHitChainDamageBehavior
                    && hitIndex > 0
                    && hitIndex % skillData.OnHitChainHitPeriod == 0)
                {
                    var chainTargets = SkillTargeting.ChainTargets(
                        roster,
                        sourceEntry,
                        source,
                        hitTarget,
                        hitPosition,
                        skillData.OnHitChainSearchRadius);
                    var targetCount = Mathf.Min(skillData.OnHitChainTargetCount, chainTargets.Count);
                    for (var i = 0; i < targetCount; i++)
                    {
                        var chainTarget = chainTargets[i];
                        if (chainTarget != null && chainTarget.IsAlive && chainTarget.Model != null)
                        {
                            manager.ApplyDamage(
                                chainTarget.Model,
                                primaryBaseDamage,
                                skillData.OnHitChainDamageAttribute,
                                source,
                                criticalAllowed: false,
                                0f,
                                0f,
                                sourceSkillId,
                                suppressOutgoingDamageTriggers: true,
                                finalDamageMultiplier: skillData.OnHitChainDamageMultiplier);
                        }
                    }
                }
            }
            finally
            {
                applyingHitEnhancement = false;
            }
        }

        /// 통과한 반응을 지연과 반복 실행으로 연결한다.
        internal static void ExecuteTriggeredReaction(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext)
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
                    ExecuteTriggeredReactionOnce(
                        combatManager,
                        roster,
                        sourceEntry,
                        source,
                        trigger,
                        triggerContext);
                }
                else
                {
                    combatManager.StartCoroutine(
                        ExecuteTriggeredReactionDelayed(
                            combatManager,
                            roster,
                            sourceEntry,
                            source,
                            trigger,
                            triggerContext,
                            delaySeconds));
                }
            }
        }

        /// 예약된 반응을 지정 시점에 실행한다.
        private static IEnumerator ExecuteTriggeredReactionDelayed(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext,
            float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
            ExecuteTriggeredReactionOnce(
                combatManager,
                roster,
                sourceEntry,
                source,
                trigger,
                triggerContext);
        }

        /// 반응 한 회분의 실행을 시작한다.
        private static void ExecuteTriggeredReactionOnce(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext)
        {
            ExecuteTriggeredOutcome(
                combatManager,
                roster,
                sourceEntry,
                source,
                trigger,
                triggerContext);
        }

        /// 반응 결과에 맞는 실행 경로를 선택한다.
        private static bool ExecuteTriggeredOutcome(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext triggerContext)
        {
            if (combatManager == null || roster == null || sourceEntry == null || source == null || trigger == null)
            {
                return false;
            }

            var sourceRuntime = source.SkillState.FindBySkillId(trigger.SourceSkillId);
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
                return sourceRuntime != null
                    && combatManager.SkillExecution.TryExecuteReactionEffect(
                        sourceEntry,
                        sourceRuntime,
                        roster,
                        combatManager,
                        trigger.Effect,
                        triggerContext.EventTarget,
                        targetPoint,
                        triggerContext.RecastGeneration,
                        trigger.SourceSkillId,
                        trigger.LockToEventTarget,
                        trigger.DamageMultiplier,
                        trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed,
                        ResolveTriggeredRawDamage(trigger, triggerContext));
            }

            if (!string.IsNullOrWhiteSpace(trigger.TargetSkillId))
            {
                if (sourceRuntime == null)
                {
                    return false;
                }

                var runtime = source.SkillState.FindBySkillId(trigger.TargetSkillId);
                if (runtime == null)
                {
                    return false;
                }

                var hasRawDamageOverride = trigger.DamageValueSource != SkillTriggerDamageValueSource.Fixed;
                var beginCast = runtime.Data is BuffSkillDefinition triggeredBuff
                    && triggeredBuff.EffectKind == BuffEffectKind.Charge;
                return combatManager.SkillExecution.TryExecuteReaction(
                    sourceEntry,
                    runtime,
                    runtime,
                    runtime.Data,
                    roster,
                    combatManager,
                    triggerContext.EventTarget,
                    targetPoint,
                    true,
                    hasRawDamageOverride,
                    ResolveTriggeredRawDamage(trigger, triggerContext),
                    triggerContext.RecastGeneration,
                    trigger.DamageMultiplier,
                    trigger.SourceSkillId,
                    trigger.LockToEventTarget,
                    trigger.PublishSkillLifecycleEvents,
                    beginCast);
            }

            return trigger.Command != null
                && ExecuteTriggeredCommand(
                    combatManager,
                    roster,
                    sourceEntry,
                    source,
                    sourceRuntime,
                    trigger.Command,
                    triggerContext);
        }

        /// 반응 command를 런타임 변화로 반영한다.
        private static bool ExecuteTriggeredCommand(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry source,
            UnitCombatState sourceState,
            SkillExecutionData sourceRuntime,
            SkillReactionCommand command,
            SkillTrigger.TriggerExecutionContext triggerContext)
        {
            if (command == null || sourceRuntime == null)
            {
                return false;
            }

            var context = new SkillActionContext(
                combatManager,
                roster,
                source,
                sourceRuntime,
                triggerContext.EventTarget,
                recastGeneration: triggerContext.RecastGeneration,
                lockToEventTarget: command.LockToEventTarget,
                publishSkillLifecycleEvents: false);
            if (command.Kind == SkillReactionCommandKind.RecastZone)
            {
                var skill = sourceRuntime.Data as ZoneSkillDefinition;
                if (skill == null
                    || (!string.IsNullOrWhiteSpace(command.TargetId)
                        && !string.Equals(command.TargetId, skill.SkillId, StringComparison.OrdinalIgnoreCase))
                    || context.RecastGeneration >= Math.Max(1, command.MaxGeneration))
                {
                    return false;
                }

                var inheritedSnapshot = sourceState.SkillState.CreateExecutionData(
                    sourceState,
                    sourceRuntime,
                    roster);
                var snapshot = command.InheritSnapshot
                    ? inheritedSnapshot
                    : SkillExecutionRuleResolver.CreateDefinitionSnapshot(skill);
                return combatManager.SkillExecution.TryExecuteRecast(
                    context,
                    snapshot,
                    skill,
                    command,
                    triggerContext.EventCenter);
            }

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

                var runtimes = CommandRuntimes(target, command.TargetId);
                for (var runtimeIndex = 0; runtimeIndex < runtimes.Count; runtimeIndex++)
                {
                    var targetRuntime = runtimes[runtimeIndex];
                    if (command.Kind == SkillReactionCommandKind.RefundCooldown)
                    {
                        changed |= targetRuntime.ReduceCooldownRemaining(
                            targetRuntime.EffectiveCooldownDuration * command.Ratio);
                    }
                    else if (command.Kind == SkillReactionCommandKind.ReduceReload)
                    {
                        changed |= targetRuntime.ReduceReloadRemaining(
                            targetRuntime.ReloadDuration * command.Ratio);
                    }
                }
            }
            return changed;
        }

        /// command가 가리키는 스킬 실행값을 모은다.
        private static IReadOnlyList<SkillExecutionData> CommandRuntimes(
            UnitCombatState target,
            string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                var runtime = target.SkillState.FindBySkillId(skillId);
                return runtime != null
                    ? new[] { runtime }
                    : Array.Empty<SkillExecutionData>();
            }
            return target.SkillState.ActiveSkills;
        }

        /// 사건 기반 반응 피해를 하나의 값으로 확정한다.
        private static float ResolveTriggeredRawDamage(
            SkillReaction trigger,
            SkillTrigger.TriggerExecutionContext context)
        {
            var value = 0f;
            switch (trigger.DamageValueSource)
            {
                case SkillTriggerDamageValueSource.ShieldAppliedAmount:
                    value = context.ShieldAppliedAmount;
                    break;
                case SkillTriggerDamageValueSource.ShieldRemainingAmount:
                    value = context.ShieldRemainingAmount;
                    break;
                case SkillTriggerDamageValueSource.ShieldAbsorbedAmount:
                    value = context.ShieldAbsorbedAmount;
                    break;
                case SkillTriggerDamageValueSource.TrackedIncomingDamage:
                    value = context.TrackedIncomingDamage(trigger.TrackedDamageAttribute);
                    break;
                case SkillTriggerDamageValueSource.EventAppliedDamage:
                    value = context.EventAppliedDamage;
                    break;
            }
            return Mathf.Max(0f, value) * Mathf.Max(0f, trigger.DamageValueMultiplier);
        }

        /// 학습 상태를 현재 정의와 동기화한다.
        public static void RebuildLearnedSkillState(UnitCombatState owner)
        {
            if (owner == null)
            {
                return;
            }

            string monsterId = null;
            if (owner.Identity != null)
            {
                monsterId = owner.Identity.DefinitionId;
            }
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                owner.SkillState.Clear();
                return;
            }

            var activeSkills = new List<SkillDefinition>();
            for (var i = 0; i < ActiveSlots.Length; i++)
            {
                var source = GameDataLoader.CurrentCatalog.GetActiveSkill(monsterId, ActiveSlots[i]);
                if (source != null)
                {
                    activeSkills.Add(source);
                }
            }

            RebuildLearnedSkillState(
                owner,
                activeSkills.ToArray(),
                GameDataLoader.CurrentCatalog.GetPassiveSkills(monsterId));
        }

        /// 전달된 정의로 학습 상태를 다시 구성한다.
        public static void RebuildLearnedSkillState(
            UnitCombatState owner,
            SkillDefinition[] activeDefinitions,
            PassiveSkillDefinition[] passiveDefinitions)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillState.Clear();
            if (owner.Skills == null)
            {
                return;
            }

            if (activeDefinitions != null)
            {
                for (var i = 0; i < activeDefinitions.Length; i++)
                {
                    var definition = activeDefinitions[i];
                    if (definition != null && owner.Skills.HasActiveSkill(definition.SkillId))
                    {
                        owner.SkillState.AddOrReplace(new SkillExecutionData(owner, definition));
                    }
                }
            }

            if (passiveDefinitions != null)
            {
                for (var i = 0; i < passiveDefinitions.Length; i++)
                {
                    var definition = passiveDefinitions[i];
                    if (definition != null && owner.Skills.HasPassiveSkill(definition.SkillId))
                    {
                        owner.SkillState.AddOrReplace(new SkillExecutionData(owner, definition));
                    }
                }
            }
        }
    }

}
