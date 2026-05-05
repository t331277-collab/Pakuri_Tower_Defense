# 캐릭터 스탯 강화 목록

> 목적: `어둠의 흔적`을 사용해 캐릭터의 기본 체급을 올리는 스탯 강화 구조를 정의한다.

캐릭터 강화 전체 순서는 [`character-growth-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fcharacter-growth-system) 문서를 따른다.

## 1. 기본 규칙

| 항목 | 규칙 |
|---|---|
| 사용 재화 | 어둠의 흔적 |
| 강화 대상 | 캐릭터별 기본 스탯 |
| 적용 범위 | 런 시작 전 영구 성장 |
| 관리 단위 | 캐릭터 1명 |
| 사용 조건 | 해금 트리에서 캐릭터 스탯 강화 해금 |
| 다음 단계 조건 | 모든 스탯 강화를 완료해야 해당 캐릭터의 액티브 스킬 강화 사용 가능 |
| 초기화 | 캐릭터 단위 초기화에 포함 |
| 강화 방식 | 단계별 성공 확률을 가진 확률 강화 |
| 실패 처리 | 실패해도 현재 단계 유지 |

캐릭터 스탯 강화는 스킬 구조를 바꾸지 않는다.
체력, 공격력, 주문력, 위력, 모든 속성 피해처럼 캐릭터의 기본 체급만 올린다.
성공률과 시도 비용은 [`enhancement-probability-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fenhancement-probability-rule) 문서를 따른다.

## 2. 스탯 강화 노드

| 노드 | 최대 단계 | 단계당 효과 | 5단계 총 효과 | 역할 |
|---|---:|---|---|---|
| 체력 강화 | 5 | 체력 +16% | 체력 +80% | 생존력 증가 |
| 공격력 강화 | 5 | 공격력 +16% | 공격력 +80% | 공격력 기반 스킬 피해 증가 |
| 주문력 강화 | 5 | 주문력 +16% | 주문력 +80% | 주문력 기반 스킬 피해 증가 |
| 위력 강화 | 5 | 위력 +10% | 위력 +50% | 모든 직접 피해 기본 화력 증가 |
| 모든 속성 피해 강화 | 5 | 모든 속성 피해 +10% | 모든 속성 피해 +50% | 물리, 화염, 번개, 얼음, 어둠, 신성 피해 전체 증가 |

## 3. 강화 확률과 비용

모든 캐릭터 스탯 강화 노드는 같은 성공률과 비용 곡선을 사용한다.
실패해도 단계는 하락하지 않는다.

| 목표 단계 | 성공 확률 | 실패 시 | 필요 어둠의 흔적 |
|---:|---:|---|---:|
| 1 | 100% | 단계 유지 | 500 |
| 2 | 70% | 단계 유지 | 800 |
| 3 | 40% | 단계 유지 | 1200 |
| 4 | 20% | 단계 유지 | 1800 |
| 5 | 10% | 단계 유지 | 2600 |

## 4. 완료 조건

| 항목 | 내용 |
|---|---|
| 스탯 강화 완료 | 체력, 공격력, 주문력, 위력, 모든 속성 피해 강화가 모두 최대 단계 |
| 완료 후 열리는 기능 | 해당 캐릭터의 액티브 스킬 강화 사용 가능 |
| 미완료 상태 | 액티브 스킬 강화가 해금되어 있어도 해당 캐릭터의 액티브 스킬 강화 목록은 잠금 표시 |

## 5. UI 표시 기준

| 상태 | 표시 |
|---|---|
| 해금 전 | 스탯 강화 탭 잠금 |
| 해금 후 | 스탯 강화 목록 표시 |
| 강화 가능 | 필요 재화와 다음 단계 효과 표시 |
| 강화 시도 가능 | 성공 확률 표시 |
| 강화 실패 | 실패 안내와 단계 유지 표시 |
| 재화 부족 | 필요 재화는 보이지만 강화 버튼 비활성 |
| 최대 단계 | 완료 표시 |

## 6. 관련 문서

- 캐릭터 강화 시스템: [`character-growth-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fcharacter-growth-system)
- 강화 확률 규칙: [`enhancement-probability-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fenhancement-probability-rule)
- 액티브 스킬 강화 목록: [`active-skill-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Factive-skill-growth-node-list)
- 액티브 스킬 각성 후보와 단계별 효과는 각 캐릭터의 액티브 스킬 문서 참고.
