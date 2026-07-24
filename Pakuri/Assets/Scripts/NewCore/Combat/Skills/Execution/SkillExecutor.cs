using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Units.Models;

/* 스킬 계열 실행기가 공유하는 대상 선정, 피해, 상태, 후속 효과 기능을 제공한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal abstract class SkillExecutor
    {
        /* 공통 실행에 필요한 카탈로그와 전투 런타임 서비스를 저장한다. */
        protected SkillExecutor(
            GameDefinitionCatalog catalog,
            SkillTargeting targeting,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue)
        {
            Catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            Targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            Actors = actors ?? throw new ArgumentNullException(nameof(actors));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
            RandomValue = randomValue ?? throw new ArgumentNullException(nameof(randomValue));
        }

        protected GameDefinitionCatalog Catalog { get; }

        protected SkillTargeting Targeting { get; }

        protected SkillActorManager Actors { get; }

        protected EffectManager Effects { get; }

        protected Func<float> RandomValue { get; }

        private readonly UnitMovementController movement =
            new UnitMovementController();

        /* 스킬 계열별 실행 절차를 파생 실행기에 위임한다. */
        public abstract bool Execute(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan);

        /* 요청의 조준 정보와 스킬 정의를 사용해 기본 대상을 선정한다. */
        protected IReadOnlyList<UnitBaseModel> ResolveTargets(SkillExecutionRequest request)
        {
            return Targeting.Resolve(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint);
        }

        /* 명시한 스킬 정의와 최대 대상 수를 사용해 대상을 선정한다. */
        protected IReadOnlyList<UnitBaseModel> ResolveTargets(
            SkillExecutionRequest request,
            SkillExecutionPlan plan)
        {
            return Targeting.Resolve(
                request.Caster,
                request.Skill,
                plan.FilterTargets(request.RegisteredUnits),
                request.TargetPoint,
                plan.ResolveHitTargetCountBonus());
        }

        /* 지정 위치에 스킬 이펙트를 만들고 생명주기 Actor에 등록한다. */
        protected EffectHandle CreateEffectAt(
            SkillExecutionRequest request,
            CombatVector2 position,
            CombatVector2 direction,
            UnitBaseModel statusTarget = null)
        {
            var visual = new EffectVisualSpec(
                SkillTargeting.ReadString(
                    request.Skill,
                    "skill_effect_prefab_path"),
                SkillTargeting.ReadString(
                    request.Skill,
                    "runtime_visual_sprite_path"),
                SkillTargeting.ReadString(
                    request.Skill,
                    "runtime_visual_animator_controller_path"),
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_visual_scale"),
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_visual_scale_x"),
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_visual_scale_y"),
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_visual_scale_z"),
                SkillTargeting.ReadInt(
                    request.Skill,
                    "runtime_visual_sorting_order"));
            if (statusTarget != null
                && string.Equals(
                    SkillTargeting.ReadString(
                        request.Skill,
                        "runtime_visual_anchor"),
                    "StatusTarget",
                    StringComparison.Ordinal))
            {
                position = statusTarget.Position;
            }

            return Effects.Create(
                visual,
                position,
                direction.Normalized);
        }

        /* 적중 이펙트를 대상 위치에 만들도록 지연 또는 즉시 실행을 등록한다. */
        protected void RegisterImpactEffect(
            SkillExecutionRequest request,
            CombatVector2 collisionPosition)
        {
            var visual = new EffectVisualSpec(
                string.Empty,
                SkillTargeting.ReadString(
                    request.Skill,
                    "runtime_impact_visual_sprite_path"),
                SkillTargeting.ReadString(
                    request.Skill,
                    "runtime_impact_visual_animator_controller_path"),
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "runtime_impact_visual_scale"),
                0f,
                0f,
                0f,
                SkillTargeting.ReadInt(
                    request.Skill,
                    "runtime_impact_visual_sorting_order"));
            if (!visual.HasResource)
            {
                return;
            }

            var effect = Effects.Create(
                visual,
                collisionPosition,
                default);
            Actors.Register(new BuffActor(
                request.Skill,
                0.1f,
                effect));
        }

        /* 스킬과 실행 계획이 지정한 상태 효과를 대상에게 적용한다. */
        protected void ApplyStatuses(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target,
            int projectileIndex = -1,
            int burstProjectileCount = 0)
        {
            string statusId = request.Skill.status_effect_id;
            if (!string.IsNullOrEmpty(statusId)
                && ShouldApplyStatus(
                    request.Caster,
                    target,
                    request.Skill,
                    statusId))
            {
                ApplyStatus(
                    combat,
                    request,
                    plan,
                    target,
                    statusId,
                    projectileIndex,
                    burstProjectileCount);
            }

            foreach (string additionalStatusId in plan.AdditionalStatusIds())
            {
                ApplyStatus(combat, request, plan, target, additionalStatusId);
            }
            for (int index = 0; index < plan.Nodes.Count; index++)
            {
                var node = plan.Nodes[index];
                if (node.node_type_id == "ThresholdApplyStatus")
                {
                    plan.ReportConsumed(node);
                    if (CountStatus(target, node.arg_1)
                        >= Math.Max(1, (int)Number(node.arg_2, 1f))
                    && Catalog.Statuses.TryGetValue(
                        node.arg_3,
                        out StatusDefinition thresholdStatus))
                    {
                        combat.ApplyStatus(
                            request.Caster,
                            target,
                            thresholdStatus,
                            null,
                            null,
                            request.Skill.skill_id);
                    }
                }
            }
        }

        /* 요청에 적중 완료를 통지한다. */
        protected static void CompleteHit(
            SkillExecutionRequest request,
            UnitBaseModel target)
        {
            request.NotifyHitCompleted(target);
        }

        /* 실행 계획의 피해, 치명타, 반복, 후속 효과 노드를 포함해 피해를 처리한다. */
        protected void ApplyDamageWithNodes(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target,
            float multiplier,
            int hitIndex,
            bool isLastHit)
        {
            request.RecordAppliedTarget(target);
            string stackStatusId = SkillTargeting.ReadString(
                request.Skill,
                "target_status_stack_status_id");
            int stackCount = CountStatus(target, stackStatusId);
            float stackBaseDamage = Math.Max(
                0f,
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "target_status_stack_base_damage"))
                * stackCount;
            float stackAttackCoefficient = Math.Max(
                0f,
                SkillTargeting.ReadFloat(
                    request.Skill,
                    "target_status_stack_attack_power_coefficient"))
                * stackCount;
            CombatResult result = combat.ApplySkillDamage(
                request.Caster,
                target,
                request.Skill,
                multiplier,
                plan.ResolveCriticalChanceBonus(target),
                plan.ResolveCriticalDamageBonus(target),
                plan.IsExecuteConditionMet(target),
                request.TriggerAncestry,
                stackBaseDamage,
                stackAttackCoefficient);
            if (result.IsDefeated)
            {
                request.NotifyTargetDefeated(target);
            }

            for (int index = 0; index < plan.Nodes.Count; index++)
            {
                var node = plan.Nodes[index];
                switch (node.node_type_id)
                {
                    case "AdditionalDamage":
                        plan.ReportConsumed(node);
                        if (RandomValue() <= Number(node.arg_3, 1f))
                        {
                            ApplySupplemental(
                                combat,
                                request,
                                target,
                                node.arg_4,
                                Number(node.arg_2, 0f),
                                0f,
                                0f,
                                Number(node.arg_1, 0f),
                                node.node_type_id);
                        }
                        break;
                    case "CoreAdditionalDamage":
                        plan.ReportConsumed(node);
                        if (request.HitZone == node.arg_1
                            && RandomValue() <= Number(node.arg_2, 1f))
                        {
                            ApplySupplemental(
                                combat,
                                request,
                                target,
                                node.arg_4,
                                0f,
                                0f,
                                0f,
                                Number(node.arg_3, 0f),
                                node.node_type_id);
                        }
                        break;
                    case "TargetStatusStackDamage":
                        plan.ReportConsumed(node);
                        int stacks = CountStatus(target, node.arg_1);
                        if (stacks > 0)
                        {
                            int maximum = (int)Number(node.arg_2, stacks);
                            stacks = maximum > 0
                                ? Math.Min(stacks, maximum)
                                : stacks;
                            ApplySupplemental(
                                combat,
                                request,
                                target,
                                request.Skill.attribute,
                                Number(node.arg_3, 0f) * stacks,
                                Number(node.arg_4, 0f) * stacks,
                                Number(node.arg_5, 0f) * stacks,
                                Number(node.arg_6, 1f),
                                node.node_type_id,
                                1f + plan.ResolveTargetStatusStackDamageRateBonus(
                                    node.arg_1));
                        }
                        break;
                    case "EveryNthHitChainDamage":
                        plan.ReportConsumed(node);
                        if ((hitIndex + 1)
                            % Math.Max(1, (int)Number(node.arg_1, 1f)) == 0)
                        {
                            ApplyChain(
                                combat,
                                request,
                                target,
                                node.arg_5,
                                Number(node.arg_2, 0f),
                                Number(node.arg_3, 0f),
                                Math.Max(1, (int)Number(node.arg_4, 1f)),
                                node.node_type_id);
                        }
                        break;
                    case "BranchDamage":
                        plan.ReportConsumed(node);
                        if (RandomValue() <= Number(node.arg_1, 0f))
                        {
                            ApplyChain(
                                combat,
                                request,
                                target,
                                request.Skill.attribute,
                                Number(node.arg_3, 0f),
                                Number(node.arg_4, 0f),
                                Math.Max(1, (int)Number(node.arg_2, 1f)),
                                node.node_type_id);
                        }
                        break;
                    case "ReloadReducePerHit":
                        plan.ReportConsumed(node);
                        ReduceReload(request.Caster, node.arg_1, node.arg_2);
                        break;
                    case "HitCountCooldownRefund":
                        plan.ReportConsumed(node);
                        if ((hitIndex + 1)
                            % Math.Max(1, (int)Number(node.arg_2, 1f)) == 0)
                        {
                            ReduceCooldown(
                                request.Caster,
                                node.arg_1,
                                node.arg_3);
                        }
                        break;
                }
            }

            ConsumeAndRedistribute(combat, request, plan, target);
            ApplyKnockback(request, plan, target);
        }

        /* 추가 피해와 코어 추가 피해 노드를 대상에게 적용한다. */
        private void ApplySupplemental(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            UnitBaseModel target,
            string attribute,
            float baseDamage,
            float attackCoefficient,
            float spellCoefficient,
            float multiplier,
            string suffix,
            float coefficientMultiplier = 1f)
        {
            if (baseDamage <= 0f
                && attackCoefficient <= 0f
                && spellCoefficient <= 0f)
            {
                baseDamage = SkillTargeting.ReadFloat(
                    request.Skill,
                    "base_damage");
                attackCoefficient = SkillTargeting.ReadFloat(
                    request.Skill,
                    "attack_power_coefficient");
                spellCoefficient = SkillTargeting.ReadFloat(
                    request.Skill,
                    "spell_power_coefficient");
            }
            combat.ApplyTriggeredDamage(
                request.Caster,
                target,
                request.Skill.skill_id + ":" + suffix,
                string.IsNullOrEmpty(attribute)
                    ? request.Skill.attribute
                    : attribute,
                Math.Max(0f, baseDamage * coefficientMultiplier),
                Math.Max(0f, attackCoefficient * coefficientMultiplier),
                Math.Max(0f, spellCoefficient * coefficientMultiplier),
                Math.Max(0f, multiplier));
        }

        /* 연쇄 피해 노드에 따라 다음 유효 대상에게 피해를 전달한다. */
        private void ApplyChain(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            UnitBaseModel origin,
            string attribute,
            float multiplier,
            float radius,
            int maximum,
            string suffix)
        {
            List<UnitBaseModel> candidates = new List<UnitBaseModel>();
            bool casterEnemy = request.Caster is EnemyModel;
            for (int index = 0; index < request.RegisteredUnits.Count; index++)
            {
                UnitBaseModel unit = request.RegisteredUnits[index];
                if (unit != null
                    && unit.IsAlive
                    && !ReferenceEquals(unit, origin)
                    && (unit is EnemyModel) != casterEnemy
                    && (radius <= 0f
                        || (unit.Position - origin.Position).Magnitude <= radius))
                {
                    candidates.Add(unit);
                }
            }
            for (int index = 1; index < candidates.Count; index++)
            {
                UnitBaseModel value = candidates[index];
                float distance =
                    (value.Position - origin.Position).SqrMagnitude;
                int insertion = index;
                while (insertion > 0
                    && (candidates[insertion - 1].Position - origin.Position)
                        .SqrMagnitude > distance)
                {
                    candidates[insertion] = candidates[insertion - 1];
                    insertion--;
                }
                candidates[insertion] = value;
            }
            for (int index = 0;
                index < Math.Min(maximum, candidates.Count);
                index++)
            {
                ApplySupplemental(
                    combat,
                    request,
                    candidates[index],
                    attribute,
                    0f,
                    0f,
                    0f,
                    multiplier,
                    suffix);
            }
        }

        /* 대상 상태를 소비하고 계산된 양을 다른 대상에게 재분배한다. */
        private void ConsumeAndRedistribute(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target)
        {
            string statusId = SkillTargeting.ReadString(
                request.Skill,
                "consume_target_status_id");
            float ratio = SkillTargeting.ReadFloat(
                request.Skill,
                "consume_target_status_ratio");
            for (int index = 0; index < plan.Nodes.Count; index++)
            {
                var node = plan.Nodes[index];
                if (node.node_type_id == "ConsumeTargetStatusRatioOverride")
                {
                    plan.ReportConsumed(node);
                    ratio = Number(node.arg_1, ratio);
                }
            }
            int consumed = target.ConsumeStatus(
                statusId,
                Math.Max(0f, Math.Min(1f, ratio)));
            if (consumed <= 0 || string.IsNullOrEmpty(statusId)) return;
            for (int nodeIndex = 0; nodeIndex < plan.Nodes.Count; nodeIndex++)
            {
                var node = plan.Nodes[nodeIndex];
                if (node.node_type_id != "RedistributeConsumedStatus"
                    || node.arg_1 != statusId)
                {
                    continue;
                }
                plan.ReportConsumed(node);
                int stacks = (int)Math.Floor(
                    consumed * Number(node.arg_2, 0f));
                stacks = Math.Max(stacks, (int)Number(node.arg_4, 0f));
                int count = Math.Max(1, (int)Number(node.arg_5, 1f));
                float radius = Number(node.arg_3, 0f);
                int applied = 0;
                bool casterEnemy = request.Caster is EnemyModel;
                for (int unitIndex = 0;
                    unitIndex < request.RegisteredUnits.Count
                        && applied < count;
                    unitIndex++)
                {
                    UnitBaseModel unit = request.RegisteredUnits[unitIndex];
                    if (unit == null
                        || !unit.IsAlive
                        || ReferenceEquals(unit, target)
                        || (unit is EnemyModel) == casterEnemy
                        || (radius > 0f
                            && (unit.Position - target.Position).Magnitude > radius))
                    {
                        continue;
                    }
                    combat.ApplyStatus(
                        request.Caster,
                        unit,
                        Catalog.GetStatus(statusId),
                        null,
                        Math.Max(1, stacks),
                        request.Skill.skill_id);
                    applied++;
                }
            }
        }

        /* 시전자 반대 방향으로 계산한 넉백 이동을 대상에게 적용한다. */
        private void ApplyKnockback(
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target)
        {
            float distance = SkillTargeting.ReadFloat(
                request.Skill,
                "knockback_distance")
                * plan.ResolveKnockbackMultiplier();
            if (distance <= 0f || !target.CanMove) return;
            CombatVector2 direction =
                (target.Position - request.Caster.Position).Normalized;
            movement.Displace(target, direction * distance);
        }

        /* 적중 횟수에 비례해 시전자의 재장전 시간을 줄인다. */
        private static void ReduceReload(
            UnitBaseModel caster,
            string skillId,
            string ratioText)
        {
            float ratio = Math.Max(0f, Math.Min(1f, Number(ratioText, 0f)));
            if (caster is MonsterModel monster
                && monster.SkillBucket.Cooldowns.TryGetValue(
                    skillId,
                    out var cooldown))
            {
                cooldown.ReduceReload(ratio);
            }
        }

        /* 적중 횟수에 비례해 지정 스킬의 재사용 대기시간을 줄인다. */
        private static void ReduceCooldown(
            UnitBaseModel caster,
            string skillId,
            string ratioText)
        {
            float ratio = Math.Max(0f, Math.Min(1f, Number(ratioText, 0f)));
            if (caster is MonsterModel monster
                && monster.SkillBucket.Cooldowns.TryGetValue(
                    skillId,
                    out var cooldown))
            {
                cooldown.ReduceCooldown(ratio);
            }
        }

        /* 대상에게 적용된 지정 상태의 전체 스택 수를 계산한다. */
        private static int CountStatus(UnitBaseModel target, string statusId)
        {
            int result = 0;
            for (int index = 0; index < target.StatusEffects.Count; index++)
            {
                if (target.StatusEffects[index].Definition.status_effect_id
                    == statusId)
                {
                    result += target.StatusEffects[index].CurrentStacks;
                }
            }
            return result;
        }

        /* 문자열을 고정 문화권 실수로 변환하고 실패하면 기본값을 반환한다. */
        private static float Number(string text, float fallback)
        {
            return float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float value)
                ? value
                : fallback;
        }

        /* 상태 정의와 실행 계획의 수치 보정을 사용해 상태 효과를 생성한다. */
        private void ApplyStatus(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target,
            string statusId,
            int projectileIndex = -1,
            int burstProjectileCount = 0)
        {
            StatusDefinition status = Catalog.GetStatus(statusId);
            int baseStacks = Math.Max(
                1,
                SkillTargeting.ReadInt(request.Skill, "status_stack_amount"));
            int stacks = plan.ResolveStatusStacks(statusId, baseStacks);
            if (projectileIndex >= 0)
            {
                stacks = plan.ResolveBurstStatusStacks(
                    stacks,
                    projectileIndex,
                    burstProjectileCount);
            }
            int maximumStacks = Math.Max(
                0,
                SkillTargeting.ReadInt(request.Skill, "status_max_stacks"));
            float duration = plan.ResolveStatusDuration(
                statusId,
                plan.ResolveDuration(
                    SkillTargeting.ReadFloat(
                        request.Skill,
                        "status_duration_seconds")));
            combat.ApplyStatus(
                request.Caster,
                target,
                status,
                duration > 0f ? duration : null,
                stacks,
                request.Skill.skill_id,
                maximumStacks > 0 ? maximumStacks : (int?)null);
            request.RecordAppliedTarget(target);
            ApplyPlanStatusModifiers(
                request,
                plan,
                target,
                duration > 0f ? duration : 0.00001f);
        }

        /* 실행 계획의 상태 수정 노드를 새 상태 효과 인스턴스에 반영한다. */
        protected void ApplyPlanStatusModifiers(
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target,
            float duration)
        {
            for (int index = 0; index < plan.Nodes.Count; index++)
            {
                var node = plan.Nodes[index];
                if (!node.node_type_id.StartsWith(
                    "Status",
                    StringComparison.Ordinal)
                    || node.node_type_id == "StatusFilteredDeployment"
                    || node.node_type_id == "StatusRuntimeKindFilter"
                    || node.node_type_id == "StatusStackAmountBonus"
                    || node.node_type_id == "StatusStackAmountSet"
                    || node.node_type_id == "StatusDurationBonus")
                {
                    continue;
                }
                plan.ReportConsumed(node);
                bool valueInSecondArgument =
                    node.node_type_id == "StatusMaxStacksBonus"
                    || node.node_type_id
                        == "StatusConditionalDamageTakenBonus"
                    || node.node_type_id
                        == "StatusConditionalStatusChanceBonus";
                float modifierValue = Number(
                    valueInSecondArgument ? node.arg_2 : node.arg_1,
                    0f);
                string filter = valueInSecondArgument
                    ? node.arg_1
                    : node.arg_2;
                target.AddRuntimeModifier(
                    node.node_type_id,
                    modifierValue,
                    filter,
                    request.Caster,
                    Math.Max(0.00001f, duration),
                    node.arg_3);
            }
        }

        /* 상태 적용 확률과 대상 조건을 검사해 적용 여부를 결정한다. */
        private bool ShouldApplyStatus(
            UnitBaseModel caster,
            UnitBaseModel target,
            SkillDefinition skill,
            string statusId)
        {
            if (!skill.Columns.TryGetValue("status_chance", out object value) || value == null)
            {
                return true;
            }

            if (!(value is float chance))
            {
                return false;
            }
            for (int index = 0; index < caster.RuntimeModifiers.Count; index++)
            {
                var modifier = caster.RuntimeModifiers[index];
                if (modifier.Kind == "StatusConditionalStatusChanceBonus"
                    && HasAnyStatus(target, modifier.Filter))
                {
                    chance += modifier.Value;
                }
            }
            chance -= target.ResolveRuntimeModifier(
                "StatusAilmentResistanceBonus");
            return RandomValue() <= Math.Max(0f, Math.Min(1f, chance));
        }

        /* 대상이 구분자로 나열된 상태 중 하나라도 보유하는지 확인한다. */
        private static bool HasAnyStatus(
            UnitBaseModel target,
            string statusIds)
        {
            string[] ids = (statusIds ?? string.Empty).Split(';');
            for (int statusIndex = 0;
                statusIndex < target.StatusEffects.Count;
                statusIndex++)
            {
                for (int idIndex = 0; idIndex < ids.Length; idIndex++)
                {
                    if (target.StatusEffects[statusIndex]
                            .Definition.status_effect_id
                        == ids[idIndex])
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
