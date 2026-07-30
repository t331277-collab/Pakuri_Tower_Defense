/*
 * 역할: 투사체 스킬 전달 조정.
 * 책임: 확정된 발사 계획대로 Projectile Actor를 생성하고 후속 발사를 예약한다.
 */

using System;
using System.Collections;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 확정된 Projectile 발사 계획을 실행하고 충돌 판정은 ProjectileSkillActor에 맡긴다.
    internal sealed class ProjectileSkillExecutor : MonoBehaviour
    {
        private EffectManager effects;

        internal static bool Execute(
            SkillExecutionContext context,
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

        private bool Initialize(
            SkillExecutionContext context,
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

        private IEnumerator ExecuteFollowUpProjectilesAfterDelay(
            SkillExecutionContext context,
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

        private static void SpawnProjectileActor(
            SkillExecutionContext context,
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

            context.Runtime?.AdvanceProjectileLaunchCount();
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
