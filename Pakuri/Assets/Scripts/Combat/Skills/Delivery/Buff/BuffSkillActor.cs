/*
 * 역할: 런타임 버프 비주얼 소유.
 * 책임: 버프 효과를 대상에 부착하고 소유 상태 효과가 끝나면 제거한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>BuffSkillActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
    public class BuffSkillActor : MonoBehaviour
    {

        private EffectManager effectManager;
        private float remainingLifetime;
        private bool hasLifetime;

        /// <summary>전달된 런타임 입력값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
        public void Initialize(
            EffectManager manager,
            float durationSeconds)
        {
            effectManager = manager;
            hasLifetime = durationSeconds > 0f;
            if (hasLifetime)
            {
                remainingLifetime = Mathf.Max(0.01f, durationSeconds);
            }
        }

        /// <summary>전달된 <c>instance</c> 값을 사용해 <c>요청값</c>를 연결한다.</summary>
        public static BuffSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<BuffSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<BuffSkillActor>();
            }

            return actor;
        }

        /// <summary>현재 Unity 프레임에서 <c>Update</c> 갱신 동작을 진행한다.</summary>
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
