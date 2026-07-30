/*
 * 역할: 전투 상태 생성.
 * 책임: 카탈로그 정의와 학습 스킬 상태에서 플레이어·적·Nexus 전투 모델을 생성한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

namespace Pakuri.InGame
{

    /// UnitCombatStateFactory 런타임 객체를 검증된 원본 데이터에서 생성한다.
    public class UnitCombatStateFactory
    {

        /// 전달된 런타임 입력값을 사용해 SelectedMonster를 생성한다.
        public UnitCombatState CreateSelectedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState = null,
            int slotIndex = 0)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "player", runState);
        }

        /// 전달된 런타임 입력값을 사용해 ManifestedMonster를 생성한다.
        public UnitCombatState CreateManifestedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState,
            int slotIndex)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "party", runState);
        }

        /// 전달된 런타임 입력값을 사용해 Enemy를 생성한다.
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
                Attribute = definition.Attribute,
                NexusDamage = definition.NexusDamage,
                Stats = MapStats(stats, maxHealth),
                Defenses = CreateRuntimeDefenses(definition.Defenses),
                Resources = new UnitCombatResources
                {
                    CurrentHealth = maxHealth,
                    CurrentShield = 0f
                },
                IsBoss = isBoss,
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            if (definition.ActiveSkills != null)
            {
                for (var i = 0; i < definition.ActiveSkills.Length; i++)
                {
                    var skill = definition.ActiveSkills[i];
                    if (skill != null)
                    {
                        model.Skills.AddActiveSkill(skill.SkillId);
                    }
                }
            }

            if (definition.PassiveSkill != null)
            {
                model.Skills.AddPassiveSkill(definition.PassiveSkill.SkillId);
            }

            return model;
        }

        /// 전달된 maxHealth 값을 사용해 Nexus를 생성한다.
        public UnitCombatState CreateNexus(float maxHealth)
        {
            return new UnitCombatState
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
                    MaxHealth = maxHealth
                },
                Resources = new UnitCombatResources
                {
                    CurrentHealth = maxHealth,
                    CurrentShield = 0f
                },
                AutoAttackEnabled = false,
                AutoSkillEnabled = false
            };
        }

        /// 전달된 런타임 입력값을 사용해 Monster를 생성한다.
        private static UnitCombatState CreateMonster(
            MonsterDefinition definition,
            UnitSide side,
            UnitRole role,
            int slotIndex,
            string unitIdPrefix,
            RunSession.RunMonsterState runState)
        {
            var maxHealth = definition.BaseStats.MaxHealth;
            var model = new UnitCombatState
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
                Defenses = CreateRuntimeDefenses(definition.Defenses),
                Resources = new UnitCombatResources
                {
                    CurrentHealth = maxHealth,
                    CurrentShield = 0f
                },
                AutoAttackEnabled = true,
                AutoSkillEnabled = true
            };

            if (runState != null)
            {
                model.Skills = runState.Skills;
            }
            return model;
        }

        /// 전달된 런타임 입력값을 사용해 Stats를 대응시킨다.
        private static UnitCombatStats MapStats(UnitCombatStats source, float maxHealth)
        {
            return new UnitCombatStats
            {
                MaxHealth = maxHealth,
                AttackPower = source.AttackPower,
                SpellPower = source.SpellPower,
                MoveSpeed = source.MoveSpeed,
                CriticalChance = source.CriticalChance,
                CriticalDamage = source.CriticalDamage,
                CriticalResistance = source.CriticalResistance
            };
        }

        /// 전달된 source 값을 사용해 RuntimeDefenses를 생성한다.
        private static UnitDefenseStats CreateRuntimeDefenses(UnitDefenseStats source)
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

        /// 전달된 런타임 입력값을 사용해 UnitId를 구성한다.
        private static string BuildUnitId(string prefix, string definitionId, int slotIndex)
        {
            return $"{prefix}-{definitionId}-{slotIndex}";
        }
    }
}
