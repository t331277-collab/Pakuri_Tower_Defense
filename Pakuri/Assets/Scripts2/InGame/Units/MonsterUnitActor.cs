using UnityEngine;

namespace Pakuri.InGame
{
    [DisallowMultipleComponent]
    public sealed class MonsterUnitActor : MonoBehaviour
    {
        private const string NameLabelObjectName = "MonsterNameLabel";
        private const string HpLabelObjectName = "MonsterHpLabel";
        private const string HpBackgroundObjectName = "Background";
        private const string HpFillObjectName = "Fill";
        private const string ShieldFillObjectName = "Shield";

        [SerializeField] private TextMesh monsterNameLabel;
        [SerializeField] private TextMesh monsterHpLabel;
        [SerializeField] private Transform hpBackground;
        [SerializeField] private Transform hpFill;
        [SerializeField] private Transform shieldFill;

        public MonsterUnitRuntimeModel Model { get; private set; }

        public void Initialize(MonsterUnitRuntimeModel model)
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

            if (monsterNameLabel != null)
            {
                monsterNameLabel.text = displayName;
            }

            if (monsterHpLabel != null)
            {
                monsterHpLabel.text = currentShield > 0f
                    ? $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)} +{FormatValue(currentShield)}"
                    : $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)}";
            }

            SetFillScale(hpFill, hpBackground, maxHealth > 0f ? currentHealth / maxHealth : 0f);
            SetFillScale(shieldFill, hpBackground, maxHealth > 0f ? Mathf.Min(currentShield / maxHealth, 1f) : 0f);
            if (shieldFill != null)
            {
                shieldFill.gameObject.SetActive(currentShield > 0f);
            }
        }

        private void ResolveDebugViewReferences()
        {
            if (monsterNameLabel == null)
            {
                monsterNameLabel = FindTextMesh(NameLabelObjectName);
            }

            if (monsterHpLabel == null)
            {
                monsterHpLabel = FindTextMesh(HpLabelObjectName);
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

        private static void SetFillScale(Transform target, Transform background, float normalizedValue)
        {
            if (target == null)
            {
                return;
            }

            var baseScaleX = background != null ? background.localScale.x : target.localScale.x;
            var scale = target.localScale;
            scale.x = baseScaleX * Mathf.Clamp01(normalizedValue);
            target.localScale = scale;
        }

        private static string FormatValue(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value))
                ? Mathf.RoundToInt(value).ToString()
                : value.ToString("0.##");
        }
    }
}
