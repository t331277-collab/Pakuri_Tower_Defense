using UnityEngine;

/*
 * 하루가 끝난 뒤 Monster의 임시 전투 상태, 체력, 자동 행동 설정을 다음 전투 상태로 회복한다.
 */
namespace Pakuri.InGame
{
    static class MonsterDayRecovery
    {
        /*
         * 한 전투에서만 유지되는 상태 효과, 보호막, 스킬 실행 상태를 초기화한다.
         */
        public static void ResetTransient(UnitCombatState model)
        {
            model.Statuses.Clear();
            model.Resources.DirectShield = 0f;
            model.Resources.CurrentShield = 0f;

            var activeSkills = model.SkillRuntime.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i].ResetRuntimeState();
            }
        }

        public static void Restore(UnitCombatState model)
        {
            model.AutoAttackEnabled = true;
            if (!IsSelectedPlayerMonster(model))
            {
                model.AutoSkillEnabled = true;
            }

            ResetTransient(model);
            model.Resources.CurrentHealth = Mathf.Max(0f, model.Stats.MaxHealth);
        }

        private static bool IsSelectedPlayerMonster(UnitCombatState model)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }
    }
}
