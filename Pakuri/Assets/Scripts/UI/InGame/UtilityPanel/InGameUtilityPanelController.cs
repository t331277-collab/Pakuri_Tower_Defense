/*
 * 역할: InGame Utility Panel 제어.
 * 책임: 일시정지·배속·Debug·Damage Meter·설정·종료 동작을 관리한다.
 */

using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    /// InGameUtilityPanelController가 담당하는 입력 또는 표시 흐름을 조정하고 관련 런타임 상태를 갱신한다.
    public class InGameUtilityPanelController : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 1.5f, 2f };

        [SerializeField] private PlayerCombatInputController playerCombatControl;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button timeButton;
        [SerializeField] private GameObject onePointFiveIndicator;
        [SerializeField] private GameObject twoTimesIndicator;

        private float baseFixedDeltaTime;
        private int timeScaleIndex;

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            ResolveReferences();
            baseFixedDeltaTime = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
            ApplyTimeScale(0);
        }

        /// Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.
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

        /// Unity가 컴포넌트를 비활성화할 때 구독과 임시 상태를 중단한다.
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

        /// Unity가 컴포넌트를 제거할 때 구독과 런타임 오브젝트를 해제한다.
        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (baseFixedDeltaTime > 0f)
            {
                Time.fixedDeltaTime = baseFixedDeltaTime;
            }
        }

        /// References를 결정한다.
        private void ResolveReferences()
        {
            if (playerCombatControl == null)
            {
                playerCombatControl = FindFirstObjectByType<PlayerCombatInputController>();
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

        /// CycleTimeScale 작업을 수행한다.
        private void CycleTimeScale()
        {
            ApplyTimeScale((timeScaleIndex + 1) % TimeScales.Length);
        }

        /// 전달된 index 값을 사용해 TimeScale를 적용한다.
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

        /// SelectedPlayerAutoSkillMode를 활성 상태를 전환한다.
        private void ToggleSelectedPlayerAutoSkillMode()
        {
            if (playerCombatControl != null)
            {
                var combatManager = FindFirstObjectByType<InGameCombatManager>();
                playerCombatControl.ToggleAutoSkillMode(
                    combatManager != null ? combatManager.Units : null);
            }
        }
    }
}
