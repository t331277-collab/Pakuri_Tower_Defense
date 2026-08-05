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
                if (!model.Monsters.ContainsKey(reward.MonsterId))
                {
                    errors.Add($"Reward choice '{reward.Id}' references unknown monster '{reward.MonsterId}'.");
                }

                 if (!model.SkillChoices.TryGetValue(reward.Id, out var rewardChoice))
                 {
                     errors.Add($"Reward choice '{reward.Id}' has no matching skill choice row with the same choice_id.");
                     continue;
                 }

                 if (!string.Equals(rewardChoice.MonsterId, reward.MonsterId, StringComparison.OrdinalIgnoreCase))
                 {
                     errors.Add(
                         $"Reward choice '{reward.Id}' monster mismatch: reward monster '{reward.MonsterId}', choice monster '{rewardChoice.MonsterId}'.");
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
                    else if (!string.Equals(rewardChoice.SkillId, reward.ActiveSkillId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Id}' active gate '{reward.ActiveSkillId}' does not match choice skill '{rewardChoice.SkillId}'.");
                    }
                    else if (rewardChoice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement)
                    {
                        errors.Add($"Reward choice '{reward.Id}' points passive choice group through active_skill_id.");
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
                    else if (!string.Equals(rewardChoice.SkillId, reward.PassiveSkillId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add(
                            $"Reward choice '{reward.Id}' passive gate '{reward.PassiveSkillId}' does not match choice skill '{rewardChoice.SkillId}'.");
                    }
                    else if (rewardChoice.ChoiceGroup != SkillChoiceGroup.PassiveEnhancement)
                    {
                        errors.Add($"Reward choice '{reward.Id}' points active choice group through passive_skill_id.");
                    }
                }

                if (string.IsNullOrWhiteSpace(reward.ActiveSkillId) && string.IsNullOrWhiteSpace(reward.PassiveSkillId))
                {
                    errors.Add($"Reward choice '{reward.Id}' must target either active_skill_id or passive_skill_id.");
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

                ValidateSkillRuntimeValues(skill, errors);
                if (!string.IsNullOrWhiteSpace(skill.TargetSelection)
                    && !Enum.TryParse<SkillTargetSelection>(skill.TargetSelection, true, out _))
                {
                    errors.Add($"Skill '{skill.Id}' has unsupported target_selection '{skill.TargetSelection}'.");
                }

                if (!string.IsNullOrWhiteSpace(skill.HitTargetCount)
                    && !IsSupportedHitTargetCount(skill.HitTargetCount))
                {
                    errors.Add($"Skill '{skill.Id}' has unsupported hit_target_count '{skill.HitTargetCount}'. Expected positive integer or global.");
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
                    errors.Add($"Skill '{enemySkill.Skill.Id}' has unsupported hit_target_count '{enemySkill.Skill.HitTargetCount}'. Expected positive integer or global.");
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

                if (skill.SkillKind == PakuriCsvSkillKind.Active
                    && (choice.ChoiceGroup == SkillChoiceGroup.PassiveEnhancement
                        || choice.ChoiceGroup == SkillChoiceGroup.PassiveBase))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses passive-only choice group on active skill '{choice.SkillId}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Passive
                    && choice.ChoiceGroup != SkillChoiceGroup.PassiveEnhancement
                    && choice.ChoiceGroup != SkillChoiceGroup.PassiveBase)
                {
                    errors.Add($"Skill choice '{choice.Id}' uses active choice group on passive skill '{choice.SkillId}'.");
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

                    if (!string.IsNullOrWhiteSpace(monster.ActiveSkillName)
                        && !string.Equals(monster.ActiveSkillName, slotA.DisplayName, StringComparison.Ordinal))
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

                    if (!string.IsNullOrWhiteSpace(monster.PassiveSkillName)
                        && !string.Equals(monster.PassiveSkillName, slotF.DisplayName, StringComparison.Ordinal))
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

            if (!string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.DeploymentRequiredTargetStatusId)))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported deployment_required_target_status_id '{skill.DeploymentRequiredTargetStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetSelectionStatusId)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.TargetSelectionStatusId)))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported target_selection_status_id '{skill.TargetSelectionStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetStatusStackStatusId)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.TargetStatusStackStatusId)))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported target_status_stack_status_id '{skill.TargetStatusStackStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.ConsumeTargetStatusId)
                && (statusEffects == null || !statusEffects.ContainsKey(skill.ConsumeTargetStatusId)))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported consume_target_status_id '{skill.ConsumeTargetStatusId}'.");
            }

            var status = skill.Status;
            var statusKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(status.StatusEffectId))
            {
                statusKey = status.StatusEffectId.Trim();
            }

            if (string.IsNullOrWhiteSpace(statusKey))
            {
                if (status.StatusChance > 0f)
                {
                    errors.Add($"Skill '{skill.Id}' has status_chance '{status.StatusChance}' but no status_effect_id.");
                }

                return;
            }

            StatusEffectRow statusDefinition = null;
            if (statusEffects == null || !statusEffects.TryGetValue(statusKey, out statusDefinition))
            {
                errors.Add($"Skill '{skill.Id}' uses status_effect_id '{statusKey}' but status_effects.csv has no matching row.");
                return;
            }

            if (!StatusValueParser.TryParseStatusKind(statusKey, out var kind))
            {
                errors.Add($"Skill '{skill.Id}' uses status_effect_id '{statusKey}' that cannot map to StatusEffectKind.");
                return;
            }

            if (statusDefinition.Classification == StatusEffectClassification.Buff)
            {
                if (string.IsNullOrWhiteSpace(targetScope)
                    && !StatusValueParser.TryParseTargetScope(status.StatusTargetScope, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_target_scope. Expected self or all_allies.");
                }

                if (!StatusValueParser.TryParseMergePolicy(status.StatusMergePolicy, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_merge_policy for buff status '{statusKey}'.");
                }
            }

            if (kind == StatusEffectKind.Shield)
            {
                if (!StatusValueParser.TryParseShieldRefreshRule(status.ShieldAmountRefreshPolicy, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported shield_amount_refresh_policy for shield status.");
                }

                if (status.StatusDurationSeconds <= 0f)
                {
                    errors.Add($"Skill '{skill.Id}' requires positive status_duration_seconds for shield status.");
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
                    trigger.SourceSkillId,
                    trigger.MonsterId);
            SkillRow sourceSkill = null;
            if (model == null
                || (!model.Monsters.ContainsKey(trigger.MonsterId)
                    && !model.Summons.ContainsKey(trigger.MonsterId)
                    && !artifactSource))
            {
                errors.Add($"Skill trigger '{trigger.Id}' references unknown monster '{trigger.MonsterId}'.");
            }

            var sourceSkillFound = model != null
                && (model.Skills.TryGetValue(trigger.SourceSkillId, out sourceSkill)
                    || model.SummonSkills.TryGetValue(trigger.SourceSkillId, out sourceSkill));
            if (!artifactSource && !sourceSkillFound)
            {
                errors.Add($"Skill trigger '{trigger.Id}' references unknown source skill '{trigger.SourceSkillId}'.");
            }
            else if (!artifactSource
                && !string.Equals(sourceSkill.MonsterId, trigger.MonsterId, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Skill trigger '{trigger.Id}' source skill '{trigger.SourceSkillId}' belongs to '{sourceSkill.MonsterId}', not '{trigger.MonsterId}'.");
            }

            if (!string.IsNullOrWhiteSpace(trigger.RequiredSourceStatusId)
                && (model == null || !model.StatusEffects.ContainsKey(trigger.RequiredSourceStatusId)))
            {
                errors.Add($"Skill trigger '{trigger.Id}' uses unsupported required_source_status_id '{trigger.RequiredSourceStatusId}'.");
            }

            var hasOwnedNodes = HasOwnedTriggerNodeSource(model, trigger.Id);
            if (!hasOwnedNodes)
            {
                errors.Add($"Skill trigger '{trigger.Id}' requires at least one owned node.");
            }

            if (trigger.RepeatCount <= 0)
            {
                errors.Add($"Skill trigger '{trigger.Id}' requires repeat_count greater than 0.");
            }

            if (trigger.RepeatIntervalSeconds < 0f)
            {
                errors.Add($"Skill trigger '{trigger.Id}' has negative repeat_interval_seconds.");
            }

            if (trigger.TriggerDelaySeconds < 0f)
            {
                errors.Add($"Skill trigger '{trigger.Id}' has negative trigger_delay_seconds.");
            }

            if (trigger.TriggerEveryCount < 0)
            {
                errors.Add($"Skill trigger '{trigger.Id}' has negative trigger_every_count.");
            }

            if (!ValidateEventSourceScope(trigger.EventSourceScope))
            {
                errors.Add($"Skill trigger '{trigger.Id}' has unsupported event_source_scope '{trigger.EventSourceScope}'. Expected owner or all_allies.");
            }

            if (trigger.ProcChance < 0f || trigger.ProcChance > 1f)
            {
                errors.Add($"Skill trigger '{trigger.Id}' has proc_chance '{trigger.ProcChance}' outside 0..1.");
            }

            if (trigger.InternalCooldownSeconds < 0f)
            {
                errors.Add($"Skill trigger '{trigger.Id}' has negative internal_cooldown_seconds.");
            }

            ValidateTriggerChoiceReference(trigger.RequiresActiveChoiceId, trigger, model, "requires_active_choice_id", errors);
            ValidateTriggerChoiceReference(trigger.ExcludesActiveChoiceId, trigger, model, "excludes_active_choice_id", errors);

            ValidateStatusConditionExpression(trigger.Id, trigger.ConditionStatusId, model, errors);

            if (!string.IsNullOrWhiteSpace(trigger.TriggerAttribute)
                && !ValidateTriggerAttributes(trigger.TriggerAttribute))
            {
                errors.Add($"Skill trigger '{trigger.Id}' uses unsupported trigger_attribute '{trigger.TriggerAttribute}'.");
            }

            if (!ValidateSkillRuntimeKindList(trigger.EventSkillRuntimeKinds))
            {
                errors.Add($"Skill trigger '{trigger.Id}' uses unsupported event_skill_runtime_kinds '{trigger.EventSkillRuntimeKinds}'.");
            }

            ValidateSkillIdList(trigger.EventSkillId, trigger, model, "event_skill_id", errors);
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

        internal static void ValidateSkillIdList(
            string rawSkillIds,
            SkillTriggerRow trigger,
            SourceModel model,
            string columnName,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(rawSkillIds))
            {
                return;
            }

            var skillIds = rawSkillIds.Split(';', ',');
            for (var i = 0; i < skillIds.Length; i++)
            {
                var skillId = string.Empty;
                if (skillIds[i] != null)
                {
                    skillId = skillIds[i].Trim();
                }
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                if (model == null
                    || (!model.Skills.ContainsKey(skillId) && !model.SummonSkills.ContainsKey(skillId)))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' {columnName} references unknown skill '{skillId}'.");
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

            var synergyLevelIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var synergy in model.ArtifactSynergies.Values)
            {
                var previousRequiredCount = 0;
                for (var i = 0; i < synergy.Levels.Length; i++)
                {
                    var level = synergy.Levels[i];
                    if (!synergyLevelIds.Add(level.Id))
                    {
                        errors.Add($"Artifact synergy level id '{level.Id}' is duplicated.");
                    }

                    if (level.RequiredCount <= previousRequiredCount)
                    {
                        errors.Add($"Artifact synergy '{synergy.Id}' level '{level.Id}' requires a count greater than the previous level.");
                    }

                    var expectedRequiredCount = (i + 1) * 2;
                    if (level.RequiredCount != expectedRequiredCount)
                    {
                        errors.Add($"Artifact synergy '{synergy.Id}' level '{level.Id}' requires count '{level.RequiredCount}', expected '{expectedRequiredCount}'.");
                    }

                    previousRequiredCount = level.RequiredCount;
                }
            }

            foreach (var artifact in model.Artifacts.Values)
            {
                if (!model.ArtifactSynergies.ContainsKey(artifact.SynergyId))
                {
                    errors.Add($"Artifact '{artifact.Id}' references unknown synergy '{artifact.SynergyId}'.");
                }
            }

            foreach (var effect in model.ArtifactEffects.Values)
            {
                if (!model.Artifacts.ContainsKey(effect.ArtifactId))
                {
                    errors.Add($"Artifact effect '{effect.Id}' references unknown artifact '{effect.ArtifactId}'.");
                }

                ValidateArtifactEffectReferences(
                    effect.Id,
                    effect.ApplicationMode,
                    effect.Recipient,
                    effect.RepeatRule,
                    effect.SelectionRule,
                    effect.RecipientMonsterId,
                    effect.TargetSkillId,
                    effect.OutcomeSkillId,
                    string.Empty,
                    model,
                    errors);
            }

            foreach (var effect in model.ArtifactSynergyEffects.Values)
            {
                if (!synergyLevelIds.Contains(effect.SynergyLevelId))
                {
                    errors.Add($"Artifact synergy effect '{effect.Id}' references unknown level '{effect.SynergyLevelId}'.");
                }

                ValidateArtifactEffectReferences(
                    effect.Id,
                    effect.ApplicationMode,
                    effect.Recipient,
                    ArtifactEffectRepeatRule.None,
                    ArtifactEffectSelectionRule.None,
                    effect.RecipientMonsterId,
                    effect.TargetSkillId,
                    effect.OutcomeSkillId,
                    effect.SpawnSummonId,
                    model,
                    errors);
            }

            foreach (var summon in model.Summons.Values)
            {
                if (model.Monsters.ContainsKey(summon.Id))
                {
                    errors.Add($"Summon '{summon.Id}' conflicts with a monster id.");
                }

                var slots = new HashSet<SkillSlot>();
                foreach (var skill in model.SummonSkills.Values)
                {
                    if (!string.Equals(skill.MonsterId, summon.Id, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!slots.Add(skill.Slot))
                    {
                        errors.Add($"Summon '{summon.Id}' has duplicate active slot '{skill.Slot}'.");
                    }
                }

                for (var slot = SkillSlot.A; slot <= SkillSlot.E; slot++)
                {
                    if (!slots.Contains(slot))
                    {
                        errors.Add($"Summon '{summon.Id}' is missing active slot '{slot}'.");
                    }
                }
            }

            foreach (var skill in model.SummonSkills.Values)
            {
                if (!model.Summons.ContainsKey(skill.MonsterId))
                {
                    errors.Add($"Summon skill '{skill.Id}' references unknown summon '{skill.MonsterId}'.");
                }

                if (model.Skills.ContainsKey(skill.Id))
                {
                    errors.Add($"Summon skill '{skill.Id}' conflicts with a monster skill id.");
                }

                ValidateSkillRuntimeValues(skill, errors);
                ValidateRuntimeStatusColumns(skill, model.StatusEffects, string.Empty, errors);
            }
        }

        private static void ValidateArtifactEffectReferences(
            string effectId,
            ArtifactEffectApplicationMode applicationMode,
            ArtifactEffectRecipient recipient,
            ArtifactEffectRepeatRule repeatRule,
            ArtifactEffectSelectionRule selectionRule,
            string recipientMonsterId,
            string targetSkillId,
            string outcomeSkillId,
            string spawnSummonId,
            SourceModel model,
            List<string> errors)
        {
            if (repeatRule != ArtifactEffectRepeatRule.None
                && (applicationMode != ArtifactEffectApplicationMode.SkillModifier
                    || recipient != ArtifactEffectRecipient.AllAllies))
            {
                errors.Add($"Artifact effect '{effectId}' repeat_rule requires SkillModifier and AllAllies.");
            }

            if (selectionRule != ArtifactEffectSelectionRule.None
                && (applicationMode != ArtifactEffectApplicationMode.SkillModifier
                    || recipient != ArtifactEffectRecipient.AllAllies))
            {
                errors.Add($"Artifact effect '{effectId}' selection_rule requires SkillModifier and AllAllies.");
            }

            if (recipient == ArtifactEffectRecipient.SpecificMonster
                && (string.IsNullOrWhiteSpace(recipientMonsterId)
                    || !model.Monsters.ContainsKey(recipientMonsterId)))
            {
                errors.Add($"Artifact effect '{effectId}' requires a known recipient_monster_id.");
            }

            if (!string.IsNullOrWhiteSpace(targetSkillId)
                && !model.Skills.ContainsKey(targetSkillId)
                && !model.SummonSkills.ContainsKey(targetSkillId))
            {
                errors.Add($"Artifact effect '{effectId}' references unknown target skill '{targetSkillId}'.");
            }

            if (!string.IsNullOrWhiteSpace(outcomeSkillId)
                && !model.Skills.ContainsKey(outcomeSkillId)
                && !model.SummonSkills.ContainsKey(outcomeSkillId))
            {
                errors.Add($"Artifact effect '{effectId}' references unknown outcome skill '{outcomeSkillId}'.");
            }

            if (applicationMode == ArtifactEffectApplicationMode.GrantSkill
                && string.IsNullOrWhiteSpace(outcomeSkillId))
            {
                errors.Add($"Artifact effect '{effectId}' requires outcome_skill_id for GrantSkill.");
            }

            if (applicationMode == ArtifactEffectApplicationMode.SpawnUnit)
            {
                if (string.IsNullOrWhiteSpace(spawnSummonId)
                    || !model.Summons.ContainsKey(spawnSummonId))
                {
                    errors.Add($"Artifact effect '{effectId}' requires a known spawn_monster_id for SpawnUnit.");
                }
            }
            else if (!string.IsNullOrWhiteSpace(spawnSummonId))
            {
                errors.Add($"Artifact effect '{effectId}' may use spawn_monster_id only with SpawnUnit.");
            }
        }

        private static void ValidateUnitRuntimeValues(SourceModel model, List<string> errors)
        {
            foreach (var monster in model.Monsters.Values)
            {
                if (monster.MaxHealth <= 0f)
                {
                    errors.Add($"Monster '{monster.Id}' requires positive max_health.");
                }
            }

            foreach (var enemy in model.Enemies.Values)
            {
                if (enemy.MaxHealth <= 0f)
                {
                    errors.Add($"Enemy '{enemy.Id}' requires positive max_health.");
                }
            }

            foreach (var summon in model.Summons.Values)
            {
                if (summon.MaxHealth <= 0f)
                {
                    errors.Add($"Summon '{summon.Id}' requires positive max_health.");
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
                    errors.Add($"Enemy skill '{skill.Skill.Id}' requires positive cast_range for hostile targeting.");
                }
            }
        }

        internal static void ValidateSkillRuntimeValues(SkillRow skill, List<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(skill.RuntimeVisualAnchor)
                && !Enum.TryParse<RuntimeSkillVisualAnchor>(skill.RuntimeVisualAnchor, true, out _))
            {
                errors.Add($"Skill '{skill.Id}' has unsupported runtime_visual_anchor '{skill.RuntimeVisualAnchor}'.");
            }

            if (skill.RuntimeVisualScale <= 0f || skill.RuntimeImpactVisualScale <= 0f)
            {
                errors.Add($"Skill '{skill.Id}' requires positive runtime visual scale values.");
            }

            if (skill.RuntimeHitboxSizeX < 0f || skill.RuntimeHitboxSizeY < 0f)
            {
                errors.Add($"Skill '{skill.Id}' has a negative runtime hitbox size.");
            }

            if (skill.SkillKind == PakuriCsvSkillKind.Active && skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                errors.Add($"Active skill '{skill.Id}' cannot use Passive runtime_kind.");
            }

            if (skill.SkillKind == PakuriCsvSkillKind.Passive && skill.RuntimeKind != SkillRuntimeKind.Passive)
            {
                errors.Add($"Passive skill '{skill.Id}' must use Passive runtime_kind.");
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
                errors.Add($"Skill '{skill.Id}' contains a negative runtime value or a status chance outside 0..1.");
            }

            if (skill.SkillKind == PakuriCsvSkillKind.Passive)
            {
                return;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.MagazineProjectile)
            {
                if (skill.MagazineCapacity <= 0)
                {
                    errors.Add($"Magazine projectile '{skill.Id}' requires positive magazine_capacity.");
                }

                if (skill.ReloadSeconds <= 0f)
                {
                    errors.Add($"Magazine projectile '{skill.Id}' requires positive reload_seconds.");
                }

                if (skill.ShotIntervalSeconds <= 0f)
                {
                    errors.Add($"Magazine projectile '{skill.Id}' requires positive shot_interval_seconds.");
                }

                if (skill.ProjectileSpeed <= 0f)
                {
                    errors.Add($"Projectile skill '{skill.Id}' requires positive projectile_speed.");
                }

                return;
            }

            if (skill.RuntimeKind == SkillRuntimeKind.CooldownProjectile)
            {
                if (skill.ProjectileSpeed <= 0f)
                {
                    errors.Add($"Projectile skill '{skill.Id}' requires positive projectile_speed.");
                }

                return;
            }

            if (skill.CooldownSeconds <= 0f)
            {
                errors.Add($"Active skill '{skill.Id}' requires positive cooldown_seconds.");
            }
        }

        internal static bool HasOwnedTriggerNodeSource(
            SourceModel model,
            string triggerId)
        {
            if (model == null || string.IsNullOrWhiteSpace(triggerId))
            {
                return false;
            }

            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                if (graph != null
                    && graph.OwnerKind == SkillNodeOwnerKind.Trigger
                    && string.Equals(graph.OwnerId, triggerId, StringComparison.OrdinalIgnoreCase))
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
            string choiceId,
            SkillTriggerRow trigger,
            SourceModel model,
            string columnName,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(choiceId))
            {
                return;
            }

            var choiceIds = choiceId.Split(';', ',');
            for (var i = 0; i < choiceIds.Length; i++)
            {
                var currentChoiceId = string.Empty;
                if (choiceIds[i] != null)
                {
                    currentChoiceId = choiceIds[i].Trim();
                }
                if (string.IsNullOrWhiteSpace(currentChoiceId))
                {
                    continue;
                }

                if (model == null || !model.SkillChoices.TryGetValue(currentChoiceId, out var choice))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' {columnName} references unknown choice '{currentChoiceId}'.");
                    continue;
                }

                if (!ChoiceAppliesToSkillId(choice, trigger.SourceSkillId))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' {columnName} choice '{currentChoiceId}' does not apply to source skill '{trigger.SourceSkillId}'.");
                }
            }
        }

        internal static bool ChoiceAppliesToSkillId(SkillChoiceRow choice, string skillId)
        {
            if (choice == null || string.IsNullOrWhiteSpace(skillId))
            {
                return false;
            }

            if (string.Equals(choice.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(choice.TargetSkillId, skillId, StringComparison.OrdinalIgnoreCase);
        }

        internal static void ValidateStatusIdList(
            string ownerId,
            string rawValue,
            SourceModel model,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                errors.Add($"Skill node '{ownerId}' requires status_ids.");
                return;
            }

            var statusIds = rawValue.Split(';');
            for (var i = 0; i < statusIds.Length; i++)
            {
                var statusId = statusIds[i].Trim();
                if (string.IsNullOrWhiteSpace(statusId)
                    || model == null
                    || !model.StatusEffects.ContainsKey(statusId))
                {
                    errors.Add($"Skill node '{ownerId}' references unknown status '{statusId}' in status_ids.");
                }
            }
        }

        private static void ValidateStatusConditionExpression(
            string ownerId,
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
                errors.Add($"Skill trigger '{ownerId}' uses unsupported condition_status_id '{rawValue}'.");
                return;
            }

            var groups = rawValue.Split(';', ',');
            for (var groupIndex = 0; groupIndex < groups.Length; groupIndex++)
            {
                var requirements = groups[groupIndex].Split('&');
                for (var requirementIndex = 0; requirementIndex < requirements.Length; requirementIndex++)
                {
                    var requirement = requirements[requirementIndex].Trim();
                    var statusId = requirement;
                    var separatorIndex = requirement.IndexOf(">=", StringComparison.Ordinal);
                    if (separatorIndex < 0)
                    {
                        separatorIndex = requirement.IndexOf(':');
                    }

                    if (separatorIndex >= 0)
                    {
                        statusId = requirement.Substring(0, separatorIndex).Trim();
                    }

                    if (model == null || !model.StatusEffects.ContainsKey(statusId))
                    {
                        errors.Add($"Skill trigger '{ownerId}' references unknown status '{statusId}' in condition_status_id.");
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

            if (!StatusValueParser.TryParseStatusKind(status.Id, out var kind) || kind == StatusEffectKind.None)
            {
                errors.Add($"Status effect '{status.Id}' is not supported by StatusEffectKind.");
            }

            if (kind == StatusEffectKind.Shield
                && !string.Equals(status.Id, "shield", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Shield status row '{status.Id}' must use canonical id 'shield'.");
            }

            if (status.BaseStackAmount <= 0)
            {
                errors.Add($"Status effect '{status.Id}' requires base_stack_amount greater than 0.");
            }

            if (!status.IsPermanent && status.DefaultDurationSeconds < 0f)
            {
                errors.Add($"Status effect '{status.Id}' has negative default_duration_seconds.");
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
                if (!model.SkillNodeTypes.TryGetValue(graph.NodeTypeId, out var nodeType))
                {
                    continue;
                }
                if (GameDataCatalogBuilder.CanProcessNode(graph.OwnerKind.ToString(), nodeType.HandlerId))
                {
                    continue;
                }

                errors.Add(
                    $"Skill graph '{BuildSkillGraphKey(graph)}' node '{graph.NodeOrder}' uses handler '{nodeType.HandlerId}' without a combat conversion route.");
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
                    || node.OwnerKind != SkillNodeOwnerKind.Trigger
                    || !GameDataCatalogBuilder.IsTriggerOutcomeHandler(node.HandlerId))
                {
                    continue;
                }

                counts.TryGetValue(node.OwnerId, out var count);
                counts[node.OwnerId] = count + 1;
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
                if (!targetLookup.ContainsKey(entry.RefId))
                {
                    errors.Add($"{tableName} entry '{entry.Id}' references unknown id '{entry.RefId}'.");
                }
            }
        }

        internal static void ValidateExpectedSlots(
            string monsterId,
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
                    errors.Add($"Monster '{monsterId}' is missing {kindLabel} slot '{slot}'.");
                }
            }
        }

        internal static void ValidateEnemyRows(SourceModel model, List<string> errors)
        {
            var referencedActiveSkillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var referencedPassiveIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var stageSortKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var enemy in model.Enemies.Values)
            {
                if (!string.Equals(enemy.StageId, "stage_one", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(enemy.StageId, "stage_two", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Enemy '{enemy.Id}' has unsupported stage_id '{enemy.StageId}'.");
                }

                if (enemy.SortOrder < 0)
                {
                    errors.Add($"Enemy '{enemy.Id}' has negative sort_order '{enemy.SortOrder}'.");
                }
                else if (!stageSortKeys.Add(enemy.StageId + ":" + enemy.SortOrder))
                {
                    errors.Add($"Enemy stage '{enemy.StageId}' has duplicate sort_order '{enemy.SortOrder}'.");
                }

                if (enemy.NexusDamage <= 0f)
                {
                    errors.Add($"Enemy '{enemy.Id}' requires positive nexus_damage.");
                }

                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotAId, SkillSlot.A, referencedActiveSkillIds, errors);
                ValidateEnemySkillSlot(model, enemy, enemy.SkillSlotBId, SkillSlot.B, referencedActiveSkillIds, errors);
                ValidateEnemyPassive(model, enemy, referencedPassiveIds, errors);
            }

            foreach (var baseSkill in model.EnemyBaseSkills.Values)
            {
                if (baseSkill == null || baseSkill.Skill == null)
                {
                    continue;
                }

                if (baseSkill.Skill.SkillKind == PakuriCsvSkillKind.Passive)
                {
                    if (!referencedPassiveIds.Contains(baseSkill.Skill.Id))
                    {
                        errors.Add($"Enemy passive skill '{baseSkill.Skill.Id}' is not referenced by enemies.csv passive_id.");
                    }
                }
                else if (!referencedActiveSkillIds.Contains(baseSkill.Skill.Id))
                {
                    errors.Add($"Enemy base skill '{baseSkill.Skill.Id}' is not referenced by an Enemy A/B skill slot.");
                }
            }

            foreach (var trigger in model.EnemyTriggers.Values)
            {
                if (!model.EnemyBaseSkills.TryGetValue(trigger.SourceSkillId, out var sourceSkill)
                    || sourceSkill == null
                    || sourceSkill.Skill == null)
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown source skill '{trigger.SourceSkillId}'.");
                }
                if (!model.EnemyBaseSkills.ContainsKey(trigger.TriggeredSkillId))
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown triggered skill '{trigger.TriggeredSkillId}'.");
                }
            }

            ValidateEnemyCombatStartTrigger(model, "OpeningCharge", SkillRuntimeKind.Buff, errors);
            ValidateEnemyCombatStartTrigger(model, "Intimidation", SkillRuntimeKind.Buff, errors);
        }

        internal static void ValidateEnemySkillSlot(
            SourceModel model,
            EnemyRow enemy,
            string skillId,
            SkillSlot slot,
            HashSet<string> referencedSkillIds,
            List<string> errors)
        {
            if (!model.EnemyBaseSkills.TryGetValue(skillId, out var skill)
                || skill == null
                || skill.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Id}' slot '{slot}' references unknown base skill '{skillId}'.");
                return;
            }

            if (skill.Skill.SkillKind != PakuriCsvSkillKind.Active
                || skill.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                errors.Add($"Enemy '{enemy.Id}' slot '{slot}' must reference an active skill, but '{skillId}' is passive.");
                return;
            }

            referencedSkillIds.Add(skillId);
        }

        internal static void ValidateEnemyPassive(
            SourceModel model,
            EnemyRow enemy,
            HashSet<string> referencedPassiveIds,
            List<string> errors)
        {
            var passiveId = string.Empty;
            if (enemy.PassiveId != null)
            {
                passiveId = enemy.PassiveId.Trim();
            }
            if (!model.EnemyBaseSkills.TryGetValue(passiveId, out var passive)
                || passive == null
                || passive.Skill == null)
            {
                errors.Add($"Enemy '{enemy.Id}' references unknown passive_id '{passiveId}'.");
                return;
            }

            if (passive.Skill.SkillKind != PakuriCsvSkillKind.Passive
                || passive.Skill.RuntimeKind != SkillRuntimeKind.Passive
                || passive.Skill.Slot != SkillSlot.F)
            {
                errors.Add($"Enemy '{enemy.Id}' passive_id '{passiveId}' must reference an Enemy passive definition.");
            }

            if (passive.PassiveModifierKind == PassiveModifierKind.None)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a supported modifier_kind.");
            }

            if (passive.PassiveModifierKind == PassiveModifierKind.DamageUp
                && !passive.PassiveHasAttribute)
            {
                errors.Add($"Enemy passive '{passiveId}' requires attribute for DamageUp.");
            }

            if (passive.PassiveModifierKind != PassiveModifierKind.DamageUp
                && passive.PassiveModifierKind != PassiveModifierKind.DefenseUp
                && passive.PassiveHasAttribute)
            {
                errors.Add($"Enemy passive '{passiveId}' cannot use attribute with '{passive.PassiveModifierKind}'.");
            }

            if (passive.PassiveModifierValue <= 0f)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a positive modifier_value.");
            }

            referencedPassiveIds.Add(passiveId);
        }

        internal static void ValidateEnemyCombatStartTrigger(
            SourceModel model,
            string skillId,
            SkillRuntimeKind runtimeKind,
            List<string> errors)
        {
            var count = 0;
            foreach (var trigger in model.EnemyTriggers.Values)
            {
                if (trigger.Enabled
                    && trigger.TriggerEvent == SkillTriggerEvent.CombatStart
                    && string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(trigger.TriggeredSkillId, skillId, StringComparison.OrdinalIgnoreCase))
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errors.Add($"Enemy skill '{skillId}' requires exactly one enabled CombatStart trigger; found '{count}'.");
            }
        }

    }
}
