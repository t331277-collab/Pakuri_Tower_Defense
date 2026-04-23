# 베가
![[vega_tower.png]]

## 1. 구현 기준
- 베가는 이동하지 않는 고정 타워다.
- 근접 검술형 캐릭터처럼 보이지만, 실제 전투는 제자리에서 검기와 원거리 참격을 날리는 방식으로 처리한다.
- 핵심 자원은 `이름표식`이다. 상세 규칙은 [`buff-debuff.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Fbuff-debuff) 참고.
- 핵심 역할은 물리 관통 피해, 침묵, 자기 강화, 표식 기반 광역 참격, 표식 기반 단일 처형이다.

## 2. 액티브 스킬

| 스킬 | 속성 | 타입 | 역할 | 상태 |
|---|---|---|---|---|
| A. 삼검난무 | 물리 | 투사체 / 탄창형 | 무한 관통 3연 검기, 표식 누적 | 확정 초안 |
| B. 침묵의 대태도 | 물리 | 비탄창 | 직선 참격, 3초 침묵 | 확정 초안 |
| C. 몰살 허가 | 없음 | 버프 | 일정 시간 행동속도와 공격력 증가 | 확정 초안 |
| D. 검은 명부 개방 | 물리 | 비탄창 | 표식 보유 적 전체 범위 참격 | 확정 초안 |
| E. 최종선고 | 물리 | 비탄창 | 표식 최다 대상 단일 처형 피해 | 확정 초안 |

## 3. 스킬 문서
- A. 삼검난무: [`a-three-sword-flurry.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fa-three-sword-flurry)
- B. 침묵의 대태도: [`b-silent-greatblade.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fb-silent-greatblade)
- C. 몰살 허가: [`c-extermination-permit.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fc-extermination-permit)
- D. 검은 명부 개방: [`d-black-ledger-release.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fd-black-ledger-release)
- E. 최종선고: [`e-final-sentence.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fe-final-sentence)
- F. 각인 심화: [`f-deep-engraving.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Ff-deep-engraving)
- G. 봉인검식: [`g-sealing-sword-form.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fg-sealing-sword-form)
- H. 처형 준비: [`h-execution-prep.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fh-execution-prep)
- I. 연쇄 참결: [`i-chain-cleaving.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fi-chain-cleaving)
- J. 사형 집행인: [`j-executioner.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F2.Monster%2Fvega%2Fskill%2Fj-executioner)

## 4. 상태 / 버프 참조
- 이름표식, 침묵, 몰살 허가: [`buff-debuff.md`](obsidian://open?vault=towerdefense_pakuri_docs&file=docs%2Freference%2F3.combat%2Fbuff-debuff)

## 5. 패시브 스킬

| 패시브 | 대응 액티브 | 역할 | 상태 |
|---|---|---|---|
| F. 각인 심화 | A. 삼검난무 | 이름표식 대상에 대한 아군 전체 집중 화력 강화 | 초안 |
| G. 봉인검식 | B. 침묵의 대태도 | 침묵 대상에 대한 아군 전체 피해와 상태이상 보조 | 초안 |
| H. 처형 준비 | C. 몰살 허가 | 몰살 허가 중 아군 전체 행동속도 강화 | 초안 |
| I. 연쇄 참결 | D. 검은 명부 개방 | 명부 피격 대상 범위 피해 강화 / 범위 피해 시 쿨타임 충전 | 초안 |
| J. 사형 집행인 | E. 최종선고 | 처형 성공 시 아군 쿨타임 충전 / 생존 대상 집중 피해 | 초안 |

## 6. 다음 작업
- 이름표식 지속시간을 영구 유지로 둘지, 웨이브/전투 단위로 초기화할지 결정
- 최종선고 적중 후 표식 50% 소모 규칙이 체감상 과한지 플레이 테스트에서 검토
- 액티브/패시브 특성 수치가 런 중 성장 곡선에 맞는지 검토
- 베가의 CSV 데이터 반영
