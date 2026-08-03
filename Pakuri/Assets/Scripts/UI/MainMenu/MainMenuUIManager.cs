/*
 * 역할: Main Menu 이동.
 * 책임: Menu 씬 참조와 Button을 연결하고 몬스터 선택 및 Run 시작을 처리한다.
 */

using Pakuri.InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject monsterSelectPanel;

    [Header("Buttons")]
    [SerializeField] private Button introGameStartButton;
    [SerializeField] private Button runButton;
    [SerializeField] private Button monsterSelectGameStartButton;
    [SerializeField] private Button arielButton;
    [SerializeField] private Button eveButton;
    [SerializeField] private Button seinButton;
    [SerializeField] private Button vegaButton;
    [SerializeField] private Button rinButton;

    [Header("Scene")]
    [SerializeField] private string newRunScenePath = "Assets/Scenes/NewScene/InGameScene.unity";
    [SerializeField] private string defaultMonsterId = "eve";

    private string selectedMonsterId;

    /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
    private void Awake()
    {
        BindButtons();
    }

    /// 컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.
    private void Start()
    {
        ShowIntro();
    }

    private void BindButtons()
    {
        Bind(introGameStartButton, ShowMainMenu);
        Bind(runButton, ShowMonsterSelect);
        Bind(monsterSelectGameStartButton, StartSelectedMonsterRun);

        Bind(arielButton, () => SelectMonster("ariel"));
        Bind(eveButton, () => SelectMonster("eve"));
        Bind(seinButton, () => SelectMonster("sein"));
        Bind(vegaButton, () => SelectMonster("vega"));
        Bind(rinButton, () => SelectMonster("rin"));
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    /// 인트로 패널을 열어 게임 시작 화면을 보여준다.
    private void ShowIntro()
    {
        SetOnlyActive(introPanel);
    }

    /// 메인 메뉴 패널을 보여준다.
    private void ShowMainMenu()
    {
        SetOnlyActive(mainMenuPanel);
    }

    /// 몬스터 선택 패널을 열어 출전 몬스터를 고르게 한다.
    private void ShowMonsterSelect()
    {
        SetOnlyActive(monsterSelectPanel);
    }

    private void SelectMonster(string monsterId)
    {
        selectedMonsterId = monsterId;
    }

    /// 선택한 몬스터를 StartContext에 저장하고 Run 씬을 연다.
    private void StartSelectedMonsterRun()
    {
        var monsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? defaultMonsterId : selectedMonsterId;
            StartContext.Prepare(monsterId);

        if (string.IsNullOrWhiteSpace(newRunScenePath))
        {
            Debug.LogError("MainMenuUIManager cannot load InGameScene because the scene path is empty.");
            return;
        }

        SceneManager.LoadScene(newRunScenePath);
    }

    private void SetOnlyActive(GameObject activePanel)
    {
        UiObjectUtility.SetActive(introPanel, introPanel == activePanel);
        UiObjectUtility.SetActive(mainMenuPanel, mainMenuPanel == activePanel);
        UiObjectUtility.SetActive(monsterSelectPanel, monsterSelectPanel == activePanel);
    }

}
