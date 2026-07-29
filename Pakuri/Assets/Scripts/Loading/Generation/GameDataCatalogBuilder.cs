using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{
    internal sealed class GameDataCatalogBuilder
    {

        private readonly CsvRuntimeCatalog assetCatalog;

        private GameDataCatalogBuilder(CsvRuntimeCatalog assetCatalog )
        {
            this.assetCatalog = assetCatalog ?? throw new ArgumentNullException(nameof(assetCatalog));
        }

        internal static GameDataCatalog BuildRuntimeCatalog(
            SourceModel model ,
            CsvRuntimeCatalog assetCatalog )
        {
            return new GameDataCatalogBuilder(assetCatalog).Build(model);
        }

        private GameDataCatalog Build(SourceModel model )
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
                monster.ActiveSkillName = ResolveMonsterSkillDisplayName(
                    model,
                    sourceMonster.Id,
                    PakuriCsvSkillKind.Active,
                    SkillSlot.A);
                monster.PassiveSkillName = ResolveMonsterSkillDisplayName(
                    model,
                    sourceMonster.Id,
                    PakuriCsvSkillKind.Passive,
                    SkillSlot.F);
                monster.MonsterIconImage = LoadSprite(sourceMonster.MonsterIconImagePath);
                monster.PowerStat = sourceMonster.PowerStat;
                monster.BaseStats = new UnitCombatStats
                {
                    MaxHealth = sourceMonster.MaxHealth,
                    AttackPower = sourceMonster.BaseAttackPower,
                    SpellPower = sourceMonster.BaseSpellPower,
                    MoveSpeed = sourceMonster.BaseMoveSpeed,
                    CriticalChance = sourceMonster.BaseCriticalChance,
                    CriticalDamage = sourceMonster.BaseCriticalDamage,
                    CriticalResistance = sourceMonster.BaseCriticalResistance
                };
                monster.Defenses = new UnitDefenseStats
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

            catalog.Monsters = monsters.ToArray();
            catalog.StageOneEnemies = BuildEnemies(model, "stage_one");
            catalog.StageTwoEnemies = BuildEnemies(model, "stage_two");
            catalog.StatusEffects = BuildStatusEffects(model);
            return catalog;
        }

        private string ResolveMonsterSkillDisplayName(
            SourceModel model ,
            string monsterId ,
            PakuriCsvSkillKind skillKind ,
            SkillSlot slot )
        {
            if (model != null && model.Skills != null)
            {
                foreach (var skill in model.Skills.Values)
                {
                    if (skill.SkillKind == skillKind
                        && skill.Slot == slot
                        && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase))
                    {
                        return skill.DisplayName;
                    }
                }
            }

            throw new CsvFatalException(
                $"Monster '{monsterId}' has no '{skillKind}' skill in slot '{slot}'.");
        }

        private EnemyDefinition[] BuildEnemies(
            SourceModel model ,
            string stageId )
        {
            var enemies = new List<EnemyDefinition>();
            var sourceEnemies = FilterAndSort(
                model.Enemies.Values,
                row => string.Equals(row.StageId, stageId, StringComparison.OrdinalIgnoreCase),
                (left, right) =>
                {
                    var orderCompare = left.SortOrder.CompareTo(right.SortOrder);
                    if (orderCompare != 0)
                    {
                        return orderCompare;
                    }

                    return string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
                });
            for (var i = 0; i < sourceEnemies.Count; i++)
            {
                var sourceEnemy = sourceEnemies[i];
                var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                enemy.EnemyId = sourceEnemy.Id;
                enemy.DisplayName = sourceEnemy.DisplayName;
                enemy.Attribute = sourceEnemy.Attribute;
                enemy.Stats = new UnitCombatStats
                {
                    MaxHealth = sourceEnemy.MaxHealth,
                    AttackPower = sourceEnemy.AttackPower,
                    SpellPower = sourceEnemy.SpellPower,
                    MoveSpeed = sourceEnemy.MoveSpeed,
                    CriticalChance = sourceEnemy.CriticalChance,
                    CriticalDamage = sourceEnemy.CriticalDamage,
                    CriticalResistance = sourceEnemy.CriticalResistance
                };
                enemy.Defenses = new UnitDefenseStats
                {
                    Physical = sourceEnemy.PhysicalDefense,
                    Fire = sourceEnemy.FireDefense,
                    Lightning = sourceEnemy.LightningDefense,
                    Ice = sourceEnemy.IceDefense,
                    Darkness = sourceEnemy.DarknessDefense,
                    Holy = sourceEnemy.HolyDefense
                };
                enemy.ActiveSkills = BuildEnemyAssignedActiveSkills(model, sourceEnemy.Id);
                enemy.SkillTriggers = BuildEnemyAssignedSkillTriggers(model, sourceEnemy.Id);
                enemy.PassiveSkill = BuildEnemyPassiveDefinition(model, sourceEnemy.PassiveId);
                enemy.NexusDamage = sourceEnemy.NexusDamage;
                enemies.Add(enemy);
            }

            return enemies.ToArray();
        }

        private EnemyPassiveDefinition BuildEnemyPassiveDefinition(SourceModel model , string passiveId )
        {
            if (model == null
                || string.IsNullOrWhiteSpace(passiveId)
                || !model.EnemyBaseSkills.TryGetValue(passiveId, out var source)
                || source == null
                || source.Skill == null)
            {
                return null;
            }

            return new EnemyPassiveDefinition
            {
                PassiveId = source.Skill.Id,
                DisplayName = source.Skill.DisplayName,
                ModifierKind = source.PassiveModifierKind,
                HasAttribute = source.PassiveHasAttribute,
                Attribute = source.PassiveAttribute,
                ModifierValue = source.PassiveModifierValue
            };
        }

        private SkillSourceDefinition[] BuildEnemyAssignedActiveSkills(SourceModel model , string enemyId )
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyId)
                || !model.Enemies.TryGetValue(enemyId, out var enemyRow))
            {
                return Array.Empty<SkillSourceDefinition>();
            }

            var definitions = new List<SkillSourceDefinition>(2);
            TryAddEnemyAssignedSkillDefinition(model, enemyRow.SkillSlotAId, SkillSlot.A, definitions);
            TryAddEnemyAssignedSkillDefinition(model, enemyRow.SkillSlotBId, SkillSlot.B, definitions);

            return definitions.ToArray();
        }

        private void TryAddEnemyAssignedSkillDefinition(
            SourceModel model ,
            string skillId ,
            SkillSlot runtimeSlot ,
            List<SkillSourceDefinition> definitions )
        {
            if (model == null
                || definitions == null
                || string.IsNullOrWhiteSpace(skillId)
                || !model.EnemyBaseSkills.TryGetValue(skillId, out var source)
                || source == null
                || source.Skill == null)
            {
                return;
            }

            definitions.Add(BuildEnemyAssignedSkillDefinition(source, runtimeSlot));
        }

        private SkillSourceDefinition BuildEnemyAssignedSkillDefinition(EnemyBaseSkillRow source , SkillSlot runtimeSlot )
        {
            var row = source.Skill;
            var definition = new SkillSourceDefinition
            {
                SkillId = row.Id,
                DisplayName = row.DisplayName,
                Slot = runtimeSlot,
                RuntimeKind = row.RuntimeKind,
                ImplementationState = SkillImplementationState.RuntimeImplemented,
                IsDefaultLearned = true,
                RuntimeVisual = BuildRuntimeVisual(row),
                DescriptionText = row.DescriptionText,
                Summary = row.Summary,
                Attribute = row.Attribute,
                BaseDamage = row.BaseDamage,
                AttackPowerCoefficient = row.AttackPowerCoefficient,
                SpellPowerCoefficient = row.SpellPowerCoefficient,
                Radius = source.EffectRadius,
                CastRange = source.CastRange,
                EffectRadius = source.EffectRadius,
                TargetScope = source.TargetScope,
                TargetSelection = MapEnemyTargetSelection(source.TargetSelection),
                ExecutionProfile = source.ExecutionProfile,
                FlatValue = source.FlatValue,
                ProjectileSpeed = row.ProjectileSpeed,
                ProjectileLifetimeSeconds = source.ProjectileLifetime,
                CooldownSeconds = row.CooldownSeconds,
                ActiveDurationSeconds = row.ActiveDurationSeconds,
                IncomingDamageMultiplier = source.IncomingDamageMultiplier,
                MoveSpeedMultiplier = source.MoveSpeedMultiplier,
                OutgoingDamageMultiplier = source.OutgoingDamageMultiplier,
                ChainDamageMultiplier = source.ChainDamageMultiplier,
                ChainDelaySeconds = source.ChainDelaySeconds,
                ChainRadius = source.ChainRadius,
                ExcludePrimaryTarget = source.ExcludePrimaryTarget,
                TargetMaxHealthRatio = source.TargetMaxHealthRatio,
                ChargeRampSeconds = source.ChargeRampSeconds,
                ChargeMoveSpeedMultiplier = source.ChargeMoveSpeedMultiplier,
                HitTargetCount = source.HitTargetCount,
                CriticalAllowed = true,
                UsePrefabHitbox = row.RuntimeHitboxSizeX > 0f || row.RuntimeHitboxSizeY > 0f
            };

            ApplyStatusPayload(definition, row.Status);
            ApplyEnemyExecutionProfile(definition);
            return definition;
        }

        private void ApplyEnemyExecutionProfile(SkillSourceDefinition definition )
        {
            if (definition == null)
            {
                return;
            }

            var profile = definition.ExecutionProfile;
            if (profile == null)
            {
                profile = string.Empty;
            }
            if (string.Equals(profile, "ApplySelfIncomingDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusDamageTakenBonus = definition.IncomingDamageMultiplier - 1f;
            }
            else if (string.Equals(profile, "ApplyAllyMoveAndDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusMoveSpeedBonus = definition.MoveSpeedMultiplier - 1f;
                definition.StatusDamageBonusRate = definition.OutgoingDamageMultiplier - 1f;
            }
            else if (string.Equals(profile, "ApplyOutgoingDamageMultiplierStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusDamageBonusRate = definition.OutgoingDamageMultiplier - 1f;
                definition.StatusPermanent = definition.StatusDurationSeconds <= 0f;
            }
            else if (string.Equals(profile, "GrantShieldToEnemyAllies", StringComparison.OrdinalIgnoreCase))
            {
                definition.BaseDamage = definition.FlatValue;
            }
        }

        private string MapEnemyTargetSelection(string selection )
        {
            if (string.Equals(selection, "FarthestHostile", StringComparison.OrdinalIgnoreCase))
            {
                return "Farthest";
            }

            if (string.Equals(selection, "RandomHostile", StringComparison.OrdinalIgnoreCase))
            {
                return "Random";
            }

            if (string.Equals(selection, "LowestHealthFriendly", StringComparison.OrdinalIgnoreCase))
            {
                return "LowestHealth";
            }

            if (string.Equals(selection, "AllHostiles", StringComparison.OrdinalIgnoreCase)
                || string.Equals(selection, "AllFriendlies", StringComparison.OrdinalIgnoreCase))
            {
                return "Nearest";
            }

            if (string.Equals(selection, "CurrentTarget", StringComparison.OrdinalIgnoreCase))
            {
                return "Nearest";
            }

            return selection;
        }

        private SkillTriggerDefinition[] BuildEnemyAssignedSkillTriggers(SourceModel model , string enemyId )
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyId)
                || !model.Enemies.TryGetValue(enemyId, out var enemyRow))
            {
                return Array.Empty<SkillTriggerDefinition>();
            }

            var assignedSkillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                enemyRow.SkillSlotAId,
                enemyRow.SkillSlotBId
            };

            var rows = FilterAndSort(
                model.EnemyTriggers.Values,
                trigger => trigger.Enabled && assignedSkillIds.Contains(trigger.SourceSkillId),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));
            var definitions = new SkillTriggerDefinition[rows.Count];
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                definitions[i] = new SkillTriggerDefinition
                {
                    TriggerId = row.Id,
                    MonsterId = enemyId,
                    SourceSkillId = row.SourceSkillId,
                    TriggerEvent = row.TriggerEvent,
                    SortOrder = row.SortOrder,
                    ProcChance = 1f,
                    NormalizedNodes = new[]
                    {
                        new SkillNodeDefinition
                        {
                            OwnerKind = SkillNodeOwnerKind.Trigger.ToString(),
                            TargetSkillId = row.SourceSkillId,
                            HandlerId = "ExecuteSkill",
                            EnabledByDefault = true,
                            Params = new[]
                            {
                                new SkillNodeParamDefinition
                                {
                                    ParamKey = "skill_id",
                                    Value = row.TriggeredSkillId
                                },
                                new SkillNodeParamDefinition
                                {
                                    ParamKey = "damage_multiplier",
                                    Value = "1"
                                }
                            }
                        }
                    }
                };
            }

            return definitions;
        }

        private StatusEffectDefinition[] BuildStatusEffects(SourceModel model )
        {
            var statuses = new List<StatusEffectDefinition>();
            foreach (var row in model.StatusEffects.Values)
            {
                StatusEffectLookup.TryParse(row.Id, out var kind);
                statuses.Add(new StatusEffectDefinition
                {
                    StatusEffectId = row.Id,
                    StatusEffectLabel = row.Label,
                    Kind = kind,
                    Classification = row.Classification,
                    HasAttribute = row.HasAttribute,
                    Attribute = row.Attribute,
                    DefaultDurationSeconds = row.DefaultDurationSeconds,
                    IsPermanent = row.IsPermanent,
                    MaxStacks = row.MaxStacks,
                    BaseStackAmount = row.BaseStackAmount,
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

        private MonsterDefinition.RewardChoiceDefinition[] BuildRewardChoices(SourceModel model , string monsterId )
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

        private SkillSourceDefinition[] BuildActiveSkills(SourceModel model , string monsterId )
        {
            var skills = FilterAndSort(
                model.Skills.Values,
                skill => skill.SkillKind == PakuriCsvSkillKind.Active
                    && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new SkillSourceDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                var definition = new SkillSourceDefinition
                {
                    SkillId = skill.Id,
                    DisplayName = skill.DisplayName,
                    Slot = skill.Slot,
                    RuntimeKind = skill.RuntimeKind,
                    ImplementationState = skill.ImplementationState,
                    IsDefaultLearned = skill.IsDefaultLearned,
                    SkillIcon = LoadSprite(skill.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(skill.SkillEffectPrefabPath),
                    RuntimeVisual = BuildRuntimeVisual(skill),
                    ImpactRuntimeVisual = BuildImpactRuntimeVisual(skill),
                    DescriptionText = skill.DescriptionText,
                    Attribute = skill.Attribute,
                    BaseDamage = skill.BaseDamage,
                    AttackPowerCoefficient = skill.AttackPowerCoefficient,
                    SpellPowerCoefficient = skill.SpellPowerCoefficient,
                    Radius = skill.Radius,
                    LineLength = skill.LineLength,
                    CastRepeatCount = skill.CastRepeatCount,
                    CastRepeatIntervalSeconds = skill.CastRepeatIntervalSeconds,
                    KnockbackDistance = skill.KnockbackDistance,
                    DamageDelaySeconds = skill.DamageDelaySeconds,
                    ExecuteHealthRatioThreshold = skill.ExecuteHealthRatioThreshold,
                    RequireExecuteThresholdToCast = skill.RequireExecuteThresholdToCast,
                    ExecuteDamageMultiplier = skill.ExecuteDamageMultiplier,
                    KillCooldownRefundRatio = skill.KillCooldownRefundRatio,
                    BossDamageMultiplier = skill.BossDamageMultiplier,
                    HitTargetCount = skill.HitTargetCount,
                    UsePrefabHitbox = skill.UsePrefabHitbox,
                    TargetSelection = skill.TargetSelection,
                    TargetSelectionStatusId = skill.TargetSelectionStatusId,
                    TargetSelectionStatusMinStacks = skill.TargetSelectionStatusMinStacks,
                    CooldownSeconds = skill.CooldownSeconds,
                    ActiveDurationSeconds = skill.ActiveDurationSeconds,
                    MagazineCapacity = skill.MagazineCapacity,
                    ReloadSeconds = skill.ReloadSeconds,
                    ShotIntervalSeconds = skill.ShotIntervalSeconds,
                    BurstIntervalSeconds = skill.BurstIntervalSeconds,
                    ProjectileBurstCount = skill.ProjectileBurstCount,
                    BurstDamageProjectileIndex = skill.BurstDamageProjectileIndex,
                    BurstDamageMultiplier = skill.BurstDamageMultiplier,
                    ProjectileSpeed = skill.ProjectileSpeed,
                    PierceCount = skill.PierceCount,
                    CriticalAllowed = skill.CriticalAllowed,
                    DeploymentRequiredTargetStatusId = skill.DeploymentRequiredTargetStatusId,
                    DeploymentRequiredTargetStatusMinStacks = skill.DeploymentRequiredTargetStatusMinStacks,
                    TargetStatusStackStatusId = skill.TargetStatusStackStatusId,
                    TargetStatusStackMaxStacks = skill.TargetStatusStackMaxStacks,
                    TargetStatusStackBaseDamage = skill.TargetStatusStackBaseDamage,
                    TargetStatusStackAttackPowerCoefficient = skill.TargetStatusStackAttackPowerCoefficient,
                    TargetStatusStackSpellPowerCoefficient = skill.TargetStatusStackSpellPowerCoefficient,
                    ConsumeTargetStatusId = skill.ConsumeTargetStatusId,
                    ConsumeTargetStatusRatio = skill.ConsumeTargetStatusRatio,
                    ConsumeTargetStatusStacks = skill.ConsumeTargetStatusStacks,
                    Summary = skill.Summary,
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, SkillChoiceGroup.ActiveEnhancement),
                    MasterSkillChoices = BuildSkillChoices(model, skill.Id, SkillChoiceGroup.ActiveMaster),
                    NormalizedNodes = BuildSkillNodeDefinitions(model, SkillNodeOwnerKind.Skill, skill.Id, skill.Id)
                };

                ApplyStatusPayload(definition, skill.Status);
                definitions[i] = definition;
            }

            return definitions;
        }

        private Dictionary<string, string> BuildSkillNodeParamValueLookup(SourceModel model , string nodeId )
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.SkillNodeParams.Count; i++)
            {
                var param = model.SkillNodeParams[i];
                if (param != null && string.Equals(param.NodeId, nodeId, StringComparison.OrdinalIgnoreCase))
                {
                    parameters[param.ParamKey] = param.Value;
                }
            }

            return parameters;
        }

        private string GetSkillNodeStringParam(Dictionary<string, string> parameters , string key )
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }

        private float GetSkillNodeFloatParam(Dictionary<string, string> parameters , string key , float defaultValue )
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private int GetSkillNodeIntParam(Dictionary<string, string> parameters , string key , int defaultValue )
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private SkillTriggerDefinition[] BuildSkillTriggers(SourceModel model , string monsterId )
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
                    RequiredSourceStatusId = trigger.RequiredSourceStatusId,
                    RequiredSourceStatusMinStacks = trigger.RequiredSourceStatusMinStacks,
                    ConditionStatusId = trigger.ConditionStatusId,
                    ConditionStatuses = StatusRuntimeCompiler.ParseConditionStatusExpression(trigger.ConditionStatusId),
                    ConditionStatusSourceSkillId = trigger.ConditionStatusSourceSkillId,
                    ConditionStatusSourceSkillIds = StatusRuntimeCompiler.ParseIdList(trigger.ConditionStatusSourceSkillId),
                    TriggerAttribute = trigger.TriggerAttribute,
                    EventSkillId = trigger.EventSkillId,
                    EventSkillRuntimeKinds = trigger.EventSkillRuntimeKinds,
                    EventSkillRuntimeKindValues = StatusRuntimeCompiler.ParseSkillRuntimeKindConditions(
                        trigger.EventSkillRuntimeKinds),
                    ProcChance = trigger.ProcChance,
                    InternalCooldownSeconds = trigger.InternalCooldownSeconds,
                    SortOrder = trigger.SortOrder,
                    RepeatCount = trigger.RepeatCount,
                    RepeatIntervalSeconds = trigger.RepeatIntervalSeconds,
                    TriggerDelaySeconds = trigger.TriggerDelaySeconds,
                    TriggerEveryCount = trigger.TriggerEveryCount,
                    EventSourceScope = trigger.EventSourceScope,
                    RequireEventExecute = trigger.RequireEventExecute,
                    NormalizedNodes = BuildSkillNodeDefinitions(
                        model,
                        SkillNodeOwnerKind.Trigger,
                        trigger.Id,
                        trigger.SourceSkillId)
                };
            }

            return definitions;
        }

        private PassiveDefinition[] BuildPassiveSkills(SourceModel model , string monsterId )
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
                    BaseModifierChoices = BuildSkillChoices(model, skill.Id, SkillChoiceGroup.PassiveBase),
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, SkillChoiceGroup.PassiveEnhancement),
                    NormalizedNodes = BuildSkillNodeDefinitions(model, SkillNodeOwnerKind.Passive, skill.Id, skill.Id)
                };
            }

            return definitions;
        }

        private SkillChoiceDefinition[] BuildSkillChoices(SourceModel model , string skillId , SkillChoiceGroup choiceGroup )
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
                var targetSkillId = choice.TargetSkillId;
                if (string.IsNullOrWhiteSpace(targetSkillId))
                {
                    targetSkillId = choice.SkillId;
                }
                var normalizedNodes = BuildSkillNodeDefinitions(
                    model,
                    SkillNodeOwnerKind.Choice,
                    choice.Id,
                    targetSkillId);

                definitions[i] = new SkillChoiceDefinition
                {
                    ChoiceId = choice.Id,
                    MonsterId = choice.MonsterId,
                    SkillId = choice.SkillId,
                    TargetSkillId = targetSkillId,
                    ChoiceGroup = choice.ChoiceGroup,
                    Title = choice.Title,
                    SkillIcon = LoadSprite(choice.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(GetChoiceNodeParam(
                        normalizedNodes,
                        "EffectVisual",
                        "skill_effect_prefab_path")),
                    DescriptionText = choice.DescriptionText,
                    NormalizedNodes = normalizedNodes
                };
            }

            return definitions;
        }

        private string GetChoiceNodeParam(
            SkillNodeDefinition[] nodes ,
            string handlerId ,
            string paramKey )
        {
            if (nodes == null || string.IsNullOrWhiteSpace(handlerId) || string.IsNullOrWhiteSpace(paramKey))
            {
                return string.Empty;
            }

            for (var nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                var node = nodes[nodeIndex];
                if (node == null
                    || !node.EnabledByDefault
                    || !string.Equals(node.HandlerId, handlerId, StringComparison.OrdinalIgnoreCase)
                    || node.Params == null)
                {
                    continue;
                }

                for (var paramIndex = 0; paramIndex < node.Params.Length; paramIndex++)
                {
                    var param = node.Params[paramIndex];
                    if (param != null
                        && string.Equals(param.ParamKey, paramKey, StringComparison.OrdinalIgnoreCase))
                    {
                        if (param.Value == null)
                        {
                            return string.Empty;
                        }

                        return param.Value;
                    }
                }
            }

            return string.Empty;
        }

        private SkillNodeDefinition[] BuildSkillNodeDefinitions(
            SourceModel model ,
            SkillNodeOwnerKind ownerKind ,
            string ownerId ,
            string defaultTargetSkillId )
        {
            var nodes = FilterAndSort(
                model.SkillNodes.Values,
                node => node.OwnerKind == ownerKind
                    && string.Equals(node.OwnerId, ownerId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            if (nodes.Count == 0)
            {
                return Array.Empty<SkillNodeDefinition>();
            }

            var definitions = new SkillNodeDefinition[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var targetSkillId = node.TargetSkillId;
                if (string.IsNullOrWhiteSpace(targetSkillId))
                {
                    targetSkillId = defaultTargetSkillId;
                }

                var definition = new SkillNodeDefinition
                {
                    OwnerKind = node.OwnerKind.ToString(),
                    TargetSkillId = targetSkillId,
                    HandlerId = node.HandlerId,
                    EnabledByDefault = node.EnabledByDefault,
                    Params = BuildSkillNodeParamDefinitions(model, node.Id)
                };

                if (string.Equals(node.HandlerId, "EffectVisual", StringComparison.OrdinalIgnoreCase))
                {
                    var parameters = BuildSkillNodeParamValueLookup(model, node.Id);
                    definition.ResolvedPrefab = LoadPrefab(
                        GetSkillNodeStringParam(parameters, "skill_effect_prefab_path"));
                }
                else if (string.Equals(node.HandlerId, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(node.HandlerId, "ShowVisual", StringComparison.OrdinalIgnoreCase))
                {
                    var parameters = BuildSkillNodeParamValueLookup(model, node.Id);
                    definition.ResolvedRuntimeVisual = BuildRuntimeVisual(
                        GetSkillNodeStringParam(parameters, "runtime_visual_sprite_path"),
                        GetSkillNodeStringParam(parameters, "runtime_visual_animator_controller_path"),
                        GetSkillNodeFloatParam(parameters, "runtime_visual_scale", 1f),
                        0f,
                        0f,
                        0f,
                        GetSkillNodeIntParam(parameters, "runtime_visual_sorting_order", 0),
                        GetSkillNodeFloatParam(parameters, "runtime_hitbox_size_x", 0f),
                        GetSkillNodeFloatParam(parameters, "runtime_hitbox_size_y", 0f));
                }

                definitions[i] = definition;
            }

            return definitions;
        }

        private SkillNodeParamDefinition[] BuildSkillNodeParamDefinitions(SourceModel model , string nodeId )
        {
            var nodeParams = FilterAndSort(
                model.SkillNodeParams,
                param => string.Equals(param.NodeId, nodeId, StringComparison.OrdinalIgnoreCase),
                (left, right) => string.Compare(left.ParamKey, right.ParamKey, StringComparison.OrdinalIgnoreCase));

            if (nodeParams.Count == 0)
            {
                return Array.Empty<SkillNodeParamDefinition>();
            }

            var definitions = new SkillNodeParamDefinition[nodeParams.Count];
            for (var i = 0; i < nodeParams.Count; i++)
            {
                var param = nodeParams[i];
                definitions[i] = new SkillNodeParamDefinition
                {
                    ParamKey = param.ParamKey,
                    Value = param.Value
                };
            }

            return definitions;
        }

        private List<T> FilterAndSort<T>(
            IEnumerable<T> source ,
            Predicate<T> predicate ,
            Comparison<T> comparison )
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

        private void ApplyStatusPayload(SkillSourceDefinition definition , StatusPayloadRow payload )
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

        private IEnumerable<CatalogEntryRow> SortCatalogEntries(Dictionary<string, CatalogEntryRow> entries )
        {
            var list = new List<CatalogEntryRow>(entries.Values);
            list.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            return list;
        }

        private Sprite LoadSprite(string assetPath )
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (assetCatalog != null && assetCatalog.TryGetSprite(assetPath, out var sprite))
            {
                return sprite;
            }

            throw new CsvFatalException($"Runtime sprite asset is missing: '{assetPath}'.");
        }

        private RuntimeSkillVisualSpec BuildRuntimeVisual(SkillRow row )
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            return BuildRuntimeVisual(
                row.RuntimeVisualSpritePath,
                row.RuntimeVisualAnimatorControllerPath,
                row.RuntimeVisualScale,
                row.RuntimeVisualScaleX,
                row.RuntimeVisualScaleY,
                row.RuntimeVisualScaleZ,
                row.RuntimeVisualSortingOrder,
                row.RuntimeHitboxSizeX,
                row.RuntimeHitboxSizeY,
                row.RuntimeVisualAnchor);
        }

        private RuntimeSkillVisualSpec BuildImpactRuntimeVisual(SkillRow row )
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            return BuildRuntimeVisual(
                row.RuntimeImpactVisualSpritePath,
                row.RuntimeImpactVisualAnimatorControllerPath,
                row.RuntimeImpactVisualScale,
                0f,
                0f,
                0f,
                row.RuntimeImpactVisualSortingOrder,
                0f,
                0f);
        }

        private RuntimeSkillVisualSpec BuildRuntimeVisual(
            string spritePath ,
            string animatorControllerPath ,
            float scale ,
            float scaleX ,
            float scaleY ,
            float scaleZ ,
            int sortingOrder ,
            float hitboxSizeX ,
            float hitboxSizeY ,
            string visualAnchor = null )
        {
            var anchor = RuntimeSkillVisualAnchor.Skill;
            if (!string.IsNullOrWhiteSpace(visualAnchor))
            {
                anchor = (RuntimeSkillVisualAnchor)Enum.Parse(
                    typeof(RuntimeSkillVisualAnchor),
                    visualAnchor,
                    true);
            }

            var localScale = Vector3.one * scale;
            if (scaleX != 0f || scaleY != 0f || scaleZ != 0f)
            {
                if (scaleX == 0f)
                {
                    scaleX = 1f;
                }
                if (scaleY == 0f)
                {
                    scaleY = 1f;
                }
                if (scaleZ == 0f)
                {
                    scaleZ = 1f;
                }

                localScale = new Vector3(scaleX, scaleY, scaleZ);
            }

            return new RuntimeSkillVisualSpec
            {
                Sprite = LoadSprite(spritePath),
                AnimatorController = LoadAnimatorController(animatorControllerPath),
                LocalScale = localScale,
                SortingOrder = sortingOrder,
                Anchor = anchor,
                Hitbox = new RuntimeSkillHitboxSpec
                {
                    Size = new Vector2(Mathf.Max(0f, hitboxSizeX), Mathf.Max(0f, hitboxSizeY))
                }
            };
        }

        private RuntimeAnimatorController LoadAnimatorController(string assetPath )
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (assetCatalog != null && assetCatalog.TryGetAnimatorController(assetPath, out var animatorController))
            {
                return animatorController;
            }

            throw new CsvFatalException($"Runtime animator controller asset is missing: '{assetPath}'.");
        }

        private GameObject LoadPrefab(string assetPath )
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (assetCatalog != null && assetCatalog.TryGetPrefab(assetPath, out var prefab))
            {
                return prefab;
            }

            throw new CsvFatalException($"Runtime prefab asset is missing: '{assetPath}'.");
        }
    }
}
