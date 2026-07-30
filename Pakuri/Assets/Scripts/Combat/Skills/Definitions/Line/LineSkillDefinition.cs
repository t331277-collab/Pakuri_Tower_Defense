/*
 * 역할: 선형 스킬 Definition.
 * 책임: 선형 범위·반복·넉백·피해·상태 적용값을 정의한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{
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
