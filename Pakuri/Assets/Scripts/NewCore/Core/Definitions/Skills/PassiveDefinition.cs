/* PassiveDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Skills
{
    public sealed class PassiveDefinition : SkillDefinition
    {
        /* CSV 레코드의 열 값을 읽어 PassiveDefinition 불변 정의를 구성한다. */
        internal PassiveDefinition(CsvDefinitionData data)
            : base(data)
        {
        }

        public string modifier_kind => OptionalString(nameof(modifier_kind));

        public float? modifier_value => OptionalFloat(nameof(modifier_value));
    }
}
