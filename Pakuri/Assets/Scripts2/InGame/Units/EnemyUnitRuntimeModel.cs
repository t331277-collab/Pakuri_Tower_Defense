using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.InGame
{
    public sealed class EnemyUnitRuntimeModel : BaseUnitRuntimeModel
    {
        public EnemyEncounterRole EncounterRole;
        public EnemyAttackType AttackType;
        public DamageAttribute Attribute;
    }
}
