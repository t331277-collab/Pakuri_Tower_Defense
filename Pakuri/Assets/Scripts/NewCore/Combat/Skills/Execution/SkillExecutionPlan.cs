using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    internal sealed class SkillExecutionPlan
    {
        private readonly List<ChoiceNodeDefinition> nodes;
        private readonly UnitBaseModel caster;
        private readonly SkillDefinition skill;
        private readonly IReadOnlyList<UnitBaseModel> units;
        private readonly Action<ChoiceNodeDefinition> nodeConsumed;

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

        public float ResolveDamageDelayMultiplier()
        {
            return Product("DamageDelayMultiplier");
        }

        public float ResolveKnockbackMultiplier()
        {
            return Product("KnockbackDistanceMultiplier");
        }

        public int ResolveMagazineBonus()
        {
            return IntegerSum("MagazineBonus");
        }

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

        public int ResolveAdditionalProjectiles()
        {
            return IntegerSum("AdditionalProjectileBonus");
        }

        public int ResolveFollowUpProjectileCount()
        {
            return Math.Max(0, (int)LastFor("FollowUpProjectile", 1, 0f));
        }

        public float ResolveFollowUpProjectileDelay()
        {
            return Math.Max(0f, LastFor("FollowUpProjectile", 2, 0f));
        }

        public float ResolveFollowUpProjectileMultiplier()
        {
            return Math.Max(0f, LastFor("FollowUpProjectile", 3, 1f));
        }

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

        public int ResolvePierceBonus()
        {
            return IntegerSum("PierceBonus");
        }

        public int ResolveHitTargetCountBonus()
        {
            return IntegerSum("HitTargetCountBonus");
        }

        public int ResolveRepeatCount()
        {
            return IntegerSum("RepeatPerTarget");
        }

        public float ResolveRepeatInterval()
        {
            return LastFor("RepeatPerTarget", 2, 0f);
        }

        public float ResolveRepeatDamageMultiplier()
        {
            return LastFor("RepeatPerTarget", 3, 1f);
        }

        public float ResolveShotIntervalMultiplier()
        {
            return Product("ShotIntervalMultiplier");
        }

        public float ResolveCooldownMultiplier()
        {
            return Product("CooldownMultiplier");
        }

        public float ResolveReloadMultiplier()
        {
            return Product("ReloadTimeMultiplier");
        }

        public float ResolveCooldownRefundRatio()
        {
            return Math.Min(
                1f,
                Math.Max(
                    0f,
                    Sum("CooldownRefund") + Sum("CooldownRefundBonus")));
        }

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

        public float ResolveShieldMultiplier()
        {
            return Product("ShieldAmountMultiplier");
        }

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

        private bool RequiresExecuteThreshold()
        {
            return skill.Columns.TryGetValue(
                    "require_execute_threshold_to_cast",
                    out object requireValue)
                && requireValue is bool requireExecute
                && requireExecute;
        }

        public bool IsExecuteConditionMet(UnitBaseModel target)
        {
            return MatchesExecuteHealth(target);
        }

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

        private static float FirstNumber(ChoiceNodeDefinition node, float fallback)
        {
            return Number(node.arg_1, fallback);
        }

        private static float LastPositiveNumber(ChoiceNodeDefinition node, float fallback)
        {
            float value = LastNumber(node, fallback);
            return value > 0f ? value : fallback;
        }

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

        internal void ReportConsumed(ChoiceNodeDefinition node)
        {
            nodeConsumed?.Invoke(node);
        }
    }
}
