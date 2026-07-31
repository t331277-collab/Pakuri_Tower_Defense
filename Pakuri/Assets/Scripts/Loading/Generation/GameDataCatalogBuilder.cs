/*
 * 역할: 핵심 런타임 카탈로그 생성.
 * 책임: 파싱된 유닛·상태·보상·공통 행을 색인된 런타임 정의로 변환한다.
 */

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

    /// GameDataCatalogBuilder 런타임 데이터를 파싱된 저작 데이터에서 생성한다.
    internal sealed partial class GameDataCatalogBuilder
    {

        private readonly CsvRuntimeCatalog assetCatalog;

        /// GameDataCatalogBuilder 인스턴스를 전달된 런타임 입력값으로 초기화한다.
        private GameDataCatalogBuilder(CsvRuntimeCatalog assetCatalog)
        {
            this.assetCatalog = assetCatalog ?? throw new ArgumentNullException(nameof(assetCatalog));
        }

        /// 전달된 런타임 입력값을 사용해 RuntimeCatalog를 구성한다.
        internal static GameDataCatalog BuildRuntimeCatalog(
            SourceModel model,
            CsvRuntimeCatalog assetCatalog)
        {
            return new GameDataCatalogBuilder(assetCatalog).Build(model);
        }

        /// 전달된 model 값을 사용해 요청값를 구성한다.
        private GameDataCatalog Build(SourceModel model)
        {
            var catalog = ScriptableObject.CreateInstance<GameDataCatalog>();
            catalog.StatusEffects = BuildStatusEffects(model);

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
                monster.ActiveSkills = BuildActiveSkills(
                    model,
                    sourceMonster.Id,
                    catalog.StatusEffects);
                monster.PassiveSkills = BuildPassiveSkills(model, monster);
                var reactions = BuildSkillReactions(
                    model,
                    sourceMonster.Id,
                    monster.ActiveSkills,
                    catalog.StatusEffects);
                AttachSkillReactions(
                    monster.ActiveSkills,
                    monster.PassiveSkills,
                    reactions);
                AttachNormalCastEffects(
                    model,
                    sourceMonster.Id,
                    monster.ActiveSkills,
                    monster.PassiveSkills,
                    catalog.StatusEffects);
                monsters.Add(monster);
            }

            catalog.Monsters = monsters.ToArray();
            catalog.StageOneEnemies = BuildEnemies(model, "stage_one", catalog.StatusEffects);
            catalog.StageTwoEnemies = BuildEnemies(model, "stage_two", catalog.StatusEffects);
            return catalog;
        }

        /// 전달된 런타임 입력값을 사용해 MonsterSkillDisplayName를 결정한다.
        private string ResolveMonsterSkillDisplayName(
            SourceModel model,
            string monsterId,
            PakuriCsvSkillKind skillKind,
            SkillSlot slot)
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

        /// 전달된 런타임 입력값을 사용해 Enemies를 구성한다.
        private EnemyDefinition[] BuildEnemies(
            SourceModel model,
            string stageId,
            StatusEffectDefinition[] statusDefinitions)
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
                enemy.ActiveSkills = BuildEnemyAssignedActiveSkills(
                    model,
                    sourceEnemy.Id,
                    statusDefinitions);
                var reactions = BuildEnemyAssignedSkillReactions(
                    model,
                    sourceEnemy.Id,
                    enemy.ActiveSkills,
                    statusDefinitions);
                AttachSkillReactions(
                    enemy.ActiveSkills,
                    null,
                    reactions);
                enemy.PassiveSkill = BuildEnemyPassiveDefinition(
                    model,
                    sourceEnemy.PassiveId,
                    reactions);
                enemy.NexusDamage = sourceEnemy.NexusDamage;
                enemies.Add(enemy);
            }

            return enemies.ToArray();
        }

        /// 전달된 런타임 입력값을 사용해 EnemyPassiveDefinition를 구성한다.
        private PassiveSkillDefinition BuildEnemyPassiveDefinition(
            SourceModel model,
            string passiveId,
            SkillReaction[] reactions)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(passiveId)
                || !model.EnemyBaseSkills.TryGetValue(passiveId, out var source)
                || source == null
                || source.Skill == null)
            {
                return null;
            }

            return new PassiveSkillDefinition
            {
                SkillId = source.Skill.Id,
                SkillName = source.Skill.DisplayName,
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
                    source.Skill.Id)
            };
        }

        /// 전달된 런타임 입력값을 사용해 EnemyAssignedActiveSkills를 구성한다.
        private SkillDefinition[] BuildEnemyAssignedActiveSkills(
            SourceModel model,
            string enemyId,
            StatusEffectDefinition[] statusDefinitions)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyId)
                || !model.Enemies.TryGetValue(enemyId, out var enemyRow))
            {
                return Array.Empty<SkillDefinition>();
            }

            var definitions = new List<SkillDefinition>(2);
            TryAddEnemyAssignedSkillDefinition(
                model,
                enemyId,
                enemyRow.SkillSlotAId,
                SkillSlot.A,
                statusDefinitions,
                definitions);
            TryAddEnemyAssignedSkillDefinition(
                model,
                enemyId,
                enemyRow.SkillSlotBId,
                SkillSlot.B,
                statusDefinitions,
                definitions);

            return definitions.ToArray();
        }

        /// 전달된 런타임 입력값을 사용해 AddEnemyAssignedSkillDefinition 작업을 시도하고 성공 여부를 반환한다.
        private void TryAddEnemyAssignedSkillDefinition(
            SourceModel model,
            string ownerId,
            string skillId,
            SkillSlot runtimeSlot,
            StatusEffectDefinition[] statusDefinitions,
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

            definitions.Add(BuildActiveDefinition(
                ownerId,
                BuildEnemyAssignedSkillDefinition(source, runtimeSlot),
                statusDefinitions));
        }

        /// 전달된 런타임 입력값을 사용해 EnemyAssignedSkillDefinition를 구성한다.
        private ActiveSkillBuildData BuildEnemyAssignedSkillDefinition(EnemyBaseSkillRow source, SkillSlot runtimeSlot)
        {
            var row = source.Skill;
            var definition = new ActiveSkillBuildData
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

        /// 전달된 definition 값을 사용해 EnemyExecutionProfile를 적용한다.
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

        /// 전달된 selection 값을 사용해 EnemyTargetSelection를 대응시킨다.
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

        /// 전달된 런타임 입력값을 사용해 EnemyAssignedSkillTriggers를 구성한다.
        private SkillReaction[] BuildEnemyAssignedSkillReactions(
            SourceModel model,
            string enemyId,
            SkillDefinition[] activeSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(enemyId)
                || !model.Enemies.TryGetValue(enemyId, out var enemyRow))
            {
                return Array.Empty<SkillReaction>();
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
            var definitions = new List<SkillReaction>(rows.Count + assignedSkillIds.Count);
            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var normalizedNodes = new[]
                {
                    new SkillNodeBuildData
                    {
                        OwnerKind = SkillNodeOwnerKind.Trigger.ToString(),
                        TargetSkillId = row.SourceSkillId,
                        HandlerId = "ExecuteSkill",
                        EnabledByDefault = true,
                        Params = new[]
                        {
                            new SkillNodeParamBuildData
                            {
                                ParamKey = "skill_id",
                                Value = row.TriggeredSkillId
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
                    ReactionId = row.Id,
                    SourceSkillId = row.SourceSkillId,
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

            foreach (var skillId in assignedSkillIds)
            {
                if (!model.EnemyBaseSkills.TryGetValue(skillId, out var source)
                    || source == null
                    || source.Skill == null
                    || !string.Equals(
                        source.ExecutionProfile,
                        "DamageThenDelayedChain",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var chainTrigger = BuildEnemyChainReaction(enemyId, source, activeSkills);
                if (chainTrigger != null)
                {
                    definitions.Add(chainTrigger);
                }
            }

            return definitions.ToArray();
        }

        /// 연쇄 공격의 후속타를 공용 Trigger와 Single 스킬로 구성한다.
        private static SkillReaction BuildEnemyChainReaction(
            string enemyId,
            EnemyBaseSkillRow source,
            SkillDefinition[] activeSkills)
        {
            SingleSkillDefinition sourceSkill = null;
            for (var i = 0; activeSkills != null && i < activeSkills.Length; i++)
            {
                if (activeSkills[i] is SingleSkillDefinition single
                    && string.Equals(
                        single.SkillId,
                        source.Skill.Id,
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
                EffectId = source.Skill.Id,
                ResolvedDefinition = new SingleSkillDefinition
                {
                    SkillId = source.Skill.Id,
                    SkillName = source.Skill.Id,
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
                ReactionId = source.Skill.Id + "__chain_on_hit",
                SourceSkillId = source.Skill.Id,
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

        /// 전달된 model 값을 사용해 StatusEffects를 구성한다.
        private StatusEffectDefinition[] BuildStatusEffects(SourceModel model)
        {
            var statuses = new List<StatusEffectDefinition>();
            foreach (var row in model.StatusEffects.Values)
            {
                var kind = StatusValueParser.ParseStatusKind(row.Id);
                var definition = new StatusEffectDefinition
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
                };
                definition.RuntimeData = BuildStatusRuntimeData(definition);
                statuses.Add(definition);
            }

            statuses.Sort((left, right) => string.Compare(left.StatusEffectId, right.StatusEffectId, StringComparison.OrdinalIgnoreCase));
            return statuses.ToArray();
        }

        /// 전달된 definition 값을 사용해 StatusRuntimeData를 구성한다.
        private static StatusRuntimeData BuildStatusRuntimeData(StatusEffectDefinition definition)
        {
            var status = new StatusRuntimeData
            {
                Definition = definition,
                Kind = definition.Kind,
                StatusTag = definition.Id,
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
                CriticalResistanceBonus = definition.CriticalResistanceBonusPerStack,
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

        /// 전달된 런타임 입력값을 사용해 StatusRuntimeData를 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 RewardChoices를 구성한다.
        private MonsterDefinition.RewardChoiceDefinition[] BuildRewardChoices(SourceModel model, string monsterId)
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

        /// 전달된 런타임 입력값을 사용해 ActiveSkills를 구성한다.
        private SkillDefinition[] BuildActiveSkills(
            SourceModel model,
            string monsterId,
            StatusEffectDefinition[] statusDefinitions)
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
                var definition = new ActiveSkillBuildData
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
                    Nodes = BuildSkillNodes(model, SkillNodeOwnerKind.Skill, skill.Id, skill.Id)
                };

                ApplyStatusPayload(definition, skill.Status);
                definitions[i] = BuildActiveDefinition(
                    monsterId,
                    definition,
                    statusDefinitions);
            }

            return definitions;
        }

        /// 전달된 런타임 입력값을 사용해 SkillNodeParamValueLookup를 구성한다.
        private Dictionary<string, string> BuildSkillNodeParamValueLookup(SourceModel model, string nodeId)
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

        /// 전달된 런타임 입력값을 사용해 SkillNodeStringParam를 반환한다.
        private string GetSkillNodeStringParam(Dictionary<string, string> parameters, string key)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }

        /// 전달된 런타임 입력값을 사용해 SkillNodeFloatParam를 반환한다.
        private float GetSkillNodeFloatParam(Dictionary<string, string> parameters, string key, float defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /// 전달된 런타임 입력값을 사용해 SkillNodeIntParam를 반환한다.
        private int GetSkillNodeIntParam(Dictionary<string, string> parameters, string key, int defaultValue)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /// 전달된 런타임 입력값을 사용해 SkillTriggers를 구성한다.
        private SkillReaction[] BuildSkillReactions(
            SourceModel model,
            string monsterId,
            SkillDefinition[] activeSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            var triggers = FilterAndSort(
                model.SkillTriggers.Values,
                trigger => string.Equals(trigger.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase)
                    && !IsNormalCastEffect(trigger),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillReaction[triggers.Count];
            for (var i = 0; i < triggers.Count; i++)
            {
                var trigger = triggers[i];
                var normalizedNodes = BuildSkillNodes(
                    model,
                    SkillNodeOwnerKind.Trigger,
                    trigger.Id,
                    trigger.SourceSkillId);
                definitions[i] = new SkillReaction
                {
                    ReactionId = trigger.Id,
                    SourceSkillId = trigger.SourceSkillId,
                    Event = trigger.TriggerEvent,
                    RequiredActiveChoiceIds = StatusValueParser.ParseIdList(trigger.RequiresActiveChoiceId),
                    ExcludedActiveChoiceIds = StatusValueParser.ParseIdList(trigger.ExcludesActiveChoiceId),
                    RequiredSourceStatusKind = string.IsNullOrWhiteSpace(trigger.RequiredSourceStatusId)
                        ? StatusEffectKind.None
                        : StatusValueParser.ParseStatusKind(trigger.RequiredSourceStatusId),
                    RequiredSourceStatusMinStacks = trigger.RequiredSourceStatusMinStacks,
                    ConditionStatuses = StatusValueParser.ParseConditionStatusExpression(trigger.ConditionStatusId),
                    ConditionStatusSourceSkillIds = StatusValueParser.ParseIdList(trigger.ConditionStatusSourceSkillId),
                    TriggerAttributes = StatusValueParser.ParseDamageAttributes(trigger.TriggerAttribute),
                    EventSkillIds = StatusValueParser.ParseIdList(trigger.EventSkillId),
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
                    RequireEventExecute = trigger.RequireEventExecute
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
                        && string.IsNullOrWhiteSpace(trigger.EventSkillId)));
        }

        private void AttachNormalCastEffects(
            SourceModel model,
            string monsterId,
            SkillDefinition[] activeSkills,
            PassiveSkillDefinition[] passiveSkills,
            StatusEffectDefinition[] statusDefinitions)
        {
            var rows = FilterAndSort(
                model.SkillTriggers.Values,
                trigger => string.Equals(
                        trigger.MonsterId,
                        monsterId,
                        StringComparison.OrdinalIgnoreCase)
                    && IsNormalCastEffect(trigger),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            for (var i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (string.Equals(
                        row.Id,
                        "eve-h-trait-3",
                        StringComparison.OrdinalIgnoreCase)
                    || string.Equals(
                        row.Id,
                        "ariel-a-master-2",
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var nodes = BuildSkillNodes(
                    model,
                    SkillNodeOwnerKind.Trigger,
                    row.Id,
                    row.SourceSkillId);
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
                    row.SourceSkillId);
                if (source == null)
                {
                    throw new InvalidOperationException(
                        "Normal cast effect source is not registered: " + row.SourceSkillId);
                }

                if (string.IsNullOrWhiteSpace(row.RequiresActiveChoiceId))
                {
                    source.Nodes = AppendNode(source.Nodes, effectNode);
                    continue;
                }

                var choice = FindSkillChoice(source, row.RequiresActiveChoiceId);
                if (choice == null)
                {
                    throw new InvalidOperationException(
                        "Normal cast effect choice is not registered: "
                        + row.RequiresActiveChoiceId);
                }
                choice.Nodes = AppendNode(choice.Nodes, effectNode);
            }
        }

        private static SkillDefinition FindSkillDefinition(
            SkillDefinition[] activeSkills,
            PassiveSkillDefinition[] passiveSkills,
            string skillId)
        {
            if (activeSkills != null)
            {
                for (var i = 0; i < activeSkills.Length; i++)
                {
                    if (activeSkills[i] != null
                        && string.Equals(
                            activeSkills[i].SkillId,
                            skillId,
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
                            passiveSkills[i].SkillId,
                            skillId,
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
            string choiceId)
        {
            var choice = FindSkillChoice(skill?.EnhancementChoices, choiceId)
                ?? FindSkillChoice(skill?.MasterChoices, choiceId);
            if (choice != null)
            {
                return choice;
            }
            return skill is PassiveSkillDefinition passive
                ? FindSkillChoice(passive.BaseModifierChoices, choiceId)
                : null;
        }

        private static SkillChoice FindSkillChoice(
            SkillChoice[] choices,
            string choiceId)
        {
            if (choices == null)
            {
                return null;
            }
            for (var i = 0; i < choices.Length; i++)
            {
                if (choices[i] != null
                    && string.Equals(
                        choices[i].ChoiceId,
                        choiceId,
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
                        skills[i].SkillId);
                }
            }
            for (var i = 0; passives != null && i < passives.Length; i++)
            {
                if (passives[i] != null)
                {
                    passives[i].Nodes = AppendReactionNodes(
                        passives[i].Nodes,
                        reactions,
                        passives[i].SkillId);
                }
            }
        }

        private static SkillNode[] AppendReactionNodes(
            SkillNode[] nodes,
            SkillReaction[] reactions,
            string sourceSkillId)
        {
            var result = nodes ?? Array.Empty<SkillNode>();
            for (var i = 0; reactions != null && i < reactions.Length; i++)
            {
                if (reactions[i] != null
                    && string.Equals(
                        reactions[i].SourceSkillId,
                        sourceSkillId,
                        StringComparison.OrdinalIgnoreCase))
                {
                    result = AppendNode(
                        result,
                        SkillNode.FromOperation(
                            new SkillReactionOp(reactions[i]),
                            sourceSkillId));
                }
            }
            return result;
        }

        /// 전달된 런타임 입력값을 사용해 PassiveSkills를 구성한다.
        private PassiveSkillDefinition[] BuildPassiveSkills(
            SourceModel model,
            MonsterDefinition monster)
        {
            var monsterId = monster.MonsterId;
            var skills = FilterAndSort(
                model.Skills.Values,
                skill => skill.SkillKind == PakuriCsvSkillKind.Passive
                    && string.Equals(skill.MonsterId, monsterId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.Slot.CompareTo(right.Slot));

            var definitions = new PassiveSkillDefinition[skills.Count];
            for (var i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];
                var definition = new PassiveSkillBuildData
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
                    Nodes = BuildSkillNodes(model, SkillNodeOwnerKind.Passive, skill.Id, skill.Id)
                };
                definitions[i] = BuildPassiveDefinition(monster, definition);
            }

            return definitions;
        }

        /// 전달된 런타임 입력값을 사용해 SkillChoices를 구성한다.
        private SkillChoiceBuildData[] BuildSkillChoices(SourceModel model, string skillId, SkillChoiceGroup choiceGroup)
        {
            var choices = FilterAndSort(
                model.SkillChoices.Values,
                choice => choice.ChoiceGroup == choiceGroup
                    && string.Equals(choice.SkillId, skillId, StringComparison.OrdinalIgnoreCase),
                (left, right) => left.SortOrder.CompareTo(right.SortOrder));

            var definitions = new SkillChoiceBuildData[choices.Count];
            for (var i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var targetSkillId = choice.TargetSkillId;
                if (string.IsNullOrWhiteSpace(targetSkillId))
                {
                    targetSkillId = choice.SkillId;
                }
                var normalizedNodes = BuildSkillNodes(
                    model,
                    SkillNodeOwnerKind.Choice,
                    choice.Id,
                    targetSkillId);

                definitions[i] = new SkillChoiceBuildData
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
                    Nodes = normalizedNodes
                };
            }

            return definitions;
        }

        /// 전달된 런타임 입력값을 사용해 ChoiceNodeParam를 반환한다.
        private string GetChoiceNodeParam(
            SkillNodeBuildData[] nodes,
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

        /// 전달된 런타임 입력값을 사용해 SkillNodes를 구성한다.
        private SkillNodeBuildData[] BuildSkillNodes(
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
                return Array.Empty<SkillNodeBuildData>();
            }

            var definitions = new SkillNodeBuildData[nodes.Count];
            for (var i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var targetSkillId = node.TargetSkillId;
                if (string.IsNullOrWhiteSpace(targetSkillId))
                {
                    targetSkillId = defaultTargetSkillId;
                }

                var definition = new SkillNodeBuildData
                {
                    OwnerKind = node.OwnerKind.ToString(),
                    TargetSkillId = targetSkillId,
                    HandlerId = node.HandlerId,
                    EnabledByDefault = node.EnabledByDefault,
                    Params = BuildSkillNodeParams(model, node.Id)
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

        /// 전달된 런타임 입력값을 사용해 SkillNodeParams를 구성한다.
        private SkillNodeParamBuildData[] BuildSkillNodeParams(SourceModel model, string nodeId)
        {
            var nodeParams = FilterAndSort(
                model.SkillNodeParams,
                param => string.Equals(param.NodeId, nodeId, StringComparison.OrdinalIgnoreCase),
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

        /// 전달된 런타임 입력값을 사용해 FilterAndSort 결과값을 생성해 반환한다.
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

        /// 전달된 런타임 입력값을 사용해 StatusPayload를 적용한다.
        private void ApplyStatusPayload(ActiveSkillBuildData definition, StatusPayloadRow payload)
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

        /// 전달된 entries 값을 사용해 CatalogEntries를 정렬한다.
        private IEnumerable<CatalogEntryRow> SortCatalogEntries(Dictionary<string, CatalogEntryRow> entries)
        {
            var list = new List<CatalogEntryRow>(entries.Values);
            list.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            return list;
        }

        /// 전달된 assetPath 값을 사용해 Sprite를 불러온다.
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

        /// 전달된 row 값을 사용해 RuntimeVisual를 구성한다.
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

        /// 전달된 row 값을 사용해 ImpactRuntimeVisual를 구성한다.
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

        /// 전달된 런타임 입력값을 사용해 RuntimeVisual를 구성한다.
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

        /// 전달된 assetPath 값을 사용해 AnimatorController를 불러온다.
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

        /// 전달된 assetPath 값을 사용해 Prefab를 불러온다.
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
