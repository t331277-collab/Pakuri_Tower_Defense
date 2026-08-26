using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// Stage 보상 버튼과 보상 패널의 표시·소비 상태를 관리한다.
    public sealed class RewardPanelUI : MonoBehaviour
    {
        private const int FixedRewardButtonCount = 13;

        private readonly List<RewardButtonView> rewardButtons = new List<RewardButtonView>();
        private readonly RewardButtonView[] rewardSlots = new RewardButtonView[FixedRewardButtonCount];

        private GameObject rewardPanel;
        private Transform rewardButtonContainer;
        private Button nextButton;
        private TMP_Text rewardSummaryText;
        private InGameUIManager uiManager;
        private RewardButtonView artifactRewardButton;
        private int prisonerButtonCount;

        internal RewardButtonView ActivePrisonerButton { get; private set; }
        internal RewardButtonView ActiveArtifactButton { get; private set; }
        private bool referencesBound;
        private bool bindingFailed;

        public event Action<bool> VisibilityChanged;
        public event Action RewardConsumed;
        public bool IsVisible => rewardPanel != null && rewardPanel.activeSelf;
        public bool AllActiveRewardsConsumed
        {
            get
            {
                for (var i = 0; i < rewardButtons.Count; i++)
                {
                    if (!rewardButtons[i].Consumed)
                    {
                        return false;
                    }
                }

                return rewardButtons.Count > 0;
            }
        }

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            BindButton(nextButton, ContinueToNextDay);
        }

        public void Show(StageManager manager)
        {
            if (manager == null || !BindObject())
            {
                return;
            }

            BindButton(nextButton, ContinueToNextDay);
            Clear();
            SetPanelVisible(true);
            if (rewardSummaryText != null)
            {
                rewardSummaryText.text = $"Stage {manager.CurrentStage}-{manager.CurrentDay} Reward";
            }

            var order = 0;
            var prisoners = manager.PendingPrisonerEnemyNames;
            for (var i = 0; i < prisoners.Count; i++)
            {
                var prisonerName = prisoners[i];
                var view = ActivateRewardSlot(order++);
                if (view == null)
                {
                    break;
                }

                view.SetDisplay("포로", uiManager.ResolvePrisonerDisplayName(prisonerName), prisonerName);
                BindButton(view.Button, () => OpenPrisonPanel(view));
            }
            prisonerButtonCount = order;

            if (manager.PendingGoldReward > 0)
            {
                var amount = manager.PendingGoldReward;
                var view = ActivateRewardSlot(order++);
                if (view != null)
                {
                    view.SetDisplay("골드", $"+{amount}", string.Empty);
                    BindButton(view.Button, () => ClaimMaterialReward(view, amount, 0));
                }
            }

            if (manager.PendingDarkTraceReward > 0)
            {
                var amount = manager.PendingDarkTraceReward;
                var view = ActivateRewardSlot(order++);
                if (view != null)
                {
                    view.SetDisplay("어둠의 흔적", $"+{amount}", string.Empty);
                    BindButton(view.Button, () => ClaimMaterialReward(view, 0, amount));
                }
            }

            var artifactChoiceCount = uiManager != null
                ? uiManager.PrepareArtifactChoices(manager.PendingArtifactChoiceCount)
                : 0;
            if (artifactChoiceCount > 0)
            {
                var view = ActivateRewardSlot(order++);
                if (view != null)
                {
                    view.SetDisplay("유물", artifactChoiceCount.ToString(), string.Empty);
                    artifactRewardButton = view;
                    BindButton(view.Button, () => OpenArtifactPanel(view));
                }
            }

            if (manager.ActiveSession != null && manager.ActiveSession.IsTutorial)
            {
                SetTutorialInteraction(-1, false, false, false);
            }

            uiManager?.RefreshInfo();
        }

        public void Hide()
        {
            SetPanelVisible(false);
        }

        public void SetVisible(bool visible)
        {
            SetPanelVisible(visible);
        }

        public void Clear()
        {
            for (var i = 0; i < rewardSlots.Length; i++)
            {
                var slot = rewardSlots[i];
                if (slot != null)
                {
                    slot.Reset();
                    slot.Button.gameObject.SetActive(false);
                }
            }

            rewardButtons.Clear();
            ActivePrisonerButton = null;
            ActiveArtifactButton = null;
            artifactRewardButton = null;
            prisonerButtonCount = 0;
        }

        public void ConsumeActivePrisonerButton()
        {
            if (ActivePrisonerButton == null || ActivePrisonerButton.Consumed)
            {
                return;
            }

            ActivePrisonerButton.SetConsumed();
            RewardConsumed?.Invoke();
        }

        public void ConsumeActiveArtifactButton()
        {
            if (ActiveArtifactButton == null || ActiveArtifactButton.Consumed)
            {
                return;
            }

            ActiveArtifactButton.SetConsumed();
            RewardConsumed?.Invoke();
        }

        public void SetTutorialInteraction(
            int allowedPrisonerIndex,
            bool allowMaterials,
            bool allowArtifact,
            bool allowNext)
        {
            for (var i = 0; i < rewardButtons.Count; i++)
            {
                var view = rewardButtons[i];
                var allowed = i < prisonerButtonCount
                    ? allowedPrisonerIndex == -2 || i == allowedPrisonerIndex
                    : view == artifactRewardButton ? allowArtifact : allowMaterials;
                view.Button.interactable = allowed && !view.Consumed;
            }

            if (nextButton != null)
            {
                nextButton.interactable = allowNext;
            }
        }

        private void ContinueToNextDay()
        {
            uiManager?.ContinueToNextDay();
        }

        private void OpenPrisonPanel(RewardButtonView view)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            ActivePrisonerButton = view;
            SetPanelVisible(false);
            uiManager?.OpenPrisonPanel();
        }

        private void OpenArtifactPanel(RewardButtonView view)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            ActiveArtifactButton = view;
            SetPanelVisible(false);
            if (uiManager == null || !uiManager.OpenArtifactPanel())
            {
                ActiveArtifactButton = null;
                SetPanelVisible(true);
            }
        }

        private void ClaimMaterialReward(RewardButtonView view, int gold, int darkTrace)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            var session = uiManager != null ? uiManager.ResolveSession() : null;
            if (session == null)
            {
                return;
            }

            session.ClaimMaterialReward(gold, darkTrace);
            view.SetConsumed();
            RewardConsumed?.Invoke();
            uiManager?.RefreshInfo();
        }

        private RewardButtonView ActivateRewardSlot(int order)
        {
            if (order < 0 || order >= rewardSlots.Length)
            {
                return null;
            }

            var view = rewardSlots[order];
            view.Reset();
            view.Button.gameObject.SetActive(true);
            rewardButtons.Add(view);
            return view;
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private void SetPanelVisible(bool visible)
        {
            var changed = rewardPanel != null && rewardPanel.activeSelf != visible;
            UiObjectUtility.SetActive(rewardPanel, visible);
            if (changed)
            {
                VisibilityChanged?.Invoke(visible);
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
            rewardPanel = gameObject;
            rewardButtonContainer = UiBindingUtility.BindChild<Transform>(
                this,
                "RewardBtnContainer",
                nameof(rewardButtonContainer),
                ref valid);
            for (var i = 0; i < rewardSlots.Length; i++)
            {
                var slotName = i == 0 ? "RewardBtn" : $"RewardBtn ({i})";
                var slotRoot = rewardButtonContainer.Find(slotName);
                if (slotRoot == null)
                {
                    Debug.LogError($"RewardPanelUI requires reward slot '{slotName}'.", this);
                    valid = false;
                    continue;
                }

                var button = UiBindingUtility.BindSelf<Button>(
                    this,
                    slotRoot,
                    $"rewardSlots[{i}].Button",
                    ref valid);
                var summary = UiBindingUtility.BindChild<TMP_Text>(
                    this,
                    slotRoot,
                    "Summary",
                    $"rewardSlots[{i}].Summary",
                    ref valid);
                var what = UiBindingUtility.BindChild<TMP_Text>(
                    this,
                    slotRoot,
                    "What",
                    $"rewardSlots[{i}].What",
                    ref valid);
                rewardSlots[i] = new RewardButtonView(button, summary, what, string.Empty);
            }
            nextButton = UiBindingUtility.BindChild<Button>(
                this,
                "NextBtn",
                nameof(nextButton),
                ref valid);
            rewardSummaryText = UiBindingUtility.BindChild<TMP_Text>(
                this,
                "Summary",
                nameof(rewardSummaryText),
                ref valid);
            uiManager = UiBindingUtility.BindSceneComponent<InGameUIManager>(
                this,
                nameof(uiManager),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

    }
}
