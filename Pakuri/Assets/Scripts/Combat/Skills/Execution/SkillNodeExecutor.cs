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
                RefundCooldown(refund.Value, context);
                return;
            }

            var reload = node.GetOperation<ReduceReloadNodeOp>();
            if (reload.HasValue)
            {
                ReduceReload(reload.Value, context);
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

            var damageSpec = new SkillDamageSpec
            {
                SkillId = context.SourceSkillId,
                Element = operation.Attribute,
                BaseDamage = operation.BaseDamage,
                AttackPowerCoefficient = operation.AttackPowerCoefficient,
                SpellPowerCoefficient = operation.SpellPowerCoefficient,
                CriticalAllowed = false
            };
            var rawDamage = DamageCalculator.CalculateRawDamage(context.Source, damageSpec);
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

            var compatibilityDefinition = new SkillEffectDefinition
            {
                EffectKind = SkillMultiEffectKind.RecastZone,
                RecastSourceSkillId = operation.SourceSkillId,
                DelaySeconds = operation.DelaySeconds,
                RecastDurationSeconds = operation.DurationSeconds,
                RecastRadiusMultiplier = operation.RadiusMultiplier,
                RecastInheritSkillData = operation.InheritSnapshot,
                RecastMaxGeneration = operation.MaxGeneration
            };
            ZoneSkillExecutor.ExecuteRecast(
                context.ExecutionContext,
                context.ExecutionData,
                compatibilityDefinition,
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

        private static void RefundCooldown(RefundCooldownNodeOp operation, SkillActionContext context)
        {
            var runtime = FindSourceRuntime(operation.SkillId, context);
            if (runtime != null)
            {
                runtime.ReduceCooldownRemaining(
                    runtime.EffectiveCooldownDuration * Mathf.Clamp01(operation.Ratio));
            }
        }

        private static void ReduceReload(ReduceReloadNodeOp operation, SkillActionContext context)
        {
            var runtime = FindSourceRuntime(operation.SkillId, context);
            if (runtime != null)
            {
                runtime.ReduceReloadRemaining(
                    runtime.ReloadDuration * Mathf.Clamp01(operation.Ratio));
            }
        }

        private static SkillUseState FindSourceRuntime(string skillId, SkillActionContext context)
        {
            if (context.Source == null
                || context.Source.Skills == null
                || string.IsNullOrWhiteSpace(skillId))
            {
                return null;
            }

            return context.Source.SkillState.FindBySkillId(skillId);
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

            return true;
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
                false);
            internal float DurationSeconds;
            internal bool HasStatusPayload;
            internal StatusPayloadNodeOp StatusPayload;
            internal bool HasVisual;
            internal ShowVisualNodeOp Visual;
            internal readonly List<RequireStatusNodeOp> Requirements = new List<RequireStatusNodeOp>();
        }
    }
}
