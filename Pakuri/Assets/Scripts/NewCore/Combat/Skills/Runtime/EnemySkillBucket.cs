using System;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

/* 적의 고정 스킬 슬롯과 공유 쿨다운 런타임을 구성한다. */
namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public sealed class EnemySkillBucket : SkillBucket
    {
        public const int ActiveSkillSlotLimit = 2;
        public const int PassiveSkillSlotLimit = 1;

        /* 적 정의와 일치하는 두 액티브 슬롯·한 패시브를 검증해 등록한다. */
        public EnemySkillBucket(
            EnemyDefinition ownerDefinition,
            SkillDefinition slotASkill,
            SkillDefinition slotBSkill,
            PassiveDefinition passiveSkill)
        {
            OwnerDefinition =
                ownerDefinition ?? throw new ArgumentNullException(nameof(ownerDefinition));
            SlotASkill = ValidateAssignedSkill(
                slotASkill,
                ownerDefinition.skill_slot_a_id,
                nameof(slotASkill));
            SlotBSkill = ValidateAssignedSkill(
                slotBSkill,
                ownerDefinition.skill_slot_b_id,
                nameof(slotBSkill));
            PassiveSkill = passiveSkill ?? throw new ArgumentNullException(nameof(passiveSkill));
            if (!string.Equals(
                passiveSkill.skill_id,
                ownerDefinition.passive_id,
                StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Passive skill does not match the enemy definition.",
                    nameof(passiveSkill));
            }

            RegisterActive(SlotASkill);
            RegisterActive(SlotBSkill);
            RegisterPassive(PassiveSkill);
        }

        public EnemyDefinition OwnerDefinition { get; }

        public SkillDefinition SlotASkill { get; }

        public SkillDefinition SlotBSkill { get; }

        public PassiveDefinition PassiveSkill { get; }

        /* 지정 스킬이 null이 아닌 액티브이며 할당 id와 일치하는지 검증한다. */
        private static SkillDefinition ValidateAssignedSkill(
            SkillDefinition skill,
            string assignedSkillId,
            string parameterName)
        {
            if (skill == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (skill is PassiveDefinition
                || !string.Equals(skill.skill_id, assignedSkillId, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Active skill does not match the enemy definition.",
                    parameterName);
            }

            return skill;
        }
    }
}
