using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Pakuri.Run
{
    [DisallowMultipleComponent]
    public class RunFlowController : MonoBehaviour
    {
        [SerializeField] private GameDataCatalog gameDataCatalog;
        [SerializeField] private CombatRuntimeController combatController;

        private Canvas rootCanvas;
        private CanvasScaler canvasScaler;
        private GraphicRaycaster graphicRaycaster;
        private Font uiFont;

        private GameObject frontPanel;
        private GameObject frontButtonRoot;
        private Text frontTitleText;
        private Text frontSummaryText;

        private GameObject hudPanel;
        private Text hudText;

        private GameObject rewardPanel;
        private Text rewardTitleText;
        private Text rewardSummaryText;
        private GameObject rewardButtonRoot;
        private Button rewardContinueButton;

        private GameObject defeatPanel;
        private Text defeatSummaryText;

        private readonly List<Button> monsterButtons = new List<Button>();
        private readonly List<Button> rewardButtons = new List<Button>();

        private RunSession currentSession;
        private RunFlowState currentState;
        private bool rewardSummaryApplied;
        private bool rewardChoiceCommitted;

        private void Awake()
        {
            ResolveReferences();
            EnsureCanvasShell();
            EnsureEventSystem();
            BuildUiScaffold();
        }

        private void Start()
        {
            ShowMonsterSelect();
        }

        private void Update()
        {
            RefreshHud();

            if (combatController == null)
            {
                return;
            }

            if (currentState == RunFlowState.Combat && combatController.IsBattleResolved)
            {
                if (combatController.IsVictory)
                {
                    EnterRewardState();
                }
                else
                {
                    EnterDefeatState();
                }
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
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }
        }

        private void EnsureCanvasShell()
        {
            if (rootCanvas == null)
            {
                rootCanvas = gameObject.AddComponent<Canvas>();
            }

            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.pixelPerfect = false;
            rootCanvas.sortingOrder = 50;

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

        private void EnsureEventSystem()
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
            frontPanel = EnsurePanel("MonsterSelectPanel", new Color(0.08f, 0.10f, 0.16f, 0.92f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(720f, 820f));
            frontTitleText = EnsureText(frontPanel.transform, "Title", "Pakuri Run Prototype", 34, TextAnchor.MiddleCenter);
            frontSummaryText = EnsureText(frontPanel.transform, "Summary", string.Empty, 20, TextAnchor.UpperLeft);
            frontButtonRoot = EnsureChild(frontPanel.transform, "MonsterButtons");
            EnsureVerticalLayout(frontPanel.GetComponent<RectTransform>(), 28f, 28f, 22f);
            EnsureVerticalLayout(frontButtonRoot.GetComponent<RectTransform>(), 0f, 0f, 14f);

            hudPanel = EnsurePanel("HudPanel", new Color(0.06f, 0.08f, 0.12f, 0.82f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(480f, 250f), new Vector2(18f, -18f));
            hudText = EnsureText(hudPanel.transform, "HudText", string.Empty, 18, TextAnchor.UpperLeft);

            rewardPanel = EnsurePanel("RewardPanel", new Color(0.10f, 0.11f, 0.16f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 760f));
            rewardTitleText = EnsureText(rewardPanel.transform, "Title", "Reward", 32, TextAnchor.MiddleCenter);
            rewardSummaryText = EnsureText(rewardPanel.transform, "Summary", string.Empty, 18, TextAnchor.UpperLeft);
            rewardButtonRoot = EnsureChild(rewardPanel.transform, "RewardButtons");
            rewardContinueButton = EnsureButton(rewardPanel.transform, "ContinueButton", "다음 일차 진행", OnContinueAfterReward);
            EnsureVerticalLayout(rewardPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);
            EnsureVerticalLayout(rewardButtonRoot.GetComponent<RectTransform>(), 0f, 0f, 12f);

            defeatPanel = EnsurePanel("DefeatPanel", new Color(0.14f, 0.05f, 0.06f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(560f, 280f));
            var defeatTitle = EnsureText(defeatPanel.transform, "Title", "Defeat", 32, TextAnchor.MiddleCenter);
            defeatSummaryText = EnsureText(defeatPanel.transform, "Summary", string.Empty, 18, TextAnchor.MiddleCenter);
            var retryButton = EnsureButton(defeatPanel.transform, "RetryButton", "현재 일차 재시도", RetryCurrentDay);
            var selectionButton = EnsureButton(defeatPanel.transform, "BackButton", "선택 화면으로", ShowMonsterSelect);
            EnsureVerticalLayout(defeatPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);

            defeatTitle.color = Color.white;
            retryButton.GetComponentInChildren<Text>().color = Color.white;
            selectionButton.GetComponentInChildren<Text>().color = Color.white;
        }

        private void ShowMonsterSelect()
        {
            currentSession = null;
            currentState = RunFlowState.MonsterSelect;
            rewardSummaryApplied = false;
            rewardChoiceCommitted = false;

            if (combatController != null)
            {
                combatController.ResetPrototypeState();
            }

            frontPanel.SetActive(true);
            hudPanel.SetActive(false);
            rewardPanel.SetActive(false);
            defeatPanel.SetActive(false);

            if (gameDataCatalog == null || gameDataCatalog.Monsters == null || gameDataCatalog.Monsters.Length == 0)
            {
                frontTitleText.text = "GameDataCatalog is missing";
                frontSummaryText.text = "Pakuri/Seed Default Game Data를 실행해 기본 데이터를 만든 뒤 다시 시도한다.";
                RebuildMonsterButtons(new MonsterDefinition[0]);
                return;
            }

            frontTitleText.text = "Pakuri Run Prototype";
            frontSummaryText.text = "몬스터를 선택하면 현재 문서 기준의 5몬스터 A 스킬 전투와 A/F 최소 보상 루프가 시작된다.";
            RebuildMonsterButtons(gameDataCatalog.Monsters);
        }

        private void RebuildMonsterButtons(MonsterDefinition[] monsters)
        {
            ClearButtons(monsterButtons);

            if (monsters == null)
            {
                return;
            }

            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null)
                {
                    continue;
                }

                var buttonText = $"{monster.DisplayName}\n{monster.RoleSummary}\n시작 A: {monster.ActiveSkillName} / F: {monster.PassiveSkillName}";
                var captured = monster;
                monsterButtons.Add(EnsureButton(frontButtonRoot.transform, $"MonsterButton_{monster.MonsterId}", buttonText, () => StartRun(captured)));
            }
        }

        private void StartRun(MonsterDefinition selectedMonster)
        {
            if (selectedMonster == null || combatController == null)
            {
                return;
            }

            currentSession = RunSession.Begin(selectedMonster);
            currentState = RunFlowState.Combat;
            rewardSummaryApplied = false;
            rewardChoiceCommitted = false;
            combatController.BeginConfiguredDay(selectedMonster, currentSession, gameDataCatalog);
            frontPanel.SetActive(false);
            hudPanel.SetActive(true);
            rewardPanel.SetActive(false);
            defeatPanel.SetActive(false);
            RefreshHud();
        }

        private void EnterRewardState()
        {
            if (currentSession == null)
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

            currentState = RunFlowState.Reward;
            rewardPanel.SetActive(true);
            defeatPanel.SetActive(false);
            hudPanel.SetActive(true);
            rewardTitleText.text = $"{currentSession.SelectedMonsterName} 보상 선택";
            rewardSummaryText.text =
                $"일차 {currentSession.DayIndex} / 전투 {combatController.EncounterLabel}\n" +
                $"골드 +{combatController.RewardGold} / 흔적 +{combatController.RewardDarkTrace}\n" +
                $"포로 표시 {combatController.RewardPrisonerCount}명 / 보스 포로 {combatController.GuaranteedPrisonerName}\n" +
                $"현재 구현된 후보만 3개 이하로 노출된다.";

            RebuildRewardButtons();
            rewardContinueButton.gameObject.SetActive(!combatController.IsWaitingForRewardChoice);
        }

        private void RebuildRewardButtons()
        {
            ClearButtons(rewardButtons);

            var rewardCount = combatController.GetRewardChoiceCount();
            for (var i = 0; i < rewardCount; i++)
            {
                var index = i;
                var rewardView = combatController.GetRewardChoiceView(index);
                rewardButtons.Add(EnsureButton(
                    rewardButtonRoot.transform,
                    $"RewardButton_{index}",
                    $"{rewardView.Title}\n{rewardView.Description}",
                    () => CommitRewardChoice(index)));
            }
        }

        private void CommitRewardChoice(int rewardIndex)
        {
            if (currentSession == null || rewardChoiceCommitted)
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
            rewardPanel.SetActive(false);
            currentState = RunFlowState.Combat;
            combatController.BeginConfiguredDay(
                gameDataCatalog.GetMonsterById(currentSession.SelectedMonsterId),
                currentSession);
            RefreshHud();
        }

        private void EnterDefeatState()
        {
            currentState = RunFlowState.Defeat;
            defeatPanel.SetActive(true);
            rewardPanel.SetActive(false);
            hudPanel.SetActive(true);
            defeatSummaryText.text = currentSession == null
                ? "Nexus가 붕괴했다."
                : $"{currentSession.SelectedMonsterName} run이 일차 {currentSession.DayIndex}에서 실패했다.\n현재 일차를 재시도하거나 선택 화면으로 돌아간다.";
        }

        private void RetryCurrentDay()
        {
            if (currentSession == null || combatController == null)
            {
                ShowMonsterSelect();
                return;
            }

            rewardSummaryApplied = false;
            rewardChoiceCommitted = false;
            defeatPanel.SetActive(false);
            rewardPanel.SetActive(false);
            currentState = RunFlowState.Combat;
            combatController.BeginConfiguredDay(
                gameDataCatalog.GetMonsterById(currentSession.SelectedMonsterId),
                currentSession);
            RefreshHud();
        }

        private void RefreshHud()
        {
            if (hudText == null)
            {
                return;
            }

            if (currentSession == null || combatController == null || !combatController.HasActiveRun)
            {
                hudText.text = "Run이 아직 시작되지 않았다.";
                return;
            }

            var reloadOrCadence = combatController.ReloadRemaining > 0f
                ? $"재장전: {combatController.ReloadRemaining:0.00}s"
                : $"발사 간격: {combatController.ShotInterval:0.00}s";

            hudText.text =
                $"{currentSession.SelectedMonsterName} / {currentSession.ActiveSkillName}\n" +
                $"스테이지 {currentSession.StageIndex} / 일차 {currentSession.DayIndex}\n" +
                $"전투: {combatController.EncounterLabel}\n" +
                $"넥서스 HP {combatController.NexusCurrentHealth:0}/{combatController.NexusMaxHealth:0}\n" +
                $"유닛 HP {combatController.UnitCurrentHealth:0}/{combatController.UnitMaxHealth:0}\n" +
                $"탄창 {combatController.CurrentShotsRemaining}/{combatController.MagazineCapacity}\n" +
                $"{reloadOrCadence}\n" +
                $"골드 {currentSession.Gold} / 흔적 {currentSession.DarkTrace}\n" +
                $"{combatController.StatusLabel}";
        }

        private GameObject EnsurePanel(string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta)
        {
            return EnsurePanel(name, color, anchorMin, anchorMax, sizeDelta, Vector2.zero);
        }

        private GameObject EnsurePanel(string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            var panel = EnsureChild(transform, name);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            var image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = panel.AddComponent<Image>();
            }

            image.color = color;
            return panel;
        }

        private Text EnsureText(Transform parent, string name, string content, int fontSize, TextAnchor anchor)
        {
            var textObject = EnsureChild(parent, name);
            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, fontSize * 3f);

            var text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
            }

            text.font = uiFont;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.text = content;
            return text;
        }

        private Button EnsureButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = EnsureChild(parent, name);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 88f);

            var image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            image.color = new Color(0.18f, 0.25f, 0.37f, 0.96f);

            var button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);

            var labelObject = EnsureChild(buttonObject.transform, "Label");
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(18f, 12f);
            labelRect.offsetMax = new Vector2(-18f, -12f);

            var text = labelObject.GetComponent<Text>();
            if (text == null)
            {
                text = labelObject.AddComponent<Text>();
            }

            text.font = uiFont;
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.text = label;
            return button;
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

        private static void EnsureVerticalLayout(RectTransform rectTransform, float leftRightPadding, float topBottomPadding, float spacing)
        {
            var layout = rectTransform.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset((int)leftRightPadding, (int)leftRightPadding, (int)topBottomPadding, (int)topBottomPadding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
            }

            var shouldFitToContent = rectTransform.gameObject.name.EndsWith("Buttons");
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = shouldFitToContent
                ? ContentSizeFitter.FitMode.PreferredSize
                : ContentSizeFitter.FitMode.Unconstrained;
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
