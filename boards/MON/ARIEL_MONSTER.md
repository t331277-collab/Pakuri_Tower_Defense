## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/ARIEL_MONSTER.md`.

# ARIEL_MONSTER

## Scope

Ariel dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Ariel file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Task: 2026-05-22 Ariel Final Shared Choice Runtime Completion

### Task title

Implement `ariel-a-trait-5` and `ariel-d-trait-5` through shared choice/status contracts and re-audit Ariel coverage.

### Goals

- Add a shared choice snapshot rule that counts shielded allies and converts the count into a per-cast damage multiplier.
- Add a shared status rule that increases incoming damage only when the attacker has a required status and the target carries the marked status.
- Confirm that no Ariel skill, choice, effect, or trigger row remains unsupported after this pass.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay reusable in shared runtime/data paths rather than adding Ariel-only execution branches.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and CSV-sync-verified.

### Next Actions

- User verifies in Play Mode that `ariel-a-trait-5` scales Ariel-A damage by `+6%` per currently shielded ally at cast time.
- User verifies in Play Mode that `ariel-d-trait-5` increases damage only when the attacker has `shield` and the target carries Ariel-D's `holy-exposure` mark.
- Run Code Reviewer only if explicitly requested.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:7` now marks `ariel-a-trait-5` as `RuntimeImplemented` with `count_status_id=shield`, `count_target_side=AllAllies`, and `damage_multiplier_per_count=0.06`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:28` now marks `ariel-d-trait-5` as `RuntimeImplemented` with `status_conditional_source_status_id=shield` and `status_conditional_damage_taken_bonus=0.1`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutionSystem.cs:216-285` now resolves choices with roster context, counts matching status holders, and applies the dynamic damage multiplier to the cast snapshot.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:291-337`, `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:234-246`, `:366-374`, and `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:965-1011` now carry source-conditional incoming-damage status data through status resolution and the live damage path.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv:1-2` now contains the current status payload schema columns, including `status_ailment_resistance_bonus` and `status_flat_element_resist_reduction`, so editor CSV sync matches the parser contract.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv | Where-Object { $_.monster_id -eq 'ariel' -and $_.implementation_state -notin @('RuntimeImplemented','ReferenceDirect') }`, the matching `monster_skill_choices.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv` checks all returned no rows.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.
- Unity-MCP console after clear plus `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-22: User asked Code Builder to implement `ariel-a-trait-5` and `ariel-d-trait-5` and confirm whether every Ariel skill was now implemented.

## Task: 2026-05-22 Ariel CSV-Only And Small Shared-Contract Follow-Up

### Task title

Implement the Ariel rows previously classified as CSV-only or requiring only small shared runtime/data contracts.

### Goals

- Finish `ariel-h-trait-3`, `ariel-i-trait-2`, and `ariel-j-trait-1` without adding skill-specific execution code.
- Add the smallest shared contracts needed to finish `ariel-b-master-1`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-i-trait-1`, and `ariel-i-trait-3`.
- Re-scan Ariel choice coverage and record the exact rows still unsupported after this pass.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay reusable in shared runtime/data paths rather than adding Ariel-only branches.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly told Builder not to run Reviewer.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that `ariel-b-master-1` grants shield amount `+50%` and status ailment resistance `+30%` only while the shield status remains active.
- User verifies passive-choice-gated effects for `ariel-f-trait-3`, `ariel-g-trait-1/2/3`, `ariel-i-trait-1/3`, and `ariel-j-trait-1`.
- Remaining Ariel choice rows still unsupported after this pass are only `ariel-a-trait-5` and `ariel-d-trait-5`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:15` now marks `ariel-b-master-1` as `RuntimeImplemented` with `status_ailment_resistance_bonus=0.3`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:40-43` now mark `ariel-f-trait-3` and `ariel-g-trait-1/2/3` as `RuntimeImplemented`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:46-50` now mark `ariel-h-trait-3`, `ariel-i-trait-1/2/3`, and `ariel-j-trait-1` as `RuntimeImplemented`; `ariel-i-trait-2` now targets `ariel-d`, and `ariel-j-trait-1` now targets `ariel-e`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:15`, `:25`, `:27`, `:29-30`, `:33-34`, and `:37` add the gated Ariel-C, Ariel-F, Ariel-G, Ariel-I, and Ariel-J effect rows used by this pass.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs:56-106` now builds a passive-choice snapshot so passive effect rows can gate on chosen passive choices.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:298-326`, `:1439-1550` now apply shield choice ailment-resistance overrides, allow effect rows to filter by active-skill attribute, and map crit-chance / ailment-resistance / flat-element-resistance status payloads into runtime status data.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs:222-262` now resolves crit chance bonus, flat element resistance reduction, and ailment resistance from active statuses; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillStatusApplyUtility.cs:18-48` now applies ailment resistance to harmful status application chance.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only `ariel-a-trait-5` and `ariel-d-trait-5`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement the rows previously classified as CSV-only or requiring only a small shared contract.

## Task: 2026-05-22 Ariel Shared Trigger/Crit/Duration Runtime Completion And Coverage Audit

### Task title

Implement Ariel's remaining shared-runtime-driven active/master effects and audit full Ariel coverage.

### Goals

- Implement `ariel-b-master-2` through a reusable shield-absorb trigger contract.
- Implement `ariel-d-trait-4` through choice-driven target-count bonus support.
- Implement `ariel-d-master-1` by wiring live InGame critical damage into the shared damage path and letting the Ariel-D mark carry a critical-damage-taken bonus.
- Implement `ariel-d-master-2` through reusable status-expire trigger plus tracked incoming damage.
- Implement `ariel-e-trait-5` through reusable runtime extension of active shield status durations.
- Re-scan Ariel choice/effect/trigger coverage and record the remaining unsupported rows.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay generic in shared runtime/data paths; no Ariel-only execution branches were added for these effects.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because the user explicitly said not to run it.

### Role Owner

Code Builder

### Status

Implemented, compile-verified, and coverage-audited.

### Next Actions

- User verifies in Play Mode that `ariel-b-master-2` reflects absorbed shield damage to the attacker, `ariel-d-master-1` increases critical damage taken on the marked target, `ariel-d-master-2` bursts on mark expiry from tracked Holy damage, and `ariel-e-trait-5` extends existing ally shield durations.
- Remaining Ariel rows still needing future work are `ariel-a-trait-5`, `ariel-b-master-1`, `ariel-d-trait-5`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-h-trait-3`, `ariel-i-trait-1`, `ariel-i-trait-2`, `ariel-i-trait-3`, and `ariel-j-trait-1`.
- `ariel-b-master-1` remains only partial because the shield amount portion is implemented, but the ailment-resistance portion still has no shared runtime contract.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:16` marks `ariel-b-master-2` `RuntimeImplemented`; `:27` marks `ariel-d-trait-4` with `hit_target_count_bonus=1`; `:29-30` mark `ariel-d-master-1` and `ariel-d-master-2` implemented; `:35` marks `ariel-e-trait-5` implemented.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:5-6` add the reusable `OnShieldAbsorb` and `OnStatusExpire` Ariel trigger rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` adds `ariel-e-trait5-extend-shield-duration` as a shared `ExtendStatusDuration` effect row.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:55`, `:103-113`, and `:248-260` add `ExtendStatusDuration`, the new trigger events/damage source, and the new choice/runtime fields used by Ariel.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:11-13`, `:135-140`, `:271-277`, `:571`, `:577-618`, and `:834-962` now route crit-aware damage, shield-absorb triggers, status-expire triggers, tracked incoming damage recording, and shared status-duration extension.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs:82-133`, `:390-396`, and `:515-518` execute shield-absorb and status-expire triggers, resolve `ShieldAbsorbedAmount` / `TrackedIncomingDamage`, and prioritize the event target when required.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -like 'ariel-*' -and $_.runtime_support_state -notin @('RuntimeImplemented','ReferenceDirect') }` returned only `ariel-a-trait-5`, `ariel-b-master-1`, `ariel-d-trait-5`, `ariel-f-trait-3`, `ariel-g-trait-1`, `ariel-g-trait-2`, `ariel-g-trait-3`, `ariel-h-trait-3`, `ariel-i-trait-1`, `ariel-i-trait-2`, `ariel-i-trait-3`, and `ariel-j-trait-1`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` assembly-version warnings remained.

### History

- 2026-05-22: User asked Code Builder to start implementing the previously proposed shared-runtime Ariel fixes, skip Code Reviewer, and then verify whether all Ariel skills, traits, and master effects were now implemented.

## Task: 2026-05-22 Ariel Triggered SingleAttack Runtime

### Task title

Implement Ariel last-shot and shield-expiry trigger skills through CSV-driven SingleAttack reuse.

### Goals

- Add `monster_skill_triger.csv` as the CSV authority for trigger-driven hidden skill executions.
- Implement `ariel-a-master-1` as two last-magazine-projectile hit explosions at 0.5 second intervals using `Assets/Prefab/Skill/Ariel/Ariel_C.prefab`.
- Implement `ariel-b-trait-4` as shield-expiry/depletion damage using `Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab`.
- Reuse SingleAttack-style target resolution and prefab hitbox collision instead of adding Ariel-only skill branches.

### Constraints

- Role Owner is Code Builder.
- Trigger rows remain CSV-owned; runtime code stays generic for trigger event dispatch.
- The requested file name is `monster_skill_triger.csv`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies in Play Mode that `ariel-a-master-1` fires two `Ariel_C` prefab-hitbox explosions from the final Ariel-A magazine projectile hit, spaced by 0.5 seconds.
- User verifies in Play Mode that `ariel-b-trait-4` triggers when Ariel-B shield statuses expire by timer or depletion and that the prefab collider matches the intended visual area.
- Run Code Reviewer only if the user explicitly requests it.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:3` defines `ariel-a-master1-last-shot-explosion` with event `OnMagazineLastProjectileHit`, `repeat_count=2`, `repeat_interval_seconds=0.5`, and prefab `Assets/Prefab/Skill/Ariel/Ariel_C.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv:4` defines `ariel-b-trait4-shield-expire` with event `OnShieldExpire`, shield-applied-amount damage source, multiplier `0.6`, and prefab `Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:8` and `:13` mark `ariel-a-master-1` and `ariel-b-trait-4` as `ReferenceDirect` with trigger CSV notes.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs:97-131` defines trigger event, trigger damage source, and `SkillTriggerDefinition`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs:12`, `:36`, `:202`, and `:334` implement projectile-hit trigger dispatch, shield-expire trigger dispatch, SingleAttack trigger execution, and prefab-hitbox overlap damage.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs:184-196` runs the last-magazine-projectile hit trigger once per projectile.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:100-104`, `:489-499`, and `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:164-179`, `:275-291`, `:401` collect expired/depleted shield statuses and preserve shield source metadata for trigger dispatch.
- `Pakuri/Assets/Prefab/Skill/Ariel/ariel-b-trait-4_Skill.prefab:119` now has a `BoxCollider2D`; `:162` records size `{x: 5.85, y: 5.46}`.
- Runtime and editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings; Unity-MCP CSV catalog sync logged successful sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-22: User asked Code Builder to create `monster_skill_triger.csv` and implement `ariel-a-master-1` plus `ariel-b-trait-4` as trigger-called SingleAttack-style prefab-hitbox skills.

## Task: 2026-05-22 Ariel CSV-First Choice Cleanup And Shield Runtime Modifiers

### Task title

Implement Ariel CSV-only fixes first, then shared shield/status runtime modifiers.

### Goals

- Correct Ariel-C stale `runtime_support_state` choice rows that are already implemented through multi-effect rows.
- Move Ariel-E conditional shield/damage/sanctuary behavior into `monster_skill_effects.csv`.
- Let Ariel-B shield amount/duration choices apply through the shared shield executor.
- Let Ariel-D status duration and Holy Exposure value choices apply through shared status snapshot handling.

### Constraints

- Role Owner is Code Builder.
- Keep damage/status/shield effect data in CSV where the current multi-effect schema can express it.
- Do not implement event-trigger behaviors such as last projectile, shield expiry, shield absorb reflection, or mark expiry in this pass.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, synced, and compile-verified.

### Next Actions

- User verifies Ariel-B shield amount/duration choices, Ariel-B trait 5 Holy damage buff, Ariel-D trait 2/3 mark effects, and Ariel-E trait/master effects in Play Mode.
- Implement remaining event-trigger Ariel items separately: `ariel-a-master-1`, `ariel-b-trait-4`, `ariel-b-master-2`, and `ariel-d-master-2`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:18`, `:19`, `:21`, `:22`, and `:23` now mark Ariel-C trait/master rows as `ReferenceDirect` because existing `monster_skill_effects.csv` rows implement them.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:10`, `:11`, `:14`, and `:15` now map Ariel-B shield amount/duration/Holy-damage-buff support, with master 1 marked partial because status ailment resistance remains unsupported.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:25` now gives `ariel-d-trait-2` `status_element_damage_taken_bonus=0.08`; `:26` keeps `duration_bonus=3` and marks it supported.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv:32`, `:34`, `:36`, and `:37` now mark Ariel-E trait/master shield, conditional damage, sanctuary, and master 2 shield support as `ReferenceDirect`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:3-8` now has Ariel-E default/replacement shield rows, Holy Exposure-only bonus damage, and the master 1 sanctuary damage-reduction status row.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:9` now has the Ariel-B trait 5 shielded-ally Holy damage status row.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:229-247` applies choice duration modifiers to resolved status durations.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1701-1718` applies shield snapshot damage/duration modifiers, and `:1757-1764` runs shield skill multi-effects after routed shield application.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP CSV sync logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`; console after clear/sync showed only that sync log and MCP client handler logs.

### History

- 2026-05-22: User asked Code Builder to implement the earlier classification in order: CSV-only items first, then CSV plus small shared runtime extensions.

## Task: 2026-05-22 Ariel-C/E Debug Learned Skill Auto-Spam Fix

### Task title

Stop DebugUI-learned Ariel-C/E SingleAttack support effects from repeatedly firing outside valid combat input/auto conditions.

### Goals

- Keep Ariel-C and Ariel-E learned active skills usable after DebugUI acquisition.
- Prevent Ariel-C buff and Ariel-E shield effects from repeating during spawn/reward or failed auto execution.
- Preserve Ariel-C/E CSV-owned multi-effect behavior and avoid Ariel-specific executor branches.

### Constraints

- Role Owner is Code Builder.
- The fix is generic in NewRunScene combat input/routing and shared SingleAttack/multi-effect execution.
- No Ariel CSV rows or prefab mappings were changed in this task.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that DebugUI-learned Ariel-C/E persist, but only fire from selected 1P manual click while Auto is off or from Auto mode when a visible enemy exists in MainCamera during combat.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv:5` keeps `ariel-c` as `SingleAttack`; `:7` keeps `ariel-e` as `SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:3` keeps `ariel-e-shield-base` as an all-ally shield using `Assets/Prefab/Skill/Ariel/Ariel_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:4-12` keep Ariel-C blessing/master rows in reusable multi-effect CSV data.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:55-72` now gates skill execution to `StageState.Combat`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:460-469` now requires selected 1P Auto plus visible MainCamera enemies for automatic player skill routing.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:689-699` now lets successfully applied support multi-effects count as routed, starting cooldown/recovery instead of retrying every frame.
- Runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User reported the cause was not DebugUI itself, but learned Ariel-C/E active skills being auto-routed later and failed SingleAttack executions repeatedly spawning effects. Code Builder fixed the generic route/input and SingleAttack multi-effect behavior.

## Task: 2026-05-22 Ariel F-J Passive CSV Runtime

### Task title

Implement Ariel F-J passive skills through the reusable CSV effect runtime.

### Goals

- Make Offering-acquired Ariel F-J passives produce runtime effects from CSV-owned data.
- Keep the implementation generic by attaching passive `monster_skill_effects.csv` rows to `PassiveDefinition`.
- Add the missing shield-received multiplier needed by Ariel-G.
- Gate Ariel-E's post-cast action-speed effect on learned passive `ariel-j` without Ariel-specific executor branches.

### Constraints

- Role Owner is Code Builder.
- User required stopping if the behavior could not be implemented through CSV runtime-read effect structure; inspected code showed it could be done by extending `monster_skill_effects.csv` and shared runtime consumers.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented, compiled, and synced through Unity CSV runtime catalog.

### Next Actions

- User verifies in Play Mode that Offering acquisition of Ariel F-J affects combat: F Holy damage, G shield received/start shield, H blessed ally bonuses, I holy-exposure damage taken, and J shielded Holy damage plus Ariel-E action speed.
- If G's one-shot shield must apply to allies spawned after combat start, extend the one-shot keying/target tracking; current runtime applies when learned passives are refreshed for existing roster entries.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` already has `ariel-f` through `ariel-j` as `Passive` rows with design values in their summaries.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` previously had an empty `PassiveSkillExecutor`, while `RunSession`/UI paths already copied learned passive IDs into `MonsterUnitRuntimeModel.State.LearnedPassiveSkillIds`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now stores `PassiveDefinition.PassiveEffects` and passive-gating fields on `SkillEffectDefinition`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains Ariel F-J rows: `ariel-f-party-holy-damage`, `ariel-g-shield-received`, `ariel-g-start-shield`, `ariel-h-blessed-holy-damage-speed`, `ariel-i-holy-exposure-damage-taken`, `ariel-j-shielded-holy-damage`, and `ariel-e-passive-j-action-speed`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGamePassiveEffectRuntime.cs` applies learned passive effect rows through the shared `SkillMultiEffectExecutor`; `InGameCombatManager.Update()` refreshes them every `0.25s`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now multiplies shield status amounts by `StatusEffectRuntime.ResolveShieldReceivedMultiplier(...)`, so Ariel-G's `status_shield_received_bonus=0.18` affects shield application.
- Unity console logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: User asked Code Builder to implement Ariel F-J, but to stop and ask if the effects could not be implemented through CSV runtime-read structure.
- 2026-05-22: Code Builder extended the existing multi-effect CSV/runtime path for passive effects instead of adding Ariel-only runtime branches.

## Task: 2026-05-22 Ariel-C Multi-Effect CSV Runtime

### Task title

Implement Ariel-C blessing, traits, and master effects through reusable multi-effect CSV rows.

### Goals

- Keep Ariel-C base `SingleAttack` enemy damage on the shared one-shot area path.
- Add all-ally action-speed blessing, trait 2, trait 3, trait 5, master 1, and master 2 behavior through `monster_skill_effects.csv`.
- Avoid Ariel-C-specific executor branches.

### Constraints

- Role Owner is Skill Builder.
- Ariel-C reference values are grounded in `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md`.
- The reference names a second wave but does not specify a time interval, so the CSV row uses `Delayed` with `delay_seconds=0` and runtime schedules it on the next frame instead of inventing a numeric delay.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder

### Status

Implemented and non-gameplay verified.

### Next Actions

- User verifies in Play Mode that Ariel-C hits once normally, applies ally blessing, applies trait/master choices, and master 2 creates the second 60% wave.
- If the second wave needs a designer-authored visible delay later, edit `delay_seconds` in `monster_skill_effects.csv`.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md` lists Ariel-C base Holy damage `28`, spell coefficient `1.2`, radius `3.0`, all-ally action speed `+12%`, buff duration `4.0초`, cooldown `8.0초`, trait 2 `+6%`, trait 3 `+2초`, trait 5 shielded-allies Holy damage `+10%`, master 1 spell power `+18%`, and master 2 second wave `60%`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps `ariel-c runtime_kind=SingleAttack`, `base_damage=28`, `spell_power_coefficient=1.2`, `radius=3`, and `cooldown_seconds=8`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains 9 `ariel-c` effect rows covering the action-speed blessing, trait combinations, shielded Holy damage, master 1 spell-power replacement, and master 2 second wave.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now separates Ariel-C effect application targets from visual placement with `center_mode` and `visual_anchor_mode`; Ariel-C ally buff rows keep `target_side=AllAllies`, use `visual_anchor_mode=AppliedTargets`, and use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` only on representative visual rows.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv:11` keeps the Ariel-C master 2 damage wave on `center_mode=PrimarySkillCenter` and `skill_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_C.prefab`, so the second wave stays on the first SingleAttack center instead of reselecting an ally or a different target.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity:12636` now maps `ariel-c` to `Assets/Prefab/Skill/Ariel/Ariel_C.prefab` in `EffectManager` for the base attack-target SingleAttack visual.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` calls `SkillMultiEffectExecutor.Execute(...)` from `SingleAttackSkillExecutor` and uses choice IDs from `SkillExecutionSnapshot` for `requires_active_choice_id` / `excludes_active_choice_id`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now uses `SkillMultiEffectCenterMode` for visual/damage centers and attaches applied-target status visuals with `InGameAttachedSkillEffectActor` when `visual_anchor_mode=AppliedTargets`.
- A PowerShell CSV reference check returned `OK effects=9 ariel_c=9` for the Ariel-C multi-effect rows, including choice references and prefab path checks. Unity-MCP `execute_menu_item` currently fails to find `Pakuri/Validate CSV Source Data`, so the final verification does not rely on that menu path.
- 2026-05-22 follow-up verification: `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_effects.csv` showed Ariel-C rows with `PrimarySkillCenter`, ally buff rows with `AppliedTargets`, and representative buff prefab rows using `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab`; runtime/editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-22: User asked for Ariel-C to be implemented without hardcoding and with reusable CSV structure for similar future skills.
- 2026-05-22: User asked Code Builder to split Ariel-C effect application target from visual center/anchor, use ally-target buff visuals, keep Ariel-B shield/buff visuals attached to units, and use `Assets/Prefab/Skill/Ariel/Ariel_C-Buff.prefab` for Ariel-C buff visuals.

## Task: 2026-05-20 Ariel-A Master 2 Holy Exposure Runtime Wiring

### Task title

Route `ariel-a-master-2` through the shared on-hit status runtime and allow a choice-specific Holy damage taken bonus.

### Goals

- Make `ariel-a-master-2` apply `holy-exposure` through the existing projectile hit path.
- Let `ariel-a-master-2` supply its own Holy damage taken bonus instead of being forced to share one global `holy-exposure` status-row value.
- Keep the effect data-authored in `monster_skill_choices.csv` rather than adding Ariel-only executor logic.
- Reuse the current shared `StatusEffectKind.HolyExposure` parse/display path already present in the working tree.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- `ariel-a-trait-5` and `ariel-a-master-1` remain unsupported and were not changed in this task.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented at the CSV/runtime-data and shared projectile-status runtime level, and non-gameplay verified.

### Next Actions

- User verifies in `NewRunScene` Play Mode that `ariel-a-master-2` now applies 1 stack of Holy Exposure on hit and increases incoming Holy damage by 15%.
- If later Ariel-A still appears to miss the debuff in gameplay, inspect whether the active choice is actually recorded in the current `RunSession`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `ariel-a-master-2` `status_tag=holy-exposure`, `status_stacks_set=1`, `status_element_damage_taken_bonus=0.15`, and `runtime_support_state=ReferenceDirect`.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skill_choices.csv | Where-Object { $_.choice_id -eq 'ariel-a-master-2' }` returned `status_tag : holy-exposure`, `status_stacks_set : 1`, `status_element_damage_taken_bonus : 0.15`, and `runtime_support_state : ReferenceDirect`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still has `ariel-a` `status_effect_id` blank and `status_chance=0`, so the master choice must provide the status tag itself.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `SkillChoiceEffectSpec.cs`, `InGameSkillDefinitionMapper.cs`, and `SkillExecutionSnapshot.cs` now carry the new choice field `status_element_damage_taken_bonus` through the shared projectile choice path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:191-241` now resolves a choice-provided `snapshot.StatusTag`, defaults new choice-only statuses to `chance=1f` and `stacks=1`, and clones the resolved `StatusEffectData` when a choice-specific `StatusElementDamageTakenBonus` override is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:115-117` and `:174-175` already contain the current working-tree `holy-exposure` / `신성 노출` parse and display strings used by the shared status runtime.
- Unity-MCP menu execution of `Pakuri/Sync CSV Runtime Catalog Assets` returned `success:true` for this task; no new sync log line was captured afterward in the inspected console window.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passed with 0 errors; a first parallel editor build failed only from `Assembly-CSharp.dll` file lock contention, and a standalone `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` then passed with 0 errors. Existing MSB3277 warnings remained.

### History

- 2026-05-20: User asked Code Builder to apply the previously explained `holy-exposure` fix path for `ariel-a-master-2`.
- 2026-05-20: User then required per-skill Holy damage taken values, so Code Builder extended the shared projectile choice status path with a choice-level `status_element_damage_taken_bonus` override and set Ariel-A master 2 to `0.15`.

## Task: 2026-05-17 Ariel-A Common Projectile Runtime Connection

### Task title

Connect Ariel-A Judgement Light through the shared InGame projectile path.

### Goals

- Route `ariel-a` to the shared `ProjectileSkillExecutor` / `InGameProjectileActor` path.
- Use the user-authored `Assets/Prefab/Skill/Ariel/Airel_A.prefab` as the Ariel-A projectile visual.
- Record which Ariel-A reference behavior is covered by the common projectile path and which behavior remains unsupported.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- Current runtime source schema does not expose per-skill base pierce count or per-skill projectile speed, so Ariel-A base pierce `1` and projectile speed `17` are mapped explicitly in `InGameSkillDefinitionMapper` from `Pakuri/reference/2.Monster/ariel/skill/a-judgement-light.md`.
- The common projectile path covers the base straight projectile, damage, magazine, reload, shot interval, prefab instantiation, and pierce. It does not implement Ariel-A critical rolls, shielded-ally damage scaling, White Judgement last-shot explosions, or Guiding Light holy exposure.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Builder implementation completed and local non-gameplay checks passed. 2026-05-18 Ariel-A projectile speed and pierce are now owned by `monster_skills.csv` instead of skill-ID-specific mapper code. 2026-05-18 supported runtime status labels can now be edited directly in CSV when `status_effect_id` is blank.

### Next Actions

- User verifies in NewRunScene Play Mode that Ariel-A fires `Airel_A.prefab`, damages enemies, and pierces one extra target.
- Add data/source schema fields for per-skill projectile speed and base pierce if more skills need those values without skill-ID-specific mapper exceptions.
- Implement separate runtime support before claiming Ariel-A master effects or shielded-ally scaling are active.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/EffectManager.cs` plus `NewRunScene.unity` now own the Ariel-A prefab mapping; `monster_skills.csv` no longer stores a base `skill_effect_prefab_path` column.
- `Pakuri/Assets/CSVData/SkillData.csv` now includes the Ariel-A reference row with base damage `18`, spell coefficient `1`, magazine `7`, reload `4.6`, shot interval `0.36`, pierce `1`, and projectile speed `17`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now serializes `arielAProjectilePrefab` and resolves `"ariel-a"` to it.
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` assigns `arielAProjectilePrefab` to `Assets/Prefab/Skill/Ariel/Airel_A.prefab` GUID `66fcb365022930d4681ad320e5fff520`.
- `Pakuri/Assets/Prefab/Skill/Ariel/Airel_A.prefab` now has trigger `BoxCollider2D` and `Pakuri.InGame.InGameProjectileActor`.
- `Pakuri/Assets/Resources/Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog.asset` now includes `Assets/Prefab/Skill/Ariel/Airel_A.prefab`.
- CSV check returned `UpperA=ariel-a`, `Pierce=1`, `Speed=17`, `SourcePrefab=Assets/Prefab/Skill/Ariel/Airel_A.prefab`, `SourceMagazine=7`, `SourceReload=4.6`, and `SourceShot=0.36`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and existing warnings; an earlier parallel runtime build failed only from an `obj\Debug\Assembly-CSharp.dll` file lock, then passed when rerun alone.
- Unity-MCP refresh reached idle; console warning/error read showed only MCP client handler logs, not C# compile errors.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores `ariel-a` `projectile_speed=17`, `pierce_count=1`, `status_chance=0`, and `status_effect_label=없음`; the CSV `range` column was removed.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` no longer has `ResolveProjectileSpeed(...)` or `ResolveBasePierceCount(...)` Ariel-A special cases.
- `ariel-b` `base_damage` in `monster_skills.csv` is now `35`, matching `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps Ariel design-only labels such as `방어막`, `축복`, and `신성 노출` with `status_chance=0`; if `ariel-a` is edited to `status_effect_label=감전`, `status_chance=1`, and `pierce_count=999`, the mapper can resolve the label to the supported `shock` status.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` parses Korean runtime labels including `감전`, and `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` falls back from blank `status_effect_id` to parseable `status_effect_label`.
- `SyncCsvRuntimeCatalogs.bat` was added for Unity batchmode sync; when the project was already open, Unity batchmode rejected duplicate project open, then Unity-MCP invoked `Pakuri.Data.PakuriCsvRuntimeData.SyncAndValidateCsvRuntimeCatalogsForEditor()` and the console logged successful sync/validation.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-17: User asked Code Builder to implement Ariel-A using `Assets/Prefab/Skill/Ariel/Airel_A.prefab` and to report any information the blueprint alone could not provide.
- 2026-05-18: Code Builder moved Ariel-A projectile speed/pierce from mapper hardcoding into the skill CSV row and filled Ariel-B shield base from the reference document.
- 2026-05-18: Code Builder added status-label fallback and CSV runtime sync batch support so supported status edits in `monster_skills.csv` can be synced without code changes.

## Task: 2026-05-18 Ariel One-Shot Area Runtime Kind

### Task title

Route Ariel C/E through the new SingleAttack runtime kind.

### Goals

- Make Ariel C and Ariel E one-shot area attacks instead of sustained `AreaAttack` rows.
- Keep the existing CSV numeric values unchanged.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Ariel C/E apply one immediate area hit through the shared executor.

### Evidence

- `Pakuri/reference/2.Monster/ariel/skill/c-blessing-wave.md` names Ariel C `축복의 파동`.
- `Pakuri/reference/2.Monster/ariel/skill/e-archangel-descent.md` names Ariel E `대천사의 강림`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `ariel-c runtime_kind=SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `ariel-e runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV rows 5 and 7 as one-shot area attack skills for the new `SingleAttack` type.

## Task: 2026-05-15 Ariel-B Phase4-C-0 Shield Effect Minimum Execution

### Task title

Connect Ariel-B to the first shared InGame attached effect actor path.

### Goals

- Add a reusable attached skill-effect actor that follows a target transform for a configured duration.
- Connect Ariel-B shield execution through the shared `ShieldSkillExecutor`.
- Use the user-authored `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` as the current Ariel-B visual prefab.
- Keep shield resource mutation in `InGameCombatManager.GrantShield(...)`.

### Constraints

- Role Owner is Code Builder.
- No Play Mode gameplay verification was run by Codex.
- This slice grants shield values and expires the visual actor only; timed shield resource expiry is not implemented here.
- `Assets/Prefab/Skill/Ariel/Airel_A.prefab` exists with the typo `Airel_A`, but `SkillData.csv` currently has no `ariel-a` row in the inspected minimum data set, so Ariel-A was not connected in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and compile/editor-refresh verified.

### Next Actions

- User verifies in Play Mode that Ariel-B shield visual appears on player units when Ariel-B is learned and cast.
- Add a timed shield resource-expiry system before declaring support-shield duration behavior complete.
- Add Ariel-A only after a matching skill data row and execution target are confirmed.

### Evidence

- Added `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameAttachedSkillEffectActor.cs`.
- `SkillExecutors.cs` now makes `ShieldSkillExecutor` call `GrantShield(...)` and instantiate a shield visual using `InGameAttachedSkillEffectActor`.
- `NewRunScene.unity` assigns `arielBShieldEffectPrefab` to `Assets/Prefab/Skill/Ariel/Ariel_B.prefab`.
- `Assets/Prefab/Skill/Ariel/Ariel_B.prefab` has `Pakuri.InGame.InGameAttachedSkillEffectActor`.
- `Pakuri/Assets/Legacy/Data/GameData/Monsters/ariel.asset` stores `ariel-b` `BaseDamage: 35`, matching the inspected `SkillData.csv` shield base value.
- Runtime and editor builds passed with 0 errors and existing assembly reference warnings.
- Unity-MCP refresh reached idle and console warning/error read showed no C# compile errors.

### History

- 2026-05-15: User asked Code Builder to create the common projectile/effect actor component and connect Ariel-B minimum execution as the first Phase4-C subtask.

## Task: 2026-05-14 Ariel NewRunScene Prefab Binding And HP Bar

### Task title

Confirm Ariel prefab actor/model binding and HP bar sprite visibility.

### Goals

- Bind `Ariel_Unit` through `NewRunSceneEntryManager`.
- Verify Ariel creates an exact `ariel` runtime model and initializes `MonsterUnitActor`.
- Make Ariel's `MonsterHpBar` render through the shared HP bar pixel sprite.

### Constraints

- Role Owner is Code Builder.
- No Ariel combat execution or Play Mode verification in this slice.

### Role Owner

Code Builder

### Status

Builder implementation completed and locally verified.

### Next Actions

- User verifies Ariel selection and HP bar visibility in Play Mode.

### Evidence

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity` references `Ariel_Unit.prefab` in `arielUnitPrefab`.
- Unity-MCP verification returned `ariel:prefab=Ariel_Unit|modelOk=True|model=ariel|actor=True|actorModel=True|hpText=HP 240/240|bgSprite=True|fillSprite=True|shieldSprite=True`.
- 2026-05-14 follow-up: `MonsterUnitActor` now scales HP fill against `Background.localScale.x`; Unity-MCP editor code returned `Ariel_Unit:bgX=20|beforeFillX=20|fullFillX=20|halfFillX=10`.

### History

- 2026-05-14: User asked to verify all five selectable prefab bindings and fix invisible `MonsterHpBar`.
- 2026-05-14: User reported `HpFill` was forced to `1` on scene entry; Builder changed fill scaling to use the background width.

## Task: 2026-05-14 Ariel CSVData Phase0-2 Seed Rows

### Task title

Record Ariel rows added to the new CSVData files.

### Goals

- Seed Ariel identity/stat data in `MonsterStat.csv` so the shield sample skill has an owner row.
- Seed Ariel-B Radiant Shield in `SkillData.csv`.
- Preserve the no-damage shield attribute distinction in CSV fields.

### Constraints

- Role Owner is Code Builder.
- No Ariel runtime behavior, prefab, scene, or Play Mode changes.
- `ariel-b` stores `skill_element` as Holy and `damage_element` as None because the inspected reference says the shield has no damage attribute.

### Role Owner

Code Builder

### Status

Builder implementation completed and CSV parsing verified.

### Next Actions

- Later CSVData mapping should handle `damage_element=None` for non-damage support skills.
- Reconfirm Ariel base HP ownership before CSVData becomes the authoritative source because `ariel-tower.md` does not list HP.

### Evidence

- `Pakuri/Assets/CSVData/MonsterStat.csv` now contains the `ariel` row with current project stat values and source notes.
- `Pakuri/Assets/CSVData/SkillData.csv` now contains `ariel-b` as `ShieldSkillData`.
- `Pakuri/reference/2.Monster/ariel/skill/b-radiant-shield.md` provides shield 35, spell coefficient 1.4, duration 5.0, cooldown 9.0, all-allies targeting, and highest-value refresh.
- `Import-Csv Pakuri\Assets\CSVData\SkillData.csv` returned `ariel-b` with `damage_element` None and `shield_base` 35.

### History

- 2026-05-14: Code Builder added Ariel seed data as part of CSVData Phase0~2.

## Task: 2026-05-13 Ariel Battlefield Facade Registration

### Task title

Route Ariel battlefield projectile and effect registration through the Phase 1 facade.

### Goals

- Preserve Ariel skill behavior while replacing direct battlefield list registration writes.
- Keep Ariel projectile/effect creation behind the new battlefield registration boundary.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user owns gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel skills in Play Mode if needed.

### Evidence

- `CombatRuntimeArielSkills.cs:244` now calls `AddBattlefieldProjectile(...)`.
- `CombatRuntimeArielSkills.cs:335`, `:722`, and `:1036` now call `AddBattlefieldSkillEffect(...)`.
- Runtime and Editor builds completed with 0 errors and existing warnings.

### History

- 2026-05-13: Phase 1 battlefield facade boundary routed Ariel battlefield object registration through facade methods.

## Task: 2026-05-10 Ariel Manifested Shield Expiry And Archangel Effect Fix

### Task title

Fix 2P-5P Ariel shield expiry on 1P and make Archangel Descent effect visible through the shared Ariel path.

### Goals

- Make shields granted to the selected 1P monster by Manifested Ariel B/E expire when their duration ends, even when the selected 1P monster is not Ariel.
- Make Ariel E `Archangel Descent` use an explicit battlefield-wide visual path for selected and Manifested Ariel casts.
- Explain the bug from inspected runtime code.

### Constraints

- Role Owner is Code Builder.
- Do not run Unity Play Mode; user performs gameplay verification.
- Code Reviewer was not run because the user did not explicitly permit Reviewer execution.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in RunScene Play Mode that Manifested Ariel shields on 1P disappear after their duration.
- User verifies selected and Manifested Ariel E show the battlefield-wide Archangel Descent effect.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before the fix, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:83` through `:88` decremented `unitShieldTimer` inside `UpdateArielSkillCooldowns()`, which only runs for the selected monster's Ariel runtime.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:518` now calls `UpdateSelectedUnitShieldTimer(Time.deltaTime)` from the common selected-unit combat update.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:86` now defines `UpdateSelectedUnitShieldTimer(...)`, clearing selected shield state and mirrored `selectedUnitRuntime` shield/Ariel fields when the timer expires.
- `CombatRuntimeArielSkills.cs:12` defines `ArielArchangelEffectDuration`; `:438` and `:693` call `CreateArielArchangelDescentEffect(skill)` for selected and unit-owned Ariel E casts.
- `CombatRuntimeArielSkills.cs:700` creates the battlefield-wide `ArchangelDescent` circle with stronger alpha/sorting and adds it to `skillEffects`.
- Follow-up: `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:35` adds `ShieldAppliedFrame`; `:160` through `:163` skip manifested shield timer decay on the frame the shield was applied.
- Follow-up: `CombatRuntimeArielSkills.cs:28` adds `unitShieldAppliedFrame`; `:95` through `:98` skip selected 1P shield timer decay on the frame the shield was applied.
- Follow-up: `CombatRuntimeArielSkills.cs:831` and `:902` stamp selected and manifested shield application with `Time.frameCount`; `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:79` mirrors the selected shield frame into `selectedUnitRuntime`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Follow-up `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors and the same existing warnings.
- `git diff --check -- Pakuri\Assets\Scripts\Combat\Skill\CombatRuntimeArielSkills.cs Pakuri\Assets\Scripts\Combat\Manager\CombatRuntimeProjectiles.cs` completed with only LF-to-CRLF warnings.
- Unity-MCP script refresh recovered to ready; console warning/error read returned only MCP client handler logs, not C# compile errors.
- Follow-up `git diff --check` over `CombatUnitRuntime.cs`, `CombatRuntimeArielSkills.cs`, and `CombatRuntimeParty.cs` completed with only LF-to-CRLF warnings; Unity-MCP console read returned only MCP client handler/timeout logs, not C# compile errors.

### History

- 2026-05-10: User reported Manifested 2P-5P Ariel shields remain on selected 1P after Ariel's shield duration ends, and Ariel E's effect is not visible.
- 2026-05-10: Code Builder moved selected-unit shield timer ticking out of selected-Ariel-only cooldown logic and routed Ariel E selected/unit casts through a dedicated battlefield visual helper.
- 2026-05-10: User reported 1P shield duration now appeared shorter than 2P-5P after Ariel shield casts; Builder aligned selected and manifested shield timers by skipping decay on the frame a shield is applied.

## Task: 2026-05-10 Ariel Unit Executor Migration And Team Shield

### Task title

Move Manifested Ariel A-E onto Ariel unit executor paths and make Ariel shield skills protect party units.

### Goals

- Dispatch Manifested Ariel skills through Ariel-specific `CombatUnitRuntime` logic before the generic manifested fallback.
- Keep Ariel A projectile damage, Holy Exposure, and White Judgement explosion source-aware for manifested Ariel.
- Make Ariel B `Radiant Shield` and Ariel E `Archangel Descent` apply shield state to selected 1P plus living manifested 2P-5P party units.
- Confirm the prior MainMenu-selected Ariel shield behavior against actual code and correct it.

### Constraints

- Role Owner is Code Builder.
- User performs Play Mode verification.
- Code Reviewer was not run because the user did not explicitly permit it.

### Role Owner

Code Builder

### Status

Implemented and locally validated by C# builds, Unity-MCP refresh, console check, and `git diff --check`.

### Next Actions

- User verifies selected Ariel B/E shields on 2P-5P teammates in RunScene Play Mode.
- User verifies Manifested Ariel A-E and Holy Exposure interactions in RunScene Play Mode.
- Run Code Reviewer only if explicitly requested.

### Evidence

- Before this change, `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:516` used selected-only `unitShieldValue` in `ApplyArielUnitShield(...)`, `CombatRuntimeProjectiles.cs:455` applied manifested damage directly to HP, and `CombatRuntimeParty.cs:2034` passed `0f` as manifested shield value.
- `Pakuri/Assets/Scripts/Combat/Monster/CombatUnitRuntime.cs:33` through `:42` now stores per-unit shield and Ariel blessing/sanctuary/Archangel shield state.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeParty.cs:637` dispatches `TryTickArielUnitSkill(...)` before generic manifested fallback.
- `Pakuri/Assets/Scripts/Combat/Skill/CombatRuntimeArielSkills.cs:422` through `:681` implements Ariel unit A-E execution paths.
- `CombatRuntimeArielSkills.cs:808` applies Ariel team shields to selected plus manifested units; `:1300` handles Ariel unit projectile hits.
- `Pakuri/Assets/Scripts/Combat/Manager/CombatRuntimeProjectiles.cs:464` through `:473` applies shield absorption to manifested unit damage before HP loss.
- Runtime and Editor builds completed with 0 errors and existing `System.Net.Http` / `System.IO.Compression` warnings.
- Unity-MCP script refresh reached idle; console warning/error read returned only MCP client handler logs.

### History

- 2026-05-10: User requested the Ariel unit executor migration from the remaining-work report and asked whether MainMenu-selected Ariel shield skills protect teammates.
- 2026-05-10: Code inspection confirmed selected Ariel shields did not protect manifested teammates before this pass; Builder added party shield state and Ariel unit executor dispatch.

## Task: 2026-05-21 Ariel-D SingleAttack Target Fix

### Task title

Fix Ariel-D strongest-enemy targeting after Mark-to-SingleAttack conversion.

### Goals

- Keep Ariel-D authored as a SingleAttack skill.
- Preserve `HighestHealth` as the first implementation of "strongest enemy".
- Prevent Ariel-D's zero radius from turning into all-enemy coverage.

### Constraints

- Role Owner is Code Builder.
- Party focus-target AI remains intentionally unimplemented per user instruction.
- User performs Play Mode verification.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies Ariel-D only damages/applies the mark status to the current highest-HP enemy in Play Mode.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Ariel-D row has `runtime_kind=SingleAttack`, `radius=0`, `target_selection=HighestHealth`, `status_effect_id=holy-exposure`, and `status_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_D.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now sets `single.Area.CoverAll` to false when `source.TargetSelection` is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` passes `coverAll` into `InGameZoneSkillActor.ApplyAreaTick(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` uses the single-target branch only when `!areaCoversAll && areaRadius <= 0f`.
- Runtime and Editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.

### History

- 2026-05-21: User reported Ariel-D appeared to hit all targets. Builder traced the behavior to `SingleAttackData.Area.CoverAll = source.Radius <= 0f` and changed it to respect explicit `target_selection`.
