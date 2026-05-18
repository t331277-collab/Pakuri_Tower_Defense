using System;
using System.Collections.Generic;
using Pakuri.InGame;

namespace Pakuri.Data
{
    public static partial class PakuriCsvRuntimeData
    {
        private static void ValidateSourceModelOrThrow(SourceModel model, PakuriCsvRuntimeAssetCatalog assetCatalog)
        {
            var errors = new List<string>();

            if (model.Monsters.Count == 0)
            {
                errors.Add("monsters.csv has no monster rows.");
            }

            if (model.StageOneEnemies.Count == 0)
            {
                errors.Add("stage_one_enemies.csv has no enemy rows.");
            }

            if (model.EnemySkills.Count == 0)
            {
                errors.Add("EnemySkillData.csv has no enemy skill rows.");
            }

            ValidateCatalogEntries(model.CatalogMonsters, model.Monsters, "catalog_monsters.csv", errors);
            ValidateCatalogEntries(model.CatalogStageOneEnemies, model.StageOneEnemies, "catalog_stage_one_enemies.csv", errors);

            foreach (var reward in model.RewardChoices.Values)
            {
                if (!model.Monsters.ContainsKey(reward.MonsterId))
                {
                    errors.Add($"Reward choice '{reward.Id}' references unknown monster '{reward.MonsterId}'.");
                }

                if (!string.IsNullOrWhiteSpace(reward.ActiveSkillId))
                {
                    if (!model.Skills.TryGetValue(reward.ActiveSkillId, out var activeSkill))
                    {
                        errors.Add($"Reward choice '{reward.Id}' references unknown active skill '{reward.ActiveSkillId}'.");
                    }
                    else if (!string.Equals(activeSkill.MonsterId, reward.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Id}' active skill '{reward.ActiveSkillId}' belongs to '{activeSkill.MonsterId}', not '{reward.MonsterId}'.");
                    }
                    else if (activeSkill.SkillKind != PakuriCsvSkillKind.Active)
                    {
                        errors.Add($"Reward choice '{reward.Id}' targets non-active skill '{reward.ActiveSkillId}'.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(reward.PassiveSkillId))
                {
                    if (!model.Skills.TryGetValue(reward.PassiveSkillId, out var passiveSkill))
                    {
                        errors.Add($"Reward choice '{reward.Id}' references unknown passive skill '{reward.PassiveSkillId}'.");
                    }
                    else if (!string.Equals(passiveSkill.MonsterId, reward.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Id}' passive skill '{reward.PassiveSkillId}' belongs to '{passiveSkill.MonsterId}', not '{reward.MonsterId}'.");
                    }
                    else if (passiveSkill.SkillKind != PakuriCsvSkillKind.Passive)
                    {
                        errors.Add($"Reward choice '{reward.Id}' targets non-passive skill '{reward.PassiveSkillId}'.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(reward.LinkedChoiceId))
                {
                    if (!model.SkillChoices.TryGetValue(reward.LinkedChoiceId, out var linkedChoice))
                    {
                        errors.Add($"Reward choice '{reward.Id}' references unknown linked choice '{reward.LinkedChoiceId}'.");
                    }
                    else if (!string.Equals(linkedChoice.MonsterId, reward.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Id}' linked choice '{reward.LinkedChoiceId}' belongs to '{linkedChoice.MonsterId}', not '{reward.MonsterId}'.");
                    }
                }
            }

            foreach (var skill in model.Skills.Values)
            {
                if (!model.Monsters.ContainsKey(skill.MonsterId))
                {
                    errors.Add($"Skill '{skill.Id}' references unknown monster '{skill.MonsterId}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Active && skill.Slot > SkillSlot.E)
                {
                    errors.Add($"Active skill '{skill.Id}' uses passive slot '{skill.Slot}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Passive && skill.Slot < SkillSlot.F)
                {
                    errors.Add($"Passive skill '{skill.Id}' uses active slot '{skill.Slot}'.");
                }

                ValidateRuntimeStatusColumns(skill, errors);
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                if (!model.Monsters.ContainsKey(choice.MonsterId))
                {
                    errors.Add($"Skill choice '{choice.Id}' references unknown monster '{choice.MonsterId}'.");
                }

                if (!model.Skills.TryGetValue(choice.SkillId, out var skill))
                {
                    errors.Add($"Skill choice '{choice.Id}' references unknown skill '{choice.SkillId}'.");
                    continue;
                }

                if (!string.Equals(skill.MonsterId, choice.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Skill choice '{choice.Id}' monster mismatch: choice monster '{choice.MonsterId}', skill monster '{skill.MonsterId}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Active && choice.ChoiceGroup == PakuriCsvChoiceGroup.PassiveEnhancement)
                {
                    errors.Add($"Skill choice '{choice.Id}' uses PassiveEnhancement on active skill '{choice.SkillId}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Passive && choice.ChoiceGroup != PakuriCsvChoiceGroup.PassiveEnhancement)
                {
                    errors.Add($"Skill choice '{choice.Id}' uses active choice group on passive skill '{choice.SkillId}'.");
                }
            }

            foreach (var enemy in model.StageOneEnemies.Values)
            {
                if (!model.EnemySkills.ContainsKey(enemy.StageOneSkill.ToString()))
                {
                    errors.Add($"Enemy '{enemy.Id}' references unknown enemy skill '{enemy.StageOneSkill}'.");
                }

                if (enemy.HasBasicSkill && !model.EnemySkills.ContainsKey(enemy.BasicSkill.ToString()))
                {
                    errors.Add($"Enemy '{enemy.Id}' references unknown basic enemy skill '{enemy.BasicSkill}'.");
                }

                ValidateEnemyPassiveColumns(enemy, errors);
            }

            foreach (var monster in model.Monsters.Values)
            {
                var activeSlots = new HashSet<SkillSlot>();
                var passiveSlots = new HashSet<SkillSlot>();
                SkillRow slotA = null;
                SkillRow slotF = null;

                foreach (var skill in model.Skills.Values)
                {
                    if (!string.Equals(skill.MonsterId, monster.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (skill.SkillKind == PakuriCsvSkillKind.Active)
                    {
                        activeSlots.Add(skill.Slot);
                        if (skill.Slot == SkillSlot.A)
                        {
                            slotA = skill;
                        }
                    }
                    else
                    {
                        passiveSlots.Add(skill.Slot);
                        if (skill.Slot == SkillSlot.F)
                        {
                            slotF = skill;
                        }
                    }
                }

                ValidateExpectedSlots(monster.Id, activeSlots, SkillSlot.A, SkillSlot.E, "active", errors);
                ValidateExpectedSlots(monster.Id, passiveSlots, SkillSlot.F, SkillSlot.J, "passive", errors);

                if (slotA == null)
                {
                    errors.Add($"Monster '{monster.Id}' is missing slot A active skill.");
                }
                else
                {
                    if (!slotA.IsDefaultLearned)
                    {
                        errors.Add($"Monster '{monster.Id}' slot A active skill must be default learned.");
                    }

                    if (!string.Equals(monster.ActiveSkillName, slotA.DisplayName, StringComparison.Ordinal))
                    {
                        errors.Add(
                            $"Monster '{monster.Id}' active_skill_name '{monster.ActiveSkillName}' does not match slot A display name '{slotA.DisplayName}'.");
                    }
                }

                if (slotF == null)
                {
                    errors.Add($"Monster '{monster.Id}' is missing slot F passive skill.");
                }
                else
                {
                    if (!slotF.IsAvailableWithoutActiveRequirement)
                    {
                        errors.Add($"Monster '{monster.Id}' slot F passive must be available without active requirement.");
                    }

                    if (!string.Equals(monster.PassiveSkillName, slotF.DisplayName, StringComparison.Ordinal))
                    {
                        errors.Add(
                            $"Monster '{monster.Id}' passive_skill_name '{monster.PassiveSkillName}' does not match slot F display name '{slotF.DisplayName}'.");
                    }
                }
            }

            ValidateReferencedAssetCoverage(model, assetCatalog, errors);

            if (errors.Count > 0)
            {
                throw new CsvFatalException("Pakuri CSV source validation failed.", errors);
            }
        }

        private static void ValidateRuntimeStatusColumns(SkillRow skill, List<string> errors)
        {
            if (skill == null)
            {
                return;
            }

            var statusKey = !string.IsNullOrWhiteSpace(skill.StatusEffectId)
                ? skill.StatusEffectId.Trim()
                : skill.StatusEffectLabel != null ? skill.StatusEffectLabel.Trim() : string.Empty;
            var hasStatusKey = !string.IsNullOrWhiteSpace(statusKey)
                && !string.Equals(statusKey, "none", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(statusKey, "없음", StringComparison.OrdinalIgnoreCase);
            if (!hasStatusKey)
            {
                if (skill.StatusChance > 0f)
                {
                    errors.Add($"Skill '{skill.Id}' has status_chance '{skill.StatusChance}' but no runtime status id or parseable label.");
                }

                return;
            }

            if (skill.StatusChance > 0f && !StatusEffectUtility.TryParse(statusKey, out _))
            {
                errors.Add(
                    $"Skill '{skill.Id}' uses unsupported runtime status '{statusKey}'. Add it to StatusEffectKind or set status_chance to 0 for design-only labels.");
            }
        }

        private static void ValidateEnemyPassiveColumns(EnemyRow enemy, List<string> errors)
        {
            if (enemy == null)
            {
                return;
            }

            var passiveId = enemy.PassiveSkillId != null ? enemy.PassiveSkillId.Trim() : string.Empty;
            if (string.IsNullOrWhiteSpace(passiveId))
            {
                if (enemy.PassiveSkillValue > 0f)
                {
                    errors.Add($"Enemy '{enemy.Id}' has passive_skill_value '{enemy.PassiveSkillValue}' but no passive_skill_id.");
                }

                return;
            }

            if (!IsSupportedEnemyPassiveId(passiveId))
            {
                errors.Add($"Enemy '{enemy.Id}' uses unsupported passive_skill_id '{passiveId}'.");
            }

            if (enemy.PassiveSkillValue <= 0f)
            {
                errors.Add($"Enemy '{enemy.Id}' passive_skill_id '{passiveId}' requires a positive passive_skill_value.");
            }
        }

        private static bool IsSupportedEnemyPassiveId(string passiveId)
        {
            return string.Equals(passiveId, "PhysicalDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "DefenseUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "CritChanceUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "CritDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "HealingUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "IncomingDamageDown", StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateReferencedAssetCoverage(
            SourceModel model,
            PakuriCsvRuntimeAssetCatalog assetCatalog,
            List<string> errors)
        {
            if (assetCatalog == null)
            {
                errors.Add("PakuriCsvRuntimeAssetCatalog is null.");
                return;
            }

            foreach (var skill in model.Skills.Values)
            {
                ValidateSpritePath(assetCatalog, skill.SkillIconPath, $"Skill '{skill.Id}' skill_icon_path", errors);
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                ValidateSpritePath(assetCatalog, choice.SkillIconPath, $"Skill choice '{choice.Id}' skill_icon_path", errors);
                ValidatePrefabPath(assetCatalog, choice.SkillEffectPrefabPath, $"Skill choice '{choice.Id}' skill_effect_prefab_path", errors);
            }

            foreach (var enemy in model.StageOneEnemies.Values)
            {
                ValidateSpritePath(assetCatalog, enemy.UnitSpritePath, $"Enemy '{enemy.Id}' unit_sprite_path", errors);
                ValidateSpritePath(assetCatalog, enemy.ProjectileSpritePath, $"Enemy '{enemy.Id}' projectile_sprite_path", errors);
            }
        }

        private static void ValidateSpritePath(
            PakuriCsvRuntimeAssetCatalog assetCatalog,
            string assetPath,
            string ownerLabel,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            if (!assetCatalog.HasSprite(assetPath))
            {
                errors.Add($"{ownerLabel} references sprite asset '{assetPath}' that is not present in the runtime asset catalog.");
            }
        }

        private static void ValidatePrefabPath(
            PakuriCsvRuntimeAssetCatalog assetCatalog,
            string assetPath,
            string ownerLabel,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            if (!assetCatalog.HasPrefab(assetPath))
            {
                errors.Add($"{ownerLabel} references prefab asset '{assetPath}' that is not present in the runtime asset catalog.");
            }
        }

        private static void ValidateRuntimeCatalogOrThrow(GameDataCatalog catalog, SourceModel sourceModel)
        {
            var errors = new List<string>();
            if (catalog == null)
            {
                errors.Add("Runtime GameDataCatalog is null.");
            }
            else
            {
                if (catalog.Monsters == null || catalog.Monsters.Length == 0)
                {
                    errors.Add("Runtime GameDataCatalog has no monsters.");
                }

                if (catalog.StageOneEnemies == null || catalog.StageOneEnemies.Length == 0)
                {
                    errors.Add("Runtime GameDataCatalog has no stage-one enemies.");
                }
            }

            if (catalog != null && sourceModel != null)
            {
                ValidateRuntimeMonsterAssets(catalog.Monsters, sourceModel, errors);
                ValidateRuntimeEnemyAssets(catalog.StageOneEnemies, sourceModel, errors);
            }

            if (errors.Count > 0)
            {
                throw new CsvFatalException("Runtime GameDataCatalog validation failed.", errors);
            }
        }

        private static void ValidateRuntimeMonsterAssets(
            MonsterDefinition[] monsters,
            SourceModel sourceModel,
            List<string> errors)
        {
            if (monsters == null)
            {
                return;
            }

            for (var i = 0; i < monsters.Length; i++)
            {
                var monster = monsters[i];
                if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
                {
                    continue;
                }

                if (!sourceModel.Monsters.TryGetValue(monster.MonsterId, out var sourceMonster))
                {
                    errors.Add($"Runtime monster '{monster.MonsterId}' has no source row.");
                    continue;
                }

                ValidateRuntimeActiveSkillAssets(monster.ActiveSkills, sourceModel, monster.MonsterId, errors);
                ValidateRuntimePassiveSkillAssets(monster.PassiveSkills, sourceModel, monster.MonsterId, errors);
            }
        }

        private static void ValidateRuntimeEnemyAssets(
            EnemyDefinition[] enemies,
            SourceModel sourceModel,
            List<string> errors)
        {
            if (enemies == null)
            {
                return;
            }

            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || string.IsNullOrWhiteSpace(enemy.EnemyId))
                {
                    continue;
                }

                if (!sourceModel.StageOneEnemies.TryGetValue(enemy.EnemyId, out var sourceEnemy))
                {
                    errors.Add($"Runtime enemy '{enemy.EnemyId}' has no source row.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sourceEnemy.UnitSpritePath) && enemy.UnitSprite == null)
                {
                    errors.Add($"Runtime enemy '{enemy.EnemyId}' is missing UnitSprite for '{sourceEnemy.UnitSpritePath}'.");
                }

                if (!string.IsNullOrWhiteSpace(sourceEnemy.ProjectileSpritePath) && enemy.ProjectileSprite == null)
                {
                    errors.Add($"Runtime enemy '{enemy.EnemyId}' is missing ProjectileSprite for '{sourceEnemy.ProjectileSpritePath}'.");
                }
            }
        }

        private static void ValidateRuntimeActiveSkillAssets(
            SkillDefinition[] skills,
            SourceModel sourceModel,
            string monsterId,
            List<string> errors)
        {
            if (skills == null)
            {
                return;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill == null)
                {
                    continue;
                }

                var skillId = skill.SkillId;
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                if (!sourceModel.Skills.TryGetValue(skillId, out var sourceSkill))
                {
                    errors.Add($"Runtime skill '{skillId}' on monster '{monsterId}' has no source row.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sourceSkill.SkillIconPath) && skill.SkillIcon == null)
                {
                    errors.Add($"Runtime skill '{skillId}' is missing SkillIcon for '{sourceSkill.SkillIconPath}'.");
                }

                var enhancementChoices = skill.EnhancementChoices;
                if (enhancementChoices != null)
                {
                    ValidateRuntimeSkillChoiceAssets(enhancementChoices, sourceModel, skillId, errors);
                }

                if (skill.MasterSkillChoices != null)
                {
                    ValidateRuntimeSkillChoiceAssets(skill.MasterSkillChoices, sourceModel, skillId, errors);
                }
            }
        }

        private static void ValidateRuntimePassiveSkillAssets(
            PassiveDefinition[] skills,
            SourceModel sourceModel,
            string monsterId,
            List<string> errors)
        {
            if (skills == null)
            {
                return;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.PassiveId))
                {
                    continue;
                }

                if (!sourceModel.Skills.TryGetValue(skill.PassiveId, out var sourceSkill))
                {
                    errors.Add($"Runtime passive '{skill.PassiveId}' on monster '{monsterId}' has no source row.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sourceSkill.SkillIconPath) && skill.SkillIcon == null)
                {
                    errors.Add($"Runtime passive '{skill.PassiveId}' is missing SkillIcon for '{sourceSkill.SkillIconPath}'.");
                }

                ValidateRuntimeSkillChoiceAssets(skill.EnhancementChoices, sourceModel, skill.PassiveId, errors);
            }
        }

        private static void ValidateRuntimeSkillChoiceAssets(
            SkillChoiceDefinition[] choices,
            SourceModel sourceModel,
            string skillId,
            List<string> errors)
        {
            if (choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId))
                {
                    continue;
                }

                if (!sourceModel.SkillChoices.TryGetValue(choice.ChoiceId, out var sourceChoice))
                {
                    errors.Add($"Runtime skill choice '{choice.ChoiceId}' for skill '{skillId}' has no source row.");
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(sourceChoice.SkillIconPath) && choice.SkillIcon == null)
                {
                    errors.Add($"Runtime skill choice '{choice.ChoiceId}' is missing SkillIcon for '{sourceChoice.SkillIconPath}'.");
                }

                if (!string.IsNullOrWhiteSpace(sourceChoice.SkillEffectPrefabPath) && choice.SkillEffectPrefab == null)
                {
                    errors.Add($"Runtime skill choice '{choice.ChoiceId}' is missing SkillEffectPrefab for '{sourceChoice.SkillEffectPrefabPath}'.");
                }
            }
        }
    }
}
