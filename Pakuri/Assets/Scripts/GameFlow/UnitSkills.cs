/*
 * 역할: 유닛별 학습 스킬 소유.
 * 책임: 학습한 액티브·패시브 스킬과 강화·마스터 선택을 보관하고 조회한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// 한 유닛이 학습한 액티브·패시브 스킬과 강화·마스터 선택을 보관한다.
    [Serializable]
    public class UnitSkills
    {

        private readonly HashSet<string> learnedActiveSkillIds = new HashSet<string>();
        private readonly HashSet<string> learnedPassiveSkillIds = new HashSet<string>();
        private readonly HashSet<string> chosenEnhancementIds = new HashSet<string>();
        private readonly HashSet<string> chosenMasterSkillIds = new HashSet<string>();

        public IReadOnlyCollection<string> LearnedActiveSkillIds => learnedActiveSkillIds;
        public IReadOnlyCollection<string> LearnedPassiveSkillIds => learnedPassiveSkillIds;
        public IReadOnlyCollection<string> ChosenEnhancementIds => chosenEnhancementIds;
        public IReadOnlyCollection<string> ChosenMasterSkillIds => chosenMasterSkillIds;

        /// 전달된 choiceId 값을 사용해 Choice를 소유한 런타임 상태에 추가한다.
        public void AddChoice(string choiceId)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return;
            }

            if (!GameDataLoader.CurrentCatalog.TryGetData(choiceId, out SkillChoice choice))
            {
                throw new InvalidOperationException($"Unknown learned skill choice '{choiceId}'.");
            }

            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster)
            {
                AddMasterSkill(choiceId);
            }
            else
            {
                AddEnhancement(choiceId);
            }
        }

        /// 전달된 skillId 값을 사용해 ActiveSkill를 소유한 런타임 상태에 추가한다.
        public void AddActiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedActiveSkillIds.Add(skillId);
            }
        }

        /// 전달된 skillId 값을 사용해 소유한 런타임 상태에 ActiveSkill가 있는지 반환한다.
        public bool HasActiveSkill(string skillId)
        {
            foreach (var learnedSkillId in learnedActiveSkillIds)
            {
                if (string.Equals(learnedSkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// 전달된 skillId 값을 사용해 ActiveSkill를 소유한 런타임 상태에서 제거한다.
        public void RemoveActiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedActiveSkillIds.Remove(skillId);
            }
        }

        /// 전달된 skillId 값을 사용해 PassiveSkill를 소유한 런타임 상태에 추가한다.
        public void AddPassiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedPassiveSkillIds.Add(skillId);
            }
        }

        /// 전달된 skillId 값을 사용해 소유한 런타임 상태에 PassiveSkill가 있는지 반환한다.
        public bool HasPassiveSkill(string skillId)
        {
            foreach (var learnedSkillId in learnedPassiveSkillIds)
            {
                if (string.Equals(learnedSkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// 전달된 skillId 값을 사용해 PassiveSkill를 소유한 런타임 상태에서 제거한다.
        public void RemovePassiveSkill(string skillId)
        {
            if (!string.IsNullOrWhiteSpace(skillId))
            {
                learnedPassiveSkillIds.Remove(skillId);
            }
        }

        /// 전달된 choiceId 값을 사용해 Enhancement를 소유한 런타임 상태에 추가한다.
        public void AddEnhancement(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenEnhancementIds.Add(choiceId);
            }
        }

        /// 전달된 choiceId 값을 사용해 소유한 런타임 상태에 Enhancement가 있는지 반환한다.
        public bool HasEnhancement(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenEnhancementIds.Contains(choiceId);
        }

        /// 전달된 choiceId 값을 사용해 Enhancement를 소유한 런타임 상태에서 제거한다.
        public void RemoveEnhancement(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenEnhancementIds.Remove(choiceId);
            }
        }

        /// 전달된 choiceId 값을 사용해 MasterSkill를 소유한 런타임 상태에 추가한다.
        public void AddMasterSkill(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenMasterSkillIds.Add(choiceId);
            }
        }

        /// 전달된 choiceId 값을 사용해 소유한 런타임 상태에 MasterSkill가 있는지 반환한다.
        public bool HasMasterSkill(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && chosenMasterSkillIds.Contains(choiceId);
        }

        /// 전달된 choiceId 값을 사용해 MasterSkill를 소유한 런타임 상태에서 제거한다.
        public void RemoveMasterSkill(string choiceId)
        {
            if (!string.IsNullOrWhiteSpace(choiceId))
            {
                chosenMasterSkillIds.Remove(choiceId);
            }
        }

        /// 전달된 choiceId 값을 사용해 소유한 런타임 상태에 Choice가 있는지 반환한다.
        public bool HasChoice(string choiceId)
        {
            return HasEnhancement(choiceId) || HasMasterSkill(choiceId);
        }

        /// 소유한 모든 런타임 값를 소유한 런타임 상태에서 비운다.
        public void Clear()
        {
            learnedActiveSkillIds.Clear();
            learnedPassiveSkillIds.Clear();
            chosenEnhancementIds.Clear();
            chosenMasterSkillIds.Clear();
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        private readonly List<SkillExecutionData> activeSkills = new List<SkillExecutionData>();
        private readonly List<SkillExecutionData> passiveSkills = new List<SkillExecutionData>();

        public IReadOnlyList<SkillExecutionData> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillExecutionData> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /// 학습한 지속 효과를 합산해 속성별 추가 피해율을 구한다.
        public float PassiveOutgoingDamageBonus(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DamageUp, attribute, false) - 1f;
        }

        /// 학습한 지속 효과를 합성해 속성별 방어 배율을 구한다.
        public float PassiveDefenseMultiplier(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DefenseUp, attribute, false);
        }

        /// 학습한 지속 효과에서 치명타 확률 보너스를 합산한다.
        public float PassiveCriticalChanceBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritChanceUp);
        }

        /// 학습한 지속 효과에서 치명타 피해 보너스를 합산한다.
        public float PassiveCriticalDamageBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritDamageUp);
        }

        /// 학습한 지속 효과를 합성해 회복량 배율을 구한다.
        public float PassiveHealingMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.HealingUp, DamageAttribute.Physical, false);
        }

        /// 학습한 피해 감소 효과를 합성해 받는 피해 보정률을 구한다.
        public float PassiveIncomingDamageBonus()
        {
            return PassiveMultiplier(PassiveModifierKind.IncomingDamageDown, DamageAttribute.Physical, true) - 1f;
        }

        /// 기본 정의에 지속 효과와 선택 효과를 반영한 이번 시전값을 만든다.
        public SkillExecutionData CreateExecutionData(
            UnitCombatState owner,
            SkillExecutionData skill,
            UnitSpawnManager roster)
        {
            return SkillExecutionRuleResolver.BuildExecutionData(owner, skill, roster);
        }

        /// 스킬을 활성·지속 목록에 분류하고 같은 항목은 교체한다.
        public void AddOrReplace(SkillExecutionData instance)
        {
            var skills = passiveSkills;
            if (instance.Data.IsActive)
            {
                skills = activeSkills;
            }
            var existingIndex = FindIndexBySkillId(skills, instance.SkillId);
            if (existingIndex >= 0)
            {
                skills[existingIndex] = instance;
                return;
            }

            skills.Add(instance);
        }

        /// 활성·지속 목록에서 식별자가 같은 스킬을 찾는다.
        public SkillExecutionData FindBySkillId(string skillId)
        {
            var index = FindIndexBySkillId(activeSkills, skillId);
            if (index >= 0)
            {
                return activeSkills[index];
            }

            index = FindIndexBySkillId(passiveSkills, skillId);
            if (index >= 0)
            {
                return passiveSkills[index];
            }

            return null;
        }

        /// 보유 스킬 전체에서 식별자가 같은 선택 효과를 찾는다.
        public SkillChoice FindChoice(string choiceId)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var choice = FindChoice(activeSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var choice = FindChoice(passiveSkills[i].Data, choiceId);
                if (choice != null)
                {
                    return choice;
                }
            }

            return null;
        }

        /// 활성 스킬 목록에서 지정 슬롯의 스킬을 찾는다.
        public SkillExecutionData FindBySlot(SkillSlot slot)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                if (activeSkills[i] != null && activeSkills[i].Slot == slot)
                {
                    return activeSkills[i];
                }
            }

            return null;
        }

        /// 모든 활성 스킬의 시간 기반 상태를 진행한다.
        public void Tick(float deltaTime)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                SkillExecution.Tick(activeSkills[i], deltaTime);
            }
        }

        /// 목록에서 식별자가 같은 스킬의 위치를 찾는다.
        private static int FindIndexBySkillId(List<SkillExecutionData> skills, string skillId)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime != null && string.Equals(runtime.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return -1;
        }

        /// 같은 종류의 지속 효과 값을 합산한다.
        private float PassiveBonus(PassiveModifierKind kind)
        {
            var bonus = 0f;
            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var passive = passiveSkills[i].Data as PassiveSkillDefinition;
                if (passive != null && passive.ModifierKind == kind)
                {
                    bonus += Mathf.Max(0f, passive.ModifierValue);
                }
            }

            return bonus;
        }

        /// 조건에 맞는 지속 효과를 순차 합성해 최종 배율을 구한다.
        private float PassiveMultiplier(
            PassiveModifierKind kind,
            DamageAttribute attribute,
            bool reduction)
        {
            var multiplier = 1f;
            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var passive = passiveSkills[i].Data as PassiveSkillDefinition;
                if (passive == null
                    || passive.ModifierKind != kind
                    || (passive.HasModifierAttribute && passive.ModifierAttribute != attribute))
                {
                    continue;
                }

                var value = Mathf.Max(0f, passive.ModifierValue);
                multiplier *= reduction
                    ? Mathf.Max(0f, 1f - value)
                    : 1f + value;
            }

            return multiplier;
        }

        /// 한 스킬의 강화·마스터·지속 선택지에서 식별자가 같은 항목을 찾는다.
        private static SkillChoice FindChoice(SkillDefinition skill, string choiceId)
        {
            var choice = FindChoice(skill.EnhancementChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            choice = FindChoice(skill.MasterChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }

            var passive = skill as PassiveSkillDefinition;
            if (passive != null)
            {
                return FindChoice(passive.BaseModifierChoices, choiceId);
            }

            return null;
        }

        /// 선택지 목록에서 식별자가 같은 항목을 찾는다.
        private static SkillChoice FindChoice(SkillChoice[] choices, string choiceId)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                if (string.Equals(choices[i].ChoiceId, choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return choices[i];
                }
            }

            return null;
        }

        /// 지속 효과 선택을 실행값으로 모은다.
        public static SkillExecutionData PassiveChoices(UnitCombatState owner, string passiveId)
        {
            return Choices(owner, passiveId, true);
        }

        /// 활성 효과 선택을 실행값으로 모은다.
        public static SkillExecutionData ActiveChoices(UnitCombatState owner, string skillId)
        {
            return Choices(owner, skillId, false);
        }

        /// 선택 효과를 스킬별 실행값으로 합친다.
        private static SkillExecutionData Choices(UnitCombatState owner, string skillId, bool useTargetSkillId)
        {
            var snapshot = new SkillExecutionData(null);
            if (owner == null || owner.Skills == null || string.IsNullOrWhiteSpace(skillId))
            {
                return snapshot;
            }

            ApplyResolvedChoices(snapshot, owner, skillId, useTargetSkillId, owner.Skills.ChosenEnhancementIds);
            ApplyResolvedChoices(snapshot, owner, skillId, useTargetSkillId, owner.Skills.ChosenMasterSkillIds);
            return snapshot;
        }

        /// 조건을 통과한 선택 효과를 실행값에 기록한다.
        private static void ApplyResolvedChoices(
            SkillExecutionData snapshot,
            UnitCombatState owner,
            string skillId,
            bool useTargetSkillId,
            IReadOnlyCollection<string> choiceIds)
        {
            foreach (var choiceId in choiceIds)
            {
                var choice = owner.SkillState.FindChoice(choiceId);
                if (choice == null)
                {
                    continue;
                }

                var choiceSkillId = choice.SkillId;
                if (useTargetSkillId && !string.IsNullOrWhiteSpace(choice.TargetSkillId))
                {
                    choiceSkillId = choice.TargetSkillId;
                }

                if (!string.Equals(choiceSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    || !SkillExecutionRuleResolver.MeetsSourceStatusRequirements(choice, skillId, owner))
                {
                    continue;
                }

                snapshot.AddActiveChoiceId(choice.ChoiceId);
                SkillExecutionRuleResolver.ApplyChoice(snapshot, choice);
            }
        }
    }
}
