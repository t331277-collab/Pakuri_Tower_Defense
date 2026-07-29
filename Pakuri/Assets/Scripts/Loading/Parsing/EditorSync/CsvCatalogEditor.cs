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

    /// <summary>CSV에 작성된 에셋 참조를 Unity Editor 런타임 카탈로그와 동기화한다.</summary>
    public static class CsvCatalogEditor
    {

        /// <summary><c>ImportedSourceCatalogsForEditor</c>를 현재 원본 상태와 동기화한다.</summary>
        public static void SyncImportedSourceCatalogsForEditor()
        {
            SyncRuntimeCatalogAssetsFromImportedSource(out _);
            ResetRuntimeState();
        }

        /// <summary><c>CsvRuntimeCatalogsForEditor</c>를 동기화하고 검증한다.</summary>
        public static void SyncAndValidateCsvRuntimeCatalogsForEditor()
        {
            var sourceModel = SyncRuntimeCatalogAssetsFromImportedSource(out var sourceCatalog);
            ResetRuntimeState();
            var catalog = BuildValidatedRuntimeCatalog(sourceCatalog, sourceModel);
            Debug.Log(FormatRuntimeCatalogSummary(catalog));
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced and validated from '{AuthoringCsvAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        /// <summary><c>RuntimeCatalogAssetsMenu</c>를 현재 원본 상태와 동기화한다.</summary>
        [MenuItem("Pakuri/Sync CSV Runtime Catalog Assets")]

        internal static void SyncRuntimeCatalogAssetsMenu()
        {
            SyncImportedSourceCatalogsForEditor();
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced from '{AuthoringCsvAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        /// <summary><c>SourceDataMenu</c>를 검증한다.</summary>
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

        /// <summary>전달된 <c>sourceCatalog</c> 값을 사용해 <c>RuntimeCatalogAssetsFromImportedSource</c>를 현재 원본 상태와 동기화한다.</summary>
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

        /// <summary><c>EnsureRuntimeResourcesFolderExists</c> 작업을 수행한다.</summary>
        internal static void EnsureRuntimeResourcesFolderExists()
        {
            var resourceFolder = Path.Combine(Application.dataPath, "Resources");
            var pakuriFolder = Path.Combine(resourceFolder, "Pakuri");
            var csvFolder = Path.Combine(pakuriFolder, "CSVRuntime");
            Directory.CreateDirectory(resourceFolder);
            Directory.CreateDirectory(pakuriFolder);
            Directory.CreateDirectory(csvFolder);
        }

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 기존 <c>Asset</c>를 반환하고 없으면 새로 생성한다.</summary>
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

        /// <summary>전달된 <c>fileName</c> 값을 사용해 <c>LoadImportedSourceTextAsset</c> 데이터를 검증하고 유효하지 않으면 예외를 던진다.</summary>
        internal static TextAsset LoadImportedSourceTextAssetOrThrow(string fileName)
        {
            var assetPath = GetAuthoringSourceAssetPath(fileName);
            return LoadTextAssetOrThrow(
                assetPath,
                "Import the source CSV into Assets/CSVdata/authoring before validation.");
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ImportedSourceTextAssetsBySuffix</c>를 불러온다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ImportedSourceTextAssetsByPrefix</c>를 불러온다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>LoadTextAsset</c> 데이터를 검증하고 유효하지 않으면 예외를 던진다.</summary>
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

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 <c>ImportTextAsset</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
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

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 <c>AbsoluteAssetPath</c>를 반환한다.</summary>
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

        /// <summary>전달된 <c>absolutePath</c> 값을 사용해 <c>AssetPathFromAbsolutePath</c>를 반환한다.</summary>
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

        /// <summary>전달된 <c>sourceModel</c> 값을 사용해 <c>SpriteEntries</c>를 구성한다.</summary>
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

        /// <summary>전달된 <c>sourceModel</c> 값을 사용해 <c>PrefabEntries</c>를 구성한다.</summary>
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

        /// <summary>전달된 <c>sourceModel</c> 값을 사용해 <c>AnimatorControllerEntries</c>를 구성한다.</summary>
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

        /// <summary><c>RuntimeState</c>를 초기 런타임 상태로 되돌린다.</summary>
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

    /// <summary><c>CsvCatalogPostprocessor</c>가 소유하는 데이터와 동작을 캡슐화한다.</summary>
    class CsvCatalogPostprocessor : AssetPostprocessor
    {
        private static bool syncQueued;

        /// <summary><c>QueueInitialSync</c> 작업을 수행한다.</summary>
        [InitializeOnLoadMethod]
        private static void QueueInitialSync()
        {
            QueueSync();
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>OnPostprocessAllAssets</c> 작업을 수행한다.</summary>
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

        /// <summary>전달된 <c>assetPaths</c> 값을 사용해 <c>TouchesImportedSource</c> 조건을 평가하고 결과를 반환한다.</summary>
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

        /// <summary><c>QueueSync</c> 작업을 수행한다.</summary>
        private static void QueueSync()
        {
            if (syncQueued)
            {
                return;
            }

            syncQueued = true;
            EditorApplication.delayCall += RunQueuedSync;
        }

        /// <summary><c>RunQueuedSync</c> 작업을 수행한다.</summary>
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
