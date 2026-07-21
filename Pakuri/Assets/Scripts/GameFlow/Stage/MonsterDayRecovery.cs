using UnityEngine;

/*
 * 하루가 끝난 뒤 아군 Monster의 체력과 자동 행동 설정을 다음 전투 상태로 회복한다.
 */
namespace Pakuri.InGame
{
    internal static class MonsterDayRecovery
    {
        public static void Restore(MonsterCombatState model)
        {
            model.AutoAttackEnabled = true;
            if (!IsSelectedPlayerMonster(model))
            {
                model.AutoSkillEnabled = true;
            }

            UnitCombatReset.ResetTransient(model);
            model.Resources.CurrentHealth = Mathf.Max(0f, model.Stats.MaxHealth);
        }

        private static bool IsSelectedPlayerMonster(MonsterCombatState model)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }
    }
}
