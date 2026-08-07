/*
 * 역할: 지원형 스킬의 설계값을 정의한다.
 * 책임: 상태, 회복, 보호막, 돌진이 사용할 대상과 고유 수치를 제공한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 지원 효과가 전투 자원을 바꾸는 방식을 구분한다.
    public enum BuffEffectKind
    {
        Status,
        Heal,
        Shield,
        Charge
    }

    /// 지원 효과의 대상과 종류별 수치를 설계한다.
    public class BuffSkillDefinition : SkillDefinition
    {
        [Header("Buff")]
        public BuffEffectKind EffectKind;
        public SkillTargetSide Target = SkillTargetSide.AllAllies;
        public bool UseConfiguredTargeting;
        public bool AttachVisualToCaster;
        public StatusApplicationSpec AttachedStatus = new StatusApplicationSpec();

        [Header("Heal")]
        public SkillDamageSpec Healing = new SkillDamageSpec();

        [Header("Shield")]
        public float ShieldBase;
        public float ShieldCoefficient;
        public float ShieldTargetMaxHealthRatio;
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public StatusRuntimeData ShieldStatus;

        [Header("Charge")]
        public float ChargeTargetMaxHealthRatio = 1f;
        public float ChargeRampSeconds = 3f;
        public float ChargeMaxMoveSpeedMultiplier = 2.5f;
    }
}
