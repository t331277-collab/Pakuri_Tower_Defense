## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/VEGA_MONSTER.md`.

# VEGA_MONSTER

## Scope

Vega dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Vega file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Vega active skills A-E and passive skills F-J are implemented and locally validated.

## Task: 2026-05-14 Vega NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Vega prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Vega_Unit` through `NewRunSceneEntryManager`.
- Verify Vega creates an exact `vega` runtime model and initializes `MonsterUnitActor`.
- Make Vega's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Vega combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified. 2026-05-18 Vega active skill CSV rows were updated to the new skill-owned projectile/status schema. 2026-05-18 Vega design-only labels remain non-runtime statuses with `status_chance=0`.

### Next Actions

- User verifies Vega selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Vega_Unit.prefab` in `vegaUnitPrefab`.
- Unity-MCP verification returned `vega:prefab=Vega_Unit|modelOk=True|model=vega|actor=True|actorModel=True|hpText=HP 225/225|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Vega_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `vega-a` `projectile_speed=16`, `magazine_capacity=5`, `reload_seconds=4.8`, and `shot_interval_seconds=0.55`, matching `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` stores `vega-a` `pierce_count=999` as the current finite-runtime sentinel for the reference document's `무한 관통`, because `InGameProjectileActor` currently consumes integer pierce counts.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` records Vega labels `이름표식`, `침묵`, `몰살 허가`, and `이름표식 연계`; these are design labels because the current `StatusEffectKind` enum does not include those Vega-specific statuses.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `vega-b` `radius=1.8`, matching `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now keeps Vega design labels `이름표식`, `침묵`, `몰살 허가`, and `이름표식 연계` with `status_chance=0`; runtime CSV validation rejects positive chance on unsupported status labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` can resolve supported labels from `status_effect_label` if a Vega row is intentionally edited to a supported status such as `감전` later.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.
- 2026-05-18: Code Builder moved Vega projectile/status tuning into skill CSV rows and encoded the reference infinite pierce as `pierce_count=999` for the current finite common projectile runtime.
- 2026-05-18: Code Builder normalized Vega design-only status labels to chance 0 and added supported status-label fallback/CSV sync batch support.

## Task: 2026-05-13 Vega Battlefield Facade Registration

### Task title

Route Vega battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Vega skill behavior while replacing direct battlefield list registration writes.
- Keep Vega projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Vega skills in Play Mode if needed.

### Evidence

- `CombatRuntimeVegaSkills.cs:706` now calls `AddBattlefieldProjectile(...)`.
- `CombatRuntimeVegaSkills.cs:888`, `:905`, and `:919` now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Vega battlefield object registration through facade methods.

## Task: 2026-05-10 Vega Unit Executor Migration

### Task title

Move Manifested Vega skill execution onto Vega unit executor paths.

### Goals

- Dispatch Manifested Vega A-E through Vega-specific `CombatUnitRuntime` / `CombatSkillRuntime` paths instead of the generic manifested fallback.
- Keep Vega A three-sword behavior while adding unit-owned Extermination Permit state.
- Make Manifested Vega B-E/F-J read the source Vega unit's Offering/passive state for silence, name marks, execute, vulnerability, critical, defense reduction, and cooldown charge.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Do not run Unity Play Mode from Codex.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution for this task.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds and Unity-MCP compile/console checks.

### Next Actions

- User verifies Manifested Vega A-E and F-J interactions in RunScene Play Mode, especially B silence/name marks, C action/attack buff, D area vulnerability/cooldown charge, E mark consumption/survivor vulnerability/kill cooldown charge, and A master afterimage.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:630` dispatches Manifested Vega through `TryTickVegaUnitSkill(...)` before the generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeVegaSkills.cs:139` implements the Vega unit skill dispatcher.
- `CombatRuntimeVegaSkills.cs:445`, `:507`, `:548`, and `:616` implement unit-owned Vega B/C/D/E active paths.
- `CombatRuntimeVegaSkills.cs:1068` implements `TryApplyVegaUnitProjectileHit(...)` for Vega projectile passive damage/critical/defense behavior.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:36` store Vega unit buff/charge state.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing System.Net.Http/System.IO.Compression warnings after a first parallel run hit only an `obj\Debug\Assembly-CSharp.dll` file lock.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings.
- `git diff --check` over the three changed scripts completed with exit code 0 and only LF-to-CRLF warnings.
- Unity-MCP script refresh reached `resulting_state=idle`; console warning/error read returned only MCP-FOR-UNITY client handler logs.

### History

- 2026-05-10: User requested Vega unit executor migration based on section 4 of `Pakuri/reference/Report/2026-05-08-monster-oop-refactor-manifested-work-status.html`.
- 2026-05-10: Code Builder added Vega unit dispatch, state, active paths, projectile hit damage hooks, and validation evidence.
