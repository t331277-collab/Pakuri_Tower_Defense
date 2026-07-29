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

        /*
         * UnitWorldDisplay에 필요한 값을 초기화한다.
         */
        public UnitWorldDisplay(Component owner /* 정보를 소유한 유닛 */)
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
        public void Refresh(UnitCombatState model /* 전투 상태를 읽거나 변경할 유닛 */)
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

        /*
         * ShowDamage 작업을 수행한다.
         */
        public void ShowDamage(float damageAmount /* 표시하거나 적용할 피해량 */)
        {
            if (damagePopup != null)
            {
                damagePopup.Show(damageAmount);
            }
        }

        /*
         * SetResourceFillSegments에 필요한 값을 설정한다.
         */
        private void SetResourceFillSegments(float currentHealth /* 현재 체력 */, float currentShield /* 현재 보호막 */, float maxHealth /* 최대 체력 */)
        {
            var totalVisibleResource = Mathf.Max(maxHealth, currentHealth + currentShield);
            var safeTotal = Mathf.Max(1f, totalVisibleResource);
            var healthRatio = Mathf.Clamp01(currentHealth / safeTotal);
            var shieldRatio = Mathf.Clamp01(currentShield / safeTotal);

            SetSegmentScaleAndPosition(hpFill, 0f, healthRatio);
            SetSegmentScaleAndPosition(shieldFill, healthRatio, shieldRatio);
        }

        /*
         * SetSegmentScaleAndPosition에 필요한 값을 설정한다.
         */
        private void SetSegmentScaleAndPosition(Transform target /* 효과가 따라갈 위치 정보 */, float leftRatio /* 왼쪽 비율 */, float widthRatio /* 너비 비율 */)
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

        /*
         * FindChild에 해당하는 값을 찾아 반환한다.
         */
        private static Transform FindChild(Component owner /* 정보를 소유한 유닛 */, string objectName /* 게임 오브젝트 이름 */)
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

        /*
         * FindTextMesh에 해당하는 값을 찾아 반환한다.
         */
        private static TextMesh FindTextMesh(Component owner /* 정보를 소유한 유닛 */, string objectName /* 게임 오브젝트 이름 */)
        {
            var target = FindChild(owner, objectName);
            if (target != null)
            {
                return target.GetComponent<TextMesh>();
            }

            return null;
        }

        /*
         * ResolveLocalRenderedWidth 결과를 계산해 반환한다.
         */
        private static float ResolveLocalRenderedWidth(Transform target /* 효과가 따라갈 위치 정보 */, float defaultWidth /* 기본 너비 */)
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

        /*
         * ResolveScaleXForRenderedWidth 결과를 계산해 반환한다.
         */
        private static float ResolveScaleXForRenderedWidth(Transform target /* 효과가 따라갈 위치 정보 */, float renderedWidth /* 화면 표시 너비 */, float defaultSignSource /* 기본 부호 발생 원본 */)
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

        /*
         * FormatValue에 맞는 문자열을 만들어 반환한다.
         */
        private static string FormatValue(float value /* 처리할 값 */)
        {
            if (Mathf.Approximately(value, Mathf.Round(value)))
            {
                return Mathf.RoundToInt(value).ToString();
            }

            return value.ToString("0.##");
        }

        /*
         * AppendStatusDisplay 작업 결과를 반환한다.
         */
        private static string AppendStatusDisplay(string displayName /* 표시 이름 */, IReadOnlyList<StatusRuntimeInstance> statuses /* 상태 효과 목록 */)
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
        private static string BuildStatusDisplaySuffix(IReadOnlyList<StatusRuntimeInstance> statuses /* 상태 효과 목록 */)
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
