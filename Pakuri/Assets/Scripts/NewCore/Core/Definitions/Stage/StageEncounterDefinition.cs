namespace Pakuri.NewCore.Definitions.Stage
{
    public sealed class StageEncounterDefinition : StageDefinition
    {
        internal StageEncounterDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(encounter_id), nameof(enemy_id));
        }

        public string encounter_id => RequiredString(nameof(encounter_id));

        public int? spawn_order => OptionalInt(nameof(spawn_order));

        public string enemy_id => RequiredString(nameof(enemy_id));

        public int? count => OptionalInt(nameof(count));

        public float? interval_sec => OptionalFloat(nameof(interval_sec));

        public float? spawn_x => OptionalFloat(nameof(spawn_x));

        public float? spawn_y_min => OptionalFloat(nameof(spawn_y_min));

        public float? spawn_y_max => OptionalFloat(nameof(spawn_y_max));

        public bool? is_boss_candidate => OptionalBool(nameof(is_boss_candidate));

        public bool? is_guaranteed_boss => OptionalBool(nameof(is_guaranteed_boss));

        public float? boss_health_multiplier_min =>
            OptionalFloat(nameof(boss_health_multiplier_min));

        public float? boss_health_multiplier_max =>
            OptionalFloat(nameof(boss_health_multiplier_max));

        public bool? guaranteed_prisoner => OptionalBool(nameof(guaranteed_prisoner));

        public string notes => OptionalString(nameof(notes));
    }
}
