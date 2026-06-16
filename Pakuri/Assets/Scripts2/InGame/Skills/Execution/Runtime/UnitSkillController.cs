using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class UnitSkillController
    {
        public delegate bool SkillRouteRequest(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint,
            System.Action<UnitRosterEntry> notifyActiveSkillAnimation);

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

                routeSkill(
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
            return routeSkill(
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
