/*
 * 역할: 시간 또는 상태 수명에 맞춰 효과 오브젝트를 정리한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// 적용된 지원 효과가 보이는 기간을 실제 효과 수명과 맞춘다.
    public class BuffSkillActor : MonoBehaviour
    {

        private EffectManager effectManager;
        private StatusRuntimeInstance persistentStatus;
        private float remainingLifetime;
        private bool hasLifetime;

        /// 정해진 시간이 지나면 사라지는 표현을 시작한다.
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

        /// 전투 상태가 끝날 때까지 표현의 생존 기준을 연결한다.
        public void InitializePersistent(
            EffectManager manager,
            StatusRuntimeInstance status)
        {
            effectManager = manager;
            persistentStatus = status;
            hasLifetime = false;
            remainingLifetime = 0f;
        }

        /// 표현이 맡은 수명이 끝났음을 효과 관리자에 알린다.
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

        /// BuffSkillActor 컴포넌트 부착
        public static BuffSkillActor Attach(GameObject instance)
        {
            var actor = instance.GetComponent<BuffSkillActor>();
            if (actor == null)
            {
                actor = instance.AddComponent<BuffSkillActor>();
            }

            return actor;
        }

        /// 시간형 표현이 만료되는 시점을 진행한다.
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
