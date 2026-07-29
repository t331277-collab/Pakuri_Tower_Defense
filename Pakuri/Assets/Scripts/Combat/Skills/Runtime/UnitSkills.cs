/*
 * 역할: 유닛별 학습 스킬 소유.
 * 책임: 학습한 액티브·패시브 스킬과 강화·마스터 선택을 보관하고 조회한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.InGame
{

    /// <summary>한 유닛이 학습한 액티브·패시브 스킬과 강화·마스터 선택을 보관한다.</summary>
    [Serializable]
    public class UnitSkills
    {

        private readonly HashSet<string> learnedActiveSkillIds = new HashSet<string>();
        private readonly HashSet<string> learnedPassiveSkillIds = new HashSet<string>();
        private readonly HashSet<string> chosenEnhancementIds = new HashSet<string>();
        private readonly HashSet<string> chosenMasterSkillIds = new HashSet<string>();

        public IReadOnlyCollection<string> LearnedActiveSkillIds => learnedActiveSkillIds;
        public IReadOnlyCollection<string> LearnedPassiveSkillIds => learnedPassiveSkillIds;
        public IReadOnlyCollection<string> ChosenEnhancementIds => chosenEnhancementIds;
        public IReadOnlyCollection<string> ChosenMasterSkillIds => chosenMasterSkillIds;

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 <c>Choice</c>를 소유한 런타임 상태에 추가한다.</summary>
        public void AddChoice(string choiceId)
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

        /// <summary>전달된 <c>skillId</c> 값을 사용해 <c>ActiveSkill</c>를 소유한 런타임 상태에 추가한다.</summary>
        public void AddActiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedActiveSkillIds.Add(skillId);
            }
        }

        /// <summary>전달된 <c>skillId</c> 값을 사용해 소유한 런타임 상태에 <c>ActiveSkill</c>가 있는지 반환한다.</summary>
        public bool HasActiveSkill(string skillId)
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

        /// <summary>전달된 <c>skillId</c> 값을 사용해 <c>ActiveSkill</c>를 소유한 런타임 상태에서 제거한다.</summary>
        public void RemoveActiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedActiveSkillIds.Remove(skillId);
            }
        }

        /// <summary>전달된 <c>skillId</c> 값을 사용해 <c>PassiveSkill</c>를 소유한 런타임 상태에 추가한다.</summary>
        public void AddPassiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedPassiveSkillIds.Add(skillId);
            }
        }

        /// <summary>전달된 <c>skillId</c> 값을 사용해 소유한 런타임 상태에 <c>PassiveSkill</c>가 있는지 반환한다.</summary>
        public bool HasPassiveSkill(string skillId)
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

        /// <summary>전달된 <c>skillId</c> 값을 사용해 <c>PassiveSkill</c>를 소유한 런타임 상태에서 제거한다.</summary>
        public void RemovePassiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedPassiveSkillIds.Remove(skillId);
            }
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 <c>Enhancement</c>를 소유한 런타임 상태에 추가한다.</summary>
        public void AddEnhancement(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenEnhancementIds.Add(choiceId);
            }
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 소유한 런타임 상태에 <c>Enhancement</c>가 있는지 반환한다.</summary>
        public bool HasEnhancement(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenEnhancementIds.Contains(choiceId);
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 <c>Enhancement</c>를 소유한 런타임 상태에서 제거한다.</summary>
        public void RemoveEnhancement(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenEnhancementIds.Remove(choiceId);
            }
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 <c>MasterSkill</c>를 소유한 런타임 상태에 추가한다.</summary>
        public void AddMasterSkill(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenMasterSkillIds.Add(choiceId);
            }
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 소유한 런타임 상태에 <c>MasterSkill</c>가 있는지 반환한다.</summary>
        public bool HasMasterSkill(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenMasterSkillIds.Contains(choiceId);
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 <c>MasterSkill</c>를 소유한 런타임 상태에서 제거한다.</summary>
        public void RemoveMasterSkill(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenMasterSkillIds.Remove(choiceId);
            }
        }

        /// <summary>전달된 <c>choiceId</c> 값을 사용해 소유한 런타임 상태에 <c>Choice</c>가 있는지 반환한다.</summary>
        public bool HasChoice(string choiceId)
        {
            return HasEnhancement(choiceId) || HasMasterSkill(choiceId);
        }

        /// <summary><c>소유한 모든 런타임 값</c>를 소유한 런타임 상태에서 비운다.</summary>
        public void Clear()
        {
            learnedActiveSkillIds.Clear();
            learnedPassiveSkillIds.Clear();
            chosenEnhancementIds.Clear();
            chosenMasterSkillIds.Clear();
        }
    }
}
