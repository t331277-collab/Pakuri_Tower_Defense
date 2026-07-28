using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * Trigger가 선택한 컴파일 Node를 작성 순서대로 실행한다.
     * Phase 1에서는 기존 Effect 경로를 유지하며 실행 데이터용 modifier Node만 전달한다.
     */
    public static class SkillNodeExecutor
    {
        private const int MaxExecutionDepth = 8;

        [ThreadStatic]
        private static int executionDepth;

        internal static bool HasRuntimeActions(IReadOnlyList<SkillNode> nodes)
        {
            if (nodes == null)
            {
                return false;
            }

            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node != null
                    && (node.GetOperation<ApplyDamageNodeOp>().HasValue
                        || node.GetOperation<ApplyStatusNodeOp>().HasValue
                        || node.GetOperation<ApplyShieldNodeOp>().HasValue
                        || node.GetOperation<ExtendStatusDurationNodeOp>().HasValue
                        || node.GetOperation<ShowVisualNodeOp>().HasValue
                        || node.GetOperation<RecastZoneNodeOp>().HasValue
                        || node.GetOperation<ExecuteSkillNodeOp>().HasValue
                        || node.GetOperation<RefundCooldownNodeOp>().HasValue
                        || node.GetOperation<ReduceReloadNodeOp>().HasValue))
                {
                    return true;
                }
            }

            return false;
        }

        public static void Execute(
            IReadOnlyList<SkillNode> nodes,
            SkillActionContext context)
        {
            if (nodes == null || nodes.Count == 0 || context == null)
            {
                return;
            }

            context.ExecutionData?.ApplyNodes(nodes);

            if (executionDepth >= MaxExecutionDepth)
            {
                return;
            }

            executionDepth++;
            try
            {
                var state = BuildState(nodes);
                if (!MeetsGlobalRequirements(context, state))
                {
                    return;
                }
                for (var i = 0; i < nodes.Count; i++)
                {
                    ExecuteNode(nodes[i], context, state);
                }
            }
            finally
            {
                executionDepth--;
            }
        }

        private static NodeExecutionState BuildState(IReadOnlyList<SkillNode> nodes)
        {
            var state = new NodeExecutionState();
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                var targets = node.GetOperation<SelectTargetsNodeOp>();
                if (targets.HasValue)
                {
                    state.Targets = targets.Value;
                }

                var duration = node.GetOperation<SetDurationNodeOp>();
                if (duration.HasValue)
                {
                    state.DurationSeconds = Mathf.Max(0f, duration.Value.DurationSeconds);
                }

                var payload = node.GetOperation<StatusPayloadNodeOp>();
                if (payload.HasValue)
                {
                    state.StatusPayload = payload.Value;
                    state.HasStatusPayload = true;
                }

                var requirement = node.GetOperation<RequireStatusNodeOp>();
                if (requirement.HasValue)
                {
                    state.Requirements.Add(requirement.Value);
                }

                var sourceRequirement = node.GetOperation<SourceStatusRequirementOp>();
                if (sourceRequirement.HasValue)
                {
                    state.SourceRequirements.Add(sourceRequirement.Value);
                }

                var statusCondition = node.GetOperation<StatusConditionNodeOp>();
                if (statusCondition.HasValue)
                {
                    state.StatusConditions.Add(statusCondition.Value);
                }

                var skillAttribute = node.GetOperation<SkillAttributeConditionNodeOp>();
                if (skillAttribute.HasValue)
                {
                    state.SkillAttributes.Add(skillAttribute.Value);
                }

                var healthRatio = node.GetOperation<HealthRatioConditionNodeOp>();
                if (healthRatio.HasValue)
                {
                    state.MaximumHealthRatio = healthRatio.Value.MaximumRatio;
                }

                var hitCount = node.GetOperation<HitCountConditionNodeOp>();
                if (hitCount.HasValue)
                {
                    state.MinimumHitCount = hitCount.Value.MinimumHitCount;
                }

                var statusMutation = node.GetOperation<StatusMutationNodeOp>();
                if (statusMutation.HasValue)
                {
                    state.StatusMutations.Add(statusMutation.Value);
                }

                if (node.GetOperation<ApplyStatusNodeOp>().HasValue
                    || node.GetOperation<ApplyShieldNodeOp>().HasValue)
                {
                    state.HasStatusAction = true;
                }

                var visual = node.GetOperation<ShowVisualNodeOp>();
                if (visual.HasValue)
                {
                    state.Visual = visual.Value;
                    state.HasVisual = true;
                }
            }

            return state;
        }

        private static void ExecuteNode(
            SkillNode node,
            SkillActionContext context,
            NodeExecutionState state)
        {
            if (node == null)
            {
                return;
            }

            var damage = node.GetOperation<ApplyDamageNodeOp>();
            if (damage.HasValue)
            {
                ExecuteDamage(damage.Value, context, state);
                return;
            }

            var status = node.GetOperation<ApplyStatusNodeOp>();
            if (status.HasValue)
            {
                ExecuteStatus(status.Value, context, state);
                return;
            }

            var shield = node.GetOperation<ApplyShieldNodeOp>();
            if (shield.HasValue)
            {
                ExecuteShield(shield.Value, context, state);
                return;
            }

            var extend = node.GetOperation<ExtendStatusDurationNodeOp>();
            if (extend.HasValue)
            {
                ExecuteStatusDurationExtension(extend.Value, context, state);
                return;
            }

            var visual = node.GetOperation<ShowVisualNodeOp>();
            if (visual.HasValue)
            {
                ExecuteVisual(visual.Value, context, state);
                return;
            }

            var recast = node.GetOperation<RecastZoneNodeOp>();
            if (recast.HasValue)
            {
                ExecuteRecast(recast.Value, context);
                return;
            }

            var executeSkill = node.GetOperation<ExecuteSkillNodeOp>();
            if (executeSkill.HasValue)
            {
                ExecuteSkill(executeSkill.Value, context);
                return;
            }

            var refund = node.GetOperation<RefundCooldownNodeOp>();
            if (refund.HasValue)
            {
                RefundCooldown(refund.Value, context, state);
                return;
            }

            var reload = node.GetOperation<ReduceReloadNodeOp>();
            if (reload.HasValue)
            {
                ReduceReload(reload.Value, context, state);
            }
        }

        private static void ExecuteDamage(
            ApplyDamageNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            SkillExecutionContext executionContext = context.ExecutionContext;
            if (executionContext == null
                || executionContext.CombatManager == null
                || context.Source == null)
            {
                return;
            }

            var rawDamage = ResolveRawDamage(operation, context);
            var multiplier = Mathf.Max(0f, operation.DamageMultiplier);
            if (context.ExecutionData != null)
            {
                multiplier *= Mathf.Max(0f, context.ExecutionData.DamageMultiplier);
            }

            var targets = ResolveTargets(context, state, operation.Radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!MatchesRequirements(target.Model, context, state))
                {
                    continue;
                }

                executionContext.CombatManager.ApplyDamage(
                    target.Model,
                    rawDamage,
                    operation.Attribute,
                    context.Source,
                    criticalAllowed: false,
                    0f,
                    0f,
                    context.SourceSkillId,
                    finalDamageMultiplier: multiplier);
            }
        }

        private static float ResolveRawDamage(
            ApplyDamageNodeOp operation,
            SkillActionContext context)
        {
            if (operation.ValueSource == NodeDamageValueSource.Fixed)
            {
                return DamageCalculator.CalculateRawDamage(
                    context.Source,
                    new SkillDamageSpec
                    {
                        SkillId = context.SourceSkillId,
                        Element = operation.Attribute,
                        BaseDamage = operation.BaseDamage,
                        AttackPowerCoefficient = operation.AttackPowerCoefficient,
                        SpellPowerCoefficient = operation.SpellPowerCoefficient,
                        CriticalAllowed = false
                    });
            }

            var value = 0f;
            switch (operation.ValueSource)
            {
                case NodeDamageValueSource.ShieldAppliedAmount:
                    value = context.EventStatus != null
                        ? context.EventStatus.AppliedShieldAmount
                        : 0f;
                    break;
                case NodeDamageValueSource.ShieldRemainingAmount:
                    value = context.EventStatus != null
                        ? context.EventStatus.RemainingShieldAmount
                        : 0f;
                    break;
                case NodeDamageValueSource.ShieldAbsorbedAmount:
                    value = context.ShieldAbsorbedAmount;
                    break;
                case NodeDamageValueSource.TrackedIncomingDamage:
                    value = context.EventStatus != null
                        ? context.EventStatus.GetTrackedIncomingDamage(operation.TrackedAttribute)
                        : 0f;
                    break;
                case NodeDamageValueSource.EventAppliedDamage:
                    value = context.EventDamage;
                    break;
            }
            return Mathf.Max(0f, value) * Mathf.Max(0f, operation.ValueSourceMultiplier);
        }

        private static void ExecuteStatus(
            ApplyStatusNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            SkillExecutionContext executionContext = context.ExecutionContext;
            if (executionContext == null || executionContext.CombatManager == null)
            {
                return;
            }

            var statusKind = operation.StatusKind;
            if (state.HasStatusPayload && state.StatusPayload.StatusKind != StatusEffectKind.None)
            {
                statusKind = state.StatusPayload.StatusKind;
            }
            if (statusKind == StatusEffectKind.None)
            {
                return;
            }

            var spec = SkillStatus.CreateDirectStatusSpec(statusKind, 1, context.ExecutionData);
            if (spec == null)
            {
                return;
            }
            if (state.HasStatusPayload)
            {
                spec.Chance = Mathf.Clamp01(state.StatusPayload.Chance);
                spec.Stacks = Mathf.Max(1, state.StatusPayload.Stacks);
                spec.MaxStacks = Mathf.Max(1, state.StatusPayload.MaxStacks);
                spec.RefreshDuration = state.StatusPayload.RefreshDuration;
                if (state.StatusPayload.DurationSeconds > 0f)
                {
                    spec.DurationSeconds = state.StatusPayload.DurationSeconds;
                    spec.Permanent = false;
                }
            }
            if (state.DurationSeconds > 0f)
            {
                spec.DurationSeconds = state.DurationSeconds;
                spec.Permanent = false;
            }
            if (!string.IsNullOrWhiteSpace(operation.TargetScope))
            {
                spec.StatusData.TargetScope = StatusRuntimeCompiler.ParseTargetScope(operation.TargetScope);
            }
            if (!string.IsNullOrWhiteSpace(operation.MergePolicy))
            {
                spec.StatusData.MergePolicy = StatusRuntimeCompiler.ParseMergePolicy(operation.MergePolicy);
            }
            ApplyStatusMutations(spec.StatusData, state.StatusMutations);
            AttachStatusVisual(spec.StatusData, state);
            spec.StatusData.SourceSkillId = string.IsNullOrWhiteSpace(context.NodeOwnerId)
                ? context.SourceSkillId
                : context.NodeOwnerId;

            var targets = ResolveTargets(context, state, 0f);
            for (var i = 0; i < targets.Count; i++)
            {
                if (MatchesRequirements(targets[i].Model, context, state))
                {
                    StatusCombatRules.ApplyStatus(
                        executionContext.CombatManager,
                        targets[i].Model,
                        spec,
                        context.Source);
                }
            }
        }

        private static void ExecuteShield(
            ApplyShieldNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            var executionContext = context.ExecutionContext;
            if (executionContext == null
                || executionContext.CombatManager == null
                || context.Source == null)
            {
                return;
            }

            var spec = SkillStatus.CreateDirectStatusSpec(
                StatusEffectKind.Shield,
                1,
                context.ExecutionData);
            if (spec == null || spec.StatusData == null)
            {
                return;
            }

            if (state.DurationSeconds > 0f)
            {
                spec.DurationSeconds = state.DurationSeconds;
                spec.Permanent = false;
            }
            ApplyStatusMutations(spec.StatusData, state.StatusMutations);
            AttachStatusVisual(spec.StatusData, state);
            spec.StatusData.SourceSkillId = string.IsNullOrWhiteSpace(context.NodeOwnerId)
                ? context.SourceSkillId
                : context.NodeOwnerId;

            var spellPower = 0f;
            if (context.Source.Stats != null)
            {
                spellPower = context.Source.Stats.SpellPower
                    * StatusCombatRules.SpellPowerMultiplier(context.Source);
            }
            var amount = operation.BaseAmount
                + spellPower * operation.SpellPowerCoefficient;
            if (context.ExecutionData != null)
            {
                amount *= Mathf.Max(0f, context.ExecutionData.ShieldAmountMultiplier);
            }

            var targets = ResolveTargets(context, state, 0f);
            for (var i = 0; i < targets.Count; i++)
            {
                if (!MatchesRequirements(targets[i].Model, context, state))
                {
                    continue;
                }

                executionContext.CombatManager.ApplyShieldStatus(
                    targets[i].Model,
                    spec.StatusData,
                    Mathf.Max(0f, amount),
                    spec.DurationSeconds,
                    spec.Stacks,
                    spec.MaxStacks,
                    spec.Permanent,
                    spec.RefreshDuration,
                    context.Source);
            }
        }

        private static void ExecuteStatusDurationExtension(
            ExtendStatusDurationNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            SkillExecutionContext executionContext = context.ExecutionContext;
            if (executionContext == null
                || executionContext.CombatManager == null
                || state.DurationSeconds <= 0f)
            {
                return;
            }

            var targets = ResolveTargets(context, state, 0f);
            for (var i = 0; i < targets.Count; i++)
            {
                if (MatchesRequirements(targets[i].Model, context, state))
                {
                    executionContext.CombatManager.ExtendStatusDuration(
                        targets[i].Model,
                        operation.StatusKind,
                        state.DurationSeconds);
                }
            }
        }

        private static void ExecuteVisual(
            ShowVisualNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            if (state.HasStatusAction)
            {
                return;
            }

            SkillExecutionContext executionContext = context.ExecutionContext;
            if (executionContext == null
                || executionContext.CombatManager == null
                || executionContext.CombatManager.Effects == null
                || (operation.Prefab == null
                    && (operation.RuntimeVisual == null || !operation.RuntimeVisual.HasVisual())))
            {
                return;
            }

            executionContext.CombatManager.Effects.CreateEffect(new EffectCreateRequest(
                operation.RuntimeVisual,
                operation.Prefab,
                "SkillNodeVisual_" + context.SourceSkillId,
                ResolveCenter(context, state),
                Quaternion.identity,
                null,
                state.DurationSeconds,
                null,
                false,
                true,
                false));
        }

        private static void ExecuteRecast(RecastZoneNodeOp operation, SkillActionContext context)
        {
            if (context.ExecutionContext == null)
            {
                return;
            }

            ZoneSkillExecutor.ExecuteRecast(
                context.ExecutionContext,
                context.ExecutionData,
                operation,
                context.EventCenter);
        }

        private static void ExecuteSkill(ExecuteSkillNodeOp operation, SkillActionContext context)
        {
            SkillExecutionContext executionContext = context.ExecutionContext;
            if (executionContext == null
                || executionContext.CombatManager == null
                || executionContext.CasterEntry == null
                || string.IsNullOrWhiteSpace(operation.SkillId))
            {
                return;
            }

            var runtime = executionContext.CasterEntry.Model.SkillState.FindBySkillId(operation.SkillId);
            if (runtime == null)
            {
                return;
            }

            executionContext.CombatManager.SkillExecution.TryExecuteTriggered(
                executionContext.CasterEntry,
                runtime,
                executionContext.Roster,
                executionContext.CombatManager,
                ResolveCenter(context, new NodeExecutionState()),
                hasTargetPoint: true,
                triggeredDamageMultiplier: Mathf.Max(0f, operation.DamageMultiplier),
                triggerSourceSkillId: context.SourceSkillId);
        }

        private static void RefundCooldown(
            RefundCooldownNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            var runtimes = ResolveRuntimes(operation.SkillId, context, state);
            for (var i = 0; i < runtimes.Count; i++)
            {
                runtimes[i].ReduceCooldownRemaining(
                    runtimes[i].EffectiveCooldownDuration * Mathf.Clamp01(operation.Ratio));
            }
        }

        private static void ReduceReload(
            ReduceReloadNodeOp operation,
            SkillActionContext context,
            NodeExecutionState state)
        {
            var runtimes = ResolveRuntimes(operation.SkillId, context, state);
            for (var i = 0; i < runtimes.Count; i++)
            {
                runtimes[i].ReduceReloadRemaining(
                    runtimes[i].ReloadDuration * Mathf.Clamp01(operation.Ratio));
            }
        }

        private static List<SkillUseState> ResolveRuntimes(
            string skillId,
            SkillActionContext context,
            NodeExecutionState state)
        {
            var runtimes = new List<SkillUseState>();
            var entries = ResolveTargets(context, state, 0f);
            if (state.Targets.TargetSide == SkillMultiEffectTargetSide.Self
                && entries.Count == 0
                && context.ExecutionContext != null
                && context.ExecutionContext.CasterEntry != null)
            {
                entries = new[] { context.ExecutionContext.CasterEntry };
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i] != null ? entries[i].Model : null;
                if (model == null || model.Skills == null)
                {
                    continue;
                }
                if (!string.IsNullOrWhiteSpace(skillId))
                {
                    var runtime = model.SkillState.FindBySkillId(skillId);
                    if (runtime != null)
                    {
                        runtimes.Add(runtime);
                    }
                    continue;
                }
                var activeSkills = model.SkillState.ActiveSkills;
                for (var skillIndex = 0; skillIndex < activeSkills.Count; skillIndex++)
                {
                    if (activeSkills[skillIndex] != null)
                    {
                        runtimes.Add(activeSkills[skillIndex]);
                    }
                }
            }
            return runtimes;
        }

        private static IReadOnlyList<CombatUnitEntry> ResolveTargets(
            SkillActionContext context,
            NodeExecutionState state,
            float radius)
        {
            SkillExecutionContext executionContext = context.ExecutionContext;
            if (executionContext == null
                || executionContext.CasterEntry == null
                || executionContext.Roster == null)
            {
                return Array.Empty<CombatUnitEntry>();
            }

            if (state.Targets.TargetSelection == SkillMultiEffectTargetSelection.EventTarget
                && context.EventTarget != null)
            {
                var eventEntry = executionContext.Roster.Find(context.EventTarget);
                return eventEntry != null
                    ? new[] { eventEntry }
                    : Array.Empty<CombatUnitEntry>();
            }

            var targeting = BuildTargeting(state.Targets, radius);
            var targets = SkillTargeting.OrderedTargets(
                executionContext.CasterEntry,
                executionContext.Roster,
                targeting);
            if (state.Targets.TargetShape == SkillMultiEffectTargetShape.Single && targets.Count > 1)
            {
                return new[] { targets[0] };
            }
            if (state.Targets.MaxTargets > 0 && targets.Count > state.Targets.MaxTargets)
            {
                var limited = new CombatUnitEntry[state.Targets.MaxTargets];
                for (var i = 0; i < limited.Length; i++)
                {
                    limited[i] = targets[i];
                }
                return limited;
            }

            return targets;
        }

        private static SkillTargetingSpec BuildTargeting(SelectTargetsNodeOp operation, float radius)
        {
            var side = SkillTargetSide.Enemy;
            if (operation.TargetSide == SkillMultiEffectTargetSide.Self)
            {
                side = SkillTargetSide.Self;
            }
            else if (operation.TargetSide == SkillMultiEffectTargetSide.AllAllies)
            {
                side = SkillTargetSide.AllAllies;
            }

            return new SkillTargetingSpec
            {
                TargetSide = side,
                Selection = operation.TargetSelection == SkillMultiEffectTargetSelection.Owner
                    ? SkillTargetSelection.Owner
                    : SkillTargetSelection.Nearest,
                Shape = operation.TargetShape == SkillMultiEffectTargetShape.Single
                    ? SkillTargetShape.Single
                    : operation.TargetShape == SkillMultiEffectTargetShape.Battlefield
                        ? SkillTargetShape.Battlefield
                        : SkillTargetShape.Circle,
                Radius = Mathf.Max(0f, radius),
                CoverAll = operation.CoverAll
                    || operation.TargetShape == SkillMultiEffectTargetShape.Battlefield
            };
        }

        private static Vector2 ResolveCenter(
            SkillActionContext context,
            NodeExecutionState state)
        {
            SkillExecutionContext executionContext = context.ExecutionContext;
            if (state.Targets.CenterMode == SkillMultiEffectCenterMode.Caster
                && executionContext != null
                && executionContext.CasterEntry != null
                && executionContext.CasterEntry.Transform != null)
            {
                return executionContext.CasterEntry.Transform.position;
            }

            if (state.Targets.CenterMode == SkillMultiEffectCenterMode.EffectTarget
                && context.EventTarget != null
                && executionContext != null
                && executionContext.Roster != null)
            {
                var target = executionContext.Roster.Find(context.EventTarget);
                if (target != null && target.Transform != null)
                {
                    return target.Transform.position;
                }
            }

            return context.EventCenter;
        }

        private static bool MatchesRequirements(
            UnitCombatState target,
            SkillActionContext context,
            NodeExecutionState state)
        {
            for (var i = 0; i < state.Requirements.Count; i++)
            {
                var requirement = state.Requirements[i];
                var subject = requirement.TargetSide == SkillMultiEffectTargetSide.Self
                    ? context.Source
                    : target;
                if (!SkillRequirement.HasSourceStatus(
                        subject,
                        requirement.StatusKind,
                        requirement.MinimumStacks))
                {
                    return false;
                }
            }

            for (var i = 0; i < state.StatusConditions.Count; i++)
            {
                var condition = state.StatusConditions[i];
                var subject = condition.TargetSide == SkillMultiEffectTargetSide.Self
                    ? context.Source
                    : target;
                if (!StatusConditionRules.MatchesConditionStatus(
                        subject,
                        StatusRuntimeCompiler.ParseConditionStatusExpression(condition.Expression),
                        StatusRuntimeCompiler.ParseIdList(condition.SourceSkillIds)))
                {
                    return false;
                }
            }

            for (var i = 0; i < state.SkillAttributes.Count; i++)
            {
                if (!HasActiveSkillAttribute(target, state.SkillAttributes[i].Attribute))
                {
                    return false;
                }
            }

            if (state.MaximumHealthRatio > 0f)
            {
                if (target == null
                    || target.Resources == null
                    || target.Stats == null
                    || target.Stats.MaxHealth <= 0f
                    || target.Resources.CurrentHealth / target.Stats.MaxHealth
                        > Mathf.Clamp01(state.MaximumHealthRatio))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MeetsGlobalRequirements(
            SkillActionContext context,
            NodeExecutionState state)
        {
            if (context == null)
            {
                return false;
            }
            if (state.MinimumHitCount > 0 && context.HitCount < state.MinimumHitCount)
            {
                return false;
            }
            for (var i = 0; i < state.SourceRequirements.Count; i++)
            {
                var requirement = state.SourceRequirements[i].Condition;
                if (!SkillRequirement.HasSourceStatus(
                        context.Source,
                        requirement.StatusKind,
                        requirement.MinimumStacks))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool HasActiveSkillAttribute(
            UnitCombatState target,
            DamageAttribute attribute)
        {
            if (target == null || target.Skills == null)
            {
                return false;
            }

            var activeSkills = target.SkillState.ActiveSkills;
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var activeSkill = activeSkills[i];
                if (activeSkill != null
                    && activeSkill.Data != null
                    && activeSkill.Data.Element == attribute)
                {
                    return true;
                }
            }
            return false;
        }

        private static void AttachStatusVisual(
            StatusRuntimeData statusData,
            NodeExecutionState state)
        {
            if (statusData == null || !state.HasVisual)
            {
                return;
            }

            if (state.Visual.Prefab != null)
            {
                statusData.StatusEffectPrefab = state.Visual.Prefab;
            }
            if (state.Visual.RuntimeVisual != null)
            {
                statusData.RuntimeVisual = state.Visual.RuntimeVisual;
            }
        }

        private static void ApplyStatusMutations(
            StatusRuntimeData statusData,
            IReadOnlyList<StatusMutationNodeOp> mutations)
        {
            if (statusData == null || mutations == null)
            {
                return;
            }

            for (var i = 0; i < mutations.Count; i++)
            {
                var mutation = mutations[i];
                switch (mutation.Kind)
                {
                    case StatusMutationKind.ActionSpeedBonus:
                        statusData.Modifiers.ActionSpeedBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.MoveSpeedBonus:
                        statusData.MoveSpeedBonus += mutation.Amount;
                        statusData.MovementSlowRate = statusData.MoveSpeedBonus < 0f
                            ? -statusData.MoveSpeedBonus
                            : 0f;
                        break;
                    case StatusMutationKind.AttackPowerBonus:
                        statusData.Modifiers.AttackPowerBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.SpellPowerBonus:
                        statusData.Modifiers.SpellPowerBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.DamageBonusRate:
                        statusData.Modifiers.DamageBonusRate += mutation.Amount;
                        SetElementModifier(statusData, mutation);
                        break;
                    case StatusMutationKind.ShieldReceivedBonus:
                        statusData.Modifiers.ShieldReceivedBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.CriticalChanceBonus:
                        statusData.Modifiers.CritChanceBonusRate += mutation.Amount;
                        break;
                    case StatusMutationKind.CriticalDamageBonus:
                        statusData.Modifiers.CritDamageBonusRate += mutation.Amount;
                        break;
                    case StatusMutationKind.CriticalResistanceBonus:
                        statusData.CriticalResistanceBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.DamageTakenBonus:
                        statusData.DamageTakenBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.ElementResistReduction:
                        statusData.ElementResistReduction += mutation.Amount;
                        statusData.Modifiers.ResistReduction = statusData.ElementResistReduction;
                        statusData.Modifiers.ResistReductionElement = mutation.Attribute;
                        SetElementModifier(statusData, mutation);
                        break;
                    case StatusMutationKind.FlatElementResistReduction:
                        statusData.FlatElementResistReduction += mutation.Amount;
                        SetElementModifier(statusData, mutation);
                        break;
                    case StatusMutationKind.ElementDamageTakenBonus:
                        statusData.ElementDamageTakenBonus += mutation.Amount;
                        SetElementModifier(statusData, mutation);
                        break;
                    case StatusMutationKind.ConditionalStatusChanceBonus:
                        statusData.ConditionalTargetStatusKinds =
                            StatusRuntimeCompiler.ParseStatusKinds(mutation.ReferenceId);
                        statusData.ConditionalStatusChanceBonus += mutation.Amount;
                        break;
                    case StatusMutationKind.RuntimeKindFilter:
                        ApplyRuntimeKindFilter(statusData, mutation.ReferenceId);
                        break;
                    case StatusMutationKind.OutgoingAdditionalDamage:
                        statusData.OutgoingAdditionalDamageMultiplier += mutation.Amount;
                        statusData.OutgoingAdditionalDamageTriggerAttribute = mutation.Attribute;
                        statusData.OutgoingAdditionalDamageAttribute = mutation.SecondaryAttribute;
                        break;
                }
            }
        }

        private static void SetElementModifier(
            StatusRuntimeData statusData,
            StatusMutationNodeOp mutation)
        {
            statusData.HasElementModifierTarget = true;
            statusData.ElementModifierTarget = mutation.Attribute;
        }

        private static void ApplyRuntimeKindFilter(
            StatusRuntimeData statusData,
            string rawValue)
        {
            var separator = rawValue == null ? -1 : rawValue.IndexOf('|');
            var incoming = separator < 0 ? rawValue : rawValue.Substring(0, separator);
            var outgoing = separator < 0 ? string.Empty : rawValue.Substring(separator + 1);
            statusData.ConditionalIncomingSkillRuntimeKinds = incoming ?? string.Empty;
            statusData.ConditionalIncomingSkillRuntimeKindValues =
                StatusRuntimeCompiler.ParseSkillRuntimeKindConditions(incoming);
            statusData.ConditionalOutgoingSkillRuntimeKinds = outgoing ?? string.Empty;
            statusData.ConditionalOutgoingSkillRuntimeKindValues =
                StatusRuntimeCompiler.ParseSkillRuntimeKindConditions(outgoing);
        }

        private sealed class NodeExecutionState
        {
            internal SelectTargetsNodeOp Targets = new SelectTargetsNodeOp(
                SkillMultiEffectTargetSide.Enemy,
                SkillMultiEffectTargetSelection.Nearest,
                SkillMultiEffectTargetShape.Single,
                SkillMultiEffectCenterMode.PrimarySkillCenter,
                SkillMultiEffectVisualAnchorMode.Center,
                false,
                false,
                0);
            internal float DurationSeconds;
            internal bool HasStatusPayload;
            internal StatusPayloadNodeOp StatusPayload;
            internal bool HasVisual;
            internal ShowVisualNodeOp Visual;
            internal bool HasStatusAction;
            internal int MinimumHitCount;
            internal float MaximumHealthRatio;
            internal readonly List<RequireStatusNodeOp> Requirements = new List<RequireStatusNodeOp>();
            internal readonly List<SourceStatusRequirementOp> SourceRequirements = new List<SourceStatusRequirementOp>();
            internal readonly List<StatusConditionNodeOp> StatusConditions = new List<StatusConditionNodeOp>();
            internal readonly List<SkillAttributeConditionNodeOp> SkillAttributes = new List<SkillAttributeConditionNodeOp>();
            internal readonly List<StatusMutationNodeOp> StatusMutations = new List<StatusMutationNodeOp>();
        }
    }
}
