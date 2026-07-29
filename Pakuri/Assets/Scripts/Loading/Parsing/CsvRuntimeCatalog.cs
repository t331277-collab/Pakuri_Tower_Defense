/*
 * 역할: CSV 참조로 생성된 런타임 에셋 조회.
 * 책임: 정규화된 에셋 경로로 Sprite·Prefab·AnimatorController를 색인한다.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Data
{

    /// <summary><c>CsvRuntimeCatalog</c>가 소유한 런타임 데이터를 색인하고 조회 기능을 제공한다.</summary>
    public class CsvRuntimeCatalog : ScriptableObject
    {

        /// <summary><c>SpriteEntry</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
        [Serializable]
        public struct SpriteEntry
        {
            public string AssetPath;
            public Sprite Asset;
        }

        /// <summary><c>PrefabEntry</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
        [Serializable]
        public struct PrefabEntry
        {
            public string AssetPath;
            public GameObject Asset;
        }

        /// <summary><c>AnimatorControllerEntry</c> 처리에 함께 전달되는 값들을 묶는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Sprite</c> 조회를 시도하고 값이 있는지 반환한다.</summary>
        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookups();
            return spriteLookup.TryGetValue(Normalize(assetPath), out sprite);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Prefab</c> 조회를 시도하고 값이 있는지 반환한다.</summary>
        public bool TryGetPrefab(string assetPath, out GameObject prefab)
        {
            EnsureLookups();
            return prefabLookup.TryGetValue(Normalize(assetPath), out prefab);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>AnimatorController</c> 조회를 시도하고 값이 있는지 반환한다.</summary>
        public bool TryGetAnimatorController(
            string assetPath,
            out RuntimeAnimatorController animatorController)
        {
            EnsureLookups();
            return animatorControllerLookup.TryGetValue(Normalize(assetPath), out animatorController);
        }

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 소유한 런타임 상태에 <c>Sprite</c>가 있는지 반환한다.</summary>
        public bool HasSprite(string assetPath)
        {
            return TryGetSprite(assetPath, out _);
        }

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 소유한 런타임 상태에 <c>Prefab</c>가 있는지 반환한다.</summary>
        public bool HasPrefab(string assetPath)
        {
            return TryGetPrefab(assetPath, out _);
        }

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 소유한 런타임 상태에 <c>AnimatorController</c>가 있는지 반환한다.</summary>
        public bool HasAnimatorController(string assetPath)
        {
            return TryGetAnimatorController(assetPath, out _);
        }

        /// <summary><c>Lookups</c>를 초기 런타임 상태로 되돌린다.</summary>
        public void ResetLookups()
        {
            spriteLookup = null;
            prefabLookup = null;
            animatorControllerLookup = null;
        }

        /// <summary>Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.</summary>
        private void OnEnable()
        {
            ResetLookups();
        }

        /// <summary><c>EnsureLookups</c> 작업을 수행한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Entries</c>를 소유한 런타임 상태에 추가한다.</summary>
        private static void AddEntries<TEntry, TAsset>(
            TEntry[] entries,
            Dictionary<string, TAsset> lookup,
            Func<TEntry, string> getPath,
            Func<TEntry, TAsset> getAsset)
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

        /// <summary>전달된 <c>assetPath</c> 값을 사용해 <c>Normalize</c> 결과값을 생성해 반환한다.</summary>
        private static string Normalize(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            return assetPath.Trim().Replace('\\', '/');
        }
    }
}
