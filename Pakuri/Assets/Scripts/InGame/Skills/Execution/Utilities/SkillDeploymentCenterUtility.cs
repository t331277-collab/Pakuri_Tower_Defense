using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 스킬 배치 반복 방식에서 사용하는 선택 값을 정의한다.
     */
    internal enum SkillDeploymentRepeatMode
    {
        RepeatNearest,
        RandomExisting
    }

    /*
     * 스킬 배치 중심점 계산과 변환 기능을 제공한다.
     */
    internal static class SkillDeploymentCenterUtility
    {
        /*
         * 대상 기준 중심점을 결정한다.
         */
        public static List<Vector2> ResolveTargetAnchoredCenters(
            SkillExecutionContext context,
            SkillTargetingSpec targeting,
            Vector2 primaryCenter,
            int deploymentCount,
            bool coverAll,
            SkillDeploymentRepeatMode repeatMode)
        {
            var centers = new List<Vector2> { primaryCenter };
            if (deploymentCount <= 1
                || context == null
                || context.CasterEntry == null
                || context.Roster == null
                || coverAll)
            {
                while (centers.Count < deploymentCount)
                {
                    centers.Add(primaryCenter);
                }

                return centers;
            }

            var orderedTargets = SkillExecutionUtility.ResolveOrderedTargets(context.CasterEntry, context.Roster, targeting);
            if (orderedTargets.Count <= 0)
            {
                while (centers.Count < deploymentCount)
                {
                    centers.Add(primaryCenter);
                }

                return centers;
            }

            centers.Clear();
            var claimedUnitIds = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < orderedTargets.Count && centers.Count < deploymentCount; i++)
            {
                var target = orderedTargets[i];
                var unitId = target != null && target.Model != null && target.Model.Identity != null
                    ? target.Model.Identity.UnitId
                    : string.Empty;
                if (!string.IsNullOrWhiteSpace(unitId) && !claimedUnitIds.Add(unitId))
                {
                    continue;
                }

                if (target == null || target.Transform == null)
                {
                    continue;
                }

                centers.Add((Vector2)target.Transform.position);
            }

            var repeatIndex = 0;
            while (centers.Count < deploymentCount)
            {
                UnitRosterEntry fallbackTarget;
                if (repeatMode == SkillDeploymentRepeatMode.RandomExisting)
                {
                    fallbackTarget = orderedTargets[Random.Range(0, orderedTargets.Count)];
                }
                else
                {
                    fallbackTarget = orderedTargets[repeatIndex % orderedTargets.Count];
                    repeatIndex++;
                }

                centers.Add(fallbackTarget != null && fallbackTarget.Transform != null
                    ? (Vector2)fallbackTarget.Transform.position
                    : primaryCenter);
            }

            return centers;
        }
    }
}
