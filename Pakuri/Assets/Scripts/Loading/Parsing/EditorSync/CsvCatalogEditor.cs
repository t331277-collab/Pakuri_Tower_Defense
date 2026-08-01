/*
 * 역할: CSV 에셋 참조의 Editor 동기화.
 * 책임: CSV 참조를 검사하고 Unity Editor에서 직렬화된 런타임 에셋 카탈로그를 재생성한다.
 */

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Pakuri.Data.CsvAssetReferenceCollector;
using static Pakuri.Data.GameDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvSourceLoader;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    /// CSV에 작성된 에셋 참조를 Unity Editor 런타임 카탈로그와 동기화한다.
    public static class CsvCatalogEditor
    {

        /// ImportedSourceCatalogsForEditor를 현재 원본 상태와 동기화한다.
        public static void SyncImportedSourceCatalogsForEditor()
        {
            SyncRuntimeCatalogAssetsFromImportedSource(out _);
            ResetRuntimeState();
        }

        /// CsvRuntimeCatalogsForEditor를 동기화하고 검증한다.
        public static void SyncAndValidateCsvRuntimeCatalogsForEditor()
        {
            var sourceModel = SyncRuntimeCatalogAssetsFromImportedSource(out var sourceCatalog);
            ResetRuntimeState();
            var catalog = BuildValidatedRuntimeCatalog(sourceCatalog, sourceModel);
            Debug.Log(FormatRuntimeCatalogSummary(catalog));
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced and validated from '{AuthoringCsvAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        /// RuntimeCatalogAssetsMenu를 현재 원본 상태와 동기화한다.
        [MenuItem("Pakuri/Sync CSV Runtime Catalog Assets")]

        internal static void SyncRuntimeCatalogAssetsMenu()
        {
            SyncImportedSourceCatalogsForEditor();
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced from '{AuthoringCsvAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        /// SourceDataMenu를 검증한다.
        [MenuItem("Pakuri/Validate CSV Source Data")]

        internal static void ValidateSourceDataMenu()
        {
            try
            {
                var sourceModel = SyncRuntimeCatalogAssetsFromImportedSource(out var sourceCatalog);
                ResetRuntimeState();
                var catalog = BuildValidatedRuntimeCatalog(sourceCatalog, sourceModel);
                Debug.Log(FormatRuntimeCatalogSummary(catalog));
            }
            catch (CsvFatalException exception)
            {
                if (exception.Errors.Count > 0)
                {
                    Debug.LogError(string.Join(Environment.NewLine, exception.Errors));
                }

                throw;
            }
        }

        internal static SourceModel SyncRuntimeCatalogAssetsFromImportedSource(
            out CsvRuntimeCatalog sourceCatalog)
        {
            EnsureRuntimeResourcesFolderExists();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            sourceCatalog = LoadOrCreateAsset<CsvRuntimeCatalog>(RuntimeCatalogAssetPath);
            sourceCatalog.CatalogMonsters = LoadImportedSourceTextAssetOrThrow(CatalogMonstersFileName);
            sourceCatalog.Monsters = LoadImportedSourceTextAssetOrThrow(MonstersFileName);
            sourceCatalog.MonsterRewardChoices = LoadImportedSourceTextAssetOrThrow(MonsterRewardChoicesFileName);
            sourceCatalog.MonsterSkillsProjectileFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillBaseCsvAssetRoot,
                "skills_projectile.csv");
            sourceCatalog.MonsterSkillsLineAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillBaseCsvAssetRoot,
                "skills_line_attack.csv");
            sourceCatalog.MonsterSkillsAreaAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillBaseCsvAssetRoot,
                "skills_area_attack.csv");
            sourceCatalog.MonsterSkillsSingleAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillBaseCsvAssetRoot,
                "skills_single_attack.csv");
            sourceCatalog.MonsterSkillsBuffFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillBaseCsvAssetRoot,
                "skills_buff.csv");
            sourceCatalog.MonsterSkillsPassiveFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillBaseCsvAssetRoot,
                "skills_passive.csv");
            sourceCatalog.MonsterSkillNodeDefinitions = LoadTextAssetOrThrow(
                $"{AuthoringMonsterSkillNodeCsvAssetRoot}/definitions/{MonsterSkillNodeDefinitionsFileName}",
                "Create the skill node definition CSV before validation.");
            sourceCatalog.MonsterSkillNodeDefinitionParams = LoadTextAssetOrThrow(
                $"{AuthoringMonsterSkillNodeCsvAssetRoot}/definitions/{MonsterSkillNodeDefinitionParamsFileName}",
                "Create the skill node definition param CSV before validation.");
            sourceCatalog.MonsterSkillGraphNodeFiles = LoadImportedSourceTextAssetsByPrefix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_graph_nodes_");
            sourceCatalog.MonsterSkillTriggerFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillTriggerCsvAssetRoot,
                "_skill_triger.csv");
            sourceCatalog.MonsterSkillChoicesProjectileFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_projectile.csv");
            sourceCatalog.MonsterSkillChoicesLineAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_line_attack.csv");
            sourceCatalog.MonsterSkillChoicesAreaAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_area_attack.csv");
            sourceCatalog.MonsterSkillChoicesSingleAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_single_attack.csv");
            sourceCatalog.MonsterSkillChoicesBuffFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_buff.csv");
            sourceCatalog.MonsterSkillChoicesPassiveFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_passive.csv");
            sourceCatalog.StatusEffects = LoadImportedSourceTextAssetOrThrow(StatusEffectsFileName);
            sourceCatalog.Enemies = LoadTextAssetOrThrow(
                $"{AuthoringEnemyCsvAssetRoot}/{EnemiesFileName}",
                "Create enemies.csv under Assets/CSVdata/authoring/enemy before validation.");
            sourceCatalog.EnemySkillBaseFiles = LoadImportedSourceTextAssetsByPrefix(
                AuthoringEnemySkillBaseCsvAssetRoot,
                "skills_");
            sourceCatalog.EnemySkillTriggerFiles = LoadImportedSourceTextAssetsBySuffix(
                AuthoringEnemySkillTriggerCsvAssetRoot,
                "_skill_triger.csv");
            EditorUtility.SetDirty(sourceCatalog);

            var sourceModel = LoadSourceModel(sourceCatalog);
            sourceCatalog.Sprites = BuildSpriteEntries(sourceModel);
            sourceCatalog.Prefabs = BuildPrefabEntries(sourceModel);
            sourceCatalog.AnimatorControllers = BuildAnimatorControllerEntries(sourceModel);
            sourceCatalog.ResetLookups();
            EditorUtility.SetDirty(sourceCatalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return sourceModel;
        }

        internal static void EnsureRuntimeResourcesFolderExists()
        {
            var resourceFolder = Path.Combine(Application.dataPath, "Resources");
            var pakuriFolder = Path.Combine(resourceFolder, "Pakuri");
            var csvFolder = Path.Combine(pakuriFolder, "CSVRuntime");
            Directory.CreateDirectory(resourceFolder);
            Directory.CreateDirectory(pakuriFolder);
            Directory.CreateDirectory(csvFolder);
        }

        internal static T LoadOrCreateAsset<T>(string assetPath)
            where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            return asset;
        }

        internal static TextAsset LoadImportedSourceTextAssetOrThrow(string fileName)
        {
            var assetPath = GetAuthoringSourceAssetPath(fileName);
            return LoadTextAssetOrThrow(
                assetPath,
                "Import the source CSV into Assets/CSVdata/authoring before validation.");
        }

        internal static TextAsset[] LoadImportedSourceTextAssetsBySuffix(string folderAssetPath, string fileNameSuffix)
        {
            var absoluteFolderPath = GetAbsoluteAssetPath(folderAssetPath);
            if (!Directory.Exists(absoluteFolderPath))
            {
                return Array.Empty<TextAsset>();
            }

            var files = Directory.GetFiles(
                absoluteFolderPath,
                "*" + fileNameSuffix,
                SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            var assets = new List<TextAsset>(files.Length);
            for (var i = 0; i < files.Length; i++)
            {
                var assetPath = GetAssetPathFromAbsolutePath(files[i]);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                if (asset == null)
                {
                    TryImportTextAsset(assetPath);
                    asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                }

                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets.ToArray();
        }

        internal static TextAsset[] LoadImportedSourceTextAssetsByPrefix(string folderAssetPath, string fileNamePrefix)
        {
            var absoluteFolderPath = GetAbsoluteAssetPath(folderAssetPath);
            if (!Directory.Exists(absoluteFolderPath))
            {
                return Array.Empty<TextAsset>();
            }

            var files = Directory.GetFiles(
                absoluteFolderPath,
                fileNamePrefix + "*.csv",
                SearchOption.AllDirectories);
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);

            var assets = new List<TextAsset>(files.Length);
            for (var i = 0; i < files.Length; i++)
            {
                var assetPath = GetAssetPathFromAbsolutePath(files[i]);
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                if (asset == null)
                {
                    TryImportTextAsset(assetPath);
                    asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                }

                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets.ToArray();
        }

        internal static TextAsset LoadTextAssetOrThrow(string assetPath, string instruction)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null)
            {
                TryImportTextAsset(assetPath);
                asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            }

            if (asset == null)
            {
                throw new CsvFatalException(
                    $"Required imported CSV TextAsset is missing at '{assetPath}'.",
                    new List<string> { instruction });
            }

            return asset;
        }

        internal static void TryImportTextAsset(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            var absolutePath = GetAbsoluteAssetPath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return;
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
        }

        internal static string GetAbsoluteAssetPath(string assetPath)
        {
            const string assetsPrefix = "Assets/";
            if (!assetPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return assetPath;
            }

            var relativePath = assetPath.Substring(assetsPrefix.Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativePath);
        }

        internal static string GetAssetPathFromAbsolutePath(string absolutePath)
        {
            var fullPath = Path.GetFullPath(absolutePath).Replace('\\', '/');
            var assetsRoot = Path.GetFullPath(Application.dataPath).Replace('\\', '/');
            if (!fullPath.StartsWith(assetsRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }

            return "Assets/" + fullPath.Substring(assetsRoot.Length + 1);
        }

        internal static CsvRuntimeCatalog.SpriteEntry[] BuildSpriteEntries(SourceModel sourceModel)
        {
            var entries = new List<CsvRuntimeCatalog.SpriteEntry>();
            foreach (var asset in CollectReferencedAssets(sourceModel).SpritePaths)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(asset.AssetPath);
                if (sprite == null)
                {
                    throw new CsvFatalException($"CSV runtime sprite asset is missing or not a Sprite: '{asset.AssetPath}'.");
                }

                entries.Add(new CsvRuntimeCatalog.SpriteEntry
                {
                    AssetPath = asset.AssetPath,
                    Asset = sprite
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        internal static CsvRuntimeCatalog.PrefabEntry[] BuildPrefabEntries(SourceModel sourceModel)
        {
            var entries = new List<CsvRuntimeCatalog.PrefabEntry>();
            foreach (var asset in CollectReferencedAssets(sourceModel).PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.AssetPath);
                if (prefab == null)
                {
                    throw new CsvFatalException($"CSV runtime prefab asset is missing or not a GameObject: '{asset.AssetPath}'.");
                }

                entries.Add(new CsvRuntimeCatalog.PrefabEntry
                {
                    AssetPath = asset.AssetPath,
                    Asset = prefab
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        internal static CsvRuntimeCatalog.AnimatorControllerEntry[] BuildAnimatorControllerEntries(SourceModel sourceModel)
        {
            var entries = new List<CsvRuntimeCatalog.AnimatorControllerEntry>();
            foreach (var asset in CollectReferencedAssets(sourceModel).AnimatorControllerPaths)
            {
                var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(asset.AssetPath);
                if (animatorController == null)
                {
                    throw new CsvFatalException($"CSV runtime animator controller asset is missing or not a RuntimeAnimatorController: '{asset.AssetPath}'.");
                }

                entries.Add(new CsvRuntimeCatalog.AnimatorControllerEntry
                {
                    AssetPath = asset.AssetPath,
                    Asset = animatorController
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        /// RuntimeState를 초기 런타임 상태로 되돌린다.
        internal static void ResetRuntimeState()
        {
            initialized = false;
            failed = false;
            runtimeCatalog = null;
        }
    }
}

namespace Pakuri.Data
{

    class CsvCatalogPostprocessor : AssetPostprocessor
    {
        private static bool syncQueued;

        [InitializeOnLoadMethod]
        private static void QueueInitialSync()
        {
            QueueSync();
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!TouchesImportedSource(importedAssets)
                && !TouchesImportedSource(deletedAssets)
                && !TouchesImportedSource(movedAssets)
                && !TouchesImportedSource(movedFromAssetPaths))
            {
                return;
            }

            QueueSync();
        }

        private static bool TouchesImportedSource(string[] assetPaths)
        {
            if (assetPaths == null)
            {
                return false;
            }

            for (var i = 0; i < assetPaths.Length; i++)
            {
                var assetPath = assetPaths[i];
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                if (GameDataLoader.IsAuthoringCsvSourceAssetPath(assetPath))
                {
                    return true;
                }
            }

            return false;
        }

        private static void QueueSync()
        {
            if (syncQueued)
            {
                return;
            }

            syncQueued = true;
            EditorApplication.delayCall += RunQueuedSync;
        }

        private static void RunQueuedSync()
        {
            syncQueued = false;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                QueueSync();
                return;
            }

            try
            {
                CsvCatalogEditor.SyncImportedSourceCatalogsForEditor();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Pakuri CSV runtime catalog auto-sync failed: {ex}");
            }
        }
    }
}
#endif
