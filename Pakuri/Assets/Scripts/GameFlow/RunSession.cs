using System;
using System.Collections.Generic;
using Pakuri.Data;

/*
 * 한 번의 런에서 유지되는 진행 상태와 파티별 성장 상태를 보관한다.
 * 스테이지·일차, 재화, 배치 파티,
 * 몬스터별 학습 스킬과 Choice를 기록한다.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public class RunSession
    {
        private const int MaxAdditionalActiveSkillCount = 2;
        private const int MaxPassiveSkillCount = 5;
        private const int MaxPartyMonsterCount = 5;
        private const int MaxActiveEnhancementCount = 3;
        private const int MaxActiveMasterCount = 1;
        private const int MaxPassiveEnhancementCount = 1;

        [Serializable]
        public class RunMonsterState
        {
            public string MonsterId;
            public readonly List<string> LearnedActives = new List<string>();
            public readonly List<string> LearnedPassives = new List<string>();
            public readonly List<string> ChosenRewardIds = new List<string>();
            public readonly List<string> ChosenChoiceIds = new List<string>();
        }

        private readonly List<RunMonsterState> partyMembers = new List<RunMonsterState>();

        public string SelectedMonsterId => partyMembers.Count > 0 ? partyMembers[0].MonsterId : string.Empty;
        public IReadOnlyList<RunMonsterState> PartyMembers => partyMembers;
        public int StageIndex = 1;
        public int DayIndex = 1;
        public int Gold;
        public int DarkTrace;

        /*
         * 선택한 몬스터로 첫 스테이지의 런 진행 상태를 만든다.
         */
        public static RunSession Begin(MonsterDefinition monster /* 몬스터 */)
        {
            if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                throw new ArgumentException("A Monster with a non-empty ID is required.", nameof(monster));
            }

            var session = new RunSession();
            session.AddPartyMemberState(monster);
            return session;
        }

        /*
         * 런 시작 시 기본 습득할 A 슬롯 액티브 스킬을 찾는다.
         */
        private static string ResolveDefaultActiveSkillId(MonsterDefinition monster /* 몬스터 */)
        {
            if (monster == null || monster.ActiveSkills == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill != null
                    && skill.Slot == SkillSlot.A
                    && skill.IsDefaultLearned
                    && !string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    return skill.SkillId;
                }
            }

            return string.Empty;
        }

        /*
         * 지정한 파티원이 선택한 Offering 보상과 습득 스킬을 기록한다.
         */
        public void RecordOfferingChoice(
            RunMonsterState member /* 보상을 받을 파티원 상태 */,
            string rewardId /* 보상 식별자 */,
            string linkedChoiceId /* 연결된 선택지 식별자 */,
            string activeSkillId /* 액티브 스킬 식별자 */,
            string passiveSkillId /* 패시브 스킬 식별자 */)
        {
            if (member == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(rewardId) && !member.ChosenRewardIds.Contains(rewardId))
            {
                member.ChosenRewardIds.Add(rewardId);
            }

            if (!string.IsNullOrWhiteSpace(linkedChoiceId) && !member.ChosenChoiceIds.Contains(linkedChoiceId))
            {
                member.ChosenChoiceIds.Add(linkedChoiceId);
            }

            if (!string.IsNullOrWhiteSpace(activeSkillId) && !member.LearnedActives.Contains(activeSkillId))
            {
                member.LearnedActives.Add(activeSkillId);
            }

            if (!string.IsNullOrWhiteSpace(passiveSkillId) && !member.LearnedPassives.Contains(passiveSkillId))
            {
                member.LearnedPassives.Add(passiveSkillId);
            }
        }

        /*
         * 파티원이 지정한 액티브 스킬을 새로 학습할 수 있는지 확인한다.
         */
        public bool CanLearnActive(
            RunMonsterState member /* 스킬을 학습할 파티원 상태 */,
            MonsterDefinition monster /* 스킬을 학습할 몬스터 */,
            SkillSourceDefinition skill /* 학습 후보 액티브 스킬 */)
        {
            if (member == null
                || monster == null
                || skill == null
                || string.IsNullOrWhiteSpace(skill.SkillId)
                || member.LearnedActives.Contains(skill.SkillId))
            {
                return false;
            }

            var defaultActiveSkillId = ResolveDefaultActiveSkillId(monster);
            var additionalCount = 0;
            for (var i = 0; i < member.LearnedActives.Count; i++)
            {
                if (!string.Equals(
                    member.LearnedActives[i],
                    defaultActiveSkillId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    additionalCount++;
                }
            }

            return additionalCount < MaxAdditionalActiveSkillCount;
        }

        /*
         * 파티원이 요구 액티브 스킬을 갖추고 패시브를 새로 학습할 수 있는지 확인한다.
         */
        public bool CanLearnPassive(
            RunMonsterState member /* 스킬을 학습할 파티원 상태 */,
            MonsterDefinition monster /* 스킬을 학습할 몬스터 */,
            PassiveDefinition passive /* 학습 후보 패시브 스킬 */)
        {
            if (member == null
                || monster == null
                || passive == null
                || string.IsNullOrWhiteSpace(passive.PassiveId)
                || member.LearnedPassives.Contains(passive.PassiveId)
                || member.LearnedPassives.Count >= MaxPassiveSkillCount)
            {
                return false;
            }

            if (passive.IsAvailableWithoutActiveRequirement)
            {
                return true;
            }

            if (monster.ActiveSkills == null)
            {
                return false;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var active = monster.ActiveSkills[i];
                if (active != null
                    && active.Slot == passive.RequiredActiveSlot
                    && member.LearnedActives.Contains(active.SkillId))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 파티원이 요구 스킬과 선행 강화를 갖추고 Choice를 선택할 수 있는지 확인한다.
         */
        public bool CanChooseSkillChoice(
            RunMonsterState member /* Choice를 선택할 파티원 상태 */,
            MonsterDefinition.RewardChoiceDefinition reward /* Choice와 연결된 보상 */,
            SkillChoiceDefinition choice /* 선택 후보 강화 효과 */)
        {
            if (member == null || reward == null || choice == null)
            {
                return false;
            }

            if (member.ChosenRewardIds.Contains(reward.RewardId)
                || member.ChosenChoiceIds.Contains(choice.ChoiceId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.ActiveSkillId)
                && !member.LearnedActives.Contains(reward.ActiveSkillId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.PassiveSkillId)
                && !member.LearnedPassives.Contains(reward.PassiveSkillId))
            {
                return false;
            }

            var sourceSkillId = reward.ActiveSkillId;
            if (string.IsNullOrWhiteSpace(sourceSkillId))
            {
                sourceSkillId = reward.PassiveSkillId;
            }

            return CanChooseSkillChoice(member, sourceSkillId, choice);
        }

        /*
         * 파티원의 선택 기록과 성장 단계 제한을 기준으로 Choice를 선택할 수 있는지 확인한다.
         */
        public bool CanChooseSkillChoice(
            RunMonsterState member /* Choice를 선택할 파티원 상태 */,
            string sourceSkillId /* Choice가 연결된 원본 스킬 식별자 */,
            SkillChoiceDefinition choice /* 선택 후보 강화 효과 */)
        {
            if (member == null || choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId))
            {
                return false;
            }

            if (member.ChosenChoiceIds.Contains(choice.ChoiceId))
            {
                return false;
            }

            var targetSkillId = ResolveChoiceTargetSkillId(choice, sourceSkillId);

            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveEnhancement)
            {
                return CountChosenChoices(member, targetSkillId, SkillChoiceGroup.ActiveEnhancement)
                    < MaxActiveEnhancementCount;
            }
            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster)
            {
                return CountChosenChoices(member, targetSkillId, SkillChoiceGroup.ActiveEnhancement)
                        >= MaxActiveEnhancementCount
                    && CountChosenChoices(member, targetSkillId, SkillChoiceGroup.ActiveMaster)
                        < MaxActiveMasterCount;
            }
            if (choice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement)
            {
                return CountChosenChoices(member, targetSkillId, SkillChoiceGroup.PassiveEnhancement)
                    < MaxPassiveEnhancementCount;
            }

            return true;
        }

        /*
         * 획득한 골드와 어둠의 흔적을 런 재화에 더한다.
         */
        public void ClaimMaterialReward(int goldReward /* 골드 보상 */, int darkTraceReward /* 어둠 흔적 보상 */)
        {
            Gold += Math.Max(0, goldReward);
            DarkTrace += Math.Max(0, darkTraceReward);
        }

        /*
         * 새 몬스터를 다음 파티 슬롯과 진행 상태에 등록한다.
         */
        public bool TryAddPartyMonster(
            MonsterDefinition monster /* 파티에 추가할 몬스터 */,
            out int slotIndex /* 추가된 파티 슬롯 순서 번호 */)
        {
            slotIndex = -1;
            if (monster == null
                || string.IsNullOrWhiteSpace(monster.MonsterId)
                || partyMembers.Count >= MaxPartyMonsterCount
                || GetPartyMemberState(monster.MonsterId) != null)
            {
                return false;
            }

            AddPartyMemberState(monster);
            slotIndex = partyMembers.Count - 1;
            return true;
        }

        /*
         * 새 파티 진행 상태를 기본 액티브 스킬과 함께 추가한다.
         */
        private RunMonsterState AddPartyMemberState(MonsterDefinition monster /* 추가할 몬스터 */)
        {
            var state = new RunMonsterState
            {
                MonsterId = monster.MonsterId
            };

            var defaultActiveSkillId = ResolveDefaultActiveSkillId(monster);
            if (!string.IsNullOrWhiteSpace(defaultActiveSkillId))
            {
                state.LearnedActives.Add(defaultActiveSkillId);
            }

            partyMembers.Add(state);
            return state;
        }

        /*
         * 몬스터 식별자와 일치하는 파티 진행 상태를 찾는다.
         */
        public RunMonsterState GetPartyMemberState(string monsterId /* 몬스터 식별자 */)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return null;
            }

            for (var i = 0; i < partyMembers.Count; i++)
            {
                var member = partyMembers[i];
                if (member != null && string.Equals(member.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }

            return null;
        }

        /*
         * 지정한 스킬과 성장 단계에 해당하는 선택 완료 Choice 수를 계산한다.
         */
        private static int CountChosenChoices(
            RunMonsterState member /* 파티원의 런 성장 상태 */,
            string skillId /* Choice가 강화하는 스킬 식별자 */,
            SkillChoiceGroup group /* 계산할 성장 단계 */)
        {
            if (member == null || string.IsNullOrWhiteSpace(skillId))
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < member.ChosenChoiceIds.Count; i++)
            {
                var choiceId = member.ChosenChoiceIds[i];
                if (GameDataLoader.CurrentCatalog.TryGetData(choiceId, out SkillChoiceDefinition choice)
                    && choice != null
                    && choice.ChoiceGroup == group
                    && string.Equals(
                        ResolveChoiceTargetSkillId(choice, string.Empty),
                        skillId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        /*
         * Choice가 적용될 스킬 ID를 명시값과 대체값 순서로 찾는다.
         */
        private static string ResolveChoiceTargetSkillId(
            SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */,
            string fallbackSkillId /* Choice에 대상이 없을 때 사용할 스킬 식별자 */)
        {
            if (choice == null)
            {
                return fallbackSkillId;
            }

            if (!string.IsNullOrWhiteSpace(choice.SkillId))
            {
                return choice.SkillId;
            }

            if (!string.IsNullOrWhiteSpace(choice.TargetSkillId))
            {
                return choice.TargetSkillId;
            }

            return fallbackSkillId;
        }

    }
}
