# COMBAT_BLACKBOARD

This is a domain-specific persistent state file created by the BLACKBOARD.md hierarchy migration.
When doing related work, follow MDTREE.md routing and update this file together with any required parent or child files.

## Migrated Task Blocks

## Task: 2026-05-08 Eve Unit Shared Skill Runtime Refactor

### Task title

Route selected and manifested Eve support skills through `CombatUnitRuntime`.

### Goals

- Continue the object-oriented unit runtime refactor requested by the user.
- Make selected EveUnit and manifested Eve units use `CombatUnitRuntime` plus `CombatSkillRuntime` for Eve B-E automatic skills.
- Keep Eve skill data sourced from each unit's `MonsterDefinition` and Offering state from each unit's `RunMonsterState`.

### Constraints

- Role Owner is Code Builder.
- This pass does not run Unity Play Mode; user verifies gameplay.
- Code Reviewer was not run because the user did not explicitly permit it.
- Eve A manual primary fire still has legacy selected-primary UI dependencies; B-E automatic support skills now use the shared caster path.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build/compile checks.

### Next Actions

- User verifies selected Eve and manifested Eve B-E behavior in Play Mode.
- Follow-up should move Eve A manual projectile state fully from selected-primary globals into `CombatSkillRuntime`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now calls `TryTickEveUnitSkill(...)` before the generic manifested skill path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` adds caster-based Eve unit skill methods for Prism Ray, Frost Field, Static Override, and Drone Beacon.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now triggers selected Eve automatic skills through `TryTriggerEveUnitAutomaticSkills(selectedUnitRuntime)`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now reads selected Eve panel cooldowns from selected `CombatSkillRuntime` values.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP refresh recovered successfully; Unity console error query reported no compile errors, but did report three existing `The referenced script (Unknown) on this Behaviour is missing!` entries.

### History

- 2026-05-08: User requested steps 1-4 of the refactor so EveUnit and 2P-5P units can own and execute their own skills instead of using separate selected-vs-manifested execution logic.

## Task: 2026-05-08 Manifested Eve Frost Field Runtime Parity

### Task title

Route manifested Eve Frost Field through persistent skill-effect runtime.

### Goals

- Complete step 6 from the manifested unit runtime refactor by binding the selected 1P object to `CombatUnitRuntime`.
- Make manifested Eve C use the same persistent field tick model as selected Eve C.
- Preserve manifested unit reward/enhancement state when applying Frost Field radius, duration, tick, damage, cooldown, chill, and freeze modifiers.

### Constraints

- Role Owner is Code Builder.
- Claims must be based on inspected code and command output.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that a manifested Eve using Offering-learned Frost Field applies repeated ice damage and chill/freeze status.
- Run Code Reviewer only if explicitly requested.
- Broader all-monster skill parity still requires extracting selected-monster private skill logic into unit-owned executors.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now attaches/configures `CombatUnitRuntime` on `eveAnchor` for the selected 1P monster and keeps HP/stat fields synchronized.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now routes manifested `eve-c` field casts into `SkillEffectRuntime` instead of the previous single area-damage path.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs` now detects `SkillEffectRuntime.ManifestedSource` and calls manifested effect damage/status handling before selected-Eve-only damage logic.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now stores `ManifestedSource` on persistent skill effects.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP script refresh was requested and Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.

### History

- 2026-05-08: User reported that selected Eve Frost Field applies ongoing chill damage, while manifested Eve Frost Field only deals one initial hit and does not apply chill/DoT.

## Task: 2026-05-05 MonsterPanel Skill State Snapshot API

### Task title

Expose selected Monster active skill state for scene MonsterPanel UI.

### Goals

- Provide UI-safe access to the selected Monster's learned active skills.
- Include skill icon, current magazine count, reload/shot cooldown, and non-magazine skill cooldown state for up to three active slots.
- Keep the runtime skill state source inside `CombatRuntimeController` instead of duplicating cooldown logic in UI code.

### Constraints

- Role Owner is Code Builder.
- The UI snapshot is read-only from the UI side.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies visual behavior in RunScene and DebugScene Play Mode, including current-ammo-only text and per-assigned-skill cooldown overlays.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now defines `MonsterPanelSkillView` and `GetMonsterPanelSkillViews(...)`.
- The snapshot uses existing runtime fields: `currentShotsRemaining`, `shotCooldown`, `reloadRemaining`, Eve/Ariel/Rin skill cooldown timers, and `FindSelectedSkill(...)` / `HasLearnedActive(...)`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` consumes this snapshot through `CombatMonsterPanelUiController`.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` now formats magazine display from `MonsterPanelSkillView.CurrentAmmo` only and clears text for non-magazine skills.
- `Pakuri/Assets/Scripts/Run/RunCombatUiController.cs` keeps cooldown overlay fill driven by each view's `CooldownRemainingRatio`, while icon fallback ignores `CooldownOverlay` images.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity refresh reached `resulting_state=idle`; Unity console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors.
- Follow-up builds completed with 0 errors after the TMP ammo text fix, and Unity console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-05: User requested MonsterPanel skill icon activation, magazine count, and cooldown/reload visual state.
- 2026-05-05: User requested the ammo UI to use one `Text (TMP)` current-count display and reported Active2/Active3 copied-state behavior; Builder kept the combat snapshot as the per-skill source and updated the UI consumer to avoid legacy `CountText` and overlay/icon cross-binding.

## Task: 2026-05-05 Debug Damage Attribute Popup Labels

### Task title

Show damage popup values with debug damage-attribute labels.

### Goals

- Make combat damage popups identify the damage attribute beside the damage number.
- Keep the popup text white, including mixed-damage Rin F follow-up popups.
- Use ` + ` between multiple damage terms when a combined popup represents mixed damage.

### Constraints

- Role Owner is Code Builder.
- The attribute notation is debugging-only display text.
- Current `DamageAttribute` code only contains `Physical`, `Fire`, `Lightning`, `Ice`, `Darkness`, and `Holy`; no unknown extra attribute was found.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that enemy and selected-Monster damage popups show labels such as `32(물리)`.
- User verifies Rin F mixed follow-up popups show combined text such as `32(물리) + 45(번개)`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/DamageCalculator.cs` defines the complete current `DamageAttribute` enum used for popup labels.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now has typed popup overloads and Korean label mapping for `Physical`, `Fire`, `Lightning`, `Ice`, `Darkness`, and `Holy`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs`, `CombatRuntimeArielSkills.cs`, `CombatRuntimeEveSkills.cs`, `CombatRuntimeRinSkills.cs`, and `CombatRuntimeEnemies.cs` now pass actual damage attributes into damage popup paths.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now returns total applied damage, including shield absorption, to keep debug popups and follow-up feedback aligned with applied damage.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP console error query returned only MCP-FOR-UNITY client handler logs, not project compile errors; Unity ready wait timed out after the compile request.
- `git diff --check` on changed combat and board files completed with no whitespace errors and CRLF conversion warnings only.

### History

- 2026-05-05: User requested readable debug damage-type labels and mixed-damage popup notation.
- 2026-05-05: Builder implemented typed debug damage popup labels, Rin F mixed popup composition, and local validation.

## Task: 2026-05-04 Rin D Execute-Only Combat Targeting

### Task title

Remove Rin D nearest-target fallback and add a simple circle hit effect.

### Goals

- Make Rin D combat targeting obey the execution-health threshold.
- Prevent non-executable enemies from being hit by Rin D.
- Add a visible hit effect using the existing circle effect path.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin D in Play Mode against enemies above and below the execution threshold.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Before this fix, `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs` `FindRinFinishingBlowTarget(...)` returned `executeTarget ?? fallback`, where `fallback` was the nearest living enemy inside range.
- `CombatRuntimeRinSkills.cs:604-632` now returns only an execution-threshold target.
- `CombatRuntimeRinSkills.cs:635-649` now centralizes Rin D threshold calculation as base 30%, trait 2 +10%, and master 2 -10%.
- `CombatRuntimeRinSkills.cs:651-667` adds `CreateRinFinishingBlowHitEffect(...)`, which uses `CreateCircleEffect(...)` and the existing circle sprite path to show a visible Rin D hit marker.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY client handler logs only.

### History

- 2026-05-04: User reported Rin D attacked 100% HP enemies and had no visible effect.
- 2026-05-04: Builder removed the fallback target path, clarified threshold handling, added the circle hit effect, and validated with build/Unity checks.

## Task: 2026-05-04 Rin Non-Magazine Combat Range

### Task title

Change Rin C/D/E combat targeting from short skill data range to battlefield-wide runtime range.

### Goals

- Make Rin non-magazine combat skills find targets anywhere on the current battlefield.
- Keep skill-specific areas such as Shockwave width and Collapse Strike damage radius intact.
- Ground the change in the existing selected-Monster combat runtime.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin C/D/E behavior in Play Mode against far enemies.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:196-211` now uses `GetRinMapWideSkillRange()` for Rin C target search and line length.
- `CombatRuntimeRinSkills.cs:297-298` now uses `GetRinMapWideSkillRange()` for Rin D target selection.
- `CombatRuntimeRinSkills.cs:367-368` now uses `GetRinMapWideSkillRange()` for Rin E target selection while preserving E's separate area radius.
- `CombatRuntimeRinSkills.cs:731-735` computes the map-wide range from `fieldSize`, `EnemySpawnX`, and `BattlefieldMaxY`, with a small padding.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors.
- The first parallel Editor build failed due to `CS2012` file locking from the simultaneous build, and the sequential rerun of `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY client handler logs only.

### History

- 2026-05-04: User reported Rin skills were too short and requested non-magazine skills to cover the whole map.
- 2026-05-04: Builder changed Rin C/D/E runtime target search to map-wide range and documented the rule in Rin skill markdown files.

## Task: 2026-05-04 Runtime Monster Sprite And Projectile Cleanup Follow-up

### Task title

Fix CSV-backed Rin/Sein runtime visuals and selected-Monster projectile cleanup timing.

### Goals

- Keep combat runtime monster visuals aligned with the CSV runtime data source.
- Make selected-Monster projectile cleanup happen at the battlefield X boundary rather than by short lifetime expiry.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the user did not explicitly request it for this change.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Rin/Sein visuals and selected-Monster projectile cleanup in Play Mode.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- Runtime data inspection showed Rin/Sein ScriptableObjects had sprites assigned, but the active CSV runtime source omitted their sprite paths.
- `Pakuri/Assets/CSVdata/source/monsters.csv` and `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now include Rin/Sein unit/projectile sprite mappings.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now uses the battlefield X boundary for selected-Monster no-hit projectile cleanup.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP assembly conflict warnings.
- Unity console error query returned only an MCP-FOR-UNITY client-handler exit log.

### History

- 2026-05-04: User reported Rin/Sein runtime sprite issues and Rin A short projectile disappearance.
- 2026-05-04: Builder fixed the runtime CSV/catalog data path and changed selected-Monster projectile cleanup to X-edge cleanup.

## Task: 2026-05-04 Rin A-E Active Runtime Implementation

### Task title

Wire Rin A-E active skills into the selected-Monster combat runtime.

### Goals

- Add Rin active skill cooldown, automatic cast, action-speed, magazine, and damage paths.
- Reuse existing combat runtime helpers for projectiles, line effects, circle effects, slows, knockback movement, and damage calculation.
- Preserve existing Eve and Ariel behavior while adding Rin dispatch branches.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented, Reviewer findings fixed, and locally validated. Code Reviewer has not been rerun because the user did not request another review.

### Next Actions

- User verifies Rin A-E combat behavior in Play Mode.
- Run another Code Reviewer pass only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:77`, `:152`, `:187`, `:287`, and `:357` implement Rin A-E combat behavior.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:466` updates Rin timed effects every frame, and `:547`/`:586` reset Rin combat timers on debug selection and prototype reset.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:49` resets Rin timers at combat-day start.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:70`, `:88`, `:108`, and `:128` add Rin to the existing selected-Monster dispatch points.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:52` now passes the physical damage actually applied by `ApplyDamageToEnemy(...)` into Rin projectile hit follow-up logic.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRinSkills.cs:502`, `:504`, and `:523` now use or return applied damage so Rin C/D/E and Howling follow-up damage cannot overcount capped shield/HP damage.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only existing Unity/MCP warnings.
- Unity-MCP script refresh reached idle and console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-04: Code Builder added Rin's selected-Monster active runtime and common dispatch integration.
- 2026-05-04: Code Builder fixed the Reviewer-reported elemental follow-up basis so Rin combat damage chains use applied damage.

## Task: 2026-05-03 Overhead Status Layout Follow-up

### Task title

Shorten the selected Monster HP bar, move enemy HP bars above sprites, and keep the layout tunable from combat controller fields.

### Goals

- Reduce the selected Monster HP bar width from the previous overly long automatic layout.
- Move enemy HP bars and labels high enough above enemy sprites to avoid overlap.
- Expose shared tuning values in `CombatRuntimeController` so the user can adjust selected-Monster and enemy overhead layout without another code edit.

### Constraints

- Role Owner is Code Builder.
- Ground the change in actual combat runtime code.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated with build plus Unity refresh/console checks.

### Next Actions

- User verifies in Play Mode that the selected Monster HP bar is no longer excessively wide.
- User verifies enemy HP bars no longer overlap enemy sprites and instead sit slightly above them.
- If the layout still needs tuning, user can edit the serialized `Selected Monster Status Layout` and `Enemy Status Layout` fields on `CombatRuntimeController`.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:235-259` now keeps selected-Monster auto bar width tighter with `selectedMonsterStatusAutoBarWidthMultiplier = 0.9f`, `selectedMonsterStatusAutoMaxBarWidth = 1.15f`, and adds the shared `Enemy Status Layout` tuning block.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:253-260` still rebuilds the selected Monster name/HP/bar stack from a computed layout, and `:344-375` now clamps the auto bar width between explicit min/max values.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:14-29` adds `EnemyStatusLayout`, while `:357-368` and `:435-455` now compute enemy label/bar positions from sprite bounds and apply them through `CreateEnemyLabel(...)` plus `ConfigureHpBarLayout(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Unity refresh completed with `resulting_state: idle`; Unity console error query returned MCP-FOR-UNITY client handler exit logs only.

### History

- 2026-05-03: User reported that the selected Monster HP bar was too long and enemy HP bars still overlapped enemy sprites.
- 2026-05-03: Code Builder tightened the selected-Monster width clamp, added enemy sprite-top layout calculation, and revalidated with build plus Unity refresh evidence.

## Task: 2026-05-03 Selected Monster Overhead Status Layout

### Task title

Separate the selected Monster name, HP text, and HP bar so they do not overlap, and scale their placement from the Monster sprite size.

### Goals

- Stop the selected Monster overhead name, HP text, and HP slider from overlapping each other.
- Base the overhead stack position and bar width on the current selected Monster sprite size instead of fixed hardcoded offsets only.
- Expose manual Inspector tuning values so the user can override the automatic layout if a specific sprite still needs hand adjustment.

### Constraints

- Role Owner is Code Builder.
- Ground the layout change in the current combat runtime scripts.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated with build plus Unity refresh/console checks.

### Next Actions

- User verifies in Play Mode that the selected Monster overhead name, HP text, and HP bar no longer overlap for the monsters they test.
- If a specific sprite still needs hand tuning, user can disable `Auto Layout Selected Monster Status` on `CombatRuntimeController` and edit the exposed manual local-position/scale fields directly in the Inspector.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:235-251` adds the serialized `Selected Monster Status Layout` tuning block, including automatic layout toggles and manual override fields such as `selectedMonsterStatusManualBarLocalPosition`, `selectedMonsterStatusManualHpTextLocalPosition`, `selectedMonsterStatusManualNameLocalPosition`, and `selectedMonsterStatusManualTextScale`.
- `CombatRuntimeController.cs:320-321` replaces the old single selected-Monster status label reference with separate `selectedMonsterNameLabel` and `selectedMonsterHpLabel` fields.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:253-283` now creates separate `MonsterNameLabel` and `MonsterHpLabel` TextMesh objects and reapplies bar layout every time the selected Monster status visuals are ensured.
- `CombatRuntimeScene.cs:324-339` now writes the name and HP text separately instead of one multiline label, while keeping the shared HP/shield bar update path intact.
- `CombatRuntimeScene.cs:344-380` now resolves selected-Monster overhead layout from the sprite bounds when automatic layout is enabled, and falls back to the serialized manual values when it is disabled.
- `CombatRuntimeScene.cs:554-585` adds `ConfigureHpBarLayout(...)` so bar position/width/height can be reapplied cleanly after the selected Monster changes.
- `CombatRuntimeScene.cs:444-469` also moves selected-Monster damage popup anchoring to the new topmost status labels so popup placement still follows the updated overhead stack.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP assembly conflict warnings.
- Unity refresh completed to `resulting_state: idle`; Unity console error query returned MCP-FOR-UNITY client handler exit logs only.

### History

- 2026-05-03: User reported that the selected Monster HP slider, HP text, and name overlap and asked for sprite-size-aware adjustment or a direct manual tuning path.
- 2026-05-03: Code Builder split the selected Monster overhead stack into separate name/HP labels, added sprite-based automatic layout, exposed manual Inspector overrides, and validated with build plus Unity refresh evidence.

## Task: 2026-05-03 Shared Combat Damage Popup Visual

### Task title

Show floating white damage numbers above both enemies and the selected Monster when they take damage.

### Goals

- Show the applied damage amount above the damaged unit's head for both enemy and player-side runtime targets.
- Reuse the existing combat TextMesh look instead of introducing a new font asset.
- Make each damage number rise slightly and fade out over 1 second.

### Constraints

- Role Owner is Code Builder.
- Ground the implementation in the current combat runtime scripts only.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly request it for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated with build plus Unity refresh/console checks.

### Next Actions

- User verifies in Play Mode that enemies and the selected Monster both show white floating damage numbers on hit.
- User verifies the numbers rise slightly and fade over about 1 second using the same readable font style already used by the combat labels.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs:143` adds the runtime popup state container, `:248` adds shared popup storage, `:422` updates popups every frame, and `:551` clears popup state during combat reset.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs:14` sets popup duration to `1f`; `:307`, `:351`, `:364`, and `:388` implement shared popup update/spawn paths for enemies and the selected Monster.
- `CombatRuntimeScene.cs:429-452` copies the existing `TextMesh` presentation from the target label template, including the existing font path when available, while keeping popup color white and `fontSize = 32`.
- `CombatRuntimeScene.cs:470` formats the displayed damage amount as a rounded integer string.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:392-402` and `CombatRuntimeScene.cs:252-276` remain the existing enemy/selected-Monster label sources that the popup text styling now reuses.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors after sandbox escalation; only the existing Unity/MCP assembly conflict warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity script refresh returned `resulting_state: idle`; Unity console error query returned only MCP-FOR-UNITY handler disposal/exit logs, not project compile errors.

### History

- 2026-05-03: User requested visible damage numbers above units on hit, using the existing font, white color, slight upward movement, and a 1-second fade.
- 2026-05-03: Code Builder added shared combat popup runtime state, wired both damage application paths, and validated the change with build plus Unity refresh evidence.

## Task: 2026-05-03 Ariel J Buff/Shield State Separation

### Task title

Separate Ariel J proclamation timing from the generic blessing timer and track Archangel shield state in combat runtime.

### Goals

- Stop Ariel J from piggybacking on unrelated blessing-state windows.
- Keep E master 1 damage-reduction timing separate from J post-cast action speed timing.
- Let combat damage resolution reduce the tracked Archangel shield share when the selected Monster shield absorbs damage.

### Constraints

- Role Owner is Code Builder.
- Ground the change in the current combat scripts and Ariel reference docs.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Builder follow-up implemented and locally validated. Code Reviewer has not been rerun because the user did not request another review.

### Next Actions

- User verifies in Play Mode that J holy-damage bonus disappears when the active pooled shield is no longer the E shield.
- User verifies Ariel E battlefield effect visibility and Ariel C held-input cadence behavior after the combat follow-up.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs:22-24` adds `arielSanctuaryProclamationTimer`, `arielArchangelShieldValue`, and `arielArchangelShieldTimer`.
- `CombatRuntimeArielSkills.cs:136-143` now updates those states per frame and clears expired Archangel shield tracking.
- `CombatRuntimeArielSkills.cs:429-451` now starts J proclamation timing directly from E cast, marks Archangel ownership through the shared shield helper, and spawns the missing battlefield-wide E visual.
- `CombatRuntimeArielSkills.cs:554-580` now resolves pooled-shield ownership inside `ApplyArielUnitShield(...)`, so J can only follow an E shield that actually owns the selected-Monster pooled shield state.
- `CombatRuntimeArielSkills.cs:592-600` and `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:315-319` still reduce the tracked E shield share during shield absorption.
- `CombatRuntimeArielSkills.cs:771`, `852`, and `898-900` now keep J holy-damage and action-speed checks on dedicated E/J state instead of generic shield/blessing paths.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs:332-356` now gates Ariel automatic support-skill retries to real firing windows, closing the held-input frame retry path behind the reported occasional C barrage.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP reference warnings.
- Unity refresh completed to `resulting_state: idle`; console errors were MCP-FOR-UNITY handler logs only.
- External Code Reviewer executed once and found that `CombatRuntimeArielSkills.cs:429-431` still recorded full E shield state even when `ApplyArielUnitShield(...)` left a larger older shield active in the pooled runtime value; Builder follow-up moved that decision into the shared shield helper and did not rerun review afterward.

### History

- 2026-05-03: User requested implementing Ariel F-J passive skills.
- 2026-05-03: Code Builder found the existing J runtime tied to the wrong timer/state and separated the combat-side buff/shield tracking.
- 2026-05-03: User explicitly requested Code Reviewer execution; Reviewer returned NEEDS_CHANGES for remaining E-shield source tracking leakage.
- 2026-05-03: User then requested fixing the reviewer finding and also reported missing Ariel E effect plus occasional Ariel C barrage behavior; Code Builder applied the combat follow-up and revalidated with build/refresh evidence.

## Task: 2026-05-02 Combat Skill Query Expansion

### Task title

Expand combat-side monster skill lookup to the new `PakuriDataManager` collection query contract.

### Goals

- Stop remaining combat consumers from reading selected-monster active skill arrays directly when the manager can resolve them.
- Reuse the same collection-level query helpers already used by run-scene UI/debug flows.
- Keep current Eve-specific runtime behavior unchanged while moving the data lookup contract.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer has not run yet for this follow-up phase.

### Role Owner

Code Builder

### Status

Implemented, locally validated, and later reviewed with no discrete actionable bug reported.

### Next Actions

- User can verify in Play Mode that Eve skill selection and runtime dispatch still work with manager-backed active skill lookup.
- If later requested, continue replacing direct monster-child array traversal in other combat runtime files.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEveSkills.cs:923` now gets active skills through `PakuriDataManager.Instance.GetActiveSkills(selectedMonster.MonsterId, selectedMonster)` before resolving the selected slot.
- `CombatRuntimeEnemies.cs` continues to use `PakuriDataManager.Instance.GetStageOneEnemies(gameDataCatalog)` from the earlier roster-level unification.
- `Select-String` over `Pakuri/Assets/Scripts/Combat/*.cs` found `PakuriDataManager` query calls in both `CombatRuntimeEnemies.cs` and `CombatRuntimeEveSkills.cs`.
- After the follow-up compile-fix pass, Unity refresh completed without C# compile errors, and `Pakuri/Validate CSV Source Data` still logged successful runtime catalog loading from `Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog`.
- External `codex review --uncommitted` later covered the modified combat-side file `CombatRuntimeEveSkills.cs` and reported no discrete actionable bug introduced by this patch.

### History

- 2026-05-02: User asked to finish the still-partial query-contract expansion after the earlier stage-one enemy pool unification.
- 2026-05-02: Builder changed Eve skill lookup to use the manager-backed active-skill query and revalidated the runtime loader path in Unity.
- 2026-05-02: The later reviewer pass inspected the modified combat-side query-expansion file and did not raise an actionable follow-up bug.

## Task: 2026-05-02 Combat Query Contract Unification

### Task title

Route stage-one enemy pool lookup through `PakuriDataManager`.

### Goals

- Remove direct combat reads of `gameDataCatalog.StageOneEnemies`.
- Reuse the same data-manager query contract used by run-entry/UI paths.
- Keep existing Stage 1 fallback enemy creation behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual script edits and actual Unity/editor output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- If later requested, continue the same pattern by moving more combat data pulls away from serialized catalog fields and toward stable ids or typed query services.
- User can verify in Play Mode that stage-one encounters still spawn the expected enemy roster.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs:103` now resolves the stage-one enemy pool with `PakuriDataManager.Instance.GetStageOneEnemies(gameDataCatalog)`.
- `CombatRuntimeEnemies.cs` still keeps the existing in-code fallback creation path for missing Stage 1 midboss/boss definitions, so this pass changed the query contract without changing encounter rules.
- After the change, the script-tree `Select-String` query for `gameDataCatalog.StageOneEnemies` no longer found combat consumer usage outside `PakuriDataManager`.
- Unity `read_console` after script refresh showed the CSV runtime catalog load log and no C# compile error entries.
- `Pakuri/Validate CSV Source Data` still loaded the runtime catalog with 5 monsters and 8 stage-one enemies after this combat-side query change.

### History

- 2026-05-02: User requested implementing the high-priority query-contract unification.
- 2026-05-02: Builder changed stage-one enemy pool resolution to use `PakuriDataManager` while preserving the current fallback encounter behavior.

## Task: 2026-05-02 Combat Catalog Source Resolution To CSV Runtime Data

### Task title

Switch combat startup catalog resolution to the new CSV runtime loader.

### Goals

- Make combat startup prefer the typed CSV runtime catalog instead of relying only on serialized scene references.
- Stop combat startup early when CSV parsing or validation fails.
- Keep current stage-one combat behavior intact while changing the data source path.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual combat script edits and actual build/console output.
- Do not run Unity Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User can verify current combat scenes still start correctly with the CSV-backed catalog.
- If stage-generalized enemy data is added later, extend the typed CSV source schema and combat loader together.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now resolves `gameDataCatalog` through `PakuriCsvRuntimeData.ResolveCatalogOrFallback(gameDataCatalog)` before using monster/enemy data.
- `PakuriCsvRuntimeData.EnsureInitialized()` runs before scene load and calls `Application.Quit()` on fatal CSV source or validation errors.
- `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from '...\\Pakuri\\data\\source' with 5 monsters and 8 stage-one enemies.`
- The generated typed CSV source set includes stage-one enemy catalog and data tables: `catalog_stage_one_enemies.csv` and `stage_one_enemies.csv`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and only the existing Unity/MCP warnings.

### History

- 2026-05-02: Code Builder updated the combat runtime controller to prefer the CSV-backed runtime catalog.
- 2026-05-02: Generated and validated the typed CSV source set used by combat startup.

## Task: 2026-05-01 Combat Structure Expansion Risk Review

### Task title

Review combat runtime structure for adding new monsters, stages, enemy families, and reward content.

### Goals

- Inspect the actual combat runtime scripts under `Pakuri/Assets/Scripts/Combat`.
- Identify hardcoded structure that will require code edits when content volume grows.

### Constraints

- Role Owner is Designer.
- Base all findings on actual script content and actual asset references.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- If the user requests implementation planning, split follow-up into stage/enemy data generalization, monster skill runtime strategy, and reward/prisoner subsystem extraction.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` resolves only `StageOneEnemies`, builds fallback Stage 1 enemies in code, and switches on `StageOneEnemySkillKind`, so enemy content is still Stage 1-specific instead of stage-agnostic.
- `Pakuri/Assets/Scripts/Data/EnemyDefinition.cs` encodes enemy skills as `StageOneEnemySkillKind` under a `[Header("Stage 1 Skill")]`, which tightly couples the SO model to one stage ruleset.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` routes manual fire through `IsSelectedEveMonster()` and `IsSelectedArielMonster()` before falling back to one generic projectile path, so new monster-specific B-E runtimes cannot be added data-only.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` and `CombatRuntimeEveSkills.cs` contain monster-ID-specific partial logic; `CombatRuntimeArielSkills.cs` explicitly branches between Eve and Ariel in shared cooldown and automatic-skill entry points.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` clamps `stageIndex` to `1..4`, keeps Eve-flavored default values, and initializes selected-monster runtime state inside one shared controller instance.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeRewards.cs` hardcodes gold/dark-trace reward values, stage multipliers, prisoner count rolls, and placeholder prisoner behavior text instead of reading reward data assets.
- Legacy non-English note retained these ASCII code references: `CombatRuntimeRewards.cs`.
- Current monster runtime supports one selected monster plus enemy units; party-wide passive descriptions exist in monster assets, but `CombatRuntimeScene.cs` still configures one selected monster only.

### History

- 2026-05-01: Reviewed `CombatRuntimeController.cs`, `CombatRuntimeScene.cs`, `CombatRuntimeEnemies.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeRewards.cs`, `CombatRuntimeEveSkills.cs`, and `CombatRuntimeArielSkills.cs` against current monster/enemy assets.

## Task: Ariel A Lifetime And Shield Bar Split Visual

### Task title

Fix Ariel A early projectile cleanup and change shield HP bars to fixed-width split segments.

### Goals

- Ariel A projectiles should not disappear after roughly half a second because of range/speed lifetime calculation.
- Shield visuals should share one fixed HP bar width with red HP and white shield segments.
- Preserve existing HP and shield numeric state.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode gameplay verification.
- Code Reviewer was not run because the latest request did not explicitly permit it.
- Codex did not control Play Mode; Unity editor state showed Play Mode active.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel A projectile lifetime and shield bar ratio behavior in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeArielSkills.cs` now keeps Ariel A lifetime at least `projectileLifetimeConfigured`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` now updates HP and shield as adjacent bar segments with `health + shield` as the visual total while shield is present.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeScene.cs` now applies the split bar helper to the selected Monster status bar.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same warnings.
- Unity console error query returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-04-30: User requested fixing Ariel A early deletion and changing all Monster shield-granting skills to a League-style single HP/shield bar visual.
- 2026-04-30: Code Builder implemented the projectile lifetime and shared shield bar visual changes.
- 2026-04-30: User requested HP Bar background black and reported Ariel A master `White Judgement` was not visibly applying. Code Builder changed shared HP bar backgrounds to `Color.black`, changed pending Ariel judgement explosions to trigger immediately on enemy hit, and made the explosion use a longer, higher-sorting circle-sprite visual.

## Task: Ariel Combat Runtime Skill Integration

### Task title

Integrate Ariel active/passive runtime behavior into the shared combat controller.

### Goals

- Add Ariel-specific skill runtime without changing Eve-specific skill behavior intentionally.
- Keep shared primary fire, cooldown, reload, projectile, shield, and status display paths working for both Eve and Ariel.
- Treat document "all allies" effects as selected-Monster effects until the runtime has an actual ally collection.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run without explicit user permission.

### Role Owner

Code Builder

### Status

Implemented and locally validated. Code Reviewer returned FAIL for Ariel behavior mismatches, and Code Builder has applied the requested correction pass.

### Next Actions

- User verifies Ariel skill behavior in Play Mode.
- If a future party/allied unit runtime is introduced, expand Ariel party-wide shield/blessing logic to that runtime collection.

### Evidence

- `CombatRuntimeArielSkills.cs` adds Ariel cooldown timers, A manual projectile, B/E shield application, C area damage/blessing, D Holy Exposure, E battlefield-wide damage, and F-J passive helpers.
- `CombatRuntimeProjectiles.cs` now calls selected-monster generic cooldown/action-speed/magazine helpers and routes Eve/Ariel A skills separately.
- `CombatRuntimeProjectiles.cs` now applies Ariel incoming damage reduction from E master 1 before selected Monster shield/HP damage.
- `CombatRuntimeEnemies.cs` now decrements and resolves Holy Exposure expiry and includes Holy Exposure in enemy status labels.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.
- Follow-up fixes addressed Reviewer findings: Ariel A uses skill A damage/range and projectile speed `17`; Ariel A master explosion is tied to projectile cleanup; Radiant Shield reflection receives the source enemy; and Holy damage bonuses are only applied in final damage calculation.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP warnings.
- Follow-up Unity refresh completed with editor `ready_for_tools=true`; console error query returned MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: Ariel A-E/F-J runtime integration was implemented after reading the actual Ariel skill documents and current combat partial scripts.
- 2026-04-30: User instructed Builder to fix Code Reviewer findings; Builder applied the Ariel correction pass and did not rerun Code Reviewer because a new review was not explicitly requested.

## Task: Hold Input Primary Skill Fire

### Task title

Use held pointer input for primary Monster combat firing.

### Goals

- Convert combat pointer input from press-once to held-input behavior.
- Keep existing combat cooldown, magazine, reload, and projectile collision behavior unchanged.
- Support both mouse and touch input paths.

### Constraints

- Role Owner is Code Builder.
- Ground claims in actual files and command output.
- Unity Play Mode gameplay verification remains user-owned.
- Code Reviewer was not run because the user did not explicitly request it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies held mouse/touch behavior in RunScene or DebugScene Play Mode.

### Evidence

- `CombatRuntimeHud.cs` changed from `Mouse.current.leftButton.wasPressedThisFrame` and `Input.GetMouseButtonDown(0)` to held input checks.
- `CombatRuntimeHud.cs` added Input System touchscreen primary-touch support and legacy `Input.touchCount` / `Input.GetTouch(0)` support.
- `CombatRuntimeProjectiles.cs` still controls repeated firing through `shotCooldown`, `currentShotsRemaining`, `reloadRemaining`, and `FirePrimarySkill()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing 2 Unity/MCP reference warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity editor state returned `ready_for_tools=true`; console errors were MCP-FOR-UNITY handler logs only.

### History

- 2026-04-30: User requested hold-to-fire behavior for the current 5 Monster A skills and their left-button/touch active trigger behavior.
- 2026-04-30: Code Builder implemented the change in the shared combat input path.

## Task: Combat Runtime Controller Split

### Task title

Rename and split `EveVerticalSliceController` into role-based combat runtime scripts.

### Goals

- Rename `EveVerticalSliceController` to a role-accurate `CombatRuntimeController`.
- Preserve the existing RunScene component connection by moving the original `.meta` to `CombatRuntimeController.cs.meta`.
- Split the large combat controller into partial scripts by responsibility without intentionally changing gameplay behavior.
- Keep current RunScene combat, reward, enemy, projectile, and HUD flows compiling.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Completed. Builder implementation, local validation, one external Code Reviewer run, user confirmation for intentional scene marker position, and user Play Mode verification are done.

### Next Actions

- Continue with the next implementation task selected by the user.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` was 87,832 bytes before the split.
- `EveVerticalSliceController.cs` was replaced by `CombatRuntimeController.cs` plus role-based partial files: `CombatRuntimeScene.cs`, `CombatRuntimeEnemies.cs`, `CombatRuntimeProjectiles.cs`, `CombatRuntimeRewards.cs`, and `CombatRuntimeHud.cs`.
- `CombatRuntimeController.cs.meta` uses the original script guid `e1c1fbd89ef220a499bf601ceaf19ced`, preserving the existing Unity MonoScript asset identity for the renamed controller.
- `RunCombatUiController.cs`, `RunFlowController.cs`, and `RunSceneBootstrap.cs` now reference `CombatRuntimeController`.
- `RunScene.unity` now records `Assembly-CSharp::Pakuri.Combat.CombatRuntimeController` in the controller component `m_EditorClassIdentifier`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity script refresh completed; console error query returned only MCP-FOR-UNITY client handler exit logs, not project script compile errors.
- `git diff --check` for the changed runtime files returned exit code 0 with CRLF warnings only.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES` in `codex_loop_logs/combat_runtime_split_reviewer_20260428.md`.
- Reviewer findings 1-2 point at stage-basic spawn-rule changes that were already reviewed as `PASS` in `codex_loop_logs/stage_basic_spawn_reviewer_20260428.md`.
- `Select-String` confirmed current `RunScene.unity` stores `EnemySpawnPoint` local position at `{x: 34.39, y: 8, z: 0}`.
- User confirmed the `EnemySpawnPoint` position was manually adjusted and should not be treated as a required fix.
- User reported Play Mode worked without notable problems after the rename/split.

### History

- 2026-04-28: User requested doing roadmap step 1 first, renaming `EveVerticalSliceController` according to its purpose and splitting scripts by role.
- 2026-04-28: Code Builder renamed the controller to `CombatRuntimeController`, split the large file into role-based partial scripts, updated runtime references, and completed local validation.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: NEEDS_CHANGES`; Code Builder is waiting for user instruction instead of auto-fixing.
- 2026-04-28: User confirmed the `EnemySpawnPoint` position was manually adjusted, so the scene marker position finding is accepted as intentional.
- 2026-04-28: User reported Play Mode worked without notable problems; task marked completed.

## Task: Combat Visual Sprite Assignment

### Task title

Allow monster/enemy ScriptableObjects and RunScene battlefield background to use editable sprites.

### Goals

- Add editable unit/projectile sprite references to monster and enemy ScriptableObjects under `Assets/Data/GameData`.
- Use assigned monster sprites for the selected monster and its projectiles at runtime.
- Use assigned enemy sprites for enemy bodies and enemy projectiles at runtime.
- Let `RunScene` use an editable battlefield background sprite without forcing the user's manual `BattlefieldBackground` scale.
- Keep unit body `SpriteRenderer.color` values white so assigned unit sprites are not tinted.
- Keep projectile, HP bar, marker, camera background, and battlefield background sprite colors white.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Preserve unrelated existing worktree changes.

### Role Owner

Code Builder

### Status

Builder implementation and local build/Unity console validation completed. User reported Play Mode verification completed. Unit, projectile, HP bar, marker, camera background, and battlefield background sprite color preservation was added. External Code Reviewer run was attempted but interrupted by the user and is not completed.

### Next Actions

- User assigns `UnitSprite` and `ProjectileSprite` on monster/enemy assets as needed.
- User assigns `BattlefieldBackgroundSprite` on `CombatRuntimeController` and adjusts `BattlefieldBackground` Transform Scale manually; keep `Auto Fit Battlefield Background To Field` off when manual scale should be preserved.
- Run Code Reviewer later if the user wants this visual-support change reviewed.

### Evidence

- `MonsterDefinition.cs` now exposes `UnitSprite` and `ProjectileSprite`.
- `EnemyDefinition.cs` now exposes `UnitSprite` and `ProjectileSprite`, and `CloneRuntimeCopy()` preserves both references.
- `CombatRuntimeScene.cs` now reads `MonsterDefinition.UnitSprite` and `MonsterDefinition.ProjectileSprite` into runtime selected sprite fields.
- `CombatRuntimeEnemies.cs` now uses `EnemyDefinition.UnitSprite` for enemy bodies and `EnemyDefinition.ProjectileSprite` for enemy projectiles, falling back to the generated shared sprite when no sprite is assigned.
- `CombatRuntimeProjectiles.cs` now uses the selected monster projectile sprite, falling back to the generated shared sprite when no sprite is assigned.
- `CombatRuntimeController.cs` now exposes `BattlefieldBackgroundAnchor`, `BattlefieldBackgroundSprite`, `BattlefieldBackgroundColor`, and `AutoFitBattlefieldBackgroundToField`.
- `CombatRuntimeScene.cs` now only rewrites `BattlefieldBackground.localScale` when `autoFitBattlefieldBackgroundToField` is true, so manual scale is preserved by default.
- `CombatRuntimeScene.cs` now applies `Color.white` to the selected monster body renderer.
- `CombatRuntimeEnemies.cs` now keeps enemy body renderer colors white in `UpdateEnemyColor()`.
- `CombatRuntimeProjectiles.cs` now applies `Color.white` to selected monster projectiles.
- `CombatRuntimeEnemies.cs` now applies `Color.white` to enemy projectiles and enemy HP bar background/fill sprites.
- `CombatRuntimeController.cs` now initializes marker and battlefield background color fields as `Color.white`.
- `CombatRuntimeScene.cs` now applies `Color.white` to the camera background and battlefield background renderer.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and only the existing 2 MCPForUnity/Unity reference warnings.
- Unity script refresh/compile was requested; console error query returned only MCP-FOR-UNITY client handler exit logs, not project compile errors.
- User reported Play Mode verification completed before the manual background scale fix.

### History

- 2026-04-28: User requested editable projectile images and monster images on `Assets/Data/GameData` enemy/monster SOs, plus an editable RunScene background image.
- 2026-04-28: Code Builder added sprite fields to monster/enemy definitions and wired runtime monster/enemy/projectile renderers to use them.
- 2026-04-28: User reported Play Mode verification completed but found `BattlefieldBackground` scale was forced on game start.
- 2026-04-28: Code Builder changed background auto-fit scaling to an opt-in serialized bool so manual `BattlefieldBackground` scale is preserved by default.
- 2026-04-28: User requested unit sprite colors stay white; Code Builder changed selected monster and enemy body renderers to keep `SpriteRenderer.color` white.
- 2026-04-29: User requested projectile, HP bar, marker, and background colors stay white; Code Builder changed those runtime color assignments to `Color.white`.

## Task: Stage Basic Enemy Spawn Rule Reset

### Task title

Reset RunScene enemy spawn positions to `stage-basic-rules.md`.

### Goals

- Treat the current RunScene battlefield as bottom-left `(0,0)` and top-right `(31,17)`.
- Treat `EnemySpawnPoint` X as `33`.
- Spawn normal enemies from X `33` with random Y in `0~17`.
- Spawn boss enemies from `(33,8)`.

### Constraints

- Role Owner is Code Builder.
- Ground all claims in actual files and command output.
- Do not run Unity-MCP Play Mode gameplay verification; user performs Play Mode verification.
- Run Code Reviewer once only after implementation.

### Role Owner

Code Builder

### Status

Builder implementation, local validation, and one Code Reviewer PASS completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Unity Play Mode that normal enemies spawn along Y `0~17` from X `33`, and bosses spawn near `(33,8)`.

### Evidence

- `Pakuri/reference/5.enemy/stage-basic-rules.md` says screen coordinates are `(0,0)` to `(31,17)`, default spawn X is `33`, normal monster Y is random `0~17`, and boss default point is `(33,8)`.
- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously serialized `enemySpawnYRange = new Vector2(6f, 10f)`.
- `SpawnEnemy()` previously used `enemySpawnAnchor.position` and applied the random Y range as an offset from `DefaultEnemySpawnPosition.y`.
- `EveVerticalSliceController.cs` now serializes `enemySpawnYRange = new Vector2(0f, 17f)`.
- `EveVerticalSliceController.cs` now defines `EnemySpawnX = 33f`, `BossSpawnY = 8f`, and `DefaultEnemySpawnPosition = new Vector3(EnemySpawnX, BossSpawnY, 0f)`.
- `ResolveEnemySpawnPosition(bool isBoss)` now forces X to `33`, uses Y `8` for bosses, and uses random Y from `enemySpawnYRange` for normal enemies.
- `Pakuri/Assets/Scenes/RunScene.unity` now stores `EnemySpawnPoint` at `{x: 33, y: 8, z: 0}` and `enemySpawnYRange: {x: 0, y: 17}`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity console error query returned only an MCP-FOR-UNITY client handler exit log, not a project script compile error.
- External Code Reviewer one-shot review returned `REVIEW_RESULT: PASS` in `codex_loop_logs/stage_basic_spawn_reviewer_20260428.md`.

### History

- 2026-04-28: User requested enemy spawn rules reset based on `Pakuri/reference/5.enemy/stage-basic-rules.md`, treating the RunScene field as `(0,0)` to `(31,17)` and `EnemySpawnPoint` X as `33`.
- 2026-04-28: Code Builder updated `EveVerticalSliceController.cs` and `RunScene.unity` to match the document rules.
- 2026-04-28: External Code Reviewer one-shot review returned `REVIEW_RESULT: PASS`; Play Mode gameplay verification remains user-owned.

## Task: EnemySpawnPoint Editable Position

### Task title

Allow scene-edited `CombatRoot/EnemySpawnPoint` position to persist when starting the game.

### Goals

- Stop runtime scene reference resolution from resetting an existing `EnemySpawnPoint` to the hardcoded default `(29, 8, 0)`.
- Keep default creation behavior for missing anchors.
- Make enemy spawn placement use the edited `EnemySpawnPoint` transform position, including vertical movement.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- User explicitly requested no Code Reviewer stage for this task; proceed with self-review only.
- All claims must be grounded in actual code and command output.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User moves `CombatRoot/EnemySpawnPoint` in `RunScene`, starts Play Mode, and verifies the marker no longer returns to `(29, 8, 0)`.
- User verifies spawned enemies appear around the edited `EnemySpawnPoint` position.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` previously called `EnsureChild(enemySpawnAnchor, "EnemySpawnPoint", new Vector3(29f, 8f, 0f))`.
- `EnsureChild()` previously assigned `current.position = worldPosition` and `existing.position = worldPosition`, which reset existing anchors.
- Added `DefaultEnemySpawnPosition` and changed `EnsureChild()` so existing `current` or found children are returned without overwriting their position.
- `SpawnEnemy()` now starts from `enemySpawnAnchor.position` and applies the configured Y random range as an offset from the default spawn Y, so edited spawn point Y also affects spawn placement.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- External Code Reviewer command was started but the user interrupted it and instructed to proceed with self-review only.

### History

- 2026-04-28: User reported that editing `Combat Root/EnemySpawnPoint` in the scene is reverted when starting the game.
- 2026-04-28: Confirmed reset cause in `EveVerticalSliceController.ResolveSceneReferences()` and `EnsureChild()`.
- 2026-04-28: Changed existing anchor handling to preserve scene-authored positions and adjusted enemy spawn placement to use the anchor transform as the base position.
- 2026-04-28: Per user instruction, skipped Code Reviewer and kept only Builder self-review plus build verification.

## Task: Monster And Enemy Hp Slider Bars

### Task title

Add overhead HP text and HP slider bars for Stage 1 enemies and the selected Player Monster.

### Goals

- Add a simple HP slider-style bar above enemies using existing/basic Unity-rendered assets.
- Add the same kind of name, HP text, and HP bar above the selected Player Monster.
- Keep HP text/bar updates tied to the current runtime health values.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.
- Do not import new visual assets for this request; use the existing generated 1x1 shared sprite path in `EveVerticalSliceController`.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. External Reviewer execution was attempted but could not complete because the Codex CLI reported a usage limit. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that enemies show name, HP text, and HP bar above their heads.
- User verifies in Play Mode that the selected Player Monster shows name, HP text, and HP bar above the Monster.
- User verifies the bars shrink as HP decreases for both enemies and the selected Player Monster.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` `EnemyRuntime` now stores `HpBarFill`.
- `EveVerticalSliceController.cs` now stores `selectedMonsterLabel` and `selectedMonsterHpBarFill`.
- `EnsureSelectedMonsterStatusVisuals()` creates/reuses `MonsterHpLabel` and `MonsterHpBar` under `eveAnchor`.
- `SpawnEnemy()` creates `EnemyHpBar` under each spawned enemy, and `UpdateEnemyLabel()` updates both text and bar fill.
- `CreateHpBar()`, `EnsureHpBarPart()`, and `UpdateHpBarFill()` implement the shared world-space HP bar with `SpriteRenderer` and the existing shared 1x1 sprite.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query returned MCP-FOR-UNITY client handler entries only, not project script compile errors.
- External Reviewer command was attempted with `codex.exe exec --skip-git-repo-check`; it failed with a Codex usage-limit message and did not produce a review verdict.

### History

- 2026-04-27: User requested HP Slider Bar using basic assets and the same name/HP display for Player Monster as enemies.
- 2026-04-27: Implemented world-space SpriteRenderer HP bars for enemies and selected Player Monster in `EveVerticalSliceController.cs`.
- 2026-04-27: Attempted external Code Reviewer execution. The command exited before review due to Codex usage limit, so only local Builder self-review, build, Unity refresh, and console checks are available for this turn.

## Task: Enemy Target Priority Monster First

### Task title

Enemy combat flow targets the selected Monster before the Nexus.

### Goals

- Enemies should move toward and attack the Monster before attacking the tower/Nexus.
- If the Monster HP reaches 0, enemies should fall back to the existing Nexus target and Nexus defeat flow.
- Keep the change grounded in the existing `EveVerticalSliceController` combat flow.

### Constraints

- Role Owner is Code Builder.
- User will run Play Mode verification.
- Do not claim gameplay verification; only build, Unity refresh, console check, and self-review are performed here.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Stage 1 enemies approach the selected Monster first.
- User verifies that Monster HP decreases before Nexus HP, and Nexus starts taking damage only after Monster HP reaches 0.

### Evidence

- `Pakuri/Assets/Scripts/Combat/EveVerticalSliceController.cs` now calls `GetEnemyPriorityTarget()` in `UpdateEnemies()` before moving or attacking.
- `GetEnemyPriorityTarget()` returns `eveAnchor` while `unitCurrentHealth > 0f`, then falls back to `nexusAnchor`.
- Enemy damage skills now call `ApplyEnemyDamageToPriorityTarget()`, which subtracts from `unitCurrentHealth` first and from `nexusCurrentHealth` only after Monster HP is depleted.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query showed MCP-FOR-UNITY transport/client handler entries only; no project script compile error was returned.

### History

- 2026-04-27: User requested enemies attack Monsters before hitting the tower.
- 2026-04-27: Confirmed the existing `UpdateEnemies()` flow targeted only `nexusAnchor` and the existing damage function subtracted only `nexusCurrentHealth`.
- 2026-04-27: Changed enemy movement and damage target selection to prefer the Monster while alive, with Nexus fallback after Monster HP reaches 0.
- 2026-04-27: Follow-up self-review fixes applied. Enemy attacks now resolve through `EnemyAttackResolver`, Monster defenses are cloned into runtime target defenses, enemy critical passive bonuses are copied from enemy stats into runtime, and fallback Stage 1 enemy ScriptableObjects are cached with `HideFlags.DontSave`.
- 2026-04-27: Ranged and melee/ranged enemies now fire enemy projectiles. HP damage is resolved only when those projectiles collide with the Monster or Nexus. Enemies now create a simple overhead `TextMesh` label showing name and HP.

## Task: Enemy Projectile And Overhead HP Display

### Task title

Ranged enemies use projectiles and enemies show simple overhead name/HP labels.

### Goals

- Ranged enemies should no longer damage the Monster or Nexus immediately at attack time.
- Enemy projectiles should apply HP damage only after touching the Monster or Nexus target.
- Enemies should show a simple overhead name and HP text.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.

### Role Owner

Code Builder

### Status

Builder implementation and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that Archer/Rogue/Hero Karin style ranged attackers spawn visible enemy projectiles.
- User verifies Monster/Nexus HP changes only when enemy projectiles reach the target.
- User verifies enemy overhead labels remain readable enough and update HP after taking damage/healing.

### Evidence

- `EveVerticalSliceController.cs` `ProjectileRuntime` now has enemy projectile fields: source enemy, target transform, and Monster/Nexus target flag.
- `TryUseStageOneEnemySkill()` now routes `EnemyAttackType.Ranged` and `EnemyAttackType.MeleeAndRanged` default attacks through `FireEnemyProjectile()`.
- `UpdateProjectiles()` now branches enemy projectiles into `TryHitEnemyProjectileTarget()`, which applies Monster or Nexus damage only on collision.
- `SpawnEnemy()` now creates an overhead `TextMesh` through `CreateEnemyLabel()`, and `UpdateEnemyLabel()` writes enemy name and current/max HP.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity refresh reached idle. Console error query returned MCP-FOR-UNITY transport/client handler entries only, not project script compile errors.

### History

- 2026-04-27: User requested ranged enemy projectiles with collision-based HP damage and simple overhead enemy name/HP display.
- 2026-04-27: Implemented enemy projectile runtime path and overhead TextMesh labels in `EveVerticalSliceController.cs`.

## Task: Combat Script Self-Review Fixes

### Task title

Fix self-review findings for Monster defense, enemy critical passives, God Script pressure, and fallback enemy allocation.

### Goals

- Apply Monster attribute defenses when enemies damage the Monster.
- Make enemy critical chance/damage passive fields participate in damage resolution.
- Reduce `EveVerticalSliceController` responsibility by moving enemy attack damage resolution into a helper.
- Avoid creating new fallback Stage 1 enemy ScriptableObjects on every combat initialization.
- Skip text-encoding changes because user confirmed current text is not an issue.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- All claims must be grounded in actual files and command output.

### Role Owner

Code Builder

### Status

Builder fixes and self-review completed. Waiting for user Play Mode verification.

### Next Actions

- User verifies in Play Mode that enemy hits against Monster now use Monster defense and that archer/rogue critical passives can affect damage.
- Future cleanup should continue splitting `EveVerticalSliceController`; current change only extracts enemy attack damage resolution.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/EnemyAttackResolver.cs`.
- `EveVerticalSliceController.cs` now stores `selectedMonsterDefenses`, clones `monster.Defenses`, and passes them to `EnemyAttackResolver.ResolveAgainstMonster`.
- Enemy runtime now copies `CriticalChanceBonus` and `CriticalMultiplierBonus` from `CombatStatBlock` deltas and existing Stage 1 passives add onto those fields.
- Fallback Stage 1 enemy creation now uses static `fallbackStageOneEnemyCache` and `HideFlags.DontSave`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity assembly version warnings.
- Unity initially reported missing `EnemyAttackResolver`; `manage_asset import` for `Assets/Scripts/Combat/EnemyAttackResolver.cs` generated/imported the MonoScript asset, and the later console error query showed only MCP-FOR-UNITY client handler entries.

### History

- 2026-04-27: User asked to fix self-review findings in order, excluding the text-encoding item.
- 2026-04-27: Implemented enemy attack damage helper, Monster defense application, enemy critical passive participation, and fallback enemy cache.

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

## Task: 2026-05-07 Sein Runtime Correction Pass

### Task title

Record combat-runtime evidence for Sein A/B/C/E correction pass.

### Goals

- Track the shared projectile-runtime changes made for locked-target Sein C projectiles.
- Track the selected-monster panel/runtime changes needed for Sein B magazine state.
- Preserve validation evidence for future combat-runtime continuation.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future combat work should check `boards/MON/SEIN_MONSTER.md` and `boards/COMBAT/PROJECTILE_BLACKBOARD.md` for detailed Sein behavior notes.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` now includes locked-target projectile metadata and B magazine panel state handling for Sein.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeProjectiles.cs` now resolves locked-target projectiles through `ResolvePlayerProjectileDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP `execute_code` confirmed `sein-b` is read as `MagazineProjectile` in both `sein.asset` and runtime catalog.

### History

- 2026-05-07: Code Builder implemented the Sein A/B/C/E correction pass after user gameplay feedback.

## Task: 2026-05-07 Sein C/E Combat Follow-up Correction

### Task title

Record combat-runtime evidence for Sein C delayed residual and E ash-zone targeting fixes.

### Goals

- Track C delayed skill-effect explosion and residual fire zone expiry behavior.
- Track E `Ashen Sky` zone placement on actual hit target positions.
- Preserve build and Unity console evidence.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future combat work should check `boards/MON/SEIN_MONSTER.md`, `boards/COMBAT/PROJECTILE_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:529` creates delayed C impact effects with `TickRemaining` set from C delay.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:624` creates `SeinFallingTrajectoryResidual` after C impact expiry.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeSeinSkills.cs:358` and `:762` create E ash zones from actual hit enemies.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP refresh/compile was requested; console error query returned existing missing-script reference errors and MCP client handler logs, not C# compile errors.

### History

- 2026-05-07: User reported C delayed explosion/path/residual issues and E ash-zone placement issue.
- 2026-05-07: Code Builder implemented and validated combat runtime corrections.

## Task: 2026-05-07 Vega Active Combat Runtime

### Task title

Record combat-runtime evidence for Vega A-E implementation.

### Goals

- Track Vega mark, silence, buff, area slash, and execute behavior in combat runtime.
- Preserve validation evidence for future Vega continuation.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future Vega combat work should check `boards/MON/VEGA_MONSTER.md`, `boards/COMBAT/PROJECTILE_BLACKBOARD.md`, and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs` implements Vega A-E active runtime paths and shared Vega damage/mark helpers.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeController.cs` adds Vega runtime state fields to `EnemyRuntime`, `ProjectileRuntime`, and `SkillEffectRuntime`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeEnemies.cs` decrements Vega silence, displays `이름표식`/`침묵`, and prevents silenced enemies from using active skills.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: Code Builder implemented Vega combat runtime behavior for active skills A-E.

## Task: 2026-05-07 Vega Passive Combat Runtime

### Task title

Record combat-runtime evidence for Vega F-J passive implementation.

### Goals

- Track Vega passive damage modifiers, defense reduction, crit bonus, cooldown charge, and timed vulnerability state.
- Preserve validation evidence for future Vega continuation.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- Future Vega combat work should check `boards/MON/VEGA_MONSTER.md`, `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`, and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:798` applies Vega passive final-damage modifiers for name-marked, silenced, area-vulnerable, and Final Sentence-vulnerable targets.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:846` applies F physical defense reduction for 10+ name-mark stacks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:862` applies G critical chance bonus for silenced and marked targets.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:963` charges Vega cooldowns after Final Sentence kills.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.

### History

- 2026-05-07: Code Builder implemented Vega combat runtime behavior for passive skills F-J.

## Task: 2026-05-07 Vega B Combat Rectangle Correction

### Task title

Record combat-runtime correction for Vega B target-centered instant rectangle damage.

### Goals

- Replace Vega B's line-beam damage from Vega's position with direct target-centered area damage.
- Keep the temporary rectangle dimensions at width 3 and height 1.
- Preserve B's silence/name-mark/damage modifier hooks.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega B in Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:178` still hosts `TryCastVegaSilentGreatblade()`, but it now chooses a nearest enemy target and centers the area on that target.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:207` and `:210` apply immediate target-centered rectangle damage for the base B hit and B master-1 extra hit.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:422` through `:441` performs the rectangle hit loop and applies damage, silence, and name marks.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeVegaSkills.cs:595` creates the visible rectangle effect by centering a 3 by 1 `CreateLineEffect(...)` visual on the target, not by starting from Vega.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing Unity/MCP reference warnings.
- Unity-MCP script refresh completed to idle and the console error query returned only MCP client-handler logs.

### History

- 2026-05-07: User clarified Vega B should immediately hit an area on the enemy, not draw a straight line from Vega to the enemy.

### History

- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `rg`, `Get-ChildItem`, `Get-Content`.
- Legacy non-English note retained these code references: `Pakuri/reference/run-systems-integration-summary-report.html`, `Pakuri/reference/Report/run-systems-integration-summary-report.html`.
- Legacy non-English note summarized in English; see the surrounding task block for retained status and evidence.
- Legacy non-English note retained these code references: `Pakuri/reference/Report/combat-monster-enemy-implementation-plan.html`.

# Task: 2026-05-07 Character Skill Effect Pipeline Review

### Task title

Combat skill and effect structure review summary

### Goals

- Preserve combat-side conclusions from the character / skill / effect pipeline review.

### Constraints

- Evidence must come from inspected scripts and Unity-MCP output.
- Designer review only; no combat code implementation or Play Mode verification was performed.

### Role Owner

Designer

### Status

Completed.

### Next Actions

- See `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.
- Future implementation should start with `CombatEffectFactory` extraction before larger skill runtime module separation.

### Evidence

- `CombatRuntimeController.cs` contains internal runtime classes for enemies, projectiles, skill effects, drones, and damage popups, plus shared lists including `enemies`, `projectiles`, `skillEffects`, and `drones`.
- `CombatRuntimeEveSkills.cs` defines shared `CreateLineEffect` and `CreateCircleEffect`; other monster skill files call these helpers and add results to `skillEffects`.
- `Select-String` under `Pakuri/Assets/Scripts` found `PakuriDataManager` and `RunSceneBootstrap`, but no factory/service/dispatcher class names for combat skill or effect orchestration.
- Report created at `Pakuri/reference/Report/2026-05-07-character-skill-effect-pipeline-review.html`.

### History

- 2026-05-07: User requested current combat skill/effect pipeline review. Designer documented concentration risk in `CombatRuntimeController` partial files and recommended staged extraction.
# Task: 2026-05-07 RunSession Learned Skill ID Refactor

### Task title

Combat learned skill checks use stable active/passive IDs.

### Goals

- Align combat skill availability and passive checks with ID-based `RunSession` learned state.

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

- User should Play Mode verify that learned B-E active skills and F-J passives still activate for each monster.

### Evidence

- Changed `CombatRuntimeController.cs` learned-state sets from name-oriented fields to `learnedActiveSkillIds` and `learnedPassiveSkillIds`.
- Changed `CombatRuntimeScene.cs` to resolve selected slot A active ID and slot F passive ID for the current monster.
- Changed `CombatRuntimeEveSkills.cs` so `ConfigureEveSkillSelectionState` reads session learned IDs and `HasLearnedActive(SkillSlot)` checks `skill.SkillId`.
- Changed passive helper fallbacks in `CombatRuntimeArielSkills.cs`, `CombatRuntimeEveSkills.cs`, `CombatRuntimeRinSkills.cs`, `CombatRuntimeSeinSkills.cs`, and `CombatRuntimeVegaSkills.cs` to check `learnedPassiveSkillIds` by `passiveId`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and 2 existing Unity/MCPForUnity reference warnings.
- Unity-MCP `execute_code` result: `monster=ariel, activeSkillId=ariel-a, firstLearnedActive=ariel-a, hasSkillId=True, hasDisplayName=False`.

### History

- 2026-05-07: Code Builder updated combat learned-state checks after the user requested the report's first-priority refactor.

# Task: 2026-05-07 Combat Effect Factory Refactor

### Task title

Extract combat line/circle effect object creation into `CombatEffectFactory`.

### Goals

- Move GameObject/SpriteRenderer creation for line and circle combat effects out of the monster skill partial methods.
- Use `SkillEffectPrefab` when a skill definition provides one.
- Preserve the existing temporary SpriteRenderer fallback when no prefab is assigned.
- Keep current `SkillEffectRuntime` ticking, collision checks, and lifetime behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected files and build/Unity console output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Ariel/Eve/Rin/Sein/Vega temporary skill effects still appear.
- Future work can move effect lifetime/pooling into `CombatEffectFactory` after visual parity is confirmed.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/CombatEffectFactory.cs` and Unity imported it as a `MonoScript` with guid `37536b92138a46f4b8ec5097ed3dd0a5`.
- `CombatEffectFactory.CreateLine(...)` and `CreateCircle(...)` instantiate `SkillEffectPrefab` when present, otherwise create the same SpriteRenderer fallback using the shared line/circle sprites.
- `CombatRuntimeEveSkills.cs` now delegates `CreateLineEffect(...)` and `CreateCircleEffect(...)` object creation to `CombatEffectFactory` while still returning `SkillEffectRuntime`.
- Ariel, Eve, Rin, Sein, and Vega direct active-skill effect paths now pass `skill.SkillEffectPrefab` into the shared effect creation path where the skill definition is available.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and the existing `System.Net.Http` / `System.IO.Compression` conflict warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP initially reported `CombatEffectFactory` missing before Unity imported the new script; after `manage_asset import` and script compile request, Unity console query returned only MCP client handler logs and no project compile errors.

### History

- 2026-05-07: User requested implementing the current first-priority recommendation from `2026-05-07-character-skill-effect-pipeline-review.html`: introduce `CombatEffectFactory` or `CombatEffectService`.

# Task: 2026-05-07 Combat Effect Prefab Scale Preservation

### Task title

Preserve original prefab scale for assigned skill-effect prefabs.

### Goals

- Let animated effect prefabs use their Inspector-authored root scale.
- Keep the existing line/circle fallback SpriteRenderer scale behavior when no prefab is assigned.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Do not change damage, hit detection, effect lifetime, or fallback visuals.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that assigned animation prefabs render at their authored scale.
- If per-skill visual scale needs data control later, add an explicit effect scale field rather than using combat range/radius.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Combat/CombatEffectFactory.cs` always set line effect scale to `(length, width, 1)` and circle effect scale to `(diameter, diameter, 1)` even when `prefab != null`.
- `CombatEffectFactory.CreateLine(...)` now applies `(length, width, 1)` only when `prefab == null`.
- `CombatEffectFactory.CreateCircle(...)` now applies `(diameter, diameter, 1)` only when `prefab == null`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- The first parallel `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` failed with `CS2012` because the parallel Assembly-CSharp build locked `obj\Debug\Assembly-CSharp.dll`; the sequential rerun completed with 0 errors and the same existing warnings.
- Unity-MCP imported `CombatEffectFactory.cs` and refreshed scripts to editor-ready state.
- Unity-MCP console error read returned only MCP client handler logs, not project compile errors.

### History

- 2026-05-07: User requested changing the effect factory so prefab animations appear at their original authored scale instead of being resized by line/circle effect dimensions.

# Task: 2026-05-07 Monster Skill Runtime Module Dispatch Refactor

### Task title

Introduce monster skill runtime modules behind `CombatRuntimeController` dispatch.

### Goals

- Stop `CombatRuntimeController` from directly listing Ariel/Eve/Rin/Sein/Vega at every skill runtime update/reset/selection dispatch point.
- Add an `IMonsterSkillRuntime` and `MonsterSkillRuntimeBase` module layer for Ariel, Eve, Rin, Sein, and Vega.
- Keep the current proven skill behavior intact while creating a safer boundary for later moving per-monster state out of controller partial files.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected files and build/Unity-MCP output.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Ariel/Eve/Rin/Sein/Vega active skill cooldowns, automatic casts, effects, magazine counts, and resets still behave the same.
- Future refactor can move one monster at a time from the wrapper module into a real standalone state owner, starting with the smallest or least coupled monster.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/CombatMonsterSkillRuntime.cs`, imported by Unity as a `MonoScript` with guid `2ab9f9b7913d4c3982f10f2d31d56de0`.
- `CombatMonsterSkillRuntime.cs` defines `IMonsterSkillRuntime`, `MonsterSkillRuntimeBase`, and Eve/Ariel/Rin/Sein/Vega runtime module classes.
- `CombatRuntimeController.cs:492` now calls `UpdateMonsterSkillRuntimeEffects()` instead of directly calling all five `Update*SkillEffects()` methods.
- `CombatRuntimeController.cs:551` and `:571` now call `ConfigureMonsterSkillRuntimeSelectionState(session)` instead of the Eve-named selection-state method directly.
- `CombatRuntimeController.cs:572`, `:910`, and `CombatRuntimeEnemies.cs:47` now reset through `ResetMonsterSkillRuntimes()`.
- `CombatRuntimeArielSkills.cs:56`, `:61`, `:66`, and `:72` now dispatch selected-monster cooldown, automatic trigger, magazine capacity, and action-speed lookup through `GetSelectedMonsterSkillRuntime()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP imported `Assets/Scripts/Combat/CombatMonsterSkillRuntime.cs`; after script compile request, editor state reported `ready_for_tools=true` and console error query returned only MCP client-handler logs, not project compile errors.

### History

- 2026-05-07: User requested the next refactor step: split monster skill runtime modules so the controller passes context and modules manage their own skill state. Builder implemented the safe first stage by adding the module interface/base and routing controller dispatch through modules while preserving existing partial skill behavior.

# Task: 2026-05-08 Manifested Monster Limited Combat Join

### Task title

Let Manifested party monsters join combat with automatic A/basic attacks only.

### Goals

- Read Manifested party members from `RunSession` at the next combat start.
- Spawn simple party visuals for Manifested monsters.
- Run only the Manifested monster's A skill/basic attack on an automatic cooldown.
- Keep selected monster combat behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- This is an initial limited combat join, not a full independent monster runtime.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify target selection, damage timing, and 2P+ panel visibility.
- Future work can replace the instant line hit with full projectile/effect/runtime-module behavior after the limited join is accepted.

### Evidence

- Added `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs`.
- `CombatRuntimeParty.cs` stores Manifested runtime entries separately from `selectedMonster`.
- `CombatRuntimeParty.cs` uses `PakuriDataManager.Instance.ResolveMonster(...)` against `RunSession.ManifestedMonsterIds`, skips the selected monster, and caps Manifested party members at 4.
- `CombatRuntimeParty.cs` exposes `PartyMonsterCount` and `GetPartyMonsterPanelSkillViews(...)` for the RunScene MonsterPanel.
- `CombatRuntimeParty.cs` finds the nearest living enemy and applies A-skill/basic attack damage through `DamageCalculator.Resolve(...)` and `ApplyDamageToEnemy(...)`.
- `CombatRuntimeController.cs` updates Manifested combat after monster skill effects and clears Manifested party state on prototype reset.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing warnings.

### History

- 2026-05-08: User requested Manifested monsters be read from party state and initially join combat with restricted automatic A/basic behavior.

# Task: 2026-05-08 Manifested Party Baseline HP And Stat State

### Task title

Make Manifested party entries explicitly carry their own HP and stats.

### Goals

- Keep Manifested monsters initialized from their own `MonsterDefinition` stats.
- Preserve limited automatic A/basic attack behavior.
- Preserve nearest-living-enemy target selection.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected combat code and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify Manifested monster target choice and A/basic damage pacing in the next combat after Manifest success.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` already used `ResolveManifestedPrimarySkill(...)` to select `SkillSlot.A`.
- `CombatRuntimeParty.cs` already used `FindNearestManifestedMonsterTarget(...)` to choose the closest living enemy by `Vector2.Distance`.
- `CombatRuntimeParty.cs` now stores `MaxHealth`, `CurrentHealth`, `BaseDamage`, and `PowerStat` per Manifested runtime entry from that monster's `MonsterDefinition`.
- `CombatRuntimeParty.cs` now resets those Manifested HP/stat values at combat reset and uses the stored `PowerStat`/`BaseDamage` in A/basic damage calculation.
- `CombatRuntimeParty.cs` now displays Manifested monster HP in the party label.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP script refresh returned `ready_for_tools=true`; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Manifested monsters were being created in order but their implementation felt wrong, and clarified they should start from their own stats/HP and A skill while auto-attacking the nearest enemy.
- 2026-05-08: Code Builder added explicit Manifested HP/stat runtime state while preserving nearest enemy A/basic attack behavior.

# Task: 2026-05-08 Manifested Monsters Use Registered Party Skills

### Task title

Stop Manifested monsters from using fake/unregistered skill behavior.

### Goals

- Build Manifested combat runtime from the monster's run party-member state.
- Sync learned active skill IDs from `RunSession.RunMonsterState` into actual `SkillDefinition` runtime entries.
- Auto-cast the Manifested monster's learned skills at the nearest living enemy.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected combat code and build/Unity-MCP output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that Manifested monsters no longer repeat an unregistered weird skill and instead use their registered learned skill definitions.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:18` stores `RunSession.RunMonsterState State` per Manifested runtime.
- `CombatRuntimeParty.cs:30` defines `ManifestedSkillRuntime` for actual learned skill runtime entries.
- `CombatRuntimeParty.cs:229` and `:259` sync learned skills before combat update/label update.
- `CombatRuntimeParty.cs:280` targets the nearest living enemy with `FindNearestManifestedMonsterTarget(...)`.
- `CombatRuntimeParty.cs:287` fires the current learned `SkillDefinition` through `FireManifestedMonsterSkill(...)`.
- `CombatRuntimeParty.cs:321` applies the registered skill's ID/name/effect behavior path.
- `CombatRuntimeParty.cs:371` resolves base damage from the Manifested monster state and skill data.
- `CombatRuntimeParty.cs:395` resolves cooldown from the registered skill data and reward modifiers.
- `CombatRuntimeParty.cs:462` syncs party-member learned active IDs to actual monster `ActiveSkills`.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP refresh completed with editor `ready_for_tools=true`; console warning/error read showed only MCP client handler logs.

### History

- 2026-05-08: User clarified Manifested monsters should behave like added starting monsters and not as users of unregistered weird skills.
- 2026-05-08: Code Builder replaced the fake single-skill Manifested loop with registered learned-skill syncing and nearest-enemy auto-casting.

# Task: 2026-05-08 Manifested CombatRoot Slots

### Task title

Use authored `CombatRoot` monster slots for Manifested combat.

### Goals

- Use `CombatRoot/2PMonster`, `3PMonster`, `4PMonster`, and `5PMonster` in Manifest order.
- Preserve nearest-enemy auto attack and registered A/default skill state.
- Keep scene slots alive and only activate/deactivate them across combat resets.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from Unity-MCP scene inspection, combat code, build output, and console output.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that the first Manifested monster appears in `2PMonster`, the next in `3PMonster`, and unused slots stay inactive during combat.

### Evidence

- Unity-MCP found scene paths `CombatRoot/2PMonster`, `CombatRoot/3PMonster`, `CombatRoot/4PMonster`, and `CombatRoot/5PMonster`.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:41` defines the slot name array.
- `CombatRuntimeParty.cs:139` resolves a slot for each Manifested runtime.
- `CombatRuntimeParty.cs:160` ensures a label under the chosen slot.
- `CombatRuntimeParty.cs:285` preserves authored scene-slot positions on combat reset.
- `CombatRuntimeParty.cs:572` deactivates scene slots instead of destroying them.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.
- Unity-MCP console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User corrected the implementation target from generated party objects to authored `2PMonster` through `5PMonster` scene slots.
- 2026-05-08: Code Builder changed Manifested combat runtime to bind those slots.
# Task: 2026-05-08 Manifested Monster Projectile Magazine Runtime

### Task title

Make Manifested monster A projectile skills use projectile, magazine, and reload state instead of instant line attacks.

### Goals

- Stop Manifested A skills from rendering as thin beam/line effects.
- Give Manifested `MagazineProjectile` skills their own ammo, shot interval, and reload state.
- Keep Manifested projectile damage separate from the selected 1P monster projectile label/passive path.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected code, build output, and Unity-MCP console output.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify that Manifested Eve fires an Arc Bolt-style projectile with ammo/reload pacing instead of a Prism Ray-like beam.
- User should verify other Manifested A projectile monsters show projectile sprites and pause for reload after their magazine empties.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:32` now stores Manifested per-skill ammo, magazine capacity, shot cooldown, shot interval, reload remaining, and reload duration.
- `CombatRuntimeParty.cs:335` updates Manifested skill timers and routes magazine projectile skills through `TryFireManifestedMagazineSkill(...)`.
- `CombatRuntimeParty.cs:418` through `:463` fires Manifested magazine skills only when not reloading and not between shots, then decrements ammo and starts reload when empty.
- `CombatRuntimeParty.cs:465` through `:550` creates real Manifested projectile GameObjects using the Manifested monster's `ProjectileSprite` and `ProjectileColor`.
- `CombatRuntimeProjectiles.cs:50` through `:79` handles `IsManifestedProjectile` separately from selected 1P projectiles.
- `CombatRuntimeParty.cs:552` through `:611` resolves Manifested projectile hits with `DamageCalculator.Resolve(...)` without selected-monster passive hooks.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings.
- Unity-MCP refresh reached `resulting_state=idle`; console warning/error read showed only MCP-FOR-UNITY client handler logs.
- Unity-MCP `validate_script` reported duplicate-signature diagnostics because it inspected partial class files in isolation; the actual runtime and editor builds completed with 0 errors.

### History

- 2026-05-08: User reported Manifested monsters still fired a thin beam, had no magazine, and behaved like a line skill instead of using each monster's A projectile skill.
- 2026-05-08: Code Builder added Manifested projectile, ammo, shot interval, reload, and isolated projectile-hit handling.

# Task: 2026-05-08 Manifested Vega A Three-Sword Runtime Follow-up

### Task title

Make Manifested Vega A use the registered three-sword projectile cadence.

### Goals

- Keep Manifested monsters using their registered learned active skills.
- Add Vega A-specific Manifested runtime behavior: three projectile shots per magazine shot, 0.12 second internal interval, last projectile 2x damage, and name-mark application.
- Keep the generic Manifested projectile path for other monsters.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Manifested Vega fires three sword projectiles per A attack and that Offering-acquired skills fire for Manifested monsters.

### Evidence

- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:43` through `:46` adds queued Vega projectile state to `ManifestedSkillRuntime`.
- `CombatRuntimeParty.cs:456` routes Manifested `vega-a` through `QueueManifestedVegaThreeSwordFlurry(...)` instead of the generic one-projectile path.
- `CombatRuntimeParty.cs:747` through `:774` queues three projectiles, fires the first immediately, spaces later shots by `VegaThreeSwordBulletInterval`, and applies 2x damage to the third shot.
- `CombatRuntimeParty.cs:581`, `:625`, and `:627` carry and apply Vega name-mark stacks on Manifested projectiles.
- Unity-MCP `execute_code` confirmed runtime catalog `vega-a` resolves as `MagazineProjectile`, magazine `5`, shot interval `0.55`, with a non-null projectile sprite.
- Runtime and Editor `dotnet build` commands completed with 0 errors and existing warnings.

### History

- 2026-05-08: User confirmed Manifested A projectiles were applying, but reported Manifested Vega A did not start with Vega's three-projectile basic attack.
- 2026-05-08: Code Builder added Manifested Vega A-specific projectile burst behavior while preserving generic Manifested projectile handling for other monsters.
# Task: 2026-05-08 Manifested Skill Visual Runtime Unification Follow-up

### Task title

Route Manifested non-projectile skills through skill-kind effect visual dispatch.

### Goals

- Replace the Manifested-only generic beam visual for non-projectile learned skills.
- Use the same `CombatEffectFactory` line/circle creation path used by selected monster skill visuals.
- Preserve Manifested projectile handling and Vega A burst handling.

### Constraints

- Role Owner is Code Builder.
- Evidence must come from inspected combat code, build output, and Unity-MCP output.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User should Play Mode verify Manifested learned non-projectile skills from Offering.

### Evidence

- Before this change, `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs:512` called a Manifested-only line visual helper after all non-projectile skill casts.
- `CombatRuntimeParty.cs:512` now calls `CreateManifestedSkillVisual(runtime, skill, target)`.
- `CombatRuntimeParty.cs:896` through `:944` dispatches visual shape by `SkillRuntimeKind`: area/field, buff/shield, execute/mark, line, and default fallback.
- `CombatRuntimeParty.cs:946` and `:958` create visuals through `CombatEffectFactory.CreateCircle(...)` and `CombatEffectFactory.CreateLine(...)`, matching the selected monster effect factory path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP script refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-08: User reported Offering-acquired Manifested skills had cooldowns but showed only a beam instead of the monster-specific effect.
- 2026-05-08: Code Builder replaced the Manifested generic non-projectile visual with skill-kind dispatch and removed the unused beam helper.

# Task: 2026-05-08 Manifested Sustained Skill Duration Follow-up

### Task title

Keep Manifested sustained skill visuals alive for their real monster skill duration.

### Goals

- Replace short hardcoded Manifested non-projectile effect lifetimes for duration skills.
- Preserve the existing Manifested projectile path and Vega A three-projectile burst.
- Give Manifested Eve E a timed drone object instead of only a generic projectile shot.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Manifested Prism Ray, Frost Field, Drone Beacon, and other sustained learned skills in RunScene Play Mode.

### Evidence

- Before this fix, `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` used hardcoded `0.24f`, `0.28f`, and `0.32f` lifetimes in `CreateManifestedSkillVisual(...)`.
- `CombatRuntimeEveSkills.cs` stores selected Eve durations as `EveBeamDuration = 1.2f`, `EveFrostFieldDuration = 4f`, and `EveDroneDuration = 5f`.
- `CombatRuntimeParty.cs` now uses `ResolveManifestedSkillVisualDuration(...)` for Manifested line, field, area, buff, shield, execute, and fallback visuals.
- `CombatRuntimeParty.cs` now adds `ManifestedDroneRuntime`, updates it from `UpdateManifestedMonsterPartyCombat()`, and clears it from `ClearManifestedMonsterParty()`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- Unity-MCP refresh completed; console warning/error read returned only an MCP client handler log.

### History

- 2026-05-08: User reported sustained Manifested skill effects were ending much earlier than their original monster skill duration.

# Task: 2026-05-08 Manifested Unit Component Runtime Refactor

### Task title

Move 2P-5P Manifested unit HP and skill runtime state onto unit components.

### Goals

- Keep 1P selected monster behavior unchanged for the later step 6 decision.
- Attach a `CombatUnitRuntime` component to each manifested `2PMonster` through `5PMonster` slot at runtime.
- Move manifested per-skill cooldown, magazine, reload, and queued Vega projectile state into `CombatSkillRuntime`.
- Keep projectile/effect creation and damage application inside `CombatRuntimeController` as the battlefield service.

### Constraints

- Role Owner is Code Builder after Designer handoff.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in RunScene Play Mode that 2P-5P Manifested monsters still fire learned skills, apply Offering upgrades, and show HP/ammo/cooldown state.
- Step 6 can migrate 1P/EveUnit onto the same component pattern if the Play Mode result is acceptable.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Added `Pakuri/reference/Report/2026-05-08-manifested-unit-runtime-refactor-design.md`.
- Added `Pakuri/Assets/Scripts/Combat/CombatSkillRuntime.cs` for per-skill cooldown, magazine, reload, and queued projectile state.
- Added `Pakuri/Assets/Scripts/Combat/CombatUnitRuntime.cs` as a `MonoBehaviour` owning manifested monster, run state, HP/stat snapshot, and learned skill runtimes.
- `Pakuri/Assets/Scripts/Combat/CombatRuntimeParty.cs` now uses `List<CombatUnitRuntime>` for manifested monsters and binds/creates `CombatUnitRuntime` on the manifested slot object.
- `CombatRuntimeParty.cs` now calls `runtime.TickManifestedCombat(Time.deltaTime)`, and `CombatUnitRuntime` calls `CombatRuntimeController.TickManifestedUnitSkill(...)` for battlefield actions.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- Unity-MCP imported `CombatSkillRuntime.cs` and `CombatUnitRuntime.cs` as `MonoScript` assets, forced script refresh to ready, and console error query returned only MCP client-handler logs, not project compile errors.

### History

- 2026-05-08: User asked to perform steps 1-5 of the object-oriented manifested runtime refactor and leave step 6 for later.
