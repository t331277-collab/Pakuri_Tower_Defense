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

        public IReadOnlyList<SkillRuntimeInstance> ActiveSkills => activeSkills;
        public int Count => activeSkills.Count;

        /*
         * 유닛의 스킬 런타임 목록을 비운다.
         */
        public void Clear()
        {
            activeSkills.Clear();
        }

        /*
         * 같은 ID의 스킬을 교체하거나 새 스킬을 추가한다.
         */
        public void AddOrReplace(SkillRuntimeInstance instance)
        {
            if (instance == null || string.IsNullOrWhiteSpace(instance.SkillId))
            {
                return;
            }

            var existingIndex = FindIndexBySkillId(instance.SkillId);
            if (existingIndex >= 0)
            {
                activeSkills[existingIndex] = instance;
                return;
            }

            activeSkills.Add(instance);
        }

        /*
         * 스킬 ID가 일치하는 런타임을 찾는다.
         */
        public SkillRuntimeInstance FindBySkillId(string skillId)
        {
            var index = FindIndexBySkillId(skillId);
            return index >= 0 ? activeSkills[index] : null;
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
        private int FindIndexBySkillId(string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return -1;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                var runtime = activeSkills[i];
                if (runtime != null && string.Equals(runtime.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
