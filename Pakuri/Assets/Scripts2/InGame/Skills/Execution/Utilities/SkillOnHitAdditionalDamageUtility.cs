using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class SkillOnHitAdditionalDamageUtility
    {
        private const string HitTarget = "HitTarget";
        private static bool applyingAdditionalDamage;

        public static void TryApply(
            InGameCombatManager manager,
            UnitRosterService roster,
            SkillRuntimeInstance runtime,
            SkillExecutionSnapshot snapshot,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            UnitRosterEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage)
        {
            if (manager == null
                || roster == null
                || snapshot == null
                || !snapshot.HasOnHitAdditionalDamageBehavior
                || source == null
                || hitTarget == null
                || hitTarget.Model == null
                || primaryBaseDamage <= 0f
                || applyingAdditionalDamage)
            {
                return;
            }

            var hitIndex = runtime != null ? runtime.AdvanceSkillHitCount() : 0;
            applyingAdditionalDamage = true;
            try
            {
                ApplyHitTargetDamage(manager, snapshot, source, sourceSkillId, hitTarget, primaryBaseDamage);
                ApplyChainDamage(manager, roster, snapshot, sourceEntry, source, sourceSkillId, hitTarget, hitPosition, primaryBaseDamage, hitIndex);
            }
            finally
            {
                applyingAdditionalDamage = false;
            }
        }

        private static void ApplyHitTargetDamage(
            InGameCombatManager manager,
            SkillExecutionSnapshot snapshot,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            UnitRosterEntry hitTarget,
            float primaryBaseDamage)
        {
            if (!snapshot.HasOnHitAdditionalDamage
                || snapshot.OnHitAdditionalDamageMultiplier <= 0f
                || !TargetsHitTarget(snapshot.OnHitAdditionalDamageTarget)
                || hitTarget == null
                || !hitTarget.IsAlive
                || hitTarget.Model == null
                || Random.value > Mathf.Clamp01(snapshot.OnHitAdditionalDamageChance))
            {
                return;
            }

            manager.ApplyDamage(
                hitTarget.Model,
                primaryBaseDamage * snapshot.OnHitAdditionalDamageMultiplier,
                snapshot.OnHitAdditionalDamageAttribute,
                source,
                false,
                0f,
                0f,
                sourceSkillId,
                true);
        }

        private static void ApplyChainDamage(
            InGameCombatManager manager,
            UnitRosterService roster,
            SkillExecutionSnapshot snapshot,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            string sourceSkillId,
            UnitRosterEntry hitTarget,
            Vector2 hitPosition,
            float primaryBaseDamage,
            int hitIndex)
        {
            if (!snapshot.HasOnHitChainDamageBehavior
                || hitIndex <= 0
                || hitIndex % snapshot.OnHitChainHitPeriod != 0)
            {
                return;
            }

            var targets = ResolveChainTargets(roster, sourceEntry, source, hitTarget, hitPosition, snapshot.OnHitChainSearchRadius);
            var count = Mathf.Min(snapshot.OnHitChainTargetCount, targets.Count);
            for (var i = 0; i < count; i++)
            {
                var target = targets[i];
                if (target == null || !target.IsAlive || target.Model == null)
                {
                    continue;
                }

                manager.ApplyDamage(
                    target.Model,
                    primaryBaseDamage * snapshot.OnHitChainDamageMultiplier,
                    snapshot.OnHitChainDamageAttribute,
                    source,
                    false,
                    0f,
                    0f,
                    sourceSkillId,
                    true);
            }
        }

        private static List<UnitRosterEntry> ResolveChainTargets(
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source,
            UnitRosterEntry hitTarget,
            Vector2 hitPosition,
            float searchRadius)
        {
            var result = new List<UnitRosterEntry>();
            if (roster == null || source == null || searchRadius <= 0f)
            {
                return result;
            }

            var primaryUnitId = ResolveUnitId(hitTarget != null ? hitTarget.Model : null);
            var candidates = ResolveOpposingEntries(roster, sourceEntry, source);
            var radiusSq = searchRadius * searchRadius;
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                if (candidate == null || !candidate.IsAlive || candidate.Model == null || candidate.Transform == null)
                {
                    continue;
                }

                var candidateUnitId = ResolveUnitId(candidate.Model);
                if ((!string.IsNullOrWhiteSpace(primaryUnitId) && candidateUnitId == primaryUnitId)
                    || candidate.Model == (hitTarget != null ? hitTarget.Model : null))
                {
                    continue;
                }

                var offset = (Vector2)candidate.Transform.position - hitPosition;
                if (offset.sqrMagnitude <= radiusSq)
                {
                    result.Add(candidate);
                }
            }

            result.Sort((left, right) =>
            {
                var leftDistance = ((Vector2)left.Transform.position - hitPosition).sqrMagnitude;
                var rightDistance = ((Vector2)right.Transform.position - hitPosition).sqrMagnitude;
                return leftDistance.CompareTo(rightDistance);
            });
            return result;
        }

        private static IReadOnlyList<UnitRosterEntry> ResolveOpposingEntries(
            UnitRosterService roster,
            UnitRosterEntry sourceEntry,
            BaseUnitRuntimeModel source)
        {
            var side = source.Identity != null
                ? source.Identity.Side
                : sourceEntry != null && sourceEntry.Model != null && sourceEntry.Model.Identity != null
                    ? sourceEntry.Model.Identity.Side
                    : UnitSide.Player;
            return side == UnitSide.Enemy ? roster.Players : roster.Enemies;
        }

        private static string ResolveUnitId(BaseUnitRuntimeModel model)
        {
            return model != null && model.Identity != null ? model.Identity.UnitId : string.Empty;
        }

        private static bool TargetsHitTarget(string target)
        {
            return string.IsNullOrWhiteSpace(target)
                || string.Equals(target, HitTarget, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
