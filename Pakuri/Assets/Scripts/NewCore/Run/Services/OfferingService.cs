using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 포로를 사용한 스킬 선택지 후보 생성과 확정을 관리한다. */
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
        /* Offering에 표시할 선택지 종류와 정의를 하나의 후보 값으로 묶는다. */
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
        /* 대상 몬스터·포로와 화면에 노출할 후보 목록을 하나의 offer로 묶는다. */
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

        /* Offering 후보 생성과 확정에 필요한 카탈로그·stage·난수 공급원을 연결한다. */
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

        /* 몬스터와 포로 소유권을 확인하고 학습 가능한 후보를 섞어 제한 수만 노출한다. */
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

        /* 현재 offer의 후보 id를 확정해 Choice를 적용하고 포로를 소비한다. */
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

        /* 몬스터가 학습·선택할 수 있는 액티브·패시브 Choice 후보를 구성한다. */
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

        /* 후보 종류에 따라 액티브·패시브 학습 또는 Choice 선택을 적용한다. */
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

        /* 몬스터와 패시브 skill id에 정확히 일치하는 PassiveBase Choice를 찾는다. */
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

        /* 몬스터가 현재 파티 소유이고 포로가 현재 인벤토리에 있는지 확인한다. */
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

        /* Offering 후보 순서를 난수 공급원으로 섞는다. */
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

        /* 난수 공급원이 반환한 index가 후보 범위 안인지 검증한다. */
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

        /* Choice 그룹 문자열을 Offering 후보 종류로 변환할 수 있는지 확인한다. */
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
