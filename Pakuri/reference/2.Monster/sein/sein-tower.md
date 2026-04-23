# 세인 타워

![[sein_tower.png]]
이미지 바뀔 예정
## 1. 설계 기준
- 세인은 불속성 퓨어딜러 타워다.
- 외형은 바람을 다루는 궁수처럼 보이지만, 실제 전투 콘셉트는 뜨거운 상승기류와 화염 화살을 사용하는 화염 저격수로 둔다.
- 주축은 화염 탄창 화력과 화염 저항 감소다.
- 보조축은 화염 장판과 디아블로의 종말 계열을 참고한 광역 결전기다.
- 세인은 제어/보호보다 피해량, 화염 중첩, 광역 마무리에 집중한다.

## 2. 액티브 스킬

| 스킬 | 속성 | 역할 | 상태 |
|---|---|---|---|
| A. 열풍 화살 | 화염 | 기본 탄창형 투사체 / 화염 피해 | 초안 |
| B. 작열 난사 | 화염 | 다연발 화염 탄창 / 단일 화력 집중 | 초안 |
| C. 화염궤도 | 화염 | 지연 폭발 화살 / 범위 보조 공격 | 초안 |
| D. 초열 지대 | 화염 | 비탄창 지속 피해 장판 | 초안 |
| E. 종말의 사선 | 화염 | 전장 광역 결전기 / 디아블로 종말 참고 | 초안 |

## 3. 스킬 문서
- A. 열풍 화살: [`a-scorching-arrow.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fa-scorching-arrow)
- B. 작열 난사: [`b-blazing-volley.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fb-blazing-volley)
- C. 화염궤도: [`c-flame-trajectory.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fc-flame-trajectory)
- D. 초열 지대: [`d-superheated-zone.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fd-superheated-zone)
- E. 종말의 사선: [`e-doomsday-line.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fe-doomsday-line)
- F. 가열 조준: [`f-heated-aim.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Ff-heated-aim)
- G. 불꽃 탄막: [`g-flame-barrage.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fg-flame-barrage)
- H. 연소 궤적: [`h-burning-trajectory.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fh-burning-trajectory)
- I. 열압 확산: [`i-thermal-spread.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fi-thermal-spread)
- J. 종말 예고: [`j-doomsday-omen.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fsein%2Fskill%2Fj-doomsday-omen)

## 4. 패시브 스킬

| 패시브 | 대응 액티브 | 역할 | 상태 |
|---|---|---|---|
| F. 가열 조준 | A. 열풍 화살 | 모든 아군 화염 피해 강화 / 화염 스킬 보유 아군 강화 | 초안 |
| G. 불꽃 탄막 | B. 작열 난사 | 아군 화염 피해 시 작열 난사 자동 발동 | 초안 |
| H. 연소 궤적 | C. 화염궤도 | 화염 피해 대상 저항 감소 | 초안 |
| I. 열압 확산 | D. 초열 지대 | 장판 피해 대상에게 모든 아군 화염 피해 증가 | 초안 |
| J. 종말 예고 | E. 종말의 사선 | 결전기 후 화염 노출 및 쿨타임 보조 | 초안 |

## 5. 패시브 비율

| 유형 | 개수 | 대상 |
|---|---:|---|
| 자기/대응 스킬 강화 | 1 | 종말 예고 |
| 아군 전체 강화 | 2 | 가열 조준, 열압 확산 |
| 아군 피해 트리거 | 1 | 불꽃 탄막 |
| 상태 대상 보너스/저항 감소 | 1 | 연소 궤적 |
| 전투 시작/처치 보상 | 0 | 추후 특성에서 보조 |

## 6. 다음 작업
- 화염 저항 감소와 화염 노출의 아이콘/표기명 확정
- 액티브 마스터 스킬 수치 조정
- CSV 데이터 반영
