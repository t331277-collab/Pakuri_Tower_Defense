/* ProjectileDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Skills
{
    public class ProjectileDefinition : SkillDefinition
    {
        /* CSV 레코드의 열 값을 읽어 ProjectileDefinition 불변 정의를 구성한다. */
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
