/*
 * 역할: 모든 스킬 Definition의 공용 계약.
 * 책임: 스킬 식별·타이밍·대상·피해·상태 적용 공통값을 정의한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{
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

    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

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
}

namespace Pakuri.InGame
{
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
        Random,
        NearestOtherFromEventTarget
    }

    public enum SkillTargetShape
    {
        Single,
        Line,
        Circle,
        Battlefield
    }

    [Serializable]
    public class SkillTimingSpec
    {
        public float Cooldown;
        public float ActiveDuration;
        public float TickInterval;
    }

    [Serializable]
    public class SkillTargetingSpec
    {
        public SkillTargetSide TargetSide = SkillTargetSide.Enemy;
        public SkillTargetSelection Selection = SkillTargetSelection.Nearest;
        public string SelectionStatusId;
        public StatusEffectKind SelectionStatusKind;
        public int SelectionStatusMinStacks;
        public bool HasSelectionSkillAttribute;
        public DamageAttribute SelectionSkillAttribute;
        public SkillTargetShape Shape = SkillTargetShape.Single;
        [Tooltip("Deprecated. InGame skills target the whole battlefield; runtime ignores this value.")]
        public float Range;
        public float Radius;
        public bool CoverAll;
    }

    [Serializable]
    public class SkillDamageSpec
    {
        public string SkillId;
        public DamageAttribute Element;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool CriticalAllowed = true;
    }

    [Serializable]
    public class StatusApplicationSpec
    {
        public bool Enabled = true;
        public StatusRuntimeData Status;
        public float Chance = 1f;
        public int Stacks = 1;
        public bool RefreshDuration = true;
        public bool RuntimeResolved;
        public float RuntimeDurationSeconds;
        public int RuntimeMaxStacks;
        public bool RuntimePermanent;
        public StatusEffectKind ThresholdSourceStatusKind;
        public int ThresholdSourceMinStacks;
        public StatusApplicationSpec ThresholdStatus;
    }

    [Serializable]
    public class AreaBlueprintSpec
    {
        public float Radius;
        public float Duration;
        public float TickInterval;
        public bool CoverAll;
    }

    public class SkillDefinition
    {
        [Header("Identity")]
        public string SkillId;
        public string SkillName;
        public SkillSlot Slot;
        public SkillRuntimeKind RuntimeKind;
        public SkillImplementationState ImplementationState = SkillImplementationState.DataOnly;
        public bool IsDefaultLearned;
        public bool IsActive = true;
        public DamageAttribute Element;
        [TextArea(2, 5)] public string Description;
        [TextArea(2, 4)] public string Summary;
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
        public SkillChoice[] EnhancementChoices = Array.Empty<SkillChoice>();
        public SkillChoice[] MasterChoices = Array.Empty<SkillChoice>();
        public SkillNode[] Nodes = Array.Empty<SkillNode>();
    }
}
