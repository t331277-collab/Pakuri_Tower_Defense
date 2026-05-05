# 던전 스쿼드 콘텐츠 카탈로그

> 목적: 원작에 어떤 콘텐츠 있고, 우리 게임의 프로토타입은 어디까지 구현할 지 정리해둔 문서


## 1. 스쿼드 / 몬스터 기능
메인 몬스터, 스쿼드 몬스터, 몬스터 토큰, 몬스터 성장


제외 : 타락 영웅, 고문실은 후속 콘텐츠로 별도 정리

## 2. 몬스터 목록
바포메트, 비홀더, 케르베로스, 키메라

상황 : 5캐릭 작성 완료, 다만 스킬은 직접 플레이해보며 리워크 필요

## 3. 영웅 / 적 기능
일반 영웅, 전투 보스, 중간보스, 보스 영웅, 포로

목표 : 포로 관련 구조는 [`prisoner-choice-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fprisoner-choice-system) 참고

스테이지 기본 규칙은 [`stage-basic-rules.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F5.enemy%2Fstage-basic-rules) 참고


## 4. 스탯 / 전투 계산 기능
공격력, 주문력, 생명력, 방어력, 속성 저항, 치명타, 행동속도

목표 : 스탯 / 전투 계산 구조는 아래 문서를 기준으로 한다.
- [`combat-stat-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Fcombat-stat-system)
- [`combat-attribute-and-damage-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Fcombat-attribute-and-damage-system)
- [`aiming-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Faiming-system)

## 5. 장비 기능
기본 장비, 조합 장비, 전설 장비, 장비 슬롯

캐릭터가 장착하는 장비는 일단 없다.
유물 시스템으로 대체 예정이며, 유물 시너지 목록은 [`artifact-synergy-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fartifact-synergy-list) 참고

현재 유물 시너지:
- 정령계약
- 처형관
- 선택받은자
- 파수꾼
- 포격대
- 추적자

## 6. 전투 보상
포로, 유물, 골드, 어둠의 흔적

모든 전투 종료 후 포로와 재화를 지급한다.
모든 전투에는 보스 개체가 존재하며, 해당 보스는 포로 보상에 확정 포함된다.
5일차 중간보스전, 10일차 중간보스전, 보스전에서는 유물을 드랍한다.
유물은 파티 시너지형 런 내부 강화로 사용한다.
전투 보상 구조는 [`combat-reward-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fcombat-reward-system) 참고



## 7. 어둠신
마리엘, 레베카, 릴리스, 타니아

목표 : 어둠신 시스템은 현재 프로토타입 범위에서 제외
제외 : 후속 콘텐츠로 어둠신 시스템 문서 작성

## 8. 진행일 선택 기능
일반 전투, 정예 전투, 상점, 전투 진입 이벤트

목표 : 프로토타입에는 일반 진행일마다 오늘 진행할 콘텐츠를 선택하게 한다.

런 구조 기준:
- 1일, 5일, 10일, 11일에는 고정 전투만 등장한다.
- 2~4일, 6~9일에는 일반 전투가 기본 선택지로 등장한다.
- 정예 전투는 매 일반 진행일마다 30% 확률로 선택지에 추가된다.
- 정예 전투를 선택하면 해당 일차 전투 전체에 정예 접두 효과가 적용된다.
- 정예 전투 추가 보상은 포로 1명이다.
- 상점은 6~9일 중 하루에 선택지로 등장한다.
- 상점을 선택하면 해당 일차는 전투 없이 상점 이용만 하고 다음 일차로 넘어간다.
- 6~9일에는 일반 전투, 정예 전투, 상점 선택지가 한 번에 모두 보일 수 있다.
- 일반 전투와 정예 전투는 전투 진입 직후 20% 확률로 이벤트가 발생할 수 있다.
- 이벤트 선택과 결과 처리가 끝나면 원래 전투로 복귀한다.

관련 문서:
- 런 일차 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure) 참고
- 정예 전투 규칙은 [`elite-combat-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Felite-combat-system) 참고
- 상점 상품과 가격은 [`shop-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fshop-system) 참고
- 전투 진입 이벤트는 [`event-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fevent-system) 참고

## 9. 상점 기능
상점 등장일, 상점 이용, 골드 소비

목표 : 각 스테이지 6~9일 중 하루에 상점 선택지를 제공한다.

런 구조 기준:
- 상점은 6~9일 중 하루가 무작위로 지정된다.
- 상점은 스테이지당 1회만 존재한다.
- 상점을 선택하면 해당 일차는 전투 없이 상점 이용만 하고 다음 일차로 넘어간다.
- 상점 이용일에는 전투 보상을 지급하지 않는다.
- 상점이 등장한 날에도 일반 전투와 정예 전투 선택지가 함께 보일 수 있다.

관련 문서:
- 런 일차 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure) 참고
- 상점 상품과 가격은 [`shop-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fshop-system) 참고


## 10. 진행 / 메타(영구 지속) 기능
어둠의 흔적, 가이던스 스톤, 시련, 난이도

프로토타입 1차 메타 성장은 `어둠의 흔적`을 사용해 액티브 스킬 각성, 캐릭터 스탯 강화, 액티브 스킬 강화를 진행하는 구조로 둔다.
액티브 스킬 각성, 캐릭터 스탯 강화, 액티브 스킬 강화는 `심연의 제단` 화면에서 진행한다.
해금 트리 기준으로는 액티브 스킬 각성을 먼저 열어 스킬의 변주 방향을 정하고, 이후 캐릭터 스탯 강화와 액티브 스킬 강화로 기본 체급과 화력을 키운다.
모든 캐릭터 강화는 단계별 성공 확률을 가진 확률 강화로 진행하며, 실패해도 강화 단계는 하락하지 않는다.
모든 캐릭터 강화는 5단계로 통일하고, 시작 단계부터 어둠의 흔적을 많이 요구하는 장기 성장 콘텐츠로 둔다.
패시브는 메타 성장에서 다루지 않는다.
강화는 캐릭터 단위로 관리하고, 초기화 시 투자한 `어둠의 흔적`은 전액 반환한다.
단, 초기화할 때마다 `어둠의 흔적` 50개를 고정 수수료로 소모한다.

메타 성장 재화는 `어둠의 흔적`만 사용한다.

메타 성장 전체 정리는 [`meta-growth-index.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fmeta-growth-index) 참고
캐릭터 강화 전체 구조는 [`character-growth-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fcharacter-growth-system) 참고
강화 성공률과 실패 규칙은 [`enhancement-probability-rule.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fenhancement-probability-rule) 참고
캐릭터 스탯 강화 노드는 [`meta-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fmeta-growth-node-list) 참고
액티브 스킬 강화 노드는 [`active-skill-growth-node-list.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Factive-skill-growth-node-list) 참고
액티브 스킬 각성 후보와 단계별 효과는 각 캐릭터의 액티브 스킬 문서 안에 함께 작성한다.
심연의 제단 화면은 [`4. abyss-altar-layout.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F7.UI%2F4.%20abyss-altar-layout) 참고

추가 예정 : 캐릭터 해금, 난이도 해금, 시작 보너스 문서 작성
