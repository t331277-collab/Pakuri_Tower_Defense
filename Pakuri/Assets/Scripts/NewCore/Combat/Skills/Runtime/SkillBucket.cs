using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public abstract class SkillBucket
    {
        private readonly List<SkillDefinition> activeSkills = new List<SkillDefinition>();
        private readonly List<PassiveDefinition> passiveSkills = new List<PassiveDefinition>();
        private readonly Dictionary<string, SkillCooldown> cooldowns =
            new Dictionary<string, SkillCooldown>(StringComparer.Ordinal);
        private readonly IReadOnlyList<SkillDefinition> readOnlyActiveSkills;
        private readonly IReadOnlyList<PassiveDefinition> readOnlyPassiveSkills;
        private readonly IReadOnlyDictionary<string, SkillCooldown> readOnlyCooldowns;

        protected SkillBucket()
        {
            readOnlyActiveSkills = new ReadOnlyCollection<SkillDefinition>(activeSkills);
            readOnlyPassiveSkills = new ReadOnlyCollection<PassiveDefinition>(passiveSkills);
            readOnlyCooldowns =
                new ReadOnlyDictionary<string, SkillCooldown>(cooldowns);
        }

        public IReadOnlyList<SkillDefinition> ActiveSkills => readOnlyActiveSkills;

        public IReadOnlyList<PassiveDefinition> PassiveSkills => readOnlyPassiveSkills;

        public IReadOnlyDictionary<string, SkillCooldown> Cooldowns => readOnlyCooldowns;

        public SkillCooldown GetCooldown(string skillId)
        {
            if (skillId == null)
            {
                throw new ArgumentNullException(nameof(skillId));
            }

            if (!cooldowns.TryGetValue(skillId, out SkillCooldown cooldown))
            {
                throw new KeyNotFoundException($"Skill '{skillId}' is not in this bucket.");
            }

            return cooldown;
        }

        public void TickCooldowns(float deltaTime)
        {
            foreach (SkillCooldown cooldown in cooldowns.Values)
            {
                cooldown.Tick(deltaTime);
            }
        }

        public void ResetRuntimeState()
        {
            foreach (SkillCooldown cooldown in cooldowns.Values)
            {
                cooldown.ResetForNextRound();
            }
        }

        protected void RegisterActive(SkillDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            activeSkills.Add(definition);
            if (!cooldowns.ContainsKey(definition.skill_id))
            {
                cooldowns.Add(definition.skill_id, new SkillCooldown(definition));
            }
        }

        protected void RegisterPassive(PassiveDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            passiveSkills.Add(definition);
        }
    }
}
