using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
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
            if (sourceCatalog.CatalogStageOneEnemies == null)
            {
                missingAssets.Add(CatalogStageOneEnemiesFileName);
            }
            if (sourceCatalog.CatalogStageTwoEnemies == null)
            {
                missingAssets.Add(CatalogStageTwoEnemiesFileName);
            }
            if (sourceCatalog.Monsters == null)
            {
                missingAssets.Add(MonstersFileName);
            }
            if (sourceCatalog.MonsterRewardChoices == null)
            {
                missingAssets.Add(MonsterRewardChoicesFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillsProjectileFiles, sourceCatalog.MonsterSkillsProjectile))
            {
                missingAssets.Add(MonsterSkillsProjectileFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillsLineAttackFiles, sourceCatalog.MonsterSkillsLineAttack))
            {
                missingAssets.Add(MonsterSkillsLineAttackFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillsAreaAttackFiles, sourceCatalog.MonsterSkillsAreaAttack))
            {
                missingAssets.Add(MonsterSkillsAreaAttackFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillsSingleAttackFiles, sourceCatalog.MonsterSkillsSingleAttack))
            {
                missingAssets.Add(MonsterSkillsSingleAttackFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillsBuffFiles, sourceCatalog.MonsterSkillsBuff))
            {
                missingAssets.Add(MonsterSkillsBuffFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillsPassiveFiles, sourceCatalog.MonsterSkillsPassive))
            {
                missingAssets.Add(MonsterSkillsPassiveFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillTriggerFiles, sourceCatalog.MonsterSkillTriggers))
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
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillChoicesProjectileFiles, sourceCatalog.MonsterSkillChoicesProjectile))
            {
                missingAssets.Add(MonsterSkillChoicesProjectileFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillChoicesLineAttackFiles, sourceCatalog.MonsterSkillChoicesLineAttack))
            {
                missingAssets.Add(MonsterSkillChoicesLineAttackFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillChoicesAreaAttackFiles, sourceCatalog.MonsterSkillChoicesAreaAttack))
            {
                missingAssets.Add(MonsterSkillChoicesAreaAttackFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillChoicesSingleAttackFiles, sourceCatalog.MonsterSkillChoicesSingleAttack))
            {
                missingAssets.Add(MonsterSkillChoicesSingleAttackFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillChoicesBuffFiles, sourceCatalog.MonsterSkillChoicesBuff))
            {
                missingAssets.Add(MonsterSkillChoicesBuffFileName);
            }
            if (!HasAnyCsvAsset(sourceCatalog.MonsterSkillChoicesPassiveFiles, sourceCatalog.MonsterSkillChoicesPassive))
            {
                missingAssets.Add(MonsterSkillChoicesPassiveFileName);
            }
            if (sourceCatalog.StatusEffects == null)
            {
                missingAssets.Add(StatusEffectsFileName);
            }
            if (sourceCatalog.StageOneEnemies == null)
            {
                missingAssets.Add(StageOneEnemiesFileName);
            }
            if (sourceCatalog.StageTwoEnemies == null)
            {
                missingAssets.Add(StageTwoEnemiesFileName);
            }
            if (sourceCatalog.EnemySkills == null)
            {
                missingAssets.Add(EnemySkillDataFileName);
            }
            if (sourceCatalog.Enemies == null)
            {
                missingAssets.Add(EnemiesFileName);
            }
            if (sourceCatalog.EnemySkillLoadouts == null)
            {
                missingAssets.Add(EnemySkillLoadoutsFileName);
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

        private static SourceModel LoadSourceModel(PakuriCsvRuntimeSourceCatalog sourceCatalog)
        {
            var model = new SourceModel();

            var catalogMonsterTable = CsvTable.Load(sourceCatalog.CatalogMonsters, CatalogMonstersFileName);
            var catalogEnemyTable = CsvTable.Load(sourceCatalog.CatalogStageOneEnemies, CatalogStageOneEnemiesFileName);
            var catalogStageTwoEnemyTable = CsvTable.Load(sourceCatalog.CatalogStageTwoEnemies, CatalogStageTwoEnemiesFileName);
            var monsterTable = CsvTable.Load(sourceCatalog.Monsters, MonstersFileName);
            var rewardChoiceTable = CsvTable.Load(sourceCatalog.MonsterRewardChoices, MonsterRewardChoicesFileName);
            var projectileSkillAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillsProjectileFiles,
                sourceCatalog.MonsterSkillsProjectile);
            var lineAttackSkillAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillsLineAttackFiles,
                sourceCatalog.MonsterSkillsLineAttack);
            var areaAttackSkillAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillsAreaAttackFiles,
                sourceCatalog.MonsterSkillsAreaAttack);
            var singleAttackSkillAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillsSingleAttackFiles,
                sourceCatalog.MonsterSkillsSingleAttack);
            var buffSkillAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillsBuffFiles,
                sourceCatalog.MonsterSkillsBuff);
            var passiveSkillAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillsPassiveFiles,
                sourceCatalog.MonsterSkillsPassive);
            var skillNodeAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillNodeFiles,
                sourceCatalog.MonsterSkillNodes);
            var skillNodeParamAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillNodeParamFiles,
                sourceCatalog.MonsterSkillNodeParams);
            var skillTriggerAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillTriggerFiles,
                sourceCatalog.MonsterSkillTriggers);
            var skillGraphNodeAssets = sourceCatalog.MonsterSkillGraphNodeFiles ?? Array.Empty<TextAsset>();
            var skillNodeDefinitionTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitions,
                MonsterSkillNodeDefinitionsFileName);
            var skillNodeDefinitionParamTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitionParams,
                MonsterSkillNodeDefinitionParamsFileName);
            var projectileChoiceAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillChoicesProjectileFiles,
                sourceCatalog.MonsterSkillChoicesProjectile);
            var lineAttackChoiceAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillChoicesLineAttackFiles,
                sourceCatalog.MonsterSkillChoicesLineAttack);
            var areaAttackChoiceAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillChoicesAreaAttackFiles,
                sourceCatalog.MonsterSkillChoicesAreaAttack);
            var singleAttackChoiceAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillChoicesSingleAttackFiles,
                sourceCatalog.MonsterSkillChoicesSingleAttack);
            var buffChoiceAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillChoicesBuffFiles,
                sourceCatalog.MonsterSkillChoicesBuff);
            var passiveChoiceAssets = ResolveSplitOrLegacyCsvAssets(
                sourceCatalog.MonsterSkillChoicesPassiveFiles,
                sourceCatalog.MonsterSkillChoicesPassive);
            var statusEffectTable = CsvTable.Load(sourceCatalog.StatusEffects, StatusEffectsFileName);
            var enemyTable = CsvTable.Load(sourceCatalog.StageOneEnemies, StageOneEnemiesFileName);
            var stageTwoEnemyTable = CsvTable.Load(sourceCatalog.StageTwoEnemies, StageTwoEnemiesFileName);
            var enemySkillTable = CsvTable.Load(sourceCatalog.EnemySkills, EnemySkillDataFileName);
            var enemySkillNodeTable = LoadOptionalCsvTable(sourceCatalog.EnemySkillNodes, EnemySkillNodesFileName);
            var enemySkillNodeParamTable = LoadOptionalCsvTable(sourceCatalog.EnemySkillNodeParams, EnemySkillNodeParamsFileName);
            var migratedEnemyTable = CsvTable.Load(sourceCatalog.Enemies, EnemiesFileName);
            var enemySkillLoadoutTable = CsvTable.Load(sourceCatalog.EnemySkillLoadouts, EnemySkillLoadoutsFileName);

            foreach (var record in catalogMonsterTable.Records)
            {
                var row = ParseCatalogEntry(record, "monster_id");
                AddUnique(model.CatalogMonsters, row.Id, row, record);
            }

            foreach (var record in catalogEnemyTable.Records)
            {
                var row = ParseCatalogEntry(record, "enemy_id");
                AddUnique(model.CatalogStageOneEnemies, row.Id, row, record);
            }

            foreach (var record in catalogStageTwoEnemyTable.Records)
            {
                var row = ParseCatalogEntry(record, "enemy_id");
                AddUnique(model.CatalogStageTwoEnemies, row.Id, row, record);
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

            for (var assetIndex = 0; assetIndex < skillNodeAssets.Length; assetIndex++)
            {
                var skillNodeTable = CsvTable.Load(
                    skillNodeAssets[assetIndex],
                    GetTextAssetCsvTableName(skillNodeAssets[assetIndex], MonsterSkillNodesFileName));
                foreach (var record in skillNodeTable.Records)
                {
                    var row = ParseSkillNodeRow(record);
                    AddUnique(model.SkillNodes, row.Id, row, record);
                }
            }

            for (var assetIndex = 0; assetIndex < skillNodeParamAssets.Length; assetIndex++)
            {
                var skillNodeParamTable = CsvTable.Load(
                    skillNodeParamAssets[assetIndex],
                    GetTextAssetCsvTableName(skillNodeParamAssets[assetIndex], MonsterSkillNodeParamsFileName));
                foreach (var record in skillNodeParamTable.Records)
                {
                    model.SkillNodeParams.Add(ParseSkillNodeParamRow(record));
                }
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

            foreach (var record in enemySkillTable.Records)
            {
                var row = ParseEnemySkillRow(record);
                AddUnique(model.EnemySkills, row.Id, row, record);
            }

            if (enemySkillNodeTable != null)
            {
                foreach (var record in enemySkillNodeTable.Records)
                {
                    model.EnemySkillNodes.Add(ParseEnemySkillNodeRow(record));
                }
            }

            if (enemySkillNodeParamTable != null)
            {
                foreach (var record in enemySkillNodeParamTable.Records)
                {
                    model.EnemySkillNodeParams.Add(ParseEnemySkillNodeParamRow(record));
                }
            }

            foreach (var record in enemyTable.Records)
            {
                var row = ParseEnemyRow(record);
                ApplyEnemySkillRow(row, model.EnemySkills, record);
                AddUnique(model.StageOneEnemies, row.Id, row, record);
            }

            foreach (var record in stageTwoEnemyTable.Records)
            {
                var row = ParseEnemyRow(record);
                ApplyEnemySkillRow(row, model.EnemySkills, record);
                AddUnique(model.StageTwoEnemies, row.Id, row, record);
            }

            foreach (var record in migratedEnemyTable.Records)
            {
                var row = ParseEnemyMigrationRow(record);
                AddUnique(model.MigratedEnemies, row.Id, row, record);
            }

            foreach (var record in enemySkillLoadoutTable.Records)
            {
                model.EnemySkillLoadouts.Add(ParseEnemySkillLoadoutRow(record));
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

        private static CsvTable LoadOptionalCsvTable(TextAsset asset, string tableName)
        {
            return asset != null ? CsvTable.Load(asset, tableName) : null;
        }

        private static bool HasAnyCsvAsset(TextAsset[] splitAssets, TextAsset legacyAsset)
        {
            if (legacyAsset != null)
            {
                return true;
            }

            if (splitAssets == null)
            {
                return false;
            }

            for (var i = 0; i < splitAssets.Length; i++)
            {
                if (splitAssets[i] != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static TextAsset[] ResolveSplitOrLegacyCsvAssets(TextAsset[] splitAssets, TextAsset legacyAsset)
        {
            if (splitAssets != null)
            {
                var assets = new List<TextAsset>(splitAssets.Length);
                for (var i = 0; i < splitAssets.Length; i++)
                {
                    if (splitAssets[i] != null)
                    {
                        assets.Add(splitAssets[i]);
                    }
                }

                if (assets.Count > 0)
                {
                    return assets.ToArray();
                }
            }

            return legacyAsset != null
                ? new[] { legacyAsset }
                : Array.Empty<TextAsset>();
        }

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
