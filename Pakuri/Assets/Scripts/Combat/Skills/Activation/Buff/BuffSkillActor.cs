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

        /// 시간형 버프 비주얼의 수명을 시작한다.
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

        /// 상태가 끝날 때까지 버프 비주얼을 유지한다.
        public void InitializePersistent(
            EffectManager manager,
            StatusRuntimeInstance status)
        {
            effectManager = manager;
            persistentStatus = status;
            hasLifetime = false;
            remainingLifetime = 0f;
        }

        /// 버프 비주얼의 수명을 끝낸다.
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

        /// 효과 객체에 버프 실행 컴포넌트를 연결한다.
        public static BuffSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<BuffSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<BuffSkillActor>();
            }

            return actor;
        }

        /// 프레임 경과에 따라 버프 비주얼 수명을 갱신한다.
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
