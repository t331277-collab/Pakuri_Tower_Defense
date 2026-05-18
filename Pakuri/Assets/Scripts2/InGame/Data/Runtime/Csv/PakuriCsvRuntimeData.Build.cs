using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using AttributeDefenseSet = Pakuri.Combat.DamageCalculator.AttributeDefenseSet;
using CombatStatBlock = Pakuri.Combat.DamageCalculator.CombatStatBlock;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private static GameDataCatalog BuildRuntimeCatalog(SourceModel model)
        {
            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();

            var monsters = new List<MonsterDefinition>();
            foreach (var entry in SortCatalogEntries(model.CatalogMonsters))
            {
                var sourceMonster = model.Monsters[entry.RefId];
                var monster = ScriptableObject.CreateInstance<MonsterDefinition>();
                monster.MonsterId = sourceMonster.Id;
                monster.DisplayName = sourceMonster.DisplayName;
                monster.RoleSummary = sourceMonster.RoleSummary;
                monster.ElementLabel = sourceMonster.ElementLabel;
                monster.PrimaryAttribute = sourceMonster.PrimaryAttribute;
                monster.ActiveSkillName = sourceMonster.ActiveSkillName;
                monster.PassiveSkillName = sourceMonster.PassiveSkillName;
                monster.MaxHealth = sourceMonster.MaxHealth;
                monster.PowerStat = sourceMonster.PowerStat;
                monster.BaseDamage = sourceMonster.BaseDamage;
                monster.PowerCoefficient = sourceMonster.PowerCoefficient;
                monster.BaseStats = new CombatStatBlock
                {
                    MaxHealth = sourceMonster.MaxHealth,
                    AttackPower = sourceMonster.BaseAttackPower,
                    SpellPower = sourceMonster.BaseSpellPower,
                    MoveSpeed = sourceMonster.BaseMoveSpeed,
                    CriticalChance = sourceMonster.BaseCriticalChance,
                    CriticalDamage = sourceMonster.BaseCriticalDamage,
                    CriticalResistance = sourceMonster.BaseCriticalResistance
                };
                monster.Defenses = new AttributeDefenseSet
                {
                    Physical = sourceMonster.PhysicalDefense,
                    Fire = sourceMonster.FireDefense,
                    Lightning = sourceMonster.LightningDefense,
                    Ice = sourceMonster.IceDefense,
                    Darkness = sourceMonster.DarknessDefense,
                    Holy = sourceMonster.HolyDefense
                };
                monster.InitialRewardChoices = BuildRewardChoices(model, sourceMonster.Id);
                monster.ActiveSkills = BuildActiveSkills(model, sourceMonster.Id);
                monster.PassiveSkills = BuildPassiveSkills(model, sourceMonster.Id);
                monsters.Add(monster);
            }

            var enemies = new List<EnemyDefinition>();
            foreach (var entry in SortCatalogEntries(model.CatalogStageOneEnemies))
            {
                var sourceEnemy = model.StageOneEnemies[entry.RefId];
                var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                enemy.EnemyId = sourceEnemy.Id;
                enemy.DisplayName = sourceEnemy.DisplayName;
                enemy.EncounterRole = sourceEnemy.EncounterRole;
                enemy.AttackType = sourceEnemy.AttackType;
                enemy.Attribute = sourceEnemy.Attribute;
                enemy.UnitSprite = LoadSprite(sourceEnemy.UnitSpritePath);
                enemy.ProjectileSprite = LoadSprite(sourceEnemy.ProjectileSpritePath);
                enemy.Stats = new CombatStatBlock
                {
                    MaxHealth = sourceEnemy.MaxHealth,
                    AttackPower = sourceEnemy.AttackPower,
                    SpellPower = sourceEnemy.SpellPower,
                    MoveSpeed = sourceEnemy.MoveSpeed,
                    CriticalChance = sourceEnemy.CriticalChance,
                    CriticalDamage = sourceEnemy.CriticalDamage,
                    CriticalResistance = sourceEnemy.CriticalResistance
                };
                enemy.Defenses = new AttributeDefenseSet
                {
                    Physical = sourceEnemy.PhysicalDefense,
                    Fire = sourceEnemy.FireDefense,
                    Lightning = sourceEnemy.LightningDefense,
                    Ice = sourceEnemy.IceDefense,
                    Darkness = sourceEnemy.DarknessDefense,
                    Holy = sourceEnemy.HolyDefense
                };
                enemy.HasBasicSkill = sourceEnemy.HasBasicSkill;
                enemy.BasicSkill = sourceEnemy.BasicSkill;
                enemy.BasicSkillName = sourceEnemy.BasicSkillName;
                enemy.BasicSkillCoefficient = sourceEnemy.BasicSkillCoefficient;
                enemy.BasicSkillCooldown = sourceEnemy.BasicSkillCooldown;
                enemy.BasicSkillDuration = sourceEnemy.BasicSkillDuration;
                enemy.BasicSkillRadius = sourceEnemy.BasicSkillRadius;
                enemy.BasicSkillFlatValue = sourceEnemy.BasicSkillFlatValue;
                enemy.BasicSkillProjectileSpeed = sourceEnemy.BasicSkillProjectileSpeed;
                enemy.BasicSkillProjectileLifetime = sourceEnemy.BasicSkillProjectileLifetime;
                enemy.BasicSkillMoveSpeedMultiplier = sourceEnemy.BasicSkillMoveSpeedMultiplier;
                enemy.BasicSkillOutgoingDamageMultiplier = sourceEnemy.BasicSkillOutgoingDamageMultiplier;
                enemy.StageOneSkill = sourceEnemy.StageOneSkill;
                enemy.ActiveSkillName = sourceEnemy.ActiveSkillName;
                enemy.ActiveSkillCoefficient = sourceEnemy.ActiveSkillCoefficient;
                enemy.ActiveSkillCooldown = sourceEnemy.ActiveSkillCooldown;
                enemy.ActiveSkillDuration = sourceEnemy.ActiveSkillDuration;
                enemy.ActiveSkillRadius = sourceEnemy.ActiveSkillRadius;
                enemy.ActiveSkillFlatValue = sourceEnemy.ActiveSkillFlatValue;
                enemy.ActiveSkillProjectileSpeed = sourceEnemy.ActiveSkillProjectileSpeed;
                enemy.ActiveSkillProjectileLifetime = sourceEnemy.ActiveSkillProjectileLifetime;
                enemy.ActiveSkillMoveSpeedMultiplier = sourceEnemy.ActiveSkillMoveSpeedMultiplier;
                enemy.ActiveSkillOutgoingDamageMultiplier = sourceEnemy.ActiveSkillOutgoingDamageMultiplier;
                enemy.PassiveSkillName = sourceEnemy.PassiveSkillName;
                enemy.PassiveSkillId = sourceEnemy.PassiveSkillId;
                enemy.PassiveSkillValue = sourceEnemy.PassiveSkillValue;
                enemy.PassiveSummary = sourceEnemy.PassiveSummary;
                enemies.Add(enemy);
            }

            catalog.Monsters = monsters.ToArray();
            catalog.StageOneEnemies = enemies.ToArray();
            return catalog;
        }

        private static MonsterDefinition.RewardChoiceDefinition[] BuildRewardChoices(SourceModel model, string monsterId)
        {
            var rewards = new List<RewardChoiceRow>();
            foreach (var reward in model.RewardChoices.Values)
            {
                if (string.Equals(reward.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                {
                    rewards.Add(reward);
                }
            }

            rewards.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new MonsterDefinition.RewardChoiceDefinition[rewards.Count];
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                definitions[i] = new MonsterDefinition.RewardChoiceDefinition
                {
                    RewardId = reward.Id,
                    ActiveSkillId = reward.ActiveSkillId,
                    PassiveSkillId = reward.PassiveSkillId,
                    LinkedChoiceId = reward.LinkedChoiceId,
                    Title = reward.Title,
                    Description = reward.Description,
                    DamageMultiplier = reward.DamageMultiplier,
                    MagazineBonus = reward.MagazineBonus,
                    ShotIntervalMultiplier = reward.ShotIntervalMultiplier,
                    ReloadDurationMultiplier = reward.ReloadDurationMultiplier,
                    MaxHealthBonus = reward.MaxHealthBonus,
                    StatusChanceBonus = reward.StatusChanceBonus
                };
            }

            return definitions;
        }

        private static SkillDefinition[] BuildActiveSkills(SourceModel model, string monsterId)
        {
            var skills = new List<SkillRow>();
            foreach (var skill in model.Skills.Values)
            {
                if (skill.SkillKind == PakuriCsvSkillKind.Active
                    && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                {
                    skills.Add(skill);
                }
            }

            skills.Sort((left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new SkillDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                definitions[i] = new SkillDefinition
                {
                    SkillId = skill.Id,
                    DisplayName = skill.DisplayName,
                    Slot = skill.Slot,
                    RuntimeKind = skill.RuntimeKind,
                    ImplementationState = skill.ImplementationState,
                    IsDefaultLearned = skill.IsDefaultLearned,
                    SkillIcon = LoadSprite(skill.SkillIconPath),
                    DescriptionText = skill.DescriptionText,
                    Attribute = skill.Attribute,
                    BaseDamage = skill.BaseDamage,
                    AttackPowerCoefficient = skill.AttackPowerCoefficient,
                    SpellPowerCoefficient = skill.SpellPowerCoefficient,
                    Radius = skill.Radius,
                    CooldownSeconds = skill.CooldownSeconds,
                    ActiveDurationSeconds = skill.ActiveDurationSeconds,
                    MagazineCapacity = skill.MagazineCapacity,
                    ReloadSeconds = skill.ReloadSeconds,
                    ShotIntervalSeconds = skill.ShotIntervalSeconds,
                    ProjectileSpeed = skill.ProjectileSpeed,
                    PierceCount = skill.PierceCount,
                    CriticalAllowed = skill.CriticalAllowed,
                    StatusEffectId = skill.StatusEffectId,
                    StatusChance = skill.StatusChance,
                    StatusEffectLabel = skill.StatusEffectLabel,
                    Summary = skill.Summary,
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.ActiveEnhancement),
                    MasterSkillChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.ActiveMaster)
                };
            }

            return definitions;
        }

        private static PassiveDefinition[] BuildPassiveSkills(SourceModel model, string monsterId)
        {
            var skills = new List<SkillRow>();
            foreach (var skill in model.Skills.Values)
            {
                if (skill.SkillKind == PakuriCsvSkillKind.Passive
                    && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                {
                    skills.Add(skill);
                }
            }

            skills.Sort((left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new PassiveDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                definitions[i] = new PassiveDefinition
                {
                    PassiveId = skill.Id,
                    DisplayName = skill.DisplayName,
                    Slot = skill.Slot,
                    RequiredActiveSlot = skill.RequiredActiveSlot,
                    IsAvailableWithoutActiveRequirement = skill.IsAvailableWithoutActiveRequirement,
                    ImplementationState = skill.ImplementationState,
                    SkillIcon = LoadSprite(skill.SkillIconPath),
                    DescriptionText = skill.DescriptionText,
                    Summary = skill.Summary,
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.PassiveEnhancement)
                };
            }

            return definitions;
        }

        private static SkillChoiceDefinition[] BuildSkillChoices(SourceModel model, string skillId, PakuriCsvChoiceGroup choiceGroup)
        {
            var choices = new List<SkillChoiceRow>();
            foreach (var choice in model.SkillChoices.Values)
            {
                if (choice.ChoiceGroup == choiceGroup
                    && string.Equals(choice.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    choices.Add(choice);
                }
            }

            choices.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillChoiceDefinition[choices.Count];
            for (var i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                definitions[i] = new SkillChoiceDefinition
                {
                    ChoiceId = choice.Id,
                    Title = choice.Title,
                    SkillIcon = LoadSprite(choice.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(choice.SkillEffectPrefabPath),
                    DescriptionText = choice.DescriptionText
                };
            }

            return definitions;
        }

        private static IEnumerable<CatalogEntryRow> SortCatalogEntries(Dictionary<string, CatalogEntryRow> entries)
        {
            var list = new List<CatalogEntryRow>(entries.Values);
            list.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            return list;
        }

        private static Sprite LoadSprite(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (runtimeAssetCatalog != null && runtimeAssetCatalog.TryGetSprite(assetPath, out var sprite))
            {
                return sprite;
            }

            return null;
        }

        private static GameObject LoadPrefab(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (runtimeAssetCatalog != null && runtimeAssetCatalog.TryGetPrefab(assetPath, out var prefab))
            {
                return prefab;
            }

            return null;
        }
    }
}
