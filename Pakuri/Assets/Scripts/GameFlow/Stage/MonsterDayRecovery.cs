using UnityEngine;

/*
 * 하루가 끝나면 몬스터의 임시 상태를 지우고 체력과 자동 행동 설정을 다음 전투용으로 복구한다.
 */
namespace Pakuri.InGame
{
    static class MonsterDayRecovery
    {
        /*
         * 한 전투에서만 유지되는 상태 효과, 보호막, 스킬 실행 상태를 초기화한다.
         */
        public static void ResetTransient(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            model.Statuses.Clear();
            model.Resources.DirectShield = 0f;
            model.Resources.CurrentShield = 0f;

            var activeSkills = model.SkillState.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i].ResetRuntimeState();
            }
        }

        /*
         * 다음 전투를 위해 자동 행동, 임시 상태, 체력을 복구한다.
         */
        public static void Restore(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            model.AutoAttackEnabled = true;
            if (!IsSelectedPlayerMonster(model))
            {
                model.AutoSkillEnabled = true;
            }

            ResetTransient(model);
            model.Resources.CurrentHealth = Mathf.Max(0f, model.Stats.MaxHealth);
        }

        /*
         * 플레이어가 직접 조작하는 선두 몬스터인지 확인한다.
         */
        private static bool IsSelectedPlayerMonster(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }
    }
}
