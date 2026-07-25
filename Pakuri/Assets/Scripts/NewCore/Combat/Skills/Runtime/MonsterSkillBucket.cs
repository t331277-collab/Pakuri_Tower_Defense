using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

/* 몬스터의 스킬 학습, 선택지 제한, 패시브 선행 조건을 관리한다. */
namespace Pakuri.NewCore.Combat.Skills.Runtime
{
    public sealed class MonsterSkillBucket : SkillBucket
    {
        public const int MaximumActiveSkills = 3;
        public const int MaximumPassiveSkills = 5;
        public const int MaximumActiveEnhancementsPerSkill = 3;
        public const int MaximumActiveMastersPerSkill = 1;
        public const int MaximumPassiveEnhancementsPerSkill = 3;

        private readonly List<SkillChoiceDefinition> selectedChoices =
            new List<SkillChoiceDefinition>();
        private readonly Dictionary<string, SkillChoiceDefinition> passiveBaseChoices =
            new Dictionary<string, SkillChoiceDefinition>(StringComparer.Ordinal);
        private readonly IReadOnlyList<SkillChoiceDefinition> readOnlySelectedChoices;

        /* 몬스터 정의의 기본 액티브와 PassiveBase 선행 조건 목록을 검증해 등록한다. */
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

        /* 소유권·슬롯·중복·학습 한도를 확인해 액티브 학습 가능 여부를 반환한다. */
        public bool CanLearnActive(SkillDefinition definition)
        {
            return IsOwnedSkill(definition)
                && !(definition is PassiveDefinition)
                && ActiveSkills.Count < MaximumActiveSkills
                && !ContainsSkill(ActiveSkills, definition.skill_id);
        }

        /* 학습 가능한 액티브를 등록하고 성공 여부를 반환한다. */
        public bool TryLearnActive(SkillDefinition definition)
        {
            if (!CanLearnActive(definition))
            {
                return false;
            }

            RegisterActive(definition);
            return true;
        }

        /* 소유권·중복·선행 액티브를 확인해 패시브 학습 가능 여부를 반환한다. */
        public bool CanLearnPassive(PassiveDefinition definition)
        {
            return IsOwnedSkill(definition)
                && PassiveSkills.Count < MaximumPassiveSkills
                && !ContainsSkill(PassiveSkills, definition.skill_id)
                && HasLearnedPassivePrerequisite(definition);
        }

        /* 학습 가능한 패시브를 등록하고 성공 여부를 반환한다. */
        public bool TryLearnPassive(PassiveDefinition definition)
        {
            if (!CanLearnPassive(definition))
            {
                return false;
            }

            RegisterPassive(definition);
            return true;
        }

        /* 선택지 소유권·선행 조건·그룹 한도를 확인해 선택 가능 여부를 반환한다. */
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

        /* 선택 가능한 Choice를 기록하고 성공 여부를 반환한다. */
        public bool TrySelectChoice(SkillChoiceDefinition choice)
        {
            if (!CanSelectChoice(choice))
            {
                return false;
            }

            selectedChoices.Add(choice);
            return true;
        }

        /* 동일 choice id가 이미 선택됐는지 확인한다. */
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

        /* 몬스터에 속한 PassiveBase Choice를 선행 조건 조회용으로 등록한다. */
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

        /* 패시브가 요구하는 액티브 슬롯의 스킬을 이미 학습했는지 확인한다. */
        private bool HasLearnedPassivePrerequisite(
            PassiveDefinition passive)
        {
            string requiredActiveSlot =
                ResolveRequiredActiveSlot(passive.slot);
            if (requiredActiveSlot == null
                || !ContainsActiveSlot(requiredActiveSlot))
            {
                return false;
            }

            return !passiveBaseChoices.TryGetValue(
                    passive.skill_id,
                    out SkillChoiceDefinition passiveBase)
                || string.IsNullOrEmpty(passiveBase.target_skill_id)
                || ContainsSkill(
                    ActiveSkills,
                    passiveBase.target_skill_id);
        }

        /* 지정 액티브 슬롯에 학습된 스킬이 있는지 확인한다. */
        private bool ContainsActiveSlot(string slot)
        {
            for (int index = 0; index < ActiveSkills.Count; index++)
            {
                if (string.Equals(
                    ActiveSkills[index].slot,
                    slot,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /* PassiveBase Choice가 요구하는 액티브 슬롯 식별자를 반환한다. */
        private static string ResolveRequiredActiveSlot(
            string passiveSlot)
        {
            if (string.IsNullOrWhiteSpace(passiveSlot)
                || passiveSlot.Length != 1)
            {
                return null;
            }

            char normalized = char.ToUpperInvariant(passiveSlot[0]);
            return normalized >= 'F' && normalized <= 'J'
                ? ((char)('A' + normalized - 'F')).ToString()
                : null;
        }

        /* Choice가 현재 몬스터에 구성된 PassiveBase인지 확인한다. */
        private bool IsConfiguredPassiveBase(SkillChoiceDefinition choice)
        {
            return passiveBaseChoices.TryGetValue(
                    choice.skill_id,
                    out SkillChoiceDefinition configured)
                && ReferenceEquals(configured, choice)
                && (string.IsNullOrEmpty(choice.target_skill_id)
                    || ContainsSkill(ActiveSkills, choice.target_skill_id));
        }

        /* 지정 스킬과 Choice 그룹에 이미 선택된 항목 수를 계산한다. */
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

        /* 스킬 정의가 현재 몬스터 소유인지 검증한다. */
        private void ValidateOwnedSkill(SkillDefinition definition)
        {
            if (!IsOwnedSkill(definition))
            {
                throw new ArgumentException(
                    "The skill does not belong to this monster.",
                    nameof(definition));
            }
        }

        /* 스킬 정의의 monster id가 현재 버킷 소유자와 일치하는지 확인한다. */
        private bool IsOwnedSkill(SkillDefinition definition)
        {
            return definition != null
                && string.Equals(
                    definition.monster_id,
                    OwnerDefinition.id,
                    StringComparison.Ordinal);
        }

        /* 스킬 목록에 동일 skill id가 포함되어 있는지 확인한다. */
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
