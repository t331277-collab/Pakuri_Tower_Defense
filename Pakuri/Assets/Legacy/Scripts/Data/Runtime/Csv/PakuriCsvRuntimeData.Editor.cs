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

            var sourceCatalog = LoadOrCreateAsset<PakuriCsvRuntimeSourceCatalog>(SourceCatalogAssetPath);
            sourceCatalog.CatalogMonsters = LoadImportedSourceTextAssetOrThrow(CatalogMonstersFileName);
            sourceCatalog.CatalogStageOneEnemies = LoadImportedSourceTextAssetOrThrow(CatalogStageOneEnemiesFileName);
            sourceCatalog.Monsters = LoadImportedSourceTextAssetOrThrow(MonstersFileName);
            sourceCatalog.MonsterRewardChoices = LoadImportedSourceTextAssetOrThrow(MonsterRewardChoicesFileName);
            sourceCatalog.MonsterSkills = LoadImportedSourceTextAssetOrThrow(MonsterSkillsFileName);
            sourceCatalog.MonsterSkillChoices = LoadImportedSourceTextAssetOrThrow(MonsterSkillChoicesFileName);
            sourceCatalog.StageOneEnemies = LoadImportedSourceTextAssetOrThrow(StageOneEnemiesFileName);
            sourceCatalog.EnemySkills = LoadTextAssetOrThrow(
                EnemySkillDataAssetPath,
                "Create EnemySkillData.csv under Assets/CSVdata before validation.");
            EditorUtility.SetDirty(sourceCatalog);

            var sourceModel = LoadSourceModel(sourceCatalog);
            var assetCatalog = LoadOrCreateAsset<PakuriCsvRuntimeAssetCatalog>(AssetCatalogAssetPath);
            assetCatalog.Sprites = BuildSpriteEntries(sourceModel);
            assetCatalog.Prefabs = BuildPrefabEntries(sourceModel);
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
            var assetPath = $"{ImportedSourceAssetRoot}/{fileName}";
            return LoadTextAssetOrThrow(
                assetPath,
                "Import the source CSV into Assets/CSVdata/source before validation.");
        }

        private static TextAsset LoadTextAssetOrThrow(string assetPath, string instruction)
        {
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null)
            {
                throw new CsvFatalException(
                    $"Required imported CSV TextAsset is missing at '{assetPath}'.",
                    new List<string> { instruction });
            }

            return asset;
        }

        private static PakuriCsvRuntimeAssetCatalog.SpriteEntry[] BuildSpriteEntries(SourceModel sourceModel)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var monster in sourceModel.Monsters.Values)
            {
                AddAssetPath(paths, monster.UnitSpritePath);
                AddAssetPath(paths, monster.ProjectileSpritePath);
            }

            foreach (var skill in sourceModel.Skills.Values)
            {
                AddAssetPath(paths, skill.SkillIconPath);
            }

            foreach (var choice in sourceModel.SkillChoices.Values)
            {
                AddAssetPath(paths, choice.SkillIconPath);
            }

            foreach (var enemy in sourceModel.StageOneEnemies.Values)
            {
                AddAssetPath(paths, enemy.UnitSpritePath);
                AddAssetPath(paths, enemy.ProjectileSpritePath);
            }

            var entries = new List<PakuriCsvRuntimeAssetCatalog.SpriteEntry>();
            foreach (var path in paths)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null)
                {
                    throw new CsvFatalException($"CSV runtime sprite asset is missing or not a Sprite: '{path}'.");
                }

                entries.Add(new PakuriCsvRuntimeAssetCatalog.SpriteEntry
                {
                    AssetPath = path,
                    Asset = sprite
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        private static PakuriCsvRuntimeAssetCatalog.PrefabEntry[] BuildPrefabEntries(SourceModel sourceModel)
        {
            var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var skill in sourceModel.Skills.Values)
            {
                AddAssetPath(paths, skill.SkillEffectPrefabPath);
            }

            foreach (var choice in sourceModel.SkillChoices.Values)
            {
                AddAssetPath(paths, choice.SkillEffectPrefabPath);
            }

            foreach (var skill in sourceModel.EnemySkills.Values)
            {
                AddAssetPath(paths, skill.SkillEffectPrefabPath);
            }

            var entries = new List<PakuriCsvRuntimeAssetCatalog.PrefabEntry>();
            foreach (var path in paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null)
                {
                    throw new CsvFatalException($"CSV runtime prefab asset is missing or not a GameObject: '{path}'.");
                }

                entries.Add(new PakuriCsvRuntimeAssetCatalog.PrefabEntry
                {
                    AssetPath = path,
                    Asset = prefab
                });
            }

            entries.Sort((left, right) => string.Compare(left.AssetPath, right.AssetPath, StringComparison.OrdinalIgnoreCase));
            return entries.ToArray();
        }

        private static void AddAssetPath(HashSet<string> paths, string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            paths.Add(assetPath.Trim().Replace('\\', '/'));
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
