using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;

/* 스킬 요청을 검증하고 실행 계획, 계열별 실행기, 쿨다운 처리를 조정한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public sealed class SkillExecutionRuntime
    {
        private readonly ProjectileExecutor projectile;
        private readonly LineAttackExecutor line;
        private readonly AreaAttackExecutor area;
        private readonly SingleAttackExecutor single;
        private readonly BuffExecutor buff;
        private readonly HealExecutor heal;
        private readonly ShieldExecutor shield;
        private readonly PassiveExecutor passive;
        private readonly SkillEffectGraphRuntime effectGraphs;
        private readonly GameDefinitionCatalog catalog;
        private readonly HashSet<string> appliedPassives =
            new HashSet<string>(StringComparer.Ordinal);

        /* 카탈로그와 런타임 서비스 의존성을 저장하고 실행기를 구성한다. */
        public SkillExecutionRuntime(
            GameDefinitionCatalog catalog,
            SkillTargeting targeting,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            ValidateReachableNodes(catalog);
            effectGraphs = new SkillEffectGraphRuntime(
                catalog,
                actors,
                effects,
                randomValue,
                ReportNodeContract);
            Triggers = new SkillTriggerDispatcher(
                catalog,
                actors,
                effects,
                randomValue,
                effectGraphs,
                ReportNodeContract);
            projectile = new ProjectileExecutor(catalog, targeting, actors, effects, randomValue);
            line = new LineAttackExecutor(catalog, targeting, actors, effects, randomValue);
            area = new AreaAttackExecutor(catalog, targeting, actors, effects, randomValue);
            single = new SingleAttackExecutor(catalog, targeting, actors, effects, randomValue);
            buff = new BuffExecutor(catalog, targeting, actors, effects, randomValue);
            heal = new HealExecutor(catalog, targeting, actors, effects, randomValue);
            shield = new ShieldExecutor(catalog, targeting, actors, effects, randomValue);
            passive = new PassiveExecutor(catalog, targeting, actors, effects, randomValue);
        }

        public SkillTriggerDispatcher Triggers { get; }

        public event Action<Definitions.Choices.ChoiceNodeDefinition>
            NodeContractExecuted;

        /* 유닛이 학습한 패시브의 효과 그래프와 전투 시작 트리거를 적용한다. */
        public void ApplyPassives(
            InGameCombatManager combat,
            IReadOnlyList<Units.Models.UnitBaseModel> units)
        {
            for (int unitIndex = 0; unitIndex < units.Count; unitIndex++)
            {
                Units.Models.UnitBaseModel unit = units[unitIndex];
                IReadOnlyList<PassiveDefinition> passives =
                    unit is Units.Models.MonsterModel monster
                        ? monster.SkillBucket.PassiveSkills
                        : unit is Units.Models.EnemyModel enemy
                            ? enemy.SkillBucket.PassiveSkills
                            : Array.Empty<PassiveDefinition>();
                for (int passiveIndex = 0;
                    passiveIndex < passives.Count;
                    passiveIndex++)
                {
                    PassiveDefinition definition = passives[passiveIndex];
                    string key = unit.GetHashCode() + ":" + definition.skill_id;
                    if (!appliedPassives.Add(key))
                    {
                        continue;
                    }
                    var request = new SkillExecutionRequest(
                        unit,
                        definition,
                        units,
                        isTriggered: true);
                    SkillExecutionPlan plan = SkillExecutionPlan.Create(
                        catalog,
                        unit,
                        definition,
                        units,
                        ReportNodeContract);
                    passive.Execute(combat, request, plan);
                    effectGraphs.ExecuteOwnedGraphs(combat, request);
                }
            }
        }

        /* 전투별 트리거 예약 상태를 초기화한다. */
        public void ResetCombat()
        {
            appliedPassives.Clear();
            Triggers.Reset();
        }

        /* 카탈로그에서 도달 가능한 선택 노드의 handler와 런타임 소유자를 검증한다. */
        private static void ValidateReachableNodes(GameDefinitionCatalog catalog)
        {
            for (int index = 0; index < catalog.ChoiceNodes.Count; index++)
            {
                var node = catalog.ChoiceNodes[index];
                if (!catalog.NodeTypes.TryGetValue(node.node_type_id, out var nodeType))
                {
                    throw new InvalidOperationException(
                        $"Reachable node '{node.node_type_id}' has no definition.");
                }

                SkillNodeSupport.Resolve(nodeType.handler_id);
                SkillNodeSupport.ResolveRuntimeOwner(node);
            }
        }

        /* 실행 가능 조건을 검사하고 스킬 계열 실행기와 후속 그래프를 순서대로 실행한다. */
        public bool TryExecute(InGameCombatManager combat, SkillExecutionRequest request)
        {
            if (combat == null || request == null)
            {
                throw new ArgumentNullException(combat == null ? nameof(combat) : nameof(request));
            }

            if (!request.Caster.IsAlive || !request.Caster.CanAct)
            {
                return false;
            }

            SkillExecutionPlan plan =
                SkillExecutionPlan.Create(
                    catalog,
                    request.Caster,
                    request.Skill,
                    request.RegisteredUnits,
                    ReportNodeContract);
            if (!request.IsTriggered && !(request.Skill is PassiveDefinition))
            {
                ConfigureMagazine(request, plan);
            }
            if (!plan.CanExecute())
            {
                return false;
            }

            if (!request.IsTriggered
                && !(request.Skill is PassiveDefinition)
                && request.Caster is Units.Models.MonsterModel monsterOwner
                && !monsterOwner.SkillBucket.GetCooldown(request.Skill.skill_id).CanUse())
            {
                return false;
            }

            if (!request.IsTriggered
                && !(request.Skill is PassiveDefinition)
                && request.Caster is Units.Models.EnemyModel enemyOwner
                && !enemyOwner.SkillBucket.GetCooldown(request.Skill.skill_id).CanUse())
            {
                return false;
            }

            SkillExecutor executor = ResolveExecutor(request.Skill);
            Pakuri.NewCore.Combat.Skills.Runtime.SkillCooldown activeCooldown =
                null;
            bool cooldownStarted = false;
            request.TargetDefeated = _ =>
            {
                if (cooldownStarted && activeCooldown != null)
                {
                    ApplyKillCooldownPlan(activeCooldown, plan);
                }
            };
            request.HitCompleted = _ =>
                effectGraphs.ExecuteOwnedGraphs(
                    combat,
                    request,
                    "OnHit");
            if (!executor.Execute(combat, request, plan))
            {
                request.HitCompleted = null;
                return false;
            }
            effectGraphs.ExecuteOwnedGraphs(
                combat,
                request,
                "OnCast");

            if (!request.IsTriggered
                && !(request.Skill is PassiveDefinition)
                && request.Caster is Units.Models.MonsterModel monster)
            {
                var cooldown =
                    monster.SkillBucket.GetCooldown(request.Skill.skill_id);
                if (!cooldown.TryUse())
                {
                    throw new InvalidOperationException(
                        "A skill executed after its cooldown became unavailable.");
                }
                activeCooldown = cooldown;
                ApplyCooldownPlan(cooldown, plan);
                cooldownStarted = true;
                if (request.DefeatedTargetCount > 0)
                {
                    ApplyKillCooldownPlan(cooldown, plan);
                }
            }
            else if (!request.IsTriggered
                && !(request.Skill is PassiveDefinition)
                && request.Caster is Units.Models.EnemyModel enemy)
            {
                var cooldown = enemy.SkillBucket.GetCooldown(request.Skill.skill_id);
                if (!cooldown.TryUse())
                {
                    throw new InvalidOperationException(
                        "A skill executed after its cooldown became unavailable.");
                }
                activeCooldown = cooldown;
                ApplyCooldownPlan(cooldown, plan);
                cooldownStarted = true;
                if (request.DefeatedTargetCount > 0)
                {
                    ApplyKillCooldownPlan(cooldown, plan);
                }
            }

            if (!(request.Skill is PassiveDefinition))
            {
                combat.NotifySkillActivated(
                    request.Caster,
                    request.Skill,
                    request.TriggerSourceSkillId,
                    request.TriggerAncestry);
            }
            return true;
        }

        /* 선택 노드가 소비되었다는 계약 검증 정보를 카탈로그에 보고한다. */
        private void ReportNodeContract(
            Definitions.Choices.ChoiceNodeDefinition node)
        {
            NodeContractExecuted?.Invoke(node);
        }

        /* 실행 계획의 탄창 보너스를 시전자 스킬 버킷에 반영한다. */
        private static void ConfigureMagazine(
            SkillExecutionRequest request,
            SkillExecutionPlan plan)
        {
            if (request.Caster is Units.Models.MonsterModel monster)
            {
                monster.SkillBucket.GetCooldown(
                    request.Skill.skill_id).SetMagazineBonus(
                        Math.Max(0, plan.ResolveMagazineBonus()));
            }
            else if (request.Caster is Units.Models.EnemyModel enemy)
            {
                enemy.SkillBucket.GetCooldown(
                    request.Skill.skill_id).SetMagazineBonus(
                        Math.Max(0, plan.ResolveMagazineBonus()));
            }
        }

        /* 실행 계획의 재사용 대기시간 감소와 초기화 규칙을 적용한다. */
        private static void ApplyCooldownPlan(
            Pakuri.NewCore.Combat.Skills.Runtime.SkillCooldown cooldown,
            SkillExecutionPlan plan)
        {
            cooldown.ScaleCooldown(plan.ResolveCooldownMultiplier());
            cooldown.ScaleReload(plan.ResolveReloadMultiplier());
            cooldown.ScaleShotInterval(plan.ResolveShotIntervalMultiplier());
        }

        /* 처치 결과에 따른 재사용 대기시간 감소와 초기화 규칙을 적용한다. */
        private static void ApplyKillCooldownPlan(
            Pakuri.NewCore.Combat.Skills.Runtime.SkillCooldown cooldown,
            SkillExecutionPlan plan)
        {
            cooldown.ReduceCooldown(plan.ResolveCooldownRefundRatio());
            if (plan.ShouldResetCooldown())
            {
                cooldown.ResetCooldown();
            }
        }

        /* 스킬 정의 타입에 대응하는 계열별 실행기를 반환한다. */
        private SkillExecutor ResolveExecutor(SkillDefinition definition)
        {
            if (definition is ProjectileDefinition)
            {
                return projectile;
            }

            if (definition is LineAttackDefinition)
            {
                return line;
            }

            if (definition is AreaAttackDefinition)
            {
                return area;
            }

            if (definition is SingleAttackDefinition)
            {
                return single;
            }

            if (definition is HealDefinition)
            {
                return heal;
            }

            if (definition is ShieldDefinition
                || string.Equals(definition.runtime_kind, "Shield", StringComparison.Ordinal))
            {
                return shield;
            }

            if (definition is BuffDefinition)
            {
                return buff;
            }

            if (definition is PassiveDefinition)
            {
                return passive;
            }

            throw new NotSupportedException(
                $"No Executor exists for '{definition.GetType().Name}'.");
        }
    }

    public enum SkillNodeBehavior
    {
        Condition,
        Damage,
        Status,
        Timing,
        Projectile,
        Targeting,
        Effect,
        Resource
    }

    public static class SkillNodeSupport
    {
        /* handler_id 접두어와 포함 문자열을 기준으로 노드 동작 분류를 반환한다. */
        public static SkillNodeBehavior Resolve(string handlerId)
        {
            if (string.IsNullOrWhiteSpace(handlerId))
            {
                throw new NotSupportedException("A reachable node has no handler_id.");
            }

            if (handlerId.StartsWith("Condition", StringComparison.Ordinal)
                || handlerId.StartsWith("Required", StringComparison.Ordinal)
                || handlerId.StartsWith("TargetHealth", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Condition;
            }

            if (handlerId.StartsWith("Status", StringComparison.Ordinal)
                || handlerId.StartsWith("AttachStatus", StringComparison.Ordinal)
                || handlerId.StartsWith("ThresholdApplyStatus", StringComparison.Ordinal)
                || handlerId.StartsWith("ApplyStatus", StringComparison.Ordinal)
                || handlerId.StartsWith("RedistributeConsumedStatus", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Status;
            }

            if (handlerId.IndexOf("Cooldown", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Reload", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Duration", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Interval", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("DamageDelay", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Timing;
            }

            if (handlerId.IndexOf("Projectile", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("Pierce", StringComparison.Ordinal)
                || handlerId.StartsWith("Magazine", StringComparison.Ordinal)
                || handlerId.StartsWith("Burst", StringComparison.Ordinal)
                || handlerId.StartsWith("Branch", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Projectile;
            }

            if (handlerId.IndexOf("Damage", StringComparison.Ordinal) >= 0
                || handlerId.IndexOf("Crit", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("Core", StringComparison.Ordinal)
                || handlerId.StartsWith("ConsecutiveHit", StringComparison.Ordinal)
                || handlerId.StartsWith("EveryNthHit", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Damage;
            }

            if (handlerId.IndexOf("Radius", StringComparison.Ordinal) >= 0
                || handlerId.StartsWith("HitTarget", StringComparison.Ordinal)
                || handlerId.StartsWith("Beam", StringComparison.Ordinal)
                || handlerId.StartsWith("Knockback", StringComparison.Ordinal)
                || handlerId.StartsWith("RepeatPerTarget", StringComparison.Ordinal)
                || handlerId.StartsWith("EffectTarget", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Targeting;
            }

            if (handlerId.StartsWith("ApplyShield", StringComparison.Ordinal)
                || handlerId.StartsWith("Effect", StringComparison.Ordinal)
                || handlerId.StartsWith("RuntimeEffect", StringComparison.Ordinal)
                || handlerId.StartsWith("RecastZone", StringComparison.Ordinal)
                || handlerId.StartsWith("FollowUp", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Effect;
            }

            if (handlerId.StartsWith("ConsumeTarget", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Resource;
            }

            if (handlerId.StartsWith("TriggerProc", StringComparison.Ordinal)
                || handlerId.StartsWith("ShieldAmount", StringComparison.Ordinal))
            {
                return SkillNodeBehavior.Resource;
            }

            throw new NotSupportedException(
                $"Reachable node handler '{handlerId}' is not implemented.");
        }

        /* 그래프 종류와 노드 타입을 기준으로 실제 소비 런타임을 결정한다. */
        public static SkillNodeRuntimeOwner ResolveRuntimeOwner(
            ChoiceNodeDefinition node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (string.Equals(
                node.graph_kind,
                "Effect",
                StringComparison.Ordinal)
                && IsEffectGraphNode(node.node_type_id))
            {
                return SkillNodeRuntimeOwner.EffectGraph;
            }
            if (string.Equals(
                node.graph_kind,
                "Plan",
                StringComparison.Ordinal)
                && IsPlanNode(node.node_type_id))
            {
                return IsExecutorOwnedPlanNode(node.node_type_id)
                    ? SkillNodeRuntimeOwner.FamilyExecutor
                    : node.node_type_id == "TriggerProcChanceBonus"
                        ? SkillNodeRuntimeOwner.TriggerDispatcher
                        : SkillNodeRuntimeOwner.ExecutionPlan;
            }

            throw new NotSupportedException(
                $"Reachable {node.graph_kind} node "
                + $"'{node.node_type_id}' has no runtime consumer.");
        }

        /* 노드 타입이 효과 그래프에서 처리되는 타입인지 확인한다. */
        private static bool IsEffectGraphNode(string nodeTypeId)
        {
            switch (nodeTypeId)
            {
                case "ApplyShield":
                case "ApplyStatus":
                case "AttachStatusPayload":
                case "ConditionAnyStatus":
                case "ConditionHealthRatioMax":
                case "ConditionHitCountMin":
                case "ConditionSkillAttribute":
                case "ConditionStatus":
                case "ConditionStatusExpression":
                case "EffectDamage":
                case "EffectExtendStatusDuration":
                case "EffectLifetime":
                case "EffectTarget":
                case "RecastZone":
                case "RequiredSourceStatus":
                case "RuntimeEffectVisual":
                case "StatusActionSpeedBonus":
                case "StatusAttackPowerBonus":
                case "StatusConditionalStatusChanceBonus":
                case "StatusCriticalChanceBonus":
                case "StatusCriticalDamageBonus":
                case "StatusCriticalResistanceBonus":
                case "StatusDamageBonusRate":
                case "StatusDamageTakenBonus":
                case "StatusElementDamageTakenBonus":
                case "StatusElementResistReduction":
                case "StatusFlatElementResistReduction":
                case "StatusModifier":
                case "StatusMoveSpeedBonus":
                case "StatusOutgoingAdditionalDamage":
                case "StatusRuntimeKindFilter":
                case "StatusShieldReceivedBonus":
                case "StatusSpellPowerBonus":
                    return true;
                default:
                    return false;
            }
        }

        /* 노드 타입이 실행 계획에서 사용할 수 있는 타입인지 확인한다. */
        private static bool IsPlanNode(string nodeTypeId)
        {
            switch (nodeTypeId)
            {
                case "AdditionalDamage":
                case "AdditionalProjectileBonus":
                case "BeamWidthBonus":
                case "BranchDamage":
                case "BurstDamageRule":
                case "BurstStatusStacksBonus":
                case "ConditionalDamageMultiplier":
                case "ConsecutiveHitDamageBonus":
                case "ConsumeTargetStatusRatioOverride":
                case "CooldownMultiplier":
                case "CooldownRefund":
                case "CooldownRefundBonus":
                case "CooldownReset":
                case "CoreAdditionalDamage":
                case "CoreDamageMultiplier":
                case "CountStatusDamageMultiplier":
                case "CritChanceBonus":
                case "CritDamageBonus":
                case "DamageDelayMultiplier":
                case "DamageMultiplier":
                case "DurationBonus":
                case "DurationMultiplier":
                case "EveryNthHitChainDamage":
                case "ExecuteCritChanceBonus":
                case "ExecuteDamageMultiplier":
                case "FollowUpProjectile":
                case "HitCountCooldownRefund":
                case "HitTargetCountBonus":
                case "KnockbackDistanceMultiplier":
                case "MagazineBonus":
                case "PierceBonus":
                case "RadiusMultiplier":
                case "RedistributeConsumedStatus":
                case "ReloadReducePerHit":
                case "ReloadTimeMultiplier":
                case "RepeatPerTarget":
                case "RequiredSourceStatus":
                case "ShieldAmountMultiplier":
                case "ShotIntervalMultiplier":
                case "StatusActionSpeedBonus":
                case "StatusAilmentResistanceBonus":
                case "StatusAttackPowerBonus":
                case "StatusConditionalDamageTakenBonus":
                case "StatusCriticalDamageTakenBonus":
                case "StatusDamageBonusRate":
                case "StatusDamageTakenBonus":
                case "StatusDurationBonus":
                case "StatusElementDamageTakenBonus":
                case "StatusFilteredDeployment":
                case "StatusMaxStacksBonus":
                case "StatusShieldReceivedBonus":
                case "StatusStackAmountBonus":
                case "StatusStackAmountSet":
                case "TargetHealthRatioCondition":
                case "TargetHealthRatioThresholdBonus":
                case "TargetPredicateDamageMultiplier":
                case "TargetStatusCritBonus":
                case "TargetStatusStackDamage":
                case "TargetStatusStackDamageMultiplier":
                case "TargetStatusStackDamageRateBonus":
                case "ThresholdApplyStatus":
                case "TriggerProcChanceBonus":
                    return true;
                default:
                    return false;
            }
        }

        /* 실행 계획 노드 중 스킬 계열 실행기가 직접 처리할 타입인지 확인한다. */
        private static bool IsExecutorOwnedPlanNode(
            string nodeTypeId)
        {
            switch (nodeTypeId)
            {
                case "AdditionalDamage":
                case "BranchDamage":
                case "ConsumeTargetStatusRatioOverride":
                case "CoreAdditionalDamage":
                case "EveryNthHitChainDamage":
                case "HitCountCooldownRefund":
                case "RedistributeConsumedStatus":
                case "ReloadReducePerHit":
                case "StatusAilmentResistanceBonus":
                case "StatusConditionalDamageTakenBonus":
                case "StatusDurationBonus":
                case "StatusFilteredDeployment":
                case "StatusMaxStacksBonus":
                case "StatusStackAmountBonus":
                case "StatusStackAmountSet":
                case "TargetStatusStackDamage":
                case "ThresholdApplyStatus":
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum SkillNodeRuntimeOwner
    {
        ExecutionPlan,
        FamilyExecutor,
        EffectGraph,
        TriggerDispatcher
    }
}
