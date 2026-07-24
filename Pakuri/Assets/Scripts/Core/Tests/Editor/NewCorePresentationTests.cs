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
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Presentation.Actors;
using Pakuri.NewCore.Presentation.Assets;
using Pakuri.NewCore.Presentation.Scene;
using Pakuri.NewCore.Presentation.UI;
using Pakuri.NewCore.Run;
using Pakuri.NewCore.Spawn;
using Pakuri.NewCore.Units.Models;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Pakuri.NewCore.Tests
{
    public sealed class NewCorePresentationTests
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

            var definitions = catalog.CreateBootstrap().Catalog;
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
                var ui = Single<NewCoreInGameUIController>(scene);
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
                    "stageManager",
                    "unitSpawnManager",
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
                AssertPrefabHas<SkillVisualActorBehaviour>(
                    MigratedUnreferencedSkillVisualPrefabPaths[index]);
            }
        }

        [Test]
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

                var effects = new EffectManager();
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
                    default);
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

                view.Bind(catalog, effects);
                view.Sync();

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
        public void EnemyActorDisablesCollidersWhenItsModelIsDefeated()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.CreateBootstrap().Catalog;
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
        public void EnemyActorDisablesCollidersAfterNexusContact()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.CreateBootstrap().Catalog;
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
        public void TerminalEnemyActorDoesNotRegrowAcrossSpawnSyncPasses()
        {
            var runtimeCatalog = AssetDatabase.LoadAssetAtPath<
                NewCoreRuntimeCatalogAsset>(CatalogPath);
            var catalog = runtimeCatalog.CreateBootstrap().Catalog;
            var spawns = new SpawnManager(catalog, _ => 0, () => 0f);
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
            var stage = new StageManager(session, catalog, 0, 0);
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
                var spawnView = host.AddComponent<NewCoreSpawnController>();
                monsterTemplate.AddComponent<MonsterActorBehaviour>();
                enemyTemplate.AddComponent<BoxCollider2D>();
                enemyTemplate.AddComponent<EnemyActorBehaviour>();

                var effects = new EffectManager();
                var actors = new SkillActorManager(effects);
                var targeting = new SkillTargeting(_ => 0);
                var execution = new SkillExecutionRuntime(
                    catalog,
                    targeting,
                    actors,
                    effects,
                    () => 0f);
                var combat = new InGameCombatManager(() => 0f, execution);
                var input = new PlayerInputController();
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

                spawnView.Bind(runtime, spawns);
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

        private static TComponent Single<TComponent>(Scene scene)
            where TComponent : Component
        {
            var values = Components<TComponent>(scene).ToArray();
            Assert.That(values.Length, Is.EqualTo(1), typeof(TComponent).Name);
            return values[0];
        }

        private static IEnumerable<TComponent> Components<TComponent>(
            Scene scene)
            where TComponent : Component
        {
            return scene.GetRootGameObjects().SelectMany(
                root => root.GetComponentsInChildren<TComponent>(true));
        }

        private static void AssertNoMissingScripts(
            Scene scene,
            string source)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                AssertNoMissingScripts(root, source);
            }
        }

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

        private static void AssertNoPreviousAuthorityTypes(
            Scene scene,
            string source)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                AssertNoPreviousAuthorityTypes(root, source);
            }
        }

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
