using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 아군 몬스터의 일시적인 전투 상태와 자원을 다음 전투용으로 초기화하는 서비스.
     */
    internal static class MonsterUnitRuntimeStateService
    {
        public static void RestoreForNextDay(MonsterUnitRuntimeModel model)
        {
            if (model == null)
            {
                return;
            }

            model.AutoAttackEnabled = true;
            if (!IsSelectedPlayerModel(model))
            {
                model.AutoSkillEnabled = true;
            }

            ResetTransientCombatState(model);
            ResetResources(model);
        }

        public static void ResetTransientCombatState(MonsterUnitRuntimeModel model)
        {
            if (model == null)
            {
                return;
            }

            model.Statuses?.Clear();
            ResetShields(model);
            ResetActiveSkillRuntime(model);
        }

        private static void ResetResources(MonsterUnitRuntimeModel model)
        {
            var resources = model != null ? model.Resources : null;
            var stats = model != null ? model.Stats : null;
            if (resources == null || stats == null)
            {
                return;
            }

            resources.DirectShield = 0f;
            resources.CurrentShield = 0f;
            resources.CurrentHealth = Mathf.Max(0f, stats.MaxHealth);
        }

        private static void ResetShields(MonsterUnitRuntimeModel model)
        {
            var resources = model != null ? model.Resources : null;
            if (resources == null)
            {
                return;
            }

            resources.DirectShield = 0f;
            resources.CurrentShield = 0f;
        }

        private static void ResetActiveSkillRuntime(MonsterUnitRuntimeModel model)
        {
            var activeSkills = model != null && model.SkillRuntime != null
                ? model.SkillRuntime.ActiveSkills
                : null;
            if (activeSkills == null)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                activeSkills[i]?.ResetRuntimeState();
            }
        }

        private static bool IsSelectedPlayerModel(MonsterUnitRuntimeModel model)
        {
            var identity = model != null ? model.Identity : null;
            return identity != null
                && identity.Side == UnitSide.Player
                && identity.Role == UnitRole.Monster
                && identity.SlotIndex == 0;
        }
    }
}
