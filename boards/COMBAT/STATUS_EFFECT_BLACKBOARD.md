# STATUS_EFFECT_BLACKBOARD

## Archived History

The pre-cleanup file, including all completed July tasks, is preserved at `boards/ARCHIVE/ACTIVE_BOARD_SNAPSHOT_2026-07-28/COMBAT/STATUS_EFFECT_BLACKBOARD.md`.

## Task: 2026-08-03 Remove Unused RemovePassiveStatus

### Task title

저장소에서 호출되지 않는 `RemovePassiveStatus`를 삭제한다.

### Goals

- `InGameCombatManager.RemovePassiveStatus`의 미사용 정의를 삭제한다.
- `DispatchCombatStartOnce`, `DispatchOutgoingDamageTriggers`, `ApplyOutgoingAdditionalDamageStatuses`의 실제 실행 경로는 유지한다.

### Constraints

- 상태 제거, 전투 시작, 피해 후속반응의 공통 실행 경로는 변경하지 않는다.
- `StatusState.Remove`, `ConsumeStatusStacks`와 기존 Trigger 경로는 변경하지 않는다.
- Unity Play Mode 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

Implementation complete. 잔여 참조 검사, diff 검사와 솔루션 빌드를 완료했다.

### Next Actions

- 별도 작업 없음. 필요 시 사용자 Play Mode에서 CombatStart 및 OnOutgoingDamage 반응을 확인한다.

### Evidence

- `InGameCombatManager.RemovePassiveStatus` 정의를 삭제했다.
- 저장소 C# 검색에서 `RemovePassiveStatus` 잔여 결과는 0건이다.
- `DispatchCombatStartOnce`는 플레이어·적 유닛 등록 경로에서 호출되고 `SkillTrigger.ExecuteCombatStart`를 유닛당 한 번 발행한다.
- `DispatchOutgoingDamageTriggers`는 일반 피해 후 호출되며 `SkillTrigger.ExecuteOutgoingDamage`와 상태 기반 추가 피해를 연결한다.
- `ApplyOutgoingAdditionalDamageStatuses`는 outgoing 추가 피해 상태를 조회해 기존 `ApplyDamage` 경로로 후속 피해를 적용한다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`가 통과했다.

### History

- 2026-08-03: 사용자가 네 메소드의 실제 사용 여부를 확인하고 미사용 `RemovePassiveStatus` 삭제를 요청했다.
- 2026-08-03: Code Builder가 호출부가 없는 메소드를 삭제하고 나머지 세 메소드의 실행 경로를 보존했다.

## Task: 2026-08-03 Clarify Status Duration Ownership

### Task title

상태 지속시간 변경 책임을 `StatusState`와 `InGameCombatManager`의 실제 코드 역할에 맞춰 명확히 한다.

### Goals

- `StatusState.ExtendDurations`가 지속시간 값을 변경하는 책임임을 주석으로 기록한다.
- `InGameCombatManager.ExtendStatusDuration`가 상태 저장소 호출과 전투 표현 갱신을 담당함을 주석으로 기록한다.
- 상태 지속시간 로직을 중복 이동하거나 `UnitCombatState`에 추가하지 않는다.

### Constraints

- 지속시간 계산·변경과 상태 만료 순서를 변경하지 않는다.
- 보호막 동기화, 유닛 표시 갱신, 상태 시각 효과 갱신을 유지한다.
- Unity Play Mode 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

Implementation complete. 주석 확인, diff 검사와 솔루션 빌드를 완료했다.

### Next Actions

- 별도 작업 없음. 필요 시 사용자 Play Mode에서 상태 지속시간 연장 표현을 확인한다.

### Evidence

- `StatusState.cs:305`의 `UnitStatusCollection.ExtendDurations`가 조건에 맞는 `StatusRuntimeInstance`의 지속시간을 실제 변경한다.
- `InGameCombatManager.cs:389`의 `ExtendStatusDuration`은 `target.Statuses.ExtendDurations` 호출 후 `SyncShield`, `RefreshDisplay`, 상태 시각 효과 갱신을 수행한다.
- `SkillExecution.cs:1954`의 반응 명령은 기존처럼 `combatManager.ExtendStatusDuration` 공통 진입점을 사용한다.
- `git diff --check`가 통과했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-03: 사용자가 상태 지속시간 관리는 `StatusState` 또는 `UnitCombatState` 책임인지 확인했다.
- 2026-08-03: Code Builder가 실제 값 변경은 `StatusState`, 전투 표현 연결은 `InGameCombatManager`라는 현재 구조를 주석으로 명확히 했다.

## Task: 2026-08-03 Move Status Visual Creation To BuffSkillExecutor

### Task title

상태이상 시각 효과 생성 메소드를 `BuffSkillExecutor`로 이동하고 공통 상태 적용 메소드에 책임 주석을 추가한다.

### Goals

- `ShowStatusEffectVisual` 구현을 `BuffSkillExecutor.cs`로 이동한다.
- `ApplyStatus`, `ApplyShieldStatus`, `ExtendStatusDuration`의 역할을 코드 주석으로 명시한다.
- 기존 상태 적용·보호막 적용·지속시간 연장 경로의 시각 효과 생성을 유지한다.

### Constraints

- 상태 확률·저항·지속시간·중첩 규칙은 `StatusCombatRules`와 `UnitStatusCollection`의 기존 경로를 유지한다.
- Buff뿐 아니라 Line, Single, Projectile, Zone과 반응 상태 적용의 시각 효과도 끊기지 않게 한다.
- Unity Play Mode 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

Implementation complete. 잔여 참조 검사, diff 검사와 솔루션 빌드를 완료했다.

### Next Actions

- Unity Play Mode에서 일반 상태, 보호막 상태와 지속시간 연장 후 시각 효과 갱신을 확인한다.

### Evidence

- `BuffSkillExecutor.cs:207`에 `ShowStatusEffectVisual`을 이동하고 `EffectManager.CreateEffect` 호출을 유지했다.
- `InGameCombatManager.cs:350`, `384`, `417`이 이동된 `BuffSkillExecutor.ShowStatusEffectVisual`을 호출한다.
- `InGameCombatManager.cs:323`, `354`, `388`에 상태 적용, 보호막 적용, 지속시간 연장 책임 주석을 추가했다.
- `InGameCombatManager.cs`의 기존 `ShowStatusEffectVisual` 구현은 삭제됐다.
- `rg`로 새 메소드와 세 호출부를 확인했고 `git diff --check`가 통과했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-03: 사용자가 상태 시각 효과 생성을 `BuffSkillExecutor`로 이동하고 공통 상태 메소드에 주석을 추가하도록 요청했다.
- 2026-08-03: Code Builder가 구현을 이동하고 기존 공통 상태 적용 경로의 호출을 연결한 뒤 빌드를 완료했다.

## Task: 2026-08-03 Inline InGameCombatManager Round Calls

### Task title

`InGameCombatManager`의 단일 사용 `Round` 래퍼를 제거하고 호출부에서 `Mathf.Round`를 직접 사용한다.

### Goals

- 보호막과 HP 차감 및 회복의 반올림 표현을 호출부에 직접 둔다.
- 사용처가 사라진 `Round` 메서드를 삭제한다.
- 기존 음수 보정과 반올림 동작을 유지한다.

### Constraints

- `UnitCombatState`와 다른 스크립트의 반올림 로직은 변경하지 않는다.
- HP·보호막 계산 순서와 자원 변경 결과를 변경하지 않는다.
- Unity Play Mode 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

Implementation complete. 잔여 참조 검사, diff 검사와 솔루션 빌드를 완료했다.

### Next Actions

- 별도 작업 없음. 필요 시 사용자 Play Mode에서 HP·보호막 수치 표시를 확인한다.

### Evidence

- `InGameCombatManager.cs:279`, `280`, `307`이 `Mathf.Round(Mathf.Max/Min(...))`를 직접 호출한다.
- `InGameCombatManager.cs`의 `Round` 메서드와 해당 호출 참조가 삭제됐다.
- 저장소 검색에서 `InGameCombatManager`의 `Round(` 참조는 0건이며 다른 파일의 독립적인 `Mathf.Round` 사용은 유지된다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`가 통과했다.

### History

- 2026-08-03: 사용자가 단일 래퍼인 `Round`를 호출부의 `Mathf.Round`로 대체할 수 있는지 확인했다.
- 2026-08-03: Code Builder가 세 호출부를 직접 치환하고 미사용 `Round` 메서드를 삭제했다.

## Task: 2026-08-03 Remove Persistent Shield Visual Actor Round Trip

### Task title

보호막 종료 시 `EffectManager → BuffSkillActor → EffectManager` 왕복 경로를 제거한다.

### Goals

- 보호막과 상태 효과가 종료되면 `EffectManager`가 상태 키로 연결된 시각 객체를 직접 삭제한다.
- 보호막 시각 효과에 불필요한 `BuffSkillActor.InitializePersistent` 연결을 제거한다.
- 시간형 버프 시각 효과의 `BuffSkillActor.Update → EffectManager.RemoveEffect` 경로는 유지한다.

### Constraints

- `StatusState`의 상태 수명과 보호막 자원 판정은 변경하지 않는다.
- `EffectCreateRequest.PersistentStatus` 기반 시각 객체 매핑은 유지한다.
- 기존 피해, 상태 제거, 스택 소모, 시간 만료 처리 순서는 유지한다.
- Unity Play Mode 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

Implementation complete. 잔여 참조 검사, diff 검사와 솔루션 빌드를 완료했다.

### Next Actions

- Unity Play Mode에서 보호막 소진, 상태 만료, 시간형 버프 시각 효과 삭제를 확인한다.

### Evidence

- `EffectManager.SignalStatusEffectEnded`를 삭제하고 `RemoveEffect(GameObject instance = null, StatusRuntimeInstance status = null)`가 상태 키로 객체를 직접 찾도록 변경했다.
- `InGameCombatManager`의 피해 소진, 상태 제거, 스택 소모와 시간 만료 경로가 `effectManager.RemoveEffect(status: ...)`를 직접 호출한다.
- `ShowStatusEffectVisual`에서 보호막 및 상태 시각 객체에 `BuffSkillActor.InitializePersistent`를 부착하던 경로를 삭제했다.
- `BuffSkillActor`의 `persistentStatus` 필드와 `InitializePersistent`를 삭제하고, 시간형 `InitializeTimed`와 `Complete` 경로는 유지했다.
- `rg`에서 `SignalStatusEffectEnded`, `InitializePersistent`, `BuffSkillActor.Attach(instance)` 참조는 0건이며, `EffectCreateRequest.PersistentStatus` 상태 키 매핑과 시간형 `InitializeTimed` 참조는 유지된다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 `System.Net.Http` 및 `System.IO.Compression` 참조 충돌 경고 2개로 완료했다.
- `git diff --check`가 통과했다.

### History

- 2026-08-03: 사용자가 보호막 종료 시 `EffectManager → BuffSkillActor → EffectManager` 왕복이 불필요한지 지적했다.
- 2026-08-03: Code Builder가 persistent 보호막 시각 경로를 `InGameCombatManager → EffectManager.RemoveEffect`로 단순화하고 시간형 Actor 경로는 보존했다.

## Task: 2026-08-03 Remove SingleSkill Internal Delay Path

### Task title

Remove SingleSkill's unused internal delayed-damage path and reuse the existing `SkillExecution -> SingleSkillExecutor` route for delayed follow-up skills.

### Goals

- Remove `SingleSkillActor` delay coroutines and execution-operation tracking.
- Keep `sein-c` projectile arrival delay as a separate generated `sein-c@arrival` SingleSkill execution.
- Keep `RepeatPerTarget` behavior while running its schedule on the existing SingleSkillExecutor object.
- Reduce `SingleSkillActor` to visual lifetime and one-batch target/result application.

### Constraints

- Preserve projectile `damage_delay_seconds` source data and `ProjectileSkillActor` arrival behavior.
- Do not modify UI files or unrelated user changes.
- Preserve immediate SingleSkill targeting, damage, status, trigger and visual behavior.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. Static residue checks and diff check passed. Full C# build is currently blocked by 3 errors in out-of-scope modified UI files.

### Next Actions

- In Unity Play Mode, verify `sein-c` arrival execution, SingleSkill immediate hitbox/area hits, and `RepeatPerTarget` timing.
- Re-run the full build after the unrelated UI compile errors are resolved.

### Evidence

- `SingleSkillActor.cs` no longer contains `PreparedDamageDelay`, `ApplyNonPrefabTargetsAfterDelay`, `ApplyPrefabHitboxAfterDelay`, `StartTrackedCoroutine`, `TrackOperation`, `TryCompleteExecution`, `BeginPreparedExecution` or `FinishPreparedExecution`.
- `SingleSkillActor.cs` reduced from 737 lines before this task to 543 lines and now keeps visual animation lifetime plus immediate target/result application.
- `SingleSkillExecutor.cs` is now the existing runtime execution component for the `RuntimeSingleExecution` object; it schedules `RepeatPerTarget` and removes its host after delayed repeats finish.
- `sein-c` still uses `ProjectileSkillActor.ExecuteArrivalSkill -> SkillExecution.TryExecuteReaction -> SingleSkillExecutor`; the generated `sein-c@arrival` SingleSkill remains separate from the projectile lifetime.
- Current authored skill CSV search has one positive `damage_delay_seconds` row, `sein-c`, which is a `CooldownProjectile`; no authored SingleSkill row uses the removed internal delay field.
- `rg` found no remaining SingleSkill internal delay or Actor operation-tracking symbols. `git diff --check` passed for all changed combat/loading files.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` reports 3 errors only in modified out-of-scope UI files: `MonsterPanelUI.cs:146`, `DebugUI.cs:665`, and `DebugUI.cs:686`; no SingleSkill or Loading error was reported. Existing Unity reference warnings remain.

### History

- 2026-08-03: Code Builder deleted the unused SingleSkill internal delay contract and moved existing repeat scheduling to the existing SingleSkillExecutor host without adding a script.

## Task: 2026-08-03 Remove SingleSkill Conditional Follow-Up Runtime

### Task title

Remove the obsolete `SingleFollowUpSpec` conditional follow-up path from SingleSkill execution.

### Goals

- Delete `SingleFollowUpSpec` and its target collection, registration, scheduling and delayed execution logic.
- Keep normal `RepeatPerTarget` deployment repetition and Trigger `RepeatCount` reaction repetition.
- Keep SingleSkill damage, status, delay tracking, lifecycle and animation handling unchanged.

### Constraints

- Do not change authored CSV data or the common repeat/reaction rules.
- Preserve delayed target evaluation and `StartTrackedCoroutine` completion tracking.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. Residual reference search, diff check and C# build passed.

### Next Actions

- In Unity Play Mode, verify SingleSkill delay and `RepeatPerTarget` behavior, plus absence of the removed conditional branch behavior.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillActor.cs` no longer declares or references `SingleFollowUpSpec`, `SingleFollowUpTarget`, `FollowUpSpec`, `RegisterFollowUpTarget`, `ScheduleConditionalFollowUps`, `ExecuteConditionalFollowUpAfterDelay` or `HasStatus`.
- `ExecuteAtCenter` retains the runtime-state gate needed to distinguish the primary execution from repeated internal deployments; its misleading `allowConditionalFollowUp` name was changed to `useRuntimeState`.
- `ScheduleRepeatedDeployments` and `ExecuteRepeatedDeploymentAfterDelay` still call `ExecuteAtCenter` with `useRuntimeState: false`, so `RepeatPerTarget` remains on the existing path.
- Repository search found no remaining conditional follow-up symbols.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillActor.cs Pakuri/Assets/Scripts/Combat/Skills/Activation/Single/SingleSkillExecutor.cs` passed.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: Code Builder removed the unused conditional status-based SingleSkill follow-up path while preserving authored repeat and reaction repetition paths.

## Task: 2026-08-03 Apply Line Status Effects Every Tick

### Task title

Remove Line skill's per-target status deduplication so active line-area status effects are attempted on every damage tick.

### Goals

- Call `StatusCombatRules.ApplyStatus` directly for each alive line-hit target on every tick.
- Remove the unused `TryApplyStatus`, `TargetKey` and per-target HashSet state.
- Preserve line damage, knockback and hit-outcome publication.

### Constraints

- Keep status chance, resistance, duration, stack and refresh behavior in `StatusCombatRules.ApplyStatus`.
- Do not change Projectile, Zone or Single status paths.
- Preserve existing user changes in `LineSkillActor.cs`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. Static residue search, diff check and C# build passed.

### Next Actions

- In Unity Play Mode, verify a line-area status is attempted on each tick and that its authored chance/refresh rules still apply.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Activation/Line/LineSkillActor.cs` now calls `StatusCombatRules.ApplyStatus(combatManager, target.Model, statusSpec, sourceModel)` directly after non-lethal damage.
- `TryApplyStatus`, `TargetKey`, `appliedBaseStatusTargets` and its initialization clear were removed; repository search found no remaining references in the Line path.
- `Pakuri/Assets/Scripts/Combat/Status/Execution/StatusRules.cs:20` remains the common owner of enabled, alive, chance, resistance, duration, stack and refresh handling.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/Skills/Activation/Line/LineSkillActor.cs` passed.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` completed with 0 errors and 2 existing assembly-reference warnings.

### History

- 2026-08-03: Code Builder removed Line skill's first-success-per-target status gate and restored direct per-tick status application attempts.

## Task: 2026-08-03 Consolidate Effect Creation Contracts

### Task title

Move runtime effect visual specifications and `EffectCreateRequest` into one `EffectCreate.cs` script.

### Goals

- Keep runtime visual, hitbox and anchor definitions together with the effect creation request contract.
- Remove the duplicate location of `EffectCreateRequest` from `EffectManager.cs`.
- Preserve existing namespaces, public type names, call sites and Unity asset GUID.

### Constraints

- Preserve effect creation behavior and existing skill/status execution paths.
- Do not merge damage, targeting or skill lifetime rules into the generic effect creation contract.
- Keep the existing `EffectManager` and `EffectVisualBuilder` responsibilities unchanged.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. Static reference checks and C# build passed.

### Next Actions

- Let Unity refresh the renamed script asset and confirm there is no import or compile error in the Editor.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Effects/EffectCreate.cs` now contains `RuntimeSkillVisualAnchor`, `RuntimeSkillHitboxSpec`, `RuntimeSkillVisualSpec` and `Pakuri.InGame.EffectCreateRequest`.
- `Pakuri/Assets/Scripts/Combat/Effects/EffectManager.cs` no longer declares `EffectCreateRequest`; it only consumes it.
- `EffectSpec.cs` and its `.meta` were removed; the original GUID `7c7276782e784ec28db67366ac1aeb7f` is preserved in `EffectCreate.cs.meta`.
- C# search found one definition for each consolidated type and no `EffectSpec.cs` or duplicate declaration reference.
- The generated `Pakuri/Assembly-CSharp.csproj` stale include was updated from `EffectSpec.cs` to `EffectCreate.cs` for local compilation; it is ignored/generated and not a source change.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore -v:minimal` completed with 0 errors and 2 existing assembly-reference warnings.
- `git diff --check` passed for the changed source and metadata paths.

### History

- 2026-08-03: Code Builder consolidated EffectSpec definitions and EffectCreateRequest into EffectCreate.cs while preserving namespaces, type names, call sites and Unity GUID.

## Task: 2026-08-01 Preserve Learned Skill Runtime Values During Reset

### Task title

전투 재설정에서 학습 강화 실행값을 보존하고 진행 상태만 초기화한다.

### Goals

- `SkillExecution.ResetRuntimeState`가 쿨다운, 시전, 재장전, 탄창과 진행 문맥만 초기화하게 한다.
- `effective` 실행값은 스테이지 전환 때 기본값으로 덮어쓰지 않는다.
- 새 `SkillExecutionData` 생성 시에만 기존 Resolver로 기본 실행값을 준비한다.

### Constraints

- `SkillExecutionRuleResolver.InitializeRuntimeValues`와 `UnitSkills`의 학습 초기화 경로를 재사용한다.
- 학습 시점에 확정된 재장전, 연사, 탄창, 쿨다운 실행값을 전투 재설정에서 보존한다.
- 기존 공개 API와 스테이지 전환 흐름을 유지한다.
- 커밋하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 정적 호출 검사와 솔루션 빌드를 완료했다.

### Next Actions

- 사용자 Play Mode에서 스테이지 전환 후 학습된 탄창, 재장전, 연사, 쿨다운 값이 유지되는지 확인한다.

### Evidence

- `SkillExecution.ResetRuntimeState`는 `effective` 필드와 `InitializeRuntimeValues`를 더 이상 초기화하지 않고 진행 상태와 `MagazineRemaining`만 초기화한다.
- `SkillExecutionData(UnitCombatState, SkillDefinition)` 생성자에서 `InitializeRuntimeValues(this, null)`을 호출해 기본 실행값을 한 번 준비한다.
- `UnitSkills.InitializeLearnedRuntimeValues`의 Snapshot 적용 호출은 그대로 유지된다.
- `rg` 결과 `InitializeRuntimeValues` 호출은 생성자, 학습 초기화와 Resolver 정의에만 남고 `ResetRuntimeState`에는 없다.
- `git diff --check`가 통과했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-01: 사용자가 전투 재설정에서 기본 실행값을 삭제한 뒤 복구하는 구조의 불필요성과 강화값 손실 위험을 지적했다.
- 2026-08-01: Code Builder가 기본 실행값 계산을 생성자에 남기고 `ResetRuntimeState`를 진행 상태 초기화로 축소했다.

## Task: 2026-08-01 Skill Runtime Dead Code Cleanup

### Task title

Combat Skills의 중복 선언과 참조 없는 실행 보조 코드를 제거한다.

### Goals

- `SkillExecution`에 남은 미사용 `applyingHitEnhancement` 선언을 제거한다.
- `SingleSkillActor`의 참조 없는 시각 수명 상수를 제거한다.
- `ZoneSkillActor`의 실제 적중 강화 재진입 방지 필드는 보존한다.

### Constraints

- 호출 흐름과 공개 API를 변경하지 않는다.
- Unity 메시지 메소드인 `Awake`, `Update`는 엔진 호출이므로 삭제하지 않는다.
- 기존 사용자 변경과 이전 미커밋 작업을 유지한다.
- 커밋하지 않는다.

### Role Owner

Code Builder.

### Status

Complete. 참조 대조, 정적 검사와 솔루션 빌드를 완료했다.

### Next Actions

- 사용자 Play Mode 검증은 기존 작업과 같이 필요할 때 수행한다.

### Evidence

- `SkillExecution.cs`의 `applyingHitEnhancement` 선언은 삭제했고, `ZoneSkillActor.cs:42,487,503,591`의 실제 사용 경로는 보존했다.
- `SingleSkillActor.cs`의 `DefaultVisualLifetimeSeconds`와 `PostDamageLifetimePaddingSeconds`는 저장소 전체 참조가 없어 삭제했다.
- 후보 제거 후 `rg`에는 `ZoneSkillActor.cs`의 재진입 방지 필드와 세 사용 지점만 남았다.
- `git diff --check`가 통과했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-01: Code Builder가 Skills Implementation과 Activation의 비공개 필드 선언 및 참조를 대조했다.
- 2026-08-01: 참조 없는 선언 3개를 제거하고, 동일 이름이지만 실제 재진입 방지에 사용되는 Zone 필드는 보존했다.

## Task: 2026-08-01 Trigger Execution Count Ownership

### Task title

Remove the global triggered execution depth cap and keep reaction repetition explicit.

### Goals

- Remove `MaxTriggeredExecutionDepth` and its shared depth counter.
- Execute each matching reaction once per event chain unless its authored `RepeatCount` requests more executions.
- Carry the reaction execution state through delayed reactions, Actors, damage, status, shield, kill and lifecycle event paths.
- Keep independent `OnOutgoingDamage` events independent so normal multi-hit skills are not capped by a global count.

### Constraints

- Reuse existing `SkillReaction.RepeatCount`, `RepeatIntervalSeconds`, skill repeat values and recast generation values.
- Preserve the public `InGameCombatManager.ApplyDamage` signature.
- Do not add a Skill Implementation script or change authored CSV data.
- Do not commit this task. Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete and uncommitted. Local build and static checks pass.

### Next Actions

- User verifies multi-hit, delayed reaction, repeated reaction and cross-skill lifecycle behavior in Unity Play Mode.
- Review whether any authored reaction intentionally needs the same reaction to re-enter across one persistent Actor lifetime.

### Evidence

- `SkillExecution.cs` no longer contains `MaxTriggeredExecutionDepth` or `triggeredExecutionDepth`.
- `SkillTrigger.TriggerExecutionState` records a reaction by owner and reaction ID and is consumed before source-owned or passive-owned scheduling.
- `SkillReaction.RepeatCount` remains the explicit per-event repeat value used by `SkillExecution.ScheduleReaction`.
- `SkillActionContext`, `SkillExecutionData`, `AttackRule`, `InGameCombatManager`, and existing skill Actors carry the same execution state through delayed and hit-time paths.
- `rg` returned `MAX_DEPTH_REMAINS=0`, `DIRECT_ACTOR_APPLY_DAMAGE_REMAINS=0`, `TRIGGER_STATE_CONSUME_SITES=3`, and `EXPLICIT_REPEAT_DEFINITION_SITES=46`.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing two assembly-reference warnings.
- `git diff --check` passed. Worktree contains nine modified C# files and no commit was created.

### History

- 2026-08-01: User rejected a global eight-level limit because it can change authored skill results and requested explicit per-reaction execution counts.
- 2026-08-01: Code Builder removed the global cap, connected per-chain reaction consumption and preserved existing repeat definitions without committing.

## Task: 2026-08-01 Consecutive Hit Damage Responsibility

### Task title

Separate consecutive-hit state progression from damage multiplier resolution.

### Goals

- Keep runtime target/repeat state progression in `SkillExecution`.
- Keep Node/base-definition/snapshot damage multiplier calculation in `SkillExecutionRuleResolver`.
- Preserve Projectile first-hit, same-target repeat, target-change reset and maximum-bonus behavior.

### Constraints

- Reuse the existing `SkillExecutionData` runtime fields and Projectile Actor path.
- Do not add a script, runtime kind, Executor, Actor base class or duplicate targeting/calculation path.
- Preserve Projectile Definition fallback values when snapshot Node values are absent.
- Keep the unrelated user change in `boards/OPS/AUTOMATION_GUIDE.md` untouched.

### Role Owner

Code Builder.

### Status

Complete. Commit: `085b6fe`.

### Next Actions

- User verifies repeated Projectile hits against the same target in Play Mode.

### Evidence

- `SkillExecution.cs` now exposes `AdvanceConsecutiveHitCount`, which only updates target ID and repeat count.
- `SkillExecutionRuleResolver.cs` now exposes `ResolveConsecutiveHitDamageMultiplier`, which owns the multiplier formula and preserves base Projectile plus snapshot values.
- `ProjectileSkillActor.cs` calls state advancement first and Resolver calculation second.
- Core/Editor builds both ended with `빌드했습니다.` and `git diff --check` passed.

### History

- 2026-08-01: User identified that `ConsecutiveHitDamageMultiplier` mixed runtime state mutation with rule calculation.
- 2026-08-01: Code Builder split the responsibilities and committed the refactor in `085b6fe`.

## Task: 2026-07-31 Common Trigger Skill Recast And Actor Hit Ownership

### Task title

Route conditional skill outcomes through the normal cast pipeline and keep physical damage in Actors.

### Goals

- Route Actor/combat events through `SkillTrigger` gates and back into the same `SkillExecution -> ExecutePrepared -> ExecuteSkill -> family Executor` path used by ordinary casts.
- Replace runtime raw reaction effects with Generation-resolved concrete family Definition links.
- Remove direct damage application from `SkillExecution` and family Executors after Actor caller migration.

### Constraints

- Reuse the existing `TryExecuteSkill`, `ExecutePrepared`, family Definitions, Executors and Actors; add no request class, runtime kind, Executor, Actor base class or Skill Implementation script.
- Preserve CSV schema, authored values, gate asymmetry, dynamic event snapshots, recursion depth, recast generation, targeting, Visual and status behavior.
- Do not turn cooldown, reload or status-duration state commands into fake Actor skills. Convert `RecastZone` to common Zone recast and keep the remaining non-spatial commands as an explicit typed exception.
- Phase 10 must reverify the inspected active-skill 37/non-default proc-count-cooldown 0 baseline before sharing those three gates with source-owned reactions. Preserve all other source/passive gate asymmetry.
- Commit every Phase separately and never activate raw effect and generated Definition execution on the same runtime path.

### Role Owner

Designer for the corrected handoff; Code Builder for Phase 10~15 implementation; Code Reviewer only by explicit user request.

### Status

Phase 10~15 implementation complete. Phase records: `05e5b22`, `22e8516`, `3075a5d`, `55ca337`, `dfa7d53`; runtime cleanup: `5213b14`; reviewer fix: `b7037d1`. Core/Editor builds pass with 0 errors. Unity EditMode is blocked by another Unity instance; Play Mode remains user-owned.

### Next Actions

- User performs Unity Play Mode gameplay verification for normal casts, Trigger reactions, recast generation, delay/repeat, targeting and dynamic damage.
- No further Code Builder runtime phase remains unless gameplay verification finds a code-backed regression.

### Evidence

- Normal/manual casts use `TryExecuteSkill -> ExecutePrepared -> ExecuteSkill`; learned-skill reactions reach `ExecutePrepared`, while raw effects enter `TryExecuteReactionEffect -> ExecuteCastEffect` and directly choose a family Executor.
- `SkillTrigger.cs:383,451` sends accepted reactions to `SkillExecution.ExecuteTriggeredReaction`.
- `SkillExecution.cs:2030,2112` owns direct hit helpers called by Zone, Projectile, Line and Single Actors.
- `BuffSkillExecutor.cs:173` applies charge contact damage directly; status, heal and shield paths are non-hit support behavior.
- Generation currently writes raw effects and typed commands, and Editor tests inspect `Effect`, `TargetSkillId` and `Command` separately.
- `ApplyHitEnhancements` combines OnHit publication, hit count, reload reduction, additional-damage chance and chain period, while current source-owned Trigger gates do not apply the passive count/proc/internal-cooldown sequence.
- Trigger CSV inspection found active-skill reactions 37/non-default proc-count-internal-cooldown 0 and passive reactions 126/non-default 13, so sharing only those three gates has no current authored active-skill behavior change if Phase 10 reproduces the count.
- Phase 10~15 implementation used the existing six Implementation scripts; no new Skill Implementation script, runtime kind, Executor or Actor base class was added.
- Final static checks found zero `TryExecuteReactionEffect`, Trigger-specific outcome helpers, `ApplyResolvedHits`, `ApplyHitEnhancements`, `SkillStatus`, `SingleSkillRules`, or direct `ApplyDamage` calls in `SkillExecution` and family Executors.
- Core and Editor project builds both ended with `빌드했습니다.`; `git diff --check` passed. Unity batchmode was blocked by an already-open Unity instance for this project.

### History

- 2026-07-31: User clarified that conditional skills must be recast through the normal Executor/Actor route after Actor events pass `SkillTrigger`, even if the base cast path needs refactoring.
- 2026-07-31: Designer validated the call graph, documented the common recast migration, and separated actual skill outcomes from non-spatial typed commands.
- 2026-07-31: Code Builder completed Phase 10~14 and committed each phase record, then committed Phase 15 resolved-outcome contract cleanup in `5213b14`.
- 2026-07-31: Code Reviewer ran once, found the missing Recast `MaxGeneration` guard, and Code Builder fixed it in `b7037d1`.

## Task: 2026-07-31 Skill Node Runtime Resolver Consolidation Handoff Correction

### Task title

Correct the Code Builder handoff for Node runtime Resolver consolidation.

### Goals

- Make `SkillExecutionRuleResolver` the sole `GetOperation<T>()` and Node-value calculator without letting it apply runtime damage, status, cooldown, reload, or recast effects.
- Preserve `SkillTrigger` event-gate ownership and route accepted outcomes through `SkillExecution`.
- Record all external `SkillExecutionData` construction/mutation callers, current Trigger asymmetry, dual-application risk, DTO copy invariants, operation flows, and parity tests before implementation.

### Constraints

- Preserve current gameplay and Trigger source-owned/passive-owned gate asymmetry unless the user separately approves a behavior fix.
- Use separate legacy and Resolver snapshots for parity tests; runtime uses one composition path per phase.
- Keep `StatusCombatRules.ApplyStatus` and `InGameCombatManager.ApplyDamage` as common application paths.
- Keep mechanical DTO copy/clone support and prevent post-build collection mutation.
- Do not add Skill Implementation scripts. Reduce each existing multi-class script toward one responsibility class by absorbing only required fields/methods and deleting obsolete classes.
- Integrate responsibilities by rewriting ownership and callers; do not split classes into new same-name files, paste a legacy class body into another script, hide it as a nested class, or keep needless forwarding wrappers.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder for handoff correction and later implementation. Code Reviewer only by explicit user request.

### Status

User concerns 1-7 validated against current code and reflected in the primary handoff. Phase 1 baseline and inventory are recorded. Phase 2 Node composition, Phase 3 runtime-state storage, Phase 4 status/single-rule absorption, Phase 5 execution/Actor Node meaning separation, Phase 6 Trigger gate-only routing, Phase 7 Context/comment cleanup, and the Code Reviewer runtime-state ownership correction are implemented and build-verified.

### Next Actions

- Phase 1 baseline is committed.
- Phase 2 Resolver Node composition is committed and verified by `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` with 0 errors.
- Phase 3 moved per-skill runtime state storage into SkillExecutionData and skill-list ownership into UnitSkills; runtime-state progression is owned by SkillExecution.
- Phase 4 removed SkillStatus and SingleSkillRules after moving status calculation to Resolver and single damage/recovery values to existing owners.
- Phase 5 moved direct Node reads and runtime hit application out of Resolver/Actors into the existing Resolver and SkillExecution responsibilities.
- Phase 6 moved reaction delay/repeat/outcome/command application into SkillExecution while preserving the existing Trigger gate asymmetry and command generation limit.
- Phase 7 removed `SkillExecutionContext` by absorbing its execution values into the existing `SkillActionContext`.
- Every completed Phase is committed separately; final comment normalization and verification are committed.
- Add parity coverage for composition order, cast-time versus hit-time values, status results, Trigger asymmetry, recursion/generation, Definition-only reaction lookup, and snapshot immutability.
- Do not alter Trigger gate asymmetry or command recursion behavior without a separate user decision.
- Keep `SkillExecution.cs` as the only file for its responsibility and reduce its four current classes to the single `SkillExecution` class; do not create `SkillExecutionContext.cs`, `SkillUseState.cs`, or `SkillExecutionState.cs`.
- Replace `ProjectileStatusHitSpec` through existing execution/status contracts and delete it with `SkillStatus.cs`; do not create `ProjectileStatusHitSpec.cs`.
- Final Skill `Implementation` file set is fixed to the six existing scripts named in the primary handoff.
- Delete responsibility-free fields, methods, helpers, and legacy types after caller migration and compatibility checks; final class count and line count must decrease.

### Evidence

- Primary handoff: `boards/COMBAT/SKILL_NODE_RUNTIME_RESOLVER_CONSOLIDATION_HANDOFF.md`.
- Before Phase 2, `SkillExecutionData` constructed source Nodes directly and `MemberwiseClone` handled damage-adjusted copies.
- Phase 1 baseline commit records the full caller inventory and fixed six-file target from the handoff.
- `SkillExecutionData` no longer contains `GetOperation<T>()`, `ApplyNodes`, or Node action handlers; Resolver now owns those operations.
- `SkillExecutionData.CopyWithDamageMultiplier` remains as the mechanical snapshot copy path.
- `rg -n "GetOperation<" Pakuri/Assets/Scripts/Combat/Skills/Implementation --glob "*.cs"` returns only `SkillExecutionRuleResolver.cs`.
- `EnemyCombatDecision.cs:193` and `DamageMeterUIController.cs:255,271` read reactions through the constructor outside Combat runtime orchestration.
- `SkillCatalogRuntimeTests.cs` directly calls the constructor and APIs planned for removal at multiple locations.
- `SkillTrigger.cs:384` executes source-owned reactions after its basic gate; `:446` applies additional passive count/proc/internal-cooldown gates.
- `SkillExecution.cs:89,231,501` limits skill/effect reaction depth; `SkillTrigger.cs:901` separately limits only Zone recast generation for commands.
- Assignment search found no writer for branch set/launch set, skill status tag/chance, or consume-status-stack override fields in `SkillExecutionData`.
- Declaration search found four top-level classes in `SkillExecution.cs` and two in `SkillStatus.cs`; `ProjectileStatusHitSpec` is consumed by the shared status path and four skill families.
- Final static checks found zero legacy symbols, six Implementation `.cs` files, one top-level `SkillExecution` class, and no runtime application calls in `SkillExecutionRuleResolver`.
- Every method under `Pakuri/Assets/Scripts/Combat/Skills` has a concise abstract comment; the mechanical-comment search returned no matches.
- `git diff --check` passed; `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore` passed with 0 errors and 2 existing reference warnings.
- Code Reviewer found runtime lifecycle declarations in `SkillExecutionData`; Code Builder moved reset, tick, cast, hit/launch, burst, cooldown, and reload coordination to `SkillExecution`, exposed the Definition-only Resolver entry points required by Editor tests, and passed Core/Editor builds with 0 errors.
- `rg` declaration search returns no runtime lifecycle method declarations in `SkillExecutionData`; the six-file Implementation set and one-class `SkillExecution` boundary remain unchanged.

### History

- 2026-07-31: User identified incomplete migration surface, responsibility conflicts, Trigger asymmetry, dual application risk, DTO copy scope, incomplete operation inventory, test gaps, and board-state drift.
- 2026-07-31: Code Builder verified all seven concerns as valid, found additional caller and gate details, and corrected the handoff without changing C# runtime behavior.
- 2026-07-31: User added the one-script/one-class preference and required responsibility-based integration instead of physical class-body movement; Designer added the convention, split targets, deletion rules, and acceptance checks.
- 2026-07-31: User corrected that the convention forbids new split files. Designer removed the same-name file plan and fixed the target to six existing Implementation scripts, one `SkillExecution` class, caller migration, legacy type deletion, and reduced class/line counts.
- 2026-07-31: Code Builder resumed under the per-Phase commit rule and closed the baseline/inventory phase before runtime edits.
- 2026-07-31: Phase 2 moved Node extraction and Node action value composition to Resolver; Assembly-CSharp build passed with 0 errors.
- 2026-07-31: Phase 3 moved per-skill runtime state into SkillExecutionData, skill-list ownership into UnitSkills, removed the two legacy state types from C# callers, and passed Assembly-CSharp build with 0 errors.
- 2026-07-31: Code Reviewer found that SkillExecutionData still owned runtime lifecycle behavior; Code Builder moved that behavior into SkillExecution, migrated all callers, exposed the Definition-only Resolver entry points, and passed Core/Editor builds with 0 errors and 2 existing reference warnings each.
- 2026-07-31: Phase 4 moved status calculation to SkillExecutionRuleResolver, reused StatusApplicationSpec for resolved status values, deleted SkillStatus and SingleSkillRules with their meta files, and passed Assembly-CSharp build with 0 errors.
- 2026-07-31: Phase 5 moved cast/repeat/core/status/refund value resolution to SkillExecutionRuleResolver, moved shared damage/status/trigger/reload application to SkillExecution, unified family hit multipliers, and passed Assembly-CSharp build with 0 errors.
- 2026-07-31: Phase 6 left Trigger as the gate owner and routed accepted reactions to SkillExecution for delay, repeat, outcome, command, targeting, and runtime application; Assembly-CSharp build passed with 0 errors.
- 2026-07-31: Phase 7 absorbed SkillExecutionContext into SkillActionContext, completed abstract comments across Combat/Skills, passed final static checks and Assembly-CSharp build, and committed the result.

## Task: 2026-07-28 Skill Trigger / Node Unification Design

### Task title

Design Trigger as the sole activation authority and ordered Nodes as the sole payload authority.

### Goals

- Remove `graph_kind`, Effect runtime ownership, and the removed intermediate terminology after a behavior-preserving migration.
- Route former Effect timing and payload through Trigger-owned Nodes.
- Keep `SkillNode` as one compiled operation container.

### Constraints

- Role Owner is Designer for the handoff and Code Builder refactoring track for later implementation.
- Preserve current damage, status, shield, timing, targeting, delay, repeat, visual, recast, Choice, Passive, and Trigger behavior.
- Do not delete the legacy Effect path before migrated family parity exists.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Superseded by the 2026-07-29 Trigger Executor Reuse Design.

### Next Actions

- No action. The preserved historical handoff is under `boards/ARCHIVE/`.
- Current work follows `boards/COMBAT/SKILL_TRIGGER_EXECUTOR_REUSE_HANDOFF.md`.

### Evidence

- `SkillEffectDefinition` and `SkillTriggerDefinition` both own timing/conditions/target/payload axes.
- Current Trigger events do not cover all current Effect timings.
- Active authoring contains 508 Effect graph rows and 256 ordinary modifier graph rows.
- Fifteen current C# files reference `SkillEffectDefinition`.
- The superseded full design is preserved at `boards/ARCHIVE/SKILL_TRIGGER_NODE_UNIFICATION_HANDOFF_2026-07-28.md`.

### History

- 2026-07-28: User selected Trigger-to-Node as the unified execution direction and removed `graph_kind` plus the rejected intermediate terminology.
- 2026-07-28: Designer created the implementation handoff without changing runtime code or CSV.
- 2026-07-28: Code Builder archived older COMBAT task history and retained this as the only active COMBAT task.
- 2026-07-29: User superseded direct Trigger Node dispatch with existing family Executor reuse.

## Task: 2026-07-29 Trigger Visual Duration Restoration

### Task title

Restore the pre-migration lifetime of standalone Trigger visuals.

### Goals

- Add one positive `SetDuration` Node to each of the ten standalone Trigger visual owners that lost its lifetime during Node migration.
- Preserve the prior one-second visual lifetime without adding a runtime fallback.

### Constraints

- Runtime and validator code remain unchanged.
- Damage, targeting, status payload, visual assets, Trigger gates, and existing Node order remain unchanged.
- `Pakuri/reference/2.Monster` remains the gameplay-intent source; the pre-migration runtime is the exact lifetime source.

### Role Owner

Code Builder

### Status

CSV implementation and non-Play-Mode verification complete. User Play Mode verification remains.

### Next Actions

- User verifies that representative Trigger visuals disappear after one second and that damage still occurs exactly once.

### Evidence

- Pre-migration `SkillTrigger`, `ZoneSkillExecutor`, and `SingleSkillExecutor` assigned transient additional-damage visuals a `1f` lifetime.
- The relevant monster references define these ten payloads as instantaneous explosions, reflections, follow-ups, or slashes and do not define a persistent visual duration.
- Ten Trigger owners now each contain exactly one `SetDuration=1` Node.
- All sixteen Trigger owners with `ShowVisual` now have a positive duration or status-owned lifetime; standalone non-positive duration count is zero.
- Unity CSV source validation passed and loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Unity Console reported zero errors; `dotnet build Pakuri/Pakuri.sln --no-restore` completed with zero errors.

### History

- 2026-07-29: User rejected a runtime zero-duration fallback and directed data-only restoration from the monster references and prior behavior.
- 2026-07-29: Code Builder restored `SetDuration=1` for ten affected Trigger owners.

## Task: 2026-07-29 Eve-E Recast Generation Regression Diagnosis

### Task title

Diagnose the Eve E zone that continues displaying and dealing damage after its intended lifetime.

### Goals

- Distinguish the actual `eve-e-master-1` OnExpire Trigger from the non-Trigger `eve-e-master-2` Choice modifiers.
- Identify why an Eve E recast zone can repeat without reaching its authored generation limit.

### Constraints

- Preserve all authored duration, generation, Trigger, Choice, damage, status, visual, prefab, scene, and generated-catalog values.
- Keep non-lifecycle Trigger events at their existing zero recast generation.
- Every conclusion must follow from current authoring and runtime code.
- Unity Play Mode reproduction remains user-owned.

### Role Owner

Code Builder

### Status

Runtime correction and local non-Play-Mode verification complete. User Play Mode verification remains.

### Next Actions

- User verifies that `eve-e-master-1` creates only one three-second recast and that `eve-e-master-2` creates no recast.

### Evidence

- `eve-e-master-2` owns only `StatusMaxStacksBonus` and `StatusCriticalDamageTakenBonus` Choice Nodes; no Trigger or visual Node with that ID exists.
- `eve-e-master-1` owns the `OnExpire` Trigger and `RecastZone` Node with `max_generation=1`.
- `ZoneSkillActor` publishes OnExpire with its current `recastGeneration`.
- Before the correction, `SkillTrigger.PublishLifecycleEvent` converted the lifecycle context to `TriggerExecutionContext`, which had no recast-generation field.
- Before the correction, `SkillTrigger.TryExecuteOwnedNodes` constructed a new `SkillExecutionContext` without a recast-generation argument, resetting it to zero.
- Before the correction, `ZoneSkillExecutor.ExecuteRecast` therefore saw zero on every expiration and passed the generation guard repeatedly.
- The pre-legacy-deletion path passed `context.RecastGeneration` directly into the next recast actor.
- `TriggerExecutionContext` now stores a non-negative `RecastGeneration`; lifecycle publication copies it from `SkillExecutionContext`, and Trigger-owned Node execution copies it into the next `SkillExecutionContext`.
- Repository-wide authoring inspection found one `RecastZone`: `eve-e-master-1`, with one matching `OnExpire` Trigger, duration 3, and `max_generation=1`; static validation reported zero errors.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with zero errors and the two pre-existing assembly-version warnings.
- Unity script validation, forced compilation, domain reload, and Console inspection completed with zero diagnostics.

### History

- 2026-07-29: User reported that the Eve E master effect remained indefinitely and continued applying damage.
- 2026-07-29: Designer found an ID mismatch in current authoring and a recast-generation loss introduced in the Trigger-to-Node execution path.
- 2026-07-29: User confirmed `eve-e-master-1` and authorized Code Builder correction plus cross-skill verification.
- 2026-07-29: Code Builder restored recast-generation propagation in the common Trigger execution path and verified every authored `RecastZone`.

## Task: 2026-07-29 Skill Runtime Responsibility Comments

### Task title

Document method-level behavior and core responsibility boundaries in the shared skill runtime.

### Goals

- Add Korean method-level comments to previously undocumented constructors, snapshot helpers, conditional rule resolvers, and runtime Node execution helpers.
- Clarify the top-level responsibility of the Definition, execution routing, execution snapshot, targeting, and Node dispatch files.
- Correct the stale `SkillExecutionRuleResolver` header so it describes its actual conditional runtime-rule responsibility.

### Constraints

- Limit changes to comments in the eight user-specified C# files.
- Preserve every type, method signature, field, operation, condition, execution order, and player-facing behavior.
- Do not change CSV, catalog, prefab, scene, asset, or runtime data contracts.

### Role Owner

Code Builder

### Status

Comment implementation and non-Play-Mode verification complete.

### Next Actions

- User may review the new responsibility comments while implementing future Base, Enhancement, Master, Passive, and Trigger skills.

### Evidence

- Method-comment coverage scan reported zero undocumented method or constructor declarations in all eight target files.
- Removing comments and whitespace from each changed file produced code text identical to its `HEAD` version (`ALL_CODE_EQUAL=True`).
- `git diff --check` completed without whitespace errors.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with zero errors and the two existing assembly-version conflict warnings.
- Unity script refresh and domain reload completed; editor state returned idle/ready and Console contained zero errors.

### History

- 2026-07-29: User requested method-level and core-responsibility comments across the shared skill runtime.
- 2026-07-29: Code Builder added comment-only documentation to the eight specified files and verified that executable code remained unchanged.

## Task: 2026-07-29 Skill Compilation Placement Refactor

### Task title

Move post-catalog skill compilation out of the Loading pipeline.

### Goals

- Move `SkillDefinitionCompiler` to `Combat/Skills/Compilation`.
- Split `SkillNodeMapper` and `SkillChoiceCompiler` into their own files.
- Preserve every compilation and runtime Node behavior.

### Constraints

- File organization and ownership only; no skill, Trigger, Node, status, damage, timing, or gameplay behavior changes.
- Preserve `Pakuri.InGame` namespaces and all public method signatures.
- Preserve the existing compiler script `.meta` GUID.
- Do not modify the user-owned comment changes already present in the eight combat runtime files.

### Role Owner

Code Builder

### Status

Implementation and non-Play-Mode verification complete.

### Next Actions

- User verifies representative compiled skills in Unity Play Mode.

### Evidence

- `SkillDefinitionCompiler` is not called by `GameDataLoader.LoadAndValidateRuntimeCatalog`.
- Inspected compiler consumers are combat `SkillExecution`, spawn state construction, and UI/run learned-skill application.
- The current source file contains three separate classes: `SkillDefinitionCompiler`, `SkillNodeMapper`, and `SkillChoiceCompiler`.
- `SkillDefinitionCompiler`, `SkillNodeMapper`, and `SkillChoiceCompiler` now reside in separate files under `Combat/Skills/Compilation`.
- The compiler script retains its original Unity GUID and the extracted files have generated `.meta` files.
- Existing namespaces and method signatures compile through `Assembly-CSharp.csproj` and Unity with zero errors.

### History

- 2026-07-29: User approved the four-stage Loading structure and Code Builder implementation.
- 2026-07-29: Code Builder recorded this separate combat-boundary task before moving compilation code.
- 2026-07-29: Code Builder moved and split the compilation classes without modifying the eight pre-existing user-owned combat runtime files.

## Task: 2026-07-29 Final Skill Catalog Direct-Use Design

### Task title

Generate final typed skill data once and use it directly from `GameDataCatalog`.

### Goals

- Remove runtime Source-to-Definition and Node compilation.
- Remove authored Node and Trigger string parsing from runtime execution.
- Remove the three `Combat/Skills/Compilation` scripts.
- Reorganize all current `Combat/Skills` scripts into Definition, Runtime, Execution, Delivery, and Reaction responsibilities.

### Constraints

- Preserve every current combat behavior, CSV value, ID, order, and asset reference.
- Keep one semantic validation call and one final catalog build.
- Keep per-cast `SkillExecutionData` and per-unit `SkillUseState`.
- Preserve moved script `.meta` GUIDs.
- Designer changes documentation only.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder implementation and available non-Play-Mode verification complete. Phases 1-6 complete.

### Next Actions

- User verifies representative active, passive, enhancement, master, Trigger, and enemy skill behavior in Unity Play Mode.

### Evidence

- Current `Combat/Skills` contains 27 C# scripts and 15,387 lines.
- `SkillExecution.RebuildLearnedSkillState` calls `CompileActive` and `CompilePassive`.
- `SkillNodeMapper.GetChoiceRuntimeNodes` performs first-use Choice Node mapping and caching.
- `SkillNodeExecutor` reparses authored scope, merge policy, condition, status-list, and runtime-kind strings during execution.
- `SkillTrigger` splits authored Choice, attribute, and event-skill lists during trigger checks and compares event source scope as a string.
- Current graph authoring has two Choice owners targeting more than one skill.
- Full current tree, all script responsibilities, final 24-script tree, data contracts, migration, risks, and verification are recorded in the handoff.
- Phase 1 started from clean commit `565eed5`; current `Combat/Skills` contains 27 C# files and 15,387 lines.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` baseline builds completed with zero errors.
- Unity CSV validation loaded 5 monsters, 8 stage-one enemies, and 8 stage-two enemies; the EditMode test job succeeded.
- Phase 2 final types now store typed status scope/policy, status conditions, status lists, RuntimeKind filters, Trigger lists/scope, Choice Nodes, and Node target skill IDs.
- `SkillNodeExecutor` contains zero authored `StatusRuntimeCompiler.Parse*` calls; the runtime/editor assembly builds with zero errors.
- Phase 3 Monster, Enemy, and RuntimeCatalog storage uses final Definition/Choice types; Combat state rebuild stores those same references without compiling.
- Generation builds status definitions first and creates status runtime payloads without re-entering `GameDataLoader.CurrentCatalog`.
- Unity CSV validation loaded 5/8/8 definitions after final catalog generation.
- Phase 4 removed `SkillChoice.Source`, Choice lazy Node mapping/cache, and runtime Trigger authored-string parsing.
- Runtime Execution/Trigger/StatusRules search found zero `Split`, `Enum.Parse`, or `TryParse` calls.
- Runtime and Editor builds completed with zero errors; Unity CSV validation retained 5 monsters and 8/8 enemies.
- Phase 5 removed all three compiler/mapper symbols and `CompileTriggers`; Generation owns the integrated Builder logic.
- `Combat/Skills` now contains the specified 24 scripts under Definitions, Runtime, Execution, Delivery, and Reactions.
- All 18 moved script GUID pairs matched; runtime/editor builds and Unity CSV validation passed.
- Phase 6 removed all Source/Definition duplicate contracts, `NormalizedNodes`, and raw final Node/Trigger/status authored-string fields.
- Removed-symbol, runtime parsing, and Generation-outside Definition-mutation searches all returned zero.
- EditMode target-filter/reference-reuse tests passed 2/2; solution build and Unity script compilation completed with zero errors; CSV validation retained 5/8/8.
- `Combat/Skills` changed from 27 scripts/15,387 lines to 24 scripts/12,102 lines: net reduction 3 scripts and 3,285 lines.

### History

- 2026-07-29: User chose final authored-data direct use and requested a Code Builder-ready markdown plus the complete `Combat/Skills` structure.
- 2026-07-29: Designer created the implementation handoff from inspected current code and data.
- 2026-07-29: Designer extended the handoff so Generation produces final typed Node and Trigger conditions and runtime code only compares or executes them.
- 2026-07-29: Code Builder completed Phase 1 baseline protection and recorded the live code, GUID, build, Unity, and CSV evidence.
- 2026-07-29: Code Builder completed Phase 2 final typed contracts while retaining the old compiler path as a buildable bridge.
- 2026-07-29: Code Builder completed Phase 3 final catalog generation and direct final-type indexing.
- 2026-07-29: Code Builder completed Phase 4 final Choice/Trigger/Status direct consumption and removed 239 net C# lines.
- 2026-07-29: Code Builder completed Phase 5 compiler deletion and responsibility-folder migration.
- 2026-07-29: Code Builder completed Phase 6 dead-contract deletion and full non-Play-Mode regression verification.

## Task: 2026-07-29 Trigger Executor Reuse Design

### Task title

Keep Trigger as the activation gate and route skill outcomes through existing family Executors.

### Goals

- Reduce `SkillTrigger` to event, condition, cooldown/count, delay/repeat, and delegation.
- Route Trigger delivery through `SkillExecution.TryExecuteTriggered` and existing family Executors.
- Delete `SkillNodeExecutor.cs` without adding a replacement script.
- Keep non-skill cooldown, reload, and status-duration commands on existing runtime APIs.

### Constraints

- Preserve current Trigger IDs, conditions, ordering, event snapshots, dynamic values, recursion limits, and player-facing behavior.
- Do not duplicate Trigger condition checks in `SkillExecution` or family Executors.
- Designer changes documentation only.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer / Code Builder refactoring handoff

### Status

Code Builder Phase 1-6 complete. Non-Play-Mode verification passed; user Play Mode confirmation remains.

### Next Actions

- User verifies representative Trigger families, commands, delay/repeat/recast, and dynamic damage in Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- `SkillNodeExecutor.Execute` and `HasRuntimeActions` are called only by `SkillTrigger.cs`.
- Current Trigger authoring contains 158 owners and 606 Nodes: 4 existing-skill calls, 51 direct delivery results, 1 recast, 21 state commands, and 81 modifier-only owners.
- `SkillExecution.ExecuteSkill` already dispatches every concrete skill family to the existing Executor.
- `InGameCombatManager` publishes shield, status, kill, damage, and combat-start events outside the family Executor boundary.
- Full contracts, migration phases, edge cases, acceptance criteria, and verification are recorded in the handoff.
- Phase 1 confirmed 158 Triggers, 606 owned Nodes, 77 action owners, 81 no-action owners, 24 Combat/Skills scripts, and 12,102 lines.
- Runtime and Editor builds completed with zero errors before implementation.
- Phase 2 generated 55 final Definitions, 22 typed commands, and 81 inactive owners.
- Focused Unity EditMode catalog test passed 1/1; runtime/editor builds remained error 0.
- Phase 3 routes all 55 final Definitions through existing family Executors.
- `BuffSkillExecutor` now uses `StatusCombatRules.ApplyStatus`; lifecycle and source snapshot policies are explicit.
- Runtime/editor builds completed with error 0 and `SkillCatalogRuntimeTests` passed 3/3.
- Phase 4 routes 1 recast, 14 cooldown refunds, 6 reload reductions, and 1 status-duration extension through existing APIs.
- `SkillTriggerDefinition.Nodes` and `SkillTrigger` legacy Node execution are removed; `SkillCatalogRuntimeTests` remains 3/3 and Unity Console reports zero errors/warnings.
- Phase 5 deletes `SkillNodeExecutor.cs/.meta`, Trigger-only public operation types, and their legacy mapping; active C# search returns zero deleted symbols.
- Runtime/editor builds remain error 0 and Unity `SkillCatalogRuntimeTests` passes 3/3 after deletion.
- Final static search returns zero deleted symbols, Trigger runtime Node payloads, and authored-string parsing in runtime consumers.
- Solution build error 0, Unity Console error/warning 0, full EditMode 3/3, CSV catalog 5/8/8.
- `Combat/Skills` is 23 C# scripts; Git diff is 579 additions / 1,547 deletions, net -968 lines.
- Whole production `Assets/Scripts` Git diff is 1,299 additions / 1,742 deletions, net -443 lines.

### History

- 2026-07-29: User selected condition-only Trigger orchestration, existing Executor reuse, and `SkillNodeExecutor` deletion.
- 2026-07-29: Designer replaced the obsolete direct Node-dispatch design with the executor-reuse handoff.
- 2026-07-29: User approved implementation; Code Builder completed the Phase 1 behavior and build baseline.
- 2026-07-29: Code Builder completed Phase 2 final Trigger outcome Generation and focused catalog verification.
- 2026-07-29: Code Builder completed Phase 3 shared family execution, status-rule parity, EventTarget filtering, and dynamic event-value snapshots.
- 2026-07-29: Code Builder completed Phase 4 typed command execution and removed Trigger runtime Node payload consumption.
- 2026-07-29: Code Builder completed Phase 5 legacy executor, operation, mapping, and stale context cleanup.
- 2026-07-29: Code Builder completed Phase 6 static/build/Unity/CSV verification and recorded final net deletion.

## Task: 2026-07-29 Passive Trigger State Consolidation

### Task title

Delete `PassiveSkill.cs` and keep its live Trigger gate state with `SkillTrigger`.

### Goals

- Move internal cooldown and N-count state to the Trigger condition owner.
- Delete `PassiveSkill.cs` and its Unity `.meta`.
- Remove the empty passive change-notification pipeline from `InGameCombatManager`.

### Constraints

- Preserve Trigger cooldown timing, count reset behavior, case-insensitive keys, and per-combat-manager state isolation.
- Do not add a replacement script, interface, factory, or gameplay behavior.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies `eve-g`, `rin-h`, `sein-g`, and `vega-i` cooldown/count behavior in Play Mode.

### Evidence

- `PassiveSkill.cs` contained two dictionaries, `Reset`, `ConsumeTriggerCooldown`, `ConsumeTriggerCount`, and six empty notification/flush methods.
- Only `SkillTrigger.cs` called the two live consume methods.
- `SkillTrigger.cs` now owns the same case-insensitive cooldown/count dictionaries in manager-keyed `TriggerGateState`.
- `InGameCombatManager.Awake` and `ResetCombatState` reset that manager's Trigger state.
- Active Combat C# search returns zero `PassiveSkill`, `PassiveEffects`, empty notification, or pending-flush references.
- `PassiveSkill.cs` and `PassiveSkill.cs.meta` are deleted; no replacement script was added.
- `git diff --check` passed.
- `Pakuri/Pakuri.sln` builds with zero errors and the two existing assembly-reference warnings.
- Unity Console reports zero errors/warnings after script refresh.
- Unity EditMode `SkillCatalogRuntimeTests` passes 4/4 and loads the catalog at 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- The implementation diff is 69 additions and 122 deletions: net reduction 53 lines.

### History

- 2026-07-29: User approved Code Builder consolidation and deletion of `PassiveSkill.cs`.
- 2026-07-29: Code Builder moved live Trigger gate state to `SkillTrigger`, removed empty manager callbacks, deleted the script/meta, and completed local verification.

## Task: 2026-07-29 Status Runtime Responsibility Refactor

### Task title

Remove runtime status compilation and separate status definition, execution, runtime, and skill-integration responsibilities.

### Goals

- Delete the `StatusRuntimeCompiler` runtime/compiler responsibility.
- Generate final `StatusRuntimeData` once in Loading Generation.
- Make Combat retrieve final status data from `GameDataCatalog`.
- Move `SkillStatus` to `Combat/Skills/Execution`.
- Organize `Combat/Status` into `Definitions`, `Execution`, and `Runtime`.

### Constraints

- Preserve current status application, stack, duration, modifier, display, and skill behavior.
- Preserve moved Unity script `.meta` GUIDs.
- Keep authored status string parsing inside Loading.
- Do not add a replacement compiler, factory, or interface.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative buff, debuff, shield, stack, duration, and status display behavior in Unity Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- Baseline worktree was clean at commit `772711c`; `Pakuri/Pakuri.sln` built with zero errors and two existing assembly-reference warnings.
- `StatusRuntimeCompiler.cs` and the `StatusRuntimeCompiler` symbol are removed.
- `Combat/Status` now contains `Definitions/StatusEffectDefinition.cs`, `Execution/StatusRules.cs`, and `Runtime/StatusState.cs`.
- `SkillStatus.cs` now resides under `Combat/Skills/Execution`; all inspected callers remain Skill Delivery executors.
- All four moved script `.meta` GUID comparisons returned `match=True`.
- Combat status and SkillStatus searches returned zero authored `Enum.Parse`, `Enum.TryParse`, or `Split` calls.
- Removed-symbol searches returned zero `StatusRuntimeCompiler` and `StatusEffectLookup` references.
- `Pakuri/Pakuri.sln` builds with zero errors; Unity Console reports zero errors/warnings after the final refresh.
- `SkillCatalogRuntimeTests` passes 4/4, including final status-data generation and RuntimeCatalog reference reuse.
- Production C# diff is 465 additions / 526 deletions: net reduction 61 lines. Tests add 18 lines, making the total C# diff net reduction 43 lines.

### History

- 2026-07-29: User approved Code Builder implementation of the status compiler removal and responsibility split.
- 2026-07-29: Code Builder moved parsing to Loading, final data generation to Generation, and lookup to RuntimeCatalog.
- 2026-07-29: Code Builder separated Status folders, moved SkillStatus to Skills/Execution, and completed static/build/Unity/EditMode verification.

## Task: 2026-07-29 Field Unit Registry Ownership Consolidation

### Task title

Make `UnitSpawnManager` the sole owner of field-unit registration and removal.

### Goals

- Keep `CombatUnitRegistry` as a hidden helper owned by `UnitSpawnManager`.
- Route selected monsters, manifested monsters, enemies, and Nexus through one field-unit manager.
- Replace external Registry access with read/query APIs on `UnitSpawnManager`.
- Remove the separate selected-player GameObject and model cache.

### Constraints

- Preserve spawning, restoration, targeting, collision lookup, AI, skills, UI display, death, and Nexus behavior.
- Keep `CombatUnitRegistry.cs` and `CombatUnitEntry`.
- Do not change CSV, scene, prefab, catalog, saved-run, or gameplay data.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies selected-monster spawn, manifestation, enemy waves, targeting, death, Nexus contact, and next-day party restoration in Play Mode.
- Run Code Reviewer only after separate explicit user approval.

### Evidence

- `CombatUnitRegistry` is now an internal sealed helper instantiated only by `UnitSpawnManager`.
- Active C# search finds `CombatUnitRegistry` only in its definition file and the private `UnitSpawnManager.unitRegistry` field.
- All Registry `Register` and `Unregister` calls now exist only inside `UnitSpawnManager`.
- External combat, skill, AI, GameFlow, and UI consumers receive or query `UnitSpawnManager`.
- `spawnedPlayerUnit`, `SpawnedPlayerModel`, `InGameCombatManager.UnitRegistry`, and the former CombatManager register/despawn APIs have zero active C# references.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` build with zero errors and the two existing assembly-reference warnings.
- Unity script refresh returned idle, Console contained zero errors, and focused script validation reported zero errors.
- `git diff --check` passed.

### History

- 2026-07-29: User clarified that all current field monsters, manifested monsters, and enemies should be managed through one owner.
- 2026-07-29: User selected Code Builder and approved `UnitSpawnManager` ownership with `CombatUnitRegistry` retained as a hidden helper.
- 2026-07-29: Code Builder moved Registry ownership and mutation to `UnitSpawnManager`, migrated external consumers, removed duplicate selected-player state, and completed local verification.

## Task: 2026-07-29 UnitSkills Single Runtime Source

### Task title

Use `UnitSkills` as the sole learned-skill and Choice ownership source.

### Goals

- Keep active, passive, enhancement, and master ownership in `UnitSkills`.
- Remove session-to-runtime ID copying.
- Keep `SkillExecutionState` as the separate transient cooldown, cast, magazine, reload, and execution-list state.

### Constraints

- Preserve full learned-skill execution-state rebuilds after post-combat learning and on spawn/restoration.
- Preserve existing skill, passive, Choice, Trigger, targeting, and delivery behavior.
- Do not introduce incremental synchronization, a replacement runtime class, or a new production script.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies newly learned active/passive skills and Choice effects in the next Play Mode combat.

### Evidence

- `UnitSkills.AddChoice` now classifies and stores confirmed Choice IDs.
- `UnitCombatStateFactory` and player restoration assign the exact `RunMonsterState.Skills` instance instead of copying collections.
- Production `AddActiveSkill`, `AddPassiveSkill`, and `AddChoice` calls exist only in `RunSession`.
- Removed copy-symbol search returned zero active production references.
- Runtime and Editor builds completed with zero errors; all five `SkillCatalogRuntimeTests` passed.
- Unity script compilation returned ready and the post-compile Console contained zero errors or warnings.

### History

- 2026-07-29: User confirmed that learning occurs only during the post-combat reward stage and selected full execution-state rebuilds over incremental synchronization.
- 2026-07-29: Code Builder retained `SkillExecutionState` behavior and consolidated persistent learned-skill ownership into `UnitSkills`.

## Task: 2026-07-30 Unified Collider Collision Resolution

### Task title

Route all Collider-based skill and Nexus contact hits through one resolver.

### Goals

- Use one collision-result path for Projectile, Line, Zone prefab hitboxes, Single prefab hitboxes, Charge, and enemy-to-Nexus contact.
- Map every raw physics Collider through `UnitSpawnManager.FindByCollider()`.
- Use actual target Colliders only, with no Transform-position collision fallback.
- Give Line a real runtime `BoxCollider2D`.

### Constraints

- Preserve caller-owned targeting filters, target order, damage, status, follow-up, pierce, and tick behavior.
- Keep direct-target, chain, and radius targeting separate because they are not Collider collision delivery.
- Preserve the moved Unity script `.meta` GUID.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies Projectile pierce/impact, visual and no-visual Line, Zone/Single prefab hitboxes, Charge contact, and enemy-to-Nexus contact in Play Mode.
- Confirm that every collision-participating unit prefab has an enabled `Collider2D`; missing Colliders now intentionally produce no hit.

### Evidence

- `UnitHitboxOverlap.cs` was renamed to `UnitCollisionResolver.cs`; GUID `4280db46ec0042e69ea67a44c8b10498` is preserved.
- `UnitCollisionResolver.CollectTargets()` is the only production caller of `UnitSpawnManager.FindByCollider()`.
- Projectile, Line, Zone prefab hitbox, Single prefab/core hitbox, Charge, and Nexus contact all call `UnitCollisionResolver.CollectTargets()`.
- Moving Projectile and Charge use the same resolver with `Collider2D.Cast`; static shapes use `Collider2D.Overlap`.
- `LineSkillActor` creates and sizes a real runtime `BoxCollider2D`; no-visual Line also creates an empty Actor through `EffectManager`.
- `CombatUnitEntry.GetHitboxColliders()`, `ResolveTargetPoint()`, Line point fallback, Charge distance fallback, Projectile `OnTriggerEnter2D`, Line `Physics2D.OverlapBox`, Nexus `OverlapPoint`, and Nexus distance fallback are removed.
- Active production C# search returns zero `UnitHitboxOverlap`, `OnTriggerEnter2D`, `Physics2D.OverlapBox`, `OverlapPoint`, or `Collider2D.Distance(...).isOverlapped` collision paths.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` build with zero errors and the two existing assembly-reference warnings.
- `git diff --check` passes.
- Unity EditMode tests pass 6/6; `CollisionResolverUsesOverlapAndMovementCast` verifies both overlap and swept movement detection.
- Final Unity script refresh is idle and Console reports zero errors.

### History

- 2026-07-29: User required all collision-required skills to use one route and prohibited Transform-position exceptions without approval.
- 2026-07-30: Code Builder introduced `UnitCollisionResolver`, migrated all inspected Collider delivery paths, removed Transform fallbacks, and completed build, static, Unity, and EditMode verification.

## Task: 2026-07-30 Enemy Passive Shared Learning Runtime

### Task title

Move Enemy passives into the shared learned-skill runtime and remove Enemy-only combat branches.

### Goals

- Make Enemy spawn initialize assigned active and passive IDs through `UnitSkills`.
- Build Enemy `SkillState` through the same learned-skill reconstruction used by Monster.
- Resolve passive damage, defense, critical, healing, and incoming-damage modifiers through `SkillExecutionState`.
- Delete `EnemyPassiveModifiers` and all Enemy-only passive state and combat calculation branches.

### Constraints

- Preserve the six existing Enemy passive modifier behaviors and authored values.
- Preserve Monster reward learning, Choice, and Trigger behavior.
- Preserve the user’s existing uncommitted roster and collision refactors in overlapping files.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder

### Status

Implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative DamageUp, DefenseUp, HealingUp, and IncomingDamageDown enemies in Play Mode.

### Evidence

- `UnitCombatStateFactory.CreateEnemy` records assigned active and passive IDs in the common `UnitSkills` storage.
- `SkillExecution.RebuildLearnedSkillState` now accepts resolved active and passive definitions for both Monster and Enemy.
- `SkillExecutionState` supplies all six passive modifier results to common damage and healing calculations.
- Passive Trigger dispatch reads each learned passive’s runtime definition instead of loading `MonsterDefinition`.
- `EnemyPassiveModifiers.cs`, its `.meta`, the empty `Enemy/Passive` folder, and all eight Enemy-only passive fields were removed.
- Static searches return zero removed Enemy passive types, fields, `RebuildAssignedSkillState`, Monster-only passive role gates, or Enemy branches in damage/healing.
- Runtime and Editor C# builds complete with zero errors and the two existing assembly-reference warnings.
- Unity catalog loading retains 5 monsters, 8 stage-one enemies, and 8 stage-two enemies.
- Full Unity EditMode tests pass 9/9, including shared Enemy spawn, all six modifier kinds, and all 16 catalog Enemy passives.
- Final Unity script refresh is idle and Console contains no project compile errors.

### History

- 2026-07-30: User rejected the separate Enemy passive runtime and required Enemy spawn to use the Monster learned-skill path.
- 2026-07-30: User explicitly required deletion of `EnemyPassiveModifiers`, Enemy-only multiplier fields, and Enemy branches in damage and healing.
- 2026-07-30: Code Builder completed the shared learned-passive migration and non-Play-Mode verification.

## Task: 2026-07-30 Skill Definition Family Consolidation

### Task title

Consolidate skill Definitions and execution logic by real delivery family.

### Goals

- Keep one concrete Single Definition and one concrete Buff Definition.
- Convert Chain to Trigger-based common Single execution.
- Move Charge initiation to the Buff family.
- Split Definitions into family folders after behavior consolidation.
- Delete duplicate type dispatch, executor branches, and dead fields.

### Constraints

- Follow `boards/COMBAT/SKILL_DEFINITION_FAMILY_CONSOLIDATION_HANDOFF.md`.
- Preserve current CSV values, IDs, assets, Trigger behavior, and gameplay behavior.
- Do not create rejected standalone contract scripts or compatibility-shell subclasses.
- Preserve existing XML-comment-tag cleanup.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Code implementation and non-Play-Mode verification complete.

### Next Actions

- User Play Mode verification for the affected combat skills.

### Evidence

- Current Generation and runtime use four removable concrete subclasses: Chain, Charge, Heal, and Shield.
- Current Chain and Charge have separate Single executor overloads.
- Current Buff Status, Heal, and Shield use three executor classes.
- Full approved behavior and file contracts are recorded in the handoff.
- `BuffSkillDefinition` now owns Status, Heal, Shield, and Charge through `BuffEffectKind`.
- `BuffSkillExecutor.Execute` is the sole Buff family entry and shares target/visual paths.
- OpeningCharge now uses the Buff active-state path plus ordinary `EnemyCombatDecision`/`EnemyActionController` targeting, movement, and `UnitCollisionResolver` contact; separate Charge Actor/State scripts are removed.
- Runtime and Editor builds completed with zero errors; Unity C# console errors were zero.
- Full Unity EditMode tests passed 11/11.

### History

- 2026-07-30: User approved implementation after the handoff MD is created.
- 2026-07-30: Code Builder created the handoff and started implementation.
- 2026-07-30: Code Builder completed Buff family consolidation and non-Play-Mode verification.

## Task: 2026-07-31 Skill Executor / Actor Responsibility Unification

### Task title

Unify skill execution as `SkillExecution -> Executor -> Actor -> EffectManager`.

### Goals

- Make spatial family Executors launch-only.
- Make spatial family Actors own targeting at the required timing, collision, hit judgment, effect application, Trigger publication, and completion.
- Make `SkillExecutionData` finalized before Executor dispatch.
- Remove duplicated hit enhancement implementations and `EffectManager`'s automatic family-Actor selection.

### Constraints

- Preserve current gameplay, Trigger, CSV, Definition, asset, and visual behavior.
- Buff remains the no-spatial-gameplay-Actor exception; `BuffSkillActor` owns Buff/status visual lifetime.
- Charge remains on `SkillUseState` plus ordinary enemy AI/movement/collision.
- Normal visual lifetime belongs to the family Actor; `EffectManager` only creates, tracks, deletes on Actor request, and force-clears combat effects.
- Spatial Actors and the no-gameplay-Actor Buff Executor reuse `SkillTargeting.cs`; no separate targeting algorithm is added.
- Add no base Actor, interface, factory, or standalone contract script.
- Unity Play Mode verification remains user-owned.

### Role Owner

Designer for the handoff. Code Builder after explicit user assignment.

### Status

Code implementation and available non-Play-Mode verification complete.

### Next Actions

- User verifies representative Projectile, Line, Single, Zone, Buff, Trigger, no-visual, and Charge behavior in Play Mode.

### Evidence

- Four spatial Executor files contain only matching Actor creation, attachment, initialization, and launch-result return; forbidden targeting, calculation, coroutine, gameplay-application, and Trigger symbols return zero matches.
- Single, Projectile, Line, and Zone family calculations, targeting, delayed/repeated work, collision, gameplay application, and completion reside in their matching Actor files.
- Spatial Actors and the Buff Executor call the existing `SkillTargeting`; no second targeting implementation was added.
- `SkillExecutionRuleResolver.ApplyHitEnhancements` is the only shared hit-enhancement implementation.
- Projectile no-visual execution uses an empty Projectile Actor; its branch visual uses `ProjectileSkillActor`, not `LineSkillActor`.
- Zone recast calls normal `ZoneSkillExecutor.Execute`; `ZoneSkillExecutor.ExecuteRecast` is removed.
- `EffectCreateRequest.DurationSeconds` and `EffectManager` automatic Single/Buff Actor attachment are removed.
- Persistent status end paths call `EffectManager.SignalStatusEffectEnded`; `BuffSkillActor` then requests removal.
- `BuffSkillExecutor.cs.meta` retains GUID `210c9a9da090fa545801a1d1fb30c1ed`.
- Removed-symbol searches find no consolidated subclass, Charge Actor/State, XML `<summary>/<c>` tag, Projectile direct-hit fallback, or Zone recast executor.
- `git diff --check` passes.
- Runtime and Editor builds complete with zero errors and the two existing assembly-reference warnings.
- Unity script compilation completes with zero Console errors.
- Full Unity EditMode tests pass 11/11.
- Full boundaries and final responsibilities are recorded in `boards/COMBAT/SKILL_EXECUTOR_ACTOR_RESPONSIBILITY_HANDOFF.md`.

### History

- 2026-07-31: User required no validation or Snapshot interpretation in Executors and gameplay judgment/application in Actors.
- 2026-07-31: User required Single to use the same high-level execute-then-judge flow as other spatial families.
- 2026-07-31: Designer created the implementation handoff from inspected current code.
- 2026-07-31: User corrected visual lifetime ownership; Designer revised the handoff so family Actors own completion and request `EffectManager` deletion.
- 2026-07-31: User confirmed reuse of `SkillTargeting.cs` and explicitly assigned Code Builder implementation.
- 2026-07-31: Code Builder completed Executor/Actor responsibility migration, EffectManager lifetime correction, Buff Executor rename, static/build/Unity/EditMode verification, and preserved Play Mode verification for the user.

## Task: 2026-07-31 Skill Cast Finalization / Actor Application

### Task title

Finalize cast-fixed skill values before delivery and reduce Actors to runtime application.

### Goals

- Finalize family values and deployment plans in `SkillExecution`/`SkillExecutionData`.
- Remove concrete Definition interpretation and cast planning from Actors.
- Consolidate targeting, status, hit application, and visual setup into existing shared paths.
- Delete cross-family Actor dependencies and direct Trigger-to-Executor bypasses.

### Constraints

- Preserve current gameplay, CSV, Definition, Trigger, visual, prefab, asset, and timing behavior.
- Keep hit-time target, collision, health/status, resistance, chance, death, recovery, consumption, and redistribution decisions in Actor/shared hit rules.
- Add no production script, family snapshot class, base Actor, interface, or factory.
- Reuse and move existing logic; do not copy algorithms.
- Actor owns normal lifetime and requests `EffectManager` deletion.
- Unity Play Mode gameplay verification remains user-owned.

### Role Owner

Code Builder for implementation. Code Reviewer for one final review pass.

### Status

Code Reviewer ran once and returned concrete fixes. Code Builder applied them in `a9088f0`; static and Runtime/Editor build verification pass. Final EditMode rerun is pending because Unity is in user-owned Play Mode.

### Next Actions

- User exits Play Mode when ready, then rerun the full EditMode suite.
- User verifies representative Projectile, Line, Single, Zone, Buff, Trigger, no-visual, and Charge behavior in Play Mode.

### Evidence

- Baseline Runtime and Editor builds complete with zero errors and two existing assembly-reference warnings.
- Baseline checkpoint commit is `c9303c4`.
- Actor calculation, Trigger bypass, shared status type placement, and cross-family Actor dependencies are recorded in the handoff.
- Family and consolidation commits are `8698375`, `4db7aef`, `fac4762`, `f6a9e33`, `95dbf2a`, `eae8a74`, `a59a415`, and `befb722`.
- Spatial Actor/Executor forbidden-symbol searches return zero concrete Definition, cast-calculation, and cross-family Actor matches.
- Runtime and Editor builds complete with zero errors and the two existing assembly-reference warnings.
- Unity focused Charge test passes 1/1 and full EditMode tests pass 11/11.
- Code Reviewer ran once and found Executor/Actor placement, triggered Definition identity, and Projectile branch/visual responsibility gaps.
- Fix commit `a9088f0` moves prepared placement/scheduling into Executors, preserves hit-time Actor application, prepares branch values and execution identity, and moves branch-line presentation into `EffectVisualBuilder`.
- Post-fix static searches and Runtime/Editor builds pass; Unity refresh passes and Console error query is empty.
- Post-fix EditMode job `167d7ad311004124906e358775f87d61` ran zero tests because Unity reported Play Mode/transition; Code Builder did not stop user-owned Play Mode.
- Full implementation evidence is recorded in `boards/COMBAT/SKILL_CAST_FINALIZATION_ACTOR_APPLICATION_HANDOFF.md`.

### History

- 2026-07-31: User approved the finalized responsibility boundary and explicitly assigned Builder implementation, intermediate commits, and final Reviewer inspection.
- 2026-07-31: Code Builder created the implementation handoff and preserved the prior completed work in checkpoint `c9303c4`.
- 2026-07-31: Code Builder completed family finalization, common-path consolidation, static/build/Unity verification, and intermediate commits.
- 2026-07-31: Code Reviewer ran once and returned concrete fixes; Code Builder applied them in `a9088f0`.
- 2026-07-31: Post-fix builds and Unity compilation passed; the full EditMode rerun remains pending because Unity was in user-owned Play Mode.

## Task: 2026-07-31 Lowest Health Ratio Targeting

### Task title

Select the ally with the lowest current-health/max-health ratio.

### Goals

- Make `LowestHealth` compare health ratios instead of absolute current health.

### Constraints

- Change only the existing shared comparison expressions.
- Preserve `HighestHealth` behavior and all targeting contracts.

### Role Owner

Code Builder.

### Status

Complete.

### Next Actions

- User verifies the Stage1 priest heals the ally with the lowest health ratio in Play Mode.

### Evidence

- `SkillTargeting.FindNearestTarget` and `SkillTargeting.CompareTargets` now compare `CurrentHealth / MaxHealth` for `LowestHealth`.
- `git diff --check` passes.
- `Assembly-CSharp.csproj` and `Assembly-CSharp-Editor.csproj` build with zero errors and the two existing assembly-reference warnings.

### History

- 2026-07-31: User requested a minimal formula-only Code Builder fix.
- 2026-07-31: Code Builder changed both shared `LowestHealth` comparison paths without adding a class or targeting mode.

## Task: 2026-07-31 Skill Trigger Reaction Logic Consolidation Design

### Task title

Remove the separate Trigger skill Definition and consolidate reactions through existing skill snapshots and Executors.

### Goals

- Keep `SkillTrigger.cs` as the single event and condition judge.
- Replace hidden TriggeredSkill Definitions with original or explicitly targeted skill snapshots.
- Record common-path conversions for cross-skill passives, event-derived damage, direct delivery, Actor-less passives, Zone recast, ChainLightning, and recursion.

### Constraints

- Add no C# script, Trigger Executor, Trigger Actor, event bus, or replacement Definition hierarchy.
- Do not copy `SkillTriggerDefinition` into another script.
- Preserve current working outcomes while restoring the approved 17 event effects and 64 normal cast effects.
- Keep event conditions in `SkillTrigger` and effect ownership in existing Skill/Choice/Passive execution.

### Role Owner

Code Builder.

### Status

User approved effect restoration and Code Builder implementation. Phases 1-8 complete. Code Reviewer corrections 1-4 are implemented and final PASS.

### Next Actions

- User Play Mode gameplay verification.
- Push local commits when repository access is available.

### Evidence

- Current design: `boards/COMBAT/SKILL_TRIGGER_REACTION_LOGIC_CONSOLIDATION_HANDOFF.md`.
- Inspected CSV join found 158 Trigger rows, 606 Trigger-owned Nodes, 77 runtime outcomes, and 81 no-runtime-outcome owners.
- The 77 outcomes are 27 damage, 21 status, 3 shield, 4 existing-skill execution, and 22 typed commands.
- The 27 damage outcomes split into 7 event-value reactions and 20 independently targeted/ranged/visual reactions.
- Learned passive source skills own 40 runtime outcomes.
- Existing code retains depth 8 in `SkillExecution`, generation 1 for `eve-e-master-1`, and ChainLightning delay 0.5, multiplier 0.5, radius 7, and primary exclusion.
- Semantic re-audit split the 158 rows into 65 working Trigger reactions, 17 Trigger-intent rows with no runtime outcome, and 76 non-Trigger rows.
- The 76 non-Trigger rows are 75 OnCast rows and one same-source OnSkillCast follow-up.
- Ariel-B trait 1~3 are direct Choice modifiers, trait 4 is an OnShieldExpire damage Trigger, trait 5 is an OnCast status modifier with no current runtime outcome, and master 2 is an OnShieldAbsorb damage Trigger.
- `SkillCatalogRuntimeTests.TriggerSemanticClassificationBaselineIsStable` fixes the semantic `65/17/76` owner classification.
- Phase 1 solution build completed with error 0; Unity focused EditMode test passed 1/1 and loaded catalog 5/8/8.
- Phase 2 routes preparation and Executors by existing `SkillRuntimeKind`; solution build error 0 and Unity EditMode 13/13.
- Current `AreaAttack` data has both `SingleSkillDefinition` (`Slash`, `FireDragonSlash`) and Zone-family results, so Phase 2 validates and preserves that existing family split.
- Phase 3 replaces the common `TryExecuteTriggered` dependency on the full Trigger Definition with `TryExecuteReaction`, passing existing runtime, Definition, snapshot runtime, and primitive execution adjustments.
- Phase 3 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 4 final runtime contains 82 semantic Trigger rows and zero OnCast/same-source follow-up rows.
- Phase 4 attaches 74 normal cast/passive payloads to existing Skill/Choice/Passive Nodes; `ariel-e-trait-4` uses the existing conditional-damage Choice operation and duplicate `eve-h-trait-3` is excluded.
- Phase 4 restores 64 `StatusModifier` payloads as normal `PassiveBuff` effects with authored target, status, source-skill, health-ratio, duration, and mutation conditions.
- Phase 4 solution build completed with error 0; Unity forced script compile and full EditMode tests passed 14/14.
- Phase 5 replaces 40 hidden direct-delivery Definitions with 57 common event effects: damage 24 and status 33, including the restored 17 incomplete status outcomes.
- Phase 5 leaves only 4 learned cross-skill references and 21 state commands; all 82 runtime reactions now have an outcome.
- Phase 5 solution build completed with error 0; Unity full EditMode tests passed 14/14.
- Phase 6 final catalog contains 48 passive source reactions: common effect 24, learned cross-skill reuse 4, and existing state command 20.
- The earlier design count 37 described a pre-refactor hidden-Definition subset, not final passive source ownership; the code-derived final count is 48.
- Phase 6 state commands remain on existing APIs: cooldown refund 14 and reload reduction 6; the remaining command is Zone recast.
- Phase 6 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 7 ChainLightning uses the original skill Damage and runtime snapshot without a `__chain` Definition; targeting remains primary-excluding, radius 7, delay 0.5, multiplier 0.5.
- Phase 7 Zone recast uses the original snapshot with delay 0.5, radius multiplier 0.6, duration 3, and generation cap 1.
- Phase 7 solution build completed with error 0; Unity full EditMode tests passed 15/15.
- Phase 8 deletes `SkillTriggerDefinition.cs/.meta`, `SkillDefinition.SkillTriggers`, and Monster/Enemy Trigger arrays.
- Existing Skill/Choice/Passive Nodes now own `SkillReactionOp`; `SkillExecutionData` exposes the active reaction snapshot to `SkillTrigger`.
- C# search finds zero `SkillTriggerDefinition` references; solution build error 0, Unity Console error 0, and full EditMode tests passed 15/15.
- Reviewer correction 1 removes the duplicate `ariel-a-master-2` cast payload, restores Vega B's delayed 0.45 same-skill line follow-up with silence and prepared aim, preserves Ariel C's prepared center, and reapplies passive effects after runtime Choice rebuild.
- Final normal cast/passive payload count is 73; solution build error 0, Unity Console error 0, and full EditMode tests passed 15/15.
- Reviewer correction 2 prevents Vega B's delayed self-reuse from scheduling the same normal follow-up again; other common reaction executions retain normal cast effects.
- Reviewer correction 3 separates reaction multiplier multiplication from normal additive Choice modifier accumulation; Vega B `1.25 × 0.45` regression is `0.5625`.
- Reviewer correction 3 solution build error 0; Unity EditMode tests passed 16/16; no compile-error console entry.
- Reviewer correction 4 removes the unused source catalog lookup from `SkillTrigger`; solution build error 0; Unity EditMode tests passed 16/16.
- Code Reviewer final PASS: obsolete Trigger symbols 0, diff check passed, build error 0, EditMode `TestResults.xml` 16/16; Play Mode remains user-owned.
- Remote push evidence: `git push origin main` failed because terminal authentication was unavailable; local commits remain intact.

### History

- 2026-07-31: User required an MD that integrates the existing blockers without adding scripts or relocating the old class contents.
- 2026-07-31: Designer wrote the common snapshot and existing-Executor consolidation handoff and preserved the earlier implemented handoff as baseline evidence.
- 2026-07-31: User clarified that ordinary modifiers and cast-time payloads are not Trigger reactions.
- 2026-07-31: Designer re-audited all 158 rows and corrected the handoff to separate working Triggers, incomplete Trigger intent, and non-Trigger authoring.
- 2026-07-31: User approved restoring the 17 event-driven effects and 64 normal cast effects, assigned Code Builder, and required per-phase commits plus Reviewer approval.
- 2026-07-31: Code Builder completed the Phase 1 classification baseline.
- 2026-07-31: Code Builder completed Phase 2 runtime-kind executor routing.
- 2026-07-31: Code Builder completed Phase 3 existing-skill runtime reuse.
- 2026-07-31: Code Builder completed Phase 4 non-Trigger extraction and normal effect ownership restoration.
- 2026-07-31: Code Builder completed Phase 5 direct-delivery consolidation and incomplete event-effect restoration.
- 2026-07-31: Code Builder completed Phase 6 Actor-less passive and state-command verification without adding runtime branches.
- 2026-07-31: Code Builder completed Phase 7 Zone and Chain snapshot reuse.
- 2026-07-31: Code Builder completed Phase 8 obsolete contract deletion and existing-node reaction ownership.
- 2026-07-31: Code Reviewer requested four behavior corrections; Code Builder implemented them through existing common paths.
- 2026-07-31: Code Reviewer found the Vega B asynchronous self-follow-up recursion; Code Builder disabled nested cast effects for that one reuse.
- 2026-07-31: Code Reviewer found reaction scaling incorrectly added to existing Choice damage modifiers; Code Builder added the shared reaction-only multiplication path and regression test.
- 2026-07-31: Code Reviewer found an unused source catalog lookup in `SkillTrigger`; Code Builder removed the dead dependency.
- 2026-07-31: Code Reviewer completed final PASS after correction 4; only user-owned Play Mode verification remains.

## Task: 2026-07-31 Skill Folder Three-Layer Reorganization

### Task title

Combat Skills를 Definitions, Implementation, Activation 세 책임 축으로 재배치하고 UnitSkills를 GameFlow로 이동한다.

### Goals

- `Definitions`는 authored skill/Choice/Node 계약을 유지한다.
- `Implementation`은 실행 문맥, snapshot, 규칙, 대상, 상태, Trigger, 단일 스킬 판정을 소유한다.
- `Activation`은 계열별 Executor와 Actor만 소유한다.
- `UnitSkills`는 전투 실행 폴더에서 제거하고 `GameFlow` 소유 상태로 이동한다.

### Constraints

- C# 본문, namespace, public API, serialized field, Unity asset GUID, scene/prefab reference를 변경하지 않는다.
- `Definitions` 내부 family/Node 구조와 authored CSV/data를 변경하지 않는다.
- 파일 이동은 `.cs`와 `.meta`를 함께 수행한다.
- Unity Play Mode gameplay 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

파일 이동 및 Unity refresh/솔루션 build/EditMode 검증 완료. Code Reviewer PASS.

### Next Actions

- 추가 코드 수정 요청 없음.
- 사용자 Play Mode gameplay 검증은 기존과 같이 사용자 소유.

### Evidence

- 이동 전 `Combat/Skills`는 31개 C# 파일, 11,228줄이었다.
- 이동 후 `Definitions` 12개, `Implementation` 8개, `Activation` 10개이며 `UnitSkills.cs`는 `GameFlow` 루트에 있다.
- 별도 `.asmdef`는 `Pakuri/Assets/Scripts` 아래에 존재하지 않는다.
- `UnitSkills` 소비자는 `RunSession`과 `UnitCombatState`이며, 클래스는 학습 active/passive/Choice ID만 보관한다.
- `Activation`은 Buff/Line/Projectile/Single/Zone의 Executor와 Actor만 포함한다.
- `Implementation`은 `SingleSkillRules`를 포함해 실행 규칙과 공통 runtime 경로를 포함한다.
- Unity가 `Assembly-CSharp.csproj`를 새 경로로 재생성했으며 active generated project의 구 `Delivery/Execution/Reactions/Runtime` 참조는 0건, `GameFlow/UnitSkills.cs` 참조는 1건이다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개로 완료했으며 기존 System.Net.Http/System.IO.Compression 충돌 경고 2개만 남았다.
- Unity full EditMode test job `553a856b5d664901bc4d355be30a7d5d`는 16/16 Passed; TestResults.xml도 `total=16`, `passed=16`, `failed=0`이다.
- staged diff는 모든 `.cs`/`.meta` 이동을 100% rename으로 인식했으며 `git diff --cached --check` whitespace 오류가 없다.

### History

- 2026-07-31: User approved Code Builder implementation of the three-layer folder organization and `UnitSkills` GameFlow relocation.
- 2026-07-31: Code Builder moved source files and `.meta` files without changing C# content or namespace.
- 2026-07-31: Unity project regeneration, solution build, full EditMode test, stale-path scan, and staged rename/GUID verification completed.
- 2026-07-31: Code Reviewer inspected the rename-only diff, layer boundaries, GUID/API preservation, build/test evidence, and returned PASS with no fix request.

## Task: 2026-08-01 Combat Skills Responsibility Comments

### Task title

Combat Skills 전체 스크립트의 역할과 메소드 의미를 실제 코드 책임에 맞춰 설명한다.

### Goals

- `Combat/Skills` 아래 모든 C# 스크립트 상단에 역할과 책임을 기록한다.
- 공개 타입과 모든 메소드, 생성자에 짧고 추상적인 한국어 설명을 둔다.
- 타입명과 매개변수를 반복하는 기계적 주석을 실제 실행 의미로 바꾼다.
- 주석에서 가운데점 문자를 사용하지 않는다.

### Constraints

- 실제 코드와 호출 흐름을 읽은 근거로만 책임을 설명한다.
- 실행 코드, API, 직렬화 계약과 동작을 변경하지 않는다.
- 필드마다 같은 설명을 반복하거나 핵심 흐름을 가리는 주석을 추가하지 않는다.
- 기존 사용자 변경인 `boards/OPS/AUTOMATION_GUIDE.md`는 수정하지 않는다.

### Role Owner

Code Builder.

### Status

Complete. 전체 주석 정비와 비게임플레이 검증을 완료했다.

### Next Actions

- 추가 코드 변경 없음.
- 주석 전용 변경이므로 별도 Play Mode 검증은 요구하지 않는다.

### Evidence

- 정비 전 `rg --files Pakuri/Assets/Scripts/Combat/Skills -g '*.cs'` 결과는 C# 28개이며 전체 10,786줄이다.
- 28개 파일 모두 상단 5줄 안에 `역할:`과 `책임:`을 가진다.
- 선언 검사 결과 타입 91개와 메소드, 생성자 330개 모두 바로 앞에 설명 주석이 있다.
- `rg -n "·" Pakuri/Assets/Scripts/Combat/Skills -g '*.cs'` 결과는 0건이다.
- 변경 전후에서 주석 줄을 제외한 C# 본문 비교 결과는 `COMMENT_ONLY_DIFF`다.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/Skills`가 통과했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개로 완료됐으며 기존 `System.Net.Http`, `System.IO.Compression` 버전 충돌 경고 2개만 남았다.

### History

- 2026-08-01: 사용자가 Code Builder에게 Combat Skills 전체 코드의 역할, 책임, 메소드 주석을 추상적이고 간결한 표현으로 정비하도록 요청했다.
- 2026-08-01: Code Builder가 Definitions, Implementation, Activation 28개 파일을 모두 읽고 주석만 수정한 뒤 선언 누락, 금지 문자, 본문 불변과 솔루션 빌드를 검증했다.

## Task: 2026-08-01 Learned Skill Runtime Initialization

### Task title

학습 시점에 스킬 실행값을 확정하고 전투 중 학습 강화 재판정을 제거한다.

### Goals

- 학습된 스킬의 쿨다운, 재장전, 연사, 탄창 실행값을 `RebuildLearnedSkillState` 완료 시 한 번 계산한다.
- 매 시전마다 실행값을 다시 반영하던 `RefreshRuntimeModifiers`를 제거한다.
- 학습 선택에 붙은 `RequiredSourceStatus`가 전투 중 강화 적용 여부를 바꾸지 않도록 고정한다.
- Trigger 사건의 `RequiredSourceStatus` 게이트와 적중 대상 상태 기반 효과는 별도 의미이므로 보존한다.

### Constraints

- 전투 중 학습과 강화 획득은 발생하지 않는다는 사용자 규칙을 기준으로 한다.
- `SkillExecutionData`의 스킬별 진행 상태와 `UnitSkills`의 스킬 목록 구조를 유지한다.
- 새 스크립트를 만들지 않고 기존 Resolver, `UnitSkills`, `SkillExecutionData` 경로를 재사용한다.
- Unity Play Mode gameplay 검증은 사용자 소유다.

### Role Owner

Code Builder.

### Status

Implementation complete. 솔루션 빌드와 정적 잔존 경로 검사를 완료했다.

### Next Actions

- 사용자 Play Mode에서 학습 강화가 스킬 시작 시점에만 반영되는지 확인한다.
- Trigger 상태 게이트가 기존 사건 경로에서 계속 판정되는지 확인한다.

### Evidence

- `UnitSkills.RebuildLearnedSkillState`가 active, passive 런타임 목록을 구성한 뒤 `SkillExecutionRuleResolver.BuildExecutionData`와 `InitializeRuntimeValues`를 호출한다.
- `SkillExecution.ResetRuntimeState`는 기본 실행값을 Resolver의 초기화 경로로 넘기며, `CanCastWithData`와 `TryBeginCast`에는 `RefreshRuntimeModifiers` 호출이 없다.
- `rg -n "RefreshRuntimeModifiers|MeetsSourceStatusRequirements|SourceStatusRequirementOp" Pakuri/Assets/Scripts Pakuri/Assets/Tests` 결과는 0건이다.
- 학습 Choice CSV의 `RequiredSourceStatus` 행은 0건이며, Trigger CSV의 동일 조건 행 4건은 보존했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:q`는 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check`가 통과했다.

### History

- 2026-08-01: 사용자가 학습 시점에만 강화 조건과 유효 실행값을 판정하고 전투 중 재판정을 제거하도록 요청했다.
- 2026-08-01: Code Builder가 유효 실행값 계산을 Resolver의 초기화 경로로 이동하고 `RefreshRuntimeModifiers`, 학습 Choice 상태 게이트와 관련 Node 경로를 제거했다.
- 2026-08-01: Trigger 상태 게이트와 적중 대상 상태 기반 Node는 별도 사건 의미로 분류해 보존하고 빌드와 정적 검사를 통과시켰다.

## Task: 2026-08-02 Zone Area Target Resolution Simplification

### Task title

Zone 스킬이 Collider와 겹친 모든 대상을 처리하도록 거리 기반 fallback과 불필요한 대상 수 제한을 제거한다.

### Goals

- Zone 실행 경로에서 `maxHitTargetCount`와 준비된 대상 수 제한 전달을 제거한다.
- `ApplyResolvedTargets`가 전달받은 유효 대상을 임의로 잘라내지 않고 모두 처리하게 한다.
- `radius <= 0`일 때 가장 가까운 대상 하나를 선택하는 우회 경로를 제거한다.
- 모든 Zone 틱이 `UnitCollisionResolver.CollectTargets`를 거치도록 실행 경로를 하나로 만든다.
- 투사체 충돌 지점의 반경 판정은 `ProjectileSkillActor`가 소유하게 한다.
- 현재 Zone 데이터의 기본 반경과 반경 배율이 양수인지 확인한다.

### Constraints

- SingleAttack의 `HitTargetCount` 실행 경로는 유지한다.
- Projectile 충돌 영역의 거리 판정, 중복 제거, 피해와 상태 적용 순서는 유지한다.
- Zone 반경 데이터는 이펙트와 Collider 크기 조정에 계속 사용한다.
- 사용자 소유의 Unity Play Mode 검증은 수행하지 않는다.

### Role Owner

Code Builder.

### Status

Complete. Zone Collider 전용 실행, Projectile 반경 판정 이동, 솔루션 빌드와 정적 검증을 완료했다.

### Next Actions

- 사용자 Play Mode에서 Zone 틱마다 Collider와 겹친 모든 대상이 처리되는지 확인한다.

### Evidence

- `ZoneSkillActor.ApplyResolvedTargets`에서 `maxTargets`, 임의 선택용 복사 목록, `Random.Range`, `RemoveRange`를 제거하고 `eligibleTargets` 전체를 순회한다.
- `ZoneSkillActor`의 `maxHitTargetCount` 필드와 `ZoneSkillExecutor`의 `PreparedHitTargetCount` 전달을 제거했다.
- `ZoneSkillActor`의 `radius <= 0` 최단 대상 우회 분기를 제거했다.
- `ZoneSkillActor.ApplyCurrentAreaTick`은 조건 분기 없이 `ApplyColliderAreaTick`만 호출한다.
- Zone Actor의 `radius`, `coverAll`, `usePrefabHitbox`와 관련 초기화 입력을 제거했다.
- 거리 기반 `ApplyAreaTargets`는 `ProjectileSkillActor.ApplyImpactAreaTargets`로 이동했으며 Projectile의 기존 반경 판정을 유지한다.
- 호출자가 사라진 `EffectVisualBuilder.ConfigureZoneEffect`와 Zone 준비 단계의 `PreparedRadius`, `PreparedCoverAll` 대입을 제거했다. 같은 준비 속성의 SingleAttack 사용은 유지했다.
- `ZoneSkillDefinition`과 Zone 데이터 빌더에서 Zone 전용 `UsesHitTargetCount`, `HitAllTargets`, `HitTargetCount`를 제거했으며 Single 경로의 같은 필드는 유지했다.
- 현재 AreaAttack 기본 반경은 `eve-c`, `eve-e`, `sein-d` 모두 `3.2`이며, Area 선택의 `RadiusMultiplier` 값은 `0.8`, `1.25`, `1.3`이다.
- 현재 Zone 행의 런타임 hitbox 크기는 `eve-c` 6.28x5.2, `eve-e` 7.38x7.24, `sein-d` 3.271948x1.705267이며 모두 양수다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check`가 통과했다.

### History

- 2026-08-02: 사용자가 Zone 대상 제한과 `radius <= 0` 우회 경로를 제거하도록 Code Builder 작업을 요청했다.
- 2026-08-02: Code Builder가 Zone 실행과 빌드 경로를 수정하고 Projectile의 현재 `int.MaxValue` 인자를 함께 제거했다.
- 2026-08-02: 사용자가 모든 Zone의 판정 규칙을 Collider 충돌로 확정했다.
- 2026-08-02: Code Builder가 Zone 거리 fallback을 제거하고 투사체 반경 판정을 Projectile로 이동한 뒤 빌드와 hitbox 데이터 검사를 완료했다.

## Task: 2026-08-02 Skill Execution Naming Cleanup

### Task title

스킬 실행 폴더와 핵심 실행 타입의 이름을 현재 책임에 맞게 정리한다.

### Goals

- `Combat/Skills/Implementation` 폴더를 `Combat/Skills/Execution`으로 변경한다.
- `SkillActionContext`를 `SkillExecutionContext`로 변경한다.
- `SkillExecutionData`를 `SkillExecutionState`로 변경한다.
- `SkillExecutionRuleResolver`를 `SkillExecutionRules`로 변경한다.
- `SkillExecution`, `SkillTargeting`, `SkillTrigger` 이름은 유지한다.

### Constraints

- 코드 동작, namespace, public member 구성과 실행 순서를 변경하지 않는다.
- 폴더와 세 스크립트의 Unity `.meta` GUID를 보존한다.
- 기존 사용자 수정은 보존하고 타입 식별자만 기계적으로 변경한다.
- 과거 통합 handoff의 역사 본문은 소급 변경하지 않고 후속 이름 변경 메모만 추가한다.

### Role Owner

Code Builder.

### Status

Complete. 폴더·타입·파일 rename과 빌드 검증을 완료했다.

### Next Actions

- Unity Editor가 새 Assets 경로를 반영한 뒤 Console에 import 오류가 없는지 확인한다.

### Evidence

- `Pakuri/Assets/Scripts/Combat/Skills/Execution`에 승인된 여섯 스크립트와 각 `.meta`가 존재한다.
- 폴더 GUID `9dbc5189d7d0d5c4297c238ae83029b1`을 보존했다.
- `SkillExecutionContext`, `SkillExecutionState`, `SkillExecutionRules`의 GUID는 각각 `82045cd00ee245b7bf885be962f8c619`, `5f6dba237e624d17a54e67ae7aeeb165`, `87ea56e14128452fbf054076cd83de47`로 기존 값과 같다.
- `SkillActionContext|SkillExecutionData|SkillExecutionRuleResolver` C# 검색 결과는 0건이다.
- `Skills/Implementation` C# 경로 검색 결과는 0건이다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check`가 통과했다.

### History

- 2026-08-02: 사용자가 실행 폴더와 핵심 타입의 책임을 확인한 뒤 새 이름을 승인했다.
- 2026-08-02: Code Builder가 폴더와 파일을 `.meta`와 함께 이동하고 모든 C# 참조를 변경했다.

## Task: 2026-08-03 Remove TriggerExecutionState

### Task title

`TriggerExecutionState`와 사건 연쇄별 반응 소비 추적을 제거하고 `IsTrigger`로 후속 반응 재진입을 차단한다.

### Goals

- `SkillTrigger.TriggerExecutionState`, `TryConsume`와 관련 전달 인자를 삭제한다.
- 모든 `SkillReaction`이 생성하는 스킬 실행에 `IsTrigger`를 전달한다.
- `IsTrigger == true`인 스킬은 피해·생명주기·투사체 적중 사건을 새 반응으로 재발행하지 않는다.
- 기존 Executor/Actor 실행, 피해 적용, 수명 종료, 명시적 반복 규칙은 유지한다.

### Constraints

- 원본 스킬 종료까지 후속 효과 생성을 예약하는 새 대기 처리는 추가하지 않는다.
- 정상 스킬의 직접 실행과 후속 반응 스킬의 수명·판정·삭제 경로를 분리하지 않는다.
- `OnHit`는 대상별 사건으로 유지하고, 반응별 전역 소비 키를 다시 추가하지 않는다.
- 사용자 Unity Play Mode 검증은 수행하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 정적 참조 검사와 `Assembly-CSharp` 빌드를 완료했다.

### Next Actions

- Unity Play Mode에서 일반 스킬, 다중 대상 OnHit, 후속 반응의 추가 피해와 후속 반응 재진입 차단을 확인한다.

### Evidence

- `SkillTrigger.cs`에서 `TriggerExecutionState`, `TryConsume`, 사건 전달용 `executionState` 인자를 제거했다.
- `SkillNodeConditions.cs:122`의 `SkillReaction.IsTrigger = true`가 후속 반응 정의의 기본 태그다.
- `SkillExecution.cs`는 반응 실행 시 `trigger.IsTrigger`를 실행 스냅샷에 전달하고, `NotifySkillCastTriggers`는 `context.IsTrigger`를 차단한다.
- `SingleSkillActor`, `LineSkillActor`, `ZoneSkillActor`, `ProjectileSkillActor`의 피해 호출은 `ApplyDamage(..., isTrigger: ...)` 공통 경로를 사용한다.
- `InGameCombatManager.ApplyDamageInternal`은 `AttackRule.IsTrigger`일 때 보호막·상태·피해·처치 후속 사건만 건너뛰고 실제 피해와 사망 정리는 유지한다.
- `ProjectileSkillActor.TryRunProjectileHitTriggers`와 `SkillTrigger.PublishLifecycleEvent`도 `IsTrigger` 실행을 새 반응으로 발행하지 않는다.
- 저장소 C# 검색에서 `TriggerExecutionState`, `ApplyDamageWithTriggerState`, `TryConsume(` 잔존 결과는 0건이다.
- `dotnet build Pakuri/Assembly-CSharp.csproj --no-restore`는 오류 0개, 기존 Unity 참조 충돌 경고 2개로 완료했다.
- `git diff --check`가 통과했다.

### History

- 2026-08-03: 사용자가 원본 종료 시점 예약 없이 `TriggerExecutionState`와 관련 불필요 로직을 제거하고 `IsTrigger` 기반 재진입 차단을 적용하도록 요청했다.
- 2026-08-03: Code Builder가 공통 실행 상태·피해 경로·Lifecycle/Projectile 경계를 수정하고 빌드와 잔존 참조 검사를 완료했다.

## Task: 2026-08-03 Consolidate AttackRule Constructors

### Task title

`AttackRule`의 일반 피해 생성자와 Trigger 피해 생성자를 하나로 통합한다.

### Goals

- 중복 생성자와 위임 초기화를 제거한다.
- `isTrigger = false` 기본값으로 일반 피해 호출을 유지한다.
- 후속 반응 피해는 기존처럼 `isTrigger` 값을 전달한다.

### Constraints

- `AttackRule` 필드와 `ApplyDamageInternal`의 동작을 변경하지 않는다.
- `AttackRule`의 일반 호출 호환성을 유지한다.
- 사용자 Unity Play Mode 검증은 수행하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 솔루션 빌드와 diff 검사를 완료했다.

### Next Actions

- 별도 작업 없음. Unity Play Mode는 필요 시 사용자 검증 영역이다.

### Evidence

- `InGameCombatManager.cs:19`의 public 생성자에 `bool isTrigger = false`를 통합했다.
- 기존 `InGameCombatManager.cs:42` internal 중복 생성자를 삭제했다.
- `InGameCombatManager.cs:217`의 내부 호출은 `isTrigger` 값을 그대로 전달한다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`가 통과했다.

### History

- 2026-08-03: 사용자가 두 `AttackRule` 생성자를 합치도록 Code Builder 작업을 요청했다.
- 2026-08-03: Code Builder가 기본값 인자를 가진 단일 public 생성자로 통합하고 빌드를 완료했다.

## Task: 2026-08-03 Inline ApplyDamageInternal

### Task title

`ApplyDamageInternal`의 단일 위임 경계를 제거하고 `ApplyDamage`를 유일한 피해 진입점으로 만든다.

### Goals

- `ApplyDamageInternal`을 삭제한다.
- `ApplyDamage` 안에서 자원 결과 처리, 화면 갱신, Trigger 후처리, 사망 정리를 유지한다.
- `DamageCalculator`는 최종 피해 수치 계산, `ApplyDamageToResources`는 HP·보호막 자원 변경을 계속 담당한다.

### Constraints

- 피해 계산식과 자원 변경 순서를 변경하지 않는다.
- `DamageCalculator`에 상태 변경·화면·Trigger 책임을 추가하지 않는다.
- Unity Play Mode 검증은 수행하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 잔여 참조 검색과 솔루션 빌드를 완료했다.

### Next Actions

- 별도 작업 없음. Unity Play Mode는 필요 시 사용자 검증 영역이다.

### Evidence

- `InGameCombatManager.cs:171`의 `ApplyDamage`가 기존 `ApplyDamageInternal` 본문을 직접 수행한다.
- `ApplyDamageInternal` 저장소 검색 결과는 0건이다.
- `InGameCombatManager.cs:245`의 `ApplyDamageToResources`와 `DamageCalculator.CalculateFinalDamage` 호출은 유지된다.
- `ApplyDamage`의 `DamageApplied`, Actor 화면 갱신, `SkillTrigger` 후처리, `RemoveUnitIfDead` 순서는 유지된다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs`가 통과했다.

### History

- 2026-08-03: 사용자가 `DamageCalculator`와 `InGameCombatManager`의 책임을 확인한 뒤 `ApplyDamageInternal` 제거 작업을 요청했다.
- 2026-08-03: Code Builder가 위임 메서드를 삭제하고 피해 후처리 순서를 `ApplyDamage`에 유지했다.

## Task: 2026-08-03 Move Outgoing Additional Damage Dispatch

### Task title

`ApplyOutgoingAdditionalDamageStatuses`와 outgoing 피해 이벤트 전달을 `SkillTrigger` 중심으로 정리한다.

### Goals

- `InGameCombatManager`의 `DispatchOutgoingDamageTriggers` 래퍼를 삭제한다.
- `SkillTrigger.ExecuteOutgoingDamage`에서 outgoing 반응 후 추가 피해 상태를 실행한다.
- 추가 피해의 실제 HP·보호막 변경은 기존 `InGameCombatManager.ApplyDamage` 경로를 재사용한다.
- `SkillExecutionRules`의 스킬 정의 해석 책임과 `CombatStart` 중복 방지 동작은 변경하지 않는다.

### Constraints

- `OutgoingAdditionalDamageSpecs`의 속성·스택·배율 판정을 변경하지 않는다.
- 추가 피해에 `criticalAllowed: true`, `suppressOutgoingDamageTriggers: true`를 유지해 기존 재귀 차단을 보존한다.
- 일반 피해의 outgoing 반응 후 추가 피해 실행 순서를 유지한다.
- Unity Play Mode 검증은 수행하지 않는다.

### Role Owner

Code Builder.

### Status

Implementation complete. 잔여 참조 검사, diff 검사, 솔루션 빌드를 완료했다.

### Next Actions

- Unity Play Mode에서 outgoing 반응과 `StatusOutgoingAdditionalDamage`의 추가 피해가 기존과 같은 순서로 실행되는지 확인한다.

### Evidence

- `InGameCombatManager.cs:219`가 `SuppressOutgoingDamageTriggers`가 아닌 일반 피해에만 `SkillTrigger.ExecuteOutgoingDamage`를 직접 호출한다.
- `InGameCombatManager.cs`의 `DispatchOutgoingDamageTriggers`와 기존 `ApplyOutgoingAdditionalDamageStatuses` 구현은 삭제했다.
- `SkillTrigger.cs:332`의 `ExecuteOutgoingDamage`가 적용 피해가 양수인 사건을 source-owned/passive-owned 반응에 전달한 뒤 추가 피해 상태를 처리한다.
- `SkillTrigger.cs:345`의 추가 피해는 `StatusCombatRules.OutgoingAdditionalDamageSpecs`를 읽고 `combatManager.ApplyDamage`를 재호출한다.
- 추가 피해 호출은 `criticalAllowed: true`, `suppressOutgoingDamageTriggers: true`를 유지한다.
- `rg -n "DispatchOutgoingDamageTriggers|ApplyOutgoingAdditionalDamageStatuses" Pakuri/Assets/Scripts -g '*.cs'` 결과에서 이전 manager 메서드는 사라지고 새 `SkillTrigger` 메서드만 남았다.
- `git diff --check -- Pakuri/Assets/Scripts/Combat/InGameCombatManager.cs Pakuri/Assets/Scripts/Combat/Skills/Execution/SkillTrigger.cs`가 통과했다.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal`은 오류 0개, 기존 참조 충돌 경고 2개로 완료했다.

### History

- 2026-08-03: 사용자가 추가 피해 실행과 outgoing dispatch를 `InGameCombatManager` 밖의 스킬 실행 경로로 정리하도록 요청했다.
- 2026-08-03: Code Builder가 manager 래퍼를 제거하고 `SkillTrigger.ExecuteOutgoingDamage`에 추가 피해 실행을 이동했다. `SkillExecutionRules`와 `CombatStart` 경로는 변경하지 않았다.

## Task: 2026-08-03 Remove Critical Resistance Runtime Path

### Task title

Remove active `CriticalResistance` data and damage calculation paths, then reuse the existing attacker-side `StatusCriticalChanceBonus` path.

### Goals

- Remove base and status critical-resistance fields from active runtime models, parsers, generation and authoring CSV schemas.
- Stop subtracting target critical resistance in `DamageCalculator`.
- Convert Vega G trait 3 from target critical-resistance reduction to conditional all-ally critical-chance bonus.

### Constraints

- Reuse existing `StatusCriticalChanceBonus`; do not add a new crit system or CSV column.
- Preserve `vulnerable`'s existing critical-damage-taken bonus; remove only its critical-resistance component.
- Preserve `Assets/Legacy` historical files; current runtime and authoring scope is `Pakuri/Assets`.
- Unity Play Mode and Editor catalog reimport remain user-owned while Unity is running.

### Role Owner

Code Builder.

### Status

Implementation complete. Active residue checks, CSV structure checks, diff check and solution build passed.

### Next Actions

- After Unity is available for a safe editor refresh, sync the runtime CSV catalog and verify Vega G trait 3 in Play Mode.

### Evidence

- `DamageCalculator.cs:70` retains attacker-side `attackRule.CritChanceBonus`; active `CriticalResistance` search across `Pakuri/Assets/Scripts` and `Pakuri/Assets/CSVdata/authoring` returned no results.
- `UnitCombatState`, unit definitions, `UnitCombatStateFactory`, CSV parsers, source models and `GameDataCatalogBuilder` no longer contain critical-resistance fields or mappings.
- `StatusEffectDefinition`, `StatusRules` and node generation no longer expose or parse `CriticalResistanceBonus`.
- `vega-g-trait-3` now selects `AllAllies` and applies `StatusCriticalChanceBonus` `0.10` while retaining the `silence&name-mark` condition.
- `status_effects.csv` retains `vulnerable` critical-damage-taken `0.03` and no longer contains a critical-resistance column.
- Current `monsters.csv`, `enemies.csv` and `status_effects.csv` import with consistent columns; `git diff --check` passed.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: User requested replacing `CriticalResistance` with the existing `CritChanceBonus` direction and removing the unnecessary resistance stat.
- 2026-08-03: Code Builder removed the active resistance path, converted Vega G trait 3 to the existing conditional attacker bonus, and preserved Legacy historical data.

## Task: 2026-08-03 Unify AreaAttack Runtime Kind

### Task title

Convert `sein-d` from `Field` to `AreaAttack` and remove the obsolete `SkillRuntimeKind.Field` execution path.

### Goals

- Author `sein-d` with the existing `AreaAttack` runtime kind.
- Remove `Field` from the runtime enum, loader allowlists, definition generation, execution routing and area-like status check.
- Update the runtime catalog editor test to validate the remaining `AreaAttack` path.

### Constraints

- Preserve `sein-d` damage, duration, tick interval, status and visual values.
- Do not change the shared `ZoneSkillDefinition` or `ZoneSkillExecutor` behavior.
- Unity catalog reimport and Play Mode remain user-owned while Unity Editor is running.

### Role Owner

Code Builder.

### Status

Implementation complete. Field residue checks, CSV parse, diff check and solution build passed.

### Next Actions

- After Unity refreshes the TextAsset, verify `sein-d` still creates the same persistent zone in Play Mode.

### Evidence

- `skills_area_attack.csv:5` now stores `sein-d` as `AreaAttack`.
- `SkillRuntimeKind.Field` was removed from `SkillDefinition`, `CsvSourceLoader`, `GameDataCatalogBuilder.Skills`, `SkillExecution` and `StatusRules`.
- `SkillCatalogRuntimeTests` no longer has a separate Field validation case.
- Active C# and authoring CSV searches for `SkillRuntimeKind.Field` and exact `"Field"` returned no results.
- `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings; `git diff --check` passed.

### History

- 2026-08-03: User requested full AreaAttack unification and removal of Field references and validation rules.
- 2026-08-03: Code Builder converted `sein-d`, removed the redundant runtime kind and updated the editor contract test.

## Task: 2026-08-03 Normalize Skill Graph Owner IDs

### Task title

Remove duplicated monster prefixes from choice graph CSV `owner_id` values while preserving canonical runtime IDs.

### Goals

- Store graph `owner_id` values as short IDs such as `c-trait-1`.
- Restore `monster_id-owner_id` during graph parsing so existing choice, trigger and skill lookup paths continue using canonical IDs.
- Preserve graph validation, target resolution, node grouping and generated node IDs.

### Constraints

- Change only `skill_graph_nodes_*.csv` under the active monster skill choices authoring tree and the graph parser needed to normalize them.
- Use the existing hyphenated ID convention (`eve-c-trait-1`), not a new underscore ID convention.
- Do not change `skill_choices_*.csv`, skill IDs, trigger IDs or target skill IDs.
- Unity TextAsset reimport and Play Mode verification remain user-owned.

### Role Owner

Code Builder.

### Status

Implementation complete. CSV normalization, parser build, diff check and solution build passed.

### Next Actions

- After Unity reimports the six graph CSV TextAssets, validate skill choices and trigger graph behavior in Play Mode.

### Evidence

- `SkillGraphParser.ParseSkillGraphNodeRow` now calls `NormalizeOwnerId(monsterId, ownerId)` before graph validation/materialization.
- `NormalizeOwnerId` is idempotent: it preserves a canonical `monster-id` input and prefixes only a short owner ID.
- All 858 rows across six active `skill_graph_nodes_*.csv` files now have short `owner_id` values; verification found `still_prefixed=0`.
- Sample authoring values are `eve / Choice / c-trait-1`, while the parser restores `eve-c-trait-1` for existing `model.SkillChoices` and graph generation.
- `git diff --check` passed; `dotnet build Pakuri/Pakuri.sln --no-restore -v:minimal` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: User requested removing duplicated monster prefixes from graph `owner_id` values and reconstructing them from `monster_id` during parsing.
- 2026-08-03: Code Builder shortened all active graph owner IDs and added canonical parser normalization without changing runtime ID consumers.

## Task: Repair CSV Header/Type Column Counts After Schema Cleanup

### Goals

- Align the current authoring CSV header and type-row column counts so `CsvParser.CsvTable.Load` can load `monsters.csv`, `enemies.csv`, and `status_effects.csv`.
- Preserve all existing CSV data values and avoid changing runtime parser logic.

### Constraints

- Modify only the three current authoring CSV type rows required by the reported error.
- Do not alter historical `Assets/Legacy` data or Unity Editor-owned reimport state.

### Role Owner

Code Builder.

### Status

Completed and statically verified; Unity Editor auto-sync/reimport remains user-owned.

### Next Actions

- Allow Unity to reimport the changed CSV TextAssets and confirm runtime catalog auto-sync completes.

### Evidence

- `monsters.csv` type row now has 22 columns, matching its 22-column header.
- `enemies.csv` type row now has 24 columns, matching its 24-column header.
- `status_effects.csv` type row now has 19 columns, matching its 19-column header.
- Quote-aware validation checked all 39 current authoring CSV files and every nonblank data row: `bad=0`.
- `git diff --check` passed; `dotnet build Pakuri/Pakuri.sln --no-restore -v:q` completed with 0 errors and the existing 2 assembly-reference warnings.

### History

- 2026-08-03: User reported the Unity auto-sync failure at `CsvParser.cs:122` for `monsters.csv`.
- 2026-08-03: Code Builder aligned the three mismatched type rows without changing CSV parser code or data values.

## Task: 2026-08-06 Stage Passive Lifetime And Combat-Start Unification

### Task title

Separate player registration from per-Stage passive and `CombatStart` execution, then remove the obsolete 0.5-second passive-status lifetime convention.

### Goals

- Register each runtime unit only when it enters the roster.
- After the complete player roster and Stage artifact effects are ready, execute player passive cast effects and `CombatStart` once for that Stage.
- Keep explicitly timed effects, including the 12-second `eve-f` shield, at their authored durations.
- Treat passive `StatusModifier` effects described as Stage-long effects as Stage-permanent statuses, cleared only by the existing Stage reset.
- Delete registration-owned or 0.5-second refresh assumptions that lose their responsibility.

### Constraints

- Do not add a periodic passive refresh timer.
- Do not periodically reapply shields, heals, damage, or other one-shot cast effects.
- Reuse `RefreshPassiveEffects`, `DispatchCombatStartOnce`, existing status `Permanent` handling, and `MonsterDayRecovery.ResetTransient`.
- Preserve enemy registration-owned `CombatStart` and enemy-target passive refresh for newly spawned enemies.
- Keep runtime registration, Stage start, passive execution, and status lifetime as separate responsibilities.
- Unity Play Mode gameplay verification remains user-owned.
- Complete and verify each implementation Phase before creating its Git commit.

### Role Owner

Code Builder.

### Status

Phase 1 design and inventory recorded. Implementation pending Phase 2.

### Next Actions

- Phase 2: centralize player Stage-start passive and `CombatStart` execution after roster/artifact preparation.
- Phase 3: replace obsolete 0.5-second passive `StatusModifier` lifetimes with the Stage-permanent contract and correct conditional modifier evaluation where required.
- Phase 4: add focused regression coverage, run final static/build checks, and finalize all three routed boards.

### Evidence

- `StageManager.ContinueToNextDay` calls `ResetCombatState`, which clears player statuses and shields through `MonsterDayRecovery.ResetTransient`.
- Existing player models remain registered, so `SpawnSelectedPlayerUnit` and `RestorePlayerPartyFromSession` skip registration and do not call `NotifyPlayerUnitRegistered` on later Days.
- `NotifyPlayerUnitRegistered` currently owns `RefreshPassiveEffects` and `DispatchCombatStartOnce`, coupling roster entry to Stage start.
- `StageManager.RunCurrentDayFlow` prepares artifact effects after `SpawnSelectedPlayerUnit`, so player `CombatStart` can run before artifact reactions exist.
- Authoring inventory found 58 passive `OnCast` `StatusModifier` effects with `SetDuration(0.5)`: 25 target `AllAllies`, 33 target `Enemy`, 11 are unconditional, and 47 carry conditions.
- `GameDataCatalogBuilder.Nodes` treats duration values of at least 9999 as `Permanent`; `MonsterDayRecovery.ResetTransient` still clears those statuses at the next Stage boundary.
- The existing 12-second `eve-f` shield and 9999-duration `ariel-g` shield are separate `ApplyShield` effects and are not part of the 58 status-modifier rows.

### History

- 2026-08-06: User approved Code Builder implementation, required the design to be recorded in Markdown, prohibited a 0.5-second periodic refresh system, requested deletion of obsolete code, and required one Git commit per Phase.
- 2026-08-06: Code Builder completed the current-code, caller, CSV, status-expiry, artifact-order, and test-gap inventory without modifying runtime code.
