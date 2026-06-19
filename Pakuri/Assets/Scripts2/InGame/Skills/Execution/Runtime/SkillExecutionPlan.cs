using Pakuri.Data;
using System.Collections.Generic;

namespace Pakuri.InGame
{
    public enum SkillExecutionPlanAuthoringSource
    {
        LegacyWideColumn,
        NormalizedRow
    }

    public enum SkillExecutionPlanNodeKind
    {
        SkillBase,
        CastCondition,
        Action,
        DamageModifier,
        CritModifier,
        OnHitAction,
        OnKillAction,
        OnExpireAction,
        Trigger,
        Visual
    }

    public sealed class SkillExecutionPlanNode
    {
        public SkillExecutionPlanNode(
            SkillExecutionPlanNodeKind kind,
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            CastConditionOp? castCondition = null,
            SkillActionOp? action = null,
            DamageModifierOp? damageModifier = null,
            CritModifierOp? critModifier = null,
            KillActionOp? killAction = null,
            SkillEffectAction effectAction = null,
            SkillTriggerAction triggerAction = null)
        {
            Kind = kind;
            AuthoringSource = authoringSource;
            RowId = rowId ?? string.Empty;
            CastCondition = castCondition;
            Action = action;
            DamageModifier = damageModifier;
            CritModifier = critModifier;
            KillAction = killAction;
            EffectAction = effectAction;
            TriggerAction = triggerAction;
        }

        public SkillExecutionPlanNodeKind Kind { get; }
        public SkillExecutionPlanAuthoringSource AuthoringSource { get; }
        public string RowId { get; }
        public CastConditionOp? CastCondition { get; }
        public SkillActionOp? Action { get; }
        public DamageModifierOp? DamageModifier { get; }
        public CritModifierOp? CritModifier { get; }
        public KillActionOp? KillAction { get; }
        public SkillEffectAction EffectAction { get; }
        public SkillTriggerAction TriggerAction { get; }

        public static SkillExecutionPlanNode FromCastCondition(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            CastConditionOp op)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.CastCondition,
                authoringSource,
                rowId,
                castCondition: op);
        }

        public static SkillExecutionPlanNode FromDamageModifier(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            DamageModifierOp op)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.DamageModifier,
                authoringSource,
                rowId,
                damageModifier: op);
        }

        public static SkillExecutionPlanNode FromAction(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            SkillActionOp op)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.Action,
                authoringSource,
                rowId,
                action: op);
        }

        public static SkillExecutionPlanNode FromCritModifier(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            CritModifierOp op)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.CritModifier,
                authoringSource,
                rowId,
                critModifier: op);
        }

        public static SkillExecutionPlanNode FromKillAction(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            KillActionOp op)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.OnKillAction,
                authoringSource,
                rowId,
                killAction: op);
        }

        public static SkillExecutionPlanNode FromEffect(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            SkillEffectDefinition effect)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.Action,
                authoringSource,
                rowId,
                effectAction: new SkillEffectAction(effect));
        }

        public static SkillExecutionPlanNode FromTrigger(
            SkillExecutionPlanAuthoringSource authoringSource,
            string rowId,
            SkillTriggerDefinition trigger)
        {
            return new SkillExecutionPlanNode(
                SkillExecutionPlanNodeKind.Trigger,
                authoringSource,
                rowId,
                triggerAction: new SkillTriggerAction(trigger));
        }
    }

    public sealed class SkillEffectAction
    {
        public SkillEffectAction(SkillEffectDefinition definition)
        {
            Definition = definition;
            EffectId = definition != null ? definition.EffectId : string.Empty;
            Kind = definition != null ? definition.EffectKind : default;
            Timing = definition != null ? definition.EffectTiming : default;
        }

        internal SkillEffectDefinition Definition { get; }
        public string EffectId { get; }
        public SkillMultiEffectKind Kind { get; }
        public SkillMultiEffectTiming Timing { get; }
    }

    public sealed class SkillTriggerAction
    {
        public SkillTriggerAction(SkillTriggerDefinition definition)
        {
            Definition = definition;
            TriggerId = definition != null ? definition.TriggerId : string.Empty;
            Event = definition != null ? definition.TriggerEvent : default;
            ActionKind = definition != null ? definition.TriggerAction : default;
        }

        internal SkillTriggerDefinition Definition { get; }
        public string TriggerId { get; }
        public SkillTriggerEvent Event { get; }
        public SkillTriggerActionKind ActionKind { get; }
    }

    public sealed class SkillExecutionPlan
    {
        public SkillExecutionPlan(
            SkillData source,
            string skillId,
            IReadOnlyList<CastConditionOp> castConditions,
            IReadOnlyList<DamageModifierOp> damageModifiers,
            IReadOnlyList<CritModifierOp> critModifiers,
            IReadOnlyList<KillActionOp> killActions,
            IReadOnlyList<SkillExecutionPlanNode> nodes = null)
        {
            Source = source;
            SkillId = skillId ?? string.Empty;
            Nodes = Copy(nodes);
            CastConditions = CopyOps(castConditions, Nodes, node => node.CastCondition);
            Actions = CopyOps<SkillActionOp>(null, Nodes, node => node.Action);
            DamageModifiers = CopyOps(damageModifiers, Nodes, node => node.DamageModifier);
            CritModifiers = CopyOps(critModifiers, Nodes, node => node.CritModifier);
            KillActions = CopyOps(killActions, Nodes, node => node.KillAction);
            EffectActions = CopyNodeReferences(Nodes, node => node.EffectAction);
            TriggerActions = CopyNodeReferences(Nodes, node => node.TriggerAction);
        }

        public SkillData Source { get; }
        public string SkillId { get; }
        public IReadOnlyList<SkillExecutionPlanNode> Nodes { get; }
        public IReadOnlyList<CastConditionOp> CastConditions { get; }
        public IReadOnlyList<SkillActionOp> Actions { get; }
        public IReadOnlyList<DamageModifierOp> DamageModifiers { get; }
        public IReadOnlyList<CritModifierOp> CritModifiers { get; }
        public IReadOnlyList<KillActionOp> KillActions { get; }
        public IReadOnlyList<SkillEffectAction> EffectActions { get; }
        public IReadOnlyList<SkillTriggerAction> TriggerActions { get; }

        private static T[] Copy<T>(IReadOnlyList<T> source)
        {
            if (source == null || source.Count == 0)
            {
                return new T[0];
            }

            var copy = new T[source.Count];
            for (var i = 0; i < source.Count; i++)
            {
                copy[i] = source[i];
            }

            return copy;
        }

        private static T[] CopyOps<T>(
            IReadOnlyList<T> legacyOps,
            IReadOnlyList<SkillExecutionPlanNode> nodes,
            System.Func<SkillExecutionPlanNode, T?> selector)
            where T : struct
        {
            var legacyCount = legacyOps != null ? legacyOps.Count : 0;
            var nodeCount = CountNodeOps(nodes, selector);
            if (legacyCount + nodeCount == 0)
            {
                return new T[0];
            }

            var copy = new T[legacyCount + nodeCount];
            for (var i = 0; i < legacyCount; i++)
            {
                copy[i] = legacyOps[i];
            }

            var index = legacyCount;
            if (nodes != null)
            {
                for (var i = 0; i < nodes.Count; i++)
                {
                    var value = selector(nodes[i]);
                    if (value.HasValue)
                    {
                        copy[index] = value.Value;
                        index++;
                    }
                }
            }

            return copy;
        }

        private static int CountNodeOps<T>(
            IReadOnlyList<SkillExecutionPlanNode> nodes,
            System.Func<SkillExecutionPlanNode, T?> selector)
            where T : struct
        {
            if (nodes == null || nodes.Count == 0)
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (selector(nodes[i]).HasValue)
                {
                    count++;
                }
            }

            return count;
        }

        private static T[] CopyNodeReferences<T>(
            IReadOnlyList<SkillExecutionPlanNode> nodes,
            System.Func<SkillExecutionPlanNode, T> selector)
            where T : class
        {
            if (nodes == null || nodes.Count == 0)
            {
                return new T[0];
            }

            var count = 0;
            for (var i = 0; i < nodes.Count; i++)
            {
                if (selector(nodes[i]) != null)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                return new T[0];
            }

            var copy = new T[count];
            var index = 0;
            for (var i = 0; i < nodes.Count; i++)
            {
                var value = selector(nodes[i]);
                if (value != null)
                {
                    copy[index] = value;
                    index++;
                }
            }

            return copy;
        }
    }

    public static class SkillExecutionPlanCompiler
    {
        public static SkillExecutionPlan Compile(SkillData source, SkillExecutionSnapshot snapshot)
        {
            return Compile(source, snapshot, null);
        }

        public static SkillExecutionPlan Compile(
            SkillData source,
            SkillExecutionSnapshot snapshot,
            IReadOnlyList<SkillExecutionPlanNode> normalizedRows)
        {
            var nodes = BuildPlanNodes(source, normalizedRows);
            return new SkillExecutionPlan(
                source,
                snapshot != null ? snapshot.SkillId : (source != null ? source.SkillId : string.Empty),
                snapshot != null ? snapshot.CastConditionOps : null,
                snapshot != null ? snapshot.DamageModifierOps : null,
                snapshot != null ? snapshot.CritModifierOps : null,
                snapshot != null ? snapshot.KillActionOps : null,
                nodes);
        }

        private static IReadOnlyList<SkillExecutionPlanNode> BuildPlanNodes(
            SkillData source,
            IReadOnlyList<SkillExecutionPlanNode> normalizedRows)
        {
            var effectCount = source != null && source.MultiEffects != null ? source.MultiEffects.Length : 0;
            var triggerCount = source != null && source.SkillTriggers != null ? source.SkillTriggers.Length : 0;
            var normalizedCount = normalizedRows != null ? normalizedRows.Count : 0;
            if (effectCount + triggerCount + normalizedCount == 0)
            {
                return normalizedRows;
            }

            var nodes = new List<SkillExecutionPlanNode>(effectCount + triggerCount + normalizedCount);
            for (var i = 0; i < effectCount; i++)
            {
                var effect = source.MultiEffects[i];
                if (effect != null)
                {
                    nodes.Add(SkillExecutionPlanNode.FromEffect(
                        SkillExecutionPlanAuthoringSource.LegacyWideColumn,
                        effect.EffectId,
                        effect));
                }
            }

            for (var i = 0; i < triggerCount; i++)
            {
                var trigger = source.SkillTriggers[i];
                if (trigger != null)
                {
                    nodes.Add(SkillExecutionPlanNode.FromTrigger(
                        SkillExecutionPlanAuthoringSource.LegacyWideColumn,
                        trigger.TriggerId,
                        trigger));
                }
            }

            if (normalizedRows != null)
            {
                for (var i = 0; i < normalizedRows.Count; i++)
                {
                    if (normalizedRows[i] != null)
                    {
                        nodes.Add(normalizedRows[i]);
                    }
                }
            }

            return nodes;
        }
    }
}
