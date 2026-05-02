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

        public SpriteEntry[] Sprites = Array.Empty<SpriteEntry>();
        public PrefabEntry[] Prefabs = Array.Empty<PrefabEntry>();

        private Dictionary<string, Sprite> spriteLookup;
        private Dictionary<string, GameObject> prefabLookup;

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

        public void ResetLookups()
        {
            spriteLookup = null;
            prefabLookup = null;
        }

        private void OnEnable()
        {
            ResetLookups();
        }

        private void EnsureLookups()
        {
            if (spriteLookup != null && prefabLookup != null)
            {
                return;
            }

            spriteLookup = new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);
            prefabLookup = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);

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
        }

        private static string Normalize(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Trim().Replace('\\', '/');
        }
    }
}
