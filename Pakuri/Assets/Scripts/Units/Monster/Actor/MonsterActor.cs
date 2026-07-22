using UnityEngine;

/*
 * 아군 Monster GameObject와 전투 상태, 전투 애니메이션을 연결한다.
 * 공통 월드 표시는 UnitWorldDisplay에 맡긴다.
 */
namespace Pakuri.InGame
{
    public class MonsterActor : MonoBehaviour
    {
        [SerializeField] private AnimationController animationController;

        private UnitWorldDisplay display;
        private bool defeated;

        public UnitCombatState Model { get; private set; }
        public bool IsDefeated => defeated;

        /*
         * Initialize에 필요한 값을 설정한다.
         */
        public void Initialize(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
            ResolveAnimationController();
            RefreshDisplay();
        }

        /*
         * ShowDamage 작업을 수행한다.
         */
        public void ShowDamage(float damageAmount /* 표시하거나 적용할 피해량 */)
        {
            display.ShowDamage(damageAmount);
        }

        /*
         * TryPlayActiveSkillAnimation 작업을 시도하고 성공 여부를 반환한다.
         */
        public void TryPlayActiveSkillAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayRandomAttack();
        }

        /*
         * TryPlayHitAnimation 작업을 시도하고 성공 여부를 반환한다.
         */
        public void TryPlayHitAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayHit();
        }

        /*
         * Defeat 작업을 수행한다.
         */
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

        /*
         * Revive 작업을 수행한다.
         */
        public void Revive()
        {
            defeated = false;
            SetTargetCollidersEnabled(true);
            ResolveAnimationController()?.ReviveToIdle();
            RefreshDisplay();
        }

        /*
         * RefreshDisplay 대상의 현재 상태를 갱신한다.
         */
        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }

        /*
         * ResolveAnimationController 결과를 계산해 반환한다.
         */
        private AnimationController ResolveAnimationController()
        {
            if (animationController == null)
            {
                animationController = GetComponent<AnimationController>();
            }

            return animationController;
        }

        /*
         * SetTargetCollidersEnabled에 필요한 값을 설정한다.
         */
        private void SetTargetCollidersEnabled(bool enabled /* 기능 활성화 여부 */)
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
