using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using Pakuri.Run;
using CombatStatBlock = Pakuri.Combat.DamageCalculator.CombatStatBlock;

namespace Pakuri.InGame
{
    public sealed class UnitFactory
    {
        public const string DefaultPhase2AMonsterId = "eve";

        public MonsterUnitRuntimeModel CreateSelectedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState = null,
            int slotIndex = 0)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "player", runState);
        }

        public MonsterUnitRuntimeModel CreateManifestedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState,
            int slotIndex)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "party", runState);
        }

        public EnemyUnitRuntimeModel CreateEnemy(EnemyDefinition definition, int slotIndex = 0, bool isBoss = false)
        {
            if (definition == null)
            {
                return null;
            }

            var stats = definition.Stats;
            var maxHealth = stats != null ? stats.MaxHealth : 100f;
            var model = new EnemyUnitRuntimeModel
            {
                Identity = new UnitIdentity
                {
                    UnitId = BuildUnitId("enemy", definition.EnemyId, slotIndex),
                    DefinitionId = definition.EnemyId,
                    DisplayName = definition.DisplayName,
                    Side = UnitSide.Enemy,
                    Role = UnitRole.Enemy,
                    SlotIndex = slotIndex
                },
                EncounterRole = definition.EncounterRole,
                AttackType = definition.AttackType,
                Attribute = definition.Attribute,
                HasBasicSkill = definition.HasBasicSkill,
                BasicSkill = definition.BasicSkill,
                BasicSkillCoefficient = definition.BasicSkillCoefficient,
                BasicSkillAttackPowerCoefficient = definition.BasicSkillAttackPowerCoefficient,
                BasicSkillSpellPowerCoefficient = definition.BasicSkillSpellPowerCoefficient,
                BasicSkillDuration = definition.BasicSkillDuration,
                BasicSkillRadius = definition.BasicSkillRadius,
                BasicSkillFlatValue = definition.BasicSkillFlatValue,
                BasicSkillProjectileSpeed = definition.BasicSkillProjectileSpeed,
                BasicSkillProjectileLifetime = definition.BasicSkillProjectileLifetime,
                BasicSkillMoveSpeedMultiplier = definition.BasicSkillMoveSpeedMultiplier,
                BasicSkillOutgoingDamageMultiplier = definition.BasicSkillOutgoingDamageMultiplier,
                BasicSkillPlan = definition.BasicSkillPlan,
                BasicSkillCooldownSeconds = definition.BasicSkillCooldown,
                StageOneSkill = definition.StageOneSkill,
                ActiveSkillCoefficient = definition.ActiveSkillCoefficient,
                ActiveSkillAttackPowerCoefficient = definition.ActiveSkillAttackPowerCoefficient,
                ActiveSkillSpellPowerCoefficient = definition.ActiveSkillSpellPowerCoefficient,
                ActiveSkillDuration = definition.ActiveSkillDuration,
                ActiveSkillRadius = definition.ActiveSkillRadius,
                ActiveSkillFlatValue = definition.ActiveSkillFlatValue,
                ActiveSkillProjectileSpeed = definition.ActiveSkillProjectileSpeed,
                ActiveSkillProjectileLifetime = definition.ActiveSkillProjectileLifetime,
                ActiveSkillMoveSpeedMultiplier = definition.ActiveSkillMoveSpeedMultiplier,
                ActiveSkillOutgoingDamageMultiplier = definition.ActiveSkillOutgoingDamageMultiplier,
                ActiveSkillPlan = definition.ActiveSkillPlan,
                ActiveSkillCooldownSeconds = definition.ActiveSkillCooldown,
                AttackAttemptRange = ResolveEnemyAttackAttemptRange(definition),
                AttackAttemptCooldownSeconds = ResolveEnemyAttackAttemptCooldown(definition),
                PassiveSkillId = definition.PassiveSkillId,
                PassiveSkillValue = definition.PassiveSkillValue,
                NexusDamage = definition.NexusDamage > 0f ? definition.NexusDamage : 1f,
                Stats = MapStats(stats, maxHealth, 0f),
                Defenses = UnitDefenseRuntime.FromDefinition(definition.Defenses),
                Resources = new UnitResourceRuntime
                {
                    CurrentHealth = Math.Max(0f, maxHealth),
                    CurrentShield = 0f
                },
                IsBoss = isBoss,
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            ApplyStageOneEnemyPassive(model);
            return model;
        }

        private static MonsterUnitRuntimeModel CreateMonster(
            MonsterDefinition definition,
            UnitSide side,
            UnitRole role,
            int slotIndex,
            string unitIdPrefix,
            RunSession.RunMonsterState runState)
        {
            if (definition == null)
            {
                return null;
            }

            var maxHealthBonus = runState != null ? runState.MaxHealthBonus : 0f;
            var maxHealth = ResolveMonsterMaxHealth(definition) + maxHealthBonus;
            var model = new MonsterUnitRuntimeModel
            {
                Identity = new UnitIdentity
                {
                    UnitId = BuildUnitId(unitIdPrefix, definition.MonsterId, slotIndex),
                    DefinitionId = definition.MonsterId,
                    DisplayName = definition.DisplayName,
                    Side = side,
                    Role = role,
                    SlotIndex = slotIndex
                },
                Stats = MapStats(definition.BaseStats, maxHealth, definition.PowerStat),
                Defenses = UnitDefenseRuntime.FromDefinition(definition.Defenses),
                Resources = new UnitResourceRuntime
                {
                    CurrentHealth = Math.Max(0f, maxHealth),
                    CurrentShield = 0f
                },
                State = new UnitStateBucket(),
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            ApplyRunState(model.State, runState);
            return model;
        }

        private static UnitStatsRuntime MapStats(CombatStatBlock source, float maxHealth, float fallbackPower)
        {
            return new UnitStatsRuntime
            {
                MaxHealth = Math.Max(0f, maxHealth),
                AttackPower = source != null ? source.AttackPower : fallbackPower,
                SpellPower = source != null ? source.SpellPower : fallbackPower,
                MoveSpeed = source != null ? source.MoveSpeed : 1f,
                CriticalChance = source != null ? source.CriticalChance : DamageCalculator.BaseCriticalChance,
                CriticalDamage = source != null ? source.CriticalDamage : DamageCalculator.BaseCriticalMultiplier,
                CriticalResistance = source != null ? source.CriticalResistance : 0f
            };
        }

        private static float ResolveMonsterMaxHealth(MonsterDefinition definition)
        {
            if (definition.BaseStats != null && definition.BaseStats.MaxHealth > 0f)
            {
                return definition.BaseStats.MaxHealth;
            }

            return definition.MaxHealth > 0f ? definition.MaxHealth : 100f;
        }

        private static float ResolveEnemyAttackAttemptRange(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return 1.4f;
            }

            if (definition.HasBasicSkill && definition.BasicSkill != definition.StageOneSkill)
            {
                return ResolveEnemySkillAttemptRange(definition.AttackType, definition.BasicSkillRadius);
            }

            return ResolveEnemySkillAttemptRange(definition.AttackType, definition.ActiveSkillRadius);
        }

        private static float ResolveEnemySkillAttemptRange(EnemyAttackType attackType, float authoredRange)
        {
            if (authoredRange > 0f)
            {
                return Math.Max(0.1f, authoredRange);
            }

            switch (attackType)
            {
                case EnemyAttackType.Ranged:
                    return 5f;
                case EnemyAttackType.MeleeAndRanged:
                    return 4f;
                case EnemyAttackType.Buffer:
                    return 5f;
                default:
                    return 1.4f;
            }
        }

        private static float ResolveEnemyAttackAttemptCooldown(EnemyDefinition definition)
        {
            if (definition == null)
            {
                return 1f;
            }

            if (definition.HasBasicSkill && definition.BasicSkill != definition.StageOneSkill)
            {
                return Math.Max(0.1f, definition.BasicSkillCooldown);
            }

            return Math.Max(0.1f, definition.ActiveSkillCooldown);
        }

        private static void ApplyStageOneEnemyPassive(EnemyUnitRuntimeModel enemy)
        {
            if (enemy == null)
            {
                return;
            }

            var value = Math.Max(0f, enemy.PassiveSkillValue);
            if (string.IsNullOrWhiteSpace(enemy.PassiveSkillId) || value <= 0f)
            {
                return;
            }

            switch (enemy.PassiveSkillId.Trim().ToLowerInvariant())
            {
                case "physicaldamageup":
                    enemy.PassivePhysicalDamageMultiplier *= 1f + value;
                    break;
                case "firedamageup":
                    enemy.PassiveFireDamageMultiplier *= 1f + value;
                    break;
                case "lightningdamageup":
                    enemy.PassiveLightningDamageMultiplier *= 1f + value;
                    break;
                case "icedamageup":
                    enemy.PassiveIceDamageMultiplier *= 1f + value;
                    break;
                case "darknessdamageup":
                    enemy.PassiveDarknessDamageMultiplier *= 1f + value;
                    break;
                case "holydamageup":
                    enemy.PassiveHolyDamageMultiplier *= 1f + value;
                    break;
                case "defenseup":
                    MultiplyDefenses(enemy.Defenses, 1f + value);
                    break;
                case "physicaldefenseup":
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Physical, 1f + value);
                    break;
                case "firedefenseup":
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Fire, 1f + value);
                    break;
                case "lightningdefenseup":
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Lightning, 1f + value);
                    break;
                case "icedefenseup":
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Ice, 1f + value);
                    break;
                case "darknessdefenseup":
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Darkness, 1f + value);
                    break;
                case "holydefenseup":
                    MultiplyDefense(enemy.Defenses, DamageAttribute.Holy, 1f + value);
                    break;
                case "critchanceup":
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalChance += value;
                    }

                    break;
                case "critdamageup":
                    if (enemy.Stats != null)
                    {
                        enemy.Stats.CriticalDamage += value;
                    }

                    break;
                case "healingup":
                    enemy.PassiveHealingMultiplier *= 1f + value;
                    break;
                case "incomingdamagedown":
                    enemy.PassiveIncomingDamageMultiplier *= Math.Max(0f, 1f - value);
                    break;
            }
        }

        private static void MultiplyDefenses(UnitDefenseRuntime defenses, float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            defenses.Physical *= multiplier;
            defenses.Fire *= multiplier;
            defenses.Lightning *= multiplier;
            defenses.Ice *= multiplier;
            defenses.Darkness *= multiplier;
            defenses.Holy *= multiplier;
        }

        private static void MultiplyDefense(UnitDefenseRuntime defenses, DamageAttribute attribute, float multiplier)
        {
            if (defenses == null)
            {
                return;
            }

            switch (attribute)
            {
                case DamageAttribute.Fire:
                    defenses.Fire *= multiplier;
                    break;
                case DamageAttribute.Lightning:
                    defenses.Lightning *= multiplier;
                    break;
                case DamageAttribute.Ice:
                    defenses.Ice *= multiplier;
                    break;
                case DamageAttribute.Darkness:
                    defenses.Darkness *= multiplier;
                    break;
                case DamageAttribute.Holy:
                    defenses.Holy *= multiplier;
                    break;
                default:
                    defenses.Physical *= multiplier;
                    break;
            }
        }

        private static void ApplyRunState(UnitStateBucket target, RunSession.RunMonsterState runState)
        {
            if (target == null || runState == null)
            {
                return;
            }

            AddRange(target.LearnedActiveSkillIds, runState.LearnedActives);
            AddRange(target.LearnedPassiveSkillIds, runState.LearnedPassives);
            AddRange(target.ChosenChoiceIds, runState.ChosenChoiceIds);
        }

        private static void AddRange(HashSet<string> target, IReadOnlyList<string> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(source[i]))
                {
                    target.Add(source[i]);
                }
            }
        }

        private static EnemyDefinition ResolveEnemy(GameDataCatalog catalog, string enemyId)
        {
            if (!string.IsNullOrWhiteSpace(enemyId))
            {
                var registered = PakuriDataManager.Instance.GetData<EnemyDefinition>(enemyId);
                if (registered != null)
                {
                    return registered;
                }

                var fromCatalog = catalog != null ? catalog.GetEnemyById(enemyId) : null;
                if (fromCatalog != null)
                {
                    return fromCatalog;
                }
            }

            var enemies = catalog != null ? catalog.StageOneEnemies : null;
            if (enemies == null || enemies.Length == 0)
            {
                enemies = catalog != null ? catalog.StageTwoEnemies : null;
            }

            if (enemies == null || enemies.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] != null)
                {
                    return enemies[i];
                }
            }

            return null;
        }

        private static string BuildUnitId(string prefix, string definitionId, int slotIndex)
        {
            var id = string.IsNullOrWhiteSpace(definitionId) ? "unknown" : definitionId;
            return $"{prefix}-{id}-{slotIndex}";
        }
    }
}
