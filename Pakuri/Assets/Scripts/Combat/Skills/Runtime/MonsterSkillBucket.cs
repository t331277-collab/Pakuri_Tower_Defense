using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public sealed class MonsterSkillBucket : SkillBucket
    {
        public const int MaximumActiveSkills = 3;
        public const int MaximumPassiveSkills = 5;
        public const int MaximumActiveEnhancementsPerSkill = 3;
        public const int MaximumActiveMastersPerSkill = 1;
        public const int MaximumPassiveEnhancementsPerSkill = 1;

        private readonly List<SkillChoiceDefinition> selectedChoices =
            new List<SkillChoiceDefinition>();
        private readonly Dictionary<string, SkillChoiceDefinition> passiveBaseChoices =
            new Dictionary<string, SkillChoiceDefinition>(StringComparer.Ordinal);
        private readonly IReadOnlyList<SkillChoiceDefinition> readOnlySelectedChoices;

        public MonsterSkillBucket(
            MonsterDefinition ownerDefinition,
            SkillDefinition defaultActiveSkill,
            IEnumerable<SkillChoiceDefinition> availablePassiveBaseChoices)
        {
            OwnerDefinition =
                ownerDefinition ?? throw new ArgumentNullException(nameof(ownerDefinition));
            ValidateOwnedSkill(defaultActiveSkill);
            if (!string.Equals(defaultActiveSkill.slot, "A", StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "The initial monster skill must use slot A.",
                    nameof(defaultActiveSkill));
            }

            RegisterActive(defaultActiveSkill);
            RegisterPassiveBaseChoices(availablePassiveBaseChoices);
            readOnlySelectedChoices =
                new ReadOnlyCollection<SkillChoiceDefinition>(selectedChoices);
        }

        public MonsterDefinition OwnerDefinition { get; }

        public IReadOnlyList<SkillChoiceDefinition> SelectedChoices =>
            readOnlySelectedChoices;

        public bool CanLearnActive(SkillDefinition definition)
        {
            return IsOwnedSkill(definition)
                && !(definition is PassiveDefinition)
                && ActiveSkills.Count < MaximumActiveSkills
                && !ContainsSkill(ActiveSkills, definition.skill_id);
        }

        public bool TryLearnActive(SkillDefinition definition)
        {
            if (!CanLearnActive(definition))
            {
                return false;
            }

            RegisterActive(definition);
            return true;
        }

        public bool CanLearnPassive(PassiveDefinition definition)
        {
            return IsOwnedSkill(definition)
                && PassiveSkills.Count < MaximumPassiveSkills
                && !ContainsSkill(PassiveSkills, definition.skill_id)
                && HasLearnedPassivePrerequisite(definition.skill_id);
        }

        public bool TryLearnPassive(PassiveDefinition definition)
        {
            if (!CanLearnPassive(definition))
            {
                return false;
            }

            RegisterPassive(definition);
            return true;
        }

        public bool CanSelectChoice(SkillChoiceDefinition choice)
        {
            if (choice == null
                || !string.Equals(
                    choice.monster_id,
                    OwnerDefinition.id,
                    StringComparison.Ordinal)
                || ContainsChoice(choice.choice_id))
            {
                return false;
            }

            switch (choice.choice_group)
            {
                case "ActiveEnhancement":
                    return ContainsSkill(ActiveSkills, choice.skill_id)
                        && CountChoices(choice.skill_id, "ActiveEnhancement")
                            < MaximumActiveEnhancementsPerSkill;

                case "ActiveMaster":
                    return ContainsSkill(ActiveSkills, choice.skill_id)
                        && CountChoices(choice.skill_id, "ActiveEnhancement")
                            == MaximumActiveEnhancementsPerSkill
                        && CountChoices(choice.skill_id, "ActiveMaster")
                            < MaximumActiveMastersPerSkill;

                case "PassiveBase":
                    return ContainsSkill(PassiveSkills, choice.skill_id)
                        && IsConfiguredPassiveBase(choice)
                        && CountChoices(choice.skill_id, "PassiveBase") == 0;

                case "PassiveEnhancement":
                    return ContainsSkill(PassiveSkills, choice.skill_id)
                        && CountChoices(choice.skill_id, "PassiveEnhancement")
                            < MaximumPassiveEnhancementsPerSkill
                        && (string.IsNullOrEmpty(choice.target_skill_id)
                            || ContainsSkill(ActiveSkills, choice.target_skill_id));

                default:
                    return false;
            }
        }

        public bool TrySelectChoice(SkillChoiceDefinition choice)
        {
            if (!CanSelectChoice(choice))
            {
                return false;
            }

            selectedChoices.Add(choice);
            return true;
        }

        private bool ContainsChoice(string choiceId)
        {
            for (int index = 0; index < selectedChoices.Count; index++)
            {
                if (string.Equals(
                    selectedChoices[index].choice_id,
                    choiceId,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private void RegisterPassiveBaseChoices(
            IEnumerable<SkillChoiceDefinition> availablePassiveBaseChoices)
        {
            if (availablePassiveBaseChoices == null)
            {
                throw new ArgumentNullException(nameof(availablePassiveBaseChoices));
            }

            foreach (SkillChoiceDefinition choice in availablePassiveBaseChoices)
            {
                if (choice == null
                    || !string.Equals(
                        choice.monster_id,
                        OwnerDefinition.id,
                        StringComparison.Ordinal)
                    || !string.Equals(
                        choice.choice_group,
                        "PassiveBase",
                        StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "Passive Base Choices must belong to this monster.",
                        nameof(availablePassiveBaseChoices));
                }

                if (!passiveBaseChoices.TryAdd(choice.skill_id, choice))
                {
                    throw new ArgumentException(
                        $"Duplicate Passive Base Choice for skill '{choice.skill_id}'.",
                        nameof(availablePassiveBaseChoices));
                }
            }
        }

        private bool HasLearnedPassivePrerequisite(string passiveSkillId)
        {
            return !passiveBaseChoices.TryGetValue(
                    passiveSkillId,
                    out SkillChoiceDefinition passiveBase)
                || string.IsNullOrEmpty(passiveBase.target_skill_id)
                || ContainsSkill(ActiveSkills, passiveBase.target_skill_id);
        }

        private bool IsConfiguredPassiveBase(SkillChoiceDefinition choice)
        {
            return passiveBaseChoices.TryGetValue(
                    choice.skill_id,
                    out SkillChoiceDefinition configured)
                && ReferenceEquals(configured, choice)
                && (string.IsNullOrEmpty(choice.target_skill_id)
                    || ContainsSkill(ActiveSkills, choice.target_skill_id));
        }

        private int CountChoices(string skillId, string choiceGroup)
        {
            int count = 0;
            for (int index = 0; index < selectedChoices.Count; index++)
            {
                SkillChoiceDefinition choice = selectedChoices[index];
                if (string.Equals(choice.skill_id, skillId, StringComparison.Ordinal)
                    && string.Equals(choice.choice_group, choiceGroup, StringComparison.Ordinal))
                {
                    count++;
                }
            }

            return count;
        }

        private void ValidateOwnedSkill(SkillDefinition definition)
        {
            if (!IsOwnedSkill(definition))
            {
                throw new ArgumentException(
                    "The skill does not belong to this monster.",
                    nameof(definition));
            }
        }

        private bool IsOwnedSkill(SkillDefinition definition)
        {
            return definition != null
                && string.Equals(
                    definition.monster_id,
                    OwnerDefinition.id,
                    StringComparison.Ordinal);
        }

        private static bool ContainsSkill<T>(IReadOnlyList<T> skills, string skillId)
            where T : SkillDefinition
        {
            for (int index = 0; index < skills.Count; index++)
            {
                if (string.Equals(skills[index].skill_id, skillId, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
