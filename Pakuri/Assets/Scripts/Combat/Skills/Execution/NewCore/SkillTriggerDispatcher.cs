using System;
using System.Collections.Generic;
using Pakuri.NewCore.Catalog;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public sealed class SkillTriggerDispatcher
    {
        private readonly GameDefinitionCatalog catalog;
        private readonly SkillActorManager actors;
        private readonly EffectManager effects;
        private readonly Func<float> randomValue;
        private readonly SkillEffectGraphRuntime effectGraphs;
        private readonly Dictionary<string, float> cooldowns =
            new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly Dictionary<string, int> counts =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> executing =
            new HashSet<string>(StringComparer.Ordinal);

        public SkillTriggerDispatcher(
            GameDefinitionCatalog catalog,
            SkillActorManager actors,
            EffectManager effects,
            Func<float> randomValue,
            SkillEffectGraphRuntime effectGraphs)
        {
            this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            this.actors = actors ?? throw new ArgumentNullException(nameof(actors));
            this.effects = effects ?? throw new ArgumentNullException(nameof(effects));
            this.randomValue = randomValue ?? throw new ArgumentNullException(nameof(randomValue));
            this.effectGraphs =
                effectGraphs ?? throw new ArgumentNullException(nameof(effectGraphs));
            foreach (SkillTriggerDefinition trigger in catalog.Triggers.Values)
            {
                SkillTriggerSupport.Validate(trigger);
            }
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime));
            }
            string[] keys = new List<string>(cooldowns.Keys).ToArray();
            for (int index = 0; index < keys.Length; index++)
            {
                cooldowns[keys[index]] = Math.Max(0f, cooldowns[keys[index]] - deltaTime);
            }
        }

        public void Reset()
        {
            cooldowns.Clear();
            counts.Clear();
            executing.Clear();
        }

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
                    if (!OwnsTrigger(triggerOwner, trigger)
                        || !MatchesChoices(triggerOwner, trigger)
                        || !MatchesEvent(
                            trigger,
                            eventSkill,
                            eventTarget,
                            eventStatusId,
                            trackedAttribute,
                            eventExecuted,
                            eventSourceSkillId)
                        || !PassesGates(triggerOwner, trigger))
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
                action = trigger.runtime_kind == "LineAttack"
                    ? "LineAttack"
                    : trigger.runtime_kind == "SingleAttack"
                        ? "SingleAttack"
                        : "TriggeredSkill";
            }
            SkillDefinition graphSkill = eventSkill
                ?? catalog.GetSkill(trigger.source_skill_id);
            var graphRequest = new SkillExecutionRequest(
                    owner,
                    graphSkill,
                    units,
                    eventTarget == null
                        ? (CombatVector2?)null
                        : eventTarget.Position - owner.Position,
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
                var cooldowns = owner is MonsterModel monster
                    ? monster.SkillBucket.Cooldowns
                    : ((EnemyModel)owner).SkillBucket.Cooldowns;
                if (string.IsNullOrEmpty(targetSkillId)
                    || !cooldowns.TryGetValue(targetSkillId, out var cooldown))
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
                var triggeredRequest = new SkillExecutionRequest(
                        owner,
                        skill,
                        units,
                        eventTarget == null ? (CombatVector2?)null : eventTarget.Position - owner.Position,
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

        private void CreateTriggerVisual(
            SkillTriggerDefinition trigger,
            UnitBaseModel owner,
            UnitBaseModel target,
            SkillDefinition definition)
        {
            var visual = new EffectVisualSpec(
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
            if (!visual.HasResource)
            {
                return;
            }

            var effect = effects.Create(
                visual,
                target.Position,
                (target.Position - owner.Position).Normalized);
            actors.Register(new BuffActor(
                definition,
                1f,
                effect));
        }

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
            chance = chance > 0f ? Math.Min(1f, chance) : 1f;
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
                        bonus += ParseFloat(node.arg_2);
                    }
                }
            }
            return bonus;
        }

        private static float ParseFloat(string value)
        {
            return float.TryParse(
                value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float parsed)
                ? parsed
                : 0f;
        }

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
            if (!string.IsNullOrEmpty(triggerAttributes)
                && (eventSkill == null
                    || !Contains(
                        triggerAttributes,
                        string.IsNullOrEmpty(eventSkill.attribute)
                            ? "Physical"
                            : eventSkill.attribute)))
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
            if (!string.IsNullOrEmpty(conditionSourceSkill)
                && (string.IsNullOrEmpty(conditionStatus)
                    ? !string.Equals(
                        eventSourceSkillId,
                        conditionSourceSkill,
                        StringComparison.Ordinal)
                    : !HasStatusFromSkill(
                        eventTarget,
                        conditionStatus,
                        conditionSourceSkill,
                        Math.Max(
                            1,
                            SkillTriggerSupport.Int(
                                trigger,
                                "required_source_status_min_stacks")))))
            {
                return false;
            }
            return true;
        }

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
            return int.TryParse(
                value,
                out int parsed)
                    ? Math.Max(0, parsed)
                    : 1;
        }

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

        private static bool ReadBool(
            SkillTriggerDefinition trigger,
            string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is bool flag
                && flag;
        }

        private static int? NullableInt(
            SkillTriggerDefinition trigger,
            string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is int number
                    ? number
                    : (int?)null;
        }

        private static bool Contains(string values, string value)
        {
            string[] split = values.Split(';', ',');
            for (int index = 0; index < split.Length; index++)
                if (split[index].Trim() == value) return true;
            return false;
        }
    }
}
