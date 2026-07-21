/*
 * 한 전투에서만 유지되는 상태 효과, 보호막, 스킬 실행 상태를 초기화한다.
 */
namespace Pakuri.InGame
{
    internal static class UnitCombatReset
    {
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
    }
}
