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

    /// <summary><c>SkillExecutionContext</c> 처리에 필요한 불변 실행 문맥을 전달한다.</summary>
    public class SkillExecutionContext
    {

        /// <summary><c>SkillExecutionContext</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
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

    /// <summary>확정된 스킬 시전을 조정하고 설정된 전달 경로로 실행을 분배한다.</summary>
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

        /// <summary><c>SkillAutoRoutePredicate</c> 사건을 전달하는 콜백 시그니처를 정의한다.</summary>
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry, SkillUseState runtime);

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteAutomaticSkills</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteManual</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteSelected</c> 실행 가능 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteSelected</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteTriggered</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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
                    false,
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecuteSkill</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Prepared</c>를 실행한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>SkillCastTriggers</c>를 관련 런타임 시스템에 알린다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Skill</c>를 실행한다.</summary>
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
                return LineSkillExecutor.Execute(context, snapshot, line);
            }

            if (skillData is SingleSkillDefinition single)
            {
                return SingleSkillExecutor.Execute(context, snapshot, single);
            }

            if (skillData is ZoneSkillDefinition zone)
            {
                return ZoneSkillExecutor.Execute(context, snapshot, zone);
            }

            if (skillData is BuffSkillDefinition buff)
            {
                return BuffSkillExecutor.Execute(context, snapshot, buff);
            }

            if (skillData is BuffShieldSkillDefinition shield)
            {
                return BuffShieldSkillExecutor.Execute(context, snapshot, shield);
            }

            if (skillData is BuffHealSkillDefinition heal)
            {
                return BuffHealSkillExecutor.Execute(context, snapshot, heal);
            }

            if (skillData is SingleChainSkillDefinition chain)
            {
                return SingleSkillExecutor.Execute(context, snapshot, chain);
            }

            if (skillData is SingleChargeSkillDefinition charge)
            {
                return SingleSkillExecutor.Execute(context, snapshot, charge);
            }

            throw new InvalidOperationException("Unsupported compiled skill data: " + skillData.GetType().Name);
        }

        /// <summary>전달된 <c>owner</c> 값을 사용해 <c>RebuildLearnedSkillState</c> 작업을 수행한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>RebuildLearnedSkillState</c> 작업을 수행한다.</summary>
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

    /// <summary><c>SkillUseState</c>의 변경 가능한 런타임 상태를 보관한다.</summary>
    public class SkillUseState
    {

        /// <summary><c>SkillUseState</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
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

        /// <summary><c>RuntimeState</c>를 초기 런타임 상태로 되돌린다.</summary>
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

        /// <summary><c>AdvanceProjectileLaunchCount</c> 결과값을 생성해 반환한다.</summary>
        public int AdvanceProjectileLaunchCount()
        {
            if (ProjectileLaunchCount == int.MaxValue)
            {
                ProjectileLaunchCount = 0;
            }

            ProjectileLaunchCount++;
            return ProjectileLaunchCount;
        }

        /// <summary><c>AdvanceSkillHitCount</c> 결과값을 생성해 반환한다.</summary>
        public int AdvanceSkillHitCount()
        {
            if (SkillHitCount == int.MaxValue)
            {
                SkillHitCount = 0;
            }

            SkillHitCount++;
            return SkillHitCount;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConsecutiveHitDamageMultiplier</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 <c>deltaTime</c> 값을 사용해 <c>요청값</c>를 경과 시간 기준으로 갱신한다.</summary>
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

        /// <summary>전달된 <c>snapshot</c> 값을 사용해 <c>CastWithData</c> 실행 가능 여부를 반환한다.</summary>
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

        /// <summary><c>BeginCast</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        public bool TryBeginCast()
        {
            return TryBeginCast(null);
        }

        /// <summary>전달된 <c>snapshot</c> 값을 사용해 <c>BeginCast</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary><c>TickReady</c> 조건 충족 여부를 반환한다.</summary>
        public bool IsTickReady()
        {
            return Data.Timing.TickInterval > 0f && TickRemaining <= 0f;
        }

        /// <summary><c>TickInterval</c>를 초기 런타임 상태로 되돌린다.</summary>
        public void ResetTickInterval()
        {
            TickRemaining = effectiveTickInterval;
        }

        /// <summary><c>CurrentBurstProjectileIndex</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 <c>seconds</c> 값을 사용해 <c>ReduceReloadRemaining</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 <c>seconds</c> 값을 사용해 <c>ReduceCooldownRemaining</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary><c>Cooldown</c>를 초기 런타임 상태로 되돌린다.</summary>
        public void ResetCooldown()
        {
            CooldownRemaining = 0f;
            if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Down</c>를 경과 시간 기준으로 갱신한다.</summary>
        private static float TickDown(float value, float deltaTime)
        {
            if (value > 0f)
            {
                return Mathf.Max(0f, value - deltaTime);
            }

            return 0f;
        }

        /// <summary><c>CastIntervalReady</c> 조건 충족 여부를 반환한다.</summary>
        private bool IsCastIntervalReady()
        {
            return effectiveTickInterval <= 0f || TickRemaining <= 0f;
        }

        /// <summary>전달된 <c>snapshot</c> 값을 사용해 <c>RuntimeModifiers</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
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

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>MaxMagazineSize</c>를 계산한다.</summary>
        private static int CalculateMaxMagazineSize(SkillDefinition data)
        {
            return Math.Max(0, data.MagazineCapacity);
        }

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>BurstProjectileCount</c> 결과값을 생성해 반환한다.</summary>
        private static int BurstProjectileCount(SkillDefinition data)
        {
            var projectile = data as ProjectileSkillDefinition;
            if (projectile != null && projectile.Projectile != null)
            {
                return Math.Max(1, projectile.Projectile.BurstProjectileCount);
            }

            return 1;
        }

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>ReloadDuration</c>를 계산한다.</summary>
        private static float CalculateReloadDuration(SkillDefinition data)
        {
            return Mathf.Max(0f, data.ReloadSeconds);
        }

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>Interval</c>를 경과 시간 기준으로 갱신한다.</summary>
        private static float TickInterval(SkillDefinition data)
        {
            return Mathf.Max(0f, data.Timing.TickInterval);
        }

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>BurstInterval</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 <c>data</c> 값을 사용해 <c>CooldownDuration</c> 결과값을 생성해 반환한다.</summary>
        private static float CooldownDuration(SkillDefinition data)
        {
            return Mathf.Max(0f, data.Timing.Cooldown);
        }

        /// <summary><c>BeginRecoveryIfNeeded</c> 작업을 수행한다.</summary>
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

    /// <summary><c>SkillExecutionState</c>의 변경 가능한 런타임 상태를 보관한다.</summary>
    public class SkillExecutionState
    {
        private readonly List<SkillUseState> activeSkills = new List<SkillUseState>();
        private readonly List<SkillUseState> passiveSkills = new List<SkillUseState>();

        public IReadOnlyList<SkillUseState> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillUseState> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /// <summary>전달된 <c>attribute</c> 값을 사용해 <c>PassiveOutgoingDamageBonus</c> 결과값을 생성해 반환한다.</summary>
        public float PassiveOutgoingDamageBonus(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DamageUp, attribute, false) - 1f;
        }

        /// <summary>전달된 <c>attribute</c> 값을 사용해 <c>PassiveDefenseMultiplier</c> 결과값을 생성해 반환한다.</summary>
        public float PassiveDefenseMultiplier(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DefenseUp, attribute, false);
        }

        /// <summary><c>PassiveCriticalChanceBonus</c> 결과값을 생성해 반환한다.</summary>
        public float PassiveCriticalChanceBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritChanceUp);
        }

        /// <summary><c>PassiveCriticalDamageBonus</c> 결과값을 생성해 반환한다.</summary>
        public float PassiveCriticalDamageBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritDamageUp);
        }

        /// <summary><c>PassiveHealingMultiplier</c> 결과값을 생성해 반환한다.</summary>
        public float PassiveHealingMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.HealingUp, DamageAttribute.Physical, false);
        }

        /// <summary><c>PassiveIncomingDamageBonus</c> 결과값을 생성해 반환한다.</summary>
        public float PassiveIncomingDamageBonus()
        {
            return PassiveMultiplier(PassiveModifierKind.IncomingDamageDown, DamageAttribute.Physical, true) - 1f;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecutionData</c>를 생성한다.</summary>
        public SkillExecutionData CreateExecutionData(
            UnitCombatState owner,
            SkillUseState skill,
            UnitSpawnManager roster)
        {
            return BuildExecutionData(owner, skill, roster);
        }

        /// <summary><c>소유한 모든 런타임 값</c>를 소유한 런타임 상태에서 비운다.</summary>
        public void Clear()
        {
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /// <summary>전달된 <c>instance</c> 값을 사용해 <c>OrReplace</c>를 소유한 런타임 상태에 추가한다.</summary>
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

        /// <summary>전달된 <c>skillId</c> 값을 사용해 <c>BySkillId</c>를 찾는다.</summary>
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

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 <c>Choice</c>를 찾는다.</summary>
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

        /// <summary>전달된 <c>slot</c> 값을 사용해 <c>BySlot</c>를 찾는다.</summary>
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

        /// <summary>전달된 <c>deltaTime</c> 값을 사용해 <c>요청값</c>를 경과 시간 기준으로 갱신한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>IndexBySkillId</c>를 찾는다.</summary>
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

        /// <summary>전달된 <c>kind</c> 값을 사용해 <c>PassiveBonus</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>PassiveMultiplier</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Choice</c>를 찾는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Choice</c>를 찾는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ExecutionData</c>를 구성한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>PassiveBaseModifiers</c>를 적용한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Choices</c>를 적용한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>DynamicChoiceRules</c>를 적용한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>CountStatusDamageMultiplier</c>를 적용한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>CountMatchingTargets</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>CountEntries</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 <c>entries</c> 값을 사용해 <c>FilterSkillTargets</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 <c>entry</c> 값을 사용해 <c>SkillTarget</c> 조건 충족 여부를 반환한다.</summary>
        private static bool IsSkillTarget(CombatUnitEntry entry)
        {
            UnitIdentity identity = null;
            if (entry != null && entry.Model != null)
            {
                identity = entry.Model.Identity;
            }
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>EntryForModel</c>를 찾는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 소유한 런타임 상태에 <c>Status</c>가 있는지 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>AppliesToSkill</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>PassiveChoices</c> 결과값을 생성해 반환한다.</summary>
        public static SkillExecutionData PassiveChoices(UnitCombatState owner, string passiveId)
        {
            return Choices(owner, passiveId, true);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ActiveChoices</c> 결과값을 생성해 반환한다.</summary>
        public static SkillExecutionData ActiveChoices(UnitCombatState owner, string skillId)
        {
            return Choices(owner, skillId, false);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Choices</c> 결과값을 생성해 반환한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResolvedChoices</c>를 적용한다.</summary>
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
