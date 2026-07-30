/*
 * 역할: 런타임 스킬 실행의 중앙 처리.
 * 책임: 시전 상태를 구성하고 전달 방식을 분배하며 결과 적용과 실행 상태 갱신을 소유한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// SkillExecutionContext 처리에 필요한 불변 실행 문맥을 전달한다.
    public class SkillExecutionContext
    {

        /// SkillExecutionContext 인스턴스를 전달된 런타임 입력값으로 초기화한다.
        public SkillExecutionContext(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry casterEntry,
            SkillUseState runtime,
            UnitCombatState eventTarget = null,
            bool hasManualAimDirection = false,
            Vector2 manualAimDirection = default,
            bool hasManualTargetPoint = false,
            Vector2 manualTargetPoint = default,
            int recastGeneration = 0,
            bool lockToEventTarget = false,
            bool publishSkillLifecycleEvents = true,
            bool applyDamageMultiplierToShield = true,
            string sourceSkillId = null)
        {
            CombatManager = combatManager;
            Roster = roster;
            CasterEntry = casterEntry;
            Runtime = runtime;
            EventTarget = eventTarget;
            HasManualAimDirection = hasManualAimDirection;
            ManualAimDirection = manualAimDirection;
            HasManualTargetPoint = hasManualTargetPoint;
            ManualTargetPoint = manualTargetPoint;
            RecastGeneration = Mathf.Max(0, recastGeneration);
            LockToEventTarget = lockToEventTarget;
            PublishSkillLifecycleEvents = publishSkillLifecycleEvents;
            ApplyDamageMultiplierToShield = applyDamageMultiplierToShield;
            SourceSkillId = string.IsNullOrWhiteSpace(sourceSkillId)
                ? runtime != null && runtime.Data != null
                    ? runtime.Data.SkillId
                    : string.Empty
                : sourceSkillId;
        }

        public InGameCombatManager CombatManager { get; }
        public UnitSpawnManager Roster { get; }
        public CombatUnitEntry CasterEntry { get; }
        public SkillUseState Runtime { get; }
        public UnitCombatState EventTarget { get; }
        public bool HasManualAimDirection { get; }
        public Vector2 ManualAimDirection { get; }
        public bool HasManualTargetPoint { get; }
        public Vector2 ManualTargetPoint { get; }
        public int RecastGeneration { get; }
        public bool LockToEventTarget { get; }
        public bool PublishSkillLifecycleEvents { get; }
        public bool ApplyDamageMultiplierToShield { get; }
        public string SourceSkillId { get; }

        public UnitCombatState Caster
        {
            get
            {
                if (CasterEntry == null)
                {
                    return null;
                }

                return CasterEntry.Model;
            }
        }
    }

    /// 확정된 스킬 시전을 조정하고 설정된 전달 경로로 실행을 분배한다.
    public class SkillExecution
    {
        private const int MaxTriggeredExecutionDepth = 8;
        private static int triggeredExecutionDepth;

        private static readonly SkillSlot[] ActiveSlots =
        {
            SkillSlot.A,
            SkillSlot.B,
            SkillSlot.C,
            SkillSlot.D,
            SkillSlot.E
        };

        /// SkillAutoRoutePredicate 사건을 전달하는 콜백 시그니처를 정의한다.
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry, SkillUseState runtime);

        /// 전달된 런타임 입력값을 사용해 ExecuteAutomaticSkills 작업을 시도하고 성공 여부를 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 ExecuteManual 작업을 시도하고 성공 여부를 반환한다.
        public bool TryExecuteManual(
            CombatUnitEntry entry,
            SkillUseState runtime,
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

        /// 전달된 런타임 입력값을 사용해 ExecuteSelected 실행 가능 여부를 반환한다.
        public bool CanExecuteSelected(
            CombatUnitEntry entry,
            SkillUseState runtime,
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

        /// 전달된 런타임 입력값을 사용해 ExecuteSelected 작업을 시도하고 성공 여부를 반환한다.
        public bool TryExecuteSelected(
            CombatUnitEntry entry,
            SkillUseState runtime,
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

        /// 전달된 런타임 입력값을 사용해 ExecuteTriggered 작업을 시도하고 성공 여부를 반환한다.
        public bool TryExecuteTriggered(
            CombatUnitEntry entry,
            SkillUseState sourceRuntime,
            SkillTriggerDefinition trigger,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            UnitCombatState eventTarget,
            Vector2 targetPoint,
            bool hasTargetPoint,
            bool hasRawDamageOverride,
            float rawDamageOverride,
            int recastGeneration)
        {
            if (entry == null
                || sourceRuntime == null
                || trigger == null
                || trigger.TriggeredSkill == null
                || triggeredExecutionDepth >= MaxTriggeredExecutionDepth)
            {
                return false;
            }

            var runtime = trigger.UsesExistingSkillRuntime
                ? entry.Model.SkillState.FindBySkillId(trigger.TriggeredSkill.SkillId)
                : new SkillUseState(entry.Model, trigger.TriggeredSkill);
            if (runtime == null)
            {
                return false;
            }

            var snapshotRuntime = trigger.UsesExistingSkillRuntime
                ? runtime
                : sourceRuntime;
            var snapshot = entry.Model.SkillState.CreateExecutionData(
                entry.Model,
                snapshotRuntime,
                roster);
            if (!Mathf.Approximately(trigger.TriggeredDamageMultiplier, 1f))
            {
                snapshot.ApplyDynamicDamageMultiplier(trigger.TriggeredDamageMultiplier);
            }
            if (hasRawDamageOverride)
            {
                snapshot.SetRawDamageOverride(rawDamageOverride);
            }

            var aimDirection = entry.Transform != null && hasTargetPoint
                ? targetPoint - (Vector2)entry.Transform.position
                : default;
            var beginTriggeredCast = trigger.UsesExistingSkillRuntime
                && trigger.TriggeredSkill is BuffSkillDefinition triggeredBuff
                && triggeredBuff.EffectKind == BuffEffectKind.Charge;
            try
            {
                triggeredExecutionDepth++;
                return ExecutePrepared(
                    entry,
                    runtime,
                    trigger.TriggeredSkill,
                    snapshot,
                    roster,
                    combatManager,
                    aimDirection.sqrMagnitude > 0.0001f,
                    aimDirection,
                    hasTargetPoint,
                    targetPoint,
                    beginTriggeredCast,
                    trigger.SourceSkillId,
                    eventTarget,
                    trigger.LockToEventTarget,
                    trigger.PublishSkillLifecycleEvents,
                    recastGeneration);
            }
            finally
            {
                triggeredExecutionDepth--;
            }
        }

        /// 전달된 런타임 입력값을 사용해 ExecuteSkill 작업을 시도하고 성공 여부를 반환한다.
        private bool TryExecuteSkill(
            CombatUnitEntry entry,
            SkillUseState runtime,
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

        /// 전달된 런타임 입력값을 사용해 Prepared를 실행한다.
        private bool ExecutePrepared(
            CombatUnitEntry entry,
            SkillUseState runtime,
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
            int recastGeneration = 0)
        {
            if (beginCast && !runtime.CanCastWithData(snapshot))
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
                && SingleSkillRules.ShouldRejectCastForExecuteThreshold(context, snapshot, single))
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

        /// 전달된 런타임 입력값을 사용해 SkillCastTriggers를 관련 런타임 시스템에 알린다.
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager,
            UnitSpawnManager roster,
            CombatUnitEntry entry,
            SkillUseState runtime,
            SkillExecutionContext context,
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

        /// 전달된 런타임 입력값을 사용해 Skill를 실행한다.
        private static bool ExecuteSkill(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            SkillDefinition skillData)
        {

            if (skillData is ProjectileSkillDefinition projectile)
            {
                return ProjectileSkillExecutor.Execute(context, snapshot, projectile);
            }

            if (skillData is LineSkillDefinition line)
            {
                return LineSkillExecutor.Execute(context, snapshot);
            }

            if (skillData is SingleSkillDefinition single)
            {
                return SingleSkillExecutor.Execute(context, snapshot, single);
            }

            if (skillData is ZoneSkillDefinition zone)
            {
                return ZoneSkillExecutor.Execute(context, snapshot);
            }

            if (skillData is BuffSkillDefinition buff)
            {
                return BuffSkillExecutor.Execute(context, snapshot, buff);
            }

            throw new InvalidOperationException("Unsupported compiled skill data: " + skillData.GetType().Name);
        }

        private static bool PrepareExecutionData(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            SkillDefinition definition)
        {
            if (context == null || snapshot == null || definition == null)
            {
                return false;
            }

            if (definition is LineSkillDefinition line)
            {
                return PrepareLineExecutionData(context, snapshot, line);
            }
            if (definition is ZoneSkillDefinition zone)
            {
                return PrepareZoneExecutionData(context, snapshot, zone, null, null);
            }

            return true;
        }

        internal bool TryExecuteRecast(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ZoneSkillDefinition skill,
            SkillTriggerCommand command,
            Vector2 center)
        {
            return PrepareZoneExecutionData(context, snapshot, skill, command, center)
                && ZoneSkillExecutor.Execute(context, snapshot);
        }

        private static bool PrepareLineExecutionData(
            SkillExecutionContext context,
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
            snapshot.PreparedOrigin = origin;
            snapshot.PreparedDirections = directions;
            snapshot.PreparedDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            snapshot.PreparedDamageAttribute = skill.DamagePerTick != null
                ? skill.DamagePerTick.Element
                : skill.Element;
            snapshot.PreparedStatus = SkillStatus.StatusSpec(skill.OnHitStatus, snapshot);
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

        private static bool PrepareZoneExecutionData(
            SkillExecutionContext context,
            SkillExecutionData snapshot,
            ZoneSkillDefinition skill,
            SkillTriggerCommand command,
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
            snapshot.PreparedCenters = centers;
            snapshot.PreparedDamage = DamageCalculator.CalculateRawDamage(context.Caster, skill.DamagePerTick);
            snapshot.PreparedDamageAttribute = skill.DamagePerTick != null
                ? skill.DamagePerTick.Element
                : skill.Element;
            snapshot.PreparedStatus = SkillStatus.StatusSpec(skill.OnTickStatus, snapshot);
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

        /// 전달된 owner 값을 사용해 RebuildLearnedSkillState 작업을 수행한다.
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

        /// 전달된 런타임 입력값을 사용해 RebuildLearnedSkillState 작업을 수행한다.
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
                        owner.SkillState.AddOrReplace(new SkillUseState(owner, definition));
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
                        owner.SkillState.AddOrReplace(new SkillUseState(owner, definition));
                    }
                }
            }
        }
    }

}

namespace Pakuri.InGame
{

    /// SkillUseState의 변경 가능한 런타임 상태를 보관한다.
    public class SkillUseState
    {

        /// SkillUseState 인스턴스를 전달된 런타임 입력값으로 초기화한다.
        public SkillUseState(UnitCombatState owner, SkillDefinition data)
        {
            Owner = owner;
            Data = data;
            ResetRuntimeState();
        }

        public UnitCombatState Owner { get; }
        public SkillDefinition Data { get; }
        public string SkillId => Data.SkillId;
        public SkillSlot Slot => Data.Slot;
        public float CooldownRemaining { get; private set; }
        public float CastRemaining { get; private set; }
        public float ActiveDurationRemaining { get; private set; }
        public float TickRemaining { get; private set; }
        public float ReloadRemaining { get; private set; }
        public int MagazineRemaining { get; private set; }
        public int ProjectileLaunchCount { get; private set; }
        public int SkillHitCount { get; private set; }

        private int effectiveMaxMagazineSize;
        private int effectiveBurstProjectileCount;
        private float effectiveReloadDuration;
        private float effectiveTickInterval;
        private float effectiveBurstInterval;
        private float effectiveCooldownDuration;
        private int queuedBurstShotsRemaining;
        private string consecutiveHitTargetUnitId;
        private int consecutiveHitRepeatCount;

        public bool IsCasting => CastRemaining > 0f;
        public bool IsActive => ActiveDurationRemaining > 0f;
        public bool IsReloading => ReloadRemaining > 0f;
        public bool IsBursting => queuedBurstShotsRemaining > 0;
        public int MaxMagazineSize => effectiveMaxMagazineSize;
        public float ReloadDuration => effectiveReloadDuration;
        public float EffectiveCooldownDuration => effectiveCooldownDuration;
        public int EffectiveBurstProjectileCount => effectiveBurstProjectileCount;
        public bool UsesMagazine => MaxMagazineSize > 0;
        public bool HasMagazine => !UsesMagazine || MagazineRemaining > 0;

        public bool CanCast => CanCastWithData(null);

        /// RuntimeState를 초기 런타임 상태로 되돌린다.
        public void ResetRuntimeState()
        {
            effectiveMaxMagazineSize = CalculateMaxMagazineSize(Data);
            effectiveBurstProjectileCount = BurstProjectileCount(Data);
            effectiveReloadDuration = CalculateReloadDuration(Data);
            effectiveTickInterval = TickInterval(Data);
            effectiveBurstInterval = BurstInterval(Data);
            effectiveCooldownDuration = CooldownDuration(Data);
            CooldownRemaining = 0f;
            CastRemaining = 0f;
            ActiveDurationRemaining = 0f;
            TickRemaining = 0f;
            ReloadRemaining = 0f;
            MagazineRemaining = MaxMagazineSize;
            queuedBurstShotsRemaining = 0;
            ProjectileLaunchCount = 0;
            SkillHitCount = 0;
            consecutiveHitTargetUnitId = string.Empty;
            consecutiveHitRepeatCount = 0;
        }

        /// AdvanceProjectileLaunchCount 결과값을 생성해 반환한다.
        public int AdvanceProjectileLaunchCount()
        {
            if (ProjectileLaunchCount == int.MaxValue)
            {
                ProjectileLaunchCount = 0;
            }

            ProjectileLaunchCount++;
            return ProjectileLaunchCount;
        }

        /// AdvanceSkillHitCount 결과값을 생성해 반환한다.
        public int AdvanceSkillHitCount()
        {
            if (SkillHitCount == int.MaxValue)
            {
                SkillHitCount = 0;
            }

            SkillHitCount++;
            return SkillHitCount;
        }

        /// 전달된 런타임 입력값을 사용해 ConsecutiveHitDamageMultiplier 결과값을 생성해 반환한다.
        public float ConsecutiveHitDamageMultiplier(UnitCombatState target, SkillExecutionData snapshot)
        {
            if (target == null)
            {
                return 1f;
            }

            var projectileData = Data as ProjectileSkillDefinition;
            var bonusRate = 0f;
            var bonusMax = 0f;
            if (projectileData != null)
            {
                bonusRate = projectileData.ConsecutiveHitBonusRate;
                bonusMax = projectileData.ConsecutiveHitMax;
            }
            if (snapshot != null && snapshot.ConsecutiveHitBonusRate > 0f)
            {
                bonusRate = snapshot.ConsecutiveHitBonusRate;
            }
            if (snapshot != null && snapshot.ConsecutiveHitMax > 0f)
            {
                bonusMax = snapshot.ConsecutiveHitMax;
            }
            if (bonusRate <= 0f || bonusMax <= 0f)
            {
                return 1f;
            }

            var unitId = string.Empty;
            if (target.Identity != null)
            {
                unitId = target.Identity.UnitId;
            }
            if (string.IsNullOrWhiteSpace(unitId))
            {
                consecutiveHitTargetUnitId = string.Empty;
                consecutiveHitRepeatCount = 0;
                return 1f;
            }

            if (string.Equals(consecutiveHitTargetUnitId, unitId, StringComparison.Ordinal))
            {
                consecutiveHitRepeatCount = Math.Min(consecutiveHitRepeatCount + 1, int.MaxValue - 1);
            }
            else
            {
                consecutiveHitTargetUnitId = unitId;
                consecutiveHitRepeatCount = 0;
            }

            var bonus = Mathf.Min(
                Mathf.Max(0f, bonusMax),
                Mathf.Max(0f, bonusRate) * consecutiveHitRepeatCount);
            return 1f + bonus;
        }

        /// 전달된 deltaTime 값을 사용해 요청값를 경과 시간 기준으로 갱신한다.
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            var actionDeltaTime = deltaTime * StatusCombatRules.ActionSpeedMultiplier(Owner);
            CooldownRemaining = TickDown(CooldownRemaining, actionDeltaTime);
            CastRemaining = TickDown(CastRemaining, actionDeltaTime);
            ActiveDurationRemaining = TickDown(ActiveDurationRemaining, deltaTime);
            TickRemaining = TickDown(TickRemaining, actionDeltaTime);
            ReloadRemaining = TickDown(ReloadRemaining, deltaTime);

            if (UsesMagazine
                && MagazineRemaining <= 0
                && ReloadRemaining <= 0f
                && CooldownRemaining <= 0f
                && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /// 전달된 snapshot 값을 사용해 CastWithData 실행 가능 여부를 반환한다.
        public bool CanCastWithData(SkillExecutionData snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            if (Data == null
                || !Data.IsActive
                || IsCasting
                || !IsCastIntervalReady())
            {
                return false;
            }

            if (IsBursting)
            {
                return !IsReloading;
            }

            return CooldownRemaining <= 0f
                && !IsReloading
                && HasMagazine;
        }

        /// BeginCast 작업을 시도하고 성공 여부를 반환한다.
        public bool TryBeginCast()
        {
            return TryBeginCast(null);
        }

        /// 전달된 snapshot 값을 사용해 BeginCast 작업을 시도하고 성공 여부를 반환한다.
        public bool TryBeginCast(SkillExecutionData snapshot)
        {
            RefreshRuntimeModifiers(snapshot);
            if (IsBursting)
            {
                queuedBurstShotsRemaining = Math.Max(0, queuedBurstShotsRemaining - 1);
                if (IsBursting)
                {
                    TickRemaining = effectiveBurstInterval;
                }
                else
                {
                    TickRemaining = effectiveTickInterval;
                    BeginRecoveryIfNeeded();
                }

                return true;
            }

            if (!CanCastWithData(snapshot))
            {
                return false;
            }

            if (UsesMagazine)
            {
                MagazineRemaining = Math.Max(0, MagazineRemaining - 1);
            }

            var timing = Data.Timing;
            ActiveDurationRemaining = Mathf.Max(0f, timing.ActiveDuration);
            queuedBurstShotsRemaining = Math.Max(0, effectiveBurstProjectileCount - 1);
            TickRemaining = effectiveTickInterval;
            if (IsBursting)
            {
                TickRemaining = effectiveBurstInterval;
            }

            if (!IsBursting)
            {
                BeginRecoveryIfNeeded();
            }

            return true;
        }

        /// 진행 중인 ActiveDuration을 종료한다.
        public void StopActive()
        {
            ActiveDurationRemaining = 0f;
        }

        /// TickReady 조건 충족 여부를 반환한다.
        public bool IsTickReady()
        {
            return Data.Timing.TickInterval > 0f && TickRemaining <= 0f;
        }

        /// TickInterval를 초기 런타임 상태로 되돌린다.
        public void ResetTickInterval()
        {
            TickRemaining = effectiveTickInterval;
        }

        /// CurrentBurstProjectileIndex 결과값을 생성해 반환한다.
        public int CurrentBurstProjectileIndex()
        {
            if (effectiveBurstProjectileCount <= 1 || !IsBursting)
            {
                return 1;
            }

            return Mathf.Clamp(
                effectiveBurstProjectileCount - queuedBurstShotsRemaining + 1,
                1,
                effectiveBurstProjectileCount);
        }

        /// 전달된 seconds 값을 사용해 ReduceReloadRemaining 조건을 평가하고 결과를 반환한다.
        public bool ReduceReloadRemaining(float seconds)
        {
            if (seconds <= 0f || ReloadRemaining <= 0f)
            {
                return false;
            }

            ReloadRemaining = Mathf.Max(0f, ReloadRemaining - seconds);
            if (ReloadRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && CooldownRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }

            return true;
        }

        /// 전달된 seconds 값을 사용해 ReduceCooldownRemaining 조건을 평가하고 결과를 반환한다.
        public bool ReduceCooldownRemaining(float seconds)
        {
            if (seconds <= 0f || CooldownRemaining <= 0f)
            {
                return false;
            }

            CooldownRemaining = Mathf.Max(0f, CooldownRemaining - seconds);
            if (CooldownRemaining <= 0f && UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }

            return true;
        }

        /// Cooldown를 초기 런타임 상태로 되돌린다.
        public void ResetCooldown()
        {
            CooldownRemaining = 0f;
            if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /// 전달된 런타임 입력값을 사용해 Down를 경과 시간 기준으로 갱신한다.
        private static float TickDown(float value, float deltaTime)
        {
            if (value > 0f)
            {
                return Mathf.Max(0f, value - deltaTime);
            }

            return 0f;
        }

        /// CastIntervalReady 조건 충족 여부를 반환한다.
        private bool IsCastIntervalReady()
        {
            return effectiveTickInterval <= 0f || TickRemaining <= 0f;
        }

        /// 전달된 snapshot 값을 사용해 RuntimeModifiers를 현재 런타임 모델을 기준으로 갱신한다.
        private void RefreshRuntimeModifiers(SkillExecutionData snapshot)
        {
            var previousMax = effectiveMaxMagazineSize;
            var nextMax = CalculateMaxMagazineSize(Data);
            var nextBurst = BurstProjectileCount(Data);
            effectiveReloadDuration = CalculateReloadDuration(Data);
            effectiveTickInterval = TickInterval(Data);
            effectiveBurstInterval = BurstInterval(Data);
            effectiveCooldownDuration = CooldownDuration(Data);

            if (snapshot != null)
            {
                nextMax = Math.Max(0, nextMax + snapshot.MagazineBonus);
                if (nextBurst > 1)
                {
                    nextBurst += snapshot.AdditionalProjectileBonus;
                }

                effectiveReloadDuration *= Mathf.Max(0f, snapshot.ReloadTimeMultiplier);
                effectiveTickInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveBurstInterval *= Mathf.Max(0f, snapshot.ShotIntervalMultiplier);
                effectiveCooldownDuration *= Mathf.Max(0f, snapshot.CooldownMultiplier);
            }

            effectiveMaxMagazineSize = nextMax;
            effectiveBurstProjectileCount = Math.Max(1, nextBurst);
            if (previousMax == effectiveMaxMagazineSize)
            {
                return;
            }

            if (effectiveMaxMagazineSize <= 0)
            {
                MagazineRemaining = 0;
                ReloadRemaining = 0f;
                return;
            }

            if (previousMax <= 0)
            {
                MagazineRemaining = effectiveMaxMagazineSize;
                return;
            }

            var delta = effectiveMaxMagazineSize - previousMax;
            MagazineRemaining = Mathf.Clamp(MagazineRemaining + delta, 0, effectiveMaxMagazineSize);
            if (MagazineRemaining > 0)
            {
                ReloadRemaining = 0f;
            }
        }

        /// 전달된 data 값을 사용해 MaxMagazineSize를 계산한다.
        private static int CalculateMaxMagazineSize(SkillDefinition data)
        {
            return Math.Max(0, data.MagazineCapacity);
        }

        /// 전달된 data 값을 사용해 BurstProjectileCount 결과값을 생성해 반환한다.
        private static int BurstProjectileCount(SkillDefinition data)
        {
            var projectile = data as ProjectileSkillDefinition;
            if (projectile != null && projectile.Projectile != null)
            {
                return Math.Max(1, projectile.Projectile.BurstProjectileCount);
            }

            return 1;
        }

        /// 전달된 data 값을 사용해 ReloadDuration를 계산한다.
        private static float CalculateReloadDuration(SkillDefinition data)
        {
            return Mathf.Max(0f, data.ReloadSeconds);
        }

        /// 전달된 data 값을 사용해 Interval를 경과 시간 기준으로 갱신한다.
        private static float TickInterval(SkillDefinition data)
        {
            return Mathf.Max(0f, data.Timing.TickInterval);
        }

        /// 전달된 data 값을 사용해 BurstInterval 결과값을 생성해 반환한다.
        private static float BurstInterval(SkillDefinition data)
        {
            var projectile = data as ProjectileSkillDefinition;
            if (projectile != null && projectile.Projectile != null)
            {
                var burstInterval = projectile.Projectile.BurstIntervalSeconds;
                if (burstInterval > 0f)
                {
                    return burstInterval;
                }
            }

            return TickInterval(data);
        }

        /// 전달된 data 값을 사용해 CooldownDuration 결과값을 생성해 반환한다.
        private static float CooldownDuration(SkillDefinition data)
        {
            return Mathf.Max(0f, data.Timing.Cooldown);
        }

        /// BeginRecoveryIfNeeded 작업을 수행한다.
        private void BeginRecoveryIfNeeded()
        {
            if (!UsesMagazine)
            {
                CooldownRemaining = effectiveCooldownDuration;
                return;
            }

            if (MagazineRemaining > 0)
            {
                return;
            }

            CooldownRemaining = effectiveCooldownDuration;
            if (ReloadDuration > 0f)
            {
                ReloadRemaining = ReloadDuration;
                return;
            }

            if (CooldownRemaining <= 0f)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }
    }
}

namespace Pakuri.InGame
{

    /// SkillExecutionState의 변경 가능한 런타임 상태를 보관한다.
    public class SkillExecutionState
    {
        private readonly List<SkillUseState> activeSkills = new List<SkillUseState>();
        private readonly List<SkillUseState> passiveSkills = new List<SkillUseState>();

        public IReadOnlyList<SkillUseState> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillUseState> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /// 전달된 attribute 값을 사용해 PassiveOutgoingDamageBonus 결과값을 생성해 반환한다.
        public float PassiveOutgoingDamageBonus(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DamageUp, attribute, false) - 1f;
        }

        /// 전달된 attribute 값을 사용해 PassiveDefenseMultiplier 결과값을 생성해 반환한다.
        public float PassiveDefenseMultiplier(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DefenseUp, attribute, false);
        }

        /// PassiveCriticalChanceBonus 결과값을 생성해 반환한다.
        public float PassiveCriticalChanceBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritChanceUp);
        }

        /// PassiveCriticalDamageBonus 결과값을 생성해 반환한다.
        public float PassiveCriticalDamageBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritDamageUp);
        }

        /// PassiveHealingMultiplier 결과값을 생성해 반환한다.
        public float PassiveHealingMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.HealingUp, DamageAttribute.Physical, false);
        }

        /// PassiveIncomingDamageBonus 결과값을 생성해 반환한다.
        public float PassiveIncomingDamageBonus()
        {
            return PassiveMultiplier(PassiveModifierKind.IncomingDamageDown, DamageAttribute.Physical, true) - 1f;
        }

        /// 전달된 런타임 입력값을 사용해 ExecutionData를 생성한다.
        public SkillExecutionData CreateExecutionData(
            UnitCombatState owner,
            SkillUseState skill,
            UnitSpawnManager roster)
        {
            return BuildExecutionData(owner, skill, roster);
        }

        /// 소유한 모든 런타임 값를 소유한 런타임 상태에서 비운다.
        public void Clear()
        {
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /// 전달된 instance 값을 사용해 OrReplace를 소유한 런타임 상태에 추가한다.
        public void AddOrReplace(SkillUseState instance)
        {
            var skills = passiveSkills;
            if (instance.Data.IsActive)
            {
                skills = activeSkills;
            }
            var existingIndex = FindIndexBySkillId(skills, instance.SkillId);
            if (existingIndex >= 0)
            {
                skills[existingIndex] = instance;
                return;
            }

            skills.Add(instance);
        }

        /// 전달된 skillId 값을 사용해 BySkillId를 찾는다.
        public SkillUseState FindBySkillId(string skillId)
        {
            var index = FindIndexBySkillId(activeSkills, skillId);
            if (index >= 0)
            {
                return activeSkills[index];
            }

            index = FindIndexBySkillId(passiveSkills, skillId);
            if (index >= 0)
            {
                return passiveSkills[index];
            }

            return null;
        }

        /// 전달된 choiceId 값을 사용해 Choice를 찾는다.
        public SkillChoice FindChoice(string choiceId)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var choice = FindChoice(activeSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var choice = FindChoice(passiveSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            return null;
        }

        /// 전달된 slot 값을 사용해 BySlot를 찾는다.
        public SkillUseState FindBySlot(SkillSlot slot)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                if (activeSkills[i] != null && activeSkills[i].Slot == slot)
                {
                    return activeSkills[i];
                }
            }

            return null;
        }

        /// 전달된 deltaTime 값을 사용해 요청값를 경과 시간 기준으로 갱신한다.
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i].Tick(deltaTime);
            }
        }

        /// 전달된 런타임 입력값을 사용해 IndexBySkillId를 찾는다.
        private static int FindIndexBySkillId(List<SkillUseState> skills, string skillId)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime != null && string.Equals(runtime.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// 전달된 kind 값을 사용해 PassiveBonus 결과값을 생성해 반환한다.
        private float PassiveBonus(PassiveModifierKind kind)
        {
            var bonus = 0f;
            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var passive = passiveSkills[i].Data as PassiveSkillDefinition;
                if (passive != null && passive.ModifierKind == kind)
                {
                    bonus += Mathf.Max(0f, passive.ModifierValue);
                }
            }

            return bonus;
        }

        /// 전달된 런타임 입력값을 사용해 PassiveMultiplier 결과값을 생성해 반환한다.
        private float PassiveMultiplier(
            PassiveModifierKind kind,
            DamageAttribute attribute,
            bool reduction)
        {
            var multiplier = 1f;
            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var passive = passiveSkills[i].Data as PassiveSkillDefinition;
                if (passive == null
                    || passive.ModifierKind != kind
                    || (passive.HasModifierAttribute && passive.ModifierAttribute != attribute))
                {
                    continue;
                }

                var value = Mathf.Max(0f, passive.ModifierValue);
                multiplier *= reduction
                    ? Mathf.Max(0f, 1f - value)
                    : 1f + value;
            }

            return multiplier;
        }

        /// 전달된 런타임 입력값을 사용해 Choice를 찾는다.
        private static SkillChoice FindChoice(SkillDefinition skill, string choiceId)
        {
            var choice = FindChoice(skill.EnhancementChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            choice = FindChoice(skill.MasterChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            var passive = skill as PassiveSkillDefinition;
            if (passive != null)
            {
                return FindChoice(passive.BaseModifierChoices, choiceId);
            }

            return null;
        }

        /// 전달된 런타임 입력값을 사용해 Choice를 찾는다.
        private static SkillChoice FindChoice(SkillChoice[] choices, string choiceId)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                if (string.Equals(choices[i].ChoiceId, choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return choices[i];
                }
            }

            return null;
        }

        /// 전달된 런타임 입력값을 사용해 ExecutionData를 구성한다.
        private SkillExecutionData BuildExecutionData(UnitCombatState owner, SkillUseState runtime, UnitSpawnManager roster)
        {

            SkillDefinition skillData = null;
            if (runtime != null)
            {
                skillData = runtime.Data;
            }
            var snapshot = new SkillExecutionData(skillData);
            ApplyPassiveBaseModifiers(snapshot, owner, skillData);
            if (skillData == null || owner == null || owner.Skills == null)
            {
                return snapshot;
            }

            ApplyChoices(snapshot, owner.Skills.ChosenEnhancementIds, skillData, owner, roster);
            ApplyChoices(snapshot, owner.Skills.ChosenMasterSkillIds, skillData, owner, roster);
            return snapshot;
        }

        /// 전달된 런타임 입력값을 사용해 PassiveBaseModifiers를 적용한다.
        private static void ApplyPassiveBaseModifiers(
            SkillExecutionData snapshot,
            UnitCombatState owner,
            SkillDefinition skillData)
        {
            if (snapshot == null
                || owner == null
                || owner.Skills == null
                || skillData == null
                || owner.Skills.LearnedPassiveSkillIds.Count == 0)
            {
                return;
            }

            foreach (var passiveId in owner.Skills.LearnedPassiveSkillIds)
            {
                var passiveRuntime = owner.SkillState.FindBySkillId(passiveId);
                PassiveSkillDefinition passive = null;
                if (passiveRuntime != null)
                {
                    passive = passiveRuntime.Data as PassiveSkillDefinition;
                }
                if (passive == null)
                {
                    continue;
                }

                for (var i = 0; i < passive.BaseModifierChoices.Length; i++)
                {
                    var modifier = passive.BaseModifierChoices[i];
                    if (modifier != null && AppliesToSkill(modifier, skillData))
                    {
                        snapshot.ApplyChoiceSpec(modifier);
                    }
                }
            }
        }

        /// 전달된 런타임 입력값을 사용해 Choices를 적용한다.
        private static void ApplyChoices(
            SkillExecutionData snapshot,
            System.Collections.Generic.IReadOnlyCollection<string> chosenChoiceIds,
            SkillDefinition skillData,
            UnitCombatState owner,
            UnitSpawnManager roster)
        {
            if (snapshot == null || chosenChoiceIds == null || skillData == null)
            {
                return;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                var choice = owner.SkillState.FindChoice(choiceId);
                if (choice != null
                    && AppliesToSkill(choice, skillData)
                    && SkillExecutionRuleResolver.MeetsSourceStatusRequirements(choice, skillData.SkillId, owner))
                {
                    snapshot.AddActiveChoiceId(choice.ChoiceId);
                    snapshot.ApplyChoiceSpec(choice);
                    ApplyDynamicChoiceRules(snapshot, choice, owner, roster);
                }
            }
        }

        /// 전달된 런타임 입력값을 사용해 DynamicChoiceRules를 적용한다.
        private static void ApplyDynamicChoiceRules(
            SkillExecutionData snapshot,
            SkillChoice choice,
            UnitCombatState owner,
            UnitSpawnManager roster)
        {
            if (snapshot == null || choice == null || roster == null)
            {
                return;
            }

            SkillNode[] nodes = choice.Nodes;
            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i] == null
                    || !string.Equals(nodes[i].TargetSkillId, snapshot.SkillId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                CountStatusDamageActionOp? action = nodes[i].GetOperation<CountStatusDamageActionOp>();
                if (!action.HasValue)
                {
                    continue;
                }

                ApplyCountStatusDamageMultiplier(
                    snapshot,
                    owner,
                    roster,
                    action.Value.TargetSide,
                    action.Value.StatusKind,
                    action.Value.AmountPerCount,
                    action.Value.MaximumCount);
            }
        }

        /// 전달된 런타임 입력값을 사용해 CountStatusDamageMultiplier를 적용한다.
        private static void ApplyCountStatusDamageMultiplier(
            SkillExecutionData snapshot,
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide targetSide,
            StatusEffectKind statusKind,
            float amountPerCount,
            int countMax)
        {
            if (snapshot == null
                || statusKind == StatusEffectKind.None
                || amountPerCount <= 0f
                || roster == null)
            {
                return;
            }

            var count = CountMatchingTargets(owner, roster, targetSide, statusKind);
            if (countMax > 0)
            {
                count = Mathf.Min(count, countMax);
            }

            if (count <= 0)
            {
                return;
            }

            snapshot.ApplyDynamicDamageMultiplier(1f + count * amountPerCount);
        }

        /// 전달된 런타임 입력값을 사용해 CountMatchingTargets 결과값을 생성해 반환한다.
        private static int CountMatchingTargets(
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide side,
            StatusEffectKind statusKind)
        {
            if (owner == null || roster == null || statusKind == StatusEffectKind.None)
            {
                return 0;
            }

            var entries = CountEntries(owner, roster, side);
            var count = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || !entry.IsAlive || entry.Model == null)
                {
                    continue;
                }

                if (HasStatus(entry.Model, statusKind))
                {
                    count++;
                }
            }

            return count;
        }

        /// 전달된 런타임 입력값을 사용해 CountEntries 결과값을 생성해 반환한다.
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> CountEntries(
            UnitCombatState owner,
            UnitSpawnManager roster,
            SkillMultiEffectTargetSide side)
        {
            if (roster == null || owner == null || owner.Identity == null)
            {
                return System.Array.Empty<CombatUnitEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var allies = roster.Players;
                    if (ownerIsEnemy)
                    {
                        allies = roster.Enemies;
                    }
                    var self = FindEntryForModel(owner, allies);
                    if (IsSkillTarget(self))
                    {
                        return new[] { self };
                    }
                    return System.Array.Empty<CombatUnitEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    if (ownerIsEnemy)
                    {
                        return FilterSkillTargets(roster.Enemies);
                    }
                    return FilterSkillTargets(roster.Players);
                default:
                    if (ownerIsEnemy)
                    {
                        return FilterSkillTargets(roster.Players);
                    }
                    return FilterSkillTargets(roster.Enemies);
            }
        }

        /// 전달된 entries 값을 사용해 FilterSkillTargets 결과값을 생성해 반환한다.
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> FilterSkillTargets(
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return System.Array.Empty<CombatUnitEntry>();
            }

            var filtered = new System.Collections.Generic.List<CombatUnitEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!IsSkillTarget(entry))
                {
                    continue;
                }

                filtered.Add(entry);
            }

            return filtered;
        }

        /// 전달된 entry 값을 사용해 SkillTarget 조건 충족 여부를 반환한다.
        private static bool IsSkillTarget(CombatUnitEntry entry)
        {
            UnitIdentity identity = null;
            if (entry != null && entry.Model != null)
            {
                identity = entry.Model.Identity;
            }
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
        }

        /// 전달된 런타임 입력값을 사용해 EntryForModel를 찾는다.
        private static CombatUnitEntry FindEntryForModel(
            UnitCombatState model,
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries)
        {
            if (model == null || entries == null)
            {
                return null;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                if (entries[i] != null && object.ReferenceEquals(entries[i].Model, model))
                {
                    return entries[i];
                }
            }

            return null;
        }

        /// 전달된 런타임 입력값을 사용해 소유한 런타임 상태에 Status가 있는지 반환한다.
        private static bool HasStatus(UnitCombatState model, StatusEffectKind statusKind, int minimumStacks = 1)
        {
            if (model == null || statusKind == StatusEffectKind.None || minimumStacks <= 0)
            {
                return false;
            }

            if (statusKind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.GetStacks(statusKind) >= minimumStacks;
        }

        /// 전달된 런타임 입력값을 사용해 AppliesToSkill 조건을 평가하고 결과를 반환한다.
        private static bool AppliesToSkill(SkillChoice choice, SkillDefinition skillData)
        {
            if (choice == null || skillData == null)
            {
                return false;
            }

            if (choice.Nodes != null && choice.Nodes.Length > 0)
            {
                for (var i = 0; i < choice.Nodes.Length; i++)
                {
                    if (choice.Nodes[i] != null
                        && string.Equals(
                            choice.Nodes[i].TargetSkillId,
                            skillData.SkillId,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            var targetSkillId = choice.SkillId;
            if (!string.IsNullOrWhiteSpace(choice.TargetSkillId))
            {
                targetSkillId = choice.TargetSkillId;
            }
            return !string.IsNullOrWhiteSpace(targetSkillId)
                && string.Equals(targetSkillId, skillData.SkillId, System.StringComparison.OrdinalIgnoreCase);
        }

        /// 전달된 런타임 입력값을 사용해 PassiveChoices 결과값을 생성해 반환한다.
        public static SkillExecutionData PassiveChoices(UnitCombatState owner, string passiveId)
        {
            return Choices(owner, passiveId, true);
        }

        /// 전달된 런타임 입력값을 사용해 ActiveChoices 결과값을 생성해 반환한다.
        public static SkillExecutionData ActiveChoices(UnitCombatState owner, string skillId)
        {
            return Choices(owner, skillId, false);
        }

        /// 전달된 런타임 입력값을 사용해 Choices 결과값을 생성해 반환한다.
        private static SkillExecutionData Choices(UnitCombatState owner, string skillId, bool useTargetSkillId)
        {
            var snapshot = new SkillExecutionData(null);
            if (owner == null || owner.Skills == null || string.IsNullOrWhiteSpace(skillId))
            {
                return snapshot;
            }

            ApplyResolvedChoices(snapshot, owner, skillId, useTargetSkillId, owner.Skills.ChosenEnhancementIds);
            ApplyResolvedChoices(snapshot, owner, skillId, useTargetSkillId, owner.Skills.ChosenMasterSkillIds);
            return snapshot;
        }

        /// 전달된 런타임 입력값을 사용해 ResolvedChoices를 적용한다.
        private static void ApplyResolvedChoices(
            SkillExecutionData snapshot,
            UnitCombatState owner,
            string skillId,
            bool useTargetSkillId,
            IReadOnlyCollection<string> choiceIds)
        {
            foreach (var choiceId in choiceIds)
            {
                var choice = owner.SkillState.FindChoice(choiceId);
                if (choice == null)
                {
                    continue;
                }

                var choiceSkillId = choice.SkillId;
                if (useTargetSkillId && !string.IsNullOrWhiteSpace(choice.TargetSkillId))
                {
                    choiceSkillId = choice.TargetSkillId;
                }

                if (!string.Equals(choiceSkillId, skillId, System.StringComparison.OrdinalIgnoreCase)
                    || !SkillExecutionRuleResolver.MeetsSourceStatusRequirements(choice, skillId, owner))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                snapshot.ApplyChoiceSpec(choice);
            }
        }
    }
}
