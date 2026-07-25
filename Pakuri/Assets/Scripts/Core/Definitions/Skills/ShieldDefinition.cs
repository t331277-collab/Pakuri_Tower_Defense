/* ShieldDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Skills
{
    public class ShieldDefinition : SkillDefinition
    {
        /* CSV 레코드의 열 값을 읽어 ShieldDefinition 불변 정의를 구성한다. */
        internal ShieldDefinition(CsvDefinitionData data)
            : base(data)
        {
        }
    }
}
