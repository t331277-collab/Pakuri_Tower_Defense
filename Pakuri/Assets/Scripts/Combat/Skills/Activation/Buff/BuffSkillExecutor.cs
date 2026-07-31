/*
 * 역할: 버프 계열 스킬 전달.
 * 책임: 상태·회복·보호막·돌진 효과를 하나의 버프 실행 경로로 전달한다.
 */

using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 시전 시 확정된 버프 효과 종류에 맞는 런타임 동작을 실행한다.
    internal static class BuffSkillExecutor
    {

        /// 설정된 버프 효과를 실행한다.
        internal static bool Execute(
            SkillActionContext context,
            SkillExecutionData snapshot)
        {
            switch (snapshot.PreparedBuffEffectKind)
            {
                case BuffEffectKind.Heal:
                    return ExecuteHeal(context, snapshot);
                case BuffEffectKind.Shield:
                    return ExecuteShield(context, snapshot);
                case BuffEffectKind.Charge:
                    return ExecuteCharge(context);
                default:
                    return ExecuteStatus(context, snapshot);
            }
        }

        private static bool ExecuteStatus(
            SkillActionContext context,
            SkillExecutionData snapshot)
        {
            var statusSpec = snapshot.PreparedStatus;
            if (statusSpec == null)
            {
                return false;
            }

            var routed = false;
            var castCommitted = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < snapshot.PreparedTargets.Count; i++)
            {
                var target = snapshot.PreparedTargets[i];
                if (!IsValid(target))
                {
                    continue;
                }

                castCommitted = true;
                if (!StatusCombatRules.ApplyStatus(
                    context.CombatManager,
                    target.Model,
                    statusSpec,
                    context.Caster))
                {
                    continue;
                }

                SpawnVisual(
                    context,
                    snapshot,
                    target,
                    "RuntimeBuffVisual",
                    statusSpec.RuntimeDurationSeconds,
                    ref casterVisualSpawned);
                routed = true;
            }

            return routed || castCommitted;
        }

        private static bool ExecuteHeal(
            SkillActionContext context,
            SkillExecutionData snapshot)
        {
            var target = snapshot.PreparedTargets.Count > 0
                ? snapshot.PreparedTargets[0]
                : null;
            if (!IsValid(target))
            {
                return false;
            }

            context.CombatManager.Heal(target.Model, snapshot.PreparedHealAmount);
            var casterVisualSpawned = false;
            SpawnVisual(
                context,
                snapshot,
                target,
                "RuntimeSupportVisual",
                0.8f,
                ref casterVisualSpawned);
            return true;
        }

        private static bool ExecuteShield(
            SkillActionContext context,
            SkillExecutionData snapshot)
        {
            if (snapshot.PreparedShieldStatusData == null || snapshot.PreparedDuration <= 0f)
            {
                return false;
            }

            var routed = false;
            var casterVisualSpawned = false;
            for (var i = 0; i < snapshot.PreparedTargets.Count; i++)
            {
                var target = snapshot.PreparedTargets[i];
                if (!IsValid(target))
                {
                    continue;
                }

                context.CombatManager.ApplyShieldStatus(
                    target.Model,
                    snapshot.PreparedShieldStatusData,
                    snapshot.PreparedShieldAmount,
                    snapshot.PreparedDuration,
                    1,
                    0,
                    false,
                    true,
                    context.Caster);
                SpawnVisual(
                    context,
                    snapshot,
                    target,
                    "RuntimeShieldVisual",
                    snapshot.PreparedDuration,
                    ref casterVisualSpawned);
                routed = true;
            }

            return routed;
        }

        private static bool ExecuteCharge(SkillActionContext context)
        {
            return context.Caster != null && context.Runtime != null;
        }

        /// 활성 Charge 버프가 접촉한 대상에게 정의된 피해와 상태를 적용한다.
        internal static bool ApplyChargeContact(
            InGameCombatManager combatManager,
            UnitCombatState caster,
            CombatUnitEntry target,
            SkillExecutionData runtime)
        {
            var snapshot = runtime != null ? runtime.ActiveExecutionData : null;
            if (combatManager == null
                || caster == null
                || !IsValid(target)
                || snapshot == null
                || snapshot.PreparedBuffEffectKind != BuffEffectKind.Charge)
            {
                return false;
            }

            var maxHealth = target.Model.Stats != null
                ? Mathf.Max(0f, target.Model.Stats.MaxHealth)
                : 0f;
            var damageResult = combatManager.ApplyDamage(
                target.Model,
                maxHealth * snapshot.PreparedChargeTargetMaxHealthRatio,
                snapshot.PreparedDamageAttribute,
                caster,
                true,
                sourceSkillId: !string.IsNullOrWhiteSpace(snapshot.PreparedSkillId)
                    ? snapshot.PreparedSkillId
                    : runtime.SkillId);

            var statusSpec = snapshot.PreparedStatus;
            if (!damageResult.IsDead && statusSpec != null)
            {
                StatusCombatRules.ApplyStatus(combatManager, target.Model, statusSpec, caster);
            }

            runtime.StopActive();
            return true;
        }

        private static bool IsValid(CombatUnitEntry target)
        {
            return target != null && target.IsAlive && target.Model != null;
        }

        private static void SpawnVisual(
            SkillActionContext context,
            SkillExecutionData snapshot,
            CombatUnitEntry target,
            string namePrefix,
            float duration,
            ref bool casterVisualSpawned)
        {
            if (snapshot.PreparedAttachVisualToCaster && casterVisualSpawned)
            {
                return;
            }

            var visualTarget = snapshot.PreparedAttachVisualToCaster
                ? context.CasterEntry.Transform
                : target.Transform;
            var effects = context.CombatManager.Effects;
            if (visualTarget == null || effects == null)
            {
                return;
            }

            var skillId = !string.IsNullOrWhiteSpace(snapshot.PreparedSkillId)
                ? snapshot.PreparedSkillId
                : snapshot.SkillId;
            var visualName = string.IsNullOrWhiteSpace(skillId)
                ? namePrefix
                : namePrefix + "_" + skillId;
            var instance = effects.CreateEffect(new EffectCreateRequest(
                snapshot.PreparedRuntimeVisual,
                snapshot.PreparedSkillEffectPrefab,
                visualName,
                visualTarget.position,
                Quaternion.identity,
                visualTarget,
                null,
                false,
                true,
                false));
            if (instance != null)
            {
                BuffSkillActor.Attach(instance).InitializeTimed(effects, duration);
                casterVisualSpawned = snapshot.PreparedAttachVisualToCaster;
            }
        }
    }
}
