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
        private const string CsvDataAssetRoot = "Assets/CSVdata";
        private const string AuthoringCsvAssetRoot = CsvDataAssetRoot + "/authoring";
        private const string AuthoringCatalogCsvAssetRoot = AuthoringCsvAssetRoot + "/catalog";
        private const string AuthoringMonsterCsvAssetRoot = AuthoringCsvAssetRoot + "/monster";
        private const string AuthoringMonsterSkillCsvAssetRoot = AuthoringMonsterCsvAssetRoot + "/skills";
        private const string AuthoringMonsterSkillBaseCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/base";
        private const string AuthoringMonsterSkillChoiceCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/choices";
        private const string AuthoringMonsterSkillTriggerCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/triggers";
        private const string AuthoringMonsterSkillNodeCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/nodes";
        private const string AuthoringEnemyCsvAssetRoot = AuthoringCsvAssetRoot + "/enemy";
        private const string AuthoringEnemySkillCsvAssetRoot = AuthoringEnemyCsvAssetRoot + "/skills";
        private const string AuthoringEnemySkillBaseCsvAssetRoot = AuthoringEnemySkillCsvAssetRoot + "/base";
        private const string AuthoringEnemySkillTriggerCsvAssetRoot = AuthoringEnemySkillCsvAssetRoot + "/triggers";
        private const string AuthoringStatusCsvAssetRoot = AuthoringCsvAssetRoot + "/status";
        private const string AuthoringSourceAssetRoot = AuthoringCsvAssetRoot;
        private const string RuntimeResourcesFolderAssetPath = "Assets/Resources/Pakuri/CSVRuntime";
        private const string SourceCatalogAssetPath = RuntimeResourcesFolderAssetPath + "/PakuriCsvRuntimeSourceCatalog.asset";
        private const string AssetCatalogAssetPath = RuntimeResourcesFolderAssetPath + "/PakuriCsvRuntimeAssetCatalog.asset";
        private const string SourceCatalogResourcesPath = "Pakuri/CSVRuntime/PakuriCsvRuntimeSourceCatalog";
        private const string AssetCatalogResourcesPath = "Pakuri/CSVRuntime/PakuriCsvRuntimeAssetCatalog";
        private const string CatalogMonstersFileName = "catalog_monsters.csv";
        private const string MonstersFileName = "monsters.csv";
        private const string MonsterRewardChoicesFileName = "monster_modifier_skill_choice.csv";
        private const string MonsterSkillsProjectileFileName = "skills_projectile.csv";
        private const string MonsterSkillsLineAttackFileName = "skills_line_attack.csv";
        private const string MonsterSkillsAreaAttackFileName = "skills_area_attack.csv";
        private const string MonsterSkillsSingleAttackFileName = "skills_single_attack.csv";
        private const string MonsterSkillsBuffFileName = "skills_buff.csv";
        private const string MonsterSkillsPassiveFileName = "skills_passive.csv";
        private const string MonsterSkillNodesFileName = "monster_skill_nodes.csv";
        private const string MonsterSkillNodeParamsFileName = "monster_skill_node_params.csv";
        private const string MonsterSkillNodeDefinitionsFileName = "skill_node_definitions.csv";
        private const string MonsterSkillNodeDefinitionParamsFileName = "skill_node_definition_params.csv";
        private const string MonsterSkillTriggersFileName = "monster_skill_triger.csv";
        private const string MonsterSkillChoicesProjectileFileName = "skill_choices_projectile.csv";
        private const string MonsterSkillChoicesLineAttackFileName = "skill_choices_line_attack.csv";
        private const string MonsterSkillChoicesAreaAttackFileName = "skill_choices_area_attack.csv";
        private const string MonsterSkillChoicesSingleAttackFileName = "skill_choices_single_attack.csv";
        private const string MonsterSkillChoicesBuffFileName = "skill_choices_buff.csv";
        private const string MonsterSkillChoicesPassiveFileName = "skill_choices_passive.csv";
        private const string StatusEffectsFileName = "status_effects.csv";
        private const string EnemiesFileName = "enemies.csv";

        private static bool initialized;
        private static bool failed;
        private static GameDataCatalog runtimeCatalog;
        private static PakuriCsvRuntimeSourceCatalog runtimeSourceCatalog;
        private static PakuriCsvRuntimeAssetCatalog runtimeAssetCatalog;

        public static string GetImportedSourceAssetPath(string fileName)
        {
            return GetAuthoringSourceAssetPath(fileName);
        }

        public static string GetAuthoringSourceAssetPath(string fileName)
        {
            switch (fileName)
            {
                case CatalogMonstersFileName:
                    return $"{AuthoringCatalogCsvAssetRoot}/{fileName}";
                case MonstersFileName:
                case MonsterRewardChoicesFileName:
                    return $"{AuthoringMonsterCsvAssetRoot}/{fileName}";
                case MonsterSkillsProjectileFileName:
                case MonsterSkillsLineAttackFileName:
                case MonsterSkillsAreaAttackFileName:
                case MonsterSkillsSingleAttackFileName:
                case MonsterSkillsBuffFileName:
                case MonsterSkillsPassiveFileName:
                    return $"{AuthoringMonsterSkillBaseCsvAssetRoot}/{fileName}";
                case MonsterSkillNodesFileName:
                case MonsterSkillNodeParamsFileName:
                    return $"{AuthoringMonsterSkillNodeCsvAssetRoot}/{fileName}";
                case MonsterSkillNodeDefinitionsFileName:
                case MonsterSkillNodeDefinitionParamsFileName:
                    return $"{AuthoringMonsterSkillNodeCsvAssetRoot}/definitions/{fileName}";
                case MonsterSkillTriggersFileName:
                    return $"{AuthoringMonsterSkillTriggerCsvAssetRoot}/{fileName}";
                case MonsterSkillChoicesProjectileFileName:
                case MonsterSkillChoicesLineAttackFileName:
                case MonsterSkillChoicesAreaAttackFileName:
                case MonsterSkillChoicesSingleAttackFileName:
                case MonsterSkillChoicesBuffFileName:
                case MonsterSkillChoicesPassiveFileName:
                    return $"{AuthoringMonsterSkillChoiceCsvAssetRoot}/{fileName}";
                case StatusEffectsFileName:
                    return $"{AuthoringStatusCsvAssetRoot}/{fileName}";
                case EnemiesFileName:
                    return $"{AuthoringEnemyCsvAssetRoot}/{fileName}";
                default:
                    return $"{AuthoringCsvAssetRoot}/{fileName}";
            }
        }

        public static bool IsRuntimeCsvSourceAssetPath(string assetPath)
        {
            return IsAuthoringCsvSourceAssetPath(assetPath);
        }

        public static bool IsAuthoringCsvSourceAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            var normalized = assetPath.Replace('\\', '/');
            return normalized.StartsWith(AuthoringCsvAssetRoot + "/", StringComparison.OrdinalIgnoreCase)
                && normalized.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        }

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
