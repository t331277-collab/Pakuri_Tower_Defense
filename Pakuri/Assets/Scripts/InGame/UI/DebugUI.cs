using System;
using Pakuri.Data;
using Pakuri.Run;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class DebugUI : MonoBehaviour
    {
        private const int TraitButtonCount = 5;
        private const int MasterButtonCount = 2;
        private const int PassiveTraitButtonCount = 3;

        private static readonly SkillSlot[] DebugSlots =
        {
            SkillSlot.A,
            SkillSlot.B,
            SkillSlot.C,
            SkillSlot.D,
            SkillSlot.E,
            SkillSlot.F,
            SkillSlot.G,
            SkillSlot.H,
            SkillSlot.I,
            SkillSlot.J
        };

        [SerializeField] private GameObject debugRootPanel;
        [SerializeField] private GameObject debugPanel;
        [SerializeField] private GameObject debugModifiedPanel;
        [SerializeField] private GameObject debugPassiveModifiedPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button[] skillButtons = new Button[10];
        [SerializeField] private Button[] modifierOpenButtons = new Button[10];
        [SerializeField] private Button modifierCloseButton;
        [SerializeField] private Button passiveModifierCloseButton;
        [SerializeField] private Button[] traitButtons = new Button[TraitButtonCount];
        [SerializeField] private Button[] masterButtons = new Button[MasterButtonCount];
        [SerializeField] private Button[] passiveTraitButtons = new Button[PassiveTraitButtonCount];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private UnitSpawnManager unitSpawnManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private MonsterPanelUI monsterPanelUI;

        private int activeModifierSlotIndex = -1;
        private bool activeModifierIsPassive;

        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindButtons();
            SetDebugRootPanelVisible(false);
            SetPanelVisible(false);
            SetModifiedPanelVisible(false);
            SetPassiveModifiedPanelVisible(false);
        }

        private void OnEnable()
        {
            ResolveReferences();
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            monsterPanelUI?.RefreshNow();
        }

        private void Update()
        {
            ResolveReferences();

            var keyboard = Keyboard.current;
            if (keyboard != null
                && (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame))
            {
                SetDebugRootPanelVisible(debugRootPanel == null || !debugRootPanel.activeSelf);
            }
        }

        public void Open()
        {
            SetPanelVisible(true);
            CloseModifiedPanel();
            RefreshButtonLabels();
        }

        public void Close()
        {
            SetPanelVisible(false);
            CloseModifiedPanel();
        }

        private void TryLearnSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= DebugSlots.Length)
            {
                return;
            }

            if (IsPassiveSlot(slotIndex))
            {
                TryLearnPassiveSlot(slotIndex);
                return;
            }

            var session = ResolveSession();
            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model as MonsterUnitRuntimeModel : null;
            var monsterId = ResolveMonsterId(session, model);
            if (session == null || string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            if (monster == null)
            {
                return;
            }

            var sourceSkill = CsvDataLoader.CurrentCatalog.ResolveActiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex],
                monster);
            if (sourceSkill == null || string.IsNullOrWhiteSpace(sourceSkill.SkillId))
            {
                return;
            }

            if (session.HasLearnedActive(monster.MonsterId, sourceSkill.SkillId))
            {
                return;
            }

            CommitDebugOfferingChoice(session, catalog, monster, null, sourceSkill.SkillId, string.Empty);
        }

        private void TryLearnPassiveSlot(int slotIndex)
        {
            var session = ResolveSession();
            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model as MonsterUnitRuntimeModel : null;
            var monsterId = ResolveMonsterId(session, model);
            if (session == null || string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            if (monster == null)
            {
                return;
            }

            var passive = CsvDataLoader.CurrentCatalog.ResolvePassiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex],
                monster);
            if (passive == null || string.IsNullOrWhiteSpace(passive.PassiveId))
            {
                return;
            }

            if (session.HasLearnedPassive(monster.MonsterId, passive.PassiveId))
            {
                return;
            }

            CommitDebugOfferingChoice(session, catalog, monster, null, string.Empty, passive.PassiveId);
        }

        private void RefreshRuntimeSkillModels(RunSession session)
        {
            var manager = ResolveCombatManager();
            if (session == null || manager == null)
            {
                return;
            }

            var players = manager.Roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i] != null ? players[i].Model as MonsterUnitRuntimeModel : null;
                if (model == null)
                {
                    continue;
                }

                SyncModelStateFromSession(session, model);
                SkillRuntimeFactory.RebuildLearnedActiveSet(model);
                manager.Roster.RefreshActor(model);
            }
        }

        private static void SyncModelStateFromSession(RunSession session, MonsterUnitRuntimeModel model)
        {
            if (session == null || model == null || model.Identity == null)
            {
                return;
            }

            var monsterId = model.Identity.DefinitionId;
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var state = session.GetPartyMemberState(monsterId);
            if (state == null)
            {
                return;
            }

            if (model.State == null)
            {
                model.State = new UnitStateBucket();
            }

            CopyListToSet(state.LearnedActives, model.State.LearnedActiveSkillIds);
            CopyListToSet(state.LearnedPassives, model.State.LearnedPassiveSkillIds);
            CopyListToSet(state.ChosenChoiceIds, model.State.ChosenChoiceIds);
        }

        private static void CopyListToSet(System.Collections.Generic.IReadOnlyList<string> source, System.Collections.Generic.HashSet<string> target)
        {
            if (source == null || target == null)
            {
                return;
            }

            target.Clear();
            for (var i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }

        private void RefreshButtonLabels()
        {
            var session = ResolveSession();
            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model as MonsterUnitRuntimeModel : null;
            var monsterId = ResolveMonsterId(session, model);
            var monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);

            for (var i = 0; i < skillButtons.Length && i < DebugSlots.Length; i++)
            {
                var button = skillButtons[i];
                if (button == null)
                {
                    continue;
                }

                var label = button.GetComponentInChildren<TMP_Text>(true);
                var slot = DebugSlots[i];
                var isPassiveSlot = IsPassiveSlot(i);
                var activeSkill = !isPassiveSlot && monster != null
                    ? CsvDataLoader.CurrentCatalog.ResolveActiveSkill(monster.MonsterId, slot, monster)
                    : null;
                var passiveSkill = isPassiveSlot && monster != null
                    ? CsvDataLoader.CurrentCatalog.ResolvePassiveSkill(monster.MonsterId, slot, monster)
                    : null;
                var hasSkill = isPassiveSlot
                    ? passiveSkill != null && !string.IsNullOrWhiteSpace(passiveSkill.PassiveId)
                    : activeSkill != null && !string.IsNullOrWhiteSpace(activeSkill.SkillId);
                var learned = hasSkill && session != null && monster != null && (isPassiveSlot
                    ? session.HasLearnedPassive(monster.MonsterId, passiveSkill.PassiveId)
                    : session.HasLearnedActive(monster.MonsterId, activeSkill.SkillId));

                button.interactable = hasSkill && !learned;
                if (label != null)
                {
                    label.text = hasSkill
                        ? string.Format("{0}\n{1}", slot, learned ? "Learned" : isPassiveSlot ? passiveSkill.DisplayName : activeSkill.DisplayName)
                        : string.Format("{0}\nNone", slot);
                }

                var modifierButton = i < modifierOpenButtons.Length ? modifierOpenButtons[i] : null;
                if (modifierButton != null)
                {
                    modifierButton.interactable = hasSkill && learned;
                }
            }
        }

        private void ResolveReferences()
        {
            if (stageManager == null)
            {
                stageManager = FindSceneObject<StageManager>();
            }

            if (unitSpawnManager == null)
            {
                unitSpawnManager = FindSceneObject<UnitSpawnManager>();
            }

            if (combatManager == null)
            {
                combatManager = FindSceneObject<InGameCombatManager>();
            }

            if (monsterPanelUI == null)
            {
                monsterPanelUI = FindSceneObject<MonsterPanelUI>();
            }
        }

        private void ResolveSceneUi()
        {
            if (debugRootPanel == null)
            {
                debugRootPanel = FindChildObject("DebugPanel");
            }

            if (debugPanel == null)
            {
                debugPanel = FindChildObject("DebugPanel/DebugUI");
            }

            if (debugModifiedPanel == null)
            {
                debugModifiedPanel = FindChildObject("DebugPanel/DebugModifiedUI");
            }

            if (debugPassiveModifiedPanel == null)
            {
                debugPassiveModifiedPanel = FindChildObject("DebugPanel/DebugPassiveModifiedUI");
            }

            if (openButton == null)
            {
                openButton = FindButton("DebugPanel/DebugUIBtn") ?? FindButton("DebugPanel/DebugBtn");
            }

            if (closeButton == null)
            {
                closeButton = FindButton("DebugPanel/DebugUI/Close");
            }

            if (modifierCloseButton == null)
            {
                modifierCloseButton = FindButton("DebugPanel/DebugModifiedUI/Close");
            }

            if (passiveModifierCloseButton == null)
            {
                passiveModifierCloseButton = FindButton("DebugPanel/DebugPassiveModifiedUI/Close");
            }

            EnsureSkillButtonArray();
            for (var i = 0; i < DebugSlots.Length; i++)
            {
                var slotName = DebugSlots[i].ToString();
                ResolveSkillButton(i, $"DebugPanel/DebugUI/{slotName} Btn", $"DebugPanel/DebugUI/{slotName}Btn");
            }

            EnsureModifierOpenButtonArray();
            for (var i = 0; i < DebugSlots.Length; i++)
            {
                var slotName = DebugSlots[i].ToString();
                var modifierButtonName = ResolveModifierButtonName(DebugSlots[i]);
                ResolveModifierOpenButton(
                    i,
                    $"DebugPanel/DebugUI/{slotName} Btn/{modifierButtonName}",
                    $"DebugPanel/DebugUI/{slotName}Btn/{modifierButtonName}");
            }

            EnsureTraitButtonArray();
            ResolveTraitButton(0, "DebugPanel/DebugModifiedUI/Trait1", "DebugPanel/DebugModifiedUI/trait1");
            ResolveTraitButton(1, "DebugPanel/DebugModifiedUI/Trait2", "DebugPanel/DebugModifiedUI/trait2");
            ResolveTraitButton(2, "DebugPanel/DebugModifiedUI/Trait3", "DebugPanel/DebugModifiedUI/trait3");
            ResolveTraitButton(3, "DebugPanel/DebugModifiedUI/Trait4", "DebugPanel/DebugModifiedUI/trait4");
            ResolveTraitButton(4, "DebugPanel/DebugModifiedUI/Trait5", "DebugPanel/DebugModifiedUI/trait5");

            EnsureMasterButtonArray();
            ResolveMasterButton(0, "DebugPanel/DebugModifiedUI/Master1", "DebugPanel/DebugModifiedUI/master1");
            ResolveMasterButton(1, "DebugPanel/DebugModifiedUI/Master2", "DebugPanel/DebugModifiedUI/master2");

            EnsurePassiveTraitButtonArray();
            ResolvePassiveTraitButton(0, "DebugPanel/DebugPassiveModifiedUI/Trait1", "DebugPanel/DebugPassiveModifiedUI/trait1");
            ResolvePassiveTraitButton(1, "DebugPanel/DebugPassiveModifiedUI/Trait2", "DebugPanel/DebugPassiveModifiedUI/trait2");
            ResolvePassiveTraitButton(2, "DebugPanel/DebugPassiveModifiedUI/Trait3", "DebugPanel/DebugPassiveModifiedUI/trait3");
        }

        private void ResolveSkillButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= skillButtons.Length || skillButtons[index] != null)
            {
                return;
            }

            skillButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        private void EnsureSkillButtonArray()
        {
            if (skillButtons == null || skillButtons.Length != DebugSlots.Length)
            {
                skillButtons = new Button[DebugSlots.Length];
            }
        }

        private void ResolveModifierOpenButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= modifierOpenButtons.Length || modifierOpenButtons[index] != null)
            {
                return;
            }

            modifierOpenButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        private void EnsureModifierOpenButtonArray()
        {
            if (modifierOpenButtons == null || modifierOpenButtons.Length != DebugSlots.Length)
            {
                modifierOpenButtons = new Button[DebugSlots.Length];
            }
        }

        private void ResolveTraitButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= traitButtons.Length || traitButtons[index] != null)
            {
                return;
            }

            traitButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        private void EnsureTraitButtonArray()
        {
            if (traitButtons == null || traitButtons.Length != TraitButtonCount)
            {
                traitButtons = new Button[TraitButtonCount];
            }
        }

        private void ResolveMasterButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= masterButtons.Length || masterButtons[index] != null)
            {
                return;
            }

            masterButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        private void EnsureMasterButtonArray()
        {
            if (masterButtons == null || masterButtons.Length != MasterButtonCount)
            {
                masterButtons = new Button[MasterButtonCount];
            }
        }

        private void ResolvePassiveTraitButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= passiveTraitButtons.Length || passiveTraitButtons[index] != null)
            {
                return;
            }

            passiveTraitButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        private void EnsurePassiveTraitButtonArray()
        {
            if (passiveTraitButtons == null || passiveTraitButtons.Length != PassiveTraitButtonCount)
            {
                passiveTraitButtons = new Button[PassiveTraitButtonCount];
            }
        }

        private void BindButtons()
        {
            BindButton(openButton, Open);
            BindButton(closeButton, Close);
            BindButton(modifierCloseButton, CloseModifiedPanel);
            BindButton(passiveModifierCloseButton, CloseModifiedPanel);

            EnsureSkillButtonArray();
            for (var i = 0; i < skillButtons.Length && i < DebugSlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(skillButtons[i], () => TryLearnSlot(capturedIndex));
            }

            EnsureModifierOpenButtonArray();
            for (var i = 0; i < modifierOpenButtons.Length && i < DebugSlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(modifierOpenButtons[i], () => OpenModifiedPanelForSlot(capturedIndex));
            }

            EnsureTraitButtonArray();
            for (var i = 0; i < traitButtons.Length; i++)
            {
                var capturedIndex = i;
                BindButton(traitButtons[i], () => ApplyModifierChoice(false, capturedIndex));
            }

            EnsureMasterButtonArray();
            for (var i = 0; i < masterButtons.Length; i++)
            {
                var capturedIndex = i;
                BindButton(masterButtons[i], () => ApplyModifierChoice(true, capturedIndex));
            }

            EnsurePassiveTraitButtonArray();
            for (var i = 0; i < passiveTraitButtons.Length; i++)
            {
                var capturedIndex = i;
                BindButton(passiveTraitButtons[i], () => ApplyPassiveModifierChoice(capturedIndex));
            }
        }

        private RunSession ResolveSession()
        {
            if (stageManager != null && stageManager.ActiveSession != null)
            {
                return stageManager.ActiveSession;
            }

            return unitSpawnManager != null ? unitSpawnManager.ActiveSession : null;
        }

        private GameDataCatalog ResolveCatalog()
        {
            return CsvDataLoader.CurrentCatalog;
        }

        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        private UnitRosterEntry ResolveSelectedPlayerEntry()
        {
            var manager = ResolveCombatManager();
            return manager != null && manager.Roster.Players.Count > 0 ? manager.Roster.Players[0] : null;
        }

        private static string ResolveMonsterId(RunSession session, MonsterUnitRuntimeModel model)
        {
            if (model != null && model.Identity != null && !string.IsNullOrWhiteSpace(model.Identity.DefinitionId))
            {
                return model.Identity.DefinitionId;
            }

            return session != null ? session.SelectedMonsterId : string.Empty;
        }

        private void SetDebugRootPanelVisible(bool visible)
        {
            if (debugRootPanel != null)
            {
                debugRootPanel.SetActive(visible);
            }
        }

        private void SetPanelVisible(bool visible)
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(visible);
            }
        }

        private void SetModifiedPanelVisible(bool visible)
        {
            if (debugModifiedPanel != null)
            {
                debugModifiedPanel.SetActive(visible);
            }
        }

        private void SetPassiveModifiedPanelVisible(bool visible)
        {
            if (debugPassiveModifiedPanel != null)
            {
                debugPassiveModifiedPanel.SetActive(visible);
            }
        }

        private void CloseModifiedPanel()
        {
            activeModifierSlotIndex = -1;
            activeModifierIsPassive = false;
            SetModifiedPanelVisible(false);
            SetPassiveModifiedPanelVisible(false);
        }

        private void OpenModifiedPanelForSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= DebugSlots.Length)
            {
                return;
            }

            if (IsPassiveSlot(slotIndex))
            {
                OpenPassiveModifiedPanelForSlot(slotIndex);
                return;
            }

            if (!TryResolveModifierContext(slotIndex, out var session, out var sourceSkill, out var monster))
            {
                SetModifiedPanelVisible(false);
                SetPassiveModifiedPanelVisible(false);
                return;
            }

            if (sourceSkill == null
                || string.IsNullOrWhiteSpace(sourceSkill.SkillId)
                || monster == null
                || !session.HasLearnedActive(monster.MonsterId, sourceSkill.SkillId))
            {
                SetModifiedPanelVisible(false);
                SetPassiveModifiedPanelVisible(false);
                return;
            }

            activeModifierSlotIndex = slotIndex;
            activeModifierIsPassive = false;
            SetModifiedPanelVisible(true);
            SetPassiveModifiedPanelVisible(false);
            RefreshModifierChoiceButtons();
        }

        private void OpenPassiveModifiedPanelForSlot(int slotIndex)
        {
            if (!TryResolvePassiveModifierContext(slotIndex, out var session, out var passive, out var monster))
            {
                SetModifiedPanelVisible(false);
                SetPassiveModifiedPanelVisible(false);
                return;
            }

            if (passive == null
                || string.IsNullOrWhiteSpace(passive.PassiveId)
                || monster == null
                || !session.HasLearnedPassive(monster.MonsterId, passive.PassiveId))
            {
                SetModifiedPanelVisible(false);
                SetPassiveModifiedPanelVisible(false);
                return;
            }

            activeModifierSlotIndex = slotIndex;
            activeModifierIsPassive = true;
            SetModifiedPanelVisible(false);
            SetPassiveModifiedPanelVisible(true);
            RefreshModifierChoiceButtons();
        }

        private void ApplyModifierChoice(bool masterChoice, int choiceIndex)
        {
            if (!TryResolveModifierContext(activeModifierSlotIndex, out var session, out var sourceSkill, out var monster))
            {
                return;
            }

            var state = session.EnsurePartyMemberState(monster);
            if (state == null || sourceSkill == null || string.IsNullOrWhiteSpace(sourceSkill.SkillId))
            {
                return;
            }

            var choices = masterChoice ? sourceSkill.MasterSkillChoices : sourceSkill.EnhancementChoices;
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            {
                return;
            }

            var choice = choices[choiceIndex];
            if (!IsChoiceAvailableForState(session, state, sourceSkill.SkillId, choice))
            {
                return;
            }

            CommitDebugOfferingChoice(session, ResolveCatalog(), monster, choice, sourceSkill.SkillId, string.Empty);

            RefreshModifierChoiceButtons();
        }

        private void ApplyPassiveModifierChoice(int choiceIndex)
        {
            if (!TryResolvePassiveModifierContext(activeModifierSlotIndex, out var session, out var passive, out var monster))
            {
                return;
            }

            var state = session.EnsurePartyMemberState(monster);
            if (state == null || passive == null || string.IsNullOrWhiteSpace(passive.PassiveId))
            {
                return;
            }

            var choices = passive.EnhancementChoices;
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            {
                return;
            }

            var choice = choices[choiceIndex];
            if (!IsChoiceAvailableForState(session, state, passive.PassiveId, choice))
            {
                return;
            }

            CommitDebugOfferingChoice(session, ResolveCatalog(), monster, choice, string.Empty, passive.PassiveId);
            RefreshModifierChoiceButtons();
        }

        private void RefreshModifierChoiceButtons()
        {
            if (activeModifierIsPassive)
            {
                RefreshPassiveModifierChoiceButtons();
                return;
            }

            if (!TryResolveModifierContext(activeModifierSlotIndex, out var session, out var sourceSkill, out var monster))
            {
                SetModifierButtonsInactive(traitButtons);
                SetModifierButtonsInactive(masterButtons);
                SetModifierButtonsInactive(passiveTraitButtons);
                if (activeModifierSlotIndex < 0)
                {
                    SetModifiedPanelVisible(false);
                    SetPassiveModifiedPanelVisible(false);
                }

                return;
            }

            var state = session.EnsurePartyMemberState(monster);
            var enhancementChoices = sourceSkill != null && sourceSkill.EnhancementChoices != null
                ? sourceSkill.EnhancementChoices
                : Array.Empty<SkillChoiceDefinition>();
            var masterChoices = sourceSkill != null && sourceSkill.MasterSkillChoices != null
                ? sourceSkill.MasterSkillChoices
                : Array.Empty<SkillChoiceDefinition>();

            BindModifierChoiceButtons(traitButtons, enhancementChoices, session, state, sourceSkill.SkillId);
            BindModifierChoiceButtons(masterButtons, masterChoices, session, state, sourceSkill.SkillId);
            SetModifierButtonsInactive(passiveTraitButtons);
        }

        private void RefreshPassiveModifierChoiceButtons()
        {
            if (!TryResolvePassiveModifierContext(activeModifierSlotIndex, out var session, out var passive, out var monster))
            {
                SetModifierButtonsInactive(passiveTraitButtons);
                SetModifierButtonsInactive(traitButtons);
                SetModifierButtonsInactive(masterButtons);
                if (activeModifierSlotIndex < 0)
                {
                    SetModifiedPanelVisible(false);
                    SetPassiveModifiedPanelVisible(false);
                }

                return;
            }

            var state = session.EnsurePartyMemberState(monster);
            var enhancementChoices = passive != null && passive.EnhancementChoices != null
                ? passive.EnhancementChoices
                : Array.Empty<SkillChoiceDefinition>();

            BindModifierChoiceButtons(passiveTraitButtons, enhancementChoices, session, state, passive.PassiveId);
            SetModifierButtonsInactive(traitButtons);
            SetModifierButtonsInactive(masterButtons);
        }

        private static void BindModifierChoiceButtons(
            Button[] buttons,
            SkillChoiceDefinition[] choices,
            RunSession session,
            RunSession.RunMonsterState state,
            string sourceSkillId)
        {
            if (buttons == null)
            {
                return;
            }

            for (var i = 0; i < buttons.Length; i++)
            {
                var button = buttons[i];
                if (button == null)
                {
                    continue;
                }

                var hasChoice = choices != null && i < choices.Length && choices[i] != null;
                button.gameObject.SetActive(hasChoice);
                button.interactable = hasChoice && IsChoiceAvailableForState(session, state, sourceSkillId, choices[i]);

                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = hasChoice ? BuildModifierButtonLabel(choices[i]) : string.Empty;
                }
            }
        }

        private static string BuildModifierButtonLabel(SkillChoiceDefinition choice)
        {
            if (choice == null)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(choice.DescriptionText)
                ? choice.Title
                : string.Format("{0}\n{1}", choice.Title, choice.DescriptionText);
        }

        private static void SetModifierButtonsInactive(Button[] buttons)
        {
            if (buttons == null)
            {
                return;
            }

            for (var i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] != null)
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }

        private bool TryResolveModifierContext(
            int slotIndex,
            out RunSession session,
            out SkillDefinition sourceSkill,
            out MonsterDefinition monster)
        {
            session = ResolveSession();
            sourceSkill = null;
            monster = null;
            if (session == null || slotIndex < 0 || slotIndex >= DebugSlots.Length)
            {
                return false;
            }

            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model as MonsterUnitRuntimeModel : null;
            var monsterId = ResolveMonsterId(session, model);
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            if (monster == null)
            {
                return false;
            }

            sourceSkill = CsvDataLoader.CurrentCatalog.ResolveActiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex],
                monster);
            return sourceSkill != null;
        }

        private bool TryResolvePassiveModifierContext(
            int slotIndex,
            out RunSession session,
            out PassiveDefinition passive,
            out MonsterDefinition monster)
        {
            session = ResolveSession();
            passive = null;
            monster = null;
            if (session == null || slotIndex < 0 || slotIndex >= DebugSlots.Length || !IsPassiveSlot(slotIndex))
            {
                return false;
            }

            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model as MonsterUnitRuntimeModel : null;
            var monsterId = ResolveMonsterId(session, model);
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            monster = CsvDataLoader.CurrentCatalog.ResolveMonster(monsterId);
            if (monster == null)
            {
                return false;
            }

            passive = CsvDataLoader.CurrentCatalog.ResolvePassiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex],
                monster);
            return passive != null;
        }

        private void CommitDebugOfferingChoice(
            RunSession session,
            GameDataCatalog catalog,
            MonsterDefinition monster,
            SkillChoiceDefinition choice,
            string activeSkillId,
            string passiveSkillId)
        {
            if (session == null || monster == null)
            {
                return;
            }

            var rewardId = ResolveRewardId(monster, choice, activeSkillId, passiveSkillId);
            var choiceId = choice != null ? choice.ChoiceId : string.Empty;
            session.EnsurePartyMemberState(monster);
            session.RecordOfferingChoice(monster.MonsterId, rewardId, choiceId, activeSkillId, passiveSkillId);

            if (choice != null)
            {
                session.AccumulateReward(
                    monster.MonsterId,
                    choice.HasDamageMultiplier ? choice.DamageMultiplier : 1f,
                    choice.HasMagazineBonus ? choice.MagazineBonus : 0,
                    choice.HasShotIntervalMultiplier ? choice.ShotIntervalMultiplier : 1f,
                    choice.HasReloadTimeMultiplier ? choice.ReloadTimeMultiplier : 1f,
                    choice.HasMaxHealthBonus ? choice.MaxHealthBonus : 0f,
                    choice.HasStatusChanceBonus ? choice.StatusChanceBonus : 0f);
            }

            RefreshRuntimeSkillModels(session);
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            monsterPanelUI?.RefreshNow();
        }

        private static string ResolveRewardId(
            MonsterDefinition monster,
            SkillChoiceDefinition choice,
            string activeSkillId,
            string passiveSkillId)
        {
            if (choice == null)
            {
                return string.Empty;
            }

            if (monster == null)
            {
                return choice.ChoiceId;
            }

            var rewards = CsvDataLoader.CurrentCatalog.GetRewardChoices(monster.MonsterId, monster);
            var choiceId = choice.ChoiceId;
            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(choiceId)
                    && string.Equals(reward.RewardId, choiceId, StringComparison.OrdinalIgnoreCase))
                {
                    return reward.RewardId;
                }
            }

            return choiceId;
        }

        private static bool IsChoiceAvailableForState(
            RunSession session,
            RunSession.RunMonsterState state,
            string sourceSkillId,
            SkillChoiceDefinition choice)
        {
            if (session == null || state == null || choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId))
            {
                return false;
            }

            if (ContainsText(state.ChosenChoiceIds, choice.ChoiceId))
            {
                return false;
            }

            var targetSkillId = ResolveChoiceTargetSkillId(choice, sourceSkillId);
            switch (choice.ChoiceGroup)
            {
                case SkillChoiceGroup.ActiveEnhancement:
                    return CountChosenChoices(state, targetSkillId, SkillChoiceGroup.ActiveEnhancement) < 3;
                case SkillChoiceGroup.ActiveMaster:
                    return CountChosenChoices(state, targetSkillId, SkillChoiceGroup.ActiveEnhancement) >= 3
                        && CountChosenChoices(state, targetSkillId, SkillChoiceGroup.ActiveMaster) < 1;
                case SkillChoiceGroup.PassiveEnhancement:
                    return CountChosenChoices(state, targetSkillId, SkillChoiceGroup.PassiveEnhancement) < 1;
                default:
                    return true;
            }
        }

        private static string ResolveChoiceTargetSkillId(SkillChoiceDefinition choice, string fallbackSkillId)
        {
            if (choice == null)
            {
                return fallbackSkillId;
            }

            if (!string.IsNullOrWhiteSpace(choice.SkillId))
            {
                return choice.SkillId;
            }

            if (!string.IsNullOrWhiteSpace(choice.TargetSkillId))
            {
                return choice.TargetSkillId;
            }

            return fallbackSkillId;
        }

        private static int CountChosenChoices(
            RunSession.RunMonsterState state,
            string skillId,
            SkillChoiceGroup group)
        {
            if (state == null || string.IsNullOrWhiteSpace(skillId))
            {
                return 0;
            }

            var count = 0;
            for (var i = 0; i < state.ChosenChoiceIds.Count; i++)
            {
                var chosen = ResolveChoice(state.ChosenChoiceIds[i]);
                if (chosen != null
                    && chosen.ChoiceGroup == group
                    && string.Equals(ResolveChoiceTargetSkillId(chosen, chosen.SkillId), skillId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            return count;
        }

        private static SkillChoiceDefinition ResolveChoice(string choiceId)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return null;
            }

            return CsvDataLoader.CurrentCatalog.TryGetData(choiceId, out SkillChoiceDefinition choice)
                ? choice
                : null;
        }

        private static bool IsPassiveSlot(int slotIndex)
        {
            return slotIndex >= 0
                && slotIndex < DebugSlots.Length
                && DebugSlots[slotIndex] >= SkillSlot.F;
        }

        private static string ResolveModifierButtonName(SkillSlot slot)
        {
            return slot >= SkillSlot.E ? "EmodifierBtn" : $"{slot}modifierBtn";
        }

        private static bool ContainsText(System.Collections.Generic.IReadOnlyList<string> values, string target)
        {
            if (values == null || string.IsNullOrWhiteSpace(target))
            {
                return false;
            }

            for (var i = 0; i < values.Count; i++)
            {
                if (string.Equals(values[i], target, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private GameObject FindChildObject(string path)
        {
            var child = FindChild(path);
            return child != null ? child.gameObject : null;
        }

        private Transform FindChild(string path)
        {
            return transform.Find(path);
        }

        private Button FindButton(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Button>() : null;
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static T FindSceneObject<T>() where T : UnityEngine.Object
        {
            var objects = Resources.FindObjectsOfTypeAll<T>();
            for (var i = 0; i < objects.Length; i++)
            {
                var component = objects[i] as Component;
                if (component != null && component.gameObject.scene.IsValid())
                {
                    return objects[i];
                }
            }

            return null;
        }
    }
}
