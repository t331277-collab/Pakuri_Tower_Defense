/*
 * 역할: 런타임 GameObject 재사용 저장소.
 * 책임: key별 비활성 인스턴스 보관·회수만 담당하고, 실제 상태 초기화는 호출자가 담당한다.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{

    public sealed class RuntimeObjectPool<TKey>
    {
        private readonly Dictionary<TKey, Stack<GameObject>> inactive = new Dictionary<TKey, Stack<GameObject>>();
        private readonly Dictionary<GameObject, TKey> activeKeys = new Dictionary<GameObject, TKey>();

        public GameObject Get(TKey key, Func<GameObject> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException(nameof(create));
            }

            if (!inactive.TryGetValue(key, out var instances))
            {
                instances = new Stack<GameObject>();
                inactive.Add(key, instances);
            }

            GameObject instance = null;
            while (instances.Count > 0 && instance == null)
            {
                instance = instances.Pop();
            }

            instance ??= create();
            if (instance == null)
            {
                return null;
            }

            activeKeys[instance] = key;
            instance.SetActive(true);
            return instance;
        }

        public bool Release(GameObject instance)
        {
            if (instance == null || !activeKeys.TryGetValue(instance, out var key))
            {
                return false;
            }

            activeKeys.Remove(instance);
            StopCoroutines(instance);
            instance.SetActive(false);

            if (!inactive.TryGetValue(key, out var instances))
            {
                instances = new Stack<GameObject>();
                inactive.Add(key, instances);
            }

            instances.Push(instance);
            return true;
        }

        public void Clear()
        {
            foreach (var pair in inactive)
            {
                while (pair.Value.Count > 0)
                {
                    DestroyInstance(pair.Value.Pop());
                }
            }

            foreach (var pair in activeKeys)
            {
                DestroyInstance(pair.Key);
            }

            inactive.Clear();
            activeKeys.Clear();
        }

        private static void StopCoroutines(GameObject instance)
        {
            var behaviours = instance.GetComponentsInChildren<MonoBehaviour>(true);
            for (var i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null)
                {
                    behaviours[i].StopAllCoroutines();
                }
            }
        }

        private static void DestroyInstance(GameObject instance)
        {
            if (instance != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                    return;
                }
#endif
                UnityEngine.Object.Destroy(instance);
            }
        }
    }
}
