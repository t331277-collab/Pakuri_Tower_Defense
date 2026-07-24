using Pakuri.NewCore.Presentation.Assets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreMainMenuController : MonoBehaviour
    {
        private const string SelectionResourcePath =
            "Pakuri/NewCore/RunStartSelection";

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
        private RunStartSelectionAsset selection;

        private void Awake()
        {
            ResolveReferences();
            selection = Resources.Load<RunStartSelectionAsset>(
                SelectionResourcePath);
            if (selection == null)
            {
                throw new System.InvalidOperationException(
                    $"Run selection asset '{SelectionResourcePath}' is missing.");
            }

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

        private void StartRun()
        {
            selection.Prepare(
                string.IsNullOrWhiteSpace(selectedMonsterId)
                    ? defaultMonsterId
                    : selectedMonsterId);
            SceneManager.LoadScene(newRunScenePath);
        }

        private void ShowMainMenu()
        {
            ShowOnly(mainMenuPanel);
        }

        private void ShowMonsterSelect()
        {
            ShowOnly(monsterSelectPanel);
        }

        private void ShowOnly(GameObject active)
        {
            SetActive(introPanel, active == introPanel);
            SetActive(mainMenuPanel, active == mainMenuPanel);
            SetActive(monsterSelectPanel, active == monsterSelectPanel);
        }

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

        private static Button ResolveButton(
            Button current,
            GameObject root,
            string childName)
        {
            if (current != null)
            {
                return current;
            }

            var child = FindChild(
                root != null ? root.transform : null,
                childName);
            return child != null ? child.GetComponent<Button>() : null;
        }

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

        private static void Bind(
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
    }
}
