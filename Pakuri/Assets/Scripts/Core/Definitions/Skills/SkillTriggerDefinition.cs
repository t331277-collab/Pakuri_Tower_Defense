/* SkillTriggerDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Skills
{
    public class SkillTriggerDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 SkillTriggerDefinition 불변 정의를 구성한다. */
        internal SkillTriggerDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string trigger_id => OptionalString(nameof(trigger_id));

        public string monster_id => OptionalString(nameof(monster_id));

        public string source_skill_id => OptionalString(nameof(source_skill_id));

        public string trigger_event => OptionalString(nameof(trigger_event));

        public string triggered_skill_id => OptionalString(nameof(triggered_skill_id));

        public string runtime_kind => OptionalString(nameof(runtime_kind));

        public int? sort_order => OptionalInt(nameof(sort_order));

        public string target_side => OptionalString(nameof(target_side));

        public string target_selection => OptionalString(nameof(target_selection));

        public string target_shape => OptionalString(nameof(target_shape));

        public string center_mode => OptionalString(nameof(center_mode));

        public float? proc_chance => OptionalFloat(nameof(proc_chance));

        public string trigger_action => OptionalString(nameof(trigger_action));

        public float? internal_cooldown_seconds =>
            OptionalFloat(nameof(internal_cooldown_seconds));
    }
}
