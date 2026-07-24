using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal abstract class SkillExecutor
    {
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

        public abstract bool Execute(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan);

        protected IReadOnlyList<UnitBaseModel> ResolveTargets(SkillExecutionRequest request)
        {
            return Targeting.Resolve(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint);
        }

        protected IReadOnlyList<UnitBaseModel> ResolveTargets(
            SkillExecutionRequest request,
            SkillExecutionPlan plan)
        {
            return plan.FilterTargets(Targeting.Resolve(
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                request.TargetPoint,
                plan.ResolveHitTargetCountBonus()));
        }

        protected EffectHandle CreateEffect(
            SkillExecutionRequest request,
            UnitBaseModel target)
        {
            string path = SkillTargeting.ReadString(
                request.Skill,
                "runtime_visual_sprite_path");
            CombatVector2 direction = request.AimDirection
                ?? (target != null
                    ? target.Position - request.Caster.Position
                    : default);
            return Effects.Create(path, request.Caster.Position, direction.Normalized);
        }

        protected void ApplyStatuses(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target)
        {
            string statusId = request.Skill.status_effect_id;
            if (!string.IsNullOrEmpty(statusId)
                && ShouldApplyStatus(
                    request.Caster,
                    target,
                    request.Skill,
                    statusId))
            {
                ApplyStatus(combat, request, plan, target, statusId);
            }

            foreach (string additionalStatusId in plan.AdditionalStatusIds())
            {
                ApplyStatus(combat, request, plan, target, additionalStatusId);
            }
            for (int index = 0; index < plan.Nodes.Count; index++)
            {
                var node = plan.Nodes[index];
                if (node.node_type_id == "ThresholdApplyStatus"
                    && CountStatus(target, node.arg_1)
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

        protected static void CompleteHit(
            SkillExecutionRequest request,
            UnitBaseModel target)
        {
            request.NotifyHitCompleted(target);
        }

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
            combat.ApplySkillDamage(
                request.Caster,
                target,
                request.Skill,
                multiplier,
                plan.ResolveCriticalChanceBonus(target),
                plan.ResolveCriticalDamageBonus(target),
                plan.IsExecuteConditionMet(target),
                request.TriggerAncestry);

            for (int index = 0; index < plan.Nodes.Count; index++)
            {
                var node = plan.Nodes[index];
                switch (node.node_type_id)
                {
                    case "AdditionalDamage":
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
                        ReduceReload(request.Caster, node.arg_1, node.arg_2);
                        break;
                    case "HitCountCooldownRefund":
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

        private static void ApplyKnockback(
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
            target.SetPosition(target.Position + (direction * distance));
        }

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

        private void ApplyStatus(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            SkillExecutionPlan plan,
            UnitBaseModel target,
            string statusId)
        {
            StatusDefinition status = Catalog.GetStatus(statusId);
            int baseStacks = Math.Max(
                1,
                SkillTargeting.ReadInt(request.Skill, "status_stack_amount"));
            int stacks = plan.ResolveStatusStacks(statusId, baseStacks);
            float duration = plan.ResolveStatusDuration(
                statusId,
                SkillTargeting.ReadFloat(request.Skill, "status_duration_seconds"));
            combat.ApplyStatus(
                request.Caster,
                target,
                status,
                duration > 0f ? duration : null,
                stacks,
                request.Skill.skill_id);
            request.RecordAppliedTarget(target);
            ApplyPlanStatusModifiers(
                request,
                plan,
                target,
                duration > 0f ? duration : 0.00001f);
        }

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
