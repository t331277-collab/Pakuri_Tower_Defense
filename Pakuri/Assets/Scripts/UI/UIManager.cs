using Pakuri.InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
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

    private void Awake()
    {
        ResolveSceneReferences();
        BindButtons();
    }

    private void Start()
    {
        ShowIntro();
    }

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

    private void ShowIntro()
    {
        SetOnlyActive(introPanel);
    }

    private void ShowMainMenu()
    {
        SetOnlyActive(mainMenuPanel);
    }

    private void ShowMonsterSelect()
    {
        SetOnlyActive(monsterSelectPanel);
    }

    private void SelectMonster(string monsterId)
    {
        selectedMonsterId = monsterId;
    }

    private void StartSelectedMonsterRun()
    {
        var monsterId = string.IsNullOrWhiteSpace(selectedMonsterId) ? defaultMonsterId : selectedMonsterId;
            StartContext.Prepare(monsterId);

        if (string.IsNullOrWhiteSpace(newRunScenePath))
        {
            Debug.LogError("UIManager cannot load NewRunScene because the scene path is empty.");
            return;
        }

        SceneManager.LoadScene(newRunScenePath);
    }

    private void SetOnlyActive(GameObject activePanel)
    {
        SetActive(introPanel, introPanel == activePanel);
        SetActive(mainMenuPanel, mainMenuPanel == activePanel);
        SetActive(monsterSelectPanel, monsterSelectPanel == activePanel);
    }

    private static void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

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
