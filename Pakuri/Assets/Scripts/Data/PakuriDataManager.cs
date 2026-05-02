using System;
using System.Collections.Generic;

namespace Pakuri.Data
{
    public sealed class PakuriDataManager
    {
        private readonly Dictionary<string, MonsterDefinition> monsters = new Dictionary<string, MonsterDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EnemyDefinition> stageOneEnemies = new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillDefinition> activeSkills = new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveDefinition> passiveSkills = new Dictionary<string, PassiveDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillChoiceDefinition> skillChoices = new Dictionary<string, SkillChoiceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition> rewardChoices = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition>(StringComparer.OrdinalIgnoreCase);

        private static readonly PakuriDataManager instance = new PakuriDataManager();

        private PakuriDataManager()
        {
        }

        public static PakuriDataManager Instance => instance;

        public GameDataCatalog CurrentCatalog { get; private set; }

        public void RegisterCatalog(GameDataCatalog catalog)
        {
            CurrentCatalog = catalog;
            monsters.Clear();
            stageOneEnemies.Clear();
            activeSkills.Clear();
            passiveSkills.Clear();
            skillChoices.Clear();
            rewardChoices.Clear();

            if (catalog == null)
            {
                return;
            }

            RegisterMonsters(catalog.Monsters);
            RegisterEnemies(catalog.StageOneEnemies);
        }

        public T GetData<T>(string id)
            where T : class
        {
            if (TryGetData(id, out T value))
            {
                return value;
            }

            return null;
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
                stageOneEnemies.TryGetValue(id, out var enemy);
                resolved = enemy;
            }
            else if (targetType == typeof(SkillDefinition))
            {
                activeSkills.TryGetValue(id, out var activeSkill);
                resolved = activeSkill;
            }
            else if (targetType == typeof(PassiveDefinition))
            {
                passiveSkills.TryGetValue(id, out var passiveSkill);
                resolved = passiveSkill;
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

            if (resolved == null)
            {
                return false;
            }

            value = resolved as T;
            return value != null;
        }

        public MonsterDefinition[] GetMonsters(GameDataCatalog fallbackCatalog = null)
        {
            var runtimeMonsters = CurrentCatalog != null ? CurrentCatalog.Monsters : null;
            if (runtimeMonsters != null && runtimeMonsters.Length > 0)
            {
                return runtimeMonsters;
            }

            var fallbackMonsters = fallbackCatalog != null ? fallbackCatalog.Monsters : null;
            return fallbackMonsters ?? Array.Empty<MonsterDefinition>();
        }

        public EnemyDefinition[] GetStageOneEnemies(GameDataCatalog fallbackCatalog = null)
        {
            var runtimeEnemies = CurrentCatalog != null ? CurrentCatalog.StageOneEnemies : null;
            if (runtimeEnemies != null && runtimeEnemies.Length > 0)
            {
                return runtimeEnemies;
            }

            var fallbackEnemies = fallbackCatalog != null ? fallbackCatalog.StageOneEnemies : null;
            return fallbackEnemies ?? Array.Empty<EnemyDefinition>();
        }

        public MonsterDefinition ResolveMonster(string id, GameDataCatalog fallbackCatalog = null)
        {
            var resolvedMonster = GetData<MonsterDefinition>(id);
            if (resolvedMonster != null)
            {
                return resolvedMonster;
            }

            var monsters = GetMonsters(fallbackCatalog);
            if (!string.IsNullOrWhiteSpace(id))
            {
                for (var i = 0; i < monsters.Length; i++)
                {
                    var monster = monsters[i];
                    if (monster != null && string.Equals(monster.MonsterId, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return monster;
                    }
                }
            }

            return monsters.Length > 0 ? monsters[0] : null;
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

                if (monster.ActiveSkills != null)
                {
                    for (var skillIndex = 0; skillIndex < monster.ActiveSkills.Length; skillIndex++)
                    {
                        RegisterActiveSkill(monster.ActiveSkills[skillIndex]);
                    }
                }

                if (monster.PassiveSkills != null)
                {
                    for (var passiveIndex = 0; passiveIndex < monster.PassiveSkills.Length; passiveIndex++)
                    {
                        RegisterPassiveSkill(monster.PassiveSkills[passiveIndex]);
                    }
                }

                if (monster.InitialRewardChoices != null)
                {
                    for (var rewardIndex = 0; rewardIndex < monster.InitialRewardChoices.Length; rewardIndex++)
                    {
                        var reward = monster.InitialRewardChoices[rewardIndex];
                        if (reward == null || string.IsNullOrWhiteSpace(reward.RewardId))
                        {
                            continue;
                        }

                        rewardChoices[reward.RewardId] = reward;
                    }
                }
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
                if (enemy == null || string.IsNullOrWhiteSpace(enemy.EnemyId))
                {
                    continue;
                }

                stageOneEnemies[enemy.EnemyId] = enemy;
            }
        }

        private void RegisterActiveSkill(SkillDefinition skill)
        {
            if (skill == null || string.IsNullOrWhiteSpace(skill.SkillId))
            {
                return;
            }

            activeSkills[skill.SkillId] = skill;
            RegisterSkillChoices(skill.EnhancementChoices);
            RegisterSkillChoices(skill.MasterSkillChoices);
        }

        private void RegisterPassiveSkill(PassiveDefinition passive)
        {
            if (passive == null || string.IsNullOrWhiteSpace(passive.PassiveId))
            {
                return;
            }

            passiveSkills[passive.PassiveId] = passive;
            RegisterSkillChoices(passive.EnhancementChoices);
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
                if (choice == null || string.IsNullOrWhiteSpace(choice.ChoiceId))
                {
                    continue;
                }

                skillChoices[choice.ChoiceId] = choice;
            }
        }
    }
}
