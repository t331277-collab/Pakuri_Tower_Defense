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
        private static readonly Color TraitSkillNameColor = new Color(0.5f, 0f, 0.5f);

        private enum OfferingKind
        {
            Normal,
            NewActiveSkill,
            NewPassiveSkill,
            Trait,
            Master
        }

        private readonly List<OfferingChoiceView> offeringChoices = new List<OfferingChoiceView>();
        [SerializeField] private GameObject offeringPanel;
        [SerializeField] private OfferingButtonView[] offeringButtonViews = new OfferingButtonView[MaxOfferingChoices];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private InGameUIManager uiManager;

        public bool OpenOfferingPanel(string monsterId)
        {
            var activePrisonerButton = uiManager?.ActivePrisonerButton;
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
            for (var i = 0; i < offeringButtonViews.Length; i++)
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
            uiManager?.ConsumeActivePrisonerButton();
            SetActive(offeringPanel, false);
            uiManager?.RefreshInfo();
            uiManager?.CompletePrisonAction();
        }

        private void BuildOfferingChoices(string monsterId)
        {
            offeringChoices.Clear();
            var session = uiManager?.ResolveSession();
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
                    Kind = OfferingKind.NewActiveSkill,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(skill.SkillName, skill.SkillId),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(skill.SkillName, skill.SkillId)}",
                    Description = ResolveDescription(skill.Summary, skill.Description, skill.SkillId),
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
                    Kind = OfferingKind.NewPassiveSkill,
                    Summary = monster.DisplayName,
                    SkillName = ResolveChoiceDisplayName(passive.SkillName, passive.SkillId),
                    Title = $"{monster.DisplayName} · {ResolveChoiceDisplayName(passive.SkillName, passive.SkillId)}",
                    Description = ResolveDescription(passive.Summary, passive.Description, passive.SkillId),
                    Icon = passive.Icon
                });
            }
        }

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
                if (choiceData == null || !session.CanChooseSkillChoice(state, reward, choiceData))
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
                    Kind = ResolveOfferingKind(choiceData),
                    Summary = monster.DisplayName,
                    SkillName = skillName,
                    Title = $"{monster.DisplayName} · {skillName}",
                    Description = ResolveDescription(null, choiceData.DescriptionText, choiceData.ChoiceId),
                    Icon = ResolveChoiceIcon(choiceData)
                });
            }
        }

        private static OfferingKind ResolveOfferingKind(SkillChoice choice)
        {
            if (choice == null)
            {
                return OfferingKind.Normal;
            }

            return choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster
                ? OfferingKind.Master
                : OfferingKind.Trait;
        }

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

        private static string BuildEnhancementSkillName(
            MonsterDefinition monster,
            MonsterDefinition.RewardChoiceDefinition reward,
            SkillChoice choice)
        {
            var sourceName = ResolveLinkedSkillDisplayName(monster, reward, choice);
            var choiceTitle = choice != null && !string.IsNullOrWhiteSpace(choice.Title)
                ? choice.Title.Trim()
                : choice != null ? choice.ChoiceId : string.Empty;

            if (string.IsNullOrWhiteSpace(sourceName))
            {
                return ResolveChoiceDisplayName(choiceTitle, choice != null ? choice.ChoiceId : string.Empty);
            }

            return string.IsNullOrWhiteSpace(choiceTitle) ? sourceName : $"{sourceName}·{choiceTitle}";
        }

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
                view.SkillNameLabel.color = ResolveSkillNameColor(choice.Kind);
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

            if (view.FallbackLabel != null && view.DescriptionLabel == null && view.SkillNameLabel == null)
            {
                view.FallbackLabel.text = $"{choice.Title}\n{choice.Description}";
            }

            if (view.IconImage != null)
            {
                view.IconImage.sprite = choice.Icon;
                view.IconImage.enabled = choice.Icon != null;
                view.IconImage.gameObject.SetActive(choice.Icon != null);
            }
        }

        private static Color ResolveSkillNameColor(OfferingKind kind)
        {
            switch (kind)
            {
                case OfferingKind.NewActiveSkill:
                case OfferingKind.NewPassiveSkill:
                    return Color.yellow;
                case OfferingKind.Trait:
                    return TraitSkillNameColor;
                case OfferingKind.Master:
                    return Color.blue;
                default:
                    return Color.white;
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

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private sealed class OfferingChoiceView
        {
            public string MonsterId;
            public string RewardId;
            public string ChoiceId;
            public string ActiveSkillId;
            public string PassiveSkillId;
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
            [SerializeField] private Button button;
            [SerializeField] private TMP_Text summaryLabel;
            [SerializeField] private TMP_Text skillNameLabel;
            [SerializeField] private TMP_Text titleLabel;
            [SerializeField] private TMP_Text descriptionLabel;
            [SerializeField] private Image iconImage;
            [SerializeField] private GameObject popUp;
            [SerializeField] private TMP_Text popUpText;

            public Button Button => button;
            public TMP_Text SummaryLabel => summaryLabel;
            public TMP_Text SkillNameLabel => skillNameLabel;
            public TMP_Text TitleLabel => titleLabel;
            public TMP_Text DescriptionLabel => descriptionLabel;
            public TMP_Text FallbackLabel => titleLabel;
            public Image IconImage => iconImage;
            public GameObject PopUp => popUp;
            public TMP_Text PopUpText => popUpText;
        }
    }
}
