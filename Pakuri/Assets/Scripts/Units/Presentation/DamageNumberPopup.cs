using System.Collections.Generic;
using UnityEngine;

/*
 * 유닛 머리 위에 피해 숫자를 복제하고 위로 이동시키며 서서히 숨긴다.
 */
namespace Pakuri.InGame
{
    internal class DamageNumberPopup : MonoBehaviour
    {
        private const float DefaultDurationSeconds = 1f;
        private const float DefaultRiseDistance = 1f;
        private const int DefaultMaxActivePopups = 12;
        private const float DefaultStackVerticalSpacing = 0.18f;

        [SerializeField] private TextMesh damageText;
        [SerializeField] private float durationSeconds = DefaultDurationSeconds;
        [SerializeField] private float riseDistance = DefaultRiseDistance;
        [SerializeField] private int maxActivePopups = DefaultMaxActivePopups;
        [SerializeField] private float stackVerticalSpacing = DefaultStackVerticalSpacing;

        private Vector3 startLocalPosition;
        private Color startColor = Color.white;
        private readonly List<ActiveDamagePopup> activePopups = new List<ActiveDamagePopup>();
        private bool initialized;

        /*
         * Initialize에 필요한 값을 설정한다.
         */
        public void Initialize(TextMesh textMesh)
        {
            damageText = textMesh != null ? textMesh : GetComponent<TextMesh>();
            if (damageText == null)
            {
                enabled = false;
                return;
            }

            startLocalPosition = transform.localPosition;
            startColor = damageText.color;
            initialized = true;
            damageText.text = string.Empty;
            var hiddenColor = startColor;
            hiddenColor.a = 0f;
            damageText.color = hiddenColor;
            enabled = false;
        }

        /*
         * Show 작업을 수행한다.
         */
        public void Show(float damageAmount)
        {
            if (!initialized)
            {
                Initialize(damageText);
            }

            if (damageText == null)
            {
                return;
            }

            gameObject.SetActive(true);
            SpawnPopup(damageAmount);
            enabled = true;
        }

        /*
         * 매 프레임 현재 상태를 갱신한다.
         */
        private void Update()
        {
            if (damageText == null)
            {
                enabled = false;
                return;
            }

            for (var i = activePopups.Count - 1; i >= 0; i--)
            {
                var popup = activePopups[i];
                if (popup == null || popup.Text == null)
                {
                    activePopups.RemoveAt(i);
                    continue;
                }

                popup.ElapsedSeconds += Time.deltaTime;
                var normalized = Mathf.Clamp01(popup.ElapsedSeconds / popup.DurationSeconds);
                var position = popup.StartLocalPosition;
                position.y += riseDistance * normalized;
                popup.Text.transform.localPosition = position;
                var popupColor = popup.StartColor;
                popupColor.a = Mathf.Clamp01(1f - normalized);
                popup.Text.color = popupColor;

                if (popup.ElapsedSeconds >= popup.DurationSeconds)
                {
                    if (popup.Instance != null)
                    {
                        Destroy(popup.Instance);
                    }

                    activePopups.RemoveAt(i);
                }
            }

            if (activePopups.Count == 0)
            {
                enabled = false;
            }
        }

        /*
         * SpawnPopup 작업을 수행한다.
         */
        private void SpawnPopup(float damageAmount)
        {
            damageText.text = string.Empty;
            var hiddenColor = startColor;
            hiddenColor.a = 0f;
            damageText.color = hiddenColor;
            enabled = false;

            for (var i = activePopups.Count - 1; i >= 0; i--)
            {
                if (activePopups[i] == null || activePopups[i].Text == null)
                {
                    activePopups.RemoveAt(i);
                }
            }

            var maxCount = Mathf.Max(1, maxActivePopups);
            while (activePopups.Count >= maxCount)
            {
                var oldest = activePopups[0];
                if (oldest != null && oldest.Instance != null)
                {
                    Destroy(oldest.Instance);
                }

                activePopups.RemoveAt(0);
            }

            var instance = Instantiate(damageText.gameObject, damageText.transform.parent);
            instance.name = $"{damageText.gameObject.name}_Popup";
            instance.SetActive(true);

            var clonedController = instance.GetComponent<DamageNumberPopup>();
            if (clonedController != null)
            {
                clonedController.enabled = false;
                Destroy(clonedController);
            }

            var text = instance.GetComponent<TextMesh>();
            if (text == null)
            {
                Destroy(instance);
                return;
            }

            var stackOffset = activePopups.Count * Mathf.Max(0f, stackVerticalSpacing);
            var popupStart = startLocalPosition;
            popupStart.y += stackOffset;
            text.transform.localPosition = popupStart;
            text.text = $"{Mathf.RoundToInt(Mathf.Max(0f, damageAmount))}(Damage)";
            var visibleColor = startColor;
            visibleColor.a = 1f;
            text.color = visibleColor;

            activePopups.Add(new ActiveDamagePopup(
                instance,
                text,
                popupStart,
                startColor,
                Mathf.Max(0.01f, durationSeconds)));
        }

        private class ActiveDamagePopup
        {
            /*
             * ActiveDamagePopup에 필요한 값을 초기화한다.
             */
            public ActiveDamagePopup(
                GameObject instance,
                TextMesh text,
                Vector3 startLocalPosition,
                Color startColor,
                float durationSeconds)
            {
                Instance = instance;
                Text = text;
                StartLocalPosition = startLocalPosition;
                StartColor = startColor;
                DurationSeconds = durationSeconds;
            }

            public GameObject Instance { get; }
            public TextMesh Text { get; }
            public Vector3 StartLocalPosition { get; }
            public Color StartColor { get; }
            public float DurationSeconds { get; }
            public float ElapsedSeconds { get; set; }
        }
    }
}
