using UnityEngine;

/*
 * 단일 공격으로 생성된 비주얼의 수명과 대상 추적을 관리한다.
 * 효과가 끝나면 직접 삭제하지 않고 EffectManager에 제거를 요청한다.
 */
namespace Pakuri.InGame
{
    public class SingleSkillActor : MonoBehaviour
    {
        // 단일 공격 시각 효과의 대상 추적과 수명 종료를 구현.
        private EffectManager effectManager;
        private Transform target;
        private Vector3 offset;
        private float remainingLifetime;
        private bool followsTarget;

        /*
         * 단일 공격 비주얼이 지정한 시간 뒤 제거되도록 설정한다.
         */
        public void InitializeTimed(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            float durationSeconds /* 지속 시간(초) */)
        {
            effectManager = manager;
            target = null;
            offset = Vector3.zero;
            followsTarget = false;
            remainingLifetime = Mathf.Max(0.01f, durationSeconds);
        }

        /*
         * 단일 공격 비주얼이 애니메이션 종료 뒤 제거되도록 설정하고 수명을 반환한다.
         */
        public float InitializeAnimation(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            float minimumLifetimeSeconds /* 최소 유지 시간(초) */)
        {
            var lifetime = EffectVisualBuilder.ResolveLifetime(gameObject, minimumLifetimeSeconds);
            InitializeTimed(manager, lifetime);
            return lifetime;
        }

        /*
         * 단일 공격 비주얼이 대상을 따라가다가 지정한 시간 뒤 제거되도록 설정한다.
         */
        public void InitializeFollowing(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            Transform followTarget /* 효과가 따라갈 대상 */,
            float durationSeconds /* 지속 시간(초) */,
            Vector3 localOffset /* 위치 보정 */)
        {
            effectManager = manager;
            target = followTarget;
            offset = localOffset;
            followsTarget = true;
            remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            transform.position = followTarget.position + offset;
        }

        /*
         * 단일 공격 효과 오브젝트에 Actor가 없으면 추가해 반환한다.
         */
        public static SingleSkillActor Attach(GameObject instance /* 단일 공격 효과 오브젝트 */)
        {
            var actor = instance.GetComponent<SingleSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<SingleSkillActor>();
            }

            return actor;
        }

        /*
         * 대상 위치와 남은 수명을 갱신하고 종료 시 관리자에게 삭제를 요청한다.
         */
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
