using Pakuri.Data;
using System.Collections.Generic;

/*
 * 스킬 한 번의 실행에 사용할 조건, 수치 변경, 행동, 효과, Trigger 노드를 정의한다.
 * SkillRuntimeData와 현재 Choice Snapshot을 정규화된 노드 순서로 컴파일해
 * Executor와 Trigger Runtime이 같은 실행 계획을 사용하도록 한다.
 */
namespace Pakuri.InGame
{
    public enum SkillExecutionPlanAuthoringSource
    {
        LegacyWideColumn,
        NormalizedRow
    }

    /*
     * 스킬 실행 계획 노드 종류에서 사용하는 선택 값을 정의한다.
     */
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

    /*
     * 스킬 실행 계획에 포함될 조건, 보정, 행동 노드를 보관한다.
     */
    public sealed class SkillExecutionPlanNode
    {
        /*
         * 스킬 실행 계획 노드에 필요한 값을 초기화한다.
         */
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

        /*
         * 시전 조건을 실행 계획 노드로 변환한다.
         */
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

        /*
         * 피해 보정값을 실행 계획 노드로 변환한다.
         */
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

        /*
         * 행동을 실행 계획 노드로 변환한다.
         */
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

        /*
         * 치명타 보정값을 실행 계획 노드로 변환한다.
         */
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

        /*
         * 처치 행동을 실행 계획 노드로 변환한다.
         */
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

        /*
         * 효과를 실행 계획 노드로 변환한다.
         */
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

        /*
         * 트리거를 실행 계획 노드로 변환한다.
         */
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

    /*
     * 스킬 효과 행동에 필요한 값을 보관한다.
     */
    public sealed class SkillEffectAction
    {
        /*
         * 스킬 효과 행동에 필요한 값을 초기화한다.
         */
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

    /*
     * 스킬 트리거 행동에 필요한 값을 보관한다.
     */
    public sealed class SkillTriggerAction
    {
        /*
         * 스킬 트리거 행동에 필요한 값을 초기화한다.
         */
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

    /*
     * 스킬 실행에 사용할 조건, 보정, 행동 목록을 보관한다.
     */
    public sealed class SkillExecutionPlan
    {
        /*
         * 스킬 실행 계획에 필요한 값을 초기화한다.
         */
        public SkillExecutionPlan(
            SkillRuntimeData source,
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

        public SkillRuntimeData Source { get; }
        public string SkillId { get; }
        public IReadOnlyList<SkillExecutionPlanNode> Nodes { get; }
        public IReadOnlyList<CastConditionOp> CastConditions { get; }
        public IReadOnlyList<SkillActionOp> Actions { get; }
        public IReadOnlyList<DamageModifierOp> DamageModifiers { get; }
        public IReadOnlyList<CritModifierOp> CritModifiers { get; }
        public IReadOnlyList<KillActionOp> KillActions { get; }
        public IReadOnlyList<SkillEffectAction> EffectActions { get; }
        public IReadOnlyList<SkillTriggerAction> TriggerActions { get; }

        /*
         * 실행 계획 목록을 새 배열로 복사한다.
         */
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

        /*
         * 규칙을 복사한다.
         */
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

        /*
         * 노드 규칙을 개수를 계산한다.
         */
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

        /*
         * 노드 참조를 복사한다.
         */
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

    /*
     * 스킬 정의와 선택지 노드를 하나의 실행 계획으로 조합한다.
     */
    public static class SkillExecutionPlanCompiler
    {
        /*
         * 스킬과 현재 선택지 상태를 하나의 실행 계획으로 조합한다.
         */
        public static SkillExecutionPlan Compile(SkillRuntimeData source, SkillExecutionSnapshot snapshot)
        {
            return Compile(source, snapshot, null);
        }

        /*
         * 스킬과 현재 선택지 상태를 하나의 실행 계획으로 조합한다.
         */
        public static SkillExecutionPlan Compile(
            SkillRuntimeData source,
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

        /*
         * 계획 노드를 구성한다.
         */
        private static IReadOnlyList<SkillExecutionPlanNode> BuildPlanNodes(
            SkillRuntimeData source,
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
