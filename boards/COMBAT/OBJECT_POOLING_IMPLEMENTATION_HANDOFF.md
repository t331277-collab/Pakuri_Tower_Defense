# Runtime Object Pooling Implementation Handoff

## Task title

`DamageNumberPopup -> EffectManager skill effects -> enemy units` 순서로 런타임 `GameObject` 풀링을 도입한다.

## Goals

- 공용 풀 구현을 한 클래스에 둔다.
- 피해 숫자로 가장 작은 재사용 경로를 먼저 검증한다.
- 검증된 풀을 `EffectManager`가 생성하는 스킬 효과에 연결한다.
- 마지막으로 적 프리팹을 재사용하되 모델·UI·Collider·Animator·보스 배율을 완전히 초기화한다.
- 각 단계가 독립적으로 빌드·검증된 뒤 다음 단계로 진행되게 한다.

## Constraints

- 현재 저장소에는 Object Pool 구현이 없다. `ObjectPool`, `IObjectPool`, `UnityEngine.Pool` 검색 결과가 비어 있다.
- `DamageNumberPopup`, `EffectManager`, `UnitSpawnManager`의 현행 사용자 동작은 바꾸지 않는다.
- 풀은 `GameObject` 저장·회수만 담당한다. 피해 숫자, 효과 Actor, 적 유닛의 상태 초기화는 기존 소유자가 담당한다.
- 전역 Singleton, `IPoolable` 인터페이스, 팩토리 계층, 사전 생성 수량 설정, CSV 필드는 추가하지 않는다.
- 플레이어 몬스터는 현재 패배 후 `MonsterActor.Revive`로 재사용되므로 대상에서 제외한다.
- 임시 소환수는 동시 한 개이고 현재 별도 생명주기를 가지므로 이번 범위에서 제외한다.
- `EffectVisualBuilder.CreateBranchDamageLine`의 `Material`은 이번 범위에서 풀링하지 않는다. 현재 코드처럼 짧은 수명 뒤 파괴한다.
- Unity Play Mode 게임플레이 검증은 사용자 소유다.

## Role Owner

Designer for this handoff. Code Builder for implementation after explicit role assignment.

## Status

Phase 1~3 C# 구현과 Phase 2 runtime visual hitbox 회귀 수정 완료. 로컬 빌드·Unity EditMode 검증 통과, 사용자 Play Mode 재확인 대기. GitHub 커밋은 하지 않았다.

## Current inspected evidence

- `DamageNumberPopup.SpawnPopup`은 표시마다 `damageText.gameObject`를 `Instantiate`하고 만료 또는 최대 개수 초과 시 `Destroy`한다.
- `DamageNumberPopup`의 활성 상한 기본값은 `12`다.
- `EffectManager.CreateObject`는 런타임 비주얼을 `new GameObject`, 프리팹을 `Instantiate`, 빈 실행 Actor를 `new GameObject`로 만든다.
- `EffectManager.RemoveEffect`와 `ClearEffects`는 효과를 `Destroy`한다.
- 구현 전 `EffectVisualBuilder.Configure`는 `SpriteRenderer`, `Animator`, `BoxCollider2D`를 매번 `AddComponent`했다.
- Projectile, Line, Single Executor는 빈 실행 오브젝트에 자신의 Executor를 매번 `AddComponent`한다.
- `ProjectileSkillActor.Initialize`는 대부분의 런타임 필드를 초기화하지만 도착 지연에서 비활성화한 Collider를 다시 켜지 않는다.
- `SingleSkillExecutor.pendingSchedules`는 현재 최초 생성 기본값에 의존한다. 재사용 시작 시 `0`으로 초기화해야 한다.
- `UnitSpawnManager.SpawnEnemyUnit`은 적 프리팹을 `Instantiate`하고, `CombatUnitEntry.HandleDefeat`은 Enemy Actor 오브젝트를 `0.95`초 뒤 `Destroy`한다.
- `UnitSpawnManager.ApplyBossVisualScale`은 루트와 직계 UI 자식 Transform을 변경한다. 보스 인스턴스를 일반 적으로 재사용하기 전에 원본 Transform을 복원해야 한다.
- Enemy Actor 프리팹은 16개다. 전부 `BoxCollider2D` 1개를 가지며 Stage2 프리팹 8개는 `Animator` 1개를 가진다. `Rigidbody2D`는 없다.
- `EnemyActor.Initialize`는 새 `EnemyCombatState`와 새 `UnitHpBar`를 연결하므로 재사용 모델 바인딩 경계로 쓸 수 있다.
- 구현 전 관련 전용 풀링 테스트는 없었다. 현재는 공용 풀과 runtime visual hitbox 재사용 검사가 있다.

## Core design

### One reusable class

신규 예정 파일:

`Pakuri/Assets/Scripts/Units/Runtime/RuntimeObjectPool.cs`

```text
RuntimeObjectPool<TKey>
├─ Get(TKey key, Func<GameObject> create)
├─ Release(GameObject instance)
└─ Clear()
```

- 내부 저장은 현재 프로젝트가 이미 쓰는 표준 `Dictionary<TKey, Stack<GameObject>>` 조합을 사용한다. 별도 패키지나 확인되지 않은 Unity Pool API에 의존하지 않는다.
- 각 owner가 자신의 풀 인스턴스를 가진다. 전역 풀은 만들지 않는다.
- `RuntimeObjectPool`은 활성 인스턴스와 생성 key의 연결만 기억한다.
- `Get`은 없을 때만 `create`를 호출하고, 재사용 시 활성화한다.
- `Release`는 Coroutine을 중단하고 비활성화한 뒤 원래 key의 풀에 반환한다.
- `Clear`는 owner 파괴 시 보관 중인 인스턴스를 정리한다. Stage 전환에서는 호출하지 않는다.
- 세션 최고 동시 생성량을 보관한다. 메모리 측정에서 문제가 확인될 때만 key별 상한을 추가한다.

### Responsibility boundary

| Owner | Pool key | Owner reset responsibility |
|---|---|---|
| `DamageNumberPopup` | 원본 `damageText.gameObject` | Text, 색, 위치, 경과 시간, 활성 목록 |
| `EffectManager` | prefab/visual/object name/Hitbox 설정 조합 | 부모, Transform, Renderer, Animator, Collider, 추적 Dictionary |
| `UnitSpawnManager` | 적 prefab 참조 | 새 모델, 이름, 위치, 원본 Scale, 보스 Scale, Collider, Animator, UI |

`RuntimeObjectPool`은 위 상태를 알지 않는다.

## Implementation order

### Phase 1 — DamageNumberPopup

대상:

- 신규 `Pakuri/Assets/Scripts/Units/Runtime/RuntimeObjectPool.cs`
- 수정 `Pakuri/Assets/Scripts/Units/Display/DamageNumberPopup.cs`
- 기존 테스트 파일 `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`

작업:

1. `RuntimeObjectPool<TKey>` 최소 API를 추가한다.
2. `DamageNumberPopup`이 자신의 원본 TextMesh를 key로 쓰는 로컬 풀을 소유한다.
3. 최초 요청만 현재 방식으로 원본을 복제하고, 복제된 `DamageNumberPopup` 컴포넌트는 비활성화 후 한 번 제거한다.
4. 만료와 `maxActivePopups` 초과 시 `Destroy` 대신 `Release`한다.
5. 재사용 시 부모, `localPosition`, Text, Color, Alpha, 표시 시간 값을 다시 쓴다.
6. Unit 오브젝트가 비활성화될 때 활성 피해 숫자를 전부 반환하는 정리 경계를 둔다.

수용 기준:

- 동시에 표시되는 피해 숫자는 기존처럼 `maxActivePopups`를 넘지 않는다.
- 치명타 빨강, 일반 색, 수직 Stack, 상승, Fade 시간이 동일하다.
- 한 번 반환된 인스턴스가 다음 표시에서 같은 Instance ID로 재사용된다.
- 만료와 상한 초과 경로에서 Popup `Destroy` 호출이 사라진다.
- 비활성화 후 재사용한 유닛에 이전 피해 숫자나 Alpha가 남지 않는다.

Phase gate:

- Phase 1 정적 검사, EditMode 검사, Runtime/Editor 빌드가 통과하기 전 Phase 2를 시작하지 않는다.

### Phase 2 — EffectManager skill effects

대상:

- 수정 `Pakuri/Assets/Scripts/Combat/Effects/EffectManager.cs`
- 수정 `Pakuri/Assets/Scripts/Combat/Effects/EffectVisualBuilder.cs`
- 수정 `ProjectileSkillExecutor.cs`, `LineSkillExecutor.cs`, `SingleSkillExecutor.cs`
- 필요 최소 수정 `ProjectileSkillActor.cs`
- 기존 테스트 파일 `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`

작업:

1. `EffectManager`가 owner-local `RuntimeObjectPool<EffectPoolKey>`를 소유한다.
2. `EffectPoolKey`는 `Prefab`, `RuntimeSkillVisualSpec`, `ObjectName`, `IncludeHitbox`, `HitboxIsTrigger`, `CreateEmptyActor`를 포함하는 `EffectManager` 내부 값 형식으로 둔다.
3. `CreateObject`의 세 경로를 모두 `pool.Get`으로 통합한다.
4. `RemoveEffect`는 Status/target 추적을 제거하고 `runtimeSkillRoot`로 부모를 복원한 뒤 `pool.Release`한다.
5. 인스턴스로 제거할 때도 그 인스턴스를 값으로 가진 `statusEffectVisuals` 항목을 제거한다. 풀 인스턴스가 이전 Status에 남지 않게 한다.
6. `ClearEffects`는 현재 활성 효과만 전부 반환한다. 비활성 풀 보관분은 Stage 간 유지한다.
7. `EffectVisualBuilder.Configure`를 `GetComponent ?? AddComponent` 방식으로 바꾸고 Renderer, Animator, Collider 값을 매번 덮어쓴다.
8. 재사용 시 root Transform, Collider enabled, Animator state를 초기화한다. Area/Line 배율은 초기 Scale 복원 뒤 다시 적용한다.
9. 빈 Projectile/Line/Single Executor는 `GetComponent ?? AddComponent`로 재사용한다.
10. `SingleSkillExecutor.Initialize` 시작 시 `pendingSchedules = 0`으로 되돌린다.
11. `ProjectileSkillActor.Initialize`에서 모든 Hitbox Collider를 다시 활성화한다.
12. 비활성 반환 시 해당 오브젝트의 Coroutine을 중단한다.

수용 기준:

- `EffectManager`가 만든 runtime visual, prefab effect, empty execution Actor가 같은 key에서 재사용된다.
- 다른 visual/prefab/가족 key끼리는 동시에 같은 인스턴스를 공유하지 않는다.
- 재사용 때 `SpriteRenderer`, `Animator`, `BoxCollider2D`, Executor/Actor 컴포넌트가 중복 추가되지 않는다.
- Projectile 도착 지연 뒤 재사용해도 Collider가 활성 상태다.
- Single 반복 예약 횟수가 이전 실행에서 누적되지 않는다.
- target-attached effect와 persistent status effect가 반환 후 이전 target/status Dictionary에 남지 않는다.
- `ClearEffects` 뒤 활성 스킬 효과가 0이고, 다음 Stage에서 보관 인스턴스를 다시 사용한다.
- 기존 Projectile, Line, Single, Zone, Buff, Status 수명과 Trigger 결과가 동일하다.

Phase gate:

- 대표 가족별 정적/Unity 검사와 빌드가 통과하기 전 Phase 3를 시작하지 않는다.

### Phase 3 — Enemy units only

대상:

- 수정 `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs`
- 수정 `Pakuri/Assets/Scripts/Units/Runtime/CombatUnitRegistry.cs`
- Phase 1의 `DamageNumberPopup` 정리 API 재사용
- Phase 2의 `EffectManager` target-attached 정리 경계 재사용
- 기존 테스트 파일 `Pakuri/Assets/Tests/Editor/SkillCatalogRuntimeTests.cs`

작업:

1. `UnitSpawnManager`가 enemy prefab을 key로 쓰는 owner-local 풀을 소유한다.
2. `SpawnEnemyUnit`의 `Instantiate`를 `pool.Get(prefab, Instantiate)`로 바꾼다. 플레이어/소환수 생성 경로는 건드리지 않는다.
3. 최초 생성 직후 root와 직계 자식의 원본 local position/rotation/scale을 인스턴스별로 기록한다.
4. 매 Spawn 전에 기록한 원본 Transform을 복원하고, 그 다음에만 `ApplyBossVisualScale(1.6)`을 한 번 적용한다.
5. 새 이름, parent, world position/rotation을 설정하고 모든 Collider를 활성화한다.
6. Animator가 있는 Stage2 적은 재사용 시 `Rebind`와 첫 프레임 갱신으로 이전 상태를 지운다.
7. `EnemyActor.Initialize(newModel)`로 모델과 `UnitHpBar`를 다시 연결하고 Registry에 새 Entry를 등록한다.
8. 패배 시 Registry에서는 현재처럼 즉시 제거한다. 기존 `0.95`초 표시 시간 뒤 `Destroy` 대신 반환한다.
9. 반환 직전 피해 숫자와 적 아래 target-attached effect를 전부 회수한다.
10. `CombatUnitEntry.HandleDefeat`는 Enemy 오브젝트 파괴를 소유하지 않게 하고, 실제 지연 반환은 `UnitSpawnManager`가 소유한다.

수용 기준:

- 같은 prefab 적이 패배 후 다시 생성될 때 반환된 Instance ID를 재사용한다.
- Registry에는 새 `EnemyCombatState`만 있고 이전 모델 참조가 없다.
- 보스 인스턴스를 일반 적으로 재사용해도 root Sprite/Hitbox와 직계 UI가 prefab 원본 크기·위치로 돌아온다.
- 일반 적을 보스로 재사용하면 Scale `1.6`이 정확히 한 번만 적용된다.
- Collider가 활성화되고 Stage2 Animator가 이전 상태를 이어받지 않는다.
- 이름, HP, Shield, Status Text, 피해 Popup이 이전 적 값을 표시하지 않는다.
- 패배 뒤 기존 `0.95`초 화면 잔류 시간은 유지된다.
- 기존 authored boss/run-assigned boss 체력 배율 구분은 바뀌지 않는다.
- 플레이어 몬스터와 임시 소환수의 현행 생성·부활·파괴 경로는 바뀌지 않는다.

## Compatibility risks

- Effect key가 너무 넓으면 다른 Actor 컴포넌트가 섞인다. 위 key 조합을 축소하지 않는다.
- 비활성화만 하고 Coroutine을 남기면 재사용 뒤 이전 예약이 실행될 수 있다. 반환 시 Coroutine 중단이 필수다.
- 보스 Scale을 현재 값에서 역산하면 누적 오차와 이중 적용 위험이 있다. 최초 생성 원본 Transform을 저장해 복원한다.
- 죽은 적의 Status visual은 모델이 Registry에서 빠져 더 이상 Tick되지 않는다. 적 반환 전에 target-attached 효과를 명시적으로 반환한다.
- `EffectManager.ClearEffects`가 비활성 풀 보관분까지 다시 반환하면 이중 Release가 된다. 활성 인스턴스만 처리한다.

## Verification expected from Code Builder

- 각 Phase 전후 `Instantiate`, `Destroy`, `new GameObject`, `AddComponent` 호출 위치 비교.
- 풀 재사용 Instance ID, key 분리, 중복 Release 방지 EditMode 검사.
- 피해 숫자 상한/Fade/치명타 색과 effect reset 검사.
- 적 normal -> boss -> normal 재사용 Transform/Collider/Animator/Registry 검사.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` 오류 0개.
- Unity script refresh/compile와 Console error 0개 확인.
- `git diff --check` 통과.
- 사용자가 Play Mode에서 피해 숫자 연타, 각 스킬 가족, Status visual, Stage 전환, 보스/일반 적 교차 재사용을 확인한다.

## Related boards

- `boards/RUN/RUN_BLACKBOARD.md`
- `boards/COMBAT/STATUS_EFFECT_BLACKBOARD.md`는 Status 동작 자체가 변경될 때만 갱신한다. 현재 설계 단계에서는 읽거나 수정하지 않았다.

## Next Actions

- 사용자가 Play Mode에서 `vega-a`와 다른 runtime visual 투사체를 실행해 MissingComponentException이 재발하지 않는지 확인한다.

## Evidence

- `Pakuri/Assets/Scripts/Units/Display/DamageNumberPopup.cs`
- `Pakuri/Assets/Scripts/Units/Display/UnitHpBar.cs`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectCreate.cs`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectManager.cs`
- `Pakuri/Assets/Scripts/Combat/Effects/EffectVisualBuilder.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Projectile/ProjectileSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Line/LineSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Line/LineSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Zone/ZoneSkillExecutor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Zone/ZoneSkillActor.cs`
- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Buff/BuffSkillActor.cs`
- `Pakuri/Assets/Scripts/GameFlow/Spawn/UnitSpawnManager.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/CombatUnitRegistry.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/Actor/EnemyActor.cs`
- `Pakuri/Assets/Scripts/Units/Runtime/Actor/MonsterActor.cs`
- `Pakuri/ProjectSettings/ProjectVersion.txt`: Unity `6000.3.14f1`.
- 구현 전 Source search: `NO_POOL_IMPLEMENTATION_FOUND`.
- Prefab static inspection: Enemy Actor prefab 16개, 전부 BoxCollider2D 1개, Stage2 8개만 Animator 1개, Rigidbody2D 0개.
- `Pakuri/Assets/Scripts/Units/Runtime/RuntimeObjectPool.cs`가 `Dictionary<TKey, Stack<GameObject>>` 기반 owner-local 풀과 active key 추적을 구현한다.
- `DamageNumberPopup`은 표시 만료·상한 초과 시 `Release`하고 `OnDisable`에서 활성 Popup을 정리한다.
- `EffectManager`는 prefab/visual/actor key별 풀, Transform·Animator·Collider reset, status/target 추적 정리와 target 하위 효과 일괄 회수를 구현한다.
- `EffectVisualBuilder`와 Projectile/Line/Single 실행 경로는 재사용 시 기존 컴포넌트를 우선 사용하고, 새 컴포넌트는 최초 생성 때만 추가한다.
- `UnitSpawnManager`는 적 prefab key 풀, 인스턴스별 원본 Transform 복원, 보스 1.6배 적용, Animator/Collider reset, 지연 회수를 구현한다. 플레이어·소환수 생성 경로는 유지했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`: 오류 0개, 기존 System.Net.Http/System.IO.Compression 참조 충돌 경고 2개.
- Unity `validate_script` 대상 Phase 1~3 변경 스크립트 9개: 오류 없음.
- Unity EditMode 전체 검사: 37개 중 37개 통과, 실패 0개. 풀 회수·재사용 검사도 통과.
- `git diff --check`: 공백 오류 없음. 현재 작업 트리에는 구현 파일과 테스트 파일만 변경·추가되어 있으며 커밋은 수행하지 않았다.
- Play Mode 스택은 `EffectVisualBuilder.ConfigureHitbox:215`의 `collider.enabled = true`에서 `MissingComponentException`을 기록했다.
- 현재 `EffectVisualBuilder.cs:214`만 저장소 전체에서 `GetComponent<UnityEngine.Object>() ?? AddComponent` 패턴을 사용한다. 같은 파일의 `ConfigureLineHitbox`는 명시적 `if (collider == null)` 패턴을 사용한다.
- Unity 6 `UnityEngine.Object` 문서는 detached object에 `??`를 지원하지 않는다고 명시한다. 따라서 `GetComponent` 결과가 Unity null로 판정되어도 C# `??`가 `AddComponent`를 실행하지 않고, 다음 `enabled` 접근이 실패할 수 있다.
- 수정 전 Editor 테스트 검색 결과 `EffectVisualBuilder`, `ConfigureHitbox`, `CreateEffect`를 실행하는 검사는 없고 공용 풀 자체 검사만 존재했다.
- `EffectVisualBuilder.ConfigureHitbox`는 Unity Object `??` 대신 명시적 `if (collider == null)` 검사를 사용한다.
- `EffectManager.CreateObject`의 runtime visual 선행 Configure를 제거해 `PrepareObject`에서 한 번만 구성한다.
- 신규 `RuntimeVisualHitboxIsCreatedAndReused` 검사는 `EffectManager` 생성→반환→동일 인스턴스 재사용과 BoxCollider2D 1개·활성 상태를 확인하고 1/1 통과했다.
- Unity EditMode 전체 검사 38/38 통과, `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` 오류 0개·기존 참조 경고 2개, 변경 스크립트 Unity 진단 오류 0개, `git diff --check` 통과.

## History

- 2026-08-08: Designer가 현재 생성·제거·초기화 경로를 다시 검사하고 3단계 풀링 구현 순서와 단계별 gate를 기록했다.
- 2026-08-08: Code Builder가 Phase 1 DamageNumberPopup, Phase 2 EffectManager 스킬 이펙트, Phase 3 적 유닛 순서로 구현하고 각 단계 gate를 통과시켰다. Play Mode 검증은 사용자에게 남겼다.
- 2026-08-08: 사용자 Play Mode에서 `Projectile_vega-a` hitbox 생성 시 `MissingComponentException`이 확인됐다. Designer가 Unity Object `??` 사용과 runtime visual 이중 Configure를 원인·동반 문제로 확정하고 Code Builder 수정 항목을 기록했다.
- 2026-08-08: Code Builder가 명시적 Unity null 검사, runtime visual 단일 Configure, 생성·재사용 회귀 검사를 반영하고 전체 EditMode 38개를 통과시켰다.
