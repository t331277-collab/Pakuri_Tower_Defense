/*
 * 역할: 투사체 공격의 발사 계획을 실행한다.
 * 책임: 확정된 방향과 순번에 맞춰 투사체를 만들고 후속 발사를 예약한다.
 */

using System;
using System.Collections;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 시전 계획을 투사체 오브젝트로 바꾸고 이동과 적중 판정을 넘긴다.
    internal sealed class ProjectileSkillExecutor : MonoBehaviour
    {
        private EffectManager effects;

        /// 후속 발사까지 유지할 임시 실행 오브젝트를 만든다.
        internal static bool Execute(
            SkillActionContext context,
            SkillExecutionData snapshot)
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

            return instance.AddComponent<ProjectileSkillExecutor>().Initialize(context, snapshot);
        }

        /// 이번 발사 묶음을 배치하고 남은 후속 발사를 결정한다.
        private bool Initialize(
            SkillActionContext context,
            SkillExecutionData snapshot)
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
                    snapshot.PreparedDamage,
                    snapshot.PreparedImpactDamage,
                    snapshot.PreparedBoundaries[i],
                    snapshot.PreparedMagazineLastProjectile,
                    true,
                    i);
            }

            if (snapshot.HasFollowUpProjectile
                && snapshot.PreparedRuntimeVisual != null
                && snapshot.PreparedRuntimeVisual.HasVisual()
                && snapshot.PreparedBurstProjectileIndex >= snapshot.PreparedBurstProjectileCount)
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
            SkillActionContext context,
            SkillExecutionData snapshot)
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
                        snapshot.PreparedDamage
                            * Mathf.Max(0f, snapshot.FollowUpProjectileDamageMultiplier),
                        snapshot.PreparedBoundaries.Count > 0
                            ? snapshot.PreparedBoundaries[0]
                            : SkillExecutionRuleResolver.ProjectileDestroyBoundaryX(
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
            SkillActionContext context,
            SkillExecutionData snapshot,
            SkillExecutionData launchSnapshot,
            Vector2 direction,
            float damage,
            float impactDamage,
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
            var skillId = !string.IsNullOrWhiteSpace(snapshot.PreparedSkillId)
                ? snapshot.PreparedSkillId
                : snapshot.SkillId;
            var objectName = string.IsNullOrWhiteSpace(skillId)
                ? "Projectile"
                : "Projectile_" + skillId;
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

            var actor = instance.GetComponent<ProjectileSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<ProjectileSkillActor>();
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
                ValueAt(snapshot.PreparedBranchChances, planIndex, 0f),
                ValueAt(snapshot.PreparedBranchCounts, planIndex, 0),
                ValueAt(snapshot.PreparedBranchDamageMultipliers, planIndex, 1f),
                ValueAt(snapshot.PreparedBranchSearchRadii, planIndex, 0f),
                snapshot.PreparedImpactStatus,
                snapshot.PreparedContactDamageEnabled,
                snapshot.PreparedStopOnFirstHit,
                snapshot.PreparedImpactDelay,
                snapshot.PreparedImpactRuntimeVisual,
                snapshot.PreparedHasImpactArea,
                snapshot.PreparedImpactRadius,
                impactDamage,
                context.Runtime,
                launchSnapshot,
                null,
                skillId,
                isMagazineLastProjectile,
                snapshot.PreparedCriticalAllowed,
                snapshot.CritChanceBonus,
                snapshot.CritDamageBonus);
        }

        /// 발사 계획에 값이 없을 때 안전한 기본값을 사용한다.
        private static T ValueAt<T>(
            System.Collections.Generic.IReadOnlyList<T> values,
            int index,
            T fallback)
        {
            return values != null && index >= 0 && index < values.Count
                ? values[index]
                : fallback;
        }
    }
}
