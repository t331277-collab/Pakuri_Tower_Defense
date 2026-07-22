using System;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

/*
 * 돌진 중인 유닛의 이동과 적 접촉을 매 프레임 처리한다.
 * 실행기가 기록한 돌진 상태를 사용해 목표를 추적하고, 접촉 시 피해와 상태 효과를 적용한다.
 */
namespace Pakuri.InGame
{
    public static class SingleChargeActor
    {
        /*
         * 돌진 이동과 대상 접촉을 갱신하고 돌진 처리 여부를 반환한다.
         */
        public static bool Tick(
            CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            float deltaTime /* 이전 갱신 이후 지난 시간 */)
        {
            var caster = casterEntry != null ? casterEntry.Model : null;
            var charge = caster != null ? caster.ActiveCharge : null;
            if (charge == null)
            {
                return false;
            }

            if (casterEntry.Transform == null || roster == null || combatManager == null)
            {
                caster.ActiveCharge = null;
                return true;
            }

            if (TryResolveHit(casterEntry, roster, combatManager, caster, charge))
            {
                return true;
            }

            var target = FindTargetByUnitId(casterEntry, roster, charge.TargetUnitId);
            if (target == null)
            {
                target = SkillTargeting.FindNearestTarget(
                    casterEntry,
                    roster,
                    new SkillTargetingSpec
                    {
                        TargetSide = SkillTargetSide.Enemy,
                        Selection = SkillTargetSelection.Random
                    });
            }

            if (target == null || target.Transform == null)
            {
                caster.ActiveCharge = null;
                return true;
            }

            charge.ElapsedSeconds += Mathf.Max(0f, deltaTime);
            var ramp = charge.RampSeconds > 0f ? Mathf.Clamp01(charge.ElapsedSeconds / charge.RampSeconds) : 1f;
            var speedMultiplier = Mathf.Lerp(1f, Mathf.Max(1f, charge.MaxMoveSpeedMultiplier), ramp);
            var baseSpeed = caster.Stats != null ? Mathf.Max(0f, caster.Stats.MoveSpeed) : 0f;
            var speed = baseSpeed * speedMultiplier * StatusCombatRules.ResolveMoveSpeedMultiplier(caster);
            if (speed > 0f && StatusCombatRules.CanMove(caster))
            {
                casterEntry.Transform.position = Vector3.MoveTowards(
                    casterEntry.Transform.position,
                    target.Transform.position,
                    speed * Mathf.Max(0f, deltaTime));
            }

            TryResolveHit(casterEntry, roster, combatManager, caster, charge);
            return true;
        }

        /*
         * 현재 접촉한 적이 있으면 돌진 적중을 처리한다.
         */
        private static bool TryResolveHit(
            CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            UnitCombatState caster /* 스킬을 사용하는 유닛 */,
            SingleChargeState charge /* 돌진 */)
        {
            var hitTarget = FindHitTarget(casterEntry, roster);
            if (hitTarget == null)
            {
                return false;
            }

            ResolveHit(caster, hitTarget, combatManager, charge);
            return true;
        }

        /*
         * 돌진 시작 때 저장한 유닛 ID와 같은 적을 찾는다.
         */
        private static CombatUnitEntry FindTargetByUnitId(
            CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            string unitId /* 유닛 식별자 */)
        {
            var targets = SkillTargeting.ResolveTargetList(
                casterEntry,
                roster,
                new SkillTargetingSpec { TargetSide = SkillTargetSide.Enemy });
            for (var i = 0; i < targets.Count; i++)
            {
                var identity = targets[i] != null && targets[i].Model != null ? targets[i].Model.Identity : null;
                if (identity != null && string.Equals(identity.UnitId, unitId, StringComparison.OrdinalIgnoreCase))
                {
                    return targets[i];
                }
            }

            return null;
        }

        /*
         * 돌진 유닛과 접촉한 첫 적을 찾는다.
         */
        private static CombatUnitEntry FindHitTarget(CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */, CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */)
        {
            var targets = SkillTargeting.ResolveTargetList(
                casterEntry,
                roster,
                new SkillTargetingSpec { TargetSide = SkillTargetSide.Enemy });
            var casterColliders = casterEntry.GetHitboxColliders();
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target != null && target.IsAlive && HasChargeContact(casterEntry, casterColliders, target))
                {
                    return target;
                }
            }

            return null;
        }

        /*
         * 콜라이더 겹침 또는 매우 가까운 거리로 돌진 접촉을 판정한다.
         */
        private static bool HasChargeContact(
            CombatUnitEntry casterEntry /* 스킬 사용자의 전투 등록 정보 */,
            Collider2D[] casterColliders /* 스킬 사용자 콜라이더 목록 */,
            CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */)
        {
            if (UnitHitboxOverlap.IsTargetInsideHitbox(casterColliders, target))
            {
                return true;
            }

            if (casterEntry == null
                || casterEntry.Transform == null
                || target == null
                || target.Transform == null)
            {
                return false;
            }

            return ((Vector2)casterEntry.Transform.position - (Vector2)target.Transform.position).sqrMagnitude <= 0.0025f;
        }

        /*
         * 최대 체력 비례 피해와 적중 상태를 적용하고 돌진을 끝낸다.
         */
        private static void ResolveHit(
            UnitCombatState caster /* 스킬을 사용하는 유닛 */,
            CombatUnitEntry target /* 효과를 받을 대상의 등록 정보 */,
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            SingleChargeState charge /* 돌진 */)
        {
            var maxHealth = target.Model != null && target.Model.Stats != null
                ? Mathf.Max(0f, target.Model.Stats.MaxHealth)
                : 0f;
            var damageResult = combatManager.ApplyDamage(
                target.Model,
                maxHealth * Mathf.Max(0f, charge.DamageTargetMaxHealthRatio),
                charge.Attribute,
                caster,
                true,
                sourceSkillId: charge.SkillId);

            var statusSpec = SkillStatus.ResolveStatusSpec(charge.OnHitStatus, null);
            if (!damageResult.IsDead && statusSpec != null)
            {
                StatusCombatRules.ApplyStatus(combatManager, target.Model, statusSpec, caster);
            }

            caster.ActiveCharge = null;
        }
    }
}
