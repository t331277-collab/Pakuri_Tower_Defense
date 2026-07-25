using System;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using Pakuri.NewCore.Units.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* 인게임 panel 전환과 stage 결과 표시를 runtime command 경계에 연결한다. */
namespace Pakuri.NewCore.UI.InGame
{
    public class InGameUIManager : MonoBehaviour
    {
        [SerializeField] private GameBootstrap combatManager;
        [SerializeField] private RewardPanelController rewardPanelController;
        [SerializeField] private PrisonPanelController prisonPanelController;
        [SerializeField] private OfferingPanelController offeringPanelController;
        [SerializeField] private ManifestationPanelController
            manifestationPanelController;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private Button winButton;
        [SerializeField] private Button defeatButton;
        [SerializeField] private string mainMenuScenePath =
            "Assets/Scenes/NewScene/NewMainMenu.unity";

        /* runtime과 네 panel owner를 연결하고 초기 화면 상태를 만든다. */
        private void Awake()
        {
            ResolveReferences();
            rewardPanelController.Initialize(
                combatManager,
                OpenPrisoner,
                ContinueRun);
            prisonPanelController.Initialize(
                combatManager,
                OpenOffering,
                BeginManifestation);
            offeringPanelController.Initialize(
                combatManager,
                CompletePrisonAction);
            manifestationPanelController.Initialize(
                combatManager,
                CompletePrisonAction);
            Bind(winButton, ReturnToMainMenu);
            Bind(defeatButton, ReturnToMainMenu);
            SetActive(winPanel, false);
            SetActive(defeatPanel, false);
        }

        /* 활성 Prison panel의 stage와 재화 표시를 runtime 상태와 동기화한다. */
        private void Update()
        {
            prisonPanelController.RefreshInfo();
        }

        /* 지급된 보상과 현재 포로 목록을 Reward panel에 표시한다. */
        public void ShowReward(RewardResult reward)
        {
            rewardPanelController.Show(reward);
        }

        /* 최종 승패에 대응하는 단일 result panel만 표시한다. */
        public void ShowResult(bool victory)
        {
            SetActive(winPanel, victory);
            SetActive(defeatPanel, !victory);
        }

        /* Reward button이 선택한 포로를 Prison panel flow로 연다. */
        private void OpenPrisoner(
            Prisoner prisoner,
            Button sourceButton)
        {
            prisonPanelController.Open(prisoner, sourceButton);
        }

        /* 점유 party slot의 Monster와 포로로 Offering panel을 연다. */
        private void OpenOffering(
            MonsterModel monster,
            Prisoner prisoner)
        {
            if (offeringPanelController.Open(monster, prisoner))
            {
                prisonPanelController.HidePanel();
            }
        }

        /* 다음 빈 party slot의 포로로 현현 시도를 시작한다. */
        private void BeginManifestation(Prisoner prisoner)
        {
            if (combatManager.CurrentReward != null
                && manifestationPanelController.Begin(
                    prisoner,
                    combatManager.CurrentReward.Definition))
            {
                prisonPanelController.HidePanel();
            }
        }

        /* Offering 또는 현현 종료 뒤 overlay flow를 닫고 Reward panel로 돌아간다. */
        private void CompletePrisonAction()
        {
            offeringPanelController.Hide();
            manifestationPanelController.HideAll();
            prisonPanelController.CloseFlow();
        }

        /* 현재 reward 처리를 완료하고 다음 day 또는 최종 승리 화면으로 진행한다. */
        private void ContinueRun()
        {
            if (combatManager.CompleteRewardAndAdvance())
            {
                rewardPanelController.Hide();
                prisonPanelController.CloseFlow();
            }
        }

        /* scene의 section 19 UI owner와 bootstrap 참조를 찾아 필수 연결을 검증한다. */
        private void ResolveReferences()
        {
            if (combatManager == null)
            {
                combatManager = FindFirstObjectByType<GameBootstrap>(
                    FindObjectsInactive.Include);
            }

            rewardPanelController ??=
                GetComponent<RewardPanelController>();
            prisonPanelController ??=
                GetComponent<PrisonPanelController>();
            offeringPanelController ??=
                GetComponent<OfferingPanelController>();
            manifestationPanelController ??=
                GetComponent<ManifestationPanelController>();
        }

        /* authored main menu scene으로 결과 flow를 복귀시킨다. */
        private void ReturnToMainMenu()
        {
            if (!string.IsNullOrWhiteSpace(mainMenuScenePath))
            {
                SceneManager.LoadScene(mainMenuScenePath);
            }
        }

        /* 선택 button에 result callback 하나를 연결한다. */
        private static void Bind(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        /* 선택 panel의 활성 상태를 존재하는 경우에만 바꾼다. */
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
