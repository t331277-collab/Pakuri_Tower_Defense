using Pakuri.InGame;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * 인트로, 메인 메뉴와 몬스터 선택 화면을 전환하고 선택한 몬스터로 새 Run을 시작한다.
 * 씬 UI 참조와 버튼 동작을 연결한 뒤 StartContext를 설정하고 NewRunScene을 불러온다.
 */
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

    /*
     * 씬 UI 참조를 확인하고 버튼 동작을 연결한다.
     */
    private void Awake()
    {
        ResolveSceneReferences();
        BindButtons();
    }

    /*
     * 첫 화면으로 인트로 패널을 표시한다.
     */
    private void Start()
    {
        ShowIntro();
    }

    /*
     * Inspector 참조가 비어 있으면 현재 씬의 이름과 계층에서 UI를 찾는다.
     */
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

    /*
     * 메뉴 이동, 몬스터 선택과 게임 시작 동작을 각 버튼에 연결한다.
     */
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

    /*
     * 버튼과 동작이 모두 있을 때 클릭 동작을 등록한다.
     */
    private static void Bind(Button button /* 연결하거나 갱신할 버튼 */, UnityEngine.Events.UnityAction action /* 동작 */)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    /*
     * 인트로 패널만 표시한다.
     */
    private void ShowIntro()
    {
        SetOnlyActive(introPanel);
    }

    /*
     * 메인 메뉴 패널만 표시한다.
     */
    private void ShowMainMenu()
    {
        SetOnlyActive(mainMenuPanel);
    }

    /*
     * 몬스터 선택 패널만 표시한다.
     */
    private void ShowMonsterSelect()
    {
        SetOnlyActive(monsterSelectPanel);
    }

    /*
     * 다음 Run에서 사용할 몬스터 식별자를 저장한다.
     */
    private void SelectMonster(string monsterId /* 몬스터 식별자 */)
    {
        selectedMonsterId = monsterId;
    }

    /*
     * 선택한 몬스터를 시작 정보에 저장하고 NewRunScene으로 이동한다.
     */
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

    /*
     * 지정한 패널만 켜고 나머지 메뉴 패널을 끈다.
     */
    private void SetOnlyActive(GameObject activePanel /* 활성 패널 여부 */)
    {
        SetActive(introPanel, introPanel == activePanel);
        SetActive(mainMenuPanel, mainMenuPanel == activePanel);
        SetActive(monsterSelectPanel, monsterSelectPanel == activePanel);
    }

    /*
     * UI 오브젝트가 있을 때 활성 상태를 변경한다.
     */
    private static void SetActive(GameObject target /* 활성화하거나 변경할 게임 오브젝트 */, bool active /* 대상 활성화 여부 */)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    /*
     * 현재 참조를 사용하거나 전달받은 이름 순서대로 씬 오브젝트를 찾는다.
     */
    private static GameObject ResolveGameObject(GameObject current /* 현재 */, params string[] names /* 이름 목록 */)
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

    /*
     * 현재 버튼을 사용하거나 지정한 UI 아래에서 버튼을 찾는다.
     */
    private static Button ResolveButton(Button current /* 현재 */, GameObject root /* 검색이나 배치의 기준 오브젝트 */, string childName /* 자식 오브젝트 이름 */)
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

    /*
     * 지정한 계층에서 이름이 같은 자식 오브젝트를 재귀적으로 찾는다.
     */
    private static GameObject FindChild(Transform root /* 검색이나 배치의 기준 오브젝트 */, string childName /* 자식 오브젝트 이름 */)
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

    /*
     * 현재 씬에 속한 오브젝트 중 이름이 같은 오브젝트를 찾는다.
     */
    private static GameObject FindSceneGameObject(string objectName /* 게임 오브젝트 이름 */)
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
