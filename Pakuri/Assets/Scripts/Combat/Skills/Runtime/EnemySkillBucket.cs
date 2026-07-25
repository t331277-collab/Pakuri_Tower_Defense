using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

/* 적의 고정 스킬 슬롯과 공유 쿨다운 런타임을 구성한다. */
namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public class EnemySkillBucket : SkillBucket
    {
        public const int ActiveSkillSlotLimit = 2;
        public const int PassiveSkillSlotLimit = 1;

        /* 적 정의와 두 액티브 슬롯·한 패시브를 저장하고 등록한다. */
        public EnemySkillBucket(
            EnemyDefinition ownerDefinition,
            SkillDefinition slotASkill,
            SkillDefinition slotBSkill,
            PassiveDefinition passiveSkill)
        {
            OwnerDefinition =
                ownerDefinition;
            SlotASkill = slotASkill;
            SlotBSkill = slotBSkill;
            PassiveSkill = passiveSkill;

            RegisterActive(SlotASkill);
            RegisterActive(SlotBSkill);
            RegisterPassive(PassiveSkill);
        }

        public EnemyDefinition OwnerDefinition { get; }

        public SkillDefinition SlotASkill { get; }

        public SkillDefinition SlotBSkill { get; }

        public PassiveDefinition PassiveSkill { get; }

    }
}
