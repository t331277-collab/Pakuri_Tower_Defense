using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Beam Skill Data", fileName = "BeamSkillData")]
    public sealed class BeamSkillData : SkillData
    {
        [Header("Beam")]
        public float BeamWidth;
        public float BeamLength;
        public float KnockbackDistance;
        public bool StopAtFirstTarget;

        [Header("Tick Damage")]
        public SkillDamageSpec DamagePerTick = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }
}
