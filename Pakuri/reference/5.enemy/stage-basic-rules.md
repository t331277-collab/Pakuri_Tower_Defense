# 스테이지 기본 규칙

> 목적: 각 스테이지에서 등장하는 일반 적, 5일차 중간보스, 10일차 중간보스, 보스의 기본 규칙을 정리한다.

## 1. 보스 출현 규칙
- 일반 전투에서는 해당 스테이지의 일반몹 중 1종이 랜덤으로 보스 개체가 되어 등장한다.
- 5일차에는 해당 스테이지의 `5일차 중간보스`가 등장한다.
- 10일차에는 해당 스테이지의 `10일차 중간보스`가 등장한다.
- 11일차 보스전에는 `5일차 중간보스`, `10일차 중간보스`, `스테이지 보스`가 모두 등장한다.
- 전투마다 등장한 보스 개체는 포로 보상에 확정 포함된다.

적은 처치 대상인 동시에 포로 보상의 후보가 되는 성장 자원이다.
즉 전투 구성은 난이도뿐 아니라 포로 확보 가능성과도 연결된다.

## 2. 전투별 보스 개체
모든 전투에는 보스 개체가 존재한다.

| 전투 종류 | 보스 개체 |
|---|---|
| 일반 전투 | 해당 스테이지 일반몹 중 1종이 랜덤 선택되어 보스 개체로 등장 |
| 5일차 중간보스 전투 | 해당 스테이지의 5일차 중간보스가 등장 |
| 10일차 중간보스 전투 | 해당 스테이지의 10일차 중간보스가 등장 |
| 11일차 보스 전투 | 5일차 중간보스 + 10일차 중간보스 + 스테이지 보스가 함께 등장 |

## 3. 일반 전투 보스 생명력
일반 전투 보스는 선택된 일반몹의 최대 생명력이 스테이지에 따라 증가한다.

| 스테이지 | 일반 전투 보스 생명력 배율 |
|---:|---:|
| 1스테이지 | 10~20배 |
| 2스테이지 | 20~30배 |
| 3스테이지 | 30~40배 |
| 4스테이지 | 40~50배 |

## 4. 스테이지별 보스 구성

| 스테이지 | 일반 적 수 | 일반 적 방향 | 5일차 중간보스 | 10일차 중간보스 | 스테이지 보스 |
|---:|---:|---|---|---|---|
| 1스테이지 | 5종 | 용사길드, 기본 직업군 학습 | 수호대장 | 공격대장 | 용사 카린 |
| 2스테이지 | 5종 | 용살자, 속성 용사 | 성창추적자 에단 | 용살자 드레이크 | 용살기사단장 아르센 |
| 3스테이지 | 5종 | 환영, 분신과 타겟 교란 | 환술사 로키 | 예언자 모르간 | 사도 라키아 |
| 4스테이지 | 6종 | 천상, 신성 심판과 보호막 | 집행관 미카엘 | 대행자 토르 | 심판자 세라프 |

## 5. 스테이지별 문서

| 문서                                                                                                                       | 역할         |     |
| ------------------------------------------------------------------------------------------------------------------------ | ---------- | --- |
| [`stage-1-enemies.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F5.enemy%2Fstage-1-enemies) | 1스테이지 적 목록 |     |
| [`stage-2-enemies.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F5.enemy%2Fstage-2-enemies) | 2스테이지 적 컨셉 |     |
| [`stage-3-enemies.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F5.enemy%2Fstage-3-enemies) | 3스테이지 적 컨셉 |     |
| [`stage-4-enemies.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F5.enemy%2Fstage-4-enemies) | 4스테이지 적 컨셉 |     |

## 6. 적 항목 작성 기준

| 항목 | 내용 |
|---|---|
| 적 이름 | 표시 이름 |
| 등장 스테이지 | 1~4스테이지 |
| 등장 전투 | 일반 / 5일차 중간보스 / 10일차 중간보스 / 보스 |
| 공격 타입 | 근거리 / 원거리 / 근거리+원거리 |
| 이동속도 | 전투 이동 속도 |
| 체력 | 기준 체력 |
| 적 스킬 | 전투 중 사용하는 스킬 |
| 보상 연결 | 포로, 골드, 어둠의 흔적, 유물 보상과의 연결 |

## 7. 관련 문서
- 런 일차 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure) 참고.
- 전투 보상은 [`combat-reward-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fcombat-reward-system) 참고.
- 포로 선택 시스템은 [`prisoner-choice-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fprisoner-choice-system) 참고.
