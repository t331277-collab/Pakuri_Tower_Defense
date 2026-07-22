using Pakuri.Data;
using UnityEngine;

/*
 * EffectManager가 생성한 런타임 효과에 스프라이트, 애니메이터, 크기와 충돌 영역을 적용한다.
 * 공격 방향과 범위에 맞는 회전·크기를 계산하고 효과 오브젝트의 유지 시간을 결정한다.
 */
namespace Pakuri.InGame
{
    /*
     * 스킬 시각 오브젝트를 런타임 설정에 맞게 구성한다.
     */
    static class EffectVisualBuilder
    {
        private const float DefaultSingleAttackLineLength = 31f;

        /*
         * 진행 방향을 2D 회전값으로 바꾼다.
         */
        public static Quaternion ResolveRotation(Vector2 direction)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity;
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return Quaternion.Euler(0f, 0f, angle);
        }

        /*
         * 런타임 시각 설정에 따라 크기, 스프라이트, 애니메이터와 충돌 영역을 구성한다.
         */
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

        /*
         * 최소 유지 시간과 애니메이션 길이 중 더 긴 값을 반환한다.
         */
        public static float ResolveLifetime(GameObject instance, float minimumLifetimeSeconds)
        {
            var minimum = Mathf.Max(0.01f, minimumLifetimeSeconds);
            var animationLength = ResolveAnimationLength(instance);
            return Mathf.Max(minimum, animationLength);
        }

        /*
         * 효과 오브젝트 또는 자식에 2D 충돌 영역이 있는지 확인한다.
         */
        public static bool HasHitbox(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            var colliders = instance.GetComponentsInChildren<Collider2D>();
            return colliders.Length > 0;
        }

        /*
         * 범위 반경과 스킬 강화 배율을 효과 오브젝트 크기에 적용한다.
         */
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

        /*
         * 단일 공격 효과를 시전자에서 대상 방향으로 회전하고 공격 폭에 맞게 조정한다.
         */
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

        /*
         * 기본 반경과 스킬 강화에서 계산한 프리팹 크기 배율을 적용한다.
         */
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

        /*
         * 런타임 충돌 영역의 크기, 중심과 Trigger 사용 여부를 설정한다.
         */
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

        /*
         * Animator와 Animation에 등록된 클립 중 가장 긴 재생 시간을 반환한다.
         */
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
