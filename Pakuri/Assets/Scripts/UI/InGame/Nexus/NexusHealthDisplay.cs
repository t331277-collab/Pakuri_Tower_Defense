using TMPro;
using UnityEngine;

/*
 * Nexus 전투 상태를 화면의 현재 체력과 최대 체력 문자열로 표시한다.
 */
namespace Pakuri.InGame
{
    internal static class NexusHealthDisplay
    {
        /*
         * Refresh 대상의 현재 상태를 갱신한다.
         */
        public static void Refresh(TextMeshProUGUI label, UnitCombatState model)
        {
            var current = Mathf.CeilToInt(Mathf.Max(0f, model.Resources.CurrentHealth));
            var maximum = Mathf.CeilToInt(Mathf.Max(0f, model.Stats.MaxHealth));
            label.text = $"{current} / {maximum}";
        }
    }
}
