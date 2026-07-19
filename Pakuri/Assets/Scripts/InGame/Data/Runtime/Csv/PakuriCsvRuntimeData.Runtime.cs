using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pakuri.Data
{
    /*
     * CSV 자산을 불러와 검증과 런타임 카탈로그 생성을 조율한다.
     */
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

        /*
         * 계산에 필요한 값을 반환한다.
         */
        public static string GetImportedSourceAssetPath(string fileName)
        {
            return GetAuthoringSourceAssetPath(fileName);
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
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
                default:
                    return $"{AuthoringCsvAssetRoot}/{fileName}";
            }
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
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

        /*
         * Scene 로드 전에 CSV 런타임 데이터를 초기화한다.
         */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeSceneLoad()
        {
            EnsureInitialized();
        }

        /*
         * CSV 카탈로그를 반환하고 초기화 실패 시 null을 반환한다.
         */
        public static GameDataCatalog ResolveCatalogOrFallback(GameDataCatalog fallback)
        {
            EnsureInitialized();
            if (failed)
            {
                return null;
            }

            return runtimeCatalog != null ? runtimeCatalog : fallback;
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
                    "PakuriCsvRuntimeData failed with an unexpected exception.",
                    new List<string> { ex.ToString() });
            }
        }

        /*
         * 원본 CSV를 읽고 검증한 뒤 런타임 카탈로그를 등록한다.
         */
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

        /*
         * 로그에 사용할 설명 문자열을 만든다.
         */
        private static string FormatRuntimeCatalogSummary(GameDataCatalog catalog)
        {
            return
                $"PakuriCsvRuntimeData loaded runtime catalog from resource source '{SourceCatalogResourcesPath}' " +
                $"with {catalog.Monsters.Length} monsters, {catalog.StageOneEnemies.Length} stage-one enemies, " +
                $"and {catalog.StageTwoEnemies.Length} stage-two enemies.";
        }

        /*
         * 치명적인 CSV 오류를 기록하고 실행을 종료한다.
         */
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static PakuriCsvRuntimeSourceCatalog LoadSourceCatalogOrThrow()
        {
            var sourceCatalog = Resources.Load<PakuriCsvRuntimeSourceCatalog>(SourceCatalogResourcesPath);
            if (sourceCatalog == null)
            {
                throw new CsvFatalException(
                    $"Pakuri CSV runtime source catalog is missing at Resources path '{SourceCatalogResourcesPath}'.",
                    new List<string>
                    {
                        $"Expected asset path: {SourceCatalogAssetPath}"
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
                    $"Pakuri CSV runtime source catalog at '{SourceCatalogResourcesPath}' has missing TextAsset references.",
                    new List<string>
                    {
                        "Missing files: " + string.Join(", ", missingAssets)
                    });
            }

            return sourceCatalog;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static PakuriCsvRuntimeAssetCatalog LoadAssetCatalogOrThrow()
        {
            var assetCatalog = Resources.Load<PakuriCsvRuntimeAssetCatalog>(AssetCatalogResourcesPath);
            if (assetCatalog == null)
            {
                throw new CsvFatalException(
                    $"Pakuri CSV runtime asset catalog is missing at Resources path '{AssetCatalogResourcesPath}'.",
                    new List<string>
                    {
                        $"Expected asset path: {AssetCatalogAssetPath}"
                    });
            }

            assetCatalog.ResetLookups();
            return assetCatalog;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static SourceModel LoadSourceModel(PakuriCsvRuntimeSourceCatalog sourceCatalog)
        {
            var model = new SourceModel();

            var catalogMonsterTable = CsvTable.Load(sourceCatalog.CatalogMonsters, CatalogMonstersFileName);
            var monsterTable = CsvTable.Load(sourceCatalog.Monsters, MonstersFileName);
            var rewardChoiceTable = CsvTable.Load(sourceCatalog.MonsterRewardChoices, MonsterRewardChoicesFileName);
            var projectileSkillAssets = sourceCatalog.MonsterSkillsProjectileFiles;
            var lineAttackSkillAssets = sourceCatalog.MonsterSkillsLineAttackFiles;
            var areaAttackSkillAssets = sourceCatalog.MonsterSkillsAreaAttackFiles;
            var singleAttackSkillAssets = sourceCatalog.MonsterSkillsSingleAttackFiles;
            var buffSkillAssets = sourceCatalog.MonsterSkillsBuffFiles;
            var passiveSkillAssets = sourceCatalog.MonsterSkillsPassiveFiles;
            var skillTriggerAssets = sourceCatalog.MonsterSkillTriggerFiles;
            var skillGraphNodeAssets = sourceCatalog.MonsterSkillGraphNodeFiles;
            var skillNodeDefinitionTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitions,
                MonsterSkillNodeDefinitionsFileName);
            var skillNodeDefinitionParamTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitionParams,
                MonsterSkillNodeDefinitionParamsFileName);
            var projectileChoiceAssets = sourceCatalog.MonsterSkillChoicesProjectileFiles;
            var lineAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesLineAttackFiles;
            var areaAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesAreaAttackFiles;
            var singleAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesSingleAttackFiles;
            var buffChoiceAssets = sourceCatalog.MonsterSkillChoicesBuffFiles;
            var passiveChoiceAssets = sourceCatalog.MonsterSkillChoicesPassiveFiles;
            var statusEffectTable = CsvTable.Load(sourceCatalog.StatusEffects, StatusEffectsFileName);
            var enemyTable = CsvTable.Load(sourceCatalog.Enemies, EnemiesFileName);

            foreach (var record in catalogMonsterTable.Records)
            {
                var row = ParseCatalogEntry(record, "monster_id");
                AddUnique(model.CatalogMonsters, row.Id, row, record);
            }

            foreach (var record in monsterTable.Records)
            {
                var row = ParseMonsterRow(record);
                AddUnique(model.Monsters, row.Id, row, record);
            }

            foreach (var record in rewardChoiceTable.Records)
            {
                var row = ParseRewardChoiceRow(record);
                AddUnique(model.RewardChoices, row.Id, row, record);
            }

            LoadSkillRows(
                model,
                projectileSkillAssets,
                MonsterSkillsProjectileFileName,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillRows(
                model,
                lineAttackSkillAssets,
                MonsterSkillsLineAttackFileName,
                SkillRuntimeKind.LineAttack);
            LoadSkillRows(
                model,
                areaAttackSkillAssets,
                MonsterSkillsAreaAttackFileName,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillRows(
                model,
                singleAttackSkillAssets,
                MonsterSkillsSingleAttackFileName,
                SkillRuntimeKind.SingleAttack);
            LoadSkillRows(
                model,
                buffSkillAssets,
                MonsterSkillsBuffFileName,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillRows(
                model,
                passiveSkillAssets,
                MonsterSkillsPassiveFileName,
                SkillRuntimeKind.Passive);

            foreach (var record in skillNodeDefinitionTable.Records)
            {
                var row = ParseSkillNodeTypeRow(record);
                AddUnique(model.SkillNodeTypes, row.Id, row, record);
            }

            foreach (var record in skillNodeDefinitionParamTable.Records)
            {
                model.SkillNodeTypeParams.Add(ParseSkillNodeTypeParamRow(record));
            }

            for (var assetIndex = 0; assetIndex < skillTriggerAssets.Length; assetIndex++)
            {
                var skillTriggerTable = CsvTable.Load(
                    skillTriggerAssets[assetIndex],
                    GetTextAssetCsvTableName(skillTriggerAssets[assetIndex], MonsterSkillTriggersFileName));
                foreach (var record in skillTriggerTable.Records)
                {
                    var row = ParseSkillTriggerRow(record, skillTriggerTable.TableName);
                    AddUnique(model.SkillTriggers, row.Id, row, record);
                }
            }

            LoadSkillChoiceRows(
                model,
                projectileChoiceAssets,
                MonsterSkillChoicesProjectileFileName,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillChoiceRows(
                model,
                lineAttackChoiceAssets,
                MonsterSkillChoicesLineAttackFileName,
                SkillRuntimeKind.LineAttack);
            LoadSkillChoiceRows(
                model,
                areaAttackChoiceAssets,
                MonsterSkillChoicesAreaAttackFileName,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillChoiceRows(
                model,
                singleAttackChoiceAssets,
                MonsterSkillChoicesSingleAttackFileName,
                SkillRuntimeKind.SingleAttack);
            LoadSkillChoiceRows(
                model,
                buffChoiceAssets,
                MonsterSkillChoicesBuffFileName,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillChoiceRows(
                model,
                passiveChoiceAssets,
                MonsterSkillChoicesPassiveFileName,
                SkillRuntimeKind.Passive);

            for (var assetIndex = 0; assetIndex < skillGraphNodeAssets.Length; assetIndex++)
            {
                var graphNodeTable = CsvTable.Load(
                    skillGraphNodeAssets[assetIndex],
                    GetTextAssetCsvTableName(skillGraphNodeAssets[assetIndex], "skill_graph_nodes.csv"));
                foreach (var record in graphNodeTable.Records)
                {
                    model.SkillGraphNodes.Add(ParseSkillGraphNodeRow(record));
                }
            }

            MaterializeSkillGraphRows(model);

            foreach (var record in statusEffectTable.Records)
            {
                var row = ParseStatusEffectRow(record);
                AddUnique(model.StatusEffects, row.Id, row, record);
            }

            foreach (var record in enemyTable.Records)
            {
                var row = ParseEnemyMigrationRow(record);
                AddUnique(model.Enemies, row.Id, row, record);
            }

            var enemyBaseAssets = sourceCatalog.EnemySkillBaseFiles ?? Array.Empty<TextAsset>();
            for (var assetIndex = 0; assetIndex < enemyBaseAssets.Length; assetIndex++)
            {
                var asset = enemyBaseAssets[assetIndex];
                var tableName = GetTextAssetCsvTableName(asset, "enemy_base_skills.csv");
                var table = CsvTable.Load(asset, tableName);
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyBaseSkillRow(record, tableName);
                    AddUnique(model.EnemyBaseSkills, row.Skill.Id, row, record);
                }
            }

            var enemyTriggerAssets = sourceCatalog.EnemySkillTriggerFiles ?? Array.Empty<TextAsset>();
            for (var assetIndex = 0; assetIndex < enemyTriggerAssets.Length; assetIndex++)
            {
                var asset = enemyTriggerAssets[assetIndex];
                var table = CsvTable.Load(asset, GetTextAssetCsvTableName(asset, "enemy_skill_triger.csv"));
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyMigrationTriggerRow(record);
                    AddUnique(model.EnemyMigrationTriggers, row.Id, row, record);
                }
            }

            return model;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static void LoadSkillRows(
            SourceModel model,
            TextAsset[] skillAssets,
            string fallbackTableName,
            params SkillRuntimeKind[] allowedRuntimeKinds)
        {
            for (var assetIndex = 0; assetIndex < skillAssets.Length; assetIndex++)
            {
                var skillAsset = skillAssets[assetIndex];
                LoadSkillRows(
                    model,
                    skillAsset,
                    GetTextAssetCsvTableName(skillAsset, fallbackTableName),
                    allowedRuntimeKinds);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static void LoadSkillRows(
            SourceModel model,
            TextAsset skillAsset,
            string tableName,
            params SkillRuntimeKind[] allowedRuntimeKinds)
        {
            var skillTable = CsvTable.Load(skillAsset, tableName);
            foreach (var record in skillTable.Records)
            {
                var row = ParseSkillRow(record, tableName);
                if (!IsAllowedSkillRuntimeKind(row.RuntimeKind, allowedRuntimeKinds))
                {
                    throw new CsvFatalException(
                        $"CSV table '{tableName}' contains skill '{row.Id}' with unsupported runtime_kind '{row.RuntimeKind}'.",
                        new List<string>
                        {
                            $"Move skill '{row.Id}' to the split monster skill CSV that owns runtime_kind '{row.RuntimeKind}'."
                        });
                }

                AddUnique(model.Skills, row.Id, row, record);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static void LoadSkillChoiceRows(
            SourceModel model,
            TextAsset[] choiceAssets,
            string fallbackTableName,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds)
        {
            for (var assetIndex = 0; assetIndex < choiceAssets.Length; assetIndex++)
            {
                var choiceAsset = choiceAssets[assetIndex];
                LoadSkillChoiceRows(
                    model,
                    choiceAsset,
                    GetTextAssetCsvTableName(choiceAsset, fallbackTableName),
                    allowedOwnerRuntimeKinds);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static void LoadSkillChoiceRows(
            SourceModel model,
            TextAsset choiceAsset,
            string tableName,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds)
        {
            var choiceTable = CsvTable.Load(choiceAsset, tableName);
            foreach (var record in choiceTable.Records)
            {
                var row = ParseSkillChoiceRow(record, tableName);
                if (!model.Skills.TryGetValue(row.SkillId, out var ownerSkill))
                {
                    throw new CsvFatalException(
                        $"CSV table '{tableName}' contains choice '{row.Id}' for unknown owner skill '{row.SkillId}'.",
                        new List<string>
                        {
                            $"Define skill '{row.SkillId}' in the split monster skill CSV before adding its choices."
                        });
                }

                if (!IsAllowedSkillRuntimeKind(ownerSkill.RuntimeKind, allowedOwnerRuntimeKinds))
                {
                    throw new CsvFatalException(
                        $"CSV table '{tableName}' contains choice '{row.Id}' for skill '{row.SkillId}' with unsupported owner runtime_kind '{ownerSkill.RuntimeKind}'.",
                        new List<string>
                        {
                            $"Move choice '{row.Id}' to the split monster skill choice CSV that owns runtime_kind '{ownerSkill.RuntimeKind}'."
                        });
                }

                AddUnique(model.SkillChoices, row.Id, row, record);
            }
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        private static bool IsAllowedSkillRuntimeKind(
            SkillRuntimeKind runtimeKind,
            SkillRuntimeKind[] allowedRuntimeKinds)
        {
            for (var i = 0; i < allowedRuntimeKinds.Length; i++)
            {
                if (allowedRuntimeKinds[i] == runtimeKind)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * TextAsset 이름으로 CSV 테이블 이름을 만든다.
         */
        private static string GetTextAssetCsvTableName(TextAsset asset, string fallbackTableName)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.name))
            {
                return fallbackTableName;
            }

            return asset.name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? asset.name
                : asset.name + ".csv";
        }

        /*
         * 중복 ID를 거부하고 원본 행을 사전에 추가한다.
         */
        private static void AddUnique<T>(Dictionary<string, T> dictionary, string id, T value, CsvRecord record)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new CsvFatalException(
                    $"CSV row {record.RowNumber} in '{record.TableName}' is missing a required id value.");
            }

            if (dictionary.ContainsKey(id))
            {
                throw new CsvFatalException(
                    $"CSV row {record.RowNumber} in '{record.TableName}' uses duplicate id '{id}'.");
            }

            dictionary.Add(id, value);
        }
    }
}
