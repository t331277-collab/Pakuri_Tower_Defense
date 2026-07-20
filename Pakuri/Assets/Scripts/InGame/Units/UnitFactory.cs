using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 정의 데이터와 런 상태를 이용해 아군·적 런타임 모델을 만드는 팩토리.
 */
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
                NexusDamage = definition.NexusDamage > 0f ? definition.NexusDamage : 1f,
                Stats = MapStats(stats, maxHealth, 0f),
                Defenses = UnitDefenseRuntime.FromDefinition(definition.Defenses),
                Resources = new UnitResourceRuntime
                {
                    CurrentHealth = Math.Max(0f, maxHealth),
                    CurrentShield = 0f
                },
                State = new UnitStateBucket(),
                IsBoss = isBoss,
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            EnemyPassiveRuntime.Apply(model, definition.PassiveSkill);
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

        private static UnitStatsRuntime MapStats(UnitStatsRuntime source, float maxHealth, float fallbackPower)
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

        private static string BuildUnitId(string prefix, string definitionId, int slotIndex)
        {
            var id = string.IsNullOrWhiteSpace(definitionId) ? "unknown" : definitionId;
            return $"{prefix}-{id}-{slotIndex}";
        }
    }
}
