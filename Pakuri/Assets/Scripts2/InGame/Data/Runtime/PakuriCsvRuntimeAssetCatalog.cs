using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.Data
{
    [CreateAssetMenu(menuName = "Pakuri/CSV Runtime Asset Catalog", fileName = "PakuriCsvRuntimeAssetCatalog")]
    public sealed class PakuriCsvRuntimeAssetCatalog : ScriptableObject
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

        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();
        public AnimatorControllerEntry[] AnimatorControllers = Array.Empty<AnimatorControllerEntry>();

        private Dictionary<string, Sprite> spriteLookup;
        private Dictionary<string, GameObject> prefabLookup;
        private Dictionary<string, RuntimeAnimatorController> animatorControllerLookup;

        public bool TryGetSprite(string assetPath, out Sprite sprite)
        {
            EnsureLookups();
            return spriteLookup.TryGetValue(Normalize(assetPath), out sprite);
        }

        public bool TryGetPrefab(string assetPath, out GameObject prefab)
        {
            EnsureLookups();
            return prefabLookup.TryGetValue(Normalize(assetPath), out prefab);
        }

        public bool TryGetAnimatorController(string assetPath, out RuntimeAnimatorController animatorController)
        {
            EnsureLookups();
            return animatorControllerLookup.TryGetValue(Normalize(assetPath), out animatorController);
        }

        public bool HasSprite(string assetPath)
        {
            EnsureLookups();
            return spriteLookup.ContainsKey(Normalize(assetPath));
        }

        public bool HasPrefab(string assetPath)
        {
            EnsureLookups();
            return prefabLookup.ContainsKey(Normalize(assetPath));
        }

        public bool HasAnimatorController(string assetPath)
        {
            EnsureLookups();
            return animatorControllerLookup.ContainsKey(Normalize(assetPath));
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

        private static string Normalize(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Trim().Replace('\\', '/');
        }
    }
}
