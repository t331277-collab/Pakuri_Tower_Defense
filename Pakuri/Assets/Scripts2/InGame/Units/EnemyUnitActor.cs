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

        [SerializeField] private TextMesh enemyNameLabel;
        [SerializeField] private TextMesh enemyHpLabel;
        [SerializeField] private Transform hpBackground;
        [SerializeField] private Transform hpFill;
        [SerializeField] private Transform shieldFill;

        public EnemyUnitRuntimeModel Model { get; private set; }

        public void Initialize(EnemyUnitRuntimeModel model)
        {
            Model = model;
            ResolveDebugViewReferences();
            RefreshDebugView();
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
            var backgroundWidth = Mathf.Abs(baseScaleX);
            var segmentWidth = backgroundWidth * Mathf.Clamp01(widthRatio);
            var scale = target.localScale;
            scale.x = segmentWidth;
            target.localScale = scale;

            var position = target.localPosition;
            position.x = backgroundCenterX - (backgroundWidth * 0.5f)
                + (backgroundWidth * Mathf.Clamp01(leftRatio))
                + (segmentWidth * 0.5f);
            target.localPosition = position;
        }

        private static string FormatValue(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value))
                ? Mathf.RoundToInt(value).ToString()
                : value.ToString("0.##");
        }
    }
}
