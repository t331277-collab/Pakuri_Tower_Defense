/* EnemyDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Units
{
    public sealed class EnemyDefinition : UnitDefinition
    {
        /* CSV 레코드의 열 값을 읽어 EnemyDefinition 불변 정의를 구성한다. */
        internal EnemyDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(
                nameof(enemy_id),
                nameof(stage_id),
                nameof(display_name),
                nameof(encounter_role),
                nameof(attack_type),
                nameof(attribute),
                nameof(skill_slot_a_id),
                nameof(skill_slot_b_id),
                nameof(passive_id));
        }

        public string enemy_id => RequiredString(nameof(enemy_id));

        public string stage_id => RequiredString(nameof(stage_id));

        public int? sort_order => OptionalInt(nameof(sort_order));

        public string display_name => RequiredString(nameof(display_name));

        public string encounter_role => RequiredString(nameof(encounter_role));

        public string attack_type => RequiredString(nameof(attack_type));

        public string attribute => RequiredString(nameof(attribute));

        public float? max_health => OptionalFloat(nameof(max_health));

        public float? attack_power => OptionalFloat(nameof(attack_power));

        public float? spell_power => OptionalFloat(nameof(spell_power));

        public float? move_speed => OptionalFloat(nameof(move_speed));

        public float? crit_chance => OptionalFloat(nameof(crit_chance));

        public float? crit_damage => OptionalFloat(nameof(crit_damage));

        public float? crit_resistance => OptionalFloat(nameof(crit_resistance));

        public float? def_physical => OptionalFloat(nameof(def_physical));

        public float? def_fire => OptionalFloat(nameof(def_fire));

        public float? def_lightning => OptionalFloat(nameof(def_lightning));

        public float? def_ice => OptionalFloat(nameof(def_ice));

        public float? def_darkness => OptionalFloat(nameof(def_darkness));

        public float? def_holy => OptionalFloat(nameof(def_holy));

        public string skill_slot_a_id => RequiredString(nameof(skill_slot_a_id));

        public string skill_slot_b_id => RequiredString(nameof(skill_slot_b_id));

        public string passive_id => RequiredString(nameof(passive_id));

        public float? nexus_damage => OptionalFloat(nameof(nexus_damage));
    }
}
