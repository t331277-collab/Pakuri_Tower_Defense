/*
 * 역할: 직선형 공격의 배치 계획을 실행한다.
 * 확정된 방향과 반복 간격에 맞춰 직선 공격 오브젝트를 만든다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 시전 계획을 직선 공격 오브젝트로 바꾸고 적중 판정을 넘긴다.
    internal sealed class LineSkillExecutor : MonoBehaviour
    {
        private EffectManager effects;

        /// 이펙트 생성
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
                "RuntimeLineExecution",
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

            var executor = instance.AddComponent<LineSkillExecutor>();
            return executor.Initialize(context, snapshot);
        }

        /// 첫 공격을 배치하고 남은 방향의 실행 시점을 정한다. -> 광선이 여러개일 경우
        private bool Initialize(
            SkillExecutionContext context,
            SkillExecutionState snapshot)
        {
            effects = context.CombatManager.Effects;
            var directions = snapshot.PreparedDirections;
            if (directions.Count == 0
                || !ExecuteOnce(context, snapshot, directions[0]))
            {
                effects.RemoveEffect(gameObject);
                return false;
            }

            if (directions.Count == 1)
            {
                effects.RemoveEffect(gameObject);
            }
            else
            {
                StartCoroutine(ExecuteRepeatedLineCasts(context, snapshot, directions));
            }
            return true;
        }

        /// 준비된 방향을 간격에 맞춰 차례로 배치한다.
        private IEnumerator ExecuteRepeatedLineCasts(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            IReadOnlyList<Vector2> directions)
        {
            for (var i = 1; i < directions.Count; i++)
            {
                yield return new WaitForSeconds(snapshot.PreparedRepeatInterval);
                if (context == null
                    || context.CombatManager == null
                    || context.CasterEntry == null
                    || context.Caster == null)
                {
                    break;
                }
                ExecuteOnce(context, snapshot, directions[i]);
            }
            effects.RemoveEffect(gameObject);
        }

        /// 한 방향의 표현과 판정 오브젝트를 완성한다.
        private static bool ExecuteOnce(
            SkillExecutionContext context,
            SkillExecutionState snapshot,
            Vector2 direction)
        {
            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return false;
            }

            var skillId = !string.IsNullOrWhiteSpace(snapshot.PreparedSkillId)
                ? snapshot.PreparedSkillId
                : snapshot.SkillId;
            var center = snapshot.PreparedOrigin
                + direction * (snapshot.PreparedLength * 0.5f);
            var objectName = string.IsNullOrWhiteSpace(skillId)
                ? "LineSkill"
                : "LineSkill_" + skillId;
            var instance = effects.CreateEffect(new EffectCreateRequest(
                snapshot.PreparedRuntimeVisual,
                snapshot.PreparedSkillEffectPrefab,
                objectName,
                center,
                EffectVisualBuilder.Rotation(direction),
                null,
                null,
                false,
                false,
                true));
            if (instance == null)
            {
                return false;
            }

            var actor = instance.GetComponent<LineSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<LineSkillActor>();
            }
            actor.Initialize(
                context.CombatManager,
                context.CasterEntry,
                context.Roster,
                snapshot.PreparedTargeting,
                snapshot.PreparedOrigin,
                direction,
                snapshot.PreparedLength,
                snapshot.PreparedWidth,
                snapshot.PreparedKnockbackDistance,
                snapshot.PreparedDuration,
                snapshot.PreparedTickInterval,
                snapshot.PreparedDamage,
                snapshot.PreparedDamageAttribute,
                snapshot.PreparedStatus,
                context.Runtime,
                snapshot,
                context.Caster,
                skillId,
                snapshot.PreparedCriticalAllowed,
                snapshot.CritChanceBonus,
                snapshot.CritDamageBonus);
            SkillTrigger.PublishLifecycleEvent(
                SkillTriggerEvent.OnDeploymentCast,
                new SkillExecutionContext(context.Caster, skillId, null, center, 0f, 0, snapshot, context));
            return true;
        }
    }
}
