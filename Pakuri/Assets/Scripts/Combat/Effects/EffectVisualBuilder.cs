/*
 * 역할: 런타임 효과 비주얼 구성.
 * 책임: 확정된 비주얼 설정으로 효과의 회전·크기·Renderer·Hitbox Collider를 구성한다.
 */

using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{
    static class EffectVisualBuilder
    {

        /// 투사체, 광선 회전 적용

        public static Quaternion Rotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }


        /// 비쥬얼 컴포넌트 조립

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

        /// 임시 분기 이펙트

        public static GameObject CreateBranchDamageLine(
            EffectManager effects,
            Vector2 origin,
            Vector2 target,
            out Material material)
        {
            material = null;
            var shader = Shader.Find("Sprites/Default");
            if (effects == null || shader == null)
            {
                return null;
            }

            var instance = effects.CreateEffect(new EffectCreateRequest(
                null,
                null,
                "InGameBranchDamageLine",
                Vector3.zero,
                Quaternion.identity,
                null,
                null,
                false,
                false,
                true));
            if (instance == null)
            {
                return null;
            }

            material = new Material(shader)
            {
                name = "RuntimeBranchDamageLineMaterial"
            };
            var line = instance.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.startWidth = 0.08f;
            line.endWidth = 0.04f;
            line.startColor = new Color(0.1f, 0.65f, 1f, 1f);
            line.endColor = new Color(0.1f, 0.35f, 1f, 0.75f);
            line.numCapVertices = 2;
            line.sortingOrder = 100;
            line.SetPosition(0, new Vector3(origin.x, origin.y, 0f));
            line.SetPosition(1, new Vector3(target.x, target.y, 0f));
            return instance;
        }

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
