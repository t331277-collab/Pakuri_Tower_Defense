using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Definitions.Skills;

/* 유닛이 보유한 액티브·패시브 스킬과 쿨다운의 공통 저장소를 제공한다. */
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

        /* 내부 스킬·쿨다운 컬렉션의 읽기 전용 view를 구성한다. */
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

        /* skill id의 쿨다운을 반환하고 미등록 id면 예외를 발생시킨다. */
        public SkillCooldown GetCooldown(string skillId)
        {
            cooldowns.TryGetValue(skillId, out SkillCooldown cooldown);
            return cooldown;
        }

        /* 등록된 모든 액티브 스킬 쿨다운을 경과 시간만큼 진행한다. */
        public void TickCooldowns(float deltaTime)
        {
            foreach (SkillCooldown cooldown in cooldowns.Values)
            {
                cooldown.Tick(deltaTime);
            }
        }

        /* 등록된 모든 쿨다운을 다음 round 초기 상태로 되돌린다. */
        public void ResetRuntimeState()
        {
            foreach (SkillCooldown cooldown in cooldowns.Values)
            {
                cooldown.ResetForNextRound();
            }
        }

        /* 액티브 스킬과 전용 쿨다운을 중복 없이 등록한다. */
        protected void RegisterActive(SkillDefinition definition)
        {

            activeSkills.Add(definition);
            if (!cooldowns.ContainsKey(definition.skill_id))
            {
                cooldowns.Add(definition.skill_id, new SkillCooldown(definition));
            }
        }

        /* 패시브 스킬을 중복 없이 등록한다. */
        protected void RegisterPassive(PassiveDefinition definition)
        {

            passiveSkills.Add(definition);
        }
    }
}
