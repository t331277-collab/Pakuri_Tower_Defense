# Damage Meter UI Code Builder Handoff

## Task title

Implement the `NewRunScene` damage meter overlay from the authored `Canvas/DamageMeterUI` hierarchy.

## Goals

- Open `Canvas/DamageMeterUI` from `Canvas/DamageMeterUIBtn`, hide the button while the overlay is open, and restore the button when the overlay closes.
- Show active party damage rows in fixed 1P to 5P order.
- Keep 1P as the initially selected monster, then map 2P to 5P from `RunSession.ManifestedMonsterIds` order.
- Track each monster's actual round damage by display source, including basic skill damage and master/trigger/additional damage when those sources are distinguishable.
- Render total damage, leader-relative percent, total meter width, and per-source skill meter segments according to `Pakuri/reference/7.UI/8-1. damage-meter-overlay-layout.md`.
- Add `MonsterIconImage` to `Pakuri/Assets/CSVdata/source/monsters.csv` and use it for the panel `Image` when present.

## Constraints

- Role Owner is Code Builder.
- Designer does not implement code or scene changes.
- Unity Play Mode gameplay verification remains user-owned.
- Use actual applied health plus shield delta for meter totals; do not use unresolved base damage or overkill-inclusive raw final damage.
- Keep `InGameUIManager.cs` focused on existing reward, Offering, and Menifest flow. Do not put the damage meter implementation there unless Code Builder finds a direct scene-binding constraint.
- Blank `MonsterIconImage` values must be accepted and must leave the panel image unchanged or hidden without failing CSV validation.
- `Skill-Meter` RectTransform position and size authored in the scene are the template authority; cloned source segments should preserve the template layout basis and only adjust segment width/position as needed.

## Role Owner

Code Builder

## Status

Designer handoff created. Implementation not started.

## Selected track

Designer implementation handoff plus gameplay-facing feedback clarity.

## Inspected evidence

- `AGENTS.md` and `MDTREE.md` require evidence-based work and minimal markdown routing.
- `AGENTS_ROLE/GAMEDESIGNER.md` says Designer does not implement code or scene changes.
- `boards/UI/RUNSCENE_UI.md` records `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` as the current top-level `NewRunScene` UI lookup/binding owner and Offering/Menifest flow owner.
- `boards/RUN/RUN_BLACKBOARD.md` records `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` as current `NewRunScene` runtime authority and `EffectManager` as current skill visual registry.
- `boards/DATA/DATA_BLACKBOARD.md` records `Pakuri/Assets/CSVdata/source/*.csv` and `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.*` as active runtime CSV authority.
- Unity-MCP found `Canvas/DamageMeterUIBtn` with `Button` and `Canvas/DamageMeterUI` with overlay `Image`.
- Unity-MCP found `Canvas/DamageMeterUI/1PDamagePanel` through `5PDamagePanel`, each with authored panel children; `1PDamagePanel` contains `Image`, `Monster_Name_Text`, `Total_Damage`, `Total_Damage_Persent`, `MeterBG`, and `Skill-Meter/SkillName`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` exposes `ApplyDamage(... BaseUnitRuntimeModel source, ... string sourceSkillId ...)` and builds `DamageApplicationOptions` with `Source` and `SourceSkillId`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` returns `InGameResourceChangeResult` with `PreviousHealth`, `CurrentHealth`, `PreviousShield`, `CurrentShield`, and `AppliedDamage`.
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs` currently uses `result.AppliedDamage` for damage popups, but that value is final calculated damage and may not equal actual resource delta when shields, low remaining health, or overkill are involved.
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs` stores `ManifestedMonsterIds`, appends manifested monsters in `RecordManifestedMonster`, and stores per-monster `PartyMembers` state.
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs` records a manifested monster, computes `slotIndex = Mathf.Clamp(session.ManifestedMonsterIds.Count, 1, 4)`, then calls `entryManager.SpawnManifestedMonster(...)`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `monsters.csv display_name` into `MonsterDefinition.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `monster_skills.csv display_name` into `SkillDefinition.DisplayName`.
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.Build.cs` maps `monster_skill_choices.csv title` into `SkillChoiceDefinition.Title`.
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs` currently has `DisplayName`, `UnitSprite`, and `ProjectileSprite`, but no dedicated monster icon field.
- `Pakuri/Assets/CSVdata/source/monsters.csv` currently has no `MonsterIconImage` column in its inspected header.

## Relevant files and Unity objects

- `Pakuri/Assets/Scenes/NewScene/NewRunScene.unity`
- `Canvas/DamageMeterUIBtn`
- `Canvas/DamageMeterUI`
- `Canvas/DamageMeterUI/Close`
- `Canvas/DamageMeterUI/1PDamagePanel` through `Canvas/DamageMeterUI/5PDamagePanel`
- `Canvas/DamageMeterUI/*PDamagePanel/Image`
- `Canvas/DamageMeterUI/*PDamagePanel/Monster_Name_Text`
- `Canvas/DamageMeterUI/*PDamagePanel/Total_Damage`
- `Canvas/DamageMeterUI/*PDamagePanel/Total_Damage_Persent`
- `Canvas/DamageMeterUI/*PDamagePanel/MeterBG`
- `Canvas/DamageMeterUI/*PDamagePanel/Skill-Meter`
- `Canvas/DamageMeterUI/*PDamagePanel/Skill-Meter/SkillName`
- `Pakuri/Assets/Scripts2/InGame/Core/InGameCombatManager.cs`
- `Pakuri/Assets/Scripts2/InGame/Run/RunSession.cs`
- `Pakuri/Assets/Scripts2/InGame/UI/InGameUIManager.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/MonsterDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Definition/SkillDefinition.cs`
- `Pakuri/Assets/Scripts2/InGame/Data/Runtime/Csv/PakuriCsvRuntimeData.*.cs`
- `Pakuri/Assets/CSVdata/source/monsters.csv`
- `Pakuri/Assets/CSVdata/source/monster_skills.csv`
- `Pakuri/Assets/CSVdata/source/monster_skill_choices.csv`

## Expected implementation surface

- Create a new runtime tracker, recommended name `DamageMeterRuntimeTracker`.
- Create a new UI controller, recommended name `DamageMeterUIController`.
- Add a serialized or auto-resolved reference from `DamageMeterUIController` to `InGameCombatManager` and the current run session source.
- Add a narrow call in `InGameCombatManager.ApplyDamage` immediately after `resourceMutations.ApplyDamage(...)` returns.
- Extend CSV source, row parsing, runtime model, asset reference collection, runtime catalog build, and validation for `MonsterIconImage`.

## Damage calculation ownership

`InGameCombatManager.ApplyDamage` is the correct event boundary. It already receives source unit and source skill id and already owns the resolved `InGameResourceChangeResult`.

The meter must calculate actual applied damage as:

```csharp
actualDamage =
    Mathf.Max(0f, result.PreviousHealth - result.CurrentHealth)
  + Mathf.Max(0f, result.PreviousShield - result.CurrentShield);
```

Only record when:

- `actualDamage > 0`
- `options.Source != null`
- source side is player/monster
- source monster id can be resolved from `options.Source.Identity.DefinitionId`

Do not record:

- healing
- shield grants
- rejected or zero-damage hits
- damage from enemies to player monsters
- damage prevented by invulnerability or missed/no-target paths

## Damage source identity

Basic implementation can use `sourceSkillId` and resolve display name from `monster_skills.csv display_name`.

Required implementation for this task should add a display-source layer so master or trigger damage can be separated from the base skill when runtime can identify it:

- Keep `sourceSkillId` for combat semantics.
- Add a meter-only source id, recommended `DamageMeterSourceId`.
- Add a meter-only display name, recommended `DamageMeterDisplayName`.
- Basic skill damage uses `sourceSkillId` and `SkillDefinition.DisplayName`.
- Choice/master-authored damage uses the `monster_skill_choices.csv title` when the triggering runtime knows the choice id.
- Trigger/effect-authored follow-up damage should pass a separate meter source id if it represents a separate displayed source, such as `vega-b-master1-second-slash`.

If Code Builder finds that a current executor passes only `sourceSkillId=vega-b` for a master-1 follow-up, then the UI cannot honestly display `침묵의 대태도 - 두번째 봉인` separately without adding meter-source metadata to that executor/trigger path.

## UI behavior

- On startup, `DamageMeterUI` should be hidden unless the scene intentionally ships it open for debug. If hidden at startup, `DamageMeterUIBtn` should be visible.
- `DamageMeterUIBtn.onClick` opens the overlay and disables/hides the button.
- `DamageMeterUI/Close.onClick` closes the overlay and re-enables/shows the button.
- Overlay opening does not pause combat.
- Open overlay refreshes immediately; while open, refresh at about `0.2` seconds or on dirty tracker events.
- Closing overlay does not reset accumulated round damage.
- Round damage resets when the next combat round starts, not when the overlay closes.

## Panel activation and party order

- Build display party list from current session:
  - index 0: selected monster id.
  - index 1 to 4: `RunSession.ManifestedMonsterIds` in existing list order.
- Bind `1PDamagePanel` to party index 0, `2PDamagePanel` to party index 1, and so on.
- Set unused panels inactive.
- Keep panel positions fixed; do not reorder panels by damage rank.
- If a monster disappears mid-combat, keep its current row visible for that combat if it was part of the session party.

## Text and meter formatting

- `Monster_Name_Text`: `MonsterDefinition.DisplayName`.
- `Total_Damage`: compact format from the layout doc.
- `Total_Damage_Persent`: leader-relative percent where top total is `100%`.
- If all totals are zero, show all active rows as `0`, `0%`, empty meter.
- Use comma formatting only where there is enough space; otherwise use compact `K`/`M`.
- Suggested compact examples: `999`, `1K`, `12.4K`, `968K`, `1.82M`.

## Skill meter rules

- Use the authored `Skill-Meter` object as the template.
- Clone one segment per nonzero meter source.
- Preserve template height, vertical position, and visual style.
- Segment width equals `monsterSourceDamage / monsterTotalDamage`.
- Segment x position is cumulative from left to right.
- `SkillName` should show the resolved display source name and compact damage value.
- Do not create a visible segment for zero-damage sources.
- Keep a stable source order:
  - base active skill order A to E first.
  - then active master or trigger sources in the order they first dealt damage.
  - then passive/additional sources in the order they first dealt damage.

## Monster icon data ownership

Add `MonsterIconImage` to `monsters.csv`.

Implementation requirements:

- Add header and type row entry, likely `asset_path`.
- Add parser row property in the monster source row model.
- Add asset-reference collection entry so the runtime asset catalog includes the sprite.
- Add `Sprite MonsterIconImage` or equivalent to `MonsterDefinition`.
- Map loaded sprite into `MonsterDefinition` during runtime catalog build.
- UI assigns it to `*PDamagePanel/Image`.
- Blank or unresolved value should leave the image blank/hidden and not crash.

## Edge cases

- Overkill: count only actual HP/shield removed.
- Shield: count shield damage and health damage together under the same source segment.
- Zero damage: do not grow segment; optional text row may show 0 only if UI has room.
- Additional outgoing status damage: if it reuses the original `sourceSkillId`, it will aggregate under that source unless Code Builder passes a distinct meter source.
- Triggered line/single/zone follow-ups: must pass distinct meter metadata when they should be displayed as separate lines.
- Enemy damage: excluded from this player-facing meter.
- Missing monster icon path: pass.
- Missing display name lookup: fallback to source id.
- Missing source id: fallback to `Unknown` only for debug; avoid visible production ambiguity if possible.

## Acceptance criteria

- Clicking `Canvas/DamageMeterUIBtn` opens `Canvas/DamageMeterUI` and hides/disables `DamageMeterUIBtn`.
- Clicking `Canvas/DamageMeterUI/Close` closes the overlay and restores `DamageMeterUIBtn`.
- Active 1P to 5P panels match selected plus manifested monster order, not damage rank.
- Unused damage panels are inactive.
- `Monster_Name_Text` shows `monsters.csv display_name` via `MonsterDefinition.DisplayName`.
- `Image` uses `MonsterIconImage` when present and safely passes when blank.
- `Total_Damage` equals actual applied health plus shield damage for the current round.
- `Total_Damage_Persent` uses highest party total as `100%`.
- Skill segments sum to the monster total and visually fill the same template meter width.
- Base skill source names come from `monster_skills.csv display_name`.
- Master/choice source names come from `monster_skill_choices.csv title` when the runtime provides that source metadata.
- Vega-B style follow-up damage can be displayed separately only if the trigger/effect path passes distinct meter-source metadata.

## Verification expected from Code Builder

- Unity-MCP scene inspection confirms `DamageMeterUIController` is attached and references or resolves `DamageMeterUIBtn`, `DamageMeterUI`, `Close`, and 1P to 5P panels.
- CSV field-count validation passes after adding `MonsterIconImage`.
- Unity `Pakuri/Validate CSV Source Data` passes.
- Unity `Pakuri/Sync CSV Runtime Catalog Assets` passes after icon asset paths are added.
- `dotnet build Pakuri\Assembly-CSharp.csproj --no-restore` passes.
- `dotnet build Pakuri\Assembly-CSharp-Editor.csproj --no-restore` passes.
- Unity console read shows no new C# compile errors or CSV runtime failures.
- User performs Play Mode gameplay verification for live combat numbers and visual fit.

## Related board files that must be updated

- `boards/UI/RUNSCENE_UI.md`
- `boards/DATA/DATA_BLACKBOARD.md`
- `boards/RUN/RUN_BLACKBOARD.md`

## History

- 2026-05-29: Designer inspected current scene objects, current damage application path, current run Menifest order, current CSV build mappings, and created this Code Builder handoff.
