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

        internal RewardButtonView ActivePrisonerButton { get; private set; }
        internal RewardButtonView ActiveArtifactButton { get; private set; }
        private bool referencesBound;
        private bool bindingFailed;

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
            UiObjectUtility.SetActive(rewardPanel, true);
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
                    BindButton(view.Button, () => OpenArtifactPanel(view));
                }
            }

            uiManager?.RefreshInfo();
        }

        public void Hide()
        {
            UiObjectUtility.SetActive(rewardPanel, false);
        }

        public void SetVisible(bool visible)
        {
            UiObjectUtility.SetActive(rewardPanel, visible);
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
        }

        public void ConsumeActivePrisonerButton()
        {
            ActivePrisonerButton?.SetConsumed();
        }

        public void ConsumeActiveArtifactButton()
        {
            ActiveArtifactButton?.SetConsumed();
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
            UiObjectUtility.SetActive(rewardPanel, false);
            uiManager?.OpenPrisonPanel();
        }

        private void OpenArtifactPanel(RewardButtonView view)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            ActiveArtifactButton = view;
            UiObjectUtility.SetActive(rewardPanel, false);
            if (uiManager == null || !uiManager.OpenArtifactPanel())
            {
                ActiveArtifactButton = null;
                UiObjectUtility.SetActive(rewardPanel, true);
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
