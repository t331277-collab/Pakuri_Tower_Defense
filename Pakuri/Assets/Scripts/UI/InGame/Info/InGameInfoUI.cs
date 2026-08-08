using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// InGame 상단 정보와 PrisonPanel 정보를 갱신한다.
    public sealed class InGameInfoUI : MonoBehaviour
    {
        private const float ArtifactContainerVerticalOffset = 93.3f;
        private const int ArtifactPopupSlotCount = 10;
        private static readonly int[] ArtifactPopupLevelCounts = { 2, 4, 6, 8 };
        private static readonly Color32 UnownedArtifactColor = new Color32(174, 170, 170, 255);

        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;
        private TMP_Text prisonStageInfoText;
        private TMP_Text prisonGoldInfoText;
        private TMP_Text prisonDarkInfoText;
        private Transform artifactContainerTemplate;
        private readonly List<ArtifactContainerView> artifactContainers = new List<ArtifactContainerView>();
        private readonly List<RectTransform> artifactPopupStack = new List<RectTransform>();
        private ArtifactContainerView activeArtifactPopupView;

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

        private void OnDisable()
        {
            ClearArtifactPopupStack();
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
                "Reward/Artifact_Container",
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
                                entries[entryIndex].AddOwnedArtifact(artifact.ArtifactName);
                            }
                            else
                            {
                                entryIndexes.Add(artifact.SynergyName, entries.Count);
                                var entry = new ArtifactDisplayEntry(
                                    catalog.GetData<ArtifactSynergyDefinition>(artifact.SynergyName));
                                entry.AddOwnedArtifact(artifact.ArtifactName);
                                entries.Add(entry);
                            }
                        }
                    }
                }
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var view = GetArtifactContainer(i);
                view.Refresh(entries[i].Synergy, entries[i].Count);
                ConfigureArtifactPopup(
                    view,
                    entries[i].Synergy,
                    GetArtifactsForSynergy(catalog, entries[i].Synergy),
                    entries[i].OwnedArtifactNames,
                    entries[i].Count);
            }

            for (var i = entries.Count; i < artifactContainers.Count; i++)
            {
                if (activeArtifactPopupView == artifactContainers[i])
                {
                    ClearArtifactPopupStack();
                }
                artifactContainers[i].Root.gameObject.SetActive(false);
            }
        }

        private static ArtifactDefinition[] GetArtifactsForSynergy(
            GameDataCatalog catalog,
            ArtifactSynergyDefinition synergy)
        {
            if (catalog == null || synergy == null || catalog.Artifacts == null)
            {
                return Array.Empty<ArtifactDefinition>();
            }

            var artifacts = new List<ArtifactDefinition>();
            for (var i = 0; i < catalog.Artifacts.Length; i++)
            {
                var artifact = catalog.Artifacts[i];
                if (artifact != null
                    && string.Equals(
                        artifact.SynergyName,
                        synergy.SynergyName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    artifacts.Add(artifact);
                }
            }

            return artifacts.ToArray();
        }

        private ArtifactContainerView GetArtifactContainer(int index)
        {
            while (artifactContainers.Count <= index)
            {
                var clone = Instantiate(
                    artifactContainerTemplate.gameObject,
                    artifactContainerTemplate.parent);
                clone.transform.SetSiblingIndex(
                    Mathf.Max(0, artifactContainerTemplate.GetSiblingIndex() - (artifactContainers.Count - 1)));
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

            var view = new ArtifactContainerView
            {
                Root = root,
                Icon = UiBindingUtility.BindChild<Image>(this, root, "Image/Icon", "artifactIcon", ref valid),
                CountText = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Cur/Text (TMP) (1)", "artifactCountText", ref valid),
                Lv2Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv2/Text (TMP) (1)", "artifactLv2Text", ref valid),
                Lv4Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv4/Text (TMP) (1)", "artifactLv4Text", ref valid),
                Lv6Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv6/Text (TMP) (1)", "artifactLv6Text", ref valid),
                Lv8Text = UiBindingUtility.BindChild<TMP_Text>(this, root, "Image/Lv8/Text (TMP) (1)", "artifactLv8Text", ref valid)
            };

            BindArtifactPopup(view, ref valid);
            return view;
        }

        private void BindArtifactPopup(ArtifactContainerView view, ref bool valid)
        {
            var root = view.Root;
            view.PopupOpenButton = UiBindingUtility.BindChild<Button>(
                this,
                root,
                "Artifact",
                "artifactPopupOpenButton",
                ref valid);
            view.PopupRoot = UiBindingUtility.BindChild<RectTransform>(
                this,
                root,
                "SynergyInfoPopup",
                "artifactPopupRoot",
                ref valid);
            view.SynergyNameText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                root,
                "SynergyInfoPopup/SynergeName",
                "artifactPopupSynergyName",
                ref valid);
            view.SynergyIcon = UiBindingUtility.BindChild<Image>(
                this,
                root,
                "SynergyInfoPopup/SynergyIcon",
                "artifactPopupSynergyIcon",
                ref valid);
            view.SynergyInfoRoot = UiBindingUtility.BindChild<RectTransform>(
                this,
                root,
                "SynergyInfoPopup/SynergyInfo",
                "artifactPopupSynergyInfoRoot",
                ref valid);
            view.SynergyInfoTitle = UiBindingUtility.BindChild<TMP_Text>(
                this,
                root,
                "SynergyInfoPopup/SynergyInfo/Info",
                "artifactPopupSynergyInfoTitle",
                ref valid);
            view.SynergyInfoText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                root,
                "SynergyInfoPopup/SynergyInfo/Text (TMP)",
                "artifactPopupSynergyInfoText",
                ref valid);
            view.ArtifactInfoRoot = UiBindingUtility.BindChild<RectTransform>(
                this,
                root,
                "SynergyInfoPopup/ArtifactInfo",
                "artifactPopupArtifactInfoRoot",
                ref valid);
            view.ArtifactInfoTitle = UiBindingUtility.BindChild<TMP_Text>(
                this,
                root,
                "SynergyInfoPopup/ArtifactInfo/Info",
                "artifactPopupArtifactInfoTitle",
                ref valid);
            view.ArtifactInfoText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                root,
                "SynergyInfoPopup/ArtifactInfo/Text (TMP)",
                "artifactPopupArtifactInfoText",
                ref valid);

            for (var i = 0; i < ArtifactPopupSlotCount; i++)
            {
                var path = i == 0
                    ? "SynergyInfoPopup/Artifact1"
                    : $"SynergyInfoPopup/Artifact1 ({i})";
                view.PopupArtifactImages[i] = UiBindingUtility.BindChild<Image>(
                    this,
                    root,
                    path,
                    $"artifactPopupImages[{i}]",
                    ref valid);
                view.PopupArtifactButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    root,
                    path,
                    $"artifactPopupButtons[{i}]",
                    ref valid);
            }

            for (var i = 0; i < ArtifactPopupLevelCounts.Length; i++)
            {
                var path = $"SynergyInfoPopup/{ArtifactPopupLevelCounts[i]}";
                view.PopupLevelImages[i] = UiBindingUtility.BindChild<Image>(
                    this,
                    root,
                    path,
                    $"artifactPopupLevelImages[{i}]",
                    ref valid);
                view.PopupLevelButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    root,
                    path,
                    $"artifactPopupLevelButtons[{i}]",
                    ref valid);
            }

            if (!valid)
            {
                return;
            }

            EnsureArtifactPopupInteractionSurfaces(view);
            BindArtifactPopupInteractions(view);
            ConfigureArtifactPopupRaycasts(view);
            view.HidePopup();
        }

        private static void EnsureArtifactPopupInteractionSurfaces(ArtifactContainerView view)
        {
            view.DismissBlocker = GetOrCreateInteractionImage(
                view.PopupRoot,
                "ArtifactPopupDismissBlocker");
            view.PopupHitArea = GetOrCreateInteractionImage(
                view.PopupRoot,
                "ArtifactPopupHitArea");

            view.DismissBlocker.SetAsFirstSibling();
            view.PopupHitArea.SetSiblingIndex(1);
            StretchToParent(view.PopupHitArea);
        }

        private static RectTransform GetOrCreateInteractionImage(Transform parent, string name)
        {
            var existing = parent.Find(name) as RectTransform;
            if (existing != null)
            {
                var existingImage = existing.GetComponent<Image>();
                existingImage.color = Color.clear;
                existingImage.raycastTarget = true;
                return existing;
            }

            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            gameObject.layer = parent.gameObject.layer;
            var rectTransform = gameObject.GetComponent<RectTransform>();
            rectTransform.SetParent(parent, false);
            var image = gameObject.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
            return rectTransform;
        }

        private static void StretchToParent(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        private void BindArtifactPopupInteractions(ArtifactContainerView view)
        {
            view.PopupOpenButton.onClick.RemoveAllListeners();
            view.PopupOpenButton.onClick.AddListener(() => OpenArtifactPopup(view));

            var dismissTrigger = view.DismissBlocker.GetComponent<EventTrigger>();
            if (dismissTrigger == null)
            {
                dismissTrigger = view.DismissBlocker.gameObject.AddComponent<EventTrigger>();
            }
            dismissTrigger.triggers = new List<EventTrigger.Entry>();
            AddEventTrigger(dismissTrigger, EventTriggerType.PointerClick, _ => PopArtifactPopupStack());

            for (var i = 0; i < view.PopupArtifactButtons.Length; i++)
            {
                var slotIndex = i;
                view.PopupArtifactButtons[i].onClick.RemoveAllListeners();
                view.PopupArtifactButtons[i].onClick.AddListener(
                    () => OpenArtifactInfo(view, slotIndex));
            }

            for (var i = 0; i < view.PopupLevelButtons.Length; i++)
            {
                var levelIndex = i;
                view.PopupLevelButtons[i].onClick.RemoveAllListeners();
                view.PopupLevelButtons[i].onClick.AddListener(
                    () => OpenSynergyInfo(view, levelIndex));
            }
        }

        private static void AddEventTrigger(
            EventTrigger trigger,
            EventTriggerType eventType,
            Action<BaseEventData> callback)
        {
            var entry = new EventTrigger.Entry
            {
                eventID = eventType
            };
            entry.callback.AddListener(eventData => callback(eventData));
            trigger.triggers.Add(entry);
        }

        private static void ConfigureArtifactPopupRaycasts(ArtifactContainerView view)
        {
            if (view.PopupOpenButton.targetGraphic != null)
            {
                view.PopupOpenButton.targetGraphic.raycastTarget = true;
            }
            view.SynergyIcon.raycastTarget = false;

            var popupBackground = view.PopupRoot.GetComponent<Image>();
            if (popupBackground != null)
            {
                popupBackground.raycastTarget = false;
            }

            var synergyInfoBackground = view.SynergyInfoRoot.GetComponent<Image>();
            if (synergyInfoBackground != null)
            {
                synergyInfoBackground.raycastTarget = true;
            }

            var artifactInfoBackground = view.ArtifactInfoRoot.GetComponent<Image>();
            if (artifactInfoBackground != null)
            {
                artifactInfoBackground.raycastTarget = true;
            }

            var texts = view.PopupRoot.GetComponentsInChildren<TMP_Text>(true);
            for (var i = 0; i < texts.Length; i++)
            {
                texts[i].raycastTarget = false;
            }

            for (var i = 0; i < view.PopupArtifactImages.Length; i++)
            {
                view.PopupArtifactImages[i].raycastTarget = true;
            }

            for (var i = 0; i < view.PopupLevelImages.Length; i++)
            {
                view.PopupLevelImages[i].raycastTarget = true;
            }
        }

        private void ConfigureArtifactPopup(
            ArtifactContainerView view,
            ArtifactSynergyDefinition synergy,
            ArtifactDefinition[] artifacts,
            IReadOnlyList<string> ownedArtifactNames,
            int ownedCount)
        {
            view.PopupConfigured = synergy != null;
            view.SynergyNameText.text = synergy != null ? synergy.DisplayName : string.Empty;
            view.SynergyIcon.sprite = synergy != null ? synergy.Icon : null;
            view.SynergyIcon.enabled = view.SynergyIcon.sprite != null;
            view.SynergyIcon.gameObject.SetActive(view.SynergyIcon.sprite != null);

            var ownedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; ownedArtifactNames != null && i < ownedArtifactNames.Count; i++)
            {
                ownedNames.Add(ownedArtifactNames[i]);
            }

            for (var i = 0; i < view.PopupArtifacts.Length; i++)
            {
                var artifact = artifacts != null && i < artifacts.Length ? artifacts[i] : null;
                view.PopupArtifacts[i] = artifact;
                var hasArtifact = artifact != null;
                var button = view.PopupArtifactButtons[i];
                var image = view.PopupArtifactImages[i];
                button.gameObject.SetActive(hasArtifact);
                button.interactable = hasArtifact;
                image.sprite = hasArtifact ? artifact.Icon : null;
                image.enabled = hasArtifact && artifact.Icon != null;
                image.color = hasArtifact && ownedNames.Contains(artifact.ArtifactName)
                    ? Color.white
                    : UnownedArtifactColor;
            }

            for (var i = 0; i < view.PopupLevels.Length; i++)
            {
                var requiredCount = ArtifactPopupLevelCounts[i];
                var level = FindSynergyLevel(synergy, requiredCount);
                view.PopupLevels[i] = level;
                view.PopupLevelButtons[i].gameObject.SetActive(level != null);
                view.PopupLevelButtons[i].interactable = level != null;
                view.PopupLevelImages[i].color = level != null && ownedCount >= requiredCount
                    ? Color.white
                    : view.PopupLevelInactiveColors[i];
            }

        }

        private static ArtifactSynergyLevelDefinition FindSynergyLevel(
            ArtifactSynergyDefinition synergy,
            int requiredCount)
        {
            for (var i = 0; synergy != null && synergy.Levels != null && i < synergy.Levels.Length; i++)
            {
                if (synergy.Levels[i] != null && synergy.Levels[i].RequiredCount == requiredCount)
                {
                    return synergy.Levels[i];
                }
            }

            return null;
        }

        private void OpenArtifactPopup(ArtifactContainerView view)
        {
            if (view == null || !view.PopupConfigured)
            {
                return;
            }

            if (activeArtifactPopupView == view && view.PopupRoot.gameObject.activeSelf)
            {
                return;
            }

            ClearArtifactPopupStack();
            activeArtifactPopupView = view;
            view.SynergyInfoRoot.gameObject.SetActive(false);
            view.ArtifactInfoRoot.gameObject.SetActive(false);
            view.PopupRoot.gameObject.SetActive(true);
            ResizeDismissBlocker(view);
            view.DismissBlocker.SetAsFirstSibling();
            view.PopupHitArea.SetSiblingIndex(1);
            artifactPopupStack.Add(view.PopupRoot);
        }

        private static void ResizeDismissBlocker(ArtifactContainerView view)
        {
            var canvas = view.PopupRoot.GetComponentInParent<Canvas>();
            var canvasRoot = canvas != null ? canvas.rootCanvas.transform as RectTransform : null;
            if (canvasRoot == null)
            {
                StretchToParent(view.DismissBlocker);
                return;
            }

            var corners = new Vector3[4];
            canvasRoot.GetWorldCorners(corners);
            var bottomLeft = view.PopupRoot.InverseTransformPoint(corners[0]);
            var topRight = view.PopupRoot.InverseTransformPoint(corners[2]);
            view.DismissBlocker.anchorMin = new Vector2(0.5f, 0.5f);
            view.DismissBlocker.anchorMax = new Vector2(0.5f, 0.5f);
            view.DismissBlocker.anchoredPosition = new Vector2(
                (bottomLeft.x + topRight.x) * 0.5f,
                (bottomLeft.y + topRight.y) * 0.5f);
            view.DismissBlocker.sizeDelta = new Vector2(
                Mathf.Abs(topRight.x - bottomLeft.x),
                Mathf.Abs(topRight.y - bottomLeft.y));
            view.DismissBlocker.localRotation = Quaternion.identity;
            view.DismissBlocker.localScale = Vector3.one;
        }

        private void OpenArtifactInfo(ArtifactContainerView view, int slotIndex)
        {
            if (view != activeArtifactPopupView
                || slotIndex < 0
                || slotIndex >= view.PopupArtifacts.Length
                || view.PopupArtifacts[slotIndex] == null)
            {
                return;
            }

            var artifact = view.PopupArtifacts[slotIndex];
            view.ArtifactInfoTitle.text = artifact.Description;
            view.ArtifactInfoText.text = artifact.DisplayName;
            PushArtifactPopupPanel(view.ArtifactInfoRoot);
        }

        private void OpenSynergyInfo(ArtifactContainerView view, int levelIndex)
        {
            if (view != activeArtifactPopupView
                || levelIndex < 0
                || levelIndex >= view.PopupLevels.Length
                || view.PopupLevels[levelIndex] == null)
            {
                return;
            }

            var level = view.PopupLevels[levelIndex];
            view.SynergyInfoTitle.text = level.Description;
            view.SynergyInfoText.text = $"{level.RequiredCount} 시너지";
            PushArtifactPopupPanel(view.SynergyInfoRoot);
        }

        private void PushArtifactPopupPanel(RectTransform panel)
        {
            if (panel == null)
            {
                return;
            }

            panel.gameObject.SetActive(true);
            if (!artifactPopupStack.Contains(panel))
            {
                artifactPopupStack.Add(panel);
            }
        }

        private void PopArtifactPopupStack()
        {
            if (artifactPopupStack.Count == 0)
            {
                return;
            }

            var lastIndex = artifactPopupStack.Count - 1;
            var panel = artifactPopupStack[lastIndex];
            artifactPopupStack.RemoveAt(lastIndex);
            panel.gameObject.SetActive(false);
            if (artifactPopupStack.Count == 0)
            {
                activeArtifactPopupView = null;
            }
        }

        private void ClearArtifactPopupStack()
        {
            for (var i = artifactPopupStack.Count - 1; i >= 0; i--)
            {
                if (artifactPopupStack[i] != null)
                {
                    artifactPopupStack[i].gameObject.SetActive(false);
                }
            }

            artifactPopupStack.Clear();
            if (activeArtifactPopupView != null)
            {
                activeArtifactPopupView.HidePopup();
                activeArtifactPopupView = null;
            }
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
            public Button PopupOpenButton;
            public RectTransform PopupRoot;
            public TMP_Text SynergyNameText;
            public Image SynergyIcon;
            public RectTransform SynergyInfoRoot;
            public TMP_Text SynergyInfoTitle;
            public TMP_Text SynergyInfoText;
            public RectTransform ArtifactInfoRoot;
            public TMP_Text ArtifactInfoTitle;
            public TMP_Text ArtifactInfoText;
            public RectTransform DismissBlocker;
            public RectTransform PopupHitArea;
            public readonly Image[] PopupArtifactImages = new Image[ArtifactPopupSlotCount];
            public readonly Button[] PopupArtifactButtons = new Button[ArtifactPopupSlotCount];
            public readonly ArtifactDefinition[] PopupArtifacts = new ArtifactDefinition[ArtifactPopupSlotCount];
            public readonly Image[] PopupLevelImages = new Image[ArtifactPopupLevelCounts.Length];
            public readonly Button[] PopupLevelButtons = new Button[ArtifactPopupLevelCounts.Length];
            public readonly ArtifactSynergyLevelDefinition[] PopupLevels = new ArtifactSynergyLevelDefinition[ArtifactPopupLevelCounts.Length];
            public readonly Color[] PopupLevelInactiveColors = new Color[ArtifactPopupLevelCounts.Length];
            public bool PopupConfigured;

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
                for (var i = 0; i < PopupLevelInactiveColors.Length; i++)
                {
                    PopupLevelInactiveColors[i] = PopupLevelImages[i] != null
                        ? PopupLevelImages[i].color
                        : Color.gray;
                }
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
                for (var i = 0; i < PopupLevelInactiveColors.Length; i++)
                {
                    PopupLevelInactiveColors[i] = source.PopupLevelInactiveColors[i];
                }
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

            public void HidePopup()
            {
                if (SynergyInfoRoot != null)
                {
                    SynergyInfoRoot.gameObject.SetActive(false);
                }
                if (ArtifactInfoRoot != null)
                {
                    ArtifactInfoRoot.gameObject.SetActive(false);
                }
                if (PopupRoot != null)
                {
                    PopupRoot.gameObject.SetActive(false);
                }
            }
        }

        private sealed class ArtifactDisplayEntry
        {
            public ArtifactSynergyDefinition Synergy { get; }
            public List<string> OwnedArtifactNames { get; } = new List<string>();
            public int Count => OwnedArtifactNames.Count;

            public ArtifactDisplayEntry(ArtifactSynergyDefinition synergy)
            {
                Synergy = synergy;
            }

            public void AddOwnedArtifact(string artifactName)
            {
                if (!string.IsNullOrWhiteSpace(artifactName))
                {
                    OwnedArtifactNames.Add(artifactName);
                }
            }
        }
    }
}
