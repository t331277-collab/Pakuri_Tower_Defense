using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class UnitSkillController
    {
        public delegate bool SkillRouteRequest(SkillExecutionRequest request);

        private readonly UnitRosterEntry entry;
        private readonly SkillRouteRequest routeSkill;

        public UnitSkillController(
            UnitRosterEntry entry,
            SkillRouteRequest routeSkill)
        {
            this.entry = entry;
            this.routeSkill = routeSkill;
        }

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
            if (model == null || !model.AutoSkillEnabled || !entry.IsAlive || !StatusEffectRuntime.CanAct(model))
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

        private void NotifyActiveSkillAnimation(UnitRosterEntry routedEntry)
        {
            var monsterActor = routedEntry != null ? routedEntry.Actor as MonsterUnitActor : null;
            monsterActor?.TryPlayActiveSkillAnimation();
        }
    }
}
