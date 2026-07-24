namespace Pakuri.NewCore.Definitions.Choices
{
    public sealed class NodeParamDefinition : CsvDefinition
    {
        internal NodeParamDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(node_type_id), nameof(param_key), nameof(value_type));
        }

        public string node_type_id => RequiredString(nameof(node_type_id));

        public int? param_order => OptionalInt(nameof(param_order));

        public string param_key => RequiredString(nameof(param_key));

        public string value_type => RequiredString(nameof(value_type));

        public bool? required => OptionalBool(nameof(required));

        public string allowed_values => OptionalString(nameof(allowed_values));
    }
}
