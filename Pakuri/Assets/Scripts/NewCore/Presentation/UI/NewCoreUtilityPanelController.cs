using Pakuri.NewCore.Presentation.Scene;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.NewCore.Presentation.UI
{
    public sealed class NewCoreUtilityPanelController : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 1.5f, 2f };

        [SerializeField] private NewCoreInputController playerCombatControl;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button timeButton;
        [SerializeField] private GameObject onePointFiveIndicator;
        [SerializeField] private GameObject twoTimesIndicator;

        private float baseFixedDeltaTime;
        private int timeScaleIndex;

        private void Awake()
        {
            ResolveReferences();
            baseFixedDeltaTime =
                Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
            Bind(autoButton, ToggleAuto);
            Bind(timeButton, CycleTimeScale);
            ApplyTimeScale(0);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (baseFixedDeltaTime > 0f)
            {
                Time.fixedDeltaTime = baseFixedDeltaTime;
            }
        }

        private void ResolveReferences()
        {
            if (playerCombatControl == null)
            {
                playerCombatControl =
                    FindFirstObjectByType<NewCoreInputController>();
            }

            var auto = transform.Find("AutoBtn");
            var time = transform.Find("TimeBtn");
            if (autoButton == null && auto != null)
            {
                autoButton = auto.GetComponent<Button>();
            }

            if (timeButton == null && time != null)
            {
                timeButton = time.GetComponent<Button>();
            }

            if (onePointFiveIndicator == null && time != null)
            {
                var child = time.Find("1.5");
                onePointFiveIndicator =
                    child != null ? child.gameObject : null;
            }

            if (twoTimesIndicator == null && time != null)
            {
                var child = time.Find("2");
                twoTimesIndicator =
                    child != null ? child.gameObject : null;
            }
        }

        private void ToggleAuto()
        {
            playerCombatControl?.ToggleAutoSkill();
        }

        private void CycleTimeScale()
        {
            ApplyTimeScale((timeScaleIndex + 1) % TimeScales.Length);
        }

        private void ApplyTimeScale(int index)
        {
            timeScaleIndex = Mathf.Clamp(index, 0, TimeScales.Length - 1);
            var scale = TimeScales[timeScaleIndex];
            Time.timeScale = scale;
            Time.fixedDeltaTime = baseFixedDeltaTime * scale;
            SetActive(onePointFiveIndicator, timeScaleIndex == 1);
            SetActive(twoTimesIndicator, timeScaleIndex == 2);
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
