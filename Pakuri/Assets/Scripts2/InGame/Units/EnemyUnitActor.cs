using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class EnemyUnitActor : MonoBehaviour
    {
        private const string NameLabelObjectName = "MonsterNameLabel";
        private const string HpLabelObjectName = "MonsterHpLabel";
        private const string HpBackgroundObjectName = "Background";
        private const string HpFillObjectName = "Fill";
        private const string ShieldFillObjectName = "Shield";
        private const string DamageTextObjectName = "Damage";

        [SerializeField] private TextMesh enemyNameLabel;
        [SerializeField] private TextMesh enemyHpLabel;
        [SerializeField] private TextMesh damageTextLabel;
        [SerializeField] private Transform hpBackground;
        [SerializeField] private Transform hpFill;
        [SerializeField] private Transform shieldFill;
        [SerializeField] private InGameDamageTextPopup damageTextPopup;

        public EnemyUnitRuntimeModel Model { get; private set; }

        public void Initialize(EnemyUnitRuntimeModel model)
        {
            Model = model;
            ResolveDebugViewReferences();
            RefreshDebugView();
        }

        public void ShowDamage(float damageAmount)
        {
            if (damageTextPopup != null)
            {
                damageTextPopup.Show(damageAmount);
            }
        }

        public void RefreshDebugView()
        {
            if (Model == null)
            {
                return;
            }

            var identity = Model.Identity;
            var resources = Model.Resources;
            var stats = Model.Stats;
            var displayName = identity != null && !string.IsNullOrWhiteSpace(identity.DisplayName)
                ? identity.DisplayName
                : identity != null ? identity.DefinitionId : string.Empty;
            displayName = AppendStatusDisplay(displayName, Model);
            var maxHealth = stats != null ? Mathf.Max(0f, stats.MaxHealth) : 0f;
            var currentHealth = resources != null ? Mathf.Clamp(resources.CurrentHealth, 0f, maxHealth) : 0f;
            var currentShield = resources != null ? Mathf.Max(0f, resources.CurrentShield) : 0f;

            if (enemyNameLabel != null)
            {
                enemyNameLabel.text = displayName;
            }

            if (enemyHpLabel != null)
            {
                enemyHpLabel.text = currentShield > 0f
                    ? $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)} +{FormatValue(currentShield)}"
                    : $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)}";
            }

            SetResourceFillSegments(currentHealth, currentShield, maxHealth);
            if (shieldFill != null)
            {
                shieldFill.gameObject.SetActive(currentShield > 0f);
            }
        }

        private void ResolveDebugViewReferences()
        {
            if (enemyNameLabel == null)
            {
                enemyNameLabel = FindTextMesh(NameLabelObjectName);
            }

            if (enemyHpLabel == null)
            {
                enemyHpLabel = FindTextMesh(HpLabelObjectName);
            }

            if (damageTextLabel == null)
            {
                damageTextLabel = FindTextMesh(DamageTextObjectName);
            }

            if (damageTextPopup == null && damageTextLabel != null)
            {
                damageTextPopup = damageTextLabel.GetComponent<InGameDamageTextPopup>();
                if (damageTextPopup == null)
                {
                    damageTextPopup = damageTextLabel.gameObject.AddComponent<InGameDamageTextPopup>();
                }

                damageTextPopup.Initialize(damageTextLabel);
            }

            if (hpBackground == null)
            {
                hpBackground = FindChildTransform(HpBackgroundObjectName);
            }

            if (hpFill == null)
            {
                hpFill = FindChildTransform(HpFillObjectName);
            }

            if (shieldFill == null)
            {
                shieldFill = FindChildTransform(ShieldFillObjectName);
            }
        }

        private TextMesh FindTextMesh(string objectName)
        {
            var target = FindChildTransform(objectName);
            return target != null ? target.GetComponent<TextMesh>() : null;
        }

        private Transform FindChildTransform(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return null;
            }

            var children = GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == objectName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private void SetResourceFillSegments(float currentHealth, float currentShield, float maxHealth)
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
        private const float DefaultDurationSeconds = 0.9f;
        private const float DefaultRiseDistance = 1f;

        [SerializeField] private TextMesh damageText;
        [SerializeField] private float durationSeconds = DefaultDurationSeconds;
        [SerializeField] private float riseDistance = DefaultRiseDistance;

        private Vector3 startLocalPosition;
        private Color startColor = Color.white;
        private float remainingSeconds;
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
            Hide();
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

            remainingSeconds = Mathf.Max(0.01f, durationSeconds);
            transform.localPosition = startLocalPosition;
            damageText.text = $"{Mathf.RoundToInt(Mathf.Max(0f, damageAmount))}(Damage)";
            SetAlpha(1f);
            gameObject.SetActive(true);
            enabled = true;
        }

        private void Update()
        {
            if (damageText == null)
            {
                enabled = false;
                return;
            }

            remainingSeconds -= Time.deltaTime;
            var duration = Mathf.Max(0.01f, durationSeconds);
            var normalized = Mathf.Clamp01(1f - (remainingSeconds / duration));
            var position = startLocalPosition;
            position.y += riseDistance * normalized;
            transform.localPosition = position;
            SetAlpha(1f - normalized);

            if (remainingSeconds <= 0f)
            {
                Hide();
            }
        }

        private void Hide()
        {
            if (damageText != null)
            {
                damageText.text = string.Empty;
                SetAlpha(0f);
            }

            gameObject.SetActive(false);
            enabled = false;
        }

        private void SetAlpha(float alpha)
        {
            var color = startColor;
            color.a = Mathf.Clamp01(alpha);
            damageText.color = color;
        }
    }
}
