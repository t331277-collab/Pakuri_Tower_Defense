/*
 * 역할: 카탈로그 정의와 학습 스킬 상태에서 플레이어·적·Nexus 전투 모델을 생성한다.
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

        public UnitCombatState CreateSelectedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState = null,
            int slotIndex = 0)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "player", runState);
        }

        public UnitCombatState CreateManifestedMonster(
            MonsterDefinition definition,
            RunSession.RunMonsterState runState,
            int slotIndex)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "party", runState);
        }

        public UnitCombatState CreateSummon(
            SummonDefinition definition,
            IReadOnlyList<SkillDefinition> learnedSkills,
            DamageAttribute skillAttribute)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var maxHealth = definition.BaseStats.MaxHealth;
            var model = new UnitCombatState
            {
                Identity = new UnitIdentity
                {
                    UnitName = BuildUnitName("summon", definition.SummonName, 0),
                    DefinitionName = definition.SummonName,
                    DisplayName = definition.DisplayName,
                    Side = UnitSide.Player,
                    Role = UnitRole.Summon,
                    SlotIndex = -1
                },
                Stats = MapStats(definition.BaseStats, maxHealth),
                Defenses = CreateRuntimeDefenses(definition.Defenses),
                Resources = new UnitCombatResources
                {
                    CurrentHealth = maxHealth,
                    CurrentShield = 0f
                },
                SkillDamageAttributeOverride = skillAttribute,
                AutoAttackEnabled = false,
                AutoSkillEnabled = true
            };

            for (var i = 0; learnedSkills != null && i < learnedSkills.Count; i++)
            {
                if (learnedSkills[i] != null)
                {
                    model.Skills.AddActiveSkill(learnedSkills[i].SkillName);
                }
            }

            model.SkillState.RebuildLearnedSkillState(
                model,
                definition.ActiveSkills,
                Array.Empty<PassiveSkillDefinition>());
            return model;
        }

        public EnemyCombatState CreateEnemy(EnemyDefinition definition, int slotIndex = 0, bool isBoss = false)
        {
            var stats = definition.Stats;
            var maxHealth = stats.MaxHealth;
            var model = new EnemyCombatState
            {
                Identity = new UnitIdentity
                {
                    UnitName = BuildUnitName("enemy", definition.EnemyName, slotIndex),
                    DefinitionName = definition.EnemyName,
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
                        model.Skills.AddActiveSkill(skill.SkillName);
                    }
                }
            }

            if (definition.PassiveSkill != null)
            {
                model.Skills.AddPassiveSkill(definition.PassiveSkill.SkillName);
            }

            return model;
        }

        public UnitCombatState CreateNexus(
            float maxHealth,
            SkillDefinition learnedSkill = null)
        {
            var model = new UnitCombatState
            {
                Identity = new UnitIdentity
                {
                    UnitName = "nexus",
                    DefinitionName = "nexus",
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
            if (learnedSkill != null)
            {
                model.Skills.AddActiveSkill(learnedSkill.SkillName);
                model.SkillState.RebuildLearnedSkillState(
                    model,
                    new[] { learnedSkill },
                    Array.Empty<PassiveSkillDefinition>());
            }
            return model;
        }

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
                    UnitName = BuildUnitName(unitIdPrefix, definition.MonsterName, slotIndex),
                    DefinitionName = definition.MonsterName,
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
                model.Artifacts = runState.Artifacts;
            }
            return model;
        }

        private static UnitCombatStats MapStats(UnitCombatStats source, float maxHealth)
        {
            return new UnitCombatStats
            {
                MaxHealth = maxHealth,
                AttackPower = source.AttackPower,
                SpellPower = source.SpellPower,
                MoveSpeed = source.MoveSpeed,
                CriticalChance = source.CriticalChance,
                CriticalDamage = source.CriticalDamage
            };
        }

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

        private static string BuildUnitName(string prefix, string definitionName, int slotIndex)
        {
            return $"{prefix}-{definitionName}-{slotIndex}";
        }
    }
}
