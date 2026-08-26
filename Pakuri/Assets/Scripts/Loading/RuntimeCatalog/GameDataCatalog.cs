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

    public class GameDataCatalog : ScriptableObject
    {
        private readonly Dictionary<string, MonsterDefinition> monsters = new Dictionary<string, MonsterDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, SummonDefinition> summons = new Dictionary<string, SummonDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ArtifactDefinition> artifacts = new Dictionary<string, ArtifactDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ArtifactSynergyDefinition> artifactSynergies = new Dictionary<string, ArtifactSynergyDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ArtifactSynergyLevelDefinition> artifactSynergyLevels = new Dictionary<string, ArtifactSynergyLevelDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ArtifactEffectDefinition> artifactEffects = new Dictionary<string, ArtifactEffectDefinition>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, ArtifactSynergyEffectDefinition> artifactSynergyEffects = new Dictionary<string, ArtifactSynergyEffectDefinition>(StringComparer.OrdinalIgnoreCase);
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
        public SummonDefinition[] Summons = Array.Empty<SummonDefinition>();
        public ArtifactDefinition[] Artifacts = Array.Empty<ArtifactDefinition>();
        public ArtifactSynergyDefinition[] ArtifactSynergies = Array.Empty<ArtifactSynergyDefinition>();
        public ArtifactSynergyLevelDefinition[] ArtifactSynergyLevels = Array.Empty<ArtifactSynergyLevelDefinition>();
        public ArtifactEffectDefinition[] ArtifactEffects = Array.Empty<ArtifactEffectDefinition>();
        public ArtifactSynergyEffectDefinition[] ArtifactSynergyEffects = Array.Empty<ArtifactSynergyEffectDefinition>();
        public EnemyDefinition[] StageOneEnemies = Array.Empty<EnemyDefinition>();
        public EnemyDefinition[] StageTwoEnemies = Array.Empty<EnemyDefinition>();
        public StatusEffectDefinition[] StatusEffects = Array.Empty<StatusEffectDefinition>();
        public StageDefinition Stage = new StageDefinition();
        public StageDefinition TutorialStage = new StageDefinition();
        public TutorialLineDefinition[] TutorialLines = Array.Empty<TutorialLineDefinition>();

        public TutorialLineDefinition GetTutorialLine(string lineId)
        {
            for (var i = 0; i < TutorialLines.Length; i++)
            {
                var line = TutorialLines[i];
                if (line != null && string.Equals(line.LineId, lineId, StringComparison.OrdinalIgnoreCase))
                {
                    return line;
                }
            }

            return null;
        }

        public void RebuildLookup()
        {
            monsters.Clear();
            summons.Clear();
            artifacts.Clear();
            artifactSynergies.Clear();
            artifactSynergyLevels.Clear();
            artifactEffects.Clear();
            artifactSynergyEffects.Clear();
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
            RegisterSummons(Summons);
            RegisterDefinitions(Artifacts, artifacts, definition => definition.ArtifactName);
            RegisterDefinitions(ArtifactSynergies, artifactSynergies, definition => definition.SynergyName);
            RegisterDefinitions(ArtifactSynergyLevels, artifactSynergyLevels, definition => definition.LevelName);
            RegisterDefinitions(ArtifactEffects, artifactEffects, definition => definition.EffectName);
            RegisterDefinitions(ArtifactSynergyEffects, artifactSynergyEffects, definition => definition.EffectName);
            RegisterEnemies(StageOneEnemies);
            RegisterEnemies(StageTwoEnemies);
            RegisterStatusEffects(StatusEffects);
        }

        public T GetData<T>(string Name)
            where T : class
        {
            return TryGetData(Name, out T value) ? value : null;
        }

        public bool TryGetData<T>(string Name, out T value)
            where T : class
        {
            value = null;
            if (string.IsNullOrWhiteSpace(Name))
            {
                return false;
            }

            object resolved = null;
            var targetType = typeof(T);
            if (targetType == typeof(MonsterDefinition))
            {
                monsters.TryGetValue(Name, out var monster);
                resolved = monster;
            }
            else if (targetType == typeof(SummonDefinition))
            {
                summons.TryGetValue(Name, out var summon);
                resolved = summon;
            }
            else if (targetType == typeof(ArtifactDefinition))
            {
                artifacts.TryGetValue(Name, out var artifact);
                resolved = artifact;
            }
            else if (targetType == typeof(ArtifactSynergyDefinition))
            {
                artifactSynergies.TryGetValue(Name, out var synergy);
                resolved = synergy;
            }
            else if (targetType == typeof(ArtifactSynergyLevelDefinition))
            {
                artifactSynergyLevels.TryGetValue(Name, out var synergyLevel);
                resolved = synergyLevel;
            }
            else if (targetType == typeof(ArtifactEffectDefinition))
            {
                artifactEffects.TryGetValue(Name, out var effect);
                resolved = effect;
            }
            else if (targetType == typeof(ArtifactSynergyEffectDefinition))
            {
                artifactSynergyEffects.TryGetValue(Name, out var synergyEffect);
                resolved = synergyEffect;
            }
            else if (targetType == typeof(EnemyDefinition))
            {
                enemies.TryGetValue(Name, out var enemy);
                resolved = enemy;
            }
            else if (targetType == typeof(SkillDefinition))
            {
                activeSkills.TryGetValue(Name, out var activeSkill);
                resolved = activeSkill;
            }
            else if (targetType == typeof(PassiveSkillDefinition))
            {
                passiveSkills.TryGetValue(Name, out var passiveSkill);
                resolved = passiveSkill;
            }
            else if (targetType == typeof(StatusEffectDefinition))
            {
                statusEffects.TryGetValue(Name, out var statusEffect);
                resolved = statusEffect;
            }
            else if (targetType == typeof(SkillChoice))
            {
                skillChoices.TryGetValue(Name, out var choice);
                resolved = choice;
            }
            else if (targetType == typeof(MonsterDefinition.RewardChoiceDefinition))
            {
                rewardChoices.TryGetValue(Name, out var rewardChoice);
                resolved = rewardChoice;
            }

            value = resolved as T;
            return value != null;
        }

        /// Monsters를 반환한다.
        public MonsterDefinition[] GetMonsters()
        {
            return Monsters;
        }

        public StatusRuntimeData GetStatusRuntimeData(StatusEffectKind kind)
        {
            return statusRuntimeData.TryGetValue(kind, out var status) ? status : null;
        }

        public MonsterDefinition GetMonster(string Name)
        {
            return GetData<MonsterDefinition>(Name);
        }

        public SummonDefinition GetSummon(string Name)
        {
            return GetData<SummonDefinition>(Name);
        }

        public SkillDefinition[] GetActiveSkills(string monsterName)
        {
            return GetRegistered(activeSkillsByMonster, monsterName);
        }

        public PassiveSkillDefinition[] GetPassiveSkills(string monsterName)
        {
            return GetRegistered(passiveSkillsByMonster, monsterName);
        }

        public MonsterDefinition.RewardChoiceDefinition[] GetRewardChoices(string monsterName)
        {
            return GetRegistered(rewardChoicesByMonster, monsterName);
        }

        public SkillDefinition GetActiveSkill(string monsterName, SkillSlot slot)
        {
            var skills = GetActiveSkills(monsterName);
            for (var i = 0; i < skills.Length; i++)
            {
                if (skills[i] != null && skills[i].Slot == slot)
                {
                    return skills[i];
                }
            }

            return null;
        }

        public PassiveSkillDefinition ResolvePassiveSkill(string monsterName, SkillSlot slot)
        {
            var passives = GetPassiveSkills(monsterName);
            for (var i = 0; i < passives.Length; i++)
            {
                if (passives[i] != null && passives[i].Slot == slot)
                {
                    return passives[i];
                }
            }

            return null;
        }

        private static T[] GetRegistered<T>(Dictionary<string, T[]> lookup, string Name)
        {
            return !string.IsNullOrWhiteSpace(Name)
                && lookup.TryGetValue(Name, out var values)
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
                if (monster == null || string.IsNullOrWhiteSpace(monster.MonsterName))
                {
                    continue;
                }

                monsters[monster.MonsterName] = monster;
                activeSkillsByMonster[monster.MonsterName] = monster.ActiveSkills ?? Array.Empty<SkillDefinition>();
                passiveSkillsByMonster[monster.MonsterName] = monster.PassiveSkills ?? Array.Empty<PassiveSkillDefinition>();
                rewardChoicesByMonster[monster.MonsterName] = monster.InitialRewardChoices ?? Array.Empty<MonsterDefinition.RewardChoiceDefinition>();

                RegisterActiveSkills(monster.ActiveSkills);
                RegisterPassiveSkills(monster.PassiveSkills);
                RegisterRewardChoices(monster.InitialRewardChoices);
            }
        }

        private void RegisterSummons(SummonDefinition[] catalogSummons)
        {
            if (catalogSummons == null)
            {
                return;
            }

            for (var i = 0; i < catalogSummons.Length; i++)
            {
                var summon = catalogSummons[i];
                if (summon == null || string.IsNullOrWhiteSpace(summon.SummonName))
                {
                    continue;
                }

                summons[summon.SummonName] = summon;
                activeSkillsByMonster[summon.SummonName] = summon.ActiveSkills ?? Array.Empty<SkillDefinition>();
                RegisterActiveSkills(summon.ActiveSkills);
            }
        }

        private static void RegisterDefinitions<T>(
            T[] definitions,
            Dictionary<string, T> lookup,
            Func<T, string> getName)
            where T : class
        {
            for (var i = 0; definitions != null && i < definitions.Length; i++)
            {
                var definition = definitions[i];
                var Name = definition == null ? string.Empty : getName(definition);
                if (!string.IsNullOrWhiteSpace(Name))
                {
                    lookup[Name] = definition;
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
                if (enemy != null && !string.IsNullOrWhiteSpace(enemy.EnemyName))
                {
                    enemies[enemy.EnemyName] = enemy;
                    if (enemy.PassiveSkill != null)
                    {
                        RegisterPassiveSkills(new[] { enemy.PassiveSkill });
                    }
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
                if (status != null && !string.IsNullOrWhiteSpace(status.StatusEffectName))
                {
                    statusEffects[status.StatusEffectName] = status;
                    if (status.RuntimeData != null)
                    {
                        statusRuntimeData[status.Kind] = status.RuntimeData;
                    }
                }
            }
        }

        private void RegisterActiveSkills(SkillDefinition[] skills)
        {
            if (skills == null)
            {
                return;
            }

            for (var i = 0; i < skills.Length; i++)
            {
                var skill = skills[i];
                if (skill == null || string.IsNullOrWhiteSpace(skill.SkillName))
                {
                    continue;
                }

                activeSkills[skill.SkillName] = skill;
                RegisterSkillChoices(skill.EnhancementChoices);
                RegisterSkillChoices(skill.MasterChoices);
            }
        }

        private void RegisterPassiveSkills(PassiveSkillDefinition[] passives)
        {
            if (passives == null)
            {
                return;
            }

            for (var i = 0; i < passives.Length; i++)
            {
                var passive = passives[i];
                if (passive == null || string.IsNullOrWhiteSpace(passive.SkillName))
                {
                    continue;
                }

                passiveSkills[passive.SkillName] = passive;
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
                if (reward != null && !string.IsNullOrWhiteSpace(reward.RewardName))
                {
                    rewardChoices[reward.RewardName] = reward;
                }
            }
        }

        private void RegisterSkillChoices(SkillChoice[] choices)
        {
            if (choices == null)
            {
                return;
            }

            for (var i = 0; i < choices.Length; i++)
            {
                var choice = choices[i];
                if (choice != null && !string.IsNullOrWhiteSpace(choice.ChoiceName))
                {
                    skillChoices[choice.ChoiceName] = choice;
                }
            }
        }
    }
}
