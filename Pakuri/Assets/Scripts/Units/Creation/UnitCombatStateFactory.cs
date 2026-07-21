using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 정의 데이터와 런 상태를 이용해 아군·적 런타임 모델을 만드는 팩토리.
 */
namespace Pakuri.InGame
{
    public sealed class UnitCombatStateFactory
    {
        public MonsterCombatState CreateSelectedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState = null,
            int slotIndex = 0)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "player", runState);
        }

        public MonsterCombatState CreateManifestedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState,
            int slotIndex)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "party", runState);
        }

        public EnemyCombatState CreateEnemy(EnemyDefinition definition, int slotIndex = 0, bool isBoss = false)
        {
            var stats = definition.Stats;
            var maxHealth = stats.MaxHealth;
            var model = new EnemyCombatState
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
                NexusDamage = definition.NexusDamage,
                Stats = MapStats(stats, maxHealth),
                Defenses = MapDefenses(definition.Defenses),
                Resources = new UnitCombatResources
                {
                    CurrentHealth = Math.Max(0f, maxHealth),
                    CurrentShield = 0f
                },
                SkillProgress = new UnitSkillProgress(),
                IsBoss = isBoss,
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            EnemyPassiveModifiers.Apply(model, definition.PassiveSkill);
            return model;
        }

        /*
         * 전투에서 사용하는 Nexus 기본 상태를 만든다.
         */
        public NexusCombatState CreateNexus(float maxHealth)
        {
            var resolvedMaxHealth = Math.Max(1f, maxHealth);
            return new NexusCombatState
            {
                Identity = new UnitIdentity
                {
                    UnitId = "nexus",
                    DefinitionId = "nexus",
                    DisplayName = "Nexus",
                    Side = UnitSide.Player,
                    Role = UnitRole.Nexus,
                    SlotIndex = 100
                },
                Stats = new UnitCombatStats
                {
                    MaxHealth = resolvedMaxHealth
                },
                Resources = new UnitCombatResources
                {
                    CurrentHealth = resolvedMaxHealth,
                    CurrentShield = 0f
                },
                AutoAttackEnabled = false,
                AutoSkillEnabled = false
            };
        }

        private static MonsterCombatState CreateMonster(
            MonsterDefinition definition,
            UnitSide side,
            UnitRole role,
            int slotIndex,
            string unitIdPrefix,
            RunSession.RunMonsterState runState)
        {
            var maxHealthBonus = runState != null ? runState.MaxHealthBonus : 0f;
            var maxHealth = ResolveMonsterMaxHealth(definition) + maxHealthBonus;
            var model = new MonsterCombatState
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
                Stats = MapStats(definition.BaseStats, maxHealth),
                Defenses = MapDefenses(definition.Defenses),
                Resources = new UnitCombatResources
                {
                    CurrentHealth = Math.Max(0f, maxHealth),
                    CurrentShield = 0f
                },
                SkillProgress = new UnitSkillProgress(),
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            ApplyRunState(model.SkillProgress, runState);
            return model;
        }

        private static UnitCombatStats MapStats(UnitCombatStats source, float maxHealth)
        {
            return new UnitCombatStats
            {
                MaxHealth = Math.Max(0f, maxHealth),
                AttackPower = source.AttackPower,
                SpellPower = source.SpellPower,
                MoveSpeed = source.MoveSpeed,
                CriticalChance = source.CriticalChance,
                CriticalDamage = source.CriticalDamage,
                CriticalResistance = source.CriticalResistance
            };
        }

        private static UnitDefenseStats MapDefenses(DamageCalculator.AttributeDefenseSet source)
        {
            return new UnitDefenseStats
            {
                Physical = source.Physical,
                Fire = source.Fire,
                Lightning = source.Lightning,
                Ice = source.Ice,
                Darkness = source.Darkness,
                Holy = source.Holy
            };
        }

        private static float ResolveMonsterMaxHealth(MonsterDefinition definition)
        {
            if (definition.BaseStats.MaxHealth > 0f)
            {
                return definition.BaseStats.MaxHealth;
            }

            return definition.MaxHealth > 0f ? definition.MaxHealth : 100f;
        }

        private static void ApplyRunState(UnitSkillProgress target, RunSession.RunMonsterState runState)
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
