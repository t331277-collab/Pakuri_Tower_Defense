using Pakuri.Data;
using UnityEngine;

/*
 * EffectManager가 생성한 런타임 효과에 스프라이트, 애니메이터, 크기와 충돌 영역을 적용한다.
 */
namespace Pakuri.InGame
{
    /*
     * 스킬 시각 오브젝트를 런타임 설정에 맞게 구성한다.
     */
    static class EffectVisualBuilder
    {
        /*
         * 진행 방향을 2D 회전값으로 바꾼다.
         * 직선·투사체·트리거 효과가 함께 사용한다.
         */
        public static Quaternion Rotation(Vector2 direction /* 진행하거나 발사할 방향 */)
        {
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return Quaternion.identity; // 회전 없음
            }

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // 각도 계산
            return Quaternion.Euler(0f, 0f, angle); // 계산 된 값으로 발사 방향으로 각도 회전
        }

        /*
         * 런타임 시각 설정에 따라 크기, 스프라이트, 애니메이터와 충돌 영역을 구성한다.
         */
        public static void Configure(
            GameObject instance /* 생성된 게임 오브젝트 */,
            RuntimeSkillVisualSpec visual /* 런타임 시각 효과 설정 */,
            bool hitboxIsTrigger /* 피격 판정 여부 트리거 여부 */,
            bool includeHitbox /* 포함 피격 판정 여부 */)
        {
            instance.transform.localScale = visual.LocalScale;

            if (visual.Sprite != null) // 스프라이트 있을때
            {
                var renderer = instance.AddComponent<SpriteRenderer>();
                renderer.sprite = visual.Sprite;
                renderer.sortingOrder = visual.SortingOrder;
            }

            if (visual.AnimatorController != null) // 애니메이션이 있을때
            {
                var animator = instance.AddComponent<Animator>();
                animator.runtimeAnimatorController = visual.AnimatorController;
            }

            if (includeHitbox && visual.Hitbox != null && visual.Hitbox.HasHitbox()) //충돌 판정
            {
                ConfigureHitbox(instance, visual.Hitbox, hitboxIsTrigger);
            }
        }

        /*
         * 범위 반경과 강화값으로 효과 크기를 적용하고 물리 변환을 갱신한다.
         */
        public static void ConfigureAreaEffect(
            GameObject instance /* 생성된 효과 오브젝트 */,
            float baseRadius /* 기본 반지름 */,
            float skillRadiusMultiplier /* 스킬 강화 반지름 배율 */,
            float skillRadiusBonus /* 스킬 강화 추가 반지름 */,
            float radiusMultiplier = 1f /* 반지름 배율 */)
        {
            var scaleFactor = SkillTargeting.PrefabScaleFactor(
                baseRadius,
                skillRadiusMultiplier,
                skillRadiusBonus);
            instance.transform.localScale *= scaleFactor;
            instance.transform.localScale *= Mathf.Max(0f, radiusMultiplier);
            Physics2D.SyncTransforms();
        }

        /*
         * 콜라이더 컴포넌트 적용 및 초기 설정
         */
        private static void ConfigureHitbox(
            GameObject instance /* 생성된 게임 오브젝트 */,
            RuntimeSkillHitboxSpec hitbox /* 피격 판정 */,
            bool hitboxIsTrigger /* 피격 판정 여부 트리거 여부 */)
        {
            var collider = instance.AddComponent<BoxCollider2D>();
            collider.size = hitbox.Size;
            collider.isTrigger = hitboxIsTrigger;
        }

    }
}
