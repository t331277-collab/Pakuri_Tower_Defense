using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class InGamePassiveEffectRuntime
    {
        public static void ApplyLearnedPassiveEffects(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            ISet<string> appliedOneShotEffectKeys)
        {
            if (combatManager == null || roster == null)
            {
                return;
            }

            var entries = roster.Entries;
            for (var i = 0; i < entries.Count; i++)
            {
                var ownerEntry = entries[i];
                var owner = ownerEntry != null ? ownerEntry.Model : null;
                var learnedPassives = owner != null && owner.State != null ? owner.State.LearnedPassiveSkillIds : null;
                if (ownerEntry == null || owner == null || learnedPassives == null || learnedPassives.Count == 0)
                {
                    continue;
                }

                foreach (var passiveId in learnedPassives)
                {
                    ApplyPassiveEffects(combatManager, roster, ownerEntry, owner, passiveId, appliedOneShotEffectKeys);
                }
            }
        }

        private static void ApplyPassiveEffects(
            InGameCombatManager combatManager,
            UnitRosterService roster,
            UnitRosterEntry ownerEntry,
            BaseUnitRuntimeModel owner,
            string passiveId,
            ISet<string> appliedOneShotEffectKeys)
        {
            if (string.IsNullOrWhiteSpace(passiveId)
                || !PakuriDataManager.Instance.TryGetData(passiveId, out PassiveDefinition passive)
                || passive == null
                || passive.PassiveEffects == null
                || passive.PassiveEffects.Length == 0)
            {
                return;
            }

            var context = new SkillExecutionContext(combatManager, roster, ownerEntry, null, 0f);
            var fallbackCenter = ownerEntry.Transform != null ? (Vector2)ownerEntry.Transform.position : Vector2.zero;
            var snapshot = BuildPassiveChoiceSnapshot(owner, passiveId);
            for (var i = 0; i < passive.PassiveEffects.Length; i++)
            {
                var effect = passive.PassiveEffects[i];
                if (effect == null
                    || !HasAllLearnedPassives(owner, effect.RequiresPassiveSkillId)
                    || HasAnyLearnedPassive(owner, effect.ExcludesPassiveSkillId))
                {
                    continue;
                }

                if (!effect.ApplyOnce)
                {
                    SkillMultiEffectExecutor.Execute(context, snapshot, new[] { effect }, fallbackCenter);
                    continue;
                }

                var key = BuildOneShotKey(owner, passiveId, effect);
                if (appliedOneShotEffectKeys != null && appliedOneShotEffectKeys.Contains(key))
                {
                    continue;
                }

                SkillMultiEffectExecutor.Execute(context, snapshot, new[] { effect }, fallbackCenter);
                appliedOneShotEffectKeys?.Add(key);
            }
        }

        private static SkillExecutionSnapshot BuildPassiveChoiceSnapshot(BaseUnitRuntimeModel owner, string passiveId)
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
                if (!string.Equals(targetSkillId, passiveId, System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!MeetsSourceStatusRequirement(choice, owner))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                snapshot.ApplyChoiceDefinition(choice);
            }

            return snapshot;
        }

        private static bool MeetsSourceStatusRequirement(SkillChoiceDefinition choice, BaseUnitRuntimeModel owner)
        {
            if (choice == null || string.IsNullOrWhiteSpace(choice.RequiredSourceStatusId))
            {
                return true;
            }

            if (!StatusEffectUtility.TryParse(choice.RequiredSourceStatusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return owner != null
                    && owner.Resources != null
                    && owner.Resources.CurrentShield > 0f;
            }

            return owner != null
                && owner.Statuses != null
                && owner.Statuses.GetStacks(kind) >= Mathf.Max(1, choice.RequiredSourceStatusMinStacks);
        }

        private static bool HasAllLearnedPassives(BaseUnitRuntimeModel owner, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && !HasLearnedPassive(owner, passiveId.Trim()))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasAnyLearnedPassive(BaseUnitRuntimeModel owner, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i];
                if (!string.IsNullOrWhiteSpace(passiveId) && HasLearnedPassive(owner, passiveId.Trim()))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasLearnedPassive(BaseUnitRuntimeModel owner, string passiveId)
        {
            return owner != null
                && owner.State != null
                && !string.IsNullOrWhiteSpace(passiveId)
                && owner.State.LearnedPassiveSkillIds.Contains(passiveId);
        }

        private static string BuildOneShotKey(BaseUnitRuntimeModel owner, string passiveId, SkillEffectDefinition effect)
        {
            var unitId = owner != null && owner.Identity != null && !string.IsNullOrWhiteSpace(owner.Identity.UnitId)
                ? owner.Identity.UnitId
                : owner != null ? owner.GetHashCode().ToString() : "unknown";
            var effectId = !string.IsNullOrWhiteSpace(effect.EffectId) ? effect.EffectId : effect.SkillId;
            return unitId + ":" + passiveId + ":" + effectId;
        }
    }
}
