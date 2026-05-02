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

        private static CatalogEntryRow ParseCatalogEntry(CsvRecord record, string refColumnName)
        {
            return new CatalogEntryRow
            {
                Id = record.ReadRequiredString("id"),
                RefId = record.ReadRequiredString(refColumnName),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        private static MonsterRow ParseMonsterRow(CsvRecord record)
        {
            return new MonsterRow
            {
                Id = record.ReadRequiredString("id"),
                DisplayName = record.ReadRequiredString("display_name"),
                RoleSummary = record.ReadString("role_summary"),
                ElementLabel = record.ReadString("element_label"),
                PrimaryAttribute = record.ReadEnum<DamageAttribute>("primary_attribute"),
                ActiveSkillName = record.ReadString("active_skill_name"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                UnitSpritePath = record.ReadString("unit_sprite_path"),
                ProjectileSpritePath = record.ReadString("projectile_sprite_path"),
                UnitColor = record.ReadColor("unit_color"),
                ProjectileColor = record.ReadColor("projectile_color"),
                MaxHealth = record.ReadFloat("max_health"),
                PowerStat = record.ReadFloat("power_stat"),
                BaseDamage = record.ReadFloat("base_damage"),
                PowerCoefficient = record.ReadFloat("power_coefficient"),
                ProjectileSpeed = record.ReadFloat("projectile_speed"),
                ProjectileLifetime = record.ReadFloat("projectile_lifetime"),
                ProjectileHitRadius = record.ReadFloat("projectile_hit_radius"),
                MagazineCapacity = record.ReadInt("magazine_capacity"),
                ReloadDuration = record.ReadFloat("reload_duration"),
                ShotInterval = record.ReadFloat("shot_interval"),
                StatusChance = record.ReadFloat("status_chance"),
                StatusEffectLabel = record.ReadString("status_effect_label"),
                BaseAttackPower = record.ReadFloat("base_attack_power"),
                BaseSpellPower = record.ReadFloat("base_spell_power"),
                BaseMoveSpeed = record.ReadFloat("base_move_speed"),
                BaseCriticalChance = record.ReadFloat("base_crit_chance"),
                BaseCriticalDamage = record.ReadFloat("base_crit_damage"),
                BaseCriticalResistance = record.ReadFloat("base_crit_resistance"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy")
            };
        }

        private static RewardChoiceRow ParseRewardChoiceRow(CsvRecord record)
        {
            return new RewardChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SortOrder = record.ReadInt("sort_order"),
                Title = record.ReadRequiredString("title"),
                Description = record.ReadString("description"),
                DamageMultiplier = record.ReadFloat("damage_multiplier"),
                MagazineBonus = record.ReadInt("magazine_bonus"),
                ShotIntervalMultiplier = record.ReadFloat("shot_interval_multiplier"),
                ReloadDurationMultiplier = record.ReadFloat("reload_duration_multiplier"),
                MaxHealthBonus = record.ReadFloat("max_health_bonus"),
                StatusChanceBonus = record.ReadFloat("status_chance_bonus")
            };
        }

        private static SkillRow ParseSkillRow(CsvRecord record)
        {
            return new SkillRow
            {
                Id = record.ReadRequiredString("skill_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SkillKind = record.ReadEnum<PakuriCsvSkillKind>("skill_kind"),
                Slot = record.ReadEnum<SkillSlot>("slot"),
                DisplayName = record.ReadRequiredString("display_name"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                ImplementationState = record.ReadEnum<SkillImplementationState>("implementation_state"),
                IsDefaultLearned = record.ReadBool("is_default_learned"),
                IsAvailableWithoutActiveRequirement = record.ReadBool("is_available_without_active_requirement"),
                RequiredActiveSlot = record.ReadEnum<SkillSlot>("required_active_slot"),
                SkillIconPath = record.ReadString("skill_icon_path"),
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path"),
                DescriptionText = record.ReadString("description_text"),
                Summary = record.ReadString("summary"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                BaseDamage = record.ReadFloat("base_damage"),
                AttackPowerCoefficient = record.ReadFloat("attack_power_coefficient"),
                SpellPowerCoefficient = record.ReadFloat("spell_power_coefficient"),
                Range = record.ReadFloat("range"),
                Radius = record.ReadFloat("radius"),
                CooldownSeconds = record.ReadFloat("cooldown_seconds"),
                MagazineCapacity = record.ReadInt("magazine_capacity"),
                ReloadSeconds = record.ReadFloat("reload_seconds"),
                ShotIntervalSeconds = record.ReadFloat("shot_interval_seconds"),
                CriticalAllowed = record.ReadBool("critical_allowed"),
                StatusEffectId = record.ReadString("status_effect_id")
            };
        }

        private static SkillChoiceRow ParseSkillChoiceRow(CsvRecord record)
        {
            return new SkillChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                SkillId = record.ReadRequiredString("skill_id"),
                ChoiceGroup = record.ReadEnum<PakuriCsvChoiceGroup>("choice_group"),
                SortOrder = record.ReadInt("sort_order"),
                Title = record.ReadRequiredString("title"),
                DescriptionText = record.ReadString("description_text"),
                SkillIconPath = record.ReadString("skill_icon_path"),
                SkillEffectPrefabPath = record.ReadString("skill_effect_prefab_path")
            };
        }

        private static EnemyRow ParseEnemyRow(CsvRecord record)
        {
            return new EnemyRow
            {
                Id = record.ReadRequiredString("enemy_id"),
                DisplayName = record.ReadRequiredString("display_name"),
                EncounterRole = record.ReadEnum<EnemyEncounterRole>("encounter_role"),
                AttackType = record.ReadEnum<EnemyAttackType>("attack_type"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
                UnitSpritePath = record.ReadString("unit_sprite_path"),
                ProjectileSpritePath = record.ReadString("projectile_sprite_path"),
                MaxHealth = record.ReadFloat("max_health"),
                AttackPower = record.ReadFloat("attack_power"),
                SpellPower = record.ReadFloat("spell_power"),
                MoveSpeed = record.ReadFloat("move_speed"),
                CriticalChance = record.ReadFloat("crit_chance"),
                CriticalDamage = record.ReadFloat("crit_damage"),
                CriticalResistance = record.ReadFloat("crit_resistance"),
                PhysicalDefense = record.ReadFloat("def_physical"),
                FireDefense = record.ReadFloat("def_fire"),
                LightningDefense = record.ReadFloat("def_lightning"),
                IceDefense = record.ReadFloat("def_ice"),
                DarknessDefense = record.ReadFloat("def_darkness"),
                HolyDefense = record.ReadFloat("def_holy"),
                StageOneSkill = record.ReadEnum<StageOneEnemySkillKind>("stage_one_skill"),
                ActiveSkillName = record.ReadString("active_skill_name"),
                ActiveSkillCoefficient = record.ReadFloat("active_skill_coefficient"),
                ActiveSkillCooldown = record.ReadFloat("active_skill_cooldown"),
                ActiveSkillDuration = record.ReadFloat("active_skill_duration"),
                ActiveSkillRadius = record.ReadFloat("active_skill_radius"),
                ActiveSkillFlatValue = record.ReadFloat("active_skill_flat_value"),
                PassiveSkillName = record.ReadString("passive_skill_name"),
                PassiveSummary = record.ReadString("passive_summary")
            };
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
