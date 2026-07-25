/* 넥서스의 체력 상태와 피해 적용 권한을 표현한다. */
namespace Pakuri.NewCore.Units.Models
{
    public class NexusModel : UnitBaseModel
    {
        /* 별도 유닛 정의 없이 지정 최대 체력으로 넥서스 모델을 구성한다. */
        public NexusModel(float maximumHealth)
            : base(null, maximumHealth)
        {
        }

        /* 넥서스 체력에 피해를 적용하고 실제 감소량을 반환한다. */
        public float ApplyNexusDamage(float amount)
        {
            return ApplyDamage(amount);
        }
    }
}
