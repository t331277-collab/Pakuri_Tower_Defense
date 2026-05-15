using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class SkillExecutionSystem
    {
        public delegate bool SkillAutoRoutePredicate(UnitRosterEntry entry, SkillRuntimeInstance runtime);

        private readonly SkillExecutorRegistry registry = new SkillExecutorRegistry();
        private readonly SkillChoiceResolver choiceResolver = new SkillChoiceResolver();

        public int LastRoutedCount { get; private set; }
        public int LastRejectedCount { get; private set; }
        public int ModifierRecordCount => choiceResolver.ModifierRecordCount;

        public void SetChoiceModifierLibrary(SkillChoiceModifierLibrary library)
        {
            choiceResolver.SetModifierLibrary(library);
        }

        public void Tick(
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            SkillAutoRoutePredicate canAutoRoute = null)
        {
            LastRoutedCount = 0;
            LastRejectedCount = 0;
            if (roster == null || deltaTime <= 0f)
            {
                return;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                TickEntry(entries[i], roster, combatManager, deltaTime, logRoutedContracts, canAutoRoute);
            }
        }

        public bool TryExecuteManual(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            Vector2 aimDirection,
            bool logRoutedContracts)
        {
            return TryRouteSkill(
                entry,
                runtime,
                roster,
                combatManager,
                deltaTime,
                logRoutedContracts,
                true,
                aimDirection);
        }

        private void TickEntry(
            UnitRosterEntry entry,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            SkillAutoRoutePredicate canAutoRoute)
        {
            var model = entry != null ? entry.Model : null;
            var skillRuntime = model != null ? model.SkillRuntime : null;
            if (skillRuntime == null)
            {
                return;
            }

            skillRuntime.Tick(deltaTime);
            if (model == null || !model.AutoSkillEnabled || !entry.IsAlive)
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

                TryRouteSkill(entry, runtime, roster, combatManager, deltaTime, logRoutedContracts, false, default);
            }
        }

        private bool TryRouteSkill(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            bool hasManualAimDirection,
            Vector2 manualAimDirection)
        {
            if (runtime == null || !runtime.CanCast)
            {
                return false;
            }

            if (!registry.TryResolve(runtime.Data, out var executor))
            {
                LastRejectedCount++;
                return false;
            }

            var context = new SkillExecutionContext(
                combatManager,
                roster,
                entry,
                runtime,
                deltaTime,
                hasManualAimDirection,
                manualAimDirection);
            var snapshot = choiceResolver.Resolve(entry.Model, runtime);
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                if (!runtime.TryBeginCast())
                {
                    return false;
                }

                LastRoutedCount++;
                if (logRoutedContracts)
                {
                    Debug.Log($"Skill execution contract routed '{result.SkillId}' through {result.ExecutorName}.");
                }
            }

            return result.Routed;
        }
    }
}
