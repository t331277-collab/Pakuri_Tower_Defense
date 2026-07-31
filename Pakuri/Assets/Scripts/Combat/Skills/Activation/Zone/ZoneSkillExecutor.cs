/*
 * 역할: Zone 스킬 전달 조정.
 * 책임: 확정된 배치 계획대로 Zone Actor를 생성하고 해석된 실행 데이터를 전달한다.
 */

using System;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 확정된 각 중심에 ZoneSkillActor를 생성하고 실행을 시작한다.
    internal static class ZoneSkillExecutor
    {
        /// 영역형 스킬의 실행 객체와 입력을 준비한다.
        internal static bool Execute(
            SkillActionContext context,
            SkillExecutionData snapshot)
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
                    snapshot.PreparedRadius,
                    snapshot.PreparedCoverAll,
                    snapshot.PreparedDuration,
                    snapshot.PreparedTickInterval,
                    snapshot.PreparedHitTargetCount,
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
