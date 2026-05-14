using System;
using UnityEngine;

namespace Pakuri.InGame
{
    public enum CharacterType
    {
        Eve,
        Ariel,
        Rin,
        Sein,
        Vega
    }

    public enum InGameSkillSlot
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J
    }

    public enum ElementType
    {
        Physical,
        Fire,
        Lightning,
        Ice,
        Darkness,
        Holy
    }

    public enum StatSource
    {
        Attack,
        Intelligence
    }

    public abstract class SkillData : ScriptableObject
    {
        [Header("Identity")]
        public string SkillId;
        public string SkillName;
        public CharacterType Character;
        public InGameSkillSlot Slot;
        public bool IsActive = true;
        public ElementType Element;
        [TextArea(2, 5)] public string Description;
        public Sprite Icon;

        [Header("Runtime Blueprint")]
        public SkillTimingSpec Timing = new SkillTimingSpec();
        public SkillTargetingSpec Targeting = new SkillTargetingSpec();

        [Header("Presentation")]
        public GameObject SkillEffectPrefab;

        [Header("Choices")]
        public SkillChoiceEffectSpec[] EnhancementChoices = Array.Empty<SkillChoiceEffectSpec>();
        public SkillChoiceEffectSpec[] MasterChoices = Array.Empty<SkillChoiceEffectSpec>();
    }
}
