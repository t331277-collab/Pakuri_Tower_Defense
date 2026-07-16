# Enemy Shared Skill Phase 0 Baseline

## 상태

- 역할: Code Builder
- 작성일: 2026-07-16
- 범위: migration plan Phase 0 기준선
- 기존 `EnemyCombatSystem`, `EnemySkillExecutor`, `EnemySkillData.csv`, `EnemySkillNodes.csv`, `EnemySkillNodeParams.csv`는 유지한다.
- 이 문서의 시나리오는 회귀 검증 항목을 고정한 것이다. Unity Play Mode 수동 재생은 아직 실행하지 않았다.

## 16개 Enemy 스킬 기준선

| skill_id | kind | AP | SP | radius | cooldown | duration | flat | projectile speed/lifetime | action | target |
|---|---|---:|---:|---:|---:|---:|---:|---|---|---|
| Slash | AreaAttack | 1 | 0 | 1.4 | 2 | 0 | 0 | - | DamageArea | CurrentTarget |
| ShieldUp | Shield | 0 | 0 | 0 | 8 | 4 | 0.25 | - | ApplySelfIncomingDamageMultiplier | Self |
| AimedShot | CooldownProjectile | 1.5 | 0 | 7 | 5 | 0 | 0 | 10 / 2.5 | SpawnProjectile | CurrentTarget |
| ShurikenThrow | CooldownProjectile | 1.4 | 0 | 6 | 4 | 0 | 0 | 9 / 2.5 | SpawnProjectile | CurrentTarget |
| Heal | Heal | 0 | 1.2 | 5 | 6 | 0 | 50 | - | Heal | LowestHealthEnemyAlly |
| GuardianFlag | Shield | 0 | 0 | 4 | 10 | 5 | 100 | - | GrantShieldToEnemyAllies | EnemyAlliesInRadius |
| ChargeCommand | Buff | 0 | 0 | 5 | 12 | 6 | 0 | - | ApplyAllyMoveAndDamageMultiplier | EnemyAlliesInRadius |
| SacredSwordWave | CooldownProjectile | 2.2 | 0 | 8 | 9 | 0 | 0 | 12 / 4 | SpawnProjectile | CurrentTarget |
| FireDragonSlash | AreaAttack | 1.2 | 0.4 | 2 | 5 | 0 | 0 | - | DamageArea | CurrentTarget |
| ChainLightning | SingleAttack | 0 | 1.2 | 7 | 5.5 | 0 | 0 | - | DamageThenDelayedChain | CurrentTarget |
| FrostPressure | SingleAttack | 0.8 | 0.4 | 2 | 6 | 3 | 0 | - | DamageAndActionSpeedDebuff | CurrentTarget |
| DarkStab | SingleAttack | 1.6 | 0 | 1.4 | 6 | 0 | 0 | - | Damage | CurrentTarget |
| HolyDragonHeal | Heal | 0 | 1.3 | 5 | 7 | 0 | 80 | - | Heal | LowestHealthEnemyAlly |
| HolySpearThrow | CooldownProjectile | 1.8 | 0.6 | 14 | 8 | 0 | 0 | 14 / 3 | SpawnProjectile | FarthestTower |
| OpeningCharge | SingleAttack | 0 | 0 | 40 | 30 | 5 | 0 | - | ChargeDamageStatus | RandomTower, CombatStart |
| Intimidation | Buff | 0 | 0 | 40 | 30 | 0 | 0 | - | ApplyOutgoingDamageMultiplierStatus | AllTowers, CombatStart |

추가 기준값:

- ChargeCommand: move speed multiplier `1.2`, outgoing damage multiplier `1.15`
- ChainLightning: delay `0.5`, chain multiplier `0.5`, chain radius `7`, primary target 제외
- FrostPressure: action speed bonus `-0.2`, duration `3`
- OpeningCharge: target max health ratio `1`, freeze duration `5`
- Intimidation: outgoing damage multiplier `0.7`

## 21개 legacy node param 이관표

| skill_id | param | value | 새 base 필드 |
|---|---|---|---|
| AimedShot | fallback_speed | 10 | projectile_speed |
| AimedShot | fallback_lifetime | 2.5 | projectile_lifetime |
| ShurikenThrow | fallback_speed | 9 | projectile_speed |
| ShurikenThrow | fallback_lifetime | 2.5 | projectile_lifetime |
| SacredSwordWave | fallback_speed | 12 | projectile_speed |
| SacredSwordWave | fallback_lifetime | 4 | projectile_lifetime |
| FireDragonSlash | attribute | Fire | attribute |
| ChainLightning | attribute | Lightning | attribute |
| ChainLightning | delay | 0.5 | chain_delay_seconds |
| ChainLightning | chain_multiplier | 0.5 | chain_damage_multiplier |
| ChainLightning | chain_radius | 7 | chain_radius |
| FrostPressure | attribute | Ice | attribute |
| FrostPressure | action_speed_bonus | -0.2 | status_action_speed_bonus |
| FrostPressure | duration | 3 | status_duration_seconds |
| DarkStab | attribute | Darkness | attribute |
| HolySpearThrow | fallback_speed | 14 | projectile_speed |
| HolySpearThrow | fallback_lifetime | 3 | projectile_lifetime |
| OpeningCharge | attribute | Physical | attribute |
| OpeningCharge | target_max_health_ratio | 1 | target_max_health_ratio |
| OpeningCharge | status_duration | 5 | status_duration_seconds |
| Intimidation | multiplier | 0.7 | outgoing_damage_multiplier |

## 15개 Enemy 스킬 프리팹 snapshot

Offset은 현재 프리팹 증거로만 기록한다. 새 CSV에는 offset 열을 만들지 않으며 런타임 중심은 `(0,0)`이다.

| prefab | scale | sorting | collider offset | collider size | 이관 hitbox |
|---|---|---:|---|---|---|
| Stage1/Warrior_Skill | `(1,1,1)` | 0 | `(-0.08671975,-0.16931048)` | `(1.297173,1.2520766)` | Slash size |
| Stage1/Shield_Skill | `(0.4971,0.4971,0.4971)` | 0 | - | - | 없음 |
| Stage1/Achor_Skill | `(1,1,1)` | 0 | `(0,0)` | `(0.97,0.45)` | AimedShot size |
| Stage1/Rogue_Skill | `(1,1,1)` | 0 | `(0,0)` | `(0.97,0.45)` | ShurikenThrow size |
| Stage1/Preist_Skill | `(1,1,1)` | 0 | - | - | 없음 |
| Stage1/Shield_King_Skill | `(1,1,1)` | 0 | - | - | 없음 |
| Stage1/Warrior_King_Skill 1 | `(1,1,1)` | 0 | - | - | 없음 |
| Stage1/Karin_Skill 1 | `(0.7507,0.7507,0.7507)` | 0 | `(0.19595027,-0.26720452)` | `(3.1323996,3.3298168)` | SacredSwordWave size |
| Stage2/fire-dragon-slayer | `(1,1,1)` | 50 | `(0,0)` | `(6.24,7.24)` | FireDragonSlash size |
| Stage2/lightning-scout_1 | `(0.5556,0.5556,0.5556)` | 0 | - | - | 없음 |
| Stage2/ice-guard_Skill | `(1,1,1)` | 0 | `(-0.08671975,-0.16931048)` | `(1.297173,1.2520766)` | FrostPressure size |
| Stage2/dark-assassin_Skill | `(0.4066,0.4066,0.4066)` | 50 | `(0,0)` | `(5.65,7.24)` | DarkStab size |
| Stage2/holy-priest_Skill | `(1,1,1)` | 0 | - | - | 없음 |
| Stage2/ethan_Skill | `(1.1569889,0.42703593,1.1569889)` | 0 | `(0,0)` | `(6.21,3.27)` | HolySpearThrow size |
| Stage2/arsen_Skill | `(0.43436006,0.43436006,0.43436006)` | 50 | - | - | 없음 |

`OpeningCharge`에 대응하는 Enemy 스킬 프리팹/씬 매핑은 검사 범위에서 확인되지 않았다. 따라서 새 base 행의 runtime visual은 비워 두었다.

## gameplay collider authority

- Projectile 접촉 collider: AimedShot, ShurikenThrow, SacredSwordWave, HolySpearThrow
- 생성된 skill hitbox collider: Slash, FireDragonSlash, FrostPressure, DarkStab
- 직접 대상/범위 처리이므로 visual collider 비권한: ShieldUp, Heal, GuardianFlag, ChargeCommand, ChainLightning, HolyDragonHeal, Intimidation
- OpeningCharge 판정 권한: 스킬 visual이 아니라 돌진하는 Enemy unit hitbox

## 대표 회귀 시나리오

1. Stage 1 검사: Slash 피해, 반경, 2초 cooldown.
2. Stage 1 궁수/도적: projectile 속도, lifetime, 접촉 hitbox 중심 `(0,0)`.
3. Stage 1 사제/대장: 최저 체력 치유, 주변 보호막, 이동/피해 배율.
4. Stage 1 카린: SacredSwordWave scale과 projectile hitbox.
5. Stage 2 화룡/흑룡/빙룡: 속성 피해, visual sorting, centered hitbox.
6. Stage 2 뇌룡: 0.5초 지연과 0.5 후속 배율.
7. Stage 2 드레이크/아르센: CombatStart 1회 실행.

## 근거

- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillData.csv`
- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillNodes.csv`
- `Pakuri/Assets/CSVdata/runtime/enemy/EnemySkillNodeParams.csv`
- `Pakuri/Assets/Prefab/Enemy/Skill/`
- `Pakuri/Assets/Scripts2/InGame/Core/EnemyCombatSystem.cs`
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Actors/InGameProjectileActor.cs`
