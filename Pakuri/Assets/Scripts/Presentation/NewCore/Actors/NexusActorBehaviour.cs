using Pakuri.NewCore.Units.Models;
using TMPro;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class NexusActorBehaviour : UnitActorBehaviour
    {
        [SerializeField] private float maxHealth = 20f;
        [SerializeField] private TextMeshProUGUI nexusHpInfo;

        public float MaxHealth => maxHealth;

        public NexusModel Nexus => Model as NexusModel;

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
