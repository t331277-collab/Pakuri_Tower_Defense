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

        private readonly HashSet<string> learnedActiveSkillNames = new HashSet<string>();
        private readonly HashSet<string> learnedPassiveSkillNames = new HashSet<string>();
        private readonly HashSet<string> chosenEnhancementNames = new HashSet<string>();
        private readonly HashSet<string> chosenMasterSkillNames = new HashSet<string>();

        private static readonly SkillSlot[] ActiveSlots =
        {
            SkillSlot.A,
            SkillSlot.B,
            SkillSlot.C,
            SkillSlot.D,
            SkillSlot.E
        };

        public IReadOnlyCollection<string> LearnedActiveSkillNames => learnedActiveSkillNames;
        public IReadOnlyCollection<string> LearnedPassiveSkillNames => learnedPassiveSkillNames;
        public IReadOnlyCollection<string> ChosenEnhancementNames => chosenEnhancementNames;
        public IReadOnlyCollection<string> ChosenMasterSkillNames => chosenMasterSkillNames;

        public void AddChoice(string choiceName)
        {
            if (string.IsNullOrWhiteSpace(choiceName))
            {
                return;
            }

            if (!GameDataLoader.CurrentCatalog.TryGetData(choiceName, out SkillChoice choice))
            {
                throw new InvalidOperationException($"Unknown learned skill choice '{choiceName}'.");
            }

            if (choice.ChoiceGroup == SkillChoiceGroup.ActiveMaster)
            {
                AddMasterSkill(choiceName);
            }
            else
            {
                AddEnhancement(choiceName);
            }
        }

        public void AddActiveSkill(string skillName)
        {
            if (!string.IsNullOrWhiteSpace(skillName))
            {
                learnedActiveSkillNames.Add(skillName);
            }
        }

        public bool HasActiveSkill(string skillName)
        {
            foreach (var learnedSkillName in learnedActiveSkillNames)
            {
                if (string.Equals(learnedSkillName, skillName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void RemoveActiveSkill(string skillName)
        {
            if (!string.IsNullOrWhiteSpace(skillName))
            {
                learnedActiveSkillNames.Remove(skillName);
            }
        }

        public void AddPassiveSkill(string skillName)
        {
            if (!string.IsNullOrWhiteSpace(skillName))
            {
                learnedPassiveSkillNames.Add(skillName);
            }
        }

        public bool HasPassiveSkill(string skillName)
        {
            foreach (var learnedSkillName in learnedPassiveSkillNames)
            {
                if (string.Equals(learnedSkillName, skillName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public void RemovePassiveSkill(string skillName)
        {
            if (!string.IsNullOrWhiteSpace(skillName))
            {
                learnedPassiveSkillNames.Remove(skillName);
            }
        }

        public void AddEnhancement(string choiceName)
        {
            if (!string.IsNullOrWhiteSpace(choiceName))
            {
                chosenEnhancementNames.Add(choiceName);
            }
        }

        public bool HasEnhancement(string choiceName)
        {
            return !string.IsNullOrWhiteSpace(choiceName) && chosenEnhancementNames.Contains(choiceName);
        }

        public void RemoveEnhancement(string choiceName)
        {
            if (!string.IsNullOrWhiteSpace(choiceName))
            {
                chosenEnhancementNames.Remove(choiceName);
            }
        }

        public void AddMasterSkill(string choiceName)
        {
            if (!string.IsNullOrWhiteSpace(choiceName))
            {
                chosenMasterSkillNames.Add(choiceName);
            }
        }

        public bool HasMasterSkill(string choiceName)
        {
            return !string.IsNullOrWhiteSpace(choiceName) && chosenMasterSkillNames.Contains(choiceName);
        }

        public void RemoveMasterSkill(string choiceName)
        {
            if (!string.IsNullOrWhiteSpace(choiceName))
            {
                chosenMasterSkillNames.Remove(choiceName);
            }
        }

        public bool HasChoice(string choiceName)
        {
            return HasEnhancement(choiceName) || HasMasterSkill(choiceName);
        }

        public void Clear()
        {
            learnedActiveSkillNames.Clear();
            learnedPassiveSkillNames.Clear();
            chosenEnhancementNames.Clear();
            chosenMasterSkillNames.Clear();
            activeSkills.Clear();
            passiveSkills.Clear();
        }

        /// 학습 결과를 전투용 런타임 목록으로 구성한다.
        public void RebuildLearnedSkillState(
            UnitCombatState owner,
            SkillDefinition[] activeDefinitions = null,
            PassiveSkillDefinition[] passiveDefinitions = null)
        {
            if (owner == null)
            {
                return;
            }

            Clear();
            if (owner.Skills == null)
            {
                return;
            }

            if (activeDefinitions == null && passiveDefinitions == null)
            {
                var monsterName = owner.Identity != null ? owner.Identity.DefinitionName : null;
                if (string.IsNullOrWhiteSpace(monsterName))
                {
                    return;
                }

                var activeSkills = new List<SkillDefinition>();
                for (var i = 0; i < ActiveSlots.Length; i++)
                {
                    var definition = GameDataLoader.CurrentCatalog.GetActiveSkill(monsterName, ActiveSlots[i]);
                    if (definition != null)
                    {
                        activeSkills.Add(definition);
                    }
                }

                activeDefinitions = activeSkills.ToArray();
                passiveDefinitions = GameDataLoader.CurrentCatalog.GetPassiveSkills(monsterName);
            }

            if (activeDefinitions != null)
            {
                for (var i = 0; i < activeDefinitions.Length; i++)
                {
                    var definition = activeDefinitions[i];
                    if (definition != null && owner.Skills.HasActiveSkill(definition.SkillName))
                    {
                        AddOrReplace(new SkillExecutionState(owner, definition));
                    }
                }
            }

            if (passiveDefinitions != null)
            {
                for (var i = 0; i < passiveDefinitions.Length; i++)
                {
                    var definition = passiveDefinitions[i];
                    if (definition != null && owner.Skills.HasPassiveSkill(definition.SkillName))
                    {
                        AddOrReplace(new SkillExecutionState(owner, definition));
                    }
                }
            }

            RefreshLearnedRuntimeValues(owner);
        }

        private readonly List<SkillExecutionState> activeSkills = new List<SkillExecutionState>();
        private readonly List<SkillExecutionState> passiveSkills = new List<SkillExecutionState>();

        public IReadOnlyList<SkillExecutionState> ActiveSkills => activeSkills;
        public IReadOnlyList<SkillExecutionState> PassiveSkills => passiveSkills;
        public int Count => activeSkills.Count + passiveSkills.Count;

        /// 학습한 지속 효과를 곱연산해 속성별 피해 배율을 구한다.
        public float PassiveOutgoingDamageMultiplier(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DamageUp, attribute, false);
        }

        /// 학습한 DefenseUp 패시브를 곱연산해 속성별 방어 배율을 구한다.
        public float PassiveDefenseMultiplier(DamageAttribute attribute)
        {
            return PassiveMultiplier(PassiveModifierKind.DefenseUp, attribute, false);
        }

        /// 학습한 지속 효과에서 치명타 확률 보너스를 합산한다.
        public float PassiveCriticalChanceBonus()
        {
            return PassiveBonus(PassiveModifierKind.CritChanceUp);
        }

        /// 학습한 지속 효과를 곱연산해 치명타 피해 배율을 구한다.
        public float PassiveCriticalDamageMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.CritDamageUp, DamageAttribute.Physical, false);
        }

        /// 학습한 지속 효과를 합성해 회복량 배율을 구한다.
        public float PassiveHealingMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.HealingUp, DamageAttribute.Physical, false);
        }

        /// 학습한 피해 감소 효과를 합성해 받는 피해 배율을 구한다.
        public float PassiveIncomingDamageMultiplier()
        {
            return PassiveMultiplier(PassiveModifierKind.IncomingDamageDown, DamageAttribute.Physical, true);
        }

        /// 기본 정의에 지속 효과와 선택 효과를 반영한 이번 시전값을 만든다.
        public SkillExecutionState CreateExecutionData(
            UnitCombatState owner,
            SkillExecutionState skill,
            UnitSpawnManager roster)
        {
            return SkillExecutionRules.BuildExecutionData(owner, skill, roster);
        }

        /// 스킬을 활성·지속 목록에 분류하고 같은 항목은 교체한다.
        public void AddOrReplace(SkillExecutionState instance)
        {
            var skills = passiveSkills;
            if (instance.Data.IsActive)
            {
                skills = activeSkills;
            }
            var existingIndex = FindIndexBySkillName(skills, instance.SkillName);
            if (existingIndex >= 0)
            {
                skills[existingIndex] = instance;
                return;
            }

            skills.Add(instance);
        }

        /// 현재 활성 효과를 기준으로 학습한 스킬의 고정 실행값을 다시 계산한다.
        internal void RefreshLearnedRuntimeValues(UnitCombatState owner)
        {
            InitializeLearnedRuntimeValues(owner, activeSkills);
            InitializeLearnedRuntimeValues(owner, passiveSkills);
        }

        /// 학습이 끝난 스킬의 고정 실행값을 계산한다.
        private static void InitializeLearnedRuntimeValues(
            UnitCombatState owner,
            IReadOnlyList<SkillExecutionState> skills)
        {
            if (owner == null || skills == null)
            {
                return;
            }

            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime == null)
                {
                    continue;
                }

                var snapshot = SkillExecutionRules.BuildExecutionData(
                    owner,
                    runtime,
                    null);
                SkillExecutionRules.InitializeRuntimeValues(runtime, snapshot);
            }
        }

        /// 활성·지속 목록에서 식별자가 같은 스킬을 찾는다.
        public SkillExecutionState FindBySkillName(string skillName)
        {
            var index = FindIndexBySkillName(activeSkills, skillName);
            if (index >= 0)
            {
                return activeSkills[index];
            }

            index = FindIndexBySkillName(passiveSkills, skillName);
            if (index >= 0)
            {
                return passiveSkills[index];
            }

            return null;
        }

        /// 정의 참조가 가리키는 학습 런타임을 찾는다.
        public SkillExecutionState FindByDefinition(SkillDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            for (var i = 0; i < activeSkills.Count; i++)
            {
                if (activeSkills[i]?.Data == definition)
                {
                    return activeSkills[i];
                }
            }

            for (var i = 0; i < passiveSkills.Count; i++)
            {
                if (passiveSkills[i]?.Data == definition)
                {
                    return passiveSkills[i];
                }
            }

            return null;
        }

        /// 보유 스킬 전체에서 식별자가 같은 선택 효과를 찾는다.
        public SkillChoice FindChoice(string choiceName)
        {
            for (var i = 0; i < activeSkills.Count; i++)
            {
                var choice = FindChoice(activeSkills[i].Data, choiceName);
                if (choice != null)
                {
                    return choice;
                }
            }

            for (var i = 0; i < passiveSkills.Count; i++)
            {
                var choice = FindChoice(passiveSkills[i].Data, choiceName);
                if (choice != null)
                {
                    return choice;
                }
            }

            return null;
        }

        /// 활성 스킬 목록에서 지정 슬롯의 스킬을 찾는다.
        public SkillExecutionState FindBySlot(SkillSlot slot)
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
        private static int FindIndexBySkillName(List<SkillExecutionState> skills, string skillName)
        {
            for (var i = 0; i < skills.Count; i++)
            {
                var runtime = skills[i];
                if (runtime != null && string.Equals(runtime.SkillName, skillName, StringComparison.OrdinalIgnoreCase))
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
        private static SkillChoice FindChoice(SkillDefinition skill, string choiceName)
        {
            var choice = FindChoice(skill.EnhancementChoices, choiceName);
            if (choice != null)
            {
                return choice;
            }

            choice = FindChoice(skill.MasterChoices, choiceName);
            if (choice != null)
            {
                return choice;
            }

            return null;
        }

        /// 선택지 목록에서 식별자가 같은 항목을 찾는다.
        private static SkillChoice FindChoice(SkillChoice[] choices, string choiceName)
        {
            for (var i = 0; i < choices.Length; i++)
            {
                if (string.Equals(choices[i].ChoiceName, choiceName, StringComparison.OrdinalIgnoreCase))
                {
                    return choices[i];
                }
            }

            return null;
        }

        /// 지속 효과 선택을 실행값으로 모은다.
        public static SkillExecutionState PassiveChoices(UnitCombatState owner, string passiveName)
        {
            return Choices(owner, passiveName, true);
        }

        /// 활성 효과 선택을 실행값으로 모은다.
        public static SkillExecutionState ActiveChoices(UnitCombatState owner, string skillName)
        {
            return Choices(owner, skillName, false);
        }

        /// 선택 효과를 스킬별 실행값으로 합친다.
        private static SkillExecutionState Choices(UnitCombatState owner, string skillName, bool useTargetSkillName)
        {
            var snapshot = new SkillExecutionState(null);
            if (owner == null || owner.Skills == null || string.IsNullOrWhiteSpace(skillName))
            {
                return snapshot;
            }

            ApplyResolvedChoices(snapshot, owner, skillName, useTargetSkillName, owner.Skills.ChosenEnhancementNames);
            ApplyResolvedChoices(snapshot, owner, skillName, useTargetSkillName, owner.Skills.ChosenMasterSkillNames);
            return snapshot;
        }

        /// 조건을 통과한 선택 효과를 실행값에 기록한다.
        private static void ApplyResolvedChoices(
            SkillExecutionState snapshot,
            UnitCombatState owner,
            string skillName,
            bool useTargetSkillName,
            IReadOnlyCollection<string> choiceNames)
        {
            foreach (var choiceName in choiceNames)
            {
                var choice = owner.SkillState.FindChoice(choiceName);
                if (choice == null)
                {
                    continue;
                }

                var choiceSkillName = choice.SkillName;
                if (useTargetSkillName && !string.IsNullOrWhiteSpace(choice.TargetSkillName))
                {
                    choiceSkillName = choice.TargetSkillName;
                }

                if (!string.Equals(choiceSkillName, skillName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                snapshot.AddActiveChoiceName(choice.ChoiceName);
                SkillExecutionRules.ApplyChoice(snapshot, choice);
            }
        }
    }
}
