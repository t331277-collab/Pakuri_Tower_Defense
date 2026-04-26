# 이벤트 시스템

> 목적: 일반 전투와 정예 전투에 진입한 직후 발생할 수 있는 이벤트형 콘텐츠를 정의한다.

현재 프로토타입의 일반 진행일 기본 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure)에서 관리한다.

## 1. 현재 적용 규칙

현재 일반 진행일에는 `일반 전투`, `정예 전투`, `상점` 중 하나가 진행될 수 있다.
이벤트는 별도 진행일이나 상점 대체 선택지가 아니라, 일반 전투 또는 정예 전투에 진입한 직후 20% 확률로 끼어든다.
이벤트 선택과 결과 처리가 끝나면 원래 진입했던 전투로 복귀한다.

| 구분 | 현재 규칙 |
|---|---|
| 발생 대상 | 일반 전투, 정예 전투 |
| 발생 시점 | 전투 진입 직후 |
| 발생 확률 | 20% |
| 처리 방식 | 이벤트 선택지 선택 후 결과 적용, 이후 원래 전투로 복귀 |
| 상점과의 관계 | 상점을 선택한 날은 전투하지 않으므로 이벤트도 발생하지 않음 |
| 정예 접두 이벤트 | 사용 |
| 상점 선택지 | 사용 |
| 이벤트 목록 문서 | 사용 |

정예 접두 이벤트는 별도 문서에서 관리한다.

- [`elite-combat-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Felite-combat-system)

이벤트 목록은 별도 문서에서 관리한다.

- [`event-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fevent-list)

## 2. 전투 진입 흐름

일반 전투 또는 정예 전투를 선택하면 아래 순서로 처리한다.

1. 전투에 진입한다.
2. 이벤트 발생 여부를 20% 확률로 판정한다.
3. 이벤트가 발생했다면 이벤트 선택지를 보여준다.
4. 선택 결과를 적용한다.
5. 원래 진입했던 일반 전투 또는 정예 전투로 복귀한다.
6. 전투 종료 후 전투 보상을 지급한다.

## 3. 관련 문서

- 런 내부 반복 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure) 참고.
- 정예 전투 규칙은 [`elite-combat-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Felite-combat-system) 참고.
- 이벤트 목록은 [`event-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fevent-list) 참고.
