namespace Pakuri.NewCore.Definitions.Choices
{
    public sealed class SkillChoiceDefinition : CsvDefinition
    {
        internal SkillChoiceDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(
                nameof(choice_id),
                nameof(skill_id),
                nameof(monster_id),
                nameof(choice_group),
                nameof(title),
                nameof(description_text));
        }

        public string choice_id => RequiredString(nameof(choice_id));

        public string skill_id => RequiredString(nameof(skill_id));

        public string monster_id => RequiredString(nameof(monster_id));

        public string target_skill_id => OptionalString(nameof(target_skill_id));

        public string choice_group => RequiredString(nameof(choice_group));

        public int? sort_order => OptionalInt(nameof(sort_order));

        public string title => RequiredString(nameof(title));

        public string description_text => RequiredString(nameof(description_text));
    }
}
