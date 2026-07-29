/*
 * 역할: 공통 유닛 월드 표시.
 * 책임: 이름·체력·보호막 Bar·상태 Text·피해 Popup·자원 Segment를 갱신한다.
 */

using System.Collections.Generic;
using Pakuri.Data;
using UnityEngine;

namespace Pakuri.InGame
{

    /// <summary><c>UnitWorldDisplay</c> 상태를 Unity UI 또는 월드 오브젝트로 표시한다.</summary>
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

        /// <summary><c>UnitWorldDisplay</c> 인스턴스를 전달된 런타임 입력값으로 초기화한다.</summary>
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

        /// <summary>전달된 <c>model</c> 값을 사용해 <c>현재 표시 상태</c>를 현재 런타임 모델을 기준으로 갱신한다.</summary>
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

        /// <summary>전달된 <c>damageAmount</c> 값을 사용해 <c>Damage</c>를 표시한다.</summary>
        public void ShowDamage(float damageAmount)
        {
            if (damagePopup != null)
            {
                damagePopup.Show(damageAmount);
            }
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>ResourceFillSegments</c>를 갱신한다.</summary>
        private void SetResourceFillSegments(float currentHealth, float currentShield, float maxHealth)
        {
            var totalVisibleResource = Mathf.Max(maxHealth, currentHealth + currentShield);
            var safeTotal = Mathf.Max(1f, totalVisibleResource);
            var healthRatio = Mathf.Clamp01(currentHealth / safeTotal);
            var shieldRatio = Mathf.Clamp01(currentShield / safeTotal);

            SetSegmentScaleAndPosition(hpFill, 0f, healthRatio);
            SetSegmentScaleAndPosition(shieldFill, healthRatio, shieldRatio);
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>SegmentScaleAndPosition</c>를 갱신한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>Child</c>를 찾는다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>TextMesh</c>를 찾는다.</summary>
        private static TextMesh FindTextMesh(Component owner, string objectName)
        {
            var target = FindChild(owner, objectName);
            if (target != null)
            {
                return target.GetComponent<TextMesh>();
            }

            return null;
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>LocalRenderedWidth</c>를 결정한다.</summary>
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

        /// <summary>전달된 런타임 입력값을 사용해 <c>ScaleXForRenderedWidth</c>를 결정한다.</summary>
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

        /// <summary>전달된 <c>value</c> 값을 사용해 <c>Value</c>를 표시 또는 직렬화 형식으로 변환한다.</summary>
        private static string FormatValue(float value)
        {
            if (Mathf.Approximately(value, Mathf.Round(value)))
            {
                return Mathf.RoundToInt(value).ToString();
            }

            return value.ToString("0.##");
        }

        /// <summary>전달된 런타임 입력값을 사용해 <c>StatusDisplay</c>를 누적 결과에 추가한다.</summary>
        private static string AppendStatusDisplay(string displayName, IReadOnlyList<StatusRuntimeInstance> statuses)
        {
            var suffix = BuildStatusDisplaySuffix(statuses);
            if (string.IsNullOrWhiteSpace(suffix))
            {
                return displayName;
            }

            return $"{displayName}{suffix}";
        }

        /// <summary>전달된 <c>statuses</c> 값을 사용해 <c>StatusDisplaySuffix</c>를 구성한다.</summary>
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
