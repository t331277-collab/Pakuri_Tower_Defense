using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public sealed class SkillEffectGraphRuntime
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly SkillActorManager actors;
        private readonly EffectManager effects;
        private readonly Func<float> randomValue;
        private readonly Action<ChoiceNodeDefinition> nodeConsumed;

        public SkillEffectGraphRuntime(
            GameDefinitionCatalog catalog,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue,
            Action<ChoiceNodeDefinition> nodeConsumed = null)
        {
            this.catalog = catalog;
            this.actors = actors;
            this.effects = effects;
            this.randomValue = randomValue;
            this.nodeConsumed = nodeConsumed;
        }

        public void ExecuteOwnedGraphs(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            string effectTiming = "OnCast")
        {
            HashSet<string> owners = new HashSet<string>(StringComparer.Ordinal)
            {
                request.Skill.skill_id
            };
            if (request.Caster is MonsterModel monster)
            {
                for (int index = 0;
                    index < monster.SkillBucket.SelectedChoices.Count;
                    index++)
                {
                    SkillChoiceDefinition choice =
                        monster.SkillBucket.SelectedChoices[index];
                    string effectiveSkillId =
                        string.IsNullOrEmpty(choice.target_skill_id)
                            ? choice.skill_id
                            : choice.target_skill_id;
                    if (effectiveSkillId == request.Skill.skill_id
                        || ChoiceTargetsSkill(
                            choice.choice_id,
                            request.Skill.skill_id))
                    {
                        owners.Add(choice.choice_id);
                    }
                }
            }
            ExecuteGraphs(
                combat,
                request,
                owners,
                effectTiming: effectTiming);
        }

        private bool ChoiceTargetsSkill(
            string choiceId,
            string skillId)
        {
            for (var index = 0;
                index < catalog.ChoiceNodes.Count;
                index++)
            {
                ChoiceNodeDefinition node =
                    catalog.ChoiceNodes[index];
                if (node.owner_id == choiceId
                    && node.target_skill_id == skillId)
                {
                    return true;
                }
            }

            return false;
        }

        public void ExecuteTriggerGraph(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            string ownerId,
            string ownerKind = null,
            string graphKind = "Effect",
            int? graphIndex = null)
        {
            ExecuteGraphs(
                combat,
                request,
                new HashSet<string>(StringComparer.Ordinal) { ownerId },
                graphKind,
                graphIndex,
                true,
                requiredOwnerKind: ownerKind);
        }

        private void ExecuteGraphs(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            HashSet<string> owners,
            string requiredGraphKind = "Effect",
            int? requiredGraphIndex = null,
            bool ownerSelectsGraphExactly = false,
            string effectTiming = null,
            string requiredOwnerKind = null)
        {
            SortedDictionary<string, List<ChoiceNodeDefinition>> graphs =
                new SortedDictionary<string, List<ChoiceNodeDefinition>>(
                    StringComparer.Ordinal);
            for (int index = 0; index < catalog.ChoiceNodes.Count; index++)
            {
                ChoiceNodeDefinition node = catalog.ChoiceNodes[index];
                if (node.graph_kind != requiredGraphKind
                    || (requiredGraphIndex.HasValue
                        && node.graph_index != requiredGraphIndex)
                    || (!string.IsNullOrEmpty(requiredOwnerKind)
                        && node.owner_kind != requiredOwnerKind)
                    || !owners.Contains(node.owner_id)
                    || (!ownerSelectsGraphExactly
                        && !string.IsNullOrEmpty(node.target_skill_id)
                        && node.target_skill_id != request.Skill.skill_id)
                    || IsExcluded(request.Caster, node.excludes_active_choice_id))
                {
                    continue;
                }
                string ownerOrder = string.Equals(
                    node.owner_id,
                    request.Skill.skill_id,
                    StringComparison.Ordinal)
                        ? "0:"
                        : "1:";
                string key = ownerOrder
                    + node.owner_id
                    + ":"
                    + (node.graph_index ?? 0);
                if (!graphs.TryGetValue(
                    key,
                    out List<ChoiceNodeDefinition> graph))
                {
                    graph = new List<ChoiceNodeDefinition>();
                    graphs.Add(key, graph);
                }
                graph.Add(node);
            }

            foreach (List<ChoiceNodeDefinition> graph in graphs.Values)
            {
                graph.Sort((left, right) =>
                    Nullable.Compare(left.node_order, right.node_order));
                ExecuteGraph(
                    combat,
                    request,
                    graph,
                    effectTiming);
            }
        }

        private void ExecuteGraph(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            List<ChoiceNodeDefinition> graph,
            string requiredEffectTiming)
        {
            string graphTiming = ReadEffectTiming(graph);
            if (!string.IsNullOrEmpty(requiredEffectTiming)
                && !string.Equals(
                    string.IsNullOrEmpty(graphTiming)
                        ? "OnCast"
                        : graphTiming,
                    requiredEffectTiming,
                    StringComparison.Ordinal))
            {
                return;
            }
            List<UnitBaseModel> targets = ResolveTargets(request, graph);
            if (!FilterTargetsBySkillAttribute(
                    graph,
                    targets)
                || !FilterTargetsByStatusConditions(
                    graph,
                    targets))
            {
                return;
            }
            if (!PassesConditions(combat, request, graph, targets))
            {
                return;
            }
            float lifetime = ReadLifetime(graph);
            float delay = ReadTargetDelay(graph);
            string runtimeKindFilter = ReadRuntimeKindFilter(graph);
            bool hasAttachedPayload = false;
            bool hasStatusApplication = false;
            bool hasStatusModifier = false;
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id == "AttachStatusPayload")
                {
                    hasAttachedPayload = true;
                    hasStatusApplication = true;
                }
                else if (graph[index].node_type_id == "ApplyStatus")
                {
                    hasStatusApplication = true;
                }
                if (graph[index].node_type_id == "StatusModifier")
                {
                    hasStatusModifier = true;
                }
            }
            for (int index = 0; index < graph.Count; index++)
            {
                ChoiceNodeDefinition node = graph[index];
                nodeConsumed?.Invoke(node);
                switch (node.node_type_id)
                {
                    case "ApplyStatus":
                        if (!hasAttachedPayload)
                        {
                            ApplyStatus(
                                combat,
                                request,
                                targets,
                                node.arg_1,
                                null,
                                null);
                        }
                        break;
                    case "AttachStatusPayload":
                        if (randomValue() <= Number(node.arg_2, 1f))
                        {
                            ApplyStatus(
                                combat,
                                request,
                                targets,
                                node.arg_1,
                                NullableNumber(node.arg_4),
                                NullableInt(node.arg_6),
                                NullableInt(node.arg_5));
                        }
                        break;
                    case "ApplyShield":
                        ApplyShield(
                            combat,
                            request,
                            targets,
                            node,
                            delay,
                            lifetime);
                        break;
                    case "EffectDamage":
                        ApplyEffectDamage(
                            combat,
                            request,
                            targets,
                            node,
                            lifetime,
                            delay);
                        break;
                    case "EffectExtendStatusDuration":
                        ExtendStatus(targets, node.arg_1, lifetime);
                        break;
                    case "RecastZone":
                        ScheduleRecast(combat, request, node);
                        break;
                    case "EffectVisual":
                    case "RuntimeEffectVisual":
                        CreateVisual(request, targets, node, lifetime);
                        break;
                    case "ConditionAnyStatus":
                    case "ConditionHealthRatioMax":
                    case "ConditionHitCountMin":
                    case "ConditionSkillAttribute":
                    case "ConditionStatus":
                    case "ConditionStatusExpression":
                    case "EffectLifetime":
                    case "EffectTarget":
                    case "RequiredSourceStatus":
                    case "StatusModifier":
                    case "StatusRuntimeKindFilter":
                        // These nodes are graph metadata consumed before
                        // the mutation loop. Keep them explicit so a
                        // reachable row cannot become a silent default.
                        break;
                    default:
                        if (node.node_type_id.StartsWith(
                            "Status",
                            StringComparison.Ordinal)
                            && node.node_type_id != "StatusModifier"
                            && node.node_type_id != "StatusRuntimeKindFilter"
                            && node.node_type_id != "StatusFilteredDeployment")
                        {
                            AddModifier(
                                request,
                                targets,
                                node,
                                lifetime,
                                runtimeKindFilter);
                            break;
                        }
                        throw new NotSupportedException(
                            $"Effect graph node '{node.node_type_id}' "
                            + "has no runtime consumer.");
                }
            }
            if (hasStatusModifier && !hasStatusApplication)
            {
                SkillExecutionPlan plan = SkillExecutionPlan.Create(
                    catalog,
                    request.Caster,
                    request.Skill,
                    request.RegisteredUnits,
                    nodeConsumed);
                for (var index = 0;
                    index < plan.Nodes.Count;
                    index++)
                {
                    ChoiceNodeDefinition node = plan.Nodes[index];
                    if (!node.node_type_id.StartsWith(
                            "Status",
                            StringComparison.Ordinal)
                        || node.node_type_id
                            == "StatusFilteredDeployment"
                        || node.node_type_id
                            == "StatusRuntimeKindFilter"
                        || node.node_type_id
                            == "StatusStackAmountBonus"
                        || node.node_type_id
                            == "StatusStackAmountSet"
                        || (node.node_type_id
                            == "StatusDurationBonus"
                            && !TargetsAllies(graph)))
                    {
                        continue;
                    }
                    plan.ReportConsumed(node);
                    AddModifier(
                        request,
                        targets,
                        node,
                        lifetime,
                        runtimeKindFilter);
                }
            }
        }

        private void ApplyEffectDamage(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            List<UnitBaseModel> targets,
            ChoiceNodeDefinition node,
            float lifetime,
            float delay)
        {
            float interval = Number(node.arg_7, 0f);
            int count = interval > 0f && lifetime > 0f
                ? Math.Max(1, (int)Math.Ceiling(lifetime / interval))
                : 1;
            actors.Register(new ScheduledSkillActor(
                request.Skill,
                count,
                interval,
                _ =>
                {
                    for (int targetIndex = 0;
                        targetIndex < targets.Count;
                        targetIndex++)
                    {
                        UnitBaseModel target = targets[targetIndex];
                        if (!target.IsAlive) continue;
                        combat.ApplyTriggeredDamage(
                            request.Caster,
                            target,
                            request.Skill.skill_id + ":effect",
                            node.arg_1,
                            Math.Max(0f, Number(node.arg_2, 0f)),
                            Math.Max(0f, Number(node.arg_6, 0f)),
                            Math.Max(0f, Number(node.arg_3, 0f)),
                            Math.Max(0f, Number(node.arg_4, 1f)),
                            request.TriggerAncestry);
                    }
                },
                null,
                Math.Max(0f, delay)));
        }

        private void ApplyShield(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            List<UnitBaseModel> targets,
            ChoiceNodeDefinition node,
            float delay,
            float lifetime)
        {
            string shieldSourceId = request.Skill.skill_id
                + "@effect"
                + ((node.graph_index ?? 0) + 1);
            SkillExecutionPlan plan = SkillExecutionPlan.Create(
                catalog,
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                nodeConsumed);
            float resolvedLifetime = plan.ResolveDuration(lifetime);
            Action<int> apply = _ =>
            {
                float amount = Math.Max(
                    0f,
                    Number(node.arg_1, 0f)
                    + (combat.CalculateSpellPower(request.Caster)
                        * Math.Max(0f, Number(node.arg_2, 0f))));
                amount *= plan.ResolveShieldMultiplier();
                for (int index = 0; index < targets.Count; index++)
                {
                    if (targets[index].IsAlive)
                    {
                        combat.AddShield(
                            request.Caster,
                            targets[index],
                            request.Skill,
                            amount,
                            shieldSourceId);
                    }
                }
            };
            if (delay <= 0f)
            {
                apply(0);
            }
            else
            {
                actors.Register(new ScheduledSkillActor(
                    request.Skill,
                    1,
                    0f,
                    apply,
                    null,
                    delay));
            }
            if (resolvedLifetime > 0f)
            {
                actors.Register(new ScheduledSkillActor(
                    request.Skill,
                    1,
                    0f,
                    _ =>
                    {
                        for (int index = 0; index < targets.Count; index++)
                        {
                            float expired = targets[index].RemoveShield(
                                    request.Caster,
                                    shieldSourceId);
                            if (expired > 0f)
                            {
                                combat.NotifyShieldExpired(
                                    request.Caster,
                                    request.Skill,
                                    targets[index],
                                    expired);
                            }
                        }
                    },
                    null,
                    Math.Max(0f, delay) + resolvedLifetime));
            }
        }

        private void ApplyStatus(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            List<UnitBaseModel> targets,
            string statusId,
            float? duration,
            int? stacks,
            int? maximumStacks = null)
        {
            if (string.IsNullOrEmpty(statusId)
                || !catalog.Statuses.TryGetValue(statusId, out var status))
            {
                return;
            }
            SkillExecutionPlan plan = SkillExecutionPlan.Create(
                catalog,
                request.Caster,
                request.Skill,
                request.RegisteredUnits,
                nodeConsumed);
            float resolvedDuration = plan.ResolveStatusDuration(
                statusId,
                duration
                    ?? status.default_duration_seconds
                    ?? 0f);
            int resolvedStacks = plan.ResolveStatusStacks(
                statusId,
                stacks
                    ?? status.base_stack_amount
                    ?? 1);
            int resolvedMaximum = maximumStacks
                ?? status.max_stacks
                ?? 0;
            for (int index = 0; index < targets.Count; index++)
            {
                if (targets[index].IsAlive)
                {
                    combat.ApplyStatus(
                        request.Caster,
                        targets[index],
                        status,
                        resolvedDuration > 0f
                            ? resolvedDuration
                            : (float?)null,
                        resolvedStacks,
                        request.Skill.skill_id,
                        resolvedMaximum > 0
                            ? resolvedMaximum
                            : (int?)null);
                    for (var planIndex = 0;
                        planIndex < plan.Nodes.Count;
                        planIndex++)
                    {
                        ChoiceNodeDefinition modifier =
                            plan.Nodes[planIndex];
                        if (!modifier.node_type_id.StartsWith(
                                "Status",
                                StringComparison.Ordinal)
                            || modifier.node_type_id
                                == "StatusFilteredDeployment"
                            || modifier.node_type_id
                                == "StatusRuntimeKindFilter"
                            || modifier.node_type_id
                                == "StatusStackAmountBonus"
                            || modifier.node_type_id
                                == "StatusStackAmountSet"
                            || modifier.node_type_id
                                == "StatusDurationBonus")
                        {
                            continue;
                        }
                        plan.ReportConsumed(modifier);
                        AddModifierToTarget(
                            request,
                            targets[index],
                            modifier,
                            resolvedDuration > 0f
                                ? resolvedDuration
                                : 0.00001f,
                            null);
                    }
                }
            }
        }

        private static void AddModifier(
            SkillExecutionRequest request,
            List<UnitBaseModel> targets,
            ChoiceNodeDefinition node,
            float lifetime,
            string runtimeKindFilter)
        {
            float duration = lifetime > 0f ? lifetime : 0.00001f;
            for (int index = 0; index < targets.Count; index++)
            {
                AddModifierToTarget(
                    request,
                    targets[index],
                    node,
                    duration,
                    runtimeKindFilter);
            }
        }

        private static void AddModifierToTarget(
            SkillExecutionRequest request,
            UnitBaseModel target,
            ChoiceNodeDefinition node,
            float duration,
            string runtimeKindFilter)
        {
            bool valueInSecondArgument =
                node.node_type_id == "StatusMaxStacksBonus"
                || node.node_type_id
                    == "StatusConditionalDamageTakenBonus"
                || node.node_type_id
                    == "StatusConditionalStatusChanceBonus"
                || node.node_type_id
                    == "StatusDurationBonus";
            float value = valueInSecondArgument
                ? Number(node.arg_2, 0f)
                : Number(node.arg_1, 0f);
            string filter = valueInSecondArgument
                ? node.arg_1
                : !string.IsNullOrEmpty(runtimeKindFilter)
                    ? runtimeKindFilter
                    : node.arg_2;
            target.AddRuntimeModifier(
                node.node_type_id,
                value,
                filter,
                request.Caster,
                Math.Max(0.00001f, duration),
                node.arg_3);
        }

        private static bool TargetsAllies(
            IReadOnlyList<ChoiceNodeDefinition> graph)
        {
            for (var index = 0; index < graph.Count; index++)
            {
                ChoiceNodeDefinition node = graph[index];
                if (node.node_type_id != "EffectTarget")
                {
                    continue;
                }

                return node.arg_1 == "AllAllies"
                    || node.arg_1 == "Ally"
                    || node.arg_1 == "Self";
            }

            return false;
        }

        private static void ExtendStatus(
            List<UnitBaseModel> targets,
            string statusId,
            float duration)
        {
            for (int targetIndex = 0;
                targetIndex < targets.Count;
                targetIndex++)
            {
                UnitBaseModel target = targets[targetIndex];
                for (int statusIndex = 0;
                    statusIndex < target.StatusEffects.Count;
                    statusIndex++)
                {
                    if (target.StatusEffects[statusIndex]
                            .Definition.status_effect_id == statusId)
                    {
                        target.StatusEffects[statusIndex].Extend(duration);
                    }
                }
            }
        }

        private void ScheduleRecast(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            ChoiceNodeDefinition node)
        {
            if (!catalog.Skills.TryGetValue(
                node.arg_1,
                out SkillDefinition recastSkill))
            {
                return;
            }
            actors.Register(new ScheduledSkillActor(
                request.Skill,
                1,
                0f,
                _ => combat.TryExecuteSkill(new SkillExecutionRequest(
                    request.Caster,
                    recastSkill,
                    request.RegisteredUnits,
                    request.AimDirection,
                    request.TargetPoint,
                    true)),
                null,
                Math.Max(0f, Number(node.arg_2, 0f))));
        }

        private void CreateVisual(
            SkillExecutionRequest request,
            List<UnitBaseModel> targets,
            ChoiceNodeDefinition node,
            float lifetime)
        {
            UnitBaseModel target = targets.Count > 0 ? targets[0] : null;
            if (target == null)
            {
                return;
            }

            bool prefabVisual =
                node.node_type_id == "EffectVisual";
            var visual = new EffectVisualSpec(
                prefabVisual ? node.arg_1 : string.Empty,
                prefabVisual ? string.Empty : node.arg_1,
                prefabVisual ? string.Empty : node.arg_2,
                prefabVisual ? 1f : Number(node.arg_3, 1f),
                0f,
                0f,
                0f,
                prefabVisual ? 0 : Integer(node.arg_4, 0));
            EffectHandle effect = effects.Create(
                visual,
                target.Position,
                (target.Position - request.Caster.Position).Normalized);
            actors.Register(new BuffActor(
                request.Skill,
                Math.Max(0.00001f, lifetime),
                effect));
        }

        private static bool PassesConditions(
            InGameCombatManager combat,
            SkillExecutionRequest request,
            List<ChoiceNodeDefinition> graph,
            List<UnitBaseModel> targets)
        {
            UnitBaseModel target = targets.Count > 0 ? targets[0] : request.Caster;
            for (int index = 0; index < graph.Count; index++)
            {
                ChoiceNodeDefinition node = graph[index];
                if (node.node_type_id == "ConditionHealthRatioMax"
                    && (target == null
                        || target.CurrentHealth / target.MaximumHealth
                        > Number(node.arg_1, 1f))
                    )
                {
                    return false;
                }
                if (node.node_type_id == "ConditionHitCountMin"
                    && combat.GetOutgoingHitCount(request.Caster)
                        < Math.Max(1, NullableInt(node.arg_1) ?? 1))
                {
                    return false;
                }
                if (node.node_type_id == "RequiredSourceStatus"
                    && !HasStatus(
                        request.Caster,
                        node.arg_1,
                        Math.Max(
                            1,
                            NullableInt(
                                node.arg_2)
                                ?? 1)))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool FilterTargetsBySkillAttribute(
            List<ChoiceNodeDefinition> graph,
            List<UnitBaseModel> targets)
        {
            string requiredAttribute = null;
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id
                    == "ConditionSkillAttribute")
                {
                    requiredAttribute = graph[index].arg_1;
                    break;
                }
            }

            if (string.IsNullOrWhiteSpace(requiredAttribute))
            {
                return true;
            }

            for (int index = targets.Count - 1; index >= 0; index--)
            {
                if (!HasLearnedActiveAttribute(
                    targets[index],
                    requiredAttribute))
                {
                    targets.RemoveAt(index);
                }
            }

            return targets.Count > 0;
        }

        private static bool FilterTargetsByStatusConditions(
            List<ChoiceNodeDefinition> graph,
            List<UnitBaseModel> targets)
        {
            bool hasTargetCondition = false;
            for (int nodeIndex = 0;
                nodeIndex < graph.Count;
                nodeIndex++)
            {
                ChoiceNodeDefinition node = graph[nodeIndex];
                if (node.node_type_id != "ConditionStatus"
                    && node.node_type_id != "ConditionAnyStatus"
                    && node.node_type_id
                        != "ConditionStatusExpression")
                {
                    continue;
                }

                hasTargetCondition = true;
                for (int targetIndex = targets.Count - 1;
                    targetIndex >= 0;
                    targetIndex--)
                {
                    if (!MatchesStatusCondition(
                        targets[targetIndex],
                        node))
                    {
                        targets.RemoveAt(targetIndex);
                    }
                }
            }

            return !hasTargetCondition || targets.Count > 0;
        }

        private static bool MatchesStatusCondition(
            UnitBaseModel target,
            ChoiceNodeDefinition node)
        {
            if (node.node_type_id == "ConditionStatus")
            {
                return HasStatus(
                    target,
                    node.arg_1,
                    Math.Max(
                        1,
                        NullableInt(node.arg_4) ?? 1),
                    node.arg_3);
            }

            char separator = node.node_type_id
                == "ConditionStatusExpression"
                    ? '&'
                    : ';';
            string[] ids = (node.arg_1 ?? string.Empty)
                .Split(separator);
            if (node.node_type_id == "ConditionAnyStatus")
            {
                for (int index = 0; index < ids.Length; index++)
                {
                    if (HasStatus(target, ids[index], 1))
                    {
                        return true;
                    }
                }

                return false;
            }

            for (int index = 0; index < ids.Length; index++)
            {
                if (!HasStatus(target, ids[index], 1))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasLearnedActiveAttribute(
            UnitBaseModel unit,
            string attribute)
        {
            IReadOnlyList<SkillDefinition> skills =
                unit is MonsterModel monster
                    ? monster.SkillBucket.ActiveSkills
                    : unit is EnemyModel enemy
                        ? enemy.SkillBucket.ActiveSkills
                        : null;
            if (skills == null)
            {
                return false;
            }

            for (int index = 0; index < skills.Count; index++)
            {
                if (string.Equals(
                    skills[index].attribute,
                    attribute,
                    StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasStatus(
            UnitBaseModel unit,
            string statusId,
            int minimumStacks,
            string sourceSkillId = null)
        {
            if (unit == null)
            {
                return false;
            }
            if (string.Equals(
                    statusId,
                    "shield",
                    StringComparison.OrdinalIgnoreCase))
            {
                return unit.HasShieldFrom(sourceSkillId);
            }
            int stacks = 0;
            for (int index = 0; index < unit.StatusEffects.Count; index++)
            {
                if (unit.StatusEffects[index].Definition.status_effect_id
                    == statusId)
                {
                    stacks += unit.StatusEffects[index].CurrentStacks;
                }
            }
            return stacks >= minimumStacks;
        }

        private static List<UnitBaseModel> ResolveTargets(
            SkillExecutionRequest request,
            List<ChoiceNodeDefinition> graph)
        {
            ChoiceNodeDefinition targetNode = null;
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id == "EffectTarget")
                {
                    targetNode = graph[index];
                    break;
                }
            }
            string side = targetNode?.arg_1;
            string selection = targetNode?.arg_2;
            string anchor = targetNode?.arg_5;
            List<UnitBaseModel> targets = new List<UnitBaseModel>();
            if (side == "Self")
            {
                targets.Add(request.Caster);
                return targets;
            }
            if (selection == "EventTarget")
            {
                if (request.EventTarget != null && request.EventTarget.IsAlive)
                {
                    targets.Add(request.EventTarget);
                }
                return targets;
            }
            if (string.IsNullOrEmpty(side)
                && string.IsNullOrEmpty(selection)
                && (anchor == "AppliedTargets"
                    || request.AppliedTargets.Count > 0))
            {
                for (int index = 0; index < request.AppliedTargets.Count; index++)
                {
                    UnitBaseModel unit = request.AppliedTargets[index];
                    if (unit != null && unit.IsAlive)
                    {
                        targets.Add(unit);
                    }
                }
                return targets;
            }
            bool sourceEnemy = request.Caster is EnemyModel;
            for (int index = 0; index < request.RegisteredUnits.Count; index++)
            {
                UnitBaseModel unit = request.RegisteredUnits[index];
                if (unit == null || !unit.IsAlive || unit is NexusModel) continue;
                bool ally = (unit is EnemyModel) == sourceEnemy;
                if (side == "AllAllies" && ally)
                {
                    targets.Add(unit);
                }
                else if (side == "Enemy" && !ally)
                {
                    targets.Add(unit);
                }
            }
            StableSortByDistance(targets, request.Caster.Position);
            if (targetNode?.arg_3 == "Single")
            {
                if (request.TargetPoint.HasValue)
                {
                    StableSortByDistance(targets, request.TargetPoint.Value);
                }
                if (targets.Count > 1)
                {
                    targets.RemoveRange(1, targets.Count - 1);
                }
            }
            return targets;
        }

        private static void StableSortByDistance(
            List<UnitBaseModel> targets,
            CombatVector2 center)
        {
            for (int index = 1; index < targets.Count; index++)
            {
                UnitBaseModel value = targets[index];
                float distance = (value.Position - center).SqrMagnitude;
                int insertion = index;
                while (insertion > 0
                    && (targets[insertion - 1].Position - center).SqrMagnitude
                        > distance)
                {
                    targets[insertion] = targets[insertion - 1];
                    insertion--;
                }
                targets[insertion] = value;
            }
        }

        private static bool IsExcluded(
            UnitBaseModel caster,
            string excludedChoiceId)
        {
            if (string.IsNullOrEmpty(excludedChoiceId)
                || !(caster is MonsterModel monster))
            {
                return false;
            }
            for (int index = 0;
                index < monster.SkillBucket.SelectedChoices.Count;
                index++)
            {
                if (monster.SkillBucket.SelectedChoices[index].choice_id
                    == excludedChoiceId)
                {
                    return true;
                }
            }
            return false;
        }

        private static float ReadLifetime(
            List<ChoiceNodeDefinition> graph)
        {
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id == "EffectLifetime")
                {
                    return Math.Max(0f, Number(graph[index].arg_1, 0f));
                }
            }
            return 0f;
        }

        private static float ReadTargetDelay(
            List<ChoiceNodeDefinition> graph)
        {
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id == "EffectTarget")
                {
                    return Math.Max(0f, Number(graph[index].arg_7, 0f));
                }
            }
            return 0f;
        }

        private static string ReadEffectTiming(
            List<ChoiceNodeDefinition> graph)
        {
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id == "EffectTarget")
                {
                    return graph[index].arg_6;
                }
            }
            return null;
        }

        private static string ReadRuntimeKindFilter(
            List<ChoiceNodeDefinition> graph)
        {
            for (int index = 0; index < graph.Count; index++)
            {
                if (graph[index].node_type_id == "StatusRuntimeKindFilter")
                {
                    return graph[index].arg_1;
                }
            }
            return null;
        }

        private static float Number(string value, float fallback)
        {
            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float parsed)
                ? parsed
                : fallback;
        }

        private static int Integer(string value, int fallback)
        {
            return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsed)
                ? parsed
                : fallback;
        }

        private static float? NullableNumber(string value)
        {
            return float.TryParse(
                value,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float parsed)
                ? parsed
                : (float?)null;
        }

        private static int? NullableInt(string value)
        {
            return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out int parsed)
                ? parsed
                : (int?)null;
        }
    }
}
