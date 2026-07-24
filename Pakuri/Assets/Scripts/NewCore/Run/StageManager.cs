using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Run
{
    public sealed class StageManager
    {
        private readonly List<UnitBaseModel> fieldUnits =
            new List<UnitBaseModel>();
        private readonly IReadOnlyList<UnitBaseModel> readOnlyFieldUnits;
        private readonly GameDefinitionCatalog catalog;
        private SpawnManager spawnManager;
        private InGameCombatManager combatManager;
        private NexusModel nexus;
        private int gold;
        private int darkTrace;

        public StageManager(
            RunSessionModel session,
            int initialGold,
            int initialDarkTrace)
            : this(
                session,
                null,
                initialGold,
                initialDarkTrace)
        {
        }

        public StageManager(
            RunSessionModel session,
            GameDefinitionCatalog catalog,
            int initialGold,
            int initialDarkTrace)
        {
            Session =
                session ?? throw new ArgumentNullException(nameof(session));
            if (initialGold < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialGold));
            }

            if (initialDarkTrace < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(initialDarkTrace));
            }

            this.catalog = catalog;
            gold = initialGold;
            darkTrace = initialDarkTrace;
            readOnlyFieldUnits =
                new ReadOnlyCollection<UnitBaseModel>(fieldUnits);
        }

        public RunSessionModel Session { get; }

        public int Gold => gold;

        public int DarkTrace => darkTrace;

        public bool IsCombatActive { get; private set; }

        public StageDayDefinition CurrentDayDefinition { get; private set; }

        public IReadOnlyList<UnitBaseModel> FieldUnits =>
            readOnlyFieldUnits;

        public IReadOnlyList<UnitBaseModel> LivingFieldUnits
        {
            get
            {
                List<UnitBaseModel> living =
                    new List<UnitBaseModel>();
                for (int index = 0; index < fieldUnits.Count; index++)
                {
                    if (fieldUnits[index].IsAlive)
                    {
                        living.Add(fieldUnits[index]);
                    }
                }

                return living.AsReadOnly();
            }
        }

        public event Action<bool> CombatResolved;

        public void AddGold(int amount)
        {
            gold = AddCurrency(gold, amount, nameof(amount));
        }

        public bool CanSpendGold(int amount)
        {
            return CanSpend(gold, amount);
        }

        public bool SpendGold(int amount)
        {
            if (!CanSpendGold(amount))
            {
                return false;
            }

            gold -= amount;
            return true;
        }

        public void AddDarkTrace(int amount)
        {
            darkTrace = AddCurrency(
                darkTrace,
                amount,
                nameof(amount));
        }

        public bool CanSpendDarkTrace(int amount)
        {
            return CanSpend(darkTrace, amount);
        }

        public bool SpendDarkTrace(int amount)
        {
            if (!CanSpendDarkTrace(amount))
            {
                return false;
            }

            darkTrace -= amount;
            return true;
        }

        public bool TryRegisterFieldUnit(UnitBaseModel unit)
        {
            if (unit == null || fieldUnits.Contains(unit))
            {
                return false;
            }

            fieldUnits.Add(unit);
            return true;
        }

        public bool TryUnregisterFieldUnit(UnitBaseModel unit)
        {
            return unit != null && fieldUnits.Remove(unit);
        }

        public void ClearFieldUnits()
        {
            fieldUnits.Clear();
        }

        public void ConfigureSpawnManager(SpawnManager manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (spawnManager != null
                && !ReferenceEquals(spawnManager, manager))
            {
                throw new InvalidOperationException(
                    "A SpawnManager is already configured.");
            }

            spawnManager = manager;
        }

        public void ConnectCombat(
            InGameCombatManager manager,
            NexusModel nexusModel)
        {
            if (manager == null)
            {
                throw new ArgumentNullException(nameof(manager));
            }

            if (nexusModel == null)
            {
                throw new ArgumentNullException(nameof(nexusModel));
            }

            DisconnectCombat();
            combatManager = manager;
            nexus = nexusModel;
            combatManager.UnitDefeated += HandleUnitDefeated;
        }

        public void DisconnectCombat()
        {
            if (combatManager != null)
            {
                combatManager.UnitDefeated -= HandleUnitDefeated;
            }

            combatManager = null;
            nexus = null;
        }

        public void StartCurrentDay()
        {
            RequireProgression();
            if (Session.Result != RunResult.Active)
            {
                throw new InvalidOperationException(
                    "A completed run cannot start another day.");
            }

            if (IsCombatActive)
            {
                throw new InvalidOperationException(
                    "The current combat is already active.");
            }

            if (Session.RewardState != RewardProcessingState.None)
            {
                throw new InvalidOperationException(
                    "The current reward must be completed before starting a day.");
            }

            CurrentDayDefinition = FindCurrentDay();
            Session.BeginDay(CurrentDayDefinition);
            PrepareFieldForDay();
            spawnManager.BeginEncounter(
                this,
                GetEncounterRows(
                    CurrentDayDefinition.encounter_id));
            IsCombatActive = true;
            spawnManager.Tick(0f);
            EvaluateCombatCompletion();
        }

        public void TickSpawnSequence(float deltaTime)
        {
            if (!IsCombatActive)
            {
                return;
            }

            spawnManager.Tick(deltaTime);
            EvaluateCombatCompletion();
        }

        public void EvaluateCombatCompletion()
        {
            if (!IsCombatActive)
            {
                return;
            }

            if (nexus != null && !nexus.IsAlive)
            {
                IsCombatActive = false;
                Session.MarkDefeat();
                CombatResolved?.Invoke(false);
                return;
            }

            if (spawnManager.HasPendingSpawns
                || HasLivingEnemy())
            {
                return;
            }

            IsCombatActive = false;
            Session.BeginReward();
            CombatResolved?.Invoke(true);
        }

        public bool CompleteRewardAndAdvance()
        {
            RequireProgression();
            Session.CompleteReward();
            Session.PrisonerInventory.Clear();

            StageDayDefinition next = FindNextDay();
            if (next == null)
            {
                Session.MarkVictory();
                return false;
            }

            Session.BeginDay(next);
            StartCurrentDay();
            return true;
        }

        public bool PlaceManifestedMonster(MonsterModel monster)
        {
            if (spawnManager == null)
            {
                throw new InvalidOperationException(
                    "SpawnManager is not configured.");
            }

            return spawnManager.PlaceManifestedMonster(this, monster);
        }

        internal bool OwnsSpawnManager(SpawnManager manager)
        {
            return manager != null
                && ReferenceEquals(spawnManager, manager);
        }

        internal SpawnManager ActiveSpawnManager =>
            spawnManager
            ?? throw new InvalidOperationException(
                "SpawnManager is not configured.");

        private void PrepareFieldForDay()
        {
            fieldUnits.Clear();
            for (int index = 0;
                index < Session.PartyRoster.Members.Count;
                index++)
            {
                MonsterModel monster =
                    Session.PartyRoster.Members[index];
                monster.ResetForNextDay(index == 0);
                fieldUnits.Add(monster);
            }

            if (nexus != null)
            {
                fieldUnits.Add(nexus);
            }
        }

        private bool HasLivingEnemy()
        {
            for (int index = 0; index < fieldUnits.Count; index++)
            {
                if (fieldUnits[index] is EnemyModel
                    && fieldUnits[index].IsAlive)
                {
                    return true;
                }
            }

            return false;
        }

        private StageDayDefinition FindCurrentDay()
        {
            StageDayDefinition found = null;
            foreach (StageDayDefinition day in catalog.StageDays.Values)
            {
                if (day.day == Session.CurrentDay
                    && string.Equals(
                        day.encounter_id,
                        Session.CurrentEncounterId,
                        StringComparison.Ordinal))
                {
                    if (found != null)
                    {
                        throw new InvalidOperationException(
                            "The current run location is ambiguous.");
                    }

                    found = day;
                }
            }

            return found
                ?? throw new InvalidOperationException(
                    "The current run location has no StageDay definition.");
        }

        private StageDayDefinition FindNextDay()
        {
            int stage = CurrentDayDefinition.stage
                ?? throw new InvalidOperationException(
                    "Current StageDay has no stage.");
            int dayNumber = CurrentDayDefinition.day
                ?? throw new InvalidOperationException(
                    "Current StageDay has no day.");
            StageDayDefinition sameStage = FindDay(stage, dayNumber + 1);
            return sameStage ?? FindDay(stage + 1, 1);
        }

        private StageDayDefinition FindDay(int stage, int day)
        {
            StageDayDefinition found = null;
            foreach (StageDayDefinition candidate
                in catalog.StageDays.Values)
            {
                if (candidate.stage != stage
                    || candidate.day != day)
                {
                    continue;
                }

                if (found != null)
                {
                    throw new InvalidOperationException(
                        "Duplicate stage/day progression key.");
                }

                found = candidate;
            }

            return found;
        }

        private IReadOnlyList<StageEncounterDefinition> GetEncounterRows(
            string encounterId)
        {
            List<StageEncounterDefinition> rows =
                new List<StageEncounterDefinition>();
            for (int index = 0;
                index < catalog.StageEncounters.Count;
                index++)
            {
                StageEncounterDefinition row =
                    catalog.StageEncounters[index];
                if (string.Equals(
                    row.encounter_id,
                    encounterId,
                    StringComparison.Ordinal))
                {
                    rows.Add(row);
                }
            }

            if (rows.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Encounter '{encounterId}' has no spawn rows.");
            }

            rows.Sort((left, right) =>
                Nullable.Compare(
                    left.spawn_order,
                    right.spawn_order));
            return rows.AsReadOnly();
        }

        private void HandleUnitDefeated(UnitBaseModel unit)
        {
            EvaluateCombatCompletion();
        }

        private void RequireProgression()
        {
            if (catalog == null)
            {
                throw new InvalidOperationException(
                    "GameDefinitionCatalog is not configured.");
            }

            if (spawnManager == null)
            {
                throw new InvalidOperationException(
                    "SpawnManager is not configured.");
            }
        }

        private static int AddCurrency(
            int current,
            int amount,
            string parameterName)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return checked(current + amount);
        }

        private static bool CanSpend(int current, int amount)
        {
            return amount >= 0 && current >= amount;
        }
    }
}
