## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/ARIEL_MONSTER.md`.

# ARIEL_MONSTER

## Scope

Ariel dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Ariel file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Task: 2026-06-19 Ariel Effect Object Node Pilot

### Task title

Implement the first Ariel normalized-node pilot for numeric choice modifiers and Ariel C blessing composition.

### Goals

- Move Ariel numeric choice effects from wide `monster_skill_choices.csv` fields to reusable normalized nodes.
- Prove Ariel C can reduce pre-combined blessing rows by composing base effect rows with trait/passive nodes.
- Keep old effect rows only as compatibility rows when not yet replaced by the generic node path.

### Constraints

- Role Owner is Code Builder.
- User selected generic `monster_skill_nodes.csv` and `monster_skill_node_params.csv`; no new specialized effect tables were added.
- User answered D trait 5 condition is attacker-self shield, J requires Ariel-E-generated shield, I exposure damage taken applies to all incoming damage, passives are always active, and durations stay seconds.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and Unity-MCP validated.

### Next Actions

- User verifies Ariel C base, trait2, trait3, H trait3, trait5, master1, and master2 combinations in Play Mode.
- Later pass should implement source-specific shield checks so Ariel J can require Ariel-E-generated shield instead of generic shield.
- Continue Ariel B/E/passive ownership cleanup only after Ariel C parity is confirmed.

### Evidence

- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_nodes.csv` now contains Ariel normalized choice nodes for damage, cooldown, magazine, reload, pierce, duration, shield-count damage, status-conditional damage-taken, and status modifier bonuses.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_node_params.csv` now carries the matching values; initial migration output reported `migrated=28 nodes=47 params=68`, and the final Ariel C trait2 targeted action-speed node addition brought the parsed param row count to 69.
- TextFieldParser CSV shape check returned `monster_skill_choices.csv header=114 rows=252 bad=`, `monster_skill_nodes.csv header=14 rows=47 bad=`, `monster_skill_node_params.csv header=4 rows=69 bad=`, and `monster_skill_effects.csv header=70 rows=131 bad=`.
- `ariel-c-trait-2-blessing-action-speed` is a `StatusActionSpeedBonus` node with `status_id=blessing` and `bonus=0.06`.
- `Pakuri/Assets/CSVdata/runtime/monster/skills/monster_skill_effects.csv` keeps Ariel C base rows but disables 9 pre-combined rows as `MigratedToEffectBinding`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` now applies normalized choice nodes during combat snapshot creation and resolves status-targeted action speed bonuses.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now applies snapshot status overrides through `SkillStatusSpecUtility.ResolveStatusData(...)`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged `InGame skill data validation passed with 0 warning(s).`

### History

- 2026-06-19: User requested Code Builder implementation of `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md` and provided answers to all ambiguous design questions.

## Task: 2026-06-19 Ariel Phase 2-5 Effect Object Cleanup

### Task title

Continue Ariel B/E/A/D/F-J normalized node, trigger-binding, and passive ownership cleanup after the Ariel C pilot.

### Goals

- Move Ariel B shield amount modifiers onto a shield-specific normalized node handler.
- Reduce Ariel E shield variants to one active shield effect plus shield amount nodes.
- Move Ariel J post-E action-speed behavior out of Ariel E effect rows into J-owned trigger/effect rows.
- Keep Ariel A/B/D trigger rows as explicit specialized trigger-binding compatibility rows.
- Add a source-specific effect condition so Ariel J shielded holy damage requires the Ariel E shield effect, not any shield.

### Constraints

- Role Owner is Code Builder, followed by one Code Reviewer pass requested by the user.
- The implementation stays on generic `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- No MSW-MCP is used; Unity checks use Unity-MCP only.
- Unity Play Mode parity remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, compiled, CSV-checked, and Unity-MCP validated. Code Reviewer pass pending in the same user request.

### Next Actions

- Run the requested Code Reviewer pass against the current diff.
- User verifies Ariel B shield amount/duration, E shield trait/master combinations, J after-E action-speed, and J Ariel-E-shield-only holy damage in Play Mode.

### Evidence

- `SkillChoiceEffectSpec`, `SkillExecutionSnapshot`, `InGameSkillDefinitionMapper`, `SkillExecutionUtility`, and `SkillMultiEffectExecutor` now carry `ShieldAmountMultiplier`; active shield skill bodies and status-effect shield amounts can use shield-specific normalized choice nodes.
- `SkillEffectDefinition`, `PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, `StatusEffectRuntime`, and `SkillMultiEffectExecutor` now carry `condition_status_source_skill_id` for effect condition checks.
- `monster_skill_nodes.csv` contains four Ariel `ShieldAmountMultiplier` nodes: B trait1, B master1, E trait2, and E master2.
- `monster_skill_effects.csv` has exactly one active `ariel-e-shield*` row, while `ariel-e-shield-trait2`, `ariel-e-shield-master2`, and `ariel-e-shield-trait2-master2` are `MigratedToEffectBinding`.
- `monster_skill_effects.csv` no longer contains `ariel-e-passive-j-*`; the post-E action-speed effects now live as `ariel-j-after-e-action-speed` and `ariel-j-after-e-action-speed-trait1`.
- `monster_skill_triger.csv` contains J-owned `OnSkillCast` trigger rows for `event_skill_id=ariel-e`, including the trait1-gated trigger.
- `ariel-j-shielded-holy-damage` now has `condition_status_id=shield` and `condition_status_source_skill_id=ariel-e-shield-base`, matching the actual effect-created shield status source.
- CSV field-count check returned no bad rows for `monster_skill_choices.csv`, `monster_skill_nodes.csv`, `monster_skill_node_params.csv`, `monster_skill_effects.csv`, and `monster_skill_triger.csv`.
- Acceptance spot-check returned `eActiveShieldRows=1`, `eDisabledShieldVariants=3`, `shieldAmountNodes=4`, `oldEJRows=0`, `jTriggerRows=2`, and `jShieldSource=ariel-e-shield-base`.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore /p:UseSharedCompilation=false` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore /p:UseSharedCompilation=false` passed with 0 errors; existing `MSB3277` warnings remained.
- Unity-MCP `Pakuri/Sync CSV Runtime Catalog Assets`, `Pakuri/Validate CSV Source Data`, and `Pakuri/InGame/Validate Skill Data` completed; console logged catalog load and `InGame skill data validation passed with 0 warning(s)`, with 0 error/warning console entries.

### History

- 2026-06-19: User requested Code Builder to perform the remaining Phase 2-5 work from `Pakuri/reference/Report/2026-06-19-ariel-effect-object-trigger-binding-handoff.md`, then run Code Reviewer.

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

## Task: 2026-06-07 Ariel Animation Clip Controller And Prefab Wiring

### Task title

Create Ariel's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Ariel's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Ariel_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Ariel_Unit.prefab` and connect `MonsterUnitActor.animationController`.

### Constraints

- Role Owner is Code Builder.
- The controller contract follows inspected `Rin_Animation_Cont.controller`.
- Unity Editor import and Play Mode animation verification were not available in this session.

### Role Owner

Code Builder

### Status

Implemented and locally YAML/build-verified.

### Next Actions

- User lets Unity import the new `.anim` and `.controller` assets.
- User verifies in Play Mode that Ariel plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/ariel/Animation/Animation_Ariel_Sprite` now contains 6 `Anim_Ariel_*.anim` files, 6 matching `.anim.meta` files, `Ariel_Animation_Cont.controller`, and `Ariel_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Ariel_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Ariel_Attack_1`, `Anim_Ariel_Attack_2`, `Anim_Ariel_Attack_3`, `Anim_Ariel_Hit`, `Anim_Ariel_Idle`, and `Anim_Ariel_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Ariel_Unit.prefab` now has `animationController: {fileID: 900100000000002}`, an `Animator` with controller GUID `b2339c033d324ea8a1f138797de25ab8`, and an `Animation_Controller` with `idleState: Anim_Ariel_Idle`, `deadState: Anim_Ariel_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Ariel controllerGuid=b2339c033d324ea8a1f138797de25ab8 linked=True`.
- The generated idle clip check returned `Ariel idleName=Anim_Ariel_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Ariel root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Ariel_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.
