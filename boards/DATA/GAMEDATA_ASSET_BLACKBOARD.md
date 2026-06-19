## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/GAMEDATA_ASSET_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad data/asset history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and other archive files under `boards/ARCHIVE/`.
- This active file now keeps only the current runtime prefab/catalog wiring still useful for day-to-day work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-06-19 CSV Runtime Source Asset Path Reorganization

### Task title

Keep runtime CSV source asset references valid after moving CSV files into purpose-specific folders.

### Goals

- Preserve runtime source catalog and asset catalog sync after CSV file moves.
- Preserve Unity object references by moving CSV `.meta` files with their CSV files.
- Keep editor auto-sync watching the new runtime CSV folder tree.

### Constraints

- Role Owner is Code Builder.
- The runtime source catalog asset remains under `Assets/Resources/Pakuri/CSVRuntime`.
- Stage-flow scene references are preserved by GUID and remain user Play Mode verified.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP sync/validate checked.

### Next Actions

- Keep prefab-path authority in CSV rows unchanged; only the CSV table asset locations changed.
- When adding a new runtime CSV file, add its purpose folder mapping in `PakuriCsvRuntimeData.GetImportedSourceAssetPath(...)`.

### Evidence

- `PakuriCsvRuntimeData.Editor.cs` now syncs source TextAssets from `Assets/CSVdata/runtime/...` instead of `Assets/CSVdata/source`.
- `PakuriCsvRuntimeCatalogPostprocessor.cs` now detects changed `.csv` files under `Assets/CSVdata/runtime`.
- `PakuriSkillEffectPrefabCsvExporter.cs` now targets `Assets/CSVdata/runtime/monster/skills/monster_skill_choices.csv`.
- Unity-MCP sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/runtime' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity-MCP warning/error console read after sync/validate returned 0 entries.

### History

- 2026-06-19: Code Builder reorganized active CSV source assets and updated runtime/editor catalog paths without changing prefab-path strings inside the CSV rows.

## Task: 2026-05-31 Stage2 Enemy Prefab Binding

### Task title

Wire Stage 2 enemy prefabs into the active `NewRunScene` enemy spawn manager.

### Goals

- Connect every Stage 2 enemy id to its prefab under `Assets/Prefab/Enemy/Stage2`.
- Keep the existing Stage 1 hardcoded prefab fallback intact.
- Verify each Stage 2 prefab has the required runtime actor and collision component.

### Constraints

- Role Owner is Code Builder.
- The new binding uses the shared `EnemySpawnManger.enemyPrefabBindings` array instead of adding eight new Stage 2 serialized fields.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, Unity-MCP inspected, and debug-view child wiring checked.

### Next Actions

- User verifies Stage 2 prefab spawn positions and visual scale in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes 8 `enemyPrefabBindings` entries for `stage2-fire-dragon-slayer`, `stage2-lightning-scout`, `stage2-ice-guard`, `stage2-dark-assassin`, `stage2-holy-priest`, `stage2-ethan`, `stage2-drake`, and `stage2-arsen`.
- Unity-MCP scene inspection after reloading `Assets/Scenes/NewScene/NewRunScene.unity` showed all 8 Stage 2 bindings on `GameManager` / `Pakuri.InGame.EnemySpawnManger`.
- Unity-MCP `manage_asset get_components` found both `Pakuri.InGame.EnemyUnitActor` and `UnityEngine.BoxCollider2D` on all 8 Stage 2 prefabs, including `Assets/Prefab/Enemy/Stage2/stage2-holy-priest.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` defines the auto-bound debug child names as `MonsterNameLabel`, `MonsterHpLabel`, `Damage`, `Background`, `Fill`, and `Shield`.
- `Pakuri/Assets/Scripts2/InGame/Units/EnemyUnitActor.cs` calls `ResolveDebugViewReferences()` from `Initialize()` and resolves those children through `UnitActorView.FindTextMesh(...)` / `FindChildTransform(...)`.
- Unity-MCP `manage_prefabs get_hierarchy` found all 8 Stage 2 prefabs have `Damage` with `TextMesh`, `MonsterHpBar` with `Background`/`Fill`/`Shield` sprite children, `MonsterHpLabel` with `TextMesh`, and `MonsterNameLabel` with `TextMesh`.

### History

- 2026-05-31: User stated `stage2-holy-priest.prefab` now had `EnemyUnitActor` and `BoxCollider2D`, then requested Stage 2 prefab connection work.
- 2026-05-31: Code Builder added the shared prefab-binding array and connected all Stage 2 prefab assets in `NewRunScene`.
- 2026-05-31: Code Builder checked the newly added Stage 2 prefab debug-view children against `EnemyUnitActor` / `UnitActorView` and found the actual prefab names match the runtime auto-binding names.

## Task: 2026-05-28 Vega-A Skill Prefab Catalog Wiring

### Task title

Keep Vega-A base projectile visuals and shared follow-up projectiles wired through the active CSV runtime asset-catalog path.

### Goals

- Author Vega-A base skill visual authority on `Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- Reuse the same prefab path for shared follow-up projectile spawning instead of creating a second Vega-only visual route.
- Keep Vega-A master-2 slash visual authority on the user-provided `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` effect row.
- Keep the active runtime asset catalog as the resolver for the CSV-authored prefab path.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- No prefab content edit was performed in this task.
- Asset-path authority stayed on the active skill CSV and runtime asset catalog.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented and Unity runtime-catalog path validated.

### Next Actions

- User verifies in Play Mode that both the base Vega-A burst projectiles and master-1 shadow follow-up projectile resolve the same requested prefab path.
- User verifies in Play Mode that Vega-A master-2 kill slashes resolve `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` through the triggered effect row.
- If later Vega-A branches require a different visual, add that as a CSV-authored choice/effect prefab path instead of a hardcoded asset lookup.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now sets `vega-a.skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_A.prefab` is the exact user-provided prefab path used for this implementation.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now sets `vega-a-master2-transfer-mark.skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` exists at the exact user-provided path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now reuses the resolved projectile prefab path when executing shared follow-up projectiles.
- Unity refresh completed after the CSV update, and the filtered Unity console returned no asset-catalog or CSV runtime errors.

### History

- 2026-05-28: User explicitly supplied `Assets/Prefab/Skill/Vega/Vega_A.prefab` as the Vega-A effect reference path, so the active wiring stayed on the existing CSV/runtime catalog route.
- 2026-05-28: User later supplied `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` as the Vega-A master-2 slash effect path, so that branch also stayed on the existing CSV/runtime catalog route.

## Task: 2026-05-27 Sein-C/D Skill Prefab And Catalog Wiring

### Task title

Keep Sein-C and Sein-D visuals wired through the active scene `EffectManager` and CSV runtime asset catalog paths.

### Goals

- Use `Assets/Prefab/Skill/Sein/Sein_B.prefab` as the flying projectile visual for Sein-C through the scene `EffectManager`.
- Keep Sein-C explosion / master effects and Sein-D zone / master effects resolvable through the runtime asset catalog from CSV-authored prefab paths.
- Avoid creating a new asset-routing path for delayed projectile impact visuals or residual zones.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Base flying projectile visual authority remains scene-owned through `EffectManager`.
- Follow-up explosion and zone visuals remain CSV-authored and runtime-catalog resolved.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and file-verified.

### Next Actions

- User verifies in Play Mode that Sein-C uses `Sein_B.prefab` while flying, then swaps to the requested impact / residual-zone visuals.
- Future delayed projectile skills should keep this split authority: scene mapping for the flying visual, CSV/runtime catalog for follow-up effect prefabs.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `sein` skill `sein-c` to prefab GUID `2d30ba8904b73e2439b402f4782aefb3`, the requested `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now points `sein-c.skill_effect_prefab_path` to `Assets/Prefab/Skill/Sein/Sein_C.prefab` and `sein-d.skill_effect_prefab_path` to `Assets/Prefab/Skill/Sein/Sein_D.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now points `sein-c-master2-contact` to `Assets/Prefab/Skill/Sein/Sein_C_Master-2.prefab`, `sein-c-master1-zone` to `Assets/Prefab/Skill/Sein/Sein_C_Master_1.prefab`, and `sein-d-master2-zone` to `Assets/Prefab/Skill/Sein/Sein_D_Master_2.prefab`.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` was updated by that sync and remains the runtime prefab-path resolver for the CSV-authored Sein effect prefabs.

### History

- 2026-05-27: User specified explicit Sein-C and Sein-D prefab paths for projectile, explosion, and zone visuals; the active wiring stayed on the existing scene `EffectManager` plus CSV runtime catalog split.

## Task: 2026-05-26 Rin Unit Animator Component Wiring

### Task title

Attach the new Rin animation controller component to the active `Rin_Unit` monster prefab.

### Goals

- Keep Rin unit animation wiring on `Assets/Prefab/Monster/Rin_Unit.prefab`.
- Reuse the already assigned `Rin_Animation_Cont.controller` Animator controller.
- Avoid scene-wide or CSV-owned animation wiring in this first implementation.

### Constraints

- Role Owner is Code Builder.
- The root prefab already carried `MonsterUnitActor` and `Animator`; this task only adds `Animation_Controller`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP inspected.

### Next Actions

- User verifies the animated Rin unit in Play Mode.

### Evidence

- Unity-MCP `manage_prefabs get_hierarchy` for `Assets/Prefab/Monster/Rin_Unit.prefab` showed the root `Rin_Unit` with `UnityEngine.Transform`, `UnityEngine.SpriteRenderer`, `Pakuri.InGame.MonsterUnitActor`, `UnityEngine.Animator`, and `Pakuri.InGame.Animation_Controller`.
- `Pakuri/Assets/Prefab/Monster/Rin_Unit.prefab` serializes `Pakuri.InGame.Animation_Controller` with script GUID `3ab96406b52c3454daa4c602c0b81989`.
- Unity editor code inspection returned `actor=True|animator=True|animationController=True|controllerName=Rin_Animation_Cont|clips=Anim_Rin_Idle,Anim_Rin_Attack_1,Anim_Rin_Attack_2,Anim_Rin_Attack_3,Anim_Rin_Dead_1,Anim_Rin_Hit`.

### History

- 2026-05-26: User requested the Rin animation implementation to be assigned only to `Assets/Prefab/Monster/Rin_Unit.prefab` for now.

## Task: 2026-05-26 Rin-B/Rin-C EffectManager Scene Wiring

### Task title

Keep Rin-B and Rin-C base skill visuals wired through the active `NewRunScene` `EffectManager` path.

### Goals

- Add the missing base `rin-b` scene visual mapping to `Assets/Prefab/Skill/Rin/Rin_B.prefab`.
- Keep `rin-c` grounded on the existing `Assets/Prefab/Skill/Rin/Rin_C.prefab` scene mapping.
- Avoid moving base monster skill prefab authority back into skill CSV rows.

### Constraints

- Role Owner is Skill Builder.
- No prefab content edit was required in this task.
- Base skill visuals remain scene-owned through `EffectManager`.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Implemented and file-verified.

### Next Actions

- User verifies in Play Mode that Rin-B shows `Rin_B.prefab` and Rin-C continues to show `Rin_C.prefab`.

### Evidence

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_B.prefab.meta` stores GUID `1265e3a5e02b7f14cb94a3a818221ffa`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `SkillId: rin-b` to `Prefab: {fileID: 2447093715789092070, guid: 1265e3a5e02b7f14cb94a3a818221ffa, type: 3}`.
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_C.prefab.meta` stores GUID `c17e18be6f4f31b49a083bf1ce120f0d`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` keeps `rin-c` mapped to `Prefab: {fileID: 8767310348598417902, guid: c17e18be6f4f31b49a083bf1ce120f0d, type: 3}`.
- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` remains the active base monster skill visual resolver through `ResolveMonsterSkillEffectPrefab(...)`.

### History

- 2026-05-26: User supplied `Assets/Prefab/Skill/Rin/Rin_B.prefab` and `Assets/Prefab/Skill/Rin/Rin_C.prefab` as the required Rin-B/Rin-C effect paths.

## Task: 2026-05-24 Rin-A Master-2 Choice Prefab Catalog Sync

### Task title

Sync Rin-A master-2 choice-level prefab path into the runtime asset catalog.

### Goals

- Keep base Rin-A visual authority scene-owned through `NewRunScene` `EffectManager`.
- Make the master-2 choice-level `skill_effect_prefab_path` resolvable through `PakuriCsvRuntimeAssetCatalog`.
- Reuse `Assets/Prefab/Skill/Rin/Rin_A.prefab` for the master-2 branch/effect path as requested.

### Constraints

- Role Owner is Skill Builder.
- No prefab content was edited.
- Base `rin-a` scene mapping remains unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder

### Status

Synced and file-verified.

### Next Actions

- User verifies in Play Mode that Rin-A master-2 uses the intended Rin_A visual on branch/effect projectiles.
- Future choice-level prefab paths should continue to sync through `Pakuri/Sync CSV Runtime Catalog Assets`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `rin-a-master-2` `skill_effect_prefab_path=Assets/Prefab/Skill/Rin/Rin_A.prefab`.
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_A.prefab.meta` stores GUID `19bfba788239eba498a44cb67c2622c6`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` already maps monster `rin` skill `rin-a` to the same GUID through `EffectManager`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains `AssetPath: Assets/Prefab/Skill/Rin/Rin_A.prefab` with GUID `19bfba788239eba498a44cb67c2622c6`.

### History

- 2026-05-24: User required Rin-A master-2 effect to use `Assets/Prefab/Skill/Rin/Rin_A.prefab`.

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
