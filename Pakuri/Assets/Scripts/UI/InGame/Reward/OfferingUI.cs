using System;
using System.Collections.Generic;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    /// Offering 보상 선택과 신규·패시브·특성·마스터 표시를 관리한다.
    public sealed class OfferingUI : MonoBehaviour
    {
        private const int MaxOfferingChoices = 3;
        private enum OfferingKind
        {
            NewActiveSkill,
            NewPassiveSkill,
            Trait,
            Master
        }

        private readonly List<OfferingChoiceView> offeringChoices = new List<OfferingChoiceView>();
        private GameObject offeringPanel;
        private OfferingButtonView[] offeringButtonViews = new OfferingButtonView[MaxOfferingChoices];
        private InGameCombatManager combatManager;
        private InGameUIManager uiManager;
        private bool referencesBound;
        private bool bindingFailed;
        private string[] tutorialSkillNames;
        private bool choiceInputEnabled = true;

        public event Action Opened;
        public event Action<string> ChoiceCommitted;

        public void SetTutorialSkills(string[] skillNames, bool inputEnabled)
        {
            tutorialSkillNames = skillNames;
            choiceInputEnabled = inputEnabled;
        }

        public void SetChoiceInputEnabled(bool enabled)
        {
            choiceInputEnabled = enabled;
            for (var i = 0; i < offeringButtonViews.Length; i++)
            {
                var button = offeringButtonViews[i]?.Button;
                if (button != null && button.gameObject.activeSelf)
                {
                    button.interactable = enabled;
                }
            }
        }

        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
            }
        }

        public bool OpenOfferingPanel(string monsterName)
        {
            if (!BindObject())
            {
                return false;
            }

            var activePrisonerButton = uiManager?.ActivePrisonerButton;
            if (activePrisonerButton == null
                || activePrisonerButton.Consumed
                || string.IsNullOrWhiteSpace(monsterName))
            {
                return false;
            }

            BuildOfferingChoices(monsterName);
            if (offeringChoices.Count == 0)
            {
                Debug.LogWarning($"Offering has no available choices for monster '{monsterName}'.");
                return false;
            }

            UiObjectUtility.SetActive(offeringPanel, true);
            for (var i = 0; i < offeringButtonViews.Length; i++)
            {
                var buttonView = offeringButtonViews[i];
                var button = buttonView != null ? buttonView.Button : null;
                if (button == null)
                {
                    continue;
                }

                button.onClick.RemoveAllListeners();
                var hasChoice = i < offeringChoices.Count;
                button.gameObject.SetActive(hasChoice);
                button.interactable = hasChoice && choiceInputEnabled;
                if (!hasChoice)
                {
                    continue;
                }

                var capturedIndex = i;
                var choice = offeringChoices[i];
                BindChoiceButton(buttonView, choice);
                button.onClick.AddListener(() => CommitOfferingChoice(capturedIndex));
            }

            Opened?.Invoke();
            return true;
        }

        public void Hide()
        {
            UiObjectUtility.SetActive(offeringPanel, false);
        }

        private void CommitOfferingChoice(int choiceIndex)
        {
            var session = uiManager?.ResolveSession();
            var activePrisonerButton = uiManager?.ActivePrisonerButton;
            if (session == null
                || activePrisonerButton == null
                || activePrisonerButton.Consumed
                || choiceIndex < 0
                || choiceIndex >= offeringChoices.Count)
            {
                return;
            }

            var choice = offeringChoices[choiceIndex];
            var state = session.GetPartyMemberState(choice.MonsterName);
            if (state == null)
            {
                return;
            }

            session.RecordOfferingChoice(
                state,
                choice.RewardName,
                choice.ChoiceName,
                choice.ActiveSkillName,
                choice.PassiveSkillName);

            RefreshRuntimeSkillModels();
            uiManager?.ConsumeActivePrisonerButton();
            UiObjectUtility.SetActive(offeringPanel, false);
            uiManager?.RefreshInfo();
            uiManager?.CompletePrisonAction();
            ChoiceCommitted?.Invoke(choice.ActiveSkillName);
        }

        private void BuildOfferingChoices(string monsterName)
        {
            offeringChoices.Clear();
            var session = uiManager?.ResolveSession();
            if (session == null)
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterName);
            if (monster == null)
            {
                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterName);
            if (state == null)
            {
                return;
            }

            if (tutorialSkillNames != null && tutorialSkillNames.Length > 0)
            {
                AddTutorialSkillChoices(session, monster, state);
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

        private void AddTutorialSkillChoices(
            RunSession session,
            MonsterDefinition monster,
            RunSession.RunMonsterState state)
        {
            for (var i = 0; i < tutorialSkillNames.Length; i++)
            {
                var skill = GameDataLoader.CurrentCatalog.GetData<SkillDefinition>(tutorialSkillNames[i]);
                if (skill == null || !session.CanLearnActive(state, monster, skill))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterName = state.MonsterName,
                    ActiveSkillName = skill.SkillName,
                    Kind = OfferingKind.NewActiveSkill,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(skill.DisplayName, skill.SkillName),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(skill.DisplayName, skill.SkillName)}",
                    Description = ResolveDescription(skill.Summary, skill.Description, skill.DisplayName),
                    Icon = skill.Icon
                });
            }
        }

        private void AddActiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var skills = GameDataLoader.CurrentCatalog.GetActiveSkills(monster.MonsterName);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (!session.CanLearnActive(state, monster, skill))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterName = state.MonsterName,
                    ActiveSkillName = skill.SkillName,
                    Kind = OfferingKind.NewActiveSkill,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(skill.DisplayName, skill.SkillName),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(skill.DisplayName, skill.SkillName)}",
                    Description = ResolveDescription(skill.Summary, skill.Description, skill.DisplayName),
                    Icon = skill.Icon
                });
            }
        }

        private void AddPassiveSkillChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var passives = GameDataLoader.CurrentCatalog.GetPassiveSkills(monster.MonsterName);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (!session.CanLearnPassive(state, monster, passive))
                {
                    continue;
                }

                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterName = state.MonsterName,
                    PassiveSkillName = passive.SkillName,
                    Kind = OfferingKind.NewPassiveSkill,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(passive.DisplayName, passive.SkillName),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(passive.DisplayName, passive.SkillName)}",
                    Description = ResolveDescription(passive.Summary, passive.Description, passive.DisplayName),
                    Icon = ResolvePassiveUnlockIcon(monster, passive)
                });
            }
        }

        private static Sprite ResolvePassiveUnlockIcon(
            MonsterDefinition monster,
            PassiveSkillDefinition passive)
        {
            if (monster?.ActiveSkills != null && passive != null)
            {
                for (var i = 0; i < monster.ActiveSkills.Length; i++)
                {
                    var active = monster.ActiveSkills[i];
                    if (active != null
                        && active.Slot == passive.RequiredActiveSlot
                        && active.Icon != null)
                    {
                        return active.Icon;
                    }
                }
            }

            return passive != null ? passive.Icon : null;
        }

        private void AddEnhancementChoices(RunSession session, MonsterDefinition monster, RunSession.RunMonsterState state)
        {
            if (monster == null || state == null)
            {
                return;
            }

            var rewards = GameDataLoader.CurrentCatalog.GetRewardChoices(monster.MonsterName);
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward == null
                    || string.IsNullOrWhiteSpace(reward.RewardName)
                    || state.ChosenRewardNames.Contains(reward.RewardName))
                {
                    continue;
                }

                var choiceData = ResolveChoice(reward.RewardName);
                if (choiceData == null || !session.CanChooseSkillChoice(state, reward, choiceData))
                {
                    continue;
                }

                var skillName = BuildEnhancementSkillName(monster, reward, choiceData);
                offeringChoices.Add(new OfferingChoiceView
                {
                    MonsterName = state.MonsterName,
                    RewardName = reward.RewardName,
                    ChoiceName = reward.RewardName,
                    ActiveSkillName = reward.ActiveSkillName,
                    PassiveSkillName = reward.PassiveSkillName,
                    Kind = ResolveOfferingKind(choiceData),
                    Summary = monster.DisplayName,
                    SkillName = skillName,
                    Title = $"{monster.DisplayName} · {skillName}",
                    Description = ResolveDescription(null, choiceData.DescriptionText, choiceData.ChoiceName),
                    Icon = ResolveChoiceIcon(choiceData)
                });
            }
        }

        private static OfferingKind ResolveOfferingKind(SkillChoice choice)
        {
            return choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster
                ? OfferingKind.Master
                : OfferingKind.Trait;
        }

        private static SkillChoice ResolveChoice(string choiceName)
        {
            if (string.IsNullOrWhiteSpace(choiceName))
            {
                return null;
            }

            var manager = GameDataLoader.CurrentCatalog;
            if (manager == null || !manager.TryGetData(choiceName, out SkillChoice choice))
            {
                return null;
            }

            return choice;
        }

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
            if (manager == null || string.IsNullOrWhiteSpace(choice.SkillName))
            {
                return null;
            }

            if (manager.TryGetData(choice.SkillName, out SkillDefinition activeSkill) && activeSkill != null)
            {
                return activeSkill.Icon;
            }

            if (manager.TryGetData(choice.SkillName, out PassiveSkillDefinition passiveSkill) && passiveSkill != null)
            {
                return passiveSkill.Icon;
            }

            return null;
        }

        private static string BuildEnhancementSkillName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoice choice)
        {
            var sourceName = ResolveLinkedSkillDisplayName(monster, reward, choice);
            var choiceTitle = choice != null && !string.IsNullOrWhiteSpace(choice.Title)
                ? choice.Title.Trim()
                : choice != null ? choice.ChoiceName : string.Empty;

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return ResolveChoiceDisplayName(choiceTitle, choice != null ? choice.ChoiceName : string.Empty);
            }

            return string.IsNullOrWhiteSpace(choiceTitle) ? sourceName : $"{sourceName}·{choiceTitle}";
        }

        private static string ResolveLinkedSkillDisplayName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoice choice)
        {
            var targetSkillName = choice != null ? choice.TargetSkillName : string.Empty;
            var choiceSkillName = choice != null ? choice.SkillName : string.Empty;
            var rewardActiveSkillName = reward != null ? reward.ActiveSkillName : string.Empty;
            var rewardPassiveSkillName = reward != null ? reward.PassiveSkillName : string.Empty;
            var Name = !string.IsNullOrWhiteSpace(targetSkillName)
                ? targetSkillName
                : !string.IsNullOrWhiteSpace(choiceSkillName)
                    ? choiceSkillName
                    : !string.IsNullOrWhiteSpace(rewardActiveSkillName)
                        ? rewardActiveSkillName
                        : rewardPassiveSkillName;

            return ResolveSkillDisplayName(monster, Name);
        }

        private static string ResolveSkillDisplayName(MonsterDefinition monster, string skillName)
        {
            if (string.IsNullOrWhiteSpace(skillName))
            {
                return string.Empty;
            }

            if (monster != null && monster.ActiveSkills != null)
            {
                for (var i = 0; i < monster.ActiveSkills.Length; i++)
                {
                    var skill = monster.ActiveSkills[i];
                    if (skill != null && string.Equals(skill.SkillName, skillName, StringComparison.OrdinalIgnoreCase))
                    {
                        return ResolveChoiceDisplayName(skill.DisplayName, skill.SkillName);
                    }
                }
            }

            if (monster != null && monster.PassiveSkills != null)
            {
                for (var i = 0; i < monster.PassiveSkills.Length; i++)
                {
                    var passive = monster.PassiveSkills[i];
                    if (passive != null && string.Equals(passive.SkillName, skillName, StringComparison.OrdinalIgnoreCase))
                    {
                        return ResolveChoiceDisplayName(passive.DisplayName, passive.SkillName);
                    }
                }
            }

            var manager = GameDataLoader.CurrentCatalog;
            if (manager != null)
            {
                if (manager.TryGetData(skillName, out SkillDefinition activeSkill) && activeSkill != null)
                {
                    return ResolveChoiceDisplayName(activeSkill.DisplayName, activeSkill.SkillName);
                }

                if (manager.TryGetData(skillName, out PassiveSkillDefinition passiveSkill) && passiveSkill != null)
                {
                    return ResolveChoiceDisplayName(passiveSkill.DisplayName, passiveSkill.SkillName);
                }
            }

            return skillName;
        }

        private static string ResolveChoiceDisplayName(string displayName, string fallback)
        {
            return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName.Trim();
        }

        private void RefreshRuntimeSkillModels()
        {
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
                    model.SkillState.RebuildLearnedSkillState(model);
                    combatManager.RefreshPassiveEffects(model);
                    units.RefreshDisplay(model);
                }
            }

            RefreshSceneMonsterActorSkillModels();
        }

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

                model.SkillState.RebuildLearnedSkillState(model);
                actor.RefreshDisplay();
            }
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
            return string.IsNullOrWhiteSpace(description)
                ? (string.IsNullOrWhiteSpace(summary) ? fallback : summary)
                : description;
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
                view.SkillNameLabel.color = Color.black;
            }

            if (view.PopUp != null)
            {
                view.PopUp.SetActive(
                    choice.Kind == OfferingKind.NewActiveSkill
                    || choice.Kind == OfferingKind.NewPassiveSkill
                    || choice.Kind == OfferingKind.Master);
            }

            if (view.PopUpText != null)
            {
                view.PopUpText.text = ResolvePopUpText(choice.Kind);
            }

            if (view.TitleLabel != null && view.SkillNameLabel == null)
            {
                view.TitleLabel.text = choice.Title;
            }

            if (view.DescriptionLabel != null)
            {
                view.DescriptionLabel.text = choice.Description;
            }

            if (view.TitleLabel != null && view.DescriptionLabel == null && view.SkillNameLabel == null)
            {
                view.TitleLabel.text = $"{choice.Title}\n{choice.Description}";
            }

            if (view.IconImage != null)
            {
                view.IconImage.sprite = choice.Icon;
                view.IconImage.enabled = choice.Icon != null;
                view.IconImage.gameObject.SetActive(choice.Icon != null);
            }
        }

        private static string ResolvePopUpText(OfferingKind kind)
        {
            switch (kind)
            {
                case OfferingKind.NewActiveSkill:
                    return "신규 획득!";
                case OfferingKind.NewPassiveSkill:
                    return "패시브 스킬";
                case OfferingKind.Master:
                    return "마스터 스킬";
                default:
                    return string.Empty;
            }
        }

        private sealed class OfferingChoiceView
        {
            public string MonsterName;
            public string RewardName;
            public string ChoiceName;
            public string ActiveSkillName;
            public string PassiveSkillName;
            public OfferingKind Kind;
            public string Summary;
            public string SkillName;
            public string Title;
            public string Description;
            public Sprite Icon;
        }

        [Serializable]
        private sealed class OfferingButtonView
        {
            private Button button;
            private TMP_Text summaryLabel;
            private TMP_Text skillNameLabel;
            private TMP_Text titleLabel;
            private TMP_Text descriptionLabel;
            private Image iconImage;
            private GameObject popUp;
            private TMP_Text popUpText;

            internal void BindObject(
                Component owner,
                Transform root,
                string choicePath,
                int choiceIndex,
                ref bool valid)
            {
                var choiceRoot = root != null ? root.Find(choicePath) : null;
                if (choiceRoot == null)
                {
                    Debug.LogError(
                        $"{owner.GetType().Name} BindObject failed: field 'offeringButtonViews[{choiceIndex}]' at path '{choicePath}' requires a choice object.",
                        owner);
                    valid = false;
                    return;
                }

                button = UiBindingUtility.BindSelf<Button>(
                    owner,
                    choiceRoot,
                    $"offeringButtonViews[{choiceIndex}].button",
                    ref valid);
                summaryLabel = UiBindingUtility.BindChild<TMP_Text>(
                    owner,
                    choiceRoot,
                    "Summary",
                    $"offeringButtonViews[{choiceIndex}].summaryLabel",
                    ref valid);
                skillNameLabel = UiBindingUtility.BindChild<TMP_Text>(
                    owner,
                    choiceRoot,
                    "SkillName",
                    $"offeringButtonViews[{choiceIndex}].skillNameLabel",
                    ref valid);
                titleLabel = UiBindingUtility.BindOptionalChild<TMP_Text>(choiceRoot, "Title");
                descriptionLabel = UiBindingUtility.BindChild<TMP_Text>(
                    owner,
                    choiceRoot,
                    "Desc",
                    $"offeringButtonViews[{choiceIndex}].descriptionLabel",
                    ref valid);
                iconImage = UiBindingUtility.BindChild<Image>(
                    owner,
                    choiceRoot,
                    "Icon",
                    $"offeringButtonViews[{choiceIndex}].iconImage",
                    ref valid);
                popUp = UiBindingUtility.BindChildObject(
                    owner,
                    choiceRoot,
                    "PopUP",
                    $"offeringButtonViews[{choiceIndex}].popUp",
                    ref valid);
                popUpText = UiBindingUtility.BindChild<TMP_Text>(
                    owner,
                    choiceRoot,
                    "PopUP/NewSkillPopUText",
                    $"offeringButtonViews[{choiceIndex}].popUpText",
                    ref valid);
            }

            public Button Button => button;
            public TMP_Text SummaryLabel => summaryLabel;
            public TMP_Text SkillNameLabel => skillNameLabel;
            public TMP_Text TitleLabel => titleLabel;
            public TMP_Text DescriptionLabel => descriptionLabel;
            public Image IconImage => iconImage;
            public GameObject PopUp => popUp;
            public TMP_Text PopUpText => popUpText;
        }

        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            var valid = true;
            offeringPanel = gameObject;
            combatManager = UiBindingUtility.BindSceneComponent<InGameCombatManager>(
                this,
                nameof(combatManager),
                ref valid);
            uiManager = UiBindingUtility.BindSceneComponent<InGameUIManager>(
                this,
                nameof(uiManager),
                ref valid);

            if (offeringButtonViews == null || offeringButtonViews.Length != MaxOfferingChoices)
            {
                offeringButtonViews = new OfferingButtonView[MaxOfferingChoices];
            }

            for (var i = 0; i < offeringButtonViews.Length; i++)
            {
                offeringButtonViews[i] = new OfferingButtonView();
                offeringButtonViews[i].BindObject(this, transform, $"Choice{i + 1}", i, ref valid);
            }

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }
    }
}
