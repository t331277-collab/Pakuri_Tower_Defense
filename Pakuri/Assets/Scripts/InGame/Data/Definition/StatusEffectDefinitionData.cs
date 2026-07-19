using System;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * 상태 효과가 이로운 효과인지 해로운 효과인지 구분한다.
     */
    public enum StatusEffectClassification
    {
        Buff,
        Debuff
    }

    [Serializable]
    /*
     * 상태 효과의 지속시간, 중첩, 행동 제한, 능력치 변경값을 보관한다.
     */
    public sealed class StatusEffectDefinitionData
    {
        // 상태 식별과 기본 적용 규칙
        public string StatusEffectId;
        public string StatusEffectLabel;
        public StatusEffectClassification Classification;
        public bool HasAttribute;
        public DamageAttribute Attribute;
        public float DefaultDurationSeconds;
        public bool IsPermanent;
        public int MaxStacks;
        public int BaseStackAmount = 1;
        // 상태가 제한하는 유닛 행동
        public bool CanMove = true;
        public bool CanAct = true;
        public bool CanUseSpecialSkill = true;
        // 중첩 하나당 적용할 능력치 변화
        public float ActionSpeedBonusPerStack;
        public float MoveSpeedBonusPerStack;
        public float AttackPowerBonusPerStack;
        public float DamageTakenBonusPerStack;
        public float CriticalDamageTakenBonusPerStack;
        public float CriticalResistanceBonusPerStack;
        public float ElementResistReductionPerStack;
        public float ElementDamageTakenBonusPerStack;
        // 상태와 함께 표시할 선택적 프리팹
        public GameObject StatusEffectPrefab;
    }
}
