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
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-05-28 Vega-B Master-1 Follow-up Returned To LineAttack

### Task title

Convert the Vega-B master-1 delayed second slash from the shared triggered `SingleAttack` path to the shared triggered `LineAttack` path so it matches the aimed slash behavior of the Vega-B base skill.

### Goals

- Make the delayed second slash rotate and travel on the same shared line-attack presentation path as base `vega-b`.
- Keep the authored `0.4s` delay, `45%` scaled damage, prefab path, and linked `1s` silence effect.
- Preserve CSV validation and runtime-catalog sync after the trigger-path change.

### Constraints

- Role Owner is Code Builder.
- This change stays on the existing `vega-b-master1-second-slash` trigger row plus the shared trigger runtime; no hidden helper skill row was added.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that Vega-B master-1 second slash now aims like base `vega-b` instead of appearing as the older self-centered `SingleAttack` follow-up.
- If design later requires the delayed slash to lock to the exact original cast target/path instead of re-resolving nearest target at `0.4s`, that would need a separate trigger-context extension.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `runtime_kind=LineAttack`, `trigger_action=LineAttack`, `target_selection=Nearest`, `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`, and linked effect `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` still authors base `vega-b` as `runtime_kind=LineAttack`, so the base and follow-up now share the same runtime kind and prefab path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now includes an explicit `SkillTriggerActionKind.LineAttack` branch and `ExecuteLineAttack(...)` shared trigger path for direct delayed line slashes.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` now resolves linked OnHit status effects through the passed `SkillExecutionSnapshot`, so the triggered line path keeps source-skill choice-gated status rules instead of losing them on the beam actor path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data` logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` logged `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

### History

- 2026-05-28: After base `vega-b` was returned to `LineAttack`, the user reported that the master-1 delayed second slash still looked like the older `SingleAttack` follow-up and requested the same aimed slash path for the follow-up hit.

## Task: 2026-05-28 Vega-B Base Skill Returned To LineAttack

### Task title

Return Vega-B base skill to the shared `LineAttack` path so the slash aims toward the target instead of spawning as a self-centered `SingleAttack`.

### Goals

- Fix the current “cast on self” visual feel reported by the user.
- Keep Vega-B using the shared beam/line actor rotation path like other straight aimed slashes.
- Preserve base damage, silence payload, cooldown, width, and prefab path.

### Constraints

- Role Owner is Code Builder.
- This change is limited to the active Vega-B base skill row and runtime-catalog sync.
- The existing master-1 delayed second slash trigger row remains on the shared triggered `SingleAttack` path for now.

### Role Owner

Code Builder

### Status

Implemented and Unity CSV-validated.

### Next Actions

- User verifies in Play Mode that base Vega-B now rotates toward the current target like a straight aimed line attack.
- If master-1 must also rotate on the same path, that follow-up still needs a separate shared trigger-beam design decision.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b runtime_kind=LineAttack`, keeps `radius=1.8`, `cooldown_seconds=8`, `status_effect_id=silence`, and keeps `Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/BeamSkillExecutor.cs` resolves target direction from nearest target and spawns the prefab with `ResolveRotation(direction)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameLineAttackActor.cs` rotates the live line actor from `lineDirection`, which is why the LineAttack path matches the user-requested aimed slash behavior.
- Unity menu `Pakuri/Validate CSV Source Data` completed and the console logged the runtime catalog load summary without new Vega-B CSV errors.
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets` completed and the console logged sync from `Assets/CSVdata/source` to `Assets/Resources/Pakuri/CSVRuntime`.

### History

- 2026-05-28: After the earlier SingleAttack contact implementation, the user reported that Vega-B still looked like a self-cast slash even though damage landed; targeted inspection confirmed the visual issue was caused by the SingleAttack prefab spawn path using identity rotation.

## Task: 2026-05-28 Vega-B Follow-up Trigger Payload Correction

### Task title

Fix the authored Vega-B master-1 follow-up trigger row so CSV validation passes and the second slash deals the intended scaled damage.

### Goals

- Remove the current Vega-B source CSV validation failure.
- Keep the second slash at the intended `45%` scaling while giving the trigger row a real damage payload.
- Preserve the existing shared triggered `SingleAttack` plus linked OnHit silence path.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- The correction stays inside the existing Vega-B row bundle and shared validator; no hidden helper skill row was added.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and editor-validated.

### Next Actions

- User verifies in Play Mode that the second slash now deals the scaled damage as expected, not just the linked `1s` silence.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `vega-b-master1-second-slash` with `base_damage=30`, `attack_power_coefficient=1.4`, and `damage_multiplier=0.45`.
- Unity menu `Pakuri/Validate CSV Source Data` completed after the correction, and the console logged the runtime catalog load summary instead of the previous Vega-B trigger validation failure.

### History

- 2026-05-28: The first authored row kept only `damage_multiplier=0.45` and zeroed the real payload fields, which was both validator-invalid and runtime-zero-damage.

## Task: 2026-05-28 Vega-B Triggered Second Slash And Silence Authoring

### Task title

Author Vega-B on the shared SingleAttack path and extend triggered SingleAttack so the delayed second slash can carry OnHit silence.

### Goals

- Keep Vega-B on `SingleAttack` with the user-provided `Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- Implement base silence, trait-2 silence duration bonus, trait-5 Name Mark application, master-1 delayed second slash, and master-2 10-stack silence extension.
- Avoid a Vega-only helper runtime or hidden extra active-skill slot.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Authority stayed on `boards/SkillBluePrint/single-attack-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md`, routed active CSV files, and the user-provided prefab path.
- The shared runtime/common-logic extension was user-approved before implementation.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that Vega-B now emits the slash from the caster position, damages each enemy on the path once, and applies base `3s` silence.
- User verifies that trait-2 extends Vega-B silence by `+1s`, trait-5 adds `name-mark` `+2` on hit, master-1 fires the delayed `0.4s` second slash with `45%` damage and `1s` silence, and master-2 refreshes silence by `+1s` at `name-mark>=10`.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-b` as `SingleAttack` with `hit_target_count=global`, `status_effect_id=silence`, `status_duration_seconds=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_B.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `vega-b-trait-2` and `vega-b-master-2` `RuntimeImplemented` through shared silence-duration and threshold-status fields.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now contains `vega-b-trait5-name-mark` and `vega-b-master1-second-silence`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now contains `vega-b-master1-second-slash`, which routes a delayed `SingleAttack` slash at `0.4s`, `damage_multiplier=0.45`, and links `vega-b-master1-second-silence`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now lets triggered `SingleAttack` hits carry shared `OnHit` status effects with the source-skill active-choice snapshot, so Vega-B master-1 reuses shared status gating and silence-duration bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/SingleAttackSkillExecutor.cs` now anchors `HitAllTargets` prefab hitboxes at the caster position, which matches the Vega-B slash-path prefab behavior instead of centering the hitbox on the target group.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.

### History

- 2026-05-28: Initial inspection confirmed Vega-B was already authored as `SingleAttack`, so the work stayed on the shared SingleAttack blueprint path instead of the beam blueprint.
- 2026-05-28: The user considered a hidden follow-up skill row for the second slash, but current active-slot validation and learned-runtime loading made that path larger than a small shared triggered-SingleAttack extension.

## Task: 2026-05-28 Vega-A Shared Projectile Runtime Extension And Skill Authoring

### Task title

Extend the shared projectile runtime for Vega-A burst timing, per-burst damage rules, and follow-up shadow shots, then author the active Vega-A data on that path.

### Goals

- Keep Vega-A on the projectile blueprint path instead of adding a Vega-only runtime.
- Author the inspected reference values for 3-hit burst timing, third-hit bonus, Name Mark application, trait-4 last-hit bonus, trait-5 conditional damage, and master-1 shadow follow-up.
- Keep master-2 grounded on the user-provided slash coefficient and prefab path without adding a Vega-only runtime.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Authority stayed on `boards/SkillBluePrint/projectile-blueprint.md`, `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md`, the routed active CSV files, and the user-provided prefab path `Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- The base reference did not provide a numeric slash-damage value for master-2, but the user later provided `attack coefficient 0.5` and `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab` as explicit authority.

### Role Owner

Code Builder / Skill Builder

### Status

Implemented, compile-verified, and Unity editor-validated.

### Next Actions

- User verifies in Play Mode that Vega-A fires 3-hit bursts with `0.12s` internal spacing and `0.55s` outer cadence.
- User verifies that trait-4 boosts only the last burst hit, trait-5 boosts only targets with at least 10 `name-mark` stacks, and master-1 spawns one next-frame shadow projectile at `45%` damage.
- User verifies in Play Mode that master-2 kill triggers now deal the small slash through the shared triggered-effect path and use `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/a-three-sword-flurry.md` specifies 3 bullets, `3번째 탄환 200%`, shot interval `0.55`, bullet interval `0.12`, hit-applied `name-mark` 1 stack, trait-4 last-hit `+50%`, trait-5 `+25%` vs `name-mark` 10+, and master-1 shadow projectile `45%`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now authors `vega-a` with `shot_interval_seconds=0.55`, `burst_interval_seconds=0.12`, `projectile_burst_count=3`, `burst_damage_projectile_index=3`, `burst_damage_multiplier=2`, `status_effect_id=name-mark`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now authors `vega-a-trait-4` through the shared last-burst-hit multiplier path, `vega-a-trait-5` through the shared conditional target-status multiplier path, and `vega-a-master-1` through the shared follow-up projectile path.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors `vega-a-master2-transfer-mark` as a shared `Damage` effect with `attack_power_coefficient=0.5`, `status_stack_amount=3`, and `skill_effect_prefab_path=Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` and `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now mark Vega-A master-2 `RuntimeImplemented` on the existing nearest-enemy OnKill trigger/effect path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Runtime/SkillRuntimeInstance.cs`, `.../Execution/Runtime/SkillExecutionSnapshot.cs`, and `.../Execution/Executors/ProjectileSkillExecutor.cs` now carry separate burst interval, burst-index damage rules, and follow-up projectile execution on the shared projectile runtime path.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` now accepts shared `Damage` effect rows with positive `attack_power_coefficient` or `spell_power_coefficient` even when `base_damage=0`, matching the actual runtime formula used by `SkillExecutionUtility.ResolveDamage(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; only the pre-existing `MSB3277` warnings remained.
- Unity refresh completed to `idle`, and the filtered Unity console returned no CSV/runtime errors after the trigger-row contract fix.

### History

- 2026-05-28: User first challenged whether burst-internal spacing already existed from Sein-B; re-inspection confirmed the existing shared burst path and narrowed the required extensions to shared burst-index damage rules and shared follow-up projectile support.
- 2026-05-28: The new Vega master-2 trigger row initially failed CSV parsing because `monster_skill_triger.csv` requires a non-empty `triggered_skill_id`; the row was corrected and Unity validation then completed without further errors.
- 2026-05-28: User later provided the missing master-2 slash authority as `attack coefficient 0.5` plus `Assets/Prefab/Skill/Vega/Vega_A_Master_2.prefab`, which completed the existing trigger/effect implementation path without further shared code changes.
- 2026-05-28: Unity source validation then exposed a shared mismatch: coeff-only `Damage` effect rows were runtime-valid but validator-invalid. Builder fixed the shared validator so Vega-A master-2 and future coeff-only damage effects no longer require fake positive `base_damage`.

## Task: 2026-05-18 Vega-B SingleAttack Runtime Kind

### Task title

Route Vega-B through the new SingleAttack runtime kind for one-shot area damage.

### Goals

- Move Vega-B out of `LineAttack` because the requested CSV row belongs to one-shot `SingleAttack`.
- Preserve existing CSV-authored damage, coefficient, radius, and cooldown.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Vega-B now behaves as a one-shot area hit in the current shared executor path.

### Evidence

- `Pakuri/reference/2.Monster/vega/skill/b-silent-greatblade.md` names Vega-B `移⑤У????쒕룄`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `vega-b runtime_kind=SingleAttack`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SingleAttackData.cs` and `SkillExecutors.cs` provide the data and executor path.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User listed CSV row 34 as a one-shot area attack skill for the new `SingleAttack` type.
