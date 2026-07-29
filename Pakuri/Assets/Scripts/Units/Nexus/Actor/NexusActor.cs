/*
 * 역할: Nexus 씬과 모델 연결.
 * 책임: Nexus 전투 모델을 월드 표시에 연결하고 표시 체력을 동기화한다.
 */

using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>NexusActor</c> 런타임 오브젝트를 나타내며 모델과 Unity 컴포넌트를 연결한다.</summary>
    public class NexusActor : MonoBehaviour
    {
        private const float DefaultMaxHealth = 20f;

        [SerializeField] private float maxHealth = DefaultMaxHealth;
        [SerializeField] private TextMeshProUGUI nexusHpInfo;

        public float MaxHealth => maxHealth;
        public UnitCombatState Model { get; private set; }

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>소유한 런타임 상태</c>를 초기화한다.</summary>
        public void Initialize(UnitCombatState model)
        {
            Model = model;
            GetComponent<BoxCollider2D>().isTrigger = true;
            RefreshDisplay();
        }

        /// <summary><c>Display</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
        public void RefreshDisplay()
        {
            NexusHealthDisplay.Refresh(nexusHpInfo, Model);
        }

        /// <summary>전달된 <c>currentHealth</c> 값을 사용해 <c>CurrentHealth</c>를 갱신한다.</summary>
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
