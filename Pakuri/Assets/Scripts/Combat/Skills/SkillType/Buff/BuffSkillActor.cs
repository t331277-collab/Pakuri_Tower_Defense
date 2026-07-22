using UnityEngine;

/*
 * 강화, 보호막, 회복 스킬 비주얼의 대상 추적과 수명을 관리한다.
 * 효과가 끝나면 직접 삭제하지 않고 EffectManager에 제거를 요청한다.
 */
namespace Pakuri.InGame
{
    public class BuffSkillActor : MonoBehaviour
    {
        private EffectManager effectManager;
        private Transform target;
        private Vector3 offset;
        private float remainingLifetime;
        private bool hasLifetime;

        /*
         * Buff 비주얼이 대상을 따라가다가 지정한 시간 뒤 제거되도록 설정한다.
         */
        public void Initialize(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            Transform followTarget /* 효과가 따라갈 대상 */,
            float durationSeconds /* 지속 시간(초) */,
            Vector3 localOffset /* 위치 보정 */)
        {
            effectManager = manager;
            target = followTarget;
            offset = localOffset;
            hasLifetime = durationSeconds > 0f;
            if (hasLifetime)
            {
                remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            }

            transform.position = followTarget.position + offset;
        }

        /*
         * Buff 효과 오브젝트에 Actor가 없으면 추가해 반환한다.
         */
        public static BuffSkillActor Attach(GameObject instance /* Buff 효과 오브젝트 */)
        {
            var actor = instance.GetComponent<BuffSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<BuffSkillActor>();
            }

            return actor;
        }

        /*
         * 대상 위치와 남은 수명을 갱신하고 종료 시 관리자에게 삭제를 요청한다.
         */
        private void Update()
        {
            if (target == null)
            {
                effectManager.RemoveEffect(gameObject);
                return;
            }

            transform.position = target.position + offset;
            if (!hasLifetime)
            {
                return;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                effectManager.RemoveEffect(gameObject);
            }
        }
    }
}
