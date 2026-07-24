using System;
using System.Collections.Generic;
using Pakuri.NewCore.Bootstrap;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Assets
{
    [CreateAssetMenu(
        fileName = "CsvRuntimeCatalog",
        menuName = "Pakuri/New Core/Runtime Catalog")]
    public sealed class NewCoreRuntimeCatalogAsset : ScriptableObject
    {
        [Serializable]
        public struct SpriteEntry
        {
            public string AssetPath;
            public Sprite Asset;
        }

        [Serializable]
        public struct PrefabEntry
        {
            public string AssetPath;
            public GameObject Asset;
        }

        [Serializable]
        public struct AnimatorControllerEntry
        {
            public string AssetPath;
            public RuntimeAnimatorController Asset;
        }

        [Header("CSV Sources")]
        public TextAsset CatalogMonsters;
        public TextAsset Monsters;
        public TextAsset MonsterRewardChoices;
        public TextAsset[] MonsterSkillsProjectileFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsLineAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsAreaAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsSingleAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsBuffFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillsPassiveFiles = Array.Empty<TextAsset>();
        public TextAsset MonsterSkillNodeDefinitions;
        public TextAsset MonsterSkillNodeDefinitionParams;
        public TextAsset[] MonsterSkillGraphNodeFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillTriggerFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesProjectileFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesLineAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesAreaAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesSingleAttackFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesBuffFiles = Array.Empty<TextAsset>();
        public TextAsset[] MonsterSkillChoicesPassiveFiles = Array.Empty<TextAsset>();
        public TextAsset StatusEffects;
        public TextAsset Enemies;
        public TextAsset[] EnemySkillBaseFiles = Array.Empty<TextAsset>();
        public TextAsset[] EnemySkillTriggerFiles = Array.Empty<TextAsset>();
        public TextAsset StageDay;
        public TextAsset StageEncounter;
        public TextAsset StageReward;

        [Header("Unity Assets")]
        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();
        public AnimatorControllerEntry[] AnimatorControllers =
            Array.Empty<AnimatorControllerEntry>();

        private Dictionary<string, Sprite> sprites;
        private Dictionary<string, GameObject> prefabs;
        private Dictionary<string, RuntimeAnimatorController> controllers;

        public GameBootstrap CreateBootstrap()
        {
            var sources = new Dictionary<string, string>(
                StringComparer.Ordinal);
            Add(sources, "Assets/CSVdata/authoring/catalog/catalog_monsters.csv", CatalogMonsters);
            Add(sources, "Assets/CSVdata/authoring/monster/monsters.csv", Monsters);
            Add(sources, "Assets/CSVdata/authoring/monster/monster_modifier_skill_choice.csv", MonsterRewardChoices);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/projectile/", MonsterSkillsProjectileFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/line_attack/", MonsterSkillsLineAttackFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/area_attack/", MonsterSkillsAreaAttackFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/single_attack/", MonsterSkillsSingleAttackFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/buff/", MonsterSkillsBuffFiles);
            AddGroup(sources, "Assets/CSVdata/authoring/monster/skills/base/passive/", MonsterSkillsPassiveFiles);
            Add(sources, "Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definitions.csv", MonsterSkillNodeDefinitions);
            Add(sources, "Assets/CSVdata/authoring/monster/skills/nodes/definitions/skill_node_definition_params.csv", MonsterSkillNodeDefinitionParams);
            AddGraphGroups(sources, MonsterSkillGraphNodeFiles);
            AddTriggerGroups(sources, "monster", MonsterSkillTriggerFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesProjectileFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesLineAttackFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesAreaAttackFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesSingleAttackFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesBuffFiles);
            AddChoiceGroups(sources, MonsterSkillChoicesPassiveFiles);
            Add(sources, "Assets/CSVdata/authoring/status/status_effects.csv", StatusEffects);
            Add(sources, "Assets/CSVdata/authoring/enemy/enemies.csv", Enemies);
            AddEnemyBaseGroups(sources, EnemySkillBaseFiles);
            AddTriggerGroups(sources, "enemy", EnemySkillTriggerFiles);
            Add(sources, "Assets/CSVdata/stage_flow/StageDay.csv", StageDay);
            Add(sources, "Assets/CSVdata/stage_flow/StageEncounter.csv", StageEncounter);
            Add(sources, "Assets/CSVdata/stage_flow/StageReward.csv", StageReward);
            return new GameBootstrap(sources);
        }

        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookups();
            return sprites.TryGetValue(Normalize(assetPath), out sprite);
        }

        public bool TryGetPrefab(string assetPath, out GameObject prefab)
        {
            EnsureLookups();
            return prefabs.TryGetValue(Normalize(assetPath), out prefab);
        }

        public bool TryGetAnimatorController(
            string assetPath,
            out RuntimeAnimatorController controller)
        {
            EnsureLookups();
            return controllers.TryGetValue(Normalize(assetPath), out controller);
        }

        private static void Add(
            IDictionary<string, string> sources,
            string path,
            TextAsset asset)
        {
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Runtime catalog is missing '{path}'.");
            }

            sources.Add(path, asset.text);
        }

        private static void AddGroup(
            IDictionary<string, string> sources,
            string folder,
            IReadOnlyList<TextAsset> assets)
        {
            if (assets == null)
            {
                throw new InvalidOperationException(
                    $"Runtime catalog group '{folder}' is null.");
            }

            for (var index = 0; index < assets.Count; index++)
            {
                var asset = assets[index];
                if (asset == null)
                {
                    throw new InvalidOperationException(
                        $"Runtime catalog group '{folder}' contains a missing asset.");
                }

                Add(sources, folder + asset.name + ".csv", asset);
            }
        }

        private static void AddGraphGroups(
            IDictionary<string, string> sources,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                "Assets/CSVdata/authoring/monster/skills/choices/",
                assets,
                "skill_graph_nodes_");
        }

        private static void AddChoiceGroups(
            IDictionary<string, string> sources,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                "Assets/CSVdata/authoring/monster/skills/choices/",
                assets,
                "skill_choices_");
        }

        private static void AddTriggerGroups(
            IDictionary<string, string> sources,
            string owner,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                $"Assets/CSVdata/authoring/{owner}/skills/triggers/",
                assets,
                string.Empty);
        }

        private static void AddEnemyBaseGroups(
            IDictionary<string, string> sources,
            IReadOnlyList<TextAsset> assets)
        {
            AddCategorized(
                sources,
                "Assets/CSVdata/authoring/enemy/skills/base/",
                assets,
                "skills_");
        }

        private static void AddCategorized(
            IDictionary<string, string> sources,
            string root,
            IReadOnlyList<TextAsset> assets,
            string prefix)
        {
            if (assets == null)
            {
                throw new InvalidOperationException(
                    $"Runtime catalog group '{root}' is null.");
            }

            for (var index = 0; index < assets.Count; index++)
            {
                var asset = assets[index];
                if (asset == null)
                {
                    throw new InvalidOperationException(
                        $"Runtime catalog group '{root}' contains a missing asset.");
                }

                var category = ResolveCategory(asset.name, prefix);
                Add(
                    sources,
                    root + category + "/" + asset.name + ".csv",
                    asset);
            }
        }

        private static string ResolveCategory(string name, string prefix)
        {
            var value = name;
            if (!string.IsNullOrEmpty(prefix)
                && value.StartsWith(prefix, StringComparison.Ordinal))
            {
                value = value.Substring(prefix.Length);
            }

            const string triggerSuffix = "_skill_triger";
            if (value.EndsWith(triggerSuffix, StringComparison.Ordinal))
            {
                value = value.Substring(0, value.Length - triggerSuffix.Length);
            }

            return value;
        }

        private void EnsureLookups()
        {
            if (sprites != null && prefabs != null && controllers != null)
            {
                return;
            }

            sprites = CreateLookup(Sprites, value => value.AssetPath, value => value.Asset);
            prefabs = CreateLookup(Prefabs, value => value.AssetPath, value => value.Asset);
            controllers = CreateLookup(
                AnimatorControllers,
                value => value.AssetPath,
                value => value.Asset);
        }

        private static Dictionary<string, TAsset> CreateLookup<TEntry, TAsset>(
            IReadOnlyList<TEntry> entries,
            Func<TEntry, string> path,
            Func<TEntry, TAsset> asset)
            where TAsset : UnityEngine.Object
        {
            var result = new Dictionary<string, TAsset>(
                StringComparer.OrdinalIgnoreCase);
            if (entries == null)
            {
                return result;
            }

            for (var index = 0; index < entries.Count; index++)
            {
                var key = Normalize(path(entries[index]));
                var value = asset(entries[index]);
                if (!string.IsNullOrEmpty(key) && value != null)
                {
                    if (result.TryGetValue(key, out var existing))
                    {
                        if (existing != value)
                        {
                            throw new InvalidOperationException(
                                $"Runtime catalog path '{key}' maps to multiple assets.");
                        }

                        continue;
                    }

                    result.Add(key, value);
                }
            }

            return result;
        }

        private static string Normalize(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Trim().Replace('\\', '/');
        }
    }
}
