/*
 * 역할: 런타임 스킬 효과 설정 계약.
 * 책임: 런타임 스킬 비주얼의 앵커·렌더링·히트박스 설정을 정의한다.
 */

using System;
using UnityEngine;

namespace Pakuri.Data
{
    public enum RuntimeSkillVisualAnchor
    {
        Skill,
        StatusTarget
    }

    [Serializable]
    public class RuntimeSkillHitboxSpec
    {
        public Vector2 Size;

        public bool HasHitbox()
        {
            return Size.x > 0f && Size.y > 0f;
        }
    }

    [Serializable]
    public class RuntimeSkillVisualSpec
    {
        public Sprite Sprite;
        public RuntimeAnimatorController AnimatorController;
        public Vector3 LocalScale = Vector3.one;
        public int SortingOrder;
        public RuntimeSkillVisualAnchor Anchor = RuntimeSkillVisualAnchor.Skill;
        public RuntimeSkillHitboxSpec Hitbox = new RuntimeSkillHitboxSpec();

        public bool HasVisual()
        {
            return Sprite != null
                || AnimatorController != null
                || (Hitbox != null && Hitbox.HasHitbox());
        }
    }
}
