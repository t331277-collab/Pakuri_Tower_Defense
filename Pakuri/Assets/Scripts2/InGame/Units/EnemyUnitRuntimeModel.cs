using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.InGame
{
    public sealed class EnemyUnitRuntimeModel : BaseUnitRuntimeModel
    {
        public EnemyEncounterRole EncounterRole;
        public EnemyAttackType AttackType;
        public DamageAttribute Attribute;
        public bool HasBasicSkill;
        public StageOneEnemySkillKind BasicSkill;
        public float BasicSkillCoefficient;
        public float BasicSkillAttackPowerCoefficient;
        public float BasicSkillSpellPowerCoefficient;
        public float BasicSkillDuration;
        public float BasicSkillRadius;
        public float BasicSkillFlatValue;
        public float BasicSkillProjectileSpeed;
        public float BasicSkillProjectileLifetime;
        public float BasicSkillMoveSpeedMultiplier = 1f;
        public float BasicSkillOutgoingDamageMultiplier = 1f;
        public EnemySkillPlanDefinition BasicSkillPlan;
        public float BasicSkillCooldownSeconds;
        public StageOneEnemySkillKind StageOneSkill;
        public float ActiveSkillCoefficient;
        public float ActiveSkillAttackPowerCoefficient;
        public float ActiveSkillSpellPowerCoefficient;
        public float ActiveSkillDuration;
        public float ActiveSkillRadius;
        public float ActiveSkillFlatValue;
        public float ActiveSkillProjectileSpeed;
        public float ActiveSkillProjectileLifetime;
        public float ActiveSkillMoveSpeedMultiplier = 1f;
        public float ActiveSkillOutgoingDamageMultiplier = 1f;
        public EnemySkillPlanDefinition ActiveSkillPlan;
        public float ActiveSkillCooldownSeconds;
        public float AttackAttemptRange;
        public float AttackAttemptCooldownSeconds;
        public string PassiveSkillId;
        public float PassiveSkillValue;
        public float NexusDamage = 1f;
        public float PassivePhysicalDamageMultiplier = 1f;
        public float PassiveFireDamageMultiplier = 1f;
        public float PassiveLightningDamageMultiplier = 1f;
        public float PassiveIceDamageMultiplier = 1f;
        public float PassiveDarknessDamageMultiplier = 1f;
        public float PassiveHolyDamageMultiplier = 1f;
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
