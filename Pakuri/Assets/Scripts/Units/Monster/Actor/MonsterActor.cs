using UnityEngine;

/*
 * 아군 Monster GameObject와 전투 상태, 전투 애니메이션을 연결한다.
 * 공통 월드 표시는 UnitWorldDisplay에 맡긴다.
 */
namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class MonsterActor : MonoBehaviour
    {
        [SerializeField] private AnimationController animationController;

        private UnitWorldDisplay display;
        private bool defeated;

        public MonsterCombatState Model { get; private set; }
        public bool IsDefeated => defeated;

        public void Initialize(MonsterCombatState model)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
            ResolveAnimationController();
            RefreshDisplay();
        }

        public void ShowDamage(float damageAmount)
        {
            display.ShowDamage(damageAmount);
        }

        public void TryPlayActiveSkillAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayRandomAttack();
        }

        public void TryPlayHitAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayHit();
        }

        public void Defeat()
        {
            if (defeated)
            {
                return;
            }

            defeated = true;
            SetTargetCollidersEnabled(false);
            ResolveAnimationController()?.PlayDeath();
        }

        public void Revive()
        {
            defeated = false;
            SetTargetCollidersEnabled(true);
            ResolveAnimationController()?.ReviveToIdle();
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }

        private AnimationController ResolveAnimationController()
        {
            if (animationController == null)
            {
                animationController = GetComponent<AnimationController>();
            }

            return animationController;
        }

        private void SetTargetCollidersEnabled(bool enabled)
        {
            var colliders = GetComponentsInChildren<Collider2D>();
            for (var i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = enabled;
                }
            }
        }
    }
}
