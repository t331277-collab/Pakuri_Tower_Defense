/*
 * 역할: 지연 단일 차지 실행.
 * 책임: 차지 시간과 대상 유효성을 추적한 뒤 단일 대상 스킬을 발동하거나 취소한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>SingleChargeActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
    public static class SingleChargeActor
    {

        private static readonly List<CombatUnitEntry> collisionTargets = new List<CombatUnitEntry>(1);

        /// <summary>전달된 런타임 입력값을 사용해 <c>요청값</c>를 경과 시간 기준으로 갱신한다.</summary>
        public static bool Tick(
            CombatUnitEntry casterEntry,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            float deltaTime)
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

            if (TryResolveHit(casterEntry, roster, combatManager, caster, charge, Vector2.zero))
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
            var speed = baseSpeed * speedMultiplier * StatusCombatRules.MoveSpeedMultiplier(caster);
            if (speed > 0f && StatusCombatRules.CanMove(caster))
            {
                var currentPosition = casterEntry.Transform.position;
                var nextPosition = Vector3.MoveTowards(
                    currentPosition,
                    target.Transform.position,
                    speed * Mathf.Max(0f, deltaTime));
                var movement = (Vector2)(nextPosition - currentPosition);
                var hitTarget = FindHitTarget(casterEntry, roster, movement);
                casterEntry.Transform.position = nextPosition;
                if (hitTarget != null)
                {
                    Hit(caster, hitTarget, combatManager, charge);
                }
            }

            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResolveHit</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private static bool TryResolveHit(
            CombatUnitEntry casterEntry,
            UnitSpawnManager roster,
            InGameCombatManager combatManager,
            UnitCombatState caster,
            SingleChargeState charge,
            Vector2 movement)
        {
            var hitTarget = FindHitTarget(casterEntry, roster, movement);
            if (hitTarget == null)
            {
                return false;
            }

            Hit(caster, hitTarget, combatManager, charge);
            return true;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>TargetByUnitId</c>를 찾는다.</summary>
        private static CombatUnitEntry FindTargetByUnitId(
            CombatUnitEntry casterEntry,
            UnitSpawnManager roster,
            string unitId)
        {
            var targets = SkillTargeting.TargetList(
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>HitTarget</c>를 찾는다.</summary>
        private static CombatUnitEntry FindHitTarget(
            CombatUnitEntry casterEntry,
            UnitSpawnManager roster,
            Vector2 movement)
        {
            var targets = SkillTargeting.TargetList(
                casterEntry,
                roster,
                new SkillTargetingSpec { TargetSide = SkillTargetSide.Enemy });
            UnitCollisionResolver.CollectTargets(
                roster,
                targets,
                casterEntry,
                movement,
                collisionTargets);
            return collisionTargets.Count > 0 ? collisionTargets[0] : null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Hit</c> 작업을 수행한다.</summary>
        private static void Hit(
            UnitCombatState caster,
            CombatUnitEntry target,
            InGameCombatManager combatManager,
            SingleChargeState charge)
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

            var statusSpec = SkillStatus.StatusSpec(charge.OnHitStatus, null);
            if (!damageResult.IsDead && statusSpec != null)
            {
                StatusCombatRules.ApplyStatus(combatManager, target.Model, statusSpec, caster);
            }

            caster.ActiveCharge = null;
        }
    }
}
