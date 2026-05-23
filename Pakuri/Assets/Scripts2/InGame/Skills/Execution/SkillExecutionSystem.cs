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
            Vector2 targetPoint,
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
                aimDirection,
                true,
                targetPoint);
        }

        public bool TryExecuteTriggered(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            bool logRoutedContracts,
            Vector2 targetPoint,
            bool hasTargetPoint)
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
                logRoutedContracts,
                hasAimDirection,
                aimDirection,
                hasTargetPoint,
                targetPoint);
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
            Vector2 manualAimDirection,
            bool hasManualTargetPoint = false,
            Vector2 manualTargetPoint = default)
        {
            if (runtime == null || entry == null || !StatusEffectRuntime.CanAct(entry.Model))
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry != null ? entry.Model : null, runtime, roster);
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
                manualAimDirection,
                hasManualTargetPoint,
                manualTargetPoint);
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

        private bool TryExecuteTriggeredSkill(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            bool logRoutedContracts,
            bool hasManualAimDirection,
            Vector2 manualAimDirection,
            bool hasManualTargetPoint,
            Vector2 manualTargetPoint)
        {
            if (runtime == null || entry == null)
            {
                return false;
            }

            var snapshot = choiceResolver.Resolve(entry.Model, runtime, roster);
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
                0f,
                hasManualAimDirection,
                manualAimDirection,
                hasManualTargetPoint,
                manualTargetPoint);
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                LastRoutedCount++;
                if (logRoutedContracts)
                {
                    Debug.Log($"Triggered skill execution routed '{result.SkillId}' through {result.ExecutorName}.");
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
            return Resolve(owner, runtime, null);
        }

        public SkillExecutionSnapshot Resolve(BaseUnitRuntimeModel owner, SkillRuntimeInstance runtime, UnitRosterService roster)
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

            ApplyChoices(snapshot, chosenChoiceIds, skillData, owner, roster);
            return snapshot;
        }

        private static void ApplyChoices(
            SkillExecutionSnapshot snapshot,
            System.Collections.Generic.ICollection<string> chosenChoiceIds,
            SkillData skillData,
            BaseUnitRuntimeModel owner,
            UnitRosterService roster)
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
                    ApplyDynamicChoiceRules(snapshot, choice, owner, roster);
                }
            }
        }

        private static void ApplyDynamicChoiceRules(
            SkillExecutionSnapshot snapshot,
            SkillChoiceDefinition choice,
            BaseUnitRuntimeModel owner,
            UnitRosterService roster)
        {
            if (snapshot == null
                || choice == null
                || string.IsNullOrWhiteSpace(choice.CountStatusId)
                || choice.DamageMultiplierPerCount <= 0f
                || roster == null)
            {
                return;
            }

            var count = CountMatchingTargets(owner, roster, choice.CountTargetSide, choice.CountStatusId);
            if (choice.CountMax > 0)
            {
                count = Mathf.Min(count, choice.CountMax);
            }

            if (count <= 0)
            {
                return;
            }

            snapshot.ApplyDynamicDamageMultiplier(1f + count * choice.DamageMultiplierPerCount);
        }

        private static int CountMatchingTargets(
            BaseUnitRuntimeModel owner,
            UnitRosterService roster,
            SkillMultiEffectTargetSide side,
            string statusId)
        {
            if (owner == null || roster == null || string.IsNullOrWhiteSpace(statusId))
            {
                return 0;
            }

            var entries = ResolveCountEntries(owner, roster, side);
            var count = 0;
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (entry == null || !entry.IsAlive || entry.Model == null)
                {
                    continue;
                }

                if (HasStatus(entry.Model, statusId))
                {
                    count++;
                }
            }

            return count;
        }

        private static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> ResolveCountEntries(
            BaseUnitRuntimeModel owner,
            UnitRosterService roster,
            SkillMultiEffectTargetSide side)
        {
            if (roster == null || owner == null || owner.Identity == null)
            {
                return System.Array.Empty<UnitRosterEntry>();
            }

            var ownerIsEnemy = owner.Identity.Side == UnitSide.Enemy;
            switch (side)
            {
                case SkillMultiEffectTargetSide.Self:
                    var self = FindEntryForModel(owner, ownerIsEnemy ? roster.Enemies : roster.Players);
                    return self != null ? new[] { self } : System.Array.Empty<UnitRosterEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    return ownerIsEnemy ? roster.Enemies : roster.Players;
                default:
                    return ownerIsEnemy ? roster.Players : roster.Enemies;
            }
        }

        private static UnitRosterEntry FindEntryForModel(
            BaseUnitRuntimeModel model,
            System.Collections.Generic.IReadOnlyList<UnitRosterEntry> entries)
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

        private static bool HasStatus(BaseUnitRuntimeModel model, string statusId)
        {
            if (model == null || string.IsNullOrWhiteSpace(statusId))
            {
                return false;
            }

            if (!StatusEffectUtility.TryParse(statusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return model.Resources != null && model.Resources.CurrentShield > 0f;
            }

            return model.Statuses != null && model.Statuses.Has(kind);
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
