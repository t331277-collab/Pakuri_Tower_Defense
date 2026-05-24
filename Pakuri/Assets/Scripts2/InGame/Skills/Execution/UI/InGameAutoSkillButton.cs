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
            button.onClick.RemoveListener(ToggleSelectedPlayerAutoSkillMode);
            button.onClick.AddListener(ToggleSelectedPlayerAutoSkillMode);
        }

        private void ToggleSelectedPlayerAutoSkillMode()
        {
            if (combatManager != null)
            {
                combatManager.ToggleSelectedPlayerAutoSkillMode();
            }
        }
    }
}
