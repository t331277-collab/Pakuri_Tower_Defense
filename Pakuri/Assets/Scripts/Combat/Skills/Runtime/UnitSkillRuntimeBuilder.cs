using System;
using System.Collections.Generic;
using Pakuri.Data;

/*
 * 유닛 정의와 학습 상태를 읽어 유닛이 사용할 스킬 런타임 목록을 다시 구성한다.
 * 목록 저장과 조회를 담당하는 UnitSkillRuntimeSet과 달리 데이터 선택과 인스턴스 생성을 맡는다.
 */
namespace Pakuri.InGame
{
    public static class UnitSkillRuntimeBuilder
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
         * 학습한 활성 스킬과 패시브 목록을 다시 구성한다.
         */
        public static void RebuildLearnedSkillSet(UnitCombatState owner)
        {
            if (owner == null)
            {
                return;
            }

            owner.SkillRuntime.Clear();
            PopulateLearnedSkillSet(owner, owner.SkillRuntime);
        }

        /*
         * 지정된 활성 목록을 다시 구성한다.
         */
        public static void RebuildAssignedActiveSet(
            UnitCombatState owner,
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
         * 학습한 활성 스킬과 패시브를 목록에 채운다.
         */
        private static void PopulateLearnedSkillSet(
            UnitCombatState owner,
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

            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            for (var i = 0; i < ActiveSlots.Length; i++)
            {
                var source = CsvDataLoader.CurrentCatalog.ResolveActiveSkill(monsterId, ActiveSlots[i]);
                if (source == null)
                {
                    continue;
                }

                var skillData = SkillRuntimeCompiler.CompileActive(monster, source);
                if (ContainsId(owner.SkillProgress.LearnedActiveSkillIds, skillData.SkillId))
                {
                    target.AddOrReplace(new SkillRuntimeInstance(owner, skillData));
                }
            }

            var passives = CsvDataLoader.CurrentCatalog.GetPassiveSkills(monsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = SkillRuntimeCompiler.CompilePassive(monster, passives[i]);
                if (ContainsId(owner.SkillProgress.LearnedPassiveSkillIds, passive.SkillId))
                {
                    target.AddOrReplace(new SkillRuntimeInstance(owner, passive));
                }
            }
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
