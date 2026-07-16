# Vega F-J Passive Shared Runtime Completion

## Goal

Record the shared runtime surface and active CSV authoring that now make Vega passive skills F-J real gameplay behavior.

## Status

Completed on 2026-05-31 by Code Builder / Skill Builder.

## Implemented Shared Runtime

1. Burst-index status hook
   - `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now carries `HasBurstStatusProjectileIndex`, `BurstStatusProjectileIndex`, and `BurstStatusStacksBonus`.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillExecutionSnapshot.cs` resolves burst-index status bonuses through `ResolveBurstStatusStacksBonus(...)`.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Executors/ProjectileSkillExecutor.cs` now consumes that burst-status bonus on projectile hit.
   - Vega usage: `vega-f-trait-3` adds `+1` `name-mark` stack on Vega-A's last burst projectile.

2. Source-status-gated passive aura
   - `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now carries `RequiredSourceStatusId` / minimum stacks on effect and trigger definitions.
   - `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.MonsterDataset.cs`, `PakuriCsvRuntimeData.Build.cs`, and `PakuriCsvRuntimeData.Validation.cs` now parse/map/validate `required_source_status_id` and `required_source_status_min_stacks`.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/InGamePassiveEffectRuntime.cs`, `SkillMultiEffectExecutor.cs`, and `SkillTriggerRuntime.cs` now gate passive rows by live owner status.
   - Vega usage: `vega-h` ally buffs and mark-target debuff are active only while `slaughter-permit` is present on the owner.

3. Event runtime-kind / skill-kind filter
   - `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs` now carries `EventSkillRuntimeKinds`, `StatusConditionalIncomingSkillRuntimeKinds`, and `StatusConditionalOutgoingSkillRuntimeKinds`.
   - `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.StatusPayload.cs`, `MonsterDataset.cs`, `Build.cs`, and `Validation.cs` now parse/map/validate those fields.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Data/StatusEffectRuntime.cs` now filters incoming/outgoing modifiers through `MatchesSkillRuntimeKinds(...)`.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now filters trigger events through `trigger.EventSkillRuntimeKinds`.
   - Vega usage: `vega-i` only modifies or listens to `Area`-kind damage.

4. Multi-unit cooldown refund
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now resolves multiple allied target runtimes through `ResolveTargetRuntimes(...)` and applies cooldown/reload changes across `TargetSide=AllAllies`.
   - Vega usage: `vega-j` base and trait 1 refund cooldown to all allied active skills on Vega-E kills.

5. Direct trigger-to-effect execution
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillMultiEffectExecutor.cs` now exposes `ExecuteDirect(...)`.
   - `Pakuri/Assets/Scripts2/InGame/Skills/Execution/Runtime/SkillTriggerRuntime.cs` now routes trigger-owned effect rows through that direct path.
   - Vega usage: passive trigger rows such as `vega-g-mark-on-hit-base` and the Vega-I / Vega-J effect triggers.

## Implemented Active CSV Authoring

### Choices

- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`
  - `vega-f-trait-1` through `vega-j-trait-3` are now `RuntimeImplemented`.
  - `vega-h-base-duration` now exists as the required `PassiveBase` row.
  - `vega-f-trait-3` uses `runtime_target_skill_ids=vega-a`, `burst_status_projectile_index=0`, and `burst_status_stacks_bonus=1`.

### Effects

- `Pakuri/Assets/CSVdata/source/monster_skill_effects.csv`
  - Vega-F passive enemy-debuff rows were added.
  - Vega-G passive silence/name-mark rows were added.
  - Vega-H source-status-gated ally/enemy aura rows were added.
  - Vega-I Vega-D-hit area-vulnerability rows were added.
  - Vega-J survive-target debuff rows were added.
  - The effect CSV header/type rows are now normalized to 70 columns and include:
    - `required_source_status_id`
    - `required_source_status_min_stacks`
    - `status_conditional_incoming_skill_runtime_kinds`
    - `status_conditional_outgoing_skill_runtime_kinds`

### Triggers

- `Pakuri/Assets/CSVdata/source/monster_skill_triger.csv`
  - Vega-G passive mark-on-hit trigger was added.
  - Vega-I Vega-D-hit effect triggers and allied-area-damage cooldown-refund trigger were added.
  - Vega-J all-allies cooldown-refund, survive-target, and Vega-D refund triggers were added.
  - Trigger CSV now uses the already-added generic field `event_skill_runtime_kinds` for the Vega-I `Area` filter.

## Verification

- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore`
  - Passed with 0 errors. Only existing `MSB3277` warnings remained.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore`
  - Passed with 0 errors. Only existing `MSB3277` warnings remained.
- Unity menu `Pakuri/Validate CSV Source Data`
  - Final console result: `PakuriCsvRuntimeData loaded runtime catalog from resource source 'Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog' with 5 monsters and 8 stage-one enemies.`
- Unity menu `Pakuri/Sync CSV Runtime Catalog Assets`
  - Final console result: `Pakuri CSV runtime catalogs synced from 'Assets/CSVdata/source' to 'Assets/Resources/Pakuri/CSVRuntime'.`

## History

- 2026-05-31: Designer created the original handoff after re-auditing Vega F-J and finding no passive base/effect/trigger authoring in the then-inspected active CSV rows.
- 2026-05-31: Code Builder implemented the shared runtime contracts and Skill Builder authored the final Vega F-J rows.
- 2026-05-31: Initial Unity validation exposed a `monster_skill_effects.csv` header/type-row mismatch at row 114; after header normalization and Unity asset refresh, validation/sync passed.
