using System;
using System.Collections.Generic;

namespace Pakuri.Data
{
    /*
     * 생성된 게임 데이터 카탈로그를 등록하고 ID와 몬스터별 데이터 조회를 제공한다.
     */
    internal sealed class GameDataLookup
    {
        private readonly Dictionary<string, MonsterDefinition> monsters = new Dictionary<string, MonsterDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, EnemyDefinition> enemies = new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillDefinition> activeSkills = new Dictionary<string, SkillDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveDefinition> passiveSkills = new Dictionary<string, PassiveDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, StatusEffectDefinition> statusEffects = new Dictionary<string, StatusEffectDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillChoiceDefinition> skillChoices = new Dictionary<string, SkillChoiceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition> rewardChoices = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SkillDefinition[]> activeSkillsByMonster = new Dictionary<string, SkillDefinition[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, PassiveDefinition[]> passiveSkillsByMonster = new Dictionary<string, PassiveDefinition[]>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, MonsterDefinition.RewardChoiceDefinition[]> rewardChoicesByMonster = new Dictionary<string, MonsterDefinition.RewardChoiceDefinition[]>(StringComparer.OrdinalIgnoreCase);

        /*
         * 새 카탈로그를 기준으로 모든 런타임 조회 표를 다시 구성한다.
         */
        public void RegisterCatalog(GameDataCatalog catalog)
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

            if (catalog == null)
            {
                return;
            }

            RegisterMonsters(catalog.Monsters);
            RegisterEnemies(catalog.StageOneEnemies);
            RegisterEnemies(catalog.StageTwoEnemies);
            RegisterStatusEffects(catalog.StatusEffects);
        }

        /*
         * ID와 요청 자료형에 맞는 데이터를 조회하고 없으면 null을 반환한다.
         */
        public T GetData<T>(string id)
            where T : class
        {
            if (TryGetData(id, out T value))
            {
                return value;
            }

            return null;
        }

        /*
         * ID와 요청 자료형에 맞는 데이터를 찾아 반환한다.
         */
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

            if (resolved == null)
            {
                return false;
            }

            value = resolved as T;
            return value != null;
        }

        /*
         * ID에 맞는 몬스터를 반환한다.
         */
        public MonsterDefinition ResolveMonster(string id)
        {
            return GetData<MonsterDefinition>(id);
        }

        /*
         * 몬스터에 등록된 액티브 스킬 목록을 반환한다.
         */
        public SkillDefinition[] GetActiveSkills(string monsterId)
        {
            if (!string.IsNullOrWhiteSpace(monsterId)
                && activeSkillsByMonster.TryGetValue(monsterId, out var registeredSkills)
                && registeredSkills != null
                && registeredSkills.Length > 0)
            {
                return registeredSkills;
            }

            return Array.Empty<SkillDefinition>();
        }

        /*
         * 몬스터에 등록된 패시브 스킬 목록을 반환한다.
         */
        public PassiveDefinition[] GetPassiveSkills(string monsterId)
        {
            if (!string.IsNullOrWhiteSpace(monsterId)
                && passiveSkillsByMonster.TryGetValue(monsterId, out var registeredPassives)
                && registeredPassives != null
                && registeredPassives.Length > 0)
            {
                return registeredPassives;
            }

            return Array.Empty<PassiveDefinition>();
        }

        /*
         * 몬스터에 등록된 초기 보상 선택지 목록을 반환한다.
         */
        public MonsterDefinition.RewardChoiceDefinition[] GetRewardChoices(string monsterId)
        {
            if (!string.IsNullOrWhiteSpace(monsterId)
                && rewardChoicesByMonster.TryGetValue(monsterId, out var registeredRewards)
                && registeredRewards != null
                && registeredRewards.Length > 0)
            {
                return registeredRewards;
            }

            return Array.Empty<MonsterDefinition.RewardChoiceDefinition>();
        }

        /*
         * 몬스터의 액티브 스킬 중 요청 슬롯에 배치된 스킬을 찾는다.
         */
        public SkillDefinition ResolveActiveSkill(string monsterId, SkillSlot slot)
        {
            var skills = GetActiveSkills(monsterId);
            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill != null && skill.Slot == slot)
                {
                    return skill;
                }
            }

            return null;
        }

        /*
         * 몬스터의 패시브 스킬 중 요청 슬롯에 배치된 스킬을 찾는다.
         */
        public PassiveDefinition ResolvePassiveSkill(string monsterId, SkillSlot slot)
        {
            var passives = GetPassiveSkills(monsterId);
            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (passive != null && passive.Slot == slot)
                {
                    return passive;
                }
            }

            return null;
        }

        /*
         * 몬스터와 몬스터가 소유한 스킬 및 보상 선택지를 조회 표에 등록한다.
         */
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
                passiveSkillsByMonster[monster.MonsterId] = monster.PassiveSkills ?? Array.Empty<PassiveDefinition>();
                rewardChoicesByMonster[monster.MonsterId] = monster.InitialRewardChoices ?? Array.Empty<MonsterDefinition.RewardChoiceDefinition>();

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

        /*
         * 적 정의를 전역 ID 조회 표에 등록한다.
         */
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

                enemies[enemy.EnemyId] = enemy;
            }
        }

        /*
         * 상태 효과 정의를 ID 조회 표에 등록한다.
         */
        private void RegisterStatusEffects(StatusEffectDefinition[] catalogStatusEffects)
        {
            if (catalogStatusEffects == null)
            {
                return;
            }

            for (var i = 0; i < catalogStatusEffects.Length; i++)
            {
                var status = catalogStatusEffects[i];
                if (status == null || string.IsNullOrWhiteSpace(status.StatusEffectId))
                {
                    continue;
                }

                statusEffects[status.StatusEffectId] = status;
            }
        }

        /*
         * 액티브 스킬과 그 성장 선택지를 조회 표에 등록한다.
         */
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

        /*
         * 패시브 스킬과 그 성장 선택지를 조회 표에 등록한다.
         */
        private void RegisterPassiveSkill(PassiveDefinition passive)
        {
            if (passive == null || string.IsNullOrWhiteSpace(passive.PassiveId))
            {
                return;
            }

            passiveSkills[passive.PassiveId] = passive;
            RegisterSkillChoices(passive.EnhancementChoices);
        }

        /*
         * 스킬 성장 선택지를 ID 조회 표에 등록한다.
         */
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
