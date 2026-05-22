## Archive Note

- Full pre-cleanup snapshot moved to `boards/ARCHIVE/STATUS_EFFECT_BLACKBOARD_ARCHIVE_2026-05-18.md`.
- Older broad combat/status history remains in `boards/ARCHIVE/COMBAT_BLACKBOARD_ARCHIVE_2026-05-14.md`.
- This active file now keeps only the current shared status runtime baseline and the resource-display rule still relevant to active work.

## Task: 2026-05-22 StatusEffectKind Mojibake Alias Cleanup

### Task title

Remove broken-encoding defensive status parse aliases from `StatusEffectKind`.

### Goals

- Keep supported status parsing on canonical ASCII IDs and normal Korean labels.
- Stop accepting mojibake strings as hidden compatibility aliases.
- Preserve current runtime status kinds and display names.

### Constraints

- Role Owner is Code Builder.
- Scope is limited to `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs`.
- This task does not remove normal Korean aliases such as `감전`, `방어막`, or `신성 노출`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If the project later chooses ID-only status parsing, first enforce populated `status_effect_id` in CSV validation, then remove normal Korean label aliases.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` no longer contains mojibake alias cases such as `媛먯쟾`, `?뷀솕`, `?좎꽦 ?몄텧`, `移⑤У`, or `紐곗궡 ?덇?`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` still keeps canonical IDs like `shock`, `shield`, `holy-exposure`, and normal Korean aliases like `감전`, `방어막`, `신성 노출`.
- `Select-String` over `StatusEffectKind.cs` for the removed mojibake marker patterns only matched the normal C# conditional expression line in `BuildDisplaySuffix`, not a status parse alias.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: User asked Code Builder to remove all broken-encoding defensive strings from `StatusEffectKind.cs`.

## Task: 2026-05-22 Passive Buff And Shield Received Runtime

### Task title

Support passive buff statuses and shield-received modifiers in the shared status runtime.

### Goals

- Add a generic status kind for passive aura-style buffs that should not collide with Ariel-C `blessing` conditions.
- Let status modifiers increase shield amounts received by a target.
- Keep Holy damage, action speed, and incoming damage passive bonuses on the existing status modifier path.

### Constraints

- Role Owner is Code Builder.
- `blessing` remains reserved for authored blessing effects; passive aura rows use `passive-buff`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- If passive buff status labels clutter combat UI, add a CSV/runtime display-hiding flag instead of reusing `blessing`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now defines and parses `StatusEffectKind.PassiveBuff` with id `passive-buff`.
- `Pakuri/Assets/CSVdata/source/status_effects.csv` now includes `passive-buff` as a generic `Buff` row.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now includes `BuffModifierSpec.ShieldReceivedBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now includes `ResolveShieldReceivedMultiplier(...)` and includes `ShieldReceivedBonus` in modifier magnitude.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` copies `SkillEffectDefinition.StatusShieldReceivedBonus` into created status data.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` applies the shield-received multiplier inside `ApplyShieldStatus(...)`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.

### History

- 2026-05-22: Ariel-G required all allies to receive `+18%` shield amount, so Code Builder added a generic shield-received status modifier instead of Ariel-specific shield code.

## Task: 2026-05-22 Multi-Effect Buff Stat Runtime

### Task title

Support multi-effect CSV buffs for action speed, spell power, and outgoing element damage.

### Goals

- Let `monster_skill_effects.csv` apply ally status effects through the shared status runtime.
- Add spell-power bonus support to runtime status modifiers.
- Add outgoing element damage bonus support for shielded-ally Ariel-C trait 5.
- Let multi-effect status rows play attached visuals on the units that actually received the status, without changing the status application target.

### Constraints

- Role Owner is Skill Builder.
- Status application remains routed through `InGameCombatManager.ApplyStatus(...)`.
- Unity Play Mode verification remains user-owned.
- Code Reviewer was not run because explicit Reviewer permission was not given.

### Role Owner

Skill Builder

### Status

Implemented and locally validated by build plus direct CSV reference checks.

### Next Actions

- Future outgoing damage buffs should use the shared status modifier path rather than skill-specific damage branches.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/SkillData.cs` now adds `BuffModifierSpec.SpellPowerBonus`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now adds `ResolveSpellPowerMultiplier(...)` and `ResolveOutgoingDamageMultiplier(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` now applies spell-power status multipliers when resolving `StatSource.Intelligence` and applies outgoing element damage multipliers in `ResolveDamage(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:896` collects successfully applied status targets when `visual_anchor_mode=AppliedTargets`; `:938` routes those targets into attached visual spawning; `:1163` creates `InGameAttachedSkillEffectActor` instances on each target transform.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:1108` resolves visual centers through `SkillMultiEffectCenterMode`, including `PrimarySkillCenter`, `Caster`, and `NearestEnemy`, so status target selection is no longer forced to double as the visual center.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` has Ariel-C rows with `status_action_speed_bonus=0.12`, trait 2 rows with `0.06`, master 1 rows with `status_spell_power_bonus=0.18`, and trait 5 rows with `status_damage_bonus_rate=0.1` scoped to Holy and conditioned on `shield`.
- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv` has Ariel-C status rows with `target_side=AllAllies` and `visual_anchor_mode=AppliedTargets`, so the buff applies to allies and the requested `Ariel_C-Buff.prefab` attaches to affected ally units.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors; existing MSB3277 warnings remained.
- 2026-05-22 follow-up runtime/editor `dotnet build` commands passed with 0 errors after the applied-target visual extension; Unity console warning/error read showed only MCP client handler logs.

### History

- 2026-05-22: Skill Builder added shared status modifier support required by Ariel-C multi-effect rows.
- 2026-05-22: Code Builder separated multi-effect status targets from visual anchors and made applied-target status visuals attach through `InGameAttachedSkillEffectActor`.

## Task: 2026-05-21 Eve-A Recursive Branch Shared Shock Path

### Task title

Keep Eve-A recursive branch hits on the same shared projectile status path as the parent hit.

### Goals

- Ensure a branch projectile can still apply Eve-A shock through the shared projectile status helper.
- Ensure recursive branch hits reuse the shared projectile branch contract instead of adding a second Eve-only shock or branch path.
- Keep the branch damage falloff and branch chance tuning owned by the current choice CSV rows.

### Constraints

- Role Owner is Code Builder.
- The implementation must stay inside the current shared projectile/status runtime.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and compile-verified.

### Next Actions

- User verifies in Play Mode that branch-generated hits still show the same shock application behavior as the base Arc Bolt hit.
- If a later status rule needs branch-only behavior, extend the shared projectile status spec first instead of forking Eve-A logic.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` still applies projectile statuses only through `TryApplyStatus(...)`, and branch children are now initialized with the same `statusOnHit` spec instead of `null`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameProjectileActor.cs` now passes `branchOnHit.CloneForChild()` into branch child initialization, so recursive branch hits continue through the same shared branch/status path without sharing transient branched-target state.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now keeps Eve-A branch tuning in choice data with `eve-a-trait-5 branch_damage_multiplier=0.7` and `eve-a-master-1 branch_damage_multiplier=0.7`.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passed with 0 errors after the edit; existing MSB3277 warnings remained.

### History

- 2026-05-21: Code Builder changed the shared projectile actor so Eve-A branch hits inherit status and branch specs, which keeps recursive branch shock on the common status path.

## Task: 2026-05-20 Shield And Buff Source-Aware Status Runtime

### Task title

Implement the shield/buff unification blueprint on the shared runtime status path.

### Goals

- Move player-skill shield application from raw `CurrentShield += amount` authority to timed status instances with mutable absorb payload.
- Make buff/shield merge identity source-aware by `status kind + source skill + merge policy`.
- Keep same-source refresh and different-source coexistence grounded in CSV-owned fields.
- Remove the hardcoded shield-duration fallback from runtime execution.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution still requires explicit user permission and was not run in this task.

### Role Owner

Code Builder

### Status

Implemented and locally verified by build, editor-side deterministic execution, and CSV runtime sync.

### Next Actions

- User verifies in Play Mode that Ariel-B shield lifetime and VFX lifetime match the CSV duration in live combat.
- If reviewer permission is given later, run the enforced Builder -> Reviewer flow instead of treating prompt memory as completion.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:179-202` adds `ApplyShieldStatus(...)` so shield now enters combat through the shared status path instead of only through raw resource grant.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:533-539` consumes `target.Statuses.ConsumeShield(finalDamage)` before direct shield and health, and `:653-673` derives `CurrentShield` from `DirectShield + timed shield`.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:118-153` merges source-aware statuses by `SourceSkillId` and `MergePolicy`; `:245-277` sums and consumes timed shield instances; `:347-455` stores mutable shield payload including `MergePolicy`, `RemainingShieldAmount`, and refresh behavior.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:850-875` resolves shield duration from `skill.ShieldDuration` or `skill.ShieldStatus.Duration` and applies shield through `context.CombatManager.ApplyShieldStatus(...)`; the previous hardcoded `5f` fallback path is gone.
- Unity editor `execute_code` returned `shieldAfterDamage=6;healthAfterDamage=100;sameSourceShieldCount=1;sameSourceShieldRemaining=8;differentSourceShieldCount=2;totalShieldAfterDifferentSource=15;sameSourceBuffCount=1;differentSourceBuffCount=2;totalBuffStacks=2;expiredShieldCount=0;shieldAfterExpire=0`, which proves same-source refresh, different-source coexistence, timed shield consumption, and timed shield expiration on the edited runtime classes.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only the pre-existing `System.Net.Http` / `System.IO.Compression` MSB3277 warnings remained.

### History

- 2026-05-20: Code Builder implemented the blueprint by extending status identity, moving timed shield ownership into the shared status runtime, and synchronizing `CurrentShield` from active runtime state.
- 2026-05-20: Initial editor sync exposed two real data issues during verification: `monster_skills.csv` had not yet been reimported into Unity, and `status_effects.csv` shield row had a broken quote that collapsed row 10 to 2 columns.
- 2026-05-20: After asset refresh plus the `status_effects.csv` quote fix, `Pakuri/Sync CSV Runtime Catalog Assets` completed successfully.

## Task: 2026-05-20 Shield And Buff Source-Aware Status Unification Design Handoff

### Task title

Prepare a Code Builder handoff for converting timed ally shield/buff skills onto a source-aware runtime status model.

### Goals

- Ground the requested shield/buff redesign in inspected runtime and CSV evidence.
- Record why shield cannot stay on the current raw `GrantShield += amount` path.
- Hand Code Builder one implementation contract for CSV schema, runtime identity, shield payload, and verification expectations.

### Constraints

- Role Owner is Designer.
- This task creates a handoff document only; it does not implement runtime code.
- Unity Play Mode verification remains user-owned.
- The handoff must stay grounded in inspected files and current runtime behavior.

### Role Owner

Designer

### Status

Implementation handoff written for Code Builder.

### Next Actions

- Code Builder reads `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` before implementation.
- If Builder changes shield canonical id ownership or runtime identity names, record that exact migration choice here when implementation lands.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:840-845` shows shield duration currently falls back to hardcoded `5f`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:862` applies shield through `GrantShield(...)`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs:514-535` stores shield only as `CurrentShield`, with no duration/source tracking.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs:114` stores statuses by `StatusEffectKind` only.
- `boards/SkillBluePrint/shield-buff-status-unification-blueprint.md` contains the current Builder contract for the redesign.

### History

- 2026-05-20: User asked whether shield should be buff/status-unified with per-skill merge rules.
- 2026-05-20: Designer confirmed the direction is viable but requires a source-aware runtime identity model and a mutable shield payload, then wrote the Builder handoff blueprint.

## Task: 2026-05-20 Ariel-A Master 2 Holy Exposure Shared Status Use

### Task title

Activate Ariel-A master 2 through the shared Holy Exposure status path, including a choice-level Holy damage taken override.

### Goals

- Reuse the shared `StatusEffectKind.HolyExposure` parse/display contract for Ariel-A master 2.
- Confirm that a choice-only status can apply without a base skill status row.
- Confirm that a choice-only status can override its own incoming Holy damage multiplier without changing the shared catalog row for every other user.
- Keep the shared executor rules explicit for future active choice debuffs.

### Constraints

- Role Owner is Code Builder.
- No gameplay verification was run by Codex.
- This task did not add a new status kind; it reused the current working-tree Holy Exposure support.
- Code Reviewer execution requires explicit user permission and was not run.

### Role Owner

Code Builder

### Status

Confirmed and activated through existing shared status runtime with choice-level override support.

### Next Actions

- Future choice-only debuffs should populate `status_tag`, set stacks explicitly when deterministic one-stack behavior is required, and use choice-level override fields when one status kind needs different values per skill.
- If another debuff still appears missing in gameplay, inspect the active choice state before adding new runtime status code.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs:191-241` shows the shared status resolver prefers `snapshot.StatusTag`, defaults missing base status chance to `1f`, applies `StatusStacksSet`, and clones the resolved status data when a choice-specific `StatusElementDamageTakenBonus` override is present.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs:115-117` parses `holy-exposure` and `신성 노출`; `:174-175` defines the shared display label `신성 노출`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` keeps `ariel-a` with no base status, which makes `ariel-a-master-2` a choice-only shared status case.
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv` now sets `ariel-a-master-2` `status_tag=holy-exposure`, `status_stacks_set=1`, and `status_element_damage_taken_bonus=0.15`.

### History

- 2026-05-20: User asked Code Builder to apply the previously explained Ariel-A master 2 Holy Exposure fix through the shared status path.
- 2026-05-20: User then required per-skill values, so Code Builder extended the shared status path with a choice-level `StatusElementDamageTakenBonus` override and set Ariel-A master 2 to `0.15`.

## Task: 2026-05-17 InGame Shared Status Runtime Baseline

### Task title

Keep the current Scripts2 status runtime grounded in `StatusEffectKind` and the shared unit-status store.

### Goals

- Keep all new status work routed through `StatusEffectKind` instead of ad hoc strings.
- Keep status storage, ticking, apply/remove/query, and label refresh owned by shared runtime code.
- Keep Eve-A shock application on the shared projectile hit path.

### Constraints

- Role Owner is Code Builder.
- Unity Play Mode verification remains user-owned.
- Code Reviewer execution requires explicit user permission and was not run for the retained tasks.
- Detailed older status-effect slices remain in the archive snapshot.

### Role Owner

Code Builder

### Status

Current active status runtime baseline summarized and retained for future work. 2026-05-18 Code Builder refactor keeps status labels on the actor path while centralizing shared actor presentation in `UnitActorView`. 2026-05-18 projectile/status tuning now reads status chance and label from `monster_skills.csv`; supported runtime labels can now be used as a fallback when `status_effect_id` is blank.

### Next Actions

- Future skills should apply statuses only through `InGameCombatManager.ApplyStatus(...)`.
- Later passive/resistance/damage work should query `StatusEffectKind`-based runtime state rather than adding parallel status storage.
- Use the archive snapshot when older shield/freeze/temporary-effect details are needed.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` defines the shared enum and central status display helpers.
- `Pakuri/Assets/Scripts2/InGame/Units/BaseUnitRuntimeModel.cs` owns the current unit status store and ticking behavior.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` owns status apply/remove/query plus actor refresh on state changes.
- `Pakuri/Assets/Scripts2/InGame/Units/MonsterUnitActor.cs` and `EnemyUnitActor.cs` delegate active status label presentation to `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` and `InGameProjectileActor.cs` currently route Eve-A shock through the shared projectile hit path.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` and `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` both passed with 0 errors; only existing MSB3277 assembly-version warnings remain.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` now contains `status_chance` and `status_effect_label` per skill; Eve-A stores `status_effect_id=shock`, `status_chance=0.15`, and `status_effect_label=감전`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` passes CSV `StatusChance` into `StatusApplicationSpec.Chance`; `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` no longer contains the Eve-A shock chance special case.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectKind.cs` now parses supported Korean labels `감전`, `추위`, `냉기`, `빙결`, `둔화`, `취약`, and `방어막` in addition to the canonical ids.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now resolves blank `status_effect_id` from a parseable `status_effect_label` and stores the canonical status tag from `StatusEffectKind`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Validation.cs` rejects positive `status_chance` values on unsupported runtime status labels/ids.
- Unsupported design labels such as `침묵`, `이름표식`, `신성 노출`, `화염 저항 감소`, `행동속도 증가`, and `넉백` remain label-only in `monster_skills.csv` with `status_chance=0` unless a matching `StatusEffectKind` is added later.

### History

- 2026-05-17: Shared status runtime, enum centralization, label suffix display, and Eve-A shock application became the active baseline.
- 2026-05-18: Code Builder commonized `MonsterUnitActor`/`EnemyUnitActor` display refresh through `UnitActorView.cs`.
- 2026-05-18: Code Builder moved status chance/label authority from monster-level rows and hardcoded Eve-A executor logic into per-skill CSV rows.
- 2026-05-18: Code Builder made supported Korean status labels parseable from CSV, added validation for unsupported positive `status_chance`, and normalized design-only labels to chance 0.

## Task: 2026-05-18 LineAttack Status Application

### Task title

Route LineAttack status application through the shared status runtime.

### Goals

- Let Eve-B apply slow through `InGameCombatManager.ApplyStatus(...)`.
- Reuse CSV status fields for LineAttack skills.
- Avoid a separate Eve-only slow implementation path.

### Constraints

- Role Owner is Code Builder.
- Status chance and status ID are read from `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated by build plus Unity-MCP mapping inspection.

### Next Actions

- User verifies in Play Mode that Eve-B applies slow at the expected 20% tick chance and that the status label refreshes through the shared unit actor path.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` exposes shared status-spec resolution and uses it for projectile and beam skills.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameLineAttackActor.cs` applies status via `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` Eve-B row has `status_effect_id=slow`, `status_chance=0.2`, and `status_effect_label=둔화`.
- Unity-MCP mapping inspection returned `status=slow|chance=0.2` for Eve-B.
- Runtime/editor builds passed with 0 errors; existing MSB3277 warnings remain.

### History

- 2026-05-18: Eve-B LineAttack implementation reused shared status runtime instead of adding an Eve-only slow path.

## Task: 2026-05-15 Rounded HP Shield Display Baseline

### Task title

Keep HP and shield mutation/display rules grounded in the current rounded-resource implementation.

### Goals

- Preserve whole-number HP and shield mutation results.
- Preserve left-to-right HP fill behavior inside the authored actor background bounds.
- Preserve current damage popup formatting and actor refresh ownership.

### Constraints

- Role Owner is Code Builder.
- This retained baseline is still relevant because HP/shield display remains part of the active InGame combat presentation.
- Detailed intermediate follow-up history is preserved in the archive snapshot.

### Role Owner

Code Builder

### Status

Retained as an active display rule that still affects current combat/runtime work.

### Next Actions

- If shield timing or presentation changes later, update this file together with `boards/RUN/RUN_BLACKBOARD.md` and `boards/UI/RUNSCENE_UI.md`.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` now contains the integrated resource-mutation helper that rounds applied damage, HP, and shield values.
- `Pakuri/Assets/Scripts2/InGame/Units/UnitActorView.cs` owns the shared left-anchored HP/shield fill presentation used by `MonsterUnitActor.cs` and `EnemyUnitActor.cs`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` still routes rounded damage popup display through the actor layer.

### History

- 2026-05-15: Rounded HP/shield mutation and stable HP fill positioning were recorded as the current baseline.
- 2026-05-18: Code Builder moved shared HP/shield fill and damage-popup presentation from separate actor scripts into `UnitActorView.cs`.

## Task: 2026-05-18 Area Skill Status Application

### Task title

Route AreaAttack and SingleAttack status application through the shared status runtime.

### Goals

- Apply Eve C chill and Eve E vulnerable from CSV-driven area ticks.
- Apply one-shot area statuses through the same shared status helper path.
- Keep unsupported design-only labels at `status_chance=0` unless `StatusEffectKind` supports them.

### Constraints

- Role Owner is Code Builder.
- Status id/chance/label values are read from `monster_skills.csv`.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Eve C applies chill per tick and Eve E applies vulnerable per tick.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies statuses through `InGameCombatManager.ApplyStatus(...)`.
- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/SkillExecutors.cs` reuses `ProjectileSkillExecutor.ResolveStatusSpec(...)` for `ZoneSkillExecutor` and `SingleAttackSkillExecutor`.
- `Pakuri/Assets/CSVdata/source/monster_skills.csv` has `eve-c status_effect_id=chill status_chance=1` and `eve-e status_effect_id=vulnerable status_chance=1`.
- Unity-MCP `InGameSkillDataValidator.ValidateCatalog()` returned `valid=True; errors=0; warnings=0`.

### History

- 2026-05-18: Code Builder added area-status routing while adding AreaAttack and SingleAttack runtime execution.

## Task: 2026-05-21 Ariel-D SingleAttack Status Target Fix

### Task title

Keep Ariel-D status application on one strongest enemy.

### Goals

- Ensure Ariel-D's status application follows the same single target as its damage.
- Keep the status effect prefab path behavior unchanged.

### Constraints

- Role Owner is Code Builder.
- This task does not implement party focus-target AI.
- Unity Play Mode verification remains user-owned.

### Role Owner

Code Builder

### Status

Implemented and locally validated.

### Next Actions

- User verifies in Play Mode that Ariel-D's mark/status VFX is attached to only the highest-HP enemy.

### Evidence

- `Pakuri/Assets/Scripts2/InGame/Skills/Execution/InGameZoneSkillActor.cs` applies damage and `TryApplyStatus(...)` to exactly one target in the `!areaCoversAll && areaRadius <= 0f` branch.
- `Pakuri/Assets/Scripts2/InGame/Skills/Data/InGameSkillDefinitionMapper.cs` now prevents explicit-selection SingleAttack rows from setting `single.Area.CoverAll=true`.
- Ariel-D's CSV row has `target_selection=HighestHealth` and `status_effect_prefab_path=Assets/Prefab/Skill/Ariel/Ariel_D.prefab`.
- Runtime and Editor `dotnet build` commands passed with 0 errors and existing MSB3277 warnings.
- Unity-MCP console warning/error read after validation returned only MCP client handler logs.

### History

- 2026-05-21: After Mark/Execute conversion, Ariel-D still applied through the cover-all area branch because `Area.CoverAll` ignored `target_selection`; Builder aligned the SingleAttack area cover flag with explicit target selection.
