using UnityEngine;

namespace Pakuri.Combat
{
    internal struct CombatEffectInstance
    {
        public CombatEffectInstance(GameObject gameObject, Transform transform, SpriteRenderer renderer)
        {
            GameObject = gameObject;
            Transform = transform;
            Renderer = renderer;
        }

        public GameObject GameObject { get; private set; }
        public Transform Transform { get; private set; }
        public SpriteRenderer Renderer { get; private set; }
    }

    internal static class CombatEffectFactory
    {
        public static CombatEffectInstance CreateLine(
            string name,
            Transform parent,
            Vector3 origin,
            Vector3 direction,
            float length,
            float width,
            GameObject prefab,
            Sprite fallbackSprite)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector3.right;
            }

            direction.Normalize();
            var effectObject = CreateEffectObject(name, parent, prefab);
            effectObject.transform.position = origin + direction * (length * 0.5f);
            effectObject.transform.right = direction;
            if (prefab == null)
            {
                effectObject.transform.localScale = new Vector3(Mathf.Max(0.01f, length), Mathf.Max(0.01f, width), 1f);
            }

            var renderer = ResolveRenderer(effectObject, prefab == null, fallbackSprite);
            if (renderer != null && prefab == null)
            {
                renderer.color = Color.white;
                renderer.sortingOrder = 22;
            }

            return new CombatEffectInstance(effectObject, effectObject.transform, renderer);
        }

        public static CombatEffectInstance CreateCircle(
            string name,
            Transform parent,
            Vector3 position,
            float radius,
            GameObject prefab,
            Sprite fallbackSprite)
        {
            var diameter = Mathf.Max(0.01f, radius * 2f);
            var effectObject = CreateEffectObject(name, parent, prefab);
            effectObject.transform.position = position;
            if (prefab == null)
            {
                effectObject.transform.localScale = new Vector3(diameter, diameter, 1f);
            }

            var renderer = ResolveRenderer(effectObject, prefab == null, fallbackSprite);
            if (renderer != null && prefab == null)
            {
                renderer.color = Color.white;
                renderer.sortingOrder = 21;
            }

            return new CombatEffectInstance(effectObject, effectObject.transform, renderer);
        }

        private static GameObject CreateEffectObject(string name, Transform parent, GameObject prefab)
        {
            GameObject effectObject;
            if (prefab != null)
            {
                effectObject = Object.Instantiate(prefab);
                effectObject.name = name;
                if (parent != null)
                {
                    effectObject.transform.SetParent(parent, false);
                }
            }
            else
            {
                effectObject = new GameObject(name);
                if (parent != null)
                {
                    effectObject.transform.SetParent(parent, false);
                }
            }

            return effectObject;
        }

        private static SpriteRenderer ResolveRenderer(GameObject effectObject, bool addFallbackRenderer, Sprite fallbackSprite)
        {
            var renderer = effectObject.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = effectObject.GetComponentInChildren<SpriteRenderer>();
            }

            if (renderer == null && addFallbackRenderer)
            {
                renderer = effectObject.AddComponent<SpriteRenderer>();
                renderer.sprite = fallbackSprite;
            }

            return renderer;
        }
    }
}
