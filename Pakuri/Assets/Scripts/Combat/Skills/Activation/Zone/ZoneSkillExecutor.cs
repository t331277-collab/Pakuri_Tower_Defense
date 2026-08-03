/*
 * 역할: 지속 영역의 배치 계획을 실행한다.
 * 확정된 중심마다 영역 오브젝트를 만들고 주기 판정에 실행값을 넘긴다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 준비된 중심마다 영역을 배치하고 실제 주기 판정을 시작한다.
    internal static class ZoneSkillExecutor
    {
        /// 각 중심에 표현과 판정 기준을 갖춘 영역 오브젝트를 만든다.
        internal static bool Execute(
            SkillExecutionContext context,
            SkillExecutionState snapshot)
        {
            var effects = context.CombatManager.Effects;
            if (effects == null)
            {
                return false;
            }

            var routed = false;
            var skillId = !string.IsNullOrWhiteSpace(snapshot.PreparedSkillId)
                ? snapshot.PreparedSkillId
                : snapshot.SkillId;
            for (var i = 0; i < snapshot.PreparedCenters.Count; i++)
            {
                var center = snapshot.PreparedCenters[i];
                var objectName = snapshot.PreparedIsRecast ? "InGameRecastZone" : "ZoneSkill";
                if (!string.IsNullOrWhiteSpace(skillId))
                {
                    objectName += "_" + skillId;
                }

                var instance = effects.CreateEffect(new EffectCreateRequest(
                    snapshot.PreparedRuntimeVisual,
                    snapshot.PreparedSkillEffectPrefab,
                    objectName,
                    center,
                    Quaternion.identity,
                    null,
                    null,
                    false,
                    true,
                    true));
                if (instance == null)
                {
                    continue;
                }

                EffectVisualBuilder.ConfigureAreaEffect(
                    instance,
                    snapshot.PreparedBaseRadius,
                    snapshot.RadiusMultiplier,
                    snapshot.RadiusBonus,
                    snapshot.PreparedVisualRadiusMultiplier);
                var actor = instance.GetComponent<ZoneSkillActor>();
                if (actor == null)
                {
                    actor = instance.AddComponent<ZoneSkillActor>();
                }
                actor.Initialize(
                    context.CombatManager,
                    context.CasterEntry,
                    context.Roster,
                    snapshot.PreparedTargeting,
                    center,
                    snapshot.PreparedDuration,
                    snapshot.PreparedTickInterval,
                    snapshot.PreparedDamage,
                    snapshot.PreparedDamageAttribute,
                    snapshot.PreparedStatus,
                    context.Runtime,
                    snapshot,
                    context.Caster,
                    snapshot.PreparedCriticalAllowed,
                    snapshot.CritChanceBonus,
                    snapshot.CritDamageBonus,
                    snapshot.PreparedRecastGeneration);
                routed = true;
            }

            return routed;
        }
    }
}
