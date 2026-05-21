using Pakuri.Data;
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
        public int ModifierRecordCount => 0;

        public void SetChoiceModifierLibrary(SkillChoiceModifierLibrary library)
        {
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
            if (runtime == null || entry == null || !StatusEffectRuntime.CanAct(entry.Model))
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

            ApplyChoices(snapshot, chosenChoiceIds, skillData);
            return snapshot;
        }

        private static void ApplyChoices(
            SkillExecutionSnapshot snapshot,
            System.Collections.Generic.ICollection<string> chosenChoiceIds,
            SkillData skillData)
        {
            if (snapshot == null || chosenChoiceIds == null || skillData == null)
            {
                return;
            }

            var manager = PakuriDataManager.Instance;
            foreach (var choiceId in chosenChoiceIds)
            {
                if (manager != null
                    && manager.TryGetData(choiceId, out SkillChoiceDefinition choice)
                    && AppliesToSkill(choice, skillData))
                {
                    snapshot.AddActiveChoiceId(choice.ChoiceId);
                    snapshot.ApplyChoiceDefinition(choice);
                }
            }
        }

        private static bool AppliesToSkill(SkillChoiceDefinition choice, SkillData skillData)
        {
            if (choice == null || skillData == null)
            {
                return false;
            }

            var targetSkillId = !string.IsNullOrWhiteSpace(choice.TargetSkillId)
                ? choice.TargetSkillId
                : choice.SkillId;
            return !string.IsNullOrWhiteSpace(targetSkillId)
                && string.Equals(targetSkillId, skillData.SkillId, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
