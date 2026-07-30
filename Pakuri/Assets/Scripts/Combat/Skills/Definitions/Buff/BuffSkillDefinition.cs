/*
 * 역할: 버프 계열 스킬 Definition.
 * 책임: 상태·회복·보호막·돌진의 공통 대상 설정과 종류별 값만 정의한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    public enum BuffEffectKind
    {
        Status,
        Heal,
        Shield,
        Charge
    }

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
        public StatSource ShieldStatSource;
        public float ShieldDuration;
        public StatusRuntimeData ShieldStatus;

        [Header("Charge")]
        public float ChargeTargetMaxHealthRatio = 1f;
        public float ChargeRampSeconds = 3f;
        public float ChargeMaxMoveSpeedMultiplier = 2.5f;
    }
}
