# 메타 성장 인덱스

> 목적: 런 외부에서 유지되는 재화, 해금, 성장 콘텐츠를 한 곳에서 관리한다.

## 1. 기본 방향
메타 성장은 런 실패 후에도 다음 도전을 조금씩 넓혀주는 장기 진행 축이다.

- 런 내부 재화인 `골드`와 메타 재화는 분리한다.
- 전투 보상으로 얻는 기본 메타 재화는 `어둠의 흔적`이다.
- 프로토타입의 1차 메타 성장 사용처는 액티브 스킬 각성, 캐릭터 스탯 강화, 액티브 스킬 강화다.
- 캐릭터 강화는 `액티브 스킬 각성 > 캐릭터 스탯 강화 > 액티브 스킬 강화` 순서로 진행한다.
- 액티브 스킬 각성, 캐릭터 스탯 강화, 액티브 스킬 강화는 `심연의 제단` 화면에서 진행한다.
- 캐릭터 강화는 단계별 성공 확률을 가진 확률 강화로 진행하며, 실패해도 단계는 하락하지 않는다.
- 모든 캐릭터 강화는 5단계로 통일하고, 시작 단계부터 어둠의 흔적 비용을 높게 요구한다.
- 어둠의 흔적은 캐릭터 성장뿐 아니라 런 경제, 유물 연구, 전투 보조, 진행 확장 콘텐츠를 여는 해금 트리에도 사용한다.
- 패시브는 메타 성장에서 다루지 않는다.
- 캐릭터별 메타 강화는 초기화할 수 있으며, 투자한 `어둠의 흔적`은 전액 반환한다.
- 초기화할 때마다 `어둠의 흔적` 50개를 고정 수수료로 소모하며, 이 수수료는 반환하지 않는다.

## 2. 현재 확정된 메타 요소

| 분류 | 문서 | 상태 |
|---|---|---|
| 어둠의 흔적 해금 트리 | [`dark-trace-unlock-tree.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fdark-trace-unlock-tree) | 하단 시작 / 상단 진행 / 노드 클릭 설명 기준 정리 |
| 캐릭터 강화 시스템 | [`character-growth-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fcharacter-growth-system) | 강화 순서와 잠금 조건 초안 완료 |
| 강화 확률 규칙 | [`enhancement-probability-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fenhancement-probability-rule) | 성공률 / 실패 유지 / 비용 수치 완료 |
| 액티브 스킬 각성 | 각 캐릭터 액티브 스킬 문서 | 스킬별 각성 후보 2개 / 5단계 효과 초안 완료 |
| 캐릭터 스탯 강화 | [`meta-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fmeta-growth-node-list) | 5단계 확률 강화 초안 완료 |
| 액티브 스킬 강화 | [`active-skill-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Factive-skill-growth-node-list) | 5단계 확률 강화 초안 완료 |
| 심연의 제단 UI | [`4. abyss-altar-layout.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F7.UI%2F4.%20abyss-altar-layout) | 캐릭터 강화 화면 초안 완료 |
| 캐릭터 / 타워 해금 | 미작성 | 후속 결정 |
| 난이도 해금 | 미작성 | 후속 결정 |
| 시작 보너스 | 미작성 | 후속 결정 |

## 3. 메타 성장 문서 후보
앞으로 아래 문서로 분리한다.

- `meta-growth-node-list.md`
- `active-skill-growth-node-list.md`
- `character-growth-system.md`
- `enhancement-probability-rule.md`
- `dark-trace-unlock-tree.md`
- `meta-unlock-system.md`
- `difficulty-unlock-system.md`
- `starting-bonus-system.md`
- `meta-economy-balance.md`

## 4. 관련 문서
- 어둠의 흔적 지급량은 [`combat-reward-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fcombat-reward-system) 참고.
- 캐릭터 강화 순서와 잠금 조건은 [`character-growth-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fcharacter-growth-system) 참고.
- 강화 성공률과 실패 규칙은 [`enhancement-probability-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fenhancement-probability-rule) 참고.
- 캐릭터별 액티브 스킬 각성 후보와 단계별 효과는 각 캐릭터의 액티브 스킬 문서 참고.
- 캐릭터 스탯 강화와 초기화 규칙은 [`meta-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fmeta-growth-node-list) 참고.
- 액티브 스킬 강화는 [`active-skill-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Factive-skill-growth-node-list) 참고.
- 캐릭터 강화 화면은 [`4. abyss-altar-layout.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F7.UI%2F4.%20abyss-altar-layout) 참고.
- 전술 지휘 조준 규칙은 [`aiming-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Faiming-system) 참고.
- 전체 콘텐츠 범위는 [`dungeon-squad-content-catalog.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F1.prototype-goals%2Fdungeon-squad-content-catalog) 참고.
