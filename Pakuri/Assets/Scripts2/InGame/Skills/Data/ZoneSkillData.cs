using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Zone Skill Data", fileName = "ZoneSkillData")]
    public sealed class ZoneSkillData : SkillData
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();

        [Header("Enemy Effect")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnTickStatus = new StatusApplicationSpec();

        [Header("Ally Effect")]
        public AllyEffectSpec AllyEffect = new AllyEffectSpec();
    }
}
