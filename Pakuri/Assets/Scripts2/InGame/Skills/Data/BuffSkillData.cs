using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Buff Skill Data", fileName = "BuffSkillData")]
    public sealed class BuffSkillData : SkillData
    {
        [Header("Buff")]
        public float BuffDuration;
        public BuffTarget Target;
        public string ApplyStatusTag;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Attached Damage")]
        public bool HasAttachedDamage;
        public SkillDamageSpec AttachedDamage = new SkillDamageSpec();
        public float AttachedDamageRadius;
        public StatusApplicationSpec AttachedStatus = new StatusApplicationSpec();
    }
}
