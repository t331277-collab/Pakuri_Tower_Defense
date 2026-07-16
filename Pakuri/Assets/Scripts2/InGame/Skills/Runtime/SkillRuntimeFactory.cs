using System;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    public static class SkillRuntimeFactory
    {
        private static readonly InGameSkillSlot[] ActiveSlots =
        {
            InGameSkillSlot.A,
            InGameSkillSlot.B,
            InGameSkillSlot.C,
            InGameSkillSlot.D,
            InGameSkillSlot.E
        };

        public static UnitSkillRuntimeSet CreateLearnedActiveSet(
            BaseUnitRuntimeModel owner,
            InGameSkillCatalog catalog)
        {
            var set = new UnitSkillRuntimeSet();
            PopulateLearnedActiveSet(owner, catalog, set);
            return set;
        }

        public static void RebuildLearnedActiveSet(
            BaseUnitRuntimeModel owner,
            InGameSkillCatalog catalog)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillRuntime.Clear();
            PopulateLearnedActiveSet(owner, catalog, owner.SkillRuntime);
        }

        private static void PopulateLearnedActiveSet(
            BaseUnitRuntimeModel owner,
            InGameSkillCatalog catalog,
            UnitSkillRuntimeSet target)
        {
            if (owner == null || catalog == null || target == null)
            {
                return;
            }

            var monsterId = owner.Identity != null ? owner.Identity.DefinitionId : null;
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            for (var i = 0; i < ActiveSlots.Length; i++)
            {
                if (!catalog.TryGetActiveSkill(monsterId, ActiveSlots[i], out var skillData))
                {
                    continue;
                }

                if (!IsLearnedActive(owner.State, skillData))
                {
                    continue;
                }

                target.AddOrReplace(new SkillRuntimeInstance(owner, skillData));
            }
        }

        private static bool IsLearnedActive(UnitStateBucket state, SkillData skillData)
        {
            if (state == null || skillData == null || string.IsNullOrWhiteSpace(skillData.SkillId))
            {
                return false;
            }

            return ContainsId(state.LearnedActiveSkillIds, skillData.SkillId);
        }

        private static bool ContainsId(IEnumerable<string> ids, string targetId)
        {
            if (ids == null || string.IsNullOrWhiteSpace(targetId))
            {
                return false;
            }

            foreach (var id in ids)
            {
                if (string.Equals(id, targetId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
