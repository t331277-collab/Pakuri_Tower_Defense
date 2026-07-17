using System.Collections.Generic;
using UnityEngine;

namespace Pakuri.InGame
{
    internal static class UnitActorView
    {
        public const string NameLabelObjectName = "MonsterNameLabel";
        public const string HpLabelObjectName = "MonsterHpLabel";
        public const string HpBackgroundObjectName = "Background";
        public const string HpFillObjectName = "Fill";
        public const string ShieldFillObjectName = "Shield";
        public const string DamageTextObjectName = "Damage";

        public static void Refresh(
            BaseUnitRuntimeModel model,
            TextMesh nameLabel,
            TextMesh hpLabel,
            Transform hpBackground,
            Transform hpFill,
            Transform shieldFill)
        {
            if (model == null)
            {
                return;
            }

            var identity = model.Identity;
            var resources = model.Resources;
            var stats = model.Stats;
            var displayName = identity != null && !string.IsNullOrWhiteSpace(identity.DisplayName)
                ? identity.DisplayName
                : identity != null ? identity.DefinitionId : string.Empty;
            displayName = AppendStatusDisplay(displayName, model);
            var maxHealth = stats != null ? Mathf.Max(0f, stats.MaxHealth) : 0f;
            var currentHealth = resources != null ? Mathf.Clamp(resources.CurrentHealth, 0f, maxHealth) : 0f;
            var currentShield = resources != null ? Mathf.Max(0f, resources.CurrentShield) : 0f;

            if (nameLabel != null)
            {
                nameLabel.text = displayName;
            }

            if (hpLabel != null)
            {
                hpLabel.text = currentShield > 0f
                    ? $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)} +{FormatValue(currentShield)}"
                    : $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)}";
            }

            SetResourceFillSegments(currentHealth, currentShield, maxHealth, hpBackground, hpFill, shieldFill);
            if (shieldFill != null)
            {
                shieldFill.gameObject.SetActive(currentShield > 0f);
            }
        }

        public static TextMesh FindTextMesh(Component owner, string objectName)
        {
            var target = FindChildTransform(owner, objectName);
            return target != null ? target.GetComponent<TextMesh>() : null;
        }

        public static Transform FindChildTransform(Component owner, string objectName)
        {
            if (owner == null || string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            var children = owner.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == objectName)
                {
                    return children[i];
                }
            }

            return null;
        }

        public static InGameDamageTextPopup EnsureDamagePopup(Component owner, TextMesh damageTextLabel)
        {
            if (damageTextLabel == null)
            {
                damageTextLabel = FindTextMesh(owner, DamageTextObjectName);
            }

            if (damageTextLabel == null)
            {
                return null;
            }

            var popup = damageTextLabel.GetComponent<InGameDamageTextPopup>();
            if (popup == null)
            {
                popup = damageTextLabel.gameObject.AddComponent<InGameDamageTextPopup>();
            }

            popup.Initialize(damageTextLabel);
            return popup;
        }

        private static void SetResourceFillSegments(
            float currentHealth,
            float currentShield,
            float maxHealth,
            Transform hpBackground,
            Transform hpFill,
            Transform shieldFill)
        {
            var totalVisibleResource = Mathf.Max(maxHealth, currentHealth + currentShield);
            var safeTotal = totalVisibleResource > 0f ? totalVisibleResource : 1f;
            var healthRatio = Mathf.Clamp01(currentHealth / safeTotal);
            var shieldRatio = Mathf.Clamp01(currentShield / safeTotal);

            SetSegmentScaleAndPosition(hpFill, hpBackground, 0f, healthRatio);
            SetSegmentScaleAndPosition(shieldFill, hpBackground, healthRatio, shieldRatio);
        }

        private static void SetSegmentScaleAndPosition(
            Transform target,
            Transform background,
            float leftRatio,
            float widthRatio)
        {
            if (target == null)
            {
                return;
            }

            var baseScaleX = background != null ? background.localScale.x : target.localScale.x;
            var backgroundCenterX = background != null ? background.localPosition.x : 0f;
            var backgroundWidth = ResolveLocalRenderedWidth(background, Mathf.Abs(baseScaleX));
            var segmentWidth = backgroundWidth * Mathf.Clamp01(widthRatio);
            var scale = target.localScale;
            scale.x = ResolveScaleXForRenderedWidth(target, segmentWidth, baseScaleX);
            target.localScale = scale;

            var position = target.localPosition;
            position.x = backgroundCenterX - (backgroundWidth * 0.5f)
                + (backgroundWidth * Mathf.Clamp01(leftRatio))
                + (segmentWidth * 0.5f);
            target.localPosition = position;
        }

        private static float ResolveLocalRenderedWidth(Transform target, float fallbackWidth)
        {
            if (target == null)
            {
                return Mathf.Max(0f, fallbackWidth);
            }

            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            var sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
            if (sprite == null)
            {
                return Mathf.Abs(target.localScale.x);
            }

            return Mathf.Abs(target.localScale.x) * Mathf.Max(0.0001f, sprite.bounds.size.x);
        }

        private static float ResolveScaleXForRenderedWidth(Transform target, float renderedWidth, float fallbackSignSource)
        {
            if (target == null)
            {
                return renderedWidth;
            }

            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            var sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
            var unitWidth = sprite != null ? Mathf.Max(0.0001f, sprite.bounds.size.x) : 1f;
            var signSource = !Mathf.Approximately(target.localScale.x, 0f)
                ? target.localScale.x
                : fallbackSignSource;
            var sign = signSource < 0f ? -1f : 1f;
            return sign * Mathf.Max(0f, renderedWidth) / unitWidth;
        }

        private static string FormatValue(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value))
                ? Mathf.RoundToInt(value).ToString()
                : value.ToString("0.##");
        }

        private static string AppendStatusDisplay(string displayName, BaseUnitRuntimeModel model)
        {
            var suffix = model != null && model.Statuses != null
                ? StatusEffectUtility.BuildDisplaySuffix(model.Statuses.ActiveStatuses)
                : string.Empty;
            return string.IsNullOrWhiteSpace(suffix) ? displayName : $"{displayName}{suffix}";
        }
    }

    [DisallowMultipleComponent]
    internal sealed class InGameDamageTextPopup : MonoBehaviour
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

            var clonedController = instance.GetComponent<InGameDamageTextPopup>();
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

        private sealed class ActiveDamagePopup
        {
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
