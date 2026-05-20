using System;
using Pakuri.Data;
using Pakuri.Run;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class DebugUI : MonoBehaviour
    {
        private const int TraitButtonCount = 5;
        private const int MasterButtonCount = 2;

        private static readonly InGameSkillSlot[] DebugSlots =
        {
            InGameSkillSlot.A,
            InGameSkillSlot.B,
            InGameSkillSlot.C,
            InGameSkillSlot.D,
            InGameSkillSlot.E
        };

        [SerializeField] private GameObject debugPanel;
        [SerializeField] private GameObject debugModifiedPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button[] skillButtons = new Button[5];
        [SerializeField] private Button[] modifierOpenButtons = new Button[5];
        [SerializeField] private Button modifierCloseButton;
        [SerializeField] private Button[] traitButtons = new Button[TraitButtonCount];
        [SerializeField] private Button[] masterButtons = new Button[MasterButtonCount];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SceneEntryManager entryManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private MonsterPanelUI monsterPanelUI;

        private int activeModifierSlotIndex = -1;

        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindButtons();
            SetPanelVisible(false);
            SetModifiedPanelVisible(false);
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

            var session = ResolveSession();
            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            var model = selectedEntry != null ? selectedEntry.Model as MonsterUnitRuntimeModel : null;
            var monsterId = ResolveMonsterId(session, model);
            if (session == null || string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var monster = PakuriDataManager.Instance.ResolveMonster(monsterId, catalog);
            if (monster == null)
            {
                return;
            }

            var sourceSkill = PakuriDataManager.Instance.ResolveActiveSkill(
                monster.MonsterId,
                InGameSkillDefinitionMapper.MapSlot(DebugSlots[slotIndex]),
                monster);
            if (sourceSkill == null || string.IsNullOrWhiteSpace(sourceSkill.SkillId))
            {
                return;
            }

            if (session.HasLearnedActive(monster.MonsterId, sourceSkill.SkillId))
            {
                return;
            }

            session.EnsurePartyMemberState(monster);
            session.RecordOfferingChoice(monster.MonsterId, string.Empty, string.Empty, sourceSkill.SkillId, string.Empty);
            RefreshRuntimeSkillModels(session, catalog);
            RefreshButtonLabels();
            monsterPanelUI?.RefreshNow();
        }

        private void RefreshRuntimeSkillModels(RunSession session, GameDataCatalog catalog)
        {
            var manager = ResolveCombatManager();
            if (session == null || manager == null)
            {
                return;
            }

            var skillCatalog = new InGameSkillCatalog(catalog);
            var players = manager.Roster.Players;
            for (var i = 0; i < players.Count; i++)
            {
                var model = players[i] != null ? players[i].Model as MonsterUnitRuntimeModel : null;
                if (model == null)
                {
                    continue;
                }

                SyncModelStateFromSession(session, model);
                SkillRuntimeFactory.RebuildLearnedActiveSet(model, skillCatalog);
                manager.RefreshUnitActor(model);
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
            var monster = PakuriDataManager.Instance.ResolveMonster(monsterId, catalog);

            for (var i = 0; i < skillButtons.Length && i < DebugSlots.Length; i++)
            {
                var button = skillButtons[i];
                if (button == null)
                {
                    continue;
                }

                var label = button.GetComponentInChildren<TMP_Text>(true);
                var slot = DebugSlots[i];
                var sourceSkill = monster != null
                    ? PakuriDataManager.Instance.ResolveActiveSkill(monster.MonsterId, InGameSkillDefinitionMapper.MapSlot(slot), monster)
                    : null;
                var hasSkill = sourceSkill != null && !string.IsNullOrWhiteSpace(sourceSkill.SkillId);
                var learned = hasSkill && session != null && session.HasLearnedActive(monster.MonsterId, sourceSkill.SkillId);

                button.interactable = hasSkill && !learned;
                if (label != null)
                {
                    label.text = hasSkill
                        ? string.Format("{0}\n{1}", slot, learned ? "Learned" : sourceSkill.DisplayName)
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

            if (entryManager == null)
            {
                entryManager = FindSceneObject<SceneEntryManager>();
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
            if (debugPanel == null)
            {
                debugPanel = FindChildObject("DebugUI");
            }

            if (debugModifiedPanel == null)
            {
                debugModifiedPanel = FindChildObject("DebugModifiedUI");
            }

            if (openButton == null)
            {
                openButton = FindButton("DebugUIBtn") ?? FindButton("DebugBtn");
            }

            if (closeButton == null)
            {
                closeButton = FindButton("DebugUI/Close");
            }

            if (modifierCloseButton == null)
            {
                modifierCloseButton = FindButton("DebugModifiedUI/Close");
            }

            EnsureSkillButtonArray();
            ResolveSkillButton(0, "DebugUI/A Btn", "DebugUI/ABtn");
            ResolveSkillButton(1, "DebugUI/B Btn", "DebugUI/BBtn");
            ResolveSkillButton(2, "DebugUI/C Btn", "DebugUI/CBtn");
            ResolveSkillButton(3, "DebugUI/D Btn", "DebugUI/DBtn");
            ResolveSkillButton(4, "DebugUI/E Btn", "DebugUI/EBtn");

            EnsureModifierOpenButtonArray();
            ResolveModifierOpenButton(0, "DebugUI/A Btn/AmodifierBtn", "DebugUI/ABtn/AmodifierBtn");
            ResolveModifierOpenButton(1, "DebugUI/B Btn/BmodifierBtn", "DebugUI/BBtn/BmodifierBtn");
            ResolveModifierOpenButton(2, "DebugUI/C Btn/CmodifierBtn", "DebugUI/CBtn/CmodifierBtn");
            ResolveModifierOpenButton(3, "DebugUI/D Btn/DmodifierBtn", "DebugUI/DBtn/DmodifierBtn");
            ResolveModifierOpenButton(4, "DebugUI/E Btn/EmodifierBtn", "DebugUI/EBtn/EmodifierBtn");

            EnsureTraitButtonArray();
            ResolveTraitButton(0, "DebugModifiedUI/Trait1", "DebugModifiedUI/trait1");
            ResolveTraitButton(1, "DebugModifiedUI/Trait2", "DebugModifiedUI/trait2");
            ResolveTraitButton(2, "DebugModifiedUI/Trait3", "DebugModifiedUI/trait3");
            ResolveTraitButton(3, "DebugModifiedUI/Trait4", "DebugModifiedUI/trait4");
            ResolveTraitButton(4, "DebugModifiedUI/Trait5", "DebugModifiedUI/trait5");

            EnsureMasterButtonArray();
            ResolveMasterButton(0, "DebugModifiedUI/Master1", "DebugModifiedUI/master1");
            ResolveMasterButton(1, "DebugModifiedUI/Master2", "DebugModifiedUI/master2");
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

        private void BindButtons()
        {
            BindButton(openButton, Open);
            BindButton(closeButton, Close);
            BindButton(modifierCloseButton, CloseModifiedPanel);

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
        }

        private RunSession ResolveSession()
        {
            if (stageManager != null && stageManager.ActiveSession != null)
            {
                return stageManager.ActiveSession;
            }

            return entryManager != null ? entryManager.ActiveSession : null;
        }

        private GameDataCatalog ResolveCatalog()
        {
            var catalog = PakuriDataManager.Instance.CurrentCatalog;
            return catalog != null ? catalog : PakuriCsvRuntimeData.ResolveCatalogOrFallback(null);
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

        private void CloseModifiedPanel()
        {
            activeModifierSlotIndex = -1;
            SetModifiedPanelVisible(false);
        }

        private void OpenModifiedPanelForSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= DebugSlots.Length)
            {
                return;
            }

            if (!TryResolveModifierContext(slotIndex, out var session, out var sourceSkill, out var monster))
            {
                SetModifiedPanelVisible(false);
                return;
            }

            if (sourceSkill == null
                || string.IsNullOrWhiteSpace(sourceSkill.SkillId)
                || monster == null
                || !session.HasLearnedActive(monster.MonsterId, sourceSkill.SkillId))
            {
                SetModifiedPanelVisible(false);
                return;
            }

            activeModifierSlotIndex = slotIndex;
            SetModifiedPanelVisible(true);
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

            session.RecordOfferingChoice(
                monster.MonsterId,
                choice.ChoiceId,
                choice.ChoiceId,
                sourceSkill.SkillId,
                string.Empty);
            session.AccumulateReward(
                monster.MonsterId,
                choice.HasDamageMultiplier ? choice.DamageMultiplier : 1f,
                choice.HasMagazineBonus ? choice.MagazineBonus : 0,
                choice.HasShotIntervalMultiplier ? choice.ShotIntervalMultiplier : 1f,
                choice.HasReloadTimeMultiplier ? choice.ReloadTimeMultiplier : 1f,
                choice.HasMaxHealthBonus ? choice.MaxHealthBonus : 0f,
                choice.HasStatusChanceBonus ? choice.StatusChanceBonus : 0f);

            RefreshRuntimeSkillModels(session, ResolveCatalog());
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            monsterPanelUI?.RefreshNow();
        }

        private void RefreshModifierChoiceButtons()
        {
            if (!TryResolveModifierContext(activeModifierSlotIndex, out var session, out var sourceSkill, out var monster))
            {
                SetModifierButtonsInactive(traitButtons);
                SetModifierButtonsInactive(masterButtons);
                if (activeModifierSlotIndex < 0)
                {
                    SetModifiedPanelVisible(false);
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

            monster = PakuriDataManager.Instance.ResolveMonster(monsterId, catalog);
            if (monster == null)
            {
                return false;
            }

            sourceSkill = PakuriDataManager.Instance.ResolveActiveSkill(
                monster.MonsterId,
                InGameSkillDefinitionMapper.MapSlot(DebugSlots[slotIndex]),
                monster);
            return sourceSkill != null;
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

            return PakuriDataManager.Instance.TryGetData(choiceId, out SkillChoiceDefinition choice)
                ? choice
                : null;
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
