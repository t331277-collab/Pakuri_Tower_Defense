using TMPro;
using UnityEngine;

/*
 * Nexus GameObject와 전투 상태를 연결하고 체력 표시를 갱신한다.
 * 모델 생성, 패배 판정, UI 문자열 작성은 각각 외부 책임으로 분리한다.
 */
namespace Pakuri.InGame
{
    public class NexusActor : MonoBehaviour
    {
        private const float DefaultMaxHealth = 20f;

        [SerializeField] private float maxHealth = DefaultMaxHealth;
        [SerializeField] private TextMeshProUGUI nexusHpInfo;

        public float MaxHealth => maxHealth;
        public UnitCombatState Model { get; private set; }

        /*
         * Initialize에 필요한 값을 설정한다.
         */
        public void Initialize(UnitCombatState model)
        {
            Model = model;
            GetComponent<BoxCollider2D>().isTrigger = true;
            RefreshDisplay();
        }

        /*
         * RefreshDisplay 대상의 현재 상태를 갱신한다.
         */
        public void RefreshDisplay()
        {
            NexusHealthDisplay.Refresh(nexusHpInfo, Model);
        }

        /*
         * SetCurrentHealth에 필요한 값을 설정한다.
         */
        public void SetCurrentHealth(float currentHealth)
        {
            Model.Resources.CurrentHealth = Mathf.Clamp(
                Mathf.Round(currentHealth),
                0f,
                Mathf.Max(1f, Model.Stats.MaxHealth));
            RefreshDisplay();
        }
    }
}
