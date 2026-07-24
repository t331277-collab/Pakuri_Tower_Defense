using System;
using Pakuri.NewCore.Definitions.Skills;

/* 트리거 정의의 지원 범위를 검증하고 열 값을 타입별로 읽는다. */
namespace Pakuri.NewCore.Combat.Skills.Execution
{
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

        /* 트리거의 지정 열에서 문자열 값을 읽는다. */
        internal static string Read(SkillTriggerDefinition trigger, string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                ? value as string
                : null;
        }

        /* 트리거의 지정 열에서 실수 값을 읽고 없으면 0을 반환한다. */
        internal static float Float(SkillTriggerDefinition trigger, string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is float number
                    ? number
                    : 0f;
        }

        /* 트리거의 지정 열에서 정수 값을 읽고 없으면 0을 반환한다. */
        internal static int Int(SkillTriggerDefinition trigger, string column)
        {
            return trigger.Columns.TryGetValue(column, out object value)
                && value is int number
                    ? number
                    : 0;
        }
    }
}
