## Archived History

- Non-July task blocks from `boards\MON\VEGA_MONSTER.md` were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-07-18.md` on 2026-07-18.

## Archive Note

- Older task blocks were moved to `boards/ARCHIVE/MON_DETAIL_ARCHIVE_2026-05-12.md` on 2026-05-12.
- This file keeps only task blocks dated 2026-05-10 based on the date in each `## Task:` / `## Recent Task:` heading.
- Source file: `boards/MON/VEGA_MONSTER.md`.

# VEGA_MONSTER

## Scope

Vega dedicated monster, skill, and runtime persistent-state file.

At the start of new work, use this active Vega file. Common monster history is archived at `boards/ARCHIVE/MON_BLACKBOARD_ARCHIVE_2026-05-14.md`; consult `boards/MON/EVE_MONSTER.md` only when a concrete implementation example is needed.

## Status

Vega active skills A-E are implemented and locally validated.
Vega passive skills F-J are now implemented on shared runtime/CSV paths and passed local build plus Unity CSV validation/sync on 2026-05-31.
- 2026-05-26 cleanup: non-core task details older than 2026-05-24 were moved to `boards/ARCHIVE/BOARD_CLEANUP_ARCHIVE_2026-05-26.md`.

## Task: 2026-07-13 Vega Positional Graph Migration

### Task title

Vega A-J Positional Skill Graph Migration

### Goals

- Vega A-J Choice/Effect/direct node 행동을 positional graph로 이전한다.
- Trigger 15개의 event envelope를 유지하면서 Trigger-owned Effect를 graph reference로 전환한다.
- 사용자 승인값인 A trait 4 마지막 탄환 `1.5`와 E trait 4 `20`스택 조건을 적용한다.

### Constraints

- Role Owner는 Code Builder다.
- Blueprint/reference markdown은 읽지 않는다.
- 근거는 `Pakuri/Assets/CSVdata/authoring/monster/skills/`와 실제 runtime code다.
- 새 graph 파일/graph 열/offset은 추가하지 않는다. B master 1에 필요한 line Trigger graph 참조 4열만 기존 Trigger schema와 같은 형식으로 추가한다.
- prefab, scene, object/collider offset은 변경하지 않는다.

### Role Owner

- Code Builder

### Status

- **Implemented and automated validation passed / user Play Mode verification remains**

### Next Actions

1. 사용자가 Play Mode에서 A trait 4 마지막 탄환 배율과 E trait 4 20스택 임계값을 확인한다.
2. A-J Choice 조합, Trigger event/internal cooldown, H source gate, I Area filter의 동작 parity를 확인한다.
3. 확인 전 제거된 legacy Effect/direct node를 다시 추가하지 않는다.

### Evidence

- 제안서: `boards/MON/VEGA_NODE_MIGRATION_PROPOSAL.md`.
- 전환 후 집계: positional graph 154행/58 graph, legacy Effect 0, Trigger 15, direct node 0, direct param 0, 중복 Choice wide 행동 값 0.
- graph 구성: `Plan` 45행/35 graph, `Effect` 109행/23 graph. Trigger graph reference는 11행이다.
- A trait 4 CSV는 `BurstDamageRule`, `projectile_index=0`, `multiplier=1.5`다.
- E trait 4 CSV는 `TargetStatusCritBonus`, `status_id=name-mark`, `crit_chance_bonus=0.35`, `min_stacks=20`이다.
- node-backed Choice 특수 handler 9개는 mapper compatibility 경로와 Snapshot 필드 적용 경로로 연결했다.
- Unity-MCP `Pakuri/Validate CSV Source Data`는 5 monsters/8+8 enemies catalog를 로드했고 `Pakuri/InGame/Validate Skill Data`는 경고 0으로 통과했다.
- runtime/Editor C# build는 오류 0이다. 변경 runtime CSV 30개 shape 검사와 `git diff --check`도 오류 0이다.
- EditMode test case는 저장소에서 0개 발견됐다. 생성된 `TestResults.xml`은 `total=0`, `failed=0`, `result=Passed`다.

### History

- 2026-07-13: Designer가 Blueprint를 제외하고 active runtime CSV와 runtime code만 검사했다.
- 2026-07-13: Vega positional graph 전환안을 작성하고 MON/DATA persistent state에 연결했다.
- 2026-07-13: 사용자가 A trait 4의 마지막 탄환 `1.5`와 E trait 4의 설명값 `20`스택을 승인했다.
- 2026-07-13: Code Builder가 graph 154행, 공용 node/composer 연결, Trigger graph 참조, legacy 제거를 구현하고 Unity 자동 검증을 통과했다.

## Task: 2026-07-14 Vega Skill Runtime Visual Migration Design

### Task title

Design Vega's prefab-to-runtime skill visual migration.

### Goals

- Move Vega A-E, A master 2, and B master 1 away from active skill-prefab instantiation.
- Preserve gameplay-authoritative colliders: Vega A projectile, Vega D marked-target deployments, and shared LineAttack runtime box queries.
- Keep A master 2 and E on their existing mathematical target-resolution paths.

### Constraints

- Role Owner is Designer.
- Implementation is complete; prefab files remain on disk.
- Skill Blueprints were excluded because this is a presentation/runtime assembly refactor.

### Role Owner

Designer

### Status

Implemented and statically validated; user Play Mode parity remains.

### Next Actions

- Verify each converted path and B collider boundaries in Play Mode.

### Evidence

- Unity-MCP found six single-root Vega skill prefabs: A, A master 2, B, C, D, and E.
- Active runtime CSV/graph references occur in the five A-E base rows, B master 1 line Trigger, and A master 2 Effect graph.
- `InGameLineAttackActor.ApplyLineTick` now uses a centered `length × width` runtime `Physics2D.OverlapBox` against target colliders.
- `InGameSkillDefinitionMapper` forces D's status-filtered deployments onto `UsePrefabHitbox`, making D's `(2.64,2.63)` collider gameplay authority.
- Detailed values and the ordered migration are recorded in `boards/MON/VEGA_SKILL_RUNTIME_VISUAL_MIGRATION_PLAN.md`.

### History

- 2026-07-14: User requested a Vega runtime visual migration plan following the Sein plan format.
- 2026-07-14: Code Builder removed active Vega prefab references, implemented runtime visuals and shared LineAttack collider queries, and passed Unity-MCP validation.

## Task: 2026-07-14 Vega Runtime Visual And Collider Migration

### Task title

Move Vega skill presentation to runtime assembly and Vega B detection to runtime collider queries.

### Goals

- Runtime-assemble Vega A-E, A master 2, and B master 1 without active Vega skill-prefab references.
- Use target colliders for Vega B and B master 1 line detection.
- Keep every runtime collider/object offset `(0,0)` and add no CSV columns.

### Constraints

- Role Owner is Code Builder.
- Original prefabs remain evidence assets and were not deleted or edited.
- Skill Blueprint was excluded; Unity-MCP was the only MCP used.

### Role Owner

Code Builder

### Status

Implemented; build and Unity-MCP static validation passed.

### Next Actions

- User verifies Vega A-E, A master 2, and B master 1 in Play Mode.
- Pay special attention to B/B master 1 edge overlap and D marked-target deployment coverage.

### Evidence

- Active runtime skill CSV/graph search has zero `Assets/Prefab/Skill/Vega` matches.
- Vega B base runtime visual lives in `skills_line_attack.csv`; B master 1 resolves it via `SkillTriggerRuntime` without Trigger CSV columns.
- `InGameLineAttackActor` queries a rotated `length × width` box and compares overlap results with target hitbox colliders.
- Vega A and D runtime hitboxes are centered; missing offset columns materialize as `(0,0)` through `BuildRuntimeVisual` defaults.
- Runtime catalog sync and both Unity validation menus completed with no skill warning/error.

### History

- 2026-07-14: Code Builder completed migration and recorded user-owned Play Mode verification.
