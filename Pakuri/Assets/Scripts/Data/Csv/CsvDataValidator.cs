using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.InGame;
using static Pakuri.Data.CsvAssetReferenceCollector;
using static Pakuri.Data.CsvDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.GameDataBuilder;
using static Pakuri.Data.SkillGraphBuilder;


/*
 * CSV를 게임 데이터로 등록하기 전에 원본 행, 참조 관계, 실행 수치와
 * 완성된 스킬 컴파일 결과를 한 번 검사한다. 전투 실행 코드는 이 검사를
 * 통과한 데이터만 받으며 같은 데이터 조건을 다시 검사하지 않는다.
 */
namespace Pakuri.Data
{
    internal static class CsvDataValidator
    {
        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

                ValidateSkillRuntimeValues(skill, errors);
                ValidateRuntimeStatusColumns(skill, model.StatusEffects, errors);
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
                    && !StatusEffectLookup.TryParse(choice.CountStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported count_status_id '{choice.CountStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.StatusConditionalSourceStatusId)
                    && !StatusEffectLookup.TryParse(choice.StatusConditionalSourceStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported status_conditional_source_status_id '{choice.StatusConditionalSourceStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.StatusDurationBonusStatusId)
                    && !StatusEffectLookup.TryParse(choice.StatusDurationBonusStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported status_duration_bonus_status_id '{choice.StatusDurationBonusStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.StatusMaxStacksBonusStatusId)
                    && !StatusEffectLookup.TryParse(choice.StatusMaxStacksBonusStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported status_max_stacks_bonus_status_id '{choice.StatusMaxStacksBonusStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ThresholdStatusId)
                    && !StatusEffectLookup.TryParse(choice.ThresholdStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported threshold_status_id '{choice.ThresholdStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ThresholdApplyStatusId)
                    && !StatusEffectLookup.TryParse(choice.ThresholdApplyStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported threshold_apply_status_id '{choice.ThresholdApplyStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ConditionalTargetStatusId)
                    && !StatusEffectLookup.TryParse(choice.ConditionalTargetStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported conditional_target_status_id '{choice.ConditionalTargetStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.ConditionalCritTargetStatusId)
                    && !StatusEffectLookup.TryParse(choice.ConditionalCritTargetStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported conditional_crit_target_status_id '{choice.ConditionalCritTargetStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.RedistributeConsumedStatusId)
                    && !StatusEffectLookup.TryParse(choice.RedistributeConsumedStatusId, out _))
                {
                    errors.Add($"Skill choice '{choice.Id}' uses unsupported redistribute_consumed_status_id '{choice.RedistributeConsumedStatusId}'.");
                }

                if (!string.IsNullOrWhiteSpace(choice.RequiredSourceStatusId)
                    && !StatusEffectLookup.TryParse(choice.RequiredSourceStatusId, out _))
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

            ValidateEnemyMigrationRows(model, errors);

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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeStatusColumns(
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
                && !StatusEffectLookup.TryParse(skill.DeploymentRequiredTargetStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported deployment_required_target_status_id '{skill.DeploymentRequiredTargetStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetSelectionStatusId)
                && !StatusEffectLookup.TryParse(skill.TargetSelectionStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported target_selection_status_id '{skill.TargetSelectionStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.TargetStatusStackStatusId)
                && !StatusEffectLookup.TryParse(skill.TargetStatusStackStatusId, out _))
            {
                errors.Add($"Skill '{skill.Id}' uses unsupported target_status_stack_status_id '{skill.TargetStatusStackStatusId}'.");
            }

            if (!string.IsNullOrWhiteSpace(skill.ConsumeTargetStatusId)
                && !StatusEffectLookup.TryParse(skill.ConsumeTargetStatusId, out _))
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

            var hasSupportedStatus = StatusEffectLookup.TryParse(statusKey, out var kind);
            if (status.StatusChance > 0f && !hasSupportedStatus)
            {
                errors.Add(
                    $"Skill '{skill.Id}' uses unsupported runtime status '{statusKey}'. Add it to StatusEffectKind or set status_chance to 0 for design-only labels.");
            }

            if (status.StatusChance > 0f && hasSupportedStatus)
            {
                var statusId = StatusEffectLookup.ToId(kind);
                if (!string.IsNullOrWhiteSpace(statusId)
                    && (statusEffects == null || !statusEffects.ContainsKey(statusId)))
                {
                    errors.Add($"Skill '{skill.Id}' uses status '{statusId}' but status_effects.csv has no matching row.");
                }
            }

            if (skill.RuntimeKind == SkillRuntimeKind.Buff || skill.RuntimeKind == SkillRuntimeKind.Shield)
            {
                if (!StatusDataCompiler.TryParseTargetScope(status.StatusTargetScope, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_target_scope for {skill.RuntimeKind}. Expected self or all_allies.");
                }

                if (!StatusDataCompiler.TryParseMergePolicy(status.StatusMergePolicy, out _))
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

                if (!StatusDataCompiler.TryParseShieldRefreshRule(status.ShieldAmountRefreshPolicy, out _))
                {
                    errors.Add($"Shield skill '{skill.Id}' requires supported shield_amount_refresh_policy.");
                }

                if (status.StatusDurationSeconds <= 0f)
                {
                    errors.Add($"Shield skill '{skill.Id}' requires positive status_duration_seconds.");
                }
            }
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillTriggerRow(
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
                && !StatusEffectLookup.TryParse(trigger.RequiredSourceStatusId, out _))
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
                var resolvedEffectId = ResolveTriggeredEffectId(trigger);
                if (HasSkillGraphReference(trigger))
                {
                    if (!string.IsNullOrWhiteSpace(trigger.TriggeredEffectId))
                    {
                        errors.Add(
                            $"Skill trigger '{trigger.Id}' cannot set both triggered_effect_id and triggered graph reference columns.");
                    }
                    if (trigger.TriggeredGraphKind != SkillGraphKind.Effect)
                    {
                        errors.Add($"Skill trigger '{trigger.Id}' graph reference must use graph_kind 'Effect'.");
                    }
                    if (!HasSkillGraphSource(model, trigger))
                    {
                        errors.Add(
                            $"Skill trigger '{trigger.Id}' references unknown skill graph '{trigger.TriggeredGraphOwnerKind}/{trigger.TriggeredGraphOwnerId}/Effect/{trigger.TriggeredGraphIndex}'.");
                    }
                }

                if (model == null || string.IsNullOrWhiteSpace(resolvedEffectId) || !HasSkillEffectSource(model, resolvedEffectId))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' references unknown triggered effect '{resolvedEffectId}'.");
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
                && !StatusDataCompiler.TryParseConditionStatusExpression(trigger.ConditionStatusId, out _))
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static bool ValidateSkillRuntimeKindList(string rawValue)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool HasPositiveDamagePayload(float baseDamage, float attackPowerCoefficient, float spellPowerCoefficient)
        {
            return baseDamage > 0f
                || attackPowerCoefficient > 0f
                || spellPowerCoefficient > 0f;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 스킬 행이 공용 실행기로 변환될 수 있는 종류와 필수 수치를 검사한다.
         */
        internal static void ValidateSkillRuntimeValues(SkillRow skill, List<string> errors)
        {
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool HasSkillEffectSource(SourceModel model, string effectId)
        {
            if (model == null || string.IsNullOrWhiteSpace(effectId))
            {
                return false;
            }

            foreach (var node in model.SkillNodes.Values)
            {
                if (node != null
                    && node.OwnerKind == SkillNodeOwnerKind.Effect
                    && string.Equals(node.OwnerId, effectId, StringComparison.OrdinalIgnoreCase)
                    && IsEffectOperationHandler(node.HandlerId))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool HasSkillGraphSource(SourceModel model, SkillTriggerRow trigger)
        {
            if (model == null || !HasSkillGraphReference(trigger))
            {
                return false;
            }

            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                if (graph.GraphKind == SkillGraphKind.Effect
                    && graph.OwnerKind == trigger.TriggeredGraphOwnerKind
                    && graph.GraphIndex == trigger.TriggeredGraphIndex
                    && string.Equals(graph.MonsterId, trigger.MonsterId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(graph.OwnerId, trigger.TriggeredGraphOwnerId, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static bool ValidateTriggerAttributes(string rawValue)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 선택지가 해당 스킬에 적용되는지 확인한다.
         */
        internal static bool ChoiceAppliesToSkillId(SkillChoiceRow choice, string skillId)
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

        /*
         * 선택지가 해당 스킬에 적용되는지 확인한다.
         */
        internal static bool ChoiceTargetsKnownSkills(SkillChoiceRow choice, SourceModel model, out string unknownSkillId)
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

        /*
         * 선택지가 해당 스킬에 적용되는지 확인한다.
         */
        internal static bool ChoiceTargetsOnlyMonsterSkills(SkillChoiceRow choice, SourceModel model, out string foreignSkillId)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool MatchesDelimitedValue(string rawValues, string expected)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateStatusEffectRow(StatusEffectRow status, List<string> errors)
        {
            if (status == null)
            {
                return;
            }

            if (!StatusEffectLookup.TryParse(status.Id, out var kind) || kind == StatusEffectKind.None)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeCatalogOrThrow(GameDataCatalog catalog, SourceModel sourceModel)
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
                ValidateRuntimeEnemyAssets(catalog.StageOneEnemies, sourceModel.Enemies, errors);
                ValidateRuntimeEnemyAssets(catalog.StageTwoEnemies, sourceModel.Enemies, errors);
            }

            if (errors.Count > 0)
            {
                throw new CsvFatalException("Runtime GameDataCatalog validation failed.", errors);
            }
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeMonsterAssets(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeEnemyAssets(
            EnemyDefinition[] enemies,
            Dictionary<string, EnemyMigrationRow> sourceEnemies,
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

            }
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeActiveSkillAssets(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimePassiveSkillAssets(
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeSkillChoiceAssets(
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

        /*
         * 조회 등록이 끝난 카탈로그의 모든 스킬을 실행 데이터로 변환해 검사한다.
         */
        internal static void ValidateCompiledSkillDataOrThrow(GameDataCatalog catalog)
        {
            var errors = new List<string>();
            ValidateCompiledMonsterSkills(catalog.Monsters, errors);
            ValidateCompiledEnemySkills(catalog.StageOneEnemies, errors);
            ValidateCompiledEnemySkills(catalog.StageTwoEnemies, errors);

            if (errors.Count > 0)
            {
                throw new CsvFatalException("Compiled skill data validation failed.", errors);
            }
        }

        /*
         * 플레이어 몬스터의 스킬 정의가 완성된 실행 데이터로 변환되는지 검사한다.
         */
        internal static void ValidateCompiledMonsterSkills(MonsterDefinition[] monsters, List<string> errors)
        {
            for (var monsterIndex = 0; monsterIndex < monsters.Length; monsterIndex++)
            {
                var monster = monsters[monsterIndex];
                for (var skillIndex = 0; skillIndex < monster.ActiveSkills.Length; skillIndex++)
                {
                    var source = monster.ActiveSkills[skillIndex];
                    var compiled = SkillRuntimeCompiler.CompileActive(monster, source);
                    ValidateCompiledSkill(source.SkillId, source.Slot, true, compiled, errors);
                }

                for (var skillIndex = 0; skillIndex < monster.PassiveSkills.Length; skillIndex++)
                {
                    var source = monster.PassiveSkills[skillIndex];
                    var compiled = SkillRuntimeCompiler.CompilePassive(monster, source);
                    ValidateCompiledSkill(source.PassiveId, source.Slot, false, compiled, errors);
                }
            }
        }

        /*
         * 적 스킬 정의가 완성된 실행 데이터로 변환되는지 검사한다.
         */
        internal static void ValidateCompiledEnemySkills(EnemyDefinition[] enemies, List<string> errors)
        {
            for (var enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
            {
                var enemy = enemies[enemyIndex];
                for (var skillIndex = 0; skillIndex < enemy.ActiveSkills.Length; skillIndex++)
                {
                    var source = enemy.ActiveSkills[skillIndex];
                    var compiled = SkillRuntimeCompiler.CompileActive(enemy.EnemyId, source, enemy.SkillTriggers);
                    ValidateCompiledSkill(source.SkillId, source.Slot, true, compiled, errors);
                }
            }
        }

        /*
         * 변환된 스킬의 식별 정보와 공통 실행 설정을 검사한다.
         */
        internal static void ValidateCompiledSkill(
            string skillId,
            SkillSlot slot,
            bool active,
            SkillRuntimeData compiled,
            List<string> errors)
        {
            if (compiled == null)
            {
                errors.Add($"Skill compiler returned null for '{skillId}'.");
                return;
            }

            if (string.IsNullOrWhiteSpace(compiled.SkillId))
            {
                errors.Add($"Compiled skill '{skillId}' has an empty skill id.");
            }

            if (compiled.IsActive != active)
            {
                errors.Add($"Compiled skill '{skillId}' has an incorrect active flag.");
            }

            if (compiled.Slot != slot)
            {
                errors.Add($"Compiled skill '{skillId}' changed slot from '{slot}' to '{compiled.Slot}'.");
            }

            if (compiled.Timing == null)
            {
                errors.Add($"Compiled skill '{skillId}' has no timing data.");
            }

            if (compiled.Targeting == null)
            {
                errors.Add($"Compiled skill '{skillId}' has no targeting data.");
            }
        }
    }
}
