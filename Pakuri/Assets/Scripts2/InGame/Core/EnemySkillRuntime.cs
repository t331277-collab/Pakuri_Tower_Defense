using Pakuri.Data;

namespace Pakuri.InGame
{
    internal enum EnemySkillSlotType
    {
        None,
        Basic,
        Special
    }

    internal struct EnemyResolvedSkillData
    {
        public EnemySkillSlotType SlotType;
        public StageOneEnemySkillKind SkillKind;
        public float Coefficient;
        public float CooldownSeconds;
        public float Duration;
        public float Radius;
        public float FlatValue;
        public float ProjectileSpeed;
        public float ProjectileLifetime;
        public float MoveSpeedMultiplier;
        public float OutgoingDamageMultiplier;
        public bool IsAssigned;
    }
}
