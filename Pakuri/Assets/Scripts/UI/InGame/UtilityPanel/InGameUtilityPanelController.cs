using UnityEngine;
using UnityEngine.UI;

/*
 * 자동 스킬 사용과 전투 배속 버튼을 제어하는 인게임 UI 컴포넌트.
 */
namespace Pakuri.InGame
{
    public class InGameUtilityPanelController : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 1.5f, 2f };

        // 자동 스킬 입력 상태는 PlayerCombatControl에 직접 요청한다.
        [SerializeField] private PlayerCombatInputController playerCombatControl;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button timeButton;
        [SerializeField] private GameObject onePointFiveIndicator;
        [SerializeField] private GameObject twoTimesIndicator;

        private float baseFixedDeltaTime;
        private int timeScaleIndex;

        /*
         * Unity가 컴포넌트를 초기화할 때 필요한 참조와 상태를 준비한다.
         */
        private void Awake()
        {
            ResolveReferences();
            baseFixedDeltaTime = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
            ApplyTimeScale(0);
        }

        /*
         * 컴포넌트가 활성화될 때 이벤트와 표시 상태를 연결한다.
         */
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

        /*
         * 컴포넌트가 비활성화될 때 연결된 이벤트를 해제한다.
         */
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

        /*
         * 컴포넌트가 제거될 때 남아 있는 연결과 상태를 정리한다.
         */
        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (baseFixedDeltaTime > 0f)
            {
                Time.fixedDeltaTime = baseFixedDeltaTime;
            }
        }

        /*
         * ResolveReferences에 필요한 값을 계산해 현재 상태에 반영한다.
         */
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

        /*
         * CycleTimeScale 작업을 수행한다.
         */
        private void CycleTimeScale()
        {
            ApplyTimeScale((timeScaleIndex + 1) % TimeScales.Length);
        }

        /*
         * ApplyTimeScale 처리를 대상에 적용한다.
         */
        private void ApplyTimeScale(int index /* 목록에서의 순서 번호 */)
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

        /*
         * ToggleSelectedPlayerAutoSkillMode 작업을 수행한다.
         */
        private void ToggleSelectedPlayerAutoSkillMode()
        {
            if (playerCombatControl != null)
            {
                var combatManager = FindFirstObjectByType<InGameCombatManager>();
                playerCombatControl.ToggleAutoSkillMode(
                    combatManager != null ? combatManager.UnitRegistry : null);
            }
        }
    }
}
