using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * CSV에 기록된 자산 경로와 런타임 Sprite, Prefab, Animator를 연결한다.
     */
    public sealed class PakuriCsvRuntimeAssetCatalog : ScriptableObject
    {
        /*
         * CSV 자산 경로와 Sprite 참조 한 쌍을 보관한다.
         */
        [Serializable]
        public struct SpriteEntry
        {
            public string AssetPath;
            public Sprite Asset;
        }

        /*
         * CSV 자산 경로와 Prefab 참조 한 쌍을 보관한다.
         */
        [Serializable]
        public struct PrefabEntry
        {
            public string AssetPath;
            public GameObject Asset;
        }

        /*
         * CSV 자산 경로와 AnimatorController 참조 한 쌍을 보관한다.
         */
        [Serializable]
        public struct AnimatorControllerEntry
        {
            public string AssetPath;
            public RuntimeAnimatorController Asset;
        }

        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();
        public AnimatorControllerEntry[] AnimatorControllers = Array.Empty<AnimatorControllerEntry>();

        private Dictionary<string, Sprite> spriteLookup;
        private Dictionary<string, GameObject> prefabLookup;
        private Dictionary<string, RuntimeAnimatorController> animatorControllerLookup;

        /*
         * 경로에 연결된 Sprite를 조회한다.
         */
        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookups();
            return spriteLookup.TryGetValue(Normalize(assetPath), out sprite);
        }

        /*
         * 경로에 연결된 Prefab을 조회한다.
         */
        public bool TryGetPrefab(string assetPath, out GameObject prefab)
        {
            EnsureLookups();
            return prefabLookup.TryGetValue(Normalize(assetPath), out prefab);
        }

        /*
         * 경로에 연결된 AnimatorController를 조회한다.
         */
        public bool TryGetAnimatorController(string assetPath, out RuntimeAnimatorController animatorController)
        {
            EnsureLookups();
            return animatorControllerLookup.TryGetValue(Normalize(assetPath), out animatorController);
        }

        /*
         * Sprite 경로가 카탈로그에 등록되어 있는지 확인한다.
         */
        public bool HasSprite(string assetPath)
        {
            EnsureLookups();
            return spriteLookup.ContainsKey(Normalize(assetPath));
        }

        /*
         * Prefab 경로가 카탈로그에 등록되어 있는지 확인한다.
         */
        public bool HasPrefab(string assetPath)
        {
            EnsureLookups();
            return prefabLookup.ContainsKey(Normalize(assetPath));
        }

        /*
         * AnimatorController 경로가 카탈로그에 등록되어 있는지 확인한다.
         */
        public bool HasAnimatorController(string assetPath)
        {
            EnsureLookups();
            return animatorControllerLookup.ContainsKey(Normalize(assetPath));
        }

        /*
         * 직렬화된 자산 목록이 바뀌었을 때 경로 조회 캐시를 비운다.
         */
        public void ResetLookups()
        {
            spriteLookup = null;
            prefabLookup = null;
            animatorControllerLookup = null;
        }

        /*
         * 카탈로그 자산이 활성화되면 이전 경로 조회 캐시를 비운다.
         */
        private void OnEnable()
        {
            ResetLookups();
        }

        /*
         * 직렬화된 자산 목록으로 종류별 경로 조회 표를 만든다.
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

            if (Sprites != null)
            {
                for (var i = 0; i < Sprites.Length; i++)
                {
                    var entry = Sprites[i];
                    var normalized = Normalize(entry.AssetPath);
                    if (string.IsNullOrWhiteSpace(normalized) || entry.Asset == null || spriteLookup.ContainsKey(normalized))
                    {
                        continue;
                    }

                    spriteLookup.Add(normalized, entry.Asset);
                }
            }

            if (Prefabs != null)
            {
                for (var i = 0; i < Prefabs.Length; i++)
                {
                    var entry = Prefabs[i];
                    var normalized = Normalize(entry.AssetPath);
                    if (string.IsNullOrWhiteSpace(normalized) || entry.Asset == null || prefabLookup.ContainsKey(normalized))
                    {
                        continue;
                    }

                    prefabLookup.Add(normalized, entry.Asset);
                }
            }

            if (AnimatorControllers != null)
            {
                for (var i = 0; i < AnimatorControllers.Length; i++)
                {
                    var entry = AnimatorControllers[i];
                    var normalized = Normalize(entry.AssetPath);
                    if (string.IsNullOrWhiteSpace(normalized)
                        || entry.Asset == null
                        || animatorControllerLookup.ContainsKey(normalized))
                    {
                        continue;
                    }

                    animatorControllerLookup.Add(normalized, entry.Asset);
                }
            }
        }

        /*
         * 자산 경로의 공백과 경로 구분자를 조회 형식에 맞춘다.
         */
        private static string Normalize(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Trim().Replace('\\', '/');
        }
    }
}
