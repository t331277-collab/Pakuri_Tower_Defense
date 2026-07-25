using Pakuri.NewCore.Units.Models;
using UnityEngine;

/* Enemy Model의 scene 결합과 공격·Nexus 접촉·패배 표현 생명주기를 소유한다. */
namespace Pakuri.NewCore.Units.Actors
{
    public sealed class EnemyActor : UnitActor
    {
        private const float DefeatVisualSeconds = 0.95f;
        private static readonly int Attack =
            Animator.StringToHash("Attack");
        private bool defeated;
        private Animator animator;

        public EnemyModel Enemy => Model as EnemyModel;

        public bool IsDefeated => defeated;

        public int AttackPresentationCount { get; private set; }

        /* 자식 hierarchy의 선택 Animator를 scene 표현 경계로 연결한다. */
        private void Awake()
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        /* 생존 Enemy의 선택 공격 trigger와 관측 횟수를 갱신한다. */
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

        /* 패배 또는 Nexus 접촉을 collider 비활성화와 오브젝트 종료에 반영한다. */
        public override void SyncFromModel()
        {
            base.SyncFromModel();
            EnemyModel enemy = Enemy;
            bool reachedNexus =
                enemy != null && enemy.HasContactedNexus;
            if (Model == null
                || (Model.IsAlive && !reachedNexus)
                || defeated)
            {
                return;
            }

            defeated = true;
            Collider2D[] colliders =
                GetComponentsInChildren<Collider2D>(true);
            for (int index = 0; index < colliders.Length; index++)
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

        /* Animator가 실제 Attack trigger parameter를 가지는지 검사한다. */
        private static bool HasAttackTrigger(Animator target)
        {
            AnimatorControllerParameter[] parameters = target.parameters;
            for (int index = 0; index < parameters.Length; index++)
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
