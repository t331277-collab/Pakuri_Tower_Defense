/*
 * 역할: 지원형 스킬의 확정값을 전투에 적용한다.
 * 책임: 상태, 회복, 보호막, 돌진을 종류에 맞는 공통 전투 경로로 보낸다.
 */

using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 확정된 지원 효과를 실제 전투 변화와 시각 표현으로 연결한다.
    internal static class BuffSkillExecutor
    {

        /// 지원 효과의 성격에 맞는 적용 방식을 고른다.
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

        /// 상태 효과를 대상마다 적용하고 성공한 표현을 남긴다.
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

        /// 우선 대상의 생명력을 회복하고 결과를 표현한다.
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

        /// 대상마다 보호막 상태와 그 수명을 시작한다.
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

        /// 돌진을 맡을 시전자와 실행 상태가 준비됐는지 확인한다.
        private static bool ExecuteCharge(SkillActionContext context)
        {
            return context.Caster != null && context.Runtime != null;
        }

        /// 살아 있는 전투 유닛만 지원 대상으로 허용한다.
        private static bool IsValid(CombatUnitEntry target)
        {
            return target != null && target.IsAlive && target.Model != null;
        }

        /// 효과가 붙을 위치와 수명에 맞춰 표현을 만든다.
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
