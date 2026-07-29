using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 자동·수동·Trigger 스킬 요청의 공통 진입점과 스킬별 사용 상태를 제공한다.
 * 시전 가능 여부와 실행 스냅샷을 확정한 뒤 Definition의 실제 계열에 맞는 Executor로 전달하며,
 * 시전 전후 lifecycle 사건은 SkillActionContext로 Trigger 시스템에 발행한다.
 */
namespace Pakuri.InGame
{
    /*
     * 한 번의 스킬 실행에 필요한 전투 시스템, 시전자, 대상과 조준 정보를 보관한다.
     */
    public class SkillExecutionContext
    {
        /*
         * 전달받은 전투 참조와 조준 정보를 실행 문맥에 기록한다.
         */
        public SkillExecutionContext(
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            UnitCombatState eventTarget = null /* 사건 대상 */,
            bool hasManualAimDirection = false /* 보유 수동 조준 방향 여부 */,
            Vector2 manualAimDirection = default /* 수동 조준 방향 */,
            bool hasManualTargetPoint = false /* 보유 수동 대상 위치 여부 */,
            Vector2 manualTargetPoint = default /* 수동 대상 위치 */,
            int recastGeneration = 0 /* 재시전 실행 세대 */,
            bool lockToEventTarget = false /* 사건 대상 고정 여부 */,
            bool publishSkillLifecycleEvents = true /* 스킬 lifecycle 발행 여부 */,
            bool applyDamageMultiplierToShield = true /* 보호막에 피해 배율 적용 여부 */,
            string sourceSkillId = null /* 피해와 사건에 사용할 원본 스킬 식별자 */)
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

    /*
     * 자동·수동·Trigger 실행 요청을 판정하고 준비된 정보를 스킬 종류별 실행기로 전달한다.
     * 계열별 피해·대상·Actor 생성은 직접 구현하지 않고 각 전용 Executor에 위임한다.
     */
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

        /*
         * 자동 시전 요청을 실행기로 전달해도 되는지 판단하는 함수 형식을 정의한다.
         */
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry /* 처리할 등록 정보 */, SkillUseState runtime /* 실행 중인 스킬 정보 */);

        /*
         * 자동 실행이 허용된 유닛의 액티브 스킬 실행을 요청한다.
         */
        public void TryExecuteAutomaticSkills(
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            SkillAutoRoutePredicate canAutoRoute = null /* 가능 자동 실행 경로 여부 */)
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

        /*
         * 수동 조준 방향과 목표 지점을 사용해 선택한 스킬의 실행을 요청한다.
         */
        public bool TryExecuteManual(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            Vector2 aimDirection /* 조준 방향 */,
            Vector2 targetPoint /* 지정한 대상 위치 */)
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

        /*
         * 현재 상태와 선택지 보정을 반영해 선택한 스킬을 시전할 수 있는지 확인한다.
         */
        public bool CanExecuteSelected(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */)
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

        /*
         * 자동 조준 방식으로 선택한 스킬의 실행을 요청한다.
         */
        public bool TryExecuteSelected(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */)
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

        /*
         * 일반·수동·트리거 요청의 실행 데이터와 실행 정보를 준비해 스킬 종류별 실행기로 전달한다.
         */
        private bool TryExecuteSkill(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            bool hasManualAimDirection /* 보유 수동 조준 방향 여부 */,
            Vector2 manualAimDirection /* 수동 조준 방향 */,
            bool hasManualTargetPoint /* 보유 수동 대상 위치 여부 */,
            Vector2 manualTargetPoint /* 수동 대상 위치 */,
            bool beginCast /* 쿨타임과 탄창을 사용하는 일반 시전 여부 */,
            float damageMultiplier /* 요청에서 추가할 피해 배율 */,
            string triggerSourceSkillId /* Trigger를 발생시킨 원본 스킬 식별자 */)
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

        /*
         * 완료된 스킬 시전 위치와 출처를 SkillTrigger에 전달한다.
         */
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            string triggerSourceSkillId = null /* 트리거 발생 원본 스킬 식별자 */)
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

        /*
         * 준비된 실행 정의의 종류에 맞는 스킬 실행기를 호출한다.
         */
        private static bool ExecuteSkill(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            SkillDefinition skillData /* 스킬 실행 데이터 */)
        {
            // 컴파일된 Definition의 실제 타입을 계열별 Executor로 전달하는 부분을 구현.
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

        /*
         * 유닛이 학습한 액티브·패시브 스킬의 실행 상태를 다시 만든다.
         */
        public static void RebuildLearnedSkillState(UnitCombatState owner /* 실행 상태를 다시 만들 유닛 */)
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

        /*
         * 전달받은 정의 중 유닛이 학습한 액티브·패시브의 실행 상태를 다시 만든다.
         */
        public static void RebuildLearnedSkillState(
            UnitCombatState owner /* 실행 상태를 다시 만들 유닛 */,
            SkillDefinition[] activeDefinitions /* 확인할 액티브 스킬 정의 목록 */,
            PassiveSkillDefinition[] passiveDefinitions /* 확인할 패시브 스킬 정의 목록 */)
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

/*
 * 실행 준비가 끝난 스킬 하나가 전투 중 가지는 변경 가능한 상태를 관리한다.
 * 재사용 대기시간, 탄창·재장전, Tick, 연속 발사, 적중 횟수를 갱신하고
 * 현재 선택한 강화 데이터에 따른 시전 가능 여부와 시간 보정값을 적용한다.
 */
namespace Pakuri.InGame
{
    public class SkillUseState
    {
        /*
         * 스킬 사용 상태에 필요한 값을 초기화한다.
         */
        public SkillUseState(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillDefinition data /* 처리할 실행 데이터 */)
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
        /* 강화 스냅샷을 만들지 않은 기본 상태에서 현재 시전 가능 여부를 반환한다. */
        public bool CanCast => CanCastWithData(null);

        /*
         * 재사용 대기시간, 탄창, 연속 적중 상태를 초기화한다.
         */
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

        /*
         * 투사체 발사 횟수를 증가시키고 현재 횟수를 반환한다.
         */
        public int AdvanceProjectileLaunchCount()
        {
            if (ProjectileLaunchCount == int.MaxValue)
            {
                ProjectileLaunchCount = 0;
            }

            ProjectileLaunchCount++;
            return ProjectileLaunchCount;
        }

        /*
         * 스킬 적중 횟수를 증가시키고 현재 횟수를 반환한다.
         */
        public int AdvanceSkillHitCount()
        {
            if (SkillHitCount == int.MaxValue)
            {
                SkillHitCount = 0;
            }

            SkillHitCount++;
            return SkillHitCount;
        }

        /*
         * 같은 대상을 연속으로 적중했을 때 적용할 피해 배율을 결정한다.
         */
        public float ConsecutiveHitDamageMultiplier(UnitCombatState target /* 효과를 받을 대상 유닛 */, SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * 스킬의 시전, 지속시간, 재사용 대기시간을 갱신한다.
         */
        public void Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */)
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

        /*
         * 시전 포함 실행 정보를 가능한 상태인지 확인한다.
         */
        public bool CanCastWithData(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * 시전을 시작하고 성공 여부를 반환한다.
         */
        public bool TryBeginCast()
        {
            return TryBeginCast(null);
        }

        /*
         * 시전을 시작하고 성공 여부를 반환한다.
         */
        public bool TryBeginCast(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * 다음 주기 효과를 실행할 시간이 되었는지 확인한다.
         */
        public bool IsTickReady()
        {
            return Data.Timing.TickInterval > 0f && TickRemaining <= 0f;
        }

        /*
         * 주기 간격을 초기화한다.
         */
        public void ResetTickInterval()
        {
            TickRemaining = effectiveTickInterval;
        }

        /*
         * 현재 연속 발사에서 몇 번째 투사체인지 계산한다.
         */
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

        /*
         * 남은 재장전 시간을 감소시킨다.
         */
        public bool ReduceReloadRemaining(float seconds /* 초 */)
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

        /*
         * 남은 재사용 대기시간을 감소시킨다.
         */
        public bool ReduceCooldownRemaining(float seconds /* 초 */)
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

        /*
         * 재사용 대기시간을 초기화한다.
         */
        public void ResetCooldown()
        {
            CooldownRemaining = 0f;
            if (UsesMagazine && MagazineRemaining <= 0 && ReloadRemaining <= 0f && !IsBursting)
            {
                MagazineRemaining = MaxMagazineSize;
            }
        }

        /*
         * 남은 시간을 0 이하로 내려가지 않게 감소시킨다.
         */
        private static float TickDown(float value /* 처리할 값 */, float deltaTime /* 이전 갱신 이후 지난 시간 */)
        {
            if (value > 0f)
            {
                return Mathf.Max(0f, value - deltaTime);
            }

            return 0f;
        }

        /*
         * 다음 시전을 실행할 간격이 지났는지 확인한다.
         */
        private bool IsCastIntervalReady()
        {
            return effectiveTickInterval <= 0f || TickRemaining <= 0f;
        }

        /*
         * 현재 선택지에 맞춰 스킬 사용 보정값을 다시 계산한다.
         */
        private void RefreshRuntimeModifiers(SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */)
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

        /*
         * 최대 탄창 크기를 결정한다.
         */
        private static int CalculateMaxMagazineSize(SkillDefinition data /* 처리할 실행 데이터 */)
        {
            return Math.Max(0, data.MagazineCapacity);
        }

        /*
         * 연속 발사 투사체 횟수를 결정한다.
         */
        private static int BurstProjectileCount(SkillDefinition data /* 처리할 실행 데이터 */)
        {
            var projectile = data as ProjectileSkillDefinition;
            if (projectile != null && projectile.Projectile != null)
            {
                return Math.Max(1, projectile.Projectile.BurstProjectileCount);
            }

            return 1;
        }

        /*
         * 재장전 지속시간을 결정한다.
         */
        private static float CalculateReloadDuration(SkillDefinition data /* 처리할 실행 데이터 */)
        {
            return Mathf.Max(0f, data.ReloadSeconds);
        }

        /*
         * 주기 간격을 결정한다.
         */
        private static float TickInterval(SkillDefinition data /* 처리할 실행 데이터 */)
        {
            return Mathf.Max(0f, data.Timing.TickInterval);
        }

        /*
         * 연속 발사 간격을 결정한다.
         */
        private static float BurstInterval(SkillDefinition data /* 처리할 실행 데이터 */)
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

        /*
         * 재사용 대기시간 지속시간을 결정한다.
         */
        private static float CooldownDuration(SkillDefinition data /* 처리할 실행 데이터 */)
        {
            return Mathf.Max(0f, data.Timing.Cooldown);
        }

        /*
         * 발사나 시전이 끝났다면 재사용 대기 또는 재장전을 시작한다.
         */
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
/*
 * 유닛별 스킬의 쿨타임, 탄창, 시전 상태와 실행 목록을 관리한다.
 * 실행 직전에는 UnitSkills에 저장된 선택 결과를 반영한 실행 데이터를 만든다.
 */
namespace Pakuri.InGame
{
    public class SkillExecutionState
    {
        private readonly List<SkillUseState> activeSkills = new List<SkillUseState>();
        private readonly List<SkillUseState> passiveSkills = new List<SkillUseState>();

        public IReadOnlyList<SkillUseState> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillUseState> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /*
         * 학습한 패시브가 현재 속성에 더하는 주는 피해 보너스를 반환한다.
         */
        public float PassiveOutgoingDamageBonus(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DamageUp, attribute, false) - 1f;
        }

        /*
         * 학습한 패시브가 현재 속성 방어력에 적용하는 배율을 반환한다.
         */
        public float PassiveDefenseMultiplier(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DefenseUp, attribute, false);
        }

        /*
         * 학습한 패시브가 더하는 치명타 확률을 반환한다.
         */
        public float PassiveCriticalChanceBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritChanceUp);
        }

        /*
         * 학습한 패시브가 더하는 치명타 피해를 반환한다.
         */
        public float PassiveCriticalDamageBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritDamageUp);
        }

        /*
         * 학습한 패시브가 회복량에 적용하는 배율을 반환한다.
         */
        public float PassiveHealingMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.HealingUp, DamageAttribute.Physical, false);
        }

        /*
         * 학습한 패시브가 더하는 받는 피해 보너스를 반환한다.
         */
        public float PassiveIncomingDamageBonus()
        {
            return PassiveMultiplier(PassiveModifierKind.IncomingDamageDown, DamageAttribute.Physical, true) - 1f;
        }

        /*
         * 현재 학습 상태와 전투 상황을 반영한 스킬 실행 데이터를 만든다.
         */
        public SkillExecutionData CreateExecutionData(
            UnitCombatState owner /* 스킬을 사용하는 유닛 */,
            SkillUseState skill /* 실행할 스킬 상태 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */)
        {
            return BuildExecutionData(owner, skill, roster);
        }

        /*
         * 유닛의 활성 스킬과 패시브 실행 목록을 비운다.
         */
        public void Clear()
        {
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /*
         * 같은 ID의 스킬을 교체하거나 새 스킬을 추가한다.
         */
        public void AddOrReplace(SkillUseState instance /* 생성된 게임 오브젝트 */)
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

        /*
         * 스킬 ID가 일치하는 사용 상태를 찾는다.
         */
        public SkillUseState FindBySkillId(string skillId /* 스킬 식별자 */)
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

        /*
         * 선택지 ID가 일치하는 실행 정의를 찾는다.
         */
        public SkillChoice FindChoice(string choiceId /* 스킬 선택지 식별자 */)
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

        /*
         * 스킬 슬롯이 일치하는 사용 상태를 찾는다.
         */
        public SkillUseState FindBySlot(SkillSlot slot /* 스킬이나 유닛이 배치될 슬롯 */)
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

        /*
         * 유닛이 보유한 모든 활성 스킬의 시간을 갱신한다.
         */
        public void Tick(float deltaTime /* 이전 갱신 이후 지난 시간 */)
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

        /*
         * 스킬 ID가 일치하는 사용 상태의 목록 위치를 찾는다.
         */
        private static int FindIndexBySkillId(List<SkillUseState> skills /* 스킬 목록 */, string skillId /* 스킬 식별자 */)
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

        /*
         * 지정 종류 패시브의 추가 수치를 합산한다.
         */
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

        /*
         * 지정 종류 패시브의 속성 조건을 확인하고 배율을 누적한다.
         */
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

        /*
         * FindChoice에 해당하는 값을 찾아 반환한다.
         */
        private static SkillChoice FindChoice(SkillDefinition skill /* 실행하거나 검사할 스킬 */, string choiceId /* 스킬 선택지 식별자 */)
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

        /*
         * FindChoice에 해당하는 값을 찾아 반환한다.
         */
        private static SkillChoice FindChoice(SkillChoice[] choices /* 선택지 목록 */, string choiceId /* 스킬 선택지 식별자 */)
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
        /*
         * 유닛이 학습한 선택지를 현재 스킬 실행 정보에 적용한다.
         */
        private SkillExecutionData BuildExecutionData(UnitCombatState owner /* 정보를 소유한 유닛 */, SkillUseState runtime /* 실행 중인 스킬 정보 */, UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */)
        {
            // Base, 패시브, 강화, 마스터를 한 번의 실행 스냅샷으로 조립하는 부분을 구현.
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

        /*
         * 패시브 기본 보정값을 적용한다.
         */
        private static void ApplyPassiveBaseModifiers(
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            SkillDefinition skillData /* 스킬 실행 데이터 */)
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

        /*
         * 선택지를 적용한다.
         */
        private static void ApplyChoices(
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            System.Collections.Generic.IReadOnlyCollection<string> chosenChoiceIds /* 선택된 선택지 식별자 목록 */,
            SkillDefinition skillData /* 스킬 실행 데이터 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */)
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

        /*
         * 동적 선택지 규칙을 적용한다.
         */
        private static void ApplyDynamicChoiceRules(
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            SkillChoice choice /* 적용하거나 검사할 스킬 선택지 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */)
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

        /*
         * 횟수 상태 피해 배율을 적용한다.
         */
        private static void ApplyCountStatusDamageMultiplier(
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide targetSide /* 대상 진영 */,
            StatusEffectKind statusKind /* 상태 효과 종류 */,
            float amountPerCount /* 수치 개별 개수 */,
            int countMax /* 개수 최대 */)
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

        /*
         * 선택지 조건과 일치하는 대상 수를 계산한다.
         */
        private static int CountMatchingTargets(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide side /* 진영 */,
            StatusEffectKind statusKind /* 상태 효과 종류 */)
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

        /*
         * 횟수 유닛 항목을 결정한다.
         */
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> CountEntries(
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            UnitSpawnManager roster /* 전투에 등록된 유닛 목록 */,
            SkillMultiEffectTargetSide side /* 진영 */)
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

        /*
         * 스킬 대상을 조건에 맞는 값만 선별한다.
         */
        private static System.Collections.Generic.IReadOnlyList<CombatUnitEntry> FilterSkillTargets(
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries /* 등록 정보 목록 */)
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

        /*
         * 유닛이 선택지 효과의 적용 대상인지 확인한다.
         */
        private static bool IsSkillTarget(CombatUnitEntry entry /* 처리할 등록 정보 */)
        {
            UnitIdentity identity = null;
            if (entry != null && entry.Model != null)
            {
                identity = entry.Model.Identity;
            }
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
        }

        /*
         * 유닛 항목 대상 모델을 찾는다.
         */
        private static CombatUnitEntry FindEntryForModel(
            UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */,
            System.Collections.Generic.IReadOnlyList<CombatUnitEntry> entries /* 등록 정보 목록 */)
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

        /*
         * 상태를 보유하고 있는지 확인한다.
         */
        private static bool HasStatus(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minimumStacks = 1 /* 최소 중첩 수 */)
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

        /*
         * 선택지 효과가 현재 스킬에 적용되는지 확인한다.
         */
        private static bool AppliesToSkill(SkillChoice choice /* 적용하거나 검사할 스킬 선택지 */, SkillDefinition skillData /* 스킬 실행 데이터 */)
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

        /*
         * 패시브에 연결된 강화 선택지를 실행 데이터로 만든다.
         */
        public static SkillExecutionData PassiveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveId /* 패시브 식별자 */)
        {
            return Choices(owner, passiveId, true);
        }

        /*
         * 활성 스킬에 연결된 강화와 마스터 선택지를 실행 데이터로 만든다.
         */
        public static SkillExecutionData ActiveChoices(UnitCombatState owner /* 정보를 소유한 유닛 */, string skillId /* 스킬 식별자 */)
        {
            return Choices(owner, skillId, false);
        }

        /*
         * Choices 결과를 계산해 반환한다.
         */
        private static SkillExecutionData Choices(UnitCombatState owner /* 정보를 소유한 유닛 */, string skillId /* 스킬 식별자 */, bool useTargetSkillId /* 사용 대상 스킬 식별자 여부 */)
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

        /*
         * 지정한 선택 목록에서 현재 스킬에 적용되는 강화 효과를 실행 데이터에 반영한다.
         */
        private static void ApplyResolvedChoices(
            SkillExecutionData snapshot /* 적용할 스킬 강화 정보 */,
            UnitCombatState owner /* 정보를 소유한 유닛 */,
            string skillId /* 스킬 식별자 */,
            bool useTargetSkillId /* 대상 스킬 식별자를 사용할지 여부 */,
            IReadOnlyCollection<string> choiceIds /* 적용할 선택지 식별자 목록 */)
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
