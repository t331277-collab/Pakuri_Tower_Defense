# 포로 선택 시스템

> 목적: 전투 후 획득한 포로를 런 내부 성장 선택지로 사용하는 규칙을 정의한다.

## 1. 기본 개념
포로는 전투 후 얻는 핵심 성장 자원이다.
단순 재화가 아니라, 몬스터 확보와 강화 방향을 고르는 분기 자원으로 사용한다.

포로는 아래 네 가지 방식으로 활용한다.

1. 현현
2. 공양
3. 동화
4. 고문 / 타락

## 2. 현현
현현은 포로를 사용해 신규 몬스터를 소환하는 선택지다.

| 항목 | 규칙 |
|---|---|
| 목적 | 스쿼드 확장 |
| 실패 확률 | 30% |
| 성공 결과 | 신규 몬스터 확보 |

## 3. 공양
공양은 포로를 기존 몬스터에게 사용해 스킬 또는 강화 선택지를 얻는 방식이다.

| 항목 | 규칙 |
|---|---|
| 선택지 개수 | 무작위 3개 |
| 선택 개수 | 1개 |
| 후보 구성 | 미습득 액티브, 미습득 패시브, 이미 배운 스킬의 강화 선택지 |
| 후보 부족 | 남은 후보 수만큼만 표시 |

공양 선택지의 세부 생성 규칙은 [`skill-choice-pool-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fskill-choice-pool-rule) 참고.

## 4. 동화
동화는 기존 몬스터에 해당 포로의 스탯과 특수 능력을 적용하는 방식이다.

- 기존에 동화했던 능력은 모두 삭제된다.
- 새 포로의 동화 능력치만 남는다.
- 기존 몬스터의 방향을 다시 잡는 덮어쓰기형 성장으로 사용한다.

## 5. 고문 / 타락
고문 / 타락은 포로를 다른 방식의 성장 자원으로 전환하는 후속 콘텐츠다.

- 현재 프로토타입 범위에서는 제외한다.
- 추후 스쿼드 확장, 적 회유, 특수 보상 계열로 확장할 수 있다.

## 6. 기획적 의미
포로 시스템의 핵심은 하나의 전투 보상이 여러 성장 방향으로 갈라지는 데 있다.

| 선택 | 의미 |
|---|---|
| 현현 | 신규 몬스터 확보 |
| 공양 | 기존 몬스터 스킬 성장 |
| 동화 | 기존 몬스터 능력 재구성 |
| 고문 / 타락 | 후속 확장 콘텐츠 |

## 7. 관련 문서
- 전투 보상 규칙은 [`combat-reward-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fcombat-reward-system) 참고.
- 런 내부 반복 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure) 참고.
- 스킬 선택지 풀은 [`skill-choice-pool-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fskill-choice-pool-rule) 참고.
