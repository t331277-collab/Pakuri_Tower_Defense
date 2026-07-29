/*
 * 역할: 런타임 효과 비주얼 구성.
 * 책임: 확정된 비주얼 설정으로 효과의 회전·크기·Renderer·Hitbox Collider를 구성한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>EffectVisualBuilder</c> 런타임 데이터를 파싱된 저작 데이터에서 생성한다.</summary>
    static class EffectVisualBuilder
    {

        /// <summary>전달된 <c>direction</c> 값을 사용해 <c>Rotation</c> 결과값을 생성해 반환한다.</summary>
        public static Quaternion Rotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Configure</c> 작업을 수행한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConfigureAreaEffect</c> 작업을 수행한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ConfigureHitbox</c> 작업을 수행한다.</summary>
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
