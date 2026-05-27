using UnityEngine;

namespace Pakuri.InGame
{
    [CreateAssetMenu(menuName = "Pakuri/InGame/Single Attack Data", fileName = "SingleAttackData")]
    public sealed class SingleAttackData : SkillData
    {
        [Header("Area")]
        public AreaBlueprintSpec Area = new AreaBlueprintSpec();
        public bool UsesHitTargetCount;
        public bool UsePrefabHitbox;
        public bool UseMultiDeployment;
        public bool HitAllTargets;
        [Min(1)] public int HitTargetCount = 1;
        [Min(1)] public int DeploymentCount = 1;
        [Min(0f)] public float DamageDelaySeconds;
        [Range(0f, 1f)] public float ExecuteHealthRatioThreshold;
        public bool RequireExecuteThresholdToCast;
        public float ExecuteDamageMultiplier = 1f;
        [Range(0f, 1f)] public float KillCooldownRefundRatio;
        public float BossDamageMultiplier = 1f;

        [Header("Enemy Effect")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();

        [Header("Ally Effect")]
        public AllyEffectSpec AllyEffect = new AllyEffectSpec();
    }
}
