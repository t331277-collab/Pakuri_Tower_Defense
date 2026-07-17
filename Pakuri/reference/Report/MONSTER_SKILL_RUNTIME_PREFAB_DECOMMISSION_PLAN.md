# 몬스터 스킬 런타임 프리팹 의존 제거 계획 및 실행 기록

## 작업 목표

- `Assets/Prefab/Skill`을 기본 스킬 비주얼 생성 경로와 fallback 경로로 사용하지 않는다.
- 스프라이트, AnimatorController, 오브젝트 크기, 정렬 순서, 콜라이더 크기는 런타임 CSV와 스킬 그래프 노드 값으로 조립한다.
- 런타임 전환이 완료된 스킬 프리팹은 원본 `.prefab`과 `.meta`를 함께 삭제한다.
- 아직 런타임 전환 대상이 아닌 Rin-D와 Rin-E만 프리팹 예외로 유지한다.

## 절대 제약

- 역할: Code Builder
- Blueprint는 읽거나 변경하지 않는다. 이 작업은 기존 스킬 동작의 프리팹 의존 제거 리팩터링이다.
- CSV에 새 오프셋 열을 추가하지 않는다.
- 런타임 오브젝트 위치는 생성 지점 기준 `(0, 0)`이며 콜라이더 오프셋도 `(0, 0)`으로 고정한다.
- `Assets/Prefab/Skill/Rin/Rin_D.prefab`과 `Assets/Prefab/Skill/Rin/Rin_E.prefab`은 이번 삭제 대상에서 제외한다.
- MSW-MCP는 사용하지 않는다. Unity 편집기 검증은 Unity-MCP만 사용한다.

## 전환 구조

1. 기본/트리거 스킬 비주얼은 각 runtime CSV의 `runtime_visual_*`, `runtime_hitbox_size_*` 값을 사용한다.
2. 강화·마스터 효과 비주얼은 `skill_graph_nodes_*.csv`의 `RuntimeEffectVisual` 노드를 사용한다.
3. `RuntimeSkillVisualFactory`가 SpriteRenderer, Animator, 크기, 정렬 순서, BoxCollider2D를 런타임 생성한다.
4. `EffectManager.monsterSkillEffects`의 프리팹 매핑은 Rin-D 하나만 유지한다.
5. `skill_effect_prefab_path`는 Rin-E 하나만 유지한다.

## 구현 중 발견한 런타임 공백과 보완

기본 투사체는 런타임 비주얼로 생성되었지만, 분기 탄환과 지연 후속 탄환은 `GameObject` 프리팹이 없으면 생성되지 않는 경로가 남아 있었다.

- `ProjectileSkillExecutor`가 분기/후속 탄환에도 `RuntimeSkillVisualSpec`을 전달하고 `RuntimeSkillVisualFactory.Create`로 생성하도록 변경했다.
- `ProjectileBranchHitSpec`이 런타임 비주얼을 보관하고 복제하도록 변경했다.
- `InGameProjectileActor`가 분기 탄환 생성 시 런타임 비주얼을 우선 사용하도록 변경했다.

이 보완은 Eve-A 계열 분기 탄환과 Vega-A Master 1 후속 탄환이 프리팹 삭제 뒤에도 생성되기 위한 공용 경로다.

## 데이터와 씬 정리

- Sein 기본 C/D/E의 `skill_effect_prefab_path`를 제거하고 기존 런타임 비주얼 값을 유지했다.
- Sein A Master 2, C Master 1/2, D Master 2, E Master 2의 prefab EffectVisual을 제거하고 런타임 비주얼만 유지했다.
- Ariel C Master 1/2, C 버프, E 방어막 효과를 `RuntimeEffectVisual`로 전환했다.
- Rin A Master 2의 중복 prefab EffectVisual을 제거했다.
- Rin D Master 1과 Rin F 후속 트리거의 prefab 경로를 제거하고 런타임 비주얼을 유지했다.
- Rin D Master 1의 런타임 콜라이더 오프셋을 `(0, 0)`으로 정규화했다.
- `NewRunScene.unity`의 `monsterSkillEffects`는 Rin-D 매핑만 유지했다.
- 구형 `Assets/Legacy/Data/GameData/Monsters/eve.asset`, `rin.asset`의 삭제 대상 프리팹 직렬화 참조를 비웠다.

## 삭제 범위

- Ariel: 7개 프리팹
- Eve: 6개 프리팹
- Rin: A, B, C, D Master 1, F의 5개 프리팹
- Sein: 9개 프리팹
- Vega: 6개 프리팹
- 합계: 런타임 전환 완료 프리팹 33개와 대응 `.meta`, 비어지는 몬스터 디렉터리 `.meta`

유지 대상:

- `Pakuri/Assets/Prefab/Skill/Rin/Rin_D.prefab`
- `Pakuri/Assets/Prefab/Skill/Rin/Rin_E.prefab`

## 완료 조건

- runtime monster skill CSV의 `Assets/Prefab/Skill` 참조가 Rin-E 하나뿐이다.
- `NewRunScene.unity`의 몬스터 스킬 프리팹 매핑이 Rin-D 하나뿐이다.
- 삭제된 프리팹 GUID를 참조하는 활성 Asset/Scene/CSV가 없다.
- runtime CSV의 비어 있지 않은 `runtime_hitbox_offset_x/y` 값은 모두 `0`이다.
- Runtime/Editor C# 빌드가 통과한다.
- Unity-MCP 자산 동기화 뒤 생성 카탈로그에 Rin-D/Rin-E 외 Skill prefab 경로가 없다.
- Play Mode에서 각 몬스터의 기본 스킬, 강화, 마스터 효과를 확인한다. Play Mode 최종 확인은 사용자 담당이다.

## 상태

- 구현 및 파일 삭제: 완료
- 정적 참조/CSV/빌드/Unity-MCP 검증: 완료
- 사용자 Play Mode 검증: 대기

## 검증 결과

- CSV 구조 검사: runtime monster skill CSV 33개 모두 행별 열 수 일치.
- 런타임 경로 검사: `runtime_visual_sprite_path`, `runtime_visual_animator_controller_path`, 그래프 `RuntimeEffectVisual`의 Sprite/Controller 경로가 모두 실제 파일과 일치.
- 오프셋 검사: 비어 있지 않은 `runtime_hitbox_offset_x/y` 값은 모두 `0`.
- 삭제 GUID 검사: 삭제한 33개 prefab meta GUID의 현재 `Pakuri/Assets` 참조 0건.
- 활성 runtime CSV prefab 경로: `Assets/Prefab/Skill/Rin/Rin_E.prefab` 1건.
- 씬 몬스터 prefab 매핑: `rin-d` 1건.
- Unity-MCP 강제 refresh/compile 뒤 생성 카탈로그 prefab 경로: `Assets/Prefab/Skill/Rin/Rin_E.prefab` 1건.
- `Assembly-CSharp.csproj`: 오류 0, 기존 Unity/MCP 참조 충돌 경고 2.
- `Assembly-CSharp-Editor.csproj`: 오류 0, 기존 Unity/MCP 참조 충돌 경고 2.

## 근거 파일

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs`
- `Pakuri/Assets/CSVdata/authoring/monster/skills/`
- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`
- `Pakuri/Assets/Prefab/Skill/`
