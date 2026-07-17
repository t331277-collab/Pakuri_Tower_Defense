using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public enum CharacterType
    {
        Unknown = -1,
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

    public enum SkillTargetSide
    {
        Enemy,
        Self,
        Ally,
        AllAllies
    }

    public enum SkillTargetSelection
    {
        Nearest,
        LowestHealth,
        HighestHealth,
        HighestStacks,
        ManualPosition,
        Owner,
        Farthest,
        Random
    }

    public enum SkillTargetShape
    {
        Single,
        Line,
        Circle,
        Battlefield
    }

    public enum ProjectileTravelMode
    {
        Straight,
        Homing,
        Arc,
        Instant
    }

    public enum ZoneAnchorMode
    {
        GroundPosition,
        Target,
        Owner,
        Battlefield
    }

    public enum ZoneTickMode
    {
        Once,
        OnInterval,
        OnEnter,
        OnExit
    }

    public enum BuffTarget
    {
        AllAllies,
        Self
    }

    public enum StatusTargetScope
    {
        Unspecified,
        AllAllies,
        Self
    }

    public enum StatusMergePolicy
    {
        Unspecified,
        SameSourceTakeHighest,
        SameSourceRefresh,
        AlwaysStack
    }

    public enum ShieldRefreshRule
    {
        Replace,
        TakeHighest,
        Stack
    }

    public enum PassiveTrigger
    {
        Always,
        DuringBuff,
        AfterSkill,
        OnTargetStatus,
        OnEvent,
        OnHitCount
    }

    public enum PassiveTarget
    {
        AllAllies,
        ElementUsers,
        Self
    }

    [Serializable]
    public sealed class SkillTimingSpec
    {
        [Min(0f)] public float Cooldown;
        [Min(0f)] public float CastTime;
        [Min(0f)] public float ActiveDuration;
        [Min(0f)] public float TickInterval;
    }

    [Serializable]
    public sealed class SkillTargetingSpec
    {
        public SkillTargetSide TargetSide = SkillTargetSide.Enemy;
        public SkillTargetSelection Selection = SkillTargetSelection.Nearest;
        public string SelectionStatusId;
        [Min(0)] public int SelectionStatusMinStacks;
        public SkillTargetShape Shape = SkillTargetShape.Single;
        [Tooltip("Deprecated. InGame skills target the whole battlefield; runtime ignores this value.")]
        [Min(0f)] public float Range;
        [Min(0f)] public float Radius;
        public bool CoverAll;
    }

    [Serializable]
    public sealed class SkillDamageSpec
    {
        public string SkillId;
        public ElementType Element;
        [Min(0f)] public float BaseDamage;
        public float StatCoefficient;
        public StatSource StatSource;
        public bool UseCombinedStatCoefficients;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool CriticalAllowed = true;
    }

    [Serializable]
    public sealed class StatusApplicationSpec
    {
        public StatusEffectData Status;
        [Range(0f, 1f)] public float Chance = 1f;
        [Min(0)] public int Stacks = 1;
        public bool RefreshDuration = true;
    }

    [Serializable]
    public sealed class ProjectileBlueprintSpec
    {
        [Min(0)] public int MagazineSize;
        [Min(0f)] public float ReloadTime;
        [Min(1)] public int BurstProjectileCount = 1;
        [Min(0f)] public float BurstIntervalSeconds;
        [Min(0)] public int BurstDamageProjectileIndex;
        [Min(0f)] public float BurstDamageMultiplier = 1f;
        [Min(1)] public int ProjectilesPerShot = 1;
        [Min(0)] public int PierceCount;
        [Min(0f)] public float ProjectileSpeed;
        [Min(0f)] public float LifetimeSeconds;
        public ProjectileTravelMode TravelMode = ProjectileTravelMode.Straight;
        public GameObject ProjectilePrefab;
    }

    [Serializable]
    public sealed class AreaBlueprintSpec
    {
        public ZoneAnchorMode AnchorMode = ZoneAnchorMode.GroundPosition;
        public ZoneTickMode TickMode = ZoneTickMode.OnInterval;
        [Min(0f)] public float DeployDelay;
        [Min(0f)] public float Radius;
        [Min(0f)] public float Duration;
        [Min(0f)] public float TickInterval;
        public bool CoverAll;
    }

    [Serializable]
    public sealed class AllyEffectSpec
    {
        public bool Enabled;
        [Min(0f)] public float ShieldBase;
        public float ShieldCoefficient;
        public StatSource ShieldStatSource;
        public string BuffTag;
        [Min(0f)] public float BuffDuration;
    }

    [Serializable]
    public sealed class BuffModifierSpec
    {
        public float ActionSpeedBonus;
        public float AttackPowerBonus;
        public float SpellPowerBonus;
        public float DamageBonusRate;
        public float ShieldReceivedBonus;
        public float CritChanceBonusRate;
        public float CritDamageBonusRate;
        public float ResistReduction;
        public ElementType ResistReductionElement;
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
        public int MagazineCapacity;
        public float ReloadSeconds;

        [Header("Presentation")]
        public GameObject SkillEffectPrefab;
        public RuntimeSkillVisualSpec RuntimeVisual = new RuntimeSkillVisualSpec();

        [Header("Choices")]
        public SkillChoiceEffectSpec[] EnhancementChoices = Array.Empty<SkillChoiceEffectSpec>();
        public SkillChoiceEffectSpec[] MasterChoices = Array.Empty<SkillChoiceEffectSpec>();
        public SkillEffectDefinition[] MultiEffects = Array.Empty<SkillEffectDefinition>();
        public SkillTriggerDefinition[] SkillTriggers = Array.Empty<SkillTriggerDefinition>();
        public SkillExecutionPlanNode[] NormalizedPlanNodes = Array.Empty<SkillExecutionPlanNode>();
    }

    [CreateAssetMenu(menuName = "Pakuri/InGame/Buff Skill Data", fileName = "BuffSkillData")]
    public sealed class BuffSkillData : SkillData
    {
        [Header("Buff")]
        public float BuffDuration;
        public BuffTarget Target;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public string ApplyStatusTag;

        [Header("Modifiers")]
        public BuffModifierSpec Modifiers = new BuffModifierSpec();

        [Header("Attached Damage")]
        public bool HasAttachedDamage;
        public SkillDamageSpec AttachedDamage = new SkillDamageSpec();
        public float AttachedDamageRadius;
        public StatusApplicationSpec AttachedStatus = new StatusApplicationSpec();
    }

    [CreateAssetMenu(menuName = "Pakuri/InGame/Heal Skill Data", fileName = "HealSkillData")]
    public sealed class HealSkillData : SkillData
    {
        public SkillDamageSpec Healing = new SkillDamageSpec();
        public bool AttachVisualToTarget = true;
    }

    [CreateAssetMenu(menuName = "Pakuri/InGame/Chain Attack Skill Data", fileName = "ChainAttackSkillData")]
    public sealed class ChainAttackSkillData : SkillData
    {
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public float ChainDamageMultiplier = 0.5f;
        public float ChainDelaySeconds = 0.5f;
        public float ChainRadius;
        public bool ExcludePrimaryTarget = true;
    }

    [CreateAssetMenu(menuName = "Pakuri/InGame/Charge Skill Data", fileName = "ChargeSkillData")]
    public sealed class ChargeSkillData : SkillData
    {
        public float TargetMaxHealthRatio = 1f;
        public float RampSeconds = 3f;
        public float MaxMoveSpeedMultiplier = 2.5f;
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }
}
