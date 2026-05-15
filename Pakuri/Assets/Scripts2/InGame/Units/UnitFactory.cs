using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;
using Pakuri.Run;

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

        public EnemyUnitRuntimeModel CreateEnemy(EnemyDefinition definition, int slotIndex = 0)
        {
            if (definition == null)
            {
                return null;
            }

            var stats = definition.Stats;
            var maxHealth = stats != null ? stats.MaxHealth : 100f;
            return new EnemyUnitRuntimeModel
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
                StageOneSkill = definition.StageOneSkill,
                ActiveSkillCoefficient = definition.ActiveSkillCoefficient,
                ActiveSkillRadius = definition.ActiveSkillRadius,
                ActiveSkillFlatValue = definition.ActiveSkillFlatValue,
                AttackAttemptRange = ResolveEnemyAttackAttemptRange(definition),
                AttackAttemptCooldownSeconds = ResolveEnemyAttackAttemptCooldown(definition),
                Stats = MapStats(stats, maxHealth, 0f),
                Defenses = UnitDefenseRuntime.FromDefinition(definition.Defenses),
                Resources = new UnitResourceRuntime
                {
                    CurrentHealth = Math.Max(0f, maxHealth),
                    CurrentShield = 0f
                },
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };
        }

        public bool TryCreatePhase2ATestModels(
            GameDataCatalog fallbackCatalog,
            out MonsterUnitRuntimeModel eveModel,
            out EnemyUnitRuntimeModel enemyModel,
            string monsterId = DefaultPhase2AMonsterId,
            string enemyId = null)
        {
            eveModel = null;
            enemyModel = null;

            var catalog = PakuriCsvRuntimeData.ResolveCatalogOrFallback(fallbackCatalog);
            var resolvedMonsterId = string.IsNullOrWhiteSpace(monsterId) ? DefaultPhase2AMonsterId : monsterId;
            var eve = PakuriDataManager.Instance.ResolveMonster(resolvedMonsterId, catalog);
            if (eve == null)
            {
                return false;
            }

            var session = RunSession.Begin(eve);
            eveModel = CreateSelectedMonster(eve, session.GetPartyMemberState(eve.MonsterId), 0);
            enemyModel = CreateEnemy(ResolveEnemy(catalog, enemyId), 0);
            return eveModel != null && enemyModel != null;
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

            switch (definition.AttackType)
            {
                case EnemyAttackType.Ranged:
                    return Math.Max(5f, definition.ActiveSkillRadius);
                case EnemyAttackType.MeleeAndRanged:
                    return Math.Max(4f, definition.ActiveSkillRadius);
                case EnemyAttackType.Buffer:
                    return Math.Max(5f, definition.ActiveSkillRadius);
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

            return Math.Max(0.1f, definition.ActiveSkillCooldown);
        }

        private static void ApplyRunState(UnitStateBucket target, RunSession.RunMonsterState runState)
        {
            if (target == null || runState == null)
            {
                return;
            }

            AddRange(target.LearnedActiveSkillIds, runState.LearnedActives);
            AddRange(target.LearnedPassiveSkillIds, runState.LearnedPassives);
            AddRange(target.ChosenChoiceIds, runState.ChosenRewardIds);
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

                var fromCatalog = catalog != null ? catalog.GetStageOneEnemyById(enemyId) : null;
                if (fromCatalog != null)
                {
                    return fromCatalog;
                }
            }

            var enemies = catalog != null ? catalog.StageOneEnemies : null;
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
