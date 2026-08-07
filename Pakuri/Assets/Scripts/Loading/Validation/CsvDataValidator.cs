/*
 * 역할: CSV 원본 모델 검증.
 * 책임: 필수 행·테이블 간 참조·런타임 제약·스킬 그래프·적 데이터·에셋 범위를 검증한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.InGame;
using static Pakuri.Data.CsvAssetReferenceCollector;
using static Pakuri.Data.GameDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    /// CsvDataValidator 데이터를 런타임 카탈로그에 넣기 전에 검증한다.
    internal static class CsvDataValidator
    {

        internal static void ValidateSourceModelOrThrow(SourceModel model, CsvRuntimeCatalog assetCatalog)
        {
            var errors = new List<string>();

            if (model.Monsters.Count == 0)
            {
                errors.Add("monsters.csv has no monster rows.");
            }

            if (model.Enemies.Count == 0)
            {
                errors.Add("enemies.csv has no enemy rows.");
            }

            if (model.StatusEffects.Count == 0)
            {
                errors.Add("status_effects.csv has no status rows.");
            }

            ValidateCatalogEntries(model.CatalogMonsters, model.Monsters, "catalog_monsters.csv", errors);
            ValidateArtifactAndSummonRows(model, errors);

            foreach (var reward in model.RewardChoices.Values)
            {
                if (!model.Monsters.ContainsKey(reward.MonsterName))
                {
                    errors.Add($"Reward choice '{reward.Name}' references unknown monster '{reward.MonsterName}'.");
                }

                 if (!model.SkillChoices.TryGetValue(reward.Name, out var rewardChoice))
                 {
                     errors.Add($"Reward choice '{reward.Name}' has no matching skill choice row with the same choice_name.");
                     continue;
                 }

                 if (!string.Equals(rewardChoice.MonsterName, reward.MonsterName, StringComparison.OrdinalIgnoreCase))
                 {
                     errors.Add(
                         $"Reward choice '{reward.Name}' monster mismatch: reward monster '{reward.MonsterName}', choice monster '{rewardChoice.MonsterName}'.");
                 }

                if (!string.IsNullOrWhiteSpace(reward.ActiveSkillName))
                {
                    if (!model.Skills.TryGetValue(reward.ActiveSkillName, out var activeSkill))
                    {
                        errors.Add($"Reward choice '{reward.Name}' references unknown active skill '{reward.ActiveSkillName}'.");
                    }
                    else if (!string.Equals(activeSkill.MonsterName, reward.MonsterName, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Name}' active skill '{reward.ActiveSkillName}' belongs to '{activeSkill.MonsterName}', not '{reward.MonsterName}'.");
                    }
                    else if (activeSkill.SkillKind != PakuriCsvSkillKind.Active)
                    {
                        errors.Add($"Reward choice '{reward.Name}' targets non-active skill '{reward.ActiveSkillName}'.");
                    }
                    else if (!string.Equals(rewardChoice.SkillName, reward.ActiveSkillName, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Name}' active gate '{reward.ActiveSkillName}' does not match choice skill '{rewardChoice.SkillName}'.");
                    }
                    else if (rewardChoice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement)
                    {
                        errors.Add($"Reward choice '{reward.Name}' points passive choice group through active_skill_name.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(reward.PassiveSkillName))
                {
                    if (!model.Skills.TryGetValue(reward.PassiveSkillName, out var passiveSkill))
                    {
                        errors.Add($"Reward choice '{reward.Name}' references unknown passive skill '{reward.PassiveSkillName}'.");
                    }
                    else if (!string.Equals(passiveSkill.MonsterName, reward.MonsterName, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Name}' passive skill '{reward.PassiveSkillName}' belongs to '{passiveSkill.MonsterName}', not '{reward.MonsterName}'.");
                    }
                    else if (passiveSkill.SkillKind != PakuriCsvSkillKind.Passive)
                    {
                        errors.Add($"Reward choice '{reward.Name}' targets non-passive skill '{reward.PassiveSkillName}'.");
                    }
                    else if (!string.Equals(rewardChoice.SkillName, reward.PassiveSkillName, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Name}' passive gate '{reward.PassiveSkillName}' does not match choice skill '{rewardChoice.SkillName}'.");
                    }
                    else if (rewardChoice.ChoiceGroup != SkillChoiceGroup.PassiveEnhancement)
                    {
                        errors.Add($"Reward choice '{reward.Name}' points active choice group through passive_skill_name.");
                    }
                }

                if (string.IsNullOrWhiteSpace(reward.ActiveSkillName) && string.IsNullOrWhiteSpace(reward.PassiveSkillName))
                {
                    errors.Add($"Reward choice '{reward.Name}' must target either active_skill_name or passive_skill_name.");
                }
            }

            foreach (var skill in model.Skills.Values)
            {
                if (!model.Monsters.ContainsKey(skill.MonsterName))
                {
                    errors.Add($"Skill '{skill.Name}' references unknown monster '{skill.MonsterName}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Active && skill.Slot > SkillSlot.E)
                {
                    errors.Add($"Active skill '{skill.Name}' uses passive slot '{skill.Slot}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Passive && skill.Slot < SkillSlot.F)
                {
                    errors.Add($"Passive skill '{skill.Name}' uses active slot '{skill.Slot}'.");
                }

                ValidateSkillRuntimeValues(skill, errors);
                if (!string.IsNullOrWhiteSpace(skill.TargetSelection)
                    && !Enum.TryParse<SkillTargetSelection>(skill.TargetSelection, true, out _))
                {
                    errors.Add($"Skill '{skill.Name}' has unsupported target_selection '{skill.TargetSelection}'.");
                }

                if (!string.IsNullOrWhiteSpace(skill.HitTargetCount)
                    && !IsSupportedHitTargetCount(skill.HitTargetCount))
                {
                    errors.Add($"Skill '{skill.Name}' has unsupported hit_target_count '{skill.HitTargetCount}'. Expected positive integer or global.");
                }

                ValidateRuntimeStatusColumns(skill, model.StatusEffects, string.Empty, errors);
            }

            foreach (var enemySkill in model.EnemyBaseSkills.Values)
            {
                if (enemySkill == null || enemySkill.Skill == null)
                {
                    continue;
                }

                ValidateSkillRuntimeValues(enemySkill.Skill, errors);
                if (!string.IsNullOrWhiteSpace(enemySkill.Skill.HitTargetCount)
                    && !IsSupportedHitTargetCount(enemySkill.Skill.HitTargetCount))
                {
                    errors.Add($"Skill '{enemySkill.Skill.Name}' has unsupported hit_target_count '{enemySkill.Skill.HitTargetCount}'. Expected positive integer or global.");
                }

                ValidateRuntimeStatusColumns(enemySkill.Skill, model.StatusEffects, enemySkill.TargetScope, errors);
            }

            foreach (var trigger in model.SkillTriggers.Values)
            {
                ValidateSkillTriggerRow(trigger, model, errors);
            }

            foreach (var status in model.StatusEffects.Values)
            {
                ValidateStatusEffectRow(status, errors);
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                if (!model.Monsters.ContainsKey(choice.MonsterName))
                {
                    errors.Add($"Skill choice '{choice.Name}' references unknown monster '{choice.MonsterName}'.");
                }

                if (!model.Skills.TryGetValue(choice.SkillName, out var skill))
                {
                    errors.Add($"Skill choice '{choice.Name}' references unknown skill '{choice.SkillName}'.");
                    continue;
                }

                if (!string.Equals(skill.MonsterName, choice.MonsterName, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(
                        $"Skill choice '{choice.Name}' monster mismatch: choice monster '{choice.MonsterName}', skill monster '{skill.MonsterName}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Active
                    && choice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement)
                {
                    errors.Add($"Skill choice '{choice.Name}' uses passive-only choice group on active skill '{choice.SkillName}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Passive
                    && choice.ChoiceGroup != SkillChoiceGroup.PassiveEnhancement)
                {
                    errors.Add($"Skill choice '{choice.Name}' uses active choice group on passive skill '{choice.SkillName}'.");
                }

            }

            ValidateNormalizedSkillAuthoringRows(model, assetCatalog, errors);
            ValidateSkillNodeHandlers(model, errors);
            ValidateTriggerOutcomes(model, errors);

            ValidateUnitRuntimeValues(model, errors);
            ValidateEnemyRows(model, errors);

            foreach (var monster in model.Monsters.Values)
            {
                var activeSlots = new HashSet<SkillSlot>();
                var passiveSlots = new HashSet<SkillSlot>();
                SkillRow slotA = null;
                SkillRow slotF = null;

                foreach (var skill in model.Skills.Values)
                {
                    if (!string.Equals(skill.MonsterName, monster.Name, StringComparison.OrdinalIgnoreCase))
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

                ValidateExpectedSlots(monster.Name, activeSlots, SkillSlot.A, SkillSlot.E, "active", errors);
                ValidateExpectedSlots(monster.Name, passiveSlots, SkillSlot.F, SkillSlot.J, "passive", errors);

                if (slotA == null)
                {
                    errors.Add($"Monster '{monster.Name}' is missing slot A active skill.");
                }
                else
                {
                    if (!slotA.IsDefaultLearned)
                    {
                        errors.Add($"Monster '{monster.Name}' slot A active skill must be default learned.");
                    }

                    if (!string.IsNullOrWhiteSpace(monster.ActiveSkillName)
                        && !string.Equals(monster.ActiveSkillName, slotA.DisplayName, StringComparison.Ordinal))
                    {
                        errors.Add(
                            $"Monster '{monster.Name}' active_skill_name '{monster.ActiveSkillName}' does not match slot A display name '{slotA.DisplayName}'.");
                    }
                }

                if (slotF == null)
                {
                    errors.Add($"Monster '{monster.Name}' is missing slot F passive skill.");
                }
                else
                {
                    if (!slotF.IsAvailableWithoutActiveRequirement)
                    {
                        errors.Add($"Monster '{monster.Name}' slot F passive must be available without active requirement.");
                    }

                    if (!string.IsNullOrWhiteSpace(monster.PassiveSkillName)
                        && !string.Equals(monster.PassiveSkillName, slotF.DisplayName, StringComparison.Ordinal))
                    {
                        errors.Add(
                            $"Monster '{monster.Name}' passive_skill_name '{monster.PassiveSkillName}' does not match slot F display name '{slotF.DisplayName}'.");
                    }
                }
            }

            ValidateReferencedAssetCoverage(model, assetCatalog, errors);

            if (errors.Count > 0)
            {
                throw new CsvFatalException("Pakuri CSV source validation failed.", errors);
            }
        }

        internal static void ValidateRuntimeStatusColumns(
            SkillRow skill,
            Dictionary<string, StatusEffectRow> statusEffects,
            string targetScope,
            List<string> errors)
        {
            if (skill == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusName)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.DeploymentRequiredTargetStatusName)))
            {
                errors.Add($"Skill '{skill.Name}' uses unsupported deployment_required_target_status_name '{skill.DeploymentRequiredTargetStatusName}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetSelectionStatusName)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.TargetSelectionStatusName)))
            {
                errors.Add($"Skill '{skill.Name}' uses unsupported target_selection_status_name '{skill.TargetSelectionStatusName}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetStatusStackStatusName)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.TargetStatusStackStatusName)))
            {
                errors.Add($"Skill '{skill.Name}' uses unsupported target_status_stack_status_name '{skill.TargetStatusStackStatusName}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.ConsumeTargetStatusName)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.ConsumeTargetStatusName)))
            {
                errors.Add($"Skill '{skill.Name}' uses unsupported consume_target_status_name '{skill.ConsumeTargetStatusName}'.");
            }

            var status = skill.Status;
            var statusKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(status.StatusEffectName))
            {
                statusKey = status.StatusEffectName.Trim();
            }

            if (string.IsNullOrWhiteSpace(statusKey))
            {
                if (status.StatusChance > 0f)
                {
                    errors.Add($"Skill '{skill.Name}' has status_chance '{status.StatusChance}' but no status_effect_name.");
                }

                return;
            }

            StatusEffectRow statusDefinition = null;
            if (statusEffects == null || !statusEffects.TryGetValue(statusKey, out statusDefinition))
            {
                errors.Add($"Skill '{skill.Name}' uses status_effect_name '{statusKey}' but status_effects.csv has no matching row.");
                return;
            }

            if (!StatusValueParser.TryParseStatusKind(statusKey, out var kind))
            {
                errors.Add($"Skill '{skill.Name}' uses status_effect_name '{statusKey}' that cannot map to StatusEffectKind.");
                return;
            }

            if (statusDefinition.Classification == StatusEffectClassification.Buff)
            {
                if (string.IsNullOrWhiteSpace(targetScope)
                    && !StatusValueParser.TryParseTargetScope(status.StatusTargetScope, out _))
                {
                    errors.Add($"Skill '{skill.Name}' requires supported status_target_scope. Expected self or all_allies.");
                }

                if (!StatusValueParser.TryParseMergePolicy(status.StatusMergePolicy, out _))
                {
                    errors.Add($"Skill '{skill.Name}' requires supported status_merge_policy for buff status '{statusKey}'.");
                }
            }

            if (kind == StatusEffectKind.Shield)
            {
                if (!StatusValueParser.TryParseShieldRefreshRule(status.ShieldAmountRefreshPolicy, out _))
                {
                    errors.Add($"Skill '{skill.Name}' requires supported shield_amount_refresh_policy for shield status.");
                }

                if (status.StatusDurationSeconds <= 0f)
                {
                    errors.Add($"Skill '{skill.Name}' requires positive status_duration_seconds for shield status.");
                }
            }
        }

        internal static void ValidateSkillTriggerRow(
            SkillTriggerRow trigger,
            SourceModel model,
            List<string> errors)
        {
            if (trigger == null)
            {
                return;
            }

            var artifactSource = model != null
                && SkillGraphParser.IsArtifactEffectOwner(
                    model,
                    trigger.SourceSkillName,
                    trigger.MonsterName);
            SkillRow sourceSkill = null;
            if (model == null
                || (!model.Monsters.ContainsKey(trigger.MonsterName)
                    && !model.Summons.ContainsKey(trigger.MonsterName)
                    && !artifactSource))
            {
                errors.Add($"Skill trigger '{trigger.Name}' references unknown monster '{trigger.MonsterName}'.");
            }

            var sourceSkillFound = model != null
                && (model.Skills.TryGetValue(trigger.SourceSkillName, out sourceSkill)
                    || model.SummonSkills.TryGetValue(trigger.SourceSkillName, out sourceSkill));
            if (!artifactSource && !sourceSkillFound)
            {
                errors.Add($"Skill trigger '{trigger.Name}' references unknown source skill '{trigger.SourceSkillName}'.");
            }
            else if (!artifactSource
                && !string.Equals(sourceSkill.MonsterName, trigger.MonsterName, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Skill trigger '{trigger.Name}' source skill '{trigger.SourceSkillName}' belongs to '{sourceSkill.MonsterName}', not '{trigger.MonsterName}'.");
            }

            if (!string.IsNullOrWhiteSpace(trigger.RequiredSourceStatusName)
                && (model == null || !model.StatusEffects.ContainsKey(trigger.RequiredSourceStatusName)))
            {
                errors.Add($"Skill trigger '{trigger.Name}' uses unsupported required_source_status_name '{trigger.RequiredSourceStatusName}'.");
            }

            var hasOwnedNodes = HasOwnedTriggerNodeSource(model, trigger.Name);
            if (!hasOwnedNodes)
            {
                errors.Add($"Skill trigger '{trigger.Name}' requires at least one owned node.");
            }

            if (trigger.RepeatCount <= 0)
            {
                errors.Add($"Skill trigger '{trigger.Name}' requires repeat_count greater than 0.");
            }

            if (trigger.RepeatIntervalSeconds < 0f)
            {
                errors.Add($"Skill trigger '{trigger.Name}' has negative repeat_interval_seconds.");
            }

            if (trigger.TriggerDelaySeconds < 0f)
            {
                errors.Add($"Skill trigger '{trigger.Name}' has negative trigger_delay_seconds.");
            }

            if (trigger.TriggerEveryCount < 0)
            {
                errors.Add($"Skill trigger '{trigger.Name}' has negative trigger_every_count.");
            }

            if (!ValidateEventSourceScope(trigger.EventSourceScope))
            {
                errors.Add($"Skill trigger '{trigger.Name}' has unsupported event_source_scope '{trigger.EventSourceScope}'. Expected owner or all_allies.");
            }

            if (trigger.ProcChance < 0f || trigger.ProcChance > 1f)
            {
                errors.Add($"Skill trigger '{trigger.Name}' has proc_chance '{trigger.ProcChance}' outside 0..1.");
            }

            if (trigger.InternalCooldownSeconds < 0f)
            {
                errors.Add($"Skill trigger '{trigger.Name}' has negative internal_cooldown_seconds.");
            }

            ValidateTriggerChoiceReference(trigger.RequiresActiveChoiceName, trigger, model, "requires_active_choice_name", errors);
            ValidateTriggerChoiceReference(trigger.ExcludesActiveChoiceName, trigger, model, "excludes_active_choice_name", errors);

            ValidateStatusConditionExpression(trigger.Name, trigger.ConditionStatusName, model, errors);

            if (!string.IsNullOrWhiteSpace(trigger.TriggerAttribute)
                && !ValidateTriggerAttributes(trigger.TriggerAttribute))
            {
                errors.Add($"Skill trigger '{trigger.Name}' uses unsupported trigger_attribute '{trigger.TriggerAttribute}'.");
            }

            if (!ValidateSkillRuntimeKindList(trigger.EventSkillRuntimeKinds))
            {
                errors.Add($"Skill trigger '{trigger.Name}' uses unsupported event_skill_runtime_kinds '{trigger.EventSkillRuntimeKinds}'.");
            }

            if (!ValidateSkillSlotList(trigger.EventSkillSlots))
            {
                errors.Add($"Skill trigger '{trigger.Name}' uses unsupported event_skill_slots '{trigger.EventSkillSlots}'.");
            }

            ValidateSkillIdList(trigger.EventSkillName, trigger, model, "event_skill_name", errors);
        }

        internal static bool ValidateSkillRuntimeKindList(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var tokens = rawValue.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = string.Empty;
                if (tokens[i] != null)
                {
                    token = tokens[i].Trim();
                }
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (string.Equals(token, "Area", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(token, "AoE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!Enum.TryParse(token, true, out SkillRuntimeKind _))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool ValidateSkillSlotList(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var tokens = rawValue.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i]?.Trim();
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (!Enum.TryParse(token, true, out SkillSlot _))
                {
                    return false;
                }
            }

            return true;
        }

        internal static void ValidateSkillIdList(
            string rawSkillNames,
            SkillTriggerRow trigger,
            SourceModel model,
            string columnName,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(rawSkillNames))
            {
                return;
            }

            var skillNames = rawSkillNames.Split(';', ',');
            for (var i = 0; i < skillNames.Length; i++)
            {
                var skillName = string.Empty;
                if (skillNames[i] != null)
                {
                    skillName = skillNames[i].Trim();
                }
                if (string.IsNullOrWhiteSpace(skillName))
                {
                    continue;
                }

                if (model == null
                    || (!model.Skills.ContainsKey(skillName) && !model.SummonSkills.ContainsKey(skillName)))
                {
                    errors.Add($"Skill trigger '{trigger.Name}' {columnName} references unknown skill '{skillName}'.");
                }
            }
        }

        private static void ValidateArtifactAndSummonRows(SourceModel model, List<string> errors)
        {
            if (model.Artifacts.Count == 0)
            {
                errors.Add("artifacts.csv has no rows.");
            }

            if (model.ArtifactSynergies.Count == 0)
            {
                errors.Add("artifact_synergies.csv has no rows.");
            }

            if (model.Summons.Count == 0)
            {
                errors.Add("summon_units.csv has no rows.");
            }

            var synergyLevelNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var synergy in model.ArtifactSynergies.Values)
            {
                if (synergy.EffectAlphaPercent < 0f || synergy.EffectAlphaPercent > 100f)
                {
                    errors.Add($"Artifact synergy '{synergy.Name}' synergy_effect_alpha must be between 0 and 100.");
                }

                var previousRequiredCount = 0;
                for (var i = 0; i < synergy.Levels.Length; i++)
                {
                    var level = synergy.Levels[i];
                    if (!synergyLevelNames.Add(level.Name))
                    {
                        errors.Add($"Artifact synergy level Name '{level.Name}' is duplicated.");
                    }

                    if (level.RequiredCount <= previousRequiredCount)
                    {
                        errors.Add($"Artifact synergy '{synergy.Name}' level '{level.Name}' requires a count greater than the previous level.");
                    }

                    var expectedRequiredCount = (i + 1) * 2;
                    if (level.RequiredCount != expectedRequiredCount)
                    {
                        errors.Add($"Artifact synergy '{synergy.Name}' level '{level.Name}' requires count '{level.RequiredCount}', expected '{expectedRequiredCount}'.");
                    }

                    previousRequiredCount = level.RequiredCount;
                }
            }

            foreach (var artifact in model.Artifacts.Values)
            {
                if (!model.ArtifactSynergies.ContainsKey(artifact.SynergyName))
                {
                    errors.Add($"Artifact '{artifact.Name}' references unknown synergy '{artifact.SynergyName}'.");
                }
            }

            foreach (var effect in model.ArtifactEffects.Values)
            {
                if (!model.Artifacts.ContainsKey(effect.ArtifactName))
                {
                    errors.Add($"Artifact effect '{effect.Name}' references unknown artifact '{effect.ArtifactName}'.");
                }

                ValidateArtifactEffectReferences(
                    effect.Name,
                    effect.ApplicationMode,
                    effect.Recipient,
                    effect.RepeatRule,
                    effect.SelectionRule,
                    effect.RecipientMonsterName,
                    effect.TargetSkillName,
                    effect.OutcomeSkillName,
                    string.Empty,
                    model,
                    errors);
            }

            foreach (var effect in model.ArtifactSynergyEffects.Values)
            {
                if (!synergyLevelNames.Contains(effect.SynergyLevelName))
                {
                    errors.Add($"Artifact synergy effect '{effect.Name}' references unknown level '{effect.SynergyLevelName}'.");
                }

                ValidateArtifactEffectReferences(
                    effect.Name,
                    effect.ApplicationMode,
                    effect.Recipient,
                    ArtifactEffectRepeatRule.None,
                    ArtifactEffectSelectionRule.None,
                    effect.RecipientMonsterName,
                    effect.TargetSkillName,
                    effect.OutcomeSkillName,
                    effect.SpawnSummonName,
                    model,
                    errors);
            }

            foreach (var summon in model.Summons.Values)
            {
                if (model.Monsters.ContainsKey(summon.Name))
                {
                    errors.Add($"Summon '{summon.Name}' conflicts with a monster Name.");
                }

                var slots = new HashSet<SkillSlot>();
                foreach (var skill in model.SummonSkills.Values)
                {
                    if (!string.Equals(skill.MonsterName, summon.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!slots.Add(skill.Slot))
                    {
                        errors.Add($"Summon '{summon.Name}' has duplicate active slot '{skill.Slot}'.");
                    }
                }

                for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
                {
                    if (!slots.Contains(slot))
                    {
                        errors.Add($"Summon '{summon.Name}' is missing active slot '{slot}'.");
                    }
                }
            }

            foreach (var skill in model.SummonSkills.Values)
            {
                if (!model.Summons.ContainsKey(skill.MonsterName))
                {
                    errors.Add($"Summon skill '{skill.Name}' references unknown summon '{skill.MonsterName}'.");
                }

                if (model.Skills.ContainsKey(skill.Name))
                {
                    errors.Add($"Summon skill '{skill.Name}' conflicts with a monster skill Name.");
                }

                ValidateSkillRuntimeValues(skill, errors);
                ValidateRuntimeStatusColumns(skill, model.StatusEffects, string.Empty, errors);
            }
        }

        private static void ValidateArtifactEffectReferences(
            string effectName,
            ArtifactEffectApplicationMode applicationMode,
            ArtifactEffectRecipient recipient,
            ArtifactEffectRepeatRule repeatRule,
            ArtifactEffectSelectionRule selectionRule,
            string recipientMonsterName,
            string targetSkillName,
            string outcomeSkillName,
            string spawnSummonName,
            SourceModel model,
            List<string> errors)
        {
            if (repeatRule != ArtifactEffectRepeatRule.None
                && (applicationMode != ArtifactEffectApplicationMode.SkillModifier
                    || (recipient != ArtifactEffectRecipient.AllAllies
                        && recipient != ArtifactEffectRecipient.Owner)))
            {
                errors.Add($"Artifact effect '{effectName}' repeat_rule requires SkillModifier and AllAllies or Owner.");
            }

            if (selectionRule != ArtifactEffectSelectionRule.None
                && (applicationMode != ArtifactEffectApplicationMode.SkillModifier
                    || recipient != ArtifactEffectRecipient.AllAllies))
            {
                errors.Add($"Artifact effect '{effectName}' selection_rule requires SkillModifier and AllAllies.");
            }

            if (recipient == ArtifactEffectRecipient.SpecificMonster
                && (string.IsNullOrWhiteSpace(recipientMonsterName)
                    || !model.Monsters.ContainsKey(recipientMonsterName)))
            {
                errors.Add($"Artifact effect '{effectName}' requires a known recipient_monster_name.");
            }

            if (!string.IsNullOrWhiteSpace(targetSkillName)
                && !model.Skills.ContainsKey(targetSkillName)
                && !model.SummonSkills.ContainsKey(targetSkillName))
            {
                errors.Add($"Artifact effect '{effectName}' references unknown target skill '{targetSkillName}'.");
            }

            if (!string.IsNullOrWhiteSpace(outcomeSkillName)
                && !model.Skills.ContainsKey(outcomeSkillName)
                && !model.SummonSkills.ContainsKey(outcomeSkillName))
            {
                errors.Add($"Artifact effect '{effectName}' references unknown outcome skill '{outcomeSkillName}'.");
            }

            if (applicationMode == ArtifactEffectApplicationMode.GrantSkill
                && string.IsNullOrWhiteSpace(outcomeSkillName))
            {
                errors.Add($"Artifact effect '{effectName}' requires outcome_skill_name for GrantSkill.");
            }

            if (applicationMode == ArtifactEffectApplicationMode.SpawnUnit)
            {
                if (string.IsNullOrWhiteSpace(spawnSummonName)
                    || !model.Summons.ContainsKey(spawnSummonName))
                {
                    errors.Add($"Artifact effect '{effectName}' requires a known spawn_monster_name for SpawnUnit.");
                }
            }
            else if (!string.IsNullOrWhiteSpace(spawnSummonName))
            {
                errors.Add($"Artifact effect '{effectName}' may use spawn_monster_name only with SpawnUnit.");
            }
        }

        private static void ValidateUnitRuntimeValues(SourceModel model, List<string> errors)
        {
            foreach (var monster in model.Monsters.Values)
            {
                if (monster.MaxHealth <= 0f)
                {
                    errors.Add($"Monster '{monster.Name}' requires positive max_health.");
                }
            }

            foreach (var enemy in model.Enemies.Values)
            {
                if (enemy.MaxHealth <= 0f)
                {
                    errors.Add($"Enemy '{enemy.Name}' requires positive max_health.");
                }
            }

            foreach (var summon in model.Summons.Values)
            {
                if (summon.MaxHealth <= 0f)
                {
                    errors.Add($"Summon '{summon.Name}' requires positive max_health.");
                }
            }

            foreach (var skill in model.EnemyBaseSkills.Values)
            {
                if (skill.Skill == null || skill.Skill.SkillKind != PakuriCsvSkillKind.Active)
                {
                    continue;
                }

                if (!skill.TargetScope.StartsWith("Hostile", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (skill.CastRange <= 0f)
                {
                    errors.Add($"Enemy skill '{skill.Skill.Name}' requires positive cast_range for hostile targeting.");
                }
            }
        }

        internal static void ValidateSkillRuntimeValues(SkillRow skill, List<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(skill.RuntimeVisualAnchor)
                && !Enum.TryParse<RuntimeSkillVisualAnchor>(skill.RuntimeVisualAnchor, true, out _))
            {
                errors.Add($"Skill '{skill.Name}' has unsupported runtime_visual_anchor '{skill.RuntimeVisualAnchor}'.");
            }

            if (skill.RuntimeVisualScale <= 0f || skill.RuntimeImpactVisualScale <= 0f)
            {
                errors.Add($"Skill '{skill.Name}' requires positive runtime visual scale values.");
            }

            if (skill.RuntimeHitboxSizeX < 0f || skill.RuntimeHitboxSizeY < 0f)
            {
                errors.Add($"Skill '{skill.Name}' has a negative runtime hitbox size.");
            }

            if (skill.SkillKind == PakuriCsvSkillKind.Active && skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                errors.Add($"Active skill '{skill.Name}' cannot use Passive runtime_kind.");
            }

            if (skill.SkillKind == PakuriCsvSkillKind.Passive && skill.RuntimeKind != SkillRuntimeKind.Passive)
            {
                errors.Add($"Passive skill '{skill.Name}' must use Passive runtime_kind.");
            }

            if (skill.Radius < 0f
                || skill.DamageDelaySeconds < 0f
                || skill.CooldownSeconds < 0f
                || skill.ReloadSeconds < 0f
                || skill.ShotIntervalSeconds < 0f
                || skill.MagazineCapacity < 0
                || skill.ProjectileBurstCount < 0
                || skill.ProjectileSpeed < 0f
                || skill.PierceCount < 0
                || skill.Status.StatusChance < 0f
                || skill.Status.StatusChance > 1f)
            {
                errors.Add($"Skill '{skill.Name}' contains a negative runtime value or a status chance outside 0..1.");
            }

            if (skill.SkillKind == PakuriCsvSkillKind.Passive)
            {
                return;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.MagazineProjectile)
            {
                if (skill.MagazineCapacity <= 0)
                {
                    errors.Add($"Magazine projectile '{skill.Name}' requires positive magazine_capacity.");
                }

                if (skill.ReloadSeconds <= 0f)
                {
                    errors.Add($"Magazine projectile '{skill.Name}' requires positive reload_seconds.");
                }

                if (skill.ShotIntervalSeconds <= 0f)
                {
                    errors.Add($"Magazine projectile '{skill.Name}' requires positive shot_interval_seconds.");
                }

                if (skill.ProjectileSpeed <= 0f)
                {
                    errors.Add($"Projectile skill '{skill.Name}' requires positive projectile_speed.");
                }

                return;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.CooldownProjectile)
            {
                if (skill.ProjectileSpeed <= 0f)
                {
                    errors.Add($"Projectile skill '{skill.Name}' requires positive projectile_speed.");
                }

                return;
            }

            if (skill.CooldownSeconds <= 0f)
            {
                errors.Add($"Active skill '{skill.Name}' requires positive cooldown_seconds.");
            }
        }

        internal static bool HasOwnedTriggerNodeSource(
            SourceModel model,
            string triggerName)
        {
            if (model == null || string.IsNullOrWhiteSpace(triggerName))
            {
                return false;
            }

            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                if (graph != null
                    && (graph.OwnerKind == SkillNodeOwnerKind.Trigger
                        || graph.OwnerKind == SkillNodeOwnerKind.Base)
                    && string.Equals(graph.OwnerName, triggerName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool ValidateEventSourceScope(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var normalized = rawValue.Trim();
            return string.Equals(normalized, "owner", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "all_allies", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ValidateTriggerAttributes(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var tokens = rawValue.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = string.Empty;
                if (tokens[i] != null)
                {
                    token = tokens[i].Trim();
                }
                if (string.IsNullOrWhiteSpace(token))
                {
                    continue;
                }

                if (!Enum.TryParse<DamageAttribute>(token, true, out _))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsSupportedHitTargetCount(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var normalized = rawValue.Trim();
            if (string.Equals(normalized, "global", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "all", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return int.TryParse(normalized, out var count) && count > 0;
        }

        internal static void ValidateTriggerChoiceReference(
            string choiceName,
            SkillTriggerRow trigger,
            SourceModel model,
            string columnName,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(choiceName))
            {
                return;
            }

            var choiceNames = choiceName.Split(';', ',');
            for (var i = 0; i < choiceNames.Length; i++)
            {
                var currentChoiceName = string.Empty;
                if (choiceNames[i] != null)
                {
                    currentChoiceName = choiceNames[i].Trim();
                }
                if (string.IsNullOrWhiteSpace(currentChoiceName))
                {
                    continue;
                }

                if (model == null || !model.SkillChoices.TryGetValue(currentChoiceName, out var choice))
                {
                    errors.Add($"Skill trigger '{trigger.Name}' {columnName} references unknown choice '{currentChoiceName}'.");
                    continue;
                }

                if (!ChoiceAppliesToSkillName(choice, trigger.SourceSkillName))
                {
                    errors.Add($"Skill trigger '{trigger.Name}' {columnName} choice '{currentChoiceName}' does not apply to source skill '{trigger.SourceSkillName}'.");
                }
            }
        }

        internal static bool ChoiceAppliesToSkillName(SkillChoiceRow choice, string skillName)
        {
            if (choice == null || string.IsNullOrWhiteSpace(skillName))
            {
                return false;
            }

            if (string.Equals(choice.SkillName, skillName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(choice.TargetSkillName, skillName, StringComparison.OrdinalIgnoreCase);
        }

        internal static void ValidateStatusIdList(
            string ownerName,
            string rawValue,
            SourceModel model,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                errors.Add($"Skill node '{ownerName}' requires status_ids.");
                return;
            }

            var statusNames = rawValue.Split(';');
            for (var i = 0; i < statusNames.Length; i++)
            {
                var statusName = statusNames[i].Trim();
                if (string.IsNullOrWhiteSpace(statusName)
                    || model == null
                    || !model.StatusEffects.ContainsKey(statusName))
                {
                    errors.Add($"Skill node '{ownerName}' references unknown status '{statusName}' in status_ids.");
                }
            }
        }

        private static void ValidateStatusConditionExpression(
            string ownerName,
            string rawValue,
            SourceModel model,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return;
            }

            if (!StatusValueParser.TryParseConditionStatusExpression(rawValue, out _))
            {
                errors.Add($"Skill trigger '{ownerName}' uses unsupported condition_status_name '{rawValue}'.");
                return;
            }

            var groups = rawValue.Split(';', ',');
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var requirements = groups[groupIndex].Split('&');
                for (var requirementIndex = 0; requirementIndex < requirements.Length; requirementIndex++)
                {
                    var requirement = requirements[requirementIndex].Trim();
                    var statusName = requirement;
                    var separatorIndex = requirement.IndexOf(">=", StringComparison.Ordinal);
                    if (separatorIndex < 0)
                    {
                        separatorIndex = requirement.IndexOf(':');
                    }

                    if (separatorIndex >= 0)
                    {
                        statusName = requirement.Substring(0, separatorIndex).Trim();
                    }

                    if (model == null || !model.StatusEffects.ContainsKey(statusName))
                    {
                        errors.Add($"Skill trigger '{ownerName}' references unknown status '{statusName}' in condition_status_name.");
                    }
                }
            }
        }

        internal static void ValidateStatusEffectRow(StatusEffectRow status, List<string> errors)
        {
            if (status == null)
            {
                return;
            }

            if (!StatusValueParser.TryParseStatusKind(status.Name, out var kind) || kind == StatusEffectKind.None)
            {
                errors.Add($"Status effect '{status.Name}' is not supported by StatusEffectKind.");
            }

            if (kind == StatusEffectKind.Shield
                && !string.Equals(status.Name, "shield", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Shield status row '{status.Name}' must use canonical Name 'shield'.");
            }

            if (status.BaseStackAmount <= 0)
            {
                errors.Add($"Status effect '{status.Name}' requires base_stack_amount greater than 0.");
            }

            if (!status.IsPermanent && status.DefaultDurationSeconds < 0f)
            {
                errors.Add($"Status effect '{status.Name}' has negative default_duration_seconds.");
            }
        }

        internal static void ValidateReferencedAssetCoverage(
            SourceModel model,
            CsvRuntimeCatalog assetCatalog,
            List<string> errors)
        {
            if (assetCatalog == null)
            {
                errors.Add("CsvRuntimeCatalog is null.");
                return;
            }

            var referencedAssets = CollectReferencedAssets(model);
            foreach (var asset in referencedAssets.SpritePaths)
            {
                ValidateSpritePath(assetCatalog, asset.AssetPath, asset.OwnerLabel, errors);
            }

            foreach (var asset in referencedAssets.PrefabPaths)
            {
                ValidatePrefabPath(assetCatalog, asset.AssetPath, asset.OwnerLabel, errors);
            }

            foreach (var asset in referencedAssets.AnimatorControllerPaths)
            {
                ValidateAnimatorControllerPath(assetCatalog, asset.AssetPath, asset.OwnerLabel, errors);
            }
        }

        internal static void ValidateSpritePath(
            CsvRuntimeCatalog assetCatalog,
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

        internal static void ValidatePrefabPath(
            CsvRuntimeCatalog assetCatalog,
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

        private static void ValidateSkillNodeHandlers(SourceModel model, List<string> errors)
        {
            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                if (!model.SkillNodeTypes.TryGetValue(graph.NodeTypeName, out var nodeType))
                {
                    continue;
                }
                if (GameDataCatalogBuilder.CanProcessNode(graph.OwnerKind.ToString(), nodeType.HandlerName))
                {
                    continue;
                }

                errors.Add(
                    $"Skill graph '{BuildSkillGraphKey(graph)}' node '{graph.NodeOrder}' uses handler '{nodeType.HandlerName}' without a combat conversion route.");
            }
        }

        private static void ValidateTriggerOutcomes(
            SourceModel model,
            List<string> errors)
        {
            var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var node in model.SkillNodes.Values)
            {
                if (node == null
                    || !node.EnabledByDefault
                    || (node.OwnerKind != SkillNodeOwnerKind.Trigger
                        && node.OwnerKind != SkillNodeOwnerKind.Base)
                    || !GameDataCatalogBuilder.IsTriggerOutcomeHandler(node.HandlerName))
                {
                    continue;
                }

                counts.TryGetValue(node.OwnerName, out var count);
                counts[node.OwnerName] = count + 1;
            }

            foreach (var pair in counts)
            {
                if (pair.Value > 1)
                {
                    errors.Add(
                        $"Skill trigger '{pair.Key}' has {pair.Value} runtime outcomes; expected at most one.");
                }
            }
        }

        internal static void ValidateAnimatorControllerPath(
            CsvRuntimeCatalog assetCatalog,
            string assetPath,
            string ownerLabel,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            if (!assetCatalog.HasAnimatorController(assetPath))
            {
                errors.Add($"{ownerLabel} references animator controller asset '{assetPath}' that is not present in the runtime asset catalog.");
            }
        }

        internal static void ValidateCatalogEntries<T>(
            Dictionary<string, CatalogEntryRow> entries,
            Dictionary<string, T> targetLookup,
            string tableName,
            List<string> errors)
        {
            if (entries.Count == 0)
            {
                errors.Add($"{tableName} has no rows.");
                return;
            }

            foreach (var entry in entries.Values)
            {
                if (!targetLookup.ContainsKey(entry.RefName))
                {
                    errors.Add($"{tableName} entry '{entry.Name}' references unknown Name '{entry.RefName}'.");
                }
            }
        }

        internal static void ValidateExpectedSlots(
            string monsterName,
            HashSet<SkillSlot> slots,
            SkillSlot first,
            SkillSlot last,
            string kindLabel,
            List<string> errors)
        {
            for (var slot = first; slot <= last; slot++)
            {
                if (!slots.Contains(slot))
                {
                    errors.Add($"Monster '{monsterName}' is missing {kindLabel} slot '{slot}'.");
                }
            }
        }

        internal static void ValidateEnemyRows(SourceModel model, List<string> errors)
        {
            var referencedActiveSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencedPassiveNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stageSortKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var enemy in model.Enemies.Values)
            {
                if (!string.Equals(enemy.StageName, "stage_one", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(enemy.StageName, "stage_two", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Enemy '{enemy.Name}' has unsupported stage_name '{enemy.StageName}'.");
                }

                if (enemy.SortOrder < 0)
                {
                    errors.Add($"Enemy '{enemy.Name}' has negative sort_order '{enemy.SortOrder}'.");
                }
                else if (!stageSortKeys.Add(enemy.StageName + ":" + enemy.SortOrder))
                {
                    errors.Add($"Enemy stage '{enemy.StageName}' has duplicate sort_order '{enemy.SortOrder}'.");
                }

                if (enemy.NexusDamage <= 0f)
                {
                    errors.Add($"Enemy '{enemy.Name}' requires positive nexus_damage.");
                }

                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotAName, SkillSlot.A, referencedActiveSkillNames, errors);
                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotBName, SkillSlot.B, referencedActiveSkillNames, errors);
                ValidateEnemyPassive(model, enemy, referencedPassiveNames, errors);
            }

            foreach (var baseSkill in model.EnemyBaseSkills.Values)
            {
                if (baseSkill == null || baseSkill.Skill == null)
                {
                    continue;
                }

                if (baseSkill.Skill.SkillKind == PakuriCsvSkillKind.Passive)
                {
                    if (!referencedPassiveNames.Contains(baseSkill.Skill.Name))
                    {
                        errors.Add($"Enemy passive skill '{baseSkill.Skill.Name}' is not referenced by enemies.csv passive_name.");
                    }
                }
                else if (!referencedActiveSkillNames.Contains(baseSkill.Skill.Name))
                {
                    errors.Add($"Enemy base skill '{baseSkill.Skill.Name}' is not referenced by an Enemy A/B skill slot.");
                }
            }

            foreach (var trigger in model.EnemyTriggers.Values)
            {
                if (!model.EnemyBaseSkills.TryGetValue(trigger.SourceSkillName, out var sourceSkill)
                    || sourceSkill == null
                    || sourceSkill.Skill == null)
                {
                    errors.Add($"Enemy trigger '{trigger.Name}' references unknown source skill '{trigger.SourceSkillName}'.");
                }
                if (!model.EnemyBaseSkills.ContainsKey(trigger.TriggeredSkillName))
                {
                    errors.Add($"Enemy trigger '{trigger.Name}' references unknown triggered skill '{trigger.TriggeredSkillName}'.");
                }
            }

            ValidateEnemyCombatStartTrigger(model, "OpeningCharge", SkillRuntimeKind.Buff, errors);
            ValidateEnemyCombatStartTrigger(model, "Intimidation", SkillRuntimeKind.Buff, errors);
        }

        internal static void ValidateEnemySkillSlot(
            SourceModel model,
            EnemyRow enemy,
            string skillName,
            SkillSlot slot,
            HashSet<string> referencedSkillNames,
            List<string> errors)
        {
            if (!model.EnemyBaseSkills.TryGetValue(skillName, out var skill)
                || skill == null
                || skill.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Name}' slot '{slot}' references unknown base skill '{skillName}'.");
                return;
            }

            if (skill.Skill.SkillKind != PakuriCsvSkillKind.Active
                || skill.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                errors.Add($"Enemy '{enemy.Name}' slot '{slot}' must reference an active skill, but '{skillName}' is passive.");
                return;
            }

            referencedSkillNames.Add(skillName);
        }

        internal static void ValidateEnemyPassive(
            SourceModel model,
            EnemyRow enemy,
            HashSet<string> referencedPassiveNames,
            List<string> errors)
        {
            var passiveName = string.Empty;
            if (enemy.PassiveName != null)
            {
                passiveName = enemy.PassiveName.Trim();
            }
            if (!model.EnemyBaseSkills.TryGetValue(passiveName, out var passive)
                || passive == null
                || passive.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Name}' references unknown passive_name '{passiveName}'.");
                return;
            }

            if (passive.Skill.SkillKind != PakuriCsvSkillKind.Passive
                || passive.Skill.RuntimeKind != SkillRuntimeKind.Passive
                || passive.Skill.Slot != SkillSlot.F)
            {
                errors.Add($"Enemy '{enemy.Name}' passive_name '{passiveName}' must reference an Enemy passive definition.");
            }

            if (passive.PassiveModifierKind == PassiveModifierKind.None)
            {
                errors.Add($"Enemy passive '{passiveName}' requires a supported modifier_kind.");
            }

            if (passive.PassiveModifierKind == PassiveModifierKind.DamageUp
                && !passive.PassiveHasAttribute)
            {
                errors.Add($"Enemy passive '{passiveName}' requires attribute for DamageUp.");
            }

            if (passive.PassiveModifierKind != PassiveModifierKind.DamageUp
                && passive.PassiveModifierKind != PassiveModifierKind.DefenseUp
                && passive.PassiveHasAttribute)
            {
                errors.Add($"Enemy passive '{passiveName}' cannot use attribute with '{passive.PassiveModifierKind}'.");
            }

            if (passive.PassiveModifierValue <= 0f)
            {
                errors.Add($"Enemy passive '{passiveName}' requires a positive modifier_value.");
            }

            referencedPassiveNames.Add(passiveName);
        }

        internal static void ValidateEnemyCombatStartTrigger(
            SourceModel model,
            string skillName,
            SkillRuntimeKind runtimeKind,
            List<string> errors)
        {
            var count = 0;
            foreach (var trigger in model.EnemyTriggers.Values)
            {
                if (trigger.Enabled
                    && trigger.TriggerEvent == SkillTriggerEvent.CombatStart
                    && string.Equals(trigger.SourceSkillName, skillName, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(trigger.TriggeredSkillName, skillName, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errors.Add($"Enemy skill '{skillName}' requires exactly one enabled CombatStart trigger; found '{count}'.");
            }
        }

    }
}
