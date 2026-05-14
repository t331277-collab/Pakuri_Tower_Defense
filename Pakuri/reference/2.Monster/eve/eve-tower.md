# 이브 타워

![[eve_tower.png|592]]

## 1. 설계 기준
- 이브는 번개/얼음 속성 엔진형 + 상태 제어 보조형 타워다.
- 주축은 번개/얼음 피해와 감전, 추위, 빙결, 취약 상태 연계다.
- 보조축은 아군이 번개/얼음 피해를 줄 때 이브의 스킬이 연쇄 발동하는 구조다.
- 이브는 단독 폭딜러보다 파티의 속성 피해, 상태 대상 피해, 자동 발동 빈도를 높이는 보조형 딜러로 둔다.

## 2. 액티브 스킬

| 스킬 | 속성 | 역할 | 상태 |
|---|---|---|---|
| A. 아크 볼트 | 번개 | 기본 탄창형 투사체 / 감전 부여 | 확정 |
| B. 프리즘 레이 | 번개 / 얼음 | 직선 관통 광선 / 속성 트리거 대상 | 작성 |
| C. 프로스트 필드 | 얼음 | 장판 제어 / 추위와 빙결 | 작성 |
| D. 스태틱 오버라이드 | 번개 | 감전 스택 폭발 / 중첩 광역 | 작성 |
| E. 플라즈마 필드 | 번개 | 전격 필드 전개 / 취약 누적 | 작성 |

## 3. 스킬 문서
- A. 아크 볼트: [`a-arc-bolt.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fa-arc-bolt)
- B. 프리즘 레이: [`b-prism-ray.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fb-prism-ray)
- C. 프로스트 필드: [`c-frost-field.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fc-frost-field)
- D. 스태틱 오버라이드: [`d-static-override.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fd-static-override)
- E. 플라즈마 필드: [`e-drone-beacon.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fe-drone-beacon)
- F. 전압 보정: [`f-voltage-calibration.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Ff-voltage-calibration)
- G. 입자 분리: [`g-weakness-analysis.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fg-weakness-analysis)
- H. 냉각 알고리즘: [`h-particle-separation.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fh-particle-separation)
- I. 과전류 회로: [`i-cooling-algorithm.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fi-cooling-algorithm)
- J. 약점 분석: [`j-overcurrent-circuit.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Feve%2Fskill%2Fj-overcurrent-circuit)

## 4. 패시브 스킬

| 패시브 | 대응 액티브 | 역할 | 상태 |
|---|---|---|---|
| F. 전압 보정 | A. 아크 볼트 | 번개 스킬 보유 아군 전투 시작 보호막 / 감전 대상 피해 강화 | 작성 |
| G. 입자 분리 | B. 프리즘 레이 | 아군 번개/얼음 피해 시 프리즘 레이 자동 발동 | 작성 |
| H. 냉각 알고리즘 | C. 프로스트 필드 | 추위/빙결 대상 피해 및 상태이상 확률 강화 | 작성 |
| I. 과전류 회로 | D. 스태틱 오버라이드 | 감전 대상 번개 피해 강화 / 번개 저항 감소 | 작성 |
| J. 약점 분석 | E. 플라즈마 필드 | 취약 대상 피해 증가 / 모든 저항 감소 | 작성 |

## 5. 패시브 비율

| 유형 | 개수 | 대상 |
|---|---:|---|
| 아군 전체 속성 강화 | 2 | 전압 보정, 과전류 회로 |
| 아군 피해 트리거 | 1 | 입자 분리 |
| 상태 대상 보너스 | 2 | 냉각 알고리즘, 약점 분석 |
| 자기 스킬 직접 강화 | 0 | 특성 단계에서 보조 |

## 6. 다음 작업
- 액티브 마스터 스킬을 속성 엔진형 기준으로 재검토
- 감전, 추위, 빙결, 취약의 상태 문서 정리
- 이브 CSV 데이터 반영
