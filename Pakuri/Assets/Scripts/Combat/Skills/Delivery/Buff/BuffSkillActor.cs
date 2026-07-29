using UnityEngine;

/*
 * 대상의 자식으로 연결된 강화, 보호막, 회복 스킬 비주얼의 수명을 관리
 */
namespace Pakuri.InGame
{
    public class BuffSkillActor : MonoBehaviour
    {
        // 대상 부착형 강화 시각 효과의 시간 제한을 구현한다.
        private EffectManager effectManager;
        private float remainingLifetime;
        private bool hasLifetime;

        /*
         * 대상에게 붙은 강화 비주얼이 지정한 시간 뒤 제거되도록 설정한다.
         */
        public void Initialize(
            EffectManager manager /* 효과 생성과 제거를 담당하는 관리자 */,
            float durationSeconds /* 지속 시간(초) */)
        {
            effectManager = manager;
            hasLifetime = durationSeconds > 0f;
            if (hasLifetime)
            {
                remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            }
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
         * 남은 수명을 갱신하고 종료 시 관리자에게 삭제를 요청한다.
         */
        private void Update()
        {
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
