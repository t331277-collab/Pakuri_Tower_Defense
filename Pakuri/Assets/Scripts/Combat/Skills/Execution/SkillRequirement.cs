using Pakuri.Data;
using UnityEngine;

/*
 * 스킬 효과와 패시브가 요구하는 선택지·패시브·상태 조건을 판정한다.
 */
namespace Pakuri.InGame
{
    internal static class SkillRequirement
    {
        internal static bool HasAllActiveChoices(SkillSnapshot snapshot, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList))
            {
                return true;
            }

            if (snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choiceId = choices[i].Trim();
                if (choiceId.Length > 0 && !snapshot.HasActiveChoice(choiceId))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool HasAnyActiveChoice(SkillSnapshot snapshot, string choiceList)
        {
            if (string.IsNullOrWhiteSpace(choiceList) || snapshot == null)
            {
                return false;
            }

            var choices = choiceList.Split(';', ',');
            for (var i = 0; i < choices.Length; i++)
            {
                var choiceId = choices[i].Trim();
                if (choiceId.Length > 0 && snapshot.HasActiveChoice(choiceId))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool HasAllLearnedPassives(UnitCombatState owner, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return true;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i].Trim();
                if (passiveId.Length > 0 && !HasLearnedPassive(owner, passiveId))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool HasAnyLearnedPassive(UnitCombatState owner, string passiveList)
        {
            if (string.IsNullOrWhiteSpace(passiveList))
            {
                return false;
            }

            var passives = passiveList.Split(';', ',');
            for (var i = 0; i < passives.Length; i++)
            {
                var passiveId = passives[i].Trim();
                if (passiveId.Length > 0 && HasLearnedPassive(owner, passiveId))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool MeetsSourceStatus(SkillChoiceDefinition choice, UnitCombatState owner)
        {
            return choice == null
                || HasSourceStatus(owner, choice.RequiredSourceStatusId, choice.RequiredSourceStatusMinStacks);
        }

        internal static bool HasSourceStatus(UnitCombatState owner, string statusId, int minimumStacks)
        {
            if (string.IsNullOrWhiteSpace(statusId))
            {
                return true;
            }

            if (!StatusEffectLookup.TryParse(statusId, out var kind))
            {
                return false;
            }

            if (kind == StatusEffectKind.Shield)
            {
                return owner != null && owner.Resources != null && owner.Resources.CurrentShield > 0f;
            }

            return owner != null
                && owner.Statuses != null
                && owner.Statuses.GetStacks(kind) >= Mathf.Max(1, minimumStacks);
        }

        private static bool HasLearnedPassive(UnitCombatState owner, string passiveId)
        {
            return owner != null
                && owner.SkillProgress != null
                && owner.SkillProgress.LearnedPassiveSkillIds.Contains(passiveId);
        }
    }
}
