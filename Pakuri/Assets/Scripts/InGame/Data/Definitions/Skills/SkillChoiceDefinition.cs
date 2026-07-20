using System;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * 선택지가 액티브·패시브의 어느 성장 단계인지 구분한다.
     */
    public enum SkillChoiceGroup
    {
        ActiveEnhancement,
        ActiveMaster,
        PassiveEnhancement,
        PassiveBase
    }

    /*
     * 스킬 성장 선택지가 변경할 전투 수치와 조건을 보관한다.
     */
    [Serializable]
    public class SkillChoiceDefinition
    {
        // 선택지 식별과 표시 정보
        public string ChoiceId;
        public string MonsterId;
        public string SkillId;
        public string TargetSkillId;
        public string RuntimeTargetSkillIds;
        public SkillChoiceGroup ChoiceGroup;
        public string Title;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        // 기본 피해, 재사용 대기시간, 탄창 변경
        public bool HasDamageMultiplier;
        public float DamageMultiplier = 1f;
        public float BaseDamageBonus;
        public bool HasCooldownMultiplier;
        public float CooldownMultiplier = 1f;
        public bool HasMagazineBonus;
        public int MagazineBonus;
        // 투사체와 연속 발사 변경
        public int AdditionalProjectileBonus;
        public int PierceBonus;
        public bool HasShotIntervalMultiplier;
        public float ShotIntervalMultiplier = 1f;
        public bool HasBurstDamageProjectileIndex;
        public int BurstDamageProjectileIndex;
        public bool HasBurstDamageMultiplier;
        public float BurstDamageMultiplier = 1f;
        public bool HasBurstStatusProjectileIndex;
        public int BurstStatusProjectileIndex;
        public int BurstStatusStacksBonus;
        public int FollowUpProjectileCount;
        public float FollowUpProjectileDelaySeconds;
        public float FollowUpProjectileDamageMultiplier = 1f;
        public bool HasReloadTimeMultiplier;
        public float ReloadTimeMultiplier = 1f;
        // 범위, 지속시간, 분기 공격 변경
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
        public bool HasMaxHealthBonus;
        public float MaxHealthBonus;
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
        // 상태 적용과 조건부 피해 변경
        public string StatusTag;
        public bool HasStatusChanceBonus;
        public float StatusChanceBonus;
        public bool HasStatusActionSpeedBonus;
        public float StatusActionSpeedBonus;
        public bool HasStatusAttackPowerBonus;
        public float StatusAttackPowerBonus;
        public int StatusStacksBonus;
        public bool HasStatusStacksSet;
        public int StatusStacksSet;
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
        public bool HasTargetStatusStackDamageMultiplier;
        public float TargetStatusStackDamageMultiplier = 1f;
        public bool HasConsumeTargetStatusRatioOverride;
        public float ConsumeTargetStatusRatioOverride;
        public bool HasConsumeTargetStatusStacksOverride;
        public int ConsumeTargetStatusStacksOverride;
        public float ConditionalCritChanceBonus;
        public string ConditionalCritTargetStatusId;
        public int ConditionalCritTargetStatusMinStacks;
        public float RedistributeConsumedStatusRatioOnKill;
        public string RedistributeConsumedStatusId;
        public float RedistributeConsumedStatusSearchRadius;
        public int RedistributeConsumedStatusTargetCount;
        public string CountStatusId;
        public SkillMultiEffectTargetSide CountTargetSide;
        public float DamageMultiplierPerCount;
        public int CountMax;
        public float ConsecutiveHitBonusRate;
        public float ConsecutiveHitMax;
        public bool HasStatusConditionalDamageTakenBonus;
        public float StatusConditionalDamageTakenBonus;
        public string StatusConditionalSourceStatusId;
        public string RequiredSourceStatusId;
        public int RequiredSourceStatusMinStacks;
        // 추가 타격, 연쇄 공격, 핵심 충돌 영역 변경
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
        public int RepeatCountPerTarget;
        public float RepeatIntervalSeconds;
        public float RepeatDamageMultiplier = 1f;
        // 정규화 그래프와 런타임 지원 상태
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
        public string RuntimeSupportState;
        [TextArea(2, 5)] public string RuntimeSupportNotes;
    }
}
