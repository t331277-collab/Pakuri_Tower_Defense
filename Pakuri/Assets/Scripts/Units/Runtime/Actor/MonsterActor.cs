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

        private UnitHpBar display;
        private bool defeated;

        public UnitCombatState Model { get; private set; }
        public bool IsDefeated => defeated;

        public void Initialize(UnitCombatState model)
        {
            Model = model;
            display = new UnitHpBar(this);
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
