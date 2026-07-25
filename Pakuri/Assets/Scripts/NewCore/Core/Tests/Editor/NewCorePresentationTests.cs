using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pakuri.NewCore.Combat;
using Pakuri.NewCore.Combat.Actions;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Combat.Skills.Actors;
using Pakuri.NewCore.Combat.Skills.Execution;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Run.Services;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.UI.InGame;
using Pakuri.NewCore.UI.InGame.DamageMeter;
using Pakuri.NewCore.UI.InGame.Debug;
using Pakuri.NewCore.UI.InGame.MonsterPanel;
using Pakuri.NewCore.UI.InGame.UtilityPanel;
using Pakuri.NewCore.UI.MainMenu;
using Pakuri.NewCore.Units.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using TMPro;
using NewCoreRuntimeCatalogAsset = Pakuri.NewCore.Bootstrap.GameBootstrap;
using RunStartSelectionAsset = Pakuri.NewCore.Bootstrap.GameBootstrap;
using NewCoreSceneRuntime = Pakuri.NewCore.Bootstrap.GameBootstrap;
using NewCoreInputController = Pakuri.NewCore.Combat.Actions.PlayerInputController;
using NewCoreEffectView = Pakuri.NewCore.Combat.Effects.EffectManager;
using NewCoreSpawnController = Pakuri.NewCore.Spawn.SpawnManager;
using NewCoreStageController = Pakuri.NewCore.Run.StageManager;
using UnitActorBehaviour = Pakuri.NewCore.Units.Actors.UnitActor;
using MonsterActorBehaviour = Pakuri.NewCore.Units.Actors.MonsterActor;
using MonsterAnimationBehaviour = Pakuri.NewCore.Units.Actors.MonsterActor;
using EnemyActorBehaviour = Pakuri.NewCore.Units.Actors.EnemyActor;
using NexusActorBehaviour = Pakuri.NewCore.Units.Actors.NexusActor;

/* NewCore scene·prefab·UI·effect 표현 연결을 검증한다. */
namespace Pakuri.NewCore.Tests
{
    public class NewCorePresentationTests
    {
        private const string CatalogPath =
            "Assets/Resources/Pakuri/NewCore/RuntimeCatalog.asset";
        private const string SelectionPath =
            "Assets/Resources/Pakuri/NewCore/RunStartSelection.asset";
        private const string RunScenePath =
            "Assets/Scenes/NewScene/NewRunScene.unity";
        private const string MainMenuScenePath =
            "Assets/Scenes/NewScene/NewMainMenu.unity";

        private static readonly string[] MonsterPrefabPaths =
        {
            "Assets/Prefab/Monster/Ariel_Unit.prefab",
            "Assets/Prefab/Monster/Eve_Unit.prefab",
            "Assets/Prefab/Monster/Rin_Unit.prefab",
            "Assets/Prefab/Monster/Sein_Unit.prefab",
            "Assets/Prefab/Monster/Vega_Unit.prefab"
        };

        private static readonly string[] EnemyPrefabPaths =
        {
            "Assets/Prefab/Enemy/Stage1/Stage1_Achor.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_Karin.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_Priest_Unit.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_Rogue_Unit.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_Shield.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_ShieldKing.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_Warrior_King.prefab",
            "Assets/Prefab/Enemy/Stage1/Stage1_Warrior_Unit.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-arsen.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-dark-assassin.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-drake.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-ethan.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-fire-dragon-slayer.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-holy-priest.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-ice-guard.prefab",
            "Assets/Prefab/Enemy/Stage2/stage2-lightning-scout.prefab"
        };

        private static readonly string[] RuntimeSkillVisualPrefabPaths =
        {
            "Assets/Prefab/Skill/Rin/Rin_D.prefab",
            "Assets/Prefab/Skill/Rin/Rin_E.prefab"
        };

        private static readonly string[]
            MigratedUnreferencedSkillVisualPrefabPaths =
        {
            "Assets/Legacy/Skill 1/Ariel/Airel_A.prefab",
            "Assets/Legacy/Skill 1/Eve/Eve_A.prefab"
        };

        [Test]
        /* RuntimeCatalogBuildsCompleteRetainedDefinitionSet 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void RuntimeCatalogBuildsCompleteRetainedDefinitionSet()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            Assert.That(catalog, Is.Not.Null);
            Assert.That(
                AssetDatabase.GetMainAssetTypeAtPath(CatalogPath),
                Is.EqualTo(typeof(NewCoreRuntimeCatalogAsset)));
            Assert.That(catalog.StageDay, Is.Not.Null);
            Assert.That(catalog.StageEncounter, Is.Not.Null);
            Assert.That(catalog.StageReward, Is.Not.Null);

            var definitions = catalog.Catalog;
            Assert.That(definitions.SourceFileCount, Is.EqualTo(42));
            Assert.That(definitions.AllDefinitions.Count, Is.EqualTo(1836));
            Assert.That(definitions.Monsters.Count, Is.EqualTo(5));
            Assert.That(definitions.Enemies.Count, Is.EqualTo(16));

            Assert.That(catalog.Sprites, Is.Not.Empty);
            Assert.That(
                catalog.TryGetSprite(
                    catalog.Sprites[0].AssetPath,
                    out var sprite),
                Is.True);
            Assert.That(sprite, Is.SameAs(catalog.Sprites[0].Asset));
            Assert.That(catalog.Prefabs, Is.Not.Empty);
            Assert.That(
                catalog.TryGetPrefab(
                    catalog.Prefabs[0].AssetPath,
                    out var prefab),
                Is.True);
            Assert.That(prefab, Is.SameAs(catalog.Prefabs[0].Asset));
            Assert.That(catalog.AnimatorControllers, Is.Not.Empty);
            Assert.That(
                catalog.TryGetAnimatorController(
                    catalog.AnimatorControllers[0].AssetPath,
                    out var controller),
                Is.True);
            Assert.That(
                controller,
                Is.SameAs(catalog.AnimatorControllers[0].Asset));
        }

        [Test]
        /* RunStartSelectionUsesNewCoreAssetType 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void RunStartSelectionUsesNewCoreAssetType()
        {
            var selection = AssetDatabase.LoadAssetAtPath<
                RunStartSelectionAsset>(SelectionPath);
            Assert.That(selection, Is.Not.Null);
            Assert.That(
                AssetDatabase.GetMainAssetTypeAtPath(SelectionPath),
                Is.EqualTo(typeof(RunStartSelectionAsset)));
        }

        [Test]
        /* RunSceneHasCompleteNewCoreComponentWiring 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void RunSceneHasCompleteNewCoreComponentWiring()
        {
            var scene = EditorSceneManager.OpenPreviewScene(RunScenePath);
            try
            {
                AssertNoMissingScripts(scene, RunScenePath);
                AssertNoPreviousAuthorityTypes(scene, RunScenePath);

                var runtime = Single<NewCoreSceneRuntime>(scene);
                var stage = Single<NewCoreStageController>(scene);
                var spawn = Single<NewCoreSpawnController>(scene);
                var effects = Single<NewCoreEffectView>(scene);
                var input = Single<NewCoreInputController>(scene);
                var ui = Single<InGameUIManager>(scene);
                var reward = Single<RewardPanelController>(scene);
                var prison = Single<PrisonPanelController>(scene);
                var offering = Single<OfferingPanelController>(scene);
                var manifestation =
                    Single<ManifestationPanelController>(scene);
                var monsterPanel = Single<NewCoreMonsterPanelUI>(scene);
                var meter = Single<NewCoreDamageMeterUIController>(scene);
                var tracker = Single<NewCoreDamageMeterTracker>(scene);
                var utility = Single<NewCoreUtilityPanelController>(scene);
                var nexus = Single<NexusActorBehaviour>(scene);

                AssertRequiredReferences(
                    runtime,
                    "playerCombatControl",
                    "effectManager");
                AssertRequiredReferences(
                    stage,
                    "combatManager",
                    "unitSpawnManager",
                    "stageDayCsv",
                    "stageEncounterCsv",
                    "stageRewardCsv",
                    "nexusActor");
                AssertRequiredReferences(
                    spawn,
                    "combatManager",
                    "playerSpawnPoint",
                    "enemySpawnPoint",
                    "runtimeEnemyRoot",
                    "runtimeMonsterRoot",
                    "arielUnitPrefab",
                    "eveUnitPrefab",
                    "rinUnitPrefab",
                    "seinUnitPrefab",
                    "vegaUnitPrefab");
                AssertArraySize(spawn, "enemyPrefabBindings", 16);
                AssertRequiredReferences(effects, "runtimeSkillRoot");
                AssertRequiredReferences(
                    ui,
                    "combatManager",
                    "rewardPanelController",
                    "prisonPanelController",
                    "offeringPanelController",
                    "manifestationPanelController",
                    "winPanel",
                    "defeatPanel");
                AssertRequiredReferences(reward, "combatManager");
                AssertRequiredReferences(
                    prison,
                    "combatManager",
                    "arielPrisonPortrait",
                    "evePrisonPortrait",
                    "rinPrisonPortrait",
                    "seinPrisonPortrait",
                    "vegaPrisonPortrait");
                AssertRequiredReferences(offering, "combatManager");
                AssertRequiredReferences(
                    manifestation,
                    "combatManager",
                    "arielPrisonPortrait",
                    "evePrisonPortrait",
                    "rinPrisonPortrait",
                    "seinPrisonPortrait",
                    "vegaPrisonPortrait");
                AssertRequiredReferences(
                    monsterPanel,
                    "monsterPanelRoot",
                    "stageManager",
                    "unitSpawnManager",
                    "combatManager");
                AssertRequiredReferences(
                    meter,
                    "openButton",
                    "meterRoot",
                    "closeButton",
                    "stageManager",
                    "unitSpawnManager",
                    "tracker");
                AssertRequiredReferences(tracker, "combatManager");
                AssertRequiredReferences(
                    utility,
                    "playerCombatControl",
                    "autoButton",
                    "timeButton",
                    "onePointFiveIndicator",
                    "twoTimesIndicator");
                Assert.That(nexus.MaxHealth, Is.GreaterThan(0f));
                Assert.That(input, Is.Not.Null);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        /* MainMenuSceneUsesNewCoreControllerAndRetainedReferences 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MainMenuSceneUsesNewCoreControllerAndRetainedReferences()
        {
            var scene = EditorSceneManager.OpenPreviewScene(MainMenuScenePath);
            try
            {
                AssertNoMissingScripts(scene, MainMenuScenePath);
                AssertNoPreviousAuthorityTypes(scene, MainMenuScenePath);
                var menu = Single<NewCoreMainMenuController>(scene);
                AssertRequiredReferences(
                    menu,
                    "introPanel",
                    "mainMenuPanel",
                    "monsterSelectPanel",
                    "introGameStartButton",
                    "runButton",
                    "monsterSelectGameStartButton",
                    "arielButton",
                    "eveButton",
                    "seinButton",
                    "vegaButton",
                    "rinButton");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        /* ActiveUnitAndSkillPrefabsUseNewCoreActors 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void ActiveUnitAndSkillPrefabsUseNewCoreActors()
        {
            for (var index = 0; index < MonsterPrefabPaths.Length; index++)
            {
                AssertPrefabHas<MonsterActorBehaviour>(
                    MonsterPrefabPaths[index]);
                AssertPrefabHas<MonsterAnimationBehaviour>(
                    MonsterPrefabPaths[index]);
            }

            for (var index = 0; index < EnemyPrefabPaths.Length; index++)
            {
                AssertPrefabHas<EnemyActorBehaviour>(
                    EnemyPrefabPaths[index]);
            }

            for (var index = 0;
                index < RuntimeSkillVisualPrefabPaths.Length;
                index++)
            {
                AssertPrefabIsClean(
                    RuntimeSkillVisualPrefabPaths[index]);
            }

            for (var index = 0;
                index < MigratedUnreferencedSkillVisualPrefabPaths.Length;
                index++)
            {
                AssertPrefabIsClean(
                    MigratedUnreferencedSkillVisualPrefabPaths[index]);
            }
        }

        [Test]
        /* EffectViewConsumesPrefabSpriteAnimatorScaleAndSorting 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EffectViewConsumesPrefabSpriteAnimatorScaleAndSorting()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var effectRoot = new GameObject("EffectRoot");
            var viewObject = new GameObject("EffectView");
            try
            {
                var view = viewObject.AddComponent<NewCoreEffectView>();
                var serialized = new SerializedObject(view);
                serialized.FindProperty("runtimeSkillRoot")
                    .objectReferenceValue = effectRoot.transform;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                var effects = view;
                var spritePath =
                    "Assets/Image/Monster/ariel/SkillEffect/"
                    + "ChatGPT Image 2026년 5월 15일 오후 05_37_22-Photoroom 1.png";
                var controllerPath =
                    "Assets/Image/Monster/ariel/SkillEffect/"
                    + "ChatGPT Image 2026년 5월 15일 오후 05_37_22-Photoroom 1.controller";
                effects.Create(
                    new EffectVisualSpec(
                        string.Empty,
                        spritePath,
                        controllerPath,
                        0.4817f,
                        0f,
                        0f,
                        0f,
                        7),
                    default,
                    new CombatVector2(0f, 1f));
                effects.Create(
                    new EffectVisualSpec(
                        "Assets/Prefab/Skill/Rin/Rin_D.prefab",
                        string.Empty,
                        string.Empty,
                        1f,
                        0f,
                        0f,
                        0f,
                        0),
                    default,
                    default);

                view.BindVisualRuntime(
                    effectRoot.transform,
                    path =>
                    {
                        if (catalog.TryGetPrefab(path, out var asset))
                        {
                            return asset;
                        }
                        return null;
                    },
                    path =>
                    {
                        if (catalog.TryGetSprite(path, out var asset))
                        {
                            return asset;
                        }
                        return null;
                    },
                    path =>
                    {
                        if (catalog.TryGetAnimatorController(
                                path,
                                out var asset))
                        {
                            return asset;
                        }
                        return null;
                    });
                view.SyncVisuals();

                Assert.That(
                    effectRoot.transform.childCount,
                    Is.EqualTo(2));
                var renderer = effectRoot
                    .GetComponentsInChildren<SpriteRenderer>(true)
                    .Single(component =>
                        component.sprite != null
                        && AssetDatabase.GetAssetPath(component.sprite)
                        == spritePath);
                Assert.That(renderer.sortingOrder, Is.EqualTo(7));
                Assert.That(
                    renderer.transform.localScale.x,
                    Is.EqualTo(0.4817f).Within(0.0001f));
                Assert.That(
                    renderer.transform.right.y,
                    Is.EqualTo(1f).Within(0.0001f));
                var animator = renderer.GetComponent<Animator>();
                Assert.That(animator, Is.Not.Null);
                Assert.That(
                    AssetDatabase.GetAssetPath(
                        animator.runtimeAnimatorController),
                    Is.EqualTo(controllerPath));
                Assert.That(
                    effectRoot.GetComponentsInChildren<Transform>(true)
                        .Any(transform =>
                            transform.name.StartsWith(
                                "Rin_D",
                                StringComparison.Ordinal)),
                    Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(viewObject);
                UnityEngine.Object.DestroyImmediate(effectRoot);
            }
        }

        [Test]
        /* EnemyActorDisablesCollidersWhenItsModelIsDefeated 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EnemyActorDisablesCollidersWhenItsModelIsDefeated()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.Catalog;
            var definition = catalog.GetEnemy("stage1-swordsman");
            var model = new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(
                    definition.passive_id));
            var instance = new GameObject("EnemyActor");
            try
            {
                var collider =
                    instance.AddComponent<BoxCollider2D>();
                var actor =
                    instance.AddComponent<EnemyActorBehaviour>();
                actor.Bind(model);
                model.ApplyDamage(model.MaximumHealth);

                actor.SyncFromModel();

                Assert.That(actor.IsDefeated, Is.True);
                Assert.That(collider.enabled, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        /* EnemyActorReceivesSkillActivationPresentation 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EnemyActorReceivesSkillActivationPresentation()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.Catalog;
            var definition =
                catalog.GetEnemy("stage1-guardian-captain");
            var enemy = new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(
                    definition.passive_id));
            var root = new GameObject("Guardian");
            try
            {
                var actor = root.AddComponent<EnemyActorBehaviour>();
                actor.Bind(enemy);
                actor.PlayAttack();
                Assert.That(
                    actor.AttackPresentationCount,
                    Is.EqualTo(1));

                enemy.ApplyDamage(enemy.MaximumHealth);
                actor.SyncFromModel();
                actor.PlayAttack();
                Assert.That(
                    actor.AttackPresentationCount,
                    Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        /* EnemyActorDisablesCollidersAfterNexusContact 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void EnemyActorDisablesCollidersAfterNexusContact()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.Catalog;
            var definition = catalog.GetEnemy("stage1-swordsman");
            var model = new EnemyModel(
                definition,
                catalog.GetSkill(definition.skill_slot_a_id),
                catalog.GetSkill(definition.skill_slot_b_id),
                (PassiveDefinition)catalog.GetSkill(
                    definition.passive_id));
            var instance = new GameObject("EnemyActor");
            try
            {
                var collider =
                    instance.AddComponent<BoxCollider2D>();
                var actor =
                    instance.AddComponent<EnemyActorBehaviour>();
                actor.Bind(model);
                model.MarkNexusContact();

                actor.SyncFromModel();

                Assert.That(actor.IsDefeated, Is.True);
                Assert.That(collider.enabled, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        /* TerminalEnemyActorDoesNotRegrowAcrossSpawnSyncPasses 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void TerminalEnemyActorDoesNotRegrowAcrossSpawnSyncPasses()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.Catalog;
            var spawns = NewCoreTestFactory.CreateSpawnManager(
                catalog,
                _ => 0,
                () => 0f);
            var initialMonster = spawns.CreateMonsterModel(
                catalog.GetMonster("ariel"),
                false);
            var firstDay = catalog.StageDays.Values
                .Where(day => day.stage.HasValue && day.day.HasValue)
                .OrderBy(day => day.stage.Value)
                .ThenBy(day => day.day.Value)
                .First();
            var session = new RunSessionModel(
                "stage" + firstDay.stage.Value,
                firstDay.day.Value,
                firstDay.encounter_id,
                new PartyRoster(initialMonster),
                new PrisonerInventory());
            var stage = NewCoreTestFactory.CreateStageManager(
                session,
                catalog,
                0,
                0);
            stage.ConfigureSpawnManager(spawns);
            stage.StartCurrentDay();
            var enemy = spawns.SpawnedEnemies[0].Model;

            var host = new GameObject("RuntimeHost");
            var playerPoint = new GameObject("PlayerPoint");
            var enemyPoint = new GameObject("EnemyPoint");
            var monsterRoot = new GameObject("MonsterRoot");
            var enemyRoot = new GameObject("EnemyRoot");
            var monsterTemplate = new GameObject("MonsterTemplate");
            var enemyTemplate = new GameObject("EnemyTemplate");
            try
            {
                var runtime = host.AddComponent<NewCoreSceneRuntime>();
                var spawnView = spawns;
                monsterTemplate.AddComponent<MonsterActorBehaviour>();
                enemyTemplate.AddComponent<BoxCollider2D>();
                enemyTemplate.AddComponent<EnemyActorBehaviour>();

                var effects =
                    NewCoreTestFactory.CreateComponent<EffectManager>();
                var actors = new SkillActorManager(effects);
                var targeting = new SkillTargeting(_ => 0);
                var execution = new SkillExecutionRuntime(
                    catalog,
                    targeting,
                    actors,
                    effects,
                    () => 0f);
                var combat = new InGameCombatManager(() => 0f, execution);
                var input = NewCoreTestFactory
                    .CreateComponent<PlayerInputController>();
                var actions = new InGameActionManager(
                    stage,
                    () => true,
                    () => { },
                    input,
                    actors,
                    execution.Triggers,
                    combat);
                var nexus = new NexusModel(20f);

                SetProperty(runtime, "Stage", stage);
                SetProperty(runtime, "Spawns", spawns);
                SetProperty(runtime, "Combat", combat);
                SetProperty(runtime, "Nexus", nexus);
                SetField(runtime, "targeting", targeting);
                SetField(runtime, "actions", actions);

                var serialized = new SerializedObject(spawnView);
                serialized.FindProperty("combatManager")
                    .objectReferenceValue = runtime;
                serialized.FindProperty("playerSpawnPoint")
                    .objectReferenceValue = playerPoint.transform;
                serialized.FindProperty("enemySpawnPoint")
                    .objectReferenceValue = enemyPoint.transform;
                serialized.FindProperty("runtimeMonsterRoot")
                    .objectReferenceValue = monsterRoot.transform;
                serialized.FindProperty("runtimeEnemyRoot")
                    .objectReferenceValue = enemyRoot.transform;
                foreach (var field in new[]
                {
                    "arielUnitPrefab",
                    "eveUnitPrefab",
                    "rinUnitPrefab",
                    "seinUnitPrefab",
                    "vegaUnitPrefab"
                })
                {
                    serialized.FindProperty(field).objectReferenceValue =
                        monsterTemplate;
                }

                var bindings =
                    serialized.FindProperty("enemyPrefabBindings");
                bindings.arraySize = 1;
                var binding = bindings.GetArrayElementAtIndex(0);
                binding.FindPropertyRelative("enemyId").stringValue =
                    enemy.EnemyDefinition.enemy_id;
                binding.FindPropertyRelative("prefab").objectReferenceValue =
                    enemyTemplate;
                serialized.ApplyModifiedPropertiesWithoutUndo();

                spawnView.BindScene(runtime);
                spawnView.SyncNewSpawns();
                Assert.That(enemyRoot.transform.childCount, Is.EqualTo(1));
                Assert.That(
                    spawnView.TryGetActor(enemy, out var actor),
                    Is.True);
                var enemyActor = (EnemyActorBehaviour)actor;
                var controllerCount = GetListCount(actions, "enemies");

                enemy.MarkNexusContact();
                spawnView.SyncActors();
                UnityEngine.Object.DestroyImmediate(enemyActor.gameObject);

                Assert.That(enemyRoot.transform.childCount, Is.Zero);
                Assert.That(
                    spawnView.TryGetActor(enemy, out _),
                    Is.False);

                for (var pass = 0; pass < 2; pass++)
                {
                    spawnView.SyncNewSpawns();
                    spawnView.SyncActors();
                    Assert.That(
                        enemyRoot.transform.childCount,
                        Is.Zero,
                        "pass " + pass);
                    Assert.That(
                        spawnView.TryGetActor(enemy, out _),
                        Is.False,
                        "pass " + pass);
                    Assert.That(
                        GetListCount(actions, "enemies"),
                        Is.EqualTo(controllerCount),
                        "pass " + pass);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(host);
                UnityEngine.Object.DestroyImmediate(playerPoint);
                UnityEngine.Object.DestroyImmediate(enemyPoint);
                UnityEngine.Object.DestroyImmediate(monsterRoot);
                UnityEngine.Object.DestroyImmediate(enemyRoot);
                UnityEngine.Object.DestroyImmediate(monsterTemplate);
                UnityEngine.Object.DestroyImmediate(enemyTemplate);
            }
        }

        [Test]
        /* OfferingWritesOwnerAndExactDescriptionIntoDedicatedFields 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void OfferingWritesOwnerAndExactDescriptionIntoDedicatedFields()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.Catalog;
            var root = new GameObject(
                "Choice",
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            try
            {
                TMP_Text skillName = CreateText(root.transform, "SkillName");
                TMP_Text summary = CreateText(root.transform, "Summary");
                TMP_Text description = CreateText(root.transform, "Desc");
                summary.text = "summary-sentinel";
                ConstructorInfo constructor = typeof(OfferingCandidate)
                    .GetConstructor(
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[]
                        {
                            typeof(OfferingCandidateKind),
                            typeof(SkillDefinition),
                            typeof(SkillChoiceDefinition)
                        },
                        null);
                Assert.That(constructor, Is.Not.Null);
                MethodInfo bind = typeof(OfferingPanelController)
                    .GetMethod(
                        "BindCandidate",
                        BindingFlags.Static | BindingFlags.NonPublic);
                Assert.That(bind, Is.Not.Null);

                SkillDefinition skill = catalog.GetSkill("ariel-b");
                var skillCandidate = (OfferingCandidate)
                    constructor.Invoke(new object[]
                    {
                        OfferingCandidateKind.ActiveSkill,
                        skill,
                        null
                    });
                bind.Invoke(
                    null,
                    new object[]
                    {
                        root.GetComponent<Button>(),
                        skillCandidate,
                        "아리엘"
                    });
                Assert.That(
                    skillName.text,
                    Is.EqualTo(skill.display_name));
                Assert.That(
                    description.text,
                    Is.EqualTo(skill.description_text));
                Assert.That(summary.text, Is.EqualTo("아리엘"));

                SkillChoiceDefinition choice =
                    catalog.GetChoice("ariel-a-trait-1");
                var choiceCandidate = (OfferingCandidate)
                    constructor.Invoke(new object[]
                    {
                        OfferingCandidateKind.ActiveEnhancement,
                        null,
                        choice
                    });
                bind.Invoke(
                    null,
                    new object[]
                    {
                        root.GetComponent<Button>(),
                        choiceCandidate,
                        "아리엘"
                    });
                Assert.That(
                    skillName.text,
                    Is.EqualTo(choice.title));
                Assert.That(
                    description.text,
                    Is.EqualTo(choice.description_text));
                Assert.That(summary.text, Is.EqualTo("아리엘"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        /* DamagePopupsRemainIndependentAndCapOnlyTheOldest 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void DamagePopupsRemainIndependentAndCapOnlyTheOldest()
        {
            var root = new GameObject("Actor");
            var templateObject = new GameObject(
                "Damage",
                typeof(TextMesh));
            templateObject.transform.SetParent(root.transform, false);
            var template = templateObject.GetComponent<TextMesh>();
            template.color = Color.red;
            var popups =
                root.AddComponent<MonsterActorBehaviour>();
            try
            {
                typeof(UnitActorBehaviour)
                    .GetMethod(
                        "InitializeDamagePopups",
                        BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(popups, new object[] { template });
                popups.ShowDamage(10f);
                popups.ShowDamage(20f);
                popups.ShowDamage(30f);

                Assert.That(
                    popups.ActiveDamagePopupCount,
                    Is.EqualTo(3));
                TextMesh[] active = root.GetComponentsInChildren<
                    TextMesh>(true)
                    .Where(text =>
                        text.gameObject.name == "Damage_Popup")
                    .OrderBy(text => text.transform.localPosition.y)
                    .ToArray();
                Assert.That(active, Has.Length.EqualTo(3));
                Assert.That(
                    active.Select(text => text.text),
                    Is.EqualTo(new[]
                    {
                        "10(Damage)",
                        "20(Damage)",
                        "30(Damage)"
                    }));
                Assert.That(
                    active[1].transform.localPosition.y
                        - active[0].transform.localPosition.y,
                    Is.EqualTo(0.18f).Within(0.0001f));

                float[] startingY = active
                    .Select(text => text.transform.localPosition.y)
                    .ToArray();
                popups.TickDamagePopups(0.5f);
                active = root.GetComponentsInChildren<TextMesh>(true)
                    .Where(text =>
                        text.gameObject.name == "Damage_Popup")
                    .OrderBy(text => text.text)
                    .ToArray();
                Assert.That(active, Has.Length.EqualTo(3));
                for (var index = 0; index < active.Length; index++)
                {
                    Assert.That(
                        active[index].transform.localPosition.y,
                        Is.EqualTo(startingY[index] + 0.5f)
                            .Within(0.0001f));
                    Assert.That(
                        active[index].color.a,
                        Is.EqualTo(0.5f).Within(0.003f));
                }

                popups.ShowDamage(40f);
                TextMesh newest = root.GetComponentsInChildren<
                        TextMesh>(true)
                    .Single(text => text.text == "40(Damage)");
                Assert.That(newest.color.a, Is.EqualTo(1f));
                popups.TickDamagePopups(0.51f);
                Assert.That(
                    popups.ActiveDamagePopupCount,
                    Is.EqualTo(1));
                Assert.That(newest, Is.Not.Null);
                Assert.That(
                    newest.color.a,
                    Is.EqualTo(0.49f).Within(0.003f));
                popups.TickDamagePopups(0.5f);
                Assert.That(
                    popups.ActiveDamagePopupCount,
                    Is.Zero);

                for (var index = 0; index < 13; index++)
                {
                    popups.ShowDamage(40f + index);
                }
                Assert.That(
                    popups.ActiveDamagePopupCount,
                    Is.EqualTo(12));
                Assert.That(
                    root.GetComponentsInChildren<TextMesh>(true)
                        .Count(text =>
                            text.gameObject.name == "Damage_Popup"),
                    Is.EqualTo(12));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        /* MonsterDeathFreezeSamplesLastFrameAndStopsAnimator 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MonsterDeathFreezeSamplesLastFrameAndStopsAnimator()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<
                GameObject>("Assets/Prefab/Monster/Rin_Unit.prefab");
            Assert.That(prefab, Is.Not.Null);
            GameObject instance =
                UnityEngine.Object.Instantiate(prefab);
            try
            {
                MonsterAnimationBehaviour animation =
                    instance.GetComponentInChildren<
                        MonsterAnimationBehaviour>(true);
                Animator animator =
                    instance.GetComponentInChildren<Animator>(true);
                Assert.That(animation, Is.Not.Null);
                Assert.That(animator, Is.Not.Null);
                MethodInfo awake = typeof(
                        MonsterAnimationBehaviour)
                    .GetMethod(
                        "Awake",
                        BindingFlags.Instance
                            | BindingFlags.NonPublic);
                Assert.That(awake, Is.Not.Null);
                awake.Invoke(animation, null);
                animation.PlayDeath();
                Assert.That(animation.IsDead, Is.True);
                animation.PlayDeath();
                MethodInfo freeze = typeof(
                        MonsterAnimationBehaviour)
                    .GetMethod(
                        "FreezeDeath",
                        BindingFlags.Instance
                            | BindingFlags.NonPublic);
                Assert.That(freeze, Is.Not.Null);
                var routine = (IEnumerator)freeze.Invoke(
                    animation,
                    new object[] { 0f });
                Assert.That(routine.MoveNext(), Is.True);
                Assert.That(routine.MoveNext(), Is.False);
                Assert.That(animation.IsDeathFrozen, Is.True);
                Assert.That(animator.speed, Is.Zero);
                Assert.That(
                    animator.GetCurrentAnimatorStateInfo(0)
                        .normalizedTime,
                    Is.EqualTo(0.999f).Within(0.01f));
                animation.PlayRandomAttack();
                animation.PlayHit();
                Assert.That(animator.speed, Is.Zero);
                animation.ReviveToIdle();
                Assert.That(animation.IsDead, Is.False);
                Assert.That(animation.IsDeathFrozen, Is.False);
                Assert.That(animator.speed, Is.EqualTo(1f));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        /* MonsterDeathFallbackReportsMissingAnimatorOnce 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MonsterDeathFallbackReportsMissingAnimatorOnce()
        {
            var root = new GameObject("MissingAnimator");
            root.SetActive(false);
            try
            {
                MonsterAnimationBehaviour animation =
                    root.AddComponent<MonsterAnimationBehaviour>();
                LogAssert.Expect(
                    LogType.Error,
                    new System.Text.RegularExpressions.Regex(
                        "requires an Animator"));
                root.SetActive(true);
                animation.PlayDeath();
                animation.PlayDeath();
                Assert.That(animation.IsDead, Is.True);
                animation.ReviveToIdle();
                Assert.That(animation.IsDead, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        /* MonsterDeathFallbackReportsMissingController 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void MonsterDeathFallbackReportsMissingController()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<
                GameObject>("Assets/Prefab/Monster/Rin_Unit.prefab");
            Assert.That(prefab, Is.Not.Null);
            GameObject root =
                UnityEngine.Object.Instantiate(prefab);
            try
            {
                MonsterAnimationBehaviour animation =
                    root.GetComponentInChildren<
                        MonsterAnimationBehaviour>(true);
                Animator animator =
                    root.GetComponentInChildren<Animator>(true);
                Assert.That(animation, Is.Not.Null);
                Assert.That(animator, Is.Not.Null);
                MethodInfo awake = typeof(
                        MonsterAnimationBehaviour)
                    .GetMethod(
                        "Awake",
                        BindingFlags.Instance
                            | BindingFlags.NonPublic);
                Assert.That(awake, Is.Not.Null);
                awake.Invoke(animation, null);
                animator.runtimeAnimatorController = null;
                LogAssert.Expect(
                    LogType.Warning,
                    new System.Text.RegularExpressions.Regex(
                        "has no RuntimeAnimatorController"));
                animation.PlayDeath();
                Assert.That(animation.IsDead, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        /* DebugBothEightKeysToggleRootAndLearningChangesOnlyGivenBucket 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void DebugBothEightKeysToggleRootAndLearningChangesOnlyGivenBucket()
        {
            var scene = EditorSceneManager.OpenPreviewScene(RunScenePath);
            try
            {
                NewCoreDebugUIController debug =
                    Single<NewCoreDebugUIController>(scene);
                MethodInfo awake = typeof(NewCoreDebugUIController)
                    .GetMethod(
                        "Awake",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                MethodInfo handleToggle =
                    typeof(NewCoreDebugUIController)
                    .GetMethod(
                        "HandleToggleInput",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(awake, Is.Not.Null);
                Assert.That(handleToggle, Is.Not.Null);
                awake.Invoke(debug, null);
                Transform root = debug.transform.Find("DebugPanel");
                Assert.That(root, Is.Not.Null);
                Assert.That(root.gameObject.activeSelf, Is.False);

                handleToggle.Invoke(
                    debug,
                    new object[] { true, false });
                Assert.That(root.gameObject.activeSelf, Is.True);

                handleToggle.Invoke(
                    debug,
                    new object[] { false, true });
                Assert.That(root.gameObject.activeSelf, Is.False);
                handleToggle.Invoke(
                    debug,
                    new object[] { true, false });
                debug.Open();
                Assert.That(
                    debug.transform.Find("DebugPanel/DebugUI")
                        .gameObject.activeSelf,
                    Is.True);
                debug.Close();
                Assert.That(root.gameObject.activeSelf, Is.True);
                Assert.That(
                    debug.transform.Find("DebugPanel/DebugUI")
                        .gameObject.activeSelf,
                    Is.False);

                var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                    NewCoreRuntimeCatalogAsset>(CatalogPath);
                var catalog = runtimeCatalog.Catalog;
                NewCoreSceneRuntime runtime =
                    Single<NewCoreSceneRuntime>(scene);
                SetProperty(runtime, "Catalog", catalog);
                var spawns = NewCoreTestFactory.CreateSpawnManager(
                    catalog,
                    _ => 0,
                    () => 0f);
                MonsterModel selected = spawns.CreateMonsterModel(
                    catalog.GetMonster("ariel"),
                    false);
                MonsterModel other = spawns.CreateMonsterModel(
                    catalog.GetMonster("eve"),
                    false);
                Assert.That(
                    runtime.TryLearnSkill(
                        selected,
                        catalog.GetSkill("ariel-b")),
                    Is.True);
                Assert.That(
                    selected.SkillBucket.ActiveSkills.Any(
                        skill => skill.skill_id == "ariel-b"),
                    Is.True);
                Assert.That(
                    other.SkillBucket.ActiveSkills.Any(
                        skill => skill.skill_id == "ariel-b"),
                    Is.False);

                MonsterModel locked = spawns.CreateMonsterModel(
                    catalog.GetMonster("ariel"),
                    false);
                Assert.That(
                    runtime.TryLearnSkill(
                        locked,
                        catalog.GetSkill("ariel-g")),
                    Is.False);
                Assert.That(
                    runtime.TryLearnSkill(
                        locked,
                        catalog.GetSkill("ariel-b")),
                    Is.True);
                Assert.That(
                    runtime.TryLearnSkill(
                        locked,
                        catalog.GetSkill("ariel-g")),
                    Is.True);
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }

        [Test]
        /* DamageMeterOrdersSourcesAndUsesLeaderRelativeSegmentWidths 시나리오의 기대 동작과 상태 변화를 검증한다. */
        public void DamageMeterOrdersSourcesAndUsesLeaderRelativeSegmentWidths()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.Catalog;
            var spawns = NewCoreTestFactory.CreateSpawnManager(
                catalog,
                _ => 0,
                () => 0f);
            MonsterModel ariel = spawns.CreateMonsterModel(
                catalog.GetMonster("ariel"),
                false);
            MonsterModel eve = spawns.CreateMonsterModel(
                catalog.GetMonster("eve"),
                false);
            var roster = new PartyRoster(ariel);
            Assert.That(roster.TryAddManifestedMonster(eve), Is.True);
            var session = new RunSessionModel(
                "stage1",
                1,
                "encounter",
                roster,
                new PrisonerInventory());
            var stage = NewCoreTestFactory.CreateStageManager(
                session,
                catalog,
                0,
                0);
            var runtimeObject = new GameObject("Runtime");
            var canvas = new GameObject(
                "Canvas",
                typeof(RectTransform),
                typeof(Canvas));
            try
            {
                var runtime =
                    runtimeObject.AddComponent<NewCoreSceneRuntime>();
                SetProperty(runtime, "Catalog", catalog);
                SetProperty(runtime, "RuntimeCatalog", runtimeCatalog);
                SetProperty(runtime, "Stage", stage);
                var tracker =
                    canvas.AddComponent<NewCoreDamageMeterTracker>();
                Button openButton = CreateButton(
                    canvas.transform,
                    "DamageMeterUIBtn");
                var meterRoot = new GameObject(
                    "DamageMeterUI",
                    typeof(RectTransform));
                meterRoot.transform.SetParent(canvas.transform, false);
                Button closeButton = CreateButton(
                    meterRoot.transform,
                    "Close");
                for (int index = 0;
                    index < MaximumDamagePanels;
                    index++)
                {
                    CreateDamagePanel(
                        meterRoot.transform,
                        index + 1);
                }

                var controller = canvas.AddComponent<
                    NewCoreDamageMeterUIController>();
                SetField(controller, "openButton", openButton);
                SetField(controller, "meterRoot", meterRoot);
                SetField(controller, "closeButton", closeButton);
                SetField(controller, "tracker", tracker);
                MethodInfo record = typeof(NewCoreDamageMeterTracker)
                    .GetMethod(
                        "Record",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(record, Is.Not.Null);
                var enemyDefinition =
                    catalog.GetEnemy("stage1-swordsman");
                EnemyModel target = new EnemyModel(
                    enemyDefinition,
                    catalog.GetSkill(
                        enemyDefinition.skill_slot_a_id),
                    catalog.GetSkill(
                        enemyDefinition.skill_slot_b_id),
                    (PassiveDefinition)catalog.GetSkill(
                        enemyDefinition.passive_id));
                record.Invoke(
                    tracker,
                    new object[]
                    {
                        new CombatResult(
                            ariel,
                            target,
                            "ariel-a-trait-1",
                            -60f,
                            0f,
                            false,
                            false)
                    });
                record.Invoke(
                    tracker,
                    new object[]
                    {
                        new CombatResult(
                            ariel,
                            target,
                            "ariel-a",
                            -40f,
                            0f,
                            false,
                            false)
                    });
                record.Invoke(
                    tracker,
                    new object[]
                    {
                        new CombatResult(
                            eve,
                            target,
                            "eve-a",
                            -50f,
                            0f,
                            false,
                            false)
                    });
                controller.Open();

                Assert.That(openButton.gameObject.activeSelf, Is.False);
                Transform firstPanel =
                    meterRoot.transform.Find("1PDamagePanel");
                RectTransform[] firstSegments = DirectChildren(
                    firstPanel,
                    "Skill-Meter");
                Assert.That(firstSegments, Has.Length.EqualTo(2));
                Assert.That(
                    firstSegments[0].rect.width,
                    Is.EqualTo(80f).Within(0.01f));
                Assert.That(
                    firstSegments[1].rect.width,
                    Is.EqualTo(120f).Within(0.01f));
                Assert.That(
                    firstSegments[0]
                        .GetComponentInChildren<TMP_Text>(true)
                        .text,
                    Does.StartWith(
                        catalog.GetSkill("ariel-a").display_name));
                Assert.That(
                    firstSegments[1]
                        .GetComponentInChildren<TMP_Text>(true)
                        .text,
                    Does.StartWith(
                        catalog.GetChoice(
                            "ariel-a-trait-1").title));
                Assert.That(
                    firstSegments[0].GetComponent<Image>().color,
                    Is.Not.EqualTo(
                        firstSegments[1].GetComponent<Image>().color));
                Assert.That(
                    firstPanel.Find("Total_Damage")
                        .GetComponent<TMP_Text>().text,
                    Is.EqualTo("100"));
                Assert.That(
                    firstPanel.Find("Total_Damage_Persent")
                        .GetComponent<TMP_Text>().text,
                    Is.EqualTo("100%"));

                Transform secondPanel =
                    meterRoot.transform.Find("2PDamagePanel");
                RectTransform[] secondSegments = DirectChildren(
                    secondPanel,
                    "Skill-Meter");
                Assert.That(secondSegments, Has.Length.EqualTo(1));
                Assert.That(
                    secondSegments[0].rect.width,
                    Is.EqualTo(100f).Within(0.01f));
                Assert.That(
                    secondPanel.Find("Total_Damage_Persent")
                        .GetComponent<TMP_Text>().text,
                    Is.EqualTo("50%"));
                Assert.That(tracker.Version, Is.EqualTo(3));

                controller.Close();
                Assert.That(openButton.gameObject.activeSelf, Is.True);
                Assert.That(meterRoot.activeSelf, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(canvas);
                UnityEngine.Object.DestroyImmediate(runtimeObject);
            }
        }

        private const int MaximumDamagePanels = 5;

        /* CreateText 테스트 대상을 필요한 의존성과 함께 구성한다. */
        private static TMP_Text CreateText(
            Transform parent,
            string name)
        {
            var target = new GameObject(
                name,
                typeof(RectTransform),
                typeof(TextMeshProUGUI));
            target.transform.SetParent(parent, false);
            return target.GetComponent<TMP_Text>();
        }

        /* CreateButton 테스트 대상을 필요한 의존성과 함께 구성한다. */
        private static Button CreateButton(
            Transform parent,
            string name)
        {
            var target = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button));
            target.transform.SetParent(parent, false);
            return target.GetComponent<Button>();
        }

        /* CreateDamagePanel 테스트 대상을 필요한 의존성과 함께 구성한다. */
        private static void CreateDamagePanel(
            Transform parent,
            int slot)
        {
            var panel = new GameObject(
                $"{slot}PDamagePanel",
                typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var portrait = new GameObject(
                "Image",
                typeof(RectTransform),
                typeof(Image));
            portrait.transform.SetParent(panel.transform, false);
            CreateText(panel.transform, "Monster_Name_Text");
            CreateText(panel.transform, "Total_Damage");
            CreateText(panel.transform, "Total_Damage_Persent");
            var background = new GameObject(
                "MeterBG",
                typeof(RectTransform),
                typeof(Image));
            background.transform.SetParent(panel.transform, false);
            ((RectTransform)background.transform).sizeDelta =
                new Vector2(200f, 20f);
            var meter = new GameObject(
                "Skill-Meter",
                typeof(RectTransform),
                typeof(Image));
            meter.transform.SetParent(panel.transform, false);
            ((RectTransform)meter.transform).sizeDelta =
                new Vector2(200f, 20f);
            CreateText(meter.transform, "SkillName");
        }

        /* DirectChildren 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static RectTransform[] DirectChildren(
            Transform parent,
            string name)
        {
            var result = new List<RectTransform>();
            for (int index = 0; index < parent.childCount; index++)
            {
                Transform child = parent.GetChild(index);
                if (child.name == name
                    && child is RectTransform rect)
                {
                    result.Add(rect);
                }
            }
            return result.ToArray();
        }

        /* AssertPrefabHas 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertPrefabHas<TComponent>(string path)
            where TComponent : Component
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                AssertNoMissingScripts(root, path);
                AssertNoPreviousAuthorityTypes(root, path);
                Assert.That(
                    root.GetComponentInChildren<TComponent>(true),
                    Is.Not.Null,
                    path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /* AssertPrefabIsClean 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertPrefabIsClean(string path)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                AssertNoMissingScripts(root, path);
                AssertNoPreviousAuthorityTypes(root, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        /* Single 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static TComponent Single<TComponent>(Scene scene)
            where TComponent : Component
        {
            var values = Components<TComponent>(scene).ToArray();
            Assert.That(values.Length, Is.EqualTo(1), typeof(TComponent).Name);
            return values[0];
        }

        /* IEnumerable 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static IEnumerable<TComponent> Components<TComponent>(
            Scene scene)
            where TComponent : Component
        {
            return scene.GetRootGameObjects().SelectMany(
                root => root.GetComponentsInChildren<TComponent>(true));
        }

        /* AssertNoMissingScripts 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertNoMissingScripts(
            Scene scene,
            string source)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                AssertNoMissingScripts(root, source);
            }
        }

        /* AssertNoMissingScripts 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertNoMissingScripts(
            GameObject root,
            string source)
        {
            var count = root.GetComponentsInChildren<Transform>(true)
                .Sum(transform =>
                    GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(
                        transform.gameObject));
            Assert.That(count, Is.Zero, source);
        }

        /* AssertNoPreviousAuthorityTypes 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertNoPreviousAuthorityTypes(
            Scene scene,
            string source)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                AssertNoPreviousAuthorityTypes(root, source);
            }
        }

        /* AssertNoPreviousAuthorityTypes 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertNoPreviousAuthorityTypes(
            GameObject root,
            string source)
        {
            var behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
            for (var index = 0; index < behaviours.Length; index++)
            {
                var type = behaviours[index].GetType();
                var fullName = type.FullName ?? type.Name;
                var previous =
                    fullName.StartsWith(
                        "Pakuri.InGame.",
                        StringComparison.Ordinal)
                    || fullName == "UIManager";
                Assert.That(previous, Is.False, source + ": " + fullName);
            }
        }

        /* AssertRequiredReferences 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertRequiredReferences(
            UnityEngine.Object target,
            params string[] propertyNames)
        {
            var serialized = new SerializedObject(target);
            for (var index = 0; index < propertyNames.Length; index++)
            {
                var property = serialized.FindProperty(propertyNames[index]);
                Assert.That(property, Is.Not.Null, propertyNames[index]);
                Assert.That(
                    property.propertyType,
                    Is.EqualTo(SerializedPropertyType.ObjectReference),
                    propertyNames[index]);
                Assert.That(
                    property.objectReferenceValue,
                    Is.Not.Null,
                    propertyNames[index]);
            }
        }

        /* AssertArraySize 검증 조건을 공통 보조 절차로 확인한다. */
        private static void AssertArraySize(
            UnityEngine.Object target,
            string propertyName,
            int expected)
        {
            var property =
                new SerializedObject(target).FindProperty(propertyName);
            Assert.That(property, Is.Not.Null);
            Assert.That(property.isArray, Is.True);
            Assert.That(property.arraySize, Is.EqualTo(expected));
        }

        /* SetField 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static void SetField(
            object target,
            string fieldName,
            object value)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        /* SetProperty 시나리오의 기대 동작과 상태 변화를 검증한다. */
        private static void SetProperty(
            object target,
            string propertyName,
            object value)
        {
            var property = target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, propertyName);
            property.SetValue(target, value);
        }

        /* GetListCount 검증에 필요한 실제 런타임 값을 읽어 반환한다. */
        private static int GetListCount(
            object target,
            string fieldName)
        {
            var field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            var list = field.GetValue(target) as ICollection;
            Assert.That(list, Is.Not.Null, fieldName);
            return list.Count;
        }
    }
}
