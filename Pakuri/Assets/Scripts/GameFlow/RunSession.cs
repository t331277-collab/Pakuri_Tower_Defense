/*
 * 역할: 한 게임 Run의 지속 상태.
 * 책임: 선택 몬스터·학습 스킬·선택지·Stage 진행·보상·Run 초기화를 추적한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.InGame
{

    public enum RunMode
    {
        Normal,
        Tutorial
    }

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
            public string MonsterName;
            public readonly UnitSkills Skills = new UnitSkills();
            public readonly ArtifactState Artifacts = new ArtifactState();
            public readonly List<string> ChosenRewardNames = new List<string>();
        }

        private readonly List<RunMonsterState> partyMembers = new List<RunMonsterState>();

        public string SelectedMonsterName => partyMembers.Count > 0 ? partyMembers[0].MonsterName : string.Empty;
        public IReadOnlyList<RunMonsterState> PartyMembers => partyMembers;
        public RunMode Mode { get; private set; }
        public bool IsTutorial => Mode == RunMode.Tutorial;
        public int StageIndex = 1;
        public int DayIndex = 1;
        public int Gold;
        public int DarkTrace;

        /// 선택한 몬스터를 첫 파티원으로 등록해 새 Run을 시작한다.
        public static RunSession Begin(MonsterDefinition monster, RunMode mode = RunMode.Normal)
        {

            var session = new RunSession
            {
                Mode = mode
            };
            session.AddPartyMemberState(monster);
            return session;
        }

        private static string ResolveDefaultActiveSkillName(MonsterDefinition monster)
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
                    && !string.IsNullOrWhiteSpace(skill.SkillName))
                {
                    return skill.SkillName;
                }
            }

            return string.Empty;
        }

        /// Offering에서 선택한 보상과 연결 스킬을 해당 파티원의 Run 상태에 기록한다.
        public void RecordOfferingChoice(
            RunMonsterState member,
            string rewardName,
            string linkedChoiceName,
            string activeSkillName,
            string passiveSkillName)
        {
            if (member == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(rewardName) && !member.ChosenRewardNames.Contains(rewardName))
            {
                member.ChosenRewardNames.Add(rewardName);
            }

            if (!string.IsNullOrWhiteSpace(linkedChoiceName) && !member.Skills.HasChoice(linkedChoiceName))
            {
                member.Skills.AddChoice(linkedChoiceName);
            }

            if (!string.IsNullOrWhiteSpace(activeSkillName) && !member.Skills.HasActiveSkill(activeSkillName))
            {
                member.Skills.AddActiveSkill(activeSkillName);
            }

            if (!string.IsNullOrWhiteSpace(passiveSkillName) && !member.Skills.HasPassiveSkill(passiveSkillName))
            {
                member.Skills.AddPassiveSkill(passiveSkillName);
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
                || string.IsNullOrWhiteSpace(skill.SkillName)
                || member.Skills.HasActiveSkill(skill.SkillName))
            {
                return false;
            }

            var defaultActiveSkillName = ResolveDefaultActiveSkillName(monster);
            var additionalCount = 0;
            foreach (var learnedActiveSkillName in member.Skills.LearnedActiveSkillNames)
            {
                if (!string.Equals(
                    learnedActiveSkillName,
                    defaultActiveSkillName,
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
                || string.IsNullOrWhiteSpace(passive.SkillName)
                || member.Skills.HasPassiveSkill(passive.SkillName)
                || member.Skills.LearnedPassiveSkillNames.Count >= MaxPassiveSkillCount)
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
                    && member.Skills.HasActiveSkill(active.SkillName))
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

            if (member.ChosenRewardNames.Contains(reward.RewardName)
                || member.Skills.HasChoice(choice.ChoiceName))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.ActiveSkillName)
                && !member.Skills.HasActiveSkill(reward.ActiveSkillName))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.PassiveSkillName)
                && !member.Skills.HasPassiveSkill(reward.PassiveSkillName))
            {
                return false;
            }

            var sourceSkillName = reward.ActiveSkillName;
            if (string.IsNullOrWhiteSpace(sourceSkillName))
            {
                sourceSkillName = reward.PassiveSkillName;
            }

            return CanChooseSkillChoice(member, sourceSkillName, choice);
        }

        public bool CanChooseSkillChoice(
            RunMonsterState member,
            string sourceSkillName,
            SkillChoice choice)
        {
            if (member == null || choice == null || string.IsNullOrWhiteSpace(choice.ChoiceName))
            {
                return false;
            }

            if (member.Skills.HasChoice(choice.ChoiceName))
            {
                return false;
            }

            var targetSkillName = ResolveChoiceTargetSkillName(choice, sourceSkillName);

            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveEnhancement)
            {
                return CountChosenChoices(member, targetSkillName, SkillChoiceGroup.ActiveEnhancement)
                    < MaxActiveEnhancementCount;
            }
            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster)
            {
                return CountChosenChoices(member, targetSkillName, SkillChoiceGroup.ActiveEnhancement)
                        >= MaxActiveEnhancementCount
                    && CountChosenChoices(member, targetSkillName, SkillChoiceGroup.ActiveMaster)
                        < MaxActiveMasterCount;
            }
            if (choice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement)
            {
                return CountChosenChoices(member, targetSkillName, SkillChoiceGroup.PassiveEnhancement)
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

        public bool HasArtifactCapacity()
        {
            for (var i = 0; i < partyMembers.Count; i++)
            {
                var member = partyMembers[i];
                if (member != null && member.Artifacts.OwnedArtifactNames.Count < ArtifactState.MaxOwnedArtifactCount)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasArtifact(string artifactName)
        {
            if (string.IsNullOrWhiteSpace(artifactName))
            {
                return false;
            }

            for (var i = 0; i < partyMembers.Count; i++)
            {
                var ownedNames = partyMembers[i].Artifacts.OwnedArtifactNames;
                for (var j = 0; j < ownedNames.Count; j++)
                {
                    if (string.Equals(ownedNames[j], artifactName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool CanAcquireArtifact(RunMonsterState member, string artifactName)
        {
            return member != null
                && partyMembers.Contains(member)
                && !HasArtifact(artifactName)
                && member.Artifacts.CanAdd(artifactName);
        }

        public bool TryAcquireArtifact(RunMonsterState member, string artifactName)
        {
            return CanAcquireArtifact(member, artifactName)
                && member.Artifacts.TryAdd(artifactName);
        }

        /// 아직 파티에 없는 몬스터를 다음 슬롯에 추가한다.
        public bool TryAddPartyMonster(
            MonsterDefinition monster,
            out int slotIndex)
        {
            slotIndex = -1;
            if (monster == null
                || string.IsNullOrWhiteSpace(monster.MonsterName)
                || partyMembers.Count >= MaxPartyMonsterCount
                || GetPartyMemberState(monster.MonsterName) != null)
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
                MonsterName = monster.MonsterName
            };

            var defaultActiveSkillName = ResolveDefaultActiveSkillName(monster);
            if (!string.IsNullOrWhiteSpace(defaultActiveSkillName))
            {
                state.Skills.AddActiveSkill(defaultActiveSkillName);
            }

            partyMembers.Add(state);
            return state;
        }

        public RunMonsterState GetPartyMemberState(string monsterName)
        {
            if (string.IsNullOrWhiteSpace(monsterName))
            {
                return null;
            }

            for (var i = 0; i < partyMembers.Count; i++)
            {
                var member = partyMembers[i];
                if (member != null && string.Equals(member.MonsterName, monsterName, StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }

            return null;
        }

        private static int CountChosenChoices(
            RunMonsterState member,
            string skillName,
            SkillChoiceGroup group)
        {
            if (member == null || string.IsNullOrWhiteSpace(skillName))
            {
                return 0;
            }

            var count = 0;
            var choiceNames = group == SkillChoiceGroup.ActiveMaster
                ? member.Skills.ChosenMasterSkillNames
                : member.Skills.ChosenEnhancementNames;
            foreach (var choiceName in choiceNames)
            {
                if (GameDataLoader.CurrentCatalog.TryGetData(choiceName, out SkillChoice choice)
                    && choice != null
                    && choice.ChoiceGroup == group
                    && string.Equals(
                        ResolveChoiceTargetSkillName(choice, string.Empty),
                        skillName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static string ResolveChoiceTargetSkillName(
            SkillChoice choice,
            string fallbackSkillName)
        {
            if (choice == null)
            {
                return fallbackSkillName;
            }

            if (!string.IsNullOrWhiteSpace(choice.SkillName))
            {
                return choice.SkillName;
            }

            if (!string.IsNullOrWhiteSpace(choice.TargetSkillName))
            {
                return choice.TargetSkillName;
            }

            return fallbackSkillName;
        }

    }
}
