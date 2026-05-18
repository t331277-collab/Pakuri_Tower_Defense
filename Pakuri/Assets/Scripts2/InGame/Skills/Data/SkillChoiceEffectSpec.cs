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
        public bool HasDamageMultiplier;
        public float DamageMultiplier = 1f;
        public float CooldownMultiplier = 1f;
        public float RadiusMultiplier = 1f;
        public float DurationMultiplier = 1f;
        public bool HasMagazineBonus;
        public int MagazineBonus;
        public bool HasReloadTimeMultiplier;
        public float ReloadTimeMultiplier = 1f;
        public bool HasShotIntervalMultiplier;
        public float ShotIntervalMultiplier = 1f;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;

        [Header("Added Effect")]
        public string AddedStatusTag;
        public BuffModifierSpec AddedModifiers = new BuffModifierSpec();
    }
}
