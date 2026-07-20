using System;
using System.Collections.Generic;
using Pakuri.Combat;
using UnityEngine;
using static Pakuri.Data.CsvDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphBuilder;


namespace Pakuri.Data
{
    /*
     * 몬스터와 적 CSV 행을 읽고 현재 데이터 규칙을 검사한다.
     */
    internal static class CsvRowParser
    {
        /*
         * 플레이어 몬스터 CSV 한 행의 능력치와 표시 정보를 보관한다.
         */
        internal sealed class MonsterRow
        {
            public string Id;
            public string DisplayName;
            public string RoleSummary;
            public string ElementLabel;
            public DamageAttribute PrimaryAttribute;
            public string ActiveSkillName;
            public string PassiveSkillName;
            public string MonsterIconImagePath;
            public float MaxHealth;
            public float PowerStat;
            public float BaseDamage;
            public float PowerCoefficient;
            public float BaseAttackPower;
            public float BaseSpellPower;
            public float BaseMoveSpeed;
            public float BaseCriticalChance;
            public float BaseCriticalDamage;
            public float BaseCriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
        }

        /*
         * 몬스터 초기 보상 선택지와 연결 스킬 ID를 보관한다.
         */
        internal sealed class RewardChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string ActiveSkillId;
            public string PassiveSkillId;
            public int SortOrder;
        }

        /*
         * 액티브·패시브 스킬 CSV 한 행의 실행 값을 보관한다.
         */
        internal sealed class SkillRow
        {
            public string Id;
            public string MonsterId;
            public PakuriCsvSkillKind SkillKind;
            public SkillSlot Slot;
            public string DisplayName;
            public SkillRuntimeKind RuntimeKind;
            public SkillImplementationState ImplementationState;
            public bool IsDefaultLearned;
            public bool IsAvailableWithoutActiveRequirement;
            public SkillSlot RequiredActiveSlot;
            public string SkillIconPath;
            public string SkillEffectPrefabPath;
            public string RuntimeVisualSpritePath;
            public string RuntimeVisualAnimatorControllerPath;
            public float RuntimeVisualScale = 1f;
            public float RuntimeVisualScaleX;
            public float RuntimeVisualScaleY;
            public float RuntimeVisualScaleZ;
            public int RuntimeVisualSortingOrder;
            public string RuntimeVisualAnchor;
            public float RuntimeHitboxSizeX;
            public float RuntimeHitboxSizeY;
            public float RuntimeHitboxOffsetX;
            public float RuntimeHitboxOffsetY;
            public string RuntimeImpactVisualSpritePath;
            public string RuntimeImpactVisualAnimatorControllerPath;
            public float RuntimeImpactVisualScale = 1f;
            public int RuntimeImpactVisualSortingOrder;
            public string DescriptionText;
            public string Summary;
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float Radius;
            public float KnockbackDistance;
            public float DamageDelaySeconds;
            public float ExecuteHealthRatioThreshold;
            public bool RequireExecuteThresholdToCast;
            public float ExecuteDamageMultiplier = 1f;
            public float KillCooldownRefundRatio;
            public float BossDamageMultiplier = 1f;
            public string HitTargetCount;
            public bool UsePrefabHitbox;
            public string TargetSelection;
            public string TargetSelectionStatusId;
            public int TargetSelectionStatusMinStacks;
            public float CooldownSeconds;
            public float ActiveDurationSeconds;
            public int MagazineCapacity;
            public float ReloadSeconds;
            public float ShotIntervalSeconds;
            public float BurstIntervalSeconds;
            public int ProjectileBurstCount;
            public int BurstDamageProjectileIndex;
            public float BurstDamageMultiplier = 1f;
            public float ProjectileSpeed;
            public int PierceCount;
            public bool CriticalAllowed;
            public string DeploymentRequiredTargetStatusId;
            public int DeploymentRequiredTargetStatusMinStacks;
            public string TargetStatusStackStatusId;
            public int TargetStatusStackMaxStacks;
            public float TargetStatusStackBaseDamage;
            public float TargetStatusStackAttackPowerCoefficient;
            public float TargetStatusStackSpellPowerCoefficient;
            public string ConsumeTargetStatusId;
            public float ConsumeTargetStatusRatio;
            public int ConsumeTargetStatusStacks;
            public StatusPayloadRow Status = new StatusPayloadRow();
        }

        /*
         * 스킬 성장 선택지 CSV 한 행의 변경값과 조건을 보관한다.
         */
        internal sealed class SkillChoiceRow
        {
            public string Id;
            public string MonsterId;
            public string SkillId;
            public string TargetSkillId;
            public string RuntimeTargetSkillIds;
            public PakuriCsvChoiceGroup ChoiceGroup;
            public int SortOrder;
            public string Title;
            public string DescriptionText;
            public string SkillIconPath;
            public string SkillEffectPrefabPath;
            public bool HasDamageMultiplier;
            public float DamageMultiplier = 1f;
            public float BaseDamageBonus;
            public bool HasCooldownMultiplier;
            public float CooldownMultiplier = 1f;
            public bool HasMagazineBonus;
            public int MagazineBonus;
            public int AdditionalProjectileBonus;
            public int PierceBonus;
            public bool HasShotIntervalMultiplier;
            public float ShotIntervalMultiplier = 1f;
            public bool HasBurstDamageProjectileIndex;
            public int BurstDamageProjectileIndex;
            public bool HasBurstDamageMultiplier;
            public float BurstDamageMultiplier = 1f;
            public bool HasBurstStatusProjectileIndex;
            public int BurstStatusProjectileIndex;
            public int BurstStatusStacksBonus;
            public int FollowUpProjectileCount;
            public float FollowUpProjectileDelaySeconds;
            public float FollowUpProjectileDamageMultiplier = 1f;
            public bool HasReloadTimeMultiplier;
            public float ReloadTimeMultiplier = 1f;
            public bool HasRadiusMultiplier;
            public float RadiusMultiplier = 1f;
            public float RadiusBonus;
            public float BeamWidthBonus;
            public bool HasKnockbackDistanceMultiplier;
            public float KnockbackDistanceMultiplier = 1f;
            public bool HasDamageDelayMultiplier;
            public float DamageDelayMultiplier = 1f;
            public bool HasExecuteHealthRatioBonus;
            public float ExecuteHealthRatioBonus;
            public bool HasDurationMultiplier;
            public float DurationMultiplier = 1f;
            public float DurationBonus;
            public float BranchChanceBonus;
            public bool HasBranchChanceSet;
            public float BranchChanceSet;
            public bool HasBranchCount;
            public int BranchCount;
            public bool HasBranchDamageMultiplier;
            public float BranchDamageMultiplier = 1f;
            public bool HasBranchSearchRadius;
            public float BranchSearchRadius;
            public int BranchLaunchPeriod;
            public bool HasBranchLaunchChanceSet;
            public float BranchLaunchChanceSet;
            public bool HasMaxHealthBonus;
            public float MaxHealthBonus;
            public int HitTargetCountBonus;
            public float CritChanceBonus;
            public float CritDamageBonus;
            public float ExecuteCritChanceBonus;
            public bool HasBossDamageMultiplier;
            public float BossDamageMultiplier = 1f;
            public bool HasKillCooldownRefundRatioBonus;
            public float KillCooldownRefundRatioBonus;
            public bool KillResetsCooldown;
            public bool KillResetsCooldownRequiresExecute;
            public string StatusTag;
            public bool HasStatusChanceBonus;
            public float StatusChanceBonus;
            public bool HasStatusActionSpeedBonus;
            public float StatusActionSpeedBonus;
            public bool HasStatusAttackPowerBonus;
            public float StatusAttackPowerBonus;
            public int StatusStacksBonus;
            public bool HasStatusStacksSet;
            public int StatusStacksSet;
            public bool HasStatusElementDamageTakenBonus;
            public float StatusElementDamageTakenBonus;
            public bool HasStatusCriticalDamageTakenBonus;
            public float StatusCriticalDamageTakenBonus;
            public bool HasStatusAilmentResistanceBonus;
            public float StatusAilmentResistanceBonus;
            public string StatusMaxStacksBonusStatusId;
            public int StatusMaxStacksBonus;
            public string StatusDurationBonusStatusId;
            public float StatusDurationBonus;
            public string ThresholdStatusId;
            public int ThresholdStatusMinStacks;
            public string ThresholdApplyStatusId;
            public bool HasConditionalDamageMultiplier;
            public float ConditionalDamageMultiplier = 1f;
            public string ConditionalTargetStatusId;
            public int ConditionalTargetStatusMinStacks;
            public bool HasTargetStatusStackDamageMultiplier;
            public float TargetStatusStackDamageMultiplier = 1f;
            public bool HasConsumeTargetStatusRatioOverride;
            public float ConsumeTargetStatusRatioOverride;
            public bool HasConsumeTargetStatusStacksOverride;
            public int ConsumeTargetStatusStacksOverride;
            public float ConditionalCritChanceBonus;
            public string ConditionalCritTargetStatusId;
            public int ConditionalCritTargetStatusMinStacks;
            public float RedistributeConsumedStatusRatioOnKill;
            public string RedistributeConsumedStatusId;
            public float RedistributeConsumedStatusSearchRadius;
            public int RedistributeConsumedStatusTargetCount;
            public string CountStatusId;
            public SkillMultiEffectTargetSide CountTargetSide;
            public float DamageMultiplierPerCount;
            public int CountMax;
            public float ConsecutiveHitBonusRate;
            public float ConsecutiveHitMax;
            public bool HasStatusConditionalDamageTakenBonus;
            public float StatusConditionalDamageTakenBonus;
            public string StatusConditionalSourceStatusId;
            public string RequiredSourceStatusId;
            public int RequiredSourceStatusMinStacks;
            public bool HasOnHitAdditionalDamage;
            public float OnHitAdditionalDamageChance;
            public float OnHitAdditionalDamageMultiplier = 1f;
            public DamageAttribute OnHitAdditionalDamageAttribute;
            public string OnHitAdditionalDamageTarget;
            public int OnHitChainHitPeriod;
            public int OnHitChainTargetCount;
            public float OnHitChainSearchRadius;
            public float OnHitChainDamageMultiplier = 1f;
            public DamageAttribute OnHitChainDamageAttribute;
            public string ReloadReduceTargetSkillId;
            public float ReloadReduceSecondsPerHit;
            public string CoreHitboxName;
            public bool HasCoreDamageMultiplier;
            public float CoreDamageMultiplier = 1f;
            public bool HasCoreOnHitAdditionalDamage;
            public float CoreOnHitAdditionalDamageChance;
            public float CoreOnHitAdditionalDamageMultiplier = 1f;
            public DamageAttribute CoreOnHitAdditionalDamageAttribute;
            public string HitCountCooldownRefundTargetSkillId;
            public int HitCountCooldownRefundMinTargets;
            public float HitCountCooldownRefundRatio;
            public int RepeatCountPerTarget;
            public float RepeatIntervalSeconds;
            public float RepeatDamageMultiplier = 1f;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        /*
         * 전투 사건에 연결된 스킬 Trigger 한 행을 보관한다.
         */
        internal sealed class SkillTriggerRow
        {
            public string Id;
            public string MonsterId;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string RequiresActiveChoiceId;
            public string ExcludesActiveChoiceId;
            public string RequiredSourceStatusId;
            public int RequiredSourceStatusMinStacks;
            public string ConditionStatusId;
            public string ConditionStatusSourceSkillId;
            public string TriggerAttribute;
            public SkillTriggerActionKind TriggerAction;
            public string EventSkillId;
            public string EventSkillRuntimeKinds;
            public float ProcChance = 1f;
            public float InternalCooldownSeconds;
            public float TriggerDelaySeconds;
            public int TriggerEveryCount;
            public string EventSourceScope;
            public string TriggeredSkillId;
            public string TargetSkillId;
            public string TriggeredEffectId;
            public SkillNodeOwnerKind TriggeredGraphOwnerKind;
            public string TriggeredGraphOwnerId;
            public SkillGraphKind TriggeredGraphKind;
            public int TriggeredGraphIndex;
            public SkillRuntimeKind RuntimeKind;
            public int SortOrder;
            public SkillMultiEffectTargetSide TargetSide;
            public SkillMultiEffectTargetSelection TargetSelection;
            public SkillMultiEffectTargetShape TargetShape;
            public SkillMultiEffectCenterMode CenterMode;
            public DamageAttribute Attribute;
            public float BaseDamage;
            public float AttackPowerCoefficient;
            public float SpellPowerCoefficient;
            public float DamageMultiplier = 1f;
            public SkillTriggerDamageSource DamageSource;
            public float DamageSourceMultiplier;
            public DamageAttribute TrackedAttribute;
            public float Radius;
            public bool CoverAll;
            public string HitTargetCount;
            public int RepeatCount = 1;
            public float RepeatIntervalSeconds;
            public bool RequireEventExecute;
            public float CooldownRefundRatio;
            public float ReloadReduceRatio;
            public string SkillEffectPrefabPath;
            public string RuntimeVisualSpritePath;
            public string RuntimeVisualAnimatorControllerPath;
            public float RuntimeVisualScale = 1f;
            public int RuntimeVisualSortingOrder;
            public string RuntimeVisualAnchor;
            public float RuntimeHitboxSizeX;
            public float RuntimeHitboxSizeY;
            public float RuntimeHitboxOffsetX;
            public float RuntimeHitboxOffsetY;
            public string RuntimeSupportState;
            public string RuntimeSupportNotes;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static MonsterRow ParseMonsterRow(CsvRecord record)
        {
            return new MonsterRow
            {
                Id = record.ReadRequiredString("id"),
                DisplayName = record.ReadRequiredString("display_name"),
                RoleSummary = record.ReadString("role_summary"),
                ElementLabel = record.ReadString("element_label"),
                PrimaryAttribute = record.ReadEnum<DamageAttribute>("primary_attribute"),
                ActiveSkillName = ReadOptionalStringIfColumnExists(record, "active_skill_name"),
                PassiveSkillName = ReadOptionalStringIfColumnExists(record, "passive_skill_name"),
                MonsterIconImagePath = ReadOptionalStringIfColumnExists(record, "MonsterIconImage"),
                MaxHealth = record.ReadFloat("max_health"),
                PowerStat = record.ReadFloat("power_stat"),
                BaseDamage = record.ReadFloat("base_damage"),
                PowerCoefficient = record.ReadFloat("power_coefficient"),
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

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static RewardChoiceRow ParseRewardChoiceRow(CsvRecord record)
        {
            return new RewardChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = record.ReadRequiredString("monster_id"),
                ActiveSkillId = record.ReadString("active_skill_id"),
                PassiveSkillId = record.ReadString("passive_skill_id"),
                SortOrder = record.ReadInt("sort_order")
            };
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillRow ParseSkillRow(CsvRecord record, string tableName, string ownerIdOverride = null)
        {
            var slot = record.ReadEnum<SkillSlot>("slot");
            return new SkillRow
            {
                Id = record.ReadRequiredString("skill_id"),
                MonsterId = !string.IsNullOrWhiteSpace(ownerIdOverride)
                    ? ownerIdOverride
                    : ReadMonsterIdOrInfer(record, tableName),
                SkillKind = ReadSkillKindOrInfer(record, slot),
                Slot = slot,
                DisplayName = record.ReadRequiredString("display_name"),
                RuntimeKind = ReadRuntimeKindOrInfer(record, slot),
                ImplementationState = ReadOptionalEnumIfColumnExists(record, "implementation_state", SkillImplementationState.RuntimeImplemented),
                IsDefaultLearned = ReadOptionalBoolWithDefaultIfColumnExists(record, "is_default_learned", slot == SkillSlot.A),
                IsAvailableWithoutActiveRequirement = ReadOptionalBoolWithDefaultIfColumnExists(record, "is_available_without_active_requirement", slot == SkillSlot.F),
                RequiredActiveSlot = ReadOptionalEnumIfColumnExists(record, "required_active_slot", InferRequiredActiveSlot(slot)),
                SkillIconPath = ReadOptionalStringIfColumnExists(record, "skill_icon_path"),
                SkillEffectPrefabPath = ReadOptionalStringIfColumnExists(record, "skill_effect_prefab_path"),
                RuntimeVisualSpritePath = ReadOptionalStringIfColumnExists(record, "runtime_visual_sprite_path"),
                RuntimeVisualAnimatorControllerPath = ReadOptionalStringIfColumnExists(record, "runtime_visual_animator_controller_path"),
                RuntimeVisualScale = ReadOptionalFloatWithDefaultIfColumnExists(record, "runtime_visual_scale", 1f),
                RuntimeVisualScaleX = ReadOptionalFloatIfColumnExists(record, "runtime_visual_scale_x"),
                RuntimeVisualScaleY = ReadOptionalFloatIfColumnExists(record, "runtime_visual_scale_y"),
                RuntimeVisualScaleZ = ReadOptionalFloatIfColumnExists(record, "runtime_visual_scale_z"),
                RuntimeVisualSortingOrder = ReadOptionalIntIfColumnExists(record, "runtime_visual_sorting_order"),
                RuntimeVisualAnchor = ReadOptionalStringIfColumnExists(record, "runtime_visual_anchor"),
                RuntimeHitboxSizeX = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_x"),
                RuntimeHitboxSizeY = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_y"),
                RuntimeHitboxOffsetX = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_offset_x"),
                RuntimeHitboxOffsetY = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_offset_y"),
                RuntimeImpactVisualSpritePath = ReadOptionalStringIfColumnExists(record, "runtime_impact_visual_sprite_path"),
                RuntimeImpactVisualAnimatorControllerPath = ReadOptionalStringIfColumnExists(record, "runtime_impact_visual_animator_controller_path"),
                RuntimeImpactVisualScale = ReadOptionalFloatWithDefaultIfColumnExists(record, "runtime_impact_visual_scale", 1f),
                RuntimeImpactVisualSortingOrder = ReadOptionalIntIfColumnExists(record, "runtime_impact_visual_sorting_order"),
                DescriptionText = ReadOptionalStringIfColumnExists(record, "description_text"),
                Summary = ReadOptionalStringIfColumnExists(record, "summary"),
                Attribute = ReadOptionalEnumIfColumnExists(record, "attribute", DamageAttribute.Physical),
                BaseDamage = ReadOptionalFloatIfColumnExists(record, "base_damage"),
                AttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "attack_power_coefficient"),
                SpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "spell_power_coefficient"),
                Radius = ReadOptionalFloatIfColumnExists(record, "radius"),
                KnockbackDistance = ReadOptionalFloatIfColumnExists(record, "knockback_distance"),
                DamageDelaySeconds = ReadOptionalFloatIfColumnExists(record, "damage_delay_seconds"),
                ExecuteHealthRatioThreshold = ReadOptionalFloatIfColumnExists(record, "execute_health_ratio_threshold"),
                RequireExecuteThresholdToCast = ReadOptionalBoolIfColumnExists(record, "require_execute_threshold_to_cast"),
                ExecuteDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "execute_damage_multiplier", 1f),
                KillCooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "kill_cooldown_refund_ratio"),
                BossDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "boss_damage_multiplier", 1f),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                UsePrefabHitbox = ReadOptionalBoolIfColumnExists(record, "use_prefab_hitbox"),
                TargetSelection = ReadOptionalStringIfColumnExists(record, "target_selection"),
                TargetSelectionStatusId = ReadOptionalStringIfColumnExists(record, "target_selection_status_id"),
                TargetSelectionStatusMinStacks = ReadOptionalIntIfColumnExists(record, "target_selection_status_min_stacks"),
                CooldownSeconds = ReadOptionalFloatIfColumnExists(record, "cooldown_seconds"),
                ActiveDurationSeconds = ReadOptionalFloatIfColumnExists(record, "active_duration_seconds"),
                MagazineCapacity = ReadOptionalIntIfColumnExists(record, "magazine_capacity"),
                ReloadSeconds = ReadOptionalFloatIfColumnExists(record, "reload_seconds"),
                ShotIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "shot_interval_seconds"),
                BurstIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "burst_interval_seconds"),
                ProjectileBurstCount = ReadOptionalIntIfColumnExists(record, "projectile_burst_count"),
                BurstDamageProjectileIndex = ReadOptionalIntIfColumnExists(record, "burst_damage_projectile_index"),
                BurstDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "burst_damage_multiplier", 1f),
                ProjectileSpeed = ReadOptionalFloatIfColumnExists(record, "projectile_speed"),
                PierceCount = ReadOptionalIntIfColumnExists(record, "pierce_count"),
                CriticalAllowed = ReadOptionalBoolIfColumnExists(record, "critical_allowed"),
                DeploymentRequiredTargetStatusId = ReadOptionalStringIfColumnExists(record, "deployment_required_target_status_id"),
                DeploymentRequiredTargetStatusMinStacks = ReadOptionalIntIfColumnExists(record, "deployment_required_target_status_min_stacks"),
                TargetStatusStackStatusId = ReadOptionalStringIfColumnExists(record, "target_status_stack_status_id"),
                TargetStatusStackMaxStacks = ReadOptionalIntIfColumnExists(record, "target_status_stack_max_stacks"),
                TargetStatusStackBaseDamage = ReadOptionalFloatIfColumnExists(record, "target_status_stack_base_damage"),
                TargetStatusStackAttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "target_status_stack_attack_power_coefficient"),
                TargetStatusStackSpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "target_status_stack_spell_power_coefficient"),
                ConsumeTargetStatusId = ReadOptionalStringIfColumnExists(record, "consume_target_status_id"),
                ConsumeTargetStatusRatio = ReadOptionalFloatIfColumnExists(record, "consume_target_status_ratio"),
                ConsumeTargetStatusStacks = ReadOptionalIntIfColumnExists(record, "consume_target_status_stacks"),
                Status = ReadStatusPayload(record, false, true)
            };
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillChoiceRow ParseSkillChoiceRow(CsvRecord record, string tableName)
        {
            var row = new SkillChoiceRow
            {
                Id = record.ReadRequiredString("choice_id"),
                MonsterId = ReadMonsterIdOrInfer(record, tableName),
                SkillId = record.ReadRequiredString("skill_id"),
                TargetSkillId = ReadOptionalStringIfColumnExists(record, "target_skill_id"),
                RuntimeTargetSkillIds = ReadOptionalStringIfColumnExists(record, "runtime_target_skill_ids"),
                ChoiceGroup = record.ReadEnum<PakuriCsvChoiceGroup>("choice_group"),
                SortOrder = record.ReadInt("sort_order"),
                Title = record.ReadRequiredString("title"),
                DescriptionText = ReadOptionalStringIfColumnExists(record, "description_text"),
                SkillIconPath = ReadOptionalStringIfColumnExists(record, "skill_icon_path"),
                SkillEffectPrefabPath = ReadOptionalStringIfColumnExists(record, "skill_effect_prefab_path"),
                StatusTag = ReadOptionalStringIfColumnExists(record, "status_tag"),
                RuntimeSupportState = ReadOptionalStringIfColumnExists(record, "runtime_support_state"),
                RuntimeSupportNotes = ReadOptionalStringIfColumnExists(record, "runtime_support_notes")
            };
            row.HasDamageMultiplier = TryReadFloatIfColumnExists(record, "damage_multiplier", out var damageMultiplier);
            row.DamageMultiplier = damageMultiplier;
            row.BaseDamageBonus = ReadOptionalFloatIfColumnExists(record, "base_damage_bonus");
            row.HasCooldownMultiplier = TryReadFloatIfColumnExists(record, "cooldown_multiplier", out var cooldownMultiplier);
            row.CooldownMultiplier = cooldownMultiplier;
            row.HasMagazineBonus = TryReadIntIfColumnExists(record, "magazine_bonus", out var magazineBonus);
            row.MagazineBonus = magazineBonus;
            row.AdditionalProjectileBonus = ReadOptionalIntIfColumnExists(record, "additional_projectile_bonus");
            row.PierceBonus = ReadOptionalIntIfColumnExists(record, "pierce_bonus");
            row.HasShotIntervalMultiplier = TryReadFloatIfColumnExists(record, "shot_interval_multiplier", out var shotIntervalMultiplier);
            row.ShotIntervalMultiplier = shotIntervalMultiplier;
            var burstDamageProjectileIndex = 0;
            row.HasBurstDamageProjectileIndex = record.HasColumn("burst_damage_projectile_index")
                && TryReadInt(record, "burst_damage_projectile_index", out burstDamageProjectileIndex);
            row.BurstDamageProjectileIndex = burstDamageProjectileIndex;
            row.HasBurstDamageMultiplier = TryReadFloatIfColumnExists(record, "burst_damage_multiplier", out var burstDamageMultiplier);
            row.BurstDamageMultiplier = burstDamageMultiplier;
            row.HasBurstStatusProjectileIndex = TryReadIntIfColumnExists(record, "burst_status_projectile_index", out var burstStatusProjectileIndex);
            row.BurstStatusProjectileIndex = burstStatusProjectileIndex;
            row.BurstStatusStacksBonus = ReadOptionalIntIfColumnExists(record, "burst_status_stacks_bonus");
            row.FollowUpProjectileCount = ReadOptionalIntIfColumnExists(record, "follow_up_projectile_count");
            row.FollowUpProjectileDelaySeconds = ReadOptionalFloatIfColumnExists(record, "follow_up_projectile_delay_seconds");
            row.FollowUpProjectileDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "follow_up_projectile_damage_multiplier", 1f);
            row.HasReloadTimeMultiplier = TryReadFloatIfColumnExists(record, "reload_time_multiplier", out var reloadTimeMultiplier);
            row.ReloadTimeMultiplier = reloadTimeMultiplier;
            row.HasRadiusMultiplier = TryReadFloatIfColumnExists(record, "radius_multiplier", out var radiusMultiplier);
            row.RadiusMultiplier = radiusMultiplier;
            row.RadiusBonus = ReadOptionalFloatIfColumnExists(record, "radius_bonus");
            row.BeamWidthBonus = ReadOptionalFloatIfColumnExists(record, "beam_width_bonus");
            row.HasKnockbackDistanceMultiplier = TryReadFloatIfColumnExists(record, "knockback_distance_multiplier", out var knockbackDistanceMultiplier);
            row.KnockbackDistanceMultiplier = knockbackDistanceMultiplier;
            row.HasDamageDelayMultiplier = TryReadFloatIfColumnExists(record, "damage_delay_multiplier", out var damageDelayMultiplier);
            row.DamageDelayMultiplier = damageDelayMultiplier;
            row.HasExecuteHealthRatioBonus = TryReadFloatIfColumnExists(record, "execute_health_ratio_bonus", out var executeHealthRatioBonus);
            row.ExecuteHealthRatioBonus = executeHealthRatioBonus;
            row.HasDurationMultiplier = TryReadFloatIfColumnExists(record, "duration_multiplier", out var durationMultiplier);
            row.DurationMultiplier = durationMultiplier;
            row.DurationBonus = ReadOptionalFloatIfColumnExists(record, "duration_bonus");
            row.BranchChanceBonus = ReadOptionalFloatIfColumnExists(record, "branch_chance_bonus");
            row.HasBranchChanceSet = TryReadFloatIfColumnExists(record, "branch_chance_set", out var branchChanceSet);
            row.BranchChanceSet = branchChanceSet;
            row.HasBranchCount = TryReadIntIfColumnExists(record, "branch_count", out var branchCount);
            row.BranchCount = branchCount;
            row.HasBranchDamageMultiplier = TryReadFloatIfColumnExists(record, "branch_damage_multiplier", out var branchDamageMultiplier);
            row.BranchDamageMultiplier = branchDamageMultiplier;
            row.HasBranchSearchRadius = TryReadFloatIfColumnExists(record, "branch_search_radius", out var branchSearchRadius);
            row.BranchSearchRadius = branchSearchRadius;
            row.BranchLaunchPeriod = ReadOptionalIntIfColumnExists(record, "branch_launch_period");
            row.HasBranchLaunchChanceSet = TryReadFloatIfColumnExists(record, "branch_launch_chance_set", out var branchLaunchChanceSet);
            row.BranchLaunchChanceSet = branchLaunchChanceSet;
            row.HasMaxHealthBonus = TryReadFloatIfColumnExists(record, "max_health_bonus", out var maxHealthBonus);
            row.MaxHealthBonus = maxHealthBonus;
            row.HitTargetCountBonus = ReadOptionalIntIfColumnExists(record, "hit_target_count_bonus");
            row.CritChanceBonus = ReadOptionalFloatIfColumnExists(record, "crit_chance_bonus");
            row.CritDamageBonus = ReadOptionalFloatIfColumnExists(record, "crit_damage_bonus");
            row.ExecuteCritChanceBonus = ReadOptionalFloatIfColumnExists(record, "execute_crit_chance_bonus");
            row.HasBossDamageMultiplier = TryReadFloatIfColumnExists(record, "boss_damage_multiplier", out var bossDamageMultiplier);
            row.BossDamageMultiplier = bossDamageMultiplier;
            row.HasKillCooldownRefundRatioBonus = TryReadFloatIfColumnExists(record, "kill_cooldown_refund_ratio_bonus", out var killCooldownRefundRatioBonus);
            row.KillCooldownRefundRatioBonus = killCooldownRefundRatioBonus;
            row.KillResetsCooldown = ReadOptionalBoolIfColumnExists(record, "kill_resets_cooldown");
            row.KillResetsCooldownRequiresExecute = ReadOptionalBoolIfColumnExists(record, "kill_resets_cooldown_requires_execute");
            row.HasStatusChanceBonus = TryReadFloatIfColumnExists(record, "status_chance_bonus", out var statusChanceBonus);
            row.StatusChanceBonus = statusChanceBonus;
            row.HasStatusActionSpeedBonus = TryReadFloatIfColumnExists(record, "status_action_speed_bonus", out var statusActionSpeedBonus);
            row.StatusActionSpeedBonus = statusActionSpeedBonus;
            row.HasStatusAttackPowerBonus = TryReadFloatIfColumnExists(record, "status_attack_power_bonus", out var statusAttackPowerBonus);
            row.StatusAttackPowerBonus = statusAttackPowerBonus;
            row.StatusStacksBonus = ReadOptionalIntIfColumnExists(record, "status_stacks_bonus");
            row.HasStatusStacksSet = TryReadIntIfColumnExists(record, "status_stacks_set", out var statusStacksSet);
            row.StatusStacksSet = statusStacksSet;
            row.HasStatusElementDamageTakenBonus = TryReadFloatIfColumnExists(record, "status_element_damage_taken_bonus", out var statusElementDamageTakenBonus);
            row.StatusElementDamageTakenBonus = statusElementDamageTakenBonus;
            row.HasStatusCriticalDamageTakenBonus = TryReadFloatIfColumnExists(record, "status_critical_damage_taken_bonus", out var statusCriticalDamageTakenBonus);
            row.StatusCriticalDamageTakenBonus = statusCriticalDamageTakenBonus;
            row.HasStatusAilmentResistanceBonus = TryReadFloatIfColumnExists(record, "status_ailment_resistance_bonus", out var statusAilmentResistanceBonus);
            row.StatusAilmentResistanceBonus = statusAilmentResistanceBonus;
            row.StatusMaxStacksBonusStatusId = ReadOptionalStringIfColumnExists(record, "status_max_stacks_bonus_status_id");
            row.StatusMaxStacksBonus = ReadOptionalIntIfColumnExists(record, "status_max_stacks_bonus");
            row.StatusDurationBonusStatusId = ReadOptionalStringIfColumnExists(record, "status_duration_bonus_status_id");
            row.StatusDurationBonus = ReadOptionalFloatIfColumnExists(record, "status_duration_bonus");
            row.ThresholdStatusId = ReadOptionalStringIfColumnExists(record, "threshold_status_id");
            row.ThresholdStatusMinStacks = ReadOptionalIntIfColumnExists(record, "threshold_status_min_stacks");
            row.ThresholdApplyStatusId = ReadOptionalStringIfColumnExists(record, "threshold_apply_status_id");
            row.HasConditionalDamageMultiplier = TryReadFloatIfColumnExists(record, "conditional_damage_multiplier", out var conditionalDamageMultiplier);
            row.ConditionalDamageMultiplier = conditionalDamageMultiplier;
            row.ConditionalTargetStatusId = ReadOptionalStringIfColumnExists(record, "conditional_target_status_id");
            row.ConditionalTargetStatusMinStacks = ReadOptionalIntIfColumnExists(record, "conditional_target_status_min_stacks");
            row.HasTargetStatusStackDamageMultiplier = TryReadFloatIfColumnExists(record, "target_status_stack_damage_multiplier", out var targetStatusStackDamageMultiplier);
            row.TargetStatusStackDamageMultiplier = targetStatusStackDamageMultiplier;
            row.HasConsumeTargetStatusRatioOverride = TryReadFloatIfColumnExists(record, "consume_target_status_ratio_override", out var consumeTargetStatusRatioOverride);
            row.ConsumeTargetStatusRatioOverride = consumeTargetStatusRatioOverride;
            row.HasConsumeTargetStatusStacksOverride = TryReadIntIfColumnExists(record, "consume_target_status_stacks_override", out var consumeTargetStatusStacksOverride);
            row.ConsumeTargetStatusStacksOverride = consumeTargetStatusStacksOverride;
            row.ConditionalCritChanceBonus = ReadOptionalFloatIfColumnExists(record, "conditional_crit_chance_bonus");
            row.ConditionalCritTargetStatusId = ReadOptionalStringIfColumnExists(record, "conditional_crit_target_status_id");
            row.ConditionalCritTargetStatusMinStacks = ReadOptionalIntIfColumnExists(record, "conditional_crit_target_status_min_stacks");
            row.RedistributeConsumedStatusRatioOnKill = ReadOptionalFloatIfColumnExists(record, "redistribute_consumed_status_ratio_on_kill");
            row.RedistributeConsumedStatusId = ReadOptionalStringIfColumnExists(record, "redistribute_consumed_status_id");
            row.RedistributeConsumedStatusSearchRadius = ReadOptionalFloatIfColumnExists(record, "redistribute_consumed_status_search_radius");
            row.RedistributeConsumedStatusTargetCount = ReadOptionalIntIfColumnExists(record, "redistribute_consumed_status_target_count");
            row.CountStatusId = ReadOptionalStringIfColumnExists(record, "count_status_id");
            row.CountTargetSide = ReadOptionalEnumIfColumnExists(
                record,
                "count_target_side",
                default(SkillMultiEffectTargetSide));
            row.DamageMultiplierPerCount = ReadOptionalFloatIfColumnExists(record, "damage_multiplier_per_count");
            row.CountMax = ReadOptionalIntIfColumnExists(record, "count_max");
            row.ConsecutiveHitBonusRate = ReadOptionalFloatIfColumnExists(record, "consecutive_hit_bonus_rate");
            row.ConsecutiveHitMax = ReadOptionalFloatIfColumnExists(record, "consecutive_hit_max");
            row.HasStatusConditionalDamageTakenBonus = TryReadFloatIfColumnExists(record, "status_conditional_damage_taken_bonus", out var statusConditionalDamageTakenBonus);
            row.StatusConditionalDamageTakenBonus = statusConditionalDamageTakenBonus;
            row.StatusConditionalSourceStatusId = ReadOptionalStringIfColumnExists(record, "status_conditional_source_status_id");
            row.RequiredSourceStatusId = ReadOptionalStringIfColumnExists(record, "required_source_status_id");
            row.RequiredSourceStatusMinStacks = ReadOptionalIntIfColumnExists(record, "required_source_status_min_stacks");
            row.HasOnHitAdditionalDamage = TryReadFloatIfColumnExists(record, "on_hit_additional_damage_chance", out var onHitAdditionalDamageChance);
            row.OnHitAdditionalDamageChance = onHitAdditionalDamageChance;
            row.OnHitAdditionalDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "on_hit_additional_damage_multiplier");
            row.OnHitAdditionalDamageAttribute = ReadOptionalEnumIfColumnExists(record, "on_hit_additional_damage_attribute", DamageAttribute.Physical);
            row.OnHitAdditionalDamageTarget = ReadOptionalStringIfColumnExists(record, "on_hit_additional_damage_target");
            row.OnHitChainHitPeriod = ReadOptionalIntIfColumnExists(record, "on_hit_chain_hit_period");
            row.OnHitChainTargetCount = ReadOptionalIntIfColumnExists(record, "on_hit_chain_target_count");
            row.OnHitChainSearchRadius = ReadOptionalFloatIfColumnExists(record, "on_hit_chain_search_radius");
            row.OnHitChainDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "on_hit_chain_damage_multiplier");
            row.OnHitChainDamageAttribute = ReadOptionalEnumIfColumnExists(record, "on_hit_chain_damage_attribute", DamageAttribute.Physical);
            row.ReloadReduceTargetSkillId = ReadOptionalStringIfColumnExists(record, "reload_reduce_target_skill_id");
            row.ReloadReduceSecondsPerHit = ReadOptionalFloatIfColumnExists(record, "reload_reduce_seconds_per_hit");
            row.CoreHitboxName = ReadOptionalStringIfColumnExists(record, "core_hitbox_name");
            row.HasCoreDamageMultiplier = TryReadFloatIfColumnExists(record, "core_damage_multiplier", out var coreDamageMultiplier);
            row.CoreDamageMultiplier = coreDamageMultiplier;
            row.HasCoreOnHitAdditionalDamage = TryReadFloatIfColumnExists(record, "core_on_hit_additional_damage_chance", out var coreOnHitAdditionalDamageChance);
            row.CoreOnHitAdditionalDamageChance = coreOnHitAdditionalDamageChance;
            row.CoreOnHitAdditionalDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "core_on_hit_additional_damage_multiplier");
            row.CoreOnHitAdditionalDamageAttribute = ReadOptionalEnumIfColumnExists(record, "core_on_hit_additional_damage_attribute", DamageAttribute.Physical);
            row.HitCountCooldownRefundTargetSkillId = ReadOptionalStringIfColumnExists(record, "hit_count_cooldown_refund_target_skill_id");
            row.HitCountCooldownRefundMinTargets = ReadOptionalIntIfColumnExists(record, "hit_count_cooldown_refund_min_targets");
            row.HitCountCooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "hit_count_cooldown_refund_ratio");
            row.RepeatCountPerTarget = ReadOptionalIntIfColumnExists(record, "repeat_count_per_target");
            row.RepeatIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "repeat_interval_seconds");
            row.RepeatDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "repeat_damage_multiplier", 1f);
            return row;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static SkillTriggerRow ParseSkillTriggerRow(CsvRecord record, string tableName)
        {
            var row = new SkillTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                MonsterId = ReadMonsterIdOrInfer(record, tableName),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                RequiresActiveChoiceId = ReadOptionalStringIfColumnExists(record, "requires_active_choice_id"),
                ExcludesActiveChoiceId = ReadOptionalStringIfColumnExists(record, "excludes_active_choice_id"),
                RequiredSourceStatusId = ReadOptionalStringIfColumnExists(record, "required_source_status_id"),
                RequiredSourceStatusMinStacks = ReadOptionalIntIfColumnExists(record, "required_source_status_min_stacks"),
                ConditionStatusId = ReadOptionalStringIfColumnExists(record, "condition_status_id"),
                ConditionStatusSourceSkillId = ReadOptionalStringIfColumnExists(record, "condition_status_source_skill_id"),
                TriggerAttribute = ReadOptionalStringIfColumnExists(record, "trigger_attribute"),
                TriggerAction = ReadOptionalEnumIfColumnExists(record, "trigger_action", SkillTriggerActionKind.Auto),
                EventSkillId = ReadOptionalStringIfColumnExists(record, "event_skill_id"),
                EventSkillRuntimeKinds = ReadOptionalStringIfColumnExists(record, "event_skill_runtime_kinds"),
                TriggeredSkillId = ReadOptionalStringIfColumnExists(record, "triggered_skill_id"),
                TargetSkillId = ReadOptionalStringIfColumnExists(record, "target_skill_id"),
                TriggeredEffectId = ReadOptionalStringIfColumnExists(record, "triggered_effect_id"),
                TriggeredGraphOwnerKind = ReadOptionalEnumIfColumnExists(
                    record,
                    "triggered_graph_owner_kind",
                    SkillNodeOwnerKind.Skill),
                TriggeredGraphOwnerId = ReadOptionalStringIfColumnExists(record, "triggered_graph_owner_id"),
                TriggeredGraphKind = ReadOptionalEnumIfColumnExists(
                    record,
                    "triggered_graph_kind",
                    SkillGraphKind.Effect),
                TriggeredGraphIndex = ReadOptionalIntIfColumnExists(record, "triggered_graph_index"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                SortOrder = record.ReadInt("sort_order"),
                TargetSide = record.ReadEnum<SkillMultiEffectTargetSide>("target_side"),
                TargetSelection = record.ReadEnum<SkillMultiEffectTargetSelection>("target_selection"),
                TargetShape = record.ReadEnum<SkillMultiEffectTargetShape>("target_shape"),
                CenterMode = record.ReadEnum<SkillMultiEffectCenterMode>("center_mode"),
                Attribute = ReadOptionalEnumIfColumnExists(record, "attribute", DamageAttribute.Physical),
                BaseDamage = ReadOptionalFloatIfColumnExists(record, "base_damage"),
                AttackPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "attack_power_coefficient"),
                SpellPowerCoefficient = ReadOptionalFloatIfColumnExists(record, "spell_power_coefficient"),
                DamageMultiplier = ReadOptionalFloatIfColumnExists(record, "damage_multiplier"),
                DamageSource = record.ReadEnum<SkillTriggerDamageSource>("damage_source"),
                DamageSourceMultiplier = ReadOptionalFloatIfColumnExists(record, "damage_source_multiplier"),
                TrackedAttribute = record.ReadEnum<DamageAttribute>("tracked_attribute"),
                Radius = ReadOptionalFloatIfColumnExists(record, "radius"),
                CoverAll = ReadOptionalBoolIfColumnExists(record, "cover_all"),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                RepeatCount = ReadOptionalIntIfColumnExists(record, "repeat_count"),
                RepeatIntervalSeconds = ReadOptionalFloatIfColumnExists(record, "repeat_interval_seconds"),
                TriggerDelaySeconds = ReadOptionalFloatIfColumnExists(record, "trigger_delay_seconds"),
                TriggerEveryCount = ReadOptionalIntIfColumnExists(record, "trigger_every_count"),
                EventSourceScope = ReadOptionalStringIfColumnExists(record, "event_source_scope"),
                RequireEventExecute = ReadOptionalBoolIfColumnExists(record, "require_event_execute"),
                CooldownRefundRatio = ReadOptionalFloatIfColumnExists(record, "cooldown_refund_ratio"),
                ReloadReduceRatio = ReadOptionalFloatIfColumnExists(record, "reload_reduce_ratio"),
                SkillEffectPrefabPath = ReadOptionalStringIfColumnExists(record, "skill_effect_prefab_path"),
                RuntimeVisualSpritePath = ReadOptionalStringIfColumnExists(record, "runtime_visual_sprite_path"),
                RuntimeVisualAnimatorControllerPath = ReadOptionalStringIfColumnExists(record, "runtime_visual_animator_controller_path"),
                RuntimeVisualScale = ReadOptionalFloatWithDefaultIfColumnExists(record, "runtime_visual_scale", 1f),
                RuntimeVisualSortingOrder = ReadOptionalIntIfColumnExists(record, "runtime_visual_sorting_order"),
                RuntimeVisualAnchor = ReadOptionalStringIfColumnExists(record, "runtime_visual_anchor"),
                RuntimeHitboxSizeX = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_x"),
                RuntimeHitboxSizeY = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_size_y"),
                RuntimeHitboxOffsetX = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_offset_x"),
                RuntimeHitboxOffsetY = ReadOptionalFloatIfColumnExists(record, "runtime_hitbox_offset_y"),
                RuntimeSupportState = ReadOptionalStringIfColumnExists(record, "runtime_support_state"),
                RuntimeSupportNotes = ReadOptionalStringIfColumnExists(record, "runtime_support_notes")
            };

            if (TryReadFloatIfColumnExists(record, "proc_chance", out var procChance))
            {
                row.ProcChance = procChance;
            }

            if (TryReadFloatIfColumnExists(record, "internal_cooldown_seconds", out var internalCooldownSeconds))
            {
                row.InternalCooldownSeconds = internalCooldownSeconds;
            }

            if (row.DamageMultiplier <= 0f)
            {
                row.DamageMultiplier = 1f;
            }

            if (row.ProcChance <= 0f)
            {
                row.ProcChance = 1f;
            }

            if (row.RepeatCount <= 0)
            {
                row.RepeatCount = 1;
            }

            if (row.TriggerEveryCount < 0)
            {
                row.TriggerEveryCount = 0;
            }

            return row;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static string ReadMonsterIdOrInfer(CsvRecord record, string tableName)
        {
            var monsterId = ReadOptionalStringIfColumnExists(record, "monster_id");
            if (!string.IsNullOrWhiteSpace(monsterId))
            {
                return monsterId;
            }

            monsterId = InferMonsterIdFromSplitTableName(tableName);
            if (!string.IsNullOrWhiteSpace(monsterId))
            {
                return monsterId;
            }

            throw new CsvFatalException(
                $"CSV row {record.RowNumber} in '{record.TableName}' is missing monster ownership.",
                new List<string>
                {
                    "Add a monster_id column to kind-grouped monster skill CSV files such as 'skills_projectile.csv'."
                });
        }

        /*
         * 파일명과 행 정보로 누락된 값을 판단한다.
         */
        internal static string InferMonsterIdFromSplitTableName(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                return string.Empty;
            }

            var splitIndex = tableName.IndexOf("_skills_", StringComparison.OrdinalIgnoreCase);
            if (splitIndex <= 0)
            {
                splitIndex = tableName.IndexOf("_skill_", StringComparison.OrdinalIgnoreCase);
            }

            return splitIndex > 0
                ? tableName.Substring(0, splitIndex).Trim().ToLowerInvariant()
                : string.Empty;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static PakuriCsvSkillKind ReadSkillKindOrInfer(CsvRecord record, SkillSlot slot)
        {
            return record.HasColumn("skill_kind")
                ? record.ReadEnum<PakuriCsvSkillKind>("skill_kind")
                : slot >= SkillSlot.F
                    ? PakuriCsvSkillKind.Passive
                    : PakuriCsvSkillKind.Active;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static SkillRuntimeKind ReadRuntimeKindOrInfer(CsvRecord record, SkillSlot slot)
        {
            if (record.HasColumn("runtime_kind"))
            {
                return record.ReadEnum<SkillRuntimeKind>("runtime_kind");
            }

            if (slot >= SkillSlot.F)
            {
                return SkillRuntimeKind.Passive;
            }

            throw new CsvFatalException(
                $"CSV row {record.RowNumber} in '{record.TableName}' is missing runtime_kind.",
                new List<string>
                {
                    "Active skill rows still require runtime_kind because active split files can contain multiple execution types."
                });
        }

        /*
         * 파일명과 행 정보로 누락된 값을 판단한다.
         */
        internal static SkillSlot InferRequiredActiveSlot(SkillSlot passiveSlot)
        {
            switch (passiveSlot)
            {
                case SkillSlot.G:
                    return SkillSlot.B;
                case SkillSlot.H:
                    return SkillSlot.C;
                case SkillSlot.I:
                    return SkillSlot.D;
                case SkillSlot.J:
                    return SkillSlot.E;
                default:
                    return SkillSlot.A;
            }
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadOptionalFloat(CsvRecord record, string columnName)
        {
            return TryReadFloat(record, columnName, out var value) ? value : 0f;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static int ReadOptionalInt(CsvRecord record, string columnName)
        {
            return TryReadInt(record, columnName, out var value) ? value : 0;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static int ReadOptionalIntIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? ReadOptionalInt(record, columnName) : 0;
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadIntIfColumnExists(CsvRecord record, string columnName, out int value)
        {
            value = 0;
            return record.HasColumn(columnName) && TryReadInt(record, columnName, out value);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadOptionalFloatIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? ReadOptionalFloat(record, columnName) : 0f;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static float ReadOptionalFloatWithDefaultIfColumnExists(CsvRecord record, string columnName, float fallback)
        {
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out var value)
                ? value
                : fallback;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static string ReadOptionalStringIfColumnExists(CsvRecord record, string columnName)
        {
            return record.HasColumn(columnName) ? record.ReadString(columnName) : string.Empty;
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static bool ReadOptionalBoolIfColumnExists(CsvRecord record, string columnName)
        {
            if (!record.HasColumn(columnName))
            {
                return false;
            }

            var raw = record.ReadString(columnName);
            return !string.IsNullOrWhiteSpace(raw) && record.ReadBool(columnName);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static bool ReadOptionalBoolWithDefaultIfColumnExists(CsvRecord record, string columnName, bool fallback)
        {
            if (!record.HasColumn(columnName))
            {
                return fallback;
            }

            var raw = record.ReadString(columnName);
            return string.IsNullOrWhiteSpace(raw) ? fallback : record.ReadBool(columnName);
        }

        /*
         * CSV 행에서 필요한 값을 읽는다.
         */
        internal static T ReadOptionalEnumIfColumnExists<T>(CsvRecord record, string columnName, T fallback) where T : struct
        {
            if (!record.HasColumn(columnName))
            {
                return fallback;
            }

            var raw = record.ReadString(columnName);
            return !string.IsNullOrWhiteSpace(raw) && Enum.TryParse(raw, true, out T value)
                ? value
                : fallback;
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadFloatIfColumnExists(CsvRecord record, string columnName, out float value)
        {
            value = 0f;
            return record.HasColumn(columnName) && TryReadFloat(record, columnName, out value);
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadFloat(CsvRecord record, string columnName, out float value)
        {
            var raw = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(raw))
            {
                value = 0f;
                return false;
            }

            value = record.ReadFloat(columnName);
            return true;
        }

        /*
         * 열이 존재하고 값이 있으면 CSV 값을 읽는다.
         */
        internal static bool TryReadInt(CsvRecord record, string columnName, out int value)
        {
            var raw = record.ReadString(columnName);
            if (string.IsNullOrWhiteSpace(raw))
            {
                value = 0;
                return false;
            }

            value = record.ReadInt(columnName);
            return true;
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
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

        /*
         * 적 CSV 한 행의 기본 능력치와 장착 스킬 ID를 보관한다.
         */
        internal sealed class EnemyMigrationRow
        {
            public string Id;
            public string StageId;
            public int SortOrder;
            public string DisplayName;
            public EnemyEncounterRole EncounterRole;
            public EnemyAttackType AttackType;
            public DamageAttribute Attribute;
            public float MaxHealth;
            public float AttackPower;
            public float SpellPower;
            public float MoveSpeed;
            public float CriticalChance;
            public float CriticalDamage;
            public float CriticalResistance;
            public float PhysicalDefense;
            public float FireDefense;
            public float LightningDefense;
            public float IceDefense;
            public float DarknessDefense;
            public float HolyDefense;
            public string SkillSlotAId;
            public string SkillSlotBId;
            public string PassiveId;
            public float NexusDamage;
        }

        /*
         * 적 스킬 CSV 한 행의 실행 방식과 전투 값을 보관한다.
         */
        internal sealed class EnemyBaseSkillRow
        {
            public SkillRow Skill;
            public string ExecutionProfile;
            public string TargetScope;
            public string TargetSelection;
            public float CastRange;
            public float EffectRadius;
            public float ProjectileLifetime;
            public float FlatValue;
            public float IncomingDamageMultiplier = 1f;
            public float MoveSpeedMultiplier = 1f;
            public float OutgoingDamageMultiplier = 1f;
            public float ChainDamageMultiplier;
            public float ChainDelaySeconds;
            public float ChainRadius;
            public bool ExcludePrimaryTarget;
            public float StatusActionSpeedBonus;
            public float StatusDurationSeconds;
            public float TargetMaxHealthRatio;
            public string HitTargetCount;
            public float ChargeRampSeconds = 3f;
            public float ChargeMoveSpeedMultiplier = 2.5f;
            public EnemyPassiveTarget PassiveApplyTarget = EnemyPassiveTarget.Self;
            public EnemyPassiveModifierKind PassiveModifierKind;
            public float PassiveModifierValue;
        }

        /*
         * 적 스킬 Trigger 한 행의 실행 대상과 순서를 보관한다.
         */
        internal sealed class EnemyMigrationTriggerRow
        {
            public string Id;
            public string SourceSkillId;
            public SkillTriggerEvent TriggerEvent;
            public string TriggeredSkillId;
            public SkillRuntimeKind RuntimeKind;
            public int SortOrder;
            public bool Enabled;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyMigrationRow ParseEnemyMigrationRow(CsvRecord record)
        {
            return new EnemyMigrationRow
            {
                Id = record.ReadRequiredString("enemy_id"),
                StageId = record.ReadRequiredString("stage_id"),
                SortOrder = record.ReadInt("sort_order"),
                DisplayName = record.ReadRequiredString("display_name"),
                EncounterRole = record.ReadEnum<EnemyEncounterRole>("encounter_role"),
                AttackType = record.ReadEnum<EnemyAttackType>("attack_type"),
                Attribute = record.ReadEnum<DamageAttribute>("attribute"),
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
                SkillSlotAId = record.ReadRequiredString("skill_slot_a_id"),
                SkillSlotBId = record.ReadRequiredString("skill_slot_b_id"),
                PassiveId = record.ReadRequiredString("passive_id"),
                NexusDamage = record.ReadFloat("nexus_damage")
            };
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyBaseSkillRow ParseEnemyBaseSkillRow(CsvRecord record, string tableName)
        {
            if (string.Equals(tableName, "skills_passive.csv", StringComparison.OrdinalIgnoreCase))
            {
                return ParseEnemyPassiveSkillRow(record);
            }

            if (record.HasColumn("runtime_hitbox_offset_x") || record.HasColumn("runtime_hitbox_offset_y"))
            {
                throw new CsvFatalException(
                    $"CSV table '{tableName}' must not define runtime hitbox offset columns. Enemy runtime hitboxes are centered at (0,0).");
            }

            var row = new EnemyBaseSkillRow
            {
                Skill = ParseSkillRow(record, tableName, "enemy-shared"),
                ExecutionProfile = ReadOptionalStringIfColumnExists(record, "execution_profile"),
                TargetScope = ReadOptionalStringIfColumnExists(record, "target_scope"),
                TargetSelection = ReadOptionalStringIfColumnExists(record, "target_selection"),
                CastRange = ReadOptionalFloatIfColumnExists(record, "cast_range"),
                EffectRadius = ReadOptionalFloatIfColumnExists(record, "effect_radius"),
                ProjectileLifetime = ReadOptionalFloatIfColumnExists(record, "projectile_lifetime"),
                FlatValue = ReadOptionalFloatIfColumnExists(record, "flat_value"),
                IncomingDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "incoming_damage_multiplier", 1f),
                MoveSpeedMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "move_speed_multiplier", 1f),
                OutgoingDamageMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "outgoing_damage_multiplier", 1f),
                ChainDamageMultiplier = ReadOptionalFloatIfColumnExists(record, "chain_damage_multiplier"),
                ChainDelaySeconds = ReadOptionalFloatIfColumnExists(record, "chain_delay_seconds"),
                ChainRadius = ReadOptionalFloatIfColumnExists(record, "chain_radius"),
                ExcludePrimaryTarget = ReadOptionalBoolIfColumnExists(record, "exclude_primary_target"),
                StatusActionSpeedBonus = ReadOptionalFloatIfColumnExists(record, "status_action_speed_bonus"),
                StatusDurationSeconds = ReadOptionalFloatIfColumnExists(record, "status_duration_seconds"),
                TargetMaxHealthRatio = ReadOptionalFloatIfColumnExists(record, "target_max_health_ratio"),
                HitTargetCount = ReadOptionalStringIfColumnExists(record, "hit_target_count"),
                ChargeRampSeconds = ReadOptionalFloatWithDefaultIfColumnExists(record, "charge_ramp_seconds", 3f),
                ChargeMoveSpeedMultiplier = ReadOptionalFloatWithDefaultIfColumnExists(record, "charge_move_speed_multiplier", 2.5f)
            };

            if (row.Skill.SkillKind == PakuriCsvSkillKind.Passive
                || row.Skill.RuntimeKind == SkillRuntimeKind.Passive)
            {
                throw new CsvFatalException(
                    $"CSV table '{tableName}' contains passive skill '{row.Skill.Id}'. Enemy passive rows must be authored in 'skills_passive.csv'.");
            }

            return row;
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyBaseSkillRow ParseEnemyPassiveSkillRow(CsvRecord record)
        {
            return new EnemyBaseSkillRow
            {
                Skill = new SkillRow
                {
                    Id = record.ReadRequiredString("skill_id"),
                    MonsterId = "enemy-shared",
                    SkillKind = PakuriCsvSkillKind.Passive,
                    Slot = SkillSlot.F,
                    DisplayName = record.ReadRequiredString("display_name"),
                    RuntimeKind = SkillRuntimeKind.Passive,
                    ImplementationState = SkillImplementationState.RuntimeImplemented,
                    IsAvailableWithoutActiveRequirement = true
                },
                PassiveApplyTarget = record.ReadEnum<EnemyPassiveTarget>("apply_target"),
                PassiveModifierKind = record.ReadEnum<EnemyPassiveModifierKind>("modifier_kind"),
                PassiveModifierValue = record.ReadFloat("modifier_value")
            };
        }

        /*
         * CSV 행을 실행에 사용할 자료로 변환한다.
         */
        internal static EnemyMigrationTriggerRow ParseEnemyMigrationTriggerRow(CsvRecord record)
        {
            return new EnemyMigrationTriggerRow
            {
                Id = record.ReadRequiredString("trigger_id"),
                SourceSkillId = record.ReadRequiredString("source_skill_id"),
                TriggerEvent = record.ReadEnum<SkillTriggerEvent>("trigger_event"),
                TriggeredSkillId = record.ReadRequiredString("triggered_skill_id"),
                RuntimeKind = record.ReadEnum<SkillRuntimeKind>("runtime_kind"),
                SortOrder = record.ReadInt("sort_order"),
                Enabled = record.ReadBool("enabled")
            };
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemyMigrationRows(SourceModel model, List<string> errors)
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

            foreach (var trigger in model.EnemyMigrationTriggers.Values)
            {
                if (!model.EnemyBaseSkills.TryGetValue(trigger.SourceSkillId, out var sourceSkill)
                    || sourceSkill == null
                    || sourceSkill.Skill == null)
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown source skill '{trigger.SourceSkillId}'.");
                }
                else if (trigger.RuntimeKind != sourceSkill.Skill.RuntimeKind)
                {
                    errors.Add(
                        $"Enemy trigger '{trigger.Id}' runtime_kind '{trigger.RuntimeKind}' does not match source skill kind '{sourceSkill.Skill.RuntimeKind}'.");
                }

                if (!model.EnemyBaseSkills.ContainsKey(trigger.TriggeredSkillId))
                {
                    errors.Add($"Enemy trigger '{trigger.Id}' references unknown triggered skill '{trigger.TriggeredSkillId}'.");
                }
            }

            ValidateEnemyCombatStartTrigger(model, "OpeningCharge", SkillRuntimeKind.Buff, errors);
            ValidateEnemyCombatStartTrigger(model, "Intimidation", SkillRuntimeKind.Buff, errors);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemySkillSlot(
            SourceModel model,
            EnemyMigrationRow enemy,
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

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemyPassive(
            SourceModel model,
            EnemyMigrationRow enemy,
            HashSet<string> referencedPassiveIds,
            List<string> errors)
        {
            var passiveId = enemy.PassiveId != null ? enemy.PassiveId.Trim() : string.Empty;
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

            if (passive.PassiveApplyTarget != EnemyPassiveTarget.Self)
            {
                errors.Add($"Enemy passive '{passiveId}' has unsupported apply_target '{passive.PassiveApplyTarget}'.");
            }

            if (passive.PassiveModifierKind == EnemyPassiveModifierKind.None)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a supported modifier_kind.");
            }

            if (passive.PassiveModifierValue <= 0f)
            {
                errors.Add($"Enemy passive '{passiveId}' requires a positive modifier_value.");
            }

            referencedPassiveIds.Add(passiveId);
        }

        /*
         * 입력값과 참조 관계가 올바른지 검사한다.
         */
        internal static void ValidateEnemyCombatStartTrigger(
            SourceModel model,
            string skillId,
            SkillRuntimeKind runtimeKind,
            List<string> errors)
        {
            var count = 0;
            foreach (var trigger in model.EnemyMigrationTriggers.Values)
            {
                if (trigger.Enabled
                    && trigger.TriggerEvent == SkillTriggerEvent.CombatStart
                    && string.Equals(trigger.SourceSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(trigger.TriggeredSkillId, skillId, StringComparison.OrdinalIgnoreCase)
                    && trigger.RuntimeKind == runtimeKind)
                {
                    count++;
                }
            }

            if (count != 1)
            {
                errors.Add($"Enemy skill '{skillId}' requires exactly one enabled CombatStart trigger; found '{count}'.");
            }
        }
        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static SourceModel LoadSourceModel(CsvRuntimeCatalog sourceCatalog)
        {
            var model = new SourceModel();

            var catalogMonsterTable = CsvTable.Load(sourceCatalog.CatalogMonsters, CatalogMonstersFileName);
            var monsterTable = CsvTable.Load(sourceCatalog.Monsters, MonstersFileName);
            var rewardChoiceTable = CsvTable.Load(sourceCatalog.MonsterRewardChoices, MonsterRewardChoicesFileName);
            var projectileSkillAssets = sourceCatalog.MonsterSkillsProjectileFiles;
            var lineAttackSkillAssets = sourceCatalog.MonsterSkillsLineAttackFiles;
            var areaAttackSkillAssets = sourceCatalog.MonsterSkillsAreaAttackFiles;
            var singleAttackSkillAssets = sourceCatalog.MonsterSkillsSingleAttackFiles;
            var buffSkillAssets = sourceCatalog.MonsterSkillsBuffFiles;
            var passiveSkillAssets = sourceCatalog.MonsterSkillsPassiveFiles;
            var skillTriggerAssets = sourceCatalog.MonsterSkillTriggerFiles;
            var skillGraphNodeAssets = sourceCatalog.MonsterSkillGraphNodeFiles;
            var skillNodeDefinitionTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitions,
                MonsterSkillNodeDefinitionsFileName);
            var skillNodeDefinitionParamTable = CsvTable.Load(
                sourceCatalog.MonsterSkillNodeDefinitionParams,
                MonsterSkillNodeDefinitionParamsFileName);
            var projectileChoiceAssets = sourceCatalog.MonsterSkillChoicesProjectileFiles;
            var lineAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesLineAttackFiles;
            var areaAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesAreaAttackFiles;
            var singleAttackChoiceAssets = sourceCatalog.MonsterSkillChoicesSingleAttackFiles;
            var buffChoiceAssets = sourceCatalog.MonsterSkillChoicesBuffFiles;
            var passiveChoiceAssets = sourceCatalog.MonsterSkillChoicesPassiveFiles;
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
                projectileSkillAssets,
                MonsterSkillsProjectileFileName,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillRows(
                model,
                lineAttackSkillAssets,
                MonsterSkillsLineAttackFileName,
                SkillRuntimeKind.LineAttack);
            LoadSkillRows(
                model,
                areaAttackSkillAssets,
                MonsterSkillsAreaAttackFileName,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillRows(
                model,
                singleAttackSkillAssets,
                MonsterSkillsSingleAttackFileName,
                SkillRuntimeKind.SingleAttack);
            LoadSkillRows(
                model,
                buffSkillAssets,
                MonsterSkillsBuffFileName,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillRows(
                model,
                passiveSkillAssets,
                MonsterSkillsPassiveFileName,
                SkillRuntimeKind.Passive);

            foreach (var record in skillNodeDefinitionTable.Records)
            {
                var row = ParseSkillNodeTypeRow(record);
                AddUnique(model.SkillNodeTypes, row.Id, row, record);
            }

            foreach (var record in skillNodeDefinitionParamTable.Records)
            {
                model.SkillNodeTypeParams.Add(ParseSkillNodeTypeParamRow(record));
            }

            for (var assetIndex = 0; assetIndex < skillTriggerAssets.Length; assetIndex++)
            {
                var skillTriggerTable = CsvTable.Load(
                    skillTriggerAssets[assetIndex],
                    GetTextAssetCsvTableName(skillTriggerAssets[assetIndex], MonsterSkillTriggersFileName));
                foreach (var record in skillTriggerTable.Records)
                {
                    var row = ParseSkillTriggerRow(record, skillTriggerTable.TableName);
                    AddUnique(model.SkillTriggers, row.Id, row, record);
                }
            }

            LoadSkillChoiceRows(
                model,
                projectileChoiceAssets,
                MonsterSkillChoicesProjectileFileName,
                SkillRuntimeKind.MagazineProjectile,
                SkillRuntimeKind.CooldownProjectile);
            LoadSkillChoiceRows(
                model,
                lineAttackChoiceAssets,
                MonsterSkillChoicesLineAttackFileName,
                SkillRuntimeKind.LineAttack);
            LoadSkillChoiceRows(
                model,
                areaAttackChoiceAssets,
                MonsterSkillChoicesAreaAttackFileName,
                SkillRuntimeKind.AreaAttack,
                SkillRuntimeKind.Field);
            LoadSkillChoiceRows(
                model,
                singleAttackChoiceAssets,
                MonsterSkillChoicesSingleAttackFileName,
                SkillRuntimeKind.SingleAttack);
            LoadSkillChoiceRows(
                model,
                buffChoiceAssets,
                MonsterSkillChoicesBuffFileName,
                SkillRuntimeKind.Buff,
                SkillRuntimeKind.Shield);
            LoadSkillChoiceRows(
                model,
                passiveChoiceAssets,
                MonsterSkillChoicesPassiveFileName,
                SkillRuntimeKind.Passive);

            for (var assetIndex = 0; assetIndex < skillGraphNodeAssets.Length; assetIndex++)
            {
                var graphNodeTable = CsvTable.Load(
                    skillGraphNodeAssets[assetIndex],
                    GetTextAssetCsvTableName(skillGraphNodeAssets[assetIndex], "skill_graph_nodes.csv"));
                foreach (var record in graphNodeTable.Records)
                {
                    model.SkillGraphNodes.Add(ParseSkillGraphNodeRow(record));
                }
            }

            MaterializeSkillGraphRows(model);

            foreach (var record in statusEffectTable.Records)
            {
                var row = ParseStatusEffectRow(record);
                AddUnique(model.StatusEffects, row.Id, row, record);
            }

            foreach (var record in enemyTable.Records)
            {
                var row = ParseEnemyMigrationRow(record);
                AddUnique(model.Enemies, row.Id, row, record);
            }

            var enemyBaseAssets = sourceCatalog.EnemySkillBaseFiles ?? Array.Empty<TextAsset>();
            for (var assetIndex = 0; assetIndex < enemyBaseAssets.Length; assetIndex++)
            {
                var asset = enemyBaseAssets[assetIndex];
                var tableName = GetTextAssetCsvTableName(asset, "enemy_base_skills.csv");
                var table = CsvTable.Load(asset, tableName);
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyBaseSkillRow(record, tableName);
                    AddUnique(model.EnemyBaseSkills, row.Skill.Id, row, record);
                }
            }

            var enemyTriggerAssets = sourceCatalog.EnemySkillTriggerFiles ?? Array.Empty<TextAsset>();
            for (var assetIndex = 0; assetIndex < enemyTriggerAssets.Length; assetIndex++)
            {
                var asset = enemyTriggerAssets[assetIndex];
                var table = CsvTable.Load(asset, GetTextAssetCsvTableName(asset, "enemy_skill_triger.csv"));
                foreach (var record in table.Records)
                {
                    var row = ParseEnemyMigrationTriggerRow(record);
                    AddUnique(model.EnemyMigrationTriggers, row.Id, row, record);
                }
            }

            return model;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillRows(
            SourceModel model,
            TextAsset[] skillAssets,
            string fallbackTableName,
            params SkillRuntimeKind[] allowedRuntimeKinds)
        {
            for (var assetIndex = 0; assetIndex < skillAssets.Length; assetIndex++)
            {
                var skillAsset = skillAssets[assetIndex];
                LoadSkillRows(
                    model,
                    skillAsset,
                    GetTextAssetCsvTableName(skillAsset, fallbackTableName),
                    allowedRuntimeKinds);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillRows(
            SourceModel model,
            TextAsset skillAsset,
            string tableName,
            params SkillRuntimeKind[] allowedRuntimeKinds)
        {
            var skillTable = CsvTable.Load(skillAsset, tableName);
            foreach (var record in skillTable.Records)
            {
                var row = ParseSkillRow(record, tableName);
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillChoiceRows(
            SourceModel model,
            TextAsset[] choiceAssets,
            string fallbackTableName,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds)
        {
            for (var assetIndex = 0; assetIndex < choiceAssets.Length; assetIndex++)
            {
                var choiceAsset = choiceAssets[assetIndex];
                LoadSkillChoiceRows(
                    model,
                    choiceAsset,
                    GetTextAssetCsvTableName(choiceAsset, fallbackTableName),
                    allowedOwnerRuntimeKinds);
            }
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static void LoadSkillChoiceRows(
            SourceModel model,
            TextAsset choiceAsset,
            string tableName,
            params SkillRuntimeKind[] allowedOwnerRuntimeKinds)
        {
            var choiceTable = CsvTable.Load(choiceAsset, tableName);
            foreach (var record in choiceTable.Records)
            {
                var row = ParseSkillChoiceRow(record, tableName);
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

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        internal static bool IsAllowedSkillRuntimeKind(
            SkillRuntimeKind runtimeKind,
            SkillRuntimeKind[] allowedRuntimeKinds)
        {
            for (var i = 0; i < allowedRuntimeKinds.Length; i++)
            {
                if (allowedRuntimeKinds[i] == runtimeKind)
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * TextAsset 이름으로 CSV 테이블 이름을 만든다.
         */
        internal static string GetTextAssetCsvTableName(TextAsset asset, string fallbackTableName)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.name))
            {
                return fallbackTableName;
            }

            return asset.name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? asset.name
                : asset.name + ".csv";
        }

        /*
         * 중복 ID를 거부하고 원본 행을 사전에 추가한다.
         */
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
