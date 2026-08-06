/*
 * 역할: 한 번의 판정으로 적용되는 공격의 설계값을 정의한다.
 * 책임: 대상 수와 배치, 처형 조건, 상태 소비, 피해와 상태 효과를 제공한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 단발성 공격의 배치와 대상별 결과를 설계한다.
    public class SingleSkillDefinition : SkillDefinition
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool UsePrefabHitbox;
        public bool UseMultiDeployment;
        public bool HitAllTargets;
        public int HitTargetCount = 1;
        public int DeploymentCount = 1;
        public string DeploymentRequiredTargetStatusName;
        public StatusEffectKind DeploymentRequiredTargetStatusKind;
        public int DeploymentRequiredTargetStatusMinStacks;
        public string TargetStatusStackStatusName;
        public StatusEffectKind TargetStatusStackStatusKind;
        public int TargetStatusStackMaxStacks;
        public string ConsumeTargetStatusName;
        public StatusEffectKind ConsumeTargetStatusKind;
        public float ConsumeTargetStatusRatio;
        public int ConsumeTargetStatusStacks;
        public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;

        [Header("Enemy Effect")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public SkillDamageSpec TargetStatusStackDamage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();
    }
}
