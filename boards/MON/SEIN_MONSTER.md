## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-09 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/SEIN_MONSTER.md`.

# SEIN_MONSTER

## Scope

Sein dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Sein file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Active Sein task history is recorded below.

## Task: 2026-05-27 Sein-B Manual Burst And Projectile Hold Input Fix

### Task title

Fix Sein-B manual burst continuation and add projectile-only manual hold firing in `NewRunScene`.

### Goals

- Make `sein-b` complete its full burst sequence from one manual click even when player auto-skill is off.
- Allow manual projectile skills to keep firing while the mouse button is held, using the current cursor direction at each shot.
- Keep beam, zone, and single-attack skills on their existing one-click manual behavior.

### Constraints

- Role Owner is Code Builder.
- No CSV, prefab, or scene serialization change is part of this fix; the issue is resolved in runtime input ownership only.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `sein-b` fires all 5 burst shots from one click in manual mode.
- User verifies that holding the mouse continues firing projectile skills toward the current cursor direction, while non-projectile active skills still react only to click-start.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors `sein-b` with `magazine_capacity=4`, `shot_interval_seconds=0.18`, and `projectile_burst_count=5`; no Sein-B CSV row change was needed.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now stores latched manual projectile input, continues manual execution while a projectile runtime is bursting, and limits hold-repeat behavior to `ProjectileSkillData`.
- The same combat manager now refreshes projectile manual aim from the current cursor position while the mouse button is held, but leaves non-projectile manual skills on `wasPressedThisFrame` behavior.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors when rerun alone after a first parallel-build file lock on `Assembly-CSharp.dll`.

### History

- 2026-05-27: User reported that Sein-B fired only one manual projectile after DebugUI learn, even though the skill CSV authored a 5-shot burst.
- 2026-05-27: Code Builder confirmed the burst CSV/runtime data were already correct and fixed the manual-input ownership path so projectile bursts can continue without enabling full auto-skill mode.

## Task: 2026-05-27 Sein-C And Sein-D Enhancement/Master Runtime Completion

### Task title

Implement Sein-C and Sein-D enhancement/master behavior on shared projectile, multi-effect, status, and zone runtime paths.

### Goals

- Convert `sein-c` from area-attack authoring to delayed-impact projectile authoring using the shared projectile runtime.
- Implement Sein-C trait/master rows and Sein-D trait/master rows through current shared choice/effect/status paths where possible.
- Reuse shared persistent-zone spawning for Sein-C master-1 and Sein-D master-2 instead of adding Sein-only runtime branches.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprints are `boards/SkillBluePrint/projectile-blueprint.md` for Sein-C and `boards/SkillBluePrint/area-attack-blueprint.md` for Sein-D.
- User explicitly approved widening scope to shared runtime/common-logic extension and CSV schema expansion where needed.
- The following values are user-provided or inferred from the nearest inspected authority and should stay explicit until the user replaces them:
  - `sein-c.projectile_speed=20` is inferred from the requested reuse of `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
  - `sein-a-hit-mark` duration `5s` is inferred for Sein-C trait-5 gating.
  - `sein-c-master-1` residual zone radius `1.2` and tick interval `0.5s` are inferred; the user only specified `25%` damage for `1.5s`.
  - `sein-d-master-2` residual zone radius `3.2` and tick interval `0.5s` reuse inspected base Sein-D values as the nearest available authority.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-C now fires the requested projectile visual, stops on first contact, then explodes after the delay.
- User verifies Sein-C master-2 contact damage and visual, Sein-C master-1 residual flame-zone spawn, and Sein-D master-2 residual ember-zone spawn.
- If the inferred Sein-C/Sein-D zone radius or tick values should change, update the authored effect rows rather than adding new runtime branches.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/c-flame-trajectory.md` and `Pakuri/reference/2.Monster/sein/skill/d-superheated-zone.md` were the inspected skill references for the requested Sein-C and Sein-D behavior bundle.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now sets `sein-c.runtime_kind=CooldownProjectile`, `sein-c.projectile_speed=20`, `sein-c.damage_delay_seconds=0.8`, and `sein-c.skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_C.prefab`; it also sets `sein-d.skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_D.prefab`, `sein-d.active_duration_seconds=4`, `sein-d.shot_interval_seconds=0.5`, and `sein-d.status_effect_id=sein-d-heat-stack`.
- The same skill CSV now marks `sein-a` with `status_effect_id=sein-a-hit-mark`, `status_chance=1`, `status_duration_seconds=5`, `status_max_stacks=1`, and `status_stack_amount=1` so Sein-C trait-5 can stay on a shared conditional-status path.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `damage_delay_multiplier`, updates `sein-c-trait-4` to `0.6`, gates `sein-c-trait-5` on `sein-a-hit-mark`, and authors `sein-c-master-1`, `sein-c-master-2`, and `sein-d-master-2` as `SharedExtension`; it also updates `sein-d-trait-1`, `sein-d-trait-2`, `sein-d-trait-5`, and `sein-d-master-1` with the shared duration / interval / conditional-damage fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now includes `active_duration_seconds` and `tick_interval_seconds` columns plus `sein-c-master2-contact`, `sein-c-master1-zone`, and `sein-d-master2-zone` effect rows using `Assets/Prefab/Skill/Sein/Sein_C_Master-2.prefab`, `Assets/Prefab/Skill/Sein/Sein_C_Master_1.prefab`, and `Assets/Prefab/Skill/Sein/Sein_D_Master_2.prefab`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now contains `sein-a-hit-mark` and `sein-d-heat-stack`.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps monster `sein` skill `sein-c` through `EffectManager` to the requested flying-arrow prefab `Assets/Prefab/Skill/Sein/Sein_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` now supports projectile contact-stop, delayed impact, on-hit follow-up effects, on-expire follow-up effects, and delayed area resolution.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now resolves shared `OnHit` / `OnExpire` multi-effects for projectiles, uses scene `EffectManager` projectile visuals before effect prefabs, and creates a projectile actor even when only delayed-impact behavior is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now supports `EventTarget` damage/status targeting and shared persistent damage-zone spawning from effect rows.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now recognizes `sein-a-hit-mark` and `sein-d-heat-stack` as shared runtime statuses.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; existing `MSB3277` warnings remain.
- Unity console filtering after `Pakuri/Validate CSV Source Data` and `Pakuri/Sync CSV Runtime Catalog Assets` showed `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.` and `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.` and did not show a `Pakuri` CSV failure in the retrieved entries.

### History

- 2026-05-27: User requested Code Builder / Skill Builder implementation for Sein-C and Sein-D enhancement/master behavior with explicit prefab paths for projectile, explosion, and zone visuals.
- 2026-05-27: User approved shared projectile delayed-impact expansion and shared residual-zone reuse instead of a helper active-skill row approach.

## Task: 2026-05-26 Sein-B Enhancement And Master Runtime Completion

### Task title

Implement Sein-B enhancement choices and master effects through the shared burst projectile and shared consecutive-hit extension paths.

### Goals

- Mark Sein-B trait 1-4 and master 1-2 as implemented through existing shared projectile choice modifiers.
- Add a reusable shared consecutive-hit damage extension for projectile skills.
- Implement Sein-B trait 5 on that shared consecutive-hit path with +8% same-target consecutive-hit damage up to +40%.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/projectile-blueprint.md`.
- User explicitly approved widening scope to a new shared runtime/common-logic extension and new CSV columns for projectile consecutive-hit behavior.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-B trait 1-4 and master 1-2 modify burst count, damage, reload speed, shot interval, and crit chance as expected.
- User verifies in Play Mode that Sein-B trait 5 deals no bonus on the first hit to a target, then gains +8% per same-target consecutive hit up to +40%, and resets when the hit target changes.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines Sein-B trait 1 burst count +2, trait 2 damage +25%, trait 3 reload speed +30%, trait 4 shot interval -25%, trait 5 same-target consecutive hit damage +8% up to +40%, master 1 burst count +4 with damage -20%, and master 2 burst count -2 with damage +90% and crit chance +20%.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now adds `consecutive_hit_bonus_rate` and `consecutive_hit_max` columns, marks `sein-b-trait-1` through `sein-b-trait-5` and `sein-b-master-1` through `sein-b-master-2` as `RuntimeImplemented`, sets `sein-b-trait-5` to `consecutive_hit_bonus_rate=0.08` and `consecutive_hit_max=0.4`, and sets `sein-b-master-2` to `crit_chance_bonus=0.2`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` now carry the new consecutive-hit choice fields through the CSV runtime definition/build path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillChoiceEffectSpec.cs`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs`, and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now carry those fields through the shared choice-to-snapshot path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks the last projectile hit target and repeat count and resolves a shared same-target consecutive-hit damage multiplier from choice snapshot data, with fallback to `ProjectileSkillData` fields when present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs` and `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now apply that shared consecutive-hit multiplier on both prefab projectile hits and direct-hit fallback projectile damage.
- CSV field-count validation returned `monster_skill_choices.csv HEADER_WIDTH=88 FIELD_COUNT_OK`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors after a first parallel file-lock retry; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity editor validation logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity runtime catalog inspection returned `sein-b-trait-5|state=RuntimeImplemented|dmg=False:1|burst=0|crit=0|consec=0.08:0.4` and `sein-b-master-2|state=RuntimeImplemented|dmg=True:1.9|burst=-2|crit=0.2|consec=0:0`.

### History

- 2026-05-26: User asked Skill Builder to implement Sein-B enhancement and master effects using `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md`.
- 2026-05-26: Builder confirmed master 2 crit chance already fit the shared choice path, but trait 5 same-target consecutive-hit damage required a new shared projectile runtime extension and new choice CSV columns.
- 2026-05-26: User approved widening scope to a reusable shared consecutive-hit extension and new CSV columns, and Builder implemented the shared path plus Sein-B choice wiring.

## Task: 2026-05-26 Sein-A Enhancement And Master Runtime Completion

### Task title

Implement Sein-A enhancement choices and master effects on the shared projectile and hit-trigger SingleAttack paths.

### Goals

- Mark Sein-A trait 1-5 and master 1 as implemented through existing shared projectile choice modifier fields.
- Implement Sein-A master 2 as an OnOutgoingDamage hit trigger that runs a shared SingleAttack explosion.
- Use `Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab` for the master-2 small explosion effect.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- Selected blueprint is `boards/SkillBluePrint/projectile-blueprint.md`.
- User pointed out the existing hit-trigger SingleAttack common runtime, so master-2 stays on that shared trigger path instead of adding Sein-only logic.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that Sein-A trait 1-5 and master 1 modify damage, magazine, reload speed, pierce, and shot interval as expected.
- User verifies in Play Mode that Sein-A master 2 spawns `Sein_A_Master-2.prefab` and deals 50% Fire explosion damage on Sein-A hits.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md` defines Sein-A as a projectile / magazine basic attack with trait 1 damage +25%, trait 2 magazine +4, trait 3 reload speed +30%, trait 4 pierce +1 and damage +10%, trait 5 shot interval -20% and damage -10%, master 1 damage +55% and pierce +1, and master 2 50% Fire small explosion on hit.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `sein-a-trait-1` through `sein-a-trait-5`, `sein-a-master-1`, and `sein-a-master-2` as `RuntimeImplemented`.
- The same choice rows use existing shared projectile choice fields: `damage_multiplier`, `magazine_bonus`, `reload_time_multiplier`, `pierce_bonus`, and `shot_interval_multiplier`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now adds `sein-a-master2-hit-explosion` with `trigger_event=OnOutgoingDamage`, `requires_active_choice_id=sein-a-master-2`, `event_skill_id=sein-a`, `trigger_action=SingleAttack`, `damage_source=EventAppliedDamage`, `damage_source_multiplier=0.5`, `attribute=Fire`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab`.
- `Pakuri/Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab` exists and contains a `BoxCollider2D`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now dispatches source-owned `OnOutgoingDamage` triggers before the existing passive-owner trigger scan, enabling active-skill choice-gated hit triggers without Sein-only branches.
- CSV field-count validation returned `FIELD_COUNT_OK` for `monster_skill_choices.csv` 86 columns / 252 rows, `monster_skill_triger.csv` 44 columns / 27 rows, and `monster_skills.csv` 57 columns / 52 rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remain.
- Unity `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- Unity editor code inspection returned all Sein-A trait/master rows as `RuntimeImplemented` and returned `trigger=sein-a-master2-hit-explosion|event=OnOutgoingDamage|action=SingleAttack|source=sein-a|choice=sein-a-master-2|eventSkill=sein-a|damage=EventAppliedDamage:0.5|prefab=Sein_A_Master-2`.

### History

- 2026-05-26: User asked Skill Builder to implement Sein-A enhancement and master effects using `Pakuri/reference/2.Monster/sein/skill/a-scorching-arrow.md`, with master-2 using `Assets/Prefab/Skill/Sein/Sein_A_Master-2.prefab`.
- 2026-05-26: Initial blueprint pass stopped on projectile impact explosion, then user pointed to the existing hit-trigger SingleAttack common logic; Builder reused that shared path and added a small source-owned `OnOutgoingDamage` dispatch extension.

## Task: 2026-05-20 Sein-B Shared Burst Projectile Implementation

### Task title

Implement Sein-B through the shared projectile burst extension.

### Goals

- Add a shared sequential burst count path instead of a Sein-only projectile branch.
- Make `sein-b` fire 5 projectiles per cycle at `shot_interval_seconds`, repeat that cycle `magazine_capacity` times, then wait on cooldown/reload.
- Wire `sein-b` to the requested `Assets/Prefab/Skill/Sein/Sein_A.prefab` visual through `EffectManager`.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Unity Play Mode gameplay verification remains user-owned.
- Keep the implementation reusable for future projectile skills such as Vega.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and non-gameplay verified.

### Next Actions

- User verifies in Play Mode that Sein-B emits 5 sequential projectiles per cycle and repeats for 4 magazine cycles before the 6 second recovery.
- If Sein-B crit-chance master behavior is required, implement that as a separate choice-modifier extension because the current shared choice path still lacks crit chance modifiers.

### Evidence

- `Pakuri/reference/2.Monster/sein/skill/b-blazing-volley.md` defines `?꾪솚 ??5`, `?꾩갹 ??4`, `?ъ옣???쒓컙 6.0珥?, `諛쒖궗 媛꾧꺽 0.18珥?, base fire damage `14`, attack coefficient `0.65`, and projectile speed `20.0`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `projectile_burst_count`; the `sein-b` row maps to `projectile_burst_count=5`, `magazine_capacity=4`, `shot_interval_seconds=0.18`, `cooldown_seconds=6`, `reload_seconds=6`, `projectile_speed=20`, and `pierce_count=0`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs` now tracks queued burst shots and starts recovery only after the queued burst completes and the magazine is exhausted.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` keeps `AdditionalProjectileBonus` as simultaneous fan-spread only when `BurstProjectileCount <= 1`; burst skills use that bonus in runtime burst count instead.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` now maps `sein-b` to prefab GUID `256552cb82ec9c2499fc2e0e01d20dd2`, the existing `Assets/Prefab/Skill/Sein/Sein_A.prefab`.
- `PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` followed by runtime mapping inspection returned `sein-b:burst=5;mag=4;interval=0.18;cooldown=6;reload=6;speed=20`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained. A first parallel runtime build hit only an `Assembly-CSharp.dll` file lock and passed when rerun alone.
- Unity-MCP console after refresh still contained MCP client-exit and `UnityEditor.Graphs` exceptions, but no `Pakuri` skill/CSV error was reported in the retrieved entries.

### History

- 2026-05-20: User approved an exact shared implementation for the Sein-B 5-shot burst cycle instead of the approximate existing magazine projectile behavior.
