/*
 * 역할: InGame 개발자 제어 및 진단.
 * 책임: 런타임 상태를 표시하고 전투·Stage·스킬·상태·스폰 유닛 Debug 동작을 제공한다.
 */

using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    /// 전투·Stage·스킬·상태·스폰 상태와 개발용 조작을 화면에 제공한다.
    public class DebugUI : MonoBehaviour
    {
        private const int TraitButtonCount = 5;
        private const int MasterButtonCount = 2;
        private const int PassiveTraitButtonCount = 3;
        private const int ArtifactDebugButtonCount = 10;

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

        private GameObject debugRootPanel;
        private GameObject debugPanel;
        private GameObject debugModifiedPanel;
        private GameObject debugPassiveModifiedPanel;
        private GameObject artifactDebugPanel;
        private GameObject artifactAcquisitionDebugPanel;
        private Button openButton;
        private Button closeButton;
        private Button artifactDebugButton;
        private Button artifactAcquisitionCloseButton;
        private Button[] skillButtons = new Button[10];
        private Button[] modifierOpenButtons = new Button[10];
        private Button[] artifactSynergyButtons = new Button[ArtifactDebugButtonCount];
        private Button[] artifactButtons = new Button[ArtifactDebugButtonCount];
        private TMP_Text[] artifactSynergyLabels = new TMP_Text[ArtifactDebugButtonCount];
        private TMP_Text[] artifactLabels = new TMP_Text[ArtifactDebugButtonCount];
        private Button modifierCloseButton;
        private Button passiveModifierCloseButton;
        private Button[] traitButtons = new Button[TraitButtonCount];
        private Button[] masterButtons = new Button[MasterButtonCount];
        private Button[] passiveTraitButtons = new Button[PassiveTraitButtonCount];
        private StageManager stageManager;
        private InGameCombatManager combatManager;
        private MonsterPanelUI monsterPanelUI;
        private InGameUIManager uiManager;
        private bool referencesBound;
        private bool bindingFailed;

        private ArtifactSynergyDefinition[] artifactSynergies = Array.Empty<ArtifactSynergyDefinition>();
        private ArtifactDefinition[] artifactChoices = Array.Empty<ArtifactDefinition>();
        private string selectedArtifactSynergyId = string.Empty;

        private int activeModifierSlotIndex = -1;
        private bool activeModifierIsPassive;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            BindButtons();
            UiObjectUtility.SetActive(debugRootPanel, false);
            UiObjectUtility.SetActive(debugPanel, false);
            UiObjectUtility.SetActive(debugModifiedPanel, false);
            UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
            UiObjectUtility.SetActive(artifactDebugPanel, false);
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, false);
        }

        /// Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.
        private void OnEnable()
        {
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            RefreshArtifactSynergyButtons();
            monsterPanelUI?.RefreshNow();
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null
                && (keyboard.digit8Key.wasPressedThisFrame || keyboard.numpad8Key.wasPressedThisFrame))
            {
                UiObjectUtility.SetActive(debugRootPanel, debugRootPanel == null || !debugRootPanel.activeSelf);
            }
        }

        public void Open()
        {
            ClearArtifactDebugState();
            UiObjectUtility.SetActive(artifactDebugPanel, false);
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, false);
            UiObjectUtility.SetActive(debugPanel, true);
            CloseModifiedPanel();
            RefreshButtonLabels();
        }

        public void Close()
        {
            UiObjectUtility.SetActive(debugPanel, false);
            CloseModifiedPanel();
            ClearArtifactDebugState();
            UiObjectUtility.SetActive(artifactDebugPanel, false);
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, false);
        }

        private void OpenArtifactDebug()
        {
            ClearArtifactDebugState();
            RefreshArtifactSynergyButtons();
            UiObjectUtility.SetActive(debugPanel, false);
            CloseModifiedPanel();
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, false);
            UiObjectUtility.SetActive(artifactDebugPanel, true);
        }

        private void CloseArtifactAcquisitionDebug()
        {
            artifactChoices = Array.Empty<ArtifactDefinition>();
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, false);
            UiObjectUtility.SetActive(artifactDebugPanel, true);
            RefreshArtifactSynergyButtons();
        }

        internal void ShowArtifactAcquisitionDebug()
        {
            if (string.IsNullOrWhiteSpace(selectedArtifactSynergyId))
            {
                OpenArtifactDebug();
                return;
            }

            RefreshArtifactChoices();
            UiObjectUtility.SetActive(debugPanel, false);
            UiObjectUtility.SetActive(artifactDebugPanel, false);
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, true);
        }

        private void SelectArtifactSynergy(int index)
        {
            if (index < 0 || index >= artifactSynergies.Length)
            {
                return;
            }

            var synergy = artifactSynergies[index];
            if (synergy == null || string.IsNullOrWhiteSpace(synergy.SynergyId))
            {
                return;
            }

            selectedArtifactSynergyId = synergy.SynergyId;
            RefreshArtifactChoices();
            UiObjectUtility.SetActive(artifactDebugPanel, false);
            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, true);
        }

        private void SelectArtifact(int index)
        {
            if (index < 0 || index >= artifactChoices.Length)
            {
                return;
            }

            var artifact = artifactChoices[index];
            if (artifact == null || string.IsNullOrWhiteSpace(artifact.ArtifactId) || uiManager == null)
            {
                return;
            }

            UiObjectUtility.SetActive(artifactAcquisitionDebugPanel, false);
            uiManager.OpenArtifactDebugAcquisition(artifact.ArtifactId);
        }

        private void RefreshArtifactSynergyButtons()
        {
            var catalog = GameDataLoader.CurrentCatalog;
            artifactSynergies = catalog != null && catalog.ArtifactSynergies != null
                ? catalog.ArtifactSynergies
                : Array.Empty<ArtifactSynergyDefinition>();

            for (var i = 0; i < artifactSynergyButtons.Length; i++)
            {
                var button = artifactSynergyButtons[i];
                ResetArtifactDebugButton(button, artifactSynergyLabels[i]);
                if (i >= artifactSynergies.Length || artifactSynergies[i] == null)
                {
                    continue;
                }

                var capturedIndex = i;
                SetArtifactDebugLabel(artifactSynergyLabels[i], artifactSynergies[i].DisplayName);
                button.interactable = true;
                button.onClick.AddListener(() => SelectArtifactSynergy(capturedIndex));
            }
        }

        private void RefreshArtifactChoices()
        {
            var catalog = GameDataLoader.CurrentCatalog;
            var artifacts = catalog != null && catalog.Artifacts != null
                ? catalog.Artifacts
                : Array.Empty<ArtifactDefinition>();
            var choices = new System.Collections.Generic.List<ArtifactDefinition>();
            for (var i = 0; i < artifacts.Length; i++)
            {
                var artifact = artifacts[i];
                if (artifact != null
                    && string.Equals(artifact.SynergyId, selectedArtifactSynergyId, StringComparison.OrdinalIgnoreCase))
                {
                    choices.Add(artifact);
                }
            }

            artifactChoices = choices.ToArray();
            for (var i = 0; i < artifactButtons.Length; i++)
            {
                var button = artifactButtons[i];
                ResetArtifactDebugButton(button, artifactLabels[i]);
                if (i >= artifactChoices.Length || artifactChoices[i] == null)
                {
                    continue;
                }

                var capturedIndex = i;
                SetArtifactDebugLabel(artifactLabels[i], artifactChoices[i].DisplayName);
                button.interactable = true;
                button.onClick.AddListener(() => SelectArtifact(capturedIndex));
            }
        }

        private void ClearArtifactDebugState()
        {
            selectedArtifactSynergyId = string.Empty;
            artifactSynergies = Array.Empty<ArtifactSynergyDefinition>();
            artifactChoices = Array.Empty<ArtifactDefinition>();
            ResetArtifactDebugButtons(artifactSynergyButtons, artifactSynergyLabels);
            ResetArtifactDebugButtons(artifactButtons, artifactLabels);
        }

        private static void ResetArtifactDebugButtons(Button[] buttons, TMP_Text[] labels)
        {
            if (buttons == null)
            {
                return;
            }

            for (var i = 0; i < buttons.Length; i++)
            {
                var label = labels != null && i < labels.Length ? labels[i] : null;
                ResetArtifactDebugButton(buttons[i], label);
            }
        }

        private static void ResetArtifactDebugButton(Button button, TMP_Text label)
        {
            if (button == null)
            {
                return;
            }

            button.gameObject.SetActive(true);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
            SetArtifactDebugLabel(label, string.Empty);
        }

        private static void SetArtifactDebugLabel(TMP_Text label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
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

            if (!TryResolveSelectedMonster(out var session, out var monster))
            {
                return;
            }

            var sourceSkill = GameDataLoader.CurrentCatalog.GetActiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex]);
            if (sourceSkill == null || string.IsNullOrWhiteSpace(sourceSkill.SkillId))
            {
                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (!session.CanLearnActive(state, monster, sourceSkill))
            {
                return;
            }

            CommitDebugOfferingChoice(session, monster, null, sourceSkill.SkillId, string.Empty);
        }

        private void TryLearnPassiveSlot(int slotIndex)
        {
            if (!TryResolveSelectedMonster(out var session, out var monster))
            {
                return;
            }

            var passive = GameDataLoader.CurrentCatalog.ResolvePassiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex]);
            if (passive == null || string.IsNullOrWhiteSpace(passive.SkillId))
            {
                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (!session.CanLearnPassive(state, monster, passive))
            {
                return;
            }

            CommitDebugOfferingChoice(session, monster, null, string.Empty, passive.SkillId);
        }

        private void RefreshRuntimeSkillModels()
        {
            var manager = combatManager;
            if (manager == null)
            {
                return;
            }

            var players = manager.Units.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var entry = players[i];
                if (entry == null || entry.Model.Identity.Role != UnitRole.Monster)
                {
                    continue;
                }

                var model = entry.Model;
                model.SkillState.RebuildLearnedSkillState(model);
                manager.RefreshPassiveEffects(model);
                manager.Units.RefreshDisplay(model);
            }
        }

        private void RefreshButtonLabels()
        {
            var session = ResolveSession();
            var selectedEntry = ResolveSelectedPlayerEntry();
            UnitCombatState model = null;
            if (selectedEntry != null)
            {
                model = selectedEntry.Model;
            }
            var monsterId = ResolveMonsterId(session, model);
            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            var state = session != null && monster != null
                ? session.GetPartyMemberState(monster.MonsterId)
                : null;

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
                    ? GameDataLoader.CurrentCatalog.GetActiveSkill(monster.MonsterId, slot)
                    : null;
                var passiveSkill = isPassiveSlot && monster != null
                    ? GameDataLoader.CurrentCatalog.ResolvePassiveSkill(monster.MonsterId, slot)
                    : null;
                var hasSkill = isPassiveSlot
                    ? passiveSkill != null && !string.IsNullOrWhiteSpace(passiveSkill.SkillId)
                    : activeSkill != null && !string.IsNullOrWhiteSpace(activeSkill.SkillId);
                var learned = hasSkill && state != null && (isPassiveSlot
                    ? state.Skills.HasPassiveSkill(passiveSkill.SkillId)
                    : state.Skills.HasActiveSkill(activeSkill.SkillId));

                button.interactable = hasSkill && !learned;
                if (label != null)
                {
                    label.text = hasSkill
                        ? string.Format("{0}\n{1}", slot, learned ? "Learned" : isPassiveSlot ? passiveSkill.SkillName : activeSkill.SkillName)
                        : string.Format("{0}\nNone", slot);
                }

                var modifierButton = i < modifierOpenButtons.Length ? modifierOpenButtons[i] : null;
                if (modifierButton != null)
                {
                    modifierButton.interactable = hasSkill && learned;
                }
            }
        }

        private void BindButtons()
        {
            BindButton(openButton, Open);
            BindButton(closeButton, Close);
            BindButton(artifactDebugButton, OpenArtifactDebug);
            BindButton(artifactAcquisitionCloseButton, CloseArtifactAcquisitionDebug);
            BindButton(modifierCloseButton, CloseModifiedPanel);
            BindButton(passiveModifierCloseButton, CloseModifiedPanel);

            skillButtons = EnsureButtonArray(skillButtons, DebugSlots.Length);
            for (var i = 0; i < skillButtons.Length && i < DebugSlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(skillButtons[i], () => TryLearnSlot(capturedIndex));
            }

            modifierOpenButtons = EnsureButtonArray(modifierOpenButtons, DebugSlots.Length);
            for (var i = 0; i < modifierOpenButtons.Length && i < DebugSlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(modifierOpenButtons[i], () => OpenModifiedPanelForSlot(capturedIndex));
            }

            traitButtons = EnsureButtonArray(traitButtons, TraitButtonCount);
            for (var i = 0; i < traitButtons.Length; i++)
            {
                var capturedIndex = i;
                BindButton(traitButtons[i], () => ApplyModifierChoice(false, capturedIndex));
            }

            masterButtons = EnsureButtonArray(masterButtons, MasterButtonCount);
            for (var i = 0; i < masterButtons.Length; i++)
            {
                var capturedIndex = i;
                BindButton(masterButtons[i], () => ApplyModifierChoice(true, capturedIndex));
            }

            passiveTraitButtons = EnsureButtonArray(passiveTraitButtons, PassiveTraitButtonCount);
            for (var i = 0; i < passiveTraitButtons.Length; i++)
            {
                var capturedIndex = i;
                BindButton(passiveTraitButtons[i], () => ApplyPassiveModifierChoice(capturedIndex));
            }
        }

        private RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        private static Button[] EnsureButtonArray(Button[] buttons, int count)
        {
            return buttons != null && buttons.Length == count ? buttons : new Button[count];
        }

        private CombatUnitEntry ResolveSelectedPlayerEntry()
        {
            var manager = combatManager;
            if (manager == null)
            {
                return null;
            }

            var players = manager.Units.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var identity = players[i].Model.Identity;
                if (identity.Role == UnitRole.Monster && identity.SlotIndex == 0)
                {
                    return players[i];
                }
            }

            return null;
        }

        private bool TryResolveSelectedMonster(out RunSession session, out MonsterDefinition monster)
        {
            session = ResolveSession();
            monster = null;
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model : null;
            var monsterId = ResolveMonsterId(session, model);
            if (session == null || string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            return monster != null;
        }

        private static string ResolveMonsterId(RunSession session, UnitCombatState model)
        {
            if (model != null && model.Identity != null && !string.IsNullOrWhiteSpace(model.Identity.DefinitionId))
            {
                return model.Identity.DefinitionId;
            }

            if (session != null)
            {
                return session.SelectedMonsterId;
            }

            return string.Empty;
        }

        private void CloseModifiedPanel()
        {
            activeModifierSlotIndex = -1;
            activeModifierIsPassive = false;
            UiObjectUtility.SetActive(debugModifiedPanel, false);
            UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
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
                UiObjectUtility.SetActive(debugModifiedPanel, false);
                UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
                return;
            }

            var state = monster != null ? session.GetPartyMemberState(monster.MonsterId) : null;
            if (sourceSkill == null
                || string.IsNullOrWhiteSpace(sourceSkill.SkillId)
                || monster == null
                || state == null
                || !state.Skills.HasActiveSkill(sourceSkill.SkillId))
            {
                UiObjectUtility.SetActive(debugModifiedPanel, false);
                UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
                return;
            }

            activeModifierSlotIndex = slotIndex;
            activeModifierIsPassive = false;
            UiObjectUtility.SetActive(debugModifiedPanel, true);
            UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
            RefreshModifierChoiceButtons();
        }

        private void OpenPassiveModifiedPanelForSlot(int slotIndex)
        {
            if (!TryResolvePassiveModifierContext(slotIndex, out var session, out var passive, out var monster))
            {
                UiObjectUtility.SetActive(debugModifiedPanel, false);
                UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
                return;
            }

            var state = monster != null ? session.GetPartyMemberState(monster.MonsterId) : null;
            if (passive == null
                || string.IsNullOrWhiteSpace(passive.SkillId)
                || monster == null
                || state == null
                || !state.Skills.HasPassiveSkill(passive.SkillId))
            {
                UiObjectUtility.SetActive(debugModifiedPanel, false);
                UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
                return;
            }

            activeModifierSlotIndex = slotIndex;
            activeModifierIsPassive = true;
            UiObjectUtility.SetActive(debugModifiedPanel, false);
            UiObjectUtility.SetActive(debugPassiveModifiedPanel, true);
            RefreshModifierChoiceButtons();
        }

        private void ApplyModifierChoice(bool masterChoice, int choiceIndex)
        {
            if (!TryResolveModifierContext(activeModifierSlotIndex, out var session, out var sourceSkill, out var monster))
            {
                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (state == null || sourceSkill == null || string.IsNullOrWhiteSpace(sourceSkill.SkillId))
            {
                return;
            }

            var choices = masterChoice ? sourceSkill.MasterChoices : sourceSkill.EnhancementChoices;
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            {
                return;
            }

            var choice = choices[choiceIndex];
            if (!session.CanChooseSkillChoice(state, sourceSkill.SkillId, choice))
            {
                return;
            }

            CommitDebugOfferingChoice(session, monster, choice, sourceSkill.SkillId, string.Empty);

            RefreshModifierChoiceButtons();
        }

        private void ApplyPassiveModifierChoice(int choiceIndex)
        {
            if (!TryResolvePassiveModifierContext(activeModifierSlotIndex, out var session, out var passive, out var monster))
            {
                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (state == null || passive == null || string.IsNullOrWhiteSpace(passive.SkillId))
            {
                return;
            }

            var choices = passive.EnhancementChoices;
            if (choices == null || choiceIndex < 0 || choiceIndex >= choices.Length)
            {
                return;
            }

            var choice = choices[choiceIndex];
            if (!session.CanChooseSkillChoice(state, passive.SkillId, choice))
            {
                return;
            }

            CommitDebugOfferingChoice(session, monster, choice, string.Empty, passive.SkillId);
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
                    UiObjectUtility.SetActive(debugModifiedPanel, false);
                    UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
                }

                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (state == null)
            {
                SetModifierButtonsInactive(traitButtons);
                SetModifierButtonsInactive(masterButtons);
                SetModifierButtonsInactive(passiveTraitButtons);
                return;
            }

            var enhancementChoices = sourceSkill != null && sourceSkill.EnhancementChoices != null
                ? sourceSkill.EnhancementChoices
                : Array.Empty<SkillChoice>();
            var masterChoices = sourceSkill != null && sourceSkill.MasterChoices != null
                ? sourceSkill.MasterChoices
                : Array.Empty<SkillChoice>();

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
                    UiObjectUtility.SetActive(debugModifiedPanel, false);
                    UiObjectUtility.SetActive(debugPassiveModifiedPanel, false);
                }

                return;
            }

            var state = session.GetPartyMemberState(monster.MonsterId);
            if (state == null)
            {
                SetModifierButtonsInactive(passiveTraitButtons);
                SetModifierButtonsInactive(traitButtons);
                SetModifierButtonsInactive(masterButtons);
                return;
            }

            var enhancementChoices = passive != null && passive.EnhancementChoices != null
                ? passive.EnhancementChoices
                : Array.Empty<SkillChoice>();

            BindModifierChoiceButtons(passiveTraitButtons, enhancementChoices, session, state, passive.SkillId);
            SetModifierButtonsInactive(traitButtons);
            SetModifierButtonsInactive(masterButtons);
        }

        private static void BindModifierChoiceButtons(
            Button[] buttons,
            SkillChoice[] choices,
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
                button.interactable = hasChoice && session.CanChooseSkillChoice(state, sourceSkillId, choices[i]);

                var label = button.GetComponentInChildren<TMP_Text>(true);
                if (label != null)
                {
                    label.text = hasChoice ? BuildModifierButtonLabel(choices[i]) : string.Empty;
                }
            }
        }

        private static string BuildModifierButtonLabel(SkillChoice choice)
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
            session = null;
            sourceSkill = null;
            monster = null;
            if (slotIndex < 0 || slotIndex >= DebugSlots.Length
                || !TryResolveSelectedMonster(out session, out monster))
            {
                return false;
            }

            sourceSkill = GameDataLoader.CurrentCatalog.GetActiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex]);
            return sourceSkill != null;
        }

        private bool TryResolvePassiveModifierContext(
            int slotIndex,
            out RunSession session,
            out PassiveSkillDefinition passive,
            out MonsterDefinition monster)
        {
            session = null;
            passive = null;
            monster = null;
            if (slotIndex < 0 || slotIndex >= DebugSlots.Length
                || !IsPassiveSlot(slotIndex)
                || !TryResolveSelectedMonster(out session, out monster))
            {
                return false;
            }

            passive = GameDataLoader.CurrentCatalog.ResolvePassiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex]);
            return passive != null;
        }

        private void CommitDebugOfferingChoice(
            RunSession session,
            MonsterDefinition monster,
            SkillChoice choice,
            string activeSkillId,
            string passiveSkillId)
        {
            if (session == null || monster == null)
            {
                return;
            }

            var rewardId = ResolveRewardId(monster, choice, activeSkillId, passiveSkillId);
            string choiceId = string.Empty;
            if (choice != null)
            {
                choiceId = choice.ChoiceId;
            }
            var state = session.GetPartyMemberState(monster.MonsterId);
            if (state == null)
            {
                return;
            }

            session.RecordOfferingChoice(state, rewardId, choiceId, activeSkillId, passiveSkillId);

            RefreshRuntimeSkillModels();
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            monsterPanelUI?.RefreshNow();
        }

        private static string ResolveRewardId(
            MonsterDefinition monster,
            SkillChoice choice,
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

            var rewards = GameDataLoader.CurrentCatalog.GetRewardChoices(monster.MonsterId);
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

        private static bool IsPassiveSlot(int slotIndex)
        {
            return slotIndex >= 0
                && slotIndex < DebugSlots.Length
                && DebugSlots[slotIndex] >= SkillSlot.F;
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

        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            var valid = true;
            debugRootPanel = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Debug/DebugPanel",
                nameof(debugRootPanel),
                ref valid);
            debugPanel = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Debug/DebugPanel/DebugUI",
                nameof(debugPanel),
                ref valid);
            debugModifiedPanel = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Debug/DebugPanel/DebugModifiedUI",
                nameof(debugModifiedPanel),
                ref valid);
            debugPassiveModifiedPanel = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Debug/DebugPanel/DebugPassiveModifiedUI",
                nameof(debugPassiveModifiedPanel),
                ref valid);
            artifactDebugPanel = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Debug/DebugPanel/ArtifactDebugUI",
                nameof(artifactDebugPanel),
                ref valid);
            artifactAcquisitionDebugPanel = UiBindingUtility.BindChildObject(
                this,
                transform,
                "Debug/DebugPanel/ArtifactAchiveDebugUI",
                nameof(artifactAcquisitionDebugPanel),
                ref valid);
            openButton = UiBindingUtility.BindChild<Button>(
                this,
                "Debug/DebugPanel/DebugUIBtn",
                nameof(openButton),
                ref valid);
            closeButton = UiBindingUtility.BindChild<Button>(
                this,
                "Debug/DebugPanel/DebugUI/Close",
                nameof(closeButton),
                ref valid);
            artifactDebugButton = UiBindingUtility.BindChild<Button>(
                this,
                "Debug/DebugPanel/DebugUI/ArtifactDebug",
                nameof(artifactDebugButton),
                ref valid);
            artifactAcquisitionCloseButton = UiBindingUtility.BindChild<Button>(
                this,
                "Debug/DebugPanel/ArtifactAchiveDebugUI/Close",
                nameof(artifactAcquisitionCloseButton),
                ref valid);

            skillButtons = new Button[DebugSlots.Length];
            modifierOpenButtons = new Button[DebugSlots.Length];
            var skillNames = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" };
            var modifierNames = new[]
            {
                "AmodifierBtn",
                "BmodifierBtn",
                "CmodifierBtn",
                "DmodifierBtn",
                "EmodifierBtn",
                "EmodifierBtn",
                "EmodifierBtn",
                "EmodifierBtn",
                "EmodifierBtn",
                "EmodifierBtn"
            };
            for (var i = 0; i < skillButtons.Length; i++)
            {
                var slotPath = $"Debug/DebugPanel/DebugUI/{skillNames[i]}Btn";
                skillButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    slotPath,
                    $"skillButtons[{i}]",
                    ref valid);
                modifierOpenButtons[i] = UiBindingUtility.BindOptionalChild<Button>(
                    transform,
                    $"{slotPath}/{modifierNames[i]}");
            }

            artifactSynergyButtons = new Button[ArtifactDebugButtonCount];
            artifactSynergyLabels = new TMP_Text[ArtifactDebugButtonCount];
            artifactButtons = new Button[ArtifactDebugButtonCount];
            artifactLabels = new TMP_Text[ArtifactDebugButtonCount];
            for (var i = 0; i < ArtifactDebugButtonCount; i++)
            {
                var buttonName = $"{(char)('A' + i)}Btn";
                artifactSynergyButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    $"Debug/DebugPanel/ArtifactDebugUI/{buttonName}",
                    $"artifactSynergyButtons[{i}]",
                    ref valid);
                artifactSynergyLabels[i] = BindDebugLabel(
                    artifactSynergyButtons[i],
                    $"artifactSynergyLabels[{i}]",
                    ref valid);
                artifactButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    $"Debug/DebugPanel/ArtifactAchiveDebugUI/{buttonName}",
                    $"artifactButtons[{i}]",
                    ref valid);
                artifactLabels[i] = BindDebugLabel(
                    artifactButtons[i],
                    $"artifactLabels[{i}]",
                    ref valid);
            }

            modifierCloseButton = UiBindingUtility.BindChild<Button>(
                this,
                "Debug/DebugPanel/DebugModifiedUI/Close",
                nameof(modifierCloseButton),
                ref valid);
            passiveModifierCloseButton = UiBindingUtility.BindChild<Button>(
                this,
                "Debug/DebugPanel/DebugPassiveModifiedUI/Close",
                nameof(passiveModifierCloseButton),
                ref valid);

            traitButtons = new Button[TraitButtonCount];
            for (var i = 0; i < traitButtons.Length; i++)
            {
                traitButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    $"Debug/DebugPanel/DebugModifiedUI/Trait{i + 1}",
                    $"traitButtons[{i}]",
                    ref valid);
            }

            masterButtons = new Button[MasterButtonCount];
            for (var i = 0; i < masterButtons.Length; i++)
            {
                masterButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    $"Debug/DebugPanel/DebugModifiedUI/Master{i + 1}",
                    $"masterButtons[{i}]",
                    ref valid);
            }

            passiveTraitButtons = new Button[PassiveTraitButtonCount];
            for (var i = 0; i < passiveTraitButtons.Length; i++)
            {
                passiveTraitButtons[i] = UiBindingUtility.BindChild<Button>(
                    this,
                    $"Debug/DebugPanel/DebugPassiveModifiedUI/Trait{i + 1}",
                    $"passiveTraitButtons[{i}]",
                    ref valid);
            }

            stageManager = UiBindingUtility.BindSceneComponent<StageManager>(
                this,
                nameof(stageManager),
                ref valid);
            combatManager = UiBindingUtility.BindSceneComponent<InGameCombatManager>(
                this,
                nameof(combatManager),
                ref valid);
            monsterPanelUI = UiBindingUtility.BindSceneComponent<MonsterPanelUI>(
                this,
                nameof(monsterPanelUI),
                ref valid);
            uiManager = UiBindingUtility.BindSceneComponent<InGameUIManager>(
                this,
                nameof(uiManager),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

        private TMP_Text BindDebugLabel(Button button, string fieldName, ref bool valid)
        {
            var label = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            if (label == null)
            {
                Debug.LogError(
                    $"{GetType().Name} BindObject failed: field '{fieldName}' requires a TMP text child.",
                    this);
                valid = false;
            }

            return label;
        }

    }
}
