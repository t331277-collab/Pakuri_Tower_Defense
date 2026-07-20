using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.InGame
{
    /*
     * 유닛이 보유한 스킬 런타임 목록을 관리한다.
     */
    public sealed class UnitSkillRuntimeSet
    {
        private readonly List<SkillRuntimeInstance> activeSkills = new List<SkillRuntimeInstance>();
        private readonly List<SkillRuntimeInstance> passiveSkills = new List<SkillRuntimeInstance>();

        public IReadOnlyList<SkillRuntimeInstance> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillRuntimeInstance> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /*
         * 유닛의 스킬 런타임 목록을 비운다.
         */
        public void Clear()
        {
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /*
         * 같은 ID의 스킬을 교체하거나 새 스킬을 추가한다.
         */
        public void AddOrReplace(SkillRuntimeInstance instance)
        {
            var skills = instance.Data.IsActive ? activeSkills : passiveSkills;
            var existingIndex = FindIndexBySkillId(skills, instance.SkillId);
            if (existingIndex >= 0)
            {
                skills[existingIndex] = instance;
                return;
            }

            skills.Add(instance);
        }

        /*
         * 스킬 ID가 일치하는 런타임을 찾는다.
         */
        public SkillRuntimeInstance FindBySkillId(string skillId)
        {
            var index = FindIndexBySkillId(activeSkills, skillId);
            if (index >= 0)
            {
                return activeSkills[index];
            }

            index = FindIndexBySkillId(passiveSkills, skillId);
            return index >= 0 ? passiveSkills[index] : null;
        }

        /*
         * 선택지 ID가 일치하는 컴파일 결과를 찾는다.
         */
        public SkillChoiceRuntimeData FindChoice(string choiceId)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var choice = FindChoice(activeSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var choice = FindChoice(passiveSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            return null;
        }

        /*
         * 스킬 슬롯이 일치하는 런타임을 찾는다.
         */
        public SkillRuntimeInstance FindBySlot(SkillSlot slot)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                if (activeSkills[i] != null && activeSkills[i].Slot == slot)
                {
                    return activeSkills[i];
                }
            }

            return null;
        }

        /*
         * 유닛이 보유한 모든 스킬 런타임 시간을 갱신한다.
         */
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i]?.Tick(deltaTime);
            }
        }

        /*
         * 스킬 ID가 일치하는 런타임의 목록 위치를 찾는다.
         */
        private static int FindIndexBySkillId(List<SkillRuntimeInstance> skills, string skillId)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime != null && string.Equals(runtime.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        private static SkillChoiceRuntimeData FindChoice(SkillRuntimeData skill, string choiceId)
        {
            var choice = FindChoice(skill.EnhancementChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            choice = FindChoice(skill.MasterChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            var passive = skill as PassiveSkillRuntimeData;
            return passive != null ? FindChoice(passive.BaseModifierChoices, choiceId) : null;
        }

        private static SkillChoiceRuntimeData FindChoice(SkillChoiceRuntimeData[] choices, string choiceId)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                if (string.Equals(choices[i].ChoiceId, choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return choices[i];
                }
            }

            return null;
        }
    }
}
