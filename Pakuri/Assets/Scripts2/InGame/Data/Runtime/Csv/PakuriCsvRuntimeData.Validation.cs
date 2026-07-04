using System;
using System.Collections.Generic;
using Pakuri.Combat;
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

            if (model.StageTwoEnemies.Count == 0)
            {
                errors.Add("stage_two_enemies.csv has no enemy rows.");
            }

            if (model.EnemySkills.Count == 0)
            {
                errors.Add("EnemySkillData.csv has no enemy skill rows.");
            }

            if (model.StatusEffects.Count == 0)
            {
                errors.Add("status_effects.csv has no status rows.");
            }

            ValidateCatalogEntries(model.CatalogMonsters, model.Monsters, "catalog_monsters.csv", errors);
            ValidateCatalogEntries(model.CatalogStageOneEnemies, model.StageOneEnemies, "catalog_stage_one_enemies.csv", errors);
            ValidateCatalogEntries(model.CatalogStageTwoEnemies, model.StageTwoEnemies, "catalog_stage_two_enemies.csv", errors);

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
                    else if (rewardChoice.ChoiceGroup == PakuriCsvChoiceGroup.PassiveEnhancement)
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
                    else if (rewardChoice.ChoiceGroup != PakuriCsvChoiceGroup.PassiveEnhancement)
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

                ValidateRuntimeStatusColumns(skill, model.StatusEffects, errors);
            }

            foreach (var effect in model.SkillEffects.Values)
            {
                ValidateSkillEffectRow(effect, model, errors);
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
                    && (choice.ChoiceGroup == PakuriCsvChoiceGroup.PassiveEnhancement
                        || choice.ChoiceGroup == PakuriCsvChoiceGroup.PassiveBase))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses passive-only choice group on active skill '{choice.SkillId}'.");
                }

                if (skill.SkillKind == PakuriCsvSkillKind.Passive
                    && choice.ChoiceGroup != PakuriCsvChoiceGroup.PassiveEnhancement
                    && choice.ChoiceGroup != PakuriCsvChoiceGroup.PassiveBase)
                {
                    errors.Add($"Skill choice '{choice.Id}' uses active choice group on passive skill '{choice.SkillId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.CountStatusId)
                    && !StatusEffectUtility.TryParse(choice.CountStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported count_status_id '{choice.CountStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.StatusConditionalSourceStatusId)
                    && !StatusEffectUtility.TryParse(choice.StatusConditionalSourceStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported status_conditional_source_status_id '{choice.StatusConditionalSourceStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.StatusDurationBonusStatusId)
                    && !StatusEffectUtility.TryParse(choice.StatusDurationBonusStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported status_duration_bonus_status_id '{choice.StatusDurationBonusStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.StatusMaxStacksBonusStatusId)
                    && !StatusEffectUtility.TryParse(choice.StatusMaxStacksBonusStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported status_max_stacks_bonus_status_id '{choice.StatusMaxStacksBonusStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ThresholdStatusId)
                    && !StatusEffectUtility.TryParse(choice.ThresholdStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported threshold_status_id '{choice.ThresholdStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ThresholdApplyStatusId)
                    && !StatusEffectUtility.TryParse(choice.ThresholdApplyStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported threshold_apply_status_id '{choice.ThresholdApplyStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ConditionalTargetStatusId)
                    && !StatusEffectUtility.TryParse(choice.ConditionalTargetStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported conditional_target_status_id '{choice.ConditionalTargetStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ConditionalCritTargetStatusId)
                    && !StatusEffectUtility.TryParse(choice.ConditionalCritTargetStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported conditional_crit_target_status_id '{choice.ConditionalCritTargetStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.RedistributeConsumedStatusId)
                    && !StatusEffectUtility.TryParse(choice.RedistributeConsumedStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported redistribute_consumed_status_id '{choice.RedistributeConsumedStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.RequiredSourceStatusId)
                    && !StatusEffectUtility.TryParse(choice.RequiredSourceStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported required_source_status_id '{choice.RequiredSourceStatusId}'.");
                }

                if (choice.HasBurstStatusProjectileIndex && choice.BurstStatusStacksBonus <= 0)
                {
                    errors.Add($"Skill choice '{choice.Id}' requires positive burst_status_stacks_bonus when burst_status_projectile_index is set.");
                }

                if (!ChoiceTargetsKnownSkills(choice, model, out var unknownRuntimeTargetSkillId))
                {
                    errors.Add($"Skill choice '{choice.Id}' references unknown runtime target skill '{unknownRuntimeTargetSkillId}'.");
                }
                else if (!ChoiceTargetsOnlyMonsterSkills(choice, model, out var foreignRuntimeTargetSkillId))
                {
                    errors.Add($"Skill choice '{choice.Id}' runtime target skill '{foreignRuntimeTargetSkillId}' belongs to another monster.");
                }
            }

            ValidateNormalizedSkillAuthoringRows(model, assetCatalog, errors);

            ValidateEnemyRows(model.StageOneEnemies.Values, model.EnemySkills, errors);
            ValidateEnemyRows(model.StageTwoEnemies.Values, model.EnemySkills, errors);
            ValidateEnemySkillNodes(model, errors);

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

        private static void ValidateRuntimeStatusColumns(
            SkillRow skill,
            Dictionary<string, StatusEffectRow> statusEffects,
            List<string> errors)
        {
            if (skill == null)
            {
                return;
            }

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

            if (!string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId)
                && !StatusEffectUtility.TryParse(skill.DeploymentRequiredTargetStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported deployment_required_target_status_id '{skill.DeploymentRequiredTargetStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetSelectionStatusId)
                && !StatusEffectUtility.TryParse(skill.TargetSelectionStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported target_selection_status_id '{skill.TargetSelectionStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetStatusStackStatusId)
                && !StatusEffectUtility.TryParse(skill.TargetStatusStackStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported target_status_stack_status_id '{skill.TargetStatusStackStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.ConsumeTargetStatusId)
                && !StatusEffectUtility.TryParse(skill.ConsumeTargetStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported consume_target_status_id '{skill.ConsumeTargetStatusId}'.");
            }

            var status = skill.Status;
            var statusKey = !string.IsNullOrWhiteSpace(status.StatusEffectId)
                ? status.StatusEffectId.Trim()
                : status.StatusEffectLabel != null ? status.StatusEffectLabel.Trim() : string.Empty;
            var hasStatusKey = !string.IsNullOrWhiteSpace(statusKey)
                && !string.Equals(statusKey, "none", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(statusKey, "없음", StringComparison.OrdinalIgnoreCase);
            if (!hasStatusKey)
            {
                if (status.StatusChance > 0f)
                {
                    errors.Add($"Skill '{skill.Id}' has status_chance '{status.StatusChance}' but no runtime status id or parseable label.");
                }

                return;
            }

            var hasSupportedStatus = StatusEffectUtility.TryParse(statusKey, out var kind);
            if (status.StatusChance > 0f && !hasSupportedStatus)
            {
                errors.Add(
                    $"Skill '{skill.Id}' uses unsupported runtime status '{statusKey}'. Add it to StatusEffectKind or set status_chance to 0 for design-only labels.");
            }

            if (status.StatusChance > 0f && hasSupportedStatus)
            {
                var statusId = StatusEffectUtility.ToId(kind);
                if (!string.IsNullOrWhiteSpace(statusId)
                    && (statusEffects == null || !statusEffects.ContainsKey(statusId)))
                {
                    errors.Add($"Skill '{skill.Id}' uses status '{statusId}' but status_effects.csv has no matching row.");
                }
            }

            if (skill.RuntimeKind == SkillRuntimeKind.Buff || skill.RuntimeKind == SkillRuntimeKind.Shield)
            {
                if (!StatusEffectRuntime.TryParseStatusTargetScope(status.StatusTargetScope, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_target_scope for {skill.RuntimeKind}. Expected self or all_allies.");
                }

                if (!StatusEffectRuntime.TryParseStatusMergePolicy(status.StatusMergePolicy, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_merge_policy for {skill.RuntimeKind}.");
                }
            }

            if (skill.RuntimeKind == SkillRuntimeKind.Shield)
            {
                if (!string.Equals(status.StatusEffectId, "shield", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Shield skill '{skill.Id}' must use canonical status_effect_id 'shield'.");
                }

                if (!StatusEffectRuntime.TryParseShieldRefreshPolicy(status.ShieldAmountRefreshPolicy, out _))
                {
                    errors.Add($"Shield skill '{skill.Id}' requires supported shield_amount_refresh_policy.");
                }

                if (status.StatusDurationSeconds <= 0f)
                {
                    errors.Add($"Shield skill '{skill.Id}' requires positive status_duration_seconds.");
                }
            }
        }

        private static void ValidateSkillEffectRow(
            SkillEffectRow effect,
            SourceModel model,
            List<string> errors)
        {
            if (effect == null)
            {
                return;
            }

            if (model == null || !model.Skills.ContainsKey(effect.SkillId))
            {
                errors.Add($"Skill effect '{effect.Id}' references unknown skill '{effect.SkillId}'.");
            }

            if (string.Equals(effect.RuntimeSupportState, "MigratedToEffectBinding", StringComparison.OrdinalIgnoreCase)
                && (!string.IsNullOrWhiteSpace(effect.RequiresActiveChoiceId)
                    || !string.IsNullOrWhiteSpace(effect.RequiresPassiveSkillId)))
            {
                errors.Add($"Skill effect '{effect.Id}' is MigratedToEffectBinding but still has executable choice/passive gates.");
            }

            ValidateChoiceReference(effect.RequiresActiveChoiceId, effect, model, "requires_active_choice_id", errors);
            ValidateChoiceReference(effect.ExcludesActiveChoiceId, effect, model, "excludes_active_choice_id", errors);
            ValidatePassiveReference(effect.RequiresPassiveSkillId, effect, model, "requires_passive_skill_id", errors);
            ValidatePassiveReference(effect.ExcludesPassiveSkillId, effect, model, "excludes_passive_skill_id", errors);
            if (!string.IsNullOrWhiteSpace(effect.RequiredSourceStatusId)
                && !StatusEffectUtility.TryParse(effect.RequiredSourceStatusId, out _))
            {
                errors.Add($"Skill effect '{effect.Id}' uses unsupported required_source_status_id '{effect.RequiredSourceStatusId}'.");
            }

            var status = effect.Status;
            var hasStatus = !string.IsNullOrWhiteSpace(status.StatusEffectId)
                || !string.IsNullOrWhiteSpace(status.StatusEffectLabel);
            var hasPositiveDamagePayload = HasPositiveDamagePayload(
                effect.BaseDamage,
                effect.AttackPowerCoefficient,
                effect.SpellPowerCoefficient);
            var isStatusOnlyPersistentZone = effect.EffectKind == SkillMultiEffectKind.Damage
                && effect.BaseDamage <= 0f
                && effect.AttackPowerCoefficient <= 0f
                && effect.SpellPowerCoefficient <= 0f
                && effect.ActiveDurationSeconds > 0f
                && effect.TickIntervalSeconds > 0f
                && hasStatus;
            if (effect.EffectKind == SkillMultiEffectKind.Damage
                && !hasPositiveDamagePayload
                && !isStatusOnlyPersistentZone)
            {
                errors.Add($"Damage skill effect '{effect.Id}' requires positive base_damage or positive attack/spell coefficient.");
            }

            if (effect.EffectKind == SkillMultiEffectKind.Status && !hasStatus)
            {
                errors.Add($"Status skill effect '{effect.Id}' requires status_effect_id or status_effect_label.");
            }

            if (hasStatus)
            {
                var statusKey = !string.IsNullOrWhiteSpace(status.StatusEffectId)
                    ? status.StatusEffectId
                    : status.StatusEffectLabel;
                if (!StatusEffectUtility.TryParse(statusKey, out var kind))
                {
                    errors.Add($"Skill effect '{effect.Id}' uses unsupported runtime status '{statusKey}'.");
                }
                else
                {
                    var statusId = StatusEffectUtility.ToId(kind);
                    if (!string.IsNullOrWhiteSpace(statusId)
                        && (model == null || !model.StatusEffects.ContainsKey(statusId)))
                    {
                        errors.Add($"Skill effect '{effect.Id}' uses status '{statusId}' but status_effects.csv has no matching row.");
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(effect.ConditionStatusId)
                && !StatusEffectRuntime.TryParseConditionStatusExpression(effect.ConditionStatusId, out _))
            {
                errors.Add($"Skill effect '{effect.Id}' uses unsupported condition_status_id '{effect.ConditionStatusId}'.");
            }

            if (effect.ConditionHealthRatioMax < 0f || effect.ConditionHealthRatioMax > 1f)
            {
                errors.Add($"Skill effect '{effect.Id}' has condition_health_ratio_max '{effect.ConditionHealthRatioMax}' outside 0..1.");
            }

            if (effect.ConditionHitCountMin < 0)
            {
                errors.Add($"Skill effect '{effect.Id}' has negative condition_hit_count_min.");
            }

            if (!string.IsNullOrWhiteSpace(status.StatusTargetScope)
                && !StatusEffectRuntime.TryParseStatusTargetScope(status.StatusTargetScope, out _))
            {
                errors.Add($"Skill effect '{effect.Id}' has unsupported status_target_scope '{status.StatusTargetScope}'.");
            }

            if (!string.IsNullOrWhiteSpace(status.StatusMergePolicy)
                && !StatusEffectRuntime.TryParseStatusMergePolicy(status.StatusMergePolicy, out _))
            {
                errors.Add($"Skill effect '{effect.Id}' has unsupported status_merge_policy '{status.StatusMergePolicy}'.");
            }

            if (!ValidateSkillRuntimeKindList(status.StatusConditionalIncomingSkillRuntimeKinds))
            {
                errors.Add($"Skill effect '{effect.Id}' uses unsupported status_conditional_incoming_skill_runtime_kinds '{status.StatusConditionalIncomingSkillRuntimeKinds}'.");
            }

            if (!ValidateSkillRuntimeKindList(status.StatusConditionalOutgoingSkillRuntimeKinds))
            {
                errors.Add($"Skill effect '{effect.Id}' uses unsupported status_conditional_outgoing_skill_runtime_kinds '{status.StatusConditionalOutgoingSkillRuntimeKinds}'.");
            }
        }

        private static void ValidateSkillTriggerRow(
            SkillTriggerRow trigger,
            SourceModel model,
            List<string> errors)
        {
            if (trigger == null)
            {
                return;
            }

            if (model == null || !model.Monsters.ContainsKey(trigger.MonsterId))
            {
                errors.Add($"Skill trigger '{trigger.Id}' references unknown monster '{trigger.MonsterId}'.");
            }

            if (model == null || !model.Skills.TryGetValue(trigger.SourceSkillId, out var sourceSkill))
            {
                errors.Add($"Skill trigger '{trigger.Id}' references unknown source skill '{trigger.SourceSkillId}'.");
            }
            else if (!string.Equals(sourceSkill.MonsterId, trigger.MonsterId, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"Skill trigger '{trigger.Id}' source skill '{trigger.SourceSkillId}' belongs to '{sourceSkill.MonsterId}', not '{trigger.MonsterId}'.");
            }

            if (!string.IsNullOrWhiteSpace(trigger.RequiredSourceStatusId)
                && !StatusEffectUtility.TryParse(trigger.RequiredSourceStatusId, out _))
            {
                errors.Add($"Skill trigger '{trigger.Id}' uses unsupported required_source_status_id '{trigger.RequiredSourceStatusId}'.");
            }

            var triggerAction = trigger.TriggerAction != SkillTriggerActionKind.Auto
                ? trigger.TriggerAction
                : trigger.RuntimeKind == SkillRuntimeKind.SingleAttack
                    ? SkillTriggerActionKind.SingleAttack
                    : SkillTriggerActionKind.TriggeredSkill;

            if (triggerAction == SkillTriggerActionKind.TriggeredSkill)
            {
                if (model == null || !model.Skills.TryGetValue(trigger.TriggeredSkillId, out var triggeredSkill))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' references unknown triggered skill '{trigger.TriggeredSkillId}'.");
                }
                else
                {
                    if (!string.Equals(triggeredSkill.MonsterId, trigger.MonsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"Skill trigger '{trigger.Id}' triggered skill '{trigger.TriggeredSkillId}' belongs to '{triggeredSkill.MonsterId}', not '{trigger.MonsterId}'.");
                    }

                    if (triggeredSkill.RuntimeKind != trigger.RuntimeKind)
                    {
                        errors.Add($"Skill trigger '{trigger.Id}' runtime_kind '{trigger.RuntimeKind}' does not match triggered skill '{trigger.TriggeredSkillId}' runtime_kind '{triggeredSkill.RuntimeKind}'.");
                    }
                }
            }

            if (triggerAction == SkillTriggerActionKind.Effect)
            {
                if (model == null || string.IsNullOrWhiteSpace(trigger.TriggeredEffectId) || !model.SkillEffects.ContainsKey(trigger.TriggeredEffectId))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' references unknown triggered_effect_id '{trigger.TriggeredEffectId}'.");
                }
            }

            if (triggerAction == SkillTriggerActionKind.CooldownRefund || triggerAction == SkillTriggerActionKind.ReloadReduce)
            {
                var targetSkillId = !string.IsNullOrWhiteSpace(trigger.TargetSkillId)
                    ? trigger.TargetSkillId
                    : trigger.TriggeredSkillId;
                SkillRow targetSkill = null;
                var requiresExplicitTargetSkill = !string.IsNullOrWhiteSpace(targetSkillId)
                    || trigger.TargetSide != SkillMultiEffectTargetSide.AllAllies;
                if (requiresExplicitTargetSkill
                    && (model == null || string.IsNullOrWhiteSpace(targetSkillId) || !model.Skills.TryGetValue(targetSkillId, out targetSkill)))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' references unknown target skill '{targetSkillId}'.");
                }
                else if (requiresExplicitTargetSkill
                    && !string.Equals(targetSkill.MonsterId, trigger.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' target skill '{targetSkillId}' belongs to '{targetSkill.MonsterId}', not '{trigger.MonsterId}'.");
                }
            }

            if (triggerAction == SkillTriggerActionKind.CooldownRefund
                && (trigger.CooldownRefundRatio <= 0f || trigger.CooldownRefundRatio > 1f))
            {
                errors.Add($"Skill trigger '{trigger.Id}' requires cooldown_refund_ratio in 0..1 for CooldownRefund.");
            }

            if (triggerAction == SkillTriggerActionKind.ReloadReduce
                && (trigger.ReloadReduceRatio <= 0f || trigger.ReloadReduceRatio > 1f))
            {
                errors.Add($"Skill trigger '{trigger.Id}' requires reload_reduce_ratio in 0..1 for ReloadReduce.");
            }

            if (trigger.RuntimeKind == SkillRuntimeKind.Passive && triggerAction == SkillTriggerActionKind.TriggeredSkill)
            {
                errors.Add($"Skill trigger '{trigger.Id}' cannot route runtime_kind Passive.");
            }

            if (!IsSupportedHitTargetCount(trigger.HitTargetCount))
            {
                errors.Add($"Skill trigger '{trigger.Id}' has unsupported hit_target_count '{trigger.HitTargetCount}'. Expected positive integer or global.");
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

            if (triggerAction == SkillTriggerActionKind.SingleAttack
                || triggerAction == SkillTriggerActionKind.LineAttack)
            {
                if (trigger.DamageSource == SkillTriggerDamageSource.Fixed
                    && !HasPositiveDamagePayload(trigger.BaseDamage, trigger.AttackPowerCoefficient, trigger.SpellPowerCoefficient))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' uses Fixed damage_source and requires positive base_damage or positive attack/spell coefficient.");
                }

                if (trigger.DamageSource != SkillTriggerDamageSource.Fixed && trigger.DamageSourceMultiplier <= 0f)
                {
                    errors.Add($"Skill trigger '{trigger.Id}' uses {trigger.DamageSource} and requires positive damage_source_multiplier.");
                }
            }

            ValidateTriggerChoiceReference(trigger.RequiresActiveChoiceId, trigger, model, "requires_active_choice_id", errors);
            ValidateTriggerChoiceReference(trigger.ExcludesActiveChoiceId, trigger, model, "excludes_active_choice_id", errors);

            if (!string.IsNullOrWhiteSpace(trigger.ConditionStatusId)
                && !StatusEffectRuntime.TryParseConditionStatusExpression(trigger.ConditionStatusId, out _))
            {
                errors.Add($"Skill trigger '{trigger.Id}' uses unsupported condition_status_id '{trigger.ConditionStatusId}'.");
            }

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

        private static bool ValidateSkillRuntimeKindList(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var tokens = rawValue.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] != null ? tokens[i].Trim() : string.Empty;
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

        private static bool HasPositiveDamagePayload(float baseDamage, float attackPowerCoefficient, float spellPowerCoefficient)
        {
            return baseDamage > 0f
                || attackPowerCoefficient > 0f
                || spellPowerCoefficient > 0f;
        }

        private static void ValidateSkillIdList(
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
                var skillId = skillIds[i] != null ? skillIds[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                if (model == null || !model.Skills.TryGetValue(skillId, out _))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' {columnName} references unknown skill '{skillId}'.");
                }
            }
        }

        private static bool ValidateEventSourceScope(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var normalized = rawValue.Trim();
            return string.Equals(normalized, "owner", StringComparison.OrdinalIgnoreCase)
                || string.Equals(normalized, "all_allies", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ValidateTriggerAttributes(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return true;
            }

            var tokens = rawValue.Split(';', ',');
            for (var i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i] != null ? tokens[i].Trim() : string.Empty;
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

        private static bool IsSupportedHitTargetCount(string rawValue)
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

        private static void ValidateChoiceReference(
            string choiceId,
            SkillEffectRow effect,
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
                var currentChoiceId = choiceIds[i] != null ? choiceIds[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(currentChoiceId))
                {
                    continue;
                }

                if (model == null || !model.SkillChoices.TryGetValue(currentChoiceId, out var choice))
                {
                    errors.Add($"Skill effect '{effect.Id}' {columnName} references unknown choice '{currentChoiceId}'.");
                    continue;
                }

                if (!ChoiceAppliesToSkillId(choice, effect.SkillId))
                {
                    errors.Add($"Skill effect '{effect.Id}' {columnName} choice '{currentChoiceId}' does not apply to skill '{effect.SkillId}'.");
                }
            }
        }

        private static void ValidateTriggerChoiceReference(
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
                var currentChoiceId = choiceIds[i] != null ? choiceIds[i].Trim() : string.Empty;
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

        private static bool ChoiceAppliesToSkillId(SkillChoiceRow choice, string skillId)
        {
            if (choice == null || string.IsNullOrWhiteSpace(skillId))
            {
                return false;
            }

            if (MatchesDelimitedValue(choice.RuntimeTargetSkillIds, skillId))
            {
                return true;
            }

            if (string.Equals(choice.SkillId, skillId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return string.Equals(choice.TargetSkillId, skillId, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ChoiceTargetsKnownSkills(SkillChoiceRow choice, SourceModel model, out string unknownSkillId)
        {
            unknownSkillId = string.Empty;
            if (choice == null || model == null || string.IsNullOrWhiteSpace(choice.RuntimeTargetSkillIds))
            {
                return true;
            }

            var targets = choice.RuntimeTargetSkillIds.Split(';', ',');
            for (var i = 0; i < targets.Length; i++)
            {
                var skillId = targets[i] != null ? targets[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(skillId))
                {
                    continue;
                }

                if (!model.Skills.ContainsKey(skillId))
                {
                    unknownSkillId = skillId;
                    return false;
                }
            }

            return true;
        }

        private static bool ChoiceTargetsOnlyMonsterSkills(SkillChoiceRow choice, SourceModel model, out string foreignSkillId)
        {
            foreignSkillId = string.Empty;
            if (choice == null || model == null || string.IsNullOrWhiteSpace(choice.RuntimeTargetSkillIds))
            {
                return true;
            }

            var targets = choice.RuntimeTargetSkillIds.Split(';', ',');
            for (var i = 0; i < targets.Length; i++)
            {
                var skillId = targets[i] != null ? targets[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(skillId)
                    || !model.Skills.TryGetValue(skillId, out var skill))
                {
                    continue;
                }

                if (!string.Equals(skill.MonsterId, choice.MonsterId, StringComparison.OrdinalIgnoreCase))
                {
                    foreignSkillId = skillId;
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesDelimitedValue(string rawValues, string expected)
        {
            if (string.IsNullOrWhiteSpace(rawValues) || string.IsNullOrWhiteSpace(expected))
            {
                return false;
            }

            var split = rawValues.Split(';', ',');
            for (var i = 0; i < split.Length; i++)
            {
                var candidate = split[i] != null ? split[i].Trim() : string.Empty;
                if (!string.IsNullOrWhiteSpace(candidate)
                    && string.Equals(candidate, expected, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidatePassiveReference(
            string passiveSkillId,
            SkillEffectRow effect,
            SourceModel model,
            string columnName,
            List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(passiveSkillId))
            {
                return;
            }

            var passiveIds = passiveSkillId.Split(';', ',');
            for (var i = 0; i < passiveIds.Length; i++)
            {
                var currentPassiveId = passiveIds[i] != null ? passiveIds[i].Trim() : string.Empty;
                if (string.IsNullOrWhiteSpace(currentPassiveId))
                {
                    continue;
                }

                if (model == null || !model.Skills.TryGetValue(currentPassiveId, out var skill))
                {
                    errors.Add($"Skill effect '{effect.Id}' {columnName} references unknown passive skill '{currentPassiveId}'.");
                    continue;
                }

                if (skill.SkillKind != PakuriCsvSkillKind.Passive)
                {
                    errors.Add($"Skill effect '{effect.Id}' {columnName} references non-passive skill '{currentPassiveId}'.");
                }
            }
        }

        private static void ValidateStatusEffectRow(StatusEffectRow status, List<string> errors)
        {
            if (status == null)
            {
                return;
            }

            if (!StatusEffectUtility.TryParse(status.Id, out var kind) || kind == StatusEffectKind.None)
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
                || string.Equals(passiveId, "IncomingDamageDown", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "PhysicalDefenseUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "FireDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "FireDefenseUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "LightningDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "LightningDefenseUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "IceDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "IceDefenseUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "DarknessDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "DarknessDefenseUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "HolyDamageUp", StringComparison.OrdinalIgnoreCase)
                || string.Equals(passiveId, "HolyDefenseUp", StringComparison.OrdinalIgnoreCase);
        }

        private static void ValidateEnemyRows(
            IEnumerable<EnemyRow> enemies,
            Dictionary<string, EnemySkillRow> enemySkills,
            List<string> errors)
        {
            if (enemies == null)
            {
                return;
            }

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                if (!enemySkills.ContainsKey(enemy.StageOneSkill.ToString()))
                {
                    errors.Add($"Enemy '{enemy.Id}' references unknown enemy skill '{enemy.StageOneSkill}'.");
                }

                if (enemy.HasBasicSkill && !enemySkills.ContainsKey(enemy.BasicSkill.ToString()))
                {
                    errors.Add($"Enemy '{enemy.Id}' references unknown basic enemy skill '{enemy.BasicSkill}'.");
                }

                ValidateEnemyPassiveColumns(enemy, errors);
            }
        }

        private static void ValidateEnemySkillNodes(SourceModel model, List<string> errors)
        {
            if (model == null)
            {
                return;
            }

            var supportedActionOps = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "DamageArea",
                "SpawnProjectile",
                "Heal",
                "ApplySelfIncomingDamageMultiplier",
                "GrantShieldToEnemyAllies",
                "ApplyAllyMoveAndDamageMultiplier",
                "Damage",
                "DamageAndActionSpeedDebuff",
                "DamageThenDelayedChain",
                "ChargeDamageStatus",
                "ApplyOutgoingDamageMultiplierStatus"
            };
            var supportedTargetSelectors = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "CurrentTarget",
                "NearestTower",
                "FarthestTower",
                "RandomTower",
                "LowestHealthEnemyAlly",
                "Self",
                "EnemyAlliesInRadius",
                "AllTowers"
            };
            var knownNodeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.EnemySkillNodes.Count; i++)
            {
                var node = model.EnemySkillNodes[i];
                if (node == null)
                {
                    continue;
                }

                if (!model.EnemySkills.ContainsKey(node.SkillId))
                {
                    errors.Add($"Enemy skill node '{node.NodeId}' references unknown enemy skill '{node.SkillId}'.");
                }

                if (string.IsNullOrWhiteSpace(node.ActionOp))
                {
                    errors.Add($"Enemy skill node '{node.NodeId}' has empty action_op.");
                }
                else if (!supportedActionOps.Contains(node.ActionOp))
                {
                    errors.Add($"Enemy skill node '{node.NodeId}' has unsupported action_op '{node.ActionOp}'.");
                }

                if (!string.IsNullOrWhiteSpace(node.TargetSelector) && !supportedTargetSelectors.Contains(node.TargetSelector))
                {
                    errors.Add($"Enemy skill node '{node.NodeId}' has unsupported target_selector '{node.TargetSelector}'.");
                }

                knownNodeIds.Add($"{node.SkillId}:{node.NodeId}");
            }

            for (var i = 0; i < model.EnemySkillNodeParams.Count; i++)
            {
                var param = model.EnemySkillNodeParams[i];
                if (param == null)
                {
                    continue;
                }

                if (!knownNodeIds.Contains($"{param.SkillId}:{param.NodeId}"))
                {
                    errors.Add($"Enemy skill node param '{param.SkillId}/{param.NodeId}/{param.ParamKey}' references unknown enemy skill node.");
                }
            }
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

            var referencedAssets = CollectReferencedAssets(model);
            foreach (var asset in referencedAssets.SpritePaths)
            {
                ValidateSpritePath(assetCatalog, asset.AssetPath, asset.OwnerLabel, errors);
            }

            foreach (var asset in referencedAssets.PrefabPaths)
            {
                ValidatePrefabPath(assetCatalog, asset.AssetPath, asset.OwnerLabel, errors);
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

                if (catalog.StageTwoEnemies == null || catalog.StageTwoEnemies.Length == 0)
                {
                    errors.Add("Runtime GameDataCatalog has no stage-two enemies.");
                }

                if (catalog.StatusEffects == null || catalog.StatusEffects.Length == 0)
                {
                    errors.Add("Runtime GameDataCatalog has no status effects.");
                }
            }

            if (catalog != null && sourceModel != null)
            {
                ValidateRuntimeMonsterAssets(catalog.Monsters, sourceModel, errors);
                ValidateRuntimeEnemyAssets(catalog.StageOneEnemies, sourceModel.StageOneEnemies, errors);
                ValidateRuntimeEnemyAssets(catalog.StageTwoEnemies, sourceModel.StageTwoEnemies, errors);
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
            Dictionary<string, EnemyRow> sourceEnemies,
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

                if (sourceEnemies == null || !sourceEnemies.TryGetValue(enemy.EnemyId, out var sourceEnemy))
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

                ValidateRuntimeSkillChoiceAssets(skill.BaseModifierChoices, sourceModel, skill.PassiveId, errors);
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

                var skillIconPath = sourceChoice.SkillIconPath;
                if (!string.IsNullOrWhiteSpace(skillIconPath) && choice.SkillIcon == null)
                {
                    errors.Add($"Runtime skill choice '{choice.ChoiceId}' is missing SkillIcon for '{skillIconPath}'.");
                }

                var skillEffectPrefabPath = sourceChoice.SkillEffectPrefabPath;
                if (!string.IsNullOrWhiteSpace(skillEffectPrefabPath) && choice.SkillEffectPrefab == null)
                {
                    errors.Add($"Runtime skill choice '{choice.ChoiceId}' is missing SkillEffectPrefab for '{skillEffectPrefabPath}'.");
                }
            }
        }
    }
}
