using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
 * 인게임 진행과 보상 선택 화면을 한 흐름으로 연결하는 UI 컴포넌트.
 * 스테이지·재화·감옥 파티 정보를 갱신하고 전투 보상 버튼을 구성하며
 * Offering의 스킬·강화 선택과 포로 Menifest 결과를 RunSession과 전투 모델에 반영한다.
 */
namespace Pakuri.InGame
{
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

        /*
         * Unity가 컴포넌트를 초기화할 때 필요한 참조와 상태를 준비한다.
         */
        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindStaticButtons();
            HideTransientPanels();
        }

        /*
         * 매 프레임 현재 상태를 갱신한다.
         */
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

        /*
         * ShowRewardPanel 작업을 수행한다.
         */
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

        /*
         * OpenPrisonPanel 작업을 수행한다.
         */
        private void OpenPrisonPanel(RewardButtonView view /* 화면 요소 */)
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

        /*
         * ClaimMaterialReward 작업을 수행한다.
         */
        private void ClaimMaterialReward(RewardButtonView view /* 화면 요소 */, int gold /* 골드 */, int darkTrace /* 어둠 흔적 */)
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

        /*
         * ContinueToNextDay 작업을 수행한다.
         */
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

        /*
         * RefreshInfo 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * ResolveReferences에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * ResolveSceneUi에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * ResolvePrisonPanelUi에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * RefreshPrisonPanel 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * ResolvePrisonPartyMonsterIds 결과를 계산해 반환한다.
         */
        private List<string> ResolvePrisonPartyMonsterIds(RunSession session /* 현재 게임 진행 상태 */)
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

        /*
         * RefreshPrisonPartySlot 대상의 현재 상태를 갱신한다.
         */
        private void RefreshPrisonPartySlot(
            PrisonPartySlotView slot /* 스킬이나 유닛이 배치될 슬롯 */,
            string monsterId /* 몬스터 식별자 */,
            bool isOccupied /* 여부 사용 중인 여부 */,
            bool isNextManifestSlot /* 여부 다음 현현 슬롯 여부 */)
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

            var monster = GameDataLoader.CurrentCatalog.ResolveMonster(monsterId);
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

        /*
         * RefreshSelectedPrisoner 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * ResolveMonsterPortrait 결과를 계산해 반환한다.
         */
        private Sprite ResolveMonsterPortrait(string monsterId /* 몬스터 식별자 */, MonsterDefinition monster /* 몬스터 */)
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

        /*
         * BindStaticButtons에 필요한 값을 설정한다.
         */
        private void BindStaticButtons()
        {
            BindButton(nextButton, ContinueToNextDay);

            for (var i = 0; i < prisonPartySlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(prisonPartySlots[i]?.Button, () => ActivatePrisonPartySlot(capturedIndex));
            }
        }

        /*
         * ActivatePrisonPartySlot 작업을 수행한다.
         */
        private void ActivatePrisonPartySlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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

        /*
         * CompletePrisonAction 작업을 수행한다.
         */
        private void CompletePrisonAction()
        {
            SetActive(prisonPanel, false);
            SetActive(prisonerChoicePopUp, false);
            offeringUI?.Hide();
            menifestUI?.Hide();
            SetActive(rewardPanel, true);
            RefreshInfo();
        }

        /*
         * HideTransientPanels 작업을 수행한다.
         */
        private void HideTransientPanels()
        {
            SetActive(rewardPanel, false);
            SetActive(prisonerChoicePopUp, false);
            SetActive(prisonPanel, false);
            offeringUI?.Hide();
            menifestUI?.Hide();
        }

        /*
         * CreateRewardButton에 필요한 결과를 만들어 반환한다.
         */
        private Button CreateRewardButton(Button template /* 원본 틀 */, string namePrefix /* 이름 접두어 */, int order /* 순서 */)
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

        /*
         * RegisterRewardButton 작업 결과를 반환한다.
         */
        private RewardButtonView RegisterRewardButton(Button button /* 연결하거나 갱신할 버튼 */, RewardKind kind /* 처리할 종류 */, int amount /* 적용할 수치 */, string prisonerId /* 수감자 식별자 */)
        {
            var view = new RewardButtonView(button, kind, amount, prisonerId);
            rewardButtons.Add(view);
            return view;
        }

        /*
         * ArrangeRewardButton 작업을 수행한다.
         */
        private void ArrangeRewardButton(Button button /* 연결하거나 갱신할 버튼 */, int order /* 순서 */)
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

        /*
         * ClearClonedRewardButtons 작업을 수행한다.
         */
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

        /*
         * ConsumePrisonerButton 작업을 수행한다.
         */
        private void ConsumePrisonerButton()
        {
            if (activePrisonerButton != null)
            {
                activePrisonerButton.SetConsumed();
            }
        }

        /*
         * ResolveSession 결과를 계산해 반환한다.
         */
        private RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        /*
         * ResolveCatalog 결과를 계산해 반환한다.
         */
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /*
         * ResolvePrisonerDisplayName 결과를 계산해 반환한다.
         */
        private string ResolvePrisonerDisplayName(string prisonerId /* 수감자 식별자 */)
        {
            var enemy = GameDataLoader.CurrentCatalog.GetData<EnemyDefinition>(prisonerId);
            if (enemy != null && !string.IsNullOrWhiteSpace(enemy.DisplayName))
            {
                return enemy.DisplayName;
            }

            return string.IsNullOrWhiteSpace(prisonerId) ? "Unknown" : prisonerId;
        }

        /*
         * ResolveCombatManager 결과를 계산해 반환한다.
         */
        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        /*
         * ResolveStageManager 결과를 계산해 반환한다.
         */
        private StageManager ResolveStageManager()
        {
            ResolveReferences();
            return stageManager;
        }

        /*
         * ResolveUnitSpawnManager 결과를 계산해 반환한다.
         */
        private UnitSpawnManager ResolveUnitSpawnManager()
        {
            ResolveReferences();
            return unitSpawnManager;
        }

        /*
         * FindChildObject에 해당하는 값을 찾아 반환한다.
         */
        private GameObject FindChildObject(string path /* 불러오거나 검사할 경로 */)
        {
            var found = FindChild(path);
            return found != null ? found.gameObject : null;
        }

        /*
         * FindChild에 해당하는 값을 찾아 반환한다.
         */
        private Transform FindChild(string path /* 불러오거나 검사할 경로 */)
        {
            return transform.Find(path);
        }

        /*
         * FindButton에 해당하는 값을 찾아 반환한다.
         */
        private Button FindButton(string path /* 불러오거나 검사할 경로 */)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Button>() : null;
        }

        /*
         * FindText에 해당하는 값을 찾아 반환한다.
         */
        private TMP_Text FindText(string path /* 불러오거나 검사할 경로 */)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<TMP_Text>() : null;
        }

        /*
         * FindImage에 해당하는 값을 찾아 반환한다.
         */
        private Image FindImage(string path /* 불러오거나 검사할 경로 */)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Image>() : null;
        }

        /*
         * BindButton에 필요한 값을 설정한다.
         */
        private static void BindButton(Button button /* 연결하거나 갱신할 버튼 */, UnityEngine.Events.UnityAction action /* 동작 */)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /*
         * SetButtonLabel에 필요한 값을 설정한다.
         */
        private static void SetButtonLabel(Button button /* 연결하거나 갱신할 버튼 */, string text /* 텍스트 */)
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

        /*
         * SetTemplateActive에 필요한 값을 설정한다.
         */
        private static void SetTemplateActive(Button button /* 연결하거나 갱신할 버튼 */, bool active /* 대상 활성화 여부 */)
        {
            if (button != null)
            {
                button.gameObject.SetActive(active);
            }
        }

        /*
         * SetActive에 필요한 값을 설정한다.
         */
        private static void SetActive(GameObject target /* 활성화하거나 변경할 게임 오브젝트 */, bool active /* 대상 활성화 여부 */)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        /*
         * FindSceneObject에 해당하는 값을 찾아 반환한다.
         */
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

        private class PrisonPartySlotView
        {
            /*
             * PrisonPartySlotView에 필요한 값을 초기화한다.
             */
            public PrisonPartySlotView(
                Image image /* 이미지 */,
                TMP_Text nameText /* 이름 텍스트 */,
                Button button /* 연결하거나 갱신할 버튼 */,
                GameObject reinforcementLabel /* 증원 표시 문구 */,
                GameObject menifestedLabel /* 현현 상태 표시 문구 */)
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

        internal class RewardButtonView
        {
            private readonly Color originalColor;

            /*
             * RewardButtonView에 필요한 값을 초기화한다.
             */
            public RewardButtonView(Button button /* 연결하거나 갱신할 버튼 */, RewardKind kind /* 처리할 종류 */, int amount /* 적용할 수치 */, string prisonerId /* 수감자 식별자 */)
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

            /*
             * SetConsumed에 필요한 값을 설정한다.
             */
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

        /*
         * OfferingUI에 필요한 값을 초기화한다.
         */
        public OfferingUI(
            GameObject offeringPanel /* 공물 패널 */,
            Button[] offeringChoiceButtons /* 공물 선택지 버튼 목록 */,
            Func<RunSession> resolveSession /* 찾기 게임 진행 상태 */,
            Func<GameDataCatalog> resolveCatalog /* 찾기 데이터 목록 */,
            Func<InGameCombatManager> resolveCombatManager /* 찾기 전투 관리자 */,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton /* 찾기 활성 수감자 버튼 */,
            Action consumePrisonerButton /* 소모 수감자 버튼 */,
            Action completePrisonAction /* 완료 감옥 동작 */,
            Action refreshInfo /* 갱신 정보 */)
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

        /*
         * OpenOfferingPanel 작업 결과를 반환한다.
         */
        public bool OpenOfferingPanel(string monsterId /* 몬스터 식별자 */)
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

        /*
         * Hide 작업을 수행한다.
         */
        public void Hide()
        {
            SetActive(offeringPanel, false);
        }

        /*
         * CommitOfferingChoice 작업을 수행한다.
         */
        private void CommitOfferingChoice(int choiceIndex /* 선택지 순서 번호 */)
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
            if (choice.MaxHealthBonus != 0f)
            {
                session.AddMaxHealthBonus(
                    choice.MonsterId,
                    choice.MaxHealthBonus);
            }

            RefreshRuntimeSkillModels();
            consumePrisonerButton?.Invoke();
            SetActive(offeringPanel, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
        }

        /*
         * BuildOfferingChoices에 필요한 결과를 구성한다.
         */
        private void BuildOfferingChoices(string monsterId /* 몬스터 식별자 */)
        {
            offeringChoices.Clear();
            var session = resolveSession?.Invoke();
            if (session == null)
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.ResolveMonster(monsterId);
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

        /*
         * AddActiveSkillChoices 작업을 수행한다.
         */
        private void AddActiveSkillChoices(RunSession session /* 현재 게임 진행 상태 */, MonsterDefinition monster /* 몬스터 */, RunSession.RunMonsterState state /* 상태 */)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var skills = GameDataLoader.CurrentCatalog.GetActiveSkills(monster.MonsterId);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (!session.CanLearnActive(monster, skill))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
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

        /*
         * AddPassiveSkillChoices 작업을 수행한다.
         */
        private void AddPassiveSkillChoices(RunSession session /* 현재 게임 진행 상태 */, MonsterDefinition monster /* 몬스터 */, RunSession.RunMonsterState state /* 상태 */)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var passives = GameDataLoader.CurrentCatalog.GetPassiveSkills(monster.MonsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (!session.CanLearnPassive(monster, passive))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
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

        /*
         * AddEnhancementChoices 작업을 수행한다.
         */
        private void AddEnhancementChoices(RunSession session /* 현재 게임 진행 상태 */, MonsterDefinition monster /* 몬스터 */, RunSession.RunMonsterState state /* 상태 */)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var rewards = GameDataLoader.CurrentCatalog.GetRewardChoices(monster.MonsterId);
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId) || session.HasChosenReward(state.MonsterId, reward.RewardId))
                {
                    continue;
                }

                var choiceData = ResolveChoice(reward.RewardId);
                if (choiceData == null
                    || !session.CanChooseSkillChoice(state.MonsterId, reward, choiceData))
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
                    Icon = ResolveChoiceIcon(choiceData),
                    MaxHealthBonus = choiceData.MaxHealthBonus
                });
            }
        }

        /*
         * ResolveChoice 결과를 계산해 반환한다.
         */
        private static SkillChoiceDefinition ResolveChoice(string choiceId /* 스킬 선택지 식별자 */)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return null;
            }

            var manager = GameDataLoader.CurrentCatalog;
            if (manager == null || !manager.TryGetData(choiceId, out SkillChoiceDefinition choice))
            {
                return null;
            }

            return choice;
        }

        /*
         * ResolveChoiceIcon 결과를 계산해 반환한다.
         */
        private static Sprite ResolveChoiceIcon(SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */)
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
                return activeSkill.SkillIcon;
            }

            if (manager.TryGetData(choice.SkillId, out PassiveDefinition passiveSkill) && passiveSkill != null)
            {
                return passiveSkill.SkillIcon;
            }

            return null;
        }

        /*
         * BuildEnhancementSkillName에 필요한 결과를 만들어 반환한다.
         */
        private static string BuildEnhancementSkillName(
            MonsterDefinition monster /* 몬스터 */,
            MonsterDefinition.RewardChoiceDefinition reward /* 보상 */,
            SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */)
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

        /*
         * ResolveLinkedSkillDisplayName 결과를 계산해 반환한다.
         */
        private static string ResolveLinkedSkillDisplayName(
            MonsterDefinition monster /* 몬스터 */,
            MonsterDefinition.RewardChoiceDefinition reward /* 보상 */,
            SkillChoiceDefinition choice /* 적용하거나 검사할 스킬 선택지 */)
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

        /*
         * ResolveSkillDisplayName 결과를 계산해 반환한다.
         */
        private static string ResolveSkillDisplayName(MonsterDefinition monster /* 몬스터 */, string skillId /* 스킬 식별자 */)
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

            var manager = GameDataLoader.CurrentCatalog;
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

        /*
         * ResolveChoiceDisplayName 결과를 계산해 반환한다.
         */
        private static string ResolveChoiceDisplayName(string displayName /* 표시 이름 */, string fallback /* 기본 결과가 없을 때 사용할 값 */)
        {
            return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName.Trim();
        }

        /*
         * RefreshRuntimeSkillModels 대상의 현재 상태를 갱신한다.
         */
        private void RefreshRuntimeSkillModels()
        {
            var combatManager = resolveCombatManager?.Invoke();
            var session = resolveSession?.Invoke();
            if (combatManager == null || session == null)
            {
                return;
            }

            var players = combatManager.UnitRegistry.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                if (entry != null && entry.Model.Identity.Role == UnitRole.Monster)
                {
                    var model = entry.Model;
                    SyncModelStateFromSession(session, model);
                    UnitSkillsBuilder.RebuildLearnedSkillSet(model);
                    combatManager.UnitRegistry.RefreshDisplay(model);
                }
            }

            RefreshSceneMonsterActorSkillModels(session);
        }

        /*
         * RefreshSceneMonsterActorSkillModels 대상의 현재 상태를 갱신한다.
         */
        private static void RefreshSceneMonsterActorSkillModels(RunSession session /* 현재 게임 진행 상태 */)
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

                SyncModelStateFromSession(session, model);
                UnitSkillsBuilder.RebuildLearnedSkillSet(model);
                actor.RefreshDisplay();
            }
        }

        /*
         * SyncModelStateFromSession 대상의 현재 상태를 갱신한다.
         */
        private static void SyncModelStateFromSession(RunSession session /* 현재 게임 진행 상태 */, UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
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

            if (model.Skills == null)
            {
                model.Skills = new UnitSkills();
            }

            CopyListToSet(state.LearnedActives, model.Skills.LearnedActiveSkillIds);
            CopyListToSet(state.LearnedPassives, model.Skills.LearnedPassiveSkillIds);
            CopyListToSet(state.ChosenChoiceIds, model.Skills.ChosenChoiceIds);
        }

        /*
         * CopyListToSet에 필요한 값을 복사한다.
         */
        private static void CopyListToSet(System.Collections.Generic.IReadOnlyList<string> source /* 효과를 발생시킨 원본 */, System.Collections.Generic.ISet<string> target /* 처리할 대상 */)
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

        /*
         * ShuffleOfferingChoices 작업을 수행한다.
         */
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

        /*
         * ResolveDescription 결과를 계산해 반환한다.
         */
        private static string ResolveDescription(string summary /* 요약 */, string description /* 설명 */, string fallback /* 기본 결과가 없을 때 사용할 값 */)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            return string.IsNullOrWhiteSpace(summary) ? fallback : summary;
        }

        /*
         * ResolveButtonViews 결과를 계산해 반환한다.
         */
        private static OfferingButtonView[] ResolveButtonViews(Button[] buttons /* 버튼 목록 */)
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

        /*
         * BindChoiceButton에 필요한 값을 설정한다.
         */
        private static void BindChoiceButton(OfferingButtonView view /* 화면 요소 */, OfferingChoiceView choice /* 적용하거나 검사할 스킬 선택지 */)
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

        /*
         * SetActive에 필요한 값을 설정한다.
         */
        private static void SetActive(GameObject target /* 활성화하거나 변경할 게임 오브젝트 */, bool active /* 대상 활성화 여부 */)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

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
            public float MaxHealthBonus;
        }

        private class OfferingButtonView
        {
            public Button Button;
            public TMP_Text SummaryLabel;
            public TMP_Text SkillNameLabel;
            public TMP_Text TitleLabel;
            public TMP_Text DescriptionLabel;
            public TMP_Text FallbackLabel;
            public Image IconImage;

            /*
             * FromButton 작업 결과를 반환한다.
             */
            public static OfferingButtonView FromButton(Button button /* 연결하거나 갱신할 버튼 */)
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

            /*
             * FindChildComponent에 해당하는 값을 찾아 반환한다.
             */
            private static T FindChildComponent<T>(Transform root /* 검색이나 배치의 기준 오브젝트 */, string childName /* 자식 오브젝트 이름 */)
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

        /*
         * MenifestUI에 필요한 값을 초기화한다.
         */
        public MenifestUI(
            GameObject manifestedFailPopUp /* 현현 실패 알림창 */,
            Button manifestedFailBackButton /* 현현 실패 화면의 뒤로 가기 버튼 */,
            GameObject manifestedSuccessPopUp /* 현현된 성공 제거 후속 */,
            Button dontChoiceButton /* 선택하지 않음 버튼 */,
            Button choiceButton /* 선택지 버튼 */,
            TMP_Text monsterNameText /* 몬스터 이름 텍스트 */,
            TMP_Text monsterDescText /* 몬스터 설명 텍스트 */,
            Image monsterImage /* 몬스터 이미지 */,
            Func<RunSession> resolveSession /* 찾기 게임 진행 상태 */,
            Func<GameDataCatalog> resolveCatalog /* 찾기 데이터 목록 */,
            Func<StageManager> resolveStageManager /* 찾기 스테이지 관리자 */,
            Func<UnitSpawnManager> resolveUnitSpawnManager /* 찾기 유닛 생성 관리자 */,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton /* 찾기 활성 수감자 버튼 */,
            Action consumePrisonerButton /* 소모 수감자 버튼 */,
            Action completePrisonAction /* 완료 감옥 동작 */,
            Action refreshInfo /* 갱신 정보 */)
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

        /*
         * TryManifestPrisoner 작업을 시도하고 성공 여부를 반환한다.
         */
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

        /*
         * Hide 작업을 수행한다.
         */
        public void Hide()
        {
            SetActive(manifestedFailPopUp, false);
            SetActive(manifestedSuccessPopUp, false);
        }

        /*
         * ShowManifestSuccessPopup 작업을 수행한다.
         */
        private void ShowManifestSuccessPopup(MonsterDefinition monster /* 몬스터 */)
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

        /*
         * SkipManifestChoice 작업을 수행한다.
         */
        private void SkipManifestChoice()
        {
            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            completePrisonAction?.Invoke();
        }

        /*
         * CompleteAfterFailure 작업을 수행한다.
         */
        private void CompleteAfterFailure()
        {
            pendingManifestMonster = null;
            SetActive(manifestedFailPopUp, false);
            completePrisonAction?.Invoke();
        }

        /*
         * CommitManifestChoice 작업을 수행한다.
         */
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
                unitSpawnManager.SpawnManifestedMonster(session, pendingManifestMonster, slotIndex);
            }

            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            refreshInfo?.Invoke();
            completePrisonAction?.Invoke();
        }

        /*
         * ResolveNextManifestCandidate 결과를 계산해 반환한다.
         */
        private MonsterDefinition ResolveNextManifestCandidate(RunSession session /* 현재 게임 진행 상태 */)
        {
            var monsters = GameDataLoader.CurrentCatalog.GetMonsters();
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

        /*
         * BuildManifestDescription에 필요한 결과를 만들어 반환한다.
         */
        private static string BuildManifestDescription(MonsterDefinition monster /* 몬스터 */)
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

        /*
         * BindButton에 필요한 값을 설정한다.
         */
        private static void BindButton(Button button /* 연결하거나 갱신할 버튼 */, UnityEngine.Events.UnityAction action /* 동작 */)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /*
         * SetActive에 필요한 값을 설정한다.
         */
        private static void SetActive(GameObject target /* 활성화하거나 변경할 게임 오브젝트 */, bool active /* 대상 활성화 여부 */)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
