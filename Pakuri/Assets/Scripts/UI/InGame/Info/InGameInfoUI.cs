using System;
using Pakuri.Data;
using TMPro;

namespace Pakuri.InGame
{
    /// InGame 상단 정보와 PrisonPanel 정보를 갱신한다.
    internal sealed class InGameInfoUI
    {
        private readonly TMP_Text stageInfoText;
        private readonly TMP_Text goldInfoText;
        private readonly TMP_Text darkInfoText;
        private readonly TMP_Text prisonStageInfoText;
        private readonly TMP_Text prisonGoldInfoText;
        private readonly TMP_Text prisonDarkInfoText;

        public InGameInfoUI(InGameInfoReferences references)
        {
            stageInfoText = references != null ? references.stageInfoText : null;
            goldInfoText = references != null ? references.goldInfoText : null;
            darkInfoText = references != null ? references.darkInfoText : null;
            prisonStageInfoText = references != null ? references.prisonStageInfoText : null;
            prisonGoldInfoText = references != null ? references.prisonGoldInfoText : null;
            prisonDarkInfoText = references != null ? references.prisonDarkInfoText : null;
        }

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
