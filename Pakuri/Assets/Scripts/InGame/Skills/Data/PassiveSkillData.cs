using UnityEngine;

namespace Pakuri.InGame
{
    public sealed class PassiveSkillData : SkillData
    {
        [Header("Trigger")]
        public PassiveTrigger TriggerType;
        public string ConditionTag;
        public int ConditionMinStacks;
        [Range(0f, 1f)] public float TriggerChance = 1f;
        public int TriggerHitCount;
        public float InternalCooldown;

        [Header("Target")]
        public PassiveTarget ApplyTarget;
        public ElementType TargetElement;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();
        public float BuffDuration;

        [Header("Linked Skill")]
        public string LinkedSkillId;
        public float LinkedSkillPowerRate;

        [Header("Secondary Trigger")]
        public bool HasSecondaryTrigger;
        public PassiveTrigger SecondaryTriggerType;
        public string SecondaryConditionTag;
        public int SecondaryConditionMinStacks;
        [Range(0f, 1f)] public float SecondaryTriggerChance = 1f;
        public int SecondaryTriggerHitCount;
    }
}
