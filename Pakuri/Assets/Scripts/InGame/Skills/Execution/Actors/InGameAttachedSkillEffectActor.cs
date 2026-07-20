using UnityEngine;

namespace Pakuri.InGame
{
    /*
     * 인게임 부착 효과 스킬 효과의 위치, 충돌, 수명 주기를 처리한다.
     */
    [DisallowMultipleComponent]
    public sealed class InGameAttachedSkillEffectActor : MonoBehaviour
    {
        private Transform target;
        private Vector3 offset;
        private float lifetime;

        /*
         * 인게임 부착 효과 스킬 효과 실행에 필요한 위치, 대상, 피해 정보를 설정한다.
         */
        public void Initialize(Transform followTarget, float durationSeconds, Vector3 localOffset)
        {
            target = followTarget;
            lifetime = Mathf.Max(0.1f, durationSeconds);
            offset = localOffset;
            FollowTarget();
        }

        /*
         * 인게임 부착 효과 스킬 효과의 이동, 수명, 주기 처리를 매 프레임 갱신한다.
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
