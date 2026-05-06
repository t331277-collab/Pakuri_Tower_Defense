# MON_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Scope

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

Legacy non-English note retained these code references: `{NAME}_MONSTER.md`.

## Common Terms

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `SkillSlot`.
- Legacy non-English note retained these code references: `MonsterDefinition`.
- Legacy non-English note retained these code references: `GameDataCatalog`.

## Creation Rules

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/{monster}`, `Assets/Data/GameData/Monsters/*.asset`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `boards/MON/EVE_MONSTER.md`.
- Legacy non-English note retained these code references: `boards/UI/DEBUGSCENE_UI.md`.
- Legacy non-English note retained these code references: `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Legacy non-English note retained these code references: `boards/COMBAT/*.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

## Character Board Rules

- Eve: `boards/MON/EVE_MONSTER.md`
- Vega: `boards/MON/VEGA_MONSTER.md`
- Ariel: `boards/MON/ARIEL_MONSTER.md`
- Sein: `boards/MON/SEIN_MONSTER.md`
- Rin: `boards/MON/RIN_MONSTER.md`

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Task title
- Source references
- Skill slots A-J
- Runtime implementation status
- Data asset status
- DebugScene test status
- Evidence
- History

## Migrated Task Blocks

## Task: 2026-05-06 Sein A-E Active Runtime Implementation

### Task title

Record common monster state for Sein active skill runtime implementation.

### Goals

- Keep the common monster board aware that Sein A-E active skills now have runtime behavior.
- Record that B-E use independent cooldown/panel state rather than the shared A-skill magazine counter.

### Constraints

- Role Owner is Code Builder.
- Detailed Sein behavior is recorded in `boards/MON/SEIN_MONSTER.md`.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Sein A-E in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `boards/MON/SEIN_MONSTER.md` contains the detailed 2026-05-06 Sein A-E task block.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now exists and implements Sein active runtime behavior.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs`, `CombatRuntimeController.cs`, and `CombatRuntimeProjectiles.cs` now include Sein in selected-Monster dispatch.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` classify Sein A-E as runtime implemented.
- Unity-MCP `execute_code` confirmed runtime catalog resolution reports Sein A-E as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.

### History

- 2026-05-06: User requested Sein active skill A-E implementation from the Sein reference skill folder.

## Task: 2026-05-05 MonsterPanel Active Skill Display

### Task title

Record common MonsterPanel display behavior for selected Monster active skills.

### Goals

- Show only the current 1P Monster in the combat MonsterPanel for now.
- Activate `Active1` through `Active3` as learned active skills are available.
- Show current magazine counts for magazine projectile skills and cooldown/reload overlay for unavailable skills.

### Constraints

- Role Owner is Code Builder.
- Future 2P-5P Monster groups remain reserved for later multi-Monster party support.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies MonsterPanel behavior for current selected Monsters in RunScene and DebugScene Play Mode, including per-slot assigned skill cooldowns and current-ammo-only text.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/reference/7.UI/8. combat-screen-layout.md` defines the character skill group as character icon plus three selected active skill icons with reload and bullet-count state.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now exposes learned selected active skill state through `GetMonsterPanelSkillViews(...)`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now binds `MonsterPanel/1PMonster/Active1..3` through `CombatMonsterPanelUiController`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now displays magazine ammo through existing TMP text as a single current count, not `current/max`, and does not create `CountText`.
- The follow-up UI binder still reads cooldown/reload values from each `MonsterPanelSkillView`, which is created from the assigned `SkillDefinition.SkillSlot`.
- Scene-specific details are recorded in `boards/UI/RUNSCENE_UI.md` and `boards/UI/DEBUGSCENE_UI.md`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Follow-up saved-scene and loaded-scene checks found no `CountText` after the cleanup.
- 2026-05-05 follow-up: `Pakuri/Assets/Data/GameData/Monsters/rin.asset` and `Pakuri/Assets/CSVdata/source/monster_skills.csv` now classify Rin A as `MagazineProjectile`, B as `Buff`, C as `LineAttack`, D as `Execute`, and E as `AreaAttack`.
- 2026-05-05 follow-up: `CombatRuntimeController.CreateMonsterPanelSkillView(...)` now treats selected active skills as magazine skills only when both `RuntimeKind == MagazineProjectile` and `MagazineCapacity > 0`, preventing zero-magazine skills from showing or following the shared magazine/reload state.
- 2026-05-05 follow-up: `CombatMonsterPanelUiController` hides the TMP ammo text for non-magazine skills and assigns `DebugUiSolid` to cooldown overlays for visible black-to-white cooldown reveal.
- 2026-05-05 follow-up validation: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and sequential `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; Unity-MCP read-only asset check reported the corrected Rin RuntimeKind values and `DebugUiSolid=DebugUiSolid`.
- 2026-05-05 Eve/Ariel follow-up: Eve data now imports as A `MagazineProjectile`, B `LineAttack`, C `Field`, D `AreaAttack`, E `MagazineProjectile`, and F-J `Passive`, all `RuntimeImplemented`.
- 2026-05-05 Eve/Ariel follow-up: Ariel data now imports as A `MagazineProjectile`, B `Shield`, C `AreaAttack`, D `Mark`, E `AreaAttack`, and F-J `Passive`, all `RuntimeImplemented`.
- 2026-05-05 Eve/Ariel follow-up validation: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings; Unity-MCP asset import reported the corrected Eve/Ariel skill kinds and implementation states.

### History

- 2026-05-05: User requested current 1PMonster MonsterPanel skill display while preserving future NP/2P-5P Monster slots.
- 2026-05-05: User requested `Text (TMP)` only for ammo text, current-count display such as `10`, `9`, `8`, and independent Active1-3 cooldown state; Builder updated the shared MonsterPanel binder and removed the old DebugScene `CountText` objects.
- 2026-05-05: User reported Howling and Shockwave were still treated like magazine skills and followed Active1 cooldown; Builder corrected Rin active skill RuntimeKind data and added a magazine-capacity guard to the shared MonsterPanel snapshot.
- 2026-05-05: User verified the Rin MonsterPanel fix in Play Mode and requested the same data audit for Eve and Ariel; Builder corrected Eve/Ariel runtime kind and implementation-state metadata.

## Task: 2026-05-05 Rin F Follow-up Visual And Debug Damage Labels

### Task title

Record Rin follow-up visual and common debug damage popup label change.

### Goals

- Keep common monster state aware that combat damage popups now include debug damage-type labels.
- Record that Rin F follow-up hits have a white circle effect and combined mixed-damage popup text.

### Constraints

- Role Owner is Code Builder.
- The damage-type suffix notation is debugging-only text.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies damage popup readability and Rin F follow-up feedback in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Rin-specific details are recorded in `boards/MON/RIN_MONSTER.md`.
- Combat popup details are recorded in `boards/COMBAT/COMBAT_BLACKBOARD.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now maps damage attributes to Korean debug labels for popup text.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` now spawns a white circle for `RinAmbidextrousFollowup`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors; Unity ready wait timed out after the compile request.

### History

- 2026-05-05: User requested Rin F follow-up visual feedback and debug damage-type notation in damage popup text.
- 2026-05-05: Builder implemented the visual/debug popup change and recorded validation evidence in Rin and combat boards.

## Task: 2026-05-05 Rin F-J Passive Runtime Implementation

### Task title

Record Rin passive F-J runtime implementation in common monster state.

### Goals

- Keep common monster board aware that Rin A-J is now marked runtime implemented.
- Record the current interpretation that "all ally" wording maps to the current one selected allied Monster runtime model.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin F-J in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Rin-specific details are recorded in `boards/MON/RIN_MONSTER.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` now contains passive runtime helpers for `rin-f`, `rin-g`, `rin-h`, `rin-i`, and `rin-j`.
- `Pakuri/Assets/Data/GameData/Monsters/rin.asset` now marks `rin-f`, `rin-g`, `rin-h`, `rin-i`, and `rin-j` as `ImplementationState: 2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-05: User requested implementation of Rin passive skills F-J from the Rin reference skill folder.

## Task: 2026-05-05 Monster Skill Runtime Range Rule

### Task title

Define monster skill runtime range as map-wide and projectile cleanup as fixed X-boundary based.

### Goals

- Treat current monster skills as having no practical skill range limit during runtime.
- Apply map-wide reach to magazine, non-magazine, and area skill implementations unless a later user request explicitly defines an exception.
- For magazine/projectile skills, keep projectile lifetime from acting as the gameplay range limit and delete the projectile when it reaches the predefined battlefield X coordinate.

### Constraints

- Role Owner is Designer.
- This is a design rule and implementation handoff note, not a completed code change.
- User performs Play Mode gameplay verification.
- Code Reviewer execution requires explicit user permission.

### Role Owner

Designer

### Status

Design rule recorded; Code Builder handoff needed if the current runtime should be made fully common across all monsters.

### Next Actions

- Code Builder should verify all selected-Monster skill implementations and remove skill-range/lifetime behavior that contradicts the map-wide runtime rule.
- Code Builder should preserve the current X-boundary projectile cleanup behavior and make it the common projectile rule.
- Run build/compile/console validation after any code change; do not run Unity Play Mode gameplay verification.

### Evidence

- `boards/COMBAT/PROJECTILE_BLACKBOARD.md` records the 2026-05-04 selected-Monster projectile X-edge cleanup task as implemented.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` currently calls `HasPlayerProjectileReachedBattlefieldXEdge(projectile)` for no-hit player projectiles and sets the status label that the projectile disappeared at the battlefield X boundary.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` currently uses `GetRinMapWideSkillRange()` for Rin C, D, and E target/search range.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` still computes Rin A projectile `RemainingLifetime` from `skill.Range`, so a Builder pass is needed if the no-range rule should be enforced for all projectile skills.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` still computes Ariel A projectile lifetime from `skill.Range` and configured lifetime, so a Builder pass is needed if the no-range rule should be enforced for all projectile skills.

### History

- 2026-05-05: User clarified that current monster magazine, non-magazine, and area skills should not have a gameplay range limit; the whole map is the range, and projectile skills disappear when reaching a predefined X coordinate.

## Task: 2026-05-04 Rin D Execution Target And Hit Effect Fix

### Task title

Record Rin D execute-only targeting and hit-effect correction.

### Goals

- Keep common monster state aware that Rin D now requires an execution-threshold target.
- Record that Rin D master threshold `-10%` means 30% max HP becomes 20% max HP.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin D in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Rin-specific details are recorded in `boards/MON/RIN_MONSTER.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` no longer returns a nearest fallback target for Rin D; it only returns an enemy at or below the execution-health threshold.
- `Pakuri/reference/2.Monster/rin/skill/d-finishing-blow.md` now documents the execute-only targeting rule and 30% to 20% master threshold interpretation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.

### History

- 2026-05-04: User reported Rin D was attacking full-health enemies and requested an execute-only target rule plus a simple hit effect.

## Task: 2026-05-04 Rin Non-Magazine Skill Map-Wide Range

### Task title

Record Rin's non-magazine whole-map runtime range rule.

### Goals

- Keep the common monster board aware that Rin B-E are non-magazine skills with battlefield-wide runtime target/search range.
- Preserve Rin A as the magazine projectile exception.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin B-E in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Rin-specific details are recorded in `boards/MON/RIN_MONSTER.md`.
- `Pakuri/reference/2.Monster/rin/skill/b-howling.md`, `c-shockwave.md`, `d-finishing-blow.md`, and `e-collapse-strike.md` now include `Runtime implementation note` sections stating Rin non-magazine skills use the whole battlefield map as target/search range.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` now routes Rin C/D/E target search through `GetRinMapWideSkillRange()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and a sequential `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP assembly conflict warnings.

### History

- 2026-05-04: User requested fixing Rin's very short skill range and documenting the non-magazine whole-map range rule.

## Task: 2026-05-04 Rin/Sein Runtime Sprite Catalog And Projectile Edge Cleanup

### Task title

Fix Rin/Sein runtime sprite resolution and make selected-Monster projectiles clean up at the battlefield X edge.

### Goals

- Restore Rin and Sein selected-Monster unit/projectile sprite resolution through the active CSV runtime data path.
- Stop Rin A from disappearing after the short computed range/speed lifetime.
- Apply the common selected-Monster projectile cleanup rule at the battlefield X boundary.

### Constraints

- Role Owner is Code Builder.
- This project uses Unity-MCP, not MSW-MCP.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin, Sein, and the other selected Monsters in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Unity AssetDatabase inspection showed Rin and Sein `MonsterDefinition` ScriptableObjects already had `UnitSprite` and `ProjectileSprite` assigned.
- `Pakuri/Assets/CSVdata/source/monsters.csv` had empty Rin/Sein `unit_sprite_path` and `projectile_sprite_path` cells before this fix; those rows now contain `Assets/Image/Monster/Rin/Rin_Temp (2).png`, `Assets/Image/Monster/Rin/Rin_Shoot.png`, `Assets/Image/Monster/Sein/Sein_Temp.png`, and `Assets/Image/Monster/Sein/Sein_Shoot.png`.
- Unity-MCP `execute_code` import/sync check resolved runtime sprites for `rin` as `UnitSprite=Rin_Temp (2), ProjectileSprite=Rin_Shoot` and for `sein` as the Sein temp/shoot sprite assets.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now contains the four Rin/Sein sprite asset entries generated from the CSV runtime source.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now cleans up selected-Monster projectiles when `HasPlayerProjectileReachedBattlefieldXEdge(...)` detects the projectile has reached the battlefield X limit.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log, not project compile errors.

### History

- 2026-05-04: User reported that only Rin and Sein failed to apply `PrototypeCombatTuning` sprites, while Ariel/Eve/Vega worked.
- 2026-05-04: Builder found that the SO assignments existed, but the active CSV runtime source/catalog omitted Rin/Sein sprite paths.
- 2026-05-04: Builder filled the CSV runtime paths, synced the runtime asset catalog, and changed selected-Monster projectile cleanup from lifetime expiry to battlefield X-edge cleanup.

## Task: 2026-05-04 Rin A-E Active Runtime Implementation

### Task title

Implement Rin active skills A-E and their enhancement/master effects.

### Goals

- Add Rin selected-Monster runtime behavior for A-E.
- Keep the implementation grounded in the current single selected Monster combat runtime.
- Mark Rin A-E active entries as runtime implemented in the Rin monster asset.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented, Reviewer findings fixed, and locally validated. Code Reviewer has not been rerun because the user did not request another review.

### Next Actions

- User verifies Rin A-E in Play Mode.
- Run another Code Reviewer pass only if the user explicitly requests it.

### Evidence

- Rin-specific details are recorded in `boards/MON/RIN_MONSTER.md`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` was added for Rin active runtime behavior.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:70`, `:88`, `:108`, and `:128` now dispatch common selected-Monster skill cooldowns, automatic skills, magazine capacity, and action speed to Rin when Rin is selected.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:52` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:502`/`:504`/`:523` now base Rin elemental follow-up damage on actual applied damage rather than calculated final damage.
- `Pakuri/Assets/Data/GameData/Monsters/rin.asset:88`, `:155`, `:222`, `:287`, and `:354` mark Rin A-E as `ImplementationState: 2`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP warnings.
- Unity-MCP script refresh reached idle and console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-04: Code Builder implemented Rin A-E active runtime from the Rin reference skill folder and updated the Rin monster data asset.
- 2026-05-04: Code Builder fixed the user-approved Reviewer finding so Rin A/C/D/E elemental follow-up damage uses applied physical damage.

## Task: 2026-05-03 Player Monster Overhead Width Follow-up

### Task title

Tighten the selected player Monster HP bar width and keep direct Inspector tuning available.

### Goals

- Reduce the selected Monster HP bar width from the previous auto-layout result.
- Preserve separate name/HP text stacking and direct manual tuning for the selected Monster.

### Constraints

- Role Owner is Code Builder.
- Ground the change in the existing selected-Monster combat runtime code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies selected Monster overhead width in Play Mode.
- If needed, user can still disable `Auto Layout Selected Monster Status` and edit the manual selected-Monster layout fields directly.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:235-251` now uses a tighter selected-Monster automatic bar-width configuration and still exposes the manual selected-Monster local-position/scale override fields.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:344-375` now clamps selected-Monster automatic bar width to an explicit max value instead of allowing the previous wider result.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP warnings.

### History

- 2026-05-03: User reported that the selected Monster HP bar was still too long after the earlier overhead-stack split change.
- 2026-05-03: Code Builder tightened the automatic selected-Monster width clamp while keeping the manual override path.

## Task: 2026-05-03 Player Monster Overhead Status Layout Tuning

### Task title

Make the selected player Monster overhead name/HP display follow sprite size and expose manual layout overrides.

### Goals

- Keep the selected Monster name readable without overlapping the HP text or HP slider.
- Adjust the selected Monster overhead stack from the Monster sprite size instead of relying on one fixed offset for all Monsters.
- Give the user direct manual tuning fields in `CombatRuntimeController` when automatic layout is not enough for a specific Monster sprite.

### Constraints

- Role Owner is Code Builder.
- Ground the change in the existing selected-Monster combat runtime display code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies the selected Monster overhead layout for the Monsters they care about in Play Mode.
- If needed, user disables `Auto Layout Selected Monster Status` on the combat controller and edits the manual bar/name/HP text positions and scale fields directly.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:235-251` adds a dedicated serialized selected-Monster status-layout section so the player Monster overhead display can be tuned without another code edit.
- `CombatRuntimeController.cs:320-321` now stores separate selected-Monster name/HP text labels instead of one combined multiline label.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:253-283` now creates `MonsterNameLabel` and a separate `MonsterHpLabel`, and repositions the selected Monster HP bar from a computed layout.
- `CombatRuntimeScene.cs:344-380` computes the automatic layout from the selected Monster sprite bounds, while the manual mode uses the serialized `selectedMonsterStatusManual*` values exactly.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; only the existing Unity/MCP warnings remained.
- Unity refresh completed with `resulting_state: idle`, and Unity console error query returned only MCP-FOR-UNITY handler exit logs.

### History

- 2026-05-03: User requested fixing the selected Monster overhead HP slider/text/name overlap and asked for direct editability if automatic tuning was hard.
- 2026-05-03: Code Builder changed the selected Monster status visuals to separate name/HP labels, sprite-aware layout, and Inspector-visible manual overrides.

## Task: 2026-05-03 Ariel J Passive Runtime Correction

### Task title

Correct Ariel passive J `Sanctuary Proclamation` so its action-speed and holy-damage windows follow the Archangel Descent reference, then close the E-shield source leak and adjacent E/C runtime bugs.

### Goals

- Keep Ariel passive F-I behavior unchanged.
- Make J action speed trigger after `Archangel Descent` for 5 seconds even when E master 1 is not selected.
- Make J holy-damage bonus depend on the remaining `Archangel Descent` shield, not any generic shield/buff timer.
- Ensure `Archangel Descent` shows a visible battlefield effect when cast.
- Stop Ariel support-skill retries from running every held-input frame while the primary shot is unavailable, which was surfacing as occasional C-skill barrage behavior.

### Constraints

- Role Owner is Code Builder.
- Ground the correction in actual Ariel reference markdown and current runtime code.
- User performs Play Mode verification.
- Code Reviewer was run once earlier for this patch line, and no second review is allowed without a new explicit user request.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally validated. Code Reviewer has not been rerun because the user did not request another review.

### Next Actions

- User verifies in Play Mode that Ariel J holy-damage bonus drops as soon as the active pooled shield is no longer the E shield.
- User verifies that Ariel E now shows a visible battlefield effect on cast.
- User verifies that holding attack no longer causes Ariel C to occasionally barrage while Ariel A is reloading or on shot cooldown.

### Evidence

- Legacy non-English note retained these ASCII code references: `Pakuri/reference/2.Monster/ariel/skill/j-sanctuary-proclamation.md:18-19`.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md:22-24` defines the E shield amount, duration, and cooldown that J depends on, and documents E as a battlefield-wide effect.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:429` now routes E shield application through `ApplyArielUnitShield(shield, duration, true)`, and `CombatRuntimeArielSkills.cs:554-580` now only marks Archangel shield state when the new shield actually claims the pooled selected-Monster shield slot while clearing it if a stronger non-E shield replaces that slot.
- `CombatRuntimeArielSkills.cs:592-600` still reduces tracked Archangel shield value on shield absorption, so J holy-damage gating continues to decay with incoming damage.
- `CombatRuntimeArielSkills.cs:444-451` now creates the missing `ArchangelDescent` battlefield circle effect for Ariel E.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:332-356` now gates Ariel automatic support-skill retries to real firing windows, preventing held-input per-frame retries while Ariel A is blocked by reload or shot cooldown.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh finished with `resulting_state: idle`, and Unity console error query returned only MCP-FOR-UNITY handler exit logs.

### History

- 2026-05-03: User requested implementing Ariel passive skills F-J from the reference folder.
- 2026-05-03: Code Builder verified that F-I were already wired, found that J was reusing the wrong timer/state path, and applied a correction pass grounded in the Ariel E/J documents.
- 2026-05-03: User explicitly requested Code Reviewer execution; Reviewer returned NEEDS_CHANGES for the remaining J shield-source leak.
- 2026-05-03: User then requested fixing the reviewer finding plus Ariel E effect omission and Ariel C occasional barrage behavior; Code Builder applied the follow-up in runtime shield ownership, E visual spawning, and Ariel-only automatic-skill trigger cadence.

## Task: Monster Shield Bar Split Visual

### Task title

Display player Monster shields as one fixed-width HP/shield split bar.

### Goals

- Keep Monster HP and shield numeric values unchanged.
- When shield is present, draw one bar with red HP and white shield sharing the fixed visual width by ratio.
- Apply the shared visual path to the selected Monster status bar.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies shielded Monster HP bar visuals in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now has `UpdateHpShieldBarFill()` using `health + shield` as the visual total while shield is present, so HP 10 and shield 1 are drawn as adjacent red/white segments within the same root bar width.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now calls `UpdateHpShieldBarFill()` for `selectedMonsterHpBarFill` and `selectedMonsterShieldBarFill`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.

### History

- 2026-04-30: User requested League-style shield visuals for all Monster shield-granting skills.
- 2026-04-30: Code Builder changed the shared Monster HP/shield bar update path.
- 2026-04-30: User requested HP Bar `Background` color to differ from white shield; Code Builder changed shared HP bar background renderers to `Color.black`.

## Task: Ariel A-E Active And F-J Enhancement Runtime

### Task title

Implement Ariel skill documents A-E and their F-J enhancement/passive effects.

### Goals

- Read actual Ariel skill markdown under `Pakuri/reference/2.Monster/ariel`.
- Implement Ariel active skills A-E in the combat runtime.
- Implement Ariel passive/enhancement effects F-J where the current selected-Monster runtime has corresponding state.
- Keep the implementation grounded in the existing single selected Monster combat model.

### Constraints

- Role Owner is Code Builder.
- User performs Unity Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it for this implementation.
- Current combat runtime has one selected allied Monster, not a collection of allied units.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Ariel behavior mismatches, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Ariel A-E and F-J selected effects in DebugScene or RunScene Play Mode.
- If exact multi-ally party behavior is added later, revisit Ariel "all allies" effects and expand them from selected Monster to the full ally collection.

### Evidence

- `boards/MON/ARIEL_MONSTER.md` contains the Ariel-specific skill slot and runtime evidence.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` was added for Ariel-specific runtime behavior.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now routes Ariel A to `FireManualArielJudgementLight(direction)` and combines Ariel damage/defense/critical modifiers with projectile hit resolution.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now tracks and displays Ariel Holy Exposure on enemies.
- `Pakuri/Assets/Data/GameData/Monsters/ariel.asset` marks Ariel A-E and F-J as runtime implemented.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY handler logs only.
- 2026-04-30 follow-up fixes addressed Reviewer findings: Ariel A now uses `ariel-a` skill damage/range with projectile speed `17`, last-shot explosion happens from projectile cleanup position, Radiant Shield reflection receives the source attacker, and Holy damage bonuses are not pre-applied before final damage calculation.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User requested Ariel A-E skill implementation plus enhancement effects after pointing to `Pakuri/reference/2.Monster/ariel`.
- 2026-04-30: Code Builder implemented Ariel runtime behavior and updated related data/state files.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder applied the Ariel correction pass and did not rerun Code Reviewer because a new review was not explicitly requested.

## Task: Hold Input Primary Skill Fire

### Task title

Allow all 5 Monster A skills to keep firing while left mouse or touch input is held.

### Goals

- Change the current one-click A skill trigger into a held-input trigger.
- Preserve existing shot interval, magazine, reload, and active-skill trigger behavior.
- Support mouse left-button hold and mobile touch hold.
- Keep the change in the shared combat input path so all 5 player Monsters use the same behavior.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual files and command output.
- Do not run Unity Play Mode gameplay verification; user verifies gameplay.
- Do not run Code Reviewer without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that holding left mouse or touch continuously fires A skill toward the held pointer position for each Monster.
- User verifies left-button/touch-triggered active skill effects still respect their cooldowns while held.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeHud.cs` now uses held input checks: `Mouse.current.leftButton.isPressed`, `Touchscreen.current.primaryTouch.press.isPressed`, `Input.GetMouseButton(0)`, and `Input.touchCount`.
- `CombatRuntimeHud.cs` still sets `fireRequestedThisFrame = true` after converting the current pointer/touch screen position into the clamped world attack point.
- `CombatRuntimeProjectiles.cs` already gates primary fire through `shotCooldown`, `currentShotsRemaining`, and `reloadRemaining`, so held input repeats through the existing fire interval and reload rules.
- `CombatRuntimeProjectiles.cs` calls `TryTriggerEveAutomaticSkills()` whenever the shared fire request is active, preserving left-button active trigger behavior through existing cooldown logic.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity editor state returned `ready_for_tools=true`; Unity console error query returned MCP-FOR-UNITY client handler logs only, not project compile errors.

### History

- 2026-04-30: User requested that holding left mouse click, or mobile touch, continuously fires the 5 Monsters' A skill toward the held pointer position and keeps the same active-skill trigger behavior.
- 2026-04-30: Code Builder changed the shared combat pointer input to treat held mouse/touch as a fire request and validated compilation.

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

## Task: Combat Monster Enemy Implementation

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `combat-monster-enemy-implementation-plan.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log`.

### Constraints

- Role Owner??Code Builder??
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `[CombatDamage]`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/CombatStatModels.cs`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `FormulaLog`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Data/SkillDefinition.cs`, `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs`.
- Legacy non-English note retained these code references: `GameDataCatalog.cs`, `StageOneEnemies`, `MonsterDefinition.cs`, `PrimaryAttribute`, `BaseStats`, `Defenses`, `ActiveSkills`, `PassiveSkills`.
- Legacy non-English note retained these code references: `RunFlowController.cs`, `RunSceneBootstrap.cs`, `GameDataCatalog`, `EveVerticalSliceController.BeginConfiguredDay(...)`.
- Legacy non-English note retained these code references: `EveVerticalSliceController.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Debug.Log("[CombatDamage] ...")`.
- Legacy non-English note retained these code references: `Pakuri/Seed Default Game Data`, `Pakuri/Assets/Data/GameData/Enemies`, `GameDataCatalog.asset`, `StageOneEnemies`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Data/GameData/Monsters/eve.asset`, `PrimaryAttribute`, `ActiveSkills`, `PassiveSkills`, `ImplementationState`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`, `System.Net.Http`, `System.IO.Compression`.
- Legacy non-English note retained these code references: `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `EveVerticalSliceController`, `0f`.
- Legacy non-English note retained these code references: `PakuriGameDataSeeder.cs`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

## Task: Combat Monster Enemy Implementation Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat`, `Pakuri/reference/5.enemy`.
- Legacy non-English note retained these code references: `Pakuri/data`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Constraints

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/reference/3.combat/combat-attribute-and-damage-system.md`, `combat-stat-system.md`, `buff-debuff.md`, `realtime-damage-meter.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/5.enemy/stage-basic-rules.md`, `enemy-stage-index.md`, `stage-1-enemies.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`, `monster-skill-patterns.md`, `skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/data/enemies.csv`, `enemy_runtime.csv`, `skills.csv`, `skill_runtime.csv`, `ally_units.csv`, `ally_runtime.csv`, `status_effects.csv`, `levelup_choices.csv`, `skill_branches.csv`, `levelup_rules.csv`.
- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `EveVerticalSliceController.cs`, `Pakuri/Assets/Scripts/Data/MonsterDefinition.cs`, `GameDataCatalog.cs`, `Pakuri/Assets/Scripts/Run/RunSession.cs`.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `rg`, `Get-ChildItem`, `Get-Content`.
- Legacy non-English note retained these code references: `Pakuri/reference/run-systems-integration-summary-report.html`, `Pakuri/reference/Report/run-systems-integration-summary-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

## Task: Monster Select Run UI Expansion Plan

### Task title

Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.

### Goals

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `2.Monster`, `skill-choice-pool-rule.md`, `combat-reward-system.md`.
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
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `g~j`.

### Evidence

- Legacy non-English note retained these code references: `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs`, `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs`.
- Legacy non-English note retained these code references: `Assets/Scenes/SampleScene.unity`, `Main Camera`, `Global Light 2D`, `CombatRoot`.
- Legacy non-English note retained these code references: `CombatRoot`, `Nexus`, `EveUnit`, `EnemySpawnPoint`, `InputTarget`, `EnemyRoot`, `ProjectileRoot`.
- Legacy non-English note retained these code references: `Pakuri/Assets`, `NO_UI_TOOLKIT_ASSETS`, `NO_UI_NAMED_ASSETS`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/monster-basic-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/skill-choice-pool-rule.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/4.run/combat-reward-system.md`.
- Legacy non-English note retained these code references: `Pakuri/reference/2.Monster/ariel/ariel-tower.md`, `eve/eve-tower.md`, `rin/rin-tower.md`, `sein/sein-tower.md`, `vega/vega-tower.md`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note summarized in English; see surrounding retained task context.
- Legacy non-English note retained these ASCII code references: `, `.
- Legacy non-English note retained these code references: `f~j`, `f-ambidextrous.md`, `g~j`.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.

### History

- Legacy non-English note retained these code references: `AGENTS.md`, `BLACKBOARD.md`.
- Legacy non-English note retained these code references: `2.Monster`, `monster-basic-rule.md`, `skill-choice-pool-rule.md`, `combat-reward-system.md`, `dungeon-squad-run-structure.md`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/monster-select-run-ui-expansion-plan.html`.
- Legacy non-English note retained these code references: `F~J`.
- Legacy non-English note retained these code references: `g~j`.
- Legacy non-English note retained these ASCII code references: `, `.

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

## Task: 2026-05-06 Sein F-J Passive Runtime Implementation

### Task title

Track Sein passive F-J runtime implementation across Monster combat data.

### Goals

- Keep the monster board aligned with the Sein-specific passive implementation.
- Record affected combat systems: projectile hit modifiers, passive procs, and enemy debuff state.
- Preserve validation evidence for future continuation after session reset.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue future Sein work from `boards/MON/SEIN_MONSTER.md`.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `boards/MON/SEIN_MONSTER.md` contains the Sein-specific F-J task block and implementation evidence.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs` now contains passive helper paths for `sein-f`, `sein-g`, `sein-h`, `sein-i`, and `sein-j`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` applies Sein passive projectile modifiers and calls Sein projectile-hit tracking.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` mark Sein F-J as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP runtime catalog inspection confirmed `sein-f` through `sein-j` resolve as `RuntimeImplemented`.

### History

- 2026-05-06: User requested implementation of Sein passive skills F-J after the A-E active skill pass.
- 2026-05-06: Code Builder implemented the passive runtime behavior and recorded detailed evidence in the Sein, projectile, and status-effect boards.

## Task: 2026-05-07 Sein Active Skill Correction Pass

### Task title

Track Sein A/B/C/E active skill correction pass.

### Goals

- Keep Monster-level history aligned with the Sein-specific correction task.
- Record that B is now treated as a magazine active skill in source CSV, asset data, and runtime catalog.
- Record projectile and target-selection behavior changes for future continuation.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue details from `boards/MON/SEIN_MONSTER.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `boards/MON/SEIN_MONSTER.md` contains the detailed 2026-05-07 Sein A/B/C/E correction task block.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/sein.asset` now classify `sein-b` as `MagazineProjectile`.
- Unity-MCP runtime catalog inspection confirmed `sein-b:MagazineProjectile:RuntimeImplemented:mag=4:reload=6:cool=6:interval=0.18`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User gave four correction requests for Sein A, B, C, and E behavior after the initial implementation.
- 2026-05-07: Code Builder implemented and validated the correction pass.

## Task: 2026-05-07 Sein C/E Follow-up Correction

### Task title

Track Monster-level evidence for Sein C delayed path/residual and E ash-zone fixes.

### Goals

- Keep Monster-level history aligned with the Sein-specific follow-up correction.
- Record that C now waits after contact before exploding and creates moving path segments.
- Record that E `Ashen Sky` now places zones on actual hit targets.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue detailed Sein behavior from `boards/MON/SEIN_MONSTER.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `boards/MON/SEIN_MONSTER.md` contains the detailed 2026-05-07 Sein C/E follow-up correction task block.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:29` now creates C path segments during projectile movement, and `:52` routes C contact into delayed impact handling.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:529` implements delayed C impact; `:624` creates the C residual fire zone; `:358` and `:762` place E ash zones from actual hit targets.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User reported follow-up C and E active skill defects after the prior correction pass.
- 2026-05-07: Code Builder implemented and validated the follow-up correction.

## Task: 2026-05-07 Vega Active Skills A-E Runtime Implementation

### Task title

Track Monster-level evidence for Vega active skills A-E implementation.

### Goals

- Keep Monster-level state aligned with the Vega-specific task block.
- Record that Vega A-E active skills now have runtime behavior and data state.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue detailed Vega behavior from `boards/MON/VEGA_MONSTER.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `boards/MON/VEGA_MONSTER.md` contains the detailed Vega A-E implementation task block.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` implements Vega A-E active runtime paths.
- Unity-MCP runtime catalog inspection confirmed Vega A-E resolve as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User requested Vega active skills A-E implementation from the Vega reference folder.
- 2026-05-07: Code Builder implemented and validated the runtime behavior and data-state updates.

## Task: 2026-05-07 Vega Passive Skills F-J Runtime Implementation

### Task title

Track Monster-level evidence for Vega passive skills F-J implementation.

### Goals

- Keep Monster-level state aligned with the Vega-specific passive task block.
- Record that Vega A-J now resolves as runtime implemented in the CSV runtime catalog.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue detailed Vega behavior from `boards/MON/VEGA_MONSTER.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `boards/MON/VEGA_MONSTER.md` contains the detailed Vega F-J implementation task block.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` now contains passive helper paths for `vega-f`, `vega-g`, `vega-h`, `vega-i`, and `vega-j`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` and `Pakuri/Assets/Data/GameData/Monsters/vega.asset` mark Vega F-J as runtime implemented.
- Unity-MCP runtime catalog inspection confirmed Vega A-J resolve as `RuntimeImplemented`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User requested Vega passive skills F-J implementation from the Vega reference folder.
- 2026-05-07: Code Builder implemented and validated the runtime behavior and data-state updates.

## Task: 2026-05-07 Vega Projectile Sprite CSV Runtime Fix

### Task title

Track Monster-level state for Vega projectile sprite runtime source correction.

### Goals

- Record that Vega projectile visuals are CSV-runtime sourced.
- Keep common Monster guidance aligned with the Vega-specific sprite fix.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue detailed evidence from `boards/MON/VEGA_MONSTER.md`.
- Future monster sprite edits should compare SO fields, `monsters.csv`, and Unity runtime catalog output.

### Evidence

- `boards/MON/VEGA_MONSTER.md` contains the detailed Vega projectile sprite fix task block.
- Unity-MCP inspection showed Vega SO projectile sprite path `Assets/Image/Monster/Vega/Vega_Shoot2.png` but runtime catalog path `Assets/Image/Monster/Vega/Vega_Shoot_Temp.png` before the fix.
- `Pakuri/Assets/CSVdata/source/monsters.csv:7` and `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset:33` now point Vega projectile resolution at `Assets/Image/Monster/Vega/Vega_Shoot2.png`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User reported Vega still showed the old projectile sprite after assigning a new projectile sprite in the SO.

## Task: 2026-05-07 Vega B Target Rectangle Correction

### Task title

Track Monster-level state for Vega B target-centered rectangle correction.

### Goals

- Keep common Monster state aligned with the Vega-specific B behavior change.
- Record that Vega B is no longer a Vega-origin line hit.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Continue detailed behavior from `boards/MON/VEGA_MONSTER.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `boards/MON/VEGA_MONSTER.md` contains the detailed Vega B target rectangle correction task block.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:207` now uses `ApplyVegaTargetRectangleSlash(...)` instead of the old line slash path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:422` through `:441` applies Vega B damage, silence, and name marks inside the target-centered 3 by 1 rectangle.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: User requested a Vega B behavior correction after testing the implemented Vega skills.

# Task: 2026-05-07 Character Skill Effect Pipeline Review

### Task title

Monster character creation and skill structure review summary

### Goals

- Preserve monster-side conclusions from the character / skill / effect pipeline review.

### Constraints

- Evidence must come from inspected scripts and Unity-MCP output.
- Designer review only; no monster runtime implementation was performed.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- See `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- Future monster work should prefer stable skill/passive IDs and extracted monster skill runtime modules.

### Evidence

- `CombatRuntimeArielSkills.cs`, `CombatRuntimeEveSkills.cs`, `CombatRuntimeRinSkills.cs`, `CombatRuntimeSeinSkills.cs`, and `CombatRuntimeVegaSkills.cs` were found as `public partial class CombatRuntimeController`, not separate monster runtime classes.
- `PakuriDataManager.cs` provides monster, active skill, passive skill, and reward lookup dictionaries.
- Unity-MCP `execute_code` confirmed runtime catalog state: `catalogNull=False, managerSame=True, monsters=5, enemies=8, firstMonster=ariel, firstActive=5, firstPassive=5`.
- Report created at `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.

### History

- 2026-05-07: User requested current structure review for character creation, skills, and effects. Designer created an HTML report with monster-specific structure findings.
# Task: 2026-05-07 RunSession Learned Skill ID Refactor

### Task title

Monster active/passive learned-state checks use IDs.

### Goals

- Ensure monster skill availability and passive activation are driven by `SkillId`/`PassiveId`, not localized display names.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should verify each monster's unlocked active/passive behavior in Play Mode.

### Evidence

- `RunSession.Begin` now learns the default active by `SkillId`; Unity-MCP confirmed Ariel begins with `ariel-a`.
- `DebugSceneController.BuildDebugSession` now stores selected active/passive IDs.
- Monster passive helper fallbacks in Ariel, Eve, Rin, Sein, and Vega skill files now check `learnedPassiveSkillIds` by `passiveId`.
- Unity-MCP `execute_code` confirmed ID behavior: `hasSkillId=True, hasDisplayName=False`.

### History

- 2026-05-07: Code Builder implemented the first-priority report refactor so learned state is no longer display-name based.
