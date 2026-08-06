using System;

namespace Pakuri.Data
{
    [Serializable]
    public sealed class StageDefinition
    {
        public StageDayDefinition[] Days = Array.Empty<StageDayDefinition>();
        public StageEncounterDefinition[] Encounters = Array.Empty<StageEncounterDefinition>();
        public StageRewardDefinition[] Rewards = Array.Empty<StageRewardDefinition>();

        public StageDayDefinition FindDay(int stage, int day)
        {
            for (var i = 0; i < Days.Length; i++)
            {
                var row = Days[i];
                if (row.Stage == stage && row.Day == day)
                {
                    return row;
                }
            }

            return null;
        }

        public StageRewardDefinition FindReward(string rewardRuleName)
        {
            for (var i = 0; i < Rewards.Length; i++)
            {
                var row = Rewards[i];
                if (string.Equals(row.RewardRuleName, rewardRuleName, StringComparison.OrdinalIgnoreCase))
                {
                    return row;
                }
            }

            return null;
        }

        public void FindEncounterRows(string encounterName, System.Collections.Generic.List<StageEncounterDefinition> results)
        {
            results.Clear();

            for (var i = 0; i < Encounters.Length; i++)
            {
                var row = Encounters[i];
                if (string.Equals(row.EncounterName, encounterName, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(row);
                }
            }

            results.Sort((left, right) => left.SpawnOrder.CompareTo(right.SpawnOrder));
        }
    }

    [Serializable]
    public sealed class StageDayDefinition
    {
        public int Stage;
        public int Day;
        public string DayKey;
        public string CombatType;
        public string EncounterName;
        public string RewardRuleName;
        public float EliteOptionChance;
        public bool ShopOptionEnabled;
        public bool EventRollEnabled;
        public string Notes;
    }

    [Serializable]
    public sealed class StageEncounterDefinition
    {
        public string EncounterName;
        public int SpawnOrder;
        public string EnemyName;
        public int Count;
        public float IntervalSeconds;
        public float SpawnX;
        public float SpawnYMin;
        public float SpawnYMax;
        public bool IsBossCandidate;
        public bool IsGuaranteedBoss;
        public float BossHealthMultiplierMin;
        public float BossHealthMultiplierMax;
        public bool GuaranteedPrisoner;
        public string Notes;
        public bool SelectedAsBoss;
    }

    [Serializable]
    public sealed class StageRewardDefinition
    {
        public string RewardRuleName;
        public string CombatType;
        public int Stage;
        public int Gold;
        public int DarkTrace;
        public float PrisonerCount1Chance;
        public float PrisonerCount2Chance;
        public float PrisonerCount3Chance;
        public float ManifestSuccessChance;
        public int EliteBonusPrisoners;
        public int ArtifactChoiceCount;
        public string GuaranteedPrisonerSource;
        public string Notes;

        public int RollPrisonerCount()
        {
            var roll = UnityEngine.Random.value;
            if (roll < PrisonerCount1Chance)
            {
                return 1 + EliteBonusPrisoners;
            }

            if (roll < PrisonerCount1Chance + PrisonerCount2Chance)
            {
                return 2 + EliteBonusPrisoners;
            }

            return 3 + EliteBonusPrisoners;
        }
    }
}
