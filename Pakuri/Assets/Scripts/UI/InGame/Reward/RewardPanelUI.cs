using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// Stage 보상 버튼과 보상 패널의 표시·소비 상태를 관리한다.
    internal sealed class RewardPanelUI
    {
        private readonly List<RewardButtonView> rewardButtons = new List<RewardButtonView>();
        private readonly GameObject rewardPanel;
        private readonly Transform rewardButtonContainer;
        private readonly Button prisonerTemplateButton;
        private readonly Button goldTemplateButton;
        private readonly Button darkTemplateButton;
        private readonly TMP_Text rewardSummaryText;
        private readonly Vector2 rewardButtonFirstColumnPosition;
        private readonly float rewardButtonColumnSpacingX;
        private readonly float rewardButtonRowSpacingY;
        private readonly int rewardButtonRowsPerColumn;
        private readonly Func<RunSession> resolveSession;
        private readonly Func<string, string> resolvePrisonerDisplayName;
        private readonly Action openPrisonPanel;
        private readonly Action continueToNextDay;
        private readonly Action refreshInfo;

        public RewardPanelUI(
            InGameRewardPanelReferences references,
            Vector2 rewardButtonFirstColumnPosition,
            float rewardButtonColumnSpacingX,
            float rewardButtonRowSpacingY,
            int rewardButtonRowsPerColumn,
            Func<RunSession> resolveSession,
            Func<string, string> resolvePrisonerDisplayName,
            Action openPrisonPanel,
            Action continueToNextDay,
            Action refreshInfo)
        {
            rewardPanel = references != null ? references.rewardPanel : null;
            rewardButtonContainer = references != null ? references.rewardButtonContainer : null;
            prisonerTemplateButton = references != null ? references.prisonerTemplateButton : null;
            darkTemplateButton = references != null ? references.darkTemplateButton : null;
            goldTemplateButton = references != null ? references.goldTemplateButton : null;
            rewardSummaryText = references != null ? references.rewardSummaryText : null;
            this.rewardButtonFirstColumnPosition = rewardButtonFirstColumnPosition;
            this.rewardButtonColumnSpacingX = rewardButtonColumnSpacingX;
            this.rewardButtonRowSpacingY = rewardButtonRowSpacingY;
            this.rewardButtonRowsPerColumn = rewardButtonRowsPerColumn;
            this.resolveSession = resolveSession;
            this.resolvePrisonerDisplayName = resolvePrisonerDisplayName;
            this.openPrisonPanel = openPrisonPanel;
            this.continueToNextDay = continueToNextDay;
            this.refreshInfo = refreshInfo;

            BindButton(references != null ? references.nextButton : null, ContinueToNextDay);
        }

        public RewardButtonView ActivePrisonerButton { get; private set; }

        public void Show(StageManager stageManager)
        {
            if (stageManager == null)
            {
                return;
            }

            Clear();
            SetActive(rewardPanel, true);
            if (rewardSummaryText != null)
            {
                rewardSummaryText.text = $"Stage {stageManager.CurrentStage}-{stageManager.CurrentDay} Reward";
            }

            var order = 0;
            var prisoners = stageManager.PendingPrisonerEnemyIds;
            for (var i = 0; i < prisoners.Count; i++)
            {
                var prisonerId = prisoners[i];
                var button = CreateRewardButton(prisonerTemplateButton, "PrisonerReward", order++);
                SetButtonLabel(button, $"Prisoner\n{resolvePrisonerDisplayName(prisonerId)}");
                var view = RegisterRewardButton(button, RewardKind.Prisoner, 0, prisonerId);
                BindButton(button, () => OpenPrisonPanel(view));
            }

            if (stageManager.PendingGoldReward > 0)
            {
                var amount = stageManager.PendingGoldReward;
                var button = CreateRewardButton(goldTemplateButton, "GoldReward", order++);
                SetButtonLabel(button, $"Gold\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.Gold, amount, string.Empty);
                BindButton(button, () => ClaimMaterialReward(view, amount, 0));
            }

            if (stageManager.PendingDarkTraceReward > 0)
            {
                var amount = stageManager.PendingDarkTraceReward;
                var button = CreateRewardButton(darkTemplateButton, "DarkTraceReward", order++);
                SetButtonLabel(button, $"Dark Trace\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.DarkTrace, amount, string.Empty);
                BindButton(button, () => ClaimMaterialReward(view, 0, amount));
            }

            SetTemplateActive(prisonerTemplateButton, false);
            SetTemplateActive(goldTemplateButton, false);
            SetTemplateActive(darkTemplateButton, false);
            refreshInfo?.Invoke();
        }

        public void Hide()
        {
            SetActive(rewardPanel, false);
        }

        public void SetVisible(bool visible)
        {
            SetActive(rewardPanel, visible);
        }

        public void Clear()
        {
            for (var i = 0; i < rewardButtons.Count; i++)
            {
                var button = rewardButtons[i].Button;
                if (button != null)
                {
                    UnityEngine.Object.Destroy(button.gameObject);
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
            continueToNextDay?.Invoke();
        }

        private void OpenPrisonPanel(RewardButtonView view)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            ActivePrisonerButton = view;
            SetActive(rewardPanel, false);
            openPrisonPanel?.Invoke();
        }

        private void ClaimMaterialReward(RewardButtonView view, int gold, int darkTrace)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            var session = resolveSession?.Invoke();
            if (session == null)
            {
                return;
            }

            session.ClaimMaterialReward(gold, darkTrace);
            view.SetConsumed();
            refreshInfo?.Invoke();
        }

        private Button CreateRewardButton(Button template, string namePrefix, int order)
        {
            if (template == null || rewardButtonContainer == null)
            {
                return null;
            }

            var button = UnityEngine.Object.Instantiate(template, rewardButtonContainer);
            button.gameObject.name = $"{namePrefix}_{order + 1}";
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            ArrangeRewardButton(button, order);
            return button;
        }

        private RewardButtonView RegisterRewardButton(Button button, RewardKind kind, int amount, string prisonerId)
        {
            var view = new RewardButtonView(button, kind, amount, prisonerId);
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
            var x = rewardButtonFirstColumnPosition.x + (rewardButtonColumnSpacingX * column);
            var y = rewardButtonFirstColumnPosition.y - (rewardButtonRowSpacingY * row);
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
