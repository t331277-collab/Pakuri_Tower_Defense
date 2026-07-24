namespace Pakuri.NewCore.Definitions.Units
{
    public sealed class CatalogMonsterDefinition : CsvDefinition
    {
        internal CatalogMonsterDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(id), nameof(monster_id));
        }

        public string id => RequiredString(nameof(id));

        public string monster_id => RequiredString(nameof(monster_id));

        public int? sort_order => OptionalInt(nameof(sort_order));
    }
}
