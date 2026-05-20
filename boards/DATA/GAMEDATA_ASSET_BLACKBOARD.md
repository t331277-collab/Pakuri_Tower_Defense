## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/GAMEDATA_ASSET_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad data/asset history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and other archive files under `boards/ARCHIVE/`.
- This active file now keeps only the current runtime prefab/catalog wiring still useful for day-to-day work.

## Task: 2026-05-20 Sein-B EffectManager Scene Wiring

### Task title

Wire Sein-B to the requested shared Sein projectile prefab.

### Goals

- Keep active monster skill visuals scene-owned through `EffectManager`.
- Reuse `Assets/Prefab/Skill/Sein/Sein_A.prefab` for `sein-b` as requested.
- Avoid adding a parallel CSV prefab-path route for base monster skill visuals.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- User-authored prefab content is preserved.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and file-verified.

### Next Actions

- User verifies in Play Mode that Sein-B projectiles use the requested `Sein_A` visual.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab` exists and its `.meta` GUID is `256552cb82ec9c2499fc2e0e01d20dd2`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now serializes `sein-b` under the `sein` `EffectManager` group with prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` resolves monster skill visuals through `ResolveMonsterSkillEffectPrefab(monsterId, skillId)`.

### History

- 2026-05-20: User requested `Sein-b` to use `Assets/Prefab/Skill/Sein/Sein_A.prefab`; Code Builder added the scene mapping.

## Task: 2026-05-19 Sein-A EffectManager Scene Wiring

### Task title

Restore the missing `NewRunScene` `EffectManager` prefab mapping for `sein-a`.

### Goals

- Keep active monster projectile visuals wired through scene-owned `EffectManager` entries.
- Restore the `sein-a` projectile prefab link without adding a parallel prefab-resolution route.

### Constraints

- Role Owner is Code Builder.
- The fix must stay grounded in inspected prefab files and actual scene serialization.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented in scene serialization and file-verified.

### Next Actions

- If future Sein active skills gain retained visuals, add them to the same `EffectManager` group instead of moving prefab-path authority back into CSV.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab` exists and `Pakuri/Assets/Prefab/Skill/Sein/Sein_A.prefab.meta` stores GUID `256552cb82ec9c2499fc2e0e01d20dd2`.
- Before this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10469` serialized `MonsterId: sein` with `SkillEffects: []`.
- After this task, `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:10468-10471` serializes `SkillId: sein-a` and the `Sein_A.prefab` GUID under the `sein` `EffectManager` group.

### History

- 2026-05-19: User reported that the in-game Sein check showed no assigned Sein prefab effect in `EffectManager`.
- 2026-05-19: Code Builder restored the `sein-a` scene mapping to `Assets/Prefab/Skill/Sein/Sein_A.prefab`.

## Task: 2026-05-17 Active Runtime Skill Asset Wiring

### Task title

Keep the current skill prefab and runtime catalog wiring explicit for the active Scripts2 path.

### Goals

- Preserve the current runtime actor/prefab wiring for active skill prefabs already used by the kept new scene flow.
- Preserve the CSV runtime asset catalog as the asset-resolution bridge for active skill prefab paths.
- Keep choice-snapshot and Offering data alignment visible from the asset board point of view.

### Constraints

- Role Owner is Code Builder.
- User-authored prefab art/layout remains preserved as authored.
- Unity Play Mode verification remains user-owned.
- Detailed older asset wiring slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active asset-wiring baseline summarized and retained for future work. 2026-05-18 Code Builder removed monster unit/projectile sprite path authority from `monsters.csv`. 2026-05-18 CSV runtime sync can now be invoked by batchmode through a public editor method.

### Next Actions

- If more monster skills become active in runtime, wire them through the same prefab-actor plus catalog path instead of creating parallel asset routes.
- Update this file together with `boards/DATA/DATA_BLACKBOARD.md` when prefab-path authority changes again.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Ariel/Airel_A.prefab` is wired as a runtime projectile actor and serialized into `NewRunScene`.
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_A.prefab` exists and `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now serializes it as the `EffectManager` visual mapping for monster `rin` skill `rin-a`.
- `Pakuri/Assets/Prefab/Skill/Eve/Eve_A.prefab` and `Pakuri/Assets/Prefab/Skill/Ariel/Ariel_B.prefab` remain the retained baseline examples for shared projectile/attached-effect actor usage.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` remains the runtime asset catalog bridge for active prefab-path resolution.
- `PakuriCsvRuntimeData.Build.cs` was recorded as the source that builds active skills, passive skills, choice rows, and reward rows from the active CSV source files.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` remains the retained scene evidence for current prefab serialization and runtime references.
- `Pakuri/Assets/CSVdata/source/monsters.csv` no longer carries `unit_sprite_path`, `projectile_sprite_path`, `unit_color`, or `projectile_color`; `PakuriCsvRuntimeData.Editor.cs` no longer adds monster sprite paths to the CSV runtime asset catalog.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` no longer validates monster unit/projectile sprite asset coverage from `monsters.csv`; enemy sprite validation remains unchanged.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` exposes `SyncAndValidateCsvRuntimeCatalogsForEditor()` for batchmode and Unity-MCP execution.
- `SyncCsvRuntimeCatalogs.bat` invokes that editor method and writes Unity batch logs to `PakuriCsvRuntimeSync.log`.
- Unity-MCP execution of `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` logged successful catalog load and validation; `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` was touched by the sync.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `System.Net.Http` and `System.IO.Compression` MSB3277 warnings remain.

### History

- 2026-05-15: Shared runtime skill prefab actor wiring became the retained baseline.
- 2026-05-17: Ariel-A prefab wiring and Eve A-J runtime catalog source alignment were added to that active baseline.
- 2026-05-18: Monster visual sprite/color source columns were removed from `monsters.csv`; current skill visual authority remains `EffectManager` plus scene/prefab wiring.
- 2026-05-18: CSV runtime catalog sync/validation was exposed as a public editor method and wrapped by `SyncCsvRuntimeCatalogs.bat`.
- 2026-05-19: Rin-A prefab wiring was added to the active `EffectManager` scene mapping using `Assets/Prefab/Skill/Rin/Rin_A.prefab`.

## Task: 2026-05-19 CSV Source Asset Import Recovery

### Task title

Harden runtime source catalog sync against not-yet-imported CSV assets.

### Goals

- Keep `PakuriCsvRuntimeSourceCatalog.asset` sync resilient when a source CSV exists on disk but Unity has not yet produced the `TextAsset`.
- Preserve the active `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime` sync path.

### Constraints

- Role Owner is Code Builder.
- Asset conclusions must stay grounded in inspected editor sync code, actual source files, and Unity console evidence.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-verified.

### Next Actions

- Reuse this recovery path for future externally-edited CSV assets instead of adding manual pre-import steps.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Editor.cs` now refreshes and imports a source CSV asset synchronously when `AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath)` initially returns `null`.
- `Pakuri/Assets/CSVdata/source/monster_modifier_skill_choice.csv` and its `.meta` existed on disk while the user-facing auto-sync stack trace still reported the imported `TextAsset` as missing, confirming the failure lived in asset import state rather than in the file path string.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` after the fix logged a successful sync to `Assets/Resources/Pakuri/CSVRuntime` and did not reproduce the previous missing-TextAsset fatal exception.

### History

- 2026-05-19: Code Builder added a synchronous refresh/import retry path so runtime source catalog sync can recover from externally-created or freshly-renamed CSV assets that are present on disk but not yet imported into Unity's AssetDatabase.

## Task: 2026-05-18 Eve-B EffectManager Wiring Evidence

### Task title

Record Eve-B LineAttack visual asset availability and scene mapping.

### Goals

- Keep Eve-B visual authority grounded in the existing prefab and `EffectManager` scene mapping.
- Avoid reintroducing skill-effect prefab path authority into `monster_skills.csv`.

### Constraints

- Role Owner is Code Builder.
- User-authored prefab art/layout is preserved.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Confirmed through Unity-MCP during Eve-B LineAttack implementation.

### Next Actions

- If future monster LineAttack skills need visuals, wire their prefabs through `EffectManager` in the same style.

### Evidence

- Unity-MCP asset info confirmed `Assets/Prefab/Skill/Eve/Eve_B.prefab` exists as a `UnityEngine.GameObject` with GUID `224f5e7622cd0264b961ee388a015d65`.
- Unity-MCP `GameManager` component inspection confirmed `EffectManager` maps monster `eve` skill `eve-b` to `Assets/Prefab/Skill/Eve/Eve_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` resolves LineAttack visuals through `EffectManager.ResolveMonsterSkillEffectPrefab(...)`.

### History

- 2026-05-18: Eve-B LineAttack implementation confirmed the current `EffectManager` prefab route instead of adding prefab paths back to monster skill CSV rows.
