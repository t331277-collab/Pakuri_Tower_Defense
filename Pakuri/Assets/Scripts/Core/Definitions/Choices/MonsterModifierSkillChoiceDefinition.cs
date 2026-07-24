namespace Pakuri.NewCore.Definitions.Choices
{
    public sealed class MonsterModifierSkillChoiceDefinition : CsvDefinition
    {
        internal MonsterModifierSkillChoiceDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(choice_id), nameof(monster_id));
        }

        public string choice_id => RequiredString(nameof(choice_id));

        public string monster_id => RequiredString(nameof(monster_id));

        public string active_skill_id => OptionalString(nameof(active_skill_id));

        public string passive_skill_id => OptionalString(nameof(passive_skill_id));

        public int? sort_order => OptionalInt(nameof(sort_order));
    }
}
