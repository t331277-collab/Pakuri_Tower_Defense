using UnityEngine;

namespace Pakuri.InGame
{
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
