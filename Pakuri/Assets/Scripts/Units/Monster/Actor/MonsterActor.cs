/*
 * 역할: 플레이어 몬스터 씬과 모델 연결.
 * 책임: 전투 모델을 월드 표시·애니메이션·패배·부활·대상 Collider 상태에 연결한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// MonsterActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public class MonsterActor : MonoBehaviour
    {
        [SerializeField] private AnimationController animationController;

        private UnitWorldDisplay display;
        private bool defeated;

        public UnitCombatState Model { get; private set; }
        public bool IsDefeated => defeated;

        /// 전달된 model 값을 사용해 소유한 런타임 상태를 초기화한다.
        public void Initialize(UnitCombatState model)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
            ResolveAnimationController();
            RefreshDisplay();
        }

        /// 전달된 damageAmount 값을 사용해 Damage를 표시한다.
        public void ShowDamage(float damageAmount)
        {
            display.ShowDamage(damageAmount);
        }

        /// PlayActiveSkillAnimation 작업을 시도하고 성공 여부를 반환한다.
        public void TryPlayActiveSkillAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayRandomAttack();
        }

        /// PlayHitAnimation 작업을 시도하고 성공 여부를 반환한다.
        public void TryPlayHitAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayHit();
        }

        /// Defeat 작업을 수행한다.
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

        /// Revive 작업을 수행한다.
        public void Revive()
        {
            defeated = false;
            SetTargetCollidersEnabled(true);
            ResolveAnimationController()?.ReviveToIdle();
            RefreshDisplay();
        }

        /// Display를 현재 런타임 모델을 기준으로 갱신한다.
        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }

        /// AnimationController를 결정한다.
        private AnimationController ResolveAnimationController()
        {
            if (animationController == null)
            {
                animationController = GetComponent<AnimationController>();
            }

            return animationController;
        }

        /// 전달된 enabled 값을 사용해 TargetCollidersEnabled를 갱신한다.
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
