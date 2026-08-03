using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{
    /// InGame 상단 정보와 PrisonPanel 정보를 갱신한다.
    public sealed class InGameInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text stageInfoText;
        [SerializeField] private TMP_Text goldInfoText;
        [SerializeField] private TMP_Text darkInfoText;
        [SerializeField] private TMP_Text prisonStageInfoText;
        [SerializeField] private TMP_Text prisonGoldInfoText;
        [SerializeField] private TMP_Text prisonDarkInfoText;

        public void Refresh(StageManager stageManager, RunSession session, bool prisonPanelVisible)
        {
            var stage = stageManager != null ? stageManager.CurrentStage : (session != null ? session.StageIndex : 1);
            var day = stageManager != null ? stageManager.CurrentDay : (session != null ? session.DayIndex : 1);
            if (stageInfoText != null)
            {
                stageInfoText.text = $"Stage {stage}-{day}";
            }

            if (goldInfoText != null)
            {
                goldInfoText.gameObject.SetActive(true);
                goldInfoText.text = $"Gold {Math.Max(0, session != null ? session.Gold : 0)}";
            }

            if (darkInfoText != null)
            {
                darkInfoText.gameObject.SetActive(true);
                darkInfoText.text = $"Dark {Math.Max(0, session != null ? session.DarkTrace : 0)}";
            }

            if (!prisonPanelVisible)
            {
                return;
            }

            if (prisonStageInfoText != null)
            {
                prisonStageInfoText.text = $"Stage {stage}-{day}";
            }

            if (prisonGoldInfoText != null)
            {
                prisonGoldInfoText.text = $"Gold {Math.Max(0, session != null ? session.Gold : 0)}";
            }

            if (prisonDarkInfoText != null)
            {
                prisonDarkInfoText.text = $"Dark {Math.Max(0, session != null ? session.DarkTrace : 0)}";
            }
        }
    }
}
