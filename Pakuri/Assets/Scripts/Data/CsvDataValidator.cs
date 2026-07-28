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


/*
 * CSV 파싱 직후 원본 행, 참조 관계, 실행 수치, Unity 자산 경로를 한 번 검사한다.
 * 전투 실행 코드는 이 검사를 통과한 데이터만 받는다.
 */
namespace Pakuri.Data
{
    internal static class CsvDataValidator
    {
        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSourceModelOrThrow(SourceModel model /* CSV에서 읽은 원본 데이터 */, CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateRuntimeStatusColumns(
            SkillRow skill /* 검사할 스킬 CSV 행 */,
            Dictionary<string, StatusEffectRow> statusEffects /* 상태 효과 정의 목록 */,
            string targetScope /* 대상 적용 범위 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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

            if (!StatusEffectLookup.TryParse(statusKey, out var kind))
            {
                errors.Add($"Skill '{skill.Id}' uses status_effect_id '{statusKey}' that cannot map to StatusEffectKind.");
                return;
            }

            if (statusDefinition.Classification == StatusEffectClassification.Buff)
            {
                if (string.IsNullOrWhiteSpace(targetScope)
                    && !StatusRuntimeCompiler.TryParseTargetScope(status.StatusTargetScope, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_target_scope. Expected self or all_allies.");
                }

                if (!StatusRuntimeCompiler.TryParseMergePolicy(status.StatusMergePolicy, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported status_merge_policy for buff status '{statusKey}'.");
                }
            }

            if (kind == StatusEffectKind.Shield)
            {
                if (!StatusRuntimeCompiler.TryParseShieldRefreshRule(status.ShieldAmountRefreshPolicy, out _))
                {
                    errors.Add($"Skill '{skill.Id}' requires supported shield_amount_refresh_policy for shield status.");
                }

                if (status.StatusDurationSeconds <= 0f)
                {
                    errors.Add($"Skill '{skill.Id}' requires positive status_duration_seconds for shield status.");
                }
            }
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillTriggerRow(
            SkillTriggerRow trigger /* 실행하거나 검사할 트리거 */,
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static bool ValidateSkillRuntimeKindList(string rawValue /* 변환 전 원본 문자열 */)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool HasPositiveDamagePayload(float baseDamage /* 방어 계산 전 기본 피해량 */, float attackPowerCoefficient /* 공격력 계수 */, float spellPowerCoefficient /* 주문력 계수 */)
        {
            return baseDamage > 0f
                || attackPowerCoefficient > 0f
                || spellPowerCoefficient > 0f;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateSkillIdList(
            string rawSkillIds /* 변환 전 스킬 식별자 목록 */,
            SkillTriggerRow trigger /* 실행하거나 검사할 트리거 */,
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            string columnName /* 읽거나 검사할 CSV 열 이름 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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

                if (model == null || !model.Skills.TryGetValue(skillId, out _))
                {
                    errors.Add($"Skill trigger '{trigger.Id}' {columnName} references unknown skill '{skillId}'.");
                }
            }
        }

        /*
         * 유닛 생성과 적 행동에 필요한 필수 수치를 로딩 경계에서 검사한다.
         */
        private static void ValidateUnitRuntimeValues(SourceModel model /* CSV에서 읽은 원본 데이터 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

        /*
         * 스킬 행이 공용 실행기로 변환될 수 있는 종류와 필수 수치를 검사한다.
         */
        internal static void ValidateSkillRuntimeValues(SkillRow skill /* 검사할 스킬 CSV 행 */, List<string> errors /* 검증 오류를 모을 목록 */)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */

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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static bool ValidateEventSourceScope(string rawValue /* 변환 전 원본 문자열 */)
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
        internal static bool ValidateTriggerAttributes(string rawValue /* 변환 전 원본 문자열 */)
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool IsSupportedHitTargetCount(string rawValue /* 변환 전 원본 문자열 */)
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
            string choiceId /* 스킬 선택지 식별자 */,
            SkillTriggerRow trigger /* 실행하거나 검사할 트리거 */,
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            string columnName /* 읽거나 검사할 CSV 열 이름 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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

        /*
         * 선택지가 해당 스킬에 적용되는지 확인한다.
         */
        internal static bool ChoiceAppliesToSkillId(SkillChoiceRow choice /* 적용하거나 검사할 스킬 선택지 */, string skillId /* 스킬 식별자 */)
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

        /*
         * ValidateStatusIdList 데이터가 올바른지 검사한다.
         */
        internal static void ValidateStatusIdList(
            string ownerId /* 소유자 식별자 */,
            string rawValue /* 변환 전 원본 문자열 */,
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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

        /*
         * ValidateStatusConditionExpression 데이터가 올바른지 검사한다.
         */
        private static void ValidateStatusConditionExpression(
            string ownerId /* 소유자 식별자 */,
            string rawValue /* 변환 전 원본 문자열 */,
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
            {
                return;
            }

            if (!StatusRuntimeCompiler.TryParseConditionStatusExpression(rawValue, out _))
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateStatusEffectRow(StatusEffectRow status /* 검사할 상태 효과 CSV 행 */, List<string> errors /* 검증 오류를 모을 목록 */)
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
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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
            CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */,
            string assetPath /* 에셋 경로 */,
            string ownerLabel /* 소유자 표시 문구 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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
            CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */,
            string assetPath /* 에셋 경로 */,
            string ownerLabel /* 소유자 표시 문구 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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
         * 각 노드 handler가 실제 전투 변환 경로를 가지는지 검사한다.
         */
        private static void ValidateSkillNodeHandlers(SourceModel model /* CSV에서 읽은 원본 데이터 */, List<string> errors /* 검증 오류를 모을 목록 */)
        {
            for (var i = 0; i < model.SkillGraphNodes.Count; i++)
            {
                var graph = model.SkillGraphNodes[i];
                if (!model.SkillNodeTypes.TryGetValue(graph.NodeTypeId, out var nodeType))
                {
                    continue;
                }
                if (SkillNodeMapper.CanProcessNode(graph.OwnerKind.ToString(), nodeType.HandlerId))
                {
                    continue;
                }

                errors.Add(
                    $"Skill graph '{BuildSkillGraphKey(graph)}' node '{graph.NodeOrder}' uses handler '{nodeType.HandlerId}' without a combat conversion route.");
            }
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateAnimatorControllerPath(
            CsvRuntimeCatalog assetCatalog /* 검사할 에셋 목록 */,
            string assetPath /* 에셋 경로 */,
            string ownerLabel /* 소유자 표시 문구 */,
            List<string> errors /* 검증 오류를 모을 목록 */)
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

    }
}


/*
 * CSV가 참조하는 Sprite, Prefab, Animator 경로를 수집하고 중복을 정리한다.
 */
namespace Pakuri.Data
{
    internal static class CsvAssetReferenceCollector
    {
        internal readonly struct ReferencedAssetPath
        {
            /*
             * 참조 자산 경로를 구성한다.
             */
            public ReferencedAssetPath(string assetPath /* 에셋 경로 */, string ownerLabel /* 소유자 표시 문구 */)
            {
                AssetPath = assetPath;
                OwnerLabel = ownerLabel;
            }

            public string AssetPath { get; }
            public string OwnerLabel { get; }
        }

        /*
         * 종류별 자산 경로와 중복 확인용 경로 집합을 보관한다.
         */
        internal class ReferencedAssetSet
        {
            internal readonly HashSet<string> spritePathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> prefabPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            internal readonly HashSet<string> animatorControllerPathLookup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            public List<ReferencedAssetPath> SpritePaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> PrefabPaths { get; } = new List<ReferencedAssetPath>();
            public List<ReferencedAssetPath> AnimatorControllerPaths { get; } = new List<ReferencedAssetPath>();

            /*
             * 항목을 대상 목록에 추가한다.
             */
            public void AddSprite(string assetPath /* 에셋 경로 */, string ownerLabel /* 소유자 표시 문구 */)
            {
                Add(assetPath, ownerLabel, spritePathLookup, SpritePaths);
            }

            /*
             * 항목을 대상 목록에 추가한다.
             */
            public void AddPrefab(string assetPath /* 에셋 경로 */, string ownerLabel /* 소유자 표시 문구 */)
            {
                Add(assetPath, ownerLabel, prefabPathLookup, PrefabPaths);
            }

            /*
             * 항목을 대상 목록에 추가한다.
             */
            public void AddAnimatorController(string assetPath /* 에셋 경로 */, string ownerLabel /* 소유자 표시 문구 */)
            {
                Add(assetPath, ownerLabel, animatorControllerPathLookup, AnimatorControllerPaths);
            }

            /*
             * 항목을 대상 목록에 추가한다.
             */
            internal static void Add(
                string assetPath /* 에셋 경로 */,
                string ownerLabel /* 소유자 표시 문구 */,
                HashSet<string> lookup /* 조회표 */,
                List<ReferencedAssetPath> paths /* 경로 목록 */)
            {
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    return;
                }

                var normalizedPath = assetPath.Trim().Replace('\\', '/');
                if (lookup.Add(normalizedPath))
                {
                    paths.Add(new ReferencedAssetPath(normalizedPath, ownerLabel));
                }
            }
        }

        /*
         * 원본 모델이 사용하는 Sprite, Prefab, Animator 참조를 모은다.
         */
        internal static ReferencedAssetSet CollectReferencedAssets(SourceModel model /* CSV에서 읽은 원본 데이터 */)
        {
            var assets = new ReferencedAssetSet();
            if (model == null)
            {
                return assets;
            }

            foreach (var skill in model.Skills.Values)
            {
                assets.AddSprite(skill.SkillIconPath, $"Skill '{skill.Id}' skill_icon_path");
                assets.AddPrefab(skill.SkillEffectPrefabPath, $"Skill '{skill.Id}' skill_effect_prefab_path");
                assets.AddPrefab(skill.Status.StatusEffectPrefabPath, $"Skill '{skill.Id}' status_effect_prefab_path");
                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Skill '{skill.Id}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Skill '{skill.Id}' runtime_visual_animator_controller_path");
                assets.AddSprite(
                    skill.RuntimeImpactVisualSpritePath,
                    $"Skill '{skill.Id}' runtime_impact_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeImpactVisualAnimatorControllerPath,
                    $"Skill '{skill.Id}' runtime_impact_visual_animator_controller_path");
            }

            foreach (var enemySkill in model.EnemyBaseSkills.Values)
            {
                SkillRow skill = null;
                if (enemySkill != null)
                {
                    skill = enemySkill.Skill;
                }
                if (skill == null)
                {
                    continue;
                }

                assets.AddSprite(skill.RuntimeVisualSpritePath, $"Enemy base skill '{skill.Id}' runtime_visual_sprite_path");
                assets.AddAnimatorController(
                    skill.RuntimeVisualAnimatorControllerPath,
                    $"Enemy base skill '{skill.Id}' runtime_visual_animator_controller_path");
            }

            foreach (var choice in model.SkillChoices.Values)
            {
                assets.AddSprite(choice.SkillIconPath, $"Skill choice '{choice.Id}' skill_icon_path");
            }

            foreach (var param in model.SkillNodeParams)
            {
                if (param == null || param.ValueType != SkillNodeValueType.AssetPath)
                {
                    continue;
                }

                if (param.ParamKey != null
                    && param.ParamKey.IndexOf("sprite", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    assets.AddSprite(param.Value, $"Skill node param '{param.NodeId}.{param.ParamKey}'");
                }
                else if (param.ParamKey != null
                    && param.ParamKey.IndexOf("animator", StringComparison.OrdinalIgnoreCase) >= 0
                    && param.ParamKey.IndexOf("controller", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    assets.AddAnimatorController(param.Value, $"Skill node param '{param.NodeId}.{param.ParamKey}'");
                }
                else
                {
                    assets.AddPrefab(param.Value, $"Skill node param '{param.NodeId}.{param.ParamKey}'");
                }
            }

            foreach (var status in model.StatusEffects.Values)
            {
                assets.AddPrefab(status.StatusEffectPrefabPath, $"Status effect '{status.Id}' status_effect_prefab_path");
            }

            foreach (var monster in model.Monsters.Values)
            {
                assets.AddSprite(monster.MonsterIconImagePath, $"Monster '{monster.Id}' MonsterIconImage");
            }

            return assets;
        }
    }
}
