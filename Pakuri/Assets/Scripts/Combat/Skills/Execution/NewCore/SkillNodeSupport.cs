using System;

namespace Pakuri.NewCore.Combat.Skills.Execution
{
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
    }
}
