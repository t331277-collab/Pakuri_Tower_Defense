using Pakuri.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Pakuri.Run
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class MainMenuFlowController : MonoBehaviour
    {
        [SerializeField] private GameDataCatalog gameDataCatalog;
        [SerializeField] private string runSceneName = "RunScene";

        private Canvas rootCanvas;
        private CanvasScaler canvasScaler;
        private GraphicRaycaster graphicRaycaster;
        private Font uiFont;

        private GameObject touchToStartPanel;
        private GameObject runMenuPanel;
        private GameObject characterSelectPanel;
        private GameObject monsterButtonRoot;

        private Button touchToStartButton;
        private Button runButton;
        private Button backButton;

        private void OnEnable()
        {
            InitializeUi();

            if (!Application.isPlaying)
            {
                ShowAllPanelsForEditing();
            }
        }

        private void Awake()
        {
            InitializeUi();
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                ShowTouchToStart();
            }
        }

        private void InitializeUi()
        {
            ResolveReferences();
            EnsureCanvasShell();
            EnsureEventSystem();
            BuildUiScaffold();
            BindStaticButtons();
            EnsureMonsterButtons();
        }

        private void ResolveReferences()
        {
            if (Application.isPlaying)
            {
                gameDataCatalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(gameDataCatalog);
            }

            rootCanvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private void EnsureCanvasShell()
        {
            if (rootCanvas == null)
            {
                rootCanvas = gameObject.AddComponent<Canvas>();
            }

            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 20;

            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private void BuildUiScaffold()
        {
            touchToStartPanel = EnsurePanel(
                "TouchToStartPanel",
                new Vector2(0.5f, 0.5f),
                new Vector2(620f, 360f),
                new Color(0.05f, 0.07f, 0.1f, 0.94f));
            EnsureVerticalLayout(touchToStartPanel.GetComponent<RectTransform>(), 34f, 34f, 18f, false);
            EnsureText(touchToStartPanel.transform, "Title", "Pakuri", 46, TextAnchor.MiddleCenter);
            EnsureText(touchToStartPanel.transform, "Summary", "Touch To Start", 24, TextAnchor.MiddleCenter);
            touchToStartButton = EnsureButton(touchToStartPanel.transform, "TouchToStartButton", "Touch To Start");

            runMenuPanel = EnsurePanel(
                "RunMenuPanel",
                new Vector2(0.5f, 0.5f),
                new Vector2(620f, 420f),
                new Color(0.06f, 0.08f, 0.12f, 0.94f));
            EnsureVerticalLayout(runMenuPanel.GetComponent<RectTransform>(), 34f, 34f, 18f, false);
            EnsureText(runMenuPanel.transform, "Title", "Main Menu", 38, TextAnchor.MiddleCenter);
            EnsureText(runMenuPanel.transform, "Summary", "Start a run.", 20, TextAnchor.MiddleCenter);
            runButton = EnsureButton(runMenuPanel.transform, "RunButton", "Run");

            characterSelectPanel = EnsurePanel(
                "CharacterSelectPanel",
                new Vector2(0.5f, 0.5f),
                new Vector2(780f, 760f),
                new Color(0.06f, 0.08f, 0.12f, 0.94f));
            EnsureVerticalLayout(characterSelectPanel.GetComponent<RectTransform>(), 34f, 34f, 16f, false);
            EnsureText(characterSelectPanel.transform, "Title", "Character Select", 38, TextAnchor.MiddleCenter);
            EnsureText(characterSelectPanel.transform, "Summary", "Choose a character before entering RunScene.", 20, TextAnchor.MiddleCenter);

            monsterButtonRoot = EnsureChild(characterSelectPanel.transform, "MonsterButtons", out _);
            EnsureVerticalLayout(monsterButtonRoot.GetComponent<RectTransform>(), 0f, 0f, 12f, true);
            backButton = EnsureButton(characterSelectPanel.transform, "BackButton", "Back");
        }

        private void BindStaticButtons()
        {
            BindButton(touchToStartButton, ShowRunMenu);
            BindButton(runButton, ShowCharacterSelect);
            BindButton(backButton, ShowRunMenu);
        }

        private void EnsureMonsterButtons()
        {
            if (monsterButtonRoot == null)
            {
                return;
            }

            var monsters = PakuriDataManager.Instance.GetMonsters(gameDataCatalog);
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null)
                {
                    continue;
                }

                var captured = monster;
                var button = EnsureButton(
                    monsterButtonRoot.transform,
                    $"MonsterButton_{monster.MonsterId}",
                    $"{monster.DisplayName}\n{monster.RoleSummary}\nA: {monster.ActiveSkillName} / F: {monster.PassiveSkillName}");
                BindButton(button, () => StartRun(captured));
            }
        }

        private void ShowTouchToStart()
        {
            SetPanelVisibility(true, false, false);
        }

        private void ShowRunMenu()
        {
            SetPanelVisibility(false, true, false);
        }

        private void ShowCharacterSelect()
        {
            SetPanelVisibility(false, false, true);
        }

        private void ShowAllPanelsForEditing()
        {
            SetPanelVisibility(true, true, true);
        }

        private void SetPanelVisibility(bool showTouchToStart, bool showRunMenu, bool showCharacterSelect)
        {
            if (touchToStartPanel != null)
            {
                touchToStartPanel.SetActive(showTouchToStart);
            }

            if (runMenuPanel != null)
            {
                runMenuPanel.SetActive(showRunMenu);
            }

            if (characterSelectPanel != null)
            {
                characterSelectPanel.SetActive(showCharacterSelect);
            }
        }

        private void StartRun(MonsterDefinition selectedMonster)
        {
            if (selectedMonster == null || string.IsNullOrWhiteSpace(runSceneName))
            {
                return;
            }

            RunStartContext.Ensure().PrepareNewRun(selectedMonster);
            SceneManager.LoadScene(runSceneName);
        }

        private GameObject EnsurePanel(string name, Vector2 centerAnchor, Vector2 size, Color color)
        {
            var panelObject = EnsureChild(transform, name, out var objectCreated);
            var rect = panelObject.GetComponent<RectTransform>();
            if (objectCreated)
            {
                rect.anchorMin = centerAnchor;
                rect.anchorMax = centerAnchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
            }

            var image = panelObject.GetComponent<Image>();
            if (image == null)
            {
                image = panelObject.AddComponent<Image>();
                image.color = color;
            }

            return panelObject;
        }

        private Text EnsureText(Transform parent, string name, string content, int fontSize, TextAnchor anchor)
        {
            var textObject = EnsureChild(parent, name, out var objectCreated);
            var rect = textObject.GetComponent<RectTransform>();
            if (objectCreated)
            {
                rect.sizeDelta = new Vector2(0f, fontSize * 3f);
            }

            var text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
                text.font = uiFont;
                text.fontSize = fontSize;
                text.alignment = anchor;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.color = Color.white;
                text.text = content;
            }

            return text;
        }

        private Button EnsureButton(Transform parent, string name, string label)
        {
            var buttonObject = EnsureChild(parent, name, out var objectCreated);
            var rect = buttonObject.GetComponent<RectTransform>();
            if (objectCreated)
            {
                rect.sizeDelta = new Vector2(0f, 92f);
            }

            var image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
                image.color = new Color(0.18f, 0.25f, 0.37f, 0.96f);
            }

            var button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            var labelObject = EnsureChild(buttonObject.transform, "Label", out var labelCreated);
            var labelRect = labelObject.GetComponent<RectTransform>();
            if (labelCreated)
            {
                labelRect.anchorMin = Vector2.zero;
                labelRect.anchorMax = Vector2.one;
                labelRect.offsetMin = new Vector2(18f, 12f);
                labelRect.offsetMax = new Vector2(-18f, -12f);
            }

            var text = labelObject.GetComponent<Text>();
            if (text == null)
            {
                text = labelObject.AddComponent<Text>();
                text.font = uiFont;
                text.fontSize = 18;
                text.alignment = TextAnchor.MiddleLeft;
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                text.color = Color.white;
                text.text = label;
            }

            return button;
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static GameObject EnsureChild(Transform parent, string name, out bool created)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                created = false;
                return child.gameObject;
            }

            created = true;
            var childObject = new GameObject(name, typeof(RectTransform));
            childObject.transform.SetParent(parent, false);
            return childObject;
        }

        private static void EnsureVerticalLayout(
            RectTransform rectTransform,
            float leftRightPadding,
            float topBottomPadding,
            float spacing,
            bool preferredHeight)
        {
            var layout = rectTransform.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                DestroyLayoutComponent(layout);
            }

            var fitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                DestroyLayoutComponent(fitter);
            }
        }

        private static void DestroyLayoutComponent(Component component)
        {
            if (Application.isPlaying)
            {
                Destroy(component);
            }
            else
            {
                DestroyImmediate(component);
            }
        }
    }
}
