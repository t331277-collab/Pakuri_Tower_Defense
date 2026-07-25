using System;
using System.Collections.Generic;
using Pakuri.NewCore.Bootstrap;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.UI.InGame.MonsterPanel;
using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/* 개발자 skill 학습·Choice 선택 panel과 숫자 8 표시 전환을 소유한다. */
namespace Pakuri.NewCore.UI.InGame.Debug
{
    public class NewCoreDebugUIController : MonoBehaviour
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

        private GameBootstrap runtime;
        private NewCoreMonsterPanelUI monsterPanel;
        private GameObject debugRootPanel;
        private GameObject debugPanel;
        private GameObject modifierPanel;
        private GameObject passiveModifierPanel;
        private Button openButton;
        private SkillDefinition selectedModifierSkill;

        public bool DebugRootVisible =>
            debugRootPanel != null && debugRootPanel.activeSelf;

        /* runtime과 debug panel 참조를 찾고 button command를 연결한다. */
        private void Awake()
        {
            runtime = FindFirstObjectByType<GameBootstrap>(
                FindObjectsInactive.Include);
            monsterPanel = FindFirstObjectByType<NewCoreMonsterPanelUI>(
                FindObjectsInactive.Include);
            ResolvePanels();
            ResolveSkillButtons();
            ResolveChoiceButtons();
            SetRootVisible(false);
        }

        /* 숫자 8 입력을 읽어 debug root 표시를 전환한다. */
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

        /* 일반 숫자키 또는 numpad 8 입력을 debug toggle command로 바꾼다. */
        private void HandleToggleInput(
            bool digitEightPressed,
            bool numpadEightPressed)
        {
            if (digitEightPressed || numpadEightPressed)
            {
                ToggleRoot();
            }
        }

        /* debug root의 현재 표시 상태를 반전한다. */
        public void ToggleRoot()
        {
            SetRootVisible(!DebugRootVisible);
        }

        /* skill 학습 panel을 열고 현재 button 상태를 그린다. */
        public void Open()
        {
            SetActive(debugRootPanel, true);
            SetActive(debugPanel, true);
            SetActive(modifierPanel, false);
            SetActive(passiveModifierPanel, false);
            RefreshButtons();
        }

        /* 편집 panel을 닫고 debug root 대기 상태로 돌아간다. */
        public void Close()
        {
            SetActive(debugPanel, false);
            SetActive(modifierPanel, false);
            SetActive(passiveModifierPanel, false);
            SetActive(debugRootPanel, true);
            RefreshButtons();
        }

        /* debug root 표시 상태와 내부 panel 초기 상태를 적용한다. */
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

        /* authored hierarchy에서 debug panel과 navigation button을 찾는다. */
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

        /* active·passive slot button에 학습과 modifier command를 연결한다. */
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

        /* Enhancement·Master choice button에 선택 command를 연결한다. */
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

        /* 선택 slot의 skill을 현재 Monster에게 학습시킨다. */
        private void TryLearnSlot(int slotIndex, bool passive)
        {
            MonsterModel monster = SelectedMonster;
            SkillDefinition skill = ResolveSkill(slotIndex, passive);
            if (monster == null || skill == null)
            {
                return;
            }

            bool learned = runtime.TryLearnSkill(monster, skill);

            if (learned)
            {
                RefreshAfterMutation();
            }
        }

        /* 선택 slot에 대응하는 Enhancement 또는 Master panel을 연다. */
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

        /* choice 선택 상태를 지우고 기본 debug panel로 돌아간다. */
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

        /* 선택 skill에 적용 가능한 Enhancement·Master Choice를 분류한다. */
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

                string enhancementGroup = "ActiveEnhancement";
                if (passive)
                {
                    enhancementGroup = "PassiveEnhancement";
                }

                if (choice.choice_group == enhancementGroup)
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

        /* 표시 목록의 지정 Choice를 현재 Monster에게 적용한다. */
        private void TrySelectVisibleChoice(
            int index,
            bool master)
        {
            List<SkillChoiceDefinition> choices = visibleTraits;
            if (master)
            {
                choices = visibleMasters;
            }

            MonsterModel monster = SelectedMonster;
            if (monster == null
                || index < 0
                || index >= choices.Count)
            {
                return;
            }

            SkillChoiceDefinition choice = choices[index];
            if (runtime.TrySelectSkillChoice(monster, choice))
            {
                RefreshAfterMutation();
                RefreshChoiceButtons(
                    selectedModifierSkill is PassiveDefinition);
            }
        }

        /* 현재 Monster의 active·passive 학습 button 상태를 갱신한다. */
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

        /* 한 skill button의 이름과 학습·modifier 가능 상태를 갱신한다. */
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

            string label = skill.display_name;
            if (string.IsNullOrWhiteSpace(label))
            {
                label = skill.skill_id;
            }

            SetButtonLabel(skillButton, label);
            bool canLearn;
            if (passive)
            {
                canLearn = monster.SkillBucket.CanLearnPassive(
                    (PassiveDefinition)skill);
            }
            else
            {
                canLearn =
                    monster.SkillBucket.CanLearnActive(skill);
            }

            skillButton.interactable = canLearn;
            if (modifierButton != null)
            {
                modifierButton.interactable =
                    ContainsLearnedSkill(monster, skill, passive);
            }
        }

        /* 현재 modifier 종류에 맞는 Choice button 묶음을 갱신한다. */
        private void RefreshChoiceButtons(bool passive)
        {
            if (passive)
            {
                RefreshChoiceSet(
                    passiveTraitButtons,
                    visibleTraits,
                    false);
                return;
            }

            RefreshChoiceSet(
                activeTraitButtons,
                visibleTraits,
                true);
            RefreshChoiceSet(
                activeMasterButtons,
                visibleMasters,
                true);
        }

        /* Choice 목록을 button 표시·문구·선택 가능 상태에 반영한다. */
        private void RefreshChoiceSet(
            Button[] buttons,
            IReadOnlyList<SkillChoiceDefinition> choices,
            bool showDescription)
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
                string label = choice.title;
                if (showDescription)
                {
                    label = choice.description_text;
                }
                if (string.IsNullOrWhiteSpace(label))
                {
                    label = choice.choice_id;
                }

                SetButtonLabel(button, label);
                button.interactable = monster != null
                    && monster.SkillBucket.CanSelectChoice(choice);
            }
        }

        /* runtime 변경 뒤 debug와 Monster panel 표시를 함께 갱신한다. */
        private void RefreshAfterMutation()
        {
            RefreshButtons();
            monsterPanel?.RefreshNow();
        }

        /* 현재 Monster와 slot에 대응하는 active 또는 passive skill을 찾는다. */
        private SkillDefinition ResolveSkill(
            int slotIndex,
            bool passive)
        {
            if (runtime == null || runtime.Catalog == null)
            {
                return null;
            }

            char firstSlot = 'A';
            if (passive)
            {
                firstSlot = 'F';
            }

            string slot =
                ((char)(firstSlot + slotIndex)).ToString();
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

        private MonsterModel SelectedMonster
        {
            get
            {
                if (runtime == null)
                {
                    return null;
                }

                return runtime.SelectedMonster;
            }
        }

        /* 지정 skill이 현재 Monster bucket에 학습되었는지 확인한다. */
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

        /* Choice를 sort_order 뒤 choice_id 순서로 안정 정렬한다. */
        private static int CompareChoices(
            SkillChoiceDefinition left,
            SkillChoiceDefinition right)
        {
            int order = Nullable.Compare(
                left.sort_order,
                right.sort_order);
            if (order != 0)
            {
                return order;
            }

            return string.CompareOrdinal(
                left.choice_id,
                right.choice_id);
        }

        /* controller Transform 아래의 지정 경로 GameObject를 찾는다. */
        private GameObject FindObject(string path)
        {
            Transform target = transform.Find(path);
            if (target == null)
            {
                return null;
            }

            return target.gameObject;
        }

        /* controller Transform 아래의 지정 경로 Button을 찾는다. */
        private Button FindButton(string path)
        {
            Transform target = transform.Find(path);
            if (target == null)
            {
                return null;
            }

            return target.GetComponent<Button>();
        }

        /* Button 자체에 속한 첫 TMP label의 문자열을 바꾼다. */
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

        /* Button의 기존 listener를 정리하고 debug command를 연결한다. */
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

        /* 존재하는 GameObject의 활성 상태를 바꾼다. */
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
