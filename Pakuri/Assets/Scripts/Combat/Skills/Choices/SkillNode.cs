using System;
using Pakuri.Data;
using System.Collections.Generic;

/*
 * 스킬 한 번의 실행에 사용할 조건, 수치 변경, 행동, 효과, Trigger 노드를 정의한다.
 * SkillRuntimeData와 현재 Choice Snapshot을 정규화된 노드 순서로 컴파일해
 * Executor와 SkillTrigger가 같은 실행 계획을 사용하도록 한다.
 */
namespace Pakuri.InGame
{
    public enum CastConditionOpKind
    {
        TargetHealthRatioBonus
    }

    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
    }

    public enum CritModifierOpKind
    {
        ExecuteChanceBonus
    }

    public enum KillActionOpKind
    {
        CooldownReset,
        CooldownRefundBonus
    }

    public enum SkillActionOpKind
    {
        DamageMultiplier,
        ShieldAmountMultiplier,
        CountStatusDamageMultiplier,
        CooldownMultiplier,
        MagazineBonus,
        ReloadTimeMultiplier,
        PierceBonus,
        RadiusMultiplier,
        RadiusBonus,
        DurationBonus,
        DurationMultiplier,
        DamageDelayMultiplier,
        AdditionalProjectileBonus,
        ShotIntervalMultiplier,
        ConsecutiveHitDamageBonus,
        BranchDamage,
        StatusStackAmountBonus,
        StatusStackAmountSet,
        StatusMaxStacksBonus,
        ConditionalDamageMultiplier,
        TargetStatusStackDamageRateBonus,
        TriggerProcChanceBonus,
        HitTargetCountBonus,
        StatusActionSpeedBonus,
        StatusAttackPowerBonus,
        StatusAilmentResistanceBonus,
        StatusDamageBonusRate,
        StatusShieldReceivedBonus,
        StatusCriticalChanceBonus,
        StatusDamageTakenBonus,
        StatusFlatElementResistReduction,
        StatusDurationBonus,
        StatusConditionalDamageTakenBonus,
        StatusElementDamageTakenBonus,
        StatusCriticalDamageTakenBonus
    }

    /*
     * 노드가 사용하는 조건과 수치 변경 값을 보관한다.
     */
    public readonly struct CastConditionOp
    {
        public CastConditionOp(CastConditionOpKind kind, float value)
        {
            Kind = kind;
            Value = value;
        }

        public CastConditionOpKind Kind { get; }
        public float Value { get; }
    }

    public readonly struct DamageModifierOp
    {
        public DamageModifierOp(DamageModifierOpKind kind, float multiplier)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    public readonly struct CritModifierOp
    {
        public CritModifierOp(CritModifierOpKind kind, float chanceBonus)
        {
            Kind = kind;
            ChanceBonus = chanceBonus;
        }

        public CritModifierOpKind Kind { get; }
        public float ChanceBonus { get; }
    }

    public readonly struct KillActionOp
    {
        public KillActionOp(KillActionOpKind kind, float ratioBonus, bool requiresExecute)
        {
            Kind = kind;
            RatioBonus = ratioBonus;
            RequiresExecute = requiresExecute;
        }

        public KillActionOpKind Kind { get; }
        public float RatioBonus { get; }
        public bool RequiresExecute { get; }
    }

    public readonly struct SkillActionOp
    {
        public SkillActionOp(
            SkillActionOpKind kind,
            float floatValue = 0f,
            int intValue = 0,
            string stringValue = null,
            string secondaryStringValue = null,
            SkillMultiEffectTargetSide targetSide = SkillMultiEffectTargetSide.Enemy,
            float secondaryFloatValue = 0f,
            float thirdFloatValue = 0f,
            StatusEffectKind statusKind = StatusEffectKind.None)
        {
            Kind = kind;
            FloatValue = floatValue;
            IntValue = intValue;
            StringValue = stringValue ?? string.Empty;
            SecondaryStringValue = secondaryStringValue ?? string.Empty;
            TargetSide = targetSide;
            SecondaryFloatValue = secondaryFloatValue;
            ThirdFloatValue = thirdFloatValue;
            StatusKind = statusKind;
        }

        public SkillActionOpKind Kind { get; }
        public float FloatValue { get; }
        public int IntValue { get; }
        public string StringValue { get; }
        public string SecondaryStringValue { get; }
        public SkillMultiEffectTargetSide TargetSide { get; }
        public float SecondaryFloatValue { get; }
        public float ThirdFloatValue { get; }
        public StatusEffectKind StatusKind { get; }
    }

    /*
     * 스킬 실행 계획 노드 종류에서 사용하는 선택 값을 정의한다.
     */
    public enum SkillNodeKind
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
    public sealed class SkillNode
    {
        /*
         * 스킬 실행 계획 노드에 필요한 값을 초기화한다.
         */
        public SkillNode(
            CastConditionOp? castCondition = null,
            SkillActionOp? action = null,
            DamageModifierOp? damageModifier = null,
            CritModifierOp? critModifier = null,
            KillActionOp? killAction = null,
            SkillEffectAction effectAction = null,
            SkillTriggerAction triggerAction = null)
        {
            CastCondition = castCondition;
            Action = action;
            DamageModifier = damageModifier;
            CritModifier = critModifier;
            KillAction = killAction;
            EffectAction = effectAction;
            TriggerAction = triggerAction;
        }

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
        public static SkillNode FromCastCondition(
            CastConditionOp op)
        {
            return new SkillNode(
                castCondition: op);
        }

        /*
         * 피해 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromDamageModifier(
            DamageModifierOp op)
        {
            return new SkillNode(
                damageModifier: op);
        }

        /*
         * 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromAction(
            SkillActionOp op)
        {
            return new SkillNode(
                action: op);
        }

        /*
         * 치명타 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCritModifier(
            CritModifierOp op)
        {
            return new SkillNode(
                critModifier: op);
        }

        /*
         * 처치 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromKillAction(
            KillActionOp op)
        {
            return new SkillNode(
                killAction: op);
        }

        /*
         * 효과를 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromEffect(
            SkillEffectDefinition effect)
        {
            return new SkillNode(
                effectAction: new SkillEffectAction(effect));
        }

        /*
         * 트리거를 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromTrigger(
            SkillTriggerDefinition trigger)
        {
            return new SkillNode(
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
        }

        internal SkillEffectDefinition Definition { get; }
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
        }

        internal SkillTriggerDefinition Definition { get; }
    }

    /*
     * 스킬 실행에 사용할 조건, 보정, 행동 목록을 보관한다.
     */
    public sealed class SkillNodePlan
    {
        /*
         * 스킬 실행 계획에 필요한 값을 초기화한다.
         */
        public SkillNodePlan(
            IReadOnlyList<CastConditionOp> castConditions,
            IReadOnlyList<DamageModifierOp> damageModifiers,
            IReadOnlyList<CritModifierOp> critModifiers,
            IReadOnlyList<KillActionOp> killActions,
            IReadOnlyList<SkillNode> nodes = null)
        {
            var copiedNodes = Copy(nodes);
            CastConditions = CopyOps(castConditions, copiedNodes, node => node.CastCondition);
            DamageModifiers = CopyOps(damageModifiers, copiedNodes, node => node.DamageModifier);
            CritModifiers = CopyOps(critModifiers, copiedNodes, node => node.CritModifier);
            KillActions = CopyOps(killActions, copiedNodes, node => node.KillAction);
            EffectActions = CopyNodeReferences(copiedNodes, node => node.EffectAction);
            TriggerActions = CopyNodeReferences(copiedNodes, node => node.TriggerAction);
        }

        public IReadOnlyList<CastConditionOp> CastConditions { get; }
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
            IReadOnlyList<SkillNode> nodes,
            System.Func<SkillNode, T?> selector)
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
            IReadOnlyList<SkillNode> nodes,
            System.Func<SkillNode, T?> selector)
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
            IReadOnlyList<SkillNode> nodes,
            System.Func<SkillNode, T> selector)
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
    public static class SkillNodeCompiler
    {
        /*
         * 스킬과 현재 선택지 상태를 하나의 실행 계획으로 조합한다.
         */
        public static SkillNodePlan Compile(
            SkillRuntimeData source,
            SkillSnapshot snapshot,
            IReadOnlyList<SkillNode> normalizedRows)
        {
            var nodes = BuildPlanNodes(source, normalizedRows);
            return new SkillNodePlan(
                snapshot != null ? snapshot.CastConditionOps : null,
                snapshot != null ? snapshot.DamageModifierOps : null,
                snapshot != null ? snapshot.CritModifierOps : null,
                snapshot != null ? snapshot.KillActionOps : null,
                nodes);
        }

        /*
         * 계획 노드를 구성한다.
         */
        private static IReadOnlyList<SkillNode> BuildPlanNodes(
            SkillRuntimeData source,
            IReadOnlyList<SkillNode> normalizedRows)
        {
            var effectCount = source != null && source.MultiEffects != null ? source.MultiEffects.Length : 0;
            var triggerCount = source != null && source.SkillTriggers != null ? source.SkillTriggers.Length : 0;
            var normalizedCount = normalizedRows != null ? normalizedRows.Count : 0;
            if (effectCount + triggerCount + normalizedCount == 0)
            {
                return normalizedRows;
            }

            var nodes = new List<SkillNode>(effectCount + triggerCount + normalizedCount);
            for (var i = 0; i < effectCount; i++)
            {
                var effect = source.MultiEffects[i];
                if (effect != null)
                {
                    nodes.Add(SkillNode.FromEffect(
                        effect));
                }
            }

            for (var i = 0; i < triggerCount; i++)
            {
                var trigger = source.SkillTriggers[i];
                if (trigger != null)
                {
                    nodes.Add(SkillNode.FromTrigger(
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

    internal static class SkillNodeAction
    {
        /*
         * 효과를 실행한다.
         */
        public static bool ExecuteEffect(
            SkillExecutionContext context,
            SkillSnapshot snapshot,
            SkillEffectDefinition effect,
            UnityEngine.Vector2 fallbackCenter,
            bool scaleStatusDurationWithSnapshot = false)
        {
            if (effect == null || context == null || context.CombatManager == null || context.CasterEntry == null || context.Roster == null)
            {
                return false;
            }

            switch (effect.EffectKind)
            {
                case SkillMultiEffectKind.Damage:
                    return SkillEffect.ExecuteDamageEffectAction(context, snapshot, effect, fallbackCenter);
                case SkillMultiEffectKind.Status:
                    return SkillEffect.ExecuteStatusEffectAction(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot);
                case SkillMultiEffectKind.ExtendStatusDuration:
                    return SkillEffect.ExecuteExtendStatusDurationEffectAction(context, effect);
                case SkillMultiEffectKind.RecastZone:
                    return ZoneSkillExecutor.ExecuteRecast(context, snapshot, effect, fallbackCenter);
            }

            return false;
        }

        /*
         * 트리거 행동을 실행한다.
         */
        public static bool ExecuteTriggerAction(
            InGameCombatManager combatManager,
            CombatUnitRegistry roster,
            CombatUnitEntry sourceEntry,
            UnitCombatState source,
            SkillTriggerDefinition trigger,
            SkillTrigger.TriggerExecutionContext triggerContext)
        {
            switch (ResolveTriggerAction(trigger))
            {
                case SkillTriggerActionKind.SingleAttack:
                    return SkillTrigger.ExecuteSingleAttackAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
                case SkillTriggerActionKind.LineAttack:
                    return SkillTrigger.ExecuteLineAttackAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
                case SkillTriggerActionKind.Effect:
                    return SkillTrigger.ExecuteEffectAction(combatManager, roster, sourceEntry, trigger, triggerContext);
                case SkillTriggerActionKind.CooldownRefund:
                    return SkillTrigger.ReduceTargetCooldownAction(roster, sourceEntry, trigger);
                case SkillTriggerActionKind.ReloadReduce:
                    return SkillTrigger.ReduceTargetReloadAction(roster, sourceEntry, trigger);
                default:
                    return SkillTrigger.ExecuteTriggeredSkillAction(combatManager, sourceEntry, trigger, triggerContext);
            }
        }

        /*
         * 트리거 행동을 결정한다.
         */
        private static SkillTriggerActionKind ResolveTriggerAction(SkillTriggerDefinition trigger)
        {
            if (trigger == null)
            {
                return SkillTriggerActionKind.Auto;
            }

            if (trigger.TriggerAction != SkillTriggerActionKind.Auto)
            {
                return trigger.TriggerAction;
            }

            return trigger.RuntimeKind == SkillRuntimeKind.SingleAttack
                ? SkillTriggerActionKind.SingleAttack
                : SkillTriggerActionKind.TriggeredSkill;
        }

        /*
         * 실행 계획에서 효과 행동만 가져온다.
         */
        public static SkillEffectDefinition[] ResolveEffects(
            SkillSnapshot snapshot,
            SkillEffectDefinition[] fallbackEffects)
        {
            var actions = snapshot != null && snapshot.Plan != null
                ? snapshot.Plan.EffectActions
                : null;
            if (actions == null || actions.Count == 0)
            {
                return fallbackEffects ?? Array.Empty<SkillEffectDefinition>();
            }

            var resolved = new SkillEffectDefinition[actions.Count];
            for (var i = 0; i < actions.Count; i++)
            {
                resolved[i] = actions[i] != null ? actions[i].Definition : null;
            }

            return resolved;
        }

        /*
         * 트리거를 결정한다.
         */
        public static SkillTriggerDefinition[] ResolveTriggers(
            SkillSnapshot snapshot,
            SkillTriggerDefinition[] fallbackTriggers)
        {
            var actions = snapshot != null && snapshot.Plan != null
                ? snapshot.Plan.TriggerActions
                : null;
            if (actions == null || actions.Count == 0)
            {
                return fallbackTriggers ?? Array.Empty<SkillTriggerDefinition>();
            }

            var resolved = new SkillTriggerDefinition[actions.Count];
            for (var i = 0; i < actions.Count; i++)
            {
                resolved[i] = actions[i] != null ? actions[i].Definition : null;
            }

            return resolved;
        }

        /*
         * 트리거를 결정한다.
         */
        public static SkillTriggerDefinition[] ResolveTriggers(
            SkillRuntimeInstance runtime,
            SkillTriggerDefinition[] fallbackTriggers)
        {
            var actions = runtime != null && runtime.BasePlan != null
                ? runtime.BasePlan.TriggerActions
                : null;
            if (actions == null || actions.Count == 0)
            {
                return fallbackTriggers ?? Array.Empty<SkillTriggerDefinition>();
            }

            var resolved = new SkillTriggerDefinition[actions.Count];
            for (var i = 0; i < actions.Count; i++)
            {
                resolved[i] = actions[i] != null ? actions[i].Definition : null;
            }

            return resolved;
        }
    }
}
