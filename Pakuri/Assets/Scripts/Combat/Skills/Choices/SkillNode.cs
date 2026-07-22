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
    public enum DamageModifierOpKind
    {
        BossMultiplier,
        ExecuteMultiplier
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
        StatusStackAmountBonus,
        StatusStackAmountSet,
        StatusMaxStacksBonus,
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
        StatusElementDamageTakenBonus,
        StatusCriticalDamageTakenBonus
    }

    /*
     * 노드가 사용하는 조건과 수치 변경 값을 보관한다.
     */
    public readonly struct CastConditionOp
    {
        /*
         * CastConditionOp에 필요한 값을 초기화한다.
         */
        public CastConditionOp(float targetHealthRatioBonus /* 대상 체력 비율 추가값 */)
        {
            TargetHealthRatioBonus = targetHealthRatioBonus;
        }

        public float TargetHealthRatioBonus { get; }
    }

    public readonly struct DamageModifierOp
    {
        /*
         * DamageModifierOp에 필요한 값을 초기화한다.
         */
        public DamageModifierOp(DamageModifierOpKind kind /* 처리할 종류 */, float multiplier /* 값에 곱할 배율 */)
        {
            Kind = kind;
            Multiplier = multiplier;
        }

        public DamageModifierOpKind Kind { get; }
        public float Multiplier { get; }
    }

    public readonly struct CritModifierOp
    {
        /*
         * CritModifierOp에 필요한 값을 초기화한다.
         */
        public CritModifierOp(float chanceBonus /* 확률 추가값 */)
        {
            ChanceBonus = chanceBonus;
        }

        public float ChanceBonus { get; }
    }

    public readonly struct KillActionOp
    {
        /*
         * KillActionOp에 필요한 값을 초기화한다.
         */
        public KillActionOp(KillActionOpKind kind /* 처리할 종류 */, float ratioBonus /* 비율 추가값 */, bool requiresExecute /* 필요 처형 여부 */)
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
        /*
         * 소수 값을 사용하는 단순 행동을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            float amount /* 적용할 수치 */)
        {
            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = string.Empty;
        }

        /*
         * 정수 값을 사용하는 단순 행동을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            int count /* 적용할 개수 */)
        {
            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = string.Empty;
        }

        /*
         * 참조 대상과 소수 값을 사용하는 단순 행동을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            string referenceId /* 참조할 데이터 식별자 */,
            float amount /* 적용할 수치 */)
        {
            if (referenceId == null)
            {
                referenceId = string.Empty;
            }

            Kind = kind;
            Amount = amount;
            Count = 0;
            ReferenceId = referenceId;
        }

        /*
         * 참조 대상과 정수 값을 사용하는 단순 행동을 초기화한다.
         */
        public SkillActionOp(
            SkillActionOpKind kind /* 처리할 종류 */,
            string referenceId /* 참조할 데이터 식별자 */,
            int count /* 적용할 개수 */)
        {
            if (referenceId == null)
            {
                referenceId = string.Empty;
            }

            Kind = kind;
            Amount = 0f;
            Count = count;
            ReferenceId = referenceId;
        }

        public SkillActionOpKind Kind { get; }
        public float Amount { get; }
        public int Count { get; }
        public string ReferenceId { get; }
    }

    /*
     * 연속 적중 횟수에 따라 증가하는 피해 값을 보관한다.
     */
    public readonly struct ConsecutiveHitActionOp
    {
        /*
         * 적중당 피해 증가율과 최대 증가율을 초기화한다.
         */
        public ConsecutiveHitActionOp(
            float bonusRate /* 적중당 피해 증가율 */,
            float maxBonus /* 최대 피해 증가율 */)
        {
            BonusRate = bonusRate;
            MaxBonus = maxBonus;
        }

        public float BonusRate { get; }
        public float MaxBonus { get; }
    }

    /*
     * 공격이 다른 대상으로 분기될 때 필요한 값을 보관한다.
     */
    public readonly struct BranchDamageActionOp
    {
        /*
         * 분기 확률, 횟수, 피해 배율, 탐색 반경을 초기화한다.
         */
        public BranchDamageActionOp(
            float chanceBonus /* 분기 확률 추가값 */,
            int branchCount /* 분기 횟수 */,
            float damageMultiplier /* 분기 피해 배율 */,
            float searchRadius /* 다음 대상을 찾을 반경 */)
        {
            ChanceBonus = chanceBonus;
            BranchCount = branchCount;
            DamageMultiplier = damageMultiplier;
            SearchRadius = searchRadius;
        }

        public float ChanceBonus { get; }
        public int BranchCount { get; }
        public float DamageMultiplier { get; }
        public float SearchRadius { get; }
    }

    /*
     * 대상의 상태 효과 조건에 따라 적용할 피해 배율을 보관한다.
     */
    public readonly struct ConditionalDamageActionOp
    {
        /*
         * 피해 배율과 필요한 상태 효과 중첩 수를 초기화한다.
         */
        public ConditionalDamageActionOp(
            float damageMultiplier /* 조건을 만족했을 때 적용할 피해 배율 */,
            StatusEffectKind requiredStatus /* 대상에게 필요한 상태 효과 */,
            int minimumStacks /* 필요한 최소 중첩 수 */)
        {
            DamageMultiplier = damageMultiplier;
            RequiredStatus = requiredStatus;
            MinimumStacks = minimumStacks;
        }

        public float DamageMultiplier { get; }
        public StatusEffectKind RequiredStatus { get; }
        public int MinimumStacks { get; }
    }

    /*
     * 지정된 진영의 상태 효과 개수에 따라 증가하는 피해 값을 보관한다.
     */
    public readonly struct CountStatusDamageActionOp
    {
        /*
         * 셀 대상, 상태 효과, 개수당 증가량, 최대 계산 개수를 초기화한다.
         */
        public CountStatusDamageActionOp(
            SkillMultiEffectTargetSide targetSide /* 상태 효과를 셀 대상 진영 */,
            StatusEffectKind statusKind /* 셀 상태 효과 종류 */,
            float amountPerCount /* 상태 효과 하나당 피해 증가량 */,
            int maximumCount /* 피해 계산에 사용할 최대 개수 */)
        {
            TargetSide = targetSide;
            StatusKind = statusKind;
            AmountPerCount = amountPerCount;
            MaximumCount = maximumCount;
        }

        public SkillMultiEffectTargetSide TargetSide { get; }
        public StatusEffectKind StatusKind { get; }
        public float AmountPerCount { get; }
        public int MaximumCount { get; }
    }

    /*
     * 공격자에게 특정 상태 효과가 있을 때 받는 피해 증가값을 보관한다.
     */
    public readonly struct StatusConditionalDamageTakenActionOp
    {
        /*
         * 받는 피해 증가값과 공격자에게 필요한 상태 효과를 초기화한다.
         */
        public StatusConditionalDamageTakenActionOp(
            float bonus /* 받는 피해 증가값 */,
            StatusEffectKind requiredSourceStatus /* 공격자에게 필요한 상태 효과 */)
        {
            Bonus = bonus;
            RequiredSourceStatus = requiredSourceStatus;
        }

        public float Bonus { get; }
        public StatusEffectKind RequiredSourceStatus { get; }
    }

    /*
     * 스킬 실행 계획에 포함될 조건, 보정, 행동 노드를 보관한다.
     */
    public class SkillNode
    {
        /*
         * 스킬 실행 계획 노드에 필요한 값을 초기화한다.
         */
        public SkillNode(
            CastConditionOp? castCondition = null /* 스킬 사용 조건 */,
            SkillActionOp? action = null /* 동작 */,
            DamageModifierOp? damageModifier = null /* 피해 보정 */,
            CritModifierOp? critModifier = null /* 치명타 보정 */,
            KillActionOp? killAction = null /* 처치 동작 */,
            SkillEffectAction effectAction = null /* 효과 동작 */,
            SkillTriggerAction triggerAction = null /* 트리거 동작 */,
            ConsecutiveHitActionOp? consecutiveHitAction = null /* 연속 적중 피해 동작 */,
            BranchDamageActionOp? branchDamageAction = null /* 분기 피해 동작 */,
            ConditionalDamageActionOp? conditionalDamageAction = null /* 상태 조건 피해 동작 */,
            CountStatusDamageActionOp? countStatusDamageAction = null /* 상태 효과 개수 피해 동작 */,
            StatusConditionalDamageTakenActionOp? statusConditionalDamageTakenAction = null /* 공격자 상태 조건 받는 피해 동작 */)
        {
            CastCondition = castCondition;
            Action = action;
            DamageModifier = damageModifier;
            CritModifier = critModifier;
            KillAction = killAction;
            EffectAction = effectAction;
            TriggerAction = triggerAction;
            ConsecutiveHitAction = consecutiveHitAction;
            BranchDamageAction = branchDamageAction;
            ConditionalDamageAction = conditionalDamageAction;
            CountStatusDamageAction = countStatusDamageAction;
            StatusConditionalDamageTakenAction = statusConditionalDamageTakenAction;
        }

        public CastConditionOp? CastCondition { get; }
        public SkillActionOp? Action { get; }
        public DamageModifierOp? DamageModifier { get; }
        public CritModifierOp? CritModifier { get; }
        public KillActionOp? KillAction { get; }
        public SkillEffectAction EffectAction { get; }
        public SkillTriggerAction TriggerAction { get; }
        public ConsecutiveHitActionOp? ConsecutiveHitAction { get; }
        public BranchDamageActionOp? BranchDamageAction { get; }
        public ConditionalDamageActionOp? ConditionalDamageAction { get; }
        public CountStatusDamageActionOp? CountStatusDamageAction { get; }
        public StatusConditionalDamageTakenActionOp? StatusConditionalDamageTakenAction { get; }

        /*
         * 시전 조건을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCastCondition(
            CastConditionOp op /* 동작 */)
        {
            return new SkillNode(
                castCondition: op);
        }

        /*
         * 피해 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromDamageModifier(
            DamageModifierOp op /* 동작 */)
        {
            return new SkillNode(
                damageModifier: op);
        }

        /*
         * 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromAction(
            SkillActionOp op /* 동작 */)
        {
            return new SkillNode(
                action: op);
        }

        /*
         * 연속 적중 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromConsecutiveHitAction(
            ConsecutiveHitActionOp op /* 연속 적중 피해 동작 */)
        {
            return new SkillNode(
                consecutiveHitAction: op);
        }

        /*
         * 분기 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromBranchDamageAction(
            BranchDamageActionOp op /* 분기 피해 동작 */)
        {
            return new SkillNode(
                branchDamageAction: op);
        }

        /*
         * 상태 조건 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromConditionalDamageAction(
            ConditionalDamageActionOp op /* 상태 조건 피해 동작 */)
        {
            return new SkillNode(
                conditionalDamageAction: op);
        }

        /*
         * 상태 효과 개수 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCountStatusDamageAction(
            CountStatusDamageActionOp op /* 상태 효과 개수 피해 동작 */)
        {
            return new SkillNode(
                countStatusDamageAction: op);
        }

        /*
         * 공격자 상태 조건 받는 피해 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromStatusConditionalDamageTakenAction(
            StatusConditionalDamageTakenActionOp op /* 공격자 상태 조건 받는 피해 동작 */)
        {
            return new SkillNode(
                statusConditionalDamageTakenAction: op);
        }

        /*
         * 치명타 보정값을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromCritModifier(
            CritModifierOp op /* 동작 */)
        {
            return new SkillNode(
                critModifier: op);
        }

        /*
         * 처치 행동을 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromKillAction(
            KillActionOp op /* 동작 */)
        {
            return new SkillNode(
                killAction: op);
        }

        /*
         * 효과를 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromEffect(
            SkillEffectDefinition effect /* 실행하거나 변환할 효과 */)
        {
            return new SkillNode(
                effectAction: new SkillEffectAction(effect));
        }

        /*
         * 트리거를 실행 계획 노드로 변환한다.
         */
        public static SkillNode FromTrigger(
            SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
        {
            return new SkillNode(
                triggerAction: new SkillTriggerAction(trigger));
        }
    }

    /*
     * 스킬 효과 행동에 필요한 값을 보관한다.
     */
    public class SkillEffectAction
    {
        /*
         * 스킬 효과 행동에 필요한 값을 초기화한다.
         */
        public SkillEffectAction(SkillEffectDefinition definition /* 변환하거나 검사할 정의 */)
        {
            Definition = definition;
        }

        internal SkillEffectDefinition Definition { get; }
    }

    /*
     * 스킬 트리거 행동에 필요한 값을 보관한다.
     */
    public class SkillTriggerAction
    {
        /*
         * 스킬 트리거 행동에 필요한 값을 초기화한다.
         */
        public SkillTriggerAction(SkillTriggerDefinition definition /* 변환하거나 검사할 정의 */)
        {
            Definition = definition;
        }

        internal SkillTriggerDefinition Definition { get; }
    }

    /*
     * 스킬 실행에 사용할 조건, 보정, 행동 목록을 보관한다.
     */
    public class SkillNodePlan
    {
        /*
         * 스킬 실행 계획에 필요한 값을 초기화한다.
         */
        public SkillNodePlan(
            IReadOnlyList<CastConditionOp> castConditions /* 스킬 사용 조건 목록 */,
            IReadOnlyList<DamageModifierOp> damageModifiers /* 피해 보정 목록 */,
            IReadOnlyList<CritModifierOp> critModifiers /* 치명타 보정 목록 */,
            IReadOnlyList<KillActionOp> killActions /* 처치 시 실행할 동작 목록 */,
            IReadOnlyList<SkillNode> nodes = null /* 노드 목록 */)
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
        private static T[] Copy<T>(IReadOnlyList<T> source /* 효과를 발생시킨 원본 */)
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
            IReadOnlyList<T> legacyOps /* 이전 형식의 동작 목록 */,
            IReadOnlyList<SkillNode> nodes /* 노드 목록 */,
            System.Func<SkillNode, T?> selector /* 선택 함수 */)
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
            IReadOnlyList<SkillNode> nodes /* 노드 목록 */,
            System.Func<SkillNode, T?> selector /* 선택 함수 */)
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
            IReadOnlyList<SkillNode> nodes /* 노드 목록 */,
            System.Func<SkillNode, T> selector /* 선택 함수 */)
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
            SkillRuntimeData source /* 복사하거나 변환할 스킬 실행 데이터 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            IReadOnlyList<SkillNode> normalizedRows /* 정규화된 행 목록 */)
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
            SkillRuntimeData source /* 복사하거나 변환할 스킬 실행 데이터 */,
            IReadOnlyList<SkillNode> normalizedRows /* 정규화된 행 목록 */)
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
            SkillExecutionContext context /* 스킬 실행에 필요한 정보 */,
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillEffectDefinition effect /* 실행하거나 변환할 효과 */,
            UnityEngine.Vector2 fallbackCenter /* 중심을 정하지 못했을 때 사용할 위치 */,
            bool scaleStatusDurationWithSnapshot = false /* 강화 배율을 상태 효과 지속 시간에 적용할지 여부 */)
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
            InGameCombatManager combatManager /* 전투 진행 관리자 */,
            CombatUnitRegistry roster /* 전투에 등록된 유닛 목록 */,
            CombatUnitEntry sourceEntry /* 효과를 발생시킨 유닛의 등록 정보 */,
            UnitCombatState source /* 효과를 발생시킨 유닛 */,
            SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */,
            SkillTrigger.TriggerExecutionContext triggerContext /* 트리거 실행에 필요한 정보 */)
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
        private static SkillTriggerActionKind ResolveTriggerAction(SkillTriggerDefinition trigger /* 실행하거나 검사할 트리거 */)
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
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillEffectDefinition[] fallbackEffects /* 대체 효과 목록 */)
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
            SkillSnapshot snapshot /* 적용할 스킬 강화 정보 */,
            SkillTriggerDefinition[] fallbackTriggers /* 대체 트리거 목록 */)
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
            SkillRuntimeInstance runtime /* 실행 중인 스킬 정보 */,
            SkillTriggerDefinition[] fallbackTriggers /* 대체 트리거 목록 */)
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
