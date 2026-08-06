/*
 * 역할: 런타임 게임 데이터 로딩 진입점.
 * 책임: CSV 원본과 에셋 카탈로그를 로드·검증하고 런타임 카탈로그를 구성해 상태를 공개한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using static Pakuri.Data.CsvDataValidator;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvSourceLoader;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.GameDataCatalogBuilder;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
        internal const string AuthoringSummonCsvAssetRoot = AuthoringCsvAssetRoot + "/summon";
        internal const string AuthoringSummonSkillCsvAssetRoot = AuthoringSummonCsvAssetRoot + "/skill";
        internal const string ArtifactCsvAssetRoot = CsvDataAssetRoot + "/Artifact";
        internal const string ArtifactEffectCsvAssetRoot = ArtifactCsvAssetRoot + "/Effect";
        internal const string ArtifactSkillCsvAssetRoot = ArtifactCsvAssetRoot + "/Skill";
        internal const string StageFlowCsvAssetRoot = CsvDataAssetRoot + "/stage_flow";
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
        internal const string ArtifactsFileName = "artifacts.csv";
        internal const string ArtifactSynergiesFileName = "artifact_synergies.csv";
        internal const string ArtifactEffectsFileName = "artifact_effects.csv";
        internal const string ArtifactSynergyEffectsFileName = "artifact_synergy_effects.csv";
        internal const string ArtifactSkillGraphNodeFileName = "skill_graph_nodes_artifact.csv";
        internal const string ArtifactSkillTriggerFileName = "artifact_skill_triger.csv";
        internal const string SummonUnitsFileName = "summon_units.csv";
        internal const string SummonSkillsFileName = "summon_units_skill.csv";

        internal static bool initialized;
        internal static bool failed;
        internal static GameDataCatalog runtimeCatalog;

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
                case SummonUnitsFileName:
                    return $"{AuthoringSummonCsvAssetRoot}/{fileName}";
                case SummonSkillsFileName:
                    return $"{AuthoringSummonSkillCsvAssetRoot}/{fileName}";
                default:
                    return $"{AuthoringCsvAssetRoot}/{fileName}";
            }
        }

        public static bool IsAuthoringCsvSourceAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            var normalized = assetPath.Replace('\\', '/');
            return (normalized.StartsWith(AuthoringCsvAssetRoot + "/", StringComparison.OrdinalIgnoreCase)
                    || normalized.StartsWith(ArtifactCsvAssetRoot + "/", StringComparison.OrdinalIgnoreCase))
                && normalized.EndsWith(".csv", StringComparison.OrdinalIgnoreCase);
        }

        /// BeforeSceneLoad를 초기화한다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitializeBeforeSceneLoad()
        {
            EnsureInitialized();
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
                    "GameDataLoader failed with an unexpected exception.",
                    new List<string> { ex.ToString() });
            }
        }

        /// AndValidateRuntimeCatalog를 불러온다.
        internal static GameDataCatalog LoadAndValidateRuntimeCatalog()
        {
            var sourceCatalog = LoadRuntimeCatalogOrThrow();
            var source = LoadSourceModel(sourceCatalog);
            return BuildValidatedRuntimeCatalog(sourceCatalog, source);
        }

        internal static GameDataCatalog BuildValidatedRuntimeCatalog(
            CsvRuntimeCatalog sourceCatalog,
            SourceModel source)
        {
            ValidateSourceModelOrThrow(source, sourceCatalog);
            var catalog = BuildRuntimeCatalog(source, sourceCatalog);
            catalog.RebuildLookup();
            runtimeCatalog = catalog;
            initialized = true;
            return catalog;
        }

        internal static string FormatRuntimeCatalogSummary(GameDataCatalog catalog)
        {
            return
                $"GameDataLoader loaded runtime catalog from resource source '{RuntimeCatalogResourcesPath}' " +
                $"with {catalog.Monsters.Length} monsters, {catalog.StageOneEnemies.Length} stage-one enemies, " +
                $"{catalog.StageTwoEnemies.Length} stage-two enemies, {catalog.Artifacts.Length} artifacts, " +
                $"{catalog.ArtifactSynergies.Length} artifact synergies, and {catalog.Summons.Length} summons.";
        }

        internal static void FailAndQuit(string message, List<string> errors)
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

        /// LoadRuntimeCatalog 데이터를 검증하고 유효하지 않으면 예외를 던진다.
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

            void Require(bool present, string name)
            {
                if (!present)
                {
                    missingAssets.Add(name);
                }
            }

            Require(sourceCatalog.CatalogMonsters != null, CatalogMonstersFileName);
            Require(sourceCatalog.Monsters != null, MonstersFileName);
            Require(sourceCatalog.MonsterRewardChoices != null, MonsterRewardChoicesFileName);
            Require(sourceCatalog.MonsterSkillsProjectileFiles?.Length > 0, MonsterSkillsProjectileFileName);
            Require(sourceCatalog.MonsterSkillsLineAttackFiles?.Length > 0, MonsterSkillsLineAttackFileName);
            Require(sourceCatalog.MonsterSkillsAreaAttackFiles?.Length > 0, MonsterSkillsAreaAttackFileName);
            Require(sourceCatalog.MonsterSkillsSingleAttackFiles?.Length > 0, MonsterSkillsSingleAttackFileName);
            Require(sourceCatalog.MonsterSkillsBuffFiles?.Length > 0, MonsterSkillsBuffFileName);
            Require(sourceCatalog.MonsterSkillsPassiveFiles?.Length > 0, MonsterSkillsPassiveFileName);
            Require(sourceCatalog.MonsterSkillTriggerFiles?.Length > 0, MonsterSkillTriggersFileName);
            Require(sourceCatalog.MonsterSkillNodeDefinitions != null, MonsterSkillNodeDefinitionsFileName);
            Require(sourceCatalog.MonsterSkillNodeDefinitionParams != null, MonsterSkillNodeDefinitionParamsFileName);
            Require(sourceCatalog.MonsterSkillGraphNodeFiles?.Length > 0, "skill_graph_nodes_*.csv");
            Require(sourceCatalog.MonsterSkillChoicesProjectileFiles?.Length > 0, MonsterSkillChoicesProjectileFileName);
            Require(sourceCatalog.MonsterSkillChoicesLineAttackFiles?.Length > 0, MonsterSkillChoicesLineAttackFileName);
            Require(sourceCatalog.MonsterSkillChoicesAreaAttackFiles?.Length > 0, MonsterSkillChoicesAreaAttackFileName);
            Require(sourceCatalog.MonsterSkillChoicesSingleAttackFiles?.Length > 0, MonsterSkillChoicesSingleAttackFileName);
            Require(sourceCatalog.MonsterSkillChoicesBuffFiles?.Length > 0, MonsterSkillChoicesBuffFileName);
            Require(sourceCatalog.MonsterSkillChoicesPassiveFiles?.Length > 0, MonsterSkillChoicesPassiveFileName);
            Require(sourceCatalog.StatusEffects != null, StatusEffectsFileName);
            Require(sourceCatalog.Enemies != null, EnemiesFileName);
            Require(sourceCatalog.EnemySkillBaseFiles?.Length > 0, "enemy/skills/baseskills_*.csv");
            Require(sourceCatalog.EnemySkillTriggerFiles?.Length > 0, "enemy/skills/triggers*_skill_triger.csv");
            Require(sourceCatalog.Artifacts != null, ArtifactsFileName);
            Require(sourceCatalog.ArtifactSynergies != null, ArtifactSynergiesFileName);
            Require(sourceCatalog.ArtifactEffects != null, ArtifactEffectsFileName);
            Require(sourceCatalog.ArtifactSynergyEffects != null, ArtifactSynergyEffectsFileName);
            Require(sourceCatalog.ArtifactSkillGraphNodeFiles?.Length > 0, ArtifactSkillGraphNodeFileName);
            Require(sourceCatalog.ArtifactSkillTriggerFiles?.Length > 0, ArtifactSkillTriggerFileName);
            Require(sourceCatalog.SummonUnits != null, SummonUnitsFileName);
            Require(sourceCatalog.SummonSkills != null, SummonSkillsFileName);
            Require(sourceCatalog.StageDay != null, StageFileNames.StageDay);
            Require(sourceCatalog.Stage1Encounter != null, "stage_flow/Stage1/" + StageFileNames.StageEncounter);
            Require(sourceCatalog.Stage1Reward != null, "stage_flow/Stage1/" + StageFileNames.StageReward);
            Require(sourceCatalog.Stage2Encounter != null, "stage_flow/Stage2/" + StageFileNames.StageEncounter);
            Require(sourceCatalog.Stage2Reward != null, "stage_flow/Stage2/" + StageFileNames.StageReward);

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
