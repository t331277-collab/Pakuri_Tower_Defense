using System;
using System.Collections.Generic;
using Pakuri.Data;

/*
 * 한 번의 런에서 유지되는 진행 상태와 파티별 성장 상태를 보관한다.
 * 스테이지·일차, 재화·포로, 선택 및 현현 몬스터,
 * 몬스터별 학습 스킬과 Choice를 기록한다.
 */
namespace Pakuri.InGame
{
    [Serializable]
    public class RunSession
    {
        [Serializable]
        public class RunMonsterState
        {
            public string MonsterId;
            public string MonsterName;
            public readonly List<string> LearnedActives = new List<string>();
            public readonly List<string> LearnedPassives = new List<string>();
            public readonly List<string> ChosenRewardIds = new List<string>();
            public readonly List<string> ChosenChoiceIds = new List<string>();
            public float MaxHealthBonus;
        }

        public string SelectedMonsterId;
        public string SelectedMonsterName;
        public string ActiveSkillId;
        public string PassiveSkillId;
        public string ActiveSkillName;
        public string PassiveSkillName;
        public int StageIndex = 1;
        public int DayIndex = 1;
        public int Gold;
        public int DarkTrace;
        public int PrisonersSeen;
        public readonly List<string> PrisonerNames = new List<string>();
        public readonly List<string> ManifestedMonsterIds = new List<string>();
        public readonly List<RunMonsterState> PartyMembers = new List<RunMonsterState>();

        /*
         * 선택한 몬스터로 첫 스테이지의 런 진행 상태를 만든다.
         */
        public static RunSession Begin(MonsterDefinition monster /* 몬스터 */)
        {
            var session = new RunSession
            {
                SelectedMonsterId = monster.MonsterId,
                SelectedMonsterName = monster.DisplayName,
                ActiveSkillId = ResolveDefaultActiveSkillId(monster),
                PassiveSkillId = ResolveDefaultPassiveSkillId(monster),
                ActiveSkillName = monster.ActiveSkillName,
                PassiveSkillName = monster.PassiveSkillName,
                StageIndex = 1,
                DayIndex = 1
            };

            session.EnsurePartyMemberState(monster);
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
         * 런 시작 시 사용할 F 슬롯 기본 패시브를 찾는다.
         */
        private static string ResolveDefaultPassiveSkillId(MonsterDefinition monster /* 몬스터 */)
        {
            if (monster == null || monster.PassiveSkills == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < monster.PassiveSkills.Length; i++)
            {
                var passive = monster.PassiveSkills[i];
                if (passive != null
                    && passive.Slot == SkillSlot.F
                    && passive.IsAvailableWithoutActiveRequirement
                    && !string.IsNullOrWhiteSpace(passive.PassiveId))
                {
                    return passive.PassiveId;
                }
            }

            return string.Empty;
        }

        /*
         * 지정한 파티원이 이미 선택한 보상인지 확인한다.
         */
        public bool HasChosenReward(string monsterId /* 몬스터 식별자 */, string rewardId /* 보상 식별자 */)
        {
            RunMonsterState member = GetPartyMemberState(monsterId);
            return member.ChosenRewardIds.Contains(rewardId);
        }

        /*
         * 지정한 파티원이 선택한 Offering 보상과 습득 스킬을 기록한다.
         */
        public void RecordOfferingChoice(
            string monsterId /* 몬스터 식별자 */,
            string rewardId /* 보상 식별자 */,
            string linkedChoiceId /* 연결된 선택지 식별자 */,
            string activeSkillId /* 액티브 스킬 식별자 */,
            string passiveSkillId /* 패시브 스킬 식별자 */)
        {
            RunMonsterState member = GetPartyMemberState(monsterId);
            if (!string.IsNullOrWhiteSpace(rewardId) && !HasChosenReward(monsterId, rewardId))
            {
                member.ChosenRewardIds.Add(rewardId);
            }

            if (!string.IsNullOrWhiteSpace(linkedChoiceId) && !member.ChosenChoiceIds.Contains(linkedChoiceId))
            {
                member.ChosenChoiceIds.Add(linkedChoiceId);
            }

            if (!string.IsNullOrWhiteSpace(activeSkillId) && !HasLearnedActive(monsterId, activeSkillId))
            {
                member.LearnedActives.Add(activeSkillId);
            }

            if (!string.IsNullOrWhiteSpace(passiveSkillId) && !HasLearnedPassive(monsterId, passiveSkillId))
            {
                member.LearnedPassives.Add(passiveSkillId);
            }
        }

        /*
         * 지정한 파티원이 액티브 스킬을 습득했는지 확인한다.
         */
        public bool HasLearnedActive(string monsterId /* 몬스터 식별자 */, string activeSkillId /* 액티브 스킬 식별자 */)
        {
            RunMonsterState member = GetPartyMemberState(monsterId);
            return member.LearnedActives.Contains(activeSkillId);
        }

        /*
         * 지정한 파티원이 패시브 스킬을 습득했는지 확인한다.
         */
        public bool HasLearnedPassive(string monsterId /* 몬스터 식별자 */, string passiveSkillId /* 패시브 스킬 식별자 */)
        {
            RunMonsterState member = GetPartyMemberState(monsterId);
            return member.LearnedPassives.Contains(passiveSkillId);
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
         * 선택한 포로를 획득 목록에 추가한다.
         */
        public void ClaimPrisonerReward(string prisonerName /* 수감자 이름 */)
        {
            if (string.IsNullOrWhiteSpace(prisonerName))
            {
                return;
            }

            PrisonersSeen += 1;
            PrisonerNames.Add(prisonerName);
        }

        /*
         * 지정한 몬스터가 이미 현현했는지 확인한다.
         */
        public bool HasManifestedMonster(string monsterId /* 몬스터 식별자 */)
        {
            return ManifestedMonsterIds.Contains(monsterId);
        }

        /*
         * 새로 현현한 몬스터를 파티 목록과 진행 상태에 등록한다.
         */
        public void RecordManifestedMonster(MonsterDefinition monster /* 몬스터 */)
        {
            if (monster == null
                || string.IsNullOrWhiteSpace(monster.MonsterId)
                || IsSelectedMonster(monster.MonsterId)
                || HasManifestedMonster(monster.MonsterId))
            {
                return;
            }

            ManifestedMonsterIds.Add(monster.MonsterId);
            EnsurePartyMemberState(monster);
        }

        /*
         * 지정한 파티원의 영구 최대 체력 증가값을 기록한다.
         */
        public void AddMaxHealthBonus(
            string monsterId /* 몬스터 식별자 */,
            float maxHealthBonus /* 최대 체력 추가값 */)
        {
            RunMonsterState member = GetPartyMemberState(monsterId);
            member.MaxHealthBonus += maxHealthBonus;
        }

        /*
         * 몬스터의 파티 진행 상태가 없으면 기본 스킬과 함께 새로 만든다.
         */
        public RunMonsterState EnsurePartyMemberState(MonsterDefinition monster /* 몬스터 */)
        {
            if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                return null;
            }

            var existing = GetPartyMemberState(monster.MonsterId);
            if (existing != null)
            {
                return existing;
            }

            var state = new RunMonsterState
            {
                MonsterId = monster.MonsterId,
                MonsterName = monster.DisplayName
            };

            state.LearnedActives.Add(ResolveDefaultActiveSkillId(monster));
            PartyMembers.Add(state);

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

            for (var i = 0; i < PartyMembers.Count; i++)
            {
                var member = PartyMembers[i];
                if (member != null && string.Equals(member.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                {
                    return member;
                }
            }

            return null;
        }

        /*
         * 지정한 몬스터가 런 시작 시 선택한 몬스터인지 확인한다.
         */
        private bool IsSelectedMonster(string monsterId /* 몬스터 식별자 */)
        {
            return !string.IsNullOrWhiteSpace(monsterId)
                && string.Equals(SelectedMonsterId, monsterId, StringComparison.OrdinalIgnoreCase);
        }

    }
}
