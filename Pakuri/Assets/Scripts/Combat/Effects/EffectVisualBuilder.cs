using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    static class EffectVisualBuilder
    {
        private const float DefaultSingleAttackLineLength = 31f;

        public static Quaternion ResolveRotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        public static void Configure(
            GameObject instance,
            RuntimeSkillVisualSpec visual,
            bool hitboxIsTrigger,
            bool includeHitbox)
        {
            if (visual.UseLocalScale)
            {
                instance.transform.localScale = visual.LocalScale;
            }
            else
            {
                var scale = visual.Scale;
                if (scale <= 0f)
                {
                    scale = 1f;
                }

                instance.transform.localScale = new Vector3(scale, scale, scale);
            }

            if (visual.Sprite != null)
            {
                var renderer = instance.GetComponent<SpriteRenderer>();
                if (renderer == null)
                {
                    renderer = instance.AddComponent<SpriteRenderer>();
                }

                renderer.sprite = visual.Sprite;
                renderer.sortingOrder = visual.SortingOrder;
            }

            if (visual.AnimatorController != null)
            {
                var animator = instance.GetComponent<Animator>();
                if (animator == null)
                {
                    animator = instance.AddComponent<Animator>();
                }

                animator.runtimeAnimatorController = visual.AnimatorController;
            }

            if (includeHitbox && visual.Hitbox != null && visual.Hitbox.HasHitbox())
            {
                ConfigureHitbox(instance, visual.Hitbox, hitboxIsTrigger);
            }
        }

        public static float ResolveLifetime(GameObject instance, float minimumLifetimeSeconds)
        {
            var minimum = Mathf.Max(0.01f, minimumLifetimeSeconds);
            var animationLength = ResolveAnimationLength(instance);
            return Mathf.Max(minimum, animationLength);
        }

        public static bool HasHitbox(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            var colliders = instance.GetComponentsInChildren<Collider2D>();
            return colliders.Length > 0;
        }

        public static void ConfigureAreaScale(
            Transform target,
            float baseRadius,
            SkillSnapshot snapshot,
            float radiusMultiplier)
        {
            if (target == null)
            {
                return;
            }

            ApplyPrefabScale(target, baseRadius, snapshot);
            target.localScale *= Mathf.Max(0f, radiusMultiplier);
        }

        public static void ConfigureSingleAttackLine(
            Transform target,
            SkillExecutionContext context,
            SingleSkillRuntimeData skill,
            SkillSnapshot snapshot,
            Vector2 center)
        {
            if (target == null || skill == null)
            {
                return;
            }

            var origin = center;
            if (context != null
                && context.CasterEntry != null
                && context.CasterEntry.Transform != null)
            {
                origin = context.CasterEntry.Transform.position;
            }

            var direction = center - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            target.position = center;
            target.rotation = ResolveRotation(direction.normalized);

            var baseRadius = SkillTargeting.ResolveBaseRadius(skill.Targeting, skill.Area);
            var width = SkillTargeting.ResolveRadius(baseRadius, snapshot);
            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                ApplyPrefabScale(target, baseRadius, snapshot);
                return;
            }

            var size = spriteRenderer.sprite.bounds.size;
            var scale = target.localScale;
            if (size.x > 0.0001f)
            {
                var xSign = 1f;
                if (scale.x < 0f)
                {
                    xSign = -1f;
                }

                scale.x = xSign * (DefaultSingleAttackLineLength / size.x);
            }

            if (size.y > 0.0001f)
            {
                var ySign = 1f;
                if (scale.y < 0f)
                {
                    ySign = -1f;
                }

                scale.y = ySign * (width / size.y);
            }

            target.localScale = scale;
        }

        private static void ApplyPrefabScale(Transform target, float baseRadius, SkillSnapshot snapshot)
        {
            if (target == null || snapshot == null)
            {
                return;
            }

            var scaleFactor = SkillTargeting.ResolvePrefabScaleFactor(baseRadius, snapshot);
            if (!Mathf.Approximately(scaleFactor, 1f))
            {
                target.localScale *= scaleFactor;
            }
        }

        private static void ConfigureHitbox(
            GameObject instance,
            RuntimeSkillHitboxSpec hitbox,
            bool hitboxIsTrigger)
        {
            var collider = instance.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = instance.AddComponent<BoxCollider2D>();
            }

            collider.size = hitbox.Size;
            collider.offset = hitbox.Offset;
            collider.isTrigger = hitboxIsTrigger;
        }

        private static float ResolveAnimationLength(GameObject instance)
        {
            if (instance == null)
            {
                return 0f;
            }

            var maxLength = 0f;
            var animators = instance.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                if (animators[i] == null)
                {
                    continue;
                }

                var controller = animators[i].runtimeAnimatorController;
                if (controller == null)
                {
                    continue;
                }

                var clips = controller.animationClips;
                for (var j = 0; j < clips.Length; j++)
                {
                    var clip = clips[j];
                    if (clip != null)
                    {
                        maxLength = Mathf.Max(maxLength, clip.length);
                    }
                }
            }

            var animations = instance.GetComponentsInChildren<Animation>(true);
            for (var i = 0; i < animations.Length; i++)
            {
                if (animations[i] == null)
                {
                    continue;
                }

                foreach (AnimationState state in animations[i])
                {
                    if (state != null)
                    {
                        maxLength = Mathf.Max(maxLength, state.length);
                    }
                }
            }

            return maxLength;
        }
    }
}
