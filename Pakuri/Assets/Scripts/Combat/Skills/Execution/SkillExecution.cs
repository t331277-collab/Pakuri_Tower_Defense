using System;
using Pakuri.Data;
using UnityEngine;

/*
 * 모든 유닛의 스킬 상태 갱신과 실행 요청 라우팅을 담당하는 전투 시스템.
 * 자동·수동·Trigger 요청에 Choice Snapshot을 적용하고 스킬 형식에 맞는
 * Executor 실행과 시전 Trigger를 연결한다.
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
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            UnitCombatState eventTarget = null /* 사건 대상 */,
            bool hasManualAimDirection = false /* 보유 수동 조준 방향 여부 */,
            Vector2 manualAimDirection = default /* 수동 조준 방향 */,
            bool hasManualTargetPoint = false /* 보유 수동 대상 위치 여부 */,
            Vector2 manualTargetPoint = default /* 수동 대상 위치 */,
            int recastGeneration = 0 /* 재시전 실행 세대 */)
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

    /*
     * 유닛의 스킬 시간을 갱신하고 자동·수동·Trigger 실행 요청을 종류별 실행기로 전달한다.
     */
    public class SkillExecution
    {
        /*
         * 자동 시전 요청을 실행기로 전달해도 되는지 판단하는 함수 형식을 정의한다.
         */
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry /* 처리할 등록 정보 */, SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */);

        private readonly SkillUpgrade choiceResolver = new SkillUpgrade();

        /*
         * 로스터의 모든 유닛 스킬 상태와 자동 시전을 갱신한다.
         */
        public void Tick(
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            float deltaTime /* 이전 갱신 이후 지난 시간 */,
            SkillAutoRoutePredicate canAutoRoute = null /* 가능 자동 실행 경로 여부 */)
        {
            if (roster == null || deltaTime <= 0f)
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
                var skillRuntime = model.SkillRuntime;

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
         * 수동 조준 방향과 목표 지점을 사용해 선택한 스킬의 실행을 요청한다.
         */
        public bool TryExecuteManual(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            Vector2 aimDirection /* 조준 방향 */,
            Vector2 targetPoint /* 지정한 대상 위치 */)
        {
            return TryRouteSkill(entry, runtime, roster, combatManager, true, aimDirection, true, targetPoint);
        }

        /*
         * 현재 상태와 선택지 보정을 반영해 선택한 스킬을 시전할 수 있는지 확인한다.
         */
        public bool CanExecuteSelected(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            if (entry == null
                || runtime == null
                || !StatusCombatRules.CanAct(entry.Model))
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
            return runtime.CanCastWithSnapshot(snapshot);
        }

        /*
         * 자동 조준 방식으로 선택한 스킬의 실행을 요청한다.
         */
        public bool TryExecuteSelected(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */)
        {
            return TryRouteSkill(entry, runtime, roster, combatManager, false, default, false, default);
        }

        /*
         * Trigger가 전달한 목표 지점과 피해 배율로 스킬 실행을 요청한다.
         */
        public bool TryExecuteTriggered(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            Vector2 targetPoint /* 지정한 대상 위치 */,
            bool hasTargetPoint /* 보유 대상 위치 여부 */,
            float triggeredDamageMultiplier = 1f /* 트리거로 실행된 피해 배율 */,
            string triggerSourceSkillId = null /* 트리거 발생 원본 스킬 식별자 */)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            var aimDirection = default(Vector2);
            if (entry.Transform != null && hasTargetPoint)
            {
                aimDirection = targetPoint - (Vector2)entry.Transform.position;
            }
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
         * 일반 실행 요청에 현재 선택지 Snapshot을 적용하고 스킬 종류별 실행기로 전달한다.
         */
        private bool TryRouteSkill(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            bool hasManualAimDirection /* 보유 수동 조준 방향 여부 */,
            Vector2 manualAimDirection /* 수동 조준 방향 */,
            bool hasManualTargetPoint /* 보유 수동 대상 위치 여부 */,
            Vector2 manualTargetPoint /* 수동 대상 위치 */)
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
         * 완료된 스킬 시전 위치와 출처를 SkillTrigger에 전달한다.
         */
        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
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
         * Trigger 전용 피해 배율을 Snapshot에 적용한 뒤 스킬을 실행한다.
         */
        private bool TryExecuteTriggeredSkill(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            bool hasManualAimDirection /* 보유 수동 조준 방향 여부 */,
            Vector2 manualAimDirection /* 수동 조준 방향 */,
            bool hasManualTargetPoint /* 보유 수동 대상 위치 여부 */,
            Vector2 manualTargetPoint /* 수동 대상 위치 */,
            float triggeredDamageMultiplier /* 트리거로 실행된 피해 배율 */,
            string triggerSourceSkillId /* 트리거 발생 원본 스킬 식별자 */)
        {
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
         * 컴파일된 런타임 자료형에 맞는 스킬 실행기를 호출한다.
         */
        private static bool ExecuteSkill(
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillRuntimeData skillData /* 스킬 실행 데이터 */)
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

            throw new InvalidOperationException("Unsupported compiled skill data: " + skillData.GetType().Name);
        }
    }

}



/*
 * 스킬 효과와 패시브가 요구하는 선택지·패시브·상태 조건을 판정한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillRequirement
    {
        /*
         * 목록에 적힌 모든 Choice가 현재 Snapshot에 적용되었는지 확인한다.
         */
        internal static bool HasAllActiveChoices(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */, string choiceList /* 선택지 목록 */)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choiceId = choices[i].Trim();
                if (choiceId.Length > 0 && !snapshot.HasActiveChoice(choiceId))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * 목록에 적힌 Choice 중 하나라도 현재 Snapshot에 적용되었는지 확인한다.
         */
        internal static bool HasAnyActiveChoice(SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */, string choiceList /* 선택지 목록 */)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choiceId = choices[i].Trim();
                if (choiceId.Length > 0 && snapshot.HasActiveChoice(choiceId))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 목록에 적힌 모든 패시브를 유닛이 학습했는지 확인한다.
         */
        internal static bool HasAllLearnedPassives(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveList /* 패시브 목록 */)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i].Trim();
                if (passiveId.Length > 0 && !HasLearnedPassive(owner, passiveId))
                {
                    return false;
                }
            }

            return true;
        }

        /*
         * 목록에 적힌 패시브 중 하나라도 유닛이 학습했는지 확인한다.
         */
        internal static bool HasAnyLearnedPassive(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveList /* 패시브 목록 */)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i].Trim();
                if (passiveId.Length > 0 && HasLearnedPassive(owner, passiveId))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * Choice가 요구하는 시전자 상태 중첩 조건을 만족하는지 확인한다.
         */
        internal static bool MeetsSourceStatus(SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */, UnitCombatState owner /* 정보를 소유한 유닛 */)
        {
            return choice == null
                || HasSourceStatus(owner, choice.RequiredSourceStatusKind, choice.RequiredSourceStatusMinStacks);
        }

        /*
         * 시전자가 지정한 상태 또는 보호막 조건을 만족하는지 확인한다.
         */
        internal static bool HasSourceStatus(UnitCombatState owner /* 정보를 소유한 유닛 */, StatusEffectKind statusKind /* 상태 효과 종류 */, int minimumStacks /* 최소 중첩 수 */)
        {
            if (statusKind == StatusEffectKind.None)
            {
                return true;
            }

            if (statusKind == StatusEffectKind.Shield)
            {
                return owner != null && owner.Resources != null && owner.Resources.CurrentShield > 0f;
            }

            return owner != null
                && owner.Statuses != null
                && owner.Statuses.GetStacks(statusKind) >= Mathf.Max(1, minimumStacks);
        }

        /*
         * 유닛의 학습한 패시브 목록에 지정한 ID가 있는지 확인한다.
         */
        private static bool HasLearnedPassive(UnitCombatState owner /* 정보를 소유한 유닛 */, string passiveId /* 패시브 식별자 */)
        {
            return owner != null
                && owner.SkillProgress != null
                && owner.SkillProgress.LearnedPassiveSkillIds.Contains(passiveId);
        }
    }
}
