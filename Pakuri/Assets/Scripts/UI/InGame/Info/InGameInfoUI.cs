using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{
    /// InGame 상단 정보와 PrisonPanel 정보를 갱신한다.
    public sealed class InGameInfoUI : MonoBehaviour
    {
        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;
        private TMP_Text prisonStageInfoText;
        private TMP_Text prisonGoldInfoText;
        private TMP_Text prisonDarkInfoText;

        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
            }
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

        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            var valid = true;
            stageInfoText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "StageInfo",
                nameof(stageInfoText),
                ref valid);
            goldInfoText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "Goldinfo",
                nameof(goldInfoText),
                ref valid);
            darkInfoText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "Darkinfo",
                nameof(darkInfoText),
                ref valid);
            prisonStageInfoText = UiBindingUtility.BindScene<TMP_Text>(
                this,
                "Reward/PrisonPanel/StageSum",
                nameof(prisonStageInfoText),
                ref valid);
            prisonGoldInfoText = UiBindingUtility.BindScene<TMP_Text>(
                this,
                "Reward/PrisonPanel/Goldinfo",
                nameof(prisonGoldInfoText),
                ref valid);
            prisonDarkInfoText = UiBindingUtility.BindScene<TMP_Text>(
                this,
                "Reward/PrisonPanel/Darkinfo",
                nameof(prisonDarkInfoText),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }
    }
}
