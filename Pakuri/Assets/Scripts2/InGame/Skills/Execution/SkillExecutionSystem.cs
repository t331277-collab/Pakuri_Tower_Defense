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
            if (runtime == null)
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry != null ? entry.Model : null, runtime);
            if (!runtime.CanCastWithSnapshot(snapshot))
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
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                if (!runtime.TryBeginCast(snapshot))
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

    public sealed class SkillExecutorRegistry
    {
        private readonly System.Collections.Generic.List<IInGameSkillExecutor> executors =
            new System.Collections.Generic.List<IInGameSkillExecutor>();

        public SkillExecutorRegistry()
        {
            RegisterDefaults();
        }

        public int Count => executors.Count;

        public void Register(IInGameSkillExecutor executor)
        {
            if (executor != null && !executors.Contains(executor))
            {
                executors.Add(executor);
            }
        }

        public bool TryResolve(SkillData skillData, out IInGameSkillExecutor executor)
        {
            executor = null;
            if (skillData == null)
            {
                return false;
            }

            for (var i = 0; i < executors.Count; i++)
            {
                if (executors[i] != null && executors[i].CanExecute(skillData))
                {
                    executor = executors[i];
                    return true;
                }
            }

            return false;
        }

        private void RegisterDefaults()
        {
            Register(new ProjectileSkillExecutor());
            Register(new BeamSkillExecutor());
            Register(new SingleAttackSkillExecutor());
            Register(new ZoneSkillExecutor());
            Register(new BuffSkillExecutor());
            Register(new ShieldSkillExecutor());
            Register(new PassiveSkillExecutor());
        }
    }

    public sealed class SkillChoiceResolver
    {
        private SkillChoiceModifierLibrary modifierLibrary = new SkillChoiceModifierLibrary();

        public int ModifierRecordCount => modifierLibrary != null ? modifierLibrary.Count : 0;

        public void SetModifierLibrary(SkillChoiceModifierLibrary library)
        {
            modifierLibrary = library ?? new SkillChoiceModifierLibrary();
        }

        public SkillExecutionSnapshot Resolve(BaseUnitRuntimeModel owner, SkillRuntimeInstance runtime)
        {
            var skillData = runtime != null ? runtime.Data : null;
            var snapshot = new SkillExecutionSnapshot(skillData);
            var monsterOwner = owner as MonsterUnitRuntimeModel;
            var chosenChoiceIds = monsterOwner != null && monsterOwner.State != null
                ? monsterOwner.State.ChosenChoiceIds
                : null;
            if (skillData == null || chosenChoiceIds == null || chosenChoiceIds.Count == 0)
            {
                return snapshot;
            }

            ApplyChoices(snapshot, chosenChoiceIds, skillData.EnhancementChoices);
            ApplyChoices(snapshot, chosenChoiceIds, skillData.MasterChoices);
            ApplyModifierRecords(snapshot, chosenChoiceIds, skillData);
            return snapshot;
        }

        private static void ApplyChoices(
            SkillExecutionSnapshot snapshot,
            System.Collections.Generic.ICollection<string> chosenChoiceIds,
            SkillChoiceEffectSpec[] choices)
        {
            if (snapshot == null || chosenChoiceIds == null || choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && chosenChoiceIds.Contains(choice.ChoiceId))
                {
                    snapshot.ApplyChoiceSpec(choice);
                }
            }
        }

        private void ApplyModifierRecords(
            SkillExecutionSnapshot snapshot,
            System.Collections.Generic.ICollection<string> chosenChoiceIds,
            SkillData skillData)
        {
            if (snapshot == null || chosenChoiceIds == null || modifierLibrary == null || skillData == null)
            {
                return;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                if (IsChoiceForSkill(choiceId, skillData)
                    && modifierLibrary.TryGet(choiceId, out var record))
                {
                    snapshot.ApplyModifierRecord(record);
                }
            }
        }

        private static bool IsChoiceForSkill(string choiceId, SkillData skillData)
        {
            if (string.IsNullOrWhiteSpace(choiceId) || skillData == null)
            {
                return false;
            }

            return ContainsChoice(skillData.EnhancementChoices, choiceId)
                || ContainsChoice(skillData.MasterChoices, choiceId);
        }

        private static bool ContainsChoice(SkillChoiceEffectSpec[] choices, string choiceId)
        {
            if (choices == null)
            {
                return false;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && string.Equals(choice.ChoiceId, choiceId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
