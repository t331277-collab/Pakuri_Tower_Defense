using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// InGame 상단 정보와 PrisonPanel 정보를 갱신한다.
    public sealed class InGameInfoUI : MonoBehaviour
    {
        private const float ArtifactContainerVerticalOffset = 93.3f;

        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;
        private TMP_Text prisonStageInfoText;
        private TMP_Text prisonGoldInfoText;
        private TMP_Text prisonDarkInfoText;
        private Transform artifactContainerTemplate;
        private readonly List<ArtifactContainerView> artifactContainers = new List<ArtifactContainerView>();

        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
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
            artifactContainerTemplate = UiBindingUtility.BindScene<Transform>(
                this,
                "HUD/Artifact_Container",
                nameof(artifactContainerTemplate),
                ref valid);
            var templateView = BindArtifactContainer(artifactContainerTemplate, ref valid);
            if (templateView != null)
            {
                templateView.CaptureInactiveColors();
                artifactContainers.Add(templateView);
            }

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

        private void RefreshArtifactDisplay(RunSession session)
        {
            var entries = new List<ArtifactDisplayEntry>();
            var entryIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var catalog = GameDataLoader.CurrentCatalog;

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
                        if (artifact != null && !string.IsNullOrWhiteSpace(artifact.SynergyName))
                        {
                            if (entryIndexes.TryGetValue(artifact.SynergyName, out var entryIndex))
                            {
                                var entry = entries[entryIndex];
                                entries[entryIndex] = new ArtifactDisplayEntry(entry.Synergy, entry.Count + 1);
                            }
                            else
                            {
                                entryIndexes.Add(artifact.SynergyName, entries.Count);
                                entries.Add(new ArtifactDisplayEntry(
                                    catalog.GetData<ArtifactSynergyDefinition>(artifact.SynergyName),
                                    1));
                            }
                        }
                    }
                }
            }

            for (var i = 0; i < artifactContainers.Count; i++)
            {
                artifactContainers[i].Root.gameObject.SetActive(false);
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var view = GetArtifactContainer(i);
                view.Refresh(entries[i].Synergy, entries[i].Count);
            }
        }

        private ArtifactContainerView GetArtifactContainer(int index)
        {
            while (artifactContainers.Count <= index)
            {
                var clone = Instantiate(
                    artifactContainerTemplate.gameObject,
                    artifactContainerTemplate.parent);
                clone.name = $"{artifactContainerTemplate.name} ({artifactContainers.Count})";
                var previous = artifactContainers[artifactContainers.Count - 1].Root;
                clone.transform.localPosition = previous.localPosition
                    + Vector3.down * ArtifactContainerVerticalOffset;
                var valid = true;
                var view = BindArtifactContainer(clone.transform, ref valid);
                if (!valid || view == null)
                {
                    Destroy(clone);
                    return artifactContainers[0];
                }

                view.CopyInactiveColorsFrom(artifactContainers[0]);
                artifactContainers.Add(view);
            }

            return artifactContainers[index];
        }

        private ArtifactContainerView BindArtifactContainer(Transform root, ref bool valid)
        {
            if (root == null)
            {
                valid = false;
                return null;
            }

            return new ArtifactContainerView
            {
                Root = root,
                Icon = UiBindingUtility.BindChild<Image>(this, root, "Image/Icon", "artifactIcon", ref valid),
                CountText = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Cur/Text (TMP) (1)", "artifactCountText", ref valid),
                Lv2Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv2/Text (TMP) (1)", "artifactLv2Text", ref valid),
                Lv4Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv4/Text (TMP) (1)", "artifactLv4Text", ref valid),
                Lv6Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv6/Text (TMP) (1)", "artifactLv6Text", ref valid),
                Lv8Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv8/Text (TMP) (1)", "artifactLv8Text", ref valid)
            };
        }

        private sealed class ArtifactContainerView
        {
            public Transform Root;
            public Image Icon;
            public TMP_Text CountText;
            public TMP_Text Lv2Text;
            public TMP_Text Lv4Text;
            public TMP_Text Lv6Text;
            public TMP_Text Lv8Text;

            private Color lv2InactiveColor;
            private Color lv4InactiveColor;
            private Color lv6InactiveColor;
            private Color lv8InactiveColor;

            public void CaptureInactiveColors()
            {
                lv2InactiveColor = Lv2Text != null ? Lv2Text.color : Color.gray;
                lv4InactiveColor = Lv4Text != null ? Lv4Text.color : Color.gray;
                lv6InactiveColor = Lv6Text != null ? Lv6Text.color : Color.gray;
                lv8InactiveColor = Lv8Text != null ? Lv8Text.color : Color.gray;
            }

            public void CopyInactiveColorsFrom(ArtifactContainerView source)
            {
                if (source == null)
                {
                    CaptureInactiveColors();
                    return;
                }

                lv2InactiveColor = source.lv2InactiveColor;
                lv4InactiveColor = source.lv4InactiveColor;
                lv6InactiveColor = source.lv6InactiveColor;
                lv8InactiveColor = source.lv8InactiveColor;
            }

            public void Refresh(ArtifactSynergyDefinition synergy, int count)
            {
                Root.gameObject.SetActive(count > 0);
                CountText.text = count.ToString();
                Icon.sprite = synergy != null ? synergy.Icon : null;
                Icon.enabled = Icon.sprite != null;
                Icon.gameObject.SetActive(Icon.sprite != null);
                SetLevelColor(Lv2Text, lv2InactiveColor, count >= 2);
                SetLevelColor(Lv4Text, lv4InactiveColor, count >= 4);
                SetLevelColor(Lv6Text, lv6InactiveColor, count >= 6);
                SetLevelColor(Lv8Text, lv8InactiveColor, count >= 8);
            }

            private static void SetLevelColor(TMP_Text text, Color inactiveColor, bool active)
            {
                if (text != null)
                {
                    text.color = active ? Color.white : inactiveColor;
                }
            }
        }

        private readonly struct ArtifactDisplayEntry
        {
            public readonly ArtifactSynergyDefinition Synergy;
            public readonly int Count;

            public ArtifactDisplayEntry(ArtifactSynergyDefinition synergy, int count)
            {
                Synergy = synergy;
                Count = count;
            }
        }
    }
}
