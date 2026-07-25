using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Pakuri.NewCore.Definitions;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;

/* CSV에서 만든 전체 게임 정의를 불변 컬렉션과 id 조회로 제공한다. */
namespace Pakuri.NewCore.Catalog
{
    public class GameDefinitionCatalog
    {
        private readonly IReadOnlyList<CsvDefinition> allDefinitions;
        private readonly IReadOnlyList<CatalogMonsterDefinition> catalogMonsters;
        private readonly IReadOnlyList<MonsterModifierSkillChoiceDefinition> modifierChoices;
        private readonly IReadOnlyList<ChoiceNodeDefinition> choiceNodes;
        private readonly IReadOnlyList<NodeParamDefinition> nodeParams;
        private readonly IReadOnlyList<StageEncounterDefinition> stageEncounters;
        private readonly IReadOnlyDictionary<string, SkillDefinition> skills;
        private readonly IReadOnlyDictionary<string, SkillChoiceDefinition> choices;
        private readonly IReadOnlyDictionary<string, SkillTriggerDefinition> triggers;
        private readonly IReadOnlyDictionary<string, NodeTypeDefinition> nodeTypes;
        private readonly IReadOnlyDictionary<string, MonsterDefinition> monsters;
        private readonly IReadOnlyDictionary<string, EnemyDefinition> enemies;
        private readonly IReadOnlyDictionary<string, StatusDefinition> statuses;
        private readonly IReadOnlyDictionary<string, StageDayDefinition> stageDays;
        private readonly IReadOnlyDictionary<string, StageRewardDefinition> stageRewards;

        /* 파싱된 정의 snapshot을 형식별 불변 목록과 고유 id 사전으로 구성한다. */
        internal GameDefinitionCatalog(
            IReadOnlyList<CsvDefinition> definitions,
            int sourceFileCount,
            int schemaFileCount)
        {
            CsvDefinition[] snapshot = definitions.ToArray();
            allDefinitions = Array.AsReadOnly(snapshot);
            SourceFileCount = sourceFileCount;
            SchemaFileCount = schemaFileCount;

            catalogMonsters = ReadOnlyOf<CatalogMonsterDefinition>(snapshot);
            modifierChoices = ReadOnlyOf<MonsterModifierSkillChoiceDefinition>(snapshot);
            choiceNodes = ReadOnlyOf<ChoiceNodeDefinition>(snapshot);
            nodeParams = ReadOnlyOf<NodeParamDefinition>(snapshot);
            stageEncounters = ReadOnlyOf<StageEncounterDefinition>(snapshot);

            skills = Unique(snapshot.OfType<SkillDefinition>(), item => item.skill_id);
            choices = Unique(snapshot.OfType<SkillChoiceDefinition>(), item => item.choice_id);
            triggers = Unique(snapshot.OfType<SkillTriggerDefinition>(), item => item.trigger_id);
            nodeTypes = Unique(snapshot.OfType<NodeTypeDefinition>(), item => item.node_type_id);
            monsters = Unique(snapshot.OfType<MonsterDefinition>(), item => item.id);
            enemies = Unique(snapshot.OfType<EnemyDefinition>(), item => item.enemy_id);
            statuses = Unique(
                snapshot.OfType<StatusDefinition>(),
                item => item.status_effect_id);
            stageDays = Unique(snapshot.OfType<StageDayDefinition>(), item => item.day_key);
            stageRewards = Unique(
                snapshot.OfType<StageRewardDefinition>(),
                item => item.reward_rule_id);
        }

        public int SourceFileCount { get; }

        public int SchemaFileCount { get; }

        public IReadOnlyList<CsvDefinition> AllDefinitions => allDefinitions;

        public IReadOnlyList<CatalogMonsterDefinition> CatalogMonsters => catalogMonsters;

        public IReadOnlyList<MonsterModifierSkillChoiceDefinition> ModifierChoices =>
            modifierChoices;

        public IReadOnlyList<ChoiceNodeDefinition> ChoiceNodes => choiceNodes;

        public IReadOnlyList<NodeParamDefinition> NodeParams => nodeParams;

        public IReadOnlyList<StageEncounterDefinition> StageEncounters => stageEncounters;

        public IReadOnlyDictionary<string, SkillDefinition> Skills => skills;

        public IReadOnlyDictionary<string, SkillChoiceDefinition> Choices => choices;

        public IReadOnlyDictionary<string, SkillTriggerDefinition> Triggers => triggers;

        public IReadOnlyDictionary<string, NodeTypeDefinition> NodeTypes => nodeTypes;

        public IReadOnlyDictionary<string, MonsterDefinition> Monsters => monsters;

        public IReadOnlyDictionary<string, EnemyDefinition> Enemies => enemies;

        public IReadOnlyDictionary<string, StatusDefinition> Statuses => statuses;

        public IReadOnlyDictionary<string, StageDayDefinition> StageDays => stageDays;

        public IReadOnlyDictionary<string, StageRewardDefinition> StageRewards => stageRewards;

        /* skill id에 해당하는 필수 스킬 정의를 반환한다. */
        public SkillDefinition GetSkill(string skillId)
        {
            return Find(skills, skillId);
        }

        /* monster id에 해당하는 필수 몬스터 정의를 반환한다. */
        public MonsterDefinition GetMonster(string monsterId)
        {
            return Find(monsters, monsterId);
        }

        /* enemy id에 해당하는 필수 적 정의를 반환한다. */
        public EnemyDefinition GetEnemy(string enemyId)
        {
            return Find(enemies, enemyId);
        }

        /* status id에 해당하는 필수 상태 정의를 반환한다. */
        public StatusDefinition GetStatus(string statusId)
        {
            return Find(statuses, statusId);
        }

        /* choice id에 해당하는 필수 선택지 정의를 반환한다. */
        public SkillChoiceDefinition GetChoice(string choiceId)
        {
            return Find(choices, choiceId);
        }

        /* 전체 정의에서 지정 형식만 골라 불변 목록으로 반환한다. */
        private static IReadOnlyList<T> ReadOnlyOf<T>(IEnumerable<CsvDefinition> definitions)
            where T : CsvDefinition
        {
            return Array.AsReadOnly(definitions.OfType<T>().ToArray());
        }

        /* 각 key의 첫 정의를 불변 사전으로 구성한다. */
        private static IReadOnlyDictionary<string, T> Unique<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector)
            where T : CsvDefinition
        {
            Dictionary<string, T> result = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (T item in items)
            {
                string key = keySelector(item);
                result.TryAdd(key, item);
            }

            return new ReadOnlyDictionary<string, T>(result);
        }

        /* 불변 사전에서 id에 대응하는 정의를 조회한다. */
        private static T Find<T>(
            IReadOnlyDictionary<string, T> definitions,
            string id)
        {
            definitions.TryGetValue(id, out T definition);
            return definition;
        }

    }
}
