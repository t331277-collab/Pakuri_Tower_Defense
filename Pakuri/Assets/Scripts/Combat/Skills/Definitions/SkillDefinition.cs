/*
 * 역할: 모든 스킬이 공유하는 설계 기준을 정의한다.
 * 책임: 식별 정보와 실행 종류, 타이밍, 대상, 표현, 학습 선택의 공통값을 제공한다.
 */

using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.Data
{

    /// 스킬 데이터와 실행 구현의 준비 상태를 구분한다.
    public enum SkillImplementationState
    {
        NotImplemented,
        DataOnly,
        RuntimeImplemented
    }

}

namespace Pakuri.InGame
{
    /// 계수 계산에 사용할 시전자 능력치를 구분한다.
    public enum StatSource
    {
        Attack,
        Intelligence
    }




    /// 시전과 지속 효과가 진행될 시간 기준을 설계한다.
    [Serializable]
    public class SkillTimingSpec
    {
        public float Cooldown;
        public float ActiveDuration;
        public float TickInterval;
    }

    /// 대상 진영과 우선순위, 적용 범위를 설계한다.
    [Serializable]
    public class SkillTargetingSpec
    {
        public SkillTargetSide TargetSide = SkillTargetSide.Enemy; // 효과를 적용할 진영
        public SkillTargetSelection Selection = SkillTargetSelection.Nearest;
        public string SelectionStatusName;
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

    /// 기본 피해와 시전자 능력치 계수를 설계한다.
    [Serializable]
    public class SkillDamageSpec
    {
        public string SkillName;
        public DamageAttribute Element;
        public float BaseDamage;
        public float AttackPowerCoefficient;
        public float SpellPowerCoefficient;
        public bool CriticalAllowed = true;
    }

    /// 상태 효과가 적용될 확률과 중첩, 지속 규칙을 설계한다.
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

    /// 공간 효과의 크기와 지속 주기를 설계한다.
    [Serializable]
    public class AreaBlueprintSpec
    {
        public float Radius;
        public float Duration;
        public float TickInterval;
        public bool CoverAll;
    }

    /// 모든 스킬이 공유하는 식별과 실행 기준을 설계한다.
    public class SkillDefinition
    {
        [Header("Identity")]
        public string SkillName;
        public string DisplayName;
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
