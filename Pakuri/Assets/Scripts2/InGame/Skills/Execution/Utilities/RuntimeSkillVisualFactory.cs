using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class RuntimeSkillVisualFactory
    {
        public static bool HasVisual(RuntimeSkillVisualSpec spec)
        {
            return spec != null && spec.HasVisual();
        }

        public static GameObject Create(
            EffectManager effects,
            RuntimeSkillVisualSpec spec,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            bool hitboxIsTrigger = false)
        {
            if (effects == null || !HasVisual(spec))
            {
                return null;
            }

            var instance = effects.CreateRuntimeSkillObject(objectName, position, rotation);
            Configure(instance, spec, hitboxIsTrigger);
            return instance;
        }

        public static void Configure(GameObject instance, RuntimeSkillVisualSpec spec, bool hitboxIsTrigger = false)
        {
            if (instance == null || !HasVisual(spec))
            {
                return;
            }

            var scale = spec.Scale > 0f ? spec.Scale : 1f;
            instance.transform.localScale = new Vector3(scale, scale, scale);

            if (spec.Sprite != null)
            {
                var renderer = instance.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = instance.AddComponent<SpriteRenderer>();
                }

                renderer.sprite = spec.Sprite;
                renderer.sortingOrder = spec.SortingOrder;
            }

            if (spec.AnimatorController != null)
            {
                var animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = spec.AnimatorController;
            }

            if (spec.Hitbox != null && spec.Hitbox.HasHitbox())
            {
                ConfigureHitbox(instance, spec.Hitbox, hitboxIsTrigger);
            }
        }

        private static void ConfigureHitbox(GameObject instance, RuntimeSkillHitboxSpec hitbox, bool hitboxIsTrigger)
        {
            var collider = instance.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = instance.AddComponent<BoxCollider2D>();
            }

            collider.size = hitbox.Size;
            collider.offset = Vector2.zero;
            collider.isTrigger = hitboxIsTrigger;
        }
    }
}
