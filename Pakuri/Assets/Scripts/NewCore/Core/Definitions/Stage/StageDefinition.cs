/* StageDefinition CSV 레코드를 형식화된 불변 런타임 정의로 표현한다. */
namespace Pakuri.NewCore.Definitions.Stage
{
    public abstract class StageDefinition : CsvDefinition
    {
        /* CSV 레코드의 열 값을 읽어 StageDefinition 불변 정의를 구성한다. */
        internal StageDefinition(CsvDefinitionData data)
            : base(data)
        {
        }
    }
}
