/*
 * 역할: 단일 스킬 런타임 Actor 동작.
 * 책임: 단일 대상 전달 상태를 소유하고 설정된 적중 또는 이동 순서를 완료한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>SingleSkillActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
    public class SingleSkillActor : MonoBehaviour
    {

        private EffectManager effectManager;
        private Transform target;
        private Vector3 offset;
        private float remainingLifetime;
        private bool followsTarget;

        /// <summary>전달된 런타임 입력값을 사용해 <c>Timed</c>를 초기화한다.</summary>
        public void InitializeTimed(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            target = null;
            offset = Vector3.zero;
            followsTarget = false;
            remainingLifetime = Mathf.Max(0.01f, durationSeconds);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Animation</c>를 초기화한다.</summary>
        public float InitializeAnimation(
            EffectManager manager,
            float durationSeconds)
        {
            var lifetime = Mathf.Max(0.01f, durationSeconds);
            InitializeTimed(manager, lifetime);
            return lifetime;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>Following</c>를 초기화한다.</summary>
        public void InitializeFollowing(
            EffectManager manager,
            Transform followTarget,
            float durationSeconds,
            Vector3 localOffset)
        {
            effectManager = manager;
            target = followTarget;
            offset = localOffset;
            followsTarget = true;
            remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            transform.position = followTarget.position + offset;
        }

        /// <summary>전달된 <c>instance</c> 값을 사용해 <c>요청값</c>를 연결한다.</summary>
        public static SingleSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<SingleSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<SingleSkillActor>();
            }

            return actor;
        }

        /// <summary>현재 Unity 프레임에서 <c>Update</c> 갱신 동작을 진행한다.</summary>
        private void Update()
        {
            if (followsTarget)
            {
                if (target == null)
                {
                    effectManager.RemoveEffect(gameObject);
                    return;
                }

                transform.position = target.position + offset;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }
    }
}
