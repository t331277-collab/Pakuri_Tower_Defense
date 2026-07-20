#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static Pakuri.Data.CsvAssetReferenceCollector;
using static Pakuri.Data.CsvDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;


namespace Pakuri.Data
{
    /*
     * Unity Editor에서 CSV와 런타임 Source·Asset 카탈로그를 동기화한다.
     */
    public static class CsvCatalogSync
    {
        /*
         * 가져온 CSV 원본으로 런타임 카탈로그 자산을 갱신한다.
         */
        public static void SyncImportedSourceCatalogsForEditor()
        {
            SyncRuntimeCatalogAssetsFromImportedSource();
            ResetRuntimeState();
        }

        /*
         * 에디터에서 CSV 원본과 런타임 카탈로그를 동기화하고 검증한다.
         */
        public static void SyncAndValidateCsvRuntimeCatalogsForEditor()
        {
            SyncImportedSourceCatalogsForEditor();
            var catalog = LoadAndValidateRuntimeCatalog();
            Debug.Log(FormatRuntimeCatalogSummary(catalog));
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced and validated from '{AuthoringSourceAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        [MenuItem("Pakuri/Sync CSV Runtime Catalog Assets")]
        /*
         * Unity 메뉴에서 런타임 카탈로그 자산 동기화를 실행한다.
         */
        internal static void SyncRuntimeCatalogAssetsMenu()
        {
            SyncImportedSourceCatalogsForEditor();
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced from '{AuthoringSourceAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        [MenuItem("Pakuri/Validate CSV Source Data")]
        /*
         * Unity 메뉴에서 CSV 원본 검증을 실행한다.
         */
        internal static void ValidateSourceDataMenu()
        {
            try
            {
                SyncImportedSourceCatalogsForEditor();
                var catalog = LoadAndValidateRuntimeCatalog();
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

        /*
         * 원본 CSV와 런타임 자산을 동기화한다.
         */
        internal static void SyncRuntimeCatalogAssetsFromImportedSource()
        {
            EnsureRuntimeResourcesFolderExists();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var sourceCatalog = LoadOrCreateAsset<CsvRuntimeCatalog>(RuntimeCatalogAssetPath);
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
        }

        /*
         * 작업에 필요한 상태를 준비한다.
         */
        internal static void EnsureRuntimeResourcesFolderExists()
        {
            var resourceFolder = Path.Combine(Application.dataPath, "Resources");
            var pakuriFolder = Path.Combine(resourceFolder, "Pakuri");
            var csvFolder = Path.Combine(pakuriFolder, "CSVRuntime");
            Directory.CreateDirectory(resourceFolder);
            Directory.CreateDirectory(pakuriFolder);
            Directory.CreateDirectory(csvFolder);
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static TextAsset LoadImportedSourceTextAssetOrThrow(string fileName)
        {
            var assetPath = GetImportedSourceAssetPath(fileName);
            return LoadTextAssetOrThrow(
                assetPath,
                "Import the source CSV into Assets/CSVdata/authoring before validation.");
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
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

        /*
         * 해당 자료 변환에 필요한 값을 구성한다.
         */
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

        /*
         * 계산에 필요한 값을 반환한다.
         */
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

        /*
         * 계산에 필요한 값을 반환한다.
         */
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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

        /*
         * 에디터 동기화 뒤 런타임 캐시를 초기화한다.
         */
        internal static void ResetRuntimeState()
        {
            initialized = false;
            failed = false;
            runtimeCatalog = null;
            runtimeCsvCatalog = null;
        }
    }
}
#endif
