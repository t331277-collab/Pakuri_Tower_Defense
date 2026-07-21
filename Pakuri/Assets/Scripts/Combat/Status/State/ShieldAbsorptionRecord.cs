/*
 * 상태 보호막이 흡수한 피해량과 해당 상태를 함께 전달한다.
 */
namespace Pakuri.InGame
{
    public readonly struct ShieldAbsorptionRecord
    {
        public ShieldAbsorptionRecord(StatusRuntimeInstance status, float absorbedAmount)
        {
            Status = status;
            AbsorbedAmount = absorbedAmount;
        }

        public StatusRuntimeInstance Status { get; }
        public float AbsorbedAmount { get; }
    }
}
