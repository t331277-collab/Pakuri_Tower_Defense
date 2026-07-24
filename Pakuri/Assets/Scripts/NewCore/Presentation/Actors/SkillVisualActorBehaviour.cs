using Pakuri.NewCore.Combat.Effects;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class SkillVisualActorBehaviour : MonoBehaviour
    {
        private EffectHandle handle;

        public void Bind(EffectHandle effectHandle)
        {
            handle = effectHandle
                ?? throw new System.ArgumentNullException(nameof(effectHandle));
            Sync();
        }

        public void Sync()
        {
            if (handle == null)
            {
                return;
            }

            transform.position = new Vector3(
                handle.Position.X,
                handle.Position.Y,
                transform.position.z);
            if (handle.Direction.SqrMagnitude > 0.0001f)
            {
                transform.right = new Vector3(
                    handle.Direction.X,
                    handle.Direction.Y,
                    0f);
            }
        }
    }
}
