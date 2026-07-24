using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.NewCore.Presentation.Actors
{
    public sealed class DamageNumberPopupBehaviour : MonoBehaviour
    {
        private const float DefaultDurationSeconds = 1f;
        private const float DefaultRiseDistance = 1f;
        private const int DefaultMaximumActivePopups = 12;
        private const float DefaultVerticalSpacing = 0.18f;

        [SerializeField] private float durationSeconds =
            DefaultDurationSeconds;
        [SerializeField] private float riseDistance =
            DefaultRiseDistance;
        [SerializeField] private int maximumActivePopups =
            DefaultMaximumActivePopups;
        [SerializeField] private float verticalSpacing =
            DefaultVerticalSpacing;

        private readonly List<Popup> popups = new List<Popup>();
        private TextMesh template;
        private Vector3 templatePosition;
        private Color templateColor;

        public int ActivePopupCount => popups.Count;

        public void Initialize(TextMesh damageTemplate)
        {
            Clear();
            template = damageTemplate;
            if (template == null)
            {
                return;
            }

            templatePosition = template.transform.localPosition;
            templateColor = template.color;
            template.text = string.Empty;
            template.gameObject.SetActive(false);
        }

        public void Show(float damageAmount)
        {
            if (template == null || damageAmount <= 0f)
            {
                return;
            }

            RemoveMissingPopups();
            while (popups.Count >= Mathf.Max(
                1,
                maximumActivePopups))
            {
                DestroyPopup(popups[0]);
                popups.RemoveAt(0);
            }

            var instance = Instantiate(
                template.gameObject,
                template.transform.parent);
            instance.name = template.gameObject.name + "_Popup";
            instance.SetActive(true);
            var text = instance.GetComponent<TextMesh>();
            if (text == null)
            {
                DestroyPopupObject(instance);
                return;
            }

            var position = templatePosition;
            position.y += popups.Count
                * Mathf.Max(0f, verticalSpacing);
            text.transform.localPosition = position;
            text.text =
                $"{Mathf.RoundToInt(damageAmount)}(Damage)";
            var color = templateColor;
            color.a = 1f;
            text.color = color;
            popups.Add(new Popup(
                instance,
                text,
                position,
                color,
                Mathf.Max(0.01f, durationSeconds)));
        }

        public void Clear()
        {
            for (var index = popups.Count - 1; index >= 0; index--)
            {
                DestroyPopup(popups[index]);
            }

            popups.Clear();
        }

        private void Update()
        {
            Tick(Time.unscaledDeltaTime);
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime < 0f
                || float.IsNaN(deltaTime)
                || float.IsInfinity(deltaTime))
            {
                throw new System.ArgumentOutOfRangeException(
                    nameof(deltaTime));
            }

            for (var index = popups.Count - 1; index >= 0; index--)
            {
                var popup = popups[index];
                if (popup.Text == null)
                {
                    popups.RemoveAt(index);
                    continue;
                }

                popup.ElapsedSeconds += deltaTime;
                var normalized = Mathf.Clamp01(
                    popup.ElapsedSeconds / popup.DurationSeconds);
                var position = popup.StartPosition;
                position.y += Mathf.Max(0f, riseDistance) * normalized;
                popup.Text.transform.localPosition = position;
                var color = popup.StartColor;
                color.a = 1f - normalized;
                popup.Text.color = color;
                if (popup.ElapsedSeconds >= popup.DurationSeconds)
                {
                    DestroyPopup(popup);
                    popups.RemoveAt(index);
                }
            }
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void RemoveMissingPopups()
        {
            for (var index = popups.Count - 1; index >= 0; index--)
            {
                if (popups[index].Text == null)
                {
                    popups.RemoveAt(index);
                }
            }
        }

        private static void DestroyPopup(Popup popup)
        {
            if (popup?.Instance != null)
            {
                DestroyPopupObject(popup.Instance);
            }
        }

        private static void DestroyPopupObject(Object target)
        {
            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }

        private sealed class Popup
        {
            public Popup(
                GameObject instance,
                TextMesh text,
                Vector3 startPosition,
                Color startColor,
                float durationSeconds)
            {
                Instance = instance;
                Text = text;
                StartPosition = startPosition;
                StartColor = startColor;
                DurationSeconds = durationSeconds;
            }

            public GameObject Instance { get; }
            public TextMesh Text { get; }
            public Vector3 StartPosition { get; }
            public Color StartColor { get; }
            public float DurationSeconds { get; }
            public float ElapsedSeconds { get; set; }
        }
    }
}
