using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Data
{
    public class GameDataCatalog : ScriptableObject
    {
        private readonly Dictionary<string, MonsterDefinition> monsters = new Dictionary<string, MonsterDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EnemyDefinition> enemies = new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillSourceDefinition> activeSkills = new Dictionary<string, SkillSourceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveDefinition> passiveSkills = new Dictionary<string, PassiveDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, StatusEffectDefinition> statusEffects = new Dictionary<string, StatusEffectDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillChoiceDefinition> skillChoices = new Dictionary<string, SkillChoiceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition> rewardChoices = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillSourceDefinition[]> activeSkillsByMonster = new Dictionary<string, SkillSourceDefinition[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveDefinition[]> passiveSkillsByMonster = new Dictionary<string, PassiveDefinition[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition[]> rewardChoicesByMonster = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition[]>(StringComparer.OrdinalIgnoreCase);

        public MonsterDefinition[] Monsters = Array.Empty<MonsterDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] StageTwoEnemies = Array.Empty<EnemyDefinition>();
        public StatusEffectDefinition[] StatusEffects = Array.Empty<StatusEffectDefinition>();

        public void RebuildLookup()
        {
            monsters.Clear();
            enemies.Clear();
            activeSkills.Clear();
            passiveSkills.Clear();
            statusEffects.Clear();
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

        public T GetData<T>(string id)
            where T : class
        {
            return TryGetData(id, out T value) ? value : null;
        }

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
            else if (targetType == typeof(SkillSourceDefinition))
            {
                activeSkills.TryGetValue(id, out var activeSkill);
                resolved = activeSkill;
            }
            else if (targetType == typeof(PassiveDefinition))
            {
                passiveSkills.TryGetValue(id, out var passiveSkill);
                resolved = passiveSkill;
            }
            else if (targetType == typeof(StatusEffectDefinition))
            {
                statusEffects.TryGetValue(id, out var statusEffect);
                resolved = statusEffect;
            }
            else if (targetType == typeof(SkillChoiceDefinition))
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

        public MonsterDefinition[] GetMonsters()
        {
            return Monsters;
        }

        public MonsterDefinition GetMonster(string id)
        {
            return GetData<MonsterDefinition>(id);
        }

        public SkillSourceDefinition[] GetActiveSkills(string monsterId)
        {
            return GetRegistered(activeSkillsByMonster, monsterId);
        }

        public PassiveDefinition[] GetPassiveSkills(string monsterId)
        {
            return GetRegistered(passiveSkillsByMonster, monsterId);
        }

        public MonsterDefinition.RewardChoiceDefinition[] GetRewardChoices(string monsterId)
        {
            return GetRegistered(rewardChoicesByMonster, monsterId);
        }

        public SkillSourceDefinition GetActiveSkill(string monsterId, SkillSlot slot)
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

        public PassiveDefinition ResolvePassiveSkill(string monsterId, SkillSlot slot)
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

        private static T[] GetRegistered<T>(Dictionary<string, T[]> lookup, string id)
        {
            return !string.IsNullOrWhiteSpace(id)
                && lookup.TryGetValue(id, out var values)
                && values != null
                ? values
                : Array.Empty<T>();
        }

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
                activeSkillsByMonster[monster.MonsterId] = monster.ActiveSkills ?? Array.Empty<SkillSourceDefinition>();
                passiveSkillsByMonster[monster.MonsterId] = monster.PassiveSkills ?? Array.Empty<PassiveDefinition>();
                rewardChoicesByMonster[monster.MonsterId] = monster.InitialRewardChoices ?? Array.Empty<MonsterDefinition.RewardChoiceDefinition>();

                RegisterActiveSkills(monster.ActiveSkills);
                RegisterPassiveSkills(monster.PassiveSkills);
                RegisterRewardChoices(monster.InitialRewardChoices);
            }
        }

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
                }
            }
        }

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
                }
            }
        }

        private void RegisterActiveSkills(SkillSourceDefinition[] skills)
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
                RegisterSkillChoices(skill.MasterSkillChoices);
            }
        }

        private void RegisterPassiveSkills(PassiveDefinition[] passives)
        {
            if (passives == null)
            {
                return;
            }

            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (passive == null || string.IsNullOrWhiteSpace(passive.PassiveId))
                {
                    continue;
                }

                passiveSkills[passive.PassiveId] = passive;
                RegisterSkillChoices(passive.EnhancementChoices);
            }
        }

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

        private void RegisterSkillChoices(SkillChoiceDefinition[] choices)
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
