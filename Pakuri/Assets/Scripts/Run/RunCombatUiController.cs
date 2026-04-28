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
        private const int RewardButtonTemplateCount = 3;
        private const string PrisonerTemplateName = "Prisoner";
        private const string MaterialTemplateName = "Material";
        private const string ArtifactTemplateName = "Artifact";
        private const float RewardButtonWidth = 620f;
        private const float RewardButtonHeight = 96f;
        private const float RewardButtonSpacing = 16f;
        private const int OfferingChoiceButtonCount = 3;
        private const int MaxRunActiveSkillCount = 3;
        private const int MaxRunPassiveSkillCount = 3;

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
        private Text prisonerTitleText;
        private readonly Button[] prisonerChoiceButtons = new Button[OfferingChoiceButtonCount];

        private GameObject defeatPanel;
        private Text defeatSummaryText;

        private RunSession currentSession;
        private bool rewardSummaryApplied;
        private bool rewardPanelEntered;
        private bool defeatPanelEntered;
        private string rewardDetailText = string.Empty;

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
            public string Title;
            public string Description;
            public string ActiveSkillName;
            public string PassiveSkillName;
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
                && transform.Find("PrisonerPanel") != null
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
                && transform.Find("PrisonerPanel") != null
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

            prisonerPanel = transform.Find("PrisonerPanel")?.gameObject;
            if (prisonerPanel != null)
            {
                prisonerTitleText = prisonerPanel.transform.Find("Title")?.GetComponent<Text>();
                for (var i = 0; i < OfferingChoiceButtonCount; i++)
                {
                    prisonerChoiceButtons[i] = prisonerPanel.transform.Find($"Choice{i + 1}")?.GetComponent<Button>();
                    if (prisonerChoiceButtons[i] != null)
                    {
                        prisonerChoiceButtons[i].onClick.RemoveAllListeners();
                    }
                }

                EnsureVerticalLayout(prisonerPanel.GetComponent<RectTransform>(), 0f, 0f, 0f);
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
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }
        }

        private void ResolveRuntimeReferences()
        {
            if (combatController == null)
            {
                combatController = FindFirstObjectByType<CombatRuntimeController>();
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

            prisonerPanel = EnsurePanel("PrisonerPanel", new Color(0.16f, 0.11f, 0.10f, 0.94f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(760f, 520f), Vector2.zero);
            prisonerTitleText = EnsureText(prisonerPanel.transform, "Title", "포로 공양", 30, TextAnchor.MiddleCenter);
            for (var i = 0; i < OfferingChoiceButtonCount; i++)
            {
                prisonerChoiceButtons[i] = EnsureButton(prisonerPanel.transform, $"Choice{i + 1}", string.Empty, null);
            }

            EnsureVerticalLayout(prisonerPanel.GetComponent<RectTransform>(), 28f, 28f, 18f);

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
            prisonerPanel.SetActive(true);
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
            ConfigurePrisonerPanelPreview();
        }

        private void ShowEditorUiForEditing()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(true);
            prisonerPanel.SetActive(true);
            defeatPanel.SetActive(true);

            EnsureRewardButtonSlots(true);
            rewardContinueButton.gameObject.SetActive(true);
            ConfigurePrisonerPanelPreview();
        }

        private void ShowRuntimeHudOnly()
        {
            hudPanel.SetActive(true);
            rewardPanel.SetActive(false);
            prisonerPanel.SetActive(false);
            defeatPanel.SetActive(false);
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
            RebuildRewardButtons();
            if (string.Equals(rewardView.RewardKind, "Prisoner", System.StringComparison.OrdinalIgnoreCase))
            {
                OpenPrisonerPanel(rewardView.PrisonerName);
                return;
            }

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
            prisonerPanel.SetActive(true);
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

            var monster = RunSceneBootstrap.ActiveMonster ?? ResolveFallbackMonster();
            if (monster == null || currentSession == null)
            {
                return;
            }

            AddActiveSkillOfferingChoices(monster);
            AddPassiveSkillOfferingChoices(monster);
            AddEnhancementOfferingChoices(monster);
            ShuffleOfferingChoices();

            while (offeringChoices.Count > OfferingChoiceButtonCount)
            {
                offeringChoices.RemoveAt(offeringChoices.Count - 1);
            }
        }

        private void AddActiveSkillOfferingChoices(MonsterDefinition monster)
        {
            if (monster.ActiveSkills == null)
            {
                return;
            }

            if (currentSession.LearnedActives.Count >= MaxRunActiveSkillCount)
            {
                return;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.DisplayName))
                {
                    continue;
                }

                var choiceId = string.IsNullOrWhiteSpace(skill.SkillId) ? $"active:{skill.DisplayName}" : skill.SkillId;
                if (currentSession.HasChosenReward(choiceId) || currentSession.HasLearnedActive(skill.DisplayName))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = choiceId,
                    ChoiceKind = OfferingChoiceKind.ActiveSkill,
                    Title = $"신규 액티브: {skill.DisplayName}",
                    Description = ResolveSkillDescription(skill.Summary, skill.DescriptionText, "액티브 스킬을 습득한다."),
                    ActiveSkillName = skill.DisplayName
                });
            }
        }

        private void AddPassiveSkillOfferingChoices(MonsterDefinition monster)
        {
            if (monster.PassiveSkills == null)
            {
                return;
            }

            if (currentSession.LearnedPassives.Count >= MaxRunPassiveSkillCount)
            {
                return;
            }

            for (var i = 0; i < monster.PassiveSkills.Length; i++)
            {
                var passive = monster.PassiveSkills[i];
                if (passive == null || string.IsNullOrWhiteSpace(passive.DisplayName))
                {
                    continue;
                }

                var choiceId = string.IsNullOrWhiteSpace(passive.PassiveId) ? $"passive:{passive.DisplayName}" : passive.PassiveId;
                if (currentSession.HasChosenReward(choiceId) || currentSession.HasLearnedPassive(passive.DisplayName))
                {
                    continue;
                }

                if (!passive.IsAvailableWithoutActiveRequirement && !HasLearnedRequiredActive(monster, passive.RequiredActiveSlot))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = choiceId,
                    ChoiceKind = OfferingChoiceKind.PassiveSkill,
                    Title = $"신규 패시브: {passive.DisplayName}",
                    Description = ResolveSkillDescription(passive.Summary, passive.DescriptionText, "패시브 스킬을 습득한다."),
                    PassiveSkillName = passive.DisplayName
                });
            }
        }

        private void AddEnhancementOfferingChoices(MonsterDefinition monster)
        {
            var structuredChoiceCount = offeringChoices.Count;
            AddActiveEnhancementOfferingChoices(monster);
            AddPassiveEnhancementOfferingChoices(monster);
            AddMasterSkillOfferingChoices(monster);

            if (offeringChoices.Count > structuredChoiceCount)
            {
                return;
            }

            if (monster.InitialRewardChoices == null)
            {
                return;
            }

            for (var i = 0; i < monster.InitialRewardChoices.Length; i++)
            {
                var reward = monster.InitialRewardChoices[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId) || currentSession.HasChosenReward(reward.RewardId))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = reward.RewardId,
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

        private void AddActiveEnhancementOfferingChoices(MonsterDefinition monster)
        {
            if (monster.ActiveSkills == null)
            {
                return;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill == null || skill.EnhancementChoices == null || !currentSession.HasLearnedActive(skill.DisplayName))
                {
                    continue;
                }

                if (CountChosenChoices(skill.EnhancementChoices) >= 3)
                {
                    continue;
                }

                AddChoiceDefinitions(skill.EnhancementChoices, $"액티브 강화: {skill.DisplayName}");
            }
        }

        private void AddPassiveEnhancementOfferingChoices(MonsterDefinition monster)
        {
            if (monster.PassiveSkills == null)
            {
                return;
            }

            for (var i = 0; i < monster.PassiveSkills.Length; i++)
            {
                var passive = monster.PassiveSkills[i];
                if (passive == null || passive.EnhancementChoices == null || !currentSession.HasLearnedPassive(passive.DisplayName))
                {
                    continue;
                }

                if (HasAnyChosenChoice(passive.EnhancementChoices))
                {
                    continue;
                }

                AddChoiceDefinitions(passive.EnhancementChoices, $"패시브 강화: {passive.DisplayName}");
            }
        }

        private void AddMasterSkillOfferingChoices(MonsterDefinition monster)
        {
            if (monster.ActiveSkills == null)
            {
                return;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill == null || skill.MasterSkillChoices == null || !currentSession.HasLearnedActive(skill.DisplayName))
                {
                    continue;
                }

                if (CountChosenChoices(skill.EnhancementChoices) < 3 || HasAnyChosenChoice(skill.MasterSkillChoices))
                {
                    continue;
                }

                AddChoiceDefinitions(skill.MasterSkillChoices, $"마스터 스킬: {skill.DisplayName}", OfferingChoiceKind.MasterSkill);
            }
        }

        private void AddChoiceDefinitions(SkillChoiceDefinition[] choices, string titlePrefix, OfferingChoiceKind kind = OfferingChoiceKind.Enhancement)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId) || currentSession.HasChosenReward(choice.ChoiceId))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    ChoiceId = choice.ChoiceId,
                    ChoiceKind = kind,
                    Title = $"{titlePrefix} - {choice.Title}",
                    Description = string.IsNullOrWhiteSpace(choice.DescriptionText) ? "강화 효과를 습득한다." : choice.DescriptionText
                });
            }
        }

        private int CountChosenChoices(SkillChoiceDefinition[] choices)
        {
            var chosenCount = 0;
            if (choices == null)
            {
                return chosenCount;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && currentSession.HasChosenReward(choice.ChoiceId))
                {
                    chosenCount++;
                }
            }

            return chosenCount;
        }

        private bool HasAnyChosenChoice(SkillChoiceDefinition[] choices)
        {
            return CountChosenChoices(choices) > 0;
        }

        private static string ResolveSkillDescription(string summary, string descriptionText, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(descriptionText))
            {
                return descriptionText;
            }

            return string.IsNullOrWhiteSpace(summary) ? fallback : summary;
        }

        private bool HasLearnedRequiredActive(MonsterDefinition monster, SkillSlot requiredSlot)
        {
            if (monster.ActiveSkills == null)
            {
                return false;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill != null
                    && skill.Slot == requiredSlot
                    && currentSession.HasLearnedActive(skill.DisplayName))
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
                var swapIndex = Random.Range(0, i + 1);
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
            currentSession.RecordOfferingChoice(choice.ChoiceId, choice.ActiveSkillName, choice.PassiveSkillName);
            if (choice.ChoiceKind == OfferingChoiceKind.Enhancement)
            {
                currentSession.AccumulateReward(
                    choice.DamageMultiplier,
                    choice.MagazineBonus,
                    choice.ShotIntervalMultiplier,
                    choice.ReloadDurationMultiplier,
                    choice.MaxHealthBonus,
                    choice.StatusChanceBonus);
            }

            rewardDetailText = $"{choice.Title} 선택 완료";
            offeringChoices.Clear();
            prisonerPanel.SetActive(false);
            rewardPanel.SetActive(true);
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
            prisonerPanel.SetActive(false);
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
            prisonerPanel.SetActive(false);
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

        private static void SetButtonLabel(Button button, string label)
        {
            var text = button != null ? button.transform.Find("Label")?.GetComponent<Text>() : null;
            if (text != null)
            {
                text.text = label;
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
