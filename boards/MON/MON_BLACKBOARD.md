# MON_BLACKBOARD

## Current State

Current monster-side design task: Spirit King behaves as a temporary Player-side moving monster without entering the Run party.

The previous Ariel, Eve, Rin, Sein, and Vega boards are preserved under `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/MON/`.

For new monster work, inspect the exact current code and data first, then add a required-field task block here only when persistent state is needed.

## Task: 2026-08-09 LineAttack Range Upgrade Runtime Repair

### Task title

Eve-B, Rin-C, Vega-B LineAttack 너비 강화·마스터 효과 적용 복구

### Goals

- LineAttack 계열의 너비 변경 노드를 실제 실행 너비 계산에 연결한다.
- 중복된 `BeamWidthBonus` 전용 경로를 제거하고 공용 `RadiusMultiplier` 계약으로 통합한다.

### Constraints

- 기존 강화 수치 의미를 보존한다: Rin-C `+25%`, `-25%`, `+60%`는 각각 배율 `1.25`, `0.75`, `1.60`이다.
- AreaAttack·SingleAttack의 기존 `RadiusMultiplier` 동작을 변경하지 않는다.

### Role Owner

Code Builder

### Status

구현 및 자동 검증 완료. Play Mode 체감 확인만 사용자 소유로 남음.

### Next Actions

- Play Mode에서 Eve-B, Rin-C, Vega-B의 해당 강화 전후 이펙트 너비와 판정 범위를 비교한다.

### Evidence

- `SkillExecution.cs`의 LineAttack 너비 계산이 `snapshot.RadiusMultiplier`를 사용한다.
- LineAttack 선택 CSV의 너비 변경 6개 노드는 모두 `RadiusMultiplier`를 사용한다.
- `Pakuri/Assets` 전체에서 `BeamWidthBonus` 참조는 0개다.
- `Pakuri.sln` 빌드 오류 0개, Unity EditMode 38/38 통과.

### History

- 2026-08-09 Code Builder가 미적용 원인을 LineAttack 실행식과 노드 handler 불일치로 확인했다.
- 2026-08-09 Rin-C 데이터 변환, 실행식 통합, `BeamWidthBonus` enum/state/parser/schema 데드코드 삭제를 완료했다.

## Task: 2026-08-07 MainMenu Monster Standing Text

### Task title

몬스터 정의의 표시 이름·역할 설명을 MainMenu Standing에 연결한다.

### Goals

- `display_name`을 `MonsterDefinition.DisplayName` 경로로 사용한다.
- `role_summary`를 `MonsterDefinition.RoleSummary` 경로로 사용한다.

### Constraints

- 몬스터 CSV와 정의 모델을 변경하지 않는다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. 기존 정의·카탈로그 데이터를 재사용했다.

### Next Actions

- Play Mode에서 Ariel, Eve, Rin, Sein, Vega의 텍스트 전환을 확인한다.

### Evidence

- `monsters.csv`에 `display_name`, `role_summary` 열과 5개 몬스터 값이 존재한다.
- `CsvRowParser`·`GameDataCatalogBuilder`가 해당 값을 `MonsterDefinition.DisplayName`·`RoleSummary`로 구성한다.
- `MainMenuUIManager.SelectMonster`가 두 값을 TMP 텍스트에 할당한다.

### History

- 2026-08-07: Code Builder가 몬스터 표시 데이터의 MainMenu 텍스트 출력을 연결했다.

## Task: 2026-08-07 MainMenu Monster Standing Selection

### Task title

플레이 가능 몬스터 선택 시 Standing 표시 데이터 연결.

### Goals

- `monsters.csv`의 `Image`를 기존 `MonsterDefinition.Image` 경로로 사용한다.
- MainMenu 선택 결과가 해당 몬스터의 Standing Sprite를 표시하게 한다.

### Constraints

- 몬스터 CSV 값·런타임 카탈로그 구조는 변경하지 않는다.
- 사용자 Play Mode 확인은 사용자 소유다.

### Role Owner

Code Builder.

### Status

구현 완료. 기존 몬스터 정의와 카탈로그 조회를 재사용했다.

### Next Actions

- Play Mode에서 Ariel, Eve, Rin, Sein, Vega 선택을 확인한다.

### Evidence

- `GameDataCatalogBuilder`가 `sourceMonster.ImagePath`를 `MonsterDefinition.Image`로 로드한다.
- `monsters.csv` 5개 행의 Image 경로와 대상 PNG 파일이 모두 존재한다.
- `MainMenuUIManager.SelectMonster`가 카탈로그 정의의 `Image`를 UI Image에 할당한다.

### History

- 2026-08-07: Code Builder가 기존 몬스터 Image 데이터 계약을 MainMenu Standing 표시에 연결했다.

## Task: 2026-08-05 Spirit King Temporary Allied Monster Design

### Task title

Reuse existing monster combat, targeting and skill execution for a moving Spirit King summon.

### Goals

- Spawn Spirit King directly from the Spirit Contract Stage effect without a Summon skill family.
- Register it as `UnitSide.Player` so `Ally/AllAllies` skills cast after spawning affect it.
- Give it existing `SingleSkillDefinition`/`ZoneSkillDefinition` skills and automatic casting.
- Move slowly toward the nearest enemy while remaining outside Run party ownership.

### Constraints

- Designer task: no C#, prefab, scene or CSV implementation.
- Use `UnitRole.Summon` so Manifest, Offering, MonsterPanel, DamageMeter party slots and Day recovery do not treat Spirit King as a normal party monster.
- Do not reuse Enemy AI wholesale; it is coupled to `EnemyCombatState`, the Enemy roster and Nexus attacks.
- Do not add `SkillRuntimeKind.Summon`, `SummonSkillDefinition` or `SummonSkillExecutor`.
- Keep summon unit and skill CSVs on the existing monster/area-attack authoring columns; do not add separate implementation metadata.

### Role Owner

Designer.

### Status

Phase 1 Spirit King source rows complete and verified; monster runtime integration not started.

### Next Actions

- After explicit Phase 2+ request, add Loading support, temporary allied-monster factory/spawn, prefab binding and separate summoned-monster catalog lookup.
- Add a movement-only controller that targets nearest enemies and respects `StatusCombatRules.CanMove` and MoveSpeed modifiers.
- Verify team buffs, enemy targeting, automatic Spirit King skills, Stage cleanup and party/UI exclusion.

### Evidence

- `CombatUnitRegistry.Register` places units in Players/Enemies from `Identity.Side`.
- `SkillTargeting.TargetList` uses `roster.Players` for a Player-side caster's `Ally/AllAllies`, excluding only Nexus where required.
- `SkillExecution.TryExecuteAutomaticSkills` scans every registered entry; `PlayerCombatInputController.CanUseAutoSkill` permits non-`EnemyCombatState` non-selected Player units.
- `NotifyPlayerUnitRegistered` immediately executes owner passives and `CombatStart`; one-shot team effects completed before Spirit King spawn are not retroactive without an additional policy.
- `EnemyActionController` is the only current transform movement path and is coupled to Enemy roster targeting and Nexus damage.
- `MenifestUI.ResolveNextManifestCandidate` consumes `GameDataCatalog.GetMonsters()`, so Spirit King cannot share the playable `Monsters` array.
- The target summon CSVs exist but are empty. The approved unit row uses HP 1000, Physical primary attribute, six defenses of 50 and move speed 0.6; no Spirit King asset paths currently exist.
- Elemental Explosion, Spirit Bombardment, Dimensional Rift and its follow-up explosion use `SingleAttack`; Elemental Storm uses `AreaAttack`. Existing `RepeatPerTarget` supplies Bombardment's two repeats after the initial cast.
- `summon_units.csv` now contains one 22-column Spirit King row; `summon_units_skill.csv` contains five 33-column rows. Strict UTF-8, reference headers, unique skill IDs, HP 1000, Physical attribute and all six defenses 50 were verified.

### History

- 2026-08-05: User changed Spirit King from a Summon skill outcome to a moving allied monster spawned through the existing unit path; Designer recorded the minimal reuse boundary.
- 2026-08-05: Designer added exact Phase 1 Spirit King unit/skill values and split Dimensional Collapse into timed pull and outcome-only explosion definitions.
- 2026-08-05: Code Builder completed the two summon CSVs without adding parser, runtime or asset paths; Unity auto-import generated their standard `TextScriptImporter` `.meta` files.

## Task: 2026-08-03 Boss HP Priority Display

### Task title

Show one highest-maximum-HP active boss through Canvas `BossHP` while lower-priority bosses retain their prefab HP displays.

### Goals

- Select active `IsBoss` enemies by `Stats.MaxHealth`.
- Hide only the selected boss's `MonsterHpBar` and show `Canvas/BossHP`.
- Move to the next highest-maximum-HP boss after the selected boss is defeated.

### Constraints

- Preserve existing boss designation and user-edited enemy prefab transforms.
- Do not mass-edit enemy prefab assets; hide the runtime `MonsterHpBar` root only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies multi-boss spawn, damage, shield, defeat, and next-boss handoff in `NewRunScene` Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/UI/InGame/Info/BossHpUI.cs` selects live `Model.IsBoss` entries by descending `Model.Stats.MaxHealth` priority and updates the selected entry each frame.
- `Pakuri/Assets/Scripts/Units/Display/UnitHpBar.cs` exposes runtime visibility for the prefab `MonsterHpBar` root; `EnemyActor` forwards the call.
- Enemy prefab scan found 16 prefabs and 0 missing `MonsterHpBar` roots.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri/Assembly-CSharp-Editor.csproj --no-restore` completed with 0 errors; existing Unity reference-conflict warnings remain.

### History

- 2026-08-03: Code Builder added highest-maximum-HP boss selection, runtime world-bar handoff, and Canvas BossHP synchronization without overwriting the existing user prefab edits.

## Task: 2026-08-01 Monster Prefab Binding Migration

### Task title

Replace hardcoded playable-monster prefab selection with `MonsterPrefabBinding[]`.

### Goals

- Remove the five monster ID constants and five individual monster prefab fields from `UnitSpawnManager`.
- Resolve playable monster prefabs through serialized ID-to-prefab bindings.
- Preserve the existing Ariel, Eve, Rin, Sein, and Vega prefab references.

### Constraints

- Preserve `ResolveMonsterPrefab(string)` callers and spawn behavior.
- Keep monster data ownership in `MonsterDefinition`; keep scene prefab references in `UnitSpawnManager`.
- Do not modify unrelated user changes.

### Role Owner

Code Builder

### Status

Implemented and verified.

### Next Actions

- User verifies playable monster spawning in Play Mode.

### Evidence

- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs:25` now owns `MonsterPrefabBinding[]`.
- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs:435` resolves by binding ID.
- Unity component inspection reported five bindings: `ariel`, `eve`, `rin`, `sein`, `vega`, each with a prefab path.
- Unity script validation returned 0 warnings and 0 errors.
- Core and Editor builds completed with 0 errors; only the existing two assembly-reference warnings remain.

### History

- 2026-08-01: Code Builder replaced hardcoded playable-monster prefab routing with serialized bindings and preserved the existing prefab assets.
