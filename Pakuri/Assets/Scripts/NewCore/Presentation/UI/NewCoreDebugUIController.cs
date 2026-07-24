using System;
using System.Collections.Generic;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Presentation.Scene;
using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreDebugUIController : MonoBehaviour
    {
        private const int ActiveSlotCount = 5;
        private const int PassiveSlotCount = 5;
        private const int ActiveTraitCount = 5;
        private const int ActiveMasterCount = 2;
        private const int PassiveTraitCount = 3;

        private readonly Button[] activeSkillButtons =
            new Button[ActiveSlotCount];
        private readonly Button[] passiveSkillButtons =
            new Button[PassiveSlotCount];
        private readonly Button[] activeModifierButtons =
            new Button[ActiveSlotCount];
        private readonly Button[] passiveModifierButtons =
            new Button[PassiveSlotCount];
        private readonly Button[] activeTraitButtons =
            new Button[ActiveTraitCount];
        private readonly Button[] activeMasterButtons =
            new Button[ActiveMasterCount];
        private readonly Button[] passiveTraitButtons =
            new Button[PassiveTraitCount];
        private readonly List<SkillChoiceDefinition> visibleTraits =
            new List<SkillChoiceDefinition>();
        private readonly List<SkillChoiceDefinition> visibleMasters =
            new List<SkillChoiceDefinition>();

        private NewCoreSceneRuntime runtime;
        private NewCoreMonsterPanelUI monsterPanel;
        private GameObject debugRootPanel;
        private GameObject debugPanel;
        private GameObject modifierPanel;
        private GameObject passiveModifierPanel;
        private Button openButton;
        private SkillDefinition selectedModifierSkill;

        public bool DebugRootVisible =>
            debugRootPanel != null && debugRootPanel.activeSelf;

        private void Awake()
        {
            runtime = FindFirstObjectByType<NewCoreSceneRuntime>(
                FindObjectsInactive.Include);
            monsterPanel = FindFirstObjectByType<NewCoreMonsterPanelUI>(
                FindObjectsInactive.Include);
            ResolvePanels();
            ResolveSkillButtons();
            ResolveChoiceButtons();
            SetRootVisible(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                HandleToggleInput(
                    keyboard.digit8Key.wasPressedThisFrame,
                    keyboard.numpad8Key.wasPressedThisFrame);
            }
        }

        private void HandleToggleInput(
            bool digitEightPressed,
            bool numpadEightPressed)
        {
            if (digitEightPressed || numpadEightPressed)
            {
                ToggleRoot();
            }
        }

        public void ToggleRoot()
        {
            SetRootVisible(!DebugRootVisible);
        }

        public void Open()
        {
            SetActive(debugRootPanel, true);
            SetActive(debugPanel, true);
            SetActive(modifierPanel, false);
            SetActive(passiveModifierPanel, false);
            RefreshButtons();
        }

        public void Close()
        {
            SetActive(debugPanel, false);
            SetActive(modifierPanel, false);
            SetActive(passiveModifierPanel, false);
            SetActive(debugRootPanel, true);
            RefreshButtons();
        }

        private void SetRootVisible(bool visible)
        {
            SetActive(debugRootPanel, visible);
            if (visible)
            {
                SetActive(debugPanel, false);
                SetActive(modifierPanel, false);
                SetActive(passiveModifierPanel, false);
                RefreshButtons();
            }
        }

        private void ResolvePanels()
        {
            debugRootPanel = FindObject("DebugPanel");
            debugPanel = FindObject("DebugPanel/DebugUI");
            modifierPanel = FindObject("DebugPanel/DebugModifiedUI");
            passiveModifierPanel =
                FindObject("DebugPanel/DebugPassiveModifiedUI");
            openButton = FindButton("DebugPanel/DebugUIBtn");
            Bind(openButton, Open);
            Bind(FindButton("DebugPanel/DebugUI/Close"), Close);
            Bind(
                FindButton("DebugPanel/DebugModifiedUI/Close"),
                ReturnToDebug);
            Bind(
                FindButton("DebugPanel/DebugPassiveModifiedUI/Close"),
                ReturnToDebug);
        }

        private void ResolveSkillButtons()
        {
            for (int index = 0; index < ActiveSlotCount; index++)
            {
                int captured = index;
                string slot = ((char)('A' + index)).ToString();
                activeSkillButtons[index] =
                    FindButton($"DebugPanel/DebugUI/{slot}Btn");
                activeModifierButtons[index] = FindButton(
                    $"DebugPanel/DebugUI/{slot}Btn/{slot}modifierBtn");
                Bind(
                    activeSkillButtons[index],
                    () => TryLearnSlot(captured, false));
                Bind(
                    activeModifierButtons[index],
                    () => OpenModifier(captured, false));
            }

            for (int index = 0; index < PassiveSlotCount; index++)
            {
                int captured = index;
                string slot = ((char)('F' + index)).ToString();
                passiveSkillButtons[index] =
                    FindButton($"DebugPanel/DebugUI/{slot}Btn");
                passiveModifierButtons[index] = FindButton(
                    $"DebugPanel/DebugUI/{slot}Btn/EmodifierBtn");
                Bind(
                    passiveSkillButtons[index],
                    () => TryLearnSlot(captured, true));
                Bind(
                    passiveModifierButtons[index],
                    () => OpenModifier(captured, true));
            }
        }

        private void ResolveChoiceButtons()
        {
            for (int index = 0; index < ActiveTraitCount; index++)
            {
                int captured = index;
                activeTraitButtons[index] = FindButton(
                    $"DebugPanel/DebugModifiedUI/Trait{index + 1}");
                Bind(
                    activeTraitButtons[index],
                    () => TrySelectVisibleChoice(captured, false));
            }

            for (int index = 0; index < ActiveMasterCount; index++)
            {
                int captured = index;
                activeMasterButtons[index] = FindButton(
                    $"DebugPanel/DebugModifiedUI/Master{index + 1}");
                Bind(
                    activeMasterButtons[index],
                    () => TrySelectVisibleChoice(captured, true));
            }

            for (int index = 0; index < PassiveTraitCount; index++)
            {
                int captured = index;
                passiveTraitButtons[index] = FindButton(
                    $"DebugPanel/DebugPassiveModifiedUI/Trait{index + 1}");
                Bind(
                    passiveTraitButtons[index],
                    () => TrySelectVisibleChoice(captured, false));
            }
        }

        private void TryLearnSlot(int slotIndex, bool passive)
        {
            MonsterModel monster = SelectedMonster;
            SkillDefinition skill = ResolveSkill(slotIndex, passive);
            if (monster == null || skill == null)
            {
                return;
            }

            bool learned = TryLearnSkill(
                monster,
                skill,
                runtime.Catalog.Choices.Values);

            if (learned)
            {
                RefreshAfterMutation();
            }
        }

        private static bool TryLearnSkill(
            MonsterModel monster,
            SkillDefinition skill,
            IEnumerable<SkillChoiceDefinition> choices)
        {
            if (monster == null || skill == null || choices == null)
            {
                return false;
            }

            if (!(skill is PassiveDefinition passive))
            {
                return monster.SkillBucket.CanLearnActive(skill)
                    && monster.SkillBucket.TryLearnActive(skill);
            }

            if (!monster.SkillBucket.CanLearnPassive(passive)
                || !monster.SkillBucket.TryLearnPassive(passive))
            {
                return false;
            }

            foreach (SkillChoiceDefinition choice in choices)
            {
                if (choice.monster_id
                    == monster.MonsterDefinition.id
                    && choice.skill_id == passive.skill_id
                    && choice.choice_group == "PassiveBase")
                {
                    if (!monster.SkillBucket.CanSelectChoice(choice)
                        || !monster.SkillBucket.TrySelectChoice(choice))
                    {
                        throw new InvalidOperationException(
                            "Configured PassiveBase selection failed.");
                    }
                    break;
                }
            }

            return true;
        }

        private void OpenModifier(int slotIndex, bool passive)
        {
            selectedModifierSkill = ResolveSkill(slotIndex, passive);
            if (selectedModifierSkill == null
                || SelectedMonster == null)
            {
                return;
            }

            BuildVisibleChoices(selectedModifierSkill, passive);
            SetActive(debugPanel, false);
            SetActive(modifierPanel, !passive);
            SetActive(passiveModifierPanel, passive);
            RefreshChoiceButtons(passive);
        }

        private void ReturnToDebug()
        {
            selectedModifierSkill = null;
            visibleTraits.Clear();
            visibleMasters.Clear();
            SetActive(modifierPanel, false);
            SetActive(passiveModifierPanel, false);
            SetActive(debugPanel, true);
            RefreshButtons();
        }

        private void BuildVisibleChoices(
            SkillDefinition skill,
            bool passive)
        {
            visibleTraits.Clear();
            visibleMasters.Clear();
            foreach (SkillChoiceDefinition choice
                in runtime.Catalog.Choices.Values)
            {
                if (choice.monster_id != skill.monster_id
                    || choice.skill_id != skill.skill_id)
                {
                    continue;
                }

                if (choice.choice_group
                    == (passive
                        ? "PassiveEnhancement"
                        : "ActiveEnhancement"))
                {
                    visibleTraits.Add(choice);
                }
                else if (!passive
                    && choice.choice_group == "ActiveMaster")
                {
                    visibleMasters.Add(choice);
                }
            }

            visibleTraits.Sort(CompareChoices);
            visibleMasters.Sort(CompareChoices);
        }

        private void TrySelectVisibleChoice(
            int index,
            bool master)
        {
            List<SkillChoiceDefinition> choices =
                master ? visibleMasters : visibleTraits;
            MonsterModel monster = SelectedMonster;
            if (monster == null
                || index < 0
                || index >= choices.Count)
            {
                return;
            }

            SkillChoiceDefinition choice = choices[index];
            if (monster.SkillBucket.CanSelectChoice(choice)
                && monster.SkillBucket.TrySelectChoice(choice))
            {
                RefreshAfterMutation();
                RefreshChoiceButtons(
                    selectedModifierSkill is PassiveDefinition);
            }
        }

        private void RefreshButtons()
        {
            MonsterModel monster = SelectedMonster;
            for (int index = 0; index < ActiveSlotCount; index++)
            {
                SkillDefinition skill = ResolveSkill(index, false);
                RefreshSkillButton(
                    activeSkillButtons[index],
                    activeModifierButtons[index],
                    skill,
                    monster,
                    false);
            }

            for (int index = 0; index < PassiveSlotCount; index++)
            {
                SkillDefinition skill = ResolveSkill(index, true);
                RefreshSkillButton(
                    passiveSkillButtons[index],
                    passiveModifierButtons[index],
                    skill,
                    monster,
                    true);
            }
        }

        private static void RefreshSkillButton(
            Button skillButton,
            Button modifierButton,
            SkillDefinition skill,
            MonsterModel monster,
            bool passive)
        {
            if (skillButton == null)
            {
                return;
            }

            bool exists = skill != null;
            skillButton.gameObject.SetActive(exists);
            if (!exists || monster == null)
            {
                skillButton.interactable = false;
                if (modifierButton != null)
                {
                    modifierButton.interactable = false;
                }
                return;
            }

            SetButtonLabel(
                skillButton,
                string.IsNullOrWhiteSpace(skill.display_name)
                    ? skill.skill_id
                    : skill.display_name);
            bool canLearn = passive
                ? monster.SkillBucket.CanLearnPassive(
                    (PassiveDefinition)skill)
                : monster.SkillBucket.CanLearnActive(skill);
            skillButton.interactable = canLearn;
            if (modifierButton != null)
            {
                modifierButton.interactable =
                    ContainsLearnedSkill(monster, skill, passive);
            }
        }

        private void RefreshChoiceButtons(bool passive)
        {
            if (passive)
            {
                RefreshChoiceSet(
                    passiveTraitButtons,
                    visibleTraits);
                return;
            }

            RefreshChoiceSet(activeTraitButtons, visibleTraits);
            RefreshChoiceSet(activeMasterButtons, visibleMasters);
        }

        private void RefreshChoiceSet(
            Button[] buttons,
            IReadOnlyList<SkillChoiceDefinition> choices)
        {
            MonsterModel monster = SelectedMonster;
            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                if (button == null)
                {
                    continue;
                }

                bool visible = index < choices.Count;
                button.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                SkillChoiceDefinition choice = choices[index];
                SetButtonLabel(
                    button,
                    string.IsNullOrWhiteSpace(choice.title)
                        ? choice.choice_id
                        : choice.title);
                button.interactable = monster != null
                    && monster.SkillBucket.CanSelectChoice(choice);
            }
        }

        private void RefreshAfterMutation()
        {
            RefreshButtons();
            monsterPanel?.RefreshNow();
        }

        private SkillDefinition ResolveSkill(
            int slotIndex,
            bool passive)
        {
            if (runtime == null || runtime.Catalog == null)
            {
                return null;
            }

            string slot = ((char)(
                (passive ? 'F' : 'A') + slotIndex)).ToString();
            MonsterModel monster = SelectedMonster;
            if (monster == null)
            {
                return null;
            }

            foreach (SkillDefinition skill
                in runtime.Catalog.Skills.Values)
            {
                if (skill.monster_id
                        == monster.MonsterDefinition.id
                    && skill.slot == slot
                    && (skill is PassiveDefinition) == passive)
                {
                    return skill;
                }
            }

            return null;
        }

        private MonsterModel SelectedMonster =>
            runtime != null ? runtime.SelectedMonster : null;

        private static bool ContainsLearnedSkill(
            MonsterModel monster,
            SkillDefinition skill,
            bool passive)
        {
            if (passive)
            {
                IReadOnlyList<PassiveDefinition> passives =
                    monster.SkillBucket.PassiveSkills;
                for (int index = 0; index < passives.Count; index++)
                {
                    if (ReferenceEquals(passives[index], skill))
                    {
                        return true;
                    }
                }
                return false;
            }

            IReadOnlyList<SkillDefinition> active =
                monster.SkillBucket.ActiveSkills;
            for (int index = 0; index < active.Count; index++)
            {
                if (ReferenceEquals(active[index], skill))
                {
                    return true;
                }
            }
            return false;
        }

        private static int CompareChoices(
            SkillChoiceDefinition left,
            SkillChoiceDefinition right)
        {
            int order = Nullable.Compare(
                left.sort_order,
                right.sort_order);
            return order != 0
                ? order
                : string.CompareOrdinal(
                    left.choice_id,
                    right.choice_id);
        }

        private GameObject FindObject(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.gameObject : null;
        }

        private Button FindButton(string path)
        {
            Transform target = transform.Find(path);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private static void SetButtonLabel(
            Button button,
            string text)
        {
            TMP_Text[] labels =
                button.GetComponentsInChildren<TMP_Text>(true);
            for (int index = 0; index < labels.Length; index++)
            {
                if (labels[index].GetComponentInParent<Button>()
                    == button)
                {
                    labels[index].text = text;
                    return;
                }
            }
        }

        private static void Bind(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        private static void SetActive(
            GameObject target,
            bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
