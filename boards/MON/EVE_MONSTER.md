## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/EVE_MONSTER_ARCHIVE_2026-05-18.md`.
- Older monster-wide history remains in `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md` and `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md`.
- This active file now keeps only the current Eve A-J runtime baseline still useful for ongoing work.

# EVE_MONSTER

This is the active Eve-domain persistent state file.
When doing related work, follow `MDTREE.md` routing and update this file together with any required parent or child files.

## Scope

- Active focus is the Scripts2 `NewRunScene` Eve A-J path.
- Older RunScene/Manifested/CombatRuntime detail is preserved in archive files and should be read only when older history is actually needed.

## Cross-Board Update Requirements

- Status work: update this file and `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.
- Data/catalog/Offering work: update this file together with `boards/DATA/DATA_BLACKBOARD.md` and `boards/DATA/GAMEDATA_ASSET_BLACKBOARD.md`.
- NewRunScene UI or Offering gating changes: update this file and `boards/UI/RUNSCENE_UI.md`.
- Eve reports: update this file when a report changes active Eve facts. There is no active report board.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-12 Eve Runtime Visual And Hitbox Migration

### Task title

Move Eve A-E and Eve-C master-2 visual/hitbox construction from prefab instantiation to shared runtime composition.

### Goals

- Build Eve A-E visuals from CSV-owned Sprite, AnimatorController, scale, sorting order, and optional BoxCollider2D data.
- Preserve Eve-D per-shocked-target overlapping collider deployments and Eve-C/E collider-backed zone behavior.
- Keep Eve-C master-2 damage/target/timing on its existing OnExpire Effect graph while replacing only its prefab visual with runtime composition.
- Retain all Eve skill prefab assets and current scene mappings until the later all-monster cleanup pass.

### Constraints

- Role Owner is Code Builder refactoring track; skill blueprints were intentionally not used.
- No offset CSV fields were added; `RuntimeSkillVisualFactory` continues to use `Vector2.zero`.
- Player-facing timing, damage, targeting, node composition, and prefab assets must remain unchanged.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented, CSV/catalog validated, and both C# projects build with 0 errors. The Eve-B/E unwanted `RuntimeStatusVisual` inheritance bug is fixed; Play Mode parity verification remains.

### Next Actions

- User verifies Eve A projectile collision, Eve-B beam width/duration, Eve-C/E collider zones and radius traits, Eve-D global overlapping deployments, and Eve-C master-2 expiry visual in Play Mode.
- User verifies Eve-B slow and Eve-E vulnerable no longer create a separate `RuntimeStatusVisual`, while their legitimate skill visuals still appear.
- Keep `Assets/Prefab/Skill/Eve/*.prefab` until all monster skill visual migrations are complete.

### Evidence

- Eve A/B/C/D/E base rows now carry `runtime_visual_*` data; A/C/D/E also carry their prefab-authored BoxCollider2D sizes.
- `BeamSkillExecutor.cs` and `ZoneSkillExecutor.cs` now prefer shared `RuntimeSkillVisualFactory` composition while preserving their old prefab fallback for unconverted skills.
- `RuntimeEffectVisual` was added to the normalized node definitions; Eve-C master-2 uses it instead of `Eve_c-master-2.prefab` while retaining `EffectDamage` and `EffectTarget(OnExpire)` nodes.
- `SkillMultiEffectExecutor.cs` now supports runtime visual creation for transient, attached, and zone effect visuals.
- No Eve prefab was deleted and `NewRunScene.unity` was not edited.
- Unity-MCP `Pakuri/Validate CSV Source Data` loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies without validation errors.
- Runtime and Editor `dotnet build --no-restore /p:UseSharedCompilation=false` completed with 0 errors and the existing two MSB3277 warnings.
- `StatusEffectRuntime.CreateStatusData(...)` now copies a skill runtime visual only when its anchor is explicitly `StatusTarget`; Eve-B/E remain at the default `Skill` anchor.
- `InGameCombatManager` creates status-attached runtime visuals with `includeHitbox: false`, preventing a status decoration from inheriting Eve-E's gameplay collider even if visual data is misrouted later.

### History

- 2026-07-12: User approved the work as a Code Builder refactor, required all offsets to stay implicit zero, and deferred prefab deletion until every skill migration is complete.
- 2026-07-12: Code Builder implemented shared Beam/Zone/Effect runtime visuals and migrated Eve A-E plus Eve-C master-2 data.
- 2026-07-12: Fixed Eve-B/E base visuals being reused as `RuntimeStatusVisual`; Eve-D and all Eve prefab assets were left unchanged.

## Task: 2026-07-11 Eve A-J Skill Graph Migration Proposal

### Task title

Design the Eve A-J migration from wide legacy skill data to the current Ariel-style skill graph structure.

### Goals

- Decompose Eve A-J base behavior, 25 active traits, 10 active masters, and 15 passive traits into base, Plan, Effect, and Trigger ownership.
- Reuse current graph nodes and existing wide-runtime features before proposing new common semantics.
- Discard the old magazine Eve-E behavior and use the revised `e-drone-beacon.md` non-magazine zone as the migration authority.
- Identify missing graph files, graph exposure nodes, owner extensions, and genuinely new runtime meanings before implementation.

### Constraints

- Role Owner is Code Builder / Skill Builder.
- Implementation follows the approved `boards/MON/EVE_NODE_MIGRATION_PROPOSAL.md` and the matching per-kind Skill Blueprints.
- Evidence is limited to routed Eve A-J runtime CSV rows, current node definitions, graph/runtime consumers, build output, and Unity-MCP CSV validation.
- Active awakening level rows are not present in the current Eve Choice CSV and remain outside this migration proposal.
- No MSW-MCP was used.

### Role Owner

Code Builder / Skill Builder

### Status

Eve A-J graph migration implemented. Source CSV validation and both C# builds pass; user Play Mode verification remains.

### Next Actions

- User verifies Eve A-J base, enhancement, and master combinations in `NewRunScene` Play Mode.
- Pay particular attention to Eve-D overlapping full-field deployments, Eve-E one-generation recast, Eve-G 4%+3% proc composition, and Eve-H status-expire graph damage.

### Evidence

- Created `boards/MON/EVE_NODE_MIGRATION_PROPOSAL.md`.
- Current Eve runtime aggregation returned 10 base rows, 50 Choice rows, 0 graph rows, 34 legacy effect rows, 3 trigger rows, and 0 legacy direct nodes.
- Current graph files exist for projectile, single-attack, and passive, while Eve-B line-attack and Eve-C/E area-attack graph files do not exist.
- The proposal retains existing graph nodes for ordinary damage/cooldown/radius/status/effect composition, exposes existing wide runtime features through graph nodes, and isolates actual new common semantics.
- Eve-D scans the full enemy roster for shocked targets and creates one independent collider-authored deployment at every match; the prefab Collider owns the base footprint and overlapping deployments can damage the same enemy multiple times.
- `StatusFilteredDeployment` is reclassified as graph exposure of the existing wide base/runtime path, and the obsolete `DeploymentSearchRadiusMultiplier` proposal is removed.
- The remaining genuinely new meanings are additive target-status stack damage rate and zone recast.
- Eve-E `약점 고정` reuses `StatusCriticalDamageTakenBonus(0.01)` because `StatusEffectRuntime.SumStacked` already multiplies status data by the vulnerable runtime stack count; only `StatusMaxStacksBonus` needs graph exposure.
- Eve-E is re-authored from the revised reference as a radius 3.2, 5-second, 0.8-second tick, 10-second cooldown non-magazine zone; `플라즈마 붕괴` becomes a guarded one-generation zone recast.
- Current passive base names G-J are shifted relative to the references; the proposal corrects them to G 입자 분리, H 냉각 알고리즘, I 과전류 회로, J 약점 분석.
- Added line-attack and area-attack graph CSVs and authored Eve graphs: projectile 18 rows, line-attack 21 rows, area-attack 30 rows, single-attack 13 rows, and passive 147 rows.
- Removed all 34 replaced Eve legacy effect rows; Eve-G triggers were consolidated from two rows to one base trigger plus `TriggerProcChanceBonus`, and Eve-H now references a Choice Effect graph.
- Implemented shared graph exposure and runtime support for duration/projectile/tick/status/conditional modifiers, target-status stack-rate damage, trigger proc bonuses, status-filtered deployment, and Zone-only `RecastZone`.
- Eve-E is now base radius 3.2, duration 5, tick 0.8, cooldown 10, magazine/reload 0, vulnerable max stacks 10; `플라즈마 붕괴` recasts once after 0.5 seconds for 3 seconds at radius 60%.
- Unity-MCP `Pakuri/Validate CSV Source Data` completed with only the successful 5-monster/8+8-enemy catalog load log.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; only the existing two assembly-version warnings remain.
- Eve-D base now uses `radius=0` so CSV does not impose an absolute footprint and `hit_target_count=global` so every enemy overlapping each spawned Collider can be hit. Existing `RadiusMultiplier` nodes continue scaling the prefab root, Sprite, and Collider together.

### History

- 2026-07-11: User requested an Eve A-J node migration proposal modeled on the Ariel guide, including every trait/master, Eve-E replacement behavior, existing-feature reuse, and reasons for each new node proposal.
- 2026-07-12: User changed Eve-D from a limited search radius to one full-map scan; the reference and proposal now remove search-range ownership, keep per-target radius-1.8 explosions, and allow overlapping explosion damage.
- 2026-07-12: Code Builder implemented the approved A-J migration, removed replaced legacy rows, regenerated runtime catalogs through Unity validation, and completed compile/data verification. Play Mode verification remains user-owned.
- 2026-07-12: After the user added an enabled `BoxCollider2D` to `Eve_D.prefab`, Code Builder set Eve-D `radius=0` and `hit_target_count=global`; Unity CSV validation passed and Play Mode hit verification remains user-owned.

## Task: 2026-07-11 Eve-E Non-Magazine Field Reference Redesign

### Task title

Redesign the Eve-E reference as a non-magazine area field using Eve-C's field structure.

### Goals

- Remove magazine, reload, single-target tick, and concurrent-field assumptions from the Eve-E design reference.
- Give Eve-E the same area/duration/tick/cooldown tuning axes as Eve-C while preserving Lightning and vulnerable identity.
- Replace the magazine-dependent master with a one-time delayed field recast suitable for later graph-node implementation.
- Keep this pass documentation-only so runtime CSV and code conversion can occur with the later Eve node migration.

### Constraints

- Role Owner is Designer.
- Only `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` is changed as the skill-design authority in this pass.
- `skills_area_attack.csv`, choice/effect/trigger CSVs, runtime code, prefabs, and scenes remain unchanged.
- Values are grounded in the inspected Eve-C reference and the current Eve-C/E area-attack CSV rows.
- No MSW-MCP was used.

### Role Owner

Designer

### Status

Eve-E reference redesigned; runtime implementation intentionally deferred to node conversion.

### Next Actions

- During Eve graph migration, change Eve-E base data from magazine/single-target behavior to the documented radius 3.2 non-magazine field with a 10-second cooldown.
- Convert the five traits into field/Choice nodes and implement `플라즈마 붕괴` as a delayed one-time field-recast Effect/Trigger graph.
- The recast must inherit the ended field's final damage/tick/critical/vulnerable snapshot, use 60% of its final radius for 3 seconds, and suppress recursive `플라즈마 붕괴` activation.
- Preserve `약점 고정` vulnerable-stack identity and verify whether its current behavior requires a shared node definition/runtime extension.
- User verifies final field cadence, full-area targeting, vulnerable stacking, and master behavior in Play Mode after implementation.

### Evidence

- `c-frost-field.md` defines Eve-C as a non-ammunition area field with radius 3.2, duration 4 seconds, tick interval 0.5 seconds, and cooldown 8 seconds.
- The current `skills_area_attack.csv` disk row still defines Eve-E with radius 0, hit-target count 1, magazine 3, reload 6 seconds, duration 5 seconds, and tick interval 0.8 seconds.
- Updated `e-drone-beacon.md` to define Eve-E as radius 3.2, duration 5 seconds, tick interval 0.8 seconds, cooldown 10 seconds, and damage to every enemy in the field.
- Traits 1-4 now use the Eve-C field axes: radius/duration, tick/status, damage/cooldown, and radius-for-damage tradeoff; trait 5 retains the vulnerable-5 Lightning damage condition.
- Replaced `감시 드론망` with `플라즈마 붕괴`: 0.5 seconds after the original field ends, it recasts once at the ended position for 3 seconds with 60% of the original field's final radius.
- The reference explicitly carries the original final damage/tick/critical/vulnerable snapshot into the recast and forbids the recast from triggering another collapse, preventing an infinite loop.
- Updated the `플라즈마 붕괴` awakening progression to scale delay, recast duration, and recast radius instead of obsolete explosion damage.
- `약점 고정` remains the second master because it is independent of magazine ownership and preserves Eve-E's vulnerable-stack specialization.

### History

- 2026-07-11: User requested changing only the Eve-E reference now, using Eve-C as the field baseline, while deferring runtime changes until Eve's node-based conversion.
- 2026-07-11: User replaced the field-end explosion with a one-time recast at the ended location after 0.5 seconds for 3 seconds at 60% radius.

## Task: 2026-05-24 Eve F-J Passive Runtime Completion

### Task title

Implement Eve passive skills F-J on shared passive/effect/trigger runtime paths and finish the interrupted `SkillTriggerRuntime.cs` follow-up.

### Goals

- Keep Eve-F/J passive behavior data-owned through `monster_skill_effects.csv`, `monster_skill_triger.csv`, and `monster_skill_choices.csv`.
- Support Eve-F combat-start shield plus shocked-target modifiers, Eve-G Lightning/Ice ally buffs plus auto Prism Ray trigger, Eve-H chill/freeze target modifiers plus freeze-expire burst, Eve-I shocked/shock-5 Lightning amplifiers, and Eve-J vulnerable multi-resistance debuffs.
- Keep all new behavior on shared runtime/status/trigger code paths instead of adding Eve-only executor branches.

### Constraints

- Role Owner is Skill Builder / Code Builder.
- The selected authority stayed on `boards/SkillBluePrint/passive-stat-blueprint.md`, the inspected Eve CSV rows, and the explicitly edited runtime/data files.
- Unity Play Mode gameplay verification remains user-owned.
- External Code Reviewer was not run because explicit user permission was not given.

### Role Owner

Skill Builder / Code Builder

### Status

Implemented, build-verified, and Unity CSV validation passed.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-F gives the combat-start shield only to allies with at least one Lightning active skill and that trait 3 grants action speed only while shielded.
- User verifies Eve-G auto-casts Eve-B from allied Lightning/Ice outgoing damage with the shared internal cooldown and that trait 3 only boosts Eve-B against shielded targets.
- User verifies Eve-H freeze-expire burst, Eve-I shock-5 Lightning resistance reduction, and Eve-J vulnerable damage/resistance amplification on live enemies.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now marks `eve-f-trait-1` through `eve-j-trait-3` as `RuntimeImplemented`; `eve-g-trait-3` now targets `eve-b`, `eve-i-trait-3` now targets `eve-d`, and `eve-j-trait-3` now targets `eve-e`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` now authors Eve F-J passive rows such as `eve-f-start-shield`, `eve-h-status-chance`, `eve-i-shock5-lightning-resist`, and the `eve-j-vulnerable-*-resist` family.
- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv` now authors `eve-g-auto-prism-ray`, `eve-g-auto-prism-ray-trait1`, and `eve-h-freeze-expire-burst`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillTriggerRuntime.cs`, `Skills/Execution/SkillExecutors.cs`, and `Skills/Data/StatusEffectRuntime.cs` now share condition-status parsing, trigger-attribute matching, and runtime-kind checks needed by Eve G/H/I/J.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the existing `MSB3277` warnings remained.
- Unity `Pakuri/Validate CSV Source Data` completed successfully and logged `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`

### History

- 2026-05-24: User asked Skill Builder to resume the interrupted Eve F-J passive implementation that had stopped during the added `SkillTriggerRuntime.cs` work.

## Task: 2026-05-17 Eve A-J Active Runtime Baseline

### Task title

Keep the current Eve A-J Scripts2 runtime state compact and explicit.

### Goals

- Preserve the current Eve A-J data/Offering baseline from the active CSV source files.
- Preserve the shared status-runtime foundation and visible label output used by Eve-A shock.
- Preserve Eve-A projectile modifier execution through the shared InGame execution path.
- Keep the board explicit that Eve B-E executor depth and F-J passive effect depth still remain later work.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older Eve slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active Eve baseline summarized and retained for future work. 2026-05-18 Eve-A/Eve status values are now read from `monster_skills.csv`. 2026-05-18 supported Korean status labels can now resolve through the shared status parser.

### Next Actions

- User verifies in `NewRunScene` Play Mode that Eve-A shock, modifier choices, and Offering gating behave as recorded.
- Continue later Eve work from the shared status/runtime path instead of reintroducing Eve-only special-case state.
- Use the archive snapshot when older prefab-binding or CombatRuntime-era Eve history is needed.

### Evidence

- `Pakuri/Assets/CSVdata/source/monster_skills.csv`, `monster_skill_choices.csv`, and `monster_modifier_skill_choice.csv` hold the retained Eve A-J source rows and active choice/modifier mappings.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs`, `SkillExecutionSystem.cs`, `SkillRuntimeInstance.cs`, and `InGameProjectileActor.cs` own the current Eve-A projectile modifier, branch, and shock execution path.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectData.cs`, `StatusEffectKind.cs`, `InGameSkillDefinitionMapper.cs`, and `BaseUnitRuntimeModel.cs` own the retained shared status foundation used by Eve work.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` was recorded as the current Offering gating point for learned active/passive Eve reward choices.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now stores Eve-A `projectile_speed=15`, `pierce_count=0`, `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=媛먯쟾`; Eve-B/C/D/E status rows are `slow`/`chill`/`shock`/`vulnerable` with labels.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains an Eve-A-only shock chance override; `InGameSkillDefinitionMapper.cs` now maps status chance from CSV into `StatusApplicationSpec`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels such as `媛먯쟾`, `?뷀솕`, `異붿쐞`, and `痍⑥빟`, and `InGameSkillDefinitionMapper.cs` can use a parseable `status_effect_label` when `status_effect_id` is blank.
- `Import-Csv -Encoding UTF8 Pakuri\Assets\CSVdata\source\monster_skills.csv` shows Eve's positive runtime statuses as `eve-a shock 0.15 媛먯쟾`, `eve-b slow 0.2 ?뷀솕`, `eve-c chill 1 異붿쐞`, `eve-d shock 1 媛먯쟾`, and `eve-e vulnerable 1 痍⑥빟`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-17: Eve A-J source data, Offering mapping, shared status foundation, Eve-A projectile modifier execution, and visible status label behavior became the current active baseline.
- 2026-05-18: Code Builder moved Eve-A shock chance and projectile speed from hardcoded/monster-level data into the Eve skill row.
- 2026-05-18: Code Builder added supported Korean status-label parsing/fallback and CSV runtime sync batch support.

## Task: 2026-06-07 Eve Animation Clip Controller And Prefab Wiring

### Task title

Create Eve's shared Rin-contract animation assets and wire the monster prefab animator.

### Goals

- Create Eve's six animation clips: attack 1, attack 2, attack 3, idle, hit, and death.
- Create `Eve_Animation_Cont.controller` with the same parameter contract as Rin: `Attack`, `AttackIndex`, `Hit`, and `Death`.
- Add Animator and `Animation_Controller` components to `Eve_Unit.prefab` and connect `MonsterUnitActor.animationController`.

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
- User verifies in Play Mode that Eve plays idle, attack 1-3, hit, and death through the shared animation parameter contract.

### Evidence

- `Pakuri/Assets/Image/Monster/Eve/Animation/Animation_Eve_Sprite` now contains 6 `Anim_Eve_*.anim` files, 6 matching `.anim.meta` files, `Eve_Animation_Cont.controller`, and `Eve_Animation_Cont.controller.meta`.
- `Select-String` confirmed `Eve_Animation_Cont.controller` contains `Attack`, `AttackIndex`, `Hit`, `Death`, and the states `Anim_Eve_Attack_1`, `Anim_Eve_Attack_2`, `Anim_Eve_Attack_3`, `Anim_Eve_Hit`, `Anim_Eve_Idle`, and `Anim_Eve_Dead_1`.
- `Pakuri/Assets/Prefab/Monster/Eve_Unit.prefab` now has `animationController: {fileID: 900200000000002}`, an `Animator` with controller GUID `cc69556112bc45619ea4177c77ae95dc`, and an `Animation_Controller` with `idleState: Anim_Eve_Idle`, `deadState: Anim_Eve_Dead_1`, and `attackStateCount: 3`.
- The controller meta GUID check returned `Eve controllerGuid=cc69556112bc45619ea4177c77ae95dc linked=True`.
- The generated idle clip check returned `Eve idleName=Anim_Eve_Idle spriteRefs=16`.
- 2026-06-07 follow-up correction verified `Eve root=4596420534878418281 rootRefs=true animatorOwner=4596420534878418281 controllerOwner=4596420534878418281 ok=true` after fixing the generated Animator and `Animation_Controller` component owner fileIDs to the root `Eve_Unit` GameObject.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing `MSB3277` warnings remained.

### History

- 2026-06-07: User asked Code Builder to create each monster's six animation clips, create controllers with Rin's parameter contract, and wire each monster prefab Animator controller.
- 2026-06-07: User reported the non-Rin monster prefabs still did not show assigned Animator / `Animation_Controller`; Code Builder found the generated component blocks were owned by the wrong GameObject fileID and corrected them to the root Unit GameObject.

## Task: 2026-05-18 Eve C/D/E Runtime Kind And Names

### Task title

Correct Eve C/D/E names and AreaAttack/SingleAttack runtime kinds from reference files.

### Goals

- Keep Eve C named `프로스트 필드`, not translated as `서리 지대`.
- Keep Eve D named `스태틱 오버라이드`, not translated as `정전기 과부하`.
- Route Eve C/E as sustained `AreaAttack` and Eve D as one-shot `SingleAttack`.

### Constraints

- Role Owner is Code Builder.
- Eve C/D/E names are grounded in `Pakuri/reference/2.Monster/eve/skill`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C ticks for 4 seconds, Eve E ticks for 5 seconds, and Eve D performs a one-shot area hit.

### Evidence

- `Pakuri/reference/2.Monster/eve/skill/c-frost-field.md` lists `스킬명 | 프로스트 필드`.
- `Pakuri/reference/2.Monster/eve/skill/d-static-override.md` lists `스킬명 | 스태틱 오버라이드`.
- `Pakuri/reference/2.Monster/eve/skill/e-drone-beacon.md` lists `스킬명 | 플라즈마 필드`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-c` display `프로스트 필드`, runtime `AreaAttack`, tick interval `0.5`, and active duration `4`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-d` display `스태틱 오버라이드` and runtime `SingleAttack`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now has `eve-e` runtime `AreaAttack`, tick interval `0.8`, and active duration `5`.
- Eve passive descriptions in `monster_skills.csv` now refer to `프로스트 필드` and `스태틱 오버라이드`.
- Runtime/editor builds passed with 0 errors; Unity-MCP skill validator returned 0 errors and 0 warnings.

### History

- 2026-05-18: User reported that the issue was not CSV corruption but wrong translated/hardcoded skill naming and requested Code Builder correction.
