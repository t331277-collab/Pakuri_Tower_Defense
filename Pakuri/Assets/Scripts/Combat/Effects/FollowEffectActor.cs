using UnityEngine;

/*
 * 생성된 효과가 지정한 대상을 따라가도록 위치와 수명을 관리한다.
 */
namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class FollowEffectActor : MonoBehaviour
    {
        private Transform target;
        private Vector3 offset;
        private float lifetime;

        /*
         * 따라갈 대상과 유지 시간, 위치 보정값을 설정한다.
         */
        public void Initialize(Transform followTarget, float durationSeconds, Vector3 localOffset)
        {
            target = followTarget;
            lifetime = Mathf.Max(0.1f, durationSeconds);
            offset = localOffset;
            FollowTarget();
        }

        /*
         * 효과 위치와 남은 수명을 매 프레임 갱신한다.
         */
        private void Update()
        {
            lifetime -= Time.deltaTime;
            if (target == null || lifetime <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            FollowTarget();
        }

        /*
         * 대상을 대상의 위치를 따라가도록 갱신한다.
         */
        private void FollowTarget()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }
    }
}
