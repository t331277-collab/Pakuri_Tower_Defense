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
        private const int MaxOfferingChoices = 3;
        private const int MaxRunActiveSkillCount = 5;
        private const int MaxRunPassiveSkillCount = 5;

        private readonly List<RewardButtonView> rewardButtons = new List<RewardButtonView>();
        private readonly List<OfferingChoiceView> offeringChoices = new List<OfferingChoiceView>();

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
        private Button manifestedButton;

        private GameObject offeringPanel;
        private readonly Button[] offeringChoiceButtons = new Button[MaxOfferingChoices];

        private GameObject manifestedFailPopUp;
        private Button manifestedFailBackButton;

        private GameObject manifestedSuccessPopUp;
        private Button dontChoiceButton;
        private Button choiceButton;
        private TMP_Text monsterNameText;
        private TMP_Text monsterDescText;
        private Image monsterImage;

        private TMP_Text stageInfoText;
        private TMP_Text goldInfoText;
        private TMP_Text darkInfoText;

        private int shownStage = -1;
        private int shownDay = -1;
        private RewardButtonView activePrisonerButton;
        private MonsterDefinition pendingManifestMonster;

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

            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);
            }

            if (rewardSummaryText != null)
            {
                rewardSummaryText.text = $"Stage {stageManager.CurrentStage}-{stageManager.CurrentDay} 보상";
            }

            var order = 0;
            var prisoners = stageManager.PendingPrisonerEnemyIds;
            for (var i = 0; i < prisoners.Count; i++)
            {
                var capturedIndex = i;
                var button = CreateRewardButton(prisonerTemplateButton, "PrisonerReward", order++);
                SetButtonLabel(button, $"포로\n{prisoners[i]}");
                var view = RegisterRewardButton(button, RewardKind.Prisoner, 0, prisoners[i]);
                button.onClick.AddListener(() => OpenPrisonerChoice(view, capturedIndex));
            }

            if (stageManager.PendingGoldReward > 0)
            {
                var amount = stageManager.PendingGoldReward;
                var button = CreateRewardButton(goldTemplateButton, "GoldReward", order++);
                SetButtonLabel(button, $"골드\n+{amount}");
                var view = RegisterRewardButton(button, RewardKind.Gold, amount, string.Empty);
                button.onClick.AddListener(() => ClaimMaterialReward(view, amount, 0));
            }

            if (stageManager.PendingDarkTraceReward > 0)
            {
                var amount = stageManager.PendingDarkTraceReward;
                var button = CreateRewardButton(darkTemplateButton, "DarkTraceReward", order++);
                SetButtonLabel(button, $"어둠의 흔적\n+{amount}");
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
            if (prisonerChoicePopUp != null)
            {
                prisonerChoicePopUp.SetActive(true);
            }
        }

        private void OpenOfferingPanel()
        {
            if (activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return;
            }

            BuildOfferingChoices();
            if (offeringPanel != null)
            {
                offeringPanel.SetActive(true);
            }

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

        private void CommitOfferingChoice(int choiceIndex)
        {
            var session = ResolveSession();
            if (session == null || choiceIndex < 0 || choiceIndex >= offeringChoices.Count)
            {
                return;
            }

            var choice = offeringChoices[choiceIndex];
            session.ClaimPrisonerReward(activePrisonerButton.PrisonerId);
            session.RecordOfferingChoice(choice.MonsterId, choice.ChoiceId, choice.ActiveSkillId, choice.PassiveSkillId);
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
            ConsumePrisonerButton();
            if (offeringPanel != null)
            {
                offeringPanel.SetActive(false);
            }

            if (prisonerChoicePopUp != null)
            {
                prisonerChoicePopUp.SetActive(false);
            }

            if (rewardPanel != null)
            {
                rewardPanel.SetActive(true);
            }

            RefreshInfo();
        }

        private void TryManifestPrisoner()
        {
            var session = ResolveSession();
            if (session == null || activePrisonerButton == null || activePrisonerButton.Consumed)
            {
                return;
            }

            session.ClaimPrisonerReward(activePrisonerButton.PrisonerId);
            ConsumePrisonerButton();
            if (prisonerChoicePopUp != null)
            {
                prisonerChoicePopUp.SetActive(false);
            }

            pendingManifestMonster = ResolveNextManifestCandidate(session);
            var succeeded = pendingManifestMonster != null && UnityEngine.Random.value < stageManager.PendingManifestSuccessChance;
            if (!succeeded)
            {
                if (manifestedFailPopUp != null)
                {
                    manifestedFailPopUp.SetActive(true);
                }

                return;
            }

            ShowManifestSuccessPopup(pendingManifestMonster);
        }

        private void ShowManifestSuccessPopup(MonsterDefinition monster)
        {
            if (manifestedSuccessPopUp != null)
            {
                manifestedSuccessPopUp.SetActive(true);
            }

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
            if (manifestedSuccessPopUp != null)
            {
                manifestedSuccessPopUp.SetActive(false);
            }

            if (prisonerChoicePopUp != null)
            {
                prisonerChoicePopUp.SetActive(false);
            }
        }

        private void CommitManifestChoice()
        {
            var session = ResolveSession();
            if (session == null || pendingManifestMonster == null)
            {
                return;
            }

            session.RecordManifestedMonster(pendingManifestMonster);
            var slotIndex = Mathf.Clamp(session.ManifestedMonsterIds.Count, 1, 4);
            if (entryManager != null)
            {
                entryManager.SpawnManifestedMonster(pendingManifestMonster, slotIndex, out _);
            }

            pendingManifestMonster = null;
            if (manifestedSuccessPopUp != null)
            {
                manifestedSuccessPopUp.SetActive(false);
            }

            if (prisonerChoicePopUp != null)
            {
                prisonerChoicePopUp.SetActive(false);
            }

            RefreshInfo();
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
            if (rewardPanel != null)
            {
                rewardPanel.SetActive(false);
            }

            ClearClonedRewardButtons();
            shownStage = -1;
            shownDay = -1;

            if (stageManager != null)
            {
                stageManager.ContinueToNextDay();
            }
        }

        private void BuildOfferingChoices()
        {
            offeringChoices.Clear();
            var session = ResolveSession();
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
                    ChoiceId = skill.SkillId,
                    ActiveSkillId = skill.SkillId,
                    Title = $"{monster.DisplayName} 신규 액티브: {skill.DisplayName}",
                    Description = ResolveDescription(skill.Summary, skill.DescriptionText, "액티브 스킬을 습득한다.")
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
                    ChoiceId = passive.PassiveId,
                    PassiveSkillId = passive.PassiveId,
                    Title = $"{monster.DisplayName} 신규 패시브: {passive.DisplayName}",
                    Description = ResolveDescription(passive.Summary, passive.DescriptionText, "패시브 스킬을 습득한다.")
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

                offeringChoices.Add(new OfferingChoiceView
                {
                    Kind = OfferingChoiceKind.Enhancement,
                    MonsterId = state.MonsterId,
                    ChoiceId = reward.RewardId,
                    Title = $"{monster.DisplayName} 스킬 강화: {reward.Title}",
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

        private List<MonsterDefinition> ResolveOfferingTargets(RunSession session)
        {
            var targets = new List<MonsterDefinition>();
            AddOfferingTarget(targets, PakuriDataManager.Instance.ResolveMonster(session.SelectedMonsterId, ResolveCatalog()));
            for (var i = 0; i < session.ManifestedMonsterIds.Count; i++)
            {
                AddOfferingTarget(targets, PakuriDataManager.Instance.ResolveMonster(session.ManifestedMonsterIds[i], ResolveCatalog()));
            }

            return targets;
        }

        private static void AddOfferingTarget(List<MonsterDefinition> targets, MonsterDefinition monster)
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

        private MonsterDefinition ResolveNextManifestCandidate(RunSession session)
        {
            var monsters = PakuriDataManager.Instance.GetMonsters(ResolveCatalog());
            var candidates = new List<MonsterDefinition>();
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

        private void RefreshRuntimeSkillModels()
        {
            if (combatManager == null)
            {
                return;
            }

            var skillCatalog = new InGameSkillCatalog(ResolveCatalog());
            var players = combatManager.Roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i] != null ? players[i].Model as MonsterUnitRuntimeModel : null;
                if (model != null)
                {
                    SkillRuntimeFactory.RebuildLearnedActiveSet(model, skillCatalog);
                    combatManager.RefreshUnitActor(model);
                }
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
            manifestedButton = FindButton("PrisonerChoicePopUp/Menifested");

            offeringPanel = FindChildObject("OfferingPanel");
            offeringChoiceButtons[0] = FindButton("OfferingPanel/Choice1");
            offeringChoiceButtons[1] = FindButton("OfferingPanel/Choice2");
            offeringChoiceButtons[2] = FindButton("OfferingPanel/Choice3");

            manifestedFailPopUp = FindChildObject("MenifestedFailPopUp");
            manifestedFailBackButton = FindButton("MenifestedFailPopUp/Back");

            manifestedSuccessPopUp = FindChildObject("MenifestedSuccessPopUp");
            dontChoiceButton = FindButton("MenifestedSuccessPopUp/DontChoiceBtn");
            choiceButton = FindButton("MenifestedSuccessPopUp/ChoiceBtn");
            monsterNameText = FindText("MenifestedSuccessPopUp/MonsterName");
            monsterDescText = FindText("MenifestedSuccessPopUp/MonsterDesc");
            monsterImage = FindImage("MenifestedSuccessPopUp/MonsterImage");

            stageInfoText = FindText("Info/StageInfo");
            goldInfoText = FindText("Info/Goldinfo");
            darkInfoText = FindText("Info/Darkinfo");
        }

        private void BindStaticButtons()
        {
            BindButton(nextButton, ContinueToNextDay);
            BindButton(offeringButton, OpenOfferingPanel);
            BindButton(manifestedButton, TryManifestPrisoner);
            BindButton(manifestedFailBackButton, () =>
            {
                if (manifestedFailPopUp != null)
                {
                    manifestedFailPopUp.SetActive(false);
                }
            });
            BindButton(dontChoiceButton, SkipManifestChoice);
            BindButton(choiceButton, CommitManifestChoice);
        }

        private void HideTransientPanels()
        {
            SetActive(rewardPanel, false);
            SetActive(prisonerChoicePopUp, false);
            SetActive(offeringPanel, false);
            SetActive(manifestedFailPopUp, false);
            SetActive(manifestedSuccessPopUp, false);
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

        private static string BuildManifestDescription(MonsterDefinition monster)
        {
            if (monster == null)
            {
                return string.Empty;
            }

            return
                $"{monster.RoleSummary}\n" +
                $"속성: {monster.ElementLabel}\n" +
                $"HP: {monster.MaxHealth:0} / 공격: {monster.PowerStat:0}\n" +
                $"A: {monster.ActiveSkillName} / F: {monster.PassiveSkillName}";
        }

        private static string ResolveDescription(string summary, string description, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(description))
            {
                return description;
            }

            return string.IsNullOrWhiteSpace(summary) ? fallback : summary;
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

        private sealed class RewardButtonView
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

        private enum RewardKind
        {
            Prisoner,
            Gold,
            DarkTrace
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
            public string ChoiceId;
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
}
