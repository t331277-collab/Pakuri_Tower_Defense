using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.InGame
{
    public sealed class EnemyUnitRuntimeModel : BaseUnitRuntimeModel
    {
        public EnemyEncounterRole EncounterRole;
        public EnemyAttackType AttackType;
        public DamageAttribute Attribute;
        public StageOneEnemySkillKind StageOneSkill;
        public float ActiveSkillCoefficient;
        public float ActiveSkillDuration;
        public float ActiveSkillRadius;
        public float ActiveSkillFlatValue;
        public float AttackAttemptRange;
        public float AttackAttemptCooldownSeconds;
        public float PassiveOutgoingDamageMultiplier = 1f;
        public float PassiveIncomingDamageMultiplier = 1f;
        public float PassiveHealingMultiplier = 1f;
        public float IncomingDamageMultiplier = 1f;
        public float IncomingDamageMultiplierRemainingSeconds;
        public float OutgoingDamageMultiplier = 1f;
        public float OutgoingDamageMultiplierRemainingSeconds;
        public float MoveSpeedMultiplier = 1f;
        public float MoveSpeedMultiplierRemainingSeconds;
    }
}
