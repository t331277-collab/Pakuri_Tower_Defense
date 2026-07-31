/*
 * 역할: 직선형 공격의 설계값을 정의한다.
 * 책임: 공격 폭과 길이, 반복 간격, 밀어내기, 피해와 상태 효과를 제공한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{
    /// 직선 영역이 유지될 방식과 적중 결과를 설계한다.
    public class LineSkillDefinition : SkillDefinition
    {
        [Header("Line")]
        public float LineWidth;
        public float LineLength;
        public int CastRepeatCount = 1;
        public float CastRepeatIntervalSeconds;
        public float KnockbackDistance;

        [Header("Tick Damage")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }
}
