# DATA_BLACKBOARD

## Archived History

The pre-cleanup file, including completed and superseded data tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/DATA/DATA_BLACKBOARD.md`.

## Task: 2026-08-05 Spirit King Skill Runtime Data

### Task title

Connect the authored Spirit King skills to the existing graph, trigger and Definition generation path.

### Goals

- Keep `summon_units.csv` in the existing monster-shaped schema with `base_move_speed=0.5`, max health 1000, Physical primary attribute and all six defenses 50.
- Generate four `SingleSkillDefinition` skills and one `ZoneSkillDefinition` from `summon_units_skill.csv`.
- Reuse existing visual resource fields for Sein-C Master 2 and Eve-C/Eve-D effects.
- Author Densest targeting, three-cast bombardment, Zone pull and OnExpire follow-up in existing graph/trigger CSVs.

### Constraints

- Do not create a new summon skill family or summon-only Node/Trigger CSV.
- `spirit-king-dimensional-rift` is `AreaAttack`; pull is `0.2 unit/tick`, damage is zero and the existing Zone lifecycle emits the follow-up.
- C repeats twice after the first cast, each repeat reselects the current densest enemy position.
- CSV remains the source of skill values and visual resource paths; runtime code consumes generated Definitions.

### Role Owner

Code Builder

### Status

Phase 1 loading/graph implementation and Phase 2 Definition-driven skill ownership are complete; runtime consumption of pull/target selection remains in Phase 3.

### Next Actions

- Runtime Phase 3 consumes the generated `Densest`, `BattlefieldCenter` and `PullToCenterActionOp` values.
- Unity catalog import and focused Definition assertions remain to be run in the Unity environment.

### Evidence

- `summon_units.csv` uses `base_move_speed=0.5`; `summon_units_skill.csv` now authors visual reuse, A/B/C `Densest`, D/E `BattlefieldCenter` and D `AreaAttack`.
- Existing graph rows author C `RepeatPerTarget(2,0.35,1)`, D `PullToCenter(0.2)` and D `OnExpire -> ExecuteSkill(E)`; the D follow-up selects `Nearest` enemies at `EventCenter` so the expiry event does not require a null `EventTarget`.
- `GameDataCatalogBuilder.BuildSummons` now attaches summon-owned reactions to the generated active skill Definitions.
- `SkillGraphParser` and `CsvDataValidator` now accept summon-owned skills/triggers without adding a summon-specific graph/trigger file.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings; edited CSVs import structurally with uniform columns.
- `ArtifactSynergyManager` consumes generated `ArtifactSynergyEffectDefinition.OutcomeSkill` and `SpawnSummon` references; no synergy ID or skill ID switch was added.

### History

- 2026-08-05: Code Builder recorded the corrected Zone Rift, visual reuse, Densest re-selection, pull and follow-up contracts before implementation.
- 2026-08-05: Code Builder connected Spirit King rows and graph/trigger data to the shared summon Loading/Generation path; runtime pull and targeting execution remains deferred to the next phase.
- 2026-08-05: Code Builder corrected the D `OnExpire` target contract and added runtime nearest fallback plus no-live-enemy gating for automatic enemy-target skills.

## Task: 2026-08-05 Artifact Synergy Icon Data Binding

### Task title

Load the Spirit Contract HUD icon from `artifact_synergies.csv` through the existing catalog pipeline.

### Goals

- Add optional `Icon_Image` asset-path data to the synergy source schema.
- Carry the field through `CsvRowParser` -> `CsvAssetReferenceCollector` -> Definition Generation -> `ArtifactSynergyDefinition.Icon`.
- Keep the current single Spirit Contract HUD container display-only; no synergy effect execution is added.

### Constraints

- Use the authored asset `Assets/Image/UI/Artifact/ChatGPT Image 2026년 8월 5일 오후 03_39_55.png`.
- Keep other synergy icon cells blank until their assets are authored; do not invent paths.
- Reuse the existing Sprite asset catalog and runtime `LoadSprite` path.

### Role Owner

Code Builder

### Status

Implemented and statically verified. Unity MCP validation timed out after the code/data change; the tracked runtime catalog entry was confirmed by direct file inspection.

### Next Actions

- On the next responsive Unity refresh, run the existing `Pakuri/Validate CSV Source Data` menu item to regenerate/confirm the serialized catalog automatically.
- User verifies the icon in `InGameScene` Play Mode.

### Evidence

- `artifact_synergies.csv` now has 17 columns including `Icon_Image`; the type row is `asset_path`, Spirit Contract has the requested path and the other five rows are blank.
- `CsvRowParser.cs` reads `Icon_Image` into `ArtifactSynergyRow.IconPath`.
- `CsvAssetReferenceCollector.cs` adds each synergy icon path to the shared Sprite reference set.
- `GameDataCatalogBuilder.Artifacts.cs` assigns `ArtifactSynergyDefinition.Icon = LoadSprite(row.IconPath)`.
- `ArtifactDefinitions.cs` exposes `ArtifactSynergyDefinition.Icon` as a `Sprite`.
- The asset exists and its `.meta` GUID is `8b537b0e0f060644cb22f8d33a5bbf01`; `CsvRuntimeCatalog.asset` contains the corresponding path/GUID/first-sprite fileID entry.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings; CSV column-count, path-existence and diff checks passed.

### History

- 2026-08-05: Code Builder added the optional synergy icon field and completed source-model, validation/collection, Definition generation and runtime catalog wiring.

## Task: 2026-08-05 Boss Artifact Reward Data Contract Design

### Task title

Use StageDay boss classification and existing artifact Definitions for reward choices.

### Goals

- Enable artifact rewards for Stage 1/2 `Day5Midboss`, `Day10Midboss` and `Boss` rows through `artifact_choice_count`.
- Keep `StageReward.csv` `artifact_choice_count` as the choice-count switch; no new reward CSV or schema.
- Populate ArtifactPanel from `ArtifactDefinition` and `ArtifactSynergyDefinition`, not direct CSV reads.

### Constraints

- Reuse the existing StageDay, StageReward and loaded Definition contracts; no new reward CSV or schema.
- The first release draw pool is limited to the ten `spirit-contract` artifacts with implemented effects.
- `resonance-compass` has no authored `artifact_icon` path; its choice intentionally hides the missing Icon instead of inventing an asset.

### Role Owner

Code Builder

### Status

Implemented. Stage 1/2 Day5 Midboss, Day10 Midboss and Day11 Boss reward counts are three; normal rows remain zero. The Spirit Contract pool has 9/10 authored icons.

### Next Actions

- Author and assign an icon for `resonance-compass` when the source asset is available.
- User: verify rendered text and icon assignments in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/stage_flow/StageDay.csv` contains Boss at Stage 1 Day 11 and Stage 2 Day 11.
- Stage 1 and Stage 2 `StageReward.csv` already define `artifact_choice_count`; both midboss rows and boss rows currently use value 3.
- `Pakuri/Assets/Scripts/Loading/Generation/StageDefinitionBuilder.cs` already parses `artifact_choice_count` into `ArtifactChoiceCount`.
- `Pakuri/Assets/Scripts/Combat/Artifact/Definition/ArtifactDefinitions.cs` exposes artifact display name, synergy ID, description, and loaded Sprite icon plus synergy display name.
- UTF-8 `Import-Csv` inspection excluding the type row reported 50 artifacts, 9 nonempty icon paths, and 41 missing icon paths.
- Stage 1 and Stage 2 `StageReward.csv` use `artifact_choice_count=3` on Day5 Midboss, Day10 Midboss and Day11 Boss; normal and inactive elite rows remain zero.
- Runtime binding uses `ArtifactDefinition.DisplayName` -> `ArtifactName`, `Description` -> `Desc`, `Icon` -> `Icon`, and `ArtifactSynergyDefinition.DisplayName` -> `Summary`.
- Both StageReward files were reimported as Unity TextAssets, and focused catalog verification passed all six eligible reward IDs at count three.

### History

- 2026-08-05: Chose existing StageDay/StageReward and Definition contracts; rejected a new artifact-reward CSV as unnecessary.
- 2026-08-05: Code Builder normalized both StageReward files for Boss-only artifact choices and restricted runtime draws to the ten Spirit Contract Definitions.
- 2026-08-05: Designer rechecked the missing-button report against current Stage data and recorded the Day11-only eligibility boundary for user confirmation.
- 2026-08-05: User confirmed Midboss inclusion; Code Builder restored count three on all four Midboss rows and removed the redundant runtime combat-type gate.

## Task: 2026-08-05 Artifact and Synergy Runtime Reuse Design

### Task title

Design first-class artifact/synergy additional-effect Definitions on the existing authoring/runtime pipeline.

### Goals

- Keep the two Effect header CSV contracts under `Artifact/Effect` and reuse the existing passive graph-node/trigger authoring files for concrete effect behavior.
- Make Phase 1 the unparsed authoring of two Effect CSVs plus Spirit King unit and skill rows.
- Route CSV through Parsing, `CsvSourceModel`, Validation and Generation before runtime use.
- Reuse Choice-like Node/Trigger mechanics without converting effects into hidden passives or Choices.
- Limit the first runtime implementation to the ten Spirit Contract artifacts; defer Spirit Contract synergy execution and the Spirit King.
- Load authored artifact icon paths into `ArtifactDefinition.Icon` through the shared Sprite asset catalog.

### Constraints

- Phase 2 owns Loading and Definition code only; prefab, scene, Stage Manager and combat execution remain excluded.
- Artifact effects must generate `ArtifactEffectDefinition` or `ArtifactSynergyEffectDefinition`, not `PassiveSkillDefinition`.
- Do not add artifact-only `effect_nodes.csv` or `effect_triggers.csv`; use existing `skill_graph_nodes_passive.csv`, `passive_skill_triger.csv` and Node definition contracts with `effect_id` ownership.
- Every individual artifact effect uses passive `SkillModifier` or `PassiveTrigger` application; synergy effects may also execute or grant concrete skills.
- Do not invent Tracker details or unsupported Nodes/events.
- Spirit King spawn is `SpawnUnit` effect data and `SummonDefinition`, not a new SkillDefinition family.
- `summon_units.csv` must copy the existing `monsters.csv` columns; `summon_units_skill.csv` must copy the existing `skills_area_attack.csv` columns without speculative metadata columns.
- Do not invent icon paths for artifacts without an existing matching PNG.

### Role Owner

Designer for contract; Code Builder for Phase 1 and Phase 2 implementation.

### Status

Phase 1, Phase 2 and the ten-artifact Phase 3 data/runtime scope are complete. All Spirit Contract Effect Nodes/Reactions generate typed Definitions through the existing Effect-owner pipeline.

### Next Actions

- Keep future Effect additions in the existing `skill_graph_nodes_passive.csv` and `passive_skill_triger.csv` owner paths.
- Keep all `ArtifactSynergyEffectDefinition` execution, Spirit King runtime, the other 40 artifacts and other synergies deferred.
- Enforce no-duplicate artifact acquisition in the future acquisition flow; Phase 3 does not expand `ArtifactState` for it.

### Evidence

- `Pakuri/reference/4.run/artifact-synergy-runtime-design.md` defines `artifact_effects.csv` and `artifact_synergy_effects.csv` as first-class Definition headers with Node/Trigger owners and Generation-resolved outcome skills.
- The same design maps all 50 authored artifacts and five detailed synergies to `SkillModifier`, `PassiveTrigger`, `ExecuteSkill` or `GrantSkill`, naming existing Nodes and unsupported gaps.
- Existing monster authoring already separates base family CSVs, choices, triggers and graph nodes under `Pakuri/Assets/CSVdata/authoring/monster/skills`.
- `skill_node_definitions.csv` and `skill_node_definition_params.csv` define operation contracts, while `skill_graph_nodes_passive.csv` and `passive_skill_triger.csv` already own concrete Node/Trigger instances.
- `SkillNodeOwnerKind.Effect` now materializes ArtifactEffect-owned graph rows, and artifact-owned Trigger rows validate their effect source without requiring a monster skill.
- Existing Loading code already follows Parsing -> Validation -> Generation -> RuntimeCatalog; artifact source rows and Definitions must join that same path.
- `GameDataCatalog` now indexes Artifact, ArtifactEffect, Synergy and SynergyEffect Definitions; no runtime state or consumer uses them yet.
- `SkillTriggerEvent` has no reload-complete/heal-received event and `SkillTargetSelection` has no densest selector; no Summon runtime kind is required by the revised design.
- Current monster Validation requires A-E active and F-J passive slots, and `MenifestUI` uses `GameDataCatalog.GetMonsters()` as Manifest candidates; Phase 2 therefore generates a separate `SummonDefinition` and `GameDataCatalog.Summons` lookup.
- `authoring/summon/summon_units.csv` now contains the Spirit King row and `authoring/summon/skill/summon_units_skill.csv` contains its five skill rows using the inspected 22/33-column schemas.
- Existing runtime Generation maps `SingleAttack` to `SingleSkillDefinition` and `AreaAttack` to `ZoneSkillDefinition`; existing `RepeatPerTarget` supports Spirit Bombardment's initial cast plus two repeats.
- Phase 1 verification passed strict UTF-8 for all four files, exact 22/33-column reference-header matching, unique IDs, catalog foreign keys and required Spirit King values. Result: 52 artifact-effect rows covering 50 artifacts, 27 synergy-effect rows covering 20 detailed levels, one summon unit and five summon skills.
- Phase 2 added six `CsvRuntimeCatalog` sources, dedicated artifact/synergy/effect/summon source collections, foreign-key and summon-slot/runtime validation, typed Definition generation and RuntimeCatalog lookups.
- `GameDataCatalogBuilder` reuses the existing active-skill generator for `SummonDefinition`; generated Spirit King skills are four `SingleSkillDefinition` and one `ZoneSkillDefinition` without entering `GameDataCatalog.Monsters`.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` updated `CsvRuntimeCatalog.asset` with all six source references.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and 2 existing assembly-reference warnings.
- Artifact runtime ownership scripts are organized under `Pakuri/Assets/Scripts/Combat/Artifact`, and `ArtifactDefinitions.cs` is under `Combat/Artifact/Definition`; Loading parser, validator and generator files remain under `Loading`.
- Focused Unity EditMode test `ArtifactAndSummonCatalogBuildsResolvedDefinitions` passed 1/1 and reported 5 monsters, 8+8 enemies, 50 artifacts, 6 synergies and 1 summon. Full `SkillCatalogRuntimeTests` ran 17 tests: 15 passed; two Trigger baseline assertions failed (`ResolvedDefinition` null entries and missing expected Silence status). Phase 2 changed no Trigger/Node source or runtime script; these failures remain a separate verification gap.
- `artifacts.csv` now has a typed `artifact_icon` column: 50 rows preserved, 9 exact filename/ID matches populated, 41 unavailable icons blank, and 0 populated paths missing on disk.
- All 9 referenced PNG `.meta` files use `textureType: 8`; `CsvAssetReferenceCollector`, Generation and `ArtifactDefinition.Icon` reuse the shared Sprite catalog.
- Unity catalog sync serialized 9 matched Artifact Sprite paths; `elemental-prism.Icon` resolves and unmatched `resonance-compass.Icon` remains null.
- The source and foundation catalog now use `정령의 비약` / `spirit-elixir`; Tracker detail remains absent.
- Current CSV evidence identifies exactly ten Spirit Contract artifacts with eight `SkillModifier` effects and two `PassiveTrigger` effects; this is the revised first runtime scope.
- `artifact_effects.csv` now has 62 effect rows; dynamic Prism variants and split conditional/count effects remain independent generated Definitions.
- `artifact_effects.csv` has 64 rows including header/type rows, 9 columns, 2 typed repeat-rule rows and 5 typed Prism selection-rule rows; all rows pass uniform column-count validation.
- `GameDataCatalogBuilder` generates typed Nodes/Reactions on `ArtifactEffectDefinition`; all ten Spirit Contract artifacts now have concrete existing Node/Trigger data.
- Four changed CSVs pass strict parsed column-count checks; focused EditMode tests pass 3/3; solution build completes with 0 errors and the existing 2 warnings.
- Full `SkillCatalogRuntimeTests` result is 19/21 passed; only the previously recorded Trigger baseline assertions remain failing, while every artifact catalog/state/resolver/trigger test passes.
- Focused EditMode verification for the new Definition metadata and manager path passes 4/4; solution build completes with 0 errors, and manager search finds 0 former artifact/synergy ID constant references.

### History

- 2026-08-05: Designer inspected current catalogs, source authoring schemas and runtime lookups, then recorded the minimal binding and skill-reuse contract.
- 2026-08-05: User rejected the hidden-runtime-passive model; Designer replaced it with first-class additional-effect Definitions and restricted first implementation to Spirit Contract.
- 2026-08-05: Designer classified all individual artifacts as passive modifier/trigger effects, added concrete Node/path mapping, and changed Phase 1 to authoring both Effect CSVs.
- 2026-08-05: User replaced the Summon-skill plan; Designer added `SpawnUnit`/`spawn_monster_id`, removed Summon Skill Definitions, and retained existing Monster/Zone Definitions.
- 2026-08-05: Designer added Spirit King unit/skill authoring to Phase 1: HP 1000, Physical primary attribute, all defenses 50, four SingleAttack rows, one AreaAttack row, three-cast repeat routing, and the Dimensional Collapse follow-up contract.
- 2026-08-05: Code Builder completed Phase 1 CSV authoring and non-runtime structural validation; no parser, C#, Node/Trigger, prefab or scene was added. Unity auto-import generated four standard `TextScriptImporter` `.meta` files for the authored CSV assets.
- 2026-08-05: Code Builder completed Phase 2 Parsing, SourceModel, Validation, Definition Generation, RuntimeCatalog registration, asset sync and focused EditMode verification using a separate `SummonDefinition`.
- 2026-08-05: Code Builder added `artifact_icon`, mapped the 9 available ID-matched images, wired the field through Parsing/asset collection/Generation, synchronized Unity RuntimeCatalog and passed focused verification.
- 2026-08-05: User moved the first runtime target to the ten Spirit Contract artifacts; Designer moved state/manager skeleton and count-only synergy logging into Phase 3 and deferred all synergy effect execution.
- 2026-08-05: User rejected artifact-only Node/Trigger CSVs; Designer changed Phase 3 to independent ArtifactState ownership plus existing passive graph-node/trigger authoring reuse, without creating `PassiveSkillDefinition` data.
- 2026-08-05: Code Builder completed Effect-owner pipeline integration and authored only the confirmed Spirit Contract modifier nodes, leaving decision-dependent data absent rather than guessed.
- 2026-08-05: Code Builder authored the resolved Prism, Black Candlestick, Spirit Elixir, Rift Gem, Elemental Codex and Resonance Compass data and verified generated Definitions plus dynamic Stage distribution.
- 2026-08-05: Code Builder moved the three artifact runtime state/manager scripts and Unity `.meta` files to `Combat/Artifact`, preserving GUIDs; generated Definition/source-model files were not moved because they belong to the existing Loading pipeline.
- 2026-08-05: Code Builder organized `ArtifactDefinitions.cs` under `Combat/Artifact/Definition` while keeping CSV parser, validator and generator code in `Loading`.
- 2026-08-05: Code Builder added `repeat_rule` and `selection_rule` to the Artifact Effect pipeline, removed manager ID constants, and verified 4/4 focused tests plus 0-error solution build.

## Task: 2026-08-05 Artifact Synergy Foundation CSVs

### Task title

Create the initial artifact synergy and artifact catalogs without runtime parsing.

### Goals

- Create a six-row synergy catalog from `artifact-synergy-list.md`.
- Create an artifact catalog containing every artifact currently detailed by the source document.
- Preserve stable IDs, UI text, fixed 2/4/6/8 thresholds and artifact-to-synergy references.

### Constraints

- Do not add CSV parsing, runtime code or Unity `.meta` files.
- Do not invent the missing Tracker detail section or Tracker artifact list.
- Store both CSV files as UTF-8.

### Role Owner

Code Builder.

### Status

Complete. Foundation CSVs created and structurally verified; runtime parsing remains intentionally absent, and unused `sort_order` columns have been removed.

### Next Actions

- Author Tracker descriptions, four level effects and artifacts in the source document before filling the blank Tracker data.
- Add parsing only through a future explicit implementation request.

### Evidence

- `Pakuri/Assets/CSVdata/Artifact/artifact_synergies.csv` contains six synergy rows with unique IDs and 2/4/6/8 thresholds.
- `Pakuri/Assets/CSVdata/Artifact/artifacts.csv` contains 50 unique artifacts: ten each for Spirit Contract, Executioner, Chosen One, Sentinel and Artillery.
- Strict UTF-8 decoding and PowerShell `Import-Csv` validation passed; all 50 artifact `synergy_id` values reference an existing synergy.
- Tracker summary and common thresholds come from the source summary, while its unavailable detailed description, level effects and artifacts remain blank/absent.
- Neither foundation CSV contains `sort_order`; no Artifact parser or code consumer exists that requires authored ordering metadata.
- The Spirit Contract catalog row now uses `spirit-elixir`, `정령의 비약`, and the revised all-damage/resistance-down description from the source document.

### History

- 2026-08-05: Code Builder created and validated the two non-parsed foundation CSV catalogs from the inspected artifact synergy reference.
- 2026-08-05: Code Builder removed the unused `sort_order` column from both catalogs and revalidated all source text, IDs, references and thresholds.
- 2026-08-05: Code Builder renamed the CSVs to `artifacts.csv` and `artifact_synergies.csv` and moved their existing Unity `.meta` files without changing hashes or GUIDs.
- 2026-08-05: Designer synchronized the requested `정령의 비약` wording and stable English ID into the unparsed foundation catalog.

## Task: 2026-08-03 Remove SingleSkill Internal Delay Data Contract

### Task title

Remove the unused SingleSkill `DamageDelaySeconds` runtime contract while preserving projectile arrival delay data.

### Goals

- Remove `SingleSkillDefinition.DamageDelaySeconds` and `SkillExecutionState.PreparedDamageDelay`.
- Stop copying source delay data into SingleSkill definitions during Generation.
- Keep `ActiveSkillBuildData.DamageDelaySeconds` for projectile arrival generation and CSV validation.

### Constraints

- Preserve `skills_projectile.csv` and the authored `sein-c` value `0.8`.
- Preserve generated `sein-c@arrival` creation and the existing runtime execution route.
- Do not change CSV schema or unrelated loading/UI behavior.

### Role Owner

Code Builder.

### Status

Implementation complete. Static data/code checks passed. Full C# build is currently blocked by 3 out-of-scope UI errors.

### Next Actions

- Unity catalog refresh and Play Mode verification remain user-owned.

### Evidence

- `GameDataCatalogBuilder.Skills.cs` still maps source `DamageDelaySeconds` to Projectile `ArrivalDelaySeconds` and builds `sein-c@arrival` when the value is positive, but no longer assigns it to `SingleSkillDefinition`.
- `SingleSkillDefinition.cs`, `SkillExecutionState.cs` and `SkillExecution.cs` no longer contain the removed SingleSkill delay members.
- `skills_projectile.csv` has the only positive authored `damage_delay_seconds` row: `sein-c`, runtime kind `CooldownProjectile`, value `0.8`.
- `skill_graph_nodes_projectile.csv` keeps `sein-c-trait-4` `DamageDelayMultiplier=0.6`; this continues to modify projectile arrival delay, not SingleSkill internal damage delay.
- `rg` found no remaining `PreparedDamageDelay`, `SingleSkillDefinition.DamageDelaySeconds`, or SingleSkill delayed-application method references.
- The full build errors are limited to modified `MonsterPanelUI.cs:146`, `DebugUI.cs:665`, and `DebugUI.cs:686`; no changed Loading or Combat file produced a reported compiler error.

### History

- 2026-08-03: Code Builder removed the unused SingleSkill delay fields and preserved projectile arrival delay as the separate generated SingleSkill flow.

## Task: 2026-08-03 Sein-C Projectile Arrival SingleSkill Migration

### Task title

Replace the delayed projectile impact-area path with target-point arrival and the existing `SingleSkill` execution path.

### Goals

- Let Sein-C fly to its cast-time target point and preserve collision-triggered trait effects.
- Execute the generated arrival `SingleSkill` after `damage_delay_seconds` at the target point.
- Remove `ProjectileSkillActor`'s direct impact-area target collection and execution path.

### Constraints

- Preserve existing CSV schema, values, authored triggers and unrelated user changes.
- Reuse `TryExecuteReaction` and the existing `SingleSkill` runtime; do not add a second area-damage executor.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. Static references and `Assembly-CSharp.csproj` build verified.

### Next Actions

- In Unity Play Mode, verify Sein-C collision trait damage, target-point delay, arrival damage, and `sein-c-master-1` OnExpire behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs` now uses `BeginArrivalDelay` and `ExecuteArrivalSkill`; `ApplyImpactAreaTargets`, `ArmImpact`, and the old impact fields are absent.
- `Pakuri/Assets/Scripts/Loading/Generation/GameDataCatalogBuilder.Skills.cs` creates a generated arrival `SingleSkillDefinition` from the projectile source data when `damage_delay_seconds > 0`.
- `Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillExecution.cs` stores the cast-time projectile target point and passes arrival data through `SkillExecutionState` and `ProjectileSkillExecutor`.
- `Import-Csv Pakuri/Assets/CSVdata/authoring/monster/skills/base/projectile/skills_projectile.csv` confirms `sein-c`: `radius=1.8`, `damage_delay_seconds=0.8`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` completed with 0 errors and 2 existing assembly-reference warnings.
- `rg` over Combat skill and Generation scripts found 0 old `ApplyImpactAreaTargets`, `ArmImpact`, `StopOnFirstHit`, `PreparedImpact`, and `impactArmed` references.

### History

- 2026-08-03: Code Builder migrated delayed projectile impact execution to target-point arrival plus generated `SingleSkill`; kept OnHit trigger publication active when base contact damage is disabled.

## Task: 2026-08-03 Monster Skill Icon Asset Copy

### Task title

Create A-E skill icon folders for the five monsters and copy each available `runtime_visual_sprite_path` PNG as `<slot>_Icon.png`.

### Goals

- Create `Pakuri/Assets/Image/Monster/Icon/Skill/<monster>/<A-E>/` for `ariel`, `eve`, `rin`, `sein`, and `vega`.
- Copy the 23 available A-E source PNGs without changing CSV authoring data.
- Keep `rin/D` and `rin/E` folders present while their CSV sprite paths remain empty.

### Constraints

- Copy/rename PNG files only; do not edit skill CSVs or add runtime code.
- Use the exact `runtime_visual_sprite_path` values from `Pakuri/Assets/CSVdata/authoring/monster/skills/base`.
- Do not overwrite an existing destination with different content.

### Role Owner

Code Builder.

### Status

23 of 25 requested icons copied and SHA-256 verified. The two Rin source paths are unavailable because `rin-d` and `rin-e` have empty `runtime_visual_sprite_path` fields.

### Next Actions

- Obtain the intended source PNG paths for `rin-d` and `rin-e`, then copy them to `rin/D/D_Icon.png` and `rin/E/E_Icon.png`.
- Let Unity generate/import child-folder and PNG `.meta` files if they are not created automatically by the editor.

### Evidence

- UTF-8 `Import-Csv` read found 25 A-E rows across six base skill CSVs and five monster IDs: `ariel`, `eve`, `rin`, `sein`, `vega`.
- Validation found 25 slot folders, 23 PNGs and 23 source/destination SHA-256 matches.
- `git status --short -- Pakuri/Assets/CSVdata/authoring/monster/skills/base` returned `NONE`.

### History

- 2026-08-03: Code Builder created the five-monster/A-E folder structure and copied 23 validated PNGs; no CSV files were changed.

## Task: 2026-08-03 Monster and Skill Icon CSV References

### Task title

Populate `MonsterIconImage` and add `SkillIconImage` to the monster skill authoring CSVs so generated runtime data resolves the icon sprites.

### Goals

- Assign all five `MonsterIconImage` values from `Assets/Image/Monster/Icon/Monster`.
- Add `SkillIconImage` plus its `asset_path` type row to all six monster base skill CSVs.
- Populate A-E skill rows from the existing `Icon/Skill` PNGs and include the generated paths in the runtime catalog.

### Constraints

- Preserve the existing CSV row values and use only verified project asset paths.
- Keep F-J passive `SkillIconImage` cells empty because no F-J icon folders/assets exist in the inspected workspace.
- Reuse the existing `SkillRow.SkillIconPath`, `GameDataCatalogBuilder.LoadSprite`, `SkillDefinition.Icon`, and asset collector flow; do not add duplicate runtime icon fields.

### Role Owner

Code Builder.

### Status

Complete. Monster and active-skill icon paths are authored, parsed and generated into `CsvRuntimeCatalog.asset`.

### Next Actions

- If passive F-J icons are required later, create those assets first, then populate their currently empty `SkillIconImage` cells.

### Evidence

- `monsters.csv` now maps `ariel`, `eve`, `rin`, `sein`, and `vega` to five existing `Monster/*_Icon.png` files.
- Six base skill CSV headers now contain `SkillIconImage`; all 25 A-E rows contain existing icon paths. Passive F-J rows remain empty by design.
- `CsvRowParser.ParseSkillRow` now reads `SkillIconImage` and falls back to legacy `skill_icon_path`; existing Generation already loads the field into `SkillDefinition.Icon`.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and 2 pre-existing assembly-reference warnings.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed; `CsvRuntimeCatalog.asset` contains five monster icon paths and 25 active skill icon paths. Unity console reported a runtime catalog with 5 monsters, 8 stage-one enemies and 8 stage-two enemies.
- All 25 target PNG `.meta` files were verified with `textureType: 8` (Sprite).

### History

- 2026-08-03: Code Builder added the CSV field, parser compatibility, generated runtime catalog references, and ran compile/path verification without adding a new Definition type.

## Task: 2026-07-31 Resolved Skill Outcome Materialization

### Task title

Materialize Trigger skill outcomes as concrete family Definitions during Generation.

### Goals

- Keep `SkillCastEffect` as a small resolved execution link instead of a second raw payload model.
- Generate Single, Zone and Buff Definitions once, then route learned and generated outcomes through the common runtime path.
- Preserve authored CSV values, targeting, visual, status, shield, timing and recast metadata.

### Constraints

- Do not change the CSV schema or add a runtime kind, Executor, Actor base class, catalog lookup layer or new Implementation script.
- Keep cooldown refund, reload reduction and status-duration extension as typed non-spatial commands.
- Auxiliary generated Definitions must not enter `UnitSkills` learned active/passive lists.
- Use the existing Generation builders, `UnitSkills.FindByDefinition`, `SkillExecution` and family Executors.

### Role Owner

Code Builder for implementation; Code Reviewer ran once after implementation by explicit user request.

### Status

Complete. Phase records: `05e5b22`, `22e8516`, `3075a5d`, `55ca337`, `dfa7d53`; implementation `5213b14`; recast guard fix `b7037d1`.

### Next Actions

- User performs Unity Play Mode and gameplay parity verification.
- Reopen this task only if runtime evidence identifies a data-generation regression.

### Evidence

- `GameDataCatalogBuilder.Nodes.cs` now writes concrete Single/Zone/Buff Definition references into `SkillCastEffect`.
- `SkillReaction.TargetSkillId` and raw `SkillCastEffect` damage/status/shield/targeting payload fields have no runtime readers.
- Core and Editor project builds both ended with `빌드했습니다.`; static legacy-contract and direct-damage boundary searches returned no output.
- Unity EditMode batchmode was blocked because another Unity instance had this project open.

### History

- 2026-07-31: Code Builder completed Generation materialization, common execution routing, Actor hit ownership and raw contract cleanup with per-Phase commits.
- 2026-07-31: Code Reviewer found the resolved Recast path did not enforce `MaxGeneration`; Code Builder restored the guard in `b7037d1`.

## Task: 2026-07-28 Skill Trigger / Node Data Contract Design

### Task title

Replace kind-branched graph authoring with Trigger-owned and owner-keyed Nodes.

### Goals

- Remove `graph_kind` from all six `skill_graph_nodes_*.csv`.
- Add no replacement grouping column or intermediate grouping type.
- Move Trigger payload fields into Trigger-owned Node data while Trigger rows retain activation rules.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve all current IDs, values, asset paths, ordering, gates, and generated catalog behavior during migration.
- Keep the legacy graph reader until converted CSV and runtime parity pass.
- No active CSV was changed in this design task.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Superseded by the 2026-07-29 Trigger Final Outcome Generation Design.

### Next Actions

- No action. The preserved historical handoff is under `boards/ARCHIVE/`.
- Current work follows `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`.

### Evidence

- `SkillGraphParser` currently branches graph kinds and rewrites Effect rows to generated Effect ownership.
- `GameDataCatalogBuilder` separately materializes ordinary Nodes and Effect definitions.
- Active graph authoring contains 508 Effect rows and 256 ordinary modifier rows.
- The handoff removes `graph_kind`, rejects replacement grouping IDs, defines owner-keyed Nodes, expands Trigger events, and specifies parser/validator/catalog/compiler migration.

### History

- 2026-07-28: User requested removal of `graph_kind`, rejection of the intermediate grouping term, and Trigger-based Node activation.
- 2026-07-28: Designer recorded the replacement data contract without editing CSV or runtime catalogs.
- 2026-07-28: Code Builder archived older DATA task history and retained this as the only active DATA task.
- 2026-07-29: User superseded runtime Trigger Node dispatch with final outcome generation and existing Executor reuse.

## Task: 2026-07-29 Trigger Visual Duration Data Repair

### Task title

Restore explicit one-second lifetime Nodes for standalone Trigger visuals.

### Goals

- Repair ten Trigger-owned Node collections whose `ShowVisual` rows had no `SetDuration`.
- Keep visual lifetime explicit in authoring data.

### Constraints

- No runtime fallback and no validator change.
- Preserve the 19-column Node CSV contract and contiguous owner-local `node_order`.
- Preserve all existing values and add only the missing duration rows.

### Role Owner

Code Builder

### Status

Complete except user-owned Play Mode verification.

### Next Actions

- User verifies one-second visual removal for representative OnExpire, OnHit, OnKill, OnOutgoingDamage, OnShieldExpire, and last-projectile events.

### Evidence

- Five graph CSV files received ten total `SetDuration=1` rows; the line-attack graph required no change.
- All six graph files retain a 19-column width for every header and row.
- Each repaired owner has exactly one positive duration Node, and the standalone non-positive Trigger visual count is zero.
- Unity CSV source validation completed without errors and the runtime catalog loaded 5/8/8 definitions.

### History

- 2026-07-29: User required explicit data duration and prohibited a runtime zero-duration fallback.
- 2026-07-29: Code Builder restored the ten missing lifetime Nodes from reference intent and pre-migration one-second behavior.

## Task: 2026-07-29 CSV Loading Pipeline Responsibility Refactor

### Task title

Reorganize CSV loading into one ordered pipeline with four responsibility folders.

### Goals

- Implement the approved Parsing, Validation, Generation, and RuntimeCatalog structure.
- Keep one parsed `SourceModel`, one semantic validation pass, one catalog build, and one lookup rebuild.
- Remove duplicate ownership and implicit static builder dependencies.

### Constraints

- Preserve current CSV, serialized asset, runtime catalog, public API, ordering, and gameplay behavior.
- Preserve existing `.meta` GUIDs and the runtime Resources path.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and non-Play-Mode verification complete.

### Next Actions

- User verifies representative gameplay flows in Unity Play Mode.

### Evidence

- The approved handoff records current file ownership, target paths, stage contracts, the single-validation rule, and compatibility gates.
- Baseline runtime and editor C# builds completed with zero errors before implementation.
- Loading now has explicit Parsing, Validation, Generation, and RuntimeCatalog folders; combat skill compilation moved to `Combat/Skills/Compilation`.
- Static search found one semantic-validation call, one catalog-build call, and one lookup-rebuild call in the ordered loader path.
- Static search found zero references to the removed `runtimeCsvCatalog` loader state.
- All moved scripts retain their original GUIDs, and all new scripts have `.meta` files.
- `Assembly-CSharp.csproj` built with zero errors; Unity compiled without project errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.

### History

- 2026-07-29: User selected Code Builder, required the handoff MD first, and authorized implementation from that MD.
- 2026-07-29: User prohibited unnecessary duplicate structure and repeat validation of an already validated source model.
- 2026-07-29: Code Builder completed the handoff implementation and all available non-Play-Mode checks.

## Task: 2026-07-29 Ponytail Loading Pipeline Simplification

### Task title

Delete dead CSV-loading code and merge duplicate lookup and handler ownership.

### Goals

- Keep the Parsing -> Validation -> Generation -> RuntimeCatalog pipeline behavior.
- Delete unused parser, DTO, validator, builder, and skill-handler metadata.
- Merge runtime lookup storage into `GameDataCatalog`.

### Constraints

- Ponytail leads the implementation; existing markdown is reference material only.
- Preserve active CSV contracts, serialized fields, public lookup APIs, and gameplay behavior.
- Preserve unrelated pre-existing working-tree changes.

### Role Owner

Code Builder, ponytail-led

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative CSV loading and skill execution in Unity Play Mode.

### Evidence

- `Loading` changed from 13 C# files and 7,084 lines to 12 C# files and 5,718 lines: net reduction 1,366 lines.
- `GameDataLookup.cs` and its `.meta` were removed; lookup registration and queries now live in `GameDataCatalog.cs`.
- Static search found zero remaining removed-symbol or block-comment matches and retained the single ordered validation, build, and lookup-rebuild calls.
- Every remaining Loading C# file has a `.meta` file.
- Runtime and Editor `dotnet build` checks completed with zero errors; the Unity EditMode test passed 1/1.
- Unity finished script compilation idle and ready with zero `Assets/Scripts/Loading` console errors; one separate MCP package transport error was present.

### History

- 2026-07-29: User assigned Code Builder and required ponytail-led deletion, consolidation, and a final net-line-reduction report.
- 2026-07-29: Code Builder removed dead data and helpers, deleted duplicate handler metadata, merged lookup ownership, and completed static, build, EditMode, and Unity console checks.

## Task: 2026-07-29 Final Skill Catalog Generation Design

### Task title

Make Loading Generation produce final typed skill data once.

### Goals

- Make `GameDataCatalogBuilder` directly create final active, passive, Choice, Trigger, and Node data.
- Parse Node and Trigger enum/list/condition authoring strings into final typed values exactly once in Generation.
- Make `GameDataCatalog` index final data instead of Source Definition wrappers.
- Prevent repeated validation, Definition compilation, Trigger compilation, and Choice Node parsing.

### Constraints

- Keep the existing Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Keep exactly one semantic validation, one build, and one lookup rebuild.
- Preserve CSV schemas, values, IDs, ordering, asset paths, and runtime behavior.
- Avoid duplicate handler-support lists between Validator and Builder.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder implementation and available non-Play-Mode verification complete. Phases 1-6 complete.

### Next Actions

- User verifies representative CSV-loaded active, passive, enhancement, master, Trigger, and enemy skill behavior in Unity Play Mode.

### Evidence

- `GameDataLoader.BuildValidatedRuntimeCatalog` currently calls validation, catalog build, and lookup rebuild once each.
- `GameDataCatalogBuilder` currently stops at Source Definition and string-param Node Definition creation.
- Combat compiler scripts perform a second static conversion during unit state rebuild or first Choice use.
- `SkillNodeExecutor` and `SkillTrigger` still parse authored scope, policy, condition, status, runtime-kind, Choice, attribute, event-skill, and event-source values during execution.
- Final Loading and Combat contracts are specified in `boards/COMBAT/SKILL_DIRECT_CATALOG_RUNTIME_HANDOFF.md`.
- Phase 1 baseline `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` builds completed with zero errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; the EditMode test job succeeded.
- Phase 2 added the final typed contracts that Generation will populate directly: final Choice Nodes, Node target IDs, typed status conditions, typed Trigger lists, and event source scope.
- Phase 3 Generation now produces and indexes final active, passive, enemy, Choice, Trigger, and Node data once.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies through the final catalog path.
- Phase 4 runtime consumers use final Choice Nodes, typed Trigger arrays/scope, and final SkillDefinition lookup values directly.
- Runtime Execution/Trigger/StatusRules search found zero authored `Split`, `Enum.Parse`, or `TryParse` calls.
- Runtime and Editor builds completed with zero errors; Unity CSV validation retained 5 monsters and 8/8 enemies.
- Phase 5 integrated final skill/Choice/Node creation into `GameDataCatalogBuilder` partials and removed all compiler/mapper symbols.
- `UnitSkills` owns learned-ID/Choice application; `StatusRuntimeCompiler.CompileTriggers` was deleted as dead code.
- All 18 moved script GUID pairs matched; Runtime/Editor builds and Unity CSV validation passed.
- Phase 6 confined temporary build contracts to Loading/Generation and removed duplicate public Source/Definition contracts from Combat.
- Removed-symbol, runtime parsing, and Generation-outside Definition-mutation searches all returned zero.
- EditMode target-filter/reference-reuse tests passed 2/2; solution build and Unity script compilation completed with zero errors; CSV validation retained 5/8/8.
- Whole-task C# diff is 909 additions and 1,069 deletions: net reduction 160 lines.

### History

- 2026-07-29: User approved direct use of final authored skill data and requested a Code Builder-ready design.
- 2026-07-29: Designer recorded the cross-domain Loading/Combat handoff without changing runtime code or CSV.
- 2026-07-29: Designer updated the Generation contract so encoded authoring strings are converted once and final runtime consumers receive enum/array values.
- 2026-07-29: Code Builder completed Phase 1 baseline protection before changing the final data contracts.
- 2026-07-29: Code Builder completed Phase 2 final typed contracts with the current compiler retained only as an intermediate compatibility path.
- 2026-07-29: Code Builder completed Phase 3 final catalog generation and final-type RuntimeCatalog indexing.
- 2026-07-29: Code Builder completed Phase 4 final catalog direct runtime consumption.
- 2026-07-29: Code Builder completed Phase 5 Generation ownership integration and Combat skill folder migration.
- 2026-07-29: Code Builder completed Phase 6 temporary-contract cleanup and full non-Play-Mode verification.

## Task: 2026-07-29 Trigger Final Outcome Generation Design

### Task title

Generate final triggered skill Definitions or typed state commands once.

### Goals

- Convert Trigger-owned authored Nodes into final concrete `SkillDefinition` references or typed non-skill commands in Generation.
- Stop building runtime `SkillNode[]` payloads for Trigger execution.
- Reuse existing catalog Definitions for the four current `ExecuteSkill` mappings.
- Preserve Choice/base modifier Node generation.

### Constraints

- Keep the current Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Do not add a new CSV schema or C# script unless inspected code proves it necessary.
- Preserve IDs, values, ordering, asset paths, dynamic event-value semantics, and one validation/build/lookup flow.
- Do not silently activate the 81 current modifier-only owners without owner-level evidence.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder Phase 1-6 complete. Final catalog and non-Play-Mode verification passed.

### Next Actions

- User verifies representative Trigger behavior in Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- No Trigger owner currently combines two delivery kinds.
- No Trigger owner currently combines delivery and a non-skill state command.
- Current delivery shapes map to existing Single, Zone, Buff, and Shield Definition families.
- Event-derived damage uses shield-applied, shield-absorbed, or event-applied damage snapshots.
- The full Generation mapping and validation rules are recorded in the handoff.
- Phase 1 confirmed 158 Trigger rows and 606 Trigger-owned Node rows with runtime/editor build error 0.
- Phase 2 generated 55 final Definitions and 22 typed commands while retaining 81 current no-action owners.
- Unity EditMode catalog verification passed 1/1 and both C# builds completed with error 0.
- Phase 3 consumes 55 final Definitions through the existing family dispatch without catalog registration of hidden Definitions.
- Runtime/editor builds completed with error 0; `SkillCatalogRuntimeTests` passed 3/3.
- Phase 4 removes `SkillTriggerDefinition.Nodes`; Generation now stores only the final Definition or typed command on each Trigger.
- The generated 22 commands are verified as recast 1, cooldown 14, reload 6, and status-duration 1; Unity EditMode tests pass 3/3.
- Phase 5 deletes the runtime Node executor and Trigger-only public operation/mapping contracts; status mutation assembly remains private to Generation.
- Runtime/editor builds remain error 0 and final-outcome catalog tests pass 3/3 after deletion.
- Final static searches return zero deleted symbol, Trigger runtime Node, and runtime consumer authored-parse hits.
- Solution build error 0, Unity Console error/warning 0, full EditMode 3/3, CSV validation catalog 5/8/8.
- Git C# diff from the Phase 1 baseline is net -968 lines in `Combat/Skills` and net -443 lines across production `Assets/Scripts`.

### History

- 2026-07-29: User selected existing Executor reuse instead of runtime Trigger Node effect dispatch.
- 2026-07-29: Designer recorded the corresponding final Generation outcome contract.
- 2026-07-29: User approved Code Builder implementation; Phase 1 fixed the current owner and build baseline.
- 2026-07-29: Code Builder completed Phase 2 final outcome Generation and focused catalog verification.
- 2026-07-29: Code Builder completed Phase 3 runtime consumption with source snapshot, lifecycle, target, and dynamic-value policies.
- 2026-07-29: Code Builder completed Phase 4 typed command consumption and removed the runtime Trigger Node payload.
- 2026-07-29: Code Builder completed Phase 5 Trigger executor/operation deletion and confined remaining authored mutation assembly to Generation.
- 2026-07-29: Code Builder completed Phase 6 final static/build/Unity/CSV verification.

## Task: 2026-07-29 Final Status Catalog Generation

### Task title

Generate final status runtime data once and index it in `GameDataCatalog`.

### Goals

- Keep status authoring parsing in `Loading/Parsing`.
- Build each `StatusRuntimeData` from its validated `StatusEffectDefinition` in Generation.
- Index final status runtime data by `StatusEffectKind` during `GameDataCatalog.RebuildLookup`.
- Remove Combat-side status compilation and lookup helpers.

### Constraints

- Preserve the existing Parsing -> Validation -> Generation -> RuntimeCatalog order.
- Keep one validation, one catalog build, and one lookup rebuild.
- Preserve CSV schema and values.
- Reuse existing `StatusRuntimeData`, `StatusEffectDefinition`, and `GameDataCatalog` types without a replacement compiler layer.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative CSV-authored status behavior in Unity Play Mode.

### Evidence

- `GameDataCatalogBuilder.BuildStatusEffects` now assigns one generated `RuntimeData` value to every status definition.
- Skill and Trigger Generation clone the generated status template before applying owner-specific overrides.
- `GameDataCatalog.GetStatusRuntimeData(StatusEffectKind)` returns the indexed final runtime reference.
- `StatusValueParser` is internal and all of its callers are under `Loading`.
- `StatusRuntimeCompiler` and `StatusEffectLookup` searches return zero references.
- EditMode verification passes 4/4 and asserts every status definition owns non-null generated runtime data reused by RuntimeCatalog.
- Solution build completes with zero errors; final Unity Console contains zero errors/warnings.

### History

- 2026-07-29: User approved aligning status data flow with the final skill catalog structure.
- 2026-07-29: Code Builder moved parse-only functions to `Loading/Parsing/StatusValueParser.cs` and absorbed runtime-data construction into Generation.
- 2026-07-29: Code Builder completed RuntimeCatalog indexing, Combat direct use, and non-Play-Mode verification.

## Task: 2026-07-30 Enemy Passive Shared Data Contract

### Task title

Generate and register Enemy passives as shared `PassiveSkillDefinition` data.

### Goals

- Replace `EnemyPassiveDefinition` and `EnemyPassiveModifierKind` with the shared passive definition contract.
- Preserve the existing Enemy passive CSV shape, IDs, values, and attribute rules.
- Register generated Enemy passives in the common passive lookup.

### Constraints

- Keep the existing five-column Enemy passive CSV and all 16 authored rows.
- Preserve one semantic validation, one catalog build, and one lookup rebuild.
- Do not alter Monster passive CSV or reward contracts.
- Store the edited CSV as UTF-8.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative Enemy passive behavior in Play Mode.

### Evidence

- Enemy passive CSV type metadata now names shared `PassiveModifierKind`; all authored rows and values are unchanged.
- `CsvRowParser` and `CsvDataValidator` parse and validate the shared enum while retaining existing attribute and positive-value rules.
- `GameDataCatalogBuilder` creates `PassiveSkillDefinition` directly for Enemy passive rows.
- `GameDataCatalog.RegisterEnemies` registers each generated Enemy passive in the common passive lookup.
- Unity catalog verification loads 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- `EnemyCatalogBuildsSharedLearnedPassives` verifies all 16 Enemy passives are generated, registered, learned, and rebuilt through the common runtime.
- Full Unity EditMode tests pass 9/9; Runtime and Editor C# builds complete with zero errors.

### History

- 2026-07-30: User approved replacing the separate Enemy passive data/runtime with shared learned passives assigned at spawn.
- 2026-07-30: Code Builder migrated parsing, validation, generation, registration, runtime initialization, and verification.

## Task: 2026-07-30 Skill Definition Family Consolidation

### Task title

Generate final Single and Buff family Definitions without compatibility subclasses.

### Goals

- Generate Chain follow-up as a final hidden Trigger `SingleSkillDefinition`.
- Generate Status, Heal, Shield, and Charge as one final `BuffSkillDefinition`.
- Keep current CSV schemas and authored tuning values.
- Remove obsolete subclass-specific Generation branches and writes.

### Constraints

- Follow `boards/COMBAT/SKILL_DEFINITION_FAMILY_CONSOLIDATION_HANDOFF.md`.
- Preserve one validation, one catalog build, and one lookup rebuild.
- Keep existing Chain fields as the tuning source; do not add duplicate Trigger CSV columns.
- Preserve all current IDs, values, paths, and ordering.

### Role Owner

Code Builder

### Status

Code implementation and non-Play-Mode verification complete.

### Next Actions

- User Play Mode verification of generated skill behavior.

### Evidence

- `ChainLightning` already carries multiplier, delay, radius, and primary-exclusion values in the base skill CSV.
- `OpeningCharge` is already authored as Buff and already has an explicit CombatStart Trigger.
- Generation currently creates the four subclasses that this task removes.
- Generation now creates only final family Definitions; searches return zero removed subclass symbols.
- ChainLightning uses its existing CSV chain values to generate a hidden Trigger Single without schema changes.
- OpeningCharge, Heal, Shield, and Status profiles generate one `BuffSkillDefinition` distinguished by `BuffEffectKind`.
- No CSV file or schema was changed.
- Focused generated-family test passed 1/1; full Unity EditMode tests passed 10/10.

### History

- 2026-07-30: User approved final family Generation and requested implementation after a written handoff.
- 2026-07-30: Code Builder completed final family Generation and catalog verification.

## Task: 2026-07-31 Trigger Reaction Generation Consolidation Design

### Task title

Generate Trigger conditions as existing skill/passive/Choice reaction Nodes instead of runtime Trigger Definitions.

### Goals

- Keep current Trigger CSV and Trigger-owned graph CSV as the first migration authoring source.
- Attach generated reaction conditions and execution-data adjustments to existing Skill, Passive, and Choice Node ownership.
- Stop generating hidden Trigger SkillDefinitions after the common runtime path is verified.

### Constraints

- Add no authoring schema or C# script in the first migration.
- Preserve the single Parsing -> Validation -> Generation -> RuntimeCatalog flow.
- Preserve IDs, values, ordering, asset paths, and current working outcomes.
- Restore the approved 17 event outcomes and 64 normal cast outcomes without mixing their ownership.

### Role Owner

Code Builder.

### Status

User approved Generation/runtime implementation. Phases 1-8 complete. Code Reviewer corrections 1-4 are implemented without schema changes and final PASS.

### Next Actions

- Re-run Code Reviewer and correct findings until approval.

### Evidence

- Full design: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`.
- Current `GameDataCatalogBuilder` builds `SkillTriggerDefinition`, hidden direct-delivery Definitions, one RecastZone command, and a hidden ChainLightning Definition.
- Current authoring contains 158 Trigger rows and 606 Trigger-owned graph Nodes.
- Current runtime outcomes are 55 skill deliveries and 22 typed commands including one Zone recast; 81 owners have no runtime outcome.
- Trigger CSV and graph Node contracts already contain the required event, condition, targeting, value-source, timing, and visual data, so the first migration needs no new schema.
- Semantic audit found 65 working Trigger reactions, 17 event-driven rows with no final outcome, and 76 rows that belong to ordinary Skill/Choice/Passive execution.
- The 76 non-Trigger rows are 75 OnCast rows plus `vega-b-master1-second-slash`, a same-source follow-up.
- The technical no-outcome 81 split into 64 OnCast modifiers and 17 incomplete event reactions; Generation must not treat those groups the same.
- `SkillCatalogRuntimeTests.TriggerSemanticClassificationBaselineIsStable` fixes the Generation result classification at `65/17/76`.
- Phase 1 solution build completed with error 0; Unity focused EditMode test passed 1/1 and loaded catalog 5/8/8.
- Phase 2 changed no CSV, Parsing, Validation, or Generation contract; runtime-kind family verification passed 13/13.
- Phase 3 changed no CSV, Parsing, Validation, or Generation contract; existing-skill reactions now pass the learned target runtime and Definition directly into the common reaction entry point.
- Phase 3 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 4 Generation excludes all 76 semantic non-Triggers from final Trigger arrays, attaches 74 normal cast/passive payloads to existing Nodes, and maps `ariel-e-trait-4` to the existing conditional-damage Choice handler.
- Phase 4 retains the current Trigger CSV schema; the `ariel-e-trait-4` graph owner/handler/value is corrected from separate Trigger damage to Choice `ConditionalDamageMultiplier(holy-exposure, 1, 1.5)`.
- Phase 4 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 5 Generation converts 40 direct-delivery outcomes to common effect payloads and materializes the 17 previously incomplete `StatusModifier` outcomes without changing CSV schema.
- Phase 5 final runtime counts are effect 57, learned-skill reference 4, command 21, and missing outcome 0.
- Phase 5 solution build completed with error 0; Unity full EditMode tests passed 14/14.
- Phase 6 final catalog has 48 passive source reactions: effect 24, learned-skill reference 4, and command 20; all have outcomes.
- Phase 6 uses the existing cooldown refund 14 and reload reduction 6 commands without schema changes.
- Phase 6 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 7 Generation no longer creates the `ChainLightning__chain` SkillDefinition and maps RecastZone node delay into the Trigger scheduler.
- Phase 7 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 8 Generation emits `SkillReactionOp` into existing Skill/Choice/Passive Nodes and no longer emits runtime Trigger owner arrays or hidden Trigger Definitions.
- `SkillTriggerDefinition` C# references are zero; solution build error 0, Unity Console error 0, and full EditMode tests passed 15/15.
- Reviewer correction 1 reuses the existing `ExecuteSkill` Node parameters to encode Vega B's `vega-b` 0.45 follow-up; no node definition or CSV schema was added.
- Final normal cast/passive payload count is 73 after excluding two duplicated event payload rows and mapping `ariel-e-trait-4` to its Choice modifier.
- Reviewer correction 2 changes only runtime execution policy for the generated Vega follow-up and does not alter Parsing, validation, or schema.
- Reviewer correction 3 changes only runtime reaction multiplier composition and does not alter CSV parsing, validation, Generation, or schema.
- Solution build completed with error 0; Unity EditMode tests passed 16/16.
- Reviewer correction 4 removes only an unused runtime catalog lookup; CSV parsing, validation, Generation, and schema remain unchanged.
- Code Reviewer final PASS confirms no data-contract change; C# obsolete Trigger symbol search is 0 and EditMode `TestResults.xml` is 16/16 passed.

### History

- 2026-07-31: User required integration rather than moving the old runtime class to another script.
- 2026-07-31: Designer recorded a Generation migration that retains the current authoring source while removing the final runtime Trigger Definition and hidden skill output.
- 2026-07-31: User clarified the semantic boundary using Ariel-B trait 4 versus traits 1~3 and 5.
- 2026-07-31: Designer corrected the Generation plan so ordinary cast/modifier rows do not become Trigger reactions.
- 2026-07-31: User approved restoration of both the 17 event outcomes and 64 normal cast outcomes and assigned Code Builder.
- 2026-07-31: Code Builder completed Phase 1 semantic catalog baseline verification.
- 2026-07-31: Code Builder completed Phase 2 without changing the data contract.
- 2026-07-31: Code Builder completed Phase 3 existing-skill runtime reuse without changing the authoring schema.
- 2026-07-31: Code Builder completed Phase 4 final ownership separation without changing the authoring schema.
- 2026-07-31: Code Builder completed Phase 5 direct-delivery and incomplete event-outcome Generation.
- 2026-07-31: Code Builder completed Phase 6 final passive-source and state-command count verification.
- 2026-07-31: Code Builder completed Phase 7 Zone/Chain Generation consolidation.
- 2026-07-31: Code Builder completed Phase 8 obsolete Trigger contract deletion without changing Parsing or authoring schemas.
- 2026-07-31: Code Builder applied Reviewer correction 1 with one existing graph handler value change and no Parsing/schema changes.
- 2026-07-31: Code Builder applied Reviewer correction 2 without data-contract changes.
- 2026-07-31: Code Builder applied Reviewer correction 3 without data-contract changes; reaction multiplier now composes multiplicatively with existing skill modifiers.
- 2026-07-31: Code Builder applied Reviewer correction 4 without data-contract changes; removed unused catalog access from `SkillTrigger`.
- 2026-07-31: Code Reviewer completed final PASS; data/CSV path remains unchanged.

## Task: 2026-08-02 Enemy Slash SingleAttack CSV Migration

### Task title

Move `Slash` and `FireDragonSlash` from the enemy AreaAttack authoring table to the SingleAttack authoring table.

### Goals

- Remove both rows from `skills_area_attack.csv`.
- Add both rows to `skills_single_attack.csv` with `runtime_kind=SingleAttack` and the SingleAttack column layout.
- Keep their damage, targeting, cooldown, visual and hitbox values while converting `DamageArea` to `Damage`.

### Constraints

- Change only the two enemy skill CSV files.
- Do not add a new runtime kind, parser field, builder branch or combat implementation.
- Do not claim Unity catalog/runtime validation; only static CSV validation was run in this task.

### Role Owner

Code Builder.

### Status

CSV migration is corrected on disk and static schema checks pass; Unity TextAsset reimport/runtime sync is pending.

### Next Actions

- In Unity, run `Pakuri/Sync CSV Runtime Catalog Assets`, then `Pakuri/Validate CSV Source Data`.
- If the same 48-column error remains, reimport the enemy `skills_single_attack.csv` TextAsset or restart the Unity Editor before validating again.

### Evidence

- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/area_attack/skills_area_attack.csv` now contains only its header and type row; `Slash` and `FireDragonSlash` are absent.
- `Pakuri/Assets/CSVdata/authoring/enemy/skills/base/single_attack/skills_single_attack.csv` contains both rows with 49 columns, `SingleAttack`, `Damage`, and `charge_ramp_seconds=3` / `charge_move_speed_multiplier=2.5`.
- Direct UTF-8 disk read reports header=49 columns and `Slash` row 6=49 columns; Unity previously reported an imported row 6 with 48 columns.
- `git diff --check` completed without whitespace errors.
- `GameDataCatalogBuilder.Skills.cs` maps `DamageArea` to `SingleSkillDefinition`; the migrated rows now use the explicit `Damage` profile.

### History

- 2026-08-02: Code Builder moved and converted the two enemy rows; no C# files were changed.
- 2026-08-02: Unity reported the pre-correction 48-column imported row; the disk CSV was rechecked at 49 columns and Unity reimport was left as the next verification step.

## Task: 2026-07-31 Reaction Outcome Definition Materialization

### Task title

Materialize skill-like reaction payloads as concrete family Definitions for the common cast path.

### Goals

- Preserve current Trigger and graph CSV schema while changing Generation output from raw runtime effect payloads to resolved Single, Zone or Buff Definition links.
- Reuse existing learned Definitions when a reaction executes an existing skill.
- Keep cooldown, reload and status-duration changes as typed non-skill commands and convert `RecastZone` to a Zone skill outcome.

### Constraints

- Preserve IDs, values, targeting, timing, source attribution, visuals, dynamic event-value policies and current outcome count parity.
- Do not register auxiliary outcome Definitions in learned active/passive slots or add a runtime kind, Executor, Actor or C# script.
- Do not enable raw effect and generated Definition outcomes simultaneously.
- Keep Parsing and authored CSV unchanged unless Phase 10 evidence proves a required value has no existing source.

### Role Owner

Designer for data-contract handoff; Code Builder for Generation and Editor-test migration.

### Status

Design pending implementation. The current completed baseline remains effect 57, learned-skill reference 4, command 21, missing 0 and must be reverified in Phase 10.

### Next Actions

- Inventory every raw effect field and map it to its concrete family Definition field before deleting runtime payload fields.
- Inventory additional-damage and hit-chain Node payloads currently consumed by `ApplyHitEnhancements`; materialize Definitions only after their proc/count semantics are fixed by tests.
- Change Editor tests to verify final Definition family/reference and typed command parity instead of the current `Effect`/`TargetSkillId`/`Command` shape.
- Record each Phase commit and build/EditMode result here and in the primary COMBAT handoff.

### Evidence

- `GameDataCatalogBuilder.Nodes.cs` currently creates raw `SkillCastEffect` values for damage, status and shield outcomes.
- The same builder creates `RecastZone`, `RefundCooldown`, `ReduceReload` and `ExtendStatusDuration` commands.
- `SkillCatalogRuntimeTests.cs` directly asserts raw effect fields, command kinds, RecastZone values and outcome-kind counts.
- `SkillExecution.TryExecuteReaction` currently requires learned runtime data, so raw effects without learned runtimes cannot be migrated by `TargetSkillId` lookup alone.
- Trigger CSV inspection found 37 active-skill reactions with zero non-default proc/count/internal-cooldown rows and 126 passive reactions with 13 non-default rows; Phase 10 must fix this as the gate-migration baseline.

### History

- 2026-07-31: User approved normal-cast-path reuse for conditional skills.
- 2026-07-31: Designer selected Generation-resolved Definition references to avoid runtime payload interpretation and runtime catalog lookup, while retaining typed non-spatial command exceptions.

## Task: 2026-08-03 Monster and Enemy Image CSV Runtime Wiring

### Task title

Load distinct monster Standing and enemy display Sprites from CSV Image paths.

### Goals

- Keep `MonsterIconImage` for monster icons.
- Add `Image` to `monsters.csv` for five Standing Sprites.
- Add `Image` to `enemies.csv` for all 16 current enemy Sprites.
- Parse, validate, generate and serialize both Image path sets through the existing runtime catalog.

### Constraints

- Use only inspected asset paths and prefab Sprite GUID mappings.
- Preserve the existing CSV-to-`CsvRuntimeCatalog` pipeline.
- Do not remove `MonsterIconImage` or add a duplicate icon field.

### Role Owner

Code Builder

### Status

Implemented. CSV asset validation, Unity catalog sync, scene validation and solution build passed; Unity EditMode suite has two unrelated existing Trigger test failures.

### Next Actions

- User verifies Standing images in `PrisonPanel` and `MenifestedSuccessPopUp`, and enemy images in `PrisonPanel/Prisonal/Image` during Play Mode.

### Evidence

- `monsters.csv` contains five `Image` paths under `Assets/Image/Monster/*/Standing` and preserves `MonsterIconImage`.
- `enemies.csv` contains 16 `Image` paths matched to current Stage1/Stage2 prefab `m_Sprite` GUIDs and `.meta` files.
- `CsvRowParser`, `MonsterDefinition`, `EnemyDefinition`, `GameDataCatalogBuilder` and `CsvAssetReferenceCollector` now carry the new paths.
- Static verification reported `monster_rows=5`, `monster_images=5`, `bad_monster=0`, `enemy_rows=16`, `enemy_images=16`, `bad_enemy=0`.
- Unity sync completed and catalog load reported 5 monsters, 8 stage-one enemies and 8 stage-two enemies.
- `dotnet build Pakuri/Pakuri.sln --no-restore`: 0 errors, 2 existing assembly-reference warnings.

### History

- 2026-08-03: Code Builder added distinct monster/enemy Image CSV fields, runtime wiring, UI consumers and removed old serialized monster portrait/Karin Sprite references.

## Task: 2026-08-03 Stage Flow CSV Split

### Task title

Organize Stage Encounter and Reward CSV data under separate `Stage1` and `Stage2` folders.

### Goals

- Create `stage_flow/Stage1/StageEncounter.csv` and `StageReward.csv`.
- Create `stage_flow/Stage2/StageEncounter.csv` and `StageReward.csv`.
- Keep StageManager runtime loading all four files in one StageFlowTable.

### Constraints

- Preserve every existing Encounter and Reward row and column value.
- Keep `StageDay.csv` at the stage_flow root because it was not part of the requested split.
- Do not overwrite unrelated user changes in prefabs, scene UI, combat scripts, or prior task files.
- Store CSV files as UTF-8.

### Role Owner

Code Builder

### Status

Implemented and statically verified; Unity asset reimport and Play Mode remain user-owned.

### Next Actions

- In Unity, allow the new folders/CSV TextAssets to import, then validate `NewRunScene` stage 1 and stage 2 day progression.

### Evidence

- Exact normalized split comparison returned `encounter_exact_split=True` for 60 Encounter rows and `reward_exact_split=True` for 9 Reward rows.
- Static CSV validation found Stage1 Encounter 30 rows, Stage2 Encounter 30 rows, Stage1 Reward 5 rows, and Stage2 Reward 4 rows; all Encounter files have 14 columns and all Reward files have 13 columns.
- `StageManager.cs` now has four serialized stage CSV references and `StageFlowTable.Load` loads both stage file pairs.
- `NewRunScene.unity` assigns the four new TextAsset GUIDs to `stage1EncounterCsv`, `stage1RewardCsv`, `stage2EncounterCsv`, and `stage2RewardCsv`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; the existing two Unity reference-conflict warnings remain.

### History

- 2026-08-03: Code Builder split the two stage-flow tables by stage, updated Inspector references and runtime loading, and removed the duplicate root CSV files after exact data comparison.

## Task: 2026-08-03 Skill Reaction IsTrigger Runtime Contract

### Task title

후속 반응 스킬을 공통 실행 경로에서 식별할 `SkillReaction.IsTrigger` 계약을 추가한다.

### Goals

- 반응 정의가 실행 스냅샷에 `IsTrigger`를 전달하도록 한다.
- 반응으로 생성된 스킬이 다시 사건 반응을 발행하지 않도록 Combat 실행 경로와 연결한다.

### Constraints

- CSV 열과 스키마를 추가하지 않는다.
- 모든 반응은 기존 `GameDataCatalogBuilder`가 생성하는 런타임 `SkillReaction` 객체의 기본값을 사용한다.
- 일반 스킬 정의 자체의 실행 경로와 기존 반복·지연 값은 변경하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 런타임 생성 경로와 Assembly-CSharp 빌드를 확인했다.

### Next Actions

- Unity Play Mode에서 CSV 런타임 카탈로그 생성 후 반응 객체의 `IsTrigger` 기본값이 적용되는지 확인한다.

### Evidence

- `GameDataCatalogBuilder.BuildRuntimeCatalog`가 매번 `SkillReaction` 객체를 생성하며, `SkillNodeConditions.SkillReaction.IsTrigger`의 기본값은 `true`다.
- 일반 시전 효과 변환은 `SkillReaction`을 임시로 사용한 뒤 `SkillCastEffect` 노드만 반환하므로 `IsTrigger` 실행 태그를 직접 사용하지 않는다.
- `SkillExecution.ExecuteReactionOutcome`가 반응의 `IsTrigger`를 `TryExecuteResolvedEffect`로 전달한다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore`는 오류 0개, 기존 Unity 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-03: Code Builder가 기존 사건 연쇄 상태 타입을 제거하는 공통 실행 계약으로 `SkillReaction.IsTrigger`를 추가했다.

## Task: 2026-08-03 Restore NewRunScene Stage CSV Inspector References

### Task title

Reconnect the four Stage1/Stage2 Encounter and Reward TextAssets on `NewRunScene.StageManager`.

### Goals

- Remove the `{fileID: 0}` serialized references that stop `StageManager.LoadTables()`.
- Preserve the existing StageFlowTable loading code and CSV files.

### Constraints

- Change only the four StageManager CSV fields in `NewRunScene.unity`.
- Use the actual GUIDs from the four CSV `.meta` files.
- Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and statically verified.

### Next Actions

- User reloads `NewRunScene` and verifies StageManager starts and enemy spawning proceeds in Play Mode.

### Evidence

- `NewRunScene.unity` now assigns `stage1EncounterCsv`, `stage1RewardCsv`, `stage2EncounterCsv`, and `stage2RewardCsv` with `fileID: 4900000` and the matching CSV GUIDs.
- The four GUIDs resolve to `CSVdata/stage_flow/Stage1/StageEncounter.csv`, `Stage1/StageReward.csv`, `Stage2/StageEncounter.csv`, and `Stage2/StageReward.csv`.
- `git diff --check -- Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` returned no whitespace errors.

### History

- 2026-08-03: Code Builder restored the four missing StageManager TextAsset Inspector references.

## Task: 2026-08-03 Stage Runtime Catalog Migration

### Task title

Move Stage CSV parsing into the Loading runtime catalog.

### Goals

- Build one `StageDefinition` from the five Stage CSV sources in Loading.
- Let `StageManager` consume `GameDataLoader.CurrentCatalog.Stage` instead of parsing CSV directly.
- Preserve the existing Stage day, encounter, reward, boss, and prisoner values.

### Constraints

- Reuse the existing `CsvParser` and ordered Loading pipeline.
- Keep the current Stage CSV paths and runtime Resources catalog.
- Do not change unrelated Combat or UI behavior.

### Role Owner

Code Builder

### Status

Implemented and compiled. Play Mode verification remains user-owned.

### Next Actions

- User verifies Stage 1/2 day progression, enemy spawning, rewards, and boss selection in Play Mode.

### Evidence

- `Assets/Scripts/Loading/RuntimeCatalog/StageDefinition.cs` defines Stage day, encounter, and reward runtime models.
- `Assets/Scripts/Loading/Generation/StageDefinitionBuilder.cs` parses the five stage TextAssets through `CsvParser.CsvTable.Load`.
- `GameDataCatalogBuilder` assigns the model to `GameDataCatalog.Stage`; `GameDataLoader` requires all five Stage source references.
- The five active Stage CSVs now contain header, type, and data rows; UTF-8 `Import-Csv` checks report 23/31/6/31/5 data rows with matching 10/14/13/14/13 columns.
- `dotnet build Pakuri/Pakuri.sln --no-restore` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder added the Stage runtime model and builder, connected the Loading catalog, normalized Stage CSV type rows, and removed StageManager's local CSV table parser.

## Task: 2026-08-03 Remove Critical Resistance Authoring Schema

### Task title

Remove critical-resistance columns from active unit/status authoring CSVs and align Generation with the existing critical-chance bonus contract.

### Goals

- Remove `base_crit_resistance`, `crit_resistance` and `critical_resistance_bonus_per_stack` from current authoring schemas.
- Remove their parser/model/generation mappings.
- Change the Vega conditional trait row to `AllAllies` plus `StatusCriticalChanceBonus` `0.10`.

### Constraints

- Do not change the current CSV loading architecture or add a replacement resistance column.
- Preserve all non-resistance unit defenses and status values, including `vulnerable` critical-damage-taken `0.03`.
- Leave `Assets/Legacy` historical source files untouched.

### Role Owner

Code Builder.

### Status

Implemented and statically verified; Unity runtime catalog synchronization is pending because Unity Editor processes are active.

### Next Actions

- Sync/reimport the current authoring CSV TextAssets in Unity, then validate the generated catalog and Vega trait behavior.

### Evidence

- `CsvRowParser.cs`, `CsvSourceModel.cs`, `GameDataCatalogBuilder.cs` and `GameDataCatalogBuilder.Skills.cs` no longer read or map critical-resistance fields.
- `skill_node_definition_params.csv` and `skill_node_definitions.csv` no longer define `StatusCriticalResistanceBonus`; current passive CSV uses `StatusCriticalChanceBonus` for Vega G trait 3.
- Current CSV checks report matching imported field counts: monsters 22 columns/5 data rows, enemies 24 columns/16 data rows, status effects 19 columns/18 data rows.
- Active authoring/script search for `CriticalResistance`, `CriticalResistanceBonus`, `StatusCriticalResistanceBonus`, `crit_resistance`, `base_crit_resistance` and `critical_resistance_bonus_per_stack` returned no results.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: User approved moving the design toward `CritChanceBonus` and removing the target critical-resistance stat.
- 2026-08-03: Code Builder removed the active authoring/runtime schema fields and preserved only the existing attacker-side critical-chance bonus mechanism.

## Task: 2026-08-03 AreaAttack Authoring Unification

### Task title

Use `AreaAttack` for the `sein-d` area skill and remove `Field` from the active skill data contract.

### Goals

- Change the `sein-d` `runtime_kind` value from `Field` to `AreaAttack`.
- Ensure the authoring loader accepts only `AreaAttack` for the area-attack CSV.
- Remove the obsolete `Field` enum value and all active generation/execution/test references.

### Constraints

- Preserve every other `sein-d` CSV value and keep the existing area-attack CSV layout.
- Do not add a replacement runtime kind or alter zone damage/timing behavior.
- Do not modify historical `Assets/Legacy` data.

### Role Owner

Code Builder.

### Status

Implemented and statically verified; Unity TextAsset reimport/runtime catalog sync remains pending.

### Next Actions

- Reimport/sync the authoring CSV in Unity and validate the generated catalog after the Editor refresh.

### Evidence

- `skills_area_attack.csv:5` has `runtime_kind=AreaAttack`; CSV import reports all three rows with the expected 33 properties.
- `CsvSourceLoader` area base/choice loaders now pass only `SkillRuntimeKind.AreaAttack` as the allowed runtime kind.
- Removing `Field` from the enum makes the generic `CsvDataValidator` enum parsing reject the obsolete value automatically; no explicit Field validator branch remains.
- Active `SkillRuntimeKind.Field` and exact `"Field"` searches returned no results.
- Solution build completed with 0 errors and 2 existing assembly-reference warnings.

### History

- 2026-08-03: User requested that the `Field` skill kind and its validation rules be removed after AreaAttack unification.
- 2026-08-03: Code Builder migrated `sein-d` and removed the obsolete data-contract references.

## Task: 2026-08-03 Shorten Skill Graph Owner IDs

### Task title

Store only the owner suffix in `skill_graph_nodes_*.csv` and reconstruct the full owner ID from `monster_id` during parsing.

### Goals

- Convert values such as `eve-c-trait-1` to `c-trait-1` in the graph authoring CSVs.
- Keep the parsed `SkillGraphNodeRow.OwnerId` canonical as `eve-c-trait-1`.
- Avoid changing existing choice, trigger, skill and target ID tables.

### Constraints

- Apply the transformation to the six active `skill_graph_nodes_*.csv` files under `monster/skills/choices` only.
- Preserve all node order, node types, arguments, target skill IDs and exclusion IDs.
- Use `monster_id + "-" + owner_id`; existing repository IDs use hyphens.

### Role Owner

Code Builder.

### Status

Implemented and statically verified; Unity TextAsset reimport/runtime catalog validation is pending.

### Next Actions

- Reimport the changed graph CSV TextAssets in Unity and run the existing CSV validation/catalog load.

### Evidence

- The six graph CSV files changed 858 duplicated prefixes; the post-transform import contains 858 rows with no `owner_id` still beginning with its `monster_id-` prefix.
- `SkillGraphParser.cs` reads `monster_id` and `owner_id` separately, then canonicalizes the owner before `ValidateSkillNodeOwner`, `ResolveSkillGraphTargetSkillId` and `MaterializeSkillGraphRows` consume it.
- The normalization is idempotent, so canonical owner IDs remain accepted during migration.
- `git diff --check` passed and the full solution build completed with 0 errors and 2 existing assembly-reference warnings.

### History

- 2026-08-03: User approved the short `owner_id` authoring format and parser reconstruction approach.
- 2026-08-03: Code Builder applied the CSV transformation and canonicalized graph owner IDs at the parser boundary.

## Task: Repair CSV Header/Type Column Counts After Schema Cleanup

### Goals

- Keep the current authoring CSV schema loadable after the reported `monsters.csv` header/type count failure.
- Record the exact data-file changes and static validation evidence for the runtime catalog input.

### Constraints

- Change only the type rows in `monsters.csv`, `enemies.csv`, and `status_effects.csv`.
- Preserve CSV data rows and leave the parser implementation unchanged.

### Role Owner

Code Builder.

### Status

Completed and statically verified; Unity Editor auto-sync/reimport remains pending.

### Next Actions

- Let Unity reimport the changed TextAssets and verify the runtime catalog synchronization in the Editor.

### Evidence

- Header/type counts are aligned at 22/22 (`monsters.csv`), 24/24 (`enemies.csv`), and 19/19 (`status_effects.csv`).
- A quote-aware scan of all 39 current authoring CSV files and all nonblank data rows returned `bad=0`.
- `git diff --check` passed; `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: The reported Unity error identified a type-row count mismatch at `CsvParser.cs:122`.
- 2026-08-03: Code Builder repaired the three current authoring CSV type rows without changing data values or parser code.
