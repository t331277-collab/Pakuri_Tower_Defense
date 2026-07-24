using UnityEngine;
using Pakuri.NewCore.Units.Models;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class EnemyActorBehaviour : UnitActorBehaviour
    {
        private const float DefeatVisualSeconds = 0.95f;
        private bool defeated;

        public EnemyModel Enemy => Model as EnemyModel;

        public bool IsDefeated => defeated;

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
    }
}
