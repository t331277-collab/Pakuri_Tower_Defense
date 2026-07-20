using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameUIManager : MonoBehaviour
    {
        private const int PrisonPartySlotCount = 5;

        private readonly List<RewardButtonView> rewardButtons = new List<RewardButtonView>();
        private readonly PrisonPartySlotView[] prisonPartySlots = new PrisonPartySlotView[PrisonPartySlotCount];
        private readonly string[] prisonSlotMonsterIds = new string[PrisonPartySlotCount];

        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private InGameCombatManager combatManager;
        [Header("Prison Panel Monster Portraits")]
        [SerializeField] private Sprite arielPrisonPortrait;
        [SerializeField] private Sprite evePrisonPortrait;
        [SerializeField] private Sprite rinPrisonPortrait;
        [SerializeField] private Sprite seinPrisonPortrait;
        [SerializeField] private Sprite vegaPrisonPortrait;
        [SerializeField] private Vector2 rewardButtonFirstColumnPosition = new Vector2(-321.97855f, 295f);
        [SerializeField] private float rewardButtonColumnSpacingX = 533.97855f;
        [SerializeField] private float rewardButtonRowSpacingY = 122f;
        [SerializeField] private int rewardButtonRowsPerColumn = 3;

        private GameObject rewardPanel;
        private Transform rewardButtonContainer;
        private Button prisonerTemplateButton;
        private Button goldTemplateButton;
        private Button darkTemplateButton;
        private Button nextButton;
        private TMP_Text rewardSummaryText;
        private GameObject prisonerChoicePopUp;
        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;
        private GameObject prisonPanel;
        private TMP_Text prisonStageInfoText;
        private TMP_Text prisonGoldInfoText;
        private TMP_Text prisonDarkInfoText;
        private Image prisonPrisonerImage;
        private TMP_Text prisonPrisonerNameText;
        private int shownStage = -1;
        private int shownDay = -1;
        private RewardButtonView activePrisonerButton;
        private OfferingUI offeringUI;
        private MenifestUI menifestUI;

        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindStaticButtons();
            HideTransientPanels();
        }

        private void Update()
        {
            ResolveReferences();
            RefreshInfo();

            if (stageManager == null || stageManager.State != StageState.RewardReady)
            {
                return;
            }

            if (shownStage == stageManager.CurrentStage && shownDay == stageManager.CurrentDay)
            {
                return;
            }

            ShowRewardPanel();
        }

        private void ShowRewardPanel()
        {
            shownStage = stageManager.CurrentStage;
            shownDay = stageManager.CurrentDay;
            HideTransientPanels();
            ClearClonedRewardButtons();

            SetActive(rewardPanel, true);

            if (rewardSummaryText != null)
            {
                rewardSummaryText.text = $"Stage {stageManager.CurrentStage}-{stageManager.CurrentDay} Reward";
            }

            var order = 0;
            var prisoners = stageManager.PendingPrisonerEnemyIds;
            for (var i = 0; i < prisoners.Count; i++)
            {
                var prisonerId = prisoners[i];
                var button = CreateRewardButton(prisonerTemplateButton, "PrisonerReward", order++);
                SetButtonLabel(button, $"Prisoner\n{ResolvePrisonerDisplayName(prisonerId)}");
                var view = RegisterRewardButton(button, RewardKind.Prisoner, 0, prisonerId);
                button.onClick.AddListener(() => OpenPrisonPanel(view));
            }

            if (stageManager.PendingGoldReward > 0)
            {
                var amount = stageManager.PendingGoldReward;
                var button = CreateRewardButton(goldTemplateButton, "GoldReward", order++);
                SetButtonLabel(button, $"Gold\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.Gold, amount, string.Empty);
                button.onClick.AddListener(() => ClaimMaterialReward(view, amount, 0));
            }

            if (stageManager.PendingDarkTraceReward > 0)
            {
                var amount = stageManager.PendingDarkTraceReward;
                var button = CreateRewardButton(darkTemplateButton, "DarkTraceReward", order++);
                SetButtonLabel(button, $"Dark Trace\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.DarkTrace, amount, string.Empty);
                button.onClick.AddListener(() => ClaimMaterialReward(view, 0, amount));
            }

            SetTemplateActive(prisonerTemplateButton, false);
            SetTemplateActive(goldTemplateButton, false);
            SetTemplateActive(darkTemplateButton, false);
            RefreshInfo();
        }

        private void OpenPrisonPanel(RewardButtonView view)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            activePrisonerButton = view;
            SetActive(rewardPanel, false);
            SetActive(prisonerChoicePopUp, false);
            SetActive(prisonPanel, true);
            RefreshPrisonPanel();
        }

        private void ClaimMaterialReward(RewardButtonView view, int gold, int darkTrace)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            var session = ResolveSession();
            if (session == null)
            {
                return;
            }

            session.ClaimMaterialReward(gold, darkTrace);
            view.SetConsumed();
            RefreshInfo();
        }

        private void ContinueToNextDay()
        {
            HideTransientPanels();
            SetActive(rewardPanel, false);
            ClearClonedRewardButtons();
            shownStage = -1;
            shownDay = -1;

            if (stageManager != null)
            {
                stageManager.ContinueToNextDay();
            }
        }

        private void RefreshInfo()
        {
            var session = ResolveSession();
            if (stageInfoText != null)
            {
                var stage = stageManager != null ? stageManager.CurrentStage : (session != null ? session.StageIndex : 1);
                var day = stageManager != null ? stageManager.CurrentDay : (session != null ? session.DayIndex : 1);
                stageInfoText.text = $"Stage {stage}-{day}";
            }

            if (goldInfoText != null)
            {
                goldInfoText.gameObject.SetActive(true);
                goldInfoText.text = $"Gold {Math.Max(0, session != null ? session.Gold : 0)}";
            }

            if (darkInfoText != null)
            {
                darkInfoText.gameObject.SetActive(true);
                darkInfoText.text = $"Dark {Math.Max(0, session != null ? session.DarkTrace : 0)}";
            }

            var refreshPrisonInfo = prisonPanel != null && prisonPanel.activeSelf;
            if (refreshPrisonInfo && prisonStageInfoText != null)
            {
                var stage = stageManager != null ? stageManager.CurrentStage : (session != null ? session.StageIndex : 1);
                var day = stageManager != null ? stageManager.CurrentDay : (session != null ? session.DayIndex : 1);
                prisonStageInfoText.text = $"Stage {stage}-{day}";
            }

            if (refreshPrisonInfo && prisonGoldInfoText != null)
            {
                prisonGoldInfoText.text = $"Gold {Math.Max(0, session != null ? session.Gold : 0)}";
            }

            if (refreshPrisonInfo && prisonDarkInfoText != null)
            {
                prisonDarkInfoText.text = $"Dark {Math.Max(0, session != null ? session.DarkTrace : 0)}";
            }
        }

        private void ResolveReferences()
        {
            if (stageManager == null)
            {
                stageManager = FindSceneObject<StageManager>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = FindSceneObject<UnitSpawnManager>();
            }

            if (combatManager == null)
            {
                combatManager = FindSceneObject<InGameCombatManager>();
            }
        }

        private void ResolveSceneUi()
        {
            rewardPanel = FindChildObject("RewardPanel");
            rewardButtonContainer = FindChild("RewardPanel/RewardBtnContainer");
            prisonerTemplateButton = FindButton("RewardPanel/RewardBtnContainer/PrisonerBtn");
            darkTemplateButton = FindButton("RewardPanel/RewardBtnContainer/DarkBtn");
            goldTemplateButton = FindButton("RewardPanel/RewardBtnContainer/GoldBtn");
            nextButton = FindButton("RewardPanel/NextBtn");
            rewardSummaryText = FindText("RewardPanel/Summary");

            prisonerChoicePopUp = FindChildObject("PrisonerChoicePopUp");
            ResolvePrisonPanelUi();

            var offeringPanel = FindChildObject("OfferingPanel");
            var offeringChoiceButtons = new[]
            {
                FindButton("OfferingPanel/Choice1"),
                FindButton("OfferingPanel/Choice2"),
                FindButton("OfferingPanel/Choice3")
            };

            offeringUI = new OfferingUI(
                offeringPanel,
                offeringChoiceButtons,
                ResolveSession,
                ResolveCatalog,
                ResolveCombatManager,
                () => activePrisonerButton,
                ConsumePrisonerButton,
                CompletePrisonAction,
                RefreshInfo);

            menifestUI = new MenifestUI(
                FindChildObject("MenifestedFailPopUp"),
                FindButton("MenifestedFailPopUp/Back"),
                FindChildObject("MenifestedSuccessPopUp"),
                FindButton("MenifestedSuccessPopUp/DontChoiceBtn"),
                FindButton("MenifestedSuccessPopUp/ChoiceBtn"),
                FindText("MenifestedSuccessPopUp/MonsterName"),
                FindText("MenifestedSuccessPopUp/MonsterDesc"),
                FindImage("MenifestedSuccessPopUp/MonsterImage"),
                ResolveSession,
                ResolveCatalog,
                ResolveStageManager,
                ResolveUnitSpawnManager,
                () => activePrisonerButton,
                ConsumePrisonerButton,
                CompletePrisonAction,
                RefreshInfo);

            stageInfoText = FindText("Info/StageInfo");
            goldInfoText = FindText("Info/Goldinfo");
            darkInfoText = FindText("Info/Darkinfo");
        }

        private void ResolvePrisonPanelUi()
        {
            prisonPanel = FindChildObject("PrisonPanel");
            prisonStageInfoText = FindText("PrisonPanel/StageSum");
            prisonGoldInfoText = FindText("PrisonPanel/Goldinfo");
            prisonDarkInfoText = FindText("PrisonPanel/Darkinfo");
            prisonPrisonerImage = FindImage("PrisonPanel/Prisonal/Image");
            prisonPrisonerNameText = FindText("PrisonPanel/Prisonal/Image/Name");

            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var slotPath = $"PrisonPanel/{i + 1}P";
                prisonPartySlots[i] = new PrisonPartySlotView(
                    FindImage($"{slotPath}/Image"),
                    FindText($"{slotPath}/Image/Name"),
                    FindButton($"{slotPath}/Button"),
                    FindChildObject($"{slotPath}/Button/Reinforcement"),
                    FindChildObject($"{slotPath}/Button/Menifested"));
            }
        }

        private void RefreshPrisonPanel()
        {
            RefreshInfo();

            var session = ResolveSession();
            var partyMonsterIds = ResolvePrisonPartyMonsterIds(session);
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var isOccupied = i < partyMonsterIds.Count;
                var isNextManifestSlot = partyMonsterIds.Count > 0
                    && partyMonsterIds.Count < PrisonPartySlotCount
                    && i == partyMonsterIds.Count;
                var monsterId = isOccupied ? partyMonsterIds[i] : string.Empty;
                prisonSlotMonsterIds[i] = monsterId;
                RefreshPrisonPartySlot(prisonPartySlots[i], monsterId, isOccupied, isNextManifestSlot);
            }

            RefreshSelectedPrisoner();
        }

        private List<string> ResolvePrisonPartyMonsterIds(RunSession session)
        {
            var monsterIds = new List<string>(PrisonPartySlotCount);
            if (session == null || string.IsNullOrWhiteSpace(session.SelectedMonsterId))
            {
                return monsterIds;
            }

            monsterIds.Add(session.SelectedMonsterId);
            for (var i = 0; i < session.ManifestedMonsterIds.Count && monsterIds.Count < PrisonPartySlotCount; i++)
            {
                var monsterId = session.ManifestedMonsterIds[i];
                if (!string.IsNullOrWhiteSpace(monsterId))
                {
                    monsterIds.Add(monsterId);
                }
            }

            return monsterIds;
        }

        private void RefreshPrisonPartySlot(
            PrisonPartySlotView slot,
            string monsterId,
            bool isOccupied,
            bool isNextManifestSlot)
        {
            if (slot == null)
            {
                return;
            }

            SetActive(slot.Image != null ? slot.Image.gameObject : null, isOccupied);
            SetActive(slot.Button != null ? slot.Button.gameObject : null, isOccupied || isNextManifestSlot);
            SetActive(slot.ReinforcementLabel, isOccupied);
            SetActive(slot.MenifestedLabel, isNextManifestSlot);

            if (slot.Button != null)
            {
                slot.Button.interactable = isOccupied || isNextManifestSlot;
            }

            if (!isOccupied)
            {
                return;
            }

            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            if (slot.NameText != null)
            {
                slot.NameText.text = monster != null && !string.IsNullOrWhiteSpace(monster.DisplayName)
                    ? monster.DisplayName
                    : monsterId;
            }

            if (slot.Image != null)
            {
                var portrait = ResolveMonsterPortrait(monsterId, monster);
                slot.Image.sprite = portrait;
                slot.Image.color = portrait != null ? Color.white : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void RefreshSelectedPrisoner()
        {
            var prisonerId = activePrisonerButton != null ? activePrisonerButton.PrisonerId : string.Empty;
            var hasPrisoner = !string.IsNullOrWhiteSpace(prisonerId);
            SetActive(prisonPrisonerImage != null ? prisonPrisonerImage.gameObject : null, hasPrisoner);
            if (!hasPrisoner)
            {
                return;
            }

            if (prisonPrisonerNameText != null)
            {
                prisonPrisonerNameText.text = ResolvePrisonerDisplayName(prisonerId);
            }

        }

        private Sprite ResolveMonsterPortrait(string monsterId, MonsterDefinition monster)
        {
            if (string.Equals(monsterId, "ariel", StringComparison.OrdinalIgnoreCase))
            {
                return arielPrisonPortrait;
            }

            if (string.Equals(monsterId, "eve", StringComparison.OrdinalIgnoreCase))
            {
                return evePrisonPortrait;
            }

            if (string.Equals(monsterId, "rin", StringComparison.OrdinalIgnoreCase))
            {
                return rinPrisonPortrait;
            }

            if (string.Equals(monsterId, "sein", StringComparison.OrdinalIgnoreCase))
            {
                return seinPrisonPortrait;
            }

            if (string.Equals(monsterId, "vega", StringComparison.OrdinalIgnoreCase))
            {
                return vegaPrisonPortrait;
            }

            return monster != null ? monster.UnitSprite : null;
        }

        private void BindStaticButtons()
        {
            BindButton(nextButton, ContinueToNextDay);

            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(prisonPartySlots[i]?.Button, () => ActivatePrisonPartySlot(capturedIndex));
            }
        }

        private void ActivatePrisonPartySlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= prisonPartySlots.Length)
            {
                return;
            }

            var monsterId = prisonSlotMonsterIds[slotIndex];
            if (!string.IsNullOrWhiteSpace(monsterId))
            {
                if (offeringUI != null && offeringUI.OpenOfferingPanel(monsterId))
                {
                    SetActive(prisonPanel, false);
                }

                return;
            }

            var session = ResolveSession();
            var occupiedCount = ResolvePrisonPartyMonsterIds(session).Count;
            if (slotIndex != occupiedCount || menifestUI == null || !menifestUI.TryManifestPrisoner())
            {
                return;
            }

            SetActive(prisonPanel, false);
        }

        private void CompletePrisonAction()
        {
            SetActive(prisonPanel, false);
            SetActive(prisonerChoicePopUp, false);
            offeringUI?.Hide();
            menifestUI?.Hide();
            SetActive(rewardPanel, true);
            RefreshInfo();
        }

        private void HideTransientPanels()
        {
            SetActive(rewardPanel, false);
            SetActive(prisonerChoicePopUp, false);
            SetActive(prisonPanel, false);
            offeringUI?.Hide();
            menifestUI?.Hide();
        }

        private Button CreateRewardButton(Button template, string namePrefix, int order)
        {
            if (template == null || rewardButtonContainer == null)
            {
                return null;
            }

            var button = Instantiate(template, rewardButtonContainer);
            button.gameObject.name = $"{namePrefix}_{order + 1}";
            button.gameObject.SetActive(true);
            button.onClick.RemoveAllListeners();
            ArrangeRewardButton(button, order);
            return button;
        }

        private RewardButtonView RegisterRewardButton(Button button, RewardKind kind, int amount, string prisonerId)
        {
            var view = new RewardButtonView(button, kind, amount, prisonerId);
            rewardButtons.Add(view);
            return view;
        }

        private void ArrangeRewardButton(Button button, int order)
        {
            var rect = button != null ? button.transform as RectTransform : null;
            var baseRect = prisonerTemplateButton != null ? prisonerTemplateButton.transform as RectTransform : null;
            if (rect == null || baseRect == null)
            {
                return;
            }

            rect.anchorMin = baseRect.anchorMin;
            rect.anchorMax = baseRect.anchorMax;
            rect.pivot = baseRect.pivot;
            rect.sizeDelta = baseRect.sizeDelta;
            var rowsPerColumn = Mathf.Max(1, rewardButtonRowsPerColumn);
            var column = order / rowsPerColumn;
            var row = order % rowsPerColumn;
            var x = rewardButtonFirstColumnPosition.x + (rewardButtonColumnSpacingX * column);
            var y = rewardButtonFirstColumnPosition.y - (rewardButtonRowSpacingY * row);
            rect.anchoredPosition = new Vector2(x, y);
        }

        private void ClearClonedRewardButtons()
        {
            for (var i = 0; i < rewardButtons.Count; i++)
            {
                var button = rewardButtons[i].Button;
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            rewardButtons.Clear();
            activePrisonerButton = null;
            SetTemplateActive(prisonerTemplateButton, false);
            SetTemplateActive(goldTemplateButton, false);
            SetTemplateActive(darkTemplateButton, false);
        }

        private void ConsumePrisonerButton()
        {
            if (activePrisonerButton != null)
            {
                activePrisonerButton.SetConsumed();
            }
        }

        private RunSession ResolveSession()
        {
            if (stageManager != null && stageManager.ActiveSession != null)
            {
                return stageManager.ActiveSession;
            }

            return unitSpawnManager != null ? unitSpawnManager.ActiveSession : null;
        }

        private GameDataCatalog ResolveCatalog()
        {
            return CsvDataLoader.CurrentCatalog;
        }

        private string ResolvePrisonerDisplayName(string prisonerId)
        {
            var enemy = CsvDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.DisplayName))
            {
                return enemy.DisplayName;
            }

            return string.IsNullOrWhiteSpace(prisonerId) ? "Unknown" : prisonerId;
        }

        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        private StageManager ResolveStageManager()
        {
            ResolveReferences();
            return stageManager;
        }

        private UnitSpawnManager ResolveUnitSpawnManager()
        {
            ResolveReferences();
            return unitSpawnManager;
        }

        private GameObject FindChildObject(string path)
        {
            var found = FindChild(path);
            return found != null ? found.gameObject : null;
        }

        private Transform FindChild(string path)
        {
            return transform.Find(path);
        }

        private Button FindButton(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private TMP_Text FindText(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        private Image FindImage(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Image>() : null;
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

        private static void SetButtonLabel(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            var label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static void SetTemplateActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static T FindSceneObject<T>() where T : UnityEngine.Object
        {
            var objects = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < objects.Length; i++)
            {
                var component = objects[i] as Component;
                if (component != null && component.gameObject.scene.IsValid())
                {
                    return objects[i];
                }
            }

            return null;
        }

        private sealed class PrisonPartySlotView
        {
            public PrisonPartySlotView(
                Image image,
                TMP_Text nameText,
                Button button,
                GameObject reinforcementLabel,
                GameObject menifestedLabel)
            {
                Image = image;
                NameText = nameText;
                Button = button;
                ReinforcementLabel = reinforcementLabel;
                MenifestedLabel = menifestedLabel;
            }

            public Image Image { get; }
            public TMP_Text NameText { get; }
            public Button Button { get; }
            public GameObject ReinforcementLabel { get; }
            public GameObject MenifestedLabel { get; }
        }

        internal sealed class RewardButtonView
        {
            private readonly Color originalColor;

            public RewardButtonView(Button button, RewardKind kind, int amount, string prisonerId)
            {
                Button = button;
                Kind = kind;
                Amount = amount;
                PrisonerId = prisonerId;
                originalColor = button != null && button.image != null ? button.image.color : Color.white;
            }

            public Button Button { get; }
            public RewardKind Kind { get; }
            public int Amount { get; }
            public string PrisonerId { get; }
            public bool Consumed { get; private set; }

            public void SetConsumed()
            {
                Consumed = true;
                if (Button == null)
                {
                    return;
                }

                Button.interactable = false;
                if (Button.image != null)
                {
                    Button.image.color = Color.Lerp(originalColor, Color.black, 0.55f);
                }
            }
        }

        internal enum RewardKind
        {
            Prisoner,
            Gold,
            DarkTrace
        }
    }

    internal sealed class OfferingUI
    {
        private const int MaxOfferingChoices = 3;
        private const int MaxAdditionalActiveSkillCount = 2;
        private const int MaxRunPassiveSkillCount = 5;

        private readonly System.Collections.Generic.List<OfferingChoiceView> offeringChoices =
            new System.Collections.Generic.List<OfferingChoiceView>();
        private readonly Button[] offeringChoiceButtons;
        private readonly OfferingButtonView[] offeringButtonViews;
        private readonly GameObject offeringPanel;
        private readonly Func<RunSession> resolveSession;
        private readonly Func<GameDataCatalog> resolveCatalog;
        private readonly Func<InGameCombatManager> resolveCombatManager;
        private readonly Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton;
        private readonly Action consumePrisonerButton;
        private readonly Action completePrisonAction;
        private readonly Action refreshInfo;

        public OfferingUI(
            GameObject offeringPanel,
            Button[] offeringChoiceButtons,
            Func<RunSession> resolveSession,
            Func<GameDataCatalog> resolveCatalog,
            Func<InGameCombatManager> resolveCombatManager,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton,
            Action consumePrisonerButton,
            Action completePrisonAction,
            Action refreshInfo)
        {
            this.offeringPanel = offeringPanel;
            this.offeringChoiceButtons = offeringChoiceButtons ?? Array.Empty<Button>();
            offeringButtonViews = ResolveButtonViews(this.offeringChoiceButtons);
            this.resolveSession = resolveSession;
            this.resolveCatalog = resolveCatalog;
            this.resolveCombatManager = resolveCombatManager;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.consumePrisonerButton = consumePrisonerButton;
            this.completePrisonAction = completePrisonAction;
            this.refreshInfo = refreshInfo;
        }

        public bool OpenOfferingPanel(string monsterId)
        {
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (activePrisonerButton == null
                || activePrisonerButton.Consumed
                || string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            BuildOfferingChoices(monsterId);
            if (offeringChoices.Count == 0)
            {
                Debug.LogWarning($"Offering has no available choices for monster '{monsterId}'.");
                return false;
            }

            SetActive(offeringPanel, true);

            for (var i = 0; i < offeringChoiceButtons.Length; i++)
            {
                var buttonView = i < offeringButtonViews.Length ? offeringButtonViews[i] : null;
                var button = buttonView != null ? buttonView.Button : null;
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                var hasChoice = i < offeringChoices.Count;
                button.gameObject.SetActive(hasChoice);
                button.interactable = hasChoice;
                if (!hasChoice)
                {
                    continue;
                }

                var capturedIndex = i;
                var choice = offeringChoices[i];
                BindChoiceButton(buttonView, choice);
                button.onClick.AddListener(() => CommitOfferingChoice(capturedIndex));
            }

            return true;
        }

        public void Hide()
        {
            SetActive(offeringPanel, false);
        }

        private void CommitOfferingChoice(int choiceIndex)
        {
            var session = resolveSession?.Invoke();
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (session == null
                || activePrisonerButton == null
                || activePrisonerButton.Consumed
                || choiceIndex < 0
                || choiceIndex >= offeringChoices.Count)
            {
                return;
            }

            var choice = offeringChoices[choiceIndex];
            session.ClaimPrisonerReward(activePrisonerButton.PrisonerId);
            session.RecordOfferingChoice(
                choice.MonsterId,
                choice.RewardId,
                choice.ChoiceId,
                choice.ActiveSkillId,
                choice.PassiveSkillId);
            if (choice.Kind == OfferingChoiceKind.Enhancement)
            {
                session.AccumulateReward(
                    choice.MonsterId,
                    choice.DamageMultiplier,
                    choice.MagazineBonus,
                    choice.ShotIntervalMultiplier,
                    choice.ReloadDurationMultiplier,
                    choice.MaxHealthBonus,
                    choice.StatusChanceBonus);
            }

            RefreshRuntimeSkillModels();
            consumePrisonerButton?.Invoke();
            SetActive(offeringPanel, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
        }

        private void BuildOfferingChoices(string monsterId)
        {
            offeringChoices.Clear();
            var session = resolveSession?.Invoke();
            if (session == null)
            {
                return;
            }

            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            if (monster == null)
            {
                return;
            }

            var state = session.EnsurePartyMemberState(monster);
            AddActiveSkillChoices(session, monster, state);
            AddPassiveSkillChoices(session, monster, state);
            AddEnhancementChoices(session, monster, state);

            ShuffleOfferingChoices();
            while (offeringChoices.Count > MaxOfferingChoices)
            {
                offeringChoices.RemoveAt(offeringChoices.Count - 1);
            }
        }

        private void AddActiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null
                || state == null
                || CountLearnedAdditionalActiveSkills(monster, state) >= MaxAdditionalActiveSkillCount)
            {
                return;
            }

            var skills = CsvDataLoader.CurrentCatalog.GetActiveSkills(monster.MonsterId, monster);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.SkillId) || session.HasLearnedActive(state.MonsterId, skill.SkillId))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    Kind = OfferingChoiceKind.ActiveSkill,
                    MonsterId = state.MonsterId,
                    ChoiceId = skill.SkillId,
                    ActiveSkillId = skill.SkillId,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(skill.DisplayName, skill.SkillId),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(skill.DisplayName, skill.SkillId)}",
                    Description = ResolveDescription(skill.Summary, skill.DescriptionText, skill.SkillId),
                    Icon = skill.SkillIcon
                });
            }
        }

        private void AddPassiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null || state.LearnedPassives.Count >= MaxRunPassiveSkillCount)
            {
                return;
            }

            var passives = CsvDataLoader.CurrentCatalog.GetPassiveSkills(monster.MonsterId, monster);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (passive == null || string.IsNullOrWhiteSpace(passive.PassiveId) || session.HasLearnedPassive(state.MonsterId, passive.PassiveId))
                {
                    continue;
                }

                if (!passive.IsAvailableWithoutActiveRequirement && !HasLearnedRequiredActive(session, monster, state, passive.RequiredActiveSlot))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    Kind = OfferingChoiceKind.PassiveSkill,
                    MonsterId = state.MonsterId,
                    ChoiceId = passive.PassiveId,
                    PassiveSkillId = passive.PassiveId,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(passive.DisplayName, passive.PassiveId),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(passive.DisplayName, passive.PassiveId)}",
                    Description = ResolveDescription(passive.Summary, passive.DescriptionText, passive.PassiveId),
                    Icon = passive.SkillIcon
                });
            }
        }

        private void AddEnhancementChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var rewards = CsvDataLoader.CurrentCatalog.GetRewardChoices(monster.MonsterId, monster);
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId) || session.HasChosenReward(state.MonsterId, reward.RewardId))
                {
                    continue;
                }

                var choiceData = ResolveChoice(reward.RewardId);
                if (choiceData == null
                    || !IsRewardChoiceAvailableForState(session, state, reward, choiceData))
                {
                    continue;
                }

                var skillName = BuildEnhancementSkillName(monster, reward, choiceData);
                offeringChoices.Add(new OfferingChoiceView
                {
                    Kind = OfferingChoiceKind.Enhancement,
                    MonsterId = state.MonsterId,
                    RewardId = reward.RewardId,
                    ChoiceId = reward.RewardId,
                    ActiveSkillId = reward.ActiveSkillId,
                    PassiveSkillId = reward.PassiveSkillId,
                    Summary = monster.DisplayName,
                    SkillName = skillName,
                    Title = $"{monster.DisplayName} · {skillName}",
                    Description = ResolveDescription(null, choiceData.DescriptionText, choiceData.ChoiceId),
                    Icon = ResolveChoiceIcon(choiceData),
                    DamageMultiplier = choiceData.HasDamageMultiplier ? choiceData.DamageMultiplier : 1f,
                    MagazineBonus = choiceData.HasMagazineBonus ? choiceData.MagazineBonus : 0,
                    ShotIntervalMultiplier = choiceData.HasShotIntervalMultiplier ? choiceData.ShotIntervalMultiplier : 1f,
                    ReloadDurationMultiplier = choiceData.HasReloadTimeMultiplier ? choiceData.ReloadTimeMultiplier : 1f,
                    MaxHealthBonus = choiceData.HasMaxHealthBonus ? choiceData.MaxHealthBonus : 0f,
                    StatusChanceBonus = choiceData.HasStatusChanceBonus ? choiceData.StatusChanceBonus : 0f
                });
            }
        }

        private static SkillChoiceDefinition ResolveChoice(string choiceId)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return null;
            }

            var manager = CsvDataLoader.CurrentCatalog;
            if (manager == null || !manager.TryGetData(choiceId, out SkillChoiceDefinition choice))
            {
                return null;
            }

            return choice;
        }

        private static Sprite ResolveChoiceIcon(SkillChoiceDefinition choice)
        {
            if (choice == null)
            {
                return null;
            }

            if (choice.SkillIcon != null)
            {
                return choice.SkillIcon;
            }

            var manager = CsvDataLoader.CurrentCatalog;
            if (manager == null || string.IsNullOrWhiteSpace(choice.SkillId))
            {
                return null;
            }

            if (manager.TryGetData(choice.SkillId, out SkillDefinition activeSkill) && activeSkill != null)
            {
                return activeSkill.SkillIcon;
            }

            if (manager.TryGetData(choice.SkillId, out PassiveDefinition passiveSkill) && passiveSkill != null)
            {
                return passiveSkill.SkillIcon;
            }

            return null;
        }

        private static string BuildEnhancementSkillName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition choice)
        {
            var sourceName = ResolveLinkedSkillDisplayName(monster, reward, choice);
            var choiceTitle = choice != null && !string.IsNullOrWhiteSpace(choice.Title)
                ? choice.Title.Trim()
                : choice != null
                    ? choice.ChoiceId
                    : string.Empty;

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return ResolveChoiceDisplayName(choiceTitle, choice != null ? choice.ChoiceId : string.Empty);
            }

            return string.IsNullOrWhiteSpace(choiceTitle) ? sourceName : $"{sourceName}·{choiceTitle}";
        }

        private static string ResolveLinkedSkillDisplayName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition choice)
        {
            var targetSkillId = choice != null ? choice.TargetSkillId : string.Empty;
            var choiceSkillId = choice != null ? choice.SkillId : string.Empty;
            var rewardActiveSkillId = reward != null ? reward.ActiveSkillId : string.Empty;
            var rewardPassiveSkillId = reward != null ? reward.PassiveSkillId : string.Empty;
            var id = !string.IsNullOrWhiteSpace(targetSkillId)
                ? targetSkillId
                : !string.IsNullOrWhiteSpace(choiceSkillId)
                    ? choiceSkillId
                    : !string.IsNullOrWhiteSpace(rewardActiveSkillId)
                        ? rewardActiveSkillId
                        : rewardPassiveSkillId;

            return ResolveSkillDisplayName(monster, id);
        }

        private static string ResolveSkillDisplayName(MonsterDefinition monster, string skillId)
        {
            if (string.IsNullOrWhiteSpace(skillId))
            {
                return string.Empty;
            }

            if (monster != null && monster.ActiveSkills != null)
            {
                for (var i = 0; i < monster.ActiveSkills.Length; i++)
                {
                    var skill = monster.ActiveSkills[i];
                    if (skill != null && string.Equals(skill.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                    {
                        return ResolveChoiceDisplayName(skill.DisplayName, skill.SkillId);
                    }
                }
            }

            if (monster != null && monster.PassiveSkills != null)
            {
                for (var i = 0; i < monster.PassiveSkills.Length; i++)
                {
                    var passive = monster.PassiveSkills[i];
                    if (passive != null && string.Equals(passive.PassiveId, skillId, StringComparison.OrdinalIgnoreCase))
                    {
                        return ResolveChoiceDisplayName(passive.DisplayName, passive.PassiveId);
                    }
                }
            }

            var manager = CsvDataLoader.CurrentCatalog;
            if (manager != null)
            {
                if (manager.TryGetData(skillId, out SkillDefinition activeSkill) && activeSkill != null)
                {
                    return ResolveChoiceDisplayName(activeSkill.DisplayName, activeSkill.SkillId);
                }

                if (manager.TryGetData(skillId, out PassiveDefinition passiveSkill) && passiveSkill != null)
                {
                    return ResolveChoiceDisplayName(passiveSkill.DisplayName, passiveSkill.PassiveId);
                }
            }

            return skillId;
        }

        private static string ResolveChoiceDisplayName(string displayName, string fallback)
        {
            return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName.Trim();
        }

        private static bool IsRewardChoiceAvailableForState(
            RunSession session,
            RunSession.RunMonsterState state,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition choice)
        {
            if (session == null || state == null || reward == null || choice == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.ActiveSkillId)
                && !session.HasLearnedActive(state.MonsterId, reward.ActiveSkillId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.PassiveSkillId)
                && !session.HasLearnedPassive(state.MonsterId, reward.PassiveSkillId))
            {
                return false;
            }

            var targetSkillId = !string.IsNullOrWhiteSpace(choice.SkillId)
                ? choice.SkillId
                : !string.IsNullOrWhiteSpace(reward.ActiveSkillId)
                    ? reward.ActiveSkillId
                    : reward.PassiveSkillId;

            switch (choice.ChoiceGroup)
            {
                case SkillChoiceGroup.ActiveEnhancement:
                    return CountChosenChoices(state, targetSkillId, SkillChoiceGroup.ActiveEnhancement) < 3;
                case SkillChoiceGroup.ActiveMaster:
                    return CountChosenChoices(state, targetSkillId, SkillChoiceGroup.ActiveEnhancement) >= 3
                        && CountChosenChoices(state, targetSkillId, SkillChoiceGroup.ActiveMaster) < 1;
                case SkillChoiceGroup.PassiveEnhancement:
                    return CountChosenChoices(state, targetSkillId, SkillChoiceGroup.PassiveEnhancement) < 1;
                default:
                    return true;
            }
        }

        private void RefreshRuntimeSkillModels()
        {
            var combatManager = resolveCombatManager?.Invoke();
            var session = resolveSession?.Invoke();
            if (combatManager == null || session == null)
            {
                return;
            }

            var players = combatManager.Roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i] != null ? players[i].Model as MonsterUnitRuntimeModel : null;
                if (model != null)
                {
                    SyncModelStateFromSession(session, model);
                    SkillRuntimeFactory.RebuildLearnedActiveSet(model);
                    combatManager.Roster.RefreshActor(model);
                }
            }

            RefreshSceneMonsterActorSkillModels(session);
        }

        private static void RefreshSceneMonsterActorSkillModels(RunSession session)
        {
            var actors = Resources.FindObjectsOfTypeAll<MonsterUnitActor>();
            for (var i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                if (actor == null || actor.gameObject == null || !actor.gameObject.scene.IsValid())
                {
                    continue;
                }

                var model = actor.Model;
                if (model == null)
                {
                    continue;
                }

                SyncModelStateFromSession(session, model);
                SkillRuntimeFactory.RebuildLearnedActiveSet(model);
                actor.RefreshDebugView();
            }
        }

        private static void SyncModelStateFromSession(RunSession session, MonsterUnitRuntimeModel model)
        {
            if (session == null || model == null || model.Identity == null)
            {
                return;
            }

            var monsterId = model.Identity.DefinitionId;
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var state = session.GetPartyMemberState(monsterId);
            if (state == null)
            {
                return;
            }

            if (model.State == null)
            {
                model.State = new UnitStateBucket();
            }

            CopyListToSet(state.LearnedActives, model.State.LearnedActiveSkillIds);
            CopyListToSet(state.LearnedPassives, model.State.LearnedPassiveSkillIds);
            CopyListToSet(state.ChosenChoiceIds, model.State.ChosenChoiceIds);
        }

        private static void CopyListToSet(System.Collections.Generic.IReadOnlyList<string> source, System.Collections.Generic.ISet<string> target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.Clear();
            for (var i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }

        private bool HasLearnedRequiredActive(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state, SkillSlot slot)
        {
            var skills = CsvDataLoader.CurrentCatalog.GetActiveSkills(monster.MonsterId, monster);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill != null && skill.Slot == slot && session.HasLearnedActive(state.MonsterId, skill.SkillId))
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

        private static string ResolveDescription(string summary, string description, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            return string.IsNullOrWhiteSpace(summary) ? fallback : summary;
        }

        private static int CountChosenChoices(
            RunSession.RunMonsterState state,
            string skillId,
            SkillChoiceGroup group)
        {
            if (state == null || string.IsNullOrWhiteSpace(skillId))
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < state.ChosenChoiceIds.Count; i++)
            {
                var chosen = ResolveChoice(state.ChosenChoiceIds[i]);
                if (chosen != null
                    && chosen.ChoiceGroup == group
                    && string.Equals(chosen.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountLearnedAdditionalActiveSkills(MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null || state.LearnedActives == null || state.LearnedActives.Count == 0)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < state.LearnedActives.Count; i++)
            {
                var skillId = state.LearnedActives[i];
                if (string.IsNullOrWhiteSpace(skillId) || IsDefaultActiveSkill(monster, skillId))
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        private static bool IsDefaultActiveSkill(MonsterDefinition monster, string skillId)
        {
            if (monster == null || monster.ActiveSkills == null || string.IsNullOrWhiteSpace(skillId))
            {
                return false;
            }

            for (var i = 0; i < monster.ActiveSkills.Length; i++)
            {
                var skill = monster.ActiveSkills[i];
                if (skill == null || !string.Equals(skill.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return skill.IsDefaultLearned || skill.Slot == SkillSlot.A;
            }

            return false;
        }

        private static OfferingButtonView[] ResolveButtonViews(Button[] buttons)
        {
            if (buttons == null || buttons.Length == 0)
            {
                return Array.Empty<OfferingButtonView>();
            }

            var views = new OfferingButtonView[buttons.Length];
            for (var i = 0; i < buttons.Length; i++)
            {
                views[i] = OfferingButtonView.FromButton(buttons[i]);
            }

            return views;
        }

        private static void BindChoiceButton(OfferingButtonView view, OfferingChoiceView choice)
        {
            if (view == null || view.Button == null || choice == null)
            {
                return;
            }

            if (view.SummaryLabel != null)
            {
                view.SummaryLabel.text = choice.Summary;
            }

            if (view.SkillNameLabel != null)
            {
                view.SkillNameLabel.text = choice.SkillName;
            }

            if (view.TitleLabel != null && view.SkillNameLabel == null)
            {
                view.TitleLabel.text = choice.Title;
            }

            if (view.DescriptionLabel != null)
            {
                view.DescriptionLabel.text = choice.Description;
            }

            if (view.FallbackLabel != null && view.DescriptionLabel == null && view.SkillNameLabel == null)
            {
                view.FallbackLabel.text = $"{choice.Title}\n{choice.Description}";
            }

            if (view.IconImage != null)
            {
                view.IconImage.sprite = choice.Icon;
                view.IconImage.enabled = choice.Icon != null;
                if (view.IconImage.gameObject != null)
                {
                    view.IconImage.gameObject.SetActive(choice.Icon != null);
                }
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private enum OfferingChoiceKind
        {
            ActiveSkill,
            PassiveSkill,
            Enhancement
        }

        private sealed class OfferingChoiceView
        {
            public OfferingChoiceKind Kind;
            public string MonsterId;
            public string RewardId;
            public string ChoiceId;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public string Summary;
            public string SkillName;
            public string Title;
            public string Description;
            public Sprite Icon;
            public float DamageMultiplier = 1f;
            public int MagazineBonus;
            public float ShotIntervalMultiplier = 1f;
            public float ReloadDurationMultiplier = 1f;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
        }

        private sealed class OfferingButtonView
        {
            public Button Button;
            public TMP_Text SummaryLabel;
            public TMP_Text SkillNameLabel;
            public TMP_Text TitleLabel;
            public TMP_Text DescriptionLabel;
            public TMP_Text FallbackLabel;
            public Image IconImage;

            public static OfferingButtonView FromButton(Button button)
            {
                if (button == null)
                {
                    return null;
                }

                var view = new OfferingButtonView
                {
                    Button = button,
                    SummaryLabel = FindChildComponent<TMP_Text>(button.transform, "Summary"),
                    SkillNameLabel = FindChildComponent<TMP_Text>(button.transform, "SkillName"),
                    TitleLabel = FindChildComponent<TMP_Text>(button.transform, "Text (TMP)"),
                    DescriptionLabel = FindChildComponent<TMP_Text>(button.transform, "Desc"),
                    IconImage = FindChildComponent<Image>(button.transform, "Icon")
                };
                view.FallbackLabel = view.TitleLabel;
                return view;
            }

            private static T FindChildComponent<T>(Transform root, string childName)
                where T : Component
            {
                if (root == null || string.IsNullOrWhiteSpace(childName))
                {
                    return null;
                }

                var transforms = root.GetComponentsInChildren<Transform>(true);
                for (var i = 0; i < transforms.Length; i++)
                {
                    var candidate = transforms[i];
                    if (candidate != null
                        && string.Equals(candidate.name, childName, StringComparison.Ordinal))
                    {
                        var component = candidate.GetComponent<T>();
                        if (component != null)
                        {
                            return component;
                        }
                    }
                }

                return null;
            }
        }
    }

    internal sealed class MenifestUI
    {
        private readonly GameObject manifestedFailPopUp;
        private readonly Button manifestedFailBackButton;
        private readonly GameObject manifestedSuccessPopUp;
        private readonly Button dontChoiceButton;
        private readonly Button choiceButton;
        private readonly TMP_Text monsterNameText;
        private readonly TMP_Text monsterDescText;
        private readonly Image monsterImage;
        private readonly Func<RunSession> resolveSession;
        private readonly Func<GameDataCatalog> resolveCatalog;
        private readonly Func<StageManager> resolveStageManager;
        private readonly Func<UnitSpawnManager> resolveUnitSpawnManager;
        private readonly Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton;
        private readonly Action consumePrisonerButton;
        private readonly Action completePrisonAction;
        private readonly Action refreshInfo;

        private MonsterDefinition pendingManifestMonster;

        public MenifestUI(
            GameObject manifestedFailPopUp,
            Button manifestedFailBackButton,
            GameObject manifestedSuccessPopUp,
            Button dontChoiceButton,
            Button choiceButton,
            TMP_Text monsterNameText,
            TMP_Text monsterDescText,
            Image monsterImage,
            Func<RunSession> resolveSession,
            Func<GameDataCatalog> resolveCatalog,
            Func<StageManager> resolveStageManager,
            Func<UnitSpawnManager> resolveUnitSpawnManager,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton,
            Action consumePrisonerButton,
            Action completePrisonAction,
            Action refreshInfo)
        {
            this.manifestedFailPopUp = manifestedFailPopUp;
            this.manifestedFailBackButton = manifestedFailBackButton;
            this.manifestedSuccessPopUp = manifestedSuccessPopUp;
            this.dontChoiceButton = dontChoiceButton;
            this.choiceButton = choiceButton;
            this.monsterNameText = monsterNameText;
            this.monsterDescText = monsterDescText;
            this.monsterImage = monsterImage;
            this.resolveSession = resolveSession;
            this.resolveCatalog = resolveCatalog;
            this.resolveStageManager = resolveStageManager;
            this.resolveUnitSpawnManager = resolveUnitSpawnManager;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.consumePrisonerButton = consumePrisonerButton;
            this.completePrisonAction = completePrisonAction;
            this.refreshInfo = refreshInfo;

            BindButton(this.manifestedFailBackButton, CompleteAfterFailure);
            BindButton(this.dontChoiceButton, SkipManifestChoice);
            BindButton(this.choiceButton, CommitManifestChoice);
        }

        public bool TryManifestPrisoner()
        {
            var session = resolveSession?.Invoke();
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return false;
            }

            session.ClaimPrisonerReward(activePrisonerButton.PrisonerId);
            consumePrisonerButton?.Invoke();

            pendingManifestMonster = ResolveNextManifestCandidate(session);
            var stageManager = resolveStageManager?.Invoke();
            var successChance = stageManager != null ? stageManager.PendingManifestSuccessChance : 0.7f;
            var succeeded = pendingManifestMonster != null && UnityEngine.Random.value < successChance;
            if (!succeeded)
            {
                SetActive(manifestedFailPopUp, true);
                return true;
            }

            ShowManifestSuccessPopup(pendingManifestMonster);
            return true;
        }

        public void Hide()
        {
            SetActive(manifestedFailPopUp, false);
            SetActive(manifestedSuccessPopUp, false);
        }

        private void ShowManifestSuccessPopup(MonsterDefinition monster)
        {
            SetActive(manifestedSuccessPopUp, true);

            if (monsterNameText != null)
            {
                monsterNameText.text = monster != null ? monster.DisplayName : "Unknown";
            }

            if (monsterDescText != null)
            {
                monsterDescText.text = BuildManifestDescription(monster);
            }

            if (monsterImage != null)
            {
                monsterImage.sprite = monster != null ? monster.UnitSprite : null;
                monsterImage.color = monster != null && monster.UnitSprite != null ? Color.white : new Color(0f, 0f, 0f, 0.3f);
            }
        }

        private void SkipManifestChoice()
        {
            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            completePrisonAction?.Invoke();
        }

        private void CompleteAfterFailure()
        {
            pendingManifestMonster = null;
            SetActive(manifestedFailPopUp, false);
            completePrisonAction?.Invoke();
        }

        private void CommitManifestChoice()
        {
            var session = resolveSession?.Invoke();
            if (session == null || pendingManifestMonster == null)
            {
                return;
            }

            session.RecordManifestedMonster(pendingManifestMonster);
            var slotIndex = Mathf.Clamp(session.ManifestedMonsterIds.Count, 1, 4);
            var unitSpawnManager = resolveUnitSpawnManager?.Invoke();
            if (unitSpawnManager != null)
            {
                unitSpawnManager.SpawnManifestedMonster(pendingManifestMonster, slotIndex);
            }

            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
        }

        private MonsterDefinition ResolveNextManifestCandidate(RunSession session)
        {
            var monsters = CsvDataLoader.CurrentCatalog.GetMonsters();
            var candidates = new System.Collections.Generic.List<MonsterDefinition>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null
                    || string.IsNullOrWhiteSpace(monster.MonsterId)
                    || string.Equals(monster.MonsterId, session.SelectedMonsterId, StringComparison.OrdinalIgnoreCase)
                    || session.HasManifestedMonster(monster.MonsterId))
                {
                    continue;
                }

                candidates.Add(monster);
            }

            return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : null;
        }

        private static string BuildManifestDescription(MonsterDefinition monster)
        {
            if (monster == null)
            {
                return string.Empty;
            }

            return
                $"{monster.RoleSummary}\n" +
                $"속성: {monster.ElementLabel}\n" +
                $"HP: {monster.MaxHealth:0} / 전투력: {monster.PowerStat:0}\n" +
                $"A: {monster.ActiveSkillName} / F: {monster.PassiveSkillName}";
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
