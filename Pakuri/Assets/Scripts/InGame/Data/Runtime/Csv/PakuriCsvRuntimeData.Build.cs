using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * 검증된 CSV 원본 모델을 게임에서 사용하는 데이터 정의로 변환한다.
     */
    public static partial class PakuriCsvRuntimeData
    {
        /*
         * CSV 원본 모델을 런타임 게임 데이터 카탈로그로 만든다.
         */
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
                monster.ActiveSkillName = ResolveMonsterSkillDisplayName(
                    model,
                    sourceMonster.Id,
                    PakuriCsvSkillKind.Active,
                    SkillSlot.A,
                    sourceMonster.ActiveSkillName);
                monster.PassiveSkillName = ResolveMonsterSkillDisplayName(
                    model,
                    sourceMonster.Id,
                    PakuriCsvSkillKind.Passive,
                    SkillSlot.F,
                    sourceMonster.PassiveSkillName);
                monster.MonsterIconImage = LoadSprite(sourceMonster.MonsterIconImagePath);
                monster.MaxHealth = sourceMonster.MaxHealth;
                monster.PowerStat = sourceMonster.PowerStat;
                monster.BaseDamage = sourceMonster.BaseDamage;
                monster.PowerCoefficient = sourceMonster.PowerCoefficient;
                monster.BaseStats = new UnitStatsRuntime
                {
                    MaxHealth = sourceMonster.MaxHealth,
                    AttackPower = sourceMonster.BaseAttackPower,
                    SpellPower = sourceMonster.BaseSpellPower,
                    MoveSpeed = sourceMonster.BaseMoveSpeed,
                    CriticalChance = sourceMonster.BaseCriticalChance,
                    CriticalDamage = sourceMonster.BaseCriticalDamage,
                    CriticalResistance = sourceMonster.BaseCriticalResistance
                };
                monster.Defenses = new DamageCalculator.AttributeDefenseSet
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

        /*
         * 현재 조건에 맞는 값을 결정한다.
         */
        private static string ResolveMonsterSkillDisplayName(
            SourceModel model,
            string monsterId,
            PakuriCsvSkillKind skillKind,
            SkillSlot slot,
            string fallback)
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

            return fallback ?? string.Empty;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static EnemyDefinition[] BuildEnemies(
            SourceModel model,
            string stageId)
        {
            var enemies = new List<EnemyDefinition>();
            var sourceEnemies = FilterAndSort(
                model.Enemies.Values,
                row => string.Equals(row.StageId, stageId, StringComparison.OrdinalIgnoreCase),
                (left, right) =>
                {
                    var orderCompare = left.SortOrder.CompareTo(right.SortOrder);
                    return orderCompare != 0
                        ? orderCompare
                        : string.Compare(left.Id, right.Id, StringComparison.OrdinalIgnoreCase);
                });
            for (var i = 0; i < sourceEnemies.Count; i++)
            {
                var sourceEnemy = sourceEnemies[i];
                var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                enemy.EnemyId = sourceEnemy.Id;
                enemy.DisplayName = sourceEnemy.DisplayName;
                enemy.EncounterRole = sourceEnemy.EncounterRole;
                enemy.AttackType = sourceEnemy.AttackType;
                enemy.Attribute = sourceEnemy.Attribute;
                enemy.Stats = new UnitStatsRuntime
                {
                    MaxHealth = sourceEnemy.MaxHealth,
                    AttackPower = sourceEnemy.AttackPower,
                    SpellPower = sourceEnemy.SpellPower,
                    MoveSpeed = sourceEnemy.MoveSpeed,
                    CriticalChance = sourceEnemy.CriticalChance,
                    CriticalDamage = sourceEnemy.CriticalDamage,
                    CriticalResistance = sourceEnemy.CriticalResistance
                };
                enemy.Defenses = new DamageCalculator.AttributeDefenseSet
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
                enemy.NexusDamage = sourceEnemy.NexusDamage > 0f ? sourceEnemy.NexusDamage : 1f;
                enemies.Add(enemy);
            }

            return enemies.ToArray();
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static EnemyPassiveDefinition BuildEnemyPassiveDefinition(SourceModel model, string passiveId)
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
                ApplyTarget = source.PassiveApplyTarget,
                ModifierKind = source.PassiveModifierKind,
                ModifierValue = source.PassiveModifierValue
            };
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillDefinition[] BuildEnemyAssignedActiveSkills(SourceModel model, string enemyId)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyId)
                || !model.Enemies.TryGetValue(enemyId, out var migratedEnemy))
            {
                return Array.Empty<SkillDefinition>();
            }

            var definitions = new List<SkillDefinition>(2);
            TryAddEnemyAssignedSkillDefinition(model, migratedEnemy.SkillSlotAId, SkillSlot.A, definitions);
            TryAddEnemyAssignedSkillDefinition(model, migratedEnemy.SkillSlotBId, SkillSlot.B, definitions);

            return definitions.ToArray();
        }

        /*
         * 조건을 만족하는 항목만 추가한다.
         */
        private static void TryAddEnemyAssignedSkillDefinition(
            SourceModel model,
            string skillId,
            SkillSlot runtimeSlot,
            List<SkillDefinition> definitions)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillDefinition BuildEnemyAssignedSkillDefinition(EnemyBaseSkillRow source, SkillSlot runtimeSlot)
        {
            var row = source.Skill;
            var definition = new SkillDefinition
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
                UseCombinedStatCoefficients = true,
                Radius = source.EffectRadius > 0f ? source.EffectRadius : row.Radius,
                CastRange = source.CastRange > 0f ? source.CastRange : row.Radius,
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
                StatusEffectId = row.Status != null ? row.Status.StatusEffectId : string.Empty,
                StatusDurationSeconds = source.StatusDurationSeconds > 0f
                    ? source.StatusDurationSeconds
                    : row.ActiveDurationSeconds,
                StatusActionSpeedBonus = source.StatusActionSpeedBonus,
                UsePrefabHitbox = row.RuntimeHitboxSizeX > 0f || row.RuntimeHitboxSizeY > 0f
            };

            ApplyEnemyExecutionProfile(definition);
            return definition;
        }

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
        private static void ApplyEnemyExecutionProfile(SkillDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            var profile = definition.ExecutionProfile ?? string.Empty;
            if (string.Equals(profile, "DamageAndActionSpeedDebuff", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusEffectId = "passive-buff";
                definition.StatusEffectLabel = "Action Speed Down";
                definition.StatusChance = 1f;
                definition.StatusMaxStacks = 1;
                definition.StatusStackAmount = 1;
            }
            else if (string.Equals(profile, "ApplySelfIncomingDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusEffectId = "passive-buff";
                definition.StatusEffectLabel = "Incoming Damage Down";
                definition.StatusChance = 1f;
                definition.StatusMaxStacks = 1;
                definition.StatusStackAmount = 1;
                definition.StatusTargetScope = "Self";
                definition.StatusDamageTakenBonus = definition.IncomingDamageMultiplier - 1f;
            }
            else if (string.Equals(profile, "ApplyAllyMoveAndDamageMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusEffectId = "passive-buff";
                definition.StatusEffectLabel = "Charge Command";
                definition.StatusChance = 1f;
                definition.StatusMaxStacks = 1;
                definition.StatusStackAmount = 1;
                definition.StatusTargetScope = "AllAllies";
                definition.StatusMoveSpeedBonus = definition.MoveSpeedMultiplier - 1f;
                definition.StatusDamageBonusRate = definition.OutgoingDamageMultiplier - 1f;
            }
            else if (string.Equals(profile, "ApplyOutgoingDamageMultiplierStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusEffectId = "passive-buff";
                definition.StatusEffectLabel = "Intimidated";
                definition.StatusChance = 1f;
                definition.StatusMaxStacks = 1;
                definition.StatusStackAmount = 1;
                definition.StatusDamageBonusRate = definition.OutgoingDamageMultiplier - 1f;
                definition.StatusPermanent = definition.StatusDurationSeconds <= 0f;
            }
            else if (string.Equals(profile, "GrantShieldToEnemyAllies", StringComparison.OrdinalIgnoreCase))
            {
                definition.BaseDamage = definition.FlatValue;
                definition.StatusChance = 1f;
                definition.StatusMaxStacks = 1;
                definition.StatusStackAmount = 1;
                definition.StatusTargetScope = "AllAllies";
            }
            else if (string.Equals(profile, "ChargeDamageStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusChance = 1f;
                definition.StatusMaxStacks = 1;
                definition.StatusStackAmount = 1;
            }
        }

        /*
         * 원본 값을 런타임 형식으로 바꾼다.
         */
        private static string MapEnemyTargetSelection(string selection)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillTriggerDefinition[] BuildEnemyAssignedSkillTriggers(SourceModel model, string enemyId)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyId)
                || !model.Enemies.TryGetValue(enemyId, out var migratedEnemy))
            {
                return Array.Empty<SkillTriggerDefinition>();
            }

            var assignedSkillIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                migratedEnemy.SkillSlotAId,
                migratedEnemy.SkillSlotBId
            };

            var rows = FilterAndSort(
                model.EnemyMigrationTriggers.Values,
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
                    TriggerAction = SkillTriggerActionKind.TriggeredSkill,
                    TriggeredSkillId = row.TriggeredSkillId,
                    RuntimeKind = row.RuntimeKind,
                    SortOrder = row.SortOrder,
                    ProcChance = 1f
                };
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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
                    RuntimeVisual = BuildRuntimeVisual(skill),
                    ImpactRuntimeVisual = BuildImpactRuntimeVisual(skill),
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
                    EnhancementChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.ActiveEnhancement),
                    MasterSkillChoices = BuildSkillChoices(model, skill.Id, PakuriCsvChoiceGroup.ActiveMaster),
                    MultiEffects = BuildSkillEffects(model, skill.Id),
                    NormalizedPlanNodes = BuildSkillNodeDefinitions(model, SkillNodeOwnerKind.Skill, skill.Id, skill.Id)
                };

                ApplyStatusPayload(definition, skill.Status);
                definitions[i] = definition;
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillEffectDefinition[] BuildSkillEffects(SourceModel model, string skillId)
        {
            return BuildEffectOwnedSkillEffects(model, skillId);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillEffectDefinition[] BuildEffectOwnedSkillEffects(SourceModel model, string skillId)
        {
            var effectNodes = FilterAndSort(
                model.SkillNodes.Values,
                node => node.OwnerKind == SkillNodeOwnerKind.Effect
                    && string.Equals(node.TargetSkillId, skillId, StringComparison.OrdinalIgnoreCase),
                (left, right) =>
                {
                    var ownerCompare = string.Compare(left.OwnerId, right.OwnerId, StringComparison.OrdinalIgnoreCase);
                    return ownerCompare != 0 ? ownerCompare : left.SortOrder.CompareTo(right.SortOrder);
                });

            if (effectNodes.Count == 0)
            {
                return Array.Empty<SkillEffectDefinition>();
            }

            var grouped = new Dictionary<string, List<SkillNodeRow>>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < effectNodes.Count; i++)
            {
                var node = effectNodes[i];
                if (node == null || string.IsNullOrWhiteSpace(node.OwnerId))
                {
                    continue;
                }

                if (!grouped.TryGetValue(node.OwnerId, out var nodes))
                {
                    nodes = new List<SkillNodeRow>();
                    grouped.Add(node.OwnerId, nodes);
                }

                nodes.Add(node);
            }

            var definitions = new List<SkillEffectDefinition>(grouped.Count);
            foreach (var entry in grouped)
            {
                var nodes = entry.Value;
                SkillNodeRow operationNode = null;
                for (var i = 0; i < nodes.Count; i++)
                {
                    if (IsEffectOperationHandler(nodes[i].HandlerId))
                    {
                        operationNode = nodes[i];
                        break;
                    }
                }

                if (operationNode == null)
                {
                    continue;
                }

                var definition = BuildEffectOwnedSkillEffectDefinition(operationNode);
                ApplyEffectOwnedSkillEffectOperationNode(model, definition, operationNode);
                for (var i = 0; i < nodes.Count; i++)
                {
                    var node = nodes[i];
                    if (node == operationNode)
                    {
                        continue;
                    }

                    ApplyEffectOwnedSkillEffectNode(model, definition, node);
                }

                definitions.Add(definition);
            }

            definitions.Sort((left, right) =>
            {
                var sortCompare = left.SortOrder.CompareTo(right.SortOrder);
                return sortCompare != 0
                    ? sortCompare
                    : string.Compare(left.EffectId, right.EffectId, StringComparison.OrdinalIgnoreCase);
            });
            return definitions.ToArray();
        }

        /*
         * 필요한 조건을 만족하는지 확인한다.
         */
        private static bool IsEffectOperationHandler(string handlerId)
        {
            return string.Equals(handlerId, "ApplyStatus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "ApplyShield", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "StatusModifier", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "EffectStatus", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "EffectDamage", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "RecastZone", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillEffectDefinition BuildEffectOwnedSkillEffectDefinition(SkillNodeRow node)
        {
            return new SkillEffectDefinition
            {
                EffectId = node.OwnerId,
                SkillId = node.TargetSkillId,
                SortOrder = node.SortOrder,
                TargetSide = SkillMultiEffectTargetSide.Enemy,
                TargetSelection = SkillMultiEffectTargetSelection.Nearest,
                TargetShape = SkillMultiEffectTargetShape.Single,
                CenterMode = SkillMultiEffectCenterMode.PrimarySkillCenter,
                VisualAnchorMode = SkillMultiEffectVisualAnchorMode.Center,
                EffectTiming = SkillMultiEffectTiming.OnCast,
                EnabledByDefault = node.EnabledByDefault,
                RequiresActiveChoiceId = node.RequiresActiveChoiceId,
                ExcludesActiveChoiceId = node.ExcludesActiveChoiceId,
                RequiresPassiveSkillId = node.RequiresPassiveSkillId,
                ExcludesPassiveSkillId = node.ExcludesPassiveSkillId,
                RuntimeSupportState = node.RuntimeSupportState,
                RuntimeSupportNotes = node.RuntimeSupportNotes,
                DamageMultiplier = 1f,
                StatusChance = 1f,
                StatusMaxStacks = 1,
                StatusStackAmount = 1
            };
        }

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
        private static void ApplyEffectOwnedSkillEffectOperationNode(
            SourceModel model,
            SkillEffectDefinition definition,
            SkillNodeRow node)
        {
            var parameters = BuildSkillNodeParamValueLookup(model, node.Id);
            if (string.Equals(node.HandlerId, "EffectDamage", StringComparison.OrdinalIgnoreCase))
            {
                definition.EffectKind = SkillMultiEffectKind.Damage;
                definition.Attribute = GetSkillNodeEnumParam(parameters, "attribute", DamageAttribute.Physical);
                definition.BaseDamage = GetSkillNodeFloatParam(parameters, "base_damage", 0f);
                definition.AttackPowerCoefficient = GetSkillNodeFloatParam(parameters, "attack_power_coefficient", 0f);
                definition.SpellPowerCoefficient = GetSkillNodeFloatParam(parameters, "spell_power_coefficient", 0f);
                definition.DamageMultiplier = GetSkillNodeFloatParam(parameters, "damage_multiplier", 1f);
                definition.Radius = GetSkillNodeFloatParam(parameters, "radius", 0f);
                definition.TickIntervalSeconds = GetSkillNodeFloatParam(parameters, "tick_interval_seconds", 0f);
            }
            else if (string.Equals(node.HandlerId, "RecastZone", StringComparison.OrdinalIgnoreCase))
            {
                definition.EffectKind = SkillMultiEffectKind.RecastZone;
                definition.RecastSourceSkillId = GetSkillNodeStringParam(parameters, "source_skill_id");
                definition.DelaySeconds = GetSkillNodeFloatParam(parameters, "delay_seconds", 0f);
                definition.RecastDurationSeconds = GetSkillNodeFloatParam(parameters, "duration_seconds", 0f);
                definition.RecastRadiusMultiplier = GetSkillNodeFloatParam(parameters, "radius_multiplier", 1f);
                definition.RecastInheritSnapshot = GetSkillNodeBoolParam(parameters, "inherit_snapshot", true);
                definition.RecastMaxGeneration = GetSkillNodeIntParam(parameters, "max_generation", 1);
            }
            else if (string.Equals(node.HandlerId, "EffectExtendStatusDuration", StringComparison.OrdinalIgnoreCase))
            {
                definition.EffectKind = SkillMultiEffectKind.ExtendStatusDuration;
                ApplyEffectOwnedStatusParams(definition, parameters);
            }
            else if (string.Equals(node.HandlerId, "ApplyShield", StringComparison.OrdinalIgnoreCase))
            {
                definition.EffectKind = SkillMultiEffectKind.Status;
                definition.StatusEffectId = "shield";
                definition.BaseDamage = GetSkillNodeFloatParam(parameters, "base_damage", 0f);
                definition.AttackPowerCoefficient = GetSkillNodeFloatParam(parameters, "attack_power_coefficient", 0f);
                definition.SpellPowerCoefficient = GetSkillNodeFloatParam(parameters, "spell_power_coefficient", 0f);
                definition.DamageMultiplier = GetSkillNodeFloatParam(parameters, "damage_multiplier", 1f);
                ApplyEffectOwnedStatusParams(definition, parameters, keepExistingStatusId: true);
            }
            else if (string.Equals(node.HandlerId, "StatusModifier", StringComparison.OrdinalIgnoreCase))
            {
                definition.EffectKind = SkillMultiEffectKind.Status;
                definition.StatusEffectId = "passive-buff";
                ApplyEffectOwnedStatusParams(definition, parameters, keepExistingStatusId: true);
            }
            else
            {
                definition.EffectKind = SkillMultiEffectKind.Status;
                ApplyEffectOwnedStatusParams(definition, parameters);
            }
        }

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
        private static void ApplyEffectOwnedStatusParams(
            SkillEffectDefinition definition,
            Dictionary<string, string> parameters,
            bool keepExistingStatusId = false)
        {
            if (!keepExistingStatusId)
            {
                definition.StatusEffectId = GetSkillNodeStringParam(parameters, "status_id");
            }

            definition.StatusChance = GetSkillNodeFloatParam(parameters, "status_chance", 1f);
            definition.StatusEffectLabel = GetSkillNodeStringParam(parameters, "status_label");
            definition.StatusEffectPrefab = LoadPrefab(GetSkillNodeStringParam(parameters, "status_effect_prefab_path"));
            definition.StatusDurationSeconds = GetSkillNodeFloatParam(parameters, "status_duration_seconds", 0f);
            definition.StatusMaxStacks = GetSkillNodeIntParam(parameters, "status_max_stacks", 1);
            definition.StatusStackAmount = GetSkillNodeIntParam(parameters, "status_stack_amount", 1);
            definition.StatusTargetScope = GetSkillNodeStringParam(parameters, "status_target_scope");
            definition.StatusMergePolicy = GetSkillNodeStringParam(parameters, "status_merge_policy");
            definition.ShieldAmountRefreshPolicy = GetSkillNodeStringParam(parameters, "shield_amount_refresh_policy");
        }

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
        private static void ApplyEffectOwnedSkillEffectNode(SourceModel model, SkillEffectDefinition definition, SkillNodeRow node)
        {
            var parameters = BuildSkillNodeParamValueLookup(model, node.Id);
            var handlerId = node.HandlerId;
            if (IsEffectOperationHandler(handlerId))
            {
                ApplyEffectOwnedSkillEffectOperationNode(model, definition, node);
                return;
            }

            if (string.Equals(handlerId, "EffectTarget", StringComparison.OrdinalIgnoreCase))
            {
                definition.TargetSide = GetSkillNodeEnumParam(parameters, "target_side", definition.TargetSide);
                definition.TargetSelection = GetSkillNodeEnumParam(parameters, "target_selection", definition.TargetSelection);
                definition.TargetShape = GetSkillNodeEnumParam(parameters, "target_shape", definition.TargetShape);
                definition.CenterMode = GetSkillNodeEnumParam(parameters, "center_mode", definition.CenterMode);
                definition.VisualAnchorMode = GetSkillNodeEnumParam(parameters, "visual_anchor_mode", definition.VisualAnchorMode);
                definition.EffectTiming = GetSkillNodeEnumParam(parameters, "effect_timing", definition.EffectTiming);
                definition.DelaySeconds = GetSkillNodeFloatParam(parameters, "delay_seconds", definition.DelaySeconds);
                definition.ApplyOnce = GetSkillNodeBoolParam(parameters, "apply_once", definition.ApplyOnce);
                definition.CoverAll = GetSkillNodeBoolParam(parameters, "cover_all", definition.CoverAll);
                return;
            }

            if (string.Equals(handlerId, "EffectVisual", StringComparison.OrdinalIgnoreCase))
            {
                definition.SkillEffectPrefab = LoadPrefab(GetSkillNodeStringParam(parameters, "skill_effect_prefab_path"));
                return;
            }

            if (string.Equals(handlerId, "AttachStatusPayload", StringComparison.OrdinalIgnoreCase))
            {
                ApplyEffectOwnedStatusParams(definition, parameters);
                return;
            }

            if (string.Equals(handlerId, "RequiredSourceStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.RequiredSourceStatusId = GetSkillNodeStringParam(parameters, "status_id");
                definition.RequiredSourceStatusMinStacks = GetSkillNodeIntParam(parameters, "min_stacks", 1);
                return;
            }

            if (string.Equals(handlerId, "StatusRuntimeKindFilter", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusConditionalIncomingSkillRuntimeKinds = GetSkillNodeStringParam(
                    parameters,
                    "incoming_skill_runtime_kinds");
                definition.StatusConditionalOutgoingSkillRuntimeKinds = GetSkillNodeStringParam(
                    parameters,
                    "outgoing_skill_runtime_kinds");
                return;
            }

            if (string.Equals(handlerId, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase))
            {
                definition.RuntimeVisual = BuildRuntimeVisual(
                    GetSkillNodeStringParam(parameters, "runtime_visual_sprite_path"),
                    GetSkillNodeStringParam(parameters, "runtime_visual_animator_controller_path"),
                    GetSkillNodeFloatParam(parameters, "runtime_visual_scale", 1f),
                    0f,
                    0f,
                    0f,
                    GetSkillNodeIntParam(parameters, "runtime_visual_sorting_order", 0),
                    GetSkillNodeFloatParam(parameters, "runtime_hitbox_size_x", 0f),
                    GetSkillNodeFloatParam(parameters, "runtime_hitbox_size_y", 0f));
                return;
            }

            if (string.Equals(handlerId, "ConditionStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.ConditionStatusId = BuildConditionStatusExpression(parameters);
                definition.ConditionTargetSide = GetSkillNodeEnumParam(parameters, "target_side", definition.TargetSide);
                definition.ConditionStatusSourceSkillId = GetSkillNodeStringParam(parameters, "source_skill_id");
                return;
            }

            if (string.Equals(handlerId, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.ConditionStatusId = GetSkillNodeStringParam(parameters, "status_ids");
                definition.ConditionTargetSide = GetSkillNodeEnumParam(parameters, "target_side", definition.TargetSide);
                definition.ConditionStatusSourceSkillId = GetSkillNodeStringParam(parameters, "source_skill_id");
                return;
            }

            if (string.Equals(handlerId, "ConditionSkillAttribute", StringComparison.OrdinalIgnoreCase))
            {
                definition.ConditionSkillAttribute = GetSkillNodeStringParam(parameters, "attribute");
                return;
            }

            if (string.Equals(handlerId, "ConditionHealthRatioMax", StringComparison.OrdinalIgnoreCase))
            {
                definition.ConditionHealthRatioMax = GetSkillNodeFloatParam(parameters, "ratio", 0f);
                return;
            }

            if (string.Equals(handlerId, "ConditionHitCountMin", StringComparison.OrdinalIgnoreCase))
            {
                definition.ConditionHitCountMin = GetSkillNodeIntParam(parameters, "min_targets", 0);
                return;
            }

            if (string.Equals(handlerId, "EffectLifetime", StringComparison.OrdinalIgnoreCase))
            {
                var duration = GetSkillNodeFloatParam(parameters, "duration_seconds", 0f);
                if (definition.EffectKind == SkillMultiEffectKind.Damage)
                {
                    definition.ActiveDurationSeconds = duration;
                }
                else
                {
                definition.StatusDurationSeconds = duration;
            }

            return;
        }

            var bonus = GetSkillNodeFloatParam(parameters, "bonus", 0f);
            if (string.Equals(handlerId, "StatusActionSpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusActionSpeedBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusMoveSpeedBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusMoveSpeedBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusAttackPowerBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusAttackPowerBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusSpellPowerBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusSpellPowerBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusDamageBonusRate", StringComparison.OrdinalIgnoreCase))
            {
                definition.Attribute = GetSkillNodeEnumParam(parameters, "attribute", definition.Attribute);
                definition.StatusDamageBonusRate += bonus;
            }
            else if (string.Equals(handlerId, "StatusShieldReceivedBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusShieldReceivedBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusDamageTakenBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusFlatElementResistReduction", StringComparison.OrdinalIgnoreCase))
            {
                definition.Attribute = GetSkillNodeEnumParam(parameters, "attribute", definition.Attribute);
                definition.StatusFlatElementResistReduction += bonus;
            }
            else if (string.Equals(handlerId, "StatusCriticalChanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusCriticalChanceBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusCriticalResistanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusCriticalResistanceBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusCriticalDamageBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusCriticalDamageBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusElementResistReduction", StringComparison.OrdinalIgnoreCase))
            {
                definition.Attribute = GetSkillNodeEnumParam(parameters, "attribute", definition.Attribute);
                definition.StatusElementResistReduction += bonus;
            }
            else if (string.Equals(handlerId, "StatusOutgoingAdditionalDamage", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusOutgoingAdditionalDamageMultiplier += GetSkillNodeFloatParam(parameters, "multiplier", 0f);
                definition.StatusOutgoingAdditionalDamageTriggerAttribute = GetSkillNodeEnumParam(
                    parameters,
                    "trigger_attribute",
                    DamageAttribute.Physical);
                definition.StatusOutgoingAdditionalDamageAttribute = GetSkillNodeEnumParam(
                    parameters,
                    "damage_attribute",
                    DamageAttribute.Physical);
            }
            else if (string.Equals(handlerId, "StatusElementDamageTakenBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.Attribute = GetSkillNodeEnumParam(parameters, "attribute", definition.Attribute);
                definition.StatusElementDamageTakenBonus += bonus;
            }
            else if (string.Equals(handlerId, "StatusConditionalStatusChanceBonus", StringComparison.OrdinalIgnoreCase))
            {
                definition.StatusConditionalTargetStatusId = GetSkillNodeStringParam(parameters, "status_ids");
                definition.StatusConditionalStatusChanceBonus += bonus;
            }
            else if (string.Equals(handlerId, "DamageMultiplier", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerId, "ShieldAmountMultiplier", StringComparison.OrdinalIgnoreCase))
            {
                definition.DamageMultiplier *= GetSkillNodeFloatParam(parameters, "multiplier", 1f);
            }
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static string BuildConditionStatusExpression(Dictionary<string, string> parameters)
        {
            var statusId = GetSkillNodeStringParam(parameters, "status_id");
            var minStacks = GetSkillNodeIntParam(parameters, "min_stacks", 1);
            if (string.IsNullOrWhiteSpace(statusId) || minStacks <= 1)
            {
                return statusId;
            }

            return string.Concat(statusId, ":", minStacks.ToString(CultureInfo.InvariantCulture));
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static Dictionary<string, string> BuildSkillNodeParamValueLookup(SourceModel model, string nodeId)
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

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static string GetSkillNodeStringParam(Dictionary<string, string> parameters, string key)
        {
            return parameters.TryGetValue(key, out var value) ? value : string.Empty;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static float GetSkillNodeFloatParam(Dictionary<string, string> parameters, string key, float defaultValue)
        {
            return parameters.TryGetValue(key, out var value)
                && float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : defaultValue;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static int GetSkillNodeIntParam(Dictionary<string, string> parameters, string key, int defaultValue)
        {
            return parameters.TryGetValue(key, out var value)
                && int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
                    ? parsed
                    : defaultValue;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static bool GetSkillNodeBoolParam(Dictionary<string, string> parameters, string key, bool defaultValue)
        {
            return parameters.TryGetValue(key, out var value)
                && bool.TryParse(value, out var parsed)
                    ? parsed
                    : defaultValue;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static TEnum GetSkillNodeEnumParam<TEnum>(
            Dictionary<string, string> parameters,
            string key,
            TEnum defaultValue)
            where TEnum : struct
        {
            return parameters.TryGetValue(key, out var value)
                && Enum.TryParse(value, true, out TEnum parsed)
                    ? parsed
                    : defaultValue;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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
                    RequiredSourceStatusId = trigger.RequiredSourceStatusId,
                    RequiredSourceStatusMinStacks = trigger.RequiredSourceStatusMinStacks,
                    ConditionStatusId = trigger.ConditionStatusId,
                    ConditionStatusSourceSkillId = trigger.ConditionStatusSourceSkillId,
                    TriggerAttribute = trigger.TriggerAttribute,
                    TriggerAction = trigger.TriggerAction,
                    EventSkillId = trigger.EventSkillId,
                    EventSkillRuntimeKinds = trigger.EventSkillRuntimeKinds,
                    ProcChance = trigger.ProcChance,
                    InternalCooldownSeconds = trigger.InternalCooldownSeconds,
                    TriggeredSkillId = trigger.TriggeredSkillId,
                    TargetSkillId = trigger.TargetSkillId,
                    TriggeredEffectId = ResolveTriggeredEffectId(trigger),
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
                    RuntimeVisual = BuildRuntimeVisual(trigger),
                    RuntimeSupportState = trigger.RuntimeSupportState,
                    RuntimeSupportNotes = trigger.RuntimeSupportNotes
                };
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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
                    PassiveEffects = BuildSkillEffects(model, skill.Id),
                    NormalizedPlanNodes = BuildSkillNodeDefinitions(model, SkillNodeOwnerKind.Passive, skill.Id, skill.Id)
                };
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
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
                var targetSkillId = string.IsNullOrWhiteSpace(choice.TargetSkillId) ? choice.SkillId : choice.TargetSkillId;
                var normalizedPlanNodes = BuildSkillNodeDefinitions(
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
                    RuntimeTargetSkillIds = choice.RuntimeTargetSkillIds,
                    ChoiceGroup = MapChoiceGroup(choice.ChoiceGroup),
                    Title = choice.Title,
                    SkillIcon = LoadSprite(choice.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(GetChoicePlanNodeParam(
                        normalizedPlanNodes,
                        "EffectVisual",
                        "skill_effect_prefab_path")),
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
                    HasBurstDamageProjectileIndex = choice.HasBurstDamageProjectileIndex,
                    BurstDamageProjectileIndex = choice.BurstDamageProjectileIndex,
                    HasBurstDamageMultiplier = choice.HasBurstDamageMultiplier,
                    BurstDamageMultiplier = choice.HasBurstDamageMultiplier ? choice.BurstDamageMultiplier : 1f,
                    HasBurstStatusProjectileIndex = choice.HasBurstStatusProjectileIndex,
                    BurstStatusProjectileIndex = choice.BurstStatusProjectileIndex,
                    BurstStatusStacksBonus = choice.BurstStatusStacksBonus,
                    FollowUpProjectileCount = choice.FollowUpProjectileCount,
                    FollowUpProjectileDelaySeconds = choice.FollowUpProjectileDelaySeconds,
                    FollowUpProjectileDamageMultiplier = choice.FollowUpProjectileDamageMultiplier > 0f ? choice.FollowUpProjectileDamageMultiplier : 1f,
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
                    HasStatusActionSpeedBonus = choice.HasStatusActionSpeedBonus,
                    StatusActionSpeedBonus = choice.StatusActionSpeedBonus,
                    HasStatusAttackPowerBonus = choice.HasStatusAttackPowerBonus,
                    StatusAttackPowerBonus = choice.StatusAttackPowerBonus,
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
                    HasTargetStatusStackDamageMultiplier = choice.HasTargetStatusStackDamageMultiplier,
                    TargetStatusStackDamageMultiplier = choice.HasTargetStatusStackDamageMultiplier ? choice.TargetStatusStackDamageMultiplier : 1f,
                    HasConsumeTargetStatusRatioOverride = choice.HasConsumeTargetStatusRatioOverride,
                    ConsumeTargetStatusRatioOverride = choice.ConsumeTargetStatusRatioOverride,
                    HasConsumeTargetStatusStacksOverride = choice.HasConsumeTargetStatusStacksOverride,
                    ConsumeTargetStatusStacksOverride = choice.ConsumeTargetStatusStacksOverride,
                    ConditionalCritChanceBonus = choice.ConditionalCritChanceBonus,
                    ConditionalCritTargetStatusId = choice.ConditionalCritTargetStatusId,
                    ConditionalCritTargetStatusMinStacks = choice.ConditionalCritTargetStatusMinStacks,
                    RedistributeConsumedStatusRatioOnKill = choice.RedistributeConsumedStatusRatioOnKill,
                    RedistributeConsumedStatusId = choice.RedistributeConsumedStatusId,
                    RedistributeConsumedStatusSearchRadius = choice.RedistributeConsumedStatusSearchRadius,
                    RedistributeConsumedStatusTargetCount = choice.RedistributeConsumedStatusTargetCount,
                    CountStatusId = choice.CountStatusId,
                    CountTargetSide = choice.CountTargetSide,
                    DamageMultiplierPerCount = choice.DamageMultiplierPerCount,
                    CountMax = choice.CountMax,
                    ConsecutiveHitBonusRate = choice.ConsecutiveHitBonusRate,
                    ConsecutiveHitMax = choice.ConsecutiveHitMax,
                    HasStatusConditionalDamageTakenBonus = choice.HasStatusConditionalDamageTakenBonus,
                    StatusConditionalDamageTakenBonus = choice.StatusConditionalDamageTakenBonus,
                    StatusConditionalSourceStatusId = choice.StatusConditionalSourceStatusId,
                    RequiredSourceStatusId = GetChoicePlanNodeParam(
                        normalizedPlanNodes,
                        "RequiredSourceStatus",
                        "status_id"),
                    RequiredSourceStatusMinStacks = GetChoicePlanNodeIntParam(
                        normalizedPlanNodes,
                        "RequiredSourceStatus",
                        "min_stacks",
                        1),
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
                    RepeatCountPerTarget = choice.RepeatCountPerTarget,
                    RepeatIntervalSeconds = choice.RepeatIntervalSeconds,
                    RepeatDamageMultiplier = choice.RepeatDamageMultiplier > 0f ? choice.RepeatDamageMultiplier : 1f,
                    NormalizedPlanNodes = normalizedPlanNodes,
                    RuntimeSupportState = choice.RuntimeSupportState,
                    RuntimeSupportNotes = choice.RuntimeSupportNotes
                };
            }

            return definitions;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static string GetChoicePlanNodeParam(
            SkillNodeDefinition[] nodes,
            string handlerId,
            string paramKey)
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
                        return param.Value ?? string.Empty;
                    }
                }
            }

            return string.Empty;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        private static int GetChoicePlanNodeIntParam(
            SkillNodeDefinition[] nodes,
            string handlerId,
            string paramKey,
            int fallback)
        {
            var raw = GetChoicePlanNodeParam(nodes, handlerId, paramKey);
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                ? value
                : fallback;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillNodeDefinition[] BuildSkillNodeDefinitions(
            SourceModel model,
            SkillNodeOwnerKind ownerKind,
            string ownerId,
            string defaultTargetSkillId)
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
                definitions[i] = new SkillNodeDefinition
                {
                    NodeId = node.Id,
                    OwnerKind = node.OwnerKind.ToString(),
                    OwnerId = node.OwnerId,
                    TargetSkillId = string.IsNullOrWhiteSpace(node.TargetSkillId) ? defaultTargetSkillId : node.TargetSkillId,
                    NodeKind = node.NodeKind.ToString(),
                    HandlerId = node.HandlerId,
                    SortOrder = node.SortOrder,
                    EnabledByDefault = node.EnabledByDefault,
                    RequiresActiveChoiceId = node.RequiresActiveChoiceId,
                    ExcludesActiveChoiceId = node.ExcludesActiveChoiceId,
                    RequiresPassiveSkillId = node.RequiresPassiveSkillId,
                    ExcludesPassiveSkillId = node.ExcludesPassiveSkillId,
                    RuntimeSupportState = node.RuntimeSupportState,
                    RuntimeSupportNotes = node.RuntimeSupportNotes,
                    Params = BuildSkillNodeParamDefinitions(model, node.Id)
                };
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static SkillNodeParamDefinition[] BuildSkillNodeParamDefinitions(SourceModel model, string nodeId)
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
                    NodeId = param.NodeId,
                    ParamKey = param.ParamKey,
                    ValueType = param.ValueType.ToString(),
                    Value = param.Value
                };
            }

            return definitions;
        }

        /*
         * 조건에 맞는 항목만 정렬해 반환한다.
         */
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

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
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

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
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
            definition.StatusConditionalIncomingSkillRuntimeKinds = payload.StatusConditionalIncomingSkillRuntimeKinds;
            definition.StatusConditionalOutgoingSkillRuntimeKinds = payload.StatusConditionalOutgoingSkillRuntimeKinds;
            definition.StatusAppliedStatusDurationBonusStatusId = payload.StatusAppliedStatusDurationBonusStatusId;
            definition.StatusAppliedStatusDurationBonus = payload.StatusAppliedStatusDurationBonus;
            definition.StatusOutgoingAdditionalDamageMultiplier = payload.StatusOutgoingAdditionalDamageMultiplier;
            definition.StatusOutgoingAdditionalDamageTriggerAttribute = payload.StatusOutgoingAdditionalDamageTriggerAttribute;
            definition.StatusOutgoingAdditionalDamageAttribute = payload.StatusOutgoingAdditionalDamageAttribute;
        }

        /*
         * 원본 값을 런타임 형식으로 바꾼다.
         */
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

        /*
         * 해당 자료 변환에 필요한 값을 구성한다.
         */
        private static IEnumerable<CatalogEntryRow> SortCatalogEntries(Dictionary<string, CatalogEntryRow> entries)
        {
            var list = new List<CatalogEntryRow>(entries.Values);
            list.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            return list;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static RuntimeSkillVisualSpec BuildRuntimeVisual(SkillRow row)
        {
            return row == null
                ? new RuntimeSkillVisualSpec()
                : BuildRuntimeVisual(
                    row.RuntimeVisualSpritePath,
                    row.RuntimeVisualAnimatorControllerPath,
                    row.RuntimeVisualScale,
                    row.RuntimeVisualScaleX,
                    row.RuntimeVisualScaleY,
                    row.RuntimeVisualScaleZ,
                    row.RuntimeVisualSortingOrder,
                    row.RuntimeHitboxSizeX,
                    row.RuntimeHitboxSizeY,
                    row.RuntimeVisualAnchor,
                    row.RuntimeHitboxOffsetX,
                    row.RuntimeHitboxOffsetY);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static RuntimeSkillVisualSpec BuildRuntimeVisual(SkillTriggerRow row)
        {
            return row == null
                ? new RuntimeSkillVisualSpec()
                : BuildRuntimeVisual(
                    row.RuntimeVisualSpritePath,
                    row.RuntimeVisualAnimatorControllerPath,
                    row.RuntimeVisualScale,
                    0f,
                    0f,
                    0f,
                    row.RuntimeVisualSortingOrder,
                    row.RuntimeHitboxSizeX,
                    row.RuntimeHitboxSizeY,
                    row.RuntimeVisualAnchor,
                    row.RuntimeHitboxOffsetX,
                    row.RuntimeHitboxOffsetY);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static RuntimeSkillVisualSpec BuildImpactRuntimeVisual(SkillRow row)
        {
            return row == null
                ? new RuntimeSkillVisualSpec()
                : BuildRuntimeVisual(
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        private static RuntimeSkillVisualSpec BuildRuntimeVisual(
            string spritePath,
            string animatorControllerPath,
            float scale,
            float scaleX,
            float scaleY,
            float scaleZ,
            int sortingOrder,
            float hitboxSizeX,
            float hitboxSizeY,
            string visualAnchor = null,
            float hitboxOffsetX = 0f,
            float hitboxOffsetY = 0f)
        {
            var anchor = RuntimeSkillVisualAnchor.Skill;
            if (!string.IsNullOrWhiteSpace(visualAnchor)
                && !Enum.TryParse(visualAnchor, true, out anchor))
            {
                anchor = RuntimeSkillVisualAnchor.Skill;
            }

            var useLocalScale = scaleX != 0f || scaleY != 0f || scaleZ != 0f;
            return new RuntimeSkillVisualSpec
            {
                Sprite = LoadSprite(spritePath),
                AnimatorController = LoadAnimatorController(animatorControllerPath),
                Scale = scale > 0f ? scale : 1f,
                UseLocalScale = useLocalScale,
                LocalScale = useLocalScale
                    ? new Vector3(
                        scaleX != 0f ? scaleX : 1f,
                        scaleY != 0f ? scaleY : 1f,
                        scaleZ != 0f ? scaleZ : 1f)
                    : Vector3.one,
                SortingOrder = sortingOrder,
                Anchor = anchor,
                Hitbox = new RuntimeSkillHitboxSpec
                {
                    Size = new Vector2(Mathf.Max(0f, hitboxSizeX), Mathf.Max(0f, hitboxSizeY)),
                    Offset = new Vector2(hitboxOffsetX, hitboxOffsetY)
                }
            };
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        private static RuntimeAnimatorController LoadAnimatorController(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (runtimeAssetCatalog != null && runtimeAssetCatalog.TryGetAnimatorController(assetPath, out var animatorController))
            {
                return animatorController;
            }

            return null;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
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
