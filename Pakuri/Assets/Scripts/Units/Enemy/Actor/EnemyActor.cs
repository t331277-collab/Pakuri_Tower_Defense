/*
 * 역할: 적 씬과 모델 연결.
 * 책임: 적 전투 모델을 월드 표시에 연결하고 피해 표시와 갱신 기능을 제공한다.
 */

using UnityEngine;

namespace Pakuri.InGame
{

    /// EnemyActor 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.
    public class EnemyActor : MonoBehaviour
    {
        private UnitWorldDisplay display;

        public EnemyCombatState Model { get; private set; }

        /// 전달된 model 값을 사용해 소유한 런타임 상태를 초기화한다.
        public void Initialize(EnemyCombatState model)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
            RefreshDisplay();
        }

        /// 전달된 damageAmount 값을 사용해 Damage를 표시한다.
        public void ShowDamage(float damageAmount)
        {
            display.ShowDamage(damageAmount);
        }

        /// Display를 현재 런타임 모델을 기준으로 갱신한다.
        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }
    }
}
