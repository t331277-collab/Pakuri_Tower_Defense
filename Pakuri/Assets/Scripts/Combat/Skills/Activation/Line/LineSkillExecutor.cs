/*
 * 역할: Line 스킬 전달 조정.
 * 책임: 확정된 방향과 반복 간격대로 Line Actor를 생성한다.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 확정된 Line 배치 계획을 실행하고 실제 판정은 LineSkillActor에 맡긴다.
    internal sealed class LineSkillExecutor : MonoBehaviour
    {
        private EffectManager effects;

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

        private bool Initialize(
            SkillActionContext context,
            SkillExecutionData snapshot)
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

        private IEnumerator ExecuteRepeatedLineCasts(
            SkillActionContext context,
            SkillExecutionData snapshot,
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

        private static bool ExecuteOnce(
            SkillActionContext context,
            SkillExecutionData snapshot,
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
                new SkillActionContext(context.Caster, skillId, null, center, 0f, 0, snapshot, context));
            return true;
        }
    }
}
