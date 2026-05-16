using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Status Effect Data", fileName = "StatusEffectData")]
    public sealed class StatusEffectData : ScriptableObject
    {
        [Header("Identity")]
        public StatusEffectKind Kind = StatusEffectKind.None;
        public string StatusTag;
        public string StatusName;

        [Header("Stacking")]
        public bool IsStackable;
        public int MaxStacks;
        public float Duration;

        [Header("Effect")]
        public float TickDamageBase;
        public float MovementSlowRate;
        public bool IsControlEffect;
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Conditional Conversion")]
        public string TriggerConditionTag;
        public int TriggerConditionStacks;
    }
}
