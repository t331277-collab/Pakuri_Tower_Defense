/*
 * 역할: 일정 공간에 남는 공격의 설계값을 정의한다.
 * 책임: 범위와 지속시간, 적용 주기, 대상 수, 피해와 상태 효과를 제공한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{
    /// 지속 영역의 주기와 대상별 결과를 설계한다.
    public class ZoneSkillDefinition : SkillDefinition
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool HitAllTargets;
        public int HitTargetCount = 1;

        [Header("Enemy Effect")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnTickStatus = new StatusApplicationSpec();
    }
}
