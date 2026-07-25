using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

/* 전투 이벤트와 학습 노드를 검사해 스킬 트리거를 예약하고 실행한다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public enum SkillTriggerEvaluationResult
    {
        Matched,
        MissingOwnership,
        ChoiceMismatch,
        EventMismatch,
        GateRejected
    }

    public class SkillTriggerDispatcher
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly SkillActorManager actors;
        private readonly EffectManager effects;
        private readonly Func<float> randomValue;
        private readonly SkillEffectGraphRuntime effectGraphs;
        private readonly Action<Definitions.Choices.ChoiceNodeDefinition>
            nodeConsumed;
        private readonly Dictionary<string, float> cooldowns =
            new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> counts =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> executing =
            new HashSet<string>(StringComparer.Ordinal);

        /* 트리거 실행에 필요한 카탈로그와 런타임 서비스를 저장한다. */
        public SkillTriggerDispatcher(
            GameDefinitionCatalog catalog,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue,
            SkillEffectGraphRuntime effectGraphs,
            Action<Definitions.Choices.ChoiceNodeDefinition>
                nodeConsumed = null)
        {
            this.catalog = catalog;
            this.actors = actors;
            this.effects = effects;
            this.randomValue = randomValue;
            this.effectGraphs =
                effectGraphs;
            this.nodeConsumed = nodeConsumed;
            foreach (SkillTriggerDefinition trigger in catalog.Triggers.Values)
            {
                SkillTriggerSupport.Validate(trigger);
            }
        }

        public event Action<
            SkillTriggerDefinition,
            UnitBaseModel,
            SkillTriggerEvaluationResult>
            TriggerEvaluated;

        /* 경과 시간만큼 트리거별 내부 재사용 대기시간을 갱신한다. */
        public void Tick(float deltaTime)
        {
            string[] keys = new List<string>(cooldowns.Keys).ToArray();
            for (int index = 0; index < keys.Length; index++)
            {
                cooldowns[keys[index]] = Math.Max(0f, cooldowns[keys[index]] - deltaTime);
            }
        }

        /* 전투별 트리거 재사용 대기시간, 횟수, 실행 중 상태를 초기화한다. */
        public void Reset()
        {
            cooldowns.Clear();
            counts.Clear();
            executing.Clear();
        }

        /* 전투 이벤트와 일치하는 소유 트리거를 평가하고 예약하며 개수를 반환한다. */
        public int Dispatch(
            string eventName,
            UnitBaseModel owner,
            SkillDefinition eventSkill,
            UnitBaseModel eventTarget,
            IReadOnlyList<UnitBaseModel> units,
            InGameCombatManager combat,
            string eventStatusId = null,
            float eventAppliedDamage = 0f,
            float eventShieldAbsorbed = 0f,
            float eventShieldApplied = 0f,
            float trackedIncomingDamage = 0f,
            string trackedAttribute = null,
            bool eventExecuted = true,
            string eventSourceSkillId = null,
            IReadOnlyCollection<string> triggerAncestry = null)
        {
            int executedCount = 0;
            foreach (SkillTriggerDefinition trigger in catalog.Triggers.Values)
            {
                if (trigger.trigger_event != eventName)
                {
                    continue;
                }
                if (ContainsTrigger(
                    triggerAncestry,
                    trigger.trigger_id))
                {
                    continue;
                }
                List<UnitBaseModel> candidateOwners =
                    ResolveEventOwners(trigger, owner, units);
                for (int ownerIndex = 0;
                    ownerIndex < candidateOwners.Count;
                    ownerIndex++)
                {
                    UnitBaseModel triggerOwner = candidateOwners[ownerIndex];
                    SkillTriggerEvaluationResult evaluation;
                    if (!OwnsTrigger(triggerOwner, trigger))
                    {
                        evaluation =
                            SkillTriggerEvaluationResult.MissingOwnership;
                    }
                    else if (!MatchesChoices(triggerOwner, trigger))
                    {
                        evaluation =
                            SkillTriggerEvaluationResult.ChoiceMismatch;
                    }
                    else if (!MatchesEvent(
                            trigger,
                            eventSkill,
                            eventTarget,
                            eventStatusId,
                            trackedAttribute,
                            eventExecuted,
                            eventSourceSkillId))
                    {
                        evaluation =
                            SkillTriggerEvaluationResult.EventMismatch;
                    }
                    else if (!PassesGates(triggerOwner, trigger))
                    {
                        evaluation =
                            SkillTriggerEvaluationResult.GateRejected;
                    }
                    else
                    {
                        evaluation =
                            SkillTriggerEvaluationResult.Matched;
                    }
                    TriggerEvaluated?.Invoke(
                        trigger,
                        triggerOwner,
                        evaluation);
                    if (evaluation
                        != SkillTriggerEvaluationResult.Matched)
                    {
                        continue;
                    }

                    string key = triggerOwner.GetHashCode()
                        + ":" + trigger.trigger_id;
                    if (!executing.Add(key))
                    {
                        continue;
                    }
                    try
                    {
                        Schedule(
                            trigger,
                            triggerOwner,
                            eventSkill,
                            eventTarget,
                            units,
                            combat,
                            eventAppliedDamage,
                            eventShieldAbsorbed,
                            eventShieldApplied,
                            trackedIncomingDamage,
                            trackedAttribute,
                            eventExecuted,
                            triggerAncestry,
                            key);
                        executedCount++;
                    }
                    catch
                    {
                        executing.Remove(key);
                        throw;
                    }
                }
            }
            return executedCount;
        }

        /* 트리거 지연과 반복 설정에 따라 실행 콜백을 Actor에 등록한다. */
        private void Schedule(
            SkillTriggerDefinition trigger,
            UnitBaseModel owner,
            SkillDefinition eventSkill,
            UnitBaseModel eventTarget,
            IReadOnlyList<UnitBaseModel> units,
            InGameCombatManager combat,
            float eventAppliedDamage,
            float eventShieldAbsorbed,
            float eventShieldApplied,
            float trackedIncomingDamage,
            string trackedAttribute,
            bool eventExecuted,
            IReadOnlyCollection<string> triggerAncestry,
            string executionKey)
        {
            int repeats = Math.Max(1, SkillTriggerSupport.Int(trigger, "repeat_count"));
            float interval = SkillTriggerSupport.Float(trigger, "repeat_interval_seconds");
            float delay = SkillTriggerSupport.Float(trigger, "trigger_delay_seconds");
            SkillDefinition source = catalog.GetSkill(trigger.source_skill_id);
            actors.Register(new ScheduledSkillActor(
                source,
                repeats,
                interval,
                repeatIndex =>
                {
                    try
                    {
                        Execute(
                            trigger,
                            owner,
                            eventSkill,
                            eventTarget,
                            units,
                            combat,
                            eventAppliedDamage,
                            eventShieldAbsorbed,
                            eventShieldApplied,
                            trackedIncomingDamage,
                            trackedAttribute,
                            eventExecuted,
                            triggerAncestry);
                    }
                    finally
                    {
                        if (repeatIndex == repeats - 1)
                        {
                            executing.Remove(executionKey);
                        }
                    }
                },
                null,
                delay));
        }

        /* 트리거 동작 종류에 맞춰 효과, 공격, 자원 조정 또는 스킬 발동을 수행한다. */
        private void Execute(
            SkillTriggerDefinition trigger,
            UnitBaseModel owner,
            SkillDefinition eventSkill,
            UnitBaseModel eventTarget,
            IReadOnlyList<UnitBaseModel> units,
            InGameCombatManager combat,
            float eventAppliedDamage,
            float eventShieldAbsorbed,
            float eventShieldApplied,
            float trackedIncomingDamage,
            string trackedAttribute,
            bool eventExecuted,
            IReadOnlyCollection<string> triggerAncestry)
        {
            string action = SkillTriggerSupport.Read(trigger, "trigger_action");
            if (string.IsNullOrEmpty(action))
            {
                action = "TriggeredSkill";
                if (trigger.runtime_kind == "LineAttack")
                {
                    action = "LineAttack";
                }
                else if (trigger.runtime_kind == "SingleAttack")
                {
                    action = "SingleAttack";
                }
            }
            SkillDefinition graphSkill = eventSkill
                ?? catalog.GetSkill(trigger.source_skill_id);
            CombatVector2? aimDirection = null;
            if (eventTarget != null)
            {
                aimDirection = eventTarget.Position - owner.Position;
            }
            var graphRequest = new SkillExecutionRequest(
                    owner,
                    graphSkill,
                    units,
                    aimDirection,
                    eventTarget?.Position,
                    true);
            graphRequest.SetEventTarget(eventTarget);
            graphRequest.InheritTriggerAncestry(
                triggerAncestry,
                trigger.trigger_id);
            string graphOwnerId = SkillTriggerSupport.Read(
                trigger,
                "triggered_graph_owner_id");
            if (!string.IsNullOrEmpty(graphOwnerId))
            {
                effectGraphs.ExecuteTriggerGraph(
                    combat,
                    graphRequest,
                    graphOwnerId,
                    SkillTriggerSupport.Read(
                        trigger,
                        "triggered_graph_owner_kind"),
                    SkillTriggerSupport.Read(
                        trigger,
                        "triggered_graph_kind") ?? "Effect",
                    NullableInt(trigger, "triggered_graph_index"));
            }
            if (action == "CooldownRefund" || action == "ReloadReduce")
            {
                string targetSkillId = SkillTriggerSupport.Read(trigger, "target_skill_id");
                if (string.IsNullOrEmpty(targetSkillId))
                {
                    targetSkillId = trigger.triggered_skill_id;
                }
                if (string.IsNullOrEmpty(targetSkillId))
                {
                    targetSkillId = eventSkill?.skill_id;
                }
                if (string.IsNullOrEmpty(targetSkillId))
                {
                    return;
                }
                Runtime.SkillCooldown cooldown = null;
                bool foundCooldown;
                if (owner is MonsterModel monster)
                {
                    foundCooldown = monster.SkillBucket.Cooldowns.TryGetValue(
                        targetSkillId,
                        out cooldown);
                }
                else
                {
                    foundCooldown = ((EnemyModel)owner)
                        .SkillBucket.Cooldowns.TryGetValue(
                            targetSkillId,
                            out cooldown);
                }
                if (!foundCooldown)
                {
                    return;
                }
                if (action == "CooldownRefund")
                {
                    float ratio = SkillTriggerSupport.Float(
                        trigger,
                        "cooldown_refund_ratio");
                    if (ratio <= 0f)
                    {
                        ratio = Math.Min(
                            1f,
                            Math.Max(0f, SkillTriggerSupport.Float(trigger, "damage_multiplier")));
                    }
                    cooldown.ReduceCooldown(
                        ratio);
                }
                else
                {
                    float ratio = SkillTriggerSupport.Float(
                        trigger,
                        "reload_reduce_ratio");
                    if (ratio <= 0f)
                    {
                        ratio = Math.Min(
                            1f,
                            Math.Max(0f, SkillTriggerSupport.Float(trigger, "damage_multiplier")));
                    }
                    cooldown.ReduceReload(
                        ratio);
                }
                return;
            }

            if (!string.IsNullOrEmpty(trigger.triggered_skill_id)
                && catalog.Skills.TryGetValue(trigger.triggered_skill_id, out SkillDefinition skill))
            {
                CombatVector2? triggeredAimDirection = null;
                if (eventTarget != null)
                {
                    triggeredAimDirection =
                        eventTarget.Position - owner.Position;
                }
                var triggeredRequest = new SkillExecutionRequest(
                        owner,
                        skill,
                        units,
                        triggeredAimDirection,
                        eventTarget?.Position,
                        true);
                triggeredRequest.SetEventTarget(eventTarget);
                triggeredRequest.InheritTriggerAncestry(
                    triggerAncestry,
                    trigger.trigger_id,
                    trigger.source_skill_id);
                combat.TryExecuteSkill(triggeredRequest);
                return;
            }

            List<UnitBaseModel> targets = ResolveTargets(
                trigger,
                owner,
                eventTarget,
                units);
            if (targets.Count == 0)
            {
                return;
            }

            string statusId = SkillTriggerSupport.Read(trigger, "triggered_effect_id");
            if (action == "Effect"
                && !string.IsNullOrEmpty(statusId)
                && catalog.Statuses.TryGetValue(statusId, out var status))
            {
                for (int index = 0; index < targets.Count; index++)
                {
                    combat.ApplyStatus(
                        owner,
                        targets[index],
                        status,
                        null,
                        null,
                        trigger.source_skill_id);
                }
                CreateTriggerVisual(
                    trigger,
                    owner,
                    targets[0],
                    graphSkill);
                return;
            }

            float multiplier = SkillTriggerSupport.Float(trigger, "damage_multiplier");
            if (multiplier <= 0f)
            {
                multiplier = 1f;
            }
            float baseDamage = SkillTriggerSupport.Float(trigger, "base_damage");
            float attackCoefficient =
                SkillTriggerSupport.Float(trigger, "attack_power_coefficient");
            float spellCoefficient =
                SkillTriggerSupport.Float(trigger, "spell_power_coefficient");
            string damageSource = SkillTriggerSupport.Read(
                trigger,
                "damage_source");
            float sourceMultiplier = SkillTriggerSupport.Float(
                trigger,
                "damage_source_multiplier");
            if (damageSource == "EventAppliedDamage")
            {
                baseDamage = eventAppliedDamage * sourceMultiplier;
                attackCoefficient = 0f;
                spellCoefficient = 0f;
            }
            else if (damageSource == "ShieldAbsorbedAmount")
            {
                baseDamage = eventShieldAbsorbed * sourceMultiplier;
                attackCoefficient = 0f;
                spellCoefficient = 0f;
            }
            else if (damageSource == "ShieldAppliedAmount")
            {
                baseDamage = eventShieldApplied * sourceMultiplier;
                attackCoefficient = 0f;
                spellCoefficient = 0f;
            }
            else if (damageSource == "TrackedIncomingDamage")
            {
                baseDamage = trackedIncomingDamage * sourceMultiplier;
                attackCoefficient = 0f;
                spellCoefficient = 0f;
            }

            if (baseDamage > 0f
                || attackCoefficient > 0f
                || spellCoefficient > 0f)
            {
                string damageAttribute =
                    SkillTriggerSupport.Read(trigger, "attribute");
                if (string.IsNullOrEmpty(damageAttribute))
                {
                    damageAttribute = SkillTriggerSupport.Read(
                        trigger,
                        "tracked_attribute");
                }
                for (int index = 0; index < targets.Count; index++)
                {
                    combat.ApplyTriggeredDamage(
                        owner,
                        targets[index],
                        trigger.trigger_id,
                        damageAttribute,
                        baseDamage,
                        attackCoefficient,
                        spellCoefficient,
                        multiplier,
                        ExtendTriggerAncestry(
                            triggerAncestry,
                            trigger.trigger_id));
                }
            }
            else if (eventSkill != null && action != "Effect")
            {
                for (int index = 0; index < targets.Count; index++)
                {
                    combat.ApplySkillDamage(
                        owner,
                        targets[index],
                        eventSkill,
                        multiplier);
                }
            }

            CreateTriggerVisual(
                trigger,
                owner,
                targets[0],
                graphSkill);
        }

        /* 트리거가 지정한 런타임 비주얼을 생성하고 생명주기를 등록한다. */
        private void CreateTriggerVisual(
            SkillTriggerDefinition trigger,
            UnitBaseModel owner,
            UnitBaseModel target,
            SkillDefinition definition)
        {
            var visual = new EffectVisualRequest(
                SkillTriggerSupport.Read(
                    trigger,
                    "skill_effect_prefab_path"),
                SkillTriggerSupport.Read(
                    trigger,
                    "runtime_visual_sprite_path"),
                SkillTriggerSupport.Read(
                    trigger,
                    "runtime_visual_animator_controller_path"),
                SkillTriggerSupport.Float(
                    trigger,
                    "runtime_visual_scale"),
                0f,
                0f,
                0f,
                SkillTriggerSupport.Int(
                    trigger,
                    "runtime_visual_sorting_order"));
            if (string.IsNullOrWhiteSpace(visual.PrefabPath)
                && string.IsNullOrWhiteSpace(visual.SpritePath)
                && string.IsNullOrWhiteSpace(
                    visual.AnimatorControllerPath))
            {
                return;
            }

            var effect = effects.Create(
                visual,
                target.Position,
                (target.Position - owner.Position).Normalized);
            actors.RegisterEffectLifetime(
                definition,
                1f,
                effect);
        }

        /* 트리거 횟수, 재사용 대기시간, 확률 제한을 검사하고 상태를 갱신한다. */
        private bool PassesGates(UnitBaseModel owner, SkillTriggerDefinition trigger)
        {
            string key = owner.GetHashCode() + ":" + trigger.trigger_id;
            int every = Math.Max(1, SkillTriggerSupport.Int(trigger, "trigger_every_count"));
            counts.TryGetValue(key, out int count);
            count++;
            counts[key] = count;
            if (count % every != 0 || (cooldowns.TryGetValue(key, out float remaining) && remaining > 0f))
            {
                return false;
            }

            float chance = SkillTriggerSupport.Float(trigger, "proc_chance");
            if (chance > 0f)
            {
                chance = Math.Min(1f, chance);
            }
            else
            {
                chance = 1f;
            }
            chance = Math.Min(
                1f,
                chance + ResolveProcChanceBonus(owner, trigger.trigger_id));
            if (randomValue() > chance)
            {
                return false;
            }

            cooldowns[key] = Math.Max(
                0f,
                trigger.internal_cooldown_seconds ?? 0f);
            return true;
        }

        /* 소유자의 실행 계획에서 지정 트리거 발동 확률 보너스를 계산한다. */
        private float ResolveProcChanceBonus(
            UnitBaseModel owner,
            string triggerId)
        {
            if (!(owner is MonsterModel monster))
            {
                return 0f;
            }
            float bonus = 0f;
            for (int choiceIndex = 0;
                choiceIndex < monster.SkillBucket.SelectedChoices.Count;
                choiceIndex++)
            {
                string choiceId =
                    monster.SkillBucket.SelectedChoices[choiceIndex].choice_id;
                for (int nodeIndex = 0;
                    nodeIndex < catalog.ChoiceNodes.Count;
                    nodeIndex++)
                {
                    var node = catalog.ChoiceNodes[nodeIndex];
                    if (node.owner_id == choiceId
                        && node.node_type_id == "TriggerProcChanceBonus"
                        && node.arg_1 == triggerId)
                    {
                        nodeConsumed?.Invoke(node);
                        bonus += ParseFloat(node.arg_2);
                    }
                }
            }
            return bonus;
        }

        /* 문자열을 고정 문화권 실수로 변환하고 실패하면 0을 반환한다. */
        private static float ParseFloat(string value)
        {
            if (float.TryParse(
                    value,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out float parsed))
            {
                return parsed;
            }
            return 0f;
        }

        /* 유닛의 스킬 버킷이 트리거 원본 스킬을 보유하는지 확인한다. */
        private static bool OwnsTrigger(UnitBaseModel owner, SkillTriggerDefinition trigger)
        {
            if (owner is MonsterModel monster)
            {
                foreach (SkillDefinition skill in monster.SkillBucket.ActiveSkills)
                    if (skill.skill_id == trigger.source_skill_id) return true;
                foreach (PassiveDefinition skill in monster.SkillBucket.PassiveSkills)
                    if (skill.skill_id == trigger.source_skill_id) return true;
            }
            else if (owner is EnemyModel enemy)
            {
                foreach (SkillDefinition skill in enemy.SkillBucket.ActiveSkills)
                    if (skill.skill_id == trigger.source_skill_id) return true;
                foreach (PassiveDefinition skill in enemy.SkillBucket.PassiveSkills)
                    if (skill.skill_id == trigger.source_skill_id) return true;
            }
            return false;
        }

        /* 트리거가 요구하는 선택 노드를 유닛이 학습했는지 확인한다. */
        private static bool MatchesChoices(UnitBaseModel owner, SkillTriggerDefinition trigger)
        {
            if (!(owner is MonsterModel monster)) return true;
            string required = SkillTriggerSupport.Read(trigger, "requires_active_choice_id");
            string excluded = SkillTriggerSupport.Read(trigger, "excludes_active_choice_id");
            HashSet<string> selected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var choice in monster.SkillBucket.SelectedChoices)
            {
                selected.Add(choice.choice_id);
                if (!string.IsNullOrEmpty(excluded)
                    && Contains(excluded, choice.choice_id))
                {
                    return false;
                }
            }
            if (string.IsNullOrEmpty(required))
            {
                return true;
            }

            string[] requiredChoices = required.Split(';', ',');
            for (int index = 0; index < requiredChoices.Length; index++)
            {
                if (!selected.Contains(requiredChoices[index].Trim()))
                {
                    return false;
                }
            }
            return true;
        }

        /* 트리거 정의가 현재 전투 이벤트와 전달된 문맥에 일치하는지 확인한다. */
        private static bool MatchesEvent(
            SkillTriggerDefinition trigger,
            SkillDefinition eventSkill,
            UnitBaseModel eventTarget,
            string eventStatusId,
            string trackedAttribute,
            bool eventExecuted,
            string eventSourceSkillId)
        {
            string eventSkillIds = SkillTriggerSupport.Read(trigger, "event_skill_id");
            if (!string.IsNullOrEmpty(eventSkillIds)
                && (eventSkill == null
                    || !Contains(eventSkillIds, eventSkill.skill_id)))
            {
                return false;
            }

            string runtimeKinds =
                SkillTriggerSupport.Read(trigger, "event_skill_runtime_kinds");
            if (!string.IsNullOrEmpty(runtimeKinds)
                && (eventSkill == null
                    || !MatchesRuntimeKind(
                        runtimeKinds,
                        eventSkill.runtime_kind)))
            {
                return false;
            }

            string triggerAttributes =
                SkillTriggerSupport.Read(trigger, "trigger_attribute");
            string eventAttribute = null;
            if (eventSkill != null)
            {
                eventAttribute = eventSkill.attribute;
                if (string.IsNullOrEmpty(eventAttribute))
                {
                    eventAttribute = "Physical";
                }
            }
            if (!string.IsNullOrEmpty(triggerAttributes)
                && (eventSkill == null
                    || !Contains(triggerAttributes, eventAttribute)))
            {
                return false;
            }

            string requiredTrackedAttribute =
                SkillTriggerSupport.Read(trigger, "tracked_attribute");
            if (string.Equals(
                    SkillTriggerSupport.Read(trigger, "damage_source"),
                    "TrackedIncomingDamage",
                    StringComparison.Ordinal)
                && !string.IsNullOrEmpty(requiredTrackedAttribute)
                && requiredTrackedAttribute != trackedAttribute)
            {
                return false;
            }

            if (ReadBool(trigger, "require_event_execute")
                && !eventExecuted)
            {
                return false;
            }

            string conditionStatus =
                SkillTriggerSupport.Read(trigger, "condition_status_id");
            if (!string.IsNullOrEmpty(conditionStatus)
                && !string.Equals(
                    conditionStatus,
                    eventStatusId,
                    StringComparison.Ordinal))
            {
                return false;
            }

            string conditionSourceSkill = SkillTriggerSupport.Read(
                trigger,
                "condition_status_source_skill_id");
            bool sourceConditionFailed = false;
            if (!string.IsNullOrEmpty(conditionSourceSkill))
            {
                if (string.IsNullOrEmpty(conditionStatus))
                {
                    sourceConditionFailed = !string.Equals(
                        eventSourceSkillId,
                        conditionSourceSkill,
                        StringComparison.Ordinal);
                }
                else
                {
                    sourceConditionFailed = !HasStatusFromSkill(
                        eventTarget,
                        conditionStatus,
                        conditionSourceSkill,
                        Math.Max(
                            1,
                            SkillTriggerSupport.Int(
                                trigger,
                                "required_source_status_min_stacks")));
                }
            }
            if (sourceConditionFailed)
            {
                return false;
            }
            return true;
        }

        /* 이벤트 스킬의 런타임 종류가 트리거 필터와 일치하는지 확인한다. */
        private static bool MatchesRuntimeKind(
            string configuredKinds,
            string runtimeKind)
        {
            if (Contains(configuredKinds, runtimeKind))
            {
                return true;
            }
            if (!Contains(configuredKinds, "Area"))
            {
                return false;
            }
            return string.Equals(
                    runtimeKind,
                    "AreaAttack",
                    StringComparison.Ordinal)
                || string.Equals(
                    runtimeKind,
                    "Field",
                    StringComparison.Ordinal);
        }

        /* 트리거 대상 규칙과 범위, 최대 수를 사용해 실행 대상을 선정한다. */
        private static List<UnitBaseModel> ResolveTargets(
            SkillTriggerDefinition trigger,
            UnitBaseModel owner,
            UnitBaseModel eventTarget,
            IReadOnlyList<UnitBaseModel> units)
        {
            List<UnitBaseModel> result = new List<UnitBaseModel>();
            string selection = trigger.target_selection;
            if (string.Equals(selection, "EventTarget", StringComparison.Ordinal)
                && eventTarget != null
                && eventTarget.IsAlive)
            {
                result.Add(eventTarget);
                return result;
            }
            if (string.Equals(trigger.target_side, "Self", StringComparison.Ordinal))
            {
                result.Add(owner);
                return result;
            }

            CombatVector2 center = ResolveCenter(
                trigger,
                owner,
                eventTarget,
                units);
            float radius = Math.Max(
                0f,
                SkillTriggerSupport.Float(trigger, "radius"));
            for (int index = 0; index < units.Count; index++)
            {
                UnitBaseModel unit = units[index];
                if (unit == null || !unit.IsAlive || ReferenceEquals(unit, owner))
                {
                    continue;
                }
                bool ally = (owner is MonsterModel && unit is MonsterModel)
                    || (owner is EnemyModel && unit is EnemyModel);
                if (string.Equals(trigger.target_side, "Enemy", StringComparison.Ordinal)
                    && ally)
                {
                    continue;
                }
                if (string.Equals(trigger.target_side, "AllAllies", StringComparison.Ordinal)
                    && !ally)
                {
                    continue;
                }
                if (string.Equals(
                        trigger.target_shape,
                        "Circle",
                        StringComparison.Ordinal)
                    && radius > 0f
                    && CombatVector2.Distance(center, unit.Position) > radius)
                {
                    continue;
                }
                result.Add(unit);
            }

            StableSortByDistance(result, center);
            int maximum = ReadHitTargetCount(trigger);
            if (string.Equals(
                    trigger.target_shape,
                    "Single",
                    StringComparison.Ordinal))
            {
                maximum = 1;
            }
            if (maximum < result.Count)
            {
                result.RemoveRange(maximum, result.Count - maximum);
            }
            return result;
        }

        /* 이벤트 소유자 규칙에 맞는 트리거 평가 주체 목록을 만든다. */
        private static List<UnitBaseModel> ResolveEventOwners(
            SkillTriggerDefinition trigger,
            UnitBaseModel eventOwner,
            IReadOnlyList<UnitBaseModel> units)
        {
            List<UnitBaseModel> owners = new List<UnitBaseModel>();
            owners.Add(eventOwner);
            if (SkillTriggerSupport.Read(trigger, "event_source_scope")
                != "all_allies")
            {
                return owners;
            }
            bool ownerEnemy = eventOwner is EnemyModel;
            for (int index = 0; index < units.Count; index++)
            {
                UnitBaseModel candidate = units[index];
                if (candidate != null
                    && candidate.IsAlive
                    && !ReferenceEquals(candidate, eventOwner)
                    && (candidate is EnemyModel) == ownerEnemy)
                {
                    owners.Add(candidate);
                }
            }
            return owners;
        }

        /* 트리거 중심점 규칙에 따라 시전자, 이벤트 대상 또는 지정 위치를 선택한다. */
        private static CombatVector2 ResolveCenter(
            SkillTriggerDefinition trigger,
            UnitBaseModel owner,
            UnitBaseModel eventTarget,
            IReadOnlyList<UnitBaseModel> units)
        {
            if ((trigger.center_mode == "EffectTarget"
                    || trigger.center_mode == "PrimarySkillCenter")
                && eventTarget != null)
            {
                return eventTarget.Position;
            }
            if (trigger.center_mode == "NearestEnemy")
            {
                bool ownerEnemy = owner is EnemyModel;
                UnitBaseModel nearest = null;
                float nearestDistance = float.MaxValue;
                for (int index = 0; index < units.Count; index++)
                {
                    UnitBaseModel unit = units[index];
                    if (unit != null
                        && unit.IsAlive
                        && (unit is EnemyModel) != ownerEnemy)
                    {
                        float distance =
                            (unit.Position - owner.Position).SqrMagnitude;
                        if (distance < nearestDistance)
                        {
                            nearest = unit;
                            nearestDistance = distance;
                        }
                    }
                }
                if (nearest != null)
                {
                    return nearest.Position;
                }
            }
            return owner.Position;
        }

        /* 트리거 열에서 적중 대상 수를 읽고 기본값과 최소값을 적용한다. */
        private static int ReadHitTargetCount(
            SkillTriggerDefinition trigger)
        {
            string value = SkillTriggerSupport.Read(
                trigger,
                "hit_target_count");
            if (string.IsNullOrEmpty(value)
                || string.Equals(
                    value,
                    "global",
                    StringComparison.OrdinalIgnoreCase))
            {
                return int.MaxValue;
            }
            if (int.TryParse(value, out int parsed))
            {
                return Math.Max(0, parsed);
            }
            return 1;
        }

        /* 원래 순서를 보조 기준으로 유지하며 중심점 거리순으로 정렬한다. */
        private static void StableSortByDistance(
            List<UnitBaseModel> units,
            CombatVector2 center)
        {
            for (int index = 1; index < units.Count; index++)
            {
                UnitBaseModel value = units[index];
                float distance = (value.Position - center).SqrMagnitude;
                int insertion = index;
                while (insertion > 0
                    && (units[insertion - 1].Position - center).SqrMagnitude
                        > distance)
                {
                    units[insertion] = units[insertion - 1];
                    insertion--;
                }
                units[insertion] = value;
            }
        }

        /* 대상이 특정 원본 스킬에서 부여된 상태를 보유하는지 확인한다. */
        private static bool HasStatusFromSkill(
            UnitBaseModel unit,
            string statusId,
            string sourceSkillId,
            int minimumStacks)
        {
            if (unit == null) return false;
            int stacks = 0;
            for (int index = 0; index < unit.StatusEffects.Count; index++)
            {
                var status = unit.StatusEffects[index];
                if ((string.IsNullOrEmpty(statusId)
                        || status.Definition.status_effect_id == statusId)
                    && status.SourceSkillId == sourceSkillId)
                {
                    stacks += status.CurrentStacks;
                }
            }
            return stacks >= minimumStacks;
        }

        /* 기존 트리거 경로에 현재 트리거 식별자를 추가해 새 집합을 만든다. */
        private static IReadOnlyCollection<string> ExtendTriggerAncestry(
            IReadOnlyCollection<string> ancestors,
            string triggerId)
        {
            HashSet<string> result =
                new HashSet<string>(StringComparer.Ordinal);
            if (ancestors != null)
            {
                foreach (string ancestor in ancestors)
                {
                    result.Add(ancestor);
                }
            }
            if (!string.IsNullOrEmpty(triggerId))
            {
                result.Add(triggerId);
            }
            return result;
        }

        /* 트리거 경로에 지정 식별자가 이미 포함됐는지 확인한다. */
        private static bool ContainsTrigger(
            IReadOnlyCollection<string> ancestors,
            string triggerId)
        {
            if (ancestors == null)
            {
                return false;
            }
            foreach (string ancestor in ancestors)
            {
                if (ancestor == triggerId)
                {
                    return true;
                }
            }
            return false;
        }

        /* 트리거 열의 논리값을 읽고 값이 없으면 기본값을 반환한다. */
        private static bool ReadBool(
            SkillTriggerDefinition trigger,
            string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is bool flag
                && flag;
        }

        /* 트리거 열의 정수값을 읽고 값이 없으면 null을 반환한다. */
        private static int? NullableInt(
            SkillTriggerDefinition trigger,
            string column)
        {
            if (trigger.Columns.TryGetValue(column, out object value)
                && value is int number)
            {
                return number;
            }
            return null;
        }

        /* 구분자로 나열된 문자열에 지정 값이 포함됐는지 확인한다. */
        private static bool Contains(string values, string value)
        {
            string[] split = values.Split(';', ',');
            for (int index = 0; index < split.Length; index++)
                if (split[index].Trim() == value) return true;
            return false;
        }
    }

    public static class SkillTriggerSupport
    {
        /* 트리거 이벤트와 실행 동작이 현재 런타임에서 지원되는지 검증한다. */
        public static void Validate(SkillTriggerDefinition trigger)
        {
            switch (trigger.trigger_event)
            {
                case "CombatStart":
                case "OnSkillCast":
                case "OnOutgoingDamage":
                case "OnMagazineLastProjectileHit":
                case "OnKill":
                case "OnStatusExpire":
                case "OnShieldExpire":
                case "OnShieldAbsorb":
                    break;
                default:
                    break;
            }

            string action = Read(trigger, "trigger_action");
            if (string.IsNullOrEmpty(action))
            {
                action = "TriggeredSkill";
                if (trigger.runtime_kind == "LineAttack")
                {
                    action = "LineAttack";
                }
                else if (trigger.runtime_kind == "SingleAttack")
                {
                    action = "SingleAttack";
                }
            }

            switch (action)
            {
                case "Effect":
                case "SingleAttack":
                case "LineAttack":
                case "CooldownRefund":
                case "ReloadReduce":
                case "TriggeredSkill":
                    return;
                default:
                    break;
            }
        }

        /* 트리거의 지정 열에서 문자열 값을 읽는다. */
        internal static string Read(SkillTriggerDefinition trigger, string column)
        {
            if (trigger.Columns.TryGetValue(column, out object value))
            {
                return value as string;
            }
            return null;
        }

        /* 트리거의 지정 열에서 실수 값을 읽고 없으면 0을 반환한다. */
        internal static float Float(SkillTriggerDefinition trigger, string column)
        {
            if (trigger.Columns.TryGetValue(column, out object value)
                && value is float number)
            {
                return number;
            }
            return 0f;
        }

        /* 트리거의 지정 열에서 정수 값을 읽고 없으면 0을 반환한다. */
        internal static int Int(SkillTriggerDefinition trigger, string column)
        {
            if (trigger.Columns.TryGetValue(column, out object value)
                && value is int number)
            {
                return number;
            }
            return 0;
        }
    }
}
