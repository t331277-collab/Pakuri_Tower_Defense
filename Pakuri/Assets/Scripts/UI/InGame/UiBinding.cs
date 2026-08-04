using UnityEngine;

namespace Pakuri.InGame
{
    /// InGame UI의 필수 씬 참조를 찾고 실패 원인을 일관되게 기록한다.
    internal static class UiBindingUtility
    {
        internal static T BindChild<T>(Component owner, string path, string fieldName, ref bool valid)
            where T : Component
        {
            return BindChild<T>(owner, owner != null ? owner.transform : null, path, fieldName, ref valid);
        }

        internal static T BindChild<T>(Component owner, Transform root, string path, string fieldName, ref bool valid)
            where T : Component
        {
            var target = root != null ? root.Find(path) : null;
            if (target == null)
            {
                LogMissing(owner, fieldName, path, typeof(T).Name);
                valid = false;
                return null;
            }

            var component = target.GetComponent<T>();
            if (component == null)
            {
                LogMissing(owner, fieldName, path, typeof(T).Name);
                valid = false;
            }

            return component;
        }

        internal static GameObject BindChildObject(Component owner, Transform root, string path, string fieldName, ref bool valid)
        {
            var target = root != null ? root.Find(path) : null;
            if (target == null)
            {
                LogMissing(owner, fieldName, path, nameof(GameObject));
                valid = false;
                return null;
            }

            return target.gameObject;
        }

        internal static T BindOptionalChild<T>(Transform root, string path)
            where T : Component
        {
            var target = root != null ? root.Find(path) : null;
            return target != null ? target.GetComponent<T>() : null;
        }

        internal static T BindSelf<T>(Component owner, Transform target, string fieldName, ref bool valid)
            where T : Component
        {
            var component = target != null ? target.GetComponent<T>() : null;
            if (component == null)
            {
                LogMissing(owner, fieldName, target != null ? target.name : "<missing>", typeof(T).Name);
                valid = false;
            }

            return component;
        }

        internal static T BindScene<T>(Component owner, string path, string fieldName, ref bool valid)
            where T : Component
        {
            Transform found = null;
            var roots = owner != null ? owner.gameObject.scene.GetRootGameObjects() : new GameObject[0];
            for (var i = 0; i < roots.Length; i++)
            {
                var candidate = roots[i].name == path ? roots[i].transform : roots[i].transform.Find(path);
                if (candidate == null)
                {
                    continue;
                }

                if (found != null && found != candidate)
                {
                    LogError(owner, $"multiple objects matched path '{path}' for field '{fieldName}'.");
                    valid = false;
                    return null;
                }

                found = candidate;
            }

            if (found == null)
            {
                LogMissing(owner, fieldName, path, typeof(T).Name);
                valid = false;
                return null;
            }

            var component = found.GetComponent<T>();
            if (component == null)
            {
                LogMissing(owner, fieldName, path, typeof(T).Name);
                valid = false;
            }

            return component;
        }

        internal static T BindSceneComponent<T>(Component owner, string fieldName, ref bool valid)
            where T : Component
        {
            T found = null;
            var roots = owner != null ? owner.gameObject.scene.GetRootGameObjects() : new GameObject[0];
            for (var i = 0; i < roots.Length; i++)
            {
                var components = roots[i].GetComponentsInChildren<T>(true);
                for (var j = 0; j < components.Length; j++)
                {
                    if (found != null && found != components[j])
                    {
                        LogError(owner, $"multiple '{typeof(T).Name}' components found for field '{fieldName}'.");
                        valid = false;
                        return null;
                    }

                    found = components[j];
                }
            }

            if (found == null)
            {
                LogMissing(owner, fieldName, "current scene", typeof(T).Name);
                valid = false;
            }

            return found;
        }

        private static void LogMissing(Component owner, string fieldName, string path, string expectedType)
        {
            LogError(owner, $"field '{fieldName}' at path '{path}' requires '{expectedType}'.");
        }

        private static void LogError(Component owner, string detail)
        {
            Debug.LogError($"{owner?.GetType().Name ?? nameof(UiBindingUtility)} BindObject failed: {detail}", owner);
        }
    }
}
