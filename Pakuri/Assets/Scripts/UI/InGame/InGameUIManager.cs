/*
 * 역할: InGame UI 컴포넌트 사이의 Stage 흐름만 제어한다.
 * 세부 표시·입력·보상 처리는 각 MonoBehaviour UI가 담당한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public class InGameUIManager : MonoBehaviour
    {
        [SerializeField] private StageManager stageManager;
        [SerializeField] private InGameInfoUI infoUI;
        [SerializeField] private RewardPanelUI rewardPanelUI;
        [SerializeField] private PrisonPanelUI prisonPanelUI;
        [SerializeField] private OfferingUI offeringUI;
        [SerializeField] private MenifestUI menifestUI;
        [SerializeField] private BossHpUI bossHpUI;

        private int shownStage = -1;
        private int shownDay = -1;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                return;
            }

            HideTransientPanels();
        }

        private void Update()
        {
            RefreshInfo();

            if (stageManager == null || stageManager.State != StageState.RewardReady)
            {
                bossHpUI?.Refresh();
                return;
            }

            if (shownStage == stageManager.CurrentStage && shownDay == stageManager.CurrentDay)
            {
                return;
            }

            ShowRewardPanel();
        }

        internal RewardButtonView ActivePrisonerButton => rewardPanelUI?.ActivePrisonerButton;

        internal void OpenPrisonPanel()
        {
            prisonPanelUI?.Open();
        }

        internal void ContinueToNextDay()
        {
            HideTransientPanels();
            rewardPanelUI?.Clear();
            shownStage = -1;
            shownDay = -1;
            stageManager?.ContinueToNextDay();
        }

        internal void CompletePrisonAction()
        {
            prisonPanelUI?.Hide();
            offeringUI?.Hide();
            menifestUI?.Hide();
            rewardPanelUI?.SetVisible(true);
            RefreshInfo();
        }

        internal void ConsumeActivePrisonerButton()
        {
            rewardPanelUI?.ConsumeActivePrisonerButton();
        }

        internal void RefreshInfo()
        {
            infoUI?.Refresh(
                stageManager,
                ResolveSession(),
                prisonPanelUI != null && prisonPanelUI.IsVisible);
        }

        internal RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        internal string ResolvePrisonerDisplayName(string prisonerId)
        {
            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.DisplayName))
            {
                return enemy.DisplayName;
            }

            return string.IsNullOrWhiteSpace(prisonerId) ? "Unknown" : prisonerId;
        }

        private void ShowRewardPanel()
        {
            shownStage = stageManager.CurrentStage;
            shownDay = stageManager.CurrentDay;
            HideTransientPanels();
            rewardPanelUI?.Show(stageManager);
        }

        private void HideTransientPanels()
        {
            rewardPanelUI?.Hide();
            prisonPanelUI?.Hide();
            offeringUI?.Hide();
            menifestUI?.Hide();
            bossHpUI?.Hide();
        }

        private bool ValidateReferences()
        {
            if (stageManager != null
                && infoUI != null
                && rewardPanelUI != null
                && prisonPanelUI != null
                && offeringUI != null
                && menifestUI != null
                && bossHpUI != null)
            {
                return true;
            }

            Debug.LogError(
                "InGameUIManager requires StageManager "
                + "and all InGame UI components to be assigned in the Inspector.",
                this);
            return false;
        }
    }

    internal static class UiObjectUtility
    {
        internal static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
