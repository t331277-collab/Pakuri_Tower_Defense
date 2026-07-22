using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using static Pakuri.Data.CsvDataValidator;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.GameDataCatalogBuilder;
#if UNITY_EDITOR
using UnityEditor;
#endif


/*
 * Scene 로드 전에 CSV 런타임 데이터를 한 번 초기화하는 진입점.
 * Resources 원본을 파싱하고 카탈로그 조회를 등록한 뒤 참조와 스킬 컴파일 결과를
 * 한 번 검증해 완성된 GameDataCatalog를 전역 조회 대상으로 제공한다.
 */
namespace Pakuri.Data
{
    public static class GameDataLoader
    {
        internal const string CsvDataAssetRoot = "Assets/CSVdata";
        internal const string AuthoringCsvAssetRoot = CsvDataAssetRoot + "/authoring";
        internal const string AuthoringCatalogCsvAssetRoot = AuthoringCsvAssetRoot + "/catalog";
        internal const string AuthoringMonsterCsvAssetRoot = AuthoringCsvAssetRoot + "/monster";
        internal const string AuthoringMonsterSkillCsvAssetRoot = AuthoringMonsterCsvAssetRoot + "/skills";
        internal const string AuthoringMonsterSkillBaseCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/base";
        internal const string AuthoringMonsterSkillChoiceCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/choices";
        internal const string AuthoringMonsterSkillTriggerCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/triggers";
        internal const string AuthoringMonsterSkillNodeCsvAssetRoot = AuthoringMonsterSkillCsvAssetRoot + "/nodes";
        internal const string AuthoringEnemyCsvAssetRoot = AuthoringCsvAssetRoot + "/enemy";
        internal const string AuthoringEnemySkillCsvAssetRoot = AuthoringEnemyCsvAssetRoot + "/skills";
        internal const string AuthoringEnemySkillBaseCsvAssetRoot = AuthoringEnemySkillCsvAssetRoot + "/base";
        internal const string AuthoringEnemySkillTriggerCsvAssetRoot = AuthoringEnemySkillCsvAssetRoot + "/triggers";
        internal const string AuthoringStatusCsvAssetRoot = AuthoringCsvAssetRoot + "/status";
        internal const string AuthoringSourceAssetRoot = AuthoringCsvAssetRoot;
        internal const string RuntimeResourcesFolderAssetPath = "Assets/Resources/Pakuri/CSVRuntime";
        internal const string RuntimeCatalogAssetPath = RuntimeResourcesFolderAssetPath + "/CsvRuntimeCatalog.asset";
        internal const string RuntimeCatalogResourcesPath = "Pakuri/CSVRuntime/CsvRuntimeCatalog";
        internal const string CatalogMonstersFileName = "catalog_monsters.csv";
        internal const string MonstersFileName = "monsters.csv";
        internal const string MonsterRewardChoicesFileName = "monster_modifier_skill_choice.csv";
        internal const string MonsterSkillsProjectileFileName = "skills_projectile.csv";
        internal const string MonsterSkillsLineAttackFileName = "skills_line_attack.csv";
        internal const string MonsterSkillsAreaAttackFileName = "skills_area_attack.csv";
        internal const string MonsterSkillsSingleAttackFileName = "skills_single_attack.csv";
        internal const string MonsterSkillsBuffFileName = "skills_buff.csv";
        internal const string MonsterSkillsPassiveFileName = "skills_passive.csv";
        internal const string MonsterSkillNodeDefinitionsFileName = "skill_node_definitions.csv";
        internal const string MonsterSkillNodeDefinitionParamsFileName = "skill_node_definition_params.csv";
        internal const string MonsterSkillTriggersFileName = "monster_skill_triger.csv";
        internal const string MonsterSkillChoicesProjectileFileName = "skill_choices_projectile.csv";
        internal const string MonsterSkillChoicesLineAttackFileName = "skill_choices_line_attack.csv";
        internal const string MonsterSkillChoicesAreaAttackFileName = "skill_choices_area_attack.csv";
        internal const string MonsterSkillChoicesSingleAttackFileName = "skill_choices_single_attack.csv";
        internal const string MonsterSkillChoicesBuffFileName = "skill_choices_buff.csv";
        internal const string MonsterSkillChoicesPassiveFileName = "skill_choices_passive.csv";
        internal const string StatusEffectsFileName = "status_effects.csv";
        internal const string EnemiesFileName = "enemies.csv";

        internal static bool initialized;
        internal static bool failed;
        internal static GameDataCatalog runtimeCatalog;
        internal static CsvRuntimeCatalog runtimeCsvCatalog;

        public static GameDataCatalog CurrentCatalog
        {
            get
            {
                if (runtimeCatalog != null)
                {
                    return runtimeCatalog;
                }

                EnsureInitialized();
                if (failed)
                {
                    return null;
                }

                return runtimeCatalog;
            }
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        public static string GetImportedSourceAssetPath(string fileName /* 파일 이름 */)
        {
            return GetAuthoringSourceAssetPath(fileName);
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        public static string GetAuthoringSourceAssetPath(string fileName /* 파일 이름 */)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        public static bool IsAuthoringCsvSourceAssetPath(string assetPath /* 에셋 경로 */)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            var normalized = assetPath.Replace('\\', '/');
            return normalized.StartsWith(AuthoringCsvAssetRoot + "/", StringComparison.OrdinalIgnoreCase)
                && normalized.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        }

        /*
         * Scene 로드 전에 CSV 런타임 데이터를 초기화한다.
         */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitializeBeforeSceneLoad()
        {
            EnsureInitialized();
        }

        /*
         * CSV 런타임 데이터가 한 번만 초기화되도록 한다.
         */
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
                    "GameDataLoader failed with an unexpected exception.",
                    new List<string> { ex.ToString() });
            }
        }

        /*
         * 원본 CSV를 읽고 검증한 뒤 런타임 카탈로그를 등록한다.
         */
        internal static GameDataCatalog LoadAndValidateRuntimeCatalog()
        {
            runtimeCsvCatalog = LoadRuntimeCatalogOrThrow();
            var source = LoadSourceModel(runtimeCsvCatalog);
            ValidateSourceModelOrThrow(source, runtimeCsvCatalog);
            var catalog = BuildRuntimeCatalog(source);
            catalog.RebuildLookup();
            runtimeCatalog = catalog;
            initialized = true;
            return catalog;
        }

        /*
         * 로그에 사용할 설명 문자열을 만든다.
         */
        internal static string FormatRuntimeCatalogSummary(GameDataCatalog catalog /* 불러온 게임 데이터 목록 */)
        {
            return
                $"GameDataLoader loaded runtime catalog from resource source '{RuntimeCatalogResourcesPath}' " +
                $"with {catalog.Monsters.Length} monsters, {catalog.StageOneEnemies.Length} stage-one enemies, " +
                $"and {catalog.StageTwoEnemies.Length} stage-two enemies.";
        }

        /*
         * 치명적인 CSV 오류를 기록하고 실행을 종료한다.
         */
        internal static void FailAndQuit(string message /* 메시지 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static CsvRuntimeCatalog LoadRuntimeCatalogOrThrow()
        {
            var sourceCatalog = Resources.Load<CsvRuntimeCatalog>(RuntimeCatalogResourcesPath);
            if (sourceCatalog == null)
            {
                throw new CsvFatalException(
                    $"Pakuri CSV runtime catalog is missing at Resources path '{RuntimeCatalogResourcesPath}'.",
                    new List<string>
                    {
                        $"Expected asset path: {RuntimeCatalogAssetPath}"
                    });
            }

            var missingAssets = new List<string>();
            if (sourceCatalog.CatalogMonsters == null)
            {
                missingAssets.Add(CatalogMonstersFileName);
            }
            if (sourceCatalog.Monsters == null)
            {
                missingAssets.Add(MonstersFileName);
            }
            if (sourceCatalog.MonsterRewardChoices == null)
            {
                missingAssets.Add(MonsterRewardChoicesFileName);
            }
            if (sourceCatalog.MonsterSkillsProjectileFiles == null || sourceCatalog.MonsterSkillsProjectileFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillsProjectileFileName);
            }
            if (sourceCatalog.MonsterSkillsLineAttackFiles == null || sourceCatalog.MonsterSkillsLineAttackFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillsLineAttackFileName);
            }
            if (sourceCatalog.MonsterSkillsAreaAttackFiles == null || sourceCatalog.MonsterSkillsAreaAttackFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillsAreaAttackFileName);
            }
            if (sourceCatalog.MonsterSkillsSingleAttackFiles == null || sourceCatalog.MonsterSkillsSingleAttackFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillsSingleAttackFileName);
            }
            if (sourceCatalog.MonsterSkillsBuffFiles == null || sourceCatalog.MonsterSkillsBuffFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillsBuffFileName);
            }
            if (sourceCatalog.MonsterSkillsPassiveFiles == null || sourceCatalog.MonsterSkillsPassiveFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillsPassiveFileName);
            }
            if (sourceCatalog.MonsterSkillTriggerFiles == null || sourceCatalog.MonsterSkillTriggerFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillTriggersFileName);
            }
            if (sourceCatalog.MonsterSkillNodeDefinitions == null)
            {
                missingAssets.Add(MonsterSkillNodeDefinitionsFileName);
            }
            if (sourceCatalog.MonsterSkillNodeDefinitionParams == null)
            {
                missingAssets.Add(MonsterSkillNodeDefinitionParamsFileName);
            }
            if (sourceCatalog.MonsterSkillGraphNodeFiles == null || sourceCatalog.MonsterSkillGraphNodeFiles.Length == 0)
            {
                missingAssets.Add("skill_graph_nodes_*.csv");
            }
            if (sourceCatalog.MonsterSkillChoicesProjectileFiles == null || sourceCatalog.MonsterSkillChoicesProjectileFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillChoicesProjectileFileName);
            }
            if (sourceCatalog.MonsterSkillChoicesLineAttackFiles == null || sourceCatalog.MonsterSkillChoicesLineAttackFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillChoicesLineAttackFileName);
            }
            if (sourceCatalog.MonsterSkillChoicesAreaAttackFiles == null || sourceCatalog.MonsterSkillChoicesAreaAttackFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillChoicesAreaAttackFileName);
            }
            if (sourceCatalog.MonsterSkillChoicesSingleAttackFiles == null || sourceCatalog.MonsterSkillChoicesSingleAttackFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillChoicesSingleAttackFileName);
            }
            if (sourceCatalog.MonsterSkillChoicesBuffFiles == null || sourceCatalog.MonsterSkillChoicesBuffFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillChoicesBuffFileName);
            }
            if (sourceCatalog.MonsterSkillChoicesPassiveFiles == null || sourceCatalog.MonsterSkillChoicesPassiveFiles.Length == 0)
            {
                missingAssets.Add(MonsterSkillChoicesPassiveFileName);
            }
            if (sourceCatalog.StatusEffects == null)
            {
                missingAssets.Add(StatusEffectsFileName);
            }
            if (sourceCatalog.Enemies == null)
            {
                missingAssets.Add(EnemiesFileName);
            }
            if (sourceCatalog.EnemySkillBaseFiles == null || sourceCatalog.EnemySkillBaseFiles.Length == 0)
            {
                missingAssets.Add("enemy/skills/base/**/skills_*.csv");
            }
            if (sourceCatalog.EnemySkillTriggerFiles == null || sourceCatalog.EnemySkillTriggerFiles.Length == 0)
            {
                missingAssets.Add("enemy/skills/triggers/**/*_skill_triger.csv");
            }

            if (missingAssets.Count > 0)
            {
                throw new CsvFatalException(
                    $"Pakuri CSV runtime catalog at '{RuntimeCatalogResourcesPath}' has missing TextAsset references.",
                    new List<string>
                    {
                        "Missing files: " + string.Join(", ", missingAssets)
                    });
            }

            sourceCatalog.ResetLookups();
            return sourceCatalog;
        }

    }
}
