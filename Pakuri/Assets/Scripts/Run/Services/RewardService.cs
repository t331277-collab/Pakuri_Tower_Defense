using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Spawn;

namespace Pakuri.NewCore.Run.Services
{
    public sealed class RewardResult
    {
        internal RewardResult(
            StageRewardDefinition definition,
            int gold,
            int darkTrace,
            IReadOnlyList<string> prisonerEnemyIds)
        {
            Definition = definition;
            Gold = gold;
            DarkTrace = darkTrace;
            PrisonerEnemyIds = prisonerEnemyIds;
        }

        public StageRewardDefinition Definition { get; }

        public int Gold { get; }

        public int DarkTrace { get; }

        public IReadOnlyList<string> PrisonerEnemyIds { get; }

        public float ManifestSuccessChance =>
            Definition.manifest_success_chance
            ?? throw new InvalidOperationException(
                "Reward definition has no manifest_success_chance.");
    }

    public sealed class RewardService
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly Func<int, int> randomIndex;
        private readonly Func<float> randomValue;

        public RewardService(
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
        }

        public RewardResult GenerateAndGrant(
            StageManager stage,
            SpawnManager spawns)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            if (spawns == null)
            {
                throw new ArgumentNullException(nameof(spawns));
            }

            if (!stage.OwnsSpawnManager(spawns))
            {
                throw new InvalidOperationException(
                    "Reward spawns do not belong to the active StageManager.");
            }

            return GenerateAndGrant(stage);
        }

        public RewardResult GenerateAndGrant(StageManager stage)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }

            SpawnManager spawns = stage.ActiveSpawnManager;
            if (stage.Session.RewardState
                != RewardProcessingState.Pending)
            {
                throw new InvalidOperationException(
                    "Combat must enter the pending reward state first.");
            }

            StageDayDefinition day = stage.CurrentDayDefinition
                ?? throw new InvalidOperationException(
                    "StageManager has no active day definition.");
            if (!catalog.StageRewards.TryGetValue(
                    day.reward_rule_id,
                    out StageRewardDefinition reward))
            {
                throw new InvalidOperationException(
                    $"Reward rule '{day.reward_rule_id}' does not exist.");
            }

            if (!string.Equals(
                    reward.combat_type,
                    day.combat_type,
                    StringComparison.Ordinal)
                || reward.stage != day.stage)
            {
                throw new InvalidOperationException(
                    "Reward rule does not match the active stage day.");
            }

            int gold = RequiredNonNegative(reward.gold, reward, "gold");
            int darkTrace = RequiredNonNegative(
                reward.dark_trace,
                reward,
                "dark_trace");
            checked
            {
                _ = stage.Gold + gold;
                _ = stage.DarkTrace + darkTrace;
            }

            int prisonerCount = ResolvePrisonerCount(reward);
            List<string> enemyIds = SelectPrisoners(
                reward,
                spawns.SpawnedEnemies,
                prisonerCount);

            stage.Session.BeginRewardProcessing();
            stage.AddGold(gold);
            stage.AddDarkTrace(darkTrace);
            stage.Session.PrisonerInventory.ReplaceRewards(enemyIds);
            return new RewardResult(
                reward,
                gold,
                darkTrace,
                new ReadOnlyCollection<string>(enemyIds));
        }

        private int ResolvePrisonerCount(
            StageRewardDefinition reward)
        {
            float one = RequiredProbability(
                reward.prisoner_count_1_chance,
                reward,
                "prisoner_count_1_chance");
            float two = RequiredProbability(
                reward.prisoner_count_2_chance,
                reward,
                "prisoner_count_2_chance");
            float three = RequiredProbability(
                reward.prisoner_count_3_chance,
                reward,
                "prisoner_count_3_chance");
            float total = one + two + three;
            if (Math.Abs(total - 1f) > 0.0001f)
            {
                throw Invalid(
                    reward,
                    "Prisoner count probabilities must sum to one.");
            }

            float roll = NextUnitValue();
            int count = roll < one
                ? 1
                : roll < one + two ? 2 : 3;
            if (string.Equals(
                    reward.combat_type,
                    "Elite",
                    StringComparison.Ordinal))
            {
                count += RequiredNonNegative(
                    reward.elite_bonus_prisoners,
                    reward,
                    "elite_bonus_prisoners");
            }

            return count;
        }

        private List<string> SelectPrisoners(
            StageRewardDefinition reward,
            IReadOnlyList<SpawnedEnemyRecord> spawned,
            int count)
        {
            if (string.IsNullOrWhiteSpace(
                    reward.guaranteed_prisoner_source))
            {
                throw Invalid(
                    reward,
                    "guaranteed_prisoner_source is required.");
            }

            if (spawned.Count < count)
            {
                throw new InvalidOperationException(
                    "The encounter spawned fewer enemies than the reward count.");
            }

            List<SpawnedEnemyRecord> pool =
                new List<SpawnedEnemyRecord>(spawned);
            List<string> result = new List<string>(count);
            List<SpawnedEnemyRecord> guaranteed =
                ResolveGuaranteedPool(
                    reward,
                    spawned);
            SpawnedEnemyRecord selectedGuaranteed =
                guaranteed[ResolveRandomIndex(guaranteed.Count)];
            result.Add(
                selectedGuaranteed.Model.EnemyDefinition.enemy_id);
            if (!pool.Remove(selectedGuaranteed))
            {
                throw new InvalidOperationException(
                    "Guaranteed prisoner is not in the encounter pool.");
            }

            while (result.Count < count)
            {
                int index = ResolveRandomIndex(pool.Count);
                result.Add(
                    pool[index].Model.EnemyDefinition.enemy_id);
                pool.RemoveAt(index);
            }

            return result;
        }

        private static List<SpawnedEnemyRecord> ResolveGuaranteedPool(
            StageRewardDefinition reward,
            IReadOnlyList<SpawnedEnemyRecord> spawned)
        {
            List<SpawnedEnemyRecord> result =
                new List<SpawnedEnemyRecord>();
            string source = reward.guaranteed_prisoner_source;
            for (int index = 0; index < spawned.Count; index++)
            {
                SpawnedEnemyRecord record = spawned[index];
                bool eligible;
                switch (source)
                {
                    case "EncounterBoss":
                        eligible = record.IsBoss
                            && record.Encounter.is_boss_candidate == true;
                        break;
                    case "GuaranteedBoss":
                    case "GuaranteedBossPool":
                        eligible = record.IsBoss
                            && record.Encounter.is_guaranteed_boss == true;
                        break;
                    default:
                        throw Invalid(
                            reward,
                            $"Unsupported guaranteed_prisoner_source '{source}'.");
                }

                if (eligible)
                {
                    result.Add(record);
                }
            }

            if (result.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Encounter has no spawned source for '{source}'.");
            }

            return result;
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

        private static int RequiredNonNegative(
            int? value,
            StageRewardDefinition reward,
            string column)
        {
            if (!value.HasValue || value.Value < 0)
            {
                throw Invalid(
                    reward,
                    column + " must be non-negative.");
            }

            return value.Value;
        }

        private static float RequiredProbability(
            float? value,
            StageRewardDefinition reward,
            string column)
        {
            if (!value.HasValue
                || value.Value < 0f
                || value.Value > 1f
                || float.IsNaN(value.Value)
                || float.IsInfinity(value.Value))
            {
                throw Invalid(
                    reward,
                    column + " must be a probability.");
            }

            return value.Value;
        }

        private static InvalidOperationException Invalid(
            StageRewardDefinition reward,
            string message)
        {
            return new InvalidOperationException(
                $"{reward.SourcePath} record {reward.SourceRecordNumber}: {message}");
        }
    }
}
