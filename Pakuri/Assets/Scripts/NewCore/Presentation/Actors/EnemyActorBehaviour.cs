using UnityEngine;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class EnemyActorBehaviour : UnitActorBehaviour
    {
        private const float DefeatVisualSeconds = 0.95f;
        private static readonly int Attack =
            Animator.StringToHash("Attack");
        private bool defeated;
        private Animator animator;

        public EnemyModel Enemy => Model as EnemyModel;

        public bool IsDefeated => defeated;

        public int AttackPresentationCount { get; private set; }

        private void Awake()
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        public void PlayAttack()
        {
            if (defeated || Model == null || !Model.IsAlive)
            {
                return;
            }

            AttackPresentationCount++;
            if (animator != null && HasAttackTrigger(animator))
            {
                animator.SetTrigger(Attack);
            }
        }

        public override void SyncFromModel()
        {
            base.SyncFromModel();
            var enemy = Enemy;
            var reachedNexus =
                enemy != null && enemy.HasContactedNexus;
            if (Model == null
                || (Model.IsAlive && !reachedNexus)
                || defeated)
            {
                return;
            }

            defeated = true;
            var colliders = GetComponentsInChildren<Collider2D>(true);
            for (var index = 0; index < colliders.Length; index++)
            {
                colliders[index].enabled = false;
            }

            if (Application.isPlaying)
            {
                if (reachedNexus)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(gameObject, DefeatVisualSeconds);
                }
            }
        }

        private static bool HasAttackTrigger(Animator target)
        {
            var parameters = target.parameters;
            for (var index = 0; index < parameters.Length; index++)
            {
                if (parameters[index].nameHash == Attack
                    && parameters[index].type
                        == AnimatorControllerParameterType.Trigger)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
