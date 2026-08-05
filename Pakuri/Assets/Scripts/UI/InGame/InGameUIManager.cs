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
        private StageManager stageManager;
        private InGameInfoUI infoUI;
        private RewardPanelUI rewardPanelUI;
        private PrisonPanelUI prisonPanelUI;
        private OfferingUI offeringUI;
        private ArtifactUI artifactUI;
        private MenifestUI menifestUI;
        private BossHpUI bossHpUI;
        private DebugUI debugUI;

        private int shownStage = -1;
        private int shownDay = -1;
        private bool debugArtifactAcquisition;
        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            if (!BindObject() || !ValidateReferences())
            {
                enabled = false;
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

        internal int PrepareArtifactChoices(int requestedCount)
        {
            return artifactUI != null
                ? artifactUI.PrepareChoices(ResolveSession(), requestedCount)
                : 0;
        }

        internal bool OpenArtifactPanel()
        {
            return artifactUI != null && artifactUI.OpenPreparedChoices();
        }

        internal void OpenArtifactAcquisition(string artifactId)
        {
            debugArtifactAcquisition = false;
            artifactUI?.Hide();
            prisonPanelUI?.OpenArtifactAcquisition(artifactId);
        }

        internal void OpenArtifactDebugAcquisition(string artifactId)
        {
            if (string.IsNullOrWhiteSpace(artifactId))
            {
                return;
            }

            debugArtifactAcquisition = true;
            artifactUI?.Hide();
            prisonPanelUI?.OpenArtifactAcquisition(artifactId);
        }

        internal void OpenPrisonPanel()
        {
            prisonPanelUI?.Open();
        }

        internal void ContinueToNextDay()
        {
            HideTransientPanels();
            artifactUI?.Clear();
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

        internal void CompleteArtifactAcquisition()
        {
            if (debugArtifactAcquisition)
            {
                debugArtifactAcquisition = false;
                prisonPanelUI?.Hide();
                debugUI?.ShowArtifactAcquisitionDebug();
                RefreshInfo();
                return;
            }

            rewardPanelUI?.ConsumeActiveArtifactButton();
            prisonPanelUI?.Hide();
            artifactUI?.Clear();
            rewardPanelUI?.SetVisible(true);
            RefreshInfo();
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
            artifactUI?.Hide();
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
                && artifactUI != null
                && menifestUI != null
                && bossHpUI != null
                && debugUI != null)
            {
                return true;
            }

            Debug.LogError(
                "InGameUIManager requires StageManager "
                + "and all InGame UI components to be bound at runtime.",
                this);
            return false;
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
            stageManager = UiBindingUtility.BindSceneComponent<StageManager>(
                this,
                nameof(stageManager),
                ref valid);
            infoUI = UiBindingUtility.BindScene<InGameInfoUI>(
                this,
                "HUD/InfoPanel",
                nameof(infoUI),
                ref valid);
            rewardPanelUI = UiBindingUtility.BindScene<RewardPanelUI>(
                this,
                "Reward/RewardPanel",
                nameof(rewardPanelUI),
                ref valid);
            prisonPanelUI = UiBindingUtility.BindScene<PrisonPanelUI>(
                this,
                "Reward/PrisonPanel",
                nameof(prisonPanelUI),
                ref valid);
            offeringUI = UiBindingUtility.BindScene<OfferingUI>(
                this,
                "Reward/OfferingPanel",
                nameof(offeringUI),
                ref valid);
            artifactUI = UiBindingUtility.BindScene<ArtifactUI>(
                this,
                "Reward/ArtifactPanel",
                nameof(artifactUI),
                ref valid);
            menifestUI = UiBindingUtility.BindScene<MenifestUI>(
                this,
                "Popup",
                nameof(menifestUI),
                ref valid);
            bossHpUI = UiBindingUtility.BindScene<BossHpUI>(
                this,
                "HUD/BossHP",
                nameof(bossHpUI),
                ref valid);
            debugUI = UiBindingUtility.BindSceneComponent<DebugUI>(
                this,
                nameof(debugUI),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
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
