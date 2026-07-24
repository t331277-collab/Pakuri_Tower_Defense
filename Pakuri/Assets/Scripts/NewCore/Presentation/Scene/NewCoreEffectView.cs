using System.Collections.Generic;
using Pakuri.NewCore.Combat.Effects;
using Pakuri.NewCore.Presentation.Actors;
using Pakuri.NewCore.Presentation.Assets;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Scene
{
    public sealed class NewCoreEffectView : MonoBehaviour
    {
        [SerializeField] private Transform runtimeSkillRoot;

        private readonly Dictionary<int, GameObject> instances =
            new Dictionary<int, GameObject>();
        private readonly List<int> removals = new List<int>();
        private NewCoreRuntimeCatalogAsset catalog;
        private EffectManager effects;

        public void Bind(
            NewCoreRuntimeCatalogAsset runtimeCatalog,
            EffectManager effectManager)
        {
            catalog = runtimeCatalog
                ?? throw new System.ArgumentNullException(nameof(runtimeCatalog));
            effects = effectManager
                ?? throw new System.ArgumentNullException(nameof(effectManager));
            if (runtimeSkillRoot == null)
            {
                throw new System.InvalidOperationException(
                    "Runtime skill root is missing.");
            }
        }

        public void Sync()
        {
            if (effects == null)
            {
                return;
            }

            for (var index = 0; index < effects.ActiveEffects.Count; index++)
            {
                var handle = effects.ActiveEffects[index];
                if (!instances.TryGetValue(handle.Id, out var instance))
                {
                    instance = CreateInstance(handle);
                    instances.Add(handle.Id, instance);
                }

                SyncInstance(instance, handle);
            }

            removals.Clear();
            foreach (var pair in instances)
            {
                if (!Contains(pair.Key))
                {
                    removals.Add(pair.Key);
                }
            }

            for (var index = 0; index < removals.Count; index++)
            {
                var id = removals[index];
                Destroy(instances[id]);
                instances.Remove(id);
            }
        }

        public void Clear()
        {
            foreach (var instance in instances.Values)
            {
                if (instance != null)
                {
                    Destroy(instance);
                }
            }

            instances.Clear();
        }

        private GameObject CreateInstance(EffectHandle handle)
        {
            var visual = handle.Visual;
            GameObject instance;
            if (!string.IsNullOrWhiteSpace(visual.PrefabPath))
            {
                if (!catalog.TryGetPrefab(
                        visual.PrefabPath,
                        out var prefab))
                {
                    throw new System.InvalidOperationException(
                        $"No visual prefab is mapped for '{visual.PrefabPath}'.");
                }

                instance = Instantiate(prefab, runtimeSkillRoot);
            }
            else if (!string.IsNullOrWhiteSpace(visual.SpritePath))
            {
                if (!catalog.TryGetSprite(
                        visual.SpritePath,
                        out var sprite))
                {
                    throw new System.InvalidOperationException(
                        $"No visual sprite is mapped for '{visual.SpritePath}'.");
                }

                instance = new GameObject(
                    string.IsNullOrWhiteSpace(sprite.name)
                        ? "NewCoreSkillVisual"
                        : sprite.name);
                instance.transform.SetParent(runtimeSkillRoot, false);
                instance.AddComponent<SpriteRenderer>().sprite = sprite;
            }
            else
            {
                if (!visual.HasResource)
                {
                    return null;
                }

                throw new System.InvalidOperationException(
                    "The effect visual has no creatable prefab or sprite.");
            }

            ConfigureRuntimeVisual(instance, visual);
            var actor = instance.GetComponentInChildren<SkillVisualActorBehaviour>(true);
            if (actor != null)
            {
                actor.Bind(handle);
            }

            return instance;
        }

        private void ConfigureRuntimeVisual(
            GameObject instance,
            EffectVisualSpec visual)
        {
            if (!string.IsNullOrWhiteSpace(visual.SpritePath))
            {
                if (!catalog.TryGetSprite(
                        visual.SpritePath,
                        out var sprite))
                {
                    throw new System.InvalidOperationException(
                        $"No visual sprite is mapped for '{visual.SpritePath}'.");
                }

                var renderer = instance.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = instance.AddComponent<SpriteRenderer>();
                }

                renderer.sprite = sprite;
                renderer.sortingOrder = visual.SortingOrder;
            }

            if (!string.IsNullOrWhiteSpace(
                    visual.AnimatorControllerPath))
            {
                if (!catalog.TryGetAnimatorController(
                        visual.AnimatorControllerPath,
                        out var controller))
                {
                    throw new System.InvalidOperationException(
                        "No visual AnimatorController is mapped for "
                        + $"'{visual.AnimatorControllerPath}'.");
                }

                var animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = controller;
            }

            if (visual.UsesLocalScale)
            {
                instance.transform.localScale = new Vector3(
                    visual.ScaleX == 0f ? 1f : visual.ScaleX,
                    visual.ScaleY == 0f ? 1f : visual.ScaleY,
                    visual.ScaleZ == 0f ? 1f : visual.ScaleZ);
            }
            else if (!string.IsNullOrWhiteSpace(visual.SpritePath)
                || !string.IsNullOrWhiteSpace(
                    visual.AnimatorControllerPath))
            {
                instance.transform.localScale =
                    Vector3.one * visual.Scale;
            }
        }

        private static void SyncInstance(
            GameObject instance,
            EffectHandle handle)
        {
            if (instance == null)
            {
                return;
            }

            var actor = instance.GetComponentInChildren<SkillVisualActorBehaviour>(true);
            if (actor != null)
            {
                actor.Sync();
                return;
            }

            instance.transform.position = new Vector3(
                handle.Position.X,
                handle.Position.Y,
                instance.transform.position.z);
            if (handle.Direction.SqrMagnitude > 0.0001f)
            {
                instance.transform.right = new Vector3(
                    handle.Direction.X,
                    handle.Direction.Y,
                    0f);
            }
        }

        private bool Contains(int id)
        {
            for (var index = 0; index < effects.ActiveEffects.Count; index++)
            {
                if (effects.ActiveEffects[index].Id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
