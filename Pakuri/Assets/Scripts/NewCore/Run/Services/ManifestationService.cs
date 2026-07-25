using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Units;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;

/* 포로 현현 시도와 성공 후보의 파티 영입 절차를 관리한다. */
namespace Pakuri.NewCore.Run.Services
{
    public sealed class ManifestationAttemptResult
    {
        /* 현현 성공 여부와 성공 시 선택된 몬스터 정의를 묶는다. */
        internal ManifestationAttemptResult(
            bool success,
            MonsterDefinition candidate)
        {
            Success = success;
            Candidate = candidate;
        }

        public bool Success { get; }

        public MonsterDefinition Candidate { get; }
    }

    public sealed class ManifestationService
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly StageManager stage;
        private readonly SpawnManager spawns;
        private readonly Func<int, int> randomIndex;
        private readonly Func<float> randomValue;
        private MonsterDefinition pendingCandidate;

        /* 현현 후보 조회·포로 소비·몬스터 생성에 필요한 서비스와 난수 공급원을 연결한다. */
        public ManifestationService(
            GameDefinitionCatalog catalog,
            StageManager stage,
            SpawnManager spawns,
            Func<int, int> randomIndex,
            Func<float> randomValue)
        {
            this.catalog =
                catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.stage =
                stage ?? throw new ArgumentNullException(nameof(stage));
            this.spawns =
                spawns ?? throw new ArgumentNullException(nameof(spawns));
            this.randomIndex =
                randomIndex ?? throw new ArgumentNullException(nameof(randomIndex));
            this.randomValue =
                randomValue ?? throw new ArgumentNullException(nameof(randomValue));
        }

        public MonsterDefinition PendingCandidate => pendingCandidate;

        /* 포로를 소비해 성공 여부와 영입 가능한 몬스터 후보를 추첨한다. */
        public ManifestationAttemptResult BeginAttempt(
            Prisoner prisoner,
            StageRewardDefinition reward)
        {
            if (pendingCandidate != null)
            {
                throw new InvalidOperationException(
                    "The successful manifestation must be recruited or skipped.");
            }

            if (!stage.Session.PrisonerInventory.CanConsume(prisoner))
            {
                throw new InvalidOperationException(
                    "Manifestation prisoner is not held.");
            }

            if (reward == null)
            {
                throw new ArgumentNullException(nameof(reward));
            }

            float successChance = reward.manifest_success_chance
                ?? throw Invalid(
                    reward,
                    "manifest_success_chance is required.");
            if (successChance < 0f
                || successChance > 1f
                || float.IsNaN(successChance)
                || float.IsInfinity(successChance))
            {
                throw Invalid(
                    reward,
                    "manifest_success_chance must be a probability.");
            }

            List<MonsterDefinition> candidates =
                BuildCandidates();
            if (candidates.Count == 0)
            {
                throw new InvalidOperationException(
                    "No manifestation candidate can enter the party.");
            }

            MonsterDefinition selected =
                candidates[ResolveRandomIndex(candidates.Count)];
            bool success = NextUnitValue() < successChance;
            if (!stage.Session.PrisonerInventory.TryConsume(prisoner))
            {
                throw new InvalidOperationException(
                    "Validated prisoner consumption failed.");
            }

            if (success)
            {
                pendingCandidate = selected;
            }

            return new ManifestationAttemptResult(
                success,
                success ? selected : null);
        }

        /* 성공한 현현 후보를 모델로 만들고 파티와 전장에 원자적으로 등록한다. */
        public MonsterModel ConfirmRecruitment()
        {
            if (pendingCandidate == null)
            {
                throw new InvalidOperationException(
                    "There is no successful manifestation to recruit.");
            }

            if (!stage.Session.PartyRoster.CanAdd(
                    pendingCandidate.id))
            {
                throw new InvalidOperationException(
                    "The manifested monster can no longer enter the party.");
            }

            MonsterModel monster = spawns.CreateMonsterModel(
                pendingCandidate,
                true);
            if (!stage.Session.PartyRoster.TryAddManifestedMonster(
                    monster))
            {
                throw new InvalidOperationException(
                    "Manifested party registration failed.");
            }

            try
            {
                if (!stage.PlaceManifestedMonster(monster))
                {
                    stage.Session.PartyRoster
                        .TryRemoveManifestedMonster(monster);
                    throw new InvalidOperationException(
                        "Manifested field placement failed.");
                }
            }
            catch
            {
                stage.Session.PartyRoster
                    .TryRemoveManifestedMonster(monster);
                throw;
            }

            pendingCandidate = null;
            return monster;
        }

        /* 대기 중인 현현 후보를 영입하지 않고 제거한다. */
        public bool SkipRecruitment()
        {
            if (pendingCandidate == null)
            {
                return false;
            }

            pendingCandidate = null;
            return true;
        }

        /* 현재 파티에 추가할 수 있는 몬스터 정의를 id 순으로 구성한다. */
        private List<MonsterDefinition> BuildCandidates()
        {
            List<MonsterDefinition> candidates =
                new List<MonsterDefinition>();
            foreach (MonsterDefinition monster
                in catalog.Monsters.Values)
            {
                if (stage.Session.PartyRoster.CanAdd(monster.id))
                {
                    candidates.Add(monster);
                }
            }

            candidates.Sort((left, right) =>
                string.CompareOrdinal(left.id, right.id));
            return candidates;
        }

        /* 난수 공급원이 반환한 index가 후보 범위 안인지 검증한다. */
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

        /* 난수 공급원이 유효한 0~1 값을 반환했는지 검증한다. */
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

        /* 보상 정의의 원본 위치를 포함한 현현 규칙 예외를 생성한다. */
        private static InvalidOperationException Invalid(
            StageRewardDefinition reward,
            string message)
        {
            return new InvalidOperationException(
                $"{reward.SourcePath} record {reward.SourceRecordNumber}: {message}");
        }
    }
}
