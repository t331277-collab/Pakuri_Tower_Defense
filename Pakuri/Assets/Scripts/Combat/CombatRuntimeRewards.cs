using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Pakuri.Combat
{
    public partial class CombatRuntimeController
    {
        public int GetRewardChoiceCount()
        {
            return rewardOptions.Count;
        }

        public RewardChoiceView GetRewardChoiceView(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= rewardOptions.Count)
            {
                return default;
            }

            var option = rewardOptions[rewardIndex];
            return new RewardChoiceView(
                option.RewardId,
                option.RewardKind,
                option.Title,
                option.Description,
                option.PrisonerName,
                option.GoldAmount,
                option.DarkTraceAmount,
                option.Claimed);
        }

        public string ApplyRewardChoice(int rewardIndex)
        {
            if (rewardIndex < 0 || rewardIndex >= rewardOptions.Count)
            {
                return string.Empty;
            }

            var option = rewardOptions[rewardIndex];
            if (option.Claimed)
            {
                return string.Empty;
            }

            option.Claimed = true;
            lastAppliedDamageMultiplier = 1f;
            lastAppliedMagazineBonus = 0;
            lastAppliedShotIntervalMultiplier = 1f;
            lastAppliedReloadDurationMultiplier = 1f;
            lastAppliedMaxHealthBonus = 0f;
            lastAppliedStatusChanceBonus = 0f;
            lastAppliedRewardUnlockedPassive = false;
            appliedRewardSummary = $"{option.Title}: {option.Description}";
            rewardApplied = AreAllRewardOptionsClaimed();
            waitingForRewardChoice = !rewardApplied;
            statusLabel = appliedRewardSummary;
            return option.RewardId;
        }

        private bool AreAllRewardOptionsClaimed()
        {
            for (var i = 0; i < rewardOptions.Count; i++)
            {
                if (!rewardOptions[i].Claimed)
                {
                    return false;
                }
            }

            return true;
        }

        public IReadOnlyList<string> GetRewardPrisonerNames()
        {
            return rewardPrisonerNames;
        }

        private void CheckBattleResolution()
        {
            if (nexusCurrentHealth <= 0f)
            {
                battleResolved = true;
                victory = false;
                waitingForRewardChoice = false;
                statusLabel = "Nexus가 붕괴했다. 현재 일차를 다시 시도한다.";
                return;
            }

            var allSpawnsFinished = spawnedNormalCount >= pendingNormalSpawnCount && spawnedBossCount >= pendingBossSpawnCount;
            if (!allSpawnsFinished || enemies.Count > 0)
            {
                return;
            }

            battleResolved = true;
            victory = true;
            PrepareVictoryRewards();
        }

        private void PrepareVictoryRewards()
        {
            rewardPrisonerCount = RollPrisonerCount();
            if (currentCombatType == RunCombatType.Elite)
            {
                rewardPrisonerCount += 1;
            }

            rewardApplied = false;
            waitingForRewardChoice = false;
            appliedRewardSummary = string.Empty;
            lastAppliedRewardUnlockedPassive = false;

            switch (currentCombatType)
            {
                case RunCombatType.Day5Midboss:
                case RunCombatType.Day10Midboss:
                    rewardGold = 30;
                    rewardDarkTrace = GetScaledDarkTraceReward(20);
                    break;
                case RunCombatType.Boss:
                    rewardGold = 50;
                    rewardDarkTrace = GetScaledDarkTraceReward(50);
                    break;
                default:
                    rewardGold = 10;
                    rewardDarkTrace = GetScaledDarkTraceReward(10);
                    break;
            }

            rewardOptions.Clear();
            BuildRewardPrisoners();
            BuildMaterialRewardItems();
            rewardApplied = rewardOptions.Count == 0;
            waitingForRewardChoice = rewardOptions.Count > 0;
            appliedRewardSummary = rewardApplied
                ? "지급할 보상이 없다. 다음 일차로 진행한다."
                : "보상 버튼을 눌러 포로, 골드, 어둠의 흔적을 직접 습득한다.";
            statusLabel = appliedRewardSummary;
        }

        private void BuildRewardPrisoners()
        {
            rewardPrisonerNames.Clear();
            var targetCount = Mathf.Max(1, rewardPrisonerCount);

            for (var i = 0; i < currentGuaranteedPrisonerDefinitions.Count && rewardPrisonerNames.Count < targetCount; i++)
            {
                AddRewardPrisoner(currentGuaranteedPrisonerDefinitions[i]);
            }

            var guard = 0;
            while (rewardPrisonerNames.Count < targetCount && guard < 32)
            {
                guard += 1;
                var candidate = currentNormalEnemyPool.Count > 0
                    ? currentNormalEnemyPool[UnityEngine.Random.Range(0, currentNormalEnemyPool.Count)]
                    : null;
                AddRewardPrisoner(candidate);
            }

            while (rewardPrisonerNames.Count < targetCount)
            {
                rewardPrisonerNames.Add("견습 검사");
            }

            for (var i = 0; i < rewardPrisonerNames.Count; i++)
            {
                var prisonerName = rewardPrisonerNames[i];
                rewardOptions.Add(new RewardOption
                {
                    RewardId = $"prisoner:{dayIndex}:{i}:{prisonerName}",
                    RewardKind = "Prisoner",
                    Title = $"포로: {prisonerName}",
                    Description = "포로 종류만 표시한다. 공양/현현/동화/고문은 아직 구현하지 않는다.",
                    PrisonerName = prisonerName
                });
            }
        }

        private void AddRewardPrisoner(EnemyDefinition definition)
        {
            if (definition == null || string.IsNullOrWhiteSpace(definition.DisplayName))
            {
                return;
            }

            rewardPrisonerNames.Add(definition.DisplayName);
        }

        private void BuildMaterialRewardItems()
        {
            if (rewardGold > 0)
            {
                rewardOptions.Add(new RewardOption
                {
                    RewardId = $"gold:{dayIndex}",
                    RewardKind = "Material",
                    Title = $"골드 +{rewardGold}",
                    Description = "현재 런 안에서 사용하는 소비 재화.",
                    GoldAmount = rewardGold
                });
            }

            if (rewardDarkTrace > 0)
            {
                rewardOptions.Add(new RewardOption
                {
                    RewardId = $"dark-trace:{stageIndex}:{dayIndex}",
                    RewardKind = "Material",
                    Title = $"어둠의 흔적 +{rewardDarkTrace}",
                    Description = "런 외부 장기 성장에 사용하는 1티어 어둠 재화.",
                    DarkTraceAmount = rewardDarkTrace
                });
            }
        }

        private static int RollPrisonerCount()
        {
            var roll = UnityEngine.Random.value;
            if (roll < 0.05f)
            {
                return 1;
            }

            if (roll < 0.85f)
            {
                return 2;
            }

            return 3;
        }

        private float GetStageValueMultiplier()
        {
            switch (stageIndex)
            {
                case 2:
                    return 1.3f;
                case 3:
                    return 1.6f;
                case 4:
                    return 2.0f;
                default:
                    return 1f;
            }
        }

        private int GetScaledDarkTraceReward(int baseReward)
        {
            return Mathf.RoundToInt(baseReward * GetStageValueMultiplier());
        }
    }
}
