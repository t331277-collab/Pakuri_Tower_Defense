using System;
using Pakuri.Combat;
using Pakuri.Data;
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
        public float BeamWidthBonus;
        public bool HasKnockbackDistanceMultiplier;
        public float KnockbackDistanceMultiplier = 1f;
        public bool HasDamageDelayMultiplier;
        public float DamageDelayMultiplier = 1f;
        public bool HasExecuteHealthRatioBonus;
        public float ExecuteHealthRatioBonus;
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
        public bool HasBurstDamageProjectileIndex;
        public int BurstDamageProjectileIndex;
        public bool HasBurstDamageMultiplier;
        public float BurstDamageMultiplier = 1f;
        public int FollowUpProjectileCount;
        public float FollowUpProjectileDelaySeconds;
        public float FollowUpProjectileDamageMultiplier = 1f;
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
        public int BranchLaunchPeriod;
        public bool HasBranchLaunchChanceSet;
        public float BranchLaunchChanceSet;
        public int StatusStacksBonus;
        public bool HasStatusStacksSet;
        public int StatusStacksSet;
        public int HitTargetCountBonus;
        public float CritChanceBonus;
        public float CritDamageBonus;
        public float ExecuteCritChanceBonus;
        public bool HasBossDamageMultiplier;
        public float BossDamageMultiplier = 1f;
        public bool HasKillCooldownRefundRatioBonus;
        public float KillCooldownRefundRatioBonus;
        public bool KillResetsCooldown;
        public bool KillResetsCooldownRequiresExecute;
        public bool HasStatusElementDamageTakenBonus;
        public float StatusElementDamageTakenBonus;
        public bool HasStatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public bool HasStatusAilmentResistanceBonus;
        public float StatusAilmentResistanceBonus;
        public string StatusMaxStacksBonusStatusId;
        public int StatusMaxStacksBonus;
        public string StatusDurationBonusStatusId;
        public float StatusDurationBonus;
        public string ThresholdStatusId;
        public int ThresholdStatusMinStacks;
        public string ThresholdApplyStatusId;
        public bool HasConditionalDamageMultiplier;
        public float ConditionalDamageMultiplier = 1f;
        public string ConditionalTargetStatusId;
        public int ConditionalTargetStatusMinStacks;
        public string CountStatusId;
        public SkillMultiEffectTargetSide CountTargetSide;
        public float DamageMultiplierPerCount;
        public int CountMax;
        public float ConsecutiveHitBonusRate;
        public float ConsecutiveHitMax;
        public bool HasStatusConditionalDamageTakenBonus;
        public float StatusConditionalDamageTakenBonus;
        public string StatusConditionalSourceStatusId;
        public bool HasMaxHealthBonus;
        public float MaxHealthBonus;
        public bool HasOnHitAdditionalDamage;
        public float OnHitAdditionalDamageChance;
        public float OnHitAdditionalDamageMultiplier = 1f;
        public DamageAttribute OnHitAdditionalDamageAttribute;
        public string OnHitAdditionalDamageTarget;
        public int OnHitChainHitPeriod;
        public int OnHitChainTargetCount;
        public float OnHitChainSearchRadius;
        public float OnHitChainDamageMultiplier = 1f;
        public DamageAttribute OnHitChainDamageAttribute;
        public string ReloadReduceTargetSkillId;
        public float ReloadReduceSecondsPerHit;
        public string CoreHitboxName;
        public bool HasCoreDamageMultiplier;
        public float CoreDamageMultiplier = 1f;
        public bool HasCoreOnHitAdditionalDamage;
        public float CoreOnHitAdditionalDamageChance;
        public float CoreOnHitAdditionalDamageMultiplier = 1f;
        public DamageAttribute CoreOnHitAdditionalDamageAttribute;
        public string HitCountCooldownRefundTargetSkillId;
        public int HitCountCooldownRefundMinTargets;
        public float HitCountCooldownRefundRatio;

        [Header("Added Effect")]
        public string StatusTag;
        public BuffModifierSpec AddedModifiers = new BuffModifierSpec();
    }
}
