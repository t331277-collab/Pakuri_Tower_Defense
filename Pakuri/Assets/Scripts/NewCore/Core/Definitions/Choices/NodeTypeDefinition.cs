namespace Pakuri.NewCore.Definitions.Choices
{
    public sealed class NodeTypeDefinition : CsvDefinition
    {
        internal NodeTypeDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(node_type_id), nameof(handler_id), nameof(node_kind));
        }

        public string node_type_id => RequiredString(nameof(node_type_id));

        public string handler_id => RequiredString(nameof(handler_id));

        public string node_kind => RequiredString(nameof(node_kind));

        public string runtime_support_state => OptionalString(nameof(runtime_support_state));

        public string runtime_support_notes => OptionalString(nameof(runtime_support_notes));
    }
}
