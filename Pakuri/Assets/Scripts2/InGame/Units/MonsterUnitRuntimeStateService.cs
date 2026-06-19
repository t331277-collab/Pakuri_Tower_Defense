using UnityEngine;

namespace Pakuri.InGame
{
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

            model.Statuses?.Clear();
            ResetResources(model);
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
