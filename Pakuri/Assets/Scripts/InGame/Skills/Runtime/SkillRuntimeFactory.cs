using System;
using System.Collections.Generic;
using Pakuri.Data;

namespace Pakuri.InGame
{
    /*
     * 유닛 정의와 학습 상태로 스킬 런타임 목록을 구성한다.
     */
    public static class SkillRuntimeFactory
    {
        private static readonly SkillSlot[] ActiveSlots =
        {
            SkillSlot.A,
            SkillSlot.B,
            SkillSlot.C,
            SkillSlot.D,
            SkillSlot.E
        };

        /*
         * 학습한 활성 목록을 다시 구성한다.
         */
        public static void RebuildLearnedActiveSet(BaseUnitRuntimeModel owner)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillRuntime.Clear();
            PopulateLearnedActiveSet(owner, owner.SkillRuntime);
        }

        /*
         * 지정된 활성 목록을 다시 구성한다.
         */
        public static void RebuildAssignedActiveSet(
            BaseUnitRuntimeModel owner,
            SkillDefinition[] definitions,
            SkillTriggerDefinition[] triggers)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillRuntime.Clear();
            if (definitions == null)
            {
                return;
            }

            var ownerId = owner.Identity != null ? owner.Identity.DefinitionId : string.Empty;
            for (var i = 0; i < definitions.Length; i++)
            {
                var data = SkillRuntimeCompiler.CompileActive(ownerId, definitions[i], triggers);
                if (data != null)
                {
                    owner.SkillRuntime.AddOrReplace(new SkillRuntimeInstance(owner, data));
                }
            }
        }

        /*
         * 학습한 활성 목록을 필요한 항목으로 채운다.
         */
        private static void PopulateLearnedActiveSet(
            BaseUnitRuntimeModel owner,
            UnitSkillRuntimeSet target)
        {
            if (owner == null || target == null)
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
                if (!TryCreateActiveSkillData(monsterId, ActiveSlots[i], out var skillData))
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

        /*
         * 카탈로그 정의를 조회해 실행용 활성 스킬 데이터를 만든다.
         */
        private static bool TryCreateActiveSkillData(
            string monsterId,
            SkillSlot slot,
            out SkillRuntimeData skillData)
        {
            skillData = null;
            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            var source = monster != null
                ? CsvDataLoader.CurrentCatalog.ResolveActiveSkill(monster.MonsterId, slot, monster)
                : ResolveActiveSkillWithoutMonster(monsterId, slot);
            if (source == null)
            {
                return false;
            }

            skillData = monster != null
                ? SkillRuntimeCompiler.CompileActive(monster, source)
                : SkillRuntimeCompiler.CompileActive(monsterId, source);
            return skillData != null;
        }

        /*
         * 몬스터 정의가 없을 때 등록된 스킬 목록에서 슬롯을 찾는다.
         */
        private static SkillDefinition ResolveActiveSkillWithoutMonster(string monsterId, SkillSlot slot)
        {
            var skills = CsvDataLoader.CurrentCatalog.GetActiveSkills(monsterId);
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

        /*
         * 활성 스킬이 학습 목록에 포함되는지 확인한다.
         */
        private static bool IsLearnedActive(UnitStateBucket state, SkillRuntimeData skillData)
        {
            if (state == null || skillData == null || string.IsNullOrWhiteSpace(skillData.SkillId))
            {
                return false;
            }

            return ContainsId(state.LearnedActiveSkillIds, skillData.SkillId);
        }

        /*
         * ID를 포함하는지 확인한다.
         */
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
