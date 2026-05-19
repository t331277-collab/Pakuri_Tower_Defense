# RUNSCENE_UI

This is the active `NewRunScene` UI persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/RUNSCENE_UI_ARCHIVE_2026-05-18.md`.
- Older RunScene/Manifested UI history remains in that snapshot and earlier archive files.
- This active file now keeps only the current `NewRunScene` UI behavior still relevant to active work.

## Task: 2026-05-19 Offering Choice1-3 Data Binding Refresh

### Task title

Bind Offering `Choice1`-`Choice3` UI from the unified monster choice CSV path.

### Goals

- Show title, description, and icon from the unified `monster_skill_choices.csv` choice rows.
- Keep Offering availability driven by the slim `monster_modifier_skill_choice.csv` gate rows plus learned-skill state.
- Remove the old one-line button label fallback for enhancement Offering rows.

### Constraints

- Role Owner is Code Builder.
- UI conclusions must stay tied to inspected scene hierarchy and inspected `InGameUIManager.cs`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution was deferred because explicit user permission was not given in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified. The active Offering panel now binds icon/title/description from exact choice rows instead of the removed reward text columns.

### Next Actions

- User verifies in Play Mode that `OfferingPanel/Choice1`-`Choice3` show the intended icon, title, and description for active, passive, and enhancement offerings.
- If later UI localization is added, keep these bindings data-driven through the unified choice rows.

### Evidence

- Scene hierarchy inspection confirmed `Canvas/OfferingPanel/Choice1`, `Choice2`, and `Choice3` each contain child objects named `Icon`, `Text (TMP)`, and `Desc`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves those child bindings once through `ResolveButtonViews(...)` and writes icon/title/description through `BindChoiceButton(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now resolves enhancement offerings through `ResolveChoice(reward.RewardId)` instead of the removed reward-row `linked_choice_id` / `title` / `description` fields.
- Active and passive Offering rows now use the monster plus learned skill/passive display name and their skill icons; enhancement rows use the exact choice row title/description plus `ResolveChoiceIcon(...)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now enforces learned-choice availability by exact `choice_id`, with active enhancements capped at three per skill, active masters unlocked after three active enhancements, and passive enhancements capped at one per passive skill.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing MSB3277 warnings remained.

### History

- 2026-05-19: Code Builder rewired the Offering panel so `Choice1`-`Choice3` bind `Icon`, `Text (TMP)`, and `Desc` from unified choice rows and no longer depend on the removed reward-row title/description/modifier columns.

## Task: 2026-05-18 NewRun Prefix Removal UI Reference Update

### Task title

Update UI scripts after removing `NewRun` from runtime manager script names.

### Goals

- Keep UI references compiling after `NewRunSceneEntryManager` became `SceneEntryManager`.
- Keep UI references compiling after `NewRunStageManager` became `StageManager` and `NewRunStageState` became `StageState`.
- Preserve existing UI behavior.

### Constraints

- Role Owner is Code Builder.
- This is a behavior-preserving naming refactor.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode only if they want UI behavior confirmation after the rename.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs`, `InGameUIManager.cs`, and `MonsterPanelUI.cs` now reference `StageManager` and/or `SceneEntryManager`; the Menifest flow now lives inside `InGameUIManager.cs`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now checks `StageState.RewardReady`.
- Search found no remaining `NewRunSceneEntryManager`, `NewRunStageManager`, `NewRunStartContext`, or `NewRunStageState` references in scripts, scene assets, prefab assets, asset files, or `Assembly-CSharp.csproj`.
- Runtime/editor builds passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-18: Code Builder updated UI script references as part of removing `NewRun` from current runtime script filenames and type names.

## Task: 2026-05-17 NewRunScene Active UI Rules

### Task title

Keep the current `NewRunScene` UI behavior compact and explicit.

### Goals

- Preserve active status suffix display on unit name labels.
- Preserve the current `AutoBtn` route that switches 1P A between manual and automatic execution.
- Preserve the current Offering enhancement availability filter based on learned active/passive state.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older UI task history remains in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active `NewRunScene` UI rules summarized and retained for future work. 2026-05-18 Code Builder refactor centralizes shared unit actor display logic and now keeps Offering/Menifest behavior inside `InGameUIManager.cs` through integrated helper types.

### Next Actions

- User verifies in Play Mode that label suffixes, AutoBtn behavior, and Offering gating still match the retained baseline.
- Future UI work should update this file only when those active rules change.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` now delegate shared name/status/HP/shield/damage-popup presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAutoSkillButton.cs` plus `InGameCombatManager.cs` own the current AutoBtn behavior.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` keeps `Canvas/AutoBtn` wired to `Pakuri.InGame.InGameAutoSkillButton`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` owns top-level `NewRunScene` UI lookup/binding and now contains the Offering/Menifest flow helper types directly in the same file.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` still owns the learned-skill Offering enhancement filter, Offering choice commit path, Menifest popup state, candidate commit, and skip behavior through those integrated helper types.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.

### History

- 2026-05-15: AutoBtn manual/auto routing became part of the active baseline.
- 2026-05-17: Status suffix display and Offering enhancement availability filtering were added to that active baseline.
- 2026-05-18: Code Builder split `InGameUIManager` into Offering and Menifest helper flows, and commonized `MonsterUnitActor`/`EnemyUnitActor` presentation through `UnitActorView.cs`.
- 2026-05-18: Code Builder later re-merged the Offering and Menifest helper files into `InGameUIManager.cs` during the repository-wide high-integration consolidation pass.

## Task: 2026-05-18 NewRunScene DebugUI and MonsterPanel Skill UI

### Task title

Add `DebugUI` skill-learn buttons and `MonsterPanel` selected-monster skill status UI to `NewRunScene`.

### Goals

- `Canvas/DebugUIBtn` opens `Canvas/DebugUI`, and `Canvas/DebugUI/Close` closes it.
- `DebugUI` A-E buttons learn the selected 1P monster's A-E active skills when the skill exists and is not already learned.
- Missing or unavailable skills return without side effects.
- `MonsterPanel/1PMonster` shows the selected monster image and up to three learned active skill slots.
- Magazine skills show current magazine count in each Active slot text.
- Cooldown/reload waits are visualized through each slot's `CooldownOverlay` image using a vertical filled overlay.

### Constraints

- Role Owner is Code Builder.
- User explicitly requested no Code Reviewer stage for this task.
- Unity Play Mode verification remains user-owned.
- Debug skill acquisition must use the same session/offering record path as Offering active-skill acquisition.

### Role Owner

Code Builder

### Status

Implemented and scene-wired. `DebugUIBtn` was renamed from the actual scene object `DebugBtn` after inspection showed the user-requested `DebugUIBtn` name did not exist yet. 2026-05-18 follow-up fixed `MonsterPanelUI` so it runs from always-active `Canvas`, forces `MonsterPanel/1PMonster` visible, binds serialized `Active1`-`Active3` slot view objects to the real child GameObjects, and uses remaining cooldown ratio for `CooldownOverlay.fillAmount`. A later 2026-05-18 follow-up changed Active slot Text so only magazine skills show `current/max`; non-magazine learned skills keep their Text object inactive and empty.

### Next Actions

- User verifies in Play Mode that `DebugUIBtn`, `Close`, A-E learn buttons, magazine counts, and vertical cooldown overlay timing match expected UX.
- If more than three learned active skills must be visible at once, expand the current `MonsterPanel` slot count beyond `Active1`-`Active3`.

### Evidence

- Unity scene hierarchy inspection showed `Canvas/DebugUI` with `Close`, `ABtn`, `BBtn`, `CBtn`, `DBtn`, `EBtn`, and showed `Canvas/DebugBtn` instead of `DebugUIBtn`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` records debug skill acquisition through `RunSession.RecordOfferingChoice(monsterId, string.Empty, string.Empty, sourceSkill.SkillId, string.Empty)`.
- `Pakuri/Assets/Scripts2/InGame/UI/DebugUI.cs` synchronizes the selected player model `UnitStateBucket` from `RunSession` and rebuilds active runtime skills with `SkillRuntimeFactory.RebuildLearnedActiveSet`.
- `Pakuri/Assets/Scripts2/InGame/UI/MonsterPanelUI.cs` resolves `MonsterPanel/1PMonster/Monster Image`, `Active1`, `Active2`, `Active3`, their `Text (TMP)` children, and their `CooldownOverlay` images.
- 2026-05-18 follow-up evidence: Unity-MCP `find_gameobjects` found `Pakuri.InGame.MonsterPanelUI` on `Canvas` only, and `Canvas/MonsterPanel` exists as the controlled panel.
- 2026-05-18 follow-up evidence: Unity-MCP editor code simulation with Eve default learned state returned `runtimeSkills=1; panel=True; oneP=True; active1=True; active1Text=6/6; active2=False; active3=False; overlayFill=0.00; overlayActive=False`.
- 2026-05-18 follow-up evidence: Unity-MCP editor code simulation after learning `eve-b` and `eve-e` returned `runtimeSkills=3; Active1=True:6/6; Active2=True:프리즘 레이; Active3=True:플라즈마 필드`.
- 2026-05-18 follow-up evidence: after the Active Text policy change, Unity-MCP editor code simulation after learning `eve-b` and `eve-e` returned `runtimeSkills=3; A1=True:textActive=True:text='6/6'; A2=True:textActive=False:text=''; A3=True:textActive=False:text=''`.
- 2026-05-18 prisoner label diagnosis: `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` line 85 contains the hardcoded label `?щ줈\n{prisoners[i]}`, while CSV search found `stage1-swordsman` in source CSV as ASCII enemy id data. The observed `?щ줈\nstage1-swordsman` therefore comes from code-side label mojibake, not from the prisoner id CSV value.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now hides `PrisonerChoicePopUp` immediately after `OfferingBtn` or `Menifested` is clicked.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now performs the same `RunSession` -> `MonsterUnitRuntimeModel.State` sync before rebuilding learned active skills through its integrated Offering flow helper.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` has `Pakuri.InGame.DebugUI` and `Pakuri.InGame.MonsterPanelUI` on `Canvas`, with `Canvas/MonsterPanel` as the controlled panel, and the scene was saved through Unity MCP.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 assembly-version warnings remain.
- Unity console after compile still showed only the pre-existing MCP client handler logs and `UnityEditor.Graphs.Edge.WakeUp` `NullReferenceException`; no new C# compile errors were reported.

### History

- 2026-05-18: Code Builder inspected `NewRunScene` UI hierarchy, implemented `DebugUI.cs` and `MonsterPanelUI.cs`, patched Offering runtime-state sync, wired components into `NewRunScene`, saved the scene, and verified runtime/editor builds with 0 errors.
- 2026-05-18 follow-up: User clarified that only learned runtime skills should fill up to `Active1`-`Active3`. Code Builder found the serialized `ActiveSkillSlotView[]` entries were non-null but unbound to child GameObjects, so `ResolveSlot` skipped binding and left authored placeholder text/visibility unchanged. `MonsterPanelUI.cs` now rebinds unbound slot views and the scene now drives `MonsterPanelUI` from `Canvas`.
- 2026-05-18 follow-up: User clarified that Active slot Text should not show skill names and should only show magazine count for magazine skills. Code Builder changed `MonsterPanelUI.cs` accordingly and added `PrisonerChoicePopUp` hiding wrappers for Offering/Menifested clicks in `InGameUIManager.cs`.

## Task: 2026-05-18 NewRunScene Reward/Offering Mojibake Cleanup

### Task title

Remove broken hardcoded reward and Offering labels from the active `NewRunScene` UI path.

### Goals

- Stop prisoner reward buttons from showing mojibake plus raw enemy IDs such as `stage1-swordsman`.
- Resolve prisoner display names through the runtime CSV catalog built from `stage_one_enemies.csv`.
- Remove broken hardcoded Korean fragments from Offering choice titles and fallback descriptions.
- Keep Offering titles/descriptions driven by monster, skill, passive, and reward data fields that originate from the current CSV runtime catalog.

### Constraints

- Role Owner is Code Builder.
- CSV data itself was not changed because `stage1-swordsman` already exists as a valid ASCII enemy ID and `stage_one_enemies.csv` already provides `display_name`.
- No authoritative CSV for static UI category labels such as Reward, Prisoner, Gold, or Dark Trace was found during this task, so those static labels remain code-side English placeholders.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that reward prisoner buttons show the CSV enemy display name, for example `Prisoner` / `검사` for `stage1-swordsman`.
- If Korean/static UI labels should be data-driven too, add or identify a dedicated UI localization CSV for labels such as Reward, Prisoner, Gold, Dark Trace, Active, Passive, and Enhancement.

### Evidence

- `Pakuri/Assets/CSVdata/source/stage_one_enemies.csv` contains `stage1-swordsman,검사`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps source enemy `DisplayName` into `EnemyDefinition.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/GameDataCatalog.cs` exposes `GetStageOneEnemyById(string enemyId)`.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now calls `ResolvePrisonerDisplayName(prisonerId)` and uses `GameDataCatalog.GetStageOneEnemyById(...)` before falling back to the raw ID.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` now builds active/passive Offering titles from CSV-backed `monster.DisplayName`, `skill.DisplayName`, and `passive.DisplayName`, and enhancement Offering titles/descriptions from the exact `monster_skill_choices.csv` row resolved by `reward.RewardId`.
- `Get-ChildItem -Path Pakuri\Assets\Scripts2\InGame\UI -Recurse -Filter *.cs | Select-String -SimpleMatch ...` found 0 remaining matches for the inspected mojibake fragments after the change.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP console read after compile showed only the existing MCP client handler log and existing `UnityEditor.Graphs.Edge.WakeUp` `NullReferenceException`; no new C# compile errors were reported.

### History

- 2026-05-18: User clarified the `stage1-swordsman` problem was not CSV corruption, but broken hardcoded UI strings in the active reward and Offering code path. Code Builder inspected the CSV/runtime catalog path, replaced the broken code-side strings, and verified builds.
