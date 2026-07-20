using Pakuri.Data;
using UnityEngine;

/*
 * 생성된 효과 오브젝트의 외형과 애니메이션 수명을 계산한다.
 */
namespace Pakuri.InGame
{
    internal static class EffectVisualUtility
    {
        /*
         * 런타임 비주얼에 실제로 생성할 외형이나 충돌 영역이 있는지 확인한다.
         */
        public static bool HasVisual(RuntimeSkillVisualSpec visual)
        {
            return visual != null && visual.HasVisual();
        }

        /*
         * 런타임 비주얼 정보로 크기, 스프라이트, 애니메이터, 충돌 영역을 설정한다.
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
                var scale = visual.Scale > 0f ? visual.Scale : 1f;
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
         * 실제 애니메이션 클립 길이와 호출자가 요구한 최소 수명 중 큰 값을 반환한다.
         */
        public static float ResolveLifetime(GameObject instance, float minimumLifetimeSeconds)
        {
            var minimum = Mathf.Max(0.01f, minimumLifetimeSeconds);
            return Mathf.Max(minimum, ResolveAnimationLength(instance));
        }

        /*
         * 런타임 비주얼의 사각 충돌 영역을 설정한다.
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
         * Animator와 Animation에 연결된 클립 중 가장 긴 재생 시간을 찾는다.
         */
        private static float ResolveAnimationLength(GameObject instance)
        {
            var maxLength = 0f;
            var animators = instance.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var controller = animators[i].runtimeAnimatorController;
                if (controller == null)
                {
                    continue;
                }

                var clips = controller.animationClips;
                for (var j = 0; j < clips.Length; j++)
                {
                    maxLength = Mathf.Max(maxLength, clips[j].length);
                }
            }

            var animations = instance.GetComponentsInChildren<Animation>(true);
            for (var i = 0; i < animations.Length; i++)
            {
                foreach (AnimationState state in animations[i])
                {
                    maxLength = Mathf.Max(maxLength, state.clip.length);
                }
            }

            return maxLength;
        }
    }
}
