/*
 * 역할: InGame Utility Panel 제어.
 * 책임: 일시정지·배속·Debug·Damage Meter·설정·종료 동작을 관리한다.
 */

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pakuri.InGame
{

    /// 일시정지·배속·Debug·Damage Meter·설정·종료 버튼을 관리한다.
    public class InGameUtilityPanelController : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 1.5f, 2f };

        private PlayerCombatInputController playerCombatControl;
        private InGameCombatManager combatManager;
        private Button autoButton;
        private Button timeButton;
        private GameObject onePointFiveIndicator;
        private GameObject twoTimesIndicator;

        private float baseFixedDeltaTime;
        private int timeScaleIndex;
        private ColorBlock autoButtonDefaultColors;
        private bool hasAutoButtonDefaultColors;
        private bool referencesBound;
        private bool bindingFailed;

        public float CurrentTimeScale => TimeScales[timeScaleIndex];
        public event Action<float> TimeScaleChanged;

        public void SetTutorialInputEnabled(bool enabled)
        {
            if (autoButton != null)
            {
                autoButton.interactable = enabled;
            }

            if (timeButton != null)
            {
                timeButton.interactable = enabled;
            }
        }

        /// Unity가 컴포넌트를 로드할 때 의존성과 소유 런타임 상태를 초기화한다.
        private void Awake()
        {
            if (!BindObject())
            {
                enabled = false;
                return;
            }

            if (autoButton != null)
            {
                autoButtonDefaultColors = autoButton.colors;
                hasAutoButtonDefaultColors = true;
            }

            baseFixedDeltaTime = Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
            ApplyTimeScale(0);
            RefreshAutoButtonVisual();
        }

        /// Unity가 컴포넌트를 활성화할 때 구독과 활성 상태를 복원한다.
        private void OnEnable()
        {
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

            RefreshAutoButtonVisual();
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
            TimeScaleChanged?.Invoke(timeScale);

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
                playerCombatControl.ToggleAutoSkillMode(
                    combatManager != null ? combatManager.Units : null);
                RefreshAutoButtonVisual();
            }
        }

        private void RefreshAutoButtonVisual()
        {
            if (autoButton == null
                || playerCombatControl == null
                || !hasAutoButtonDefaultColors)
            {
                return;
            }

            var colors = autoButtonDefaultColors;
            var stateColor = playerCombatControl.AutoSkillEnabled
                ? autoButtonDefaultColors.selectedColor
                : autoButtonDefaultColors.normalColor;
            colors.normalColor = stateColor;
            colors.highlightedColor = stateColor;
            colors.pressedColor = stateColor;
            colors.selectedColor = stateColor;
            autoButton.colors = colors;
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
            playerCombatControl = UiBindingUtility.BindSceneComponent<PlayerCombatInputController>(
                this,
                nameof(playerCombatControl),
                ref valid);
            combatManager = UiBindingUtility.BindSceneComponent<InGameCombatManager>(
                this,
                nameof(combatManager),
                ref valid);
            autoButton = UiBindingUtility.BindChild<Button>(
                this,
                "AutoBtn",
                nameof(autoButton),
                ref valid);
            timeButton = UiBindingUtility.BindChild<Button>(
                this,
                "TimeBtn",
                nameof(timeButton),
                ref valid);
            onePointFiveIndicator = UiBindingUtility.BindChildObject(
                this,
                transform,
                "TimeBtn/1.5",
                nameof(onePointFiveIndicator),
                ref valid);
            twoTimesIndicator = UiBindingUtility.BindChildObject(
                this,
                transform,
                "TimeBtn/2",
                nameof(twoTimesIndicator),
                ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }
    }
}
