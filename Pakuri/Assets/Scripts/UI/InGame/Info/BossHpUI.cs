/*
 * 역할: 선택 보스 화면 HP 표시.
 * 책임: 최고 최대 체력 보스 하나의 월드 HP 표시와 Canvas BossHP를 동기화한다.
 */

using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Pakuri.InGame
{
    /// 살아 있는 보스 중 최대 체력이 가장 높은 유닛의 화면 HP를 표시한다.
    public sealed class BossHpUI : MonoBehaviour
    {
        private GameObject root;
        private TMP_Text nameText;
        private TMP_Text hpText;
        private RectTransform background;
        private RectTransform fill;
        private RectTransform shield;
        private UnitSpawnManager unitSpawnManager;

        private CombatUnitEntry displayedBoss;
        private bool referencesBound;
        private bool bindingFailed;

        private void Awake()
        {
            BindObject();
            Hide();
        }

        public void Refresh()
        {
            if (!BindObject())
            {
                UiObjectUtility.SetActive(root, false);
                return;
            }

            var nextBoss = SelectBoss(unitSpawnManager != null ? unitSpawnManager.Enemies : null);
            if (nextBoss != displayedBoss)
            {
                if (displayedBoss != null
                    && displayedBoss.Model != null
                    && displayedBoss.Model.Resources != null
                    && displayedBoss.IsAlive)
                {
                    SetWorldHpBarVisible(displayedBoss, true);
                }

                displayedBoss = nextBoss;
                if (displayedBoss != null)
                {
                    SetWorldHpBarVisible(displayedBoss, false);
                }
            }

            if (displayedBoss == null
                || displayedBoss.Model == null
                || displayedBoss.Model.Stats == null
                || displayedBoss.Model.Resources == null)
            {
                if (displayedBoss != null)
                {
                    Debug.LogError(
                        "BossHpUI detected a displayed boss entry with an invalid combat model and cleared it.",
                        this);
                }

                ClearDisplayedBoss();
                return;
            }

            UiObjectUtility.SetActive(root, true);
            RefreshValues(displayedBoss.Model);
        }

        public void Hide()
        {
            ClearDisplayedBoss();
        }

        /// BossHP 하위 표시 요소와 현재 씬의 UnitSpawnManager를 런타임 참조로 연결한다.
        private bool BindObject()
        {
            if (referencesBound)
            {
                return true;
            }

            if (bindingFailed)
            {
                return false;
            }

            root = gameObject;
            var valid = true;
            nameText = UiBindingUtility.BindChild<TMP_Text>(this, "Name", nameof(nameText), ref valid);
            hpText = UiBindingUtility.BindChild<TMP_Text>(this, "HPText", nameof(hpText), ref valid);
            background = UiBindingUtility.BindChild<RectTransform>(this, "BackGround", nameof(background), ref valid);
            fill = UiBindingUtility.BindChild<RectTransform>(this, "Fill", nameof(fill), ref valid);
            shield = UiBindingUtility.BindChild<RectTransform>(this, "Shield", nameof(shield), ref valid);
            unitSpawnManager = UiBindingUtility.BindSceneComponent<UnitSpawnManager>(this, nameof(unitSpawnManager), ref valid);

            referencesBound = valid;
            bindingFailed = !valid;
            return valid;
        }

        /// 현재 보스 참조를 정리하고 월드 HP바와 Canvas BossHP를 숨긴다.
        private void ClearDisplayedBoss()
        {
            if (displayedBoss != null)
            {
                SetWorldHpBarVisible(displayedBoss, true);
            }

            displayedBoss = null;
            UiObjectUtility.SetActive(root, false);
        }

        private void RefreshValues(UnitCombatState model)
        {
            if (model == null || model.Stats == null || model.Resources == null)
            {
                return;
            }

            var displayName = model.Identity != null ? model.Identity.DisplayName : null;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = model.Identity != null ? model.Identity.DefinitionName : string.Empty;
            }

            var maxHealth = Mathf.Max(0f, model.Stats.MaxHealth);
            var currentHealth = Mathf.Clamp(model.Resources.CurrentHealth, 0f, maxHealth);
            var currentShield = Mathf.Max(0f, model.GetTotalShield());
            var totalMaxResource = maxHealth + currentShield;
            var totalCurrentResource = currentHealth + currentShield;

            if (nameText != null)
            {
                nameText.text = displayName;
            }

            if (hpText != null)
            {
                hpText.text = $"{FormatValue(totalCurrentResource)} / {FormatValue(totalMaxResource)}";
            }

            var safeTotal = Mathf.Max(1f, totalMaxResource);
            var healthRatio = Mathf.Clamp01(currentHealth / safeTotal);
            var shieldRatio = Mathf.Clamp01(currentShield / safeTotal);
            SetSegment(fill, 0f, healthRatio);
            SetSegment(shield, healthRatio, shieldRatio);
            UiObjectUtility.SetActive(shield != null ? shield.gameObject : null, currentShield > 0f);
        }

        private void SetSegment(RectTransform target, float leftRatio, float widthRatio)
        {
            if (target == null || background == null)
            {
                return;
            }

            var backgroundWidth = Mathf.Abs(background.rect.width);
            if (backgroundWidth <= 0f)
            {
                backgroundWidth = Mathf.Abs(background.sizeDelta.x);
            }

            var segmentWidth = backgroundWidth * Mathf.Clamp01(widthRatio);
            var size = target.sizeDelta;
            size.x = segmentWidth;
            target.sizeDelta = size;

            var position = target.anchoredPosition;
            position.x = background.anchoredPosition.x
                - backgroundWidth * 0.5f
                + backgroundWidth * Mathf.Clamp01(leftRatio)
                + segmentWidth * 0.5f;
            target.anchoredPosition = position;
        }

        private static CombatUnitEntry SelectBoss(IReadOnlyList<CombatUnitEntry> enemies)
        {
            CombatUnitEntry highest = null;
            if (enemies == null)
            {
                return null;
            }

            for (var i = 0; i < enemies.Count; i++)
            {
                var entry = enemies[i];
                var model = entry != null ? entry.Model : null;
                if (model == null || model.Stats == null || model.Resources == null || !entry.IsAlive || !model.IsBoss)
                {
                    continue;
                }

                if (highest == null || model.Stats.MaxHealth > highest.Model.Stats.MaxHealth)
                {
                    highest = entry;
                }
            }

            return highest;
        }

        private static void SetWorldHpBarVisible(CombatUnitEntry entry, bool visible)
        {
            if (entry != null && entry.Actor is EnemyActor enemy)
            {
                enemy.SetWorldHpBarVisible(visible);
            }
        }

        private static string FormatValue(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value))
                ? Mathf.RoundToInt(value).ToString()
                : value.ToString("0.##");
        }

    }
}
