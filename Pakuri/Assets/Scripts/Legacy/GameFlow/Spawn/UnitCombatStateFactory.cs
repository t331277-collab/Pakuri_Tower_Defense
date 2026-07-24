using System;
using System.Collections.Generic;
using Pakuri.Combat;
using Pakuri.Data;

/*
 * 정의 데이터와 런 상태를 이용해 아군·적 런타임 모델을 만드는 팩토리.
 */
namespace Pakuri.InGame
{
    public class UnitCombatStateFactory
    {
        /*
         * CreateSelectedMonster에 필요한 결과를 만들어 반환한다.
         */
        public UnitCombatState CreateSelectedMonster(
            MonsterDefinition definition /* 변환하거나 검사할 정의 */,
            RunSession.RunMonsterState runState = null /* 게임 진행 상태 */,
            int slotIndex = 0 /* 배치할 슬롯 순서 번호 */)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "player", runState);
        }

        /*
         * CreateManifestedMonster에 필요한 결과를 만들어 반환한다.
         */
        public UnitCombatState CreateManifestedMonster(
            MonsterDefinition definition /* 변환하거나 검사할 정의 */,
            RunSession.RunMonsterState runState /* 게임 진행 상태 */,
            int slotIndex /* 배치할 슬롯 순서 번호 */)
        {
            return CreateMonster(definition, UnitSide.Player, UnitRole.Monster, slotIndex, "party", runState);
        }

        /*
         * CreateEnemy에 필요한 결과를 만들어 반환한다.
         */
        public EnemyCombatState CreateEnemy(EnemyDefinition definition /* 변환하거나 검사할 정의 */, int slotIndex = 0 /* 배치할 슬롯 순서 번호 */, bool isBoss = false /* 여부 보스 여부 */)
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

            EnemyPassiveModifiers.Apply(model, definition.PassiveSkill);
            return model;
        }

        /*
         * 전투에서 사용하는 Nexus 기본 상태를 만든다.
         */
        public UnitCombatState CreateNexus(float maxHealth /* 최대 체력 */)
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

        /*
         * CreateMonster에 필요한 결과를 만들어 반환한다.
         */
        private static UnitCombatState CreateMonster(
            MonsterDefinition definition /* 변환하거나 검사할 정의 */,
            UnitSide side /* 진영 */,
            UnitRole role /* 역할 */,
            int slotIndex /* 배치할 슬롯 순서 번호 */,
            string unitIdPrefix /* 유닛 식별자 접두어 */,
            RunSession.RunMonsterState runState /* 게임 진행 상태 */)
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

            ApplyRunState(model.Skills, runState);
            return model;
        }

        /*
         * MapStats에 필요한 형식으로 변환해 반환한다.
         */
        private static UnitCombatStats MapStats(UnitCombatStats source /* 복사할 전투 능력치 */, float maxHealth /* 최대 체력 */)
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

        /*
         * 정의 데이터와 분리된 런타임 방어력을 만든다.
         */
        private static UnitDefenseStats CreateRuntimeDefenses(UnitDefenseStats source /* 원본 방어력 */)
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

        /*
         * ApplyRunState 처리를 대상에 적용한다.
         */
        private static void ApplyRunState(UnitSkills target /* 유닛이 보유한 스킬 정보 */, RunSession.RunMonsterState runState /* 게임 진행 상태 */)
        {
            if (runState == null)
            {
                return;
            }

            SkillDefinitionCompiler.ApplyLearnedSkills(
                target,
                runState.LearnedActives,
                runState.LearnedPassives,
                runState.ChosenChoiceIds);
        }

        /*
         * BuildUnitId에 필요한 결과를 만들어 반환한다.
         */
        private static string BuildUnitId(string prefix /* 접두어 */, string definitionId /* 정의 식별자 */, int slotIndex /* 배치할 슬롯 순서 번호 */)
        {
            return $"{prefix}-{definitionId}-{slotIndex}";
        }
    }
}
