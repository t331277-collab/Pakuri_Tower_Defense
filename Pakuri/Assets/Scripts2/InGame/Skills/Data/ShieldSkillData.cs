using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Shield Skill Data", fileName = "ShieldSkillData")]
    public sealed class ShieldSkillData : SkillData
    {
        [Header("Shield")]
        public BuffTarget Target;
        public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public ShieldRefreshRule RefreshRule;

        [Header("Reflect")]
        public bool CanReflectDamage;
        public float ReflectDamageRate;
        public ElementType ReflectElement;
    }
}
