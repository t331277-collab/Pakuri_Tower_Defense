/*
 * 역할: 주 InGame UI 조정.
 * 책임: 씬 Control을 연결하고 스킬 Slot·자원·Stage·보상·선택·Panel을 갱신한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    /// InGameUIManager가 담당하는 작업을 조정하고 공유 런타임 상태를 소유한다.
    public class InGameUIManager : MonoBehaviour
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

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindStaticButtons();
            HideTransientPanels();
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
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

        /// RewardPanel를 표시한다.
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

        /// 전달된 view 값을 사용해 OpenPrisonPanel 작업을 수행한다.
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

        /// 전달된 런타임 입력값을 사용해 ClaimMaterialReward 작업을 수행한다.
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

        /// ContinueToNextDay 작업을 수행한다.
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

        /// Info를 현재 런타임 모델을 기준으로 갱신한다.
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

        /// References를 결정한다.
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

        /// SceneUi를 결정한다.
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

        /// PrisonPanelUi를 결정한다.
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

        /// PrisonPanel를 현재 런타임 모델을 기준으로 갱신한다.
        private void RefreshPrisonPanel()
        {
            RefreshInfo();

            var session = ResolveSession();
            var partyMembers = session != null ? session.PartyMembers : null;
            var occupiedCount = partyMembers != null
                ? Math.Min(partyMembers.Count, PrisonPartySlotCount)
                : 0;
            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var isOccupied = i < occupiedCount;
                var isNextManifestSlot = occupiedCount > 0
                    && occupiedCount < PrisonPartySlotCount
                    && i == occupiedCount;
                var monsterId = isOccupied ? partyMembers[i].MonsterId : string.Empty;
                prisonSlotMonsterIds[i] = monsterId;
                RefreshPrisonPartySlot(prisonPartySlots[i], monsterId, isOccupied, isNextManifestSlot);
            }

            RefreshSelectedPrisoner();
        }

        /// 전달된 런타임 입력값을 사용해 PrisonPartySlot를 현재 런타임 모델을 기준으로 갱신한다.
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

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
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

        /// SelectedPrisoner를 현재 런타임 모델을 기준으로 갱신한다.
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

        /// 전달된 런타임 입력값을 사용해 MonsterPortrait를 결정한다.
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

            if (monster != null)
            {
                return monster.MonsterIconImage;
            }

            return null;
        }

        /// StaticButtons를 런타임 사건 또는 씬 대상에 연결한다.
        private void BindStaticButtons()
        {
            BindButton(nextButton, ContinueToNextDay);

            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(prisonPartySlots[i]?.Button, () => ActivatePrisonPartySlot(capturedIndex));
            }
        }

        /// 전달된 slotIndex 값을 사용해 ActivatePrisonPartySlot 작업을 수행한다.
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
            var occupiedCount = session != null
                ? Math.Min(session.PartyMembers.Count, PrisonPartySlotCount)
                : 0;
            if (slotIndex != occupiedCount || menifestUI == null || !menifestUI.TryManifestPrisoner())
            {
                return;
            }

            SetActive(prisonPanel, false);
        }

        /// PrisonAction를 완료한다.
        private void CompletePrisonAction()
        {
            SetActive(prisonPanel, false);
            SetActive(prisonerChoicePopUp, false);
            offeringUI?.Hide();
            menifestUI?.Hide();
            SetActive(rewardPanel, true);
            RefreshInfo();
        }

        /// TransientPanels를 숨긴다.
        private void HideTransientPanels()
        {
            SetActive(rewardPanel, false);
            SetActive(prisonerChoicePopUp, false);
            SetActive(prisonPanel, false);
            offeringUI?.Hide();
            menifestUI?.Hide();
        }

        /// 전달된 런타임 입력값을 사용해 RewardButton를 생성한다.
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

        /// 전달된 런타임 입력값을 사용해 RewardButton를 소유 런타임 Registry에 등록한다.
        private RewardButtonView RegisterRewardButton(Button button, RewardKind kind, int amount, string prisonerId)
        {
            var view = new RewardButtonView(button, kind, amount, prisonerId);
            rewardButtons.Add(view);
            return view;
        }

        /// 전달된 런타임 입력값을 사용해 ArrangeRewardButton 작업을 수행한다.
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

        /// ClonedRewardButtons를 소유한 런타임 상태에서 비운다.
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

        /// PrisonerButton를 현재 런타임 상태에서 소비한다.
        private void ConsumePrisonerButton()
        {
            if (activePrisonerButton != null)
            {
                activePrisonerButton.SetConsumed();
            }
        }

        /// Session를 결정한다.
        private RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        /// Catalog를 결정한다.
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /// 전달된 prisonerId 값을 사용해 PrisonerDisplayName를 결정한다.
        private string ResolvePrisonerDisplayName(string prisonerId)
        {
            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.DisplayName))
            {
                return enemy.DisplayName;
            }

            return string.IsNullOrWhiteSpace(prisonerId) ? "Unknown" : prisonerId;
        }

        /// CombatManager를 결정한다.
        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        /// StageManager를 결정한다.
        private StageManager ResolveStageManager()
        {
            ResolveReferences();
            return stageManager;
        }

        /// UnitSpawnManager를 결정한다.
        private UnitSpawnManager ResolveUnitSpawnManager()
        {
            ResolveReferences();
            return unitSpawnManager;
        }

        /// 전달된 path 값을 사용해 ChildObject를 찾는다.
        private GameObject FindChildObject(string path)
        {
            var found = FindChild(path);
            return found != null ? found.gameObject : null;
        }

        /// 전달된 path 값을 사용해 Child를 찾는다.
        private Transform FindChild(string path)
        {
            return transform.Find(path);
        }

        /// 전달된 path 값을 사용해 Button를 찾는다.
        private Button FindButton(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Button>() : null;
        }

        /// 전달된 path 값을 사용해 Text를 찾는다.
        private TMP_Text FindText(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        /// 전달된 path 값을 사용해 Image를 찾는다.
        private Image FindImage(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Image>() : null;
        }

        /// 전달된 런타임 입력값을 사용해 Button를 런타임 사건 또는 씬 대상에 연결한다.
        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /// 전달된 런타임 입력값을 사용해 ButtonLabel를 갱신한다.
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

        /// 전달된 런타임 입력값을 사용해 TemplateActive를 갱신한다.
        private static void SetTemplateActive(Button button, bool active)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        /// 전달된 런타임 입력값을 사용해 Active를 갱신한다.
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        /// SceneObject를 찾는다.
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

        /// PrisonPartySlotView가 소유하는 데이터와 동작을 캡슐화한다.
        private class PrisonPartySlotView
        {

            /// PrisonPartySlotView 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

        /// RewardButtonView가 소유하는 데이터와 동작을 캡슐화한다.
        internal class RewardButtonView
        {
            private readonly Color originalColor;

            /// RewardButtonView 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

            /// Consumed를 갱신한다.
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

        /// RewardKind에서 지원하는 값의 종류를 정의한다.
        internal enum RewardKind
        {
            Prisoner,
            Gold,
            DarkTrace
        }
    }

    /// OfferingUI 상태를 Unity UI 또는 월드 오브젝트로 표시한다.
    internal class OfferingUI
    {
        private const int MaxOfferingChoices = 3;

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

        /// OfferingUI 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

        /// 전달된 monsterId 값을 사용해 OpenOfferingPanel 조건을 평가하고 결과를 반환한다.
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

        /// 요청값를 숨긴다.
        public void Hide()
        {
            SetActive(offeringPanel, false);
        }

        /// 전달된 choiceIndex 값을 사용해 CommitOfferingChoice 작업을 수행한다.
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
            var state = session.GetPartyMemberState(choice.MonsterId);
            if (state == null)
            {
                return;
            }

            session.RecordOfferingChoice(
                state,
                choice.RewardId,
                choice.ChoiceId,
                choice.ActiveSkillId,
                choice.PassiveSkillId);

            RefreshRuntimeSkillModels();
            consumePrisonerButton?.Invoke();
            SetActive(offeringPanel, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
        }

        /// 전달된 monsterId 값을 사용해 OfferingChoices를 구성한다.
        private void BuildOfferingChoices(string monsterId)
        {
            offeringChoices.Clear();
            var session = resolveSession?.Invoke();
            if (session == null)
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            if (monster == null)
            {
                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (state == null)
            {
                return;
            }

            AddActiveSkillChoices(session, monster, state);
            AddPassiveSkillChoices(session, monster, state);
            AddEnhancementChoices(session, monster, state);

            ShuffleOfferingChoices();
            while (offeringChoices.Count > MaxOfferingChoices)
            {
                offeringChoices.RemoveAt(offeringChoices.Count - 1);
            }
        }

        /// 전달된 런타임 입력값을 사용해 ActiveSkillChoices를 소유한 런타임 상태에 추가한다.
        private void AddActiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var skills = GameDataLoader.CurrentCatalog.GetActiveSkills(monster.MonsterId);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (!session.CanLearnActive(state, monster, skill))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterId = state.MonsterId,
                    ActiveSkillId = skill.SkillId,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(skill.SkillName, skill.SkillId),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(skill.SkillName, skill.SkillId)}",
                    Description = ResolveDescription(skill.Summary, skill.Description, skill.SkillId),
                    Icon = skill.Icon
                });
            }
        }

        /// 전달된 런타임 입력값을 사용해 PassiveSkillChoices를 소유한 런타임 상태에 추가한다.
        private void AddPassiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var passives = GameDataLoader.CurrentCatalog.GetPassiveSkills(monster.MonsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (!session.CanLearnPassive(state, monster, passive))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterId = state.MonsterId,
                    PassiveSkillId = passive.SkillId,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(passive.SkillName, passive.SkillId),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(passive.SkillName, passive.SkillId)}",
                    Description = ResolveDescription(passive.Summary, passive.Description, passive.SkillId),
                    Icon = passive.Icon
                });
            }
        }

        /// 전달된 런타임 입력값을 사용해 EnhancementChoices를 소유한 런타임 상태에 추가한다.
        private void AddEnhancementChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var rewards = GameDataLoader.CurrentCatalog.GetRewardChoices(monster.MonsterId);
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward == null
                    || string.IsNullOrWhiteSpace(reward.RewardId)
                    || state.ChosenRewardIds.Contains(reward.RewardId))
                {
                    continue;
                }

                var choiceData = ResolveChoice(reward.RewardId);
                if (choiceData == null
                    || !session.CanChooseSkillChoice(state, reward, choiceData))
                {
                    continue;
                }

                var skillName = BuildEnhancementSkillName(monster, reward, choiceData);
                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterId = state.MonsterId,
                    RewardId = reward.RewardId,
                    ChoiceId = reward.RewardId,
                    ActiveSkillId = reward.ActiveSkillId,
                    PassiveSkillId = reward.PassiveSkillId,
                    Summary = monster.DisplayName,
                    SkillName = skillName,
                    Title = $"{monster.DisplayName} · {skillName}",
                    Description = ResolveDescription(null, choiceData.DescriptionText, choiceData.ChoiceId),
                    Icon = ResolveChoiceIcon(choiceData)
                });
            }
        }

        /// 전달된 choiceId 값을 사용해 Choice를 결정한다.
        private static SkillChoice ResolveChoice(string choiceId)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return null;
            }

            var manager = GameDataLoader.CurrentCatalog;
            if (manager == null || !manager.TryGetData(choiceId, out SkillChoice choice))
            {
                return null;
            }

            return choice;
        }

        /// 전달된 choice 값을 사용해 ChoiceIcon를 결정한다.
        private static Sprite ResolveChoiceIcon(SkillChoice choice)
        {
            if (choice == null)
            {
                return null;
            }

            if (choice.SkillIcon != null)
            {
                return choice.SkillIcon;
            }

            var manager = GameDataLoader.CurrentCatalog;
            if (manager == null || string.IsNullOrWhiteSpace(choice.SkillId))
            {
                return null;
            }

            if (manager.TryGetData(choice.SkillId, out SkillDefinition activeSkill) && activeSkill != null)
            {
                return activeSkill.Icon;
            }

            if (manager.TryGetData(choice.SkillId, out PassiveSkillDefinition passiveSkill) && passiveSkill != null)
            {
                return passiveSkill.Icon;
            }

            return null;
        }

        /// 전달된 런타임 입력값을 사용해 EnhancementSkillName를 구성한다.
        private static string BuildEnhancementSkillName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoice choice)
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

        /// 전달된 런타임 입력값을 사용해 LinkedSkillDisplayName를 결정한다.
        private static string ResolveLinkedSkillDisplayName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoice choice)
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

        /// 전달된 런타임 입력값을 사용해 SkillDisplayName를 결정한다.
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
                        return ResolveChoiceDisplayName(skill.SkillName, skill.SkillId);
                    }
                }
            }

            if (monster != null && monster.PassiveSkills != null)
            {
                for (var i = 0; i < monster.PassiveSkills.Length; i++)
                {
                    var passive = monster.PassiveSkills[i];
                    if (passive != null && string.Equals(passive.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                    {
                        return ResolveChoiceDisplayName(passive.SkillName, passive.SkillId);
                    }
                }
            }

            var manager = GameDataLoader.CurrentCatalog;
            if (manager != null)
            {
                if (manager.TryGetData(skillId, out SkillDefinition activeSkill) && activeSkill != null)
                {
                    return ResolveChoiceDisplayName(activeSkill.SkillName, activeSkill.SkillId);
                }

                if (manager.TryGetData(skillId, out PassiveSkillDefinition passiveSkill) && passiveSkill != null)
                {
                    return ResolveChoiceDisplayName(passiveSkill.SkillName, passiveSkill.SkillId);
                }
            }

            return skillId;
        }

        /// 전달된 런타임 입력값을 사용해 ChoiceDisplayName를 결정한다.
        private static string ResolveChoiceDisplayName(string displayName, string fallback)
        {
            return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName.Trim();
        }

        /// RuntimeSkillModels를 현재 런타임 모델을 기준으로 갱신한다.
        private void RefreshRuntimeSkillModels()
        {
            var combatManager = resolveCombatManager?.Invoke();
            var units = combatManager != null ? combatManager.Units : null;
            if (units == null)
            {
                return;
            }

            var players = units.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                if (entry != null && entry.Model.Identity.Role == UnitRole.Monster)
                {
                    var model = entry.Model;
                    SkillExecution.RebuildLearnedSkillState(model);
                    units.RefreshDisplay(model);
                }
            }

            RefreshSceneMonsterActorSkillModels();
        }

        /// SceneMonsterActorSkillModels를 현재 런타임 모델을 기준으로 갱신한다.
        private static void RefreshSceneMonsterActorSkillModels()
        {
            var actors = Resources.FindObjectsOfTypeAll<MonsterActor>();
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

                SkillExecution.RebuildLearnedSkillState(model);
                actor.RefreshDisplay();
            }
        }

        /// ShuffleOfferingChoices 작업을 수행한다.
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

        /// 전달된 런타임 입력값을 사용해 Description를 결정한다.
        private static string ResolveDescription(string summary, string description, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            return string.IsNullOrWhiteSpace(summary) ? fallback : summary;
        }

        /// 전달된 buttons 값을 사용해 ButtonViews를 결정한다.
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

        /// 전달된 런타임 입력값을 사용해 ChoiceButton를 런타임 사건 또는 씬 대상에 연결한다.
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

        /// 전달된 런타임 입력값을 사용해 Active를 갱신한다.
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        /// OfferingChoiceView가 소유하는 데이터와 동작을 캡슐화한다.
        private class OfferingChoiceView
        {
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
        }

        /// OfferingButtonView가 소유하는 데이터와 동작을 캡슐화한다.
        private class OfferingButtonView
        {
            public Button Button;
            public TMP_Text SummaryLabel;
            public TMP_Text SkillNameLabel;
            public TMP_Text TitleLabel;
            public TMP_Text DescriptionLabel;
            public TMP_Text FallbackLabel;
            public Image IconImage;

            /// 전달된 button 값을 사용해 FromButton 결과값을 생성해 반환한다.
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

            /// 전달된 런타임 입력값을 사용해 ChildComponent를 찾는다.
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

    /// MenifestUI 상태를 Unity UI 또는 월드 오브젝트로 표시한다.
    internal class MenifestUI
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

        /// MenifestUI 인스턴스를 전달된 런타임 입력값으로 초기화한다.
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

        /// ManifestPrisoner 작업을 시도하고 성공 여부를 반환한다.
        public bool TryManifestPrisoner()
        {
            var session = resolveSession?.Invoke();
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return false;
            }

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

        /// 요청값를 숨긴다.
        public void Hide()
        {
            SetActive(manifestedFailPopUp, false);
            SetActive(manifestedSuccessPopUp, false);
        }

        /// 전달된 monster 값을 사용해 ManifestSuccessPopup를 표시한다.
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
                monsterImage.sprite = null;
                monsterImage.color = new Color(0f, 0f, 0f, 0.3f);
                if (monster != null && monster.MonsterIconImage != null)
                {
                    monsterImage.sprite = monster.MonsterIconImage;
                    monsterImage.color = Color.white;
                }
            }
        }

        /// SkipManifestChoice 작업을 수행한다.
        private void SkipManifestChoice()
        {
            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            completePrisonAction?.Invoke();
        }

        /// AfterFailure를 완료한다.
        private void CompleteAfterFailure()
        {
            pendingManifestMonster = null;
            SetActive(manifestedFailPopUp, false);
            completePrisonAction?.Invoke();
        }

        /// CommitManifestChoice 작업을 수행한다.
        private void CommitManifestChoice()
        {
            var session = resolveSession?.Invoke();
            if (session == null || pendingManifestMonster == null)
            {
                return;
            }

            if (!session.TryAddPartyMonster(pendingManifestMonster, out var slotIndex))
            {
                return;
            }

            var unitSpawnManager = resolveUnitSpawnManager?.Invoke();
            if (unitSpawnManager != null)
            {
                unitSpawnManager.SpawnManifestedMonster(session, pendingManifestMonster, slotIndex);
            }

            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
        }

        /// 전달된 session 값을 사용해 NextManifestCandidate를 결정한다.
        private MonsterDefinition ResolveNextManifestCandidate(RunSession session)
        {
            var monsters = GameDataLoader.CurrentCatalog.GetMonsters();
            var candidates = new System.Collections.Generic.List<MonsterDefinition>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null
                    || string.IsNullOrWhiteSpace(monster.MonsterId)
                    || session.GetPartyMemberState(monster.MonsterId) != null)
                {
                    continue;
                }

                candidates.Add(monster);
            }

            return candidates.Count > 0 ? candidates[UnityEngine.Random.Range(0, candidates.Count)] : null;
        }

        /// 전달된 monster 값을 사용해 ManifestDescription를 구성한다.
        private static string BuildManifestDescription(MonsterDefinition monster)
        {
            if (monster == null)
            {
                return string.Empty;
            }

            return
                $"{monster.RoleSummary}\n" +
                $"속성: {monster.ElementLabel}\n" +
                $"HP: {monster.BaseStats.MaxHealth:0} / 전투력: {monster.PowerStat:0}\n" +
                $"A: {monster.ActiveSkillName} / F: {monster.PassiveSkillName}";
        }

        /// 전달된 런타임 입력값을 사용해 Button를 런타임 사건 또는 씬 대상에 연결한다.
        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /// 전달된 런타임 입력값을 사용해 Active를 갱신한다.
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
