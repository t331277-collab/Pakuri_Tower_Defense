using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// InGame 상단 정보와 PrisonPanel 정보를 갱신한다.
    public sealed class InGameInfoUI : MonoBehaviour
    {
        private const string DisplayedSynergyName = "spirit-contract";

        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;
        private TMP_Text prisonStageInfoText;
        private TMP_Text prisonGoldInfoText;
        private TMP_Text prisonDarkInfoText;
        private Transform artifactContainer;
        private Image artifactIcon;
        private TMP_Text artifactCountText;
        private TMP_Text artifactLv2Text;
        private TMP_Text artifactLv4Text;
        private TMP_Text artifactLv6Text;
        private TMP_Text artifactLv8Text;
        private Color artifactLv2InactiveColor;
        private Color artifactLv4InactiveColor;
        private Color artifactLv6InactiveColor;
        private Color artifactLv8InactiveColor;

        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            artifactLv2InactiveColor = artifactLv2Text != null ? artifactLv2Text.color : Color.gray;
            artifactLv4InactiveColor = artifactLv4Text != null ? artifactLv4Text.color : Color.gray;
            artifactLv6InactiveColor = artifactLv6Text != null ? artifactLv6Text.color : Color.gray;
            artifactLv8InactiveColor = artifactLv8Text != null ? artifactLv8Text.color : Color.gray;
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

            RefreshArtifactDisplay(session);

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
            artifactContainer = UiBindingUtility.BindScene<Transform>(
                this,
                "HUD/Artifact_Container",
                nameof(artifactContainer),
                ref valid);
            artifactIcon = UiBindingUtility.BindChild<Image>(
                this,
                artifactContainer,
                "Image/Icon",
                nameof(artifactIcon),
                ref valid);
            artifactCountText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                artifactContainer,
                "Image/Cur/Text (TMP) (1)",
                nameof(artifactCountText),
                ref valid);
            artifactLv2Text = UiBindingUtility.BindChild<TMP_Text>(
                this,
                artifactContainer,
                "Image/Lv2/Text (TMP) (1)",
                nameof(artifactLv2Text),
                ref valid);
            artifactLv4Text = UiBindingUtility.BindChild<TMP_Text>(
                this,
                artifactContainer,
                "Image/Lv4/Text (TMP) (1)",
                nameof(artifactLv4Text),
                ref valid);
            artifactLv6Text = UiBindingUtility.BindChild<TMP_Text>(
                this,
                artifactContainer,
                "Image/Lv6/Text (TMP) (1)",
                nameof(artifactLv6Text),
                ref valid);
            artifactLv8Text = UiBindingUtility.BindChild<TMP_Text>(
                this,
                artifactContainer,
                "Image/Lv8/Text (TMP) (1)",
                nameof(artifactLv8Text),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

        private void RefreshArtifactDisplay(RunSession session)
        {
            var count = 0;
            var catalog = GameDataLoader.CurrentCatalog;
            var synergy = catalog != null
                ? catalog.GetData<ArtifactSynergyDefinition>(DisplayedSynergyName)
                : null;

            if (catalog != null && session != null)
            {
                for (var memberIndex = 0; memberIndex < session.PartyMembers.Count; memberIndex++)
                {
                    var member = session.PartyMembers[memberIndex];
                    if (member == null || member.Artifacts == null)
                    {
                        continue;
                    }

                    for (var artifactIndex = 0; artifactIndex < member.Artifacts.OwnedArtifactNames.Count; artifactIndex++)
                    {
                        var artifact = catalog.GetData<ArtifactDefinition>(
                            member.Artifacts.OwnedArtifactNames[artifactIndex]);
                        if (artifact != null
                            && string.Equals(
                                artifact.SynergyName,
                                DisplayedSynergyName,
                                StringComparison.OrdinalIgnoreCase))
                        {
                            count++;
                        }
                    }
                }
            }

            if (artifactContainer != null)
            {
                artifactContainer.gameObject.SetActive(count > 0);
            }

            if (artifactCountText != null)
            {
                artifactCountText.text = count.ToString();
            }

            if (artifactIcon != null)
            {
                artifactIcon.sprite = synergy != null ? synergy.Icon : null;
                artifactIcon.enabled = artifactIcon.sprite != null;
                artifactIcon.gameObject.SetActive(artifactIcon.sprite != null);
            }

            SetArtifactLevelColor(artifactLv2Text, artifactLv2InactiveColor, count >= 2);
            SetArtifactLevelColor(artifactLv4Text, artifactLv4InactiveColor, count >= 4);
            SetArtifactLevelColor(artifactLv6Text, artifactLv6InactiveColor, count >= 6);
            SetArtifactLevelColor(artifactLv8Text, artifactLv8InactiveColor, count >= 8);
        }

        private static void SetArtifactLevelColor(TMP_Text text, Color inactiveColor, bool active)
        {
            if (text != null)
            {
                text.color = active ? Color.white : inactiveColor;
            }
        }
    }
}
