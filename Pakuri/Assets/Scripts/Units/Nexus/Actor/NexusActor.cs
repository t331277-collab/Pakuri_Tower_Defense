using TMPro;
using UnityEngine;

/*
 * Nexus GameObject와 전투 상태를 연결하고 체력 표시를 갱신한다.
 * 모델 생성, 패배 판정, UI 문자열 작성은 각각 외부 책임으로 분리한다.
 */
namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class NexusActor : MonoBehaviour
    {
        private const float DefaultMaxHealth = 20f;

        [SerializeField] private float maxHealth = DefaultMaxHealth;
        [SerializeField] private TextMeshProUGUI nexusHpInfo;

        public float MaxHealth => maxHealth;
        public NexusCombatState Model { get; private set; }

        public void Initialize(NexusCombatState model)
        {
            Model = model;
            GetComponent<BoxCollider2D>().isTrigger = true;
            RefreshDisplay();
        }

        public void RefreshDisplay()
        {
            NexusHealthDisplay.Refresh(nexusHpInfo, Model);
        }

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
