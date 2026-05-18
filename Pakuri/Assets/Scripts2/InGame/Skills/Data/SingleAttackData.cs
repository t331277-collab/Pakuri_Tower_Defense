using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Single Attack Data", fileName = "SingleAttackData")]
    public sealed class SingleAttackData : SkillData
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();

        [Header("Enemy Effect")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();

        [Header("Ally Effect")]
        public AllyEffectSpec AllyEffect = new AllyEffectSpec();
    }
}
