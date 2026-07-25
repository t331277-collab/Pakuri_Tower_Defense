using Pakuri.NewCore.Combat.Actions;
using UnityEngine;
using UnityEngine.UI;

/* Auto 전투 command와 공통 Time.timeScale 순환 표시를 소유한다. */
namespace Pakuri.NewCore.UI.InGame.UtilityPanel
{
    public sealed class NewCoreUtilityPanelController : MonoBehaviour
    {
        private static readonly float[] TimeScales = { 1f, 1.5f, 2f };

        [SerializeField] private PlayerInputController playerCombatControl;
        [SerializeField] private Button autoButton;
        [SerializeField] private Button timeButton;
        [SerializeField] private GameObject onePointFiveIndicator;
        [SerializeField] private GameObject twoTimesIndicator;

        private float baseFixedDeltaTime;
        private int timeScaleIndex;

        /* utility 참조와 button command를 연결하고 기본 배속을 적용한다. */
        private void Awake()
        {
            ResolveReferences();
            baseFixedDeltaTime =
                Time.fixedDeltaTime / Mathf.Max(Time.timeScale, 0.0001f);
            Bind(autoButton, ToggleAuto);
            Bind(timeButton, CycleTimeScale);
            ApplyTimeScale(0);
        }

        /* scene 종료 시 전역 시간 배율과 fixed delta를 기본값으로 복원한다. */
        private void OnDestroy()
        {
            Time.timeScale = 1f;
            if (baseFixedDeltaTime > 0f)
            {
                Time.fixedDeltaTime = baseFixedDeltaTime;
            }
        }

        /* authored field가 비어 있으면 scene hierarchy에서 utility 참조를 찾는다. */
        private void ResolveReferences()
        {
            if (playerCombatControl == null)
            {
                playerCombatControl =
                    FindFirstObjectByType<PlayerInputController>();
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

        /* PlayerInputController의 자동 skill 사용 상태를 전환한다. */
        private void ToggleAuto()
        {
            playerCombatControl?.ToggleAutoSkill();
        }

        /* 지원하는 다음 시간 배율로 순환한다. */
        private void CycleTimeScale()
        {
            ApplyTimeScale((timeScaleIndex + 1) % TimeScales.Length);
        }

        /* 선택 시간 배율과 대응 indicator 상태를 적용한다. */
        private void ApplyTimeScale(int index)
        {
            timeScaleIndex = Mathf.Clamp(index, 0, TimeScales.Length - 1);
            var scale = TimeScales[timeScaleIndex];
            Time.timeScale = scale;
            Time.fixedDeltaTime = baseFixedDeltaTime * scale;
            SetActive(onePointFiveIndicator, timeScaleIndex == 1);
            SetActive(twoTimesIndicator, timeScaleIndex == 2);
        }

        /* Button의 기존 listener를 정리하고 utility command를 연결한다. */
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

        /* 존재하는 GameObject의 활성 상태를 바꾼다. */
        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
