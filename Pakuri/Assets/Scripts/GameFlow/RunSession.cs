/*
 * 역할: 한 게임 Run의 지속 상태.
 * 책임: 선택 몬스터·학습 스킬·선택지·Stage 진행·보상·Run 초기화를 추적한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;

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

        /// 선택한 몬스터를 첫 파티원으로 등록해 새 Run을 시작한다.
        public static RunSession Begin(MonsterDefinition monster)
        {

            var session = new RunSession();
            session.AddPartyMemberState(monster);
            return session;
        }

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

        /// Offering에서 선택한 보상과 연결 스킬을 해당 파티원의 Run 상태에 기록한다.
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

        /// 파티원이 새 액티브 스킬을 배울 수 있는지 현재 학습 한도와 조건으로 판정한다.
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

        /// 파티원이 새 패시브 스킬을 배울 수 있는지 선행 액티브 스킬과 한도로 판정한다.
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

        /// 보상 선택지를 해당 파티원이 아직 선택할 수 있는지 판정한다.
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

        /// 전투 보상으로 얻은 재화를 RunSession에 누적한다.
        public void ClaimMaterialReward(int goldReward, int darkTraceReward)
        {
            Gold += Math.Max(0, goldReward);
            DarkTrace += Math.Max(0, darkTraceReward);
        }

        /// 아직 파티에 없는 몬스터를 다음 슬롯에 추가한다.
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
