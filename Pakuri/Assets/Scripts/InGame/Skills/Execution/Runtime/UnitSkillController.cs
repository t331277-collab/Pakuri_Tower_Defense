using UnityEngine;

/*
 * 유닛 하나의 스킬 Tick과 자동·수동 실행 요청을 만드는 컨트롤러.
 * 스킬 사용 가능 여부와 선택 대상을 확인해 SkillExecutionSystem으로 요청을 보내고
 * 실행이 시작되면 해당 Actor의 공격 애니메이션을 알린다.
 */
namespace Pakuri.InGame
{
    public sealed class UnitSkillController
    {
        /*
         * 스킬 전달 요청 호출 형식을 정의한다.
         */
        public delegate bool SkillRouteRequest(SkillExecutionRequest request);

        private readonly UnitRosterEntry entry;
        private readonly SkillRouteRequest routeSkill;

        /*
         * 유닛 스킬 컨트롤러에 필요한 값을 초기화한다.
         */
        public UnitSkillController(
            UnitRosterEntry entry,
            SkillRouteRequest routeSkill)
        {
            this.entry = entry;
            this.routeSkill = routeSkill;
        }

        /*
         * 유닛의 스킬 시간과 자동 시전을 갱신한다.
         */
        public void Tick(
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            SkillExecutionSystem.SkillAutoRoutePredicate canAutoRoute)
        {
            var model = entry != null ? entry.Model : null;
            var skillRuntime = model != null ? model.SkillRuntime : null;
            if (skillRuntime == null)
            {
                return;
            }

            skillRuntime.Tick(deltaTime);
            if (model == null || !model.AutoSkillEnabled || !entry.IsAlive || !StatusEffectRules.CanAct(model))
            {
                return;
            }

            var activeSkills = skillRuntime.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (canAutoRoute != null && !canAutoRoute(entry, runtime))
                {
                    continue;
                }

                routeSkill(CreateAutoRequest(runtime, roster, combatManager, deltaTime, logRoutedContracts));
            }
        }

        /*
         * 수동을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteManual(
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            Vector2 aimDirection,
            Vector2 targetPoint,
            bool logRoutedContracts)
        {
            return routeSkill(CreateManualRequest(
                runtime,
                roster,
                combatManager,
                deltaTime,
                aimDirection,
                targetPoint,
                logRoutedContracts));
        }

        /*
         * 선택된을 실행하고 성공 여부를 반환한다.
         */
        public bool TryExecuteSelected(
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts)
        {
            return routeSkill(CreateAutoRequest(
                runtime,
                roster,
                combatManager,
                deltaTime,
                logRoutedContracts));
        }

        /*
         * 자동 요청을 생성한다.
         */
        private SkillExecutionRequest CreateAutoRequest(
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts)
        {
            return new SkillExecutionRequest(
                entry,
                runtime,
                roster,
                combatManager,
                deltaTime,
                logRoutedContracts,
                false,
                default,
                false,
                default,
                NotifyActiveSkillAnimation);
        }

        /*
         * 수동 요청을 생성한다.
         */
        private SkillExecutionRequest CreateManualRequest(
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            Vector2 aimDirection,
            Vector2 targetPoint,
            bool logRoutedContracts)
        {
            return new SkillExecutionRequest(
                entry,
                runtime,
                roster,
                combatManager,
                deltaTime,
                logRoutedContracts,
                true,
                aimDirection,
                true,
                targetPoint,
                NotifyActiveSkillAnimation);
        }

        /*
         * 활성 스킬 애니메이션을 변경 사실을 전달한다.
         */
        private void NotifyActiveSkillAnimation(UnitRosterEntry routedEntry)
        {
            var monsterActor = routedEntry != null ? routedEntry.Actor as MonsterUnitActor : null;
            monsterActor?.TryPlayActiveSkillAnimation();
        }
    }
}
