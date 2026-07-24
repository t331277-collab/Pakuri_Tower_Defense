namespace Pakuri.NewCore.Definitions.Skills
{
    public sealed class PassiveDefinition : SkillDefinition
    {
        internal PassiveDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string modifier_kind => OptionalString(nameof(modifier_kind));

        public float? modifier_value => OptionalFloat(nameof(modifier_value));
    }
}
