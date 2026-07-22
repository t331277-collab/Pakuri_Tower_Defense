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
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
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
        public SkillUseState Runtime { get; }
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
     * 자동·수동·Trigger 실행 요청을 판정하고 준비된 정보를 스킬 종류별 실행기로 전달한다.
     */
    public class SkillExecution
    {
        /*
         * 자동 시전 요청을 실행기로 전달해도 되는지 판단하는 함수 형식을 정의한다.
         */
        public delegate bool SkillAutoRoutePredicate(CombatUnitEntry entry /* 처리할 등록 정보 */, SkillUseState runtime /* 실행 중인 스킬 정보 */);

        /*
         * 자동 실행이 허용된 유닛의 액티브 스킬 실행을 요청한다.
         */
        public void TryExecuteAutomaticSkills(
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
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

                var activeSkills = model.Skills.ActiveSkills;
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
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
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
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            if (entry == null
                || runtime == null
                || !StatusCombatRules.CanAct(entry.Model))
            {
                return false;
            }

            var snapshot = entry.Model.Skills.CreateSnapshot(entry.Model, runtime, roster);
            return runtime.CanCastWithSnapshot(snapshot);
        }

        /*
         * 자동 조준 방식으로 선택한 스킬의 실행을 요청한다.
         */
        public bool TryExecuteSelected(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
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

        /*
         * Trigger가 전달한 목표 지점과 피해 배율로 스킬 실행을 요청한다.
         */
        public bool TryExecuteTriggered(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
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
            return TryExecuteSkill(
                entry,
                runtime,
                roster,
                combatManager,
                hasAimDirection,
                aimDirection,
                hasTargetPoint,
                targetPoint,
                false,
                triggeredDamageMultiplier,
                triggerSourceSkillId);
        }

        /*
         * 일반·수동·Trigger 요청의 Snapshot과 실행 정보를 준비해 스킬 종류별 실행기로 전달한다.
         */
        private bool TryExecuteSkill(
            CombatUnitEntry entry /* 처리할 등록 정보 */,
            SkillUseState runtime /* 실행 중인 스킬 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
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

            var snapshot = entry.Model.Skills.CreateSnapshot(entry.Model, runtime, roster);
            if (!Mathf.Approximately(damageMultiplier, 1f))
            {
                snapshot.ApplyDynamicDamageMultiplier(damageMultiplier);
            }

            if (beginCast && !runtime.CanCastWithSnapshot(snapshot))
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
                if (beginCast && !runtime.TryBeginCast(snapshot))
                {
                    return false;
                }

                var monsterActor = entry.Actor as MonsterActor;
                if (beginCast && monsterActor != null)
                {
                    monsterActor.TryPlayActiveSkillAnimation();
                }

                NotifySkillCastTriggers(combatManager, roster, entry, runtime, context, triggerSourceSkillId);
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
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillExecutionDefinition skillData /* 스킬 실행 데이터 */)
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
    }

}
