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
