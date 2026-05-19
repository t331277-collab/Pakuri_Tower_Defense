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
        public float BaseDamageBonus;
        public bool HasCooldownMultiplier;
        public float CooldownMultiplier = 1f;
        public bool HasRadiusMultiplier;
        public float RadiusMultiplier = 1f;
        public float RadiusBonus;
        public bool HasDurationMultiplier;
        public float DurationMultiplier = 1f;
        public float DurationBonus;
        public bool HasMagazineBonus;
        public int MagazineBonus;
        public int AdditionalProjectileBonus;
        public int PierceBonus;
        public bool HasReloadTimeMultiplier;
        public float ReloadTimeMultiplier = 1f;
        public bool HasShotIntervalMultiplier;
        public float ShotIntervalMultiplier = 1f;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;
        public float BranchChanceBonus;
        public bool HasBranchChanceSet;
        public float BranchChanceSet;
        public bool HasBranchCount;
        public int BranchCount;
        public bool HasBranchDamageMultiplier;
        public float BranchDamageMultiplier = 1f;
        public bool HasBranchSearchRadius;
        public float BranchSearchRadius;
        public int StatusStacksBonus;
        public bool HasStatusStacksSet;
        public int StatusStacksSet;
        public bool HasMaxHealthBonus;
        public float MaxHealthBonus;

        [Header("Added Effect")]
        public string StatusTag;
        public BuffModifierSpec AddedModifiers = new BuffModifierSpec();
    }
}
