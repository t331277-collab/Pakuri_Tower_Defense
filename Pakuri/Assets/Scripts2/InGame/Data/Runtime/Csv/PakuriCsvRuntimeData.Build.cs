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
                monster.SkillTriggers = BuildSkillTriggers(model, sourceMonster.Id);
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
            catalog.StatusEffects = BuildStatusEffects(model);
            return catalog;
        }

        private static StatusEffectDefinitionData[] BuildStatusEffects(SourceModel model)
        {
            var statuses = new List<StatusEffectDefinitionData>();
            foreach (var row in model.StatusEffects.Values)
            {
                statuses.Add(new StatusEffectDefinitionData
                {
                    StatusEffectId = row.Id,
                    StatusEffectLabel = row.Label,
                    Classification = row.Classification,
                    HasAttribute = row.HasAttribute,
                    Attribute = row.Attribute,
                    DefaultDurationSeconds = row.DefaultDurationSeconds,
                    IsPermanent = row.IsPermanent,
                    MaxStacks = row.MaxStacks,
                    BaseStackAmount = row.BaseStackAmount > 0 ? row.BaseStackAmount : 1,
                    CanMove = row.CanMove,
                    CanAct = row.CanAct,
                    CanUseSpecialSkill = row.CanUseSpecialSkill,
                    ActionSpeedBonusPerStack = row.ActionSpeedBonusPerStack,
                    MoveSpeedBonusPerStack = row.MoveSpeedBonusPerStack,
                    AttackPowerBonusPerStack = row.AttackPowerBonusPerStack,
                    DamageTakenBonusPerStack = row.DamageTakenBonusPerStack,
                    CriticalDamageTakenBonusPerStack = row.CriticalDamageTakenBonusPerStack,
                    CriticalResistanceBonusPerStack = row.CriticalResistanceBonusPerStack,
                    ElementResistReductionPerStack = row.ElementResistReductionPerStack,
                    ElementDamageTakenBonusPerStack = row.ElementDamageTakenBonusPerStack,
                    StatusEffectPrefab = LoadPrefab(row.StatusEffectPrefabPath)
                });
            }

            statuses.Sort((left, right) => string.Compare(left.StatusEffectId, right.StatusEffectId, StringComparison.OrdinalIgnoreCase));
            return statuses.ToArray();
        }

        private static MonsterDefinition.RewardChoiceDefinition[] BuildRewardChoices(SourceModel model, string monsterId)
        {
            var rewards = FilterAndSort(
                model.RewardChoices.Values,
                reward => string.Equals(reward.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new MonsterDefinition.RewardChoiceDefinition[rewards.Count];
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                definitions[i] = new MonsterDefinition.RewardChoiceDefinition
                {
                    RewardId = reward.Id,
                    ActiveSkillId = reward.ActiveSkillId,
                    PassiveSkillId = reward.PassiveSkillId
                };
            }

            return definitions;
        }

        private static SkillDefinition[] BuildActiveSkills(SourceModel model, string monsterId)
        {
            var skills = FilterAndSort(
                model.Skills.Values,
                skill => skill.SkillKind == PakuriCsvSkillKind.Active
                    && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new SkillDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                var definition = new SkillDefinition
                {
                    SkillId = skill.Id,
                    DisplayName = skill.DisplayName,
                    Slot = skill.Slot,
                    RuntimeKind = skill.RuntimeKind,
                    ImplementationState = skill.ImplementationState,
                    IsDefaultLearned = skill.IsDefaultLearned,
                    SkillIcon = LoadSprite(skill.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(skill.SkillEffectPrefabPath),
                    DescriptionText = skill.DescriptionText,
                    Attribute = skill.Attribute,
                    BaseDamage = skill.BaseDamage,
                    AttackPowerCoefficient = skill.AttackPowerCoefficient,
                    SpellPowerCoefficient = skill.SpellPowerCoefficient,
                    Radius = skill.Radius,
                    KnockbackDistance = skill.KnockbackDistance,
                    DamageDelaySeconds = skill.DamageDelaySeconds,
                    ExecuteHealthRatioThreshold = skill.ExecuteHealthRatioThreshold,
                    RequireExecuteThresholdToCast = skill.RequireExecuteThresholdToCast,
                    ExecuteDamageMultiplier = skill.ExecuteDamageMultiplier,
                    KillCooldownRefundRatio = skill.KillCooldownRefundRatio,
                    BossDamageMultiplier = skill.BossDamageMultiplier,
                    HitTargetCount = skill.HitTargetCount,
                    TargetSelection = skill.TargetSelection,
                    CooldownSeconds = skill.CooldownSeconds,
                    ActiveDurationSeconds = skill.ActiveDurationSeconds,
                    MagazineCapacity = skill.MagazineCapacity,
                    ReloadSeconds = skill.ReloadSeconds,
                    ShotIntervalSeconds = skill.ShotIntervalSeconds,
                    ProjectileBurstCount = skill.ProjectileBurstCount,
                    ProjectileSpeed = skill.ProjectileSpeed,
                    PierceCount = skill.PierceCount,
                    CriticalAllowed = skill.CriticalAllowed,
                    Summary = skill.Summary,
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.ActiveEnhancement),
                    MasterSkillChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.ActiveMaster),
                    MultiEffects = BuildSkillEffects(model, skill.Id)
                };

                ApplyStatusPayload(definition, skill.Status);
                definitions[i] = definition;
            }

            return definitions;
        }

        private static SkillEffectDefinition[] BuildSkillEffects(SourceModel model, string skillId)
        {
            var effects = FilterAndSort(
                model.SkillEffects.Values,
                effect => string.Equals(effect.SkillId, skillId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillEffectDefinition[effects.Count];
            for (var i = 0; i < effects.Count; i++)
            {
                var effect = effects[i];
                var definition = new SkillEffectDefinition
                {
                    EffectId = effect.Id,
                    SkillId = effect.SkillId,
                    SortOrder = effect.SortOrder,
                    EffectKind = effect.EffectKind,
                    TargetSide = effect.TargetSide,
                    TargetSelection = effect.TargetSelection,
                    TargetShape = effect.TargetShape,
                    CenterMode = effect.CenterMode,
                    VisualAnchorMode = effect.VisualAnchorMode,
                    EffectTiming = effect.EffectTiming,
                    DelaySeconds = effect.DelaySeconds,
                    EnabledByDefault = effect.EnabledByDefault,
                    RequiresActiveChoiceId = effect.RequiresActiveChoiceId,
                    ExcludesActiveChoiceId = effect.ExcludesActiveChoiceId,
                    RequiresPassiveSkillId = effect.RequiresPassiveSkillId,
                    ExcludesPassiveSkillId = effect.ExcludesPassiveSkillId,
                    ApplyOnce = effect.ApplyOnce,
                    ConditionStatusId = effect.ConditionStatusId,
                    ConditionTargetSide = effect.ConditionTargetSide,
                    ConditionSkillAttribute = effect.ConditionSkillAttribute,
                    ConditionHealthRatioMax = effect.ConditionHealthRatioMax,
                    ConditionHitCountMin = effect.ConditionHitCountMin,
                    Attribute = effect.Attribute,
                    BaseDamage = effect.BaseDamage,
                    AttackPowerCoefficient = effect.AttackPowerCoefficient,
                    SpellPowerCoefficient = effect.SpellPowerCoefficient,
                    DamageMultiplier = effect.DamageMultiplier,
                    Radius = effect.Radius,
                    CoverAll = effect.CoverAll,
                    ActiveDurationSeconds = effect.ActiveDurationSeconds,
                    TickIntervalSeconds = effect.TickIntervalSeconds,
                    SkillEffectPrefab = LoadPrefab(effect.SkillEffectPrefabPath),
                    RuntimeSupportState = effect.RuntimeSupportState,
                    RuntimeSupportNotes = effect.RuntimeSupportNotes
                };

                ApplyStatusPayload(definition, effect.Status);
                definitions[i] = definition;
            }

            return definitions;
        }

        private static SkillTriggerDefinition[] BuildSkillTriggers(SourceModel model, string monsterId)
        {
            var triggers = FilterAndSort(
                model.SkillTriggers.Values,
                trigger => string.Equals(trigger.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillTriggerDefinition[triggers.Count];
            for (var i = 0; i < triggers.Count; i++)
            {
                var trigger = triggers[i];
                definitions[i] = new SkillTriggerDefinition
                {
                    TriggerId = trigger.Id,
                    MonsterId = trigger.MonsterId,
                    SourceSkillId = trigger.SourceSkillId,
                    TriggerEvent = trigger.TriggerEvent,
                    RequiresActiveChoiceId = trigger.RequiresActiveChoiceId,
                    ExcludesActiveChoiceId = trigger.ExcludesActiveChoiceId,
                    ConditionStatusId = trigger.ConditionStatusId,
                    ConditionStatusSourceSkillId = trigger.ConditionStatusSourceSkillId,
                    TriggerAttribute = trigger.TriggerAttribute,
                    TriggerAction = trigger.TriggerAction,
                    EventSkillId = trigger.EventSkillId,
                    ProcChance = trigger.ProcChance,
                    InternalCooldownSeconds = trigger.InternalCooldownSeconds,
                    TriggeredSkillId = trigger.TriggeredSkillId,
                    TargetSkillId = trigger.TargetSkillId,
                    TriggeredEffectId = trigger.TriggeredEffectId,
                    RuntimeKind = trigger.RuntimeKind,
                    SortOrder = trigger.SortOrder,
                    TargetSide = trigger.TargetSide,
                    TargetSelection = trigger.TargetSelection,
                    TargetShape = trigger.TargetShape,
                    CenterMode = trigger.CenterMode,
                    Attribute = trigger.Attribute,
                    BaseDamage = trigger.BaseDamage,
                    AttackPowerCoefficient = trigger.AttackPowerCoefficient,
                    SpellPowerCoefficient = trigger.SpellPowerCoefficient,
                    DamageMultiplier = trigger.DamageMultiplier,
                    DamageSource = trigger.DamageSource,
                    DamageSourceMultiplier = trigger.DamageSourceMultiplier,
                    TrackedAttribute = trigger.TrackedAttribute,
                    Radius = trigger.Radius,
                    CoverAll = trigger.CoverAll,
                    HitTargetCount = trigger.HitTargetCount,
                    RepeatCount = trigger.RepeatCount,
                    RepeatIntervalSeconds = trigger.RepeatIntervalSeconds,
                    TriggerDelaySeconds = trigger.TriggerDelaySeconds,
                    TriggerEveryCount = trigger.TriggerEveryCount,
                    EventSourceScope = trigger.EventSourceScope,
                    RequireEventExecute = trigger.RequireEventExecute,
                    CooldownRefundRatio = trigger.CooldownRefundRatio,
                    ReloadReduceRatio = trigger.ReloadReduceRatio,
                    SkillEffectPrefab = LoadPrefab(trigger.SkillEffectPrefabPath),
                    RuntimeSupportState = trigger.RuntimeSupportState,
                    RuntimeSupportNotes = trigger.RuntimeSupportNotes
                };
            }

            return definitions;
        }

        private static PassiveDefinition[] BuildPassiveSkills(SourceModel model, string monsterId)
        {
            var skills = FilterAndSort(
                model.Skills.Values,
                skill => skill.SkillKind == PakuriCsvSkillKind.Passive
                    && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.Slot.CompareTo(right.Slot));

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
                    BaseModifierChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.PassiveBase),
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.PassiveEnhancement),
                    PassiveEffects = BuildSkillEffects(model, skill.Id)
                };
            }

            return definitions;
        }

        private static SkillChoiceDefinition[] BuildSkillChoices(SourceModel model, string skillId, PakuriCsvChoiceGroup choiceGroup)
        {
            var choices = FilterAndSort(
                model.SkillChoices.Values,
                choice => choice.ChoiceGroup == choiceGroup
                    && string.Equals(choice.SkillId, skillId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillChoiceDefinition[choices.Count];
            for (var i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                definitions[i] = new SkillChoiceDefinition
                {
                    ChoiceId = choice.Id,
                    MonsterId = choice.MonsterId,
                    SkillId = choice.SkillId,
                    TargetSkillId = string.IsNullOrWhiteSpace(choice.TargetSkillId) ? choice.SkillId : choice.TargetSkillId,
                    ChoiceGroup = MapChoiceGroup(choice.ChoiceGroup),
                    Title = choice.Title,
                    SkillIcon = LoadSprite(choice.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(choice.SkillEffectPrefabPath),
                    DescriptionText = choice.DescriptionText,
                    HasDamageMultiplier = choice.HasDamageMultiplier,
                    DamageMultiplier = choice.HasDamageMultiplier ? choice.DamageMultiplier : 1f,
                    BaseDamageBonus = choice.BaseDamageBonus,
                    HasCooldownMultiplier = choice.HasCooldownMultiplier,
                    CooldownMultiplier = choice.HasCooldownMultiplier ? choice.CooldownMultiplier : 1f,
                    HasMagazineBonus = choice.HasMagazineBonus,
                    MagazineBonus = choice.MagazineBonus,
                    AdditionalProjectileBonus = choice.AdditionalProjectileBonus,
                    PierceBonus = choice.PierceBonus,
                    HasShotIntervalMultiplier = choice.HasShotIntervalMultiplier,
                    ShotIntervalMultiplier = choice.HasShotIntervalMultiplier ? choice.ShotIntervalMultiplier : 1f,
                    HasReloadTimeMultiplier = choice.HasReloadTimeMultiplier,
                    ReloadTimeMultiplier = choice.HasReloadTimeMultiplier ? choice.ReloadTimeMultiplier : 1f,
                    HasRadiusMultiplier = choice.HasRadiusMultiplier,
                    RadiusMultiplier = choice.HasRadiusMultiplier ? choice.RadiusMultiplier : 1f,
                    RadiusBonus = choice.RadiusBonus,
                    BeamWidthBonus = choice.BeamWidthBonus,
                    HasKnockbackDistanceMultiplier = choice.HasKnockbackDistanceMultiplier,
                    KnockbackDistanceMultiplier = choice.HasKnockbackDistanceMultiplier ? choice.KnockbackDistanceMultiplier : 1f,
                    HasDamageDelayMultiplier = choice.HasDamageDelayMultiplier,
                    DamageDelayMultiplier = choice.HasDamageDelayMultiplier ? choice.DamageDelayMultiplier : 1f,
                    HasExecuteHealthRatioBonus = choice.HasExecuteHealthRatioBonus,
                    ExecuteHealthRatioBonus = choice.ExecuteHealthRatioBonus,
                    HasDurationMultiplier = choice.HasDurationMultiplier,
                    DurationMultiplier = choice.HasDurationMultiplier ? choice.DurationMultiplier : 1f,
                    DurationBonus = choice.DurationBonus,
                    BranchChanceBonus = choice.BranchChanceBonus,
                    HasBranchChanceSet = choice.HasBranchChanceSet,
                    BranchChanceSet = choice.BranchChanceSet,
                    HasBranchCount = choice.HasBranchCount,
                    BranchCount = choice.BranchCount,
                    HasBranchDamageMultiplier = choice.HasBranchDamageMultiplier,
                    BranchDamageMultiplier = choice.HasBranchDamageMultiplier ? choice.BranchDamageMultiplier : 1f,
                    HasBranchSearchRadius = choice.HasBranchSearchRadius,
                    BranchSearchRadius = choice.BranchSearchRadius,
                    BranchLaunchPeriod = choice.BranchLaunchPeriod,
                    HasBranchLaunchChanceSet = choice.HasBranchLaunchChanceSet,
                    BranchLaunchChanceSet = choice.BranchLaunchChanceSet,
                    HasMaxHealthBonus = choice.HasMaxHealthBonus,
                    MaxHealthBonus = choice.MaxHealthBonus,
                    HitTargetCountBonus = choice.HitTargetCountBonus,
                    CritChanceBonus = choice.CritChanceBonus,
                    CritDamageBonus = choice.CritDamageBonus,
                    ExecuteCritChanceBonus = choice.ExecuteCritChanceBonus,
                    HasBossDamageMultiplier = choice.HasBossDamageMultiplier,
                    BossDamageMultiplier = choice.HasBossDamageMultiplier ? choice.BossDamageMultiplier : 1f,
                    HasKillCooldownRefundRatioBonus = choice.HasKillCooldownRefundRatioBonus,
                    KillCooldownRefundRatioBonus = choice.KillCooldownRefundRatioBonus,
                    KillResetsCooldown = choice.KillResetsCooldown,
                    KillResetsCooldownRequiresExecute = choice.KillResetsCooldownRequiresExecute,
                    StatusTag = choice.StatusTag,
                    HasStatusChanceBonus = choice.HasStatusChanceBonus,
                    StatusChanceBonus = choice.StatusChanceBonus,
                    StatusStacksBonus = choice.StatusStacksBonus,
                    HasStatusStacksSet = choice.HasStatusStacksSet,
                    StatusStacksSet = choice.StatusStacksSet,
                    HasStatusElementDamageTakenBonus = choice.HasStatusElementDamageTakenBonus,
                    StatusElementDamageTakenBonus = choice.StatusElementDamageTakenBonus,
                    HasStatusCriticalDamageTakenBonus = choice.HasStatusCriticalDamageTakenBonus,
                    StatusCriticalDamageTakenBonus = choice.StatusCriticalDamageTakenBonus,
                    HasStatusAilmentResistanceBonus = choice.HasStatusAilmentResistanceBonus,
                    StatusAilmentResistanceBonus = choice.StatusAilmentResistanceBonus,
                    StatusMaxStacksBonusStatusId = choice.StatusMaxStacksBonusStatusId,
                    StatusMaxStacksBonus = choice.StatusMaxStacksBonus,
                    StatusDurationBonusStatusId = choice.StatusDurationBonusStatusId,
                    StatusDurationBonus = choice.StatusDurationBonus,
                    ThresholdStatusId = choice.ThresholdStatusId,
                    ThresholdStatusMinStacks = choice.ThresholdStatusMinStacks,
                    ThresholdApplyStatusId = choice.ThresholdApplyStatusId,
                    HasConditionalDamageMultiplier = choice.HasConditionalDamageMultiplier,
                    ConditionalDamageMultiplier = choice.HasConditionalDamageMultiplier ? choice.ConditionalDamageMultiplier : 1f,
                    ConditionalTargetStatusId = choice.ConditionalTargetStatusId,
                    ConditionalTargetStatusMinStacks = choice.ConditionalTargetStatusMinStacks,
                    CountStatusId = choice.CountStatusId,
                    CountTargetSide = choice.CountTargetSide,
                    DamageMultiplierPerCount = choice.DamageMultiplierPerCount,
                    CountMax = choice.CountMax,
                    ConsecutiveHitBonusRate = choice.ConsecutiveHitBonusRate,
                    ConsecutiveHitMax = choice.ConsecutiveHitMax,
                    HasStatusConditionalDamageTakenBonus = choice.HasStatusConditionalDamageTakenBonus,
                    StatusConditionalDamageTakenBonus = choice.StatusConditionalDamageTakenBonus,
                    StatusConditionalSourceStatusId = choice.StatusConditionalSourceStatusId,
                    HasOnHitAdditionalDamage = choice.HasOnHitAdditionalDamage,
                    OnHitAdditionalDamageChance = choice.OnHitAdditionalDamageChance,
                    OnHitAdditionalDamageMultiplier = choice.HasOnHitAdditionalDamage && choice.OnHitAdditionalDamageMultiplier > 0f ? choice.OnHitAdditionalDamageMultiplier : 1f,
                    OnHitAdditionalDamageAttribute = choice.OnHitAdditionalDamageAttribute,
                    OnHitAdditionalDamageTarget = choice.OnHitAdditionalDamageTarget,
                    OnHitChainHitPeriod = choice.OnHitChainHitPeriod,
                    OnHitChainTargetCount = choice.OnHitChainTargetCount,
                    OnHitChainSearchRadius = choice.OnHitChainSearchRadius,
                    OnHitChainDamageMultiplier = choice.OnHitChainDamageMultiplier > 0f ? choice.OnHitChainDamageMultiplier : 1f,
                    OnHitChainDamageAttribute = choice.OnHitChainDamageAttribute,
                    ReloadReduceTargetSkillId = choice.ReloadReduceTargetSkillId,
                    ReloadReduceSecondsPerHit = choice.ReloadReduceSecondsPerHit,
                    CoreHitboxName = choice.CoreHitboxName,
                    HasCoreDamageMultiplier = choice.HasCoreDamageMultiplier,
                    CoreDamageMultiplier = choice.HasCoreDamageMultiplier && choice.CoreDamageMultiplier > 0f ? choice.CoreDamageMultiplier : 1f,
                    HasCoreOnHitAdditionalDamage = choice.HasCoreOnHitAdditionalDamage,
                    CoreOnHitAdditionalDamageChance = choice.CoreOnHitAdditionalDamageChance,
                    CoreOnHitAdditionalDamageMultiplier = choice.HasCoreOnHitAdditionalDamage && choice.CoreOnHitAdditionalDamageMultiplier > 0f ? choice.CoreOnHitAdditionalDamageMultiplier : 1f,
                    CoreOnHitAdditionalDamageAttribute = choice.CoreOnHitAdditionalDamageAttribute,
                    HitCountCooldownRefundTargetSkillId = choice.HitCountCooldownRefundTargetSkillId,
                    HitCountCooldownRefundMinTargets = choice.HitCountCooldownRefundMinTargets,
                    HitCountCooldownRefundRatio = choice.HitCountCooldownRefundRatio,
                    RuntimeSupportState = choice.RuntimeSupportState,
                    RuntimeSupportNotes = choice.RuntimeSupportNotes
                };
            }

            return definitions;
        }

        private static List<T> FilterAndSort<T>(
            IEnumerable<T> source,
            Predicate<T> predicate,
            Comparison<T> comparison)
        {
            var filtered = new List<T>();
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    filtered.Add(item);
                }
            }

            filtered.Sort(comparison);
            return filtered;
        }

        private static void ApplyStatusPayload(SkillDefinition definition, StatusPayloadRow payload)
        {
            if (definition == null || payload == null)
            {
                return;
            }

            definition.StatusEffectId = payload.StatusEffectId;
            definition.StatusChance = payload.StatusChance;
            definition.StatusEffectLabel = payload.StatusEffectLabel;
            definition.StatusEffectPrefab = LoadPrefab(payload.StatusEffectPrefabPath);
            definition.StatusDurationSeconds = payload.StatusDurationSeconds;
            definition.StatusMaxStacks = payload.StatusMaxStacks;
            definition.StatusStackAmount = payload.StatusStackAmount;
            definition.StatusTargetScope = payload.StatusTargetScope;
            definition.StatusMergePolicy = payload.StatusMergePolicy;
            definition.ShieldAmountRefreshPolicy = payload.ShieldAmountRefreshPolicy;
            definition.StatusActionSpeedBonus = payload.StatusActionSpeedBonus;
            definition.StatusMoveSpeedBonus = payload.StatusMoveSpeedBonus;
            definition.StatusAttackPowerBonus = payload.StatusAttackPowerBonus;
            definition.StatusDamageTakenBonus = payload.StatusDamageTakenBonus;
            definition.StatusCriticalDamageTakenBonus = payload.StatusCriticalDamageTakenBonus;
            definition.StatusCriticalDamageBonus = payload.StatusCriticalDamageBonus;
            definition.StatusAilmentResistanceBonus = payload.StatusAilmentResistanceBonus;
            definition.StatusCriticalResistanceBonus = payload.StatusCriticalResistanceBonus;
            definition.StatusElementResistReduction = payload.StatusElementResistReduction;
            definition.StatusFlatElementResistReduction = payload.StatusFlatElementResistReduction;
            definition.StatusElementDamageTakenBonus = payload.StatusElementDamageTakenBonus;
        }

        private static void ApplyStatusPayload(SkillEffectDefinition definition, StatusPayloadRow payload)
        {
            if (definition == null || payload == null)
            {
                return;
            }

            definition.StatusEffectId = payload.StatusEffectId;
            definition.StatusChance = payload.StatusChance;
            definition.StatusEffectLabel = payload.StatusEffectLabel;
            definition.StatusEffectPrefab = LoadPrefab(payload.StatusEffectPrefabPath);
            definition.StatusDurationSeconds = payload.StatusDurationSeconds;
            definition.StatusMaxStacks = payload.StatusMaxStacks;
            definition.StatusStackAmount = payload.StatusStackAmount;
            definition.StatusTargetScope = payload.StatusTargetScope;
            definition.StatusMergePolicy = payload.StatusMergePolicy;
            definition.ShieldAmountRefreshPolicy = payload.ShieldAmountRefreshPolicy;
            definition.StatusActionSpeedBonus = payload.StatusActionSpeedBonus;
            definition.StatusMoveSpeedBonus = payload.StatusMoveSpeedBonus;
            definition.StatusAttackPowerBonus = payload.StatusAttackPowerBonus;
            definition.StatusSpellPowerBonus = payload.StatusSpellPowerBonus;
            definition.StatusDamageBonusRate = payload.StatusDamageBonusRate;
            definition.StatusShieldReceivedBonus = payload.StatusShieldReceivedBonus;
            definition.StatusDamageTakenBonus = payload.StatusDamageTakenBonus;
            definition.StatusCriticalDamageTakenBonus = payload.StatusCriticalDamageTakenBonus;
            definition.StatusAilmentResistanceBonus = payload.StatusAilmentResistanceBonus;
            definition.StatusCriticalChanceBonus = payload.StatusCriticalChanceBonus;
            definition.StatusCriticalResistanceBonus = payload.StatusCriticalResistanceBonus;
            definition.StatusElementResistReduction = payload.StatusElementResistReduction;
            definition.StatusFlatElementResistReduction = payload.StatusFlatElementResistReduction;
            definition.StatusElementDamageTakenBonus = payload.StatusElementDamageTakenBonus;
            definition.StatusConditionalTargetStatusId = payload.StatusConditionalTargetStatusId;
            definition.StatusConditionalStatusChanceBonus = payload.StatusConditionalStatusChanceBonus;
            definition.StatusAppliedStatusDurationBonusStatusId = payload.StatusAppliedStatusDurationBonusStatusId;
            definition.StatusAppliedStatusDurationBonus = payload.StatusAppliedStatusDurationBonus;
            definition.StatusOutgoingAdditionalDamageMultiplier = payload.StatusOutgoingAdditionalDamageMultiplier;
            definition.StatusOutgoingAdditionalDamageTriggerAttribute = payload.StatusOutgoingAdditionalDamageTriggerAttribute;
            definition.StatusOutgoingAdditionalDamageAttribute = payload.StatusOutgoingAdditionalDamageAttribute;
        }

        private static SkillChoiceGroup MapChoiceGroup(PakuriCsvChoiceGroup group)
        {
            switch (group)
            {
                case PakuriCsvChoiceGroup.ActiveMaster:
                    return SkillChoiceGroup.ActiveMaster;
                case PakuriCsvChoiceGroup.PassiveBase:
                    return SkillChoiceGroup.PassiveBase;
                case PakuriCsvChoiceGroup.PassiveEnhancement:
                    return SkillChoiceGroup.PassiveEnhancement;
                default:
                    return SkillChoiceGroup.ActiveEnhancement;
            }
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
