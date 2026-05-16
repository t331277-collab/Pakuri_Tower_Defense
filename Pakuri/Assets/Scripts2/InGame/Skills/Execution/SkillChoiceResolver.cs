using System.Collections.Generic;

namespace Pakuri.InGame
{
    public sealed class SkillChoiceResolver
    {
        private SkillChoiceModifierLibrary modifierLibrary = new SkillChoiceModifierLibrary();

        public int ModifierRecordCount => modifierLibrary != null ? modifierLibrary.Count : 0;

        public void SetModifierLibrary(SkillChoiceModifierLibrary library)
        {
            modifierLibrary = library ?? new SkillChoiceModifierLibrary();
        }

        public SkillExecutionSnapshot Resolve(BaseUnitRuntimeModel owner, SkillRuntimeInstance runtime)
        {
            var skillData = runtime != null ? runtime.Data : null;
            var snapshot = new SkillExecutionSnapshot(skillData);
            var monsterOwner = owner as MonsterUnitRuntimeModel;
            var chosenChoiceIds = monsterOwner != null && monsterOwner.State != null
                ? monsterOwner.State.ChosenChoiceIds
                : null;
            if (skillData == null || chosenChoiceIds == null || chosenChoiceIds.Count == 0)
            {
                return snapshot;
            }

            ApplyChoices(snapshot, chosenChoiceIds, skillData.EnhancementChoices);
            ApplyChoices(snapshot, chosenChoiceIds, skillData.MasterChoices);
            ApplyModifierRecords(snapshot, chosenChoiceIds, skillData);
            return snapshot;
        }

        private static void ApplyChoices(
            SkillExecutionSnapshot snapshot,
            ICollection<string> chosenChoiceIds,
            SkillChoiceEffectSpec[] choices)
        {
            if (snapshot == null || chosenChoiceIds == null || choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && chosenChoiceIds.Contains(choice.ChoiceId))
                {
                    snapshot.ApplyChoiceSpec(choice);
                }
            }
        }

        private void ApplyModifierRecords(
            SkillExecutionSnapshot snapshot,
            ICollection<string> chosenChoiceIds,
            SkillData skillData)
        {
            if (snapshot == null || chosenChoiceIds == null || modifierLibrary == null || skillData == null)
            {
                return;
            }

            foreach (var choiceId in chosenChoiceIds)
            {
                if (IsChoiceForSkill(choiceId, skillData)
                    && modifierLibrary.TryGet(choiceId, out var record))
                {
                    snapshot.ApplyModifierRecord(record);
                }
            }
        }

        private static bool IsChoiceForSkill(string choiceId, SkillData skillData)
        {
            if (string.IsNullOrWhiteSpace(choiceId) || skillData == null)
            {
                return false;
            }

            return ContainsChoice(skillData.EnhancementChoices, choiceId)
                || ContainsChoice(skillData.MasterChoices, choiceId);
        }

        private static bool ContainsChoice(SkillChoiceEffectSpec[] choices, string choiceId)
        {
            if (choices == null)
            {
                return false;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && string.Equals(choice.ChoiceId, choiceId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
