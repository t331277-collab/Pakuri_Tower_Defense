using System;
using Pakuri.NewCore.Definitions.Skills;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
    public static class SkillTriggerSupport
    {
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
                    throw new NotSupportedException(
                        $"Trigger event '{trigger.trigger_event}' is not implemented.");
            }

            string action = Read(trigger, "trigger_action");
            if (string.IsNullOrEmpty(action))
            {
                action = trigger.runtime_kind == "LineAttack"
                    ? "LineAttack"
                    : trigger.runtime_kind == "SingleAttack"
                        ? "SingleAttack"
                        : "TriggeredSkill";
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
                    throw new NotSupportedException(
                        $"Trigger action '{action}' is not implemented.");
            }
        }

        internal static string Read(SkillTriggerDefinition trigger, string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                ? value as string
                : null;
        }

        internal static float Float(SkillTriggerDefinition trigger, string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is float number
                    ? number
                    : 0f;
        }

        internal static int Int(SkillTriggerDefinition trigger, string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is int number
                    ? number
                    : 0;
        }
    }
}
