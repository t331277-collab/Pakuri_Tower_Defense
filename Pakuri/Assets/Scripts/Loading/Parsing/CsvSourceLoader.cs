/*
 * 역할: CSV 원본 획득.
 * 책임: 필수 CSV TextAsset을 로드하고 검증에 사용할 전체 원본 모델을 생성한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using static Pakuri.Data.GameDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    /// CsvSourceLoader가 소유하는 데이터와 동작을 캡슐화한다.
    internal static class CsvSourceLoader
    {

        /// 전달된 sourceCatalog 값을 사용해 SourceModel를 불러온다.
        internal static SourceModel LoadSourceModel(CsvRuntimeCatalog sourceCatalog)
        {
            var model = new SourceModel();

            var catalogMonsterTable = CsvTable.Load(sourceCatalog.CatalogMonsters, CatalogMonstersFileName);
            var monsterTable = CsvTable.Load(sourceCatalog.Monsters, MonstersFileName);
            var rewardChoiceTable = CsvTable.Load(sourceCatalog.MonsterRewardChoices, MonsterRewardChoicesFileName);
            var skillNodeDefinitionTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitions,
                MonsterSkillNodeDefinitionsFileName);
            var skillNodeDefinitionParamTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitionParams,
                MonsterSkillNodeDefinitionParamsFileName);
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
                sourceCatalog.MonsterSkillsProjectileFiles,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillRows(
                model,
                sourceCatalog.MonsterSkillsLineAttackFiles,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.LineAttack);
            LoadSkillRows(
                model,
                sourceCatalog.MonsterSkillsAreaAttackFiles,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillRows(
                model,
                sourceCatalog.MonsterSkillsSingleAttackFiles,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.SingleAttack);
            LoadSkillRows(
                model,
                sourceCatalog.MonsterSkillsBuffFiles,
                PakuriCsvSkillKind.Active,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillRows(
                model,
                sourceCatalog.MonsterSkillsPassiveFiles,
                PakuriCsvSkillKind.Passive,
                SkillRuntimeKind.Passive);

            foreach (var record in statusEffectTable.Records)
            {
                var row = ParseStatusEffectRow(record);
                AddUnique(model.StatusEffects, row.Id, row, record);
            }

            foreach (var record in skillNodeDefinitionTable.Records)
            {
                var row = ParseSkillNodeTypeRow(record);
                AddUnique(model.SkillNodeTypes, row.Id, row, record);
            }

            foreach (var record in skillNodeDefinitionParamTable.Records)
            {
                model.SkillNodeTypeParams.Add(ParseSkillNodeTypeParamRow(record));
            }

            for (var assetIndex = 0; assetIndex < sourceCatalog.MonsterSkillTriggerFiles.Length; assetIndex++)
            {
                var skillTriggerTable = CsvTable.Load(
                    sourceCatalog.MonsterSkillTriggerFiles[assetIndex],
                    GetTextAssetCsvTableName(sourceCatalog.MonsterSkillTriggerFiles[assetIndex]));
                foreach (var record in skillTriggerTable.Records)
                {
                    var row = ParseSkillTriggerRow(record);
                    AddUnique(model.SkillTriggers, row.Id, row, record);
                }
            }

            LoadSkillChoiceRows(
                model,
                sourceCatalog.MonsterSkillChoicesProjectileFiles,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillChoiceRows(
                model,
                sourceCatalog.MonsterSkillChoicesLineAttackFiles,
                SkillRuntimeKind.LineAttack);
            LoadSkillChoiceRows(
                model,
                sourceCatalog.MonsterSkillChoicesAreaAttackFiles,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillChoiceRows(
                model,
                sourceCatalog.MonsterSkillChoicesSingleAttackFiles,
                SkillRuntimeKind.SingleAttack);
            LoadSkillChoiceRows(
                model,
                sourceCatalog.MonsterSkillChoicesBuffFiles,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillChoiceRows(
                model,
                sourceCatalog.MonsterSkillChoicesPassiveFiles,
                SkillRuntimeKind.Passive);

            for (var assetIndex = 0; assetIndex < sourceCatalog.MonsterSkillGraphNodeFiles.Length; assetIndex++)
            {
                var graphNodeTable = CsvTable.Load(
                    sourceCatalog.MonsterSkillGraphNodeFiles[assetIndex],
                    GetTextAssetCsvTableName(sourceCatalog.MonsterSkillGraphNodeFiles[assetIndex]));
                foreach (var record in graphNodeTable.Records)
                {
                    model.SkillGraphNodes.Add(ParseSkillGraphNodeRow(record));
                }
            }

            MaterializeSkillGraphRows(model);

            foreach (var record in enemyTable.Records)
            {
                var row = ParseEnemyRow(record);
                AddUnique(model.Enemies, row.Id, row, record);
            }

            for (var assetIndex = 0; assetIndex < sourceCatalog.EnemySkillBaseFiles.Length; assetIndex++)
            {
                var asset = sourceCatalog.EnemySkillBaseFiles[assetIndex];
                var tableName = GetTextAssetCsvTableName(asset);
                var table = CsvTable.Load(asset, tableName);
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyBaseSkillRow(record, tableName);
                    AddUnique(model.EnemyBaseSkills, row.Skill.Id, row, record);
                }
            }

            for (var assetIndex = 0; assetIndex < sourceCatalog.EnemySkillTriggerFiles.Length; assetIndex++)
            {
                var asset = sourceCatalog.EnemySkillTriggerFiles[assetIndex];
                var table = CsvTable.Load(asset, GetTextAssetCsvTableName(asset));
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyTriggerRow(record);
                    AddUnique(model.EnemyTriggers, row.Id, row, record);
                }
            }

            return model;
        }

        /// 전달된 런타임 입력값을 사용해 SkillRows를 불러온다.
        internal static void LoadSkillRows(
            SourceModel model,
            TextAsset[] skillAssets,
            PakuriCsvSkillKind skillKind,
            params SkillRuntimeKind[] allowedRuntimeKinds)
        {
            for (var assetIndex = 0; assetIndex < skillAssets.Length; assetIndex++)
            {
                var skillAsset = skillAssets[assetIndex];
                LoadSkillRows(
                    model,
                    skillAsset,
                    GetTextAssetCsvTableName(skillAsset),
                    skillKind,
                    allowedRuntimeKinds);
            }
        }

        /// 전달된 런타임 입력값을 사용해 SkillRows를 불러온다.
        internal static void LoadSkillRows(
            SourceModel model,
            TextAsset skillAsset,
            string tableName,
            PakuriCsvSkillKind skillKind,
            params SkillRuntimeKind[] allowedRuntimeKinds)
        {
            var skillTable = CsvTable.Load(skillAsset, tableName);
            foreach (var record in skillTable.Records)
            {
                var row = ParseSkillRow(record, skillKind);
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

        /// 전달된 런타임 입력값을 사용해 SkillChoiceRows를 불러온다.
        internal static void LoadSkillChoiceRows(
            SourceModel model,
            TextAsset[] choiceAssets,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds)
        {
            for (var assetIndex = 0; assetIndex < choiceAssets.Length; assetIndex++)
            {
                var choiceAsset = choiceAssets[assetIndex];
                LoadSkillChoiceRows(
                    model,
                    choiceAsset,
                    GetTextAssetCsvTableName(choiceAsset),
                    allowedOwnerRuntimeKinds);
            }
        }

        /// 전달된 런타임 입력값을 사용해 SkillChoiceRows를 불러온다.
        internal static void LoadSkillChoiceRows(
            SourceModel model,
            TextAsset choiceAsset,
            string tableName,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds)
        {
            var choiceTable = CsvTable.Load(choiceAsset, tableName);
            foreach (var record in choiceTable.Records)
            {
                var row = ParseSkillChoiceRow(record);
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

        /// 전달된 런타임 입력값을 사용해 AllowedSkillRuntimeKind 조건 충족 여부를 반환한다.
        internal static bool IsAllowedSkillRuntimeKind(
            SkillRuntimeKind runtimeKind,
            SkillRuntimeKind[] allowedRuntimeKinds)
        {
            return Array.IndexOf(allowedRuntimeKinds, runtimeKind) >= 0;
        }

        /// 전달된 asset 값을 사용해 TextAssetCsvTableName를 반환한다.
        internal static string GetTextAssetCsvTableName(TextAsset asset)
        {
            if (asset == null)
            {
                throw new CsvFatalException("CSV runtime catalog contains a null TextAsset reference.");
            }

            if (string.IsNullOrWhiteSpace(asset.name))
            {
                throw new CsvFatalException("CSV runtime catalog contains a TextAsset without a name.");
            }

            if (asset.name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                return asset.name;
            }

            return asset.name + ".csv";
        }

        /// 전달된 런타임 입력값을 사용해 Unique를 소유한 런타임 상태에 추가한다.
        internal static void AddUnique<T>(Dictionary<string, T> dictionary, string id, T value, CsvRecord record)
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
