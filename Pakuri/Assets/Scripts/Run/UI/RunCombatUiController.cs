using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

namespace Pakuri.Run
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class RunCombatUiController : MonoBehaviour
    {
        private const int RewardButtonTemplateCount = 3;
        private const string PrisonerTemplateName = "Prisoner";
        private const string MaterialTemplateName = "Material";
        private const string ArtifactTemplateName = "Artifact";
        private const float RewardButtonWidth = 620f;
        private const float RewardButtonHeight = 96f;
        private const float RewardButtonSpacing = 16f;
        private const int PrisonerChoiceButtonCount = 4;
        private const int OfferingChoiceButtonCount = 3;
        private const int MaxRunActiveSkillCount = 3;
        private const int MaxRunPassiveSkillCount = 3;
        private const float ManifestSuccessChance = 0.70f;

        [SerializeField] private CombatRuntimeController combatController;
        [SerializeField] private GameDataCatalog fallbackCatalog;

        private readonly List<Button> rewardButtons = new List<Button>();
        private readonly List<OfferingChoiceView> offeringChoices = new List<OfferingChoiceView>();

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

        private GameObject prisonerPanel;
        private GameObject prisonerOfferingPanel;
        private Text prisonerTitleText;
        private readonly Button[] prisonerChoiceButtons = new Button[OfferingChoiceButtonCount];
        private GameObject prisonerChoicePanel;
        private Text prisonerChoiceTitleText;
        private readonly Button[] prisonerModeButtons = new Button[PrisonerChoiceButtonCount];
        private GameObject prisonerSummonerPanel;
        private Text prisonerSummonerTitleText;
        private Text prisonerSummonerSummaryText;
        private Image prisonerSummonerImage;
        private Button prisonerSummonerButton;
        private Button prisonerSummonerContinueButton;
        private Button prisonerSummonerBackButton;
        private GameObject prisonerManifestFailurePopup;
        private Text prisonerManifestFailureText;
        private Button prisonerManifestFailureCloseButton;

        private GameObject defeatPanel;
        private Text defeatSummaryText;
        private CombatMonsterPanelUiController monsterPanelUi;

        private RunSession currentSession;
        private bool rewardSummaryApplied;
        private bool rewardPanelEntered;
        private bool defeatPanelEntered;
        private string rewardDetailText = string.Empty;
        private string activePrisonerName = string.Empty;
        private MonsterDefinition pendingManifestMonster;

        private enum OfferingChoiceKind
        {
            ActiveSkill,
            PassiveSkill,
            Enhancement,
            MasterSkill
        }

        private sealed class OfferingChoiceView
        {
            public string ChoiceId;
            public string MonsterId;
            public string Title;
            public string Description;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public OfferingChoiceKind ChoiceKind;
            public float DamageMultiplier = 1f;
            public int MagazineBonus;
            public float ShotIntervalMultiplier = 1f;
            public float ReloadDurationMultiplier = 1f;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
        }

        private void OnEnable()
        {
            var hadExistingUi = transform.Find("HudPanel") != null
                && transform.Find("RewardPanel") != null
                && transform.Find("PrisonerOfferingPanel") != null
                && transform.Find("DefeatPanel") != null;
            InitializeUi();

            if (Application.isPlaying)
            {
                ShowRuntimeHudOnly();
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
            BindMonsterPanelUi();
            RefreshHud();

            if (combatController == null || currentSession == null || !combatController.IsBattleResolved)
            {
                return;
            }

            if (combatController.IsVictory)
            {
                if (IsRewardModalOpen())
                {
                    return;
                }

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
                && transform.Find("PrisonerOfferingPanel") != null
                && transform.Find("DefeatPanel") != null)
            {
                CacheUiReferences();
            }
            else
            {
                BuildUiScaffold();
            }

            BindMonsterPanelUi();
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

            prisonerOfferingPanel = transform.Find("PrisonerOfferingPanel")?.gameObject;
            prisonerPanel = transform.Find("PrisonerPanel")?.gameObject;
            var offeringPanel = prisonerOfferingPanel != null ? prisonerOfferingPanel : prisonerPanel;
            if (offeringPanel != null)
            {
                prisonerTitleText = offeringPanel.transform.Find("Title")?.GetComponent<Text>();
                for (var i = 0; i < OfferingChoiceButtonCount; i++)
                {
                    prisonerChoiceButtons[i] = offeringPanel.transform.Find($"Choice{i + 1}")?.GetComponent<Button>();
                    if (prisonerChoiceButtons[i] != null)
                    {
                        prisonerChoiceButtons[i].onClick.RemoveAllListeners();
                    }
                }

                EnsureVerticalLayout(offeringPanel.GetComponent<RectTransform>(), 0f, 0f, 0f);
            }

            EnsurePrisonerChoicePanels();

            defeatPanel = transform.Find("DefeatPanel")?.gameObject;
            defeatSummaryText = defeatPanel != null ? defeatPanel.transform.Find("Summary")?.GetComponent<Text>() : null;
            if (defeatPanel != null)
            {
                EnsureVerticalLayout(defeatPanel.GetComponent<RectTransform>(), 0f, 0f, 0f);
            }
        }

        private void ResolveReferences()
        {
            if (Application.isPlaying)
            {
                fallbackCatalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(fallbackCatalog);
            }

            rootCanvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (combatController == null)
            {
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }
        }

        private void ResolveRuntimeReferences()
        {
            fallbackCatalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(fallbackCatalog);

            if (combatController == null)
            {
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }

            if (currentSession == null)
            {
                currentSession = RunSceneBootstrap.ActiveSession;
            }
        }

        private void BindMonsterPanelUi()
        {
            monsterPanelUi = GetComponent<CombatMonsterPanelUiController>();
            if (monsterPanelUi == null && transform.Find("MonsterPanel") != null)
            {
                monsterPanelUi = gameObject.AddComponent<CombatMonsterPanelUiController>();
            }

            if (monsterPanelUi != null)
            {
                monsterPanelUi.Bind(combatController);
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

            prisonerPanel = transform.Find("PrisonerPanel")?.gameObject;
            prisonerOfferingPanel = EnsurePanel("PrisonerOfferingPanel", new Color(0.16f, 0.11f, 0.10f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 520f), Vector2.zero);
            prisonerTitleText = EnsureText(prisonerOfferingPanel.transform, "Title", "포로 공양", 30, TextAnchor.MiddleCenter);
            for (var i = 0; i < OfferingChoiceButtonCount; i++)
            {
                prisonerChoiceButtons[i] = EnsureButton(prisonerOfferingPanel.transform, $"Choice{i + 1}", string.Empty, null);
            }

            EnsureVerticalLayout(prisonerOfferingPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);
            EnsurePrisonerChoicePanels();

            defeatPanel = EnsurePanel("DefeatPanel", new Color(0.14f, 0.05f, 0.06f, 0.94f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(560f, 300f), new Vector2(-320f, 180f));
            var defeatTitle = EnsureText(defeatPanel.transform, "Title", "Defeat", 32, TextAnchor.MiddleCenter);
            defeatSummaryText = EnsureText(defeatPanel.transform, "Summary", string.Empty, 18, TextAnchor.MiddleCenter);
            EnsureVerticalLayout(defeatPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);
            defeatTitle.color = Color.white;
        }

        private void EnsurePrisonerChoicePanels()
        {
            prisonerChoicePanel = EnsurePanel("PrisonerChoicePanel", new Color(0.12f, 0.10f, 0.16f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 540f), Vector2.zero);
            prisonerChoiceTitleText = EnsureText(prisonerChoicePanel.transform, "Title", "Prisoner Choice", 30, TextAnchor.MiddleCenter);
            prisonerModeButtons[0] = EnsureButton(prisonerChoicePanel.transform, "ManifestButton", "Manifest", TryManifestPrisonerMonster);
            prisonerModeButtons[1] = EnsureButton(prisonerChoicePanel.transform, "AssimilateButton", "Assimilate", OnAssimilateClicked);
            prisonerModeButtons[2] = EnsureButton(prisonerChoicePanel.transform, "OfferingButton", "Offering", () => OpenPrisonerOfferingPanel(activePrisonerName));
            prisonerModeButtons[3] = EnsureButton(prisonerChoicePanel.transform, "CorruptButton", "Torture / Corrupt", OnCorruptClicked);
            EnsureVerticalLayout(prisonerChoicePanel.GetComponent<RectTransform>(), 28f, 28f, 18f);

            prisonerSummonerPanel = EnsurePanel("PrisonerSummonerPanel", new Color(0.08f, 0.13f, 0.15f, 0.96f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(820f, 620f), Vector2.zero);
            prisonerSummonerTitleText = EnsureText(prisonerSummonerPanel.transform, "Title", "Manifest", 30, TextAnchor.MiddleCenter);
            var imageObject = EnsureChild(prisonerSummonerPanel.transform, "MonsterImage", out var imageCreated);
            var imageRect = imageObject.GetComponent<RectTransform>();
            if (imageCreated)
            {
                imageRect.sizeDelta = new Vector2(160f, 160f);
            }

            prisonerSummonerImage = imageObject.GetComponent<Image>();
            if (prisonerSummonerImage == null)
            {
                prisonerSummonerImage = imageObject.AddComponent<Image>();
                prisonerSummonerImage.color = Color.white;
                prisonerSummonerImage.preserveAspect = true;
            }

            prisonerSummonerSummaryText = EnsureText(prisonerSummonerPanel.transform, "Summary", string.Empty, 18, TextAnchor.UpperLeft);
            prisonerSummonerButton = EnsureButton(prisonerSummonerPanel.transform, "SummonButton", "Manifest Resolved", ClosePrisonerSummonerResult, true);
            prisonerSummonerContinueButton = EnsureButton(prisonerSummonerPanel.transform, "ContinueButton", "Continue", ClosePrisonerSummonerResult, true);
            prisonerSummonerBackButton = EnsureButton(prisonerSummonerPanel.transform, "BackButton", "Back to Reward", ClosePrisonerSummonerWithoutManifest, true);
            EnsureVerticalLayout(prisonerSummonerPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);

            prisonerManifestFailurePopup = EnsurePanel("PrisonerManifestFailurePopup", new Color(0.15f, 0.06f, 0.07f, 0.97f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(620f, 320f), Vector2.zero);
            EnsureText(prisonerManifestFailurePopup.transform, "Title", "Manifest Failed", 30, TextAnchor.MiddleCenter);
            prisonerManifestFailureText = EnsureText(prisonerManifestFailurePopup.transform, "Summary", string.Empty, 18, TextAnchor.MiddleCenter);
            prisonerManifestFailureCloseButton = EnsureButton(prisonerManifestFailurePopup.transform, "CloseButton", "Return to Reward", ClosePrisonerManifestFailurePopup, true);
            EnsureVerticalLayout(prisonerManifestFailurePopup.GetComponent<RectTransform>(), 28f, 28f, 18f);

            if (!Application.isPlaying)
            {
                ConfigurePrisonerChoicePanelPreview();
                ConfigurePrisonerSummonerPanelPreview();
            }
        }

        private void ShowEditorPreview()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(true);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, true);
            prisonerChoicePanel.SetActive(true);
            prisonerSummonerPanel.SetActive(true);
            prisonerManifestFailurePopup.SetActive(true);
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
            ConfigurePrisonerChoicePanelPreview();
            ConfigurePrisonerSummonerPanelPreview();
            ConfigurePrisonerPanelPreview();
        }

        private void ShowEditorUiForEditing()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(true);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, true);
            prisonerChoicePanel.SetActive(true);
            prisonerSummonerPanel.SetActive(true);
            prisonerManifestFailurePopup.SetActive(true);
            defeatPanel.SetActive(true);

            EnsureRewardButtonSlots(true);
            rewardContinueButton.gameObject.SetActive(true);
            ConfigurePrisonerChoicePanelPreview();
            ConfigurePrisonerSummonerPanelPreview();
            ConfigurePrisonerPanelPreview();
        }

        private void ShowRuntimeHudOnly()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
            prisonerManifestFailurePopup.SetActive(false);
            defeatPanel.SetActive(false);
            SetOptionalPanelActive(transform.Find("MonsterPanel")?.gameObject, true);
            rewardSummaryApplied = false;
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
                rewardSummaryApplied = true;
            }

            if (!rewardPanelEntered)
            {
                rewardPanelEntered = true;
                RebuildRewardButtons();
            }

            rewardPanel.SetActive(true);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
            prisonerManifestFailurePopup.SetActive(false);
            defeatPanel.SetActive(false);
            rewardTitleText.text = $"{currentSession.SelectedMonsterName} 보상 선택";
            rewardSummaryText.text =
                $"일차 {currentSession.DayIndex} / 전투 {combatController.EncounterLabel}\n" +
                $"골드 +{combatController.RewardGold} / 흔적 +{combatController.RewardDarkTrace}\n" +
                $"포로 {combatController.RewardPrisonerCount}명: {combatController.RewardPrisonerSummary}\n" +
                $"보스 포로 확정 기준: {combatController.GuaranteedPrisonerName}\n" +
                $"각 보상 버튼을 눌러 직접 습득한다. 포로 버튼은 공양 선택지를 연다." +
                (string.IsNullOrWhiteSpace(rewardDetailText) ? string.Empty : $"\n\n확인: {rewardDetailText}");
            rewardContinueButton.gameObject.SetActive(true);
        }

        private void RebuildRewardButtons()
        {
            ClearRuntimeRewardButtons();
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
            for (var i = 0; i < rewardCount; i++)
            {
                var index = i;
                var rewardView = combatController.GetRewardChoiceView(index);
                var template = ResolveRewardTemplate(rewardView.RewardKind);
                if (template == null)
                {
                    continue;
                }

                var buttonObject = Instantiate(template.gameObject, rewardButtonRoot.transform, false);
                buttonObject.name = $"RewardItem_{i:00}_{rewardView.RewardKind}";
                var button = buttonObject.GetComponent<Button>();
                if (button == null)
                {
                    Destroy(buttonObject);
                    continue;
                }

                var rect = button.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.sizeDelta = new Vector2(RewardButtonWidth, RewardButtonHeight);
                    rect.anchorMin = new Vector2(0.5f, 1f);
                    rect.anchorMax = new Vector2(0.5f, 1f);
                    rect.pivot = new Vector2(0.5f, 1f);
                    rect.anchoredPosition = new Vector2(0f, -i * (RewardButtonHeight + RewardButtonSpacing));
                }

                buttonObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                SetButtonLabel(button, rewardView.Claimed
                    ? $"{rewardView.Title}\n습득 완료"
                    : $"{rewardView.Title}\n{rewardView.Description}");
                button.onClick.AddListener(() => CommitRewardChoice(index));
                button.interactable = !rewardView.Claimed;
                rewardButtons.Add(button);
            }
        }

        private void CommitRewardChoice(int rewardIndex)
        {
            if (currentSession == null || combatController == null)
            {
                return;
            }

            var rewardView = combatController.GetRewardChoiceView(rewardIndex);
            var rewardId = combatController.ApplyRewardChoice(rewardIndex);
            if (string.IsNullOrWhiteSpace(rewardId))
            {
                return;
            }

            ApplyClaimedRewardToSession(rewardView);
            rewardDetailText = combatController.AppliedRewardSummary;
            if (IsPrisonerReward(rewardView, rewardId))
            {
                OpenPrisonerChoicePanel(rewardView.PrisonerName);
                return;
            }

            RebuildRewardButtons();
            EnterRewardState();
        }

        private void ApplyClaimedRewardToSession(CombatRuntimeController.RewardChoiceView rewardView)
        {
            if (currentSession == null)
            {
                return;
            }

            if (string.Equals(rewardView.RewardKind, "Prisoner", System.StringComparison.OrdinalIgnoreCase))
            {
                currentSession.ClaimPrisonerReward(rewardView.PrisonerName);
                return;
            }

            if (string.Equals(rewardView.RewardKind, "Material", System.StringComparison.OrdinalIgnoreCase))
            {
                currentSession.ClaimMaterialReward(rewardView.GoldAmount, rewardView.DarkTraceAmount);
            }
        }

        private void OpenPrisonerChoicePanel(string prisonerName)
        {
            activePrisonerName = string.IsNullOrWhiteSpace(prisonerName) ? "Unknown Prisoner" : prisonerName;
            pendingManifestMonster = null;
            rewardPanel.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerSummonerPanel.SetActive(false);
            prisonerManifestFailurePopup.SetActive(false);
            prisonerChoicePanel.SetActive(true);

            if (prisonerChoiceTitleText != null)
            {
                prisonerChoiceTitleText.text = $"{activePrisonerName} prisoner choice";
            }

            SetButtonLabel(prisonerModeButtons[0], "Manifest\n70% success. On success, a new monster joins from the next combat.");
            SetButtonLabel(prisonerModeButtons[1], "Assimilate\nNot implemented yet.");
            SetButtonLabel(prisonerModeButtons[2], "Offering\nOpen the existing skill offering choices.");
            SetButtonLabel(prisonerModeButtons[3], "Torture / Corrupt\nNot implemented yet.");
            for (var i = 0; i < prisonerModeButtons.Length; i++)
            {
                if (prisonerModeButtons[i] != null)
                {
                    prisonerModeButtons[i].interactable = true;
                    prisonerModeButtons[i].gameObject.SetActive(true);
                }
            }
        }

        private void OpenPrisonerOfferingPanel(string prisonerName)
        {
            OpenPrisonerPanel(prisonerName);
        }

        private void TryManifestPrisonerMonster()
        {
            if (currentSession == null)
            {
                return;
            }

            pendingManifestMonster = ResolveNextManifestCandidate();
            if (pendingManifestMonster == null)
            {
                ShowManifestFailurePopup("Manifest failed because no candidate monster exists.");
                return;
            }

            var succeeded = UnityEngine.Random.value < ManifestSuccessChance;
            if (!succeeded)
            {
                rewardDetailText = $"Manifest failed for {activePrisonerName}.";
                ShowManifestFailurePopup($"{activePrisonerName} Manifest failed.\nNo monster joined.");
                return;
            }

            prisonerChoicePanel.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerManifestFailurePopup.SetActive(false);
            prisonerSummonerPanel.SetActive(true);
            if (prisonerSummonerButton != null)
            {
                prisonerSummonerButton.gameObject.SetActive(false);
                prisonerSummonerButton.interactable = false;
            }
            if (prisonerSummonerBackButton != null)
            {
                prisonerSummonerBackButton.gameObject.SetActive(false);
            }

            currentSession.RecordManifestedMonster(pendingManifestMonster);
            if (combatController != null)
            {
                combatController.RefreshManifestedMonsterParty(currentSession);
            }

            SetSummonerPanel(pendingManifestMonster, $"Manifest succeeded.\n{pendingManifestMonster.DisplayName} will join combat from the next day.");
            if (prisonerSummonerButton != null)
            {
                prisonerSummonerButton.interactable = false;
            }

            rewardDetailText = $"Manifested {pendingManifestMonster.DisplayName}.";
            ShowPrisonerSummonerContinue();
        }

        private void ShowManifestFailurePopup(string message)
        {
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerManifestFailurePopup.SetActive(true);
            if (prisonerManifestFailureText != null)
            {
                prisonerManifestFailureText.text = message;
            }

            if (prisonerManifestFailureCloseButton != null)
            {
                prisonerManifestFailureCloseButton.gameObject.SetActive(true);
                prisonerManifestFailureCloseButton.interactable = true;
            }
        }

        private void ClosePrisonerManifestFailurePopup()
        {
            prisonerManifestFailurePopup.SetActive(false);
            ClosePrisonerSummonerResult();
        }

        private void ShowPrisonerSummonerContinue()
        {
            if (prisonerSummonerContinueButton != null)
            {
                prisonerSummonerContinueButton.gameObject.SetActive(true);
                prisonerSummonerContinueButton.interactable = true;
            }
        }

        private void ClosePrisonerSummonerResult()
        {
            prisonerSummonerPanel.SetActive(false);
            prisonerChoicePanel.SetActive(false);
            prisonerManifestFailurePopup.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            rewardPanel.SetActive(true);
            rewardPanelEntered = false;
            EnterRewardState();
            RefreshHud();
        }

        private void ClosePrisonerSummonerWithoutManifest()
        {
            pendingManifestMonster = null;
            rewardDetailText = "Manifest skipped.";
            if (prisonerSummonerButton != null)
            {
                prisonerSummonerButton.interactable = false;
            }

            ClosePrisonerSummonerResult();
        }

        private void OnAssimilateClicked()
        {
            rewardDetailText = "Assimilate is not implemented yet.";
            if (prisonerChoiceTitleText != null)
            {
                prisonerChoiceTitleText.text = $"{activePrisonerName} assimilate is not implemented.";
            }
        }

        private void OnCorruptClicked()
        {
            rewardDetailText = "Torture / Corrupt is not implemented yet.";
            if (prisonerChoiceTitleText != null)
            {
                prisonerChoiceTitleText.text = $"{activePrisonerName} torture / corrupt is not implemented.";
            }
        }

        private MonsterDefinition ResolveNextManifestCandidate()
        {
            if (currentSession == null)
            {
                return null;
            }

            var monsters = PakuriDataManager.Instance.GetMonsters(fallbackCatalog);
            var candidates = new List<MonsterDefinition>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null
                    || string.IsNullOrWhiteSpace(monster.MonsterId)
                    || string.Equals(monster.MonsterId, currentSession.SelectedMonsterId, StringComparison.OrdinalIgnoreCase)
                    || currentSession.HasManifestedMonster(monster.MonsterId))
                {
                    continue;
                }

                candidates.Add(monster);
            }

            if (candidates.Count == 0)
            {
                return null;
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private void SetSummonerPanel(MonsterDefinition monster, string prefix)
        {
            if (prisonerSummonerTitleText != null)
            {
                prisonerSummonerTitleText.text = monster == null ? "Manifest" : $"Manifest: {monster.DisplayName}";
            }

            if (prisonerSummonerImage != null)
            {
                prisonerSummonerImage.sprite = monster != null ? monster.UnitSprite : null;
                prisonerSummonerImage.color = monster != null && monster.UnitSprite != null ? Color.white : new Color(1f, 1f, 1f, 0.18f);
            }

            if (prisonerSummonerSummaryText != null)
            {
                prisonerSummonerSummaryText.text = monster == null
                    ? prefix
                    : $"{prefix}\n\n{BuildManifestMonsterSummary(monster)}";
            }
        }

        private static string BuildManifestMonsterSummary(MonsterDefinition monster)
        {
            if (monster == null)
            {
                return string.Empty;
            }

            var primary = FindManifestPrimarySkill(monster);
            var skillSummary = primary == null
                ? "A skill: none"
                : $"A skill: {primary.DisplayName}\n{ResolveSkillDescription(primary.Summary, primary.DescriptionText, "A skill basic attack.")}";
            return
                $"Name: {monster.DisplayName}\n" +
                $"Element: {monster.ElementLabel}\n" +
                $"HP: {monster.MaxHealth:0}\n" +
                $"Power: {monster.PowerStat:0}\n" +
                $"Base damage: {monster.BaseDamage:0}\n" +
                $"Attribute: {monster.PrimaryAttribute}\n\n" +
                skillSummary;
        }

        private static SkillDefinition FindManifestPrimarySkill(MonsterDefinition monster)
        {
            if (monster == null || monster.ActiveSkills == null)
            {
                return null;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill != null && skill.Slot == SkillSlot.A)
                {
                    return skill;
                }
            }

            return null;
        }

        private void OpenPrisonerPanel(string prisonerName)
        {
            BuildOfferingChoices();
            if (offeringChoices.Count == 0)
            {
                rewardDetailText = "공양 후보가 남아 있지 않다.";
                EnterRewardState();
                return;
            }

            rewardPanel.SetActive(false);
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, true);
            if (prisonerTitleText != null)
            {
                prisonerTitleText.text = $"{prisonerName} 공양 선택";
            }

            for (var i = 0; i < OfferingChoiceButtonCount; i++)
            {
                var button = prisonerChoiceButtons[i];
                if (button == null)
                {
                    continue;
                }

                var hasChoice = i < offeringChoices.Count;
                button.gameObject.SetActive(hasChoice);
                button.onClick.RemoveAllListeners();
                if (!hasChoice)
                {
                    continue;
                }

                var choiceIndex = i;
                var choice = offeringChoices[choiceIndex];
                SetButtonLabel(button, $"{choice.Title}\n{choice.Description}");
                button.interactable = true;
                button.onClick.AddListener(() => CommitOfferingChoice(choiceIndex));
            }
        }

        private void BuildOfferingChoices()
        {
            offeringChoices.Clear();

            if (currentSession == null)
            {
                return;
            }

            var monsters = ResolveOfferingTargetMonsters();
            for (var i = 0; i < monsters.Count; i++)
            {
                var monster = monsters[i];
                var memberState = currentSession.EnsurePartyMemberState(monster);
                if (monster == null || memberState == null)
                {
                    continue;
                }

                AddActiveSkillOfferingChoices(monster, memberState);
                AddPassiveSkillOfferingChoices(monster, memberState);
                AddEnhancementOfferingChoices(monster, memberState);
            }

            ShuffleOfferingChoices();

            while (offeringChoices.Count > OfferingChoiceButtonCount)
            {
                offeringChoices.RemoveAt(offeringChoices.Count - 1);
            }
        }

        private List<MonsterDefinition> ResolveOfferingTargetMonsters()
        {
            var monsters = new List<MonsterDefinition>();
            AddOfferingTargetMonster(monsters, RunSceneBootstrap.ActiveMonster ?? ResolveFallbackMonster());

            if (currentSession == null || currentSession.ManifestedMonsterIds == null)
            {
                return monsters;
            }

            for (var i = 0; i < currentSession.ManifestedMonsterIds.Count; i++)
            {
                AddOfferingTargetMonster(monsters, PakuriDataManager.Instance.ResolveMonster(currentSession.ManifestedMonsterIds[i], fallbackCatalog));
            }

            return monsters;
        }

        private static void AddOfferingTargetMonster(List<MonsterDefinition> monsters, MonsterDefinition monster)
        {
            if (monsters == null || monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                return;
            }

            for (var i = 0; i < monsters.Count; i++)
            {
                if (monsters[i] != null && string.Equals(monsters[i].MonsterId, monster.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            monsters.Add(monster);
        }

        private void AddActiveSkillOfferingChoices(MonsterDefinition monster, RunSession.RunMonsterState memberState)
        {
            if (memberState == null || memberState.LearnedActives.Count >= MaxRunActiveSkillCount)
            {
                return;
            }

            var activeSkills = GetActiveSkills(monster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.DisplayName))
                {
                    continue;
                }

                var choiceId = string.IsNullOrWhiteSpace(skill.SkillId) ? $"active:{skill.DisplayName}" : skill.SkillId;
                if (currentSession.HasChosenReward(memberState.MonsterId, choiceId) || currentSession.HasLearnedActive(memberState.MonsterId, skill.SkillId))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = choiceId,
                    MonsterId = memberState.MonsterId,
                    ChoiceKind = OfferingChoiceKind.ActiveSkill,
                    Title = $"신규 액티브: {skill.DisplayName}",
                    Description = ResolveSkillDescription(skill.Summary, skill.DescriptionText, "액티브 스킬을 습득한다."),
                    ActiveSkillId = skill.SkillId
                });
            }
        }

        private void AddPassiveSkillOfferingChoices(MonsterDefinition monster, RunSession.RunMonsterState memberState)
        {
            if (memberState == null || memberState.LearnedPassives.Count >= MaxRunPassiveSkillCount)
            {
                return;
            }

            var passiveSkills = GetPassiveSkills(monster);
            for (var i = 0; i < passiveSkills.Length; i++)
            {
                var passive = passiveSkills[i];
                if (passive == null || string.IsNullOrWhiteSpace(passive.DisplayName))
                {
                    continue;
                }

                var choiceId = string.IsNullOrWhiteSpace(passive.PassiveId) ? $"passive:{passive.DisplayName}" : passive.PassiveId;
                if (currentSession.HasChosenReward(memberState.MonsterId, choiceId) || currentSession.HasLearnedPassive(memberState.MonsterId, passive.PassiveId))
                {
                    continue;
                }

                if (!passive.IsAvailableWithoutActiveRequirement && !HasLearnedRequiredActive(monster, memberState, passive.RequiredActiveSlot))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = choiceId,
                    MonsterId = memberState.MonsterId,
                    ChoiceKind = OfferingChoiceKind.PassiveSkill,
                    Title = $"신규 패시브: {passive.DisplayName}",
                    Description = ResolveSkillDescription(passive.Summary, passive.DescriptionText, "패시브 스킬을 습득한다."),
                    PassiveSkillId = passive.PassiveId
                });
            }
        }

        private void AddEnhancementOfferingChoices(MonsterDefinition monster, RunSession.RunMonsterState memberState)
        {
            var structuredChoiceCount = offeringChoices.Count;
            AddActiveEnhancementOfferingChoices(monster, memberState);
            AddPassiveEnhancementOfferingChoices(monster, memberState);
            AddMasterSkillOfferingChoices(monster, memberState);

            if (offeringChoices.Count > structuredChoiceCount)
            {
                return;
            }

            var rewardChoices = GetRewardChoices(monster);
            for (var i = 0; i < rewardChoices.Length; i++)
            {
                var reward = rewardChoices[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId) || currentSession.HasChosenReward(memberState.MonsterId, reward.RewardId))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = reward.RewardId,
                    MonsterId = memberState.MonsterId,
                    ChoiceKind = OfferingChoiceKind.Enhancement,
                    Title = $"스킬 강화: {reward.Title}",
                    Description = reward.Description,
                    DamageMultiplier = reward.DamageMultiplier,
                    MagazineBonus = reward.MagazineBonus,
                    ShotIntervalMultiplier = reward.ShotIntervalMultiplier,
                    ReloadDurationMultiplier = reward.ReloadDurationMultiplier,
                    MaxHealthBonus = reward.MaxHealthBonus,
                    StatusChanceBonus = reward.StatusChanceBonus
                });
            }
        }

        private void AddActiveEnhancementOfferingChoices(MonsterDefinition monster, RunSession.RunMonsterState memberState)
        {
            var activeSkills = GetActiveSkills(monster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null || skill.EnhancementChoices == null || !currentSession.HasLearnedActive(memberState.MonsterId, skill.SkillId))
                {
                    continue;
                }

                if (CountChosenChoices(skill.EnhancementChoices, memberState) >= 3)
                {
                    continue;
                }

                AddChoiceDefinitions(skill.EnhancementChoices, memberState, $"{monster.DisplayName} 액티브 강화: {skill.DisplayName}");
            }
        }

        private void AddPassiveEnhancementOfferingChoices(MonsterDefinition monster, RunSession.RunMonsterState memberState)
        {
            var passiveSkills = GetPassiveSkills(monster);
            for (var i = 0; i < passiveSkills.Length; i++)
            {
                var passive = passiveSkills[i];
                if (passive == null || passive.EnhancementChoices == null || !currentSession.HasLearnedPassive(memberState.MonsterId, passive.PassiveId))
                {
                    continue;
                }

                if (HasAnyChosenChoice(passive.EnhancementChoices, memberState))
                {
                    continue;
                }

                AddChoiceDefinitions(passive.EnhancementChoices, memberState, $"{monster.DisplayName} 패시브 강화: {passive.DisplayName}");
            }
        }

        private void AddMasterSkillOfferingChoices(MonsterDefinition monster, RunSession.RunMonsterState memberState)
        {
            var activeSkills = GetActiveSkills(monster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null || skill.MasterSkillChoices == null || !currentSession.HasLearnedActive(memberState.MonsterId, skill.SkillId))
                {
                    continue;
                }

                if (CountChosenChoices(skill.EnhancementChoices, memberState) < 3 || HasAnyChosenChoice(skill.MasterSkillChoices, memberState))
                {
                    continue;
                }

                AddChoiceDefinitions(skill.MasterSkillChoices, memberState, $"{monster.DisplayName} 마스터 스킬: {skill.DisplayName}", OfferingChoiceKind.MasterSkill);
            }
        }

        private void AddChoiceDefinitions(SkillChoiceDefinition[] choices, RunSession.RunMonsterState memberState, string titlePrefix, OfferingChoiceKind kind = OfferingChoiceKind.Enhancement)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId) || currentSession.HasChosenReward(memberState.MonsterId, choice.ChoiceId))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = choice.ChoiceId,
                    MonsterId = memberState.MonsterId,
                    ChoiceKind = kind,
                    Title = $"{titlePrefix} - {choice.Title}",
                    Description = string.IsNullOrWhiteSpace(choice.DescriptionText) ? "강화 효과를 습득한다." : choice.DescriptionText
                });
            }
        }

        private int CountChosenChoices(SkillChoiceDefinition[] choices, RunSession.RunMonsterState memberState)
        {
            var chosenCount = 0;
            if (choices == null)
            {
                return chosenCount;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && currentSession.HasChosenReward(memberState.MonsterId, choice.ChoiceId))
                {
                    chosenCount++;
                }
            }

            return chosenCount;
        }

        private bool HasAnyChosenChoice(SkillChoiceDefinition[] choices, RunSession.RunMonsterState memberState)
        {
            return CountChosenChoices(choices, memberState) > 0;
        }

        private static string ResolveSkillDescription(string summary, string descriptionText, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(descriptionText))
            {
                return descriptionText;
            }

            return string.IsNullOrWhiteSpace(summary) ? fallback : summary;
        }

        private bool HasLearnedRequiredActive(MonsterDefinition monster, RunSession.RunMonsterState memberState, SkillSlot requiredSlot)
        {
            var activeSkills = GetActiveSkills(monster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill != null
                    && skill.Slot == requiredSlot
                    && currentSession.HasLearnedActive(memberState.MonsterId, skill.SkillId))
                {
                    return true;
                }
            }

            return false;
        }

        private void ShuffleOfferingChoices()
        {
            for (var i = offeringChoices.Count - 1; i > 0; i--)
            {
                var swapIndex = UnityEngine.Random.Range(0, i + 1);
                var current = offeringChoices[i];
                offeringChoices[i] = offeringChoices[swapIndex];
                offeringChoices[swapIndex] = current;
            }
        }

        private void CommitOfferingChoice(int choiceIndex)
        {
            if (currentSession == null || choiceIndex < 0 || choiceIndex >= offeringChoices.Count)
            {
                return;
            }

            var choice = offeringChoices[choiceIndex];
            currentSession.RecordOfferingChoice(choice.MonsterId, choice.ChoiceId, choice.ActiveSkillId, choice.PassiveSkillId);
            if (choice.ChoiceKind == OfferingChoiceKind.Enhancement)
            {
                currentSession.AccumulateReward(
                    choice.MonsterId,
                    choice.DamageMultiplier,
                    choice.MagazineBonus,
                    choice.ShotIntervalMultiplier,
                    choice.ReloadDurationMultiplier,
                    choice.MaxHealthBonus,
                    choice.StatusChanceBonus);
            }

            rewardDetailText = $"{choice.Title} 선택 완료";
            if (combatController != null)
            {
                combatController.RefreshManifestedMonsterParty(currentSession);
            }

            offeringChoices.Clear();
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
            rewardPanel.SetActive(true);
            rewardPanelEntered = false;
            EnterRewardState();
            RefreshHud();
        }

        private void OnContinueAfterReward()
        {
            if (currentSession == null || combatController == null)
            {
                return;
            }

            currentSession.AdvanceDay();
            rewardSummaryApplied = false;
            rewardPanelEntered = false;
            rewardDetailText = string.Empty;
            rewardPanel.SetActive(false);
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
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
            SetOptionalPanelActive(prisonerPanel, false);
            SetOptionalPanelActive(prisonerOfferingPanel, false);
            prisonerChoicePanel.SetActive(false);
            prisonerSummonerPanel.SetActive(false);
            defeatSummaryText.text = currentSession == null
                ? "Nexus가 붕괴했다."
                : $"{currentSession.SelectedMonsterName} run이 일차 {currentSession.DayIndex}에서 실패했다.";
        }

        private MonsterDefinition ResolveFallbackMonster()
        {
            return PakuriDataManager.Instance.ResolveMonster(RunSceneBootstrap.FallbackMonsterId, fallbackCatalog);
        }

        private static SkillDefinition[] GetActiveSkills(MonsterDefinition monster)
        {
            return monster == null
                ? Array.Empty<SkillDefinition>()
                : PakuriDataManager.Instance.GetActiveSkills(monster.MonsterId, monster);
        }

        private static PassiveDefinition[] GetPassiveSkills(MonsterDefinition monster)
        {
            return monster == null
                ? Array.Empty<PassiveDefinition>()
                : PakuriDataManager.Instance.GetPassiveSkills(monster.MonsterId, monster);
        }

        private static MonsterDefinition.RewardChoiceDefinition[] GetRewardChoices(MonsterDefinition monster)
        {
            return monster == null
                ? Array.Empty<MonsterDefinition.RewardChoiceDefinition>()
                : PakuriDataManager.Instance.GetRewardChoices(monster.MonsterId, monster);
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
                rootRect.sizeDelta = new Vector2(Mathf.Max(rootRect.sizeDelta.x, RewardButtonWidth), (RewardButtonHeight * RewardButtonTemplateCount) + (RewardButtonSpacing * (RewardButtonTemplateCount - 1)));
            }

            for (var i = 0; i < RewardButtonTemplateCount; i++)
            {
                var slotName = ResolveTemplateName(i);
                var legacySlot = rewardButtonRoot.transform.Find($"RewardButton_{i}");
                if (legacySlot != null && rewardButtonRoot.transform.Find(slotName) == null)
                {
                    legacySlot.name = slotName;
                }

                var existingSlot = rewardButtonRoot.transform.Find(slotName);
                var templateLabel = ResolveTemplatePreviewLabel(i);
                var button = EnsureButton(
                    rewardButtonRoot.transform,
                    slotName,
                    showPreviewLabels ? templateLabel : string.Empty,
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
                if (child == null || child.GetComponent<Button>() == null || IsRewardButtonTemplateName(child.name))
                {
                    continue;
                }

                child.gameObject.SetActive(false);
            }
        }

        private Button ResolveRewardTemplate(string rewardKind)
        {
            var index = string.Equals(rewardKind, "Prisoner", System.StringComparison.OrdinalIgnoreCase)
                ? 0
                : string.Equals(rewardKind, "Artifact", System.StringComparison.OrdinalIgnoreCase)
                    ? 1
                    : 2;
            return rewardButtonRoot.transform.Find(ResolveTemplateName(index))?.GetComponent<Button>();
        }

        private void ClearRuntimeRewardButtons()
        {
            if (rewardButtonRoot == null)
            {
                return;
            }

            for (var i = rewardButtonRoot.transform.childCount - 1; i >= 0; i--)
            {
                var child = rewardButtonRoot.transform.GetChild(i);
                if (child == null || IsRewardButtonTemplateName(child.name))
                {
                    continue;
                }

                if (!child.name.StartsWith("RewardItem_", System.StringComparison.OrdinalIgnoreCase))
                {
                    child.gameObject.SetActive(false);
                    continue;
                }

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private static string ResolveTemplatePreviewLabel(int index)
        {
            switch (index)
            {
                case 0:
                    return "포로 버튼 템플릿";
                case 1:
                    return "유물 버튼 템플릿";
                default:
                    return "기타 보상 버튼 템플릿";
            }
        }

        private static string ResolveTemplateName(int index)
        {
            switch (index)
            {
                case 0:
                    return PrisonerTemplateName;
                case 1:
                    return ArtifactTemplateName;
                default:
                    return MaterialTemplateName;
            }
        }

        private void ConfigurePrisonerPanelPreview()
        {
            if (prisonerTitleText != null)
            {
                prisonerTitleText.text = "포로 공양 선택지";
            }

            for (var i = 0; i < prisonerChoiceButtons.Length; i++)
            {
                if (prisonerChoiceButtons[i] == null)
                {
                    continue;
                }

                prisonerChoiceButtons[i].gameObject.SetActive(true);
                prisonerChoiceButtons[i].onClick.RemoveAllListeners();
                SetButtonLabel(prisonerChoiceButtons[i], $"공양 선택지 {i + 1}\nPlay Mode에서 실제 후보로 교체된다.");
            }
        }

        private void ConfigurePrisonerChoicePanelPreview()
        {
            if (prisonerChoiceTitleText != null)
            {
                prisonerChoiceTitleText.text = "Prisoner Choice";
            }

            SetButtonLabel(prisonerModeButtons[0], "Manifest\nPreview button");
            SetButtonLabel(prisonerModeButtons[1], "Assimilate\nPreview button");
            SetButtonLabel(prisonerModeButtons[2], "Offering\nPreview button");
            SetButtonLabel(prisonerModeButtons[3], "Torture / Corrupt\nPreview button");
            for (var i = 0; i < prisonerModeButtons.Length; i++)
            {
                if (prisonerModeButtons[i] != null)
                {
                    prisonerModeButtons[i].gameObject.SetActive(true);
                }
            }
        }

        private void ConfigurePrisonerSummonerPanelPreview()
        {
            if (prisonerSummonerTitleText != null)
            {
                prisonerSummonerTitleText.text = "Manifest Preview";
            }

            if (prisonerSummonerSummaryText != null)
            {
                prisonerSummonerSummaryText.text = "Manifest result will show monster image, name, A skill, and basic stats in Play Mode.";
            }

            if (prisonerSummonerImage != null)
            {
                prisonerSummonerImage.color = new Color(1f, 1f, 1f, 0.18f);
            }

            if (prisonerSummonerButton != null)
            {
                SetButtonLabel(prisonerSummonerButton, "Try Manifest");
            }

            if (prisonerSummonerContinueButton != null)
            {
                SetButtonLabel(prisonerSummonerContinueButton, "Continue");
                prisonerSummonerContinueButton.gameObject.SetActive(true);
            }

            if (prisonerSummonerBackButton != null)
            {
                SetButtonLabel(prisonerSummonerBackButton, "Back to Reward");
                prisonerSummonerBackButton.gameObject.SetActive(true);
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

        private static void SetOptionalPanelActive(GameObject panel, bool isActive)
        {
            if (panel != null)
            {
                panel.SetActive(isActive);
            }
        }

        private static bool IsRewardButtonTemplateName(string objectName)
        {
            return objectName == PrisonerTemplateName
                || objectName == ArtifactTemplateName
                || objectName == MaterialTemplateName
                || objectName == "RewardButton_0"
                || objectName == "RewardButton_1"
                || objectName == "RewardButton_2";
        }

        private static bool IsPrisonerReward(CombatRuntimeController.RewardChoiceView rewardView, string rewardId)
        {
            return string.Equals(rewardView.RewardKind, "Prisoner", System.StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(rewardView.PrisonerName) && rewardId.StartsWith("prisoner:", System.StringComparison.OrdinalIgnoreCase));
        }

        private bool IsRewardModalOpen()
        {
            return IsPanelActive(prisonerChoicePanel)
                || IsPanelActive(prisonerSummonerPanel)
                || IsPanelActive(prisonerManifestFailurePopup)
                || IsPanelActive(prisonerPanel)
                || IsPanelActive(prisonerOfferingPanel);
        }

        private static bool IsPanelActive(GameObject panel)
        {
            return panel != null && panel.activeSelf;
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

    [DisallowMultipleComponent]
    public sealed class CombatMonsterPanelUiController : MonoBehaviour
    {
        private const int SkillSlotCount = 3;
        private const int MonsterGroupCount = 5;
        private const string PanelName = "MonsterPanel";
        private static readonly string[] MonsterGroupNames = { "1PMonster", "2PMonster", "3PMonster", "4PMonster", "5PMonster" };

        [SerializeField] private CombatRuntimeController combatController;
        [SerializeField] private Color availableColor = Color.white;
        [SerializeField] private Color cooldownOverlayColor = new Color(0f, 0f, 0f, 0.58f);

        private readonly List<CombatRuntimeController.MonsterPanelSkillView> skillViews = new List<CombatRuntimeController.MonsterPanelSkillView>(SkillSlotCount);
        private readonly GameObject[] partyMonsterGroups = new GameObject[MonsterGroupCount];
        private readonly SkillSlotBinding[][] partySlots = new SkillSlotBinding[MonsterGroupCount][];
        private GameObject monsterPanel;
        private Sprite cooldownOverlaySprite;

        private sealed class SkillSlotBinding
        {
            public GameObject Root;
            public Image IconImage;
            public Image CooldownOverlay;
            public TMP_Text AmmoText;
            public TMP_Text NameText;
            public Sprite DefaultSprite;
        }

        public void Bind(CombatRuntimeController controller)
        {
            combatController = controller;
            BindPanelHierarchy();
            Refresh();
        }

        private void Awake()
        {
            BindPanelHierarchy();
        }

        private void Update()
        {
            Refresh();
        }

        private void BindPanelHierarchy()
        {
            monsterPanel = FindDescendant(transform, PanelName)?.gameObject;
            if (monsterPanel == null)
            {
                return;
            }

            for (var groupIndex = 0; groupIndex < MonsterGroupCount; groupIndex++)
            {
                var group = FindDescendant(monsterPanel.transform, MonsterGroupNames[groupIndex])?.gameObject;
                partyMonsterGroups[groupIndex] = group;
                if (group == null)
                {
                    continue;
                }

                partySlots[groupIndex] = new SkillSlotBinding[SkillSlotCount];
                for (var slotIndex = 0; slotIndex < SkillSlotCount; slotIndex++)
                {
                    partySlots[groupIndex][slotIndex] = BindSkillSlot(group.transform, slotIndex);
                }
            }
        }

        private void Refresh()
        {
            if (combatController == null)
            {
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }

            if (monsterPanel == null)
            {
                BindPanelHierarchy();
            }

            if (monsterPanel == null)
            {
                return;
            }

            monsterPanel.SetActive(true);
            var partyCount = combatController != null ? Mathf.Clamp(combatController.PartyMonsterCount, 1, MonsterGroupCount) : 1;
            for (var groupIndex = 0; groupIndex < MonsterGroupCount; groupIndex++)
            {
                var group = partyMonsterGroups[groupIndex];
                if (group == null)
                {
                    continue;
                }

                var groupActive = groupIndex < partyCount;
                group.SetActive(groupActive);
                if (!groupActive)
                {
                    continue;
                }

                var count = combatController != null
                    ? combatController.GetPartyMonsterPanelSkillViews(groupIndex, skillViews, SkillSlotCount)
                    : 0;
                var slots = partySlots[groupIndex];
                for (var slotIndex = 0; slotIndex < SkillSlotCount; slotIndex++)
                {
                    ApplySlot(slots != null ? slots[slotIndex] : null, slotIndex < count ? skillViews[slotIndex] : default, slotIndex < count);
                }
            }
        }

        private SkillSlotBinding BindSkillSlot(Transform monsterGroup, int index)
        {
            var slotTransform = FindDescendant(monsterGroup, $"Active{index + 1}");
            if (slotTransform == null)
            {
                return null;
            }

            var icon = slotTransform.GetComponent<Image>() ?? FindSlotIconImage(slotTransform);
            var overlay = EnsureCooldownOverlay(slotTransform);
            var ammoText = FindDescendant(slotTransform, "Text (TMP)")?.GetComponent<TMP_Text>()
                ?? FindDescendant(slotTransform, "AmmoText")?.GetComponent<TMP_Text>()
                ?? slotTransform.GetComponentInChildren<TMP_Text>(true);
            var nameText = FindDescendant(slotTransform, "NameText")?.GetComponent<TMP_Text>();

            if (ammoText != null)
            {
                ammoText.transform.SetAsLastSibling();
            }

            return new SkillSlotBinding
            {
                Root = slotTransform.gameObject,
                IconImage = icon,
                CooldownOverlay = overlay,
                AmmoText = ammoText,
                NameText = nameText,
                DefaultSprite = icon != null ? icon.sprite : null
            };
        }

        private void ApplySlot(SkillSlotBinding slot, CombatRuntimeController.MonsterPanelSkillView view, bool isActive)
        {
            if (slot == null || slot.Root == null)
            {
                return;
            }

            slot.Root.SetActive(isActive);
            if (!isActive)
            {
                return;
            }

            if (slot.IconImage != null)
            {
                slot.IconImage.sprite = view.Icon != null ? view.Icon : slot.DefaultSprite;
                slot.IconImage.color = availableColor;
            }

            if (slot.NameText != null)
            {
                slot.NameText.text = view.DisplayName;
            }

            if (slot.AmmoText != null)
            {
                slot.AmmoText.gameObject.SetActive(view.IsMagazine);
                slot.AmmoText.text = view.IsMagazine ? Mathf.Max(0, view.CurrentAmmo).ToString(CultureInfo.InvariantCulture) : string.Empty;
                slot.AmmoText.color = Color.white;
                slot.AmmoText.transform.SetAsLastSibling();
            }

            if (slot.CooldownOverlay != null)
            {
                slot.CooldownOverlay.gameObject.SetActive(view.IsCoolingDown);
                slot.CooldownOverlay.color = cooldownOverlayColor;
                slot.CooldownOverlay.fillAmount = view.IsCoolingDown ? view.CooldownRemainingRatio : 0f;
            }
        }

        private Image EnsureCooldownOverlay(Transform slotTransform)
        {
            var overlayTransform = FindDescendant(slotTransform, "CooldownOverlay");
            if (overlayTransform == null)
            {
                var overlayObject = new GameObject("CooldownOverlay", typeof(RectTransform), typeof(Image));
                overlayTransform = overlayObject.transform;
                overlayTransform.SetParent(slotTransform, false);
                var rect = overlayObject.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
            }

            var overlay = overlayTransform.GetComponent<Image>() ?? overlayTransform.gameObject.AddComponent<Image>();
            overlay.sprite = GetCooldownOverlaySprite();
            overlay.raycastTarget = false;
            overlay.type = Image.Type.Filled;
            overlay.fillMethod = Image.FillMethod.Vertical;
            overlay.fillOrigin = (int)Image.OriginVertical.Bottom;
            overlay.fillAmount = 0f;
            overlay.color = cooldownOverlayColor;
            overlayTransform.SetAsLastSibling();
            return overlay;
        }

        private Sprite GetCooldownOverlaySprite()
        {
            if (cooldownOverlaySprite == null)
            {
                cooldownOverlaySprite = Resources.Load<Sprite>("DebugUiSolid");
            }

            return cooldownOverlaySprite;
        }

        private static Image FindSlotIconImage(Transform slotTransform)
        {
            if (slotTransform == null)
            {
                return null;
            }

            var images = slotTransform.GetComponentsInChildren<Image>(true);
            for (var i = 0; i < images.Length; i++)
            {
                var image = images[i];
                if (image != null && !IsUnderNamedTransform(image.transform, "CooldownOverlay"))
                {
                    return image;
                }
            }

            return null;
        }

        private static bool IsUnderNamedTransform(Transform transform, string targetName)
        {
            while (transform != null)
            {
                if (transform.name == targetName)
                {
                    return true;
                }

                transform = transform.parent;
            }

            return false;
        }

        private static Transform FindDescendant(Transform root, string targetName)
        {
            if (root == null || string.IsNullOrWhiteSpace(targetName))
            {
                return null;
            }

            if (root.name == targetName)
            {
                return root;
            }

            for (var i = 0; i < root.childCount; i++)
            {
                var result = FindDescendant(root.GetChild(i), targetName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
