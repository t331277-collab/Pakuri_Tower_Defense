## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/GAMEDATA_ASSET_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad data/asset history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and other archive files under `boards/ARCHIVE/`.
- This active file now keeps only the current runtime prefab/catalog wiring still useful for day-to-day work.

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
