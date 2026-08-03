using System.Collections.Generic;
using UnityEngine;
using static Pakuri.Data.CsvParser;

namespace Pakuri.Data
{
    internal static class StageDefinitionBuilder
    {
        internal static StageDefinition Build(CsvRuntimeCatalog sourceCatalog)
        {
            return new StageDefinition
            {
                Days = LoadDays(sourceCatalog.StageDay),
                Encounters = LoadEncounters(sourceCatalog.Stage1Encounter, sourceCatalog.Stage2Encounter),
                Rewards = LoadRewards(sourceCatalog.Stage1Reward, sourceCatalog.Stage2Reward)
            };
        }

        private static StageDayDefinition[] LoadDays(TextAsset csv)
        {
            var rows = new List<StageDayDefinition>();
            foreach (var record in CsvTable.Load(csv, StageFileNames.StageDay).Records)
            {
                rows.Add(new StageDayDefinition
                {
                    Stage = record.ReadInt("stage"),
                    Day = record.ReadInt("day"),
                    DayKey = record.ReadRequiredString("day_key"),
                    CombatType = record.ReadRequiredString("combat_type"),
                    EncounterId = record.ReadRequiredString("encounter_id"),
                    RewardRuleId = record.ReadRequiredString("reward_rule_id"),
                    EliteOptionChance = record.ReadFloat("elite_option_chance"),
                    ShopOptionEnabled = record.ReadBool("shop_option_enabled"),
                    EventRollEnabled = record.ReadBool("event_roll_enabled"),
                    Notes = record.ReadString("notes")
                });
            }

            return rows.ToArray();
        }

        private static StageEncounterDefinition[] LoadEncounters(TextAsset stage1Csv, TextAsset stage2Csv)
        {
            var rows = new List<StageEncounterDefinition>();
            LoadEncounterRows(rows, stage1Csv);
            LoadEncounterRows(rows, stage2Csv);
            return rows.ToArray();
        }

        private static void LoadEncounterRows(List<StageEncounterDefinition> rows, TextAsset csv)
        {
            foreach (var record in CsvTable.Load(csv, StageFileNames.StageEncounter).Records)
            {
                rows.Add(new StageEncounterDefinition
                {
                    EncounterId = record.ReadRequiredString("encounter_id"),
                    SpawnOrder = record.ReadInt("spawn_order"),
                    EnemyId = record.ReadRequiredString("enemy_id"),
                    Count = record.ReadInt("count"),
                    IntervalSeconds = record.ReadFloat("interval_sec"),
                    SpawnX = record.ReadFloat("spawn_x"),
                    SpawnYMin = record.ReadFloat("spawn_y_min"),
                    SpawnYMax = record.ReadFloat("spawn_y_max"),
                    IsBossCandidate = record.ReadBool("is_boss_candidate"),
                    IsGuaranteedBoss = record.ReadBool("is_guaranteed_boss"),
                    BossHealthMultiplierMin = record.ReadFloat("boss_health_multiplier_min"),
                    BossHealthMultiplierMax = record.ReadFloat("boss_health_multiplier_max"),
                    GuaranteedPrisoner = record.ReadBool("guaranteed_prisoner"),
                    Notes = record.ReadString("notes")
                });
            }
        }

        private static StageRewardDefinition[] LoadRewards(TextAsset stage1Csv, TextAsset stage2Csv)
        {
            var rows = new List<StageRewardDefinition>();
            LoadRewardRows(rows, stage1Csv);
            LoadRewardRows(rows, stage2Csv);
            return rows.ToArray();
        }

        private static void LoadRewardRows(List<StageRewardDefinition> rows, TextAsset csv)
        {
            foreach (var record in CsvTable.Load(csv, StageFileNames.StageReward).Records)
            {
                rows.Add(new StageRewardDefinition
                {
                    RewardRuleId = record.ReadRequiredString("reward_rule_id"),
                    CombatType = record.ReadRequiredString("combat_type"),
                    Stage = record.ReadInt("stage"),
                    Gold = record.ReadInt("gold"),
                    DarkTrace = record.ReadInt("dark_trace"),
                    PrisonerCount1Chance = record.ReadFloat("prisoner_count_1_chance"),
                    PrisonerCount2Chance = record.ReadFloat("prisoner_count_2_chance"),
                    PrisonerCount3Chance = record.ReadFloat("prisoner_count_3_chance"),
                    ManifestSuccessChance = record.ReadFloat("manifest_success_chance"),
                    EliteBonusPrisoners = record.ReadInt("elite_bonus_prisoners"),
                    ArtifactChoiceCount = record.ReadInt("artifact_choice_count"),
                    GuaranteedPrisonerSource = record.ReadRequiredString("guaranteed_prisoner_source"),
                    Notes = record.ReadString("notes")
                });
            }
        }
    }

    internal static class StageFileNames
    {
        internal const string StageDay = "StageDay.csv";
        internal const string StageEncounter = "StageEncounter.csv";
        internal const string StageReward = "StageReward.csv";
    }
}
