#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Pakuri.Data
{
    internal sealed class PakuriCsvRuntimeCatalogPostprocessor : AssetPostprocessor
    {
        private static bool syncQueued;

        [InitializeOnLoadMethod]
        private static void QueueInitialSync()
        {
            QueueSync();
        }

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!TouchesImportedSource(importedAssets)
                && !TouchesImportedSource(deletedAssets)
                && !TouchesImportedSource(movedAssets)
                && !TouchesImportedSource(movedFromAssetPaths))
            {
                return;
            }

            QueueSync();
        }

        private static bool TouchesImportedSource(string[] assetPaths)
        {
            if (assetPaths == null)
            {
                return false;
            }

            for (var i = 0; i < assetPaths.Length; i++)
            {
                var assetPath = assetPaths[i];
                if (string.IsNullOrWhiteSpace(assetPath))
                {
                    continue;
                }

                if (PakuriCsvRuntimeData.IsRuntimeCsvSourceAssetPath(assetPath))
                {
                    return true;
                }
            }

            return false;
        }

        private static void QueueSync()
        {
            if (syncQueued)
            {
                return;
            }

            syncQueued = true;
            EditorApplication.delayCall += RunQueuedSync;
        }

        private static void RunQueuedSync()
        {
            syncQueued = false;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                QueueSync();
                return;
            }

            try
            {
                PakuriCsvRuntimeData.SyncImportedSourceCatalogsForEditor();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Pakuri CSV runtime catalog auto-sync failed: {ex}");
            }
        }
    }
}
#endif
