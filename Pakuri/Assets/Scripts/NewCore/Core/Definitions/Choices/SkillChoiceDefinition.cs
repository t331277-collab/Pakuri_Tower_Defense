/* 스킬 선택과 몬스터별 보상 선택 매핑의 CSV 정의를 소유한다. */
namespace Pakuri.NewCore.Definitions.Choices
{
    public class SkillChoiceDefinition : CsvDefinition
    {
        /* 스킬 선택 행의 필수 식별자와 표시 필드를 검증한다. */
        internal SkillChoiceDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(
                nameof(choice_id),
                nameof(skill_id),
                nameof(monster_id),
                nameof(choice_group),
                nameof(title),
                nameof(description_text));
        }

        public string choice_id => RequiredString(nameof(choice_id));

        public string skill_id => RequiredString(nameof(skill_id));

        public string monster_id => RequiredString(nameof(monster_id));

        public string target_skill_id => OptionalString(nameof(target_skill_id));

        public string choice_group => RequiredString(nameof(choice_group));

        public int? sort_order => OptionalInt(nameof(sort_order));

        public string title => RequiredString(nameof(title));

        public string description_text => RequiredString(nameof(description_text));
    }

    public class MonsterModifierSkillChoiceDefinition : CsvDefinition
    {
        /* 몬스터별 보상 선택 매핑의 필수 선택지와 몬스터 식별자를 검증한다. */
        internal MonsterModifierSkillChoiceDefinition(CsvDefinitionData data)
            : base(data)
        {
            ValidateRequired(nameof(choice_id), nameof(monster_id));
        }

        public string choice_id => RequiredString(nameof(choice_id));

        public string monster_id => RequiredString(nameof(monster_id));

        public string active_skill_id => OptionalString(nameof(active_skill_id));

        public string passive_skill_id => OptionalString(nameof(passive_skill_id));

        public int? sort_order => OptionalInt(nameof(sort_order));
    }
}
