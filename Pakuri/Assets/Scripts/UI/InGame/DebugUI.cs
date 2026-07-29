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

    /// <summary><c>DebugUI</c> 상태를 Unity UI 또는 월드 오브젝트로 표시한다.</summary>
    public class DebugUI : MonoBehaviour
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

        /// <summary>Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.</summary>
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

        /// <summary>Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.</summary>
        private void OnEnable()
        {
            ResolveReferences();
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            monsterPanelUI?.RefreshNow();
        }

        /// <summary>현재 Unity 프레임에서 <c>Update</c> 갱신 동작을 진행한다.</summary>
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

        /// <summary><c>Open</c> 작업을 수행한다.</summary>
        public void Open()
        {
            SetPanelVisible(true);
            CloseModifiedPanel();
            RefreshButtonLabels();
        }

        /// <summary><c>Close</c> 작업을 수행한다.</summary>
        public void Close()
        {
            SetPanelVisible(false);
            CloseModifiedPanel();
        }

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>LearnSlot</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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
            UnitCombatState model = null;
            if (selectedEntry != null)
            {
                model = selectedEntry.Model;
            }
            var monsterId = ResolveMonsterId(session, model);
            if (session == null || string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            if (monster == null)
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

            CommitDebugOfferingChoice(session, catalog, monster, null, sourceSkill.SkillId, string.Empty);
        }

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>LearnPassiveSlot</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private void TryLearnPassiveSlot(int slotIndex)
        {
            var session = ResolveSession();
            var catalog = ResolveCatalog();
            var selectedEntry = ResolveSelectedPlayerEntry();
            UnitCombatState model = null;
            if (selectedEntry != null)
            {
                model = selectedEntry.Model;
            }
            var monsterId = ResolveMonsterId(session, model);
            if (session == null || string.IsNullOrWhiteSpace(monsterId))
            {
                return;
            }

            var monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            if (monster == null)
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

            CommitDebugOfferingChoice(session, catalog, monster, null, string.Empty, passive.SkillId);
        }

        /// <summary><c>RuntimeSkillModels</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
        private void RefreshRuntimeSkillModels()
        {
            var manager = ResolveCombatManager();
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
                SkillExecution.RebuildLearnedSkillState(model);
                manager.Units.RefreshDisplay(model);
            }
        }

        /// <summary><c>ButtonLabels</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
        private void RefreshButtonLabels()
        {
            var session = ResolveSession();
            var catalog = ResolveCatalog();
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

        /// <summary><c>References</c>를 결정한다.</summary>
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

        /// <summary><c>SceneUi</c>를 결정한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>SkillButton</c>를 결정한다.</summary>
        private void ResolveSkillButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= skillButtons.Length || skillButtons[index] != null)
            {
                return;
            }

            skillButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /// <summary><c>EnsureSkillButtonArray</c> 작업을 수행한다.</summary>
        private void EnsureSkillButtonArray()
        {
            if (skillButtons == null || skillButtons.Length != DebugSlots.Length)
            {
                skillButtons = new Button[DebugSlots.Length];
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ModifierOpenButton</c>를 결정한다.</summary>
        private void ResolveModifierOpenButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= modifierOpenButtons.Length || modifierOpenButtons[index] != null)
            {
                return;
            }

            modifierOpenButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /// <summary><c>EnsureModifierOpenButtonArray</c> 작업을 수행한다.</summary>
        private void EnsureModifierOpenButtonArray()
        {
            if (modifierOpenButtons == null || modifierOpenButtons.Length != DebugSlots.Length)
            {
                modifierOpenButtons = new Button[DebugSlots.Length];
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>TraitButton</c>를 결정한다.</summary>
        private void ResolveTraitButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= traitButtons.Length || traitButtons[index] != null)
            {
                return;
            }

            traitButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /// <summary><c>EnsureTraitButtonArray</c> 작업을 수행한다.</summary>
        private void EnsureTraitButtonArray()
        {
            if (traitButtons == null || traitButtons.Length != TraitButtonCount)
            {
                traitButtons = new Button[TraitButtonCount];
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>MasterButton</c>를 결정한다.</summary>
        private void ResolveMasterButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= masterButtons.Length || masterButtons[index] != null)
            {
                return;
            }

            masterButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /// <summary><c>EnsureMasterButtonArray</c> 작업을 수행한다.</summary>
        private void EnsureMasterButtonArray()
        {
            if (masterButtons == null || masterButtons.Length != MasterButtonCount)
            {
                masterButtons = new Button[MasterButtonCount];
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>PassiveTraitButton</c>를 결정한다.</summary>
        private void ResolvePassiveTraitButton(int index, string primaryPath, string fallbackPath)
        {
            if (index < 0 || index >= passiveTraitButtons.Length || passiveTraitButtons[index] != null)
            {
                return;
            }

            passiveTraitButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /// <summary><c>EnsurePassiveTraitButtonArray</c> 작업을 수행한다.</summary>
        private void EnsurePassiveTraitButtonArray()
        {
            if (passiveTraitButtons == null || passiveTraitButtons.Length != PassiveTraitButtonCount)
            {
                passiveTraitButtons = new Button[PassiveTraitButtonCount];
            }
        }

        /// <summary><c>Buttons</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
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

        /// <summary><c>Session</c>를 결정한다.</summary>
        private RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        /// <summary><c>Catalog</c>를 결정한다.</summary>
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /// <summary><c>CombatManager</c>를 결정한다.</summary>
        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        /// <summary><c>SelectedPlayerEntry</c>를 결정한다.</summary>
        private CombatUnitEntry ResolveSelectedPlayerEntry()
        {
            var manager = ResolveCombatManager();
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>MonsterId</c>를 결정한다.</summary>
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

        /// <summary>전달된 <c>visible</c> 값을 사용해 <c>DebugRootPanelVisible</c>를 갱신한다.</summary>
        private void SetDebugRootPanelVisible(bool visible)
        {
            if (debugRootPanel != null)
            {
                debugRootPanel.SetActive(visible);
            }
        }

        /// <summary>전달된 <c>visible</c> 값을 사용해 <c>PanelVisible</c>를 갱신한다.</summary>
        private void SetPanelVisible(bool visible)
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(visible);
            }
        }

        /// <summary>전달된 <c>visible</c> 값을 사용해 <c>ModifiedPanelVisible</c>를 갱신한다.</summary>
        private void SetModifiedPanelVisible(bool visible)
        {
            if (debugModifiedPanel != null)
            {
                debugModifiedPanel.SetActive(visible);
            }
        }

        /// <summary>전달된 <c>visible</c> 값을 사용해 <c>PassiveModifiedPanelVisible</c>를 갱신한다.</summary>
        private void SetPassiveModifiedPanelVisible(bool visible)
        {
            if (debugPassiveModifiedPanel != null)
            {
                debugPassiveModifiedPanel.SetActive(visible);
            }
        }

        /// <summary><c>CloseModifiedPanel</c> 작업을 수행한다.</summary>
        private void CloseModifiedPanel()
        {
            activeModifierSlotIndex = -1;
            activeModifierIsPassive = false;
            SetModifiedPanelVisible(false);
            SetPassiveModifiedPanelVisible(false);
        }

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>OpenModifiedPanelForSlot</c> 작업을 수행한다.</summary>
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

            var state = monster != null ? session.GetPartyMemberState(monster.MonsterId) : null;
            if (sourceSkill == null
                || string.IsNullOrWhiteSpace(sourceSkill.SkillId)
                || monster == null
                || state == null
                || !state.Skills.HasActiveSkill(sourceSkill.SkillId))
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

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>OpenPassiveModifiedPanelForSlot</c> 작업을 수행한다.</summary>
        private void OpenPassiveModifiedPanelForSlot(int slotIndex)
        {
            if (!TryResolvePassiveModifierContext(slotIndex, out var session, out var passive, out var monster))
            {
                SetModifiedPanelVisible(false);
                SetPassiveModifiedPanelVisible(false);
                return;
            }

            var state = monster != null ? session.GetPartyMemberState(monster.MonsterId) : null;
            if (passive == null
                || string.IsNullOrWhiteSpace(passive.SkillId)
                || monster == null
                || state == null
                || !state.Skills.HasPassiveSkill(passive.SkillId))
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ModifierChoice</c>를 적용한다.</summary>
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

            CommitDebugOfferingChoice(session, ResolveCatalog(), monster, choice, sourceSkill.SkillId, string.Empty);

            RefreshModifierChoiceButtons();
        }

        /// <summary>전달된 <c>choiceIndex</c> 값을 사용해 <c>PassiveModifierChoice</c>를 적용한다.</summary>
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

            CommitDebugOfferingChoice(session, ResolveCatalog(), monster, choice, string.Empty, passive.SkillId);
            RefreshModifierChoiceButtons();
        }

        /// <summary><c>ModifierChoiceButtons</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
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

        /// <summary><c>PassiveModifierChoiceButtons</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ModifierChoiceButtons</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
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

        /// <summary>전달된 <c>choice</c> 값을 사용해 <c>ModifierButtonLabel</c>를 구성한다.</summary>
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

        /// <summary>전달된 <c>buttons</c> 값을 사용해 <c>ModifierButtonsInactive</c>를 갱신한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResolveModifierContext</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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
            UnitCombatState model = null;
            if (selectedEntry != null)
            {
                model = selectedEntry.Model;
            }
            var monsterId = ResolveMonsterId(session, model);
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            if (monster == null)
            {
                return false;
            }

            sourceSkill = GameDataLoader.CurrentCatalog.GetActiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex]);
            return sourceSkill != null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResolvePassiveModifierContext</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        private bool TryResolvePassiveModifierContext(
            int slotIndex,
            out RunSession session,
            out PassiveSkillDefinition passive,
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
            UnitCombatState model = null;
            if (selectedEntry != null)
            {
                model = selectedEntry.Model;
            }
            var monsterId = ResolveMonsterId(session, model);
            if (string.IsNullOrWhiteSpace(monsterId))
            {
                return false;
            }

            monster = GameDataLoader.CurrentCatalog.GetMonster(monsterId);
            if (monster == null)
            {
                return false;
            }

            passive = GameDataLoader.CurrentCatalog.ResolvePassiveSkill(
                monster.MonsterId,
                DebugSlots[slotIndex]);
            return passive != null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>CommitDebugOfferingChoice</c> 작업을 수행한다.</summary>
        private void CommitDebugOfferingChoice(
            RunSession session,
            GameDataCatalog catalog,
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>RewardId</c>를 결정한다.</summary>
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

        /// <summary>전달된 <c>slotIndex</c> 값을 사용해 <c>PassiveSlot</c> 조건 충족 여부를 반환한다.</summary>
        private static bool IsPassiveSlot(int slotIndex)
        {
            return slotIndex >= 0
                && slotIndex < DebugSlots.Length
                && DebugSlots[slotIndex] >= SkillSlot.F;
        }

        /// <summary>전달된 <c>slot</c> 값을 사용해 <c>ModifierButtonName</c>를 결정한다.</summary>
        private static string ResolveModifierButtonName(SkillSlot slot)
        {
            return slot >= SkillSlot.E ? "EmodifierBtn" : $"{slot}modifierBtn";
        }

        /// <summary>전달된 <c>path</c> 값을 사용해 <c>ChildObject</c>를 찾는다.</summary>
        private GameObject FindChildObject(string path)
        {
            var child = FindChild(path);
            return child != null ? child.gameObject : null;
        }

        /// <summary>전달된 <c>path</c> 값을 사용해 <c>Child</c>를 찾는다.</summary>
        private Transform FindChild(string path)
        {
            return transform.Find(path);
        }

        /// <summary>전달된 <c>path</c> 값을 사용해 <c>Button</c>를 찾는다.</summary>
        private Button FindButton(string path)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Button>() : null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Button</c>를 런타임 사건 또는 씬 대상에 연결한다.</summary>
        private static void BindButton(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /// <summary><c>SceneObject</c>를 찾는다.</summary>
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
