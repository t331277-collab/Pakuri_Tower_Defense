using System;
using UnityEngine;

namespace Pakuri.InGame
{
    [Serializable]
    public sealed class SkillChoiceEffectSpec
    {
        [Header("Identity")]
        public string ChoiceId;
        public string Title;
        [TextArea(2, 5)] public string Description;
        public Sprite Icon;
        public GameObject SkillEffectPrefab;

        [Header("Multipliers")]
        public float DamageMultiplier = 1f;
        public float CooldownMultiplier = 1f;
        public float RadiusMultiplier = 1f;
        public float DurationMultiplier = 1f;

        [Header("Added Effect")]
        public string AddedStatusTag;
        public BuffModifierSpec AddedModifiers = new BuffModifierSpec();
    }
}
