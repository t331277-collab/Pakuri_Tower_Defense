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
            if (sourceCatalog.Monsters == null)
            {
                missingAssets.Add(MonstersFileName);
            }
            if (sourceCatalog.MonsterRewardChoices == null)
            {
                missingAssets.Add(MonsterRewardChoicesFileName);
            }
            if (sourceCatalog.MonsterSkills == null)
            {
                missingAssets.Add(MonsterSkillsFileName);
            }
            if (sourceCatalog.MonsterSkillChoices == null)
            {
                missingAssets.Add(MonsterSkillChoicesFileName);
            }
            if (sourceCatalog.StageOneEnemies == null)
            {
                missingAssets.Add(StageOneEnemiesFileName);
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
            var monsterTable = CsvTable.Load(sourceCatalog.Monsters, MonstersFileName);
            var rewardChoiceTable = CsvTable.Load(sourceCatalog.MonsterRewardChoices, MonsterRewardChoicesFileName);
            var skillTable = CsvTable.Load(sourceCatalog.MonsterSkills, MonsterSkillsFileName);
            var skillChoiceTable = CsvTable.Load(sourceCatalog.MonsterSkillChoices, MonsterSkillChoicesFileName);
            var enemyTable = CsvTable.Load(sourceCatalog.StageOneEnemies, StageOneEnemiesFileName);

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

            foreach (var record in skillTable.Records)
            {
                var row = ParseSkillRow(record);
                AddUnique(model.Skills, row.Id, row, record);
            }

            foreach (var record in skillChoiceTable.Records)
            {
                var row = ParseSkillChoiceRow(record);
                AddUnique(model.SkillChoices, row.Id, row, record);
            }

            foreach (var record in enemyTable.Records)
            {
                var row = ParseEnemyRow(record);
                AddUnique(model.StageOneEnemies, row.Id, row, record);
            }

            return model;
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
