using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class InGameSkillCatalog
    {
        public InGameSkillCatalog(GameDataCatalog fallbackCatalog)
        {
            SourceCatalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(fallbackCatalog);
        }

        public GameDataCatalog SourceCatalog { get; }

        public bool HasSourceCatalog => SourceCatalog != null;

        public bool TryGetActiveSkill(string monsterId, InGameSkillSlot slot, out SkillData skillData)
        {
            skillData = null;
            var monster = ResolveMonster(monsterId);
            var sourceSlot = InGameSkillDefinitionMapper.MapSlot(slot);
            var source = monster != null
                ? PakuriDataManager.Instance.ResolveActiveSkill(monster.MonsterId, sourceSlot, monster)
                : ResolveActiveSkillWithoutMonster(monsterId, sourceSlot);

            if (source == null)
            {
                return false;
            }

            skillData = monster != null
                ? InGameSkillDefinitionMapper.CreateActiveSkillData(monster, source)
                : InGameSkillDefinitionMapper.CreateActiveSkillData(monsterId, source);
            return skillData != null;
        }

        public bool TryGetPassiveSkill(string monsterId, InGameSkillSlot slot, out PassiveSkillData skillData)
        {
            skillData = null;
            var monster = ResolveMonster(monsterId);
            if (monster == null)
            {
                return false;
            }

            var source = PakuriDataManager.Instance.ResolvePassiveSkill(
                monster.MonsterId,
                InGameSkillDefinitionMapper.MapSlot(slot),
                monster);

            if (source == null)
            {
                return false;
            }

            skillData = InGameSkillDefinitionMapper.CreatePassiveSkillData(monster, source);
            return skillData != null;
        }

        private MonsterDefinition ResolveMonster(string monsterId)
        {
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return null;
            }

            return PakuriDataManager.Instance.ResolveMonster(monsterId, SourceCatalog);
        }

        private static SkillDefinition ResolveActiveSkillWithoutMonster(string monsterId, SkillSlot slot)
        {
            var skills = PakuriDataManager.Instance.GetActiveSkills(monsterId);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill != null && skill.Slot == slot)
                {
                    return skill;
                }
            }

            return null;
        }
    }
}
