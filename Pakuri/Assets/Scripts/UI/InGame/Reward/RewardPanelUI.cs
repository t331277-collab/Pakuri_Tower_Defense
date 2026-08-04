using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// Stage 보상 버튼과 보상 패널의 표시·소비 상태를 관리한다.
    public sealed class RewardPanelUI : MonoBehaviour
    {
        private readonly List<RewardButtonView> rewardButtons = new List<RewardButtonView>();

        private GameObject rewardPanel;
        private Transform rewardButtonContainer;
        private Button prisonerTemplateButton;
        private Button goldTemplateButton;
        private Button darkTemplateButton;
        private Button nextButton;
        private TMP_Text rewardSummaryText;
        private InGameUIManager uiManager;
        [SerializeField] private Vector2 rewardButtonFirstColumnPosition = new Vector2(-321.97855f, 295f);
        [SerializeField] private float rewardButtonColumnSpacingX = 533.97855f;
        [SerializeField] private float rewardButtonRowSpacingY = 122f;
        [SerializeField] private int rewardButtonRowsPerColumn = 3;

        internal RewardButtonView ActivePrisonerButton { get; private set; }
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
            var prisoners = manager.PendingPrisonerEnemyIds;
            for (var i = 0; i < prisoners.Count; i++)
            {
                var prisonerId = prisoners[i];
                var button = CreateRewardButton(prisonerTemplateButton, "PrisonerReward", order++);
                SetButtonLabel(button, $"Prisoner\n{uiManager.ResolvePrisonerDisplayName(prisonerId)}");
                var view = RegisterRewardButton(button, prisonerId);
                BindButton(button, () => OpenPrisonPanel(view));
            }

            if (manager.PendingGoldReward > 0)
            {
                var amount = manager.PendingGoldReward;
                var button = CreateRewardButton(goldTemplateButton, "GoldReward", order++);
                SetButtonLabel(button, $"Gold\n+{amount}");
                var view = RegisterRewardButton(button, string.Empty);
                BindButton(button, () => ClaimMaterialReward(view, amount, 0));
            }

            if (manager.PendingDarkTraceReward > 0)
            {
                var amount = manager.PendingDarkTraceReward;
                var button = CreateRewardButton(darkTemplateButton, "DarkTraceReward", order++);
                SetButtonLabel(button, $"Dark Trace\n+{amount}");
                var view = RegisterRewardButton(button, string.Empty);
                BindButton(button, () => ClaimMaterialReward(view, 0, amount));
            }

            SetTemplateActive(prisonerTemplateButton, false);
            SetTemplateActive(goldTemplateButton, false);
            SetTemplateActive(darkTemplateButton, false);
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
            for (var i = 0; i < rewardButtons.Count; i++)
            {
                var button = rewardButtons[i].Button;
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            rewardButtons.Clear();
            ActivePrisonerButton = null;
            SetTemplateActive(prisonerTemplateButton, false);
            SetTemplateActive(goldTemplateButton, false);
            SetTemplateActive(darkTemplateButton, false);
        }

        public void ConsumeActivePrisonerButton()
        {
            ActivePrisonerButton?.SetConsumed();
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

        private Button CreateRewardButton(Button template, string namePrefix, int order)
        {
            if (template == null || rewardButtonContainer == null)
            {
                return null;
            }

            var button = Instantiate(template, rewardButtonContainer);
            button.gameObject.name = $"{namePrefix}_{order + 1}";
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            ArrangeRewardButton(button, order);
            return button;
        }

        private RewardButtonView RegisterRewardButton(Button button, string prisonerId)
        {
            var view = new RewardButtonView(button, prisonerId);
            rewardButtons.Add(view);
            return view;
        }

        private void ArrangeRewardButton(Button button, int order)
        {
            var rect = button != null ? button.transform as RectTransform : null;
            var baseRect = prisonerTemplateButton != null ? prisonerTemplateButton.transform as RectTransform : null;
            if (rect == null || baseRect == null)
            {
                return;
            }

            rect.anchorMin = baseRect.anchorMin;
            rect.anchorMax = baseRect.anchorMax;
            rect.pivot = baseRect.pivot;
            rect.sizeDelta = baseRect.sizeDelta;
            var rowsPerColumn = Mathf.Max(1, rewardButtonRowsPerColumn);
            var column = order / rowsPerColumn;
            var row = order % rowsPerColumn;
            var x = rewardButtonFirstColumnPosition.x + rewardButtonColumnSpacingX * column;
            var y = rewardButtonFirstColumnPosition.y - rewardButtonRowSpacingY * row;
            rect.anchoredPosition = new Vector2(x, y);
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

        private static void SetButtonLabel(Button button, string text)
        {
            var label = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetTemplateActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
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
            prisonerTemplateButton = UiBindingUtility.BindChild<Button>(
                this,
                "RewardBtnContainer/PrisonerBtn",
                nameof(prisonerTemplateButton),
                ref valid);
            goldTemplateButton = UiBindingUtility.BindChild<Button>(
                this,
                "RewardBtnContainer/GoldBtn",
                nameof(goldTemplateButton),
                ref valid);
            darkTemplateButton = UiBindingUtility.BindChild<Button>(
                this,
                "RewardBtnContainer/DarkBtn",
                nameof(darkTemplateButton),
                ref valid);
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
