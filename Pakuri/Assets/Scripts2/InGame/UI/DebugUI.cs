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
        private static readonly InGameSkillSlot[] DebugSlots =
        {
            InGameSkillSlot.A,
            InGameSkillSlot.B,
            InGameSkillSlot.C,
            InGameSkillSlot.D,
            InGameSkillSlot.E
        };

        [SerializeField] private GameObject debugPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button[] skillButtons = new Button[5];
        [SerializeField] private StageManager stageManager;
        [SerializeField] private SceneEntryManager entryManager;
        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private MonsterPanelUI monsterPanelUI;

        private void Awake()
        {
            ResolveReferences();
            ResolveSceneUi();
            BindButtons();
            SetPanelVisible(false);
        }

        private void OnEnable()
        {
            ResolveReferences();
            RefreshButtonLabels();
            monsterPanelUI?.RefreshNow();
        }

        private void Update()
        {
            ResolveReferences();
        }

        public void Open()
        {
            SetPanelVisible(true);
            RefreshButtonLabels();
        }

        public void Close()
        {
            SetPanelVisible(false);
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

            if (openButton == null)
            {
                openButton = FindButton("DebugUIBtn") ?? FindButton("DebugBtn");
            }

            if (closeButton == null)
            {
                closeButton = FindButton("DebugUI/Close");
            }

            EnsureSkillButtonArray();
            ResolveSkillButton(0, "DebugUI/A Btn", "DebugUI/ABtn");
            ResolveSkillButton(1, "DebugUI/B Btn", "DebugUI/BBtn");
            ResolveSkillButton(2, "DebugUI/C Btn", "DebugUI/CBtn");
            ResolveSkillButton(3, "DebugUI/D Btn", "DebugUI/DBtn");
            ResolveSkillButton(4, "DebugUI/E Btn", "DebugUI/EBtn");
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

        private void BindButtons()
        {
            BindButton(openButton, Open);
            BindButton(closeButton, Close);

            EnsureSkillButtonArray();
            for (var i = 0; i < skillButtons.Length && i < DebugSlots.Length; i++)
            {
                var capturedIndex = i;
                BindButton(skillButtons[i], () => TryLearnSlot(capturedIndex));
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
