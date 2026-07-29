/*
 * 역할: Main Menu 이동.
 * 책임: Menu 씬 참조와 Button을 연결하고 몬스터 선택 및 Run 시작을 처리한다.
 */

using Pakuri.InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary><c>MainMenuUIManager</c>가 담당하는 작업을 조정하고 공유 런타임 상태를 소유한다.</summary>
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
    [SerializeField] private string newRunScenePath = "Assets/Scenes/NewScene/NewRunScene.unity";
    [SerializeField] private string defaultMonsterId = "eve";

    private string selectedMonsterId;

    /// <summary>Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.</summary>
    private void Awake()
    {
        ResolveSceneReferences();
        BindButtons();
    }

    /// <summary>컴포넌트가 첫 프레임을 처리하기 전에 런타임 초기화를 마친다.</summary>
    private void Start()
    {
        ShowIntro();
    }

    /// <summary><c>SceneReferences</c>를 결정한다.</summary>
    private void ResolveSceneReferences()
    {
        introPanel = ResolveGameObject(introPanel, "Intro");
        mainMenuPanel = ResolveGameObject(mainMenuPanel, "MainMenuUI");
        monsterSelectPanel = ResolveGameObject(monsterSelectPanel, "MosterSelectUI", "MonsterSelectUI");

        introGameStartButton = ResolveButton(introGameStartButton, introPanel, "GameStart");
        runButton = ResolveButton(runButton, mainMenuPanel, "RunBtn");
        monsterSelectGameStartButton = ResolveButton(monsterSelectGameStartButton, monsterSelectPanel, "GameStart");

        arielButton = ResolveButton(arielButton, monsterSelectPanel, "Ariel");
        eveButton = ResolveButton(eveButton, monsterSelectPanel, "Eve");
        seinButton = ResolveButton(seinButton, monsterSelectPanel, "Sein");
        vegaButton = ResolveButton(vegaButton, monsterSelectPanel, "Vega");
        rinButton = ResolveButton(rinButton, monsterSelectPanel, "Rin");
    }

    /// <summary><c>Buttons</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
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

    /// <summary>전달된 런타임 입력값을 사용해 <c>요청값</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    /// <summary><c>Intro</c>를 표시한다.</summary>
    private void ShowIntro()
    {
        SetOnlyActive(introPanel);
    }

    /// <summary><c>MainMenu</c>를 표시한다.</summary>
    private void ShowMainMenu()
    {
        SetOnlyActive(mainMenuPanel);
    }

    /// <summary><c>MonsterSelect</c>를 표시한다.</summary>
    private void ShowMonsterSelect()
    {
        SetOnlyActive(monsterSelectPanel);
    }

    /// <summary>전달된 <c>monsterId</c> 값을 사용해 <c>Monster</c>를 선택한다.</summary>
    private void SelectMonster(string monsterId)
    {
        selectedMonsterId = monsterId;
    }

    /// <summary><c>SelectedMonsterRun</c>를 시작한다.</summary>
    private void StartSelectedMonsterRun()
    {
        var monsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? defaultMonsterId : selectedMonsterId;
            StartContext.Prepare(monsterId);

        if (string.IsNullOrWhiteSpace(newRunScenePath))
        {
            Debug.LogError("MainMenuUIManager cannot load NewRunScene because the scene path is empty.");
            return;
        }

        SceneManager.LoadScene(newRunScenePath);
    }

    /// <summary>전달된 <c>activePanel</c> 값을 사용해 <c>OnlyActive</c>를 갱신한다.</summary>
    private void SetOnlyActive(GameObject activePanel)
    {
        SetActive(introPanel, introPanel == activePanel);
        SetActive(mainMenuPanel, mainMenuPanel == activePanel);
        SetActive(monsterSelectPanel, monsterSelectPanel == activePanel);
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>Active</c>를 갱신한다.</summary>
    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>GameObject</c>를 결정한다.</summary>
    private static GameObject ResolveGameObject(GameObject current, params string[] names)
    {
        if (current != null)
        {
            return current;
        }

        for (var i = 0; i < names.Length; i++)
        {
            var found = FindSceneGameObject(names[i]);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>Button</c>를 결정한다.</summary>
    private static Button ResolveButton(Button current, GameObject root, string childName)
    {
        if (current != null)
        {
            return current;
        }

        var target = FindChild(root != null ? root.transform : null, childName);
        if (target == null)
        {
            target = FindSceneGameObject(childName);
        }

        return target != null ? target.GetComponent<Button>() : null;
    }

    /// <summary>전달된 런타임 입력값을 사용해 <c>Child</c>를 찾는다.</summary>
    private static GameObject FindChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        if (root.name == childName)
        {
            return root.gameObject;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindChild(root.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>전달된 <c>objectName</c> 값을 사용해 <c>SceneGameObject</c>를 찾는다.</summary>
    private static GameObject FindSceneGameObject(string objectName)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        var objects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (var i = 0; i < objects.Length; i++)
        {
            var candidate = objects[i];
            if (candidate == null || candidate.name != objectName || !candidate.scene.IsValid())
            {
                continue;
            }

            return candidate;
        }

        return null;
    }
}
