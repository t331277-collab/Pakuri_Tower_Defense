using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

/*
 * Enemy와 Monster Actor가 공통으로 쓰는 이름, 체력, 보호막, 피해 숫자 표시를 관리한다.
 * 프리팹의 정해진 자식 이름을 한 번 찾아 보관해 두 Actor의 중복 탐색 코드를 없앤다.
 */
namespace Pakuri.InGame
{
    internal class UnitWorldDisplay
    {
        private const string NameLabelObjectName = "MonsterNameLabel";
        private const string HpLabelObjectName = "MonsterHpLabel";
        private const string HpBackgroundObjectName = "Background";
        private const string HpFillObjectName = "Fill";
        private const string ShieldFillObjectName = "Shield";
        private const string DamageTextObjectName = "Damage";

        private readonly TextMesh nameLabel;
        private readonly TextMesh hpLabel;
        private readonly Transform hpBackground;
        private readonly Transform hpFill;
        private readonly Transform shieldFill;
        private readonly DamageNumberPopup damagePopup;

        public UnitWorldDisplay(Component owner)
        {
            nameLabel = FindTextMesh(owner, NameLabelObjectName);
            hpLabel = FindTextMesh(owner, HpLabelObjectName);
            hpBackground = FindChild(owner, HpBackgroundObjectName);
            hpFill = FindChild(owner, HpFillObjectName);
            shieldFill = FindChild(owner, ShieldFillObjectName);

            var damageLabel = FindTextMesh(owner, DamageTextObjectName);
            if (damageLabel != null)
            {
                damagePopup = damageLabel.GetComponent<DamageNumberPopup>();
                if (damagePopup == null)
                {
                    damagePopup = damageLabel.gameObject.AddComponent<DamageNumberPopup>();
                }

                damagePopup.Initialize(damageLabel);
            }
        }

        /*
         * 현재 모델의 이름, 상태 효과, 체력과 보호막을 월드 표시에 반영한다.
         */
        public void Refresh(UnitCombatState model)
        {
            var displayName = model.Identity.DisplayName;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = model.Identity.DefinitionId;
            }

            displayName = AppendStatusDisplay(displayName, model.Statuses.ActiveStatuses);
            var maxHealth = Mathf.Max(0f, model.Stats.MaxHealth);
            var currentHealth = Mathf.Clamp(model.Resources.CurrentHealth, 0f, maxHealth);
            var currentShield = Mathf.Max(0f, model.Resources.CurrentShield);

            if (nameLabel != null)
            {
                nameLabel.text = displayName;
            }

            if (hpLabel != null)
            {
                if (currentShield > 0f)
                {
                    hpLabel.text = $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)} +{FormatValue(currentShield)}";
                }
                else
                {
                    hpLabel.text = $"HP {FormatValue(currentHealth)}/{FormatValue(maxHealth)}";
                }
            }

            SetResourceFillSegments(currentHealth, currentShield, maxHealth);
            if (shieldFill != null)
            {
                shieldFill.gameObject.SetActive(currentShield > 0f);
            }
        }

        public void ShowDamage(float damageAmount)
        {
            if (damagePopup != null)
            {
                damagePopup.Show(damageAmount);
            }
        }

        private void SetResourceFillSegments(float currentHealth, float currentShield, float maxHealth)
        {
            var totalVisibleResource = Mathf.Max(maxHealth, currentHealth + currentShield);
            var safeTotal = Mathf.Max(1f, totalVisibleResource);
            var healthRatio = Mathf.Clamp01(currentHealth / safeTotal);
            var shieldRatio = Mathf.Clamp01(currentShield / safeTotal);

            SetSegmentScaleAndPosition(hpFill, 0f, healthRatio);
            SetSegmentScaleAndPosition(shieldFill, healthRatio, shieldRatio);
        }

        private void SetSegmentScaleAndPosition(Transform target, float leftRatio, float widthRatio)
        {
            if (target == null)
            {
                return;
            }

            var baseScaleX = hpBackground != null ? hpBackground.localScale.x : target.localScale.x;
            var backgroundCenterX = hpBackground != null ? hpBackground.localPosition.x : 0f;
            var backgroundWidth = ResolveLocalRenderedWidth(hpBackground, Mathf.Abs(baseScaleX));
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

        private static Transform FindChild(Component owner, string objectName)
        {
            var children = owner.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < children.Length; i++)
            {
                if (children[i].name == objectName)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static TextMesh FindTextMesh(Component owner, string objectName)
        {
            var target = FindChild(owner, objectName);
            if (target != null)
            {
                return target.GetComponent<TextMesh>();
            }

            return null;
        }

        private static float ResolveLocalRenderedWidth(Transform target, float defaultWidth)
        {
            if (target == null)
            {
                return Mathf.Max(0f, defaultWidth);
            }

            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null || spriteRenderer.sprite == null)
            {
                return Mathf.Abs(target.localScale.x);
            }

            return Mathf.Abs(target.localScale.x) * Mathf.Max(0.0001f, spriteRenderer.sprite.bounds.size.x);
        }

        private static float ResolveScaleXForRenderedWidth(Transform target, float renderedWidth, float defaultSignSource)
        {
            var spriteRenderer = target.GetComponent<SpriteRenderer>();
            var unitWidth = 1f;
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitWidth = Mathf.Max(0.0001f, spriteRenderer.sprite.bounds.size.x);
            }

            var signSource = target.localScale.x;
            if (Mathf.Approximately(signSource, 0f))
            {
                signSource = defaultSignSource;
            }

            var sign = signSource < 0f ? -1f : 1f;
            return sign * Mathf.Max(0f, renderedWidth) / unitWidth;
        }

        private static string FormatValue(float value)
        {
            if (Mathf.Approximately(value, Mathf.Round(value)))
            {
                return Mathf.RoundToInt(value).ToString();
            }

            return value.ToString("0.##");
        }

        private static string AppendStatusDisplay(string displayName, IReadOnlyList<StatusRuntimeInstance> statuses)
        {
            var suffix = BuildStatusDisplaySuffix(statuses);
            if (string.IsNullOrWhiteSpace(suffix))
            {
                return displayName;
            }

            return $"{displayName}{suffix}";
        }

        /*
         * 같은 상태 종류의 중첩을 합산해 이름표 뒤에 붙일 문자열을 만든다.
         */
        private static string BuildStatusDisplaySuffix(IReadOnlyList<StatusRuntimeInstance> statuses)
        {
            if (statuses == null || statuses.Count == 0)
            {
                return string.Empty;
            }

            var totals = new Dictionary<StatusEffectKind, int>();
            var labels = new Dictionary<StatusEffectKind, string>();
            for (var i = 0; i < statuses.Count; i++)
            {
                var status = statuses[i];
                if (status == null || status.Kind == StatusEffectKind.None || status.Stacks <= 0)
                {
                    continue;
                }

                var statusName = status.DisplayName;
                if (string.IsNullOrWhiteSpace(statusName))
                {
                    statusName = StatusEffectLookup.ToDisplayName(status.Kind);
                }

                if (totals.ContainsKey(status.Kind))
                {
                    totals[status.Kind] += status.Stacks;
                }
                else
                {
                    totals.Add(status.Kind, status.Stacks);
                }

                labels[status.Kind] = statusName;
            }

            var names = new List<string>();
            foreach (var pair in totals)
            {
                names.Add($"{labels[pair.Key]} +{pair.Value}");
            }

            if (names.Count == 0)
            {
                return string.Empty;
            }

            return $"[{string.Join("/", names)}]";
        }
    }
}
