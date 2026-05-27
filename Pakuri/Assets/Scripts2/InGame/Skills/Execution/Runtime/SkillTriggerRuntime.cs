using System;
using System.Collections;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillTriggerRuntime
    {
        private readonly struct TriggerExecutionContext
        {
            public TriggerExecutionContext(
                BaseUnitRuntimeModel eventTarget,
                BaseUnitRuntimeModel attacker,
                Vector2 eventCenter,
                UnitStatusRuntime status,
                float shieldAbsorbedAmount,
                float eventAppliedDamage,
                DamageAttribute eventAttribute,
                string eventSourceSkillId,
                BaseUnitRuntimeModel eventSource = null,
                bool eventWasExecute = false)
            {
                EventTarget = eventTarget;
                Attacker = attacker;
                EventCenter = eventCenter;
                Status = status;
                ShieldAbsorbedAmount = shieldAbsorbedAmount;
                EventAppliedDamage = eventAppliedDamage;
                EventAttribute = eventAttribute;
                EventSourceSkillId = eventSourceSkillId;
                EventSource = eventSource;
                EventWasExecute = eventWasExecute;
            }

            public BaseUnitRuntimeModel EventTarget { get; }
            public BaseUnitRuntimeModel Attacker { get; }
            public Vector2 EventCenter { get; }
            public UnitStatusRuntime Status { get; }
            public float ShieldAbsorbedAmount { get; }
            public float EventAppliedDamage { get; }
            public DamageAttribute EventAttribute { get; }
            public string EventSourceSkillId { get; }
            public BaseUnitRuntimeModel EventSource { get; }
            public bool EventWasExecute { get; }
        }

        public static void ExecuteProjectileHit(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            bool isMagazineLastProjectile,
            Vector2 eventCenter)
        {
            if (!isMagazineLastProjectile)
            {
                return;
            }

            ExecuteSourceOwnedTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnMagazineLastProjectileHit,
                new TriggerExecutionContext(source, null, eventCenter, null, 0f, 0f, DamageAttribute.Physical, sourceSkillId, source));
        }

        public static void ExecuteShieldExpire(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel shieldTarget,
            UnitStatusRuntime shieldStatus)
        {
            if (shieldTarget == null || shieldStatus == null || !shieldStatus.IsShieldStatus)
            {
                return;
            }

            var source = ResolveSourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
            var sourceSkillId = !string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)
                ? shieldStatus.SourceSkillId
                : string.Empty;
            var center = ResolveUnitPosition(roster, shieldTarget);
            var triggerContext = new TriggerExecutionContext(
                shieldTarget,
                null,
                center,
                shieldStatus,
                0f,
                0f,
                DamageAttribute.Physical,
                sourceSkillId,
                source);
            ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnShieldExpire, triggerContext);
            ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldExpire, triggerContext);
        }

        public static void ExecuteShieldAbsorb(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel shieldTarget,
            BaseUnitRuntimeModel attacker,
            UnitStatusRuntime shieldStatus,
            float absorbedAmount)
        {
            if (shieldTarget == null || shieldStatus == null || !shieldStatus.IsShieldStatus || absorbedAmount <= 0f)
            {
                return;
            }

            var source = ResolveSourceModel(roster, shieldStatus.SourceUnitId, shieldStatus.SourceDefinitionId);
            var sourceSkillId = !string.IsNullOrWhiteSpace(shieldStatus.SourceSkillId)
                ? shieldStatus.SourceSkillId
                : string.Empty;
            var center = attacker != null
                ? ResolveUnitPosition(roster, attacker)
                : ResolveUnitPosition(roster, shieldTarget);
            var triggerContext = new TriggerExecutionContext(
                attacker,
                attacker,
                center,
                shieldStatus,
                absorbedAmount,
                0f,
                DamageAttribute.Physical,
                sourceSkillId,
                source);
            ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
            ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnShieldAbsorb, triggerContext);
        }

        public static void ExecuteStatusExpire(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel statusOwner,
            UnitStatusRuntime status)
        {
            if (statusOwner == null || status == null)
            {
                return;
            }

            var source = ResolveSourceModel(roster, status.SourceUnitId, status.SourceDefinitionId);
            var sourceSkillId = !string.IsNullOrWhiteSpace(status.SourceSkillId)
                ? status.SourceSkillId
                : string.Empty;
            var center = ResolveUnitPosition(roster, statusOwner);
            var triggerContext = new TriggerExecutionContext(
                statusOwner,
                null,
                center,
                status,
                0f,
                0f,
                DamageAttribute.Physical,
                sourceSkillId,
                source);
            ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnStatusExpire, triggerContext);
            ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnStatusExpire, triggerContext);
        }

        public static void ExecuteOutgoingDamage(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            BaseUnitRuntimeModel eventTarget,
            DamageAttribute attribute,
            float eventAppliedDamage,
            bool eventWasExecute = false)
        {
            if (combatManager == null || roster == null || source == null)
            {
                return;
            }

            var center = eventTarget != null
                ? ResolveUnitPosition(roster, eventTarget)
                : ResolveUnitPosition(roster, source);
            var triggerContext = new TriggerExecutionContext(
                eventTarget,
                null,
                center,
                null,
                0f,
                eventAppliedDamage,
                attribute,
                sourceSkillId,
                source,
                eventWasExecute);

            ExecuteSourceOwnedTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnOutgoingDamage,
                triggerContext);
            ExecutePassiveOwnerTriggers(
                combatManager,
                roster,
                SkillTriggerEvent.OnOutgoingDamage,
                triggerContext);
        }

        public static void ExecuteSkillCast(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            Vector2 eventCenter)
        {
            if (combatManager == null || roster == null || source == null)
            {
                return;
            }

            var triggerContext = new TriggerExecutionContext(
                source,
                source,
                eventCenter,
                null,
                0f,
                0f,
                DamageAttribute.Physical,
                sourceSkillId,
                source);
            ExecuteSourceOwnedTriggers(combatManager, roster, source, sourceSkillId, SkillTriggerEvent.OnSkillCast, triggerContext);
            ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnSkillCast, triggerContext);
        }

        public static void ExecuteKill(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            BaseUnitRuntimeModel eventTarget,
            DamageAttribute attribute,
            float eventAppliedDamage,
            bool eventWasExecute = false)
        {
            if (combatManager == null || roster == null || source == null)
            {
                return;
            }

            var center = eventTarget != null
                ? ResolveUnitPosition(roster, eventTarget)
                : ResolveUnitPosition(roster, source);
            var triggerContext = new TriggerExecutionContext(
                eventTarget,
                source,
                center,
                null,
                0f,
                eventAppliedDamage,
                attribute,
                sourceSkillId,
                source,
                eventWasExecute);
            ExecuteSourceOwnedTriggers(
                combatManager,
                roster,
                source,
                sourceSkillId,
                SkillTriggerEvent.OnKill,
                triggerContext);
            ExecutePassiveOwnerTriggers(combatManager, roster, SkillTriggerEvent.OnKill, triggerContext);
        }

        private static void ExecuteSourceOwnedTriggers(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            SkillTriggerEvent triggerEvent,
            TriggerExecutionContext triggerContext)
        {
            if (combatManager == null || roster == null || source == null || string.IsNullOrWhiteSpace(sourceSkillId))
            {
                return;
            }

            var monsterId = source.Identity != null ? source.Identity.DefinitionId : string.Empty;
            var monster = PakuriDataManager.Instance.ResolveMonster(monsterId);
            var triggers = monster != null ? monster.SkillTriggers : null;
            if (triggers == null || triggers.Length == 0)
            {
                return;
            }

            var sourceMonster = source as MonsterUnitRuntimeModel;
            for (var i = 0; i < triggers.Length; i++)
            {
                var trigger = triggers[i];
                if (!ShouldRunSourceOwnedTrigger(trigger, sourceMonster, sourceSkillId, triggerEvent, triggerContext))
                {
                    continue;
                }

                ExecuteTrigger(combatManager, roster, roster.Find(source), source, trigger, triggerContext);
            }
        }

        private static void ExecutePassiveOwnerTriggers(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            SkillTriggerEvent triggerEvent,
            TriggerExecutionContext triggerContext)
        {
            if (combatManager == null || roster == null)
            {
                return;
            }

            var players = roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var ownerEntry = players[i];
                var owner = ownerEntry != null ? ownerEntry.Model as MonsterUnitRuntimeModel : null;
                if (ownerEntry == null || owner == null || owner.State == null || owner.State.LearnedPassiveSkillIds.Count == 0)
                {
                    continue;
                }

                var monsterId = owner.Identity != null ? owner.Identity.DefinitionId : string.Empty;
                var monster = PakuriDataManager.Instance.ResolveMonster(monsterId);
                var triggers = monster != null ? monster.SkillTriggers : null;
                if (triggers == null || triggers.Length == 0)
                {
                    continue;
                }

                for (var j = 0; j < triggers.Length; j++)
                {
                    var trigger = triggers[j];
                    if (!ShouldRunPassiveOwnerTrigger(trigger, owner, triggerEvent, triggerContext))
                    {
                        continue;
                    }

                    if (!PassesCountGate(combatManager, owner, trigger))
                    {
                        continue;
                    }

                    if (!PassesProcGate(combatManager, owner, trigger))
                    {
                        continue;
                    }

                    ExecuteTrigger(combatManager, roster, ownerEntry, owner, trigger, triggerContext);
                }
            }
        }

        private static bool ShouldRunSourceOwnedTrigger(
            SkillTriggerDefinition trigger,
            MonsterUnitRuntimeModel source,
            string sourceSkillId,
            SkillTriggerEvent triggerEvent,
            TriggerExecutionContext triggerContext)
        {
            return trigger != null
                && trigger.TriggerEvent == triggerEvent
                && string.Equals(trigger.SourceSkillId, sourceSkillId, StringComparison.OrdinalIgnoreCase)
                && MatchesEventSkillId(trigger.EventSkillId, triggerContext.EventSourceSkillId)
                && (!trigger.RequireEventExecute || triggerContext.EventWasExecute)
                && HasAllChoices(source, trigger.RequiresActiveChoiceId)
                && !HasAnyChoice(source, trigger.ExcludesActiveChoiceId);
        }

        private static bool ShouldRunPassiveOwnerTrigger(
            SkillTriggerDefinition trigger,
            MonsterUnitRuntimeModel owner,
            SkillTriggerEvent triggerEvent,
            TriggerExecutionContext triggerContext)
        {
            if (trigger == null
                || owner == null
                || owner.State == null
                || trigger.TriggerEvent != triggerEvent
                || string.IsNullOrWhiteSpace(trigger.SourceSkillId)
                || !owner.State.LearnedPassiveSkillIds.Contains(trigger.SourceSkillId)
                || !MatchesEventSkillId(trigger.EventSkillId, triggerContext.EventSourceSkillId)
                || (trigger.RequireEventExecute && !triggerContext.EventWasExecute)
                || !HasAllChoices(owner, trigger.RequiresActiveChoiceId)
                || HasAnyChoice(owner, trigger.ExcludesActiveChoiceId))
            {
                return false;
            }

            if (!MatchesConditionStatus(trigger, triggerContext.Status))
            {
                return false;
            }

            if (!MatchesConditionStatusSourceSkill(trigger.ConditionStatusSourceSkillId, triggerContext.EventTarget))
            {
                return false;
            }

            return MatchesTriggerAttribute(trigger.TriggerAttribute, triggerContext.EventAttribute)
                && MatchesEventSourceScope(trigger.EventSourceScope, owner, triggerContext.EventSource);
        }

        private static bool HasAllChoices(MonsterUnitRuntimeModel source, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (source == null || source.State == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i] != null ? choices[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(choice) && !source.State.ChosenChoiceIds.Contains(choice))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyChoice(MonsterUnitRuntimeModel source, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || source == null || source.State == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i] != null ? choices[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(choice) && source.State.ChosenChoiceIds.Contains(choice))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesConditionStatus(SkillTriggerDefinition trigger, UnitStatusRuntime status)
        {
            return trigger == null || StatusEffectRuntime.MatchesConditionStatus(status, trigger.ConditionStatusId);
        }

        private static bool MatchesTriggerAttribute(string rawAttribute, DamageAttribute eventAttribute)
        {
            if (string.IsNullOrWhiteSpace(rawAttribute))
            {
                return true;
            }

            var tokens = rawAttribute.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] != null ? tokens[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (string.Equals(token, eventAttribute.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool PassesProcGate(InGameCombatManager combatManager, MonsterUnitRuntimeModel owner, SkillTriggerDefinition trigger)
        {
            if (combatManager == null || owner == null || trigger == null)
            {
                return false;
            }

            var chance = trigger.ProcChance > 0f ? Mathf.Clamp01(trigger.ProcChance) : 1f;
            if (chance <= 0f || UnityEngine.Random.value > chance)
            {
                return false;
            }

            return combatManager.ConsumePassiveTriggerCooldown(BuildPassiveTriggerCooldownKey(owner, trigger), trigger.InternalCooldownSeconds);
        }

        private static bool PassesCountGate(InGameCombatManager combatManager, MonsterUnitRuntimeModel owner, SkillTriggerDefinition trigger)
        {
            if (combatManager == null || owner == null || trigger == null)
            {
                return false;
            }

            return combatManager.ConsumePassiveTriggerCount(BuildPassiveTriggerCooldownKey(owner, trigger), trigger.TriggerEveryCount);
        }

        private static bool MatchesEventSourceScope(string scope, MonsterUnitRuntimeModel owner, BaseUnitRuntimeModel eventSource)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return true;
            }

            if (owner == null || eventSource == null)
            {
                return false;
            }

            var normalized = scope.Trim();
            if (string.Equals(normalized, "owner", StringComparison.OrdinalIgnoreCase))
            {
                return IsSameUnit(owner, eventSource);
            }

            if (string.Equals(normalized, "all_allies", StringComparison.OrdinalIgnoreCase))
            {
                return owner.Identity != null
                    && eventSource.Identity != null
                    && owner.Identity.Side == eventSource.Identity.Side;
            }

            return false;
        }

        private static bool MatchesEventSkillId(string rawSkillIds, string eventSkillId)
        {
            if (string.IsNullOrWhiteSpace(rawSkillIds))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(eventSkillId))
            {
                return false;
            }

            var tokens = rawSkillIds.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] != null ? tokens[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(token)
                    && string.Equals(token, eventSkillId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesConditionStatusSourceSkill(string rawSourceSkillId, BaseUnitRuntimeModel target)
        {
            if (string.IsNullOrWhiteSpace(rawSourceSkillId))
            {
                return true;
            }

            var statuses = target != null && target.Statuses != null ? target.Statuses.ActiveStatuses : null;
            if (statuses == null)
            {
                return false;
            }

            var tokens = rawSourceSkillId.Split(';', ',');
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                var sourceData = status != null ? status.SourceData : null;
                var sourceSkillId = sourceData != null ? sourceData.SourceSkillId : string.Empty;
                if (string.IsNullOrWhiteSpace(sourceSkillId))
                {
                    continue;
                }

                for (var j = 0; j < tokens.Length; j++)
                {
                    var token = tokens[j] != null ? tokens[j].Trim() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(token)
                        && string.Equals(token, sourceSkillId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsSameUnit(BaseUnitRuntimeModel left, BaseUnitRuntimeModel right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            var leftId = left != null && left.Identity != null ? left.Identity.UnitId : string.Empty;
            var rightId = right != null && right.Identity != null ? right.Identity.UnitId : string.Empty;
            return !string.IsNullOrWhiteSpace(leftId)
                && string.Equals(leftId, rightId, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildPassiveTriggerCooldownKey(MonsterUnitRuntimeModel owner, SkillTriggerDefinition trigger)
        {
            var unitId = owner != null && owner.Identity != null && !string.IsNullOrWhiteSpace(owner.Identity.UnitId)
                ? owner.Identity.UnitId
                : owner != null ? owner.GetHashCode().ToString() : "unknown";
            var triggerId = trigger != null && !string.IsNullOrWhiteSpace(trigger.TriggerId)
                ? trigger.TriggerId
                : trigger != null ? trigger.SourceSkillId : "unknown";
            return unitId + ":" + triggerId;
        }

        private static void ExecuteTrigger(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            if (trigger == null)
            {
                return;
            }

            var repeatCount = Mathf.Max(1, trigger.RepeatCount);
            for (var i = 0; i < repeatCount; i++)
            {
                var delaySeconds = Mathf.Max(0f, trigger.TriggerDelaySeconds)
                    + (i > 0 ? Mathf.Max(0f, trigger.RepeatIntervalSeconds) * i : 0f);
                if (delaySeconds <= 0f)
                {
                    ExecuteOnce(combatManager, roster, sourceEntry, source, trigger, triggerContext);
                    continue;
                }

                combatManager.StartCoroutine(ExecuteDelayed(
                    combatManager,
                    roster,
                    sourceEntry,
                    source,
                    trigger,
                    triggerContext,
                    delaySeconds));
            }
        }

        private static IEnumerator ExecuteDelayed(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext,
            float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));
            ExecuteOnce(combatManager, roster, sourceEntry, source, trigger, triggerContext);
        }

        private static void ExecuteOnce(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            switch (ResolveTriggerAction(trigger))
            {
                case SkillTriggerActionKind.SingleAttack:
                    ExecuteSingleAttack(combatManager, roster, sourceEntry, source, trigger, triggerContext);
                    return;
                case SkillTriggerActionKind.Effect:
                    ExecuteEffect(combatManager, roster, sourceEntry, trigger, triggerContext);
                    return;
                case SkillTriggerActionKind.CooldownRefund:
                    ReduceTargetCooldown(sourceEntry, trigger);
                    return;
                case SkillTriggerActionKind.ReloadReduce:
                    ReduceTargetReload(sourceEntry, trigger);
                    return;
                default:
                    ExecuteTriggeredSkill(combatManager, sourceEntry, trigger, triggerContext);
                    return;
            }
        }

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

        private static bool ExecuteTriggeredSkill(
            InGameCombatManager combatManager,
            UnitRosterEntry sourceEntry,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            if (combatManager == null
                || sourceEntry == null
                || trigger == null
                || sourceEntry.Model == null
                || sourceEntry.Model.SkillRuntime == null
                || string.IsNullOrWhiteSpace(trigger.TriggeredSkillId))
            {
                return false;
            }

            var runtime = sourceEntry.Model.SkillRuntime.FindBySkillId(trigger.TriggeredSkillId);
            if (runtime == null || runtime.Data == null || !MatchesRuntimeKind(runtime.Data, trigger.RuntimeKind))
            {
                return false;
            }

            var targetPoint = triggerContext.EventTarget != null
                ? triggerContext.EventCenter
                : triggerContext.EventCenter;
            return combatManager.TryExecuteTriggeredSkill(sourceEntry, runtime, targetPoint, true);
        }

        private static bool ExecuteEffect(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            if (combatManager == null
                || roster == null
                || sourceEntry == null
                || trigger == null
                || string.IsNullOrWhiteSpace(trigger.TriggeredEffectId))
            {
                return false;
            }

            var effect = ResolveTriggeredEffect(sourceEntry.Model, trigger.TriggeredEffectId);
            if (effect == null)
            {
                return false;
            }

            var context = new SkillExecutionContext(combatManager, roster, sourceEntry, null, 0f);
            var snapshot = BuildPassiveChoiceSnapshot(sourceEntry.Model as MonsterUnitRuntimeModel, trigger.SourceSkillId);
            return SkillMultiEffectExecutor.Execute(context, snapshot, new[] { effect }, triggerContext.EventCenter);
        }

        private static SkillEffectDefinition ResolveTriggeredEffect(BaseUnitRuntimeModel source, string effectId)
        {
            if (source == null || source.Identity == null || string.IsNullOrWhiteSpace(effectId))
            {
                return null;
            }

            var monster = PakuriDataManager.Instance.ResolveMonster(source.Identity.DefinitionId);
            if (monster == null)
            {
                return null;
            }

            var effect = FindEffect(monster.ActiveSkills, effectId);
            if (effect != null)
            {
                return effect;
            }

            return FindEffect(monster.PassiveSkills, effectId);
        }

        private static SkillEffectDefinition FindEffect(SkillDefinition[] skills, string effectId)
        {
            if (skills == null || string.IsNullOrWhiteSpace(effectId))
            {
                return null;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var effects = skills[i] != null ? skills[i].MultiEffects : null;
                var effect = FindEffect(effects, effectId);
                if (effect != null)
                {
                    return effect;
                }
            }

            return null;
        }

        private static SkillEffectDefinition FindEffect(PassiveDefinition[] skills, string effectId)
        {
            if (skills == null || string.IsNullOrWhiteSpace(effectId))
            {
                return null;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var effects = skills[i] != null ? skills[i].PassiveEffects : null;
                var effect = FindEffect(effects, effectId);
                if (effect != null)
                {
                    return effect;
                }
            }

            return null;
        }

        private static SkillEffectDefinition FindEffect(SkillEffectDefinition[] effects, string effectId)
        {
            if (effects == null || string.IsNullOrWhiteSpace(effectId))
            {
                return null;
            }

            for (var i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                if (effect != null && string.Equals(effect.EffectId, effectId, StringComparison.OrdinalIgnoreCase))
                {
                    return effect;
                }
            }

            return null;
        }

        private static SkillExecutionSnapshot BuildPassiveChoiceSnapshot(MonsterUnitRuntimeModel owner, string passiveId)
        {
            var snapshot = new SkillExecutionSnapshot(null);
            var chosenChoiceIds = owner != null && owner.State != null ? owner.State.ChosenChoiceIds : null;
            if (chosenChoiceIds == null || chosenChoiceIds.Count == 0 || string.IsNullOrWhiteSpace(passiveId))
            {
                return snapshot;
            }

            var manager = PakuriDataManager.Instance;
            foreach (var choiceId in chosenChoiceIds)
            {
                if (manager == null || !manager.TryGetData(choiceId, out SkillChoiceDefinition choice) || choice == null)
                {
                    continue;
                }

                var targetSkillId = !string.IsNullOrWhiteSpace(choice.TargetSkillId)
                    ? choice.TargetSkillId
                    : choice.SkillId;
                if (!string.Equals(targetSkillId, passiveId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                snapshot.ApplyChoiceDefinition(choice);
            }

            return snapshot;
        }

        private static bool ReduceTargetCooldown(UnitRosterEntry sourceEntry, SkillTriggerDefinition trigger)
        {
            var runtime = ResolveTargetRuntime(sourceEntry, trigger);
            if (runtime == null || trigger == null || trigger.CooldownRefundRatio <= 0f)
            {
                return false;
            }

            return runtime.ReduceCooldownRemaining(runtime.EffectiveCooldownDuration * Mathf.Clamp01(trigger.CooldownRefundRatio));
        }

        private static bool ReduceTargetReload(UnitRosterEntry sourceEntry, SkillTriggerDefinition trigger)
        {
            var runtime = ResolveTargetRuntime(sourceEntry, trigger);
            if (runtime == null || trigger == null || trigger.ReloadReduceRatio <= 0f)
            {
                return false;
            }

            return runtime.ReduceReloadRemaining(runtime.ReloadDuration * Mathf.Clamp01(trigger.ReloadReduceRatio));
        }

        private static SkillRuntimeInstance ResolveTargetRuntime(UnitRosterEntry sourceEntry, SkillTriggerDefinition trigger)
        {
            var skillId = trigger != null && !string.IsNullOrWhiteSpace(trigger.TargetSkillId)
                ? trigger.TargetSkillId
                : trigger != null ? trigger.TriggeredSkillId : string.Empty;
            return sourceEntry != null
                && sourceEntry.Model != null
                && sourceEntry.Model.SkillRuntime != null
                && !string.IsNullOrWhiteSpace(skillId)
                    ? sourceEntry.Model.SkillRuntime.FindBySkillId(skillId)
                    : null;
        }

        private static bool ExecuteSingleAttack(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            TriggerExecutionContext triggerContext)
        {
            if (combatManager == null || roster == null || sourceEntry == null)
            {
                return false;
            }

            var targeting = BuildTargeting(trigger);
            var center = ResolveCenter(sourceEntry, roster, triggerContext, trigger, targeting);
            var damage = ResolveDamage(source, trigger, triggerContext);
            if (damage <= 0f)
            {
                return false;
            }

            var damageSourceSkillId = ResolveTriggeredDamageSourceSkillId(trigger);

            if (IsPrefabHitboxTrigger(trigger) && trigger.SkillEffectPrefab != null && combatManager.Effects != null)
            {
                var instance = combatManager.Effects.InstantiateSkillPrefab(trigger.SkillEffectPrefab, center, Quaternion.identity);
                if (instance == null)
                {
                    return false;
                }

                Physics2D.SyncTransforms();
                var routed = ApplyPrefabHitbox(
                    combatManager,
                    sourceEntry,
                    roster,
                    targeting,
                    instance,
                    IsGlobalHitCount(trigger.HitTargetCount) ? int.MaxValue : ParseHitTargetCount(trigger.HitTargetCount),
                    damage,
                    trigger.Attribute,
                    damageSourceSkillId,
                    triggerContext.EventTarget);
                UnityEngine.Object.Destroy(instance, 1f);
                return routed;
            }

            var routedArea = ApplyAreaTrigger(
                combatManager,
                sourceEntry,
                roster,
                targeting,
                center,
                Mathf.Max(0f, trigger.Radius),
                trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield,
                IsGlobalHitCount(trigger.HitTargetCount) ? int.MaxValue : ParseHitTargetCount(trigger.HitTargetCount),
                damage,
                trigger.Attribute,
                damageSourceSkillId,
                triggerContext.EventTarget,
                trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget);
            if (routedArea && trigger.SkillEffectPrefab != null && combatManager.Effects != null)
            {
                SkillVisualSpawnUtility.SpawnTransient(combatManager.Effects, trigger.SkillEffectPrefab, center, Quaternion.identity, 1f);
            }

            return routedArea;
        }

        private static string ResolveTriggeredDamageSourceSkillId(SkillTriggerDefinition trigger)
        {
            if (!string.IsNullOrWhiteSpace(trigger != null ? trigger.TriggeredSkillId : string.Empty))
            {
                return trigger.TriggeredSkillId;
            }

            return trigger != null ? trigger.SourceSkillId : string.Empty;
        }

        private static SkillTargetingSpec BuildTargeting(SkillTriggerDefinition trigger)
        {
            return new SkillTargetingSpec
            {
                TargetSide = trigger.TargetSide == SkillMultiEffectTargetSide.Self
                    ? SkillTargetSide.Self
                    : trigger.TargetSide == SkillMultiEffectTargetSide.AllAllies
                        ? SkillTargetSide.AllAllies
                        : SkillTargetSide.Enemy,
                Selection = trigger.TargetSelection == SkillMultiEffectTargetSelection.Owner
                    ? SkillTargetSelection.Owner
                    : trigger.TargetSelection == SkillMultiEffectTargetSelection.EventTarget
                        ? SkillTargetSelection.HighestHealth
                        : SkillTargetSelection.Nearest,
                Shape = trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield
                    ? SkillTargetShape.Battlefield
                    : trigger.TargetShape == SkillMultiEffectTargetShape.Single
                        ? SkillTargetShape.Single
                        : SkillTargetShape.Circle,
                Radius = trigger.Radius,
                CoverAll = trigger.CoverAll || trigger.TargetShape == SkillMultiEffectTargetShape.Battlefield
            };
        }

        private static Vector2 ResolveCenter(
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            TriggerExecutionContext triggerContext,
            SkillTriggerDefinition trigger,
            SkillTargetingSpec targeting)
        {
            if (trigger != null)
            {
                switch (trigger.CenterMode)
                {
                    case SkillMultiEffectCenterMode.Caster:
                        return sourceEntry.Transform != null ? (Vector2)sourceEntry.Transform.position : triggerContext.EventCenter;
                    case SkillMultiEffectCenterMode.NearestEnemy:
                        var nearest = SkillExecutionUtility.FindNearestTarget(sourceEntry, roster, targeting);
                        return nearest != null && nearest.Transform != null ? (Vector2)nearest.Transform.position : triggerContext.EventCenter;
                    case SkillMultiEffectCenterMode.EffectTarget:
                        return ResolveUnitPosition(roster, triggerContext.EventTarget);
                }
            }

            return triggerContext.EventCenter;
        }

        private static float ResolveDamage(BaseUnitRuntimeModel source, SkillTriggerDefinition trigger, TriggerExecutionContext triggerContext)
        {
            switch (trigger.DamageSource)
            {
                case SkillTriggerDamageSource.ShieldAppliedAmount:
                    return Mathf.Max(0f, triggerContext.Status != null ? triggerContext.Status.AppliedShieldAmount : 0f)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.ShieldRemainingAmount:
                    return Mathf.Max(0f, triggerContext.Status != null ? triggerContext.Status.RemainingShieldAmount : 0f)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.ShieldAbsorbedAmount:
                    return Mathf.Max(0f, triggerContext.ShieldAbsorbedAmount)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.TrackedIncomingDamage:
                    return Mathf.Max(0f, triggerContext.Status != null
                            ? triggerContext.Status.GetTrackedIncomingDamage(ResolveTrackedAttribute(trigger))
                            : 0f)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                case SkillTriggerDamageSource.EventAppliedDamage:
                    return Mathf.Max(0f, triggerContext.EventAppliedDamage)
                        * Mathf.Max(0f, trigger.DamageSourceMultiplier)
                        * Mathf.Max(0f, trigger.DamageMultiplier);
                default:
                    var useSpellPower = Mathf.Abs(trigger.SpellPowerCoefficient) >= Mathf.Abs(trigger.AttackPowerCoefficient);
                    var damageSpec = new SkillDamageSpec
                    {
                        Element = (ElementType)(int)trigger.Attribute,
                        BaseDamage = trigger.BaseDamage,
                        StatCoefficient = useSpellPower ? trigger.SpellPowerCoefficient : trigger.AttackPowerCoefficient,
                        StatSource = useSpellPower ? StatSource.Intelligence : StatSource.Attack,
                        CriticalAllowed = true
                    };
                    return SkillExecutionUtility.ResolveDamage(source, damageSpec, null) * Mathf.Max(0f, trigger.DamageMultiplier);
            }
        }

        private static bool ApplyPrefabHitbox(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            GameObject hitboxObject,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            string sourceSkillId,
            BaseUnitRuntimeModel preferredTarget)
        {
            if (manager == null || sourceEntry == null || roster == null || hitboxObject == null || maxTargets <= 0)
            {
                return false;
            }

            var hitboxColliders = hitboxObject.GetComponentsInChildren<Collider2D>();
            if (hitboxColliders == null || hitboxColliders.Length == 0)
            {
                return false;
            }

            var targets = ResolveOrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferredTarget != null);
            var routed = false;
            var hitCount = 0;
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (!IsTargetInsideHitbox(hitboxColliders, target))
                {
                    continue;
                }

                manager.ApplyDamage(target.Model, damage, attribute, sourceEntry.Model, true, 0f, 0f, sourceSkillId);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            return routed;
        }

        private static List<UnitRosterEntry> ResolveOrderedTargets(
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            BaseUnitRuntimeModel preferredTarget,
            bool preferEventTarget)
        {
            var candidates = SkillExecutionUtility.ResolveTargetList(sourceEntry, roster, targeting);
            var targets = new List<UnitRosterEntry>();
            for (var i = 0; i < candidates.Count; i++)
            {
                var target = candidates[i];
                if (target != null && target.IsAlive && target.Model != null && target.Transform != null)
                {
                    targets.Add(target);
                }
            }

            targets.Sort((left, right) =>
            {
                if (preferEventTarget)
                {
                    var leftPreferred = MatchesModel(left, preferredTarget);
                    var rightPreferred = MatchesModel(right, preferredTarget);
                    if (leftPreferred != rightPreferred)
                    {
                        return leftPreferred ? -1 : 1;
                    }
                }

                var leftDistance = ResolveDistanceSquared(sourceEntry, left);
                var rightDistance = ResolveDistanceSquared(sourceEntry, right);
                return leftDistance.CompareTo(rightDistance);
            });
            return targets;
        }

        private static bool ApplyAreaTrigger(
            InGameCombatManager manager,
            UnitRosterEntry sourceEntry,
            UnitRosterService roster,
            SkillTargetingSpec targeting,
            Vector2 center,
            float radius,
            bool coverAll,
            int maxTargets,
            float damage,
            DamageAttribute attribute,
            string sourceSkillId,
            BaseUnitRuntimeModel preferredTarget,
            bool preferEventTarget)
        {
            if (manager == null || sourceEntry == null || roster == null || maxTargets <= 0)
            {
                return false;
            }

            var targets = ResolveOrderedTargets(sourceEntry, roster, targeting, preferredTarget, preferEventTarget);
            if (!coverAll && radius <= 0f)
            {
                var target = preferEventTarget ? FindPreferredEntry(roster, preferredTarget) : (targets.Count > 0 ? targets[0] : null);
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    return false;
                }

                manager.ApplyDamage(target.Model, damage, attribute, sourceEntry.Model, true, 0f, 0f, sourceSkillId);
                return true;
            }

            var routed = false;
            var hitCount = 0;
            var radiusSq = Mathf.Max(0f, radius) * Mathf.Max(0f, radius);
            for (var i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null || target.Transform == null)
                {
                    continue;
                }

                if (!coverAll)
                {
                    var offset = (Vector2)target.Transform.position - center;
                    if (offset.sqrMagnitude > radiusSq)
                    {
                        continue;
                    }
                }

                manager.ApplyDamage(target.Model, damage, attribute, sourceEntry.Model, true, 0f, 0f, sourceSkillId);
                routed = true;
                hitCount++;
                if (hitCount >= maxTargets)
                {
                    break;
                }
            }

            return routed;
        }

        private static UnitRosterEntry FindPreferredEntry(UnitRosterService roster, BaseUnitRuntimeModel preferredTarget)
        {
            return preferredTarget != null && roster != null ? roster.Find(preferredTarget) : null;
        }

        private static bool MatchesModel(UnitRosterEntry entry, BaseUnitRuntimeModel preferredTarget)
        {
            return entry != null && preferredTarget != null && entry.Model == preferredTarget;
        }

        private static DamageAttribute ResolveTrackedAttribute(SkillTriggerDefinition trigger)
        {
            if (trigger == null)
            {
                return DamageAttribute.Physical;
            }

            return trigger.TrackedAttribute == DamageAttribute.Physical && trigger.Attribute != DamageAttribute.Physical
                ? trigger.Attribute
                : trigger.TrackedAttribute;
        }

        private static bool IsTargetInsideHitbox(Collider2D[] hitboxColliders, UnitRosterEntry target)
        {
            return UnitHitboxUtility.IsTargetInsideHitbox(hitboxColliders, target);
        }

        private static BaseUnitRuntimeModel ResolveSourceModel(UnitRosterService roster, string sourceUnitId, string sourceDefinitionId)
        {
            if (roster == null)
            {
                return null;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i] != null ? entries[i].Model : null;
                var identity = model != null ? model.Identity : null;
                if (identity == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sourceUnitId) && string.Equals(identity.UnitId, sourceUnitId, StringComparison.OrdinalIgnoreCase))
                {
                    return model;
                }
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var model = entries[i] != null ? entries[i].Model : null;
                var identity = model != null ? model.Identity : null;
                if (identity != null
                    && !string.IsNullOrWhiteSpace(sourceDefinitionId)
                    && string.Equals(identity.DefinitionId, sourceDefinitionId, StringComparison.OrdinalIgnoreCase))
                {
                    return model;
                }
            }

            return null;
        }

        private static Vector2 ResolveUnitPosition(UnitRosterService roster, BaseUnitRuntimeModel model)
        {
            var entry = roster != null ? roster.Find(model) : null;
            return entry != null && entry.Transform != null ? (Vector2)entry.Transform.position : Vector2.zero;
        }

        private static float ResolveDistanceSquared(UnitRosterEntry sourceEntry, UnitRosterEntry target)
        {
            if (sourceEntry == null || sourceEntry.Transform == null || target == null || target.Transform == null)
            {
                return float.MaxValue;
            }

            var offset = target.Transform.position - sourceEntry.Transform.position;
            offset.z = 0f;
            return offset.sqrMagnitude;
        }

        private static bool IsPrefabHitboxTrigger(SkillTriggerDefinition trigger)
        {
            return trigger != null && trigger.SkillEffectPrefab != null;
        }

        private static bool IsGlobalHitCount(string rawValue)
        {
            return string.Equals(rawValue, "global", StringComparison.OrdinalIgnoreCase)
                || string.Equals(rawValue, "all", StringComparison.OrdinalIgnoreCase);
        }

        private static int ParseHitTargetCount(string rawValue)
        {
            return int.TryParse(rawValue, out var count) ? Mathf.Max(1, count) : 1;
        }

        private static bool MatchesRuntimeKind(SkillData data, SkillRuntimeKind runtimeKind)
        {
            switch (runtimeKind)
            {
                case SkillRuntimeKind.MagazineProjectile:
                case SkillRuntimeKind.CooldownProjectile:
                    return data is ProjectileSkillData;
                case SkillRuntimeKind.LineAttack:
                    return data is BeamSkillData;
                case SkillRuntimeKind.SingleAttack:
                    return data is SingleAttackData;
                case SkillRuntimeKind.AreaAttack:
                case SkillRuntimeKind.Field:
                case SkillRuntimeKind.Mark:
                case SkillRuntimeKind.Execute:
                    return data is ZoneSkillData;
                case SkillRuntimeKind.Buff:
                case SkillRuntimeKind.Heal:
                    return data is BuffSkillData;
                case SkillRuntimeKind.Shield:
                    return data is ShieldSkillData;
                case SkillRuntimeKind.Passive:
                    return data is PassiveSkillData;
                default:
                    return false;
            }
        }
    }
}
