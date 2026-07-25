/* 스킬 선택과 몬스터별 보상 선택 매핑의 CSV 정의를 소유한다. */
namespace Pakuri.NewCore.Definitions.Choices
{
    public class SkillChoiceDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 스킬 선택 정의를 구성한다. */
        internal SkillChoiceDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string choice_id => OptionalString(nameof(choice_id));

        public string skill_id => OptionalString(nameof(skill_id));

        public string monster_id => OptionalString(nameof(monster_id));

        public string target_skill_id => OptionalString(nameof(target_skill_id));

        public string choice_group => OptionalString(nameof(choice_group));

        public int? sort_order => OptionalInt(nameof(sort_order));

        public string title => OptionalString(nameof(title));

        public string description_text => OptionalString(nameof(description_text));
    }

    public class MonsterModifierSkillChoiceDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 몬스터별 보상 선택 매핑을 구성한다. */
        internal MonsterModifierSkillChoiceDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string choice_id => OptionalString(nameof(choice_id));

        public string monster_id => OptionalString(nameof(monster_id));

        public string active_skill_id => OptionalString(nameof(active_skill_id));

        public string passive_skill_id => OptionalString(nameof(passive_skill_id));

        public int? sort_order => OptionalInt(nameof(sort_order));
    }
}
