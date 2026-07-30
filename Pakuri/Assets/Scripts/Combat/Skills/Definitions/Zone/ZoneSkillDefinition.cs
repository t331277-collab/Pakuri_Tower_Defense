/*
 * 역할: 영역 스킬 Definition.
 * 책임: 영역 지속·주기·대상 수·피해·상태 적용값을 정의한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{
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
