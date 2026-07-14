using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class InGameUtilityPanelController : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 1.5f, 2f };

        [SerializeField] private InGameCombatManager combatManager;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button timeButton;
        [SerializeField] private GameObject onePointFiveIndicator;
        [SerializeField] private GameObject twoTimesIndicator;

        private float baseFixedDeltaTime;
        private int timeScaleIndex;

        private void Awake()
        {
            ResolveReferences();
            baseFixedDeltaTime = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
            ApplyTimeScale(0);
        }

        private void OnEnable()
        {
            ResolveReferences();

            if (autoButton != null)
            {
                autoButton.onClick.RemoveListener(ToggleSelectedPlayerAutoSkillMode);
                autoButton.onClick.AddListener(ToggleSelectedPlayerAutoSkillMode);
            }

            if (timeButton != null)
            {
                timeButton.onClick.RemoveListener(CycleTimeScale);
                timeButton.onClick.AddListener(CycleTimeScale);
            }
        }

        private void OnDisable()
        {
            if (autoButton != null)
            {
                autoButton.onClick.RemoveListener(ToggleSelectedPlayerAutoSkillMode);
            }

            if (timeButton != null)
            {
                timeButton.onClick.RemoveListener(CycleTimeScale);
            }
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
            if (combatManager == null)
            {
                combatManager = FindFirstObjectByType<InGameCombatManager>();
            }

            var autoTransform = transform.Find("AutoBtn");
            var timeTransform = transform.Find("TimeBtn");

            if (autoButton == null && autoTransform != null)
            {
                autoButton = autoTransform.GetComponent<Button>();
            }

            if (timeButton == null && timeTransform != null)
            {
                timeButton = timeTransform.GetComponent<Button>();
            }

            if (onePointFiveIndicator == null && timeTransform != null)
            {
                var indicator = timeTransform.Find("1.5");
                onePointFiveIndicator = indicator != null ? indicator.gameObject : null;
            }

            if (twoTimesIndicator == null && timeTransform != null)
            {
                var indicator = timeTransform.Find("2");
                twoTimesIndicator = indicator != null ? indicator.gameObject : null;
            }
        }

        private void CycleTimeScale()
        {
            ApplyTimeScale((timeScaleIndex + 1) % TimeScales.Length);
        }

        private void ApplyTimeScale(int index)
        {
            timeScaleIndex = Mathf.Clamp(index, 0, TimeScales.Length - 1);
            var timeScale = TimeScales[timeScaleIndex];
            Time.timeScale = timeScale;
            Time.fixedDeltaTime = baseFixedDeltaTime * timeScale;

            if (onePointFiveIndicator != null)
            {
                onePointFiveIndicator.SetActive(timeScaleIndex == 1);
            }

            if (twoTimesIndicator != null)
            {
                twoTimesIndicator.SetActive(timeScaleIndex == 2);
            }
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
