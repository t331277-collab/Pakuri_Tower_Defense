using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Spawn
{
    public sealed class SpawnedEnemyRecord
    {
        internal SpawnedEnemyRecord(
            EnemyModel model,
            StageEncounterDefinition encounter,
            bool isBoss)
        {
            Model = model;
            Encounter = encounter;
            IsBoss = isBoss;
        }

        public EnemyModel Model { get; }

        public StageEncounterDefinition Encounter { get; }

        public bool IsBoss { get; }

        public bool GuaranteesPrisoner =>
            Encounter.guaranteed_prisoner == true;
    }

    public sealed class SpawnManager
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly Func<int, int> randomIndex;
        private readonly Func<float> randomValue;
        private readonly List<SpawnEntry> pending =
            new List<SpawnEntry>();
        private readonly List<SpawnedEnemyRecord> spawned =
            new List<SpawnedEnemyRecord>();
        private readonly IReadOnlyList<SpawnedEnemyRecord> readOnlySpawned;
        private StageManager stageManager;
        private int nextIndex;
        private float nextDelay;

        public SpawnManager(
            GameDefinitionCatalog catalog,
            Func<int, int> randomIndex,
            Func<float> randomValue)
        {
            this.catalog =
                catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.randomIndex =
                randomIndex ?? throw new ArgumentNullException(nameof(randomIndex));
            this.randomValue =
                randomValue ?? throw new ArgumentNullException(nameof(randomValue));
            readOnlySpawned =
                new ReadOnlyCollection<SpawnedEnemyRecord>(spawned);
        }

        public bool HasPendingSpawns => nextIndex < pending.Count;

        public IReadOnlyList<SpawnedEnemyRecord> SpawnedEnemies =>
            readOnlySpawned;

        public void BeginEncounter(
            StageManager stage,
            IReadOnlyList<StageEncounterDefinition> encounterRows)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            if (encounterRows == null || encounterRows.Count == 0)
            {
                throw new ArgumentException(
                    "Encounter rows are required.",
                    nameof(encounterRows));
            }

            stageManager = stage;
            pending.Clear();
            spawned.Clear();
            nextIndex = 0;

            List<StageEncounterDefinition> rows =
                new List<StageEncounterDefinition>(encounterRows);
            rows.Sort((left, right) =>
                Nullable.Compare(
                    left.spawn_order,
                    right.spawn_order));
            string encounterId = rows[0].encounter_id;
            List<int> bossCandidates = new List<int>();
            for (int index = 0; index < rows.Count; index++)
            {
                if (!string.Equals(
                        rows[index].encounter_id,
                        encounterId,
                        StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "Encounter rows must share one encounter id.",
                        nameof(encounterRows));
                }

                if (rows[index].is_boss_candidate == true)
                {
                    bossCandidates.Add(index);
                }
            }

            int selectedBossRow = bossCandidates.Count == 0
                ? -1
                : bossCandidates[
                    ResolveRandomIndex(bossCandidates.Count)];
            for (int rowIndex = 0;
                rowIndex < rows.Count;
                rowIndex++)
            {
                StageEncounterDefinition row = rows[rowIndex];
                int count = RequiredPositive(
                    row.count,
                    row,
                    "count");
                float interval = RequiredNonNegative(
                    row.interval_sec,
                    row,
                    "interval_sec");
                for (int instance = 0; instance < count; instance++)
                {
                    bool isBoss = row.is_guaranteed_boss == true
                        || (rowIndex == selectedBossRow
                            && instance == 0);
                    pending.Add(new SpawnEntry(
                        row,
                        isBoss,
                        pending.Count == 0 ? 0f : interval));
                }
            }

            nextDelay = pending[0].Delay;
        }

        public int Tick(float deltaTime)
        {
            ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (!HasPendingSpawns)
            {
                return 0;
            }

            nextDelay -= deltaTime;
            int created = 0;
            while (HasPendingSpawns && nextDelay <= 0f)
            {
                float carry = -nextDelay;
                SpawnEntry entry = pending[nextIndex++];
                Spawn(entry);
                created++;
                nextDelay = HasPendingSpawns
                    ? pending[nextIndex].Delay - carry
                    : 0f;
            }

            return created;
        }

        public MonsterModel CreateMonsterModel(
            MonsterDefinition definition,
            bool autoSkillEnabled)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            IEnumerable<SkillChoiceDefinition> passiveBases =
                catalog.Choices.Values
                    .Where(choice =>
                        string.Equals(
                            choice.monster_id,
                            definition.id,
                            StringComparison.Ordinal)
                        && string.Equals(
                            choice.choice_group,
                            "PassiveBase",
                            StringComparison.Ordinal));
            return new MonsterModel(
                definition,
                catalog.GetSkill(definition.id + "-a"),
                passiveBases,
                autoSkillEnabled);
        }

        public bool PlaceManifestedMonster(
            StageManager stage,
            MonsterModel monster)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            if (monster == null)
            {
                throw new ArgumentNullException(nameof(monster));
            }

            return stage.TryRegisterFieldUnit(monster);
        }

        private void Spawn(SpawnEntry entry)
        {
            EnemyDefinition definition =
                catalog.GetEnemy(entry.Encounter.enemy_id);
            float multiplier = entry.IsBoss
                ? ResolveBossHealthMultiplier(entry.Encounter)
                : 1f;
            EnemyModel model = new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(
                    definition.passive_id),
                multiplier);
            float x = RequiredFinite(
                entry.Encounter.spawn_x,
                entry.Encounter,
                "spawn_x");
            float yMinimum = RequiredFinite(
                entry.Encounter.spawn_y_min,
                entry.Encounter,
                "spawn_y_min");
            float yMaximum = RequiredFinite(
                entry.Encounter.spawn_y_max,
                entry.Encounter,
                "spawn_y_max");
            if (yMaximum < yMinimum)
            {
                throw Invalid(
                    entry.Encounter,
                    "spawn_y_max is less than spawn_y_min.");
            }

            model.SetPosition(new CombatVector2(
                x,
                yMinimum
                    + ((yMaximum - yMinimum) * NextUnitValue())));
            if (!stageManager.TryRegisterFieldUnit(model))
            {
                throw new InvalidOperationException(
                    "Spawned enemy registration failed.");
            }

            spawned.Add(new SpawnedEnemyRecord(
                model,
                entry.Encounter,
                entry.IsBoss));
        }

        private float ResolveBossHealthMultiplier(
            StageEncounterDefinition encounter)
        {
            float minimum = RequiredPositive(
                encounter.boss_health_multiplier_min,
                encounter,
                "boss_health_multiplier_min");
            float maximum = RequiredPositive(
                encounter.boss_health_multiplier_max,
                encounter,
                "boss_health_multiplier_max");
            if (maximum < minimum)
            {
                throw Invalid(
                    encounter,
                    "boss_health_multiplier_max is less than minimum.");
            }

            return minimum + ((maximum - minimum) * NextUnitValue());
        }

        private int ResolveRandomIndex(int count)
        {
            int index = randomIndex(count);
            if (index < 0 || index >= count)
            {
                throw new InvalidOperationException(
                    "The random index source returned an invalid index.");
            }

            return index;
        }

        private float NextUnitValue()
        {
            float value = randomValue();
            if (value < 0f
                || value > 1f
                || float.IsNaN(value)
                || float.IsInfinity(value))
            {
                throw new InvalidOperationException(
                    "The random value source must return [0, 1].");
            }

            return value;
        }

        private static int RequiredPositive(
            int? value,
            StageEncounterDefinition row,
            string column)
        {
            if (!value.HasValue || value.Value <= 0)
            {
                throw Invalid(row, column + " must be positive.");
            }

            return value.Value;
        }

        private static float RequiredPositive(
            float? value,
            StageEncounterDefinition row,
            string column)
        {
            float result = RequiredFinite(value, row, column);
            if (result <= 0f)
            {
                throw Invalid(row, column + " must be positive.");
            }

            return result;
        }

        private static float RequiredNonNegative(
            float? value,
            StageEncounterDefinition row,
            string column)
        {
            float result = RequiredFinite(value, row, column);
            if (result < 0f)
            {
                throw Invalid(row, column + " cannot be negative.");
            }

            return result;
        }

        private static float RequiredFinite(
            float? value,
            StageEncounterDefinition row,
            string column)
        {
            if (!value.HasValue
                || float.IsNaN(value.Value)
                || float.IsInfinity(value.Value))
            {
                throw Invalid(row, column + " must be finite.");
            }

            return value.Value;
        }

        private static InvalidOperationException Invalid(
            StageEncounterDefinition row,
            string message)
        {
            return new InvalidOperationException(
                $"{row.SourcePath} record {row.SourceRecordNumber}: {message}");
        }

        private static void ValidateNonNegativeFinite(
            float value,
            string parameterName)
        {
            if (value < 0f
                || float.IsNaN(value)
                || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private sealed class SpawnEntry
        {
            public SpawnEntry(
                StageEncounterDefinition encounter,
                bool isBoss,
                float delay)
            {
                Encounter = encounter;
                IsBoss = isBoss;
                Delay = delay;
            }

            public StageEncounterDefinition Encounter { get; }

            public bool IsBoss { get; }

            public float Delay { get; }
        }
    }
}
