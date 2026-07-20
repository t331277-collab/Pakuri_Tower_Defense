#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Pakuri.Data
{
    /*
     * Unity Editor에서 CSV 변경을 감지하고 런타임 카탈로그 동기화를 예약한다.
     */
    internal sealed class CsvCatalogPostprocessor : AssetPostprocessor
    {
        private static bool syncQueued;

        /*
         * Editor가 스크립트를 불러온 뒤 최초 카탈로그 동기화를 예약한다.
         */
        [InitializeOnLoadMethod]
        private static void QueueInitialSync()
        {
            QueueSync();
        }

        /*
         * 가져오기, 삭제, 이동된 자산에 CSV 원본이 포함되면 동기화를 예약한다.
         */
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

        /*
         * 변경된 자산 경로에 관리 중인 CSV 원본이 있는지 확인한다.
         */
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

                if (CsvDataLoader.IsAuthoringCsvSourceAssetPath(assetPath))
                {
                    return true;
                }
            }

            return false;
        }

        /*
         * 같은 Editor 갱신 구간에 동기화 요청이 중복 등록되지 않게 예약한다.
         */
        private static void QueueSync()
        {
            if (syncQueued)
            {
                return;
            }

            syncQueued = true;
            EditorApplication.delayCall += RunQueuedSync;
        }

        /*
         * 컴파일과 자산 갱신이 끝난 시점에 런타임 카탈로그를 동기화한다.
         */
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
                CsvDataLoader.SyncImportedSourceCatalogsForEditor();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Pakuri CSV runtime catalog auto-sync failed: {ex}");
            }
        }
    }
}
#endif
