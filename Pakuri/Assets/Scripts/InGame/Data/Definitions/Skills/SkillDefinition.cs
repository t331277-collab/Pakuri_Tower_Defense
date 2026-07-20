using System;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * 몬스터가 보유할 수 있는 스킬 슬롯을 구분한다.
     */
    public enum SkillSlot
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

    /*
     * 스킬 데이터가 런타임에서 지원되는 단계를 구분한다.
     */
    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

    /*
     * 스킬을 실행할 공용 런타임 종류를 구분한다.
     */
    public enum SkillRuntimeKind
    {
        MagazineProjectile,
        CooldownProjectile,
        LineAttack,
        AreaAttack,
        SingleAttack,
        Field,
        Buff,
        Shield,
        Heal,
        Mark,
        Execute,
        Passive
    }

    /*
     * 액티브 스킬의 실행 종류, 피해, 투사체, 상태, 성장 선택지를 보관한다.
     */
    [Serializable]
    public class SkillDefinition
    {
        // 스킬 식별과 표시 정보
        public string SkillId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public bool IsDefaultLearned;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        [TextArea(2, 5)] public string DescriptionText;
        // 기본 피해와 대상 범위
        public DamageAttribute Attribute;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool UseCombinedStatCoefficients;
        public float Radius;
        public float CastRange;
        public float EffectRadius;
        public string TargetScope;
        // 특수 실행 프로필과 전용 수치
        public string ExecutionProfile;
        public float FlatValue;
        public float ProjectileLifetimeSeconds;
        public float IncomingDamageMultiplier = 1f;
        public float MoveSpeedMultiplier = 1f;
        public float OutgoingDamageMultiplier = 1f;
        public float ChainDamageMultiplier;
        public float ChainDelaySeconds;
        public float ChainRadius;
        public bool ExcludePrimaryTarget;
        public float TargetMaxHealthRatio;
        public float ChargeRampSeconds = 3f;
        public float ChargeMoveSpeedMultiplier = 2.5f;
        public float KnockbackDistance;
        public float DamageDelaySeconds;
        [Range(0f, 1f)] public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        [Range(0f, 1f)] public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;
        // 명중 대상 수와 대상 선택
        public string HitTargetCount;
        public bool UsePrefabHitbox;
        public string TargetSelection;
        public string TargetSelectionStatusId;
        public int TargetSelectionStatusMinStacks;
        // 재사용 대기시간과 투사체 동작
        public float CooldownSeconds;
        public float ActiveDurationSeconds;
        public int MagazineCapacity;
        public float ReloadSeconds;
        public float ShotIntervalSeconds;
        public float BurstIntervalSeconds;
        public int ProjectileBurstCount;
        public int BurstDamageProjectileIndex;
        public float BurstDamageMultiplier = 1f;
        public float ProjectileSpeed;
        public int PierceCount;
        public bool CriticalAllowed = true;
        // 대상 상태 중첩을 이용하는 공격 설정
        public string DeploymentRequiredTargetStatusId;
        public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusId;
        public int TargetStatusStackMaxStacks;
        public float TargetStatusStackBaseDamage;
        public float TargetStatusStackAttackPowerCoefficient;
        public float TargetStatusStackSpellPowerCoefficient;
        public string ConsumeTargetStatusId;
        [Range(0f, 1f)] public float ConsumeTargetStatusRatio;
        public int ConsumeTargetStatusStacks;
        // 스킬이 적용할 상태와 능력치 변경값
        public string StatusEffectId;
        [Range(0f, 1f)] public float StatusChance;
        public string StatusEffectLabel;
        public GameObject StatusEffectPrefab;
        public float StatusDurationSeconds;
        public int StatusMaxStacks;
        public int StatusStackAmount;
        public string StatusTargetScope;
        public string StatusMergePolicy;
        public string ShieldAmountRefreshPolicy;
        public float StatusActionSpeedBonus;
        public float StatusMoveSpeedBonus;
        public float StatusAttackPowerBonus;
        public float StatusSpellPowerBonus;
        public float StatusDamageBonusRate;
        public bool StatusPermanent;
        public float StatusDamageTakenBonus;
        public float StatusCriticalDamageTakenBonus;
        public float StatusCriticalDamageBonus;
        public float StatusAilmentResistanceBonus;
        public float StatusCriticalResistanceBonus;
        public float StatusElementResistReduction;
        public float StatusFlatElementResistReduction;
        public float StatusElementDamageTakenBonus;
        // 성장 선택지와 추가 효과 그래프
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] MasterSkillChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }

    /*
     * 패시브 스킬의 요구 슬롯, 성장 선택지, 효과 그래프를 보관한다.
     */
    [Serializable]
    public class PassiveDefinition
    {
        public string PassiveId;
        public string DisplayName;
        public SkillSlot Slot;
        public SkillSlot RequiredActiveSlot;
        public bool IsAvailableWithoutActiveRequirement;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public Sprite SkillIcon;
        public GameObject SkillEffectPrefab;
        [TextArea(2, 5)] public string DescriptionText;
        [TextArea(2, 4)] public string Summary;
        public SkillChoiceDefinition[] BaseModifierChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillChoiceDefinition[] EnhancementChoices = Array.Empty<SkillChoiceDefinition>();
        public SkillEffectDefinition[] PassiveEffects = Array.Empty<SkillEffectDefinition>();
        public SkillNodeDefinition[] NormalizedPlanNodes = Array.Empty<SkillNodeDefinition>();
    }
}
