/* SkillDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Skills
{
    public abstract class SkillDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 SkillDefinition 불변 정의를 구성한다. */
        internal SkillDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(skill_id));
        }

        public string skill_id => RequiredString(nameof(skill_id));

        public string monster_id => OptionalString(nameof(monster_id));

        public string skill_kind => OptionalString(nameof(skill_kind));

        public string slot => OptionalString(nameof(slot));

        public string display_name => OptionalString(nameof(display_name));

        public string runtime_kind => OptionalString(nameof(runtime_kind));

        public string description_text => OptionalString(nameof(description_text));

        public string summary => OptionalString(nameof(summary));

        public string attribute => OptionalString(nameof(attribute));

        public float? base_damage => OptionalFloat(nameof(base_damage));

        public float? attack_power_coefficient => OptionalFloat(nameof(attack_power_coefficient));

        public float? spell_power_coefficient => OptionalFloat(nameof(spell_power_coefficient));

        public float? cooldown_seconds => OptionalFloat(nameof(cooldown_seconds));

        public string status_effect_id => OptionalString(nameof(status_effect_id));
    }
}
