using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Pakuri.Run
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class RunCombatUiController : MonoBehaviour
    {
        private const int RewardButtonSlotCount = 3;
        private const float RewardButtonWidth = 620f;
        private const float RewardButtonHeight = 96f;
        private const float RewardButtonSpacing = 16f;

        [SerializeField] private EveVerticalSliceController combatController;
        [SerializeField] private GameDataCatalog fallbackCatalog;

        private readonly List<Button> rewardButtons = new List<Button>();

        private Canvas rootCanvas;
        private CanvasScaler canvasScaler;
        private GraphicRaycaster graphicRaycaster;
        private Font uiFont;

        private GameObject hudPanel;
        private Text hudText;

        private GameObject rewardPanel;
        private Text rewardTitleText;
        private Text rewardSummaryText;
        private GameObject rewardButtonRoot;
        private Button rewardContinueButton;

        private GameObject defeatPanel;
        private Text defeatSummaryText;

        private RunSession currentSession;
        private bool rewardSummaryApplied;
        private bool rewardChoiceCommitted;
        private bool rewardPanelEntered;
        private bool defeatPanelEntered;

        private void OnEnable()
        {
            var hadExistingUi = transform.Find("HudPanel") != null
                && transform.Find("RewardPanel") != null
                && transform.Find("DefeatPanel") != null;
            InitializeUi();

            if (Application.isPlaying)
            {
                return;
            }

            if (hadExistingUi)
            {
                ShowEditorUiForEditing();
            }
            else
            {
                ShowEditorPreview();
            }
        }

        private void Awake()
        {
            InitializeUi();
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveRuntimeReferences();
            ShowRuntimeHudOnly();
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ResolveRuntimeReferences();
            RefreshHud();

            if (combatController == null || currentSession == null || !combatController.IsBattleResolved)
            {
                return;
            }

            if (combatController.IsVictory)
            {
                EnterRewardState();
            }
            else
            {
                EnterDefeatState();
            }
        }

        private void InitializeUi()
        {
            ResolveReferences();
            EnsureCanvasShell();
            EnsureEventSystem();

            if (transform.Find("HudPanel") != null
                && transform.Find("RewardPanel") != null
                && transform.Find("DefeatPanel") != null)
            {
                CacheUiReferences();
            }
            else
            {
                BuildUiScaffold();
            }
        }

        private void CacheUiReferences()
        {
            hudPanel = transform.Find("HudPanel")?.gameObject;
            hudText = hudPanel != null ? hudPanel.transform.Find("HudText")?.GetComponent<Text>() : null;

            rewardPanel = transform.Find("RewardPanel")?.gameObject;
            if (rewardPanel != null)
            {
                rewardTitleText = rewardPanel.transform.Find("Title")?.GetComponent<Text>();
                rewardSummaryText = rewardPanel.transform.Find("Summary")?.GetComponent<Text>();
                rewardButtonRoot = rewardPanel.transform.Find("RewardButtons")?.gameObject;
                rewardContinueButton = EnsureButton(rewardPanel.transform, "ContinueButton", "다음 일차 진행", OnContinueAfterReward);
                EnsureVerticalLayout(rewardPanel.GetComponent<RectTransform>(), 0f, 0f, 0f);
                if (rewardButtonRoot != null)
                {
                    EnsureVerticalLayout(rewardButtonRoot.GetComponent<RectTransform>(), 0f, 0f, 0f);
                    EnsureRewardButtonSlots(false);
                }
            }

            defeatPanel = transform.Find("DefeatPanel")?.gameObject;
            defeatSummaryText = defeatPanel != null ? defeatPanel.transform.Find("Summary")?.GetComponent<Text>() : null;
            if (defeatPanel != null)
            {
                EnsureVerticalLayout(defeatPanel.GetComponent<RectTransform>(), 0f, 0f, 0f);
            }
        }

        private void ResolveReferences()
        {
            rootCanvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (combatController == null)
            {
                combatController = FindFirstObjectByType<EveVerticalSliceController>();
            }
        }

        private void ResolveRuntimeReferences()
        {
            if (combatController == null)
            {
                combatController = FindFirstObjectByType<EveVerticalSliceController>();
            }

            if (currentSession == null)
            {
                currentSession = RunSceneBootstrap.ActiveSession;
            }
        }

        private void EnsureCanvasShell()
        {
            if (rootCanvas == null)
            {
                rootCanvas = gameObject.AddComponent<Canvas>();
            }

            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 40;

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
            hudPanel = EnsurePanel("HudPanel", new Color(0.06f, 0.08f, 0.12f, 0.82f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(500f, 270f), new Vector2(18f, -18f));
            hudText = EnsureText(hudPanel.transform, "HudText", string.Empty, 18, TextAnchor.UpperLeft);

            rewardPanel = EnsurePanel("RewardPanel", new Color(0.10f, 0.11f, 0.16f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 760f), Vector2.zero);
            rewardTitleText = EnsureText(rewardPanel.transform, "Title", "Reward", 32, TextAnchor.MiddleCenter);
            rewardSummaryText = EnsureText(rewardPanel.transform, "Summary", string.Empty, 18, TextAnchor.UpperLeft);
            rewardButtonRoot = EnsureChild(rewardPanel.transform, "RewardButtons");
            rewardContinueButton = EnsureButton(rewardPanel.transform, "ContinueButton", "다음 일차 진행", OnContinueAfterReward);
            EnsureVerticalLayout(rewardPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);
            EnsureVerticalLayout(rewardButtonRoot.GetComponent<RectTransform>(), 0f, 0f, 12f);
            EnsureRewardButtonSlots(false);

            defeatPanel = EnsurePanel("DefeatPanel", new Color(0.14f, 0.05f, 0.06f, 0.94f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(560f, 300f), new Vector2(-320f, 180f));
            var defeatTitle = EnsureText(defeatPanel.transform, "Title", "Defeat", 32, TextAnchor.MiddleCenter);
            defeatSummaryText = EnsureText(defeatPanel.transform, "Summary", string.Empty, 18, TextAnchor.MiddleCenter);
            EnsureVerticalLayout(defeatPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);
            defeatTitle.color = Color.white;
        }

        private void ShowEditorPreview()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(true);
            defeatPanel.SetActive(true);

            hudText.text =
                "HUD Preview\n" +
                "Stage 1 / Day 1\n" +
                "Tower HP 500/500\n" +
                "Unit HP 220/220\n" +
                "Magazine 6/6\n" +
                "Reload remaining: 0.00s";
            rewardTitleText.text = "Reward Preview";
            rewardSummaryText.text =
                "Stage clear reward panel preview.\n" +
                "Play mode에서는 전투 클리어 후 실제 보상 후보가 표시된다.";
            defeatSummaryText.text = "Defeat panel preview.";

            EnsureRewardButtonSlots(true);
            rewardContinueButton.gameObject.SetActive(true);
        }

        private void ShowEditorUiForEditing()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(true);
            defeatPanel.SetActive(true);

            EnsureRewardButtonSlots(true);
            rewardContinueButton.gameObject.SetActive(true);
        }

        private void ShowRuntimeHudOnly()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(false);
            defeatPanel.SetActive(false);
            rewardSummaryApplied = false;
            rewardChoiceCommitted = false;
            rewardPanelEntered = false;
            defeatPanelEntered = false;
        }

        private void EnterRewardState()
        {
            if (currentSession == null || combatController == null)
            {
                return;
            }

            if (!rewardSummaryApplied)
            {
                currentSession.ApplyPostCombatSummary(
                    combatController.RewardGold,
                    combatController.RewardDarkTrace,
                    combatController.RewardPrisonerCount);
                rewardSummaryApplied = true;
            }

            if (!rewardPanelEntered)
            {
                rewardPanelEntered = true;
                rewardChoiceCommitted = false;
                RebuildRewardButtons();
            }

            rewardPanel.SetActive(true);
            defeatPanel.SetActive(false);
            rewardTitleText.text = $"{currentSession.SelectedMonsterName} 보상 선택";
            rewardSummaryText.text =
                $"일차 {currentSession.DayIndex} / 전투 {combatController.EncounterLabel}\n" +
                $"골드 +{combatController.RewardGold} / 흔적 +{combatController.RewardDarkTrace}\n" +
                $"포로 표시 {combatController.RewardPrisonerCount}명 / 보스 포로 {combatController.GuaranteedPrisonerName}\n" +
                $"현재 구현된 후보만 3개 이하로 노출된다.";
            rewardContinueButton.gameObject.SetActive(!combatController.IsWaitingForRewardChoice || rewardChoiceCommitted);
        }

        private void RebuildRewardButtons()
        {
            rewardButtons.Clear();

            if (combatController == null)
            {
                return;
            }

            if (rewardButtonRoot == null)
            {
                return;
            }

            EnsureRewardButtonSlots(false);
            var rewardCount = combatController.GetRewardChoiceCount();
            for (var i = 0; i < RewardButtonSlotCount; i++)
            {
                var button = rewardButtonRoot.transform.Find($"RewardButton_{i}")?.GetComponent<Button>();
                if (button == null)
                {
                    continue;
                }

                var hasReward = i < rewardCount;
                button.gameObject.SetActive(hasReward);
                button.interactable = true;
                button.onClick.RemoveAllListeners();

                if (!hasReward)
                {
                    SetButtonLabel(button, string.Empty);
                    continue;
                }

                var index = i;
                var rewardView = combatController.GetRewardChoiceView(index);
                SetButtonLabel(button, $"{rewardView.Title}\n{rewardView.Description}");
                button.onClick.AddListener(() => CommitRewardChoice(index));
                rewardButtons.Add(button);
            }
        }

        private void CommitRewardChoice(int rewardIndex)
        {
            if (currentSession == null || combatController == null || rewardChoiceCommitted)
            {
                return;
            }

            var rewardId = combatController.ApplyRewardChoice(rewardIndex);
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return;
            }

            rewardChoiceCommitted = true;
            currentSession.RecordRewardChoice(
                rewardId,
                combatController.LastAppliedRewardUnlockedPassive ? combatController.SelectedMonsterPassiveName : string.Empty);
            currentSession.AccumulateReward(
                combatController.LastAppliedDamageMultiplier,
                combatController.LastAppliedMagazineBonus,
                combatController.LastAppliedShotIntervalMultiplier,
                combatController.LastAppliedReloadDurationMultiplier,
                combatController.LastAppliedMaxHealthBonus,
                combatController.LastAppliedStatusChanceBonus);
            rewardSummaryText.text += $"\n\n선택 완료: {combatController.AppliedRewardSummary}";
            rewardContinueButton.gameObject.SetActive(true);
            SetButtonsInteractable(rewardButtons, false);
        }

        private void OnContinueAfterReward()
        {
            if (currentSession == null || combatController == null)
            {
                return;
            }

            currentSession.AdvanceDay();
            rewardSummaryApplied = false;
            rewardChoiceCommitted = false;
            rewardPanelEntered = false;
            rewardPanel.SetActive(false);
            combatController.BeginConfiguredDay(
                RunSceneBootstrap.ActiveMonster ?? ResolveFallbackMonster(),
                currentSession);
            RefreshHud();
        }

        private void EnterDefeatState()
        {
            if (defeatPanelEntered)
            {
                return;
            }

            defeatPanelEntered = true;
            defeatPanel.SetActive(true);
            rewardPanel.SetActive(false);
            defeatSummaryText.text = currentSession == null
                ? "Nexus가 붕괴했다."
                : $"{currentSession.SelectedMonsterName} run이 일차 {currentSession.DayIndex}에서 실패했다.";
        }

        private MonsterDefinition ResolveFallbackMonster()
        {
            if (fallbackCatalog == null || fallbackCatalog.Monsters == null || fallbackCatalog.Monsters.Length == 0)
            {
                return null;
            }

            return fallbackCatalog.GetMonsterById(RunSceneBootstrap.FallbackMonsterId) ?? fallbackCatalog.Monsters[0];
        }

        private void RefreshHud()
        {
            if (hudText == null)
            {
                return;
            }

            if (currentSession == null || combatController == null || !combatController.HasActiveRun)
            {
                hudText.text = "Run combat is waiting for a session.";
                return;
            }

            var reloadOrCadence = combatController.ReloadRemaining > 0f
                ? $"재장전 남은 시간: {combatController.ReloadRemaining:0.00}s"
                : $"발사 간격: {combatController.ShotInterval:0.00}s";

            hudText.text =
                $"{currentSession.SelectedMonsterName} / {currentSession.ActiveSkillName}\n" +
                $"스테이지 {currentSession.StageIndex} / 일차 {currentSession.DayIndex}\n" +
                $"전투: {combatController.EncounterLabel}\n" +
                $"타워 HP {combatController.NexusCurrentHealth:0}/{combatController.NexusMaxHealth:0}\n" +
                $"캐릭터 HP {combatController.UnitCurrentHealth:0}/{combatController.UnitMaxHealth:0}\n" +
                $"탄창 {combatController.CurrentShotsRemaining}/{combatController.MagazineCapacity}\n" +
                $"{reloadOrCadence}\n" +
                $"골드 {currentSession.Gold} / 흔적 {currentSession.DarkTrace}\n" +
                $"{combatController.StatusLabel}";
        }

        private GameObject EnsurePanel(string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            var panel = EnsureChild(transform, name, out var panelCreated);
            var rect = panel.GetComponent<RectTransform>();
            if (panelCreated)
            {
                rect.anchorMin = anchorMin;
                rect.anchorMax = anchorMax;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = sizeDelta;
                rect.anchoredPosition = anchoredPosition;
            }

            var image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = panel.AddComponent<Image>();
                image.color = color;
            }

            return panel;
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

        private Button EnsureButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick, bool overwriteLabel = false)
        {
            var buttonObject = EnsureChild(parent, name, out var objectCreated);
            var rect = buttonObject.GetComponent<RectTransform>();
            if (objectCreated)
            {
                rect.sizeDelta = new Vector2(RewardButtonWidth, RewardButtonHeight);
            }
            else if (rect.sizeDelta.x < 1f || rect.sizeDelta.y < 1f)
            {
                rect.sizeDelta = new Vector2(Mathf.Max(rect.sizeDelta.x, RewardButtonWidth), Mathf.Max(rect.sizeDelta.y, RewardButtonHeight));
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

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
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
            }

            if (overwriteLabel || labelCreated || string.IsNullOrWhiteSpace(text.text))
            {
                text.text = label;
            }

            return button;
        }

        private void EnsureRewardButtonSlots(bool showPreviewLabels)
        {
            if (rewardButtonRoot == null)
            {
                return;
            }

            var rootRect = rewardButtonRoot.GetComponent<RectTransform>();
            if (rootRect != null && rootRect.sizeDelta.y < 1f)
            {
                rootRect.sizeDelta = new Vector2(Mathf.Max(rootRect.sizeDelta.x, RewardButtonWidth), (RewardButtonHeight * RewardButtonSlotCount) + (RewardButtonSpacing * (RewardButtonSlotCount - 1)));
            }

            for (var i = 0; i < RewardButtonSlotCount; i++)
            {
                var slotName = $"RewardButton_{i}";
                var existingSlot = rewardButtonRoot.transform.Find(slotName);
                var button = EnsureButton(
                    rewardButtonRoot.transform,
                    slotName,
                    showPreviewLabels ? $"Reward Slot {i + 1}" : string.Empty,
                    null);
                var rect = button.GetComponent<RectTransform>();
                var shouldApplyDefaultTransform = existingSlot == null || (rect != null && (rect.sizeDelta.x < 1f || rect.sizeDelta.y < 1f));
                if (rect != null && shouldApplyDefaultTransform)
                {
                    rect.sizeDelta = new Vector2(RewardButtonWidth, RewardButtonHeight);
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -i * (RewardButtonHeight + RewardButtonSpacing));
                }

                button.gameObject.SetActive(showPreviewLabels);
            }

            for (var i = 0; i < rewardButtonRoot.transform.childCount; i++)
            {
                var child = rewardButtonRoot.transform.GetChild(i);
                if (child == null || child.GetComponent<Button>() == null || IsRewardButtonSlotName(child.name))
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private static void SetButtonLabel(Button button, string label)
        {
            var text = button != null ? button.transform.Find("Label")?.GetComponent<Text>() : null;
            if (text != null)
            {
                text.text = label;
            }
        }

        private static bool IsRewardButtonSlotName(string objectName)
        {
            return objectName == "RewardButton_0"
                || objectName == "RewardButton_1"
                || objectName == "RewardButton_2";
        }

        private GameObject EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var childObject = new GameObject(name, typeof(RectTransform));
            childObject.transform.SetParent(parent, false);
            return childObject;
        }

        private GameObject EnsureChild(Transform parent, string name, out bool created)
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

        private static void EnsureVerticalLayout(RectTransform rectTransform, float leftRightPadding, float topBottomPadding, float spacing)
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

        private static void ClearButtons(List<Button> buttons)
        {
            for (var i = buttons.Count - 1; i >= 0; i--)
            {
                var button = buttons[i];
                if (button != null)
                {
                    if (Application.isPlaying)
                    {
                        button.gameObject.name += "_QueuedForDestroy";
                        Destroy(button.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(button.gameObject);
                    }
                }
            }

            buttons.Clear();
        }

        private static void SetButtonsInteractable(List<Button> buttons, bool interactable)
        {
            for (var i = 0; i < buttons.Count; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].interactable = interactable;
                }
            }
        }
    }
}
