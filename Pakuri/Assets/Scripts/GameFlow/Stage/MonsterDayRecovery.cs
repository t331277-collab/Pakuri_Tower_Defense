/*
 * 역할: 일차 사이 몬스터 회복.
 * 책임: Stage 사이에 플레이어 유닛 전투 자원을 회복하고 임시 전투 상태를 정리한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// MonsterDayRecovery가 소유하는 데이터와 동작을 캡슐화한다.
    static class MonsterDayRecovery
    {

        /// 전달된 model 값을 사용해 Transient를 초기 런타임 상태로 되돌린다.
        public static void ResetTransient(UnitCombatState model)
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

        /// 전달된 model 값을 사용해 Restore 작업을 수행한다.
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

        /// 전달된 model 값을 사용해 SelectedPlayerMonster 조건 충족 여부를 반환한다.
        private static bool IsSelectedPlayerMonster(UnitCombatState model)
        {
            return model.Identity.Side == UnitSide.Player
                && model.Identity.Role == UnitRole.Monster
                && model.Identity.SlotIndex == 0;
        }
    }
}
