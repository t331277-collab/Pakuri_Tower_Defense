using Pakuri.Combat;

/*
 * Single 돌진 스킬이 진행되는 동안 필요한 대상과 이동·피해 값을 보관한다.
 */
namespace Pakuri.InGame
{
    public sealed class SingleChargeState
    {
        public string SkillId;
        public string TargetUnitId;
        public float ElapsedSeconds;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public float DamageTargetMaxHealthRatio = 1f;
        public StatusApplicationSpec OnHitStatus;
        public DamageAttribute Attribute = DamageAttribute.Physical;
    }
}
