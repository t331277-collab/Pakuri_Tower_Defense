/*
 * 역할: Nexus 체력 표시.
 * 책임: 등록된 Nexus 모델을 읽어 보이는 체력 값을 갱신한다.
 */

using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{

    /// Nexus의 현재 체력과 보호막을 전투 HUD에 표시한다.
    internal static class NexusHealthDisplay
    {

        public static void Refresh(TextMeshProUGUI label, UnitCombatState model)
        {
            var current = Mathf.CeilToInt(Mathf.Max(0f, model.Resources.CurrentHealth));
            var maximum = Mathf.CeilToInt(Mathf.Max(0f, model.Stats.MaxHealth));
            label.text = $"{current} / {maximum}";
        }
    }
}
