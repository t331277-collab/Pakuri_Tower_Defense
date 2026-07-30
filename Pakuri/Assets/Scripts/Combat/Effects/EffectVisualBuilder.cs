/*
 * 역할: 런타임 효과 비주얼 구성.
 * 책임: 확정된 비주얼 설정으로 효과의 회전·크기·Renderer·Hitbox Collider를 구성한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// EffectVisualBuilder 런타임 데이터를 파싱된 저작 데이터에서 생성한다.
    static class EffectVisualBuilder
    {

        /// 전달된 direction 값을 사용해 Rotation 결과값을 생성해 반환한다.
        public static Quaternion Rotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        /// 전달된 런타임 입력값을 사용해 Configure 작업을 수행한다.
        public static void Configure(
            GameObject instance,
            RuntimeSkillVisualSpec visual,
            bool hitboxIsTrigger,
            bool includeHitbox)
        {
            instance.transform.localScale = visual.LocalScale;

            if (visual.Sprite != null)
            {
                var renderer = instance.AddComponent<SpriteRenderer>();
                renderer.sprite = visual.Sprite;
                renderer.sortingOrder = visual.SortingOrder;
            }

            if (visual.AnimatorController != null)
            {
                var animator = instance.AddComponent<Animator>();
                animator.runtimeAnimatorController = visual.AnimatorController;
            }

            if (includeHitbox && visual.Hitbox != null && visual.Hitbox.HasHitbox())
            {
                ConfigureHitbox(instance, visual.Hitbox, hitboxIsTrigger);
            }
        }

        /// 전달된 런타임 입력값을 사용해 ConfigureAreaEffect 작업을 수행한다.
        public static void ConfigureAreaEffect(
            GameObject instance,
            float baseRadius,
            float skillRadiusMultiplier,
            float skillRadiusBonus,
            float radiusMultiplier = 1f)
        {
            var scaleFactor = SkillTargeting.PrefabScaleFactor(
                baseRadius,
                skillRadiusMultiplier,
                skillRadiusBonus);
            instance.transform.localScale *= scaleFactor;
            instance.transform.localScale *= Mathf.Max(0f, radiusMultiplier);
            Physics2D.SyncTransforms();
        }

        public static void ConfigureLineEffect(
            GameObject instance,
            Vector2 origin,
            Vector2 direction,
            float length,
            float width)
        {
            instance.transform.position = origin + direction * (length * 0.5f);
            instance.transform.rotation = Rotation(direction);
            var renderer = instance.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            var size = renderer.sprite.bounds.size;
            var scale = instance.transform.localScale;
            if (size.x > 0.0001f)
            {
                scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (length / size.x);
            }
            if (size.y > 0.0001f)
            {
                scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (width / size.y);
            }
            instance.transform.localScale = scale;
        }

        public static BoxCollider2D ConfigureLineHitbox(
            GameObject instance,
            float length,
            float width)
        {
            var collider = instance.GetComponent<BoxCollider2D>();
            if (collider == null)
            {
                collider = instance.AddComponent<BoxCollider2D>();
            }

            var scale = instance.transform.lossyScale;
            collider.size = new Vector2(
                length / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
                width / Mathf.Max(0.0001f, Mathf.Abs(scale.y)));
            collider.offset = Vector2.zero;
            collider.isTrigger = true;
            return collider;
        }

        public static void ConfigureZoneEffect(
            GameObject instance,
            Vector2 center,
            float radius,
            bool coverAll,
            bool usePrefabHitbox)
        {
            instance.transform.position = center;
            if (usePrefabHitbox || coverAll || radius <= 0f)
            {
                return;
            }

            var renderer = instance.GetComponent<SpriteRenderer>();
            if (renderer == null || renderer.sprite == null)
            {
                return;
            }

            var size = renderer.sprite.bounds.size;
            var scale = instance.transform.localScale;
            var diameter = radius * 2f;
            if (size.x > 0.0001f)
            {
                scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x) * (diameter / size.x);
            }
            if (size.y > 0.0001f)
            {
                scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y) * (diameter / size.y);
            }
            instance.transform.localScale = scale;
        }

        /// 전달된 런타임 입력값을 사용해 ConfigureHitbox 작업을 수행한다.
        private static void ConfigureHitbox(
            GameObject instance,
            RuntimeSkillHitboxSpec hitbox,
            bool hitboxIsTrigger)
        {
            var collider = instance.AddComponent<BoxCollider2D>();
            collider.size = hitbox.Size;
            collider.isTrigger = hitboxIsTrigger;
        }

    }
}
