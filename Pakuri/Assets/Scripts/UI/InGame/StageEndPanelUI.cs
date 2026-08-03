using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    public sealed class StageEndPanelUI : MonoBehaviour
    {
        [SerializeField] private Button returnButton;

        private UnityAction boundReturnAction;

        public void BindReturn(UnityAction action)
        {
            if (returnButton == null || action == null)
            {
                return;
            }

            if (boundReturnAction != null)
            {
                returnButton.onClick.RemoveListener(boundReturnAction);
            }

            boundReturnAction = action;
            returnButton.onClick.AddListener(boundReturnAction);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}
