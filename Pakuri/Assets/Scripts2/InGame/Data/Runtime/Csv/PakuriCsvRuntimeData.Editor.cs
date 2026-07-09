#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        public static void SyncImportedSourceCatalogsForEditor()
        {
            SyncRuntimeCatalogAssetsFromImportedSource();
            ResetRuntimeState();
        }

        public static void SyncAndValidateCsvRuntimeCatalogsForEditor()
        {
            SyncImportedSourceCatalogsForEditor();
            var catalog = LoadAndValidateRuntimeCatalog();
            Debug.Log(FormatRuntimeCatalogSummary(catalog));
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced and validated from '{ImportedSourceAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        [MenuItem("Pakuri/Sync CSV Runtime Catalog Assets")]
        private static void SyncRuntimeCatalogAssetsMenu()
        {
            SyncImportedSourceCatalogsForEditor();
            Debug.Log(
                $"Pakuri CSV runtime catalogs synced from '{ImportedSourceAssetRoot}' to '{RuntimeResourcesFolderAssetPath}'.");
        }

        [MenuItem("Pakuri/Validate CSV Source Data")]
        private static void ValidateSourceDataMenu()
        {
            SyncImportedSourceCatalogsForEditor();
            var catalog = LoadAndValidateRuntimeCatalog();
            Debug.Log(FormatRuntimeCatalogSummary(catalog));
        }

        private static void SyncRuntimeCatalogAssetsFromImportedSource()
        {
            EnsureRuntimeResourcesFolderExists();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            var sourceCatalog = LoadOrCreateAsset<PakuriCsvRuntimeSourceCatalog>(SourceCatalogAssetPath);
            sourceCatalog.CatalogMonsters = LoadImportedSourceTextAssetOrThrow(CatalogMonstersFileName);
            sourceCatalog.CatalogStageOneEnemies = LoadImportedSourceTextAssetOrThrow(CatalogStageOneEnemiesFileName);
            sourceCatalog.CatalogStageTwoEnemies = LoadImportedSourceTextAssetOrThrow(CatalogStageTwoEnemiesFileName);
            sourceCatalog.Monsters = LoadImportedSourceTextAssetOrThrow(MonstersFileName);
            sourceCatalog.MonsterRewardChoices = LoadImportedSourceTextAssetOrThrow(MonsterRewardChoicesFileName);
            sourceCatalog.MonsterSkillsProjectileFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillBaseCsvAssetRoot,
                "skills_projectile.csv");
            sourceCatalog.MonsterSkillsLineAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillBaseCsvAssetRoot,
                "skills_line_attack.csv");
            sourceCatalog.MonsterSkillsAreaAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillBaseCsvAssetRoot,
                "skills_area_attack.csv");
            sourceCatalog.MonsterSkillsSingleAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillBaseCsvAssetRoot,
                "skills_single_attack.csv");
            sourceCatalog.MonsterSkillsBuffFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillBaseCsvAssetRoot,
                "skills_buff.csv");
            sourceCatalog.MonsterSkillsPassiveFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillBaseCsvAssetRoot,
                "skills_passive.csv");
            sourceCatalog.MonsterSkillsProjectile = sourceCatalog.MonsterSkillsProjectileFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillsProjectileFileName)
                : null;
            sourceCatalog.MonsterSkillsLineAttack = sourceCatalog.MonsterSkillsLineAttackFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillsLineAttackFileName)
                : null;
            sourceCatalog.MonsterSkillsAreaAttack = sourceCatalog.MonsterSkillsAreaAttackFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillsAreaAttackFileName)
                : null;
            sourceCatalog.MonsterSkillsSingleAttack = sourceCatalog.MonsterSkillsSingleAttackFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillsSingleAttackFileName)
                : null;
            sourceCatalog.MonsterSkillsBuff = sourceCatalog.MonsterSkillsBuffFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillsBuffFileName)
                : null;
            sourceCatalog.MonsterSkillsPassive = sourceCatalog.MonsterSkillsPassiveFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillsPassiveFileName)
                : null;
            sourceCatalog.MonsterSkillNodeFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillNodeCsvAssetRoot,
                "_skill_nodes.csv");
            sourceCatalog.MonsterSkillNodeParamFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillNodeCsvAssetRoot,
                "_skill_node_params.csv");
            sourceCatalog.MonsterSkillNodes = sourceCatalog.MonsterSkillNodeFiles.Length == 0
                ? LoadImportedSourceTextAssetIfPresent(MonsterSkillNodesFileName)
                : null;
            sourceCatalog.MonsterSkillNodeParams = sourceCatalog.MonsterSkillNodeParamFiles.Length == 0
                ? LoadImportedSourceTextAssetIfPresent(MonsterSkillNodeParamsFileName)
                : null;
            sourceCatalog.MonsterSkillEffectFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillEffectCsvAssetRoot,
                "_skill_effects.csv");
            sourceCatalog.MonsterSkillTriggerFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillTriggerCsvAssetRoot,
                "_skill_triger.csv");
            sourceCatalog.MonsterSkillEffects = sourceCatalog.MonsterSkillEffectFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillEffectsFileName)
                : null;
            sourceCatalog.MonsterSkillTriggers = sourceCatalog.MonsterSkillTriggerFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillTriggersFileName)
                : null;
            sourceCatalog.MonsterSkillChoicesProjectileFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_projectile.csv");
            sourceCatalog.MonsterSkillChoicesLineAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_line_attack.csv");
            sourceCatalog.MonsterSkillChoicesAreaAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_area_attack.csv");
            sourceCatalog.MonsterSkillChoicesSingleAttackFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_single_attack.csv");
            sourceCatalog.MonsterSkillChoicesBuffFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_buff.csv");
            sourceCatalog.MonsterSkillChoicesPassiveFiles = LoadImportedSourceTextAssetsBySuffix(
                RuntimeMonsterSkillChoiceCsvAssetRoot,
                "skill_choices_passive.csv");
            sourceCatalog.MonsterSkillChoicesProjectile = sourceCatalog.MonsterSkillChoicesProjectileFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesProjectileFileName)
                : null;
            sourceCatalog.MonsterSkillChoicesLineAttack = sourceCatalog.MonsterSkillChoicesLineAttackFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesLineAttackFileName)
                : null;
            sourceCatalog.MonsterSkillChoicesAreaAttack = sourceCatalog.MonsterSkillChoicesAreaAttackFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesAreaAttackFileName)
                : null;
            sourceCatalog.MonsterSkillChoicesSingleAttack = sourceCatalog.MonsterSkillChoicesSingleAttackFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesSingleAttackFileName)
                : null;
            sourceCatalog.MonsterSkillChoicesBuff = sourceCatalog.MonsterSkillChoicesBuffFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesBuffFileName)
                : null;
            sourceCatalog.MonsterSkillChoicesPassive = sourceCatalog.MonsterSkillChoicesPassiveFiles.Length == 0
                ? LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesPassiveFileName)
                : null;
            sourceCatalog.StatusEffects = LoadImportedSourceTextAssetOrThrow(StatusEffectsFileName);
            sourceCatalog.StageOneEnemies = LoadImportedSourceTextAssetOrThrow(StageOneEnemiesFileName);
            sourceCatalog.StageTwoEnemies = LoadImportedSourceTextAssetOrThrow(StageTwoEnemiesFileName);
            sourceCatalog.EnemySkills = LoadTextAssetOrThrow(
                GetImportedSourceAssetPath(EnemySkillDataFileName),
                "Create EnemySkillData.csv under Assets/CSVdata/runtime/enemy before validation.");
            sourceCatalog.EnemySkillNodes = LoadImportedSourceTextAssetIfPresent(EnemySkillNodesFileName);
            sourceCatalog.EnemySkillNodeParams = LoadImportedSourceTextAssetIfPresent(EnemySkillNodeParamsFileName);
            EditorUtility.SetDirty(sourceCatalog);

            var sourceModel = LoadSourceModel(sourceCatalog);
            var assetCatalog = LoadOrCreateAsset<PakuriCsvRuntimeAssetCatalog>(AssetCatalogAssetPath);
            assetCatalog.Sprites = BuildSpriteEntries(sourceModel);
            assetCatalog.Prefabs = BuildPrefabEntries(sourceModel);
            assetCatalog.AnimatorControllers = BuildAnimatorControllerEntries(sourceModel);
            assetCatalog.ResetLookups();
            EditorUtility.SetDirty(assetCatalog);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureRuntimeResourcesFolderExists()
        {
            var resourceFolder = Path.Combine(Application.dataPath, "Resources");
            var pakuriFolder = Path.Combine(resourceFolder, "Pakuri");
            var csvFolder = Path.Combine(pakuriFolder, "CSVRuntime");
            Directory.CreateDirectory(resourceFolder);
            Directory.CreateDirectory(pakuriFolder);
            Directory.CreateDirectory(csvFolder);
        }

        private static T LoadOrCreateAsset<T>(string assetPath)
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

        private static TextAsset LoadImportedSourceTextAssetOrThrow(string fileName)
        {
            var assetPath = GetImportedSourceAssetPath(fileName);
            return LoadTextAssetOrThrow(
                assetPath,
                "Import the source CSV into Assets/CSVdata/runtime before validation.");
        }

        private static TextAsset LoadImportedSourceTextAssetIfPresent(string fileName)
        {
            var assetPath = GetImportedSourceAssetPath(fileName);
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset != null)
            {
                return asset;
            }

            TryImportTextAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
        }

        private static TextAsset[] LoadImportedSourceTextAssetsBySuffix(string folderAssetPath, string fileNameSuffix)
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

        private static TextAsset LoadTextAssetOrThrow(string assetPath, string instruction)
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

        private static void TryImportTextAsset(string assetPath)
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

        private static string GetAbsoluteAssetPath(string assetPath)
        {
            const string assetsPrefix = "Assets/";
            if (!assetPath.StartsWith(assetsPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return assetPath;
            }

            var relativePath = assetPath.Substring(assetsPrefix.Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relativePath);
        }

        private static string GetAssetPathFromAbsolutePath(string absolutePath)
        {
            var fullPath = Path.GetFullPath(absolutePath).Replace('\\', '/');
            var assetsRoot = Path.GetFullPath(Application.dataPath).Replace('\\', '/');
            if (!fullPath.StartsWith(assetsRoot + "/", StringComparison.OrdinalIgnoreCase))
            {
                return fullPath;
            }

            return "Assets/" + fullPath.Substring(assetsRoot.Length + 1);
        }

        private static PakuriCsvRuntimeAssetCatalog.SpriteEntry[] BuildSpriteEntries(SourceModel sourceModel)
        {
            var entries = new List<PakuriCsvRuntimeAssetCatalog.SpriteEntry>();
            foreach (var asset in CollectReferencedAssets(sourceModel).SpritePaths)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(asset.AssetPath);
                if (sprite == null)
                {
                    throw new CsvFatalException($"CSV runtime sprite asset is missing or not a Sprite: '{asset.AssetPath}'.");
                }

                entries.Add(new PakuriCsvRuntimeAssetCatalog.SpriteEntry
                {
                    AssetPath = asset.AssetPath,
                    Asset = sprite
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        private static PakuriCsvRuntimeAssetCatalog.PrefabEntry[] BuildPrefabEntries(SourceModel sourceModel)
        {
            var entries = new List<PakuriCsvRuntimeAssetCatalog.PrefabEntry>();
            foreach (var asset in CollectReferencedAssets(sourceModel).PrefabPaths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(asset.AssetPath);
                if (prefab == null)
                {
                    throw new CsvFatalException($"CSV runtime prefab asset is missing or not a GameObject: '{asset.AssetPath}'.");
                }

                entries.Add(new PakuriCsvRuntimeAssetCatalog.PrefabEntry
                {
                    AssetPath = asset.AssetPath,
                    Asset = prefab
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        private static PakuriCsvRuntimeAssetCatalog.AnimatorControllerEntry[] BuildAnimatorControllerEntries(SourceModel sourceModel)
        {
            var entries = new List<PakuriCsvRuntimeAssetCatalog.AnimatorControllerEntry>();
            foreach (var asset in CollectReferencedAssets(sourceModel).AnimatorControllerPaths)
            {
                var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(asset.AssetPath);
                if (animatorController == null)
                {
                    throw new CsvFatalException($"CSV runtime animator controller asset is missing or not a RuntimeAnimatorController: '{asset.AssetPath}'.");
                }

                entries.Add(new PakuriCsvRuntimeAssetCatalog.AnimatorControllerEntry
                {
                    AssetPath = asset.AssetPath,
                    Asset = animatorController
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        private static void ResetRuntimeState()
        {
            initialized = false;
            failed = false;
            runtimeCatalog = null;
            runtimeSourceCatalog = null;
            runtimeAssetCatalog = null;
            PakuriDataManager.Instance.RegisterCatalog(null);
        }
    }
}
#endif
