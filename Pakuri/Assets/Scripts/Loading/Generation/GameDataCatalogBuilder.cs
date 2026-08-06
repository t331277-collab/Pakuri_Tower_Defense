/*
 * 역할: 핵심 런타임 카탈로그 생성.
 * 책임: 파싱된 유닛·상태·보상·공통 행을 색인된 런타임 정의로 변환한다.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;

namespace Pakuri.Data
{

    /// GameDataCatalogBuilder 런타임 데이터를 파싱된 저작 데이터에서 생성한다.
    internal sealed partial class GameDataCatalogBuilder
    {

        private readonly CsvRuntimeCatalog assetCatalog;

        private GameDataCatalogBuilder(CsvRuntimeCatalog assetCatalog)
        {
            this.assetCatalog = assetCatalog ?? throw new ArgumentNullException(nameof(assetCatalog));
        }

        internal static GameDataCatalog BuildRuntimeCatalog(
            SourceModel model,
            CsvRuntimeCatalog assetCatalog)
        {
            return new GameDataCatalogBuilder(assetCatalog).Build(model);
        }

        private GameDataCatalog Build(SourceModel model)
        {
            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            catalog.StatusEffects = BuildStatusEffects(model);

            var monsters = new List<MonsterDefinition>();
            foreach (var entry in SortCatalogEntries(model.CatalogMonsters))
            {
                var sourceMonster = model.Monsters[entry.RefName];
                var monster = ScriptableObject.CreateInstance<MonsterDefinition>();
                monster.MonsterName = sourceMonster.Name;
                monster.DisplayName = sourceMonster.DisplayName;
                monster.RoleSummary = sourceMonster.RoleSummary;
                monster.ElementLabel = sourceMonster.ElementLabel;
                monster.PrimaryAttribute = sourceMonster.PrimaryAttribute;
                monster.ActiveSkillName = ResolveMonsterSkillDisplayName(
                    model,
                    sourceMonster.Name,
                    PakuriCsvSkillKind.Active,
                    SkillSlot.A);
                monster.PassiveSkillName = ResolveMonsterSkillDisplayName(
                    model,
                    sourceMonster.Name,
                    PakuriCsvSkillKind.Passive,
                    SkillSlot.F);
                monster.MonsterIconImage = LoadSprite(sourceMonster.MonsterIconImagePath);
                monster.Image = LoadSprite(sourceMonster.ImagePath);
                monster.PowerStat = sourceMonster.PowerStat;
                monster.BaseStats = new UnitCombatStats
                {
                    MaxHealth = sourceMonster.MaxHealth,
                    AttackPower = sourceMonster.BaseAttackPower,
                    SpellPower = sourceMonster.BaseSpellPower,
                    MoveSpeed = sourceMonster.BaseMoveSpeed,
                    CriticalChance = sourceMonster.BaseCriticalChance,
                    CriticalDamage = sourceMonster.BaseCriticalDamage
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
                monster.InitialRewardChoices = BuildRewardChoices(model, sourceMonster.Name);
                monster.ActiveSkills = BuildActiveSkills(
                    model,
                    sourceMonster.Name,
                    catalog.StatusEffects);
                monster.PassiveSkills = BuildPassiveSkills(model, monster);
                var reactions = BuildSkillReactions(
                    model,
                    sourceMonster.Name,
                    monster.ActiveSkills,
                    catalog.StatusEffects);
                AttachSkillReactions(
                    monster.ActiveSkills,
                    monster.PassiveSkills,
                    reactions);
                AttachNormalCastEffects(
                    model,
                    sourceMonster.Name,
                    monster.ActiveSkills,
                    monster.PassiveSkills,
                    catalog.StatusEffects);
                monsters.Add(monster);
            }

            catalog.Monsters = monsters.ToArray();
            catalog.Summons = BuildSummons(model, catalog.StatusEffects);
            BuildArtifactDefinitions(model, catalog, catalog.StatusEffects);
            catalog.StageOneEnemies = BuildEnemies(model, "stage_one", catalog.StatusEffects);
            catalog.StageTwoEnemies = BuildEnemies(model, "stage_two", catalog.StatusEffects);
            catalog.Stage = StageDefinitionBuilder.Build(assetCatalog);
            return catalog;
        }

        private string ResolveMonsterSkillDisplayName(
            SourceModel model,
            string monsterName,
            PakuriCsvSkillKind skillKind,
            SkillSlot slot)
        {
            if (model != null && model.Skills != null)
            {
                foreach (var skill in model.Skills.Values)
                {
                    if (skill.SkillKind == skillKind
                        && skill.Slot == slot
                        && string.Equals(skill.MonsterName, monsterName, StringComparison.OrdinalIgnoreCase))
                    {
                        return skill.DisplayName;
                    }
                }
            }

            throw new CsvFatalException(
                $"Monster '{monsterName}' has no '{skillKind}' skill in slot '{slot}'.");
        }

        private EnemyDefinition[] BuildEnemies(
            SourceModel model,
            string stageName,
            StatusEffectDefinition[] statusDefinitions)
        {
            var enemies = new List<EnemyDefinition>();
            var sourceEnemies = FilterAndSort(
                model.Enemies.Values,
                row => string.Equals(row.StageName, stageName, StringComparison.OrdinalIgnoreCase),
                (left, right) =>
                {
                    var orderCompare = left.SortOrder.CompareTo(right.SortOrder);
                    if (orderCompare != 0)
                    {
                        return orderCompare;
                    }

                    return string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
                });
            for (var i = 0; i < sourceEnemies.Count; i++)
            {
                var sourceEnemy = sourceEnemies[i];
                var enemy = ScriptableObject.CreateInstance<EnemyDefinition>();
                enemy.EnemyName = sourceEnemy.Name;
                enemy.DisplayName = sourceEnemy.DisplayName;
                enemy.Image = LoadSprite(sourceEnemy.ImagePath);
                enemy.Attribute = sourceEnemy.Attribute;
                enemy.Stats = new UnitCombatStats
                {
                    MaxHealth = sourceEnemy.MaxHealth,
                    AttackPower = sourceEnemy.AttackPower,
                    SpellPower = sourceEnemy.SpellPower,
                    MoveSpeed = sourceEnemy.MoveSpeed,
                    CriticalChance = sourceEnemy.CriticalChance,
                    CriticalDamage = sourceEnemy.CriticalDamage
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
                enemy.ActiveSkills = BuildEnemyAssignedActiveSkills(
                    model,
                    sourceEnemy.Name,
                    statusDefinitions);
                var reactions = BuildEnemyAssignedSkillReactions(
                    model,
                    sourceEnemy.Name,
                    enemy.ActiveSkills,
                    statusDefinitions);
                AttachSkillReactions(
                    enemy.ActiveSkills,
                    null,
                    reactions);
                enemy.PassiveSkill = BuildEnemyPassiveDefinition(
                    model,
                    sourceEnemy.PassiveName,
                    reactions);
                enemy.NexusDamage = sourceEnemy.NexusDamage;
                enemies.Add(enemy);
            }

            return enemies.ToArray();
        }

        private PassiveSkillDefinition BuildEnemyPassiveDefinition(
            SourceModel model,
            string passiveName,
            SkillReaction[] reactions)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(passiveName)
                || !model.EnemyBaseSkills.TryGetValue(passiveName, out var source)
                || source == null
                || source.Skill == null)
            {
                return null;
            }

            return new PassiveSkillDefinition
            {
                SkillName = source.Skill.Name,
                DisplayName = source.Skill.DisplayName,
                Slot = SkillSlot.F,
                RuntimeKind = SkillRuntimeKind.Passive,
                ImplementationState = source.Skill.ImplementationState,
                IsActive = false,
                IsAvailableWithoutActiveRequirement = true,
                ModifierKind = source.PassiveModifierKind,
                HasModifierAttribute = source.PassiveHasAttribute,
                ModifierAttribute = source.PassiveAttribute,
                ModifierValue = source.PassiveModifierValue,
                Nodes = AppendReactionNodes(
                    Array.Empty<SkillNode>(),
                    reactions,
                    source.Skill.Name)
            };
        }

        private SkillDefinition[] BuildEnemyAssignedActiveSkills(
            SourceModel model,
            string enemyName,
            StatusEffectDefinition[] statusDefinitions)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyName)
                || !model.Enemies.TryGetValue(enemyName, out var enemyRow))
            {
                return Array.Empty<SkillDefinition>();
            }

            var definitions = new List<SkillDefinition>(2);
            TryAddEnemyAssignedSkillDefinition(
                model,
                enemyName,
                enemyRow.SkillSlotAName,
                SkillSlot.A,
                statusDefinitions,
                definitions);
            TryAddEnemyAssignedSkillDefinition(
                model,
                enemyName,
                enemyRow.SkillSlotBName,
                SkillSlot.B,
                statusDefinitions,
                definitions);

            return definitions.ToArray();
        }

        private void TryAddEnemyAssignedSkillDefinition(
            SourceModel model,
            string ownerName,
            string skillName,
            SkillSlot runtimeSlot,
            StatusEffectDefinition[] statusDefinitions,
            List<SkillDefinition> definitions)
        {
            if (model == null
                || definitions == null
                || string.IsNullOrWhiteSpace(skillName)
                || !model.EnemyBaseSkills.TryGetValue(skillName, out var source)
                || source == null
                || source.Skill == null)
            {
                return;
            }

            definitions.Add(BuildActiveDefinition(
                ownerName,
                BuildEnemyAssignedSkillDefinition(source, runtimeSlot),
                statusDefinitions));
        }

        private ActiveSkillBuildData BuildEnemyAssignedSkillDefinition(EnemyBaseSkillRow source, SkillSlot runtimeSlot)
        {
            var row = source.Skill;
            var definition = new ActiveSkillBuildData
            {
                SkillName = row.Name,
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

        private void ApplyEnemyExecutionProfile(ActiveSkillBuildData definition)
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

        private string MapEnemyTargetSelection(string selection)
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

        private SkillReaction[] BuildEnemyAssignedSkillReactions(
            SourceModel model,
            string enemyName,
            SkillDefinition[] activeSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyName)
                || !model.Enemies.TryGetValue(enemyName, out var enemyRow))
            {
                return Array.Empty<SkillReaction>();
            }

            var assignedSkillNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                enemyRow.SkillSlotAName,
                enemyRow.SkillSlotBName
            };

            var rows = FilterAndSort(
                model.EnemyTriggers.Values,
                trigger => trigger.Enabled && assignedSkillNames.Contains(trigger.SourceSkillName),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));
            var definitions = new List<SkillReaction>(rows.Count + assignedSkillNames.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var normalizedNodes = new[]
                {
                    new SkillNodeBuildData
                    {
                        OwnerKind = SkillNodeOwnerKind.Trigger.ToString(),
                        TargetSkillName = row.SourceSkillName,
                        HandlerName = "ExecuteSkill",
                        EnabledByDefault = true,
                        Params = new[]
                        {
                            new SkillNodeParamBuildData
                            {
                                ParamKey = "skill_name",
                                Value = row.TriggeredSkillName
                            },
                            new SkillNodeParamBuildData
                            {
                                ParamKey = "damage_multiplier",
                                Value = "1"
                            }
                        }
                    }
                };
                var definition = new SkillReaction
                {
                    ReactionName = row.Name,
                    SourceSkillName = row.SourceSkillName,
                    Event = row.TriggerEvent,
                    SortOrder = row.SortOrder,
                    ProcChance = 1f,
                    EventSourceScope = SkillTriggerEventSourceScope.Any
                };
                BuildReactionOutcome(
                    definition,
                    normalizedNodes,
                    statusDefinitions,
                    activeSkills);
                definitions.Add(definition);
            }

            foreach (var skillName in assignedSkillNames)
            {
                if (!model.EnemyBaseSkills.TryGetValue(skillName, out var source)
                    || source == null
                    || source.Skill == null
                    || !string.Equals(
                        source.ExecutionProfile,
                        "DamageThenDelayedChain",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var chainTrigger = BuildEnemyChainReaction(enemyName, source, activeSkills);
                if (chainTrigger != null)
                {
                    definitions.Add(chainTrigger);
                }
            }

            return definitions.ToArray();
        }

        /// 연쇄 공격의 후속타를 공용 Trigger와 Single 스킬로 구성한다.
        private static SkillReaction BuildEnemyChainReaction(
            string enemyName,
            EnemyBaseSkillRow source,
            SkillDefinition[] activeSkills)
        {
            SingleSkillDefinition sourceSkill = null;
            for (var i = 0; activeSkills != null && i < activeSkills.Length; i++)
            {
                if (activeSkills[i] is SingleSkillDefinition single
                    && string.Equals(
                        single.SkillName,
                        source.Skill.Name,
                        StringComparison.OrdinalIgnoreCase))
                {
                    sourceSkill = single;
                    break;
                }
            }
            if (sourceSkill == null)
            {
                return null;
            }

            var searchRadius = source.ChainRadius > 0f
                ? source.ChainRadius
                : source.EffectRadius;
            var followUp = new SkillCastEffect
            {
                EffectName = source.Skill.Name,
                ResolvedDefinition = new SingleSkillDefinition
                {
                    SkillName = source.Skill.Name,
                    DisplayName = source.Skill.Name,
                    RuntimeKind = SkillRuntimeKind.SingleAttack,
                    ImplementationState = SkillImplementationState.RuntimeImplemented,
                    SkillEffectPrefab = sourceSkill.SkillEffectPrefab,
                    RuntimeVisual = sourceSkill.RuntimeVisual,
                    Targeting = new SkillTargetingSpec
                    {
                        TargetSide = SkillTargetSide.Enemy,
                        Selection = source.ExcludePrimaryTarget
                            ? SkillTargetSelection.NearestOtherFromEventTarget
                            : SkillTargetSelection.Nearest,
                        Shape = SkillTargetShape.Single,
                        Radius = searchRadius
                    },
                    Area = new AreaBlueprintSpec
                    {
                        Radius = 0f,
                        CoverAll = false
                    },
                    UsesHitTargetCount = true,
                    HitTargetCount = 1,
                    Damage = sourceSkill.Damage
                }
            };

            return new SkillReaction
            {
                ReactionName = source.Skill.Name + "__chain_on_hit",
                SourceSkillName = source.Skill.Name,
                Event = SkillTriggerEvent.OnHit,
                ProcChance = 1f,
                DelaySeconds = Mathf.Max(0f, source.ChainDelaySeconds),
                EventSourceScope = SkillTriggerEventSourceScope.Any,
                Effect = followUp,
                DamageMultiplier = Mathf.Max(0f, source.ChainDamageMultiplier),
                LockToEventTarget = !source.ExcludePrimaryTarget,
                CenterMode = SkillTriggerCenterMode.EventTarget,
                PublishSkillLifecycleEvents = false
            };
        }

        private StatusEffectDefinition[] BuildStatusEffects(SourceModel model)
        {
            var statuses = new List<StatusEffectDefinition>();
            foreach (var row in model.StatusEffects.Values)
            {
                var kind = StatusValueParser.ParseStatusKind(row.Name);
                var definition = new StatusEffectDefinition
                {
                    StatusEffectName = row.Name,
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
                    ElementResistReductionPerStack = row.ElementResistReductionPerStack,
                    ElementDamageTakenBonusPerStack = row.ElementDamageTakenBonusPerStack,
                    StatusEffectPrefab = LoadPrefab(row.StatusEffectPrefabPath)
                };
                definition.RuntimeData = BuildStatusRuntimeData(definition);
                statuses.Add(definition);
            }

            statuses.Sort((left, right) => string.Compare(left.StatusEffectName, right.StatusEffectName, StringComparison.OrdinalIgnoreCase));
            return statuses.ToArray();
        }

        private static StatusRuntimeData BuildStatusRuntimeData(StatusEffectDefinition definition)
        {
            var status = new StatusRuntimeData
            {
                Definition = definition,
                Kind = definition.Kind,
                StatusTag = definition.Name,
                StatusName = definition.StatusEffectLabel,
                Duration = definition.DefaultDurationSeconds,
                MaxStacks = definition.MaxStacks,
                IsStackable = definition.MaxStacks != 1,
                BaseStackAmount = definition.BaseStackAmount,
                Permanent = definition.IsPermanent && definition.DefaultDurationSeconds <= 0f,
                CanMove = definition.CanMove,
                CanAct = definition.CanAct,
                CanUseSpecialSkill = definition.CanUseSpecialSkill,
                MoveSpeedBonus = definition.MoveSpeedBonusPerStack,
                DamageTakenBonus = definition.DamageTakenBonusPerStack,
                CriticalDamageTakenBonus = definition.CriticalDamageTakenBonusPerStack,
                ElementResistReduction = definition.ElementResistReductionPerStack,
                ElementDamageTakenBonus = definition.ElementDamageTakenBonusPerStack,
                StatusEffectPrefab = definition.StatusEffectPrefab
            };
            if (status.MoveSpeedBonus < 0f)
            {
                status.MovementSlowRate = -status.MoveSpeedBonus;
            }

            status.Modifiers.ActionSpeedBonus = definition.ActionSpeedBonusPerStack;
            status.Modifiers.AttackPowerBonus = definition.AttackPowerBonusPerStack;
            if (definition.HasAttribute)
            {
                status.HasElementModifierTarget = true;
                status.ElementModifierTarget = definition.Attribute;
                status.Modifiers.ResistReductionElement = definition.Attribute;
            }

            status.Modifiers.ResistReduction = status.ElementResistReduction;
            status.IsControlEffect = !status.CanMove || !status.CanAct || !status.CanUseSpecialSkill;
            return status;
        }

        private static StatusRuntimeData GetStatusRuntimeData(
            StatusEffectKind kind,
            StatusEffectDefinition[] definitions,
            string label = null)
        {
            if (definitions != null)
            {
                for (var i = 0; i < definitions.Length; i++)
                {
                    var definition = definitions[i];
                    if (definition?.Kind != kind || definition.RuntimeData == null)
                    {
                        continue;
                    }

                    var status = definition.RuntimeData.Clone();
                    if (!string.IsNullOrWhiteSpace(label))
                    {
                        status.StatusName = label;
                    }

                    return status;
                }
            }

            throw new KeyNotFoundException($"Status definition '{kind}' is not registered.");
        }

        private MonsterDefinition.RewardChoiceDefinition[] BuildRewardChoices(SourceModel model, string monsterName)
        {
            var rewards = FilterAndSort(
                model.RewardChoices.Values,
                reward => string.Equals(reward.MonsterName, monsterName, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new MonsterDefinition.RewardChoiceDefinition[rewards.Count];
            for (var i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                definitions[i] = new MonsterDefinition.RewardChoiceDefinition
                {
                    RewardName = reward.Name,
                    ActiveSkillName = reward.ActiveSkillName,
                    PassiveSkillName = reward.PassiveSkillName
                };
            }

            return definitions;
        }

        private SkillDefinition[] BuildActiveSkills(
            SourceModel model,
            string monsterName,
            StatusEffectDefinition[] statusDefinitions,
            IEnumerable<SkillRow> sourceRows = null)
        {
            var skills = FilterAndSort(
                sourceRows ?? model.Skills.Values,
                skill => skill.SkillKind == PakuriCsvSkillKind.Active
                    && string.Equals(skill.MonsterName, monsterName, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new SkillDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                var definition = new ActiveSkillBuildData
                {
                    SkillName = skill.Name,
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
                    TargetSelectionStatusName = skill.TargetSelectionStatusName,
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
                    DeploymentRequiredTargetStatusName = skill.DeploymentRequiredTargetStatusName,
                    DeploymentRequiredTargetStatusMinStacks = skill.DeploymentRequiredTargetStatusMinStacks,
                    TargetStatusStackStatusName = skill.TargetStatusStackStatusName,
                    TargetStatusStackMaxStacks = skill.TargetStatusStackMaxStacks,
                    TargetStatusStackBaseDamage = skill.TargetStatusStackBaseDamage,
                    TargetStatusStackAttackPowerCoefficient = skill.TargetStatusStackAttackPowerCoefficient,
                    TargetStatusStackSpellPowerCoefficient = skill.TargetStatusStackSpellPowerCoefficient,
                    ConsumeTargetStatusName = skill.ConsumeTargetStatusName,
                    ConsumeTargetStatusRatio = skill.ConsumeTargetStatusRatio,
                    ConsumeTargetStatusStacks = skill.ConsumeTargetStatusStacks,
                    Summary = skill.Summary,
                    EnhancementChoices = BuildSkillChoices(model, skill.Name, SkillChoiceGroup.ActiveEnhancement),
                    MasterSkillChoices = BuildSkillChoices(model, skill.Name, SkillChoiceGroup.ActiveMaster),
                    Nodes = BuildSkillNodes(model, SkillNodeOwnerKind.Skill, skill.Name, skill.Name)
                };

                ApplyStatusPayload(definition, skill.Status);
                definitions[i] = BuildActiveDefinition(
                    monsterName,
                    definition,
                    statusDefinitions);
            }

            return definitions;
        }

        private Dictionary<string, string> BuildSkillNodeParamValueLookup(SourceModel model, string nodeName)
        {
            var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < model.SkillNodeParams.Count; i++)
            {
                var param = model.SkillNodeParams[i];
                if (param != null && string.Equals(param.NodeName, nodeName, StringComparison.OrdinalIgnoreCase))
                {
                    parameters[param.ParamKey] = param.Value;
                }
            }

            return parameters;
        }

        private string GetSkillNodeStringParam(Dictionary<string, string> parameters, string key)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }

        private float GetSkillNodeFloatParam(Dictionary<string, string> parameters, string key, float defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private int GetSkillNodeIntParam(Dictionary<string, string> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private SkillReaction[] BuildSkillReactions(
            SourceModel model,
            string monsterName,
            SkillDefinition[] activeSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            return BuildSkillReactions(
                model,
                trigger => string.Equals(
                    trigger.MonsterName,
                    monsterName,
                    StringComparison.OrdinalIgnoreCase),
                activeSkills,
                statusDefinitions);
        }

        private SkillReaction[] BuildSkillReactions(
            SourceModel model,
            Predicate<SkillTriggerRow> includes,
            SkillDefinition[] activeSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            var triggers = FilterAndSort(
                model.SkillTriggers.Values,
                trigger => includes(trigger) && !IsNormalCastEffect(trigger),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillReaction[triggers.Count];
            for (var i = 0; i < triggers.Count; i++)
            {
                var trigger = triggers[i];
                var normalizedNodes = BuildSkillNodes(
                    model,
                    GetPassiveTriggerOwnerKind(model, trigger),
                    trigger.Name,
                    trigger.SourceSkillName);
                definitions[i] = new SkillReaction
                {
                    ReactionName = trigger.Name,
                    SourceSkillName = trigger.SourceSkillName,
                    Event = trigger.TriggerEvent,
                    RequiredActiveChoiceNames = StatusValueParser.ParseIdList(trigger.RequiresActiveChoiceName),
                    ExcludedActiveChoiceNames = StatusValueParser.ParseIdList(trigger.ExcludesActiveChoiceName),
                    RequiredSourceStatusKind = string.IsNullOrWhiteSpace(trigger.RequiredSourceStatusName)
                        ? StatusEffectKind.None
                        : StatusValueParser.ParseStatusKind(trigger.RequiredSourceStatusName),
                    RequiredSourceStatusMinStacks = trigger.RequiredSourceStatusMinStacks,
                    ConditionStatuses = StatusValueParser.ParseConditionStatusExpression(trigger.ConditionStatusName),
                    ConditionStatusSourceSkillNames = StatusValueParser.ParseIdList(trigger.ConditionStatusSourceSkillName),
                    TriggerAttributes = StatusValueParser.ParseDamageAttributes(trigger.TriggerAttribute),
                    EventSkillNames = StatusValueParser.ParseIdList(trigger.EventSkillName),
                    EventSkillRuntimeKindValues = StatusValueParser.ParseSkillRuntimeKindConditions(
                        trigger.EventSkillRuntimeKinds),
                    ProcChance = trigger.ProcChance,
                    InternalCooldownSeconds = trigger.InternalCooldownSeconds,
                    SortOrder = trigger.SortOrder,
                    RepeatCount = trigger.RepeatCount,
                    RepeatIntervalSeconds = trigger.RepeatIntervalSeconds,
                    DelaySeconds = trigger.TriggerDelaySeconds,
                    EveryCount = trigger.TriggerEveryCount,
                    EventSourceScope = StatusValueParser.ParseEventSourceScope(trigger.EventSourceScope),
                    RequireEventExecute = trigger.RequireEventExecute,
                    RequireEventCritical = trigger.RequireEventCritical
                };
                BuildReactionOutcome(
                    definitions[i],
                    normalizedNodes,
                    statusDefinitions,
                    activeSkills);
                if (definitions[i].Effect == null
                    && definitions[i].Command == null
                    && HasHandler(normalizedNodes, "StatusModifier"))
                {
                    definitions[i].Effect = BuildNormalStatusModifierEffect(
                        trigger,
                        normalizedNodes,
                        statusDefinitions);
                    definitions[i].Effect.DelaySeconds = 0f;
                }
            }

            return definitions;
        }

        private static bool IsNormalCastEffect(SkillTriggerRow trigger)
        {
            return trigger != null
                && (trigger.TriggerEvent == SkillTriggerEvent.OnCast
                    || (trigger.TriggerEvent == SkillTriggerEvent.OnSkillCast
                        && string.IsNullOrWhiteSpace(trigger.EventSkillName)));
        }

        private static SkillNodeOwnerKind GetPassiveTriggerOwnerKind(
            SourceModel model,
            SkillTriggerRow trigger)
        {
            if (trigger != null
                && string.IsNullOrWhiteSpace(trigger.RequiresActiveChoiceName)
                && model.Skills.TryGetValue(trigger.SourceSkillName, out var sourceSkill)
                && sourceSkill.SkillKind == PakuriCsvSkillKind.Passive
                && model.SkillNodes.Values.Any(node =>
                    node.OwnerKind == SkillNodeOwnerKind.Base
                    && string.Equals(node.OwnerName, trigger.Name, StringComparison.OrdinalIgnoreCase)))
            {
                return SkillNodeOwnerKind.Base;
            }

            return SkillNodeOwnerKind.Trigger;
        }

        private void AttachNormalCastEffects(
            SourceModel model,
            string monsterName,
            SkillDefinition[] activeSkills,
            PassiveSkillDefinition[] passiveSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            var rows = FilterAndSort(
                model.SkillTriggers.Values,
                trigger => string.Equals(
                        trigger.MonsterName,
                        monsterName,
                        StringComparison.OrdinalIgnoreCase)
                    && IsNormalCastEffect(trigger),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (string.Equals(
                        row.Name,
                        "eve-h-trait-3",
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        row.Name,
                        "ariel-a-master-2",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var nodes = BuildSkillNodes(
                    model,
                    GetPassiveTriggerOwnerKind(model, row),
                    row.Name,
                    row.SourceSkillName);
                var effectNode = BuildNormalCastEffectNode(
                    row,
                    nodes,
                    statusDefinitions,
                    activeSkills,
                    passiveSkills);
                if (effectNode == null)
                {
                    continue;
                }

                var source = FindSkillDefinition(
                    activeSkills,
                    passiveSkills,
                    row.SourceSkillName);
                if (source == null)
                {
                    throw new InvalidOperationException(
                        "Normal cast effect source is not registered: " + row.SourceSkillName);
                }

                if (string.IsNullOrWhiteSpace(row.RequiresActiveChoiceName))
                {
                    source.Nodes = AppendNode(source.Nodes, effectNode);
                    continue;
                }

                var choice = FindSkillChoice(source, row.RequiresActiveChoiceName);
                if (choice == null)
                {
                    throw new InvalidOperationException(
                        "Normal cast effect choice is not registered: "
                        + row.RequiresActiveChoiceName);
                }
                choice.Nodes = AppendNode(choice.Nodes, effectNode);
            }
        }

        private static SkillDefinition FindSkillDefinition(
            SkillDefinition[] activeSkills,
            PassiveSkillDefinition[] passiveSkills,
            string skillName)
        {
            if (activeSkills != null)
            {
                for (var i = 0; i < activeSkills.Length; i++)
                {
                    if (activeSkills[i] != null
                        && string.Equals(
                            activeSkills[i].SkillName,
                            skillName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return activeSkills[i];
                    }
                }
            }
            if (passiveSkills != null)
            {
                for (var i = 0; i < passiveSkills.Length; i++)
                {
                    if (passiveSkills[i] != null
                        && string.Equals(
                            passiveSkills[i].SkillName,
                            skillName,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        return passiveSkills[i];
                    }
                }
            }
            return null;
        }

        private static SkillChoice FindSkillChoice(
            SkillDefinition skill,
            string choiceName)
        {
            var choice = FindSkillChoice(skill?.EnhancementChoices, choiceName)
                ?? FindSkillChoice(skill?.MasterChoices, choiceName);
            if (choice != null)
            {
                return choice;
            }
            return null;
        }

        private static SkillChoice FindSkillChoice(
            SkillChoice[] choices,
            string choiceName)
        {
            if (choices == null)
            {
                return null;
            }
            for (var i = 0; i < choices.Length; i++)
            {
                if (choices[i] != null
                    && string.Equals(
                        choices[i].ChoiceName,
                        choiceName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return choices[i];
                }
            }
            return null;
        }

        private static SkillNode[] AppendNode(SkillNode[] nodes, SkillNode node)
        {
            var length = nodes?.Length ?? 0;
            var result = new SkillNode[length + 1];
            if (length > 0)
            {
                Array.Copy(nodes, result, length);
            }
            result[length] = node;
            return result;
        }

        /// reaction을 source Skill/Passive Node에 연결한다.
        private static void AttachSkillReactions(
            SkillDefinition[] skills,
            PassiveSkillDefinition[] passives,
            SkillReaction[] reactions)
        {
            for (var i = 0; skills != null && i < skills.Length; i++)
            {
                if (skills[i] != null)
                {
                    skills[i].Nodes = AppendReactionNodes(
                        skills[i].Nodes,
                        reactions,
                        skills[i].SkillName);
                }
            }
            for (var i = 0; passives != null && i < passives.Length; i++)
            {
                if (passives[i] != null)
                {
                    passives[i].Nodes = AppendReactionNodes(
                        passives[i].Nodes,
                        reactions,
                        passives[i].SkillName);
                }
            }
        }

        private static SkillNode[] AppendReactionNodes(
            SkillNode[] nodes,
            SkillReaction[] reactions,
            string sourceSkillName)
        {
            var result = nodes ?? Array.Empty<SkillNode>();
            for (var i = 0; reactions != null && i < reactions.Length; i++)
            {
                if (reactions[i] != null
                    && string.Equals(
                        reactions[i].SourceSkillName,
                        sourceSkillName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result = AppendNode(
                        result,
                        SkillNode.FromOperation(
                            new SkillReactionOp(reactions[i]),
                            sourceSkillName));
                }
            }
            return result;
        }

        private PassiveSkillDefinition[] BuildPassiveSkills(
            SourceModel model,
            MonsterDefinition monster)
        {
            var monsterName = monster.MonsterName;
            var skills = FilterAndSort(
                model.Skills.Values,
                skill => skill.SkillKind == PakuriCsvSkillKind.Passive
                    && string.Equals(skill.MonsterName, monsterName, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new PassiveSkillDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                var definition = new PassiveSkillBuildData
                {
                    PassiveName = skill.Name,
                    DisplayName = skill.DisplayName,
                    Slot = skill.Slot,
                    RequiredActiveSlot = skill.RequiredActiveSlot,
                    IsAvailableWithoutActiveRequirement = skill.IsAvailableWithoutActiveRequirement,
                    ImplementationState = skill.ImplementationState,
                    SkillIcon = LoadSprite(skill.SkillIconPath),
                    DescriptionText = skill.DescriptionText,
                    Summary = skill.Summary,
                    EnhancementChoices = BuildSkillChoices(model, skill.Name, SkillChoiceGroup.PassiveEnhancement),
                    BaseNodes = BuildPassiveBaseNodes(model, skill.Name),
                    Nodes = BuildSkillNodes(model, SkillNodeOwnerKind.Passive, skill.Name, skill.Name)
                };
                definitions[i] = BuildPassiveDefinition(monster, definition);
            }

            return definitions;
        }

        private SkillNodeBuildData[] BuildPassiveBaseNodes(
            SourceModel model,
            string passiveSkillName)
        {
            var result = new List<SkillNodeBuildData>();
            var triggers = FilterAndSort(
                model.SkillTriggers.Values,
                trigger => string.Equals(
                        trigger.SourceSkillName,
                        passiveSkillName,
                        StringComparison.OrdinalIgnoreCase)
                    && string.IsNullOrWhiteSpace(trigger.RequiresActiveChoiceName)
                    && GetPassiveTriggerOwnerKind(model, trigger) == SkillNodeOwnerKind.Base,
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            for (var i = 0; i < triggers.Count; i++)
            {
                var graphRows = model.SkillNodes.Values
                    .Where(node => node.OwnerKind == SkillNodeOwnerKind.Base
                        && string.Equals(
                            node.OwnerName,
                            triggers[i].Name,
                        StringComparison.OrdinalIgnoreCase))
                    .ToArray();
                if (graphRows.Length == 0
                    || graphRows.Any(node => !IsPassiveBaseSnapshotNode(node.HandlerName)))
                {
                    continue;
                }

                result.AddRange(BuildSkillNodes(
                    model,
                    SkillNodeOwnerKind.Base,
                    triggers[i].Name,
                    triggers[i].SourceSkillName));
            }

            return result.ToArray();
        }

        private static bool IsPassiveBaseSnapshotNode(string handlerName)
        {
            return string.Equals(handlerName, "DurationMultiplier", StringComparison.OrdinalIgnoreCase)
                || string.Equals(handlerName, "ShotIntervalMultiplier", StringComparison.OrdinalIgnoreCase);
        }

        private SkillChoiceBuildData[] BuildSkillChoices(SourceModel model, string skillName, SkillChoiceGroup choiceGroup)
        {
            var choices = FilterAndSort(
                model.SkillChoices.Values,
                choice => choice.ChoiceGroup == choiceGroup
                    && string.Equals(choice.SkillName, skillName, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillChoiceBuildData[choices.Count];
            for (var i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var targetSkillName = choice.TargetSkillName;
                if (string.IsNullOrWhiteSpace(targetSkillName))
                {
                    targetSkillName = choice.SkillName;
                }
                var normalizedNodes = BuildSkillNodes(
                    model,
                    SkillNodeOwnerKind.Choice,
                    choice.Name,
                    targetSkillName);

                definitions[i] = new SkillChoiceBuildData
                {
                    ChoiceName = choice.Name,
                    MonsterName = choice.MonsterName,
                    SkillName = choice.SkillName,
                    TargetSkillName = targetSkillName,
                    ChoiceGroup = choice.ChoiceGroup,
                    Title = choice.Title,
                    SkillIcon = LoadSprite(choice.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(GetChoiceNodeParam(
                        normalizedNodes,
                        "EffectVisual",
                        "skill_effect_prefab_path")),
                    DescriptionText = choice.DescriptionText,
                    Nodes = normalizedNodes
                };
            }

            return definitions;
        }

        private string GetChoiceNodeParam(
            SkillNodeBuildData[] nodes,
            string handlerName,
            string paramKey)
        {
            if (nodes == null || string.IsNullOrWhiteSpace(handlerName) || string.IsNullOrWhiteSpace(paramKey))
            {
                return string.Empty;
            }

            for (var nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                var node = nodes[nodeIndex];
                if (node == null
                    || !node.EnabledByDefault
                    || !string.Equals(node.HandlerName, handlerName, StringComparison.OrdinalIgnoreCase)
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

        private SkillNodeBuildData[] BuildSkillNodes(
            SourceModel model,
            SkillNodeOwnerKind ownerKind,
            string ownerName,
            string defaultTargetSkillName)
        {
            var nodes = FilterAndSort(
                model.SkillNodes.Values,
                node => node.OwnerKind == ownerKind
                    && string.Equals(node.OwnerName, ownerName, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            if (nodes.Count == 0)
            {
                return Array.Empty<SkillNodeBuildData>();
            }

            var definitions = new SkillNodeBuildData[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var targetSkillName = node.TargetSkillName;
                if (string.IsNullOrWhiteSpace(targetSkillName))
                {
                    targetSkillName = defaultTargetSkillName;
                }

                var definition = new SkillNodeBuildData
                {
                    OwnerKind = node.OwnerKind.ToString(),
                    TargetSkillName = targetSkillName,
                    HandlerName = node.HandlerName,
                    EnabledByDefault = node.EnabledByDefault,
                    Params = BuildSkillNodeParams(model, node.Name)
                };

                if (string.Equals(node.HandlerName, "EffectVisual", StringComparison.OrdinalIgnoreCase))
                {
                    var parameters = BuildSkillNodeParamValueLookup(model, node.Name);
                    definition.ResolvedPrefab = LoadPrefab(
                        GetSkillNodeStringParam(parameters, "skill_effect_prefab_path"));
                }
                else if (string.Equals(node.HandlerName, "RuntimeEffectVisual", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(node.HandlerName, "ShowVisual", StringComparison.OrdinalIgnoreCase))
                {
                    var parameters = BuildSkillNodeParamValueLookup(model, node.Name);
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

        private SkillNodeParamBuildData[] BuildSkillNodeParams(SourceModel model, string nodeName)
        {
            var nodeParams = FilterAndSort(
                model.SkillNodeParams,
                param => string.Equals(param.NodeName, nodeName, StringComparison.OrdinalIgnoreCase),
                (left, right) => string.Compare(left.ParamKey, right.ParamKey, StringComparison.OrdinalIgnoreCase));

            if (nodeParams.Count == 0)
            {
                return Array.Empty<SkillNodeParamBuildData>();
            }

            var definitions = new SkillNodeParamBuildData[nodeParams.Count];
            for (var i = 0; i < nodeParams.Count; i++)
            {
                var param = nodeParams[i];
                definitions[i] = new SkillNodeParamBuildData
                {
                    ParamKey = param.ParamKey,
                    Value = param.Value
                };
            }

            return definitions;
        }

        private List<T> FilterAndSort<T>(
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

        private void ApplyStatusPayload(ActiveSkillBuildData definition, StatusPayloadRow payload)
        {
            if (definition == null || payload == null)
            {
                return;
            }

            definition.StatusEffectName = payload.StatusEffectName;
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
            definition.StatusElementResistReduction = payload.StatusElementResistReduction;
            definition.StatusFlatElementResistReduction = payload.StatusFlatElementResistReduction;
            definition.StatusElementDamageTakenBonus = payload.StatusElementDamageTakenBonus;
        }

        private IEnumerable<CatalogEntryRow> SortCatalogEntries(Dictionary<string, CatalogEntryRow> entries)
        {
            var list = new List<CatalogEntryRow>(entries.Values);
            list.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            return list;
        }

        private Sprite LoadSprite(string assetPath)
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

        private RuntimeSkillVisualSpec BuildRuntimeVisual(SkillRow row)
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

        private RuntimeSkillVisualSpec BuildImpactRuntimeVisual(SkillRow row)
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
            string spritePath,
            string animatorControllerPath,
            float scale,
            float scaleX,
            float scaleY,
            float scaleZ,
            int sortingOrder,
            float hitboxSizeX,
            float hitboxSizeY,
            string visualAnchor = null)
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

        private RuntimeAnimatorController LoadAnimatorController(string assetPath)
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

        private GameObject LoadPrefab(string assetPath)
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
