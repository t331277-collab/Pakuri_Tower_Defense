/* 몬스터 런타임 정의와 카탈로그 순서 매핑의 CSV 데이터를 소유한다. */
namespace Pakuri.NewCore.Definitions.Units
{
    public class MonsterDefinition : UnitDefinition
    {
        /* CSV 레코드의 열 값을 읽어 몬스터 정의를 구성한다. */
        internal MonsterDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string id => OptionalString(nameof(id));

        public string display_name => OptionalString(nameof(display_name));

        public string role_summary => OptionalString(nameof(role_summary));

        public string element_label => OptionalString(nameof(element_label));

        public string primary_attribute => OptionalString(nameof(primary_attribute));

        public float? max_health => OptionalFloat(nameof(max_health));

        public float? power_stat => OptionalFloat(nameof(power_stat));

        public float? base_damage => OptionalFloat(nameof(base_damage));

        public float? power_coefficient => OptionalFloat(nameof(power_coefficient));

        public float? base_attack_power => OptionalFloat(nameof(base_attack_power));

        public float? base_spell_power => OptionalFloat(nameof(base_spell_power));

        public float? base_move_speed => OptionalFloat(nameof(base_move_speed));

        public float? base_crit_chance => OptionalFloat(nameof(base_crit_chance));

        public float? base_crit_damage => OptionalFloat(nameof(base_crit_damage));

        public float? base_crit_resistance => OptionalFloat(nameof(base_crit_resistance));

        public float? def_physical => OptionalFloat(nameof(def_physical));

        public float? def_fire => OptionalFloat(nameof(def_fire));

        public float? def_lightning => OptionalFloat(nameof(def_lightning));

        public float? def_ice => OptionalFloat(nameof(def_ice));

        public float? def_darkness => OptionalFloat(nameof(def_darkness));

        public float? def_holy => OptionalFloat(nameof(def_holy));

        public string MonsterIconImage => OptionalString(nameof(MonsterIconImage));
    }

    public class CatalogMonsterDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 카탈로그 몬스터 정의를 구성한다. */
        internal CatalogMonsterDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string id => OptionalString(nameof(id));

        public string monster_id => OptionalString(nameof(monster_id));

        public int? sort_order => OptionalInt(nameof(sort_order));
    }
}
