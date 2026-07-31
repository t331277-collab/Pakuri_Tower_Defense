/*
 * 역할: 이동형 공격의 설계값을 정의한다.
 * 책임: 발사 수와 속도, 관통, 직격, 충돌 후 범위 효과를 제공한다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 한 번의 발사가 만들 투사체 묶음과 이동 방식을 설계한다.
    [Serializable]
    public class ProjectileBlueprintSpec
    {
        public int MagazineSize;
        public float ReloadTime;
        public int BurstProjectileCount = 1;
        public float BurstIntervalSeconds;
        public int BurstDamageProjectileIndex;
        public float BurstDamageMultiplier = 1f;
        public int ProjectilesPerShot = 1;
        public int PierceCount;
        public float ProjectileSpeed;
        public float LifetimeSeconds;
    }

    /// 투사체의 직격과 충돌 후 결과를 함께 설계한다.
    public class ProjectileSkillDefinition : SkillDefinition
    {
        [Header("Projectile")]
        public ProjectileBlueprintSpec Projectile = new ProjectileBlueprintSpec();

        [Header("Damage")]
        public SkillDamageSpec Damage = new SkillDamageSpec();
        public StatusApplicationSpec OnHitStatus = new StatusApplicationSpec();

        [Header("Consecutive Hit")]
        public float ConsecutiveHitBonusRate;
        public float ConsecutiveHitMax;

        [Header("Impact Area")]
        public bool ContactDamageEnabled = true;
        public bool StopOnFirstHit;
        public float ImpactDelaySeconds;
        public RuntimeSkillVisualSpec ImpactRuntimeVisual = new RuntimeSkillVisualSpec();
        public bool HasImpactArea;
        public AreaBlueprintSpec ImpactArea = new AreaBlueprintSpec();
        public SkillDamageSpec ImpactDamage = new SkillDamageSpec();
        public StatusApplicationSpec ImpactStatus = new StatusApplicationSpec();
    }
}
