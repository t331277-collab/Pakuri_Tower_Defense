# New Core Script Structure Migration Plan

## Task State

Task title: Section 19 Script Move And Consolidation

Goals:

- Make the production source directly under `Pakuri/Assets/Scripts` follow section 19 of `new-core-architecture-blueprint.md`.
- Do not add folders or production script names that section 19 does not define.
- Consolidate five previously unclassified Skill support scripts into their nearest section 19 owners.
- Keep the remaining six Skill Execution scripts independent because each has a separate execution responsibility.
- Move, rename, or consolidate every other production script into a section 19 owner.
- Preserve current gameplay, CSV terminology, Unity resource references, scene and prefab connections, and Script GUIDs while the migration is in progress.
- Keep only necessary validation at actual untrusted boundaries. Remove defensive checks, fallback branches, and pass-through layers that have no reachable failure case after initialization.

Constraints:

- This document authorizes documentation only. Code Builder implementation still requires an explicit user request.
- Section 19 of `new-core-architecture-blueprint.md` is the structure authority.
- No `Contracts`, `Internal`, `Unity`, `Planning`, `Graphs`, `Triggers`, `Targeting`, `Calculation`, or other new production folder may be added.
- No collaborator script may be created unless section 19 already names that file.
- The six independent Skill Execution scripts listed in section 2 keep their current paths, names, and responsibilities.
- A moved Unity script must retain its existing `.meta` file until every serialized reference has been migrated or intentionally replaced.
- Do not change gameplay rules, CSV columns, authored values, skill results, stage flow, UI behavior, or resource values during this migration.
- `Pakuri/Assets/Scripts/Legacy` is not a source dependency.
- The five EditMode test scripts under `Core/Tests/Editor` are outside this production migration. The 117 tests are not an execution gate for this work.
- Play Mode verification remains user-owned.

Role Owner: Code Builder active under explicit user authorization.

Status: IMPLEMENTATION COMPLETE — M6 COMPLETE; PLAY MODE USER-OWNED

Next Actions:

- User performs the Play Mode verification retained by this plan.
- Keep all BLACKBOARD and domain-board files unchanged per user direction.

Evidence:

- `new-core-architecture-blueprint.md` section 19 defines the allowed production roots as `Core`, `Run`, `Units`, `Combat`, `Spawn`, and `UI`.
- The pre-migration `Pakuri/Assets/Scripts/NewCore` production inventory contained section 19 files, Presentation adapters, ten consolidatable support files, and six independent Skill Execution files.
- Current production references prove that `SkillActor`, `TimedSkillActor`, and `ScheduledSkillActor` share the lifecycle controlled by `SkillActorManager`.
- Current production references prove that only `SkillTriggerDispatcher` consumes `SkillTriggerSupport`, and only `SkillExecutionRuntime` consumes `SkillNodeSupport`.
- Current production paths prove all six independent Skill Execution files exist.
- `EnemyActionController` currently repeats constructor null guards for dependencies created before `RegisterEnemy`; this is one inspected example of a defensive check that must be judged against the final initialization boundary instead of retained automatically.
- The current Runtime Catalog and Run Start Selection are Unity `ScriptableObject` sources with serialized references, so their data must be copied and reconnected before their scripts can be consolidated.
- Current Presentation scripts are referenced by scenes, prefabs, or `.asset` files; their `.meta` GUIDs or replacement mappings must be verified during migration.
- `SkillEffectGraphRuntime.CreateVisual` currently constructs `EffectVisualSpec`, creates an Effect handle, and selects a `BuffActor` lifetime. `NewCoreEffectView` performs the actual GameObject creation. This split is replaced by one EffectManager visual authority.
- M0 baseline is anchored to Git commit `41fa0a5445b3950967dcb75144f3d15e2b93d209`.
- M0 recorded 107 NewCore C# files: 102 production scripts and five excluded EditMode test scripts.
- M0 recorded 1,177 public, internal, or protected declaration lines, 43 serialized Script GUID references across 27 scene, prefab, or asset files, and 13 NewCore assembly definition/reference files.
- M0 serialized component values are preserved in `new-core-script-structure-migration-m0-serialized-components.json`; path, GUID, SHA-256, API, assembly, and serialized-reference inventories are preserved in the five `new-core-script-structure-migration-m0-*.csv` manifests.
- M0 runtime and Editor `dotnet build` commands completed with zero errors and the existing two `MSB3277` warnings.
- M0 Unity script compilation returned to `ready_for_tools=true`; Console contained no project-script error and one known MCP package transport error, `Cannot access a disposed object`.
- M1 moved public `CsvDefinition` and internal `CsvDefinitionData` from `Core/Definitions/CsvDefinition.cs` into `Core/Parsing/CsvParser.cs` while preserving their `Pakuri.NewCore.Definitions` namespace and APIs; removed source GUID `0e357b0a67153bc4bb5d90679d54c3f9`.
- M1 moved `MonsterModifierSkillChoiceDefinition` from its separate file into `SkillChoiceDefinition.cs`; removed source GUID `6de311743465b644cada8a1f431362cb`.
- M1 moved `CatalogMonsterDefinition` from its separate file into `MonsterDefinition.cs`; removed source GUID `c11566aa831d53d4c89dbe3da3092722`.
- Repository scans found no scene, prefab, asset, controller, animation, or remaining source reference to the three removed GUIDs or old paths.
- M1 changed no CSV file, parser dispatch rule, required CSV path, namespace, public property, or authored value.
- M1 added required Korean responsibility comments to the three owner files and every declared method or constructor in `CsvParser.cs`.
- M1 runtime and Editor builds passed with zero errors; the runtime build retained two existing `MSB3277` warnings and the Editor build also reported the existing empty-source `CS2008` warning.
- M1 Unity compilation returned to `ready_for_tools=true`; Console warning/error query returned zero entries.
- M2 moved `CombatVector2` from `Units/Models/CombatVector2.cs` into `Units/Models/UnitBaseModel.cs` with the same public type name, namespace, operators, equality behavior, finite-value validation, and API; removed source GUID `136dabe68a7eb1640b1b34947364a8ab`.
- M2 moved `RuntimeCombatModifier` from `Combat/Status/RuntimeCombatModifier.cs` into `Combat/Status/StatusEffect.cs` with the same public type name, namespace, properties, and Tick behavior; removed source GUID `be8779e639f9fb2489a2c51b3a38b7c6`.
- Production search proved `RuntimeCombatModifier` had one constructor caller, public `UnitBaseModel.AddRuntimeModifier`; kind, finite value, non-negative finite duration, and non-null source validation moved to that public boundary, and the internal constructor now trusts the validated invariant.
- Repository scans found no scene, prefab, asset, controller, animation, source, or generated-project reference to the two removed GUIDs or old paths after Unity project regeneration.
- M2 added required Korean responsibility comments to `UnitBaseModel.cs`, `StatusEffect.cs`, and every declared method or constructor in those files.
- M2 changed no CSV, scene, prefab, `.asset`, namespace, public signature, unit numeric rule, shield rule, status rule, or runtime modifier result.
- M2 runtime and Editor builds passed with zero errors and the same existing warnings as M1.
- M2 Unity compilation initially exposed a stale removed-source path, then passed after `Assets/Open C# Project` regenerated Unity project files; final Console warning/error query returned zero entries.
- M3 moved `SkillActor`, `TimedSkillActor`, and `ScheduledSkillActor` into `Combat/Skills/Actors/SkillActorManager.cs`; removed source GUIDs `ef2b03ac16bffbe4db1eeb523906df67`, `27f46991cd08a264e826f2a496263608`, and `db480316602cf7f4bac3ca88be5cb548`.
- M3 moved public `SkillTriggerSupport` into `SkillTriggerDispatcher.cs` and public `SkillNodeSupport`, `SkillNodeBehavior`, and `SkillNodeRuntimeOwner` into `SkillExecutionRuntime.cs`; their public APIs were retained because inspected EditMode tests call them directly. Removed source GUIDs were `c286177fce36cc2449dae2d51f7159cf` and `b6df3a582967933488c8630c25a71ad1`.
- Repository scans found zero remaining source files for the five Skill support paths and zero serialized non-`.meta` references to their removed Script GUIDs.
- M3 left exactly 15 production files in `Combat/Skills/Execution`: the nine section 19 files plus the six explicitly allowed independent exceptions `SkillEffectGraphRuntime`, `SkillExecutionPlan`, `SkillExecutionRequest`, `SkillExecutionRuntime`, `SkillExecutor`, and `SkillTriggerDispatcher`.
- M3 declared engine-neutral `EffectVisualRequest` inside `EffectManager.cs`; all production visual call sites now send authored path, scale, sorting, position, and direction values through that request. Production search found `EffectVisualSpec` construction only inside `EffectManager`.
- `EffectManager` now owns final visual-spec construction, prefab/Sprite/AnimatorController lookup, Unity GameObject creation, active handles, Transform synchronization, and Play/Edit Mode deletion. `Pakuri.NewCore.Runtime.asmdef` changed `noEngineReferences` from `true` to `false`, as permitted by section 10, so the section 19 owner can compile Unity-facing visual responsibility without a new folder or circular assembly reference.
- `SkillEffectGraphRuntime.CreateVisual` became request-only `RequestVisual`; static scans found no `UnityEngine`, `GameObject`, `Transform`, `Instantiate`, `Destroy`, `EffectVisualSpec`, visual handle collection, synchronization, or deletion code in that file.
- Generic graph, Trigger, and impact visual timing now calls `SkillActorManager.RegisterEffectLifetime`; its private `EffectLifetimeActor` owns only duration completion. These call sites no longer choose `BuffActor` solely for visual lifetime.
- Existing `NewCoreEffectView` scene GUID `748fd1c9d9b75de40b51291026eb4180` remains because `NewRunScene.unity` still serializes it; the component is now a thin M4 migration adapter that only binds the catalog/root and delegates Sync/Clear to `EffectManager`.
- Existing `SkillVisualActorBehaviour` GUID `c29622d2ef50bc34e953b2cd9876644f` remains because the retained unreferenced `Eve_A.prefab` and `Airel_A.prefab` still serialize it; its handle-binding and Transform behavior moved to `EffectManager`, leaving a stateless M4 compatibility marker until those two serialized components are intentionally migrated.
- M3 retained public validation only at externally callable APIs and Unity binding; the sole inspected internal M3 constructor, `EffectHandle`, contains no repeated guard. No gameplay, CSV, scene, prefab, `.asset`, UI, central Tick order, namespace, or existing public visual API was changed.
- Unity project regeneration added the required UnityEngine references to `Pakuri.NewCore.Runtime.csproj`. `Pakuri.NewCore.Runtime`, `Pakuri.NewCore.Presentation`, `Pakuri.NewCore.EditMode.Tests`, `Assembly-CSharp`, and `Assembly-CSharp-Editor` builds passed with zero errors; full runtime/editor builds retained only the existing two `MSB3277` warnings and Editor `CS2008` warning.
- Focused EditMode jobs `f05857e1ed65410fa23bbbc9babee899` and `34e0b4c743f9428d8c457551bb9a1db5` passed four of four tests covering Unity visual creation/configuration, Actor registration/removal timing, complete visual specifications, and independent graph-effect lifetime.
- Final forced Unity script compilation completed and the Console warning/error query returned zero entries. The test framework's two package-owned setup/cleanup warnings and results-path message were recorded and cleared before the final gate. Play Mode was not run.
- M4 moved `Presentation/Actors/UnitActorBehaviour.cs`, `MonsterActorBehaviour.cs`, `EnemyActorBehaviour.cs`, and `NexusActorBehaviour.cs` to `Units/Actors/UnitActor.cs`, `MonsterActor.cs`, `EnemyActor.cs`, and `NexusActor.cs`. Their Script GUIDs remain `910185fae1feafb4a99a44a988ef7cd6`, `563a47fcaccf140499efb7f222c0c006`, `c82c4da6aacba674f864c62da59bf109`, and `dde153838e2141945aa1f7cc523f7717`.
- `DamageNumberPopupBehaviour` moved into `UnitActor`; `MonsterAnimationBehaviour` moved into `MonsterActor`; `SkillVisualActorBehaviour` was removed after its two remaining legacy-prefab components were intentionally removed. Removed GUIDs `879a27bbf0724a49b5ec3c9d42d9078f`, `ac027aa8c5506d5488ec4c797833239d`, and `c29622d2ef50bc34e953b2cd9876644f` have zero remaining non-`.meta` asset references.
- All five Monster prefabs preserve their authored idle state, dead state, attack-state count, and Animator component. All sixteen Enemy prefabs resolve `Pakuri.NewCore.Units.Actors.EnemyActor`; all five Monster prefabs resolve `Pakuri.NewCore.Units.Actors.MonsterActor`; `NewRunScene` resolves `Pakuri.NewCore.Units.Actors.NexusActor`.
- `Presentation/Scene/NewCoreSceneRuntime.cs` moved into `Core/Bootstrap/GameBootstrap.cs`; `NewCoreStageController`, `NewCoreSpawnController`, `NewCoreEffectView`, and `NewCoreInputController` moved into `Run/StageManager.cs`, `Spawn/SpawnManager.cs`, `Combat/Effects/EffectManager.cs`, and `Combat/Actions/PlayerInputController.cs`. The scene Script GUIDs `5bc74f204a88b16499661050ce2eee8b`, `fa9a3193209459a4d9b2e27b12b3e0e8`, `119dd9f412ed3074ab18f6e8ac562b9d`, `748fd1c9d9b75de40b51291026eb4180`, and `4717eeab25417eb4eba081006c884d45` now belong to those section 19 owner files.
- The replaced pure-manager GUIDs `d9263b35d4747084d91847fe80e8ca82`, `c984a818af64a864696b0800ac0dcf75`, `61539cf51eca507409f966df96153e16`, `3e7abfd4837c1224b8c1ef68a8ecb800`, and `88a6c463d98613046af6c61ef63295a5` had no serialized asset references before replacement and have zero remaining references.
- `NewCoreRuntimeCatalogAsset` and `RunStartSelectionAsset` were removed only after their values moved into the `GameBootstrap` scene component. A normalized M0-to-current comparison found the complete Runtime Catalog body as an exact match and retained `defaultMonsterId: eve`. Their removed Script GUIDs `6d9b293472613b242b8a3c359d02a496` and `da22dc09447e20c4ea34d3d04f395c38` have zero remaining asset references.
- The old Core, Runtime, and Presentation assembly split became one root `Pakuri.NewCore.asmdef` because the section 19 Unity-facing owners and runtime managers now share files. The original Core asmdef GUID `963383dcb1f58a24da4ea506a50f04a2` remains; only the root production asmdef and the Editor test asmdef remain under `NewCore`.
- Deleted-meta audit inspected 38 removed `.meta` paths: 10 GUIDs exist at their new owner `.meta`, 28 GUIDs have zero remaining asset references, and zero unresolved deleted GUIDs remain.
- Serialized comparison inspected 27 preserved M4 components and found zero value mismatches. Runtime Catalog exact-body, run-selection value, copied Monster animation values, Animator references, and nine target owner `.meta` GUID checks all passed.
- The five Editor test files remain outside production migration and the 117 tests were not run. Their construction and type references were updated only so the renamed MonoBehaviour owners and public CSV bootstrap boundary compile.
- Comment audit inspected 177 declared methods and constructors across the nine moved or consolidated M4 owner files and found zero missing required Korean `/* */` role comments. Each file also has its required namespace responsibility comment.
- Final `Pakuri.NewCore.csproj` and `Pakuri.NewCore.EditMode.Tests.csproj` builds each passed with zero warnings and zero errors. A forced Unity refresh/domain reload completed, Editor state returned ready, and the final Console error query returned zero entries.
- Unity scene validation reported zero issues, zero Missing Scripts, and zero broken prefabs for both `NewMainMenu` and `NewRunScene`. Unity prefab inspection resolved the expected new Actor type on all 21 migrated Monster and Enemy prefabs. Play Mode was not run.
- M5 replaced `NewCoreInGameUIController.cs` with the five exact section 19 owners `InGameUIManager.cs`, `RewardPanelController.cs`, `PrisonPanelController.cs`, `OfferingPanelController.cs`, and `ManifestationPanelController.cs`. `InGameUIManager.cs` retains Script GUID `a29b50b3033957d438458097cb36ab39`; the four extracted owners use GUIDs `cece438d32343c34da8fcc2cba16f986`, `b7d220d22fce8ff43aa44def0f3091e2`, `a21ed6261c64e2a40a84c1c3278cad60`, and `ef6bc3cf6355f784386ec14838b95907`.
- The six reusable UI components moved to their section 19 UI folders with their existing Script GUIDs. Normalized M0-to-current comparison found zero serialized-value mismatches across those six components, and the original 25 authored hierarchy paths used by `NewCoreInGameUIController` equal the 25 paths used by the five split owners.
- The old in-game UI portrait GUIDs and reward layout values were copied exactly to the extracted Prison, Manifestation, and Reward owners. Unity component inspection resolved all five new owners, their `GameBootstrap` references, both result panels, five portraits, and the retained `(-321.97855, 295)`, `533.9786`, `122`, and `3` layout values.
- Stage-result fields and presentation moved out of `StageManager` into `InGameUIManager`. Static search found zero UI imports or result-panel fields in `StageManager`; its `winStageIndex: 2` and `winDayIndex: 11` remain serialized.
- UI source search found zero direct Model, currency, cooldown, `PrisonerInventory`, `PartyRoster`, or `SkillBucket` write paths. Debug skill-learning and Choice mutations now enter through public `GameBootstrap.TryLearnSkill` and `TrySelectSkillChoice` commands.
- Both scenes contain zero persistent Button calls: `NewRunScene` has 55 Buttons and `NewMainMenu` has eight. Each runtime binding path clears runtime listeners before adding its single command callback.
- M6 removed the empty old `Presentation` root after its five folder GUIDs were checked and found to have zero non-`.meta` references. Production roots are exactly `Combat`, `Core`, `Run`, `Spawn`, `UI`, and `Units`; the prohibited folder scan found zero `Contracts`, `Internal`, `Unity`, `Planning`, `Graphs`, `Triggers`, `Targeting`, `Calculation`, or `Presentation` directories.
- Final production inventory contains 86 C# files, zero Legacy source dependencies, zero obsolete support source files, and only the root production asmdef plus the Editor test asmdef. Source and serialized scans found zero old `Pakuri.NewCore.Presentation` or `Pakuri.NewCore.Runtime::` identifiers.
- Final deleted-meta audit inspected 50 removed `.meta` paths: 17 GUIDs exist at their new owner `.meta`, 33 GUIDs have zero remaining asset references, and zero unresolved deleted GUIDs remain.
- Final comment audit inspected 520 declared methods and constructors across 30 changed production files and found zero missing Korean `/* */` role comments; all 30 also have namespace responsibility comments.
- Final `Pakuri.NewCore.csproj` and `Pakuri.NewCore.EditMode.Tests.csproj` builds each passed with zero warnings and zero errors. Unity forced refresh/domain reload returned `ready_for_tools=true`; the final Console error query returned zero entries.
- Final Unity validation reported zero issues, zero Missing Scripts, and zero broken prefabs for both `NewMainMenu` and `NewRunScene`. Runtime inspection resolved the six moved reusable UI types and all five exact in-game UI owners. The 117 EditMode tests and Play Mode were not run.

History:

- 2026-07-25: Initial migration plan introduced extra responsibility folders and collaborator files beyond section 19.
- 2026-07-25: Plan revised. Extra folders and collaborator scripts were removed from the target. Eleven unclassified Skill runtime files became explicit holdovers. Remaining production scripts now move or consolidate into section 19 owners only. Unnecessary defensive code became a prohibited convention.
- 2026-07-25: Skill runtime classification revised again. Three Actor base/scheduling files move into `SkillActorManager.cs`, `SkillTriggerSupport` moves into `SkillTriggerDispatcher.cs`, and `SkillNodeSupport` moves into `SkillExecutionRuntime.cs`. Six execution files remain independent. Visual creation, synchronization, and deletion become `EffectManager` responsibility while `SkillEffectGraphRuntime` only decides when and where to request a visual.
- 2026-07-25: Code Builder implementation started under explicit user authorization. M0 exact before-state manifests were created, runtime and Editor builds passed with zero errors, Unity compilation returned ready, and no BLACKBOARD or domain-board file was changed.
- 2026-07-25: M1 Core Definition consolidation completed. Three support source files and their unreferenced `.meta` files were removed after their types moved unchanged into section 19 owners; builds and Unity compilation passed.
- 2026-07-25: M2 Unit and Status consolidation completed. `CombatVector2` and `RuntimeCombatModifier` moved into section 19 owners, redundant internal modifier validation moved to the public `AddRuntimeModifier` boundary, and final builds plus Unity compilation passed.
- 2026-07-25: M3 Combat Structure completed. Five Skill support files moved into section 19 owners, the six independent Execution exceptions remained exact, visual construction/synchronization/deletion moved to `EffectManager`, generic visual lifetime moved to `SkillActorManager`, four focused EditMode tests passed, and final builds plus Unity compilation passed.
- 2026-07-25: M4 Actors, Bootstrap, Input, Spawn, and Resources completed. Actor and scene adapters moved into section 19 owners, ScriptableObject values moved into `GameBootstrap`, one root production assembly replaced the circular split, all serialized GUID/value audits passed, both builds passed, and Unity reported zero errors and zero Missing Scripts.
- 2026-07-25: M5 UI completed. Existing UI scripts moved with retained GUIDs, `NewCoreInGameUIController` split into the five exact owners, stage-result presentation moved into `InGameUIManager`, Inspector values and hierarchy paths were preserved, and UI state mutations were routed through runtime command boundaries.
- 2026-07-25: M6 and final gates completed. The old Presentation root and obsolete support paths were removed after reference scans, final structure/comment/GUID audits passed, both builds passed without warnings or errors, and Unity reported zero Console errors and zero scene validation issues.

## 1. Structure Authority

The final structure is section 19, not the previous expanded migration tree.

Allowed production folders:

```text
Pakuri/Assets/Scripts/
├─ Core/
│  ├─ Bootstrap/
│  ├─ Parsing/
│  ├─ Catalog/
│  └─ Definitions/
│     ├─ Skills/
│     ├─ Choices/
│     ├─ Units/
│     ├─ Stage/
│     └─ Status/
├─ Run/
│  └─ Services/
├─ Units/
│  ├─ Models/
│  └─ Actors/
├─ Combat/
│  ├─ Actions/
│  ├─ Skills/
│  │  ├─ Runtime/
│  │  ├─ Execution/
│  │  └─ Actors/
│  ├─ Status/
│  └─ Effects/
├─ Spawn/
└─ UI/
   ├─ MainMenu/
   └─ InGame/
      ├─ MonsterPanel/
      ├─ Nexus/
      ├─ DamageMeter/
      ├─ UtilityPanel/
      └─ Debug/
```

Rules:

- Use section 19 file names when section 19 provides an exact name.
- When section 19 says to reuse existing UI scripts, move the existing script into that UI folder without inventing a replacement name.
- A support type may be placed in its section 19 owner file when removing a separate non-section-19 script.
- The primary public type must match the file name.
- Do not create a new file only to preserve a type that has no independent state authority or lifecycle.
- Do not keep a `Presentation` root after all scene and asset references have moved.

## 2. Skill Runtime Consolidation And Six Independent Files

Five support files move into section 19 owners:

| Current file | Target owner | Result |
|---|---|---|
| `Combat/Skills/Actors/SkillActor.cs` | `Combat/Skills/Actors/SkillActorManager.cs` | Keep the abstract lifecycle type in the Manager file and remove the separate source file |
| `Combat/Skills/Actors/TimedSkillActor.cs` | `Combat/Skills/Actors/SkillActorManager.cs` | Keep duration-completion behavior as an internal type in the Manager file |
| `Combat/Skills/Actors/ScheduledSkillActor.cs` | `Combat/Skills/Actors/SkillActorManager.cs` | Keep delayed and repeated execution as an internal type in the Manager file |
| `Combat/Skills/Execution/SkillTriggerSupport.cs` | `Combat/Skills/Execution/SkillTriggerDispatcher.cs` | Move validation and typed trigger-column readers into the Dispatcher |
| `Combat/Skills/Execution/SkillNodeSupport.cs` | `Combat/Skills/Execution/SkillExecutionRuntime.cs` | Move node handler classification and runtime-owner validation into the Runtime |

Consolidation rules:

- Preserve the three Actor types while current concrete Actors and Executors require their distinct lifecycle contracts.
- Do not collapse timed completion and scheduled repeated execution into one conditional class.
- Reduce consolidated support types and methods to `internal` or `private` where external access is not required.
- Remove each old `.cs` and `.meta` only after every source reference compiles against the target owner.
- Do not create replacement files or subfolders.

The remaining six files keep independent responsibility:

1. `Combat/Skills/Execution/SkillEffectGraphRuntime.cs`
2. `Combat/Skills/Execution/SkillExecutionPlan.cs`
3. `Combat/Skills/Execution/SkillExecutionRequest.cs`
4. `Combat/Skills/Execution/SkillExecutionRuntime.cs`
5. `Combat/Skills/Execution/SkillExecutor.cs`
6. `Combat/Skills/Execution/SkillTriggerDispatcher.cs`

Independent-file rule:

- Keep current paths, names, namespaces, and public callers.
- Do not merge these six merely to reduce file count.
- `SkillExecutionPlan` owns learned Choice and Plan-node composition.
- `SkillEffectGraphRuntime` owns effect-graph interpretation, not visual object ownership.
- `SkillExecutionRequest` carries one execution context.
- `SkillExecutionRuntime` coordinates plans, family Executors, cooldowns, graphs, and triggers.
- `SkillExecutor` owns family-shared damage and status execution.
- `SkillTriggerDispatcher` owns combat-event matching, trigger gates, scheduling, and trigger actions.

## 3. Files That Already Match Section 19

The following files keep their current section 19 path, name, and responsibility:

### Core

- `Core/Bootstrap/GameBootstrap.cs`
- `Core/Parsing/CsvParser.cs`
- `Core/Catalog/GameDefinitionCatalog.cs`
- All section 19 Skill Definition files
- `ChoiceNodeDefinition.cs`
- `NodeTypeDefinition.cs`
- `NodeParamDefinition.cs`
- `SkillChoiceDefinition.cs`
- `UnitDefinition.cs`
- `MonsterDefinition.cs`
- `EnemyDefinition.cs`
- All four Stage Definition files
- `StatusDefinition.cs`

### Run

- `RunSessionModel.cs`
- `StageManager.cs`
- `PartyRoster.cs`
- `PrisonerInventory.cs`
- `RewardService.cs`
- `OfferingService.cs`
- `ManifestationService.cs`

### Units, Combat, And Spawn

- `UnitBaseModel.cs`
- `MonsterModel.cs`
- `EnemyModel.cs`
- `NexusModel.cs`
- `InGameCombatManager.cs`
- All six section 19 Action files
- All four Skill Runtime files
- All nine section 19 Execution files: `SkillTargeting` and the eight family Executors
- `SkillActorManager.cs`
- `ProjectileActor.cs`
- `LineAttackActor.cs`
- `AreaAttackActor.cs`
- `SingleAttackActor.cs`
- `BuffActor.cs`
- `StatusEffect.cs`
- `EffectManager.cs`
- `SpawnManager.cs`

These files may absorb responsibilities listed in sections 4 through 7. They are not renamed merely to reflect current implementation details.

## 4. Five Non-Section-19 Support Files To Consolidate

These five files are not holdovers. Their types and behavior move into an existing section 19 owner, then the separate source file and `.meta` are removed after compilation and reference checks.

| Current file | Section 19 owner | Consolidated responsibility |
|---|---|---|
| `Core/Definitions/CsvDefinition.cs` | `Core/Parsing/CsvParser.cs` | CSV row metadata, typed column access, and parser-created definition input |
| `Core/Definitions/Choices/MonsterModifierSkillChoiceDefinition.cs` | `Core/Definitions/Choices/SkillChoiceDefinition.cs` | Monster-specific reward Choice mapping data |
| `Core/Definitions/Units/CatalogMonsterDefinition.cs` | `Core/Definitions/Units/MonsterDefinition.cs` | Catalog order-to-monster mapping data |
| `Units/Models/CombatVector2.cs` | `Units/Models/UnitBaseModel.cs` | Engine-independent unit position and combat-vector value type |
| `Combat/Status/RuntimeCombatModifier.cs` | `Combat/Status/StatusEffect.cs` | Timed runtime combat modifier state |

Consolidation rules:

- Preserve public type names when current callers require them.
- Do not create replacement files for these five types.
- Keep the section 19 owner as the primary public type of the target file.
- Update compile-time references only where namespace changes are unavoidable.
- Remove the old `.cs` and `.meta` only after no source or serialized reference targets the old Script GUID.

## 5. Actor Migration And Consolidation

Section 19 permits only four Unit Actor files. Existing Actor behavior moves as follows:

| Current file | Target file | Action |
|---|---|---|
| `Presentation/Actors/UnitActorBehaviour.cs` | `Units/Actors/UnitActor.cs` | Move and rename; retain common Model, Transform, and damage-presentation boundary |
| `Presentation/Actors/MonsterActorBehaviour.cs` | `Units/Actors/MonsterActor.cs` | Move and rename; retain Monster scene binding |
| `Presentation/Actors/MonsterAnimationBehaviour.cs` | `Units/Actors/MonsterActor.cs` | Consolidate animation and death-frame behavior into Monster Actor |
| `Presentation/Actors/EnemyActorBehaviour.cs` | `Units/Actors/EnemyActor.cs` | Move and rename; retain Enemy scene binding |
| `Presentation/Actors/NexusActorBehaviour.cs` | `Units/Actors/NexusActor.cs` | Move and rename; retain Nexus scene binding |
| `Presentation/Actors/DamageNumberPopupBehaviour.cs` | `Units/Actors/UnitActor.cs` | Consolidate shared world-space damage popup lifecycle |
| `Presentation/Actors/SkillVisualActorBehaviour.cs` | `Combat/Effects/EffectManager.cs` | Consolidate visual handle binding and Transform synchronization |

Skill Actor runtime consolidation:

- Move `SkillActor`, `TimedSkillActor`, and `ScheduledSkillActor` type declarations into `SkillActorManager.cs`.
- `SkillActorManager` remains the central Tick and active/pending collection authority.
- `SkillActor` remains the common elapsed-time and completion contract.
- `TimedSkillActor` remains the duration-completion base used by Area, Line, Buff, and SingleAttack Actors.
- `ScheduledSkillActor` remains delayed and repeated execution used by Executors, effect graphs, and triggers.
- The Manager file owns these lifecycle types together; it does not absorb damage, targeting, graph interpretation, or skill learning.

Serialized safety:

- Move the `.meta` with the script that retains the primary component role.
- For a consolidated secondary component, copy its serialized fields and behavior into the target component before removing the old component.
- Inspect all Monster prefabs, Enemy prefabs, Nexus scene object, Skill visual prefabs, and current damage popup references.
- Remove the source component only after every affected object reports no Missing Script.

## 6. Bootstrap, Asset, Input, Effect, Spawn, And Stage Integration

The current Presentation scene and asset scripts do not remain as separate production files.

| Current file | Section 19 owner | Integration |
|---|---|---|
| `Presentation/Scene/NewCoreSceneRuntime.cs` | `Core/Bootstrap/GameBootstrap.cs` | Central initialization, runtime graph construction, central Tick entry, combat completion, and next-stage connection |
| `Presentation/Assets/NewCoreRuntimeCatalogAsset.cs` | `Core/Bootstrap/GameBootstrap.cs` and `Core/Catalog/GameDefinitionCatalog.cs` | Copy serialized CSV/resource references into bootstrap initialization; catalog remains immutable runtime data authority |
| `Presentation/Assets/RunStartSelectionAsset.cs` | `Core/Bootstrap/GameBootstrap.cs` | Move pending run-start selection into the bootstrap lifecycle; remove duplicate ScriptableObject state after both scenes use the new path |
| `Presentation/Scene/NewCoreInputController.cs` | `Combat/Actions/PlayerInputController.cs` | Integrate Unity input reading with manual command creation |
| `Presentation/Scene/NewCoreEffectView.cs` | `Combat/Effects/EffectManager.cs` | Integrate visual prefab creation, synchronization, and deletion |
| `Presentation/Scene/NewCoreSpawnController.cs` | `Spawn/SpawnManager.cs` | Integrate Actor prefab creation and Model binding |
| `Presentation/Scene/NewCoreStageController.cs` | `Run/StageManager.cs` and `UI/InGame/InGameUIManager.cs` | Stage state remains in StageManager; result-panel presentation moves to InGameUIManager |

Boundary rules:

- `GameBootstrap` is the one initialization boundary for required CSV assets, prefabs, scene references, and Manager connections.
- `GameDefinitionCatalog` remains immutable data authority and does not search scenes or update UI.
- `StageManager` owns stage, day, field unit, Gold, and DarkTrace state; it does not render panels.
- `SpawnManager` creates Models and Actors but does not decide manifestation success or stage progression.
- `EffectManager` is the single visual authority. It owns visual specifications, resource resolution, GameObject creation, active handles, Transform synchronization, and deletion.
- `SkillActorManager` owns effect lifetime timing and sends removal commands to `EffectManager` when an Actor completes.
- `SkillEffectGraphRuntime` may decide that an `EffectVisual` or `RuntimeEffectVisual` node has matched, choose the gameplay target, and send a neutral visual request. It does not instantiate resources, construct Unity objects, keep visual handles, synchronize Transforms, or choose a concrete visual-lifetime Actor.
- `EffectManager` does not evaluate Choice conditions, select combat targets, calculate damage, or read skill-learning state.
- `PlayerInputController` translates input into commands; it does not directly mutate unit state.

Visual request boundary:

```text
SkillEffectGraphRuntime
  → matched visual node, target position, direction, lifetime, authored visual values
EffectManager
  → visual specification, resource lookup, instance creation, handle ownership, synchronization, deletion
SkillActorManager
  → lifetime completion, then EffectManager removal command
```

Implementation rules:

- Remove `SkillEffectGraphRuntime.CreateVisual` as a visual-construction method.
- Replace it with a request-only path that passes authored visual values and resolved gameplay placement to `EffectManager`.
- Do not pass `ChoiceNodeDefinition` into `EffectManager`; the visual system must not depend on graph schema.
- If a neutral visual request value type is required, declare it inside `EffectManager.cs`; do not create another source file.
- Move `EffectVisualSpec` construction into `EffectManager`.
- Move `NewCoreEffectView.CreateInstance`, `ConfigureRuntimeVisual`, `SyncInstance`, and deletion behavior into `EffectManager`.
- Move `SkillVisualActorBehaviour` handle-to-Transform behavior into `EffectManager`.
- Do not register `BuffActor` from `SkillEffectGraphRuntime` only to time a visual. Route lifetime registration through `SkillActorManager`.

ScriptableObject removal gate:

1. Record every field and current value from both `.asset` instances.
2. Add exact replacement fields or runtime lookup paths to `GameBootstrap`.
3. Copy and inspect all CSV, Sprite, prefab, AnimatorController, and selected-monster references.
4. Change both scene call paths to the new bootstrap authority.
5. Confirm no source or serialized object references either old Script GUID.
6. Only then remove the old `.cs`, `.meta`, and obsolete `.asset` files.

## 7. UI Migration

Use only section 19 UI folders.

### Exact section 19 files

`NewCoreInGameUIController.cs` is replaced by:

- `UI/InGame/InGameUIManager.cs`
- `UI/InGame/RewardPanelController.cs`
- `UI/InGame/PrisonPanelController.cs`
- `UI/InGame/OfferingPanelController.cs`
- `UI/InGame/ManifestationPanelController.cs`

The original `NewCoreInGameUIController.cs.meta` GUID stays with `InGameUIManager.cs`. The four panel controllers receive new GUIDs because section 19 explicitly names them as separate scripts.

### Existing UI scripts reused as section 19 requires

| Current file | Target folder | Rule |
|---|---|---|
| `NewCoreMainMenuController.cs` | `UI/MainMenu/` | Move existing script; do not invent a new MainMenu script name |
| `NewCoreMonsterPanelUI.cs` | `UI/InGame/MonsterPanel/` | Move and adapt existing script |
| `NewCoreDamageMeterTracker.cs` | `UI/InGame/DamageMeter/` | Move existing tracker |
| `NewCoreDamageMeterUIController.cs` | `UI/InGame/DamageMeter/` | Move existing renderer |
| `NewCoreUtilityPanelController.cs` | `UI/InGame/UtilityPanel/` | Move existing Auto and time-scale UI |
| `NewCoreDebugUIController.cs` | `UI/InGame/Debug/` | Move existing Debug UI |

Current inventory has no dedicated Nexus UI script. Do not invent one. Keep the current Nexus display path in `InGameUIManager` until a real existing script or a separate user requirement establishes another owner.

UI rules:

- UI reads state and sends commands.
- UI does not directly change Model fields, currencies, cooldowns, or SkillBuckets.
- Existing hierarchy, layout, button order, portraits, and serialized values remain unchanged.
- Each button has one command path.
- Copy Inspector values to the four extracted panel components before removing old fields.

## 8. Unnecessary Defensive Code Convention

Unnecessary defensive code is prohibited.

Required rule:

```text
Validate once at the untrusted boundary.
After successful initialization, internal code trusts the established invariant.
```

Allowed validation boundaries:

- `CsvParser`: external CSV content, schema, primitive conversion, required ids, and duplicate ids.
- `GameBootstrap`: required scene objects, Inspector fields, CSV assets, prefabs, and runtime wiring.
- Public UI or Service entry points: actual user input and commands that can legally fail.
- Public APIs that are genuinely callable by code outside the initialized runtime graph.

Removal targets:

- Repeated `argument ?? throw new ArgumentNullException(...)` checks on internal constructors when `GameBootstrap` already proved the dependency exists.
- Repeated null checks before every internal call after a required reference was initialized.
- Range, enum, and collection checks repeated below a validated CSV or public-command boundary.
- Silent fallback values that replace missing CSV authority.
- Empty-list, default-object, temporary-object, or Legacy fallback paths that cannot be reached in the approved runtime.
- Guards retained only because a hypothetical future caller might misuse an internal type.
- Pass-through wrappers whose only purpose is another defensive layer.

Retention test:

A defensive check remains only when all four answers are concrete:

```text
Boundary = Which untrusted input reaches this check?
Failure  = Which current caller can provide the bad value?
Response = Why is this method the correct owner of the error?
Evidence = Which inspected source, scene, prefab, asset, or CSV proves the path?
```

If any answer is missing, remove the defensive code instead of documenting a hypothetical risk.

API rule:

- Reduce implementation-only constructors and methods to `internal` or `private` where possible.
- Do not keep a public API solely to justify defensive validation.
- Use `Try...` or `Can...` only for expected gameplay failure.
- Initialization invariant violations fail once at `GameBootstrap`; they are not rechecked throughout Tick execution.

## 9. Comment Convention

For files being moved or consolidated:

- Add one short Korean `/* */` responsibility comment immediately above the namespace.
- Add one truthful Korean `/* */` role comment immediately above every declared method and constructor.
- Comment purpose, authority, transition, or lifecycle reason.
- Do not translate the method name into Korean without adding information.
- Do not add comments to `.meta`, asmdef, asmref, CSV, scene, prefab, or `.asset` files.

Independent-file exception:

- Preserve the current comment state of the six independent Skill Execution files.
- When moving Actor and Support methods into their target owners, preserve or add truthful comments at the new declarations.

## 10. Assembly And Dependency Direction

No extra source folder may be created to preserve the current Presentation split.

Required dependency direction:

```text
UI and Unity-facing owners
        ↓
Run and Combat
        ↓
Core Definitions and Catalog
```

Rules:

- Definitions do not depend on UI.
- Run does not depend on UI.
- Combat does not depend on UI panels.
- UI may call public query and command APIs.
- If an existing asmdef prevents a section 19 integration, change the asmdef or asmref layout without creating a new source folder.
- Do not introduce circular assembly references.
- Remove the old `Presentation` root only after every moved script compiles from its section 19 owner.

## 11. Migration Phases

### M0 — Freeze Exact Before-State

- Record all production `.cs` paths and `.meta` GUIDs.
- Record namespaces, primary types, public/internal signatures, and SHA-256 values.
- Record scene, prefab, AnimatorController, and `.asset` Script GUID references.
- Record current serialized values for Runtime Catalog, Run Start Selection, scene runtime, actors, and UI.
- Confirm the five Skill support consolidation sources and six independent Skill Execution paths.
- Confirm Play Mode is not running before source migration.

Gate:

- `dotnet build` succeeds.
- Unity compilation completes.
- Unity Console has zero unexplained project errors.
- Do not run the 117 EditMode tests.

### M1 — Core Definition Consolidation

- Consolidate `CsvDefinition.cs` into `CsvParser.cs`.
- Consolidate `MonsterModifierSkillChoiceDefinition.cs` into `SkillChoiceDefinition.cs`.
- Consolidate `CatalogMonsterDefinition.cs` into `MonsterDefinition.cs`.
- Preserve exact CSV field names and parser output.
- Apply the defensive-code convention at the parser/catalog boundary.

Gate:

- Same CSV paths and Definition counts.
- Same catalog ids and reference validation.
- Build, Unity compile, and Console pass.

### M2 — Unit And Status Consolidation

- Consolidate `CombatVector2.cs` into `UnitBaseModel.cs`.
- Consolidate `RuntimeCombatModifier.cs` into `StatusEffect.cs`.
- Keep Unit health, shield, status, position, and runtime modifier authority unchanged.
- Remove redundant internal defensive checks only after the owning boundary is proven.

Gate:

- Public behavior and signatures remain compatible.
- Build, Unity compile, and Console pass.

### M3 — Combat Structure

- Keep all section 19 Action, Skill Runtime, family Executor, named Skill Actor, Status, and Effect files in their exact folders.
- Consolidate `SkillActor`, `TimedSkillActor`, and `ScheduledSkillActor` into `SkillActorManager.cs`.
- Consolidate `SkillTriggerSupport` into `SkillTriggerDispatcher.cs`.
- Consolidate `SkillNodeSupport` into `SkillExecutionRuntime.cs`.
- Keep the remaining six independent Skill Execution files at their current paths.
- Integrate `SkillVisualActorBehaviour` into `EffectManager.cs`.
- Move visual specification construction, resource instantiation, handle synchronization, and deletion into `EffectManager`.
- Reduce `SkillEffectGraphRuntime` visual handling to matched-node interpretation and a neutral visual request.
- Route visual lifetime registration through `SkillActorManager` instead of selecting `BuffActor` inside the graph runtime.
- Do not create Execution subfolders or collaborator scripts.
- Audit internal constructor guards and fallback branches against section 8.

Gate:

- The five old Skill support source files have no remaining production references.
- Exactly six independent Skill Execution files remain as section 19 file-name exceptions.
- `SkillEffectGraphRuntime` contains no Unity object creation, visual handle collection, Transform synchronization, or direct visual deletion.
- `EffectManager` is the only owner of visual creation, synchronization, active handles, and deletion.
- No new production folder or file exists outside section 19 plus the six independent files.
- Central Tick order remains unchanged.
- Build, Unity compile, and Console pass.

### M4 — Actors, Bootstrap, Input, Spawn, And Resources

- Move and consolidate Actor scripts according to section 5.
- Integrate scene and asset scripts according to section 6.
- Preserve or explicitly replace every serialized Script GUID.
- Copy all Inspector and ScriptableObject values before deleting source components or assets.
- Reconnect current prefabs, scenes, sprites, AnimatorControllers, and CSV TextAssets.

Gate:

- Zero Missing Scripts.
- Every recorded GUID is preserved or has an inspected replacement.
- Every recorded serialized value is preserved.
- Build, Unity compile, and Console pass.

### M5 — UI

- Move existing reusable UI scripts into section 19 UI folders.
- Split `NewCoreInGameUIController` only into the five exact section 19 scripts.
- Integrate stage-result presentation into `InGameUIManager`.
- Do not create a Nexus UI script because none currently exists.
- Preserve button commands, hierarchy, layout, portraits, and Inspector values.

Gate:

- Every button has one command path.
- UI contains no direct writes to Model, currency, cooldown, or SkillBucket state.
- Zero Missing Scripts.
- Build, Unity compile, and Console pass.

### M6 — Remove Old Presentation Paths

- Confirm no production script remains under `Presentation`.
- Confirm no source dependency points to Legacy.
- Confirm all non-independent production files either match section 19 or were consolidated into a section 19 owner.
- Remove obsolete source `.cs`, `.meta`, and `.asset` files only after exact reference scans pass.
- Run final comment and defensive-code audits.

Gate:

- Final production roots match section 19.
- Only the six listed independent Skill Execution files remain as section 19 file-name exceptions.
- No unapproved new production folders or collaborator scripts.
- No redundant defensive checks below validated initialization boundaries.
- `dotnet build` succeeds with zero errors.
- Unity compilation succeeds.
- Unity Console has zero unexplained project errors.
- Do not run the 117 EditMode tests.
- User performs Play Mode verification.

## 12. Acceptance Criteria

Structure:

- Production roots are exactly `Core`, `Run`, `Units`, `Combat`, `Spawn`, and `UI`.
- No production `Presentation` root remains.
- No `Contracts`, `Internal`, `Unity`, `Planning`, `Graphs`, `Triggers`, `Targeting`, or `Calculation` production folders exist.
- Section 19 exact file names are used where specified.
- Existing UI script names are retained where section 19 says to reuse existing scripts.
- Only the six independent Skill Execution scripts remain outside the section 19 file list.

Responsibility:

- Definitions preserve CSV terminology.
- `GameDefinitionCatalog` owns immutable game data.
- `StageManager` owns stage, day, field units, Gold, and DarkTrace.
- `PartyRoster` owns party order.
- `PrisonerInventory` owns prisoners.
- Unit Models own mutable unit state.
- `InGameCombatManager` coordinates combat results.
- Skill Buckets own learning and cooldown state.
- Skill Actors own skill lifecycle.
- `SkillActorManager.cs` contains the common, timed, and scheduled Skill Actor lifecycle types and owns their central Tick collections.
- `SkillExecutionRuntime.cs` contains node-support classification and validation.
- `SkillTriggerDispatcher.cs` contains trigger validation and typed column reading.
- `SkillEffectGraphRuntime` decides when and where an effect-graph visual request occurs but owns no visual objects.
- `EffectManager` exclusively owns visual specification construction, resource instantiation, active handles, synchronization, and deletion.
- UI reads state and sends commands only.

Compatibility:

- Current gameplay rules and numeric values remain unchanged.
- Existing CSV files remain unchanged.
- Current scene, prefab, Sprite, AnimatorController, and `.asset` references are preserved or explicitly migrated.
- Zero Missing Scripts.
- No Legacy dependency.
- Central Tick order remains unchanged.

Code convention:

- Validation occurs once at an actual untrusted boundary.
- Internal runtime paths do not repeat already-proven null, range, enum, id, or collection checks.
- No arbitrary fallback replaces CSV or initialized runtime authority.
- No dead branch, speculative API, temporary object, or pass-through wrapper remains.
- Every retained defensive check has Boundary, Failure, Response, and Evidence.

Verification:

- Each phase passes `dotnet build`, Unity compilation, and Console checks.
- The 117 EditMode tests are not run.
- Codex does not run Play Mode.
- User performs final Play Mode verification.

## 13. Code Builder Handoff

Selected tracks:

- Structure
- Refactoring
- Quality
- Verification
- UI during M5

Builder must execute M0 through M6 in order.

For each phase report:

- exact old and new paths;
- moved `.meta` GUIDs;
- consolidated types and deleted source files;
- namespace and API changes;
- serialized reference migration;
- defensive checks removed or retained with evidence;
- comment coverage;
- build result;
- Unity compile result;
- Console result;
- remaining user Play Mode checks.

Do not widen the structure to solve an implementation inconvenience. If strict section 19 integration creates an unresolved assembly, serialization, or Unity component constraint, stop that exact file migration, record the inspected blocker, and leave the current file intact until the user decides.
