/*
 * 역할: 투사체 공격의 발사 계획을 실행한다.
 * 확정된 방향과 횟수(마지막 탄창 유무)에 맞춰 투사체를 만들고 후속 발사를 예약한다.
 */

using System;
using System.Collections;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 시전 계획을 투사체 오브젝트로 바꾸고 이동과 적중 판정을 넘긴다.
    internal sealed class ProjectileSkillManager : MonoBehaviour
    {
        private EffectManager effects;

        /// 후속 발사까지 유지할 임시 실행 오브젝트를 만든다.
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionState snapshot)
        {
            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return false;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                null,
                null,
                "RuntimeProjectileExecution",
                context.CasterEntry.Transform != null
                    ? context.CasterEntry.Transform.position
                    : Vector3.zero,
                Quaternion.identity,
                null,
                null,
                false,
                false,
                true));
            if (instance == null)
            {
                return false;
            }

            var executor = instance.GetComponent<ProjectileSkillManager>();
            if (executor == null)
            {
                executor = instance.AddComponent<ProjectileSkillManager>();
            }

            return executor.Initialize(context, snapshot);
        }

        private bool Initialize(
            SkillExecutionContext context,
            SkillExecutionState snapshot)
        {
            effects = context.CombatManager.Effects;
            var launchSnapshot = snapshot.CopyWithDamageMultiplier(
                snapshot.PreparedBurstDamageMultiplier);
            for (var i = 0; i < snapshot.PreparedDirections.Count; i++)
            {
                SpawnProjectileActor(
                    context,
                    snapshot,
                    launchSnapshot,
                    snapshot.PreparedDirections[i],
                    snapshot.PreparedDamage * (snapshot.PreparedMagazineLastProjectile
                        ? Mathf.Max(0f, snapshot.MagazineLastProjectileDamageMultiplier)
                        : 1f),
                    snapshot.PreparedBoundaries[i],
                    snapshot.PreparedMagazineLastProjectile,
                    true,
                    i);
            }

            if (snapshot.HasFollowUpProjectile
                && snapshot.PreparedRuntimeVisual != null
                && snapshot.PreparedRuntimeVisual.HasVisual()
                && snapshot.PreparedBurstProjectileIndex >= snapshot.PreparedBurstProjectileCount
                && (!snapshot.FollowUpProjectileFirstMagazineOnly
                    || snapshot.PreparedMagazineFirstProjectile))
            {
                StartCoroutine(ExecuteFollowUpProjectilesAfterDelay(context, snapshot));
            }
            else
            {
                effects.RemoveEffect(gameObject);
            }
            return true;
        }

        /// 정해진 지연 뒤 같은 조준 방향으로 추가 발사를 잇는다.
        private IEnumerator ExecuteFollowUpProjectilesAfterDelay(
            SkillExecutionContext context,
            SkillExecutionState snapshot)
        {
            if (snapshot.FollowUpProjectileDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(snapshot.FollowUpProjectileDelaySeconds);
            }
            else
            {
                yield return null;
            }

            if (context != null
                && context.CombatManager != null
                && context.CombatManager.Effects != null
                && snapshot.PreparedRuntimeVisual != null
                && snapshot.PreparedRuntimeVisual.HasVisual())
            {
                var count = Math.Max(1, snapshot.FollowUpProjectileCount);
                var planOffset = snapshot.PreparedDirections.Count;
                for (var i = 0; i < count; i++)
                {
                    SpawnProjectileActor(
                        context,
                        snapshot,
                        snapshot,
                        snapshot.PreparedDirection,
                        snapshot.PreparedDamage
                            * Mathf.Max(0f, snapshot.FollowUpProjectileDamageMultiplier),
                        snapshot.PreparedBoundaries.Count > 0
                            ? snapshot.PreparedBoundaries[0]
                            : SkillExecutionRules.ProjectileDestroyBoundaryX(
                                snapshot.PreparedOrigin,
                                snapshot.PreparedDirection,
                                snapshot.PreparedProjectileSpeed,
                                snapshot.PreparedProjectileLifetime),
                        false,
                        false,
                        planOffset + i);
                }
            }
            effects.RemoveEffect(gameObject);
        }

        /// 한 발의 표현과 이동, 충돌 입력을 완성한다.
        private static void SpawnProjectileActor(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            SkillExecutionState launchSnapshot,
            Vector2 direction,
            float damage,
            float boundary,
            bool isMagazineLastProjectile,
            bool createFallbackHitbox,
            int planIndex)
        {
            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return;
            }

            SkillExecution.AdvanceProjectileLaunchCount(context.Runtime);
            var skillName = !string.IsNullOrWhiteSpace(snapshot.PreparedSkillName)
                ? snapshot.PreparedSkillName
                : snapshot.SkillName;
            var objectName = string.IsNullOrWhiteSpace(skillName)
                ? "Projectile"
                : "Projectile_" + skillName;
            var instance = effects.CreateEffect(new EffectCreateRequest(
                snapshot.PreparedRuntimeVisual,
                null,
                objectName,
                snapshot.PreparedOrigin,
                EffectVisualBuilder.Rotation(direction),
                null,
                null,
                true,
                true,
                createFallbackHitbox));
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponent<ProjectileSkillObject>();
            if (actor == null)
            {
                actor = instance.AddComponent<ProjectileSkillObject>();
            }
            actor.Initialize(
                context.CombatManager,
                context.Caster,
                direction,
                snapshot.PreparedProjectileSpeed,
                damage,
                snapshot.PreparedDamageAttribute,
                snapshot.PreparedPierceCount,
                boundary,
                snapshot.PreparedProjectileLifetime,
                snapshot.PreparedStatus,
                snapshot.PreparedBranchChances[planIndex],
                snapshot.PreparedBranchCounts[planIndex],
                snapshot.PreparedBranchDamageMultipliers[planIndex],
                snapshot.PreparedBranchSearchRadii[planIndex],
                snapshot.PreparedContactDamageEnabled,
                snapshot.PreparedArrivalDelay,
                snapshot.PreparedArrivalSkill,
                snapshot.PreparedHasProjectileTargetPoint,
                snapshot.PreparedProjectileTargetPoint,
                context.Runtime,
                launchSnapshot,
                null,
                skillName,
                isMagazineLastProjectile,
                snapshot.PreparedCriticalAllowed,
                snapshot.CritChanceBonus,
                snapshot.CritDamageBonus);
        }

    }
}
