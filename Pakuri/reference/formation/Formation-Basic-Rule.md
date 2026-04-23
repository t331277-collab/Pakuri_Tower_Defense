# 포메이션 기본 규칙

> 기준: Dungeon Squad Miraheze Wiki의 Formation 문서

## 원작 포메이션 목록
- Basic Formation
- Charge Formation
- Siege Formation
- Salvo Formation
- Defense Formation
- Element Formation
- Black and White Formation
- Megalith Formation

## 기본 구조
- 포메이션은 1~5번 위치 효과로 구성된다.
- 일부 위치 효과는 해당 위치에 배치된 몬스터만 강화한다.
- 일부 위치 효과는 조건을 만족하면 모든 몬스터 또는 모든 영웅에게 적용된다.
- 원작에서 빨간색으로 표시된 페널티 또는 적 강화 효과는 프로토타입 포메이션 문서에서 제외한다.
- 원작은 포메이션마다 5개 위치를 사용하지만, 현재 프로젝트에서는 15개 공용 배치 후보 중 포메이션별로 5개 슬롯을 선택해 사용한다.

## 현재 문서화 범위
- [[dense-formation]] : Charge Formation 기준
- [[defense-formation]] : Defense Formation 기준
- [[element-formation]] : Element Formation 기준
- [[dark-formation]] : Black and White Formation 기준
- [[siege-formation]] : Siege Formation 기준
- [[salvo-formation]] : Salvo Formation 기준
- [[megalith-formation]] : Megalith Formation 기준

## 원작 참고 이미지
![Basic Formation](basic-formation.png)

## 배치 좌표 기준
- 현재 프로젝트의 좌표 배치는 [`combat-scene-layout.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2FScene%2Fcombat-scene-layout) 참고
- 원작 위키는 이미지 기준 포메이션 위치를 제공하므로, 실제 좌표는 프로젝트 전투 화면 기준으로 별도 해석한다.
- 넥서스 기준 위치는 `(2,8)`이다.
- 아군 타워 배치 가능 영역은 `(4~10, 3~15)` 안으로 제한한다.
- 적은 우측에서 진입하므로, `x`값이 클수록 전방 슬롯으로 본다.

## 공용 타워 배치 후보 15개
![[Pasted image 20260419194543.png]]

| 슬롯 ID | 좌표 | 역할 기준 |
|---|---:|---|
| rear-top | `(4,12)` | 넥서스 근처 후열 상단 |
| rear-upper | `(4,10)` | 넥서스 근처 후열 중상단 |
| rear-center | `(4,8)` | 넥서스 근처 후열 중앙 |
| rear-lower | `(4,6)` | 넥서스 근처 후열 중하단 |
| rear-bottom | `(4,4)` | 넥서스 근처 후열 하단 |
| mid-top | `(6,12)` | 중앙 라인 상단 |
| mid-upper | `(6,10)` | 중앙 라인 중상단 |
| mid-center | `(6,8)` | 중앙 라인 중앙 |
| mid-lower | `(6,6)` | 중앙 라인 중하단 |
| mid-bottom | `(6,4)` | 중앙 라인 하단 |
| front-top | `(8,12)` | 적 진입 방향 전열 상단 |
| front-upper | `(8,10)` | 적 진입 방향 전열 중상단 |
| front-center | `(8,8)` | 적 진입 방향 전열 중앙 |
| front-lower | `(8,6)` | 적 진입 방향 전열 중하단 |
| front-bottom | `(8,4)` | 적 진입 방향 전열 하단 |

## 포메이션별 슬롯 적용 기준
- 각 포메이션 문서의 1~5번 효과는 위 15개 후보 중 선택된 5개 슬롯에 매핑한다.
- 1번 효과는 보통 중심 또는 핵심 슬롯에 우선 배정한다.
- 2~3번 효과는 같은 성격의 좌우/상하 보조 슬롯으로 묶는다.
- 4~5번 효과는 화력, 회복, 조건 발동 등 후속 보정 슬롯으로 묶는다.
- 정확한 포메이션별 슬롯 5개는 각 포메이션 문서에서 별도로 지정한다.

## Basic Formation 위치 기준

| 원작 번호 | 프로젝트 슬롯     |       좌표 |
| ----- | ----------- | -------: |
| 1번    | mid-center  |  `(6,8)` |
| 2번    | rear-upper  | `(4,10)` |
| 3번    | rear-lower  |  `(4,6)` |
| 4번    | front-upper | `(8,10)` |
| 5번    | front-lower |  `(8,6)` |

## 참고 링크
- https://dungeonsquad.miraheze.org/wiki/Formation
