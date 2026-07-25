using System;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 단일 공격 스킬의 대상 피해, 배치, 부가 상태 실행을 담당한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal class SingleAttackExecutor : SkillExecutor
    {
        /* 공통 카탈로그·대상 선정·Actor·이펙트 서비스를 단일 공격 실행기에 연결한다. */
        public SingleAttackExecutor(GameDefinitionCatalog catalog, SkillTargeting targeting, SkillActorManager actors, EffectManager effects, Func<float> randomValue)
            : base(catalog, targeting, actors, effects, randomValue) { }

        /* 단일 공격 plan의 대상·배치 규칙을 해석해 공격 실행 여부를 반환한다. */
        public override bool Execute(InGameCombatManager combat, SkillExecutionRequest request, SkillExecutionPlan plan)
        {
            bool global = string.Equals(
                SkillTargeting.ReadString(request.Skill, "hit_target_count"),
                "global",
                StringComparison.OrdinalIgnoreCase);
            string selection = SkillTargeting.ReadString(
                request.Skill,
                "target_selection");
            if (global
                && UsesStatusFilteredDeployments(
                    request.Skill,
                    plan))
            {
                return ExecuteStatusFilteredDeployments(
                    combat,
                    request,
                    plan);
            }
            bool spatial = request.TargetPoint.HasValue
                && !global
                && string.IsNullOrEmpty(selection)
                && (SkillTargeting.ReadFloat(request.Skill, "radius") > 0f
                    || SkillTargeting.ReadFloat(
                        request.Skill,
                        "runtime_hitbox_size_x") > 0f
                    || SkillTargeting.ReadFloat(
                        request.Skill,
                        "runtime_hitbox_size_y") > 0f);
            System.Collections.Generic.IReadOnlyList<UnitBaseModel> targets;
            CombatVector2 visualCenter;
            if (spatial)
            {
                var ordered = plan.FilterTargets(
                    Targeting.ResolveOrderedAll(
                        request.Caster,
                        request.Skill,
                        request.RegisteredUnits,
                        request.TargetPoint));
                float radius = plan.ResolveRadius(
                    SkillTargeting.ReadFloat(request.Skill, "radius"));
                targets = LimitHitTargets(
                    Targeting.InRadius(
                        ordered,
                        request.TargetPoint.Value,
                        radius),
                    request.Skill,
                    plan);
                visualCenter = request.TargetPoint.Value;
            }
            else
            {
                targets = ResolveTargets(request, plan);
                if (targets.Count == 0) return false;
                visualCenter = request.TargetPoint
                    ?? ResolveCenter(targets);
            }

            for (int targetIndex = 0;
                targetIndex < targets.Count;
                targetIndex++)
            {
                ExecuteTarget(
                    combat,
                    request,
                    plan,
                    targets[targetIndex]);
            }
            UnitBaseModel visualTarget = null;
            if (targets.Count > 0)
            {
                visualTarget = targets[0];
            }
            Actors.Register(new SingleAttackActor(
                (SingleAttackDefinition)request.Skill,
                1f,
                CreateEffectAt(
                    request,
                    visualCenter,
                    request.AimDirection ?? default,
                    visualTarget)));
            return true;
        }

        /* authored 적중 수와 plan 보너스로 대상 목록의 최대 길이를 제한한다. */
        private static System.Collections.Generic.IReadOnlyList<UnitBaseModel>
            LimitHitTargets(
                System.Collections.Generic.IReadOnlyList<UnitBaseModel> targets,
                SkillDefinition skill,
                SkillExecutionPlan plan)
        {
            string authored = SkillTargeting.ReadString(
                skill,
                "hit_target_count");
            if (!int.TryParse(authored, out int baseCount))
            {
                return targets;
            }

            int maximum = Math.Max(
                0,
                baseCount + plan.ResolveHitTargetCountBonus());
            if (maximum >= targets.Count)
            {
                return targets;
            }

            var limited =
                new System.Collections.Generic.List<UnitBaseModel>(maximum);
            for (var index = 0; index < maximum; index++)
            {
                limited.Add(targets[index]);
            }
            return limited;
        }

        /* 상태 필터별 배치 node를 만족하는 대상에게 개별 공격을 실행한다. */
        private bool ExecuteStatusFilteredDeployments(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan)
        {
            System.Collections.Generic.IReadOnlyList<UnitBaseModel> candidates =
                Targeting.ResolveOrderedAll(
                    request.Caster,
                    request.Skill,
                    request.RegisteredUnits);
            System.Collections.Generic.IReadOnlyList<UnitBaseModel>
                planFiltered = plan.FilterTargets(candidates);
            string requiredStatusId = SkillTargeting.ReadString(
                request.Skill,
                "deployment_required_target_status_id");
            int requiredStacks = Math.Max(
                1,
                SkillTargeting.ReadInt(
                    request.Skill,
                    "deployment_required_target_status_min_stacks"));
            var centers =
                new System.Collections.Generic.List<UnitBaseModel>();
            for (var index = 0; index < planFiltered.Count; index++)
            {
                if (string.IsNullOrEmpty(requiredStatusId)
                    || CountStatus(
                        planFiltered[index],
                        requiredStatusId) >= requiredStacks)
                {
                    centers.Add(planFiltered[index]);
                }
            }
            if (centers.Count == 0)
            {
                return false;
            }

            float radius = SkillTargeting.ReadFloat(
                request.Skill,
                "radius");
            if (radius <= 0f)
            {
                float hitboxX = SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_hitbox_size_x");
                float hitboxY = SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_hitbox_size_y");
                radius = 0.5f * (float)Math.Sqrt(
                    (hitboxX * hitboxX)
                    + (hitboxY * hitboxY));
            }
            radius = plan.ResolveRadius(radius);

            for (var centerIndex = 0;
                centerIndex < centers.Count;
                centerIndex++)
            {
                UnitBaseModel center = centers[centerIndex];
                System.Collections.Generic.IReadOnlyList<UnitBaseModel> hits =
                    Targeting.InRadius(
                        candidates,
                        center.Position,
                        radius);
                for (var hitIndex = 0;
                    hitIndex < hits.Count;
                    hitIndex++)
                {
                    ExecuteTarget(
                        combat,
                        request,
                        plan,
                        hits[hitIndex]);
                }
                Actors.Register(new SingleAttackActor(
                    (SingleAttackDefinition)request.Skill,
                    1f,
                    CreateEffectAt(
                        request,
                        center.Position,
                        request.AimDirection ?? default,
                        center)));
            }

            return true;
        }

        /* 한 대상에게 피해·상태·후속 효과와 시각 효과를 적용한다. */
        private void ExecuteTarget(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target)
        {
            ApplyDamageWithNodes(
                combat,
                request,
                plan,
                target,
                plan.ResolveDamageMultiplier(
                    target,
                    0,
                    request.HitZone),
                0);
            ApplyStatuses(combat, request, plan, target);
            CompleteHit(request, target);
            int repeatCount = plan.ResolveRepeatCount();
            if (repeatCount <= 0)
            {
                return;
            }

            Actors.Register(new ScheduledSkillActor(
                request.Skill,
                repeatCount,
                plan.ResolveRepeatInterval(),
                repeatIndex =>
                {
                    if (!target.IsAlive) return;
                    ApplyDamageWithNodes(
                        combat,
                        request,
                        plan,
                        target,
                        plan.ResolveDamageMultiplier(
                            target,
                            repeatIndex + 1,
                            request.HitZone)
                            * plan.ResolveRepeatDamageMultiplier(),
                        repeatIndex + 1);
                    ApplyStatuses(combat, request, plan, target);
                    CompleteHit(request, target);
                },
                null,
                plan.ResolveRepeatInterval()));
        }

        /* plan에 상태 필터 기반 배치 node가 포함되어 있는지 확인한다. */
        private static bool UsesStatusFilteredDeployments(
            SkillDefinition skill,
            SkillExecutionPlan plan)
        {
            if (!string.IsNullOrEmpty(
                SkillTargeting.ReadString(
                    skill,
                    "deployment_required_target_status_id")))
            {
                return true;
            }
            for (var index = 0; index < plan.Nodes.Count; index++)
            {
                if (plan.Nodes[index].node_type_id
                    == "StatusFilteredDeployment")
                {
                    return true;
                }
            }

            return false;
        }

        /* 대상에게 적용된 지정 status id의 총 스택 수를 계산한다. */
        private static int CountStatus(
            UnitBaseModel target,
            string statusId)
        {
            int result = 0;
            for (var index = 0;
                index < target.StatusEffects.Count;
                index++)
            {
                if (target.StatusEffects[index]
                        .Definition.status_effect_id == statusId)
                {
                    result += target.StatusEffects[index].CurrentStacks;
                }
            }
            return result;
        }

        /* 수동 목표점, 첫 대상 위치, 시전자 위치 순으로 시각 효과 중심을 정한다. */
        private static CombatVector2 ResolveCenter(
            System.Collections.Generic.IReadOnlyList<UnitBaseModel> targets)
        {
            float x = 0f;
            float y = 0f;
            for (int index = 0; index < targets.Count; index++)
            {
                x += targets[index].Position.X;
                y += targets[index].Position.Y;
            }

            return new CombatVector2(x / targets.Count, y / targets.Count);
        }
    }
}
