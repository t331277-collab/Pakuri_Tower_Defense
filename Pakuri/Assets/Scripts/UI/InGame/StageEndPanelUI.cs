using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    public sealed class StageEndPanelUI : MonoBehaviour
    {
        private Button returnButton;

        private UnityAction boundReturnAction;
        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            BindObject();
        }

        public void BindReturn(UnityAction action)
        {
            if (!BindObject() || action == null)
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
            if (!BindObject())
            {
                return;
            }

            gameObject.SetActive(visible);
        }

        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            var valid = true;
            returnButton = UiBindingUtility.BindChild<Button>(
                this,
                "Button",
                nameof(returnButton),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }
    }
}
