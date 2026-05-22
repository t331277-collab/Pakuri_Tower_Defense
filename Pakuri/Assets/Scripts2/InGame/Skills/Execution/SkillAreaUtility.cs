using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillAreaUtility
    {
        public static Vector2 ResolveAreaCenter(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            AreaBlueprintSpec area)
        {
            var origin = context != null && context.CasterEntry != null && context.CasterEntry.Transform != null
                ? context.CasterEntry.Transform.position
                : Vector3.zero;
            if (context != null && context.HasManualTargetPoint)
            {
                return context.ManualTargetPoint;
            }

            if (context != null && context.HasManualAimDirection && context.ManualAimDirection.sqrMagnitude > 0.0001f)
            {
                var radius = ResolveBaseRadius(targeting, area);
                return (Vector2)origin + context.ManualAimDirection.normalized * Mathf.Max(1f, radius);
            }

            var target = context != null
                ? SkillExecutionUtility.FindNearestTarget(context.CasterEntry, context.Roster, targeting)
                : null;
            return target != null && target.Transform != null
                ? (Vector2)target.Transform.position
                : (Vector2)origin;
        }

        public static float ResolveBaseRadius(SkillTargetingSpec targeting, AreaBlueprintSpec area)
        {
            return area != null && area.Radius > 0f
                ? area.Radius
                : targeting != null ? targeting.Radius : 0f;
        }

        public static float ResolveRadius(float baseRadius, SkillExecutionSnapshot snapshot)
        {
            var radius = baseRadius;
            if (snapshot != null)
            {
                radius = radius * Mathf.Max(0f, snapshot.RadiusMultiplier) + snapshot.RadiusBonus;
            }

            return Mathf.Max(0f, radius);
        }

        public static float ResolvePrefabScaleFactor(float baseRadius, SkillExecutionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return 1f;
            }

            if (baseRadius <= 0.0001f)
            {
                return Mathf.Max(0.01f, snapshot.RadiusMultiplier);
            }

            return Mathf.Max(0.01f, ResolveRadius(baseRadius, snapshot) / baseRadius);
        }
    }
}
