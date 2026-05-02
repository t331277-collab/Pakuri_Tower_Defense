#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Pakuri.Combat;
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

        [MenuItem("Pakuri/Bootstrap CSV Source Data From Current Catalog")]
        private static void BootstrapSourceDataMenu()
        {
            var sourceRoot = GetImportedSourceRootFileSystemPath();
            BootstrapSourceFilesFromCurrentCatalog(sourceRoot);
            SyncImportedSourceCatalogsForEditor();
            Debug.Log($"Pakuri CSV source data bootstrapped under '{sourceRoot}'.");
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

        private static string GetImportedSourceRootFileSystemPath()
        {
            return Path.Combine(Application.dataPath, "CSVdata", "source");
        }

        private static void BootstrapSourceFilesFromCurrentCatalog(string sourceRoot)
        {
            Directory.CreateDirectory(sourceRoot);

            var catalog = AssetDatabase.LoadAssetAtPath<GameDataCatalog>(LegacyCatalogAssetPath);
            if (catalog == null)
            {
                throw new CsvFatalException(
                    $"Cannot bootstrap CSV source because '{LegacyCatalogAssetPath}' does not exist.");
            }

            if (catalog.Monsters == null || catalog.Monsters.Length == 0)
            {
                throw new CsvFatalException("Cannot bootstrap CSV source because catalog monsters are empty.");
            }

            if (catalog.StageOneEnemies == null || catalog.StageOneEnemies.Length == 0)
            {
                throw new CsvFatalException("Cannot bootstrap CSV source because catalog stage-one enemies are empty.");
            }

            WriteCatalogMonsterCsv(sourceRoot, catalog.Monsters);
            WriteCatalogEnemyCsv(sourceRoot, catalog.StageOneEnemies);
            WriteMonsterCsv(sourceRoot, catalog.Monsters);
            WriteMonsterRewardChoiceCsv(sourceRoot, catalog.Monsters);
            WriteMonsterSkillCsv(sourceRoot, catalog.Monsters);
            WriteMonsterSkillChoiceCsv(sourceRoot, catalog.Monsters);
            WriteStageOneEnemyCsv(sourceRoot, catalog.StageOneEnemies);
            AssetDatabase.Refresh();
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
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            if (asset == null)
            {
                throw new CsvFatalException(
                    $"Required imported CSV TextAsset is missing at '{assetPath}'.",
                    new List<string> { "Import the source CSV into Assets/CSVdata/source before validation." });
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

        private static void WriteCatalogMonsterCsv(string sourceRoot, MonsterDefinition[] monsters)
        {
            var rows = new List<string[]>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    $"catalog-monster-{monster.MonsterId}",
                    monster.MonsterId ?? string.Empty,
                    FormatInt(i)
                });
            }

            WriteTable(
                Path.Combine(sourceRoot, CatalogMonstersFileName),
                new[] { "id", "monster_id", "sort_order" },
                new[] { "id", "string", "int" },
                rows);
        }

        private static void WriteCatalogEnemyCsv(string sourceRoot, EnemyDefinition[] enemies)
        {
            var rows = new List<string[]>();
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    $"catalog-stage1-enemy-{enemy.EnemyId}",
                    enemy.EnemyId ?? string.Empty,
                    FormatInt(i)
                });
            }

            WriteTable(
                Path.Combine(sourceRoot, CatalogStageOneEnemiesFileName),
                new[] { "id", "enemy_id", "sort_order" },
                new[] { "id", "string", "int" },
                rows);
        }

        private static void WriteMonsterCsv(string sourceRoot, MonsterDefinition[] monsters)
        {
            var rows = new List<string[]>();
            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    monster.MonsterId ?? string.Empty,
                    monster.DisplayName ?? string.Empty,
                    monster.RoleSummary ?? string.Empty,
                    monster.ElementLabel ?? string.Empty,
                    monster.PrimaryAttribute.ToString(),
                    monster.ActiveSkillName ?? string.Empty,
                    monster.PassiveSkillName ?? string.Empty,
                    GetAssetPath(monster.UnitSprite),
                    GetAssetPath(monster.ProjectileSprite),
                    FormatColor(monster.UnitColor),
                    FormatColor(monster.ProjectileColor),
                    FormatFloat(monster.MaxHealth),
                    FormatFloat(monster.PowerStat),
                    FormatFloat(monster.BaseDamage),
                    FormatFloat(monster.PowerCoefficient),
                    FormatFloat(monster.ProjectileSpeed),
                    FormatFloat(monster.ProjectileLifetime),
                    FormatFloat(monster.ProjectileHitRadius),
                    FormatInt(monster.MagazineCapacity),
                    FormatFloat(monster.ReloadDuration),
                    FormatFloat(monster.ShotInterval),
                    FormatFloat(monster.StatusChance),
                    monster.StatusEffectLabel ?? string.Empty,
                    FormatFloat(monster.BaseStats != null ? monster.BaseStats.AttackPower : monster.PowerStat),
                    FormatFloat(monster.BaseStats != null ? monster.BaseStats.SpellPower : monster.PowerStat),
                    FormatFloat(monster.BaseStats != null ? monster.BaseStats.MoveSpeed : 1f),
                    FormatFloat(monster.BaseStats != null ? monster.BaseStats.CriticalChance : DamageCalculator.BaseCriticalChance),
                    FormatFloat(monster.BaseStats != null ? monster.BaseStats.CriticalDamage : DamageCalculator.BaseCriticalMultiplier),
                    FormatFloat(monster.BaseStats != null ? monster.BaseStats.CriticalResistance : 0f),
                    FormatFloat(monster.Defenses != null ? monster.Defenses.Physical : 0f),
                    FormatFloat(monster.Defenses != null ? monster.Defenses.Fire : 0f),
                    FormatFloat(monster.Defenses != null ? monster.Defenses.Lightning : 0f),
                    FormatFloat(monster.Defenses != null ? monster.Defenses.Ice : 0f),
                    FormatFloat(monster.Defenses != null ? monster.Defenses.Darkness : 0f),
                    FormatFloat(monster.Defenses != null ? monster.Defenses.Holy : 0f)
                });
            }

            WriteTable(
                Path.Combine(sourceRoot, MonstersFileName),
                new[]
                {
                    "id", "display_name", "role_summary", "element_label", "primary_attribute", "active_skill_name", "passive_skill_name",
                    "unit_sprite_path", "projectile_sprite_path", "unit_color", "projectile_color",
                    "max_health", "power_stat", "base_damage", "power_coefficient", "projectile_speed", "projectile_lifetime",
                    "projectile_hit_radius", "magazine_capacity", "reload_duration", "shot_interval", "status_chance", "status_effect_label",
                    "base_attack_power", "base_spell_power", "base_move_speed", "base_crit_chance", "base_crit_damage", "base_crit_resistance",
                    "def_physical", "def_fire", "def_lightning", "def_ice", "def_darkness", "def_holy"
                },
                new[]
                {
                    "id", "string", "string", "string", "enum:DamageAttribute", "string", "string",
                    "asset_path", "asset_path", "color", "color",
                    "float", "float", "float", "float", "float", "float",
                    "float", "int", "float", "float", "float", "string",
                    "float", "float", "float", "float", "float", "float",
                    "float", "float", "float", "float", "float", "float"
                },
                rows);
        }

        private static void WriteMonsterRewardChoiceCsv(string sourceRoot, MonsterDefinition[] monsters)
        {
            var rows = new List<string[]>();
            for (var monsterIndex = 0; monsterIndex < monsters.Length; monsterIndex++)
            {
                var monster = monsters[monsterIndex];
                if (monster == null || monster.InitialRewardChoices == null)
                {
                    continue;
                }

                for (var i = 0; i < monster.InitialRewardChoices.Length; i++)
                {
                    var reward = monster.InitialRewardChoices[i];
                    if (reward == null)
                    {
                        continue;
                    }

                    rows.Add(new[]
                    {
                        string.IsNullOrWhiteSpace(reward.RewardId) ? $"{monster.MonsterId}-reward-{i + 1}" : reward.RewardId,
                        monster.MonsterId ?? string.Empty,
                        FormatInt(i),
                        reward.Title ?? string.Empty,
                        reward.Description ?? string.Empty,
                        FormatFloat(reward.DamageMultiplier),
                        FormatInt(reward.MagazineBonus),
                        FormatFloat(reward.ShotIntervalMultiplier),
                        FormatFloat(reward.ReloadDurationMultiplier),
                        FormatFloat(reward.MaxHealthBonus),
                        FormatFloat(reward.StatusChanceBonus)
                    });
                }
            }

            WriteTable(
                Path.Combine(sourceRoot, MonsterRewardChoicesFileName),
                new[]
                {
                    "choice_id", "monster_id", "sort_order", "title", "description", "damage_multiplier", "magazine_bonus",
                    "shot_interval_multiplier", "reload_duration_multiplier", "max_health_bonus", "status_chance_bonus"
                },
                new[]
                {
                    "id", "string", "int", "string", "string", "float", "int",
                    "float", "float", "float", "float"
                },
                rows);
        }

        private static void WriteMonsterSkillCsv(string sourceRoot, MonsterDefinition[] monsters)
        {
            var rows = new List<string[]>();
            for (var monsterIndex = 0; monsterIndex < monsters.Length; monsterIndex++)
            {
                var monster = monsters[monsterIndex];
                if (monster == null)
                {
                    continue;
                }

                AppendMonsterSkillRows(rows, monster.MonsterId, monster.ActiveSkills, PakuriCsvSkillKind.Active);
                AppendMonsterSkillRows(rows, monster.MonsterId, monster.PassiveSkills, PakuriCsvSkillKind.Passive);
            }

            WriteTable(
                Path.Combine(sourceRoot, MonsterSkillsFileName),
                new[]
                {
                    "skill_id", "monster_id", "skill_kind", "slot", "display_name", "runtime_kind", "implementation_state",
                    "is_default_learned", "is_available_without_active_requirement", "required_active_slot",
                    "skill_icon_path", "skill_effect_prefab_path", "description_text", "summary", "attribute",
                    "base_damage", "attack_power_coefficient", "spell_power_coefficient", "range", "radius",
                    "cooldown_seconds", "magazine_capacity", "reload_seconds", "shot_interval_seconds", "critical_allowed", "status_effect_id"
                },
                new[]
                {
                    "id", "string", "enum:PakuriCsvSkillKind", "enum:SkillSlot", "string", "enum:SkillRuntimeKind", "enum:SkillImplementationState",
                    "bool", "bool", "enum:SkillSlot",
                    "asset_path", "asset_path", "string", "string", "enum:DamageAttribute",
                    "float", "float", "float", "float", "float",
                    "float", "int", "float", "float", "bool", "string"
                },
                rows);
        }

        private static void AppendMonsterSkillRows(
            List<string[]> rows,
            string monsterId,
            SkillDefinition[] activeSkills,
            PakuriCsvSkillKind skillKind)
        {
            if (activeSkills == null)
            {
                return;
            }

            for (var i = 0; i < activeSkills.Length; i++)
            {
                var skill = activeSkills[i];
                if (skill == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    skill.SkillId ?? string.Empty,
                    monsterId ?? string.Empty,
                    skillKind.ToString(),
                    skill.Slot.ToString(),
                    skill.DisplayName ?? string.Empty,
                    skill.RuntimeKind.ToString(),
                    skill.ImplementationState.ToString(),
                    FormatBool(skill.IsDefaultLearned),
                    FormatBool(false),
                    SkillSlot.A.ToString(),
                    GetAssetPath(skill.SkillIcon),
                    GetAssetPath(skill.SkillEffectPrefab),
                    skill.DescriptionText ?? string.Empty,
                    skill.Summary ?? string.Empty,
                    skill.Attribute.ToString(),
                    FormatFloat(skill.BaseDamage),
                    FormatFloat(skill.AttackPowerCoefficient),
                    FormatFloat(skill.SpellPowerCoefficient),
                    FormatFloat(skill.Range),
                    FormatFloat(skill.Radius),
                    FormatFloat(skill.CooldownSeconds),
                    FormatInt(skill.MagazineCapacity),
                    FormatFloat(skill.ReloadSeconds),
                    FormatFloat(skill.ShotIntervalSeconds),
                    FormatBool(skill.CriticalAllowed),
                    skill.StatusEffectId ?? string.Empty
                });
            }
        }

        private static void AppendMonsterSkillRows(
            List<string[]> rows,
            string monsterId,
            PassiveDefinition[] passiveSkills,
            PakuriCsvSkillKind skillKind)
        {
            if (passiveSkills == null)
            {
                return;
            }

            for (var i = 0; i < passiveSkills.Length; i++)
            {
                var skill = passiveSkills[i];
                if (skill == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    skill.PassiveId ?? string.Empty,
                    monsterId ?? string.Empty,
                    skillKind.ToString(),
                    skill.Slot.ToString(),
                    skill.DisplayName ?? string.Empty,
                    SkillRuntimeKind.Passive.ToString(),
                    skill.ImplementationState.ToString(),
                    FormatBool(false),
                    FormatBool(skill.IsAvailableWithoutActiveRequirement),
                    skill.RequiredActiveSlot.ToString(),
                    GetAssetPath(skill.SkillIcon),
                    GetAssetPath(skill.SkillEffectPrefab),
                    skill.DescriptionText ?? string.Empty,
                    skill.Summary ?? string.Empty,
                    DamageAttribute.Physical.ToString(),
                    FormatFloat(0f),
                    FormatFloat(0f),
                    FormatFloat(0f),
                    FormatFloat(0f),
                    FormatFloat(0f),
                    FormatFloat(0f),
                    FormatInt(0),
                    FormatFloat(0f),
                    FormatFloat(0f),
                    FormatBool(false),
                    string.Empty
                });
            }
        }

        private static void WriteMonsterSkillChoiceCsv(string sourceRoot, MonsterDefinition[] monsters)
        {
            var rows = new List<string[]>();
            for (var monsterIndex = 0; monsterIndex < monsters.Length; monsterIndex++)
            {
                var monster = monsters[monsterIndex];
                if (monster == null)
                {
                    continue;
                }

                AppendSkillChoiceRows(rows, monster.MonsterId, monster.ActiveSkills);
                AppendSkillChoiceRows(rows, monster.MonsterId, monster.PassiveSkills);
            }

            WriteTable(
                Path.Combine(sourceRoot, MonsterSkillChoicesFileName),
                new[]
                {
                    "choice_id", "monster_id", "skill_id", "choice_group", "sort_order", "title", "description_text", "skill_icon_path", "skill_effect_prefab_path"
                },
                new[]
                {
                    "id", "string", "string", "enum:PakuriCsvChoiceGroup", "int", "string", "string", "asset_path", "asset_path"
                },
                rows);
        }

        private static void AppendSkillChoiceRows(List<string[]> rows, string monsterId, SkillDefinition[] skills)
        {
            if (skills == null)
            {
                return;
            }

            for (var skillIndex = 0; skillIndex < skills.Length; skillIndex++)
            {
                var skill = skills[skillIndex];
                if (skill == null)
                {
                    continue;
                }

                AppendChoiceRows(rows, monsterId, skill.SkillId, PakuriCsvChoiceGroup.ActiveEnhancement, skill.EnhancementChoices);
                AppendChoiceRows(rows, monsterId, skill.SkillId, PakuriCsvChoiceGroup.ActiveMaster, skill.MasterSkillChoices);
            }
        }

        private static void AppendSkillChoiceRows(List<string[]> rows, string monsterId, PassiveDefinition[] skills)
        {
            if (skills == null)
            {
                return;
            }

            for (var skillIndex = 0; skillIndex < skills.Length; skillIndex++)
            {
                var skill = skills[skillIndex];
                if (skill == null)
                {
                    continue;
                }

                AppendChoiceRows(rows, monsterId, skill.PassiveId, PakuriCsvChoiceGroup.PassiveEnhancement, skill.EnhancementChoices);
            }
        }

        private static void AppendChoiceRows(
            List<string[]> rows,
            string monsterId,
            string skillId,
            PakuriCsvChoiceGroup choiceGroup,
            SkillChoiceDefinition[] choices)
        {
            if (choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    string.IsNullOrWhiteSpace(choice.ChoiceId) ? $"{skillId}-{choiceGroup}-{i + 1}" : choice.ChoiceId,
                    monsterId ?? string.Empty,
                    skillId ?? string.Empty,
                    choiceGroup.ToString(),
                    FormatInt(i),
                    choice.Title ?? string.Empty,
                    choice.DescriptionText ?? string.Empty,
                    GetAssetPath(choice.SkillIcon),
                    GetAssetPath(choice.SkillEffectPrefab)
                });
            }
        }

        private static void WriteStageOneEnemyCsv(string sourceRoot, EnemyDefinition[] enemies)
        {
            var rows = new List<string[]>();
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null)
                {
                    continue;
                }

                rows.Add(new[]
                {
                    enemy.EnemyId ?? string.Empty,
                    enemy.DisplayName ?? string.Empty,
                    enemy.EncounterRole.ToString(),
                    enemy.AttackType.ToString(),
                    enemy.Attribute.ToString(),
                    GetAssetPath(enemy.UnitSprite),
                    GetAssetPath(enemy.ProjectileSprite),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.MaxHealth : 0f),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.AttackPower : 0f),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.SpellPower : 0f),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.MoveSpeed : 0f),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.CriticalChance : 0f),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.CriticalDamage : 0f),
                    FormatFloat(enemy.Stats != null ? enemy.Stats.CriticalResistance : 0f),
                    FormatFloat(enemy.Defenses != null ? enemy.Defenses.Physical : 0f),
                    FormatFloat(enemy.Defenses != null ? enemy.Defenses.Fire : 0f),
                    FormatFloat(enemy.Defenses != null ? enemy.Defenses.Lightning : 0f),
                    FormatFloat(enemy.Defenses != null ? enemy.Defenses.Ice : 0f),
                    FormatFloat(enemy.Defenses != null ? enemy.Defenses.Darkness : 0f),
                    FormatFloat(enemy.Defenses != null ? enemy.Defenses.Holy : 0f),
                    enemy.StageOneSkill.ToString(),
                    enemy.ActiveSkillName ?? string.Empty,
                    FormatFloat(enemy.ActiveSkillCoefficient),
                    FormatFloat(enemy.ActiveSkillCooldown),
                    FormatFloat(enemy.ActiveSkillDuration),
                    FormatFloat(enemy.ActiveSkillRadius),
                    FormatFloat(enemy.ActiveSkillFlatValue),
                    enemy.PassiveSkillName ?? string.Empty,
                    enemy.PassiveSummary ?? string.Empty
                });
            }

            WriteTable(
                Path.Combine(sourceRoot, StageOneEnemiesFileName),
                new[]
                {
                    "enemy_id", "display_name", "encounter_role", "attack_type", "attribute", "unit_sprite_path", "projectile_sprite_path",
                    "max_health", "attack_power", "spell_power", "move_speed", "crit_chance", "crit_damage", "crit_resistance",
                    "def_physical", "def_fire", "def_lightning", "def_ice", "def_darkness", "def_holy",
                    "stage_one_skill", "active_skill_name", "active_skill_coefficient", "active_skill_cooldown",
                    "active_skill_duration", "active_skill_radius", "active_skill_flat_value", "passive_skill_name", "passive_summary"
                },
                new[]
                {
                    "id", "string", "enum:EnemyEncounterRole", "enum:EnemyAttackType", "enum:DamageAttribute", "asset_path", "asset_path",
                    "float", "float", "float", "float", "float", "float", "float",
                    "float", "float", "float", "float", "float", "float",
                    "enum:StageOneEnemySkillKind", "string", "float", "float",
                    "float", "float", "float", "string", "string"
                },
                rows);
        }

        private static void WriteTable(string path, string[] headers, string[] types, List<string[]> rows)
        {
            var builder = new StringBuilder();
            builder.AppendLine(JoinCsv(headers));
            builder.AppendLine(JoinCsv(types));

            for (var i = 0; i < rows.Count; i++)
            {
                builder.AppendLine(JoinCsv(rows[i]));
            }

            File.WriteAllText(path, builder.ToString(), new UTF8Encoding(false));
        }

        private static string JoinCsv(string[] values)
        {
            var escaped = new string[values.Length];
            for (var i = 0; i < values.Length; i++)
            {
                escaped[i] = EscapeCsv(values[i] ?? string.Empty);
            }

            return string.Join(",", escaped);
        }

        private static string EscapeCsv(string value)
        {
            var normalized = (value ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n').Replace("\n", "\\n");
            if (normalized.IndexOfAny(new[] { ',', '"', '\n' }) >= 0 || normalized.StartsWith(" ", StringComparison.Ordinal) || normalized.EndsWith(" ", StringComparison.Ordinal))
            {
                return "\"" + normalized.Replace("\"", "\"\"") + "\"";
            }

            return normalized;
        }

        private static string FormatFloat(float value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private static string FormatInt(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatBool(bool value)
        {
            return value ? "true" : "false";
        }

        private static string FormatColor(Color value)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{0}|{1}|{2}|{3}",
                value.r,
                value.g,
                value.b,
                value.a);
        }

        private static string GetAssetPath(UnityEngine.Object asset)
        {
            return asset == null ? string.Empty : AssetDatabase.GetAssetPath(asset);
        }
    }
}
#endif
