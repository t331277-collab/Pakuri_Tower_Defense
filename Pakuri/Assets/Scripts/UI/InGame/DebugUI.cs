using System;
using Pakuri.Data;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
 * 선택 몬스터의 스킬 학습과 선택지 적용을 직접 시험하는 디버그 UI 컴포넌트.
 * 활성·패시브 슬롯과 강화·최종 강화 버튼을 실제 카탈로그 기준으로 구성하고
 * 선택 결과를 RunSession의 공유 스킬 상태에 기록하고 런타임 스킬을 다시 만든다.
 */
namespace Pakuri.InGame
{
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

        /*
         * Unity가 컴포넌트를 초기화할 때 필요한 참조와 상태를 준비한다.
         */
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

        /*
         * 컴포넌트가 활성화될 때 이벤트와 표시 상태를 연결한다.
         */
        private void OnEnable()
        {
            ResolveReferences();
            RefreshButtonLabels();
            RefreshModifierChoiceButtons();
            monsterPanelUI?.RefreshNow();
        }

        /*
         * 매 프레임 현재 상태를 갱신한다.
         */
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

        /*
         * Open 작업을 수행한다.
         */
        public void Open()
        {
            SetPanelVisible(true);
            CloseModifiedPanel();
            RefreshButtonLabels();
        }

        /*
         * Close 작업을 수행한다.
         */
        public void Close()
        {
            SetPanelVisible(false);
            CloseModifiedPanel();
        }

        /*
         * TryLearnSlot 작업을 시도하고 성공 여부를 반환한다.
         */
        private void TryLearnSlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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

        /*
         * TryLearnPassiveSlot 작업을 시도하고 성공 여부를 반환한다.
         */
        private void TryLearnPassiveSlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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

        /*
         * RefreshRuntimeSkillModels 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * RefreshButtonLabels 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * ResolveReferences에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * ResolveSceneUi에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * ResolveSkillButton에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolveSkillButton(int index /* 목록에서의 순서 번호 */, string primaryPath /* 주 대상 경로 */, string fallbackPath /* 대상을 찾지 못했을 때 사용할 경로 */)
        {
            if (index < 0 || index >= skillButtons.Length || skillButtons[index] != null)
            {
                return;
            }

            skillButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /*
         * EnsureSkillButtonArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
        private void EnsureSkillButtonArray()
        {
            if (skillButtons == null || skillButtons.Length != DebugSlots.Length)
            {
                skillButtons = new Button[DebugSlots.Length];
            }
        }

        /*
         * ResolveModifierOpenButton에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolveModifierOpenButton(int index /* 목록에서의 순서 번호 */, string primaryPath /* 주 대상 경로 */, string fallbackPath /* 대상을 찾지 못했을 때 사용할 경로 */)
        {
            if (index < 0 || index >= modifierOpenButtons.Length || modifierOpenButtons[index] != null)
            {
                return;
            }

            modifierOpenButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /*
         * EnsureModifierOpenButtonArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
        private void EnsureModifierOpenButtonArray()
        {
            if (modifierOpenButtons == null || modifierOpenButtons.Length != DebugSlots.Length)
            {
                modifierOpenButtons = new Button[DebugSlots.Length];
            }
        }

        /*
         * ResolveTraitButton에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolveTraitButton(int index /* 목록에서의 순서 번호 */, string primaryPath /* 주 대상 경로 */, string fallbackPath /* 대상을 찾지 못했을 때 사용할 경로 */)
        {
            if (index < 0 || index >= traitButtons.Length || traitButtons[index] != null)
            {
                return;
            }

            traitButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /*
         * EnsureTraitButtonArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
        private void EnsureTraitButtonArray()
        {
            if (traitButtons == null || traitButtons.Length != TraitButtonCount)
            {
                traitButtons = new Button[TraitButtonCount];
            }
        }

        /*
         * ResolveMasterButton에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolveMasterButton(int index /* 목록에서의 순서 번호 */, string primaryPath /* 주 대상 경로 */, string fallbackPath /* 대상을 찾지 못했을 때 사용할 경로 */)
        {
            if (index < 0 || index >= masterButtons.Length || masterButtons[index] != null)
            {
                return;
            }

            masterButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /*
         * EnsureMasterButtonArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
        private void EnsureMasterButtonArray()
        {
            if (masterButtons == null || masterButtons.Length != MasterButtonCount)
            {
                masterButtons = new Button[MasterButtonCount];
            }
        }

        /*
         * ResolvePassiveTraitButton에 필요한 값을 계산해 현재 상태에 반영한다.
         */
        private void ResolvePassiveTraitButton(int index /* 목록에서의 순서 번호 */, string primaryPath /* 주 대상 경로 */, string fallbackPath /* 대상을 찾지 못했을 때 사용할 경로 */)
        {
            if (index < 0 || index >= passiveTraitButtons.Length || passiveTraitButtons[index] != null)
            {
                return;
            }

            passiveTraitButtons[index] = FindButton(primaryPath) ?? FindButton(fallbackPath);
        }

        /*
         * EnsurePassiveTraitButtonArray에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
        private void EnsurePassiveTraitButtonArray()
        {
            if (passiveTraitButtons == null || passiveTraitButtons.Length != PassiveTraitButtonCount)
            {
                passiveTraitButtons = new Button[PassiveTraitButtonCount];
            }
        }

        /*
         * BindButtons에 필요한 값을 설정한다.
         */
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

        /*
         * ResolveSession 결과를 계산해 반환한다.
         */
        private RunSession ResolveSession()
        {
            return stageManager != null ? stageManager.ActiveSession : null;
        }

        /*
         * ResolveCatalog 결과를 계산해 반환한다.
         */
        private GameDataCatalog ResolveCatalog()
        {
            return GameDataLoader.CurrentCatalog;
        }

        /*
         * ResolveCombatManager 결과를 계산해 반환한다.
         */
        private InGameCombatManager ResolveCombatManager()
        {
            ResolveReferences();
            return combatManager;
        }

        /*
         * ResolveSelectedPlayerEntry 결과를 계산해 반환한다.
         */
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

        /*
         * ResolveMonsterId 결과를 계산해 반환한다.
         */
        private static string ResolveMonsterId(RunSession session /* 현재 게임 진행 상태 */, UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
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

        /*
         * SetDebugRootPanelVisible에 필요한 값을 설정한다.
         */
        private void SetDebugRootPanelVisible(bool visible /* 화면 표시 여부 */)
        {
            if (debugRootPanel != null)
            {
                debugRootPanel.SetActive(visible);
            }
        }

        /*
         * SetPanelVisible에 필요한 값을 설정한다.
         */
        private void SetPanelVisible(bool visible /* 화면 표시 여부 */)
        {
            if (debugPanel != null)
            {
                debugPanel.SetActive(visible);
            }
        }

        /*
         * SetModifiedPanelVisible에 필요한 값을 설정한다.
         */
        private void SetModifiedPanelVisible(bool visible /* 화면 표시 여부 */)
        {
            if (debugModifiedPanel != null)
            {
                debugModifiedPanel.SetActive(visible);
            }
        }

        /*
         * SetPassiveModifiedPanelVisible에 필요한 값을 설정한다.
         */
        private void SetPassiveModifiedPanelVisible(bool visible /* 화면 표시 여부 */)
        {
            if (debugPassiveModifiedPanel != null)
            {
                debugPassiveModifiedPanel.SetActive(visible);
            }
        }

        /*
         * CloseModifiedPanel 작업을 수행한다.
         */
        private void CloseModifiedPanel()
        {
            activeModifierSlotIndex = -1;
            activeModifierIsPassive = false;
            SetModifiedPanelVisible(false);
            SetPassiveModifiedPanelVisible(false);
        }

        /*
         * OpenModifiedPanelForSlot 작업을 수행한다.
         */
        private void OpenModifiedPanelForSlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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

        /*
         * OpenPassiveModifiedPanelForSlot 작업을 수행한다.
         */
        private void OpenPassiveModifiedPanelForSlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
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

        /*
         * ApplyModifierChoice 처리를 대상에 적용한다.
         */
        private void ApplyModifierChoice(bool masterChoice /* 마스터 선택지 여부 */, int choiceIndex /* 선택지 순서 번호 */)
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

        /*
         * ApplyPassiveModifierChoice 처리를 대상에 적용한다.
         */
        private void ApplyPassiveModifierChoice(int choiceIndex /* 선택지 순서 번호 */)
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

        /*
         * RefreshModifierChoiceButtons 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * RefreshPassiveModifierChoiceButtons 대상의 현재 상태를 갱신한다.
         */
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

        /*
         * BindModifierChoiceButtons에 필요한 값을 설정한다.
         */
        private static void BindModifierChoiceButtons(
            Button[] buttons /* 버튼 목록 */,
            SkillChoice[] choices /* 선택지 목록 */,
            RunSession session /* 현재 게임 진행 상태 */,
            RunSession.RunMonsterState state /* 상태 */,
            string sourceSkillId /* 효과를 발생시킨 스킬 식별자 */)
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

        /*
         * BuildModifierButtonLabel에 필요한 결과를 만들어 반환한다.
         */
        private static string BuildModifierButtonLabel(SkillChoice choice /* 적용하거나 검사할 스킬 선택지 */)
        {
            if (choice == null)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(choice.DescriptionText)
                ? choice.Title
                : string.Format("{0}\n{1}", choice.Title, choice.DescriptionText);
        }

        /*
         * SetModifierButtonsInactive에 필요한 값을 설정한다.
         */
        private static void SetModifierButtonsInactive(Button[] buttons /* 버튼 목록 */)
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

        /*
         * TryResolveModifierContext 작업을 시도하고 성공 여부를 반환한다.
         */
        private bool TryResolveModifierContext(
            int slotIndex /* 배치할 슬롯 순서 번호 */,
            out RunSession session /* 현재 게임 진행 상태 */,
            out SkillDefinition sourceSkill /* 발생 원본 스킬 */,
            out MonsterDefinition monster /* 몬스터 */)
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

        /*
         * TryResolvePassiveModifierContext 작업을 시도하고 성공 여부를 반환한다.
         */
        private bool TryResolvePassiveModifierContext(
            int slotIndex /* 배치할 슬롯 순서 번호 */,
            out RunSession session /* 현재 게임 진행 상태 */,
            out PassiveSkillDefinition passive /* 패시브 */,
            out MonsterDefinition monster /* 몬스터 */)
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

        /*
         * CommitDebugOfferingChoice 작업을 수행한다.
         */
        private void CommitDebugOfferingChoice(
            RunSession session /* 현재 게임 진행 상태 */,
            GameDataCatalog catalog /* 불러온 게임 데이터 목록 */,
            MonsterDefinition monster /* 몬스터 */,
            SkillChoice choice /* 적용하거나 검사할 스킬 선택지 */,
            string activeSkillId /* 액티브 스킬 식별자 */,
            string passiveSkillId /* 패시브 스킬 식별자 */)
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

        /*
         * ResolveRewardId 결과를 계산해 반환한다.
         */
        private static string ResolveRewardId(
            MonsterDefinition monster /* 몬스터 */,
            SkillChoice choice /* 적용하거나 검사할 스킬 선택지 */,
            string activeSkillId /* 액티브 스킬 식별자 */,
            string passiveSkillId /* 패시브 스킬 식별자 */)
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

        /*
         * IsPassiveSlot 조건을 만족하는지 확인한다.
         */
        private static bool IsPassiveSlot(int slotIndex /* 배치할 슬롯 순서 번호 */)
        {
            return slotIndex >= 0
                && slotIndex < DebugSlots.Length
                && DebugSlots[slotIndex] >= SkillSlot.F;
        }

        /*
         * ResolveModifierButtonName 결과를 계산해 반환한다.
         */
        private static string ResolveModifierButtonName(SkillSlot slot /* 스킬이나 유닛이 배치될 슬롯 */)
        {
            return slot >= SkillSlot.E ? "EmodifierBtn" : $"{slot}modifierBtn";
        }

        /*
         * FindChildObject에 해당하는 값을 찾아 반환한다.
         */
        private GameObject FindChildObject(string path /* 불러오거나 검사할 경로 */)
        {
            var child = FindChild(path);
            return child != null ? child.gameObject : null;
        }

        /*
         * FindChild에 해당하는 값을 찾아 반환한다.
         */
        private Transform FindChild(string path /* 불러오거나 검사할 경로 */)
        {
            return transform.Find(path);
        }

        /*
         * FindButton에 해당하는 값을 찾아 반환한다.
         */
        private Button FindButton(string path /* 불러오거나 검사할 경로 */)
        {
            var child = FindChild(path);
            return child != null ? child.GetComponent<Button>() : null;
        }

        /*
         * BindButton에 필요한 값을 설정한다.
         */
        private static void BindButton(Button button /* 연결하거나 갱신할 버튼 */, UnityEngine.Events.UnityAction action /* 동작 */)
        {
            if (button == null || action == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        /*
         * FindSceneObject에 해당하는 값을 찾아 반환한다.
         */
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
