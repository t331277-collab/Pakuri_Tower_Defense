/*
 * 역할: 한 게임 Run의 지속 상태.
 * 책임: 선택 몬스터·학습 스킬·선택지·Stage 진행·보상·Run 초기화를 추적한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.InGame
{

    /// RunSession가 소유하는 데이터와 동작을 캡슐화한다.
    [Serializable]
    public class RunSession
    {
        private const int MaxAdditionalActiveSkillCount = 2;
        private const int MaxPassiveSkillCount = 5;
        private const int MaxPartyMonsterCount = 5;
        private const int MaxActiveEnhancementCount = 3;
        private const int MaxActiveMasterCount = 1;
        private const int MaxPassiveEnhancementCount = 1;

        /// RunMonsterState의 변경 가능한 런타임 상태를 보관한다.
        [Serializable]
        public class RunMonsterState
        {
            public string MonsterId;
            public readonly UnitSkills Skills = new UnitSkills();
            public readonly List<string> ChosenRewardIds = new List<string>();
        }

        private readonly List<RunMonsterState> partyMembers = new List<RunMonsterState>();

        public string SelectedMonsterId => partyMembers.Count > 0 ? partyMembers[0].MonsterId : string.Empty;
        public IReadOnlyList<RunMonsterState> PartyMembers => partyMembers;
        public int StageIndex = 1;
        public int DayIndex = 1;
        public int Gold;
        public int DarkTrace;

        /// 전달된 monster 값을 사용해 Begin 결과값을 생성해 반환한다.
        public static RunSession Begin(MonsterDefinition monster)
        {

            var session = new RunSession();
            session.AddPartyMemberState(monster);
            return session;
        }

        /// 전달된 monster 값을 사용해 DefaultActiveSkillId를 결정한다.
        private static string ResolveDefaultActiveSkillId(MonsterDefinition monster)
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

        /// 전달된 런타임 입력값을 사용해 RecordOfferingChoice 작업을 수행한다.
        public void RecordOfferingChoice(
            RunMonsterState member,
            string rewardId,
            string linkedChoiceId,
            string activeSkillId,
            string passiveSkillId)
        {
            if (member == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(rewardId) && !member.ChosenRewardIds.Contains(rewardId))
            {
                member.ChosenRewardIds.Add(rewardId);
            }

            if (!string.IsNullOrWhiteSpace(linkedChoiceId) && !member.Skills.HasChoice(linkedChoiceId))
            {
                member.Skills.AddChoice(linkedChoiceId);
            }

            if (!string.IsNullOrWhiteSpace(activeSkillId) && !member.Skills.HasActiveSkill(activeSkillId))
            {
                member.Skills.AddActiveSkill(activeSkillId);
            }

            if (!string.IsNullOrWhiteSpace(passiveSkillId) && !member.Skills.HasPassiveSkill(passiveSkillId))
            {
                member.Skills.AddPassiveSkill(passiveSkillId);
            }
        }

        /// 전달된 런타임 입력값을 사용해 LearnActive 실행 가능 여부를 반환한다.
        public bool CanLearnActive(
            RunMonsterState member,
            MonsterDefinition monster,
            SkillDefinition skill)
        {
            if (member == null
                || monster == null
                || skill == null
                || string.IsNullOrWhiteSpace(skill.SkillId)
                || member.Skills.HasActiveSkill(skill.SkillId))
            {
                return false;
            }

            var defaultActiveSkillId = ResolveDefaultActiveSkillId(monster);
            var additionalCount = 0;
            foreach (var learnedActiveSkillId in member.Skills.LearnedActiveSkillIds)
            {
                if (!string.Equals(
                    learnedActiveSkillId,
                    defaultActiveSkillId,
                    StringComparison.OrdinalIgnoreCase))
                {
                    additionalCount++;
                }
            }

            return additionalCount < MaxAdditionalActiveSkillCount;
        }

        /// 전달된 런타임 입력값을 사용해 LearnPassive 실행 가능 여부를 반환한다.
        public bool CanLearnPassive(
            RunMonsterState member,
            MonsterDefinition monster,
            PassiveSkillDefinition passive)
        {
            if (member == null
                || monster == null
                || passive == null
                || string.IsNullOrWhiteSpace(passive.SkillId)
                || member.Skills.HasPassiveSkill(passive.SkillId)
                || member.Skills.LearnedPassiveSkillIds.Count >= MaxPassiveSkillCount)
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
                    && member.Skills.HasActiveSkill(active.SkillId))
                {
                    return true;
                }
            }

            return false;
        }

        /// 전달된 런타임 입력값을 사용해 ChooseSkillChoice 실행 가능 여부를 반환한다.
        public bool CanChooseSkillChoice(
            RunMonsterState member,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoice choice)
        {
            if (member == null || reward == null || choice == null)
            {
                return false;
            }

            if (member.ChosenRewardIds.Contains(reward.RewardId)
                || member.Skills.HasChoice(choice.ChoiceId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.ActiveSkillId)
                && !member.Skills.HasActiveSkill(reward.ActiveSkillId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.PassiveSkillId)
                && !member.Skills.HasPassiveSkill(reward.PassiveSkillId))
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

        /// 전달된 런타임 입력값을 사용해 ChooseSkillChoice 실행 가능 여부를 반환한다.
        public bool CanChooseSkillChoice(
            RunMonsterState member,
            string sourceSkillId,
            SkillChoice choice)
        {
            if (member == null || choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId))
            {
                return false;
            }

            if (member.Skills.HasChoice(choice.ChoiceId))
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

        /// 전달된 런타임 입력값을 사용해 ClaimMaterialReward 작업을 수행한다.
        public void ClaimMaterialReward(int goldReward, int darkTraceReward)
        {
            Gold += Math.Max(0, goldReward);
            DarkTrace += Math.Max(0, darkTraceReward);
        }

        /// 전달된 런타임 입력값을 사용해 AddPartyMonster 작업을 시도하고 성공 여부를 반환한다.
        public bool TryAddPartyMonster(
            MonsterDefinition monster,
            out int slotIndex)
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

        /// 전달된 monster 값을 사용해 PartyMemberState를 소유한 런타임 상태에 추가한다.
        private RunMonsterState AddPartyMemberState(MonsterDefinition monster)
        {
            var state = new RunMonsterState
            {
                MonsterId = monster.MonsterId
            };

            var defaultActiveSkillId = ResolveDefaultActiveSkillId(monster);
            if (!string.IsNullOrWhiteSpace(defaultActiveSkillId))
            {
                state.Skills.AddActiveSkill(defaultActiveSkillId);
            }

            partyMembers.Add(state);
            return state;
        }

        /// 전달된 monsterId 값을 사용해 PartyMemberState를 반환한다.
        public RunMonsterState GetPartyMemberState(string monsterId)
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

        /// 전달된 런타임 입력값을 사용해 CountChosenChoices 결과값을 생성해 반환한다.
        private static int CountChosenChoices(
            RunMonsterState member,
            string skillId,
            SkillChoiceGroup group)
        {
            if (member == null || string.IsNullOrWhiteSpace(skillId))
            {
                return 0;
            }

            var count = 0;
            var choiceIds = group == SkillChoiceGroup.ActiveMaster
                ? member.Skills.ChosenMasterSkillIds
                : member.Skills.ChosenEnhancementIds;
            foreach (var choiceId in choiceIds)
            {
                if (GameDataLoader.CurrentCatalog.TryGetData(choiceId, out SkillChoice choice)
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

        /// 전달된 런타임 입력값을 사용해 ChoiceTargetSkillId를 결정한다.
        private static string ResolveChoiceTargetSkillId(
            SkillChoice choice,
            string fallbackSkillId)
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
