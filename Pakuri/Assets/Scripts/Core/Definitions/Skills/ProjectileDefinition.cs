namespace Pakuri.NewCore.Definitions.Skills
{
    public sealed class ProjectileDefinition : SkillDefinition
    {
        internal ProjectileDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public int? magazine_capacity => OptionalInt(nameof(magazine_capacity));

        public float? reload_seconds => OptionalFloat(nameof(reload_seconds));

        public float? shot_interval_seconds => OptionalFloat(nameof(shot_interval_seconds));

        public int? projectile_burst_count => OptionalInt(nameof(projectile_burst_count));

        public float? projectile_speed => OptionalFloat(nameof(projectile_speed));

        public int? pierce_count => OptionalInt(nameof(pierce_count));

        public bool? critical_allowed => OptionalBool(nameof(critical_allowed));

        public string target_selection => OptionalString(nameof(target_selection));

        public string runtime_visual_sprite_path => OptionalString(nameof(runtime_visual_sprite_path));

        public string runtime_impact_visual_sprite_path =>
            OptionalString(nameof(runtime_impact_visual_sprite_path));
    }
}
