/*
 * 역할: Nexus 체력 표시.
 * 책임: 등록된 Nexus 모델을 읽어 보이는 체력 값을 갱신한다.
 */

using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{

    /// NexusHealthDisplay 상태를 Unity UI 또는 월드 오브젝트로 표시한다.
    internal static class NexusHealthDisplay
    {

        /// 전달된 런타임 입력값을 사용해 현재 표시 상태를 현재 런타임 모델을 기준으로 갱신한다.
        public static void Refresh(TextMeshProUGUI label, UnitCombatState model)
        {
            var current = Mathf.CeilToInt(Mathf.Max(0f, model.Resources.CurrentHealth));
            var maximum = Mathf.CeilToInt(Mathf.Max(0f, model.Stats.MaxHealth));
            label.text = $"{current} / {maximum}";
        }
    }
}
