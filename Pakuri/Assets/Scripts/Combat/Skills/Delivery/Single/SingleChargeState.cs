/*
 * 역할: 단일 차지 런타임 데이터.
 * 책임: 대기 중인 차지의 시전자·대상·확정 스킬 데이터·시간·취소 상태를 보관한다.
 */

using Pakuri.Combat;

namespace Pakuri.InGame
{

    /// <summary><c>SingleChargeState</c>의 변경 가능한 런타임 상태를 보관한다.</summary>
    public class SingleChargeState
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
