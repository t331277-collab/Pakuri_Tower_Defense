using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class SkillExecutionSystem
    {
        public delegate bool SkillAutoRoutePredicate(UnitRosterEntry entry, SkillRuntimeInstance runtime);

        private readonly SkillExecutorRegistry registry = new SkillExecutorRegistry();
        private readonly SkillChoiceResolver choiceResolver = new SkillChoiceResolver();
        private readonly Dictionary<UnitRosterEntry, UnitSkillController> unitControllers =
            new Dictionary<UnitRosterEntry, UnitSkillController>();
        private readonly List<UnitRosterEntry> staleControllerEntries = new List<UnitRosterEntry>();

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
            PruneControllerCache(entries);

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
            if (entry == null)
            {
                return false;
            }

            var controller = GetOrCreateController(entry);
            return controller.TryExecuteManual(
                runtime,
                roster,
                combatManager,
                deltaTime,
                aimDirection,
                targetPoint,
                logRoutedContracts);
        }

        public bool TryExecuteTriggered(
            UnitRosterEntry entry,
            SkillRuntimeInstance runtime,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            bool logRoutedContracts,
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
                logRoutedContracts,
                hasAimDirection,
                aimDirection,
                hasTargetPoint,
                targetPoint,
                triggeredDamageMultiplier,
                triggerSourceSkillId);
        }

        private void TickEntry(
            UnitRosterEntry entry,
            UnitRosterService roster,
            InGameCombatManager combatManager,
            float deltaTime,
            bool logRoutedContracts,
            SkillAutoRoutePredicate canAutoRoute)
        {
            if (entry == null)
            {
                return;
            }

            var controller = GetOrCreateController(entry);
            controller.Tick(roster, combatManager, deltaTime, logRoutedContracts, canAutoRoute);
        }

        private UnitSkillController GetOrCreateController(UnitRosterEntry entry)
        {
            if (!unitControllers.TryGetValue(entry, out var controller))
            {
                controller = new UnitSkillController(entry, TryRouteSkill);
                unitControllers.Add(entry, controller);
            }

            return controller;
        }

        private void PruneControllerCache(IReadOnlyList<UnitRosterEntry> activeEntries)
        {
            if (unitControllers.Count == 0)
            {
                return;
            }

            staleControllerEntries.Clear();
            foreach (var pair in unitControllers)
            {
                if (!ContainsEntry(activeEntries, pair.Key))
                {
                    staleControllerEntries.Add(pair.Key);
                }
            }

            for (var i = 0; i < staleControllerEntries.Count; i++)
            {
                unitControllers.Remove(staleControllerEntries[i]);
            }

            staleControllerEntries.Clear();
        }

        private static bool ContainsEntry(IReadOnlyList<UnitRosterEntry> entries, UnitRosterEntry candidate)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                if (ReferenceEquals(entries[i], candidate))
                {
                    return true;
                }
            }

            return false;
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
            Vector2 manualTargetPoint = default,
            System.Action<UnitRosterEntry> notifyActiveSkillAnimation = null)
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
                hasManualAimDirection: hasManualAimDirection,
                manualAimDirection: manualAimDirection,
                hasManualTargetPoint: hasManualTargetPoint,
                manualTargetPoint: manualTargetPoint);
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                if (!runtime.TryBeginCast(snapshot))
                {
                    return false;
                }

                notifyActiveSkillAnimation?.Invoke(entry);
                NotifySkillCastTriggers(combatManager, entry, runtime, context);
                LastRoutedCount++;
                if (logRoutedContracts)
                {
                    Debug.Log($"Skill execution contract routed '{result.SkillId}' through {result.ExecutorName}.");
                }
            }

            return result.Routed;
        }

        private static void NotifySkillCastTriggers(
            InGameCombatManager combatManager,
            UnitRosterEntry entry,
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
            combatManager.DispatchSkillCastTriggers(entry, runtime.Data.SkillId, center, triggerSourceSkillId);
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
                hasManualAimDirection: hasManualAimDirection,
                manualAimDirection: manualAimDirection,
                hasManualTargetPoint: hasManualTargetPoint,
                manualTargetPoint: manualTargetPoint);
            var result = executor.Execute(context, snapshot);
            if (result.Routed)
            {
                NotifySkillCastTriggers(combatManager, entry, runtime, context, triggerSourceSkillId);
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
            ApplyPassiveBaseModifiers(snapshot, owner as MonsterUnitRuntimeModel, skillData);
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

        private static void ApplyPassiveBaseModifiers(
            SkillExecutionSnapshot snapshot,
            MonsterUnitRuntimeModel owner,
            SkillData skillData)
        {
            if (snapshot == null
                || owner == null
                || owner.State == null
                || skillData == null
                || owner.State.LearnedPassiveSkillIds == null
                || owner.State.LearnedPassiveSkillIds.Count == 0)
            {
                return;
            }

            var manager = PakuriDataManager.Instance;
            foreach (var passiveId in owner.State.LearnedPassiveSkillIds)
            {
                if (manager == null
                    || !manager.TryGetData(passiveId, out PassiveDefinition passive)
                    || passive == null
                    || passive.BaseModifierChoices == null
                    || passive.BaseModifierChoices.Length == 0)
                {
                    continue;
                }

                for (var i = 0; i < passive.BaseModifierChoices.Length; i++)
                {
                    var modifier = passive.BaseModifierChoices[i];
                    if (modifier != null && AppliesToSkill(modifier, skillData))
                    {
                        snapshot.ApplyChoiceDefinition(modifier);
                    }
                }
            }
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
                    && AppliesToSkill(choice, skillData)
                    && MeetsSourceStatusRequirement(choice, owner))
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
                    return IsSkillTarget(self) ? new[] { self } : System.Array.Empty<UnitRosterEntry>();
                case SkillMultiEffectTargetSide.AllAllies:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Enemies : roster.Players);
                default:
                    return FilterSkillTargets(ownerIsEnemy ? roster.Players : roster.Enemies);
            }
        }

        private static System.Collections.Generic.IReadOnlyList<UnitRosterEntry> FilterSkillTargets(
            System.Collections.Generic.IReadOnlyList<UnitRosterEntry> entries)
        {
            if (entries == null || entries.Count == 0)
            {
                return System.Array.Empty<UnitRosterEntry>();
            }

            var filtered = new System.Collections.Generic.List<UnitRosterEntry>();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                if (!IsSkillTarget(entry))
                {
                    continue;
                }

                filtered.Add(entry);
            }

            return filtered;
        }

        private static bool IsSkillTarget(UnitRosterEntry entry)
        {
            var identity = entry != null && entry.Model != null ? entry.Model.Identity : null;
            return entry != null && (identity == null || identity.Role != UnitRole.Nexus);
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

        private static bool HasStatus(BaseUnitRuntimeModel model, string statusId, int minimumStacks = 1)
        {
            if (model == null || string.IsNullOrWhiteSpace(statusId) || minimumStacks <= 0)
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

            return model.Statuses != null && model.Statuses.GetStacks(kind) >= minimumStacks;
        }

        private static bool AppliesToSkill(SkillChoiceDefinition choice, SkillData skillData)
        {
            if (choice == null || skillData == null)
            {
                return false;
            }

            if (MatchesAnySkillId(choice.RuntimeTargetSkillIds, skillData.SkillId))
            {
                return true;
            }

            var targetSkillId = !string.IsNullOrWhiteSpace(choice.TargetSkillId)
                ? choice.TargetSkillId
                : choice.SkillId;
            return !string.IsNullOrWhiteSpace(targetSkillId)
                && string.Equals(targetSkillId, skillData.SkillId, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool MeetsSourceStatusRequirement(SkillChoiceDefinition choice, BaseUnitRuntimeModel owner)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.RequiredSourceStatusId))
            {
                return true;
            }

            return HasStatus(owner, choice.RequiredSourceStatusId, Mathf.Max(1, choice.RequiredSourceStatusMinStacks));
        }

        private static bool MatchesAnySkillId(string rawSkillIds, string skillId)
        {
            if (string.IsNullOrWhiteSpace(rawSkillIds) || string.IsNullOrWhiteSpace(skillId))
            {
                return false;
            }

            var split = rawSkillIds.Split(';', ',');
            for (var i = 0; i < split.Length; i++)
            {
                var candidate = split[i] != null ? split[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(candidate)
                    && string.Equals(candidate, skillId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
