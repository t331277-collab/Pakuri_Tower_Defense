## Archived History

- Non-July task blocks from `boards\DATA\GAMEDATA_ASSET_BLACKBOARD.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/GAMEDATA_ASSET_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad data/asset history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and other archive files under `boards/ARCHIVE/`.
- This active file now keeps only the current runtime prefab/catalog wiring still useful for day-to-day work.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-17 PrisonPanel Portrait Asset Wiring

### Task title

Wire player and prisoner portrait assets into the authored PrisonPanel without adding CSV asset columns.

### Goals

- Serialize the five user-selected player portrait sprites on the active `InGameUIManager` scene component.
- Reuse current enemy prefab root sprites for Stage 1/2 prisoner portraits.
- Keep prisoner Korean names owned by the existing enemy CSV `display_name` values.

### Constraints

- Role Owner is Code Builder.
- No CSV schema or runtime asset-catalog entry was added.
- Enemy combat prefab bindings remain the portrait mapping authority for prisoner images.
- Unity Play Mode visual verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated; user Play Mode visual verification pending.

### Next Actions

- User verifies aspect/cropping and portrait clarity for all five player units and Stage 1/2 prisoners.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` serializes the requested Ariel, Eve, Rin, Sein, and Vega Sprite GUIDs on `Pakuri.InGame.InGameUIManager`.
- Unity-MCP component inspection resolved those five references back to the exact requested asset paths.
- `Pakuri/Assets/Scripts2/InGame/Core/EnemySpawnManger.cs` exposes `ResolveEnemyPortraitSprite(...)`, which reads the root SpriteRenderer from the already routed enemy prefab.
- Unity scene validation reported 0 missing scripts, 0 broken prefabs, and 0 total issues.

### History

- 2026-07-17: Code Builder connected the requested player images and existing enemy prefab images to PrisonPanel presentation.

## Task: 2026-07-17 Enemy Source Catalog Simplification

### Task title

Remove the Enemy loadout and unit/projectile sprite CSV inputs from runtime source/catalog authority.

### Goals

- Stop serializing and loading `enemy_skill_loadouts.csv`.
- Keep Enemy unit visuals owned by spawned prefab bindings.
- Keep skill visuals owned by each typed base skill row.

### Constraints

- Role Owner is Code Builder.
- Existing Enemy prefab bindings and runtime skill visual asset paths remain unchanged.
- No Enemy skill prefab is deleted or moved by this task.

### Role Owner

Code Builder

### Status

Implemented and statically verified.

### Next Actions

- User verifies prefab-spawned Enemy visuals and base-row skill visuals in Play Mode.
- Run the normal Unity source-catalog sync only if Unity later regenerates the catalog asset.

### Evidence

- `PakuriCsvRuntimeSourceCatalog.cs` and `PakuriCsvRuntimeSourceCatalog.asset` contain no `EnemySkillLoadouts` entry.
- `PakuriCsvRuntimeData.Loader.cs`, `.Editor.cs`, `.AssetReferences.cs`, and `.Build.cs` no longer load loadouts or Enemy unit/projectile sprite paths.
- `EnemySpawnManger` continues to instantiate configured prefab bindings; `UnitFactory` does not require Enemy CSV sprite paths.
- Search found 0 deleted loadout GUID references under `Pakuri/Assets`.
- Solution build passed with 0 errors.

### History

- 2026-07-17: Code Builder removed redundant Enemy source-catalog inputs while preserving prefab and typed-base visual authority.

## Task: 2026-07-17 Eve-A Branch Runtime Visual Removal

### Task title

Remove branch-projectile visual dependency and use a temporary runtime blue line for Eve-A branch damage.

### Goals

- Stop resolving the base projectile runtime visual or prefab for branch effects.
- Create the temporary electrical connection directly at runtime.
- Keep existing Eve projectile assets and catalogs unchanged.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- No prefab, Sprite, AnimatorController, scene mapping, catalog entry, or CSV asset-path column is added.
- The temporary line is presentation-only and must not own hit detection.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented without asset or catalog changes; user Play Mode visual verification remains.

### Next Actions

- User checks blue-line visibility, width, sorting, and 0.12-second duration in Play Mode.
- Keep existing Eve projectile assets until their separate decommission task is complete.

### Evidence

- `ProjectileBranchDamageSpec` now contains only chance, count, damage multiplier, and search radius.
- The previous runtime visual and projectile prefab fields were removed from the branch spec.
- `InGameProjectileActor.SpawnBranchDamageLine(...)` creates and destroys a runtime `LineRenderer` and material without asset lookup.
- No prefab, scene, runtime catalog, or asset catalog file was changed for this task.
- Runtime and Editor builds passed with 0 errors.

### History

- 2026-07-17: User requested an interim blue electrical line instead of spawned branch projectiles.
- 2026-07-17: Code Builder implemented the presentation entirely in runtime code and left all asset/catalog authority unchanged.

## Task: 2026-07-16 Enemy Skill Prefab Legacy Move And Visual Fallback Removal

### Task title

Remove active scene-owned Enemy skill visual fallback while preserving the old prefabs as GUID-stable Legacy evidence.

### Goals

- Use base-row runtime visual data as the active Enemy skill visual authority.
- Remove Enemy prefab mapping from `EffectManager` and `NewRunScene`.
- Move all old Enemy skill prefabs to Legacy without deleting or regenerating them.

### Constraints

- Role Owner is Code Builder.
- Prefab `.meta` files and GUIDs must remain unchanged.
- Enemy runtime hitbox offset remains `(0,0)`; only size is authored.
- `OpeningCharge` has no runtime visual because the inspected legacy scene/prefab mapping had none.

### Role Owner

Code Builder

### Status

Scene fallback removed and prefab Legacy move completed; static and Unity validation pass.

### Next Actions

- User verifies the 15 represented visuals and centered gameplay hitboxes in Play Mode.
- Keep `Pakuri/Assets/Legacy/Enemy/Skill` as migration evidence unless a later explicit archive policy changes it.

### Evidence

- `EffectManager.cs` has no Enemy skill prefab registry/resolver; `NewRunScene.unity` has no `enemySkillEffects` block.
- `Pakuri/Assets/Prefab/Enemy/Skill` is absent.
- `Pakuri/Assets/Legacy/Enemy/Skill` contains 15 prefabs and 15 matching prefab metadata files.
- All 15 prefab GUIDs match their pre-move `HEAD` values.
- Search outside the Legacy skill folder found 0 references to those 15 GUIDs.
- Runtime visual asset verification found 15 visual-bearing skills with 0 missing Sprite/Animator assets; `OpeningCharge` is the sole intentional visual-less skill.
- Enemy CSV has no `runtime_hitbox_offset_x/y`; the Enemy parser only contains a guard rejecting those columns.

### History

- 2026-07-16: Code Builder removed the scene fallback, preserved all Enemy skill prefab GUIDs, and moved the folder to Legacy.

## Task: 2026-07-16 Enemy Skill Prefab Legacy Preservation Gate

### Task title

Preserve Enemy skill prefabs during Phase 9 cleanup and define their later Legacy destination.

### Goals

- Prevent deletion of the 15 current Enemy skill prefabs.
- Preserve Unity GUIDs through `.meta`-retaining folder movement.
- Delay movement until runtime visual parity and serialized-reference cleanup are proven.

### Constraints

- Role Owner is Code Builder.
- No prefab or `.meta` file is moved in this task.
- Collider offset is not migrated; only gameplay-required size remains runtime data.

### Role Owner

Code Builder

### Status

Preservation policy documented. Actual movement blocked by remaining scene fallback and Unity reference verification.

### Next Actions

- Complete visual/hitbox Play Mode parity for all 16 skills.
- Remove `EffectManager` Enemy enum fallback and confirm scene/prefab/asset serialized references are 0.
- Move the complete `Pakuri/Assets/Prefab/Enemy/Skill` folder to `Pakuri/Assets/Legacy/Enemy/Skill` with folder and prefab `.meta` files preserved.

### Evidence

- Source folder contains 15 `.prefab` files and 15 matching `.prefab.meta` files.
- `Pakuri/Assets/Prefab/Enemy/Skill.meta`, `Stage1.meta`, and `Stage2.meta` exist.
- `Pakuri/Assets/Legacy` exists, but `Pakuri/Assets/Legacy/Enemy/Skill` does not.
- `EffectManager.cs` still contains `StageOneEnemySkillKind` Enemy prefab mappings.
- Migration report now states that Enemy skill prefabs are not Phase 9 deletion candidates.

### History

- 2026-07-16: User required Legacy preservation instead of deletion; Code Builder recorded the exact source, destination, and `.meta` boundary.

## Task: 2026-07-16 Enemy Runtime Visual Consumption Phase 4-6

### Task title

Use Enemy base-row runtime visuals and centered hitbox sizes from shared typed executors.

### Goals

- Spawn Phase 4-6 Enemy visuals through `RuntimeSkillVisualFactory` from base CSV data.
- Use explicit projectile lifetime and gameplay-required centered hitbox size.
- Preserve scene prefab mapping only as fallback.

### Constraints

- Role Owner is Code Builder.
- No runtime hitbox offset columns or prefab offset transfer.
- Existing Enemy skill prefabs and `EffectManager` enum mappings are not removed.
- OpeningCharge still has no authored runtime visual.

### Role Owner

Code Builder

### Status

Runtime visual consumption code implemented and compile-verified. Unity visual parity remains.

### Next Actions

- Verify 15 represented visuals, non-uniform HolySpearThrow scale, sorting, projectile contact, and centered area/single hitboxes in Play Mode.
- Remove scene fallback only after report gate conditions pass.

### Evidence

- Projectile, SingleAttack, Buff, Shield, Heal, and Chain executors prefer `RuntimeSkillVisualSpec` before prefab fallback.
- DamageArea maps to `SingleAttackSkillExecutor`, using runtime hitbox size and no authored offset.
- Heal/Chain support visuals attach to resolved targets; ChargeCommand/GuardianFlag configured visuals attach once to caster.
- Enemy base CSV width check passed with 0 mismatched rows.
- C# solution build passed with 0 errors.

### History

- 2026-07-16: Code Builder activated Enemy base-row runtime visuals for Phase 4-6 shared execution while retaining scene fallback.

## Task: 2026-07-16 Enemy Runtime Visual Catalog Phase 0-3

### Task title

Register Enemy base-row Sprite and Animator Controller paths for runtime visual assembly while retaining scene prefab fallback.

### Goals

- Give each of the 16 base skill rows direct runtime visual authority.
- Catalog all referenced Enemy skill sprites/controllers.
- Preserve non-uniform Ethan scale through shared runtime visual data.
- Transfer only gameplay-required hitbox size and keep runtime offset `(0,0)`.

### Constraints

- Role Owner is Code Builder.
- Existing 15 Enemy skill prefabs and scene mappings are not removed.
- OpeningCharge has no confirmed current skill visual mapping, so its base visual remains empty.
- Phase 4+ behavior transfer is excluded.

### Role Owner

Code Builder

### Status

Phase 0-3 asset references authored and resource catalogs updated. Unity visual parity remains.

### Next Actions

- Run Unity catalog sync/validation after leaving Play Mode or completing script refresh.
- Verify all 15 represented visuals, Ethan non-uniform scale, sorting orders, and centered gameplay hitboxes.
- Remove scene fallback only after later migration phases satisfy the report gates.

### Evidence

- `PakuriCsvRuntimeData.AssetReferences.cs` collects migrated Enemy base visual paths.
- `PakuriCsvRuntimeAssetCatalog.asset` contains 11 unique Enemy skill sprites and 11 Animator Controllers used by the 16 base rows.
- `RuntimeSkillVisualSpec` and `RuntimeSkillVisualFactory` now support optional non-uniform local scale; existing scalar scale behavior remains default.
- New Enemy base CSV contains no offset columns.
- Phase 0 baseline records prefab offsets only as evidence and lists the eight gameplay-required size transfers.
- Static asset-path validation found 0 missing Sprite/Animator Controller paths, and both runtime and Editor C# builds passed with 0 errors.

### History

- 2026-07-16: Code Builder cataloged Enemy base visuals and added non-uniform runtime scale support needed by HolySpearThrow.

## Task: 2026-07-16 Enemy Skill Prefab To Runtime Visual Design

### Task title

Design the migration of Enemy skill prefab metadata from scene enum mappings to shared runtime skill CSV and asset-catalog resolution.

### Goals

- Preserve the current Enemy skill visuals, animator assets, local scale, sorting order, and gameplay-required collider sizes.
- Resolve Enemy skill visuals by the direct A/B base skill IDs rather than Enemy ID plus StageOneEnemySkillKind scene mappings.
- Store runtime visual fields directly on each typed base skill row without a separate visual override layer.

### Constraints

- Role Owner is Designer.
- No prefab, scene, or asset catalog was edited.
- Existing scene mappings remain a fallback until all 16 skill visuals and 15 prefab parity checks pass.
- Follow `boards/MON/VEGA_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`: do not migrate prefab collider offsets or add offset CSV columns.
- Runtime hitbox offset is `(0,0)`; carry only a size proven necessary for gameplay collision.
- One skill ID owns one runtime visual; different visuals use distinct skill IDs/base rows while sharing the same typed executor.

### Role Owner

Designer

### Status

Prefab and scene evidence recorded in the migration report; implementation has not started.

### Next Actions

- Extract prefab Sprite and Animator Controller asset paths, scale, sorting, and gameplay-required hitbox size directly into typed base skill runtime visual fields.
- Classify collider authority per skill before authoring size; omit hitbox data when targeting/radius/query code already owns gameplay detection.
- Generalize `EffectManager` to one shared visual resolver backed by `PakuriCsvRuntimeAssetCatalog`.
- Define an explicit visual policy for `OpeningCharge`/Stage 2 Drake before removing scene fallback.
- Verify runtime actor attachment because the inspected prefabs contain no MonoBehaviour behavior.

### Evidence

- Design report: `Pakuri/reference/Report/2026-07-16-enemy-shared-skill-runtime-csv-migration-plan.md`.
- `Pakuri/Assets/Prefab/Enemy/Skill/` contains 15 inspected prefabs.
- Each inspected prefab has one GameObject, one SpriteRenderer, one Animator, and zero MonoBehaviours.
- Eight prefabs contain BoxCollider2D and seven are visual-only.
- Karin, Warrior, and Ice Guard contain inspected non-zero prefab collider offsets, but the Vega migration boundary intentionally does not transfer prefab offsets and keeps runtime collider centers at `(0,0)`.
- `RuntimeSkillVisualFactory.ConfigureHitbox(...)` currently assigns both size and offset from `RuntimeSkillHitboxSpec`; the Enemy migration contract must therefore leave offset input absent/defaulted to zero rather than authoring prefab offsets.
- `RuntimeSkillVisualFactory` creates SpriteRenderer, optional Animator, and optional BoxCollider2D at runtime, matching the Vega migration method.
- Monster typed base CSV headers already contain direct runtime visual sprite/controller/scale/sorting/hitbox-size columns, supporting the same direct-authority structure for Enemy.
- `NewRunScene.unity` contains 21 Enemy ID plus StageOneEnemySkillKind mappings in `EffectManager`; no Stage 2 Drake/OpeningCharge mapping was found.
- `EffectManager` currently has separate Monster and Enemy skill effect prefab resolution paths.

### History

- 2026-07-16: Inspected all Enemy skill prefab YAML and the scene mapping structure; documented shared visual resolution and fallback retirement gates.
- 2026-07-16: Revised the plan to match Vega runtime visuals: no prefab offset migration, no offset CSV columns, and size only for gameplay-required colliders.
- 2026-07-16: Removed visual override ownership; each Enemy typed base row now directly owns its runtime visual fields.

## Task: 2026-07-14 Choice Prefab Graph Authority Cleanup

### Task title

Move remaining Choice prefab metadata to graph authority and remove obsolete export/catalog inputs.

### Goals

- Keep Rin A master-2 prefab resolution through the runtime asset catalog while moving its source path to a graph node.
- Prevent editor tooling from recreating deleted Choice wide columns.
- Remove empty legacy Effect TextAsset references from the runtime source catalog.

### Constraints

- Role Owner is Code Builder.
- No prefab or scene content was changed.
- Runtime asset resolution still uses `PakuriCsvRuntimeAssetCatalog` and normalized asset-path params.

### Role Owner

Code Builder

### Status

Implemented and Unity catalog/source validation passed.

### Next Actions

- User verifies Rin A master-2 visual parity in Play Mode.
- Future choice-specific prefab paths are authored as Choice/Plan `EffectVisual` graph nodes.

### Evidence

- `rin-a-master-2` now stores `Assets/Prefab/Skill/Rin/Rin_A.prefab` in `skill_graph_nodes_projectile.csv` as `EffectVisual arg_1`; the Choice CSV has no prefab column.
- Generic normalized-node asset collection already catalogs `asset_path` params, and Unity source validation loaded the catalog successfully.
- `PakuriCsvRuntimeSourceCatalog.asset` no longer serializes the six removed legacy Effect TextAssets.
- The deleted `PakuriSkillEffectPrefabCsvExporter.cs` searched missing `Assets/Data/GameData/Monsters` and emitted a Unity error; removing it prevents regeneration of `skill_effect_prefab_path` on Choice CSVs.
- Unity-MCP sync completed and InGame skill data validation passed with 0 warnings.

### History

- 2026-07-14: Code Builder moved Rin choice prefab authority to graph metadata, removed obsolete legacy asset inputs/tooling, and synchronized the runtime catalogs.

## Task: 2026-07-13 Sein Zone Prefab Collider Authority

### Task title

Use the authored Sein zone prefab colliders as runtime hit boundaries.

### Goals

- Keep collider shape/offset authority on the three user-edited Sein prefabs.
- Ensure runtime mapping does not suppress the collider path for `sein-d` solely because its runtime kind is `Field`.

### Constraints

- Role Owner is Code Builder.
- The prefab edits are user-authored and must not be overwritten.
- No scene or runtime asset-catalog path changes are required.

### Role Owner

Code Builder

### Status

Runtime mapping fixed and Unity-MCP prefab hierarchy validated; Play Mode boundary verification remains.

### Next Actions

- User verifies the exact collider shapes and offsets against visible effects in Play Mode.

### Evidence

- Git diff shows user-added enabled root `BoxCollider2D` components on `Sein_C_Master_1.prefab`, `Sein_D.prefab`, and `Sein_D_Master_2.prefab`.
- Unity-MCP `get_hierarchy` reports each prefab root as active with `Transform`, `SpriteRenderer`, `Animator`, and `BoxCollider2D`.
- `InGameSkillDefinitionMapper.cs` now leaves `Field` zones spatial unless explicit all-target data requests `CoverAll`, allowing `InGameZoneSkillActor` to consume those prefab colliders.
- No prefab, scene, CSV, or asset-catalog file was edited by Code Builder for this fix.

### History

- 2026-07-13: User authored the Sein zone colliders; Code Builder connected Sein-D to the existing collider-first zone runtime by removing the implicit `Field` cover-all override.

## Task: 2026-07-12 Rin Runtime Visual Asset Boundary

### Task title

Record which Rin skill prefab contracts can move to shared runtime visuals.

### Goals

- Move approved single-root compositions out of prefab authority, including Rin D master 1 through an exact offset-preserving runtime spec.
- Preserve Rin D base prefab authority and Rin E named child-hitbox authority.
- Keep prefab deletion separate from the runtime migration.

### Constraints

- Role Owner is Code Builder for the approved implementation phase.
- Prefab assets and scene mappings remain untouched; runtime asset catalog changes are generated from CSV paths.
- Existing prefab assets remain available for parity inspection.

### Role Owner

Code Builder

### Status

Runtime visual asset paths implemented and catalog-validated; user Play Mode parity remains.

### Next Actions

- User verifies runtime visuals for Rin A/B/C/F and D master 1 in Play Mode.
- Retain `Rin_D.prefab` and `Rin_E.prefab` as active prefab dependencies.
- Keep converted prefabs and A-C scene mappings on disk/as fallback evidence until parity confirmation.

### Evidence

- Scene `EffectManager` currently maps Rin A-D base visuals.
- Active CSV paths additionally reference Rin A master 2, Rin D master 1, Rin E, and Rin F.
- Unity-MCP found only `Rin_E.prefab` has a child object; its `CoreHitBox` name is consumed by SingleAttack core-effect code.
- `Rin_D_master_1.prefab` has a non-zero collider offset; revised design preserves it through optional runtime offset fields rather than retaining prefab execution.
- User explicitly selected Rin D base for prefab retention and D master 1 for runtime conversion.
- No Rin G-J prefab/status visual reference exists in the active runtime skill CSV tree.
- Runtime asset catalog now contains Rin A/B/C/F/D-master1 sprite paths and B/C/F animator-controller paths from active CSV data.
- Runtime specs take precedence, while existing A-C scene mappings and F/D-master1 Trigger prefab paths remain fallback references.
- `git diff --name-only` found no change under `Assets/Prefab/Skill/Rin` or `NewRunScene.unity`.
- Unity-MCP source validation loaded the updated catalog without asset errors.

### History

- 2026-07-12: Designer recorded the Rin runtime visual conversion boundary in `boards/MON/RIN_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- 2026-07-13: Designer revised asset authority: Rin D base remains scene/prefab-backed, while Rin D master 1 becomes runtime-backed after offset support is implemented.
- 2026-07-13: Code Builder authored the approved runtime asset paths; Unity auto-sync updated the runtime asset catalog while all Rin prefabs and scene mappings remained unchanged.

## Task: 2026-07-12 Rin Node Migration Asset Compatibility

### Task title

Record the prefab and scene contracts that Rin node migration must preserve.

### Goals

- Keep current Rin base visual mappings and prefab catalog paths intact.
- Preserve Rin-E named `CoreHitBox` Collider behavior and Trigger-owned skill prefabs.

### Constraints

- Role Owner is Designer.
- No prefab, scene, catalog, or visual migration is included.
- Asset cleanup or runtime visual conversion requires a separate approved task.

### Role Owner

Designer

### Status

Compatibility constraints documented; assets unchanged.

### Next Actions

- Code Builder must not rename/remove `CoreHitBox` or replace D/F Trigger prefab paths during node migration.
- Update this board again only if implementation actually changes asset authority.

### Evidence

- `Rin_E.prefab` contains a `CoreHitBox` child and `BoxCollider2D`; `SingleAttackSkillExecutor` resolves that exact name.
- Rin-A/B/C/D have `NewRunScene` `EffectManager` mappings, while Rin-E keeps its base prefab path in normalized single-attack data.
- `Rin_D_master_1.prefab` and `Rin_F.prefab` exist and are referenced by retained Trigger paths.

### History

- 2026-07-12: Designer preserved current Rin prefab/hitbox contracts in the node migration proposal.

## Task: 2026-07-12 Eve Runtime Visual Asset Catalog

### Task title

Catalog Eve runtime Sprite and AnimatorController assets while retaining old prefabs for later cleanup.

### Goals

- Add every Eve A-E and Eve-C master-2 runtime visual asset to `PakuriCsvRuntimeAssetCatalog`.
- Keep old prefab assets and current scene references intact while converted runtime paths stop resolving them.

### Constraints

- Role Owner is Code Builder refactoring track.
- No prefab deletion or `NewRunScene` serialization edit is included.
- Runtime asset lookup remains path-owned through the existing CSV catalog.

### Role Owner

Code Builder

### Status

Catalog regenerated and Unity validation passed. Eve-C master-2 resolves only its runtime Sprite/Animator assets; its retained prefab is no longer a runtime catalog dependency.

### Next Actions

- Retain Eve prefabs until all monster skill migrations are complete, then perform one explicit cleanup pass.
- User verifies Sprite/Animator parity in Play Mode.
- User verifies Eve-B/E do not create status-attached duplicates and Ariel-D keeps its intentional target-attached status visual.

### Evidence

- `PakuriCsvRuntimeAssetCatalog.asset` now includes Eve A-E and Eve-C master-2 Sprite and AnimatorController paths resolved from the retained prefabs' GUID-backed assets.
- The regenerated catalog contains Eve-C master-2's Sprite and AnimatorController paths and contains no `Assets/Prefab/Skill/Eve/Eve_c-master-2.prefab` entry.
- AnimatorController-valued graph asset params are now cataloged and validated as animator controllers instead of prefabs.
- `Pakuri/Sync CSV Runtime Catalog Assets` regenerated the runtime catalog, and `Pakuri/Validate CSV Source Data` subsequently loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- `Assets/Prefab/Skill/Eve/Eve_A.prefab` through `Eve_E.prefab` and `Eve_c-master-2.prefab` still exist; `NewRunScene.unity` was not changed.
- Runtime visual anchor data changes only composition ownership; no Sprite, AnimatorController, prefab, catalog GUID, or scene mapping was deleted or replaced by the Eve-B/E bug fix.

### History

- 2026-07-12: Added Eve runtime visual assets to the catalog and intentionally deferred prefab deletion.
- 2026-07-12: Separated status-target visual ownership and disabled hitbox creation on status decorations; retained the existing Eve asset catalog and prefabs.
- 2026-07-13: Regenerated the catalog after authoring Eve-C master-2's runtime Collider size; retained the prefab asset on disk without restoring a prefab-path dependency.

## Task: 2026-07-12 Eve Skill Graph Catalog Wiring

### Task title

Register the new Eve line-attack and area-attack graph sources in the runtime catalog.

### Goals

- Import the two new graph CSV assets and keep generated runtime catalog references synchronized.

### Constraints

- Role Owner is Code Builder.
- Unity-MCP is the only MCP used; no MSW-MCP.

### Role Owner

Code Builder

### Status

Complete; Play Mode behavior verification remains user-owned.

### Next Actions

- Preserve both graph source references during future catalog regeneration.

### Evidence

- Unity generated `.meta` files for `skill_graph_nodes_line_attack.csv` and `skill_graph_nodes_area_attack.csv`.
- `PakuriCsvRuntimeSourceCatalog.asset` gained the two graph sources and Unity CSV validation loaded the 5-monster catalog successfully.
- The user added an enabled `BoxCollider2D` to `Assets/Prefab/Skill/Eve/Eve_D.prefab`. Eve-D now uses that authored footprint as its base hitbox, while existing root-scale radius modifiers resize Sprite and Collider together.

### History

- 2026-07-12: Added and validated Eve line/area graph catalog wiring.
- 2026-07-12: Recorded Eve-D prefab Collider as base footprint authority; the paired CSV row uses `radius=0` and `hit_target_count=global`.

## Task: 2026-07-10 Runtime Skill Visual Asset Catalog

### Task title

Add runtime skill visual Sprite/AnimatorController catalog support for Ariel migration.

### Goals

- Let runtime skill CSV rows reference Sprite and AnimatorController assets directly instead of instantiating Ariel skill prefabs.
- Keep old Ariel prefabs as assets, but do not use them as fallback for converted Ariel base/trigger/status paths.

### Constraints

- Role Owner is Code Builder.
- Active runtime CSV authority remains under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
- No MSW-MCP was used.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- Run Unity Editor CSV runtime catalog sync/validation after editor reload if asset import validation is needed.
- Keep future runtime visual asset paths in base/trigger CSVs unless a separate node-effect visual migration is explicitly approved.

### Evidence

- `PakuriCsvRuntimeAssetCatalog.cs` now stores Sprite, Prefab, and RuntimeAnimatorController entries.
- `PakuriCsvRuntimeData.AssetReferences.cs`, `PakuriCsvRuntimeData.Editor.cs`, `PakuriCsvRuntimeData.Validation.cs`, and `PakuriCsvRuntimeData.Build.cs` collect, build, validate, and load runtime visual sprite/controller paths.
- Ariel runtime visual CSV fields were added to `skills_projectile.csv`, `skills_buff.csv`, `skills_single_attack.csv`, `buff_skill_triger.csv`, and `projectile_skill_triger.csv`.
- Runtime hitbox CSV fields now keep only size data. BoxCollider2D creation and trigger-state policy are code-owned instead of asset-catalog-owned.
- CSV field-count verification passed for those five files.
- Runtime catalog sync now forces AssetDatabase synchronous import before reading source TextAssets, preventing stale imported CSV content from feeding the asset catalog after external file edits.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets` and `Pakuri/Validate CSV Source Data` completed without CSV fatal errors after the refresh/import change.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.

### History

- 2026-07-10: Code Builder implemented Ariel runtime visual/hitbox migration and extended the runtime asset catalog to carry AnimatorController references.
- 2026-07-10: Code Builder removed the common hitbox shape/trigger CSV columns from the runtime visual path; the asset catalog remains responsible for Sprite/AnimatorController resolution only.
- 2026-07-10: Code Builder fixed a stale TextAsset sync risk reported as `skills_projectile.csv` row 4 width mismatch by forcing synchronous import before runtime catalog source TextAssets are read.

## Task: 2026-07-05 Monster Skill Choice Split SourceCatalog Wiring

### Task title

Wire split monster skill choice runtime CSV files and purpose folders into the runtime source catalog.

### Goals

- Replace the serialized single `MonsterSkillChoices` TextAsset reference with six split choice TextAsset references.
- Preserve moved CSV asset references by keeping existing body/effect/trigger/node `.meta` GUIDs with their moved CSV files.
- Make editor sync and runtime loading use the new purpose folders under `Assets/CSVdata/authoring/monster/skills`.
- Keep the skill effect prefab exporter compatible with split choice CSV files.

### Constraints

- Role Owner is Code Builder.
- Runtime source catalog asset remains under `Assets/Resources/Pakuri/CSVRuntime`.
- No prefab-path ownership changed; only CSV file locations and split TextAsset wiring changed.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- When adding another monster skill choice split, add the TextAsset field, filename constant, path mapping, editor assignment, serialized source catalog reference, and exporter path together.

### Evidence

- `PakuriCsvRuntimeSourceCatalog.cs` now declares `MonsterSkillChoicesProjectile`, `MonsterSkillChoicesLineAttack`, `MonsterSkillChoicesAreaAttack`, `MonsterSkillChoicesSingleAttack`, `MonsterSkillChoicesBuff`, and `MonsterSkillChoicesPassive`.
- `PakuriCsvRuntimeSourceCatalog.asset` now serializes split choice GUIDs `964c31a67f8c4fa6a922d8d7c2270fe8`, `51ee29cf373a41eea8cc76054785af64`, `d45513b00f504a0e8ead76ca995e0c4b`, `bcb4dbe3464a43979e4476b3f5bcdc05`, `30f713f3e3f3440abd8934dd1ca66bb6`, and `e5779c680c304497a2525150a34fa156`.
- `PakuriCsvRuntimeData.cs` maps skill body CSVs to `skills/base`, split choice CSVs to `skills/choices`, effects to `skills/effects`, triggers to `skills/triggers`, and nodes/node params to `skills/nodes`.
- `PakuriCsvRuntimeData.Editor.cs` loads all six split choice TextAssets through `LoadImportedSourceTextAssetOrThrow(...)`.
- `PakuriCsvRuntimeData.Loader.cs` loads all six split choice files and rejects a choice row when the owner skill's `runtime_kind` does not belong to that split file.
- `PakuriCsvRuntimeData.MonsterDataset.cs` now parses omitted split-choice payload columns through optional-if-column-exists helpers.
- `PakuriSkillEffectPrefabCsvExporter.cs` now updates all six split choice CSV paths and adds `skill_effect_prefab_path` only when a matching row needs that column in a split file.
- PowerShell verification returned `ROOT_CSV_COUNT=0`, `OLD_CHOICE_EXISTS=False`, and SourceCatalog GUID lines matching the six split choice `.meta` GUIDs.
- PowerShell search under `Pakuri/Assets` returned no remaining `monster_skill_choices.csv`, `MonsterSkillChoicesFileName`, `MonsterSkillChoices:`, old choice GUID `5b5f094e9fbfaef4593518ad6d855917`, `monster_skills.csv`, `MonsterSkillsFileName`, or `MonsterSkills:` matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User approved splitting `monster_skill_choices.csv` by owner skill `runtime_kind` and organizing active monster skill runtime CSVs into folders.

## Task: 2026-07-05 Monster Skills Split SourceCatalog Wiring

### Task title

Wire split monster skill runtime CSV files into the runtime source catalog.

### Goals

- Replace the serialized single `MonsterSkills` TextAsset reference with six split monster skill TextAsset references.
- Preserve the existing source catalog asset path under `Assets/Resources/Pakuri/CSVRuntime`.
- Keep editor sync loading the split CSV files from `Assets/CSVdata/authoring/monster/skills`.

### Constraints

- Role Owner is Code Builder.
- No prefab-path or runtime asset catalog ownership changed in this task.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- When adding another monster skill body split file, add the TextAsset field, file-name constant, path mapping, editor assignment, and serialized source catalog reference together.

### Evidence

- `PakuriCsvRuntimeSourceCatalog.cs` now declares `MonsterSkillsProjectile`, `MonsterSkillsLineAttack`, `MonsterSkillsAreaAttack`, `MonsterSkillsSingleAttack`, `MonsterSkillsBuff`, and `MonsterSkillsPassive`.
- `PakuriCsvRuntimeSourceCatalog.asset` now serializes those six fields using GUIDs `4d2829c1fdc345b7bf1aba23dd7fd4b1`, `1b8eb880f8494399b8151ef2cc0c6ade`, `5fc269887d9b4f6da4981087cd15e34a`, `a6b19806a3ca4578a9204e35ab3c0182`, `b0c7408603e54abd9bbb7fabdb492c7e`, and `22160b9cc31c4eefaca07d56f6e9abd3`.
- `PakuriCsvRuntimeData.Editor.cs` loads all six split files through `LoadImportedSourceTextAssetOrThrow(...)`.
- `PakuriCsvRuntimeData.cs` maps all six split filenames to `RuntimeMonsterSkillCsvAssetRoot`.
- `Select-String` under active Scripts2/resources/CSVdata paths found no remaining `MonsterSkills:` serialized field, `MonsterSkillsFileName`, `monster_skills.csv`, or old monolithic GUID reference.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User asked Code Builder to implement the runtime-kind split and remove the monolithic `monster_skills.csv` source path.

## Task: 2026-07-05 Monster Skill Base CSV SourceCatalog Cleanup

### Task title

Remove unused monster skill base TextAsset references from the runtime source catalog.

### Goals

- Keep `PakuriCsvRuntimeSourceCatalog` aligned with the remaining active runtime monster skill CSV files.
- Remove serialized source catalog references to deleted base CSV tables.
- Preserve the existing runtime CSV source catalog asset path.

### Constraints

- Role Owner is Code Builder.
- Runtime source catalog asset remains under `Assets/Resources/Pakuri/CSVRuntime`.
- No prefab-path or asset-catalog authority changed in this task.
- MSW-MCP is not used; Unity-MCP remains the only MCP path if Unity Editor validation is needed.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- When adding a new runtime monster skill CSV file, add its TextAsset field and path mapping explicitly instead of reintroducing the removed base tables.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/PakuriCsvRuntimeSourceCatalog.cs` no longer declares `MonsterSkillBase` or `MonsterSkillChoiceBase`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` no longer serializes `MonsterSkillBase` or `MonsterSkillChoiceBase`.
- `PakuriCsvRuntimeData.Editor.cs` no longer loads `MonsterSkillBaseFileName` or `MonsterSkillChoiceBaseFileName` into the source catalog.
- `PakuriCsvRuntimeData.cs` no longer maps `monster_skill_base.csv` or `monster_skill_choice_base.csv` through `GetImportedSourceAssetPath(...)`.
- `Select-String` on `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` for removed base-table symbols and filenames returned no matches.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false /clp:ErrorsOnly /v:minimal` passed with 0 errors.

### History

- 2026-07-05: User asked Code Builder to delete `monster_skill_base.csv` and `monster_skill_choice_base.csv`, and to unify runtime monster skill choice references onto `monster_skill_choices.csv`.

## Task: 2026-07-13 Sein Runtime Visual Asset Catalog Wiring

### Task title

Register Sein runtime-composed skill sprites and animator controllers in the runtime asset catalog.

### Goals

- Resolve every authored Sein runtime sprite/controller path through `PakuriCsvRuntimeAssetCatalog`.
- Preserve existing prefab and scene references as staged fallbacks.

### Constraints

- Role Owner is Code Builder.
- No Sein prefab or scene mapping is deleted in this phase.
- User Play Mode parity is required before fallback cleanup.

### Role Owner

Code Builder

### Status

Catalog synchronized and Unity-MCP validated.

### Next Actions

- User verifies runtime presentation in Play Mode.
- Remove fallback prefab/scene wiring later only after parity confirmation.

### Evidence

- `PakuriCsvRuntimeAssetCatalog.asset` now contains Sein sprites `Sein_Shoot.png`, `1.png`, `B-1.png`, `C-1.png`, and `E-1.png` with resolved GUIDs.
- The catalog also contains `1.controller`, `B-1.controller`, `C-1.controller`, and `E-1.controller` with resolved GUIDs.
- Unity-MCP sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/authoring' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Post-sync validation logged the runtime catalog summary and the error-only console query returned 0 entries.

### History

- 2026-07-13: Code Builder synchronized Sein runtime visual references into the asset catalog while retaining prefab fallbacks.

## Task: 2026-07-14 Vega Runtime Visual Asset Handoff

### Task title

Prepare Vega's skill sprite and animator-controller paths for runtime catalog ownership.

### Goals

- Resolve Vega A-E runtime visual assets through `PakuriCsvRuntimeAssetCatalog`.
- Remove active prefab-path authority after static catalog validation while retaining prefab files for comparison.
- Avoid adding Vega skill prefab references to `NewRunScene`.

### Constraints

- Role Owner is Designer.
- The six Vega prefab assets remain on disk during migration.
- Catalog sync and prefab-reference cleanup were completed by the related Code Builder task below.

### Role Owner

Designer

### Status

Implemented and catalog-validated; user Play Mode parity remains.

### Next Actions

- User verifies runtime presentation in Play Mode.

### Evidence

- Unity-MCP and prefab GUID resolution identified the exact sprite/controller paths listed in `boards/MON/VEGA_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.
- Search of all six Vega skill prefab GUIDs returned no occurrence in `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`.
- Active prefab authority is currently located in runtime CSV/graph rows and therefore reaches the runtime asset catalog through the existing CSV asset-reference collector.

### History

- 2026-07-14: Designer recorded the Vega runtime visual catalog handoff while preserving prefabs as staged fallbacks.

## Task: 2026-07-14 Vega Runtime Visual Asset Catalog Sync

### Task title

Resolve Vega runtime skill sprites/controllers through the runtime asset catalog.

### Goals

- Catalog Vega A-E assets authored by active runtime CSV/graph data.
- Remove active Vega skill-prefab catalog authority without deleting prefab files.

### Constraints

- Role Owner is Code Builder.
- Unity-MCP alone executed Editor menu operations.
- Scene and prefab assets were not edited.

### Role Owner

Code Builder

### Status

Catalog synchronized and Unity-MCP validated.

### Next Actions

- User confirms visual parity in Play Mode.

### Evidence

- `PakuriCsvRuntimeAssetCatalog.asset` was regenerated by `Pakuri/Sync CSV Runtime Catalog Assets`.
- Vega assets `Vega_Shoot2`, `B_1`, C effect, `D_1`, `E_1`, and their authored controllers exist and resolve.
- Unity logged successful sync from `Assets/CSVdata/authoring` to `Assets/Resources/Pakuri/CSVRuntime`.
- CSV source validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; InGame skill validation passed with 0 warnings.

### History

- 2026-07-14: Code Builder synchronized and validated Vega runtime visual catalog references.

## Task: 2026-07-17 Enemy Passive Source Catalog Coverage

### Task title

Confirm the Enemy passive base CSV remains reachable through the runtime source catalog.

### Goals

- Keep `skills_passive.csv` included in `EnemySkillBaseFiles`.
- Avoid unnecessary runtime asset-catalog entries because Enemy passive rows contain no visual asset paths.

### Constraints

- Role Owner is Code Builder.
- No prefab, sprite, animator, scene, or runtime asset-catalog mutation is required for this passive-only data change.

### Role Owner

Code Builder

### Status

Source-catalog coverage and Unity catalog sync/validation passed.

### Next Actions

- No GameData asset action remains for this passive-only change.

### Evidence

- `skills_passive.csv.meta` GUID is `7ffdb20f69d1449d83ea68a3801eec8b`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog.asset` already contains that GUID in `EnemySkillBaseFiles`.
- `PakuriCsvRuntimeData.Editor.cs` discovers all Enemy `skills_*.csv` files recursively under the base root.
- Passive rows contain no runtime visual or prefab paths, so `PakuriCsvRuntimeAssetCatalog.asset` needs no new asset reference.
- Open-Editor sync/validation logged `[EnemyPassiveCsvValidation] PASS`; the temporary hook and `.meta` were removed afterward.

### History

- 2026-07-17: Code Builder verified existing source-catalog registration for the populated Enemy passive CSV.

## Task: 2026-07-17 CSV Authoring Asset Path Migration

### Task title

Preserve Unity asset identity while moving CSV source assets from `runtime` to `authoring`.

### Goals

- Preserve all retained TextAsset GUID references through the folder rename.
- Regenerate the runtime source catalog without references to deleted empty node CSV assets.
- Keep `Assets/Resources/Pakuri/CSVRuntime` as the generated runtime catalog location.

### Constraints

- Role Owner is Code Builder.
- Move the root folder and matching `.meta` together.
- Delete only the eight verified zero-data-row node CSV assets and their folder metadata.
- Runtime asset catalog output paths and resource load paths remain unchanged.

### Role Owner

Code Builder

### Status

Implemented and Unity-validated.

### Next Actions

- Use `SyncCsvRuntimeCatalogs.bat` after future authoring CSV changes.

### Evidence

- Root folder GUID `764a31a743b22f8468ef8ce3e253f371` is preserved in `Pakuri/Assets/CSVdata/authoring.meta`.
- Retained definition assets keep GUIDs `1280beeb031fbe549abb95b4c85ffd3a` and `5ca5bc924d237e749b25e8faaec9852b`.
- `PakuriCsvRuntimeSourceCatalog.asset` now serializes empty `MonsterSkillNodeFiles` and `MonsterSkillNodeParamFiles` arrays.
- Search found zero deleted direct-node TextAsset GUIDs in the regenerated source catalog.
- `SyncCsvRuntimeCatalogs.bat` now checks `Assets/CSVdata/authoring/monster/monsters.csv`.
- The batch wrapper reads `ProjectSettings/ProjectVersion.txt` before falling back to installed Editor discovery.
- Batch sync logged `Pakuri CSV runtime catalogs synced and validated from 'Assets/CSVdata/authoring' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Open-Editor validation logged `[AuthoringCsvMigrationValidation] PASS`, and the temporary hook plus `.meta` were removed.

### History

- 2026-07-17: Code Builder preserved retained asset identities, removed stale empty-node catalog references, and validated the authoring-to-runtime catalog bridge.

## Task: 2026-07-18 Unified Enemy Prefab Scene Bindings

### Task title

Move all Stage 1 Enemy prefab references into the shared NewRunScene Enemy binding array.

### Goals

- Keep one Scene serialization shape for Stage 1 and Stage 2 Enemy prefabs.
- Preserve existing Stage 1 prefab asset GUIDs.
- Add the missing Stage 1 archer prefab binding.

### Constraints

- Role Owner is Code Builder.
- No prefab asset content or `.meta` identity is changed.
- No CSV content is changed; `enemies.csv` is comparison authority only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and asset-reference validated.

### Next Actions

- User verifies all Stage 1/2 Enemy visuals instantiate correctly in `NewRunScene` Play Mode.

### Evidence

- `NewRunScene.unity` contains 16 `enemyId + prefab` entries matching all 16 authored Enemy IDs with no missing, extra, or duplicate binding.
- Seven existing Stage 1 serialized prefab GUIDs were moved without change from dedicated fields into `enemyPrefabBindings`.
- `stage1-archer` was added with existing `Pakuri/Assets/Prefab/Enemy/Stage1/Stage1_Achor.prefab` GUID `bffcd0db2ede5a34a9297596966f6697` and root file ID `6219452113495756200`.
- `git diff --check` passed for the two scripts and Scene; only line-ending normalization warnings were reported.
- Unity asset refresh and compilation reached idle with `ready_for_tools=true`.

### History

- 2026-07-18: Code Builder replaced the asymmetric Stage 1 dedicated Scene fields with the shared binding array already used by Stage 2.
