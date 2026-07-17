using System;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    public sealed class UnitSkillRuntimeSet
    {
        private readonly List<SkillRuntimeInstance> activeSkills = new List<SkillRuntimeInstance>();

        public IReadOnlyList<SkillRuntimeInstance> ActiveSkills => activeSkills;
        public int Count => activeSkills.Count;

        public void Clear()
        {
            activeSkills.Clear();
        }

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

        public SkillRuntimeInstance FindBySkillId(string skillId)
        {
            var index = FindIndexBySkillId(skillId);
            return index >= 0 ? activeSkills[index] : null;
        }

        public SkillRuntimeInstance FindBySlot(InGameSkillSlot slot)
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
