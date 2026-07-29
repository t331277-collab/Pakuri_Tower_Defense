/*
 * 역할: 플레이어 몬스터 씬과 모델 연결.
 * 책임: 전투 모델을 월드 표시·애니메이션·패배·부활·대상 Collider 상태에 연결한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>MonsterActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
    public class MonsterActor : MonoBehaviour
    {
        [SerializeField] private AnimationController animationController;

        private UnitWorldDisplay display;
        private bool defeated;

        public UnitCombatState Model { get; private set; }
        public bool IsDefeated => defeated;

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
        public void Initialize(UnitCombatState model)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
            ResolveAnimationController();
            RefreshDisplay();
        }

        /// <summary>전달된 <c>damageAmount</c> 값을 사용해 <c>Damage</c>를 표시한다.</summary>
        public void ShowDamage(float damageAmount)
        {
            display.ShowDamage(damageAmount);
        }

        /// <summary><c>PlayActiveSkillAnimation</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        public void TryPlayActiveSkillAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayRandomAttack();
        }

        /// <summary><c>PlayHitAnimation</c> 작업을 시도하고 성공 여부를 반환한다.</summary>
        public void TryPlayHitAnimation()
        {
            if (defeated)
            {
                return;
            }

            ResolveAnimationController()?.PlayHit();
        }

        /// <summary><c>Defeat</c> 작업을 수행한다.</summary>
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

        /// <summary><c>Revive</c> 작업을 수행한다.</summary>
        public void Revive()
        {
            defeated = false;
            SetTargetCollidersEnabled(true);
            ResolveAnimationController()?.ReviveToIdle();
            RefreshDisplay();
        }

        /// <summary><c>Display</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }

        /// <summary><c>AnimationController</c>를 결정한다.</summary>
        private AnimationController ResolveAnimationController()
        {
            if (animationController == null)
            {
                animationController = GetComponent<AnimationController>();
            }

            return animationController;
        }

        /// <summary>전달된 <c>enabled</c> 값을 사용해 <c>TargetCollidersEnabled</c>를 갱신한다.</summary>
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
