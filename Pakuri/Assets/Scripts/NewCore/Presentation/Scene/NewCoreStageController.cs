using Pakuri.NewCore.Presentation.Actors;
using Pakuri.NewCore.Presentation.Assets;
using Pakuri.NewCore.Catalog;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.Scene
{
    public sealed class NewCoreStageController : MonoBehaviour
    {
        [SerializeField] private NewCoreSceneRuntime combatManager;
        [SerializeField] private NewCoreSpawnController unitSpawnManager;
        [SerializeField] private TextAsset stageDayCsv;
        [SerializeField] private TextAsset stageEncounterCsv;
        [SerializeField] private TextAsset stageRewardCsv;
        [SerializeField] private bool startFlowOnStart = true;
        [SerializeField] private float clearCheckInterval = 0.25f;
        [SerializeField] private bool restorePlayerHealthOnDayAdvance = true;
        [SerializeField] private NexusActorBehaviour nexusActor;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private Button winButton;
        [SerializeField] private Button defeatButton;
        [SerializeField] private string mainMenuScenePath =
            "Assets/Scenes/NewScene/NewMainMenu.unity";
        [SerializeField] private int winStageIndex = 2;
        [SerializeField] private int winDayIndex = 11;

        public bool StartFlowOnStart => startFlowOnStart;
        public float ClearCheckInterval => clearCheckInterval;
        public NexusActorBehaviour NexusActor => nexusActor;

        private void Awake()
        {
            SetActive(winPanel, false);
            SetActive(defeatPanel, false);
            BindButton(winButton, ReturnToMainMenu);
            BindButton(defeatButton, ReturnToMainMenu);
        }

        public void ValidateConnections(
            NewCoreRuntimeCatalogAsset catalog,
            GameDefinitionCatalog definitions)
        {
            if (combatManager == null)
            {
                combatManager = GetComponent<NewCoreSceneRuntime>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = GetComponent<NewCoreSpawnController>();
            }

            if (combatManager == null
                || unitSpawnManager == null
                || nexusActor == null)
            {
                throw new System.InvalidOperationException(
                    "New Core stage scene references are incomplete.");
            }

            if (stageDayCsv != catalog.StageDay
                || stageEncounterCsv != catalog.StageEncounter
                || stageRewardCsv != catalog.StageReward)
            {
                throw new System.InvalidOperationException(
                    "Stage CSV Inspector values do not match the New Core runtime catalog.");
            }

            if (!restorePlayerHealthOnDayAdvance)
            {
                throw new System.InvalidOperationException(
                    "Current scene requires player health restoration on day advance.");
            }

            if (clearCheckInterval <= 0f)
            {
                throw new System.InvalidOperationException(
                    "Clear-check interval must be positive.");
            }

            if (winStageIndex < 1 || winDayIndex < 1)
            {
                throw new System.InvalidOperationException(
                    "Win-stage Inspector values must be positive.");
            }

            ValidateWinBoundary(definitions);
        }

        public void ShowResult(bool victory)
        {
            SetActive(winPanel, victory);
            SetActive(defeatPanel, !victory);
        }

        private void ReturnToMainMenu()
        {
            if (!string.IsNullOrWhiteSpace(mainMenuScenePath))
            {
                SceneManager.LoadScene(mainMenuScenePath);
            }
        }

        private static void BindButton(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.AddListener(action);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private void ValidateWinBoundary(GameDefinitionCatalog definitions)
        {
            var finalStage = 0;
            var finalDay = 0;
            foreach (var day in definitions.StageDays.Values)
            {
                if (!day.stage.HasValue || !day.day.HasValue)
                {
                    continue;
                }

                if (day.stage.Value > finalStage
                    || (day.stage.Value == finalStage
                        && day.day.Value > finalDay))
                {
                    finalStage = day.stage.Value;
                    finalDay = day.day.Value;
                }
            }

            if (winStageIndex != finalStage || winDayIndex != finalDay)
            {
                throw new System.InvalidOperationException(
                    "Win-stage Inspector values do not match the final StageDay.");
            }
        }
    }
}
