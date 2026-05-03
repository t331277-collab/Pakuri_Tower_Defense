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

