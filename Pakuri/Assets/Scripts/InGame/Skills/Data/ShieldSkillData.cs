using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class ShieldSkillData : SkillData
    {
        [Header("Shield")]
        public BuffTarget Target;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public ShieldRefreshRule RefreshRule;
        public StatusEffectData ShieldStatus;

        [Header("Reflect")]
        public bool CanReflectDamage;
        public float ReflectDamageRate;
        public ElementType ReflectElement;
    }
}
