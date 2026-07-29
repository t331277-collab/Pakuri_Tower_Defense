using System;
using System.Collections.Generic;
using Pakuri.Data;

/*
 * 유닛이 학습한 액티브·패시브 스킬과 선택한 강화·마스터 ID를 보관한다.
 * 전투 실행 상태나 스킬 효과를 계산하지 않고 확정된 학습 결과의
 * 추가, 조회, 삭제만 담당
 */
namespace Pakuri.InGame
{
    [Serializable]
    public class UnitSkills
    {
        // 유닛이 학습하거나 선택한 스킬·강화 ID 저장소를 구현.
        private readonly HashSet<string> learnedActiveSkillIds = new HashSet<string>();
        private readonly HashSet<string> learnedPassiveSkillIds = new HashSet<string>();
        private readonly HashSet<string> chosenEnhancementIds = new HashSet<string>();
        private readonly HashSet<string> chosenMasterSkillIds = new HashSet<string>();

        public IReadOnlyCollection<string> LearnedActiveSkillIds => learnedActiveSkillIds;
        public IReadOnlyCollection<string> LearnedPassiveSkillIds => learnedPassiveSkillIds;
        public IReadOnlyCollection<string> ChosenEnhancementIds => chosenEnhancementIds;
        public IReadOnlyCollection<string> ChosenMasterSkillIds => chosenMasterSkillIds;

        /*
         * 선택한 Choice ID를 강화 또는 마스터 저장소에 추가한다.
         */
        public void AddChoice(string choiceId /* 선택한 Choice 식별자 */)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return;
            }

            if (!GameDataLoader.CurrentCatalog.TryGetData(choiceId, out SkillChoice choice))
            {
                throw new InvalidOperationException($"Unknown learned skill choice '{choiceId}'.");
            }

            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster)
            {
                AddMasterSkill(choiceId);
            }
            else
            {
                AddEnhancement(choiceId);
            }
        }

        /*
         * 학습한 액티브 스킬 ID를 추가한다.
         */
        public void AddActiveSkill(string skillId /* 학습한 액티브 스킬 식별자 */)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedActiveSkillIds.Add(skillId);
            }
        }

        /*
         * 액티브 스킬을 학습했는지 확인한다.
         */
        public bool HasActiveSkill(string skillId /* 확인할 액티브 스킬 식별자 */)
        {
            foreach (var learnedSkillId in learnedActiveSkillIds)
            {
                if (string.Equals(learnedSkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 학습한 액티브 스킬 ID를 삭제한다.
         */
        public void RemoveActiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedActiveSkillIds.Remove(skillId);
            }
        }

        /*
         * 학습한 패시브 스킬 ID를 추가한다.
         */
        public void AddPassiveSkill(string skillId )
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedPassiveSkillIds.Add(skillId);
            }
        }

        /*
         * 패시브 스킬을 학습했는지 확인한다.
         */
        public bool HasPassiveSkill(string skillId )
        {
            foreach (var learnedSkillId in learnedPassiveSkillIds)
            {
                if (string.Equals(learnedSkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 학습한 패시브 스킬 ID를 삭제한다.
         */
        public void RemovePassiveSkill(string skillId )
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedPassiveSkillIds.Remove(skillId);
            }
        }

        /*
         * 선택한 강화 효과 ID를 추가한다.
         */
        public void AddEnhancement(string choiceId /* 선택한 강화 효과 식별자 */)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenEnhancementIds.Add(choiceId);
            }
        }

        /*
         * 강화 효과를 학습했는지 확인
         */
        public bool HasEnhancement(string choiceId /* 확인할 강화 효과 식별자 */)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenEnhancementIds.Contains(choiceId);
        }

        /*
         * 선택한 강화 효과 ID를 삭제한다.
         */
        public void RemoveEnhancement(string choiceId /* 삭제할 강화 효과 식별자 */)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenEnhancementIds.Remove(choiceId);
            }
        }

        /*
         * 선택한 마스터 스킬 ID를 추가한다.
         */
        public void AddMasterSkill(string choiceId /* 선택한 마스터 스킬 식별자 */)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenMasterSkillIds.Add(choiceId);
            }
        }

        /*
         * 마스터 스킬을 선택했는지 확인한다.
         */
        public bool HasMasterSkill(string choiceId /* 확인할 마스터 스킬 식별자 */)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenMasterSkillIds.Contains(choiceId);
        }

        /*
         * 선택한 마스터 스킬 ID를 삭제한다.
         */
        public void RemoveMasterSkill(string choiceId /* 삭제할 마스터 스킬 식별자 */)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenMasterSkillIds.Remove(choiceId);
            }
        }

        /*
         * 강화 효과나 마스터 스킬을 선택했는지 확인한다.
         */
        public bool HasChoice(string choiceId /* 확인할 선택지 식별자 */)
        {
            return HasEnhancement(choiceId) || HasMasterSkill(choiceId);
        }

        /*
         * 모든 학습 스킬과 선택 결과를 삭제한다.
         */
        public void Clear()
        {
            learnedActiveSkillIds.Clear();
            learnedPassiveSkillIds.Clear();
            chosenEnhancementIds.Clear();
            chosenMasterSkillIds.Clear();
        }
    }
}
