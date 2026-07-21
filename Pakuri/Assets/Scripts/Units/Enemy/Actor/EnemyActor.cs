using UnityEngine;

/*
 * 적 GameObject와 전투 상태를 연결한다.
 * 공통 월드 표시는 UnitWorldDisplay에 맡긴다.
 */
namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class EnemyActor : MonoBehaviour
    {
        private UnitWorldDisplay display;

        public EnemyCombatState Model { get; private set; }

        public void Initialize(EnemyCombatState model)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
            RefreshDisplay();
        }

        public void ShowDamage(float damageAmount)
        {
            display.ShowDamage(damageAmount);
        }

        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }
    }
}
