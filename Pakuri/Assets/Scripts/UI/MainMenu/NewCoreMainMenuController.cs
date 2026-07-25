using Pakuri.NewCore.Bootstrap;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/* MainMenu panel 전환과 선택 Monster run 시작 command를 소유한다. */
namespace Pakuri.NewCore.UI.MainMenu
{
    public class NewCoreMainMenuController : MonoBehaviour
    {
        [SerializeField] private GameObject introPanel;
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject monsterSelectPanel;
        [SerializeField] private Button introGameStartButton;
        [SerializeField] private Button runButton;
        [SerializeField] private Button monsterSelectGameStartButton;
        [SerializeField] private Button arielButton;
        [SerializeField] private Button eveButton;
        [SerializeField] private Button seinButton;
        [SerializeField] private Button vegaButton;
        [SerializeField] private Button rinButton;
        [SerializeField] private string newRunScenePath =
            "Assets/Scenes/NewScene/NewRunScene.unity";
        [SerializeField] private string defaultMonsterId = "eve";

        private string selectedMonsterId;

        /* MainMenu 참조와 button command를 연결하고 Intro panel을 연다. */
        private void Awake()
        {
            ResolveReferences();
            Bind(introGameStartButton, ShowMainMenu);
            Bind(runButton, ShowMonsterSelect);
            Bind(monsterSelectGameStartButton, StartRun);
            Bind(arielButton, () => selectedMonsterId = "ariel");
            Bind(eveButton, () => selectedMonsterId = "eve");
            Bind(seinButton, () => selectedMonsterId = "sein");
            Bind(vegaButton, () => selectedMonsterId = "vega");
            Bind(rinButton, () => selectedMonsterId = "rin");
            ShowOnly(introPanel);
        }

        /* 선택 Monster id를 run 시작 상태에 기록하고 NewRun scene을 연다. */
        private void StartRun()
        {
            string monsterId = selectedMonsterId;
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                monsterId = defaultMonsterId;
            }

            GameBootstrap.PrepareRun(monsterId);
            SceneManager.LoadScene(newRunScenePath);
        }

        /* MainMenu panel만 보이도록 전환한다. */
        private void ShowMainMenu()
        {
            ShowOnly(mainMenuPanel);
        }

        /* Monster 선택 panel만 보이도록 전환한다. */
        private void ShowMonsterSelect()
        {
            ShowOnly(monsterSelectPanel);
        }

        /* 지정 panel만 활성화하고 나머지 MainMenu panel은 숨긴다. */
        private void ShowOnly(GameObject active)
        {
            SetActive(introPanel, active == introPanel);
            SetActive(mainMenuPanel, active == mainMenuPanel);
            SetActive(monsterSelectPanel, active == monsterSelectPanel);
        }

        /* authored field가 비어 있으면 기존 scene hierarchy에서 UI 참조를 찾는다. */
        private void ResolveReferences()
        {
            introPanel = ResolvePanel(introPanel, "Intro");
            mainMenuPanel = ResolvePanel(mainMenuPanel, "MainMenuUI");
            monsterSelectPanel = ResolvePanel(
                monsterSelectPanel,
                "MosterSelectUI",
                "MonsterSelectUI");
            introGameStartButton = ResolveButton(
                introGameStartButton,
                introPanel,
                "GameStart");
            runButton = ResolveButton(runButton, mainMenuPanel, "RunBtn");
            monsterSelectGameStartButton = ResolveButton(
                monsterSelectGameStartButton,
                monsterSelectPanel,
                "GameStart");
            arielButton = ResolveButton(arielButton, monsterSelectPanel, "Ariel");
            eveButton = ResolveButton(eveButton, monsterSelectPanel, "Eve");
            seinButton = ResolveButton(seinButton, monsterSelectPanel, "Sein");
            vegaButton = ResolveButton(vegaButton, monsterSelectPanel, "Vega");
            rinButton = ResolveButton(rinButton, monsterSelectPanel, "Rin");
        }

        /* 현재 참조 또는 후보 이름으로 MainMenu panel을 찾는다. */
        private static GameObject ResolvePanel(
            GameObject current,
            params string[] names)
        {
            if (current != null)
            {
                return current;
            }

            for (var index = 0; index < names.Length; index++)
            {
                var found = GameObject.Find(names[index]);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /* 현재 참조 또는 panel 자식 이름으로 Button을 찾는다. */
        private static Button ResolveButton(
            Button current,
            GameObject root,
            string childName)
        {
            if (current != null)
            {
                return current;
            }

            Transform rootTransform = null;
            if (root != null)
            {
                rootTransform = root.transform;
            }

            var child = FindChild(rootTransform, childName);
            if (child == null)
            {
                return null;
            }

            return child.GetComponent<Button>();
        }

        /* 지정 이름의 Transform을 hierarchy에서 재귀 탐색한다. */
        private static Transform FindChild(
            Transform root,
            string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (var index = 0; index < root.childCount; index++)
            {
                var found = FindChild(root.GetChild(index), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /* 존재하는 Button에 command listener를 연결한다. */
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

        /* 존재하는 GameObject의 활성 상태를 바꾼다. */
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
