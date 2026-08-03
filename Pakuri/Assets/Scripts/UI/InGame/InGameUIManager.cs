/*
 * 역할: InGame UI 모듈을 조립하고 Stage 흐름만 제어한다.
 * 세부 표시·입력·보상 처리는 Reward/Info 하위 모듈이 담당한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public class InGameUIManager : MonoBehaviour
    {
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private InGameUIReferences uiReferences = new InGameUIReferences();
        [SerializeField] private Vector2 rewardButtonFirstColumnPosition = new Vector2(-321.97855f, 295f);
        [SerializeField] private float rewardButtonColumnSpacingX = 533.97855f;
        [SerializeField] private float rewardButtonRowSpacingY = 122f;
        [SerializeField] private int rewardButtonRowsPerColumn = 3;

        private int shownStage = -1;
        private int shownDay = -1;
        private InGameInfoUI infoUI;
        private RewardPanelUI rewardPanelUI;
        private PrisonPanelUI prisonPanelUI;
        private OfferingUI offeringUI;
        private MenifestUI menifestUI;
        private BossHpUI bossHpUI;

        private void Awake()
        {
            if (!ValidateReferences())
            {
                return;
            }

            CreateUiModules();
            BindStaticButtons();
            HideTransientPanels();
        }

        private void Update()
        {
            RefreshInfo();

            if (stageManager == null || stageManager.State != StageState.RewardReady)
            {
                bossHpUI?.Refresh(unitSpawnManager);
                return;
            }

            if (shownStage == stageManager.CurrentStage && shownDay == stageManager.CurrentDay)
            {
                return;
            }

            ShowRewardPanel();
        }

        private void ShowRewardPanel()
        {
            shownStage = stageManager.CurrentStage;
            shownDay = stageManager.CurrentDay;
            HideTransientPanels();
            rewardPanelUI?.Show(stageManager);
        }

        private void ContinueToNextDay()
        {
            HideTransientPanels();
            rewardPanelUI?.Clear();
            shownStage = -1;
            shownDay = -1;

            stageManager?.ContinueToNextDay();
        }

        private void CompletePrisonAction()
        {
            prisonPanelUI?.Hide();
            offeringUI?.Hide();
            menifestUI?.Hide();
            rewardPanelUI?.SetVisible(true);
            RefreshInfo();
        }

        private void RefreshInfo()
        {
            infoUI?.Refresh(
                stageManager,
                ResolveSession(),
                prisonPanelUI != null && prisonPanelUI.IsVisible);
        }

        private void CreateUiModules()
        {
            rewardPanelUI = new RewardPanelUI(
                uiReferences.reward,
                rewardButtonFirstColumnPosition,
                rewardButtonColumnSpacingX,
                rewardButtonRowSpacingY,
                rewardButtonRowsPerColumn,
                ResolveSession,
                ResolvePrisonerDisplayName,
                () => prisonPanelUI?.Open(),
                ContinueToNextDay,
                RefreshInfo);

            offeringUI = new OfferingUI(
                uiReferences.offering,
                ResolveSession,
                ResolveCombatManager,
                () => rewardPanelUI?.ActivePrisonerButton,
                () => rewardPanelUI?.ConsumeActivePrisonerButton(),
                CompletePrisonAction,
                RefreshInfo);

            menifestUI = new MenifestUI(
                uiReferences.menifest,
                ResolveSession,
                ResolveStageManager,
                ResolveUnitSpawnManager,
                () => rewardPanelUI?.ActivePrisonerButton,
                () => rewardPanelUI?.ConsumeActivePrisonerButton(),
                CompletePrisonAction,
                RefreshInfo);

            prisonPanelUI = new PrisonPanelUI(
                uiReferences.prison,
                ResolveSession,
                ResolvePrisonerDisplayName,
                () => rewardPanelUI?.ActivePrisonerButton,
                offeringUI,
                menifestUI,
                RefreshInfo);

            infoUI = new InGameInfoUI(uiReferences.info);
            bossHpUI = new BossHpUI(uiReferences.bossHp);
        }

        private void BindStaticButtons()
        {
            prisonPanelUI?.BindStaticButtons();
        }

        private void HideTransientPanels()
        {
            rewardPanelUI?.Hide();
            prisonPanelUI?.Hide();
            offeringUI?.Hide();
            menifestUI?.Hide();
            bossHpUI?.Hide();
        }

        private RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        private string ResolvePrisonerDisplayName(string prisonerId)
        {
            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.DisplayName))
            {
                return enemy.DisplayName;
            }

            return string.IsNullOrWhiteSpace(prisonerId) ? "Unknown" : prisonerId;
        }

        private InGameCombatManager ResolveCombatManager()
        {
            return combatManager;
        }

        private StageManager ResolveStageManager()
        {
            return stageManager;
        }

        private UnitSpawnManager ResolveUnitSpawnManager()
        {
            return unitSpawnManager;
        }

        private bool ValidateReferences()
        {
            if (stageManager != null && unitSpawnManager != null && combatManager != null && uiReferences != null)
            {
                return true;
            }

            Debug.LogError(
                "InGameUIManager requires StageManager, UnitSpawnManager, InGameCombatManager, " +
                "and InGameUIReferences to be assigned in the Inspector.",
                this);
            return false;
        }
    }
}
