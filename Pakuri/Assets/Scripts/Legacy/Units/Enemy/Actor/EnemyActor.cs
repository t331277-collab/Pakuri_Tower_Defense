using UnityEngine;

/*
 * 적 GameObject와 전투 상태를 연결한다.
 * 공통 월드 표시는 UnitWorldDisplay에 맡긴다.
 */
namespace Pakuri.InGame
{
    public class EnemyActor : MonoBehaviour
    {
        private UnitWorldDisplay display;

        public EnemyCombatState Model { get; private set; }

        /*
         * Initialize에 필요한 값을 설정한다.
         */
        public void Initialize(EnemyCombatState model /* 처리할 상태 모델 */)
        {
            Model = model;
            display = new UnitWorldDisplay(this);
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
         * RefreshDisplay 대상의 현재 상태를 갱신한다.
         */
        public void RefreshDisplay()
        {
            display.Refresh(Model);
        }
    }
}
