using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 학습한 선택 노드를 스킬 실행에 사용할 수치, 조건, 대상 규칙으로 해석한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class SkillExecutionPlan
    {
        private readonly List<ChoiceNodeDefinition> nodes;
        private readonly UnitBaseModel caster;
        private readonly SkillDefinition skill;
        private readonly IReadOnlyList<UnitBaseModel> units;
        private readonly Action<ChoiceNodeDefinition> nodeConsumed;

        /* 선택 노드와 실행 문맥을 하나의 실행 계획으로 저장한다. */
        private SkillExecutionPlan(
            List<ChoiceNodeDefinition> nodes,
            UnitBaseModel caster,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> units,
            Action<ChoiceNodeDefinition> nodeConsumed)
        {
            this.nodes = nodes;
            this.caster = caster;
            this.skill = skill;
            this.units = units;
            this.nodeConsumed = nodeConsumed;
        }

        /* 시전자가 학습한 선택지에서 현재 스킬에 속한 계획 노드를 수집한다. */
        public static SkillExecutionPlan Create(
            GameDefinitionCatalog catalog,
            UnitBaseModel caster,
            SkillDefinition skill,
            IReadOnlyList<UnitBaseModel> units,
            Action<ChoiceNodeDefinition> nodeConsumed = null)
        {
            List<ChoiceNodeDefinition> result = new List<ChoiceNodeDefinition>();
            HashSet<string> owners = new HashSet<string>(StringComparer.Ordinal)
            {
                skill.skill_id
            };
            if (caster is MonsterModel monster)
            {
                for (int index = 0; index < monster.SkillBucket.SelectedChoices.Count; index++)
                {
                    SkillChoiceDefinition choice =
                        monster.SkillBucket.SelectedChoices[index];
                    string effectiveSkillId =
                        string.IsNullOrEmpty(choice.target_skill_id)
                            ? choice.skill_id
                            : choice.target_skill_id;
                    if (effectiveSkillId == skill.skill_id
                        || ChoiceTargetsSkill(
                            catalog,
                            choice.choice_id,
                            skill.skill_id))
                    {
                        owners.Add(choice.choice_id);
                    }
                }

                for (int index = 0; index < monster.SkillBucket.PassiveSkills.Count; index++)
                {
                    if (monster.SkillBucket.PassiveSkills[index].skill_id
                        == skill.skill_id)
                    {
                        owners.Add(
                            monster.SkillBucket.PassiveSkills[index].skill_id);
                    }
                }
            }

            for (int index = 0; index < catalog.ChoiceNodes.Count; index++)
            {
                ChoiceNodeDefinition node = catalog.ChoiceNodes[index];
                if (owners.Contains(node.owner_id)
                    && string.Equals(
                        node.graph_kind,
                        "Plan",
                        StringComparison.Ordinal)
                    && (string.IsNullOrEmpty(node.target_skill_id)
                        || string.Equals(
                            node.target_skill_id,
                            skill.skill_id,
                            StringComparison.Ordinal)))
                {
                    result.Add(node);
                }
            }

            result.Sort((left, right) =>
            {
                int graph = Nullable.Compare(left.graph_index, right.graph_index);
                return graph != 0
                    ? graph
                    : Nullable.Compare(left.node_order, right.node_order);
            });
            return new SkillExecutionPlan(
                result,
                caster,
                skill,
                units,
                nodeConsumed);
        }

        /* 선택지의 대상 스킬 식별자가 현재 스킬과 일치하는지 확인한다. */
        private static bool ChoiceTargetsSkill(
            GameDefinitionCatalog catalog,
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

        /* 대상과 현재 적중 문맥에 맞는 모든 피해 배율 노드를 계산한다. */
        public float ResolveDamageMultiplier(
            UnitBaseModel target,
            int hitIndex = 0,
            bool isLastHit = false,
            string hitZone = null)
        {
            float multiplier = 1f;
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                switch (node.node_type_id)
                {
                    case "DamageMultiplier":
                        ReportConsumed(node);
                        if (MatchesCondition(node, target))
                        {
                            multiplier *= LastPositiveNumber(node, 1f);
                        }
                        break;
                    case "CoreDamageMultiplier":
                        ReportConsumed(node);
                        if (node.arg_1 == hitZone)
                        {
                            multiplier *= Number(node.arg_2, 1f);
                        }
                        break;
                    case "ExecuteDamageMultiplier":
                        ReportConsumed(node);
                        if (MatchesExecuteHealth(target))
                        {
                            multiplier *= Number(node.arg_1, 1f);
                        }
                        break;
                    case "TargetPredicateDamageMultiplier":
                        ReportConsumed(node);
                        if (MatchesPredicate(target, node.arg_1))
                        {
                            multiplier *= Number(node.arg_2, 1f);
                        }
                        break;
                    case "TargetStatusStackDamageMultiplier":
                        ReportConsumed(node);
                        string targetStackStatusId = SkillTargeting.ReadString(
                            skill,
                            "target_status_stack_status_id");
                        if (string.IsNullOrEmpty(targetStackStatusId))
                        {
                            targetStackStatusId = SkillTargeting.ReadString(
                                skill,
                                "target_selection_status_id");
                        }
                        if (HasStatus(target, targetStackStatusId, 1))
                        {
                            multiplier *= FirstNumber(node, 1f);
                        }
                        break;
                    case "ConditionalDamageMultiplier":
                        ReportConsumed(node);
                        if (MatchesCondition(node, target))
                        {
                            multiplier *= LastPositiveNumber(node, 1f);
                        }
                        break;
                    case "CountStatusDamageMultiplier":
                        ReportConsumed(node);
                        multiplier *= 1f
                            + (CountSideStatus(node.arg_1)
                                * Number(node.arg_3, 0f));
                        break;
                    case "BurstDamageRule":
                        ReportConsumed(node);
                        if (hitIndex == (int)Number(node.arg_1, 0f))
                        {
                            multiplier *= Number(node.arg_2, 1f);
                        }
                        break;
                    case "ConsecutiveHitDamageBonus":
                        ReportConsumed(node);
                        multiplier *= 1f + Math.Min(
                            Number(node.arg_2, 0f),
                            Math.Max(0, hitIndex)
                                * Number(node.arg_1, 0f));
                        break;
                }
            }

            return multiplier;
        }

        /* 실행 전제 조건 노드가 현재 시전자 상태에서 충족되는지 확인한다. */
        public bool CanExecute()
        {
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (node.node_type_id == "RequiredSourceStatus"
                    )
                {
                    ReportConsumed(node);
                    if (!HasStatus(
                        caster,
                        node.arg_1,
                        Math.Max(1, (int)Number(node.arg_2, 1f))))
                    {
                        return false;
                    }
                }
            }

            if (RequiresExecuteThreshold())
            {
                bool casterIsEnemy = caster is EnemyModel;
                for (int index = 0; index < units.Count; index++)
                {
                    UnitBaseModel target = units[index];
                    if (target != null
                        && target.IsAlive
                        && !(target is NexusModel)
                        && (target is EnemyModel) != casterIsEnemy
                        && MatchesExecuteHealth(target))
                    {
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        /* 대상 조건과 실행 체력 조건을 충족하는 유닛만 남긴다. */
        public IReadOnlyList<UnitBaseModel> FilterTargets(
            IReadOnlyList<UnitBaseModel> candidates)
        {
            string statusId = SkillTargeting.ReadString(
                skill,
                "deployment_required_target_status_id");
            int minimum = Math.Max(
                1,
                SkillTargeting.ReadInt(
                    skill,
                    "deployment_required_target_status_min_stacks"));
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id == "StatusFilteredDeployment")
                {
                    ReportConsumed(nodes[index]);
                    statusId = nodes[index].arg_1;
                    minimum = Math.Max(
                        1,
                        (int)Number(nodes[index].arg_2, 1f));
                }
            }
            bool requiresStatus = !string.IsNullOrEmpty(statusId);
            bool requiresExecute = RequiresExecuteThreshold();
            if (!requiresStatus && !requiresExecute)
            {
                return candidates;
            }
            List<UnitBaseModel> result = new List<UnitBaseModel>();
            for (int index = 0; index < candidates.Count; index++)
            {
                UnitBaseModel candidate = candidates[index];
                if ((!requiresStatus
                        || HasStatus(candidate, statusId, minimum))
                    && (!requiresExecute
                        || MatchesExecuteHealth(candidate)))
                {
                    result.Add(candidate);
                }
            }
            return result.AsReadOnly();
        }

        /* 피해 지연시간 배율 노드의 누적 곱을 반환한다. */
        public float ResolveDamageDelayMultiplier()
        {
            return Product("DamageDelayMultiplier");
        }

        /* 넉백 거리 배율 노드의 누적 곱을 반환한다. */
        public float ResolveKnockbackMultiplier()
        {
            return Product("KnockbackDistanceMultiplier");
        }

        /* 탄창 증가 노드의 정수 합을 반환한다. */
        public int ResolveMagazineBonus()
        {
            return IntegerSum("MagazineBonus");
        }

        /* 기본 반경에 반경 보너스와 배율 노드를 적용한다. */
        public float ResolveRadius(float baseRadius)
        {
            float multiplier = 1f;
            float bonus = 0f;
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (string.Equals(node.node_type_id, "RadiusMultiplier", StringComparison.Ordinal)
                    || string.Equals(node.node_type_id, "BeamWidthBonus", StringComparison.Ordinal))
                {
                    ReportConsumed(node);
                    multiplier *= FirstNumber(node, 1f);
                }
                else if (string.Equals(node.node_type_id, "RadiusBonus", StringComparison.Ordinal))
                {
                    ReportConsumed(node);
                    bonus += FirstNumber(node, 0f);
                }
            }

            return Math.Max(0f, (baseRadius * multiplier) + bonus);
        }

        /* 기본 지속시간에 지속시간 보너스와 배율 노드를 적용한다. */
        public float ResolveDuration(float baseDuration)
        {
            float result = baseDuration;
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (string.Equals(node.node_type_id, "DurationBonus", StringComparison.Ordinal)
                    )
                {
                    ReportConsumed(node);
                    result += LastNumber(node, 0f);
                }
                else if (string.Equals(node.node_type_id, "DurationMultiplier", StringComparison.Ordinal))
                {
                    ReportConsumed(node);
                    result *= LastPositiveNumber(node, 1f);
                }
            }

            return Math.Max(0f, result);
        }

        /* 지정 상태의 기본 지속시간에 관련 선택 노드를 적용한다. */
        public float ResolveStatusDuration(
            string statusId,
            float baseDuration)
        {
            float result = baseDuration;
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (node.node_type_id == "StatusDurationBonus"
                    && node.arg_1 == statusId)
                {
                    ReportConsumed(node);
                    result += Number(node.arg_2, 0f);
                }
            }
            return Math.Max(0f, result);
        }

        /* 지정 상태의 기본 스택에 설정값과 보너스 노드를 적용한다. */
        public int ResolveStatusStacks(string statusId, int baseStacks)
        {
            int result = baseStacks;
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (!string.Equals(node.arg_1, statusId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (string.Equals(node.node_type_id, "StatusStackAmountBonus", StringComparison.Ordinal))
                {
                    ReportConsumed(node);
                    result += (int)Number(node.arg_2, 0f);
                }
                else if (string.Equals(node.node_type_id, "StatusStackAmountSet", StringComparison.Ordinal))
                {
                    ReportConsumed(node);
                    result = (int)Number(node.arg_2, result);
                }
            }

            return Math.Max(1, result);
        }

        /* 연사 문맥에 맞는 상태 스택 보너스를 기본 스택에 적용한다. */
        public int ResolveBurstStatusStacks(
            int baseStacks,
            int projectileIndex,
            int burstProjectileCount)
        {
            int result = baseStacks;
            for (var index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (node.node_type_id != "BurstStatusStacksBonus")
                {
                    continue;
                }

                int configuredIndex = (int)Number(node.arg_1, 0f);
                bool matches = configuredIndex == 0
                    ? burstProjectileCount > 0
                        && projectileIndex == burstProjectileCount - 1
                    : projectileIndex + 1 == configuredIndex;
                if (!matches)
                {
                    continue;
                }

                ReportConsumed(node);
                result += (int)Number(node.arg_2, 0f);
            }

            return Math.Max(1, result);
        }

        /* 선택 노드가 추가로 부여하도록 지정한 상태 식별자를 열거한다. */
        public IEnumerable<string> AdditionalStatusIds()
        {
            var yielded = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (string.Equals(
                        node.node_type_id,
                        "ApplyStatus",
                        StringComparison.Ordinal)
                    && !string.IsNullOrEmpty(node.arg_1))
                {
                    ReportConsumed(node);
                    if (yielded.Add(node.arg_1))
                    {
                        yield return node.arg_1;
                    }
                }
                else if (string.Equals(
                        node.node_type_id,
                        "StatusStackAmountSet",
                        StringComparison.Ordinal)
                    && !string.IsNullOrEmpty(node.arg_1)
                    && !string.Equals(
                        node.arg_1,
                        skill.status_effect_id,
                        StringComparison.Ordinal)
                    && yielded.Add(node.arg_1))
                {
                    yield return node.arg_1;
                }
            }
        }

        /* 일반, 처형, 대상 상태 조건에 맞는 치명타 확률 보너스를 합산한다. */
        public float ResolveCriticalChanceBonus(UnitBaseModel target)
        {
            float result = Sum("CritChanceBonus");
            if (MatchesExecuteHealth(target))
            {
                result += Sum("ExecuteCritChanceBonus");
            }
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (node.node_type_id == "TargetStatusCritBonus"
                    && HasStatus(
                        target,
                        node.arg_1,
                        Math.Max(1, (int)Number(node.arg_4, 1f))))
                {
                    ReportConsumed(node);
                    result += Number(node.arg_2, 0f);
                }
            }
            return result;
        }

        /* 일반 및 대상 상태 조건에 맞는 치명타 피해 보너스를 합산한다. */
        public float ResolveCriticalDamageBonus(UnitBaseModel target)
        {
            float result = Sum("CritDamageBonus");
            for (int index = 0; index < nodes.Count; index++)
            {
                ChoiceNodeDefinition node = nodes[index];
                if (node.node_type_id == "TargetStatusCritBonus"
                    && HasStatus(
                        target,
                        node.arg_1,
                        Math.Max(1, (int)Number(node.arg_4, 1f))))
                {
                    ReportConsumed(node);
                    result += Number(node.arg_3, 0f);
                }
            }
            return result;
        }

        /* 추가 투사체 수 노드의 정수 합을 반환한다. */
        public int ResolveAdditionalProjectiles()
        {
            return IntegerSum("AdditionalProjectileBonus");
        }

        /* 후속 투사체 노드가 지정한 발사 수를 반환한다. */
        public int ResolveFollowUpProjectileCount()
        {
            return Math.Max(0, (int)LastFor("FollowUpProjectile", 1, 0f));
        }

        /* 후속 투사체 노드가 지정한 발사 지연을 반환한다. */
        public float ResolveFollowUpProjectileDelay()
        {
            return Math.Max(0f, LastFor("FollowUpProjectile", 2, 0f));
        }

        /* 후속 투사체 노드가 지정한 피해 배율을 반환한다. */
        public float ResolveFollowUpProjectileMultiplier()
        {
            return Math.Max(0f, LastFor("FollowUpProjectile", 3, 1f));
        }

        /* 대상의 지정 상태 스택에 비례하는 피해율 보너스를 계산한다. */
        public float ResolveTargetStatusStackDamageRateBonus(string statusId)
        {
            float result = 0f;
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id
                        == "TargetStatusStackDamageRateBonus"
                    && nodes[index].arg_1 == statusId)
                {
                    ReportConsumed(nodes[index]);
                    result += Number(nodes[index].arg_2, 0f);
                }
            }
            return result;
        }

        /* 관통 횟수 보너스 노드의 정수 합을 반환한다. */
        public int ResolvePierceBonus()
        {
            return IntegerSum("PierceBonus");
        }

        /* 적중 대상 수 보너스 노드의 정수 합을 반환한다. */
        public int ResolveHitTargetCountBonus()
        {
            return IntegerSum("HitTargetCountBonus");
        }

        /* 대상별 반복 실행 횟수를 반환한다. */
        public int ResolveRepeatCount()
        {
            return IntegerSum("RepeatPerTarget");
        }

        /* 대상별 반복 실행 사이의 간격을 반환한다. */
        public float ResolveRepeatInterval()
        {
            return LastFor("RepeatPerTarget", 2, 0f);
        }

        /* 대상별 반복 실행에 사용할 피해 배율을 반환한다. */
        public float ResolveRepeatDamageMultiplier()
        {
            return LastFor("RepeatPerTarget", 3, 1f);
        }

        /* 발사 간격 배율 노드의 누적 곱을 반환한다. */
        public float ResolveShotIntervalMultiplier()
        {
            return Product("ShotIntervalMultiplier");
        }

        /* 재사용 대기시간 배율 노드의 누적 곱을 반환한다. */
        public float ResolveCooldownMultiplier()
        {
            return Product("CooldownMultiplier");
        }

        /* 재장전 시간 배율 노드의 누적 곱을 반환한다. */
        public float ResolveReloadMultiplier()
        {
            return Product("ReloadTimeMultiplier");
        }

        /* 재사용 대기시간 환급 노드의 비율을 합산해 유효 범위로 제한한다. */
        public float ResolveCooldownRefundRatio()
        {
            return Math.Min(
                1f,
                Math.Max(
                    0f,
                    Sum("CooldownRefund") + Sum("CooldownRefundBonus")));
        }

        /* 조건을 충족한 재사용 대기시간 초기화 노드가 있는지 확인한다. */
        public bool ShouldResetCooldown()
        {
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id == "CooldownReset"
                    && string.Equals(
                        nodes[index].arg_1,
                        "true",
                        StringComparison.OrdinalIgnoreCase))
                {
                    ReportConsumed(nodes[index]);
                    return true;
                }
            }
            return false;
        }

        /* 보호막 양 배율 노드의 누적 곱을 반환한다. */
        public float ResolveShieldMultiplier()
        {
            return Product("ShieldAmountMultiplier");
        }

        /* 지정 트리거에 적용되는 발동 확률 보너스를 합산한다. */
        public float ResolveTriggerProcChanceBonus(string triggerId)
        {
            float result = 0f;
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id == "TriggerProcChanceBonus"
                    && nodes[index].arg_1 == triggerId)
                {
                    ReportConsumed(nodes[index]);
                    result += Number(nodes[index].arg_2, 0f);
                }
            }
            return result;
        }

        internal IReadOnlyList<ChoiceNodeDefinition> Nodes => nodes;

        /* 지정 노드 타입의 첫 번째 인자를 정수로 변환해 합산한다. */
        private int IntegerSum(string nodeTypeId)
        {
            int result = 0;
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id == nodeTypeId)
                {
                    ReportConsumed(nodes[index]);
                    result += (int)FirstNumber(nodes[index], 0f);
                }
            }
            return result;
        }

        /* 지정 노드 타입의 첫 번째 수치 인자를 모두 곱한다. */
        private float Product(string nodeTypeId)
        {
            float result = 1f;
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id == nodeTypeId)
                {
                    ReportConsumed(nodes[index]);
                    result *= FirstNumber(nodes[index], 1f);
                }
            }
            return result;
        }

        /* 지정 노드 타입에서 마지막으로 발견된 인자 값을 반환한다. */
        private float LastFor(string nodeTypeId, int argument, float fallback)
        {
            for (int index = nodes.Count - 1; index >= 0; index--)
            {
                if (nodes[index].node_type_id == nodeTypeId)
                {
                    ReportConsumed(nodes[index]);
                    string[] args =
                    {
                        nodes[index].arg_1, nodes[index].arg_2, nodes[index].arg_3
                    };
                    return Number(args[argument - 1], fallback);
                }
            }
            return fallback;
        }

        /* 지정 노드 타입의 첫 번째 수치 인자를 모두 더한다. */
        private float Sum(string nodeTypeId)
        {
            float result = 0f;
            for (int index = 0; index < nodes.Count; index++)
            {
                if (string.Equals(nodes[index].node_type_id, nodeTypeId, StringComparison.Ordinal))
                {
                    ReportConsumed(nodes[index]);
                    result += LastNumber(nodes[index], 0f);
                }
            }

            return result;
        }

        /* 선택 노드의 대상 조건이 지정 유닛 상태와 일치하는지 확인한다. */
        private static bool MatchesCondition(ChoiceNodeDefinition node, UnitBaseModel target)
        {
            if (!string.Equals(node.node_type_id, "ConditionalDamageMultiplier", StringComparison.Ordinal)
                && !string.Equals(node.node_type_id, "TargetStatusStackDamageMultiplier", StringComparison.Ordinal)
                && !string.Equals(node.node_type_id, "CountStatusDamageMultiplier", StringComparison.Ordinal))
            {
                return true;
            }

            string statusId = node.arg_1;
            int minimum = (int)Number(node.arg_2, 1f);
            int stacks = 0;
            for (int index = 0; index < target.StatusEffects.Count; index++)
            {
                if (string.Equals(
                    target.StatusEffects[index].Definition.status_effect_id,
                    statusId,
                    StringComparison.Ordinal))
                {
                    stacks += target.StatusEffects[index].CurrentStacks;
                }
            }

            return stacks >= minimum;
        }

        /* 대상 체력 비율이 처형 조건 노드의 임계값을 충족하는지 확인한다. */
        private bool MatchesExecuteHealth(UnitBaseModel target)
        {
            float threshold = 0f;
            for (int index = 0; index < nodes.Count; index++)
            {
                if (nodes[index].node_type_id == "TargetHealthRatioCondition")
                {
                    ReportConsumed(nodes[index]);
                    threshold = Number(nodes[index].arg_1, threshold);
                }
                else if (nodes[index].node_type_id
                    == "TargetHealthRatioThresholdBonus")
                {
                    ReportConsumed(nodes[index]);
                    threshold += Number(nodes[index].arg_1, 0f);
                }
            }
            return threshold > 0f
                && target.CurrentHealth / target.MaximumHealth <= threshold;
        }

        /* 현재 계획에 대상 체력 임계값 조건이 포함됐는지 확인한다. */
        private bool RequiresExecuteThreshold()
        {
            return skill.Columns.TryGetValue(
                    "require_execute_threshold_to_cast",
                    out object requireValue)
                && requireValue is bool requireExecute
                && requireExecute;
        }

        /* 지정 대상이 현재 계획의 실행 체력 조건을 충족하는지 반환한다. */
        public bool IsExecuteConditionMet(UnitBaseModel target)
        {
            return MatchesExecuteHealth(target);
        }

        /* 대상 술어 이름을 실제 유닛 상태와 비교한다. */
        private static bool MatchesPredicate(
            UnitBaseModel target,
            string predicate)
        {
            if (predicate == "is_boss")
            {
                return target.Definition.Columns.TryGetValue(
                    "is_boss",
                    out object value)
                    && value is bool flag
                    && flag;
            }
            return false;
        }

        /* 시전자와 같은 진영 유닛이 보유한 지정 상태 수를 합산한다. */
        private int CountSideStatus(string statusId)
        {
            int count = 0;
            bool casterEnemy = caster is EnemyModel;
            for (int index = 0; index < units.Count; index++)
            {
                UnitBaseModel unit = units[index];
                if (unit != null
                    && (unit is EnemyModel) == casterEnemy
                    && HasStatus(unit, statusId, 1))
                {
                    count++;
                }
            }
            return count;
        }

        /* 유닛의 상태 효과 목록에 지정 상태가 존재하는지 확인한다. */
        private static bool HasStatus(
            UnitBaseModel target,
            string statusId,
            int minimum)
        {
            if (string.IsNullOrEmpty(statusId))
            {
                return false;
            }
            if (string.Equals(
                    statusId,
                    "shield",
                    StringComparison.OrdinalIgnoreCase))
            {
                return target != null && target.CurrentShield > 0f;
            }
            int stacks = 0;
            for (int index = 0; index < target.StatusEffects.Count; index++)
            {
                if (target.StatusEffects[index].Definition.status_effect_id
                    == statusId)
                {
                    stacks += target.StatusEffects[index].CurrentStacks;
                }
            }
            return stacks >= minimum;
        }

        /* 노드의 첫 번째 인자를 실수로 읽는다. */
        private static float FirstNumber(ChoiceNodeDefinition node, float fallback)
        {
            return Number(node.arg_1, fallback);
        }

        /* 노드 인자 중 마지막 양수 값을 반환한다. */
        private static float LastPositiveNumber(ChoiceNodeDefinition node, float fallback)
        {
            float value = LastNumber(node, fallback);
            return value > 0f ? value : fallback;
        }

        /* 노드 인자 중 마지막 유효 수치 값을 반환한다. */
        private static float LastNumber(ChoiceNodeDefinition node, float fallback)
        {
            string[] values =
            {
                node.arg_12, node.arg_11, node.arg_10, node.arg_9,
                node.arg_8, node.arg_7, node.arg_6, node.arg_5,
                node.arg_4, node.arg_3, node.arg_2, node.arg_1
            };
            for (int index = 0; index < values.Length; index++)
            {
                if (float.TryParse(
                    values[index],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float number))
                {
                    return number;
                }
            }

            return fallback;
        }

        /* 문자열을 고정 문화권 실수로 변환하고 실패하면 기본값을 반환한다. */
        private static float Number(string text, float fallback)
        {
            return float.TryParse(
                text,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out float number)
                ? number
                : fallback;
        }

        /* 실제 사용된 선택 노드를 계약 검증 콜백에 보고한다. */
        internal void ReportConsumed(ChoiceNodeDefinition node)
        {
            nodeConsumed?.Invoke(node);
        }
    }
}
