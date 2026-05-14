# 스킬 클래스 설계 문서

> 5개 캐릭터 × 10개 스킬 총 50개 분석 기반  
> Unity C# / ScriptableObject 구조 기준

---

## 1. 전체 클래스 계층

```
SkillData (abstract ScriptableObject)
├── [액티브 계열]
│   ├── ProjectileSkillData      — 탄창식 투사체
│   ├── BeamSkillData            — 지속 빔
│   ├── ZoneSkillData            — 지면 장판
│   ├── BuffSkillData            — 스탯 증가 / 특수효과
│   └── ShieldSkillData          — 실드 부여 (체력/피해 흡수 시스템)
│
└── [패시브 계열]
    └── PassiveSkillData         — triggerType 필드 + CheckTrigger()로 조건 처리
```



---

## 2. 공통 베이스

### `SkillData` (abstract)

| 필드 | 타입 | 설명 |
|------|------|------|
| `skillId` | string | 고유 ID (예: `eve_a_arc_bolt`) |
| `skillName` | string | 표시 이름 |
| `character` | CharacterType (enum) | Eve / Ariel / Rin / Sein / Vega |
| `slot` | SkillSlot (enum) | A~J |
| `isActive` | bool | 액티브 여부 |
| `element` | ElementType (enum) | Physical / Fire / Lightning / Ice / Holy |
| `description` | string | 툴팁 텍스트 |
| `icon` | Sprite | 아이콘 |

```csharp
public enum CharacterType { Eve, Ariel, Rin, Sein, Vega }
public enum SkillSlot { A, B, C, D, E, F, G, H, I, J }
public enum ElementType { Physical, Fire, Lightning, Ice, Holy }
```

---

## 3. 액티브 스킬 클래스

---

### `ProjectileSkillData` — 탄창식 투사체

**해당 스킬**: [Eve-A](../reference/2.Monster/eve/skill/a-arc-bolt.md), [Ariel-A](../reference/2.Monster/ariel/skill/a-judgement-light.md), [Rin-A](../reference/2.Monster/rin/skill/a-shattering-fist.md), [Sein-A](../reference/2.Monster/sein/skill/a-scorching-arrow.md) / [B](../reference/2.Monster/sein/skill/b-blazing-volley.md), [Vega-A](../reference/2.Monster/vega/skill/a-three-sword-flurry.md)

탄약을 소비해 투사체를 발사. 탄창 소진 시 장전 대기.

| 필드 | 타입 | 설명 |
|------|------|------|
| `magazineSize` | int | 최대 탄약 수 |
| `reloadTime` | float | 장전 시간(초) |
| `projectilesPerShot` | int | 발당 투사체 수 (기본 1, Blazing Volley = 5) |
| `baseDamage` | float | 기본 데미지 |
| `statCoefficient` | float | 스탯 계수 |
| `statSource` | StatSource (enum) | Attack / Intelligence |
| `pierceCount` | int | 관통 횟수 (0 = 미관통) |
| `onHitStatus` | StatusEffectData | 피격 시 부여 상태이상 |
| `onHitStatusChance` | float | 상태이상 적용 확률 (0~1) |
| `projectileSpeed` | float | 투사체 속도 |
| `consecutiveHitBonusRate` | float | 연속 피격 데미지 배율 증가 (Blazing Volley용) |
| `consecutiveHitMax` | float | 연속 피격 보너스 상한 |

```csharp
public enum StatSource { Attack, Intelligence }
```

---

### `BeamSkillData` — 지속 빔

**해당 스킬**: [Eve-B (Prism Ray)](../reference/2.Monster/eve/skill/b-prism-ray.md)

쿨다운 기반으로 지속 빔 발사. 틱마다 raycast/overlap으로 피해 적용.  
투사체와 달리 이동 오브젝트 없이 선형 범위를 즉시 감지.

| 필드 | 타입 | 설명 |
|------|------|------|
| `cooldown` | float | 재사용 대기시간 |
| `activeDuration` | float | 빔 지속 시간(초) |
| `tickInterval` | float | 틱 간격(초) |
| `baseDamagePerTick` | float | 틱당 기본 데미지 |
| `statCoefficient` | float | 스탯 계수 |
| `statSource` | StatSource | Attack / Intelligence |
| `beamWidth` | float | 빔 폭 |
| `onHitStatus` | StatusEffectData | 틱 피격 시 상태이상 |
| `onHitStatusChance` | float | 상태이상 적용 확률 |

---

### `ZoneSkillData` — 지면 장판

**해당 스킬**: [Eve-C](../reference/2.Monster/eve/skill/c-frost-field.md) / [D](../reference/2.Monster/eve/skill/d-static-override.md) / [E](../reference/2.Monster/eve/skill/e-drone-beacon.md), [Sein-C](../reference/2.Monster/sein/skill/c-flame-trajectory.md) / [D](../reference/2.Monster/sein/skill/d-superheated-zone.md) / [E](../reference/2.Monster/sein/skill/e-inferno-proclamation.md), [Ariel-E](../reference/2.Monster/ariel/skill/e-archangel-descent.md), [Rin-C](../reference/2.Monster/rin/skill/c-shockwave.md) / [E](../reference/2.Monster/rin/skill/e-collapse-strike.md), [Vega-B](../reference/2.Monster/vega/skill/b-silent-greatblade.md) / [D](../reference/2.Monster/vega/skill/d-black-ledger-release.md)

지면에 피해 구역을 생성. 드론(Eve-E)도 로직 동일, 비주얼만 다름.  
광역 궁극기는 `coverAll = true`로 처리.

| 필드 | 타입 | 설명 |
|------|------|------|
| `cooldown` | float | 재사용 대기시간 |
| `deployDelay` | float | 착지 전 딜레이(초) (Flame Trajectory 등) |
| `coverAll` | bool | 전체 필드 적용 여부 (true면 radius 무시) |
| `radius` | float | 구역 반경 |
| `duration` | float | 구역 지속 시간(초) |
| `tickInterval` | float | 피해 틱 간격(초) |
| `baseDamagePerTick` | float | 틱당 기본 데미지 |
| `statCoefficient` | float | 스탯 계수 |
| `statSource` | StatSource | Attack / Intelligence |
| `onTickStatus` | StatusEffectData | 틱마다 부여 상태이상 |
| `onTickStatusChance` | float | 상태이상 확률 |
| `hasAllyEffect` | bool | 아군 부가 효과 여부 (궁극기용) |
| `allyShieldBase` | float | 아군 실드량 |
| `allyShieldCoeff` | float | 아군 실드 계수 |
| `allyBuffTag` | string | 아군에 적용할 버프 태그 |
| `allyBuffDuration` | float | 아군 버프 지속 시간(초) |

---

### `BuffSkillData` — 스탯 증가 / 특수효과

**해당 스킬**: [Ariel-C (Blessing Wave)](../reference/2.Monster/ariel/skill/c-blessing-wave.md), [Rin-B (Howling)](../reference/2.Monster/rin/skill/b-howling.md), [Vega-C (Extermination Permit)](../reference/2.Monster/vega/skill/c-extermination-permit.md)

아군 전체 또는 자신에게 임시 스탯 버프 부여.

| 필드 | 타입 | 설명 |
|------|------|------|
| `cooldown` | float | 재사용 대기시간 |
| `buffDuration` | float | 버프 지속 시간(초) |
| `target` | BuffTarget (enum) | AllAllies / Self |
| `actionSpeedBonus` | float | 행동 속도 증가율 |
| `attackPowerBonus` | float | 공격력 증가율 |
| `applyStatusTag` | string | 버프 식별 태그 (예: "blessed", "permit") |
| `hasAttachedDamage` | bool | 버프와 동시에 피해 발생 여부 (Blessing Wave) |
| `attachedDamageBase` | float | 동반 피해 기본값 |
| `attachedDamageCoeff` | float | 동반 피해 계수 |
| `attachedDamageRadius` | float | 동반 피해 반경 |

```csharp
public enum BuffTarget { AllAllies, Self }
```

---

### `ShieldSkillData` — 실드 부여

**해당 스킬**: [Ariel-B (Radiant Shield)](../reference/2.Monster/ariel/skill/b-radiant-shield.md)

아군에게 피해 흡수 실드를 부여. 체력/피해 처리 시스템과 직접 연동.

| 필드 | 타입 | 설명 |
|------|------|------|
| `cooldown` | float | 재사용 대기시간 |
| `target` | BuffTarget | AllAllies / Self |
| `shieldBase` | float | 실드 기본량 |
| `shieldCoefficient` | float | 실드 스탯 계수 |
| `shieldStatSource` | StatSource | Attack / Intelligence |
| `shieldDuration` | float | 실드 지속 시간(초) |
| `refreshRule` | ShieldRefreshRule (enum) | Replace / TakeHighest / Stack |

```csharp
public enum ShieldRefreshRule { Replace, TakeHighest, Stack }
```

---

> **[Rin-D (Finishing Blow)](../reference/2.Monster/rin/skill/d-finishing-blow.md), [Vega-E (Final Sentence)](../reference/2.Monster/vega/skill/e-final-sentence.md)** — 하드코딩  
> 조건·스케일 방식이 서로 달라 공통 데이터 클래스로 추상화하지 않음.

---

## 4. 패시브 스킬 클래스

### `PassiveSkillData`

**해당 스킬**: 패시브 F~J 전체 25개 (auto-cast 포함)

모든 패시브는 트리거 기반. 클래스는 하나, 조건은 `triggerType` 필드로 구분.  
`CheckTrigger()`에서 조건을 검사하고, 통과 시 버프 효과를 적용하거나 `linkedSkillId` 스킬을 자동 시전.

| 필드 | 타입 | 설명 |
|------|------|------|
| **[트리거]** | | |
| `triggerType` | PassiveTrigger (enum) | 아래 표 참고 |
| `conditionTag` | string | 대상 상태이상 태그 또는 버프 태그 (triggerType에 따라 해석) |
| `conditionMinStacks` | int | 최소 스택 수 조건 (OnTargetStatus용) |
| `triggerChance` | float | 발동 확률 0~1 (OnEvent용) |
| `triggerHitCount` | int | 발동 기준 누적 피격 수 (OnEvent / OnHitCount용) |
| `internalCooldown` | float | 재발동 방지 쿨다운(초) |
| **[효과]** | | |
| `applyTarget` | PassiveTarget (enum) | AllAllies / ElementUsers / Self |
| `targetElement` | ElementType | 특정 속성 유저 한정 시 사용 |
| `damageBonusRate` | float | 아군 데미지 증가율 |
| `actionSpeedBonusRate` | float | 아군 행동 속도 증가율 |
| `critChanceBonusRate` | float | 치명타 확률 증가율 |
| `resistReduction` | float | 대상 속성 저항 감소 (flat) |
| `resistReductionElement` | ElementType | 저항 감소 대상 속성 |
| `buffDuration` | float | 효과 지속 시간(초). 0 = 상시 |
| `linkedSkillId` | string | 자동 시전할 스킬 ID (OnEvent 전용) |
| `linkedSkillPowerRate` | float | 자동 시전 위력 배율 |

```csharp
public enum PassiveTrigger
{
    Always,          // 상시 적용. 조건 검사 없음. (Eve-F, Ariel-F, Sein-F, Rin-F 스탯 보너스)
    DuringBuff,      // 지정 버프 활성 중에만 효과 적용. (Rin-G, Vega-H)
    AfterSkill,      // 스킬 사용/적중/처치 후 발동. (Ariel-J, Sein-J, Rin-I, Rin-J, Vega-I, Vega-J)
    OnTargetStatus,  // 대상의 상태이상 스택 조건 충족 시. (Eve-H/I/J, Ariel-H/I, Sein-H/I, Vega-F/G)
    OnEvent,         // 특정 이벤트 확률/횟수 기반 자동 시전. linkedSkillId 참조. (Eve-G, Rin-H, Sein-G)
}

public enum PassiveTarget { AllAllies, ElementUsers, Self }

// 런타임에서 조건 검사
public bool CheckTrigger(PassiveTriggerContext ctx) { ... }
```

**트리거 타입별 동작**

| triggerType | conditionTag 해석 | 자동 시전 |
|---|---|---|
| Always | 미사용 | 없음 |
| DuringBuff | 감시할 버프 태그 (예: `"howling"`, `"permit"`) | 없음 |
| AfterSkill | 감시할 스킬 ID 또는 이벤트 (`"onKill"`, `"onCast"`) | 없음 |
| OnTargetStatus | 감시할 상태이상 태그 (예: `"electrify"`, `"name_mark"`) | 없음 |
| OnEvent | 이벤트 종류 (예: `"allyLightningDamage"`, `"physicalHit"`) | `linkedSkillId` 스킬 |

> **Rin-F 양손잡이** 특이 케이스: Always(물리 피해 +12%) + OnHit(추가타 35%) 두 효과가 같은 슬롯에 공존.  
> PassiveSkillData 2개를 F 슬롯에 배열로 등록하거나 `secondaryTrigger` 필드를 추가해 처리.

---

## 5. 트리거가 없는 패시브 여부 확인 결과

**결론: 없음.** 전체 25개 패시브 모두 트리거 기반.

- `Always` 타입(Eve-F, Ariel-F, Sein-F, Rin-F 스탯 보너스)은 "조건 = 항상 참"으로 처리.  
  런타임에서 `CheckTrigger()`를 호출할 필요 없이 전투 시작 시 1회 적용하면 됨.
- `OnEvent` 자동 시전 패시브(Eve-G, Rin-H, Sein-G)는 `linkedSkillId`로 기존 액티브 스킬 데이터를 참조.  
  별도 공격 수치 정의 불필요.

---

## 6. 공통 보조 데이터 구조

### `StatusEffectData` (ScriptableObject)

| 필드 | 타입 | 설명 |
|------|------|------|
| `statusTag` | string | 고유 태그 (예: `"electrify"`, `"chill"`, `"name_mark"`) |
| `statusName` | string | 표시 이름 |
| `isStackable` | bool | 스택 중첩 여부 |
| `maxStacks` | int | 최대 스택 수 |
| `duration` | float | 지속 시간(초) (0 = 무기한) |
| `tickDamageBase` | float | 틱 데미지 기본값 (DoT용) |
| `movementSlowRate` | float | 이동 속도 감소율 |
| `isControlEffect` | bool | CC기 여부 (행동 불가) |
| `triggerConditionTag` | string | 상위 상태로 전환되는 조건 태그 (예: `"freeze if chill >= 4"`) |
| `triggerConditionStacks` | int | 전환 필요 스택 수 |

---

## 7. 캐릭터별 스킬 구성 요약

| 캐릭터 | 액티브 클래스 | 패시브 triggerType | 특이 메커니즘 |
|--------|-------------|-------------------|---------------|
| **Eve** | Projectile + Beam + Zone + Zone(드론) | Always / OnEvent / OnTargetStatus | 3가지 상태이상(전격/냉기/취약) 독립 시너지 트리 |
| **Ariel** | Projectile + Shield + Buff + Zone(coverAll) | Always / OnTargetStatus / AfterSkill | 실드 갱신 규칙(TakeHighest), `blessed` 태그 중심 |
| **Rin** | Projectile + Buff + Zone | Always + OnHit / DuringBuff / OnEvent / AfterSkill | Rin-F 슬롯에 패시브 2개, 하드코딩 처형기(Rin-D) |
| **Sein** | Projectile + Zone + Zone(coverAll) | Always / OnEvent / OnTargetStatus / AfterSkill | 순수 화염 특화, 하드코딩 처형기 없음 |
| **Vega** | Projectile + Buff | OnTargetStatus / DuringBuff / AfterSkill | `name_mark` 스택 별도 카운터, 처치 시 소모, 하드코딩 처형기(Vega-E) |

---

## 8. 구현 우선순위 제안

```
1단계 (기반)
  - SkillData 베이스 + ElementType / StatSource enum
  - StatusEffectData + StatusEffectManager
  - ProjectileSkillData + 탄창/장전 시스템

2단계 (다양화)
  - ZoneSkillData + ZoneInstance (틱 처리)
  - BuffSkillData + ShieldSkillData + 실드 갱신 규칙
  - BeamSkillData + raycast 틱 처리

3단계 (패시브)
  - PassiveSkillData + PassiveTrigger enum
  - Always 타입: 전투 시작 시 1회 적용
  - OnTargetStatus / DuringBuff: 상태이상·버프 이벤트 버스 구독
  - AfterSkill: 스킬 이벤트 버스 구독
  - OnEvent: 피해 이벤트 버스 + linkedSkillId 자동 시전

4단계 (고급)
  - Rin-D / Vega-E 처형 로직 하드코딩
  - Rin-F 이중 패시브 슬롯 처리
  - name_mark 스택 카운터 + 처치 시 소모 로직
```
