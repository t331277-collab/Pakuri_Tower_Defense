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

목표 : 포로 관련 구조는 [`dungeon-squad-combat-levelup-choice-flow.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2Fdungeon-squad-combat-levelup-choice-flow) 참고

스테이지별 적 구성은 [`enemy-stage-index.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F5.enemy%2Fenemy-stage-index) 참고


## 4. 스탯 / 전투 계산 기능
공격력, 주문력, 생명력, 방어력, 속성 저항, 치명타, 행동속도

목표 : 스탯 / 전투 계산 구조는 아래 문서를 기준으로 한다.
- [`combat-stat-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Fcombat-stat-system)
- [`combat-attribute-and-damage-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Fcombat-attribute-and-damage-system)

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

## 6 전투 보상
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

## 8. 이벤트 기능
상점 조우, 이벤트 조우, 이벤트 선택, 비용 지불, 보상 변형

목표 : 프로토타입에는 일반 진행일에 확률적으로 상점과 이벤트가 추가 등장한다.

런 구조 기준:
- 1일, 5일, 10일, 11일에는 상점과 이벤트가 등장하지 않는다.
- 2~4일, 6~9일에는 일반 전투가 고정 등장한다.
- 일반 진행일에는 상점과 이벤트가 추가로 등장할 수 있다.
- 상점은 일반 진행일마다 15% 확률, 스테이지당 최대 1회
- 이벤트는 일반 진행일마다 25% 확률, 등장 횟수 제한 없음

관련 문서:
- 런 일차 구조는 [`dungeon-squad-run-structure.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fdungeon-squad-run-structure) 참고
- 상점 상품과 가격은 [`shop-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fshop-system) 참고
- 이벤트 예시는 [`event-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F4.run%2Fevent-system) 참고


## 9. 진행 / 메타(영구 지속) 기능
어둠의 흔적, 가이던스 스톤, 시련, 난이도

어둠 계열 재화는 5단계 티어로 구성한다.
- 어둠의 흔적
- 어둠의 결정
- 어둠의 정수
- 어둠의 심핵
- 심연의 성흔

메타 성장 전체 정리는 [`meta-growth-index.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fmeta-growth-index) 참고
어둠 계열 재화 티어와 승급 규칙은 [`dark-trace-currency-system.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F6.meta%2Fdark-trace-currency-system) 참고

추가 예정 : 메타 성장 노드 문서 작성
