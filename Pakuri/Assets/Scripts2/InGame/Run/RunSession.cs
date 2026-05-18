using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.Run
{
    [Serializable]
    public class RunSession
    {
        [Serializable]
        public sealed class RunMonsterState
        {
            public string MonsterId;
            public string MonsterName;
            public readonly List<string> LearnedActives = new List<string>();
            public readonly List<string> LearnedPassives = new List<string>();
            public readonly List<string> ChosenRewardIds = new List<string>();
            public readonly List<string> ChosenChoiceIds = new List<string>();
            public float DamageMultiplier = 1f;
            public int MagazineBonus;
            public float ShotIntervalMultiplier = 1f;
            public float ReloadDurationMultiplier = 1f;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
        }

        public string SelectedMonsterId;
        public string SelectedMonsterName;
        public string ActiveSkillId;
        public string PassiveSkillId;
        public string ActiveSkillName;
        public string PassiveSkillName;
        public int StageIndex = 1;
        public int DayIndex = 1;
        public RunCombatType CurrentCombatType = RunCombatType.Normal;
        public RunDayModel CurrentDayModel;
        public int Gold;
        public int DarkTrace;
        public int PrisonersSeen;
        public float DamageMultiplier = 1f;
        public int MagazineBonus;
        public float ShotIntervalMultiplier = 1f;
        public float ReloadDurationMultiplier = 1f;
        public float MaxHealthBonus;
        public float StatusChanceBonus;
        public readonly List<string> LearnedActives = new List<string>();
        public readonly List<string> LearnedPassives = new List<string>();
        public readonly List<string> ChosenRewardIds = new List<string>();
        public readonly List<string> ChosenChoiceIds = new List<string>();
        public readonly List<string> PrisonerNames = new List<string>();
        public readonly List<string> ManifestedMonsterIds = new List<string>();
        public readonly List<RunMonsterState> PartyMembers = new List<RunMonsterState>();

        public static RunSession Begin(MonsterDefinition monster)
        {
            var session = new RunSession
            {
                SelectedMonsterId = monster != null ? monster.MonsterId : string.Empty,
                SelectedMonsterName = monster != null ? monster.DisplayName : "Unknown",
                ActiveSkillId = ResolveDefaultActiveSkillId(monster),
                PassiveSkillId = ResolveDefaultPassiveSkillId(monster),
                ActiveSkillName = monster != null ? monster.ActiveSkillName : string.Empty,
                PassiveSkillName = monster != null ? monster.PassiveSkillName : string.Empty,
                StageIndex = 1,
                DayIndex = 1
            };

            if (!string.IsNullOrWhiteSpace(session.ActiveSkillId))
            {
                session.AddLearnedActive(session.ActiveSkillId);
            }

            session.EnsurePartyMemberState(monster);
            session.RefreshDayModel();
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
                if (skill != null && skill.IsDefaultLearned && !string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    return skill.SkillId;
                }
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill != null && skill.Slot == SkillSlot.A && !string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    return skill.SkillId;
                }
            }

            return string.Empty;
        }

        private static string ResolveDefaultPassiveSkillId(MonsterDefinition monster)
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

        public bool HasChosenReward(string rewardId)
        {
            return HasChosenReward(SelectedMonsterId, rewardId);
        }

        public bool HasChosenReward(string monsterId, string rewardId)
        {
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return false;
            }

            var member = GetPartyMemberState(monsterId);
            var values = member != null ? member.ChosenRewardIds : ChosenRewardIds;
            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], rewardId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void RecordRewardChoice(string rewardId, string passiveIdIfUnlocked)
        {
            if (!string.IsNullOrWhiteSpace(rewardId) && !HasChosenReward(rewardId))
            {
                ChosenRewardIds.Add(rewardId);
            }

            AddLearnedPassive(passiveIdIfUnlocked);
        }

        public void RecordOfferingChoice(string rewardId, string linkedChoiceId, string activeSkillId, string passiveSkillId)
        {
            RecordOfferingChoice(SelectedMonsterId, rewardId, linkedChoiceId, activeSkillId, passiveSkillId);
        }

        public void RecordOfferingChoice(
            string monsterId,
            string rewardId,
            string linkedChoiceId,
            string activeSkillId,
            string passiveSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            var chosenRewards = member != null ? member.ChosenRewardIds : ChosenRewardIds;
            if (!string.IsNullOrWhiteSpace(rewardId) && !HasChosenReward(monsterId, rewardId))
            {
                AddUniqueText(chosenRewards, rewardId);
                if (IsSelectedMonster(monsterId))
                {
                    AddUniqueText(ChosenRewardIds, rewardId);
                }
            }

            var chosenChoices = member != null ? member.ChosenChoiceIds : ChosenChoiceIds;
            AddUniqueText(chosenChoices, linkedChoiceId);
            if (IsSelectedMonster(monsterId))
            {
                AddUniqueText(ChosenChoiceIds, linkedChoiceId);
            }

            AddLearnedActive(monsterId, activeSkillId);
            AddLearnedPassive(monsterId, passiveSkillId);
        }

        public void AddLearnedActive(string activeSkillId)
        {
            AddLearnedActive(SelectedMonsterId, activeSkillId);
        }

        public void AddLearnedActive(string monsterId, string activeSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            AddUniqueText(member != null ? member.LearnedActives : LearnedActives, activeSkillId);
            if (IsSelectedMonster(monsterId))
            {
                AddUniqueText(LearnedActives, activeSkillId);
            }
        }

        public void AddLearnedPassive(string passiveSkillId)
        {
            AddLearnedPassive(SelectedMonsterId, passiveSkillId);
        }

        public void AddLearnedPassive(string monsterId, string passiveSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            AddUniqueText(member != null ? member.LearnedPassives : LearnedPassives, passiveSkillId);
            if (IsSelectedMonster(monsterId))
            {
                AddUniqueText(LearnedPassives, passiveSkillId);
            }
        }

        public bool HasLearnedActive(string activeSkillId)
        {
            return HasLearnedActive(SelectedMonsterId, activeSkillId);
        }

        public bool HasLearnedActive(string monsterId, string activeSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            return ContainsText(member != null ? member.LearnedActives : LearnedActives, activeSkillId);
        }

        public bool HasLearnedPassive(string passiveSkillId)
        {
            return HasLearnedPassive(SelectedMonsterId, passiveSkillId);
        }

        public bool HasLearnedPassive(string monsterId, string passiveSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            return ContainsText(member != null ? member.LearnedPassives : LearnedPassives, passiveSkillId);
        }

        private static void AddUniqueText(List<string> values, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            if (!ContainsText(values, value))
            {
                values.Add(value);
            }
        }

        private static bool ContainsText(IReadOnlyList<string> values, string target)
        {
            if (string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void ApplyPostCombatSummary(int goldReward, int darkTraceReward, int prisonerCount)
        {
            ApplyPostCombatSummary(goldReward, darkTraceReward, prisonerCount, null);
        }

        public void ApplyPostCombatSummary(int goldReward, int darkTraceReward, int prisonerCount, IReadOnlyList<string> prisonerNames)
        {
            Gold += goldReward;
            DarkTrace += darkTraceReward;
            PrisonersSeen += prisonerCount;

            if (prisonerNames == null)
            {
                return;
            }

            for (var i = 0; i < prisonerNames.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(prisonerNames[i]))
                {
                    PrisonerNames.Add(prisonerNames[i]);
                }
            }
        }

        public void ClaimMaterialReward(int goldReward, int darkTraceReward)
        {
            Gold += Math.Max(0, goldReward);
            DarkTrace += Math.Max(0, darkTraceReward);
        }

        public void ClaimPrisonerReward(string prisonerName)
        {
            if (string.IsNullOrWhiteSpace(prisonerName))
            {
                return;
            }

            PrisonersSeen += 1;
            PrisonerNames.Add(prisonerName);
        }

        public bool HasManifestedMonster(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            return ContainsText(ManifestedMonsterIds, monsterId);
        }

        public void RecordManifestedMonster(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId)
                || IsSelectedMonster(monsterId)
                || HasManifestedMonster(monsterId))
            {
                return;
            }

            ManifestedMonsterIds.Add(monsterId);
        }

        public void RecordManifestedMonster(MonsterDefinition monster)
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

        public void AccumulateReward(
            float damageMultiplier,
            int magazineBonus,
            float shotIntervalMultiplier,
            float reloadDurationMultiplier,
            float maxHealthBonus,
            float statusChanceBonus)
        {
            DamageMultiplier *= damageMultiplier > 0f ? damageMultiplier : 1f;
            MagazineBonus += magazineBonus;
            ShotIntervalMultiplier *= shotIntervalMultiplier > 0f ? shotIntervalMultiplier : 1f;
            ReloadDurationMultiplier *= reloadDurationMultiplier > 0f ? reloadDurationMultiplier : 1f;
            MaxHealthBonus += maxHealthBonus;
            StatusChanceBonus += statusChanceBonus;
        }

        public void AccumulateReward(
            string monsterId,
            float damageMultiplier,
            int magazineBonus,
            float shotIntervalMultiplier,
            float reloadDurationMultiplier,
            float maxHealthBonus,
            float statusChanceBonus)
        {
            var member = GetPartyMemberState(monsterId);
            if (member == null)
            {
                AccumulateReward(damageMultiplier, magazineBonus, shotIntervalMultiplier, reloadDurationMultiplier, maxHealthBonus, statusChanceBonus);
                return;
            }

            if (IsSelectedMonster(monsterId))
            {
                AccumulateReward(damageMultiplier, magazineBonus, shotIntervalMultiplier, reloadDurationMultiplier, maxHealthBonus, statusChanceBonus);
            }

            member.DamageMultiplier *= damageMultiplier > 0f ? damageMultiplier : 1f;
            member.MagazineBonus += magazineBonus;
            member.ShotIntervalMultiplier *= shotIntervalMultiplier > 0f ? shotIntervalMultiplier : 1f;
            member.ReloadDurationMultiplier *= reloadDurationMultiplier > 0f ? reloadDurationMultiplier : 1f;
            member.MaxHealthBonus += maxHealthBonus;
            member.StatusChanceBonus += statusChanceBonus;
        }

        public RunMonsterState EnsurePartyMemberState(MonsterDefinition monster)
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

            AddUniqueText(state.LearnedActives, ResolveDefaultActiveSkillId(monster));
            PartyMembers.Add(state);

            if (string.Equals(monster.MonsterId, SelectedMonsterId, StringComparison.OrdinalIgnoreCase))
            {
                CopyUnique(LearnedActives, state.LearnedActives);
                CopyUnique(LearnedPassives, state.LearnedPassives);
                CopyUnique(ChosenRewardIds, state.ChosenRewardIds);
                CopyUnique(ChosenChoiceIds, state.ChosenChoiceIds);
            }

            return state;
        }

        public RunMonsterState GetPartyMemberState(string monsterId)
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

        private bool IsSelectedMonster(string monsterId)
        {
            return !string.IsNullOrWhiteSpace(monsterId)
                && string.Equals(SelectedMonsterId, monsterId, StringComparison.OrdinalIgnoreCase);
        }

        private static void CopyUnique(IReadOnlyList<string> source, List<string> target)
        {
            if (source == null || target == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                AddUniqueText(target, source[i]);
            }
        }

        public void AdvanceDay()
        {
            DayIndex += 1;
            if (DayIndex <= 11)
            {
                RefreshDayModel();
                return;
            }

            DayIndex = 1;
            StageIndex = Math.Min(StageIndex + 1, 4);
            RefreshDayModel();
        }

        public void RefreshDayModel()
        {
            CurrentDayModel = RunDayModel.Resolve(StageIndex, DayIndex);
            CurrentCombatType = CurrentDayModel.CombatType;
        }
    }
}
