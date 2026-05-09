# EVE_MONSTER

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Scope

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

Legacy non-English note retained these code references: `boards/MON/MON_BLACKBOARD.md`.

## Eve Runtime Summary

- Eve active skills A-E runtime work exists in the migrated task blocks below.
- Eve passive skills F-J runtime work exists in the migrated task blocks below.
- Arc Bolt has projectile, branch damage, magazine, reload, and enhancement/master behavior history.
- Eve status runtime includes shock, chill/freeze interactions, vulnerability, shield, action-speed, and passive damage modifiers.
- DebugScene testing for Eve skill toggles is tied to `boards/UI/DEBUGSCENE_UI.md`.

## Cross-Board Update Requirements

- Projectile changes: update this file and `boards/COMBAT/PROJECTILE_BLACKBOARD.md`.
- Status/shield/freeze/vulnerability changes: update this file and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- DebugScene Eve skill toggle changes: update this file, `boards/MON/MON_BLACKBOARD.md`, and `boards/UI/DEBUGSCENE_UI.md`.
- Eve data asset changes: update this file and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Reports about Eve implementation: update this file and `boards/REPORT/REPORT_BLACKBOARD.md`.

## Migrated Task Blocks

## Task: 2026-05-08 Manifested Eve A Auto-Target Runtime

### Task title

Move manifested Eve A onto Eve Arc Bolt-specific unit runtime execution.

### Goals

- Use the original Eve A projectile/enhancement logic for manifested Eve.
- Add only automatic target/direction selection for the manifested unit.
- Keep Eve A magazine, reload, projectile count, pierce, branch, status, and damage choices sourced from the manifested Eve `RunMonsterState`.

### Constraints

- Role Owner is Code Builder.
- Selected Eve manual fire is not Play Mode verified by Codex.
- User performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies manifested Eve A auto-fire, branching, pierce, magazine, reload, and Offering upgrades in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:192` routes Eve unit A runtime to `TryFireEveUnitArcBolt(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:650` computes target direction from the manifested unit position to the nearest enemy and applies Eve A choices.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:780` creates manifested Arc Bolt projectiles with lightning attribute, status, pierce, and branch fields.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:661` applies manifested projectile branch logic on hit.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User asked why manifested Eve A could not simply use original Eve A with auto aim and then requested that path to be implemented.

## Task: 2026-05-08 Eve Manifest Candidate Availability

### Task title

Allow selected Eve to be added as a manifested Eve party member.

### Goals

- Fix the case where Eve does not appear as a Manifest candidate when Eve is also the MainMenu-selected unit.
- Allow Manifest selection to add Eve to the manifested party list.

### Constraints

- Role Owner is Code Builder.
- This pass changes RunSession manifest duplicate logic, not Eve skill runtime behavior.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies Eve appears in the Manifest candidate panel and is added after selection.
- Follow-up may still be needed if selected Eve and manifested Eve must have independent Offering state while sharing the same `MonsterId`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` gets Manifest candidates from monster data and excludes ids only through `currentSession.HasManifestedMonster(monster.MonsterId)`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` now makes `HasManifestedMonster(...)` return true only when `monsterId` is in `ManifestedMonsterIds`.
- `Pakuri/Assets/Scripts/Run/RunSession.cs` keeps `RecordManifestedMonster(...)` adding `monster.MonsterId` to `ManifestedMonsterIds`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings; Unity refresh returned idle and console error query returned only MCP client handler exit logs.

### History

- 2026-05-08: User reported Eve did not show in Manifest candidates and selecting Eve did not add Eve.

## Task: 2026-05-08 Eve B-E Shared Unit Runtime

### Task title

Move Eve automatic support skills onto a shared caster-based unit runtime path.

### Goals

- Make selected EveUnit and manifested Eve use the same caster-based execution functions for Eve B-E.
- Read skill source data from `CombatSkillRuntime.Skill`.
- Read Offering choices from the caster's `RunMonsterState.ChosenRewardIds`.

### Constraints

- Role Owner is Code Builder.
- Eve A manual primary fire still needs a separate follow-up to move fully out of selected-primary globals.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies selected Eve and manifested Eve Prism Ray, Frost Field, Static Override, and Drone Beacon in Play Mode.
- Follow-up migrates Arc Bolt manual projectile runtime into the same caster path.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now has `TryTriggerEveUnitAutomaticSkills(...)`, `TryTickEveUnitSkill(...)`, `TryCastEveUnitPrismRay(...)`, `TryCastEveUnitFrostField(...)`, `TryCastEveUnitStaticOverride(...)`, and `TryCastEveUnitDroneBeacon(...)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` selected Eve automatic triggering now calls `TryTriggerEveUnitAutomaticSkills(selectedUnitRuntime)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` calls `TryTickEveUnitSkill(...)` for manifested Eve units before the older generic manifested path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` reads selected Eve cooldown display from selected Eve `CombatSkillRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested unit-owned skill behavior rather than copying the visual EveUnit object.

## Task: 2026-05-08 Manifested Eve Frost Field Parity

### Task title

Make manifested Eve C follow selected Eve Frost Field tick and status behavior.

### Goals

- Ensure manifested Eve Frost Field is not a one-shot area hit.
- Apply repeated ice damage, chill stacks, and freeze duration from Eve C traits while using the manifested Eve unit's Offering state.
- Keep manifested Eve damage resolution separate from selected-Eve-only passive checks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in RunScene Play Mode that manifested Eve C applies repeated damage and chill/freeze effects after Offering acquisition.
- Consider follow-up extraction of Eve A/B/D/E selected-skill code into unit-owned executors if exact manifested parity is required for all Eve skills.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Selected Eve C in `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` uses `CreateCircleEffect(...)`, `skillEffects.Add(effect)`, `TickSkillEffect(...)`, and applies `ApplyChill(...)` for `SkillId == "eve-c"`.
- Before this pass, `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` handled manifested `SkillRuntimeKind.Field` by applying `ApplyManifestedSkillDamage(...)` once in the radius and then creating only a visual.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now creates a `ManifestedFrostField` persistent effect with Eve C trait modifiers from `runtime.State.ChosenRewardIds`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now routes manifested persistent effects to `ApplyManifestedSkillEffectDamage(...)`, which applies ice damage plus `ApplyChill(...)` and freeze duration for `eve-c`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User provided the repro: selected Eve Frost Field applies ongoing freeze/chill damage, but manifested Eve Frost Field only deals the first hit.

## Task: 2026-05-05 Eve Skill Data RuntimeKind Audit

### Task title

Align Eve skill data with implemented runtime behavior for MonsterPanel and runtime selection.

### Goals

- Keep Eve active skill `RuntimeKind` values consistent with actual combat code and reference skill documents.
- Mark Eve A-E and F-J as runtime implemented in both ScriptableObject and CSV source data.
- Preserve Eve E Drone Beacon as the only non-A Eve active with magazine-style charges/reload display.

### Constraints

- Role Owner is Code Builder.
- Data-only correction; no Play Mode verification was run by Codex.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Eve Active1-3 MonsterPanel display in Play Mode after learning B-E.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/d-static-override.md` states Static Override is `범위 / 비탄창 / 감전 연계`.
- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` states Drone Beacon is `탄창 / 드론 / 표식 / 디버프` with magazine count 3 and reload 6 seconds.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` contains `TryCastEvePrismRay`, `TryCastEveFrostField`, `TryCastEveStaticOverride`, and `TryCastEveDroneBeacon`; Drone Beacon uses `eveDroneChargesRemaining` and `eveDroneReloadRemaining`.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` now stores Eve A `MagazineProjectile`, B `LineAttack`, C `Field`, D `AreaAttack`, E `MagazineProjectile`, and F-J `Passive`, all `ImplementationState: 2`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores the same Eve runtime kinds and `RuntimeImplemented` states.
- Unity-MCP read-only Editor import reported `eve-a:MagazineProjectile:RuntimeImplemented`, `eve-b:LineAttack:RuntimeImplemented`, `eve-c:Field:RuntimeImplemented`, `eve-d:AreaAttack:RuntimeImplemented`, `eve-e:MagazineProjectile:RuntimeImplemented`, and Eve F-J passives as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-05: After verifying the Rin MonsterPanel fix, user requested auditing Eve and Ariel skill data so they apply correctly too.
- 2026-05-05: Builder corrected Eve Static Override away from `MagazineProjectile`, kept Drone Beacon as a magazine-charge skill, and aligned implementation-state metadata.

## Task: Eve Arc Branch And DebugScene Skill Toggle Runtime

### Task title

Narrow Eve Arc Bolt extra projectile spread, implement immediate lightning branch damage, and add DebugScene skill-toggle testing UI.

### Goals

- Reduce the extra projectile spread angle for Eve A Arc Bolt.
- Change Eve A lightning branch semantics from status chance to immediate branch damage on hit.
- Draw a thin straight rectangular lightning line from the hit enemy to each branch target.
- Add a DebugScene-only controller under `Assets/Scenes/DebugScene.unity` that can test the 5 monster assets and toggle skills A-J plus enhancement/master effects.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Do not run external Code Reviewer unless the user explicitly grants permission.
- The current data model has `SkillSlot` A-J only; there is no K enum value. DebugScene shows K as a disabled no-data toggle rather than inventing runtime data.
- Preserve unrelated existing worktree changes from previous Eve runtime and report tasks.

### Role Owner

Code Builder

### Status

Builder correction pass completed for the prior `eve-a-master-1` findings, DebugScene UI flow was reworked to match the newer user request, and a follow-up SkillDebugPanel visibility fix was applied. A later Builder pass changed `DebugSceneController` toward scene-bound editable UI and saved static skill/choice toggle slots into `DebugScene.unity`. User then instructed Builder to restore `DebugSetupPanel` and setup controls; those scene paths were restored and build/console validation passed. The later root-scale finding was fixed by serializing the `DebugSceneController` root `RectTransform` scale as `{1,1,1}` and by guarding only the zero-scale case in `EnsureCanvasShell()`, while external Code Reviewer execution is deferred until user permission.

### Next Actions

- User Play Mode verifies `DebugScene` because Codex does not run Unity-MCP Play Mode gameplay verification.
- Run external Code Reviewer only after explicit user permission.

### Evidence

- `Pakuri/Assets/Scripts/Data/SkillDefinition.cs` defines `SkillSlot` A through J only; no K slot exists.
- `Pakuri/Assets/Data/GameData/Monsters/eve.asset` has `eve-a-trait-5` with branch chance text and `eve-a-master-1` with branch circuit text.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now uses `EveArcExtraProjectileAngleStep = 3f` instead of the previous 4-degree spread step.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now applies branch damage immediately after an Eve projectile hit, selects nearby targets within branch radius, and creates line visuals through `CreateEveArcBranchLine`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` was added to create a DebugScene-only uGUI panel with 5 monster buttons, skill toggles, enhancement/master toggles, and immediate `RunSession` restart into `CombatRuntimeController.BeginConfiguredDay(...)`.
- `Pakuri/Assets/Scenes/DebugScene.unity` now contains a `DebugSceneController` object wired to `Assets/Data/GameData/GameDataCatalog.asset` and `CombatRoot`.
- In `Pakuri/Assets/Scenes/DebugScene.unity`, the duplicated `RunSceneBootstrap` and `RunCombatUiController` components are serialized disabled to keep DebugScene separate from the RunScene flow.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `eve-a-master-1` branch damage is implemented as 100% in `CombatRuntimeEveSkills.cs`, but `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` says branch damage is 60%.
- Reviewer finding 2: `eve-a-master-1` does not add the documented magazine +2; `GetEveArcMagazineCapacity()` currently only applies `eve-a-trait-1` +4.
- User approved the Reviewer findings. `CombatRuntimeEveSkills.cs` now sets `eve-a-master-1` branch damage multiplier to `0.60f`, and `GetEveArcMagazineCapacity()` now adds `+2` for `eve-a-master-1`.
- User clarified DebugScene flow: starting DebugScene must not spawn enemies; user selects monster, opens skill debug, toggles A-J, chooses enhancement effects in a separate closable UI, then presses Start to spawn enemies.
- `CombatRuntimeController.cs` now exposes `ApplyDebugSelection(...)` so DebugScene can update selected monster/skill state without calling `BeginPrototypeDay(...)` or spawning enemies.
- `DebugSceneController.cs` now keeps monster selection and skill/enhancement configuration separate from `StartCombat()`, uses `BeginConfiguredDay(...)` only when Start is pressed, and uses `ApplyDebugSelection(...)` for pre-start or mid-combat debug changes.
- `DebugSceneController.cs` now disables passive F unless active A is checked; unchecking A also clears F.
- `DebugSceneController.cs` now opens an enhancement modal when a skill/passive is checked, and the modal has a close button.
- `DebugSceneController.cs` ignores prisoner reward UI entirely; after combat resolves, the existing `CombatRuntimeController` stays resolved until the DebugScene Start button is pressed again.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY transport/client handler logs, not project compile errors.
- User reported the SkillDebugPanel skill list was not visible.
- `DebugSceneController.OpenSkillWindow()` now activates `SkillDebugPanel` before rebuilding its toggles.
- `DebugSceneController.RebuildSkillToggles()` and `OpenEnhancementModal()` now call `RefreshToggleContentHeight(...)` after creating toggles so ScrollRect content has a concrete height.
- `DebugSceneController.EnsureToggle(...)` now assigns fixed toggle anchors/pivot plus `LayoutElement` min/preferred height, and `EnsureScrollContent(...)` assigns fixed scroll viewport `LayoutElement` height.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested after the SkillDebugPanel fix; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer command was attempted for the latest DebugScene changes, but Codex CLI exited with usage-limit errors and did not return a review verdict.
- User reported the SkillDebugPanel skill list still did not appear and requested editable scene UI instead of UI generated during game execution.
- `DebugSceneController.cs` was changed so `Awake()` calls `BindSceneUi()` instead of `BuildUi()`, and the controller now binds buttons/toggles from scene object paths instead of creating panels/toggles at runtime.
- Unity Edit Mode code saved static toggle objects in `Pakuri/Assets/Scenes/DebugScene.unity`: `Active_A` through `Active_E`, `Passive_F` through `Passive_J`, and `Choice_01` through `Choice_08`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs, not project compile errors.
- Required external Code Reviewer returned `REVIEW_RESULT: FAIL`.
- Reviewer finding 1: `DebugSceneController.cs` line 156 requires direct child `DebugSetupPanel`, but `DebugScene.unity` line 8213 shows only `SkillDebugPanel` and `EnhancementModal` under `DebugSceneController`; `Select-String` found no `m_Name: DebugSetupPanel`.
- Reviewer finding 2: `DebugSceneController.cs` line 166 expects `Title`, `Status`, `CombatText`, `MonsterButtons`, `SkillWindowButton`, and `StartButton` under `DebugSetupPanel`, but scene search found those setup paths absent.
- User instructed Code Builder to restore `DebugSetupPanel` and setup controls into the scene and re-run build validation.
- Unity Edit Mode code restored and saved scene objects for `DebugSetupPanel`, `DebugSetupPanel/Title`, `DebugSetupPanel/Status`, `DebugSetupPanel/MonsterButtons`, `DebugSetupPanel/SkillWindowButton`, `DebugSetupPanel/StartButton`, and `DebugSetupPanel/CombatText`.
- The same scene save pass also ensured `SkillDebugPanel/SkillScroll/Viewport/Content`, `EnhancementModal/ChoiceScroll/Viewport/Content`, A-J skill toggles, and `Choice_01` through `Choice_08` exist as editable scene objects.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- Required external Code Reviewer verified the requested scene paths now exist, but returned `REVIEW_RESULT: FAIL`.
- Latest Reviewer finding: `DebugScene.unity` line 10104 stores `DebugSceneController` root `RectTransform` with `m_LocalScale: {x: 0, y: 0, z: 0}`; since child UI is parented under this root, the UI can remain visually collapsed/non-interactive.
- User later instructed that Code Reviewer must be run only with user permission.
- `Pakuri/Assets/Scenes/DebugScene.unity` now stores the `DebugSceneController` root `RectTransform` with `m_LocalScale: {x: 1, y: 1, z: 1}`.
- `Pakuri/Assets/Scripts/Run/DebugSceneController.cs` now restores `transform.localScale` only when it is exactly `Vector3.zero`, preserving non-zero user-edited UI scale and position.
- `Select-String` confirmed `DebugSetupPanel`, `SkillDebugPanel`, `EnhancementModal`, `Active_A` through `Active_E`, `Passive_F` through `Passive_J`, and `Choice_01` / `Choice_08` are present in `Pakuri/Assets/Scenes/DebugScene.unity`.
- Unity read-only `execute_code` confirmed `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` has children `Active_A,Active_B,Active_C,Active_D,Active_E,Passive_F,Passive_G,Passive_H,Passive_I,Passive_J`.
- Unity `mcpforunity://scene/gameobject/65632` showed the loaded `DebugSceneController` transform scale and lossyScale are `{1,1,1}`.
- User reported that `Content` child skill toggles were clickable but their descriptions and checkmark were invisible.
- `DebugSceneController.ConfigureToggle(...)` now calls `ConfigureToggleVisuals(...)` to rebuild each scene-bound toggle slot's `Background`, `Checkmark/Glyph`, and `Label` visuals every time the slot is bound.
- `DebugSceneController.ConfigureToggleVisuals(...)` uses a separate `Checkmark/Glyph` child `Text` as the Toggle graphic, because the existing `Checkmark` object already has an `Image` graphic and Unity did not add a second `Text` graphic to the same GameObject in the runtime inspection.
- Legacy non-English note retained these ASCII code references: `execute_code`, `Active_A`, `toggle.graphic=Text:Checkmark/Glyph`, `labelAlpha=1`.
- Runtime Unity missing-script inspection returned `missingTotal=0`; the visible console still contained older `The referenced script (Unknown) on this Behaviour is missing!` entries with no file/line.
- User reported the Label skill text and checkbox were still not visible. Builder replaced the Text-glyph checkmark approach with Unity built-in `UISprite` and `Checkmark` sprites in `DebugSceneController.ConfigureToggleVisuals(...)`.
- Legacy non-English note retained these ASCII code references: `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content`, `Active_A`, `Passive_J`, `DebugScene.unity`, `labelAlpha=1`, `bgSprite=UISprite`, `checkSprite=Checkmark`, `toggleGraphic=Checkmark`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity refresh/compile completed with editor state `ready_for_tools=true`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the previous `DebugSceneController requires DebugSetupPanel...` project error.
- User reported `Failed to find UI/Skin/UISprite.psd` from `DebugSceneController.ConfigureToggleVisuals(...)`.
- `Select-String` confirmed the old `UI/Skin` and `GetBuiltinResource<Sprite>` calls were removed from `Pakuri/Assets/Scripts/Run/DebugSceneController.cs`; the only sprite load is now `Resources.Load<Sprite>("DebugUiSolid")`.
- `Pakuri/Assets/Resources/DebugUiSolid.png` was created as a project-owned 1x1 Sprite resource, avoiding Unity built-in UI skin paths.
- Unity Edit Mode scene save updated the actual `DebugSceneController/SkillDebugPanel/SkillScroll/Viewport/Content` slots so `Active_A` through `Passive_J` remain editable scene objects and their `Background` / `Background/Checkmark` images use `DebugUiSolid`.
- Legacy non-English note retained these ASCII code references: `execute_code`, `resourceSprite=DebugUiSolid`, `contentCount=10`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, `checkSprite=DebugUiSolid`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Unity refresh/compile completed with `resulting_state=idle`; Unity console error query showed only MCP-FOR-UNITY client handler logs and did not show the `Failed to find UI/Skin/UISprite.psd` project error.
- User requested the same visible/editable rebuild for `EnhancementModal` children.
- Unity read-only `execute_code` first confirmed `EnhancementModal/ChoiceScroll/Viewport/Content` had 8 choices but `Choice_01` had `bgSprite=null` and `checkSprite=null`.
- Unity Edit Mode code deleted all existing children under `DebugSceneController/EnhancementModal`, recreated `Title`, `Summary`, `CloseButton`, `ChoiceScroll/Viewport/Content`, and `Choice_01` through `Choice_08`, and saved `Assets/Scenes/DebugScene.unity`.
- Unity read-only `execute_code` then confirmed `modalActive=False`, `title=Enhancements`, `closeButton=True`, `count=8`, `choice01Label=Choice Slot 01`, `labelAlpha=1`, `bgSprite=DebugUiSolid`, and `checkSprite=DebugUiSolid`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings after the scene rebuild.
- Unity refresh completed with `resulting_state=idle`; Unity console error query showed only MCP-FOR-UNITY client handler logs.

### History

- 2026-04-29: User requested narrower extra projectile angle, immediate lightning branch damage/visuals, and a separate DebugScene for testing monster skills and enhancements through toggles.
- 2026-04-29: Code Builder inspected actual combat scripts, scene files, monster assets, and the SkillSlot enum before editing.
- 2026-04-29: Code Builder implemented branch projectile fields, immediate branch damage, line visuals, DebugScene controller script, and DebugScene scene wiring.
- 2026-04-29: External Code Reviewer found two `eve-a-master-1` spec mismatches and returned `NEEDS_CHANGES`; Builder paused per AGENTS.md.
- 2026-04-30: User approved the prior Reviewer findings and then clarified the desired DebugScene UI flow.
- 2026-04-30: Code Builder fixed `eve-a-master-1` branch damage and magazine size, added `CombatRuntimeController.ApplyDebugSelection(...)`, and rewrote `DebugSceneController` so enemies spawn only from the DebugScene Start button.
- 2026-04-30: User reported the SkillDebugPanel skill list was not visible; Code Builder updated the panel activation order and ScrollRect/LayoutElement sizing, then rebuilt and checked Unity console.
- 2026-04-30: Code Builder attempted the required external Code Reviewer pass, but Codex CLI reported a usage limit and no verdict was produced.
- 2026-04-30: User reported the SkillDebugPanel issue persisted and requested editable scene UI rather than game-run generated UI. Code Builder changed the controller toward scene-bound UI and saved static toggle slots, but the required external Code Reviewer found missing `DebugSetupPanel` setup controls and returned `FAIL`; Builder paused per AGENTS.md.
- 2026-04-30: User instructed Builder to restore `DebugSetupPanel` and setup controls. Builder restored those scene objects and validated build/console, then external Reviewer found the root scale `{0,0,0}` scene issue and returned `FAIL`; Builder paused per AGENTS.md.
- 2026-04-30: User instructed Builder to fix the actual Content skill-list visibility and preserve user-edited UI Scale/Position, and also instructed that Code Reviewer execution now requires user permission. Builder fixed the serialized root scale, kept only a zero-scale runtime guard, rebuilt, refreshed Unity, checked the console, and did not run Code Reviewer.
- 2026-04-30: User reported the `Content` child skill descriptions and checkmarks were still invisible while clicks worked. Builder changed toggle visual binding to normalize scene-bound Label/Background/Checkmark/Glyph elements, applied the same normalization to the current runtime instance, rebuilt, checked Unity console/missing-script state, and did not run Code Reviewer.
- 2026-04-30: User reported the Label skill text and checkbox were still invisible. Builder switched checkboxes to built-in Unity UI sprites, saved the actual scene slots in Edit Mode, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer.
- 2026-04-30: User reported `Failed to find UI/Skin/UISprite.psd` and asked to rebuild `SkillDebugPanel` as visible editable scene UI. Builder removed built-in UI skin sprite usage, created project-owned `Assets/Resources/DebugUiSolid.png`, saved the scene toggle visuals against that sprite, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer because user permission was not granted.
- 2026-04-30: User requested the same rebuild for `EnhancementModal` children. Builder deleted and recreated the modal children as editable scene uGUI objects using `DebugUiSolid`, verified the first choice label/checkmark state, rebuilt, refreshed Unity, checked console, and did not run Code Reviewer because user permission was not granted.

## Task: Eve Passive Runtime Implementation

### Task title

Implement Eve passive runtime effects for the Eve skill documents under `Pakuri/reference/2.Monster/eve`.

### Goals

- Implement Eve passive effects from the existing Eve passive documents `f-voltage-calibration.md`, `g-weakness-analysis.md`, `h-particle-separation.md`, `i-cooling-algorithm.md`, and `j-overcurrent-circuit.md`.
- Connect selected passive and passive-trait reward ids to runtime combat behavior.
- Add a white shield HP bar overlay to the selected monster HP bar while keeping the full HP bar length unchanged.
- Apply behavior speed, cooldown, duration, firing interval, and damage-area adjustments according to `Pakuri/reference/3.combat/combat-stat-system.md`.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run one Code Reviewer after Builder logic changes.
- The user mentioned `k`, but the actual Eve skill folder contains `f` through `j` and no `k` file; this pass treated the existing `h-particle-separation.md` / slot H document as the missing fifth passive.
- Preserve unrelated existing worktree changes, including the prior next-work HTML report and the user-deferred `eve.asset` trailing whitespace finding.

### Role Owner

Code Builder

### Status

Builder implementation and reviewer correction pass completed. Local build/Unity console validation completed, and the follow-up external Code Reviewer returned `REVIEW_RESULT: PASS`.

### Next Actions

- User can Play Mode verify Eve passive effects, including Voltage Calibration shield/reload acceleration, Particle Separation Prism Ray proc, Cooling Algorithm freeze interactions, Overcurrent Circuit lightning bonuses, and Weakness Analysis vulnerable-target bonuses.
- Continue to the next requested design or implementation task.

### Evidence

- Actual Eve passive files present under `Pakuri/reference/2.Monster/eve/skill`: `f-voltage-calibration.md`, `g-weakness-analysis.md`, `h-particle-separation.md`, `i-cooling-algorithm.md`, and `j-overcurrent-circuit.md`; no `k` file exists.
- `combat-stat-system.md` says action speed accelerates projectile firing interval and active skill cooldown charging, while duration and firing interval are separate stats.
- `CombatRuntimeController.cs` now has learned passive state and selected monster shield runtime fields.
- `CombatRuntimeScene.cs` now creates and updates a white selected monster shield bar overlay on `MonsterHpBar`.
- `CombatRuntimeProjectiles.cs` now applies Eve passive damage/defense/status chance modifiers and selected monster shield absorption.
- `CombatRuntimeEnemies.cs` now applies selected monster shield absorption to direct enemy attacks and triggers Eve H trait 3 freeze-release damage.
- `CombatRuntimeEveSkills.cs` now implements Eve F/G/H/I/J passive checks, shield, action speed helper, passive damage multipliers, resistance reductions, status chance bonus, and particle-separation Prism Ray proc.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Initial parallel Editor build failed with a file lock on `obj\Debug\Assembly-CSharp.dll`; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` sequentially completed with 0 errors and the same warnings.
- Unity script refresh/compile was requested; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `CombatRuntimeProjectiles.cs` line 250 decrements Arc Bolt reload with raw `Time.deltaTime`, so `eve-f-trait-3` action speed does not affect reload while shielded.
- Reviewer finding 2: current uncommitted changes include the prior unrelated `Next Roadmap Work Plan Report` block in `BLACKBOARD.md` and untracked `Pakuri/reference/Report/2026-04-29-next-work-plan.html`, which are outside the Eve passive runtime implementation scope unless explicitly justified or separated.
- Reviewer finding 1 was corrected by applying `GetEveActionSpeedMultiplier()` to the Arc Bolt reload countdown in `CombatRuntimeProjectiles.UpdateSelectedMonsterCombat()`.
- Reviewer finding 2 is explicitly justified here: `Pakuri/reference/Report/2026-04-29-next-work-plan.html` and the `Next Roadmap Work Plan Report` BLACKBOARD block were created in the immediately preceding user-requested Designer task, are preserved as completed task evidence, and are not part of the Eve passive runtime implementation logic.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing Unity/MCP assembly conflict warnings.
- Follow-up parallel Editor build hit a transient write lock on `obj\Debug\Assembly-CSharp.dll`; rerunning `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` sequentially completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; follow-up refresh returned `resulting_state=idle`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- Follow-up external Code Reviewer confirmed prior finding 1 fixed, accepted the explicit separation/justification for prior finding 2, and returned `REVIEW_RESULT: PASS`.

### History

- 2026-04-29: User requested implementation of Eve passive effects for active skills A-E, shield HP bar overlay, and timing/range handling based on `combat-stat-system.md`.
- 2026-04-29: Code Builder confirmed actual Eve passive documents are F-J and no K document exists; implementation treated H as the missing fifth passive.
- 2026-04-29: Code Builder implemented the runtime pass and completed local build/Unity console validation.
- 2026-04-29: External Code Reviewer returned `NEEDS_CHANGES`; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to fix the reviewer findings; Builder applied the Arc Bolt reload action-speed correction and documented the prior next-work report as a separate completed user-requested task.
- 2026-04-29: Code Builder rebuilt, rechecked Unity console, and follow-up external Code Reviewer returned `PASS`.

## Task: Eve Active Skill Status Runtime

### Task title

Implement Eve active skill A-E runtime status effects before roadmap step 6.

### Goals

- Make Eve learned active skills A-E cast on player click with automatic nearest-enemy targeting.
- Keep skills from auto-casting without a click.
- Implement Eve-related combat statuses first: shock, chill/freeze blue tint, slow, vulnerability, and shield bar visuals.
- Apply selected Eve active trait choices to actual runtime behavior.
- Use Eve's implementation shape as the later framework for other monsters.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run one Code Reviewer after Builder logic changes.
- Preserve the existing user-deferred reviewer finding in `Pakuri/Assets/Data/GameData/Monsters/eve.asset` without fixing it unless requested.

### Role Owner

Code Builder

### Status

Builder implemented the user-approved correction pass for Eve A manual firing, B-E click-triggered automatic targeting, infinite skill target range, the prior reviewer findings, the mojibake status message fix, and RunScene manual transform preservation for EveUnit status visuals. Build, Unity console validation, and the required one-shot external Code Reviewer pass completed with `REVIEW_RESULT: PASS`.

### Next Actions

- User can Play Mode verify Eve A/B-E behavior and RunScene manual transform preservation.
- Continue to the next requested design or implementation task.

### Evidence

- User clarified that learned active skills should be cast by player click, auto-targeting the nearest enemy in range, but should not auto-cast by themselves.
- User clarified selected trait enhancement effects should actually apply.
- User accepted targeting recommendation for Eve D: target the nearest shocked enemy in range, and do not cast if none exists.
- User clarified chill and freeze can both use the same blue-tint visual for now and should be documented later in HTML.
- `CombatRuntimeEveSkills.cs` was added to implement Eve A-E click-cast behavior, beam/field/drone runtime objects, status application helpers, and trait checks by `eve-*-trait-*` reward ids.
- `CombatRuntimeProjectiles.cs` now supports player projectile pierce, per-projectile hit tracking, Eve drone vulnerability application, and delegates Eve click casting before legacy click-to-point firing.
- `CombatRuntimeEnemies.cs` now tracks shock/chill/freeze/slow/vulnerability timers/stacks, applies blue tint for shock/chill/freeze, and updates a white shield bar overlay.
- Enemy and selected monster HP bars are now red, while the shield bar is white.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Reviewer finding 1: `eve-a-trait-5` applies power +25% but not the documented lightning/status chance +35%; reviewer cited `CombatRuntimeEveSkills.cs` around line 172, `CombatRuntimeProjectiles.cs` around lines 58-60, and `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md` line 52.
- Reviewer finding 2: `FreezeTimer` is declared/consumed but no code path sets it; reviewer cited `CombatRuntimeController.cs` around line 62, `CombatRuntimeEnemies.cs` around lines 643 and 671, `CombatRuntimeEveSkills.cs` around line 360, and `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` line 44.
- User clarified the correction: Eve A must be manual firing toward the clicked direction, not automatic casting or automatic targeting; that same click is the trigger for the other Eve skills.
- User clarified B-E should conditionally auto-cast and auto-target once the click trigger fires.
- User clarified skill range should be infinite; if the trigger works, the skill should execute on the nearest enemy or the skill-specific priority target.
- `CombatRuntimeProjectiles.UpdateSelectedMonsterCombat()` now calls `TryTriggerEveAutomaticSkills()` on click without consuming the primary A firing path.
- `CombatRuntimeProjectiles.FirePrimarySkill()` now routes Eve A to `FireManualEveArcBolt(direction)` after deriving the clicked direction from `currentAttackPoint`.
- `CombatRuntimeEveSkills.TryTriggerEveAutomaticSkills()` now triggers only B-E, not A.
- `CombatRuntimeEveSkills.FireManualEveArcBolt()` now applies Eve A trait projectile count, pierce, damage, fire interval, reload, and trait 5 status chance modifiers while preserving clicked-direction firing.
- `ProjectileRuntime.StatusChance` and projectile hit handling now allow Eve A trait 5 to add +35% status chance without changing the global configured chance for other projectiles.
- Eve B, C, D, and drone E targeting now use `float.PositiveInfinity` range; D still keeps its shocked-target predicate as the skill-specific priority.
- `SkillEffectRuntime.FreezeDuration` is now set by `eve-c-trait-5`, and Frost Field ticks apply `enemy.FreezeTimer` when that trait is selected.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review for the correction pass returned `REVIEW_RESULT: NEEDS_CHANGES`.
- Latest reviewer finding: `CombatRuntimeEveSkills.cs` contains mojibake user-facing `statusLabel` messages at and around lines 87, 106, 171, 288, 353, 425, and 489. Reviewer verified the core logic requirements as satisfied but flagged the visible broken text.
- `CombatRuntimeEveSkills.cs` statusLabel messages at lines 87, 106, 171, 288, 354, 425, and 489 were changed to readable ASCII English text to resolve the mojibake finding.
- `CombatRuntimeScene.EnsureStatusLabel()` now preserves existing `MonsterHpLabel` local position and scale, assigning defaults only when the label object is newly created.
- `CombatRuntimeEnemies.CreateHpBar()` now preserves existing `MonsterHpBar` root position and scale and preserves existing Background/Fill transforms, assigning defaults only when those objects are newly created.
- `CombatRuntimeEnemies.CreateShieldBarFill()` now preserves an existing Shield transform and only assigns default shield transform values when newly created.
- `CombatRuntimeScene.EnsureSpriteRenderer()` no longer overwrites existing anchors with SpriteRenderers; in the current `RunScene`, `EveUnit` already has a SpriteRenderer, so its scene-authored scale is preserved.
- `CombatRuntimeScene.EnsureBattlefieldBackgroundVisual()` no longer forces `BattlefieldBackground` position; scale is still only changed when `autoFitBattlefieldBackgroundToField` is true. `RunScene.unity` currently has `autoFitBattlefieldBackgroundToField: 0`.
- `Pakuri/Assets/Scenes/RunScene.unity` contains actual scene-authored `EveUnit`, `MonsterHpLabel`, `MonsterHpBar`, and `BattlefieldBackground` objects.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh/compile was requested; editor state returned `ready_for_tools=true`, and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- External Code Reviewer one-shot review for the latest changes returned `REVIEW_RESULT: PASS`.
- Added `Pakuri/reference/Report/2026-04-29-eve-active-skill-runtime-implementation.html` documenting the Eve A-E runtime implementation, the user clarification process that reduced implementation ambiguity, status/effect wiring, manual transform preservation, and verification results.

### History

- 2026-04-29: User requested Eve Monster active skill A-E status/effect runtime before roadmap step 6 and provided detailed semantics for pierce, extra projectiles, beams, area instant skills, drones, blue status tint, red HP bar, and white shield bar.
- 2026-04-29: Designer asked five implementation interpretation questions; user clarified click-cast auto-targeting, actual trait application, D shocked-target behavior, and blue tint for both ice states.
- 2026-04-29: Code Builder implemented Eve A-E runtime behavior and completed local build/Unity console validation.
- 2026-04-29: External Code Reviewer found two missing trait/status behavior issues; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to prioritize restoring A as manual clicked-direction firing, make B-E click-triggered automatic infinite-range skills, and fix the two reviewer findings.
- 2026-04-29: Code Builder implemented the correction pass and completed local build/Unity console validation; required external Reviewer pass remains pending.
- 2026-04-29: External Code Reviewer verified the correction logic but returned `NEEDS_CHANGES` for mojibake status messages in `CombatRuntimeEveSkills.cs`; Builder paused per AGENTS.md.
- 2026-04-29: User instructed Builder to fix the reviewer finding and preserve manually edited RunScene `EveUnit` child HP Label/HPBar position and scale, plus other scene-authored transforms where applicable.
- 2026-04-29: Code Builder fixed Eve status messages, preserved existing status visual transforms and scene-authored anchor transforms, completed build/Unity validation, and external Code Reviewer returned `PASS`.
- 2026-04-29: Code Builder added an HTML implementation report for the Eve active skill runtime work under `Pakuri/reference/Report`.

## Task: Eve Initial Combat Preview

### Task title

Legacy non-English note retained these code references: `dungeon-squad-run-structure.md`.

### Goals

- Legacy non-English note retained these code references: `reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/4.run/dungeon-squad-run-structure.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/eve-tower.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/Scene/combat-scene-layout.md`, `(2,8)`, `(4~10, 3~15)`.
- Legacy non-English note retained these code references: `Pakuri/reference/dungeon-squad-combat-player-controls.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/5.enemy/stage-1-enemies.md`.
- Legacy non-English note retained these code references: `manage_scene get_active`, `Assets/Scenes/SampleScene.unity`, `manage_scene get_hierarchy`, `Main Camera`, `Global Light 2D`.
- Legacy non-English note retained these code references: `manage_asset search`, `Assets`, `Scenes`, `Settings`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-run-structure.md`, `eve-tower.md`, `current-architecture-plan.html`.
- Legacy non-English note retained these code references: `a-arc-bolt.md`, `combat-scene-layout.md`, `combat-reward-system.md`, `dungeon-squad-combat-player-controls.md`, `combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `stage-1-enemies.md`.
- Legacy non-English note retained these code references: `Pakuri/reference`.

## Task: Eve Combat Vertical Slice Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `eve-initial-combat-vertical-slice-preview.html`.
- Legacy non-English note retained these code references: `CombatRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed with manual reviewer pass in-session. External Codex reviewer commands timed out and did not produce a new review artifact.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `codex review`, `codex exec`.

### Evidence

- Legacy non-English note retained these code references: `Assets/Scripts/Combat/DamageCalculator.cs`.
- Legacy non-English note retained these code references: `Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `manage_asset search path=Assets/Scripts`, `Combat`, `DamageCalculator.cs`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `SampleScene.unity`, `CombatRoot`, `Pakuri.Combat.EveVerticalSliceController`.
- Legacy non-English note retained these code references: `manage_scene get_hierarchy include_transform=true`.
  - Legacy non-English note retained these code references: `Main Camera`, `15.5, 8.5, -10`.
  - Legacy non-English note retained these code references: `Nexus`, `2, 8, 0`.
  - Legacy non-English note retained these code references: `EveUnit`, `6, 8, 0`.
  - Legacy non-English note retained these code references: `EnemySpawnPoint`, `29, 8, 0`.
  - Legacy non-English note retained these code references: `InputTarget`, `16, 8, 0`.
- Legacy non-English note retained these code references: `SampleScene.unity`, `orthographic: 1`, `orthographic size: 10`, `CombatRoot`, `EveVerticalSliceController`.
- Legacy non-English note retained these code references: `execute_code`.
  - Legacy non-English note retained these code references: `Enemy_Normal_01`, `Enemy_Boss_01`.
  - Legacy non-English note retained these code references: `battleResolved=True`, `victory=True`, `waitingForRewardChoice=True`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - `Assets/Screenshots/screenshot-20260424-165841.png`
  - `Assets/Screenshots/screenshot-20260424-165958.png`
- Legacy non-English note retained these code references: `validate_script`, `DamageCalculator.cs`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `codex review --uncommitted`.
- Legacy non-English note retained these code references: `codex exec`.
- Legacy non-English note retained these code references: `DamageCalculator.cs`, `EveVerticalSliceController.cs`, `SampleScene.unity`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `eve-initial-combat-vertical-slice-preview.html`.
- Legacy non-English note retained these code references: `Assets/Scripts`, `Assets/Scripts/Combat`.
- Legacy non-English note retained these code references: `DamageCalculator.cs`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `CombatRoot`, `EveVerticalSliceController`.
- Legacy non-English note retained these code references: `Main Camera`.
- Legacy non-English note retained these code references: `ExecuteAlways`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `codex review --uncommitted`, `codex exec`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

## Task: Eve Projectile Click Hold Compliance Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/dungeon-squad-combat-player-controls.md`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/eve/skill/a-arc-bolt.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`, `wasPressedThisFrame`, `GetMouseButtonDown(0)`.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-hold-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `dungeon-squad-combat-player-controls.md`, `a-arc-bolt.md`, `combat-attribute-and-damage-system.md`, `EveVerticalSliceController.cs`, `eve-combat-implementation-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-hold-plan.html`.

## Task: Eve Projectile Click Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Completed without Code Review. External reviewer commands timed out again, so only Builder-side validation was performed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`, `ProjectileRuntime`, `projectileRoot`, `UpdateProjectiles()`, `TryHitEnemy()`, `HandlePointerInput()`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scenes/SampleScene.unity`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `manage_scene save`, `Assets/Scenes/SampleScene.unity`.
- Legacy non-English note retained these code references: `find_gameobjects by_name ProjectileRoot`, `ProjectileRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - Legacy non-English note retained these code references: `projectileCount = 1`.
  - Legacy non-English note retained these code references: `projectileCount = 0`.
  - Legacy non-English note retained these code references: `enemyHealth = 37.95`.
  - Legacy non-English note retained these code references: `currentShotsRemaining = 0`, `reloadRemaining = 4.0`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Screenshots/eve-projectile-click-runtime.png`.
- Legacy non-English note retained these code references: `validate_script`.
- Legacy non-English note retained these code references: `read_console`, `FindObjectOfType<Camera>()`, `FindFirstObjectByType<Camera>()`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
  - `codex review --uncommitted` timeout
  - Legacy non-English note retained these code references: `codex exec`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`, `eve-projectile-click-hold-plan.html`, `a-arc-bolt.md`, `dungeon-squad-combat-player-controls.md`, `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `ProjectileRoot`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `FireArcBolt()`.
- Legacy non-English note retained these code references: `FindFirstObjectByType<Camera>()`.
- Legacy non-English note retained these code references: `Pakuri/reference/eve-projectile-click-implementation-report.html`.
- Legacy non-English note retained these code references: `codex review --uncommitted`, `codex exec`.

# Task: 2026-05-08 Manifested Eve Arc Bolt Correction

### Task title

Prevent Manifested Eve A from using Prism Ray prefab/line behavior.

### Goals

- Keep Eve A as `MagazineProjectile` and default learned for Manifested Eve.
- Remove the CSV `eve-a` reference to the Eve B Prism Ray prefab.
- Route Manifested Eve A through projectile sprite and magazine/reload state.

### Constraints

- Role Owner is Code Builder.
- Eve-specific CSV data and combat behavior must remain evidence-based.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve fires Arc Bolt-style projectiles and does not show the Prism Ray prefab.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:13` now leaves `eve-a` `skill_effect_prefab_path` empty.
- `monster_skills.csv:14` still keeps `eve-b` pointing at `Assets/Image/Monster/Eve/Effect_Prefab/Eve_Skill_B.prefab`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:465` creates Manifested A projectile visuals from `runtime.Monster.ProjectileSprite`, not `SkillEffectPrefab`.
- `CombatRuntimeParty.cs:418` through `:463` applies magazine/reload state to Manifested Eve A because `eve-a` is `MagazineProjectile` with `MagazineCapacity=6`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User reported Manifested Eve played the B Prism Ray effect and attacked abnormally instead of firing Arc Bolt.
- 2026-05-08: Code Builder removed the incorrect `eve-a` CSV effect-prefab reference and changed Manifested projectile handling.

# Task: 2026-05-08 Manifested Eve Sustained Skills Follow-up

### Task title

Keep Manifested Eve Prism Ray, Frost Field, and Drone Beacon visible for their Eve runtime durations.

### Goals

- Use Eve's existing selected-monster duration constants for Manifested Eve sustained visuals.
- Make Manifested Eve Drone Beacon deploy a timed drone that fires projectiles.

### Constraints

- Role Owner is Code Builder.
- This pass changes the Manifested party runtime path, not the selected 1P Eve runtime path.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Eve B, C, and E in RunScene Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` defines `EveBeamDuration = 1.2f`, `EveFrostFieldDuration = 4f`, `EveDroneDuration = 5f`, and `EveDroneAttackPeriod = 0.8f`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now maps `eve-b`, `eve-c`, and `eve-e` to those durations in `ResolveManifestedSkillVisualDuration(...)`.
- `CombatRuntimeParty.cs` now routes Manifested `eve-e` through `DeployManifestedEveDroneBeacon(...)` before the generic projectile branch.
- Runtime and Editor builds completed with 0 errors.

### History

- 2026-05-08: User specifically named Eve Drone Beacon, Frost Field, and Prism Ray as sustained skills whose Manifested duration appeared too short.
