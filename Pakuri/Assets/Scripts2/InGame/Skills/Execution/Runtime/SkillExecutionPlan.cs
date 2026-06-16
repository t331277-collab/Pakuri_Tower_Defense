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
            DamageModifierOp? damageModifier = null,
            CritModifierOp? critModifier = null,
            KillActionOp? killAction = null)
        {
            Kind = kind;
            AuthoringSource = authoringSource;
            RowId = rowId ?? string.Empty;
            CastCondition = castCondition;
            DamageModifier = damageModifier;
            CritModifier = critModifier;
            KillAction = killAction;
        }

        public SkillExecutionPlanNodeKind Kind { get; }
        public SkillExecutionPlanAuthoringSource AuthoringSource { get; }
        public string RowId { get; }
        public CastConditionOp? CastCondition { get; }
        public DamageModifierOp? DamageModifier { get; }
        public CritModifierOp? CritModifier { get; }
        public KillActionOp? KillAction { get; }

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
            DamageModifiers = CopyOps(damageModifiers, Nodes, node => node.DamageModifier);
            CritModifiers = CopyOps(critModifiers, Nodes, node => node.CritModifier);
            KillActions = CopyOps(killActions, Nodes, node => node.KillAction);
        }

        public SkillData Source { get; }
        public string SkillId { get; }
        public IReadOnlyList<SkillExecutionPlanNode> Nodes { get; }
        public IReadOnlyList<CastConditionOp> CastConditions { get; }
        public IReadOnlyList<DamageModifierOp> DamageModifiers { get; }
        public IReadOnlyList<CritModifierOp> CritModifiers { get; }
        public IReadOnlyList<KillActionOp> KillActions { get; }

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
            return new SkillExecutionPlan(
                source,
                snapshot != null ? snapshot.SkillId : (source != null ? source.SkillId : string.Empty),
                snapshot != null ? snapshot.CastConditionOps : null,
                snapshot != null ? snapshot.DamageModifierOps : null,
                snapshot != null ? snapshot.CritModifierOps : null,
                snapshot != null ? snapshot.KillActionOps : null,
                normalizedRows);
        }
    }
}
