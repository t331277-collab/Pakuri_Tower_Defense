using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Data
{
    public class CsvRuntimeCatalog : ScriptableObject
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
        public TextAsset[] MonsterSkillsProjectileFiles;
        public TextAsset[] MonsterSkillsLineAttackFiles;
        public TextAsset[] MonsterSkillsAreaAttackFiles;
        public TextAsset[] MonsterSkillsSingleAttackFiles;
        public TextAsset[] MonsterSkillsBuffFiles;
        public TextAsset[] MonsterSkillsPassiveFiles;
        public TextAsset MonsterSkillNodeDefinitions;
        public TextAsset MonsterSkillNodeDefinitionParams;
        public TextAsset[] MonsterSkillGraphNodeFiles;
        public TextAsset[] MonsterSkillTriggerFiles;
        public TextAsset[] MonsterSkillChoicesProjectileFiles;
        public TextAsset[] MonsterSkillChoicesLineAttackFiles;
        public TextAsset[] MonsterSkillChoicesAreaAttackFiles;
        public TextAsset[] MonsterSkillChoicesSingleAttackFiles;
        public TextAsset[] MonsterSkillChoicesBuffFiles;
        public TextAsset[] MonsterSkillChoicesPassiveFiles;
        public TextAsset StatusEffects;
        public TextAsset Enemies;
        public TextAsset[] EnemySkillBaseFiles;
        public TextAsset[] EnemySkillTriggerFiles;

        [Header("Unity Assets")]
        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();
        public AnimatorControllerEntry[] AnimatorControllers = Array.Empty<AnimatorControllerEntry>();

        private Dictionary<string, Sprite> spriteLookup;
        private Dictionary<string, GameObject> prefabLookup;
        private Dictionary<string, RuntimeAnimatorController> animatorControllerLookup;

        public bool TryGetSprite(string assetPath , out Sprite sprite )
        {
            EnsureLookups();
            return spriteLookup.TryGetValue(Normalize(assetPath), out sprite);
        }

        public bool TryGetPrefab(string assetPath , out GameObject prefab )
        {
            EnsureLookups();
            return prefabLookup.TryGetValue(Normalize(assetPath), out prefab);
        }

        public bool TryGetAnimatorController(
            string assetPath ,
            out RuntimeAnimatorController animatorController )
        {
            EnsureLookups();
            return animatorControllerLookup.TryGetValue(Normalize(assetPath), out animatorController);
        }

        public bool HasSprite(string assetPath )
        {
            return TryGetSprite(assetPath, out _);
        }

        public bool HasPrefab(string assetPath )
        {
            return TryGetPrefab(assetPath, out _);
        }

        public bool HasAnimatorController(string assetPath )
        {
            return TryGetAnimatorController(assetPath, out _);
        }

        public void ResetLookups()
        {
            spriteLookup = null;
            prefabLookup = null;
            animatorControllerLookup = null;
        }

        private void OnEnable()
        {
            ResetLookups();
        }

        private void EnsureLookups()
        {
            if (spriteLookup != null && prefabLookup != null && animatorControllerLookup != null)
            {
                return;
            }

            spriteLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            prefabLookup = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            animatorControllerLookup = new Dictionary<string, RuntimeAnimatorController>(StringComparer.OrdinalIgnoreCase);

            AddEntries(Sprites, spriteLookup, entry => entry.AssetPath, entry => entry.Asset);
            AddEntries(Prefabs, prefabLookup, entry => entry.AssetPath, entry => entry.Asset);
            AddEntries(
                AnimatorControllers,
                animatorControllerLookup,
                entry => entry.AssetPath,
                entry => entry.Asset);
        }

        private static void AddEntries<TEntry, TAsset>(
            TEntry[] entries ,
            Dictionary<string, TAsset> lookup ,
            Func<TEntry, string> getPath ,
            Func<TEntry, TAsset> getAsset )
            where TAsset : UnityEngine.Object
        {
            if (entries == null)
            {
                return;
            }

            for (var i = 0; i < entries.Length; i++)
            {
                var path = Normalize(getPath(entries[i]));
                var asset = getAsset(entries[i]);
                if (string.IsNullOrWhiteSpace(path) || asset == null || lookup.ContainsKey(path))
                {
                    continue;
                }

                lookup.Add(path, asset);
            }
        }

        private static string Normalize(string assetPath )
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            return assetPath.Trim().Replace('\\', '/');
        }
    }
}
