using System;
using Pakuri.Data;

namespace Pakuri.InGame
{
    internal static class SkillPlanActionDispatcher
    {
        public static bool ExecuteEffect(
            SkillExecutionContext context,
            SkillExecutionSnapshot snapshot,
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
                    return SkillMultiEffectExecutor.ExecuteDamageEffectAction(context, snapshot, effect, fallbackCenter);
                case SkillMultiEffectKind.Status:
                    return SkillMultiEffectExecutor.ExecuteStatusEffectAction(context, snapshot, effect, fallbackCenter, scaleStatusDurationWithSnapshot);
                case SkillMultiEffectKind.ExtendStatusDuration:
                    return SkillMultiEffectExecutor.ExecuteExtendStatusDurationEffectAction(context, effect);
            }

            return false;
        }

        public static bool ExecuteTriggerAction(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            SkillTriggerDefinition trigger,
            SkillTriggerRuntime.TriggerExecutionContext triggerContext)
        {
            switch (ResolveTriggerAction(trigger))
            {
                case SkillTriggerActionKind.SingleAttack:
                    return SkillTriggerRuntime.ExecuteSingleAttackAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
                case SkillTriggerActionKind.LineAttack:
                    return SkillTriggerRuntime.ExecuteLineAttackAction(combatManager, roster, sourceEntry, source, trigger, triggerContext);
                case SkillTriggerActionKind.Effect:
                    return SkillTriggerRuntime.ExecuteEffectAction(combatManager, roster, sourceEntry, trigger, triggerContext);
                case SkillTriggerActionKind.CooldownRefund:
                    return SkillTriggerRuntime.ReduceTargetCooldownAction(roster, sourceEntry, trigger);
                case SkillTriggerActionKind.ReloadReduce:
                    return SkillTriggerRuntime.ReduceTargetReloadAction(roster, sourceEntry, trigger);
                default:
                    return SkillTriggerRuntime.ExecuteTriggeredSkillAction(combatManager, sourceEntry, trigger, triggerContext);
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

        public static SkillEffectDefinition[] ResolveEffects(
            SkillExecutionSnapshot snapshot,
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

        public static SkillTriggerDefinition[] ResolveTriggers(
            SkillExecutionSnapshot snapshot,
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
