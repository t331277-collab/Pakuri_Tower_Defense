using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 런타임 카탈로그 생성에 필요한 CSV와 Unity 자산 참조를 함께 보관한다.
 */
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

        /*
         * TryGetSprite 작업을 시도하고 성공 여부를 반환한다.
         */
        public bool TryGetSprite(string assetPath /* 에셋 경로 */, out Sprite sprite /* 스프라이트 */)
        {
            EnsureLookups();
            return spriteLookup.TryGetValue(Normalize(assetPath), out sprite);
        }

        /*
         * TryGetPrefab 작업을 시도하고 성공 여부를 반환한다.
         */
        public bool TryGetPrefab(string assetPath /* 에셋 경로 */, out GameObject prefab /* 생성할 프리팹 */)
        {
            EnsureLookups();
            return prefabLookup.TryGetValue(Normalize(assetPath), out prefab);
        }

        /*
         * TryGetAnimatorController 작업을 시도하고 성공 여부를 반환한다.
         */
        public bool TryGetAnimatorController(
            string assetPath /* 에셋 경로 */,
            out RuntimeAnimatorController animatorController /* 애니메이터 제어기 */)
        {
            EnsureLookups();
            return animatorControllerLookup.TryGetValue(Normalize(assetPath), out animatorController);
        }

        /*
         * HasSprite 조건을 만족하는지 확인한다.
         */
        public bool HasSprite(string assetPath /* 에셋 경로 */)
        {
            EnsureLookups();
            return spriteLookup.ContainsKey(Normalize(assetPath));
        }

        /*
         * HasPrefab 조건을 만족하는지 확인한다.
         */
        public bool HasPrefab(string assetPath /* 에셋 경로 */)
        {
            EnsureLookups();
            return prefabLookup.ContainsKey(Normalize(assetPath));
        }

        /*
         * HasAnimatorController 조건을 만족하는지 확인한다.
         */
        public bool HasAnimatorController(string assetPath /* 에셋 경로 */)
        {
            EnsureLookups();
            return animatorControllerLookup.ContainsKey(Normalize(assetPath));
        }

        /*
         * ResetLookups 작업을 수행한다.
         */
        public void ResetLookups()
        {
            spriteLookup = null;
            prefabLookup = null;
            animatorControllerLookup = null;
        }

        /*
         * 컴포넌트가 활성화될 때 이벤트와 표시 상태를 연결한다.
         */
        private void OnEnable()
        {
            ResetLookups();
        }

        /*
         * EnsureLookups에 필요한 상태가 준비되어 있는지 확인하고 구성한다.
         */
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

        /*
         * AddEntries 작업을 수행한다.
         */
        private static void AddEntries<TEntry, TAsset>(
            TEntry[] entries /* 등록 정보 목록 */,
            Dictionary<string, TAsset> lookup /* 조회표 */,
            Func<TEntry, string> getPath /* 가져오기 경로 */,
            Func<TEntry, TAsset> getAsset /* 가져오기 에셋 */)
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

        /*
         * Normalize 작업 결과를 반환한다.
         */
        private static string Normalize(string assetPath /* 에셋 경로 */)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            return assetPath.Trim().Replace('\\', '/');
        }
    }
}
