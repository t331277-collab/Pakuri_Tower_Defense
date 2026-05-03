# GAMEDATA_ASSET_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: 2026-05-04 Rin/Sein CSV Runtime Sprite Catalog Fix

### Task title

Fill Rin/Sein monster sprite paths in CSV runtime source and sync the runtime asset catalog.

### Goals

- Keep runtime monster sprite resolution aligned with the active CSV source path.
- Avoid mistaking assigned legacy ScriptableObject sprite fields for the runtime source when the CSV catalog is active.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin/Sein visuals in Play Mode.
- Keep future monster visual changes in `Pakuri/Assets/CSVdata/source/monsters.csv` and sync the CSV runtime asset catalog.

### Evidence

- Unity AssetDatabase inspection showed the Rin/Sein `MonsterDefinition` SO sprite fields were already assigned, while `Pakuri/Assets/CSVdata/source/monsters.csv` had empty Rin/Sein sprite path cells.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now contains Rin sprite paths `Assets/Image/Monster/Rin/Rin_Temp (2).png` and `Assets/Image/Monster/Rin/Rin_Shoot.png`.
- `Pakuri/Assets/CSVdata/source/monsters.csv` now contains Sein sprite paths `Assets/Image/Monster/Sein/Sein_Temp.png` and `Assets/Image/Monster/Sein/Sein_Shoot.png`.
- Unity-MCP `execute_code` imported the CSV and ran `PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor()`, then resolved runtime Rin and Sein sprites as non-null.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains four generated entries for the Rin/Sein unit/projectile sprites.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Rin/Sein were the only Monsters whose `PrototypeCombatTuning` sprites were not applied.
- 2026-05-04: Builder confirmed the active runtime path was CSV-backed, filled the missing CSV path cells, and synced the runtime asset catalog.

## Task: 2026-05-02 GameData Catalog CSV Bootstrap Source

### Task title

Keep current ScriptableObject assets as an explicit bootstrap source only, not as a hidden runtime fallback.

### Goals

- Keep existing `Assets/Data/GameData` assets usable as an explicit bootstrap baseline.
- Export those assets into `Pakuri/Assets/CSVdata/source/*.csv` when the bootstrap menu is used.
- Prevent `GameDataCatalog.asset` from acting as the runtime source if CSV startup validation fails.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual asset files, actual scripts, and actual generated CSV output.
- Do not claim that the asset catalog is the sole runtime source anymore.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally revalidated after the Reviewer findings. A later builder pass also split the CSV runtime bootstrap/sync code out of the old `PakuriCsvRuntimeData.cs` monolith. The original Reviewer verdict remains FAIL until another review is explicitly requested.

### Next Actions

- If the team fully commits to CSV-first authoring later, reduce or remove duplicated tuning data between the legacy asset catalog and CSV export source.
- If stage/reward/shop/event assets are introduced later, add matching typed CSV tables before expanding runtime consumers.

### Evidence

- `PakuriCsvRuntimeData` now keeps `LegacyCatalogAssetPath = "Assets/Data/GameData/GameDataCatalog.asset"` only for the explicit editor bootstrap path.
- `BootstrapSourceFilesFromCurrentCatalog(...)` still loads `GameDataCatalog.asset` through `AssetDatabase.LoadAssetAtPath<GameDataCatalog>(...)`, but it now writes to `Pakuri/Assets/CSVdata/source`.
- The editor-only bootstrap and runtime-catalog sync path now lives in `Pakuri/Assets/Scripts/Data/PakuriCsvRuntimeData.Editor.cs` instead of sharing one file with runtime parse/validation/build logic.
- Runtime startup no longer reads `GameDataCatalog.asset` or `Pakuri/data/source`; it reads `PakuriCsvRuntimeSourceCatalog` and `PakuriCsvRuntimeAssetCatalog` from `Assets/Resources/Pakuri/CSVRuntime`.
- `ResolveCatalogOrFallback(...)` now returns `null` when CSV initialization failed, so serialized `GameDataCatalog` scene fields no longer become the runtime data source after a CSV failure.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` exists and references the 7 imported `Assets/CSVdata/source/*.csv` TextAssets.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` exists and stores the runtime-safe sprite dependency map extracted from current CSV rows.
- Unity console compile finished without C# errors after the builder follow-up, and `Pakuri/Validate CSV Source Data` previously logged `5 monsters` and `8 stage-one enemies` from the resource-backed CSV runtime source.
- After the split, Unity generated `.meta` files for the new partial files on full asset refresh and `Pakuri/Validate CSV Source Data` still logged the same 5-monster / 8-enemy runtime catalog summary.

### History

- 2026-05-02: Code Builder added editor bootstrap/export logic so the existing game-data assets can seed the new typed CSV source set.
- 2026-05-02: Initial migration still allowed the asset catalog to remain a hidden upstream source, and Code Reviewer marked that direction FAIL.
- 2026-05-02: Builder follow-up demoted `GameDataCatalog.asset` to an explicit bootstrap-only path, moved the active source set to `Assets/CSVdata/source`, and generated resource-backed runtime catalogs from the imported CSV.
- 2026-05-02: Builder later split the CSV runtime bootstrap/sync/editor code into `PakuriCsvRuntimeData.Editor.cs` while preserving the same asset-bootstrap contract and runtime validation behavior.

## Task: 2026-05-01 Game Data Asset Expansion Risk Review

### Task title

Review current `GameDataCatalog` / monster / enemy asset structure for future content additions.

### Goals

- Check whether current SO assets are sufficient for adding new gameplay content without code changes.
- Record concrete asset-model gaps found in `Pakuri/Assets/Data/GameData`.

### Constraints

- Role Owner is Designer.
- Base all findings on actual asset YAML and actual C# definitions.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If asset-driven expansion is requested later, introduce dedicated stage/run/reward/shop/prisoner config assets before scaling content quantity.

### Evidence

- `Pakuri/Assets/Data/GameData/GameDataCatalog.asset` contains only 2 gameplay groups: `Monsters` and `StageOneEnemies`.
- `Pakuri/Assets/Data/GameData/Monsters/*.asset` contain full A-J skill/passive payloads, but `SkillDefinition.RuntimeKind`, `SkillImplementationState`, `SkillEffectPrefab`, and `StatusEffectId` are not runtime-driven today.
- `eve.asset` shows only `eve-a` as `ImplementationState: 2`, while `ariel.asset` shows A-E/F-J as `ImplementationState: 2`; this means content-state metadata is not consistently synced with runtime capability.
- `rin.asset` and `sein.asset` still have `UnitSprite: {fileID: 0}` and `ProjectileSprite: {fileID: 0}`, so missing visual assignments currently fail soft instead of being validated at authoring time.
- There are no SO assets under `Pakuri/Assets/Data/GameData` for stage progression, reward tables, shop inventory, event pools, or prisoner behavior.

### History

- 2026-05-01: Reviewed `GameDataCatalog.asset`, sampled `eve.asset`, `rin.asset`, and `stage1-swordsman.asset`, and compared them against `MonsterDefinition.cs`, `SkillDefinition.cs`, and `EnemyDefinition.cs`.

## Task: Ariel Runtime Implementation State

### Task title

Mark Ariel A-E and F-J skill data as runtime implemented.

### Goals

- Keep Ariel `MonsterDefinition` data aligned with the newly implemented runtime code.
- Ensure future data seeding preserves Ariel runtime implementation states.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual asset and seeder code.
- Do not run Play Mode verification from Codex.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User can run Play Mode verification using DebugScene or RunScene.
- If Unity regenerates C# project files, confirm `CombatRuntimeArielSkills.cs` remains included after refresh.

### Evidence

- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` now has `ImplementationState: 2` for `ariel-a` through `ariel-e` and `ariel-f` through `ariel-j`.
- `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs` now uses `IsRuntimeImplementedActive(...)` and `IsRuntimeImplementedPassive(...)`.
- Seeder helper `IsRuntimeImplementedMonster(...)` returns true for `eve` and `ariel`, so future seeding keeps Eve/Ariel A-E and F-J runtime implemented.
- `Select-String` confirmed all Ariel A-E/F-J `ImplementationState` values are `2`.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-04-30: Code Builder updated Ariel asset state and seeder behavior during Ariel skill runtime implementation.

## Task: Monster A-J Skill Data Cleanup

### Task title

Prepare the 5 monster A-J skill data cleanup from reference documents.

### Goals

- Use `Pakuri/reference/Report/2026-04-28-reference-implementation-roadmap.html` step 5 as the implementation direction.
- Compare the 5 monster A-J skill documents under `Pakuri/reference/2.Monster` against current `Assets/Data/GameData/Monsters/*.asset`.
- Represent A as the default active skill, B-E as selectable actives, F as a selectable base passive, and G-J as passives unlocked by their matching active skills.
- Keep this pass focused on data/selection/unlock structure before full runtime effects.

### Constraints

- Role Owner is Designer until explicit Builder handoff.
- Ground all claims in actual files and command output.
- Current `SkillDefinition`/`PassiveDefinition` can store base skill/passive fields but has no structured fields for active enhancements, passive enhancements, or master skill branches.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation completed, and the user reported Play Mode verification completed. The required one-shot external Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; the user chose not to fix that reviewer finding for now. The finding is limited to trailing whitespace in `Pakuri/Assets/Data/GameData/Monsters/eve.asset`.

### Next Actions

- Continue to the next requested design or implementation task.
- If the user later wants the reviewer finding cleaned, remove the trailing whitespace in `eve.asset`, rerun `git diff --check`, rebuild, and update this block.

### Evidence

- Roadmap report step 5 says to organize monster A-J skill data first, completing selection/unlock structure before all complex effects.
- `Pakuri/reference/2.Monster` contains `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `monster-skill-patterns.md`, 5 monster tower documents, and 50 A-J skill documents.
- `SkillDefinition.cs` currently contains `SkillId`, `DisplayName`, `Slot`, `RuntimeKind`, `ImplementationState`, damage/range/cooldown/magazine fields, `StatusEffectId`, and `Summary`.
- `PassiveDefinition` currently contains `PassiveId`, `DisplayName`, `Slot`, `RequiredActiveSlot`, `ImplementationState`, and `Summary`.
- `MonsterDefinition.cs` currently stores `InitialRewardChoices`, `ActiveSkills`, and `PassiveSkills`, but no active-enhancement, passive-enhancement, or master-skill structured data.
- Current monster assets already contain A-E active entries and F-J passive entries; all A entries are `RuntimeImplemented`, B-E and F-J are `DataOnly`.
- `monster-basic-rule.md` states each monster starts with active A learned, starts with no passives learned, F is selectable without a specific active unlock, and G-J unlock after the matching B-E active is learned.
- `skill-choice-pool-rule.md` defines active enhancements, passive enhancements, and master skill candidates, but the current SO model has no dedicated structures for these candidates.
- `SkillDefinition.cs` now adds `SkillChoiceDefinition`, `SkillIcon`, `SkillEffectPrefab`, `DescriptionText`, active `EnhancementChoices`, active `MasterSkillChoices`, passive `EnhancementChoices`, `IsDefaultLearned`, and `IsAvailableWithoutActiveRequirement`.
- `PakuriGameDataSeeder.cs` now reads `Pakuri/reference/2.Monster/{monster}/skill/*.md` and populates A-E active and F-J passive data from those documents.
- `RunCombatUiController.cs` now adds structured active enhancements, passive enhancements, and master skill choices to the prisoner offering pool; it bypasses the active requirement only when `PassiveDefinition.IsAvailableWithoutActiveRequirement` is true.
- After running `Pakuri/Seed Default Game Data`, each monster asset has 5 `SkillId` entries, 5 `PassiveId` entries, 10 `EnhancementChoices` blocks, and 5 `MasterSkillChoices` blocks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing 2 Unity/MCP reference warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- External Code Reviewer returned `REVIEW_RESULT: NEEDS_CHANGES`; verified with `git diff --check -- Pakuri\Assets\Data\GameData\Monsters\eve.asset`, which reports trailing whitespace at lines 225, 238, 288, 301, 352, and 365.
- Added `Pakuri/reference/Report/2026-04-29-roadmap-implementation-result.html` comparing today's implementation result against `2026-04-28-reference-implementation-roadmap.html`.
- Added `Pakuri/reference/Report/2026-04-29-token-optimization-savings.html` estimating token savings from document parsing/token reduction based on measured file sizes.

### History

- 2026-04-29: User requested starting roadmap step 5, monster A-J skill data cleanup, and asked for questions if needed.
- 2026-04-29: User selected the data-structure expansion path, requested per-skill icon/effect/description fields, confirmed reference documents are the conflict source of truth, and confirmed F passive should be selectable from prisoner offering instead of default-granted.
- 2026-04-29: Code Builder expanded skill data structures, connected structured choices to prisoner offering, seeded monster A-J data from reference documents, and ran build/Unity validation.
- 2026-04-29: External Code Reviewer one-shot review returned `NEEDS_CHANGES` for trailing whitespace in `eve.asset`; Builder paused for user instruction per AGENTS.md.
- 2026-04-29: User reported Play Mode verification completed and chose not to fix the reviewer-raised whitespace issue for now.
- 2026-04-29: Designer added roadmap comparison and token optimization savings HTML reports under `Pakuri/reference/Report`.

## Task: Run Flow UICanvas Prototype Implementation

### Task title

Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.

### Goals

- Legacy non-English note retained these code references: `RunSession`, `RunFlowController`, `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `EveVerticalSliceController`.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `UICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

### Next Actions

- Legacy non-English note retained these code references: `RunUICanvas`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `B/G, C/H, D/I, E/J`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/Editor/PakuriGameDataSeeder.cs`, `Pakuri/Seed Default Game Data`, `Assets/Data/GameData/GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Run/RunSession.cs`, `RunFlowState.cs`, `RunFlowController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `RunUICanvas`, `EventSystem`.
- Legacy non-English note retained these code references: `Assets/Data/GameData/GameDataCatalog.asset`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `Canvas`, `CanvasScaler`, `GraphicRaycaster`, `RunFlowController`, `EventSystem`, `InputSystemUIInputModule`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `RunSession`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `stageIndex`.
- Legacy non-English note retained these code references: `RunFlowController.ClearButtons(...)`, `QueuedForDestroy`.
- Legacy non-English note retained these code references: `RunFlowController.ResolveReferences()`, `Arial.ttf`, `ArgumentException`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`, `Arial.ttf`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these code references: `MonsterDefinition`, `GameDataCatalog`, `PakuriGameDataSeeder`, `RunSession`, `RunFlowState`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `GameDataCatalog.asset`.
- Legacy non-English note retained these code references: `RunUICanvas`, `EventSystem`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Resources.GetBuiltinResource<Font>("Arial.ttf")`, `RunFlowController`, `LegacyRuntime.ttf`.
- Legacy non-English note retained these code references: `LegacyRuntime.ttf`.

## Task: Run Systems Integration Summary Report

### Task title

Legacy non-English note retained these code references: `monster-select-run-ui-builder-handoff`, `monster-select-run-ui-expansion-plan`, `save-and-load-plan`.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see surrounding retained task context.

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
- Legacy non-English note retained these code references: `RunSession`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-builder-handoff.html`, `RunSession`, `RunFlowController`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`, `RunSession`.
- Legacy non-English note retained these code references: `Pakuri/reference/save-and-load-plan.html`, `MetaSaveData`, `RunSnapshot`, `GameDataCatalog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `Scenes`, `Screenshots`, `Scripts`, `Settings`, `Resources`, `StreamingAssets`, `DataGenerated`.
- Legacy non-English note retained these code references: `.uxml`, `.uss`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/rin/rin-tower.md`, `rin/skill/g~j`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `ScriptableObject`, `CreateAssetMenu`, `GameDataCatalog`, `CsvGameDataImporter`, `Resources.Load`, `TextAsset`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`, `Pakuri/reference/4.run/combat-reward-system.md`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these ASCII code references: `Pakuri/reference/run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `run-systems-integration-summary-report.html`.
- Legacy non-English note retained these code references: `RunSession`, `run-systems-integration-summary-report.html`.

# Task: 2026-05-04 Rin A-E Runtime Implementation State

## Task title

Mark Rin active skills A-E as runtime implemented in the Rin monster data asset.

## Goals

- Keep Rin data asset implementation-state flags aligned with the newly added combat runtime.
- Do not edit reference planning documents.

## Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

## Role Owner

Code Builder

## Status

Implemented and locally validated.

## Next Actions

- User verifies Rin A-E runtime behavior in Play Mode.

## Evidence

- `Pakuri/Assets/Data/GameData/Monsters/rin.asset:88`, `:155`, `:222`, `:287`, and `:354` now show `ImplementationState: 2` for `rin-a` through `rin-e`.
- `Pakuri/reference/2.Monster/rin/skill/*.md` files were read as source references but were not edited.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP warnings.
- Unity-MCP script refresh reached idle and console error query returned only MCP-FOR-UNITY client handler logs.

## History

- 2026-05-04: Code Builder updated Rin A-E implementation-state flags after adding combat runtime support.
