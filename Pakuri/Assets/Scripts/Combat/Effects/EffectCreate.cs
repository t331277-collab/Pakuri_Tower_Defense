/*
 * 역할: 런타임 스킬 효과 생성 설정
 */

using System;
using Pakuri.Data;
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
        public float Alpha = 1f;
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

namespace Pakuri.InGame
{
    /// 런타임 효과 GameObject 생성에 필요한 외형·배치·수명 연결 정보를 묶는다.
    public readonly struct EffectCreateRequest
    {
        public EffectCreateRequest(
            RuntimeSkillVisualSpec visual,
            GameObject prefab,
            string objectName,
            Vector3 position,
            Quaternion rotation,
            Transform targetTransform,
            StatusRuntimeInstance persistentStatus,
            bool hitboxIsTrigger,
            bool includeHitbox,
            bool createEmptyActor)
        {
            Visual = visual;
            Prefab = prefab;
            ObjectName = objectName;
            Position = position;
            Rotation = rotation;
            TargetTransform = targetTransform;
            PersistentStatus = persistentStatus;
            HitboxIsTrigger = hitboxIsTrigger;
            IncludeHitbox = includeHitbox;
            CreateEmptyActor = createEmptyActor;
        }

        public RuntimeSkillVisualSpec Visual { get; }
        public GameObject Prefab { get; }
        public string ObjectName { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Transform TargetTransform { get; }
        public StatusRuntimeInstance PersistentStatus { get; }
        public bool HitboxIsTrigger { get; }
        public bool IncludeHitbox { get; }
        public bool CreateEmptyActor { get; }
    }
}
