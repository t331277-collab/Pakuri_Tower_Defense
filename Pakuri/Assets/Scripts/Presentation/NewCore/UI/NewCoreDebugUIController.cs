using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreDebugUIController : MonoBehaviour
    {
        private GameObject debugRootPanel;
        private GameObject debugPanel;
        private GameObject modifierPanel;
        private GameObject passiveModifierPanel;
        private Button openButton;
        private Button closeButton;

        private void Awake()
        {
            debugRootPanel = FindObject("DebugPanel");
            debugPanel = FindObject("DebugPanel/DebugUI");
            modifierPanel = FindObject("DebugPanel/DebugModifiedUI");
            passiveModifierPanel =
                FindObject("DebugPanel/DebugPassiveModifiedUI");
            openButton = FindButton("DebugPanel/DebugUIBtn")
                ?? FindButton("DebugPanel/DebugBtn");
            closeButton = FindButton("DebugPanel/DebugUI/Close");
            Bind(openButton, Open);
            Bind(closeButton, Close);
            Close();
        }

        public void Open()
        {
            SetActive(debugRootPanel, true);
            SetActive(debugPanel, true);
            SetActive(modifierPanel, false);
            SetActive(passiveModifierPanel, false);
        }

        public void Close()
        {
            SetActive(debugRootPanel, false);
        }

        private GameObject FindObject(string path)
        {
            var target = transform.Find(path);
            return target != null ? target.gameObject : null;
        }

        private Button FindButton(string path)
        {
            var target = transform.Find(path);
            return target != null ? target.GetComponent<Button>() : null;
        }

        private static void Bind(
            Button button,
            UnityEngine.Events.UnityAction action)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(action);
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
