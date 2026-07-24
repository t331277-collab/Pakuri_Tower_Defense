using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Pakuri.NewCore.Definitions;
using Pakuri.NewCore.Definitions.Choices;
using Pakuri.NewCore.Definitions.Skills;
using Pakuri.NewCore.Definitions.Stage;
using Pakuri.NewCore.Definitions.Status;
using Pakuri.NewCore.Definitions.Units;

namespace Pakuri.NewCore.Catalog
{
    public sealed class GameDefinitionCatalog
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

            skills = Unique(snapshot.OfType<SkillDefinition>(), item => item.skill_id, "skill_id");
            choices = Unique(snapshot.OfType<SkillChoiceDefinition>(), item => item.choice_id, "choice_id");
            triggers = Unique(snapshot.OfType<SkillTriggerDefinition>(), item => item.trigger_id, "trigger_id");
            nodeTypes = Unique(snapshot.OfType<NodeTypeDefinition>(), item => item.node_type_id, "node_type_id");
            monsters = Unique(snapshot.OfType<MonsterDefinition>(), item => item.id, "id");
            enemies = Unique(snapshot.OfType<EnemyDefinition>(), item => item.enemy_id, "enemy_id");
            statuses = Unique(
                snapshot.OfType<StatusDefinition>(),
                item => item.status_effect_id,
                "status_effect_id");
            stageDays = Unique(snapshot.OfType<StageDayDefinition>(), item => item.day_key, "day_key");
            stageRewards = Unique(
                snapshot.OfType<StageRewardDefinition>(),
                item => item.reward_rule_id,
                "reward_rule_id");

            EnsureUnique(
                catalogMonsters,
                item => item.id,
                "catalog monster id");
            EnsureUnique(
                modifierChoices,
                item => item.choice_id,
                "modifier choice_id");
            EnsureUnique(
                nodeParams,
                item => $"{item.node_type_id}\u001f{item.param_order}",
                "node parameter key");
            EnsureUnique(
                choiceNodes,
                item => $"{item.owner_kind}\u001f{item.owner_id}\u001f{item.graph_kind}\u001f{item.graph_index}\u001f{item.node_order}",
                "choice node key");
            EnsureUnique(
                stageEncounters,
                item => $"{item.encounter_id}\u001f{item.spawn_order}",
                "encounter spawn key");
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

        public SkillDefinition GetSkill(string skillId)
        {
            return GetRequired(skills, skillId, "skill");
        }

        public MonsterDefinition GetMonster(string monsterId)
        {
            return GetRequired(monsters, monsterId, "monster");
        }

        public EnemyDefinition GetEnemy(string enemyId)
        {
            return GetRequired(enemies, enemyId, "enemy");
        }

        public StatusDefinition GetStatus(string statusId)
        {
            return GetRequired(statuses, statusId, "status");
        }

        public SkillChoiceDefinition GetChoice(string choiceId)
        {
            return GetRequired(choices, choiceId, "choice");
        }

        private static IReadOnlyList<T> ReadOnlyOf<T>(IEnumerable<CsvDefinition> definitions)
            where T : CsvDefinition
        {
            return Array.AsReadOnly(definitions.OfType<T>().ToArray());
        }

        private static IReadOnlyDictionary<string, T> Unique<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            string keyName)
            where T : CsvDefinition
        {
            Dictionary<string, T> result = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (T item in items)
            {
                string key = keySelector(item);
                if (string.IsNullOrEmpty(key))
                {
                    throw Invalid(item, $"Required {keyName} is empty.");
                }

                if (!result.TryAdd(key, item))
                {
                    throw Invalid(item, $"Duplicate {keyName} '{key}'.");
                }
            }

            return new ReadOnlyDictionary<string, T>(result);
        }

        private static void EnsureUnique<T>(
            IEnumerable<T> items,
            Func<T, string> keySelector,
            string keyName)
            where T : CsvDefinition
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (T item in items)
            {
                string key = keySelector(item);
                if (!seen.Add(key))
                {
                    throw Invalid(item, $"Duplicate {keyName} '{key}'.");
                }
            }
        }

        private static T GetRequired<T>(
            IReadOnlyDictionary<string, T> definitions,
            string id,
            string kind)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (!definitions.TryGetValue(id, out T definition))
            {
                throw new KeyNotFoundException($"Unknown {kind} id '{id}'.");
            }

            return definition;
        }

        private static InvalidDataException Invalid(CsvDefinition item, string message)
        {
            return new InvalidDataException(
                $"{item.SourcePath} record {item.SourceRecordNumber}: {message}");
        }
    }
}
