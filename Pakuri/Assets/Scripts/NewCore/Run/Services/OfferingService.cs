using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Run.Services
{
    public enum OfferingCandidateKind
    {
        ActiveSkill,
        PassiveSkill,
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement
    }

    public sealed class OfferingCandidate
    {
        internal OfferingCandidate(
            OfferingCandidateKind kind,
            SkillDefinition skill,
            SkillChoiceDefinition choice)
        {
            Kind = kind;
            Skill = skill;
            Choice = choice;
            Id = skill != null ? skill.skill_id : choice.choice_id;
        }

        public string Id { get; }

        public OfferingCandidateKind Kind { get; }

        public SkillDefinition Skill { get; }

        public SkillChoiceDefinition Choice { get; }
    }

    public sealed class OfferingOffer
    {
        internal OfferingOffer(
            MonsterModel monster,
            Prisoner prisoner,
            IReadOnlyList<OfferingCandidate> candidates)
        {
            Monster = monster;
            Prisoner = prisoner;
            Candidates = candidates;
        }

        public MonsterModel Monster { get; }

        public Prisoner Prisoner { get; }

        public IReadOnlyList<OfferingCandidate> Candidates { get; }
    }

    public sealed class OfferingService
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly StageManager stage;
        private readonly Func<int, int> randomIndex;

        public OfferingService(
            GameDefinitionCatalog catalog,
            StageManager stage,
            Func<int, int> randomIndex)
        {
            this.catalog =
                catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.stage =
                stage ?? throw new ArgumentNullException(nameof(stage));
            this.randomIndex =
                randomIndex ?? throw new ArgumentNullException(nameof(randomIndex));
        }

        public OfferingOffer PendingOffer { get; private set; }

        public OfferingOffer GenerateCandidates(
            MonsterModel monster,
            Prisoner prisoner)
        {
            if (PendingOffer != null)
            {
                throw new InvalidOperationException(
                    "The current offering must be completed.");
            }

            RequireOwnedInputs(monster, prisoner);
            List<OfferingCandidate> eligible =
                BuildEligible(monster);
            Shuffle(eligible);
            int visibleCount = Math.Min(3, eligible.Count);
            List<OfferingCandidate> visible =
                eligible.GetRange(0, visibleCount);
            OfferingOffer offer = new OfferingOffer(
                monster,
                prisoner,
                new ReadOnlyCollection<OfferingCandidate>(visible));
            if (visible.Count > 0)
            {
                PendingOffer = offer;
            }

            return offer;
        }

        public bool TryConfirm(string candidateId)
        {
            if (string.IsNullOrEmpty(candidateId)
                || PendingOffer == null)
            {
                return false;
            }

            OfferingCandidate candidate = null;
            for (int index = 0;
                index < PendingOffer.Candidates.Count;
                index++)
            {
                if (string.Equals(
                    PendingOffer.Candidates[index].Id,
                    candidateId,
                    StringComparison.Ordinal))
                {
                    candidate = PendingOffer.Candidates[index];
                    break;
                }
            }

            if (candidate == null
                || !stage.Session.PrisonerInventory.CanConsume(
                    PendingOffer.Prisoner)
                || !ApplyCandidate(PendingOffer.Monster, candidate))
            {
                return false;
            }

            if (!stage.Session.PrisonerInventory.TryConsume(
                    PendingOffer.Prisoner))
            {
                throw new InvalidOperationException(
                    "Validated prisoner consumption failed.");
            }

            PendingOffer = null;
            return true;
        }

        private List<OfferingCandidate> BuildEligible(
            MonsterModel monster)
        {
            List<OfferingCandidate> result =
                new List<OfferingCandidate>();
            List<SkillDefinition> skills =
                new List<SkillDefinition>(catalog.Skills.Values);
            skills.Sort((left, right) =>
                string.CompareOrdinal(left.skill_id, right.skill_id));
            for (int index = 0; index < skills.Count; index++)
            {
                SkillDefinition skill = skills[index];
                if (!string.Equals(
                        skill.monster_id,
                        monster.MonsterDefinition.id,
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (skill is PassiveDefinition passive)
                {
                    if (monster.SkillBucket.CanLearnPassive(passive))
                    {
                        result.Add(new OfferingCandidate(
                            OfferingCandidateKind.PassiveSkill,
                            passive,
                            null));
                    }
                }
                else if (monster.SkillBucket.CanLearnActive(skill))
                {
                    result.Add(new OfferingCandidate(
                        OfferingCandidateKind.ActiveSkill,
                        skill,
                        null));
                }
            }

            List<SkillChoiceDefinition> choices =
                new List<SkillChoiceDefinition>(catalog.Choices.Values);
            choices.Sort((left, right) =>
                string.CompareOrdinal(left.choice_id, right.choice_id));
            for (int index = 0; index < choices.Count; index++)
            {
                SkillChoiceDefinition choice = choices[index];
                if (!monster.SkillBucket.CanSelectChoice(choice)
                    || !TryResolveChoiceKind(
                        choice.choice_group,
                        out OfferingCandidateKind kind))
                {
                    continue;
                }

                result.Add(new OfferingCandidate(
                    kind,
                    null,
                    choice));
            }

            return result;
        }

        private bool ApplyCandidate(
            MonsterModel monster,
            OfferingCandidate candidate)
        {
            switch (candidate.Kind)
            {
                case OfferingCandidateKind.ActiveSkill:
                    return monster.SkillBucket.TryLearnActive(
                        candidate.Skill);

                case OfferingCandidateKind.PassiveSkill:
                    PassiveDefinition passive =
                        (PassiveDefinition)candidate.Skill;
                    if (!monster.SkillBucket.TryLearnPassive(passive))
                    {
                        return false;
                    }

                    SkillChoiceDefinition passiveBase =
                        FindPassiveBase(
                            monster.MonsterDefinition.id,
                            passive.skill_id);
                    if (passiveBase != null
                        && !monster.SkillBucket.TrySelectChoice(
                            passiveBase))
                    {
                        throw new InvalidOperationException(
                            "Configured PassiveBase selection failed.");
                    }

                    return true;

                default:
                    return monster.SkillBucket.TrySelectChoice(
                        candidate.Choice);
            }
        }

        private SkillChoiceDefinition FindPassiveBase(
            string monsterId,
            string skillId)
        {
            SkillChoiceDefinition found = null;
            foreach (SkillChoiceDefinition choice
                in catalog.Choices.Values)
            {
                if (!string.Equals(
                        choice.monster_id,
                        monsterId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        choice.skill_id,
                        skillId,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        choice.choice_group,
                        "PassiveBase",
                        StringComparison.Ordinal))
                {
                    continue;
                }

                if (found != null)
                {
                    throw new InvalidOperationException(
                        "Multiple PassiveBase choices target one passive.");
                }

                found = choice;
            }

            return found;
        }

        private void RequireOwnedInputs(
            MonsterModel monster,
            Prisoner prisoner)
        {
            if (monster == null)
            {
                throw new ArgumentNullException(nameof(monster));
            }

            if (prisoner == null)
            {
                throw new ArgumentNullException(nameof(prisoner));
            }

            if (!ReferenceEquals(
                    stage.Session.PartyRoster.GetByMonsterId(
                        monster.MonsterDefinition.id),
                    monster))
            {
                throw new InvalidOperationException(
                    "Offering target is not in the active party.");
            }

            if (!stage.Session.PrisonerInventory.CanConsume(prisoner))
            {
                throw new InvalidOperationException(
                    "Offering prisoner is not held.");
            }
        }

        private void Shuffle(List<OfferingCandidate> candidates)
        {
            for (int index = candidates.Count - 1;
                index > 0;
                index--)
            {
                int selected = ResolveRandomIndex(index + 1);
                OfferingCandidate value = candidates[index];
                candidates[index] = candidates[selected];
                candidates[selected] = value;
            }
        }

        private int ResolveRandomIndex(int count)
        {
            int index = randomIndex(count);
            if (index < 0 || index >= count)
            {
                throw new InvalidOperationException(
                    "The random index source returned an invalid index.");
            }

            return index;
        }

        private static bool TryResolveChoiceKind(
            string choiceGroup,
            out OfferingCandidateKind kind)
        {
            switch (choiceGroup)
            {
                case "ActiveEnhancement":
                    kind = OfferingCandidateKind.ActiveEnhancement;
                    return true;
                case "ActiveMaster":
                    kind = OfferingCandidateKind.ActiveMaster;
                    return true;
                case "PassiveEnhancement":
                    kind = OfferingCandidateKind.PassiveEnhancement;
                    return true;
                default:
                    kind = default;
                    return false;
            }
        }
    }
}
