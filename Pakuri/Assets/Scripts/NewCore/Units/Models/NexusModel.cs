namespace Pakuri.NewCore.Units.Models
{
    public sealed class NexusModel : UnitBaseModel
    {
        public NexusModel(float maximumHealth)
            : base(null, maximumHealth)
        {
        }

        public float ApplyNexusDamage(float amount)
        {
            return ApplyDamage(amount);
        }
    }
}
