using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Spawn;

/* 전투 종료 보상 규칙을 검증하고 재화와 포로 보상을 지급한다. */
namespace Pakuri.NewCore.Run.Services
{
    public class RewardResult
    {
        /* 적용된 보상 정의와 재화·포로 결과를 불변 값으로 묶는다. */
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
            Definition.manifest_success_chance.GetValueOrDefault();
    }

    public class RewardService
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly Func<int, int> randomIndex;
        private readonly Func<float> randomValue;

        /* 보상 정의 조회와 포로 추첨에 필요한 카탈로그와 난수 공급원을 연결한다. */
        public RewardService(
            GameDefinitionCatalog catalog,
            Func<int, int> randomIndex,
            Func<float> randomValue)
        {
            this.catalog =
                catalog;
            this.randomIndex =
                randomIndex;
            this.randomValue =
                randomValue;
        }

        /* 전달된 SpawnManager의 소유권을 검증한 뒤 현재 stage 보상을 지급한다. */
        public RewardResult GenerateAndGrant(
            StageManager stage,
            SpawnManager spawns)
        {

            return GenerateAndGrant(stage);
        }

        /* 현재 day의 보상 규칙을 검증하고 재화와 추첨된 포로를 session에 지급한다. */
        public RewardResult GenerateAndGrant(StageManager stage)
        {

            SpawnManager spawns = stage.ActiveSpawnManager;

            StageDayDefinition day = stage.CurrentDayDefinition;
            catalog.StageRewards.TryGetValue(
                day.reward_rule_id,
                out StageRewardDefinition reward);

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

        /* 보상 확률표를 검증하고 난수로 지급할 포로 수를 결정한다. */
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

            float roll = NextUnitValue();
            int count;
            if (roll < one)
            {
                count = 1;
            }
            else if (roll < one + two)
            {
                count = 2;
            }
            else
            {
                count = 3;
            }

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

        /* 보장 보스와 일반 spawn pool에서 중복 없이 포로 적 id를 추첨한다. */
        private List<string> SelectPrisoners(
            StageRewardDefinition reward,
            IReadOnlyList<SpawnedEnemyRecord> spawned,
            int count)
        {

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
            pool.Remove(selectedGuaranteed);

            while (result.Count < count)
            {
                int index = ResolveRandomIndex(pool.Count);
                result.Add(
                    pool[index].Model.EnemyDefinition.enemy_id);
                pool.RemoveAt(index);
            }

            return result;
        }

        /* 실제 spawn 기록에서 보스 보장 포로 후보를 추출한다. */
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
                bool eligible = false;
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
                        break;
                }

                if (eligible)
                {
                    result.Add(record);
                }
            }

            return result;
        }

        /* 난수 공급원이 반환한 index가 요청 범위 안인지 검증한다. */
        private int ResolveRandomIndex(int count)
        {
            int index = randomIndex(count);

            return index;
        }

        /* 난수 공급원이 유효한 0~1 값을 반환했는지 검증한다. */
        private float NextUnitValue()
        {
            float value = randomValue();

            return value;
        }

        /* 보상 정의의 필수 정수가 존재하고 음수가 아닌지 검증한다. */
        private static int RequiredNonNegative(
            int? value,
            StageRewardDefinition reward,
            string column)
        {

            return value.Value;
        }

        /* 보상 정의의 필수 실수가 유효한 확률 범위인지 검증한다. */
        private static float RequiredProbability(
            float? value,
            StageRewardDefinition reward,
            string column)
        {

            return value.Value;
        }

        /* 보상 정의의 원본 경로와 레코드 번호를 포함한 예외를 생성한다. */
        private static InvalidOperationException Invalid(
            StageRewardDefinition reward,
            string message)
        {
            return new InvalidOperationException(
                $"{reward.SourcePath} record {reward.SourceRecordNumber}: {message}");
        }
    }
}
