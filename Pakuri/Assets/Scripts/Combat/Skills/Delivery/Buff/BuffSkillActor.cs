/*
 * 역할: 런타임 버프 비주얼 소유.
 * 책임: 버프 효과를 대상에 부착하고 소유 상태 효과가 끝나면 제거한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// BuffSkillActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public class BuffSkillActor : MonoBehaviour
    {

        private EffectManager effectManager;
        private StatusRuntimeInstance persistentStatus;
        private float remainingLifetime;
        private bool hasLifetime;

        /// 전달된 시간 동안 유지되는 버프 비주얼을 초기화한다.
        public void InitializeTimed(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            persistentStatus = null;
            hasLifetime = durationSeconds > 0f;
            if (hasLifetime)
            {
                remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            }
        }

        /// 상태 런타임과 함께 유지되는 버프 비주얼을 초기화한다.
        public void InitializePersistent(
            EffectManager manager,
            StatusRuntimeInstance status)
        {
            effectManager = manager;
            persistentStatus = status;
            hasLifetime = false;
            remainingLifetime = 0f;
        }

        /// 버프 비주얼 수명을 종료하고 EffectManager에 삭제를 요청한다.
        public void Complete()
        {
            if (effectManager == null)
            {
                return;
            }

            var manager = effectManager;
            effectManager = null;
            manager.RemoveEffect(gameObject, persistentStatus);
        }

        /// 전달된 instance 값을 사용해 요청값를 연결한다.
        public static BuffSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<BuffSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<BuffSkillActor>();
            }

            return actor;
        }

        /// 현재 Unity 프레임에서 Update 갱신 동작을 진행한다.
        private void Update()
        {
            if (!hasLifetime)
            {
                return;
            }

            remainingLifetime -= Time.deltaTime;
            if (remainingLifetime <= 0f)
            {
                Complete();
            }
        }
    }
}
