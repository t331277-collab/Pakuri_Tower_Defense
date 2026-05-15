using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public sealed class InGameAutoSkillButton : MonoBehaviour
    {
        [SerializeField] private InGameCombatManager combatManager;

        private void Awake()
        {
            if (combatManager == null)
            {
                combatManager = FindFirstObjectByType<InGameCombatManager>();
            }

            var button = GetComponent<Button>();
            button.onClick.RemoveListener(EnablePlayerAutoSkillMode);
            button.onClick.AddListener(EnablePlayerAutoSkillMode);
        }

        private void EnablePlayerAutoSkillMode()
        {
            if (combatManager != null)
            {
                combatManager.EnablePlayerAutoSkillMode();
            }
        }
    }
}
