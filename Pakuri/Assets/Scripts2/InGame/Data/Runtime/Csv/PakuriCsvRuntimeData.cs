using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private const string ImportedSourceAssetRoot = "Assets/CSVdata/source";
        private const string RuntimeResourcesFolderAssetPath = "Assets/Resources/Pakuri/CSVRuntime";
        private const string SourceCatalogAssetPath = RuntimeResourcesFolderAssetPath + "/PakuriCsvRuntimeSourceCatalog.asset";
        private const string AssetCatalogAssetPath = RuntimeResourcesFolderAssetPath + "/PakuriCsvRuntimeAssetCatalog.asset";
        private const string SourceCatalogResourcesPath = "Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog";
        private const string AssetCatalogResourcesPath = "Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog";
        private const string CatalogMonstersFileName = "catalog_monsters.csv";
        private const string CatalogStageOneEnemiesFileName = "catalog_stage_one_enemies.csv";
        private const string CatalogStageTwoEnemiesFileName = "catalog_stage_two_enemies.csv";
        private const string MonstersFileName = "monsters.csv";
        private const string MonsterRewardChoicesFileName = "monster_modifier_skill_choice.csv";
        private const string MonsterSkillsFileName = "monster_skills.csv";
        private const string MonsterSkillEffectsFileName = "monster_skill_effects.csv";
        private const string MonsterSkillTriggersFileName = "monster_skill_triger.csv";
        private const string MonsterSkillChoicesFileName = "monster_skill_choices.csv";
        private const string StatusEffectsFileName = "status_effects.csv";
        private const string StageOneEnemiesFileName = "stage_one_enemies.csv";
        private const string StageTwoEnemiesFileName = "stage_two_enemies.csv";
        private const string EnemySkillDataFileName = "EnemySkillData.csv";
        private const string EnemySkillDataAssetPath = "Assets/CSVdata/EnemySkillData.csv";

        private static bool initialized;
        private static bool failed;
        private static GameDataCatalog runtimeCatalog;
        private static PakuriCsvRuntimeSourceCatalog runtimeSourceCatalog;
        private static PakuriCsvRuntimeAssetCatalog runtimeAssetCatalog;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            EnsureInitialized();
        }

        public static GameDataCatalog ResolveCatalogOrFallback(GameDataCatalog fallback)
        {
            EnsureInitialized();
            if (failed)
            {
                return null;
            }

            return runtimeCatalog != null ? runtimeCatalog : fallback;
        }

        public static void EnsureInitialized()
        {
            if (initialized || failed)
            {
                return;
            }

            try
            {
                runtimeCatalog = LoadAndValidateRuntimeCatalog();
                initialized = true;
                Debug.Log(FormatRuntimeCatalogSummary(runtimeCatalog));
            }
            catch (CsvFatalException ex)
            {
                FailAndQuit(ex.Message, ex.Errors);
            }
            catch (Exception ex)
            {
                FailAndQuit(
                    "PakuriCsvRuntimeData failed with an unexpected exception.",
                    new List<string> { ex.ToString() });
            }
        }

        private static GameDataCatalog LoadAndValidateRuntimeCatalog()
        {
            runtimeSourceCatalog = LoadSourceCatalogOrThrow();
            runtimeAssetCatalog = LoadAssetCatalogOrThrow();
            var source = LoadSourceModel(runtimeSourceCatalog);
            ValidateSourceModelOrThrow(source, runtimeAssetCatalog);
            var catalog = BuildRuntimeCatalog(source);
            ValidateRuntimeCatalogOrThrow(catalog, source);
            PakuriDataManager.Instance.RegisterCatalog(catalog);
            return catalog;
        }

        private static string FormatRuntimeCatalogSummary(GameDataCatalog catalog)
        {
            return
                $"PakuriCsvRuntimeData loaded runtime catalog from resource source '{SourceCatalogResourcesPath}' " +
                $"with {catalog.Monsters.Length} monsters, {catalog.StageOneEnemies.Length} stage-one enemies, " +
                $"and {catalog.StageTwoEnemies.Length} stage-two enemies.";
        }

        private static void FailAndQuit(string message, List<string> errors)
        {
            failed = true;
            Debug.LogError(message);

            if (errors != null)
            {
                for (var i = 0; i < errors.Count; i++)
                {
                    Debug.LogError(errors[i]);
                }
            }

#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
#endif
            Application.Quit();
        }
    }
}
