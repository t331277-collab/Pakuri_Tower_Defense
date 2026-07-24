using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
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
}
