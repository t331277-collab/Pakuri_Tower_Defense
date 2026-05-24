using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameAttachedSkillEffectActor : MonoBehaviour
    {
        private Transform target;
        private Vector3 offset;
        private float lifetime;

        public void Initialize(Transform followTarget, float durationSeconds, Vector3 localOffset)
        {
            target = followTarget;
            lifetime = Mathf.Max(0.1f, durationSeconds);
            offset = localOffset;
            FollowTarget();
        }

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

        private void FollowTarget()
        {
            if (target != null)
            {
                transform.position = target.position + offset;
            }
        }
    }
}
