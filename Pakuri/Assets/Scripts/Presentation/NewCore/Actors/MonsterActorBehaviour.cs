using Pakuri.NewCore.Units.Models;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class MonsterActorBehaviour : UnitActorBehaviour
    {
        [SerializeField] private MonsterAnimationBehaviour animationController;

        private bool defeated;

        public MonsterModel Monster => Model as MonsterModel;

        public override void SyncFromModel()
        {
            base.SyncFromModel();
            if (Model == null)
            {
                return;
            }

            if (!Model.IsAlive && !defeated)
            {
                defeated = true;
                SetColliders(false);
                ResolveAnimation()?.PlayDeath();
            }
            else if (Model.IsAlive && defeated)
            {
                defeated = false;
                SetColliders(true);
                ResolveAnimation()?.ReviveToIdle();
            }
        }

        public void PlayAttack()
        {
            if (!defeated)
            {
                ResolveAnimation()?.PlayRandomAttack();
            }
        }

        public void PlayHit()
        {
            if (!defeated)
            {
                ResolveAnimation()?.PlayHit();
            }
        }

        private MonsterAnimationBehaviour ResolveAnimation()
        {
            if (animationController == null)
            {
                animationController =
                    GetComponent<MonsterAnimationBehaviour>();
            }

            return animationController;
        }

        private void SetColliders(bool enabled)
        {
            var colliders = GetComponentsInChildren<Collider2D>(true);
            for (var index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = enabled;
            }
        }
    }
}
