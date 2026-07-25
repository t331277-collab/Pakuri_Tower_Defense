using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;

/* Nexus Model의 scene 결합과 Inspector 체력 설정·화면 HP 표시를 소유한다. */
namespace Pakuri.NewCore.Units.Actors
{
    public sealed class NexusActor : UnitActor
    {
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private TextMeshProUGUI nexusHpInfo;

        public float MaxHealth => maxHealth;

        public NexusModel Nexus => Model as NexusModel;

        /* Nexus 체력을 공통 월드 표시와 전용 UI 라벨에 함께 반영한다. */
        public override void SyncFromModel()
        {
            base.SyncFromModel();
            if (Nexus != null && nexusHpInfo != null)
            {
                nexusHpInfo.text =
                    $"Nexus HP {Mathf.CeilToInt(Nexus.CurrentHealth)}/{Mathf.CeilToInt(Nexus.MaximumHealth)}";
            }
        }
    }
}
