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
    private const string NewRunScenePath = "Assets/Scenes/NewScene/InGameScene.unity";
    private const string DefaultMonsterId = "eve";

    private GameObject introPanel;
    private GameObject mainMenuPanel;
    private GameObject monsterSelectPanel;
    private Button introGameStartButton;
    private Button runButton;
    private Button monsterSelectGameStartButton;
    private Button arielButton;
    private Button eveButton;
    private Button seinButton;
    private Button vegaButton;
    private Button rinButton;

    private string selectedMonsterId;

    /// 컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.
    private void Start()
    {
        if (!BindObject())
        {
            return;
        }

        BindButtons();
        ShowIntro();
    }

    /// MainMenuScene의 패널과 버튼을 계층 경로로 찾아 런타임 참조를 구성한다.
    private bool BindObject()
    {
        var canvas = FindSceneRoot("Canvas");
        if (canvas == null)
        {
            Debug.LogError(
                "MainMenuUIManager BindObject failed: scene root 'Canvas' was not found.",
                this);
            return false;
        }

        var valid = true;
        introPanel = FindGameObject(canvas, "Intro", nameof(introPanel), ref valid);
        mainMenuPanel = FindGameObject(canvas, "MainMenuUI", nameof(mainMenuPanel), ref valid);
        monsterSelectPanel = FindGameObject(canvas, "MosterSelectUI", nameof(monsterSelectPanel), ref valid);

        introGameStartButton = FindComponent<Button>(canvas, "Intro/GameStart", nameof(introGameStartButton), ref valid);
        runButton = FindComponent<Button>(canvas, "MainMenuUI/RunBtn", nameof(runButton), ref valid);
        monsterSelectGameStartButton = FindComponent<Button>(canvas, "MosterSelectUI/GameStart", nameof(monsterSelectGameStartButton), ref valid);
        arielButton = FindComponent<Button>(canvas, "MosterSelectUI/Ariel", nameof(arielButton), ref valid);
        eveButton = FindComponent<Button>(canvas, "MosterSelectUI/Eve", nameof(eveButton), ref valid);
        seinButton = FindComponent<Button>(canvas, "MosterSelectUI/Sein", nameof(seinButton), ref valid);
        vegaButton = FindComponent<Button>(canvas, "MosterSelectUI/Vega", nameof(vegaButton), ref valid);
        rinButton = FindComponent<Button>(canvas, "MosterSelectUI/Rin", nameof(rinButton), ref valid);
        return valid;
    }

    /// 현재 씬의 루트 오브젝트 중 이름이 일치하는 계층을 찾는다.
    private GameObject FindSceneRoot(string objectName)
    {
        var roots = gameObject.scene.GetRootGameObjects();
        for (var i = 0; i < roots.Length; i++)
        {
            if (roots[i] != null && string.Equals(roots[i].name, objectName, System.StringComparison.Ordinal))
            {
                return roots[i];
            }
        }

        return null;
    }

    /// 계층 경로의 GameObject를 찾아 필드에 연결하고 누락 시 오류를 기록한다.
    private GameObject FindGameObject(
        GameObject root,
        string path,
        string fieldName,
        ref bool valid)
    {
        var target = root != null ? root.transform.Find(path) : null;
        if (target == null)
        {
            LogBindingError(fieldName, path, "GameObject");
            valid = false;
            return null;
        }

        return target.gameObject;
    }

    /// 계층 경로의 컴포넌트를 찾아 필드에 연결하고 누락 시 오류를 기록한다.
    private T FindComponent<T>(
        GameObject root,
        string path,
        string fieldName,
        ref bool valid)
        where T : Component
    {
        var target = root != null ? root.transform.Find(path) : null;
        var component = target != null ? target.GetComponent<T>() : null;
        if (component == null)
        {
            LogBindingError(fieldName, path, typeof(T).Name);
            valid = false;
            return null;
        }

        return component;
    }

    /// 자동 참조 연결 실패 원인을 필드·경로·타입과 함께 기록한다.
    private void LogBindingError(string fieldName, string path, string expectedType)
    {
        Debug.LogError(
            $"MainMenuUIManager BindObject failed: field '{fieldName}', path 'Canvas/{path}', expected '{expectedType}'.",
            this);
    }

    private void BindButtons()
    {
        Bind(introGameStartButton, ShowMainMenu, nameof(introGameStartButton));
        Bind(runButton, ShowMonsterSelect, nameof(runButton));
        Bind(monsterSelectGameStartButton, StartSelectedMonsterRun, nameof(monsterSelectGameStartButton));

        Bind(arielButton, () => SelectMonster("ariel"), nameof(arielButton));
        Bind(eveButton, () => SelectMonster("eve"), nameof(eveButton));
        Bind(seinButton, () => SelectMonster("sein"), nameof(seinButton));
        Bind(vegaButton, () => SelectMonster("vega"), nameof(vegaButton));
        Bind(rinButton, () => SelectMonster("rin"), nameof(rinButton));
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action, string fieldName)
    {
        if (button == null)
        {
            Debug.LogError($"MainMenuUIManager Bind failed: button field '{fieldName}' is null.");
            return;
        }

        if (action == null)
        {
            Debug.LogError($"MainMenuUIManager Bind failed: action for '{fieldName}' is null.");
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
        var monsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? DefaultMonsterId : selectedMonsterId;
            StartContext.Prepare(monsterId);

        if (string.IsNullOrWhiteSpace(NewRunScenePath))
        {
            Debug.LogError("MainMenuUIManager cannot load InGameScene because the scene path is empty.");
            return;
        }

        SceneManager.LoadScene(NewRunScenePath);
    }

    private void SetOnlyActive(GameObject activePanel)
    {
        UiObjectUtility.SetActive(introPanel, introPanel == activePanel);
        UiObjectUtility.SetActive(mainMenuPanel, mainMenuPanel == activePanel);
        UiObjectUtility.SetActive(monsterSelectPanel, monsterSelectPanel == activePanel);
    }

}
