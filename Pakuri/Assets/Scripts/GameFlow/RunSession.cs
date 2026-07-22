using System;
using System.Collections.Generic;
using Pakuri.Data;

/*
 * 한 번의 런에서 유지되는 진행 상태와 파티별 성장 상태를 보관한다.
 * 스테이지·일차·전투 종류, 재화·포로, 선택 및 현현 몬스터,
 * 몬스터별 학습 스킬과 Choice를 기록하고 보상 적용과 다음 날짜 진행을 처리한다.
 */
namespace Pakuri.InGame
{
    public enum RunCombatType
    {
        Normal,
        Elite,
        Day5Midboss,
        Day10Midboss,
        Boss,
        Shop
    }

    [Serializable]
    public readonly struct RunDayModel
    {
        /*
         * RunDayModel에 필요한 값을 초기화한다.
         */
        public RunDayModel(int stageIndex, int dayIndex, RunCombatType combatType, bool hasEliteOption, bool hasShopOption)
        {
            StageIndex = Math.Max(1, Math.Min(stageIndex, 4));
            DayIndex = Math.Max(1, Math.Min(dayIndex, 11));
            CombatType = combatType;
            HasEliteOption = hasEliteOption;
            HasShopOption = hasShopOption;
        }

        public int StageIndex { get; }
        public int DayIndex { get; }
        public RunCombatType CombatType { get; }
        public bool HasEliteOption { get; }
        public bool HasShopOption { get; }

        /*
         * Resolve 결과를 계산해 반환한다.
         */
        public static RunDayModel Resolve(int stageIndex, int dayIndex)
        {
            var clampedDay = Math.Max(1, Math.Min(dayIndex, 11));
            if (clampedDay == 5)
            {
                return new RunDayModel(stageIndex, clampedDay, RunCombatType.Day5Midboss, false, false);
            }

            if (clampedDay == 10)
            {
                return new RunDayModel(stageIndex, clampedDay, RunCombatType.Day10Midboss, false, false);
            }

            if (clampedDay == 11)
            {
                return new RunDayModel(stageIndex, clampedDay, RunCombatType.Boss, false, false);
            }

            var canOfferElite = clampedDay >= 2 && clampedDay <= 4 || clampedDay >= 6 && clampedDay <= 9;
            var canOfferShop = clampedDay >= 6 && clampedDay <= 9;
            return new RunDayModel(stageIndex, clampedDay, RunCombatType.Normal, canOfferElite, canOfferShop);
        }
    }

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

        /*
         * Begin 작업 결과를 반환한다.
         */
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

        /*
         * ResolveDefaultActiveSkillId 결과를 계산해 반환한다.
         */
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

        /*
         * ResolveDefaultPassiveSkillId 결과를 계산해 반환한다.
         */
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

        /*
         * HasChosenReward 조건을 만족하는지 확인한다.
         */
        public bool HasChosenReward(string rewardId)
        {
            return HasChosenReward(SelectedMonsterId, rewardId);
        }

        /*
         * HasChosenReward 조건을 만족하는지 확인한다.
         */
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

        /*
         * RecordOfferingChoice 작업을 수행한다.
         */
        public void RecordOfferingChoice(string rewardId, string linkedChoiceId, string activeSkillId, string passiveSkillId)
        {
            RecordOfferingChoice(SelectedMonsterId, rewardId, linkedChoiceId, activeSkillId, passiveSkillId);
        }

        /*
         * RecordOfferingChoice 작업을 수행한다.
         */
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

        /*
         * AddLearnedActive 작업을 수행한다.
         */
        public void AddLearnedActive(string activeSkillId)
        {
            AddLearnedActive(SelectedMonsterId, activeSkillId);
        }

        /*
         * AddLearnedActive 작업을 수행한다.
         */
        public void AddLearnedActive(string monsterId, string activeSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            AddUniqueText(member != null ? member.LearnedActives : LearnedActives, activeSkillId);
            if (IsSelectedMonster(monsterId))
            {
                AddUniqueText(LearnedActives, activeSkillId);
            }
        }

        /*
         * AddLearnedPassive 작업을 수행한다.
         */
        public void AddLearnedPassive(string passiveSkillId)
        {
            AddLearnedPassive(SelectedMonsterId, passiveSkillId);
        }

        /*
         * AddLearnedPassive 작업을 수행한다.
         */
        public void AddLearnedPassive(string monsterId, string passiveSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            AddUniqueText(member != null ? member.LearnedPassives : LearnedPassives, passiveSkillId);
            if (IsSelectedMonster(monsterId))
            {
                AddUniqueText(LearnedPassives, passiveSkillId);
            }
        }

        /*
         * HasLearnedActive 조건을 만족하는지 확인한다.
         */
        public bool HasLearnedActive(string activeSkillId)
        {
            return HasLearnedActive(SelectedMonsterId, activeSkillId);
        }

        /*
         * HasLearnedActive 조건을 만족하는지 확인한다.
         */
        public bool HasLearnedActive(string monsterId, string activeSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            return ContainsText(member != null ? member.LearnedActives : LearnedActives, activeSkillId);
        }

        /*
         * HasLearnedPassive 조건을 만족하는지 확인한다.
         */
        public bool HasLearnedPassive(string passiveSkillId)
        {
            return HasLearnedPassive(SelectedMonsterId, passiveSkillId);
        }

        /*
         * HasLearnedPassive 조건을 만족하는지 확인한다.
         */
        public bool HasLearnedPassive(string monsterId, string passiveSkillId)
        {
            var member = GetPartyMemberState(monsterId);
            return ContainsText(member != null ? member.LearnedPassives : LearnedPassives, passiveSkillId);
        }

        /*
         * AddUniqueText 작업을 수행한다.
         */
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

        /*
         * ContainsText 조건을 만족하는지 확인한다.
         */
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

        /*
         * ApplyPostCombatSummary 처리를 대상에 적용한다.
         */
        public void ApplyPostCombatSummary(int goldReward, int darkTraceReward, int prisonerCount)
        {
            ApplyPostCombatSummary(goldReward, darkTraceReward, prisonerCount, null);
        }

        /*
         * ApplyPostCombatSummary 처리를 대상에 적용한다.
         */
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

        /*
         * ClaimMaterialReward 작업을 수행한다.
         */
        public void ClaimMaterialReward(int goldReward, int darkTraceReward)
        {
            Gold += Math.Max(0, goldReward);
            DarkTrace += Math.Max(0, darkTraceReward);
        }

        /*
         * ClaimPrisonerReward 작업을 수행한다.
         */
        public void ClaimPrisonerReward(string prisonerName)
        {
            if (string.IsNullOrWhiteSpace(prisonerName))
            {
                return;
            }

            PrisonersSeen += 1;
            PrisonerNames.Add(prisonerName);
        }

        /*
         * HasManifestedMonster 조건을 만족하는지 확인한다.
         */
        public bool HasManifestedMonster(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            return ContainsText(ManifestedMonsterIds, monsterId);
        }

        /*
         * RecordManifestedMonster 작업을 수행한다.
         */
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

        /*
         * RecordManifestedMonster 작업을 수행한다.
         */
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

        /*
         * AccumulateReward 작업을 수행한다.
         */
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

        /*
         * AccumulateReward 작업을 수행한다.
         */
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

        /*
         * EnsurePartyMemberState에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
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

        /*
         * GetPartyMemberState에 해당하는 값을 찾아 반환한다.
         */
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

        /*
         * IsSelectedMonster 조건을 만족하는지 확인한다.
         */
        private bool IsSelectedMonster(string monsterId)
        {
            return !string.IsNullOrWhiteSpace(monsterId)
                && string.Equals(SelectedMonsterId, monsterId, StringComparison.OrdinalIgnoreCase);
        }

        /*
         * CopyUnique에 필요한 값을 복사한다.
         */
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

        /*
         * AdvanceDay 작업을 수행한다.
         */
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

        /*
         * RefreshDayModel 대상의 현재 상태를 갱신한다.
         */
        public void RefreshDayModel()
        {
            CurrentDayModel = RunDayModel.Resolve(StageIndex, DayIndex);
            CurrentCombatType = CurrentDayModel.CombatType;
        }
    }
}
