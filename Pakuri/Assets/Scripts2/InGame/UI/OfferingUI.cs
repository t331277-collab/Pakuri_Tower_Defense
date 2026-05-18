using System;
using System.Collections.Generic;
using Pakuri.Data;
using Pakuri.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    internal sealed class OfferingUI
    {
        private const int MaxOfferingChoices = 3;
        private const int MaxRunActiveSkillCount = 5;
        private const int MaxRunPassiveSkillCount = 5;

        private readonly List<OfferingChoiceView> offeringChoices = new List<OfferingChoiceView>();
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
                    Title = $"{monster.DisplayName} ?좉퇋 ?≫떚釉? {skill.DisplayName}",
                    Description = ResolveDescription(skill.Summary, skill.DescriptionText, "?≫떚釉??ㅽ궗???듬뱷?쒕떎.")
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
                    Title = $"{monster.DisplayName} ?좉퇋 ?⑥떆釉? {passive.DisplayName}",
                    Description = ResolveDescription(passive.Summary, passive.DescriptionText, "?⑥떆釉??ㅽ궗???듬뱷?쒕떎.")
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

                offeringChoices.Add(new OfferingChoiceView
                {
                    Kind = OfferingChoiceKind.Enhancement,
                    MonsterId = state.MonsterId,
                    RewardId = reward.RewardId,
                    LinkedChoiceId = reward.LinkedChoiceId,
                    ActiveSkillId = reward.ActiveSkillId,
                    PassiveSkillId = reward.PassiveSkillId,
                    Title = $"{monster.DisplayName} ?ㅽ궗 媛뺥솕: {reward.Title}",
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

        private List<MonsterDefinition> ResolveOfferingTargets(RunSession session)
        {
            var targets = new List<MonsterDefinition>();
            var catalog = resolveCatalog?.Invoke();
            AddOfferingTarget(targets, PakuriDataManager.Instance.ResolveMonster(session.SelectedMonsterId, catalog));
            for (var i = 0; i < session.ManifestedMonsterIds.Count; i++)
            {
                AddOfferingTarget(targets, PakuriDataManager.Instance.ResolveMonster(session.ManifestedMonsterIds[i], catalog));
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

        private void RefreshRuntimeSkillModels()
        {
            var combatManager = resolveCombatManager?.Invoke();
            if (combatManager == null)
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
                    SkillRuntimeFactory.RebuildLearnedActiveSet(model, skillCatalog);
                    combatManager.RefreshUnitActor(model);
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
}
