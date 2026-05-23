using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Zone Skill Data", fileName = "ZoneSkillData")]
    public sealed class ZoneSkillData : SkillData
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool HitAllTargets;
        [Min(1)] public int HitTargetCount = 1;

        [Header("Enemy Effect")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnTickStatus = new StatusApplicationSpec();

        [Header("Ally Effect")]
        public AllyEffectSpec AllyEffect = new AllyEffectSpec();
    }
}
