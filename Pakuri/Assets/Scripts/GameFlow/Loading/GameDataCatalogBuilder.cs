using System;
using System.Collections.Generic;
using System.Globalization;
using Pakuri.Combat;
using Pakuri.InGame;
using UnityEngine;
using static Pakuri.Data.GameDataLoader;
using static Pakuri.Data.CsvParser;
using static Pakuri.Data.CsvRowParser;
using static Pakuri.Data.CsvSourceModel;
using static Pakuri.Data.SkillGraphParser;


/*
 * 검증된 SourceModel을 게임이 직접 사용하는 GameDataCatalog로 변환한다.
 * 몬스터·적·상태·스킬·선택지 정의를 만들고 노드, 효과, 트리거, 상태 부가값을 연결하며
 * 각 정의가 참조하는 스프라이트, 프리팹, 애니메이터 자산도 함께 해석한다.
 */
namespace Pakuri.Data
{
    internal static class GameDataCatalogBuilder
    {
        /*
         * CSV 원본 모델을 런타임 게임 데이터 카탈로그로 만든다.
         */
        internal static GameDataCatalog BuildRuntimeCatalog(SourceModel model /* CSV에서 읽은 원본 데이터 */)
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

        /*
         * 현재 조건에 맞는 값을 결정한다.
         */
        internal static string ResolveMonsterSkillDisplayName(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            string monsterId /* 몬스터 식별자 */,
            PakuriCsvSkillKind skillKind /* 스킬 종류 */,
            SkillSlot slot /* 스킬이나 유닛이 배치될 슬롯 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static EnemyDefinition[] BuildEnemies(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            string stageId /* 스테이지 식별자 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static EnemyPassiveDefinition BuildEnemyPassiveDefinition(SourceModel model /* CSV에서 읽은 원본 데이터 */, string passiveId /* 패시브 식별자 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillSourceDefinition[] BuildEnemyAssignedActiveSkills(SourceModel model /* CSV에서 읽은 원본 데이터 */, string enemyId /* 적 식별자 */)
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

        /*
         * 조건을 만족하는 항목만 추가한다.
         */
        internal static void TryAddEnemyAssignedSkillDefinition(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            string skillId /* 스킬 식별자 */,
            SkillSlot runtimeSlot /* 런타임 슬롯 */,
            List<SkillSourceDefinition> definitions /* 정의 목록 */)
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
        internal static SkillSourceDefinition BuildEnemyAssignedSkillDefinition(EnemyBaseSkillRow source /* 효과를 발생시킨 원본 */, SkillSlot runtimeSlot /* 런타임 슬롯 */)
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

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
        internal static void ApplyEnemyExecutionProfile(SkillSourceDefinition definition /* 변환하거나 검사할 정의 */)
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

        /*
         * 원본 값을 런타임 형식으로 바꾼다.
         */
        internal static string MapEnemyTargetSelection(string selection /* 선택 방식 */)
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
        internal static SkillTriggerDefinition[] BuildEnemyAssignedSkillTriggers(SourceModel model /* CSV에서 읽은 원본 데이터 */, string enemyId /* 적 식별자 */)
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
        internal static StatusEffectDefinition[] BuildStatusEffects(SourceModel model /* CSV에서 읽은 원본 데이터 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static MonsterDefinition.RewardChoiceDefinition[] BuildRewardChoices(SourceModel model /* CSV에서 읽은 원본 데이터 */, string monsterId /* 몬스터 식별자 */)
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
        internal static SkillSourceDefinition[] BuildActiveSkills(SourceModel model /* CSV에서 읽은 원본 데이터 */, string monsterId /* 몬스터 식별자 */)
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
        internal static SkillEffectDefinition[] BuildSkillEffects(SourceModel model /* CSV에서 읽은 원본 데이터 */, string skillId /* 스킬 식별자 */)
        {
            return BuildEffectOwnedSkillEffects(model, skillId);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillEffectDefinition[] BuildEffectOwnedSkillEffects(SourceModel model /* CSV에서 읽은 원본 데이터 */, string skillId /* 스킬 식별자 */)
        {
            var effectNodes = FilterAndSort(
                model.SkillNodes.Values,
                node => node.OwnerKind == SkillNodeOwnerKind.Effect
                    && string.Equals(node.TargetSkillId, skillId, StringComparison.OrdinalIgnoreCase),
                (left, right) =>
                {
                    var ownerCompare = string.Compare(left.OwnerId, right.OwnerId, StringComparison.OrdinalIgnoreCase);
                    if (ownerCompare != 0)
                    {
                        return ownerCompare;
                    }

                    return left.SortOrder.CompareTo(right.SortOrder);
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
                if (sortCompare != 0)
                {
                    return sortCompare;
                }

                return string.Compare(left.EffectId, right.EffectId, StringComparison.OrdinalIgnoreCase);
            });
            return definitions.ToArray();
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillEffectDefinition BuildEffectOwnedSkillEffectDefinition(SkillNodeRow node /* 노드 */)
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
                DamageMultiplier = 1f,
                StatusChance = 1f,
                StatusMaxStacks = 1,
                StatusStackAmount = 1
            };
        }

        /*
         * 계산된 값을 대상 정의에 적용한다.
         */
        internal static void ApplyEffectOwnedSkillEffectOperationNode(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            SkillEffectDefinition definition /* 변환하거나 검사할 정의 */,
            SkillNodeRow node /* 노드 */)
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
                definition.RecastInheritSkillData = GetSkillNodeBoolParam(parameters, "inherit_snapshot", true);
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
        internal static void ApplyEffectOwnedStatusParams(
            SkillEffectDefinition definition /* 변환하거나 검사할 정의 */,
            Dictionary<string, string> parameters /* 매개변수 목록 */,
            bool keepExistingStatusId = false /* 기존 상태 효과 식별자 유지 여부 */)
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
        internal static void ApplyEffectOwnedSkillEffectNode(SourceModel model /* CSV에서 읽은 원본 데이터 */, SkillEffectDefinition definition /* 변환하거나 검사할 정의 */, SkillNodeRow node /* 노드 */)
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
                definition.ConditionStatuses = StatusRuntimeCompiler.ParseConditionStatusExpression(
                    definition.ConditionStatusId);
                definition.ConditionTargetSide = GetSkillNodeEnumParam(parameters, "target_side", definition.TargetSide);
                definition.ConditionStatusSourceSkillId = GetSkillNodeStringParam(parameters, "source_skill_id");
                definition.ConditionStatusSourceSkillIds = StatusRuntimeCompiler.ParseIdList(
                    definition.ConditionStatusSourceSkillId);
                return;
            }

            if (string.Equals(handlerId, "ConditionAnyStatus", StringComparison.OrdinalIgnoreCase))
            {
                definition.ConditionStatusId = GetSkillNodeStringParam(parameters, "status_ids");
                definition.ConditionStatuses = StatusRuntimeCompiler.ParseConditionStatusExpression(
                    definition.ConditionStatusId);
                definition.ConditionTargetSide = GetSkillNodeEnumParam(parameters, "target_side", definition.TargetSide);
                definition.ConditionStatusSourceSkillId = GetSkillNodeStringParam(parameters, "source_skill_id");
                definition.ConditionStatusSourceSkillIds = StatusRuntimeCompiler.ParseIdList(
                    definition.ConditionStatusSourceSkillId);
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
                definition.StatusConditionalTargetStatusKinds = StatusRuntimeCompiler.ParseStatusKinds(
                    GetSkillNodeStringParam(parameters, "status_ids"));
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
        internal static string BuildConditionStatusExpression(Dictionary<string, string> parameters /* 매개변수 목록 */)
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
        internal static Dictionary<string, string> BuildSkillNodeParamValueLookup(SourceModel model /* CSV에서 읽은 원본 데이터 */, string nodeId /* 노드 식별자 */)
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
        internal static string GetSkillNodeStringParam(Dictionary<string, string> parameters /* 매개변수 목록 */, string key /* 조회 키 */)
        {
            if (parameters.TryGetValue(key, out var value))
            {
                return value;
            }

            return string.Empty;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        internal static float GetSkillNodeFloatParam(Dictionary<string, string> parameters /* 매개변수 목록 */, string key /* 조회 키 */, float defaultValue /* 값이 없을 때 사용할 기본값 */)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return float.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        internal static int GetSkillNodeIntParam(Dictionary<string, string> parameters /* 매개변수 목록 */, string key /* 조회 키 */, int defaultValue /* 값이 없을 때 사용할 기본값 */)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return int.Parse(value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        internal static bool GetSkillNodeBoolParam(Dictionary<string, string> parameters /* 매개변수 목록 */, string key /* 조회 키 */, bool defaultValue /* 값이 없을 때 사용할 기본값 */)
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return bool.Parse(value);
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        internal static TEnum GetSkillNodeEnumParam<TEnum>(
            Dictionary<string, string> parameters /* 매개변수 목록 */,
            string key /* 조회 키 */,
            TEnum defaultValue /* 값이 없을 때 사용할 기본값 */)
            where TEnum : struct
        {
            if (!parameters.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            {
                return defaultValue;
            }

            return (TEnum)Enum.Parse(typeof(TEnum), value, true);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillTriggerDefinition[] BuildSkillTriggers(SourceModel model /* CSV에서 읽은 원본 데이터 */, string monsterId /* 몬스터 식별자 */)
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
                    TriggerAction = trigger.TriggerAction,
                    EventSkillId = trigger.EventSkillId,
                    EventSkillRuntimeKinds = trigger.EventSkillRuntimeKinds,
                    EventSkillRuntimeKindValues = StatusRuntimeCompiler.ParseSkillRuntimeKindConditions(
                        trigger.EventSkillRuntimeKinds),
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
                    NormalizedNodes = BuildSkillNodeDefinitions(
                        model,
                        SkillNodeOwnerKind.Trigger,
                        trigger.Id,
                        trigger.SourceSkillId)
                };
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static PassiveDefinition[] BuildPassiveSkills(SourceModel model /* CSV에서 읽은 원본 데이터 */, string monsterId /* 몬스터 식별자 */)
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
                    PassiveEffects = BuildSkillEffects(model, skill.Id),
                    NormalizedPlanNodes = BuildSkillNodeDefinitions(model, SkillNodeOwnerKind.Passive, skill.Id, skill.Id)
                };
            }

            return definitions;
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillChoiceDefinition[] BuildSkillChoices(SourceModel model /* CSV에서 읽은 원본 데이터 */, string skillId /* 스킬 식별자 */, SkillChoiceGroup choiceGroup /* 선택지 그룹 */)
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
                    ChoiceGroup = choice.ChoiceGroup,
                    Title = choice.Title,
                    SkillIcon = LoadSprite(choice.SkillIconPath),
                    SkillEffectPrefab = LoadPrefab(GetChoicePlanNodeParam(
                        normalizedPlanNodes,
                        "EffectVisual",
                        "skill_effect_prefab_path")),
                    DescriptionText = choice.DescriptionText,
                    NormalizedPlanNodes = normalizedPlanNodes
                };
            }

            return definitions;
        }

        /*
         * 계산에 필요한 값을 반환한다.
         */
        internal static string GetChoicePlanNodeParam(
            SkillNodeDefinition[] nodes /* 노드 목록 */,
            string handlerId /* 처리기 식별자 */,
            string paramKey /* 매개변수 조회 키 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillNodeDefinition[] BuildSkillNodeDefinitions(
            SourceModel model /* CSV에서 읽은 원본 데이터 */,
            SkillNodeOwnerKind ownerKind /* 소유자 종류 */,
            string ownerId /* 소유자 식별자 */,
            string defaultTargetSkillId /* 기본 대상 스킬 식별자 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static SkillNodeParamDefinition[] BuildSkillNodeParamDefinitions(SourceModel model /* CSV에서 읽은 원본 데이터 */, string nodeId /* 노드 식별자 */)
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

        /*
         * 조건에 맞는 항목만 정렬해 반환한다.
         */
        internal static List<T> FilterAndSort<T>(
            IEnumerable<T> source /* 효과를 발생시킨 원본 */,
            Predicate<T> predicate /* 판정 조건 */,
            Comparison<T> comparison /* 비교 방식 */)
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
        internal static void ApplyStatusPayload(SkillSourceDefinition definition /* 변환하거나 검사할 정의 */, StatusPayloadRow payload /* 적용 데이터 */)
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
        internal static void ApplyStatusPayload(SkillEffectDefinition definition /* 변환하거나 검사할 정의 */, StatusPayloadRow payload /* 적용 데이터 */)
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
         * 해당 자료 변환에 필요한 값을 구성한다.
         */
        internal static IEnumerable<CatalogEntryRow> SortCatalogEntries(Dictionary<string, CatalogEntryRow> entries /* 등록 정보 목록 */)
        {
            var list = new List<CatalogEntryRow>(entries.Values);
            list.Sort((left, right) => left.SortOrder.CompareTo(right.SortOrder));
            return list;
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static Sprite LoadSprite(string assetPath /* 에셋 경로 */)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (runtimeCsvCatalog != null && runtimeCsvCatalog.TryGetSprite(assetPath, out var sprite))
            {
                return sprite;
            }

            throw new CsvFatalException($"Runtime sprite asset is missing: '{assetPath}'.");
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static RuntimeSkillVisualSpec BuildRuntimeVisual(SkillRow row /* 스킬 CSV 행 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static RuntimeSkillVisualSpec BuildRuntimeVisual(SkillTriggerRow row /* 스킬 트리거 CSV 행 */)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            return BuildRuntimeVisual(
                row.RuntimeVisualSpritePath,
                row.RuntimeVisualAnimatorControllerPath,
                row.RuntimeVisualScale,
                0f,
                0f,
                0f,
                row.RuntimeVisualSortingOrder,
                row.RuntimeHitboxSizeX,
                row.RuntimeHitboxSizeY,
                row.RuntimeVisualAnchor);
        }

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static RuntimeSkillVisualSpec BuildImpactRuntimeVisual(SkillRow row /* 스킬 CSV 행 */)
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

        /*
         * 원본 값으로 런타임 자료를 만든다.
         */
        internal static RuntimeSkillVisualSpec BuildRuntimeVisual(
            string spritePath /* 스프라이트 경로 */,
            string animatorControllerPath /* 애니메이터 제어기 경로 */,
            float scale /* 크기 배율 */,
            float scaleX /* 크기 배율 X축 */,
            float scaleY /* 크기 배율 Y축 */,
            float scaleZ /* 크기 배율 Z축 */,
            int sortingOrder /* 시각 효과의 정렬 순서 */,
            float hitboxSizeX /* 피격 판정 크기 X축 */,
            float hitboxSizeY /* 피격 판정 크기 Y축 */,
            string visualAnchor = null /* 시각 효과를 배치할 기준점 */)
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

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static RuntimeAnimatorController LoadAnimatorController(string assetPath /* 에셋 경로 */)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (runtimeCsvCatalog != null && runtimeCsvCatalog.TryGetAnimatorController(assetPath, out var animatorController))
            {
                return animatorController;
            }

            throw new CsvFatalException($"Runtime animator controller asset is missing: '{assetPath}'.");
        }

        /*
         * 필요한 CSV 또는 자산을 불러온다.
         */
        internal static GameObject LoadPrefab(string assetPath /* 에셋 경로 */)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            if (runtimeCsvCatalog != null && runtimeCsvCatalog.TryGetPrefab(assetPath, out var prefab))
            {
                return prefab;
            }

            throw new CsvFatalException($"Runtime prefab asset is missing: '{assetPath}'.");
        }
    }
}
