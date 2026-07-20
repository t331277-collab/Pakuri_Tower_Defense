using Pakuri.Data;
using UnityEngine;

/*
 * 생성된 효과 오브젝트의 외형과 애니메이션 수명을 계산한다.
 */
namespace Pakuri.InGame
{
    internal static class EffectVisualUtility
    {
        private const float DefaultSingleAttackLineLength = 31f;

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
        public static float ResolveLifetime(
            GameObject instance,
            float minimumLifetimeSeconds,
            float fallbackLifetimeSeconds = 0f)
        {
            var minimum = Mathf.Max(0.01f, minimumLifetimeSeconds);
            var animationLength = ResolveAnimationLength(instance);
            var resolvedLength = animationLength > 0f
                ? animationLength
                : Mathf.Max(0f, fallbackLifetimeSeconds);
            return Mathf.Max(minimum, resolvedLength);
        }

        /*
         * 효과 오브젝트에 판정용 2D 충돌 영역이 있는지 확인한다.
         */
        public static bool HasHitbox(GameObject instance)
        {
            if (instance == null)
            {
                return false;
            }

            var colliders = instance.GetComponentsInChildren<Collider2D>();
            return colliders != null && colliders.Length > 0;
        }

        /*
         * 범위 효과의 기본 반경과 스킬 배율을 오브젝트 크기에 반영한다.
         */
        public static void ConfigureAreaScale(
            Transform transform,
            float baseRadius,
            SkillExecutionSnapshot snapshot,
            float radiusMultiplier)
        {
            if (transform == null)
            {
                return;
            }

            SkillExecutionUtility.ApplyPrefabScale(transform, baseRadius, snapshot);
            transform.localScale *= Mathf.Max(0f, radiusMultiplier);
        }

        /*
         * 단일 공격의 직선형 다중 배치 비주얼 방향과 길이를 설정한다.
         */
        public static void ConfigureSingleAttackLine(
            Transform transform,
            SkillExecutionContext context,
            SingleSkillRuntimeData skill,
            SkillExecutionSnapshot snapshot,
            Vector2 center)
        {
            if (transform == null || skill == null)
            {
                return;
            }

            var origin = context != null
                && context.CasterEntry != null
                && context.CasterEntry.Transform != null
                    ? (Vector2)context.CasterEntry.Transform.position
                    : center;
            var direction = center - origin;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = Vector2.right;
            }

            transform.position = center;
            transform.rotation = SkillExecutionUtility.ResolveRotation(direction.normalized);

            var width = SkillAreaUtility.ResolveRadius(
                SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area),
                snapshot);
            var spriteRenderer = transform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                var size = spriteRenderer.sprite.bounds.size;
                var scale = transform.localScale;
                if (size.x > 0.0001f)
                {
                    scale.x = Mathf.Sign(scale.x == 0f ? 1f : scale.x)
                        * (DefaultSingleAttackLineLength / size.x);
                }

                if (size.y > 0.0001f)
                {
                    scale.y = Mathf.Sign(scale.y == 0f ? 1f : scale.y)
                        * (width / size.y);
                }

                transform.localScale = scale;
                return;
            }

            SkillExecutionUtility.ApplyPrefabScale(
                transform,
                SkillAreaUtility.ResolveBaseRadius(skill.Targeting, skill.Area),
                snapshot);
        }

        /*
         * 단일 공격이 직선 형태 다중 배치 비주얼을 사용하는지 확인한다.
         */
        public static bool UsesSingleLineVisual(SingleSkillRuntimeData skill)
        {
            return skill != null
                && skill.UseMultiDeployment
                && string.IsNullOrWhiteSpace(skill.DeploymentRequiredTargetStatusId);
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
            if (instance == null)
            {
                return 0f;
            }

            var maxLength = 0f;
            var animators = instance.GetComponentsInChildren<Animator>(true);
            for (var i = 0; i < animators.Length; i++)
            {
                var controller = animators[i] != null
                    ? animators[i].runtimeAnimatorController
                    : null;
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
