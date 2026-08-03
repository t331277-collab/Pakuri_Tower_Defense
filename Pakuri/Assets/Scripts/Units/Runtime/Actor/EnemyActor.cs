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
        private UnitHpBar display;

        public EnemyCombatState Model { get; private set; }

        public void Initialize(EnemyCombatState model)
        {
            Model = model;
            display = new UnitHpBar(this);
            RefreshDisplay();
        }

        public void ShowDamage(float damageAmount)
        {
            display.ShowDamage(damageAmount);
        }

        internal void SetWorldHpBarVisible(bool visible)
        {
            display?.SetWorldHpBarVisible(visible);
        }

        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }
    }
}
