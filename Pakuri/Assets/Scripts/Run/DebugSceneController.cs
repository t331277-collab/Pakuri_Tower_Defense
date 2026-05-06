using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Pakuri.Run
{
    [DisallowMultipleComponent]
    public sealed class DebugSceneController : MonoBehaviour
    {
        [SerializeField] private GameDataCatalog gameDataCatalog;
        [SerializeField] private CombatRuntimeController combatController;

        private readonly Dictionary<SkillSlot, bool> activeSkillStates = new Dictionary<SkillSlot, bool>();
        private readonly Dictionary<SkillSlot, bool> passiveSkillStates = new Dictionary<SkillSlot, bool>();
        private readonly Dictionary<string, bool> choiceStates = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly List<Button> monsterButtons = new List<Button>();
        private readonly List<Toggle> skillToggles = new List<Toggle>();
        private readonly List<Toggle> modalChoiceToggles = new List<Toggle>();

        private Canvas rootCanvas;
        private CanvasScaler canvasScaler;
        private GraphicRaycaster graphicRaycaster;
        private Font uiFont;
        private Sprite solidUiSprite;
        private Texture2D generatedSolidUiTexture;
        private GameObject setupPanel;
        private GameObject monsterButtonRoot;
        private GameObject skillPanel;
        private GameObject skillToggleRoot;
        private GameObject enhancementModal;
        private GameObject modalChoiceRoot;
        private Text titleText;
        private Text statusText;
        private Text combatText;
        private Text modalTitleText;
        private Text modalSummaryText;
        private Button setupToggleButton;
        private Button startButton;
        private Button skillWindowButton;
        private Button closeSkillWindowButton;
        private Button closeModalButton;
        private CombatMonsterPanelUiController monsterPanelUi;
        private MonsterDefinition selectedMonster;
        private bool isRebuilding;

        private void Awake()
        {
            DisableRunSceneControllers();
            ResolveReferences();
            EnsureCanvasShell();
            EnsureEventSystem();
            BindSceneUi();
            BindMonsterPanelUi();
        }

        private void Start()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (combatController != null)
            {
                combatController.ResetPrototypeState();
            }

            var monsters = PakuriDataManager.Instance.GetMonsters(gameDataCatalog);
            if (monsters.Length == 0)
            {
                SetStatus("No monster data is available.");
                return;
            }

            SetStatus("Select a monster, configure skills, then press Start.");
        }

        private void Update()
        {
            BindMonsterPanelUi();
            RefreshCombatText();
        }

        private void DisableRunSceneControllers()
        {
            var bootstrap = FindFirstObjectByType<RunSceneBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.enabled = false;
            }

            var runCombatUi = FindFirstObjectByType<RunCombatUiController>();
            if (runCombatUi != null)
            {
                runCombatUi.enabled = false;
            }

            var runFlowUi = FindFirstObjectByType<RunFlowController>();
            if (runFlowUi != null)
            {
                runFlowUi.enabled = false;
            }
        }

        private void ResolveReferences()
        {
            if (Application.isPlaying)
            {
                gameDataCatalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(gameDataCatalog);
            }

            rootCanvas = GetComponent<Canvas>();
            canvasScaler = GetComponent<CanvasScaler>();
            graphicRaycaster = GetComponent<GraphicRaycaster>();
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            solidUiSprite = Resources.Load<Sprite>("DebugUiSolid");

            if (combatController == null)
            {
                combatController = FindFirstObjectByType<CombatRuntimeController>();
            }
        }

        private void BindMonsterPanelUi()
        {
            monsterPanelUi = GetComponent<CombatMonsterPanelUiController>();
            if (monsterPanelUi == null && transform.Find("MonsterPanel") != null)
            {
                monsterPanelUi = gameObject.AddComponent<CombatMonsterPanelUiController>();
            }

            if (monsterPanelUi != null)
            {
                monsterPanelUi.Bind(combatController);
            }
        }

        private void EnsureCanvasShell()
        {
            if (transform.localScale == Vector3.zero)
            {
                transform.localScale = Vector3.one;
            }

            if (rootCanvas == null)
            {
                rootCanvas = gameObject.AddComponent<Canvas>();
            }

            rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            rootCanvas.sortingOrder = 80;

            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;

            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private void BindSceneUi()
        {
            setupPanel = FindDirectChild("DebugSetupPanel");
            skillPanel = FindDirectChild("SkillDebugPanel");
            enhancementModal = FindDirectChild("EnhancementModal");

            if (setupPanel == null || skillPanel == null || enhancementModal == null)
            {
                Debug.LogError("DebugSceneController requires DebugSetupPanel, SkillDebugPanel, and EnhancementModal objects in DebugScene.");
                return;
            }

            titleText = FindText(setupPanel.transform, "Title");
            statusText = FindText(setupPanel.transform, "Status");
            combatText = FindText(setupPanel.transform, "CombatText");
            monsterButtonRoot = FindDirectChild(setupPanel.transform, "MonsterButtons");
            skillWindowButton = FindButton(setupPanel.transform, "SkillWindowButton");
            startButton = FindButton(setupPanel.transform, "StartButton");
            setupToggleButton = FindButton(transform, "SetupToggleButton");
            if (setupToggleButton == null)
            {
                setupToggleButton = EnsureSetupToggleButton();
            }

            BindButton(setupToggleButton, ToggleSetupPanelVisibility);
            BindButton(skillWindowButton, OpenSkillWindow);
            BindButton(startButton, StartCombat);

            skillToggleRoot = FindDirectChild(skillPanel.transform, "SkillScroll/Viewport/Content");
            closeSkillWindowButton = FindButton(skillPanel.transform, "CloseButton");
            BindButton(closeSkillWindowButton, CloseSkillWindow);
            skillPanel.SetActive(false);

            modalTitleText = FindText(enhancementModal.transform, "Title");
            modalSummaryText = FindText(enhancementModal.transform, "Summary");
            modalChoiceRoot = FindDirectChild(enhancementModal.transform, "ChoiceScroll/Viewport/Content");
            closeModalButton = FindButton(enhancementModal.transform, "CloseButton");
            BindButton(closeModalButton, CloseEnhancementModal);
            enhancementModal.SetActive(false);

            RebuildMonsterButtons();
            BindSkillToggleSlots();
            BindChoiceToggleSlots();
            RefreshStartButtonState();
            RefreshSetupToggleButtonState();
        }

        private void RebuildMonsterButtons()
        {
            monsterButtons.Clear();

            if (monsterButtonRoot == null)
            {
                Debug.LogError("DebugSceneController requires DebugSetupPanel/MonsterButtons in DebugScene.");
                return;
            }

            var monsters = PakuriDataManager.Instance.GetMonsters(gameDataCatalog);
            var buttonSlots = monsterButtonRoot.GetComponentsInChildren<Button>(true);
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null)
                {
                    continue;
                }

                if (i >= buttonSlots.Length)
                {
                    Debug.LogError($"DebugSceneController has no monster button slot for {monster.MonsterId}.");
                    break;
                }

                var captured = monster;
                var button = buttonSlots[i];
                button.gameObject.name = $"Monster_{monster.MonsterId}";
                button.gameObject.SetActive(true);
                SetButtonLabel(button, $"{monster.DisplayName} / {monster.MonsterId}");
                BindButton(button, () => SelectMonster(captured));
                monsterButtons.Add(button);
            }

            for (var i = monsters.Length; i < buttonSlots.Length; i++)
            {
                buttonSlots[i].gameObject.SetActive(false);
            }
        }

        private void SelectMonster(MonsterDefinition monster)
        {
            if (monster == null)
            {
                return;
            }

            selectedMonster = monster;
            InitializeStates(monster);
            RebuildSkillToggles();
            if (combatController != null)
            {
                combatController.ResetPrototypeState();
                combatController.ApplyDebugSelection(selectedMonster, BuildDebugSession(), gameDataCatalog);
            }

            SetStatus($"{monster.DisplayName} selected. Configure skills, then press Start.");
            RefreshStartButtonState();
        }

        private void InitializeStates(MonsterDefinition monster)
        {
            activeSkillStates.Clear();
            passiveSkillStates.Clear();
            choiceStates.Clear();

            var activeSkills = GetActiveSkills(monster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null)
                {
                    continue;
                }

                activeSkillStates[skill.Slot] = skill.Slot == SkillSlot.A || skill.IsDefaultLearned;
                RegisterChoiceState(skill.EnhancementChoices);
                RegisterChoiceState(skill.MasterSkillChoices);
            }

            var passiveSkills = GetPassiveSkills(monster);
            for (var i = 0; i < passiveSkills.Length; i++)
            {
                var passive = passiveSkills[i];
                if (passive == null)
                {
                    continue;
                }

                passiveSkillStates[passive.Slot] = false;
                RegisterChoiceState(passive.EnhancementChoices);
            }
        }

        private void RegisterChoiceState(SkillChoiceDefinition[] choices)
        {
            if (choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && !string.IsNullOrWhiteSpace(choice.ChoiceId))
                {
                    choiceStates[choice.ChoiceId] = false;
                }
            }
        }

        private void OpenSkillWindow()
        {
            if (selectedMonster == null)
            {
                SetStatus("Select a monster first.");
                return;
            }

            skillPanel.SetActive(true);
            RebuildSkillToggles();
            enhancementModal.SetActive(false);
        }

        private void CloseSkillWindow()
        {
            skillPanel.SetActive(false);
            ApplySelectionWithoutRestart();
        }

        private void RebuildSkillToggles()
        {
            isRebuilding = true;
            HideToggleSlots(skillToggles);

            var activeSkills = GetActiveSkills(selectedMonster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null)
                {
                    continue;
                }

                var captured = skill;
                ConfigureToggle(
                    FindSkillToggleSlot(false, skill.Slot),
                    $"{skill.Slot}: {skill.DisplayName}",
                    GetState(activeSkillStates, skill.Slot),
                    true,
                    value => OnActiveSkillToggle(captured, value));
            }

            var passiveSkills = GetPassiveSkills(selectedMonster);
            for (var i = 0; i < passiveSkills.Length; i++)
            {
                var passive = passiveSkills[i];
                if (passive == null)
                {
                    continue;
                }

                var captured = passive;
                var interactable = passive.Slot != SkillSlot.F || GetState(activeSkillStates, SkillSlot.A);
                if (!interactable)
                {
                    passiveSkillStates[passive.Slot] = false;
                }

                ConfigureToggle(
                    FindSkillToggleSlot(true, passive.Slot),
                    $"{passive.Slot}: {passive.DisplayName}",
                    GetState(passiveSkillStates, passive.Slot),
                    interactable,
                    value => OnPassiveSkillToggle(captured, value));
            }

            isRebuilding = false;
            RefreshToggleContentHeight(skillToggleRoot, skillToggles.Count);
        }

        private void OnActiveSkillToggle(SkillDefinition skill, bool value)
        {
            if (isRebuilding || skill == null)
            {
                return;
            }

            activeSkillStates[skill.Slot] = value;
            if (skill.Slot == SkillSlot.A && !value && passiveSkillStates.ContainsKey(SkillSlot.F))
            {
                passiveSkillStates[SkillSlot.F] = false;
            }

            RebuildSkillToggles();
            ApplySelectionWithoutRestart();

            if (value)
            {
                OpenEnhancementModal(skill.DisplayName, skill.EnhancementChoices, skill.MasterSkillChoices);
            }
        }

        private void OnPassiveSkillToggle(PassiveDefinition passive, bool value)
        {
            if (isRebuilding || passive == null)
            {
                return;
            }

            if (passive.Slot == SkillSlot.F && value && !GetState(activeSkillStates, SkillSlot.A))
            {
                passiveSkillStates[SkillSlot.F] = false;
                RebuildSkillToggles();
                SetStatus("F passive requires A skill to be checked.");
                return;
            }

            passiveSkillStates[passive.Slot] = value;
            ApplySelectionWithoutRestart();

            if (value)
            {
                OpenEnhancementModal(passive.DisplayName, passive.EnhancementChoices, null);
            }
        }

        private void OpenEnhancementModal(string title, SkillChoiceDefinition[] enhancementChoices, SkillChoiceDefinition[] masterChoices)
        {
            HideToggleSlots(modalChoiceToggles);
            enhancementModal.SetActive(true);
            modalTitleText.text = $"{title} Enhancements";

            var added = AddModalChoiceToggles(title, enhancementChoices);
            added += AddModalChoiceToggles($"{title} Master", masterChoices);
            modalSummaryText.text = added == 0
                ? "No enhancement choices exist for this skill."
                : "Select enhancement effects, then close this window.";
            RefreshToggleContentHeight(modalChoiceRoot, modalChoiceToggles.Count);
        }

        private int AddModalChoiceToggles(string sourceName, SkillChoiceDefinition[] choices)
        {
            if (choices == null)
            {
                return 0;
            }

            var added = 0;
            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId))
                {
                    continue;
                }

                var capturedChoiceId = choice.ChoiceId;
                var choiceSlot = FindNextChoiceToggleSlot();
                if (choiceSlot == null)
                {
                    Debug.LogError($"DebugSceneController has no enhancement choice slot for {capturedChoiceId}.");
                    break;
                }

                ConfigureToggle(
                    choiceSlot,
                    $"{sourceName}: {choice.Title}",
                    GetChoiceState(capturedChoiceId),
                    true,
                    value =>
                    {
                        choiceStates[capturedChoiceId] = value;
                        ApplySelectionWithoutRestart();
                    });
                added += 1;
            }

            return added;
        }

        private void CloseEnhancementModal()
        {
            enhancementModal.SetActive(false);
            ApplySelectionWithoutRestart();
        }

        private void ToggleSetupPanelVisibility()
        {
            if (setupPanel == null)
            {
                return;
            }

            SetSetupPanelVisible(!setupPanel.activeSelf);
        }

        private void StartCombat()
        {
            if (selectedMonster == null)
            {
                SetStatus("Select a monster first.");
                return;
            }

            if (combatController == null)
            {
                SetStatus("CombatRuntimeController is missing.");
                return;
            }

            combatController.BeginConfiguredDay(selectedMonster, BuildDebugSession(), gameDataCatalog);
            SetStatus($"{selectedMonster.DisplayName} spawn test started.");
        }

        private void ApplySelectionWithoutRestart()
        {
            if (selectedMonster == null || combatController == null)
            {
                return;
            }

            combatController.ApplyDebugSelection(selectedMonster, BuildDebugSession(), gameDataCatalog);
            RefreshStartButtonState();
        }

        private RunSession BuildDebugSession()
        {
            var session = RunSession.Begin(selectedMonster);
            session.LearnedActives.Clear();
            session.LearnedPassives.Clear();
            session.ChosenRewardIds.Clear();
            session.StageIndex = 1;
            session.DayIndex = 1;
            session.RefreshDayModel();

            AddSelectedActives(session);
            AddSelectedPassives(session);
            AddSelectedChoices(session);
            return session;
        }

        private void AddSelectedActives(RunSession session)
        {
            var activeSkills = GetActiveSkills(selectedMonster);
            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null || !GetState(activeSkillStates, skill.Slot) || string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    continue;
                }

                session.AddLearnedActive(skill.SkillId);
            }
        }

        private void AddSelectedPassives(RunSession session)
        {
            var passiveSkills = GetPassiveSkills(selectedMonster);
            for (var i = 0; i < passiveSkills.Length; i++)
            {
                var passive = passiveSkills[i];
                if (passive == null || !GetState(passiveSkillStates, passive.Slot))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(passive.PassiveId))
                {
                    session.AddLearnedPassive(passive.PassiveId);
                    session.ChosenRewardIds.Add(passive.PassiveId);
                }
            }
        }

        private static SkillDefinition[] GetActiveSkills(MonsterDefinition monster)
        {
            return monster == null
                ? Array.Empty<SkillDefinition>()
                : PakuriDataManager.Instance.GetActiveSkills(monster.MonsterId, monster);
        }

        private static PassiveDefinition[] GetPassiveSkills(MonsterDefinition monster)
        {
            return monster == null
                ? Array.Empty<PassiveDefinition>()
                : PakuriDataManager.Instance.GetPassiveSkills(monster.MonsterId, monster);
        }

        private void AddSelectedChoices(RunSession session)
        {
            foreach (var pair in choiceStates)
            {
                if (pair.Value && !string.IsNullOrWhiteSpace(pair.Key))
                {
                    session.ChosenRewardIds.Add(pair.Key);
                }
            }
        }

        private void RefreshCombatText()
        {
            if (combatText == null)
            {
                return;
            }

            if (selectedMonster == null)
            {
                combatText.text = "Monster: not selected\nSpawn state: stopped";
                return;
            }

            if (combatController == null || !combatController.HasActiveRun)
            {
                combatText.text = $"Monster: {selectedMonster.DisplayName}\nSpawn state: stopped\nPress Start to spawn enemies.";
                return;
            }

            var state = combatController.IsBattleResolved
                ? "cleared; waiting for Start"
                : "running";
            combatText.text =
                $"Monster: {combatController.SelectedMonsterName}\n" +
                $"Spawn state: {state}\n" +
                $"Nexus HP: {combatController.NexusCurrentHealth:0}/{combatController.NexusMaxHealth:0}\n" +
                $"Unit HP: {combatController.UnitCurrentHealth:0}/{combatController.UnitMaxHealth:0}\n" +
                $"Magazine: {combatController.CurrentShotsRemaining}/{combatController.MagazineCapacity}\n" +
                $"Reload: {combatController.ReloadRemaining:0.00}s\n" +
                $"Status: {combatController.StatusLabel}";
        }

        private void RefreshStartButtonState()
        {
            if (startButton != null)
            {
                startButton.interactable = selectedMonster != null;
            }

            if (skillWindowButton != null)
            {
                skillWindowButton.interactable = selectedMonster != null;
            }
        }

        private void RefreshSetupToggleButtonState()
        {
            if (setupToggleButton == null)
            {
                return;
            }

            SetButtonLabel(setupToggleButton, setupPanel != null && setupPanel.activeSelf ? "Hide Setup" : "Show Setup");
        }

        private void SetSetupPanelVisible(bool visible)
        {
            if (setupPanel == null)
            {
                return;
            }

            setupPanel.SetActive(visible);
            RefreshSetupToggleButtonState();
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        private GameObject FindDirectChild(string path)
        {
            return FindDirectChild(transform, path);
        }

        private static GameObject FindDirectChild(Transform parent, string path)
        {
            if (parent == null || string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var child = parent.Find(path);
            return child != null ? child.gameObject : null;
        }

        private static Text FindText(Transform parent, string path)
        {
            var target = FindDirectChild(parent, path);
            return target != null ? target.GetComponent<Text>() : null;
        }

        private static Button FindButton(Transform parent, string path)
        {
            var target = FindDirectChild(parent, path);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private Button EnsureSetupToggleButton()
        {
            var button = EnsureButton(transform, "SetupToggleButton", "Hide Setup", ToggleSetupPanelVisibility);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(180f, 46f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            return button;
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction onClick)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<Text>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        private void BindSkillToggleSlots()
        {
            skillToggles.Clear();
            if (skillToggleRoot == null)
            {
                Debug.LogError("DebugSceneController requires SkillDebugPanel/SkillScroll/Viewport/Content in DebugScene.");
                return;
            }

            AddSkillToggleSlot("Active_A");
            AddSkillToggleSlot("Active_B");
            AddSkillToggleSlot("Active_C");
            AddSkillToggleSlot("Active_D");
            AddSkillToggleSlot("Active_E");
            AddSkillToggleSlot("Passive_F");
            AddSkillToggleSlot("Passive_G");
            AddSkillToggleSlot("Passive_H");
            AddSkillToggleSlot("Passive_I");
            AddSkillToggleSlot("Passive_J");
            HideToggleSlots(skillToggles);
        }

        private void AddSkillToggleSlot(string path)
        {
            var toggleObject = FindDirectChild(skillToggleRoot.transform, path);
            if (toggleObject == null)
            {
                Debug.LogError($"DebugSceneController is missing skill toggle slot {path}.");
                return;
            }

            var toggle = toggleObject.GetComponent<Toggle>();
            if (toggle == null)
            {
                Debug.LogError($"DebugSceneController skill toggle slot {path} has no Toggle component.");
                return;
            }

            skillToggles.Add(toggle);
        }

        private Toggle FindSkillToggleSlot(bool passive, SkillSlot slot)
        {
            var slotName = passive ? $"Passive_{slot}" : $"Active_{slot}";
            for (var i = 0; i < skillToggles.Count; i++)
            {
                var toggle = skillToggles[i];
                if (toggle != null && string.Equals(toggle.gameObject.name, slotName, StringComparison.OrdinalIgnoreCase))
                {
                    return toggle;
                }
            }

            Debug.LogError($"DebugSceneController has no skill toggle slot named {slotName}.");
            return null;
        }

        private void BindChoiceToggleSlots()
        {
            modalChoiceToggles.Clear();
            if (modalChoiceRoot == null)
            {
                Debug.LogError("DebugSceneController requires EnhancementModal/ChoiceScroll/Viewport/Content in DebugScene.");
                return;
            }

            var toggles = modalChoiceRoot.GetComponentsInChildren<Toggle>(true);
            for (var i = 0; i < toggles.Length; i++)
            {
                modalChoiceToggles.Add(toggles[i]);
            }

            HideToggleSlots(modalChoiceToggles);
        }

        private Toggle FindNextChoiceToggleSlot()
        {
            for (var i = 0; i < modalChoiceToggles.Count; i++)
            {
                var toggle = modalChoiceToggles[i];
                if (toggle != null && !toggle.gameObject.activeSelf)
                {
                    return toggle;
                }
            }

            return null;
        }

        private static void HideToggleSlots(List<Toggle> toggles)
        {
            for (var i = 0; i < toggles.Count; i++)
            {
                var toggle = toggles[i];
                if (toggle == null)
                {
                    continue;
                }

                toggle.onValueChanged.RemoveAllListeners();
                toggle.SetIsOnWithoutNotify(false);
                toggle.gameObject.SetActive(false);
            }
        }

        private void ConfigureToggle(Toggle toggle, string label, bool isOn, bool interactable, UnityEngine.Events.UnityAction<bool> onChanged)
        {
            if (toggle == null)
            {
                return;
            }

            ConfigureToggleVisuals(toggle, label, interactable);
            toggle.interactable = interactable;
            toggle.SetIsOnWithoutNotify(isOn);
            if (toggle.graphic != null)
            {
                toggle.graphic.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
            }

            toggle.onValueChanged.RemoveAllListeners();
            if (onChanged != null)
            {
                toggle.onValueChanged.AddListener(onChanged);
            }

            toggle.gameObject.SetActive(true);
        }

        private static bool GetState(Dictionary<SkillSlot, bool> states, SkillSlot slot)
        {
            return states.TryGetValue(slot, out var value) && value;
        }

        private bool GetChoiceState(string choiceId)
        {
            return !string.IsNullOrWhiteSpace(choiceId) && choiceStates.TryGetValue(choiceId, out var value) && value;
        }

        private GameObject EnsurePanel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPosition)
        {
            var panel = EnsureChild(parent, name);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            var image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = panel.AddComponent<Image>();
            }

            image.color = color;
            return panel;
        }

        private Text EnsureText(Transform parent, string name, string content, int fontSize, TextAnchor anchor, float height)
        {
            var textObject = EnsureChild(parent, name);
            var rect = textObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, height);

            var text = textObject.GetComponent<Text>();
            if (text == null)
            {
                text = textObject.AddComponent<Text>();
            }

            text.font = uiFont;
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.text = content;
            return text;
        }

        private Button EnsureButton(Transform parent, string name, string label, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObject = EnsureChild(parent, name);
            var rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 46f);

            var image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = buttonObject.AddComponent<Image>();
            }

            image.color = new Color(0.18f, 0.24f, 0.32f, 0.96f);

            var button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = buttonObject.AddComponent<Button>();
            }

            button.onClick.RemoveAllListeners();
            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            var labelObject = EnsureChild(buttonObject.transform, "Label");
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(14f, 6f);
            labelRect.offsetMax = new Vector2(-14f, -6f);

            var text = labelObject.GetComponent<Text>();
            if (text == null)
            {
                text = labelObject.AddComponent<Text>();
            }

            text.font = uiFont;
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = Color.white;
            text.text = label;
            return button;
        }

        private Toggle EnsureToggle(Transform parent, string name, string label, bool isOn, bool interactable, UnityEngine.Events.UnityAction<bool> onChanged)
        {
            var toggleObject = EnsureChild(parent, name);
            var rect = toggleObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 34f);

            var layoutElement = toggleObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = toggleObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = 34f;
            layoutElement.preferredHeight = 34f;
            layoutElement.flexibleHeight = 0f;

            var toggle = toggleObject.GetComponent<Toggle>();
            if (toggle == null)
            {
                toggle = toggleObject.AddComponent<Toggle>();
            }

            var backgroundObject = EnsureChild(toggleObject.transform, "Background");
            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0f, 0.5f);
            backgroundRect.pivot = new Vector2(0f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(24f, 24f);

            var backgroundImage = backgroundObject.GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = backgroundObject.AddComponent<Image>();
            }

            backgroundImage.color = interactable ? new Color(0.18f, 0.22f, 0.28f, 1f) : new Color(0.10f, 0.10f, 0.10f, 0.65f);

            var checkmarkObject = EnsureChild(backgroundObject.transform, "Checkmark");
            var checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = new Vector2(5f, 5f);
            checkmarkRect.offsetMax = new Vector2(-5f, -5f);

            var checkmarkImage = checkmarkObject.GetComponent<Image>();
            if (checkmarkImage == null)
            {
                checkmarkImage = checkmarkObject.AddComponent<Image>();
            }

            checkmarkImage.color = new Color(0.65f, 0.92f, 1f, 1f);

            var labelObject = EnsureChild(toggleObject.transform, "Label");
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(34f, 0f);
            labelRect.offsetMax = Vector2.zero;

            var text = labelObject.GetComponent<Text>();
            if (text == null)
            {
                text = labelObject.AddComponent<Text>();
            }

            text.font = uiFont;
            text.fontSize = 15;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.color = interactable ? Color.white : new Color(0.70f, 0.70f, 0.70f, 1f);
            text.text = label;

            ConfigureToggleVisuals(toggle, label, interactable);
            toggle.interactable = interactable;
            toggle.SetIsOnWithoutNotify(isOn);
            if (toggle.graphic != null)
            {
                toggle.graphic.canvasRenderer.SetAlpha(isOn ? 1f : 0f);
            }

            toggle.onValueChanged.RemoveAllListeners();
            if (onChanged != null)
            {
                toggle.onValueChanged.AddListener(onChanged);
            }

            return toggle;
        }

        private void ConfigureToggleVisuals(Toggle toggle, string label, bool interactable)
        {
            var toggleObject = toggle.gameObject;
            var rect = toggleObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Max(rect.sizeDelta.y, 34f));

            var layoutElement = toggleObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = toggleObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = Mathf.Max(layoutElement.minHeight, 34f);
            layoutElement.preferredHeight = Mathf.Max(layoutElement.preferredHeight, 34f);
            layoutElement.flexibleHeight = 0f;

            var backgroundObject = EnsureChild(toggleObject.transform, "Background");
            backgroundObject.transform.SetAsFirstSibling();
            backgroundObject.SetActive(true);

            var backgroundRect = backgroundObject.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0f, 0.5f);
            backgroundRect.pivot = new Vector2(0f, 0.5f);
            backgroundRect.anchoredPosition = Vector2.zero;
            backgroundRect.sizeDelta = new Vector2(24f, 24f);
            backgroundRect.localScale = Vector3.one;

            var backgroundImage = backgroundObject.GetComponent<Image>();
            if (backgroundImage == null)
            {
                backgroundImage = backgroundObject.AddComponent<Image>();
            }

            backgroundImage.enabled = true;
            backgroundImage.sprite = GetSolidUiSprite();
            backgroundImage.type = Image.Type.Simple;
            backgroundImage.raycastTarget = true;
            backgroundImage.color = interactable ? new Color(0.18f, 0.22f, 0.28f, 1f) : new Color(0.10f, 0.10f, 0.10f, 0.65f);
            backgroundImage.canvasRenderer.SetAlpha(1f);

            var checkmarkObject = EnsureChild(backgroundObject.transform, "Checkmark");
            checkmarkObject.SetActive(true);

            var checkmarkRect = checkmarkObject.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            checkmarkRect.localScale = Vector3.one;

            var checkmarkImage = checkmarkObject.GetComponent<Image>();
            if (checkmarkImage == null)
            {
                checkmarkImage = checkmarkObject.AddComponent<Image>();
            }

            checkmarkImage.enabled = true;
            checkmarkImage.sprite = GetSolidUiSprite();
            checkmarkImage.type = Image.Type.Simple;
            checkmarkImage.raycastTarget = false;
            checkmarkImage.color = new Color(0.65f, 0.92f, 1f, 1f);

            var labelObject = EnsureChild(toggleObject.transform, "Label");
            labelObject.transform.SetAsLastSibling();
            labelObject.SetActive(true);

            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(34f, 0f);
            labelRect.offsetMax = Vector2.zero;
            labelRect.localScale = Vector3.one;

            var text = labelObject.GetComponent<Text>();
            if (text == null)
            {
                text = labelObject.AddComponent<Text>();
            }

            text.enabled = true;
            text.font = uiFont;
            text.fontSize = 15;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.color = interactable ? Color.white : new Color(0.70f, 0.70f, 0.70f, 1f);
            text.text = label;
            text.canvasRenderer.SetAlpha(1f);

            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;
        }

        private Sprite GetSolidUiSprite()
        {
            if (solidUiSprite != null)
            {
                return solidUiSprite;
            }

            if (generatedSolidUiTexture == null)
            {
                generatedSolidUiTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                generatedSolidUiTexture.name = "DebugSceneSolidUiTexture";
                generatedSolidUiTexture.SetPixel(0, 0, Color.white);
                generatedSolidUiTexture.Apply();
            }

            solidUiSprite = Sprite.Create(generatedSolidUiTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            solidUiSprite.name = "DebugSceneSolidUiSprite";
            return solidUiSprite;
        }

        private GameObject EnsureScrollContent(Transform parent, string name, float height)
        {
            var scrollObject = EnsureChild(parent, name);
            var scrollRectTransform = scrollObject.GetComponent<RectTransform>();
            scrollRectTransform.sizeDelta = new Vector2(0f, height);

            var layoutElement = scrollObject.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = scrollObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
            layoutElement.flexibleHeight = 0f;

            var scrollImage = scrollObject.GetComponent<Image>();
            if (scrollImage == null)
            {
                scrollImage = scrollObject.AddComponent<Image>();
            }

            scrollImage.color = new Color(0.03f, 0.04f, 0.06f, 0.55f);

            var scrollRect = scrollObject.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = scrollObject.AddComponent<ScrollRect>();
            }

            var viewport = EnsureChild(scrollObject.transform, "Viewport");
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(8f, 8f);
            viewportRect.offsetMax = new Vector2(-8f, -8f);

            var viewportImage = viewport.GetComponent<Image>();
            if (viewportImage == null)
            {
                viewportImage = viewport.AddComponent<Image>();
            }

            viewportImage.color = Color.clear;

            var mask = viewport.GetComponent<Mask>();
            if (mask == null)
            {
                mask = viewport.AddComponent<Mask>();
            }

            mask.showMaskGraphic = false;

            var content = EnsureChild(viewport.transform, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            EnsureVerticalLayout(contentRect, 0, 0, 6, true);
            return content;
        }

        private static void RefreshToggleContentHeight(GameObject contentRoot, int itemCount)
        {
            if (contentRoot == null)
            {
                return;
            }

            var rect = contentRoot.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            var itemHeight = itemCount <= 0 ? 34f : itemCount * 40f;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, itemHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            Canvas.ForceUpdateCanvases();
        }

        private GameObject EnsureChild(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var childObject = new GameObject(name, typeof(RectTransform));
            childObject.transform.SetParent(parent, false);
            return childObject;
        }

        private static void EnsureVerticalLayout(RectTransform rectTransform, int horizontalPadding, int verticalPadding, int spacing, bool fitHeight)
        {
            var layout = rectTransform.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = rectTransform.gameObject.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset(horizontalPadding, horizontalPadding, verticalPadding, verticalPadding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = rectTransform.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = rectTransform.gameObject.AddComponent<ContentSizeFitter>();
            }

            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = fitHeight ? ContentSizeFitter.FitMode.PreferredSize : ContentSizeFitter.FitMode.Unconstrained;
        }

        private static void ClearButtons(List<Button> buttons)
        {
            for (var i = buttons.Count - 1; i >= 0; i--)
            {
                if (buttons[i] != null)
                {
                    DestroyUiObject(buttons[i].gameObject);
                }
            }

            buttons.Clear();
        }

        private static void ClearToggles(List<Toggle> toggles)
        {
            for (var i = toggles.Count - 1; i >= 0; i--)
            {
                if (toggles[i] != null)
                {
                    DestroyUiObject(toggles[i].gameObject);
                }
            }

            toggles.Clear();
        }

        private static void DestroyUiObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
