using System;
using Pakuri.Combat;
using UnityEngine;

/*
 * 추가 효과의 시각 오브젝트가 붙을 위치를 구분한다.
 */
namespace Pakuri.Data
{
    public enum SkillMultiEffectVisualAnchorMode
    {
        Center,
        AppliedTargets
    }

    /*
     * 스킬 시각 오브젝트가 스킬과 상태 중 어디에 속하는지 구분한다.
     */
    public enum RuntimeSkillVisualAnchor
    {
        Skill,
        StatusTarget
    }

    /*
     * 런타임 스킬 충돌 영역의 크기와 중심 보정값을 보관한다.
     */
    [Serializable]
    public sealed class RuntimeSkillHitboxSpec
    {
        public Vector2 Size;
        public Vector2 Offset;

        /*
         * 너비와 높이가 모두 설정됐는지 확인한다.
         */
        public bool HasHitbox()
        {
            return Size.x > 0f && Size.y > 0f;
        }
    }

    /*
     * 런타임에서 조합할 스프라이트, 애니메이터, 크기, 충돌 영역을 보관한다.
     */
    [Serializable]
    public sealed class RuntimeSkillVisualSpec
    {
        public Sprite Sprite;
        public RuntimeAnimatorController AnimatorController;
        public float Scale = 1f;
        public bool UseLocalScale;
        public Vector3 LocalScale = Vector3.one;
        public int SortingOrder;
        public RuntimeSkillVisualAnchor Anchor = RuntimeSkillVisualAnchor.Skill;
        public RuntimeSkillHitboxSpec Hitbox = new RuntimeSkillHitboxSpec();

        /*
         * 화면 표시나 충돌 영역으로 사용할 데이터가 있는지 확인한다.
         */
        public bool HasVisual()
        {
            return Sprite != null
                || AnimatorController != null
                || (Hitbox != null && Hitbox.HasHitbox());
        }
    }
}
