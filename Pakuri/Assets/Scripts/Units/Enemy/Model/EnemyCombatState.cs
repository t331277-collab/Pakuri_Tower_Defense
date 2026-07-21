using Pakuri.Combat;
using Pakuri.Data;

/*
 * 적 전투에만 필요한 역할, 공격 속성, 넥서스 피해와 패시브 배율을 보관한다.
 */
namespace Pakuri.InGame
{
    public sealed class EnemyCombatState : UnitCombatState
    {
        public EnemyEncounterRole EncounterRole;
        public EnemyAttackType AttackType;
        public DamageAttribute Attribute;
        public float NexusDamage = 1f;
        public float PassivePhysicalDamageMultiplier = 1f;
        public float PassiveFireDamageMultiplier = 1f;
        public float PassiveLightningDamageMultiplier = 1f;
        public float PassiveIceDamageMultiplier = 1f;
        public float PassiveDarknessDamageMultiplier = 1f;
        public float PassiveHolyDamageMultiplier = 1f;
        public float PassiveIncomingDamageMultiplier = 1f;
        public float PassiveHealingMultiplier = 1f;
    }
}
