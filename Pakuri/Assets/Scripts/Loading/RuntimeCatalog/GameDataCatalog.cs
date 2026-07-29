/*
 * 역할: 런타임 게임 데이터 색인.
 * 책임: 검증된 정의를 소유하고 유닛·스킬·선택지·상태·Trigger의 타입별 조회를 제공한다.
 */

using System;
using System.Collections.Generic;
using Pakuri.InGame;
using UnityEngine;

namespace Pakuri.Data
{

    /// <summary><c>GameDataCatalog</c>가 소유한 런타임 데이터를 색인하고 조회 기능을 제공한다.</summary>
    public class GameDataCatalog : ScriptableObject
    {
        private readonly Dictionary<string, MonsterDefinition> monsters = new Dictionary<string, MonsterDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EnemyDefinition> enemies = new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillDefinition> activeSkills = new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveSkillDefinition> passiveSkills = new Dictionary<string, PassiveSkillDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, StatusEffectDefinition> statusEffects = new Dictionary<string, StatusEffectDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<StatusEffectKind, StatusRuntimeData> statusRuntimeData = new Dictionary<StatusEffectKind, StatusRuntimeData>();
        private readonly Dictionary<string, SkillChoice> skillChoices = new Dictionary<string, SkillChoice>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition> rewardChoices = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillDefinition[]> activeSkillsByMonster = new Dictionary<string, SkillDefinition[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveSkillDefinition[]> passiveSkillsByMonster = new Dictionary<string, PassiveSkillDefinition[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition[]> rewardChoicesByMonster = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition[]>(StringComparer.OrdinalIgnoreCase);

        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] StageTwoEnemies = Array.Empty<EnemyDefinition>();
        public StatusEffectDefinition[] StatusEffects = Array.Empty<StatusEffectDefinition>();

        /// <summary><c>RebuildLookup</c> 작업을 수행한다.</summary>
        public void RebuildLookup()
        {
            monsters.Clear();
            enemies.Clear();
            activeSkills.Clear();
            passiveSkills.Clear();
            statusEffects.Clear();
            statusRuntimeData.Clear();
            skillChoices.Clear();
            rewardChoices.Clear();
            activeSkillsByMonster.Clear();
            passiveSkillsByMonster.Clear();
            rewardChoicesByMonster.Clear();

            RegisterMonsters(Monsters);
            RegisterEnemies(StageOneEnemies);
            RegisterEnemies(StageTwoEnemies);
            RegisterStatusEffects(StatusEffects);
        }

        /// <summary>전달된 <c>id</c> 값을 사용해 <c>Data</c>를 반환한다.</summary>
        public T GetData<T>(string id)
            where T : class
        {
            return TryGetData(id, out T value) ? value : null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Data</c> 조회를 시도하고 값이 있는지 반환한다.</summary>
        public bool TryGetData<T>(string id, out T value)
            where T : class
        {
            value = null;
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            object resolved = null;
            var targetType = typeof(T);
            if (targetType == typeof(MonsterDefinition))
            {
                monsters.TryGetValue(id, out var monster);
                resolved = monster;
            }
            else if (targetType == typeof(EnemyDefinition))
            {
                enemies.TryGetValue(id, out var enemy);
                resolved = enemy;
            }
            else if (targetType == typeof(SkillDefinition))
            {
                activeSkills.TryGetValue(id, out var activeSkill);
                resolved = activeSkill;
            }
            else if (targetType == typeof(PassiveSkillDefinition))
            {
                passiveSkills.TryGetValue(id, out var passiveSkill);
                resolved = passiveSkill;
            }
            else if (targetType == typeof(StatusEffectDefinition))
            {
                statusEffects.TryGetValue(id, out var statusEffect);
                resolved = statusEffect;
            }
            else if (targetType == typeof(SkillChoice))
            {
                skillChoices.TryGetValue(id, out var choice);
                resolved = choice;
            }
            else if (targetType == typeof(MonsterDefinition.RewardChoiceDefinition))
            {
                rewardChoices.TryGetValue(id, out var rewardChoice);
                resolved = rewardChoice;
            }

            value = resolved as T;
            return value != null;
        }

        /// <summary><c>Monsters</c>를 반환한다.</summary>
        public MonsterDefinition[] GetMonsters()
        {
            return Monsters;
        }

        /// <summary>전달된 <c>kind</c> 값을 사용해 <c>StatusRuntimeData</c>를 반환한다.</summary>
        public StatusRuntimeData GetStatusRuntimeData(StatusEffectKind kind)
        {
            return statusRuntimeData.TryGetValue(kind, out var status) ? status : null;
        }

        /// <summary>전달된 <c>id</c> 값을 사용해 <c>Monster</c>를 반환한다.</summary>
        public MonsterDefinition GetMonster(string id)
        {
            return GetData<MonsterDefinition>(id);
        }

        /// <summary>전달된 <c>monsterId</c> 값을 사용해 <c>ActiveSkills</c>를 반환한다.</summary>
        public SkillDefinition[] GetActiveSkills(string monsterId)
        {
            return GetRegistered(activeSkillsByMonster, monsterId);
        }

        /// <summary>전달된 <c>monsterId</c> 값을 사용해 <c>PassiveSkills</c>를 반환한다.</summary>
        public PassiveSkillDefinition[] GetPassiveSkills(string monsterId)
        {
            return GetRegistered(passiveSkillsByMonster, monsterId);
        }

        /// <summary>전달된 <c>monsterId</c> 값을 사용해 <c>RewardChoices</c>를 반환한다.</summary>
        public MonsterDefinition.RewardChoiceDefinition[] GetRewardChoices(string monsterId)
        {
            return GetRegistered(rewardChoicesByMonster, monsterId);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ActiveSkill</c>를 반환한다.</summary>
        public SkillDefinition GetActiveSkill(string monsterId, SkillSlot slot)
        {
            var skills = GetActiveSkills(monsterId);
            for (var i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null && skills[i].Slot == slot)
                {
                    return skills[i];
                }
            }

            return null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>PassiveSkill</c>를 결정한다.</summary>
        public PassiveSkillDefinition ResolvePassiveSkill(string monsterId, SkillSlot slot)
        {
            var passives = GetPassiveSkills(monsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                if (passives[i] != null && passives[i].Slot == slot)
                {
                    return passives[i];
                }
            }

            return null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Registered</c>를 반환한다.</summary>
        private static T[] GetRegistered<T>(Dictionary<string, T[]> lookup, string id)
        {
            return !string.IsNullOrWhiteSpace(id)
                && lookup.TryGetValue(id, out var values)
                && values != null
                ? values
                : Array.Empty<T>();
        }

        /// <summary>전달된 <c>catalogMonsters</c> 값을 사용해 <c>Monsters</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterMonsters(MonsterDefinition[] catalogMonsters)
        {
            if (catalogMonsters == null)
            {
                return;
            }

            for (var i = 0; i < catalogMonsters.Length; i++)
            {
                var monster = catalogMonsters[i];
                if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterId))
                {
                    continue;
                }

                monsters[monster.MonsterId] = monster;
                activeSkillsByMonster[monster.MonsterId] = monster.ActiveSkills ?? Array.Empty<SkillDefinition>();
                passiveSkillsByMonster[monster.MonsterId] = monster.PassiveSkills ?? Array.Empty<PassiveSkillDefinition>();
                rewardChoicesByMonster[monster.MonsterId] = monster.InitialRewardChoices ?? Array.Empty<MonsterDefinition.RewardChoiceDefinition>();

                RegisterActiveSkills(monster.ActiveSkills);
                RegisterPassiveSkills(monster.PassiveSkills);
                RegisterRewardChoices(monster.InitialRewardChoices);
            }
        }

        /// <summary>전달된 <c>catalogEnemies</c> 값을 사용해 <c>Enemies</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterEnemies(EnemyDefinition[] catalogEnemies)
        {
            if (catalogEnemies == null)
            {
                return;
            }

            for (var i = 0; i < catalogEnemies.Length; i++)
            {
                var enemy = catalogEnemies[i];
                if (enemy != null && !string.IsNullOrWhiteSpace(enemy.EnemyId))
                {
                    enemies[enemy.EnemyId] = enemy;
                    if (enemy.PassiveSkill != null)
                    {
                        RegisterPassiveSkills(new[] { enemy.PassiveSkill });
                    }
                }
            }
        }

        /// <summary>전달된 <c>catalogStatusEffects</c> 값을 사용해 <c>StatusEffects</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterStatusEffects(StatusEffectDefinition[] catalogStatusEffects)
        {
            if (catalogStatusEffects == null)
            {
                return;
            }

            for (var i = 0; i < catalogStatusEffects.Length; i++)
            {
                var status = catalogStatusEffects[i];
                if (status != null && !string.IsNullOrWhiteSpace(status.StatusEffectId))
                {
                    statusEffects[status.StatusEffectId] = status;
                    if (status.RuntimeData != null)
                    {
                        statusRuntimeData[status.Kind] = status.RuntimeData;
                    }
                }
            }
        }

        /// <summary>전달된 <c>skills</c> 값을 사용해 <c>ActiveSkills</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterActiveSkills(SkillDefinition[] skills)
        {
            if (skills == null)
            {
                return;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.SkillId))
                {
                    continue;
                }

                activeSkills[skill.SkillId] = skill;
                RegisterSkillChoices(skill.EnhancementChoices);
                RegisterSkillChoices(skill.MasterChoices);
            }
        }

        /// <summary>전달된 <c>passives</c> 값을 사용해 <c>PassiveSkills</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterPassiveSkills(PassiveSkillDefinition[] passives)
        {
            if (passives == null)
            {
                return;
            }

            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (passive == null || string.IsNullOrWhiteSpace(passive.SkillId))
                {
                    continue;
                }

                passiveSkills[passive.SkillId] = passive;
                RegisterSkillChoices(passive.BaseModifierChoices);
                RegisterSkillChoices(passive.EnhancementChoices);
            }
        }

        /// <summary>전달된 <c>rewards</c> 값을 사용해 <c>RewardChoices</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterRewardChoices(MonsterDefinition.RewardChoiceDefinition[] rewards)
        {
            if (rewards == null)
            {
                return;
            }

            for (var i = 0; i < rewards.Length; i++)
            {
                var reward = rewards[i];
                if (reward != null && !string.IsNullOrWhiteSpace(reward.RewardId))
                {
                    rewardChoices[reward.RewardId] = reward;
                }
            }
        }

        /// <summary>전달된 <c>choices</c> 값을 사용해 <c>SkillChoices</c>를 소유 런타임 Registry에 등록한다.</summary>
        private void RegisterSkillChoices(SkillChoice[] choices)
        {
            if (choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && !string.IsNullOrWhiteSpace(choice.ChoiceId))
                {
                    skillChoices[choice.ChoiceId] = choice;
                }
            }
        }
    }
}
