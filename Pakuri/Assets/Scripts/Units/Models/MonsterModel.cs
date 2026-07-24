using System;
using System.Collections.Generic;
using Pakuri.NewCore.Combat.Skills.Runtime;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Units;

namespace Pakuri.NewCore.Units.Models
{
    public sealed class MonsterModel : UnitBaseModel
    {
        public MonsterModel(
            MonsterDefinition definition,
            SkillDefinition defaultActiveSkill,
            IEnumerable<SkillChoiceDefinition> passiveBaseChoices,
            bool autoSkillEnabled)
            : base(
                definition ?? throw new ArgumentNullException(nameof(definition)),
                RequiredMaximumHealth(definition))
        {
            MonsterDefinition = definition;
            SkillBucket = new MonsterSkillBucket(
                definition,
                defaultActiveSkill,
                passiveBaseChoices);
            AutoAttackEnabled = true;
            AutoSkillEnabled = autoSkillEnabled;
        }

        public MonsterDefinition MonsterDefinition { get; }

        public MonsterSkillBucket SkillBucket { get; }

        public bool AutoAttackEnabled { get; private set; }

        public bool AutoSkillEnabled { get; private set; }

        public void SetAutoAttackEnabled(bool enabled)
        {
            AutoAttackEnabled = enabled;
        }

        public void SetAutoSkillEnabled(bool enabled)
        {
            AutoSkillEnabled = enabled;
        }

        public void ResetForNextDay(bool isSelectedMonster)
        {
            ResetVitalsAndStatuses();
            SkillBucket.ResetRuntimeState();
            AutoAttackEnabled = true;
            AutoSkillEnabled = !isSelectedMonster;
        }

        private static float RequiredMaximumHealth(MonsterDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return definition.max_health
                ?? throw new ArgumentException(
                    "Monster definition has no max_health.",
                    nameof(definition));
        }
    }
}
