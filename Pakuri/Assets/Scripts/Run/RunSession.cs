using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.Run
{
    [Serializable]
    public class RunSession
    {
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
        public readonly List<string> PrisonerNames = new List<string>();

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
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return false;
            }

            for (var i = 0; i < ChosenRewardIds.Count; i++)
            {
                if (string.Equals(ChosenRewardIds[i], rewardId, StringComparison.OrdinalIgnoreCase))
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

        public void RecordOfferingChoice(string choiceId, string activeSkillId, string passiveSkillId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId) && !HasChosenReward(choiceId))
            {
                ChosenRewardIds.Add(choiceId);
            }

            AddLearnedActive(activeSkillId);
            AddLearnedPassive(passiveSkillId);
        }

        public void AddLearnedActive(string activeSkillId)
        {
            AddUniqueText(LearnedActives, activeSkillId);
        }

        public void AddLearnedPassive(string passiveSkillId)
        {
            AddUniqueText(LearnedPassives, passiveSkillId);
        }

        public bool HasLearnedActive(string activeSkillId)
        {
            return ContainsText(LearnedActives, activeSkillId);
        }

        public bool HasLearnedPassive(string passiveSkillId)
        {
            return ContainsText(LearnedPassives, passiveSkillId);
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
