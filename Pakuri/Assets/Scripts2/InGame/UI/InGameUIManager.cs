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

        [SerializeField] private StageManager stageManager;
        [SerializeField] private SceneEntryManager entryManager;
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
                var capturedIndex = i;
                var prisonerId = prisoners[i];
                var button = CreateRewardButton(prisonerTemplateButton, "PrisonerReward", order++);
                SetButtonLabel(button, $"Prisoner\n{ResolvePrisonerDisplayName(prisonerId)}");
                var view = RegisterRewardButton(button, RewardKind.Prisoner, 0, prisonerId);
                button.onClick.AddListener(() => OpenPrisonerChoice(view, capturedIndex));
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
                stageManager = FindSceneObject<StageManager>();
            }

            if (entryManager == null)
            {
                entryManager = FindSceneObject<SceneEntryManager>();
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
            BindButton(offeringButton, OpenOfferingFromPrisonerChoice);
            BindButton(menifestedButton, TryManifestFromPrisonerChoice);
        }

        private void OpenOfferingFromPrisonerChoice()
        {
            offeringUI?.OpenOfferingPanel();
            SetActive(prisonerChoicePopUp, false);
        }

        private void TryManifestFromPrisonerChoice()
        {
            menifestUI?.TryManifestPrisoner();
            SetActive(prisonerChoicePopUp, false);
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

        private string ResolvePrisonerDisplayName(string prisonerId)
        {
            var catalog = ResolveCatalog();
            var enemy = catalog != null ? catalog.GetStageOneEnemyById(prisonerId) : null;
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

        private SceneEntryManager ResolveEntryManager()
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

    internal sealed class OfferingUI
    {
        private const int MaxOfferingChoices = 3;
        private const int MaxRunActiveSkillCount = 5;
        private const int MaxRunPassiveSkillCount = 5;

        private readonly System.Collections.Generic.List<OfferingChoiceView> offeringChoices =
            new System.Collections.Generic.List<OfferingChoiceView>();
        private readonly Button[] offeringChoiceButtons;
        private readonly GameObject offeringPanel;
        private readonly GameObject prisonerChoicePopUp;
        private readonly GameObject rewardPanel;
        private readonly Func<RunSession> resolveSession;
        private readonly Func<GameDataCatalog> resolveCatalog;
        private readonly Func<InGameCombatManager> resolveCombatManager;
        private readonly Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton;
        private readonly Action consumePrisonerButton;
        private readonly Action refreshInfo;

        public OfferingUI(
            GameObject offeringPanel,
            Button[] offeringChoiceButtons,
            GameObject prisonerChoicePopUp,
            GameObject rewardPanel,
            Func<RunSession> resolveSession,
            Func<GameDataCatalog> resolveCatalog,
            Func<InGameCombatManager> resolveCombatManager,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton,
            Action consumePrisonerButton,
            Action refreshInfo)
        {
            this.offeringPanel = offeringPanel;
            this.offeringChoiceButtons = offeringChoiceButtons ?? Array.Empty<Button>();
            this.prisonerChoicePopUp = prisonerChoicePopUp;
            this.rewardPanel = rewardPanel;
            this.resolveSession = resolveSession;
            this.resolveCatalog = resolveCatalog;
            this.resolveCombatManager = resolveCombatManager;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.consumePrisonerButton = consumePrisonerButton;
            this.refreshInfo = refreshInfo;
        }

        public void OpenOfferingPanel()
        {
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return;
            }

            BuildOfferingChoices();
            SetActive(offeringPanel, true);

            for (var i = 0; i < offeringChoiceButtons.Length; i++)
            {
                var button = offeringChoiceButtons[i];
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
                SetButtonLabel(button, $"{choice.Title}\n{choice.Description}");
                button.onClick.AddListener(() => CommitOfferingChoice(capturedIndex));
            }
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
                choice.LinkedChoiceId,
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
            SetActive(prisonerChoicePopUp, false);
            SetActive(rewardPanel, true);
            refreshInfo?.Invoke();
        }

        private void BuildOfferingChoices()
        {
            offeringChoices.Clear();
            var session = resolveSession?.Invoke();
            if (session == null)
            {
                return;
            }

            var targets = ResolveOfferingTargets(session);
            for (var i = 0; i < targets.Count; i++)
            {
                var monster = targets[i];
                var state = session.EnsurePartyMemberState(monster);
                AddActiveSkillChoices(session, monster, state);
                AddPassiveSkillChoices(session, monster, state);
                AddEnhancementChoices(session, monster, state);
            }

            ShuffleOfferingChoices();
            while (offeringChoices.Count > MaxOfferingChoices)
            {
                offeringChoices.RemoveAt(offeringChoices.Count - 1);
            }
        }

        private void AddActiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null || state.LearnedActives.Count >= MaxRunActiveSkillCount)
            {
                return;
            }

            var skills = PakuriDataManager.Instance.GetActiveSkills(monster.MonsterId, monster);
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
                    ActiveSkillId = skill.SkillId,
                    Title = $"{monster.DisplayName}\n{skill.DisplayName}",
                    Description = ResolveDescription(skill.Summary, skill.DescriptionText, skill.SkillId)
                });
            }
        }

        private void AddPassiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null || state.LearnedPassives.Count >= MaxRunPassiveSkillCount)
            {
                return;
            }

            var passives = PakuriDataManager.Instance.GetPassiveSkills(monster.MonsterId, monster);
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
                    PassiveSkillId = passive.PassiveId,
                    Title = $"{monster.DisplayName}\n{passive.DisplayName}",
                    Description = ResolveDescription(passive.Summary, passive.DescriptionText, passive.PassiveId)
                });
            }
        }

        private void AddEnhancementChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var rewards = PakuriDataManager.Instance.GetRewardChoices(monster.MonsterId, monster);
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId) || session.HasChosenReward(state.MonsterId, reward.RewardId))
                {
                    continue;
                }

                if (!IsRewardChoiceAvailableForState(session, state, reward))
                {
                    continue;
                }

                var linkedChoice = ResolveLinkedChoice(reward.LinkedChoiceId);
                var title = linkedChoice != null && !string.IsNullOrWhiteSpace(linkedChoice.Title)
                    ? linkedChoice.Title
                    : reward.Title;
                var description = linkedChoice != null && !string.IsNullOrWhiteSpace(linkedChoice.DescriptionText)
                    ? linkedChoice.DescriptionText
                    : reward.Description;
                offeringChoices.Add(new OfferingChoiceView
                {
                    Kind = OfferingChoiceKind.Enhancement,
                    MonsterId = state.MonsterId,
                    RewardId = reward.RewardId,
                    LinkedChoiceId = reward.LinkedChoiceId,
                    ActiveSkillId = reward.ActiveSkillId,
                    PassiveSkillId = reward.PassiveSkillId,
                    Title = $"{monster.DisplayName}\n{title}",
                    Description = ResolveDescription(null, description, linkedChoice != null ? linkedChoice.ChoiceId : reward.RewardId),
                    DamageMultiplier = ResolveDamageMultiplier(reward, linkedChoice),
                    MagazineBonus = ResolveMagazineBonus(reward, linkedChoice),
                    ShotIntervalMultiplier = ResolveShotIntervalMultiplier(reward, linkedChoice),
                    ReloadDurationMultiplier = ResolveReloadDurationMultiplier(reward, linkedChoice),
                    MaxHealthBonus = ResolveMaxHealthBonus(reward, linkedChoice),
                    StatusChanceBonus = ResolveStatusChanceBonus(reward, linkedChoice)
                });
            }
        }

        private static SkillChoiceDefinition ResolveLinkedChoice(string linkedChoiceId)
        {
            if (string.IsNullOrWhiteSpace(linkedChoiceId))
            {
                return null;
            }

            var manager = PakuriDataManager.Instance;
            if (manager == null || !manager.TryGetData(linkedChoiceId, out SkillChoiceDefinition linkedChoice))
            {
                return null;
            }

            return linkedChoice;
        }

        private static float ResolveDamageMultiplier(
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition linkedChoice)
        {
            if (linkedChoice != null && linkedChoice.HasDamageMultiplier)
            {
                return linkedChoice.DamageMultiplier;
            }

            return reward != null && reward.DamageMultiplier > 0f ? reward.DamageMultiplier : 1f;
        }

        private static int ResolveMagazineBonus(
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition linkedChoice)
        {
            if (linkedChoice != null && linkedChoice.HasMagazineBonus)
            {
                return linkedChoice.MagazineBonus;
            }

            return reward != null ? reward.MagazineBonus : 0;
        }

        private static float ResolveShotIntervalMultiplier(
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition linkedChoice)
        {
            if (linkedChoice != null && linkedChoice.HasShotIntervalMultiplier)
            {
                return linkedChoice.ShotIntervalMultiplier;
            }

            return reward != null && reward.ShotIntervalMultiplier > 0f ? reward.ShotIntervalMultiplier : 1f;
        }

        private static float ResolveReloadDurationMultiplier(
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition linkedChoice)
        {
            if (linkedChoice != null && linkedChoice.HasReloadDurationMultiplier)
            {
                return linkedChoice.ReloadDurationMultiplier;
            }

            return reward != null && reward.ReloadDurationMultiplier > 0f ? reward.ReloadDurationMultiplier : 1f;
        }

        private static float ResolveMaxHealthBonus(
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition linkedChoice)
        {
            if (linkedChoice != null && linkedChoice.HasMaxHealthBonus)
            {
                return linkedChoice.MaxHealthBonus;
            }

            return reward != null ? reward.MaxHealthBonus : 0f;
        }

        private static float ResolveStatusChanceBonus(
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoiceDefinition linkedChoice)
        {
            if (linkedChoice != null && linkedChoice.HasStatusChanceBonus)
            {
                return linkedChoice.StatusChanceBonus;
            }

            return reward != null ? reward.StatusChanceBonus : 0f;
        }

        private static bool IsRewardChoiceAvailableForState(
            RunSession session,
            RunSession.RunMonsterState state,
            MonsterDefinition.RewardChoiceDefinition reward)
        {
            if (session == null || state == null || reward == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(reward.ActiveSkillId))
            {
                return session.HasLearnedActive(state.MonsterId, reward.ActiveSkillId);
            }

            if (!string.IsNullOrWhiteSpace(reward.PassiveSkillId))
            {
                return session.HasLearnedPassive(state.MonsterId, reward.PassiveSkillId);
            }

            return true;
        }

        private System.Collections.Generic.List<MonsterDefinition> ResolveOfferingTargets(RunSession session)
        {
            var targets = new System.Collections.Generic.List<MonsterDefinition>();
            var catalog = resolveCatalog?.Invoke();
            AddOfferingTarget(targets, PakuriDataManager.Instance.ResolveMonster(session.SelectedMonsterId, catalog));
            for (var i = 0; i < session.ManifestedMonsterIds.Count; i++)
            {
                AddOfferingTarget(targets, PakuriDataManager.Instance.ResolveMonster(session.ManifestedMonsterIds[i], catalog));
            }

            return targets;
        }

        private static void AddOfferingTarget(System.Collections.Generic.List<MonsterDefinition> targets, MonsterDefinition monster)
        {
            if (targets == null || monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
            {
                return;
            }

            for (var i = 0; i < targets.Count; i++)
            {
                if (targets[i] != null && string.Equals(targets[i].MonsterId, monster.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            targets.Add(monster);
        }

        private void RefreshRuntimeSkillModels()
        {
            var combatManager = resolveCombatManager?.Invoke();
            var session = resolveSession?.Invoke();
            if (combatManager == null || session == null)
            {
                return;
            }

            var skillCatalog = new InGameSkillCatalog(resolveCatalog?.Invoke());
            var players = combatManager.Roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i] != null ? players[i].Model as MonsterUnitRuntimeModel : null;
                if (model != null)
                {
                    SyncModelStateFromSession(session, model);
                    SkillRuntimeFactory.RebuildLearnedActiveSet(model, skillCatalog);
                    combatManager.RefreshUnitActor(model);
                }
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
            var skills = PakuriDataManager.Instance.GetActiveSkills(monster.MonsterId, monster);
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
            public string LinkedChoiceId;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public string Title;
            public string Description;
            public float DamageMultiplier = 1f;
            public int MagazineBonus;
            public float ShotIntervalMultiplier = 1f;
            public float ReloadDurationMultiplier = 1f;
            public float MaxHealthBonus;
            public float StatusChanceBonus;
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
        private readonly GameObject prisonerChoicePopUp;
        private readonly Func<RunSession> resolveSession;
        private readonly Func<GameDataCatalog> resolveCatalog;
        private readonly Func<StageManager> resolveStageManager;
        private readonly Func<SceneEntryManager> resolveEntryManager;
        private readonly Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton;
        private readonly Action consumePrisonerButton;
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
            GameObject prisonerChoicePopUp,
            Func<RunSession> resolveSession,
            Func<GameDataCatalog> resolveCatalog,
            Func<StageManager> resolveStageManager,
            Func<SceneEntryManager> resolveEntryManager,
            Func<InGameUIManager.RewardButtonView> resolveActivePrisonerButton,
            Action consumePrisonerButton,
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
            this.prisonerChoicePopUp = prisonerChoicePopUp;
            this.resolveSession = resolveSession;
            this.resolveCatalog = resolveCatalog;
            this.resolveStageManager = resolveStageManager;
            this.resolveEntryManager = resolveEntryManager;
            this.resolveActivePrisonerButton = resolveActivePrisonerButton;
            this.consumePrisonerButton = consumePrisonerButton;
            this.refreshInfo = refreshInfo;

            BindButton(this.manifestedFailBackButton, () => SetActive(this.manifestedFailPopUp, false));
            BindButton(this.dontChoiceButton, SkipManifestChoice);
            BindButton(this.choiceButton, CommitManifestChoice);
        }

        public void TryManifestPrisoner()
        {
            var session = resolveSession?.Invoke();
            var activePrisonerButton = resolveActivePrisonerButton?.Invoke();
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return;
            }

            session.ClaimPrisonerReward(activePrisonerButton.PrisonerId);
            consumePrisonerButton?.Invoke();
            SetActive(prisonerChoicePopUp, false);

            pendingManifestMonster = ResolveNextManifestCandidate(session);
            var stageManager = resolveStageManager?.Invoke();
            var successChance = stageManager != null ? stageManager.PendingManifestSuccessChance : 0.7f;
            var succeeded = pendingManifestMonster != null && UnityEngine.Random.value < successChance;
            if (!succeeded)
            {
                SetActive(manifestedFailPopUp, true);
                return;
            }

            ShowManifestSuccessPopup(pendingManifestMonster);
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
            SetActive(prisonerChoicePopUp, false);
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
            var entryManager = resolveEntryManager?.Invoke();
            if (entryManager != null)
            {
                entryManager.SpawnManifestedMonster(pendingManifestMonster, slotIndex, out _);
            }

            pendingManifestMonster = null;
            SetActive(manifestedSuccessPopUp, false);
            SetActive(prisonerChoicePopUp, false);
            refreshInfo?.Invoke();
        }

        private MonsterDefinition ResolveNextManifestCandidate(RunSession session)
        {
            var monsters = PakuriDataManager.Instance.GetMonsters(resolveCatalog?.Invoke());
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
                $"??욧쉐: {monster.ElementLabel}\n" +
                $"HP: {monster.MaxHealth:0} / ?⑤벀爰? {monster.PowerStat:0}\n" +
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
