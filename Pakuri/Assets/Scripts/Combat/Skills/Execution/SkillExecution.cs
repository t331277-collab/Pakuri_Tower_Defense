using Pakuri.Data;
using UnityEngine;

/*
 * 모든 유닛의 스킬 상태 갱신과 실행 요청 라우팅을 담당하는 전투 시스템.
 * 자동·수동·Trigger 요청에 Choice Snapshot을 적용하고 스킬 형식에 맞는
 * Executor 실행과 시전 Trigger를 연결한다.
 */
namespace Pakuri.InGame
{
    public sealed class SkillExecutionContext
    {
        /*
         * 한 번의 스킬 실행에 필요한 전투 대상과 조준 정보를 보관한다.
         */
        public SkillExecutionContext(
            InGameCombatManager combatManager,
            CombatUnitRegistry roster,
            CombatUnitEntry casterEntry,
            SkillRuntimeInstance runtime,
            UnitCombatState eventTarget = null,
            bool hasManualAimDirection = false,
            Vector2 manualAimDirection = default,
            bool hasManualTargetPoint = false,
            Vector2 manualTargetPoint = default,
            int recastGeneration = 0)
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
        }

        public InGameCombatManager CombatManager { get; }
        public CombatUnitRegistry Roster { get; }
        public CombatUnitEntry CasterEntry { get; }
        public SkillRuntimeInstance Runtime { get; }
        public UnitCombatState EventTarget { get; }
        public bool HasManualAimDirection { get; }
        public Vector2 ManualAimDirection { get; }
        public bool HasManualTargetPoint { get; }
        public Vector2 ManualTargetPoint { get; }
        public int RecastGeneration { get; }

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

    public sealed class SkillExecution
    {
        /*
         * 스킬 자동 전달 조건 함수 호출 형식을 정의한다.
         */
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry, SkillRuntimeInstance runtime);

        private readonly SkillUpgrade choiceResolver = new SkillUpgrade();

        /*
         * 로스터의 모든 유닛 스킬 상태와 자동 시전을 갱신한다.
         */
        public void Tick(
            CombatUnitRegistry roster,
            InGameCombatManager combatManager,
            float deltaTime,
            SkillAutoRoutePredicate canAutoRoute = null)
        {
            if (roster == null || deltaTime <= 0f)
            {
                return;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var model = entry != null ? entry.Model : null;
                var skillRuntime = model != null ? model.SkillRuntime : null;
                if (skillRuntime == null)
                {
                    continue;
                }

                skillRuntime.Tick(deltaTime);
                if (!model.AutoSkillEnabled || !entry.IsAlive || !StatusCombatRules.CanAct(model))
                {
                    continue;
                }

                var activeSkills = skillRuntime.ActiveSkills;
                for (var skillIndex = 0; skillIndex < activeSkills.Count; skillIndex++)
                {
                    var runtime = activeSkills[skillIndex];
                    if (canAutoRoute != null && !canAutoRoute(entry, runtime))
                    {
                        continue;
                    }

                    TryRouteSkill(entry, runtime, roster, combatManager, false, default, false, default);
                }
            }
        }

        /*
         * 수동을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteManual(
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            CombatUnitRegistry roster,
            InGameCombatManager combatManager,
            Vector2 aimDirection,
            Vector2 targetPoint)
        {
            return TryRouteSkill(entry, runtime, roster, combatManager, true, aimDirection, true, targetPoint);
        }

        /*
         * 처형 선택된을 가능한 상태인지 확인한다.
         */
        public bool CanExecuteSelected(
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            CombatUnitRegistry roster)
        {
            if (entry == null
                || runtime == null
                || !StatusCombatRules.CanAct(entry.Model)
                || !CanExecute(runtime.Data))
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
            return runtime.CanCastWithSnapshot(snapshot);
        }

        /*
         * 선택된을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteSelected(
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            CombatUnitRegistry roster,
            InGameCombatManager combatManager)
        {
            return TryRouteSkill(entry, runtime, roster, combatManager, false, default, false, default);
        }

        /*
         * 트리거된을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteTriggered(
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            CombatUnitRegistry roster,
            InGameCombatManager combatManager,
            Vector2 targetPoint,
            bool hasTargetPoint,
            float triggeredDamageMultiplier = 1f,
            string triggerSourceSkillId = null)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            var aimDirection = entry.Transform != null && hasTargetPoint
                ? targetPoint - (Vector2)entry.Transform.position
                : default;
            var hasAimDirection = hasTargetPoint && aimDirection.sqrMagnitude > 0.0001f;
            return TryExecuteTriggeredSkill(
                entry,
                runtime,
                roster,
                combatManager,
                hasAimDirection,
                aimDirection,
                hasTargetPoint,
                targetPoint,
                triggeredDamageMultiplier,
                triggerSourceSkillId);
        }

        /*
         * 스킬 데이터에 맞는 실행기로 요청 전달을 시도한다.
         */
        private bool TryRouteSkill(
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            CombatUnitRegistry roster,
            InGameCombatManager combatManager,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint)
        {
            if (runtime == null || entry == null || !StatusCombatRules.CanAct(entry.Model))
            {
                return false;
            }

            // 실행 직전에 학습 선택지를 반영한 스냅샷으로 시전 가능 여부를 판단한다.
            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
            if (!runtime.CanCastWithSnapshot(snapshot))
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
                manualTargetPoint: manualTargetPoint);
            var routed = ExecuteSkill(context, snapshot, runtime.Data);
            if (routed)
            {
                // 실행기가 요청을 처리한 경우에만 재사용 대기시간과 시전 트리거를 시작한다.
                if (!runtime.TryBeginCast(snapshot))
                {
                    return false;
                }

                var monsterActor = entry.Actor as MonsterActor;
                if (monsterActor != null)
                {
                    monsterActor.TryPlayActiveSkillAnimation();
                }

                NotifySkillCastTriggers(combatManager, roster, entry, runtime, context);
            }

            return routed;
        }

        /*
         * 스킬 시전 사실을 트리거 런타임에 전달한다.
         */
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager,
            CombatUnitRegistry roster,
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            SkillExecutionContext context,
            string triggerSourceSkillId = null)
        {
            if (combatManager == null || entry == null || runtime == null || runtime.Data == null)
            {
                return;
            }

            var center = context != null && context.HasManualTargetPoint
                ? context.ManualTargetPoint
                : entry.Transform != null ? (Vector2)entry.Transform.position : Vector2.zero;
            SkillTrigger.ExecuteSkillCast(
                combatManager,
                roster,
                entry.Model,
                runtime.Data.SkillId,
                center,
                triggerSourceSkillId);
        }

        /*
         * 트리거된 스킬을 실행하고 성공 여부를 반환한다.
         */
        private bool TryExecuteTriggeredSkill(
            CombatUnitEntry entry,
            SkillRuntimeInstance runtime,
            CombatUnitRegistry roster,
            InGameCombatManager combatManager,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            float triggeredDamageMultiplier,
            string triggerSourceSkillId)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
            if (!Mathf.Approximately(triggeredDamageMultiplier, 1f))
            {
                snapshot.ApplyDynamicDamageMultiplier(triggeredDamageMultiplier);
            }

            var context = new SkillExecutionContext(
                combatManager,
                roster,
                entry,
                runtime,
                hasManualAimDirection: hasManualAimDirection,
                manualAimDirection: manualAimDirection,
                hasManualTargetPoint: hasManualTargetPoint,
                manualTargetPoint: manualTargetPoint);
            var routed = ExecuteSkill(context, snapshot, runtime.Data);
            if (routed)
            {
                NotifySkillCastTriggers(combatManager, roster, entry, runtime, context, triggerSourceSkillId);
            }

            return routed;
        }

        /*
         * 스킬 자료형에 맞는 무상태 실행기를 직접 호출한다.
         */
        private static bool ExecuteSkill(
            SkillExecutionContext context,
            SkillSnapshot snapshot,
            SkillRuntimeData skillData)
        {
            if (skillData is ProjectileSkillRuntimeData projectile)
            {
                return ProjectileSkillExecutor.Execute(context, snapshot, projectile);
            }

            if (skillData is LineSkillRuntimeData line)
            {
                return LineSkillExecutor.Execute(context, snapshot, line);
            }

            if (skillData is SingleSkillRuntimeData single)
            {
                return SingleSkillExecutor.Execute(context, snapshot, single);
            }

            if (skillData is ZoneSkillRuntimeData zone)
            {
                return ZoneSkillExecutor.Execute(context, snapshot, zone);
            }

            if (skillData is BuffSkillRuntimeData buff)
            {
                return BuffSkillExecutor.Execute(context, snapshot, buff);
            }

            if (skillData is BuffShieldSkillRuntimeData shield)
            {
                return BuffShieldSkillExecutor.Execute(context, snapshot, shield);
            }

            if (skillData is BuffHealSkillRuntimeData heal)
            {
                return BuffHealSkillExecutor.Execute(context, snapshot, heal);
            }

            if (skillData is SingleChainSkillRuntimeData chain)
            {
                return SingleSkillExecutor.Execute(context, snapshot, chain);
            }

            if (skillData is SingleChargeSkillRuntimeData charge)
            {
                return SingleSkillExecutor.Execute(context, snapshot, charge);
            }

            return false;
        }

        /*
         * 현재 실행 시스템이 처리하는 스킬 자료형인지 확인한다.
         */
        private static bool CanExecute(SkillRuntimeData skillData)
        {
            return skillData is ProjectileSkillRuntimeData
                || skillData is LineSkillRuntimeData
                || skillData is SingleSkillRuntimeData
                || skillData is ZoneSkillRuntimeData
                || skillData is BuffSkillRuntimeData
                || skillData is BuffShieldSkillRuntimeData
                || skillData is BuffHealSkillRuntimeData
                || skillData is SingleChainSkillRuntimeData
                || skillData is SingleChargeSkillRuntimeData;
        }
    }

}

