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
        private readonly List<RewardButtonView> rewardButtons = new List<RewardButtonView>();

        [SerializeField] private NewRunStageManager stageManager;
        [SerializeField] private NewRunSceneEntryManager entryManager;
        [SerializeField] private InGameCombatManager combatManager;

        private GameObject rewardPanel;
        private Transform rewardButtonContainer;
        private Button prisonerTemplateButton;
        private Button goldTemplateButton;
        private Button darkTemplateButton;
        private Button nextButton;
        private TMP_Text rewardSummaryText;
        private GameObject prisonerChoicePopUp;
        private Button offeringButton;
        private Button menifestedButton;
        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;
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

            if (stageManager == null || stageManager.State != NewRunStageState.RewardReady)
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
                rewardSummaryText.text = $"Stage {stageManager.CurrentStage}-{stageManager.CurrentDay} 蹂댁긽";
            }

            var order = 0;
            var prisoners = stageManager.PendingPrisonerEnemyIds;
            for (var i = 0; i < prisoners.Count; i++)
            {
                var capturedIndex = i;
                var button = CreateRewardButton(prisonerTemplateButton, "PrisonerReward", order++);
                SetButtonLabel(button, $"?щ줈\n{prisoners[i]}");
                var view = RegisterRewardButton(button, RewardKind.Prisoner, 0, prisoners[i]);
                button.onClick.AddListener(() => OpenPrisonerChoice(view, capturedIndex));
            }

            if (stageManager.PendingGoldReward > 0)
            {
                var amount = stageManager.PendingGoldReward;
                var button = CreateRewardButton(goldTemplateButton, "GoldReward", order++);
                SetButtonLabel(button, $"怨⑤뱶\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.Gold, amount, string.Empty);
                button.onClick.AddListener(() => ClaimMaterialReward(view, amount, 0));
            }

            if (stageManager.PendingDarkTraceReward > 0)
            {
                var amount = stageManager.PendingDarkTraceReward;
                var button = CreateRewardButton(darkTemplateButton, "DarkTraceReward", order++);
                SetButtonLabel(button, $"?대몺???붿쟻\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.DarkTrace, amount, string.Empty);
                button.onClick.AddListener(() => ClaimMaterialReward(view, 0, amount));
            }

            SetTemplateActive(prisonerTemplateButton, false);
            SetTemplateActive(goldTemplateButton, false);
            SetTemplateActive(darkTemplateButton, false);
            RefreshInfo();
        }

        private void OpenPrisonerChoice(RewardButtonView view, int rewardIndex)
        {
            if (view == null || view.Consumed)
            {
                return;
            }

            activePrisonerButton = view;
            SetActive(prisonerChoicePopUp, true);
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
        }

        private void ResolveReferences()
        {
            if (stageManager == null)
            {
                stageManager = FindSceneObject<NewRunStageManager>();
            }

            if (entryManager == null)
            {
                entryManager = FindSceneObject<NewRunSceneEntryManager>();
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
            offeringButton = FindButton("PrisonerChoicePopUp/OfferingBtn");
            menifestedButton = FindButton("PrisonerChoicePopUp/Menifested");

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
                prisonerChoicePopUp,
                rewardPanel,
                ResolveSession,
                ResolveCatalog,
                ResolveCombatManager,
                () => activePrisonerButton,
                ConsumePrisonerButton,
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
                prisonerChoicePopUp,
                ResolveSession,
                ResolveCatalog,
                ResolveStageManager,
                ResolveEntryManager,
                () => activePrisonerButton,
                ConsumePrisonerButton,
                RefreshInfo);

            stageInfoText = FindText("Info/StageInfo");
            goldInfoText = FindText("Info/Goldinfo");
            darkInfoText = FindText("Info/Darkinfo");
        }

        private void BindStaticButtons()
        {
            BindButton(nextButton, ContinueToNextDay);
            BindButton(offeringButton, () => offeringUI?.OpenOfferingPanel());
            BindButton(menifestedButton, () => menifestUI?.TryManifestPrisoner());
        }

        private void HideTransientPanels()
        {
            SetActive(rewardPanel, false);
            SetActive(prisonerChoicePopUp, false);
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

            var spacing = ResolveRewardButtonSpacing(baseRect);
            rect.anchorMin = baseRect.anchorMin;
            rect.anchorMax = baseRect.anchorMax;
            rect.pivot = baseRect.pivot;
            rect.sizeDelta = baseRect.sizeDelta;
            rect.anchoredPosition = baseRect.anchoredPosition + new Vector2(0f, -spacing * order);
        }

        private float ResolveRewardButtonSpacing(RectTransform baseRect)
        {
            var referenceRect = goldTemplateButton != null ? goldTemplateButton.transform as RectTransform : null;
            if (referenceRect != null)
            {
                var delta = Mathf.Abs(baseRect.anchoredPosition.y - referenceRect.anchoredPosition.y);
                if (delta > 1f)
                {
                    return delta;
                }
            }

            return Mathf.Max(1f, baseRect.sizeDelta.y + 16f);
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

            return entryManager != null ? entryManager.ActiveSession : null;
        }

        private GameDataCatalog ResolveCatalog()
        {
            var catalog = PakuriDataManager.Instance.CurrentCatalog;
            return catalog != null ? catalog : PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
        }

        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        private NewRunStageManager ResolveStageManager()
        {
            ResolveReferences();
            return stageManager;
        }

        private NewRunSceneEntryManager ResolveEntryManager()
        {
            ResolveReferences();
            return entryManager;
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
}
